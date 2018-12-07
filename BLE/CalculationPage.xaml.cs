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
        private Random rnd = new Random();
        public CalculationPage()
        {
            this.InitializeComponent();
            CoilDeviced = App.AnchorCollection;
            SensorDeviced = App.SensorCollection;
            for(var i=0; i< 5; i++)
            {
                var x = i + rnd.NextDouble();
                var y = i + rnd.NextDouble();
                TestDraw.Add(new TestNode(x, y));
            }

        }

    }

    
}
