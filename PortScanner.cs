using System.IO.Ports;

namespace kimandtodd.GPX_Reader
{
    public class PortScanner
    {
        private string[] _portList;

        public PortScanner()
        {
            this.scanPorts();
        }

        public string[] getPorts()
        {
            return this._portList;
        }

        public void scanPorts()
        {
            // Get a list of serial port names. 
            this._portList = SerialPort.GetPortNames();
        }

        public int getPortCount()
        {
            return this._portList.Length;
        }
    }
}
