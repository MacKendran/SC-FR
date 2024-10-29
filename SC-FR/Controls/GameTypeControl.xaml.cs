using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Path = System.IO.Path;
using static SC_FR_Library.Enumerator;

namespace SCFR.Controls
{
    /// <summary>
    /// Logique d'interaction pour GameTypeControl.xaml
    /// </summary>
    public partial class GameTypeControl : System.Windows.Controls.UserControl
    {

        internal const string TRAD_FILE_NAME = "global.ini";
        internal const string DIR_LANGUAGE = "french_(france)";
        internal const string DIR_DATA = "data";
        internal const string DIR_LOCALIZATION = "Localization";

        internal GameType gameType;
        public string text
        {
            get { return (string)GetValue(TextProp); }
            set 
            { 
                foreach (GameType e in Enum.GetValues(typeof(GameType)))
                {
                    if (e.ToString().Equals(value))
                    {
                        gameType = e;
                        label.Content = e.ToString();
                        SetValue(TextProp, value);
                    }
                }
            }
        }

        public bool isChecked 
        {
            get { return (this.checkbox.IsEnabled ? (this.checkbox.IsChecked??false) : false); }
            set
            {
                if (this.checkbox.IsEnabled)
                {
                    SetCheck(value, true);
                }
            }
        }

        public bool checkboxEnabled
        {
            get { return this.checkbox.IsEnabled; }
            set { this.checkbox.IsEnabled = value;}
        }

        public static readonly DependencyProperty TextProp =
        DependencyProperty.Register("text", typeof(string), typeof(GameTypeControl), new PropertyMetadata());


        public GameTypeControl()
        {
            InitializeComponent();
        }

        public void SetCheck(bool check, bool saveIni)
        { 
            this.checkbox.IsChecked = check;
            if(saveIni) 
                ((App)App.Current).ini.Write(gameType.ToString(),check?"1":"0",IniSection.Options);
        }
        
        private void checkbox_Click(object sender, RoutedEventArgs e)
        {
            if (this.checkbox.IsEnabled)
            {
                if (!this.checkbox.IsChecked??false)
                {
                    var p = (App)App.Current;
                    
                    string path = Path.Combine(p.param.Get(SCPathType.Games),text);
                    string file = Path.Combine(path,DIR_DATA,DIR_LOCALIZATION,DIR_LANGUAGE,TRAD_FILE_NAME);


                    if (File.Exists(file) 
                        && (System.Windows.MessageBox.Show("Voulez-vous supprimer le fichier de traduction ?",$"Fichier de traduction {text}",MessageBoxButton.YesNo,MessageBoxImage.Question) == MessageBoxResult.Yes)) 
                    {   
                        File.Delete(file);
                        p.trad.SetUserCfgLang(path, true);
                        p.param.Set(ParamVersion.version,this.gameType,string.Empty);
                    }
                }
            }
        }
    }
}
