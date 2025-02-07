﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;

using KLPlugins.DynLeaderboards.Common;
using KLPlugins.DynLeaderboards.Log;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
#if DEBUG
using System.Diagnostics;
#endif

namespace KLPlugins.DynLeaderboards.Settings;

[JsonObject(MemberSerialization.OptIn)]
public sealed class DynLeaderboardConfig {
    private const int _CURRENT_CONFIG_VERSION = 3;
    [JsonProperty("Version", Required = Required.Always)]
    public int Version { get; internal set; } = DynLeaderboardConfig._CURRENT_CONFIG_VERSION;

    private string _name = "";
    [JsonProperty("Name", Required = Required.Always)]
    public string Name {
        get => this._name;
        internal set {
            var arr = value.ToCharArray();
            arr = Array.FindAll(arr, char.IsLetterOrDigit);
            this._name = new string(arr);
        }
    }

    public ReadonlyOutProps<OutPropsBase<OutCarProp>, OutCarProp> OutCarProps => this._OutCarProps.AsReadonly();
    [JsonProperty("OutCarProps")]
    internal OutCarProps _OutCarProps { get; set; } = new(
        OutCarProp.CAR_NUMBER
        | OutCarProp.CAR_CLASS
        | OutCarProp.IS_FINISHED
        | OutCarProp.CAR_CLASS_COLOR
        | OutCarProp.CAR_CLASS_TEXT_COLOR
        | OutCarProp.TEAM_CUP_CATEGORY_COLOR
        | OutCarProp.TEAM_CUP_CATEGORY_TEXT_COLOR
        | OutCarProp.RELATIVE_ON_TRACK_LAP_DIFF
    );

    public ReadonlyOutProps<OutPropsBase<OutPitProp>, OutPitProp> OutPitProps => this._OutPitProps.AsReadonly();
    [JsonProperty("OutPitProps")]
    internal OutPitProps _OutPitProps { get; set; } = new(OutPitProp.IS_IN_PIT_LANE);

    public ReadonlyOutProps<OutPropsBase<OutPosProp>, OutPosProp> OutPosProps => this._OutPosProps.AsReadonly();
    [JsonProperty("OutPosProps")]
    internal OutPosProps _OutPosProps { get; set; } = new(OutPosProp.DYNAMIC_POSITION);

    public ReadonlyOutProps<OutPropsBase<OutGapProp>, OutGapProp> OutGapProps => this._OutGapProps.AsReadonly();
    [JsonProperty("OutGapProps")]
    internal OutGapProps _OutGapProps { get; set; } = new(OutGapProp.DYNAMIC_GAP_TO_FOCUSED);

    public ReadonlyOutProps<OutPropsBase<OutStintProp>, OutStintProp> OutStintProps => this._OutStintProps.AsReadonly();
    [JsonProperty("OutStingProps")]
    internal OutStintProps _OutStintProps { get; set; } = new(OutStintProp.NONE);

    public ReadonlyOutProps<OutPropsBase<OutDriverProp>, OutDriverProp> OutDriverProps =>
        this._OutDriverProps.AsReadonly();
    [JsonProperty("OutDriverProps")]
    internal OutDriverProps _OutDriverProps { get; set; } = new(OutDriverProp.INITIAL_PLUS_LAST_NAME);

    public ReadonlyOutProps<OutPropsBase<OutLapProp>, OutLapProp> OutLapProps => this._OutLapProps.AsReadonly();
    [JsonProperty("OutLapProps")]
    internal OutLapProps _OutLapProps { get; set; } = new(
        OutLapProp.LAPS
        | OutLapProp.LAST_LAP_TIME
        | OutLapProp.BEST_LAP_TIME
        | OutLapProp.DYNAMIC_BEST_LAP_DELTA_TO_FOCUSED_BEST
        | OutLapProp.DYNAMIC_LAST_LAP_DELTA_TO_FOCUSED_LAST
    );

    public int NumOverallPos => this._NumOverallPos.Value;
    [JsonProperty("NumOverallPos")]
    [JsonConverter(typeof(BoxJsonConverter<int>))]
    internal Box<int> _NumOverallPos { get; set; } = new(16);

