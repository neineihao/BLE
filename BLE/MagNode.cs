using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Security.Cryptography;
using Windows.Devices.Enumeration;
using Windows.Storage.Streams;


namespace BLE
{
    class MagNode : BLENode
    {
        private int viberate = 0;

        public MagNode(string BluetoothLEid, string Name) : base(BluetoothLEid, Name){ }

        private async Task<bool> WriteBufferToSelectedCharacteristicAsync(IBuffer buffer)
        {
            try
            {
                // BT_Code: Writes the value from the buffer to the characteristic.
                var result = await this.writeCharacteristic.WriteValueWithResultAsync(buffer);

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

        public void IOSignal()
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
