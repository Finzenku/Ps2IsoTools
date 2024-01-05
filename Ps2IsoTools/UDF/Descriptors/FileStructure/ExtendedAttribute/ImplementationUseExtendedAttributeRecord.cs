using Ps2IsoTools.DiscUtils.Utils;
using Ps2IsoTools.Extensions;
using Ps2IsoTools.UDF.EntityIdentifiers;

namespace Ps2IsoTools.UDF.Descriptors.FileStructure.ExtendedAttribute
{
    internal class ImplementationUseExtendedAttributeRecord : ExtendedAttributeRecord
    {
        public ImplementationEntityIdentifier ImplementationIdentifier;
        public byte[] ImplementationUseData;
        private int iuSize;

        public ImplementationUseExtendedAttributeRecord() { }

        public ImplementationUseExtendedAttributeRecord(ImplementationEntityIdentifier implementationEntityIdentifier, byte[] implementationUse)
        {
            AttributeType = 0x800;
            AttributeSubType = 0x1;
            ImplementationIdentifier = implementationEntityIdentifier;
            ImplementationUseData = implementationUse;
            iuSize = ImplementationUseData.Length;
            recordLength = (uint)(48 + iuSize);
        }

        public override int ReadFrom(byte[] buffer, int offset)
        {
            int read = base.ReadFrom(buffer, offset);

            iuSize = EndianUtilities.ToInt32LittleEndian(buffer, offset + 12);

            ImplementationIdentifier = EndianUtilities.ToStruct<ImplementationEntityIdentifier>(buffer, offset + 16);
            ImplementationUseData = buffer.Slice((offset + 48), (offset + 48 + iuSize));

            return read;
        }

        public override void WriteTo(byte[] buffer, int offset)
        {
            base.WriteTo(buffer, offset);
            EndianUtilities.WriteBytesLittleEndian(iuSize, buffer, offset + 12);
            ImplementationIdentifier.WriteTo(buffer, offset + 16);
            Array.Copy(ImplementationUseData, 0, buffer, offset + 48, ImplementationUseData.Length);
        }
    }
}
