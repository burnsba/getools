using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Getools.Lib.BinPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Getools.Lib.Converters
{
    public class ClaimedStringPointerJsonConverter : JsonConverter
    {
        private readonly Type[] _types;

        public ClaimedStringPointerJsonConverter(params Type[] types)
        {
            _types = types;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is ClaimedStringPointer csp)
            {
                writer.WriteValue(csp.GetString());
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var s = reader.Value as string;
            return new ClaimedStringPointer(s);
        }

        public override bool CanConvert(Type objectType)
        {
            return _types.Any(t => t == objectType);
        }
    }
}
