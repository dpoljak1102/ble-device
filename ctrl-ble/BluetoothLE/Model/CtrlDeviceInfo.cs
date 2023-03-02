using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluetoothLE.Model
{
    public class CtrlDeviceInfo
    {
        public string DeviceId { get; set; }
        public string Name { get; set; }
        public string Firmware { get; set; }
        public string Hardware { get; set; }
        public string Manufacturer { get; set; }
        public string SerialNumber { get; set; }
        public string ModelNumber { get; set; }
        public int BatteryPercent { get; set; }
        public int GimbalLeftX { get; set; }
        public int GimbalLeftY { get; set; }
        public int GimbalRightX { get; set; }
        public int GimbalRightY { get; set; }
        public int LeftSwitchButton { get; set; }
        public int RightSwitchButton { get; set; }
        public int LeftPressButton { get; set; }
        public int RightPressButton { get; set; }
        public int ButtonA { get; set; }
        public int ButtonB { get; set; }
        public int Ghost { get; set; }
    }
}
