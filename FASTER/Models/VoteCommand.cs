using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;

namespace FASTER.Models
{
    [Serializable]
    public class VoteCommand : INotifyPropertyChanged
    {
        public static readonly string[] VoteCommandNames       = { "admin", "missions", "mission", "kick", "restart", "reassign" };
        public static readonly string[] VotedAdminCommandNames = { "mission", "missions", "restart", "reassign", "kick" };

        private string  name = "";
        private bool    preMissionStart  = true;
        private bool    postMissionStart = true;
        private double? threshold;

        public string Name
        {
            get => name;
            set
            {
                name = value;
                RaisePropertyChanged(nameof(Name));
            }
        }

        public bool PreMissionStart
        {
            get => preMissionStart;
            set
            {
                preMissionStart = value;
                RaisePropertyChanged(nameof(PreMissionStart));
            }
        }

        public bool PostMissionStart
        {
            get => postMissionStart;
            set
            {
                postMissionStart = value;
                RaisePropertyChanged(nameof(PostMissionStart));
            }
        }

        public double? Threshold
        {
            get => threshold;
            set
            {
                threshold = value;
                RaisePropertyChanged(nameof(Threshold));
            }
        }

        public bool IsDefault => preMissionStart && postMissionStart && threshold == null;

        public string ToCfg()
        {
            return threshold == null
                ? $"{{ \"{name}\", {Bool(preMissionStart)}, {Bool(postMissionStart)} }}"
                : $"{{ \"{name}\", {Bool(preMissionStart)}, {Bool(postMissionStart)}, {threshold.Value.ToString(CultureInfo.InvariantCulture)} }}";
        }

        private static string Bool(bool b)
        {
            return b ? "true" : "false";
        }

        public static VoteCommand[] CreateDefaults(string[] names)
        {
            return names.Select(n => new VoteCommand { Name = n }).ToArray();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void RaisePropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
    }
}
