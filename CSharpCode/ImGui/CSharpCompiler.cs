using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.EGui
{
    public class CSharpCompiler
    {
        public static System.CodeDom.Compiler.CompilerResults CompilerImGuiCode(string file)
        {
            try
            {
                string text = System.IO.File.ReadAllText(file);
                var objCompilerParameters = new System.CodeDom.Compiler.CompilerParameters();
                objCompilerParameters.ReferencedAssemblies.Add("Sample.Net.exe");
                objCompilerParameters.GenerateExecutable = false;
                objCompilerParameters.GenerateInMemory = true;
                objCompilerParameters.IncludeDebugInformation = true;
                //objCompilerParameters.OutputAssembly = @"D:\Work\TProject\Sample.Net\bin\Debug\gui_gen.dll";
                objCompilerParameters.CompilerOptions = "/unsafe";
                var tmpPath = System.IO.Directory.GetCurrentDirectory() + "/gui_gen";
                if (System.IO.Directory.Exists(tmpPath))
                {
                    var files = System.IO.Directory.GetFiles(tmpPath);
                    foreach (var i in files)
                    {
                        try
                        {
                            System.IO.File.Delete(i);
                        }
                        catch
                        {
                            continue;
                        }
                    }
                }
                else
                {
                    System.IO.Directory.CreateDirectory(tmpPath);
                }
                objCompilerParameters.TempFiles = new System.CodeDom.Compiler.TempFileCollection("./gui_gen", true);

                return null;
                //var mCSharpCodePrivoder = new Microsoft.CodeDom.Providers.DotNetCompilerPlatform.CSharpCodeProvider();
                ////Microsoft.CSharp.CSharpCodeProvider mCSharpCodePrivoder = new Microsoft.CSharp.CSharpCodeProvider(
                ////    new Dictionary<string, string>()
                ////    {
                ////        { "CompilerVersion", "v5.0" }
                ////    });
                //var cr = mCSharpCodePrivoder.CompileAssemblyFromSource(objCompilerParameters, new string[] { text });
                //if (cr.Errors.HasErrors)
                //{
                //    foreach (var i in cr.Errors)
                //    {
                //        Console.WriteLine(i);
                //    }
                //    return null;
                //}
                //else
                //{
                //    Console.WriteLine("UI reloaded");
                //    return cr;
                //}
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return null;
            }
        }
    }
}
