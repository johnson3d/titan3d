using System.Reflection;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Framework;

namespace Titan.Tests.CSharpCodeTools;

[TestFixture]
public class SourceGenerationTests
{
    private static string Strip(string text)
    {
        var type = typeof(global::CSharpCodeTools.ProjectSourceInputs).Assembly.GetType("CSharpCodeTools.LegacyGeneratedBlocks");
        Assert.That(type, Is.Not.Null, "缺少严格工具区块迁移器");
        try { return (string)type!.GetMethod("Strip")!.Invoke(null, new object[] { text, "Fixture.cs" })!; }
        catch (TargetInvocationException error) { throw error.InnerException!; }
    }

    [TestCase("RPC")]
    [TestCase("AutoSync")]
    [TestCase("Macross")]
    public void LegacyMigration_RemovesOnlyCompleteToolBlock(string mode)
    {
        var original = "// 用户注释\r\n#if USER\r\npublic class Keep {}\r\n#endif\r\n";
        var block = $"#if TitanEngine_AutoGen_{mode}\r\n#region TitanEngine_AutoGen_{mode}\r\n#if INNER\r\npublic class Generated {{}}\r\n#endif\r\n#endregion//TitanEngine_AutoGen_{mode}\r\n#endif//TitanEngine_AutoGen_{mode}\r\n";
        Assert.That(Strip(original + block + "// 尾部\r\n"), Is.EqualTo(original + "// 尾部\r\n"));
    }

    [Test]
    public void LegacyMigration_TrailingBlockDoesNotLeaveBlankLinesAtEof()
    {
        // 旧区块前的分隔空行在区块移除后会成为文件尾空行（git diff --check 失败）；只裁整行空白，保留末行原有行尾。
        var block = "#if TitanEngine_AutoGen_Macross\r\n#region TitanEngine_AutoGen_Macross\r\nclass Generated {}\r\n#endregion//TitanEngine_AutoGen_Macross\r\n#endif//TitanEngine_AutoGen_Macross";
        Assert.That(Strip("class Keep {} \r\n\r\n\r\n" + block), Is.EqualTo("class Keep {} \r\n"));
        Assert.That(Strip("class Keep {}\n\n" + block.Replace("\r\n", "\n") + "\n\n"), Is.EqualTo("class Keep {}\n"));
        Assert.That(Strip(block), Is.Empty);
    }

    [Test]
    public void LegacyMigration_PreservesMarkerTextInsideRawStringAndComment()
    {
        var code = """"
            class Keep { string Text = """
            #if TitanEngine_AutoGen_RPC
            #endregion//TitanEngine_AutoGen_RPC
            """; }
            /*
            #if TitanEngine_AutoGen_Macross
            */
            """";
        Assert.That(Strip(code), Is.EqualTo(code));
    }

    [TestCase("#if TitanEngine_AutoGen_RPC\n#region TitanEngine_AutoGen_RPC\nclass Generated {}\n")]
    [TestCase("#endif//TitanEngine_AutoGen_Macross\n")]
    [TestCase("#if TitanEngine_AutoGen_RPC\n#region TitanEngine_AutoGen_RPC\n#endregion//TitanEngine_AutoGen_RPC\n#endif//TitanEngine_AutoGen_Macross\n")]
    public void LegacyMigration_RejectsIncompleteOrMismatchedMarkers(string code)
    {
        Assert.That(() => Strip(code), Throws.TypeOf<InvalidDataException>().With.Message.Contains("Fixture.cs"));
    }

    [TestCase("#if TitanEngine_AutoGen_RPC\npublic void Keep() {}\n#region TitanEngine_AutoGen_RPC\n#endregion//TitanEngine_AutoGen_RPC\n#endif//TitanEngine_AutoGen_RPC\n")]
    [TestCase("#if TitanEngine_AutoGen_RPC\n#region TitanEngine_AutoGen_RPC\n#endregion//TitanEngine_AutoGen_RPC\n// 用户尾注\n#endif//TitanEngine_AutoGen_RPC\n")]
    [TestCase("#if TitanEngine_AutoGen_RPC\n#region TitanEngine_AutoGen_RPC\n#endregion//TitanEngine_AutoGen_RPC\n#else\nclass Keep {}\n#endif//TitanEngine_AutoGen_RPC\n")]
    public void LegacyMigration_RejectsContentOutsideToolRegion(string code)
    {
        Assert.That(() => Strip(code), Throws.TypeOf<InvalidDataException>().With.Message.Contains("Fixture.cs"));
    }

