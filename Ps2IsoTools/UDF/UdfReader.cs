using Ps2IsoTools.DiscUtils.Streams.Buffers;
using Ps2IsoTools.DiscUtils.Streams.Utils;
using Ps2IsoTools.DiscUtils.Utils;
using Ps2IsoTools.ISO.VolumeDescriptors;
using Ps2IsoTools.UDF.Descriptors;
using Ps2IsoTools.UDF.Descriptors.FileStructure;
using Ps2IsoTools.UDF.Descriptors.VolumeStructure;
using Ps2IsoTools.UDF.Files;
using Ps2IsoTools.UDF.Partitions;
using Directory = Ps2IsoTools.UDF.Files.Directory;
using File = Ps2IsoTools.UDF.Files.File;

namespace Ps2IsoTools.UDF
{
    public class UdfReader : IDisposable
    {
        private FileStream data;
        private UdfContext Context;
        private List<BaseTaggedDescriptor> Descriptors;
        private List<PartitionDescriptor> PartitionDescriptors;
        private AnchorVolumeDescriptorPointer AVDP;
        private Descriptors.PrimaryVolumeDescriptor UPVD;
        private LogicalVolumeDescriptor LVD;
        private FileSetDescriptor FSD;
        private ImplementationUseVolumeDescriptor IUVD;
        private Directory RootDirectory;
        private List<Directory> directories;

        public string VolumeLabel => LVD.LogicalVolumeIdentifier;

        public static uint SectorSize => IsoUtilities.SectorSize;
        public UdfReader(string filePath)
        {
            data = System.IO.File.Open(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.Read);
            if (!Detect(data))
                throw new InvalidDataException("File is not a valid UDF ISO");

            directories = new();
            Descriptors = new();
            PartitionDescriptors = new();
            UPVD = new();
            LVD = new();
            FSD = new();
            IUVD = new();

            Initialize();
        }

        private void Initialize()
        {
            Context = new UdfContext
            {
                PhysicalPartitions = new Dictionary<ushort, PhysicalPartition>(),
                PhysicalSectorSize = (int)SectorSize,
                LogicalPartitions = new List<LogicalPartition>()
            };

            IBuffer dataBuffer = new StreamBuffer(data, Ownership.None);

            AVDP = AnchorVolumeDescriptorPointer.FromStream(data, 256, SectorSize);
            Descriptors.Add(AVDP);
            uint sector = AVDP.MainDescriptorSequence.Location;
            bool terminatorFound = false;
            while (!terminatorFound)
            {
                data.Position = sector * SectorSize;

                DescriptorTag dt;
                if (!DescriptorTag.TryFromStream(data, out dt))
                {
                    break;
                }
                switch (dt.TagIdentifier)
                {
                    case TagIdentifier.PrimaryVolumeDescriptor:
                        UPVD = UDF.Descriptors.PrimaryVolumeDescriptor.FromStream(data, sector, SectorSize);
                        Descriptors.Add(UPVD);
                        break;

                    case TagIdentifier.ImplementationUseVolumeDescriptor:
                        IUVD = ImplementationUseVolumeDescriptor.FromStream(data, sector, SectorSize);
                        Descriptors.Add(IUVD);
                        break;

                    case TagIdentifier.PartitionDescriptor:
                        var PD = PartitionDescriptor.FromStream(data, sector, SectorSize);
                        if (Context.PhysicalPartitions.ContainsKey(PD.PartitionNumber))
                            throw new IOException($"Duplicate partion number ({PD.PartitionNumber}) reading UDF Partition Descriptor");
                        Context.PhysicalPartitions[PD.PartitionNumber] = new PhysicalPartition(PD, dataBuffer, SectorSize);
                        Descriptors.Add(PD);
                        PartitionDescriptors.Add(PD);
                        break;

                    case TagIdentifier.LogicalVolumeDescriptor:
                        LVD = LogicalVolumeDescriptor.FromStream(data, sector, SectorSize);
                        Descriptors.Add(LVD);
                        break;

                    case TagIdentifier.UnallocatedSpaceDescriptor:
                        Descriptors.Add(UnallocatedSpaceDescriptor.FromStream(data, sector, SectorSize));
                        break;

                    case TagIdentifier.TerminatingDescriptor:
                        Descriptors.Add(TerminalDescriptor.FromStream(data, sector, SectorSize));
                        terminatorFound = true;
                        break;

                    default:
                        break;
                }
                sector++;
            }

            //Convert logical partition descriptors into actual partition objects
            for (int i = 0; i < LVD.PartitionMaps.Length; i++)
                Context.LogicalPartitions.Add(LogicalPartition.FromDescriptor(Context, LVD, i));

            byte[] fsdBuffer = UdfUtilities.ReadExtent(Context, LVD.FileSetDescriptorLocation);
            if (DescriptorTag.IsValid(fsdBuffer, 0))
            {
                FSD = EndianUtilities.ToStruct<FileSetDescriptor>(fsdBuffer, 0);
                RootDirectory = (Directory)File.FromDescriptor(Context, FSD.RootDirectoryICB);
                directories.Add(RootDirectory);
                foreach (FileIdentifier fi in RootDirectory.AllEntries)
                {
                    if (fi.IsDirectory)
                        directories.Add((Directory)ConvertFileIdToFile(fi));
                }
                Descriptors.Add(FSD);
            }
        }

