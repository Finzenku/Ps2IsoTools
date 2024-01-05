using Ps2IsoTools.DiscUtils.Utils;
using Ps2IsoTools.Extensions;
using Ps2IsoTools.UDF.Descriptors.FileStructure.ExtendedAttribute;
using Ps2IsoTools.UDF.EntityIdentifiers;
using Ps2IsoTools.UDF.Time;

namespace Ps2IsoTools.UDF.Descriptors.FileStructure
{
    internal class FileEntry : TaggedDescriptor<FileEntry>
    {
        internal InformationControlBlockTag InformationControlBlockTag;
        internal uint Uid;
        internal uint Gid;
        internal FilePermissions Permissions;
        internal ushort FileLinkCount;
        internal byte RecordFormat;
        internal byte RecordDisplayAttributes;
        internal uint RecordLength;
        internal ulong InformationLength;
        internal ulong LogicalBlocksRecorded;
        internal TimeStamp AccessTime;
        internal TimeStamp ModificationTime;
        internal TimeStamp AttributeTime;
        internal uint Checkpoint;
        internal LongAllocationDescriptor ExtendedAttributeICB;
        internal EntityIdentifier ImplementationIdentifier;
        internal ulong UniqueID;
        internal uint LengthofExtendedAttributes;
        internal uint LengthofAllocationDescriptors;
        internal ExtendedAttributes ExtendedAttributes { get; set; }
        internal List<ExtendedAttributeRecord> ExtendedAttributesList => ExtendedAttributes.Attributes;
        internal byte[] AllocationDescriptors;

        public override int Size => (int)(176 + LengthofExtendedAttributes + LengthofAllocationDescriptors);

        public FileEntry() : base(TagIdentifier.FileEntry) { }
        public FileEntry(TagIdentifier tag) : base(tag) { }

        public FileEntry(uint sector, FileType fileType, ulong fileSize, ulong fileBlockSize, ulong uniqueId, byte[] allocationDescriptors) : base(TagIdentifier.FileEntry)
        {
            Tag = new DescriptorTag()
            {
                TagIdentifier = TagIdentifier.FileEntry,
                DescriptorCRCLength = (ushort)(Size - 16),
                TagLocation = sector
            };
            InformationControlBlockTag = new InformationControlBlockTag(fileType);
            Uid = uint.MaxValue;
            Gid = uint.MaxValue;
            Permissions = FilePermissions.OthersExecute | FilePermissions.OthersRead |
                          FilePermissions.GroupExecute | FilePermissions.GroupRead |
                          FilePermissions.OwnerExecute | FilePermissions.OwnerRead;
            FileLinkCount = 1;
            RecordFormat = 0;
            RecordDisplayAttributes = 0;
            RecordLength = 0;
            InformationLength = fileSize;
            LogicalBlocksRecorded = fileBlockSize;
            DateTime utcNow = DateTime.UtcNow;
            AccessTime = TimeStamp.FromDateTime(utcNow);
            ModificationTime = TimeStamp.FromDateTime(utcNow);
            AttributeTime = TimeStamp.FromDateTime(utcNow);
            Checkpoint = 1;
            ExtendedAttributeICB = new LongAllocationDescriptor();
            ImplementationIdentifier = CommonIdentifiers.Ps2IsoToolsImplementation;
            UniqueID = uniqueId;
            ExtendedAttributes = new ExtendedAttributes(sector);
            LengthofExtendedAttributes = (uint)ExtendedAttributes.Size;
            AllocationDescriptors = allocationDescriptors;
            LengthofAllocationDescriptors = (uint)AllocationDescriptors.Length;
        }

