using System;
using System.ComponentModel;
using System.Globalization;
// ReSharper disable RedundantDefaultMemberInitializer

namespace FASTER.Models
{
    public static class ProfileCfgArrays
    {
        public static string[] LimitedDistanceStrings { get; } = { "Never", "Limited Distance", "Always" };
        public static string[] FadeOutStrings { get; } = { "Never", "Fade Out", "Always" };
        public static string[] EnabledStrings { get; } = { "Disabled", "Enabled" };
        public static string[] TacticalPingStrings { get; } = { "Disabled", "3D only", "Map only", "Both" };
        public static string[] AiPresetStrings { get; } = { "Low", "Normal", "High", "Custom" };
        public static string[] ThirdPersonStrings { get; } = { "Disabled", "Enabled", "Vehicles Only" };
        public static string[] ForcedDifficultyStrings { get; } = { "Recruit", "Regular", "Veteran", "Custom" };
    }

    [Serializable]
    public class Arma3Profile : INotifyPropertyChanged
    {
        private ushort reducedDamage      = 0;
        private ushort groupIndicators    = 2;
        private ushort friendlyTags       = 1;
        private ushort enemyTags          = 0;
        private ushort detectedMines      = 1;
        private ushort commands           = 1;
        private ushort waypoints          = 1;
        private ushort weaponInfo         = 2;
        private ushort stanceIndicator    = 2;
        private ushort staminaBar         = 1;
        private ushort weaponCrosshair    = 1;
        private ushort visionAid          = 0;
        private ushort thirdPersonView    = 1;
        private ushort cameraShake        = 1;
        private ushort scoreTable         = 1;
        private ushort deathMessages      = 1;
        private ushort vonID              = 1;
        private ushort mapContentFriendly = 1;
        private ushort mapContentEnemy    = 1;
        private ushort mapContentMines    = 1;
        private ushort autoReport         = 0;
        private ushort multipleSaves      = 0;
        private int tacticalPing          = 1;

        private ushort aiLevelPreset      = 3;
        private double skillAi            = 0.5;
        private double precisionAi        = 0.5;

        private string armaProfileContent;

        public string ArmaProfileContent
        {
            get => armaProfileContent;
            set 
            { 
                armaProfileContent = value;
                RaisePropertyChanged("ArmaProfileContent");
            }
        }

        public string ReducedDamage
        {
            get => ProfileCfgArrays.EnabledStrings[reducedDamage];
            set
            {
                reducedDamage = (ushort)IndexOrKeep(ProfileCfgArrays.EnabledStrings, value, reducedDamage);
                RaisePropertyChanged("ReducedDamage");
            }
        }

        public string GroupIndicators
        {
            get => ProfileCfgArrays.LimitedDistanceStrings[groupIndicators];
            set
            {
                groupIndicators = (ushort)IndexOrKeep(ProfileCfgArrays.LimitedDistanceStrings, value, groupIndicators);
                RaisePropertyChanged("GroupIndicators");
            }
        }

        public string FriendlyTags
        {
            get => ProfileCfgArrays.LimitedDistanceStrings[friendlyTags];
            set
            {
                friendlyTags = (ushort)IndexOrKeep(ProfileCfgArrays.LimitedDistanceStrings, value, friendlyTags);
                RaisePropertyChanged("FriendlyTags");
            }
        }

        public string EnemyTags
        {
            get => ProfileCfgArrays.LimitedDistanceStrings[enemyTags];
            set
            {
                enemyTags = (ushort)IndexOrKeep(ProfileCfgArrays.LimitedDistanceStrings, value, enemyTags);
                RaisePropertyChanged("EnemyTags");
            }
        }

        public string DetectedMines
        {
            get => ProfileCfgArrays.LimitedDistanceStrings[detectedMines];
            set
            {
                detectedMines = (ushort)IndexOrKeep(ProfileCfgArrays.LimitedDistanceStrings, value, detectedMines);
                RaisePropertyChanged("DetectedMines");
            }
        }

