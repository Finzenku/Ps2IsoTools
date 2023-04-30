namespace Ps2IsoTools.UDF.Descriptors.VolumeStructure
{
    internal class TerminalDescriptor : TaggedDescriptor<TerminalDescriptor>
    {
        public TerminalDescriptor() : base(TagIdentifier.TerminatingDescriptor) { }

        public TerminalDescriptor(uint sector) : base(TagIdentifier.TerminatingDescriptor)
        {
            Tag = new DescriptorTag()
            {
                TagIdentifier = TagIdentifier.TerminatingDescriptor,
                DescriptorCRCLength = (ushort)(Size - 16),
                TagLocation = sector,
            };

            FixChecksums();
        }
        public override int Parse(byte[] buffer, int offset)
        {
            return Size;
        }

        public override void Write(byte[] buffer, int offset)
        {
            return;
        }
    }
}
