using System;
using System.Text.RegularExpressions;

namespace Titan.Build
{
    /// <summary>
    /// 求值失败必须是显式异常：静默漏项会直接把源文件从清单里丢掉，比构建失败更难查。
    /// </summary>
    public sealed class TitanEvalException : Exception
    {
        public TitanEvalException(string message) : base(message) { }
        public TitanEvalException(string message, Exception inner) : base(message, inner) { }
    }

    /// <summary>
    /// 工程条件求值的唯一实现。只接受仓库实际出现的形态，其他形态直接失败，
    /// 避免"不认识就当不匹配"造成的静默漏源/漏宏。
    /// </summary>
    public sealed class ConditionMatcher
    {
        static readonly Regex ConfigOnly =
            new Regex(@"^'\$\(Configuration\)'=='(Debug|Release)'$", RegexOptions.CultureInvariant);
        static readonly Regex ConfigWithPlatform =
            new Regex(@"^'\$\(Configuration\)\|\$\(Platform\)'=='(Debug|Release)\|([A-Za-z0-9_]+)'$", RegexOptions.CultureInvariant);

        readonly string _configuration;
        readonly string _platform;

        public ConditionMatcher(string configuration, string platform)
        {
            if (configuration != "Debug" && configuration != "Release")
                throw new TitanEvalException($"[titan-eval] 不支持的配置: {configuration}");
            _configuration = configuration;
            _platform = platform;
        }

        public bool IsSupportedOnlyAsBoolean(string condition) => !string.IsNullOrWhiteSpace(condition);

        public bool Matches(string condition)
        {
            if (string.IsNullOrWhiteSpace(condition))
                return true;
            var c = Strip(condition);
            var m = ConfigWithPlatform.Match(c);
            if (m.Success)
            {
                return m.Groups[1].Value == _configuration
                    && string.Equals(m.Groups[2].Value, _platform, StringComparison.Ordinal);
            }
            m = ConfigOnly.Match(c);
            if (m.Success)
                return m.Groups[1].Value == _configuration;
            throw new TitanEvalException($"[titan-eval] 不支持条件: {c}");
        }

        static string Strip(string value)
        {
            var chars = new char[value.Length];
            var n = 0;
            foreach (var ch in value)
            {
                if (ch == ' ' || ch == '\t' || ch == '\r' || ch == '\n')
                    continue;
                chars[n++] = ch;
            }
            return new string(chars, 0, n);
        }
    }
}
