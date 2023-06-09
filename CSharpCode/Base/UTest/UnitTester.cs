using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.UTest
{
    public class UTestAttribute : Attribute
    {
        public bool Enable = true;
    }
    public class UnitTestManager
    {
        public static void TMessage(string message, [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
            [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
            [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0)
        {
            Profiler.Log.WriteLine(Profiler.ELogTag.Info, "UTest", $"Info:{sourceFilePath}:{sourceLineNumber}->{memberName}->{message}");
        }
        public static void TAssert(bool condi, string message, [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
            [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
            [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0)
        {
            if (condi == false)
            {
                Profiler.Log.WriteLine(Profiler.ELogTag.Error, "UTest", $"Assert:{sourceFilePath}:{sourceLineNumber}->{memberName}->{message}");
                System.Diagnostics.Debug.Assert(false);
            }
        }
        public static void DoUnitTests()
        {
            var ass = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var i in ass)
            {
                var types = i.GetTypes();
                foreach (var j in types)
                {
                    var attrs = j.GetCustomAttributes(typeof(UTestAttribute), false);
                    if (attrs.Length == 0)
                        continue;
                    var ut = attrs[0] as UTestAttribute;
                    if (ut.Enable == false)
                        continue;
                    var obj = Rtti.UTypeDescManager.CreateInstance(j);
                    var mtd = j.GetMethod("UnitTestEntrance");
                    if (mtd != null)
                    {
                        try
                        {
                            mtd.Invoke(obj, null);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                        }
                    }
                }
            }
        }
    }
}
