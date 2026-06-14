using AForge.Video;
using AForge.Video.DirectShow;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ZXing;

namespace Attendance_Monitoring_System
{
    public partial class Camera_Attendace : Form
    {
        private static readonly TimeSpan DuplicateScanWindow = TimeSpan.FromSeconds(10);
        private static readonly TimeSpan MinimumDecodeInterval = TimeSpan.FromMilliseconds(250);

        private readonly Dictionary<string, DateTime> lastSuccessfulScans =
            new Dictionary<string, DateTime>(StringComparer.Ordinal);

        private VideoCaptureDevice camera;
        private int processing;
        private long nextDecodeTicks;
        private volatile bool closing;

        public Camera_Attendace()
        {
            InitializeComponent();
            ConfigureAttendanceGrid();
            btnEnd.Enabled = false;
        }

        private async void Camera_Attendance_Load(object sender, EventArgs e)
        {
            try
            {
                await LoadAttendance();
            }
            catch
            {
                if (!closing)
                {
                    lblStatus.Text = "Failed to load attendance";
                }
            }
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (camera != null && camera.IsRunning)
            {
                lblStatus.Text = "Camera is already running";
                return;
            }

            try
            {
                StopCamera();
                var cameras = new FilterInfoCollection(FilterCategory.VideoInputDevice);

                if (cameras.Count == 0)
                {
                    lblStatus.Text = "No camera found";
                    return;
                }

                camera = new VideoCaptureDevice(cameras[0].MonikerString);
                camera.NewFrame += Camera_NewFrame;
                camera.Start();

                btnStart.Enabled = false;
                btnEnd.Enabled = true;
                lblStatus.Text = "Camera started";
            }
            catch
            {
                StopCamera();
                lblStatus.Text = "Unable to start camera";
            }
        }

        private void btnEnd_Click(object sender, EventArgs e)
        {
            StopCamera();
            lblStatus.Text = "Camera stopped";
        }

        private void Camera_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            if (closing)
            {
                return;
            }

            try
            {
                ShowPreview((Bitmap)eventArgs.Frame.Clone());
            }
            catch
            {
                return;
            }

            long nowTicks = DateTime.UtcNow.Ticks;
            if (nowTicks < Interlocked.Read(ref nextDecodeTicks))
            {
                return;
            }

            if (Interlocked.CompareExchange(ref processing, 1, 0) != 0)
            {
                return;
            }

            Interlocked.Exchange(ref nextDecodeTicks, nowTicks + MinimumDecodeInterval.Ticks);

            try
            {
                var decodeBitmap = (Bitmap)eventArgs.Frame.Clone();
                Task.Run(() => DecodeFrame(decodeBitmap));
            }
            catch
            {
                ReleaseProcessing();
            }
        }

        private void ShowPreview(Bitmap frame)
        {
            try
            {
                BeginInvoke(new Action(() =>
                {
                    if (closing || IsDisposed)
                    {
                        frame.Dispose();
                        return;
                    }

                    var oldFrame = cameraBox.Image;
                    cameraBox.Image = frame;
                    if (oldFrame != null)
                    {
                        oldFrame.Dispose();
                    }
                }));
            }
            catch
            {
                frame.Dispose();
            }
        }

        private void DecodeFrame(Bitmap decodeBitmap)
        {
            string rawCode = null;

            try
            {
                var reader = new BarcodeReader
                {
                    AutoRotate = true,
                    Options = { TryHarder = true }
                };

                var result = reader.Decode(decodeBitmap);
                if (result != null
                    && ScanCodeValidator.IsValid(result.Text)
                    && !WasRecentlyScanned(result.Text))
                {
                    rawCode = result.Text;
                }
            }
            catch
            {
                // A bad frame should not stop future scans.
            }
            finally
            {
                decodeBitmap.Dispose();
            }

            if (rawCode == null || closing)
            {
                ReleaseProcessing();
                return;
            }

            try
            {
                BeginInvoke(new Action(() => ProcessScan(rawCode)));
            }
            catch
            {
                ReleaseProcessing();
            }
        }

        private async void ProcessScan(string rawCode)
        {
            try
            {
                if (closing)
                {
                    return;
                }

                lblStatus.Text = "Confirming attendance...";

                try
                {
                    await AttendanceApiClient.LogAttendanceAsync(rawCode);
                }
                catch
                {
                    if (!closing)
                    {
                        lblStatus.Text = "Attendance was not recorded. Please scan again.";
                    }

                    return;
                }

                if (closing)
                {
                    return;
                }

                RememberSuccessfulScan(rawCode);
                txtTime.Text = DateTime.Now.ToString("hh:mm:ss tt");
                System.Media.SystemSounds.Beep.Play();

                try
                {
                    await LoadAttendance();

                    if (!closing)
                    {
                        lblStatus.Text = "Attendance recorded";
                    }
                }
                catch
                {
                    if (!closing)
                    {
                        lblStatus.Text = "Attendance recorded, but the list could not refresh";
                    }
                }
            }
            finally
            {
                ReleaseProcessing();
            }
        }

        private async Task LoadAttendance()
        {
            gridAttendance.DataSource = await AttendanceApiClient.GetTodayAsync();
        }

        private bool WasRecentlyScanned(string rawCode)
        {
            DateTime now = DateTime.UtcNow;
            DateTime cutoff = now - DuplicateScanWindow;
            var staleCodes = new List<string>();

            foreach (var scan in lastSuccessfulScans)
            {
                if (scan.Value <= cutoff)
                {
                    staleCodes.Add(scan.Key);
                }
            }

            foreach (string staleCode in staleCodes)
            {
                lastSuccessfulScans.Remove(staleCode);
            }

            DateTime lastScan;
            return lastSuccessfulScans.TryGetValue(rawCode, out lastScan)
                && now - lastScan < DuplicateScanWindow;
        }

        private void RememberSuccessfulScan(string rawCode)
        {
            lastSuccessfulScans[rawCode] = DateTime.UtcNow;
        }

        private void StopCamera()
        {
            var activeCamera = camera;
            camera = null;

            if (activeCamera != null)
            {
                activeCamera.NewFrame -= Camera_NewFrame;

                try
                {
                    if (activeCamera.IsRunning)
                    {
                        activeCamera.SignalToStop();
                        activeCamera.WaitForStop();
                    }
                }
                catch
                {
                    // Camera drivers can disappear while the application is shutting down.
                }
            }

            btnStart.Enabled = true;
            btnEnd.Enabled = false;
        }

        private void ConfigureAttendanceGrid()
        {
            gridAttendance.AllowUserToAddRows = false;
            gridAttendance.AllowUserToDeleteRows = false;
            gridAttendance.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            gridAttendance.ReadOnly = true;
        }

        private void ReleaseProcessing()
        {
            Interlocked.Exchange(ref processing, 0);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            closing = true;
            StopCamera();

            var oldFrame = cameraBox.Image;
            cameraBox.Image = null;
            if (oldFrame != null)
            {
                oldFrame.Dispose();
            }

            base.OnFormClosing(e);
        }
    }
}
