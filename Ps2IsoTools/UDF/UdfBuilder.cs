//
// Copyright (c) 2008-2011, Kenneth Bell
//
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.
//

using System.Text;
using Ps2IsoTools.DiscUtils.Streams;
using Ps2IsoTools.DiscUtils.Utils;
using Ps2IsoTools.ISO.Builders;
using Ps2IsoTools.ISO.VolumeDescriptors;
using Ps2IsoTools.ISO.VolumeDescriptors.Regions;
using Ps2IsoTools.UDF.Builders;
using Ps2IsoTools.UDF.Descriptors;
using Ps2IsoTools.UDF.Descriptors.VolumeStructure;
using Ps2IsoTools.UDF.Descriptors.FileStructure;
using Ps2IsoTools.UDF.PartitionMaps;

namespace Ps2IsoTools.UDF
{
    public class UdfBuilder : StreamBuilder
    {
        private const long DiskStart = 0x8000;
        private BuildParameters buildParams;
        private UdfBuildDirectoryInfo rootDirectory;
        private List<UdfBuildDirectoryInfo> dirs;
        private List<BuildFileInfo> files;

        /// <summary>
        /// Gets or sets the Volume Identifier for the ISO file.
        /// </summary>
        /// <remarks>
        /// Must be a valid identifier, i.e. max 32 characters in the range A-Z, 0-9 or _.
        /// Lower-case characters are not permitted.
        /// </remarks>
        public string VolumeIdentifier
        {
            get { return buildParams.VolumeIdentifier; }

            set
            {
                if (value.Length > 32)
                {
                    throw new ArgumentException("Not a valid volume identifier");
                }
                buildParams.VolumeIdentifier = value;
            }
        }

        public bool UseJoliet
        {
            get => buildParams.UseJoliet;
            set => buildParams.UseJoliet = value;
        }

        public UdfBuilder()
        {
            dirs = new List<UdfBuildDirectoryInfo>();
            files = new List<BuildFileInfo>();
            buildParams = new();
            UseJoliet = false;
            rootDirectory = new UdfBuildDirectoryInfo("\0", null);
            dirs.Add(rootDirectory);
        }

