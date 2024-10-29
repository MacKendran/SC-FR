using Microsoft.Win32;
using System.IO;
using static SC_FR_Library.Enumerator;
using Path = System.IO.Path;

namespace SCFR
{
    public class WinPathTools : SC_FR_Library.PathTools
    {
        internal const string REG_RSI = "81bfc699-f883-50c7-b674-2483b6baae23";

        const string DEFAULT_PATH = @"C:\Program Files\Roberts Space Industries\";


        internal static void DetectPaths(IniFile ini)
        {
            string pathToCheck = Path.Combine(DEFAULT_PATH, "RSI Launcher", SC_LAUNCHER_EXE);
            // Launcher
            if (File.Exists(pathToCheck))
            {
                ini.Write(SCPathType.Launcher.ToString(), pathToCheck, "Path");

                DetectGamePath(DEFAULT_PATH, ini, false);
            }
            else
            {
                // try to get RSI Launcher by Registry

                string regPath = Registry.GetValue(@$"HKEY_LOCAL_MACHINE\SOFTWARE\{REG_RSI}", "installLocation", "").ToString();
                if (string.IsNullOrEmpty(regPath))
                    return;

                pathToCheck = Path.Combine(regPath, SC_LAUNCHER_EXE);
                if (File.Exists(pathToCheck))
                {
                    ini.Write(SCPathType.Launcher.ToString(), pathToCheck, "Path");

                    pathToCheck = Directory.GetParent(regPath).FullName;
                    DetectGamePath(pathToCheck, ini, false);

                }
            }

        }

        internal static bool DetectGamePath(string path, IniFile ini, bool eraseIfExists)
        {
            string gameType = SCPathType.Games.ToString();
            string live = GameType.Live.ToString();


            if ( ini !=null && !string.IsNullOrEmpty(ini.Read(gameType, IniSection.Path)) && !eraseIfExists)
                return true;

            if (path.EndsWith(SC_GAME_EXE))
            {
                path = Path.GetDirectoryName(path);
                path = Directory.GetParent(path).Parent.FullName;

                ini.Write(gameType, path, IniSection.Path);
                return true;
            }

            if (path.EndsWith(SC_GAME_ROOT))
            {
                if (CheckGamePathRoot(path))
                {
                    ini.Write(gameType, path, IniSection.Path);
                    return true;
                }
            }

            string pathToCheck = Path.Combine(new string[] { path, SC_GAME_ROOT, live, SC_GAME_BIN, SC_GAME_EXE });
            if (File.Exists(pathToCheck))
            {
                return DetectGamePath(pathToCheck, ini, eraseIfExists);
            }

            return false;
        }

    }
}
