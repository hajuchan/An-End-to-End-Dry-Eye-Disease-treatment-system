
#Signal input part for DED stimulator 

#2022.12.09. Juchan Ha & Jaehoon Cheon

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections;




namespace Test_2
{
    public delegate void DataPushEventHandler(string item);
    public partial class Form1 : Form
    {
        Form3 f3;
        public DataPushEventHandler DataSendEvent;
        static public bool Form2_on;
        SerialPort sPort;
        int[] data_buff = new int[200];
        static int buffsize = 2000;
        double[] input_Data_1 = new double[buffsize];

        double[] input_Draw_1 = new double[buffsize];
        double[] a = new double[buffsize];
        public double B;
        public int k;
        public double IBI = 0;
        public double adjust_f1 = 0;
        public double avg_noise_P_f1 = 0;
        public double avg_noise_N_f1 = 0;
        public double avg_signal_P_f1 = 0;
        public double avg_signal_N_f1 = 0;
        public double threshold_P_f1 = 0;
        public double threshold_N_f1 = 0;
        int start_byte = 0;
        int start_flag = 0;
        int data_count = 0;
        int Data_1;

        int blink = 0;
        int upcheck = 0;
        int downcheck = 1;

        int s = 0;
        double nTotalSeconds = 0;
        DateTime dt;




        string thisdate = DateTime.Now.ToString("yyMMdd");

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {

            if (null != sPort)
            {
                if (sPort.IsOpen)
                {
                    sPort.Close();
                    sPort.Dispose();
                    sPort = null;
                }
            }
        }



        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (checkBox2.Checked)
            {
                while (sPort.BytesToRead > 0)
                {
                    if (sPort.IsOpen)
                    {
                        if (start_flag == 0)
                        {
                            start_byte = sPort.ReadByte();
                        }
                    }
                    if (start_byte == 0x81)
                    {
                        if (upcheck == 1 && downcheck == 1)
                        {
                            blink++;
                            upcheck = 0;
                            downcheck = 0;
                        }
                        B = blink / 2;
                        textBox1.Text = B.ToString();
                        start_flag = 1;
                        data_buff[data_count] = sPort.ReadByte();

                        data_count++;

                        if (data_count == 4)
                        {
                            Data_1 = ((data_buff[0] & 0x7f) << 7) + (data_buff[1] & 0x7f);
                            start_flag = 2;
                            data_count = 0;
                        }


                        if (start_flag == 2)
                        {

                            for (int i = 0; i < buffsize - 1; i++)
                            {
                                input_Data_1[i] = input_Data_1[i + 1];
                            }


                            input_Data_1[buffsize - 1] = (Data_1 - 7000 - 4350 + 250) * 10 - adjust_f1;
                            if (input_Data_1[buffsize - 1] < (avg_noise_P_f1) && input_Data_1[buffsize - 1] > (avg_noise_N_f1))
                            {
                                input_Data_1[buffsize - 1] = input_Data_1[buffsize - 1] / 10;
                            }
                            else
                            {
                                input_Data_1[buffsize - 1] = input_Data_1[buffsize - 1] * 2;
                            }

                            input_Draw_1 = input_Data_1;

                            if (input_Data_1[buffsize - 1] > threshold_P_f1)
                            {
                                upcheck = 1;
                            }
                            else if (input_Data_1[buffsize - 1] < threshold_N_f1)
                            {
                                downcheck = 1;
                            }
                            start_flag = 0;
                        }
                    }
                }
            }    
        }

        private void On_time1(object sender, EventArgs e)
        {
            scope1.Channels[0].Data.SetYData(input_Data_1);
        }

        private void Ttimer2(object sender, EventArgs e)
        {
                    nTotalSeconds++;
                    textBox2.Text = dt.AddSeconds(nTotalSeconds).ToString("ss");
                    timer2.Start();
            if (nTotalSeconds == 30)
            {
                timer2.Stop();
                if (B > 1 && B < 3)
                {
                    IBI = 30 / B;
                    k = 5;
                    blink = 0;
                    nTotalSeconds = 0;
                    textBox1.Text = "0";
                    textBox2.Text = "0";
                    checkBox1.Checked = true;
                   
                }
                else if (B > 2 && B < 4)
                {
                    IBI = 30 / B;
                    k = 6;
                    blink = 0;
                    nTotalSeconds = 0;
                    textBox1.Text = "0";
                    textBox2.Text = "0";
                    checkBox1.Checked = true;
                }
                else if (B > 3 && B < 5)
                {
                    IBI = 30 / B;
                    k = 7;
                    blink = 0;
                    nTotalSeconds = 0;
                    textBox1.Text = "0";
                    textBox2.Text = "0";
                    checkBox1.Checked = true;
                }
                else if (B > 4)
                {
                    IBI = 30 / B;
                    k = 7;
                    blink = 0;
                    nTotalSeconds = 0;
                    textBox1.Text = "0";
                    textBox2.Text = "0";
                    checkBox1.Checked = true;
                }
                else
                {
                    IBI = 30 / B;
                    nTotalSeconds = 0;
                    blink = 0;
                    textBox1.Text = "0";
                    textBox2.Text = "0";
                    timer2.Start();
                    timer2.Interval = 1000;
                    checkBox2.Checked = true;
                } 
             }

        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e) 
        {
            if (checkBox1.Checked)
            {
                textBox1.Text = "0";
                B = 0;
                Form2 second = new Form2(this);
                second.Show();
                nTotalSeconds = 0;
                nTotalSeconds++;
                checkBox2.Checked = false;
            }
        }
        private void BtnOpen_Click(object sender, EventArgs e)
        {
            checkBox2.Checked = true;
            dt = new DateTime();
            timer2.Interval = 1000;
            nTotalSeconds = 0;
            B = 0;
            checkBox5.Checked = true;
            timer2.Start();
            try
            {
                if (null == sPort)
                {
                    sPort = new SerialPort();
                    sPort.DataReceived += new SerialDataReceivedEventHandler(SerialPort_DataReceived);

                    sPort.PortName = cboPortName.SelectedItem.ToString();
                    sPort.BaudRate = Convert.ToInt32(txtBaudRate.Text);
                    sPort.DataBits = (int)8;
                    sPort.Parity = Parity.None;
                    sPort.StopBits = StopBits.One;
                    sPort.Open();
                }
                if (sPort.IsOpen)
                {
                    btnOpen.Enabled = false;
                    btnClose.Enabled = true;
                }
                else
                {
                    btnOpen.Enabled = true;
                    btnClose.Enabled = false;
                }

            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void BtnClose_Click(object sender, EventArgs e)
        {
            if (null != sPort)
            {
                if (sPort.IsOpen)
                {
                    sPort.Close();
                    sPort.Dispose();
                    sPort = null;
                }
            }
            btnOpen.Enabled = true;
            btnClose.Enabled = false;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            btnOpen.Enabled = true;
            btnClose.Enabled = false;

            cboPortName.BeginUpdate();
            foreach (string comport in SerialPort.GetPortNames())
            {
                cboPortName.Items.Add(comport);
            }
            cboPortName.EndUpdate();

            cboPortName.SelectedItem = "COM12";
            txtBaudRate.Text = "115200";

            CheckForIllegalCrossThreadCalls = false;
            txtDate.Text = thisdate;
        }

        public Form1()
        {
            InitializeComponent();

        }

        private void button1_Click(object sender, EventArgs e)
        {
            Form3 calibration = new Form3(this);
            calibration.Show();
            timer2.Stop();

        }
    }
}
