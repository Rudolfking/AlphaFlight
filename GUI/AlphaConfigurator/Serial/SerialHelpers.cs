using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlphaConfigurator.Serial
{
    public static class SerialHelpers
    {
        public static string[] GetAvailableComPorts()
        {
            App.Current.Log("Getting port names...");
            var got = SerialPort.GetPortNames();
            var tolo = "";
            foreach (var item in got)
            {
                tolo += item + " ";
            }
            App.Current.Log(tolo);
            return got;
        }

        public static int[] GetBaudRates()
        {
            return new[] { 1200, 2400, 4800, 9600, 14400, 19200, 28800, 38400, 57600, 115200, 250000, 500000, 1000000 };
        }
    }
}