    public int NumClassPos => this._NumClassPos.Value;
    [JsonProperty("NumClassPos")]
    [JsonConverter(typeof(BoxJsonConverter<int>))]
    internal Box<int> _NumClassPos { get; set; } = new(16);

    public int NumCupPos => this._NumCupPos.Value;
    [JsonProperty("NumCupPos")]
    [JsonConverter(typeof(BoxJsonConverter<int>))]
    internal Box<int> _NumCupPos { get; set; } = new(16);

    public int NumOnTrackRelativePos => this._NumOnTrackRelativePos.Value;
    [JsonProperty("NumOnTrackRelativePos")]
    [JsonConverter(typeof(BoxJsonConverter<int>))]
    internal Box<int> _NumOnTrackRelativePos { get; set; } = new(5);

    public int NumOverallRelativePos => this._NumOverallRelativePos.Value;
    [JsonProperty("NumOverallRelativePos")]
    [JsonConverter(typeof(BoxJsonConverter<int>))]
    internal Box<int> _NumOverallRelativePos { get; set; } = new(5);

    public int NumClassRelativePos => this._NumClassRelativePos.Value;
    [JsonProperty("NumClassRelativePos")]
    [JsonConverter(typeof(BoxJsonConverter<int>))]
    internal Box<int> _NumClassRelativePos { get; set; } = new(5);

    public int NumCupRelativePos => this._NumCupRelativePos.Value;
    [JsonProperty("NumCupRelativePos")]
    [JsonConverter(typeof(BoxJsonConverter<int>))]
    internal Box<int> _NumCupRelativePos { get; set; } = new(5);

    public int NumDrivers => this._NumDrivers.Value;
    [JsonProperty("NumDrivers")]
    [JsonConverter(typeof(BoxJsonConverter<int>))]
    internal Box<int> _NumDrivers { get; set; } = new(1);

    public int PartialRelativeOverallNumOverallPos => this._PartialRelativeOverallNumOverallPos.Value;
    [JsonProperty("PartialRelativeOverallNumOverallPos")]
    [JsonConverter(typeof(BoxJsonConverter<int>))]
    internal Box<int> _PartialRelativeOverallNumOverallPos { get; set; } = new(5);

    public int PartialRelativeOverallNumRelativePos => this._PartialRelativeOverallNumRelativePos.Value;
    [JsonProperty("PartialRelativeOverallNumRelativePos")]
    [JsonConverter(typeof(BoxJsonConverter<int>))]
    internal Box<int> _PartialRelativeOverallNumRelativePos { get; set; } = new(5);

    public int PartialRelativeClassNumClassPos => this._PartialRelativeClassNumClassPos.Value;
    [JsonProperty("PartialRelativeClassNumClassPos")]
    [JsonConverter(typeof(BoxJsonConverter<int>))]
    internal Box<int> _PartialRelativeClassNumClassPos { get; set; } = new(5);

    public int PartialRelativeClassNumRelativePos => this._PartialRelativeClassNumRelativePos.Value;
    [JsonProperty("PartialRelativeClassNumRelativePos")]
    [JsonConverter(typeof(BoxJsonConverter<int>))]
    internal Box<int> _PartialRelativeClassNumRelativePos { get; set; } = new(5);

    public int PartialRelativeCupNumCupPos => this._PartialRelativeCupNumCupPos.Value;
    [JsonProperty("PartialRelativeCupNumCupPos")]
    [JsonConverter(typeof(BoxJsonConverter<int>))]
    internal Box<int> _PartialRelativeCupNumCupPos { get; set; } = new(5);

    public int PartialRelativeCupNumRelativePos => this._PartialRelativeCupNumRelativePos.Value;
    [JsonProperty("PartialRelativeCupNumRelativePos")]
    [JsonConverter(typeof(BoxJsonConverter<int>))]
    internal Box<int> _PartialRelativeCupNumRelativePos { get; set; } = new(5);

    public ReadOnlyCollection<LeaderboardConfig> Order { get; private set; }
    [JsonProperty("Order")]
    internal List<LeaderboardConfig> _Order {
        get => this._order;
        set {
            this._order = value;
            this.Order = this._order.AsReadOnly();
        }
    }
    private List<LeaderboardConfig> _order;


