using Ps2IsoTools.DiscUtils.Utils;
using Ps2IsoTools.UDF.EntityIdentifiers;

namespace Ps2IsoTools.UDF.Descriptors.VolumeStructure
{
    internal class LogicalVolumeIntegrityImplementationUse : IByteArraySerializable
    {
        public EntityIdentifier ImplementationID { get; set; }
        public uint NumberOfFiles { get; set; }
        public uint NumberOfDirectories { get; set; }
        public ushort MinimumUDFReadRevision { get; set; }
        public ushort MinimumUDFWriteRevision { get; set; }
        public ushort MaximumUDFWriteRevision { get; set; }
        public byte[] ImplementationUse { get; set; }

        public int Size => 46 + ImplementationUse.Length;

        // Should prob'ly put this default into a static common class using a different ctor but am lazy
        public LogicalVolumeIntegrityImplementationUse() 
        {
            ImplementationID = CommonIdentifiers.Ps2IsoToolsImplementation;
            MinimumUDFReadRevision = UdfUtilities.UDFMin;
            MinimumUDFWriteRevision = UdfUtilities.UDFMin;
            MaximumUDFWriteRevision = UdfUtilities.UDFCurrent;
            ImplementationUse = new byte[0];
        }

        public LogicalVolumeIntegrityImplementationUse(uint lengthOfImplementationUse)
        {
            if (lengthOfImplementationUse > 46)
                ImplementationUse = new byte[lengthOfImplementationUse - 46];
            else
                ImplementationUse = new byte[0];
        }

        public LogicalVolumeIntegrityImplementationUse(ImplementationEntityIdentifier entityIdentifier, byte[] implementationUse)
        {
            ImplementationID = entityIdentifier;
            MinimumUDFReadRevision = UdfUtilities.UDFMin;
            MinimumUDFWriteRevision = UdfUtilities.UDFMin;
            MaximumUDFWriteRevision = UdfUtilities.UDFCurrent;
            ImplementationUse = implementationUse;
        }

        public int ReadFrom(byte[] buffer, int offset)
        {
            ImplementationID = EndianUtilities.ToStruct<ImplementationEntityIdentifier>(buffer, offset);
            NumberOfFiles = EndianUtilities.ToUInt32LittleEndian(buffer, offset + 32);
            NumberOfDirectories = EndianUtilities.ToUInt32LittleEndian(buffer, offset + 36);
            MinimumUDFReadRevision = EndianUtilities.ToUInt16LittleEndian(buffer, offset + 40);
            MinimumUDFWriteRevision = EndianUtilities.ToUInt16LittleEndian(buffer, offset + 42);
            MaximumUDFWriteRevision = EndianUtilities.ToUInt16LittleEndian(buffer, offset + 44);
            ImplementationUse = EndianUtilities.ToByteArray(buffer, offset +  46, ImplementationUse.Length);

            return Size;
        }

        public void WriteTo(byte[] buffer, int offset)
        {
            if (buffer.Length < offset + Size)
                throw new InvalidDataException("The size of the buffer was too small to write to");

            ImplementationID.WriteTo(buffer, offset);
            EndianUtilities.WriteBytesLittleEndian(NumberOfFiles, buffer, offset + 32);
            EndianUtilities.WriteBytesLittleEndian(NumberOfDirectories, buffer, offset + 36);
            EndianUtilities.WriteBytesLittleEndian(MinimumUDFReadRevision, buffer, offset + 40);
            EndianUtilities.WriteBytesLittleEndian(MinimumUDFWriteRevision, buffer, offset + 42);
            EndianUtilities.WriteBytesLittleEndian(MaximumUDFWriteRevision, buffer, offset + 44);
            Array.Copy(ImplementationUse, 0, buffer, offset + 46, ImplementationUse.Length);
        }
    }
}
