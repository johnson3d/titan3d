using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CppWeaving
{
    // 解析失败的头不会产出 C# 绑定，静默跳过会让两端绑定不一致；汇总后由退出码把关
    public static class ParseFailures
    {
        static readonly ConcurrentDictionary<string, string> mFailures = new ConcurrentDictionary<string, string>();

        public static void Record(string file, string message)
        {
            mFailures.TryAdd(file, message);
        }
        public static IReadOnlyDictionary<string, string> Snapshot()
        {
            return new Dictionary<string, string>(mFailures);
        }
        public static void Clear()
        {
            mFailures.Clear();
        }
        public static int Report(IReadOnlyDictionary<string, string> failures, bool allow, TextWriter output)
        {
            if (failures.Count == 0)
                return 0;
            output.WriteLine($"CppWeaving: {failures.Count} header(s) failed to parse:");
            foreach (var i in failures.OrderBy(x => x.Key, StringComparer.Ordinal))
            {
                output.WriteLine($"  {i.Key}");
                output.WriteLine($"    {i.Value}");
            }
            if (allow)
            {
                output.WriteLine("CppWeaving: AllowParseErrors=1, continuing with incomplete bindings");
                return 0;
            }
            return 2;
        }
    }
}
