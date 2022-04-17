﻿using KLPlugins.Leaderboard.Enums;
using KLPlugins.Leaderboard.ksBroadcastingNetwork;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;


namespace KLPlugins.Leaderboard {
    /// <summary>
    /// Settings class, make sure it can be correctly serialized using JSON.net
    /// </summary>
    public class PluginSettings {
        public class DynLeaderboardConfig {
            public string Name { get; set; }

            public OutCarProp OutCarProps = OutCarProp.CarNumber 
                | OutCarProp.CarClass 
                | OutCarProp.IsFinished 
                | OutCarProp.CarClassColor 
                | OutCarProp.TeamCupCategoryColor 
                | OutCarProp.TeamCupCategoryTextColor;
            public OutPitProp OutPitProps = OutPitProp.IsInPitLane;
            public OutPosProp OutPosProps = OutPosProp.OverallPosition | OutPosProp.ClassPosition;
            public OutGapProp OutGapProps = OutGapProp.DynamicGapToFocused;
            public OutStintProp OutStintProps = OutStintProp.None;
            public OutDriverProp OutDriverProps = OutDriverProp.InitialPlusLastName;
            public OutLapProp OutLapProps = OutLapProp.Laps 
                | OutLapProp.LastLapTime 
                | OutLapProp.BestLapTime
                | OutLapProp.DynamicBestLapDeltaToFocusedBest
                | OutLapProp.DynamicLastLapDeltaToFocusedLast;

            public int NumOverallPos { get; set; } = 16;
            public int NumClassPos { get; set; } = 16;
            public int NumOnTrackRelativePos { get; set; } = 5;
            public int NumOverallRelativePos { get; set; } = 5;
            public int NumClassRelativePos { get; set; } = 5;
            public int NumDrivers { get; set; } = 1;
            public int PartialRelativeOverallNumOverallPos { get; set; } = 5;
            public int PartialRelativeOverallNumRelativePos { get; set; } = 5;
            public int PartialRelativeClassNumClassPos { get; set; } = 5;
            public int PartialRelativeClassNumRelativePos { get; set; } = 5;

            public List<Leaderboard> Order { get; set; } = new List<Leaderboard>() { 
                Leaderboard.Overall,
                Leaderboard.Class,
                Leaderboard.PartialRelativeOverall,
                Leaderboard.PartialRelativeClass,
                Leaderboard.RelativeOverall,
                Leaderboard.RelativeClass,
                Leaderboard.RelativeOnTrack
            };

            public int CurrentLeaderboardIdx = 0;
            public Leaderboard CurrentLeaderboard() {
                if (Order.Count > 0) {
                    return Order[CurrentLeaderboardIdx];
                } else {
                    return Leaderboard.None;
                }
            }

            public DynLeaderboardConfig(string name) {
                Name = name;
            }
        }

        internal string PluginDataLocation { get; set; }

        public string AccDataLocation { get; set; }
        public bool Log { get; set; }
        public int BroadcastDataUpdateRateMs { get; set; }
        public List<DynLeaderboardConfig> DynLeaderboardConfigs { get; set; }
        public OutGeneralProp OutGeneralProps = OutGeneralProp.None;

        public Dictionary<CarClass, string> CarClassColors { get; set; }
        public Dictionary<TeamCupCategory, string> TeamCupCategoryColors { get; set; }
        public Dictionary<TeamCupCategory, string> TeamCupCategoryTextColors { get; set; }
        public Dictionary<DriverCategory, string> DriverCategoryColors { get; set; }

        private const string _defPluginsDataLocation = "PluginsData\\KLPlugins\\Leaderboard";
        private static readonly string _defAccDataLocation = "C:\\Users\\" + Environment.UserName + "\\Documents\\Assetto Corsa Competizione";

        public PluginSettings() {
            PluginDataLocation = _defPluginsDataLocation;
            AccDataLocation = _defAccDataLocation;
            Log = false;
            BroadcastDataUpdateRateMs = 1000;
            DynLeaderboardConfigs = new List<DynLeaderboardConfig>();
            CarClassColors = CreateDefCarClassColors();
            TeamCupCategoryColors = CreateDefCupColors();
            TeamCupCategoryTextColors = CreateDefCupTextColors();
            DriverCategoryColors = CreateDefDriverCategoryColors();

        }

        public int GetMaxNumClassPos() {
            int max = 0;
            if (DynLeaderboardConfigs.Count > 0) {
                foreach (var v in DynLeaderboardConfigs) {
                    max = Math.Max(max, v.NumClassPos);
                }
            }
            return max;
        }

        private static Dictionary<CarClass, string> CreateDefCarClassColors() { 
            var carClassColors = new Dictionary<CarClass, string>(8);
            foreach (var c in Enum.GetValues(typeof(CarClass))) {
                var cls = (CarClass)c;
                if (cls == CarClass.Unknown || cls == CarClass.Overall) continue;
                carClassColors.Add(cls, cls.GetACCColor());
            }
            return carClassColors;
        }

        private static Dictionary<TeamCupCategory, string> CreateDefCupColors() {
            var cupColors = new Dictionary<TeamCupCategory, string>(5);
            foreach (var c in Enum.GetValues(typeof(TeamCupCategory))) {
                var cup = (TeamCupCategory)c;
                cupColors.Add(cup, cup.GetACCColor());
            }
            return cupColors;
        }

        private static Dictionary<TeamCupCategory, string> CreateDefCupTextColors() {
            var cupTextColors = new Dictionary<TeamCupCategory, string>(5);
            foreach (var c in Enum.GetValues(typeof(TeamCupCategory))) {
                var cup = (TeamCupCategory)c;
                cupTextColors.Add(cup, cup.GetACCTextColor());
            }
            return cupTextColors;
        }

        private static Dictionary<DriverCategory, string> CreateDefDriverCategoryColors() { 
            var dict = new Dictionary<DriverCategory, string>(4);
            foreach (var c in Enum.GetValues(typeof(DriverCategory))) { 
                var cat = (DriverCategory)c;
                if (cat == DriverCategory.Error) continue;
                dict.Add(cat, cat.GetAccColor());
            }
            return dict;
        }


        public bool SetAccDataLocation(string newLoc) {
            if (!Directory.Exists($"{newLoc}\\Config")) {
                if (Directory.Exists($"{_defAccDataLocation}\\Config")) {
                    AccDataLocation = _defAccDataLocation;
                    LeaderboardPlugin.LogWarn($"Set ACC data location doesn't exist. Using default location '{_defAccDataLocation}'");
                    return false;
                } else {
                    LeaderboardPlugin.LogWarn("Set ACC data location doesn't exist. Please check your configuration file.");
                    return false;
                }
            } else {
                AccDataLocation = newLoc;
                return true;
            }
        }
    }

   

}