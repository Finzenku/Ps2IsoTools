namespace Ps2IsoTools.UDF.Descriptors.FileStructure
{
    internal enum FileType : byte
    {
        None = 0,
        UnallocatedSpaceEntry = 1,
        PartitionIntegrityEntry = 2,
        IndirectEntry = 3,
        Directory = 4,
        RandomBytes = 5,
        SpecialBlockDevice = 6,
        SpecialCharacterDevice = 7,
        ExtendedAttributes = 8,
        Fifo = 9,
        Socket = 10,
        TerminalEntry = 11,
        SymbolicLink = 12,
        StreamDirectory = 13,

        UdfVirtualAllocationTable = 248,
        UdfRealTimeFile = 249,
        UdfMetadataFile = 250,
        UdfMetadataMirrorFile = 251,
        UdfMetadataBitmapFile = 252
    }
}