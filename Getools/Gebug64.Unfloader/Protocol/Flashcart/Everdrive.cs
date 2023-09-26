using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Gebug64.Unfloader.Protocol.Flashcart.Message;
using Gebug64.Unfloader.Protocol.Parse;
using Gebug64.Unfloader.Protocol.Unfloader;
using Gebug64.Unfloader.Protocol.Unfloader.Message;
using Gebug64.Unfloader.SerialPort;
using Getools.Lib;
using Microsoft.Extensions.Logging;

namespace Gebug64.Unfloader.Protocol.Flashcart
{
    public class Everdrive : Flashcart
    {
        public Everdrive(SerialPortProvider portProvider)
          : base(portProvider)
        {
        }

        public override void SendRom(byte[] filedata, CancellationToken? token = null)
        {
            var size = filedata.Length;
            UInt32 padSize = BitUtility.CalculatePadsize((UInt32)size);
            var bytesLeft = (int)padSize;
            int bytesDo;
            int bytesDone = 0;

            // if padded size is different from rom size, resize the file buffer.
            if (size != (int)padSize)
            {
                var newFileData = new byte[(int)padSize];
                Array.Copy(filedata, newFileData, size);
                filedata = newFileData;
            }

            Send(new EverdriveCmdWriteRom(bytesLeft));

            while (true)
            {
                if (bytesLeft >= 0x8000)
                {
                    bytesDo = 0x8000;
                }
                else
                {
                    bytesDo = bytesLeft;
                }

                // End if we've got nothing else to send
                if (bytesDo <= 0)
                    break;

                // Try to send chunks
                var sendBuffer = new byte[bytesDo];
                Array.Copy(filedata, bytesDone, sendBuffer, 0, bytesDo);

                WriteRaw(sendBuffer);

                bytesLeft -= bytesDo;
                bytesDone += bytesDo;

                double percentDone = 100.0 * (double)bytesDone / (double)((int)padSize);

                //_logger.Log(LogLevel.Debug, $"{nameof(SendRom)}: sent {bytesDone} out of {(int)padSize} = {percentDone:0.00}%, {bytesLeft} remain");

                if (token.HasValue && token.Value.IsCancellationRequested)
                {
                    return;
                }
            }

            // Delay is needed or it won't boot properly
            System.Threading.Thread.Sleep(3000);

            Send(new EverdriveCmdPifboot());
        }

        public override bool TestInMenu()
        {
            bool receivedTestResponse = false;

            Action<IFlashcartPacket> callback = packet =>
            {
                receivedTestResponse = true;
            };

            Func<IFlashcartPacket, bool> filter = packet =>
            {
                if (packet is EverdriveCmdTestResponse)
                {
                    return true;
                }

                return false;
            };

            _flashcartPacketBus.Subscribe(callback, 1, filter);

            WriteRaw(new EverdriveCmdTestSend().GetInnerPacket());

            int timeoutMs = 1000;
            var sw = Stopwatch.StartNew();

            while (receivedTestResponse != true)
            {
                System.Threading.Thread.Sleep(10);
                if (sw.Elapsed.TotalMilliseconds > timeoutMs)
                {
                    return false;
                }
            }

            return receivedTestResponse;
        }

        protected override FlashcartPacketParseResult TryParse(List<byte> data)
        {
            return EverdrivePacket.TryParse(data);
        }

        protected override IFlashcartPacket MakePacket(byte[] data)
        {
            return new EverdrivePacket(data);
        }
    }
}
