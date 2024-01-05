using Ps2IsoTools.DiscUtils.Utils;
using Ps2IsoTools.Extensions;
using Ps2IsoTools.UDF.CharacterSet;
using Ps2IsoTools.UDF.EntityIdentifiers;
using Ps2IsoTools.UDF.Strings;
using Ps2IsoTools.UDF.Time;

namespace Ps2IsoTools.UDF.Descriptors
{
    internal class PrimaryVolumeDescriptor : TaggedDescriptor<PrimaryVolumeDescriptor>
    {
        public uint VolumeDescriptorSequenceNumber;
        public uint PrimaryVolumeDescriptorNumber;
        public Dstring VolumeIdentifier;//32 bytes
        public ushort VolumeSequenceNumber;
        public ushort MaximumVolumeSequenceNumber;
        public ushort InterchangeLevel;
        public ushort MaximumInterchangeLevel;
        public uint CharacterSetList;
        public uint MaximumCharacterSetList;
        public Dstring VolumeSetIdentifier;//128 bytes
        public CharacterSetSpecification DescriptorCharacterSet;
        public CharacterSetSpecification ExplanatoryCharacterSet;
        public ExtentDescriptor VolumeAbstract;
        public ExtentDescriptor VolumeCopyrightNotice;
        public EntityIdentifier ApplicationIdentifier;
        public TimeStamp RecordingTimeStamp;
        public EntityIdentifier ImplementationIdentifier;
        public byte[] ImplementationUse = new byte[64];
        public uint PredecessorVolumeDescriptorSequenceLocation;
        public ushort Flags;
        public byte[] Reserved = new byte[22];

        public PrimaryVolumeDescriptor() : base(TagIdentifier.PrimaryVolumeDescriptor) { }

        public PrimaryVolumeDescriptor(uint sector, uint descriptorSequenceNumber, string volumeIdentifier) : base(TagIdentifier.PrimaryVolumeDescriptor)
        {
            Tag = new DescriptorTag()
            {
                TagIdentifier = TagIdentifier.PrimaryVolumeDescriptor,
                DescriptorCRCLength = (ushort)(Size - 16),
                TagLocation = sector,
            };
            VolumeDescriptorSequenceNumber = descriptorSequenceNumber;
            PrimaryVolumeDescriptorNumber = 0;
            VolumeIdentifier = new Dstring(volumeIdentifier, CompressionID.UTF8, 32);
            VolumeSequenceNumber = 1;
            MaximumVolumeSequenceNumber = 1;
            InterchangeLevel = 2;
            MaximumInterchangeLevel = 2;
            CharacterSetList = 1;
            MaximumCharacterSetList = 1;
            string setId = $"{DateTimeOffset.UtcNow.ToUnixTimeSeconds():X8}{volumeIdentifier}";
            VolumeSetIdentifier = new Dstring(setId, CompressionID.UTF16, 128);
            DescriptorCharacterSet = CommonCharacterSets.OSTACompressedUnicode;
            ExplanatoryCharacterSet = CommonCharacterSets.OSTACompressedUnicode;
            VolumeAbstract = new ExtentDescriptor();
            VolumeCopyrightNotice = new ExtentDescriptor();
            ApplicationIdentifier = CommonIdentifiers.CurrentApplicationIdentifier;
            RecordingTimeStamp = TimeStamp.FromDateTime(DateTime.UtcNow);
            ImplementationIdentifier = CommonIdentifiers.Ps2IsoToolsImplementation;
            PredecessorVolumeDescriptorSequenceLocation = 0;
            Flags = 0;
            FixChecksums();
        }

