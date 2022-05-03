using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace EngineNS.Bricks.CodeBuilder
{
    public enum EVisisMode
    {
        Public,
        Protected,
        Private,
        Local,
        Internal,
    }

    public enum EValueType
    {
        Boolean,
        String,
        Int8,
        UInt8,
        Int16,
        UInt16,
        Int32,
        UInt32,
        Int64,
        UInt64,
        Float,
        Double,
    }

    public enum EMethodArgumentAttribute
    {
        Default,
        In,
        Out,
        Ref,
    }

    public class UCodeObject
    {
    }

    public class UExpressionBase : UCodeObject { }
    public class UStatementBase : UCodeObject
    {
        public UStatementBase Next;
    }
    public class UTypeReference : IO.ISerializer
    {
        string mTypeFullName;
        [Rtti.Meta]
        public string TypeFullName
        {
            get
            {
                if(mTypeDesc != null)
                {
                    return mTypeDesc.FullName;
                }
                return mTypeFullName;
            }
            set
            {
                mTypeDesc = Rtti.UTypeDesc.TypeOf(value);
                if (mTypeDesc == null)
                    mTypeFullName = value;
            }
        }
        Rtti.UTypeDesc mTypeDesc;
        public Rtti.UTypeDesc TypeDesc
        {
            get { return mTypeDesc; }
        }
        bool mIsEnum = false;
        [Rtti.Meta]
        public bool IsEnum
        {
            get
            {
                if (mTypeDesc != null)
                    return mTypeDesc.IsEnum;
                return mIsEnum;
            }
            set
            {
                mIsEnum = value;
            }
        }
        public UTypeReference(string typeFullName, bool isEnum = false)
        {
            mTypeFullName = typeFullName;
            mIsEnum = isEnum;
        }
        public UTypeReference(Rtti.UTypeDesc typeDesc)
        {
            mTypeDesc = typeDesc;
        }
        public UTypeReference(Type type)
        {
            mTypeDesc = Rtti.UTypeDesc.TypeOf(type);
        }
        public static bool operator == (UTypeReference lhs, UTypeReference rhs)
        {
            if (lhs is null)
                return rhs is null;
            return lhs.Equals(rhs);
        }
        public static bool operator != (UTypeReference lhs, UTypeReference rhs)
        {
            return !(lhs == rhs);
        }
        public override bool Equals(object obj)
        {
            var typeR = obj as UTypeReference;
            if (typeR == null)
                return false;

            return typeR.mTypeFullName == mTypeFullName;
        }
        public override int GetHashCode()
        {
            return mTypeFullName.GetHashCode();
        }
        public override string ToString()
        {
            return TypeFullName;
        }

        public void OnPreRead(object tagObject, object hostObject, bool fromXml)
        {
        }

        public void OnPropertyRead(object tagObject, PropertyInfo prop, bool fromXml)
        {
        }

        public bool IsEqual(Type type)
        {
            if (mTypeDesc != null)
                return mTypeDesc.IsEqual(type);
            return TypeFullName == type.FullName;
        }
    }
    public class UVariableDeclaration : UStatementBase, IO.ISerializer
    {
        [Rtti.Meta]
        public UTypeReference VariableType { get; set; }
        [Rtti.Meta]
        public string VariableName { get; set; } = "Unknow";
        [Rtti.Meta]
        public UExpressionBase InitValue { get; set; }
        [Rtti.Meta]
        public UCommentStatement Comment { get; set; }
        [Rtti.Meta]
        public EVisisMode VisitMode { get; set; } = EVisisMode.Public;

        public UVariableDeclaration()
        {
        }

        public void OnPreRead(object tagObject, object hostObject, bool fromXml)
        {
        }

        public void OnPropertyRead(object tagObject, PropertyInfo prop, bool fromXml)
        {
        }

        public override bool Equals(object obj)
        {
            var varDec = obj as UVariableDeclaration;
            if (varDec == null)
                return false;
            return (VariableName == varDec.VariableName) && (VariableType == varDec.VariableType);
        }
        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }
        public override string ToString()
        {
            return (VariableType.TypeFullName + " " + VariableName);
        }
    }

    public class UMethodArgumentDeclaration : UCodeObject, IO.ISerializer
    {
        public Rtti.MetaParameterAttribute Meta;

        [Rtti.Meta]
        public UTypeReference VariableType { get; set; }
        [Rtti.Meta]
        public string VariableName { get; set; }
        [Rtti.Meta]
        public UExpressionBase InitValue { get; set; }
        [Rtti.Meta]
        public EMethodArgumentAttribute OperationType { get; set; } = EMethodArgumentAttribute.Default;
        [Rtti.Meta]
        public bool IsParamArray { get; set; } = false;
        [Rtti.Meta]
        public bool HasDefaultValue { get; set; } = false;

        public static EMethodArgumentAttribute GetOperationType(System.Reflection.ParameterInfo info)
        {
            if (info.IsIn)
                return EMethodArgumentAttribute.In;
            if (info.IsOut)
                return EMethodArgumentAttribute.Out;
            if (info.ParameterType.IsByRef)
                return EMethodArgumentAttribute.Ref;
            return EMethodArgumentAttribute.Default;
        }
        public static EMethodArgumentAttribute GetOperationType(Rtti.UClassMeta.MethodMeta.ParamMeta info)
        {
            if (info.IsIn)
                return EMethodArgumentAttribute.In;
            if (info.IsOut)
                return EMethodArgumentAttribute.Out;
            if (info.IsRef)
                return EMethodArgumentAttribute.Ref;
            return EMethodArgumentAttribute.Default;
        }
        public static bool GetIsParamArray(System.Reflection.ParameterInfo info)
        {
            var att = info.GetCustomAttributes(typeof(System.ParamArrayAttribute), true);
            if (att.Length > 0)
                return true;
            return false;
        }
        public static bool GetIsParamArray(Rtti.UClassMeta.MethodMeta.ParamMeta info)
        {
            return info.IsParamArray;
        }
        public static UMethodArgumentDeclaration GetParam(System.Reflection.ParameterInfo info)
        {
            var retVal = new UMethodArgumentDeclaration()
            {
                VariableType = new UTypeReference(info.ParameterType),
                VariableName = info.Name,
                HasDefaultValue = info.HasDefaultValue,
            };
            if (info.HasDefaultValue)
                retVal.InitValue = new UPrimitiveExpression(Rtti.UTypeDesc.TypeOf(info.DefaultValue.GetType()), info.DefaultValue);
            retVal.OperationType = GetOperationType(info);
            retVal.IsParamArray = GetIsParamArray(info);
            var attrs = info.GetCustomAttributes(typeof(Rtti.MetaParameterAttribute), false);
            if (attrs.Length > 0)
                retVal.Meta = attrs[0] as Rtti.MetaParameterAttribute;
            return retVal;
        }
        public static UMethodArgumentDeclaration GetParam(Rtti.UClassMeta.MethodMeta.ParamMeta info)
        {
            var retVal = new UMethodArgumentDeclaration()
            {
                VariableType = new UTypeReference(info.ParameterType),
                VariableName = info.Name,
                HasDefaultValue = info.HasDefaultValue,
            };
            if (info.HasDefaultValue)
                retVal.InitValue = new UPrimitiveExpression(Rtti.UTypeDesc.TypeOf(info.DefaultValue.GetType()), info.DefaultValue);
            retVal.OperationType = GetOperationType(info);
            retVal.IsParamArray = GetIsParamArray(info);
            retVal.Meta = info.Meta;
            return retVal;
        }

        public void OnPreRead(object tagObject, object hostObject, bool fromXml)
        {
        }

        public void OnPropertyRead(object tagObject, PropertyInfo prop, bool fromXml)
        {
        }

        public override bool Equals(object obj)
        {
            var arg = obj as UMethodArgumentDeclaration;
            if (arg == null)
                return false;
            return (VariableType.Equals(arg.VariableType) &&
                    VariableName == arg.VariableName &&
                    IsParamArray == IsParamArray);
        }
        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }
        public override string ToString()
        {
            return OperationType.ToString() + " " + IsParamArray.ToString() + " " + VariableType.TypeFullName + " " + VariableName;
        }
    }

    public class UMethodDeclaration : UCodeObject, IO.ISerializer
    {
        [Rtti.Meta]
        public EVisisMode VisitMode { get; set; } = EVisisMode.Public;
        [Rtti.Meta]
        public UVariableDeclaration ReturnValue { get; set; }
        [Rtti.Meta]
        public string MethodName { get; set; } = "Unknow";
        [Rtti.Meta]
        public List<UMethodArgumentDeclaration> Arguments { get; set; } = new List<UMethodArgumentDeclaration>();
        [Rtti.Meta]
        public List<UVariableDeclaration> LocalVariables { get; set; } = new List<UVariableDeclaration>();
        [Rtti.Meta]
        public UCommentStatement Comment { get; set; }
        [Rtti.Meta]
        public bool IsOverride { get; set; } = false;

        public UExecuteSequenceStatement MethodBody = new UExecuteSequenceStatement();
        public int MethodSegmentDeep = 0;
        public bool ReturnHasGenerated = false;

        public UClassDeclaration HostClass;

        public void ResetRuntimeData()
        {
            MethodSegmentDeep = 0;
            ReturnHasGenerated = false;
        }

        string mKeyword = "";
        public string GetKeyword()
        {
            if (!string.IsNullOrEmpty(mKeyword))
                return mKeyword;
            mKeyword = ((ReturnValue != null) ? ReturnValue.VariableType.TypeFullName : "") + " " + MethodName + "(";
            for(int i=0; i<Arguments.Count; i++)
            {
                mKeyword += //Arguments[i].OperationType.ToString() + " " +
                          //(Arguments[i].IsParamArray ? "param " : "") +
                          Arguments[i].VariableType.TypeFullName + ",";
            }
            mKeyword = mKeyword.TrimEnd(',') + ")";
            return mKeyword;
        }

        public static string GetKeyword(System.Reflection.MethodInfo method)
        {
            var retStr = method.Name + "(";
            var parameters = method.GetParameters();
            for(int i=0; i<parameters.Length; i++)
            {
                retStr += UMethodArgumentDeclaration.GetOperationType(parameters[i]) + " ";
                retStr += UMethodArgumentDeclaration.GetIsParamArray(parameters[i]) ? "param " : "";
                retStr += parameters[i].ParameterType.FullName + ",";
            }
            retStr = retStr.TrimEnd(',') + ")";
            return retStr;
        }
        public static string GetKeyword(Rtti.UClassMeta.MethodMeta method)
        {
            var retStr = method.MethodName + "(";
            var parameters = method.Parameters;
            for(int i=0; i<parameters.Length; i++)
            {
                retStr += UMethodArgumentDeclaration.GetOperationType(parameters[i]) + " ";
                retStr += UMethodArgumentDeclaration.GetIsParamArray(parameters[i]) ? "param " : "";
                retStr += parameters[i].ParameterType.FullName + ",";
            }
            retStr = retStr.TrimEnd(',') + ")";
            return retStr;
        }

        public static UMethodDeclaration GetMethodDeclaration(System.Reflection.MethodInfo method)
        {
            var retVal = new UMethodDeclaration();
            retVal.IsOverride = true;
            if(method.ReturnType != typeof(void))
            {
                retVal.ReturnValue = new UVariableDeclaration()
                {
                    VariableType = new UTypeReference(method.ReturnType),
                };
            }
            retVal.MethodName = method.Name;
            var parameters = method.GetParameters();
            retVal.Arguments = new List<UMethodArgumentDeclaration>(parameters.Length);
            for(int paramIdx=0; paramIdx < parameters.Length; paramIdx++)
            {
                retVal.Arguments.Add(UMethodArgumentDeclaration.GetParam(parameters[paramIdx]));
            }
            return retVal;
        }
        public static UMethodDeclaration GetMethodDeclaration(Rtti.UClassMeta.MethodMeta method)
        {
            var retVal = new UMethodDeclaration();
            retVal.IsOverride = true;
            if(method.ReturnType.IsEqual(typeof(void)))
            {
                retVal.ReturnValue = new UVariableDeclaration()
                {
                    VariableType = new UTypeReference(method.ReturnType),
                };
            }
            retVal.MethodName = method.MethodName;
            var parameters = method.Parameters;
            retVal.Arguments = new List<UMethodArgumentDeclaration>(parameters.Length);
            for (int paramIdx = 0; paramIdx < parameters.Length; paramIdx++)
            {
                retVal.Arguments.Add(UMethodArgumentDeclaration.GetParam(parameters[paramIdx]));
            }
            return retVal;
        }

        public void OnPreRead(object tagObject, object hostObject, bool fromXml)
        {
        }

        public void OnPropertyRead(object tagObject, PropertyInfo prop, bool fromXml)
        {
        }

        public bool HasLocalVariable(string variableName)
        {
            for(int i=0; i<LocalVariables.Count; i++)
            {
                if (LocalVariables[i].VariableName == variableName)
                    return true;
            }
            return false;
        }

        public void AddLocalVar(UVariableDeclaration var)
        {
            LocalVariables.Add(var);
        }

        public override bool Equals(object obj)
        {
            var dec = obj as UMethodDeclaration;
            if (dec == null)
                return false;
            if (Arguments.Count != dec.Arguments.Count)
                return false;
            for(int i=0; i<Arguments.Count; i++)
            {
                if (!Arguments[i].Equals(dec.Arguments[i]))
                    return false;
            }
            return MethodName == dec.MethodName;
        }
        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }
        public override string ToString()
        {
            string retStr = MethodName + "(";
            for(int i=0; i<Arguments.Count; i++)
            {
                retStr += Arguments[i].ToString() + ",";
            }
            retStr = retStr.TrimEnd(',') + ")";
            return retStr;
        }
    }

    public class UNamespaceDeclaration : UCodeObject, IO.ISerializer
    {
        [Rtti.Meta]
        public string Namespace { get; set; } = "Unknow";
        [Rtti.Meta]
        public List<UClassDeclaration> Classes { get; set; } = new List<UClassDeclaration>();

        public UNamespaceDeclaration(string ns)
        {
            Namespace = ns;
        }
        public override bool Equals(object obj)
        {
            var ns = obj as UNamespaceDeclaration;
            if (ns == null)
                return false;
            return Namespace == ns.Namespace;
        }
        public override int GetHashCode()
        {
            return Namespace.GetHashCode();
        }

        public void OnPreRead(object tagObject, object hostObject, bool fromXml)
        {
        }

        public void OnPropertyRead(object tagObject, PropertyInfo prop, bool fromXml)
        {
        }

        public static bool operator !=(UNamespaceDeclaration lh, UNamespaceDeclaration rh)
        {
            return !(lh == rh);
        }
        public static bool operator ==(UNamespaceDeclaration lh, UNamespaceDeclaration rh)
        {
            if (lh is null)
                return rh is null;
            return lh.Equals(rh);
        }
        public override string ToString()
        {
            return Namespace;
        }
    }

    public class UClassDeclaration : UCodeObject, IO.ISerializer
    {
        [Rtti.Meta]
        public EVisisMode VisitMode { get; set; } = EVisisMode.Public;
        [Rtti.Meta]
        public bool IsStruct { get; set; } = false;
        [Rtti.Meta]
        public string ClassName { get; set; } = "Unknow";
        [Rtti.Meta]
        public List<string> SupperClassNames { get; set; } = new List<string>();
        [Rtti.Meta]
        public List<UVariableDeclaration> Properties { get; set; } = new List<UVariableDeclaration>();
        [Rtti.Meta]
        public List<UMethodDeclaration> Methods { get; set; } = new List<UMethodDeclaration>();

        public UNamespaceDeclaration Namespace;
        [Rtti.Meta]
        public UCommentStatement Comment { get; set; }

        public string GetFullName()
        {
            return ((Namespace != null) ? (Namespace.Namespace + ".") : "") + ClassName;
        }
        public Rtti.UTypeDesc TryGetTypeDesc()
        {
            return Rtti.UTypeDescManager.Instance.GetTypeDescFromFullName(GetFullName());
        }
        public void Reset()
        {
            VisitMode = EVisisMode.Public;
            ClassName = "Unknow";
            IsStruct = false;
            SupperClassNames = new List<string>();
            Properties = new List<UVariableDeclaration>();
            Methods = new List<UMethodDeclaration>();
            Namespace = null;
            Comment = null;
        }
        public void ClearMethods()
        {
            for(int i=0; i<Methods.Count; i++)
            {
                Methods[i].HostClass = null;
            }
            Methods.Clear();
        }
        public void AddMethod(UMethodDeclaration method)
        {
            if (!Methods.Contains(method))
                Methods.Add(method);
            method.HostClass = this;
        }
        public void RemoveMethod(UMethodDeclaration method)
        {
            method.HostClass = null;
            Methods.Remove(method);
        }

        public UVariableDeclaration FindMember(string name)
        {
            for(int i=0; i<Properties.Count; i++)
            {
                if (Properties[i].VariableName == name)
                    return Properties[i];
            }
            return null;
        }

        public void OnPreRead(object tagObject, object hostObject, bool fromXml)
        {
        }

        public void OnPropertyRead(object tagObject, PropertyInfo prop, bool fromXml)
        {
        }

        public override bool Equals(object obj)
        {
            var dec = obj as UClassDeclaration;
            if (dec == null)
                return false;
            if (Namespace != dec.Namespace)
                return false;
            if (ClassName != dec.ClassName)
                return false;
            return true;
        }
        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }
        public override string ToString()
        {
            return ((Namespace != null) ? Namespace.ToString() : "") + "." + ClassName;
        }
    }

    public class UClassReferenceExpression : UExpressionBase
    {
        public Rtti.UTypeDesc Class;
        public override bool Equals(object obj)
        {
            var cRef = obj as UClassReferenceExpression;
            if (cRef == null)
                return false;
            return Class.Equals(cRef.Class);
        }
        public override int GetHashCode()
        {
            return Class.GetHashCode();
        }
        public override string ToString()
        {
            return "ref(" + Class.ToString() + ")";
        }
    }

    public class UVariableReferenceExpression : UExpressionBase
    {
        public UExpressionBase Host;
        public string VariableName = "Unknow";
        public bool IsProperty = false;

        public UVariableReferenceExpression()
        {
        }
        public UVariableReferenceExpression(string name, UExpressionBase host = null)
        {
            Host = host;
            VariableName = name;
        }
        public override bool Equals(object obj)
        {
            var vRef = obj as UVariableReferenceExpression;
            if (vRef == null)
                return false;
            if (VariableName != vRef.VariableName)
                return false;
            if (Host == null && vRef.Host == null)
                return true;
            if (Host != null)
                return Host.Equals(vRef.Host);
            return false;
        }
        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }
        public override string ToString()
        {
            return ((Host != null) ? Host.ToString() : "") + "." + VariableName;
        }
    }

    public class USelfReferenceExpression : UExpressionBase    
    {
        public override bool Equals(object obj)
        {
            var val = obj as USelfReferenceExpression;
            if (val == null)
                return false;
            return true;
        }
        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }
        public override string ToString()
        {
            return "self";
        }
    }

    public class UMethodInvokeArgumentExpression : UExpressionBase
    {
        public UExpressionBase Expression;
        public EMethodArgumentAttribute OperationType = EMethodArgumentAttribute.Default;

        public override bool Equals(object obj)
        {
            var iArg = obj as UMethodInvokeArgumentExpression;
            if (iArg == null)
                return false;
            return Expression.Equals(iArg.Expression);
        }
        public override int GetHashCode()
        {
            return Expression.GetHashCode();
        }
        public override string ToString()
        {
            return "arg(" + Expression.ToString() + ")";
        }
    }

    public class UMethodInvokeStatement : UStatementBase
    {
        public UExpressionBase Host;
        public string MethodName = "Unknow";
        public List<UMethodInvokeArgumentExpression> Arguments = new List<UMethodInvokeArgumentExpression>();
        public UVariableDeclaration ReturnValue;

        public UMethodInvokeStatement() { }
        public UMethodInvokeStatement(string methodName, UVariableDeclaration retValue, UExpressionBase host, params UMethodInvokeArgumentExpression[] arguments)
        {
            MethodName = methodName;
            Host = host;
            ReturnValue = retValue;
            foreach(var item in arguments)
            {
                Arguments.Add(item);
            }
        }

        public override bool Equals(object obj)
        {
            var invoke = obj as UMethodInvokeStatement;
            if (invoke == null)
                return false;
            bool argsEqual = Arguments.Count == invoke.Arguments.Count;
            if (argsEqual == false)
                return false;
            for(int i=0; i<Arguments.Count; i++)
            {
                if (!Arguments[i].Equals(invoke.Arguments[i]))
                    return false;
            }
            if (MethodName != invoke.MethodName)
                return false;
            if (Host != null && !Host.Equals(invoke.Host))
                return false;
            if (Host == null && invoke.Host != null)
                return false;
            return true;
        }
        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }
        public override string ToString()
        {
            string retStr = "";
            if (Host != null)
                retStr = Host.ToString() + ".";
            retStr += MethodName + "(";
            for (int i = 0; i < Arguments.Count; i++)
            {
                retStr += Arguments[i].ToString() + ",";
            }
            retStr = retStr.TrimEnd(',') + ")";
            return retStr;
        }
    }

    public class UAssignOperatorStatement : UStatementBase
    {
        public UExpressionBase To;
        public UExpressionBase From;

        public override bool Equals(object obj)
        {
            var assign = obj as UAssignOperatorStatement;
            if (assign == null)
                return false;

            return (To.Equals(assign.To) && From.Equals(assign.From));
        }
        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }
        public override string ToString()
        {
            return To.ToString() + "=" + From.ToString();
        }
    }

    public class UBinaryOperatorExpression : UExpressionBase
    {
        public enum EBinaryOperation
        {
            Add,
            Subtract,
            Multiply,
            Divide,
            Modulus,
            Inequality,
            Equality,
            NotEquality,
            BitwiseOr,
            BitwiseXOR,
            BitwiseAnd,
            BitwiseLeftShift,
            BitwiseRightShift,
            BooleanOr,
            BooleanAnd,
            LessThan,
            LessThanOrEqual,
            GreaterThan,
            GreaterThanOrEqual,
        }
        public EBinaryOperation Operation = EBinaryOperation.Add;
        public UExpressionBase Left;
        public UExpressionBase Right;

        public override bool Equals(object obj)
        {
            var b = obj as UBinaryOperatorExpression;
            if (b == null)
                return false;
            return (Left.Equals(b.Left) &&
                    Right.Equals(b.Right) &&
                    (Operation == b.Operation));
        }
        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }
        public override string ToString()
        {
            return Left.ToString() + " " + Operation.ToString() + " " + Right.ToString();
        }
    }

    public class UUnaryOperatorExpression : UExpressionBase
    {
        public enum EUnaryOperation
        {
            Negative,
            BooleanNot,
            BitwiseNot,
        }
        public EUnaryOperation Operation = EUnaryOperation.Negative;
        public UExpressionBase Value;

        public override bool Equals(object obj)
        {
            var u = obj as UUnaryOperatorExpression;
            if (u == null)
                return false;
            return ((Operation == u.Operation) &&
                    Value.Equals(u.Value));
        }
        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }
        public override string ToString()
        {
            return Operation.ToString() + "(" + Value.ToString() + ")";
        }
    }

    public class UIndexerOperatorExpression : UExpressionBase
    {
        public UExpressionBase Target;
        public List<UExpressionBase> Indices = new List<UExpressionBase>();

        public override bool Equals(object obj)
        {
            var id = obj as UIndexerOperatorExpression;
            if (id == null)
                return false;
            if (Indices.Count != id.Indices.Count)
                return false;
            for(int i=0; i<Indices.Count; i++)
            {
                if (!Indices[i].Equals(id.Indices[i]))
                    return false;
            }
            return Target.Equals(id.Target);
        }
        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }
        public override string ToString()
        {
            var retVal = Target.ToString();
            for (int i = 0; i < Indices.Count; i++)
                retVal += "[" + Indices[i] + "]";
            return retVal;
        }
    }

    public class UPrimitiveExpression : UExpressionBase
    {
        protected Support.UAnyValue Value;
        protected System.Enum EnumValue = null;
        public Rtti.UTypeDesc Type;

        public override bool Equals(object obj)
        {
            var p = obj as UPrimitiveExpression;
            if (p == null)
                return false;
            return Value.IsEqual(p.Value) && (EnumValue == p.EnumValue);
        }
        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }
        public override string ToString()
        {
            if (EnumValue != null)
                return EnumValue.ToString();
            return Value.ToString();
        }

        public UPrimitiveExpression(Byte val)
        {
            Type = Rtti.UTypeDesc.TypeOf(typeof(Byte));
            Value.SetUI8(val);
        }
        public UPrimitiveExpression(UInt16 val)
        {
            Type = Rtti.UTypeDesc.TypeOf(typeof(UInt16));
            Value.SetUI16(val);
        }
        public UPrimitiveExpression(UInt32 val)
        {
            Type = Rtti.UTypeDesc.TypeOf(typeof(UInt32));
            Value.SetUI32(val);
        }
        public UPrimitiveExpression(UInt64 val)
        {
            Type = Rtti.UTypeDesc.TypeOf(typeof(UInt64));
            Value.SetUI64(val);
        }
        public UPrimitiveExpression(SByte val)
        {
            Type = Rtti.UTypeDesc.TypeOf(typeof(SByte));
            Value.SetI8(val);
        }
        public UPrimitiveExpression(Int16 val)
        {
            Type = Rtti.UTypeDesc.TypeOf(typeof(Int16));
            Value.SetI16(val);
        }
        public UPrimitiveExpression(Int32 val)
        {
            Type = Rtti.UTypeDesc.TypeOf(typeof(Int32));
            Value.SetI32(val);
        }
        public UPrimitiveExpression(Int64 val)
        {
            Type = Rtti.UTypeDesc.TypeOf(typeof(Int64));
            Value.SetI64(val);
        }
        public UPrimitiveExpression(float val)
        {
            Type = Rtti.UTypeDesc.TypeOf(typeof(float));
            Value.SetF32(val);
        }
        public UPrimitiveExpression(double val)
        {
            Type = Rtti.UTypeDesc.TypeOf(typeof(double));
            Value.SetF64(val);
        }
        public UPrimitiveExpression(string val)
        {
            Type = Rtti.UTypeDesc.TypeOf(typeof(string));
            Value.SetName(val);
        }
        public UPrimitiveExpression(bool val)
        {
            Type = Rtti.UTypeDesc.TypeOf(typeof(bool));
            Value.SetValue(val);
        }
        public UPrimitiveExpression(Vector2 val)
        {
            Type = Rtti.UTypeDesc.TypeOf(typeof(Vector2));
            Value.SetValue(val);
        }
        public UPrimitiveExpression(Vector3 val)
        {
            Type = Rtti.UTypeDesc.TypeOf(typeof(Vector3));
            Value.SetValue(val);
        }
        public UPrimitiveExpression(Vector4 val)
        {
            Type = Rtti.UTypeDesc.TypeOf(typeof(Vector4));
            Value.SetValue(val);
        }
        public UPrimitiveExpression(Rtti.UTypeDesc type, object value)
        {
            Type = type;
            if (type.IsEnum)
                EnumValue = (System.Enum)value;
            else if (type.IsEqual(typeof(SByte)))
                Value.SetValue((sbyte)value);
            else if (type.IsEqual(typeof(Int16)))
                Value.SetValue((Int16)value);
            else if (type.IsEqual(typeof(Int32)))
                Value.SetValue((Int32)value);
            else if (type.IsEqual(typeof(Int64)))
                Value.SetValue((Int64)value);
            else if (type.IsEqual(typeof(byte)))
                Value.SetValue((byte)value);
            else if (type.IsEqual(typeof(UInt16)))
                Value.SetValue((UInt16)value);
            else if (type.IsEqual(typeof(UInt32)))
                Value.SetValue((UInt32)value);
            else if (type.IsEqual(typeof(UInt64)))
                Value.SetValue((UInt64)value);
            else if (type.IsEqual(typeof(float)))
                Value.SetValue((float)value);
            else if (type.IsEqual(typeof(double)))
                Value.SetValue((double)value);
            else if (type.IsEqual(typeof(string)))
                Value.SetName((string)value);
            else if (type.IsEqual(typeof(Vector2)))
                Value.SetValue((Vector2)value);
            else if (type.IsEqual(typeof(Vector3)))
                Value.SetValue((Vector3)value);
            else if (type.IsEqual(typeof(Vector4)))
                Value.SetValue((Vector4)value);
        }

        public void SetValue<T>(T value) where T : unmanaged
        {
            Type = Rtti.UTypeDesc.TypeOf(typeof(T));
            Value.SetValue(value);
        }
        public void SetValue(string value)
        {
            Type = Rtti.UTypeDesc.TypeOf(typeof(string));
            Value.SetName(value);
        }
        public void GetValue<T>(ref T value) where T : unmanaged
        {
            Value.GetValue(ref value);
        }
        public string GetValueString()
        {
            if (EnumValue != null)
                return EnumValue.ToString();
            return Value.ToString();
        }
    }

    public class UCastExpression : UExpressionBase
    {
        public UTypeReference TargetType;
        public UExpressionBase Expression;

        public override bool Equals(object obj)
        {
            var ce = obj as UCastExpression;
            if (ce == null)
                return false;
            return TargetType.Equals(ce.TargetType) &&
                   Expression.Equals(ce.Expression);
        }
        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }
        public override string ToString()
        {
            return "(" + TargetType.ToString() + ")" + Expression.ToString();
        }
    }

    public class UCreateObjectExpression : UExpressionBase
    {
        public string TypeName;
        public List<UExpressionBase> Parameters = new List<UExpressionBase>();

        public override bool Equals(object obj)
        {
            var co = obj as UCreateObjectExpression;
            if (co == null)
                return false;
            if (Parameters.Count != co.Parameters.Count)
                return false;
            if (TypeName != co.TypeName)
                return false;
            for(int i=0; i<Parameters.Count; i++)
            {
                if (!Parameters[i].Equals(co.Parameters[i]))
                    return false;
            }
            return true;
        }
        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }
        public override string ToString()
        {
            var retVal = TypeName + "(";
            for (int i = 0; i < Parameters.Count; i++)
                retVal += Parameters[i].ToString() + ",";
            retVal = retVal.TrimEnd(',') + ")";
            return retVal;
        }
    }

    public class UDefaultValueExpression : UExpressionBase
    {
        public UTypeReference Type;
        public UDefaultValueExpression() { }
        public UDefaultValueExpression(Rtti.UTypeDesc type)
        {
            Type = new UTypeReference(type);
        }
        public UDefaultValueExpression(Type type)
        {
            Type = new UTypeReference(type);
        }
        public UDefaultValueExpression(UTypeReference type)
        {
            Type = type;
        }

        public override bool Equals(object obj)
        {
            var val = obj as UDefaultValueExpression;
            if (val == null)
                return false;
            return Type.Equals(val.Type);
        }
        public override int GetHashCode()
        {
            return Type.GetHashCode();
        }
        public override string ToString()
        {
            return "default " + Type.ToString();
        }
    }

    public class UNullValueExpression : UExpressionBase 
    {
        public override bool Equals(object obj)
        {
            var val = obj as UNullValueExpression;
            if (val == null)
                return false;
            return true;
        }
        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }
        public override string ToString()
        {
            return "null value";
        }
    }

    public class UExecuteSequenceStatement : UStatementBase
    {
        public List<UStatementBase> Sequence = new List<UStatementBase>();

        public override bool Equals(object obj)
        {
            var val = obj as UExecuteSequenceStatement;
            if (val == null)
                return false;
            if (Sequence.Count != val.Sequence.Count)
                return false;
            for(int i=0; i<Sequence.Count; i++)
            {
                if (!Sequence[i].Equals(val.Sequence[i]))
                    return false;
            }
            return true;
        }
        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }
        public override string ToString()
        {
            var retStr = "";
            for (int i = 0; i < Sequence.Count; i++)
                retStr += Sequence[i].ToString() + ",";
            retStr = retStr.TrimEnd(',');
            return retStr;
        }
    }
    
    public class UReturnStatement : UStatementBase 
    {
        public override bool Equals(object obj)
        {
            var val = obj as UReturnStatement;
            if (val == null)
                return false;
            return true;
        }
        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }
        public override string ToString()
        {
            return "return value";
        }
    }

    public class UIfStatement : UStatementBase
    {
        public UExpressionBase Condition;
        public UStatementBase TrueStatement;
        public UStatementBase FalseStatement;
        public List<UIfStatement> ElseIfs;

        public override bool Equals(object obj)
        {
            var val = obj as UIfStatement;
            if (val == null)
                return false;
            if (ElseIfs.Count != val.ElseIfs.Count)
                return false;
            for(int i=0; i<ElseIfs.Count; i++)
            {
                if (!ElseIfs[i].Equals(val.ElseIfs[i]))
                    return false;
            }
            return (Condition.Equals(val.Condition) &&
                    TrueStatement.Equals(val.TrueStatement) &&
                    FalseStatement.Equals(val.FalseStatement));
        }
        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }
        public override string ToString()
        {
            var retVal = "if(" + Condition.ToString() + ")";
            if (TrueStatement != null)
                retVal = "true=(" + TrueStatement.ToString() + ")";
            if(FalseStatement != null)
                retVal = "false=(" + FalseStatement.ToString() + ")";
            for(int i=0; i<ElseIfs.Count; i++)
                retVal += "else " + ElseIfs[i].ToString() + "; ";
            return retVal;
        }
    }

    public class UForLoopStatement : UStatementBase
    {
        public bool IncludeEnd = false;
        public string LoopIndexName;
        public UExpressionBase BeginExpression;
        public UExpressionBase EndExpression;
        public UExpressionBase StepExpression;
        public UStatementBase LoopBody;

        public override bool Equals(object obj)
        {
            var val = obj as UForLoopStatement;
            if (val == null)
                return false;

            return (IncludeEnd == val.IncludeEnd) &&
                   (LoopIndexName == val.LoopIndexName) &&
                   BeginExpression.Equals(val.BeginExpression) &&
                   EndExpression.Equals(val.EndExpression) &&
                   StepExpression.Equals(val.StepExpression) &&
                   LoopBody.Equals(val.LoopBody);
        }
        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }
        public override string ToString()
        {
            return "for(" + LoopIndexName + "=" + BeginExpression.ToString() +
                LoopIndexName + (IncludeEnd?"<=":"<") + EndExpression.ToString() + 
                LoopIndexName + "+=" + StepExpression.ToString() + ")\r\n{" +
                LoopBody.ToString() + "}";
        }
    }

    public class UWhileLoopStatement : UStatementBase
    {
        public UExpressionBase Condition;
        public UStatementBase LoopBody;

        public override bool Equals(object obj)
        {
            var val = obj as UWhileLoopStatement;
            if (val == null)
                return false;
            return Condition.Equals(val.Condition) && LoopBody.Equals(val.LoopBody);
        }
        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }
        public override string ToString()
        {
            return "while(" + Condition.ToString() + "){" + LoopBody.ToString() + "}";
        }
    }

    public class UContinueStatement : UStatementBase 
    {
        public override bool Equals(object obj)
        {
            var val = obj as UContinueStatement;
            return val != null;
        }
        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }
        public override string ToString()
        {
            return "continue statement";
        }
    }
    public class UBreakStatement : UStatementBase
    {
        public override bool Equals(object obj)
        {
            var val = obj as UBreakStatement;
            return val != null;
        }
        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }
        public override string ToString()
        {
            return "break statement";
        }
    }

    public class UCommentStatement : UStatementBase, IO.ISerializer
    {
        [Rtti.Meta]
        public string CommentString { get; set; }
        public UCommentStatement(string comment)
        {
            CommentString = comment;
        }

        public void OnPreRead(object tagObject, object hostObject, bool fromXml)
        {
        }

        public void OnPropertyRead(object tagObject, PropertyInfo prop, bool fromXml)
        {
        }

        public override bool Equals(object obj)
        {
            var val = obj as UCommentStatement;
            if (val == null)
                return false;
            return CommentString == val.CommentString;
        }
        public override int GetHashCode()
        {
            return CommentString.GetHashCode();
        }
        public override string ToString()
        {
            return "/*" + CommentString + "*/";
        }
    }
    public class UExpressionStatement : UStatementBase
    {
        public UExpressionBase Expression;
        public UStatementBase NextStatement;
        public UExpressionStatement(UExpressionBase exp)
        {
            Expression = exp;
        }

        public override bool Equals(object obj)
        {
            var val = obj as UExpressionStatement;
            if (val == null)
                return false;

            if (!Expression.Equals(val.Expression))
                return false;
            if (NextStatement != null && !NextStatement.Equals(val.NextStatement))
                return false;
            if (NextStatement == null && val.NextStatement != null)
                return false;
            return true;
        }
        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }
        public override string ToString()
        {
            return Expression.ToString() + ((NextStatement != null) ? NextStatement.ToString() : "");
        }
    }
    public class UTest_Expressions
    {
        public static UNamespaceDeclaration GetTestNamespace()
        {
            var ns = new UNamespaceDeclaration("ExpressionTest");
            var cls = new UClassDeclaration()
            {
                VisitMode = EVisisMode.Public,
                IsStruct = false,
                ClassName = "TestClass",
            };
            cls.SupperClassNames.Add("SuperClass");
            ns.Classes.Add(cls);

            var publicIntVar = new UVariableDeclaration()
            {
                VariableType = new UTypeReference(typeof(int)),
                VariableName = "PublicIntVal",
                InitValue = new UPrimitiveExpression(0),
                Comment = new UCommentStatement("this is a test int value"),
                VisitMode = EVisisMode.Public,
            };
            cls.Properties.Add(publicIntVar);
            var protectStringVar = new UVariableDeclaration()
            {
                VariableType = new UTypeReference(typeof(string)),
                VariableName = "ProtectStringVar",
                InitValue = new UPrimitiveExpression("string value"),
                Comment = new UCommentStatement("this is a test string value"),
                VisitMode = EVisisMode.Protected,
            };
            cls.Properties.Add(protectStringVar);
            var privateBooleanVar = new UVariableDeclaration()
            {
                VariableType = new UTypeReference(typeof(bool)),
                VariableName = "ProtectBooleanVar",
                InitValue = new UPrimitiveExpression(true),
                Comment = new UCommentStatement("this is a test boolean value"),
                VisitMode = EVisisMode.Protected,
            };
            cls.Properties.Add(privateBooleanVar);
            var localInt8Var = new UVariableDeclaration()
            {
                VariableType = new UTypeReference(typeof(sbyte)),
                VariableName = "LocalInt8Var",
                InitValue = new UPrimitiveExpression((sbyte)1),
                Comment = new UCommentStatement("this is a test Int8 value"),
                VisitMode = EVisisMode.Protected,
            };
            cls.Properties.Add(localInt8Var);
            var internalUInt8Var = new UVariableDeclaration()
            {
                VariableType = new UTypeReference(typeof(byte)),
                VariableName = "ProtectUInt8Var",
                InitValue = new UPrimitiveExpression((byte)2),
                Comment = new UCommentStatement("this is a test UInt8 value"),
                VisitMode = EVisisMode.Protected,
            };
            cls.Properties.Add(internalUInt8Var);
            var int16Var = new UVariableDeclaration()
            {
                VariableType = new UTypeReference(typeof(Int16)),
                VariableName = "Int16Var",
                InitValue = new UPrimitiveExpression((Int16)3),
            };
            cls.Properties.Add(int16Var);
            var int32Var = new UVariableDeclaration()
            {
                VariableType = new UTypeReference(typeof(Int32)),
                VariableName = "Int32Var",
                InitValue = new UPrimitiveExpression((Int32)4),
            };
            cls.Properties.Add(int32Var);
            var int64Var = new UVariableDeclaration()
            {
                VariableType = new UTypeReference(typeof(Int64)),
                VariableName = "Int64Var",
                InitValue = new UPrimitiveExpression((Int64)5),
            };
            cls.Properties.Add(int64Var);
            var uInt16Var = new UVariableDeclaration()
            {
                VariableType = new UTypeReference(typeof(UInt16)),
                VariableName = "UInt16Var",
                InitValue = new UPrimitiveExpression((UInt16)6),
            };
            cls.Properties.Add(uInt16Var);
            var uInt32Var = new UVariableDeclaration()
            {
                VariableType = new UTypeReference(typeof(UInt32)),
                VariableName = "UInt32Var",
                InitValue = new UPrimitiveExpression((UInt32)7),
            };
            cls.Properties.Add(uInt32Var);
            var uInt64Var = new UVariableDeclaration()
            {
                VariableType = new UTypeReference(typeof(UInt64)),
                VariableName = "UInt64Var",
                InitValue = new UPrimitiveExpression((UInt64)8),
            };
            cls.Properties.Add(uInt64Var);
            var floatVar = new UVariableDeclaration()
            {
                VariableType = new UTypeReference(typeof(float)),
                VariableName = "FloatVar",
                InitValue = new UPrimitiveExpression((float)8),
            };
            cls.Properties.Add(floatVar);
            var doubleVar = new UVariableDeclaration()
            {
                VariableType = new UTypeReference(typeof(double)),
                VariableName = "DoubleVar",
                InitValue = new UPrimitiveExpression((double)8),
            };
            cls.Properties.Add(doubleVar);

            var protectedMethod = new UMethodDeclaration()
            {
                VisitMode = EVisisMode.Protected,
                MethodName = "ProtectedMethodTest",
                ReturnValue = int32Var,
                Comment = new UCommentStatement("this is protected method test"),
                IsOverride = false,
            };
            cls.AddMethod(protectedMethod);
            var privateMethod = new UMethodDeclaration()
            {
                VisitMode = EVisisMode.Private,
                MethodName = "PrivateMethodTest",
                ReturnValue = floatVar,
                Comment = new UCommentStatement("this is private method test"),
                IsOverride = false,
            };
            cls.AddMethod(privateMethod);
            var publicMethod = new UMethodDeclaration()
            {
                VisitMode = EVisisMode.Public,
                MethodName = "PublicMethodWithArgsTest",
                Comment = new UCommentStatement("this is public method test"),
                ReturnValue = int32Var,
                IsOverride = false,
            };
            cls.AddMethod(publicMethod);
            publicMethod.Arguments.Add(new UMethodArgumentDeclaration()
            {
                VariableType = new UTypeReference(typeof(byte)),
                VariableName = "byteArg",
                InitValue = new UPrimitiveExpression((byte)1),
                OperationType = EMethodArgumentAttribute.Default,
                IsParamArray = false,
            });
            publicMethod.Arguments.Add(new UMethodArgumentDeclaration()
            {
                VariableType = new UTypeReference(typeof(sbyte)),
                VariableName = "sbyteArg",
                InitValue = new UPrimitiveExpression((sbyte)-1),
                OperationType = EMethodArgumentAttribute.In,
                IsParamArray = false,
            });
            publicMethod.Arguments.Add(new UMethodArgumentDeclaration()
            {
                VariableType = new UTypeReference(typeof(Int16)),
                VariableName = "int16Arg",
                InitValue = new UPrimitiveExpression((Int16)(-2)),
                OperationType = EMethodArgumentAttribute.Out,
                IsParamArray = false,
            });
            publicMethod.Arguments.Add(new UMethodArgumentDeclaration()
            {
                VariableType = new UTypeReference(typeof(UInt16)),
                VariableName = "uint16Arg",
                InitValue = new UPrimitiveExpression((UInt16)2),
                OperationType = EMethodArgumentAttribute.Ref,
                IsParamArray = false,
            });
            publicMethod.Arguments.Add(new UMethodArgumentDeclaration()
            {
                VariableType = new UTypeReference(typeof(int[])),
                VariableName = "paramArg",
                OperationType = EMethodArgumentAttribute.Default,
                IsParamArray = true,
            });

            var methodInvoke = new UMethodInvokeStatement()
            {
                Host = new USelfReferenceExpression(),
                MethodName = "PublicMethodWithArgsTest",
            };
            methodInvoke.ReturnValue = int32Var;
            methodInvoke.Arguments.Add(new UMethodInvokeArgumentExpression()
            {
                Expression = new UPrimitiveExpression((byte)10),
            });
            methodInvoke.Arguments.Add(new UMethodInvokeArgumentExpression()
            {
                Expression = new UPrimitiveExpression((sbyte)(-10)),
                OperationType = EMethodArgumentAttribute.In,
            });
            methodInvoke.Arguments.Add(new UMethodInvokeArgumentExpression()
            {
                Expression = new UPrimitiveExpression((Int16)(-20)),
                OperationType = EMethodArgumentAttribute.Out,
            });
            methodInvoke.Arguments.Add(new UMethodInvokeArgumentExpression()
            {
                Expression = new UPrimitiveExpression((UInt16)20),
                OperationType = EMethodArgumentAttribute.Ref,
            });
            methodInvoke.Arguments.Add(new UMethodInvokeArgumentExpression()
            {
                Expression = new UPrimitiveExpression(0),
            });
            methodInvoke.Arguments.Add(new UMethodInvokeArgumentExpression()
            {
                Expression = new UPrimitiveExpression(1),
            });
            methodInvoke.Arguments.Add(new UMethodInvokeArgumentExpression()
            {
                Expression = new UPrimitiveExpression(2),
            });
            var methodInvokeStatement = methodInvoke;
            publicMethod.MethodBody.Sequence.Add(methodInvokeStatement);

            return ns;
        }

        public void UnitTestEntrance()
        {
            GetTestNamespace();
        }
    }
}