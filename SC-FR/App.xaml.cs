using SC_FR_Library;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using static SC_FR_Library.Enumerator;

namespace SCFR
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {

        internal string iniFile;
        internal IniFile ini;
        
        internal Trad trad;

        internal MemoryParam param = new MemoryParam();

        public App() : base()
        {
            trad = new Trad();
            this.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            iniFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),"SC-FR", "SC-FR.ini");
            ini = new IniFile(iniFile);
        }
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            if (!File.Exists(iniFile))
            {
                if (!Directory.Exists(Path.GetDirectoryName(iniFile)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(iniFile));
                }
                WinPathTools.DetectPaths(this.ini);
            }
            LoadIni();


            if (e.Args.Length > 0)
            {
                LoadArgs(e);
            }

            ConfigForm configForm = new ConfigForm();
            if (param.Get(IniOption.AutoMaj) == "1")
            {
                var progress = new ProgressForm("MAJ Automatique de la traduction",true);
                progress.ShowDialog();
                this.LaunchStarCitizen();
                configForm.autoClose = true;
                configForm.autoCloseTiming = 30;
            }
            
            if(param.Get(ParamOption.Silent) != "1")
                configForm.ShowDialog();
            
            this.Shutdown();

        }
        internal void LoadIni()
        {
            string key = string.Empty;
            string value = string.Empty;
            foreach (SCPathType p in Enum.GetValues(typeof(SCPathType)))
            {
                key = p.ToString();
                param.Set(key, ini.Read(key, IniSection.Path));
            }

            foreach (GameType g in Enum.GetValues(typeof(GameType)))
            {
                key = g.ToString();
                value = ini.Read(key, IniSection.Options);
                if(value.Equals("0"))
                    param.Set(key,"0");
                else
                    param.Set(key,"1");

                value = ini.Read(key, IniSection.Versions);
                if(!string.IsNullOrEmpty(value))
                    param.Set(ParamVersion.version,g,value);
            }

            foreach (IniOption o in Enum.GetValues(typeof(IniOption)))
            {
                param.Set(o, ini.Read(o.ToString(), IniSection.Options));
            }

        }
        internal void LoadArgs(StartupEventArgs e)
        {
            const int PARAM = 0;
            const int VALUE = 1;
            StringBuilder sbError = new StringBuilder();

            foreach (string arg in e.Args)
            {
                string[] argData;
                int idxParam = arg.IndexOf('=');

                if (idxParam == -1)
                    argData = new string[] { arg, string.Empty };
                else
                    argData = arg.Split('=', idxParam);

                switch (argData[PARAM].Trim().ToLower())
                {
                    //--launcher / -l < chemin_du_launcher_RSI >
                    case "--launcher":
                    case "-l":
                        string checkLauncher = argData[VALUE].Replace("\"", "");
                        if (!checkLauncher.EndsWith(PathTools.SC_LAUNCHER_EXE))
                            checkLauncher = Path.Combine(checkLauncher, PathTools.SC_LAUNCHER_EXE);

                        if (File.Exists(checkLauncher))
                            param.Set(SCPathType.Launcher, checkLauncher);
                        else
                            sbError.AppendLine($"- Impossible de trouver le Launcher spécifié : {arg}");
                        break;
                    //--game / -g < chemin_du_dossier_racine_StarCitizen >
                    case "--game":
                    case "-g":
                        string checkGameRoot = argData[VALUE].Replace("\"", "");
                        if (PathTools.CheckGamePathRoot(checkGameRoot))
                            param.Set(SCPathType.Games, checkGameRoot);
                        else
                            sbError.AppendLine($"- Pas d'instance StarCitizen : {arg}");
                        break;
                    //--env / -e < Live | PTU | EPTU | TECHPREVIEW | HOTFIX | all >
                    case "--env":
                    case "-e":
                        string env = argData[VALUE].Trim();
                        bool isAll = argData[VALUE].Equals("all", StringComparison.InvariantCultureIgnoreCase);

                        foreach (GameType g in Enum.GetValues(typeof(GameType)))
                        {
                            if (isAll)
                                param.Set(g, "1");
                            else if (env.Equals(g.ToString(), StringComparison.InvariantCultureIgnoreCase))
                                param.Set(g, "1");
                            else
                                param.Set(g, "0");
                        }
                        break;
                    case "--silent":
                    case "-s":
                        param.Set(ParamOption.Silent, "1");
                        break;
                    case "--force":
                    case "-f":
                        param.Set(ParamOption.ForceDownload, "1");
                        break;
                }
            }

            param.Set(IniOption.AutoMaj, "1");

            if (string.IsNullOrEmpty(param.Get(SCPathType.Launcher)))
                sbError.AppendLine($"- le chemin du Launcher n'est pas spécifié");

            if (string.IsNullOrEmpty(param.Get(SCPathType.Games)))
                sbError.AppendLine($"- le chemin des instances de StarCitizen n'est pas spécifié");

            if (sbError.Length > 0)
            {
                System.Windows.MessageBox.Show($"Une ou des anomalies ont empêché d'exécuter le programme : \n{sbError.ToString()}\n\n Merci d'utiliser le configurateur du programme de traduction ou de corriger les paramètres", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Shutdown();
            }
        }

        internal bool NeedToUpgrade()
        {
            if(param.Get(ParamOption.ForceDownload) == "1")
                return true;

            var gameTypes = Enum.GetValues(typeof(GameType));
            foreach (GameType gameType in gameTypes)
            {

                if(!PathTools.GameTypeExists(param.Get(SCPathType.Games),gameType))
                    continue;

                string activeKey = param.Get(gameType);
                if (activeKey == "1")
                {
                    var fileTrad = Path.Combine(new string[] { param.Get(SCPathType.Games), PathTools.GetGamePathSection(gameType), PathTools.DIR_DATA, PathTools.DIR_LOCALIZATION, Trad.DIR_LANGUAGE, Trad.TRAD_FILE_NAME });
                    if (!param.Get(ParamVersion.version, gameType).Equals(trad.version.update) || !File.Exists(fileTrad))
                        return true;
                }
            }
            return false;
        }
        internal void LaunchStarCitizen()
        {
            string launcherPath = param.Get(SCPathType.Launcher);
            Process.Start(launcherPath);
        }

        internal void UpdateTrad(bool silentSuccess)
        {
            trad.LoadVersion();

            var progressForm = new ProgressForm("Mise à jour de la traduction",silentSuccess);
            progressForm.Activate();
            progressForm.ShowDialog();
        }


        /*
        internal ApplyTradReturn ApplyTrad(GameType gameType)
        {

            string path = param.Get(SCPathType.Games);
            string activeKey = param.Get(gameType);

            if (activeKey == "0")
                return ApplyTradReturn.Ingnored;

            if (string.IsNullOrEmpty(path))
                return ApplyTradReturn.EmptyPath;

            if (!trad.isDownloaded)
                return ApplyTradReturn.TradNotValid;


            path = Path.Combine(path, gameType.ToString());

            if (!Directory.Exists(path))
                return ApplyTradReturn.Ingnored;

            if (!File.Exists(Path.Combine(path, PathTools.SC_GAME_BIN, "StarCitizen.exe")))
                return ApplyTradReturn.InvalidPath;

            SetUserCfgLang(path);

            path = Path.Combine(path, DIR_DATA, DIR_LOCALIZATION, DIR_LANGUAGE);

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);


            string tradFile = Path.Combine(path, TRAD_FILE_NAME);

            if (File.Exists(tradFile))
                File.Delete(tradFile);

            try
            {
                var taskWriter = trad.WriteTrad(tradFile);
                while (!taskWriter.IsCompleted)
                {
                    Thread.Sleep(100);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return ApplyTradReturn.ErrorFile;
            }

            return ApplyTradReturn.Success;
        }*/
    }
}
