using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace RTZParser
{
    internal class RTZImportFunctionEntry
    {
        public UInt32 Hash;
        public bool NotImported;
        public RTZFunctionEntry Data;

        public byte[] GetBytes()
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);
            bw.Write(BitConverter.GetBytes(Hash));
            if (NotImported)
            {
                bw.Write((byte)1);
                bw.Write(Data.GetBytes());
            }
            else
            {
                bw.Write((byte)0);
            }
            byte[] buffer = ms.ToArray();
            ms.Close();
            return buffer;
        }

        public override bool Equals(object obj)
        {
            var tmp = obj as RTZImportFunctionEntry;
            if (tmp == null) return false;
            bool result0 = tmp.Hash == this.Hash;
            bool result1 = tmp.NotImported == this.NotImported;
            if (!tmp.NotImported || !this.NotImported) return result0 && result1;
            bool result2 = tmp.Data.Hash == this.Data.Hash;
            bool result3 = tmp.Data.ParameterNumber == this.Data.ParameterNumber;
            bool result4 = tmp.Data.Num1 == this.Data.Num1;
            return result0 && result1 && result2 && result3 && result4;
        }
    }
}
