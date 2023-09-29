using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gebug64.Unfloader.SerialPort
{
    /// <summary>
    /// Interface to match implementation of <see cref="System.IO.Ports.SerialPort"/>.
    /// Extended to allow a local in-memory connection to another <see cref="ISerialPort"/>.
    /// </summary>
    public interface ISerialPort : IDisposable
    {
        /// <summary>
        /// Indicates that data has been received through a port represented by the SerialPort object.
        /// </summary>
        event System.IO.Ports.SerialDataReceivedEventHandler DataReceived;

        /// <summary>
        /// Indicates that an error has occurred with a port represented by a SerialPort object.
        /// </summary>
        event System.IO.Ports.SerialErrorReceivedEventHandler ErrorReceived;

        /// <summary>
        /// Gets the number of bytes of data in the receive buffer.
        /// </summary>
        int BytesToRead { get; }

        /// <summary>
        /// Gets the number of bytes of data in the send buffer.
        /// </summary>
        int BytesToWrite { get; }

        /// <summary>
        /// Gets a value indicating the open or closed status of the SerialPort object.
        /// </summary>
        bool IsOpen { get; }

        /// <summary>
        /// Gets or sets the number of milliseconds before a time-out occurs when a read operation does not finish.
        /// </summary>
        int ReadTimeout { get; set; }

        /// <summary>
        /// Gets or sets the number of milliseconds before a time-out occurs when a write operation does not finish.
        /// </summary>
        int WriteTimeout { get; set; }

        /// <summary>
        /// Gets or sets the port for communications, including but not limited to all available COM ports.
        /// </summary>
        string PortName { get; }

        /// <summary>
        /// Gets the underlying Stream object for a SerialPort object.
        /// </summary>
        System.IO.Stream BaseStream { get; }

        /// <summary>
        /// Gets or sets a value that enables the Data Terminal Ready (DTR) signal during serial communication.
        /// </summary>
        bool DtrEnable { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the Request to Send (RTS) signal is enabled during serial communication.
        /// </summary>
        bool RtsEnable { get; set; }

        /// <summary>
        /// Closes the port connection, sets the <see cref="IsOpen"/> property to false, and disposes of the internal Stream object.
        /// </summary>
        void Close();

        /// <summary>
        /// Discards data from the serial driver's receive buffer.
        /// </summary>
        void DiscardInBuffer();

        /// <summary>
        /// Discards data from the serial driver's transmit buffer.
        /// </summary>
        void DiscardOutBuffer();

        /// <summary>
        /// Opens a new serial port connection.
        /// </summary>
        void Open();

        /// <summary>
        /// Reads a number of bytes from the SerialPort input buffer and writes those bytes into a byte array at the specified offset.
        /// </summary>
        /// <param name="buffer">The byte array to write the input to.</param>
        /// <param name="offset">The offset in <paramref name="buffer"/> at which to write the bytes.</param>
        /// <param name="count">The maximum number of bytes to read. Fewer bytes are read if <paramref name="count"/> is greater than the number of bytes in the input buffer.</param>
        void Read(byte[] buffer, int offset, int count);

        /// <summary>
        /// Writes a specified number of bytes to the serial port using data from a buffer.
        /// </summary>
        /// <param name="buffer">The byte array to write the input to.</param>
        /// <param name="offset">The offset in <paramref name="buffer"/> at which to write the bytes.</param>
        /// <param name="count">The maximum number of bytes to read. Fewer bytes are read if <paramref name="count"/> is greater than the number of bytes in the input buffer.</param>
        void Write(byte[] buffer, int offset, int count);

        /// <summary>
        /// Connects the output of this <see cref="ISerialPort"/> to the input of the other,
        /// and output of the other to this input of this <see cref="ISerialPort"/>.
        /// </summary>
        /// <param name="other">Other serial port to connect to.</param>
        void Connect(ISerialPort other);
    }
}
