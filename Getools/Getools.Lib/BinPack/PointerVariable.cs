﻿using System;
using System.Collections.Generic;
using System.Text;
using Getools.Lib.Game;
using Newtonsoft.Json;

namespace Getools.Lib.BinPack
{
    /// <summary>
    /// Lib object to model pointer used in game file.
    /// </summary>
    public class PointerVariable : IGetoolsLibObject, IBinData, IPointerVariable, IComparable
    {
        private IGetoolsLibObject? _pointsTo = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="PointerVariable"/> class.
        /// </summary>
        public PointerVariable()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PointerVariable"/> class.
        /// </summary>
        /// <param name="pointsTo">Object pointer points to.</param>
        public PointerVariable(IGetoolsLibObject pointsTo)
        {
            _pointsTo = pointsTo;
        }

        /// <inheritdoc />
        [JsonIgnore]
        public int ByteAlignment => Config.TargetPointerAlignment;

        /// <summary>
        /// Gets or sets the .data section offset of the pointer.
        /// </summary>
        [JsonIgnore]
        public int BaseDataOffset { get; set; }

        /// <summary>
        /// Gets the .data section size in bytes of the pointer.
        /// </summary>
        [JsonIgnore]
        public int BaseDataSize => Config.TargetPointerSize;

        /// <inheritdoc />
        [JsonIgnore]
        public int PointedToOffset { get; set; }

        /// <inheritdoc />
        [JsonIgnore]
        public int PointedToSize { get; set; }

        /// <inheritdoc />
        public string? AddressOfVariableName { get; set; }

        /// <inheritdoc />
        [JsonIgnore]
        public Guid MetaId { get; private set; } = Guid.NewGuid();

        /// <inheritdoc />
        public bool IsNull
        {
            get
            {
                return object.ReferenceEquals(null, _pointsTo);
            }
        }

        /// <summary>
        /// Implicit convertion from pointer to int, returns
        /// the <see cref="PointedToOffset"/> unless pointer is
        /// null, then <see cref="NullReferenceException"/> is thrown.
        /// </summary>
        /// <param name="pointer">Pointer variable.</param>
        public static implicit operator int(PointerVariable pointer)
        {
            /* The order of logic is a bit odd here, deserialization can set
             * the PointedToOffset without _pointsTo object being set, so
             * first check if the value is not null, then do null check,
             * then fall back to else statement (not null but zero).
             * */
            if (pointer.PointedToOffset > 0)
            {
                return pointer.PointedToOffset;
            }

            if (object.ReferenceEquals(null, pointer._pointsTo))
            {
                return 0;
            }

            return pointer.PointedToOffset;
        }

        /// <inheritdoc />
        public void AssignPointer(IGetoolsLibObject pointsTo)
        {
            _pointsTo = pointsTo;
        }

        /// <inheritdoc />
        public IGetoolsLibObject? Dereference()
        {
            return _pointsTo;
        }

        /// <summary>
        /// Gets value of the pointer as byte array.
        /// </summary>
        /// <param name="prependBytesCount">Optional parameter, number of '\0' characters to prepend before string.</param>
        /// <param name="appendBytesCount">Optional parameter, number of '\0' characters to append after string.</param>
        /// <returns>Pointer value.</returns>
        public byte[] ToByteArray(int? prependBytesCount = null, int? appendBytesCount = null)
        {
            int prepend = prependBytesCount ?? 0;
            var resultLength = prepend + Config.TargetPointerSize + (appendBytesCount ?? 0);

            var b = new byte[resultLength];

            BitUtility.Insert32Big(b, prepend, PointedToOffset);
            return b;
        }

        /// <inheritdoc />
        public void Collect(IAssembleContext context)
        {
            context.AppendToDataSection(this);
        }

        /// <inheritdoc />
        public void Assemble(IAssembleContext context)
        {
            context.AssembleAppendBytes(ToByteArray(), Config.TargetWordSize);
        }

        /// <inheritdoc />
        public int CompareTo(object? obj)
        {
            var other = obj as PointerVariable;

            if (object.ReferenceEquals(null, other) || other.IsNull)
            {
                // this object should be placed before null item
                return -1;
            }

            if (object.ReferenceEquals(obj, this) || PointedToOffset == other.PointedToOffset)
            {
                return 0;
            }

            return PointedToOffset.CompareTo(other.PointedToOffset);
        }
    }
}
