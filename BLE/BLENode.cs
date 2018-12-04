using System;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.ComponentModel;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using System.Collections.Generic;
using Windows.Security.Cryptography;
using Windows.Devices.Bluetooth.GenericAttributeProfile;


namespace BLE
{
    class BLENode : INotifyPropertyChanged
    {
        private string BluetoothLEid;
        private BluetoothLEDevice bluetoothLeDevice = null;
        private GattCharacteristic magCharacteristic;
        protected GattCharacteristic writeCharacteristic;
        private GattDeviceService selectedService;
        private GattCharacteristicsResult tempCharacteristic;
        //private IBuffer buffer ;
        private byte[] data;
        private int times = 5;
        public event PropertyChangedEventHandler PropertyChanged;

        // other place for this UUID
        protected Guid MagUUID = new Guid("00002AA1-0000-1000-8000-00805f9b34fb");
        protected Guid WriteUUID = new Guid("00002A57-0000-1000-8000-00805f9b34fb");
        protected Guid HEALTH_THERMOMETER_UUID = new Guid("00001809-0000-1000-8000-00805f9b34fb");
        //
        public float[] MagValue { get; } = new float[3];
        public float[] Position { get; set; } = new float[3];
        private float[] MagTemp = new float[3];
        public float MagX { get { return MagValue[0]; } }
        public float MagY { get { return MagValue[1]; } }
        public float MagZ { get { return MagValue[2]; } }
        public int AvgTimes { get { return times; } set { times = (int)value; } }
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

        public BLENode(string BluetoothLEid, string Name)
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

        public async void GetMagData()
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
        }

        public void Measure()
        {
            GetMagData();
            OnPropertyChanged("MagString");
        }// end Measure method

        public async void AverageMeasure()
        {
            Debug.WriteLine("The times of avg: " + times);
            for (int i = 0; i < times; i++)
            {
                await Task.Delay(TimeSpan.FromSeconds(0.2));
                GetMagData();
                Debug.WriteLine(" X: " + MagX + " Y: " + MagY + " Z: " + MagY);
                MagTemp[0] += MagValue[0];
                MagTemp[1] += MagValue[1];
                MagTemp[2] += MagValue[2];
            }
            MagValue[0] = MagTemp[0] / times;
            MagValue[1] = MagTemp[1] / times;
            MagValue[2] = MagTemp[2] / times;
            OnPropertyChanged("MagString");
            MagTemp[0] = 0;
            MagTemp[1] = 0;
            MagTemp[2] = 0;
        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}

