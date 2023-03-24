using BluetoothLE;
using BluetoothLE.Events;
using BluetoothLE.Model;
using CtrlBluetoothLE.Core;
using CtrlBluetoothLE.Service.Common;
using CtrlBluetoothLE.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CtrlBluetoothLE.ViewModels
{
    public class FpvCtrlViewModel : ViewModelBase
    {

        #region CONSTRUCTOR
        public FpvCtrlViewModel(INavigationService navigationService)
        {
            NavigationService = navigationService;

            CtrlDevice = new CtrlDevice();
            UnpairedCollection = new ObservableCollection<WatcherDevice>();
            PairedCollection = new ObservableCollection<WatcherDevice>();

            // Monitor the connection status
            CtrlDevice.ConnectionStatusChanged -= CtrlDeviceOnDeviceConnectionStatusChanged;
            CtrlDevice.ConnectionStatusChanged += CtrlDeviceOnDeviceConnectionStatusChanged;

            // Listen for parsed values of given characteristic
            CtrlDevice.ValueChanged -= CtrlDeviceParserOnValueChanged;
            CtrlDevice.ValueChanged += CtrlDeviceParserOnValueChanged;

            NavigateAvailableControllersCommand = new AsyncRelayCommand(ConnectToDeviceAsync, (ex) => ConnectToDeviceExceptionHandler(ex));

            CalibrationCommand = new AsyncRelayCommand(CalibrationAsync, (ex) => ConnectToDeviceExceptionHandler(ex));

        }
        #endregion

        #region VIEW

        #region BUTTONS

        #region GIMBALS

        #region LEFT GIMBAL

        private double _gimbalLeftX;
        public double GimbalLeftX
        {
            get { return _gimbalLeftX; }
            set { _gimbalLeftX = value; OnPropertyChanged(); }
        }


        private double _gimbalLeftY;
        public double GimbalLeftY
        {
            get { return _gimbalLeftY; }
            set { _gimbalLeftY = value; OnPropertyChanged(); }
        }

        #endregion

        #region LEFT CALIBRATION GIMBAL

        private double _gimbalCalibrationLeftX = 50;
        public double GimbalCalibrationLeftX
        {
            get { return _gimbalCalibrationLeftX; }
            set { _gimbalCalibrationLeftX = value; OnPropertyChanged(); }
        }


        private double _gimbalCalibrationLeftY = 50;
        public double GimbalCalibrationLeftY
        {
            get { return _gimbalCalibrationLeftY; }
            set { _gimbalCalibrationLeftY = value; OnPropertyChanged(); }
        }

        #endregion

        #region RIGHT GIMBAL

        private double _gimbalRightX;
        public double GimbalRightX
        {
            get { return _gimbalRightX; }
            set { _gimbalRightX = value; OnPropertyChanged(); }
        }

        private double _gimbalRightY;
        public double GimbalRightY
        {
            get { return _gimbalRightY; }
            set { _gimbalRightY = value; OnPropertyChanged(); }
        }
        #endregion

        #region RIGHT CALIBRATION GIMBAL
        private double _gimbalCalibrationRightX = 50;
        public double GimbalCalibrationRightX
        {
            get { return _gimbalCalibrationRightX; }
            set { _gimbalCalibrationRightX = value; OnPropertyChanged(); }
        }

        private double _gimbalCalibrationRightY = 50;
        public double GimbalCalibrationRightY
        {
            get { return _gimbalCalibrationRightY; }
            set { _gimbalCalibrationRightY = value; OnPropertyChanged(); }
        }
        #endregion

        #endregion

        #region SWITCH BUTTONS

        #region LEFT BUTTON
        private double _switchButtonLeft;
        public double SwitchButtonLeft
        {
            get { return _switchButtonLeft; }
            set { _switchButtonLeft = value; OnPropertyChanged(); }
        }
        #endregion

        #region RIGHT BUTTON
        private double _switchButtonRight;
        public double SwitchButtonRight
        {
            get { return _switchButtonRight; }
            set { _switchButtonRight = value; OnPropertyChanged(); }
        }
        #endregion

        #endregion

        #region PRESS BUTTONS

        #region LEFT BUTTON
        private double _presshButtonLeft;
        public double PressButtonLeft
        {
            get { return _presshButtonLeft; }
            set { _presshButtonLeft = value; OnPropertyChanged(); }
        }
        #endregion

        #region RIGHT BUTTON
        private double _presshButtonRight;
        public double PressButtonRight
        {
            get { return _presshButtonRight; }
            set { _presshButtonRight = value; OnPropertyChanged(); }
        }

        #endregion

        #endregion

        #region BUTTON A
        private double _buttonA;
        public double ButtonA
        {
            get { return _buttonA; }
            set { _buttonA = value; OnPropertyChanged(); }
        }
        #endregion

        #region BUTTON B
        private double _buttonB;
        public double ButtonB
        {
            get { return _buttonB; }
            set { _buttonB = value; OnPropertyChanged(); }
        }
        #endregion

        #region GHOST
        private double _ghostUberLite;
        public double GhostUberLite
        {
            get { return _ghostUberLite; }
            set { _ghostUberLite = value; OnPropertyChanged(); }
        }


        #endregion

        #endregion

        #region CONNECTION ERROR MESSAGE
        private string _connectionErrorMessage;

        public string ConnectionErrorMessage
        {
            get { return _connectionErrorMessage; }
            set { _connectionErrorMessage = value; OnPropertyChanged(); }
        }
        #endregion

        #region SEARCH BLUETOOTH DEVICE NAME
        private string _searchText;
        public string SearchText
        {
            get { return _searchText; }
            set
            {
                _searchText = value;
                OnPropertyChanged("SearchText");
            }
        }

        #endregion

        #region CONNECT BUTTON

        private string _connectionButtonContent = "Add Bluetooth FPV.Ctrl device";

        public string ConnectionButtonContent
        {
            get { return _connectionButtonContent; }
            set { _connectionButtonContent = value; OnPropertyChanged(); }
        }


        #endregion

        #region IS DEVICE CONNECTED

        private bool _isDeviceConnected = false;

        public bool IsDeviceConnected
        {
            get { return _isDeviceConnected; }
            set { _isDeviceConnected = value; OnPropertyChanged(); }
        }
        #endregion

        #region IS GHOST CONNECTED
        private bool _isGhostConnected = false;
        public bool IsGhostConnected
        {
            get { return _isGhostConnected; }
            set { _isGhostConnected = value; OnPropertyChanged(); }
        }
        #endregion

        #endregion

        #region COMMANDS
        public ICommand NavigateAvailableControllersCommand { get; set; }
        public ICommand CalibrationCommand { get; set; }

        #endregion

        #region CONNECTION HANDLER
        private async Task ConnectToDeviceAsync()
        {
            // Restart errors when we reconnect to device
            ConnectionErrorMessage = "";
            try
            {
                if (CtrlDevice.IsConnected)
                {
                    SelectedDeviceId = string.Empty;
                    SelectedDeviceName = string.Empty;
                    await CtrlDevice.DisconnectAsync();
                }


                AvailableCtrlControllersPopupWindow availableCtrlControllersPopupWindow = new AvailableCtrlControllersPopupWindow();
                var results = availableCtrlControllersPopupWindow.ShowDialog();

                if (results.Value)
                {
                    SelectedDeviceId = availableCtrlControllersPopupWindow.DeviceId;
                    SelectedDeviceName = availableCtrlControllersPopupWindow.DeviceName;
                    ConnectionButtonContent = $"Connecting to {SelectedDeviceName}, please wait...";

                    var connectResult = await CtrlDevice.ConnectAsync(SelectedDeviceId);
                    if (!connectResult.IsConnected)
                        throw new Exception(connectResult.ErrorMessage);

                    IsDeviceConnected = true;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void ConnectToDeviceExceptionHandler(Exception ex)
        {
            IsDeviceConnected = false;
            ConnectionErrorMessage = ex.Message;
        }

        #endregion

        #region CTRL DEVICE CONNECTION STATUS && PARSER VALUE
        private async void CtrlDeviceOnDeviceConnectionStatusChanged(object sender, ConnectionStatusChangedEventArgs args)
        {

            try
            {
                bool connected = args.IsConnected;
                if (connected)
                {
                    var device = await CtrlDevice.GetDeviceInfoAsync();
                    ConnectionButtonContent = "Add Bluetooth FPV.Ctrl device";
                    SelectedDeviceName = device.Name;
                    SelectedDeviceBattery = device.BatteryPercent;
                }
                else
                {
                    GimbalLeftX = 0;
                    GimbalLeftY = 0;
                    GimbalRightX = 0;
                    GimbalRightY = 0;
                    SwitchButtonLeft = 0;
                    SwitchButtonRight = 0;
                    PressButtonLeft = 0;
                    PressButtonRight = 0;
                    IsDeviceConnected = false;
                    IsGhostConnected = false;
                    ConnectionButtonContent = "Add Bluetooth FPV.Ctrl device";
                    ConnectionErrorMessage = "Device is disconnected.";

                }
            }
            catch (Exception ex)
            {
                IsDeviceConnected = false;
                ConnectionErrorMessage = ex.Message;
            }

        }

        private void CtrlDeviceParserOnValueChanged(object sender, DeviceChangedEventArgs arg)
        {
           
            GimbalLeftX = arg.GimbalLeftX;
            GimbalLeftY = arg.GimbalLeftY;
            GimbalRightX = arg.GimbalRightX;
            GimbalRightY = arg.GimbalRightY;
            SwitchButtonLeft = arg.LeftSwitchButton;
            SwitchButtonRight = arg.RightSwitchButton;
            PressButtonLeft = arg.LeftPressButton;
            PressButtonRight = arg.RightPressButton;
            GhostUberLite = arg.Ghost;

            //Show ghost connected on view
            IsGhostConnected = GhostUberLite > 0;
        }
        
        #endregion

        #region CALIBRATION

        private async Task CalibrationAsync()
        {
            try
            {
                await CtrlDevice.CalibrationAsync();
                await CalibrationMovementAsync();
            }
            catch (Exception ex)
            {
                throw ex;
            }
           
        }

        private async Task CalibrationMovementAsync()
        {
            GimbalCalibrationLeftX = 50;
            GimbalCalibrationLeftY = 50;
            GimbalCalibrationRightX = 50;
            GimbalCalibrationRightY = 50;
            await Task.Delay(3000);

            GimbalCalibrationLeftX = 100;
            GimbalCalibrationLeftY = 100;
            GimbalCalibrationRightX = 100;
            GimbalCalibrationRightY = 100;
            await Task.Delay(3000);

            GimbalCalibrationLeftX = 50;
            GimbalCalibrationLeftY = 100;
            GimbalCalibrationRightX = 50;
            GimbalCalibrationRightY = 100;
            await Task.Delay(3000);

            GimbalCalibrationLeftX = 0;
            GimbalCalibrationLeftY = 100;
            GimbalCalibrationRightX = 0;
            GimbalCalibrationRightY = 100;
            await Task.Delay(3000);

            GimbalCalibrationLeftX = 0;
            GimbalCalibrationLeftY = 50;
            GimbalCalibrationRightX = 0;
            GimbalCalibrationRightY = 50;
            await Task.Delay(3000);

            GimbalCalibrationLeftX = 0;
            GimbalCalibrationLeftY = 0;
            GimbalCalibrationRightX = 0;
            GimbalCalibrationRightY = 0;
            await Task.Delay(3000);

            GimbalCalibrationLeftX = 50;
            GimbalCalibrationLeftY = 0;
            GimbalCalibrationRightX = 50;
            GimbalCalibrationRightY = 0;
            await Task.Delay(3000);

            GimbalCalibrationLeftX = 100;
            GimbalCalibrationLeftY = 0;
            GimbalCalibrationRightX = 100;
            GimbalCalibrationRightY = 0;
            await Task.Delay(3000);

            GimbalCalibrationLeftX = 100;
            GimbalCalibrationLeftY = 50;
            GimbalCalibrationRightX = 100;
            GimbalCalibrationRightY = 50;
            await Task.Delay(3000);


            GimbalCalibrationLeftX = 50;
            GimbalCalibrationLeftY = 50;
            GimbalCalibrationRightX = 50;
            GimbalCalibrationRightY = 50;
        }

        #endregion

    }
}
