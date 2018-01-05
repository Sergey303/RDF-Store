using System.Text;

namespace RDFCommon.Hashes
{
    public static class ModifiedBernsteinHash
    {

        public static uint GetHashModifiedBernstein(this string s, int position = 0, int length = 4)
        {
            return ComputeHash(Encoding.UTF32.GetBytes(s), position, length);
        }
        public static uint ComputeHash(this byte[] dataBytes, int position=0, int length=4)
        {
            uint h=0;
            for (int i = position; i < (position + length); i++)
            {
                h = (0x21 * h) ^ dataBytes[i];
            }
            return h;
        }

    }
}
