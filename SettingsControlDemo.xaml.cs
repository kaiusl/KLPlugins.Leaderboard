﻿using System;
using System.Windows;
using System.Windows.Controls;
using SimHub.Plugins.UI;
using SimHub.Plugins.Styles;
using System.IO;
using System.Windows.Media;
using MdXaml;
using System.Windows.Data;
using MahApps.Metro.Controls;
using Xceed.Wpf.Toolkit;
using KLPlugins.Leaderboard.Enums;
using System.Collections.Generic;
using KLPlugins.Leaderboard.ksBroadcastingNetwork;
using System.Linq;

namespace KLPlugins.Leaderboard
{
    /// <summary>
    /// Logique d'interaction pour SettingsControlDemo.xaml
    /// </summary>
    public partial class SettingsControlDemo : UserControl {
        public LeaderboardPlugin Plugin { get; }
        public PluginSettings Settings { get => LeaderboardPlugin.Settings; }
        public DynLeaderboardSettings CurrentDynLeaderboardSettings { get; private set; }

        private Dictionary<CarClass, ColorPicker> _classColorPickers = new Dictionary<CarClass, ColorPicker>(8);
        private Dictionary<TeamCupCategory, ColorPicker> _cupColorPickers = new Dictionary<TeamCupCategory, ColorPicker>(5);
        private Dictionary<TeamCupCategory, ColorPicker> _cupTextColorPickers = new Dictionary<TeamCupCategory, ColorPicker>(5);
        private Dictionary<DriverCategory, ColorPicker> _driverCategoryColorPickers = new Dictionary<DriverCategory, ColorPicker>(4);

        public SettingsControlDemo() {
            InitializeComponent();
            DataContext = this;
        }

        public SettingsControlDemo(LeaderboardPlugin plugin) : this() {
            this.Plugin = plugin;
            
            if (Settings.DynLeaderboardSettings.Count == 0) {
                Settings.DynLeaderboardSettings.Add(new DynLeaderboardSettings($"Dynamic1"));
                Plugin.AddNewLeaderboard(Settings.DynLeaderboardSettings.Last());
            }
            CurrentDynLeaderboardSettings = Settings.DynLeaderboardSettings[0];

            SelectDynLeaderboard_ComboBox.SelectedIndex = 0;
            foreach (var l in Settings.DynLeaderboardSettings) { 
                SelectDynLeaderboard_ComboBox.Items.Add(l.Name);
            }

            AddDynLeaderboardSettings();
            AddOtherToggles();
            AddColors();

            // Set current values for other settings
            AccDataLocation_TextBox.Text = Settings.AccDataLocation;
            AccDataLocation_TextBox.Background = Brushes.LightGreen;
            Logging_ToggleButton.IsChecked = Settings.Log;
                        
        }


        #region General settings

        private void AddOtherToggles() {
            OtherProperties_StackPanel.Children.Clear();
            OtherProperties_StackPanel.Children.Add(CreatePropertyTogglesDescriptionRow());
            OtherProperties_StackPanel.Children.Add(CreateToggleSeparator());
            foreach (var v in (OutGeneralProp[])Enum.GetValues(typeof(OutGeneralProp))) {
                if (v == OutGeneralProp.None) continue;

                var sp = CreatePropertyToggleRow(
                    v.ToString(),
                    v.ToString(),
                    LeaderboardPlugin.Settings.OutGeneralProps.Includes(v),
                    (sender, e) => LeaderboardPlugin.Settings.OutGeneralProps.Combine(v),
                    (sender, e) => LeaderboardPlugin.Settings.OutGeneralProps.Remove(v),
                    v.ToolTipText()
                );

                OtherProperties_StackPanel.Children.Add(sp);
                OtherProperties_StackPanel.Children.Add(CreateToggleSeparator());
            }
        }

        private void AddColors() {
            AddClassColors();
            AddTeamCupColors();
            AddDriverCategoryColors();
        }

