using Gebug64.Unfloader;
using Gebug64.Unfloader.Message;
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

            var device = new Unfloader.Flashcart.Everdrive();
            var dm = new DeviceManager(device);

            dm.Init();
            dm.Start();

            while (true)
            {
                if (_sigint)
                {
                    break;
                }

                IGebugMessage? msg = null;

                if (dm.MessagesFromConsole.TryDequeue(out msg))
                {
                    Log(msg);
                    Print(msg);
                }
            }

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