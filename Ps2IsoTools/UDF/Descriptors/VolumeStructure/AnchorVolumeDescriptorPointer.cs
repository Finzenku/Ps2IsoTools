using Ps2IsoTools.DiscUtils.Utils;

namespace Ps2IsoTools.UDF.Descriptors.VolumeStructure
{
    internal class AnchorVolumeDescriptorPointer : TaggedDescriptor<AnchorVolumeDescriptorPointer>
    {
        public ExtentDescriptor MainDescriptorSequence;
        public ExtentDescriptor ReserveDescriptorSequence;
        public AnchorVolumeDescriptorPointer() : base(TagIdentifier.AnchorVolumeDescriptorPointer) { }

        public AnchorVolumeDescriptorPointer(uint sector, ExtentDescriptor mainDescriptorSequence, ExtentDescriptor? reserveDescriptorSequence = null) : base(TagIdentifier.AnchorVolumeDescriptorPointer)
        {
            Tag = new DescriptorTag()
            {
                TagIdentifier = TagIdentifier.AnchorVolumeDescriptorPointer,
                DescriptorCRCLength = (ushort)(Size - 16),
                TagLocation = sector,
            };
            MainDescriptorSequence = mainDescriptorSequence;
            ReserveDescriptorSequence = reserveDescriptorSequence ?? new ExtentDescriptor();

            FixChecksums();
        }

        public override int Parse(byte[] buffer, int offset)
        {
            MainDescriptorSequence = EndianUtilities.ToStruct<ExtentDescriptor>(buffer, offset + 16);
            ReserveDescriptorSequence = EndianUtilities.ToStruct<ExtentDescriptor>(buffer, offset + 24);

            return Size;
        }

        public override void Write(byte[] buffer, int offset)
        {
            MainDescriptorSequence.WriteTo(buffer, offset + 16);
            ReserveDescriptorSequence.WriteTo(buffer, offset + 24);
        }
    }
}
