﻿using KLPlugins.Leaderboard.Enums;
using MathNet.Numerics.Interpolation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLPlugins.Leaderboard.ksBroadcastingNetwork.Structs
{
    public class LapInterpolator {
        public LinearSpline Interpolator { get; }
        public double LapTime { get; }

        public LapInterpolator(LinearSpline interpolator, double lapTime) {
            Interpolator = interpolator;
            LapTime = lapTime;
        } 
    }

    public class TrackData {
        public string TrackName { get; internal set; }
        public TrackType TrackId { get; internal set; }
        public float TrackMeters { get; internal set; }
        public static CarClassArray<LapInterpolator> LapInterpolators = null;

        /// <summary>
        /// Read default lap data for calculation of gaps.
        /// </summary>
        public static void ReadDefBestLaps() {
            if (LapInterpolators != null) return; // We have already initialized
            LapInterpolators = new CarClassArray<LapInterpolator>(null);

            AddLapInterpolator(CarClass.GT3);
            AddLapInterpolator(CarClass.GT4);
            AddLapInterpolator(CarClass.TCX);
            AddLapInterpolator(CarClass.CUP21);
            AddLapInterpolator(CarClass.CUP17);
            AddLapInterpolator(CarClass.ST15);
            AddLapInterpolator(CarClass.ST21);
            AddLapInterpolator(CarClass.CHL);

            SetReplacements(CarClass.GT3, new CarClass[] { CarClass.CUP21, CarClass.ST21, CarClass.CUP17, CarClass.ST15, CarClass.CHL });
            SetReplacements(CarClass.CUP21, new CarClass[] { CarClass.CUP17, CarClass.ST21, CarClass.ST15, CarClass.CHL, CarClass.GT3 });
            SetReplacements(CarClass.CUP17, new CarClass[] { CarClass.CUP21, CarClass.ST21, CarClass.ST15, CarClass.CHL, CarClass.GT3 });
            SetReplacements(CarClass.ST21, new CarClass[] { CarClass.CUP21, CarClass.CUP17, CarClass.ST15, CarClass.CHL, CarClass.GT3 });
            SetReplacements(CarClass.ST15, new CarClass[] { CarClass.ST21, CarClass.CUP21, CarClass.CUP17,  CarClass.CHL, CarClass.GT3 });
        }

        private static void AddLapInterpolator(CarClass cls) {
            var fname = $"{LeaderboardPlugin.Settings.PluginDataLocation}\\laps\\{(int)Values.TrackData.TrackId}_{cls}.txt";
            if (!File.Exists(fname)) {
                LeaderboardPlugin.LogInfo($"Couldn't build lap interpolator for {cls} because no suitable track data exists.");
                return;
            }

            var pos = new List<double>();
            var time = new List<double>();
            try {
                foreach (var l in File.ReadLines(fname)) {
                    if (l == "") continue;
                    // Data order: splinePositions, laptime in ms, speed in kmh
                    var splits = l.Split(';');
                    double p = float.Parse(splits[0]);
                    var t = double.Parse(splits[1]) / 1000.0;
                    pos.Add(p);
                    time.Add(t);
                }

                LapInterpolators[cls] = new LapInterpolator(LinearSpline.InterpolateSorted(pos.ToArray(), time.ToArray()), time.Last());
                LeaderboardPlugin.LogInfo($"Build lap interpolator for {cls} from file {fname}");
            } catch (Exception ex) {
                LeaderboardPlugin.LogError($"Failed to read {fname} with error: {ex}");
            }
        }

        private static void SetReplacements(CarClass cls, CarClass[] replacements) {
            if (LapInterpolators[cls] == null) {
                foreach (var r in replacements) {
                    if (LapInterpolators[r] != null) {
                        LapInterpolators[cls] = LapInterpolators[r];
                        return;
                    }
                }
                LeaderboardPlugin.LogInfo($"Couldn't find replacement lap interpolator for {cls}.");
            }
        }
    }
}
