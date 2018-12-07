using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using System.ComponentModel;


namespace BLE
{
    class MovingPoint : Canvas, INotifyPropertyChanged
    {
        private double _x;
        private double _y;
        public double X
        {
            set
            {
                _x = value;
                OnPropertyChanged("X");
            }

            get
            {
                return _x;
            }
        }

        public double Y
        {
            set
            {
                _y = value;
                OnPropertyChanged("Y");
            }

            get
            {
                return _y;
            }
        }


        public MovingPoint(double x, double y) {
            this._x = x;
            this._y = y;
            this.Width = 10;
            this.Height = 10;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
