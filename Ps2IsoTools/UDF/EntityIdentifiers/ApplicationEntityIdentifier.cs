using Ps2IsoTools.DiscUtils.Utils;

namespace Ps2IsoTools.UDF.EntityIdentifiers
{
    internal class ApplicationEntityIdentifier : EntityIdentifier
    {
        public ApplicationEntityIdentifier() { }
        public ApplicationEntityIdentifier(string identifier, ushort udfRev = 0)
        {
            Identifier = identifier;
            EndianUtilities.WriteBytesLittleEndian(udfRev, Suffix, 0);
        }
        public override string ToString()
        {
            return $"{Identifier} {Suffix[1]:X}.{Suffix[0]:X2}";
        }
    }
}
