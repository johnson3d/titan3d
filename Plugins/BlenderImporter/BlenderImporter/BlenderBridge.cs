using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;

namespace EngineNS.Bricks.AssetImpExp
{
    public class TtBlendImportSetting
    {
        [Category("FileInfo"), ReadOnly(true)]
        public string SourceFile { get; set; } = "";
        [Category("FileInfo"), ReadOnly(true)]
        public string ExportedFile { get; set; } = "";
        [Category("FileInfo"), ReadOnly(true)]
        public string MaterialManifest { get; set; } = "";
        [Category("Blender")]
        public string BlenderPath { get; set; } = "";
        [Category("Blender")]
        public int TimeoutSeconds { get; set; } = 1800;
        [Category("Export")]
        public bool UseVisibleObjects { get; set; } = true;
        [Category("Export")]
        public bool ApplyModifiers { get; set; } = true;
        [Category("Export")]
        public bool ExportAnimations { get; set; } = true;
        [Category("Export")]
        public bool ExportCameras { get; set; } = false;
        [Category("Export")]
        public bool ExportLights { get; set; } = false;
        [Category("Export")]
        public bool KeepIntermediate { get; set; } = true;
    }

    public class TtBlenderExportResult
    {
        public bool Succeeded { get; set; }
        public int ExitCode { get; set; }
        public string BlenderPath { get; set; } = "";
        public string IntermediateFile { get; set; } = "";
        public string MaterialManifest { get; set; } = "";
        public string LogFile { get; set; } = "";
        public string Error { get; set; } = "";
        public string StandardOutput { get; set; } = "";
        public string StandardError { get; set; } = "";
    }

    public class TtBlenderMaterialManifest
    {
        [System.Text.Json.Serialization.JsonPropertyName("titan_blender_bridge_version")]
        public int TitanBlenderBridgeVersion { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("source_blend")]
        public string SourceBlend { get; set; } = "";
        [System.Text.Json.Serialization.JsonPropertyName("blender_version")]
        public string BlenderVersion { get; set; } = "";
        [System.Text.Json.Serialization.JsonPropertyName("materials")]
        public List<TtBlenderMaterialInfo> Materials { get; set; } = new List<TtBlenderMaterialInfo>();
    }

    public class TtBlenderMaterialInfo
    {
        [System.Text.Json.Serialization.JsonPropertyName("name")]
        public string Name { get; set; } = "";
        [System.Text.Json.Serialization.JsonPropertyName("blend_method")]
        public string BlendMethod { get; set; } = "";
        [System.Text.Json.Serialization.JsonPropertyName("selected_principled")]
        public string SelectedPrincipled { get; set; } = "";
        [System.Text.Json.Serialization.JsonPropertyName("principled")]
        public Dictionary<string, JsonElement> Principled { get; set; } = new Dictionary<string, JsonElement>();
        [System.Text.Json.Serialization.JsonPropertyName("resolved_textures")]
        public TtBlenderResolvedTextures ResolvedTextures { get; set; } = new TtBlenderResolvedTextures();
    }

    public class TtBlenderResolvedTextures
    {
        [System.Text.Json.Serialization.JsonPropertyName("base_color")]
        public TtBlenderTextureRef BaseColor { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("normal")]
        public TtBlenderTextureRef Normal { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("metallic")]
        public TtBlenderTextureRef Metallic { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("roughness")]
        public TtBlenderTextureRef Roughness { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("specular")]
        public TtBlenderTextureRef Specular { get; set; }
    }

    public class TtBlenderTextureRef
    {
        [System.Text.Json.Serialization.JsonPropertyName("node")]
        public string Node { get; set; } = "";
        [System.Text.Json.Serialization.JsonPropertyName("image")]
        public string Image { get; set; } = "";
        [System.Text.Json.Serialization.JsonPropertyName("filepath")]
        public string FilePath { get; set; } = "";
        [System.Text.Json.Serialization.JsonPropertyName("packed")]
        public bool Packed { get; set; }
    }

    public static class TtBlenderBridge
    {
        const int MaterialManifestVersion = 2;

