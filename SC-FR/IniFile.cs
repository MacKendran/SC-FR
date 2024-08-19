using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using static SCFR.Enumerator;

namespace SCFR
{
    internal class IniFile
    {
        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        static extern long WritePrivateProfileString(string section, string key, string value, string filePath);

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        static extern int GetPrivateProfileString(string section, string key, string Default, StringBuilder retVal, int size, string filePath);

        string file = null;

        private string GetEnumSection(IniSection section)
        {
            if (section != IniSection.None)
                return section.ToString();

            return string.Empty;
        }

        public IniFile(string file = "")
        {
            if (string.IsNullOrEmpty(file))
                file = Assembly.GetExecutingAssembly().GetName().Name + ".ini";
            else
                this.file = file;
        }

        public string Read(string key, IniSection section)
        {
            return Read(key, GetEnumSection(section));
        }

        public string Read(string key, string section = "")
        {
            var retVal = new StringBuilder(255);
            GetPrivateProfileString(section, key, "", retVal, 255, file);
            return retVal.ToString();
        }

        public void Write(string key, string value, IniSection section)
        {
            Write(key, value, GetEnumSection(section));
        }

        public void Write(string key, string value, string section = "")
        {
            WritePrivateProfileString(section, key, value, file);
        }

        public void DeleteKey(string key, IniSection section)
        { 
            DeleteKey(key, GetEnumSection(section));
        }

        public void DeleteKey(string key, string section = "")
        {
            Write(key, null, section);
        }

        public void DeleteSection(IniSection section)
        {
            DeleteSection(GetEnumSection(section));
        }

        public void DeleteSection(string section = "")
        {
            Write(null, null, section);
        }

        public bool KeyExists(string key, IniSection section)
        { 
            return KeyExists(key, GetEnumSection(section));
        }

        public bool KeyExists(string key, string section)
        {
            return Read(key, section).Length > 0;
        }
    }
}
