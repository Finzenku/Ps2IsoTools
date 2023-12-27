using System.Text;
using Ps2IsoTools.DiscUtils.Utils;
using Ps2IsoTools.Extensions;

namespace Ps2IsoTools.ISO.Files
{
    public class DirectoryEntry : IByteArraySerializable
    {
        public byte EntryLength;
        public byte ExtendedAttributeRecordLength;
        public int EntryLocation;//, EntryLocationBigEnd;
        public int EntrySize;//, EntrySizeBigEnd;
        public DateTime RecordingDateAndTime;
        public FileFlags FileFlags;
        public byte InterleaveUnitSize;
        public byte InterleaveGapSize;
        public ushort VolumeSequenceNumber;//, VolumeSequenceNumberBigEnd;
        public byte NameLength;
        public string Name = string.Empty;
        public byte[] SystemUseData = new byte[0];


        public byte PadLength => Name.Length % 2 == 0 ? (byte)1 : (byte)0;
        public int Size { get => 33 + Name.Length + PadLength + SystemUseData.Length; }
        public bool IsDirectory => (FileFlags & FileFlags.Directory) > 0;

        public int ReadFrom(byte[] buffer, int offset)
        {
            EntryLength = buffer[offset];
            ExtendedAttributeRecordLength = buffer[offset + 1];
            EntryLocation = EndianUtilities.ToInt32LittleEndian(buffer, offset + 2);
            //EntryLocationBigEnd = EndianUtilities.ToInt32BigEndian(buffer, offset + 6);
            EntrySize = EndianUtilities.ToInt32LittleEndian(buffer, offset + 10);
            //EntrySizeBigEnd = EndianUtilities.ToInt32BigEndian(buffer, offset + 14);
            RecordingDateAndTime = IsoUtilities.ToUTCDateTimeFromDirectoryTime(buffer, offset + 18);
            FileFlags = (FileFlags)buffer[offset + 25];
            InterleaveUnitSize = buffer[offset + 26];
            InterleaveGapSize = buffer[offset + 27];
            VolumeSequenceNumber = EndianUtilities.ToUInt16LittleEndian(buffer, offset + 28);
            //VolumeSequenceNumberBigEnd = EndianUtilities.ToUInt16BigEndian(buffer, offset + 30);
            NameLength = buffer[offset + 32];
            Name = Encoding.Default.GetString(buffer.Slice((offset + 33), (offset + 33 + NameLength)));
            SystemUseData = new byte[EntryLength - Size];

            return Size;
        }

        public void WriteTo(byte[] buffer, int offset)
        {
            throw new NotImplementedException();
        }

        public override bool Equals(object? obj)
        {
            if (obj is not null && obj is DirectoryEntry)
            {
                DirectoryEntry entry = (obj as DirectoryEntry)!;
                return EntryLocation == entry.EntryLocation
                    && EntrySize == entry.EntrySize
                    && VolumeSequenceNumber == entry.VolumeSequenceNumber;
            }
            return base.Equals(obj);
        }

        public override int GetHashCode() => base.GetHashCode();
    }
}
