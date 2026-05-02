using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace EngineNS.Lints
{
    /// <summary>
    /// TT2202: 在 AddWaitTask 回调 (lambda / 匿名方法) 内部禁止调用
    /// GetResultAndRelease() / WaitCompletedAndDispose()。
    /// 对应 documents/coding/CodingGuidelines.md § 2.2 反例 2。
    ///
    /// 原因: TtTaskCollector.Tick 在回调返回后会自动 Dispose 任务,
    /// 回调里再 Dispose (GetResultAndRelease 内部 = GetAwaiter().GetResult() = Dispose) 会
    /// 双重释放 TtTaskData, 把对象池搞乱。
    ///
    /// 正确写法: 在回调内部用 DirectResult 只读结果, 不再 Dispose。
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class AddWaitTaskCallbackAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "TT2202";

        private const string Category = "TitanEngine.Async";
        private const string Title = "AddWaitTask 回调里禁止再次 Dispose 任务";
        private const string MessageFormat = "AddWaitTask 回调内部不能调用 '{0}', 会双重释放 TtTaskData (Tick 之后会自动 Dispose)。请改用 DirectResult 只读结果。详见 CodingGuidelines.md § 2.2";
        private const string Description =
            "在 AddWaitTask((task) => { ... }) 的 lambda 内部禁止调用 GetResultAndRelease / WaitCompletedAndDispose, TtTaskCollector.Tick 已经会在回调返回后自动 Dispose, 再次 Dispose 会污染对象池.";
        private const string HelpLink = "documents/coding/CodingGuidelines.md#22";

        private const string TtTaskMetadataName = "EngineNS.Thread.Async.TtTask";
        private const string TtTaskGenericMetadataName = "EngineNS.Thread.Async.TtTask`1";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: Title,
            messageFormat: MessageFormat,
            category: Category,
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: Description,
            helpLinkUri: HelpLink);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterCompilationStartAction(startCtx =>
            {
                var ttTaskType = startCtx.Compilation.GetTypeByMetadataName(TtTaskMetadataName);
                var ttTaskGenericType = startCtx.Compilation.GetTypeByMetadataName(TtTaskGenericMetadataName);
                if (ttTaskType == null && ttTaskGenericType == null)
                    return;

                startCtx.RegisterSyntaxNodeAction(
                    ctx => AnalyzeInvocation(ctx, ttTaskType, ttTaskGenericType),
                    SyntaxKind.InvocationExpression);
            });
        }

        private static readonly System.Collections.Generic.HashSet<string> ForbiddenMethods =
            new System.Collections.Generic.HashSet<string>
            {
                "GetResultAndRelease",
                "WaitCompletedAndDispose",
            };

        private static void AnalyzeInvocation(
            SyntaxNodeAnalysisContext context,
            INamedTypeSymbol? ttTaskType,
            INamedTypeSymbol? ttTaskGenericType)
        {
            var inv = (InvocationExpressionSyntax)context.Node;

            // 1) 必须是 xxx.MethodName() 形态
            if (inv.Expression is not MemberAccessExpressionSyntax member)
                return;

            var methodName = member.Name.Identifier.ValueText;
            if (!ForbiddenMethods.Contains(methodName))
                return;

            // 2) 必须是 TtTask / TtTask<T> 上的成员调用 (避免命中其他类型的同名方法)
            var receiverType = context.SemanticModel.GetTypeInfo(member.Expression, context.CancellationToken).Type;
            if (receiverType == null)
                return;
            if (!IsTtTaskType(receiverType, ttTaskType, ttTaskGenericType))
                return;

            // 3) 向上找最近的 lambda / 匿名方法; 必须存在, 否则不在回调里
            var enclosingLambda = FindEnclosingLambda(inv);
            if (enclosingLambda == null)
                return;

            // 4) 该 lambda 必须正好作为 AddWaitTask 调用的实参
            if (!IsArgumentOfAddWaitTask(enclosingLambda))
                return;

            var diag = Diagnostic.Create(Rule, inv.GetLocation(), methodName);
            context.ReportDiagnostic(diag);
        }

        private static SyntaxNode? FindEnclosingLambda(SyntaxNode node)
        {
            for (var cur = node.Parent; cur != null; cur = cur.Parent)
            {
                switch (cur)
                {
                    case SimpleLambdaExpressionSyntax:
                    case ParenthesizedLambdaExpressionSyntax:
                    case AnonymousMethodExpressionSyntax:
                        return cur;
                    case MethodDeclarationSyntax:
                    case LocalFunctionStatementSyntax:
                        // 撞到方法体边界仍未找到 lambda, 说明不在 AddWaitTask 回调里
                        return null;
                }
            }
            return null;
        }

        private static bool IsArgumentOfAddWaitTask(SyntaxNode lambda)
        {
            // lambda -> Argument -> ArgumentList -> InvocationExpression
            // 中间可能套一层 CastExpression / ParenthesizedExpression, 也兼容
            var cur = lambda.Parent;
            while (cur is CastExpressionSyntax or ParenthesizedExpressionSyntax)
                cur = cur.Parent;

            if (cur is not ArgumentSyntax arg)
                return false;
            if (arg.Parent is not ArgumentListSyntax argList)
                return false;
            if (argList.Parent is not InvocationExpressionSyntax inv)
                return false;
            if (inv.Expression is not MemberAccessExpressionSyntax member)
                return false;

            return member.Name.Identifier.ValueText == "AddWaitTask";
        }

        private static bool IsTtTaskType(
            ITypeSymbol type,
            INamedTypeSymbol? ttTaskType,
            INamedTypeSymbol? ttTaskGenericType)
        {
            if (type is not INamedTypeSymbol named)
                return false;

            if (ttTaskType != null && SymbolEqualityComparer.Default.Equals(named, ttTaskType))
                return true;

            if (ttTaskGenericType != null &&
                named.IsGenericType &&
                SymbolEqualityComparer.Default.Equals(named.OriginalDefinition, ttTaskGenericType))
                return true;

            return false;
        }
    }
}
