using SCFR.Controls;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using static SCFR.Enumerator;

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


        public ConfigForm()
        {
            InitializeComponent();

            fileSelectorList = new List<(SelectFile control, SCPathType type)>()
            {
                (scLauncherFileSelect, SCPathType.Launcher),
                (scGamePathSelect, SCPathType.Games)
            };
            /*
            controlCheck = new List<(GameTypeControl control, GameType type)>()
            {
                (this.liveControl,GameType.Live),
                (this.PTUControl,GameType.PTU),
                (this.EPTUControl,GameType.EPTU)
            };
            */
            //fileSelectorList.Add((scLiveFileSelect, Enum.PathType.Games));

            scGamePathSelect.PathChanged += ScGamePathSelect_PathChanged;

            foreach (var f in fileSelectorList)
            {
                f.control.file = app.GetParam(f.type);
            }
        }

        private void ScGamePathSelect_PathChanged(object? sender, EventArgs e)
        {
            var s = sender as SelectFile;

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

                if (PathTools.GameTypeExists(s.file, g))
                {
                    string val = app.GetParam(g);
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

        }

        private void bExeSC_Click(object sender, RoutedEventArgs e)
        {
            SaveIni();

            string launcherPath = app.GetParam(SCPathType.Launcher);
            Process.Start(launcherPath);

            this.Close();
        }

        private void bUpdate_Click(object sender, RoutedEventArgs e)
        {
            SaveIni();

            var p = (App)System.Windows.Application.Current;

            p.UpdateTrad(false);
            
            return;
        }
    }
}