        public override int Parse(byte[] buffer, int offset)
        {
            VolumeDescriptorSequenceNumber = EndianUtilities.ToUInt32LittleEndian(buffer, offset + 16);
            PrimaryVolumeDescriptorNumber = EndianUtilities.ToUInt32LittleEndian(buffer, offset + 20);
            VolumeIdentifier = Dstring.FromBytes(buffer, offset + 24, 32);
            VolumeSequenceNumber = EndianUtilities.ToUInt16LittleEndian(buffer, offset + 56);
            MaximumVolumeSequenceNumber = EndianUtilities.ToUInt16LittleEndian(buffer, offset + 58);
            InterchangeLevel = EndianUtilities.ToUInt16LittleEndian(buffer, offset + 60);
            MaximumInterchangeLevel = EndianUtilities.ToUInt16LittleEndian(buffer, offset + 62);
            CharacterSetList = EndianUtilities.ToUInt32LittleEndian(buffer, offset + 64);
            MaximumCharacterSetList = EndianUtilities.ToUInt32LittleEndian(buffer, offset + 68);
            VolumeSetIdentifier = Dstring.FromBytes(buffer, offset + 72, 128);
            DescriptorCharacterSet = EndianUtilities.ToStruct<CharacterSetSpecification>(buffer, offset + 200);
            ExplanatoryCharacterSet = EndianUtilities.ToStruct<CharacterSetSpecification>(buffer, offset + 264);
            VolumeAbstract = EndianUtilities.ToStruct<ExtentDescriptor>(buffer, offset + 328);
            VolumeCopyrightNotice = EndianUtilities.ToStruct<ExtentDescriptor>(buffer, offset + 336);
            ApplicationIdentifier = EndianUtilities.ToStruct<ApplicationEntityIdentifier>(buffer, offset + 344);
            RecordingTimeStamp = EndianUtilities.ToStruct<TimeStamp>(buffer, offset + 376);
            ImplementationIdentifier = EndianUtilities.ToStruct<ImplementationEntityIdentifier>(buffer, offset + 388);
            ImplementationUse = buffer.Slice((offset + 420), (offset + 484));
            PredecessorVolumeDescriptorSequenceLocation = EndianUtilities.ToUInt32LittleEndian(buffer, offset + 484);
            Flags = EndianUtilities.ToUInt16LittleEndian(buffer, offset + 488);

            return Size;
        }

        public override void Write(byte[] buffer, int offset)
        {
            EndianUtilities.WriteBytesLittleEndian(VolumeDescriptorSequenceNumber, buffer, offset + 16);
            EndianUtilities.WriteBytesLittleEndian(PrimaryVolumeDescriptorNumber, buffer, offset + 20);
            VolumeIdentifier.WriteTo(buffer, offset + 24);
            EndianUtilities.WriteBytesLittleEndian(VolumeSequenceNumber, buffer, offset + 56);
            EndianUtilities.WriteBytesLittleEndian(MaximumVolumeSequenceNumber, buffer, offset + 58);
            EndianUtilities.WriteBytesLittleEndian(InterchangeLevel, buffer, offset + 60);
            EndianUtilities.WriteBytesLittleEndian(MaximumInterchangeLevel, buffer, offset + 62);
            EndianUtilities.WriteBytesLittleEndian(CharacterSetList, buffer, offset + 64);
            EndianUtilities.WriteBytesLittleEndian(MaximumCharacterSetList, buffer, offset + 68);
            VolumeSetIdentifier.WriteTo(buffer, offset + 72);
            DescriptorCharacterSet.WriteTo(buffer, offset + 200);
            ExplanatoryCharacterSet.WriteTo(buffer, offset + 264);
            VolumeAbstract.WriteTo(buffer, offset + 328);
            VolumeCopyrightNotice.WriteTo(buffer, offset + 336);
            ApplicationIdentifier.WriteTo(buffer, offset + 344);
            RecordingTimeStamp.WriteTo(buffer, offset + 376);
            ImplementationIdentifier.WriteTo(buffer, offset + 388);
            Array.Copy(ImplementationUse, 0, buffer, 420, ImplementationUse.Length);
            EndianUtilities.WriteBytesLittleEndian(PredecessorVolumeDescriptorSequenceLocation, buffer, offset + 484);
            EndianUtilities.WriteBytesLittleEndian(Flags, buffer, offset + 488);
            //Array.Copy(Reserved, 0, buffer, 490, Reserved.Length);
        }
    }
}
