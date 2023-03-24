using BluetoothLE;
using BluetoothLE.Events;
using BluetoothLE.Model;
using CtrlBluetoothLE.Core;
using CtrlBluetoothLE.Service.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CtrlBluetoothLE.ViewModels
{
    public class ViewModelBase : ObservableObject
    {
        protected CtrlDeviceWatcher _unpairedWatcher;
        protected CtrlDeviceWatcher _pairedWatcher;


        private CtrlDevice _ctrlDevice;
        public CtrlDevice CtrlDevice
        {
            get { return _ctrlDevice; }
            set { _ctrlDevice = value; OnPropertyChanged(); }
        }

        private string _selectedDeviceName;
        public string SelectedDeviceName
        {
            get { return _selectedDeviceName; }
            set { _selectedDeviceName = value; OnPropertyChanged(); }
        }

        private string _selectedDeviceId;
        public string SelectedDeviceId
        {
            get { return _selectedDeviceId; }
            set { _selectedDeviceId = value; OnPropertyChanged(); }
        }

        private int _selectedDeviceBattery;
        public int SelectedDeviceBattery
        {
            get { return _selectedDeviceBattery; }
            set { _selectedDeviceBattery = value; OnPropertyChanged(); }
        }

        private ObservableCollection<WatcherDevice> _unpairedCollection;
        public ObservableCollection<WatcherDevice> UnpairedCollection
        {
            get { return _unpairedCollection; }
            set { _unpairedCollection = value;  OnPropertyChanged(); }
        }

        private ObservableCollection<WatcherDevice> _pairedCollection;
        public ObservableCollection<WatcherDevice> PairedCollection
        {
            get { return _pairedCollection; }
            set { _pairedCollection = value; OnPropertyChanged(); }
        }

        private INavigationService _navigationService;
        public INavigationService NavigationService
        {
            get { return _navigationService; }
            set { _navigationService = value; OnPropertyChanged(); }
        }


        #region BLUETOOTH DEVICE PICKER

        /// <summary>
        /// Type of CollectionView doest not support changes to its SourceCollection from a thread different from the Dispatcher thread
        /// We createrd colletion on UI thread so we need to run it on UI thread
        /// </summary>
        /// <param name="actrion"></param>
        /// <returns></returns>
        private async Task SourceCollectionRunOnUiThread(Action actrion)
        {
            await App.Current.Dispatcher.InvokeAsync(() =>
            {
                actrion();
            });
        }


        protected async void OnPaired_DeviceRemoved(object sender, DeviceRemovedEventArgs e)
        {
            await SourceCollectionRunOnUiThread(() =>
            {
                try
                {
                    if (PairedCollection.Count > 0)
                    {
                        var foundItem = PairedCollection.FirstOrDefault(a => a.Id == e.Device.Id);
                        if (foundItem != null)
                            PairedCollection.Remove(foundItem);

                        Console.WriteLine("Paired device removed: " + e.Device.Name);
                    }
                }
                catch
                {
                    //Leave this empty
                }

            });
        }


        protected async void OnPaired_DeviceAdded(object sender, DeviceAddedEventArgs e)
        {

            await SourceCollectionRunOnUiThread(() =>
            {

                PairedCollection.Add(e.Device);
                Console.WriteLine("Paired device added: " + e.Device.Name);
            });

        }

        protected async void OnDeviceRemoved(object sender, DeviceRemovedEventArgs e)
        {

            await SourceCollectionRunOnUiThread(() =>
            {
                try
                {

                    if (UnpairedCollection.Count > 0)
                    {
                        var foundItem = UnpairedCollection.FirstOrDefault(a => a.Id == e.Device.Id);
                        if (foundItem != null)
                            UnpairedCollection.Remove(foundItem);

                        Console.WriteLine("Unpaired device removed: " + e.Device.Name);
                    }
                }
                catch
                {
                    //LEAVE THIS EMTPY
                }
            });
        }
        protected async void OnDeviceAdded(object sender, DeviceAddedEventArgs e)
        {

            await SourceCollectionRunOnUiThread(() =>
            {

                if (e.Device != null && e.Device.Name.Contains("FPV.Ctrl"))
                {
                    UnpairedCollection.Add(e.Device);
                    Console.WriteLine("Unpaired device added: " + e.Device.Name);
                }
            });


        }

        #endregion


    }
}
