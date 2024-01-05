using Ps2IsoTools.DiscUtils.Utils;
using Ps2IsoTools.Extensions;

namespace Ps2IsoTools.UDF.Descriptors
{
    internal class LongAllocationDescriptor : IByteArraySerializable
    {
        public uint ExtentLength;
        public LogicalBlockAddress ExtentLocation;
        public byte[] ImplementationUse = new byte[6];

        public int Size => 16;

        public LongAllocationDescriptor() { ExtentLocation = new(); }

        public LongAllocationDescriptor(uint length, uint sector, ushort partition = 0)
        {
            ExtentLength = length;
            ExtentLocation = new(sector, partition);
        }

        public int ReadFrom(byte[] buffer, int offset)
        {
            ExtentLength = EndianUtilities.ToUInt32LittleEndian(buffer, offset);
            ExtentLocation = EndianUtilities.ToStruct<LogicalBlockAddress>(buffer, offset + 4);
            ImplementationUse = buffer.Slice((offset + 10), (offset + 16));

            return Size;
        }

        public void WriteTo(byte[] buffer, int offset)
        {
            if (buffer.Length < offset + Size)
                throw new InvalidDataException("The size of the buffer was too small to write to");

            EndianUtilities.WriteBytesLittleEndian(ExtentLength, buffer, offset);
            ExtentLocation.WriteTo(buffer, offset + 4);
            Array.Copy(ImplementationUse, 0, buffer, offset + 10, ImplementationUse.Length);
        }

        public override string ToString() => $"{ExtentLocation}:+{ExtentLength:X8}";
    }
}
