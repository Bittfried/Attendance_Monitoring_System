using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Attendance_Monitoring_System
{
    public partial class Scanner_Attendance : Form
    {
        string supabaseUrl = "https://klydsxazcmxavgqvxrjv.supabase.co";
        string supabaseKey = "sb_publishable_By0K2pvbnVBRQ8tp_Ny-dg_qCExPABw";

        private static readonly HttpClient client = new HttpClient();

        public Scanner_Attendance()
        {
            InitializeComponent();

            if (!client.DefaultRequestHeaders.Contains("apikey"))
                client.DefaultRequestHeaders.Add("apikey", supabaseKey);

            if (!client.DefaultRequestHeaders.Contains("Authorization"))
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {supabaseKey}");

            this.Load += Form1_Load;

            txtScan.Focus();
        }

        private async void txtScan_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
                return;

            e.SuppressKeyPress = true;

            string rawCode = txtScan.Text.Trim();

            txtTime.Text = DateTime.Now.ToString("hh:mm:ss tt");

            if (rawCode.Length != 10)
            {
                MessageBox.Show("Warning: scanned code is wrong", "Input Rejected");
                txtScan.Clear();
                txtScan.Focus();
                return;
            }

            try
            {
                await SendScan(rawCode);
                await LoadAttendance();
            }
            catch
            {
                MessageBox.Show("Network error while sending attendance.");
            }

            txtTime.Clear();
            txtScan.Clear();
            txtScan.Focus();
        }

        async Task SendScan(string rawCode)
        {
            var content = new StringContent(
                JsonConvert.SerializeObject(new { raw_code = rawCode }),
                Encoding.UTF8,
                "application/json"
            );

            var response = await client.PostAsync(
                $"{supabaseUrl}/rest/v1/rpc/log_attendance",
                content
            );

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Failed to log attendance.");
            }
        }

        async Task LoadAttendance()
        {
            try
            {
                var response = await client.GetStringAsync(
                    $"{supabaseUrl}/rest/v1/attendance_today_view"
                );

                var data = JsonConvert.DeserializeObject<List<Attendance>>(response);

                gridAttendance.DataSource = data;
                gridAttendance.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            }
            catch
            {
                MessageBox.Show("Failed to load attendance.");
            }
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            await LoadAttendance();
        }

        private void Scanner_Attendance_FormClosing(object sender, FormClosingEventArgs e)
        {
            Menu menu = new Menu();
            menu.Show();
        }
    }


}