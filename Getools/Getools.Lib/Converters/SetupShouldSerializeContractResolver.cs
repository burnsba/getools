using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Getools.Lib.BinPack;
using Getools.Lib.Game;
using Getools.Lib.Game.Asset.Stan;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Getools.Lib.Converters
{
    /// <summary>
    /// Setup JSON serializer helper. Determines which properties should be included
    /// in the JSON or not.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:Elements should be documented", Justification = "<Justification>")]
    public class SetupShouldSerializeContractResolver : DefaultContractResolver
    {
        public static readonly SetupShouldSerializeContractResolver Instance = new SetupShouldSerializeContractResolver();

        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            var jsonProperties = base.CreateProperties(type, memberSerialization);

            foreach (var jsonProperty in jsonProperties)
            {
                if (jsonProperty.PropertyType == typeof(ClaimedStringPointer))
                {
                    jsonProperty.Converter = new ClaimedStringPointerJsonConverter(typeof(ClaimedStringPointer));
                }
            }

            return jsonProperties;
        }

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty property = base.CreateProperty(member, memberSerialization);

            if (property.PropertyType == typeof(ClaimedStringPointer))
            {
                property.Converter = new ClaimedStringPointerJsonConverter(typeof(ClaimedStringPointer));
            }

            return property;
        }
    }
}
