using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Security.Cryptography;
using Windows.Devices.Enumeration;
using Windows.Storage.Streams;


namespace BLE
{
    class MagBLE
    {
        private string BluetoothLEid;
        private BluetoothLEDevice bluetoothLeDevice = null;
        private GattCharacteristic magCharacteristic;
        private GattCharacteristic writeCharacteristic;
        private GattDeviceService selectedService;
        private GattCharacteristicsResult tempCharacteristic;
        private IBuffer buffer ;
        private byte[] data;
        private int viberate = 0;

        // other place for this UUID
        private Guid MagUUID = new Guid("00002AA1-0000-1000-8000-00805f9b34fb");
        private Guid WriteUUID = new Guid("00002A57-0000-1000-8000-00805f9b34fb");
        private Guid HEALTH_THERMOMETER_UUID = new Guid("00001809-0000-1000-8000-00805f9b34fb");
        //
        public float[] MagValue { get; } = new float[3];
        public float MagX { get { return MagValue[0]; } }
        public float MagY { get { return MagValue[1]; } }
        public float MagZ { get { return MagValue[2]; } }
        public string Name { get; set; }
        public string ID
        {
            get
            {
                return this.BluetoothLEid;
            }
            set
            {
                this.BluetoothLEid = value;
            }
        }
        public string MagString
        {
            get
            {
                return String.Format("MagX: {0}, MagY: {1}, MagZ: {2}", MagValue[0], MagValue[1], MagValue[2]);
            }
        }

        public MagBLE(string BluetoothLEid, string Name)
        {
            this.BluetoothLEid = BluetoothLEid;
            this.Name = Name;
        }
        

        public async void Connect()
        {
            bluetoothLeDevice = await BluetoothLEDevice.FromIdAsync(this.BluetoothLEid);
            if (bluetoothLeDevice != null)
            {
                GattDeviceServicesResult result = await bluetoothLeDevice.GetGattServicesAsync(BluetoothCacheMode.Uncached);
                if (result.Status == GattCommunicationStatus.Success)
                {
                    var services = result.Services;
                    foreach (var service in services)
                    {
                        if (service.Uuid == HEALTH_THERMOMETER_UUID)
                        {
                            selectedService = service;
                            tempCharacteristic = await selectedService.GetCharacteristicsForUuidAsync(MagUUID);
                            magCharacteristic = tempCharacteristic.Characteristics[0];
                            tempCharacteristic = await selectedService.GetCharacteristicsForUuidAsync(WriteUUID);
                            writeCharacteristic = tempCharacteristic.Characteristics[0];
                        }
                    }
                }
                else
                {
                    Debug.WriteLine("Connect Fail");
                }
            }
        }//end method connect

        public async void Measure()
        {
            GattReadResult result = await magCharacteristic.ReadValueAsync(BluetoothCacheMode.Uncached);
            if (result.Status == GattCommunicationStatus.Success)
            {
                CryptographicBuffer.CopyToByteArray(result.Value, out data);
                try
                {   //change the data to three float 
                    MagValue[0] = BitConverter.ToSingle(data, 0);
                    MagValue[1] = BitConverter.ToSingle(data, 4);
                    MagValue[2] = BitConverter.ToSingle(data, 8);
                }
                catch (ArgumentException)
                {
                    Debug.WriteLine("Bit Changing Error");
                }
            }
            else
            {
                Debug.WriteLine("Read Fail");
            }
        }// end Measure method

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
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return false;
            }
        }
        
        public async Task<bool> WriteValueAsync(string writevalue)
        {
            var writeBuffer = CryptographicBuffer.ConvertStringToBinary(writevalue,
                    BinaryStringEncoding.Utf8);
            var writeSuccessful = await WriteBufferToSelectedCharacteristicAsync(writeBuffer);
            return writeSuccessful;
        }

        public void Viberate()
        {
            if (viberate == 0)
            {
                viberate = 1;
            }
            else if (viberate == 1)
            {
                viberate = 0;
            }
            else
            {
                viberate = 0;
            }
            var result = WriteValueAsync(viberate.ToString());
            Debug.WriteLine("Write T/F :" + result);
        }

    }
}
