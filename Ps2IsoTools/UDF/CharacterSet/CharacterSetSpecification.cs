using System.Text;
using Ps2IsoTools.DiscUtils.Utils;
using Ps2IsoTools.Extensions;

namespace Ps2IsoTools.UDF.CharacterSet
{
    internal class CharacterSetSpecification : IByteArraySerializable
    {
        public CharacterSetType Type;
        public byte[] Information = new byte[63];
        public string InfoString => Encoding.ASCII.GetString(Information);
        public int Size => 64;

        public CharacterSetSpecification() { }
        public CharacterSetSpecification(string specification, CharacterSetType characterSetType = CharacterSetType.CharacterSet0)
        {
            Type = characterSetType;
            Array.Copy(Encoding.ASCII.GetBytes(specification), Information, Encoding.ASCII.GetByteCount(specification));
        }

        public int ReadFrom(byte[] buffer, int offset)
        {
            Type = (CharacterSetType)buffer[offset];
            Information = buffer.Slice((offset + 1), (offset + 64));

            return Size;
        }

        public void WriteTo(byte[] buffer, int offset)
        {
            if (buffer.Length < offset + Size)
                throw new InvalidDataException("The size of the buffer was too small to write to");

            buffer[offset] = (byte)Type;
            Array.Copy(Information, 0, buffer, offset + 1, Information.Length);
        }
    }
}