        private void AddClassColors() {

            foreach (var c in Enum.GetValues(typeof(CarClass))) {
                var cls = (CarClass)c;
                if (cls == CarClass.Unknown || cls == CarClass.Overall) continue;

                var sp = new StackPanel();
                sp.Orientation = Orientation.Horizontal;

                var t = new TextBlock();
                t.Text = cls.ToString() + ": ";
                t.Width = 60;

                var cp = new ColorPicker();
                cp.Width = 100;
                cp.Height = 25;
                cp.SelectedColor = (Color)ColorConverter.ConvertFromString(LeaderboardPlugin.Settings.CarClassColors[cls]);
                cp.SelectedColorChanged += (sender, e) => SelectedColorChanged(sender, e, cls, LeaderboardPlugin.Settings.CarClassColors);

                _classColorPickers.Add(cls, cp);

                var btn = new SHButtonPrimary();
                btn.Content = "Reset";
                btn.Click += (sender, e) => ClassColorPickerReset(cls);
                btn.Height = 25;

                sp.Children.Add(t);
                sp.Children.Add(cp);
                sp.Children.Add(btn);

                ClassColors_StackPanel.Children.Add(sp);
            }
        }

        private void AddTeamCupColors() {
            var sp = new StackPanel();
            sp.Orientation = Orientation.Horizontal;

            var t = new TextBlock();
            t.Text = "Background";
            t.Width = 190;
            t.Margin = new Thickness(65, 0, 0, 0);
            var t2 = new TextBlock();
            t2.Text = "Text";
            t2.Width = 100;

            sp.Children.Add(t);
            sp.Children.Add(t2);
            TeamCupColors_StackPanel.Children.Add(sp);


            foreach (var c in Enum.GetValues(typeof(TeamCupCategory))) {
                var cup = (TeamCupCategory)c;

                sp = new StackPanel();
                sp.Orientation = Orientation.Horizontal;

                t = new TextBlock();
                t.Text = cup.ToString() + ": ";
                t.Width = 60;

                var cp1 = new ColorPicker();
                cp1.Width = 100;
                cp1.Height = 25;
                cp1.SelectedColor = (Color)ColorConverter.ConvertFromString(LeaderboardPlugin.Settings.TeamCupCategoryColors[cup]);
                cp1.SelectedColorChanged += (sender, e) => SelectedColorChanged(sender, e, cup, LeaderboardPlugin.Settings.TeamCupCategoryColors);
                _cupColorPickers.Add(cup, cp1);

                var btn1 = new SHButtonPrimary();
                btn1.Content = "Reset";
                btn1.Click += (sender, e) => TeamCupColorPickerReset(cup);
                btn1.Height = 25;


                var cp2 = new ColorPicker();
                cp2.Margin = new Thickness(25, 0, 0, 0);
                cp2.Width = 100;
                cp2.Height = 25;
                cp2.SelectedColor = (Color)ColorConverter.ConvertFromString(LeaderboardPlugin.Settings.TeamCupCategoryTextColors[cup]);
                cp2.SelectedColorChanged += (sender, e) => SelectedColorChanged(sender, e, cup, LeaderboardPlugin.Settings.TeamCupCategoryTextColors);
                _cupTextColorPickers.Add(cup, cp2);

                var btn2 = new SHButtonPrimary();
                btn2.Content = "Reset";
                btn2.Click += (sender, e) => TeamCupTextColorPickerReset(cup);
                btn2.Height = 25;

                sp.Children.Add(t);
                sp.Children.Add(cp1);
                sp.Children.Add(btn1);
                sp.Children.Add(cp2);
                sp.Children.Add(btn2);

                TeamCupColors_StackPanel.Children.Add(sp);
            }
        }

