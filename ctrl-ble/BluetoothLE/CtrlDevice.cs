using BluetoothLE.Events;
using BluetoothLE.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.Security.Cryptography;
using Windows.Storage.Streams;

namespace BluetoothLE
{
    public class CtrlDevice
    {
        private BluetoothLEDevice _ctrlDevice = null;
        private List<BluetoothAttribute> _serviceCollection = new List<BluetoothAttribute>();

        private BluetoothAttribute _ctrlMeasurementAttribute;
        private BluetoothAttribute _ctrlAttribute;
        private GattCharacteristic _ctrlMeasurementCharacteristic;

        /// <summary>
        /// Occurs when [connection status changed].
        /// </summary>
        public event EventHandler<Events.ConnectionStatusChangedEventArgs> ConnectionStatusChanged;
        /// <summary>
        /// Raises the <see cref="E:ConnectionStatusChanged" /> event.
        /// </summary>
        /// <param name="e">The <see cref="Events.ConnectionStatusChangedEventArgs"/> instance containing the event data.</param>
        protected virtual void OnConnectionStatusChanged(Events.ConnectionStatusChangedEventArgs e)
        {
            ConnectionStatusChanged?.Invoke(this, e);
        }

        /// <summary>
        /// Occurs when [value changed].
        /// </summary>
        public event EventHandler<Events.DeviceChangedEventArgs> ValueChanged;
        /// <summary>
        /// Raises the <see cref="E:ValueChanged" /> event.
        /// </summary>
        /// <param name="e">The <see cref="Events.DeviceChangedEventArgs"/> instance containing the event data.</param>
        protected virtual void OnValueChanged(Events.DeviceChangedEventArgs e)
        {
            ValueChanged?.Invoke(this, e);
        }


        public async Task<ConnectionResult> ConnectAsync(string deviceId)
        {
            _ctrlDevice = await BluetoothLEDevice.FromIdAsync(deviceId);
            if (_ctrlDevice == null)
            {
                return new Model.ConnectionResult()
                {
                    IsConnected = false,
                    ErrorMessage = "Could not find specified device"
                };
            }

            if (!_ctrlDevice.DeviceInformation.Pairing.IsPaired)
            {
                _ctrlDevice = null;
                return new Model.ConnectionResult()
                {
                    IsConnected = false,
                    ErrorMessage = "Device is not paired"
                };
            }

            // we should always monitor the connection status
            _ctrlDevice.ConnectionStatusChanged -= DeviceConnectionStatusChanged;
            _ctrlDevice.ConnectionStatusChanged += DeviceConnectionStatusChanged;

            var isReachable = await GetDeviceServicesAsync();
            if (!isReachable)
            {
                _ctrlDevice = null;
                return new Model.ConnectionResult()
                {
                    IsConnected = false,
                    ErrorMessage = "Heart rate device is unreachable (i.e. out of range or shutoff)"
                };
            }

            CharacteristicResult characteristicResult;
            characteristicResult = await SetupCtrlCharacteristic();
            if (!characteristicResult.IsSuccess)
                return new Model.ConnectionResult()
                {
                    IsConnected = false,
                    ErrorMessage = characteristicResult.Message
                };


            // we could force propagation of event with connection status change, to run the callback for initial status
            DeviceConnectionStatusChanged(_ctrlDevice, null);

            return new Model.ConnectionResult()
            {
                IsConnected = _ctrlDevice.ConnectionStatus == BluetoothConnectionStatus.Connected,
                Name = _ctrlDevice.Name
            };
        }


        private async Task<List<BluetoothAttribute>> GetServiceCharacteristicsAsync(BluetoothAttribute service)
        {
            IReadOnlyList<GattCharacteristic> characteristics = null;
            try
            {
                // Ensure we have access to the device.
                var accessStatus = await service.service.RequestAccessAsync();
                if (accessStatus == DeviceAccessStatus.Allowed)
                {
                    // BT_Code: Get all the child characteristics of a service. Use the cache mode to specify uncached characterstics only 
                    // and the new Async functions to get the characteristics of unpaired devices as well. 
                    var result = await service.service.GetCharacteristicsAsync(BluetoothCacheMode.Uncached);
                    if (result.Status == GattCommunicationStatus.Success)
                    {
                        characteristics = result.Characteristics;
                    }
                    else
                    {
                        characteristics = new List<GattCharacteristic>();
                    }
                }
                else
                {
                    // Not granted access
                    // On error, act as if there are no characteristics.
                    characteristics = new List<GattCharacteristic>();
                }
            }
            catch (Exception)
            {
                characteristics = new List<GattCharacteristic>();
            }

            var characteristicCollection = new List<BluetoothAttribute>();
            characteristicCollection.AddRange(characteristics.Select(a => new BluetoothAttribute(a)));
            return characteristicCollection;
        }


