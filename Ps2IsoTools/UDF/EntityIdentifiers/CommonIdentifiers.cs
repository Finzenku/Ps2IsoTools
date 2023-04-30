using Ps2IsoTools.DiscUtils.Utils;
using System.Reflection;

namespace Ps2IsoTools.UDF.EntityIdentifiers
{
    internal static class CommonIdentifiers
    {
        public static DomainEntityIdentifier OSTAUDFCompliant { get; private set; }
        public static ApplicationEntityIdentifier CurrentApplicationIdentifier { get; private set; }
        public static ImplementationEntityIdentifier UDFLVInfoIdentifier { get; private set; }
        public static ImplementationEntityIdentifier UDFFreeEASpaceIdentifier { get; private set; }
        public static ImplementationEntityIdentifier UDFDVDCGMSInfoIdentifier { get; private set; }
        public static ApplicationEntityIdentifier NSR02Identifier { get; private set; }
        public static ImplementationEntityIdentifier Ps2IsoToolsImplementation { get; private set; }

        static CommonIdentifiers()
        {
            OSTAUDFCompliant = new DomainEntityIdentifier("*OSTA UDF Compliant", UdfUtilities.UDFCurrent, DomainFlags.HardWriteProtect | DomainFlags.SoftWriteProtect);
            CurrentApplicationIdentifier = ReflectionApplicationIdentifier();
            UDFLVInfoIdentifier = new ImplementationEntityIdentifier("*UDF LV Info", UdfUtilities.UDFCurrent);
            UDFFreeEASpaceIdentifier = new ImplementationEntityIdentifier("*UDF FreeEASpace", UdfUtilities.UDFCurrent);
            UDFDVDCGMSInfoIdentifier = new ImplementationEntityIdentifier("*UDF DVD CGMS Info", UdfUtilities.UDFCurrent);
            NSR02Identifier = new ApplicationEntityIdentifier("+NSR02");
            Ps2IsoToolsImplementation = new ImplementationEntityIdentifier($"*{typeof(CommonIdentifiers).Assembly.GetName().Name} v{typeof(CommonIdentifiers).Assembly.GetName().Version}");
        }

        private static ApplicationEntityIdentifier ReflectionApplicationIdentifier()
        {
            var assembly = Assembly.GetEntryAssembly();
            string identifier;
            if (assembly is null)
                identifier = "*Unmanaged Application";
            else
                identifier = $"*{assembly.GetName().Name} v{assembly.GetName().Version}";

            return new ApplicationEntityIdentifier(identifier);
        }
    }
}
