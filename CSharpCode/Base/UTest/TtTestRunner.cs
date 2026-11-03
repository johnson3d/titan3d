using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace EngineNS.UnitTest
{
    public sealed class TtTestRunner
    {
        public TtTestRunSummary RunTests(string categoryFilter, string resultsDirectory)
        {
            return RunTests(DiscoverTests(), categoryFilter);
        }

        public TtTestRunSummary RunTests(IEnumerable<Type> testTypes, string categoryFilter)
        {
            var summary = new TtTestRunSummary();
            var filter = categoryFilter ?? string.Empty;
            foreach (var testType in testTypes.OrderBy(type => type.FullName, StringComparer.Ordinal))
            {
                var fullName = testType.FullName ?? testType.Name;
                if (filter.Length != 0 && fullName.IndexOf(filter, StringComparison.OrdinalIgnoreCase) < 0)
                    continue;

                var attribute = testType.GetCustomAttribute<TtTestAttribute>(false);
                if (attribute == null)
                    continue;

                var result = new TtTestResult
                {
                    ClassName = fullName,
                    MethodName = "UnitTestEntrance",
                };
                summary.Results.Add(result);

                if (!attribute.Enable)
                {
                    result.Status = ETestStatus.Skipped;
                    result.Message = "Disabled by TtTestAttribute";
                    continue;
                }

                var method = testType.GetMethod("UnitTestEntrance", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (method == null)
                {
                    result.Status = ETestStatus.Error;
                    result.Message = "UnitTestEntrance method was not found";
                    continue;
                }

                var stopwatch = Stopwatch.StartNew();
                try
                {
                    var instance = Activator.CreateInstance(testType, true);
                    method.Invoke(instance, null);
                    result.Status = ETestStatus.Passed;
                }
                catch (TargetInvocationException exception)
                {
                    RecordException(result, exception.InnerException ?? exception);
                }
                catch (Exception exception)
                {
                    RecordException(result, exception);
                }
                finally
                {
                    stopwatch.Stop();
                    result.DurationMs = stopwatch.Elapsed.TotalMilliseconds;
                }
            }
            return summary;
        }

        static IEnumerable<Type> DiscoverTests()
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                // 只扫描引擎内定义了 AssemblyDesc 的程序集, 避免遍历所有类型的开销和异常
                if (FindAssemblyDesc(assembly) == null)
                    continue;

                Type[] types;
                try
                {
                    types = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException exception)
                {
                    types = exception.Types.Where(type => type != null).ToArray();
                }
                catch
                {
                    continue;
                }

                foreach (var type in types)
                {
                    if (type == null)
                        continue;

                    bool isTest;
                    try
                    {
                        isTest = type.IsDefined(typeof(TtTestAttribute), false);
                    }
                    catch
                    {
                        continue;
                    }

                    if (isTest)
                        yield return type;
                }
            }
        }

        static Rtti.TtAssemblyDesc FindAssemblyDesc(Assembly assembly)
        {
            var entryType = assembly.GetType("EngineNS.Rtti.AssemblyEntry", false);
            if (entryType == null)
                return null;
            var mtd = entryType.GetMethod("GetAssemblyDesc", BindingFlags.Public | BindingFlags.Static);
            if (mtd == null)
                return null;
            try
            {
                return mtd.Invoke(null, null) as Rtti.TtAssemblyDesc;
            }
            catch
            {
                return null;
            }
        }

        static void RecordException(TtTestResult result, Exception exception)
        {
            result.Status = exception is TtTestAssertionException ? ETestStatus.Failed : ETestStatus.Error;
            result.Message = exception.ToString();
        }
    }
}
