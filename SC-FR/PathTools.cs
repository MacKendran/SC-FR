using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SCFR.Enumerator;

namespace SCFR
{
    internal static class PathTools
    {
        internal const string REG_RSI = "81bfc699-f883-50c7-b674-2483b6baae23";

        const string DEFAULT_PATH = @"C:\Program Files\Roberts Space Industries\";

        internal const string SC_GAME_ROOT = "StarCitizen";
        internal const string SC_GAME_BIN = "Bin64";
        internal const string SC_GAME_EXE = "StarCitizen.exe";

        internal static void DetectPaths(IniFile ini)
        {
            string pathToCheck = Path.Combine(DEFAULT_PATH, "RSI Launcher", "RSI Launcher.exe");
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

                pathToCheck = Path.Combine(regPath, "RSI Launcher.exe");
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
                if (File.Exists(Path.Combine(new string[] { path, live, SC_GAME_BIN, SC_GAME_EXE })))
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

        internal static bool GameTypeExists(string gamePath, GameType gameType)
        {
            return File.Exists(Path.Combine(new string[] { gamePath, gameType.ToString(), SC_GAME_BIN, SC_GAME_EXE }));
        }

    }
}
