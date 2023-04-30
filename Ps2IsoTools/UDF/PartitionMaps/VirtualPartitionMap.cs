using Ps2IsoTools.DiscUtils.Utils;

namespace Ps2IsoTools.UDF.PartitionMaps
{
    internal sealed class VirtualPartitionMap : PartitionMap
    {
        public ushort VolumeSequenceNumber;
        public ushort PartitionNumber;
        public override int Size => 64;

        protected override int Parse(byte[] buffer, int offset)
        {
            VolumeSequenceNumber = EndianUtilities.ToUInt16LittleEndian(buffer, offset + 36);
            PartitionNumber = EndianUtilities.ToUInt16LittleEndian(buffer, offset + 38);

            return Size;
        }

        protected override void Write(byte[] buffer, int offset)
        {
            EndianUtilities.WriteBytesLittleEndian(VolumeSequenceNumber, buffer, offset + 36);
            EndianUtilities.WriteBytesLittleEndian(PartitionNumber, buffer, offset + 38);
        }
    }
}
