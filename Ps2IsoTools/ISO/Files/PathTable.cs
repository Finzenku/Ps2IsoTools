using Ps2IsoTools.DiscUtils.Utils;

namespace Ps2IsoTools.ISO.Files
{
    internal class PathTable : IByteArraySerializable
    {
        public List<PathTableEntry> Entries;

        public PathTable()
        {
            Entries = new();
        }

        public int Size => Entries.Select(E => E.Size).Sum();

        public int ReadFrom(byte[] buffer, int offset)
        {
            PathTableEntry entry;
            do
            {
                entry = new();
                offset += entry.ReadFrom(buffer, offset);
                if (entry.NameLength > 0)
                    Entries.Add(entry);

            } while (entry.NameLength > 0);
            return Entries.Count;
        }

        public void WriteTo(byte[] buffer, int offset)
        {
            foreach (PathTableEntry entry in Entries)
            {
                entry.WriteTo(buffer, offset);
                offset += entry.Size;
            }
        }
    }
}
