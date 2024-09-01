using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SC_FR_Library.Enumerator;

namespace SC_FR_Library
{
    public class Trad
    {

        internal const string TRAD_FILE_NAME = "global.ini";
        internal const string DIR_LANGUAGE = "french_(france)";

        internal const string URL_TRAD = "https://trad.sc.tasul.fr/api/file/fr";
        internal byte[] tradFileBytes = new byte[0];

        public bool isDownloaded { get; private set; }

        public Trad()
        { }

        public async Task<(bool,Exception)> DownloadTrad()
        {
            isDownloaded = false;
            try
            {
                var r =  await Task.Run(() => new HttpClient().GetByteArrayAsync(URL_TRAD));
                if (r != null)
                {
                    this.tradFileBytes = r;
                    isDownloaded = true;
                    return (true,null);
                }
            }
            catch (Exception ex)
            {
                return(false,ex);
            }

            return (false, null);
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

                        if (remove == false)
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

        public ApplyTradReturn ApplyTrad(GameType gameType, string path)
        {

            if (string.IsNullOrEmpty(path))
                return ApplyTradReturn.EmptyPath;

            if (!this.isDownloaded)
                return ApplyTradReturn.TradNotValid;


            path = Path.Combine(path, gameType.ToString());

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
