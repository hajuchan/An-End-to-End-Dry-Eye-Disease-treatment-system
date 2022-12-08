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
//using System.Diagnostics; //Stopwatch 기능
using System.Collections;




namespace Test_2
{
    public delegate void DataPushEventHandler(string item);//폼2에 전달
    public partial class Form1 : Form
    {
        public DataPushEventHandler DataSendEvent;
        static public bool Form2_on;
        SerialPort sPort;
        int[] data_buff = new int[200];
        static int buffsize = 2000;
        double[] input_Data_1 = new double[buffsize];
        double[] input_Data_2 = new double[buffsize];
        double[] score = new double[2000];
        double[] calibration_data = new double[2000];
        public double[] input_Draw_1 = new double[buffsize];
        public double[] a = new double[buffsize];
        public double avg_noise_P = 0;
        public double avg_noise_N = 0;
        public double avg_signal_P = 0;
        public double avg_signal_N = 0;
        public double threshold_P = 0;
        public double threshold_N = 0;
        int start_byte = 0;
        int start_flag = 0;
        int start_flag2 = 0;
        int data_count = 0;
        int Data_1;
        int Data_2;
        //int score;
        int blink = 0; //눈 깜빡인 횟수
        int upcheck = 0;
        int downcheck = 0;
        public double adjust = 0;
        int s = 0;
        double nTotalSeconds = 0;
        DateTime dt;
        public double B ;
        public int k;
        public double IBI = 0;



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
            if(checkBox5.Checked)
            {
                for (int count = 0; count < 7999; count++)
                {
                   
                    if (sPort.IsOpen)
                    {
                        if (start_flag2 == 0)
                        {
                            start_byte = sPort.ReadByte();
                        }
                    }
                    if (start_byte == 0x81) 
                    {
                        start_flag2 = 1;
                        data_buff[data_count] = sPort.ReadByte();

                        data_count++;

                        if (data_count == 4)
                        {
                            Data_2 = ((data_buff[0] & 0x7f) << 7) + (data_buff[1] & 0x7f);
                            start_flag2 = 2;
                            data_count = 0;
                            s++;
                        }


                        if (start_flag2 == 2)
                        {

                            for (int i = 0; i < buffsize - 1; i++)
                            {
                                input_Data_2[i] = input_Data_2[i + 1];
                            }


                            input_Data_2[buffsize - 1] = (Data_2 - 7000 - 4350 + 250) * 10; // 평균 적인 값을 처리한 과정
                            //textBox3.Text = Convert.ToString(input_Data_2[buffsize - 1]);
                            score[s - 1] = input_Data_2[buffsize - 1];

                            start_flag2 = 0;
                            //textBox3.Text = Convert.ToString(score.Length);

                        }

                    }
                    
                }
                start_flag2 = 5;


                //************************************** Obtaining Mode *******************
                if (start_flag2 == 5)
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
                            max = index[a]; // MAX
                            mode = a;
                            //textBox3.Text = Convert.ToString(mode);
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
                    //textBox3.Text = Convert.ToString(adjust);
                    start_flag2 = 6;
                    checkBox5.Checked = false;
                    checkBox2.Checked = true;
                }

                /*if (start_flag2 == 6)
                {
                    //************************ MAX and MIN value *********************
                    for (int p = 0; p < calibration_data.Length; p++)
                    {
                        double[] max_value = new double[2];
                        double[] min_value = new double[2];
                        Int32 Max = Int32.MinValue;
                        Int32 Min = Int32.MaxValue;
                        calibration_data[p] = score[p] - adjust;
                        if (p < 1000)
                        {

                            for (i = 1; i < 3; i++)
                            {
                                if (p >= (i - 1) * 500 && p <= (i * 500 - 1))
                                {
                                    if (calibration_data[p] > 0)
                                    {
                                        if (calibration_data[p] > Max)
                                        {
                                            Max = (int)calibration_data[p]; // MAX
                                        }
                                        max_value.Append(Max);
                                    }
                                    else
                                    {
                                        if (calibration_data[p] < 0)
                                        {
                                            if (calibration_data[p] < Min)
                                            {
                                                Min = (int)calibration_data[p]; // MAX
                                            }
                                            min_value.Append(Min);
                                        }
                                    }
                                }
                            }
                            avg_noise_P = max_value.Average();
                            avg_noise_N = min_value.Average();
                            Array.Clear(max_value, 0, max_value.Length);
                            Array.Clear(min_value, 0, min_value.Length);
                        }
                        else
                        {
                            for (i = 3; i < 5; i++)
                            {
                                if (p >= (i - 1) * 500 && p <= (i * 500 - 1))
                                {
                                    if (calibration_data[s] > 0)
                                    {
                                        if (calibration_data[s] > Max)
                                        {
                                            Max = (int)calibration_data[s]; // MAX
                                        }
                                        max_value.Append(Max);
                                    }
                                }
                                else
                                {
                                    if (calibration_data[s] < 0)
                                    {
                                        if (calibration_data[s] < Min)
                                        {
                                            Min = (int)calibration_data[s]; // MAX
                                        }
                                        min_value.Append(Min);
                                    }
                                }
                                avg_signal_P = max_value.Average();
                                avg_signal_N = min_value.Average();
                                Array.Clear(max_value, 0, max_value.Length);
                                Array.Clear(min_value, 0, min_value.Length);
                            }
                        }
                    }
                    threshold_N = (avg_noise_N + avg_signal_N) / 2;
                    threshold_P = (avg_noise_P + avg_signal_P) / 2;
                    //textBox3.Text = Convert.ToString(calibration_data[500]);
                    checkBox5.Checked = false;
                    checkBox2.Checked = true;
                    start_flag2 = 7;
                }*/

