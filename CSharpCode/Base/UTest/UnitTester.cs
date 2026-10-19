using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.UnitTest
{
    public class TtTestCategory : Profiler.TtLogCategory
    {
        public override string ToString()
        {
            return "TtTest";
        }
    }

    public class TtTestAttribute : Attribute
    {
        public bool Enable = true;
    }
    public class TtUnitTestManager
    {
        [ThreadStatic]
        static TtTestAssertionException mLastAssertion;
        public static TtTestAssertionException LastAssertion => mLastAssertion;

        public static void TMessage(string message, [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
            [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
            [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0)
        {
            Profiler.Log.WriteLine<TtTestCategory>(Profiler.ELogTag.Info, $"Info:{sourceFilePath}:{sourceLineNumber}->{memberName}->{message}");
        }
        public static void TAssert(bool condi, string message, [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
            [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
            [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0)
        {
            if (condi == false)
            {
                var detail = $"Assert:{sourceFilePath}:{sourceLineNumber}->{memberName}->{message}";
                Profiler.Log.WriteLine<TtTestCategory>(Profiler.ELogTag.Error, detail);
                mLastAssertion = new TtTestAssertionException(detail);
                throw mLastAssertion;
            }
        }
        public static void DoUnitTests()
        {
            var summary = new TtTestRunner().RunTests(string.Empty, string.Empty);
            foreach (var result in summary.Results)
            {
                if (result.Status == ETestStatus.Passed || result.Status == ETestStatus.Skipped)
                    continue;
                Profiler.Log.WriteLine<TtTestCategory>(Profiler.ELogTag.Error,
                    $"{result.ClassName}.{result.MethodName}: {result.Message}");
            }
        }
    }
}
