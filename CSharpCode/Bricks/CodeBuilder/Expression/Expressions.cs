using EngineNS.Bricks.NodeGraph;
using EngineNS.EGui.Controls.PropertyGrid;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
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

    public class UCodeObject : IO.ISerializer
    {
        public virtual void OnPreRead(object tagObject, object hostObject, bool fromXml)
        {
        }

        public virtual void OnPropertyRead(object tagObject, PropertyInfo prop, bool fromXml)
        {
        }
    }

    public class UExpressionBase : UCodeObject { }
    public class UStatementBase : UCodeObject
    {
        public UStatementBase Next;
    }
    public class UTypeReference : IO.ISerializer
    {
        string mTypeFullName;
        [Rtti.Meta(Order = 1)]
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
                if (mTypeDesc == null)
                    mTypeDesc = Rtti.UTypeDesc.TypeOfFullName(value);
                else if (mTypeDesc.FullName != value)
                    mTypeDesc = Rtti.UTypeDesc.TypeOfFullName(value);
                mTypeFullName = value;
            }
        }
        Rtti.UTypeDesc mTypeDesc;
        [Rtti.Meta(Order = 0)]
        public Rtti.UTypeDesc TypeDesc
        {
            get => mTypeDesc;
            set => mTypeDesc = value;
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

        public bool IsPointer
        {
            get
            {
                if (mTypeDesc != null)
                    return mTypeDesc.IsPointer;
                return false;
            }
        }

        public bool IsTask => (mTypeDesc.IsEqual(typeof(System.Threading.Tasks.Task)) || mTypeDesc.IsSubclassOf(typeof(System.Threading.Tasks.Task)));

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

            return typeR.TypeFullName == TypeFullName;
        }
        public override int GetHashCode()
        {
            return TypeFullName.GetHashCode();
        }
        public override string ToString()
        {
            return TypeFullName;
        }

        public bool IsEqual(Type type)
        {
            if (mTypeDesc != null)
                return mTypeDesc.IsEqual(type);
            return TypeFullName == type.FullName;
        }
        public virtual void OnPreRead(object tagObject, object hostObject, bool fromXml)
        {
        }

        public virtual void OnPropertyRead(object tagObject, PropertyInfo prop, bool fromXml)
        {
        }
    }

    public class UDebuggerSetWatchVariable : UStatementBase
    {
        [Rtti.Meta]
        public UTypeReference VariableType { get; set; }
        [Rtti.Meta]
        public string VariableName { get; set; }
        [Rtti.Meta]
        public UExpressionBase VariableValue { get; set; }

        public override bool Equals(object obj)
        {
            var w = obj as UDebuggerSetWatchVariable;
            if (w == null)
                return false;
            return (VariableName == w.VariableName) &&
                   (VariableValue == w.VariableValue) &&
                   (VariableType == w.VariableType);
        }
        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }
        public override string ToString()
        {
            return "debugger:" + VariableType.TypeFullName + " " + VariableName + " " + VariableValue.ToString();
        }
    }
    public class UDebuggerTryBreak : UStatementBase
    {
        [Rtti.Meta]
        public string BreakName { get; set; }

        public UDebuggerTryBreak(string name)
        {
            BreakName = name;
        }

        public override bool Equals(object obj)
        {
            var b = obj as UDebuggerTryBreak;
            if (b == null)
                return false;
            return (BreakName == b.BreakName);
        }
        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }
        public override string ToString()
        {
            return "debugger:breaker_" + BreakName;
        }
    }
    public class UVariableDeclaration : UStatementBase, IO.ISerializer, EGui.Controls.PropertyGrid.IPropertyCustomization, NodeGraph.UEditableValue.IValueEditNotify
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

        [Browsable(false)]
        public bool IsPropertyVisibleDirty { get; set; } = false;

        public UVariableDeclaration()
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

        public void GetProperties(ref EngineNS.EGui.Controls.PropertyGrid.CustomPropertyDescriptorCollection collection, bool parentIsValueType)
        {
            var pros = TypeDescriptor.GetProperties(this);
            var thisType = Rtti.UTypeDesc.TypeOf(this.GetType());
            foreach(PropertyDescriptor prop in pros)
            {
                var proDesc = EGui.Controls.PropertyGrid.PropertyCollection.PropertyDescPool.QueryObjectSync();
                proDesc.InitValue(this, thisType, prop, parentIsValueType);
                switch(proDesc.Name)
                {
                    case "VariableType":
                        {
                            List<Rtti.UTypeDesc> types = new List<Rtti.UTypeDesc>(200);
                            proDesc.PropertyType = Rtti.UTypeDesc.TypeOf(typeof(System.Type));
                            foreach(var service in Rtti.UTypeDescManager.Instance.Services.Values)
                            {
                                foreach(var type in service.Types.Values)
                                {
                                    types.Add(type);
                                }
                            }
                            proDesc.CustomValueEditor = new EGui.Controls.PropertyGrid.PGTypeEditorAttribute(types.ToArray());
                        }
                        break;
                    case "InitValue":
                        {
                            if(VariableType.TypeDesc == null)
                            {
                                proDesc.ReleaseObject();
                                continue;
                            }
                            var editor = NodeGraph.UEditableValue.CreateEditableValue(this, VariableType.TypeDesc, proDesc);
                            if(editor == null)
                            {
                                proDesc.ReleaseObject();
                                continue;
                            }
                            proDesc.PropertyType = VariableType.TypeDesc;
                            proDesc.CustomValueEditor = editor;
                        }
                        break;
                    case "Comment":
                        {
                            proDesc.PropertyType = Rtti.UTypeDesc.TypeOf(typeof(System.String));
                        }
                        break;
                }
                collection.Add(proDesc);
            }
        }

        public object GetPropertyValue(string propertyName)
        {
            switch(propertyName)
            {
                case "VariableType":
                    return VariableType.TypeDesc;
                case "InitValue":
                    {
                        var pe = InitValue as UPrimitiveExpression;
                        if(pe != null)
                            return pe.GetValue();
                    }
                    break;
                case "Comment":
                    return Comment?.CommentString;
                default:
                    {
                        var proInfo = GetType().GetProperty(propertyName);
                        if (proInfo != null)
                            return proInfo.GetValue(this);
                    }
                    break;
            }
            return null;
        }

        public void SetPropertyValue(string propertyName, object value)
        {
            switch(propertyName)
            {
                case "VariableType":
                    {
                        var tagType = value as Rtti.UTypeDesc;
                        if(tagType != VariableType.TypeDesc)
                        {
                            InitValue = new UPrimitiveExpression(tagType, tagType.IsValueType ? Rtti.UTypeDescManager.CreateInstance(tagType) : null);
                            VariableType.TypeDesc = tagType;
                        }
                    }
                    break;
                case "InitValue":
                    {
                        var pe = InitValue as UPrimitiveExpression;
                        if(pe != null)
                            pe.ValueStr = UPrimitiveExpression.CalculateValueString(pe.Type, value);
                    }
                    break;
                case "Comment":
                    {
                        Comment.CommentString = (string)value;
                    }
                    break;
                default:
                    {
                        var proInfo = GetType().GetProperty(propertyName);
                        if(proInfo != null)
                            proInfo.SetValue(this, value);
                    }
                    break;
            }
        }

        public void OnValueChanged(UEditableValue ev)
        {
            var pe = InitValue as UPrimitiveExpression;
            if(pe != null)
            {
                pe.ValueStr = UPrimitiveExpression.CalculateValueString(pe.Type, ev.Value);
            }
        }
    }

    public class TtIncludeDeclaration : UStatementBase, IO.ISerializer
    {
        [Rtti.Meta]
        public string FilePath { get; set; }
    }

    public class UMethodArgumentDeclaration : UCodeObject, IO.ISerializer, EGui.Controls.PropertyGrid.IPropertyCustomization, NodeGraph.UEditableValue.IValueEditNotify
    {
        [Browsable(false)]
        public bool IsPropertyVisibleDirty { get; set; } = false;

        bool mOperationVisible = true;
        [Browsable(false)]
        [Rtti.Meta]
        public bool OperationVisible 
        {
            get => mOperationVisible;
            set
            {
                mOperationVisible = value;
                IsPropertyVisibleDirty = true;
            }
        }
        bool mInitValueVisible = true;
        [Browsable(false)]
        [Rtti.Meta]
        public bool InitValueVisible 
        {
            get => mInitValueVisible;
            set
            {
                mInitValueVisible = value;
                IsPropertyVisibleDirty = true;
            }
        }

        public Rtti.MetaParameterAttribute Meta;

        [Rtti.Meta]
        public UTypeReference VariableType { get; set; } = new UTypeReference(Rtti.UTypeDesc.TypeOf<int>());
        public Action<string, string> OnVariableNameChanged = null;
        string mVariableName = "NewValue";
        [Rtti.Meta]
        [VariableName]
        public string VariableName 
        {
            get => mVariableName;
            set
            {
                var oldName = mVariableName;
                mVariableName = value;
                OnVariableNameChanged?.Invoke(oldName, value);
            }
        }
        public delegate string Delegate_GetErrorString(in PGCustomValueEditorAttribute.EditorInfo info, UMethodArgumentDeclaration dec, object newValue);
        public Delegate_GetErrorString GetErrorStringAction;
        class VariableNameAttribute : EGui.Controls.PropertyGrid.PGCustomValueEditorAttribute
        {
            public VariableNameAttribute()
            {
                UserDraw = false;
            }
            public override string GetErrorString<T>(in EditorInfo info, T newValue)
            {
                var maDec = info.ObjectInstance as UMethodArgumentDeclaration;
                return maDec?.GetErrorStringAction?.Invoke(in info, maDec, newValue);
            }
        }
        [Rtti.Meta]
        public UExpressionBase InitValue { get; set; } = new UPrimitiveExpression(Rtti.UTypeDesc.TypeOf<int>(), 0);
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

        public override bool Equals(object obj)
        {
            var arg = obj as UMethodArgumentDeclaration;
            if (arg == null)
                return false;
            return (VariableType.Equals(arg.VariableType) &&
                    VariableName == arg.VariableName &&
                    IsParamArray == arg.IsParamArray &&
                    InitValue == arg.InitValue);
        }
        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }
        public override string ToString()
        {
            return OperationType.ToString() + " " + IsParamArray.ToString() + " " + VariableType.TypeFullName + " " + VariableName;
        }

        public void GetProperties(ref CustomPropertyDescriptorCollection collection, bool parentIsValueType)
        {
            var pros = TypeDescriptor.GetProperties(this);
            var thisType = Rtti.UTypeDesc.TypeOf(this.GetType());
            foreach(PropertyDescriptor prop in pros)
            {
                var proDesc = EGui.Controls.PropertyGrid.PropertyCollection.PropertyDescPool.QueryObjectSync();
                proDesc.InitValue(this, thisType, prop, parentIsValueType);
                if (!proDesc.IsBrowsable)
                    continue;
                switch(proDesc.Name)
                {
                    case "VariableType":
                        {
                            var types = new List<Rtti.UTypeDesc>(200);
                            proDesc.PropertyType = Rtti.UTypeDesc.TypeOf(typeof(System.Type));
                            foreach(var service in Rtti.UTypeDescManager.Instance.Services.Values)
                            {
                                foreach(var type in service.Types.Values)
                                {
                                    types.Add(type);
                                }
                            }
                            proDesc.CustomValueEditor = new EGui.Controls.PropertyGrid.PGTypeEditorAttribute(types.ToArray());
                            collection.Add(proDesc);
                        }
                        break;
                    case "InitValue":
                        if(InitValueVisible)
                        {
                            if (VariableType.TypeDesc == null)
                            {
                                proDesc.ReleaseObject();
                                continue;
                            }
                            var editor = NodeGraph.UEditableValue.CreateEditableValue(this, VariableType.TypeDesc, proDesc);
                            if (editor == null)
                            {
                                proDesc.ReleaseObject();
                                continue;
                            }
                            proDesc.PropertyType = VariableType.TypeDesc;
                            proDesc.CustomValueEditor = editor;
                            collection.Add(proDesc);
                        }
                        break;
                    case "OperationType":
                        if(OperationVisible)
                        {
                            collection.Add(proDesc);
                        }
                        break;
                    default:
                        collection.Add(proDesc);
                        break;
                }
            }
            IsPropertyVisibleDirty = false;
        }

        public object GetPropertyValue(string propertyName)
        {
            switch(propertyName)
            {
                case "VariableType":
                    return VariableType.TypeDesc;
                case "InitValue":
                    {
                        var pe = InitValue as UPrimitiveExpression;
                        if (pe != null)
                            return pe.GetValue();
                    }
                    break;
                default:
                    {
                        var proInfo = GetType().GetProperty(propertyName);
                        if (proInfo != null)
                            return proInfo.GetValue(this);
                    }
                    break;
            }
            return null;
        }

        public void SetPropertyValue(string propertyName, object value)
        {
            switch(propertyName)
            {
                case "VariableType":
                    {
                        var tagType = value as Rtti.UTypeDesc;
                        if(tagType != VariableType.TypeDesc)
                        {
                            InitValue = new UPrimitiveExpression(tagType, tagType.IsValueType ? Rtti.UTypeDescManager.CreateInstance(tagType) : null);
                            VariableType.TypeDesc = tagType;
                        }
                    }
                    break;
                case "InitValue":
                    {
                        var pe = InitValue as UPrimitiveExpression;
                        if (pe != null)
                            pe.ValueStr = UPrimitiveExpression.CalculateValueString(pe.Type, value);
                    }
                    break;
                default:
                    {
                        var proInfo = GetType().GetProperty(propertyName);
                        if (proInfo != null)
                            proInfo.SetValue(this, value);
                    }
                    break;
            }
        }
        public void OnValueChanged(UEditableValue ev)
        {
            var pe = InitValue as UPrimitiveExpression;
            if (pe != null)
            {
                pe.ValueStr = UPrimitiveExpression.CalculateValueString(pe.Type, ev.Value);
            }
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
        [Rtti.Meta]
        public bool IsAsync { get; set; } = false;

        public bool IsUnsafe
        {
            get
            {
                if (ReturnValue != null && ReturnValue.VariableType.IsPointer)
                    return true;
                for(int i=0; i<Arguments.Count; i++)
                {
                    if (Arguments[i].VariableType.IsPointer)
                        return true;
                }
                return false;
            }
        }

        public UExecuteSequenceStatement MethodBody = new UExecuteSequenceStatement();
        public int MethodSegmentDeep = 0;
        public bool ReturnHasGenerated = false;
        public bool HasUnsafeCode = false;
        public bool HasAwaitCode = false;

        public UClassDeclaration HostClass;

        public void ResetRuntimeData()
        {
            MethodSegmentDeep = 0;
            ReturnHasGenerated = false;
            HasUnsafeCode = false;
            HasAwaitCode = false;
        }

        public string GetKeyword()
        {
            var keyword = ((ReturnValue != null) ? ReturnValue.VariableType.TypeFullName : "") + " " + MethodName + "(";
            for(int i=0; i<Arguments.Count; i++)
            {
                keyword += //Arguments[i].OperationType.ToString() + " " +
                          //(Arguments[i].IsParamArray ? "param " : "") +
                          Arguments[i].VariableType.TypeFullName + ",";
            }
            keyword = keyword.TrimEnd(',') + ")";
            return keyword;
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
            if ((method.ReturnType != typeof(void)) && (method.ReturnType != typeof(System.Threading.Tasks.Task)))
            {
                var retType = method.ReturnType;
                if (method.ReturnType.BaseType == typeof(System.Threading.Tasks.Task))
                {
                    retType = method.ReturnType.GetGenericArguments()[0];
                    retVal.IsAsync = true;
                }

                retVal.ReturnValue = new UVariableDeclaration()
                {
                    VariableType = new UTypeReference(retType),
                    InitValue = new UDefaultValueExpression(retType),
                    VariableName = "ret_" + (UInt32)Guid.NewGuid().ToString().GetHashCode(),
                };
            }
            else if (method.ReturnType.BaseType == typeof(System.Threading.Tasks.Task))
                retVal.IsAsync = true;

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
            if(!method.ReturnType.IsEqual(typeof(void)) && !method.ReturnType.IsEqual(typeof(System.Threading.Tasks.Task)))
            {
                var retType = method.ReturnType;
                if(retType.IsSubclassOf(typeof(System.Threading.Tasks.Task)))
                {
                    retType = Rtti.UTypeDesc.TypeOf(method.ReturnType.GetGenericArguments()[0]);
                    retVal.IsAsync = true;
                }
                retVal.ReturnValue = new UVariableDeclaration()
                {
                    VariableType = new UTypeReference(retType),
                    InitValue = new UDefaultValueExpression(retType),
                    VariableName = "ret_" + (UInt32)Guid.NewGuid().ToString().GetHashCode(),
                };
            }
            else if(method.ReturnType.IsEqual(typeof(System.Threading.Tasks.Task)))
                retVal.IsAsync = true;

            retVal.MethodName = method.MethodName;
            var parameters = method.Parameters;
            retVal.Arguments = new List<UMethodArgumentDeclaration>(parameters.Length);
            for (int paramIdx = 0; paramIdx < parameters.Length; paramIdx++)
            {
                retVal.Arguments.Add(UMethodArgumentDeclaration.GetParam(parameters[paramIdx]));
            }
            return retVal;
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

        public List<UVariableDeclaration> PreDefineVariables = new List<UVariableDeclaration>();

        public List<TtIncludeDeclaration> PreIncludeHeads = new List<TtIncludeDeclaration>();
        public void PushPreInclude(string file)
        {
            foreach (var i in PreIncludeHeads)
            {
                if(i.FilePath == file)
                {
                    return;
                }
            }
            PreIncludeHeads.Add(new TtIncludeDeclaration()
            {
                FilePath = file,
            });
        }

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
            PreDefineVariables.Clear();
        }
        public void ResetRuntimeData()
        {
            PreDefineVariables.Clear();
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

    public class UClassReferenceExpression : UExpressionBase, IO.ISerializer
    {
        [Rtti.Meta]
        public Rtti.UTypeDesc Class { get; set; }
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
            return "ref(" + Class?.ToString() + ")";
        }
    }

    public class UVariableReferenceExpression : UExpressionBase, IO.ISerializer
    {
        [Rtti.Meta]
        public UExpressionBase Host { get; set; }
        [Rtti.Meta]
        public string VariableName { get; set; } = "Unknow";
        [Rtti.Meta]
        public bool IsProperty { get; set; } = false;

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

    public class UMethodInvokeArgumentExpression : UExpressionBase, IO.ISerializer
    {
        [Rtti.Meta]
        public UExpressionBase Expression;// { get; set; }
        [Rtti.Meta]
        public EMethodArgumentAttribute OperationType { get; set; } = EMethodArgumentAttribute.Default;

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

    public class UMethodInvokeStatement : UStatementBase, IO.ISerializer
    {
        [Rtti.Meta]
        public UExpressionBase Host { get; set; }
        [Rtti.Meta]
        public string MethodName { get; set; } = "Unknow";
        [Rtti.Meta]
        public List<UMethodInvokeArgumentExpression> Arguments { get; set; } = new List<UMethodInvokeArgumentExpression>();
        [Rtti.Meta]
        public UVariableDeclaration ReturnValue { get; set; }
        [Rtti.Meta]
        public bool ForceCastReturnType { get; set; } = false;
        [Rtti.Meta]
        public bool IsAsync { get; set; } = false;
        [Rtti.Meta]
        public bool IsUnsafe { get; set; } = false;
        
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

    public class ULambdaExpression : UExpressionBase, IO.ISerializer
    {
        [Rtti.Meta]
        public UTypeReference ReturnType { get; set; }
        [Rtti.Meta]
        public List<UMethodInvokeArgumentExpression> LambdaArguments { get; set; } = new List<UMethodInvokeArgumentExpression>();
        [Rtti.Meta]
        public UMethodInvokeStatement MethodInvoke { get; set; }
        [Rtti.Meta]
        public bool IsAsync { get; set; } = false;

    }

    public class UAssignOperatorStatement : UStatementBase, IO.ISerializer
    {
        [Rtti.Meta]
        public UExpressionBase To { get; set; }
        [Rtti.Meta]
        public UExpressionBase From { get; set; }

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

    public class UBinaryOperatorExpression : UExpressionBase, IO.ISerializer
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
        [Rtti.Meta]
        public EBinaryOperation Operation { get; set; } = EBinaryOperation.Add;
        [Rtti.Meta]
        public UExpressionBase Left { get; set; }
        [Rtti.Meta]
        public UExpressionBase Right { get; set; }

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

    public class UUnaryOperatorExpression : UExpressionBase, IO.ISerializer
    {
        public enum EUnaryOperation
        {
            Negative,
            BooleanNot,
            BitwiseNot,
        }
        [Rtti.Meta]
        public EUnaryOperation Operation { get; set; } = EUnaryOperation.Negative;
        [Rtti.Meta]
        public UExpressionBase Value { get; set; }

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

    public class UIndexerOperatorExpression : UExpressionBase, IO.ISerializer
    {
        [Rtti.Meta]
        public UExpressionBase Target { get; set; }
        [Rtti.Meta]
        public List<UExpressionBase> Indices { get; set; } = new List<UExpressionBase>();

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

    public class UPrimitiveExpression : UExpressionBase, IO.ISerializer
    {
        [Rtti.Meta(Order = 0)]
        public Rtti.UTypeDesc Type { get; set; }
        string mValueStr;
        [Rtti.Meta]
        public string ValueStr 
        {
            get => mValueStr;
            set => mValueStr = value;
        }

        public override bool Equals(object obj)
        {
            var p = obj as UPrimitiveExpression;
            if (p == null)
                return false;
            return (mValueStr == p.mValueStr);
        }
        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }
        public override string ToString()
        {
            return mValueStr;
        }

        public UPrimitiveExpression(Byte val)
        {
            Type = Rtti.UTypeDesc.TypeOf(typeof(Byte));
            mValueStr = val.ToString();
        }
        public UPrimitiveExpression(UInt16 val)
        {
            Type = Rtti.UTypeDesc.TypeOf(typeof(UInt16));
            mValueStr = val.ToString();
        }
        public UPrimitiveExpression(UInt32 val)
        {
            Type = Rtti.UTypeDesc.TypeOf(typeof(UInt32));
            mValueStr = val.ToString();
        }
        public UPrimitiveExpression(UInt64 val)
        {
            Type = Rtti.UTypeDesc.TypeOf(typeof(UInt64));
            mValueStr = val.ToString();
        }
        public UPrimitiveExpression(SByte val)
        {
            Type = Rtti.UTypeDesc.TypeOf(typeof(SByte));
            mValueStr = val.ToString();
        }
        public UPrimitiveExpression(Int16 val)
        {
            Type = Rtti.UTypeDesc.TypeOf(typeof(Int16));
            mValueStr = val.ToString();
        }
        public UPrimitiveExpression(Int32 val)
        {
            Type = Rtti.UTypeDesc.TypeOf(typeof(Int32));
            mValueStr = val.ToString();
        }
        public UPrimitiveExpression(Int64 val)
        {
            Type = Rtti.UTypeDesc.TypeOf(typeof(Int64));
            mValueStr = val.ToString();
        }
        public UPrimitiveExpression(float val)
        {
            Type = Rtti.UTypeDesc.TypeOf(typeof(float));
            mValueStr = val.ToString();
        }
        public UPrimitiveExpression(double val)
        {
            Type = Rtti.UTypeDesc.TypeOf(typeof(double));
            mValueStr = val.ToString();
        }
        public UPrimitiveExpression(string val)
        {
            Type = Rtti.UTypeDesc.TypeOf(typeof(string));
            mValueStr = val;
        }
        public UPrimitiveExpression(bool val)
        {
            Type = Rtti.UTypeDesc.TypeOf(typeof(bool));
            mValueStr = val.ToString();
            if (val)
                mValueStr = "true";
            else
                mValueStr = "false";
        }
        public UPrimitiveExpression(Vector2 val)
        {
            Type = Rtti.UTypeDesc.TypeOf(typeof(Vector2));
            mValueStr = val.ToString();
        }
        public UPrimitiveExpression(Vector3 val)
        {
            Type = Rtti.UTypeDesc.TypeOf(typeof(Vector3));
            mValueStr = val.ToString();
        }
        public UPrimitiveExpression(Vector4 val)
        {
            Type = Rtti.UTypeDesc.TypeOf(typeof(Vector4));
            mValueStr = val.ToString();
        }
        public UPrimitiveExpression(Vector2i val)
        {
            Type = Rtti.UTypeDesc.TypeOf(typeof(Vector2i));
            mValueStr = val.ToString();
        }
        public UPrimitiveExpression(Vector3i val)
        {
            Type = Rtti.UTypeDesc.TypeOf(typeof(Vector3i));
            mValueStr = val.ToString();
        }
        public UPrimitiveExpression(Vector4i val)
        {
            Type = Rtti.UTypeDesc.TypeOf(typeof(Vector4i));
            mValueStr = val.ToString();
        }
        public UPrimitiveExpression(Rtti.UTypeDesc type, object value)
        {
            Type = type;
            ValueStr = CalculateValueString(type, value);
        }
        public static string CalculateValueString(Rtti.UTypeDesc type, object value)
        {
            string retValue;
            if (value == null)
            {
                retValue = "null";
            }
            else if (type == Rtti.UTypeDescGetter<bool>.TypeDesc)
            {
                var v = (bool)value;
                retValue = v ? "true" : "false";
            }
            else if (type == Rtti.UTypeDescGetter<RName>.TypeDesc)
            {
                var v = (RName)value;
                retValue = $"EngineNS.RName.GetRName(\"{v.Name}\", EngineNS.RName.ERNameType.{v.RNameType})";
            }
            else if(type == Rtti.UTypeDescGetter<System.Type>.TypeDesc)
            {
                var typeDesc = (Rtti.UTypeDesc)value;
                retValue = $"typeof({typeDesc.FullName})";
            }
            else
            {
                retValue = value.ToString();
            }
            return retValue;
        }
        public object GetValue()
        {
            if (Type.IsEqual(typeof(Byte)))
                return System.Convert.ToByte(ValueStr);
            else if (Type.IsEqual(typeof(UInt16)))
                return System.Convert.ToUInt16(ValueStr);
            else if (Type.IsEqual(typeof(UInt32)))
                return System.Convert.ToUInt32(ValueStr);
            else if (Type.IsEqual(typeof(UInt64)))
                return System.Convert.ToUInt64(ValueStr);
            else if (Type.IsEqual(typeof(SByte)))
                return System.Convert.ToSByte(ValueStr);
            else if (Type.IsEqual(typeof(Int16)))
                return System.Convert.ToInt16(ValueStr);
            else if (Type.IsEqual(typeof(Int32)))
                return System.Convert.ToInt32(ValueStr);
            else if (Type.IsEqual(typeof(Int64)))
                return System.Convert.ToInt64(ValueStr);
            else if (Type.IsEqual(typeof(float)))
                return System.Convert.ToSingle(ValueStr);
            else if (Type.IsEqual(typeof(double)))
                return System.Convert.ToDouble(ValueStr);
            else if (Type.IsEqual(typeof(string)))
                return ValueStr;
            else if(Type.IsEqual(typeof(bool)))
                return System.Convert.ToBoolean(ValueStr);
            else if(Type.IsEqual(typeof(Vector2)))
                return Vector2.FromString(ValueStr);
            else if (Type.IsEqual(typeof(Vector3)))
                return Vector3.FromString(ValueStr);
            else if (Type.IsEqual(typeof(Vector4)))
                return Vector4.FromString(ValueStr);
            else if (Type.IsEqual(typeof(Vector2i)))
                return Vector2i.FromString(ValueStr);
            else if (Type.IsEqual(typeof(Vector3i)))
                return Vector3i.FromString(ValueStr);
            else if (Type.IsEqual(typeof(Vector4i)))
                return Vector4i.FromString(ValueStr);
            else if(Type.IsEqual(typeof(RName)))
            {
                var idxStart = ValueStr.IndexOf('(');
                var idxEnd = ValueStr.IndexOf(')');
                if (idxStart >= 0 && idxEnd >= 0)
                {
                    var subStrs = ValueStr.Substring(idxStart, idxEnd - idxStart).Split(',');
                    var name = subStrs[0].TrimStart('"', ' ').TrimEnd('"', ' ');
                    var rnameType = subStrs[1].TrimStart(' ').TrimEnd(' ');
                    return RName.GetRName(name, (EngineNS.RName.ERNameType)System.Enum.Parse(typeof(EngineNS.RName.ERNameType), rnameType));
                }
            }
            else if(Type.IsEqual(typeof(System.Type)))
            {
                var idxStart = ValueStr.IndexOf('(');
                var idxEnd = ValueStr.IndexOf(')');
                return Rtti.UTypeDesc.TypeOfFullName(ValueStr.Substring(idxStart, idxEnd - idxStart));
            }
            return null;
        }
        public void SetValue<T>(T value) where T : unmanaged
        {
            Type = Rtti.UTypeDesc.TypeOf(typeof(T));
            ValueStr = value.ToString();
        }
        public void SetValue(string value)
        {
            Type = Rtti.UTypeDesc.TypeOf(typeof(string));
            ValueStr = value;
        }
    }

    public class UCastExpression : UExpressionBase, IO.ISerializer
    {
        [Rtti.Meta]
        public UTypeReference TargetType { get; set; }
        [Rtti.Meta]
        public UTypeReference SourceType { get; set; }
        [Rtti.Meta]
        public UExpressionBase Expression { get; set; }

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

    public class UCreateObjectExpression : UExpressionBase, IO.ISerializer
    {
        [Rtti.Meta]
        public string TypeName { get; set; }
        [Rtti.Meta]
        public List<UExpressionBase> Parameters { get; set; } = new List<UExpressionBase>();

        public UCreateObjectExpression(string typeName, params UExpressionBase[] exps)
        {
            TypeName = typeName;
            Parameters.AddRange(exps);
        }

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

    public class UDefaultValueExpression : UExpressionBase, IO.ISerializer
    {
        [Rtti.Meta]
        public UTypeReference Type { get; set; }
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

    public class UExecuteSequenceStatement : UStatementBase, IO.ISerializer
    {
        [Rtti.Meta]
        public List<UStatementBase> Sequence { get; set; } = new List<UStatementBase>();

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

    public class UIfStatement : UStatementBase, IO.ISerializer
    {
        [Rtti.Meta]
        public UExpressionBase Condition { get; set; }
        [Rtti.Meta]
        public UStatementBase TrueStatement { get; set; }
        [Rtti.Meta]
        public UStatementBase FalseStatement { get; set; }
        [Rtti.Meta]
        public List<UIfStatement> ElseIfs { get; set; } = new List<UIfStatement>();

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

    public class UForLoopStatement : UStatementBase, IO.ISerializer
    {
        [Rtti.Meta]
        public bool IncludeEnd { get; set; } = false;
        [Rtti.Meta]
        public string LoopIndexName { get; set; }
        [Rtti.Meta]
        public UExpressionBase BeginExpression { get; set; }
        [Rtti.Meta]
        public UExpressionBase EndExpression { get; set; }
        [Rtti.Meta]
        public UExpressionBase StepExpression { get; set; }
        [Rtti.Meta]
        public UStatementBase LoopBody { get; set; }

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

    public class UWhileLoopStatement : UStatementBase, IO.ISerializer
    {
        [Rtti.Meta]
        public UExpressionBase Condition { get; set; }
        [Rtti.Meta]
        public UStatementBase LoopBody { get; set; }

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
    public class UExpressionStatement : UStatementBase, IO.ISerializer
    {
        [Rtti.Meta]
        public UExpressionBase Expression { get; set; }
        [Rtti.Meta]
        public UStatementBase NextStatement { get; set; }
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