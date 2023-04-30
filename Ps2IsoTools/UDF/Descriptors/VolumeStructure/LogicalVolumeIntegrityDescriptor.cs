using Ps2IsoTools.DiscUtils.Utils;
using Ps2IsoTools.UDF.Time;

namespace Ps2IsoTools.UDF.Descriptors.VolumeStructure
{
    internal class LogicalVolumeIntegrityDescriptor : BaseTaggedDescriptor
    {
        public TimeStamp RecordingTimeStamp { get; set; }
        public IntegrityType IntegrityType { get; set; }
        public ExtentDescriptor NextIntegrityExtent { get; set; }
        public LogicalVolumeHeaderDescriptor LogicalVolumeHeader { get; set; }
        public uint NumberOfPartitions { get; set; }
        public uint LengthOfImplementationUse { get; set; }
        public uint[] FreeSpaceTable { get; set; }
        public uint[] SizeTable { get; set; }
        public LogicalVolumeIntegrityImplementationUse ImplementationUse { get; set; }


        public LogicalVolumeIntegrityDescriptor() : base(TagIdentifier.LogicalVolumeIntegrityDescriptor) { }

        public LogicalVolumeIntegrityDescriptor(uint sector, uint numberOfPartitions, uint[] freeSpaceTable, uint[] sizeTable) : base(TagIdentifier.LogicalVolumeIntegrityDescriptor)
        {
            Tag = new DescriptorTag()
            {
                TagIdentifier = TagIdentifier.LogicalVolumeIntegrityDescriptor,
                DescriptorCRCLength = (ushort)(Size - 16),
                TagLocation = sector,
            };

            RecordingTimeStamp = TimeStamp.FromDateTime(DateTime.UtcNow);
            IntegrityType = IntegrityType.CloseIntegrityDescriptor;
            NextIntegrityExtent = new ExtentDescriptor();
            LogicalVolumeHeader = new LogicalVolumeHeaderDescriptor();
            NumberOfPartitions = numberOfPartitions;
            FreeSpaceTable = freeSpaceTable;
            SizeTable = sizeTable;
            ImplementationUse = new LogicalVolumeIntegrityImplementationUse();
            LengthOfImplementationUse = (uint)ImplementationUse.Size;

            FixChecksums();
        }

        public override int Parse(byte[] buffer, int offset)
        {
            RecordingTimeStamp = EndianUtilities.ToStruct<TimeStamp>(buffer, offset + 16);
            IntegrityType = (IntegrityType)EndianUtilities.ToUInt32LittleEndian(buffer, offset + 28);
            NextIntegrityExtent = EndianUtilities.ToStruct<ExtentDescriptor>(buffer, offset + 32);
            LogicalVolumeHeader = EndianUtilities.ToStruct<LogicalVolumeHeaderDescriptor>(buffer, offset + 40);
            NumberOfPartitions = EndianUtilities.ToUInt32LittleEndian(buffer, offset + 72);
            LengthOfImplementationUse = EndianUtilities.ToUInt32LittleEndian(buffer, offset + 76);
            FreeSpaceTable = new uint[NumberOfPartitions];
            SizeTable = new uint[NumberOfPartitions];
            for(int i = 0; i < NumberOfPartitions; i++)
            {
                FreeSpaceTable[i] = EndianUtilities.ToUInt32LittleEndian(buffer, offset + 80 + (i * 4));
                SizeTable[i] = EndianUtilities.ToUInt32LittleEndian(buffer, offset + 80 + ((int)NumberOfPartitions * 4) + (i * 4));
            }
            //Cannot use EndianUtilities.ToStruct here because the LogicalVolumeIntegrityImplementationUse doesn't know how long its self is
            ImplementationUse = new LogicalVolumeIntegrityImplementationUse(LengthOfImplementationUse);
            ImplementationUse.ReadFrom(buffer, offset + 80 + (int)NumberOfPartitions * 8);

            return Size;
        }

        public override void Write(byte[] buffer, int offset)
        {
            RecordingTimeStamp.WriteTo(buffer, offset + 16);
            EndianUtilities.WriteBytesLittleEndian((int)IntegrityType, buffer, offset + 28);
            NextIntegrityExtent.WriteTo(buffer, offset + 32);
            LogicalVolumeHeader.WriteTo(buffer, offset + 40);
            EndianUtilities.WriteBytesLittleEndian(NumberOfPartitions, buffer, offset + 72);
            EndianUtilities.WriteBytesLittleEndian(LengthOfImplementationUse, buffer, offset + 76);
            for (int i = 0; i < NumberOfPartitions; i++)
            {
                EndianUtilities.WriteBytesLittleEndian(FreeSpaceTable[i], buffer, offset + 80 + (i * 4));
                EndianUtilities.WriteBytesLittleEndian(SizeTable[i], buffer, offset + 80 + (((int)NumberOfPartitions * 4) + (i * 4)));
            }
            ImplementationUse.WriteTo(buffer, offset + 80 + (int)NumberOfPartitions * 8);
        }
    }

    internal enum IntegrityType : uint
    {
        OpenIntegrityDescriptor = 0,
        CloseIntegrityDescriptor = 1,
    }
}
