using EngineNS.Animation.StateMachine;
using EngineNS.Bricks.CodeBuilder;
using EngineNS.Bricks.StateMachine.TimedSM;
using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Design;
using EngineNS.Rtti;

namespace EngineNS.DesignMacross
{
    public class TtASTBuildUtil
    {
        public static bool bGenerateDebugable = false;
        public static string ClassNamePrefix => "DMC_";
        public static string VariableNamePrefix => "";
        public static string MethodNamePrefix => "";
        public static string MethodLocalVarNamePrefix => "";
        public static string InitMethdName = "Initialize";
        public static string InitMethodReturedValueVarName = "returnedValue";
        public static string GenerateClassName(IClassDescription classDescription)
        {
            if(bGenerateDebugable)
            {
                return ClassNamePrefix + GetDescriptionCascadeName(classDescription); 
            }
            else
            {
                return ClassNamePrefix + classDescription.Name + "_" + (uint)classDescription.Id.GetHashCode() ;
            }
        }
        public static string GenerateVariableName(IVariableDescription  variableDescription)
        {
            if (bGenerateDebugable)
            {
                return VariableNamePrefix + GetDescriptionCascadeName(variableDescription);
            }
            else
            {
                return VariableNamePrefix + variableDescription.Name;// + "_" + (uint)variableDescription.Id.GetHashCode();
            }
        }
        public static string GenerateMethodName(IMethodDescription  methodDescription)
        {
            if (bGenerateDebugable)
            {
                return MethodNamePrefix + GetDescriptionCascadeName(methodDescription);
            }
            else
            {
                return MethodNamePrefix + methodDescription.Name;// + "_" + (uint)methodDescription.Id.GetHashCode();
            }
        }
        public static string GenerateMethodLocalVarName(IVariableDescription variableDescription)
        {
            if (bGenerateDebugable)
            {
                return MethodLocalVarNamePrefix + GetDescriptionCascadeName(variableDescription);
            }
            else
            {
                return MethodLocalVarNamePrefix + variableDescription.Name;// + "_" + (uint)variableDescription.Id.GetHashCode();
            }
        }
        static string GetDescriptionCascadeName(IDescription description)
        {
            if(description.Parent != null)
            {
                return GetDescriptionCascadeName(description.Parent) + "_" + description.Name;
            }
            return description.Name;
        }
        public static TtClassDeclaration BuildClassDeclaration(IClassDescription description, ref FClassBuildContext classBuildContext)
        {
            TtClassDeclaration declaration = new();
            declaration.IsStruct = description.IsStruct;
            declaration.ClassName = description.ClassName;
            declaration.Comment = description.Comment;
            declaration.Namespace = classBuildContext.MainClassDescription.Namespace;
            declaration.VisitMode = description.VisitMode;
            declaration.SupperClassNames = description.SupperClassNames;
            foreach (var variableDesc in description.Variables)
            {
                var varDeclaration = variableDesc.BuildVariableDeclaration(ref classBuildContext);
                declaration.Properties.Add(varDeclaration);
            }
            foreach (var methodDesc in description.Methods)
            {
                var methodDeclaration = methodDesc.BuildMethodDeclaration(ref classBuildContext);
                declaration.Methods.Add(methodDeclaration);
            }
            return declaration;
        }



        public static TtVariableDeclaration CreateVariableDeclaration(IVariableDescription description, ref FClassBuildContext classBuildContext)
        {
            TtVariableDeclaration declaration = new();
            declaration.VariableName = description.VariableName;
            declaration.VariableType = description.VariableType;
            declaration.InitValue = description.InitValue;
            declaration.Comment = description.Comment;
            declaration.VisitMode = description.VisitMode;
            return declaration;
        }
        public static TtMethodDeclaration CreateMethodDeclaration(IMethodDescription description, ref FClassBuildContext classBuildContext)
        {
            var returnVar = TtASTBuildUtil.CreateMethodReturnVariableDeclaration(new(description.ReturnValueType), TtASTBuildUtil.CreateDefaultValueExpression(new(description.ReturnValueType)));
            List<TtMethodArgumentDeclaration> args = new();
            foreach(var argDesc in description.Arguments)
            {
                args.Add(CreateMethodArgumentDeclaration(argDesc));
            }
           var methodDeclaration = CreateMethodDeclaration(description.Name, returnVar, args);

            return methodDeclaration;
        }
        public static TtMethodArgumentDeclaration CreateMethodArgumentDeclaration(TtMethodArgumentDescription methodArgumentDescription)
        {
            return CreateMethodArgumentDeclaration(methodArgumentDescription.Name, new(methodArgumentDescription.VariableType), methodArgumentDescription.OperationType, false);
        }




