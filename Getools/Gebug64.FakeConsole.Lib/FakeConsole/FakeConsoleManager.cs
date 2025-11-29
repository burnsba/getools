using Gebug64.FakeConsole.Lib.Flashcart;
using Gebug64.FakeConsole.Lib.SerialPort;
using Gebug64.Unfloader.SerialPort;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gebug64.FakeConsole.Lib.FakeConsole
{
    public class FakeConsoleManager
    {
        private ISerialPort _fakeSerialPort;
        private IFakeFlashcart _flashcart;
        private Dictionary<Type, IFakeConsole> _consoleInstances = new Dictionary<Type, IFakeConsole>();

        public FakeConsoleManager()
        {
            _fakeSerialPort = new FakeSerialPort("VSP1");
            _flashcart = new FakeEverdrive();
        }

        public IFakeConsole GetCreateConsole(Type type)
        {
            if (_consoleInstances.ContainsKey(type))
            {
                return _consoleInstances[type];
            }

            var instance = (IFakeConsole)Activator.CreateInstance(type, _fakeSerialPort, _flashcart)!;
            _consoleInstances.Add(type, instance);

            return instance;
        }

        public ISerialPort GetSerialPort() => _fakeSerialPort;
    }
}
