using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

using LiveCharts;
using LiveCharts.Configurations;

using MahApps.Metro;

namespace FASTER
{
    /// <summary>
    /// Interaction logic for ServerStatus.xaml
    /// </summary>
    public partial class ServerStatus : UserControl
    {
        public bool Updating {get; set; }

        private readonly PerformanceCounter cpuCounter;
        private readonly PerformanceCounter ramCounter;
        
        private ObservableCollection<ProcessSpy> processes = new ObservableCollection<ProcessSpy>();
        readonly long totalRamBytes;

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetPhysicallyInstalledSystemMemory(out long TotalMemoryInKilobytes);
        
        public ServerStatus()
        {
            InitializeComponent();
            dgProcess.ItemsSource = processes;
            Updating = true;

            //Start Performance counters
            cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            ramCounter = new PerformanceCounter("Memory", "Available KBytes");
            
            //Get Total System Ram
            GetPhysicallyInstalledSystemMemory(out totalRamBytes);
            gaugeRam.To = Convert.ToInt32(totalRamBytes / 1024);

            //Format gauges labels
            gaugeRam.LabelFormatter = value => $"{value / 1024:0.00} Gb";
            gaugeCpu.LabelFormatter = value => $"{value:0.00} %";

            RefreshServers();
            Task.Factory.StartNew(Updater, TaskCreationOptions.LongRunning);
        }

        private void Updater()
        {
            while (Updating)
            {
                var ram = ramCounter.NextValue();
                Dispatcher?.BeginInvoke(new Action(() =>
                {
                    gaugeCpu.Value = cpuCounter.NextValue();
                    gaugeRam.Value = totalRamBytes > ram 
                        ? Convert.ToInt64((totalRamBytes - ram) / 1024)
                        : Convert.ToInt64(totalRamBytes         / 1024);
                }));
                Thread.Sleep(1000);
            }
        }

        #region Gauges buttons
        private void IToggleGauges_Click(object sender, RoutedEventArgs e)
        {
            Updating = !Updating;
            if(Updating)
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
        { RefreshServers(); }
        #endregion


        private void RefreshServers()
        {
            processes.Clear();
            foreach (var proc in Process.GetProcessesByName("arma3server"))
            {
                var p = new ProcessSpy(proc);
                processes.Add(p);
            }
        }

        //private void Expander_Expanded(object sender, RoutedEventArgs e)
        //{
        //    if (GaugeExpander.IsExpanded)
        //    {
        //        GaugeRow.Height = new GridLength(2, GridUnitType.Star);
        //        DataRow.Height = new GridLength(0.5, GridUnitType.Star);
        //    }
        //    else
        //    {
        //        GaugeRow.Height = GridLength.Auto;
        //        DataRow.Height = new GridLength(2, GridUnitType.Star);
        //    }
        //}
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
                OnPropertyChanged("IsReading");
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
            ProcessCmd  = p.ProcessName; //TODO use real command line parameters 
            proc        = p;
            proc.EnableRaisingEvents = true;
            proc.OutputDataReceived += DataToString;

            token = new CancellationToken();
            var r = new Random();
            var col = ThemeManager.ColorSchemes[r.Next(0, ThemeManager.ColorSchemes.Count)];
            Color = col.ShowcaseBrush;

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
                OnPropertyChanged("AxisMax");
            }
        }

        public double AxisMin
        {
            get => _axisMin;
            set
            {
                _axisMin = value;
                OnPropertyChanged("AxisMin");
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

    public class MeasureModel
    {
        public DateTime DateTime { get; set; }
        public double   Value    { get; set; }

        public override string ToString()
        { return Value.ToString(CultureInfo.InvariantCulture); }
    }
}
