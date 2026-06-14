using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Attendance_Monitoring_System
{
    public partial class Scanner_Attendance : Form
    {
        private bool processing;
        private bool closing;

        public Scanner_Attendance()
        {
            InitializeComponent();
            ConfigureAttendanceGrid();
        }

        private async void txtScan_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter || processing)
            {
                return;
            }

            e.SuppressKeyPress = true;

            string rawCode = txtScan.Text.Trim();

            if (!ScanCodeValidator.IsValid(rawCode))
            {
                MessageBox.Show(
                    "The scanned code must contain exactly 10 letters or digits.",
                    "Input Rejected");
                txtScan.Clear();
                txtScan.Focus();
                return;
            }

            processing = true;
            txtScan.Enabled = false;

            try
            {
                try
                {
                    await AttendanceApiClient.LogAttendanceAsync(rawCode);
                }
                catch
                {
                    if (!closing)
                    {
                        MessageBox.Show(
                            "Attendance was not confirmed by the server. Please scan again.",
                            "Attendance Not Recorded");
                    }

                    return;
                }

                if (closing)
                {
                    return;
                }

                txtTime.Text = DateTime.Now.ToString("hh:mm:ss tt");

                try
                {
                    await LoadAttendance();
                }
                catch
                {
                    if (!closing)
                    {
                        MessageBox.Show(
                            "Attendance was recorded, but the list could not refresh.",
                            "Refresh Failed");
                    }
                }
            }
            finally
            {
                processing = false;

                if (!closing && !IsDisposed)
                {
                    txtScan.Enabled = true;
                    txtScan.Clear();
                    txtScan.Focus();
                }
            }
        }

        private async Task LoadAttendance()
        {
            gridAttendance.DataSource = await AttendanceApiClient.GetTodayAsync();
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                await LoadAttendance();
            }
            catch
            {
                if (!closing)
                {
                    MessageBox.Show("Failed to load attendance.", "Network Error");
                }
            }

            if (!closing)
            {
                txtScan.Focus();
            }
        }

        private void ConfigureAttendanceGrid()
        {
            gridAttendance.AllowUserToAddRows = false;
            gridAttendance.AllowUserToDeleteRows = false;
            gridAttendance.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            gridAttendance.ReadOnly = true;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            closing = true;
            base.OnFormClosing(e);
        }
    }
}
