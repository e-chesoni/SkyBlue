using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyBlue.Desktop.WindowsApp.BluetoothLE
{
    /// <summary>
    /// Information about a BLE device
    /// </summary>
    public class SkyBlueBluetoothLEDevice
    {
        #region Public Properties
        
        /// <summary>
        /// The time of the broadcast advertisement message of the device
        /// </summary>
        public DateTimeOffset BroadcastTime { get; }

        /// <summary>
        /// The address of the device
        /// </summary>
        public ulong Address { get; }

        /// <summary>
        /// The name of the device
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The signal strength in dB
        /// </summary>
        public short SignalStrengthInDB { get; }

        /// <summary>
        /// Indicates if we are connected to this device
        /// </summary>
        public bool Connected { get; }

        /// <summary>
        /// Indicates if this device supports pairing
        /// </summary>
        public bool CanPair { get;  }

        /// <summary>
        /// Indicates if currently paired to this device
        /// </summary>
        public bool Paired { get;  }

        /// <summary>
        /// The permanent unique id of this device
        /// </summary>
        public string DeviceId { get;  }

        #endregion

        #region Constructor
        
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="address">The Bluetooth device address</param>
        /// <param name="name">The device name</param>
        /// <param name="rssi">The signal strenght</param>
        /// <param name="broadcastTime">The broadcast time of discovery</param>
        /// <param name="connected">If connected to the device</param>
        /// <param name="canPair">If we can pair to the device</param>
        /// <param name="isPaired">If we are paired to the device</param>
        /// <param name="deviceId">The unique id of the device</param>
        public SkyBlueBluetoothLEDevice(
            ulong address, 
            string name, 
            short rssi, 
            DateTimeOffset broadcastTime,
            bool connected,
            bool canPair,
            bool paired,
            string deviceId)
        {
            Address = address;
            Name = name;
            SignalStrengthInDB = rssi;
            BroadcastTime = broadcastTime;
            Connected = connected;
            CanPair = canPair;
            Paired = paired;
            DeviceId = deviceId;
        }

        #endregion

        /// <summary>
        /// User-friendly to string
        /// </summary>
        /// <returns>string</returns>
        public override string ToString()
        {
            return $"{ (string.IsNullOrEmpty(Name) ? "[No Name]" : Name) } {DeviceId} ({SignalStrengthInDB})";
        }
    }
}