    [Test]
    public void LegacyMigration_PreservesUnapprovedBareMarker()
    {
        var code = "#if TitanEngine_AutoGen\n#region TitanEngine_AutoGen\nclass Keep {}\n#endregion//TitanEngine_AutoGen\n#endif//TitanEngine_AutoGen\n";
        Assert.That(Strip(code), Is.EqualTo(code));
    }

    [TestCase(false)]
    [TestCase(true)]
    public void LegacyMigrationCli_RejectsRootWithoutBusinessSourceDirectories(bool exists)
    {
        var root = Path.Combine(Path.GetTempPath(), "Titan Empty Migration " + Guid.NewGuid().ToString("N"));
        if (exists) Directory.CreateDirectory(root);
        try
        {
            var result = ManagedProjectTests.Run(typeof(global::CSharpCodeTools.LegacyGeneratedBlocks).Assembly.Location, "TitanRoot=" + root, "MigrateLegacyBlocks=Check");
            Assert.That(result.Code, Is.Not.Zero);
            Assert.That(result.Log, Does.Contain(root));
        }
        finally { if (exists) Directory.Delete(root, true); }
    }

    [Test]
    public void ExportInputs_AcceptsAndroidAndRejectsUnknownPlatforms()
    {
        var root = Path.Combine(Path.GetTempPath(), "Titan Export " + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            var runner = typeof(global::CSharpCodeTools.ProjectSourceInputs).Assembly.GetType("CSharpCodeTools.SourceGenerationRunner");
            Assert.That(runner, Is.Not.Null);
            var export = runner!.GetMethod("ExportInputs");
            Assert.That(export, Is.Not.Null);
            var arguments = new object?[] { root, "Engine", "Android", "Debug", "Engine Fixture.csproj", Path.Combine(root, "list.txt"), "CSharp", false };
            // Android 已通过平台校验，失败点应推进到缺失的生成目录。
            var android = Assert.Throws<TargetInvocationException>(() => export!.Invoke(null, arguments));
            Assert.That(android!.InnerException, Is.TypeOf<InvalidDataException>().With.Message.Contains("平台生成目录"));
            arguments[2] = "Linux";
            var unknown = Assert.Throws<TargetInvocationException>(() => export!.Invoke(null, arguments));
            Assert.That(unknown!.InnerException, Is.TypeOf<ArgumentException>());
        }
        finally { Directory.Delete(root, true); }
    }

