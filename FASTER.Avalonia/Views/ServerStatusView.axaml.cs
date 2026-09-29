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
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;

using SkiaSharp;

namespace FASTER.Avalonia.Views;

public partial class ServerStatusView : UserControl
{
    private readonly ISystemMetrics _metrics =
        OperatingSystem.IsWindows() ? new WindowsSystemMetrics() : new LinuxSystemMetrics();

    private readonly ObservableValue _cpuValue = new(0);
    private readonly ObservableValue _ramValue = new(0);

    private readonly ObservableCollection<double> _cpuHistory = new();
    private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromMilliseconds(500) };
    private static readonly int[] s_intervalsMs = [100, 250, 500, 1000, 2000, 5000];

    public ServerStatusView()
    {
        InitializeComponent();
        CpuGauge.Series = CreateGauge(_cpuValue);
        RamGauge.Series = CreateGauge(_ramValue);
        CpuChart.Series = new ISeries[]
        {
            new LineSeries<double>
            {
                Values = _cpuHistory,
                Fill = null,
                Stroke = new SolidColorPaint(SKColor.Parse("#119EDA"), 2),
                GeometrySize = 0,
                LineSmoothness = 0
            }
        };
        CpuChart.XAxes = new[] { new Axis { IsVisible = false } };
        CpuChart.YAxes = new[] { new Axis { MinLimit = 0, MaxLimit = 100, LabelsPaint = new SolidColorPaint(SKColors.Gray) } };
        _timer.Tick += (_, _) => Refresh();
        _timer.Start();
        RefreshServers();
    }

    private static ISeries[] CreateGauge(ObservableValue value) =>
    [
        new PieSeries<ObservableValue>
        {
            Values = new[] { value },
            InnerRadius = 60,
            MaxRadialColumnWidth = 18,
            Fill = new SolidColorPaint(SKColor.Parse("#119EDA")),
            Stroke = null
        },
        new PieSeries<ObservableValue>
        {
            Values = new[] { new ObservableValue(100) },
            IsFillSeries = true,
            InnerRadius = 60,
            MaxRadialColumnWidth = 18,
            Fill = new SolidColorPaint(SKColors.Gray.WithAlpha(70)),
            Stroke = null
        }
    ];

    private void Refresh()
    {
        try
        {
            float cpu = _metrics.GetTotalCpuUsage();
            ulong total = _metrics.GetTotalMemoryBytes();
            ulong available = _metrics.GetAvailableMemoryBytes();
            ulong used = total > available ? total - available : 0;

            _cpuValue.Value = Math.Clamp(cpu, 0, 100);
            CpuLabel.Text = $"{cpu:F1} %";
            _ramValue.Value = total > 0 ? (double)used / total * 100 : 0;
            RamPercentLabel.Text = total > 0 ? $"{_ramValue.Value:F1} %" : "Unavailable";
            RamLabel.Text = total > 0 ? $"{used / 1024 / 1024} / {total / 1024 / 1024} MB" : "Unavailable";

            _cpuHistory.Add(cpu);
            while (_cpuHistory.Count > 60)
                _cpuHistory.RemoveAt(0);
        }
        catch
        {
            // Do not present stale measurements as live readings.
            CpuLabel.Text = "Unavailable";
            RamPercentLabel.Text = "Unavailable";
            RamLabel.Text = "Unavailable";
            _cpuValue.Value = 0;
            _ramValue.Value = 0;
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
            { /* Already exited or not killable from here, the rescan below sorts it out. */ }
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
            { /* Same story, already gone or not killable from here. */ }
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
                    { /* Exited mid-scan or not inspectable, so not one of ours. */ return false; }
                })
                .ToList();
        }
        catch
        { /* Process listing failed, keep showing the old list. */ }
    }
}
