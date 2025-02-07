﻿using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;

using KLPlugins.DynLeaderboards.Common;
using KLPlugins.DynLeaderboards.Log;

using SimHub.Plugins.Styles;
using SimHub.Plugins.UI;

namespace KLPlugins.DynLeaderboards.Settings.UI;

public class MessageDialog : SHDialogContentBase {
    public MessageDialog(string titleText, string msg) {
        this.ShowOk = true;

        var sp = new StackPanel();
        this.Content = sp;

        var title = new SHSectionTitle { Text = titleText, Margin = new Thickness(0, 0, 0, 25) };

        sp.Children.Add(title);

        sp.Children.Add(new TextBlock { Text = msg });
    }
}

public class ButtonMenuItem : MenuItem {
    public bool ShowDropDown {
        get => (bool)this.GetValue(ButtonMenuItem.ShowDropDownProperty);
        set => this.SetValue(ButtonMenuItem.ShowDropDownProperty, value);
    }

    public static readonly DependencyProperty ShowDropDownProperty =
        DependencyProperty.RegisterAttached(
            "ShowDropDown",
            typeof(bool),
            typeof(ButtonMenuItem),
            new PropertyMetadata(false)
        );
}

public class SectionTitle : UserControl {
    public string HelpPath {
        get => (string)this.GetValue(SectionTitle.HelpPathProperty);
        set => this.SetValue(SectionTitle.HelpPathProperty, value);
    }

    public static readonly DependencyProperty HelpPathProperty =
        DependencyProperty.RegisterAttached(
            "HelpPath",
            typeof(string),
            typeof(SectionTitle),
            new PropertyMetadata("null")
        );
}

public class DocsHyperlink : Hyperlink {
    public string RelativePath {
        get => (string)this.GetValue(DocsHyperlink.RelativePathProperty);
        set => this.SetValue(DocsHyperlink.RelativePathProperty, value);
    }

    public static readonly DependencyProperty RelativePathProperty =
        DependencyProperty.RegisterAttached(
            "RelativePath",
            typeof(string),
            typeof(DocsHyperlink),
            new PropertyMetadata("")
        );

    public DocsHyperlink() {
        this.RequestNavigate += (_, e) => { System.Diagnostics.Process.Start(e.Uri.ToString()); };
    }
}

public class DocsHelpButton : UserControl {
    public string RelativePath {
        get => (string)this.GetValue(DocsHelpButton.RelativePathProperty);
        set => this.SetValue(DocsHelpButton.RelativePathProperty, value);
    }

    public static readonly DependencyProperty RelativePathProperty =
        DependencyProperty.RegisterAttached(
            "RelativePath",
            typeof(string),
            typeof(DocsHelpButton),
            new PropertyMetadata("")
        );
}

public sealed class DocsPathConverter : IValueConverter {
    #if DOCS_DEBUG
        public const string DOCS_ROOT = "http://127.0.0.1:8000/KLPlugins.DynLeaderboards/";
    #else
    public const string DOCS_ROOT = "https://kaiusl.github.io/KLPlugins.DynLeaderboards/2.0.x/";
    #endif

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) {
        if (value == null) {
            return null;
        }

        try {
            var fullPath = DocsPathConverter.DOCS_ROOT + (string)value;
            return fullPath;
        } catch {
            return null;
        }
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) {
        throw new NotImplementedException();
    }
}

public partial class SettingsControl : UserControl {
    internal DynLeaderboardConfig _CurrentDynLeaderboardSettings { get; private set; }
    internal PluginSettings _Settings { get; }
    internal ClassInfos.Manager _ClassesManager { get; }

    public const double DISABLED_OPTION_OPACITY = 0.333;

    internal Game _Game { get; }

