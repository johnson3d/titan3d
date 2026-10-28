using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Text;

namespace CSharpCodeTools;

// 迁移只识别真正的预处理指令，不把字符串和用户注释中的文本当成工具区块。
public static class LegacyGeneratedBlocks
{
    private static readonly HashSet<string> Names = new(StringComparer.Ordinal)
    {
        "TitanEngine_AutoGen_RPC", "TitanEngine_AutoGen_AutoSync", "TitanEngine_AutoGen_Macross"
    };

    public static string[] MigrateRepository(string root, bool apply)
    {
        root = Path.GetFullPath(root);
        if (!Directory.Exists(Path.Combine(root, "CSharpCode")) && !Directory.Exists(Path.Combine(root, "Plugins")))
            throw new DirectoryNotFoundException("迁移根目录必须包含 CSharpCode 或 Plugins：" + root);
        IEnumerable<string> Enumerate(string directory)
        {
            if ((File.GetAttributes(directory) & FileAttributes.ReparsePoint) != 0)
                throw new InvalidDataException("迁移不允许重解析目录：" + directory);
            foreach (var file in Directory.EnumerateFiles(directory, "*.cs")) yield return file;
            foreach (var child in Directory.EnumerateDirectories(directory))
            {
                var name = Path.GetFileName(child);
                if (name.Equals("bin", StringComparison.OrdinalIgnoreCase) || name.Equals("obj", StringComparison.OrdinalIgnoreCase)
                    || name.Equals("binaries", StringComparison.OrdinalIgnoreCase) || name.Equals("intermediate", StringComparison.OrdinalIgnoreCase)
                    || name.StartsWith("build", StringComparison.OrdinalIgnoreCase)) continue;
                foreach (var file in Enumerate(child)) yield return file;
            }
        }
        var files = new[] { "CSharpCode", "Plugins" }.Select(folder => Path.Combine(root, folder))
            .Where(Directory.Exists).SelectMany(Enumerate);
        return MigrateFilesCore(files, apply, (file, original) =>
        {
            var baseline = Path.Combine(root, "artifacts", "platform-codegen-baseline", Path.GetRelativePath(root, file));
            if (!File.Exists(baseline) || !File.ReadAllBytes(baseline).AsSpan().SequenceEqual(original))
                throw new InvalidDataException("迁移要求已保存且字节一致的原始源码基线：" + file + "; 基线=" + baseline);
        });
    }

    // 一次性迁移先校验所有输入；获取全部写锁后才改写，标记或文件访问失败不留下部分迁移。
    public static string[] MigrateFiles(IEnumerable<string> files, bool apply) => MigrateFilesCore(files, apply, null);

    private static string[] MigrateFilesCore(IEnumerable<string> files, bool apply, Action<string, byte[]> verifyBaseline)
    {
        var comparer = OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;
        var pending = new List<(string Path, byte[] Original, byte[] Migrated)>();
        foreach (var file in files.Select(Path.GetFullPath).Distinct(comparer).OrderBy(p => p, StringComparer.Ordinal))
        {
            try
            {
                if ((File.GetAttributes(file) & FileAttributes.ReparsePoint) != 0)
                    throw new InvalidDataException("迁移不允许符号链接或重解析点");
                var original = File.ReadAllBytes(file);
                var encoding = DetectEncoding(original);
                var preamble = encoding.GetPreamble();
                var offset = original.AsSpan().StartsWith(preamble) ? preamble.Length : 0;
                var source = encoding.GetString(original, offset, original.Length - offset);
                var migrated = Strip(source, file);
                if (migrated == source) continue;
                if (apply) verifyBaseline?.Invoke(file, original);
                var bytes = original.Take(offset).Concat(encoding.GetBytes(migrated)).ToArray();
                pending.Add((file, original, bytes));
            }
            catch (Exception error) when (error is IOException or UnauthorizedAccessException or DecoderFallbackException)
            {
                throw new InvalidDataException("无法校验迁移文件：" + file + ": " + error.Message, error);
            }
        }
        if (!apply) return pending.Select(p => p.Path).ToArray();
        var streams = new List<FileStream>();
        var attempted = -1;
        try
        {
            // 禁止并发写入/替换，并在提交前确认预检查期间源码未发生变化。
            foreach (var item in pending)
            {
                try
                {
                    var stream = File.Open(item.Path, FileMode.Open, FileAccess.ReadWrite, FileShare.Read);
                    streams.Add(stream);
                    using var snapshot = new MemoryStream();
                    stream.CopyTo(snapshot);
                    if (!snapshot.ToArray().AsSpan().SequenceEqual(item.Original))
                        throw new InvalidDataException("预检查后源码发生变化");
                }
                catch (Exception error) when (error is IOException or UnauthorizedAccessException)
                {
                    throw new InvalidDataException("无法锁定迁移文件：" + item.Path + ": " + error.Message, error);
                }
            }
            for (var i = 0; i < pending.Count; i++)
            {
                attempted = i;
                Replace(streams[i], pending[i].Migrated);
            }
        }
        catch (Exception error)
        {
            var failures = new List<Exception>();
            for (var i = attempted; i >= 0; i--)
            {
                try { Replace(streams[i], pending[i].Original); }
                catch (Exception rollback) { failures.Add(new IOException("无法恢复迁移文件：" + pending[i].Path, rollback)); }
            }
            if (failures.Count != 0) throw new AggregateException("迁移失败且部分文件恢复失败", failures.Prepend(error));
            if (attempted >= 0) throw new IOException("迁移写入失败，已恢复原始源码：" + pending[attempted].Path, error);
            throw;
        }
        finally { foreach (var stream in streams) stream.Dispose(); }
        return pending.Select(p => p.Path).ToArray();
    }

