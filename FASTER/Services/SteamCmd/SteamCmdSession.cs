using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Channels;

namespace FASTER.Services.SteamCmd;

internal sealed class SteamCmdSession : IAsyncDisposable
{
    private static readonly TimeSpan ShutdownGracePeriod = TimeSpan.FromSeconds(7);

    private readonly SteamCmdPseudoConsole _console;
    private readonly Process _process;
    private readonly StreamWriter _input;
    private readonly StreamReader _output;
    private readonly SteamCmdOutputParser _outputParser;
    private readonly Channel<SteamCmdOutputEvent> _events;
    private readonly SemaphoreSlim _inputGate = new(1, 1);
    private readonly IProgress<SteamCmdProgress>? _progress;
    private readonly Task _outputPump;
    private int _stopping;
    private bool _disposed;

    private SteamCmdSession(
        SteamCmdPseudoConsole console,
        IEnumerable<string> secrets,
        IProgress<SteamCmdProgress>? progress)
    {
        _console = console;
        _process = console.Process;
        _input = new StreamWriter(console.Input, new UTF8Encoding(false), 4096, leaveOpen: true)
        {
            NewLine = "\r\n"
        };
        _input.AutoFlush = true;
        _output = new StreamReader(console.Output, new UTF8Encoding(false), true, 4096, leaveOpen: true);
        _progress = progress;
        _outputParser = new SteamCmdOutputParser(secrets);
        _events = Channel.CreateUnbounded<SteamCmdOutputEvent>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false,
            AllowSynchronousContinuations = false
        });

        _outputPump = Task.Factory.StartNew(
            () => PumpOutput(_output, _outputParser),
            CancellationToken.None,
            TaskCreationOptions.LongRunning,
            TaskScheduler.Default);
    }

    public bool HasExited
    {
        get
        {
            try
            {
                return _process.HasExited;
            }
            catch (InvalidOperationException)
            {
                return true;
            }
        }
    }

    public int? ExitCode => HasExited ? _process.ExitCode : null;

    public static SteamCmdSession Start(
        string executablePath,
        string workingDirectory,
        IEnumerable<string> secrets,
        IProgress<SteamCmdProgress>? progress)
    {
        SteamCmdPseudoConsole? console = null;
        try
        {
            console = SteamCmdPseudoConsole.Start(executablePath, workingDirectory);
            SteamCmdSession session = new(console, secrets, progress);
            console = null;
            return session;
        }
        catch
        {
            console?.Dispose();
            throw;
        }
    }

    public async Task SendCommandAsync(string command, CancellationToken cancellationToken)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (string.IsNullOrWhiteSpace(command) || command.IndexOfAny(['\r', '\n', '\0']) >= 0)
            throw new ArgumentException("A SteamCMD command must be one non-empty line.", nameof(command));

        await WriteLineAsync(command, cancellationToken).ConfigureAwait(false);
    }

    public async Task SendSecretAsync(string secret, CancellationToken cancellationToken)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (secret.IndexOfAny(['\r', '\n', '\0']) >= 0)
            throw new ArgumentException("A Steam credential response must be one line.", nameof(secret));

        _outputParser.AddSecret(secret);
        await WriteLineAsync(secret, cancellationToken).ConfigureAwait(false);
    }

    public async Task<SteamCmdOutputEvent> NextEventAsync(
        TimeSpan timeout,
        CancellationToken cancellationToken)
    {
        using CancellationTokenSource timeoutSource = new(timeout);
        using CancellationTokenSource linkedSource = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken,
            timeoutSource.Token);
        try
        {
            return await _events.Reader.ReadAsync(linkedSource.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested && timeoutSource.IsCancellationRequested)
        {
            throw new SteamCmdException("SteamCMD did not respond before the operation timed out.");
        }
        catch (ChannelClosedException exception)
        {
            string exitDescription = ExitCode is int exitCode ? $" (exit code {exitCode})" : string.Empty;
            throw new SteamCmdException($"SteamCMD exited before completing the command{exitDescription}.", exception);
        }
    }

    public async Task<int?> QuitAndWaitAsync(CancellationToken cancellationToken)
    {
        if (!HasExited)
        {
            try
            {
                await SendCommandAsync(SteamCmdCommandBuilder.BuildQuitCommand(), cancellationToken).ConfigureAwait(false);
            }
            catch (SteamCmdException)
            {
                // The process may exit between HasExited and writing the quit command.
            }
            catch (IOException)
            {
            }
            catch (InvalidOperationException)
            {
            }
        }

        await WaitForExitOrKillAsync(cancellationToken).ConfigureAwait(false);
        return ExitCode;
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        if (Interlocked.Exchange(ref _stopping, 1) != 0)
        {
            if (!HasExited)
                await _process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);
            return;
        }

        if (!HasExited)
        {
            try
            {
                await SendCommandAsync(SteamCmdCommandBuilder.BuildQuitCommand(), CancellationToken.None).ConfigureAwait(false);
            }
            catch (Exception exception) when (exception is SteamCmdException or IOException or InvalidOperationException or ObjectDisposedException)
            {
                // The process may exit between HasExited and writing the quit command.
            }
        }

        await WaitForExitOrKillAsync(cancellationToken).ConfigureAwait(false);
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
            return;

        await StopAsync(CancellationToken.None).ConfigureAwait(false);
        _disposed = true;
        _input.Dispose();
        _console.Close();
        await ObservePumpAsync(_outputPump).ConfigureAwait(false);
        _output.Dispose();
        _inputGate.Dispose();
        _console.Dispose();
    }

    private async Task WriteLineAsync(string value, CancellationToken cancellationToken)
    {
        await _inputGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (HasExited)
                throw new SteamCmdException("SteamCMD exited before input could be sent.");
            await _input.WriteLineAsync(value.AsMemory(), cancellationToken).ConfigureAwait(false);
            await _input.FlushAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            _inputGate.Release();
        }
    }

    private void PumpOutput(StreamReader reader, SteamCmdOutputParser parser)
    {
        char[] buffer = new char[1024];
        try
        {
            while (true)
            {
                int read = reader.Read(buffer, 0, buffer.Length);
                if (read == 0)
                    break;
                Publish(parser.Feed(new string(buffer, 0, read)));
            }

            Publish(parser.Complete());
        }
        catch (Exception exception) when (exception is IOException or ObjectDisposedException)
        {
        }
        finally
        {
            _events.Writer.TryComplete();
        }
    }

    private void Publish(IReadOnlyList<SteamCmdOutputEvent> parsedEvents)
    {
        foreach (SteamCmdOutputEvent parsedEvent in parsedEvents)
        {
            _events.Writer.TryWrite(parsedEvent);
            if (parsedEvent.Kind == SteamCmdOutputEventKind.Output && !string.IsNullOrWhiteSpace(parsedEvent.Text))
            {
                _progress?.Report(new SteamCmdProgress(
                    SteamCmdProgressKind.Output,
                    parsedEvent.Text));
            }
        }
    }

    private async Task WaitForExitOrKillAsync(CancellationToken cancellationToken)
    {
        if (!HasExited)
        {
            Task exitTask = _process.WaitForExitAsync(CancellationToken.None);
            Task graceTask = Task.Delay(ShutdownGracePeriod, CancellationToken.None);
            if (await Task.WhenAny(exitTask, graceTask).ConfigureAwait(false) != exitTask)
                TryKillProcessTree();
        }

        if (!HasExited)
            await _process.WaitForExitAsync(CancellationToken.None).ConfigureAwait(false);

        cancellationToken.ThrowIfCancellationRequested();
    }

    private void TryKillProcessTree()
    {
        try
        {
            if (!HasExited)
                _process.Kill(true);
        }
        catch (InvalidOperationException)
        {
        }
    }

    private static async Task ObservePumpAsync(Task pump)
    {
        try
        {
            await pump.ConfigureAwait(false);
        }
        catch (Exception exception) when (exception is IOException or ObjectDisposedException)
        {
        }
    }
}
