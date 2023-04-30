using Ps2IsoTools.DiscUtils.Utils;

namespace Ps2IsoTools.UDF.PartitionMaps
{
    internal class Type1PartitionMap : PartitionMap
    {
        public ushort VolumeSequenceNumber;
        public ushort PartitionNumber;

        public override int Size => 6;

        public Type1PartitionMap() { }

        public Type1PartitionMap(ushort volumeSequenceNumber, ushort partitionNumber)
        {
            Type = 1;
            VolumeSequenceNumber = volumeSequenceNumber;
            PartitionNumber = partitionNumber;
        }

        protected override int Parse(byte[] buffer, int offset)
        {
            VolumeSequenceNumber = EndianUtilities.ToUInt16LittleEndian(buffer, offset + 2);
            PartitionNumber = EndianUtilities.ToUInt16LittleEndian(buffer, offset + 4);

            return Size;
        }

        public override string ToString() => $"{Type:X2} {VolumeSequenceNumber:X4},p{PartitionNumber:X4}";

        protected override void Write(byte[] buffer, int offset)
        {
            EndianUtilities.WriteBytesLittleEndian(VolumeSequenceNumber, buffer, offset + 2);
            EndianUtilities.WriteBytesLittleEndian(PartitionNumber, buffer, offset + 4);
        }
    }
}