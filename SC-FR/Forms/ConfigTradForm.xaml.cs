using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static SC_FR_Library.Enumerator;
using SCFR;
using System.Configuration;

namespace SC_FR.Forms
{

    /// <summary>
    /// Logique d'interaction pour ConfigTradForm.xaml
    /// </summary>
    public partial class ConfigTradForm : Window
    {
        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
        private const int GWL_STYLE = -16;
        private const int WS_SYSMENU = 0x80000;

        App app = (App)System.Windows.Application.Current;


        public ConfigTradForm()
        {
            InitializeComponent();
            var hwnd = new WindowInteropHelper(this).Handle;
            SetWindowLong(hwnd, GWL_STYLE, GetWindowLong(hwnd, GWL_STYLE) & ~WS_SYSMENU);

            foreach (TradElement o in Enum.GetValues(typeof(TradElement)))
            {
                string v = app.param.Get(o);
                var radio = this.FindName(o.ToString().ToLower() + v);
                if (radio != null)
                { ((System.Windows.Controls.RadioButton)radio).IsChecked = true; }
            }
        }

        private void SetAllElement(TradType trad)
        {
            foreach (TradElement o in Enum.GetValues(typeof(TradElement)))
            {
                var radio = this.FindName(o.ToString().ToLower() + trad.ToString());
                if (radio != null)
                { ((System.Windows.Controls.RadioButton)radio).IsChecked = true; }
            }
        }

        private void bVO_Click(object sender, RoutedEventArgs e)
        {
            this.SetAllElement(TradType.VO);
        }

        private void bSCFR_Click(object sender, RoutedEventArgs e)
        {
            this.SetAllElement(TradType.SCFR);
        }

        private void bSpeedou_Click(object sender, RoutedEventArgs e)
        {
            this.SetAllElement(TradType.Speedou);
        }

        private void bCircus_Click(object sender, RoutedEventArgs e)
        {
            this.SetAllElement(TradType.Circus);
        }

        private void bCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void bValid_Click(object sender, RoutedEventArgs e)
        {
            bool needUpdateTrad = false;

            foreach (TradElement elem in Enum.GetValues(typeof(TradElement)))
            {
                string actualValue = app.param.Get(elem);

                foreach (TradType trad in Enum.GetValues(typeof(TradType)))
                {
                    var radio = this.FindName(elem.ToString().ToLower() + trad.ToString());
                    if (radio != null && ((System.Windows.Controls.RadioButton)radio).IsChecked == true)
                    {
                        if (actualValue != trad.ToString())
                        {
                            needUpdateTrad = true;
                            app.param.Set(elem, trad.ToString());
                            app.ini.Write(elem.ToString(), trad.ToString(), IniSection.Options);
                        }
                        break;
                    }
                }
            }
            if (needUpdateTrad)
            {
                app.UpdateTrad(false,true);
            }
            this.Close();
        }
    }
}
