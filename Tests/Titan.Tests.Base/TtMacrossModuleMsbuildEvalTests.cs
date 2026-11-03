using System;
using System.IO;
using EngineNS.Macross;
using NUnit.Framework;

namespace EngineNS.Tests;

/// <summary>
/// CompileGameProject 的工程输入求值必须走 MSBuild CLI（-getItem + dump target），
/// 而不是 XDocument 抠原始 XML：前者自动求值 TFM/平台表达式（如
/// $(TitanEngineAssemblyPath) 按宿主平台选 Engine.Window/Engine.Mac/Engine.Android），
/// 并让 Macross 生成文件经 Compile glob 自动进入编译列表。
/// </summary>
[TestFixture]
public class TtMacrossModuleMsbuildEvalTests
{
    private string _root;
    private string _projectFile;

    [OneTimeSetUp]
    public void CreateFixtureProject()
    {
        _root = Path.Combine(TestContext.CurrentContext.WorkDirectory,
            $"TitanEvalFixture-{Guid.NewGuid():N}");
        var projDir = Path.Combine(_root, "Module", "GameProj");
        Directory.CreateDirectory(projDir);
        Directory.CreateDirectory(Path.Combine(_root, "BuildScript", "msbuild"));
        Directory.CreateDirectory(Path.Combine(_root, "BuildScript", "common"));
        Directory.CreateDirectory(Path.Combine(projDir, "src"));

        File.Copy(Path.Combine(TestContext.CurrentContext.TestDirectory,
            "..", "..", "..", "..", "..", "BuildScript", "msbuild", "TitanPlatform.props"),
            Path.Combine(_root, "BuildScript", "msbuild", "TitanPlatform.props"));
        File.Copy(Path.Combine(TestContext.CurrentContext.TestDirectory,
            "..", "..", "..", "..", "..", "BuildScript", "msbuild", "TitanFramework.props"),
            Path.Combine(_root, "BuildScript", "msbuild", "TitanFramework.props"));
        File.Copy(Path.Combine(TestContext.CurrentContext.TestDirectory,
            "..", "..", "..", "..", "..", "BuildScript", "msbuild", "TitanManagedProject.props"),
            Path.Combine(_root, "BuildScript", "msbuild", "TitanManagedProject.props"));
        File.Copy(Path.Combine(TestContext.CurrentContext.TestDirectory,
            "..", "..", "..", "..", "..", "BuildScript", "common", "dependencies.json"),
            Path.Combine(_root, "BuildScript", "common", "dependencies.json"));
        File.WriteAllText(Path.Combine(_root, "Directory.Build.props"),
            "<Project>\n  <Import Project=\"$(MSBuildThisFileDirectory)BuildScript\\msbuild\\TitanFramework.props\" />\n</Project>");

        File.WriteAllText(Path.Combine(projDir, "src", "Hello.cs"), "class Hello { }\n");

        _projectFile = Path.Combine(projDir, "GameProj.csproj");
        File.WriteAllText(_projectFile, """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <TargetFramework>$(_TitanTargetFramework)</TargetFramework>
                <TitanMacrossCsDir Condition="'$(TitanMacrossCsDir)' == ''">$(MSBuildThisFileDirectory)macross\</TitanMacrossCsDir>
              </PropertyGroup>
              <Import Project="$(MSBuildThisFileDirectory)..\..\BuildScript\msbuild\TitanManagedProject.props" Condition="'$(TitanEngineAssemblyName)' == ''" />
              <ItemGroup>
                <Reference Include="TitanEngine">
                  <HintPath>..\..\binaries\$(_TitanTargetFramework)\$(TitanEngineAssemblyName).dll</HintPath>
                </Reference>
              </ItemGroup>
              <ItemGroup>
                <Compile Include="$(TitanMacrossCsDir)**\*.cs" />
              </ItemGroup>
              <Target Name="TitanDumpCompileInputs">
                <WriteLinesToFile File="$(TitanCompileInputsFile)" Lines="@(Compile->'%(Identity)')" Overwrite="true" />
              </Target>
            </Project>
            """);
        Directory.CreateDirectory(Path.Combine(projDir, "macross"));
        File.WriteAllText(Path.Combine(projDir, "macross", "Gen.cs"), "class Gen { }\n");
    }

    [OneTimeTearDown]
    public void DeleteFixtureProject()
    {
        if (Directory.Exists(_root))
            Directory.Delete(_root, true);
    }

    [Test]
    public void Evaluate_ResolvesPlatformExpressionsAndCompileGlob()
    {
        var inputs = TtMacrossModule.EvaluateProjectCompileInputs(_projectFile);

        var expectedTfm = $"net{Environment.Version.Major}.0";
        Assert.That(inputs.References, Has.Some.EqualTo(
            Path.GetFullPath(Path.Combine(_root, "binaries", expectedTfm, "Engine.Window.dll"))));
        Assert.That(inputs.CompileFiles, Has.Some.Contains(Path.Combine("src", "Hello.cs")));
        Assert.That(inputs.CompileFiles, Has.Some.Contains(Path.Combine("macross", "Gen.cs")));
    }

    /// <summary>
    /// 指纹未变化时二次求值必须命中缓存（不重起 dotnet 进程），
    /// 热编译路径靠这一点把求值开销降为零。
    /// </summary>
    [Test]
    public void Evaluate_HitsFingerprintCache()
    {
        var first = TtMacrossModule.EvaluateProjectCompileInputs(_projectFile);
        var second = TtMacrossModule.EvaluateProjectCompileInputs(_projectFile);
        Assert.That(second, Is.SameAs(first));
    }
}
