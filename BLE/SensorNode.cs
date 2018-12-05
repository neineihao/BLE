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

        private PositionCal PositionManager;

        public SensorNode(string BluetoothLEid, string Name, double[] position) : base(BluetoothLEid, Name, position) {}

        public void GetPosition()
        {
            //PositionManager.OwnPosition = Position;
            //PositionManager.MagData = MeasureData;
            // May have problem here
            PositionManager = new PositionCal(Position, MeasureData);
            CalBuffer buffer = PositionManager.Calculation();
            Position = buffer.Pos;
            OnPropertyChanged("Position");
        }
    }
}
