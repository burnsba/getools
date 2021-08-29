﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace Getools.Lib.Game.Asset.Setup
{
    /// <summary>
    /// Default filler block section.
    /// </summary>
    public class UnrefSectionUnknown : SetupDataSection
    {
        private const string _defaultVariableName = "unknown_setup_block";

        // 1 for byte, 4 for word
        private int _dataElementSize = 1;

        // "u8" for byte, "s32" for word
        private string _dataTypeName = "u8";

        private byte[] _byteData = null;
        private int[] _wordData = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnrefSectionUnknown"/> class.
        /// </summary>
        /// <param name="data">Section data.</param>
        [JsonConstructor]
        public UnrefSectionUnknown(byte[] data)
            : base(SetupSectionId.UnreferencedUnknown, _defaultVariableName)
        {
            IsUnreferenced = true;

            var len = data.Length;
            if ((len % 4) == 0)
            {
                _dataTypeName = "s32";
                _dataElementSize = 4;

                var wordLen = len / 4;

                _wordData = new int[wordLen];

                int index = 0;
                for (int i = 0; i < wordLen; i++)
                {
                    _wordData[i] = BitUtility.Read32Big(data, index);
                    index += 4;
                }
            }

            // should always be available as a byte array ...
            _byteData = new byte[len];
            Array.Copy(data, _byteData, data.Length);
        }

        /// <summary>
        /// Gets the length in bytes of the data.
        /// </summary>
        public int Length => _byteData?.Length ?? 0;

        [JsonProperty("Data")]
        private byte[] JsonPropData
        {
            get
            {
                return _byteData;
            }
        }

        /// <summary>
        /// Gets the section as a sequence of ints.
        /// </summary>
        /// <returns>Data.</returns>
        public int[] GetDataWords()
        {
            if (_dataElementSize != Config.TargetWordSize)
            {
                throw new NotSupportedException($"Data element is not word sized (={Config.TargetWordSize} bytes)");
            }

            return _wordData;
        }

        /// <summary>
        /// Gets the section as a sequence of bytes.
        /// </summary>
        /// <returns>Data.</returns>
        public byte[] GetDataBytes()
        {
            return _byteData;
        }

        /// <inheritdoc />
        public override void DeserializeFix(int startingIndex = 0)
        {
            int index = startingIndex;
            VariableName = $"{_defaultVariableName}_{index}";
        }

        /// <inheritdoc />
        public override string GetDeclarationTypeName()
        {
            return $"{_dataTypeName} {VariableName}[]";
        }

        /// <inheritdoc />
        public override int GetEntriesCount()
        {
            return 1;
        }

        /// <inheritdoc />
        public override void WritePrequelData(StreamWriter sw)
        {
            // nothing to do
        }

        /// <inheritdoc />
        public override void WriteSectionData(StreamWriter sw)
        {
            sw.Write($"{GetDeclarationTypeName()} = {{ ");

            if (_dataElementSize == 4)
            {
                sw.Write(string.Join(", ", _wordData));
            }
            else
            {
                sw.Write(string.Join(", ", _byteData));
            }

            sw.WriteLine(" };");
        }

        /// <inheritdoc />
        public override int GetPrequelDataSize()
        {
            return Length;
        }
    }
}
