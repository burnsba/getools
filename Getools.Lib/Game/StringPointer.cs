//using System;
//using System.Collections.Generic;
//using System.Text;

//namespace Getools.Lib.Game
//{
//    /// <summary>
//    /// String pointer convenience class for dealing with a pointer
//    /// to a string.
//    /// </summary>
//    public class StringPointer
//    {
//        /// <summary>
//        /// Initializes a new instance of the <see cref="StringPointer"/> class.
//        /// </summary>
//        public StringPointer()
//        {
//        }

//        /// <summary>
//        /// Initializes a new instance of the <see cref="StringPointer"/> class.
//        /// </summary>
//        /// <param name="value">Zero terminated string at offset.</param>
//        public StringPointer(string value)
//        {
//            Value = value;
//        }

//        /// <summary>
//        /// Initializes a new instance of the <see cref="StringPointer"/> class.
//        /// </summary>
//        /// <param name="offset">Offset being pointed to.</param>
//        /// <param name="value">Zero terminated string at offset.</param>
//        public StringPointer(int? offset, string value)
//        {
//            Offset = offset;
//            Value = value;
//        }

//        /// <summary>
//        /// Gets or sets the offset being pointed to.
//        /// </summary>
//        public int? Offset { get; set; }

//        /// <summary>
//        /// Gets or sets the value of the string.
//        /// </summary>
//        public string Value { get; set; }

//        /// <summary>
//        /// Implicit conversion from string.
//        /// </summary>
//        /// <param name="value">Zero terminated string.</param>
//        public static implicit operator StringPointer(string value)
//        {
//            return new StringPointer(value);
//        }

//        /// <summary>
//        /// Implicit convertion from int; offst value.
//        /// </summary>
//        /// <param name="offset">Offset being pointed to.</param>
//        public static implicit operator StringPointer(int offset)
//        {
//            return new StringPointer() { Offset = offset };
//        }

//        /// <inheritdoc />
//        public override string ToString()
//        {
//            return Value;
//        }

//        /// <summary>
//        /// Converts the string to a quoted c literal.
//        /// If the <see cref="Value"/> is null or empty, an
//        /// empty string is returned.
//        /// </summary>
//        /// <param name="prefix">Optional prefix before string.</param>
//        /// <returns>Quoted value.</returns>
//        public string ToCValue(string prefix = "")
//        {
//            if (string.IsNullOrEmpty(Value))
//            {
//                return prefix + Formatters.Strings.ToQuotedString(string.Empty);
//            }

//            return prefix + Formatters.Strings.ToQuotedString(Value);
//        }

//        /// <summary>
//        /// Converts the string to a quoted c literal.
//        /// If the <see cref="Value"/> is null or empty, the c macro NULL
//        /// is returned (without quotes).
//        /// </summary>
//        /// <param name="prefix">Optional prefix before string.</param>
//        /// <returns>Quoted value.</returns>
//        public string ToCValueOrNull(string prefix = "")
//        {
//            if (string.IsNullOrEmpty(Value))
//            {
//                return $"{prefix}NULL";
//            }

//            return prefix + Formatters.Strings.ToQuotedString(Value);
//        }

//        /// <summary>
//        /// Converts the string to a quoted c literal.
//        /// If the <see cref="Value"/> is null, the c macro NULL
//        /// is returned (without quotes). Otherwise, a quoted string is returned (this may be <see cref="string.Empty"/>).
//        /// </summary>
//        /// <param name="prefix">Optional prefix before string.</param>
//        /// <returns>Quoted value.</returns>
//        public string ToCValueOrNullEmpty(string prefix = "")
//        {
//            if (object.ReferenceEquals(null, Value))
//            {
//                return $"{prefix}NULL";
//            }

//            return prefix + Formatters.Strings.ToQuotedString(Value);
//        }
//    }
//}
