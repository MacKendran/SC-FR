using System.Net.Http.Json;
using System.Text;
using static SC_FR_Library.Enumerator;

using File = System.IO.File;

namespace SC_FR_Library
{
    public class JsonTradVersion
    {
        public string service { get; set; }
        public string version1 { get; set; }
        public string version2 { get; set; }
        public string lastPush { get; set; }
        public string update { get; set; }
    }

    public class Trad
    {

        public const string TRAD_FILE_NAME = "global.ini";
        public const string DIR_LANGUAGE = "french_(france)";

        internal const string URL_BASE = "https://trad.sc.tasul.fr/";

        internal const string URL_TRAD = URL_BASE + "api/file/fr";
        internal const string URL_TRAD_CUSTOM = URL_BASE + "api/file/custom?ui={0}&uiship={1}&item={2}&mission={3}";
        internal const string URL_VERSION = URL_BASE + "version/versions";

        internal byte[] tradFileBytes = new byte[0];

        public JsonTradVersion version = null;
        public bool? getVersionFailed;


        public bool isDownloaded { get; private set; }

        public Trad()
        { 
            LoadVersion();
        }

        public async void LoadVersion()
        {
            try
            {
                this.version = await new HttpClient().GetFromJsonAsync<JsonTradVersion>(URL_VERSION);
                this.getVersionFailed = false;
            }
            catch(Exception e) 
            {
                this.getVersionFailed = true;
            }
        }

        public async Task<bool> DownloadTradCustom(TradType ui, TradType uiShip, TradType item, TradType mission)
        {
            this.tradFileBytes = new byte[0];
            isDownloaded = false;

            string url = string.Format(URL_TRAD_CUSTOM,(int)ui,(int)uiShip,(int)item,(int)mission);

            var r = await Task.Run(() => new HttpClient().GetByteArrayAsync(url));
            if (r != null)
            {
                this.tradFileBytes = r;
                isDownloaded = true;
                return (true);
            }

            return (false);
        }


        public async Task<bool> DownloadTrad()
        {
            this.tradFileBytes = new byte[0];
            isDownloaded = false;
            
            var r =  await Task.Run(() => new HttpClient().GetByteArrayAsync(URL_TRAD));
            if (r != null)
            {
                this.tradFileBytes = r;
                isDownloaded = true;
                return (true);
            }

            return (false);
        }

        public async Task WriteTrad(string tradFile)
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

        public void SetUserCfgLang(string path, bool remove = false)
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

                        sb.Append("g_language = ");

                        if (remove == false)
                            sb.AppendLine("french_(france)");
                        else
                            sb.AppendLine("english");

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

        public ApplyTradReturn ApplyTrad(GameType gameType, string path)
        {

            if (string.IsNullOrEmpty(path))
                return ApplyTradReturn.EmptyPath;

            if (!this.isDownloaded)
                return ApplyTradReturn.TradNotValid;


            path = Path.Combine(path, PathTools.GetGamePathSection(gameType));

            if (!Directory.Exists(path))
                return ApplyTradReturn.Ingnored;

            if (!File.Exists(Path.Combine(path, PathTools.SC_GAME_BIN, "StarCitizen.exe")))
                return ApplyTradReturn.InvalidPath;

            SetUserCfgLang(path);

            path = Path.Combine(path, PathTools.DIR_DATA, PathTools.DIR_LOCALIZATION, DIR_LANGUAGE);

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);


            string tradFile = Path.Combine(path, TRAD_FILE_NAME);

            if (File.Exists(tradFile))
                File.Delete(tradFile);

            try
            {
                var taskWriter = this.WriteTrad(tradFile);
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
        }
    }
}
