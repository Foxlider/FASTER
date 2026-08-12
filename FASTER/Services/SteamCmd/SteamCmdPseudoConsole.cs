using Microsoft.Win32.SafeHandles;

using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace FASTER.Services.SteamCmd;

/// <summary>
/// Hosts SteamCMD in the native Windows pseudoconsole (ConPTY). SteamCMD does
/// not flush its interactive prompts or consume redirected stdin reliably when
/// it is launched with ordinary anonymous pipes.
/// </summary>
internal sealed class SteamCmdPseudoConsole : IDisposable
{
    private const uint ExtendedStartupInfoPresent = 0x00080000;
    private const int StartfUseStdHandles = 0x00000100;
    private static readonly IntPtr PseudoConsoleAttribute = (IntPtr)0x00020016;

    private readonly FileStream _input;
    private readonly FileStream _output;
    private IntPtr _pseudoConsole;
    private int _closed;

    private SteamCmdPseudoConsole(
        Process process,
        FileStream input,
        FileStream output,
        IntPtr pseudoConsole)
    {
        Process = process;
        _input = input;
        _output = output;
        _pseudoConsole = pseudoConsole;
    }

    public Process Process { get; }

    public Stream Input => _input;

    public Stream Output => _output;

    public static SteamCmdPseudoConsole Start(string executablePath, string workingDirectory)
    {
        if (!OperatingSystem.IsWindowsVersionAtLeast(10, 0, 17763))
        {
            throw new PlatformNotSupportedException(
                "SteamCMD interactive sessions require Windows 10 version 1809, Windows Server 2019, or newer.");
        }

        SafeFileHandle? pseudoConsoleInput = null;
        SafeFileHandle? hostInput = null;
        SafeFileHandle? hostOutput = null;
        SafeFileHandle? pseudoConsoleOutput = null;
        FileStream? inputStream = null;
        FileStream? outputStream = null;
        Process? process = null;
        IntPtr pseudoConsole = IntPtr.Zero;
        IntPtr attributeList = IntPtr.Zero;
        ProcessInformation processInformation = default;

        try
        {
            CreatePipeOrThrow(out pseudoConsoleInput, out hostInput);
            CreatePipeOrThrow(out hostOutput, out pseudoConsoleOutput);

            Coord consoleSize = new(160, 40);
            int createResult = CreatePseudoConsole(
                consoleSize,
                pseudoConsoleInput.DangerousGetHandle(),
                pseudoConsoleOutput.DangerousGetHandle(),
                0,
                out pseudoConsole);
            ThrowForHResult(createResult, "Windows could not create a pseudoconsole for SteamCMD.");

            StartupInfoEx startupInfo = new();
            startupInfo.StartupInfo.cb = Marshal.SizeOf<StartupInfoEx>();
            // Explicitly clear inherited standard handles. Without this flag a
            // child launched by a console-hosted parent (including tests and
            // some shells) can keep writing to the parent's console instead of
            // the attached ConPTY.
            startupInfo.StartupInfo.Flags = StartfUseStdHandles;

            IntPtr attributeListSize = IntPtr.Zero;
            _ = InitializeProcThreadAttributeList(IntPtr.Zero, 1, 0, ref attributeListSize);
            if (attributeListSize == IntPtr.Zero)
                throw new Win32Exception(Marshal.GetLastWin32Error(), "Windows could not size the pseudoconsole startup attributes.");

            attributeList = Marshal.AllocHGlobal(attributeListSize);
            if (!InitializeProcThreadAttributeList(attributeList, 1, 0, ref attributeListSize))
                throw new Win32Exception(Marshal.GetLastWin32Error(), "Windows could not initialize the pseudoconsole startup attributes.");

            startupInfo.AttributeList = attributeList;
            if (!UpdateProcThreadAttribute(
                    attributeList,
                    0,
                    PseudoConsoleAttribute,
                    pseudoConsole,
                    (IntPtr)IntPtr.Size,
                    IntPtr.Zero,
                    IntPtr.Zero))
            {
                throw new Win32Exception(Marshal.GetLastWin32Error(), "Windows could not attach SteamCMD to its pseudoconsole.");
            }

            StringBuilder commandLine = new($"\"{executablePath}\"");
            if (!CreateProcessW(
                    executablePath,
                    commandLine,
                    IntPtr.Zero,
                    IntPtr.Zero,
                    false,
                    ExtendedStartupInfoPresent,
                    IntPtr.Zero,
                    workingDirectory,
                    ref startupInfo,
                    out processInformation))
            {
                throw new Win32Exception(Marshal.GetLastWin32Error(), "SteamCMD could not be started in the pseudoconsole.");
            }

            process = Process.GetProcessById(checked((int)processInformation.ProcessId));

            // ConPTY requires synchronous pipe handles. FileStream still exposes
            // task-based reads/writes; its synchronous handles are serviced by
            // the session's independent input and output tasks.
            inputStream = new FileStream(hostInput, FileAccess.Write, 4096, false);
            hostInput = null;
            outputStream = new FileStream(hostOutput, FileAccess.Read, 4096, false);
            hostOutput = null;

            // Once CreateProcess succeeds, our copies of the ConPTY-facing ends
            // must close so broken-pipe/EOF detection works when the session ends.
            pseudoConsoleInput.Dispose();
            pseudoConsoleInput = null;
            pseudoConsoleOutput.Dispose();
            pseudoConsoleOutput = null;

            SteamCmdPseudoConsole result = new(process, inputStream, outputStream, pseudoConsole);
            process = null;
            inputStream = null;
            outputStream = null;
            pseudoConsole = IntPtr.Zero;
            return result;
        }
        catch (EntryPointNotFoundException exception)
        {
            throw new PlatformNotSupportedException(
                "This Windows installation does not provide the ConPTY API required by SteamCMD.",
                exception);
        }
        catch (SteamCmdException)
        {
            throw;
        }
        catch (Exception exception) when (exception is Win32Exception or IOException or ArgumentException)
        {
            throw new SteamCmdException("SteamCMD's interactive console could not be created.", exception);
        }
        finally
        {
            if (attributeList != IntPtr.Zero)
            {
                DeleteProcThreadAttributeList(attributeList);
                Marshal.FreeHGlobal(attributeList);
            }

            CloseNativeHandle(processInformation.ThreadHandle);
            CloseNativeHandle(processInformation.ProcessHandle);

            if (process != null)
            {
                try
                {
                    if (!process.HasExited)
                        process.Kill(true);
                }
                catch (Exception exception) when (exception is InvalidOperationException or Win32Exception)
                {
                }
                process.Dispose();
            }

            inputStream?.Dispose();
            outputStream?.Dispose();
            pseudoConsoleInput?.Dispose();
            hostInput?.Dispose();
            hostOutput?.Dispose();
            pseudoConsoleOutput?.Dispose();

            if (pseudoConsole != IntPtr.Zero)
                ClosePseudoConsole(pseudoConsole);
        }
    }

