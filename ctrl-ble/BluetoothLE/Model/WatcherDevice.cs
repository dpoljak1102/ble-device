﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluetoothLE.Model
{
    public class WatcherDevice
    {
        public string Id { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsDefault { get; set; }
        public string Name { get; set; }
        public bool IsPaired { get; set; }
        public bool IsConnected { get; set; }
        public string Kind { get; set; }
        public Dictionary<string, object> Properties { get; set; }
    }
}
