using EngineNS.Animation.BlendTree;
using EngineNS.Animation.StateMachine;
using EngineNS.Bricks.CodeBuilder;
using EngineNS.Bricks.StateMachine;
using EngineNS.Bricks.StateMachine.TimedSM;
using EngineNS.DesignMacross.Base.Description;
using EngineNS.DesignMacross.Design;
using EngineNS.Rtti;
using MathNet.Numerics.Differentiation;
using System.Reflection;

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
            if (bGenerateDebugable)
            {
                return ClassNamePrefix + GetDescriptionCascadeName(classDescription);
            }
            else
            {
                return ClassNamePrefix + classDescription.Name + "_" + (uint)classDescription.Id.GetHashCode();
            }
        }
        public static string GenerateVariableName(IVariableDescription variableDescription)
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
        public static string GenerateMethodName(IMethodDescription methodDescription)
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
            if (description.Parent != null)
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

            classBuildContext.ClassDeclaration = declaration;
            classBuildContext.ClassDescription = description;
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
            TtVariableDeclaration returnVar = null;
            if (description.ReturnValueType != null)
            {
                returnVar = TtASTBuildUtil.CreateMethodReturnVariableDeclaration(
                    new(description.ReturnValueType),
                    TtASTBuildUtil.CreateDefaultValueExpression(new(description.ReturnValueType)));
            }
            List<TtMethodArgumentDeclaration> args = new();
            foreach (var argDesc in description.Arguments)
            {
                args.Add(CreateMethodArgumentDeclaration(argDesc));
            }
            var methodDeclaration = CreateMethodDeclaration(description.Name, returnVar, args, description.IsOverride, description.AsyncType);

            return methodDeclaration;
        }
        public static TtMethodArgumentDeclaration CreateMethodArgumentDeclaration(TtMethodArgumentDescription methodArgumentDescription)
        {
            return CreateMethodArgumentDeclaration(methodArgumentDescription.Name, new(methodArgumentDescription.VariableType), methodArgumentDescription.OperationType, false);
        }

        public static TtMethodDeclaration CreateMethodDeclaration(string methodName, TtVariableDeclaration returnVar, List<TtMethodArgumentDeclaration> arguments, bool isOverrid = false, TtMethodDeclaration.EAsyncType asyncType = TtMethodDeclaration.EAsyncType.None)
        {
            TtMethodDeclaration methodDeclaration = new();
            methodDeclaration.IsOverride = isOverrid;
            methodDeclaration.AsyncType = asyncType;
            methodDeclaration.MethodName = methodName;
            if (returnVar != null)
            {
                methodDeclaration.ReturnValue = returnVar;
            }
            if (arguments != null)
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
        public static TtMethodDeclaration CreateAfterTickMethodDeclaration(bool isOverride = true)
        {
            var args = new List<TtMethodArgumentDeclaration>
            {
                TtASTBuildUtil.CreateMethodArgumentDeclaration("elapseSecond", new(TtTypeDesc.TypeOf<float>()), EMethodArgumentAttribute.Default)
            };
            var methodDeclaration = TtASTBuildUtil.CreateMethodDeclaration("AfterTick", null, args, isOverride);

            return methodDeclaration;
        }
        public static void CreateCenterDataAssignStatement(IDesignableVariableDescription description, TtMethodDeclaration method)
        {
            var centerDataAssign = TtASTBuildUtil.CreateAssignOperatorStatement(
                new TtVariableReferenceExpression("CenterData", new TtVariableReferenceExpression(description.VariableName)),
                new TtVariableReferenceExpression("CenterData"));
            method.MethodBody.Sequence.Add(centerDataAssign);
        }

        // 通过反射将list中的所有数值转换为CodeBuilder表达式
        public static void CreateListss(string listName, System.Collections.IList list, TtMethodDeclaration method)
        {
            //new list           
            var listType = TtTypeDesc.TypeOf(list.GetType());

            var listCreate = TtASTBuildUtil.CreateVariableDeclaration(listName, new TtTypeReference(listType),
                                        new TtCreateObjectExpression(listType.CSharpTypeName));
            method.MethodBody.Sequence.Add(listCreate);
            if (list.GetType().IsGenericType)
            {
                var genericType = list.GetType().GenericTypeArguments[0];
                ;
                var genericTypeName = genericType.Name;
                foreach (var item in list)
                {
                    List<TtPrimitiveExpression> initPrimitives = new();
                    foreach (var property in item.GetType().GetProperties())
                    {
                        initPrimitives.Add(new TtPrimitiveExpression(TtTypeDesc.TypeOf(property.PropertyType), property.GetValue(item)));
                    }
                    var itemCreateExp = new TtCreateObjectExpression(genericType.FullName, initPrimitives.ToArray());

                    var listAddStatment = new TtMethodInvokeStatement("Add", null, new TtVariableReferenceExpression(listName), new TtMethodInvokeArgumentExpression { Expression = itemCreateExp });
                    method.MethodBody.Sequence.Add(listAddStatment);
                }
            }

        }

        public static void CreateItem(Type itemType, object item, string itemName, TtMethodDeclaration method)
        {
            var listCreate = TtASTBuildUtil.CreateVariableDeclaration(itemName, new TtTypeReference(itemType),
                            new TtCreateObjectExpression(itemType.FullName));
            method.MethodBody.Sequence.Add(listCreate);
            foreach (var property in itemType.GetProperties())
            {
                var propertyType = property.PropertyType;
                if (propertyType.IsValueType && (propertyType.IsPrimitive || propertyType.IsEnum))
                {
                    AssignProperty(property, itemName, item, method);
                }
                else if (propertyType == typeof(String))
                {
                    AssignProperty(property, itemName, item, method);
                }
                else
                {
                    var pType = property.PropertyType;
                    var pValue = property.GetValue(item);
                    var pName = $"{property.Name}_Instance_{(uint)Guid.NewGuid().GetHashCode()}";
                    CreateItem(pType, pValue, pName, method);
                    var left = new TtVariableReferenceExpression(property.Name, new TtVariableReferenceExpression(itemName));
                    var right = new TtVariableReferenceExpression(pName);
                    var assign = CreateAssignOperatorStatement(left, right);
                    method.MethodBody.Sequence.Add(assign);
                }
            }

        }
        public static void AssignProperty(PropertyInfo property, string propertyHostName, object item, TtMethodDeclaration method)
        {
            var left = new TtVariableReferenceExpression(property.Name, new TtVariableReferenceExpression(propertyHostName));
            var right = new TtPrimitiveExpression(TtTypeDesc.TypeOf(property.PropertyType), property.GetValue(item));
            var assign = CreateAssignOperatorStatement(left, right);
            method.MethodBody.Sequence.Add(assign);
        }

        public static void CreateList(string listName, System.Collections.IList list, TtMethodDeclaration method)
        {
            //new list           
            var listType = TtTypeDesc.TypeOf(list.GetType());

            var listCreate = TtASTBuildUtil.CreateVariableDeclaration(listName, new TtTypeReference(listType),
                                        new TtCreateObjectExpression(listType.CSharpTypeName));
            method.MethodBody.Sequence.Add(listCreate);
            if (list.GetType().IsGenericType)
            {
                var genericType = list.GetType().GenericTypeArguments[0];

                foreach (var item in list)
                {
                    var itemName = $"{listName}_Item_{(uint)Guid.NewGuid().GetHashCode()}";
                    var itemType = item.GetType();
                    CreateItem(itemType, item, itemName, method);

                    var listAddStatment = new TtMethodInvokeStatement("Add", null, new TtVariableReferenceExpression(listName), new TtMethodInvokeArgumentExpression { Expression = new TtVariableReferenceExpression(itemName) });
                    method.MethodBody.Sequence.Add(listAddStatment);
                }

            }
        }
    }
}