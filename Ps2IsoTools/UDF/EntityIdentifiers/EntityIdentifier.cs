using System.Text;
using Ps2IsoTools.DiscUtils.Utils;
using Ps2IsoTools.Extensions;

namespace Ps2IsoTools.UDF.EntityIdentifiers
{
    internal abstract class EntityIdentifier : IByteArraySerializable
    {
        public byte Flags;
        public string Identifier = "";
        public byte[] Suffix = new byte[8];
        public int Size => 32;

        public int ReadFrom(byte[] buffer, int offset)
        {
            Flags = buffer[offset];
            Identifier = Encoding.ASCII.GetString(buffer.Slice((offset + 1), (offset + 24)));
            Suffix = buffer.Slice((offset + 24), (offset + 32));

            return Size;
        }

        public void WriteTo(byte[] buffer, int offset)
        {
            if (buffer.Length < offset + Size)
                throw new InvalidDataException("The size of the buffer was too small to write to");

            buffer[offset] = Flags;
            Array.Copy(Encoding.ASCII.GetBytes(Identifier), 0, buffer, offset + 1, Encoding.ASCII.GetByteCount(Identifier));
            Array.Copy(Suffix, 0, buffer, offset + 24, 8);
        }
    }
}
