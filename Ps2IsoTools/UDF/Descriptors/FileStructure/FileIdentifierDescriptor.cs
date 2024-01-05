using System.Text;
using Ps2IsoTools.DiscUtils.Utils;
using Ps2IsoTools.Extensions;
using Ps2IsoTools.UDF.Strings;

namespace Ps2IsoTools.UDF.Descriptors.FileStructure
{
    internal class FileIdentifierDescriptor : TaggedDescriptor<FileIdentifierDescriptor>
    {
        public ushort FileVersionNumber;
        public FileCharacteristics FileCharacteristics;
        public byte LengthofFileIdentifier;
        public LongAllocationDescriptor InformationControlBlock;
        public ushort LengthofImplementationUse;
        public byte[] ImplementationUse;
        public Dstring FileIdentifier;
        public byte[] Padding;

        public override int Size => MathUtilities.RoundUp(38 + LengthofImplementationUse + LengthofFileIdentifier, 4);

        public FileIdentifierDescriptor() : base(TagIdentifier.FileIdentifierDescriptor)
        {
            FileVersionNumber = 1;
        }

        public FileIdentifierDescriptor(uint sector, string fileName, LongAllocationDescriptor fileLocation, FileCharacteristics fileType = FileCharacteristics.Default) : base(TagIdentifier.FileIdentifierDescriptor)
        {
            Tag = new DescriptorTag()
            {
                TagIdentifier = TagIdentifier.FileIdentifierDescriptor,
                DescriptorCRCLength = (ushort)(Size - 16),
                TagLocation = sector,
            };

            FileVersionNumber = 1;
            FileCharacteristics = fileType;
            fileName = fileName.Split(';')[0];
            FileIdentifier = new Dstring(fileName, CompressionID.UTF16);
            LengthofFileIdentifier = (byte)FileIdentifier.DataLength;
            InformationControlBlock = fileLocation;

            // I don't think I need these right now
            ImplementationUse = new byte[0];
            LengthofImplementationUse = 0;

            FixChecksums();
        }

        public override int Parse(byte[] buffer, int offset)
        {
            FileVersionNumber = EndianUtilities.ToUInt16LittleEndian(buffer, offset + 16);
            FileCharacteristics = (FileCharacteristics)buffer[offset + 18];
            LengthofFileIdentifier = buffer[offset + 19];
            InformationControlBlock = EndianUtilities.ToStruct<LongAllocationDescriptor>(buffer, offset + 20);
            LengthofImplementationUse = EndianUtilities.ToUInt16LittleEndian(buffer, offset + 36);
            ImplementationUse = buffer.Slice((offset + 38), (offset + 38 + LengthofImplementationUse));
            FileIdentifier = Dstring.FromBytes(buffer, offset + 38 + LengthofImplementationUse, LengthofFileIdentifier);

            return Size;
        }

        public override void Write(byte[] buffer, int offset)
        {
            EndianUtilities.WriteBytesLittleEndian(FileVersionNumber, buffer, offset + 16);
            buffer[offset + 18] = (byte)FileCharacteristics;
            buffer[offset + 19] = LengthofFileIdentifier;
            InformationControlBlock.WriteTo(buffer, offset + 20);
            EndianUtilities.WriteBytesLittleEndian(LengthofImplementationUse, buffer, offset + 36);
            Array.Copy(ImplementationUse, 0, buffer, offset + 38, ImplementationUse.Length);
            FileIdentifier.WriteTo(buffer, offset + 38 + LengthofImplementationUse);
        }

        public static uint GetDataLength(string fileName, Encoding enc) => (uint)MathUtilities.RoundUp(38 + Dstring.CalcLength(fileName, enc == Encoding.UTF8 ? CompressionID.UTF8 : CompressionID.UTF16), 4);
    }
}
