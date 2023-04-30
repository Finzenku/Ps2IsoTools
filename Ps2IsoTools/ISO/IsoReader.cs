using Ps2IsoTools.DiscUtils.Utils;
using Ps2IsoTools.ISO.Files;
using Directory = Ps2IsoTools.ISO.Files.Directory;

namespace Ps2IsoTools.ISO
{
    internal class IsoReader : IDisposable
    {
        private static readonly int SectorSize = 0x800;
        private ISO.VolumeDescriptors.PrimaryVolumeDescriptor PVD;
        private List<Directory> directories;
        private PathTable PathTable;
        FileStream ms;

        public IsoReader(string filePath)
        {
            byte[] buffer = new byte[SectorSize];
            ms = File.Open(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
            ms.Position = 0x10 * SectorSize;
            ms.Read(buffer);
            PVD = new ISO.VolumeDescriptors.PrimaryVolumeDescriptor(buffer, 0);
            ms.Position = PVD.TypeLPathTableLocation * PVD.LogicalBlockSize;
            ms.Read(buffer);
            PathTable = new PathTable();
            PathTable.ReadFrom(buffer, 0);

            directories = new();

            foreach (PathTableEntry entry in PathTable.Entries)
            {
                ms.Position = entry.DirectoryLocation * PVD.LogicalBlockSize;
                ms.Read(buffer);
                Directory dir = new();
                dir.ReadFrom(buffer, 0);
                directories.Add(dir);
            }
        }

        public bool CopyFile(DirectoryEntry file, string filePath = "")
        {
            if (filePath == "")
                filePath = file.Name.Split(";")[0];
            FileInfo fi = new FileInfo(filePath);
            using (FileStream stream = fi.Create())
            {
                ms.Position = file.EntryLocation * SectorSize;
                StreamUtilities.CopyStream(ms, stream, file.EntrySize);
            }
            return File.Exists(filePath) && fi.Length == file.EntrySize;
        }

        public DirectoryEntry? GetFileByName(string fileName)
        {
            string[] path = fileName.Split('\\', '/');
            if (path.Length == 1)
                return GetFileNoPath(path[0]);

            Directory? dir = directories[0];
            DirectoryEntry? entry = null;
            for (int i = 0; i < path.Length; i++)
            {
                entry = dir?.GetEntryByName(path[i]);
                if (entry is null)
                    return null;
                if (entry.IsDirectory)
                {
                    dir = GetDirectoryByIdentifier(entry);
                }
            }
            if (entry is not null && !entry.IsDirectory)
            {
                return entry;
            }
            return null;
        }
        private DirectoryEntry? GetFileNoPath(string fileName)
        {
            foreach (Directory dir in directories)
            {
                DirectoryEntry? entry = dir.GetEntryByName(fileName);
                if (entry is not null)
                    return entry;
            }
            return null;
        }

        public int GetFileLocationByName(string fileName)
        {
            string[] path = fileName.Split('\\', '/');
            if (path.Length == 1)
                return GetFileLocationNoPath(path[0]);

            Directory? dir = directories[0];
            DirectoryEntry? entry = null; 
            for (int i = 0; i < path.Length; i++)
            {
                entry = dir?.GetEntryByName(path[i]);
                if (entry is null)
                    return -1;
                if (entry.IsDirectory)
                {
                    dir = GetDirectoryByIdentifier(entry);
                }
            }
            if (entry is not null && !entry.IsDirectory)
            {
                return entry.EntryLocation * SectorSize;
            }
            return -1;
        }

        private int GetFileLocationNoPath(string fileName)
        {
            foreach (Directory dir in directories)
            {
                DirectoryEntry? entry = dir.GetEntryByName(fileName);
                if (entry is not null)
                    return entry.EntryLocation * SectorSize;
            }
            return -1;
        }


        public Directory? GetDirectoryByIdentifier(DirectoryEntry fileIdentifier)
        {
            foreach(Directory dir in directories)
            {
                if (dir.Identifier.Equals(fileIdentifier))
                    return dir;
            }
            return null;
        }

        public void Dispose()
        {
            ms.Dispose();
        }
    }
}
