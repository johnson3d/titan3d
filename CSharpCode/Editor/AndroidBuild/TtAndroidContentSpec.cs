using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace EngineNS.Editor.AndroidBuild
{
    // 只负责 Module/AloneGame.Maui/TitanContent.txt：解析、校验、增删与 ASCII CRLF 写回。
    // 暂存脚本（Stage-AndroidContent.ps1）是它的唯一消费者，格式约束由脚本决定。
    public sealed class TtAndroidContentSpec
    {
        public const string HeaderComment = "# Android 运行期内容：每行一条相对仓库根的匹配式，# 为注释，** 递归。必须纯 ASCII。";
        private static readonly string[] AllowedPrefixes = { "content/", "enginecontent/", "binaries/" };

        private readonly List<string> mPatterns = new List<string>();

        public IReadOnlyList<string> Patterns => mPatterns;

        public static TtAndroidContentSpec Parse(string text)
        {
            var spec = new TtAndroidContentSpec();
            if (string.IsNullOrEmpty(text))
                return spec;
            foreach (var rawLine in text.Replace("\r\n", "\n").Split('\n'))
            {
                var line = rawLine.Trim();
                if (line.Length == 0 || line.StartsWith("#") || line.StartsWith("//"))
                    continue;
                spec.mPatterns.Add(spec.Normalize(line));
            }
            return spec;
        }

        public static TtAndroidContentSpec Load(string path)
        {
            if (!File.Exists(path))
                return new TtAndroidContentSpec();
            return Parse(File.ReadAllText(path, Encoding.UTF8));
        }

        // 统一为脚本可识别的写法：反斜杠转正斜杠、合并连续斜杠、去掉 ./ 与首尾空白。
        public string Normalize(string pattern)
        {
            var text = (pattern ?? string.Empty).Trim().Replace('\\', '/');
            while (text.Contains("//"))
                text = text.Replace("//", "/");
            if (text.StartsWith("./"))
                text = text.Substring(2);
            return text;
        }

        // 返回 null 表示合法；否则返回中文原因供面板展示。
        public static string Validate(string pattern)
        {
            if (string.IsNullOrWhiteSpace(pattern))
                return "内容为空。";
            if (pattern.Contains(".."))
                return "不允许出现 .. 路径穿越。";
            if (pattern.StartsWith("/"))
                return "不允许以 / 开头。";
            if (pattern.Length >= 2 && pattern[1] == ':')
                return "不允许盘符。";
            foreach (var c in pattern)
            {
                if (c > 127)
                    return "只接受 ASCII 路径（暂存脚本以 Ascii 编码写 manifest.txt）。";
            }
            bool allowed = false;
            foreach (var prefix in AllowedPrefixes)
            {
                if (pattern.StartsWith(prefix))
                {
                    allowed = true;
                    break;
                }
            }
            if (!allowed)
                return "只允许 content/、enginecontent/、binaries/ 前缀。";
            return null;
        }

        public bool Add(string pattern)
        {
            var normalized = Normalize(pattern);
            if (Validate(normalized) != null || mPatterns.Contains(normalized))
                return false;
            mPatterns.Add(normalized);
            return true;
        }

        public bool Remove(string pattern)
        {
            return mPatterns.Remove(Normalize(pattern));
        }

        public string Serialize()
        {
            var builder = new StringBuilder();
            builder.AppendLine(HeaderComment);
            foreach (var pattern in mPatterns)
                builder.AppendLine(pattern);
            return builder.ToString();
        }

        // 暂存脚本按行读，模式行均为 ASCII（Validate 保证）；中文头注释用 UTF-8 无 BOM 落盘，
        // 不得用 ASCII 编码（会把注释写成 '?????'）。
        public void Save(string path)
        {
            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);
            File.WriteAllText(path, Serialize(), new UTF8Encoding(false));
        }
    }
}
