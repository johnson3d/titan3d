using System;
using System.Collections.Generic;
using System.Text;
using Mono.Cecil;

namespace EngineNS.Bricks.CSScripting
{
    public class UScript
    {
        public static UScript Compile(string code)
        {
            return null;
        }
        public object Invoke(string type, string method, object host, object[] parms)
        {
            return null;
        }
        //UScriptType FindType(string name);
        //UScriptMethod FindTypeMethod(UScriptType type, string name);
        //public object Invoke(UScriptMethod method, object host, object[] parms);
    }
}

namespace EngineNS.UTest
{
    public struct StructTTT
    {
        public object Type;
        public int f0;
        public float f1;
    }
    [UTest.UTest]
    public class UTest_XndTester
    {
        public struct MemberStruct
        {
            public int Type;
            public int f0;
            public float f1;
        }
        public int f0;
        public float f1;
        public MemberStruct f2;
        private int Add(int a, float b)
        {
            return a + (int)b;
        }
        public unsafe int TestFunction(int a0, int a1, int a2, int a3, int a4, int a5, int a6, int a7, MemberStruct* b1, MemberStruct* b2)
        {
            //float t0 = 33.3f;
            //int t1 = 100 + a3;
            //f0 = 100;
            //f0 += Add(t1, t0);
            //f1 = 3.1f;
            //f2.f0 = 200;
            //f2.f1 = 400.0f;

            //int result = 0;
            //for (int i = 0; i < 100; i++)
            //{
            //    result += a0 + a1;
            //}
            //return result;

            *b1 = *b2;

            return 0;
        }
        public unsafe void UnitTestEntrance()
        {
            fixed(MemberStruct* p_f2 = &f2)
            fixed (int* p_f0 = &f0)
            {
                var offset = (int)((byte*)p_f2 - (byte*)p_f0);
                if(offset == 8)
                {

                }
            }
            
            var dllName = UEngine.Instance.FileManager.GetRoot(IO.FileManager.ERootDir.Current) + "net5.0/Engine.Window.dll";
            var ilfixAassembly = AssemblyDefinition.ReadAssembly(dllName);
            if (ilfixAassembly == null)
                return;
            foreach (var i in ilfixAassembly.Modules)
            {
                foreach (var j in i.Types)
                {
                    if (j.Name == "StructTTT")
                    {
                        foreach (var k in j.Fields)
                        {
                            if (k.FieldType.IsValueType)
                            {

                            }
                        }
                    }
                    else if (j.FullName == "EngineNS.UTest.UTest_XndTester")
                    {
                        foreach (var k in j.Methods)
                        {
                            if (k.Name == "TestFunction")
                            {
                                for (int m = 0; m < k.Body.Instructions.Count; m++)
                                {
                                    Console.WriteLine($"{k.Body.Instructions[m].OpCode}, {k.Body.Instructions[m].Operand}");
                                }
                            }
                        }
                    }
                }
            }
            if (ilfixAassembly == null)
                return;
        }
    }
}
