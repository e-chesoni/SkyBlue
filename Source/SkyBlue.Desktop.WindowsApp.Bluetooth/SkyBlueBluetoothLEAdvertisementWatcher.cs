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
        private readonly Dictionary<ulong, SkyBlueBluetoothLEDevice> mDiscoveredDevices = new Dictionary<ulong, SkyBlueBluetoothLEDevice>();

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
            var device = await GetSkyBlueBluetoothLEDeviceAsync(args.BluetoothAddress);

            // Null guard
            if (device == null)
                return;

            // REMOVE
            return;

            // Is new discovery?
            var newDiscovery = !mDiscoveredDevices.ContainsKey(args.BluetoothAddress);

            // Name changed?
            var nameChanged =
                // If it already exists
                !newDiscovery &&
                // And is not a blank name
                !string.IsNullOrEmpty(args.Advertisement.LocalName) &&
                // And the name is different
                mDiscoveredDevices[args.BluetoothAddress].Name != args.Advertisement.LocalName;

            lock (mThreadLock)
            {
                // Get the name of the device
                var name = args.Advertisement.LocalName;

                // If new name is blank, and we already have a device...
                if (string.IsNullOrEmpty(name) && !newDiscovery)
                    // Don't override what could be an actual name already
                    name = mDiscoveredDevices[args.BluetoothAddress].Name;

                // Create new device info instance
                device = new SkyBlueBluetoothLEDevice
                (
                    // Bluetooth address
                    address: args.BluetoothAddress,
                    
                    // Name
                    name: name,
                    
                    // Broadcast time
                    broadcastTime: args.Timestamp,
                    
                    // Signal strength
                    rssi: args.RawSignalStrengthInDBm
                );

                // Add/update the device in the dictionary
                mDiscoveredDevices[args.BluetoothAddress] = device;
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
        /// </summary>
        /// <returns></returns>
        private async Task<SkyBlueBluetoothLEDevice> GetSkyBlueBluetoothLEDeviceAsync(ulong address)
        {
            // Get bluetooth device info
            var device = await BluetoothLEDevice.FromBluetoothAddressAsync(address).AsTask();

            // Null guard
            if (device == null)
                return null;

            // Device name
            var name = device.Name;

            // Get GATT Services that are available
            var gatt = await device.GetGattServicesAsync().AsTask();

            // If we have any services
            if (gatt.Status == GattCommunicationStatus.Success)
            {
                // Loop each GATT Service
                foreach (var service in gatt.Services)
                {
                    //var gattProfile = service.Uuid;
                }
            }

            return null;
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
