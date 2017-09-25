using AlphaConfigurator.Serial;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace AlphaConfigurator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
            this.DataContext = this;

            AvailableComPorts = new ObservableCollection<string>();
            var items = SerialHelpers.GetAvailableComPorts();
            foreach (var item in items)
            {
                AvailableComPorts.Add(item);
            }
            AvailableBaudRates = new ObservableCollection<string>();
            var br = SerialHelpers.GetBaudRates();
            foreach (var item in br)
            {
                AvailableBaudRates.Add(item.ToString());
            }
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (AvailableComPorts?.Count > 0)
            {
                comCombo.SelectedIndex = 0;
            }
            if (AvailableBaudRates?.Count > 9)
            {
                baudCombo.SelectedIndex = 9;
            }
        }

        public ObservableCollection<string> AvailableComPorts { get; set; }
        public ObservableCollection<string> AvailableBaudRates { get; set; }

        public void Log(string log)
        {
            this.logField.AppendText(DateTime.Now+ ": "+ log + Environment.NewLine);
            this.logField.ScrollToEnd();
        }

        private SerialHandler ourPort = null;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ourPort = new SerialHandler(comCombo.SelectedItem.ToString(), int.Parse(baudCombo.SelectedItem.ToString()));
            ourPort.Open();
        }

        private void closePort_Click(object sender, RoutedEventArgs e)
        {
            if (ourPort == null)
                MessageBox.Show("Port is not open!");
            else
            {
                try
                {
                    ourPort.Dispose();
                    ourPort = null;
                    Log("Port closed");
                }
                catch (Exception ex)
                {

                    MessageBox.Show("Failed to close port: " + ex.Message);
                    Log("Failed to close port: " + ex.Message);
                }
            }
        }

        private void testWrite_Click(object sender, RoutedEventArgs e)
        {
            if (ourPort == null)
                MessageBox.Show("Port is not open!");
            else
            {
                ourPort.SendData(testTextBox.Text);
            }
        }

        private void testWriteLine_Click(object sender, RoutedEventArgs e)
        {
            if (ourPort == null)
                MessageBox.Show("Port is not open!");
            else
            {
                ourPort.SendDataLine(testTextBox.Text);
            }
        }
    }
}
