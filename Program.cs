using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace HzChanger
{
    public class HzForm : Form
    {
        private Label lblCurrentHz;
        private Button btnLowHz;
        private Button btnMaxHz;
        private Button btnToggle;

        private int currentHz;
        private int maxSupportedHz;
        private const int DEFAULT_LOW_HZ = 60;

        [DllImport("user32.dll")]
        private static extern bool EnumDisplaySettings(string? deviceName, int modeNum, ref DEVMODE devMode);

        [DllImport("user32.dll")]
        private static extern int ChangeDisplaySettings(ref DEVMODE devMode, int flags);

        private const int ENUM_CURRENT_SETTINGS = -1;
        private const int CDS_UPDATEREGISTRY = 0x01;
        private const int DISP_CHANGE_SUCCESSFUL = 0;

        [StructLayout(LayoutKind.Sequential)]
        public struct DEVMODE
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string dmDeviceName;
            public short dmSpecVersion;
            public short dmDriverVersion;
            public short dmSize;
            public short dmDriverExtra;
            public int dmFields;
            public int dmPositionX;
            public int dmPositionY;
            public int dmDisplayOrientation;
            public int dmDisplayFixedOutput;
            public short dmColor;
            public short dmDuplex;
            public short dmYResolution;
            public short dmTTOption;
            public short dmCollate;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string dmFormName;
            public short dmLogPixels;
            public int dmBitsPerPel;
            public int dmPelsWidth;
            public int dmPelsHeight;
            public int dmDisplayFlags;
            public int dmDisplayFrequency;
            public int dmICMMethod;
            public int dmICMIntent;
            public int dmMediaType;
            public int dmDitherType;
            public int dmReserved1;
            public int dmReserved2;
            public int dmPanningWidth;
            public int dmPanningHeight;
        }

        public HzForm()
        {
            DetectRefreshRates();
            InitializeUI();
            UpdateUI();
        }

        private void DetectRefreshRates()
        {
            DEVMODE dm = new DEVMODE();
            dm.dmSize = (short)Marshal.SizeOf(dm);

            // Get current Hz
            if (EnumDisplaySettings(null, ENUM_CURRENT_SETTINGS, ref dm))
            {
                currentHz = dm.dmDisplayFrequency;
            }

            // Detect max supported Hz
            int modeIndex = 0;
            int maxHz = 60;

            while (EnumDisplaySettings(null, modeIndex, ref dm))
            {
                if (dm.dmDisplayFrequency > maxHz)
                {
                    maxHz = dm.dmDisplayFrequency;
                }
                modeIndex++;
            }

            maxSupportedHz = maxHz;
        }

        private void InitializeUI()
        {
            this.Text = "Hz Changer";
            this.Size = new Size(240, 160);
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            this.StartPosition = FormStartPosition.Manual;
            this.TopMost = true;
            this.MaximizeBox = false;

            Rectangle workingArea = Screen.PrimaryScreen!.WorkingArea;
            this.Location = new Point(workingArea.Right - this.Width - 15, workingArea.Bottom - this.Height - 15);

            lblCurrentHz = new Label
            {
                Location = new Point(15, 12),
                Size = new Size(195, 20),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            };
            this.Controls.Add(lblCurrentHz);

            btnLowHz = new Button
            {
                Text = $"{DEFAULT_LOW_HZ} Hz",
                Location = new Point(15, 40),
                Size = new Size(92, 32)
            };
            btnLowHz.Click += (s, e) => SetHz(DEFAULT_LOW_HZ);
            this.Controls.Add(btnLowHz);

            btnMaxHz = new Button
            {
                Text = $"{maxSupportedHz} Hz (Max)",
                Location = new Point(118, 40),
                Size = new Size(92, 32)
            };
            btnMaxHz.Click += (s, e) => SetHz(maxSupportedHz);
            this.Controls.Add(btnMaxHz);

            btnToggle = new Button
            {
                Text = "⚡ Quick Switch",
                Location = new Point(15, 78),
                Size = new Size(195, 30)
            };
            btnToggle.Click += (s, e) => ToggleHz();
            this.Controls.Add(btnToggle);
        }

        private void UpdateUI()
        {
            DEVMODE dm = new DEVMODE();
            dm.dmSize = (short)Marshal.SizeOf(dm);
            if (EnumDisplaySettings(null, ENUM_CURRENT_SETTINGS, ref dm))
            {
                currentHz = dm.dmDisplayFrequency;
            }

            lblCurrentHz.Text = $"Current Rate: {currentHz} Hz";
            lblCurrentHz.ForeColor = (currentHz >= maxSupportedHz) ? Color.DarkGreen : Color.DarkBlue;
        }

        private void SetHz(int targetHz)
        {
            DEVMODE dm = new DEVMODE();
            dm.dmSize = (short)Marshal.SizeOf(dm);

            if (EnumDisplaySettings(null, ENUM_CURRENT_SETTINGS, ref dm))
            {
                dm.dmDisplayFrequency = targetHz;
                int result = ChangeDisplaySettings(ref dm, CDS_UPDATEREGISTRY);

                if (result == DISP_CHANGE_SUCCESSFUL)
                {
                    UpdateUI();
                }
                else
                {
                    MessageBox.Show($"{targetHz} Hz is not supported by your display.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ToggleHz()
        {
            int targetHz = (currentHz == DEFAULT_LOW_HZ) ? maxSupportedHz : DEFAULT_LOW_HZ;
            SetHz(targetHz);
        }

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new HzForm());
        }
    }
}