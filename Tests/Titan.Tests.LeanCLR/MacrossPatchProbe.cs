#nullable disable
using System.Security.Cryptography;
using System.Text;
using EngineNS.LeanCLR;
using EngineNS.Macross;
using Titan.HostGame;

// Stage A 只能在 runtime.Initialize 之前；Stage B 及调用在 AttachRuntime + RegisterAssembly(HostGame) 之后
internal sealed class MacrossPatchProbe
{
    public const string PatchAssembly = "Titan.Tests.LeanCLR.Patch";
    public const string MirrorAssembly = "Titan.Tests.LeanCLR.HostGame.LeanClrStub";
    private static readonly string[] PatchedMethods = { "Attack(System.Int32)", "Describe(System.String)", "Heal(System.Int32)", "Explode()", "Ratio(System.Single)", "Echo(Titan.HostGame.TtPatchableActor)", "Chain(System.Int32)", "GetElement(Titan.HostGame.EElement)", "ApplyDamage(Titan.HostGame.FDamage)" };
    private const string CheckClass = "Titan.HostGame.TtPatchCheckActor";
    private readonly string mRoot;
    private readonly string mPatchDll;
    private readonly string mMirrorDll;
    private TtMacrossPatchLoader mBadMvid, mBadId, mUnlisted, mBadSig, mBadRet, mOverload, mGood;

    public MacrossPatchProbe(string repoRoot, string configuration, string mirrorDll)
    {
        mRoot = Path.Combine(repoRoot, "cache", "macrosspatch_probe");
        mPatchDll = Path.Combine(repoRoot, "Tests", "Titan.Tests.LeanCLR.Patch", "bin", configuration, "netstandard2.0", PatchAssembly + ".dll");
        mMirrorDll = mirrorDll;
        if (!File.Exists(mPatchDll))
            throw new FileNotFoundException("Build Titan.Tests.LeanCLR.Patch before running this probe.", mPatchDll);
    }

    public static void RunPureTests()
    {
        Expect(TtMacrossPatchId.Compute("") == 14695981039346656037UL, "FNV-1a empty");
        Expect(TtMacrossPatchId.Compute("a") == 0x089BE207B544F1E4UL, "FNV-1a 'a' (UTF-16LE)");
        Expect(TtMacrossPatchId.Compute("Titan.HostGame.TtPatchableActor::Attack(System.Int32)") == 0x60B8EE50E749554AUL, "FNV-1a Attack");
        Expect(TtMacrossPatchMarshal.SlotBytes<int>() == 8 && TtMacrossPatchMarshal.SlotBytes<string>() == 8 &&
               TtMacrossPatchMarshal.SlotBytes<System.Numerics.Vector3>() == 16, "SlotBytes");
        unsafe
        {
            var slot = stackalloc byte[8];
            TtMacrossPatchMarshal.Write<short>(slot, -2);
            Expect(*(int*)slot == -2 && *(int*)(slot + 4) == 0, "sign-extend short");
            TtMacrossPatchMarshal.Write(slot, true);
            Expect(*(long*)slot == 1 && TtMacrossPatchMarshal.Read<bool>(slot), "bool slot");
            TtMacrossPatchMarshal.Write(slot, 1.5f);
            Expect(TtMacrossPatchMarshal.Read<float>(slot) == 1.5f, "float slot");
        }
        Expect(TtMacrossPatchLoader.CountParameters("A::B()") == 0 && TtMacrossPatchLoader.CountParameters("A::B(System.Int32)") == 1 &&
               TtMacrossPatchLoader.CountParameters("A::B(System.Collections.Generic.Dictionary<System.Int32,System.String>,System.Single)") == 2, "CountParameters");
        Console.WriteLine("Macross patch pure tests passed.");
    }

    public List<KeyValuePair<string, string>> VmAssemblies { get; } = new();

