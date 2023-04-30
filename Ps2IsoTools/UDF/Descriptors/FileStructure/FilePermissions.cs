namespace Ps2IsoTools.UDF.Descriptors.FileStructure
{
    [Flags]
    internal enum FilePermissions
    {
        None = 0,
        OthersExecute = 0x1,
        OthersWrite = 0x2,
        OthersRead = 0x4,
        OthersChangeAttributes = 0x8,
        OthersDelete = 0x10,
        GroupExecute = 0x20,
        GroupWrite = 0x40,
        GroupRead = 0x80,
        GroupChangeAttributes = 0x100,
        GroupDelete = 0x200,
        OwnerExecute = 0x400,
        OwnerWrite = 0x800,
        OwnerRead = 0x1000,
        OwnerChangeAttributes = 0x2000,
        OwnerDelete = 0x4000,
    }
}
