using Ps2IsoTools.DiscUtils.Utils;
using Ps2IsoTools.Extensions;
using Ps2IsoTools.UDF.CharacterSet;
using Ps2IsoTools.UDF.EntityIdentifiers;
using Ps2IsoTools.UDF.Strings;

namespace Ps2IsoTools.UDF.Descriptors
{
    internal class LVInformation : IByteArraySerializable
    {
        public CharacterSetSpecification LVICharset;
        public Dstring LogicalVolumeIdentifier;//128 bytes
        public Dstring LVInfo1;//36 bytes
        public Dstring LVInfo2;//36 bytes
        public Dstring LVInfo3;//36 bytes
        public EntityIdentifier ImplementationID;
        public byte[] ImplementationUse = new byte[128];

        public int Size => 460;

        public LVInformation() { }
        public LVInformation(string identifier, string info1 = "", string info2 = "", string info3 = "")
        {
            LVICharset = CommonCharacterSets.OSTACompressedUnicode;
            LogicalVolumeIdentifier = new Dstring(identifier, CompressionID.UTF8, 128);
            LVInfo1 = new Dstring(info1, CompressionID.UTF8, 36);
            LVInfo2 = new Dstring(info2, CompressionID.UTF8, 36);
            LVInfo3 = new Dstring(info3, CompressionID.UTF8, 36);
            ImplementationID = CommonIdentifiers.CurrentApplicationIdentifier;
        }

        public int ReadFrom(byte[] buffer, int offset)
        {
            LVICharset = EndianUtilities.ToStruct<CharacterSetSpecification>(buffer, offset);
            LogicalVolumeIdentifier = Dstring.FromBytes(buffer, offset + 64, 128);// UdfUtilities.ReadDString(buffer, offset + 64, 128);
            LVInfo1 = Dstring.FromBytes(buffer, offset + 192, 36);
            LVInfo2 = Dstring.FromBytes(buffer, offset + 228, 36);
            LVInfo3 = Dstring.FromBytes(buffer, offset + 264, 36);
            ImplementationID = EndianUtilities.ToStruct<ApplicationEntityIdentifier>(buffer, offset + 300);
            ImplementationUse = buffer.Slice((offset + 332), (offset + 460));

            return Size;
        }

        public void WriteTo(byte[] buffer, int offset)
        {
            if (buffer.Length < offset + Size)
                throw new InvalidDataException("The size of the buffer was too small to write to");

            LVICharset.WriteTo(buffer, offset);
            LogicalVolumeIdentifier.WriteTo(buffer, offset + 64);
            LVInfo1.WriteTo(buffer, offset + 192);
            LVInfo2.WriteTo(buffer, offset + 228);
            LVInfo3.WriteTo(buffer, offset + 264);
            ImplementationID.WriteTo(buffer, offset + 300);
            Array.Copy(ImplementationUse, 0, buffer, offset + 332, ImplementationUse.Length);
        }
    }
}
