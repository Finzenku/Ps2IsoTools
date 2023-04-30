using Ps2IsoTools.DiscUtils.Utils;

namespace Ps2IsoTools.UDF.Descriptors
{
    internal class UnallocatedSpaceDescriptor : TaggedDescriptor<UnallocatedSpaceDescriptor>
    {
        public uint VolumeDescriptorSequenceNumber;
        public uint NumberofAllocationDescriptors;
        public ExtentDescriptor[] AllocationDescriptors;

        //public override int Size => (int)(24 + NumberofAllocationDescriptors*8);

        public UnallocatedSpaceDescriptor() : base(TagIdentifier.UnallocatedSpaceDescriptor) { }

        public UnallocatedSpaceDescriptor(uint sector, uint descriptorSequenceNumber, ExtentDescriptor[] allocationDescriptors) : base(TagIdentifier.UnallocatedSpaceDescriptor)
        {
            Tag = new DescriptorTag()
            {
                TagIdentifier = TagIdentifier.UnallocatedSpaceDescriptor,
                DescriptorCRCLength = (ushort)(Size - 16),
                TagLocation = sector,
            };
            VolumeDescriptorSequenceNumber = descriptorSequenceNumber;
            NumberofAllocationDescriptors = (uint)allocationDescriptors.Length;
            AllocationDescriptors = allocationDescriptors;

            FixChecksums();
        }

        public override int Parse(byte[] buffer, int offset)
        {
            VolumeDescriptorSequenceNumber = EndianUtilities.ToUInt32LittleEndian(buffer, offset + 16);
            NumberofAllocationDescriptors = EndianUtilities.ToUInt32LittleEndian(buffer, offset + 20);
            AllocationDescriptors = new ExtentDescriptor[NumberofAllocationDescriptors];
            for (int i = 0; i < NumberofAllocationDescriptors; i++)
            {
                AllocationDescriptors[i] = EndianUtilities.ToStruct<ExtentDescriptor>(buffer, offset + 24 + (i * 8));
            }

            return Size;
        }

        public override void Write(byte[] buffer, int offset)
        {
            EndianUtilities.WriteBytesLittleEndian(VolumeDescriptorSequenceNumber, buffer, offset + 16);
            EndianUtilities.WriteBytesLittleEndian(NumberofAllocationDescriptors, buffer, offset + 20);
            for (int i = 0; i < AllocationDescriptors.Length; i++)
            {
                AllocationDescriptors[i].WriteTo(buffer, offset + 24 + (i * 8));
            }
        }
    }
}
