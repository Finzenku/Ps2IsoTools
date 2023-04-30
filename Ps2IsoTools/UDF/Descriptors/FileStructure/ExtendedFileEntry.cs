using Ps2IsoTools.DiscUtils.Utils;
using Ps2IsoTools.UDF.EntityIdentifiers;
using Ps2IsoTools.UDF.Time;

namespace Ps2IsoTools.UDF.Descriptors.FileStructure
{
    internal class ExtendedFileEntry : FileEntry
    {
        public TimeStamp CreationTime;
        public ulong ObjectSize;
        public LongAllocationDescriptor StreamDirectoryIcb;

        public override int Size => 216 + (int)LengthofExtendedAttributes + (int)LengthofAllocationDescriptors;
        public ExtendedFileEntry() : base(TagIdentifier.ExtendedFileEntry) { }

        public override int Parse(byte[] buffer, int offset)
        {
            InformationControlBlockTag = EndianUtilities.ToStruct<InformationControlBlockTag>(buffer, offset + 16);
            Uid = EndianUtilities.ToUInt32LittleEndian(buffer, offset + 36);
            Gid = EndianUtilities.ToUInt32LittleEndian(buffer, offset + 40);
            Permissions = (FilePermissions)EndianUtilities.ToUInt32LittleEndian(buffer, offset + 44);
            FileLinkCount = EndianUtilities.ToUInt16LittleEndian(buffer, offset + 48);
            RecordFormat = buffer[offset + 50];
            RecordDisplayAttributes = buffer[offset + 51];
            RecordLength = EndianUtilities.ToUInt16LittleEndian(buffer, offset + 52);
            InformationLength = EndianUtilities.ToUInt64LittleEndian(buffer, offset + 56);
            ObjectSize = EndianUtilities.ToUInt64LittleEndian(buffer, offset + 64);
            LogicalBlocksRecorded = EndianUtilities.ToUInt64LittleEndian(buffer, offset + 72);
            AccessTime = EndianUtilities.ToStruct<TimeStamp>(buffer, offset + 80);
            ModificationTime = EndianUtilities.ToStruct<TimeStamp>(buffer, offset + 92);
            CreationTime = EndianUtilities.ToStruct<TimeStamp>(buffer, offset + 104);
            AttributeTime = EndianUtilities.ToStruct<TimeStamp>(buffer, offset + 116);
            Checkpoint = EndianUtilities.ToUInt32LittleEndian(buffer, offset + 128);
            ExtendedAttributeICB = EndianUtilities.ToStruct<LongAllocationDescriptor>(buffer, offset + 136);
            StreamDirectoryIcb = EndianUtilities.ToStruct<LongAllocationDescriptor>(buffer, offset + 152);
            ImplementationIdentifier = EndianUtilities.ToStruct<ImplementationEntityIdentifier>(buffer, offset + 168);
            UniqueID = EndianUtilities.ToUInt64LittleEndian(buffer, offset + 200);
            LengthofExtendedAttributes = EndianUtilities.ToUInt32LittleEndian(buffer, offset + 208);
            LengthofAllocationDescriptors = EndianUtilities.ToUInt32LittleEndian(buffer, offset + 212);
            ExtendedAttributes = new((int)LengthofExtendedAttributes);
            ExtendedAttributes.ReadFrom(buffer, offset + 176);

            AllocationDescriptors = EndianUtilities.ToByteArray(buffer, offset + 216 + (int)LengthofExtendedAttributes,
                (int)LengthofAllocationDescriptors);


            return Size;
        }

        public override void Write(byte[] buffer, int offset)
        {
            InformationControlBlockTag.WriteTo(buffer, offset + 16);
            EndianUtilities.WriteBytesLittleEndian(Uid, buffer, offset + 36);
            EndianUtilities.WriteBytesLittleEndian(Gid, buffer, offset + 40);
            EndianUtilities.WriteBytesLittleEndian((uint)Permissions, buffer, offset + 44);
            EndianUtilities.WriteBytesLittleEndian(FileLinkCount, buffer, offset + 48);
            buffer[offset + 50] = RecordFormat;
            buffer[offset + 51] = RecordDisplayAttributes;
            EndianUtilities.WriteBytesLittleEndian(RecordLength, buffer, offset + 52);
            EndianUtilities.WriteBytesLittleEndian(InformationLength, buffer, offset + 56);
            EndianUtilities.WriteBytesLittleEndian(ObjectSize, buffer, offset + 64);
            EndianUtilities.WriteBytesLittleEndian(LogicalBlocksRecorded, buffer, offset + 72);
            AccessTime.WriteTo(buffer, offset + 80);
            ModificationTime.WriteTo(buffer, offset + 92);
            CreationTime.WriteTo(buffer, offset + 104);
            AttributeTime.WriteTo(buffer, offset + 116);
            EndianUtilities.WriteBytesLittleEndian(Checkpoint, buffer, offset + 128);
            ExtendedAttributeICB.WriteTo(buffer, offset + 136);
            StreamDirectoryIcb.WriteTo(buffer, offset + 152);
            ImplementationIdentifier.WriteTo(buffer, offset + 168);
            EndianUtilities.WriteBytesLittleEndian(UniqueID, buffer, offset + 200);
            EndianUtilities.WriteBytesLittleEndian(LengthofExtendedAttributes, buffer, offset + 208);
            EndianUtilities.WriteBytesLittleEndian(LengthofAllocationDescriptors, buffer, offset + 212);

            int extAtrOffset = 0;
            for (int i = 0; i < ExtendedAttributesList.Count; i++)
            {
                ExtendedAttributesList[i].WriteTo(buffer, offset + 176 + extAtrOffset);
                extAtrOffset += ExtendedAttributesList[i].Size;
            }
            Array.Copy(AllocationDescriptors, 0, buffer, offset + 176 + extAtrOffset, AllocationDescriptors.Length);
        }
    }
}