        public List<FileIdentifier> GetAllEntries()
        {
            List<FileIdentifier> entries = new();

            foreach (Directory d in directories)
                entries.AddRange(d.AllEntries);

            return entries;
        }

        public List<string> GetAllFileFullNames()
        {
            List<string> names = new();
            foreach (FileIdentifier fi in RootDirectory.AllEntries)
            {
                string n = fi.FileName;
                if (fi.IsDirectory)
                    names.AddRange(GetDirectoryNames(n, (Directory)ConvertFileIdToFile(fi)));
                else
                    names.Add(n);
            }
            return names;
        }

        private List<string> GetDirectoryNames(string dirName, Directory dir)
        {
            List<string> names = new();
            foreach (FileIdentifier fi in dir.AllEntries)
            {
                string n = $"{dirName}\\{fi.FileName}";
                if (fi.IsDirectory)
                    names.AddRange(GetDirectoryNames(n, (Directory)ConvertFileIdToFile(fi)));
                else
                    names.Add(n);
            }
            return names;
        }

        public bool CopyFile(FileIdentifier file, string filePath = "")
        {
            if (filePath == "")
                filePath = file.FileName.Split(';')[0];
            FileInfo fi = new FileInfo(filePath);
            Stream fileStream = GetFileStream(file);
            using (FileStream stream = fi.Create())
            {
                StreamUtilities.CopyStream(fileStream, stream, (int)fileStream.Length);
            }
            return System.IO.File.Exists(filePath) && fi.Length == fileStream.Length;
        }

        public Stream GetFileStream(FileIdentifier file) => new BufferStream(ConvertFileIdToFile(file).FileContent, FileAccess.Read);

        internal Stream GetWritableFileStream(FileIdentifier file) => new BufferStream(ConvertFileIdToFile(file).FileContent, FileAccess.ReadWrite);
        internal Stream GetFileStream(string fileName) => GetFileStream(GetFileByName(fileName));

        public FileIdentifier? GetFileByName(string fileName)
        {
            string[] path = fileName.Split('\\', '/');
            if (path.Length == 1)
                return GetFileNoPath(path[0]);

            Directory? dir = directories[0];
            FileIdentifier? entry = null;
            for (int i = 0; i < path.Length; i++)
            {
                entry = dir?.GetEntryByName(path[i]);
                if (entry is null)
                    return null;
                if (entry.IsDirectory)
                {
                    dir = (Directory)ConvertFileIdToFile(entry);
                }
            }
            if (entry is not null && !entry.IsDirectory)
            {
                return entry;
            }
            return null;
        }
        private FileIdentifier? GetFileNoPath(string fileName)
        {
            foreach (Directory dir in directories)
            {
                FileIdentifier? entry = dir.GetEntryByName(fileName);
                if (entry is not null)
                    return entry;
            }
            return null;
        }

        internal File ConvertFileIdToFile(FileIdentifier dirEntry)
        {
            return File.FromDescriptor(Context, dirEntry.InformationControlBlock);
        }

        public static bool Detect(Stream data)
        {
            if (data.Length < SectorSize)
                return false;
            byte[] buffer = new byte[SectorSize];

            long descriptorPos = 0x8000;

            bool validDescriptor = true;
            bool foundUdfMarker = false;

            BaseVolumeDescriptor bvd;
            while (validDescriptor)
            {
                data.Position = descriptorPos;
                int numRead = StreamUtilities.ReadMaximum(data, buffer, 0, (int)SectorSize);
                if (numRead != SectorSize)
                    break;

                bvd = new BaseVolumeDescriptor(buffer, 0);
                switch (bvd.Identifier)
                {
                    case "NSR02":
                    case "NSR03":
                        foundUdfMarker = true;
                        break;
                    case "CD001":
                    case "CD002":
                    case "BOOT2":
                    case "BEA01":
                    case "TEA01":
                        break;
                    default:
                        validDescriptor = false;
                        break;
                }
                descriptorPos += SectorSize;
            }
            return foundUdfMarker;
        }

        public void Dispose()
        {
            data.Dispose();
        }
    }

}
