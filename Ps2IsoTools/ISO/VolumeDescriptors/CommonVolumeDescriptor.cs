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

using System.Text;
using Ps2IsoTools.DiscUtils.Utils;
using Ps2IsoTools.Extensions;
using Ps2IsoTools.ISO.Builders;
using Ps2IsoTools.ISO.Files;

namespace Ps2IsoTools.ISO.VolumeDescriptors
{
    internal class CommonVolumeDescriptor : BaseVolumeDescriptor
    {
        public string SystemIdentifier;//32 bytes
        public string VolumeIdentifier;//32 bytes
        public uint VolumeSize;//, VolumeSizeBigEnd;
        public ushort VolumeSetSize;//, VolumeSetSizeBigEnd;
        public ushort VolumeSequenceNumber;//, VolumeSequenceNumberBigEnd;
        public ushort LogicalBlockSize;//, LogicalBlockSizeBigEnd;
        public uint PathTableSize;//, PathTableSizeBigEnd;
        public uint TypeLPathTableLocation, TypeLOptionalPathTableLocation;
        public uint TypeMPathTableLocation, TypeMOptionalPathTableLocation;
        public DirectoryRecord RootDirectory;
        public string VolumeSetIdentifier;//128 bytes
        public string PublisherIdentifier;//128 bytes
        public string DataPreparerIdentifier;//128 bytes
        public string ApplicationIdentifier;//128 bytes
        public string CopyrightFileIdentifier;//37 bytes
        public string AbstractFileIdentifier;//37 bytes
        public string BibliographicFileIdentifier;//37 bytes
        public DateTime CreationDateAndTime;
        public DateTime ModificationDateAndTime;
        public DateTime ExpirationDateAndTime;
        public DateTime EffectiveDateAndTime;
        public byte FileStructureVersion;

        public Encoding Encoding;

        public CommonVolumeDescriptor(byte[] buffer, int offset, Encoding enc) : base(buffer, offset)
        {
            Encoding = enc;
            RootDirectory = new();

            SystemIdentifier = enc.GetString(buffer.Slice((offset + 8), (offset + 40))).TrimEnd();
            VolumeIdentifier = enc.GetString(buffer.Slice((offset + 40), (offset + 72))).TrimEnd();
            //72 - 79 Unused
            VolumeSize = EndianUtilities.ToUInt32LittleEndian(buffer, offset + 80);
            //88 - 119 Unused
            VolumeSetSize = EndianUtilities.ToUInt16LittleEndian(buffer, offset + 120);
            VolumeSequenceNumber = EndianUtilities.ToUInt16LittleEndian(buffer, offset + 124);
            LogicalBlockSize = EndianUtilities.ToUInt16LittleEndian(buffer, offset + 128);
            PathTableSize = EndianUtilities.ToUInt32LittleEndian(buffer, offset + 132);
            TypeLPathTableLocation = EndianUtilities.ToUInt32LittleEndian(buffer, offset + 140);
            TypeLOptionalPathTableLocation = EndianUtilities.ToUInt32LittleEndian(buffer, offset + 144);
            TypeMPathTableLocation = EndianUtilities.ToUInt32BigEndian(buffer, offset + 148);
            TypeMOptionalPathTableLocation = EndianUtilities.ToUInt32BigEndian(buffer, offset + 152);
            //RootDirectory.ReadFrom(buffer, offset + 156); //RootDirectory used to be a DirectoryEntry
            DirectoryRecord.ReadFrom(buffer, offset + 156, Encoding, out RootDirectory);
            VolumeSetIdentifier = enc.GetString(buffer.Slice((offset + 190), (offset + 318))).TrimEnd();
            PublisherIdentifier = enc.GetString(buffer.Slice((offset + 318), (offset + 446))).TrimEnd();
            DataPreparerIdentifier = enc.GetString(buffer.Slice((offset + 446), (offset + 574))).TrimEnd();
            ApplicationIdentifier = enc.GetString(buffer.Slice((offset + 574), (offset + 702))).TrimEnd();
            CopyrightFileIdentifier = enc.GetString(buffer.Slice((offset + 702), (offset + 739))).TrimEnd();
            AbstractFileIdentifier = enc.GetString(buffer.Slice((offset + 739), (offset + 776))).TrimEnd();
            BibliographicFileIdentifier = enc.GetString(buffer.Slice((offset + 776), (offset + 813))).TrimEnd();
            CreationDateAndTime = IsoUtilities.ToDateTimeFromVolumeDescriptorTime(buffer, offset + 813);
            ModificationDateAndTime = IsoUtilities.ToDateTimeFromVolumeDescriptorTime(buffer, offset + 830);
            ExpirationDateAndTime = IsoUtilities.ToDateTimeFromVolumeDescriptorTime(buffer, offset + 847);
            EffectiveDateAndTime = IsoUtilities.ToDateTimeFromVolumeDescriptorTime(buffer, offset + 864);
            FileStructureVersion = buffer[offset + 881];
            //882 Unused
            //883 - 1394 Application Reserved
            //1395 - 2047 ISO Reserved
        }

        public CommonVolumeDescriptor(
            VolumeDescriptorType type,
            byte version,
            uint volumeSpaceSize,
            uint pathTableSize,
            uint typeLPathTableLocation,
            uint typeMPathTableLocation,
            uint rootDirExtentLocation,
            uint rootDirDataLength,
            DateTime buildTime,
            Encoding enc)
            : base(type, version)
        {
            Encoding = enc;

            SystemIdentifier = string.Empty;
            VolumeIdentifier = string.Empty;
            VolumeSize = volumeSpaceSize;
            VolumeSetSize = 1;
            VolumeSequenceNumber = 1;
            LogicalBlockSize = IsoUtilities.SectorSize;
            PathTableSize = pathTableSize;
            TypeLPathTableLocation = typeLPathTableLocation;
            ////OptionalTypeLPathTableLocation = 0;
            TypeMPathTableLocation = typeMPathTableLocation;
            ////OptionalTypeMPathTableLocation = 0;
            RootDirectory = new DirectoryRecord();
            RootDirectory.ExtendedAttributeRecordLength = 0;
            RootDirectory.LocationOfExtent = rootDirExtentLocation;
            RootDirectory.DataLength = rootDirDataLength;
            RootDirectory.RecordingDateAndTime = buildTime;
            RootDirectory.Flags = FileFlags.Directory;
            RootDirectory.FileUnitSize = 0;
            RootDirectory.InterleaveGapSize = 0;
            RootDirectory.VolumeSequenceNumber = 1;
            RootDirectory.FileIdentifier = "\0";
            VolumeSetIdentifier = string.Empty;
            PublisherIdentifier = string.Empty;
            DataPreparerIdentifier = string.Empty;
            ApplicationIdentifier = string.Empty;
            CopyrightFileIdentifier = string.Empty;
            AbstractFileIdentifier = string.Empty;
            BibliographicFileIdentifier = string.Empty;
            CreationDateAndTime = buildTime;
            ModificationDateAndTime = buildTime;
            ExpirationDateAndTime = DateTime.MinValue;
            EffectiveDateAndTime = buildTime;
            FileStructureVersion = 1; // V1
        }

        internal virtual void WriteTo(byte[] buffer, int offset)
        {
            Array.Clear(buffer, offset, IsoUtilities.SectorSize);
            buffer[offset] = (byte)VolumeDescriptorType;
            IsoUtilities.WriteAChars(buffer, offset + 1, 5, Identifier);
            buffer[offset + 6] = VolumeDescriptorVersion;
        }
    }
}
