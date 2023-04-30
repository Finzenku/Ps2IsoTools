using Ps2IsoTools.DiscUtils.Utils;

namespace Ps2IsoTools.UDF.Descriptors
{
    internal abstract class BaseTaggedDescriptor : IByteArraySerializable
    {
        public readonly TagIdentifier RequiredTagIdentifier;
        public DescriptorTag Tag;

        public virtual int Size => 2048;

        protected BaseTaggedDescriptor(TagIdentifier id)
        {
            RequiredTagIdentifier = id;
        }

        public int ReadFrom(byte[] buffer, int offset)
        {
            if (!DescriptorTag.IsValid(buffer, offset))
                throw new InvalidDataException("Invalid Descriptor Tag");
            Tag = EndianUtilities.ToStruct<DescriptorTag>(buffer, offset);
            if (UdfUtilities.ComputeCrc(buffer, offset + Tag.Size, Tag.DescriptorCRCLength) != Tag.DescriptorCRC)
                throw new InvalidDataException($"Invalid Descriptor Tag (Invalid CRC) Calculated: {UdfUtilities.ComputeCrc(buffer, offset + Tag.Size, Tag.DescriptorCRCLength):X4}, Expected: {Tag.DescriptorCRC:X4}");

            return Parse(buffer, offset);
        }

        public void WriteTo(byte[] buffer, int offset)
        {
            if (buffer.Length < offset + Size)
                throw new InvalidDataException("The size of the buffer was too small to write to");

            Tag.WriteTo(buffer, offset);
            Write(buffer, offset);
        }

        public void FixChecksums()
        {
            Tag.DescriptorCRCLength = (ushort)(Size - 16);
            byte[] buffer = new byte[Tag.DescriptorCRCLength];
            Write(buffer, -16);
            Tag.DescriptorCRC = UdfUtilities.ComputeCrc(buffer, 0, Tag.DescriptorCRCLength);

            byte[] tagBuffer = new byte[Tag.Size];
            Tag.WriteTo(tagBuffer, 0);
            byte checkSum = 0;
            for (int i = 0; i < 4; i++)
                checkSum += tagBuffer[i];
            for (int i = 5; i < 16; i++)
                checkSum += tagBuffer[i];
            Tag.TagChecksum = checkSum;
        }

        public abstract int Parse(byte[] buffer, int offset);
        public abstract void Write(byte[] buffer, int offset);
    }
}
