using System.Collections.Generic;

namespace LocalizationTool
{
    // 候选字符串的改写方式。
    public enum ERewriteKind
    {
        None,            // 不可自动改写（仅展示，供人工处理）
        Tr,              // TtLocalization.Tr("...")
        Format,          // TtLocalization.Format("... {0} ...", args)
        Label,           // TtLocalization.Label("visible", "##id" 或 $"##{id}")
        ConstReference,  // TtLocalization.Tr(ConstName)
    }

    // 候选类别，用于分组、默认勾选与风险提示。
    public enum ECandidateCategory
    {
        UiText,             // 传入已知 UI 函数的可见文本（默认勾选）
        UiMetadata,         // DisplayName/Description/Category 等 UI 元数据（默认勾选）
        ConstDeclaration,   // const string 声明处
        Path,               // 资产/文件路径
        Url,                // URL
        Guid,               // GUID 字面量
        Log,                // 日志/断言文本
        SerializationKey,   // 序列化/XML/JSON 键
        Shader,             // shader / cginc / SQL 等
        TypeName,           // 类型全名
        ImGuiId,            // 纯 ImGui ## ID（无可见文字）
        Other,              // 其它普通字符串
    }

    // 单个字符串出现位置（occurrence）。改写与勾选以 occurrence 为粒度，展示时按 SourceKey 聚合。
    public class TtStringOccurrence
    {
        public string RelativeFilePath;
        public string AbsoluteFilePath;
        public string TypeName = "";
        public string MemberName = "";
        public int MemberOrdinal;           // 同一成员内规范化表达式的序号
        public int Line;

        public string RawExpressionText;    // 源码中字符串表达式的原始文本
        public string SourceKey;            // 作为翻译键的规范化原文（Label 取可见前缀，Format 取 composite 格式串）
        public string InvocationTarget;     // 最近的调用目标，如 "ImGuiAPI.Button"
        public int ArgumentIndex = -1;

        public ERewriteKind RewriteKind = ERewriteKind.None;
        public ECandidateCategory Category = ECandidateCategory.Other;
        public bool IsRisk;
        public bool AlreadyLocalized;

        // 改写位置（相对当前源码文本）与预先生成的替换文本。
        public int SpanStart;
        public int SpanLength;
        public string ReplacementText;

        // 用户状态（持久化到 scan-state）。
        public bool Selected;
        public bool Ignored;

        // 稳定指纹：相对路径 + 类型 + 成员 + 规范化表达式 + 成员内序号。
        public string Fingerprint;

        public bool CanRewrite => RewriteKind != ERewriteKind.None && string.IsNullOrEmpty(ReplacementText) == false;
    }

    // 按 SourceKey 聚合的翻译条目。
    public class TtTranslationEntry
    {
        public string Source;
        public int PlaceholderCount;
        public bool IsFormat;
        public List<TtStringOccurrence> Occurrences = new List<TtStringOccurrence>();
        // culture -> 译文。
        public Dictionary<string, string> Translations = new Dictionary<string, string>();
        // 源码中已不存在但目录里仍有译文时标记为 obsolete。
        public bool Obsolete;
    }

    // 一次扫描的结果。
    public class TtScanResult
    {
        public List<TtStringOccurrence> Occurrences = new List<TtStringOccurrence>();
        public List<string> ScannedFiles = new List<string>();
        public List<string> Errors = new List<string>();
        public int AlreadyLocalizedCount;
    }
}
