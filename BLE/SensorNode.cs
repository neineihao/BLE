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

        private PositionCal PositionManager = new PositionCal();

        SensorNode(string BluetoothLEid, string Name) : base(BluetoothLEid, Name) {}

        public void GetPosition()
        {
            PositionManager.OwnPosition = Position;
            PositionManager.MagData = MeasureData;
            // May have problem here
            CalBuffer buffer = PositionManager.Calculation();
            Position = buffer.Pos;
            OnPropertyChanged("Position");
        }
    }
}
