using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using KLPlugins.DynLeaderboards.Car;
using KLPlugins.DynLeaderboards.Common;
using KLPlugins.DynLeaderboards.Log;
using KLPlugins.DynLeaderboards.Settings;

namespace KLPlugins.DynLeaderboards;

public sealed class DynLeaderboard {
    public delegate CarData? GetDynCarDelegate(int i);

    public delegate TimeSpan? DynGapDelegate(int i);

    public delegate TimeSpan? DynLapDeltaDelegate(int i);

    public delegate int? DynPositionDelegate(int i);

    public GetDynCarDelegate GetDynCar { get; private set; } = null!;
    public DynGapDelegate GetDynGapToFocused { get; private set; } = null!;
    public DynGapDelegate GetDynGapToAhead { get; private set; } = null!;
    public DynLapDeltaDelegate GetDynBestLapDeltaToFocusedBest { get; private set; } = null!;
    public DynLapDeltaDelegate GetDynLastLapDeltaToFocusedBest { get; private set; } = null!;
    public DynLapDeltaDelegate GetDynLastLapDeltaToFocusedLast { get; private set; } = null!;

    public DynPositionDelegate GetDynPosition { get; private set; } = null!;
    public DynPositionDelegate GetDynPositionStart { get; private set; } = null!;

    public string Name => this._Config.Name;
    public string CurrentLeaderboardDisplayName => this._Config.CurrentLeaderboardDisplayName;
    public string CurrentLeaderboardCompactName => this._Config.CurrentLeaderboardCompactName;
    public string NextLeaderboardActionNAme => this._Config.NextLeaderboardActionName;
    public string PreviousLeaderboardActionNAme => this._Config.PreviousLeaderboardActionName;
    public int MaxPositions => this._Config.MaxPositions();
    public LeaderboardKind CurrentLeaderboard => this._Config.CurrentLeaderboard().Kind;

    internal DynLeaderboardConfig _Config { get; set; }

    /// <summary>
    ///     List of cars for this dynamic leaderboard in the order they are displayed.
    /// </summary>
    public ReadOnlyCollection<CarData?> Cars { get; }

    private List<CarData?> _cars { get; } = [];

    /// <summary>
    ///     Focused car's index in <c>this.Cars</c>.
    /// </summary>
    public int? FocusedIndex = null;

    internal DynLeaderboard(DynLeaderboardConfig config, Values v) {
        this._Config = config;
        this.SetDynGetters(v);
        this.Cars = this._cars.AsReadOnly();
    }

    #if TIMINGS
    private static readonly Timer _onDataUpdateTimer = Timers.AddOrGetAndRestart("DynLeaderboard.OnDataUpdate");
    private static readonly Timer _getCarTimer = Timers.AddOrGetAndRestart("DynLeaderboard.GetCar");
    #endif

    internal void OnDataUpdate(Values v) {
        #if TIMINGS
        DynLeaderboard._onDataUpdateTimer.Restart();
        #endif
        this.SetCars(v);
        #if TIMINGS
        DynLeaderboard._onDataUpdateTimer.StopAndWriteMicros();
        #endif
    }

    private void SetDynGettersDefault() {
        this.GetDynCar = i => this._cars.ElementAtOrDefault(i);
        this.GetDynGapToFocused = _ => null;
        this.GetDynGapToAhead = _ => null;
        this.GetDynBestLapDeltaToFocusedBest = _ => null;
        this.GetDynLastLapDeltaToFocusedBest = _ => null;
        this.GetDynLastLapDeltaToFocusedLast = _ => null;
        this.GetDynPosition = _ => null;
        this.GetDynPositionStart = _ => null;
    }

