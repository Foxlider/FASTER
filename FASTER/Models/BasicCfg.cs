using System;
using System.ComponentModel;
using System.Globalization;

namespace FASTER.Models
{
    static class BasicCfgArrays
    {
        public static string[] PerfPresets { get; } = {"Custom", "Arma3 Defaults", "1Mb Preset", "250Mb Preset"};
    }

    [Serializable]
    public class BasicCfg : INotifyPropertyChanged
    {
        private ushort maxMsgSend = 128;
        private ushort maxSizeGuaranteed = 256;
        private ushort maxSizeNonguaranteed = 512;
        private ulong minBandwidth = 131072;
        private ulong maxBandwidth = 10000000000;
        private double minErrorToSend = 0.001;
        private double minErrorToSendNear = 0.01;
        private ushort maxCustomFileSize = 1024;
        private ushort maxPacketSize = 1400;

        private string basicContent;

        public string BasicContent
        {
            get => basicContent;
            set
            {
                basicContent = value;
                RaisePropertyChanged("BasicContent");
            }
        }

        public ushort MaxSizeGuaranteed
        {
            get => maxSizeGuaranteed;
            set
            {
                maxSizeGuaranteed = value;
                RaisePropertyChanged("MaxSizeGuaranteed");
            }
        }

        public ushort MaxSizeNonGuaranteed
        {
            get => maxSizeNonguaranteed;
            set
            {
                maxSizeNonguaranteed = value;
                RaisePropertyChanged("MaxSizeNonGuaranteed");
            }
        }

        public ushort MaxMsgSend
        {
            get => maxMsgSend;
            set
            {
                maxMsgSend = value;
                RaisePropertyChanged("MaxMsgSend");
            }
        }

        public ulong MinBandwidth
        {
            get => minBandwidth;
            set
            {
                minBandwidth = value;
                RaisePropertyChanged("MinBandwidth");
            }
        }

        public ulong MaxBandwidth
        {
            get => maxBandwidth;
            set
            {
                maxBandwidth = value;
                RaisePropertyChanged("MaxBandwidth");
            }
        }

        public ushort MaxPacketSize
        {
            get => maxPacketSize;
            set
            {
                maxPacketSize = value;
                RaisePropertyChanged("MaxPacketSize");
            }
        }

        public double MinErrorToSend
        {
            get => minErrorToSend;
            set
            {
                minErrorToSend = value;
                RaisePropertyChanged("MinErrorToSend");
            }
        }

        public double MinErrorToSendNear
        {
            get => minErrorToSendNear;
            set
            {
                minErrorToSendNear = value;
                RaisePropertyChanged("MinErrorToSendNear");
            }
        }

        public ushort MaxCustomFileSize
        {
            get => maxCustomFileSize;
            set
            {
                maxCustomFileSize = value;
                RaisePropertyChanged("MaxCustomFileSize");
            }
        }

        public string PerfPreset
        {
            get => "Custom";
            set
            {
                switch ((short)Array.IndexOf(BasicCfgArrays.PerfPresets, value))
                {
                    case 1:
                        MaxMsgSend = 128;
                        MaxSizeGuaranteed = 512;
                        MaxSizeNonGuaranteed = 256;
                        MinBandwidth = 131072;
                        MaxBandwidth = 10000000000;
                        MinErrorToSend = 0.001;
                        MinErrorToSendNear = 0.01;
                        MaxPacketSize = 1400;
                        MaxCustomFileSize = 0;
                        break;
                    case 2:
                        MaxMsgSend = 256;
                        MaxSizeGuaranteed = 512;
                        MaxSizeNonGuaranteed = 256;
                        MinBandwidth = 768000;
                        MaxBandwidth = 10000000000;
                        MinErrorToSend = 0.001;
                        MinErrorToSendNear = 0.01;
                        MaxPacketSize = 1400;
                        MaxCustomFileSize = 160;
                        break;
                    case 3:
                        MaxMsgSend           = 256;
                        MaxSizeGuaranteed    = 512;
                        MaxSizeNonGuaranteed = 256;
                        MinBandwidth         = 2000000000;
                        MaxBandwidth         = 10000000000;
                        MinErrorToSend       = 0.001;
                        MinErrorToSendNear   = 0.01;
                        MaxPacketSize        = 1400;
                        MaxCustomFileSize    = 160;
                        break;
                }
                RaisePropertyChanged("PerfPreset");
            }
        }

        public BasicCfg()
        { BasicContent = ProcessFile(); }

        public string ProcessFile()
        {
            string output = "// These options are created by default\r\n"
                          + "language=\"English\";\r\n"
                          + "adapter=-1;\r\n"
                          + "3D_Performance=1.000000;\r\n"
                          + "Resolution_W=800;\r\n"
                          + "Resolution_H=600;\r\n"
                          + "Resolution_Bpp=32;\r\n"
                          + "\r\n"
                          + "\r\n"
                          + "// These options are important for performance tuning\r\n"
                          + "\r\n"
                          + $"MinBandwidth = {minBandwidth};\t\t\t// Bandwidth the server is guaranteed to have (in bps). This value helps server to estimate bandwidth available. Increasing it to too optimistic values can increase lag and CPU load, as too many messages will be sent but discarded. Default: 131072\r\n"
                          + $"MaxBandwidth = {maxBandwidth};\t\t// Bandwidth the server is guaranteed to never have. This value helps the server to estimate bandwidth available.\r\n"
                          + "\r\n"
                          + $"MaxMsgSend = {maxMsgSend};\t\t\t// Maximum number of messages that can be sent in one simulation cycle. Increasing this value can decrease lag on high upload bandwidth servers. Default: 128\r\n"
                          + $"MaxSizeGuaranteed = {maxSizeGuaranteed};\t\t// Maximum size of guaranteed packet in bytes (without headers). Small messages are packed to larger frames. Guaranteed messages are used for non-repetitive events like shooting. Default: 512\r\n"
                          + $"MaxSizeNonguaranteed = {maxSizeNonguaranteed};\t\t// Maximum size of non-guaranteed packet in bytes (without headers). Non-guaranteed messages are used for repetitive updates like soldier or vehicle position. Increasing this value may improve bandwidth requirement, but it may increase lag. Default: 256\r\n"
                          + "\r\n"
                          + $"MinErrorToSend = {minErrorToSend.ToString(CultureInfo.InvariantCulture)};\t\t\t// Minimal error to send updates across network. Using a smaller value can make units observed by binoculars or sniper rifle to move smoother. Default: 0.001\r\n"
                          + $"MinErrorToSendNear = {minErrorToSendNear.ToString(CultureInfo.InvariantCulture)};\t\t// Minimal error to send updates across network for near units. Using larger value can reduce traffic sent for near units. Used to control client to server traffic as well. Default: 0.01\r\n"
                          + "\r\n"
                          + $"MaxCustomFileSize = {maxCustomFileSize};\t\t\t// (bytes) Users with custom face or custom sound larger than this size are kicked when trying to connect.\r\n" 
                          + $"class sockets{{ maxPacketSize = {maxPacketSize};}};";
            return output;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged(string property)
        {
            if (PropertyChanged == null) return;
            PropertyChanged(this, new PropertyChangedEventArgs(property));
            if (property != "BasicContent") BasicContent = ProcessFile();
        }
    }
}
