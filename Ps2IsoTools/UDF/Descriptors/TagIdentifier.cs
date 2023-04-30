namespace Ps2IsoTools.UDF.Descriptors
{
    internal enum TagIdentifier : ushort
    {
        None = 0,
        PrimaryVolumeDescriptor = 1,
        AnchorVolumeDescriptorPointer = 2,
        VolumeDescriptorPointer = 3,
        ImplementationUseVolumeDescriptor = 4,
        PartitionDescriptor = 5,
        LogicalVolumeDescriptor = 6,
        UnallocatedSpaceDescriptor = 7,
        TerminatingDescriptor = 8,
        LogicalVolumeIntegrityDescriptor = 9,

        FileSetDescriptor = 0x100,
        FileIdentifierDescriptor = 0x101,
        AllocationExtentDescriptor = 0x102,
        IndirectEntry = 0x103,
        TerminalEntry = 0x104,
        FileEntry = 0x105,
        ExtendedAttributeHeaderDescriptor = 0x106,
        UnallocatedSpaceEntry = 0x107,
        SpaceBitmapDescriptor = 0x108,
        PartitionIntegrityEntry = 0x109,
        ExtendedFileEntry = 0x10A
    }
}
