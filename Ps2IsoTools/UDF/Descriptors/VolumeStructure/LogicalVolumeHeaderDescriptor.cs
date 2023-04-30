using Ps2IsoTools.DiscUtils.Utils;

namespace Ps2IsoTools.UDF.Descriptors.VolumeStructure
{
    internal class LogicalVolumeHeaderDescriptor : IByteArraySerializable
    {
        public ulong UniqueID { get; set; }
        public byte[] Reserved { get; set; } = new byte[24];
        public int Size => 32;

        public LogicalVolumeHeaderDescriptor()
        {
            UniqueID = 0xFFFFFFFF;
        }

        public int ReadFrom(byte[] buffer, int offset)
        {
            UniqueID = EndianUtilities.ToUInt64LittleEndian(buffer, offset);
            Reserved = EndianUtilities.ToByteArray(buffer, offset + 8, 24);

            return Size;
        }

        public void WriteTo(byte[] buffer, int offset)
        {
            if (buffer.Length < offset + Size)
                throw new InvalidDataException("The size of the buffer was too small to write to");

            EndianUtilities.WriteBytesLittleEndian(UniqueID, buffer, offset);
            Array.Copy(Reserved, 0, buffer, offset + 8, Reserved.Length);
        }
    }
}
