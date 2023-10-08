using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Getools.Lib.BinPack
{
    /// <summary>
    /// MIPS file to build/assemble.
    /// See documentation on <see cref="IAssembleContext"/> for outline of compile process.
    /// </summary>
    public class MipsFile : IAssembleContext
    {
        private MipsElfSection _currentSection = MipsElfSection.DefaultUnknown;

        /// <summary>
        /// .data section contents as lib objects.
        /// </summary>
        private List<IBinData> _contents = new List<IBinData>();

        /// <summary>
        /// .rodata section contents as lib objects.
        /// </summary>
        private List<IBinData> _rodataContents = new List<IBinData>();

        /// <summary>
        /// Assembly phase file contents as byte arrays.
        /// </summary>
        private List<byte[]> _dataList = new List<byte[]>();

        /// <summary>
        /// Dictionary of pointer <see cref="IGetoolsLibObject.MetaId"/> to lib object <see cref="IGetoolsLibObject.MetaId"/>.
        /// </summary>
        private Dictionary<Guid, Guid> _pointerToObjectLookup = new Dictionary<Guid, Guid>();

        /// <summary>
        /// Reverse pointer lookup.
        /// For any lib object <see cref="IGetoolsLibObject.MetaId"/>, contains collection
        /// of pointer <see cref="IGetoolsLibObject.MetaId"/> that point to this object.
        /// </summary>
        private Dictionary<Guid, HashSet<Guid>> _objectToPointersLookup = new Dictionary<Guid, HashSet<Guid>>();

        /// <summary>
        /// Collection of pointer <see cref="IGetoolsLibObject.MetaId"/> that are NULL pointers.
        /// </summary>
        private HashSet<Guid> _nullPointers = new HashSet<Guid>();

        /// <summary>
        /// Dictionary of pointer <see cref="IGetoolsLibObject.MetaId"/> to pointer lib object.
        /// </summary>
        private Dictionary<Guid, IPointerVariable> _pointers = new Dictionary<Guid, IPointerVariable>();

        /// <summary>
        /// Collected, assembled, linked file contents.
        /// </summary>
        private byte[]? _linkedFile = null;

        /// <summary>
        /// Current file assembly context state.
        /// </summary>
        private FileBuildState _buildState = FileBuildState.DefaultUnknown;

        /// <summary>
        /// Current address of file while being built.
        /// This is updated during the assembly phase.
        /// </summary>
        private int _currentAddress = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="MipsFile"/> class.
        /// </summary>
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
            _pointers = new Dictionary<Guid, IPointerVariable>();
        }

        /// <inheritdoc />
        public void AppendToDataSection(IBinData data)
        {
            _contents.Add(data);
        }

        /// <inheritdoc />
        public void AppendToRodataSection(IBinData data)
        {
            _rodataContents.Add(data);
        }

        /// <inheritdoc />
        public void Assemble()
        {
            _currentSection = MipsElfSection.Data;

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

            _currentSection = MipsElfSection.Rodata;

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

            _currentSection = MipsElfSection.DefaultUnknown;
        }

        /// <inheritdoc />
        public byte[]? GetLinkedFile()
        {
            if (_buildState == FileBuildState.FullyLinked)
            {
                return _linkedFile;
            }

            if (object.ReferenceEquals(null, _dataList) || !_dataList.Any())
            {
                throw new InvalidOperationException($"Error, no byte arrays to build. Call {nameof(Assemble)} first.");
            }

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

        /// <inheritdoc />
        public AssembleAddressContext AssembleAppendBytes(byte[] bytes, int align)
        {
            int prior = _currentAddress;

            if (align > 1)
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

        /// <inheritdoc />
        public int GetCurrentAddress()
        {
            return _currentAddress;
        }

        /// <inheritdoc />
        public MipsElfSection GetCurrentSection()
        {
            return _currentSection;
        }

        /// <inheritdoc />
        public void RegisterPointer(IPointerVariable pointer)
        {
            var pointerKey = pointer.MetaId;

            IGetoolsLibObject? pointsTo = pointer.Dereference();

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

        /// <inheritdoc />
        public void RemovePointer(IPointerVariable pointer)
        {
            var pointerKey = pointer.MetaId;

            IGetoolsLibObject? pointsTo = pointer.Dereference();

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

        /// <inheritdoc />
        public void UnreferenceObject(IGetoolsLibObject pointsTo)
        {
            if (object.ReferenceEquals(null, pointsTo))
            {
                return;
            }

            HashSet<Guid>? backRef = null;
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
            HashSet<Guid>? backRef = null;

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
            HashSet<Guid>? backRef = null;

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
