using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Getools.Lib.BinPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Getools.Lib.Converters
{
    /// <summary>
    /// Helper JSON converter class.
    /// </summary>
    public class ClaimedStringPointerJsonConverter : JsonConverter
    {
        private readonly Type[] _types;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClaimedStringPointerJsonConverter"/> class.
        /// </summary>
        /// <param name="types">Types that can be converted.</param>
        public ClaimedStringPointerJsonConverter(params Type[] types)
        {
            _types = types;
        }

        /// <inheritdoc />
        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            if (value is ClaimedStringPointer csp)
            {
                writer.WriteValue(csp.GetString());
            }
        }

        /// <inheritdoc />
        public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            var s = reader.Value as string;

            if (object.ReferenceEquals(null, s))
            {
                throw new NullReferenceException();
            }

            return new ClaimedStringPointer(s);
        }

        /// <inheritdoc />
        public override bool CanConvert(Type objectType)
        {
            return _types.Any(t => t == objectType);
        }
    }
}