    [Test]
    public void GeneratedManifests_ArePerConfiguration_BothConfigsRemainValid()
    {
        // 回归：清单曾按 owner 目录单份存放，Debug→Release 双轮生成后 Release 收尾，
        // 另一个配置的插件 ValidateOutputs-only 消费即报「缺少或不匹配的源码成功清单」。
        // 清单改为按 owner+configuration 子目录存放后，两个配置必须同时可校验。
        var root = Path.Combine(Path.GetTempPath(), "Titan PerConfig Manifest " + Guid.NewGuid().ToString("N"));
        var msbuildDir = Path.Combine(root, "BuildScript", "msbuild");
        Directory.CreateDirectory(msbuildDir);
        // Run 内部 LoadPlatformProjects 需要 ClonePlatformTargets 引擎侧输入采集目标
        File.Copy(Path.Combine(ManagedProjectTests.Root(), "BuildScript", "msbuild", "TitanSourceInputs.targets"), Path.Combine(msbuildDir, "TitanSourceInputs.targets"));
        try
        {
            var project = Path.Combine(root, "Synthetic.csproj");
            File.WriteAllText(project, "<Project Sdk=\"Microsoft.NET.Sdk\"><PropertyGroup><TargetFramework>net8.0</TargetFramework></PropertyGroup></Project>");
            File.WriteAllText(Path.Combine(root, "Item.cs"), "namespace EngineNS.Test { [TtRpcClass(RunTarget=ERunTarget.Client,Executer=EExecuter.Client)] [UAutoSync] public partial class Item { [Meta][TtRpcMethod(Index=1)] public void Ping(int value, TtCallContext context) {} [UAutoSync(Index=0)] public int Value { get; set; } } }");
            var binder = Path.Combine(root, "codegen", "Mac", "NativeBinder");
            Directory.CreateDirectory(binder);
            File.WriteAllText(Path.Combine(binder, "CodeGenCSharp.projitems"), "<Project />");
            var runner = typeof(global::CSharpCodeTools.ProjectSourceInputs).Assembly.GetType("CSharpCodeTools.SourceGenerationRunner");
            Assert.That(runner, Is.Not.Null);
            var run = runner!.GetMethod("Run");
            Assert.That(run, Is.Not.Null);
            var export = runner.GetMethod("ExportInputs");
            Assert.That(export, Is.Not.Null);
            foreach (var configuration in new[] { "Debug", "Release" })
            {
                run!.Invoke(null, new object?[] { root, project, "Mac", configuration, null, new[] { "Rpc", "AutoSync", "Macross" } });
                foreach (var category in new[] { "Rpc", "Property", "Macross" })
                {
                    var manifest = Path.Combine(root, "codegen", "Mac", category, "Engine", configuration, ".outputs.json");
                    Assert.That(File.Exists(manifest), $"{category} 缺少 {configuration} 清单：{manifest}");
                    Assert.That(File.ReadAllText(manifest), Does.Contain($"\"Configuration\": \"{configuration}\""));
                }
                var list = Path.Combine(root, $"inputs-{configuration}.txt");
                export!.Invoke(null, new object?[] { root, "Engine", "Mac", configuration, project, list, "CSharp", true });
                Assert.That(File.ReadAllText(list), Does.Contain(configuration));
            }
        }
        finally { Directory.Delete(root, true); }
    }

    [Test]
    public void LeanClrStub_DoesNotConsumeRegularPluginGeneratedSources()
    {
        var result = ManagedProjectTests.Run("msbuild", Path.Combine(ManagedProjectTests.Root(), "Plugins/Game/Survivor/Survivor.LeanClrStub/Survivor.LeanClrStub.csproj"), "-nologo", "-t:TitanGenerateGlueCode,TitanConsumeManagedGeneratedInputs", "-p:_CSharpCodeToolsDll=does-not-exist.dll", "-getItem:Compile");
        Assert.That(result.Code, Is.Zero, result.Log);
        using var json = System.Text.Json.JsonDocument.Parse(result.Log);
        var files = json.RootElement.GetProperty("Items").GetProperty("Compile").EnumerateArray().Select(i => i.GetProperty("FullPath").GetString()!).ToArray();
        Assert.That(files, Has.None.Contains("codegen"));
        var template = File.ReadAllText(Path.Combine(ManagedProjectTests.Root(), "BuildScript", "scaffold", "New-LeanClrStub.ps1"));
        Assert.That(template, Does.Contain("<TitanConsumeSourceOutputs>false</TitanConsumeSourceOutputs>"), "新桩模板必须遵守相同的消费隔离");
    }

    private static string[] MigrateFiles(string[] paths, bool apply)
    {
        var method = typeof(global::CSharpCodeTools.LegacyGeneratedBlocks).GetMethod("MigrateFiles");
        Assert.That(method, Is.Not.Null, "缺少全量预检查、保留编码的文件迁移入口");
        try { return (string[])method!.Invoke(null, new object[] { paths, apply })!; }
        catch (TargetInvocationException error) { throw error.InnerException!; }
    }

