using System;
using System.Collections.Generic;
using System.Linq;

namespace EngineNS.UnitTest
{
    public enum ETestStatus
    {
        Passed,
        Failed,
        Skipped,
        Error,
        Timeout,
    }

    public sealed class TtTestResult
    {
        public string ClassName;
        public string MethodName;
        public ETestStatus Status;
        public string Message;
        public double DurationMs;
    }

    public sealed class TtTestRunSummary
    {
        public List<TtTestResult> Results { get; } = new List<TtTestResult>();
        public int Passed => Results.Count(result => result.Status == ETestStatus.Passed);
        public int Failed => Results.Count(result => result.Status != ETestStatus.Passed && result.Status != ETestStatus.Skipped);
        public int Skipped => Results.Count(result => result.Status == ETestStatus.Skipped);
    }

    public sealed class TtTestAssertionException : Exception
    {
        public TtTestAssertionException(string message)
            : base(message)
        {
        }
    }
}