        public static TtMethodDeclaration CreateMethodDeclaration(string methodName, TtVariableDeclaration returnVar, List<TtMethodArgumentDeclaration> arguments, bool isOverrid = false, TtMethodDeclaration.EAsyncType asyncType = TtMethodDeclaration.EAsyncType.None)
        {
            TtMethodDeclaration methodDeclaration = new();
            methodDeclaration.IsOverride = true;
            methodDeclaration.AsyncType = asyncType;
            methodDeclaration.MethodName = methodName;
            if(returnVar != null)
            {
                methodDeclaration.ReturnValue = returnVar;
            }
            if(arguments != null)
            {
                methodDeclaration.Arguments = arguments;
            }
           
            return methodDeclaration;
        }

        public static TtMethodArgumentDeclaration CreateMethodArgumentDeclaration(string argName, TtTypeReference argType, EMethodArgumentAttribute argOperationType, bool ssParamArray = false)
        {
           return new TtMethodArgumentDeclaration()
            {
                VariableType = argType,
                VariableName = argName,
                //InitValue = new UPrimitiveExpression(),
                OperationType = argOperationType,
                IsParamArray = ssParamArray,
            };
        }
        public static TtVariableDeclaration CreateVariableDeclaration(string varName, TtTypeReference varType, TtExpressionBase varInitValue, EVisisMode varVisisMode = EVisisMode.Public)
        {
            return new TtVariableDeclaration()
            {
                VariableType = varType,
                InitValue = varInitValue,
                VariableName = varName,
                VisitMode = varVisisMode
            };
        }
        public static TtVariableDeclaration CreateMethodReturnVariableDeclaration(TtTypeReference varType, TtExpressionBase varInitValue, EVisisMode varVisisMode = EVisisMode.Public)
        {
            return new TtVariableDeclaration()
            {
                VariableType = varType,
                InitValue = varInitValue,
                VariableName = InitMethodReturedValueVarName,
                VisitMode = varVisisMode
            };
        }
        public static TtDefaultValueExpression CreateDefaultValueExpression(TtTypeReference type)   
        {
            return new TtDefaultValueExpression(type);
        }
        public static TtAssignOperatorStatement CreateAssignOperatorStatement(TtExpressionBase leftHandSide, TtExpressionBase rightHandSide)
        {
            TtAssignOperatorStatement assignOperatorStatement = new()
            {
                To = leftHandSide,
                From = rightHandSide
            };
            return assignOperatorStatement;
        }
        public static TtMethodDeclaration CreateInitMethodDeclaration(bool isOverride = true)
        {
            var returnVar = TtASTBuildUtil.CreateMethodReturnVariableDeclaration(new(typeof(bool)), TtASTBuildUtil.CreateDefaultValueExpression(new(typeof(bool))));
            var methodDeclaration = TtASTBuildUtil.CreateMethodDeclaration("Initialize", returnVar, null, isOverride, TtMethodDeclaration.EAsyncType.CustomTask);

            return methodDeclaration;
        }
        public static TtMethodDeclaration CreateTickMethodDeclaration(bool isOverride = true)
        {
            var args = new List<TtMethodArgumentDeclaration>
            {
                TtASTBuildUtil.CreateMethodArgumentDeclaration("elapseSecond", new(TtTypeDesc.TypeOf<float>()), EMethodArgumentAttribute.Default)
            };
            var methodDeclaration = TtASTBuildUtil.CreateMethodDeclaration("Tick", null, args, isOverride);

            return methodDeclaration;
        }
    }
}
