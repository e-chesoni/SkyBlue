using SkyBlue.Desktop.WindowsApp.BluetoothLE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Devices.Bluetooth.GenericAttributeProfile;

namespace SkyBlue.Desktop.WindowsApp.Bluetooth
{
    /// <summary>
    /// Wraps and makes use of the <see cref="BluetoothLEAdvertisementWatcher"/>
    /// for easier consumption
    /// </summary>
    public class SkyBlueBluetoothLEAdvertisementWatcher
    {
        #region Private Members

        /// <summary>
        /// The underlying bluetooth watcher class
        /// </summary>
        private readonly BluetoothLEAdvertisementWatcher mWatcher;

        /// <summary>
        /// A list of discovered devices
        /// </summary>
        private readonly Dictionary<string, SkyBlueBluetoothLEDevice> mDiscoveredDevices = new Dictionary<string, SkyBlueBluetoothLEDevice>();

        /// <summary>
        /// Details about GATT Services
        /// </summary>
        private readonly GattServiceIds mGattServiceIds;

        /// <summary>
        /// A thread lock object for this class
        /// </summary>
        private readonly object mThreadLock = new object();

        #endregion

        #region Public Properties

        /// <summary>
        /// Indicates if this watcher is listening for advertisements
        /// </summary>
        public bool Listening => mWatcher.Status == BluetoothLEAdvertisementWatcherStatus.Started;

        /// <summary>
        /// A list of discovered devices
        /// </summary>
        public IReadOnlyCollection<SkyBlueBluetoothLEDevice> DiscoveredDevices
        {
            get
            {
                // Clean up any timeouts
                CleanupTimeouts();

                // Practice thread-safety
                lock (mThreadLock)
                {
                    return mDiscoveredDevices.Values.ToList().AsReadOnly();
                }
            }
        }

        /// <summary>
        /// Time limit (in seconds) for device removal;
        /// if a device does not re-advertise in the alloted time,
        /// it should be removed from the <see cref="DiscoveredDevices"/>
        /// </summary>
        public int HeartbeatTimeout { get; set; } = 30; // Change to 3 to test

        #endregion

        #region Public Events

        /// <summary>
        /// Fired when the bluetooth watcher stops listening
        /// </summary>
        public event Action StoppedListening = () => { };

        /// <summary>
        /// Fired when the bluetooth watcher starts listening
        /// </summary>
        public event Action StartedListening = () => { };

        /// <summary>
        /// Fired when a device is discovered
        /// </summary>
        public event Action<SkyBlueBluetoothLEDevice> DeviceDiscovered = (device) => {};

        /// <summary>
        /// Fired when a new device is discovered
        /// </summary>
        public event Action<SkyBlueBluetoothLEDevice> NewDeviceDiscovered = (device) => { };

        /// <summary>
        /// Fired when a device name changes
        /// </summary>
        public event Action<SkyBlueBluetoothLEDevice> DeviceNameChanged = (device) => { };

        /// <summary>
        /// Fired when a device is removed because of a timeout
        /// </summary>
        public event Action<SkyBlueBluetoothLEDevice> DeviceTimeout = (device) => { };

        #endregion

        #region Constructor

