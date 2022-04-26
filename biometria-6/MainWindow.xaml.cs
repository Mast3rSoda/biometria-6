using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace biometria_6
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public int SliderValue
        {
            get => sliderValue;
            set
            {
                sliderValue = value;
                PropertyChanged?.Invoke(this, new(nameof(SliderValue)));
            }
        }

        private int sliderValue;

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        private void OpenFile(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.txt;)|*.txt|All files (*.*)|*.*";

            if (openFileDialog.ShowDialog() == true)
            {
                string fileName = openFileDialog.FileName;
                using (FileStream fs = File.OpenRead(fileName))
                {
                    byte[] b = new byte[1024];
                    UTF8Encoding temp = new UTF8Encoding(true);
                    StringBuilder stringBuilder = new StringBuilder();
                    while (fs.Read(b, 0, b.Length) > 0)
                    {
                        stringBuilder.Append(temp.GetString(b));
                    }
                    ReadData.Text=stringBuilder.ToString();
                }
            }
        }
       
    }
}
