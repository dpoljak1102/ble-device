using BluetoothLE;
using BluetoothLE.Events;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
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
        private CtrlDevice _ctrlDevice;
        private string SelectedDeviceId { get; set; }
        private string SelectedDeviceName { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            _ctrlDevice = new CtrlDevice();

            // we should always monitor the connection status
            _ctrlDevice.ConnectionStatusChanged -= CtrlDeviceOnDeviceConnectionStatusChanged;
            _ctrlDevice.ConnectionStatusChanged += CtrlDeviceOnDeviceConnectionStatusChanged;

            //// we can create value parser and listen for parsed values of given characteristic
            //HrParser.ConnectWithCharacteristic(HrDevice.HeartRate.HeartRateMeasurement);
            _ctrlDevice.ValueChanged -= CtrlDeviceParserOnValueChanged;
            _ctrlDevice.ValueChanged += CtrlDeviceParserOnValueChanged;
        }

        protected async override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            if (_ctrlDevice.IsConnected)
            {
                await _ctrlDevice.DisconnectAsync();
            }
        }

        private async Task RunDispatcherOnUiThread(Action action)
        {
            await this.Dispatcher.InvokeAsync(() =>
            {
                action();
            });
        }


        private async void CtrlDeviceParserOnValueChanged(object sender, DeviceChangedEventArgs arg)
        {
            await RunDispatcherOnUiThread(() =>
            {
                GimbalLeftX.Text = String.Format("{0}", arg.GimbalLeftX);
                GimbalLeftY.Text = String.Format("{0}", arg.GimbalLeftY);
                GimbalRightX.Text = String.Format("{0}", arg.GimbalRightX);
                GimbalRightY.Text = String.Format("{0}", arg.GimbalRightY);

                gimbalLeft.SetValue(Canvas.LeftProperty, (double)arg.GimbalLeftX); //set x
                gimbalLeft.SetValue(Canvas.BottomProperty, (double)arg.GimbalLeftY); //set y

                gimbalRight.SetValue(Canvas.LeftProperty, (double)arg.GimbalRightX); //set x
                gimbalRight.SetValue(Canvas.BottomProperty, (double)arg.GimbalRightY); //set y

                LeftSwitchButton.Value = arg.LeftSwitchButton;
                RightSwitchButton.Value = arg.RightSwitchButton;

                LeftPressButton.Value = arg.LeftPressButton;
                RightPressButton.Value = arg.RightPressButton;

                ButtonA.Value = arg.ButtonA;
                ButtonB.Value = arg.ButtonB;

                GhostUbert.Visibility = arg.Ghost == 0 ? Visibility.Collapsed : Visibility.Visible;
            });
        }

        private async void CtrlDeviceOnDeviceConnectionStatusChanged(object sender, ConnectionStatusChangedEventArgs args)
        {
            //d("Current connection status is: " + args.IsConnected);
            await RunDispatcherOnUiThread(async () =>
            {
                bool connected = args.IsConnected;
                if (connected)
                {
                    var device = await _ctrlDevice.GetDeviceInfoAsync();
                    TxtStatus.Text = SelectedDeviceName + ": connected";
                    TxtBattery.Text = String.Format("battery level: {0}%", device.BatteryPercent);
                }
                else
                {
                    TxtStatus.Text = SelectedDeviceName + ": disconnected";
                    TxtBattery.Text = "battery level: --";
                    TxtHr.Text = "--";
                }

                BtnReadInfo.IsEnabled = connected;
            });
        }

        private async void CalibrationButton_Click(object sender, RoutedEventArgs e)
        {
            await _ctrlDevice.Calibration();
            await CalibrationMovementAsync();
        }
        private async Task CalibrationMovementAsync()
        {
            gimbalCalibrationLeft.Visibility = Visibility.Visible;
            gimbalCalibrationRight.Visibility = Visibility.Visible;

            gimbalCalibrationLeft.SetValue(Canvas.LeftProperty, (double)50);
            gimbalCalibrationLeft.SetValue(Canvas.BottomProperty, (double)50);

            gimbalCalibrationRight.SetValue(Canvas.LeftProperty, (double)50);
            gimbalCalibrationRight.SetValue(Canvas.BottomProperty, (double)50);
            await Task.Delay(3000);

            gimbalCalibrationLeft.SetValue(Canvas.LeftProperty, (double)100);
            gimbalCalibrationLeft.SetValue(Canvas.BottomProperty, (double)100);

            gimbalCalibrationRight.SetValue(Canvas.LeftProperty, (double)100);
            gimbalCalibrationRight.SetValue(Canvas.BottomProperty, (double)100);
            await Task.Delay(3000);

            gimbalCalibrationLeft.SetValue(Canvas.LeftProperty, (double)50);
            gimbalCalibrationLeft.SetValue(Canvas.BottomProperty, (double)100);

            gimbalCalibrationRight.SetValue(Canvas.LeftProperty, (double)50);
            gimbalCalibrationRight.SetValue(Canvas.BottomProperty, (double)100);
            await Task.Delay(3000);

            gimbalCalibrationLeft.SetValue(Canvas.LeftProperty, (double)0);
            gimbalCalibrationLeft.SetValue(Canvas.BottomProperty, (double)100);

            gimbalCalibrationRight.SetValue(Canvas.LeftProperty, (double)0);
            gimbalCalibrationRight.SetValue(Canvas.BottomProperty, (double)100);
            await Task.Delay(3000);

            gimbalCalibrationLeft.SetValue(Canvas.LeftProperty, (double)0);
            gimbalCalibrationLeft.SetValue(Canvas.BottomProperty, (double)50);

            gimbalCalibrationRight.SetValue(Canvas.LeftProperty, (double)0);
            gimbalCalibrationRight.SetValue(Canvas.BottomProperty, (double)50);
            await Task.Delay(3000);

            gimbalCalibrationLeft.SetValue(Canvas.LeftProperty, (double)0);
            gimbalCalibrationLeft.SetValue(Canvas.BottomProperty, (double)0);

            gimbalCalibrationRight.SetValue(Canvas.LeftProperty, (double)0);
            gimbalCalibrationRight.SetValue(Canvas.BottomProperty, (double)0);
            await Task.Delay(3000);

            gimbalCalibrationLeft.SetValue(Canvas.LeftProperty, (double)50);
            gimbalCalibrationLeft.SetValue(Canvas.BottomProperty, (double)0);

            gimbalCalibrationRight.SetValue(Canvas.LeftProperty, (double)50);
            gimbalCalibrationRight.SetValue(Canvas.BottomProperty, (double)0);
            await Task.Delay(3000);

            gimbalCalibrationLeft.SetValue(Canvas.LeftProperty, (double)100);
            gimbalCalibrationLeft.SetValue(Canvas.BottomProperty, (double)0);

            gimbalCalibrationRight.SetValue(Canvas.LeftProperty, (double)100);
            gimbalCalibrationRight.SetValue(Canvas.BottomProperty, (double)0);
            await Task.Delay(3000);

            gimbalCalibrationLeft.SetValue(Canvas.LeftProperty, (double)100);
            gimbalCalibrationLeft.SetValue(Canvas.BottomProperty, (double)50);

            gimbalCalibrationRight.SetValue(Canvas.LeftProperty, (double)100);
            gimbalCalibrationRight.SetValue(Canvas.BottomProperty, (double)50);
            await Task.Delay(3000);

            gimbalCalibrationLeft.Visibility = Visibility.Collapsed;
            gimbalCalibrationRight.Visibility = Visibility.Collapsed;

        }

        private async void BtnReadInfo_Click(object sender, RoutedEventArgs e)
        {
            var deviceInfo = await _ctrlDevice.GetDeviceInfoAsync();

            d($" Manufacturer : {deviceInfo.Manufacturer}"); d("");
            d($"    Model : {deviceInfo.ModelNumber}"); d("");
            d($"      S/N : {deviceInfo.SerialNumber}"); d("");
            d($" Firmware : {deviceInfo.Firmware}"); d("");
            d($" Hardware : {deviceInfo.Hardware}"); d("");

            TxtBattery.Text = $"battery level: {deviceInfo.BatteryPercent}%";
        }

        [Conditional("DEBUG")]
        private void d(string txt)
        {
            Debug.WriteLine(txt);
        }

        private async void PairDeviceButton_Click(object sender, RoutedEventArgs e)
        {
            if (_ctrlDevice.IsConnected)
            {
                SelectedDeviceId = string.Empty;
                SelectedDeviceName = string.Empty;

                await _ctrlDevice.DisconnectAsync();
            }

            var devicePicker = new DevicePicker();
            var result = devicePicker.ShowDialog();
            if (result.Value)
            {
                SelectedDeviceId = devicePicker.SelectedDeviceId;
                SelectedDeviceName = devicePicker.SelectedDeviceName;

                var connectResult = await _ctrlDevice.ConnectAsync(SelectedDeviceId);
                if (!connectResult.IsConnected)
                    MessageBox.Show(connectResult.ErrorMessage);
            }
        }

    }
}
