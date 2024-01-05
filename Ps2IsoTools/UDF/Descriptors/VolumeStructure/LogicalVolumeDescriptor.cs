using Ps2IsoTools.DiscUtils.Utils;
using Ps2IsoTools.Extensions;
using Ps2IsoTools.UDF.CharacterSet;
using Ps2IsoTools.UDF.EntityIdentifiers;
using Ps2IsoTools.UDF.PartitionMaps;
using Ps2IsoTools.UDF.Strings;

namespace Ps2IsoTools.UDF.Descriptors
{
    internal class LogicalVolumeDescriptor : TaggedDescriptor<LogicalVolumeDescriptor>
    {
        internal uint VolumeDescriptorSequenceNumber;
        internal CharacterSetSpecification DescriptorCharacterSet;
        internal Dstring LogicalVolumeIdentifier;//128 bytes
        internal uint LogicalBlockSize;
        internal EntityIdentifier DomainIdentifier;
        internal byte[] LogicalVolumeContentsUse = new byte[16];
        internal uint MapTableLength;
        internal uint NumberofPartitionMaps;
        internal EntityIdentifier ImplementationIdentifier;
        internal byte[] ImplementationUse = new byte[128];
        internal ExtentDescriptor IntegritySequenceExtent;
        internal PartitionMap[] PartitionMaps;

        public override int Size => 440 + (int)MapTableLength;

        public LongAllocationDescriptor FileSetDescriptorLocation
        {
            get => EndianUtilities.ToStruct<LongAllocationDescriptor>(LogicalVolumeContentsUse, 0);
            set => value.WriteTo(LogicalVolumeContentsUse, 0);
        }

        public LogicalVolumeDescriptor() : base(TagIdentifier.LogicalVolumeDescriptor) { }

        public LogicalVolumeDescriptor(uint sector, uint descriptorSequenceNumber, string volumeIdentifier, LongAllocationDescriptor fileSetDescriptorLocation, ExtentDescriptor integrityDescriptorExtent, PartitionMap[] partitionMaps, uint blockSize = 0x800) : base(TagIdentifier.LogicalVolumeDescriptor)
        {
            Tag = new DescriptorTag()
            {
                TagIdentifier = TagIdentifier.LogicalVolumeDescriptor,
                DescriptorCRCLength = (ushort)(Size - 16),
                TagLocation = sector,
            };
            VolumeDescriptorSequenceNumber = descriptorSequenceNumber;
            DescriptorCharacterSet = CommonCharacterSets.OSTACompressedUnicode;
            LogicalVolumeIdentifier = new Dstring(volumeIdentifier, CompressionID.UTF8, 128);
            LogicalBlockSize = blockSize;
            DomainIdentifier = CommonIdentifiers.OSTAUDFCompliant;
            FileSetDescriptorLocation = fileSetDescriptorLocation;
            IntegritySequenceExtent = integrityDescriptorExtent;
            NumberofPartitionMaps = (uint)partitionMaps.Length;
            ImplementationIdentifier = CommonIdentifiers.Ps2IsoToolsImplementation;
            PartitionMaps = partitionMaps;
            MapTableLength = (uint)PartitionMaps.Select(pm => pm.Size).Sum();
            FixChecksums();
        }

        public override int Parse(byte[] buffer, int offset)
        {
            VolumeDescriptorSequenceNumber = EndianUtilities.ToUInt32LittleEndian(buffer, offset + 16);
            DescriptorCharacterSet = EndianUtilities.ToStruct<CharacterSetSpecification>(buffer, offset + 20);
            LogicalVolumeIdentifier = Dstring.FromBytes(buffer, offset + 84, 128);
            LogicalBlockSize = EndianUtilities.ToUInt32LittleEndian(buffer, offset + 212);
            DomainIdentifier = EndianUtilities.ToStruct<DomainEntityIdentifier>(buffer, offset + 216);
            LogicalVolumeContentsUse = buffer.Slice((offset + 248), (offset + 264));
            MapTableLength = EndianUtilities.ToUInt32LittleEndian(buffer, offset + 264);
            NumberofPartitionMaps = EndianUtilities.ToUInt32LittleEndian(buffer, offset + 268);
            ImplementationIdentifier = EndianUtilities.ToStruct<ImplementationEntityIdentifier>(buffer, offset + 272);
            ImplementationUse = buffer.Slice((offset + 304), (offset + 432));
            IntegritySequenceExtent = EndianUtilities.ToStruct<ExtentDescriptor>(buffer, offset + 432);

            int pmOffset = 0;
            PartitionMaps = new PartitionMap[NumberofPartitionMaps];
            for (int i = 0; i < NumberofPartitionMaps; i++)
            {
                PartitionMaps[i] = PartitionMap.CreateFrom(buffer, offset + 440 + pmOffset);
                pmOffset += PartitionMaps[i].Size;
            }

            return Size;
        }

        public override void Write(byte[] buffer, int offset)
        {
            EndianUtilities.WriteBytesLittleEndian(VolumeDescriptorSequenceNumber, buffer, offset + 16);
            DescriptorCharacterSet.WriteTo(buffer, offset + 20);
            LogicalVolumeIdentifier.WriteTo(buffer, offset + 84);
            EndianUtilities.WriteBytesLittleEndian(LogicalBlockSize, buffer, offset + 212);
            DomainIdentifier.WriteTo(buffer, offset + 216);
            //Array.Copy(LogicalVolumeContentsUse, 0, buffer, offset + 248, LogicalVolumeContentsUse.Length);
            FileSetDescriptorLocation.WriteTo(buffer, offset + 248);
            EndianUtilities.WriteBytesLittleEndian(MapTableLength, buffer, offset + 264);
            EndianUtilities.WriteBytesLittleEndian(NumberofPartitionMaps, buffer, offset + 268);
            ImplementationIdentifier.WriteTo(buffer, offset + 272);
            Array.Copy(ImplementationUse, 0, buffer, offset + 304, ImplementationUse.Length);
            IntegritySequenceExtent.WriteTo(buffer, offset + 432);
            int pmOffset = 0;
            for (int i = 0; i < PartitionMaps.Length; i++)
            {
                PartitionMaps[i].WriteTo(buffer, offset + 440 + pmOffset);
                pmOffset += PartitionMaps[i].Size;
            }
        }
    }
}
