using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLE
{
    class SensorNode : BLENode
    {
        public List<CalUnit> MeasureData { get;  set; }
        SensorNode(string BluetoothLEid, string Name) : base(BluetoothLEid, Name) { }

    }
}
