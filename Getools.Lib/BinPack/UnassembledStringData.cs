//using System;
//using System.Collections.Generic;
//using System.Text;

//namespace Getools.Lib.BinPack
//{
//    /// <summary>
//    /// Container for a string that should end up in .rodat section.
//    /// </summary>
//    public class UnassembledStringData : IUnassembledData
//    {
//        /// <summary>
//        /// Initializes a new instance of the <see cref="UnassembledStringData"/> class.
//        /// </summary>
//        public UnassembledStringData()
//        {
//        }

//        /// <summary>
//        /// Initializes a new instance of the <see cref="UnassembledStringData"/> class.
//        /// </summary>
//        /// <param name="value">Value for <see cref="Data"/>.</param>
//        public UnassembledStringData(string value)
//        {
//            Data = value;
//        }

//        /// <summary>
//        /// String should be word aligned.
//        /// </summary>
//        public int ByteAlignment { get; } = 4;

//        /// <inheritdoc />
//        public object UncastData { get; set; }

//        /// <summary>
//        /// Gets or sets strongly typed string data.
//        /// Pass through to <see cref="UncastData"/>.
//        /// </summary>
//        public string Data
//        {
//            get
//            {
//                return (string)UncastData;
//            }

//            set
//            {
//                UncastData = (object)value;
//            }
//        }

//        /// <summary>
//        /// Implicit conversion from string.
//        /// </summary>
//        /// <param name="value">Zero terminated string (regular dotnet string).</param>
//        public static implicit operator UnassembledStringData(string value)
//        {
//            return new UnassembledStringData(value);
//        }

//        /// <inheritdoc />
//        public byte[] GetAssembledData(int currentAddress)
//        {
//            int endAddress;
//            int size;

//            if (string.IsNullOrEmpty(Data))
//            {
//                endAddress = BitUtility.AlignToWidth(currentAddress + ByteAlignment, ByteAlignment);
//                size = endAddress - currentAddress;

//                return new byte[size];
//            }

//            var len = Data.Length;
//            endAddress = BitUtility.AlignToWidth(currentAddress + len, ByteAlignment);
//            size = endAddress - currentAddress;

//            var arr = new byte[size];
//            Array.Copy(System.Text.Encoding.ASCII.GetBytes(Data), arr, len);

//            return arr;
//        }
//    }
//}
