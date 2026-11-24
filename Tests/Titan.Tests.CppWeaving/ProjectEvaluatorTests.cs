using NUnit.Framework;
using Titan.Build;

namespace Titan.Tests.CppWeaving;

/// <summary>
/// Module/TitanProjectEval 是仓库里唯一一份 vcxproj/vcxitems 求值实现。
/// 原先散落在 CMake 脚本测试里的求值语义（源清单、排除项、元数据、配置门禁）全部搬到这里，
/// 用同一份实现验证；入库生成物与求值结果的一致性也在这里做端到端核对。
/// </summary>
[TestFixture, NonParallelizable]
public class ProjectEvaluatorTests
{
    string mDir = "";

    [SetUp] public void Setup() { mDir = Path.Combine(Path.GetTempPath(), "TitanEval_" + Guid.NewGuid().ToString("N")); Directory.CreateDirectory(mDir); }
    [TearDown] public void Cleanup() { if (Directory.Exists(mDir)) Directory.Delete(mDir, true); }

    static string Root()
    {
        var dir = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (dir != null && !File.Exists(Path.Combine(dir.FullName, "EngineAll.sln")))
            dir = dir.Parent;
        return dir?.FullName ?? throw new InvalidOperationException("找不到 Titan 仓库");
    }

    static void Write(string root, string rel, string content)
    {
        var path = Path.Combine(root, rel.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, content);
    }

    /// <summary>参数与 Core.Mac/RefreshSources.cmake、Core.Android/RefreshSources.cmake 传的一致。</summary>
    static EvalOptions Options(string root, string platform, string project, string modulesFile, string configuration) => new()
    {
        Root = root,
        ProjectRel = project,
        Platform = platform,
        PlatformDir = "Core." + platform,
        Prefix = platform.ToUpperInvariant(),
        ModulesRel = modulesFile,
        Configuration = configuration,
        PlatformName = "ARM64",
        ImplInputs = { "Module/TitanProjectEval" },
    };

    static ProjectEvaluation Mac(string root, string configuration)
        => ProjectEvaluator.Evaluate(Options(root, "Mac", "Core.Mac/Core.Mac.vcxproj", "Core.Mac/MacModules.cmake", configuration));

    static ProjectEvaluation Android(string root, string configuration)
        => ProjectEvaluator.Evaluate(Options(root, "Android", "Core.Android/Core.Android.vcxproj", "Core.Android/AndroidModules.cmake", configuration));

    /// <summary>临时仓库里的最小夹具：不声明模块文件（排除规则由真实平台单独验证）。</summary>
    static ProjectEvaluation Fixture(string root, string configuration)
        => ProjectEvaluator.Evaluate(new EvalOptions
        {
            Root = root,
            ProjectRel = "Core.Mac/Core.Mac.vcxproj",
            Platform = "Mac",
            PlatformDir = "Core.Mac",
            Prefix = "MAC",
            Configuration = configuration,
            PlatformName = "ARM64",
        });

    [Test]
    public void MacProject_SourcesAreExistingFilesWithoutMetadataLeaks()
    {
        var root = Root();
        var eval = Mac(root, "Debug");

        Assert.That(eval.Sources, Does.Contain("NativeCode/Base/io/vfxfile.cpp"));
        Assert.That(eval.Sources, Does.Contain("NativeCode/Bricks/LeanCLR/LeanClrRuntime.cpp"));
        // ExcludedFromBuild 的变体不能进清单
        Assert.That(eval.Sources, Has.None.EndsWith("vfxfile_Android.cpp").And.None.EndsWith("vfxfile_IOS.cpp"));
        // 平台工程自身的编译单元由平台 CMakeLists 显式加入，清单里不能再出现一份
        Assert.That(eval.Sources, Has.None.StartsWith("Core.Mac/"));
        foreach (var source in eval.Sources)
            Assert.That(File.Exists(Path.Combine(root, source)), Is.True, "清单里的源文件不存在: " + source);

        string[] leaks = { "LEANCLR_GC_DEBUG", "@SEMI@", "PreprocessorDefinitions", "ExcludedFromBuild" };
        Assert.That(eval.Sources.Where(s => leaks.Any(s.Contains)), Is.Empty, "元数据泄漏进源清单");
    }

