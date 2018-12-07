using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Diagnostics;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using System.Text;
using Windows.Security.Cryptography;
using Windows.Storage.Streams;

// 空白頁項目範本已記錄在 https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x404

namespace BLE
{
    /// <summary>
    /// 可以在本身使用或巡覽至框架內的空白頁面。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public static MainPage Current;
        public string SelectedBleDeviceId;
        public string SelectedBleDeviceName = "No device selected";

        public MainPage()
        {
            this.InitializeComponent();
            MyFrame.Navigate(typeof(PairPage));

        }

        private void Scenario1_Click(object sender, RoutedEventArgs e)
        {
            MyFrame.Navigate(typeof(PairPage));
        }

        private void Scenario2_Click(object sender, RoutedEventArgs e)
        {
            MyFrame.Navigate(typeof(CalculationPage));
        }
    }
}
