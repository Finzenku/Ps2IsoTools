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

using Ps2IsoTools.DiscUtils.Streams.Buffers;
using Ps2IsoTools.DiscUtils.Utils;
using Ps2IsoTools.UDF.Descriptors;
using Ps2IsoTools.UDF.Descriptors.FileStructure;
using Ps2IsoTools.UDF.Descriptors.FileStructure.ExtendedAttribute;
using Ps2IsoTools.UDF.Partitions;

namespace Ps2IsoTools.UDF.Files
{
    internal class File : IVfsFile
    {
        protected uint _blockSize;
        protected IBuffer _content;
        protected UdfContext _context;
        protected FileEntry _fileEntry;
        protected Partition _partition;

        public File(UdfContext context, Partition partition, FileEntry fileEntry, uint blockSize)
        {
            _context = context;
            _partition = partition;
            _fileEntry = fileEntry;
            _blockSize = blockSize;
            _content = new FileContentBuffer(_context, _partition, _fileEntry, _blockSize);
        }

        public List<ExtendedAttributeRecord> ExtendedAttributes
        {
            get { return _fileEntry.ExtendedAttributesList; }
        }

        public FileEntry FileEntry => _fileEntry;

        public IBuffer FileContent
        {
            get
            {
                if (_content != null)
                {
                    return _content;
                }

                _content = new FileContentBuffer(_context, _partition, _fileEntry, _blockSize);
                return _content;
            }
        }

        public DateTime LastAccessTimeUtc
        {
            get { return _fileEntry.AccessTime.DateTime; }
            set { throw new NotSupportedException(); }
        }

        public DateTime LastWriteTimeUtc
        {
            get { return _fileEntry.ModificationTime.DateTime; }
            set { throw new NotSupportedException(); }
        }

        public DateTime CreationTimeUtc
        {
            get
            {
                ExtendedFileEntry? efe = _fileEntry as ExtendedFileEntry;
                if (efe != null)
                {
                    return efe.CreationTime.DateTime;
                }
                return LastWriteTimeUtc;
            }

            set { throw new NotSupportedException(); }
        }

        public FileAttributes FileAttributes
        {
            get
            {
                FileAttributes attribs = 0;
                InformationControlBlockFlags flags = _fileEntry.InformationControlBlockTag.Flags;

                if (_fileEntry.InformationControlBlockTag.FileType == FileType.Directory)
                {
                    attribs |= FileAttributes.Directory;
                }
                else if (_fileEntry.InformationControlBlockTag.FileType == FileType.Fifo
                         || _fileEntry.InformationControlBlockTag.FileType == FileType.Socket
                         || _fileEntry.InformationControlBlockTag.FileType == FileType.SpecialBlockDevice
                         || _fileEntry.InformationControlBlockTag.FileType == FileType.SpecialCharacterDevice
                         || _fileEntry.InformationControlBlockTag.FileType == FileType.TerminalEntry)
                {
                    attribs |= FileAttributes.Device;
                }

                if ((flags & InformationControlBlockFlags.Archive) != 0)
                {
                    attribs |= FileAttributes.Archive;
                }

                if ((flags & InformationControlBlockFlags.System) != 0)
                {
                    attribs |= FileAttributes.System | FileAttributes.Hidden;
                }

                if ((int)attribs == 0)
                {
                    attribs = FileAttributes.Normal;
                }

                return attribs;
            }

            set { throw new NotSupportedException(); }
        }

        public long FileLength
        {
            get { return (long)_fileEntry.InformationLength; }
        }

        public static File FromDescriptor(UdfContext context, LongAllocationDescriptor icb)
        {
            LogicalPartition partition = context.LogicalPartitions[icb.ExtentLocation.Partition];

            byte[] rootDirData = UdfUtilities.ReadExtent(context, icb);
            DescriptorTag rootDirTag = EndianUtilities.ToStruct<DescriptorTag>(rootDirData, 0);

            if (rootDirTag.TagIdentifier == TagIdentifier.ExtendedFileEntry)
            {
                ExtendedFileEntry fileEntry = EndianUtilities.ToStruct<ExtendedFileEntry>(rootDirData, 0);
                if (fileEntry.InformationControlBlockTag.FileType == FileType.Directory)
                {
                    return new Directory(context, partition, fileEntry);
                }
                return new File(context, partition, fileEntry, (uint)partition.LogicalBlockSize);
            }
            if (rootDirTag.TagIdentifier == TagIdentifier.FileEntry)
            {
                FileEntry fileEntry = EndianUtilities.ToStruct<FileEntry>(rootDirData, 0);
                if (fileEntry.InformationControlBlockTag.FileType == FileType.Directory)
                {
                    return new Directory(context, partition, fileEntry);
                }
                return new File(context, partition, fileEntry, (uint)partition.LogicalBlockSize);
            }
            throw new NotImplementedException("Only ExtendedFileEntries implemented");
        }
    }
}