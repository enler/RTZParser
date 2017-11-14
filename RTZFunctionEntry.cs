using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace RTZParser
{
    internal class RTZFunctionEntry
    {
        public UInt32 Hash;
        public UInt32 ParameterNumber;
        public UInt32 Num1;
        public byte[] InstructionBuffer;

        public byte[] GetBytes()
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);
            bw.Write(BitConverter.GetBytes(Hash));
            bw.Write((byte)0);
            bw.Write(RTZFile.EncodeNum(ParameterNumber));
            bw.Write(RTZFile.EncodeNum(Num1));
            bw.Write(RTZFile.EncodeNum((UInt32)InstructionBuffer.Length));
            bw.Write(InstructionBuffer);
            byte[] buffer = ms.ToArray();
            ms.Close();
            return buffer;
        }
    }
}
