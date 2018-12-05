using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLE
{
    class CalBuffer
    {
        public double Cost { set; get; }
        public double[] Pos { set; get; } = new double[3];
        public CalBuffer() { }
    }
    class PositionCal
    {
        private double Co = 285692.36935118f;
        public int Times { get; set; } = 100;
        public float Alpha { get; set; } = 0.001f;
        public double[] OwnPosition { set; get; } 
        public List<CalUnit> MagData { set; get; }
        public PositionCal(double[] position, List<CalUnit> MagData)
        {
            this.OwnPosition = position;
            this.MagData = MagData;

            //Input data : Initial position, Coil positioni & receive signal
        }

        public CalBuffer PartCalculation(double[] cal, CalUnit magInfo)
        {
            CalBuffer result = new CalBuffer();
            double[] dif = new double[3];
            double a1, a2;
            for (var i = 0; i< 3; i++)
            {
                dif[i] = cal[i] - magInfo.Position[i];
            }

            a1 = Math.Pow(dif[0], 2) / 16 + Math.Pow(dif[1], 2) / 16 + Math.Pow(dif[1], 2) / 25;
            a2 = Co * Math.Pow(a1, -1.5) - magInfo.Signal;
            result.Cost = Math.Pow(a2, 2);
            double gradCo = -3 * Co * Math.Pow(a1, -2.5) * a2;
            double[] grad = new double[3];
            grad[0] = gradCo * dif[0] / 8;
            grad[1] = gradCo * dif[1] / 8;
            grad[2] = gradCo * dif[2] * 2 / 25;
            result.Pos = grad;
            return result;
        }

        public CalBuffer Calculation()
        {
            CalBuffer result = new CalBuffer();
            double obj = 0;
            double talpha = Alpha / MagData.Count;
            for(var i = 0; i < Times; i++)
            {
                obj = 0;
                //Debug.WriteLine("Position Before: (" + OwnPosition[0] + ", " + OwnPosition[1] + ", " + OwnPosition[2] + ")");
                for (var j = 0; j < MagData.Count; j++)
                {
                    CalBuffer buffer =  PartCalculation(OwnPosition, MagData[j]);
                    obj += buffer.Cost;
                    for(var k = 0; k < 3; k++)
                    {
                  //      Debug.WriteLine("Test: " + OwnPosition[k]);
                        OwnPosition[k] -= talpha * buffer.Pos[k];
                  //      Debug.WriteLine("Test: " + OwnPosition[k]);
                    }
                }
                //Debug.WriteLine("Position now: (" + OwnPosition[0] + ", " + OwnPosition[1] + ", " + OwnPosition[2] + ")");
                //Debug.WriteLine("Obj now: " + obj);
            }
            result.Cost = obj;
            result.Pos = OwnPosition;
            return result;
        }
    }
}
