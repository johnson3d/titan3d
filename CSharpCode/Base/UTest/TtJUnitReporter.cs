using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace EngineNS.UnitTest
{
    public static class TtJUnitReporter
    {
        public static void Write(TtTestRunSummary summary, string reportPath)
        {
            var directory = Path.GetDirectoryName(Path.GetFullPath(reportPath));
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            var suite = new XElement("testsuite",
                new XAttribute("name", "TitanEngine"),
                new XAttribute("tests", summary.Results.Count),
                new XAttribute("failures", summary.Results.Count(result => result.Status == ETestStatus.Failed)),
                new XAttribute("errors", summary.Results.Count(result => result.Status == ETestStatus.Error || result.Status == ETestStatus.Timeout)),
                new XAttribute("skipped", summary.Skipped),
                new XAttribute("time", FormatSeconds(summary.Results.Sum(result => result.DurationMs))));

            foreach (var result in summary.Results)
            {
                var testCase = new XElement("testcase",
                    new XAttribute("classname", result.ClassName ?? string.Empty),
                    new XAttribute("name", result.MethodName ?? string.Empty),
                    new XAttribute("time", FormatSeconds(result.DurationMs)));

                switch (result.Status)
                {
                    case ETestStatus.Failed:
                        testCase.Add(new XElement("failure",
                            new XAttribute("message", FirstLine(result.Message)),
                            result.Message ?? string.Empty));
                        break;
                    case ETestStatus.Error:
                    case ETestStatus.Timeout:
                        testCase.Add(new XElement("error",
                            new XAttribute("message", FirstLine(result.Message)),
                            result.Message ?? string.Empty));
                        break;
                    case ETestStatus.Skipped:
                        testCase.Add(new XElement("skipped",
                            new XAttribute("message", result.Message ?? string.Empty)));
                        break;
                }
                suite.Add(testCase);
            }

            new XDocument(new XDeclaration("1.0", "utf-8", null), suite).Save(reportPath);
        }

        static string FormatSeconds(double durationMs)
        {
            return (durationMs / 1000.0).ToString("0.######", CultureInfo.InvariantCulture);
        }

        static string FirstLine(string message)
        {
            if (string.IsNullOrEmpty(message))
                return string.Empty;
            var separator = message.IndexOfAny(new[] { '\r', '\n' });
            return separator < 0 ? message : message.Substring(0, separator);
        }
    }
}
