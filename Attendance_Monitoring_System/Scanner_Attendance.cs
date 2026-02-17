using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ClosedXML.Excel;
using System.IO;


namespace Attendance_Monitoring_System
{
    public partial class Scanner_Attendance : Form
    {
        string supabaseUrl = "https://klydsxazcmxavgqvxrjv.supabase.co";
        string supabaseKey = "sb_publishable_By0K2pvbnVBRQ8tp_Ny-dg_qCExPABw";

        public Scanner_Attendance()
        {
            InitializeComponent();
            txtScan.Focus();
        }

        private async void txtScan_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                string rawCode = txtScan.Text.Trim();

                if (rawCode.Length != 10)
                {
                    MessageBox.Show("warning, scanned code is wrong", "input rejected");
                    txtScan.Clear();
                    return;
                }


                await SendScan(rawCode);
                await LoadAttendance();

                txtScan.Clear();
            }
        }

        async Task SendScan(string rawCode)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("apikey", supabaseKey);
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {supabaseKey}");

                var content = new StringContent(
                    JsonConvert.SerializeObject(new { raw_code = rawCode }),
                    Encoding.UTF8,
                    "application/json"
                );

                await client.PostAsync(
                    $"{supabaseUrl}/rest/v1/rpc/log_attendance",
                    content
                );
            }
        }

        async Task LoadAttendance()
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("apikey", supabaseKey);
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {supabaseKey}");

                var response = await client.GetStringAsync(
                    $"{supabaseUrl}/rest/v1/attendance_today_view"
                );

                var data = JsonConvert.DeserializeObject(response);

                gridAttendance.DataSource = data;
                gridAttendance.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                ExportToExcel(data);
            }
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            await LoadAttendance();
        }

        void ExportToExcel(dynamic data)
        {
            string folder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "AttendanceLogs"
            );

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            string filePath = Path.Combine(
                folder,
                $"Attendance_{DateTime.Now:yyyy-MM-dd}.xlsx"
            );

            using (var workbook = new XLWorkbook())
            {
                var sheet = workbook.Worksheets.Add("Attendance");

                // headers
                sheet.Cell(1, 1).Value = "First Name";
                sheet.Cell(1, 2).Value = "Last Name";
                sheet.Cell(1, 3).Value = "Time In";
                sheet.Cell(1, 4).Value = "Time Out";

                int row = 2;

                foreach (var item in data)
                {
                    sheet.Cell(row, 1).Value = item.first_name;
                    sheet.Cell(row, 2).Value = item.last_name;
                    sheet.Cell(row, 3).Value = item.time_in;
                    sheet.Cell(row, 4).Value = item.time_out;
                    row++;
                }

                sheet.Columns().AdjustToContents();

                workbook.SaveAs(filePath);
            }
        }

        private void Scanner_Attendance_FormClosing(object sender, FormClosingEventArgs e)
        {
            Menu menu = new Menu();
            menu.Show();
        }


    }
}