        private async Task<CharacteristicResult> SetupCtrlCharacteristic()
        {
            // GET "Custom service" for CTRL DEVICE
            // Custom service :  b0b63b7a-c440-e683-ce49-752e2bbb5fa0
            _ctrlAttribute = _serviceCollection.Where(serviceName => serviceName.Name.Contains("Custom")).FirstOrDefault();

            if (_ctrlAttribute == null)
            {
                return new CharacteristicResult()
                {
                    IsSuccess = false,
                    Message = "Cannot find Devices service"
                };
            }

            var characteristics = await GetServiceCharacteristicsAsync(_ctrlAttribute);

            // GET Characteristics from custom service for CTRL DEVICE
            // Custom service :  b0b63b7a-c440-e683-ce49-752e2bbb5fa0
            _ctrlMeasurementAttribute = characteristics.Where(character => character.Name.Contains("Custom")).FirstOrDefault();
            if (_ctrlMeasurementAttribute == null)
            {
                return new CharacteristicResult()
                {
                    IsSuccess = false,
                    Message = "Cannot find Custom characteristic"
                };
            }
            _ctrlMeasurementCharacteristic = _ctrlMeasurementAttribute.characteristic;


            // Get all the child descriptors of a characteristics. Use the cache mode to specify uncached descriptors only 
            // and the new Async functions to get the descriptors of unpaired devices as well. 
            var result = await _ctrlMeasurementCharacteristic.GetDescriptorsAsync(BluetoothCacheMode.Uncached);
            if (result.Status != GattCommunicationStatus.Success)
            {
                return new CharacteristicResult()
                {
                    IsSuccess = false,
                    Message = result.Status.ToString()
                };
            }

            if (_ctrlMeasurementCharacteristic.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Notify))
            {
                var status = await _ctrlMeasurementCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify);
                if (status == GattCommunicationStatus.Success)
                    _ctrlMeasurementCharacteristic.ValueChanged += DeviceValueChanged;

