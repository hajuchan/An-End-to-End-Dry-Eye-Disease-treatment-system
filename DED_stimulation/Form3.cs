
#Calibration part for DED stimulator 

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
using Mitov.PlotLab;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Button;
using System.Globalization;

namespace Test_2
{
    public partial class Form3 : Form
    {
        SerialPort sPort;
        int[] data_buff = new int[200];
        static int buffsize = 2000;
        double[] input_Data_1 = new double[buffsize];
        double[] score = new double[1000];
        double[] calibration = new double[1000];

        double[] input_Draw_1 = new double[buffsize];
        double[] a = new double[buffsize];
        public double avg_noise_P = 0;
        public double avg_noise_N = 0;
        public double avg_signal_P = 0;
        public double avg_signal_N = 0;
        public double threshold_P = 0;
        public double threshold_N = 0;
        public double adjust = 0;

        int start_byte = 0;
        int start_flag = 0;
        int data_count = 0;
        int start_flag1 = 0;
        int start_flag2 = 0;
        int start_flag3 = 0;
        int Data_1;
        int s = 0;
        int d = 0;
        int f = 0;
        DateTime dt;
        Form1 f1;

        string thisdate = DateTime.Now.ToString("yyMMdd");

        public Form3()
        {
            InitializeComponent();
        }
        public Form3(Form1 form)
        {
            InitializeComponent();
            f1 = form;
        }

