using Ps2IsoTools.DiscUtils.Utils;

namespace Ps2IsoTools.UDF.Descriptors
{
    internal class ShortAllocationDescriptor : IByteArraySerializable
    {
        public ShortAllocationFlags Flags;
        public uint ExtentLength;
        public uint ExtentLocation;

        public int Size => 8;

        public ShortAllocationDescriptor() { }

        public ShortAllocationDescriptor(uint sector, uint length, ShortAllocationFlags flags = ShortAllocationFlags.RecordedAndAllocated)
        {
            ExtentLength = length;
            ExtentLocation = sector;
            Flags = flags;
        }

        public int ReadFrom(byte[] buffer, int offset)
        {
            ExtentLength = EndianUtilities.ToUInt32LittleEndian(buffer, offset);
            ExtentLocation = EndianUtilities.ToUInt32LittleEndian(buffer, offset + 4);

            // The PS2 doesn't seem to use this flag and instead uses the entire 4 bytes of the ExtentLength for the length to allow files larger than 1GB
            Flags = ShortAllocationFlags.RecordedAndAllocated;

            return Size;
        }

        public void WriteTo(byte[] buffer, int offset)
        {
            if (buffer.Length < offset + Size)
                throw new InvalidDataException("The size of the buffer was too small to write to");

            uint len = (uint)((byte)Flags) << 30 | ExtentLength;
            EndianUtilities.WriteBytesLittleEndian(len, buffer, offset);
            EndianUtilities.WriteBytesLittleEndian(ExtentLocation, buffer, offset + 4);
        }

        public override string ToString() => $"{ExtentLocation:X8}:+{ExtentLength:X8} [{Flags}]";
    }
}
