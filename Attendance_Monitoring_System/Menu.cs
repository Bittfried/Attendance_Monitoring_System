using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Attendance_Monitoring_System
{
    public partial class Menu : Form
    {
        public Menu()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Scanner_Attendance scanner_Attendance = new Scanner_Attendance();
            scanner_Attendance.Show(); ;
            this.Hide();
            
        }

        private void s_Click(object sender, EventArgs e)
        {
            Camera_Attendace camera_Attendace = new Camera_Attendace();
            camera_Attendace.Show();
            this.Hide();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
        }

        private void Menu_FormClosing(object sender, FormClosingEventArgs e)
        {

        }
    }
}
