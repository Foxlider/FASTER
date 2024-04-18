using ControlzEx.Theming;

using LiveCharts;
using LiveCharts.Configurations;

using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace FASTER.Views
{
    /// <summary>
    /// Interaction logic for ServerStatus.xaml
    /// </summary>
    public partial class ServerStatus
    {
        public bool Updating
        { get; set; }

        internal object             locked = new object();
        private  PerformanceCounter _cpuCounter;
        private  PerformanceCounter _ramCounter;

        private ObservableCollection<ProcessSpy> processes = new ObservableCollection<ProcessSpy>();
        readonly long totalRamBytes;
        private TempData td;

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetPhysicallyInstalledSystemMemory(out long totalMemoryInKilobytes);

        public ServerStatus()
        {
            InitializeComponent();
            dgProcess.ItemsSource = processes;

            Task.Factory.StartNew(StartPerfsCounters);

            try
            {
                //Get Total System Ram
                GetPhysicallyInstalledSystemMemory(out totalRamBytes);
                gaugeRam.To = Convert.ToInt32(totalRamBytes / 1024);
            }
            catch
            {
                IFlyoutMessage.Content = "Could not get the RAM amount.";
                IFlyout.IsOpen         = true;
            }
            //Format gauges labels
            gaugeRam.LabelFormatter = value => $"{value / 1024:0.00} Gb";
            gaugeCpu.LabelFormatter = value => $"{value:0.00} %";

            td = new TempData();
            ICPUChart.DataContext = td;

            RefreshServers();
        }

        private void StartPerfsCounters()
        {
            //Start Performance counters
            try
            {
                _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                _ramCounter = new PerformanceCounter("Memory", "Available KBytes");
                Updating = true;
            }
            catch
            {
                IFlyoutMessage.Content = "Could not start the performance counters.";
                IFlyout.IsOpen = true;
            }
            Task.Factory.StartNew(Updater, TaskCreationOptions.LongRunning);
        }

        public MainWindow MetroWindow => (MainWindow)Window.GetWindow(this);

        private void Updater()
        {
            while (Updating)
            {
                lock (locked)
                {
                    var ram = _ramCounter.NextValue();
                    Dispatcher?.BeginInvoke(new Action(() =>
                    {
                        gaugeCpu.Value = _cpuCounter.NextValue();
                        gaugeRam.Value = totalRamBytes > ram 
                            ? Convert.ToInt64((totalRamBytes - ram) / 1024)
                            : Convert.ToInt64(totalRamBytes         / 1024);
                    }));
                }
                Thread.Sleep(1000);
            }
        }

        #region Gauges buttons
        private void IToggleGauges_Click(object sender, RoutedEventArgs e)
        {
            Updating = !Updating;
            if (Updating)
            { Task.Factory.StartNew(Updater, TaskCreationOptions.LongRunning); }
        }
        #endregion

        #region Process Buttons
        private void PlayPause_Click(object sender, RoutedEventArgs e)
        {
            if (!(((FrameworkElement) e.Source).DataContext is ProcessSpy view)) return;
            view.StartStop();
        }

        private void KillProcess_Click(object sender, RoutedEventArgs e)
        {
            if (!(((FrameworkElement) e.Source).DataContext is ProcessSpy view)) return;
            view.IsReading = false;
            view.proc.Kill();
            RefreshServers();
        }
        #endregion

        #region Console Viewer
        private void ReadOutput_Click(object sender, RoutedEventArgs e)
        {
            if (!(((FrameworkElement) e.Source).DataContext is ProcessSpy view)) return;
            var s = view.GetOutput();
            IConsoleViewer.Title = $"Process {view.ProcessId}";
            IConsoleViewer.IsOpen = true;
            IConsoleViewerContent.Text = s ?? ">_ ";
        }

        private void IConsoleViewer_Closing(object sender, CancelEventArgs e)
        { IConsoleViewerContent.Text = ""; }
        #endregion

        private void DgProcess_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        { dgProcess.UnselectAll(); }

        #region Processes Buttons
        private void IStartPerfs_Click(object sender, RoutedEventArgs e)
        {
            foreach (ProcessSpy element in dgProcess.Items)
            {
                if (!element.IsReading)
                { element.StartStop(); }
            }
        }

        private void IStopPerfs_Click(object sender, RoutedEventArgs e)
        {
            foreach (ProcessSpy element in dgProcess.Items)
            {
                if (element.IsReading)
                { element.StartStop(); }
            }
        }

        private async void IKillAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (ProcessSpy element in dgProcess.Items)
            {
                element.IsReading = false;
                element.proc.Kill();
            }
            await Task.Delay(1000);
            RefreshServers();
        }

        private void IRescanAll_Click(object sender, RoutedEventArgs e)
        {
            Analytics.TrackEvent("ServerStatus - Rescanning Servers", new Dictionary<string, string> {
                { "Name", MetroWindow.SteamUpdaterViewModel.Parameters.Username}
            });
            RefreshServers();
        }
        #endregion

        private void RefreshServers()
        {
            processes.Clear();
            foreach (var proc in Process.GetProcesses().Where(p => p.ProcessName.Contains("arma3server")))
            {
                try
                {
                    var p = new ProcessSpy(proc);
                    processes.Add(p);
                }
                catch (InvalidOperationException e)
                {
                    /*The process exited. Cannot add it back*/
                    Crashes.TrackError(e, new Dictionary<string, string> { { "Name", Properties.Settings.Default.steamUserName } });
                }
            }
        }

        private void Expander_Expanded(object sender, RoutedEventArgs e)
        {
            if (GaugeExpander.IsExpanded)
            {
                GaugeRow.Height = new GridLength(2, GridUnitType.Star);
                DataRow.Height = new GridLength(0.5, GridUnitType.Star);
                td.StartStop(true);
            }
            else
            {
                GaugeRow.Height = GridLength.Auto;
                DataRow.Height = new GridLength(2, GridUnitType.Star);
                td.StartStop(false);
            }
        }
    }

    public class ProcessSpy : INotifyPropertyChanged
    {
        public          int     ProcessId   { get; set; }
        public          string  ProcessName { get; set; }
        public          string  ProcessCmd  { get; set; }

        public bool IsReading
        {
            get => _isReading;
            set
            {
                _isReading = value;
                OnPropertyChanged(nameof(IsReading));
            }
        }

        public readonly Process proc;

        public ChartValues<MeasureModel> CPUChartValues      { get; set; }
        public ChartValues<MeasureModel> MemChartValues      { get; set; }
        public double                    CPUAxisStep         { get; set; }
        public double                    CPUAxisUnit         { get; set; }
        public Func<double, string>      DateTimeFormatter   { get; set; }
        public Func<double, string>      MemFormatter        { get; set; }
        public Func<double, string>      PercentageFormatter { get; set; }
        public Brush                     Color               { get; set; }

        private PerformanceCounter         cpuPerf;
        private          double            _axisMax;
        private          double            _axisMin;
        private          bool              _isReading;
        private readonly CancellationToken token;

        private string Output;

        public ProcessSpy(Process p)
        {
            //Process Data
            ProcessId   = p.Id;
            ProcessName = p.ProcessName;
            ProcessCmd  = p.ProcessName;
            proc        = p;
            proc.EnableRaisingEvents = true;
            proc.OutputDataReceived += DataToString;

            token = new CancellationToken();
            var r = new Random();
            var color = ThemeManager.Current.Themes[r.Next(0, ThemeManager.Current.Themes.Count)].PrimaryAccentColor;
            Color = new SolidColorBrush(color);

            //Create mapper for CPU Graph
            var mapper = Mappers.Xy<MeasureModel>()
                                .X(model => model.DateTime.Ticks) //use DateTime.Ticks as X
                                .Y(model => model.Value);         //use the value property as Y
            Charting.For<MeasureModel>(mapper);

            //storing values for the CPU Data
            CPUChartValues = new ChartValues<MeasureModel>();
            MemChartValues = new ChartValues<MeasureModel>();

            //Fore distance between each step for 1 second
            CPUAxisStep = TimeSpan.FromSeconds(1).Ticks;
            CPUAxisUnit = TimeSpan.TicksPerSecond;

            //Set the formatter for the labelling
            DateTimeFormatter = value => new DateTime((long) value).ToString("mm:ss");
            MemFormatter = value => $"{value:0} MB";
            PercentageFormatter = value => $"{value:0.00} %";

            SetAxisLimits(DateTime.Now);
            IsReading = false;
        }

        private void DataToString(object sender, DataReceivedEventArgs e)
        { Output += e.Data; }

        public double AxisMax
        {
            get => _axisMax;
            set
            {
                _axisMax = value;
                OnPropertyChanged(nameof(AxisMax));
            }
        }

        public double AxisMin
        {
            get => _axisMin;
            set
            {
                _axisMin = value;
                OnPropertyChanged(nameof(AxisMin));
            }
        }

        public void ReadCPU()
        {
            //Get Performance counters
            cpuPerf = new PerformanceCounter("Process", "% Processor Time", ProcessName, true);

            //And now, loop
            while (IsReading)
            {
                var now = DateTime.Now;

                //Get current CPU usage
                try
                { 
                    CPUChartValues.Add(new MeasureModel
                    {
                        DateTime = now,
                        Value    = cpuPerf.NextValue()
                    });
                }
                catch
                { 
                    //If the performance counter fails somehow, fill data with 0
                    CPUChartValues.Add(new MeasureModel
                    {
                        DateTime = now,
                        Value    = 0
                    });
                }

                //Get current Memory usage
                MemChartValues.Add(new MeasureModel
                {
                    DateTime = now,
                    Value    = proc.WorkingSet64/(1024.0*1024.0)
                });

                //recalculate axes
                SetAxisLimits(now);
 
                //lets only use the last 20 values
                if (MemChartValues.Count > 20) 
                    MemChartValues.RemoveAt(0);
                if (CPUChartValues.Count > 20) 
                    CPUChartValues.RemoveAt(0);

                //Wait 1sec before next measurement
                Thread.Sleep(1000);
            }
        }

        public string GetOutput()
        { return Output; }

        private void SetAxisLimits(DateTime now)
        {
            AxisMax = now.Ticks + TimeSpan.FromSeconds(0).Ticks; // lets force the axis to be 1 second ahead
            AxisMin = now.Ticks - TimeSpan.FromSeconds(15).Ticks; // and 8 seconds behind
        }

        public void StartStop()
        {
            IsReading = !IsReading;
            if (IsReading) 
                Task.Factory.StartNew(ReadCPU, token);
        }

        #region INotifyPropertyChanged implementation

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName = null)
        { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }

        #endregion
    }

    public class TempData : INotifyPropertyChanged
    {
        public ChartValues<MeasureModel> ChartValues       { get; set; }
        public double                    AxisStep          { get; set; }
        public double                    AxisUnit          { get; set; }
        public Func<double, string>      DateTimeFormatter { get; set; }
        public Func<double, string>      TempFormatter     { get; set; }
        public bool IsReading
        {
            get => _isReading;
            set
            {
                _isReading = value;
                OnPropertyChanged(nameof(IsReading));
            }
        }

        private double _axisMax;
        private double _axisMin;
        private double _axisYMax;
        private double _axisYMin = 150;
        private bool   _isReading;

        public TempData()
        {
            var mapper = Mappers.Xy<MeasureModel>()
                                .X(model => model.DateTime.Ticks) //use DateTime.Ticks as X
                                .Y(model => model.Value);         //use the value property as Y
            Charting.For<MeasureModel>(mapper);

            //Fore distance between each step for 1 second
            AxisStep = TimeSpan.FromSeconds(1).Ticks;
            AxisUnit = TimeSpan.TicksPerSecond;

            //Set the formatter for the labelling
            DateTimeFormatter = value => new DateTime((long) value).ToString("mm:ss");
            TempFormatter = value => $"{value:0.00} °C";

            //storing values for the CPU Data
            ChartValues = new ChartValues<MeasureModel>();
            SetAxisLimits(DateTime.Now);
        }
        public void StartStop(bool? launch = null)
        {
            if (launch != null) 
                IsReading = (bool) launch;
            else
            { IsReading = !IsReading; }
            if (IsReading)
                Task.Factory.StartNew(TempLoop, TaskCreationOptions.LongRunning);
        }

        private void TempLoop()
        {
            while (IsReading)
            {
                var now = DateTime.Now;
                using(ManagementObjectSearcher searcher = new ManagementObjectSearcher(@"root\WMI", "SELECT * FROM MSAcpi_ThermalZoneTemperature"))
                {
                    var obj = searcher.Get().OfType<ManagementObject>().FirstOrDefault();
                    if (obj == null)
                        return;
                    var temperature = float.Parse(obj["CurrentTemperature"].ToString());
                    // Convert the value to celsius degrees
                    temperature = (temperature - (float)2732.0) / (float)10.0;
                    if (temperature >= AxisYMax) AxisYMax = temperature + 1;
                    if (temperature <= AxisYMin) AxisYMin = temperature - 1;
                    try { ChartValues.Add(new MeasureModel { DateTime = now, Value = temperature }); }
                    catch
                    {
                        //If the performance counter fails somehow, fill data with 0
                        ChartValues.Add(new MeasureModel { DateTime = now, Value = 0 });
                    }
                }

                //recalculate axes
                SetAxisLimits(now);

                //lets only use the last 20 values
                if (ChartValues.Count > 20) 
                { ChartValues.RemoveAt(0); }

                //Wait 1sec before next measurement
                Thread.Sleep(1000);
            }
        }

        public double AxisXMax
        {
            get => _axisMax;
            set
            {
                _axisMax = value;
                OnPropertyChanged(nameof(AxisXMax));
            }
        }

        public double AxisXMin
        {
            get => _axisMin;
            set
            {
                _axisMin = value;
                OnPropertyChanged(nameof(AxisXMin));
            }
        }

        public double AxisYMax
        {
            get => _axisYMax;
            set
            {
                _axisYMax = value;
                OnPropertyChanged(nameof(AxisYMax));
            }
        }

        public double AxisYMin
        {
            get => _axisYMin;
            set
            {
                _axisYMin = value;
                OnPropertyChanged(nameof(AxisYMin));
            }
        }

        private void SetAxisLimits(DateTime now)
        {
            AxisXMax = now.Ticks + TimeSpan.FromSeconds(1).Ticks;  // lets force the axis to be 1 second ahead
            AxisXMin = now.Ticks - TimeSpan.FromSeconds(15).Ticks; // and 8 seconds behind
        }

        #region INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
 
        protected virtual void OnPropertyChanged(string propertyName = null)
        { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); }
        #endregion
    }

    public class MeasureModel
    {
        public DateTime DateTime { get; set; }
        public double   Value    { get; set; }

        public override string ToString()
        { return Value.ToString(CultureInfo.InvariantCulture); }
    }
}
