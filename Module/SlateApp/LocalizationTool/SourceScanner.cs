using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace LocalizationTool
{
    // 基于 Roslyn 的源码扫描器：收集字符串字面量 / 逐字 / raw / 插值字符串 / const 引用，
    // 分类、生成改写文本，并跳过已经多国化过的节点（保证重复扫描/改写幂等）。
    public class TtSourceScanner
    {
        readonly string mEngineRoot;
        readonly TtToolConfig mConfig;

        public TtSourceScanner(string engineRoot, TtToolConfig config)
        {
            mEngineRoot = engineRoot.Replace('\\', '/').TrimEnd('/') + "/";
            mConfig = config;
        }

        public TtScanResult Scan()
        {
            var result = new TtScanResult();
            var files = GatherFiles();
            result.ScannedFiles.AddRange(files);

            var trees = new Dictionary<string, SyntaxTree>();
            foreach (var file in files)
            {
                try
                {
                    var text = File.ReadAllText(file);
                    trees[file] = CSharpSyntaxTree.ParseText(text, path: file);
                }
                catch (Exception ex)
                {
                    result.Errors.Add($"Parse failed {file}: {ex.Message}");
                }
            }

            // 语义模型仅用于 const 符号解析，best-effort；失败不影响字面量/插值扫描。
            CSharpCompilation compilation = null;
            try
            {
                compilation = CSharpCompilation.Create("LocScan", trees.Values, GatherReferences());
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Compilation create failed: {ex.Message}");
            }

            foreach (var kv in trees)
            {
                try
                {
                    SemanticModel model = null;
                    try { model = compilation?.GetSemanticModel(kv.Value); }
                    catch { model = null; }
                    ScanTree(kv.Key, kv.Value, model, result);
                }
                catch (Exception ex)
                {
                    result.Errors.Add($"Scan failed {kv.Key}: {ex.Message}");
                }
            }
            return result;
        }

        List<string> GatherFiles()
        {
            var files = new List<string>();
            foreach (var scanRoot in mConfig.ScanRoots)
            {
                var abs = Path.Combine(mEngineRoot, scanRoot);
                if (Directory.Exists(abs) == false)
                    continue;
                foreach (var f in Directory.GetFiles(abs, "*.cs", SearchOption.AllDirectories))
                {
                    var rel = ToRelative(f);
                    if (mConfig.IsExcluded(rel))
                        continue;
                    files.Add(f.Replace('\\', '/'));
                }
            }
            files.Sort(StringComparer.OrdinalIgnoreCase);
            return files;
        }

        static List<MetadataReference> GatherReferences()
        {
            var refs = new List<MetadataReference>();
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    if (asm.IsDynamic)
                        continue;
                    if (string.IsNullOrEmpty(asm.Location))
                        continue;
                    refs.Add(MetadataReference.CreateFromFile(asm.Location));
                }
                catch
                {
                }
            }
            return refs;
        }

        string ToRelative(string absolute)
        {
            var p = absolute.Replace('\\', '/');
            if (p.StartsWith(mEngineRoot, StringComparison.OrdinalIgnoreCase))
                return p.Substring(mEngineRoot.Length);
            return p;
        }

        public static string ComputeFileHash(string absoluteFile)
        {
            using var sha = System.Security.Cryptography.SHA256.Create();
            var bytes = sha.ComputeHash(File.ReadAllBytes(absoluteFile));
            var sb = new StringBuilder();
            foreach (var b in bytes)
                sb.Append(b.ToString("x2"));
            return sb.ToString();
        }

        #region Tree walk
        void ScanTree(string file, SyntaxTree tree, SemanticModel model, TtScanResult result)
        {
            var root = tree.GetCompilationUnitRoot();
            var rel = ToRelative(file);
            var ordinals = new Dictionary<string, int>();

            foreach (var node in root.DescendantNodes())
            {
                if (node is LiteralExpressionSyntax lit && lit.IsKind(SyntaxKind.StringLiteralExpression))
                {
                    // 插值串里的内层字面量作为 Format 参数处理，这里跳过避免重复。
                    if (HasAncestor<InterpolationSyntax>(lit))
                        continue;
                    HandleLiteral(rel, file, lit, model, ordinals, result);
                }
                else if (node is InterpolatedStringExpressionSyntax interp)
                {
                    if (HasAncestor<InterpolationSyntax>(interp))
                        continue;
                    HandleInterpolated(rel, file, interp, model, ordinals, result);
                }
            }
        }

        static bool HasAncestor<T>(SyntaxNode node) where T : SyntaxNode
        {
            return node.Ancestors().OfType<T>().Any();
        }

        void HandleLiteral(string rel, string file, LiteralExpressionSyntax lit, SemanticModel model, Dictionary<string, int> ordinals, TtScanResult result)
        {
            var value = lit.Token.ValueText;
            if (string.IsNullOrEmpty(value) || string.IsNullOrWhiteSpace(value))
                return;

            var occ = NewOccurrence(rel, file, lit, ordinals);
            occ.RawExpressionText = lit.ToString();

            // PG 会直接翻译标准 DisplayName/Description/Category；这里只采集目录键，不改写 Attribute 类型。
            if (GetContainingMetadataAttribute(lit) != null)
            {
                occ.Category = ECandidateCategory.UiMetadata;
                occ.Selected = true;
                occ.SourceKey = value;
                occ.RewriteKind = ERewriteKind.None;
                AddOccurrence(result, occ);
                return;
            }

            ResolveInvocation(lit, occ);
            if (IsLocalizationInvocation(occ.InvocationTarget))
            {
                result.AlreadyLocalizedCount++;
                // 已经包裹的本地化 key 仍参与目录构建，避免源码有 Tr、目录却缺 key。
                // Label 的第二个参数是 ImGui ID，因此只收集所有本地化函数的第一个参数。
                if (occ.ArgumentIndex == 0)
                {
                    occ.Category = ECandidateCategory.UiText;
                    occ.Selected = true;
                    occ.SourceKey = value;
                    occ.RewriteKind = ERewriteKind.None;
                    occ.AlreadyLocalized = true;
                    AddOccurrence(result, occ);
                }
                return;
            }
            if (IsAlreadyLocalized(lit))
            {
                // 字面量位于本地化调用的嵌套表达式中，无法作为稳定 key 单独提取。
                result.AlreadyLocalizedCount++;
                return;
            }

            var isMenuItemProxyMenuName = IsMenuItemProxyMenuNameAssignment(lit, model);
            if (isMenuItemProxyMenuName)
            {
                // 与旧版将 MenuName 视为普通字符串时保存的 Selected=false 状态隔离。
                // 新指纹仍会持久化用户后续对菜单文本的显式选择。
                occ.Fingerprint += "|MenuItemProxy.MenuName";
            }
            bool isUi = IsUiInvocation(occ.InvocationTarget, occ.ArgumentIndex) || isMenuItemProxyMenuName;

            ClassifyValue(value, occ, isUi);
            BuildLiteralRewrite(lit, value, occ);
            AddOccurrence(result, occ);
        }

        void HandleInterpolated(string rel, string file, InterpolatedStringExpressionSyntax interp, SemanticModel model, Dictionary<string, int> ordinals, TtScanResult result)
        {
            if (IsAlreadyLocalized(interp))
            {
                result.AlreadyLocalizedCount++;
                return;
            }

            var occ = NewOccurrence(rel, file, interp, ordinals);
            occ.RawExpressionText = interp.ToString();
            ResolveInvocation(interp, occ);
            bool isUi = IsUiInvocation(occ.InvocationTarget, occ.ArgumentIndex);

            var interpolations = interp.Contents.OfType<InterpolationSyntax>().Any();
            if (interpolations == false)
            {
                // 纯文本插值串 $"abc"，等价普通文本。
                var text = GetInterpolatedPlainText(interp);
                if (string.IsNullOrWhiteSpace(text))
                    return;
                ClassifyValue(text, occ, isUi);
                if (occ.Category == ECandidateCategory.ImGuiId)
                {
                    occ.RewriteKind = ERewriteKind.None;
                }
                else
                {
                    occ.SourceKey = text;
                    occ.RewriteKind = ERewriteKind.Tr;
                    occ.ReplacementText = $"TtLocalization.Tr({EncodeCSharpString(text)})";
                }
                AddOccurrence(result, occ);
                return;
            }

            // 有插值：优先处理 "##" 可见前缀（Label），否则 composite Format。
            if (TryBuildLabelFromInterpolated(interp, occ, isUi))
            {
                AddOccurrence(result, occ);
                return;
            }

            var args = new List<string>();
            var composite = BuildCompositeFormat(interp, args);
            if (composite.Contains("##"))
            {
                // 含 ## 但非干净可见前缀，避免误翻译 ID，标记不可自动改写。
                occ.Category = ECandidateCategory.ImGuiId;
                occ.IsRisk = true;
                occ.RewriteKind = ERewriteKind.None;
                occ.SourceKey = composite;
                occ.Selected = false;
                AddOccurrence(result, occ);
                return;
            }

            occ.Category = isUi ? ECandidateCategory.UiText : ECandidateCategory.Other;
            occ.Selected = isUi;
            occ.SourceKey = composite;
            occ.RewriteKind = ERewriteKind.Format;
            var argList = args.Count > 0 ? ", " + string.Join(", ", args) : "";
            occ.ReplacementText = $"TtLocalization.Format({EncodeCSharpString(composite)}{argList})";
            AddOccurrence(result, occ);
        }

        void AddOccurrence(TtScanResult result, TtStringOccurrence occ)
        {
            if (string.IsNullOrEmpty(occ.SourceKey))
                return;
            result.Occurrences.Add(occ);
        }
        #endregion

        #region Occurrence helpers
        TtStringOccurrence NewOccurrence(string rel, string file, SyntaxNode node, Dictionary<string, int> ordinals)
        {
            var occ = new TtStringOccurrence();
            occ.RelativeFilePath = rel;
            occ.AbsoluteFilePath = file;
            occ.SpanStart = node.SpanStart;
            occ.SpanLength = node.Span.Length;
            occ.Line = node.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
            occ.TypeName = GetContainingTypeName(node);
            occ.MemberName = GetContainingMemberName(node);

            var key = occ.TypeName + "::" + occ.MemberName;
            ordinals.TryGetValue(key, out var ord);
            occ.MemberOrdinal = ord;
            ordinals[key] = ord + 1;

            var normalized = node.ToString();
            occ.Fingerprint = $"{rel}|{occ.TypeName}|{occ.MemberName}|{normalized}|{occ.MemberOrdinal}";
            return occ;
        }

        static string GetContainingTypeName(SyntaxNode node)
        {
            var type = node.Ancestors().OfType<BaseTypeDeclarationSyntax>().FirstOrDefault();
            return type?.Identifier.Text ?? "";
        }

        static string GetContainingMemberName(SyntaxNode node)
        {
            foreach (var a in node.Ancestors())
            {
                switch (a)
                {
                    case MethodDeclarationSyntax m: return m.Identifier.Text;
                    case PropertyDeclarationSyntax p: return p.Identifier.Text;
                    case ConstructorDeclarationSyntax c: return c.Identifier.Text + ".ctor";
                    case FieldDeclarationSyntax f:
                        return f.Declaration.Variables.Count > 0 ? f.Declaration.Variables[0].Identifier.Text : "field";
                    case EventDeclarationSyntax e: return e.Identifier.Text;
                }
            }
            return "";
        }

        void ResolveInvocation(SyntaxNode node, TtStringOccurrence occ)
        {
            foreach (var a in node.Ancestors())
            {
                if (a is ArgumentSyntax argSyntax && argSyntax.Parent is ArgumentListSyntax argList &&
                    argList.Parent is InvocationExpressionSyntax invocation)
                {
                    occ.InvocationTarget = GetTargetText(invocation.Expression);
                    occ.ArgumentIndex = argList.Arguments.IndexOf(argSyntax);
                    return;
                }
                // 遇到语句边界即停止向上找。
                if (a is StatementSyntax || a is MemberDeclarationSyntax)
                    return;
            }
        }

        static string GetTargetText(ExpressionSyntax expr)
        {
            switch (expr)
            {
                case MemberAccessExpressionSyntax ma:
                    {
                        // 取最后两段，如 ImGuiAPI.Button / DockProxy.BeginMainForm。
                        var name = ma.Name.Identifier.Text;
                        if (ma.Expression is MemberAccessExpressionSyntax inner)
                            return inner.Name.Identifier.Text + "." + name;
                        if (ma.Expression is IdentifierNameSyntax id)
                            return id.Identifier.Text + "." + name;
                        return name;
                    }
                case IdentifierNameSyntax idn:
                    return idn.Identifier.Text;
                default:
                    return expr.ToString();
            }
        }

        static bool IsVisibleMenuArgument(string method, int argumentIndex)
        {
            switch (method)
            {
                // 菜单 API 的其它 string 参数通常是 shortcut、filter 或内部数据，不应进入翻译目录。
                case "MenuItem":
                case "BeginMenu":
                case "BeginMenuItem":
                case "AddMenuItem":
                case "AddMenuDraw":
                case "AddMenuSeparator":
                case "BeginMainForm":
                case "BeginPanel":
                    return argumentIndex == 0;
                case "InsertMenuItem":
                    return argumentIndex == 1;
                default:
                    return true;
            }
        }

        bool IsUiInvocation(string target, int argumentIndex)
        {
            if (string.IsNullOrEmpty(target))
                return false;
            var method = target.Contains('.') ? target.Substring(target.LastIndexOf('.') + 1) : target;
            if (IsVisibleMenuArgument(method, argumentIndex) == false)
                return false;
            foreach (var q in mConfig.UiTextQualified)
            {
                // GetTargetText 规范化为末两段（如 DockProxy.BeginMainForm），
                // 同时兼容旧配置中的完整限定名 UIProxy.DockProxy.BeginMainForm。
                if (target.EndsWith(q, StringComparison.Ordinal) || q.EndsWith(target, StringComparison.Ordinal))
                    return true;
            }
            return mConfig.UiTextMethods.Contains(method);
        }

        // MenuItemProxy 最终以变量 MenuName 调用 ImGui 菜单 API，调用点无法回溯到初始化字面量；
        // 因此将该成员的字符串赋值直接视为可见 UI 文本。
        static bool IsMenuItemProxyMenuNameAssignment(SyntaxNode node, SemanticModel model)
        {
            if (node.Parent is not AssignmentExpressionSyntax assignment ||
                assignment.IsKind(SyntaxKind.SimpleAssignmentExpression) == false ||
                assignment.Right != node)
                return false;

            var memberName = assignment.Left switch
            {
                IdentifierNameSyntax id => id.Identifier.Text,
                MemberAccessExpressionSyntax member => member.Name.Identifier.Text,
                _ => string.Empty,
            };
            if (string.Equals(memberName, "MenuName", StringComparison.Ordinal) == false)
                return false;

            var symbol = model?.GetSymbolInfo(assignment.Left).Symbol;
            if (symbol?.ContainingType?.Name == "MenuItemProxy")
                return true;

            // 语义模型可能因工程条件编译或缺少引用而无法解析，保留对象初始化器语法回退。
            if (assignment.Left is IdentifierNameSyntax &&
                assignment.Parent is InitializerExpressionSyntax initializer &&
                initializer.Parent is ObjectCreationExpressionSyntax creation)
            {
                var typeName = creation.Type.ToString();
                return typeName == "MenuItemProxy" || typeName.EndsWith(".MenuItemProxy", StringComparison.Ordinal);
            }
            return false;
        }
        #endregion

        #region Already-localized detection
        static readonly string[] LocalizationMethods = { "Tr", "Format", "Label" };

        static bool IsLocalizationInvocation(string target)
        {
            if (string.IsNullOrEmpty(target) || target.Contains("TtLocalization") == false)
                return false;
            var method = target.Contains('.') ? target.Substring(target.LastIndexOf('.') + 1) : target;
            return LocalizationMethods.Contains(method);
        }

        static bool IsAlreadyLocalized(SyntaxNode node)
        {
            foreach (var a in node.Ancestors())
            {
                if (a is InvocationExpressionSyntax inv)
                {
                    var target = GetTargetText(inv.Expression);
                    var method = target.Contains('.') ? target.Substring(target.LastIndexOf('.') + 1) : target;
                    if (target.Contains("TtLocalization") && LocalizationMethods.Contains(method))
                        return true;
                }
            }
            return false;
        }
        #endregion

        #region Attribute handling
        string GetContainingMetadataAttribute(SyntaxNode node)
        {
            var attr = node.Ancestors().OfType<AttributeSyntax>().FirstOrDefault();
            if (attr == null)
                return null;
            var name = GetAttributeName(attr);
            if (name == null)
                return null;
            var shortName = name.Contains('.') ? name.Substring(name.LastIndexOf('.') + 1) : name;
            if (shortName.EndsWith("Attribute", StringComparison.Ordinal))
                shortName = shortName.Substring(0, shortName.Length - "Attribute".Length);
            return mConfig.UiMetadataAttributes.Contains(shortName) ? shortName : null;
        }

        static string GetAttributeName(AttributeSyntax attr)
        {
            switch (attr.Name)
            {
                case IdentifierNameSyntax id: return id.Identifier.Text;
                case QualifiedNameSyntax q: return q.ToString();
                default: return attr.Name.ToString();
            }
        }

        #endregion

        #region Classification
        void ClassifyValue(string value, TtStringOccurrence occ, bool isUi)
        {
            if (isUi)
            {
                occ.Category = ECandidateCategory.UiText;
                occ.Selected = true;
                return;
            }

            occ.Selected = false;
            if (value.StartsWith("##", StringComparison.Ordinal))
            {
                occ.Category = ECandidateCategory.ImGuiId;
                occ.IsRisk = true;
            }
            else if (LooksLikeUrl(value))
            {
                occ.Category = ECandidateCategory.Url;
                occ.IsRisk = true;
            }
            else if (LooksLikeGuid(value))
            {
                occ.Category = ECandidateCategory.Guid;
                occ.IsRisk = true;
            }
            else if (LooksLikeTypeName(value))
            {
                occ.Category = ECandidateCategory.TypeName;
                occ.IsRisk = true;
            }
            else if (LooksLikeShader(value))
            {
                occ.Category = ECandidateCategory.Shader;
                occ.IsRisk = true;
            }
            else if (LooksLikePath(value))
            {
                occ.Category = ECandidateCategory.Path;
                occ.IsRisk = true;
            }
            else if (LooksLikeSerializationKey(value))
            {
                occ.Category = ECandidateCategory.SerializationKey;
                occ.IsRisk = true;
            }
            else
            {
                occ.Category = ECandidateCategory.Other;
            }
        }

        static bool LooksLikeUrl(string v) => v.Contains("://");
        static bool LooksLikeGuid(string v) => Guid.TryParse(v, out _);
        static bool LooksLikeTypeName(string v) => v.Contains('@') || (v.Contains('.') && v.Contains(' ') == false && v.Any(char.IsUpper) && v.Length > 6 && v.EndsWith(".") == false && v.Any(c => c == '/') == false && v.Split('.').All(seg => seg.Length > 0 && (char.IsLetter(seg[0]) || seg[0] == '_')));
        static bool LooksLikeShader(string v)
        {
            var lower = v.ToLowerInvariant();
            return lower.EndsWith(".cginc") || lower.EndsWith(".hlsl") || lower.EndsWith(".compute") ||
                   lower.Contains("select ") || lower.Contains("cbuffer");
        }
        static bool LooksLikePath(string v)
        {
            if (v.Contains('/') || v.Contains('\\'))
                return true;
            var lower = v.ToLowerInvariant();
            string[] exts = { ".material", ".uminst", ".vms", ".ums", ".srv", ".scene", ".prefab", ".macross", ".ui", ".animclip", ".uvanim", ".otf", ".ttf", ".rpolicy", ".png", ".dds", ".json", ".xml", ".txt", ".dll", ".ini", ".cs", ".dat" };
            return exts.Any(e => lower.EndsWith(e));
        }
        static bool LooksLikeSerializationKey(string v)
        {
            // 无空格的短标识符串，多半是键/枚举名/内部标记。
            return v.Length <= 24 && v.Contains(' ') == false && v.All(c => char.IsLetterOrDigit(c) || c == '_');
        }
        #endregion

        #region Literal rewrite (Tr / Label)
        void BuildLiteralRewrite(LiteralExpressionSyntax lit, string value, TtStringOccurrence occ)
        {
            var idx = value.IndexOf("##", StringComparison.Ordinal);
            if (idx >= 0)
            {
                var visible = value.Substring(0, idx);
                var suffix = value.Substring(idx); // 含 ## 前缀
                if (string.IsNullOrEmpty(visible))
                {
                    // 纯 ID，无可见文字。
                    occ.Category = ECandidateCategory.ImGuiId;
                    occ.IsRisk = true;
                    occ.RewriteKind = ERewriteKind.None;
                    occ.Selected = false;
                    occ.SourceKey = value;
                    return;
                }
                occ.RewriteKind = ERewriteKind.Label;
                occ.SourceKey = visible;
                occ.ReplacementText = $"TtLocalization.Label({EncodeCSharpString(visible)}, {EncodeCSharpString(suffix)})";
                return;
            }

            var invocationMethod = occ.InvocationTarget?.Contains('.') == true ?
                occ.InvocationTarget.Substring(occ.InvocationTarget.LastIndexOf('.') + 1) : occ.InvocationTarget;
            if (occ.ArgumentIndex == 0 && (invocationMethod == "BeginMainForm" || invocationMethod == "BeginPanel"))
            {
                // Dock 标题参与 ImGui 窗口身份计算；翻译可见标题时保留原文作为稳定 ID。
                occ.RewriteKind = ERewriteKind.Label;
                occ.SourceKey = value;
                occ.ReplacementText = $"TtLocalization.Label({lit.ToString()}, {EncodeCSharpString("###" + value)})";
                return;
            }

            occ.RewriteKind = ERewriteKind.Tr;
            occ.SourceKey = value;
            // 复用原始字面量文本，保留其转义/逐字/raw 形式。
            occ.ReplacementText = $"TtLocalization.Tr({lit.ToString()})";
        }
        #endregion

        #region Interpolated helpers
        static string GetInterpolatedPlainText(InterpolatedStringExpressionSyntax interp)
        {
            var sb = new StringBuilder();
            foreach (var content in interp.Contents)
            {
                if (content is InterpolatedStringTextSyntax t)
                    sb.Append(t.TextToken.ValueText);
            }
            return sb.ToString();
        }

        // composite format：文本部分转义花括号，插值转成 {index[,align][:format]}。
        static string BuildCompositeFormat(InterpolatedStringExpressionSyntax interp, List<string> args)
        {
            var sb = new StringBuilder();
            int index = 0;
            foreach (var content in interp.Contents)
            {
                if (content is InterpolatedStringTextSyntax t)
                {
                    sb.Append(t.TextToken.ValueText.Replace("{", "{{").Replace("}", "}}"));
                }
                else if (content is InterpolationSyntax it)
                {
                    sb.Append('{').Append(index);
                    if (it.AlignmentClause != null)
                        sb.Append(it.AlignmentClause.ToString());
                    if (it.FormatClause != null)
                        sb.Append(it.FormatClause.ToString());
                    sb.Append('}');
                    args.Add(it.Expression.ToString());
                    index++;
                }
            }
            return sb.ToString();
        }

        // 仅处理 "##" 出现在第一个纯文本段、且其前无任何插值的干净场景 -> Label(visible, $"##...")。
        bool TryBuildLabelFromInterpolated(InterpolatedStringExpressionSyntax interp, TtStringOccurrence occ, bool isUi)
        {
            var contents = interp.Contents.ToList();
            int splitContent = -1;
            int splitPos = -1;
            for (int i = 0; i < contents.Count; i++)
            {
                if (contents[i] is InterpolationSyntax)
                {
                    // ## 之前出现插值，可见前缀非常量，放弃 Label 方案。
                    if (splitContent < 0)
                        return false;
                    break;
                }
                if (contents[i] is InterpolatedStringTextSyntax t)
                {
                    var p = t.TextToken.ValueText.IndexOf("##", StringComparison.Ordinal);
                    if (p >= 0)
                    {
                        splitContent = i;
                        splitPos = p;
                        break;
                    }
                }
            }
            if (splitContent < 0)
                return false;

            // 组装可见前缀（全部为常量文本）。
            var visible = new StringBuilder();
            for (int i = 0; i < splitContent; i++)
            {
                if (contents[i] is InterpolatedStringTextSyntax tt)
                    visible.Append(tt.TextToken.ValueText);
            }
            var splitText = ((InterpolatedStringTextSyntax)contents[splitContent]).TextToken.ValueText;
            visible.Append(splitText.Substring(0, splitPos));

            if (visible.Length == 0)
            {
                occ.Category = ECandidateCategory.ImGuiId;
                occ.IsRisk = true;
                occ.RewriteKind = ERewriteKind.None;
                occ.SourceKey = "##";
                occ.Selected = false;
                return true;
            }

            // 重建后缀插值串 $"##...{expr}..."。
            var suffix = new StringBuilder("$\"");
            suffix.Append(EscapeInterpolatedText(splitText.Substring(splitPos)));
            for (int i = splitContent + 1; i < contents.Count; i++)
            {
                if (contents[i] is InterpolatedStringTextSyntax tt)
                    suffix.Append(EscapeInterpolatedText(tt.TextToken.ValueText));
                else if (contents[i] is InterpolationSyntax it)
                    suffix.Append(it.ToString());
            }
            suffix.Append('"');

            occ.Category = isUi ? ECandidateCategory.UiText : ECandidateCategory.Other;
            occ.Selected = isUi;
            occ.RewriteKind = ERewriteKind.Label;
            occ.SourceKey = visible.ToString();
            occ.ReplacementText = $"TtLocalization.Label({EncodeCSharpString(visible.ToString())}, {suffix})";
            return true;
        }

        static string EscapeInterpolatedText(string s)
        {
            var sb = new StringBuilder();
            foreach (var c in s)
            {
                switch (c)
                {
                    case '\\': sb.Append("\\\\"); break;
                    case '"': sb.Append("\\\""); break;
                    case '{': sb.Append("{{"); break;
                    case '}': sb.Append("}}"); break;
                    case '\n': sb.Append("\\n"); break;
                    case '\r': sb.Append("\\r"); break;
                    case '\t': sb.Append("\\t"); break;
                    default: sb.Append(c); break;
                }
            }
            return sb.ToString();
        }
        #endregion

        #region Encoding
        // 生成常规（非逐字）C# 字符串字面量，保留非 ASCII 原样。
        public static string EncodeCSharpString(string s)
        {
            var sb = new StringBuilder("\"");
            foreach (var c in s)
            {
                switch (c)
                {
                    case '\\': sb.Append("\\\\"); break;
                    case '"': sb.Append("\\\""); break;
                    case '\n': sb.Append("\\n"); break;
                    case '\r': sb.Append("\\r"); break;
                    case '\t': sb.Append("\\t"); break;
                    case '\0': sb.Append("\\0"); break;
                    default:
                        if (char.IsControl(c))
                            sb.Append("\\u").Append(((int)c).ToString("x4"));
                        else
                            sb.Append(c);
                        break;
                }
            }
            sb.Append('"');
            return sb.ToString();
        }
        #endregion
    }
}
