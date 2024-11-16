using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SC_FR_Library
{
    public static class Enumerator
    {
        public enum SCPathType
        {
            Launcher,
            Games
        }

        public enum GameType
        {
            Live = 0,
            PTU = 1,
            EPTU = 2,
            TECHPREVIEW = 3,
            HOTFIX = 4
        }       

        public static string[] GameTypePathSection = new string[] { "Live", "PTU", "EPTU","TECH-PREVIEW","HOTFIX" };

        public enum ApplyTradReturn
        {
            Success,
            Ingnored,
            EmptyPath,
            InvalidPath,
            TradNotValid,
            ErrorFile
        }

        public enum ParamVersion
        {
            version,
            lastUpdate
        }

        public enum IniOption
        {
            AutoMaj
        }

        public enum ParamOption
        {
            Silent,
            ForceDownload
        }

        public enum IniSection
        {
            None,
            Path,
            Options,
            Versions
        }

        public enum TradElement
        { 
            UI,
            UI_Ship,
            Item,
            Mission
        }

        public enum TradType
        {
            VO = 0,
            SCFR = 1,
            Speedou = 2,
            Circus = 3
        }
    }
}
