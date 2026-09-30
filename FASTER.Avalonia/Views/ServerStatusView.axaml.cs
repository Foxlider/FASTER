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

public sealed partial class ServerStatusView : UserControl, IDisposable
{
    private readonly ISystemMetrics? _metrics = CreateMetrics();

    private static ISystemMetrics? CreateMetrics()
    {
        try { return OperatingSystem.IsWindows() ? new WindowsSystemMetrics() : new LinuxSystemMetrics(); }
        catch (Exception ex) { FASTER.Models.Logger.LogCritical("System metrics unavailable: " + ex.Message); return null; }
    }

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
        Loaded += (_, _) => { RefreshServers(); Refresh(); _timer.Start(); };
        Unloaded += (_, _) => _timer.Stop();
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

    private DateTime _nextTemperature;
    private bool _temperaturePending;

    private async Task RefreshTemperatureAsync()
    {
        if (_temperaturePending || DateTime.UtcNow < _nextTemperature) return;
        _temperaturePending = true;
        try
        {
            var temperature = await Task.Run(CpuTemperature.Read);
            TemperatureLabel.Text = temperature.HasValue ? $"CPU temperature: {temperature:F1} °C" : "CPU temperature: Unavailable";
        }
        catch (Exception ex)
        {
            TemperatureLabel.Text = "CPU temperature: Unavailable";
            FASTER.Models.Logger.LogCritical("Temperature reading failed: " + ex.Message);
        }
        finally { _nextTemperature = DateTime.UtcNow.AddSeconds(5); _temperaturePending = false; }
    }

    private void Refresh()
    {
        try
        {
            _ = RefreshTemperatureAsync();
            if (AppServices.Processes.IsPaused) return;
            RefreshProcesses();
            if (_metrics == null) return;
            float cpu = _metrics.GetTotalCpuUsage();
            if (!float.IsFinite(cpu)) throw new InvalidOperationException("CPU reading unavailable");
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
        if (IsLoaded) _timer.Start();
    }

    private void Rescan_Click(object? sender, RoutedEventArgs e) => RefreshServers();

    private async void KillAll_Click(object? sender, RoutedEventArgs e)
    {
        if (!await AppServices.Dialogs.ShowConfirmationAsync(this, "Terminate servers", "Terminate all listed server processes?")) return;
        foreach (var process in AppServices.Processes.Sample().Where(p => !p.Exited)) Terminate(process);
        RefreshServers();
    }

    private async void KillProcess_Click(object? sender, RoutedEventArgs e)
    {
        if ((sender as Control)?.DataContext is MonitoredProcess process &&
            await AppServices.Dialogs.ShowConfirmationAsync(this, "Terminate server", $"Terminate process {process.Id}?"))
        { Terminate(process); RefreshProcesses(); }
    }

    private void Terminate(MonitoredProcess process)
    {
        try { AppServices.Processes.Terminate(process.Identity); }
        catch (Exception ex) { App.Main.ShowStatus("Could not terminate process: " + ex.Message); }
    }

    private void PauseProcess_Click(object? sender, RoutedEventArgs e)
    {
        if ((sender as Control)?.DataContext is MonitoredProcess process)
        { AppServices.Processes.Pause(process.Identity, !process.Paused); RefreshProcesses(); }
    }

    private void PauseAll_Click(object? sender, RoutedEventArgs e)
    {
        AppServices.Processes.IsPaused = !AppServices.Processes.IsPaused;
        PauseAllButton.Content = AppServices.Processes.IsPaused ? "Resume monitoring" : "Pause monitoring";
        RefreshProcesses();
    }

    private void Output_Click(object? sender, RoutedEventArgs e)
    {
        if ((sender as Control)?.DataContext is not MonitoredProcess process) return;
        var text = new TextBox { IsReadOnly = true, AcceptsReturn = true,
            FontFamily = new global::Avalonia.Media.FontFamily("monospace"), Text = AppServices.Processes.ReadOutput(process.Identity) };
        var window = new Window { Title = $"Process {process.Id} output", Width = 800, Height = 450, Content = text };
        var refresh = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        refresh.Tick += (_, _) => text.Text = AppServices.Processes.ReadOutput(process.Identity);
        window.Closed += (_, _) => refresh.Stop();
        window.Show((Window)TopLevel.GetTopLevel(this)!);
        refresh.Start();
    }

    private void RefreshProcesses()
    {
        var selected = (ProcessGrid.SelectedItem as MonitoredProcess)?.Identity;
        var processes = AppServices.Processes.Sample();
        ProcessGrid.ItemsSource = processes;
        ProcessGrid.SelectedItem = processes.FirstOrDefault(p => p.Identity == selected);
        UpdateProcessChart();
    }

    private void ProcessSelectionChanged(object? sender, SelectionChangedEventArgs e) => UpdateProcessChart();
    private void UpdateProcessChart()
    {
        if (ProcessChart == null) return;
        var history = (ProcessGrid.SelectedItem as MonitoredProcess)?.History ?? Array.Empty<ProcessSample>();
        ProcessChart.Series = new ISeries[] {
            new LineSeries<double> { Name = "CPU %", Values = history.Select(p => p.CpuPercent).ToArray(), GeometrySize = 0, Fill = null },
            new LineSeries<double> { Name = "Memory MB", Values = history.Select(p => p.MemoryBytes / 1048576d).ToArray(), GeometrySize = 0, Fill = null, ScalesYAt = 1 }
        };
        ProcessChart.XAxes = new[] { new Axis { IsVisible = false } };
        ProcessChart.YAxes = new[] { new Axis { MinLimit = 0, MaxLimit = 100, Name = "CPU %" }, new Axis { MinLimit = 0, Name = "Memory MB", Position = LiveChartsCore.Measure.AxisPosition.End } };
    }

    private void RefreshServers()
    {
        try
        {
            AppServices.Processes.Rescan(); RefreshProcesses();
            _ = RefreshTemperatureAsync();
        }
        catch (Exception ex) { App.Main.ShowStatus("Could not scan processes: " + ex.Message); }
    }

    public void Dispose()
    {
        _timer.Stop();
        if (_metrics is IDisposable disposable) disposable.Dispose();
    }
}
