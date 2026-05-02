using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace EngineNS.Lints
{
    /// <summary>
    /// TT2203: 局部 TtSemaphore 变量必须显式 FreeSemaphore (或在所有控制流分支上释放)。
    /// 对应 documents/coding/CodingGuidelines.md § 2.2 反例 3。
    ///
    /// 检测范围 (避免误报):
    /// - 仅检查通过 TtSemaphore.CreateSemaphore(...) 创建并赋给 *局部变量* 的 semaphore。
    /// - 字段、属性、参数、被赋给字段/对象成员、被作为参数传出去/作为返回值的 semaphore
    ///   都跳过 (生命周期不归本函数管)。
    ///
    /// 触发条件: 局部变量在所属方法体内, 任何控制流路径上都没有出现过对它的
    /// .FreeSemaphore() 调用 → 报警告 (Warning, 非 Error, 因为存在合法的"调用方继续持有"模式)。
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class SemaphoreFreeAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "TT2203";

        private const string Category = "TitanEngine.Async";
        private const string Title = "局部 TtSemaphore 应显式 FreeSemaphore";
        private const string MessageFormat = "局部 TtSemaphore '{0}' 在本方法内未发现 FreeSemaphore() 调用, 可能造成 PostEvent / Waiter 资源残留。详见 CodingGuidelines.md § 2.2";
        private const string Description =
            "通过 TtSemaphore.CreateSemaphore 创建的局部 semaphore 在使用完毕后应调用 FreeSemaphore() 释放, 否则其内部 PostEvent / Waiter 引用会残留, 长期累积造成内存泄漏.";
        private const string HelpLink = "documents/coding/CodingGuidelines.md#22";

        private const string SemaphoreMetadataName = "EngineNS.Thread.TtSemaphore";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            id: DiagnosticId,
            title: Title,
            messageFormat: MessageFormat,
            category: Category,
            defaultSeverity: DiagnosticSeverity.Warning,
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
                var semaphoreType = startCtx.Compilation.GetTypeByMetadataName(SemaphoreMetadataName);
                if (semaphoreType == null)
                    return;

                startCtx.RegisterSyntaxNodeAction(
                    ctx => AnalyzeMethodBody(ctx, semaphoreType),
                    SyntaxKind.MethodDeclaration,
                    SyntaxKind.LocalFunctionStatement,
                    SyntaxKind.AnonymousMethodExpression,
                    SyntaxKind.SimpleLambdaExpression,
                    SyntaxKind.ParenthesizedLambdaExpression);
            });
        }

        private static void AnalyzeMethodBody(
            SyntaxNodeAnalysisContext context,
            INamedTypeSymbol semaphoreType)
        {
            // 提取该函数体范围内所有 LocalDeclarationStatement
            var body = GetBody(context.Node);
            if (body == null)
                return;

            var locals = body.DescendantNodes()
                .OfType<LocalDeclarationStatementSyntax>()
                .ToList();
            if (locals.Count == 0)
                return;

            foreach (var local in locals)
            {
                foreach (var declarator in local.Declaration.Variables)
                {
                    if (declarator.Initializer == null)
                        continue;

                    if (!IsSemaphoreCreate(declarator.Initializer.Value, semaphoreType, context))
                        continue;

                    // 拿到本地符号
                    var localSymbol = context.SemanticModel.GetDeclaredSymbol(declarator, context.CancellationToken) as ILocalSymbol;
                    if (localSymbol == null)
                        continue;

                    // 排除"赋出"场景: 该局部被赋给字段、被作为返回值、被作为参数传出
                    if (IsOwnershipTransferred(body, localSymbol, context))
                        continue;

                    // 检查方法体内是否存在对该局部的 .FreeSemaphore() 调用
                    if (HasFreeSemaphoreCall(body, localSymbol, context))
                        continue;

                    var diag = Diagnostic.Create(
                        Rule,
                        declarator.Identifier.GetLocation(),
                        localSymbol.Name);
                    context.ReportDiagnostic(diag);
                }
            }
        }

        private static SyntaxNode? GetBody(SyntaxNode node) => node switch
        {
            MethodDeclarationSyntax m => (SyntaxNode?)m.Body ?? m.ExpressionBody,
            LocalFunctionStatementSyntax lf => (SyntaxNode?)lf.Body ?? lf.ExpressionBody,
            AnonymousMethodExpressionSyntax am => am.Body,
            SimpleLambdaExpressionSyntax sl => sl.Body,
            ParenthesizedLambdaExpressionSyntax pl => pl.Body,
            _ => null,
        };

        private static bool IsSemaphoreCreate(
            ExpressionSyntax initializer,
            INamedTypeSymbol semaphoreType,
            SyntaxNodeAnalysisContext context)
        {
            // 必须是 invocation 形态 (TtSemaphore.CreateSemaphore(...))
            if (initializer is not InvocationExpressionSyntax inv)
                return false;

            var symbol = context.SemanticModel.GetSymbolInfo(inv, context.CancellationToken).Symbol as IMethodSymbol;
            if (symbol == null)
                return false;

            if (symbol.Name != "CreateSemaphore")
                return false;

            // 静态方法, 接收类型必须是 TtSemaphore
            return SymbolEqualityComparer.Default.Equals(symbol.ContainingType, semaphoreType);
        }

        private static bool HasFreeSemaphoreCall(
            SyntaxNode body,
            ILocalSymbol target,
            SyntaxNodeAnalysisContext context)
        {
            // 兼容两种调用方式:
            //  1) localVar.FreeSemaphore()
            //  2) item.FieldXxx.FreeSemaphore() 这类不算 (target 不匹配, 自动跳过)
            foreach (var inv in body.DescendantNodes().OfType<InvocationExpressionSyntax>())
            {
                if (inv.Expression is not MemberAccessExpressionSyntax member)
                    continue;
                if (member.Name.Identifier.ValueText != "FreeSemaphore")
                    continue;

                if (member.Expression is not IdentifierNameSyntax id)
                    continue;

                var symbol = context.SemanticModel.GetSymbolInfo(id, context.CancellationToken).Symbol;
                if (symbol == null)
                    continue;

                if (SymbolEqualityComparer.Default.Equals(symbol, target))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 判断一个局部 semaphore 的所有权是否被"转移出去"了:
        /// - 被赋给字段 / 属性 / 数组元素 / 对象初始化器
        /// - 被作为参数传给其他方法 (调用方可能持有所有权)
        /// - 被作为本方法的 return 返回出去
        /// 这些场景下, 释放责任不归本函数, 不应报警。
        /// </summary>
        private static bool IsOwnershipTransferred(
            SyntaxNode body,
            ILocalSymbol target,
            SyntaxNodeAnalysisContext context)
        {
            foreach (var idNode in body.DescendantNodes().OfType<IdentifierNameSyntax>())
            {
                if (idNode.Identifier.ValueText != target.Name)
                    continue;

                var symbol = context.SemanticModel.GetSymbolInfo(idNode, context.CancellationToken).Symbol;
                if (symbol == null || !SymbolEqualityComparer.Default.Equals(symbol, target))
                    continue;

                // 跳过 declarator 自身
                if (idNode.Parent is VariableDeclaratorSyntax)
                    continue;

                // 1) 出现在 return / yield 表达式里
                for (var p = idNode.Parent; p != null && p != body; p = p.Parent)
                {
                    if (p is ReturnStatementSyntax || p is YieldStatementSyntax)
                        return true;

                    // 2) 被赋给字段/属性/数组元素 (左边不是局部变量声明)
                    if (p is AssignmentExpressionSyntax assign &&
                        assign.Right.DescendantNodesAndSelf().Contains(idNode) &&
                        IsAssignmentTargetNonLocal(assign.Left, context))
                    {
                        return true;
                    }

                    // 3) 出现在对象初始化器 / 集合初始化器中
                    if (p is InitializerExpressionSyntax)
                        return true;

                    // 4) 作为参数传出去
                    if (p is ArgumentSyntax)
                        return true;
                }
            }
            return false;
        }

        private static bool IsAssignmentTargetNonLocal(
            ExpressionSyntax left,
            SyntaxNodeAnalysisContext context)
        {
            var symbol = context.SemanticModel.GetSymbolInfo(left, context.CancellationToken).Symbol;
            if (symbol == null)
                return true; // 保守: 不能确定就当作转移
            return symbol.Kind switch
            {
                SymbolKind.Field => true,
                SymbolKind.Property => true,
                SymbolKind.ArrayType => true,
                SymbolKind.Parameter => true,
                _ => false,
            };
        }
    }
}
