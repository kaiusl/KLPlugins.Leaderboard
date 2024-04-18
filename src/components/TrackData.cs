﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

using GameReaderCommon;

using KLPlugins.DynLeaderboards.Car;
using KLPlugins.DynLeaderboards.Helpers;
using KLPlugins.DynLeaderboards.Settings;

using MathNet.Numerics.Interpolation;

using Newtonsoft.Json;
namespace KLPlugins.DynLeaderboards.Track {
    internal class LapInterpolator {
        internal TimeSpan LapTime { get; }
        private LinearSpline _interpolator { get; }

        internal LapInterpolator(LinearSpline interpolator, TimeSpan lapTime) {
            this._interpolator = interpolator;
            this.LapTime = lapTime;
        }

        internal TimeSpan Interpolate(double splinePos) {
            return TimeSpan.FromSeconds(this._interpolator.Interpolate(splinePos));
        }
    }

    public class TrackData {
        public string PrettyName { get; }
        public string Id { get; }
        public double LengthMeters { get; private set; }
        public double SplinePosOffset { get; }
        internal Dictionary<CarClass, LapInterpolator> LapInterpolators = [];
        private static Dictionary<string, double>? _splinePosOffsets = null;

        internal TrackData(GameData data) {
            this.PrettyName = data.NewData.TrackName;
            this.Id = data.NewData.TrackId;
            this.LengthMeters = data.NewData.TrackLength;
            this.SplinePosOffset = _splinePosOffsets?.GetValueOr(this.Id, 0.0) ?? 0.0;

            this.CreateInterpolators();
        }

        internal void SetLength(GameData data) {
            this.LengthMeters = data.NewData.TrackLength;
        }

        internal static void OnPluginInit(string gameName) {
            _splinePosOffsets = ReadSplinePosOffsets(gameName);
        }

        internal void OnLapFinished(CarClass cls, List<(double, TimeSpan)> lapData) {
            if (lapData.Count == 0) {
                return;
            }

            var newLapTime = lapData.Last().Item2;
            var newLapLastPos = lapData.Last().Item1;

            var current = this.LapInterpolators.GetValueOr(cls, null);
            if (current == null || current.Interpolate(newLapLastPos) > newLapTime) {
                // Save new lap

                var sw = new Stopwatch();
                sw.Start();

                var txt = "";
                foreach (var (splinePos, lapTime) in lapData) {
                    txt += splinePos + ";" + lapTime.TotalSeconds + "\n";
                }
                var path = $"{PluginSettings.PluginDataDir}\\{DynLeaderboardsPlugin.Game.Name}\\laps_data\\{this.Id}_{cls}.txt";

                var dirPath = Path.GetDirectoryName(path);
                if (!Directory.Exists(dirPath)) {
                    Directory.CreateDirectory(dirPath);
                }

                // Create backups of current files
                if (File.Exists($"{path}.10.bak")) {
                    File.Delete($"{path}.10.bak");
                }

                if (File.Exists(path)) {
                    for (var i = 9; i > 0; i--) {
                        var bakpath = $"{path}.{i}.bak";
                        if (File.Exists(bakpath)) {
                            File.Move(bakpath, $"{path}.{i + 1}.bak");
                        }
                    }

                    File.Move(path, $"{path}.1.bak");
                }

                File.WriteAllText(path, txt);

                this.AddLapInterpolator(path, cls);

                sw.Stop();

                DynLeaderboardsPlugin.LogInfo($"Saved new best lap for {cls}: {newLapTime} (to {path}). Took {sw.ElapsedMilliseconds}ms");
            }
        }

        private static Dictionary<string, double>? ReadSplinePosOffsets(string gameName) {
            var path = $"{PluginSettings.PluginDataDir}\\{gameName}\\SplinePosOffsets.json";
            if (File.Exists(path)) {
                return JsonConvert.DeserializeObject<Dictionary<string, double>>(File.ReadAllText(path));
            } else {
                return null;
            }
        }