    private void SetDynGetters(Values v) {
        switch (this._Config.CurrentLeaderboard().Kind) {
            case LeaderboardKind.OVERALL:
                this.GetDynCar = i => v.OverallOrder.ElementAtOrDefault(i);
                this.GetDynGapToFocused = i => this.GetDynCar(i)?.GapToLeader;
                this.GetDynGapToAhead = i => this.GetDynCar(i)?.GapToAhead;
                this.GetDynBestLapDeltaToFocusedBest = i => this.GetDynCar(i)?.BestLap?.DeltaToLeaderBest;
                this.GetDynLastLapDeltaToFocusedBest = i => this.GetDynCar(i)?.LastLap?.DeltaToLeaderBest;
                this.GetDynLastLapDeltaToFocusedLast = i => this.GetDynCar(i)?.LastLap?.DeltaToLeaderLast;
                this.GetDynPosition = i => this.GetDynCar(i)?.PositionOverall;
                this.GetDynPositionStart = i => this.GetDynCar(i)?.PositionOverallStart;
                break;

            case LeaderboardKind.CLASS:
                this.GetDynCar = i => v.ClassOrder.ElementAtOrDefault(i);
                this.GetDynGapToFocused = i => this.GetDynCar(i)?.GapToClassLeader;
                this.GetDynGapToAhead = i => this.GetDynCar(i)?.GapToAheadInClass;
                this.GetDynBestLapDeltaToFocusedBest = i => this.GetDynCar(i)?.BestLap?.DeltaToClassLeaderBest;
                this.GetDynLastLapDeltaToFocusedBest = i => this.GetDynCar(i)?.LastLap?.DeltaToClassLeaderBest;
                this.GetDynLastLapDeltaToFocusedLast = i => this.GetDynCar(i)?.LastLap?.DeltaToClassLeaderLast;
                this.GetDynPosition = i => this.GetDynCar(i)?.PositionInClass;
                this.GetDynPositionStart = i => this.GetDynCar(i)?.PositionInClassStart;
                break;

            case LeaderboardKind.CUP:
                this.GetDynCar = i => v.CupOrder.ElementAtOrDefault(i);
                this.GetDynGapToFocused = i => this.GetDynCar(i)?.GapToCupLeader;
                this.GetDynGapToAhead = i => this.GetDynCar(i)?.GapToAheadInCup;
                this.GetDynBestLapDeltaToFocusedBest = i => this.GetDynCar(i)?.BestLap?.DeltaToCupLeaderBest;
                this.GetDynLastLapDeltaToFocusedBest = i => this.GetDynCar(i)?.LastLap?.DeltaToCupLeaderBest;
                this.GetDynLastLapDeltaToFocusedLast = i => this.GetDynCar(i)?.LastLap?.DeltaToCupLeaderLast;
                this.GetDynPosition = i => this.GetDynCar(i)?.PositionInCup;
                this.GetDynPositionStart = i => this.GetDynCar(i)?.PositionInCupStart;
                break;

            case LeaderboardKind.RELATIVE_OVERALL:
            case LeaderboardKind.PARTIAL_RELATIVE_OVERALL:
                this.GetDynCar = this.GetCar;
                this.GetDynGapToFocused = i => this.GetCar(i)?.GapToFocusedTotal;
                this.GetDynGapToAhead = i => this.GetCar(i)?.GapToAhead;
                this.GetDynBestLapDeltaToFocusedBest = i => this.GetCar(i)?.BestLap?.DeltaToFocusedBest;
                this.GetDynLastLapDeltaToFocusedBest = i => this.GetCar(i)?.LastLap?.DeltaToFocusedBest;
                this.GetDynLastLapDeltaToFocusedLast = i => this.GetCar(i)?.LastLap?.DeltaToFocusedLast;
                this.GetDynPosition = i => this.GetCar(i)?.PositionOverall;
                this.GetDynPositionStart = i => this.GetCar(i)?.PositionOverallStart;
                break;

            case LeaderboardKind.RELATIVE_CLASS:
            case LeaderboardKind.PARTIAL_RELATIVE_CLASS:
                this.GetDynCar = this.GetCar;
                this.GetDynGapToFocused = i => this.GetCar(i)?.GapToFocusedTotal;
                this.GetDynGapToAhead = i => this.GetCar(i)?.GapToAheadInClass;
                this.GetDynBestLapDeltaToFocusedBest = i => this.GetCar(i)?.BestLap?.DeltaToFocusedBest;
                this.GetDynLastLapDeltaToFocusedBest = i => this.GetCar(i)?.LastLap?.DeltaToFocusedBest;
                this.GetDynLastLapDeltaToFocusedLast = i => this.GetCar(i)?.LastLap?.DeltaToFocusedLast;
                this.GetDynPosition = i => this.GetCar(i)?.PositionInClass;
                this.GetDynPositionStart = i => this.GetCar(i)?.PositionInClassStart;
                break;

            case LeaderboardKind.RELATIVE_CUP:
            case LeaderboardKind.PARTIAL_RELATIVE_CUP:
                this.GetDynCar = this.GetCar;
                this.GetDynGapToFocused = i => this.GetCar(i)?.GapToFocusedTotal;
                this.GetDynGapToAhead = i => this.GetCar(i)?.GapToAheadInCup;
                this.GetDynBestLapDeltaToFocusedBest = i => this.GetCar(i)?.BestLap?.DeltaToFocusedBest;
                this.GetDynLastLapDeltaToFocusedBest = i => this.GetCar(i)?.LastLap?.DeltaToFocusedBest;
                this.GetDynLastLapDeltaToFocusedLast = i => this.GetCar(i)?.LastLap?.DeltaToFocusedLast;
                this.GetDynPosition = i => this.GetCar(i)?.PositionInCup;
                this.GetDynPositionStart = i => this.GetCar(i)?.PositionInCupStart;
                break;

            case LeaderboardKind.RELATIVE_ON_TRACK:
            case LeaderboardKind.RELATIVE_ON_TRACK_WO_PIT:
                this.GetDynCar = this.GetCar;
                this.GetDynGapToFocused = i => this.GetCar(i)?.GapToFocusedOnTrack;
                this.GetDynGapToAhead = i => this.GetCar(i)?.GapToAheadOnTrack;
                this.GetDynBestLapDeltaToFocusedBest = i => this.GetCar(i)?.BestLap?.DeltaToFocusedBest;
                this.GetDynLastLapDeltaToFocusedBest = i => this.GetCar(i)?.LastLap?.DeltaToFocusedBest;
                this.GetDynLastLapDeltaToFocusedLast = i => this.GetCar(i)?.LastLap?.DeltaToFocusedLast;
                this.GetDynPosition = i => this.GetCar(i)?.PositionOverall;
                this.GetDynPositionStart = i => this.GetCar(i)?.PositionOverallStart;
                break;

            case LeaderboardKind.NONE:
            default:
                this.SetDynGettersDefault();
                break;
        }
    }

