using System;
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
            ShowScanner(new Scanner_Attendance());
        }

        private void s_Click(object sender, EventArgs e)
        {
            ShowScanner(new Camera_Attendace());
        }

        private void ShowScanner(Form scanner)
        {
            Hide();

            try
            {
                scanner.ShowDialog();
            }
            finally
            {
                scanner.Dispose();
                Show();
                Activate();
            }
        }
    }
}
