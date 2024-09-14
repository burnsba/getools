using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Gebug64.Unfloader.Protocol.Gebug.Message;
using Gebug64.Win.Enum;
using Gebug64.Win.Mvvm;
using Getools.Lib;

namespace Gebug64.Win.ViewModels
{
    /// <summary>
    /// Viewmodel for single/active memory watch.
    /// </summary>
    public class MemoryWatchViewModel : ViewModelBase
    {
        private byte[] _valueArr;
        private string _friendlyDataValue = string.Empty;
        private MemoryDisplayFormat _displayFormat = MemoryDisplayFormat.Decimal;
        private MemoryDataType _dataType;

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryWatchViewModel"/> class.
        /// </summary>
        public MemoryWatchViewModel()
        {
            _valueArr = new byte[1];
            UpdateFriendlyDataValue();
        }

        /// <summary>
        /// Common list of available display formats to show in UI.
        /// </summary>
        public static List<MemoryDisplayFormat> AvailableDisplayFormat { get; } = new List<MemoryDisplayFormat>()
        {
            MemoryDisplayFormat.Decimal,
            MemoryDisplayFormat.Hex,
            MemoryDisplayFormat.Float0_00,
            MemoryDisplayFormat.Float0_0000,
            MemoryDisplayFormat.Float0_x,
        };

        /// <summary>
        /// Common list of available data types to show in UI.
        /// </summary>
        public static List<MemoryDataType> AvailableDataType { get; } = new List<MemoryDataType>()
        {
            MemoryDataType.S8,
            MemoryDataType.U8,
            MemoryDataType.S16,
            MemoryDataType.U16,
            MemoryDataType.S32,
            MemoryDataType.U32,
            MemoryDataType.F32,
            MemoryDataType.Pointer,
            MemoryDataType.Array,
        };

        /// <summary>
        /// Unique memory watch id.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Friendly name/alias of <see cref="MemoryAddress"/>.
        /// </summary>
        public string FriendlyAddress { get; set; } = string.Empty;

        /// <summary>
        /// Underlying memory address of memory watch.
        /// </summary>
        public UInt32 MemoryAddress { get; set; }

        /// <summary>
        /// How to display memory watch data.
        /// </summary>
        public MemoryDisplayFormat DisplayFormat
        {
            get => _displayFormat;
            set
            {
                if (_displayFormat == value)
                {
                    return;
                }

                _displayFormat = value;
                OnPropertyChanged(nameof(DisplayFormat));
                OnPropertyChanged(nameof(FriendlyDataValue));

                UpdateFriendlyDataValue();
            }
        }

        /// <summary>
        /// Data structure of memory watch data.
        /// </summary>
        public MemoryDataType DataType
        {
            get => _dataType;
            set
            {
                if (_dataType == value)
                {
                    return;
                }

                _dataType = value;
                OnPropertyChanged(nameof(DataType));
                OnPropertyChanged(nameof(FriendlyDataValue));

                UpdateFriendlyDataValue();
            }
        }

        /// <summary>
        /// Number of bytes read by memory watch.
        /// </summary>
        public int Size { get; set; }

        /// <summary>
        /// Format and display the memory watch data value according to <see cref="DataType"/> and <see cref="DisplayFormat"/>.
        /// </summary>
        public string FriendlyDataValue => _friendlyDataValue;

        /// <summary>
        /// Update ViewModel data value and notify property change.
        /// Assumes the values from console are for this <see cref="Id"/>.
        /// </summary>
        /// <param name="watch">Memory watch read values from console.</param>
        public void UpdateFromConsole(GebugMesgMemoryWatchItem watch)
        {
            if (_valueArr.Length < Size)
            {
                _valueArr = new byte[Size];
            }

            if (Enumerable.SequenceEqual(watch.Data!, _valueArr))
            {
                return;
            }

            Array.Clear(_valueArr, 0, Size);
            Array.Copy(watch.Data!, 0, _valueArr, 0, Size);

            UpdateFriendlyDataValue();
        }

