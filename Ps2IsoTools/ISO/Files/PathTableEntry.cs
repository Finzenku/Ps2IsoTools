using System.Text;
using Ps2IsoTools.DiscUtils.Utils;
using Ps2IsoTools.Extensions;

namespace Ps2IsoTools.ISO.Files
{
    internal class PathTableEntry : IByteArraySerializable
    {
        public byte NameLength;
        public byte ExtendedAttributeRecordLength;
        public uint DirectoryLocation;
        public short ParentIndex;
        public string PathName = string.Empty;
        public bool LittleEndian = true;

        public byte PadLength { get => (byte)(NameLength % 2); }// == 0 ? (byte)0 : (byte)1; }
        public int Size { get => 8 + NameLength + PadLength; }

        public int ReadFrom(byte[] buffer, int offset)
        {
            NameLength = buffer[offset];
            ExtendedAttributeRecordLength = buffer[offset + 1];
            DirectoryLocation = EndianUtilities.ToUInt32LittleEndian(buffer, offset + 2);
            if (DirectoryLocation >= 0x10000)
            {
                LittleEndian = false;
                DirectoryLocation = EndianUtilities.ToUInt32BigEndian(buffer, offset + 2);
                ParentIndex = EndianUtilities.ToInt16BigEndian(buffer, offset + 6);
            }
            else
                ParentIndex = EndianUtilities.ToInt16LittleEndian(buffer, offset + 6);
            PathName = Encoding.Default.GetString(buffer.Slice((offset + 8), (offset + 8 + NameLength)));

            return Size;
        }

        public void WriteTo(byte[] buffer, int offset)
        {
            throw new NotImplementedException();
        }
    }
}