    /// <summary>
    /// Ends the console session. Closing the output channel first follows the
    /// documented deadlock-safe teardown path on Windows versions before 11 24H2.
    /// </summary>
    public void Close()
    {
        if (Interlocked.Exchange(ref _closed, 1) != 0)
            return;

        _input.Dispose();
        _output.Dispose();
        IntPtr pseudoConsole = Interlocked.Exchange(ref _pseudoConsole, IntPtr.Zero);
        if (pseudoConsole != IntPtr.Zero)
            ClosePseudoConsole(pseudoConsole);
    }

    public void Dispose()
    {
        Close();
        Process.Dispose();
    }

    private static void CreatePipeOrThrow(out SafeFileHandle readPipe, out SafeFileHandle writePipe)
    {
        if (!CreatePipe(out readPipe, out writePipe, IntPtr.Zero, 0))
            throw new Win32Exception(Marshal.GetLastWin32Error(), "Windows could not create a SteamCMD console pipe.");
    }

    private static void ThrowForHResult(int result, string message)
    {
        if (result < 0)
            throw new SteamCmdException(message, Marshal.GetExceptionForHR(result) ?? new InvalidOperationException(message));
    }

    private static void CloseNativeHandle(IntPtr handle)
    {
        if (handle != IntPtr.Zero && handle != new IntPtr(-1))
            _ = CloseHandle(handle);
    }

    [StructLayout(LayoutKind.Sequential)]
    private readonly struct Coord
    {
        public Coord(short x, short y)
        {
            X = x;
            Y = y;
        }

        public readonly short X;
        public readonly short Y;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct StartupInfo
    {
        public int cb;
        public string? Reserved;
        public string? Desktop;
        public string? Title;
        public int X;
        public int Y;
        public int XSize;
        public int YSize;
        public int XCountChars;
        public int YCountChars;
        public int FillAttribute;
        public int Flags;
        public short ShowWindow;
        public short Reserved2Count;
        public IntPtr Reserved2;
        public IntPtr StandardInput;
        public IntPtr StandardOutput;
        public IntPtr StandardError;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct StartupInfoEx
    {
        public StartupInfo StartupInfo;
        public IntPtr AttributeList;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct ProcessInformation
    {
        public IntPtr ProcessHandle;
        public IntPtr ThreadHandle;
        public uint ProcessId;
        public uint ThreadId;
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CreatePipe(
        out SafeFileHandle readPipe,
        out SafeFileHandle writePipe,
        IntPtr pipeAttributes,
        uint size);

    [DllImport("kernel32.dll")]
    private static extern int CreatePseudoConsole(
        Coord size,
        IntPtr input,
        IntPtr output,
        uint flags,
        out IntPtr pseudoConsole);

    [DllImport("kernel32.dll")]
    private static extern void ClosePseudoConsole(IntPtr pseudoConsole);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool InitializeProcThreadAttributeList(
        IntPtr attributeList,
        int attributeCount,
        int flags,
        ref IntPtr size);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UpdateProcThreadAttribute(
        IntPtr attributeList,
        uint flags,
        IntPtr attribute,
        IntPtr value,
        IntPtr size,
        IntPtr previousValue,
        IntPtr returnSize);

    [DllImport("kernel32.dll")]
    private static extern void DeleteProcThreadAttributeList(IntPtr attributeList);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CreateProcessW(
        string? applicationName,
        StringBuilder commandLine,
        IntPtr processAttributes,
        IntPtr threadAttributes,
        [MarshalAs(UnmanagedType.Bool)] bool inheritHandles,
        uint creationFlags,
        IntPtr environment,
        string currentDirectory,
        ref StartupInfoEx startupInfo,
        out ProcessInformation processInformation);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CloseHandle(IntPtr handle);
}
