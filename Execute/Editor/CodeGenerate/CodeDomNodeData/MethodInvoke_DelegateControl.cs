using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Text;
using System.Windows.Data;
using CodeGenerateSystem.Base;
using EngineNS.IO;

namespace CodeDomNode
{
    [CodeGenerateSystem.CustomConstructionParams(typeof(MethodInvoke_DelegateControlConstructionParams))]
    public partial class MethodInvoke_DelegateControl : CodeGenerateSystem.Base.BaseNodeControl
    {
        [EngineNS.Rtti.MetaClass]
        public class MethodInvoke_DelegateControlConstructionParams : CodeGenerateSystem.Base.ConstructionParams
        {
            [EngineNS.Rtti.MetaData]
            public MethodInvokeNode.enParamConstructType ConstructType
            {
                get;
                set;
            } = MethodInvokeNode.enParamConstructType.MethodInvoke;
            [EngineNS.Rtti.MetaData]
            public MethodParamInfoAssist ParamInfo
            {
                get;
                set;
            }
            [EngineNS.Rtti.MetaData]
            public List<MethodParamInfoAssist> InputParmas
            {
                get;
                set;
            } = new List<MethodParamInfoAssist>();

            [EngineNS.Rtti.MetaData]
            public CustomMethodInfo DelegateMethodInfo
            {
                get;
                set;
            }
            [EngineNS.Rtti.MetaData]
            public int CustomParamStartIndexInDelegateMethodInfo
            {
                get;
                set;
            } = 0;
            [EngineNS.Rtti.MetaData]
            public List<Guid> InParamIndexes
            {
                get;
                set;
            } = new List<Guid>();

            public MethodInvoke_DelegateControlConstructionParams()
            {
            }
            public override EditorCommon.CodeGenerateSystem.INodeConstructionParams Duplicate()
            {
                var retVal = base.Duplicate() as MethodInvoke_DelegateControlConstructionParams;
                retVal.ConstructType = ConstructType;
                retVal.ParamInfo = ParamInfo;
                retVal.InputParmas = new List<MethodParamInfoAssist>(InputParmas);
                retVal.DelegateMethodInfo = DelegateMethodInfo;
                retVal.CustomParamStartIndexInDelegateMethodInfo = CustomParamStartIndexInDelegateMethodInfo;
                retVal.InParamIndexes = new List<Guid>(InParamIndexes);
                return retVal;
            }
            public override bool Equals(object obj)
            {
                if (!base.Equals(obj))
                    return false;
                var param = obj as MethodInvoke_DelegateControlConstructionParams;
                if (param == null)
                    return false;
                if ((ConstructType == param.ConstructType) &&
                    (ParamInfo == param.ParamInfo))
                    return true;
                return false;
            }

            public override int GetHashCode()
            {
                return (base.GetHashCodeString() + ConstructType.ToString() + ParamInfo.ToString()).GetHashCode();
            }
        }

        public Type ParamType
        {
            get
            {
                return ((MethodInvoke_DelegateControlConstructionParams)CSParam).ParamInfo.ParameterType;
            }
        }
        public string ParamName
        {
            get
            {
                return ((MethodInvoke_DelegateControlConstructionParams)CSParam).ParamInfo.ParamName;
            }
        }
        public System.CodeDom.FieldDirection Direction
        {
            get
            {
                return ((MethodInvoke_DelegateControlConstructionParams)CSParam).ParamInfo.FieldDirection;
            }
        }
        public string MethodName
        {
            get { return ParamName + "_" + EngineNS.Editor.Assist.GetValuedGUIDString(Id); }
        }

        partial void InitConstruction();
        public MethodInvoke_DelegateControl(CodeGenerateSystem.Base.ConstructionParams csParam)
            : base(csParam)
        {
            var param = csParam as MethodInvoke_DelegateControlConstructionParams;
            IsOnlyReturnValue = true;
            NodeName = param.ParamInfo.ParamName;//param.ConstructParam;

            InitConstruction();
            switch (param.ConstructType)
            {
                case MethodInvokeNode.enParamConstructType.MethodInvoke:
                    break;
                default:
                    throw new InvalidOperationException();
            }
        }

        public static void InitNodePinTypes(CodeGenerateSystem.Base.ConstructionParams smParam)
        {

        }

