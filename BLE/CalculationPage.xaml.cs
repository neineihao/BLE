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
        public ObservableCollection<Ellipse> SensorContainer = new ObservableCollection<Ellipse>();
        public ObservableCollection<Ellipse> CoilContainer = new ObservableCollection<Ellipse>();
        public ObservableCollection<Ellipse> EllipseContainer = new ObservableCollection<Ellipse>();
        private Random rnd = new Random();
        public CalculationPage()
        {
            this.InitializeComponent();
            CoilDeviced = App.AnchorCollection;
            SensorDeviced = App.SensorCollection;
            double[] position = new double[3]{ -10, 20, 0};
            CanvasPackage CatchCanvas = CreateDrawObj(position, 3);
            PlacePart.Children.Add(CatchCanvas.OwnCanvas);
            if(CoilDeviced != null)
            {
                for (var i = 0; i < CoilDeviced.Count; i++)
                {
                    CanvasPackage tempCanvasPackage = CreateDrawObj(CoilDeviced[i].Position, 0);
                    PlacePart.Children.Add(tempCanvasPackage.OwnCanvas);
                    EllipseContainer.Add(tempCanvasPackage.OwnEllipse);
                }
            }
            if(SensorDeviced != null)
            {
                for (var i = 0; i < SensorDeviced.Count; i++)
                {
                    CanvasPackage tempCanvasPackage = CreateDrawObj(SensorDeviced[i].Position, 1);
                    PlacePart.Children.Add(tempCanvasPackage.OwnCanvas);
                    EllipseContainer.Add(tempCanvasPackage.OwnEllipse);
                }
            }
           
        }


        private double[] Project2D(double[] origin)
        {
            double[] result = new double[2];
            result[0] = 813 - 7 * origin[0] + 10 * origin[1];
            result[1] = 510 + 4 * origin[0] - 10 * origin[2];
            return result;
        }

        private CanvasPackage CreateDrawObj(double []position, int colorCase)
        {
            double[] position2D = new double[2];
            position2D = Project2D(position);
            return CreateAnCanvas(position2D[0], position2D[1], colorCase);
            
        }

        private async void Move(object sender, RoutedEventArgs e)
        {
            double[] position = new double[3] { 0, 0, 0 };
            CanvasPackage CatchCanvas = CreateDrawObj(position, 1);
            PlacePart.Children.Add(CatchCanvas.OwnCanvas);
            EllipseContainer.Add(CatchCanvas.OwnEllipse);

            for (var i = 0; i < 20; i++)
            {
                position[0] += 1;
                SetPostion(EllipseContainer[0],position);
                await Task.Delay(TimeSpan.FromSeconds(0.005));
            }

            for (var i = 0; i < 20; i++)
            {
                position[1] += 1;
                SetPostion(EllipseContainer[0], position);
                await Task.Delay(TimeSpan.FromSeconds(0.005));
            }

            for (var i = 0; i < 20; i++)
            {
                position[2] += 1;
                SetPostion(EllipseContainer[0], position);
                await Task.Delay(TimeSpan.FromSeconds(0.005));
            }
        }


        public void SetPostion(Ellipse obj, double[] position3D)
        {
            double[] position2D = Project2D(position3D);
            obj.SetValue(Canvas.LeftProperty, position2D[0]);
            obj.SetValue(Canvas.TopProperty, position2D[1]);

        }

        public CanvasPackage CreateAnCanvas(double x, double y, int colorCase)
        {
            Canvas testCanvas = new Canvas();
            Ellipse testEllipse = CreateAnEllipse(colorCase);
            testCanvas.Children.Add(testEllipse);
            testEllipse.SetValue(Canvas.LeftProperty, x);
            testEllipse.SetValue(Canvas.TopProperty, y);
            CanvasPackage temp = new CanvasPackage(testCanvas, testEllipse);
            return temp;
        }

        public Ellipse CreateAnEllipse(int colorCase)
        {
            Ellipse newRectangle = new Ellipse();
            SolidColorBrush azureBrush = new SolidColorBrush
            {
                Color = Colors.Azure
            };
            SolidColorBrush blackBrush = new SolidColorBrush
            {
                Color = Colors.Black
            };
            SolidColorBrush coralBrush = new SolidColorBrush
            {
                Color = Colors.Coral
            };
            SolidColorBrush chocolateBrush = new SolidColorBrush
            {
                Color = Colors.Chocolate
            };


            switch (colorCase)
            {
                case 0:
                    newRectangle.Fill = azureBrush;
                    break;
                case 1:
                    newRectangle.Fill = coralBrush;
                    break;

                default:
                    newRectangle.Fill = chocolateBrush;
                    break;
            }
            
            newRectangle.Height = 50;
            newRectangle.Width = 50;
            newRectangle.StrokeThickness = 4;
            newRectangle.Opacity = 0.8;
            newRectangle.Stroke = blackBrush;
            
            return newRectangle;
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
