using Ps2IsoTools.DiscUtils.Utils;
using Ps2IsoTools.Extensions;

namespace Ps2IsoTools.UDF.Descriptors.FileStructure.ExtendedAttribute
{
    internal class ExtendedAttributeRecord : IByteArraySerializable
    {
        public uint AttributeType;
        public byte AttributeSubType;
        protected uint recordLength;
        public byte[] AttributeData;


        public int Size => (int)recordLength;

        public ExtendedAttributeRecord() { AttributeData = new byte[0]; }

        public virtual int ReadFrom(byte[] buffer, int offset)
        {
            AttributeType = EndianUtilities.ToUInt32LittleEndian(buffer, offset);
            AttributeSubType = buffer[offset + 4];
            recordLength = EndianUtilities.ToUInt32LittleEndian(buffer, offset + 8);
            AttributeData = buffer.Slice((offset + 12), (offset + (int)recordLength - 1));

            return Size;
        }

        public virtual void WriteTo(byte[] buffer, int offset)
        {
            if (buffer.Length < offset + Size)
                throw new InvalidDataException("The size of the buffer was too small to write to");

            EndianUtilities.WriteBytesLittleEndian(AttributeType, buffer, offset);
            buffer[offset + 4] = AttributeSubType;
            EndianUtilities.WriteBytesLittleEndian(recordLength, buffer, offset + 8);
            Array.Copy(AttributeData, 0, buffer, 12, AttributeData.Length);
        }
    }
}