    private int _currentLeaderboardIdx = 0;
    [JsonProperty("CurrentLeaderboardIdx")]
    public int CurrentLeaderboardIdx {
        get => this._currentLeaderboardIdx;
        set {
            this._currentLeaderboardIdx = value > -1 && value < this._Order.Count ? value : 0;
            var currentLeaderboard = this.CurrentLeaderboard();
            this.CurrentLeaderboardDisplayName = currentLeaderboard.Kind.ToDisplayString();
            this.CurrentLeaderboardCompactName = currentLeaderboard.Kind.ToCompactString();
        }
    }

    [JsonProperty("IsEnabled")]
    public bool IsEnabled { get; internal set; } = true;
    public string NextLeaderboardActionName { get; }
    public string PreviousLeaderboardActionName { get; }
    public string CurrentLeaderboardDisplayName { get; private set; }
    public string CurrentLeaderboardCompactName { get; private set; }

    private delegate JObject Migration(JObject o);

    public LeaderboardConfig CurrentLeaderboard() {
        return this._Order.ElementAt(this.CurrentLeaderboardIdx);
    }

    [JsonConstructor]
    internal DynLeaderboardConfig(string name, List<LeaderboardConfig> order, int currentLeaderboardIdx) {
        this.Name = name;
        this._order = order;
        this.Order = this._order.AsReadOnly();
        this.NextLeaderboardActionName = $"{this.Name}.NextLeaderboard";
        this.PreviousLeaderboardActionName = $"{this.Name}.PreviousLeaderboard";
        // this.CurrentLeaderboardIdx.set will set these too
        this.CurrentLeaderboardDisplayName = null!;
        this.CurrentLeaderboardCompactName = null!;
        this.CurrentLeaderboardIdx = currentLeaderboardIdx;
    }

    internal DynLeaderboardConfig(string name) {
        this.Name = name;
        this.NextLeaderboardActionName = $"{this.Name}.NextLeaderboard";
        this.PreviousLeaderboardActionName = $"{this.Name}.PreviousLeaderboard";
        // Don't set order in property definition, deserializing from json will append to it!
        this._order = [
            new LeaderboardConfig(LeaderboardKind.OVERALL, isEnabled: true),
            new LeaderboardConfig(LeaderboardKind.CLASS, true, true, isEnabled: true),
            new LeaderboardConfig(LeaderboardKind.CUP, true, true),
            new LeaderboardConfig(LeaderboardKind.PARTIAL_RELATIVE_OVERALL, isEnabled: true),
            new LeaderboardConfig(LeaderboardKind.PARTIAL_RELATIVE_CLASS, true, true, isEnabled: true),
            new LeaderboardConfig(LeaderboardKind.PARTIAL_RELATIVE_CUP, true, true),
            new LeaderboardConfig(LeaderboardKind.RELATIVE_OVERALL, isEnabled: true),
            new LeaderboardConfig(LeaderboardKind.RELATIVE_CLASS, true, true, isEnabled: true),
            new LeaderboardConfig(LeaderboardKind.RELATIVE_CUP, true, true),
            new LeaderboardConfig(LeaderboardKind.RELATIVE_ON_TRACK, isEnabled: true),
            new LeaderboardConfig(LeaderboardKind.RELATIVE_ON_TRACK_WO_PIT),
        ];
        this.Order = this._order.AsReadOnly();
        this.CurrentLeaderboardDisplayName = this._Order[this._currentLeaderboardIdx].Kind.ToDisplayString();
        this.CurrentLeaderboardCompactName = this._Order[this._currentLeaderboardIdx].Kind.ToCompactString();
    }

    internal DynLeaderboardConfig DeepClone() {
        return JsonConvert.DeserializeObject<DynLeaderboardConfig>(JsonConvert.SerializeObject(this))!;
    }

    internal void Rename(string newName) {
        var configFileName = PluginPaths.DynLeaderboardConfigFilePath(this.Name);
        if (File.Exists(configFileName)) {
            File.Move(configFileName, PluginPaths.DynLeaderboardConfigFilePath(newName));
        }

        for (var i = 5; i > -1; i--) {
            var currentBackupName = PluginPaths.DynLeaderboardConfigBackupFilePath(this.Name, i + 1);
            if (File.Exists(currentBackupName)) {
                File.Move(
                    currentBackupName,
                    PluginPaths.DynLeaderboardConfigBackupFilePath(newName, i + 1)
                );
            }
        }

        this.Name = newName;
    }

