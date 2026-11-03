using System;
using EngineNS;
using NUnit.Framework;

namespace EngineNS.Tests;

/// <summary>
/// TtEngine.DotNetVersion 必须跟随宿主运行时（System.Environment.Version），
/// 否则 CSScripting 等以 "{DotNetVersion}/xxx.dll" 拼路径的代码在升级 TFM 后
/// 会拼出不存在的目录（历史缺陷：switch 只枚举 Major 6/7/8，net10 下得到 "?"）。
/// </summary>
[TestFixture]
public class TtEngineVersionTests
{
    [Test]
    public void DotNetVersion_MatchesHostRuntime()
    {
        var expected = $"net{Environment.Version.Major}.0";
        Assert.That(TtEngine.DotNetVersion, Is.EqualTo(expected),
            $"宿主运行时为 {Environment.Version}，DotNetVersion 应派生自它而不是硬编码 switch");
    }
}
