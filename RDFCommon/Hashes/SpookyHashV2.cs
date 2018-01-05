using System;
using System.Collections.Generic;
using System.Text;

namespace RDFCommon.Hashes
{
    public static class SpookyHashV2
    {
        private static readonly IReadOnlyList<int> EndPartialRotationParameters = new int[] { 0x2c, 15, 0x22, 0x15, 0x26, 0x21, 10, 13, 0x26, 0x35, 0x2a, 0x36 };
        private static readonly IReadOnlyList<int> MixRotationParameters = new int[] { 11, 0x20, 0x2b, 0x1f, 0x11, 0x1c, 0x27, 0x39, 0x37, 0x36, 0x16, 0x2e };

        public static uint GetHashSpooky(this string s)
        {
            return BitConverter.ToUInt32(ComputeHash(Encoding.UTF32.GetBytes(s)),0);
        }

        public static byte[] ComputeHash(byte[] data, int hashSize=32, ulong initVal1=0l, ulong initVal2=0l)
        {

            ulong num;
            ulong num2;
            ulong num4;
            ulong num5;
            ulong num7;
            ulong num8;
            ulong[] h = new ulong[12];
            h[9] = num = initVal1;
            h[6] = num2 = num;
            h[0] = h[3] = num2;
            h[10] = num4 = (hashSize == 0x80) ? initVal2 : initVal1;
            h[7] = num5 = num4;
            h[1] = h[4] = num5;
            h[11] = num7 = 16045690984833335023L;
            h[8] = num8 = num7;
            h[2] = h[5] = num8;
            byte[] remainderData = new byte[0x60];
            ForEachGroup(0x60, delegate (byte[] dataGroup, int position, int length) {
                Mix(h, dataGroup, position, length);
            }, delegate (byte[] remainder, int position, int length) {
                Array.Copy(remainder, position, remainderData, 0, length);
                remainderData[0x5f] = (byte)length;
            }, data);
            End(h, remainderData, 0);
            byte[] array = null;
            switch (hashSize)
            {
                case 0x20:
                    return BitConverter.GetBytes((uint)h[0]);

                case 0x40:
                    return BitConverter.GetBytes(h[0]);

                case 0x80:
                    array = new byte[0x10];
                    BitConverter.GetBytes(h[0]).CopyTo(array, 0);
                    BitConverter.GetBytes(h[1]).CopyTo(array, 8);
                    return array;
            }
            return array;
        }

      
        private static void End(ulong[] h, byte[] data, int position)
        {
            for (int i = 0; i < 12; i++)
            {
                h[i] += BitConverter.ToUInt64(data, position + (i * 8));
            }
            EndPartial(h);
            EndPartial(h);
            EndPartial(h);
        }

        private static void EndPartial(ulong[] h)
        {
            for (int i = 0; i < 12; i++)
            {
                h[(i + 11) % 12] += h[(i + 1) % 12];
                h[(i + 2) % 12] ^= h[(i + 11) % 12];
                h[(i + 1) % 12] =RotateLeft(h[(i + 1) % 12], EndPartialRotationParameters[i]);
            }
        }
          
        private static void Mix(ulong[] h, byte[] data, int position, int length)
        {
            for (int i = position; i < (position + length); i += 0x60)
            {
                for (int j = 0; j < 12; j++)
                {
                    h[j] += BitConverter.ToUInt64(data, i + (j * 8));
                    h[(j + 2) % 12] ^= h[(j + 10) % 12];
                    h[(j + 11) % 12] ^= h[j];
                    h[j] =RotateLeft(h[j],MixRotationParameters[j]);
                    h[(j + 11) % 12] += h[(j + 1) % 12];
                }
            }
        }
        public static void ForEachGroup(int groupSize, Action<byte[], int, int> action, Action<byte[], int, int> remainderAction, byte[] data)
        {
            if (groupSize <= 0)
            {
                throw new ArgumentOutOfRangeException("groupSize", "bufferSize must be greater than 0.");
            }
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }
            int num = data.Length % groupSize;
            if ((data.Length - num) > 0)
            {
                action(data, 0, data.Length - num);
            }
            if ((remainderAction != null) && (num > 0))
            {
                remainderAction(data, data.Length - num, num);
            }
        }


        public static ulong RotateLeft(ulong operand, int shiftCount)
        {
            shiftCount &= 0x3f;
            return ((operand << shiftCount) | (operand >> (0x40 - shiftCount)));
        }


    }
}