using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
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
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using static SCFR.Enumerator;


namespace SCFR
{
    /// <summary>
    /// Logique d'interaction pour SelectFile.xaml
    /// </summary>
    public partial class SelectFile : System.Windows.Controls.UserControl
    {

        public event EventHandler PathChanged;

        public string labelText
        {
            get { return (string)GetValue(LabelTextProp); }
            set { SetValue(LabelTextProp, value); }
        }
        public static readonly DependencyProperty LabelTextProp =
        DependencyProperty.Register("labelText", typeof(string), typeof(SelectFile), new PropertyMetadata());

        public string filterFile
        {
            get { return (string)GetValue(FilterFileProp); }
            set { SetValue(FilterFileProp, value); }
        }
        public static readonly DependencyProperty FilterFileProp =
        DependencyProperty.Register("filterFile", typeof(string), typeof(SelectFile), new PropertyMetadata());

        public string file
        {
            get { return textbox.Text; }
            set { SetFile(value); }
        }


        public SelectFile()
        {
            InitializeComponent();
            textbox.Text = string.Empty;
        }

        private void SetFile(string file)
        {
            if (string.IsNullOrEmpty(file))
            {
                textbox.Text = string.Empty;
            }
            else
            {
                textbox.Text = file;
            }
            PathChanged?.Invoke(this, null);
        }

        private void click(object sender, RoutedEventArgs e)
        {

            if (string.IsNullOrEmpty(filterFile))
            {
                var folderBrowserDialog = new FolderBrowserDialog();
                var result = folderBrowserDialog.ShowDialog();
                if (result == DialogResult.OK)
                {
                    foreach (GameType g in Enum.GetValues(typeof(GameType)))
                    {
                        if (PathTools.GameTypeExists(folderBrowserDialog.SelectedPath, g))
                        {
                            SetFile(folderBrowserDialog.SelectedPath);
                            return;
                        }
                    }

                    MessageBox.Show("Le répertoire sélectionné ne contient pas d'instance correspondant à StartCitizen ", "Instance StarCitizen", MessageBoxButton.OK, MessageBoxImage.Hand);

                }

            }
            else
            {
                var fileDialog = new OpenFileDialog();
                fileDialog.Multiselect = false;
                fileDialog.Filter = filterFile;
                if (fileDialog.ShowDialog() == true)
                {
                    SetFile(fileDialog.FileName);
                }
            }
        }
    }
}
