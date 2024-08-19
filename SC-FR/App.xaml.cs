using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Net.Http;
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
        internal bool saveIni = true;
        public App() : base()
        {
            this.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            iniFile = Path.Combine(AppContext.BaseDirectory, "SC-FR.ini");
            ini = new IniFile(iniFile);
        }
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            if (e.Args.Length > 0)
            {
                foreach (string arg in e.Args)
                {
                    /*
                    --launcher / -l < chemin_du_launcher_RSI >
                    --game / -g < chemin_du_dossier_racine_StarCitizen >
                    --env / -e < Live | PTU | EPTU | TECHPREVIEW | HOTFIX | all >
                    */
                }
            }
            else
            {
                if (!File.Exists(iniFile))
                {
                    PathTools.DetectPaths(this.ini);
                }
                LoadIni();
            }

            var configForm = new ConfigForm();
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

        internal void SetParam(string key, string value)
        {
            if(!param.TryAdd(key, value))
                param[key] = value;
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
