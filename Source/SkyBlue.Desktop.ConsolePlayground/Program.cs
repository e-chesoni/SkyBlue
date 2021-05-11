using SkyBlue.Desktop.WindowsApp.Bluetooth;
using System;

namespace SkyBlue.Desktop.ConsolePlayground
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            // New watcher
            var watcher = new SkyBlueBluetoothLEAdvertisementWatcher();

            // Hook into events
            watcher.StartedListening += () =>
            {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine("Started Listening");
            };

            watcher.StoppedListening += () =>
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine("Stopped Listening");
            };

            watcher.NewDeviceDiscovered += (device) =>
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"New device: {device}");
            };

            watcher.DeviceNameChanged += (device) =>
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Device name changed: {device}");
            };

            watcher.DeviceTimeout += (device) =>
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Device timeout: {device}");
            };

            // Start listening
            watcher.StartListening();

            while (true)
            {
                // Pause until enter is pressed
                Console.ReadLine();

                var devices = watcher.DiscoveredDevices;

                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"{devices.Count} devices.......");

                foreach (var device in devices)
                    Console.WriteLine(device);
            }
        }
    }
}
