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
    /// Stan JSON serializer helper. Determines which properties should be included
    /// in the JSON or not.
    /// Used to toggle release vs beta values.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:Elements should be documented", Justification = "<Justification>")]
    public class StanShouldSerializeContractResolver : DefaultContractResolver
    {
        public static readonly StanShouldSerializeContractResolver Instance = new StanShouldSerializeContractResolver();

        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            var jsonProperties = base.CreateProperties(type, memberSerialization);

            // Filter here based on type, attribute or whatever and if want to customize a specific property type:
            foreach (var jsonProperty in jsonProperties)
            {
                if (jsonProperty.PropertyName == nameof(StandTile.DebugName))
                {
                    jsonProperty.Converter = new ClaimedStringPointerJsonConverter(typeof(ClaimedStringPointer));
                }
            }

            return jsonProperties;
        }

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty property = base.CreateProperty(member, memberSerialization);

            if (property.DeclaringType == typeof(StandTile) &&
                (property.PropertyName == nameof(StandTile.InternalName)
                || property.PropertyName == nameof(StandTile.Room)))
            {
                property.ShouldSerialize =
                    instance =>
                    {
                        StandTile x = (StandTile)instance;
                        return x.Format == TypeFormat.Normal;
                    };
            }
            else if (property.DeclaringType == typeof(StandTile) &&
                (property.PropertyName == nameof(StandTile.DebugName)
                || property.PropertyName == nameof(StandTile.UnknownBeta)))
            {
                property.ShouldSerialize =
                    instance =>
                    {
                        if (property.PropertyName == nameof(StandTile.DebugName))
                        {
                            property.Converter = new ClaimedStringPointerJsonConverter(typeof(ClaimedStringPointer));
                        }

                        StandTile x = (StandTile)instance;
                        return x.Format == TypeFormat.Beta;
                    };
            }
            else if (property.DeclaringType == typeof(StandTilePoint) &&
                (property.PropertyName == nameof(StandTilePoint.X)
                || property.PropertyName == nameof(StandTilePoint.Y)
                || property.PropertyName == nameof(StandTilePoint.Z)))
            {
                property.ShouldSerialize =
                    instance =>
                    {
                        StandTilePoint x = (StandTilePoint)instance;
                        return x.Format == TypeFormat.Normal;
                    };
            }
            else if (property.DeclaringType == typeof(StandTilePoint) &&
                (property.PropertyName == nameof(StandTilePoint.FloatX)
                || property.PropertyName == nameof(StandTilePoint.FloatY)
                || property.PropertyName == nameof(StandTilePoint.FloatZ)))
            {
                property.ShouldSerialize =
                    instance =>
                    {
                        StandTilePoint x = (StandTilePoint)instance;
                        return x.Format == TypeFormat.Beta;
                    };
            }

            return property;
        }
    }
}