    private int? _maxPositions = null;

    public int MaxPositions() {
        if (this._maxPositions == null) {
            var numPos = new[] {
                this._NumOverallPos.Value,
                this._NumClassPos.Value,
                this._NumCupPos.Value,
                this._NumOverallRelativePos.Value * 2 + 1,
                this._NumClassRelativePos.Value * 2 + 1,
                this._NumCupRelativePos.Value * 2 + 1,
                this._NumOnTrackRelativePos.Value * 2 + 1,
                this._PartialRelativeClassNumClassPos.Value + this._PartialRelativeClassNumRelativePos.Value * 2 + 1,
                this._PartialRelativeOverallNumOverallPos.Value
                + this._PartialRelativeOverallNumRelativePos.Value * 2
                + 1,
                this._PartialRelativeCupNumCupPos.Value + this._PartialRelativeCupNumRelativePos.Value * 2 + 1,
            };

            this._maxPositions = numPos.Max();
        }

        return this._maxPositions.Value;
    }

    /// <summary>
    ///     Checks if settings version is changed since last save and migrates to current version if needed.
    ///     Old settings file is rewritten by the new one.
    ///     Should be called before reading the settings from file.
    /// </summary>
    internal static void Migrate() {
        var migrations = DynLeaderboardConfig.CreateMigrationsDict();

        foreach (var filePath in Directory.GetFiles(PluginPaths._LeaderboardConfigsDataDir)) {
            if (!File.Exists(filePath) || !filePath.EndsWith(".json")) {
                continue;
            }

            var savedSettings = JObject.Parse(File.ReadAllText(filePath));

            var version = 0; // If settings doesn't contain version key, it's 0
            if (savedSettings.TryGetValue("Version", out var setting)) {
                version = (int)setting!;
            }

            if (version == DynLeaderboardConfig._CURRENT_CONFIG_VERSION) {
                return;
            }

            var fileName = Path.GetFileName(filePath);
            // Migrate step by step to current version.
            while (version != DynLeaderboardConfig._CURRENT_CONFIG_VERSION) {
                // create backup of old settings before migrating
                using var backupFile = File.CreateText(
                    $"{PluginPaths._LeaderboardConfigsDataBackupDir}\\{fileName}.v{version}.bak"
                );
                var serializer1 = new JsonSerializer { Formatting = Formatting.Indented };
                serializer1.Serialize(backupFile, savedSettings);

                // migrate
                savedSettings = migrations[$"{version}_{version + 1}"](savedSettings);
                version += 1;
            }

            // Save up-to-date setting back to the disk
            using var file = File.CreateText(filePath);
            var serializer = new JsonSerializer { Formatting = Formatting.Indented };
            serializer.Serialize(file, savedSettings);
        }
    }

    /// <summary>
    ///     Creates dictionary of migrations to be called. Key is "old_version_new_version".
    /// </summary>
    /// <returns></returns>
    private static Dictionary<string, Migration> CreateMigrationsDict() {
        var migrations = new Dictionary<string, Migration> {
            ["1_2"] = DynLeaderboardConfig.Mig1To2, ["2_3"] = DynLeaderboardConfig.Mig2To3,
        };

        #if DEBUG
        for (var i = 1; i < DynLeaderboardConfig._CURRENT_CONFIG_VERSION; i++) {
            Debug.Assert(
                migrations.ContainsKey($"{i}_{i + 1}"),
                $"Migration from v{i} to v{i + 1} is not set for DynLeaderboardConfig."
            );
        }
        #endif

        return migrations;
    }

    /// <summary>
    ///     Migration of setting from version 0 to version 1
    /// </summary>
    /// <param name="cfg"></param>
    /// <returns></returns>
    private static JObject Mig1To2(JObject cfg) {
        cfg["Version"] = 2;
        cfg["NumCupPos"] = cfg["NumClassPos"];
        cfg["NumCupRelativePos"] = cfg["NumClassRelativePos"];
        cfg["PartialRelativeCupNumCupPos"] = cfg["PartialRelativeClassNumClassPos"];
        cfg["PartialRelativeCupNumRelativePos"] = cfg["PartialRelativeClassNumRelativePos"];

        Logging.LogInfo($"Migrated DynLeaderboardConfig {cfg["Name"]} from v1 to v2.");

        return cfg;
    }