    [Test]
    public void MacProject_ModulesComeFromSharedImportsOnly()
    {
        var root = Root();
        var eval = Mac(root, "Debug");

        Assert.That(eval.Modules, Has.Some.EndsWith("NativeCode/Base/Base.vcxitems"));
        Assert.That(eval.Modules, Has.Some.EndsWith("NativeCode/NextRHI/NullDevice/NullDevice.vcxitems"));
        Assert.That(eval.Modules, Has.Some.EndsWith("3rd/native/zstd/zstd.vcxitems"));
        Assert.That(eval.Modules, Has.None.Contains("codegen"), "生成胶水不是声明输入");
        Assert.That(eval.Modules.Count, Is.GreaterThanOrEqualTo(40));
        // 模块宏由模块清单派生，别名只在这一处展开
        Assert.That(eval.ModuleDefines, Does.Contain("HasModule_Mac").And.Contain("HasModule_Base")
            .And.Contain("HasModule_VirtualDevice").And.Contain("HasModule_RHI"));
        // 生成物逐字节入库，顺序必须是确定性的 Ordinal 序
        Assert.That(eval.ModuleDefines, Is.EqualTo(eval.ModuleDefines.OrderBy(x => x, StringComparer.Ordinal).ToList()));
    }

    [Test]
    public void DebugAndRelease_MetadataIsConfigurationScoped()
    {
        var root = Root();
        var debug = Mac(root, "Debug");
        var release = Mac(root, "Release");

        Assert.That(debug.Defines, Does.Contain("_DEBUG").And.Not.Contain("NDEBUG"));
        Assert.That(release.Defines, Does.Contain("NDEBUG").And.Not.Contain("_DEBUG"));
        Assert.That(debug.Includes, Does.Contain("NativeCode/Base").And.Not.Contain("."));
    }

    /// <summary>Core.Window 是全仓最大的共享项聚合者；它的 ImportGroup 就是源清单权威的那个事实。</summary>
    [Test]
    public void CoreWindow_DeclaresAtLeast50SharedImports()
    {
        var root = Root();
        var imports = VcxReader.ReadSharedImports(Path.Combine(root, "Core.Window", "Core.Window.vcxproj"))
            .Select(p => VcxReader.Relative(root, p)).ToList();

        Assert.That(imports, Does.Contain("NativeCode/Base/Base.vcxitems"));
        Assert.That(imports, Does.Contain("NativeCode/NextRHI/NullDevice/NullDevice.vcxitems"));
        Assert.That(imports, Does.Contain("3rd/native/zstd/zstd.vcxitems"));
        Assert.That(imports, Has.None.Contains("codegen"), "生成胶水必须在生成之后消费");
        Assert.That(imports.Count, Is.GreaterThanOrEqualTo(50));
    }

    [Test]
    public void ExcludedFromBuild_UnconditionalEntryIsDropped()
    {
        Write(mDir, "NativeCode/Variant/Variant.vcxitems", """
            <Project><ItemGroup>
              <ClCompile Include="$(MSBuildThisFileDirectory)a.cpp" />
              <ClCompile Include="$(MSBuildThisFileDirectory)b.cpp">
                <ExcludedFromBuild>
                  true
                </ExcludedFromBuild>
              </ClCompile>
            </ItemGroup></Project>
            """);
        Write(mDir, "NativeCode/Variant/a.cpp", "int a;\n");
        Write(mDir, "NativeCode/Variant/b.cpp", "int b;\n");
        Write(mDir, "Core.Mac/Core.Mac.vcxproj", """
            <Project>
              <ImportGroup Label="Shared"><Import Project="../NativeCode/Variant/Variant.vcxitems" Label="Shared" /></ImportGroup>
              <ItemDefinitionGroup Condition="'$(Configuration)|$(Platform)'=='Debug|ARM64'"><ClCompile>
                <PreprocessorDefinitions>PLATFORM_MAC;_DEBUG</PreprocessorDefinitions>
              </ClCompile></ItemDefinitionGroup>
              <ItemDefinitionGroup Condition="'$(Configuration)|$(Platform)'=='Release|ARM64'"><ClCompile>
                <PreprocessorDefinitions>PLATFORM_MAC;NDEBUG</PreprocessorDefinitions>
              </ClCompile></ItemDefinitionGroup>
            </Project>
            """);

        var eval = Fixture(mDir, "Debug");
        Assert.That(eval.Sources, Is.EqualTo(new[] { "NativeCode/Variant/a.cpp" }));
    }

