using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AloneGame.Maui.Services;

// 纯 BCL 实现：Android 从 assets 读取，测试直接链接本文件，不依赖 MAUI 宿主。
public static class TitanContentInstaller
{
    // 返回带末尾 `/` 的 EngineRoot，供 TtEngine 的 EngineRoot 参数拼接使用。
    public static string Install(string filesDir, Func<string, Stream> openAsset)
    {
        var root = Path.Combine(filesDir, "titan");
        string manifest;
        using (var input = openAsset("titan/manifest.txt"))
        using (var reader = new StreamReader(input)) manifest = reader.ReadToEnd();
        var lines = manifest.Replace("\r", "").Split('\n', StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length < 2) throw new InvalidDataException("内容 manifest 为空");
        var buildId = lines[0];
        var paths = lines.Skip(1).ToArray();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        // 先完整校验再动磁盘：非法 manifest 不能留下“已删除旧内容”的中间态。
        foreach (var path in paths)
        {
            if (Path.IsPathRooted(path) || path.Contains('\\') || path.Contains(':') ||
                path.Split('/').Any(p => p is ".." or "." or "") || !seen.Add(path) ||
                !(path.StartsWith("content/", StringComparison.Ordinal) ||
                  path.StartsWith("enginecontent/", StringComparison.Ordinal) ||
                  path.StartsWith("binaries/", StringComparison.Ordinal)))
                throw new InvalidDataException("非法内容路径：" + path);
        }
        var marker = Path.Combine(root, ".installed");
        if (File.Exists(marker) && File.ReadAllText(marker) == buildId)
            return root.Replace('\\', '/').TrimEnd('/') + "/";
        Directory.CreateDirectory(root);
        if (File.Exists(marker)) File.Delete(marker);
        foreach (var name in new[] { "content", "enginecontent", "binaries" })
        {
            var old = Path.Combine(root, name);
            if (Directory.Exists(old)) Directory.Delete(old, true);
        }
        foreach (var path in paths)
        {
            var destination = Path.Combine(root, path);
            Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
            using var input = openAsset("titan/" + path);
            using var output = File.Create(destination);
            input.CopyTo(output);
        }
        // 只有全部复制成功才落标记；中断时下次启动会整体重装。
        var temporaryMarker = marker + ".tmp";
        File.WriteAllText(temporaryMarker, buildId);
        File.Move(temporaryMarker, marker, true);
        return root.Replace('\\', '/').TrimEnd('/') + "/";
    }
}
