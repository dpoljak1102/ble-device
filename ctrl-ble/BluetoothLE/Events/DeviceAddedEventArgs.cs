using BluetoothLE.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluetoothLE.Events
{
    public class DeviceAddedEventArgs
    {
        public WatcherDevice Device { get; set; }
    }
}
