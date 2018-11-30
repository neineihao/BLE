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
        private DeviceWatcher deviceWatcher;
        private BluetoothLEDevice bluetoothLeDevice = null;
        private BluetoothLEDevice bluetoothLeDevice2 = null;
        private GattCharacteristic magCharacteristic;
        private GattCharacteristic magCharacteristic2;
        private GattCharacteristic writeCharacteristic;
        private GattDeviceService selectedService;
        private GattDeviceService selectedService2;
        private List<float> dataList = new List<float>(); 
        private float MagX;
        private float MagY;
        private int viberate = 0;
        private float MagZ;
        //private BluetoothLEDeviceDisplay SelectedBleDevice;
        public string SelectedBleDeviceId;
        public string SelectedBleDeviceId2;

        // Only one registered characteristic at a time.
        private GattCharacteristic registeredCharacteristic;
        private GattPresentationFormat presentationFormat;

        #region Error Codes
        readonly int E_BLUETOOTH_ATT_WRITE_NOT_PERMITTED = unchecked((int)0x80650003);
        readonly int E_BLUETOOTH_ATT_INVALID_PDU = unchecked((int)0x80650004);
        readonly int E_ACCESSDENIED = unchecked((int)0x80070005);
        readonly int E_DEVICE_NOT_AVAILABLE = unchecked((int)0x800710df); // HRESULT_FROM_WIN32(ERROR_DEVICE_NOT_AVAILABLE)
        #endregion

        #region Uuid
        Guid MagUUID = new Guid("00002AA1-0000-1000-8000-00805f9b34fb");
        Guid WriteUUID = new Guid("00002A57-0000-1000-8000-00805f9b34fb");
        Guid HEALTH_THERMOMETER_UUID = new Guid("00001809-0000-1000-8000-00805f9b34fb");
        #endregion



        public MainPage()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedFrom(NavigationEventArgs e)
        {
            {
                
                /*var success = await ClearBluetoothLEDeviceAsync();
                if (!success)
                {
                    Debug.WriteLine("Error: Unable to reset app state");
                }
                */
                StopBleDeviceWatcher();
                
                // Save the selected device's ID for use in other scenarios.
                /*
                var bleDeviceDisplay = ResultsListView.SelectedItem as BluetoothLEDeviceDisplay;
                if (bleDeviceDisplay != null)
                {
                    Debug.WriteLine("Id: " + bleDeviceDisplay.Id);
                    Debug.WriteLine("Name: " + bleDeviceDisplay.Name);
                }
                /*/
            }
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
                          
        private bool subscribedForNotifications = false;
        /*
        private async Task<bool> ClearBluetoothLEDeviceAsync()
        {
            if (subscribedForNotifications)
            {
                // Need to clear the CCCD from the remote device so we stop receiving notifications
                var result = await registeredCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.None);
                if (result != GattCommunicationStatus.Success)
                {
                    return false;
                }
                else
                {
                    magCharacteristic.ValueChanged -= Characteristic_ValueChanged;
                    subscribedForNotifications = false;
                }
            }
            bluetoothLeDevice?.Dispose();
            bluetoothLeDevice = null;
            return true;
        }
        */

             
        #region Connect Button
        private async void Connect_Button()
        {
            var SelectedBleDevice = (BluetoothLEDeviceDisplay)BLEcmbbox.SelectedItem;
            SelectedBleDeviceId = SelectedBleDevice.Id;
            Debug.WriteLine(SelectedBleDeviceId);
            Debug.WriteLine("Stop Here");
            try
            {
                bluetoothLeDevice = await BluetoothLEDevice.FromIdAsync(SelectedBleDeviceId);
                if (SelectedBleDevice.IsConnectable)
                {
                    Info.Text = "Connectable";
                }
                else
                {
                    Info.Text = "Unconnectable, Please try again";
                }
                if (bluetoothLeDevice == null)
                {
                    Info.Text = "Failed to connect to device with BLE.";
                }
            }
            catch (Exception ex) when (ex.HResult == E_DEVICE_NOT_AVAILABLE)
            {
                Info.Text = "Bluetooth radio is not on.";
            }

            if (bluetoothLeDevice != null)
            {
                // Note: BluetoothLEDevice.GattServices property will return an empty list for unpaired devices. For all uses we recommend using the GetGattServicesAsync method.
                // BT_Code: GetGattServicesAsync returns a list of all the supported services of the device (even if it's not paired to the system).
                // If the services supported by the device are expected to change during BT usage, subscribe to the GattServicesChanged event.
                GattDeviceServicesResult result = await bluetoothLeDevice.GetGattServicesAsync(BluetoothCacheMode.Uncached);
                //Debug.WriteLine("Sucess to get bluetooth object");
                if (result.Status == GattCommunicationStatus.Success)
                {
                    var services = result.Services;
                    foreach (var service in services)
                    {
                        if (service.Uuid == HEALTH_THERMOMETER_UUID)
                        {
                            selectedService = service;
                            GattCharacteristicsResult resultCharacteristic = await selectedService.GetCharacteristicsForUuidAsync(MagUUID);
                            magCharacteristic = resultCharacteristic.Characteristics[0];
                            Debug.WriteLine("Get magCharacteristic");
                            resultCharacteristic = await selectedService.GetCharacteristicsForUuidAsync(WriteUUID);
                            writeCharacteristic = resultCharacteristic.Characteristics[0];
                            Debug.WriteLine("Write magCharacteristic");
                            presentationFormat = null;
                        }
                    }
                }
                else
                {
                    Debug.WriteLine("Device unreachable");
                }
            }
        }

        private async void Connect_Button2()
        {
            var SelectedBleDevice2 = (BluetoothLEDeviceDisplay)BLEcmbbox2.SelectedItem;
            SelectedBleDeviceId2 = SelectedBleDevice2.Id;
            try
            {
                 bluetoothLeDevice2 = await BluetoothLEDevice.FromIdAsync(SelectedBleDeviceId2);
                if (SelectedBleDevice2.IsConnectable)
                {
                    Info.Text = "Connectable";
                }
                else
                {
                    Info.Text = "Unconnectable, Please try again";
                }
                if (bluetoothLeDevice2 == null)
                {
                    Info.Text = "Failed to connect to device with BLE.";
                }
            }
            catch (Exception ex) when (ex.HResult == E_DEVICE_NOT_AVAILABLE)
            {
                Info.Text = "Bluetooth radio is not on.";
            }

            if (bluetoothLeDevice2 != null)
            {
                // Note: BluetoothLEDevice.GattServices property will return an empty list for unpaired devices. For all uses we recommend using the GetGattServicesAsync method.
                // BT_Code: GetGattServicesAsync returns a list of all the supported services of the device (even if it's not paired to the system).
                // If the services supported by the device are expected to change during BT usage, subscribe to the GattServicesChanged event.
                GattDeviceServicesResult result2 = await bluetoothLeDevice2.GetGattServicesAsync(BluetoothCacheMode.Uncached);
                //Debug.WriteLine("Sucess to get bluetooth object");
                if (result2.Status == GattCommunicationStatus.Success)
                {
                    var services2 = result2.Services;             
                    foreach (var service in services2)
                    {
                        if (service.Uuid == HEALTH_THERMOMETER_UUID)
                        {
                            selectedService2 = service;
                            GattCharacteristicsResult resultCharacteristic2 = await selectedService2.GetCharacteristicsForUuidAsync(MagUUID);
                            magCharacteristic2 = resultCharacteristic2.Characteristics[0];
                            CharacteristicLatestValue2.Text = "Get Characteristic";
                            presentationFormat = null;
                        }
                    }
                }
                else
                {
                    CharacteristicLatestValue2.Text = "Device unreachable";
                }
            }
        }
        #endregion
        /*
        #region Handler
        private void AddValueChangedHandler()
        {
            Subscribe.Content = "Unsubscribe";
            if (!subscribedForNotifications)
            {
                registeredCharacteristic = magCharacteristic;
                registeredCharacteristic.ValueChanged += Characteristic_ValueChanged;
                subscribedForNotifications = true;
            }
        }

        private void RemoveValueChangedHandler()
        {
            Subscribe.Content = "Subscribe";
            if (subscribedForNotifications)
            {
                registeredCharacteristic.ValueChanged -= Characteristic_ValueChanged;
                registeredCharacteristic = null;
                subscribedForNotifications = false;
            }
        }
        #endregion
            
        #region Subscribe Button Click
        private async void ValueChangedSubscribeToggle_Click()
        {
            if (!subscribedForNotifications)
            {
                // initialize status
                GattCommunicationStatus status = GattCommunicationStatus.Unreachable;
                var cccdValue = GattClientCharacteristicConfigurationDescriptorValue.None;
                if (magCharacteristic.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Indicate))
                {
                    cccdValue = GattClientCharacteristicConfigurationDescriptorValue.Indicate;
                }

                else if (magCharacteristic.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Notify))
                {
                    cccdValue = GattClientCharacteristicConfigurationDescriptorValue.Notify;
                }

                try
                {
                    // BT_Code: Must write the CCCD in order for server to send indications.
                    // We receive them in the ValueChanged event handler.
                    status = await magCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(cccdValue);

                    if (status == GattCommunicationStatus.Success)
                    {
                        AddValueChangedHandler();
                        Info.Text = "Successfully subscribed for value changes";
                    }
                    else
                    {
                        Info.Text = $"Error registering for value changes: {status}";
                    }
                }
                catch (UnauthorizedAccessException ex)
                {
                    // This usually happens when a device reports that it support indicate, but it actually doesn't.
                    Info.Text = ex.Message;
                }
            }
            else
            {
                try
                {
                    // BT_Code: Must write the CCCD in order for server to send notifications.
                    // We receive them in the ValueChanged event handler.
                    // Note that this sample configures either Indicate or Notify, but not both.
                    var result = await
                            magCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(
                                GattClientCharacteristicConfigurationDescriptorValue.None);
                    if (result == GattCommunicationStatus.Success)
                    {
                        subscribedForNotifications = false;
                        RemoveValueChangedHandler();
                        Info.Text = "Successfully un-registered for notifications";
                    }
                    else
                    {
                        Info.Text = $"Error un-registering for notifications: {result}";
                    }
                }
                catch (UnauthorizedAccessException ex)
                {
                    // This usually happens when a device reports that it support notify, but it actually doesn't.
                    Info.Text = ex.Message;
                }
            }
        }
    
        private async void Characteristic_ValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            // BT_Code: An Indicate or Notify reported that the value has changed.
            // Display the new value with a timestamp.
            var newValue = FormatValueByPresentation(args.CharacteristicValue, presentationFormat);
            var message = $"Value at {DateTime.Now:hh:mm:ss.FFF}: {newValue}";
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () => CharacteristicLatestValue.Text = message);
        }
        */
        private string FormatValueByPresentation(IBuffer buffer, GattPresentationFormat format)
        {
            // BT_Code: For the purpose of this sample, this function converts only UInt32 and
            // UTF-8 buffers to readable text. It can be extended to support other formats if your app needs them.
            byte[] data;
            CryptographicBuffer.CopyToByteArray(buffer, out data);
            if (format != null)
            {
                if (format.FormatType == GattPresentationFormatTypes.UInt32 && data.Length >= 4)
                {
                    return BitConverter.ToInt32(data, 0).ToString();
                }
                else if (format.FormatType == GattPresentationFormatTypes.Utf8)
                {
                    try
                    {
                        return Encoding.UTF8.GetString(data);
                    }
                    catch (ArgumentException)
                    {
                        return "(error: Invalid UTF-8 string)";
                    }
                }
                else
                {
                    // Add support for other format types as needed.
                    return "Unsupported format: " + CryptographicBuffer.EncodeToHexString(buffer);
                }
            }
            else if (data != null)
            {    
                    try
                    {   //change the data to three float 
                        MagX = BitConverter.ToSingle(data, 0);
                        MagY = BitConverter.ToSingle(data, 4);
                        MagZ = BitConverter.ToSingle(data, 8);
                        return "MagX: " + MagX.ToString() + ", MagY: " + MagY.ToString() +
                            ", MagZ: " + MagZ.ToString();
                    }
                    catch (ArgumentException)
                    {
                        return "Bit Changing Error";
                    }
            }
            else
            {
                return "Empty data received";
            }
        }
        //#endregion
        #region Viberate Button Click
 /*       private async void ViberateToggle_Click()
        {
            
            if (viberate == 0)
            {
                viberate = 1;
                Viberate.Content = "Stop";
            }
            else if(viberate == 1)
            {
                viberate = 0;
                Viberate.Content = "Viberate";
            }
            else
            {
                viberate = 0;
                Info.Text = "Can't get other";
            }
            Info.Text = viberate.ToString();
            var writeBuffer = CryptographicBuffer.ConvertStringToBinary(viberate.ToString(),
                    BinaryStringEncoding.Utf8);

            var writeSuccessful = await WriteBufferToSelectedCharacteristicAsync(writeBuffer);
        }
        */
        private async Task<bool> WriteBufferToSelectedCharacteristicAsync(IBuffer buffer)
        {
            try
            {
                // BT_Code: Writes the value from the buffer to the characteristic.
                var result = await writeCharacteristic.WriteValueWithResultAsync(buffer);

                if (result.Status == GattCommunicationStatus.Success)
                {
                    Debug.WriteLine("Successfully wrote value to device");
                    return true;
                }
                else
                {
                    Debug.WriteLine($"Write failed: {result.Status}");
                    return false;
                }
            }
            catch (Exception ex) when (ex.HResult == E_BLUETOOTH_ATT_INVALID_PDU)
            {
                Info.Text = ex.Message;
                return false;
            }
            catch (Exception ex) when (ex.HResult == E_BLUETOOTH_ATT_WRITE_NOT_PERMITTED || ex.HResult == E_ACCESSDENIED)
            {
                // This usually happens when a device reports that it support writing, but it actually doesn't.
                Info.Text = ex.Message;
                return false;
            }
        }
        #endregion
        #region Read Value
        /*
        private async void Read_Click()
        {
            string read_context;
            GattReadResult result = await writeCharacteristic.ReadValueAsync(BluetoothCacheMode.Uncached);
            read_context = readValue(result.Value);
            if (result.Status == GattCommunicationStatus.Success)
            {
                Read_value.Text = $"Read result: {read_context}";
            }
            else
            {
                Read_value.Text = $"Read failed: {result.Status}";
            }
        }
        */
        private async void Read_All_Data()
        {
            string read_context;
            string read_context2;

            GattReadResult result = await magCharacteristic.ReadValueAsync(BluetoothCacheMode.Uncached);
            GattReadResult result2 = await magCharacteristic2.ReadValueAsync(BluetoothCacheMode.Uncached);

            read_context = ReadMagValue(result.Value);
            read_context2 = ReadMagValue(result2.Value);

            if (result.Status == GattCommunicationStatus.Success)
            {
                CharacteristicLatestValue.Text = $"Read result: {read_context}";
                CharacteristicLatestValue2.Text = $"Read result: {read_context2}";
                Info.Text = "Read Sucess";
            }
            else
            {
                CharacteristicLatestValue.Text = $"Read failed: {result.Status}";
                CharacteristicLatestValue2.Text = $"Read failed: {result.Status}";
                Info.Text = "Read Fail";
            }
        }

        private string ReadValue(IBuffer buffer)
        {
            byte[] data;
            CryptographicBuffer.CopyToByteArray(buffer, out data);
            return data[0].ToString();
        }

        private string ReadMagValue(IBuffer buffer)
        {
            byte[] data;
            CryptographicBuffer.CopyToByteArray(buffer, out data);
            try
            {   //change the data to three float 
                MagX = BitConverter.ToSingle(data, 0);
                MagY = BitConverter.ToSingle(data, 4);
                MagZ = BitConverter.ToSingle(data, 8);
                return "MagX: " + MagX.ToString() + ", MagY: " + MagY.ToString() +
                    ", MagZ: " + MagZ.ToString();
            }
            catch (ArgumentException)
            {
                return "Bit Changing Error";
            }
        }


        #endregion
    }
}