        public string Commands
        {
            get => ProfileCfgArrays.FadeOutStrings[commands];
            set
            {
                commands = (ushort)IndexOrKeep(ProfileCfgArrays.FadeOutStrings, value, commands);
                RaisePropertyChanged("Commands");
            }
        }

        public string Waypoints
        {
            get => ProfileCfgArrays.FadeOutStrings[waypoints];
            set
            {
                waypoints = (ushort)IndexOrKeep(ProfileCfgArrays.FadeOutStrings, value, waypoints);
                RaisePropertyChanged("Waypoints");
            }
        }

        public string WeaponInfo
        {
            get => ProfileCfgArrays.FadeOutStrings[weaponInfo];
            set
            {
                weaponInfo = (ushort)IndexOrKeep(ProfileCfgArrays.FadeOutStrings, value, weaponInfo);
                RaisePropertyChanged("WeaponInfo");
            }
        }

        public string StanceIndicator
        {
            get => ProfileCfgArrays.FadeOutStrings[stanceIndicator];
            set
            {
                stanceIndicator = (ushort)IndexOrKeep(ProfileCfgArrays.FadeOutStrings, value, stanceIndicator);
                RaisePropertyChanged("StanceIndicator");
            }
        }

        public string StaminaBar
        {
            get => ProfileCfgArrays.EnabledStrings[staminaBar];
            set
            {
                staminaBar = (ushort)IndexOrKeep(ProfileCfgArrays.EnabledStrings, value, staminaBar);
                RaisePropertyChanged("StaminaBar");
            }
        }

        public string WeaponCrosshair
        {
            get => ProfileCfgArrays.EnabledStrings[weaponCrosshair];
            set
            {
                weaponCrosshair = (ushort)IndexOrKeep(ProfileCfgArrays.EnabledStrings, value, weaponCrosshair);
                RaisePropertyChanged("WeaponCrosshair");
            }
        }

        public string VisionAid
        {
            get => ProfileCfgArrays.EnabledStrings[visionAid];
            set
            {
                visionAid = (ushort)IndexOrKeep(ProfileCfgArrays.EnabledStrings, value, visionAid);
                RaisePropertyChanged("VisionAid");
            }
        }

        public string ThirdPersonView
        {
            get => ProfileCfgArrays.ThirdPersonStrings[thirdPersonView];
            set
            {
                thirdPersonView = (ushort)IndexOrKeep(ProfileCfgArrays.ThirdPersonStrings, value, thirdPersonView);
                RaisePropertyChanged("ThirdPersonView");
            }
        }

        public string CameraShake
        {
            get => ProfileCfgArrays.EnabledStrings[cameraShake];
            set
            {
                cameraShake = (ushort)IndexOrKeep(ProfileCfgArrays.EnabledStrings, value, cameraShake);
                RaisePropertyChanged("CameraShake");
            }
        }

        public string ScoreTable
        {
            get => ProfileCfgArrays.EnabledStrings[scoreTable];
            set
            {
                scoreTable = (ushort)IndexOrKeep(ProfileCfgArrays.EnabledStrings, value, scoreTable);
                RaisePropertyChanged("ScoreTable");
            }
        }

        public string DeathMessages
        {
            get => ProfileCfgArrays.EnabledStrings[deathMessages];
            set
            {
                deathMessages = (ushort)IndexOrKeep(ProfileCfgArrays.EnabledStrings, value, deathMessages);
                RaisePropertyChanged("DeathMessages");
            }
        }

        public string VonID
        {
            get => ProfileCfgArrays.EnabledStrings[vonID];
            set
            {
                vonID = (ushort)IndexOrKeep(ProfileCfgArrays.EnabledStrings, value, vonID);
                RaisePropertyChanged("VonID");
            }
        }