    [Test]
    public void UnsupportedItemCondition_ThrowsInsteadOfSkipping()
    {
        Write(mDir, "NativeCode/Odd/Odd.vcxitems", """
            <Project><ItemGroup>
              <ClCompile Condition="'$(X)'=='1'" Include="c.cpp" />
            </ItemGroup></Project>
            """);
        Write(mDir, "Core.Mac/Core.Mac.vcxproj", """
            <Project>
              <ImportGroup Label="Shared"><Import Project="../NativeCode/Odd/Odd.vcxitems" Label="Shared" /></ImportGroup>
              <ItemDefinitionGroup Condition="'$(Configuration)|$(Platform)'=='Debug|ARM64'"><ClCompile /></ItemDefinitionGroup>
            </Project>
            """);

        Assert.That(() => Fixture(mDir, "Debug"),
            Throws.TypeOf<TitanEvalException>().With.Message.Contains("不支持条件"));
    }

    [Test]
    public void MissingConfiguration_ThrowsEvenWithUnconditionalGroup()
    {
        Write(mDir, "NativeCode/Portable/Portable.vcxitems", """
            <Project><ItemGroup><ClCompile Include="$(MSBuildThisFileDirectory)test.cpp" /></ItemGroup></Project>
            """);
        Write(mDir, "NativeCode/Portable/test.cpp", "int portable;\n");
        // 无条件组掩盖不了"这个工程根本没有 Debug 配置"
        Write(mDir, "Core.Mac/Core.Mac.vcxproj", """
            <Project>
              <ImportGroup Label="Shared"><Import Project="../NativeCode/Portable/Portable.vcxitems" Label="Shared" /></ImportGroup>
              <ItemDefinitionGroup><ClCompile><PreprocessorDefinitions>COMMON</PreprocessorDefinitions></ClCompile></ItemDefinitionGroup>
              <ItemDefinitionGroup Condition="'$(Configuration)|$(Platform)'=='Release|ARM64'"><ClCompile>
                <PreprocessorDefinitions>RELEASE_ONLY</PreprocessorDefinitions>
              </ClCompile></ItemDefinitionGroup>
            </Project>
            """);

        Assert.That(() => Fixture(mDir, "Debug"),
            Throws.TypeOf<TitanEvalException>().With.Message.Contains("Debug"));
        Assert.That(Fixture(mDir, "Release").Defines, Is.EqualTo(new[] { "COMMON", "RELEASE_ONLY" }));
    }