                return new CharacteristicResult()
                {
                    IsSuccess = status == GattCommunicationStatus.Success,
                    Message = status.ToString()
                };
            }
            else
            {
                return new CharacteristicResult()
                {
                    IsSuccess = false,
                    Message = "Characteristic does not support Gatt Characteristic Properties Nofity"
                };

            }

        }

        private async Task<bool> GetDeviceServicesAsync()
        {
            // Note: BluetoothLEDevice.GattServices property will return an empty list for unpaired devices. For all uses we recommend using the GetGattServicesAsync method.
            // BT_Code: GetGattServicesAsync returns a list of all the supported services of the device (even if it's not paired to the system).
            // If the services supported by the device are expected to change during BT usage, subscribe to the GattServicesChanged event.
            GattDeviceServicesResult result = await _ctrlDevice.GetGattServicesAsync(BluetoothCacheMode.Uncached);

            if (result.Status == GattCommunicationStatus.Success)
            {
                _serviceCollection.AddRange(result.Services.Select(a => new BluetoothAttribute(a)));
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task DisconnectAsync()
        {
            if (_ctrlDevice != null)
            {
                if (_ctrlMeasurementCharacteristic != null)
                {
                    //NOTE: might want to do something here if the result is not successful
                    var result = await _ctrlMeasurementCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.None);
                    if (_ctrlMeasurementCharacteristic.Service != null)
                        _ctrlMeasurementCharacteristic.Service.Dispose();
                    _ctrlMeasurementCharacteristic = null;
                }

                if (_ctrlMeasurementAttribute != null)
                {
                    if (_ctrlMeasurementAttribute.service != null)
                        _ctrlMeasurementAttribute.service.Dispose();
                    _ctrlMeasurementAttribute = null;
                }

                if (_ctrlAttribute != null)
                {
                    if (_ctrlAttribute.service != null)
                        _ctrlAttribute.service.Dispose();
                    _ctrlAttribute = null;
                }

                _serviceCollection = new List<BluetoothAttribute>();

                _ctrlDevice.Dispose();
                _ctrlDevice = null;

                DeviceConnectionStatusChanged(null, null);
            }
        }

        private void DeviceConnectionStatusChanged(BluetoothLEDevice sender, object args)
        {
            var result = new ConnectionStatusChangedEventArgs()
            {
                IsConnected = sender != null && (sender.ConnectionStatus == BluetoothConnectionStatus.Connected)
            };

            OnConnectionStatusChanged(result);
        }

        private void DeviceValueChanged(GattCharacteristic sender, GattValueChangedEventArgs e)
        {
            byte[] data;
            CryptographicBuffer.CopyToByteArray(e.CharacteristicValue, out data);

            var deviceValues = Utilities.ParseDataValueFromControllerDevice(data);
            var args = new Events.DeviceChangedEventArgs()
            {
                GimbalLeftX = deviceValues.GimbalLeftX,
                GimbalLeftY = deviceValues.GimbalLeftY,
                GimbalRightX = deviceValues.GimbalRightX,
                GimbalRightY = deviceValues.GimbalRightY,
                LeftSwitchButton = deviceValues.LeftSwitchButton,
                RightSwitchButton = deviceValues.RightSwitchButton,
                LeftPressButton = deviceValues.LeftPressButton,
                RightPressButton = deviceValues.RightPressButton,
                ButtonA = deviceValues.ButtonA,
                ButtonB = deviceValues.ButtonB,
                Ghost = deviceValues.Ghost
            };
            OnValueChanged(args);
        }

        public bool IsConnected
        {
            get { return _ctrlDevice != null ? _ctrlDevice.ConnectionStatus == BluetoothConnectionStatus.Connected : false; }
        }

        public async Task<CtrlDeviceInfo> GetDeviceInfoAsync()
        {
            if (_ctrlDevice != null && _ctrlDevice.ConnectionStatus == BluetoothConnectionStatus.Connected)
            {
                var deviceInfoService = _serviceCollection.Where(a => a.Name == "DeviceInformation").FirstOrDefault();
                var deviceInfocharacteristics = await GetServiceCharacteristicsAsync(deviceInfoService);

                var batteryService = _serviceCollection.Where(a => a.Name == "Battery").FirstOrDefault();
                var batteryCharacteristics = await GetServiceCharacteristicsAsync(batteryService);
                //byte battery = await _batteryParser.ReadAsync();

                return new CtrlDeviceInfo()
                {
                    DeviceId = _ctrlDevice.DeviceId,
                    Name = _ctrlDevice.Name,
                    Firmware = await Utilities.ReadCharacteristicValueAsync(deviceInfocharacteristics, "FirmwareRevisionString"),
                    Hardware = await Utilities.ReadCharacteristicValueAsync(deviceInfocharacteristics, "HardwareRevisionString"),
                    Manufacturer = await Utilities.ReadCharacteristicValueAsync(deviceInfocharacteristics, "ManufacturerNameString"),
                    SerialNumber = await Utilities.ReadCharacteristicValueAsync(deviceInfocharacteristics, "SerialNumberString"),
                    ModelNumber = await Utilities.ReadCharacteristicValueAsync(deviceInfocharacteristics, "ModelNumberString"),
                    BatteryPercent = Convert.ToInt32(await Utilities.ReadCharacteristicValueAsync(batteryCharacteristics, "BatteryLevel"))
                };
            }
            else
            {
                return new CtrlDeviceInfo();
            }
        }

        public async Task Calibration()
        {
            //Write to CTRL
            if (_ctrlMeasurementCharacteristic.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Write))
            {
                var status = await _ctrlMeasurementCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify);
                if (status == GattCommunicationStatus.Success)
                {
                    var calibrationMode = new byte[8];
                    IBuffer buffer = calibrationMode.AsBuffer();
                    await _ctrlMeasurementCharacteristic.WriteValueAsync(buffer);
                }


            }

        }
    }
}