    private CarData? GetCar(int i) {
        #if TIMINGS
        DynLeaderboard._getCarTimer.Restart();
        #endif
        var res = this._cars.ElementAtOrDefault(i);
        #if TIMINGS
        DynLeaderboard._getCarTimer.StopAndWriteMicros();
        #endif
        return res;
    }

    private void SetCars(Values v) {
        if (v.FocusedCar == null) {
            this.FocusedIndex = null;
            return;
        }

        this._cars.Clear();
        switch (this._Config.CurrentLeaderboard().Kind) {
            case LeaderboardKind.OVERALL:
                this.FocusedIndex = v.FocusedCar.IndexOverall;
                break;
            case LeaderboardKind.CLASS:
                this.FocusedIndex = v.FocusedCar.IndexClass;
                break;
            case LeaderboardKind.CUP:
                this.FocusedIndex = v.FocusedCar.IndexCup;
                break;
            case LeaderboardKind.RELATIVE_OVERALL:
                this.SetCarsRelativeX(
                    numRelPos: this._Config.NumOverallRelativePos,
                    cars: v.OverallOrder,
                    focusedCarIndexInCars: v.FocusedCar.IndexOverall
                );
                break;
            case LeaderboardKind.RELATIVE_CLASS:
                this.SetCarsRelativeX(
                    numRelPos: this._Config.NumClassRelativePos,
                    cars: v.ClassOrder,
                    focusedCarIndexInCars: v.FocusedCar.IndexClass
                );
                break;
            case LeaderboardKind.RELATIVE_CUP:
                this.SetCarsRelativeX(
                    numRelPos: this._Config.NumCupRelativePos,
                    cars: v.CupOrder,
                    focusedCarIndexInCars: v.FocusedCar.IndexCup
                );
                break;
            case LeaderboardKind.PARTIAL_RELATIVE_OVERALL:
                this.SetCarsPartialRelativeX(
                    numTopPos: this._Config.PartialRelativeOverallNumOverallPos,
                    numRelPos: this._Config.PartialRelativeOverallNumRelativePos,
                    cars: v.OverallOrder,
                    focusedCarIndexInCars: v.FocusedCar.IndexOverall
                );
                break;
            case LeaderboardKind.PARTIAL_RELATIVE_CLASS:
                this.SetCarsPartialRelativeX(
                    numTopPos: this._Config.PartialRelativeClassNumClassPos,
                    numRelPos: this._Config.PartialRelativeClassNumRelativePos,
                    cars: v.ClassOrder,
                    focusedCarIndexInCars: v.FocusedCar.IndexClass
                );
                break;
            case LeaderboardKind.PARTIAL_RELATIVE_CUP:
                this.SetCarsPartialRelativeX(
                    numTopPos: this._Config.PartialRelativeCupNumCupPos,
                    numRelPos: this._Config.PartialRelativeCupNumRelativePos,
                    cars: v.CupOrder,
                    focusedCarIndexInCars: v.FocusedCar.IndexCup
                );
                break;
            case LeaderboardKind.RELATIVE_ON_TRACK: {
                var relPos = this._Config.NumOnTrackRelativePos;

                if (v.RelativeOnTrackAheadOrder.Count < relPos) {
                    for (var i = 0; i < relPos - v.RelativeOnTrackAheadOrder.Count; i++) {
                        this._cars.Add(null);
                    }
                }

                foreach (var car in v.RelativeOnTrackAheadOrder.Take(relPos).Reverse()) {
                    this._cars.Add(car);
                }

                this._cars.Add(v.FocusedCar);
                this.FocusedIndex = relPos;

                foreach (var car in v.RelativeOnTrackBehindOrder.Take(relPos)) {
                    this._cars.Add(car);
                }
            }
                break;

            case LeaderboardKind.RELATIVE_ON_TRACK_WO_PIT: {
                var relPos = this._Config.NumOnTrackRelativePos;

                var aheadCars = v.RelativeOnTrackAheadOrder
                    .Where(c => !c.IsInPitLane)
                    .Take(relPos)
                    .Reverse()
                    .ToList();
                var aheadCount = aheadCars.Count;

                if (aheadCount < relPos) {
                    for (var i = 0; i < relPos - aheadCount; i++) {
                        this._cars.Add(null);
                    }
                }

                foreach (var car in aheadCars) {
                    this._cars.Add(car);
                }

                this._cars.Add(v.FocusedCar);
                this.FocusedIndex = relPos;

                var behindCars = v.RelativeOnTrackBehindOrder
                    .Where(c => !c.IsInPitLane)
                    .Take(relPos);
                foreach (var car in behindCars) {
                    this._cars.Add(car);
                }
            }
                break;
            case LeaderboardKind.NONE:
            default:
                break;
        }
    }

