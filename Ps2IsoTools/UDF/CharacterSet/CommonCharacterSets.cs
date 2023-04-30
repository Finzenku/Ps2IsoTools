namespace Ps2IsoTools.UDF.CharacterSet
{
    internal static class CommonCharacterSets
    {
        public static CharacterSetSpecification OSTACompressedUnicode { get; private set; }

        static CommonCharacterSets()
        {
            OSTACompressedUnicode = new CharacterSetSpecification("OSTA Compressed Unicode");
        }
    }
}
