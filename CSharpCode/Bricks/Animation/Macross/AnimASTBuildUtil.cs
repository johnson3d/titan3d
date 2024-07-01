using EngineNS.Animation.StateMachine;
using EngineNS.Bricks.CodeBuilder;
using EngineNS.DesignMacross;
using EngineNS.DesignMacross.Design;
using EngineNS.Rtti;

namespace EngineNS.Animation.Macross
{
    public class TtAnimASTBuildUtil 
    {
        public static void CreateNewAndInitInvokeStatement(TtDesignableVariableDescription description, UMethodDeclaration method)
        {
            var attachmentAssign = TtASTBuildUtil.CreateAssignOperatorStatement(new UVariableReferenceExpression(description.Name), new UCreateObjectExpression(description.VariableType.TypeFullName));
            method.MethodBody.Sequence.Add(attachmentAssign);
            var attachmentInitializeInvoke = new UMethodInvokeStatement("Initialize",
                null, new UVariableReferenceExpression(description.Name),
                new UMethodInvokeArgumentExpression { Expression = new UVariableReferenceExpression("context") });
            attachmentInitializeInvoke.IsAsync = true;
            method.MethodBody.Sequence.Add(attachmentInitializeInvoke);
        }

        public static UMethodDeclaration CreateOverridedInitMethodStatement()
        {
            var returnVar = TtASTBuildUtil.CreateMethodReturnVariableDeclaration(new(typeof(bool)), TtASTBuildUtil.CreateDefaultValueExpression(new(typeof(bool))));
            var args = new List<UMethodArgumentDeclaration>
            {
                TtASTBuildUtil.CreateMethodArgumentDeclaration("context", new(UTypeDesc.TypeOf<TtAnimStateMachineContext>()), EMethodArgumentAttribute.Default)
            };
            var methodDeclaration = TtASTBuildUtil.CreateMethodDeclaration("Initialize", returnVar, args, true, UMethodDeclaration.EAsyncType.CustomTask);

            return methodDeclaration;
        }

        public static void CreateBaseInitInvokeStatement(UMethodDeclaration method)
        {
            var attachmentInitializeInvoke = new UMethodInvokeStatement("Initialize",
                null, new UBaseReferenceExpression(),
                new UMethodInvokeArgumentExpression { Expression = new UVariableReferenceExpression("context") });
            attachmentInitializeInvoke.IsAsync = true;
            method.MethodBody.Sequence.Add(attachmentInitializeInvoke);
        }
    }
}