        private void AddDriverCategoryColors() {

            foreach (var c in Enum.GetValues(typeof(DriverCategory))) {
                var cls = (DriverCategory)c;
                if (cls == DriverCategory.Error) continue;

                var sp = new StackPanel();
                sp.Orientation = Orientation.Horizontal;

                var t = new TextBlock();
                t.Text = cls.ToString() + ": ";
                t.Width = 60;

                var cp = new ColorPicker();
                cp.Width = 100;
                cp.Height = 25;
                cp.SelectedColor = (Color)ColorConverter.ConvertFromString(LeaderboardPlugin.Settings.DriverCategoryColors[cls]);
                cp.SelectedColorChanged += (sender, e) => SelectedColorChanged(sender, e, cls, LeaderboardPlugin.Settings.DriverCategoryColors);

                _driverCategoryColorPickers.Add(cls, cp);

                var btn = new SHButtonPrimary();
                btn.Content = "Reset";
                btn.Click += (sender, e) => DriverCategoryColorPickerReset(cls);
                btn.Height = 25;

                sp.Children.Add(t);
                sp.Children.Add(cp);
                sp.Children.Add(btn);

                DriverCategoryColors_StackPanel.Children.Add(sp);
            }
        }

        private void AccDataLocation_TextChanged(object sender, TextChangedEventArgs e) {
            var success = LeaderboardPlugin.Settings.SetAccDataLocation(AccDataLocation_TextBox.Text);
            if (success) {
                AccDataLocation_TextBox.Background = Brushes.LightGreen;
            } else {
                AccDataLocation_TextBox.Background = Brushes.LightPink;
            }

        }

        private void Logging_ToggleButton_Click(object sender, RoutedEventArgs e) {
            LeaderboardPlugin.Settings.Log = !LeaderboardPlugin.Settings.Log;
        }

        private void SelectedColorChanged<T>(object sender, RoutedPropertyChangedEventArgs<Color?> e, T c, Dictionary<T, string> settingsColors) {
            if (e.NewValue != null) {
                var newColor = (Color)e.NewValue;
                settingsColors[c] = newColor.ToString();
            }
        }


        private void ClassColorPickerReset(CarClass cls) {
            LeaderboardPlugin.Settings.CarClassColors[cls] = cls.GetACCColor();
            _classColorPickers[cls].SelectedColor = (Color)ColorConverter.ConvertFromString(cls.GetACCColor());
        }

        private void TeamCupColorPickerReset(TeamCupCategory cup) {
            LeaderboardPlugin.Settings.TeamCupCategoryColors[cup] = cup.GetACCColor();
            _cupColorPickers[cup].SelectedColor = (Color)ColorConverter.ConvertFromString(cup.GetACCColor());
        }

        private void TeamCupTextColorPickerReset(TeamCupCategory cup) {
            LeaderboardPlugin.Settings.TeamCupCategoryTextColors[cup] = cup.GetACCColor();
            _cupTextColorPickers[cup].SelectedColor = (Color)ColorConverter.ConvertFromString(cup.GetACCTextColor());
        }

        private void DriverCategoryColorPickerReset(DriverCategory cls) {
            LeaderboardPlugin.Settings.DriverCategoryColors[cls] = cls.GetAccColor();
            _driverCategoryColorPickers[cls].SelectedColor = (Color)ColorConverter.ConvertFromString(cls.GetAccColor());
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e) {
            System.Diagnostics.Process.Start(e.Uri.ToString());
        }



        #endregion


        #region Dynamic leaderboard


        /// <summary>
        /// Add settings for currently selected dynamic leaderboard.
        /// This is rerun if dynamic leaderboard selection is changed.
        /// This means that everything here should clear old items before adding new items.
        /// </summary>
        private void AddDynLeaderboardSettings() {
            // Techically we don't need to reset all setting ui items but only bindings and values.
            // But it's not critical and this is way simpler.
            AddDynLeaderboardToggles();
            AddNumPositionsSetters();
            AddPropertyToggles();
        }

