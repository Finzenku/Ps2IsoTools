using Ps2IsoTools.DiscUtils.Utils;
using Ps2IsoTools.Extensions;
using Ps2IsoTools.UDF.CharacterSet;
using Ps2IsoTools.UDF.EntityIdentifiers;
using Ps2IsoTools.UDF.Strings;
using Ps2IsoTools.UDF.Time;

namespace Ps2IsoTools.UDF.Descriptors.FileStructure
{
    internal class FileSetDescriptor : TaggedDescriptor<FileSetDescriptor>
    {
        public TimeStamp RecordingTimeStamp;
        public ushort InterchangeLevel;
        public ushort MaximumInterchangeLevel;
        public uint CharacterSetList;
        public uint MaximumCharacterSetList;
        public uint FileSetNumber;
        public uint FileSetDescriptorNumber;
        public CharacterSetSpecification LogicalVolumeIdentifierCharacterSet;
        public Dstring LogicalVolumeIdentifier;//128 bytes
        public CharacterSetSpecification FileSetCharacterSet;
        public Dstring FileSetIdentifier;//32 bytes
        public Dstring CopyrightFileIdentifier;//32 bytes
        public Dstring AbstractFileIdentifier;//32 bytes
        public LongAllocationDescriptor RootDirectoryICB;
        public EntityIdentifier DomainIdentifier;
        public LongAllocationDescriptor NextExtent;
        public LongAllocationDescriptor SystemStreamDirectoryICB;
        public byte[] Reserved = new byte[32];

        public FileSetDescriptor() : base(TagIdentifier.FileSetDescriptor) { }

        public FileSetDescriptor(uint sector, string volumeIdentifier, string fileSetIdentifier, LongAllocationDescriptor rootDirectoryICB, LongAllocationDescriptor? nextFileSetDescriptor = null, LongAllocationDescriptor? systemStreamDirectoryICB = null, string copyrightFileIdentifier = "", string abstractFileIdentifier = "") : base(TagIdentifier.FileSetDescriptor)
        {
            Tag = new DescriptorTag()
            {
                TagIdentifier = TagIdentifier.FileSetDescriptor,
                DescriptorCRCLength = (ushort)(Size - 16),
                TagLocation = sector,
            };
            RecordingTimeStamp = TimeStamp.FromDateTime(DateTime.UtcNow);
            InterchangeLevel = 3;
            MaximumInterchangeLevel = 3;
            CharacterSetList = 1;
            MaximumCharacterSetList = 1;
            FileSetNumber = 0;
            FileSetDescriptorNumber = 0;
            LogicalVolumeIdentifierCharacterSet = CommonCharacterSets.OSTACompressedUnicode;
            LogicalVolumeIdentifier = new Dstring(volumeIdentifier, CompressionID.UTF8, 128);
            FileSetCharacterSet = CommonCharacterSets.OSTACompressedUnicode;
            FileSetIdentifier = new Dstring(fileSetIdentifier, CompressionID.UTF8, 32);
            CopyrightFileIdentifier = new Dstring(copyrightFileIdentifier, CompressionID.UTF8, 32);
            AbstractFileIdentifier = new Dstring(abstractFileIdentifier, CompressionID.UTF8, 32);
            RootDirectoryICB = rootDirectoryICB;
            DomainIdentifier = CommonIdentifiers.OSTAUDFCompliant;
            NextExtent = nextFileSetDescriptor ?? new();
            SystemStreamDirectoryICB = systemStreamDirectoryICB ?? new();

            FixChecksums();
        }

        public override int Parse(byte[] buffer, int offset)
        {
            RecordingTimeStamp = EndianUtilities.ToStruct<TimeStamp>(buffer, offset + 16);
            InterchangeLevel = EndianUtilities.ToUInt16LittleEndian(buffer, offset + 28);
            MaximumInterchangeLevel = EndianUtilities.ToUInt16LittleEndian(buffer, offset + 30);
            CharacterSetList = EndianUtilities.ToUInt32LittleEndian(buffer, offset + 32);
            MaximumCharacterSetList = EndianUtilities.ToUInt32LittleEndian(buffer, offset + 36);
            FileSetNumber = EndianUtilities.ToUInt32LittleEndian(buffer, offset + 40);
            FileSetDescriptorNumber = EndianUtilities.ToUInt32LittleEndian(buffer, offset + 44);
            LogicalVolumeIdentifierCharacterSet = EndianUtilities.ToStruct<CharacterSetSpecification>(buffer, offset + 48);
            LogicalVolumeIdentifier = Dstring.FromBytes(buffer, offset + 112, 128);
            FileSetCharacterSet = EndianUtilities.ToStruct<CharacterSetSpecification>(buffer, offset + 240);
            FileSetIdentifier = Dstring.FromBytes(buffer, offset + 304, 32);
            CopyrightFileIdentifier = Dstring.FromBytes(buffer, offset + 336, 32);
            AbstractFileIdentifier = Dstring.FromBytes(buffer, offset + 368, 32);
            RootDirectoryICB = EndianUtilities.ToStruct<LongAllocationDescriptor>(buffer, offset + 400);
            DomainIdentifier = EndianUtilities.ToStruct<DomainEntityIdentifier>(buffer, offset + 416);
            NextExtent = EndianUtilities.ToStruct<LongAllocationDescriptor>(buffer, offset + 448);
            SystemStreamDirectoryICB = EndianUtilities.ToStruct<LongAllocationDescriptor>(buffer, offset + 464);
            Reserved = buffer.Slice((offset + 480), (offset + 512));

            return Size;
        }

        public override void Write(byte[] buffer, int offset)
        {
            RecordingTimeStamp.WriteTo(buffer, offset + 16);
            EndianUtilities.WriteBytesLittleEndian(InterchangeLevel, buffer, offset + 28);
            EndianUtilities.WriteBytesLittleEndian(MaximumInterchangeLevel, buffer, offset + 30);
            EndianUtilities.WriteBytesLittleEndian(CharacterSetList, buffer, offset + 32);
            EndianUtilities.WriteBytesLittleEndian(MaximumCharacterSetList, buffer, offset + 36);
            EndianUtilities.WriteBytesLittleEndian(FileSetNumber, buffer, offset + 40);
            EndianUtilities.WriteBytesLittleEndian(FileSetDescriptorNumber, buffer, offset + 44);
            LogicalVolumeIdentifierCharacterSet.WriteTo(buffer, offset + 48);
            LogicalVolumeIdentifier.WriteTo(buffer, offset + 112);
            FileSetCharacterSet.WriteTo(buffer, offset + 240);
            FileSetIdentifier.WriteTo(buffer, offset + 304);
            CopyrightFileIdentifier.WriteTo(buffer, offset + 336);
            AbstractFileIdentifier.WriteTo(buffer, offset + 368);
            RootDirectoryICB.WriteTo(buffer, offset + 400);
            DomainIdentifier.WriteTo(buffer, offset + 416);
            NextExtent.WriteTo(buffer, offset + 448);
            SystemStreamDirectoryICB.WriteTo(buffer, offset + 464);
            Array.Copy(Reserved, 0, buffer, offset + 480, Reserved.Length);
        }
    }
}
