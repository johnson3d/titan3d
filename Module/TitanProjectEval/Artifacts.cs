using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Titan.Build
{
    /// <summary>
    /// 生成物渲染。格式与既有入库文件逐字节一致（除有意新增的 INPUT_HASHES 与求值器实现输入），
    /// 这样"换实现"这件事本身可以用 diff 证明没有夹带行为变化。
    /// </summary>
    public static class Artifacts
    {
        /// <summary>能力清单里需要固定列出的开关（不来自模块文件的平台裁剪项）。</summary>
        static readonly string[] KnownCapabilities =
        {
            "HasModule_FBX", "HasModule_PythonRuntime", "HasModule_CrossShaderCompiler", "HasModule_TcpServer",
            "HasModule_PhysicsCore", "HasModule_NvPhysX5", "HasModule_MeshSimplify", "HasModule_TextureCompress",
            "HasModule_Quark", "HasModule_Collision", "HasModule_Vulkan", "HasModule_Dx11", "HasModule_Dx12",
            "HasModule_RenderDoc", "HasModule_GpuDump",
        };

        public static string RenderSourcesCmake(ProjectEvaluation debug, ProjectEvaluation release)
        {
            var sb = new StringBuilder();
            var dir = debug.PlatformDir;
            var prefix = debug.Prefix;
            sb.Append("# 由 ").Append(dir).Append("/RefreshSources.cmake 生成，勿手改。\n");
            sb.Append("# 刷新（仓库根）: cmake -P ").Append(dir).Append("/RefreshSources.cmake\n");
            sb.Append("# sources: ").Append(debug.Sources.Count).Append('\n');
            sb.Append("set(TITAN_SOURCES_FINGERPRINT \"").Append(debug.Fingerprint).Append("\")\n");
            AppendSet(sb, "TITAN_SOURCES_INPUTS", debug.Inputs);
            // 逐输入哈希：配置期不跑求值器就能判断已入库清单是否过期，并指出具体文件。
            AppendSet(sb, "TITAN_SOURCES_INPUT_HASHES", debug.InputHashes);
            AppendSet(sb, "TITAN_" + prefix + "_MODULES", debug.Modules);
            AppendSet(sb, "TITAN_" + prefix + "_MODULE_DEFINES", debug.ModuleDefines);
            AppendSet(sb, "TITAN_MODULE_DIRS", debug.ModuleDirs);
            AppendSet(sb, "TITAN_MODULE_SOURCES", debug.Sources);
            foreach (var eval in new[] { debug, release })
            {
                var upper = eval.Configuration.ToUpperInvariant();
                AppendSet(sb, "TITAN_" + prefix + "_INCLUDES_" + upper, eval.Includes);
                AppendSet(sb, "TITAN_" + prefix + "_DEFINES_" + upper, eval.Defines);
            }
            AppendSet(sb, "TITAN_" + prefix + "_SOURCE_DEFINES",
                debug.SourceDefines.Concat(release.SourceDefines));
            return sb.ToString();
        }

        public static string RenderModulesHeader(ProjectEvaluation eval)
        {
            var sb = new StringBuilder();
            sb.Append("#pragma once\n");
            sb.Append("// 由 ").Append(eval.PlatformDir).Append("/RefreshSources.cmake 生成，勿手改。\n");
            sb.Append("#define TITAN_").Append(eval.Prefix).Append("_MODULES_FINGERPRINT \"")
              .Append(eval.Fingerprint).Append("\"\n");
            foreach (var define in eval.ModuleDefines)
                sb.Append("#define ").Append(define).Append('\n');
            return sb.ToString();
        }

        public static string RenderCapabilitiesCs(ProjectEvaluation eval)
        {
            var known = eval.ModuleDefines.Concat(KnownCapabilities)
                .Distinct(StringComparer.Ordinal)
                .OrderBy(x => x, StringComparer.Ordinal);
            var sb = new StringBuilder();
            sb.Append("// 由 ").Append(eval.PlatformDir).Append("/RefreshSources.cmake 生成，勿手改。\n");
            sb.Append("namespace EngineNS\n{\n    public static class TitanNativeCapabilities\n    {\n");
            foreach (var define in known)
            {
                var enabled = eval.ModuleDefines.Contains(define) ? "true" : "false";
                sb.Append("        public const bool ").Append(define).Append(" = ").Append(enabled).Append(";\n");
            }
            sb.Append("    }\n}\n");
            return sb.ToString();
        }

        public static string RenderCapabilitiesProps(ProjectEvaluation eval)
        {
            var known = eval.ModuleDefines.Concat(KnownCapabilities)
                .Distinct(StringComparer.Ordinal)
                .OrderBy(x => x, StringComparer.Ordinal);
            var capabilityDefines = new List<string>();
            foreach (var define in known)
            {
                if (!eval.ModuleDefines.Contains(define))
                    continue;
                var name = define.Substring("HasModule_".Length).ToUpperInvariant();
                capabilityDefines.Add("TITAN_HAS_" + name);
            }
            var sb = new StringBuilder();
            sb.Append("<Project>\n  <PropertyGroup>\n");
            sb.Append("    <TitanNativeCapabilityDefines>")
              .Append(string.Join(";", capabilityDefines))
              .Append("</TitanNativeCapabilityDefines>\n");
            sb.Append("    <TitanNativeCapabilitiesFingerprint>").Append(eval.Fingerprint)
              .Append("</TitanNativeCapabilitiesFingerprint>\n");
            sb.Append("  </PropertyGroup>\n  <ItemGroup><Compile Include=\"$(MSBuildThisFileDirectory)")
              .Append("Titan").Append(eval.Platform).Append("Capabilities.cs\" TitanGenerated=\"true\" /></ItemGroup>\n");
            sb.Append("</Project>\n");
            return sb.ToString();
        }

        /// <summary>一次写出四个生成物；check 模式只比对内容，返回不一致的文件列表。</summary>
        public static List<string> WriteAll(ProjectEvaluation debug, ProjectEvaluation release, string sourcesOutPath, bool check)
        {
            var dir = Path.GetDirectoryName(Path.GetFullPath(sourcesOutPath));
            Directory.CreateDirectory(dir);
            var outputs = new List<(string Path, string Content)>
            {
                (sourcesOutPath, RenderSourcesCmake(debug, release)),
                (Path.Combine(dir, "Titan" + debug.Platform + "Modules.h"), RenderModulesHeader(debug)),
                (Path.Combine(dir, "Titan" + debug.Platform + "Capabilities.props"), RenderCapabilitiesProps(debug)),
                (Path.Combine(dir, "Titan" + debug.Platform + "Capabilities.cs"), RenderCapabilitiesCs(debug)),
            };

            var stale = new List<string>();
            foreach (var output in outputs)
            {
                var content = output.Content;
                if (check)
                {
                    if (!File.Exists(output.Path) || Normalize(File.ReadAllText(output.Path)) != Normalize(content))
                        stale.Add(output.Path);
                    continue;
                }
                var existing = File.Exists(output.Path) ? File.ReadAllText(output.Path) : null;
                if (existing != null && Normalize(existing) == Normalize(content))
                    continue;
                File.WriteAllText(output.Path, content, new UTF8Encoding(false));
                Console.Out.WriteLine("[titan-eval] updated: " + output.Path);
            }
            if (stale.Count == 0)
                Console.Out.WriteLine("[titan-eval] " + debug.Platform + " up to date");
            return stale;
        }

        static string Normalize(string text) => text.Replace("\r", string.Empty);

        static void AppendSet(StringBuilder sb, string name, IEnumerable<string> values)
        {
            sb.Append("set(").Append(name).Append('\n');
            foreach (var value in values)
                sb.Append("  \"").Append(value).Append("\"\n");
            sb.Append(")\n");
        }
    }
}
