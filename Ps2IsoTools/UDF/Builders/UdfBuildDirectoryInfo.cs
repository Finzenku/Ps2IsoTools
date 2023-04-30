//
// Copyright (c) 2008-2011, Kenneth Bell
//
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.
//


using Ps2IsoTools.DiscUtils.Utils;
using Ps2IsoTools.ISO.Builders;
using Ps2IsoTools.UDF.Descriptors;
using Ps2IsoTools.UDF.Descriptors.FileStructure;
using System.Text;

namespace Ps2IsoTools.UDF.Builders
{
    /// <summary>
    /// Represents a directory that will be built into the ISO image.
    /// </summary>
    public sealed class UdfBuildDirectoryInfo : BuildDirectoryInfo
    {
        internal UdfBuildDirectoryInfo(string name, UdfBuildDirectoryInfo parent) : base(name, parent) { }

        internal long GetUdfDataSize(Encoding enc)
        {
            List<BuildDirectoryMember> sorted = GetSortedMembers();

            long total = 40;
            total = FileIdentifierDescriptor.GetDataLength("\0", enc);

            foreach (BuildDirectoryMember m in sorted)
            {
                uint recordSize = FileIdentifierDescriptor.GetDataLength(m.ShortName, enc);

                // If this record would span a sector boundary, then the current sector is
                // zero-padded, and the record goes at the start of the next sector.
                if (total % IsoUtilities.SectorSize + recordSize > IsoUtilities.SectorSize)
                {
                    long padLength = IsoUtilities.SectorSize - total % IsoUtilities.SectorSize;
                    total += padLength;
                }

                total += recordSize;
            }

            return total;
        }

        internal int WriteUdf(byte[] buffer, int offset, uint sector, Dictionary<BuildDirectoryMember, LongAllocationDescriptor> locationTable, Encoding enc)
        {
            int pos = 0;

            List<BuildDirectoryMember> sorted = GetSortedMembers();

            // Self entry
            pos += WriteUdfMember(this, sector, "\0", enc, buffer, offset + pos, locationTable, FileCharacteristics.Parent | FileCharacteristics.Directory);

            foreach (BuildDirectoryMember m in sorted)
            {
                uint recordSize = m.GetDirectoryRecordSize(enc);

                if (pos % IsoUtilities.SectorSize + recordSize > IsoUtilities.SectorSize)
                {
                    int padLength = IsoUtilities.SectorSize - pos % IsoUtilities.SectorSize;
                    Array.Clear(buffer, offset + pos, padLength);
                    pos += padLength;
                }

                pos += WriteUdfMember(m, sector, null, enc, buffer, offset + pos, locationTable);
            }

            // Ensure final padding data is zero'd
            int finalPadLength = MathUtilities.RoundUp(pos, IsoUtilities.SectorSize) - pos;
            Array.Clear(buffer, offset + pos, finalPadLength);

            return pos + finalPadLength;
        }

        private static int WriteUdfMember(BuildDirectoryMember m, uint sector, string nameOverride, Encoding nameEnc, byte[] buffer, int offset, Dictionary<BuildDirectoryMember, LongAllocationDescriptor> locationTable, FileCharacteristics fileCharacteristics = FileCharacteristics.Default)
        {
            if (m is BuildDirectoryInfo)
                fileCharacteristics |= FileCharacteristics.Directory;
            FileIdentifierDescriptor fid = new FileIdentifierDescriptor(sector, m.PickName(nameOverride, nameEnc), locationTable[m], fileCharacteristics);
            fid.WriteTo(buffer, offset);
            return fid.Size;
        }

    }
}