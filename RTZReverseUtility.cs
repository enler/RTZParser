using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RTZParser
{
    class RTZReverseUtility
    {
        private static int[] opcodeLens = new int[106] { 1, 1, 1, 1, 1, 1, 1, 1, 2, 2, 5, 2, 3, 5, 5, 5,
                3, 2, 3, 2, 3, 2, 3, 2, 3, 3, 1, 2, 3, 2, 3, 2,
                3, 2, 3, 3, 1, -1, 1, 3, 3, 3, 3, 1, 1, 1, -1, 1,
                -1, -1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
                1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, -1, 1, 1, 2,
                2, 4, 4, 4, 1, 1, 1, 3, 3, 3, 4, 1, 2, 1, 3, 3,
                3, 3, 5, 1, 1, 1, 1, 5, 1, 1 };
        public static string DisassembleFunction(RTZFile rtz, byte[] buffer)
        {
            StringBuilder sb = new StringBuilder();
            int i = 0;
            while (i < buffer.Length)
            {
                int opLen = opcodeLens[buffer[i]];
                if (opLen == -1)
                {
                    throw new NotSupportedException();
                }
                sb.AppendFormat("{0:X08}:\t", i);
                for (int j = 0; j < opLen; j++)
                {
                    sb.AppendFormat("{0:X02} ", buffer[i + j]);
                }
                int opcode = buffer[i];
                switch (opcode)
                {
                    case 0x1:
                        sb.Append("(operator int)");
                        break;
                    case 0x2:
                        sb.Append("(operator bool)");
                        break;
                    case 0x3:
                        sb.Append("(operator float)");
                        break;
                    case 0x4:
                        sb.Append("(operator string)");
                        break;
                    case 0x5:
                        sb.Append("(operator wstring)");
                        break;
                    case 0x6:
                        sb.Append("(push null)");
                        break;
                    case 0x7:
                        sb.Append("(push void)");
                        break;
                    case 0x9:
                        bool flag = buffer[i + 1] == 1;
                        sb.AppendFormat("(push bool {0})", flag.ToString());
                        break;
                    case 0xA:
                        Int32 value = BitConverter.ToInt32(buffer, i + 1);
                        sb.AppendFormat("(push int {0})", value);
                        break;
                    case 0xB:
                        value = buffer[i + 1];
                        sb.AppendFormat("(push byte {0})", value);
                        break;
                    case 0xC:
                        value = BitConverter.ToInt16(buffer, i + 1);
                        sb.AppendFormat("(push short {0})", value);
                        break;
                    case 0xD:
                        float f = BitConverter.ToSingle(buffer, i + 1);
                        sb.AppendFormat("(push float {0})", f);
                        break;
                    case 0xE:
                        UInt32 hash = BitConverter.ToUInt32(buffer, i + 1);
                        var strObj = rtz.StringObjTableUTF8.Find(item => item.Hash == hash);
                        sb.AppendFormat("(push str \"{0}\")", strObj.Value);
                        break;
                    case 0xF:
                        hash = BitConverter.ToUInt32(buffer, i + 1);
                        strObj = rtz.StringObjTableUTF16.Find(item => item.Hash == hash);
                        sb.AppendFormat("(push wstr \"{0}\")", strObj.Value);
                        break;
                    case 0x10:
                        value = BitConverter.ToUInt16(buffer, i + 1);
                        sb.AppendFormat("(push function {0})", value);
                        break;
                    case 0x11:
                        value = buffer[i + 1];
                        sb.AppendFormat("(push function {0})", value);
                        break;
                    case 0x12:
                        value = BitConverter.ToUInt16(buffer, i + 1);
                        sb.AppendFormat("(push evar {0})", value);
                        break;
                    case 0x13:
                        value = buffer[i + 1];
                        sb.AppendFormat("(push evar {0})", value);
                        break;
                    case 0x14:
                        value = BitConverter.ToUInt16(buffer, i + 1);
                        sb.AppendFormat("(push gvar {0})", value);
                        break;
                    case 0x15:
                        value = buffer[i + 1];
                        sb.AppendFormat("(push gvar {0})", value);
                        break;
                    case 0x16:
                        value = BitConverter.ToUInt16(buffer, i + 1);
                        sb.AppendFormat("(push stack {0})", value);
                        break;
                    case 0x17:
                        value = buffer[i + 1];
                        sb.AppendFormat("(push stack {0})", value);
                        break;
                    case 0x18:
                        value = BitConverter.ToUInt16(buffer, i + 1);
                        sb.AppendFormat("(push s0->m{0})", value);
                        break;
                    case 0x19:
                        value = BitConverter.ToUInt16(buffer, i + 1);
                        sb.AppendFormat("(push this->m{0})", value);
                        break;
                    case 0x1A:
                        sb.Append("(push this)");
                        break;
                    case 0x1C:
                        value = BitConverter.ToUInt16(buffer, i + 1);
                        sb.AppendFormat("(set s0 to evar {0})", value);
                        break;
                    case 0x1D:
                        value = buffer[i + 1];
                        sb.AppendFormat("(set s0 to evar {0})", value);
                        break;
                    case 0x1E:
                        value = BitConverter.ToUInt16(buffer, i + 1);
                        sb.AppendFormat("(set s0 to gvar {0})", value);
                        break;
                    case 0x1F:
                        value = buffer[i + 1];
                        sb.AppendFormat("(set s0 to gvar {0})", value);
                        break;
                    case 0x20:
                        value = BitConverter.ToUInt16(buffer, i + 1);
                        sb.AppendFormat("(set s0 to stack {0})", value);
                        break;
                    case 0x21:
                        value = buffer[i + 1];
                        sb.AppendFormat("(set s0 to stack {0})", value);
                        break;
                    case 0x22:
                    case 0x25:
                        value = BitConverter.ToUInt16(buffer, i + 1);
                        sb.AppendFormat("(set s0 to s1->m{0})", value);
                        break;
                    case 0x23:
                        value = BitConverter.ToUInt16(buffer, i + 1);
                        sb.AppendFormat("(set s0 to this->m{0})", value);
                        break;
                    case 0x24:
                        sb.Append("(set s2 to s1->s0)");
                        break;
                    case 0x26:
                        sb.Append("(s1 = s0)");
                        break;
                    case 0x27:
                        value = BitConverter.ToUInt16(buffer, i + 1);
                        sb.AppendFormat("(evar {0} = s0)", value);
                        break;
                    case 0x28:
                        value = BitConverter.ToUInt16(buffer, i + 1);
                        sb.AppendFormat("(gvar {0} = s0)", value);
                        break;
                    case 0x29:
                        value = BitConverter.ToUInt16(buffer, i + 1);
                        sb.AppendFormat("(stack {0} = s0)", value);
                        break;
                    case 0x2A:
                        value = BitConverter.ToUInt16(buffer, i + 1);
                        sb.AppendFormat("(this->m{0} = s0)", value);
                        break;
                    case 0x2B:
                        sb.Append("(s1 += s0)");
                        break;
                    case 0x2C:
                        sb.Append("(s1 -= s0)");
                        break;
                    case 0x2D:
                        sb.Append("(s1 *= s0)");
                        break;
                    case 0x2E:
                        sb.Append("(s1 %= s0)");
                        break;
                    case 0x2F:
                        sb.Append("(s1 /= s0)");
                        break;
                    case 0x30:
                        sb.Append("(s1 <<= s0)");
                        break;
                    case 0x31:
                        sb.Append("(s1 >>= s0)");
                        break;
                    case 0x32:
                        sb.Append("(s1 |= s0)");
                        break;
                    case 0x33:
                        sb.Append("(s1 &= s0)");
                        break;
                    case 0x34:
                        sb.Append("(s1 ^= s0)");
                        break;
                    case 0x35:
                        sb.Append("(s0 = s1 || s0)");
                        break;
                    case 0x36:
                        sb.Append("(s0 = s1 && s0)");
                        break;
                    case 0x37:
                        sb.Append("(s0 = s1 == s0)");
                        break;
                    case 0x38:
                        sb.Append("(s0 = s1 != s0)");
                        break;
                    case 0x39:
                        sb.Append("(s0 = s1 <= s0)");
                        break;
                    case 0x3A:
                        sb.Append("(s0 = s1 >= s0)");
                        break;
                    case 0x3B:
                        sb.Append("(s0 = s1 < s0)");
                        break;
                    case 0x3C:
                        sb.Append("(s0 = s1 > s0)");
                        break;
                    case 0x3D:
                        sb.Append("(s0 = s1 | s0)");
                        break;
                    case 0x3E:
                        sb.Append("(s0 = s1 & s0)");
                        break;
                    case 0x3F:
                        sb.Append("(s0 = s1 ^ s0)");
                        break;
                    case 0x40:
                        sb.Append("(s0 = s1 << s0)");
                        break;
                    case 0x41:
                        sb.Append("(s0 = s1 >> s0)");
                        break;
                    case 0x42:
                        sb.Append("(s0 = s1 + s0)");
                        break;
                    case 0x43:
                        sb.Append("(s0 = s1 - s0)");
                        break;
                    case 0x44:
                        sb.Append("(s0 = s1 * s0)");
                        break;
                    case 0x45:
                        sb.Append("(s0 = s1 / s0)");
                        break;
                    case 0x46:
                        sb.Append("(s0 = s1 % s0)");
                        break;
                    case 0x47:
                        sb.Append("(++s0)");
                        break;
                    case 0x48:
                        sb.Append("(--s0)");
                        break;
                    case 0x49:
                        sb.Append("(s0++)");
                        break;
                    case 0x4A:
                        sb.Append("(s0--)");
                        break;
                    case 0x4B:
                        sb.Append("(-s0)");
                        break;
                    case 0x4C:
                        sb.Append("(~s0)");
                        break;
                    case 0x4D:
                        sb.Append("(s0 = s0 == 0)");
                        break;
                    case 0x4E:
                        sb.Append("(s0 = s1[s0])");
                        break;
                    case 0x4F:
                        sb.Append("(call fucntion s0(s1,...) )");
                        break;
                    case 0x50:
                        sb.Append("(call fucntion s0(this,...) )");
                        break;
                    case 0x51:
                    case 0x52:
                    case 0x53:
                    case 0x5A:
                        int funcID = BitConverter.ToUInt16(buffer, i + 1);
                        string funcName = RTZCFunction.GetName(rtz.FunctionTable[funcID].Hash);
                        if (funcName != "")
                        {
                            sb.AppendFormat("(call {0})", funcName);
                        }
                        else
                        {
                            sb.AppendFormat("(call Sub Function {0})", funcID);
                        }
                        break;
                    case 0x54:
                        sb.Append("(boxing)");
                        break;
                    case 0x55:
                        sb.Append("(push s0)");
                        break;
                    case 0x56:
                        sb.Append("(pop s0)");
                        break;
                    case 0x58:
                        value = BitConverter.ToUInt16(buffer, i + 1);
                        sb.AppendFormat("(s0 = new class {0})", value);
                        break;
                    case 0x59:
                        value = BitConverter.ToUInt16(buffer, i + 1);
                        sb.AppendFormat("(s0 = new class[s0] {0})", value);
                        break;
                    case 0x5B:
                        sb.Append("(s0 = s0)");
                        break;

                    case 0x5E:
                        int offset = BitConverter.ToInt16(buffer, i + 1);
                        sb.AppendFormat("(jp {0:X4})", i + offset);
                        break;
                    case 0x5F:
                        offset = BitConverter.ToUInt16(buffer, i + 1);
                        sb.AppendFormat("(j {0:X4})", offset);
                        break;
                    case 0x60:
                        offset = BitConverter.ToInt16(buffer, i + 1);
                        sb.AppendFormat("(if (s0 == 0) jp {0:X4})", i + offset);
                        break;
                    case 0x61:
                        offset = BitConverter.ToInt16(buffer, i + 1);
                        sb.AppendFormat("(if (s0 != s1) jp {0:X4})", i + offset);
                        break;
                    case 0x62:
                        offset = BitConverter.ToInt16(buffer, i + 1);
                        UInt32 typeID = BitConverter.ToUInt16(buffer, i + 3);
                        sb.AppendFormat("(if (s0.typeID != {0} jp {1:X4})", typeID, i + offset);
                        break;
                    case 0x63:
                        sb.Append("(return)");
                        break;
                    case 0x64:
                        sb.Append("(return n)");
                        break;
                    case 0x66:
                        sb.Append("(abort)");
                        break;
                    case 0x69:
                        sb.Append("(exit)");
                        break;
                }
                i += opLen;
                sb.AppendLine();
            }
            return sb.ToString();
        }

        public static List<byte[]> SplitInstruction(byte[] buffer)
        {
            List<byte[]> instrList = new List<byte[]>();
            int i = 0;
            while (i < buffer.Length)
            {
                int opLen = opcodeLens[buffer[i]];
                if (opLen == -1)
                {
                    throw new NotSupportedException();
                }
                byte[] instr = new byte[opLen];
                Array.Copy(buffer, i, instr, 0, opLen);
                i += opLen;
                instrList.Add(instr);
            }
            return instrList;
        }
    }
}
