using EngineNS.LeanCLR;
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;

namespace EngineNS.Tests.LeanCLR;

[TestFixture]
public class LeanClrAssemblyScannerTests
{
    private string mRoot = null!;

    // 任意托管程序集即可：扫描只读元数据名，不加载
    private static string ManagedDll => typeof(Assert).Assembly.Location;
    private static string ManagedDllName => typeof(Assert).Assembly.GetName().Name!;
    private static string EngineStubDll => Path.Combine(TestContext.CurrentContext.TestDirectory, TtLeanClrHostBinding.EngineStubAssemblyName + ".dll");

    [SetUp]
    public void SetUp()
    {
        mRoot = Path.Combine(Path.GetTempPath(), "TitanLeanClrScan_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(mRoot);
    }

    [TearDown]
    public void TearDown()
    {
        Directory.Delete(mRoot, true);
    }

    private string Place(string source, string relativePath)
    {
        var target = Path.Combine(mRoot, relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(target)!);
        File.Copy(source, target);
        return target;
    }

    [Test]
    public void Collect_ScansSubdirectories_AndNamesByMetadata()
    {
        var stub = Place(EngineStubDll, TtLeanClrHostBinding.EngineStubAssemblyName + ".dll");
        var renamed = Place(ManagedDll, Path.Combine("Survivor", "Renamed.dll"));

        var set = TtLeanClrAssemblyScanner.Collect(mRoot, Array.Empty<string>());

        Assert.That(set.Errors, Is.Empty);
        Assert.That(set.Assemblies.Select(i => (i.Key, i.Value)), Is.EquivalentTo(new[]
        {
            (TtLeanClrHostBinding.EngineStubAssemblyName, stub),
            (ManagedDllName, renamed),
        }));
        Assert.That(set.ShouldStartRuntime, Is.True);
    }

    [Test]
    public void Collect_WithoutEngineStubOrExtras_DoesNotStartRuntime()
    {
        Place(ManagedDll, "mscorlib_like.dll");

        var set = TtLeanClrAssemblyScanner.Collect(mRoot, Array.Empty<string>());

        Assert.That(set.Assemblies, Has.Count.EqualTo(1));
        Assert.That(set.ShouldStartRuntime, Is.False);
    }

    [Test]
    public void Collect_SameAssemblyNameTwice_ReportsError()
    {
        Place(ManagedDll, Path.Combine("A", "One.dll"));
        Place(ManagedDll, Path.Combine("B", "Two.dll"));

        var set = TtLeanClrAssemblyScanner.Collect(mRoot, Array.Empty<string>());

        Assert.That(set.Errors, Has.Count.EqualTo(1));
        Assert.That(set.Errors[0], Does.Contain(ManagedDllName));
    }

    [Test]
    public void Collect_NonManagedDll_IsSkippedWithWarning()
    {
        File.WriteAllBytes(Path.Combine(mRoot, "native.dll"), new byte[] { 1, 2, 3, 4 });

        var set = TtLeanClrAssemblyScanner.Collect(mRoot, Array.Empty<string>());

        Assert.That(set.Assemblies, Is.Empty);
        Assert.That(set.Errors, Is.Empty);
        Assert.That(set.Warnings, Has.Count.EqualTo(1));
    }

    [Test]
    public void Collect_ExtraFiles_AreAppendedAndStartRuntime()
    {
        var scanned = Place(ManagedDll, "Scanned.dll");
        var extraDir = Path.Combine(mRoot, "..", Path.GetFileName(mRoot) + "_extra");
        Directory.CreateDirectory(extraDir);
        var extra = Path.Combine(extraDir, "Extra.dll");
        File.Copy(EngineStubDll, extra);
        try
        {
            // 额外列表重复给出已扫描到的同一文件不算重名
            var set = TtLeanClrAssemblyScanner.Collect(mRoot, new[] { extra, scanned });

            Assert.That(set.Errors, Is.Empty);
            Assert.That(set.Assemblies.Select(i => i.Value), Is.EquivalentTo(new[] { scanned, Path.GetFullPath(extra) }));
            Assert.That(set.ShouldStartRuntime, Is.True);
        }
        finally
        {
            Directory.Delete(extraDir, true);
        }
    }

    [Test]
    public void Collect_MissingExtraFile_ReportsError()
    {
        var set = TtLeanClrAssemblyScanner.Collect(mRoot, new[] { Path.Combine(mRoot, "missing.dll") });

        Assert.That(set.Errors, Has.Count.EqualTo(1));
    }

    [Test]
    public void Collect_MissingScanDirectory_IsEmpty()
    {
        var set = TtLeanClrAssemblyScanner.Collect(Path.Combine(mRoot, "none"), Array.Empty<string>());

        Assert.That(set.Assemblies, Is.Empty);
        Assert.That(set.Errors, Is.Empty);
        Assert.That(set.ShouldStartRuntime, Is.False);
    }
}
