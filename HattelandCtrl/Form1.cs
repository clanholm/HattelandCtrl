using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;
using System.Windows.Forms.VisualStyles;

namespace HattelandCtrl
{
    public partial class Form1 : Form
    {
        byte[] brCmd = { 0x07, 0xFF, 0x42, 0x52, 0x54, 0x01, 0x10, 0x99, 0x00 };
        string dataIn;
        int brightnessLevel = 10;
        
         
                        
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string[] ports = System.IO.Ports.SerialPort.GetPortNames();
            cBoxCommPort.Items.AddRange(ports);

            btnConnect.Enabled = false;
            btnDisconnect.Enabled = false; 
            trackBar1.Enabled = false;

            trackBar1.Value= 10;
            lblLevel.Text = "10";
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            try
            {
                serialPort1.PortName = cBoxCommPort.Text;
                serialPort1.BaudRate = 9600;
                serialPort1.DataBits = 8;
                serialPort1.StopBits = StopBits.One;
                serialPort1.DtrEnable = false;
                serialPort1.RtsEnable = false;
                serialPort1.Parity = Parity.None;

                serialPort1.Open();

                btnConnect.Enabled = false;
                btnDisconnect.Enabled = true;
                trackBar1.Enabled = true;
            }

            catch (Exception err)
            {
                MessageBox.Show(err.Message, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                btnDisconnect.Enabled = false;
                btnConnect.Enabled = true;
                trackBar1.Enabled = false;
            }
        }

        private void btnDisconnect_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                serialPort1.Close();
                btnDisconnect.Enabled = false;
                btnConnect.Enabled = true;
                trackBar1.Enabled = false;
            }
        }

        private void cBoxCommPort_SelectedIndexChanged(object sender, EventArgs e)
        {
            if((cBoxCommPort.Text != "") && (!serialPort1.IsOpen))    
            {
                btnConnect.Enabled = true;
            }
        }

        private byte calculateCheckSum(byte[] data)
        {
            int accum = 0;
            
            for(int idx = 0; idx < data.Length; idx++) 
            {
                accum = accum += data[idx];
            }
            
            byte[] total = BitConverter.GetBytes(accum);
            int difference = 255 - total[0];
            byte[] holder = BitConverter.GetBytes(difference);
            byte checkSum = holder[0];
            return checkSum;
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            timer1.Enabled = true;
            timer1.Interval = 500;
            timer1.Start();
            brightnessLevel = trackBar1.Value;
        }

        private void serialPort1_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            dataIn = serialPort1.ReadExisting();
            this.Invoke(new EventHandler(ShowData));
        }

        private void ShowData(object sender, EventArgs e)
        {
            int dataLengthIn = dataIn.Length;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(serialPort1.IsOpen) 
            {
                serialPort1.Close();
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            double brtLevel = brightnessLevel * .3922;
            lblLevel.Text = string.Format("{0:0}", brtLevel);

            byte[] valueOfSlider = BitConverter.GetBytes(brightnessLevel);
            brCmd[7] = valueOfSlider[0];
            byte[] dataToCheck = { brCmd[7] };
            brCmd[8] = calculateCheckSum(dataToCheck);

            if (serialPort1.IsOpen)
            {
                serialPort1.Write(brCmd, 0, brCmd.Length);
            }
            timer1.Stop();
        }
    }
}
