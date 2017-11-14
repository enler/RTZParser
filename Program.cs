using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace RTZParser
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 3 && args[0] == "--dis" && File.Exists(args[1]))
            {
                RTZFile file = new RTZFile(args[1]);
                file.DisassembleFile(args[2]);
            }
        }
    }
}