        /// <summary>
        /// Read default lap data for calculation of gaps.
        /// </summary>
        private void CreateInterpolators() {
            var lapsDataPath = $"{PluginSettings.PluginDataDir}\\{DynLeaderboardsPlugin.Game.Name}\\laps_data\\";
            if (!Directory.Exists(lapsDataPath)) {
                return;
            }
            foreach (var path in Directory.GetFiles(lapsDataPath)) {
                if (!path.EndsWith(".txt")) continue;

                var fileName = Path.GetFileNameWithoutExtension(path);
                if (fileName.StartsWith(this.Id)) {
                    var carClass = new CarClass(fileName.Substring(this.Id.Length + 1));
                    this.AddLapInterpolator(path, carClass);
                }
            }
        }

        /// Assumes that `this.LapInterpolators != null`
        private void AddLapInterpolator(string fname, CarClass carClass) {
            Debug.Assert(this.LapInterpolators != null, "Expected this.LapInterpolators != null");

            //var fname = $"{DynLeaderboardsPlugin.Settings.PluginDataLocation}\\{DynLeaderboardsPlugin.Game.Name}\\laps_data\\{this.Id}_{carClass}.txt";
            if (!File.Exists(fname)) {
                DynLeaderboardsPlugin.LogInfo($"Couldn't build lap interpolator for {carClass} because no suitable track data exists.");
                return;
            }

            try {
                var data = this.ReadLapInterpolatorData(fname);
                this.LapInterpolators![carClass] = new LapInterpolator(LinearSpline.InterpolateSorted(
                    data.Item1.ToArray(),
                    data.Item2.ToArray()),
                    TimeSpan.FromSeconds(data.Item2.Last()
                ));
                DynLeaderboardsPlugin.LogInfo($"Build lap interpolator for {carClass} from file {fname}");
            } catch (Exception ex) {
                DynLeaderboardsPlugin.LogError($"Failed to read {fname} with error: {ex}");
            }
        }

        private Tuple<List<double>, List<double>> ReadLapInterpolatorData(string fname) {
            // Default lap_data files have 200 data points
            var pos = new List<double>(200);
            var time = new List<double>(200);
            pos.Add(0.0);
            time.Add(0.0);

            var lines = File.ReadAllLines(fname);
            var i = 0;
            // Find first point where spline position is < 0.1.
            // On some tracks there may be an offset and the data starts at 0.9x or something.
            // That is wrong pos to start. We use this.SplinePosOffset to correct it but it's not perfect, 
            // or it may be missing where it's needed.
            for (; i < lines.Length;) {
                var l = lines[i];
                if (l == "") {
                    continue;
                }

                var splits = l.Split(';');
                double p = float.Parse(splits[0]) + this.SplinePosOffset;
                if (p > 1.0) {
                    p -= 1.0;
                }

                if (p < 0.1) {
                    break;
                } else {
                    i++;
                }
            }

            for (; i < lines.Length; i++) {
                var l = lines[i];
                if (l == "") {
                    continue;
                }
                // Data order: splinePositions, lap time in ms, speed in kmh
                var splits = l.Split(';');
                double p = float.Parse(splits[0]) + this.SplinePosOffset;
                var t = double.Parse(splits[1]);

                if (p > 1.0) {
                    p -= 1.0;
                }

                if (p == pos.Last() || t == time.Last()) {
                    // Interpolator expects increasing set of values. 
                    // If two consecutive values are the same, the interpolator can return double.NaN.
                    continue;
                }

                if (p < pos.Last() || p > 1.0) {
                    // pos needs to increasing for the linear interpolator to properly work
                    break;
                }

                pos.Add(p);
                time.Add(t);
            }

            // Extrapolate so that last point is at 1.0
            if (pos.Last() != 1.0) {
                var x0 = pos[pos.Count - 2];
                var x1 = pos.Last();

                var y0 = time[pos.Count - 2];
                var y1 = time.Last();

                pos.Add(1.0);
                var slope = (y1 - y0) / (x1 - x0);
                time.Add(y0 + (1.0 - x0) * slope);
            }

            //DynLeaderboardsPlugin.LogInfo($"Read {pos.Count} points from {fname}. pos={pos.ToJson()}, time={time.ToJson()}");

            return new Tuple<List<double>, List<double>>(pos, time);
        }
    }
}