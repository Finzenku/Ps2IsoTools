using Ps2IsoTools.DiscUtils.Utils;

namespace Ps2IsoTools.UDF.PartitionMaps
{
    internal sealed class MetadataPartitionMap : PartitionMap
    {
        public ushort VolumeSequenceNumber;
        public ushort PartitionNumber;
        public uint MetadataFileLocation;
        public uint MetadataMirrorFileLocation;
        public uint MetadataBitmapFileLocation;
        public uint AllocationUnitSize;
        public ushort AlightmentUnitSize;
        public byte Flags;

        public override int Size => 64;

        protected override int Parse(byte[] buffer, int offset)
        {
            VolumeSequenceNumber = EndianUtilities.ToUInt16LittleEndian(buffer, offset + 36);
            PartitionNumber = EndianUtilities.ToUInt16LittleEndian(buffer, offset + 38);
            MetadataFileLocation = EndianUtilities.ToUInt32LittleEndian(buffer, offset + 40);
            MetadataMirrorFileLocation = EndianUtilities.ToUInt32LittleEndian(buffer, offset + 44);
            MetadataBitmapFileLocation = EndianUtilities.ToUInt32LittleEndian(buffer, offset + 48);
            AllocationUnitSize = EndianUtilities.ToUInt32LittleEndian(buffer, offset + 52);
            AlightmentUnitSize = EndianUtilities.ToUInt16LittleEndian(buffer, offset + 56);
            Flags = buffer[offset + 58];

            return Size;
        }

        protected override void Write(byte[] buffer, int offset)
        {
            EndianUtilities.WriteBytesLittleEndian(VolumeSequenceNumber, buffer, offset + 36);
            EndianUtilities.WriteBytesLittleEndian(PartitionNumber, buffer, offset + 38);
            EndianUtilities.WriteBytesLittleEndian(MetadataFileLocation, buffer, offset + 40);
            EndianUtilities.WriteBytesLittleEndian(MetadataMirrorFileLocation, buffer, offset + 44);
            EndianUtilities.WriteBytesLittleEndian(MetadataBitmapFileLocation, buffer, offset + 48);
            EndianUtilities.WriteBytesLittleEndian(AllocationUnitSize, buffer, offset + 52);
            EndianUtilities.WriteBytesLittleEndian(AlightmentUnitSize, buffer, offset + 56);
            buffer[offset + 58] = Flags;
        }
    }
}
