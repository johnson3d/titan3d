using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;
using EngineNS.LeanCLR;

namespace EngineNS.Macross
{
    public sealed class TtMacrossPatchManifest
    {
        public sealed class FMethod
        {
            public ulong MethodId;
            public string Class;
            public string Method;
            public string Signature;
        }
        public sealed class FFile
        {
            public string Name;
            public string Sha256;
        }

        public Guid GameMvid;
        public string MirrorAssembly;
        public string MirrorSha256;
        public string PatchAssembly;
        public readonly List<FMethod> Methods = new();
        public readonly List<FFile> Files = new();

        public static TtMacrossPatchManifest Parse(byte[] json)
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            var baseline = root.GetProperty("BaselineId");
            var result = new TtMacrossPatchManifest
            {
                GameMvid = Guid.Parse(baseline.GetProperty("GameMvid").GetString()),
                MirrorAssembly = RequireString(baseline, "MirrorAssembly"),
                MirrorSha256 = RequireString(baseline, "MirrorSha256"),
                PatchAssembly = RequireString(root, "PatchAssembly"),
            };
            foreach (var m in root.GetProperty("Methods").EnumerateArray())
            {
                var id = RequireString(m, "MethodId");
                if (!id.StartsWith("0x", StringComparison.OrdinalIgnoreCase) ||
                    !ulong.TryParse(id.AsSpan(2), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out var methodId))
                    throw new FormatException($"Invalid MethodId: {id}");
                result.Methods.Add(new FMethod
                {
                    MethodId = methodId,
                    Class = RequireString(m, "Class"),
                    Method = RequireString(m, "Method"),
                    Signature = RequireString(m, "Signature"),
                });
            }
            foreach (var f in root.GetProperty("Files").EnumerateArray())
                result.Files.Add(new FFile { Name = RequireString(f, "Name"), Sha256 = RequireString(f, "Sha256") });
            return result;
        }