    private void SetCarsRelativeX(int numRelPos, ReadOnlyCollection<CarData> cars, int focusedCarIndexInCars) {
        this.FocusedIndex = numRelPos;
        var start = focusedCarIndexInCars - numRelPos;
        var end = start + numRelPos * 2 + 1;

        var i = start;
        for (; i < 0; i++) {
            this._cars.Add(null);
        }

        for (; i < end; i++) {
            this._cars.Add(cars.ElementAtOrDefault(i));
        }
    }

    private void SetCarsPartialRelativeX(
        int numTopPos,
        int numRelPos,
        ReadOnlyCollection<CarData> cars,
        int focusedCarIndexInCars
    ) {
        // Top positions are always added
        for (var i = 0; i < numTopPos; i++) {
            var car = cars.ElementAtOrDefault(i);
            this._cars.Add(car);
            if (car != null && car.IsFocused) {
                this.FocusedIndex = i;
            }
        }

        // Calculate relative part start and end
        var start = focusedCarIndexInCars - numTopPos;
        var end = start + numRelPos * 2 + 1;

        // if start reaches into the top positions, shift it down so it doesn't overlap
        if (start <= numTopPos) {
            var diff = numTopPos - start;
            start += diff;
            end += diff;
        }

        for (var i = start; i < end; i++) {
            var car = cars.ElementAtOrDefault(i);
            this._cars.Add(car);
            if (car != null && car.IsFocused) {
                this.FocusedIndex = this._cars.Count - 1;
            }
        }
    }

    internal void NextLeaderboard(Values values) {
        var isSingleClass = values.NumClassesInSession < 2;
        var isSingleCup = values.NumCupsInSession == values.NumClassesInSession;
        // For loop so that we don't go into an infinite loop if all leaderboards should be excluded
        for (var i = 0; i < this._Config.Order.Count; i++) {
            if (this._Config.CurrentLeaderboardIdx == this._Config.Order.Count - 1) {
                this._Config.CurrentLeaderboardIdx = 0;
            } else {
                this._Config.CurrentLeaderboardIdx++;
            }

            var currentLeaderboard = this._Config.CurrentLeaderboard();
            if (!currentLeaderboard.IsEnabled
                || (isSingleClass && currentLeaderboard.RemoveIfSingleClass)
                || (isSingleCup && currentLeaderboard.RemoveIfSingleCup)
            ) { } else {
                break;
            }
        }

        this.OnLeaderboardChange(values);
    }

    internal void PreviousLeaderboard(Values values) {
        var isSingleClass = values.NumClassesInSession < 2;
        var isSingleCup = values.NumCupsInSession == values.NumClassesInSession;
        // For loop so that we don't go into an infinite loop if all leaderboards should be excluded
        for (var i = 0; i < this._Config.Order.Count; i++) {
            if (this._Config.CurrentLeaderboardIdx == 0) {
                this._Config.CurrentLeaderboardIdx = this._Config.Order.Count - 1;
            } else {
                this._Config.CurrentLeaderboardIdx--;
            }

            var currentLeaderboard = this._Config.CurrentLeaderboard();
            if (!currentLeaderboard.IsEnabled
                || (isSingleClass && currentLeaderboard.RemoveIfSingleClass)
                || (isSingleCup && currentLeaderboard.RemoveIfSingleCup)
            ) { } else {
                break;
            }
        }

        this.OnLeaderboardChange(values);
    }

    private void OnLeaderboardChange(Values v) {
        Logging.LogInfo($"OnLeaderboardChange [{this._Config.Name}]: {this._Config.CurrentLeaderboard().Kind}");
        this.SetDynGetters(v);
    }
}