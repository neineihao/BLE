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
        private IBuffer buffer;
        private byte[] data;
        private float[] Mag = new float[3];

        // other place for this UUID
        private Guid MagUUID = new Guid("00002AA1-0000-1000-8000-00805f9b34fb");
        private Guid WriteUUID = new Guid("00002A57-0000-1000-8000-00805f9b34fb");
        private Guid HEALTH_THERMOMETER_UUID = new Guid("00001809-0000-1000-8000-00805f9b34fb");
        //

        public MagBLE(string BluetoothLEid)
        {
            this.BluetoothLEid = BluetoothLEid;
        }
        public float[] MagValue
        {
            get
            {
                return this.Mag;
            }
        }
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
                CryptographicBuffer.CopyToByteArray(buffer, out data);
                try
                {   //change the data to three float 
                    Mag[0] = BitConverter.ToSingle(data, 0);
                    Mag[1] = BitConverter.ToSingle(data, 4);
                    Mag[2] = BitConverter.ToSingle(data, 8);
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
        }
    }

}
