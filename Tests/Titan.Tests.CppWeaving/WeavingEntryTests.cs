using CppWeaving;
using NUnit.Framework;
using System.Diagnostics;

namespace Titan.Tests.CppWeaving;

[TestFixture, NonParallelizable]
public class WeavingEntryTests
{
    private string mRoot = "";
    [SetUp] public void Setup()
    {
        mRoot = Path.Combine(Path.GetTempPath(), "TitanEntry_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(mRoot);
        File.WriteAllText(Path.Combine(mRoot, "EngineAll.sln"), "");
        foreach (var (dir, platform, arch) in new[] { ("Core.Mac", "PLATFORM_MAC", "ARM64"), ("Core.Window", "PLATFORM_WIN", "x64") })
        {
            Directory.CreateDirectory(Path.Combine(mRoot, dir));
            File.WriteAllText(Path.Combine(mRoot, dir, "pch.h"), "#pragma once\n");
            File.WriteAllText(Path.Combine(mRoot, dir, dir + ".vcxproj"), $"""
                <Project xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
                  <ImportGroup Label="Shared" />
                  <ItemDefinitionGroup Condition="'$(Configuration)|$(Platform)'=='Debug|{arch}'"><ClCompile><PreprocessorDefinitions>{platform};_DEBUG</PreprocessorDefinitions></ClCompile></ItemDefinitionGroup>
                  <ItemDefinitionGroup Condition="'$(Configuration)|$(Platform)'=='Release|{arch}'"><ClCompile><PreprocessorDefinitions>{platform};NDEBUG</PreprocessorDefinitions></ClCompile></ItemDefinitionGroup>
                </Project>
                """);
        }
    }
    [TearDown] public void Cleanup() { Directory.Delete(mRoot, true); }
    private (int Code, string Log) Run(params string[] args)
    {
        var start = new ProcessStartInfo("dotnet") { WorkingDirectory = mRoot, RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false };
        start.ArgumentList.Add(typeof(HppCollector).Assembly.Location);
        foreach (var arg in args) start.ArgumentList.Add(arg);
        using var process = Process.Start(start)!;
        var stdout = process.StandardOutput.ReadToEndAsync(); var stderr = process.StandardError.ReadToEndAsync();
        Assert.That(process.WaitForExit(30000), Is.True, "CppWeaving 超时");
        return (process.ExitCode, stdout.GetAwaiter().GetResult() + stderr.GetAwaiter().GetResult());
    }
    [TestCase("Mac", "Release", "Core.Mac", "codegen/Mac/NativeBinder")]
    [TestCase("Windows", "Debug", "Core.Window", "codegen/Windows/NativeBinder")]
    public void Entry_SelectsPlatformProjectAndDefaultOutput(string platform, string config, string project, string output)
    {
        var result = Run("Platform=" + platform, "Configuration=" + config, "TitanRoot=" + mRoot);
        Assert.That(result.Code, Is.Zero, result.Log);
        Assert.That(result.Log.Replace('\\', '/'), Does.Contain(project + "/" + project + ".vcxproj").And.Contain("Configuration=" + config));
        Assert.That(File.Exists(Path.Combine(mRoot, output, "CodeGen.vcxitems")), Is.True);
        Assert.That(File.Exists(Path.Combine(mRoot, output, "CodeGenCSharp.projitems")), Is.True);
        if (platform == "Mac") Assert.That(Directory.Exists(Path.Combine(mRoot, "codegen/NativeBinder")), Is.False);
    }
    [Test] public void Entry_UsesExplicitProjectAndSeparateOutputsWithoutLocalTemplates()
    {
        var cpp = Path.Combine(mRoot, "cpp"); var cs = Path.Combine(mRoot, "cs");
        var result = Run("Platform=Mac", "vcxproj=" + Path.Combine(mRoot, "Core.Mac/Core.Mac.vcxproj"), "Pch=" + Path.Combine(mRoot, "Core.Mac/pch.h"), "CppOut=" + cpp, "CsOut=" + cs, "TargetCppPOD=" + Path.Combine(cpp, "PODStructDefine.h"), "ModuleNC=Test.Native");
        Assert.That(result.Code, Is.Zero, result.Log);
        Assert.That(File.Exists(Path.Combine(cpp, "CodeGen.vcxitems")), Is.True);
        Assert.That(File.Exists(Path.Combine(cs, "CodeGenCSharp.projitems")), Is.True);
        Assert.That(File.Exists(Path.Combine(cpp, "CodeGenCSharp.projitems")), Is.False);
        Assert.That(File.Exists(Path.Combine(cs, "CodeGen.vcxitems")), Is.False);
    }
    [TestCase("Platform=Linux")]
    [TestCase("Configuration=Profile")]
    public void Entry_RejectsInvalidSelectionWithoutWritingOutputs(string arg)
    {
        var result = Run("TitanRoot=" + mRoot, arg);
        Assert.That(result.Code, Is.Not.Zero);
        Assert.That(Directory.Exists(Path.Combine(mRoot, "codegen")), Is.False);
    }
    [Test] public void Entry_ParseFailureDoesNotReplaceExistingOutputs()
    {
        var project = Path.Combine(mRoot, "Core.Window/Core.Window.vcxproj");
        File.WriteAllText(Path.Combine(mRoot, "Core.Window/Broken.h"), "// TR_CLASS\nstruct Broken { int a }\n");
        File.WriteAllText(Path.Combine(mRoot, "Core.Window/Broken.vcxitems"), "<Project><ItemGroup><ClInclude Include=\"$(MSBuildThisFileDirectory)Broken.h\" /></ItemGroup></Project>");
        File.WriteAllText(project, File.ReadAllText(project).Replace("<ImportGroup Label=\"Shared\" />", "<ImportGroup Label=\"Shared\"><Import Project=\"Broken.vcxitems\" Label=\"Shared\" /></ImportGroup>"));
        var output = Path.Combine(mRoot, "output"); Directory.CreateDirectory(output);
        var pod = Path.Combine(output, "PODStructDefine.h"); File.WriteAllText(pod, "existing sentinel");
        var result = Run("Platform=Windows", "TitanRoot=" + mRoot, "CppOut=" + output, "CsOut=" + output);
        Assert.That(result.Code, Is.Not.Zero, result.Log);
        Assert.That(File.ReadAllText(pod), Is.EqualTo("existing sentinel"));
        Assert.That(File.Exists(Path.Combine(output, "CodeGen.vcxitems")), Is.False);
    }

    [Test] public void Entry_MissingMacProjectNeverFallsBackToWindows()
    {
        File.Delete(Path.Combine(mRoot, "Core.Mac/Core.Mac.vcxproj"));
        var result = Run("TitanRoot=" + mRoot, "Platform=Mac");
        Assert.That(result.Code, Is.Not.Zero);
        Assert.That(result.Log, Does.Contain("Core.Mac"));
        Assert.That(Directory.Exists(Path.Combine(mRoot, "codegen")), Is.False);
    }
}
