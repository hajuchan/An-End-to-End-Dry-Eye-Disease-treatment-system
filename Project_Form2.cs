using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Test_2
{
    public partial class Form2 : Form
    {
        Form1 f1;
        DateTime dt;
        double stimulseconds = 5;
        //double Form2_off;
        public Form2()
        {
            InitializeComponent();
        }

        public Form2(Form1 form)
        {
            InitializeComponent();
            f1 = form;
        }
        private void Form2_Load(object sender, EventArgs e)
        {

        }

        public void SetActionValue1(string param2)
        {
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (f1.k > 4 && f1.k < 6)
            {
                Current.Text = "1mA 자극";
            }
            else if (f1.k > 5 && f1.k < 7)
            {
                Current.Text = "2mA 자극";
            }
            else if (f1.k > 6 && f1.k < 8)
            {
                Current.Text = "3mA 자극";
            }
            else if (f1.k > 7)
            {
                Current.Text = "4mA 자극";
            }
            dt = new DateTime();
            timer1.Interval = 1000;
            timer1.Enabled = true;
            f1.k = 0;
            Interval.Text = Convert.ToString(f1.IBI);
         }

        

        private void Ttimer1(object sender, EventArgs e)
        {
            timer1.Enabled = true;
            stimulseconds--;
            textBox1.Text = dt.AddSeconds(stimulseconds).ToString("ss");
            if(stimulseconds == 0)
            {
                stimulseconds = 5;

                this.Hide();
                f1.textBox1.Text = "0";
                f1.checkBox1.Checked = false;
                timer1.Enabled = false;
                f1.timer2.Interval = 1000;
                f1.timer2.Start();
                
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }
    }
}
