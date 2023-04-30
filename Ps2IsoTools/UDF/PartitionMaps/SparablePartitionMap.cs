using Ps2IsoTools.DiscUtils.Utils;

namespace Ps2IsoTools.UDF.PartitionMaps
{
    internal sealed class SparablePartitionMap : PartitionMap
    {
        public ushort VolumeSequenceNumber;
        public ushort PartitionNumber;
        public ushort PacketLength;
        public byte NumSparingTables;
        public uint SparingTableSize;
        public uint[] LocationsOfSparingTables;

        public override int Size => 64;

        protected override int Parse(byte[] buffer, int offset)
        {
            VolumeSequenceNumber = EndianUtilities.ToUInt16LittleEndian(buffer, offset + 36);
            PartitionNumber = EndianUtilities.ToUInt16LittleEndian(buffer, offset + 38);
            PacketLength = EndianUtilities.ToUInt16LittleEndian(buffer, offset + 40);
            NumSparingTables = buffer[offset + 42];
            SparingTableSize = EndianUtilities.ToUInt32LittleEndian(buffer, offset + 44);
            LocationsOfSparingTables = new uint[NumSparingTables];
            for (int i = 0; i < NumSparingTables; i++)
                LocationsOfSparingTables[i] = EndianUtilities.ToUInt32LittleEndian(buffer, offset + 48 + (4 * i));

            return Size;
        }

        protected override void Write(byte[] buffer, int offset)
        {
            EndianUtilities.WriteBytesLittleEndian(VolumeSequenceNumber, buffer, offset + 36);
            EndianUtilities.WriteBytesLittleEndian(PartitionNumber, buffer, offset + 38);
            EndianUtilities.WriteBytesLittleEndian(PacketLength, buffer, offset + 40);
            buffer[offset + 42] = NumSparingTables;
            EndianUtilities.WriteBytesLittleEndian(SparingTableSize, buffer, offset + 44);
            for (int i = 0; i < NumSparingTables; i++)
                EndianUtilities.WriteBytesLittleEndian(LocationsOfSparingTables[i], buffer, offset + 48 + (4 * i));
        }
    }
}