        public static TtBlenderExportResult ExportToGlb(string sourceBlend, TtBlendImportSetting setting = null)
        {
            var result = new TtBlenderExportResult();
            setting ??= new TtBlendImportSetting();
            setting.SourceFile = sourceBlend;

            if (string.IsNullOrWhiteSpace(sourceBlend) || !File.Exists(sourceBlend))
            {
                result.Error = $"Blender source file does not exist: {sourceBlend}";
                return result;
            }

            var blenderPath = ResolveBlenderPath(setting.BlenderPath);
            result.BlenderPath = blenderPath;
            setting.BlenderPath = blenderPath;

            var sourceHash = EngineNS.IO.TtFileInfo.StaticCalcFileHash(sourceBlend);
            if (string.IsNullOrEmpty(sourceHash))
                sourceHash = Guid.NewGuid().ToString("N");
            var cacheDir = GetCacheDirectory(sourceHash);
            EngineNS.IO.TtFileManager.SureDirectory(cacheDir);

            var pureName = EngineNS.IO.TtFileManager.GetPureName(sourceBlend);
            var outGlb = EngineNS.IO.TtFileManager.CombinePath(cacheDir, pureName + ".glb");
            var manifest = EngineNS.IO.TtFileManager.CombinePath(cacheDir, "material.ttblendmat.json");
            var script = EngineNS.IO.TtFileManager.CombinePath(cacheDir, "export_blend.py");
            var logFile = EngineNS.IO.TtFileManager.CombinePath(cacheDir, "blender_export.log");

            setting.ExportedFile = outGlb;
            setting.MaterialManifest = manifest;
            result.IntermediateFile = outGlb;
            result.MaterialManifest = manifest;
            result.LogFile = logFile;

            if (IsCacheValid(outGlb, manifest, sourceBlend))
            {
                result.Succeeded = true;
                return result;
            }

            EngineNS.IO.TtFileManager.WriteAllText(script, BuildExportScript());

            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = blenderPath,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    WorkingDirectory = cacheDir,
                    StandardOutputEncoding = Encoding.UTF8,
                    StandardErrorEncoding = Encoding.UTF8,
                };

                startInfo.ArgumentList.Add("--background");
                startInfo.ArgumentList.Add(sourceBlend);
                startInfo.ArgumentList.Add("--python-exit-code");
                startInfo.ArgumentList.Add("11");
                startInfo.ArgumentList.Add("--python");
                startInfo.ArgumentList.Add(script);
                startInfo.ArgumentList.Add("--");
                startInfo.ArgumentList.Add("--out");
                startInfo.ArgumentList.Add(outGlb);
                startInfo.ArgumentList.Add("--manifest");
                startInfo.ArgumentList.Add(manifest);
                startInfo.ArgumentList.Add("--visible");
                startInfo.ArgumentList.Add(setting.UseVisibleObjects ? "1" : "0");
                startInfo.ArgumentList.Add("--apply");
                startInfo.ArgumentList.Add(setting.ApplyModifiers ? "1" : "0");
                startInfo.ArgumentList.Add("--animations");
                startInfo.ArgumentList.Add(setting.ExportAnimations ? "1" : "0");
                startInfo.ArgumentList.Add("--cameras");
                startInfo.ArgumentList.Add(setting.ExportCameras ? "1" : "0");
                startInfo.ArgumentList.Add("--lights");
                startInfo.ArgumentList.Add(setting.ExportLights ? "1" : "0");

                using var process = new Process { StartInfo = startInfo };
                process.Start();

                var outputTask = process.StandardOutput.ReadToEndAsync();
                var errorTask = process.StandardError.ReadToEndAsync();
                var finished = process.WaitForExit(Math.Max(1, setting.TimeoutSeconds) * 1000);

                if (!finished)
                {
                    try
                    {
                        process.Kill(true);
                    }
                    catch
                    {
                    }
                    result.Error = $"Blender export timed out after {setting.TimeoutSeconds} seconds.";
                }

                result.ExitCode = finished ? process.ExitCode : -1;
                var output = outputTask.GetAwaiter().GetResult();
                var error = errorTask.GetAwaiter().GetResult();
                result.StandardOutput = output;
                result.StandardError = error;

                EngineNS.IO.TtFileManager.WriteAllText(logFile, output + Environment.NewLine + error);

                if (!finished)
                    return result;

                if (process.ExitCode != 0)
                {
                    result.Error = $"Blender export failed with exit code {process.ExitCode}. See {logFile}";
                    return result;
                }

                if (!File.Exists(outGlb))
                {
                    result.Error = $"Blender export completed but no GLB was generated: {outGlb}";
                    return result;
                }

