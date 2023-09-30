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
    /// <summary>
    /// Describes Everdrive flashcart device.
    /// </summary>
    public class Everdrive : Flashcart
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Everdrive"/> class.
        /// </summary>
        /// <param name="portProvider">Serial port provider.</param>
        /// <param name="logger">Logger.</param>
        public Everdrive(SerialPortProvider portProvider, ILogger logger)
          : base(portProvider, logger)
        {
        }

        /// <summary>
        /// Sends binary data to Everdrive to load and boot/run, then sends the PIFBoot command.
        /// </summary>
        /// <param name="filedata">ROM contents. It is assumed this is already in correct endieness.</param>
        /// <param name="token">Optional cancellation token.</param>
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

            _disableProcessIncoming = true;

            Send(new EverdriveCmdWriteRom(bytesLeft));

            var startSend = Stopwatch.StartNew();

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
                {
                    break;
                }

                // Try to send chunks
                var sendBuffer = new byte[bytesDo];
                Array.Copy(filedata, bytesDone, sendBuffer, 0, bytesDo);

                WriteRaw(sendBuffer);

                bytesLeft -= bytesDo;
                bytesDone += bytesDo;

                double percentDone = 100.0 * (double)bytesDone / (double)((int)padSize);
                if (percentDone > 99.99)
                {
                    percentDone = 100.0;
                }

                _logger.Log(LogLevel.Debug, $"{nameof(SendRom)}: sent {bytesDone} out of {(int)padSize} = {percentDone:0.00}%, {bytesLeft} remain");

                if (token.HasValue && token.Value.IsCancellationRequested)
                {
                    _logger.Log(LogLevel.Debug, $"{nameof(SendRom)}: cancelled");
                    _disableProcessIncoming = false;
                    return;
                }
            }

            startSend.Stop();

            int byteRate = (int)((double)bytesDone / startSend.Elapsed.TotalSeconds);
            _logger.Log(LogLevel.Debug, $"{nameof(SendRom)}: {bytesDone} bytes / {startSend.Elapsed.TotalSeconds:0.00} sec = {byteRate} bytes/sec");

            // Delay is needed or it won't boot properly
            System.Threading.Thread.Sleep(3000);

            Send(new EverdriveCmdPifboot());

            _disableProcessIncoming = false;

            // Process any waiting data that was disabled for SendRom.
            Task.Run(() => TryReadPacket());
        }

        /// <summary>
        /// Executes specific test to determine whether the connection is currently
        /// in the Everdrive menu.
        /// </summary>
        /// <returns>True if a valid Everdrive level response is received (test command response), false otherwise.</returns>
        /// <remarks>
        /// Used to test connection level.
        /// </remarks>
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

        /// <summary>
        /// Reads bytes from incoming source and attempts to read as many as rquired to
        /// parse a single Everdrive packet.
        /// </summary>
        /// <param name="data">Data to parse.</param>
        /// <returns>Parse result.</returns>
        protected override FlashcartPacketParseResult TryParse(List<byte> data)
        {
            return EverdrivePacket.TryParse(data);
        }

        /// <summary>
        /// Wraps binary data to create an Everdrive packet.
        /// </summary>
        /// <param name="data">Data to create packet.</param>
        /// <returns>Packet.</returns>
        protected override IFlashcartPacket MakePacket(byte[] data)
        {
            return new EverdrivePacket(data);
        }
    }
}
