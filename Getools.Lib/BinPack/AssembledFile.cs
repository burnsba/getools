//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;

//namespace Getools.Lib.BinPack
//{
//    /// <summary>
//    /// This will collect all the .data section data into <see cref="Data"/>.
//    /// Some items should be placed in .rodata, but the final offset will not be known
//    /// while building the .data section. These will be described by <see cref="RodataPointers"/>.
//    /// Once all .data and .rodata is described, call <see cref="GetLinkedFile"/> to resolve
//    /// all pointers and build the final file.
//    /// </summary>
//    public class AssembledFile
//    {
//        private List<byte[]> _dataSectionList = new List<byte[]>();
//        private List<byte[]> _rodataSectionList = new List<byte[]>();
//        private byte[] _linkedFile = null;
//        private FileBuildState _buildState = FileBuildState.DefaultUnknown;

//        /// <summary>
//        /// Initializes a new instance of the <see cref="AssembledFile"/> class.
//        /// </summary>
//        public AssembledFile()
//        {
//        }

//        private enum FileBuildState
//        {
//            DefaultUnknown,
//            StartedAssembly,
//            FullyLinked,
//        }

//        /// <summary>
//        /// Gets or sets items that should end up in .rodata.
//        /// </summary>
//        public List<PointerRodata> RodataPointers { get; set; } = new List<PointerRodata>();

//        /// <summary>
//        /// Optional call to clear interal state of <see cref="AssembledFile"/>.
//        /// If <see cref="GetLinkedFile"/> has been called, this will reset state to allow assembling again.
//        /// </summary>
//        public void BeginAssembling()
//        {
//            _buildState = FileBuildState.StartedAssembly;
//            _linkedFile = null;
//            _dataSectionList = new List<byte[]>();
//            _rodataSectionList = new List<byte[]>();
//        }

//        /// <summary>
//        /// Adds byte array to .data section of file. No alignment is considered at this time.
//        /// </summary>
//        /// <param name="data">Data chunk.</param>
//        public void AppendData(byte[] data)
//        {
//            if (_buildState == FileBuildState.DefaultUnknown || _buildState == FileBuildState.StartedAssembly)
//            {
//                _dataSectionList.Add(data);
//            }
//            else
//            {
//                throw new InvalidOperationException($"File is currently in invalid build state ({_buildState}). Call {nameof(BeginAssembling)} to reset state.");
//            }
//        }

//        /// <summary>
//        /// Builds .data section and .rodata section.
//        /// Resulting file is saved in an internal variable which can be retrieved by calling this again
//        /// (i.e., the return value can be ignored one or more times).
//        /// All pointers from .data to .rodata are resolved.
//        /// The .rodata section and end of file are aligned to 16 bytes.
//        /// Call <see cref="BeginAssembling"/> to reset the state.
//        /// </summary>
//        /// <returns>Full linked and assembled file as byte array.</returns>
//        public byte[] GetLinkedFile()
//        {
//            if (_buildState == FileBuildState.FullyLinked)
//            {
//                return _linkedFile;
//            }

//            var totalDataSectionSize = _dataSectionList.Sum(x => x.Length);
//            var dataSection = new byte[totalDataSectionSize];
//            var dataSectionOffset = 0;
//            var fileOffset = 0;
//            foreach (var list in _dataSectionList)
//            {
//                Array.Copy(list, 0, dataSection, dataSectionOffset, list.Length);
//                dataSectionOffset += list.Length;
//            }

//            // .rodata should be 16 aligned.
//            fileOffset = BitUtility.Align16(dataSectionOffset);
//            var rodataStart = fileOffset;

//            // build each piece of the .rodata and set the .data section pointers.
//            foreach (var p in RodataPointers)
//            {
//                p.Assemble(fileOffset);
//                _rodataSectionList.Add(p.AssembledData);

//                p.SetPointerAddress(dataSection);
//                fileOffset += p.RodataSize;
//            }

//            // end of file size needs to be aligned to 16 bytes.
//            int totalLinkedSize = BitUtility.Align16(fileOffset);

//            // now combine .data and .rodata into one byte array.
//            var fileBytes = new byte[totalLinkedSize];

//            dataSection.CopyTo(fileBytes, 0);

//            fileOffset = rodataStart;

//            foreach (var list in _rodataSectionList)
//            {
//                Array.Copy(list, 0, fileBytes, fileOffset, list.Length);
//                fileOffset += list.Length;
//            }

//            _linkedFile = fileBytes;
//            _buildState = FileBuildState.FullyLinked;
//            return _linkedFile;
//        }
//    }
//}
