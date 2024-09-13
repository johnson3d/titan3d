using EngineNS.Bricks.CodeBuilder;
using EngineNS.Bricks.StateMachine.TimedSM;
using EngineNS.DesignMacross;
using EngineNS.DesignMacross.Design;
using EngineNS.Rtti;

namespace EngineNS.Bricks.StateMachine.Macross
{
    public class TtStateMachineASTBuildUtil
    {
        public static void CreateNewAndInitInvokeStatement(TtDesignableVariableDescription description, TtMethodDeclaration method)
        {
            var createAssign = TtASTBuildUtil.CreateAssignOperatorStatement(new TtVariableReferenceExpression(description.Name), new TtCreateObjectExpression(description.VariableType.TypeFullName));
            method.MethodBody.Sequence.Add(createAssign);
            var initializeInvoke = new TtMethodInvokeStatement("Initialize",
                null, new TtVariableReferenceExpression(description.Name),
                new TtMethodInvokeArgumentExpression { Expression = new TtVariableReferenceExpression("context") });
            initializeInvoke.IsAsync = true;
            method.MethodBody.Sequence.Add(initializeInvoke);
        }

        public static TtMethodDeclaration CreateOverridedInitMethodStatement()
        {
            var returnVar = TtASTBuildUtil.CreateMethodReturnVariableDeclaration(new(typeof(bool)), TtASTBuildUtil.CreateDefaultValueExpression(new(typeof(bool))));
            var args = new List<TtMethodArgumentDeclaration>
            {
                TtASTBuildUtil.CreateMethodArgumentDeclaration("context", new(UTypeDesc.TypeOf<TtStateMachineContext>()), EMethodArgumentAttribute.Default)
            };
            var methodDeclaration = TtASTBuildUtil.CreateMethodDeclaration("Initialize", returnVar, args, true, TtMethodDeclaration.EAsyncType.CustomTask);

            return methodDeclaration;
        }

        public static void CreateBaseInitInvokeStatement(TtMethodDeclaration method)
        {
            var baseInitializeInvoke = new TtMethodInvokeStatement("Initialize",
                null, new TtBaseReferenceExpression(),
                new TtMethodInvokeArgumentExpression { Expression = new TtVariableReferenceExpression("context") });
            baseInitializeInvoke.IsAsync = true;
            method.MethodBody.Sequence.Add(baseInitializeInvoke);
        }
    }
}
