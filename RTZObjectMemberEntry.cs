using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace RTZParser
{
    internal class RTZObjectMemberEntry
    {
        public UInt32 TypeHash;
        public UInt32 MemberNameHash;

        public override string ToString()
        {
            string tName = RTZDefaultType.GetName(TypeHash);
            if (tName == "")
            {
                return string.Format("t{0:X08} v{1:X08}", TypeHash, MemberNameHash);
            }
            else
            {
                return string.Format("{0} v{1:X08};", tName, MemberNameHash);
            }
        }

        public byte[] GetBytes()
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);
            bw.Write(MemberNameHash);
            bw.Write(TypeHash);
            byte[] buffer = ms.ToArray();
            ms.Close();
            return buffer;
        }
    }
}
