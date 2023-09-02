using Gebug64.Unfloader.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gebug64.Unfloader.Flashcart
{
    public class Everdrive : FlashcartBase
    {
        private const int WriteRomTargetAddress = 0x10000000;

        private const string Command_Test_Send = "cmdt";
        private const string Command_Test_Response = "cmdr";
        private const string Command_WriteRom = "cmdW";
        private const string Command_PifBoot_Send = "cmds";

        public Everdrive()
        {

        }

        public override void ProcessData()
        {
            throw new NotImplementedException();
        }

        public override void Send(IGebugMessage message)
        {
            throw new NotImplementedException();
        }

        public override bool Test()
        {
            SendEverdriveCommand(Command_Test_Send, 0, 0, 0);

            System.Threading.Thread.Sleep(100);

            var response = Read();

            if (object.ReferenceEquals(null, response))
            {
                return false;
            }

            if (response.Length < 4)
            {
                return false;
            }

            var commandResponse = System.Text.Encoding.ASCII.GetString(response.Take(Command_Test_Response.Length).ToArray());

            return commandResponse == Command_Test_Response;
        }

        public override void SendRom(byte[] filedata)
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

            Console.WriteLine($"Sending command: {Command_WriteRom}");

            var chunks = bytesLeft / 512;
            SendEverdriveCommand(Command_WriteRom, WriteRomTargetAddress, chunks, 0);

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

                Write(sendBuffer);

                bytesLeft -= bytesDo;
                bytesDone += bytesDo;

                double percentDone = 100.0 * (double)bytesDone / (double)((int)padSize);

                Console.WriteLine($"loop: sent {bytesDone} out of {(int)padSize} = {percentDone:0.00}%, {bytesLeft} remain");
            }

            var startSpin = System.Diagnostics.Stopwatch.StartNew();
            int spinCount = 0;

            while (true)
            {
                SendEverdriveCommand(Command_Test_Send, 0, 0, 0);
                System.Threading.Thread.Sleep(100);
                var response = Read();
                string commandResponse;
                if (!object.ReferenceEquals(null, response))
                {
                    commandResponse = System.Text.Encoding.ASCII.GetString(response);
                    if (commandResponse.StartsWith(Command_Test_Response))
                    {
                        break;
                    }
                }
                else
                {
                    spinCount++;
                }

                if (spinCount > 1000)
                {
                    throw new InvalidOperationException("Error uploading rom. All bytes written, but can't receive valid test command response from everdrive.");
                }
            }

            startSpin.Stop();

            // Delay is needed or it won't boot properly
            System.Threading.Thread.Sleep(500);

            SendEverdriveCommand(Command_PifBoot_Send, 0, 0, 0);

            Console.WriteLine($"Sending command: {Command_PifBoot_Send}");
        }

        /// <summary>
        /// Send a command to everdrive.
        /// </summary>
        /// <param name="commandText">4 letter command to send.</param>
        /// <param name="address">Address paramter. Should be zero if unused.</param>
        /// <param name="size512">Number of 512-byte chunks. Should be zero if unused.</param>
        /// <param name="arg">Argument paramter. Should be zero if unused.</param>
        /// <exception cref="NullReferenceException"></exception>
        /// <exception cref="ArgumentException"></exception>
        private void SendEverdriveCommand(string commandText, Int32 address, Int32 size512, Int32 arg)
        {
            if (string.IsNullOrEmpty(commandText))
            {
                throw new NullReferenceException($"{nameof(commandText)}");
            }

            if (commandText.Length != 4)
            {
                throw new ArgumentException($"Invalid everdrive command: {commandText}");
            }

            var sendBuffer = new byte[16];
            Array.Clear(sendBuffer, 0, sendBuffer.Length);

            int pos = 0;
            var cmdBytes = System.Text.Encoding.ASCII.GetBytes(commandText);
            foreach (var b in cmdBytes)
            {
                sendBuffer[pos++] = b;
            }

            BitUtility.Insert32Big(sendBuffer, pos, address);
            pos += sizeof(Int32);

            BitUtility.Insert32Big(sendBuffer, pos, size512);
            pos += sizeof(Int32);

            BitUtility.Insert32Big(sendBuffer, pos, arg);
            pos += sizeof(Int32);

            Write(sendBuffer);
        }
    }
}
