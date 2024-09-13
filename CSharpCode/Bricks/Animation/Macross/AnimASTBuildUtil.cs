using EngineNS.Animation.BlendTree;
using EngineNS.Animation.StateMachine;
using EngineNS.Bricks.CodeBuilder;
using EngineNS.DesignMacross;
using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Design;
using EngineNS.Rtti;

namespace EngineNS.Animation.Macross
{
    public class TtAnimASTBuildUtil 
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

        public static void CreateNewThenCenterDataAssignThenInitInvokeStatement(TtDesignableVariableDescription description, TtMethodDeclaration method)
        {
            var createAssign = TtASTBuildUtil.CreateAssignOperatorStatement(new TtVariableReferenceExpression(description.Name), new TtCreateObjectExpression(description.VariableType.TypeFullName));
            method.MethodBody.Sequence.Add(createAssign);

            var centerDataAssign = TtASTBuildUtil.CreateAssignOperatorStatement(
                new TtVariableReferenceExpression("CenterData", new TtVariableReferenceExpression(description.VariableName)),
                new TtVariableReferenceExpression("CenterData"));
            method.MethodBody.Sequence.Add(centerDataAssign);

            var initializeInvoke = new TtMethodInvokeStatement("Initialize",
                null, new TtVariableReferenceExpression(description.Name),
                new TtMethodInvokeArgumentExpression { Expression = new TtVariableReferenceExpression("context") });
            initializeInvoke.IsAsync = true;
            method.MethodBody.Sequence.Add(initializeInvoke);
        }

        public static TtMethodDeclaration CreateBlendTreeOverridedInitMethodStatement()
        {
            var returnVar = TtASTBuildUtil.CreateMethodReturnVariableDeclaration(new(typeof(bool)), TtASTBuildUtil.CreateDefaultValueExpression(new(typeof(bool))));
            var args = new List<TtMethodArgumentDeclaration>
            {
                TtASTBuildUtil.CreateMethodArgumentDeclaration("context", new(UTypeDesc.TypeOf<FAnimBlendTreeContext>()), EMethodArgumentAttribute.Default)
            };
            var methodDeclaration = TtASTBuildUtil.CreateMethodDeclaration("Initialize", returnVar, args, true, TtMethodDeclaration.EAsyncType.CustomTask);

            return methodDeclaration;
        }
        public static TtMethodDeclaration CreateStateMachineOverridedInitMethodStatement()
        {
            var returnVar = TtASTBuildUtil.CreateMethodReturnVariableDeclaration(new(typeof(bool)), TtASTBuildUtil.CreateDefaultValueExpression(new(typeof(bool))));
            var args = new List<TtMethodArgumentDeclaration>
            {
                TtASTBuildUtil.CreateMethodArgumentDeclaration("context", new(UTypeDesc.TypeOf<TtAnimStateMachineContext>()), EMethodArgumentAttribute.Default)
            };
            var methodDeclaration = TtASTBuildUtil.CreateMethodDeclaration("Initialize", returnVar, args, true, TtMethodDeclaration.EAsyncType.CustomTask);

            return methodDeclaration;
        }

        public static void CreateCenterDataAssignStatement(IDesignableVariableDescription description, TtMethodDeclaration method)
        {
            var centerDataAssign = TtASTBuildUtil.CreateAssignOperatorStatement(
                new TtVariableReferenceExpression("CenterData", new TtVariableReferenceExpression(description.VariableName)),
                new TtVariableReferenceExpression("CenterData"));
            method.MethodBody.Sequence.Add(centerDataAssign);
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
