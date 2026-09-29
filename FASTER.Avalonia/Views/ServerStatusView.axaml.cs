using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;

using FASTER.Services;

using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;

namespace FASTER.Avalonia.Views;

public partial class ServerStatusView : UserControl
{
    private readonly ISystemMetrics _metrics =
        OperatingSystem.IsWindows() ? new WindowsSystemMetrics() : new LinuxSystemMetrics();

    private readonly ObservableCollection<double> _cpuHistory = new();
    private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromMilliseconds(500) };
    private static readonly int[] s_intervalsMs = [250, 500, 1000, 2000, 5000];

    public ServerStatusView()
    {
        InitializeComponent();
        CpuChart.Series = new ISeries[]
        {
            new LineSeries<double>
            {
                Values = _cpuHistory,
                Fill = null,
                GeometrySize = 0,
                LineSmoothness = 0
            }
        };
        CpuChart.XAxes = new[] { new Axis { IsVisible = false } };
        CpuChart.YAxes = new[] { new Axis { MinLimit = 0, MaxLimit = 100 } };
        _timer.Tick += (_, _) => Refresh();
        _timer.Start();
        RefreshServers();
    }

    private void Refresh()
    {
        try
        {
            float cpu = _metrics.GetTotalCpuUsage();
            ulong total = _metrics.GetTotalMemoryBytes();
            ulong available = _metrics.GetAvailableMemoryBytes();
            ulong used = total > available ? total - available : 0;

            CpuBar.Value = cpu;
            CpuLabel.Text = $"{cpu:F1} %";
            RamBar.Value = total > 0 ? (double)used / total * 100 : 0;
            RamLabel.Text = $"{used / 1024 / 1024} / {total / 1024 / 1024} MB";

            _cpuHistory.Add(cpu);
            while (_cpuHistory.Count > 60)
                _cpuHistory.RemoveAt(0);
        }
        catch
        {
            // Counters can fail on some machines; the view simply keeps old values.
        }
    }

    private void IntervalBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        // Sender is used instead of the named control: this also fires while
        // the view is still loading and the generated field is not set yet.
        if (sender is not ComboBox box)
            return;
        int index = box.SelectedIndex;
        if (index < 0 || index >= s_intervalsMs.Length)
            return;
        _timer.Stop();
        _timer.Interval = TimeSpan.FromMilliseconds(s_intervalsMs[index]);
        _timer.Start();
    }

    private void Rescan_Click(object? sender, RoutedEventArgs e) => RefreshServers();

    private async void KillAll_Click(object? sender, RoutedEventArgs e)
    {
        foreach (var process in Process.GetProcesses().Where(p => p.ProcessName.Contains("arma3server")))
        {
            try
            { process.Kill(); }
            catch
            { }
        }
        await Task.Delay(1000);
        RefreshServers();
    }

    private void KillProcess_Click(object? sender, RoutedEventArgs e)
    {
        if ((sender as Control)?.DataContext is Process process)
        {
            try
            { process.Kill(); }
            catch
            { }
            RefreshServers();
        }
    }

    private void RefreshServers()
    {
        try
        {
            ProcessGrid.ItemsSource = Process.GetProcesses()
                .Where(p =>
                {
                    try
                    { return p.ProcessName.Contains("arma3server"); }
                    catch
                    { return false; }
                })
                .ToList();
        }
        catch
        { }
    }
}
