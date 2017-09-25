using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlphaConfigurator.Serial
{
    public class SerialHandler : IDisposable
    {
        private string originalPortName;
        private int originalBaud;

        Task readTask;
        SerialPort port = null;
        bool canRead = true;
        /// <summary>
        /// Creates a new alphaflight serial port handler. Needs port name and baud. Creates but NOT opens the port.
        /// Call Open after creation.
        /// </summary>
        /// <param name="portName">The port name, eg COM1</param>
        /// <param name="baud">The baud rate eg. 115200</param>
        public SerialHandler(string portName, int baud)
        {
            port = new SerialPort(portName, baud);
            port.ReadTimeout = 500;
            port.WriteTimeout = 500;

            originalPortName = portName;
            originalBaud = baud;
        }

        /// <summary>
        /// Opens our serial port, after this, it is ready to use
        /// </summary>
        public void Open()
        {
            try
            {
                if (!port.IsOpen)
                    port.Open();
                App.Current.Log("Port is open.");

                readTask = new Task(() =>
                {
                    Read();
                });
                readTask.Start();
            }
            catch (Exception e)
            {
                App.Current.Log("Failed to open port: " + e.Message);
            }
        }

        public void Read()
        {
            while (canRead)
            {
                try
                {
                    string message = port.ReadLine();
                    App.Current.Log("[READLN ]: " + message);
                }
                catch (TimeoutException)
                {
                    App.Current.Log("Timeouted reading.");
                }
                catch (Exception e)
                {
                    App.Current.Log("Read threw exception: " + e.Message);
                }
            }
        }

        public bool SendData(string data)
        {
            if (!port.IsOpen)
                return false;
            try
            {
                port.Write(data);
                App.Current.Log("[WRITE  ]: " + data);
                return true;
            }
            catch (Exception e)
            {
                App.Current.Log("Error writing '" + data + "' to serial port: " + e.Message);
                return false;
            }
        }


        public bool SendDataLine(string data)
        {
            if (!port.IsOpen)
                return false;
            try
            {
                port.WriteLine(data);
                App.Current.Log("[WRITELN]: " + data);
                return true;
            }
            catch (Exception e)
            {
                App.Current.Log("Error writing '" + data + "' to serial port: " + e.Message);
                return false;
            }
        }

        private bool disposed = false;
        public void Dispose()
        {
            if (disposed)
                return;
            disposed = true;
            canRead = false;
            try
            {
                readTask.Wait();
            }
            catch { }
            try
            {
                port.Close();
            }
            catch { }
            port.Dispose();
        }
    }
}