    /// <summary>
    ///     Migration of setting from version 0 to version 1
    /// </summary>
    private static JObject Mig2To3(JObject cfg) {
        // v2 to v3 changes:
        // - Bump versions!
        // - DynLeaderboardConfigs Order is of type Leaderboard and includes RemoveIfSingleClass/Cup properties,
        //   Although the conversion is automatic since Leaderboard can be converted from LeaderboardKind/int
        // - 

        cfg["Version"] = 3;

        Logging.LogInfo($"Migrated DynLeaderboardConfig {cfg["Name"]} from v2 to v3.");

        return cfg;
    }
}

[TypeConverter(typeof(TyConverter))]
public sealed class LeaderboardConfig {
    [JsonProperty(Required = Required.Always)]
    public LeaderboardKind Kind { get; private set; }

    [JsonProperty("RemoveIfSingleClass")]
    public bool RemoveIfSingleClass { get; internal set; }

    [JsonProperty("RemoveIfSingleCup")]
    public bool RemoveIfSingleCup { get; internal set; }

    [JsonProperty("IsEnabled")]
    public bool IsEnabled { get; internal set; }

    [JsonConstructor]
    internal LeaderboardConfig(
        LeaderboardKind kind,
        bool removeIfSingleClass = false,
        bool removeIfSingleCup = false,
        bool isEnabled = true
    ) {
        this.Kind = kind;
        this.RemoveIfSingleClass = removeIfSingleClass;
        this.RemoveIfSingleCup = removeIfSingleCup;
        this.IsEnabled = isEnabled;
    }

    private class TyConverter : TypeConverter {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) {
            // long because Newtonsoft.Json converts integers to long first, and look for the converter on long
            return sourceType == typeof(long);
        }

        public override object ConvertFrom(
            ITypeDescriptorContext context,
            System.Globalization.CultureInfo culture,
            object? value
        ) {
            if (value is long val) {
                return new LeaderboardConfig(LeaderboardKindExtensions.From(val));
            }

            throw new NotSupportedException($"cannot convert object of type `{value?.GetType()}` to `LeaderboardKind`");
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType) {
            return false;
        }

        // public override object ConvertTo(
        //     ITypeDescriptorContext context,
        //     System.Globalization.CultureInfo culture,
        //     object value,
        //     Type destinationType
        // ) {
        //     return base.ConvertTo(context, culture, value, destinationType);
        // }
    }
}

// IMPORTANT: new leaderboards need to be added to the end in order to not break older configurations
public enum LeaderboardKind {
    NONE,
    OVERALL,
    CLASS,
    RELATIVE_OVERALL,
    RELATIVE_CLASS,
    PARTIAL_RELATIVE_OVERALL,
    PARTIAL_RELATIVE_CLASS,
    RELATIVE_ON_TRACK,
    RELATIVE_ON_TRACK_WO_PIT,
    CUP,
    RELATIVE_CUP,
    PARTIAL_RELATIVE_CUP,
}

internal static class LeaderboardKindExtensions {
    internal const int MAX_VALUE = (int)LeaderboardKind.PARTIAL_RELATIVE_CUP;

    /// <summary>
    ///     Converts an integer to a corresponding <see cref="LeaderboardKind" /> enumeration value.
    /// </summary>
    /// <param name="i">The integer representation of a leaderboard kind.</param>
    /// <returns>The <see cref="LeaderboardKind" /> corresponding to the given integer.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     Thrown when the integer is not within the valid range of leaderboard kinds.
    /// </exception>
    internal static LeaderboardKind From(int i) {
        return LeaderboardKindExtensions.From((long)i);
    }

    /// <summary>
    ///     Converts a long integer to a corresponding <see cref="LeaderboardKind" /> enumeration value.
    /// </summary>
    /// <param name="i">The long integer representation of a leaderboard kind.</param>
    /// <returns>The <see cref="LeaderboardKind" /> corresponding to the given long integer.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     Thrown when the long integer is not within the valid range of leaderboard kinds.
    /// </exception>
    internal static LeaderboardKind From(long i) {
        if (i is >= 0 and <= LeaderboardKindExtensions.MAX_VALUE) {
            return (LeaderboardKind)i;
        }

        throw new ArgumentOutOfRangeException(nameof(i), i, null);
    }