    [TestCase("utf8")]
    [TestCase("utf8-bom")]
    [TestCase("utf16-le")]
    [TestCase("utf16-be")]
    [TestCase("utf32-le")]
    public void LegacyFileMigration_PreservesEncodingAndNonToolBytesAndSecondRunIsNoOp(string encodingName)
    {
        var directory = Path.Combine(Path.GetTempPath(), "Titan Migration " + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            System.Text.Encoding encoding = encodingName switch
            {
                "utf8" => new System.Text.UTF8Encoding(false, true),
                "utf8-bom" => new System.Text.UTF8Encoding(true, true),
                "utf16-le" => new System.Text.UnicodeEncoding(false, true, true),
                "utf16-be" => new System.Text.UnicodeEncoding(true, true, true),
                _ => new System.Text.UTF32Encoding(false, true, true)
            };
            var keep = "// 用户注释与空格  \r\n#if USER\nclass Keep {}\n#endif\r\n";
            var block = "#if TitanEngine_AutoGen_Macross\r\n#region TitanEngine_AutoGen_Macross\r\nclass Generated {}\r\n#endregion//TitanEngine_AutoGen_Macross\r\n#endif//TitanEngine_AutoGen_Macross\r\n";
            var suffix = "// 尾部，无换行";
            var original = encoding.GetPreamble().Concat(encoding.GetBytes(keep + block + suffix)).ToArray();
            var expected = encoding.GetPreamble().Concat(encoding.GetBytes(keep + suffix)).ToArray();
            var file = Path.Combine(directory, "Item.cs");
            File.WriteAllBytes(file, original);
            Assert.That(MigrateFiles(new[] { file, file }, false), Is.EqualTo(new[] { file }));
            Assert.That(File.ReadAllBytes(file), Is.EqualTo(original), "预检查不能写入源码");
            Assert.That(MigrateFiles(new[] { file }, true), Is.EqualTo(new[] { file }));
            Assert.That(File.ReadAllBytes(file), Is.EqualTo(expected));
            var modified = File.GetLastWriteTimeUtc(file);
            Assert.That(MigrateFiles(new[] { file }, true), Is.Empty);
            Assert.That(File.GetLastWriteTimeUtc(file), Is.EqualTo(modified));
        }
        finally { Directory.Delete(directory, true); }
    }

    [TestCase(false)]
    [TestCase(true)]
    public void LegacyFileMigration_InvalidLaterFileCannotPartiallyRewriteEarlierFile(bool lockedFile)
    {
        var directory = Path.Combine(Path.GetTempPath(), "Titan Migration Failure " + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var valid = "// 保留\n#if TitanEngine_AutoGen_RPC\n#region TitanEngine_AutoGen_RPC\nclass Generated {}\n#endregion//TitanEngine_AutoGen_RPC\n#endif//TitanEngine_AutoGen_RPC\n";
            var first = Path.Combine(directory, "A.cs");
            var second = Path.Combine(directory, "Z.cs");
            File.WriteAllText(first, valid);
            File.WriteAllText(second, lockedFile ? valid : "#if TitanEngine_AutoGen_RPC\n");
            var before = File.ReadAllBytes(first);
            using var gate = lockedFile ? File.Open(second, FileMode.Open, FileAccess.Read, FileShare.Read) : null;
            Assert.That(() => MigrateFiles(new[] { first, second }, true), Throws.Exception.With.Message.Contains("Z.cs"));
            Assert.That(File.ReadAllBytes(first), Is.EqualTo(before));
        }
        finally { Directory.Delete(directory, true); }
    }

