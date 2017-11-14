using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace RTZParser
{
    internal class RTZBaseValueEntry
    {
        public UInt32 Hash;
        public UInt32 Index;

        public byte[] GetBytes()
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);
            bw.Write(Hash);
            bw.Write(Index);
            byte[] buffer = ms.ToArray();
            ms.Close();
            return buffer;
        }
    }
}