    internal static string ToDisplayString(this LeaderboardKind kind) {
        return kind switch {
            LeaderboardKind.NONE => "None",
            LeaderboardKind.OVERALL => "Overall",
            LeaderboardKind.CLASS => "Class",
            LeaderboardKind.RELATIVE_OVERALL => "Relative overall",
            LeaderboardKind.RELATIVE_CLASS => "Relative class",
            LeaderboardKind.PARTIAL_RELATIVE_OVERALL => "Partial relative overall",
            LeaderboardKind.PARTIAL_RELATIVE_CLASS => "Partial relative class",
            LeaderboardKind.RELATIVE_ON_TRACK => "Relative on track",
            LeaderboardKind.RELATIVE_ON_TRACK_WO_PIT => "Relative on track (wo pit)",
            LeaderboardKind.CUP => "Cup",
            LeaderboardKind.RELATIVE_CUP => "Relative cup",
            LeaderboardKind.PARTIAL_RELATIVE_CUP => "Partial relative cup",
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null),
        };
    }

    internal static string ToCompactString(this LeaderboardKind kind) {
        return kind switch {
            LeaderboardKind.NONE => "None",
            LeaderboardKind.OVERALL => "Overall",
            LeaderboardKind.CLASS => "Class",
            LeaderboardKind.RELATIVE_OVERALL => "RelativeOverall",
            LeaderboardKind.RELATIVE_CLASS => "RelativeClass",
            LeaderboardKind.PARTIAL_RELATIVE_OVERALL => "PartialRelativeOverall",
            LeaderboardKind.PARTIAL_RELATIVE_CLASS => "PartialRelativeClass",
            LeaderboardKind.RELATIVE_ON_TRACK => "RelativeOnTrack",
            LeaderboardKind.RELATIVE_ON_TRACK_WO_PIT => "RelativeOnTrackWoPit",
            LeaderboardKind.CUP => "Cup",
            LeaderboardKind.RELATIVE_CUP => "RelativeCup",
            LeaderboardKind.PARTIAL_RELATIVE_CUP => "PartialRelativeCup",
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null),
        };
    }

    internal static string Tooltip(this LeaderboardKind l) {
        return l switch {
            LeaderboardKind.OVERALL => "`N` top overall positions. `N` can be set below.",
            LeaderboardKind.CLASS => "`N` top class positions. `N` can be set below.",
            LeaderboardKind.CUP => "`N` top class and cup positions. `N` can be set below.",
            LeaderboardKind.RELATIVE_OVERALL =>
                "`2N + 1` relative positions to the focused car in overall order. `N` can be set below.",
            LeaderboardKind.RELATIVE_CLASS =>
                "`2N + 1` relative positions to the focused car in focused car's class order. `N` can be set below.",
            LeaderboardKind.RELATIVE_CUP =>
                "`2N + 1` relative positions to the focused car in focused car's class and cup order. `N` can be set below.",
            LeaderboardKind.RELATIVE_ON_TRACK =>
                "`2N + 1` relative positions to the focused car on track. `N` can be set below.",
            LeaderboardKind.RELATIVE_ON_TRACK_WO_PIT =>
                "`2N + 1` relative positions to the focused car on track excluding the cars in the pit lane which are not on the same lap as the focused car. `N` can be set below.",
            LeaderboardKind.PARTIAL_RELATIVE_OVERALL =>
                "`N` top positions and `2M + 1` relative positions in overall order. If the focused car is inside the first `N + M + 1` positions the order will be just as the overall leaderboard. `N` and `M` can be set below.",
            LeaderboardKind.PARTIAL_RELATIVE_CLASS =>
                "`N` top positions and `2M + 1` relative positions in focused car's class order. If the focused car is inside the first `N + M + 1` positions the order will be just as the class leaderboard. `N` and `M` can be set below.",
            LeaderboardKind.PARTIAL_RELATIVE_CUP =>
                "`N` top positions and `2M + 1` relative positions in focused car's class and cup order. If the focused car is inside the first `N + M + 1` positions the order will be just as the cup leaderboard. `N` and `M` can be set below.",
            _ => "Unknown",
        };
    }
}