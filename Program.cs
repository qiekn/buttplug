using Buttplug.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Raylib_cs;
using System.Numerics;
using Microsoft.VisualBasic;

namespace ck.qiekn;

internal class Program {
  private static async Task WaitForKey() {
    Console.WriteLine("Press any key to continue.");
    while (!Console.KeyAvailable) {
      await Task.Delay(1);
    }
    Console.ReadKey(true);
  }

  private static async Task RunClient() {
    var client = new ButtplugClient("My Client");

    // Whenever a client connects, it asks the server for a list of devices
    // that may already be connected. Therefore we'll want to set up our
    // device handlers before we connect, so we can see what devices may
    // already be connected to the server.
    void HandleDeviceAdded(object? aObj, DeviceAddedEventArgs aArgs) {
      Console.WriteLine($"Device connected: {aArgs.Device.Name}");
    }

    client.DeviceAdded += HandleDeviceAdded;

    void HandleDeviceRemoved(object? aObj, DeviceRemovedEventArgs aArgs) {
      Console.WriteLine($"Device connected: {aArgs.Device.Name}");
    }

    client.DeviceRemoved += HandleDeviceRemoved;

    // Now we can connect.
    await client.ConnectAsync(new ButtplugWebsocketConnector(new Uri("ws://127.0.0.1:12345")));

    async Task ScanForDevices() {
      await client.StartScanningAsync();
      // await WaitForKey();
      // Stop scanning now, 'cause we don't want new devices popping up anymore.
      await client.StopScanningAsync();
    }

    // Scan for devices before we get to the main menu.
    await ScanForDevices();

    // Now we define the device control menus. After we've scanned for
    // devices, the user can use this menu to select a device, then
    // select an action for that device to take.
    async Task ControlDevice() {
      if (client.Devices.Length == 0) {
        Console.WriteLine("No devices available. Please scan for a device.");
        return;
      }

      var options = new List<uint>();

      foreach (var dev in client.Devices) {
        Console.WriteLine($"{dev.Index}. {dev.Name}");
        options.Add(dev.Index);
      }

      var deviceChoice = 0;
      var device = client.Devices.First(dev => dev.Index == deviceChoice);

      Console.WriteLine($"Running all vibrators of {device.Name} at 50% for 1s.");
      try {
        await device.VibrateAsync(0.5);
        await Task.Delay(1000);
        await device.VibrateAsync(0);
      } catch (Exception e) {
        Console.WriteLine($"Problem vibrating: {e}");
      }
    }

    // And finally, we arrive at the main menu. We give the user the
    // choice to scan for more devices (in case they forgot to turn them
    // on earlier or whatever), run a command on a device, or just quit.
    while (true) {
      Console.Clear();
      Console.WriteLine("--------------------------------------------");
      Console.WriteLine("s  [S]can For More Devices");
      Console.WriteLine("r  [R]un commnads -> Control Devices");
      Console.WriteLine("q  [Q]uit");
      Console.WriteLine("Choose an option: ");
      Console.WriteLine("--------------------------------------------");

      var cmd = Console.ReadLine()?.Trim().ToLower();
      if (string.IsNullOrEmpty(cmd)) {
        Console.WriteLine("Invalid choice, try again.");
        continue;
      }

      switch (cmd) {
        case "s":
          await ScanForDevices();
          continue;
        case "r":
          await ControlDevice();
          continue;
        case "q":
          return;
        default:
          // Due to the check above, we'll never hit this, but eh.
          continue;
      }
    }
  }

  private static void Main() {
    // Setup a client, and wait until everything is done before exiting.
    // RunClient().Wait();


    Raylib.InitWindow(Conf.ScreenWidth, Conf.ScreenHeight, "game");
    Raylib.SetTargetFPS(144);
    Raylib.SetConfigFlags(ConfigFlags.Msaa4xHint | ConfigFlags.HighDpiWindow);

    // 加载 Noto Serif 字体
    Font notoRegular = Raylib.LoadFontEx("assets/fonts/noto-regular.ttf", 64, null, 0);
    Font notoItalic = Raylib.LoadFontEx("assets/fonts/noto-italic.ttf", 64, null, 0);

    var game = new Game(notoRegular, notoItalic);
    game.Init();

    while (!Raylib.WindowShouldClose()) {
      game.HandleInput();
      game.Update();
      game.Draw();
    }

    game.CleanUp();
    Raylib.CloseWindow();
  }
}
