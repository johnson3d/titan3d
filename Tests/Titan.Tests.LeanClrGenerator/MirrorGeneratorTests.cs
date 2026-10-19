using System.Collections.Immutable;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using System.Text.Json;
using EngineNS.LeanCLR.Generator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;

namespace Titan.Tests.LeanClrGenerator;

[TestFixture]
public class MirrorGeneratorTests
{
    private const string RegistryHint = "TtLeanClrRegistry_GameProject.g.cs";
    private const string StubsHint = "TtLeanClrStubs_GameProject.g.cs";
    private const string BaselineHint = "TtMacrossPatchBaseline_GameProject.g.cs";

    private const string EngineSurface = """
        namespace EngineNS.Rtti
        {
            [System.AttributeUsage(System.AttributeTargets.All)]
            public class MetaAttribute : System.Attribute
            {
                [System.Flags]
                public enum EMetaFlags : uint { NoMacrossUseable = 1, NoMacrossCreate = 2 }
                public EMetaFlags Flags = 0;
            }
        }
        public struct VNameString
        {
            public string Text;
            public static VNameString FromString(string text) => new VNameString { Text = text };
        }
        namespace EngineNS.Macross
        {
            [System.AttributeUsage(System.AttributeTargets.Class, Inherited = false)]
            public sealed class TtMacrossAttribute : System.Attribute { }
        }
        namespace EngineNS.LeanCLR
        {
            [System.AttributeUsage(System.AttributeTargets.Class, Inherited = false)]
            public sealed class TtLeanClrAttribute : System.Attribute { }
            [System.AttributeUsage(System.AttributeTargets.Assembly)]
            public sealed class TtLeanClrRegistryAttribute : System.Attribute
            {
                public TtLeanClrRegistryAttribute(System.Type registryType) { RegistryType = registryType; }
                public System.Type RegistryType { get; }
            }
            public unsafe struct FLeanClrIntrinsicCall
            {
                public void* Args;
                public void* Ret;
                public void* GetArgumentBuffer() => Args;
                public void* GetReturnBuffer() => Ret;
            }
            public unsafe class TtLeanClrIntrinsicDispatcher
            {
                public delegate bool FIntrinsicHandler(FLeanClrIntrinsicCall* call);
                public readonly System.Collections.Generic.Dictionary<string, FIntrinsicHandler> Handlers = new();
                public void Register(global::VNameString signature, FIntrinsicHandler handler) => Handlers[signature.Text] = handler;
                public bool Unregister(global::VNameString signature) => Handlers.Remove(signature.Text);
            }
            public interface ITtLeanClrRegistry
            {
                string StubAssemblyName { get; }
                void Register(TtLeanClrIntrinsicDispatcher dispatcher);
                void Unregister(TtLeanClrIntrinsicDispatcher dispatcher);
                void CollectHostTypes(System.Collections.Generic.List<System.Collections.Generic.KeyValuePair<System.Type, string>> hostTypes);
            }
            public static unsafe class TtLeanClrHostBinding
            {
                public static T ResolveHost<T>(void* vmObject) where T : class
                    => vmObject == null ? null : System.Runtime.InteropServices.GCHandle.FromIntPtr((System.IntPtr)vmObject).Target as T;
                public static void* GetVmObjectPointer(object host)
                    => host == null ? null : (void*)System.Runtime.InteropServices.GCHandle.ToIntPtr(System.Runtime.InteropServices.GCHandle.Alloc(host));
                public static string ReadString(void* vmString) => null;
                public static void* NewVmString(string value) => null;
            }
        }
        """;

    private const string MacrossSource = """
        namespace Game
        {
            [EngineNS.Macross.TtMacross]
            [EngineNS.LeanCLR.TtLeanClr]
            public class PlayerActor
            {
                public int Hp = 100;
                [EngineNS.Rtti.Meta] public int Attack(int damage) => damage * 2;
                [EngineNS.Rtti.Meta] public void Heal(int amount) { Hp += amount; }
                [EngineNS.Rtti.Meta] public bool IsAlive() => Hp > 0;
            }

            [EngineNS.Macross.TtMacross]
            [EngineNS.LeanCLR.TtLeanClr]
            public class EnemyActor : PlayerActor
            {
                [EngineNS.Rtti.Meta] public int Level() => 5;
            }
        }
        """;

    private sealed record GeneratorRun(
        CSharpCompilation Input,
        Compilation Output,
        IReadOnlyDictionary<string, string> Sources,
        IReadOnlyList<Diagnostic> Diagnostics);

