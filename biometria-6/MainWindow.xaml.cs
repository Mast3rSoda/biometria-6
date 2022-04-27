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
    public struct Measure
    {
        public string KeyName { get; set; }
        public int DwellTime { get; set; }
        public int FlightTime { get; set; }
        public Measure(string KeyName, int DwellTime, int FlightTime)
        {
            this.KeyName = KeyName;
            this.DwellTime = DwellTime;
            this.FlightTime = FlightTime;
        }
    }
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public string MainFileName { get; set; }
        Dictionary<string, List<Measure>> AllFilesMeasures = new();

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
                AllFilesMeasures = new();
                MainFileName = openFileDialog.FileName;

                StringBuilder stringBuilder = new StringBuilder();

                string[] lines;

                string[] fileEntries = Directory.GetFiles( string.Join("\\", MainFileName.Split("\\").SkipLast(1)));

                foreach (var file in fileEntries)
                {
                    stringBuilder = new StringBuilder();

                    lines = File.ReadAllLines(file, Encoding.UTF8);
                    AllFilesMeasures.Add(file, new());

                    foreach (string line in lines)
                    {
                        if(string.IsNullOrWhiteSpace(line))
                            continue;
                        stringBuilder.AppendLine(line);
                        var data = line.Split(",");

                        AllFilesMeasures[file].Add(new Measure(data[0], int.Parse(data[2]), int.Parse(data[1])));
                    }
                }


            }

        }

        private void KNN(object sender, RoutedEventArgs e)
        {
                StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"Plik: {MainFileName[^8..^4]}, \n");

            foreach (var res in Algorithm.KNN(AllFilesMeasures, MainFileName, (ClasifyBy)(int)EnumSlider.Value, SliderValue))
            {
                if (string.IsNullOrWhiteSpace(res.Key))
                    continue;
                stringBuilder.AppendLine($"{res.Key} - {Math.Round(res.Value,3)}, \n");
            }
            ReadData.Text = stringBuilder.ToString();
        }

        private void Bayes(object sender, RoutedEventArgs e)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"Plik: {MainFileName[^8..^4]}, \n");
            foreach (var pair in Algorithm.Bayes(AllFilesMeasures, MainFileName, (ClasifyBy)(int)EnumSlider.Value, SliderValue))
            {
                stringBuilder.AppendLine($"{pair.Key} - {pair.Value.Key}E-{pair.Value.Value}, \n");
            }

            ReadData.Text = stringBuilder.ToString();
        }
    }
}