    [Test]
    public void LegacyMigrationCli_RequiresMatchingBaselineAndNeverScansGeneratedOrUnrelatedFiles()
    {
        var root = Path.Combine(Path.GetTempPath(), "Titan Migration CLI " + Guid.NewGuid().ToString("N"));
        var source = Path.Combine(root, "CSharpCode", "Item.cs");
        var plugin = Path.Combine(root, "Plugins", "Portable", "Item.cs");
        var baseline = Path.Combine(root, "artifacts", "platform-codegen-baseline");
        var block = "// 保留\n#if TitanEngine_AutoGen_RPC\n#region TitanEngine_AutoGen_RPC\nclass Generated {}\n#endregion//TitanEngine_AutoGen_RPC\n#endif//TitanEngine_AutoGen_RPC\n";
        Directory.CreateDirectory(Path.GetDirectoryName(source)!);
        Directory.CreateDirectory(Path.GetDirectoryName(plugin)!);
        try
        {
            File.WriteAllText(source, block);
            File.WriteAllText(plugin, block);
            var ignored = new[] { "Plugins/Portable/bin/Invalid.cs", "Plugins/Portable/obj/Invalid.cs", "Plugins/Portable/build-debug/Invalid.cs", "codegen/Windows/Invalid.cs", "Module/Unrelated.cs" };
            foreach (var name in ignored)
            {
                var file = Path.Combine(root, name);
                Directory.CreateDirectory(Path.GetDirectoryName(file)!);
                File.WriteAllText(file, "#if TitanEngine_AutoGen_RPC\n");
            }
            var tool = typeof(global::CSharpCodeTools.LegacyGeneratedBlocks).Assembly.Location;
            var check = ManagedProjectTests.Run(tool, "TitanRoot=" + root, "MigrateLegacyBlocks=Check");
            Assert.That(check.Code, Is.Zero, check.Log);
            Assert.That(check.Log, Does.Contain(source).And.Contain(plugin));
            Assert.That(File.ReadAllText(source), Is.EqualTo(block));
            var failure = ManagedProjectTests.Run(tool, "TitanRoot=" + root, "MigrateLegacyBlocks=Apply");
            Assert.That(failure.Code, Is.Not.Zero);
            Assert.That(failure.Log, Does.Contain("基线"));
            Assert.That(File.ReadAllText(source), Is.EqualTo(block));
            foreach (var file in new[] { source, plugin })
            {
                var dest = Path.Combine(baseline, Path.GetRelativePath(root, file));
                Directory.CreateDirectory(Path.GetDirectoryName(dest)!);
                File.Copy(file, dest);
            }
            File.AppendAllText(Path.Combine(baseline, "Plugins", "Portable", "Item.cs"), "// 错误基线");
            failure = ManagedProjectTests.Run(tool, "TitanRoot=" + root, "MigrateLegacyBlocks=Apply");
            Assert.That(failure.Code, Is.Not.Zero);
            Assert.That(failure.Log, Does.Contain(plugin));
            Assert.That(File.ReadAllText(source), Is.EqualTo(block));
            File.WriteAllText(Path.Combine(baseline, "Plugins", "Portable", "Item.cs"), block);
            var applied = ManagedProjectTests.Run(tool, "TitanRoot=" + root, "MigrateLegacyBlocks=Apply");
            Assert.That(applied.Code, Is.Zero, applied.Log);
            Assert.That(File.ReadAllText(source), Is.EqualTo("// 保留\n"));
            Assert.That(File.ReadAllText(plugin), Is.EqualTo("// 保留\n"));
            Assert.That(File.ReadAllText(Path.Combine(baseline, "CSharpCode", "Item.cs")), Is.EqualTo(block));
            foreach (var name in ignored) Assert.That(File.ReadAllText(Path.Combine(root, name)), Is.EqualTo("#if TitanEngine_AutoGen_RPC\n"));
        }
        finally { Directory.Delete(root, true); }
    }

