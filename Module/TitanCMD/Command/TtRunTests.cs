using System;
using System.IO;

namespace ProjectCooker.Command
{
    class TtRunTests : TtCookCommand
    {
        public override async EngineNS.Thread.Async.TtTask<int> ExecuteCommand(string[] args)
        {
            await EngineNS.Thread.TtAsyncDummyClass.DummyFunc();

            var filter = FindArgument(args, "Filter=") ?? string.Empty;
            var resultsDirectory = FindArgument(args, "ResultsDir=") ?? "TestResults";
            var runner = new EngineNS.UnitTest.TtTestRunner();
            var summary = runner.RunTests(filter, resultsDirectory);
            var reportPath = Path.Combine(resultsDirectory, "engine-tests.xml");
            EngineNS.UnitTest.TtJUnitReporter.Write(summary, reportPath);

            Console.WriteLine($"Engine tests: passed={summary.Passed}, failed={summary.Failed}, skipped={summary.Skipped}");
            Console.WriteLine($"JUnit report: {Path.GetFullPath(reportPath)}");
            return summary.Failed == 0 ? 0 : 1;
        }
    }
}
