using EngineNS.Bricks.CodeBuilder;
using EngineNS.Bricks.StateMachine.TimedSM;
using EngineNS.DesignMacross;
using EngineNS.DesignMacross.Design;
using EngineNS.Rtti;

namespace EngineNS.Bricks.StateMachine.Macross
{
    public class TtStateMachineASTBuildUtil
    {
        public static void CreateNewAndInitInvokeStatement(TtDesignableVariableDescription description, UMethodDeclaration method)
        {
            var createAssign = TtASTBuildUtil.CreateAssignOperatorStatement(new UVariableReferenceExpression(description.Name), new UCreateObjectExpression(description.VariableType.TypeFullName));
            method.MethodBody.Sequence.Add(createAssign);
            var initializeInvoke = new UMethodInvokeStatement("Initialize",
                null, new UVariableReferenceExpression(description.Name),
                new UMethodInvokeArgumentExpression { Expression = new UVariableReferenceExpression("context") });
            initializeInvoke.IsAsync = true;
            method.MethodBody.Sequence.Add(initializeInvoke);
        }

        public static UMethodDeclaration CreateOverridedInitMethodStatement()
        {
            var returnVar = TtASTBuildUtil.CreateMethodReturnVariableDeclaration(new(typeof(bool)), TtASTBuildUtil.CreateDefaultValueExpression(new(typeof(bool))));
            var args = new List<UMethodArgumentDeclaration>
            {
                TtASTBuildUtil.CreateMethodArgumentDeclaration("context", new(UTypeDesc.TypeOf<TtStateMachineContext>()), EMethodArgumentAttribute.Default)
            };
            var methodDeclaration = TtASTBuildUtil.CreateMethodDeclaration("Initialize", returnVar, args, true, UMethodDeclaration.EAsyncType.CustomTask);

            return methodDeclaration;
        }

        public static void CreateBaseInitInvokeStatement(UMethodDeclaration method)
        {
            var baseInitializeInvoke = new UMethodInvokeStatement("Initialize",
                null, new UBaseReferenceExpression(),
                new UMethodInvokeArgumentExpression { Expression = new UVariableReferenceExpression("context") });
            baseInitializeInvoke.IsAsync = true;
            method.MethodBody.Sequence.Add(baseInitializeInvoke);
        }
    }
}
