//
// Copyright (c) 2008-2011, Kenneth Bell
//
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.
//

using Ps2IsoTools.DiscUtils.Utils;

namespace Ps2IsoTools.UDF.Descriptors
{
    internal class DescriptorTag : IByteArraySerializable
    {
        public TagIdentifier TagIdentifier;
        public ushort DescriptorVersion;
        public byte TagChecksum;
        public byte Reserved;
        public ushort TagSerialNumber;
        public ushort DescriptorCRC;
        public ushort DescriptorCRCLength;
        public uint TagLocation;

        public int Size => 16;

        public DescriptorTag() 
        {
            DescriptorVersion = 2;
        }

        public int ReadFrom(byte[] buffer, int offset)
        {
            TagIdentifier = (TagIdentifier)EndianUtilities.ToUInt16LittleEndian(buffer, offset);
            DescriptorVersion = EndianUtilities.ToUInt16LittleEndian(buffer, offset + 2);
            TagChecksum = buffer[offset + 4];
            Reserved = buffer[offset + 5];
            TagSerialNumber = EndianUtilities.ToUInt16LittleEndian(buffer, offset + 6);
            DescriptorCRC = EndianUtilities.ToUInt16LittleEndian(buffer, offset + 8);
            DescriptorCRCLength = EndianUtilities.ToUInt16LittleEndian(buffer, offset + 10);
            TagLocation = EndianUtilities.ToUInt32LittleEndian(buffer, offset + 12);

            return Size;
        }

        public void WriteTo(byte[] buffer, int offset)
        {
            if (buffer.Length < offset + Size)
                throw new InvalidDataException("The size of the buffer was too small to write to");

            EndianUtilities.WriteBytesLittleEndian((ushort)TagIdentifier, buffer, offset);
            EndianUtilities.WriteBytesLittleEndian(DescriptorVersion, buffer, offset + 2);
            buffer[offset + 4] = TagChecksum;
            buffer[offset + 5] = Reserved;
            EndianUtilities.WriteBytesLittleEndian(TagSerialNumber, buffer, offset + 6);
            EndianUtilities.WriteBytesLittleEndian(DescriptorCRC, buffer, offset + 8);
            EndianUtilities.WriteBytesLittleEndian(DescriptorCRCLength, buffer, offset + 10);
            EndianUtilities.WriteBytesLittleEndian(TagLocation, buffer, offset + 12);
        }

        public static bool IsValid(byte[] buffer, int offset)
        {
            byte checkSum = 0;

            if (EndianUtilities.ToUInt16LittleEndian(buffer, offset) == 0)
            {
                return false;
            }

            for (int i = 0; i < 4; ++i)
            {
                checkSum += buffer[offset + i];
            }

            for (int i = 5; i < 16; ++i)
            {
                checkSum += buffer[offset + i];
            }

            return checkSum == buffer[offset + 4];
        }
        public static bool TryFromStream(Stream stream, out DescriptorTag result)
        {
            byte[] next = StreamUtilities.ReadExact(stream, 512);
            if (!IsValid(next, 0))
            {
                result = null;
                return false;
            }

            DescriptorTag dt = new DescriptorTag();
            dt.ReadFrom(next, 0);

            result = dt;
            return true;
        }
    }
}
