using System;
using System.Collections.Generic;
using System.Text;
using Gebug64.Unfloader.SerialPort;

namespace Gebug64.FakeConsole.Lib.FakeConsole
{
    [ConsoleDescription("Another", 2)]
    public class AnotherConsole : IFakeConsole
    {
        private ISerialPort _serialPort;

        public AnotherConsole(ISerialPort serialPort)
        {
            _serialPort = serialPort;
        }
    }
}
