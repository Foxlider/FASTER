using System.Diagnostics;
using System.Text;

namespace FASTER.Services;

public readonly record struct ProcessIdentity(int Pid, DateTime StartedUtc);
public sealed record ProcessSample(DateTime Time, double CpuPercent, long MemoryBytes);
public sealed record MonitoredProcess(ProcessIdentity Identity, string Name, bool Paused, bool Exited,
    IReadOnlyList<ProcessSample> History, bool OutputAvailable)
{
    public int Id => Identity.Pid;
    public double CpuPercent => History.LastOrDefault()?.CpuPercent ?? 0;
    public long MemoryBytes => History.LastOrDefault()?.MemoryBytes ?? 0;
    public string State => Exited ? "Exited" : Paused ? "Paused" : "Monitoring";
}

public interface IProcessMonitor : IDisposable
{
    ProcessIdentity Launch(ProcessStartInfo start);
    void Rescan();
    IReadOnlyList<MonitoredProcess> Sample();
    void Pause(ProcessIdentity identity, bool paused);
    bool IsPaused { get; set; }
    void Terminate(ProcessIdentity identity);
    string ReadOutput(ProcessIdentity identity);
}

public sealed class ProcessMonitor : IProcessMonitor
{
    private sealed class Entry(Process process, bool captured, string? outputFile = null, string? name = null)
    {
        public Process Process { get; } = process;
        public string Name { get; } = name ?? process.ProcessName;
        public string? OutputFile { get; } = outputFile;
        public bool Captured { get; } = captured;
        public bool Paused;
        public bool Exited;
        public TimeSpan? PreviousCpu;
        public long PreviousTimestamp;
        public Queue<ProcessSample> History { get; } = new();
        public Queue<string> Output { get; } = new();
        public int OutputCharacters;
    }

    private readonly Dictionary<ProcessIdentity, Entry> _entries = new();
    private readonly object _gate = new();
    private readonly int _historyLimit;
    private readonly int _outputLimit;
    private readonly string? _outputDirectory;
    private readonly CancellationTokenSource _lifetime = new();
    private bool _disposed;
    private bool _paused;

    public ProcessMonitor(int historyLimit = 120, int outputLimit = 65536, string? outputDirectory = null)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(historyLimit, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(outputLimit, 1);
        _historyLimit = historyLimit;
        _outputLimit = outputLimit;
        _outputDirectory = outputDirectory;
    }

    public bool IsPaused
    {
        get { lock (_gate) return _paused; }
        set { lock (_gate) { _paused = value; foreach (var entry in _entries.Values) entry.PreviousCpu = null; } }
    }

