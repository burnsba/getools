using System.Buffers.Binary;
using System.ComponentModel;
using System.Text;

namespace GeModelBboxDump
{
    internal class Program
    {
        static void Main(string[] args)
        {
            DirectoryInfo d = new DirectoryInfo("\\\\wsl.localhost\\Debian\\home\\tolos\\code\\goldeneye_src\\assets\\obseg\\prop\\");

            FileInfo[] files = d.GetFiles("P*Z.bin");

            var sortedFiles = files.OrderBy(x => x.Name);

            var specialSkip = new List<string>()
            {
                "PgoldeneyelogoZ.bin",
                "PlegalpageZ.bin",
                "PnintendologoZ.bin",
            };

            var outputfile = "output.json";
            var singleItems = new List<string>();

            foreach (FileInfo file in sortedFiles)
            {
                if (specialSkip.Contains(file.Name))
                {
                    continue;
                }

                var data = GetModelDataFromFile(file.FullName);
                singleItems.Add("    " + data.ToJsonString());
            }

            var outputText = "{\n" + string.Join(",\n", singleItems) + "\n}";

            System.IO.File.WriteAllText(outputfile, outputText);
        }

        public static ModelData GetModelDataFromFile(string fullPath)
        {
            var filename = System.IO.Path.GetFileNameWithoutExtension(fullPath);

            var bytes = System.IO.File.ReadAllBytes(fullPath);

            int position = 0;

            int modeldata_position_offset = 0;
            int modeldata_held_position_offset = 0;
            int modeldata_bbox_offset = 0;

            int found = 0;

            Span<byte> span;

            while (true)
            {
                if (found == 2)
                {
                    break;
                }

                // optional "05 special offset"
                if (bytes[position] == 0x05)
                {
                    position += 4;
                    continue;
                }

                if (bytes[position] == 0x00)
                {
                    if (bytes[position + 1] == 0x00)
                    {
                        if (bytes[position + 2] == 0x00 && bytes[position + 3] == 0x00)
                        {
                            // null ptional "05 special offset"
                            position += 4;
                            continue;
                        }

                        // image texture information
                        position += 12;
                        continue;
                    }

                    // main object heading
                    if (bytes[position + 1] == 0x01)
                    {
                        position += 24;
                        continue;
                    }

                    // struct model node: position data
                    if (bytes[position + 1] == 0x02)
                    {
                        span = bytes.AsSpan(position + 4, 4);

                        modeldata_position_offset = BinaryPrimitives.ReadInt32BigEndian(span);
                        modeldata_position_offset -= 0x05000000;

                        position += 24;

                        found++;

                        continue;
                    }

                    // struct model node: position held item
                    if (bytes[position + 1] == 0x15)
                    {
                        span = bytes.AsSpan(position + 4, 4);

                        modeldata_held_position_offset = BinaryPrimitives.ReadInt32BigEndian(span);
                        modeldata_held_position_offset -= 0x05000000;

                        position += 24;

                        found++;

                        continue;
                    }

                    // struct model node: bbox data
                    if (bytes[position + 1] == 0x0a)
                    {
                        span = bytes.AsSpan(position + 4, 4);

                        modeldata_bbox_offset = BinaryPrimitives.ReadInt32BigEndian(span);
                        modeldata_bbox_offset -= 0x05000000;

                        position += 24;

                        found++;

                        continue;
                    }

                    throw new NotSupportedException();
                }

                throw new NotSupportedException();
            }

            Single xpos = 0;
            Single ypos = 0;
            Single zpos = 0;
            Single bbox_radius = 0;

            if (modeldata_position_offset > 0)
            {
                span = bytes.AsSpan(modeldata_position_offset, 4);
                xpos = BinaryPrimitives.ReadSingleBigEndian(span);

                span = bytes.AsSpan(modeldata_position_offset + 4, 4);
                ypos = BinaryPrimitives.ReadSingleBigEndian(span);

                span = bytes.AsSpan(modeldata_position_offset + 8, 4);
                zpos = BinaryPrimitives.ReadSingleBigEndian(span);

                span = bytes.AsSpan(modeldata_position_offset + 24, 4);
                bbox_radius = BinaryPrimitives.ReadSingleBigEndian(span);
            }
            else if (modeldata_held_position_offset > 0)
            {
                span = bytes.AsSpan(modeldata_held_position_offset, 4);
                xpos = BinaryPrimitives.ReadSingleBigEndian(span);

                span = bytes.AsSpan(modeldata_held_position_offset + 4, 4);
                ypos = BinaryPrimitives.ReadSingleBigEndian(span);

                span = bytes.AsSpan(modeldata_held_position_offset + 8, 4);
                zpos = BinaryPrimitives.ReadSingleBigEndian(span);

                span = bytes.AsSpan(modeldata_held_position_offset + 16, 4);
                bbox_radius = BinaryPrimitives.ReadSingleBigEndian(span);
            }

            span = bytes.AsSpan(modeldata_bbox_offset + 4, 4);
            Single bbox_minx = BinaryPrimitives.ReadSingleBigEndian(span);

            span = bytes.AsSpan(modeldata_bbox_offset + 8, 4);
            Single bbox_maxx = BinaryPrimitives.ReadSingleBigEndian(span);

            span = bytes.AsSpan(modeldata_bbox_offset + 12, 4);
            Single bbox_miny = BinaryPrimitives.ReadSingleBigEndian(span);

            span = bytes.AsSpan(modeldata_bbox_offset + 16, 4);
            Single bbox_maxy = BinaryPrimitives.ReadSingleBigEndian(span);

            span = bytes.AsSpan(modeldata_bbox_offset + 20, 4);
            Single bbox_minz = BinaryPrimitives.ReadSingleBigEndian(span);

            span = bytes.AsSpan(modeldata_bbox_offset + 24, 4);
            Single bbox_maxz = BinaryPrimitives.ReadSingleBigEndian(span);

            var result = new ModelData()
            {
                XPos = xpos,
                YPos = ypos,
                ZPos = zpos,
                BboxRadius = bbox_radius,
                BboxMinX = bbox_minx,
                BboxMaxX = bbox_maxx,
                BboxMinY = bbox_miny,
                BboxMaxY = bbox_maxy,
                BboxMinZ = bbox_minz,
                BboxMaxZ = bbox_maxz,
                Name = filename,
            };

            return result;
        }
    }

    public class ModelData
    {
        public Single XPos { get; set; }
        public Single YPos { get; set; }
        public Single ZPos { get; set; }

        public Single BboxRadius { get; set; }

        public Single BboxMinX { get; set; }
        public Single BboxMaxX { get; set; }
        public Single BboxMinY { get; set; }
        public Single BboxMaxY { get; set; }
        public Single BboxMinZ { get; set; }
        public Single BboxMaxZ { get; set; }

        public string Name { get; set; }

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