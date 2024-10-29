﻿using SCFR.Controls;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Interop;
using static SC_FR_Library.Enumerator;

namespace SCFR
{
    /// <summary>
    /// Logique d'interaction pour Window1.xaml
    /// </summary>
    public partial class ConfigForm : Window
    {
        App app = (App)System.Windows.Application.Current;

        List<(SelectFile control, SCPathType type)> fileSelectorList;
        List<(GameTypeControl control, GameType type)> controlCheck;

        public bool autoClose = false;
        public uint autoCloseTiming;

        System.Timers.Timer timer;

        public ConfigForm()
        {
            this.Loaded += ConfigForm_Loaded;
            this.Activated += ConfigForm_Activated;
            InitializeComponent();
            fileSelectorList = new List<(SelectFile control, SCPathType type)>()
            {
                (scLauncherFileSelect, SCPathType.Launcher),
                (scGamePathSelect, SCPathType.Games)
            };

            scLauncherFileSelect.PathChanged += ScLauncherFileSelect_PathChanged;
            scGamePathSelect.PathChanged += ScGamePathSelect_PathChanged;

            foreach (var f in fileSelectorList)
            {
                f.control.file = app.param.Get(f.type);
            }

            CheckboxAutoLaunch.IsChecked = app.param.Get(IniOption.AutoMaj) == "1";
            

        }

        private void ConfigForm_Activated(object? sender, EventArgs e)
        {
            if (autoClose && timer != null)
            { 
                autoClose = false;
                timer.Stop();
                this.labelAutoClose.Visibility = Visibility.Hidden;
            }
        }

        private void ConfigForm_Loaded(object sender, RoutedEventArgs e)
        {
            if (autoClose)
            {
                labelAutoClose.Content = string.Empty;
                labelAutoClose.Visibility = Visibility.Visible;
                
                timer = new System.Timers.Timer(1000);
                timer.Elapsed += AutoCloseTimer_Elapsed;
                timer.Start();
            }
        }

        private void AutoCloseTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            if (!autoClose)
            {
                labelAutoClose.Visibility = Visibility.Hidden;
                this.timer.Stop();
            }
            autoCloseTiming--;
            Dispatcher.Invoke(() =>
            {
                labelAutoClose.Content = $"{autoCloseTiming} secondes avant fermeture automatique";
            });

            if (autoCloseTiming == 0)
            {
                Dispatcher.Invoke(() =>
                {
                    this.Close();
                });
            }
        }

        private void ScLauncherFileSelect_PathChanged(object? sender, EventArgs e)
        {
            app.param.Set(SCPathType.Launcher,scLauncherFileSelect.textbox.Text);
        }

        private void ScGamePathSelect_PathChanged(object? sender, EventArgs e)
        {
            var s = sender as SelectFile;
            app.param.Set(SCPathType.Games, scGamePathSelect.textbox.Text);

            foreach (GameType g in Enum.GetValues(typeof(GameType)))
            {
                GameTypeControl control = null;
                string controlName = g.ToString() + "Control";
                foreach (GameTypeControl c in GameControlPanel.Children)
                {
                    if (c.Name.Equals(controlName))
                    {
                        control = c;
                        break;
                    }
                }

                if (WinPathTools.GameTypeExists(s.file, g))
                {
                    string val = app.param.Get(g);
                    if (control == null)
                    {
                        control = new GameTypeControl();
                        control.Name = controlName;
                        control.text = g.ToString();
                        GameControlPanel.Children.Add(control);
                    }
                    control.checkboxEnabled = true;
                    if (string.IsNullOrEmpty(val) || val == "1")
                        control.SetCheck(true, false);
                    else
                        control.SetCheck(false, false);
                }
                else if (control != null)
                { 
                    control.checkboxEnabled = false;
                }
            }
        }

        private void SaveIni()
        {

            foreach (var f in fileSelectorList)
            {
                app.ini.Write(f.type.ToString(), f.control.file, IniSection.Path);
            }
            foreach (GameTypeControl f in GameControlPanel.Children)
            {
                if (f.checkboxEnabled )
                    app.ini.Write(f.text, f.isChecked?"1":"0", IniSection.Options);
            }
            foreach (IniOption f in Enum.GetValues(typeof(IniOption)))
            {
                app.ini.Write(f.ToString(), app.param.Get(f), IniSection.Options);
            }
        }

        private void bExeSC_Click(object sender, RoutedEventArgs e)
        {
            SaveIni();
            
            app.LaunchStarCitizen();
            
            this.Close();
        }

        private void bUpdate_Click(object sender, RoutedEventArgs e)
        {
            SaveIni();

            var p = (App)System.Windows.Application.Current;

            p.UpdateTrad(false);
            
            return;
        }

        private void CheckboxAutoLaunch_Click(object sender, RoutedEventArgs e)
        {
            var s = sender as System.Windows.Controls.CheckBox;
            app.param.Set(IniOption.AutoMaj, s.IsChecked == true ? "1" : "0");
            SaveIni();
        }
    }
}