    public void Stage(Func<string, string, bool> register)
    {
        if (Directory.Exists(mRoot))
            Directory.Delete(mRoot, true);
        var gameMvid = typeof(TtPatchableActor).Assembly.ManifestModule.ModuleVersionId;
        var mirrorSha = Sha(mMirrorDll);

        Expect(new TtMacrossPatchLoader(Path.Combine(mRoot, "missing")).StageA(VmAssemblies, register) == TtMacrossPatchLoader.EStageResult.NoPatch, "missing dir -> NoPatch");
        ExpectRejected(MakePackage("badhash", gameMvid, mirrorSha, corruptHash: true), register, "bad file hash");
        ExpectRejected(MakePackage("badmirror", gameMvid, new string('0', 64)), register, "mirror hash mismatch");
        var denied = new TtMacrossPatchLoader(MakePackage("denied", gameMvid, mirrorSha), new DenyVerifier());
        Expect(denied.StageA(VmAssemblies, register) == TtMacrossPatchLoader.EStageResult.Rejected, "verifier denied");

        mBadMvid = Staged(MakePackage("badmvid", Guid.NewGuid(), mirrorSha), register);
        mBadId = Staged(MakePackage("badid", gameMvid, mirrorSha, corruptId: true), register);
        mUnlisted = Staged(MakePackage("unlisted", gameMvid, mirrorSha, signatures: ActorSignatures(PatchedMethods.Where(m => m != "Chain(System.Int32)"))), register);
        mBadSig = Staged(MakePackage("badsig", gameMvid, mirrorSha, signatures: ActorSignatures(PatchedMethods.Select(m => m == "Attack(System.Int32)" ? "Attack(System.Int64)" : m))), register);
        mBadRet = Staged(MakePackage("badret", gameMvid, mirrorSha, signatures: new[] { CheckClass + "::Value()" }), register);
        mOverload = Staged(MakePackage("overload", gameMvid, mirrorSha, signatures: new[] { CheckClass + "::Pick(System.Int32)", CheckClass + "::Pick(System.Single)" }), register);
        mGood = Staged(MakePackage("good", gameMvid, mirrorSha), register);
        Console.WriteLine("Macross patch Stage A probes passed.");
    }

    public void Run(TtLeanClrIntrinsicDispatcher dispatcher)
    {
        var vmThreadId = Environment.CurrentManagedThreadId;
        Func<bool> isVmThread = () => Environment.CurrentManagedThreadId == vmThreadId;
        var game = typeof(TtPatchableActor).Assembly;
        var actor = new TtPatchableActor();

        Expect(!mBadMvid.StageB(dispatcher, game, isVmThread) && !TtMacrossPatch.Active && mBadMvid.LastError.Contains("MVID"), "bad MVID rejected");
        Expect(!mBadId.StageB(dispatcher, game, isVmThread) && !TtMacrossPatch.Active && mBadId.LastError.Contains("MethodId"), "bad MethodId rejected");
        // 补丁类中 new 了 Chain 却未登记：宿主调用会走原逻辑、VM 内调用走新逻辑，必须整包拒绝
        ExpectRejectedB(mUnlisted, dispatcher, game, isVmThread, TtPatchableActorIds.Signature("Chain(System.Int32)") + " is declared in the patch but not listed");
        // 同名同参数个数但类型不同：仅按名字和个数匹配会绑错方法
        ExpectRejectedB(mBadSig, dispatcher, game, isVmThread, TtPatchableActorIds.Signature("Attack(System.Int64)") + " has no matching method in Titan.HostGame.TtPatchableActor__Patch");
        ExpectRejectedB(mBadRet, dispatcher, game, isVmThread, CheckClass + "::Value() returns System.String, host returns System.Int32");
        ExpectRejectedB(mOverload, dispatcher, game, isVmThread, CheckClass + "__Patch.Pick is overloaded");
        Expect(actor.Attack(3) == 3, "inactive -> original");

        Expect(mGood.StageB(dispatcher, game, isVmThread) && TtMacrossPatch.Active, "good package activated: " + mGood.LastError);
        Expect(actor.Attack(3) == 31, "Attack patched + Bonus original");
        Expect(actor.Untouched(3) == 4, "unpatched method -> original");
        Expect(actor.Describe("勇者") == "patched:勇者:100", "string arg/ret + field read");
        actor.Heal(5);
        Expect(actor.Hp == 110, "void + host field write");
        Expect(actor.Ratio(2f) == 0.5f, "float");
        var other = new TtPatchableActor();
        Expect(ReferenceEquals(actor.Echo(other), other) && ReferenceEquals(actor.Echo(null), actor), "host object arg/ret + null");
        Expect(actor.Chain(2) == 42, "nested VM call through host stub");
        // enum 参数/返回值
        Expect(actor.GetElement(EElement.Fire) == EElement.Ice, "enum patched: Fire->Ice");
        Expect(actor.GetElement(EElement.Ice) == EElement.Lightning, "enum patched: Ice->Lightning");
        // blittable struct 参数/返回值
        var dmg = new FDamage { Amount = 5, Element = 1 };
        var patched = actor.ApplyDamage(dmg);
        Expect(patched.Amount == 10 && patched.Element == 11, $"struct patched: Amount={patched.Amount}, Element={patched.Element}");
        // 已注册的派生宿主类型：VM 对象是派生类桩（继承基类桩而非 __Patch），调用继承来的被补丁方法
        var child = new TtPatchableActorChild();
        Expect(child.Attack(3) == 31 && child.Chain(2) == 42, "derived instance -> base patch");
        child.Heal(5);
        Expect(child.Hp == 110 && child.ChildBonus() == 5, "derived instance field write through base patch");

        int fallback = 0;
        var thread = new Thread(() => fallback = actor.Attack(3) + actor.Attack(3));
        thread.Start();
        thread.Join();
        Expect(fallback == 6 && TtMacrossPatch.Runtime.ThreadFallbackWarnings == 1, "non-VM thread -> original, warn once");
        Expect(actor.Attack(3) == 31, "VM thread still patched");

        try
        {
            actor.Explode();
            Expect(false, "Explode should throw");
        }
        catch (TtMacrossPatchException ex)
        {
            Expect(ex.MethodId == TtPatchableActorIds.Explode && ex.VmError.Contains("System.InvalidOperationException: patch boom"), "exception detail: " + ex.VmError);
        }
        Expect(TtMacrossPatch.Runtime.IsBroken(TtPatchableActorIds.Explode) && actor.Explode() == -1, "broken -> original");
        Console.WriteLine("Macross patch Stage B and invoke probes passed.");
    }

