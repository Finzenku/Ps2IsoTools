using Ps2IsoTools.DiscUtils.Utils;
using Ps2IsoTools.UDF.EntityIdentifiers;

namespace Ps2IsoTools.UDF.PartitionMaps
{
    internal abstract class PartitionMap : IByteArraySerializable
    {
        public byte Type;
        public abstract int Size { get; }

        public int ReadFrom(byte[] buffer, int offset)
        {
            Type = buffer[offset];
            return Parse(buffer, offset);
        }

        public void WriteTo(byte[] buffer, int offset)
        {
            if (buffer.Length < offset + Size)
                throw new InvalidDataException("The size of the buffer was too small to write to");

            buffer[offset] = Type;
            buffer[offset + 1] = (byte)Size;
            Write(buffer, offset);
        }

        public static PartitionMap CreateFrom(byte[] buffer, int offset)
        {
            PartitionMap result;

            byte type = buffer[offset];
            if (type == 1)
                result = EndianUtilities.ToStruct<Type1PartitionMap>(buffer, offset);
            else if (type == 2)
            {
                EntityIdentifier id = EndianUtilities.ToStruct<UdfEntityIdentifier>(buffer, offset + 4);
                switch (id.Identifier)
                {
                    case "*UDF Virtual Partition":
                        result = EndianUtilities.ToStruct<VirtualPartitionMap>(buffer, offset);
                        break;
                    case "*UDF Sparable Partition":
                        result = EndianUtilities.ToStruct<SparablePartitionMap>(buffer, offset);
                        break;
                    case "*UDF Metadata Partition":
                        result = EndianUtilities.ToStruct<MetadataPartitionMap>(buffer, offset);
                        break;
                    default:
                        throw new InvalidDataException("Unrecognized partition map entity id: " + id);
                }
            }
            else
                throw new InvalidDataException("Unrecognized partition map type: " + type);

            return result!;
        }


        protected abstract int Parse(byte[] buffer, int offset);
        protected abstract void Write(byte[] buffer, int offset);
    }
}