        public string MapContentFriendly
        {
            get => ProfileCfgArrays.EnabledStrings[mapContentFriendly];
            set
            {
                mapContentFriendly = (ushort)IndexOrKeep(ProfileCfgArrays.EnabledStrings, value, mapContentFriendly);
                RaisePropertyChanged("MapContentFriendly");
            }
        }

        public string MapContentEnemy
        {
            get => ProfileCfgArrays.EnabledStrings[mapContentEnemy];
            set
            {
                mapContentEnemy = (ushort)IndexOrKeep(ProfileCfgArrays.EnabledStrings, value, mapContentEnemy);
                RaisePropertyChanged("MapContentEnemy");
            }
        }

        public string MapContentMines
        {
            get => ProfileCfgArrays.EnabledStrings[mapContentMines];
            set
            {
                mapContentMines = (ushort)IndexOrKeep(ProfileCfgArrays.EnabledStrings, value, mapContentMines);
                RaisePropertyChanged("MapContentMines");
            }
        }

        public string AutoReport
        {
            get => ProfileCfgArrays.EnabledStrings[autoReport];
            set
            {
                autoReport = (ushort)IndexOrKeep(ProfileCfgArrays.EnabledStrings, value, autoReport);
                RaisePropertyChanged("AutoReport");
            }
        }

        public string MultipleSaves
        {
            get => ProfileCfgArrays.EnabledStrings[multipleSaves];
            set
            {
                multipleSaves = (ushort)IndexOrKeep(ProfileCfgArrays.EnabledStrings, value, multipleSaves);
                RaisePropertyChanged("MultipleSaves");
            }
        }

        public string TacticalPing
        {
            get => ProfileCfgArrays.TacticalPingStrings[tacticalPing];
            set
            {
                tacticalPing = IndexOrKeep(ProfileCfgArrays.TacticalPingStrings, value, tacticalPing);
                RaisePropertyChanged("TacticalPing");
            }
        }

        public string AiLevelPreset
        {
            get => ProfileCfgArrays.AiPresetStrings[aiLevelPreset];
            set
            {
                aiLevelPreset = (ushort)IndexOrKeep(ProfileCfgArrays.AiPresetStrings, value, aiLevelPreset);
                RaisePropertyChanged("AiLevelPreset");
            }
        }

        public double SkillAi
        {
            get => skillAi;
            set
            {
                skillAi = Math.Round(value, 2);
                RaisePropertyChanged("SkillAi");
            }
        }

        public double PrecisionAi
        {
            get => precisionAi;
            set
            {
                precisionAi = Math.Round(value, 2);
                RaisePropertyChanged("PrecisionAi");
            }
        }

        private static int IndexOrKeep(string[] options, string? value, int current)
        {
            var index = Array.IndexOf(options, value);
            return index < 0 ? current : index;
        }

        public Arma3Profile()
        { ArmaProfileContent = ProcessFile(); }

