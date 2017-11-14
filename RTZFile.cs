using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace RTZParser
{
    internal class RTZFile
    {
        public static Dictionary<UInt32, string> StringTable = new Dictionary<uint, string>();

        public object UnknownHeader;//zero escape 3 does not use this

        public List<string> IncludePathList = new List<string>();
        public List<RTZTypeEntry> TypeTable = new List<RTZTypeEntry>();

        public List<RTZImportFunctionEntry> FunctionTable = new List<RTZImportFunctionEntry>();
        public RTZFunctionEntry MainFunction;

        public UInt32 EVarLength;
        public UInt32 GVarLength;

        public List<RTZBaseValueEntry> EVarTable = new List<RTZBaseValueEntry>();
        public List<RTZBaseValueEntry> GVarTable = new List<RTZBaseValueEntry>();

        public List<RTZStringObj> StringObjTableUTF8 = new List<RTZStringObj>();
        public List<RTZStringObj> StringObjTableUTF16 = new List<RTZStringObj>();

        public static UInt32 DecodeNum(byte[] buffer, int offset, out int len)
        {
            UInt32 num = 0;
            len = 1;
            if (buffer[offset] == 0xFC)
            {
                num = (UInt32)(buffer[offset + 2] << 8 | buffer[offset + 1]);
                len = 3;
            }
            else
                num = buffer[offset];
            return num;
        }

        public static byte[] EncodeNum(UInt32 num)
        {
            byte[] buffer = null;
            if (num < 0xFC)
            {
                return new byte[] { (byte)num };
            }
            else if (num < 0x10000)
            {
                buffer = new byte[3];
                buffer[0] = 0xFC;
                Array.Copy(BitConverter.GetBytes((ushort)num), 0, buffer, 1, 2);
                return buffer;
            }
            else
                throw new NotSupportedException();
        }

        private ushort ConvertEndian(ushort num)
        {
            return (ushort)(num << 8 | num >> 8);
        }

        private int LoadHeader(byte[] buffer, int offset)
        {
            int outLen = 0;
            int numLen;
            int headerLen = (int)DecodeNum(buffer, offset, out numLen);
            outLen += numLen;
            if (headerLen == 0)
                return outLen;
            else
                throw new NotSupportedException();
        }

        private int LoadIncludePath(byte[] buffer, int offset)
        {
            int numLen;
            int length = 0;
            MemoryStream ms = new MemoryStream(buffer);
            BinaryReader br = new BinaryReader(ms);
            int strNr = (int)DecodeNum(buffer, offset, out numLen);
            length += numLen;
            offset += numLen;
            for (int i = 0; i < strNr; i++)
            {
                int strLen = (int)DecodeNum(buffer, offset, out numLen);
                offset += numLen;
                byte[] strBuf = new byte[strLen];
                ms.Seek(offset, SeekOrigin.Begin);
                ms.Read(strBuf, 0, strLen);
                IncludePathList.Add(Encoding.UTF8.GetString(strBuf));
                length += strLen + numLen;
                offset += strLen;
            }
            ms.Close();
            return length;
        }

        private int LoadTypeTable(byte[] buffer, int offset)
        {
            int length = 0;
            MemoryStream ms = new MemoryStream(buffer);
            BinaryReader br = new BinaryReader(ms);
            int numLen;
            int typeNr = (int)DecodeNum(buffer, offset, out numLen);
            ms.Seek(offset + numLen, SeekOrigin.Begin);
            length += numLen;
            for (int i = 0; i < typeNr; i++)
            {
                RTZTypeEntry entry = new RTZTypeEntry();
                entry.Hash = br.ReadUInt32();
                entry.Index = br.ReadUInt32();
                length += 8;
                TypeTable.Add(entry);
            }
            typeNr = (int)DecodeNum(buffer, offset + length, out numLen);
            ms.Seek(numLen, SeekOrigin.Current);
            length += numLen;
            for (int i = 0; i < typeNr; i++)
            {
                int instrNr = (int)DecodeNum(buffer, offset + length, out numLen);
                ms.Seek(numLen, SeekOrigin.Current);
                length += numLen;
                var entry = TypeTable.Find(item => item.Index == i);
                entry.MemberList = new List<RTZObjectMemberEntry>();
                for (int j = 0; j < instrNr; j++)
                {
                    RTZObjectMemberEntry member = new RTZObjectMemberEntry();
                    member.MemberNameHash = br.ReadUInt32();
                    member.TypeHash = br.ReadUInt32();
                    entry.MemberList.Add(member);
                    length += 8;
                }
                UInt32 typeHash = br.ReadUInt32();
                UInt32 inheritanceFromHash = br.ReadUInt32();
                entry.InheritanceFrom = TypeTable.Find(item => item.Hash == inheritanceFromHash);
                length += 8;
            }
            TypeTable.Sort(new Comparison<RTZTypeEntry>((item0, item1) => (int)(item0.Index - item1.Index)));
            return length;
        }

        private RTZFunctionEntry LoadFunctionEntry(byte[] buffer, int offset, out int length)
        {
            length = 0;
            RTZFunctionEntry entry = new RTZFunctionEntry();
            MemoryStream ms = new MemoryStream(buffer);
            BinaryReader br = new BinaryReader(ms);
            ms.Seek(offset, SeekOrigin.Begin);
            int numLen;
            entry.Hash = br.ReadUInt32();
            length += 4;
            int preDataNr = (int)DecodeNum(buffer, offset + length, out numLen);
            if (preDataNr != 0) throw new NotSupportedException();
            length += numLen;
            entry.ParameterNumber = DecodeNum(buffer, offset + length, out numLen);
            length += numLen;
            entry.Num1 = DecodeNum(buffer, offset + length, out numLen);
            length += numLen;
            int bufLen = (int)DecodeNum(buffer, offset + length, out numLen);
            length += numLen;
            entry.InstructionBuffer = new byte[bufLen];
            ms.Seek(offset + length, SeekOrigin.Begin);
            ms.Read(entry.InstructionBuffer, 0, bufLen);
            length += bufLen;
            ms.Close();
            return entry;
        }

        private RTZImportFunctionEntry LoadImportFunctionEntry(byte[] buffer, int offset, out int length)
        {
            length = 0;
            int outLen;
            RTZImportFunctionEntry entry = new RTZImportFunctionEntry();
            MemoryStream ms = new MemoryStream(buffer);
            ms.Seek(offset, SeekOrigin.Begin);
            BinaryReader br = new BinaryReader(ms);
            entry.Hash = br.ReadUInt32();
            entry.NotImported = br.ReadByte() == 1;
            length += 5;
            if (entry.NotImported)
            {
                entry.Data = LoadFunctionEntry(buffer, offset + length, out outLen);
                length += outLen;
            }
            return entry;
        }

        private int LoadFunctionTable(byte[] buffer, int offset)
        {
            int length = 0;
            MemoryStream ms = new MemoryStream(buffer);
            BinaryReader br = new BinaryReader(ms);
            int numLen;
            int funcNr = (int)DecodeNum(buffer, offset, out numLen);
            ms.Seek(offset + numLen, SeekOrigin.Begin);
            length += numLen;
            int outLen;
            for (int i = 0; i < funcNr; i++)
            {
                FunctionTable.Add(LoadImportFunctionEntry(buffer, offset + length, out outLen));
                length += outLen;
            }
            ms.Close();
            return length;
        }

        private int LoadBaseValue(byte[] buffer, int offset, List<RTZBaseValueEntry> table)
        {
            int length = 0;
            MemoryStream ms = new MemoryStream(buffer);
            BinaryReader br = new BinaryReader(ms);
            int numLen;
            int funcNr = (int)DecodeNum(buffer, offset, out numLen);
            ms.Seek(offset + numLen, SeekOrigin.Begin);
            length += numLen;
            for (int i = 0; i < funcNr; i++)
            {
                RTZBaseValueEntry entry = new RTZBaseValueEntry();
                entry.Hash = br.ReadUInt32();
                entry.Index = br.ReadUInt32();
                length += 8;
                table.Add(entry);
            }
            return length;
        }

        private int LoadRTZStringObj(byte[] buffer, int offset, Encoding coding, List<RTZStringObj> StringObjTable)
        {
            int length = 0;
            MemoryStream ms = new MemoryStream(buffer);
            BinaryReader br = new BinaryReader(ms);
            int numLen;
            int strNr = (int)DecodeNum(buffer, offset, out numLen);
            ms.Seek(offset + numLen, SeekOrigin.Begin);
            length += numLen;
            for (int i = 0; i < strNr; i++)
            {
                UInt32 hash = br.ReadUInt32();
                length += 4;
                int strLen = (int)DecodeNum(buffer, offset + length, out numLen);
                if (coding == Encoding.Unicode)
                    strLen *= 2;
                ms.Seek(numLen, SeekOrigin.Current);
                length += numLen;
                byte[] strBuf = new byte[strLen];
                ms.Read(strBuf, 0, strLen);
                string value = coding.GetString(strBuf);
                if (coding == Encoding.UTF8)
                {
                    UInt32 crc32 = CRC32.CalcCRC32(0xFFFFFFFF, strBuf);
                    if (!StringTable.ContainsKey(crc32))
                    {
                        StringTable.Add(crc32, value);
                    }
                }
                length += strLen;
                RTZStringObj obj = new RTZStringObj() { Hash = hash, Value = value };
                StringObjTable.Add(obj);
            }
            return length;
        }

        private string DumpBuffer(byte[] buffer)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i * 0x10 < buffer.Length; i++)
            {
                sb.AppendFormat("{0:X08}:\t", i);
                for (int j = 0; j + i * 0x10 < buffer.Length && j < 0x10; j++)
                {
                    sb.AppendFormat("{0:X02} ", buffer[i * 0x10 + j]);
                }
                sb.AppendLine();
            }
            return sb.ToString();
        }

        private string DumpGlobalValue(List<RTZBaseValueEntry> baseValueTable)
        {
            if (baseValueTable != null)
            {
                StringBuilder sb = new StringBuilder();
                baseValueTable.Sort(new Comparison<RTZBaseValueEntry>((entry0, entry1) => (int)(entry0.Index - entry1.Index)));
                foreach (var entry in baseValueTable)
                {
                    if (StringTable.ContainsKey(entry.Hash))
                    {
                        sb.AppendFormat("index = {0}, name = {1}" + Environment.NewLine, entry.Index, StringTable[entry.Hash]);
                    }
                    else
                        sb.AppendFormat("index = {0}, hash = {1:X08}" + Environment.NewLine, entry.Index, entry.Hash);
                }
                return sb.ToString();
            }
            return "";
        }

        public void DisassembleFile(string path)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var t in TypeTable)
            {
                sb.AppendFormat("type index = {0}" + Environment.NewLine, t.Index);
                sb.AppendLine(t.ToString());
            }
            sb.AppendLine("Top Function");
            sb.AppendFormat("Hash:{0:X08}\tparameter num:{1}\tNum1:{2}", MainFunction.Hash, MainFunction.ParameterNumber, MainFunction.Num1);
            sb.AppendLine();
            sb.AppendLine(RTZReverseUtility.DisassembleFunction(this, MainFunction.InstructionBuffer));
            for (int i = 0; i < FunctionTable.Count; i++)
            {
                if (FunctionTable[i].Data != null)
                {
                    sb.AppendFormat("Sub Function {0}" + Environment.NewLine, i);
                    sb.AppendFormat("Hash0:{0:X08}\tHash1:{1:X08}\tparameter num:{2}\tNum1:{3}", FunctionTable[i].Hash, FunctionTable[i].Data.Hash, FunctionTable[i].Data.ParameterNumber, FunctionTable[i].Data.Num1);
                    sb.AppendLine();
                    sb.AppendLine(RTZReverseUtility.DisassembleFunction(this, FunctionTable[i].Data.InstructionBuffer));
                }
                else
                {
                    sb.AppendFormat("Sub Function {0}" + Environment.NewLine, i);
                    sb.AppendFormat("Name:{0:X08}", RTZCFunction.GetName(FunctionTable[i].Hash));
                    sb.AppendLine();
                }
            }
            sb.AppendFormat("evar table length = {0}" + Environment.NewLine, EVarLength);
            sb.AppendFormat("gvar table length = {0}" + Environment.NewLine, GVarLength);
            sb.AppendLine("evar table:");
            sb.AppendLine(DumpGlobalValue(EVarTable));
            sb.AppendLine("gvar table:");
            sb.AppendLine(DumpGlobalValue(GVarTable));
            File.WriteAllText(path, sb.ToString(), Encoding.Unicode);
        }

        private byte[] ConvertHeader()
        {
            if (UnknownHeader == null)
                return new byte[] { 0 };
            else
                throw new NotSupportedException();
        }

        private byte[] ConvertIncludePath()
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);
            bw.Write(EncodeNum((UInt32)IncludePathList.Count));
            for (int i = 0; i < IncludePathList.Count; i++)
            {
                byte[] strBuf = Encoding.UTF8.GetBytes(IncludePathList[i]);
                bw.Write(EncodeNum((UInt32)strBuf.Length));
                bw.Write(strBuf);
            }
            byte[] buffer = ms.ToArray();
            ms.Close();
            return buffer;
        }

        private byte[] ConvertTypeTable()
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);
            var sortMethod = new Comparison<RTZTypeEntry>((entry0, entry1) => entry0.Hash > entry1.Hash ? 1 : -1);
            TypeTable.Sort(sortMethod);
            TypeTable.Sort(sortMethod);
            bw.Write(EncodeNum((UInt32)TypeTable.Count));
            foreach (var typeEntry in TypeTable)
            {
                bw.Write(typeEntry.GetBytes());
            }
            bw.Write(EncodeNum((UInt32)TypeTable.Count));
            TypeTable.Sort(new Comparison<RTZTypeEntry>((item0, item1) => (int)(item0.Index - item1.Index)));
            foreach (var typeEntry in TypeTable)
            {
                bw.Write(EncodeNum((UInt32)typeEntry.MemberList.Count));
                if (typeEntry.MemberList != null && typeEntry.MemberList.Count != 0)
                {
                    foreach (var member in typeEntry.MemberList)
                    {
                        bw.Write(member.GetBytes());
                    }
                }
                bw.Write(typeEntry.Hash);
                bw.Write(typeEntry.InheritanceFrom == null ? 0 : typeEntry.InheritanceFrom.Hash);
            }
            byte[] buffer = ms.ToArray();
            ms.Close();
            return buffer;
        }

        private byte[] ConvertFunctionTable()
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);
            bw.Write(EncodeNum((UInt32)FunctionTable.Count));
            foreach (var funcEntry in FunctionTable)
            {
                bw.Write(funcEntry.GetBytes());
            }
            byte[] buffer = ms.ToArray();
            ms.Close();
            return buffer;
        }

        private byte[] ConvertBaseValueTable(List<RTZBaseValueEntry> table)
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);
            bw.Write(EncodeNum((UInt32)table.Count));
            foreach (var entry in table)
            {
                bw.Write(entry.GetBytes());
            }
            byte[] buffer = ms.ToArray();
            ms.Close();
            return buffer;
        }

        private byte[] ConvertStringObjTable(Encoding coding, List<RTZStringObj> StringObjTable)
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);
            bw.Write(EncodeNum((UInt32)StringObjTable.Count));
            StringObjTable.Sort(
                (entry0, entry1)
                    =>
                {
                    if (entry0.Hash > entry1.Hash)
                        return 1;
                    else return -1;
                });
            foreach (var strObj in StringObjTable)
            {
                byte[] strBuf = coding.GetBytes(strObj.Value);
                bw.Write(strObj.Hash);
                int strLen = coding == Encoding.Unicode ? strBuf.Length / 2 : strBuf.Length;
                bw.Write(EncodeNum((UInt32)strLen));
                bw.Write(strBuf);
            }
            byte[] buffer = ms.ToArray();
            ms.Close();
            return buffer;
        }

        public byte[] GetBytes()
        {
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);
            bw.Write(ConvertHeader());
            bw.Write(ConvertIncludePath());
            bw.Write(ConvertTypeTable());
            bw.Write(ConvertFunctionTable());
            bw.Write(MainFunction.GetBytes());
            bw.Write(BitConverter.GetBytes(EVarLength));
            bw.Write(BitConverter.GetBytes(GVarLength));
            bw.Write(ConvertBaseValueTable(EVarTable));
            bw.Write(ConvertBaseValueTable(GVarTable));
            bw.Write(ConvertStringObjTable(Encoding.UTF8, StringObjTableUTF8));
            bw.Write(ConvertStringObjTable(Encoding.Unicode, StringObjTableUTF16));
            byte[] buffer = ms.ToArray();
            ms.Close();
            return buffer;
        }

        public void Save(string path)
        {
            File.WriteAllBytes(path, GetBytes());
        }

        public void UpdateFunction(UInt32 hash, byte[] instructionBuffer)
        {
            var funcEntry = FunctionTable.Find(entry => entry.Hash == hash);
            if (funcEntry != null)
            {
                if (funcEntry.Data != null)
                    funcEntry.Data.InstructionBuffer = instructionBuffer;
            }
        }

        public void ImportFunction(UInt32 hash)
        {
            if (!FunctionTable.Exists(entry => entry.Hash == hash))
            {
                RTZImportFunctionEntry entry = new RTZImportFunctionEntry();
                entry.Data = null;
                entry.NotImported = false;
                entry.Hash = hash;
                FunctionTable.Add(entry);
                Console.WriteLine("function index = {0}", FunctionTable.Count - 1);
            }
            else
            {
                Console.WriteLine("import failed!");
                Console.WriteLine("function index = {0}", FunctionTable.FindIndex(entry => entry.Hash == hash));
            }
        }

        public void ImportFunction(string functionName)
        {
            ImportFunction(CRC32.CalcCRC32(Encoding.UTF8.GetBytes(functionName)));
        }

        public void AddNewString(string str, Encoding coding)
        {
            if (coding == Encoding.UTF8 || coding == Encoding.Unicode)
            {
                var hash = ~CRC32.CalcCRC32(Encoding.UTF8.GetBytes(str));
                var table = coding == Encoding.UTF8 ? StringObjTableUTF8 : StringObjTableUTF16;
                if (table != null && table.FindIndex(entry => entry.Hash == hash) == -1)
                {
                    RTZStringObj obj = new RTZStringObj();
                    obj.Hash = hash;
                    obj.Value = str;
                    table.Add(obj);
                    Console.WriteLine("hash = {0:X8}", hash);
                }
            }
        }

        public void MoveFunction(RTZFile targetFile, int funcIndex)
        {
            if (funcIndex < this.FunctionTable.Count)
            {
                byte[] instrBuffer = this.FunctionTable[funcIndex].Data.InstructionBuffer;
                var instrList = RTZReverseUtility.SplitInstruction(instrBuffer);
                MemoryStream ms = new MemoryStream();
                foreach (var instr in instrList)
                {
                    if (instr[0] == 0x51 || instr[0] == 0x52 || instr[0] == 0x53 || instr[0] == 0x5A)
                    {
                        var func = this.FunctionTable[BitConverter.ToUInt16(instr, 1)];
                        int index = targetFile.FunctionTable.FindIndex(entry =>
                        {
                            return func.Equals(entry);
                        });
                        if (index < 0)
                        {
                            targetFile.FunctionTable.Add(this.FunctionTable[BitConverter.ToUInt16(instr, 1)]);
                            byte[] funcIndexBuffer = BitConverter.GetBytes((ushort)targetFile.FunctionTable.Count - 1);
                            Array.Copy(funcIndexBuffer, 0, instr, 1, 2);
                        }
                        else
                        {
                            byte[] funcIndexBuffer = BitConverter.GetBytes((ushort)index);
                            Array.Copy(funcIndexBuffer, 0, instr, 1, 2);
                        }
                    }
                    else if (instr[0] == 0xF)
                    {
                        UInt32 hash = BitConverter.ToUInt32(instr, 1);
                        int index = targetFile.StringObjTableUTF16.FindIndex(entry => entry.Hash == hash);
                        if (index < 0)
                            targetFile.AddNewString(this.StringObjTableUTF16.Find(entry => entry.Hash == hash).Value, Encoding.Unicode);
                    }
                    else if (instr[0] == 0xE)
                    {
                        UInt32 hash = BitConverter.ToUInt32(instr, 1);
                        int index = targetFile.StringObjTableUTF8.FindIndex(entry => entry.Hash == hash);
                        if (index < 0)
                            targetFile.AddNewString(this.StringObjTableUTF8.Find(entry => entry.Hash == hash).Value, Encoding.UTF8);
                    }
                    else if (instr[0] == 0x58 || instr[0] == 0x59)
                    {
                        int index = BitConverter.ToUInt16(instr, 1);
                        var rtzType = this.TypeTable[index];
                        var tagIndex = targetFile.TypeTable.FindIndex(entry => entry.Equals(rtzType));
                        if (tagIndex < 0) throw new NotImplementedException();
                        byte[] typeIndexBuffer = BitConverter.GetBytes((ushort)tagIndex);
                        Array.Copy(typeIndexBuffer, 0, instr, 1, 2);
                    }
                    ms.Write(instr, 0, instr.Length);
                }
                UInt32 funchash = this.FunctionTable[funcIndex].Hash;
                var tagFunc = targetFile.FunctionTable.Find(entry => entry.Hash == funchash);
                tagFunc.Data.InstructionBuffer = ms.ToArray();
                ms.Close();
            }
        }

        public RTZFile(string path)
        {
            int outLen;
            byte[] buffer = File.ReadAllBytes(path);
            int offset = 0;
            offset += LoadHeader(buffer, offset);
            offset += LoadIncludePath(buffer, offset);
            offset += LoadTypeTable(buffer, offset);
            offset += LoadFunctionTable(buffer, offset);
            MainFunction = LoadFunctionEntry(buffer, offset, out outLen);
            offset += outLen;
            EVarLength = BitConverter.ToUInt32(buffer, offset);
            offset += 4;
            GVarLength = BitConverter.ToUInt32(buffer, offset);
            offset += 4;
            offset += LoadBaseValue(buffer, offset, EVarTable);
            offset += LoadBaseValue(buffer, offset, GVarTable);
            if (buffer[offset] != 0)
                offset += LoadRTZStringObj(buffer, offset, Encoding.UTF8, StringObjTableUTF8);
            if (buffer[offset] != 0)
                offset += LoadRTZStringObj(buffer, offset, Encoding.Unicode, StringObjTableUTF16);
        }
    }
}
