using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeGenerateSystem
{
    public partial class Program
    {
        public static long DoubleClickThreshold = 400;
        public static void RegisterNodeAssembly()
        {
            CodeGenerateSystem.Program.RegisterNodeAssembly("CodeGenerateSystem.dll", typeof(CodeGenerateSystem.Program).Assembly);
        }
    }
}