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

namespace HGA22
{
    public partial class Form1 : Form
    {

        public static readonly List<string> baudRates = new List<string>
        {
            "200",
            "300",
            "600",
            "1200",
            "2400",
            "4800",
            "9600",
            "19200",
            "38400",
            "57600",
            "115200",
            "230400",
            "460800",
            "921600"
        };

        bool HGAStartBitDetected = false;
        byte[] HGAData = new byte[300];
        int HGAByteCounter = 0;

        public Form1()
        {
            InitializeComponent();
            string[] ports = SerialPort.GetPortNames();
            foreach(string port in ports)
            {
                comboBox1.Items.Add(port);
            }
            foreach(String baudrate in baudRates)
            {
                comboBoxBaudrate.Items.Add(baudrate);
            }
            comboBoxBaudrate.SelectedIndex = comboBoxBaudrate.Items.IndexOf("200");
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {

            try
            {
                if (serialPort1.IsOpen)
                {
                    serialPort1.Close();
                    btnOpen.Text = "Open";
                }
                else
                {
                    serialPort1.PortName = comboBox1.SelectedItem.ToString();
                    var baudrate = Convert.ToInt32(comboBoxBaudrate.SelectedItem.ToString());
                    serialPort1.BaudRate = baudrate;
                    serialPort1.Open();
                    btnOpen.Text = "Close";
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(serialPort1.IsOpen)
            {
                serialPort1.Close();
            }
        }

        private void serialPort1_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            int length = serialPort1.BytesToRead;
            byte[] buf = new byte[length];
            serialPort1.Read(buf, 0, length);
            for(int i = 0; i < length; i++)
            {
                richTextBox1.Invoke((MethodInvoker)delegate
                {
                    richTextBox2.Text += " 0x" + buf[i].ToString("X");
                    if (buf[i] == 0x16)
                    {
                        richTextBox2.Text += System.Environment.NewLine;
                    }
                    richTextBox2.SelectionStart = richTextBox2.Text.Length;
                    richTextBox2.ScrollToCaret();
                    if (HGAStartBitDetected)
                    {
                        HGAData[HGAByteCounter] = buf[i];
                        HGAByteCounter++;
                        if (HGAByteCounter == 3)
                        {
                            if (HGAData[1] != HGAData[2])
                            {
                                HGAStartBitDetected = false;
                            }
                        }
                        if (HGAData[1] + 4 == HGAByteCounter)
                        {
                            HGAStartBitDetected = false;
                            if (HGAData[0] == 0x68 && HGAData[1] == 0x0A && HGAData[2] == 0x0A && HGAData[5] == 0x00 && HGAData[6] == 0x00 && HGAData[7] == 0x00)
                            {
                                try
                                {
                                    DateTime DateTime = new DateTime(2000 + HGAData[13], HGAData[12], HGAData[11] & 0x1F, HGAData[10] & 0x1F, HGAData[9] & 0x3F, (HGAData[8] & 0xFC) >> 2);
                                    richTextBox1.Text += DateTime + System.Environment.NewLine;
                                    richTextBox1.SelectionStart = richTextBox1.Text.Length;
                                    richTextBox1.ScrollToCaret();
                                }
                                catch (Exception ex)
                                {

                                }

                            }
                        }
                    }
                    else
                    {
                        if (buf[i] == 104)
                        {
                            HGAData[0] = 0x68;
                            HGAStartBitDetected = true;
                            HGAByteCounter = 1;
                        }
                    }
                });
            }
        }
    }
}