    [TestCase("Rpc")]
    [TestCase("AutoSync")]
    public void Generate_MergesAttributedAndUnattributedNestedPartialDeclarations(string mode)
    {
        var directory = Path.Combine(Path.GetTempPath(), "Titan Partial RPC " + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var project = Path.Combine(directory, "Synthetic.csproj");
            File.WriteAllText(project, "<Project Sdk=\"Microsoft.NET.Sdk\"><PropertyGroup><TargetFramework>net8.0</TargetFramework></PropertyGroup></Project>");
            File.WriteAllText(Path.Combine(directory, "First.cs"), "namespace EngineNS.Test; public partial class Outer<T> where T : class { [TtRpcClass(RunTarget=ERunTarget.Client,Executer=EExecuter.Client,CallerInClass=true)] [UAutoSync] public partial class Inner<U> where U : unmanaged { [TtRpcMethod(Index=1)] public void One(TtCallContext context) {} [UAutoSync(Index=0)] public int Value { get; set; } } }");
            File.WriteAllText(Path.Combine(directory, "Second.cs"), "namespace EngineNS.Test; public partial class Outer<T> where T : class { public partial class Inner<U> where U : unmanaged { [TtRpcMethod(Index=2)] public void Other(TtCallContext context) {} [UAutoSync(Index=1)] public int Another { get; set; } } }");
            var inputs = global::CSharpCodeTools.ProjectSourceInputs.Load(project, "Mac", "Debug");
            global::CSharpCodeTools.UCodeManagerBase manager = mode == "Rpc" ? new global::CSharpCodeTools.URpcCodeManager() : new global::CSharpCodeTools.PropertyGen.UPropertyCodeManager();
            manager.SetProjectInputs(inputs);
            var output = Path.Combine(directory, "output");
            Directory.CreateDirectory(output);
            if (mode == "Rpc") ((global::CSharpCodeTools.URpcCodeManager)manager).GatherRpcClass(output, Path.Combine(directory, "callers"));
            else ((global::CSharpCodeTools.PropertyGen.UPropertyCodeManager)manager).GatherAutoSyncClass(output);
            var code = string.Join("\n", Directory.GetFiles(output, "*.cs").Select(File.ReadAllText));
            Assert.That(code, Does.Contain(mode == "Rpc" ? "RPC_Other" : "Another"));
            var tree = CSharpSyntaxTree.ParseText(code);
            Assert.That(tree.GetDiagnostics().Where(d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error), Is.Empty);
            var classes = tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().ToArray();
            var inner = classes.Single(c => c.Identifier.Text == "Inner");
            Assert.That(inner.Parent, Is.TypeOf<ClassDeclarationSyntax>());
            Assert.That(((ClassDeclarationSyntax)inner.Parent!).Identifier.Text, Is.EqualTo("Outer"));
            Assert.That(inner.ConstraintClauses.ToString(), Does.Contain("unmanaged"));
            if (mode == "Rpc") Assert.That(classes.Single(c => c.Identifier.Text == "Inner_RpcCaller").TypeParameterList!.Parameters.Single().Identifier.Text, Is.EqualTo("U"));
            else Assert.That(inner.Members.OfType<PropertyDeclarationSyntax>().Count(p => p.Identifier.Text == "Flags"), Is.EqualTo(1));
        }
        finally { Directory.Delete(directory, true); }
    }

    [Test]
    public void Macross_MergesPartialMembersAndCompilesNestedGenericWrappersWithAlias()
    {
        var directory = Path.Combine(Path.GetTempPath(), "Titan Partial Generation " + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var project = Path.Combine(directory, "Synthetic.csproj");
            File.WriteAllText(project, "<Project Sdk=\"Microsoft.NET.Sdk\"><PropertyGroup><TargetFramework>net8.0</TargetFramework></PropertyGroup></Project>");
            var first = "using Number = System.Int32; namespace EngineNS.Test; public partial class Outer<T> where T : class { public partial class Inner<U> where U : unmanaged { [Meta] public Number One(Number value) => value; [Meta] public V Two<V>(V value) => value; } }";
            var second = "namespace EngineNS.Test; public partial class Outer<T> where T : class { public partial class Inner<U> where U : unmanaged { [Meta] public int Three() => 3; } }";
            File.WriteAllText(Path.Combine(directory, "First.cs"), first);
            File.WriteAllText(Path.Combine(directory, "Second.cs"), second);
            var inputs = global::CSharpCodeTools.ProjectSourceInputs.Load(project, "Mac", "Debug");
            var manager = new global::CSharpCodeTools.Macross.UMacrossClassManager();
            manager.SetProjectInputs(inputs);
            var output = Path.Combine(directory, "output");
            Directory.CreateDirectory(output);
            manager.GatherMacrossClass(output);
            var generated = Directory.GetFiles(output, "*.cs").Select(File.ReadAllText).ToArray();
            var methods = generated.SelectMany(c => CSharpSyntaxTree.ParseText(c).GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>()).Select(m => m.Identifier.Text).ToArray();
            Assert.That(methods, Is.EquivalentTo(new[] { "macross_One", "macross_Two", "macross_Three" }));
            var support = "namespace EngineNS { public class MetaAttribute : System.Attribute {} } namespace EngineNS.Macross { public class TtMacrossStackTracer { public object TopFrame => null; } }";
            var compilation = Microsoft.CodeAnalysis.CSharp.CSharpCompilation.Create("Wrappers", generated.Append(first).Append(second).Append(support).Select(c => CSharpSyntaxTree.ParseText(c)), new[] { Microsoft.CodeAnalysis.MetadataReference.CreateFromFile(typeof(object).Assembly.Location) }, new CSharpCompilationOptions(Microsoft.CodeAnalysis.OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true));
            using var stream = new MemoryStream();
            var result = compilation.Emit(stream);
            Assert.That(result.Success, Is.True, string.Join("\n", result.Diagnostics));
            Assert.That(File.ReadAllText(Path.Combine(directory, "First.cs")), Is.EqualTo(first));
            Assert.That(File.ReadAllText(Path.Combine(directory, "Second.cs")), Is.EqualTo(second));
        }
        finally { Directory.Delete(directory, true); }
    }