        void InitDelegateMethodInfo()
        {
            var param = CSParam as MethodInvoke_DelegateControlConstructionParams;
            if (param.DelegateMethodInfo == null)
            {
                param.DelegateMethodInfo = new CodeDomNode.CustomMethodInfo()
                {
                    MethodName = param.ParamInfo.ParamName,
                    CSType = param.CSType,
                };
                var method = param.ParamInfo.ParameterType.GetMethod("Invoke");
                var methodParams = method.GetParameters();
                foreach (var methodParam in methodParams)
                {
                    var funcParam = new CustomMethodInfo.FunctionParam();
                    funcParam.HostMethodInfo = param.DelegateMethodInfo;
                    funcParam.ParamName = methodParam.Name;
                   
                    if (methodParam.IsOut)
                    {
                        //var typefullname = methodParam.ParameterType.FullName.Substring(0, methodParam.ParameterType.FullName.Length - 1);
                        //var type = methodParam.ParameterType.Assembly.GetType(typefullname);
                        //funcParam.ParamType = new VariableType(type, param.CSType);
                        funcParam.ParamType = new VariableType(methodParam.ParameterType, param.CSType);
                        param.DelegateMethodInfo.OutParams.Add(funcParam);
                    }
                    else
                    {
                        funcParam.ParamType = new VariableType(methodParam.ParameterType, param.CSType);
                        param.DelegateMethodInfo.InParams.Add(funcParam);
                    }
                }
                param.CustomParamStartIndexInDelegateMethodInfo = param.DelegateMethodInfo.InParams.Count;
                if (method.ReturnType != typeof(void) && method.ReturnType != typeof(System.Threading.Tasks.Task))
                {
                    var funcParam = new CustomMethodInfo.FunctionParam();
                    funcParam.HostMethodInfo = param.DelegateMethodInfo;
                    funcParam.ParamName = "Return";
                    if (method.ReturnType.BaseType == typeof(System.Threading.Tasks.Task))
                    {
                        var genericType = method.ReturnType.GetGenericArguments()[0];
                        funcParam.ParamType = new VariableType(genericType, param.CSType);
                        param.DelegateMethodInfo.IsAsync = true;
                    }
                    else
                    {
                        funcParam.ParamType = new VariableType(method.ReturnType, param.CSType);
                    }
                    param.DelegateMethodInfo.OutParams.Add(funcParam);
                }
                else if (method.ReturnType == typeof(System.Threading.Tasks.Task))
                    param.DelegateMethodInfo.IsAsync = true;
            }
        }
        partial void OnLoad_WPF();
        public override async System.Threading.Tasks.Task Load(XndNode xndNode)
        {
            await base.Load(xndNode);

            var param = CSParam as MethodInvoke_DelegateControlConstructionParams;
            for(int i=0; i<mChildNodes.Count; i++)
            {
                var paramNode = mChildNodes[i] as MethodInvokeParameterControl;
                if (paramNode == null)
                    continue;

                // 将参数的配置指向同一个地址
                var paramNodeParam = paramNode.CSParam as MethodInvokeParameterControl.MethodInvokeParameterConstructionParams;
                paramNodeParam.ParamInfo = param.InputParmas[i];
                paramNode.ParamPin.SetBinding(CodeGenerateSystem.Controls.LinkInControl.NameStringProperty, new Binding("UIDisplayParamName") { Source = paramNodeParam.ParamInfo });
            }

            OnLoad_WPF();
        }

        public CodeExpression[] TempParamNames;
        public override CodeExpression GCode_CodeDom_GetValue(LinkPinControl element, GenerateCodeContext_Method context, GenerateCodeContext_PreNode preNodeContext = null)
        {
            var param = CSParam as MethodInvoke_DelegateControlConstructionParams;

            var lambdaExp = new CodeGenerateSystem.CodeDom.CodeLambdaExpression();
            lambdaExp.LambdaFieldName = "lambda_" + MethodName;

            var method = param.ParamInfo.ParameterType.GetMethod("Invoke");
            var methodInvokeExp = new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression();
            methodInvokeExp.Method = new CodeMethodReferenceExpression(new CodeThisReferenceExpression(), MethodName);
            foreach(var methodParam in method.GetParameters())
            {
                var lambdaParam = new CodeGenerateSystem.CodeDom.CodeLambdaExpression.LambdaParam();
                if (methodParam.IsOut)
                {
                    lambdaParam.Dir = FieldDirection.Out;
                    lambdaParam.Name = "__" + methodParam.Name;
                    lambdaExp.LambdaParams.Add(lambdaParam);
                }
                else if(methodParam.ParameterType.IsByRef)
                {
                    lambdaParam.Dir = FieldDirection.Ref;
                    lambdaParam.Name = "__" + methodParam.Name;
                    lambdaExp.LambdaParams.Add(lambdaParam);
                }
                else
                {
                    lambdaParam.Dir = FieldDirection.In;
                    lambdaParam.Name = "__" + methodParam.Name;
                    lambdaExp.LambdaParams.Add(lambdaParam);
                }
                methodInvokeExp.Parameters.Add(new CodeDirectionExpression(lambdaParam.Dir, new CodeVariableReferenceExpression(lambdaParam.Name)));
            }

            foreach (var child in mChildNodes)
            {
                // 获取连线参数的数值
                var paramNode = child as MethodInvokeParameterControl;
                var linkOI = paramNode.ParamPin;
                var fromNode = linkOI.GetLinkedObject(0, true);
                var exp = fromNode.GCode_CodeDom_GetValue(linkOI.GetLinkedPinControl(0, true), context);
                methodInvokeExp.Parameters.Add(exp);
            }

            lambdaExp.MethodInvoke = methodInvokeExp;

            if (method.ReturnType != typeof(void))
            {
                lambdaExp.NeedReturn = true;
            }

            return lambdaExp;
        }
        public override async System.Threading.Tasks.Task GCode_CodeDom_GenerateCode(CodeTypeDeclaration codeClass, CodeStatementCollection codeStatementCollection, CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            var param = CSParam as MethodInvoke_DelegateControlConstructionParams;
            if(mChildNodes.Count == 0)
            {
                // 不包含自定义参数，直接使用函数成员，减少lambda造成的gc
                var fieldName = "lambda_" + MethodName;
                var lambdaField = new CodeMemberField(new CodeTypeReference(param.ParamInfo.ParameterType), fieldName);
                codeClass.Members.Add(lambdaField);

                var cc = codeClass as CodeGenerateSystem.CodeDom.CodeTypeDeclaration;
                if (cc == null)
                    throw new InvalidOperationException("请使用 CodeGenerateSystem.CodeDom.CodeTypeDeclaration 替换 System.CodeDom.CodeTypeDeclaration");
                cc.ConstructStatements.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), fieldName), new CodeVariableReferenceExpression(MethodName)));
            }

            await GenerateCode(codeClass, context.ClassContext);
        }
    }
}
