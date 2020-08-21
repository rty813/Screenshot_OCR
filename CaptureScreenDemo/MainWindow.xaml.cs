using System;
using System.Configuration;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;

namespace CaptureScreenDemo {
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class MainWindow {
        [DllImport("user32.dll")]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
        [DllImport("user32.dll")]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private System.Windows.Forms.NotifyIcon notifyIcon;
        private readonly RisCaptureLib.ScreenCaputre screenCaputre = new RisCaptureLib.ScreenCaputre();

        public MainWindow() {
            InitializeComponent();

            screenCaputre.ScreenCaputred += OnScreenCaputred;

            notifyIcon = new System.Windows.Forms.NotifyIcon();
            notifyIcon.Icon = System.Drawing.Icon.ExtractAssociatedIcon(System.Windows.Forms.Application.ExecutablePath);
            notifyIcon.Text = "豆腐OCR截屏";
            notifyIcon.Visible = true;
            notifyIcon.DoubleClick += capture;
            //图标右键菜单
            notifyIcon.ContextMenu = new System.Windows.Forms.ContextMenu(new[]
            {
                new System.Windows.Forms.MenuItem("设置API接口", (sender, args) =>
                {
                    Visibility = Visibility.Visible;
                    ShowInTaskbar = true;
                }),
                new System.Windows.Forms.MenuItem("退出程序", (sender, args) =>
                {
                    Application.Current.Shutdown();
                    notifyIcon.Dispose();
                }),
            });
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e) {
            if (OCRHelper.SecretID != "" && OCRHelper.SecretKey != "") {
                e.Cancel = true;
                Visibility = Visibility.Hidden;
                ShowInTaskbar = false;
            }
        }

        private void capture(object sender, EventArgs e) {
            Console.WriteLine("Capture!");
            screenCaputre.StartCaputre(60);

        }

        private void OnScreenCaputred(object sender, RisCaptureLib.ScreenCaputredEventArgs e) {
            var bmp = e.Bmp;
            var win = new Window { SizeToContent = SizeToContent.WidthAndHeight, ResizeMode = ResizeMode.NoResize };

            var grid = new Grid();
            RowDefinition row1 = new RowDefinition();
            RowDefinition row2 = new RowDefinition();
            row1.Height = new GridLength(bmp.Height);
            row2.Height = new GridLength(60);
            grid.RowDefinitions.Add(row1);
            grid.RowDefinitions.Add(row2);

            var canvas = new Canvas { Width = bmp.Width, Height = bmp.Height, Background = new ImageBrush(bmp) };
            grid.Children.Add(canvas);
            var textbox = new TextBox();
            textbox.Width = bmp.Width;
            textbox.Text = OCRHelper.recognize(OCRHelper.Img2Base64(bmp));
            grid.Children.Add(textbox);
            Grid.SetRow(canvas, 0);
            Grid.SetRow(textbox, 1);

            win.Content = grid;
            win.Title = "豆腐OCR截图";
            win.Show();
        }

        private const Int32 MY_HOTKEYID = 0x9999;

        protected override void OnSourceInitialized(EventArgs e) {
            base.OnSourceInitialized(e);

            IntPtr handle = new WindowInteropHelper(this).Handle;
            RegisterHotKey(handle, MY_HOTKEYID, 0x0002, 0x71); // CTRL+F2

            HwndSource source = PresentationSource.FromVisual(this) as HwndSource;
            source.AddHook(WndProc);

            OCRHelper.SecretID = ConfigurationManager.AppSettings["SecretID"];
            OCRHelper.SecretKey = ConfigurationManager.AppSettings["SecretKey"];
            tbID.Text = OCRHelper.SecretID;
            tbKey.Text = OCRHelper.SecretKey;
            if (OCRHelper.SecretID == "" || OCRHelper.SecretKey == "") {
                MessageBox.Show("请设置腾讯云的SecretID、SecretKey");
            }
            else {
                Visibility = Visibility.Hidden;
                ShowInTaskbar = false;
            }
        }

        IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handle) {
            //Debug.WriteLine("hwnd:{0},msg:{1},wParam:{2},lParam{3}:,handle:{4}"
            //                ,hwnd,msg,wParam,lParam,handle);
            if (wParam.ToInt32() == MY_HOTKEYID) {
                // 全局快捷键要执行的命令
                capture(null, null);
            }
            return IntPtr.Zero;
        }

        private void btnOk_Click(object sender, RoutedEventArgs e) {
            if (tbID.Text == "" || tbKey.Text == "") {
                MessageBox.Show("请检查输入！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else {
                MessageBox.Show("设置成功");
                Configuration cfa = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                cfa.AppSettings.Settings["SecretID"].Value = tbID.Text;
                cfa.AppSettings.Settings["SecretKey"].Value = tbKey.Text;
                cfa.Save();

                OCRHelper.SecretID = tbID.Text;
                OCRHelper.SecretKey = tbKey.Text;
                Visibility = Visibility.Hidden;
                ShowInTaskbar = false;
            }
        }
    }
}