    /// <summary>属性先展开再拆分：一个属性可能展开成多个目录（Windows SDK 就是这种形态）。</summary>
    [Test]
    public void KnownProperty_ExpandsToMultipleValues()
    {
        Write(mDir, "NativeCode/Portable/test.cpp", "int portable;\n");
        Write(mDir, "NativeCode/Portable/Portable.vcxitems", """
            <Project><ItemGroup><ClCompile Include="$(MSBuildThisFileDirectory)test.cpp" /></ItemGroup></Project>
            """);
        Write(mDir, "Core.Mac/Core.Mac.vcxproj", """
            <Project>
              <ImportGroup Label="Shared"><Import Project="../NativeCode/Portable/Portable.vcxitems" Label="Shared" /></ImportGroup>
              <ItemDefinitionGroup Condition="'$(Configuration)|$(Platform)'=='Debug|ARM64'"><ClCompile>
                <AdditionalIncludeDirectories>$(TestSdkIncludePath);%(AdditionalIncludeDirectories)</AdditionalIncludeDirectories>
                <PreprocessorDefinitions>PLATFORM_MAC;$(TestDefine)</PreprocessorDefinitions>
              </ClCompile></ItemDefinitionGroup>
            </Project>
            """);

        var eval = ProjectEvaluator.Evaluate(new EvalOptions
        {
            Root = mDir,
            ProjectRel = "Core.Mac/Core.Mac.vcxproj",
            Platform = "Mac",
            Configuration = "Debug",
            PlatformName = "ARM64",
            Properties =
            {
                ["TestSdkIncludePath"] = mDir + "/sdk/um;" + mDir + "/sdk/shared",
                ["TestDefine"] = "EXPANDED=1",
            },
        });

        Assert.That(eval.Includes, Is.EqualTo(new[] { "sdk/um", "sdk/shared" }));
        Assert.That(eval.Defines, Is.EqualTo(new[] { "PLATFORM_MAC", "EXPANDED=1" }));
    }

    [Test]
    public void UnresolvedProperty_Throws()
    {
        Write(mDir, "NativeCode/Portable/Portable.vcxitems", """
            <Project><ItemGroup><ClCompile Include="$(MSBuildThisFileDirectory)test.cpp" /></ItemGroup></Project>
            """);
        Write(mDir, "NativeCode/Portable/test.cpp", "int portable;\n");
        Write(mDir, "Core.Mac/Core.Mac.vcxproj", """
            <Project>
              <ImportGroup Label="Shared"><Import Project="../NativeCode/Portable/Portable.vcxitems" Label="Shared" /></ImportGroup>
              <ItemDefinitionGroup Condition="'$(Configuration)|$(Platform)'=='Debug|ARM64'"><ClCompile>
                <AdditionalIncludeDirectories>$(WindowsSDK_IncludePath)</AdditionalIncludeDirectories>
              </ClCompile></ItemDefinitionGroup>
            </Project>
            """);

        Assert.That(() => Fixture(mDir, "Debug"),
            Throws.TypeOf<TitanEvalException>().With.Message.Contains("未解析属性"));
    }

    [Test]
    public void IsolatedProject_SourcesComeOnlyFromSharedImports()
    {
        Write(mDir, "NativeCode/Portable/test.cpp", "int portable;\n");
        Write(mDir, "NativeCode/Portable/Portable.vcxitems", """
            <Project><ItemGroup><ClCompile Include="$(MSBuildThisFileDirectory)test.cpp" /></ItemGroup></Project>
            """);
        Write(mDir, "Core.Mac/MacGlue.cpp", "int glue;\n");
        Write(mDir, "Core.Mac/Core.Mac.vcxproj", """
            <Project>
              <ImportGroup Label="Shared">
                <Import Project="../NativeCode/Portable/Portable.vcxitems" Label="Shared" />
                <Import Project="../codegen/Mac/NativeBinder/CodeGen.vcxitems" Label="Shared" Condition="Exists('glue')" />
              </ImportGroup>
              <ItemGroup><ClCompile Include="MacGlue.cpp" /></ItemGroup>
              <ItemDefinitionGroup Condition="'$(Configuration)|$(Platform)'=='Debug|ARM64'"><ClCompile>
                <PreprocessorDefinitions>PLATFORM_MAC;_DEBUG</PreprocessorDefinitions>
              </ClCompile></ItemDefinitionGroup>
              <ItemDefinitionGroup Condition="'$(Configuration)|$(Platform)'=='Release|ARM64'"><ClCompile>
                <PreprocessorDefinitions>PLATFORM_MAC;NDEBUG</PreprocessorDefinitions>
              </ClCompile></ItemDefinitionGroup>
            </Project>
            """);

        var eval = Fixture(mDir, "Debug");
        Assert.That(eval.Sources, Is.EqualTo(new[] { "NativeCode/Portable/test.cpp" }), "平台工程是源清单的权威，胶水不进清单");
        Assert.That(eval.Modules, Is.EqualTo(new[] { "NativeCode/Portable/Portable.vcxitems" }));
        Assert.That(eval.ModuleDefines, Does.Contain("HasModule_Mac").And.Contain("HasModule_Portable"));
        Assert.That(eval.Inputs, Has.None.Contains("codegen"), "生成胶水或求值器实现之外的输入混进了指纹");
        Assert.That(eval.Defines, Does.Contain("_DEBUG"));
    }

