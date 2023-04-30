namespace Ps2IsoTools.ISO.Files
{
    [Flags]
    public enum FileFlags : byte
    {
        None = 0x0,
        Hidden = 0x1,
        Directory = 0x2,
        AssociatedFile = 0x4,
        Record = 0x8,
        Protection = 0x10,
        MultiExtent = 0x80,
    }
}
