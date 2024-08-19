using SCFR.Controls;
using System.Diagnostics;
using System.Windows;
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
            controlCheck = new List<(GameTypeControl control, GameType type)>()
            {
                (this.liveControl,GameType.Live),
                (this.PTUControl,GameType.PTU),
                (this.EPTUControl,GameType.EPTU)
            };

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



            foreach (var c in controlCheck)
            {
                if (PathTools.GameTypeExists(s.file, c.type))
                {
                    c.control.checkboxEnabled = true;
                    string val = app.GetParam(c.type);
                    if (string.IsNullOrEmpty(val) || val == "1") 
                        c.control.SetCheck(true,false);
                    else
                        c.control.SetCheck(false, false);
                }
                else
                {
                    c.control.SetCheck(false, false);
                    c.control.checkboxEnabled = false;
                    
                }
                

            }
        }

        private void SaveIni()
        {
            foreach (var f in fileSelectorList)
            {
                app.ini.Write(f.type.ToString(), f.control.file, IniSection.Path);
            }
            foreach (var f in controlCheck)
            {
                if (f.control.checkboxEnabled)
                    app.ini.Write(f.type.ToString(), f.control.isChecked?"1":"0", IniSection.Options);
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
