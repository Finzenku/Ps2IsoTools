using Ps2IsoTools.DiscUtils.Utils;

namespace Ps2IsoTools.UDF.Descriptors.FileStructure
{
    internal class InformationControlBlockTag : IByteArraySerializable
    {
        public uint PriorRecordedNumberofDirectEntries;
        public ushort StrategyType;
        public ushort StrategyParameter;
        public ushort MaximumNumberofEntries;
        private byte Reserved;
        public FileType FileType;
        public LogicalBlockAddress ParentICBLocation;
        public AllocationType AllocationType;
        public InformationControlBlockFlags Flags;

        public int Size => 20;

        public InformationControlBlockTag() { }

        public InformationControlBlockTag(FileType fileType)
        {
            PriorRecordedNumberofDirectEntries = 0;
            StrategyType = 4;
            StrategyParameter = 0;
            MaximumNumberofEntries = 1;
            FileType = fileType;
            ParentICBLocation = new LogicalBlockAddress();
            AllocationType = 0;
            Flags = InformationControlBlockFlags.SetUid | InformationControlBlockFlags.SetGid | InformationControlBlockFlags.System | InformationControlBlockFlags.Transformed;
        }
        public int ReadFrom(byte[] buffer, int offset)
        {
            PriorRecordedNumberofDirectEntries = EndianUtilities.ToUInt32LittleEndian(buffer, offset);
            StrategyType = EndianUtilities.ToUInt16LittleEndian(buffer, offset + 4);
            StrategyParameter = EndianUtilities.ToUInt16LittleEndian(buffer, offset + 6);
            MaximumNumberofEntries = EndianUtilities.ToUInt16LittleEndian(buffer, offset + 8);
            FileType = (FileType)buffer[offset + 11];
            ParentICBLocation = EndianUtilities.ToStruct<LogicalBlockAddress>(buffer, offset + 12);
            ushort flagsField = EndianUtilities.ToUInt16LittleEndian(buffer, offset + 18);
            AllocationType = (AllocationType)(flagsField & 0x3);
            Flags = (InformationControlBlockFlags)(flagsField & 0xFFFC);

            return Size;
        }

        public void WriteTo(byte[] buffer, int offset)
        {
            if (buffer.Length < offset + Size)
                throw new InvalidDataException("The size of the buffer was too small to write to");

            EndianUtilities.WriteBytesLittleEndian(PriorRecordedNumberofDirectEntries, buffer, offset);
            EndianUtilities.WriteBytesLittleEndian(StrategyType, buffer, offset + 4);
            EndianUtilities.WriteBytesLittleEndian(StrategyParameter, buffer, offset + 6);
            EndianUtilities.WriteBytesLittleEndian(MaximumNumberofEntries, buffer, offset + 8);
            buffer[offset + 11] = (byte)FileType;
            ParentICBLocation.WriteTo(buffer, offset + 12);
            EndianUtilities.WriteBytesLittleEndian((ushort)((ushort)AllocationType | (ushort)Flags), buffer, offset + 18);
        }
    }
}