        /// <summary>
        /// Helper method.
        /// Should only be called if <see cref="_valueArr"/> has changed.
        /// Updates <see cref="_friendlyDataValue"/> then notifies <see cref="FriendlyDataValue"/> has changed.
        /// </summary>
        private void UpdateFriendlyDataValue()
        {
            _friendlyDataValue = string.Empty;

            if (DataType == MemoryDataType.S8)
            {
                sbyte val = (sbyte)_valueArr[0];

                if (DisplayFormat == MemoryDisplayFormat.Decimal)
                {
                    _friendlyDataValue = $"{val}";
                }
                else if (DisplayFormat == MemoryDisplayFormat.Hex)
                {
                    _friendlyDataValue = Getools.Lib.Formatters.IntegralTypes.ToHex2((byte)val);
                }
            }
            else if (DataType == MemoryDataType.U8)
            {
                byte val = _valueArr[0];

                if (DisplayFormat == MemoryDisplayFormat.Decimal)
                {
                    _friendlyDataValue = $"{val}";
                }
                else if (DisplayFormat == MemoryDisplayFormat.Hex)
                {
                    _friendlyDataValue = Getools.Lib.Formatters.IntegralTypes.ToHex2(val);
                }
            }
            else if (DataType == MemoryDataType.S16 && Size >= 2 && _valueArr.Length >= 2)
            {
                short val16 = (short)BitUtility.Read16Big(_valueArr, 0);

                if (DisplayFormat == MemoryDisplayFormat.Decimal)
                {
                    _friendlyDataValue = $"{val16}";
                }
                else if (DisplayFormat == MemoryDisplayFormat.Hex)
                {
                    _friendlyDataValue = Getools.Lib.Formatters.IntegralTypes.ToHex4(val16);
                }
            }
            else if (DataType == MemoryDataType.U16 && Size >= 2 && _valueArr.Length >= 2)
            {
                ushort val16 = (ushort)BitUtility.Read16Big(_valueArr, 0);

                if (DisplayFormat == MemoryDisplayFormat.Decimal)
                {
                    _friendlyDataValue = $"{val16}";
                }
                else if (DisplayFormat == MemoryDisplayFormat.Hex)
                {
                    _friendlyDataValue = Getools.Lib.Formatters.IntegralTypes.ToHex4(val16);
                }
            }
            else if (DataType == MemoryDataType.S32 && Size >= 4 && _valueArr.Length >= 4)
            {
                Int32 val32 = (Int32)BitUtility.Read32Big(_valueArr, 0);

                if (DisplayFormat == MemoryDisplayFormat.Decimal)
                {
                    _friendlyDataValue = $"{val32}";
                }
                else if (DisplayFormat == MemoryDisplayFormat.Hex)
                {
                    _friendlyDataValue = Getools.Lib.Formatters.IntegralTypes.ToHex8(val32);
                }
            }
            else if (DataType == MemoryDataType.U32 && Size >= 4 && _valueArr.Length >= 4)
            {
                UInt32 val32 = (UInt32)BitUtility.Read32Big(_valueArr, 0);

                if (DisplayFormat == MemoryDisplayFormat.Decimal)
                {
                    _friendlyDataValue = $"{val32}";
                }
                else if (DisplayFormat == MemoryDisplayFormat.Hex)
                {
                    _friendlyDataValue = Getools.Lib.Formatters.IntegralTypes.ToHex8(val32);
                }
            }
            else if (DataType == MemoryDataType.F32 && Size >= 4 && _valueArr.Length >= 4)
            {
                Single val32 = BitUtility.CastToFloat((int)BitUtility.Read32Big(_valueArr, 0));

                if (DisplayFormat == MemoryDisplayFormat.Float0_00)
                {
                    _friendlyDataValue = $"{val32:0.00}";
                }
                else if (DisplayFormat == MemoryDisplayFormat.Float0_0000)
                {
                    _friendlyDataValue = $"{val32:0.0000}";
                }
                else if (DisplayFormat == MemoryDisplayFormat.Float0_x)
                {
                    _friendlyDataValue = $"{val32}";
                }
            }
            else if (DataType == MemoryDataType.Pointer && Size >= 4 && _valueArr.Length >= 4)
            {
                UInt32 val32 = (UInt32)BitUtility.Read32Big(_valueArr, 0);

                _friendlyDataValue = Getools.Lib.Formatters.IntegralTypes.ToHex8(val32);
            }
            else if (DataType == MemoryDataType.Array)
            {
                _friendlyDataValue = string.Join(" ", _valueArr.Select(x => x.ToString("X2")));
            }
            else
            {
                return;
            }

            OnPropertyChanged(nameof(FriendlyDataValue));
        }
    }
}
