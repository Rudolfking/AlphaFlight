using AlphaConfigurator.ManeuverUtil;
using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace AlphaConfigurator
{
    /// <summary>
    /// Interaction logic for MovementConfigurator.xaml
    /// </summary>
    public partial class MovementConfigurator : Window
    {
        public MovementConfigurator()
        {
            InitializeComponent();
        }

        private void add_Click(object sender, RoutedEventArgs e)
        {
            var dc = (sender as Button)?.DataContext as Maneuver;
            var newParam = new Movement(int.Parse(yawT.Text), int.Parse(pitchT.Text), int.Parse(rollT.Text), int.Parse(timeT.Text), dc);
            dc.AddCommand.Execute(newParam);
        }

        private void okBtn_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