    private static void Replace(FileStream stream, byte[] bytes)
    {
        stream.Position = 0;
        stream.Write(bytes);
        stream.SetLength(bytes.Length);
        stream.Flush(true);
    }

    private static Encoding DetectEncoding(byte[] bytes)
    {
        if (bytes.AsSpan().StartsWith(new byte[] { 0xff, 0xfe, 0, 0 })) return new UTF32Encoding(false, true, true);
        if (bytes.AsSpan().StartsWith(new byte[] { 0, 0, 0xfe, 0xff })) return new UTF32Encoding(true, true, true);
        if (bytes.AsSpan().StartsWith(new byte[] { 0xff, 0xfe })) return new UnicodeEncoding(false, true, true);
        if (bytes.AsSpan().StartsWith(new byte[] { 0xfe, 0xff })) return new UnicodeEncoding(true, true, true);
        return new UTF8Encoding(bytes.AsSpan().StartsWith(new byte[] { 0xef, 0xbb, 0xbf }), true);
    }

    public static string Strip(string source, string file)
    {
        var tree = CSharpSyntaxTree.ParseText(source);
        var text = tree.GetText();
        var directives = tree.GetRoot().DescendantTrivia(descendIntoTrivia: true)
            .Select(t => t.GetStructure()).OfType<DirectiveTriviaSyntax>().ToArray();
        var spans = new List<TextSpan>();
        InvalidDataException Invalid() => new("工具生成区块标记不完整或不匹配：" + file);
        string Line(DirectiveTriviaSyntax directive) => text.Lines.GetLineFromPosition(directive.SpanStart).ToString().Trim();

        foreach (var start in directives.OfType<IfDirectiveTriviaSyntax>())
        {
            var name = start.Condition.ToString();
            if (!Names.Contains(name)) continue;
            if (Line(start) != "#if " + name) throw Invalid();
            var related = start.GetRelatedDirectives();
            if (related.LastOrDefault() is not EndIfDirectiveTriviaSyntax end || Line(end) != "#endif//" + name)
                throw Invalid();
            var inside = directives.Where(d => d.SpanStart > start.SpanStart && d.SpanStart < end.SpanStart).ToArray();
            if (inside.FirstOrDefault() is not RegionDirectiveTriviaSyntax region || Line(region) != "#region " + name)
                throw Invalid();
            if (inside.LastOrDefault() is not EndRegionDirectiveTriviaSyntax endRegion || Line(endRegion) != "#endregion//" + name)
                throw Invalid();
            if (region.GetRelatedDirectives().LastOrDefault()?.SpanStart != endRegion.SpanStart)
                throw Invalid();
            var afterIf = text.Lines.GetLineFromPosition(start.SpanStart).EndIncludingLineBreak;
            var beforeRegion = text.Lines.GetLineFromPosition(region.SpanStart).Start;
            var afterRegion = text.Lines.GetLineFromPosition(endRegion.SpanStart).EndIncludingLineBreak;
            var beforeEnd = text.Lines.GetLineFromPosition(end.SpanStart).Start;
            if (!string.IsNullOrWhiteSpace(source[afterIf..beforeRegion]) || !string.IsNullOrWhiteSpace(source[afterRegion..beforeEnd]))
                throw Invalid();
            var begin = text.Lines.GetLineFromPosition(start.SpanStart).Start;
            var finish = text.Lines.GetLineFromPosition(end.SpanStart).EndIncludingLineBreak;
            var span = TextSpan.FromBounds(begin, finish);
            if (spans.Any(s => s.OverlapsWith(span))) throw Invalid();
            spans.Add(span);
        }
        foreach (var directive in directives)
        {
            var line = Line(directive);
            if (Names.Any(name => line == "#endif//" + name || line == "#region " + name || line == "#endregion//" + name)
                && !spans.Any(s => s.Contains(directive.SpanStart))) throw Invalid();
        }
        var trailingBlock = spans.Any(s => string.IsNullOrWhiteSpace(source[s.End..]));
        foreach (var span in spans.OrderByDescending(s => s.Start))
            source = source.Remove(span.Start, span.Length);
        if (trailingBlock)
        {
            // 仅在工具区块位于文件尾时移除遗留的整行分隔空白，保留业务末行及其原始行尾。
            var remaining = SourceText.From(source);
            var end = 0;
            for (var i = remaining.Lines.Count - 1; i >= 0; i--)
            {
                if (string.IsNullOrWhiteSpace(remaining.Lines[i].ToString())) continue;
                end = remaining.Lines[i].EndIncludingLineBreak;
                break;
            }
            source = source[..end];
        }
        return source;
    }
}
