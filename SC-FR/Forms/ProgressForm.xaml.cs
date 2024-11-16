using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MessageBox = System.Windows.MessageBox;
using static SC_FR_Library.Enumerator;
using SC_FR_Library;
using System.Windows.Threading;

namespace SCFR
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class ProgressForm : Window
    {

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
        private const int GWL_STYLE = -16;
        private const int WS_SYSMENU = 0x80000;

        App app = (App)System.Windows.Application.Current;

        BackgroundWorker worker = new BackgroundWorker();
        bool silentSuccess;
        bool forceUpdate;

        public ProgressForm(string initalText, bool silentSuccess, bool forceUpdate = false):base()
        {
            InitializeComponent();
            this.textBlock.Text = initalText;
            this.progressBar.Maximum = 100;
            this.silentSuccess = silentSuccess;
            this.forceUpdate = forceUpdate;
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {

            var hwnd = new WindowInteropHelper(this).Handle;
            SetWindowLong(hwnd, GWL_STYLE, GetWindowLong(hwnd, GWL_STYLE) & ~WS_SYSMENU);

            worker.WorkerReportsProgress = true;
            worker.DoWork += Worker_DoWork;
            worker.ProgressChanged += Worker_ProgressChanged;
            worker.RunWorkerCompleted += Worker_RunWorkerCompleted;

            worker.RunWorkerAsync();
        }

        private void Worker_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e)
        {
            this.Close();
        }

        private void Worker_ProgressChanged(object? sender, ProgressChangedEventArgs e)
        {
            this.progressBar.Value = e.ProgressPercentage;
            //throw new NotImplementedException();
        }

        private void Worker_DoWork(object? sender, DoWorkEventArgs e)
        {
            var p = (App)System.Windows.Application.Current;
            var w = (sender as BackgroundWorker);

            if (string.IsNullOrEmpty(p.param.Get(SCPathType.Games)))
            {
                MessageBox.Show("Aucun chemin pour les répertoires d'instance de StarCitizen", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            int percent = 1;

            while (!p.trad.getVersionFailed.HasValue)
            { 
                Thread.Sleep(100);
            }

            if (p.trad.getVersionFailed.HasValue && p.trad.getVersionFailed.Value == true)
            {
                MessageBox.Show("Impossible d'obtenir les informations de version de la traduction", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            Dictionary<TradElement, TradType> optionTrad = new Dictionary<TradElement, TradType>();

            bool customTrad = false;

            foreach (TradElement elem in Enum.GetValues(typeof(TradElement)))
            {
                string v = app.param.Get(elem);
                if (v.Equals(TradType.SCFR.ToString()))
                    optionTrad.Add(elem, TradType.SCFR);
                else
                {
                    foreach (TradType type in Enum.GetValues(typeof(TradType)))
                    {
                        if (type.ToString().Equals(v) && type != TradType.SCFR)
                        {
                            customTrad = true;
                            optionTrad.Remove(elem);
                            optionTrad.Add(elem, type);
                            break;
                        }
                    }
                }
            }

            if (!p.NeedToUpgrade() && !forceUpdate)
            {
                percent = 100;
                w.ReportProgress(percent);
                e.Cancel = true;
                if (!silentSuccess)
                    this.Dispatcher.Invoke(()=>MessageBox.Show(this, $"la version {p.trad.version.update} est déjà installée."));
                    
                return;
            }

            Task<bool> downloadTrad = null;

            if (customTrad)
            {
                downloadTrad = p.trad.DownloadTradCustom(optionTrad[TradElement.UI],
                                                         optionTrad[TradElement.UI_Ship],
                                                         optionTrad[TradElement.Item],
                                                         optionTrad[TradElement.Mission]);
            }
            else
            {
                downloadTrad = p.trad.DownloadTrad();
            }

            while (!downloadTrad.IsCompleted)
            {
                w.ReportProgress(percent);
                Thread.Sleep(1000);
                percent++;
            }
            if (downloadTrad.Exception != null)
            {
                MessageBox.Show($"une exception est survenue pendant le téléchargement du fichier de traduction\n\n{downloadTrad.Exception.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (downloadTrad.IsFaulted || downloadTrad.IsCanceled || downloadTrad.Result == false)
            {
                MessageBox.Show("une erreur est survenue au téléchargement du fichier de traduction", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            percent = 60;
            w.ReportProgress(percent);

            var gameTypes = System.Enum.GetValues(typeof(GameType));
            var resultList = new List<(GameType type, ApplyTradReturn result)>();
            foreach (GameType gameType in gameTypes)
            {
                
                string activeKey = p.param.Get(gameType);

                if (activeKey == "0")
                    resultList.Add((gameType, ApplyTradReturn.Ingnored));
                else
                {
                    string path = p.param.Get(SCPathType.Games);
                    var res = p.trad.ApplyTrad(gameType, path);
                    resultList.Add((gameType, res));
                    if (res == ApplyTradReturn.Success)
                    {
                        p.param.Set(ParamVersion.version, gameType, p.trad.version.update);
                        p.ini.Write(gameType.ToString(), p.trad.version.update, IniSection.Versions);
                    }
                }
                
                percent += 40 / gameTypes.Length ;
                w.ReportProgress(percent);
            }

            var countSuccess = resultList.Count(x => x.result == ApplyTradReturn.Success || x.result == ApplyTradReturn.EmptyPath || x.result == ApplyTradReturn.Ingnored);
            w.ReportProgress(100);

            if (countSuccess == resultList.Count)
            {                
                if (!silentSuccess)
                    MessageBox.Show("Mise à jour effectuée", "Mise à jour", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                StringBuilder sb = new StringBuilder();
                foreach (var r in resultList)
                {
                    sb.Append($"{r.type.ToString()} : ");
                    switch (r.result)
                    {
                        case ApplyTradReturn.InvalidPath:
                            sb.AppendLine($"chemin invalide");
                            break;
                        case ApplyTradReturn.Success:
                            sb.AppendLine($"mise à jour effectuée");
                            break;
                        case ApplyTradReturn.EmptyPath:
                            sb.AppendLine($" ignoré car chemin vide");
                            break;
                        case ApplyTradReturn.TradNotValid:
                            sb.AppendLine($" traduction non valide");
                            break;
                    }
                }

                MessageBox.Show(sb.ToString(), "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            e.Cancel = true;
        }
    }
}