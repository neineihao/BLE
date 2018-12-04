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

        private ObservableCollection<BluetoothLEDeviceDisplay> KnownDevices = new ObservableCollection<BluetoothLEDeviceDisplay>();
        private List<DeviceInformation> UnknownDevices = new List<DeviceInformation>();
        private ObservableCollection<BluetoothLEAttributeDisplay> ServiceCollection = new ObservableCollection<BluetoothLEAttributeDisplay>();
        private ObservableCollection<MagNode> MagDeviced = new ObservableCollection<MagNode>();
        private ObservableCollection<SensorNode> SensorDeviced = new ObservableCollection<SensorNode>();
        private DeviceWatcher deviceWatcher;
        private List<float> dataList = new List<float>(); 
        //private BluetoothLEDeviceDisplay SelectedBleDevice;
        public string SelectedBleDeviceId;
        public string SelectedBleDeviceId2;
        public float temp;
        private string testID = "BluetoothLE#BluetoothLEe8:2a:ea:ca:da:9a-f5:68:a7:84:e8:f6";
        private MagNode MagNode;

        #region Uuid
        Guid MagUUID = new Guid("00002AA1-0000-1000-8000-00805f9b34fb");
        Guid WriteUUID = new Guid("00002A57-0000-1000-8000-00805f9b34fb");
        Guid HEALTH_THERMOMETER_UUID = new Guid("00001809-0000-1000-8000-00805f9b34fb");
        #endregion



        public MainPage()
        {
            this.InitializeComponent();

        }

        #region Device Watcher
        private void Start_Button(object sender, RoutedEventArgs e)
        {
            if (deviceWatcher == null)
            {
                StartBleDeviceWatcher();
                Info.Text = "Device watcher start";
            }
            else
            {
                Info.Text = "Already start device watcher";
            }
        }
        private void Stop_Button(object sender, RoutedEventArgs e)
        {
            if (deviceWatcher != null)
            {
                StopBleDeviceWatcher();
                Info.Text = "Device watcher stop";
            }
            else
            {
                Info.Text = "Nothing Stop";
            }
        }

        /// <summary>
        /// Starts a device watcher that looks for all nearby Bluetooth devices (paired or unpaired). 
        /// Attaches event handlers to populate the device collection.
        /// </summary>
        private void StartBleDeviceWatcher()
        {
            // Additional properties we would like about the device.
            // Property strings are documented here https://msdn.microsoft.com/en-us/library/windows/desktop/ff521659(v=vs.85).aspx
            string[] requestedProperties = { "System.Devices.Aep.DeviceAddress", "System.Devices.Aep.IsConnected", "System.Devices.Aep.Bluetooth.Le.IsConnectable" };

            // BT_Code: Example showing paired and non-paired in a single query.
            string aqsAllBluetoothLEDevices = "(System.Devices.Aep.ProtocolId:=\"{bb7bb05e-5972-42b5-94fc-76eaa7084d49}\")";

            deviceWatcher =
                        DeviceInformation.CreateWatcher(
                        aqsAllBluetoothLEDevices,
                        requestedProperties,
                        DeviceInformationKind.AssociationEndpoint);

            // Register event handlers before starting the watcher.
            deviceWatcher.Added += DeviceWatcher_Added;
            deviceWatcher.Updated += DeviceWatcher_Updated;
            deviceWatcher.Removed += DeviceWatcher_Removed;
            deviceWatcher.EnumerationCompleted += DeviceWatcher_EnumerationCompleted;
            deviceWatcher.Stopped += DeviceWatcher_Stopped;

            // Start over with an empty collection.
            KnownDevices.Clear();

            // Start the watcher. Active enumeration is limited to approximately 30 seconds.
            // This limits power usage and reduces interference with other Bluetooth activities.
            // To monitor for the presence of Bluetooth LE devices for an extended period,
            // use the BluetoothLEAdvertisementWatcher runtime class. See the BluetoothAdvertisement
            // sample for an example.
            deviceWatcher.Start();
        }

        /// <summary>
        /// Stops watching for all nearby Bluetooth devices.
        /// </summary>
        private void StopBleDeviceWatcher()
        {
            if (deviceWatcher != null)
            {
                // Unregister the event handlers.
                deviceWatcher.Added -= DeviceWatcher_Added;
                deviceWatcher.Updated -= DeviceWatcher_Updated;
                deviceWatcher.Removed -= DeviceWatcher_Removed;
                deviceWatcher.EnumerationCompleted -= DeviceWatcher_EnumerationCompleted;
                deviceWatcher.Stopped -= DeviceWatcher_Stopped;

                // Stop the watcher.
                deviceWatcher.Stop();
                deviceWatcher = null;
            }
        }

        private BluetoothLEDeviceDisplay FindBluetoothLEDeviceDisplay(string id)
        {
            foreach (BluetoothLEDeviceDisplay bleDeviceDisplay in KnownDevices)
            {
                if (bleDeviceDisplay.Id == id)
                {
                    return bleDeviceDisplay;
                }
            }
            return null;
        }

        private DeviceInformation FindUnknownDevices(string id)
        {
            foreach (DeviceInformation bleDeviceInfo in UnknownDevices)
            {
                if (bleDeviceInfo.Id == id)
                {
                    return bleDeviceInfo;
                }
            }
            return null;
        }

        private async void DeviceWatcher_Added(DeviceWatcher sender, DeviceInformation deviceInfo)
        {
            // We must update the collection on the UI thread because the collection is databound to a UI element.
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                lock (this)
                {
                    //Debug.WriteLine(String.Format("Added {0}{1}", deviceInfo.Id, deviceInfo.Name));

                    // Protect against race condition if the task runs after the app stopped the deviceWatcher.
                    if (sender == deviceWatcher)
                    {
                        // Make sure device isn't already present in the list.
                        if (FindBluetoothLEDeviceDisplay(deviceInfo.Id) == null)
                        {
                            if (deviceInfo.Name != string.Empty)
                            {
                                // If device has a friendly name display it immediately.
                                KnownDevices.Add(new BluetoothLEDeviceDisplay(deviceInfo));
                            }
                            else
                            {
                                // Add it to a list in case the name gets updated later. 
                                UnknownDevices.Add(deviceInfo);
                            }
                        }

                    }
                }
            });
        }

        private async void DeviceWatcher_Updated(DeviceWatcher sender, DeviceInformationUpdate deviceInfoUpdate)
        {
            // We must update the collection on the UI thread because the collection is databound to a UI element.
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                lock (this)
                {
                    //Debug.WriteLine(String.Format("Updated {0}{1}", deviceInfoUpdate.Id, ""));

                    // Protect against race condition if the task runs after the app stopped the deviceWatcher.
                    if (sender == deviceWatcher)
                    {
                        BluetoothLEDeviceDisplay bleDeviceDisplay = FindBluetoothLEDeviceDisplay(deviceInfoUpdate.Id);
                        if (bleDeviceDisplay != null)
                        {
                            // Device is already being displayed - update UX.
                            bleDeviceDisplay.Update(deviceInfoUpdate);
                            return;
                        }

                        DeviceInformation deviceInfo = FindUnknownDevices(deviceInfoUpdate.Id);
                        if (deviceInfo != null)
                        {
                            deviceInfo.Update(deviceInfoUpdate);
                            // If device has been updated with a friendly name it's no longer unknown.
                            if (deviceInfo.Name != String.Empty)
                            {
                                KnownDevices.Add(new BluetoothLEDeviceDisplay(deviceInfo));
                                UnknownDevices.Remove(deviceInfo);
                            }
                        }
                    }
                }
            });
        }

        private async void DeviceWatcher_Removed(DeviceWatcher sender, DeviceInformationUpdate deviceInfoUpdate)
        {
            // We must update the collection on the UI thread because the collection is databound to a UI element.
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                lock (this)
                {
                    //Debug.WriteLine(String.Format("Removed {0}{1}", deviceInfoUpdate.Id, ""));

                    // Protect against race condition if the task runs after the app stopped the deviceWatcher.
                    if (sender == deviceWatcher)
                    {
                        // Find the corresponding DeviceInformation in the collection and remove it.
                        BluetoothLEDeviceDisplay bleDeviceDisplay = FindBluetoothLEDeviceDisplay(deviceInfoUpdate.Id);
                        if (bleDeviceDisplay != null)
                        {
                            KnownDevices.Remove(bleDeviceDisplay);
                        }

                        DeviceInformation deviceInfo = FindUnknownDevices(deviceInfoUpdate.Id);
                        if (deviceInfo != null)
                        {
                            UnknownDevices.Remove(deviceInfo);
                        }
                    }
                }
            });
        }

        private async void DeviceWatcher_EnumerationCompleted(DeviceWatcher sender, object e)
        {
            // We must update the collection on the UI thread because the collection is databound to a UI element.
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                // Protect against race condition if the task runs after the app stopped the deviceWatcher.
                if (sender == deviceWatcher)
                {
                    /*rootPage.NotifyUser($"{KnownDevices.Count} devices found. Enumeration completed.",
                        NotifyType.StatusMessage);*/
                    Info.Text = $"{ KnownDevices.Count} devices found. Enumeration completed.";
                }
            });
            StopBleDeviceWatcher();

        }

        private async void DeviceWatcher_Stopped(DeviceWatcher sender, object e)
        {
            // We must update the collection on the UI thread because the collection is databound to a UI element.
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                // Protect against race condition if the task runs after the app stopped the deviceWatcher.
                if (sender == deviceWatcher)
                {
                /*rootPage.NotifyUser($"No longer watching for devices.",
                        sender.Status == DeviceWatcherStatus.Aborted ? NotifyType.ErrorMessage : NotifyType.StatusMessage);*/
                Info.Text = "No Longer watching for devices";

                }
            });
        }
        #endregion
        
        private void Anchor_Add()
        {
            var SelectedBleDevice = (BluetoothLEDeviceDisplay)BLEcmbbox.SelectedItem;
            SelectedBleDeviceId = SelectedBleDevice.Id;
            MagNode = new MagNode(SelectedBleDeviceId, SelectedBleDevice.Name);
            MagNode.Connect();
            MagDeviced.Add(MagNode);
            Debug.WriteLine(SelectedBleDeviceId);
        }

        public float Len_Cal(float[] a, float[] b)
        {
            temp = 0;
            for(var i = 0; i < 3; i++)
            {
                temp += (float)Math.Pow((a[i] - b[i]), 2);
            }
            return (float)Math.Pow(temp, 0.5);
        }

        public async void Get_Cal_Data()
        {
            List<float[]> buffer = new List<float[]>();
            for (var i = 0; i < SensorDeviced.Count; i++)
            {
                SensorDeviced[i].MeasureData = new List<CalUnit>();
            }// init the MeasureData list in class of SensorDeviced
            for (var i = 0; i < MagDeviced.Count; i++)
            {
                for (var j = 0; j < SensorDeviced.Count; j++)
                {
                    SensorDeviced[j].MeasureData.Add(new CalUnit(MagDeviced[i].Position, 0));
                }
            }
            for (var i = 0; i< MagDeviced.Count ; i++)
            {
                MagDeviced[i].IOSignal();
                await Task.Delay(TimeSpan.FromSeconds(0.1));
                for (var j = 0; j < SensorDeviced.Count; j++)
                {
                    SensorDeviced[j].AverageMeasure();
                    buffer.Add(SensorDeviced[j].MagValue);
                }
                MagDeviced[i].IOSignal();
                await Task.Delay(TimeSpan.FromSeconds(0.1));
                for (var j = 0; j < SensorDeviced.Count; j++)
                {
                    SensorDeviced[j].AverageMeasure();
                    float len_result = Len_Cal(SensorDeviced[j].MagValue, buffer[j]);
                    SensorDeviced[j].MeasureData[i].Signal = len_result;
                }
            }
            for (var i = 0; i < SensorDeviced.Count; i++)
            {
                SensorDeviced[i].GetPosition();
            }
        }






    }
}
