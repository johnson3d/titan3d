using EngineNS.Bricks.CodeBuilder;
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
        public static UClassDeclaration BuildClassDeclaration(IClassDescription description, ref FClassBuildContext classBuildContext)
        {
            UClassDeclaration declaration = new UClassDeclaration();
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



        public static UVariableDeclaration CreateVariableDeclaration(IVariableDescription description, ref FClassBuildContext classBuildContext)
        {
            UVariableDeclaration declaration = new UVariableDeclaration();
            declaration.VariableName = description.VariableName;
            declaration.VariableType = description.VariableType;
            declaration.InitValue = description.InitValue;
            declaration.Comment = description.Comment;
            declaration.VisitMode = description.VisitMode;
            return declaration;
        }
        public static UMethodDeclaration CreateMethodDeclaration(IMethodDescription description, ref FClassBuildContext classBuildContext)
        {
            var returnVar = TtASTBuildUtil.CreateMethodReturnVariableDeclaration(new(description.ReturnValueType), TtASTBuildUtil.CreateDefaultValueExpression(new(description.ReturnValueType)));
            List<UMethodArgumentDeclaration> args = new();
            foreach(var argDesc in description.Arguments)
            {
                args.Add(CreateMethodArgumentDeclaration(argDesc));
            }
           var methodDeclaration = CreateMethodDeclaration(description.Name, returnVar, args);

            return methodDeclaration;
        }
        public static UMethodArgumentDeclaration CreateMethodArgumentDeclaration(TtMethodArgumentDescription methodArgumentDescription)
        {
            return CreateMethodArgumentDeclaration(methodArgumentDescription.Name, new(methodArgumentDescription.VariableType), methodArgumentDescription.OperationType, false);
        }




        public static UMethodDeclaration CreateMethodDeclaration(string methodName, UVariableDeclaration returnVar, List<UMethodArgumentDeclaration> arguments, bool isOverrid = false, UMethodDeclaration.EAsyncType asyncType = UMethodDeclaration.EAsyncType.None)
        {
            UMethodDeclaration methodDeclaration = new();
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

        public static UMethodArgumentDeclaration CreateMethodArgumentDeclaration(string argName, UTypeReference argType, EMethodArgumentAttribute argOperationType, bool ssParamArray = false)
        {
           return new UMethodArgumentDeclaration()
            {
                VariableType = argType,
                VariableName = argName,
                //InitValue = new UPrimitiveExpression(),
                OperationType = argOperationType,
                IsParamArray = ssParamArray,
            };
        }
        public static UVariableDeclaration CreateVariableDeclaration(string varName, UTypeReference varType, UExpressionBase varInitValue, EVisisMode varVisisMode = EVisisMode.Public)
        {
            return new UVariableDeclaration()
            {
                VariableType = varType,
                InitValue = varInitValue,
                VariableName = varName,
                VisitMode = varVisisMode
            };
        }
        public static UVariableDeclaration CreateMethodReturnVariableDeclaration(UTypeReference varType, UExpressionBase varInitValue, EVisisMode varVisisMode = EVisisMode.Public)
        {
            return new UVariableDeclaration()
            {
                VariableType = varType,
                InitValue = varInitValue,
                VariableName = InitMethodReturedValueVarName,
                VisitMode = varVisisMode
            };
        }
        public static UDefaultValueExpression CreateDefaultValueExpression(UTypeReference type)   
        {
            return new UDefaultValueExpression(type);
        }
        public static UAssignOperatorStatement CreateAssignOperatorStatement(UExpressionBase leftHandSide, UExpressionBase rightHandSide)
        {
            UAssignOperatorStatement assignOperatorStatement = new()
            {
                To = leftHandSide,
                From = rightHandSide
            };
            return assignOperatorStatement;
        }
        public static UMethodDeclaration CreateInitMethodDeclaration(bool isOverride = true)
        {
            UMethodDeclaration methodDeclaration = new UMethodDeclaration();
            methodDeclaration.IsOverride = isOverride;
            methodDeclaration.MethodName = "Initialize";
            methodDeclaration.ReturnValue = CreateMethodReturnVariableDeclaration(new UTypeReference(typeof(bool)), new UDefaultValueExpression(typeof(bool)));
            return methodDeclaration;
        }
    }
}