        /// <summary>
        /// Adds a directory to the ISO image.
        /// </summary>
        /// <param name="name">The name of the directory on the ISO image.</param>
        /// <returns>The object representing this directory.</returns>
        /// <remarks>
        /// The name is the full path to the directory, for example:
        /// <example><code>
        ///   builder.AddDirectory(@"DIRA\DIRB\DIRC");
        /// </code></example>
        /// </remarks>
        public UdfBuildDirectoryInfo AddDirectory(string name)
        {
            string[] nameElements = name.Split(new[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
            return GetDirectory(nameElements, nameElements.Length, true);
        }

        /// <summary>
        /// Adds a byte array to the ISO image as a file.
        /// </summary>
        /// <param name="name">The name of the file on the ISO image.</param>
        /// <param name="content">The contents of the file.</param>
        /// <returns>The object representing this file.</returns>
        /// <remarks>
        /// The name is the full path to the file, for example:
        /// <example><code>
        ///   builder.AddFile(@"DIRA\DIRB\FILE.TXT;1", new byte[]{0,1,2});
        /// </code></example>
        /// <para>Note the version number at the end of the file name is optional, if not
        /// specified the default of 1 will be used.</para>
        /// </remarks>
        public BuildFileInfo AddFile(string name, byte[] content)
        {
            string[] nameElements = name.Split(new[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
            UdfBuildDirectoryInfo dir = GetDirectory(nameElements, nameElements.Length - 1, true);

            BuildDirectoryMember existing;
            if (dir.TryGetMember(nameElements[nameElements.Length - 1], out existing))
            {
                throw new IOException("File already exists");
            }
            BuildFileInfo fi = new BuildFileInfo(nameElements[nameElements.Length - 1], dir, content);
            files.Add(fi);
            dir.Add(fi);
            return fi;
        }

        /// <summary>
        /// Adds a disk file to the ISO image as a file.
        /// </summary>
        /// <param name="name">The name of the file on the ISO image.</param>
        /// <param name="sourcePath">The name of the file on disk.</param>
        /// <returns>The object representing this file.</returns>
        /// <remarks>
        /// The name is the full path to the file, for example:
        /// <example><code>
        ///   builder.AddFile(@"DIRA\DIRB\FILE.TXT;1", @"C:\temp\tempfile.bin");
        /// </code></example>
        /// <para>Note the version number at the end of the file name is optional, if not
        /// specified the default of 1 will be used.</para>
        /// </remarks>
        public BuildFileInfo AddFile(string name, string sourcePath)
        {
            string[] nameElements = name.Split(new[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
            UdfBuildDirectoryInfo dir = GetDirectory(nameElements, nameElements.Length - 1, true);

            BuildDirectoryMember existing;
            if (dir.TryGetMember(nameElements[nameElements.Length - 1], out existing))
            {
                throw new IOException("File already exists");
            }
            BuildFileInfo fi = new BuildFileInfo(nameElements[nameElements.Length - 1], dir, sourcePath);
            files.Add(fi);
            dir.Add(fi);
            return fi;
        }

        /// <summary>
        /// Adds a stream to the ISO image as a file.
        /// </summary>
        /// <param name="name">The name of the file on the ISO image.</param>
        /// <param name="source">The contents of the file.</param>
        /// <returns>The object representing this file.</returns>
        /// <remarks>
        /// The name is the full path to the file, for example:
        /// <example><code>
        ///   builder.AddFile(@"DIRA\DIRB\FILE.TXT;1", stream);
        /// </code></example>
        /// <para>Note the version number at the end of the file name is optional, if not
        /// specified the default of 1 will be used.</para>
        /// </remarks>
        public BuildFileInfo AddFile(string name, Stream source, bool makeCopy = false)
        {
            if (!source.CanSeek)
            {
                throw new ArgumentException("source doesn't support seeking", nameof(source));
            }
            MemoryStream streamCopy = new();
            if (makeCopy)
            {
                source.CopyTo(streamCopy);
                streamCopy.Position = 0;
            }
            string[] nameElements = name.Split(new[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
            UdfBuildDirectoryInfo dir = GetDirectory(nameElements, nameElements.Length - 1, true);

            BuildDirectoryMember existing;
            if (dir.TryGetMember(nameElements[nameElements.Length - 1], out existing))
            {
                throw new IOException("File already exists");
            }
            BuildFileInfo fi = new BuildFileInfo(nameElements[nameElements.Length - 1], dir, makeCopy ? streamCopy : source);
            files.Add(fi);
            dir.Add(fi);
            return fi;
        }

        private UdfBuildDirectoryInfo GetDirectory(string[] path, int pathLength, bool createMissing)
        {
            UdfBuildDirectoryInfo di = TryGetDirectory(path, pathLength, createMissing);

            if (di == null)
            {
                throw new DirectoryNotFoundException("Directory not found");
            }

            return di;
        }

        private UdfBuildDirectoryInfo TryGetDirectory(string[] path, int pathLength, bool createMissing)
        {
            UdfBuildDirectoryInfo focus = rootDirectory;

            for (int i = 0; i < pathLength; ++i)
            {
                BuildDirectoryMember next;
                if (!focus.TryGetMember(path[i], out next))
                {
                    if (createMissing)
                    {
                        // This directory doesn't exist, create it...
                        UdfBuildDirectoryInfo di = new UdfBuildDirectoryInfo(path[i], focus);
                        focus.Add(di);
                        dirs.Add(di);
                        focus = di;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    UdfBuildDirectoryInfo nextAsUdfBuildDirectoryInfo = next as UdfBuildDirectoryInfo;
                    if (nextAsUdfBuildDirectoryInfo == null)
                    {
                        throw new IOException("File with conflicting name exists");
                    }
                    focus = nextAsUdfBuildDirectoryInfo;
                }
            }

            return focus;
        }

        protected override List<BuilderExtent> FixExtents(out long totalLength)
        {
            List<BuilderExtent> fixedRegions = new List<BuilderExtent>();

            DateTime buildTime = DateTime.UtcNow;

            Encoding suppEncoding = UseJoliet ? Encoding.BigEndianUnicode : Encoding.UTF8;

            Dictionary<BuildDirectoryMember, uint> primaryIsoLocationTable = new Dictionary<BuildDirectoryMember, uint>();
            Dictionary<BuildDirectoryMember, uint> suppIsoLocationTable = new Dictionary<BuildDirectoryMember, uint>();
            Dictionary<BuildDirectoryMember, ShortAllocationDescriptor> udfFileLocationTable = new Dictionary<BuildDirectoryMember, ShortAllocationDescriptor>();
            Dictionary<BuildDirectoryMember, LongAllocationDescriptor> udfEntryLocationTable = new Dictionary<BuildDirectoryMember, LongAllocationDescriptor>();


            // ####################################################################
            // # 0. Anchor Volume Descriptor Pointer
            // ####################################################################
            long focus = 256 * IsoUtilities.SectorSize;

            // The volume descriptors could prob'ly start earlier but I'm just copying where it is in my example ISO
            // 16 sectors for the Iso9660 descriptors and 16 sectors for the UDF descriptors
            uint mainVolumeDescriptorSequenceSector = 32,
                reserveVolumeDescriptorSequenceSector = 48,
                integretyDescriptorSector = 64,
                volumeDescriptorSequenceLength = 16 * IsoUtilities.SectorSize;
            ExtentDescriptor mainVolumeExtentDescriptor = new ExtentDescriptor(volumeDescriptorSequenceLength, mainVolumeDescriptorSequenceSector),
                reserveVolumeExtentDescriptor = new ExtentDescriptor(volumeDescriptorSequenceLength, reserveVolumeDescriptorSequenceSector);

            AnchorVolumeDescriptorPointer avdp1 = new AnchorVolumeDescriptorPointer((uint)focus/IsoUtilities.SectorSize, mainVolumeExtentDescriptor, reserveVolumeExtentDescriptor);
            fixedRegions.Add(new BaseTaggedDescriptorRegion(avdp1, focus));
            focus += IsoUtilities.SectorSize;

            // ####################################################################
            // # 1. Fix file locations
            // ####################################################################

            // Calculate where each region should begin so we can place the files in appropriatae sectors

            // Path Tables
            long pathTableStart = focus, pathTableLength = 0;
            foreach (UdfBuildDirectoryInfo di in dirs)
            {
                pathTableLength += di.GetPathTableEntrySize(Encoding.UTF8);
            }
            focus += MathUtilities.RoundUp(pathTableLength, IsoUtilities.SectorSize) * (buildParams.UseJoliet ? 4 : 2);

            // Iso9660 Directories
            long directoryStart = focus, directoryLength = 0;
            foreach (UdfBuildDirectoryInfo di in dirs)
            {
                directoryLength += di.GetDataSize(Encoding.UTF8);
            }
            focus += UseJoliet ? directoryLength * 2 : directoryLength;

            // Start of Partition[0] (FileSetDescriptor + TerminatingDescriptor)
            long partitionStart = focus;
            focus += IsoUtilities.SectorSize * 2;

            // UDF Directory Locations
            foreach (UdfBuildDirectoryInfo di in dirs)
            {
                udfFileLocationTable.Add(di, new ShortAllocationDescriptor((uint)((focus - partitionStart) / IsoUtilities.SectorSize), (uint)di.GetUdfDataSize(Encoding.BigEndianUnicode)));
                focus += IsoUtilities.SectorSize;
            }

            // UDF File Entry Locations
            uint rootDirectorySector = (uint)(focus - partitionStart)/IsoUtilities.SectorSize;
            foreach (UdfBuildDirectoryInfo di in dirs)
            {
                udfEntryLocationTable.Add(di, new LongAllocationDescriptor(0x13C, (uint)(focus - partitionStart)/IsoUtilities.SectorSize));
                focus += IsoUtilities.SectorSize;
            }
            foreach(BuildFileInfo fi in files)
            {
                udfEntryLocationTable.Add(fi, new LongAllocationDescriptor(0x13C, (uint)(focus - partitionStart)/IsoUtilities.SectorSize));
                focus += IsoUtilities.SectorSize;
            }

            // Find end of the file data, fixing the files in place as we go
            foreach (BuildFileInfo fi in files)
            {
                primaryIsoLocationTable.Add(fi, (uint)(focus / IsoUtilities.SectorSize));
                if (UseJoliet)
                    suppIsoLocationTable.Add(fi, (uint)(focus / IsoUtilities.SectorSize));
                udfFileLocationTable.Add(fi, new ShortAllocationDescriptor((uint)((focus-partitionStart) / IsoUtilities.SectorSize), (uint)fi.GetDataSize(Encoding.UTF8)));
                FileExtent extent = new FileExtent(fi, focus);

                // Only remember files of non-zero length (otherwise we'll stomp on a valid file)
                if (extent.Length != 0)
                {
                    fixedRegions.Add(extent);
                }

                focus += MathUtilities.RoundUp(extent.Length, IsoUtilities.SectorSize);
            }

            // Final Anchor Volume Descriptor Pointer
            AnchorVolumeDescriptorPointer avdp2 = new AnchorVolumeDescriptorPointer((uint)focus/IsoUtilities.SectorSize, mainVolumeExtentDescriptor, reserveVolumeExtentDescriptor);
            fixedRegions.Add(new BaseTaggedDescriptorRegion(avdp2, focus));
            focus += IsoUtilities.SectorSize;

            // Find the end of the disk
            totalLength = focus;

            // I only plan on using a single partition so if someone smart is here, plz fix. ty
            // I have no idea why an ISO would need multiple partitions
            // Also does the partition length include the last avdp? Idk, should double check
            long partitionLength = totalLength - partitionStart;

            // ####################################################################
            // # 2. Fix directory locations
            // ####################################################################

            // There are two directory tables
            //  1. Primary        (std ISO9660)
            //  2. Supplementary  (Joliet)

            // Find start of the second set of directory data, fixing ASCII directories in place.
            focus = directoryStart;
            long startOfFirstDirData = focus;
            foreach (UdfBuildDirectoryInfo di in dirs)
            {
                primaryIsoLocationTable.Add(di, (uint)(focus / IsoUtilities.SectorSize));
                DirectoryExtent extent = new DirectoryExtent(di, primaryIsoLocationTable, Encoding.ASCII, focus);
                fixedRegions.Add(extent);
                focus += MathUtilities.RoundUp(extent.Length, IsoUtilities.SectorSize);
            }

            // Find end of the second directory table, fixing supplementary directories in place.
            long startOfSecondDirData = focus;
            if (UseJoliet)
            {
                foreach (UdfBuildDirectoryInfo di in dirs)
                {
                    suppIsoLocationTable.Add(di, (uint)(focus / IsoUtilities.SectorSize));
                    DirectoryExtent extent = new DirectoryExtent(di, suppIsoLocationTable, suppEncoding, focus);
                    fixedRegions.Add(extent);
                    focus += MathUtilities.RoundUp(extent.Length, IsoUtilities.SectorSize);
                }
            }

            // ####################################################################
            // # 3. File Set Descriptor
            // ####################################################################

            LongAllocationDescriptor rootDirectoryICB = new((uint)rootDirectory.GetUdfDataSize(Encoding.BigEndianUnicode), rootDirectorySector);
            FileSetDescriptor fsd = new FileSetDescriptor(0, buildParams.VolumeIdentifier, "PLAYSTATION2 DVD-ROM FILE SET", rootDirectoryICB);
            fixedRegions.Add(new BaseTaggedDescriptorRegion(fsd, focus));
            focus += IsoUtilities.SectorSize;

            TerminalDescriptor fstd = new TerminalDescriptor((uint)focus / IsoUtilities.SectorSize);
            fixedRegions.Add(new BaseTaggedDescriptorRegion(fstd, focus));
            focus += IsoUtilities.SectorSize;

            // ####################################################################
            // # 4. UDF Directories
            // ####################################################################

            foreach (UdfBuildDirectoryInfo di in dirs)
            {
                UdfDirectoryExtent extent = new UdfDirectoryExtent((uint)(focus - partitionStart)/IsoUtilities.SectorSize, di, udfEntryLocationTable, Encoding.BigEndianUnicode, focus);
                fixedRegions.Add(extent);
                focus += MathUtilities.RoundUp(extent.Length, IsoUtilities.SectorSize);
            }

            // ####################################################################
            // # 5. UDF File Entries
            // ####################################################################

            ulong uniqueId = 0; //First directory has uniqueId of 0 but everything after increments from 16
            foreach (UdfBuildDirectoryInfo di in dirs)
            {
                // Need size of UDF Directory Entry
                FileEntry fileEntry = new FileEntry((uint)(focus - partitionStart)/IsoUtilities.SectorSize, FileType.Directory, uniqueId, udfFileLocationTable[di]);
                fixedRegions.Add(new BaseTaggedDescriptorRegion(fileEntry, focus));
                focus += IsoUtilities.SectorSize;
                if ((uniqueId & 0xFFFFFFFF) > 0)
                    uniqueId++;
                else
                    uniqueId = (uniqueId & 0xFFFFFFFF00000000) | 16;
            }
            foreach (BuildFileInfo fi in files)
            {
                FileEntry fileEntry = new FileEntry((uint)(focus - partitionStart)/IsoUtilities.SectorSize, FileType.None, uniqueId, udfFileLocationTable[fi]);
                fixedRegions.Add(new BaseTaggedDescriptorRegion(fileEntry, focus));
                focus += IsoUtilities.SectorSize;
                if ((uniqueId & 0xFFFFFFFF) > 0)
                    uniqueId++;
                else
                    uniqueId = (uniqueId & 0xFFFFFFFF00000000) | 16;
            }

            // ####################################################################
            // # 6. Fix path tables
            // ####################################################################

            // There are four path tables:
            //  1. LE, ASCII
            //  2. BE, ASCII
            //  3. LE, Supp Encoding (Joliet)
            //  4. BE, Supp Encoding (Joliet)

            List<BuildDirectoryInfo> pathTableDirs = dirs.Cast<BuildDirectoryInfo>().ToList();

            // Find end of the path table
            focus = pathTableStart;
            long startOfFirstPathTable = focus;
            PathTableBuilder pathTable = new PathTableBuilder(false, Encoding.UTF8, pathTableDirs, primaryIsoLocationTable, focus);
            fixedRegions.Add(pathTable);
            focus += MathUtilities.RoundUp(pathTable.Length, IsoUtilities.SectorSize);
            long primaryPathTableLength = pathTable.Length;

            long startOfSecondPathTable = focus;
            pathTable = new PathTableBuilder(true, Encoding.UTF8, pathTableDirs, primaryIsoLocationTable, focus);
            fixedRegions.Add(pathTable);
            focus += MathUtilities.RoundUp(pathTable.Length, IsoUtilities.SectorSize);


            long startOfThirdPathTable = focus;
            long supplementaryPathTableLength = 0;
            long startOfFourthPathTable = 0;

            if (UseJoliet)
            {
                pathTable = new PathTableBuilder(false, suppEncoding, pathTableDirs, suppIsoLocationTable, focus);
                fixedRegions.Add(pathTable);
                focus += MathUtilities.RoundUp(pathTable.Length, IsoUtilities.SectorSize);
                supplementaryPathTableLength = pathTable.Length;

                startOfFourthPathTable = focus;
                pathTable = new PathTableBuilder(true, suppEncoding, pathTableDirs, suppIsoLocationTable, focus);
                fixedRegions.Add(pathTable);
            }


            // ####################################################################
            // # 7. Prepare volume descriptors now other structures are fixed
            // ####################################################################

            int regionIdx = 0;
            focus = DiskStart;
            ISO.VolumeDescriptors.PrimaryVolumeDescriptor pvDesc = new ISO.VolumeDescriptors.PrimaryVolumeDescriptor(
                (uint)(totalLength / IsoUtilities.SectorSize), // VolumeSpaceSize
                (uint)primaryPathTableLength, // PathTableSize
                (uint)(startOfFirstPathTable / IsoUtilities.SectorSize), // TypeLPathTableLocation
                (uint)(startOfSecondPathTable / IsoUtilities.SectorSize), // TypeMPathTableLocation
                (uint)(startOfFirstDirData / IsoUtilities.SectorSize), // RootDirectory.LocationOfExtent
                (uint)rootDirectory.GetDataSize(Encoding.UTF8), // RootDirectory.DataLength
                buildTime);
            pvDesc.VolumeIdentifier = buildParams.VolumeIdentifier;
            PrimaryVolumeDescriptorRegion pvdr = new PrimaryVolumeDescriptorRegion(pvDesc, focus);
            fixedRegions.Insert(regionIdx++, pvdr);
            focus += IsoUtilities.SectorSize;

            // Supplementary
            if (UseJoliet)
            {
                SupplementaryVolumeDescriptor svDesc = new SupplementaryVolumeDescriptor(
                    (uint)(totalLength / IsoUtilities.SectorSize), // VolumeSpaceSize
                    (uint)supplementaryPathTableLength, // PathTableSize
                    (uint)(startOfThirdPathTable / IsoUtilities.SectorSize), // TypeLPathTableLocation
                    (uint)(startOfFourthPathTable / IsoUtilities.SectorSize), // TypeMPathTableLocation
                    (uint)(startOfSecondDirData / IsoUtilities.SectorSize), // RootDirectory.LocationOfExtent
                    (uint)rootDirectory.GetDataSize(suppEncoding), // RootDirectory.DataLength
                    buildTime,
                    suppEncoding);
                svDesc.VolumeIdentifier = buildParams.VolumeIdentifier;
                SupplementaryVolumeDescriptorRegion svdr = new SupplementaryVolumeDescriptorRegion(svDesc, focus);
                fixedRegions.Insert(regionIdx++, svdr);
                focus += IsoUtilities.SectorSize;
            }

            VolumeDescriptorSetTerminator evDesc = new VolumeDescriptorSetTerminator();
            VolumeDescriptorSetTerminatorRegion evdr = new VolumeDescriptorSetTerminatorRegion(evDesc, focus);
            fixedRegions.Insert(regionIdx++, evdr);
            focus += IsoUtilities.SectorSize;

            // "BEA01" 	Denotes the beginning of the extended descriptor section. 
            BaseVolumeDescriptorRegion bea = new BaseVolumeDescriptorRegion(new ISO.VolumeDescriptors.BaseVolumeDescriptor("BEA01"), focus);
            fixedRegions.Insert(regionIdx++, bea);
            focus += IsoUtilities.SectorSize;

            // "NSR02" 	Indicates that this volume contains a UDF file system. 
            BaseVolumeDescriptorRegion nsr = new BaseVolumeDescriptorRegion(new ISO.VolumeDescriptors.BaseVolumeDescriptor("NSR02"), focus);
            fixedRegions.Insert(regionIdx++, nsr);
            focus += IsoUtilities.SectorSize;

            // "TEA01" 	Denotes the end of the extended descriptor section. 
            BaseVolumeDescriptorRegion tea = new BaseVolumeDescriptorRegion(new ISO.VolumeDescriptors.BaseVolumeDescriptor("TEA01"), focus);
            fixedRegions.Insert(regionIdx++, tea);


            // ####################################################################
            // # 8. Prepare UDF volume descriptors
            // ####################################################################

            focus = mainVolumeDescriptorSequenceSector * IsoUtilities.SectorSize;
            uint sector = mainVolumeDescriptorSequenceSector, descriptorSequenceNumber = 0;
            Descriptors.PrimaryVolumeDescriptor pvd = new(sector++, descriptorSequenceNumber++, buildParams.VolumeIdentifier);
            fixedRegions.Add(new BaseTaggedDescriptorRegion(pvd, focus));
            focus += MathUtilities.RoundUp(pvd.Size, IsoUtilities.SectorSize);

            ImplementationUseVolumeDescriptor iuvd = new(sector++, descriptorSequenceNumber++, buildParams.VolumeIdentifier);
            fixedRegions.Add(new BaseTaggedDescriptorRegion(iuvd, focus));
            focus += MathUtilities.RoundUp(iuvd.Size, IsoUtilities.SectorSize);

            PartitionDescriptor pd = new(sector++, descriptorSequenceNumber++, 0, (uint) partitionStart / IsoUtilities.SectorSize, (uint) partitionLength / IsoUtilities.SectorSize);
            fixedRegions.Add(new BaseTaggedDescriptorRegion(pd, focus));
            focus += MathUtilities.RoundUp(pd.Size, IsoUtilities.SectorSize);

            LongAllocationDescriptor fileSetAllocationDescriptor = new(2 * IsoUtilities.SectorSize, 0);
            ExtentDescriptor integreityDescriptorExtent = new(2 * IsoUtilities.SectorSize, integretyDescriptorSector);
            PartitionMap[] partitionMaps = new[] { new Type1PartitionMap(1, 0) };
            LogicalVolumeDescriptor lvd = new(sector++, descriptorSequenceNumber++, buildParams.VolumeIdentifier, fileSetAllocationDescriptor, integreityDescriptorExtent, partitionMaps, IsoUtilities.SectorSize);
            fixedRegions.Add(new BaseTaggedDescriptorRegion(lvd, focus));
            focus += MathUtilities.RoundUp(lvd.Size, IsoUtilities.SectorSize);

            UnallocatedSpaceDescriptor usd = new(sector++, descriptorSequenceNumber, new ExtentDescriptor[0]);
            fixedRegions.Add(new BaseTaggedDescriptorRegion(usd, focus));
            focus += MathUtilities.RoundUp(usd.Size, IsoUtilities.SectorSize);

            fixedRegions.Add(new BaseTaggedDescriptorRegion(new TerminalDescriptor(sector), focus));

            // Reserve volume descriptors
            focus = reserveVolumeDescriptorSequenceSector * IsoUtilities.SectorSize;
            sector = reserveVolumeDescriptorSequenceSector;
            descriptorSequenceNumber = 0;
            Descriptors.PrimaryVolumeDescriptor pvd2 = new(sector++, descriptorSequenceNumber++, buildParams.VolumeIdentifier);
            fixedRegions.Add(new BaseTaggedDescriptorRegion(pvd2, focus));
            focus += MathUtilities.RoundUp(pvd.Size, IsoUtilities.SectorSize);

            ImplementationUseVolumeDescriptor iuvd2 = new(sector++, descriptorSequenceNumber++, buildParams.VolumeIdentifier);
            fixedRegions.Add(new BaseTaggedDescriptorRegion(iuvd2, focus));
            focus += MathUtilities.RoundUp(iuvd.Size, IsoUtilities.SectorSize);

            PartitionDescriptor pd2 = new(sector++, descriptorSequenceNumber++, 0, (uint)partitionStart / IsoUtilities.SectorSize, (uint)partitionLength / IsoUtilities.SectorSize);
            fixedRegions.Add(new BaseTaggedDescriptorRegion(pd2, focus));
            focus += MathUtilities.RoundUp(pd.Size, IsoUtilities.SectorSize);

            LogicalVolumeDescriptor lvd2 = new(sector++, descriptorSequenceNumber++, buildParams.VolumeIdentifier, fileSetAllocationDescriptor, integreityDescriptorExtent, partitionMaps, IsoUtilities.SectorSize);
            fixedRegions.Add(new BaseTaggedDescriptorRegion(lvd2, focus));
            focus += MathUtilities.RoundUp(lvd.Size, IsoUtilities.SectorSize);

            UnallocatedSpaceDescriptor usd2 = new(sector++, descriptorSequenceNumber, new ExtentDescriptor[0]);
            fixedRegions.Add(new BaseTaggedDescriptorRegion(usd2, focus));
            focus += MathUtilities.RoundUp(usd.Size, IsoUtilities.SectorSize);

            fixedRegions.Add(new BaseTaggedDescriptorRegion(new TerminalDescriptor(sector), focus));

            // ####################################################################
            // # 9. Logical Volume Integrity Descriptor
            // ####################################################################

            focus = integretyDescriptorSector * IsoUtilities.SectorSize;
            LogicalVolumeIntegrityDescriptor lvid = new(integretyDescriptorSector, 1, new uint[1] { 0x0 }, new uint[1] { (uint) partitionLength / IsoUtilities.SectorSize } );
            fixedRegions.Add(new BaseTaggedDescriptorRegion(lvid, focus));
            focus += MathUtilities.RoundUp(lvid.Size, IsoUtilities.SectorSize);

            fixedRegions.Add(new BaseTaggedDescriptorRegion(new TerminalDescriptor(integretyDescriptorSector + 1), focus));

            return fixedRegions;
        }

    }
}
