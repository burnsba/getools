using Gebug64.Unfloader;
using Gebug64.Unfloader.Message;
using System.IO.Ports;
using System.Text;

/**
TODO:

  - handle ungraceful disconnect
  - resolve whether a message has a response or not
 
*/

namespace Gebug64.ConsoleApp
{
    internal class Program
    {
        private static bool _sigint = false;

        static void Main(string[] args)
        {
            Console.CancelKeyPress += delegate {
                _sigint = true;
            };

            var ports = SerialPort.GetPortNames();
            foreach (var port in ports)
            {
                Console.WriteLine($"serial port: {port}");
            }

            var usePort = "COM5";
            var device = new Unfloader.Flashcart.Everdrive();
            var dm = new DeviceManager(device);

            dm.Init(usePort);
            dm.Start();
            var testResult = dm.Test();

            Console.WriteLine($"testResult: {testResult}");

            if (!testResult)
            {
                dm.Stop();
                Console.WriteLine("Test command failed.");
                return;
            }

            dm.SendRom("sm64.z64");

            System.Threading.Thread.Sleep(3000);

            //while (true)
            //{
            //    if (_sigint)
            //    {
            //        break;
            //    }

            //    IGebugMessage? msg = null;

            //    if (dm.MessagesFromConsole.TryDequeue(out msg))
            //    {
            //        Log(msg);
            //        Print(msg);
            //    }
            //}

            dm.Stop();
        }

        static void Log(IGebugMessage msg)
        {
            //
        }

        static void Print(IGebugMessage msg)
        {
            Console.WriteLine(msg.GetFriendlyLogText());
        }
    }
}