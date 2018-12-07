using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using Windows.UI.Xaml.Shapes;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI;

namespace BLE
{
    class TestNode : INotifyPropertyChanged
    {
        public Canvas OwnCanvas { set; get; }
        public Ellipse OwnEllipse { set; get; }
        private int x;
        private int y;
        public int X { set; get; }
        public int Y { set; get; }

        public TestNode(int x, int y)
        {
            this.x = x;
            this.y = y;
            OwnCanvas = new Canvas();
            OwnEllipse = new Ellipse();
        }

        public void DrawObj()
        {
            OwnCanvas.Children.Add(OwnEllipse);
            SetPostion(x, y);
        }

        public void EllipseSetting()
        {
            OwnEllipse.Height = 100;
            OwnEllipse.Width = 200;
            SolidColorBrush blueBrush = new SolidColorBrush();
            blueBrush.Color = Colors.Blue;
            OwnEllipse.Fill = blueBrush;
            OwnEllipse.Opacity = 0.5;
        }

        public void SetPostion(int x, int y)
        {
            this.x = x;
            this.y = y;
            OwnEllipse.SetValue(Canvas.TopProperty, y);
            OwnEllipse.SetValue(Canvas.LeftProperty, x);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

}
