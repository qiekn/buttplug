﻿using Buttplug.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApplicationExample {
  internal class Program {
    private static async Task WaitForKey() {
      Console.WriteLine("Press any key to continue.");
      while (!Console.KeyAvailable) {
        await Task.Delay(1);
      }
      Console.ReadKey(true);
    }

    private static async Task RunExample() {
      // Now that we've seen all of the different parts of Buttplug, let's
      // put them together in a small program.
      //
      // This program will:
      // - Create an websocket connector
      // - Scan, this time using real Managers, so we'll see devices
      //   (assuming you have them hooked up)
      // - List the connected devices for the user
      // - Let the user select a device, and trigger some sort of event on
      //   that device (vibration, thrusting, etc...).

      // As usual, we start off with our connector setup. We really don't
      // need access to the connector this time, so we can just pass the
      // created connector directly to the client.
      var client = new ButtplugClient("Example Client");

      // Whenever a client connects, it asks the server for a list of devices
      // that may already be connected. Therefore we'll want to set up our
      // device handlers before we connect, so we can see what devices may
      // already be connected to the server.
      void HandleDeviceAdded(object aObj, DeviceAddedEventArgs aArgs) {
        Console.WriteLine($"Device connected: {aArgs.Device.Name}");
      }

      client.DeviceAdded += HandleDeviceAdded;

      void HandleDeviceRemoved(object aObj, DeviceRemovedEventArgs aArgs) {
        Console.WriteLine($"Device connected: {aArgs.Device.Name}");
      }

      client.DeviceRemoved += HandleDeviceRemoved;

      // Now we can connect.
      await client.ConnectAsync(new ButtplugWebsocketConnector(new Uri("ws://127.0.0.1:12345")));


      // The structure here is gonna get a little weird now, because I'm
      // using method scoped functions. We'll be defining our scanning
      // function first, then running it just to find any devices up front.
      // Then we'll define our command sender. Finally, with all of that
      // done, we'll end up in our main menu

      // Here's the scanning part. Pretty simple, just scan until the user
      // hits a button. Any time a new device is found, print it so the
      // user knows we found it.
      async Task ScanForDevices() {
        Console.WriteLine("Scanning for devices until key is pressed.");
        Console.WriteLine("Found devices will be printed to console.");
        await client.StartScanningAsync();
        await WaitForKey();

        // Stop scanning now, 'cause we don't want new devices popping up anymore.
        await client.StopScanningAsync();
      }

      // Scan for devices before we get to the main menu.
      await ScanForDevices();

      // Now we define the device control menus. After we've scanned for
      // devices, the user can use this menu to select a device, then
      // select an action for that device to take.
      async Task ControlDevice() {
        // Controlling a device has 2 steps: selecting the device to
        // control, and choosing which command to send. We'll just list
        // the devices the client has available, then search the device
        // message capabilities once that's done to figure out what we
        // can send. Note that this is using the Device Index, which is
        // assigned by the device manager and may not be sequential
        // (which is why we can't just use an array index).

        // Of course, if we don't have any devices yet, that's not gonna work.
        if (!client.Devices.Any()) {
          Console.WriteLine("No devices available. Please scan for a device.");
          return;
        }

        var options = new List<uint>();

        foreach (var dev in client.Devices) {
          Console.WriteLine($"{dev.Index}. {dev.Name}");
          options.Add(dev.Index);
        }
        Console.WriteLine("Choose a device: ");
        if (!uint.TryParse(Console.ReadLine(), out var deviceChoice) ||
            !options.Contains(deviceChoice)) {
          Console.WriteLine("Invalid choice");
          return;
        }

        var device = client.Devices.First(dev => dev.Index == deviceChoice);

        // Now that we've gotten a device, we need to choose an action
        // for that device to take. For sake of simplicity, right now
        // we'll just use the 3 generic commands available:
        //
        // - Vibrate
        // - Rotate
        // - Linear (stroke/oscillate)
        //
        // Each device supported by the Buttplug C# library supports at
        // least one of these 3 commands, so we know that the user will
        // always have some option.

        Console.WriteLine("Choose an action:");
        Console.WriteLine("1. Vibrate");
        Console.WriteLine("2. Oscillate");
        Console.WriteLine("3. Rotate");
        Console.WriteLine("4. Linear");
        Console.WriteLine("5. Get Battery Level");

        if (!uint.TryParse(Console.ReadLine(), out var cmdChoice) ||
            cmdChoice == 0 || cmdChoice > 5) {
          Console.WriteLine("Invalid choice, try again.");
          return;
        }

        // We've got a device, and a command to take on that device.
        // Let's do this thing. For each command we'll either run at a
        // speed, then stop, or move to a position, then back again. To
        // ensure that we don't have to deal with concurrent commands
        // (again, for sake of example simplicity, real world situations
        // are gonna be far more dynamic than this), we'll just block
        // while this action is happening.
        //
        // We'll wrap each of our commands in a ButtplugDeviceException
        // try block, as a device might be disconnected between the time
        // we enter the command menu and send the command, and we don't
        // want to crash when that happens.
        if (cmdChoice == 1) {
          Console.WriteLine(
              $"Running all vibrators of {device.Name} at 50% for 1s.");
          try {
            await device.VibrateAsync(0.3);
            await Task.Delay(2000);
            await device.VibrateAsync(0);
          } catch (Exception e) {
            Console.WriteLine($"Problem vibrating: {e}");
          }
        } else if (cmdChoice == 2) {
          Console.WriteLine(
              $"Running all oscillators of {device.Name} at 50% for 1s.");
          try {
            await device.OscillateAsync(0.5);
            await Task.Delay(1000);
            await device.OscillateAsync(0);
          } catch (Exception e) {
            Console.WriteLine($"Problem oscillating: {e}");
          }
        } else if (cmdChoice == 3) {
          Console.WriteLine($"Rotating {device.Name} at 50% for 1s.");
          try {
            await device.RotateAsync(0.5, true);
            await Task.Delay(1000);
            await device.RotateAsync(0, true);
          } catch (Exception e) {
            Console.WriteLine($"Problem rotating: {e}");
          }
        } else if (cmdChoice == 4) {
          Console.WriteLine(
              $"Oscillating linear motors of {device.Name} from 20% to 80% over 3s");
          try {
            await device.LinearAsync(1000, 0.2);
            await Task.Delay(1100);
            await device.LinearAsync(1000, 0.8);
            await Task.Delay(1100);
            await device.LinearAsync(1000, 0.2);
            await Task.Delay(1100);
          } catch (Exception e) {
            Console.WriteLine($"Problem moving linearly: {e}");
          }
        } else if (cmdChoice == 5) {
          Console.WriteLine(
              $"Checking battery level of {device.Name}");
          try {
            Console.WriteLine($"Battery Level: {await device.BatteryAsync()}");
          } catch (Exception e) {
            Console.WriteLine($"Problem getting battery level: {e}");
          }
        }
      }

      // And finally, we arrive at the main menu. We give the user the
      // choice to scan for more devices (in case they forgot to turn them
      // on earlier or whatever), run a command on a device, or just quit.
      while (true) {
        Console.WriteLine("1. Scan For More Devices");
        Console.WriteLine("2. Control Devices");
        Console.WriteLine("3. Quit");
        Console.WriteLine("Choose an option: ");
        if (!uint.TryParse(Console.ReadLine(), out var choice) ||
            (choice == 0 || choice > 3)) {
          Console.WriteLine("Invalid choice, try again.");
          continue;
        }

        switch (choice) {
          case 1:
            await ScanForDevices();
            continue;
          case 2:
            await ControlDevice();
            continue;
          case 3:
            return;

          default:

            // Due to the check above, we'll never hit this, but eh.
            continue;
        }
      }

      // That's it! A full buttplug program. It doesn't do much, but with
      // the right toys, the right commands, and a user that doesn't
      // possibly mind getting lube on their keyboard, this program could
      // possibly get someone off.
      //
      // Mission Accomplished.
    }

    private static void Main() {
      // Setup a client, and wait until everything is done before exiting.
      RunExample().Wait();
    }
  }
}
