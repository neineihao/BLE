using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLE
{
    class CalUnit
    {
        public double[] Position { get; set; } = new double[3];
        public float Signal { get; set; }
        public CalUnit(double[] position, float signal)
        {
            this.Signal = signal;
            this.Position = position;
        }
    }
}
