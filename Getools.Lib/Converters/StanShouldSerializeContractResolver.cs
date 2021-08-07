﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
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
                (property.PropertyName == nameof(StandTile.TileNameOffset)
                || property.PropertyName == nameof(StandTile.TileName)
                || property.PropertyName == nameof(StandTile.UnknownBeta)))
            {
                property.ShouldSerialize =
                    instance =>
                    {
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
