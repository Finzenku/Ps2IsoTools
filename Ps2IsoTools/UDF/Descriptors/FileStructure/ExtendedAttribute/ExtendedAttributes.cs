using Ps2IsoTools.DiscUtils.Utils;
using Ps2IsoTools.Extensions;
using Ps2IsoTools.UDF.EntityIdentifiers;

namespace Ps2IsoTools.UDF.Descriptors.FileStructure.ExtendedAttribute
{
    internal class ExtendedAttributes : IByteArraySerializable
    {
        public ExtendedAttributeHeaderDescriptor HeaderDescriptor { get; set; }
        public List<ExtendedAttributeRecord> ImplementationAttributes { get; set; }
        public List<ExtendedAttributeRecord> ApplicationAttributes { get; set; }

        public List<ExtendedAttributeRecord> Attributes => new List<ExtendedAttributeRecord>(ImplementationAttributes.Concat(ApplicationAttributes));

        private int _size;
        public int Size { get => _size; private set => _size = value; }

        private int listsSize => ImplementationAttributes.Concat(ApplicationAttributes).Select(atr => atr.Size).Sum();

        public ExtendedAttributes() { }

        public ExtendedAttributes(int size)
        {
            Size = size;
        }

        public ExtendedAttributes(uint sector)
        {
            ImplementationAttributes = new();
            ApplicationAttributes = new();
            ImplementationAttributes.Add(new ImplementationUseExtendedAttributeRecord(CommonIdentifiers.UDFFreeEASpaceIdentifier, new byte[4] { 0x61, 0x05, 0x0, 0x0 }));
            ApplicationAttributes.Add(new ImplementationUseExtendedAttributeRecord(CommonIdentifiers.UDFDVDCGMSInfoIdentifier, new byte[8] { 0x49, 0x05, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0 }));
            HeaderDescriptor = new(sector, 24, (uint)ImplementationAttributes.Select(impAtr => impAtr.Size).Sum() + 24);
            Size = HeaderDescriptor.Size + listsSize;
        }

        public ExtendedAttributes(uint sector, List<ExtendedAttributeRecord> implementationAttributes, List<ExtendedAttributeRecord> applicationAttributes)
        {
            ImplementationAttributes = implementationAttributes;
            ApplicationAttributes = applicationAttributes;
            HeaderDescriptor = new(sector, 24, (uint)ImplementationAttributes.Select(impAtr => impAtr.Size).Sum() + 24);
            Size = HeaderDescriptor.Size + listsSize;
        }

        public int ReadFrom(byte[] buffer, int offset)
        {
            if (Size < 24)
                return 0;

            HeaderDescriptor = EndianUtilities.ToStruct<ExtendedAttributeHeaderDescriptor>(buffer, offset);
            if (HeaderDescriptor.ImplementationAttributesLocation < Size)
            {
                var impData = buffer.Slice((int)(offset + HeaderDescriptor.ImplementationAttributesLocation), (int)(offset + HeaderDescriptor.ApplicationAttributesLocation));
                ImplementationAttributes = ReadExtendedAttributesList(impData, true);
            }
            else
                ImplementationAttributes = new();
            if (HeaderDescriptor.ApplicationAttributesLocation < Size)
            {
                var appData = buffer.Slice((int)(offset + HeaderDescriptor.ApplicationAttributesLocation), (offset + Size - 1));
                ImplementationAttributes = ReadExtendedAttributesList(appData, false);
            }
            else
                ApplicationAttributes = new();

            return Size;
        }

        public void WriteTo(byte[] buffer, int offset)
        {
            if (buffer.Length < offset + Size)
                throw new InvalidDataException("The size of the buffer was too small to write to");
            HeaderDescriptor.WriteTo(buffer, offset);
            int atrOffset = 0;
            foreach (ExtendedAttributeRecord ea in Attributes)
            {
                ea.WriteTo(buffer, offset + 24 + atrOffset);
                atrOffset += ea.Size;
            }
        }

        public static ExtendedAttributes ReadExtendedAttributes(byte[] eaData)
        {
            if (eaData != null && eaData.Length != 0)
            {
                ExtendedAttributeHeaderDescriptor EAHD = EndianUtilities.ToStruct<ExtendedAttributeHeaderDescriptor>(eaData, 0);

                List<ExtendedAttributeRecord> impAtrs = new();
                int pos = (int)EAHD.ImplementationAttributesLocation;
                while (pos < (int)EAHD.ApplicationAttributesLocation)
                {
                    ExtendedAttributeRecord ea = new ImplementationUseExtendedAttributeRecord();
                    pos += ea.ReadFrom(eaData, pos);
                    impAtrs.Add(ea);
                }

                List<ExtendedAttributeRecord> appAtrs = new();
                pos = (int)EAHD.ApplicationAttributesLocation;
                while (pos < eaData.Length)
                {
                    ExtendedAttributeRecord ea = new();
                    pos += ea.ReadFrom(eaData, pos);
                    appAtrs.Add(ea);
                }
                ExtendedAttributes extendedAttributes = new()
                {
                    HeaderDescriptor = EAHD,
                    ImplementationAttributes = impAtrs,
                    ApplicationAttributes = appAtrs,
                    Size = eaData.Length
                };
                return extendedAttributes;
            }
            else
                throw new ArgumentException("The value of eaData was either null or of length 0");
        }

        protected static List<ExtendedAttributeRecord> ReadExtendedAttributesList(byte[] eaData, bool implementationAttributes = true)
        {
            List<ExtendedAttributeRecord> atrs = new();

            int pos = 0;
            while (pos < eaData.Length)
            {
                ExtendedAttributeRecord ea;
                if (implementationAttributes)
                    ea = new ImplementationUseExtendedAttributeRecord();
                else
                    ea = new ExtendedAttributeRecord();
                pos += ea.ReadFrom(eaData, pos);
                atrs.Add(ea);
            }
            return atrs;
        }
    }
}