                result.Succeeded = true;
                return result;
            }
            catch (Exception ex)
            {
                result.Error = $"Failed to start Blender. {ex.Message}";
                EngineNS.Profiler.Log.WriteException(ex);
                return result;
            }
        }

        public static TtBlenderMaterialManifest LoadMaterialManifest(string manifestPath)
        {
            if (string.IsNullOrWhiteSpace(manifestPath) || !File.Exists(manifestPath))
                return null;

            try
            {
                var json = File.ReadAllText(manifestPath);
                return JsonSerializer.Deserialize<TtBlenderMaterialManifest>(json, new JsonSerializerOptions()
                {
                    PropertyNameCaseInsensitive = true,
                });
            }
            catch (Exception ex)
            {
                EngineNS.Profiler.Log.WriteException(ex);
                return null;
            }
        }

        static bool IsCacheValid(string outGlb, string manifest, string sourceBlend)
        {
            if (!File.Exists(outGlb) ||
                new FileInfo(outGlb).Length <= 0 ||
                File.GetLastWriteTimeUtc(outGlb) < File.GetLastWriteTimeUtc(sourceBlend) ||
                !File.Exists(manifest))
            {
                return false;
            }

            try
            {
                using var document = JsonDocument.Parse(File.ReadAllText(manifest));
                if (!document.RootElement.TryGetProperty("titan_blender_bridge_version", out var versionElement))
                    return false;

                return versionElement.GetInt32() >= MaterialManifestVersion;
            }
            catch
            {
                return false;
            }
        }

        static string GetCacheDirectory(string sourceHash)
        {
            var root = TtEngine.Instance.FileManager.GetRoot(EngineNS.IO.TtFileManager.ERootDir.Cache);
            return EngineNS.IO.TtFileManager.CombinePath(root, $"import/blender/{sourceHash.Substring(0, Math.Min(16, sourceHash.Length))}/");
        }

        public static string ResolveBlenderPath(string configuredPath = null)
        {
            var candidates = new List<string>();
            AddCandidate(candidates, configuredPath);
            AddCandidate(candidates, Environment.GetEnvironmentVariable("TITAN_BLENDER_PATH"));
            AddCandidate(candidates, Environment.GetEnvironmentVariable("BLENDER_PATH"));
            AddCandidate(candidates, Environment.GetEnvironmentVariable("BLENDER_EXE"));

            var pathEnv = Environment.GetEnvironmentVariable("PATH");
            if (!string.IsNullOrWhiteSpace(pathEnv))
            {
                foreach (var part in pathEnv.Split(Path.PathSeparator))
                {
                    if (string.IsNullOrWhiteSpace(part))
                        continue;
                    AddCandidate(candidates, Path.Combine(part.Trim(), "blender.exe"));
                    AddCandidate(candidates, Path.Combine(part.Trim(), "blender"));
                }
            }

            AddBlenderFoundationCandidates(candidates, Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles));
            AddBlenderFoundationCandidates(candidates, Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86));

            foreach (var candidate in candidates)
            {
                if (File.Exists(candidate))
                    return candidate;
            }

            return "blender";
        }

        static void AddCandidate(List<string> candidates, string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return;
            path = path.Trim('"', ' ');
            if (Directory.Exists(path))
                path = Path.Combine(path, "blender.exe");
            foreach (var candidate in candidates)
            {
                if (string.Equals(candidate, path, StringComparison.OrdinalIgnoreCase))
                    return;
            }
            candidates.Add(path);
        }

        static void AddBlenderFoundationCandidates(List<string> candidates, string programFiles)
        {
            if (string.IsNullOrWhiteSpace(programFiles))
                return;

            var root = Path.Combine(programFiles, "Blender Foundation");
            if (!Directory.Exists(root))
                return;

            var dirs = Directory.GetDirectories(root, "Blender*");
            Array.Sort(dirs, StringComparer.OrdinalIgnoreCase);
            for (var i = dirs.Length - 1; i >= 0; i--)
            {
                AddCandidate(candidates, Path.Combine(dirs[i], "blender.exe"));
            }
        }

        static string BuildExportScript()
        {
            return @"import argparse
import json
import os
import sys

import bpy

MANIFEST_VERSION = 2


def parse_bool(value):
    return str(value).lower() in ('1', 'true', 'yes', 'on')


def socket_value(socket):
    if socket is None:
        return None
    if socket.is_linked:
        link = socket.links[0]
        return {
            'linked': True,
            'from_node': link.from_node.name,
            'from_socket': link.from_socket.name,
            'from_type': link.from_node.bl_idname,
        }
    value = getattr(socket, 'default_value', None)
    if value is None:
        return None
    try:
        return list(value)
    except TypeError:
        return value


def image_info(node):
    image = getattr(node, 'image', None)
    if image is None:
        return None
    filepath = getattr(image, 'filepath', '') or ''
    if filepath:
        try:
            filepath = bpy.path.abspath(filepath)
        except Exception:
            pass
    return {
        'node': node.name,
        'image': image.name,
        'filepath': filepath,
        'packed': bool(getattr(image, 'packed_file', None)),
    }


def iter_linked_inputs(node, preferred_names=()):
    emitted = set()
    for name in preferred_names:
        for index, socket in enumerate(node.inputs):
            if index in emitted:
                continue
            if socket.name == name and socket.is_linked:
                emitted.add(index)
                yield socket
    for index, socket in enumerate(node.inputs):
        if index in emitted:
            continue
        if socket.is_linked and socket.name not in ('Factor', 'Fac'):
            emitted.add(index)
            yield socket
    for index, socket in enumerate(node.inputs):
        if index in emitted:
            continue
        if socket.is_linked:
            emitted.add(index)
            yield socket


def resolve_texture_from_socket(socket, visited=None, depth=0):
    if socket is None or not socket.is_linked:
        return None
    if visited is None:
        visited = set()
    if depth > 16:
        return None
    for link in socket.links:
        result = resolve_texture_from_node(link.from_node, link.from_socket, visited, depth + 1)
        if result is not None:
            return result
    return None


def resolve_texture_from_node(node, from_socket=None, visited=None, depth=0):
    if node is None:
        return None
    if visited is None:
        visited = set()
    key = (node.name, from_socket.name if from_socket else '')
    if key in visited or depth > 16:
        return None
    visited.add(key)

    if node.bl_idname == 'ShaderNodeTexImage':
        return image_info(node)

    preferred = ()
    if node.bl_idname in ('ShaderNodeNormalMap', 'ShaderNodeInvert', 'ShaderNodeSeparateColor'):
        preferred = ('Color', 'Image')
    elif node.bl_idname == 'ShaderNodeBump':
        preferred = ('Height', 'Normal')
    elif node.bl_idname == 'ShaderNodeValToRGB':
        preferred = ('Fac', 'Factor')
    elif node.bl_idname in ('ShaderNodeMix', 'ShaderNodeMixRGB'):
        preferred = ('A', 'B', 'Color1', 'Color2', 'Image', 'Color')
    elif node.bl_idname == 'ShaderNodeMath':
        preferred = ('Value',)
    elif node.bl_idname == 'ShaderNodeCombineColor':
        preferred = ('Red', 'Green', 'Blue')

    for input_socket in iter_linked_inputs(node, preferred):
        result = resolve_texture_from_socket(input_socket, visited, depth + 1)
        if result is not None:
            return result
    return None


def collect_upstream_principled(node, visited=None):
    if node is None:
        return []
    if visited is None:
        visited = set()
    if node.name in visited:
        return []
    visited.add(node.name)
    if node.bl_idname == 'ShaderNodeBsdfPrincipled':
        return [node]
    result = []
    for socket in iter_linked_inputs(node):
        for link in socket.links:
            result.extend(collect_upstream_principled(link.from_node, visited))
    return result


def get_input(node, names):
    if node is None:
        return None
    for name in names:
        if name in node.inputs:
            return node.inputs[name]
    return None


def collect_resolved_textures(principled):
    if principled is None:
        return {}
    resolved = {}
    mappings = (
        ('base_color', ('Base Color',)),
        ('normal', ('Normal',)),
        ('metallic', ('Metallic',)),
        ('roughness', ('Roughness',)),
        ('specular', ('Specular IOR Level', 'Specular', 'Specular Tint')),
    )
    for key, names in mappings:
        texture = resolve_texture_from_socket(get_input(principled, names))
        if texture is not None:
            resolved[key] = texture
    return resolved


def score_principled(node):
    resolved = collect_resolved_textures(node)
    score = 0
    if 'base_color' in resolved:
        score += 100
    if 'normal' in resolved:
        score += 20
    if 'roughness' in resolved or 'specular' in resolved:
        score += 10
    return score


def find_principled(material):
    if material is None or not material.use_nodes or material.node_tree is None:
        return None
    nodes = material.node_tree.nodes
    outputs = [n for n in nodes if n.bl_idname == 'ShaderNodeOutputMaterial']
    outputs.sort(key=lambda n: 0 if getattr(n, 'is_active_output', False) else 1)
    for output in outputs:
        surface = output.inputs.get('Surface')
        if surface is None or not surface.is_linked:
            continue
        candidates = []
        for link in surface.links:
            candidates.extend(collect_upstream_principled(link.from_node))
        if candidates:
            candidates.sort(key=score_principled, reverse=True)
            return candidates[0]
    candidates = [node for node in nodes if node.bl_idname == 'ShaderNodeBsdfPrincipled']
    if candidates:
        candidates.sort(key=score_principled, reverse=True)
        return candidates[0]
    return None


def collect_materials():
    materials = []
    interesting_inputs = [
        'Base Color',
        'Metallic',
        'Roughness',
        'Alpha',
        'Normal',
        'Emission Color',
        'Emission Strength',
        'Coat Weight',
        'Coat Roughness',
        'Sheen Weight',
        'IOR',
        'Transmission Weight',
    ]
    for mat in bpy.data.materials:
        principled = find_principled(mat)
        mat_info = {
            'name': mat.name,
            'use_nodes': bool(mat.use_nodes),
            'blend_method': getattr(mat, 'blend_method', None),
            'surface': principled.bl_idname if principled else None,
            'selected_principled': principled.name if principled else None,
            'principled': {},
            'resolved_textures': collect_resolved_textures(principled),
            'unsupported_nodes': [],
        }
        if mat.use_nodes and mat.node_tree:
            for node in mat.node_tree.nodes:
                if node.bl_idname not in (
                    'ShaderNodeOutputMaterial',
                    'ShaderNodeBsdfPrincipled',
                    'ShaderNodeTexImage',
                    'ShaderNodeNormalMap',
                    'ShaderNodeBump',
                    'ShaderNodeMapping',
                    'ShaderNodeTexCoord',
                    'ShaderNodeValue',
                    'ShaderNodeRGB',
                    'ShaderNodeMix',
                    'ShaderNodeMixRGB',
                    'ShaderNodeMath',
                    'ShaderNodeValToRGB',
                    'ShaderNodeInvert',
                    'ShaderNodeSeparateColor',
                    'ShaderNodeCombineColor',
                ):
                    mat_info['unsupported_nodes'].append({'name': node.name, 'type': node.bl_idname})
        if principled:
            for input_name in interesting_inputs:
                if input_name in principled.inputs:
                    mat_info['principled'][input_name] = socket_value(principled.inputs[input_name])
        materials.append(mat_info)
    return materials


def supported_export_args():
    try:
        return {prop.identifier for prop in bpy.ops.export_scene.gltf.get_rna_type().properties}
    except Exception:
        return set()


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('--out', required=True)
    parser.add_argument('--manifest', required=True)
    parser.add_argument('--visible', default='1')
    parser.add_argument('--apply', default='1')
    parser.add_argument('--animations', default='1')
    parser.add_argument('--cameras', default='0')
    parser.add_argument('--lights', default='0')
    args = parser.parse_args(sys.argv[sys.argv.index('--') + 1:] if '--' in sys.argv else [])

    os.makedirs(os.path.dirname(args.out), exist_ok=True)
    os.makedirs(os.path.dirname(args.manifest), exist_ok=True)

    manifest = {
        'titan_blender_bridge_version': MANIFEST_VERSION,
        'source_blend': bpy.data.filepath,
        'blender_version': bpy.app.version_string,
        'materials': collect_materials(),
    }
    with open(args.manifest, 'w', encoding='utf-8') as f:
        json.dump(manifest, f, ensure_ascii=False, indent=2)

    export_kwargs = {
        'filepath': args.out,
        'export_format': 'GLB',
        'export_materials': 'EXPORT',
        'export_animations': parse_bool(args.animations),
        'export_cameras': parse_bool(args.cameras),
        'export_lights': parse_bool(args.lights),
        'export_apply': parse_bool(args.apply),
        'use_visible': parse_bool(args.visible),
    }
    supported = supported_export_args()
    if supported:
        export_kwargs = {k: v for k, v in export_kwargs.items() if k in supported}
    bpy.ops.export_scene.gltf(**export_kwargs)


if __name__ == '__main__':
    main()
";
        }
    }
}