    private static GeneratorRun Run(string source, string assemblyName = "GameProject")
    {
        var parseOptions = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Latest);
        var compilation = CSharpCompilation.Create(
            assemblyName,
            new[]
            {
                CSharpSyntaxTree.ParseText(EngineSurface, parseOptions),
                CSharpSyntaxTree.ParseText(source, parseOptions),
            },
            PlatformReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true, nullableContextOptions: NullableContextOptions.Disable));

        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            new[] { new TtMacrossMirrorGenerator().AsSourceGenerator() }, parseOptions: parseOptions);
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var output, out var generatorDiagnostics);
        var result = driver.GetRunResult();
        var sources = result.Results.SelectMany(r => r.GeneratedSources)
            .ToDictionary(s => s.HintName, s => s.SourceText.ToString());
        return new GeneratorRun(compilation, output, sources, generatorDiagnostics);
    }

    private static IEnumerable<MetadataReference> PlatformReferences()
    {
        var tpa = (string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!;
        return tpa.Split(Path.PathSeparator)
            .Where(p => Path.GetFileName(p).StartsWith("System.", StringComparison.Ordinal) ||
                        Path.GetFileName(p) is "netstandard.dll" or "mscorlib.dll")
            .Select(p => MetadataReference.CreateFromFile(p));
    }

    private static string[] Errors(Compilation compilation)
    {
        return compilation.GetDiagnostics()
            .Where(d => d.Severity == DiagnosticSeverity.Error)
            .Select(d => d.ToString())
            .ToArray();
    }

    private static string BaselineJson(Compilation compilation, string assemblyName = "GameProject")
    {
        var type = compilation.GetTypeByMetadataName(
            $"EngineNS.LeanCLR.Generated.TtMacrossPatchBaseline_{assemblyName}");
        Assert.That(type, Is.Not.Null, "Baseline artifact type not generated");
        var field = type!.GetMembers("DraftJson").OfType<IFieldSymbol>().Single();
        return (string)field.ConstantValue!;
    }

    [Test]
    public void MirrorGeneratorEmitsRegistryForMacrossClasses()
    {
        var run = Run(MacrossSource);
        Assert.That(run.Sources.ContainsKey(RegistryHint), Is.True, "Registry not generated");
        var registry = run.Sources[RegistryHint];
        Assert.That(registry, Does.Contain("\"Game.PlayerActor::Attack(System.Int32)\""));
        Assert.That(registry, Does.Contain("\"Game.PlayerActor::Heal(System.Int32)\""));
        Assert.That(registry, Does.Contain("\"Game.PlayerActor::IsAlive()\""));
        Assert.That(registry, Does.Contain("\"Game.EnemyActor::Level()\""));
        Assert.That(Errors(run.Output), Is.Empty);
    }

    [Test]
    public void MirrorGeneratorEmitsStubsAndBaselineDraft()
    {
        var run = Run(MacrossSource);
        Assert.That(run.Sources.ContainsKey(StubsHint), Is.True, "Stubs not generated");
        Assert.That(run.Sources.ContainsKey(BaselineHint), Is.True, "Baseline draft not generated");
        var stubs = run.Sources[StubsHint];
        Assert.That(stubs, Does.Contain("class PlayerActor"));
        Assert.That(stubs, Does.Contain("class EnemyActor"));
        var draft = BaselineJson(run.Output);
        using var document = JsonDocument.Parse(draft);
        var methods = document.RootElement.EnumerateArray().ToArray();
        Assert.That(methods.Any(m => m.GetProperty("Signature").GetString() ==
            "Game.PlayerActor::Attack(System.Int32)"), Is.True);
        Assert.That(methods.All(m => m.GetProperty("MethodId").GetString()!.StartsWith("0x")), Is.True);
    }

    [Test]
    public void MirrorGeneratorUsesAssemblyNameInHintNames()
    {
        var run = Run(MacrossSource, "MyGame");
        Assert.That(run.Sources.ContainsKey("TtLeanClrRegistry_MyGame.g.cs"), Is.True);
        Assert.That(run.Sources.ContainsKey("TtLeanClrStubs_MyGame.g.cs"), Is.True);
        Assert.That(run.Sources.ContainsKey("TtMacrossPatchBaseline_MyGame.g.cs"), Is.True);
        Assert.That(BaselineJson(run.Output, "MyGame"), Does.Contain("Game.PlayerActor::Attack(System.Int32)"));
    }

    [Test]
    public void DuplicateMethodIdsProduceDiagnostic()
    {
        var source = """
            namespace Game
            {
                [EngineNS.Macross.TtMacross]
                [EngineNS.LeanCLR.TtLeanClr]
                public class PlayerActor
                {
                    [EngineNS.Rtti.Meta] public int Attack(int damage) => damage;
                    [EngineNS.Rtti.Meta] public int Attack(int damage) => damage * 2;
                }
            }
            """;
        var run = Run(source);
        Assert.That(run.Diagnostics.Any(d => d.Id == "TTMC001"), Is.True);
    }

    [Test]
    public void NoMacrossClassesProducesNoOutput()
    {
        var source = """
            namespace Game
            {
                [EngineNS.LeanCLR.TtLeanClr]
                public class PlainActor
                {
                    [EngineNS.Rtti.Meta] public int Value() => 1;
                }
            }
            """;
        var run = Run(source);
        // PlainActor has [TtLeanClr] but not [TtMacross] — MirrorGenerator should skip it
        Assert.That(run.Sources.ContainsKey(RegistryHint), Is.False);
    }
}
