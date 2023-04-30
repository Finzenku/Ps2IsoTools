using Ps2IsoTools.DiscUtils.Utils;

namespace Ps2IsoTools.UDF.Descriptors
{
    internal class ExtentDescriptor : IByteArraySerializable
    {
        public uint Length;
        public uint Location;

        public int Size => 8;

        public ExtentDescriptor() { }

        public ExtentDescriptor(uint length, uint location)
        {
            Length = length;
            Location = location;
        }

        public int ReadFrom(byte[] buffer, int offset)
        {
            Length = EndianUtilities.ToUInt32LittleEndian(buffer, offset);
            Location = EndianUtilities.ToUInt32LittleEndian(buffer, offset + 4);

            return Size;
        }

        public void WriteTo(byte[] buffer, int offset)
        {
            if (buffer.Length < offset + Size)
                throw new InvalidDataException("The size of the buffer was too small to write to");

            EndianUtilities.WriteBytesLittleEndian(Length, buffer, offset);
            EndianUtilities.WriteBytesLittleEndian(Location, buffer, offset + 4);
        }

        public override string ToString() => $"{Location:X8}:+{Length:X8}";
    }
}
