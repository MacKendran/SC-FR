using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCFR
{
    internal static class Enumerator 
    {
        public enum SCPathType
        {
            Launcher,
            Games
        }

        public enum GameType
        {
            Live,
            PTU,
            EPTU,
            TECHPREVIEW,
            HOTFIX
        }

        public enum ApplyTradReturn
        { 
            Success,
            Ingnored,
            EmptyPath,
            InvalidPath,
            TradNotValid,
            ErrorFile
        }
        public enum IniOption
        { 
            AutoMaj
        }

        public enum ParamOption
        {
            Silent
        }

        public enum IniSection
        { 
            None,
            Path,
            Options
        }

    }
}
