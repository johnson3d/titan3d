using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace LocalizationTool
{
    // 单个文件的改写预览结果。
    public class TtFileRewritePreview
    {
        public string AbsoluteFilePath;
        public string RelativeFilePath;
        public string OriginalText;
        public string NewText;
        public int ReplacementCount;
        public bool Changed => ReplacementCount > 0 && string.Equals(OriginalText, NewText, StringComparison.Ordinal) == false;
        public string Error;
    }

    // 一次改写（预览或应用）的汇总。
    public class TtRewriteResult
    {
        public List<TtFileRewritePreview> Files = new List<TtFileRewritePreview>();
        public List<string> Errors = new List<string>();
        public int AppliedFileCount;
        public int AppliedOccurrenceCount;
    }

    // 幂等源码改写器：
    // - 只处理 CanRewrite 且被用户勾选、未忽略的 occurrence；
    // - 改写前校验文件哈希（扫描后被外部改动则拒绝，要求重扫）；
    // - 同一文件内按 SpanStart 降序做纯文本替换，避免偏移错位；
    // - 写入前重新 parse，出现新增 error diagnostics 则终止该文件；
    // - 通过临时文件替换，失败不留下半写文件。
    public class TtSourceRewriter
    {
        // 生成预览而不写盘。fileHashes 为扫描时记录的 “绝对路径 -> sha256”，可为 null 表示不校验。
        public TtRewriteResult BuildPreview(IEnumerable<TtStringOccurrence> occurrences, IReadOnlyDictionary<string, string> fileHashes)
        {
            var result = new TtRewriteResult();
            foreach (var group in GroupByFile(occurrences))
            {
                var preview = BuildFilePreview(group.Key, group.Value, fileHashes, result);
                if (preview != null)
                    result.Files.Add(preview);
            }
            return result;
        }

        // 生成预览并把有改动、无错误的文件写回磁盘。
        public TtRewriteResult Apply(IEnumerable<TtStringOccurrence> occurrences, IReadOnlyDictionary<string, string> fileHashes)
        {
            var result = BuildPreview(occurrences, fileHashes);
            foreach (var file in result.Files)
            {
                if (file.Changed == false || string.IsNullOrEmpty(file.Error) == false)
                    continue;
                try
                {
                    WriteAtomic(file.AbsoluteFilePath, file.NewText);
                    result.AppliedFileCount++;
                    result.AppliedOccurrenceCount += file.ReplacementCount;
                }
                catch (Exception ex)
                {
                    file.Error = $"Write failed: {ex.Message}";
                    result.Errors.Add($"{file.RelativeFilePath}: {ex.Message}");
                }
            }
            return result;
        }

        static Dictionary<string, List<TtStringOccurrence>> GroupByFile(IEnumerable<TtStringOccurrence> occurrences)
        {
            var map = new Dictionary<string, List<TtStringOccurrence>>(StringComparer.OrdinalIgnoreCase);
            foreach (var occ in occurrences)
            {
                if (occ == null)
                    continue;
                if (occ.Ignored || occ.Selected == false || occ.CanRewrite == false)
                    continue;
                if (map.TryGetValue(occ.AbsoluteFilePath, out var list) == false)
                {
                    list = new List<TtStringOccurrence>();
                    map[occ.AbsoluteFilePath] = list;
                }
                list.Add(occ);
            }
            return map;
        }

        TtFileRewritePreview BuildFilePreview(string absoluteFile, List<TtStringOccurrence> occurrences,
            IReadOnlyDictionary<string, string> fileHashes, TtRewriteResult result)
        {
            var preview = new TtFileRewritePreview
            {
                AbsoluteFilePath = absoluteFile,
                RelativeFilePath = occurrences.Count > 0 ? occurrences[0].RelativeFilePath : absoluteFile,
            };

            if (File.Exists(absoluteFile) == false)
            {
                preview.Error = "File not found (rescan required).";
                result.Errors.Add($"{preview.RelativeFilePath}: file not found");
                return preview;
            }

            // 校验文件哈希：扫描后文件被外部改动则拒绝改写。
            if (fileHashes != null && fileHashes.TryGetValue(absoluteFile, out var expectedHash))
            {
                var actual = TtSourceScanner.ComputeFileHash(absoluteFile);
                if (string.Equals(actual, expectedHash, StringComparison.OrdinalIgnoreCase) == false)
                {
                    preview.Error = "File changed since scan (rescan required).";
                    result.Errors.Add($"{preview.RelativeFilePath}: changed since scan");
                    return preview;
                }
            }

            var original = File.ReadAllText(absoluteFile);
            preview.OriginalText = original;

            // 排序：按 SpanStart 降序，从文件末尾往前替换，保证前面的 span 偏移不受影响。
            // 同起点时，先做长度更长的替换（如 attribute 名替换与其子字面量不会重叠，这里仅作稳定排序）。
            var ordered = occurrences
                .Where(o => o.SpanLength > 0 && string.IsNullOrEmpty(o.ReplacementText) == false)
                .OrderByDescending(o => o.SpanStart)
                .ThenByDescending(o => o.SpanLength)
                .ToList();

            var sb = new StringBuilder(original);
            int lastStart = int.MaxValue;
            int applied = 0;
            foreach (var occ in ordered)
            {
                var end = occ.SpanStart + occ.SpanLength;
                if (occ.SpanStart < 0 || end > sb.Length)
                {
                    preview.Error = "Span out of range (rescan required).";
                    result.Errors.Add($"{preview.RelativeFilePath}: span out of range at {occ.SpanStart}");
                    return preview;
                }
                // 检测重叠：当前替换的结束位置不能越过上一次替换的起点。
                if (end > lastStart)
                {
                    // 重叠（如 attribute 名替换与其内部字面量），跳过后者，避免破坏文本。
                    continue;
                }
                sb.Remove(occ.SpanStart, occ.SpanLength);
                sb.Insert(occ.SpanStart, occ.ReplacementText);
                lastStart = occ.SpanStart;
                applied++;
            }

            preview.NewText = sb.ToString();
            preview.ReplacementCount = applied;

            if (preview.Changed)
            {
                var diagError = CheckNewErrors(original, preview.NewText);
                if (diagError != null)
                {
                    preview.Error = $"Rewrite introduced errors: {diagError}";
                    result.Errors.Add($"{preview.RelativeFilePath}: {diagError}");
                }
            }
            return preview;
        }

        // 重新 parse 改写后的文本；若相比原文新增了 error 级别 diagnostics，则视为改写破坏了语法。
        static string CheckNewErrors(string originalText, string newText)
        {
            try
            {
                var before = CountSyntaxErrors(originalText, out _);
                var after = CountSyntaxErrors(newText, out var firstError);
                if (after > before)
                    return firstError ?? $"error count {before} -> {after}";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return null;
        }

        static int CountSyntaxErrors(string text, out string firstError)
        {
            firstError = null;
            var tree = CSharpSyntaxTree.ParseText(text);
            int count = 0;
            foreach (var d in tree.GetDiagnostics())
            {
                if (d.Severity == DiagnosticSeverity.Error)
                {
                    if (firstError == null)
                        firstError = d.GetMessage();
                    count++;
                }
            }
            return count;
        }

        // 原子写入：写临时文件后替换目标，避免中途失败留下半写文件。保留 UTF-8（无 BOM）。
        static void WriteAtomic(string absoluteFile, string content)
        {
            var dir = Path.GetDirectoryName(absoluteFile);
            var tmp = Path.Combine(dir, Path.GetFileName(absoluteFile) + ".loctmp");
            var utf8 = new UTF8Encoding(false);
            File.WriteAllText(tmp, content, utf8);
            try
            {
                File.Copy(tmp, absoluteFile, true);
            }
            finally
            {
                try { File.Delete(tmp); } catch { }
            }
        }
    }
}
