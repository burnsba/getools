using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Getools.Lib.Game.Asset.Model
{
    public class ModelData
    {
        [JsonProperty(PropertyName = "xpos")]
        public Single XPos { get; set; }

        [JsonProperty(PropertyName = "ypos")]
        public Single YPos { get; set; }

        [JsonProperty(PropertyName = "zpos")]
        public Single ZPos { get; set; }

        [JsonProperty(PropertyName = "bboxradius")]
        public Single BboxRadius { get; set; }

        [JsonProperty(PropertyName = "bbox_minx")]
        public Single BboxMinX { get; set; }

        [JsonProperty(PropertyName = "bbox_maxx")]
        public Single BboxMaxX { get; set; }

        [JsonProperty(PropertyName = "bbox_miny")]
        public Single BboxMinY { get; set; }

        [JsonProperty(PropertyName = "bbox_maxy")]
        public Single BboxMaxY { get; set; }

        [JsonProperty(PropertyName = "bbox_minz")]
        public Single BboxMinZ { get; set; }

        [JsonProperty(PropertyName = "bbox_maxz")]
        public Single BboxMaxZ { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

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
