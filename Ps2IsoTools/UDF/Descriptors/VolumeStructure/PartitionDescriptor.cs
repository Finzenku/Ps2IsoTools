using Ps2IsoTools.DiscUtils.Utils;
using Ps2IsoTools.Extensions;
using Ps2IsoTools.UDF.EntityIdentifiers;

namespace Ps2IsoTools.UDF.Descriptors.VolumeStructure
{
    internal class PartitionDescriptor : TaggedDescriptor<PartitionDescriptor>
    {
        public uint VolumeDescriptorSequencNumber;
        public ushort PartitionFlags;
        public ushort PartitionNumber;
        public EntityIdentifier PartitionContents;
        public PartitionHeaderDescriptor PartitionContentsUse;
        public uint AccessType;
        public uint PartitionStartingLocation;
        public uint PartitionLength;
        public EntityIdentifier ImplementationIdentifier;
        public byte[] ImplementationUse = new byte[128];
        public byte[] Reserved = new byte[156];


        public PartitionDescriptor() : base(TagIdentifier.PartitionDescriptor) { }

        public PartitionDescriptor(uint sector, uint descriptorSequenceNumber, ushort partitionNumber, uint paritionStartingSector, uint partitionLength) : base(TagIdentifier.PartitionDescriptor)
        {
            Tag = new DescriptorTag()
            {
                TagIdentifier = TagIdentifier.PartitionDescriptor,
                DescriptorCRCLength = (ushort)(Size - 16),
                TagLocation = sector,
            };
            VolumeDescriptorSequencNumber = descriptorSequenceNumber;
            PartitionFlags = 1;
            PartitionNumber = partitionNumber;
            PartitionContents = CommonIdentifiers.NSR02Identifier;
            PartitionContentsUse = new PartitionHeaderDescriptor();
            AccessType = 0;
            PartitionStartingLocation = paritionStartingSector;
            PartitionLength = partitionLength;
            ImplementationIdentifier = CommonIdentifiers.Ps2IsoToolsImplementation;
            FixChecksums();
        }
        public override int Parse(byte[] buffer, int offset)
        {
            VolumeDescriptorSequencNumber = EndianUtilities.ToUInt32LittleEndian(buffer, offset + 16);
            PartitionFlags = EndianUtilities.ToUInt16LittleEndian(buffer, offset + 20);
            PartitionNumber = EndianUtilities.ToUInt16LittleEndian(buffer, offset + 22);
            PartitionContents = EndianUtilities.ToStruct<ApplicationEntityIdentifier>(buffer, offset + 24);
            PartitionContentsUse = EndianUtilities.ToStruct<PartitionHeaderDescriptor>(buffer, offset + 56);
            AccessType = EndianUtilities.ToUInt32LittleEndian(buffer, offset + 184);
            PartitionStartingLocation = EndianUtilities.ToUInt32LittleEndian(buffer, offset + 188);
            PartitionLength = EndianUtilities.ToUInt32LittleEndian(buffer, offset + 192);
            ImplementationIdentifier = EndianUtilities.ToStruct<ImplementationEntityIdentifier>(buffer, offset + 196);
            ImplementationUse = buffer.Slice((offset + 228), (offset + 356));

            return Size;
        }

        public override void Write(byte[] buffer, int offset)
        {
            if (buffer.Length < offset + Size)
                throw new InvalidDataException("The size of the buffer was too small to write to");

            EndianUtilities.WriteBytesLittleEndian(VolumeDescriptorSequencNumber, buffer, offset + 16);
            EndianUtilities.WriteBytesLittleEndian(PartitionFlags, buffer, offset + 20);
            EndianUtilities.WriteBytesLittleEndian(PartitionNumber, buffer, offset + 22);
            PartitionContents.WriteTo(buffer, offset + 24);
            PartitionContentsUse.WriteTo(buffer, offset + 56);
            EndianUtilities.WriteBytesLittleEndian(AccessType, buffer, offset + 184);
            EndianUtilities.WriteBytesLittleEndian(PartitionStartingLocation, buffer, offset + 188);
            EndianUtilities.WriteBytesLittleEndian(PartitionLength, buffer, offset + 192);
            ImplementationIdentifier.WriteTo(buffer, offset + 196);
            Array.Copy(ImplementationUse, 0, buffer, 228, ImplementationUse.Length);
        }

        public class PartitionHeaderDescriptor : IByteArraySerializable
        {
            public ShortAllocationDescriptor UnallocatedSpaceTable;
            public ShortAllocationDescriptor UnallocatedSpaceBitmap;
            public ShortAllocationDescriptor PartitionIntegrityTable;
            public ShortAllocationDescriptor FreedSpaceTable;
            public ShortAllocationDescriptor FreedSpaceBitmap;
            public byte[] Reserved = new byte[88];

            public int Size => 128;

            public PartitionHeaderDescriptor()
            {
                UnallocatedSpaceTable = new();
                UnallocatedSpaceBitmap = new();
                PartitionIntegrityTable = new();
                FreedSpaceTable = new();
                FreedSpaceBitmap = new();
            }

            public int ReadFrom(byte[] buffer, int offset)
            {
                UnallocatedSpaceTable = EndianUtilities.ToStruct<ShortAllocationDescriptor>(buffer, offset);
                UnallocatedSpaceBitmap = EndianUtilities.ToStruct<ShortAllocationDescriptor>(buffer, offset + 8);
                PartitionIntegrityTable = EndianUtilities.ToStruct<ShortAllocationDescriptor>(buffer, offset + 16);
                FreedSpaceTable = EndianUtilities.ToStruct<ShortAllocationDescriptor>(buffer, offset + 24);
                FreedSpaceBitmap = EndianUtilities.ToStruct<ShortAllocationDescriptor>(buffer, offset + 32);
                Reserved = buffer.Slice((offset + 40), (offset + 128));

                return Size;
            }

            public void WriteTo(byte[] buffer, int offset)
            {
                if (buffer.Length < offset + Size)
                    throw new InvalidDataException("The size of the buffer was too small to write to");

                UnallocatedSpaceTable.WriteTo(buffer, offset);
                UnallocatedSpaceBitmap.WriteTo(buffer, offset + 8);
                PartitionIntegrityTable.WriteTo(buffer, offset + 16);
                FreedSpaceTable.WriteTo(buffer, offset + 24);
                FreedSpaceBitmap.WriteTo(buffer, offset + 32);
                Array.Copy(Reserved, 0, buffer, 40, Reserved.Length);
            }
        }
    }
}
