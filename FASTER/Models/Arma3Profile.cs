using System;
using System.ComponentModel;
// ReSharper disable RedundantDefaultMemberInitializer

namespace FASTER.Models
{
    static class ProfileCfgArrays
    {
        public static string[] LimitedDistanceStrings { get; } = { "Never", "Limited Distance", "Always" };
        public static string[] FadeOutStrings { get; } = { "Never", "Fade Out", "Always" };
        public static string[] EnabledStrings { get; } = { "Disabled", "Enabled" };

        public static string[] AiPresetStrings { get; } = { "Low", "Normal", "High", "Custom" };
        public static string[] ThirdPersonStrings { get; } = { "Disabled", "Enabled", "Vehicles Only" };
    }

    [Serializable]
    public class Arma3Profile : INotifyPropertyChanged
    {
        private ushort reducedDamage = 0;
        private ushort groupIndicators = 2;
        private ushort friendlyTags = 1;
        private ushort enemyTags = 0;
        private ushort detectedMines = 1;
        private ushort commands = 1;
        private ushort waypoints = 1;
        private ushort weaponInfo = 2;
        private ushort stanceIndicator = 2;
        private ushort staminaBar = 1;
        private ushort weaponCrosshair = 1;
        private ushort visionAid = 0;
        private ushort thirdPersonView = 1;
        private ushort cameraShake = 1;
        private ushort scoreTable = 1;
        private ushort deathMessages = 1;
        private ushort vonID = 1;
        private ushort mapContentFriendly = 1;
        private ushort mapContentEnemy = 1;
        private ushort mapContentMines = 1;
        private ushort autoReport = 0;
        private ushort multipleSaves = 0;
        private ushort tacticalPing = 1;

        private ushort aiLevelPreset = 3;