        /// <summary>
        /// The default constructor
        /// </summary>
        public SkyBlueBluetoothLEAdvertisementWatcher(GattServiceIds gattIds)
        {
            // Null guard
            mGattServiceIds = gattIds ?? throw new ArgumentNullException(nameof(gattIds));

            // Create bluetoothLE listener
            mWatcher = new BluetoothLEAdvertisementWatcher
            {
                ScanningMode = BluetoothLEScanningMode.Active
            };

            // Listen for new advertisements
            mWatcher.Received += WatcherAdvertisementReceivedAsync;

            // Listen for stop events when the watcher stops listening
            mWatcher.Stopped += (watcher, e) =>
            {
                // Inform listeners
                StoppedListening();
            };
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Listens for watcher advertisements
        /// </summary>
        /// <param name="sender">The watcher</param>
        /// <param name="args">The arguments</param>
        private async void WatcherAdvertisementReceivedAsync(BluetoothLEAdvertisementWatcher sender, BluetoothLEAdvertisementReceivedEventArgs args)
        {
            // Cleanup Timeouts
            CleanupTimeouts();

            // Get BLE device info
            //SkyBlueBluetoothLEDevice device = null;
            var device = await GetSkyBlueBluetoothLEDeviceAsync(
                args.BluetoothAddress, 
                args.Timestamp, 
                args.RawSignalStrengthInDBm);

            // Null guard
            if (device == null)
                return;

            // REMOVE: DO NOT RUN APP W/O BREAK POINTS WITH THIS IN--IT WILL CAUSE UNHANDLED EXCEPTIONS
            //return;

            // Is new discovery?
            var newDiscovery = false;

            var existingName = default(string);

            // Lock it up
            lock (mThreadLock)
            {
                // Check if this is a new discovery
                newDiscovery = !mDiscoveredDevices.ContainsKey(device.DeviceId);

                // If this is not new
                if (!newDiscovery)
                    // Store the old name
                    existingName = mDiscoveredDevices[device.DeviceId].Name;
            }

            // Name changed?
            var nameChanged =
                // If it already exists
                !newDiscovery &&
                // And is not a blank name
                !string.IsNullOrEmpty(device.Name) &&
                // And the name is different
                existingName != device.Name;

            lock (mThreadLock)
            {
                // Add/update the device in the dictionary
                mDiscoveredDevices[device.DeviceId] = device;
            }

            // Inform listeners
            DeviceDiscovered(device);

            // If name changed...
            if (nameChanged)
                // Inform listeners
                DeviceNameChanged(device);

            // If new discovery...
            if (newDiscovery)
                // Inform listeners
                NewDeviceDiscovered(device);
        }

        /// <summary>
        /// Connects to the BLE device and extracts more information from the 
        /// <see cref="https://docs.microsoft.com/en-us/uwp/api/windows.devices.bluetooth.bluetoothledevice?view=winrt-19041"/>
        /// <param name="address">The bluetooth address of the device to connect to</param>
        /// <param name="broadcastTime">The time the broadcast message was received</param>
        /// <param name="rssi">The signal strength in db </param>
        /// <returns></returns>
        private async Task<SkyBlueBluetoothLEDevice> GetSkyBlueBluetoothLEDeviceAsync(ulong address, DateTimeOffset broadcastTime, short rssi)
        {
            // Get bluetooth device info
            var device = await BluetoothLEDevice.FromBluetoothAddressAsync(address).AsTask();

            // Null guard
            if (device == null)
                return null;

            // Get GATT Services that are available
            var gatt = await device.GetGattServicesAsync().AsTask();

            // If we have any services
            if (gatt.Status == GattCommunicationStatus.Success)
            {
                // Loop each GATT Service
                foreach (var service in gatt.Services)
                {
                    // This ID contains the GATT profile assigned number we want!
                    // TODO: Get more info and connect
                    var gattProfile = service.Uuid;
                }
            }

            // Return the new device information
            return new SkyBlueBluetoothLEDevice
            (
                // Device ID
                deviceId: device.DeviceId,
                // Bluetooth Address
                address: device.BluetoothAddress,
                // Device Name
                name: device.Name,
                // Broadcast Time
                broadcastTime: broadcastTime,
                // Signal Strength
                rssi: rssi,
                // Is Connected?
                connected: device.ConnectionStatus == BluetoothConnectionStatus.Connected,
                // Can Pair?
                canPair: device.DeviceInformation.Pairing.CanPair,
                // Is Paired?
                paired: device.DeviceInformation.Pairing.IsPaired
            );
        }

        /// <summary>
        /// Prune any timed out devices that we have not heard from
        /// </summary>
        private void CleanupTimeouts()
        {
            lock (mThreadLock)
            {
                // The date and time used to assess timeout
                var threshold = DateTime.UtcNow - TimeSpan.FromSeconds(HeartbeatTimeout);

                // Any devices that have not sent a new broadcast within the heartbeat time
                mDiscoveredDevices.Where(f => f.Value.BroadcastTime < threshold).ToList().ForEach(device =>
                {
                    // Remove device
                    mDiscoveredDevices.Remove(device.Key);

                    // Inform listeners
                    DeviceTimeout(device.Value);
                });
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Starts listening for advertisements
        /// </summary>
        public void StartListening()
        {
            lock (mThreadLock)
            {
                // If we are currently listening...
                if (Listening)
                    // Do nothing more
                    return;

                // Start the underlying watcher
                mWatcher.Start();
            }

            // Inform listeners
            StartedListening();
        }

        /// <summary>
        /// Stops listening for advertisements
        /// </summary>
        public void StopListening()
        {
            lock (mThreadLock)
            {
                // If we are not currently listening...
                if (!Listening)
                    // Do nothing more
                    return;

                // Stop listening
                mWatcher.Stop();

                // Clear any devices
                mDiscoveredDevices.Clear();
            }
        }

        #endregion
    }
}
