namespace Ps2IsoTools.Extensions
{
    internal static class ArrayExtensions
    {
        public static byte[] Slice(this byte[] source, int start, int end)
        {
            var len = end - start;
            var result = new byte[len];
            Array.Copy(source, start, result, 0, len);
            return result;
        }
    }
}