        /// <summary>
        /// Add all number of position row to the corresponding stack panel.
        /// </summary>
        private void AddNumPositionsSetters() {
            NumPositions_StackPanel.Children.Clear();

            void AddSmallTitle(string name) {
                var t = new SHSmallTitle();
                t.Content = name;
                NumPositions_StackPanel.Children.Add(t);
            }

            AddSmallTitle("Overall leaderboard");

            NumPositions_StackPanel.Children.Add(
                CreateNumRow(
                    "Overall: ",
                    "Set number of overall positions exposed as properties. " +
                    "Note that this must be larger than all the cars as otherwise " +
                    "in class and relative positions may be missing some cars.",
                    nameof(DynLeaderboardSettings.NumOverallPos),
                    0,
                    100,
                    1
                )
            );


            AddSmallTitle("Relative leaderboards");

            NumPositions_StackPanel.Children.Add(
                CreateNumRow(
                    "Overall: ",
                    "Set number of overall relative positions exposed from the focused car in one direction." +
                    " That is if it's set to 5, we show 5 cars ahead and 5 behind.",
                    nameof(DynLeaderboardSettings.NumOverallRelativePos),
                    0,
                    50,
                    1
                )
            );
            NumPositions_StackPanel.Children.Add(CreateToggleSeparator());
            NumPositions_StackPanel.Children.Add(
                CreateNumRow(
                    "Class: ",
                    "Set number of class relative positions exposed from the focused car in one direction." +
                    " That is if it's set to 5, we show 5 cars ahead and 5 behind.",
                    nameof(DynLeaderboardSettings.NumClassRelativePos),
                    0,
                    50,
                    1
                )
            );
            NumPositions_StackPanel.Children.Add(CreateToggleSeparator());
            NumPositions_StackPanel.Children.Add(
                CreateNumRow(
                    "On track: ",
                    "Set number of on track relative positions exposed from the focused car in one direction. " +
                    "That is if it's set to 5, we show 5 cars ahead and 5 behind.",
                    nameof(DynLeaderboardSettings.NumOnTrackRelativePos),
                    0,
                    50,
                    1
                )
            );

            AddSmallTitle("Partial relative leaderboards");
            NumPositions_StackPanel.Children.Add(
               CreateNumRow(
                   "Overall - top positions: ",
                   "Set number of overall positions exposed for partial relative overall leaderboard.",
                   nameof(DynLeaderboardSettings.PartialRelativeOverallNumOverallPos),
                   0,
                   100,
                   1
                )
            );
            NumPositions_StackPanel.Children.Add(CreateToggleSeparator());
            NumPositions_StackPanel.Children.Add(
               CreateNumRow(
                   "Overall - relative positions: ",
                   "Set number of relative positions exposed for partial relative overall " +
                   "leaderboard from the focused car in one direction. That is if it's set to 5, " +
                   "we show 5 cars ahead and 5 behind.",
                   nameof(DynLeaderboardSettings.PartialRelativeOverallNumRelativePos),
                   0,
                   50,
                   1
                )
            );
            NumPositions_StackPanel.Children.Add(CreateToggleSeparator());
            NumPositions_StackPanel.Children.Add(
               CreateNumRow(
                   "Class    - top positions: ",
                   "Set number of class positions exposed for partial relative class leaderboard.",
                   nameof(DynLeaderboardSettings.PartialRelativeClassNumClassPos),
                   0,
                   100,
                   1
                )
            );
            NumPositions_StackPanel.Children.Add(CreateToggleSeparator());
            NumPositions_StackPanel.Children.Add(
               CreateNumRow(
                   "Class    - relative positions: ",
                   "Set number of relative positions exposed for partial relative class " +
                   "leaderboard from the focused car in one direction. " +
                   "That is if it's set to 5, we show 5 cars ahead and 5 behind.",
                   nameof(DynLeaderboardSettings.PartialRelativeOverallNumRelativePos),
                   0,
                   50,
                   1
                )
            );

            AddSmallTitle("Drivers");

            NumPositions_StackPanel.Children.Add(
               CreateNumRow(
                   "Number of drivers",
                   "Set number of drivers shown per car. If set to 1 shown only current driver.",
                   nameof(DynLeaderboardSettings.NumDrivers),
                   0,
                   10,
                   1
                )
            );



        }

