using Ps2IsoTools.DiscUtils.Utils;
using Ps2IsoTools.UDF.OS;
using System.Globalization;

namespace Ps2IsoTools.UDF.EntityIdentifiers
{
    internal class UdfEntityIdentifier : EntityIdentifier
    {
        public override string ToString()
        {
            string major = ((uint)Suffix[1]).ToString("X", CultureInfo.InvariantCulture);
            string minor = ((uint)Suffix[0]).ToString("X", CultureInfo.InvariantCulture);
            OSClass osClass = (OSClass)Suffix[2];
            OSIdentifier osId = (OSIdentifier)EndianUtilities.ToUInt16BigEndian(Suffix, 2);
            return string.Format(CultureInfo.InvariantCulture, "{0} [UDF {1}.{2} : OS {3} {4}]", Identifier, major,
                minor, osClass, osId);
        }
    }
}
