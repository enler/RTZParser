using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace RTZParser
{
    internal class RTZStringObj
    {
        public UInt32 Hash;
        public String Value;
    }

    internal class RTZTypeEntry
    {
        public UInt32 Hash;
        public UInt32 Index;
        public List<RTZObjectMemberEntry> MemberList;
        public RTZTypeEntry InheritanceFrom;

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("class {0}", Name);
            if (InheritanceFrom != null)
            {
                sb.AppendFormat(": {0}", InheritanceFrom.Name);
            }
            sb.AppendLine();
            sb.AppendLine("{");
            foreach (var member in MemberList)
            {
                sb.AppendFormat("\t{0}", member.ToString());
                sb.AppendLine();
            }
            sb.AppendLine("};");
            return sb.ToString();
        }

        public string Name
        {
            get
            {
                string tName = RTZDefaultType.GetName(Hash);
                if (tName == "")
                    return "c" + Hash.ToString("X08");
                else
                    return tName;
            }
        }

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

        public override bool Equals(object obj)
        {
            var tmp = obj as RTZTypeEntry;
            if (tmp == null) return false;
            bool result0 = tmp.Hash == this.Hash;
            bool result1 = tmp.MemberList.Count == this.MemberList.Count;
            if (result1)
            {
                for (int i = 0; i < tmp.MemberList.Count && i < this.MemberList.Count; i++)
                {
                    result1 = result1 && tmp.MemberList[i].MemberNameHash == this.MemberList[i].MemberNameHash && tmp.MemberList[i].TypeHash == this.MemberList[i].TypeHash;
                }
            }
            if (!result0 || !result1) return false;
            bool result2;
            if (this.InheritanceFrom == null && tmp.InheritanceFrom == null)
                result2 = true;
            else if (this.InheritanceFrom != null && tmp.InheritanceFrom != null)
            {
                return this.InheritanceFrom.Equals(tmp.InheritanceFrom);
            }
            else
                result2 = false;
            return result0 && result1 && result2;
        }
    }
}
