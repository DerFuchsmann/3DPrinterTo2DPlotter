using System.Text;
using System.Diagnostics;
using System.IO.Ports;

namespace SVGToGCodeGUI
{
    internal class SendGCodeManager
    {
        public event PercentEventHandler LineEvent;
        private string? data;
        private readonly object sync;
        private Boolean printFlag;
        public SendGCodeManager()
        {
            this.data = null;
            this.sync = new object();
            this.printFlag = false;
        }

        public void CancelSendingGCode()
        {
            this.printFlag = false;
        }

        public void ManageSendingGCode(List<string>lines)
        {
            this.printFlag = true;
            Task task = new Task(() =>
            {
                var ports = SerialPort.GetPortNames();
                if (ports.Length > 0)
                {
                    SerialPort port = new SerialPort(ports[0]);
                    try
                    {
                        port.Open();
                        port.BaudRate = 250000;
                        port.Parity = Parity.None;
                        port.StopBits = StopBits.One;
                        port.DataBits = 8;
                        port.WriteTimeout = 5000;
                        port.ReadTimeout = 5000;
                        port.DtrEnable = true;
                        port.RtsEnable = true;
                        port.Handshake = Handshake.None;
                        port.Encoding = Encoding.ASCII;

                        port.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);

                        int loopCounter = 0;
                        while (loopCounter < lines.Count && printFlag)
                        {
                            lock (this.sync)
                            {
                                Debug.Print("Waiting for data from printer...");
                                Monitor.Wait(this.sync);
                                Debug.Print("Got data from printer: " + this.data);
                            }
                            port.Write(lines[loopCounter]);
                            loopCounter++;
                            int percentage = (int) Math.Round(((double)loopCounter/ lines.Count)*100);
                            if(LineEvent != null)
                            {
                                LineEvent(this, new LineSentEventArgs(percentage));
                            }

                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.Print(ex.StackTrace);
                    } finally
                    {
                        port.Close();
                    }
                }
                else
                {
                    Debug.Print("Kein Drucker gefunden!");
                }
            });
            task.Start();
        }

        private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            lock (this.sync)
            {
                this.data = null;
                SerialPort sp = (SerialPort)sender;
                sp.Encoding = Encoding.ASCII;
                this.data = sp.ReadExisting();

                Monitor.Pulse(this.sync);
            }
        }
    }
    public class LineSentEventArgs : EventArgs
    {
        public int Percentage { get; set; }

        public LineSentEventArgs(int percent)
        {
            Percentage = percent;
        }
    }
}
