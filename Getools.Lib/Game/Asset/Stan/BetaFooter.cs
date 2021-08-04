using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;

namespace Getools.Lib.Game.Asset.Stan
{
    public class BetaFooter
    {
        public BetaFooter()
        {
        }

        /// <summary>
        /// Beta stan files include ASCII names of points after the above footer.
        /// </summary>
        public List<String> BetaPointList { get; set; } = new List<string>();

        public static BetaFooter ReadFromBetaBinFile(BinaryReader br)
        {
            var result = new BetaFooter();

            var buffer = new Byte[16];
            long position = br.BaseStream.Position;
            Byte b;
            int bufferPosition = 0;

            // read points until end of file
            while (position < br.BaseStream.Length - 1)
            {
                b = br.ReadByte();
                if (b > 0)
                {
                    buffer[bufferPosition] = b;
                    bufferPosition++;
                }
                else if (b == 0)
                {
                    if (buffer[0] > 0)
                    {
                        var pointName = System.Text.Encoding.ASCII.GetString(buffer);
                        result.BetaPointList.Add(pointName);

                        Array.Clear(buffer, 0, 16);
                        bufferPosition = 0;
                    }
                }

                if (bufferPosition >= 16)
                {
                    throw new Exception($"Error reading stan, beta point name exceeded buffer length. Stream positiion: {position}");
                }
            }

            if (buffer[0] > 0)
            {
                var pointName = System.Text.Encoding.ASCII.GetString(buffer);
                result.BetaPointList.Add(pointName);
            }

            return result;
        }
    }
}
