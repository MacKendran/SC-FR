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

    }
}