        private double skillAi = 0.5;
        private double precisionAi = 0.5;


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
                reducedDamage = (ushort)Array.IndexOf(ProfileCfgArrays.EnabledStrings, value);
                RaisePropertyChanged("ReducedDamage");
            }
        }

        public string GroupIndicators
        {
            get => ProfileCfgArrays.LimitedDistanceStrings[groupIndicators];
            set
            {
                groupIndicators = (ushort)Array.IndexOf(ProfileCfgArrays.LimitedDistanceStrings, value);
                RaisePropertyChanged("GroupIndicators");
            }
        }

        public string FriendlyTags
        {
            get => ProfileCfgArrays.LimitedDistanceStrings[friendlyTags];
            set
            {
                friendlyTags = (ushort)Array.IndexOf(ProfileCfgArrays.LimitedDistanceStrings, value);
                RaisePropertyChanged("FriendlyTags");
            }
        }

        public string EnemyTags
        {
            get => ProfileCfgArrays.LimitedDistanceStrings[enemyTags];
            set
            {
                enemyTags = (ushort)Array.IndexOf(ProfileCfgArrays.LimitedDistanceStrings, value);
                RaisePropertyChanged("EnemyTags");
            }
        }

        public string DetectedMines
        {
            get => ProfileCfgArrays.LimitedDistanceStrings[detectedMines];
            set
            {
                detectedMines = (ushort)Array.IndexOf(ProfileCfgArrays.LimitedDistanceStrings, value);
                RaisePropertyChanged("DetectedMines");
            }
        }

        public string Commands
        {
            get => ProfileCfgArrays.FadeOutStrings[commands];
            set
            {
                commands = (ushort)Array.IndexOf(ProfileCfgArrays.FadeOutStrings, value);
                RaisePropertyChanged("Commands");
            }
        }

        public string Waypoints
        {
            get => ProfileCfgArrays.FadeOutStrings[waypoints];
            set
            {
                waypoints = (ushort)Array.IndexOf(ProfileCfgArrays.FadeOutStrings, value);
                RaisePropertyChanged("Waypoints");
            }
        }

        public string WeaponInfo
        {
            get => ProfileCfgArrays.FadeOutStrings[weaponInfo];
            set
            {
                weaponInfo = (ushort)Array.IndexOf(ProfileCfgArrays.FadeOutStrings, value);
                RaisePropertyChanged("WeaponInfo");
            }
        }

        public string StanceIndicator
        {
            get => ProfileCfgArrays.FadeOutStrings[stanceIndicator];
            set
            {
                stanceIndicator = (ushort)Array.IndexOf(ProfileCfgArrays.FadeOutStrings, value);
                RaisePropertyChanged("StanceIndicator");
            }
        }

        public string StaminaBar
        {
            get => ProfileCfgArrays.EnabledStrings[staminaBar];
            set
            {
                staminaBar = (ushort)Array.IndexOf(ProfileCfgArrays.EnabledStrings, value);
                RaisePropertyChanged("StaminaBar");
            }
        }

        public string WeaponCrosshair
        {
            get => ProfileCfgArrays.EnabledStrings[weaponCrosshair];
            set
            {
                weaponCrosshair = (ushort)Array.IndexOf(ProfileCfgArrays.EnabledStrings, value);
                RaisePropertyChanged("WeaponCrosshair");
            }
        }

        public string VisionAid
        {
            get => ProfileCfgArrays.EnabledStrings[visionAid];
            set
            {
                visionAid = (ushort)Array.IndexOf(ProfileCfgArrays.EnabledStrings, value);
                RaisePropertyChanged("VisionAid");
            }
        }

        public string ThirdPersonView
        {
            get => ProfileCfgArrays.ThirdPersonStrings[thirdPersonView];
            set
            {
                thirdPersonView = (ushort)Array.IndexOf(ProfileCfgArrays.ThirdPersonStrings, value);
                RaisePropertyChanged("ThirdPersonView");
            }
        }

        public string CameraShake
        {
            get => ProfileCfgArrays.EnabledStrings[cameraShake];
            set
            {
                cameraShake = (ushort)Array.IndexOf(ProfileCfgArrays.EnabledStrings, value);
                RaisePropertyChanged("CameraShake");
            }
        }

        public string ScoreTable
        {
            get => ProfileCfgArrays.EnabledStrings[scoreTable];
            set
            {
                scoreTable = (ushort)Array.IndexOf(ProfileCfgArrays.EnabledStrings, value);
                RaisePropertyChanged("ScoreTable");
            }
        }

        public string DeathMessages
        {
            get => ProfileCfgArrays.EnabledStrings[deathMessages];
            set
            {
                deathMessages = (ushort)Array.IndexOf(ProfileCfgArrays.EnabledStrings, value);
                RaisePropertyChanged("DeathMessages");
            }
        }

        public string VonID
        {
            get => ProfileCfgArrays.EnabledStrings[vonID];
            set
            {
                vonID = (ushort)Array.IndexOf(ProfileCfgArrays.EnabledStrings, value);
                RaisePropertyChanged("VonID");
            }
        }

        public string MapContentFriendly
        {
            get => ProfileCfgArrays.EnabledStrings[mapContentFriendly];
            set
            {
                mapContentFriendly = (ushort)Array.IndexOf(ProfileCfgArrays.EnabledStrings, value);
                RaisePropertyChanged("MapContentFriendly");
            }
        }

        public string MapContentEnemy
        {
            get => ProfileCfgArrays.EnabledStrings[mapContentEnemy];
            set
            {
                mapContentEnemy = (ushort)Array.IndexOf(ProfileCfgArrays.EnabledStrings, value);
                RaisePropertyChanged("MapContentEnemy");
            }
        }

        public string MapContentMines
        {
            get => ProfileCfgArrays.EnabledStrings[mapContentMines];
            set
            {
                mapContentMines = (ushort)Array.IndexOf(ProfileCfgArrays.EnabledStrings, value);
                RaisePropertyChanged("MapContentMines");
            }
        }

        public string AutoReport
        {
            get => ProfileCfgArrays.EnabledStrings[autoReport];
            set
            {
                autoReport = (ushort)Array.IndexOf(ProfileCfgArrays.EnabledStrings, value);
                RaisePropertyChanged("AutoReport");
            }
        }

        public string MultipleSaves
        {
            get => ProfileCfgArrays.EnabledStrings[multipleSaves];
            set
            {
                multipleSaves = (ushort)Array.IndexOf(ProfileCfgArrays.EnabledStrings, value);
                RaisePropertyChanged("MultipleSaves");
            }
        }

        public string TacticalPing
        {
            get => ProfileCfgArrays.EnabledStrings[tacticalPing];
            set
            {
                tacticalPing = (ushort)Array.IndexOf(ProfileCfgArrays.EnabledStrings, value);
                RaisePropertyChanged("TacticalPing");
            }
        }

        public string AiLevelPreset
        {
            get => ProfileCfgArrays.AiPresetStrings[aiLevelPreset];
            set
            {
                aiLevelPreset = (ushort)Array.IndexOf(ProfileCfgArrays.AiPresetStrings, value);
                RaisePropertyChanged("AiLevelPreset");
            }
        }

        public double SkillAi
        {
            get => skillAi;
            set
            {
                skillAi = value;
                RaisePropertyChanged("SkillAi");
            }
        }

        public double PrecisionAi
        {
            get => precisionAi;
            set
            {
                precisionAi = value;
                RaisePropertyChanged("PrecisionAi");
            }
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
                "	class Custom\r\n" +
                "	{\r\n" +
                "		class Options\r\n" +
                "		{\r\n" +
                "			/* Simulation */\r\n" +
                $"			reducedDamage = {reducedDamage};		// Reduced damage\r\n" +
                "			/* Situational awareness */\r\n" +
                $"			groupIndicators = {groupIndicators};	// Group indicators (0 = never, 1 = limited distance, 2 = always)\r\n" +
                $"			friendlyTags = {friendlyTags};		// Friendly name tags (0 = never, 1 = limited distance, 2 = always)\r\n" +
                $"			enemyTags = {enemyTags};			// Enemy name tags (0 = never, 1 = limited distance, 2 = always)\r\n" +
                $"			detectedMines = {detectedMines};		// Detected mines (0 = never, 1 = limited distance, 2 = always)\r\n" +
                $"			commands = {commands};			// Commands (0 = never, 1 = fade out, 2 = always)\r\n" +
                $"			waypoints = {waypoints};			// Waypoints (0 = never, 1 = fade out, 2 = always)\r\n" +
                $"			tacticalPing = {tacticalPing};		// Tactical ping (0 = disable, 1 = enable)\r\n" +
                "			/* Personal awareness */" +
                $"			weaponInfo = {weaponInfo};			// Weapon info (0 = never, 1 = fade out, 2 = always)\r\n" +
                $"			stanceIndicator = {stanceIndicator};	// Stance indicator (0 = never, 1 = fade out, 2 = always)\r\n" +
                $"			staminaBar = {staminaBar};			// Stamina bar\r\n" +
                $"			weaponCrosshair = {weaponCrosshair};	// Weapon crosshair\r\n" +
                $"			visionAid = {visionAid};			// Vision aid\r\n" +
                "			/* View */" +
                $"			thirdPersonView = {thirdPersonView};	// 3rd person view\r\n" +
                $"			cameraShake = {cameraShake};		// Camera shake\r\n" +
                "			/* Multiplayer */" +
                $"			scoreTable = {scoreTable};			// Score table\r\n" +
                $"			deathMessages = {deathMessages};		// Killed by\r\n" +
                $"			vonID = {vonID};				// VoN ID\r\n" +
                "			/* Misc */\r\n" +
                $"			mapContentFriendly = {mapContentFriendly};	// Map friendlies		(0 = disabled, 1 = enabled) // since  Arma 3 v1.68\r\n" +
                $"			mapContentEnemy = {mapContentEnemy};    // Map Enemies			(0 = disabled, 1 = enabled) // since  Arma 3 v1.68\r\n" +
                $"			mapContentMines = {mapContentMines};    // Map Mines 			(0 = disabled, 1 = enabled) // since  Arma 3 v1.68\r\n" +
                $"			autoReport = {autoReport};			// (former autoSpot) Automatic reporting of spotted enemied by players only. This doesn't have any effect on AIs.\r\n" +
                $"			multipleSaves = {multipleSaves};		// Multiple saves\r\n" +
                "		};\r\n" +
                "		\r\n" +
                "		// aiLevelPreset defines AI skill level and is counted from 0 and can have following values: 0 (Low), 1 (Normal), 2 (High), 3 (Custom).\r\n" +
                "		// when 3 (Custom) is chosen, values of skill and precision are taken from the class CustomAILevel.\r\n" +
                $"		aiLevelPreset = {aiLevelPreset};\r\n" +
                "	};\r\n" +
                "	class CustomAILevel\r\n" +
                "	{\r\n" +
                $"		skillAI = {skillAi};\r\n" +
                $"		precisionAI = {precisionAi};\r\n" +
                "	};\r\n" +
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
