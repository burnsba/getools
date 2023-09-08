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
            string strExeFilePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string strWorkPath = System.IO.Path.GetDirectoryName(strExeFilePath);
            if (!Directory.Exists(strWorkPath))
            {
                throw new DirectoryNotFoundException(strWorkPath);
            }
            var localFiles = Directory.GetFiles(strWorkPath).Select(x => Path.GetFileName(x));
            var filename = localFiles.Where(x => x.StartsWith("ge007")).Where(x => x.EndsWith(".z64")).OrderBy(x => x).Last();

            Console.CancelKeyPress += (_, ccea) => {
                // Console.CancelKeyPress event immediately terminates the program after this
                // event handler finishes, resulting in a race condition where the program
                // is stuck looking at the remaining code but unwilling to execute it.
                // Setting the cancel property will communicate the app shouldn't be terminated.
                ccea.Cancel = true;
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

            Console.WriteLine($"Send file: {filename}");
            dm.SendRom(filename);

            System.Threading.Thread.Sleep(3000);

            while (true)
            {
                if (_sigint)
                {
                    break;
                }

                //if (device.HasReadData)
                //{
                //    var bytes = device.Read()!;

                //    UsbPacket pp = null;
                //    var parseResult = UsbPacket.Unwrap(bytes, out pp);
                //    if (parseResult == UsbPacketParseResult.Success)
                //    {
                //        var text = System.Text.Encoding.ASCII.GetString(pp.GetData());
                //        Console.WriteLine($"n64: size={pp.Size}, type={pp.DataType}, text={text}");
                //    }
                //    else
                //    {
                //        Console.WriteLine($"Error reading packet: {parseResult}");
                //    }
                //}

            //    IGebugMessage? msg = null;

            //    if (dm.MessagesFromConsole.TryDequeue(out msg))
            //    {
            //        Log(msg);
            //        Print(msg);
            //    }

                if (!dm.MessagesFromConsole.IsEmpty)
                {
                    IGebugMessage? msg = null;
                    
                    while (!dm.MessagesFromConsole.TryDequeue(out msg))
                    {
                        ;
                    }

                    Console.WriteLine(msg.UsbPacket.ToString());
                }
            }

            device.Disconnect();
            dm.Stop();

            System.Threading.Thread.Sleep(3000);
        }

        //static void Log(IGebugMessage msg)
        //{
        //    //
        //}

        //static void Print(IGebugMessage msg)
        //{
        //    Console.WriteLine(msg.GetFriendlyLogText());
        //}
    }
}