    public SettingsControl(PluginSettings settings, Game game) {
        this.InitializeComponent();
        this.DataContext = this;
        this._Settings = settings;
        this._Game = game;

        this._ClassesManager = new ClassInfos.Manager(this._Settings.Infos.ClassInfos);
        this._ClassesManager.CollectionChanged += (_, e) => {
            if (e.NewItems != null) {
                foreach (OverridableClassInfo.Manager item in e.NewItems) {
                    this.TryAddCarClass(item._Key);
                }
            }
        };

        this.GeneralSettingsTab_SHTabItem.Content = new GeneralSettingsTab(this._Settings);
        this.DynamicLeaderboardsTab_SHTabItem.Content = new DynamicLeaderboardsTab(
            this._Settings,
            this
        );
        this.CarSettingsTab_SHTabItem.Content = new CarSettingsTab(this);
        this.ClassSettingsTab_SHTabItem.Content = new ClassSettingsTab(this, this._ClassesManager);

        this._CurrentDynLeaderboardSettings = this._Settings.DynLeaderboardConfigs[0];

        this.SetAllClassesAndManufacturers();
        this.AddColorsTab();
    }

    internal readonly ObservableCollection<string> _AllClasses = [];
    internal readonly ObservableCollection<string> _AllManufacturers = [];

    /// <summary>
    ///     Tries to add a new class but does nothing if the class already exists.
    /// </summary>
    internal void TryAddCarClass(CarClass cls) {
        var clsStr = cls.AsString();
        if (!this._AllClasses.Contains(clsStr)) {
            this._AllClasses.Add(clsStr);
            this._Settings.Infos.ClassInfos.GetOrAdd(cls);
        }
    }

    internal void TryAddCarManufacturer(string manufacturer) {
        if (!this._AllManufacturers.Contains(manufacturer)) {
            this._AllManufacturers.Add(manufacturer);
        }
    }

    internal async void DoOnConfirmation(Action action) {
        try {
            var dialogWindow = new ConfirmDialog("Are you sure?", "All custom overrides will be lost.");
            var res = await dialogWindow.ShowDialogWindowAsync(this);

            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (res) {
                case System.Windows.Forms.DialogResult.Yes:
                    action();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        } catch (Exception e) {
            Logging.LogError($"Failed to do on confirmation: {e}");
        }
    }

    private void SetAllClassesAndManufacturers() {
        // Go through all cars and check for class colors. 
        // If there are new classes then trying to Values.CarClassColors.Get will add them to the dictionary.
        foreach (var c in this._Settings.Infos.CarInfos) {
            CarClass?[] classes = [c.Value._ClassDontCheckEnabled, c.Value._BaseClass];
            foreach (var cls in classes) {
                if (cls != null) {
                    var info = this._Settings.Infos.ClassInfos.GetOrAdd(cls.Value);
                    if (info._ReplaceWithDontCheckEnabled != null) {
                        var _ = this._Settings.Infos.ClassInfos.GetOrAdd(info._ReplaceWithDontCheckEnabled!.Value);
                    }
                }
            }

            string?[] manufacturers = [c.Value.Manufacturer, c.Value._BaseManufacturer];
            foreach (var manufacturer in manufacturers) {
                if (manufacturer != null) {
                    this.TryAddCarManufacturer(manufacturer);
                }
            }
        }

        foreach (var c in this._Settings.Infos.ClassInfos) {
            this.TryAddCarClass(c.Key);
        }
    }


    private void AddColorsTab() {
        new ColorsTabSection<TeamCupCategory>(
            this,
            "Category",
            this._Settings.Infos.TeamCupCategoryColors,
            this.ColorsTab_TeamCupCategoryColors_Menu,
            this.ColorsTab_TeamCupCategoryColors_Grid
        ).Build(c => c == TeamCupCategory.Default);

        new ColorsTabSection<DriverCategory>(
            this,
            "Category",
            this._Settings.Infos.DriverCategoryColors,
            this.ColorsTab_DriverCategoryColors_Menu,
            this.ColorsTab_DriverCategoryColors_Grid
        ).Build(c => c == DriverCategory.Default);
    }
}