        private static string RequireString(JsonElement element, string name)
        {
            var value = element.GetProperty(name).GetString();
            if (string.IsNullOrEmpty(value))
                throw new FormatException($"patch.json field {name} is empty.");
            return value;
        }
    }

    /// <summary>
    /// 补丁包签名校验扩展点；signatureBytes 为 patch.sig 内容，不存在时为 null。
    /// </summary>
    public interface ITtMacrossPatchVerifier
    {
        bool Verify(byte[] manifestBytes, byte[] signatureBytes);
    }

    public sealed class TtMacrossPatchAcceptAllVerifier : ITtMacrossPatchVerifier
    {
        public static readonly TtMacrossPatchAcceptAllVerifier Instance = new();
        public bool Verify(byte[] manifestBytes, byte[] signatureBytes) => true;
    }

    /// <summary>
    /// 补丁包加载：Stage A 在 VM Initialize 前校验并注册补丁程序集；
    /// Stage B 在 GameProject 加载后校验 MVID、解析补丁方法并激活。任一步失败整包拒绝。
    /// </summary>
    public sealed class TtMacrossPatchLoader
    {
        public enum EStageResult
        {
            NoPatch,
            Staged,
            Rejected,
        }

        public const string ManifestFileName = "patch.json";
        public const string SignatureFileName = "patch.sig";
        public const string PatchClassSuffix = "__Patch";

        private readonly ITtMacrossPatchVerifier mVerifier;
        private Assembly mActivatedAssembly;

        public TtMacrossPatchLoader(string patchDirectory, ITtMacrossPatchVerifier verifier = null)
        {
            PatchDirectory = patchDirectory;
            mVerifier = verifier ?? TtMacrossPatchAcceptAllVerifier.Instance;
        }

        public string PatchDirectory { get; }
        public TtMacrossPatchManifest Manifest { get; private set; }
        public string LastError { get; private set; }

        public EStageResult StageA(IEnumerable<KeyValuePair<string, string>> vmAssemblies, Func<string, string, bool> registerAssembly)
        {
            var manifestPath = Path.Combine(PatchDirectory, ManifestFileName);
            if (!File.Exists(manifestPath))
                return EStageResult.NoPatch;
            try
            {
                var manifestBytes = File.ReadAllBytes(manifestPath);
                var signaturePath = Path.Combine(PatchDirectory, SignatureFileName);
                var signatureBytes = File.Exists(signaturePath) ? File.ReadAllBytes(signaturePath) : null;
                if (!mVerifier.Verify(manifestBytes, signatureBytes))
                    return RejectA("signature verification failed");

                var manifest = TtMacrossPatchManifest.Parse(manifestBytes);
                var patchFileName = manifest.PatchAssembly + ".dll";
                var hasPatchFile = false;
                foreach (var file in manifest.Files)
                {
                    if (Path.GetFileName(file.Name) != file.Name)
                        return RejectA($"file name must not contain a path: {file.Name}");
                    var path = Path.Combine(PatchDirectory, file.Name);
                    if (!File.Exists(path))
                        return RejectA($"missing file {file.Name}");
                    if (!string.Equals(Sha256(path), file.Sha256, StringComparison.OrdinalIgnoreCase))
                        return RejectA($"hash mismatch: {file.Name}");
                    hasPatchFile |= string.Equals(file.Name, patchFileName, StringComparison.OrdinalIgnoreCase);
                }
                if (!hasPatchFile)
                    return RejectA($"Files must contain {patchFileName}");

                string mirrorPath = null;
                foreach (var assembly in vmAssemblies)
                {
                    if (string.Equals(assembly.Key, manifest.PatchAssembly, StringComparison.OrdinalIgnoreCase))
                        return RejectA($"patch assembly {manifest.PatchAssembly} conflicts with a shipped LeanCLR assembly");
                    if (string.Equals(assembly.Key, manifest.MirrorAssembly, StringComparison.OrdinalIgnoreCase))
                        mirrorPath = assembly.Value;
                }
                if (mirrorPath == null)
                    return RejectA($"mirror assembly {manifest.MirrorAssembly} is not shipped");
                if (!string.Equals(Sha256(mirrorPath), manifest.MirrorSha256, StringComparison.OrdinalIgnoreCase))
                    return RejectA($"mirror assembly {manifest.MirrorAssembly} does not match the patch baseline");

                if (!registerAssembly(manifest.PatchAssembly, Path.Combine(PatchDirectory, patchFileName)))
                    return RejectA($"registering {manifest.PatchAssembly} failed");
                Manifest = manifest;
                LastError = null;
                return EStageResult.Staged;
            }
            catch (Exception ex)
            {
                return RejectA(ex.Message);
            }
        }

        public bool StageB(TtLeanClrIntrinsicDispatcher dispatcher, Assembly gameAssembly, Func<bool> isVmThread)
        {
            if (Manifest == null || gameAssembly == null)
                return false;
            if (ReferenceEquals(mActivatedAssembly, gameAssembly) && TtMacrossPatch.Active)
                return true;
            if (mActivatedAssembly != null)
            {
                TtMacrossPatch.Deactivate();
                mActivatedAssembly = null;
            }
            if (!dispatcher.EnsureOwnerThread())
                return RejectB("LeanCLR runtime is not ready");
            var runtime = dispatcher.Runtime;
            // 注册 GameProject 中生成的 thunk 注册表；无注册表时返回 false，由后续宿主类型检查兜底
            dispatcher.RegisterAssembly(gameAssembly);
            if (gameAssembly.ManifestModule.ModuleVersionId != Manifest.GameMvid)
                return RejectB($"GameProject MVID {gameAssembly.ManifestModule.ModuleVersionId} does not match baseline {Manifest.GameMvid}");
            if (!runtime.LoadAssembly(Manifest.PatchAssembly))
                return RejectB($"loading {Manifest.PatchAssembly} failed: {runtime.GetLastError().Text}");

            var patchRuntime = new TtMacrossPatchRuntime(runtime, isVmThread, dispatcher.EnsureOwnerThread);
            var overrides = new Dictionary<Type, string>();
            var patchClasses = new HashSet<string>();
            foreach (var m in Manifest.Methods)
                patchClasses.Add(m.Class + PatchClassSuffix);
            Dictionary<string, List<TtMacrossPatchMetadata.FMethod>> declared;
            try
            {
                declared = TtMacrossPatchMetadata.ReadPatchMethods(Path.Combine(PatchDirectory, Manifest.PatchAssembly + ".dll"), patchClasses, PatchClassSuffix);
            }
            catch (Exception ex)
            {
                return RejectB($"reading {Manifest.PatchAssembly} metadata failed: {ex.Message}", patchRuntime);
            }
            var listed = new HashSet<string>(StringComparer.Ordinal);
            foreach (var m in Manifest.Methods)
            {
                if (TtMacrossPatchId.Compute(m.Signature) != m.MethodId)
                    return RejectB($"MethodId 0x{m.MethodId:X16} does not match {m.Signature}", patchRuntime);
                if (!m.Signature.StartsWith(m.Class + "::" + m.Method + "(", StringComparison.Ordinal))
                    return RejectB($"{m.Signature} does not belong to {m.Class}.{m.Method}", patchRuntime);
                var hostType = gameAssembly.GetType(m.Class);
                if (hostType == null || !TtLeanClrHostBinding.IsRegisteredHostType(hostType))
                    return RejectB($"{m.Class} is not a mirrored host type", patchRuntime);
                var patchClass = m.Class + PatchClassSuffix;
                var error = CheckPatchMethod(m, hostType, patchClass, declared);
                if (error != null)
                    return RejectB(error, patchRuntime);
                var method = runtime.FindMethod(Manifest.PatchAssembly, patchClass, m.Method);
                if (!method.IsValidPointer)
                    return RejectB($"{patchClass}.{m.Method} not found", patchRuntime);
                if (method.GetArgumentCount() != CountParameters(m.Signature) || !patchRuntime.Add(m.MethodId, m.Signature, method, out error))
                {
                    unsafe
                    {
                        CoreSDK.IUnknown_Release(method.NativePointer.ToPointer());
                    }
                    return RejectB($"{m.Signature}: {error ?? "parameter count mismatch"}", patchRuntime);
                }
                listed.Add(m.Signature);
                overrides[hostType] = patchClass;
            }
            // 补丁类里未登记的方法：宿主调用走原逻辑而 VM 内调用走新逻辑，不允许存在
            foreach (var i in declared)
            {
                foreach (var d in i.Value)
                {
                    if (!listed.Contains(d.Signature))
                        return RejectB($"{d.Signature} is declared in the patch but not listed in {ManifestFileName}", patchRuntime);
                }
            }
            foreach (var i in overrides)
                TtLeanClrHostBinding.OverrideHostType(i.Key, Manifest.PatchAssembly, i.Value);
            TtMacrossPatch.Activate(patchRuntime);
            mActivatedAssembly = gameAssembly;
            LastError = null;
            Profiler.Log.WriteLine<Profiler.TtCoreGategory>(Profiler.ELogTag.Info,
                $"Macross patch activated: {patchRuntime.Count} methods from {PatchDirectory}.");
            return true;
        }

        // VM 按名字解析补丁方法，故补丁类内不允许同名重载；签名与返回类型都须和宿主方法一致
        private static string CheckPatchMethod(TtMacrossPatchManifest.FMethod m, Type hostType, string patchClass,
            Dictionary<string, List<TtMacrossPatchMetadata.FMethod>> declared)
        {
            if (!declared.TryGetValue(patchClass, out var patchMethods))
                return $"{patchClass} not found in the patch assembly";
            TtMacrossPatchMetadata.FMethod patchMethod = null;
            var sameName = 0;
            foreach (var d in patchMethods)
            {
                if (d.Name != m.Method)
                    continue;
                ++sameName;
                if (d.Signature == m.Signature)
                    patchMethod = d;
            }
            if (sameName > 1)
                return $"{patchClass}.{m.Method} is overloaded; patch methods are resolved by name";
            if (patchMethod == null)
                return $"{m.Signature} has no matching method in {patchClass}";

            MethodInfo hostMethod = null;
            foreach (var i in hostType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
            {
                if (i.Name == m.Method && TtMacrossPatchMetadata.BuildSignature(m.Class, i) == m.Signature)
                {
                    hostMethod = i;
                    break;
                }
            }
            if (hostMethod == null)
                return $"{m.Signature} is not a method of {hostType.FullName}";
            var hostReturn = Rtti.TtTypeDesc.GetCSharpTypeNameString(hostMethod.ReturnType);
            if (hostReturn != patchMethod.ReturnType)
                return $"{m.Signature} returns {patchMethod.ReturnType}, host returns {hostReturn}";
            return null;
        }

        public static int CountParameters(string signature)
        {
            var open = signature.IndexOf('(');
            var close = signature.LastIndexOf(')');
            if (open < 0 || close <= open)
                return -1;
            if (close == open + 1)
                return 0;
            var count = 1;
            var depth = 0;
            for (var i = open + 1; i < close; ++i)
            {
                var c = signature[i];
                if (c == '<' || c == '[')
                    ++depth;
                else if (c == '>' || c == ']')
                    --depth;
                else if (c == ',' && depth == 0)
                    ++count;
            }
            return count;
        }

        private static string Sha256(string path)
        {
            return Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)));
        }

        private EStageResult RejectA(string reason)
        {
            Reject(reason);
            return EStageResult.Rejected;
        }

        private bool RejectB(string reason, TtMacrossPatchRuntime patchRuntime = null)
        {
            patchRuntime?.Dispose();
            Reject(reason);
            return false;
        }

        private void Reject(string reason)
        {
            LastError = reason;
            Profiler.Log.WriteLine<Profiler.TtCoreGategory>(Profiler.ELogTag.Error,
                $"Macross patch rejected ({PatchDirectory}): {reason}");
        }
    }
}
