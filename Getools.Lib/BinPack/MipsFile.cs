using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Getools.Lib.BinPack
{
    public class MipsFile : IAssembleContext
    {
        private List<IBinData> _contents = new List<IBinData>();
        private List<IBinData> _rodataContents = new List<IBinData>();
        private List<byte[]> _dataList = new List<byte[]>();
        private Dictionary<Guid, Guid> _pointerToObjectLookup = new Dictionary<Guid, Guid>();
        private Dictionary<Guid, HashSet<Guid>> _objectToPointersLookup = new Dictionary<Guid, HashSet<Guid>>();
        private HashSet<Guid> _nullPointers = new HashSet<Guid>();
        private Dictionary<Guid, PointerVariable> _pointers = new Dictionary<Guid, PointerVariable>();
        private byte[] _linkedFile = null;
        private FileBuildState _buildState = FileBuildState.DefaultUnknown;
        private int _currentAddress = 0;

        public MipsFile()
        {
        }

        private enum FileBuildState
        {
            DefaultUnknown,
            StartedAssembly,
            FullyLinked,
        }

        /// <summary>
        /// Optional call to clear interal state of <see cref="AssembledFile"/>.
        /// If <see cref="GetLinkedFile"/> has been called, this will reset state to allow assembling again.
        /// </summary>
        public void BeginAssembling()
        {
            _buildState = FileBuildState.StartedAssembly;
            _linkedFile = null;
            _dataList = new List<byte[]>();
            _currentAddress = 0;
            _pointerToObjectLookup = new Dictionary<Guid, Guid>();
            _objectToPointersLookup = new Dictionary<Guid, HashSet<Guid>>();
            _nullPointers = new HashSet<Guid>();
            _pointers = new Dictionary<Guid, PointerVariable>();
        }

        public void AppendToDataSection(IBinData data)
        {
            _contents.Add(data);
        }

        public void AppendToRodataSection(IBinData data)
        {
            _rodataContents.Add(data);
        }

        public void Assemble()
        {
            foreach (var item in _contents)
            {
                item.Assemble(this);

                var pointsTo = item.MetaId;
                if (_objectToPointersLookup.ContainsKey(pointsTo))
                {
                    var pointerIds = _objectToPointersLookup[pointsTo];
                    foreach (var pointerId in pointerIds)
                    {
                        if (_pointers.ContainsKey(pointerId))
                        {
                            var pointer = _pointers[pointerId];
                            pointer.PointedToOffset = item.BaseDataOffset;
                        }
                    }
                }
            }

            var rodataAddress = BitUtility.AlignToWidth(_currentAddress, 16);
            var delta = rodataAddress - _currentAddress;

            if (delta > 0)
            {
                _dataList.Add(new byte[delta]);
                _currentAddress += delta;
            }

            foreach (var item in _rodataContents)
            {
                item.Assemble(this);

                var pointsTo = item.MetaId;
                if (_objectToPointersLookup.ContainsKey(pointsTo))
                {
                    var pointerIds = _objectToPointersLookup[pointsTo];
                    foreach (var pointerId in pointerIds)
                    {
                        if (_pointers.ContainsKey(pointerId))
                        {
                            var pointer = _pointers[pointerId];
                            pointer.PointedToOffset = item.BaseDataOffset;
                        }
                    }
                }
            }

            var endOfFileAddress = BitUtility.AlignToWidth(_currentAddress, 16);
            delta = endOfFileAddress - _currentAddress;

            if (delta > 0)
            {
                _dataList.Add(new byte[delta]);
            }
        }

        /// <summary>
        /// Builds .data section and .rodata section.
        /// Resulting file is saved in an internal variable which can be retrieved by calling this again
        /// (i.e., the return value can be ignored one or more times).
        /// All pointers from .data to .rodata are resolved.
        /// The .rodata section and end of file are aligned to 16 bytes.
        /// Call <see cref="BeginAssembling"/> to reset the state.
        /// </summary>
        /// <returns>Full linked and assembled file as byte array.</returns>
        public byte[] GetLinkedFile()
        {
            if (_buildState == FileBuildState.FullyLinked)
            {
                return _linkedFile;
            }

            // var totalFileSize = _dataList.Sum(x => x.Length);
            // var totalFile = new byte[totalFileSize];
            // var fileOffset = 0;
            // 
            // foreach (var list in _dataList)
            // {
            //     Array.Copy(list, 0, totalFile, fileOffset, list.Length);
            //     fileOffset += list.Length;
            // }

            var totalFile = _dataList.SelectMany(x => x).ToArray();

            // Now the file is fully assembled, set the pointer values
            foreach (var kvp in _pointers)
            {
                BitUtility.InsertPointer32Big(totalFile, kvp.Value.BaseDataOffset, kvp.Value.PointedToOffset);
            }

            _linkedFile = totalFile;
            _buildState = FileBuildState.FullyLinked;

            return _linkedFile;
        }

        public AssembleAddressContext AssembleAppendBytes(byte[] bytes, int align)
        {
            int prior = _currentAddress;

            if (align > 0)
            {
                var nextAddress = BitUtility.AlignToWidth(_currentAddress, align);
                var size = nextAddress - _currentAddress;
                if (size > 0)
                {
                    _currentAddress += size;
                    _dataList.Add(new byte[size]);
                }
            }

            int dataStart = _currentAddress;

            _dataList.Add(bytes);
            _currentAddress += bytes.Length;

            int finalCurrentAddress = _currentAddress;

            return new AssembleAddressContext(prior, dataStart, finalCurrentAddress);
        }

        public int GetCurrentAddress()
        {
            return _currentAddress;
        }

        public void RegisterPointer(PointerVariable pointer)
        {
            var pointerKey = pointer.MetaId;

            IGetoolsLibObject pointsTo = pointer.Dereference();

            if (!object.ReferenceEquals(null, pointsTo))
            {
                var pointsToKey = pointsTo.MetaId;

                SetPointerToObject(pointerKey, pointsToKey);
                AddPointerReferenceOnObject(pointerKey, pointsToKey);

                if (!_pointers.ContainsKey(pointerKey))
                {
                    _pointers.Add(pointerKey, pointer);
                }
            }
            else
            {
                _nullPointers.Add(pointerKey);
            }
        }

        public void RemovePointer(PointerVariable pointer)
        {
            var pointerKey = pointer.MetaId;

            IGetoolsLibObject pointsTo = pointer.Dereference();

            if (!object.ReferenceEquals(null, pointsTo))
            {
                var pointsToKey = pointsTo.MetaId;

                RemovePointerToObject(pointerKey);
                RemovePointerReferenceOnObject(pointerKey, pointsToKey);

                if (_pointers.ContainsKey(pointerKey))
                {
                    _pointers.Remove(pointerKey);
                }
            }
            else
            {
                _nullPointers.Remove(pointerKey);
            }
        }

        public void UnreferenceObject(IGetoolsLibObject pointsTo)
        {
            if (object.ReferenceEquals(null, pointsTo))
            {
                return;
            }

            HashSet<Guid> backRef = null;
            var pointsToKey = pointsTo.MetaId;

            if (_objectToPointersLookup.TryGetValue(pointsToKey, out backRef))
            {
                if (object.ReferenceEquals(null, backRef))
                {
                    // nothing to do
                }
                else
                {
                    foreach (var pointerId in backRef)
                    {
                        RemovePointerToObject(pointerId);
                    }

                    _objectToPointersLookup.Remove(pointsToKey);
                }
            }
        }

        private void SetPointerToObject(Guid pointerKey, Guid pointsToKey)
        {
            if (!_pointerToObjectLookup.ContainsKey(pointerKey))
            {
                _pointerToObjectLookup.Add(pointerKey, pointsToKey);
            }
            else
            {
                _pointerToObjectLookup[pointerKey] = pointsToKey;
            }
        }

        private void AddPointerReferenceOnObject(Guid pointerKey, Guid pointsToKey)
        {
            HashSet<Guid> backRef = null;

            if (_objectToPointersLookup.TryGetValue(pointsToKey, out backRef))
            {
                if (object.ReferenceEquals(null, backRef))
                {
                    backRef = new HashSet<Guid>();
                    backRef.Add(pointerKey);
                    _objectToPointersLookup[pointsToKey] = backRef;
                }
                else
                {
                    backRef.Add(pointerKey);
                }
            }
            else
            {
                backRef = new HashSet<Guid>();
                backRef.Add(pointerKey);
                _objectToPointersLookup.Add(pointsToKey, backRef);
            }
        }

        private void RemovePointerToObject(Guid pointerKey)
        {
            if (_pointerToObjectLookup.ContainsKey(pointerKey))
            {
                _pointerToObjectLookup.Remove(pointerKey);
            }
        }

        private void RemovePointerReferenceOnObject(Guid pointerKey, Guid pointsToKey)
        {
            HashSet<Guid> backRef = null;

            if (_objectToPointersLookup.TryGetValue(pointsToKey, out backRef))
            {
                if (object.ReferenceEquals(null, backRef))
                {
                    // nothing to remove
                }
                else
                {
                    backRef.Remove(pointerKey);
                }
            }

            // else nothing to remove
        }
    }
}
