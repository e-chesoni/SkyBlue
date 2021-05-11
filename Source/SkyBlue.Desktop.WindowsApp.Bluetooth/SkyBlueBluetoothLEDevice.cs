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

        #endregion

        #region Constructor
        
        /// <summary>
        /// Default constructor
        /// </summary>
        public SkyBlueBluetoothLEDevice(ulong address, string name, short rssi, DateTimeOffset broadcastTime)
        {
            Address = address;
            Name = name;
            SignalStrengthInDB = rssi;
            BroadcastTime = broadcastTime;
        }

        #endregion

        /// <summary>
        /// User-friendly to string
        /// </summary>
        /// <returns>string</returns>
        public override string ToString()
        {
            return $"{ (string.IsNullOrEmpty(Name) ? "[No Name]" : Name) } {Address} ({SignalStrengthInDB})";
        }
    }
}
