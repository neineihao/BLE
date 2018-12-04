using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLE
{
    class SensorNode
    {
        public float[] Position { set; get; } = new float[3];
        public List<CalUnit> MeasureData { get;  set; }

        

    }
}
