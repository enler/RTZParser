using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RTZParser
{
    class CRC32
    {
        private static UInt32[] crc32Table;

        static CRC32()
        {
            crc32Table = new UInt32[256];
            for (int i = 0; i < crc32Table.Length; i++)
            {
                UInt32 temp = (UInt32)i;
                for (int j = 0; j < 8; j++)
                {
                    if ((temp & 1) != 0)
                    {
                        temp = 0xEDB88320 ^ (temp >> 1);
                    }
                    else
                    {
                        temp = temp >> 1;
                    }
                }
                crc32Table[i] = temp;
            }
        }

        public static UInt32 CalcCRC32(UInt32 initial, byte[] buffer)
        {
            for (int i = 0; i < buffer.Length; i++)
            {
                initial = (initial >> 8) ^ crc32Table[buffer[i] ^ (initial & 0xFF)];
            }
            return ~initial;
        }

        public static UInt32 CalcCRC32(byte[] buffer)
        {
            return CalcCRC32(0xFFFFFFFF, buffer);
        }
    }
}
