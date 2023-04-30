using Ps2IsoTools.DiscUtils.Utils;

namespace Ps2IsoTools.UDF.Descriptors.FileStructure.ExtendedAttribute
{
    internal class ExtendedAttributeHeaderDescriptor : TaggedDescriptor<ExtendedAttributeHeaderDescriptor>
    {
        public uint ImplementationAttributesLocation { get; set; }
        public uint ApplicationAttributesLocation { get; set; }

        public override int Size => 24;

        public ExtendedAttributeHeaderDescriptor() : base(TagIdentifier.ExtendedAttributeHeaderDescriptor) { }

        public ExtendedAttributeHeaderDescriptor(uint sector, uint implementationAtrributesLocaation, uint applicationAttributesLocation) : base(TagIdentifier.ExtendedAttributeHeaderDescriptor)
        {
            Tag = new DescriptorTag()
            {
                TagIdentifier = TagIdentifier.ExtendedAttributeHeaderDescriptor,
                DescriptorCRCLength = (ushort)(Size - 16),
                TagLocation = sector,
            };
            ImplementationAttributesLocation = implementationAtrributesLocaation;
            ApplicationAttributesLocation = applicationAttributesLocation;

            FixChecksums();
        }

        public override int Parse(byte[] buffer, int offset)
        {
            ImplementationAttributesLocation = EndianUtilities.ToUInt32LittleEndian(buffer, offset + 16);
            ApplicationAttributesLocation = EndianUtilities.ToUInt32LittleEndian(buffer, offset + 20);

            return Size;
        }

        public override void Write(byte[] buffer, int offset)
        {
            EndianUtilities.WriteBytesLittleEndian(ImplementationAttributesLocation, buffer, offset + 16);
            EndianUtilities.WriteBytesLittleEndian(ApplicationAttributesLocation, buffer, offset + 20);
        }
    }
}
