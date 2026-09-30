using System;
using System.ComponentModel;

namespace FASTER.Models
{
    [Serializable]
    public class ChannelRestriction : INotifyPropertyChanged
    {
        private int  id;
        private bool text;
        private bool voice;
        private bool mapMarkers;
        private bool drawOnMap;

        public int Id
        {
            get => id;
            set
            {
                id = value;
                RaisePropertyChanged(nameof(Id));
                RaisePropertyChanged(nameof(Name));
            }
        }

        public string Name
        {
            get
            {
                switch (id)
                {
                    case 0:  return "Global";
                    case 1:  return "Side";
                    case 2:  return "Command";
                    case 3:  return "Group";
                    case 4:  return "Vehicle";
                    case 5:  return "Direct";
                    case 16: return "System";
                    default: return id.ToString();
                }
            }
        }

        public bool Text
        {
            get => text;
            set
            {
                text = value;
                RaisePropertyChanged(nameof(Text));
            }
        }

        public bool Voice
        {
            get => voice;
            set
            {
                voice = value;
                RaisePropertyChanged(nameof(Voice));
            }
        }

        public bool MapMarkers
        {
            get => mapMarkers;
            set
            {
                mapMarkers = value;
                RaisePropertyChanged(nameof(MapMarkers));
            }
        }

        public bool DrawOnMap
        {
            get => drawOnMap;
            set
            {
                drawOnMap = value;
                RaisePropertyChanged(nameof(DrawOnMap));
            }
        }

        public bool IsDefault => !text && !voice && !mapMarkers && !drawOnMap;

        public string ToCfg()
        {
            return $"{{ {id}, {Bool(text)}, {Bool(voice)}, {Bool(mapMarkers)}, {Bool(drawOnMap)} }}";
        }

        private static string Bool(bool b)
        {
            return b ? "true" : "false";
        }

        public static ChannelRestriction[] CreateDefaults()
        {
            return new[]
            {
                new ChannelRestriction { Id = 0 },
                new ChannelRestriction { Id = 1 },
                new ChannelRestriction { Id = 2 },
                new ChannelRestriction { Id = 3 },
                new ChannelRestriction { Id = 4 },
                new ChannelRestriction { Id = 5 },
                new ChannelRestriction { Id = 16 }
            };
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void RaisePropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
    }
}
