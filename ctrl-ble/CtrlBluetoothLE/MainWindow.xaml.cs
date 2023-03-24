using BluetoothLE;
using BluetoothLE.Events;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CtrlBluetoothLE
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        #region ACTION(WINDOWS) BUTTONS
        private void WindowCloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        //Move window with mouse
        private void Windows_Drag(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        //Minimize window
        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            Minimize(this);
        }

        // Maximize/Minimize window depends on the state
        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
                Restore(this);
            else
                Maximize(this);
        }

        private static void Maximize(Window window)
        {
            window.ResizeMode = ResizeMode.NoResize;

            // Get handle for nearest monitor to this window
            var wih = new WindowInteropHelper(window);

            // Nearest monitor to window
            const int MONITOR_DEFAULTTONEAREST = 2;
            var hMonitor = MonitorFromWindow(wih.Handle, MONITOR_DEFAULTTONEAREST);

            // Get monitor info
            var monitorInfo = GetMonitorInfo(window, hMonitor);

            // Create working area dimensions, converted to DPI-independent values
            var source = HwndSource.FromHwnd(wih.Handle);

            if (source?.CompositionTarget == null)
            {
                return;
            }

            var matrix = source.CompositionTarget.TransformFromDevice;
            var workingArea = monitorInfo.rcWork;

            var dpiIndependentSize =
                matrix.Transform(
                    new Point(workingArea.Right - workingArea.Left,
                              workingArea.Bottom - workingArea.Top));



            // Maximize the window to the device-independent working area ie
            // the area without the taskbar.
            window.Top = workingArea.Top;
            window.Left = workingArea.Left;

            window.MaxWidth = dpiIndependentSize.X;
            window.MaxHeight = dpiIndependentSize.Y;

            window.WindowState = WindowState.Maximized;
        }
        private static void Minimize(Window window)
        {
            if (window == null)
            {
                return;
            }

            window.WindowState = WindowState.Minimized;
        }
        private static void Restore(Window window)
        {
            if (window == null)
            {
                return;
            }

            window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            window.WindowState = WindowState.Normal;
            window.ResizeMode = ResizeMode.CanResizeWithGrip;

        }
        #endregion

        #region EVENT HANDLE WINDOW
        // Nearest monitor to window
        const int MONITOR_DEFAULTTONEAREST = 2;
        // To get a handle to the specified monitor
        [DllImport("user32.dll")]
        static extern IntPtr MonitorFromWindow(IntPtr hwnd, int dwFlags);

        // Rectangle (used by MONITORINFOEX)
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        // Monitor information (used by GetMonitorInfo())
        [StructLayout(LayoutKind.Sequential)]
        public class MONITORINFOEX
        {
            public int cbSize;
            public RECT rcMonitor; // Total area
            public RECT rcWork; // Working area
            public int dwFlags;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x20)]
            public char[] szDevice;
        }

        // To get the working area of the specified monitor
        [DllImport("user32.dll")]
        public static extern bool GetMonitorInfo(HandleRef hmonitor, [In, Out] MONITORINFOEX monitorInfo);

        private static MONITORINFOEX GetMonitorInfo(Window window, IntPtr monitorPtr)
        {
            var monitorInfo = new MONITORINFOEX();
            monitorInfo.cbSize = Marshal.SizeOf(monitorInfo);
            GetMonitorInfo(new HandleRef(window, monitorPtr), monitorInfo);
            return monitorInfo;
        }

        void WindowSourceInitialized(object sender, EventArgs e)
        {

            // Make window borderless
            this.WindowStyle = WindowStyle.None;
            this.ResizeMode = ResizeMode.NoResize;

            // Get handle for nearest monitor to this window
            WindowInteropHelper wih = new WindowInteropHelper(this);
            IntPtr hMonitor = MonitorFromWindow(wih.Handle, MONITOR_DEFAULTTONEAREST);

            // Get monitor info
            MONITORINFOEX monitorInfo = new MONITORINFOEX();
            monitorInfo.cbSize = Marshal.SizeOf(monitorInfo);
            GetMonitorInfo(new HandleRef(this, hMonitor), monitorInfo);

            // Create working area dimensions, converted to DPI-independent values
            HwndSource source = HwndSource.FromHwnd(wih.Handle);
            if (source == null) return; // Should never be null
            if (source.CompositionTarget == null) return; // Should never be null
            Matrix matrix = source.CompositionTarget.TransformFromDevice;
            RECT workingArea = monitorInfo.rcWork;
            Point dpiIndependentSize = matrix.Transform(new Point(workingArea.Right - workingArea.Left, workingArea.Bottom - workingArea.Top));

            // Maximize the window to the device-independent working area ie
            // the area without the taskbar.
            // NOTE - window state must be set to Maximized as this adds certain
            // maximized behaviors eg you can't move a window while it is maximized,
            // such as by calling Window.DragMove
            this.Top = 0;
            this.Left = 0;
            this.MaxWidth = dpiIndependentSize.X;
            this.MaxHeight = dpiIndependentSize.Y;
            this.WindowState = WindowState.Maximized;

        }
        #endregion





        //private CtrlDevice _ctrlDevice;
        //private string SelectedDeviceId { get; set; }
        //private string SelectedDeviceName { get; set; }

        //public MainWindow()
        //{
        //    InitializeComponent();
        //    _ctrlDevice = new CtrlDevice();

        //    // we should always monitor the connection status
        //    _ctrlDevice.ConnectionStatusChanged -= CtrlDeviceOnDeviceConnectionStatusChanged;
        //    _ctrlDevice.ConnectionStatusChanged += CtrlDeviceOnDeviceConnectionStatusChanged;

        //    //// we can create value parser and listen for parsed values of given characteristic
        //    //HrParser.ConnectWithCharacteristic(HrDevice.HeartRate.HeartRateMeasurement);
        //    _ctrlDevice.ValueChanged -= CtrlDeviceParserOnValueChanged;
        //    _ctrlDevice.ValueChanged += CtrlDeviceParserOnValueChanged;
        //}

        //protected async override void OnClosing(CancelEventArgs e)
        //{
        //    base.OnClosing(e);

        //    if (_ctrlDevice.IsConnected)
        //    {
        //        await _ctrlDevice.DisconnectAsync();
        //    }
        //}

        //private async Task RunDispatcherOnUiThread(Action action)
        //{
        //    await this.Dispatcher.InvokeAsync(() =>
        //    {
        //        action();
        //    });
        //}


        //private async void CtrlDeviceParserOnValueChanged(object sender, DeviceChangedEventArgs arg)
        //{
        //    //Daodati try catch
        //    await RunDispatcherOnUiThread(() =>
        //    {
        //        GimbalLeftX.Text = String.Format("{0}", arg.GimbalLeftX);
        //        GimbalLeftY.Text = String.Format("{0}", arg.GimbalLeftY);
        //        GimbalRightX.Text = String.Format("{0}", arg.GimbalRightX);
        //        GimbalRightY.Text = String.Format("{0}", arg.GimbalRightY);

        //        gimbalLeft.SetValue(Canvas.LeftProperty, (double)arg.GimbalLeftX); //set x
        //        gimbalLeft.SetValue(Canvas.BottomProperty, (double)arg.GimbalLeftY); //set y

        //        gimbalRight.SetValue(Canvas.LeftProperty, (double)arg.GimbalRightX); //set x
        //        gimbalRight.SetValue(Canvas.BottomProperty, (double)arg.GimbalRightY); //set y

        //        LeftSwitchButton.Value = arg.LeftSwitchButton;
        //        RightSwitchButton.Value = arg.RightSwitchButton;

        //        LeftPressButton.Value = arg.LeftPressButton;
        //        RightPressButton.Value = arg.RightPressButton;

        //        ButtonA.Value = arg.ButtonA;
        //        ButtonB.Value = arg.ButtonB;

        //        GhostUbert.Visibility = arg.Ghost == 0 ? Visibility.Collapsed : Visibility.Visible;
        //    });
        //}

        //private async void CtrlDeviceOnDeviceConnectionStatusChanged(object sender, ConnectionStatusChangedEventArgs args)
        //{

        //    await RunDispatcherOnUiThread(async () =>
        //    {
        //        try
        //        {
        //            bool connected = args.IsConnected;
        //            if (connected)
        //            {
        //                var device = await _ctrlDevice.GetDeviceInfoAsync();
        //                TxtStatus.Text = SelectedDeviceName + ": connected";
        //                TxtBattery.Text = String.Format("battery level: {0}% ", device.BatteryPercent);

        //            }
        //            else
        //            {
        //                TxtStatus.Text = SelectedDeviceName + ": disconnected";
        //                TxtBattery.Text = "battery level: --";
        //                TxtHr.Text = "--";
        //            }

        //            BtnReadInfo.IsEnabled = connected;
        //        }
        //        catch (Exception ex)
        //        {
        //            //Awaiting CTRL more
        //            MessageBox.Show(ex.Message);
        //        }

        //    });



        //}

        //private async void CalibrationButton_Click(object sender, RoutedEventArgs e)
        //{
        //    await _ctrlDevice.Calibration();
        //    await CalibrationMovementAsync();
        //}
        //private async Task CalibrationMovementAsync()
        //{
        //    gimbalCalibrationLeft.Visibility = Visibility.Visible;
        //    gimbalCalibrationRight.Visibility = Visibility.Visible;

        //    gimbalCalibrationLeft.SetValue(Canvas.LeftProperty, (double)50);
        //    gimbalCalibrationLeft.SetValue(Canvas.BottomProperty, (double)50);

        //    gimbalCalibrationRight.SetValue(Canvas.LeftProperty, (double)50);
        //    gimbalCalibrationRight.SetValue(Canvas.BottomProperty, (double)50);
        //    await Task.Delay(3000);

        //    gimbalCalibrationLeft.SetValue(Canvas.LeftProperty, (double)100);
        //    gimbalCalibrationLeft.SetValue(Canvas.BottomProperty, (double)100);

        //    gimbalCalibrationRight.SetValue(Canvas.LeftProperty, (double)100);
        //    gimbalCalibrationRight.SetValue(Canvas.BottomProperty, (double)100);
        //    await Task.Delay(3000);

        //    gimbalCalibrationLeft.SetValue(Canvas.LeftProperty, (double)50);
        //    gimbalCalibrationLeft.SetValue(Canvas.BottomProperty, (double)100);

        //    gimbalCalibrationRight.SetValue(Canvas.LeftProperty, (double)50);
        //    gimbalCalibrationRight.SetValue(Canvas.BottomProperty, (double)100);
        //    await Task.Delay(3000);

        //    gimbalCalibrationLeft.SetValue(Canvas.LeftProperty, (double)0);
        //    gimbalCalibrationLeft.SetValue(Canvas.BottomProperty, (double)100);

        //    gimbalCalibrationRight.SetValue(Canvas.LeftProperty, (double)0);
        //    gimbalCalibrationRight.SetValue(Canvas.BottomProperty, (double)100);
        //    await Task.Delay(3000);

        //    gimbalCalibrationLeft.SetValue(Canvas.LeftProperty, (double)0);
        //    gimbalCalibrationLeft.SetValue(Canvas.BottomProperty, (double)50);

        //    gimbalCalibrationRight.SetValue(Canvas.LeftProperty, (double)0);
        //    gimbalCalibrationRight.SetValue(Canvas.BottomProperty, (double)50);
        //    await Task.Delay(3000);

        //    gimbalCalibrationLeft.SetValue(Canvas.LeftProperty, (double)0);
        //    gimbalCalibrationLeft.SetValue(Canvas.BottomProperty, (double)0);

        //    gimbalCalibrationRight.SetValue(Canvas.LeftProperty, (double)0);
        //    gimbalCalibrationRight.SetValue(Canvas.BottomProperty, (double)0);
        //    await Task.Delay(3000);

        //    gimbalCalibrationLeft.SetValue(Canvas.LeftProperty, (double)50);
        //    gimbalCalibrationLeft.SetValue(Canvas.BottomProperty, (double)0);

        //    gimbalCalibrationRight.SetValue(Canvas.LeftProperty, (double)50);
        //    gimbalCalibrationRight.SetValue(Canvas.BottomProperty, (double)0);
        //    await Task.Delay(3000);

        //    gimbalCalibrationLeft.SetValue(Canvas.LeftProperty, (double)100);
        //    gimbalCalibrationLeft.SetValue(Canvas.BottomProperty, (double)0);

        //    gimbalCalibrationRight.SetValue(Canvas.LeftProperty, (double)100);
        //    gimbalCalibrationRight.SetValue(Canvas.BottomProperty, (double)0);
        //    await Task.Delay(3000);

        //    gimbalCalibrationLeft.SetValue(Canvas.LeftProperty, (double)100);
        //    gimbalCalibrationLeft.SetValue(Canvas.BottomProperty, (double)50);

        //    gimbalCalibrationRight.SetValue(Canvas.LeftProperty, (double)100);
        //    gimbalCalibrationRight.SetValue(Canvas.BottomProperty, (double)50);
        //    await Task.Delay(3000);

        //    gimbalCalibrationLeft.Visibility = Visibility.Collapsed;
        //    gimbalCalibrationRight.Visibility = Visibility.Collapsed;

        //}

        //private async void BtnReadInfo_Click(object sender, RoutedEventArgs e)
        //{
        //    var deviceInfo = await _ctrlDevice.GetDeviceInfoAsync();

        //    d($" Manufacturer : {deviceInfo.Manufacturer}"); d("");
        //    d($"    Model : {deviceInfo.ModelNumber}"); d("");
        //    d($"      S/N : {deviceInfo.SerialNumber}"); d("");
        //    d($" Firmware : {deviceInfo.Firmware}"); d("");
        //    d($" Hardware : {deviceInfo.Hardware}"); d("");

        //    TxtBattery.Text = $"battery level: {deviceInfo.BatteryPercent}%";
        //}

        //[Conditional("DEBUG")]
        //private void d(string txt)
        //{
        //    Debug.WriteLine(txt);
        //}

        //private async void PairDeviceButton_Click(object sender, RoutedEventArgs e)
        //{
        //    if (_ctrlDevice.IsConnected)
        //    {
        //        SelectedDeviceId = string.Empty;
        //        SelectedDeviceName = string.Empty;

        //        await _ctrlDevice.DisconnectAsync();
        //    }

        //    var devicePicker = new DevicePicker();
        //    var result = devicePicker.ShowDialog();
        //    if (result.Value)
        //    {
        //        SelectedDeviceId = devicePicker.SelectedDeviceId;
        //        SelectedDeviceName = devicePicker.SelectedDeviceName;

        //        var connectResult = await _ctrlDevice.ConnectAsync(SelectedDeviceId);
        //        if (!connectResult.IsConnected)
        //            MessageBox.Show(connectResult.ErrorMessage);
        //    }
        //}

        //private void Button_Click(object sender, RoutedEventArgs e)
        //{
        //    ConnectCtrlView connectCtrlView = new ConnectCtrlView();
        //    this.Close();
        //    connectCtrlView.Show();
        //}
    }
}