        /// <summary>
        /// Creates a row to set number of positions
        /// </summary>
        private StackPanel CreateNumRow(
            string name,
            string tooltip,
            string settingsPropertyName,
            int min,
            int max,
            int interval
        ) {
            var sp = new StackPanel();
            sp.Orientation = Orientation.Horizontal;
            sp.ToolTip = tooltip;

            var t = new TextBlock();
            t.HorizontalAlignment = HorizontalAlignment.Left;
            t.VerticalAlignment = VerticalAlignment.Center;
            t.TextWrapping = TextWrapping.Wrap;
            t.Width = 200;
            t.Margin = new Thickness(3, 3, 3, 3);
            t.Text = name;

            var num = new NumericUpDown();
            num.HorizontalAlignment = HorizontalAlignment.Left;
            num.VerticalAlignment = VerticalAlignment.Center;
            num.Height = 25;
            num.Width = 100;
            num.HasDecimals = false;
            num.Minimum = min;
            num.Maximum = max;
            num.Interval = interval;
            num.Margin = new Thickness(3, 3, 3, 3);

            var bind = new Binding(settingsPropertyName);
            bind.Source = CurrentDynLeaderboardSettings;
            bind.Mode = BindingMode.TwoWay;

            num.SetBinding(NumericUpDown.ValueProperty, bind);

            sp.Children.Add(t);
            sp.Children.Add(num);

            return sp;
        }


        private void AddDynLeaderboardToggles() {
            DynLeaderboards_ListView.Items.Clear();
            // Add currently selected leaderboards
            foreach (var l in CurrentDynLeaderboardSettings.Order) {
                var sp = new StackPanel();
                sp.Orientation = Orientation.Horizontal;

                var tb = new SHToggleButton();
                tb.Name = $"{l.ToString()}_toggle_listview";
                tb.IsChecked = true;
                tb.Checked += (a, b) => CreateDynamicLeaderboardList();
                tb.Unchecked += (a, b) => CreateDynamicLeaderboardList();
                //tb.ToolTip = tooltip;

                var t = new TextBlock();
                t.Text = l.ToString();

                sp.Children.Add(tb);
                sp.Children.Add(t);

                DynLeaderboards_ListView.Items.Add(sp);
            }

            // Add all others to the end
            foreach (var l in (Leaderboard[])Enum.GetValues(typeof(Leaderboard))) {
                if (CurrentDynLeaderboardSettings.Order.Contains(l)) continue;
                var sp = new StackPanel();
                sp.Orientation = Orientation.Horizontal;

                var tb = new SHToggleButton();
                tb.Name = $"{l.ToString()}_toggle_listview";
                tb.IsChecked = false;
                tb.Checked += (a, b) => CreateDynamicLeaderboardList();
                tb.Unchecked += (a, b) => CreateDynamicLeaderboardList();
                //tb.ToolTip = tooltip;

                var t = new TextBlock();
                t.Text = l.ToString();

                sp.Children.Add(tb);
                sp.Children.Add(t);

                DynLeaderboards_ListView.Items.Add(sp);
            }
        }

        /// <summary>
        /// Create list of currently selected leaderboards for currenlty selected dynalic leaderboard
        /// </summary>
        private void CreateDynamicLeaderboardList() {
            var selected = SelectDynLeaderboard_ComboBox.SelectedIndex;
            Settings.DynLeaderboardSettings[selected].Order.Clear();
            foreach (var v in DynLeaderboards_ListView.Items) {
                var sp = (StackPanel)v;
                var tb = (SHToggleButton)sp.Children[0];
                var txt = (TextBlock)sp.Children[1];
                if (tb.IsChecked == null || tb.IsChecked == false) continue;

                if (Enum.TryParse(txt.Text, out Leaderboard variant)) {
                    Settings.DynLeaderboardSettings[selected].Order.Add(variant);
                }
            }
        }

        private void AddPropertyToggles() {
            OutCarProps_StackPanel.Children.Add(CreatePropertyTogglesDescriptionRow());
            AddPitToggles();
            AddPosToggles();
            AddGapToggles();
            AddDistanceToggles();
            AddStintToggles();
            AddLapToggles();
            AddCarToggles();
            AddDriverToggles();
        }

