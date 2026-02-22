using AForge.Video;
using AForge.Video.DirectShow;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;
using ZXing;
using ClosedXML.Excel;
using System.IO;


namespace Attendance_Monitoring_System
{
    public partial class Camera_Attendace : Form
    {
        string supabaseUrl = "https://klydsxazcmxavgqvxrjv.supabase.co";
        string supabaseKey = "sb_publishable_By0K2pvbnVBRQ8tp_Ny-dg_qCExPABw";

        FilterInfoCollection cameras;
        VideoCaptureDevice camera;
        bool processing = false;

        public Camera_Attendace()
        {
            InitializeComponent();
        }

        private async void Camera_Attendance_Load(object sender, EventArgs e)
        {
            await LoadAttendance();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            cameras = new FilterInfoCollection(FilterCategory.VideoInputDevice);

            if (cameras.Count == 0)
            {
                lblStatus.Text = "No camera found";
                return;
            }

            camera = new VideoCaptureDevice(cameras[0].MonikerString);
            camera.NewFrame += Camera_NewFrame;
            camera.Start();

            lblStatus.Text = "Camera started";

        }

        private void btnEnd_Click(object sender, EventArgs e)
        {
            if (camera != null && camera.IsRunning)
            {
                camera.SignalToStop();
                camera.WaitForStop();
            }

            lblStatus.Text = "Camera stopped";
        }

        private void Camera_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            Bitmap bitmap = (Bitmap)eventArgs.Frame.Clone();
            cameraBox.Image = bitmap;

            if (processing) return;

            BarcodeReader reader = new BarcodeReader();
            var result = reader.Decode(bitmap);

            if (result != null && result.Text.Length == 10)
            {
                processing = true;

                this.Invoke(new Action(async () =>
                {
                    lblStatus.Text = "Scanned: " + result.Text;
                    txtTime.Text = DateTime.Now.ToString("hh:mm:ss tt");

                    await SendScan(result.Text);
                    await LoadAttendance();

                    System.Media.SystemSounds.Beep.Play();

                    await Task.Delay(2000);
                    processing = false;
                }));
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
        void StopCamera()
        {
            if (camera != null && camera.IsRunning)
            {
                camera.SignalToStop();
                camera.WaitForStop();
                lblStatus.Text = "";
                txtTime.Clear();
            }
        }
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (camera != null && camera.IsRunning)
                camera.SignalToStop();

            base.OnFormClosing(e);
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
                    sheet.Cell(row, 3).Value = Convert.ToDateTime(item.time_in).ToString("hh:mm tt");
                    sheet.Cell(row, 3).Value = Convert.ToDateTime(item.time_out).ToString("hh:mm tt");

                    row++;
                }

                sheet.Columns().AdjustToContents();

                workbook.SaveAs(filePath);
            }
        }

        private void Camera_Attendace_FormClosing(object sender, FormClosingEventArgs e)
        {
            Menu menu = new Menu();
            menu.Show();
        }
    }
}
