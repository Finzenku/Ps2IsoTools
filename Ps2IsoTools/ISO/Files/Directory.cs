using Ps2IsoTools.DiscUtils.Utils;

namespace Ps2IsoTools.ISO.Files
{
    public class Directory : IByteArraySerializable
    {
        public DirectoryEntry Identifier, ParentIdentifier;
        public List<DirectoryEntry> Entries;

        public int Size => Identifier.EntrySize;
        public bool IsRoot => Identifier.Equals(ParentIdentifier);

        public Directory()
        {
            Identifier = new();
            ParentIdentifier = new();
            Entries = new();
        }

        public int ReadFrom(byte[] buffer, int offset)
        {
            int readLen = Identifier.ReadFrom(buffer, offset);
            if (readLen < Size)
                readLen += ParentIdentifier.ReadFrom(buffer, offset + readLen);
            while (readLen < Size)
            {
                DirectoryEntry entry = new();
                readLen += entry.ReadFrom(buffer, offset + readLen);
                Entries.Add(entry);
            }

            return Size;
        }

        public void WriteTo(byte[] buffer, int offset)
        {
            throw new NotImplementedException();
        }

        public DirectoryEntry? GetEntryByName(string Name)
        {
            if (Name.Contains(";"))
                Name = Name.Split(';')[0];
            foreach (DirectoryEntry fi in Entries)
            {
                if (string.Compare(Name, fi.Name.Split(';')[0], StringComparison.OrdinalIgnoreCase) == 0)
                    return fi;
            }
            return null;
        }
    }
}
