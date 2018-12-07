using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Diagnostics;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.UI.Core;
using System.Text;
using Windows.Security.Cryptography;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Shapes;
using Windows.UI;

// 空白頁項目範本已記錄在 https://go.microsoft.com/fwlink/?LinkId=234238

namespace BLE
{
    /// <summary>
    /// 可以在本身使用或巡覽至框架內的空白頁面。
    /// </summary>
    public sealed partial class CalculationPage : Page
    {
        private ObservableCollection<MagNode> CoilDeviced = new ObservableCollection<MagNode>();
        private ObservableCollection<SensorNode> SensorDeviced = new ObservableCollection<SensorNode>();
        private ObservableCollection<TestNode> TestDraw = new ObservableCollection<TestNode>();
        public ObservableCollection<Ellipse> EllipseContainer = new ObservableCollection<Ellipse>();
        private Random rnd = new Random();
        public CalculationPage()
        {
            this.InitializeComponent();
            CoilDeviced = App.AnchorCollection;
            SensorDeviced = App.SensorCollection;
            CanvasPackage CatchCanvas = CreateAnCanvas(815, 510);
            PlacePart.Children.Add(CatchCanvas.OwnCanvas);
            //CanvasPackage CatchCanvas = CreateAnCanvas(20, 100);
            //PlacePart.Children.Add(CatchCanvas.OwnCanvas);
            //CatchCanvas.OwnEllipse.SetValue(Canvas.LeftProperty, 100);
            //EllipseContainer.Add(CatchCanvas.OwnEllipse);
            //EllipseContainer[0].SetValue(Canvas.TopProperty, 100);
            //SetPostion(EllipseContainer[0], 525.3, 533.5);
            /*
            for (var i = 0; i < 5; i++)
            {
                TestDraw.Add(new TestNode(i * 100, i * 100));
                Debug.WriteLine("" + TestDraw.Count);
            }
            for (var i = 0; i < TestDraw.Count; i++)
            {
                TestDraw[i].DrawObj();
                PlacePart.Children.Add(TestDraw[i].OwnCanvas);
            }
            */
        }

        private async void Move(object sender, RoutedEventArgs e)
        {

            CanvasPackage CatchCanvas = CreateAnCanvas(200, 400);
            PlacePart.Children.Add(CatchCanvas.OwnCanvas);
            //CatchCanvas.OwnEllipse.SetValue(Canvas.LeftProperty, 100);
            EllipseContainer.Add(CatchCanvas.OwnEllipse);
            //EllipseContainer[0].SetValue(Canvas.TopProperty, 100);
            //SetPostion(EllipseContainer[0], 525.3, 533.5);
            //TestMoving(EllipseContainer[0], 200, 400, 500, 500);
            for(var i = 0; i< 100;  i++)
            {
                for(var j = 0; j< 20; j ++)
                {

                    SetPostion(EllipseContainer[0], 200 + 5 * i, 400 - j * 10);
                    await Task.Delay(TimeSpan.FromSeconds(0.001));
                }
            }
        }



        private async void TestMoving(Ellipse obj,int initX, int initY, int moveX, int moveY)
        {
            for (var i = initX; i< moveX; i++)
            {
                for(var j = initY; j < moveY; j++)
                {
                    SetPostion(obj, i , j);
                    await Task.Delay(TimeSpan.FromSeconds(0.01));
                }
            }
        }


        public void SetPostion(Ellipse obj, double x, double y)
        {
            obj.SetValue(Canvas.LeftProperty, x);
            obj.SetValue(Canvas.TopProperty, y);

        }

        public CanvasPackage CreateAnCanvas(int x, int y)
        {
            Canvas testCanvas = new Canvas();
            Ellipse testEllipse = CreateAnEllipse();
            testCanvas.Children.Add(testEllipse);
            testEllipse.SetValue(Canvas.LeftProperty, x);
            testEllipse.SetValue(Canvas.TopProperty, y);
            CanvasPackage temp = new CanvasPackage(testCanvas, testEllipse);
            return temp;
        }

        public Ellipse CreateAnEllipse()
        {
            Ellipse blueRectangle = new Ellipse();
            blueRectangle.Height = 50;
            blueRectangle.Width = 50;
            SolidColorBrush blueBrush = new SolidColorBrush();
            blueBrush.Color = Colors.Azure;
            SolidColorBrush blackBrush = new SolidColorBrush();
            blackBrush.Color = Colors.Black;
            blueRectangle.StrokeThickness = 4;
            blueRectangle.Opacity = 0.8;
            blueRectangle.Stroke = blackBrush;
            blueRectangle.Fill = blueBrush;
            return blueRectangle;
            //blueRectangle.Width = 400;

        }
    }
    
    public class CanvasPackage
    {
        public Canvas OwnCanvas { get; set; }
        public Ellipse OwnEllipse { get; set; }
        public CanvasPackage(Canvas x, Ellipse y)
        {
            this.OwnCanvas = x;
            this.OwnEllipse = y;
        }

    }
    
}