    private string MakePackage(string name, Guid gameMvid, string mirrorSha, bool corruptHash = false, bool corruptId = false, IEnumerable<string> signatures = null)
    {
        var dir = Path.Combine(mRoot, name);
        Directory.CreateDirectory(dir);
        File.Copy(mPatchDll, Path.Combine(dir, PatchAssembly + ".dll"));
        var methods = new StringBuilder();
        foreach (var sig in signatures ?? ActorSignatures(PatchedMethods))
        {
            var id = TtMacrossPatchId.Compute(sig) + (corruptId && methods.Length == 0 ? 1UL : 0UL);
            var sep = sig.IndexOf("::", StringComparison.Ordinal);
            if (methods.Length > 0)
                methods.Append(',');
            methods.Append($"{{\"MethodId\":\"0x{id:X16}\",\"Class\":\"{sig.Substring(0, sep)}\",\"Method\":\"{sig.Substring(sep + 2, sig.IndexOf('(') - sep - 2)}\",\"Signature\":\"{sig}\"}}");
        }
        var fileSha = corruptHash ? new string('0', 64) : Sha(Path.Combine(dir, PatchAssembly + ".dll"));
        var json = $"{{\"BaselineId\":{{\"GameMvid\":\"{gameMvid}\",\"MirrorAssembly\":\"{MirrorAssembly}\",\"MirrorSha256\":\"{mirrorSha}\"}}," +
                   $"\"PatchAssembly\":\"{PatchAssembly}\",\"Methods\":[{methods}],\"Files\":[{{\"Name\":\"{PatchAssembly}.dll\",\"Sha256\":\"{fileSha}\"}}]}}";
        File.WriteAllText(Path.Combine(dir, TtMacrossPatchLoader.ManifestFileName), json);
        return dir;
    }

    private TtMacrossPatchLoader Staged(string dir, Func<string, string, bool> register)
    {
        var loader = new TtMacrossPatchLoader(dir);
        Expect(loader.StageA(VmAssemblies, register) == TtMacrossPatchLoader.EStageResult.Staged, $"{dir} staged: {loader.LastError}");
        return loader;
    }

    private static IEnumerable<string> ActorSignatures(IEnumerable<string> methods) => methods.Select(TtPatchableActorIds.Signature);

    private static void ExpectRejectedB(TtMacrossPatchLoader loader, TtLeanClrIntrinsicDispatcher dispatcher, System.Reflection.Assembly game, Func<bool> isVmThread, string reason)
    {
        Expect(!loader.StageB(dispatcher, game, isVmThread) && !TtMacrossPatch.Active, "rejected: " + reason);
        Expect(loader.LastError != null && loader.LastError.Contains(reason), $"expected '{reason}', got '{loader.LastError}'");
    }

    private void ExpectRejected(string dir, Func<string, string, bool> register, string what)
    {
        var loader = new TtMacrossPatchLoader(dir);
        Expect(loader.StageA(VmAssemblies, register) == TtMacrossPatchLoader.EStageResult.Rejected, what);
    }

    private static string Sha(string path) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)));

    private static void Expect(bool condition, string what)
    {
        if (!condition)
            throw new InvalidOperationException("Macross patch probe failed: " + what);
    }

    private sealed class DenyVerifier : ITtMacrossPatchVerifier
    {
        public bool Verify(byte[] manifestBytes, byte[] signatureBytes) => false;
    }
}
