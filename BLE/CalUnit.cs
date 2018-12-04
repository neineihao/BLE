using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLE
{
    class CalUnit
    {
        public float[] Position { get; set; } = new float[3];
        public float Signal { get; set; }
        public CalUnit(float[] position, float signal)
        {
            this.Signal = signal;
            this.Position = position;
        }
    }
}
