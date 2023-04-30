using Ps2IsoTools.DiscUtils.Utils;

namespace Ps2IsoTools.UDF
{
    internal class LogicalBlockAddress : IByteArraySerializable
    {
        public uint LogicalBlock;
        public ushort Partition;
        public int Size => 6;

        public LogicalBlockAddress() { }
        public LogicalBlockAddress(uint sector, ushort partition)
        {
            LogicalBlock = sector;
            Partition = partition;
        }

        public int ReadFrom(byte[] buffer, int offset)
        {
            LogicalBlock = EndianUtilities.ToUInt32LittleEndian(buffer, offset);
            Partition = EndianUtilities.ToUInt16LittleEndian(buffer, offset + 4);

            return Size;
        }

        public void WriteTo(byte[] buffer, int offset)
        {
            if (buffer.Length < offset + Size)
                throw new InvalidDataException("The size of the buffer was too small to write to");

            EndianUtilities.WriteBytesLittleEndian(LogicalBlock, buffer, offset);
            EndianUtilities.WriteBytesLittleEndian(Partition, buffer, offset + 4);
        }

        public override string ToString() => $"{LogicalBlock:X8},p{Partition:X4}";
    }
}
