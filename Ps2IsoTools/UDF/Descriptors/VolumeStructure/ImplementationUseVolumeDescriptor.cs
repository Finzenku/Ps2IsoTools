using Ps2IsoTools.DiscUtils.Utils;
using Ps2IsoTools.UDF.EntityIdentifiers;

namespace Ps2IsoTools.UDF.Descriptors
{
    internal class ImplementationUseVolumeDescriptor : TaggedDescriptor<ImplementationUseVolumeDescriptor>
    {
        public uint VolumeDescriptorSequenceNumber;
        public EntityIdentifier ImplementationIdentifier;
        public LVInformation ImplementationUse;

        public ImplementationUseVolumeDescriptor() : base(TagIdentifier.ImplementationUseVolumeDescriptor) { }

        public ImplementationUseVolumeDescriptor(uint sector, uint descriptorSequenceNumber, string volumeIdentifier, string volumeInfo1 = "", string volumeInfo2 = "", string volumeInfo3 = "") : base(TagIdentifier.ImplementationUseVolumeDescriptor)
        {
            Tag = new DescriptorTag()
            {
                TagIdentifier = TagIdentifier.ImplementationUseVolumeDescriptor,
                DescriptorCRCLength = (ushort)(Size - 16),
                TagLocation = sector,
            };

            VolumeDescriptorSequenceNumber = descriptorSequenceNumber;
            ImplementationIdentifier = CommonIdentifiers.UDFLVInfoIdentifier;
            ImplementationUse = new LVInformation(volumeIdentifier, volumeInfo1, volumeInfo2, volumeInfo3);

            FixChecksums();
        }

        public override int Parse(byte[] buffer, int offset)
        {
            VolumeDescriptorSequenceNumber = EndianUtilities.ToUInt32LittleEndian(buffer, offset + 16);
            ImplementationIdentifier = EndianUtilities.ToStruct<ApplicationEntityIdentifier>(buffer, offset + 20);
            ImplementationUse = EndianUtilities.ToStruct<LVInformation>(buffer, offset + 52);

            return Size;
        }

        public override void Write(byte[] buffer, int offset)
        {
            EndianUtilities.WriteBytesLittleEndian(VolumeDescriptorSequenceNumber, buffer, offset + 16);
            ImplementationIdentifier.WriteTo(buffer, offset + 20);
            ImplementationUse.WriteTo(buffer, offset + 52);
        }
    }
}
