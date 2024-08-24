using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using static SCFR.Enumerator;

namespace SCFR
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {

        internal const string URL_TRAD = "https://trad.sc.tasul.fr/api/file/fr";
        internal const string TRAD_FILE_NAME = "global.ini";
        internal const string DIR_LANGUAGE = "french_(france)";

        internal const string DIR_DATA = "data";
        internal const string DIR_LOCALIZATION = "Localization";

        internal string iniFile;
        internal IniFile ini;
        internal byte[] tradFileBytes = new byte[0];

        internal Dictionary<string, string> param = new Dictionary<string, string>();

        public App() : base()
        {
            this.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            iniFile = Path.Combine(AppContext.BaseDirectory, "SC-FR.ini");
            ini = new IniFile(iniFile);
        }
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            if (!File.Exists(iniFile))
            {
                PathTools.DetectPaths(this.ini);
            }
            LoadIni();


            if (e.Args.Length > 0)
            {
                LoadArgs(e);
            }

            ConfigForm configForm = new ConfigForm();
            if (GetParam(IniOption.AutoMaj) == "1")
            {
                var progress = new ProgressForm("MAJ Automatique de la traduction",true);
                progress.ShowDialog();
                this.LaunchStarCitizen();
                configForm.autoClose = true;
                configForm.autoCloseTiming = 30;
            }
            
            if(GetParam(ParamOption.Silent) != "1")
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
                SetParam(key, ini.Read(key, IniSection.Path));
            }

            foreach (GameType g in Enum.GetValues(typeof(GameType)))
            {
                key = g.ToString();
                value = ini.Read(key, IniSection.Options);
                if(value.Equals("0"))
                    SetParam(key,"0");
                else
                    SetParam(key,"1");
            }

            foreach (IniOption o in Enum.GetValues(typeof(IniOption)))
            {
                SetParam(o, ini.Read(o.ToString(), IniSection.Options));
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
                            SetParam(SCPathType.Launcher, checkLauncher);
                        else
                            sbError.AppendLine($"- Impossible de trouver le Launcher spécifié : {arg}");
                        break;
                    //--game / -g < chemin_du_dossier_racine_StarCitizen >
                    case "--game":
                    case "-g":
                        string checkGameRoot = argData[VALUE].Replace("\"", "");
                        if (PathTools.CheckGamePathRoot(checkGameRoot))
                            SetParam(SCPathType.Games, checkGameRoot);
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
                                SetParam(g, "1");
                            else if (env.Equals(g.ToString(), StringComparison.InvariantCultureIgnoreCase))
                                SetParam(g, "1");
                            else
                                SetParam(g, "0");
                        }
                        break;
                    case "--silent":
                    case "-s":
                        SetParam(ParamOption.Silent, "1");
                        break;
                }
            }

            SetParam(IniOption.AutoMaj, "1");

            if (string.IsNullOrEmpty(GetParam(SCPathType.Launcher)))
                sbError.AppendLine($"- le chemin du Launcher n'est pas spécifié");

            if (string.IsNullOrEmpty(GetParam(SCPathType.Games)))
                sbError.AppendLine($"- le chemin des instances de StarCitizen n'est pas spécifié");

            if (sbError.Length > 0)
            {
                System.Windows.MessageBox.Show($"Une ou des anomalies ont empêché d'exécuter le programme : \n{sbError.ToString()}\n\n Merci d'utiliser le configurateur du programme de traduction ou de corriger les paramètres", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Shutdown();
            }
        }
        internal string GetParam(string key)
        {
            if (param.TryGetValue(key, out string val))
                return val;
            else
                return string.Empty;
        }
        internal string GetParam(SCPathType type)
        { return GetParam(type.ToString()); }
        internal string GetParam(GameType type)
        { return GetParam(type.ToString()); }
        internal string GetParam(IniOption type)
        { return GetParam(type.ToString()); }
        internal string GetParam(ParamOption type)
        { return GetParam(type.ToString()); }

        internal void SetParam(string key, string value)
        {
            if(!param.TryAdd(key, value))
                param[key] = value;
        }
        internal void SetParam(GameType key, string val)
        { SetParam(key.ToString(), val); }
        internal void SetParam(SCPathType key, string val)
        { SetParam(key.ToString(), val); }
        internal void SetParam(IniOption key, string val)
        { SetParam(key.ToString(), val); }
        internal void SetParam(ParamOption key, string val)
        { SetParam(key.ToString(), val); }

        internal void LaunchStarCitizen()
        {
            string launcherPath = this.GetParam(SCPathType.Launcher);
            Process.Start(launcherPath);
        }

        internal void UpdateTrad(bool silentSuccess)
        {
            var progressForm = new ProgressForm("Mise à jour de la traduction",silentSuccess);
            progressForm.Activate();
            progressForm.ShowDialog();
        }

        internal async Task<byte[]> LoadTrad()
        {
            return Task.Run(() => new HttpClient().GetByteArrayAsync(URL_TRAD)).Result;
        }

        internal void SetUserCfgLang(string path, bool remove = false)
        {
            string userCfgFile = Path.Combine(path, "user.cfg");
            if (File.Exists(userCfgFile))
            {
                string userCfgData = File.ReadAllText(userCfgFile);
                int langIdx = userCfgData.IndexOf("g_language =");
                if (langIdx > -1)
                {
                    if (!userCfgData.Substring(langIdx + "g_language".Length).Replace("=", "").TrimStart().StartsWith("french_(france)") 
                        || remove)
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.AppendLine(userCfgData.Substring(0, langIdx));
                        
                        if(remove == false)
                            sb.AppendLine("g_language = french_(france)\n");

                        if (userCfgData.IndexOf("\n", langIdx) > -1)
                            sb.Append(userCfgData.Substring(userCfgData.IndexOf("\n", langIdx) + 1));

                        File.WriteAllText(userCfgFile, sb.ToString());
                    }
                }
                else if (remove == false)
                    File.AppendAllText(userCfgFile, $@"g_language = french_(france)");
            }
            else if (remove == false)
                File.WriteAllText(userCfgFile, $@"g_language = french_(france)");
        }

        internal ApplyTradReturn ApplyTrad(GameType gameType)
        {

            string path = this.GetParam(SCPathType.Games);
            string activeKey = this.GetParam(gameType);

            if (activeKey == "0")
                return ApplyTradReturn.Ingnored;

            if (string.IsNullOrEmpty(path))
                return ApplyTradReturn.EmptyPath;

            if (tradFileBytes == null)
                return ApplyTradReturn.TradNotValid;

            
            path = Path.Combine( path,gameType.ToString());

            if(!Directory.Exists(path))
                return ApplyTradReturn.Ingnored;

            if (!File.Exists(Path.Combine(path,PathTools.SC_GAME_BIN,"StarCitizen.exe")))
                return ApplyTradReturn.InvalidPath ;

            SetUserCfgLang(path);

            path = Path.Combine(path, DIR_DATA, DIR_LOCALIZATION, DIR_LANGUAGE);

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            

            string tradFile = Path.Combine(path, TRAD_FILE_NAME);

            if (File.Exists(tradFile))
                File.Delete(tradFile);

            try
            {
                using var writer = new FileStream(tradFile, FileMode.Create);
                var taskWriter = writer.BeginWrite(tradFileBytes, 0, tradFileBytes.Length, null, null);

                while (!taskWriter.IsCompleted)
                {
                    Thread.Sleep(100);
                }
                writer.Flush(true);
                writer.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return ApplyTradReturn.ErrorFile;
            }

            return ApplyTradReturn.Success;
        }
    }
}
