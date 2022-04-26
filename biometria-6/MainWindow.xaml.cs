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
    struct Measure
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
        List<Measure> MainMeasureList = new List<Measure>();
        List<List<Measure>> AllFilesMeasures = new();

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

                StringBuilder stringBuilder = new StringBuilder();

                string[] lines = File.ReadAllLines(fileName, Encoding.UTF8);

                foreach (string line in lines)
                {
                    stringBuilder.AppendLine(line);
                    var data = line.Split(",");
                    MainMeasureList.Add(new Measure(data[0], int.Parse(data[2]), int.Parse(data[1])));
                }
                ReadData.Text = stringBuilder.ToString();

                string[] fileEntries = Directory.GetFiles( string.Join("\\", fileName.Split("\\").SkipLast(1)));

                foreach (string file in fileEntries)
                {
                    if (file == fileName)
                        continue;
                    stringBuilder = new StringBuilder();

                    lines = File.ReadAllLines(file, Encoding.UTF8);
                    AllFilesMeasures.Add(new());

                    var measure = AllFilesMeasures.Last();
                    foreach (string line in lines)
                    {
                        if(string.IsNullOrWhiteSpace(line))
                            continue;
                        stringBuilder.AppendLine(line);
                        var data = line.Split(",");

                        measure.Add(new Measure(data[0], int.Parse(data[2]), int.Parse(data[1])));
                    }
                }


            }

        }

       
    }
}