        private void AddCarToggles() {
            // Add Car properties
            OutCarProps_StackPanel.Children.Clear();
            OutOtherProps_StackPanel.Children.Clear();

            StackPanel panel = OutCarProps_StackPanel;
            foreach (var v in (OutCarProp[])Enum.GetValues(typeof(OutCarProp))) {
                if (v == OutCarProp.None) continue;

                if (v == OutCarProp.IsFinished) panel = OutOtherProps_StackPanel;

                StackPanel sp = CreatePropertyToggleRow(
                       v.ToString(),
                       v.ToPropName(),
                       CurrentDynLeaderboardSettings.OutCarProps.Includes(v),
                       (sender, e) => CurrentDynLeaderboardSettings.OutCarProps.Combine(v),
                       (sender, e) => CurrentDynLeaderboardSettings.OutCarProps.Remove(v),
                       v.ToolTipText()
                   );

                panel.Children.Add(sp);
                panel.Children.Add(CreateToggleSeparator());
            }
        }

        private void AddLapToggles() {
            // Add Lap Properties
            OutLapProps_StackPanel.Children.Clear();
            foreach (var v in (OutLapProp[])Enum.GetValues(typeof(OutLapProp))) {
                if (v == OutLapProp.None) continue;

                void AddSmallTitle(string name) {
                    var t = new SHSmallTitle();
                    t.Content = name;
                    OutLapProps_StackPanel.Children.Add(t);
                }

                // Group by similarity
                switch (v) {
                    case OutLapProp.BestLapDeltaToOverallBest:
                        AddSmallTitle("Best lap deltas");
                        break;
                    case OutLapProp.LastLapDeltaToOverallBest:
                        AddSmallTitle("Last lap deltas");
                        break;
                    default:
                        break;
                }

                StackPanel sp = CreatePropertyToggleRow(
                       v.ToString(),
                       v.ToPropName(),
                       CurrentDynLeaderboardSettings.OutLapProps.Includes(v),
                       (sender, e) => CurrentDynLeaderboardSettings.OutLapProps.Combine(v),
                       (sender, e) => CurrentDynLeaderboardSettings.OutLapProps.Remove(v),
                       v.ToolTipText()
                   );

                OutLapProps_StackPanel.Children.Add(sp);
                OutLapProps_StackPanel.Children.Add(CreateToggleSeparator());
            }
        }

        private void AddStintToggles() {
            OutStintProps_StackPanel.Children.Clear();
            // Add Stint Properties
            foreach (var v in (OutStintProp[])Enum.GetValues(typeof(OutStintProp))) {
                if (v == OutStintProp.None) continue;

                StackPanel sp = CreatePropertyToggleRow(
                       v.ToString(),
                       v.ToPropName(),
                       CurrentDynLeaderboardSettings.OutStintProps.Includes(v),
                       (sender, e) => CurrentDynLeaderboardSettings.OutStintProps.Combine(v),
                       (sender, e) => CurrentDynLeaderboardSettings.OutStintProps.Remove(v),
                       v.ToolTipText()
                   );

                OutStintProps_StackPanel.Children.Add(sp);
                OutStintProps_StackPanel.Children.Add(CreateToggleSeparator());
            }
        }

        private void AddDistanceToggles() {
            OutDistancesProps_StackPanel.Children.Clear();
            // Add Distance Properties
            foreach (var v in (OutDistanceProp[])Enum.GetValues(typeof(OutDistanceProp))) {
                if (v == OutDistanceProp.None) continue;

                StackPanel sp = CreatePropertyToggleRow(
                       v.ToString(),
                       v.ToPropName(),
                       CurrentDynLeaderboardSettings.OutDistanceProps.Includes(v),
                       (sender, e) => CurrentDynLeaderboardSettings.OutDistanceProps.Combine(v),
                       (sender, e) => CurrentDynLeaderboardSettings.OutDistanceProps.Remove(v),
                       v.ToolTipText()
                   );

                OutDistancesProps_StackPanel.Children.Add(sp);
                OutDistancesProps_StackPanel.Children.Add(CreateToggleSeparator());
            }
        }

        private void AddGapToggles() {
            OutGapsProps_StackPanel.Children.Clear();
            // Add Gap Properties
            foreach (var v in (OutGapProp[])Enum.GetValues(typeof(OutGapProp))) {
                if (v == OutGapProp.None) continue;

                StackPanel sp = CreatePropertyToggleRow(
                       v.ToString(),
                       v.ToPropName(),
                       CurrentDynLeaderboardSettings.OutGapProps.Includes(v),
                       (sender, e) => CurrentDynLeaderboardSettings.OutGapProps.Combine(v),
                       (sender, e) => CurrentDynLeaderboardSettings.OutGapProps.Remove(v),
                       v.ToolTipText()
                   );

                OutGapsProps_StackPanel.Children.Add(sp);
                OutGapsProps_StackPanel.Children.Add(CreateToggleSeparator());
            }
        }