    [TestCase("Rpc")]
    [TestCase("AutoSync")]
    [TestCase("Macross")]
    public void Generate_UsesProjectSymbolsAndSeparateFilesWithoutChangingInputs(string mode)
    {
        var directory = Path.Combine(Path.GetTempPath(), "Titan Source Generation " + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var project = Path.Combine(directory, "Synthetic.csproj");
            File.WriteAllText(project, "<Project Sdk=\"Microsoft.NET.Sdk\"><PropertyGroup><TargetFramework>net8.0</TargetFramework><DefineConstants>$(DefineConstants);PMac</DefineConstants></PropertyGroup></Project>");
            var source = Path.Combine(directory, "Item.cs");
            File.WriteAllText(source, "namespace EngineNS.Test { [TtRpcClass(RunTarget=ERunTarget.Client,Executer=EExecuter.Client)] [UAutoSync] public partial class Item {\n#if PMac\n[Meta][TtRpcMethod(Index=1)] public void Mac(int value, TtCallContext context) {}\n[UAutoSync(Index=0)] public int Value { get; set; }\n#else\n[Meta][TtRpcMethod(Index=2)] public void Windows(int value, TtCallContext context) {}\n#endif\n} }\n");
            var before = File.ReadAllBytes(source);
            var inputs = global::CSharpCodeTools.ProjectSourceInputs.Load(project, "Mac", "Debug");
            global::CSharpCodeTools.UCodeManagerBase manager = mode switch
            {
                "Rpc" => new global::CSharpCodeTools.URpcCodeManager(),
                "AutoSync" => new global::CSharpCodeTools.PropertyGen.UPropertyCodeManager(),
                _ => new global::CSharpCodeTools.Macross.UMacrossClassManager()
            };
            var bind = manager.GetType().GetMethod("SetProjectInputs");
            Assert.That(bind, Is.Not.Null, "生成器必须使用有效工程源码及 ParseOptions");
            bind!.Invoke(manager, new object[] { inputs });
            var output = Path.Combine(directory, "output");
            Directory.CreateDirectory(output);
            switch (mode)
            {
                case "Rpc": ((global::CSharpCodeTools.URpcCodeManager)manager).GatherRpcClass(output, output); break;
                case "AutoSync": ((global::CSharpCodeTools.PropertyGen.UPropertyCodeManager)manager).GatherAutoSyncClass(output); break;
                default: ((global::CSharpCodeTools.Macross.UMacrossClassManager)manager).GatherMacrossClass(output); break;
            }
            Assert.That(File.ReadAllBytes(source), Is.EqualTo(before));
            var files = Directory.GetFiles(output, "*.cs", SearchOption.AllDirectories);
            Assert.That(files, Is.Not.Empty);
            var generated = string.Join("\n", files.Select(File.ReadAllText));
            Assert.That(generated, Does.Contain(mode == "Rpc" ? "RPC_Mac" : mode == "AutoSync" ? "AutoSyncWriteValue" : "macross_Mac"));
            Assert.That(generated, Does.Not.Contain("RPC_Windows").And.Not.Contain("macross_Windows"));
            Assert.That(files.SelectMany(f => CSharpSyntaxTree.ParseText(File.ReadAllText(f)).GetDiagnostics()).Where(d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error), Is.Empty);
        }
        finally { Directory.Delete(directory, true); }
    }
}