    public ProcessIdentity Launch(ProcessStartInfo start)
    {
        lock (_gate)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            start.UseShellExecute = false;
            start.RedirectStandardOutput = true;
            start.RedirectStandardError = true;
            string? outputFile = null;
            string name = Path.GetFileNameWithoutExtension(start.FileName);
            if (OperatingSystem.IsLinux())
            {
                // A file remains writable after FASTER exits; a closed capture pipe can stop a server with SIGPIPE.
                var directory = _outputDirectory ?? Path.Combine(Path.GetDirectoryName(FASTER.Models.AppSettings.SettingsPath)!, "ProcessOutput");
                Directory.CreateDirectory(directory);
                outputFile = Path.Combine(directory, Guid.NewGuid().ToString("N") + ".log");
                using (new FileStream(outputFile, new FileStreamOptions { Mode = FileMode.CreateNew, Access = FileAccess.Write,
                    UnixCreateMode = UnixFileMode.UserRead | UnixFileMode.UserWrite }))
                {
                    // Create the owner-only file before the shell opens it for output.
                }
                var shell = new ProcessStartInfo("/bin/sh") { WorkingDirectory = start.WorkingDirectory, UseShellExecute = false };
                foreach (var pair in start.Environment) shell.Environment[pair.Key] = pair.Value;
                shell.Environment["FASTER_CAPTURE_OUTPUT"] = outputFile;
                shell.ArgumentList.Add("-c");
                shell.ArgumentList.Add("exec \"$@\" >> \"$FASTER_CAPTURE_OUTPUT\" 2>&1");
                shell.ArgumentList.Add("faster-capture");
                shell.ArgumentList.Add(start.FileName);
                foreach (var argument in start.ArgumentList.Count > 0 ? start.ArgumentList : SplitArguments(start.Arguments))
                    shell.ArgumentList.Add(argument);
                start = shell;
            }
            var process = new Process { StartInfo = start };
            try
            {
                process.Start();
                var identity = Identify(process);
                var entry = new Entry(process, true, outputFile, name);
                _entries.Add(identity, entry);
                if (outputFile == null)
                {
                process.OutputDataReceived += (_, args) => AppendOutput(entry, args.Data);
                process.ErrorDataReceived += (_, args) => AppendOutput(entry, args.Data);
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                }
                else _ = TailFileAsync(entry, _lifetime.Token);
                return identity;
            }
            catch { process.Dispose(); throw; }
        }
    }

    internal static IReadOnlyList<string> SplitArguments(string commandLine)
    {
        var result = new List<string>();
        var value = new StringBuilder();
        bool quoted = false, started = false;
        for (int i = 0; i < commandLine.Length; i++)
        {
            char c = commandLine[i];
            if (c == '\\')
            {
                int count = 1;
                while (i + 1 < commandLine.Length && commandLine[i + 1] == '\\') { count++; i++; }
                if (i + 1 < commandLine.Length && commandLine[i + 1] == '"')
                {
                    value.Append('\\', count / 2);
                    if (count % 2 != 0) { value.Append('"'); i++; }
                }
                else value.Append('\\', count);
                started = true;
            }
            else if (c == '"') { quoted = !quoted; started = true; }
            else if (char.IsWhiteSpace(c) && !quoted)
            {
                if (started) { result.Add(value.ToString()); value.Clear(); started = false; }
            }
            else { value.Append(c); started = true; }
        }
        if (started) result.Add(value.ToString());
        return result;
    }

    private async Task TailFileAsync(Entry entry, CancellationToken token)
    {
        try
        {
            using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(100));
            do
            {
                using var stream = new FileStream(entry.OutputFile!, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                if (stream.Length > _outputLimit) stream.Seek(-_outputLimit, SeekOrigin.End);
                var bytes = new byte[Math.Min(stream.Length, _outputLimit)];
                int count = await stream.ReadAsync(bytes, token);
                string text = Encoding.UTF8.GetString(bytes, 0, count);
                lock (_gate)
                {
                    if (_disposed) return;
                    entry.Output.Clear();
                    if (text.Length > _outputLimit) text = text[^_outputLimit..];
                    entry.Output.Enqueue(text);
                    entry.OutputCharacters = text.Length;
                    if (entry.Exited || entry.Process.HasExited) { entry.Exited = true; return; }
                }
                if (stream.Length > Math.Max(_outputLimit * 16L, 1048576))
                {
                    using var truncate = new FileStream(entry.OutputFile!, FileMode.Open, FileAccess.Write, FileShare.ReadWrite);
                    truncate.SetLength(0);
                }
            } while (await timer.WaitForNextTickAsync(token));
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested)
        {
            // Disposing the monitor stops capture but leaves the server running.
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        { AppendOutput(entry, "Output capture unavailable: " + ex.GetType().Name); }
    }

    private void AppendOutput(Entry entry, string? line)
    {
        if (line == null) return;
        lock (_gate)
        {
            if (_disposed) return;
            line = line.Length >= _outputLimit ? line[^(_outputLimit - 1)..] : line;
            line += "\n";
            entry.Output.Enqueue(line);
            entry.OutputCharacters += line.Length;
            while (entry.OutputCharacters > _outputLimit && entry.Output.Count > 0)
                entry.OutputCharacters -= entry.Output.Dequeue().Length;
        }
    }

    private static ProcessIdentity Identify(Process process) => new(process.Id, process.StartTime.ToUniversalTime());

    public void Rescan()
    {
        lock (_gate)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            foreach (var process in Process.GetProcesses())
            {
                bool retained = false;
                try
                {
                    if (!process.ProcessName.Contains("arma3server", StringComparison.OrdinalIgnoreCase)) continue;
                    var identity = Identify(process);
                    if (_entries.ContainsKey(identity)) continue;
                    _entries.Add(identity, new Entry(process, false));
                    retained = true;
                }
                catch (Exception ex) when (ex is InvalidOperationException or System.ComponentModel.Win32Exception or NotSupportedException)
                {
                    // Skip processes that exit during the scan or cannot be inspected.
                }
                finally { if (!retained) process.Dispose(); }
            }
            // Keep a bounded set of exited entries so their last output remains accessible.
            foreach (var identity in _entries.Where(p => p.Value.Exited).Select(p => p.Key).OrderByDescending(p => p.StartedUtc).Skip(20).ToArray())
            {
                _entries[identity].Process.Dispose();
                _entries.Remove(identity);
            }
        }
    }

    public IReadOnlyList<MonitoredProcess> Sample()
    {
        lock (_gate)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            foreach (var (identity, entry) in _entries)
            {
                if (entry.Exited) continue;
                try
                {
                    entry.Process.Refresh();
                    if (entry.Process.HasExited || Identify(entry.Process) != identity)
                    { entry.Exited = true; continue; }
                    if (_paused || entry.Paused) { entry.PreviousCpu = null; continue; }
                    long timestamp = Stopwatch.GetTimestamp();
                    var cpu = entry.Process.TotalProcessorTime;
                    double percent = entry.PreviousCpu.HasValue
                        ? CalculateCpu(cpu - entry.PreviousCpu.Value, Stopwatch.GetElapsedTime(entry.PreviousTimestamp, timestamp), Environment.ProcessorCount)
                        : 0;
                    entry.PreviousCpu = cpu;
                    entry.PreviousTimestamp = timestamp;
                    entry.History.Enqueue(new ProcessSample(DateTime.UtcNow, percent, entry.Process.WorkingSet64));
                    while (entry.History.Count > _historyLimit) entry.History.Dequeue();
                }
                catch (Exception ex) when (ex is InvalidOperationException or System.ComponentModel.Win32Exception or NotSupportedException)
                { entry.Exited = true; }
            }
            return _entries.Select(p => new MonitoredProcess(p.Key, p.Value.Name, p.Value.Paused || _paused,
                p.Value.Exited, p.Value.History.ToArray(), p.Value.Captured)).ToArray();
        }
    }

    internal static double CalculateCpu(TimeSpan processorTime, TimeSpan elapsed, int processors) =>
        elapsed.TotalSeconds <= 0 || processors <= 0 ? 0 : Math.Clamp(processorTime.TotalSeconds / elapsed.TotalSeconds / processors * 100, 0, 100);

    public void Pause(ProcessIdentity identity, bool paused)
    {
        lock (_gate) if (_entries.TryGetValue(identity, out var entry))
        { entry.Paused = paused; entry.PreviousCpu = null; }
    }

    public void Terminate(ProcessIdentity identity)
    {
        lock (_gate)
        {
            if (!_entries.TryGetValue(identity, out var entry) || entry.Exited) return;
            entry.Process.Refresh();
            if (!entry.Process.HasExited && Identify(entry.Process) == identity) entry.Process.Kill();
        }
    }

    public string ReadOutput(ProcessIdentity identity)
    {
        lock (_gate) return _entries.TryGetValue(identity, out var entry) && entry.Captured
            ? string.Concat(entry.Output) : "Captured output is unavailable for externally launched processes.";
    }

    public void Dispose()
    {
        lock (_gate)
        {
            if (_disposed) return;
            _disposed = true;
            _lifetime.Cancel();
            _lifetime.Dispose();
            foreach (var entry in _entries.Values) entry.Process.Dispose();
            _entries.Clear();
        }
    }
}
