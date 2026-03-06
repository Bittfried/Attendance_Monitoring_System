using AForge.Video;
using AForge.Video.DirectShow;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ZXing;

namespace Attendance_Monitoring_System
{
    public partial class Camera_Attendace : Form
    {
        string supabaseUrl = "https://klydsxazcmxavgqvxrjv.supabase.co";
        string supabaseKey = "sb_publishable_By0K2pvbnVBRQ8tp_Ny-dg_qCExPABw";

        private static readonly HttpClient client = new HttpClient();

        FilterInfoCollection cameras;
        VideoCaptureDevice camera;

        bool processing = false;

        Dictionary<string, DateTime> lastScan = new Dictionary<string, DateTime>();

        public Camera_Attendace()
        {
            InitializeComponent();

            if (!client.DefaultRequestHeaders.Contains("apikey"))
                client.DefaultRequestHeaders.Add("apikey", supabaseKey);

            if (!client.DefaultRequestHeaders.Contains("Authorization"))
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {supabaseKey}");
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
            Bitmap frame = (Bitmap)eventArgs.Frame.Clone();
            Bitmap decodeBitmap = (Bitmap)frame.Clone();

            if (this.IsHandleCreated && !this.IsDisposed)
            {
                try
                {
                    this.BeginInvoke(new Action(() =>
                    {
                        var old = cameraBox.Image;
                        cameraBox.Image = frame;
                        old?.Dispose();
                    }));
                }
                catch
                {
                    frame.Dispose();
                }
            }
            else
            {
                frame.Dispose();
            }

            lock (this)
            {
                if (processing)
                {
                    decodeBitmap.Dispose();
                    return;
                }
                processing = true;
            }

            Task.Run(async () =>
            {
                try
                {
                    var reader = new BarcodeReader
                    {
                        AutoRotate = true,
                        Options = { TryHarder = true }
                    };

                    var result = reader.Decode(decodeBitmap);

                    if (result != null && result.Text.Length == 10)
                    {
                        if (lastScan.ContainsKey(result.Text))
                        {
                            if ((DateTime.Now - lastScan[result.Text]).TotalSeconds < 10)
                            {
                                processing = false;
                                return;
                            }
                        }

                        lastScan[result.Text] = DateTime.Now;

                        this.BeginInvoke(new Action(async () =>
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
                    else
                    {
                        processing = false;
                    }
                }
                catch
                {
                    processing = false;
                }
                finally
                {
                    decodeBitmap.Dispose();
                }
            });
        }

        async Task SendScan(string rawCode)
        {
            try
            {
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
            catch
            {
                lblStatus.Text = "Network error";
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
                lblStatus.Text = "Failed to load attendance";
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (camera != null)
            {
                camera.NewFrame -= Camera_NewFrame;

                if (camera.IsRunning)
                {
                    camera.SignalToStop();
                    camera.WaitForStop();
                }

                camera = null;
            }

            base.OnFormClosing(e);
        }

        private void Camera_Attendace_FormClosing(object sender, FormClosingEventArgs e)
        {
            Menu menu = new Menu();
            menu.Show();
        }
    }

    public class Attendance
    {
        public string first_name { get; set; }
        public string last_name { get; set; }
        public DateTime? time_in { get; set; }
        public DateTime? time_out { get; set; }
    }
}