using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Firmata.NET;

namespace ArduinoPCCommunicationClient
{
    public partial class Form1 : Form
    {
        private static volatile bool _shouldStop = false;
        private static Arduino arduino;
        private Thread doValueUpdateThread;
        public Form1()
        {
            InitializeComponent();
            int posy = 32;
            for (int i = 0; i < 14; i++)
            {
                Button button = new Button();
                string name = $"btn{i}";
                button.Name = name;
                button.Click += SetDigitalValue;
                button.Location = new Point(230, posy);
                button.Size = new Size(75, 21);
                button.Text = "LOW";
                posy += 30;
                this.Controls.Add(button);
            }
            doValueUpdateThread = new Thread(DoValueUpdate);
            foreach (var serialPort in SerialPort.GetPortNames().ToList())
            {
                var toolStripMenuItem = new ToolStripMenuItem(serialPort);
                toolStripMenuItem.Click += OnSerialPortClick;
                portToolStripMenuItem.DropDownItems.Add(toolStripMenuItem);
            }
        }

        private void SetDigitalValue(object sender, EventArgs e)
        {
            var tmp = ((Button) sender).Name.Remove(0,3);
            int pin = int.Parse(tmp);
            arduino.digitalWrite(pin, arduino.digitalRead(pin) == 1 ? 0 : 1);
        }

        private void OnSerialPortClick(object sender, EventArgs e)
        {
            var comPort = ((ToolStripMenuItem) sender).Text;
            arduino = new Arduino(comPort);
            doValueUpdateThread.Start();
        }

        public void DoValueUpdate()
        {
            while (!_shouldStop)
            {
                this.Invoke(
                    (MethodInvoker) delegate {
                        lblA0.Text = arduino.analogRead(0).ToString();
                        lblA1.Text = arduino.analogRead(1).ToString();
                        lblA2.Text = arduino.analogRead(2).ToString();
                        lblA3.Text = arduino.analogRead(3).ToString();
                        lblA4.Text = arduino.analogRead(4).ToString();
                        lblA5.Text = arduino.analogRead(5).ToString();
                    }
                );
            }
        }

        private void ShouldStop()
        {
            _shouldStop = true;
        }
    }
}
