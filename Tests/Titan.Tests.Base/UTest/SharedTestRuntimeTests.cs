using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using EngineNS.UnitTest;
using NUnit.Framework;

namespace EngineNS.Tests.UTest;

[TestFixture]
public class SharedTestRuntimeTests
{
    [Test]
    public void TAssert_False_ThrowsAssertionException()
    {
        var exception = Assert.Throws<TtTestAssertionException>(() =>
            TtUnitTestManager.TAssert(false, "expected failure"));

        Assert.That(exception!.Message, Does.Contain("expected failure"));
    }

    [Test]
    public void RunTests_MixedOutcomes_CollectsStructuredResults()
    {
        var runner = new TtTestRunner();
        var summary = runner.RunTests(new[]
        {
            typeof(PassingTest),
            typeof(FailingTest),
            typeof(ErrorTest),
            typeof(DisabledTest),
        }, "");

        Assert.Multiple(() =>
        {
            Assert.That(summary.Passed, Is.EqualTo(1));
            Assert.That(summary.Failed, Is.EqualTo(2));
            Assert.That(summary.Skipped, Is.EqualTo(1));
            Assert.That(summary.Results.Single(r => r.ClassName.EndsWith(nameof(FailingTest))).Status,
                Is.EqualTo(ETestStatus.Failed));
            Assert.That(summary.Results.Single(r => r.ClassName.EndsWith(nameof(ErrorTest))).Status,
                Is.EqualTo(ETestStatus.Error));
            Assert.That(summary.Results.All(r => r.DurationMs >= 0.0), Is.True);
        });
    }

    [Test]
    public void RunTests_Filter_ExecutesOnlyMatchingClass()
    {
        var runner = new TtTestRunner();
        var summary = runner.RunTests(new[] { typeof(PassingTest), typeof(ErrorTest) }, nameof(PassingTest));

        Assert.That(summary.Results, Has.Count.EqualTo(1));
        Assert.That(summary.Results[0].ClassName, Does.EndWith(nameof(PassingTest)));
    }

    [Test]
    public void JUnitReporter_MixedOutcomes_WritesCountsAndFailureKinds()
    {
        var summary = new TtTestRunSummary();
        summary.Results.Add(new TtTestResult
        {
            ClassName = "Tests.Passing",
            MethodName = "UnitTestEntrance",
            Status = ETestStatus.Passed,
            DurationMs = 25.0,
        });
        summary.Results.Add(new TtTestResult
        {
            ClassName = "Tests.Failing",
            MethodName = "UnitTestEntrance",
            Status = ETestStatus.Failed,
            Message = "assertion",
            DurationMs = 5.0,
        });
        summary.Results.Add(new TtTestResult
        {
            ClassName = "Tests.Error",
            MethodName = "UnitTestEntrance",
            Status = ETestStatus.Error,
            Message = "exception",
            DurationMs = 10.0,
        });

        var directory = Path.Combine(Path.GetTempPath(), "TitanTests", Guid.NewGuid().ToString("N"));
        var reportPath = Path.Combine(directory, "engine-tests.xml");
        try
        {
            TtJUnitReporter.Write(summary, reportPath);

            var suite = XDocument.Load(reportPath).Root;
            Assert.Multiple(() =>
            {
                Assert.That(suite, Is.Not.Null);
                Assert.That(suite!.Attribute("tests")?.Value, Is.EqualTo("3"));
                Assert.That(suite.Attribute("failures")?.Value, Is.EqualTo("1"));
                Assert.That(suite.Attribute("errors")?.Value, Is.EqualTo("1"));
                Assert.That(suite.Descendants("failure").Single().Value, Does.Contain("assertion"));
                Assert.That(suite.Descendants("error").Single().Value, Does.Contain("exception"));
            });
        }
        finally
        {
            if (Directory.Exists(directory))
                Directory.Delete(directory, true);
        }
    }

    [TtTest]
    private sealed class PassingTest
    {
        public void UnitTestEntrance()
        {
        }
    }

    [TtTest]
    private sealed class FailingTest
    {
        public void UnitTestEntrance()
        {
            TtUnitTestManager.TAssert(false, "runner assertion");
        }
    }

    [TtTest]
    private sealed class ErrorTest
    {
        public void UnitTestEntrance()
        {
            throw new InvalidOperationException("runner error");
        }
    }

    [TtTest(Enable = false)]
    private sealed class DisabledTest
    {
        public void UnitTestEntrance()
        {
            throw new InvalidOperationException("disabled test ran");
        }
    }
}
