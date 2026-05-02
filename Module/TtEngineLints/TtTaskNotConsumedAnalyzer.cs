using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace EngineNS.Lints
{
    /// <summary>
    /// TT2201: 检测 TtTask / TtTask&lt;T&gt; 返回值未被消费的 fire-and-forget 调用。
    /// 对应 documents/coding/CodingGuidelines.md § 2.2 反例 1。
    ///
    /// 触发条件：一条 ExpressionStatement 的最外层表达式返回 TtTask / TtTask&lt;T&gt;，
    /// 但既没有被 await，也没有被赋值给变量/字段，也没有立即调用消费方法
    /// (AddWaitTask / WaitCompletedAndDispose / GetResultUntilCompleted /
    ///  GetResultAndRelease / WaitCompleted)。
    ///
    /// 例如:
    ///   _ = WorkerLoop();              // ❌ 报错
    ///   WorkerLoop();                  // ❌ 报错
    ///   await WorkerLoop();            // ✓ ok (await 消费了)
    ///   WorkerLoop().AddWaitTask();    // ✓ ok (链式消费了)
    ///   var t = WorkerLoop();          // ✓ ok (赋给了变量, 留给后续逻辑处理)
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class TtTaskNotConsumedAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "TT2201";

        private const string Category = "TitanEngine.Async";
        private const string Title = "TtTask 返回值必须被消费";
        private const string MessageFormat = "TtTask{0} 返回值未被消费 (await / AddWaitTask / WaitCompletedAndDispose / GetResultUntilCompleted 等), 会导致 TtTaskData 对象池泄漏。详见 CodingGuidelines.md § 2.2";
        private const string Description =
            "EngineNS.Thread.Async.TtTask / TtTask<T> 返回值必须通过 await 或 .AddWaitTask() / .WaitCompletedAndDispose() / .GetResultUntilCompleted() 等显式消费, 否则 TtTaskData 无法归还对象池, 会导致池失效 + 异常被吞.";
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
                    return; // 当前编译单元没有引入 TtTask, 跳过

                startCtx.RegisterSyntaxNodeAction(
                    ctx => AnalyzeExpressionStatement(ctx, ttTaskType, ttTaskGenericType),
                    SyntaxKind.ExpressionStatement);
            });
        }

        private static void AnalyzeExpressionStatement(
            SyntaxNodeAnalysisContext context,
            INamedTypeSymbol? ttTaskType,
            INamedTypeSymbol? ttTaskGenericType)
        {
            var stmt = (ExpressionStatementSyntax)context.Node;
            var expr = stmt.Expression;

            // 1) 处理 _ = XxxAsync() 这种 discard 赋值
            if (expr is AssignmentExpressionSyntax assign &&
                assign.Kind() == SyntaxKind.SimpleAssignmentExpression &&
                assign.Left is IdentifierNameSyntax leftId &&
                leftId.Identifier.ValueText == "_")
            {
                ReportIfTtTask(context, assign.Right, ttTaskType, ttTaskGenericType);
                return;
            }

            // 2) 跳过其他形态的赋值 (留给变量定义自己负责消费)
            if (expr is AssignmentExpressionSyntax)
                return;

            // 3) 跳过 await 表达式 (await 已经消费了)
            if (expr is AwaitExpressionSyntax)
                return;

            // 4) 普通方法调用 / 成员访问作为语句: 形如 XxxAsync();
            ReportIfTtTask(context, expr, ttTaskType, ttTaskGenericType);
        }

        private static void ReportIfTtTask(
            SyntaxNodeAnalysisContext context,
            ExpressionSyntax expr,
            INamedTypeSymbol? ttTaskType,
            INamedTypeSymbol? ttTaskGenericType)
        {
            // 只关心调用表达式 (Foo()) 或条件链式调用 (a?.Foo())
            if (expr is not InvocationExpressionSyntax)
                return;

            var typeInfo = context.SemanticModel.GetTypeInfo(expr, context.CancellationToken);
            var type = typeInfo.Type;
            if (type == null)
                return;

            // 链式消费: 如 WorkerLoop().AddWaitTask() 这样的写法,
            // 最外层 InvocationExpression 的 type 通常是 void (AddWaitTask 返回 void),
            // 所以走不到这里。我们只关心"最外层调用本身就返回 TtTask"的情况。
            if (!IsTtTaskType(type, ttTaskType, ttTaskGenericType))
                return;

            // 进一步: 如果整条表达式语句的最外层 invocation 调用的就是 AddWaitTask /
            // WaitCompletedAndDispose / GetResultAndRelease / GetResultUntilCompleted /
            // WaitCompleted, 说明这是引擎自己的消费 API, 不应误报
            // (它们的返回值类型也是 TtTask 时不会出现, 但稳妥起见)
            if (IsKnownConsumerCall(expr))
                return;

            var typeDisplay = type is INamedTypeSymbol named && named.IsGenericType
                ? "<" + named.TypeArguments[0].ToDisplayString() + ">"
                : "";

            var diag = Diagnostic.Create(Rule, expr.GetLocation(), typeDisplay);
            context.ReportDiagnostic(diag);
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

        /// <summary>
        /// 当语句本身就是 xxx.AddWaitTask() / xxx.WaitCompletedAndDispose() 等消费 API 时,
        /// 整条语句不应该被报错 (只是它们碰巧也返回 TtTask 时才会触发, 通常返回 void)。
        /// </summary>
        private static bool IsKnownConsumerCall(ExpressionSyntax expr)
        {
            if (expr is not InvocationExpressionSyntax inv)
                return false;
            if (inv.Expression is not MemberAccessExpressionSyntax member)
                return false;

            var name = member.Name.Identifier.ValueText;
            return name is "AddWaitTask"
                or "WaitCompleted"
                or "WaitCompletedAndDispose"
                or "GetResultUntilCompleted"
                or "GetResultAndRelease";
        }
    }
}
