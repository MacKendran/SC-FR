using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SC_FR_Library.Enumerator;

namespace SC_FR_Library
{
    public class MemoryParam
    {
        private const string tradOption = "trad_op";


        internal Dictionary<string, string> param = new Dictionary<string, string>();


        public MemoryParam() { }

        public string Get(string key)
        {
            if (param.TryGetValue(key, out string val))
                return val;
            else
                return string.Empty;
        }
        public string Get(SCPathType type)
        { return Get(type.ToString()); }
        public string Get(GameType type)
        { return Get(type.ToString()); }
        public string Get(IniOption type)
        { return Get(type.ToString()); }
        public string Get(ParamOption type)
        { return Get(type.ToString()); }
        public string Get(ParamVersion version, GameType game)
        { return Get(version.ToString()+"_"+game.ToString());}
        public string Get(ParamVersion type)
        { return Get(type.ToString()); }
        public string Get(TradElement tradOption)
        { 
            string v = Get(tradOption + tradOption.ToString());
            if (string.IsNullOrEmpty(v))
                return TradType.SCFR.ToString();

            return v;
        }

        public void Set(string key, string value)
        {
            if (!param.TryAdd(key, value))
                param[key] = value;
        }
        public void Set(GameType key, string val)
        { Set(key.ToString(), val); }
        public void Set(SCPathType key, string val)
        { Set(key.ToString(), val); }
        public void Set(IniOption key, string val)
        { Set(key.ToString(), val); }
        public void Set(ParamOption key, string val)
        { Set(key.ToString(), val); }
        public void Set(ParamVersion key, string val)
        { Set(key.ToString(), val); }
        public void Set(ParamVersion version, GameType game, string val)
        { Set(version.ToString() + "_" + game.ToString(),val); }
        public void Set(TradElement tradOption, string val)
        { Set(tradOption + tradOption.ToString(), val); }

    }
}