        private void AddPosToggles() {
            OutPosProps_StackPanel.Children.Clear();
            // Add Pos Properties
            foreach (var v in (OutPosProp[])Enum.GetValues(typeof(OutPosProp))) {
                if (v == OutPosProp.None) continue;

                StackPanel sp = CreatePropertyToggleRow(
                       v.ToString(),
                       v.ToPropName(),
                       CurrentDynLeaderboardSettings.OutPosProps.Includes(v),
                       (sender, e) => CurrentDynLeaderboardSettings.OutPosProps.Combine(v),
                       (sender, e) => CurrentDynLeaderboardSettings.OutPosProps.Remove(v),
                       v.ToolTipText()
                   );

                OutPosProps_StackPanel.Children.Add(sp);
                OutPosProps_StackPanel.Children.Add(CreateToggleSeparator());
            }
        }

        private void AddPitToggles() {
            OutPitProps_StackPanel.Children.Clear();
            // Add Pit Properties
            foreach (var v in (OutPitProp[])Enum.GetValues(typeof(OutPitProp))) {
                if (v == OutPitProp.None) continue;

                StackPanel sp = CreatePropertyToggleRow(
                       v.ToString(),
                       v.ToPropName(),
                       CurrentDynLeaderboardSettings.OutPitProps.Includes(v),
                       (sender, e) => CurrentDynLeaderboardSettings.OutPitProps.Combine(v),
                       (sender, e) => CurrentDynLeaderboardSettings.OutPitProps.Remove(v),
                       v.ToolTipText()
                   );

                OutPitProps_StackPanel.Children.Add(sp);
                OutPitProps_StackPanel.Children.Add(CreateToggleSeparator());
            }
        }

        private void AddDriverToggles() {
            ExposedDriverProperties_StackPanel.Children.Clear();
            ExposedDriverProperties_StackPanel.Children.Add(CreatePropertyTogglesDescriptionRow());
            foreach (var v in (OutDriverProp[])Enum.GetValues(typeof(OutDriverProp))) {
                if (v == OutDriverProp.None) continue;

                if (v == OutDriverProp.FirstName) {
                    var stitle = new SHSmallTitle();
                    stitle.Content = "Names";
                    ExposedDriverProperties_StackPanel.Children.Add(stitle);
                } else if (v == OutDriverProp.Nationality) {
                    var stitle = new SHSmallTitle();
                    stitle.Content = "Other";
                    ExposedDriverProperties_StackPanel.Children.Add(stitle);
                }


                var sp = CreatePropertyToggleRow(
                    v.ToString(),
                    v.ToString(),
                    CurrentDynLeaderboardSettings.OutDriverProps.Includes(v),
                    (sender, e) => CurrentDynLeaderboardSettings.OutDriverProps.Combine(v),
                    (sender, e) => CurrentDynLeaderboardSettings.OutDriverProps.Remove(v),
                    v.ToolTipText()
                );
                ExposedDriverProperties_StackPanel.Children.Add(sp);
                ExposedDriverProperties_StackPanel.Children.Add(CreateToggleSeparator());
            }
        }

        private StackPanel CreatePropertyTogglesDescriptionRow() {
            var sp = new StackPanel();
            sp.Orientation = Orientation.Horizontal;

            var t1 = new TextBlock();
            t1.Width = 70;

            var t = new TextBlock();
            t.Text = "Property name";
            t.Width = 250;
            var t2 = new TextBlock();
            t2.Text = "Description";
            t2.Width = 500;
            t2.TextWrapping = TextWrapping.Wrap;

            sp.Children.Add(t1);
            sp.Children.Add(t);
            sp.Children.Add(t2);
            return sp;
        }

