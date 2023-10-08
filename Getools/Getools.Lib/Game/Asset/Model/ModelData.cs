using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Getools.Lib.Game.Asset.Model
{
    /// <summary>
    /// Maps model data, as found in assets\obseg\prop\P*Z.bin files.
    /// </summary>
    public class ModelData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ModelData"/> class.
        /// </summary>
        public ModelData()
        {
        }

        /// <summary>
        /// X position.
        /// </summary>
        [JsonProperty(PropertyName = "xpos")]
        public Single XPos { get; init; }

        /// <summary>
        /// Y position.
        /// </summary>
        [JsonProperty(PropertyName = "ypos")]
        public Single YPos { get; init; }

        /// <summary>
        /// Z position.
        /// </summary>
        [JsonProperty(PropertyName = "zpos")]
        public Single ZPos { get; init; }

        /// <summary>
        /// Bounding box radius.
        /// </summary>
        [JsonProperty(PropertyName = "bboxradius")]
        public Single BboxRadius { get; init; }

        /// <summary>
        /// Bounding box, min x.
        /// </summary>
        [JsonProperty(PropertyName = "bbox_minx")]
        public Single BboxMinX { get; init; }

        /// <summary>
        /// Bounding box, min x.
        /// </summary>
        [JsonProperty(PropertyName = "bbox_maxx")]
        public Single BboxMaxX { get; init; }

        /// <summary>
        /// Bounding box, min y.
        /// </summary>
        [JsonProperty(PropertyName = "bbox_miny")]
        public Single BboxMinY { get; init; }

        /// <summary>
        /// Bounding box, max y.
        /// </summary>
        [JsonProperty(PropertyName = "bbox_maxy")]
        public Single BboxMaxY { get; init; }

        /// <summary>
        /// Bounding box, min z.
        /// </summary>
        [JsonProperty(PropertyName = "bbox_minz")]
        public Single BboxMinZ { get; init; }

        /// <summary>
        /// Bounding box, max z.
        /// </summary>
        [JsonProperty(PropertyName = "bbox_maxz")]
        public Single BboxMaxZ { get; init; }

        /// <summary>
        /// Name.
        /// </summary>
        [JsonProperty(PropertyName = "name")]
        public string? Name { get; init; }

        /// <summary>
        /// Copies object.
        /// </summary>
        /// <returns>Copy.</returns>
        public ModelData Clone()
        {
            return new ModelData()
            {
                XPos = XPos,
                YPos = YPos,
                ZPos = ZPos,
                BboxRadius = BboxRadius,
                BboxMinX = BboxMinX,
                BboxMaxX = BboxMaxX,
                BboxMinY = BboxMinY,
                BboxMaxY = BboxMaxY,
                BboxMinZ = BboxMinZ,
                BboxMaxZ = BboxMaxZ,
                Name = Name,
            };
        }

        /// <summary>
        /// Convert to standard json format.
        /// </summary>
        /// <returns>JSON string.</returns>
        public string ToJsonString()
        {
            var sb = new StringBuilder();

            sb.Append("{ ");
            sb.Append($"\"name\": \"{Name}\", ");

            sb.Append($"\"xpos\": {XPos}, ");
            sb.Append($"\"ypos\": {YPos}, ");
            sb.Append($"\"zpos\": {ZPos}, ");

            sb.Append($"\"bboxradius\": {BboxRadius}, ");

            sb.Append($"\"bbox_minx\": {BboxMinX}, ");
            sb.Append($"\"bbox_maxx\": {BboxMaxX}, ");
            sb.Append($"\"bbox_miny\": {BboxMinY}, ");
            sb.Append($"\"bbox_maxy\": {BboxMaxY}, ");
            sb.Append($"\"bbox_minz\": {BboxMinZ}, ");
            sb.Append($"\"bbox_maxz\": {BboxMaxZ}");

            sb.Append(" }");

            return sb.ToString();
        }
    }
}
