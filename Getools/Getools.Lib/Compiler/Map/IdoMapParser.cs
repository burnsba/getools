using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Getools.Lib.Compiler.Map
{
    /// <summary>
    /// Used to parse IDO 5 compiler output map file.
    /// </summary>
    public class IdoMapParser : IMapParser
    {
        private Regex _fileStartRe = new Regex("^\\s*\\.(text|bss|data|rodata)\\s*(0x[0-9a-fA-F]+)\\s*(0x[0-9a-fA-F]+)\\s*([a-zA-Z0-9_/\\.]+)\\s*$");
        private Regex _addressDetailRe = new Regex("^\\s*(0x[0-9a-fA-F]+)\\s*([a-zA-Z0-9_]+)\\s*$");

        /// <summary>
        /// Simple state machine for parsing.
        /// </summary>
        private enum ParseState
        {
            Initial,
            ReadFileLineStart,
            ReadingFile,
        }

        /// <inheritdoc />
        public List<MapDetail> ParseMapFile(string path, Func<string, bool>? segmentFilter = null, Func<UInt32, bool>? addressFilter = null)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException();
            }

            var results = new List<MapDetail>();

            var state = ParseState.Initial;
            var lines = File.ReadAllLines(path);
            string segmentName = string.Empty;
            string filename = string.Empty;
            string variableName;
            UInt32 address;

            foreach (var line in lines)
            {
                // If the line matches the "file start" regex, mark the segment and filename.
                if (_fileStartRe.IsMatch(line))
                {
                    var matches = _fileStartRe.Match(line);
                    var pendingSegmentName = matches.Groups[1].Value;

                    // Check parameter filter first.
                    if (segmentFilter != null)
                    {
                        if (!segmentFilter(pendingSegmentName))
                        {
                            continue;
                        }
                    }

                    // Ok, filter allowed this, so now set state and local variables.
                    state = ParseState.ReadFileLineStart;

                    segmentName = pendingSegmentName;
                    filename = GetFilenameFromLine(matches.Groups[4].Value);
                }
                else if (_addressDetailRe.IsMatch(line))
                {
                    /*** Else if the line matches the "detail" regex, extract info.
                     */

                    // The segment and filename need to be known.
                    // Valid states are ReadFileLineStart and ReadingFile.
                    // Or since there's only 3 states, continue if the other state.
                    if (state == ParseState.Initial)
                    {
                        continue;
                    }

                    var matches = _addressDetailRe.Match(line);
                    UInt32 pendingAddress = To32BitAddress(matches.Groups[1].Value);

                    // Check parameter filter first.
                    if (addressFilter != null)
                    {
                        if (!addressFilter(pendingAddress))
                        {
                            continue;
                        }
                    }

                    // Ok, filter allowed this, so now set state and local variables.
                    state = ParseState.ReadingFile;
                    variableName = matches.Groups[2].Value;
                    address = pendingAddress;

                    var entry = new MapDetail()
                    {
                        Address = address,
                        FileSource = filename,
                        Name = variableName,
                        SegmentName = segmentName,
                    };

                    results.Add(entry);
                }
                else
                {
                    /***
                     * Otherwise no regex matched, so revert to initial state.
                     **/
                    state = ParseState.Initial;
                }
            }

            return results;
        }

        /// <summary>
        /// Helper method to extract the filename without directory or filename extension.
        /// </summary>
        /// <param name="line">Line from map file.</param>
        /// <returns>Plain filename without extension. If the filename can't be parsed, returns <see cref="String.Empty"/>.</returns>
        private string GetFilenameFromLine(string line)
        {
            int lastSlash = line.LastIndexOf('/');
            if (lastSlash < 0)
            {
                lastSlash = 0;
            }
            else
            {
                lastSlash++;
            }

            int lastDot = line.LastIndexOf(".");
            if (lastDot < 0)
            {
                lastDot = line.Length;
            }

            int nameLength = lastDot - lastSlash;
            if (nameLength < 0)
            {
                return string.Empty;
            }

            return line.Substring(lastSlash, nameLength);
        }

        /// <summary>
        /// Attempts to parse an address as a 32 bit value.
        /// </summary>
        /// <param name="source">String to parse.</param>
        /// <returns>Parsed address, or zero on failure.</returns>
        private UInt32 To32BitAddress(string source)
        {
            UInt32 address = 0;

            try
            {
                int intval = (int)new System.ComponentModel.Int32Converter()!.ConvertFromString(source!)!;
                address = (UInt32)intval;
            }
            catch
            {
            }

            return address;
        }
    }
}
