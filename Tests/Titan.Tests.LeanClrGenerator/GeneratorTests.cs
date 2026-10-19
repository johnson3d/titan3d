using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using EngineNS.LeanCLR.Generator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;

namespace Titan.Tests.LeanClrGenerator;

[TestFixture]
public class GeneratorTests
{
    private const string RegistryHint = "TtLeanClrRegistry.g.cs";
    private const string StubsHint = "TtLeanClrStubs.g.cs";

    // 引擎 API 的最小替身：生成的注册表只依赖这些名字；HostBinding 用 GCHandle 模拟 VM 对象指针
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
        // 与真实引擎一致：VNameString 位于全局命名空间
        public struct VNameString
        {
            public string Text;
            public static VNameString FromString(string text) => new VNameString { Text = text };
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
                public bool InvokeRaw(string signature, System.IntPtr args, System.IntPtr ret)
                {
                    var call = new FLeanClrIntrinsicCall { Args = (void*)args, Ret = (void*)ret };
                    return Handlers[signature](&call);
                }
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
                    => host == null ? null : (void*)Pin(host);
                public static System.IntPtr Pin(object host)
                    => System.Runtime.InteropServices.GCHandle.ToIntPtr(System.Runtime.InteropServices.GCHandle.Alloc(host));
                // 模拟 VM 字符串布局：16 字节对象头、偏移 16 为长度、偏移 20 起为 UTF-16 字符
                public static string ReadString(void* vmString)
                    => vmString == null ? null : new string((char*)((byte*)vmString + 20), 0, *(int*)((byte*)vmString + 16));
                public static void* NewVmString(string value)
                {
                    if (value == null)
                        return null;
                    var p = (byte*)System.Runtime.InteropServices.Marshal.AllocHGlobal(20 + value.Length * 2 + 2);
                    *(int*)(p + 16) = value.Length;
                    fixed (char* chars = value)
                        System.Buffer.MemoryCopy(chars, p + 20, value.Length * 2, value.Length * 2);
                    *(char*)(p + 20 + value.Length * 2) = '\0';
                    return p;
                }
            }
        }
        """;

    private const string ActorSource = """
        namespace Game
        {
            [EngineNS.LeanCLR.TtLeanClr]
            public class Actor
            {
                public int Hp = 100;
                [EngineNS.Rtti.Meta] public int Damage(int amount) { Hp -= amount; return Hp; }
                [EngineNS.Rtti.Meta] public static int Twice(int value) => value * 2;
                [EngineNS.Rtti.Meta] public bool IsAlive() => Hp > 0;
                [EngineNS.Rtti.Meta] public float Scale(float value) => value * 1.5f;
                [EngineNS.Rtti.Meta] public Actor Self() => this;
                [EngineNS.Rtti.Meta] private long Secret(long value) => value + Hp;
                [EngineNS.Rtti.Meta] public void Heal(short value, bool full) { Hp = full ? 100 : Hp + value; }
                [EngineNS.Rtti.Meta] public double Mix(byte left, double right) => left + right;
                [EngineNS.Rtti.Meta(Flags = EngineNS.Rtti.MetaAttribute.EMetaFlags.NoMacrossUseable)] public int Hidden() => 7;
                [EngineNS.Rtti.Meta(Flags = EngineNS.Rtti.MetaAttribute.EMetaFlags.NoMacrossCreate)] public int Visible() => 8;
                public int NoMeta() => 9;
                [EngineNS.Rtti.Meta] public int Count(int[] values) => values.Length;
                [EngineNS.Rtti.Meta] public string Greet(string name) => name == null ? null : "Hi " + name + " " + Hp;
                [EngineNS.Rtti.Meta] public static int Length(string text) => text == null ? -1 : text.Length;
                [EngineNS.Rtti.Meta] public static string Repeat(string text, int count) => string.Concat(System.Linq.Enumerable.Repeat(text, count));
            }

            [EngineNS.LeanCLR.TtLeanClr]
            public class Weapon
            {
                [EngineNS.Rtti.Meta] public int Attack(Actor target, int amount) => target.Damage(amount);
            }

            [EngineNS.LeanCLR.TtLeanClr]
            public class Hero : Actor
            {
                [EngineNS.Rtti.Meta] public int Level() => 3;
            }

            public class Plain { }

            [EngineNS.LeanCLR.TtLeanClr]
            public class FromPlain : Plain { }

            public class Outer
            {
                [EngineNS.LeanCLR.TtLeanClr]
                public class Inner
                {
                    [EngineNS.Rtti.Meta] public int Value() => 1;
                }
            }
        }
        """;

    private sealed record GeneratorRun(
        CSharpCompilation Input,
        Compilation Output,
        IReadOnlyDictionary<string, string> Sources,
        IReadOnlyList<Diagnostic> Diagnostics);

    private static GeneratorRun Run(string source, string assemblyName = "Game.Plugin")
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
            new[] { new TtLeanClrGenerator().AsSourceGenerator() }, parseOptions: parseOptions);
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

    private static string FindRepoRoot()
    {
        for (var dir = new DirectoryInfo(TestContext.CurrentContext.TestDirectory); dir != null; dir = dir.Parent)
        {
            if (File.Exists(Path.Combine(dir.FullName, "EngineAll.sln")))
                return dir.FullName;
        }
        throw new DirectoryNotFoundException("EngineAll.sln not found.");
    }

    private static string[] Errors(Compilation compilation)
    {
        return compilation.GetDiagnostics()
            .Where(d => d.Severity == DiagnosticSeverity.Error)
            .Select(d => d.ToString())
            .ToArray();
    }

    [Test]
    public void PublicMethodsProduceInternalNameSignatures()
    {
        var run = Run(ActorSource);
        var registry = run.Sources[RegistryHint];

        Assert.That(registry, Does.Contain("\"Game.Actor::Damage(System.Int32)\""));
        Assert.That(registry, Does.Contain("\"Game.Actor::Twice(System.Int32)\""));
        Assert.That(registry, Does.Contain("\"Game.Actor::Heal(System.Int16,System.Boolean)\""));
        Assert.That(registry, Does.Contain("\"Game.Actor::IsAlive()\""));
        Assert.That(registry, Does.Contain("\"Game.Actor::Visible()\""));
        Assert.That(registry, Does.Contain("\"Game.Plugin.LeanClrStub\""));
    }

    [Test]
    public void NoMacrossUseableMethodIsNotExposed()
    {
        var run = Run(ActorSource);
        Assert.That(run.Sources[RegistryHint], Does.Not.Contain("Hidden"));
        Assert.That(run.Sources[StubsHint], Does.Not.Contain("Hidden"));
    }

    [Test]
    public void MethodWithoutMetaIsNotExposed()
    {
        var run = Run(ActorSource);
        Assert.That(run.Sources[RegistryHint], Does.Not.Contain("NoMeta"));
        Assert.That(run.Sources[StubsHint], Does.Not.Contain("NoMeta"));
    }

    [Test]
    public void UnsupportedParameterTypeWarnsAndSkipsMethod()
    {
        var run = Run(ActorSource);
        var diagnostic = run.Diagnostics.SingleOrDefault(d => d.Id == "TTLC001");
        Assert.That(diagnostic, Is.Not.Null);
        Assert.That(diagnostic!.Severity, Is.EqualTo(DiagnosticSeverity.Warning));
        Assert.That(diagnostic.GetMessage(), Does.Contain("Count"));
        Assert.That(run.Sources[RegistryHint], Does.Not.Contain("::Count("));
        Assert.That(run.Sources[StubsHint], Does.Not.Contain(" Count("));
    }

    [Test]
    public void StringParametersAndReturnsAreExposed()
    {
        var run = Run(ActorSource);
        var registry = run.Sources[RegistryHint];
        var stubs = run.Sources[StubsHint];

        Assert.That(registry, Does.Contain("\"Game.Actor::Greet(System.String)\""));
        Assert.That(registry, Does.Contain("\"Game.Actor::Length(System.String)\""));
        Assert.That(registry, Does.Contain("\"Game.Actor::Repeat(System.String,System.Int32)\""));
        Assert.That(stubs, Does.Contain("public string Greet(string name)"));
        Assert.That(stubs, Does.Contain("public static string Repeat(string text, int count)"));
        Assert.That(run.Diagnostics.Where(d => d.Id == "TTLC001").Select(d => d.GetMessage()),
            Has.None.Contains("Greet").And.None.Contains("Length").And.None.Contains("Repeat"));
    }

    [Test]
    public void NestedClassWarnsAndIsSkipped()
    {
        var run = Run(ActorSource);
        var diagnostic = run.Diagnostics.SingleOrDefault(d => d.Id == "TTLC002");
        Assert.That(diagnostic, Is.Not.Null);
        Assert.That(diagnostic!.Severity, Is.EqualTo(DiagnosticSeverity.Warning));
        Assert.That(run.Sources[RegistryHint], Does.Not.Contain("Inner"));
        Assert.That(run.Sources[StubsHint], Does.Not.Contain("Inner"));
    }

    [Test]
    public void StubSourceCompilesAgainstEngineStubOnly()
    {
        var run = Run(ActorSource);
        var stubOptions = CSharpParseOptions.Default
            .WithLanguageVersion(LanguageVersion.Latest)
            .WithPreprocessorSymbols("TITAN_LEANCLR_STUB");
        var engineStub = File.ReadAllText(Path.Combine(FindRepoRoot(), "Module", "LeanClrEngineStub", "TtLeanClrHostObject.cs"));
        var stubCompilation = CSharpCompilation.Create(
            "Game.Plugin.LeanClrStub",
            new[]
            {
                CSharpSyntaxTree.ParseText(engineStub, stubOptions),
                CSharpSyntaxTree.ParseText(run.Sources[StubsHint], stubOptions),
                // 注册表在桩工程中必须整体失效
                CSharpSyntaxTree.ParseText(run.Sources[RegistryHint], stubOptions),
            },
            PlatformReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        Assert.That(Errors(stubCompilation), Is.Empty);
        var actor = stubCompilation.GetTypeByMetadataName("Game.Actor");
        Assert.That(actor, Is.Not.Null);
        Assert.That(actor!.GetMembers("Damage").OfType<IMethodSymbol>().Single().DeclaredAccessibility, Is.EqualTo(Accessibility.Public));
        Assert.That(actor.GetMembers("Secret"), Has.Length.EqualTo(1));
        Assert.That(actor.InstanceConstructors.Single().DeclaredAccessibility, Is.EqualTo(Accessibility.Protected));
    }

    [Test]
    public void StubBaseFollowsAnnotatedBaseClass()
    {
        var stubs = Run(ActorSource).Sources[StubsHint];
        Assert.That(stubs, Does.Contain("class Hero : global::Game.Actor"));
        Assert.That(stubs, Does.Contain("class Actor : global::EngineNS.LeanCLR.TtLeanClrHostObject"));
        Assert.That(stubs, Does.Contain("class FromPlain : global::EngineNS.LeanCLR.TtLeanClrHostObject"));
    }

    [Test]
    public void AnnotatedClassParameterUsesFullName()
    {
        var registry = Run(ActorSource).Sources[RegistryHint];
        Assert.That(registry, Does.Contain("\"Game.Weapon::Attack(Game.Actor,System.Int32)\""));
        Assert.That(registry, Does.Contain("\"Game.Actor::Self()\""));
    }

    [Test]
    public void SourceWithoutAnnotatedClassesStillEmitsEmptyStubs()
    {
        var run = Run("namespace Game { public class Nothing { } }");
        Assert.That(run.Sources.ContainsKey(StubsHint), Is.True);
        Assert.That(run.Sources.ContainsKey(RegistryHint), Is.False);
    }

    [Test]
    public void HostCompilationWithRegistryHasNoErrors()
    {
        var run = Run(ActorSource);
        Assert.That(Errors(run.Output), Is.Empty);
    }

    // ---- ABI 扩展: enum ----

    private const string EnumSource = """
        namespace Game
        {
            public enum EColor : byte { Red, Green, Blue }
            public enum EDirection { North, South, East, West }

            [EngineNS.LeanCLR.TtLeanClr]
            public class Painter
            {
                [EngineNS.Rtti.Meta] public EColor GetColor() => EColor.Red;
                [EngineNS.Rtti.Meta] public int UseColor(EColor c) => (int)c;
                [EngineNS.Rtti.Meta] public EDirection Facing(EDirection d) => d;
            }
        }
        """;

    [Test]
    public void EnumParameterAndReturnAreExposed()
    {
        var run = Run(EnumSource);
        var registry = run.Sources[RegistryHint];
        Assert.That(registry, Does.Contain("\"Game.Painter::GetColor()\""));
        Assert.That(registry, Does.Contain("\"Game.Painter::UseColor(Game.EColor)\""));
        Assert.That(registry, Does.Contain("\"Game.Painter::Facing(Game.EDirection)\""));
        Assert.That(Errors(run.Output), Is.Empty);
    }

    [Test]
    public unsafe void EnumProxiesFollowIntrinsicCallAbi()
    {
        var run = Run(EnumSource);
        Assert.That(Errors(run.Output), Is.Empty);
        using var stream = new MemoryStream();
        var emit = run.Output.Emit(stream);
        Assert.That(emit.Success, Is.True, string.Join(Environment.NewLine, emit.Diagnostics));
        stream.Position = 0;
        var context = new AssemblyLoadContext("EnumProbe", isCollectible: true);
        try
        {
            var assembly = context.LoadFromStream(stream);
            var registryAttribute = assembly.GetCustomAttributesData()
                .Single(a => a.AttributeType.Name == "TtLeanClrRegistryAttribute");
            var registryType = (Type)registryAttribute.ConstructorArguments[0].Value!;
            var registry = Activator.CreateInstance(registryType)!;
            var dispatcherType = assembly.GetType("EngineNS.LeanCLR.TtLeanClrIntrinsicDispatcher")!;
            var dispatcher = Activator.CreateInstance(dispatcherType)!;
            registryType.GetMethod("Register")!.Invoke(registry, new[] { dispatcher });
            var invokeRaw = dispatcherType.GetMethod("InvokeRaw")!;
            var pin = assembly.GetType("EngineNS.LeanCLR.TtLeanClrHostBinding")!.GetMethod("Pin")!;
            var painterType = assembly.GetType("Game.Painter")!;
            var painter = Activator.CreateInstance(painterType)!;
            var painterPtr = (IntPtr)pin.Invoke(null, new[] { painter })!;

            var buffer = stackalloc long[4];
            bool Call(string sig) => (bool)invokeRaw.Invoke(dispatcher, new object[] { sig, (IntPtr)buffer, (IntPtr)buffer })!;

            // GetColor() -> EColor.Red == 0, 写满 int32
            buffer[0] = (long)painterPtr;
            Assert.That(Call("Game.Painter::GetColor()"), Is.True);
            Assert.That(*(int*)buffer, Is.EqualTo(0));

            // UseColor(EColor.Blue==2) -> 2
            buffer[0] = (long)painterPtr; *(byte*)(buffer + 1) = 2;
            Assert.That(Call("Game.Painter::UseColor(Game.EColor)"), Is.True);
            Assert.That(*(int*)buffer, Is.EqualTo(2));

            // Facing(EDirection.East==2) -> East==2, 作为 int32 enum
            buffer[0] = (long)painterPtr; *(int*)(buffer + 1) = 2;
            Assert.That(Call("Game.Painter::Facing(Game.EDirection)"), Is.True);
            Assert.That(*(int*)buffer, Is.EqualTo(2));
        }
        finally
        {
            context.Unload();
        }
    }

    // ---- ABI 扩展: blittable struct ----

    private const string StructSource = """
        namespace Game
        {
            [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
            public struct Vec3 { public float X, Y, Z; }

            [EngineNS.LeanCLR.TtLeanClr]
            public class Mover
            {
                [EngineNS.Rtti.Meta] public Vec3 GetPosition() => new Vec3 { X = 1, Y = 2, Z = 3 };
                [EngineNS.Rtti.Meta] public float Distance(Vec3 a) => a.X + a.Y + a.Z;
            }
        }
        """;

    [Test]
    public void BlittableStructParameterAndReturnAreExposed()
    {
        var run = Run(StructSource);
        var registry = run.Sources[RegistryHint];
        Assert.That(registry, Does.Contain("\"Game.Mover::GetPosition()\""));
        Assert.That(registry, Does.Contain("\"Game.Mover::Distance(Game.Vec3)\""));
        Assert.That(Errors(run.Output), Is.Empty);
    }

    [Test]
    public void NonBlittableStructSkippedWithWarning()
    {
        var source = """
            namespace Game
            {
                public struct BadStruct { public string Name; }
                [EngineNS.LeanCLR.TtLeanClr]
                public class BadUser
                {
                    [EngineNS.Rtti.Meta] public BadStruct Get() => default;
                }
            }
            """;
        var run = Run(source);
        Assert.That(run.Diagnostics.Any(d => d.Id == "TTLC001" && d.GetMessage().Contains("BadStruct")));
        // BadStruct 方法被跳过，不出现在注册表中
        if (run.Sources.ContainsKey(RegistryHint))
            Assert.That(run.Sources[RegistryHint], Does.Not.Contain("BadStruct"));
    }

    [Test]
    public unsafe void ProxiesFollowIntrinsicCallAbi()
    {
        var run = Run(ActorSource);
        Assert.That(Errors(run.Output), Is.Empty);

        using var stream = new MemoryStream();
        var emit = run.Output.Emit(stream);
        Assert.That(emit.Success, Is.True, string.Join(Environment.NewLine, emit.Diagnostics));
        stream.Position = 0;
        var context = new AssemblyLoadContext("LeanClrGeneratorProbe", isCollectible: true);
        try
        {
            var assembly = context.LoadFromStream(stream);
            var registryAttribute = assembly.GetCustomAttributesData()
                .Single(a => a.AttributeType.Name == "TtLeanClrRegistryAttribute");
            var registryType = (Type)registryAttribute.ConstructorArguments[0].Value!;
            var registry = Activator.CreateInstance(registryType)!;
            var dispatcherType = assembly.GetType("EngineNS.LeanCLR.TtLeanClrIntrinsicDispatcher")!;
            var dispatcher = Activator.CreateInstance(dispatcherType)!;
            registryType.GetMethod("Register")!.Invoke(registry, new[] { dispatcher });
            var invokeRaw = dispatcherType.GetMethod("InvokeRaw")!;
            var pin = assembly.GetType("EngineNS.LeanCLR.TtLeanClrHostBinding")!.GetMethod("Pin")!;
            var actorType = assembly.GetType("Game.Actor")!;
            var hpField = actorType.GetField("Hp")!;
            var actor = Activator.CreateInstance(actorType)!;
            var actorPtr = (IntPtr)pin.Invoke(null, new[] { actor })!;

            // args 与 ret 指向同一块内存：代理必须先读完参数再写返回值
            var buffer = stackalloc long[4];
            bool Call(string signature) => (bool)invokeRaw.Invoke(dispatcher, new object[] { signature, (IntPtr)buffer, (IntPtr)buffer })!;

            buffer[0] = (long)actorPtr; buffer[1] = 30;
            Assert.That(Call("Game.Actor::Damage(System.Int32)"), Is.True);
            Assert.That(*(int*)buffer, Is.EqualTo(70));
            Assert.That(hpField.GetValue(actor), Is.EqualTo(70));

            buffer[0] = 21;
            Assert.That(Call("Game.Actor::Twice(System.Int32)"), Is.True);
            Assert.That(*(int*)buffer, Is.EqualTo(42));

            // bool 返回值必须写满 int32，不能留下 this 指针的高位字节
            buffer[0] = (long)actorPtr;
            Assert.That(Call("Game.Actor::IsAlive()"), Is.True);
            Assert.That(*(int*)buffer, Is.EqualTo(1));

            buffer[0] = (long)actorPtr; *(float*)(buffer + 1) = 2f;
            Assert.That(Call("Game.Actor::Scale(System.Single)"), Is.True);
            Assert.That(*(float*)buffer, Is.EqualTo(3f));

            buffer[0] = (long)actorPtr;
            Assert.That(Call("Game.Actor::Self()"), Is.True);
            Assert.That(GCHandle.FromIntPtr((IntPtr)buffer[0]).Target, Is.SameAs(actor));

            buffer[0] = (long)actorPtr; buffer[1] = 5;
            Assert.That(Call("Game.Actor::Secret(System.Int64)"), Is.True);
            Assert.That(buffer[0], Is.EqualTo(75L));

            buffer[0] = (long)actorPtr; buffer[1] = 0; *(short*)(buffer + 1) = -20; buffer[2] = 0;
            Assert.That(Call("Game.Actor::Heal(System.Int16,System.Boolean)"), Is.True);
            Assert.That(hpField.GetValue(actor), Is.EqualTo(50));

            buffer[0] = (long)actorPtr; buffer[1] = 4; *(double*)(buffer + 2) = 0.5;
            Assert.That(Call("Game.Actor::Mix(System.Byte,System.Double)"), Is.True);
            Assert.That(*(double*)buffer, Is.EqualTo(4.5));

            var weaponType = assembly.GetType("Game.Weapon")!;
            var weaponPtr = (IntPtr)pin.Invoke(null, new[] { Activator.CreateInstance(weaponType)! })!;
            buffer[0] = (long)weaponPtr; buffer[1] = (long)actorPtr; buffer[2] = 10;
            Assert.That(Call("Game.Weapon::Attack(Game.Actor,System.Int32)"), Is.True);
            Assert.That(*(int*)buffer, Is.EqualTo(40));

            var bindingType = assembly.GetType("EngineNS.LeanCLR.TtLeanClrHostBinding")!;
            var newVmString = bindingType.GetMethod("NewVmString")!;
            var readString = bindingType.GetMethod("ReadString")!;
            IntPtr VmString(string value) => (IntPtr)Pointer.Unbox(newVmString.Invoke(null, new object[] { value })!);
            string HostString(long vmString) => (string)readString.Invoke(null, new object[] { Pointer.Box((void*)vmString, typeof(void*)) })!;

            // string 参数与返回值走 VM 对象指针槽，含中文字符
            buffer[0] = (long)actorPtr; buffer[1] = (long)VmString("勇者");
            Assert.That(Call("Game.Actor::Greet(System.String)"), Is.True);
            Assert.That(HostString(buffer[0]), Is.EqualTo("Hi 勇者 40"));

            // null 双向透传
            buffer[0] = (long)actorPtr; buffer[1] = 0;
            Assert.That(Call("Game.Actor::Greet(System.String)"), Is.True);
            Assert.That(buffer[0], Is.EqualTo(0L));

            buffer[0] = (long)VmString("");
            Assert.That(Call("Game.Actor::Length(System.String)"), Is.True);
            Assert.That(*(int*)buffer, Is.EqualTo(0));

            buffer[0] = 0;
            Assert.That(Call("Game.Actor::Length(System.String)"), Is.True);
            Assert.That(*(int*)buffer, Is.EqualTo(-1));

            buffer[0] = (long)VmString("ab"); buffer[1] = 3;
            Assert.That(Call("Game.Actor::Repeat(System.String,System.Int32)"), Is.True);
            Assert.That(HostString(buffer[0]), Is.EqualTo("ababab"));

            // this 无法解析时代理返回 false，由 dispatcher 标记调用失败
            buffer[0] = 0;
            Assert.That(Call("Game.Actor::IsAlive()"), Is.False);
        }
        finally
        {
            context.Unload();
        }
    }
}