                //****************************************************************
                /*threshold_N = (avg_noise_N + avg_signal_N) / 2;
                threshold_P = (avg_noise_P + avg_signal_P) / 2;*/
                // checkBox5.Checked = false;
                // checkBox2.Checked = true;
                //start_flag2 = 5;
                textBox1.Text = "0";
                textBox2.Text = "0";
                nTotalSeconds= 0;
                textBox3.Text = Convert.ToString(adjust);
                //textBox3.Text = Convert.ToString(threshold_N);
            }

            if (start_flag2 == 6) { 
                if (checkBox2.Checked)
                {
                    //textBox3.Text = Convert.ToString(sPort.BytesToRead);
                    while (sPort.BytesToRead > 0)
                    {
                        if (sPort.IsOpen)
                        {
                            if (start_flag == 0)
                            {
                                start_byte = sPort.ReadByte();
                            }
                        }
                        if (start_byte == 0x81) //
                        {
                            if (upcheck == 1 && downcheck == 1) // +20 이상의 피크를 갖고 0 이하로 값이 떨어지면 blink 인식
                            {
                                blink++;
                                upcheck = 0;
                                downcheck = 0; // 이후의 횟수 인식을 위해 초기화
                            }
                            B = blink/2;
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


                                input_Data_1[buffsize - 1] = (Data_1 - 7000 - 4350 + 250) * 10 - adjust;// 평균 적인 값을 처리한 과정
                                if (input_Data_1[buffsize - 1] < 25 && input_Data_1[buffsize - 1] > -25)
                                {
                                    input_Data_1[buffsize - 1] = input_Data_1[buffsize - 1] / 10;
                                }
                                else
                                {
                                    input_Data_1[buffsize - 1] = input_Data_1[buffsize - 1] * 2;
                                }

                                input_Draw_1 = input_Data_1;

                                //textBox3.Text = Convert.ToString(adjust);

                                if (input_Data_1[buffsize - 1] > 40)
                                {
                                    upcheck = 1; // blink 횟수 체크
                                }
                                else if ((input_Data_1[buffsize - 1] < -40))
                                {
                                    downcheck = 1; // blink 횟수 체크 변수
                                }
                                start_flag = 0;
                            }
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
                    nTotalSeconds++; // blink rate를 재는데 기준이 되는 시간 표시
                    textBox2.Text = dt.AddSeconds(nTotalSeconds).ToString("ss"); //시간
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
                    //Form2_on = false;
                    textBox1.Text = "0";
                    textBox2.Text = "0";
                    //this.Enabled = false;
                    //timer2.Enabled = true;
                    
                    checkBox1.Checked = true;
                   
                   //timer2.Start();
                }
                else if (B > 2 && B < 4)
                {
                    IBI = 30 / B;
                    k = 6;
                    blink = 0;
                    nTotalSeconds = 0;
                    //Form2_on = false;
                    textBox1.Text = "0";
                    textBox2.Text = "0";
                    //this.Enabled = false;
                    //timer2.Enabled = true;

                    checkBox1.Checked = true;

                    //timer2.Start();
                }
                else if (B > 3 && B < 5)
                {
                    IBI = 30 / B;
                    k = 7;
                    blink = 0;
                    nTotalSeconds = 0;
                    //Form2_on = false;
                    textBox1.Text = "0";
                    textBox2.Text = "0";
                    //this.Enabled = false;
                    //timer2.Enabled = true;

                    checkBox1.Checked = true;

                    //timer2.Start();
                }
                else if (B > 4)
                {
                    IBI = 30 / B;
                    k = 7;
                    blink = 0;
                    nTotalSeconds = 0;
                    //Form2_on = false;
                    textBox1.Text = "0";
                    textBox2.Text = "0";
                    //this.Enabled = false;
                    //timer2.Enabled = true;

                    checkBox1.Checked = true;

                    //timer2.Start();
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

                //timer2.Enabled = false;
            
                
             }
           
           

            //else
            //{
            //   blink = 0;
            //    nTotalSeconds = 0;
            //   textBox2.Text = dt.AddSeconds(nTotalSeconds).ToString("ss"); //시간
            //    timer2.Enabled = true;
            //}
        }


        //Form2 발동
        //Form2 second = new Form2(this);
        private void checkBox1_CheckedChanged(object sender, EventArgs e) 
        {
            if (checkBox1.Checked) //B>4 check
            {
                textBox1.Text = "0";
                B = 0;
                Form2 second = new Form2(this);
                second.Show();
                nTotalSeconds = 0;
                nTotalSeconds++;
                checkBox2.Checked = false;
                //timer2.Interval = 1000;
                //timer2.Start();
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
            //start_flag2 = 5;
           // nTotalSeconds++; // blink rate를 재는데 기준이 되는 시간 표시
           // textBox2.Text = dt.AddSeconds(nTotalSeconds).ToString("ss"); //시간
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

            cboPortName.SelectedItem = "COM3";
            txtBaudRate.Text = "115200";

            CheckForIllegalCrossThreadCalls = false;
            txtDate.Text = thisdate;
        }

        public Form1()
        {
            InitializeComponent();

        }

    }
}
