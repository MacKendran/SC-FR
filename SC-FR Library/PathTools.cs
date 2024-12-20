using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SC_FR_Library.Enumerator;

namespace SC_FR_Library
{
    public class PathTools
    {
        public const string SC_GAME_ROOT = "StarCitizen";
        public const string SC_GAME_BIN = "Bin64";
        public const string SC_GAME_EXE = "StarCitizen.exe";
        public const string SC_LAUNCHER_EXE = "RSI Launcher.exe";

        public const string DIR_DATA = "data";
        public const string DIR_LOCALIZATION = "Localization";

        public static string GetEnumDescription<T>(T enumValue)
        {
            var fieldInfo = enumValue.GetType().GetField(enumValue.ToString());
            var descriptionAttributes = (DescriptionAttribute[])fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);
            return descriptionAttributes.Length > 0 ? descriptionAttributes[0].Description : enumValue.ToString();
        }

        public static string GetGamePathSection(GameType gameType)
        {
            return GetEnumDescription(gameType);
            //return GameTypePathSection[(int)gameType];
        }

        public static bool CheckGamePathRoot(string path)
        {
            foreach (GameType g in Enum.GetValues(typeof(GameType)))
            {
                if (File.Exists(Path.Combine(path, GetGamePathSection(g), SC_GAME_BIN, SC_GAME_EXE)))
                    return true;
            }
            return false;
        }

        public static bool GameTypeExists(string gamePath, GameType gameType)
        {
            return File.Exists(Path.Combine(new string[] { gamePath, GetGamePathSection(gameType), SC_GAME_BIN, SC_GAME_EXE }));
        }

    }
}
