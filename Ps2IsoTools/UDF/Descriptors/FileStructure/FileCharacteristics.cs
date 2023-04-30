namespace Ps2IsoTools.UDF.Descriptors.FileStructure
{
    [Flags]
    internal enum FileCharacteristics : byte
    {
        Default = 0,
        Existence = 0x1,
        Directory = 0x2,
        Deleted = 0x4,
        Parent = 0x8,
        Metadata= 0x10
    }
}
