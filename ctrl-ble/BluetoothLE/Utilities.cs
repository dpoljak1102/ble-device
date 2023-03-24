using BluetoothLE.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Security.Cryptography;
using Windows.Storage.Streams;

namespace BluetoothLE
{
    public static class Utilities
    {
        public static async Task<string> ReadCharacteristicValueAsync(List<BluetoothAttribute> characteristics, string characteristicName)
        {
            var characteristic = characteristics.FirstOrDefault(a => a.Name == characteristicName)?.characteristic;
            if (characteristic == null)
                return "0";

            var readResult = await characteristic.ReadValueAsync();

            if (readResult.Status == GattCommunicationStatus.Success)
            {
                byte[] data;
                CryptographicBuffer.CopyToByteArray(readResult.Value, out data);

                if (characteristic.Uuid.Equals(GattCharacteristicUuids.BatteryLevel))
                {
                    try
                    {
                        // battery level is encoded as a percentage value in the first byte according to
                        // https://www.bluetooth.com/specifications/gatt/viewer?attributeXmlFile=org.bluetooth.characteristic.battery_level.xml
                        return data[0].ToString();
                    }
                    catch (ArgumentException)
                    {
                        return "0";
                    }
                }
                else
                    return Encoding.UTF8.GetString(data);
            }
            else
            {
                return $"Read failed: {readResult.Status}";
            }
        }


        /// <summary>
        ///     Converts from standard 128bit UUID to the assigned 32bit UUIDs. Makes it easy to compare services
        ///     that devices expose to the standard list.
        /// </summary>
        /// <param name="uuid">UUID to convert to 32 bit</param>
        /// <returns></returns>
        public static ushort ConvertUuidToShortId(Guid uuid)
        {
            // Get the short Uuid
            var bytes = uuid.ToByteArray();
            var shortUuid = (ushort)(bytes[0] | (bytes[1] << 8));
            return shortUuid;
        }

        /// <summary>
        ///     Converts from a buffer to a properly sized byte array
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static byte[] ReadBufferToBytes(IBuffer buffer)
        {
            var dataLength = buffer.Length;
            var data = new byte[dataLength];
            using (var reader = DataReader.FromBuffer(buffer))
            {
                reader.ReadBytes(data);
            }
            return data;
        }


        public static CtrlDeviceInfo ParseDataValueFromControllerDevice(byte[] data)
        {
            var deviceValue = new CtrlDeviceInfo();
            deviceValue.GimbalLeftX = BitConverter.ToUInt16(data, 2);
            deviceValue.GimbalLeftY = BitConverter.ToUInt16(data, 4);
            deviceValue.GimbalRightX = BitConverter.ToUInt16(data, 6);
            deviceValue.GimbalRightY = BitConverter.ToUInt16(data, 8);

            //Buttons [14]
            string buttonsArray = Convert.ToString(data[14], 2).PadLeft(8, '0');
            deviceValue.LeftSwitchButton = CalculateTwoWaySwitch(buttonsArray, 7, 6);
            deviceValue.RightSwitchButton = CalculateTwoWaySwitch(buttonsArray, 5, 4);
            deviceValue.LeftPressButton = CalculateButton(buttonsArray, 3);
            deviceValue.RightPressButton = CalculateButton(buttonsArray, 2);
            deviceValue.ButtonA = CalculateButton(buttonsArray, 1);
            deviceValue.ButtonB = CalculateButton(buttonsArray, 0);
            deviceValue.Ghost = GhostMode(Convert.ToString(data[19], 2).PadLeft(8, '0'));

            //Note :  GHOST PACKET(20) active when value is 00000000 
            //Console.WriteLine($"Packet 20 : {Convert.ToString(data[19], 2).PadLeft(8, '0')}");
            //Console.WriteLine($"Ghost mode : {GhostMode(Convert.ToString(data[19], 2).PadLeft(8, '0'))}");

            //Note : When we write to service, then packets coming to 18
            //Console.WriteLine($"Packet 18 : {Convert.ToString(data[18], 2).PadLeft(8, '0')}");

            return deviceValue;

        }

        private static int CalculateTwoWaySwitch(string byteString, int bitIndexUp, int bitIndexDown)
        {
            //Two way switch is pressed up
            if (byteString[bitIndexUp] == '1')
                return 0;
            //Two way switch is pressed down
            else if (byteString[bitIndexDown] == '1')
                return 2;
            //Two way switch is pressed middle
            return 1;
        }

        private static int CalculateButton(string byteString, int bitIndexUp)
        {
            //Button pressed
            if (byteString[bitIndexUp] == '1')
                return 1;
            //Button relised
            return 0;
        }

        private static int GhostMode(string byteString)
        {
            //Ghost mode off
            if (byteString.Contains('1'))
                return 0;
            //Ghost mode on
            return 1;
        }

    }
}
