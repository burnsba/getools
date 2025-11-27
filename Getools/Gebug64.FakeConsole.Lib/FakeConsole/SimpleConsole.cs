using System;
using System.Collections.Generic;
using System.Text;
using Gebug64.Unfloader.SerialPort;

namespace Gebug64.FakeConsole.Lib.FakeConsole
{
    [ConsoleDescription("Simple", 1)]
    public class SimpleConsole : IFakeConsole
    {
        private ISerialPort _serialPort;

        public SimpleConsole(ISerialPort serialPort)
        {
            _serialPort = serialPort;
        }
    }
}
