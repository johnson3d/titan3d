using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace EngineNS.Macross
{
    /// <summary>
    /// 补丁工程编译/校验/打包核心服务。TitanCMD macross-patch 子命令和编辑器"导出补丁"菜单共用。
    /// </summary>
    public sealed class TtMacrossPatchBuilder
    {
        public sealed class FBuildResult
        {
            public bool Success;
            public string OutputDirectory;
            public readonly List<string> Diagnostics = new();
        }

        private readonly string mPatchProjectDir;
        private readonly string mBaselinePath;
        private readonly string mOutputDir;

        public TtMacrossPatchBuilder(string patchProjectDir, string baselinePath, string outputDir)
        {
            mPatchProjectDir = patchProjectDir;
            mBaselinePath = baselinePath;
            mOutputDir = outputDir;
        }

        public FBuildResult Build()
        {
            var result = new FBuildResult { OutputDirectory = mOutputDir };

            // 1. 加载基线清单
            TtMacrossPatchBaseline baseline;
            try
            {
                baseline = TtMacrossPatchBaseline.Load(mBaselinePath);
            }
            catch (Exception ex)
            {
                result.Diagnostics.Add($"[ERROR] Failed to load baseline: {ex.Message}");
                return result;
            }

            if (baseline.Methods.Count == 0)
            {
                result.Diagnostics.Add("[ERROR] Baseline contains no methods.");
                return result;
            }

            // 2. 查找补丁 DLL（预编译或进程内 Roslyn）
            var patchDllPath = FindOrCompilePatchDll(result);
            if (patchDllPath == null)
                return result;

            // 3. 读取补丁元数据
            Dictionary<string, List<TtMacrossPatchMetadata.FMethod>> declaredMethods;
            try
            {
                var patchClasses = new HashSet<string>();
                foreach (var m in baseline.Methods)
                {
                    var dotIdx = m.Signature.IndexOf("::", StringComparison.Ordinal);
                    if (dotIdx > 0)
                    {
                        var cls = m.Signature.Substring(0, dotIdx);
                        patchClasses.Add(cls + TtMacrossPatchLoader.PatchClassSuffix);
                    }
                }
                declaredMethods = TtMacrossPatchMetadata.ReadPatchMethods(patchDllPath, patchClasses, TtMacrossPatchLoader.PatchClassSuffix);
            }
            catch (Exception ex)
            {
                result.Diagnostics.Add($"[ERROR] Failed to read patch metadata: {ex.Message}");
                return result;
            }

            // 4. 校验补丁方法 — 与基线匹配
            var baselineMap = new Dictionary<string, TtMacrossPatchBaseline.FMethod>();
            foreach (var m in baseline.Methods)
                baselineMap[m.Signature] = m;

            var patchMethods = new List<TtMacrossPatchManifest.FMethod>();
            foreach (var kvp in declaredMethods)
            {
                foreach (var method in kvp.Value)
                {
                    // 校验签名存在于基线
                    if (!baselineMap.TryGetValue(method.Signature, out var baselineMethod))
                    {
                        result.Diagnostics.Add($"[ERROR] {method.Signature} is not in the baseline — requires a new release.");
                        continue;
                    }

                    // 校验 MethodId 一致
                    var computedId = TtMacrossPatchId.Compute(method.Signature);
                    if (computedId != baselineMethod.MethodId)
                    {
                        result.Diagnostics.Add($"[ERROR] {method.Signature}: MethodId mismatch (computed=0x{computedId:X16}, baseline=0x{baselineMethod.MethodId:X16}).");
                        continue;
                    }

                    // 提取宿主类名和方法名
                    var colonIdx = method.Signature.IndexOf("::", StringComparison.Ordinal);
                    var parenIdx = method.Signature.IndexOf('(');
                    var hostClass = method.Signature.Substring(0, colonIdx);
                    var methodName = method.Signature.Substring(colonIdx + 2, parenIdx - colonIdx - 2);

                    patchMethods.Add(new TtMacrossPatchManifest.FMethod
                    {
                        MethodId = computedId,
                        Class = hostClass,
                        Method = methodName,
                        Signature = method.Signature,
                    });
                }
            }

            if (result.Diagnostics.Exists(d => d.StartsWith("[ERROR]")))
                return result;

            if (patchMethods.Count == 0)
            {
                result.Diagnostics.Add("[WARNING] No patch methods found — nothing to package.");
                return result;
            }

            // 5. 打包输出
            IO.TtFileManager.CreateDirectory(mOutputDir);
            var patchAssemblyName = "GameProject.Patch";
            var dllFileName = patchAssemblyName + ".dll";
            var pdbFileName = patchAssemblyName + ".pdb";

            var files = new List<TtMacrossPatchManifest.FFile>();
            CopyAndHash(patchDllPath, Path.Combine(mOutputDir, dllFileName), dllFileName, files);

            var pdbPath = Path.ChangeExtension(patchDllPath, ".pdb");
            if (File.Exists(pdbPath))
                CopyAndHash(pdbPath, Path.Combine(mOutputDir, pdbFileName), pdbFileName, files);

            // 6. 生成 patch.json
            var manifest = new
            {
                BaselineId = new
                {
                    GameMvid = baseline.GameMvid.ToString(),
                    MirrorAssembly = baseline.MirrorAssembly,
                    MirrorSha256 = baseline.MirrorSha256,
                },
                PatchAssembly = patchAssemblyName,
                Methods = patchMethods.ConvertAll(m => new
                {
                    MethodId = $"0x{m.MethodId:X16}",
                    m.Class,
                    m.Method,
                    m.Signature,
                }),
                Files = files.ConvertAll(f => new { f.Name, f.Sha256 }),
            };

            var jsonPath = Path.Combine(mOutputDir, TtMacrossPatchLoader.ManifestFileName);
            var jsonBytes = JsonSerializer.SerializeToUtf8Bytes(manifest, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllBytes(jsonPath, jsonBytes);

            result.Success = true;
            result.Diagnostics.Add($"[INFO] Patch package written to {mOutputDir} ({patchMethods.Count} methods).");
            return result;
        }

        private string FindOrCompilePatchDll(FBuildResult result)
        {
            // 查找预编译 DLL
            var binDebug = Path.Combine(mPatchProjectDir, "bin", "Debug");
            if (Directory.Exists(binDebug))
            {
                foreach (var dir in Directory.GetDirectories(binDebug))
                {
                    var candidate = Path.Combine(dir, "GameProject.Patch.dll");
                    if (File.Exists(candidate))
                        return candidate;
                }
            }
            var binRelease = Path.Combine(mPatchProjectDir, "bin", "Release");
            if (Directory.Exists(binRelease))
            {
                foreach (var dir in Directory.GetDirectories(binRelease))
                {
                    var candidate = Path.Combine(dir, "GameProject.Patch.dll");
                    if (File.Exists(candidate))
                        return candidate;
                }
            }

            result.Diagnostics.Add("[ERROR] GameProject.Patch.dll not found. Build the patch project first (dotnet build).");
            return null;
        }

        private static void CopyAndHash(string src, string dst, string name, List<TtMacrossPatchManifest.FFile> files)
        {
            var bytes = File.ReadAllBytes(src);
            File.WriteAllBytes(dst, bytes);
            files.Add(new TtMacrossPatchManifest.FFile
            {
                Name = name,
                Sha256 = Convert.ToHexString(SHA256.HashData(bytes)),
            });
        }
    }
}
