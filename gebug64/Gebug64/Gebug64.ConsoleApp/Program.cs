﻿using Gebug64.Unfloader;
using Gebug64.Unfloader.Message;
using System.IO.Ports;
using System.Text;

namespace Gebug64.ConsoleApp
{
    internal class Program
    {
        private static bool _sigint = false;

        static void Main(string[] args)
        {
            var logger = new Logger();

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
            var device = new Unfloader.Flashcart.Everdrive(logger);
            var dm = new DeviceManager(device);

            dm.Init(usePort);
            dm.Start();
            //var testResult = dm.TestEverdriveConnected();

            //Console.WriteLine($"testResult: {testResult}");

            //if (!testResult)
            //{
            //    dm.Stop();
            //    Console.WriteLine("Test command failed.");
            //    return;
            //}

            //Console.WriteLine($"Send file: {filename}");
            //dm.SendRom(filename);

            //System.Threading.Thread.Sleep(3000);

            bool toggle = false;
            var sw = System.Diagnostics.Stopwatch.StartNew();

            while (true)
            {
                if (_sigint)
                {
                    break;
                }

                if (!dm.MessagesFromConsole.IsEmpty)
                {
                    IGebugMessage? msg = null;
                    
                    while (!dm.MessagesFromConsole.TryDequeue(out msg))
                    {
                        ;
                    }

                    if (msg is RomMessage)
                    {
                        Console.WriteLine(((RomMessage)msg).ToString());
                    }
                    else
                    {
                        Console.WriteLine(msg.GetUsbPacket().ToString());
                    }
                }

                if (sw.Elapsed.TotalSeconds > 5)
                {
                    sw.Stop();

                    Console.WriteLine("enqueue message ...");
                    if (toggle)
                    {
                        dm.EnqueueMessage(new RomMetaMessage(Unfloader.Message.MessageType.GebugCmdMeta.Ping) { Source = CommunicationSource.Pc } );
                    }
                    else
                    {
                        dm.EnqueueMessage(new RomMiscMessage(Unfloader.Message.MessageType.GebugCmdMisc.OsTime) { Source = CommunicationSource.Pc } );
                    }

                    toggle = !toggle;

                    sw = System.Diagnostics.Stopwatch.StartNew();
                }

                System.Threading.Thread.Sleep(10);
            }

            device.Disconnect();
            dm.Stop();

            System.Threading.Thread.Sleep(3000);
        }
    }
}