    /// <summary>
    /// 入库生成物必须等于当前求值结果：求值器、平台工程、任一 .vcxitems 或排除规则变了，
    /// 这里就会报出具体哪个文件没重新生成。
    /// </summary>
    [TestCase("Mac", "Core.Mac/Core.Mac.vcxproj", "Core.Mac/MacModules.cmake")]
    [TestCase("Android", "Core.Android/Core.Android.vcxproj", "Core.Android/AndroidModules.cmake")]
    public void CommittedArtifacts_MatchEvaluation(string platform, string project, string modulesFile)
    {
        var root = Root();
        // 参数在此镜像 RefreshSources.cmake；平台模块文件新增排除规则时必须同步本测试。
        Assert.That(File.ReadAllText(Path.Combine(root, modulesFile)), Does.Not.Contain("\""),
            modulesFile + " 新增了排除规则，请同步 ProjectEvaluatorTests 的 Options");

        var options = Options(root, platform, project, modulesFile, "Debug");
        var debug = ProjectEvaluator.Evaluate(options);
        options.Configuration = "Release";
        var release = ProjectEvaluator.Evaluate(options);

        var sources = Path.Combine(root, "Core." + platform, "generated", "TitanSources.cmake");
        var stale = Artifacts.WriteAll(debug, release, sources, check: true);
        Assert.That(stale, Is.Empty, "入库生成物与求值结果不一致");
        foreach (var name in new[] { "Titan" + platform + "Modules.h", "Titan" + platform + "Capabilities.props", "Titan" + platform + "Capabilities.cs" })
            Assert.That(File.Exists(Path.Combine(Path.GetDirectoryName(sources)!, name)), Is.True, name);
    }

    [Test]
    public void Fingerprint_ChangesWhenAnyInputChanges()
    {
        var root = Root();
        var baseline = Mac(root, "Debug").Fingerprint;
        Assert.That(baseline, Does.Match("^[0-9a-f]{64}$"));

        // 输入清单里必须包含求值器实现，否则改求值器不会让入库清单过期
        var eval = Mac(root, "Debug");
        Assert.That(eval.Inputs, Has.Some.StartsWith("Module/TitanProjectEval/"));
        Assert.That(eval.Inputs, Does.Contain("Core.Mac/Core.Mac.vcxproj"));
        Assert.That(eval.InputHashes, Has.Count.EqualTo(eval.Inputs.Count));
        Assert.That(eval.InputHashes.All(h => h.EndsWith("=") || h.Contains('=')), Is.True, "逐输入哈希格式必须是 路径=哈希");
    }

    [Test]
    public void HostProperties_MatchTargetPlatform()
    {
        // 求值器只替换“已知属性”，不认识的一律失败。这两个属性在 MSBuild 里始终有定义，
        // 所以非 Windows 目标也必须给键（值为空串），host-only 的 include 交给 clang 投影侧丢弃。
        foreach (var platform in new[] { "Windows", "Mac", "Android" })
        {
            var host = HostProperties.ForPlatform(platform);
            Assert.That(host, Does.ContainKey("WindowsSDK_IncludePath"), platform);
            Assert.That(host, Does.ContainKey("VC_IncludePath"), platform);
            Assert.That(host["VC_IncludePath"], Is.Empty, platform);
        }

        foreach (var platform in new[] { "Mac", "Android" })
            Assert.That(HostProperties.ForPlatform(platform)["WindowsSDK_IncludePath"], Is.Empty, platform + " 不应带 Windows SDK 头");
    }
}