        public FileEntry(uint sector, FileType fileType, ulong uniqueId, ShortAllocationDescriptor allocationDescriptor) : base(TagIdentifier.FileEntry)
        {
            Tag = new DescriptorTag()
            {
                TagIdentifier = TagIdentifier.FileEntry,
                DescriptorCRCLength = (ushort)(Size - 16),
                TagLocation = sector
            };
            InformationControlBlockTag = new InformationControlBlockTag(fileType);
            Uid = uint.MaxValue;
            Gid = uint.MaxValue;
            Permissions = FilePermissions.OthersExecute | FilePermissions.OthersRead |
                          FilePermissions.GroupExecute | FilePermissions.GroupRead |
                          FilePermissions.OwnerExecute | FilePermissions.OwnerRead;
            FileLinkCount = 1;
            RecordFormat = 0;
            RecordDisplayAttributes = 0;
            RecordLength = 0;
            InformationLength = allocationDescriptor.ExtentLength;
            LogicalBlocksRecorded = (ulong)MathUtilities.RoundUp((int)InformationLength, IsoUtilities.SectorSize) / IsoUtilities.SectorSize;
            DateTime utcNow = DateTime.UtcNow;
            AccessTime = TimeStamp.FromDateTime(utcNow);
            ModificationTime = TimeStamp.FromDateTime(utcNow);
            AttributeTime = TimeStamp.FromDateTime(utcNow);
            Checkpoint = 1;
            ExtendedAttributeICB = new LongAllocationDescriptor();
            ImplementationIdentifier = CommonIdentifiers.Ps2IsoToolsImplementation;
            UniqueID = uniqueId;
            ExtendedAttributes = new ExtendedAttributes(sector);
            LengthofExtendedAttributes = (uint)ExtendedAttributes.Size;
            AllocationDescriptors = new byte[allocationDescriptor.Size];
            allocationDescriptor.WriteTo(AllocationDescriptors, 0);
            LengthofAllocationDescriptors = (uint)AllocationDescriptors.Length;

            FixChecksums();
        }

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
            LogicalBlocksRecorded = EndianUtilities.ToUInt64LittleEndian(buffer, offset + 64);
            AccessTime = EndianUtilities.ToStruct<TimeStamp>(buffer, offset + 72);
            ModificationTime = EndianUtilities.ToStruct<TimeStamp>(buffer, offset + 84);
            AttributeTime = EndianUtilities.ToStruct<TimeStamp>(buffer, offset + 96);
            Checkpoint = EndianUtilities.ToUInt32LittleEndian(buffer, offset + 108);
            ExtendedAttributeICB = EndianUtilities.ToStruct<LongAllocationDescriptor>(buffer, offset + 112);
            ImplementationIdentifier = EndianUtilities.ToStruct<ImplementationEntityIdentifier>(buffer, offset + 128);
            UniqueID = EndianUtilities.ToUInt64LittleEndian(buffer, offset + 160);
            LengthofExtendedAttributes = EndianUtilities.ToUInt32LittleEndian(buffer, offset + 168);
            LengthofAllocationDescriptors = EndianUtilities.ToUInt32LittleEndian(buffer, offset + 172);
            ExtendedAttributes = new((int)LengthofExtendedAttributes);
            ExtendedAttributes.ReadFrom(buffer, offset + 176);
            //ExtendedAttributesList = ReadExtendedAttributes(buffer.Slice((offset + 176), (offset + 176 + (int)LengthofExtendedAttributes)));
            AllocationDescriptors = buffer.Slice((offset + 176 + (int)LengthofExtendedAttributes), (offset + 176 + (int)LengthofExtendedAttributes + (int)LengthofAllocationDescriptors));

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
            EndianUtilities.WriteBytesLittleEndian(LogicalBlocksRecorded, buffer, offset + 64);
            AccessTime.WriteTo(buffer, offset + 72);
            ModificationTime.WriteTo(buffer, offset + 84);
            AttributeTime.WriteTo(buffer, offset + 96);
            EndianUtilities.WriteBytesLittleEndian(Checkpoint, buffer, offset + 108);
            ExtendedAttributeICB.WriteTo(buffer, offset + 112);
            ImplementationIdentifier.WriteTo(buffer, offset + 128);
            EndianUtilities.WriteBytesLittleEndian(UniqueID, buffer, offset + 160);
            EndianUtilities.WriteBytesLittleEndian(LengthofExtendedAttributes, buffer, offset + 168);
            EndianUtilities.WriteBytesLittleEndian(LengthofAllocationDescriptors, buffer, offset + 172);

            ExtendedAttributes.WriteTo(buffer, offset + 176);

            Array.Copy(AllocationDescriptors, 0, buffer, offset + 176 + ExtendedAttributes.Size, AllocationDescriptors.Length);
        }
    }
}