        /// <summary>
        /// Creates row to toggle property
        /// </summary>
        private StackPanel CreatePropertyToggleRow(
            string name, 
            string displayName, 
            bool isChecked, 
            RoutedEventHandler checkHandler, 
            RoutedEventHandler uncheckHandler, 
            string tooltip
        ) {
            var sp = new StackPanel();
            sp.Orientation = Orientation.Horizontal;

            var tb = new SHToggleButton();
            tb.Name = $"{name}_toggle";
            tb.IsChecked = isChecked;
            tb.Checked += checkHandler;
            tb.Unchecked += uncheckHandler;
            tb.ToolTip = tooltip;

            var t = new TextBlock();
            t.Text = displayName;
            t.ToolTip = tooltip;
            t.Width = 250;
            var t2 = new TextBlock();
            t2.Text = tooltip;
            t2.Width = 500;
            t2.TextWrapping = TextWrapping.Wrap;

            sp.Children.Add(tb);
            sp.Children.Add(t);
            sp.Children.Add(t2);

            return sp;
        }

        /// <summary>
        /// Creates separator to insert between property toggle rows.
        /// </summary>
        private Separator CreateToggleSeparator() {
            var s = new Separator();
            s.Background = Brushes.LightGray;
            s.Height = 1;
            s.Margin = new Thickness(25, 0, 25, 0);
            return s;
        }



        // From https://stackoverflow.com/questions/12540457/moving-an-item-up-and-down-in-a-wpf-list-box
        private void DynLeaderboard_ListView_Up(object sender, RoutedEventArgs e) {
            var selectedIndex = DynLeaderboards_ListView.SelectedIndex;

            if (selectedIndex > 0) {
                var itemToMoveUp = DynLeaderboards_ListView.Items[selectedIndex];
                DynLeaderboards_ListView.Items.RemoveAt(selectedIndex);
                DynLeaderboards_ListView.Items.Insert(selectedIndex - 1, itemToMoveUp);
                DynLeaderboards_ListView.SelectedIndex = selectedIndex - 1;
            }

            CreateDynamicLeaderboardList();
        }

        // From https://stackoverflow.com/questions/12540457/moving-an-item-up-and-down-in-a-wpf-list-box
        private void DynLeaderboard_ListView_Down(object sender, RoutedEventArgs e) {
            var selectedIndex = DynLeaderboards_ListView.SelectedIndex;

            if (selectedIndex + 1 < DynLeaderboards_ListView.Items.Count) {
                var itemToMoveDown = DynLeaderboards_ListView.Items[selectedIndex];
                DynLeaderboards_ListView.Items.RemoveAt(selectedIndex);
                DynLeaderboards_ListView.Items.Insert(selectedIndex + 1, itemToMoveDown);
                DynLeaderboards_ListView.SelectedIndex = selectedIndex + 1;
            }

            CreateDynamicLeaderboardList();
        }

        private void AddNewLeaderboard_Button_Click(object sender, RoutedEventArgs e) {
            Settings.DynLeaderboardSettings.Add(new DynLeaderboardSettings($"Dynamic{Settings.DynLeaderboardSettings.Count + 1}"));
            SelectDynLeaderboard_ComboBox.Items.Add($"Dynamic{Settings.DynLeaderboardSettings.Count}");
            Plugin.AddNewLeaderboard(Settings.DynLeaderboardSettings.Last());
        }

        private void RemoveLeaderboard_ButtonClick(object sender, RoutedEventArgs e) {
            if (SelectDynLeaderboard_ComboBox.Items.Count == 1) return;

            int selected = SelectDynLeaderboard_ComboBox.SelectedIndex;
            SelectDynLeaderboard_ComboBox.SelectedIndex = 0;
            DynLeaderboards_ListView.Items.RemoveAt(selected);
            Plugin.RemoveLeaderboardAt(selected);
            Settings.DynLeaderboardSettings.RemoveAt(selected);

        }

        private void SelectDynLeaderboard_ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            int selected = SelectDynLeaderboard_ComboBox.SelectedIndex;
            CurrentDynLeaderboardSettings = Settings.DynLeaderboardSettings[selected];

            AddDynLeaderboardSettings();
        }

        #endregion



    }
}