        private void Form3_Load(object sender, EventArgs e)
        {
            Baseline.Enabled = true;
            Noise.Enabled = true; 
            Signal.Enabled = true;

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

        private void Form3_Closed(object sender, FormClosedEventArgs e)
        {
        }

        private void Baseline_Click(object sender, EventArgs e)
        {
            Array.Clear(score, 0, score.Length);
            start_flag1 = 1;
            start_flag2 = 0;
            start_flag3 = 0;
            textBox8.Text = "Stop eye moving";
            dt = new DateTime();
            try
            {
                if (null == sPort)
                {
                    sPort = new SerialPort();
                    sPort.DataReceived += new SerialDataReceivedEventHandler(SPort_DataReceived);

                    sPort.PortName = cboPortName.SelectedItem.ToString();
                    sPort.BaudRate = Convert.ToInt32(txtBaudRate.Text);
                    sPort.DataBits = (int)8;
                    sPort.Parity = Parity.None;
                    sPort.StopBits = StopBits.One;
                    sPort.Open();
                }
                if (sPort.IsOpen)
                {
                    Baseline.Enabled = true;
                    Noise.Enabled = true;
                    Signal.Enabled = true;
                }
                else
                {
                    Baseline.Enabled = false;
                    Noise.Enabled = false;
                    Signal.Enabled = false;
                }

            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Noise_Click(object sender, EventArgs e)
        {
            Array.Clear(score, 0, score.Length);
            start_flag1 = 0;
            start_flag2 = 1;
            start_flag3 = 0;
            textBox8.Text = "Stop eye moving";
            try
            {
                if (null == sPort)
                {
                    sPort = new SerialPort();
                    sPort.DataReceived += new SerialDataReceivedEventHandler(SPort_DataReceived);

                    sPort.PortName = cboPortName.SelectedItem.ToString();
                    sPort.BaudRate = Convert.ToInt32(txtBaudRate.Text);
                    sPort.DataBits = (int)8;
                    sPort.Parity = Parity.None;
                    sPort.StopBits = StopBits.One;
                    sPort.Open();
                }
                if (sPort.IsOpen)
                {
                    Baseline.Enabled = true;
                    Noise.Enabled = true;
                    Signal.Enabled = true;
                }
                else
                {
                    Baseline.Enabled = false;
                    Noise.Enabled = false;
                    Signal.Enabled = false;
                }

            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Signal_Click(object sender, EventArgs e)
        {
            Array.Clear(score, 0, score.Length);
            start_flag1 = 0;
            start_flag2 = 0;
            start_flag3 = 1;
            textBox8.Text = "Please Blinking";
            try
            {
                if (null == sPort)
                {
                    sPort = new SerialPort();
                    sPort.DataReceived += new SerialDataReceivedEventHandler(SPort_DataReceived);

                    sPort.PortName = cboPortName.SelectedItem.ToString();
                    sPort.BaudRate = Convert.ToInt32(txtBaudRate.Text);
                    sPort.DataBits = (int)8;
                    sPort.Parity = Parity.None;
                    sPort.StopBits = StopBits.One;
                    sPort.Open();
                }
                if (sPort.IsOpen)
                {
                    Baseline.Enabled = true;
                    Noise.Enabled = true;
                    Signal.Enabled = true;
                }
                else
                {
                    Baseline.Enabled = false;
                    Noise.Enabled = false;
                    Signal.Enabled = false;
                }

            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void SPort_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            //***************************************************baseline**********************************
            if (start_flag1 == 1)
            {
                for (int count = 0; count < 3999; count++)
                {

                    if (sPort.IsOpen)
                    {
                        if (start_flag1 == 1)
                        {
                            start_byte = sPort.ReadByte();
                        }
                    }
                    if (start_byte == 0x81)
                    {
                        start_flag1 = 2;
                        data_buff[data_count] = sPort.ReadByte();

                        data_count++;

                        if (data_count == 4)
                        {
                            Data_1 = ((data_buff[0] & 0x7f) << 7) + (data_buff[1] & 0x7f);
                            start_flag1 = 3;
                            data_count = 0;
                            s++;
                        }


                        if (start_flag1 == 3)
                        {

                            for (int i = 0; i < buffsize - 1; i++)
                            {
                                input_Data_1[i] = input_Data_1[i + 1];
                            }


                            input_Data_1[buffsize - 1] = (Data_1 - 7000 - 4350 + 250) * 10;
                            score[s - 1] = input_Data_1[buffsize - 1];
                            input_Draw_1 = input_Data_1;

                            start_flag1 = 1;
                        }
                    }
                }
                start_flag1 = 4;

                if (start_flag1 == 4)
                {
                    int mode = 0;
                    int[] index = new int[3500];
                    int max = Int32.MinValue;
                    for (int j = 0; j < score.Length; j++)
                    {
                        index[(int)score[j]]++;
                    }
                    for (int a = 0; a < index.Length; a++)
                    {
                        if (index[a] > max)
                        {
                            max = index[a];
                            mode = a;
                        }
                    }

                    if (mode != 0)
                    {
                        if (mode > 0)
                        {
                            adjust = mode;
                        }
                        else if (mode < 0)
                        {
                            adjust = -mode;
                        }
                    }
                    f1.adjust_f1 = adjust;
                    start_flag1 =5;
                    textBox1.Text = Convert.ToString(adjust);
                }
            }
            //**************************************************************Noise***********************************
            if (start_flag2 == 1)
            {
                for (int count = 0; count < 3999; count++)
                {

                    if (sPort.IsOpen)
                    {
                        if (start_flag2 == 1)
                        {
                            start_byte = sPort.ReadByte();
                        }
                    }
                    if (start_byte == 0x81)
                    {
                        start_flag2 = 2;
                        data_buff[data_count] = sPort.ReadByte();

                        data_count++;

                        if (data_count == 4)
                        {
                            Data_1 = ((data_buff[0] & 0x7f) << 7) + (data_buff[1] & 0x7f);
                            start_flag2 = 3;
                            data_count = 0;
                            d++;
                        }


                        if (start_flag2 == 3)
                        {

                            for (int i = 0; i < buffsize - 1; i++)
                            {
                                input_Data_1[i] = input_Data_1[i + 1];
                            }
                            input_Data_1[buffsize - 1] = (Data_1 - 7000 - 4350 + 250) * 10 - adjust;
                            score[d - 1] = input_Data_1[buffsize - 1];
                            input_Draw_1 = input_Data_1;
                            start_flag2 = 1;

                        }

                    }

                }
                start_flag2 = 4;
                if (start_flag2 == 4)
                {
                    double[] max_value = new double[5];
                    double[] min_value = new double[5];
                    Int32 Max = Int32.MinValue;
                    Int32 Min = Int32.MaxValue;
                    for (int p = 0; p < 1000; p++)
                    {
                        calibration[p] = score[p];
                    }
                    for (int j = 1; j < 6; j++) 
                    {
                        for (int p = 200*(j-1); (p >= (j - 1) * 200 && p <= (j * 200 - 1)); p++)
                        {
                            if (calibration[p] > 0)
                            {
                                if (calibration[p] > Max)
                                {
                                    Max = (int)calibration[p];
                                    max_value[j - 1] = Max;
                                }
                            }
                            else
                            {
                                if (calibration[p] < Min)
                                {
                                    Min = (int)calibration[p];
                                    min_value[j-1] = Min;
                                }
                            }
                        }
                    }
                    avg_noise_P = max_value.Average();
                    avg_noise_N = min_value.Average();
                    f1.avg_noise_P_f1 = avg_noise_P;
                    f1.avg_noise_N_f1 = avg_noise_N;
                    Array.Clear(max_value, 0, max_value.Length);
                    Array.Clear(min_value, 0, min_value.Length);
                    textBox2.Text = Convert.ToString(avg_noise_P);
                    textBox3.Text = Convert.ToString(avg_noise_N);
                    start_flag2 = 5;
                }
            }

            //**********************************************Signal************************************
            if (start_flag3 == 1)
            {
                for (int count = 0; count < 3999; count++)
                {

                    if (sPort.IsOpen)
                    {
                        if (start_flag3 == 1)
                        {
                            start_byte = sPort.ReadByte();
                        }
                    }
                    if (start_byte == 0x81)
                    {
                        start_flag3 = 2;
                        data_buff[data_count] = sPort.ReadByte();

                        data_count++;

                        if (data_count == 4)
                        {
                            Data_1 = ((data_buff[0] & 0x7f) << 7) + (data_buff[1] & 0x7f);
                            start_flag3 = 3;
                            data_count = 0;
                            f++;
                        }


                        if (start_flag3 == 3)
                        {

                            for (int i = 0; i < buffsize - 1; i++)
                            {
                                input_Data_1[i] = input_Data_1[i + 1];
                            }


                            input_Data_1[buffsize - 1] = (Data_1 - 7000 - 4350 + 250) * 10 - adjust;
                            score[f - 1] = input_Data_1[buffsize - 1];
                            input_Draw_1 = input_Data_1;

                            start_flag3 = 1;

                        }

                    }

                }
                start_flag3 = 4;

                if (start_flag3 == 4)
                {
                    double[] max_value = new double[2000];
                    double[] min_value = new double[2000];
                    Int32 Max = Int32.MinValue;
                    Int32 Min = Int32.MaxValue;
                    for (int p = 0; p < 1000; p++)
                    {
                        calibration[p] = score[p];
                    }
                    for (int p = 0; p < 1000; p++)
                    {
                        if (calibration[p] > 0)
                        {
                            if (calibration[p] > Max)
                            {
                                Max = (int)calibration[p];
                            }
                        }
                        else
                        {
                            if (calibration[p] < Min)
                            {
                                Min = (int)calibration[p];
                            }
                        }
                    }
                    avg_signal_P = Max;
                    avg_signal_N = Min;
                    Array.Clear(max_value, 0, max_value.Length);
                    Array.Clear(min_value, 0, min_value.Length);
                    textBox4.Text = Convert.ToString(avg_signal_P);
                    textBox5.Text = Convert.ToString(avg_signal_N);
                    f1.avg_signal_P_f1 = avg_signal_P;
                    f1.avg_signal_N_f1 = avg_signal_N;
                    start_flag3 = 5;
                }

                if (start_flag3 == 5)
                {
                    threshold_N = (avg_noise_N + avg_signal_N) / 2;
                    threshold_P = (avg_noise_P + avg_signal_P) / 2;
                    f1.threshold_P_f1 = threshold_P;
                    f1.threshold_N_f1 = threshold_N; 
                    textBox6.Text = Convert.ToString(threshold_P);
                    textBox7.Text = Convert.ToString(threshold_N);
                    start_flag3 = 6;
                }
            }
        }

        private void On_timer1(object sender, EventArgs e)
        {
            scope1.Channels[0].Data.SetYData(input_Data_1);
        }

        private void Restart_Click(object sender, EventArgs e)
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
            this.Hide();
            f1.timer2.Start();
        }
    }
}
