using BluetoothLE;
using BluetoothLE.Model;
using CtrlBluetoothLE.Core;
using CtrlBluetoothLE.Service.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace CtrlBluetoothLE.ViewModels
{
    public class AvailableCtrlControllersViewModel : ViewModelBase
    {
        #region CONNECT BUTTON

        private string _connectionButtonContent = "Connect";

        public string ConnectionButtonContent
        {
            get { return _connectionButtonContent; }
            set { _connectionButtonContent = value; OnPropertyChanged(); }
        }

        #endregion

        #region CONNECTION ERROR MESSAGE
        private string _connectionErrorMessage;

        public string ConnectionErrorMessage
        {
            get { return _connectionErrorMessage; }
            set { _connectionErrorMessage = value; OnPropertyChanged(); }
        }
        #endregion

        #region LOADING CONTENT
        private object _contentState = LoadingViewModel.CreateView();
        public object ContentState
        {
            get { return _contentState; }
            set { _contentState = value; OnPropertyChanged(); }
        }

        private bool _isLoading = true;
        public bool IsLoading
        {
            get { return _isLoading; }
            set { _isLoading = value; OnPropertyChanged(); }
        }

        #endregion


        private WatcherDevice _selecteBluetoothDevice;

        public WatcherDevice SelecteBluetoothDevice
        {
            get { return _selecteBluetoothDevice; }
            set { _selecteBluetoothDevice = value; OnPropertyChanged(); }
        }


        public ICommand WindowsOnCloseCommand { get; set; }
        public ICommand ConnectToBluetoothDeviceCommand { get; set; }

        public AvailableCtrlControllersViewModel()
        {
            WindowsOnCloseCommand = new RelayCommand(o =>
            {
                _unpairedWatcher.Stop();
                _pairedWatcher.Stop();
            });

            ConnectToBluetoothDeviceCommand = new AsyncRelayCommand(ConnectToBluetoothLeDeviceAsync, (ex) => ConnectToBluetoothLeDeviceExceptionHandler(ex));

            //CtrlDevice = new CtrlDevice();
            UnpairedCollection = new ObservableCollection<WatcherDevice>();
            PairedCollection = new ObservableCollection<WatcherDevice>();

            _unpairedWatcher = new CtrlDeviceWatcher(DeviceSelector.BluetoothLeUnpairedOnly);
            _unpairedWatcher.DeviceAdded += OnDeviceAdded;
            _unpairedWatcher.DeviceRemoved += OnDeviceRemoved;
            _unpairedWatcher.Start();

            _pairedWatcher = new CtrlDeviceWatcher(DeviceSelector.BluetoothLePairedOnly);
            _pairedWatcher.DeviceAdded += OnPaired_DeviceAdded;
            _pairedWatcher.DeviceRemoved += OnPaired_DeviceRemoved;

            _pairedWatcher.Start();
            SelectedDeviceId = string.Empty;
            SelectedDeviceName = string.Empty;
            
            LoadingContentContent();
        }

        private void LoadingContentContent()
        {
            Task.Run(async () =>
            {
                await Task.Delay(6000);
                IsLoading = false;
            });
        }


        private async Task ConnectToBluetoothLeDeviceAsync()
        {
            try
            {
                var selectedDevice = SelecteBluetoothDevice;
                ConnectionErrorMessage = "";

                if (selectedDevice == null)
                    throw new Exception("Bluetooth device is not selected.");

                ConnectionButtonContent = "Pairing";
                //Try to pair with device
                var result = await PairingHelper.PairDeviceAsync(selectedDevice.Id);

                //We paired with device
                if (result.Status != "Paired")
                    throw new Exception($"Bluetooth device {selectedDevice.Name} failed to pair.");

                //For now we don't need this
                //var selectedItem = (WatcherDevice)PairedCollection.Where(device => device.Id == selectedDevice.Id);

                SelectedDeviceId = selectedDevice.Id;
                SelectedDeviceName = selectedDevice.Name;

                //ConnectionButtonContent = "Connecting";
                //var connectResult = await CtrlDevice.ConnectAsync(SelectedDeviceId);
                //if (!connectResult.IsConnected)
                //    throw new Exception(connectResult.ErrorMessage);

                Console.WriteLine("*********************************************");
                Console.WriteLine("*********************************************");
                Console.WriteLine("*********************************************");

                //_unpairedWatcher.Stop();
                //_pairedWatcher.Stop();
                //NavigationService.NavigateTo<FpvCtrlViewModel>();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                ConnectionButtonContent = "Connect";
               
            }

        }
        private void ConnectToBluetoothLeDeviceExceptionHandler(Exception ex)
        {
            ConnectionErrorMessage = ex.Message;
        }
        public async Task<WatcherDevice> PairToBluetoothLeDeviceAsync()
        {
            WatcherDevice selectedDevice = new WatcherDevice();
            try
            {
                selectedDevice = SelecteBluetoothDevice;
                ConnectionErrorMessage = "";

                if (selectedDevice == null)
                    throw new Exception("Bluetooth device is not selected.");

                ConnectionButtonContent = "Pairing";
                //Try to pair with device
                var result = await PairingHelper.PairDeviceAsync(selectedDevice.Id);

                //We paired with device
                if (result.Status != "Paired")
                    throw new Exception($"Bluetooth device {selectedDevice.Name} failed to pair.");

                //For now we don't need this
                //var selectedItem = (WatcherDevice)PairedCollection.Where(device => device.Id == selectedDevice.Id);

                SelectedDeviceId = selectedDevice.Id;
                SelectedDeviceName = selectedDevice.Name;

              
                Console.WriteLine("*********************************************");
                Console.WriteLine("*********************************************");
                Console.WriteLine("*********************************************");

                // Get all paired devices on computer
                //foreach (var pairedDevice in PairedCollection)
                //{
                //    // Check if we already paired with FPV.Ctrl device
                //    if (pairedDevice.IsPaired && pairedDevice.Name.Contains("FPV.Ctrl"))
                //    {
                //        if (pairedDevice.Id != SelectedDeviceId)
                //            await PairingHelper.UnpairDeviceAsync(pairedDevice.Id);
                //    }
                //}

                return selectedDevice;
            }
            catch (Exception)
            {
                return null;
            }
            finally
            {
                ConnectionButtonContent = "Connect";
            }

        }

    }
}