        public string ProcessFile()
        {
            string output = 
                "//\r\n" +
                "// Arma3Profile\r\n" +
                "//\r\n" +
                "class DifficultyPresets\r\n" +
                "{\r\n" +
                "\tclass CustomDifficulty\r\n" +
                "\t{\r\n" +
                "\t\tclass Options\r\n" +
                "\t\t{\r\n" +
                "\t\t\t/* Simulation */\r\n" +
                $"\t\t\treducedDamage = {reducedDamage};\t\t// Reduced damage\r\n" +
                "\t\t\t/* Situational awareness */\r\n" +
                $"\t\t\tgroupIndicators = {groupIndicators};\t\t// Group indicators (0 = never, 1 = limited distance, 2 = always)\r\n" +
                $"\t\t\tfriendlyTags = {friendlyTags};\t\t\t// Friendly name tags (0 = never, 1 = limited distance, 2 = always)\r\n" +
                $"\t\t\tenemyTags = {enemyTags};\t\t\t// Enemy name tags (0 = never, 1 = limited distance, 2 = always)\r\n" +
                $"\t\t\tdetectedMines = {detectedMines};\t\t// Detected mines (0 = never, 1 = limited distance, 2 = always)\r\n" +
                $"\t\t\tcommands = {commands};\t\t\t// Commands (0 = never, 1 = fade out, 2 = always)\r\n" +
                $"\t\t\twaypoints = {waypoints};\t\t\t// Waypoints (0 = never, 1 = fade out, 2 = always)\r\n" +
                $"\t\t\ttacticalPing = {tacticalPing};\t\t\t// Tactical ping (0 = disable, 1 = In 3D scene, 2 = On map, 3 = Both)\r\n" +
                "\t\t\t/* Personal awareness */\r\n" +
                $"\t\t\tweaponInfo = {weaponInfo};\t\t\t// Weapon info (0 = never, 1 = fade out, 2 = always)\r\n" +
                $"\t\t\tstanceIndicator = {stanceIndicator};\t\t// Stance indicator (0 = never, 1 = fade out, 2 = always)\r\n" +
                $"\t\t\tstaminaBar = {staminaBar};\t\t\t// Stamina bar\r\n" +
                $"\t\t\tweaponCrosshair = {weaponCrosshair};\t\t// Weapon crosshair\r\n" +
                $"\t\t\tvisionAid = {visionAid};\t\t\t// Vision aid\r\n" +
                "\t\t\t/* View */\r\n" +
                $"\t\t\tthirdPersonView = {thirdPersonView};\t\t// 3rd person view\r\n" +
                $"\t\t\tcameraShake = {cameraShake};\t\t\t// Camera shake\r\n" +
                "\t\t\t/* Multiplayer */\r\n" +
                $"\t\t\tscoreTable = {scoreTable};\t\t\t// Score table\r\n" +
                $"\t\t\tdeathMessages = {deathMessages};\t\t// Killed by\r\n" +
                $"\t\t\tvonID = {vonID};\t\t\t// VoN ID\r\n" +
                "\t\t\t/* Misc */\r\n" +
                $"\t\t\tmapContentFriendly = {mapContentFriendly};\t\t// Map friendlies\t\t(0 = disabled, 1 = enabled) // since  Arma 3 v1.68\r\n" +
                $"\t\t\tmapContentEnemy = {mapContentEnemy};\t\t// Map Enemies\t\t(0 = disabled, 1 = enabled) // since  Arma 3 v1.68\r\n" +
                $"\t\t\tmapContentMines = {mapContentMines};\t\t// Map Mines \t\t(0 = disabled, 1 = enabled) // since  Arma 3 v1.68\r\n" +
                $"\t\t\tautoReport = {autoReport};\t\t\t// (former autoSpot) Automatic reporting of spotted enemied by players only. This doesn't have any effect on AIs.\r\n" +
                $"\t\t\tmultipleSaves = {multipleSaves};\t\t// Multiple saves\r\n" +
                "\t\t};\r\n" +
                "\t\t\r\n" +
                "\t\t// aiLevelPreset defines AI skill level and is counted from 0 and can have following values: 0 (Low), 1 (Normal), 2 (High), 3 (Custom).\r\n" +
                "\t\t// when 3 (Custom) is chosen, values of skill and precision are taken from the class CustomAILevel.\r\n" +
                $"\t\taiLevelPreset = {aiLevelPreset};\r\n" +
                "\t};\r\n" +
                "\tclass CustomAILevel\r\n" +
                "\t{\r\n" +
                $"\t\tskillAI = {skillAi.ToString(CultureInfo.InvariantCulture)};\r\n" +
                $"\t\tprecisionAI = {precisionAi.ToString(CultureInfo.InvariantCulture)};\r\n" +
                "\t};\r\n" +
                "};";
            return output;
        }


        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged(string property)
        {
            if (PropertyChanged == null) return;
            PropertyChanged(this, new PropertyChangedEventArgs(property));
            if (property != "ArmaProfileContent") ArmaProfileContent = ProcessFile();
        }
    }
}
