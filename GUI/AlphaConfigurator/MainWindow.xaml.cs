using AlphaConfigurator.ManeuverUtil;
using AlphaConfigurator.Serial;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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
                comCombo.SelectedIndex = AvailableComPorts.Count - 1;
            }
            if (AvailableBaudRates?.Count > 9)
            {
                baudCombo.SelectedIndex = 3;
            }
        }

        public ObservableCollection<string> AvailableComPorts { get; set; }
        public ObservableCollection<string> AvailableBaudRates { get; set; }

        public bool IsAutoScroll { get; set; } = true;

        public void Log(string log)
        {
            var time = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff",
                                            CultureInfo.InvariantCulture);
            this.logField.AppendText(time + ": "+ log + (isEnded(log) ? "" : Environment.NewLine));
            if (IsAutoScroll)
                this.logField.ScrollToEnd();
        }

        private bool isEnded(string log)
        {
            if (log.EndsWith("\r") || log.EndsWith("\n") || log.EndsWith("\r\n") || log.EndsWith(Environment.NewLine))
                return true;
            return false;
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

        private void sendCommands_Click(object sender, RoutedEventArgs e)
        {
            if (ourPort == null)
                MessageBox.Show("Port is not open!");
            else
            {
                Log("---------------- Sending data to copter! ------------------");
                Task.Run(() =>
                {
                    try
                    {
                        var toSendArray = ManeuverTrack.ToArray();
                        var ppmList = new List<Tuple<ManeuverReference, Movement>>();
                        foreach (var item in toSendArray)
                        {
                            var man = Maneuvers.First(x => x.Uid == item.UidReference);
                            ppmList.AddRange(man.Movements.Select(x=>new Tuple<ManeuverReference, Movement>(item, x)));
                        }
                        foreach (var item in ppmList)
                        {
                            if (cancelled)
                                break;
                            item.Item1.IsHighlighted = true;
                            tryRefresh();
                            ourPort.SendData(item.Item2.GetInSerialFormat());
                            Thread.Sleep(item.Item2.TimeMs);
                            item.Item1.IsHighlighted = false;
                            tryRefresh();
                        }
                    }
                    catch (Exception ex)
                    {
                        Dispatcher.BeginInvoke((Action)(() =>
                        {
                            Log("EXCEPTION sending data to copter! Reason: " + ex.Message);
                            Log("Sending data to copter halted!");
                        }));
                    }
                    Dispatcher.BeginInvoke((Action)(() =>
                    {
                        if (cancelled)
                        {
                            cancelled = false;
                            Log("XXXXXXXXXXXX --- Data sending cancelled --- XXXXXXXXXXX");
                        }
                        else
                        {
                            Log("-------XXX------ Data sent to copter! --------XXX------");
                        }
                    }));                    
                });
            }
        }

        private bool cancelled = false;
        private void cancelCommands_Click(object sender, RoutedEventArgs e)
        {
            cancelled = true;
        }

        private void tryRefresh()
        {
            try
            {
                Dispatcher.BeginInvoke((Action)(() =>
                {
                    try
                    {
                        CollectionViewSource.GetDefaultView(Maneuvers).Refresh();
                        CollectionViewSource.GetDefaultView(ManeuverTrack).Refresh();
                    }
                    catch { }
                }));
            }
            catch { }
        }

        private void flushButton_Click(object sender, RoutedEventArgs e)
        {
            logField.Text = "";
        }

        public ObservableCollection<Maneuver> Maneuvers { get; set; } = new ObservableCollection<Maneuver>();

        public ObservableCollection<ManeuverReference> ManeuverTrack { get; set; } = new ObservableCollection<ManeuverReference>();

        private void addMan_Click(object sender, RoutedEventArgs e)
        {
            var wind = new MovementConfigurator()
            {
                Owner = this,
            };
            var theManover = new Maneuver("Movement X");
            wind.DataContext = theManover;
            var res = wind.ShowDialog();
            if (res == true)
            {
                Maneuvers.Add(theManover);
            }
        }

        private void deleteBtn_Click(object sender, RoutedEventArgs e)
        {
            var dc = (sender as Button)?.DataContext as Maneuver;
            Maneuvers.Remove(dc);
        }

        private void duplicateBtn_Click(object sender, RoutedEventArgs e)
        {
            var dc = (sender as Button)?.DataContext as Maneuver;
            var newManeuver = new Maneuver(dc); // "copy constructor"
            Maneuvers.Add(newManeuver);
        }

        private void editBtn_Click(object sender, RoutedEventArgs e)
        {
            var dc = (sender as Button)?.DataContext as Maneuver;
            var oldInd = -1;
            for (int i = 0; i < Maneuvers.Count; i++)
            {
                if (Maneuvers[i] == dc)
                {
                    oldInd = i;
                    break;
                }
            }
            var oldDc = new Maneuver(dc);
            var wind = new MovementConfigurator();
            var theManover = dc;
            wind.DataContext = theManover;
            var res = wind.ShowDialog();
            if (res != true)
            {
                Maneuvers[oldInd] = oldDc;
            }
            tryRefresh();
        }

        private void addToTrackBtn_Click(object sender, RoutedEventArgs e)
        {
            var dc = (sender as Button)?.DataContext as Maneuver;
            var theRef = new ManeuverReference(dc.Uid, this);
            ManeuverTrack.Add(theRef);
        }

        private void trackDeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            var dc = (sender as Button)?.DataContext as ManeuverReference;
            ManeuverTrack.Remove(dc);
        }

        private void saveManeuvers_Click(object sender, RoutedEventArgs e)
        {
            var ser = JsonConvert.SerializeObject(this.Maneuvers);
            File.WriteAllText(saveLoadFileName.Text, ser);
        }

        private void loadManeuvers_Click(object sender, RoutedEventArgs e)
        {
            var deser = File.ReadAllText(saveLoadFileName.Text);
            var man = JsonConvert.DeserializeObject<ObservableCollection<Maneuver>>(deser);
            var newUid = -1;
            foreach (var item in man)
            {
                if (newUid < item.Uid)
                    newUid = item.Uid;
            }
            newUid++;
            Maneuver.UpdateUid(newUid);
            foreach (var item in man)
            {
                foreach (var mov in item.Movements)
                {
                    mov.Host = item;
                }
                this.Maneuvers.Add(item);
            }
            tryRefresh();
        }

        private void saveTrackBtn_Click(object sender, RoutedEventArgs e)
        {
            var ser = JsonConvert.SerializeObject(this.ManeuverTrack);
            File.WriteAllText(saveLoadTrackName.Text, ser);
        }

        private void loadTrackBtn_Click(object sender, RoutedEventArgs e)
        {
            var deser = File.ReadAllText(saveLoadTrackName.Text);
            var man = JsonConvert.DeserializeObject<ObservableCollection<ManeuverReference>>(deser);
            var newUid = -1;
            foreach (var item in man)
            {
                if (newUid < item.MyUid)
                    newUid = item.MyUid;
            }
            newUid++;
            ManeuverReference.UpdateUid(newUid);
            foreach (var item in man)
            {
                item.TheHost = this;
                this.ManeuverTrack.Add(item);
            }
            tryRefresh();
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItem = (sender as ListBox)?.SelectedItem as Maneuver;
            if (selectedItem == null)
            {
                selectedInfo.Text = "";
                return;
            }
            var text = "Yaw  Pitch  Roll  Time" + Environment.NewLine; 
            text += selectedItem.GetPrettyMovementsText();
            selectedInfo.Text = text;
        }
    }
}
