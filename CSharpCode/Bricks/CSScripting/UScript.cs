using System;
using System.Collections.Generic;
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
        private unsafe struct TestFunction_Variables
        {
            public int* a0;
            public int* a1;
            public WeakReference<UTest_XndTester> This;
        }
        public unsafe int TestFunction(int a0, int a1, int a2, int a3, int a4, int a5, int a6, int a7, MemberStruct* b1, MemberStruct* b2)
        {
            TestFunction_Variables variables = new TestFunction_Variables();
            variables.a0 = &a0;
            variables.a1 = &a1;
            variables.This = new WeakReference<UTest_XndTester>(this);
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
            int tmp_a = a0 + a1;
            int tmp_b = a2 + a3;

            *b1 = *b2;

            b1->Type = tmp_a + tmp_b;

            return 0;
        }
        public string ReadText(string code, int StartLine, int StartColumn, int EndLine, int EndColumn)
        {
            int curPos = 0;
            for(int i=0; i<StartLine; i++)
            {
                curPos = code.IndexOf("\r\n", curPos) + 2;
            }
            int startPos = curPos + StartColumn;

            curPos = 0;
            for (int i = 0; i < EndLine; i++)
            {
                curPos = code.IndexOf("\r\n", curPos) + 2;
            }
            int endPos = curPos + EndColumn;
            var str = code.Substring(startPos, endPos - startPos);
            return str;
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

            var st = new System.Diagnostics.StackTrace();
            System.Diagnostics.StackFrame[] sfs = st.GetFrames();
            foreach(var i in sfs)
            {
                if (System.Diagnostics.StackFrame.OFFSET_UNKNOWN == i.GetILOffset()) 
                    break;
                var name = i.GetMethod().Name;
                var variables = i.GetMethod().GetMethodBody().LocalVariables;
                foreach(var j in variables)
                {
                    j.ToString();
                }
            }

            var dllName = TtEngine.Instance.FileManager.GetRoot(IO.TtFileManager.ERootDir.Execute) + $"{TtEngine.DotNetVersion}/Engine.Window.dll";
            var option = new ReaderParameters();
            option.ReadSymbols = true;
            //option.ReadingMode = ReadingMode.Immediate;
            var ilfixAassembly = AssemblyDefinition.ReadAssembly(dllName, option);
            if (ilfixAassembly == null)
                return;

            //var code = IO.FileManager.ReadAllText(@"D:\work\titan3d\CSharpCode\Bricks\CSScripting\UScript.cs");
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
                                var pt = k.DebugInformation.GetSequencePointMapping();
                                foreach (var m in pt)
                                {
                                    m.Value.ToString();
                                    //var str = ReadText(code, m.Value.StartLine, m.Value.StartColumn, m.Value.EndLine, m.Value.EndColumn);
                                    //Console.WriteLine($"{str}");
                                }
                                foreach (var m in k.Body.Variables)
                                {
                                    var define = m.Resolve();
                                    var tokens = k.DebugInformation.MetadataToken;
                                    Console.WriteLine($"{m.ToString()}");
                                }
                                foreach(var m in k.Body.Instructions)
                                {
                                    Console.WriteLine($"{m.OpCode}, {m.Operand}");
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
