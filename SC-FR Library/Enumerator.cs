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
            [Description("LIVE")]
            Live = 0,
            [Description("PTU")]
            PTU = 1,
            [Description("EPTU")]
            EPTU = 2,
            [Description("TECHPREVIEW")]
            TECHPREVIEW = 3,
            [Description("HOTFIX")]
            HOTFIX = 4,
            [Description("4.0_PREVIEW")]
            PREVIEW_4_0 = 5
        }       

        public static string[] GameTypePathSection = new string[] { "Live", "PTU", "EPTU","TECH-PREVIEW","HOTFIX","4.0_PREVIEW" };

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
            AutoTradMaj,
            AutoExecLauncher,
            AutoClose
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
