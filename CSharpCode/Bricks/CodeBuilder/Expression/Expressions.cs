using EngineNS.Bricks.CodeBuilder.MacrossNode;
using EngineNS.Bricks.NodeGraph;
using EngineNS.EGui.Controls.PropertyGrid;
using EngineNS.Rtti;
using EngineNS.Thread.Async;
using MathNet.Numerics.LinearAlgebra.Factorization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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

    [Rtti.Meta(NameAlias = new string[] { "EngineNS.Bricks.CodeBuilder.UCodeObject@EngineCore", "EngineNS.Bricks.CodeBuilder.UCodeObject" })]
    public class TtCodeObject : IO.ISerializer
    {
        public virtual void OnPreRead(object tagObject, object hostObject, bool fromXml)
        {
        }

        public virtual void OnPropertyRead(object tagObject, PropertyInfo prop, bool fromXml)
        {
        }
    }

    [Rtti.Meta(NameAlias = new string[] { "EngineNS.Bricks.CodeBuilder.UExpressionBase@EngineCore", "EngineNS.Bricks.CodeBuilder.UExpressionBase" })]
    public class TtExpressionBase : TtCodeObject { }
    [Rtti.Meta(NameAlias = new string[] { "EngineNS.Bricks.CodeBuilder.UStatementBase@EngineCore", "EngineNS.Bricks.CodeBuilder.UStatementBase" })]
    public class TtStatementBase : TtCodeObject
    {
        public TtStatementBase Next;
    }
    [Rtti.Meta(NameAlias = new string[] { "EngineNS.Bricks.CodeBuilder.UTypeReference@EngineCore", "EngineNS.Bricks.CodeBuilder.UTypeReference" })]
    public class TtTypeReference : IO.ISerializer
    {
        string mTypeFullName;
        [Rtti.Meta(Order = 1)]
        public string TypeFullName
        {
            get
            {
                if (mTypeDesc != null)
                {
                    var name = Rtti.TtTypeDesc.GetCSharpTypeNameString(mTypeDesc.SystemType);
                    if (mTypeDesc.IsRefType)
                    {
                        return "ref " + name.Substring(0, name.Length - 1);
                    }
                    return name;
                }
                return mTypeFullName;
            }
            set
            {
                if (mTypeDesc == null)
                    mTypeDesc = Rtti.TtTypeDesc.TypeOfFullName(value);
                else if (mTypeDesc.FullName != value)
                    mTypeDesc = Rtti.TtTypeDesc.TypeOfFullName(value);
                mTypeFullName = value;
            }
        }
        Rtti.TtTypeDesc mTypeDesc;
        [Rtti.Meta(Order = 0)]
        public Rtti.TtTypeDesc TypeDesc
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

        public bool IsRefType
        {
            get
            {
                if (mTypeDesc != null)
                    return mTypeDesc.IsRefType;
                return false;
            }
        }

        public bool IsTask
        {
            get
            {
                return (mTypeDesc.IsEqual(typeof(System.Threading.Tasks.Task)) || mTypeDesc.IsSubclassOf(typeof(System.Threading.Tasks.Task))) || (mTypeDesc.GetInterface(nameof(ITask)) != null);
            }
        }

        public TtTypeReference(string typeFullName, bool isEnum = false)
        {
            mTypeFullName = typeFullName;
            mIsEnum = isEnum;
        }
        public TtTypeReference(Rtti.TtTypeDesc typeDesc)
        {
            mTypeDesc = typeDesc;
        }
        public TtTypeReference(Type type)
        {
            mTypeDesc = Rtti.TtTypeDesc.TypeOf(type);
        }
        public static bool operator == (TtTypeReference lhs, TtTypeReference rhs)
        {
            if (lhs is null)
                return rhs is null;
            return lhs.Equals(rhs);
        }
        public static bool operator != (TtTypeReference lhs, TtTypeReference rhs)
        {
            return !(lhs == rhs);
        }
        public override bool Equals(object obj)
        {
            var typeR = obj as TtTypeReference;
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
        public bool IsEqual(TtTypeDesc type)
        {
            if (mTypeDesc != null)
                return mTypeDesc == type;
            return TypeFullName == type.FullName;
        }
        public virtual void OnPreRead(object tagObject, object hostObject, bool fromXml)
        {
        }

        public virtual void OnPropertyRead(object tagObject, PropertyInfo prop, bool fromXml)
        {
        }
    }

    [Rtti.Meta(NameAlias = new string[] { "EngineNS.Bricks.CodeBuilder.UDebuggerSetWatchVariable@EngineCore", "EngineNS.Bricks.CodeBuilder.UDebuggerSetWatchVariable" })]
    public class TtDebuggerSetWatchVariable : TtStatementBase
    {
        [Rtti.Meta]
        public TtTypeReference VariableType { get; set; }
        [Rtti.Meta]
        public string VariableName { get; set; }
        [Rtti.Meta]
        public TtExpressionBase VariableValue { get; set; }

        public override bool Equals(object obj)
        {
            var w = obj as TtDebuggerSetWatchVariable;
            if (w == null)
                return false;
            return (VariableName == w.VariableName) &&
                   (VariableValue.Equals(w.VariableValue)) &&
                   (VariableType.Equals(w.VariableType));
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
    [Rtti.Meta(NameAlias = new string[] { "EngineNS.Bricks.CodeBuilder.UDebuggerTryBreak@EngineCore", "EngineNS.Bricks.CodeBuilder.UDebuggerTryBreak" })]
    public class TtDebuggerTryBreak : TtStatementBase
    {
        [Rtti.Meta]
        public string BreakName { get; set; }

        public TtDebuggerTryBreak(string name)
        {
            BreakName = name;
        }

        public override bool Equals(object obj)
        {
            var b = obj as TtDebuggerTryBreak;
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
    public class TtAttribute : TtExpressionBase
    {
        [Rtti.Meta]
        public TtTypeReference AttributeType { get; set; }
        [Rtti.Meta]
        public List<TtExpressionBase> Arguments { get; set; } = new List<TtExpressionBase>();

        public override bool Equals(object obj)
        {
            var varDec = obj as TtAttribute;
            if (varDec == null)
                return false;
            if (!AttributeType.Equals(varDec.AttributeType))
                return false;
            if (Arguments.Count != varDec.Arguments.Count)
                return false;
            for(int i=0; i<Arguments.Count; i++)
            {
                if (!Arguments[i].Equals(varDec.Arguments[i]))
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
            string retVal = AttributeType.TypeFullName;
            for(int i=0; i<Arguments.Count; i++)
            {
                retVal += Arguments[i].ToString();
            }
            return retVal;
        }
    }
    [Rtti.Meta(NameAlias = new string[] { "EngineNS.Bricks.CodeBuilder.UVariableDeclaration@EngineCore", "EngineNS.Bricks.CodeBuilder.UVariableDeclaration" })]
    public class TtVariableDeclaration : TtStatementBase, IO.ISerializer, EGui.Controls.PropertyGrid.IPropertyCustomization, NodeGraph.UEditableValue.IValueEditNotify
    {
        [Rtti.Meta]
        public TtTypeReference VariableType { get; set; }
        [Rtti.Meta]
        public string VariableName { get; set; } = "Unknow";
        public Func<TtVariableDeclaration, string> GetDisplayNameFunc;
        public string DisplayName
        {
            get
            {
                if (GetDisplayNameFunc != null)
                    return GetDisplayNameFunc.Invoke(this);
                return VariableName;
            }
        }
        [Rtti.Meta]
        public TtExpressionBase InitValue { get; set; }
        [Rtti.Meta]
        public TtCommentStatement Comment { get; set; }
        [Rtti.Meta]
        public EVisisMode VisitMode { get; set; } = EVisisMode.Public;
        [Rtti.Meta]
        public List<TtAttribute> Attributes { get; set; } = new List<TtAttribute>();

        [Browsable(false)]
        [Rtti.Meta]
        public bool IsBindable { get; set; } = false;
        [Rtti.Meta]
        public bool IsAutoSaveLoad { get; set; } = true;
        [Browsable(false)]
        public bool IsPropertyVisibleDirty { get; set; } = false;

        public TtVariableDeclaration()
        {
        }

        public override bool Equals(object obj)
        {
            var varDec = obj as TtVariableDeclaration;
            if (varDec == null)
                return false;
            return (VariableName == varDec.VariableName) && (VariableType.Equals(varDec.VariableType));
        }

        public bool HasAttribute(TtTypeDesc desc)
        {
            for(int i=0; i<Attributes.Count; i++)
            {
                if (Attributes[i].AttributeType.IsEqual(desc))
                    return true;
            }
            return false;
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
            var thisType = Rtti.TtTypeDesc.TypeOf(this.GetType());
            foreach(PropertyDescriptor prop in pros)
            {
                var proDesc = EGui.Controls.PropertyGrid.PropertyCollection.PropertyDescPool.QueryObjectSync();
                proDesc.InitValue(this, thisType, prop, parentIsValueType);
                switch(proDesc.Name)
                {
                    case "VariableType":
                        {
                            List<Rtti.TtTypeDesc> types = new List<Rtti.TtTypeDesc>(200);
                            proDesc.PropertyType = Rtti.TtTypeDesc.TypeOf(typeof(System.Type));
                            foreach(var service in Rtti.TtTypeDescManager.Instance.Services.Values)
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
                            proDesc.PropertyType = Rtti.TtTypeDesc.TypeOf(typeof(System.String));
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
                        var pe = InitValue as TtPrimitiveExpression;
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
                        var tagType = value as Rtti.TtTypeDesc;
                        if(tagType != VariableType.TypeDesc)
                        {
                            InitValue = new TtPrimitiveExpression(tagType, tagType.IsValueType ? Rtti.TtTypeDescManager.CreateInstance(tagType) : null);
                            VariableType.TypeDesc = tagType;
                        }
                    }
                    break;
                case "InitValue":
                    {
                        var pe = InitValue as TtPrimitiveExpression;
                        if (pe != null)
                        {
                            pe.CalculateValueString(pe.Type, value);
                        }
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
            var pe = InitValue as TtPrimitiveExpression;
            if(pe != null)
            {
                pe.CalculateValueString(pe.Type, ev.Value);
            }
        }
    }

    public class TtIncludeDeclaration : TtStatementBase, IO.ISerializer
    {
        [Rtti.Meta]
        public string FilePath { get; set; }
        public override bool Equals(object obj)
        {
            var dec = obj as TtIncludeDeclaration;
            return FilePath == dec.FilePath;
        }
        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }
        public override string ToString()
        {
            return "include:" + FilePath;
        }
    }

    [Rtti.Meta(NameAlias = new string[] { "EngineNS.Bricks.CodeBuilder.UMethodArgumentDeclaration@EngineCore", "EngineNS.Bricks.CodeBuilder.UMethodArgumentDeclaration" })]
    public class TtMethodArgumentDeclaration : TtCodeObject, IO.ISerializer, EGui.Controls.PropertyGrid.IPropertyCustomization, NodeGraph.UEditableValue.IValueEditNotify
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

        /* 项目“Engine.Android”的未合并的更改
        在此之前:
                public UTypeReference VariableType { get; set; } = new UTypeReference(Rtti.TtTypeDesc.TypeOf<int>());
                public Action<string, string> OnVariableNameChanged = null;
        在此之后:
                public TtTypeReference VariableType { get; set; } = new UTypeReference(Rtti.TtTypeDesc.TypeOf<int>());
                public Action<string, string> OnVariableNameChanged = null;
        */
        public TtTypeReference VariableType { get; set; } = new TtTypeReference(Rtti.TtTypeDesc.TypeOf<int>());
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
        public delegate string Delegate_GetErrorString(in PGCustomValueEditorAttribute.EditorInfo info, TtMethodArgumentDeclaration dec, object newValue);
        public Delegate_GetErrorString GetErrorStringAction;
        class VariableNameAttribute : EGui.Controls.PropertyGrid.PGCustomValueEditorAttribute
        {
            public VariableNameAttribute()
            {
                UserDraw = false;
            }
            public override string GetErrorString<T>(in EditorInfo info, T newValue)
            {
                var maDec = info.ObjectInstance as TtMethodArgumentDeclaration;
                return maDec?.GetErrorStringAction?.Invoke(in info, maDec, newValue);
            }
        }
        [Rtti.Meta]
        public TtExpressionBase InitValue { get; set; } = new TtPrimitiveExpression(Rtti.TtTypeDesc.TypeOf<int>(), 0);
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
        public static EMethodArgumentAttribute GetOperationType(Rtti.TtClassMeta.TtMethodMeta.TtParamMeta info)
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
        public static bool GetIsParamArray(Rtti.TtClassMeta.TtMethodMeta.TtParamMeta info)
        {
            return info.IsParamArray;
        }

        public static bool IsMatching(TtMethodArgumentDeclaration desc, ParameterInfo info)
        {
            Debug.Assert(desc != null);
            Debug.Assert(info != null);

            return desc.VariableType.IsEqual(info.ParameterType) &&
                   (desc.OperationType == GetOperationType(info)) &&
                   (desc.IsParamArray == GetIsParamArray(info));
        }
        public static TtMethodArgumentDeclaration GetParam(System.Reflection.ParameterInfo info)
        {
            var retVal = new TtMethodArgumentDeclaration()
            {
                VariableType = new TtTypeReference(info.ParameterType),
                VariableName = info.Name,
                HasDefaultValue = info.HasDefaultValue,
            };
            if (info.HasDefaultValue)
                retVal.InitValue = new TtPrimitiveExpression(Rtti.TtTypeDesc.TypeOf(info.DefaultValue.GetType()), info.DefaultValue);
            retVal.OperationType = GetOperationType(info);
            retVal.IsParamArray = GetIsParamArray(info);
            var attrs = info.GetCustomAttributes(typeof(Rtti.MetaParameterAttribute), false);
            if (attrs.Length > 0)
                retVal.Meta = attrs[0] as Rtti.MetaParameterAttribute;
            return retVal;
        }
        public static TtMethodArgumentDeclaration GetParam(Rtti.TtClassMeta.TtMethodMeta.TtParamMeta info)
        {
            var retVal = new TtMethodArgumentDeclaration()
            {
                VariableType = new TtTypeReference(info.ParameterType),
                VariableName = info.Name,
                HasDefaultValue = info.HasDefaultValue,
            };
            if (info.HasDefaultValue)
                retVal.InitValue = new TtPrimitiveExpression(Rtti.TtTypeDesc.TypeOf(info.DefaultValue.GetType()), info.DefaultValue);
            retVal.OperationType = GetOperationType(info);
            retVal.IsParamArray = GetIsParamArray(info);
            retVal.Meta = info.Meta;
            return retVal;
        }

        public override bool Equals(object obj)
        {
            var arg = obj as TtMethodArgumentDeclaration;
            if (arg == null)
                return false;
            return (VariableType.Equals(arg.VariableType) &&
                    VariableName == arg.VariableName &&
                    IsParamArray == arg.IsParamArray &&
                    InitValue.Equals(arg.InitValue));
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
            var thisType = Rtti.TtTypeDesc.TypeOf(this.GetType());
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
                            var types = new List<Rtti.TtTypeDesc>(200);
                            proDesc.PropertyType = Rtti.TtTypeDesc.TypeOf(typeof(System.Type));
                            foreach(var service in Rtti.TtTypeDescManager.Instance.Services.Values)
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
                        var pe = InitValue as TtPrimitiveExpression;
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
                        var tagType = value as Rtti.TtTypeDesc;
                        if(tagType != VariableType.TypeDesc)
                        {
                            InitValue = new TtPrimitiveExpression(tagType, tagType.IsValueType ? Rtti.TtTypeDescManager.CreateInstance(tagType) : null);
                            VariableType.TypeDesc = tagType;
                        }
                    }
                    break;
                case "InitValue":
                    {
                        var pe = InitValue as TtPrimitiveExpression;
                        if (pe != null)
                        {
                            pe.CalculateValueString(pe.Type, value);
                        }
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
            var pe = InitValue as TtPrimitiveExpression;
            if (pe != null)
            {
                pe.CalculateValueString(pe.Type, ev.Value);
            }
        }
    }

    [Rtti.Meta(NameAlias = new string[] { "EngineNS.Bricks.CodeBuilder.UMethodDeclaration@EngineCore", "EngineNS.Bricks.CodeBuilder.UMethodDeclaration" })]
    public class TtMethodDeclaration : TtCodeObject, IO.ISerializer
    {
        public Rtti.TtClassMeta.TtMethodMeta OverrideMethod = null;
        [Rtti.Meta]
        public EVisisMode VisitMode { get; set; } = EVisisMode.Public;
        [Rtti.Meta]
        public TtVariableDeclaration ReturnValue { get; set; }
        [Rtti.Meta]
        public string MethodName { get; set; } = "Unknow";
        public Func<TtMethodDeclaration, string> GetDisplayNameFunc;
        public string UniqueMethodName
        {
            get
            {
                return $"{MethodName}_{UniHash32.APHash(this.ToString())}";
            }
        }
        public string DisplayName
        {
            get
            {
                if (GetDisplayNameFunc != null)
                    return GetDisplayNameFunc.Invoke(this);
                return MethodName;
            }
        }
        [Rtti.Meta]
        public List<TtMethodArgumentDeclaration> Arguments { get; set; } = new List<TtMethodArgumentDeclaration>();
        [Rtti.Meta]
        public List<TtVariableDeclaration> LocalVariables { get; set; } = new List<TtVariableDeclaration>();
        [Rtti.Meta]
        public TtCommentStatement Comment { get; set; }
        [Rtti.Meta]
        public List<TtAttribute> Attributes { get; set; } = new List<TtAttribute>();
        [Rtti.Meta]
        public bool IsOverride { get; set; } = false;
        public enum EAsyncType
        {
            None,
            SystemTask,
            CustomTask,
        }
        [Rtti.Meta]
        public EAsyncType AsyncType { get; set; } = EAsyncType.None;

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


/* 项目“Engine.Android”的未合并的更改
在此之前:
        public UExecuteSequenceStatement MethodBody = new UExecuteSequenceStatement();
        public int MethodSegmentDeep = 0;
在此之后:
        public TtExecuteSequenceStatement MethodBody = new UExecuteSequenceStatement();
        public int MethodSegmentDeep = 0;
*/
        public TtExecuteSequenceStatement MethodBody = new TtExecuteSequenceStatement();
        public int MethodSegmentDeep = 0;
        public bool ReturnHasGenerated = false;
        public bool HasUnsafeCode = false;
        public bool HasAwaitCode = false;

        public TtClassDeclaration HostClass;

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
            if(Arguments != null)
            {
                for(int i=0; i<Arguments.Count; i++)
                {
                    keyword += //Arguments[i].OperationType.ToString() + " " +
                              //(Arguments[i].IsParamArray ? "param " : "") +
                              Arguments[i].VariableType.TypeFullName + ",";
                }
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
                retStr += TtMethodArgumentDeclaration.GetOperationType(parameters[i]) + " ";
                retStr += TtMethodArgumentDeclaration.GetIsParamArray(parameters[i]) ? "param " : "";
                retStr += parameters[i].ParameterType.FullName + ",";
            }
            retStr = retStr.TrimEnd(',') + ")";
            return retStr;
        }
        public static string GetKeyword(Rtti.TtClassMeta.TtMethodMeta method)
        {
            var retStr = method.MethodName + "(";
            var parameters = method.Parameters;
            for(int i=0; i<parameters.Count; i++)
            {
                retStr += TtMethodArgumentDeclaration.GetOperationType(parameters[i]) + " ";
                retStr += TtMethodArgumentDeclaration.GetIsParamArray(parameters[i]) ? "param " : "";
                retStr += parameters[i].ParameterType.FullName + ",";
            }
            retStr = retStr.TrimEnd(',') + ")";
            return retStr;
        }

        public static MethodData.EErrorType IsMatching(TtMethodDeclaration methodDesc, MethodInfo methodInfo)
        {
            Debug.Assert(methodDesc != null);
            Debug.Assert(methodInfo != null);

            var methodInfoParams = methodInfo.GetParameters();
            if (methodDesc.Arguments.Count != methodInfoParams.Length)
                return MethodData.EErrorType.InvalidParam;

            if (methodInfo.ReturnType != typeof(void))
            {
                if(methodInfo.ReturnType.BaseType == typeof(System.Threading.Tasks.Task) ||
                   methodInfo.ReturnType.BaseType.IsSubclassOf(typeof(System.Threading.Tasks.Task)))
                {
                    if (!methodDesc.ReturnValue.VariableType.IsEqual(methodInfo.ReturnType.GetGenericArguments()[0]))
                        return MethodData.EErrorType.InvalidReturn;
                }
                else if(methodInfo.ReturnType.BaseType.GetInterface(nameof(ITask)) != null)
                {
                    if (!methodDesc.ReturnValue.VariableType.IsEqual(methodInfo.ReturnType.GetGenericArguments()[0]))
                        return MethodData.EErrorType.InvalidReturn;
                }
                else
                {
                    if(!methodDesc.ReturnValue.VariableType.IsEqual(methodInfo.ReturnType))
                        return MethodData.EErrorType.InvalidReturn;
                }
            }

            for(int i=0; i<methodInfoParams.Length; i++)
            {
                if(!TtMethodArgumentDeclaration.IsMatching(methodDesc.Arguments[i], methodInfoParams[i]))
                    return MethodData.EErrorType.InvalidParam;
            }

            return MethodData.EErrorType.None;
        }

        public static TtMethodDeclaration GetMethodDeclaration(System.Reflection.MethodInfo method)
        {
            var retVal = new TtMethodDeclaration();
            retVal.IsOverride = true;
            if ((method.ReturnType != typeof(void)) && 
                (method.ReturnType != typeof(System.Threading.Tasks.Task)
                && method.ReturnType != typeof(EngineNS.Thread.Async.TtTask)))
            {
                var retType = method.ReturnType;
                if (method.ReturnType.BaseType == typeof(System.Threading.Tasks.Task) ||
                    method.ReturnType.BaseType.IsSubclassOf(typeof(System.Threading.Tasks.Task)))
                {
                    retType = method.ReturnType.GetGenericArguments()[0];
                    retVal.AsyncType = EAsyncType.SystemTask;
                }
                else if (method.ReturnType.BaseType.GetInterface(nameof(ITask)) != null)
                {
                    retType = method.ReturnType.GetGenericArguments()[0];
                    retVal.AsyncType = EAsyncType.CustomTask;
                }

                retVal.ReturnValue = new TtVariableDeclaration()
                {
                    VariableType = new TtTypeReference(retType),
                    InitValue = new TtDefaultValueExpression(retType),
                    VariableName = "ret_" + (UInt32)Guid.NewGuid().ToString().GetHashCode(),
                };
            }
            else if (method.ReturnType.BaseType == typeof(System.Threading.Tasks.Task) || method.ReturnType.IsSubclassOf(typeof(System.Threading.Tasks.Task)))
                retVal.AsyncType = EAsyncType.SystemTask;
            else if (method.ReturnType.GetInterface(nameof(ITask)) != null)
                retVal.AsyncType = EAsyncType.CustomTask;

            retVal.MethodName = method.Name;
            var parameters = method.GetParameters();
            retVal.Arguments = new List<TtMethodArgumentDeclaration>(parameters.Length);
            for(int paramIdx=0; paramIdx < parameters.Length; paramIdx++)
            {
                retVal.Arguments.Add(TtMethodArgumentDeclaration.GetParam(parameters[paramIdx]));
            }
            return retVal;
        }
        public static TtMethodDeclaration GetMethodDeclaration(Rtti.TtClassMeta.TtMethodMeta method)
        {
            var retVal = new TtMethodDeclaration();
            retVal.IsOverride = true;
            retVal.OverrideMethod = method;
            if (!method.ReturnType.IsEqual(typeof(void)) && 
                !method.ReturnType.IsEqual(typeof(System.Threading.Tasks.Task)) &&
                !method.ReturnType.IsEqual(typeof(EngineNS.Thread.Async.TtTask)))
            {
                var retType = method.ReturnType;
                if (retType.IsSubclassOf(typeof(System.Threading.Tasks.Task)))
                {
                    retType = Rtti.TtTypeDesc.TypeOf(method.ReturnType.GetGenericArguments()[0]);
                    retVal.AsyncType = EAsyncType.SystemTask;
                }
                else if (retType.GetInterface(nameof(ITask)) != null)
                {
                    retType = Rtti.TtTypeDesc.TypeOf(method.ReturnType.GetGenericArguments()[0]);
                    retVal.AsyncType = EAsyncType.CustomTask;
                }
                retVal.ReturnValue = new TtVariableDeclaration()
                {
                    VariableType = new TtTypeReference(retType),
                    InitValue = new TtDefaultValueExpression(retType),
                    VariableName = "ret_" + (UInt32)Guid.NewGuid().ToString().GetHashCode(),
                };
            }
            else if(method.ReturnType.IsEqual(typeof(System.Threading.Tasks.Task)))
                retVal.AsyncType = EAsyncType.SystemTask;
            else if (method.ReturnType.IsEqual(typeof(TtTask)))
                retVal.AsyncType = EAsyncType.CustomTask;

            retVal.MethodName = method.MethodName;
            var parameters = method.Parameters;
            retVal.Arguments = new List<TtMethodArgumentDeclaration>(parameters.Count);
            for (int paramIdx = 0; paramIdx < parameters.Count; paramIdx++)
            {
                retVal.Arguments.Add(TtMethodArgumentDeclaration.GetParam(parameters[paramIdx]));
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

        public void AddLocalVar(TtVariableDeclaration var)
        {
            LocalVariables.Add(var);
        }

        public override bool Equals(object obj)
        {
            var dec = obj as TtMethodDeclaration;
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

    [Rtti.Meta(NameAlias = new string[] { "EngineNS.Bricks.CodeBuilder.UNamespaceDeclaration@EngineCore", "EngineNS.Bricks.CodeBuilder.UNamespaceDeclaration" })]
    public class TtNamespaceDeclaration : TtCodeObject, IO.ISerializer
    {
        [Rtti.Meta]
        public string Namespace { get; set; } = "Unknow";
        [Rtti.Meta]
        public List<TtClassDeclaration> Classes { get; set; } = new List<TtClassDeclaration>();

        public TtNamespaceDeclaration(string ns)
        {
            Namespace = ns;
        }
        public override bool Equals(object obj)
        {
            var ns = obj as TtNamespaceDeclaration;
            if (ns == null)
                return false;
            return Namespace == ns.Namespace;
        }
        public override int GetHashCode()
        {
            return Namespace.GetHashCode();
        }

        public static bool operator !=(TtNamespaceDeclaration lh, TtNamespaceDeclaration rh)
        {
            return !(lh == rh);
        }
        public static bool operator ==(TtNamespaceDeclaration lh, TtNamespaceDeclaration rh)
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

    [Rtti.Meta(NameAlias = new string[] { "EngineNS.Bricks.CodeBuilder.UClassDeclaration@EngineCore", "EngineNS.Bricks.CodeBuilder.UClassDeclaration" })]
    public class TtClassDeclaration : TtCodeObject, IO.ISerializer
    {
        [Rtti.Meta]
        public bool IsUnsafe { get; set; } = false;
        [Rtti.Meta]
        public EVisisMode VisitMode { get; set; } = EVisisMode.Public;
        [Rtti.Meta]
        public bool IsStruct { get; set; } = false;
        [Rtti.Meta]
        public string ClassName { get; set; } = "Unknow";
        [Rtti.Meta]
        public List<string> SupperClassNames { get; set; } = new List<string>();
        [Rtti.Meta]
        public List<TtVariableDeclaration> Properties { get; set; } = new List<TtVariableDeclaration>();
        [Rtti.Meta]
        public List<TtMethodDeclaration> Methods { get; set; } = new List<TtMethodDeclaration>();

        public TtNamespaceDeclaration Namespace;
        [Rtti.Meta]
        public TtCommentStatement Comment { get; set; }

        public List<TtVariableDeclaration> PreDefineVariables = new List<TtVariableDeclaration>();

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
        public Rtti.TtTypeDesc TryGetTypeDesc()
        {
            return Rtti.TtTypeDescManager.Instance.GetTypeDescFromFullName(GetFullName());
        }
        public void Reset()
        {
            VisitMode = EVisisMode.Public;
            ClassName = "Unknow";
            IsStruct = false;
            SupperClassNames = new List<string>();
            Properties = new List<TtVariableDeclaration>();
            Methods = new List<TtMethodDeclaration>();
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
        public void AddMethod(TtMethodDeclaration method)
        {
            if (!Methods.Contains(method))
                Methods.Add(method);
            method.HostClass = this;
        }
        public void RemoveMethod(TtMethodDeclaration method)
        {
            method.HostClass = null;
            Methods.Remove(method);
        }
        public TtMethodDeclaration FindMethod(string name)
        {
            for(int i=0; i<Methods.Count; i++)
            {
                if (Methods[i].MethodName == name)
                    return Methods[i];
            }
            return null;
        }

        public TtVariableDeclaration FindMember(string name)
        {
            for(int i=0; i<Properties.Count; i++)
            {
                if (Properties[i].VariableName == name)
                    return Properties[i];
            }
            return null;
        }
        public bool RemoveMember(string name)
        {
            for(int i=0; i<Properties.Count; i++)
            {
                if (Properties[i].VariableName == name)
                {
                    Properties.RemoveAt(i);
                    return true;
                }
            }
            return false;
        }

        public override bool Equals(object obj)
        {
            var dec = obj as TtClassDeclaration;
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

        public static TtClassDeclaration GetClassDeclaration(TtTypeDesc type)
        {
            var dec = new TtClassDeclaration()
            {
                VisitMode = type.IsPublic? EVisisMode.Public : EVisisMode.Private,
                IsStruct = type.SystemType.IsValueType,
                ClassName = type.Name,
                Namespace = new TtNamespaceDeclaration(type.Namespace),
            };
            dec.SupperClassNames.Add(type.BaseType.FullName);
            return dec;
        }
    }

    [Rtti.Meta(NameAlias = new string[] { "EngineNS.Bricks.CodeBuilder.UClassReferenceExpression@EngineCore", "EngineNS.Bricks.CodeBuilder.UClassReferenceExpression" })]
    public class TtClassReferenceExpression : TtExpressionBase, IO.ISerializer
    {
        [Rtti.Meta]
        public Rtti.TtTypeDesc Class { get; set; }

        public TtClassReferenceExpression() { }
        public TtClassReferenceExpression(Rtti.TtTypeDesc classType)
        {
            Class = classType;
        }

        public override bool Equals(object obj)
        {
            var cRef = obj as TtClassReferenceExpression;
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

    [Rtti.Meta(NameAlias = new string[] { "EngineNS.Bricks.CodeBuilder.UVariableReferenceExpression@EngineCore", "EngineNS.Bricks.CodeBuilder.UVariableReferenceExpression" })]
    public class TtVariableReferenceExpression : TtExpressionBase, IO.ISerializer
    {
        [Rtti.Meta]
        public TtExpressionBase Host { get; set; }
        [Rtti.Meta]
        public string VariableName { get; set; } = "Unknow";
        public bool IsProperty { get; set; }
        [Rtti.Meta]
        public TtTypeDesc PropertyDeclClass { get; set; } = null;
        
        public TtVariableReferenceExpression()
        {
        }
        public TtVariableReferenceExpression(string name, TtExpressionBase host = null)
        {
            Host = host;
            VariableName = name;
        }
        public override bool Equals(object obj)
        {
            var vRef = obj as TtVariableReferenceExpression;
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

    [Rtti.Meta(NameAlias = new string[] { "EngineNS.Bricks.CodeBuilder.USelfReferenceExpression@EngineCore", "EngineNS.Bricks.CodeBuilder.USelfReferenceExpression" })]
    public class TtSelfReferenceExpression : TtExpressionBase    
    {
        public override bool Equals(object obj)
        {
            var val = obj as TtSelfReferenceExpression;
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

    [Rtti.Meta(NameAlias = new string[] { "EngineNS.Bricks.CodeBuilder.UBaseReferenceExpression@EngineCore", "EngineNS.Bricks.CodeBuilder.UBaseReferenceExpression" })]
    public class TtBaseReferenceExpression : TtExpressionBase
    {
        public override bool Equals(object obj)
        {
            var val = obj as TtBaseReferenceExpression;
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
            return "base";
        }
    }

    [Rtti.Meta(NameAlias = new string[] { "EngineNS.Bricks.CodeBuilder.UMethodInvokeArgumentExpression@EngineCore", "EngineNS.Bricks.CodeBuilder.UMethodInvokeArgumentExpression" })]
    public class TtMethodInvokeArgumentExpression : TtExpressionBase, IO.ISerializer
    {
        public TtExpressionBase Expression;// { get; set; }
        [Rtti.Meta]
        public EMethodArgumentAttribute OperationType { get; set; } = EMethodArgumentAttribute.Default;

        public TtMethodInvokeArgumentExpression()
        {

        }
        public TtMethodInvokeArgumentExpression(TtExpressionBase exp, EMethodArgumentAttribute operation = EMethodArgumentAttribute.Default)
        {
            Expression = exp;
            OperationType = operation;
        }

        public override bool Equals(object obj)
        {
            var iArg = obj as TtMethodInvokeArgumentExpression;
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

    [Rtti.Meta(NameAlias = new string[] { "EngineNS.Bricks.CodeBuilder.UMethodInvokeStatement@EngineCore", "EngineNS.Bricks.CodeBuilder.UMethodInvokeStatement" })]
    public class TtMethodInvokeStatement : TtStatementBase, IO.ISerializer
    {
        public Rtti.TtClassMeta.TtMethodMeta Method = null;
        [Rtti.Meta]
        public TtExpressionBase Host { get; set; }
        [Rtti.Meta]
        public string MethodName { get; set; } = "Unknow";
        [Rtti.Meta]
        public bool IsReturnRef { get; set; } = false;
        [Rtti.Meta]
        public List<TtMethodInvokeArgumentExpression> Arguments { get; set; } = new List<TtMethodInvokeArgumentExpression>();
        [Rtti.Meta]
        public TtVariableDeclaration ReturnValue { get; set; }
        [Rtti.Meta]
        public bool DeclarationReturnValue { get; set; } = false;
        [Rtti.Meta]
        public bool ForceCastReturnType { get; set; } = false;
        [Rtti.Meta]
        public bool IsAsync { get; set; } = false;
        [Rtti.Meta]
        public bool IsUnsafe { get; set; } = false;
        public List<TtTypeDesc> GenericTypes { get; set; } = new List<TtTypeDesc>();
        
        public TtMethodInvokeStatement() { }
        public TtMethodInvokeStatement(string methodName, TtVariableDeclaration retValue, TtExpressionBase host)
        {
            MethodName = methodName;
            Host = host;
            ReturnValue = retValue;
        }
        public TtMethodInvokeStatement(string methodName, TtVariableDeclaration retValue, TtExpressionBase host, params TtMethodInvokeArgumentExpression[] arguments)
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
            var invoke = obj as TtMethodInvokeStatement;
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

    [Rtti.Meta(NameAlias = new string[] { "EngineNS.Bricks.CodeBuilder.ULambdaExpression@EngineCore", "EngineNS.Bricks.CodeBuilder.ULambdaExpression" })]
    public class TtLambdaExpression : TtExpressionBase, IO.ISerializer
    {
        [Rtti.Meta]
        public TtTypeReference ReturnType { get; set; }
        [Rtti.Meta]
        public List<TtMethodInvokeArgumentExpression> LambdaArguments { get; set; } = new List<TtMethodInvokeArgumentExpression>();
        [Rtti.Meta]
        public List<TtStatementBase> Sequence { get; set; } = new List<TtStatementBase>();
        [Rtti.Meta]
        public TtMethodDeclaration MethodDesc;
        [Rtti.Meta]
        public TtMethodInvokeStatement MethodInvoke { get; set; }
        [Rtti.Meta]
        public bool IsAsync { get; set; } = false;

    }

    [Rtti.Meta(NameAlias = new string[] { "EngineNS.Bricks.CodeBuilder.UAssignOperatorStatement@EngineCore", "EngineNS.Bricks.CodeBuilder.UAssignOperatorStatement" })]
    public class TtAssignOperatorStatement : TtStatementBase, IO.ISerializer
    {
        [Rtti.Meta]
        public TtExpressionBase To { get; set; }
        [Rtti.Meta]
        public TtExpressionBase From { get; set; }

        public override bool Equals(object obj)
        {
            var assign = obj as TtAssignOperatorStatement;
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

    [Rtti.Meta(NameAlias = new string[] { "EngineNS.Bricks.CodeBuilder.UBinaryOperatorExpression@EngineCore", "EngineNS.Bricks.CodeBuilder.UBinaryOperatorExpression" })]
    public class TtBinaryOperatorExpression : TtExpressionBase, IO.ISerializer
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
            AddAssignment,
            SubtractAssignment,
            Is,
        }
        [Rtti.Meta]
        public EBinaryOperation Operation { get; set; } = EBinaryOperation.Add;
        [Rtti.Meta]
        public TtExpressionBase Left { get; set; }
        [Rtti.Meta]
        public TtExpressionBase Right { get; set; }
        [Rtti.Meta]
        public bool Cell { get; set; } = true;  // true在生成时增加括号

        public override bool Equals(object obj)
        {
            var b = obj as TtBinaryOperatorExpression;
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

    [Rtti.Meta(NameAlias = new string[] { "EngineNS.Bricks.CodeBuilder.UUnaryOperatorExpression@EngineCore", "EngineNS.Bricks.CodeBuilder.UUnaryOperatorExpression" })]
    public class TtUnaryOperatorExpression : TtExpressionBase, IO.ISerializer
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
        public TtExpressionBase Value { get; set; }

        public override bool Equals(object obj)
        {
            var u = obj as TtUnaryOperatorExpression;
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

    [Rtti.Meta(NameAlias = new string[] { "EngineNS.Bricks.CodeBuilder.UIndexerOperatorExpression@EngineCore", "EngineNS.Bricks.CodeBuilder.UIndexerOperatorExpression" })]
    public class TtIndexerOperatorExpression : TtExpressionBase, IO.ISerializer
    {
        [Rtti.Meta]
        public TtExpressionBase Target { get; set; }
        [Rtti.Meta]
        public List<TtExpressionBase> Indices { get; set; } = new List<TtExpressionBase>();

        public override bool Equals(object obj)
        {
            var id = obj as TtIndexerOperatorExpression;
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

    [Rtti.Meta(NameAlias = new string[] { "EngineNS.Bricks.CodeBuilder.UPrimitiveExpression@EngineCore", "EngineNS.Bricks.CodeBuilder.UPrimitiveExpression" })]
    public class TtPrimitiveExpression : TtExpressionBase, IO.ISerializer
    {
        [Rtti.Meta(Order = 0)]
        public Rtti.TtTypeDesc Type { get; set; }

        [Rtti.Meta]
        public string ObjectStr { get; set; }
        string mValueStr;
        [Rtti.Meta]
        public string ValueStr 
        {
            get => mValueStr;
            set => mValueStr = value;
        }
        [Rtti.Meta]
        public bool TypeIsTypeof { get; set; } = true;

        public override bool Equals(object obj)
        {
            var p = obj as TtPrimitiveExpression;
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

        public TtPrimitiveExpression(Byte val)
        {
            Type = Rtti.TtTypeDesc.TypeOf(typeof(Byte));
            mValueStr = val.ToString();
            ObjectStr = mValueStr;
        }
        public TtPrimitiveExpression(UInt16 val)
        {
            Type = Rtti.TtTypeDesc.TypeOf(typeof(UInt16));
            mValueStr = val.ToString();
            ObjectStr = mValueStr;
        }
        public TtPrimitiveExpression(UInt32 val)
        {
            Type = Rtti.TtTypeDesc.TypeOf(typeof(UInt32));
            mValueStr = val.ToString();
            ObjectStr = mValueStr;
        }
        public TtPrimitiveExpression(UInt64 val)
        {
            Type = Rtti.TtTypeDesc.TypeOf(typeof(UInt64));
            mValueStr = val.ToString();
            ObjectStr = mValueStr;
        }
        public TtPrimitiveExpression(SByte val)
        {
            Type = Rtti.TtTypeDesc.TypeOf(typeof(SByte));
            mValueStr = val.ToString();
            ObjectStr = mValueStr;
        }
        public TtPrimitiveExpression(Int16 val)
        {
            Type = Rtti.TtTypeDesc.TypeOf(typeof(Int16));
            mValueStr = val.ToString();
            ObjectStr = mValueStr;
        }
        public TtPrimitiveExpression(Int32 val)
        {
            Type = Rtti.TtTypeDesc.TypeOf(typeof(Int32));
            mValueStr = val.ToString();
            ObjectStr = mValueStr;
        }
        public TtPrimitiveExpression(Int64 val)
        {
            Type = Rtti.TtTypeDesc.TypeOf(typeof(Int64));
            mValueStr = val.ToString();
            ObjectStr = mValueStr;
        }
        public TtPrimitiveExpression(float val)
        {
            Type = Rtti.TtTypeDesc.TypeOf(typeof(float));
            mValueStr = val.ToString();
            ObjectStr = mValueStr;
        }
        public TtPrimitiveExpression(double val)
        {
            Type = Rtti.TtTypeDesc.TypeOf(typeof(double));
            mValueStr = val.ToString();
            ObjectStr = mValueStr;
        }
        public TtPrimitiveExpression(string val)
        {
            Type = Rtti.TtTypeDesc.TypeOf(typeof(string));
            mValueStr = val;
            ObjectStr = mValueStr;
        }
        public TtPrimitiveExpression(bool val)
        {
            Type = Rtti.TtTypeDesc.TypeOf(typeof(bool));
            mValueStr = val.ToString();
            if (val)
                mValueStr = "true";
            else
                mValueStr = "false";
            ObjectStr = mValueStr;
        }
        public TtPrimitiveExpression(Vector2 val)
        {
            Type = Rtti.TtTypeDesc.TypeOf(typeof(Vector2));
            mValueStr = val.ToString();
            ObjectStr = mValueStr;
        }
        public TtPrimitiveExpression(Vector3 val)
        {
            Type = Rtti.TtTypeDesc.TypeOf(typeof(Vector3));
            mValueStr = val.ToString();
            ObjectStr = mValueStr;
        }
        public TtPrimitiveExpression(Color3f val)
        {
            Type = Rtti.TtTypeDesc.TypeOf(typeof(Color3f));
            mValueStr = val.ToString();
            ObjectStr = mValueStr;
        }
        public TtPrimitiveExpression(Vector4 val)
        {
            Type = Rtti.TtTypeDesc.TypeOf(typeof(Vector4));
            mValueStr = val.ToString();
            ObjectStr = mValueStr;
        }
        public TtPrimitiveExpression(Color4f val)
        {
            Type = Rtti.TtTypeDesc.TypeOf(typeof(Color4f));
            mValueStr = val.ToString();
            ObjectStr = mValueStr;
        }
        public TtPrimitiveExpression(Vector2i val)
        {
            Type = Rtti.TtTypeDesc.TypeOf(typeof(Vector2i));
            mValueStr = val.ToString();
            ObjectStr = mValueStr;
        }
        public TtPrimitiveExpression(Vector3i val)
        {
            Type = Rtti.TtTypeDesc.TypeOf(typeof(Vector3i));
            mValueStr = val.ToString();
            ObjectStr = mValueStr;
        }
        public TtPrimitiveExpression(Vector4i val)
        {
            Type = Rtti.TtTypeDesc.TypeOf(typeof(Vector4i));
            mValueStr = val.ToString();
            ObjectStr = mValueStr;
        }
        public TtPrimitiveExpression(Matrix val)
        {
            Type = Rtti.TtTypeDesc.TypeOf(typeof(Matrix));
            mValueStr = val.ToString();
            ObjectStr = mValueStr;
        }
        public TtPrimitiveExpression(Rtti.TtTypeDesc type, object value)
        {
            Type = type;
            CalculateValueString(type, value);
        }
        public TtPrimitiveExpression(Rtti.TtTypeDesc type, bool typeIsTypeOf)
        {
            Type = Rtti.TtTypeDesc.TypeOf(typeof(System.Type));
            TypeIsTypeof = typeIsTypeOf;
            CalculateValueString(Type, type, typeIsTypeOf);
        }
        public TtPrimitiveExpression(Enum enumVal)
        {
            Type = Rtti.TtTypeDesc.TypeOf(enumVal.GetType());
            ValueStr = enumVal.ToString();
        }
        public void CalculateValueString(Rtti.TtTypeDesc type, object value, bool typeIsTypeof = true)
        {
            ObjectStr = value?.ToString();
            string retValue;
            if (value == null)
            {
                retValue = "null";
            }
            else if (type == Rtti.TtTypeDescGetter<bool>.TypeDesc)
            {
                var v = (bool)value;
                retValue = v ? "true" : "false";
            }
            else if (type == Rtti.TtTypeDescGetter<RName>.TypeDesc)
            {
                var v = (RName)value;
                retValue = $"EngineNS.RName.GetRName(\"{v.Name}\", EngineNS.RName.ERNameType.{v.RNameType})";
            }
            else if(type == Rtti.TtTypeDescGetter<System.Type>.TypeDesc)
            {
                var typeDesc = (Rtti.TtTypeDesc)value;
                if (typeIsTypeof)
                    retValue = $"typeof({typeDesc.FullName})";
                else
                    retValue = typeDesc.FullName;
            }
            else if(type == Rtti.TtTypeDescGetter<Color4f>.TypeDesc)
            {
                retValue = $"new EngineNS.Color4f({value.ToString()})";
            }
            else if (type == Rtti.TtTypeDescGetter<Color3f>.TypeDesc)
            {
                retValue = $"new EngineNS.Color3f({value.ToString()})";
            }
            else
            {
                retValue = value.ToString();
            }

            ValueStr = retValue;
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
            else if (Type.IsEqual(typeof(bool)))
                return System.Convert.ToBoolean(ValueStr);
            else if (Type.IsEqual(typeof(Vector2)))
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
            else if (Type.IsEqual(typeof(Matrix)))
                return Matrix.FromString(ValueStr);
            else if (Type.IsEqual(typeof(RName)))
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
            else if (Type.IsEqual(typeof(System.Type)))
            {
                var idxStart = ValueStr.IndexOf('(');
                var idxEnd = ValueStr.IndexOf(')');
                return Rtti.TtTypeDesc.TypeOfFullName(ValueStr.Substring(idxStart, idxEnd - idxStart));
            }
            else if(Type.IsEnum)
            {
                object outValue;
                if (System.Enum.TryParse(Type.SystemType, ValueStr, out outValue))
                    return outValue;
            }
            return null;
        }
        public void SetValue<T>(T value) where T : unmanaged
        {
            Type = Rtti.TtTypeDesc.TypeOf(typeof(T));
            ValueStr = value.ToString();
        }
        public void SetValue(string value)
        {
            Type = Rtti.TtTypeDesc.TypeOf(typeof(string));
            ValueStr = value;
        }
    }

    [Rtti.Meta(NameAlias = new string[] { "EngineNS.Bricks.CodeBuilder.UCastExpression@EngineCore", "EngineNS.Bricks.CodeBuilder.UCastExpression" })]
    public class TtCastExpression : TtExpressionBase, IO.ISerializer
    {
        [Rtti.Meta]
        public TtTypeReference TargetType { get; set; }
        [Rtti.Meta]
        public TtTypeReference SourceType { get; set; }
        [Rtti.Meta]
        public TtExpressionBase Expression { get; set; }

        public override bool Equals(object obj)
        {
            var ce = obj as TtCastExpression;
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

    [Rtti.Meta(NameAlias = new string[] { "EngineNS.Bricks.CodeBuilder.UCreateObjectExpression@EngineCore", "EngineNS.Bricks.CodeBuilder.UCreateObjectExpression" })]
    public class TtCreateObjectExpression : TtExpressionBase, IO.ISerializer
    {
        [Rtti.Meta]
        public string TypeName { get; set; }
        [Rtti.Meta]
        public List<TtExpressionBase> Parameters { get; set; } = new List<TtExpressionBase>();

        public TtCreateObjectExpression(string typeName, params TtExpressionBase[] exps)
        {
            TypeName = typeName;
            Parameters.AddRange(exps);
        }

        public override bool Equals(object obj)
        {
            var co = obj as TtCreateObjectExpression;
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

    [Rtti.Meta(NameAlias = new string[] { "EngineNS.Bricks.CodeBuilder.UDefaultValueExpression@EngineCore", "EngineNS.Bricks.CodeBuilder.UDefaultValueExpression" })]
    public class TtDefaultValueExpression : TtExpressionBase, IO.ISerializer
    {
        [Rtti.Meta]
        public TtTypeReference Type { get; set; }
        public TtDefaultValueExpression() { }
        public TtDefaultValueExpression(Rtti.TtTypeDesc type)
        {
            Type = new TtTypeReference(type);
        }
        public TtDefaultValueExpression(Type type)
        {
            Type = new TtTypeReference(type);
        }
        public TtDefaultValueExpression(TtTypeReference type)
        {
            Type = type;
        }

        public override bool Equals(object obj)
        {
            var val = obj as TtDefaultValueExpression;
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

    [Rtti.Meta(NameAlias = new string[] { "EngineNS.Bricks.CodeBuilder.UNullValueExpression@EngineCore", "EngineNS.Bricks.CodeBuilder.UNullValueExpression" })]
    public class TtNullValueExpression : TtExpressionBase 
    {
        public override bool Equals(object obj)
        {
            var val = obj as TtNullValueExpression;
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
    public class TtTypeOfExpression : TtExpressionBase
    {
        [Rtti.Meta]
        public TtTypeReference Variable { get; set; }

        public TtTypeOfExpression()
        {

        }
        public TtTypeOfExpression(TtTypeReference val)
        {
            Variable = val;
        }
        public TtTypeOfExpression(Rtti.TtTypeDesc type)
        {
            Variable = new TtTypeReference(type);
        }

        public override bool Equals(object obj)
        {
            var val = obj as TtTypeOfExpression;
            if (val == null)
                return false;
            return Variable.Equals(val.Variable);
        }
        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }
        public override string ToString()
        {
            return $"typeof({Variable.ToString()})";
        }
    }

    [Rtti.Meta(NameAlias = new string[] { "EngineNS.Bricks.CodeBuilder.UExecuteSequenceStatement@EngineCore", "EngineNS.Bricks.CodeBuilder.UExecuteSequenceStatement" })]
    public class TtExecuteSequenceStatement : TtStatementBase, IO.ISerializer
    {
        [Rtti.Meta]
        public List<TtStatementBase> Sequence { get; set; } = new List<TtStatementBase>();

        public TtExecuteSequenceStatement()
        {

        }
        public TtExecuteSequenceStatement(params TtStatementBase[] statements)
        {
            for (int i = 0; i < statements.Length; i++)
                Sequence.Add(statements[i]);
        }

        public override bool Equals(object obj)
        {
            var val = obj as TtExecuteSequenceStatement;
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
        public TtStatementBase FindStatement(TtStatementBase statement)
        {
            for(int i=0; i<Sequence.Count; i++)
            {
                if (Sequence[i].Equals(statement))
                    return Sequence[i];
            }
            return null;
        }
    }

    [Rtti.Meta(NameAlias = new string[] { "EngineNS.Bricks.CodeBuilder.UReturnStatement@EngineCore", "EngineNS.Bricks.CodeBuilder.UReturnStatement" })]
    public class TtReturnStatement : TtStatementBase 
    {
        public override bool Equals(object obj)
        {
            var val = obj as TtReturnStatement;
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

    [Rtti.Meta(NameAlias = new string[] { "EngineNS.Bricks.CodeBuilder.UIfStatement@EngineCore", "EngineNS.Bricks.CodeBuilder.UIfStatement" })]
    public class TtIfStatement : TtStatementBase, IO.ISerializer
    {
        [Rtti.Meta]
        public TtExpressionBase Condition { get; set; }
        [Rtti.Meta]
        public TtStatementBase TrueStatement { get; set; }
        [Rtti.Meta]
        public TtStatementBase FalseStatement { get; set; }
        [Rtti.Meta]
        public List<TtIfStatement> ElseIfs { get; set; } = new List<TtIfStatement>();

        public override bool Equals(object obj)
        {
            var val = obj as TtIfStatement;
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

    [Rtti.Meta(NameAlias = new string[] { "EngineNS.Bricks.CodeBuilder.UForLoopStatement@EngineCore", "EngineNS.Bricks.CodeBuilder.UForLoopStatement" })]
    public class TtForLoopStatement : TtStatementBase, IO.ISerializer
    {
        [Rtti.Meta]
        public bool IncludeEnd { get; set; } = false;
        [Rtti.Meta]
        public string LoopIndexName { get; set; }
        [Rtti.Meta]
        public TtExpressionBase BeginExpression { get; set; }
        [Rtti.Meta]
        public TtExpressionBase EndExpression { get; set; }
        [Rtti.Meta]
        public TtExpressionBase StepExpression { get; set; }
        [Rtti.Meta]
        public TtStatementBase LoopBody { get; set; }

        public override bool Equals(object obj)
        {
            var val = obj as TtForLoopStatement;
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

    [Rtti.Meta(NameAlias = new string[] { "EngineNS.Bricks.CodeBuilder.UWhileLoopStatement@EngineCore", "EngineNS.Bricks.CodeBuilder.UWhileLoopStatement" })]
    public class TtWhileLoopStatement : TtStatementBase, IO.ISerializer
    {
        [Rtti.Meta]
        public TtExpressionBase Condition { get; set; }
        [Rtti.Meta]
        public TtStatementBase LoopBody { get; set; }

        public override bool Equals(object obj)
        {
            var val = obj as TtWhileLoopStatement;
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

    [Rtti.Meta(NameAlias = new string[] { "EngineNS.Bricks.CodeBuilder.UContinueStatement@EngineCore", "EngineNS.Bricks.CodeBuilder.UContinueStatement" })]
    public class TtContinueStatement : TtStatementBase 
    {
        public override bool Equals(object obj)
        {
            var val = obj as TtContinueStatement;
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
    [Rtti.Meta(NameAlias = new string[] { "EngineNS.Bricks.CodeBuilder.UBreakStatement@EngineCore", "EngineNS.Bricks.CodeBuilder.UBreakStatement" })]
    public class TtBreakStatement : TtStatementBase
    {
        public override bool Equals(object obj)
        {
            var val = obj as TtBreakStatement;
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

    [Rtti.Meta(NameAlias = new string[] { "EngineNS.Bricks.CodeBuilder.UCommentStatement@EngineCore", "EngineNS.Bricks.CodeBuilder.UCommentStatement" })]
    public class TtCommentStatement : TtStatementBase, IO.ISerializer
    {
        [Rtti.Meta]
        public string CommentString { get; set; }
        public TtCommentStatement(string comment)
        {
            CommentString = comment;
        }
        public override bool Equals(object obj)
        {
            var val = obj as TtCommentStatement;
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
    [Rtti.Meta(NameAlias = new string[] { "EngineNS.Bricks.CodeBuilder.UExpressionStatement@EngineCore", "EngineNS.Bricks.CodeBuilder.UExpressionStatement" })]
    public class TtExpressionStatement : TtStatementBase, IO.ISerializer
    {
        [Rtti.Meta]
        public TtExpressionBase Expression { get; set; }
        [Rtti.Meta]
        public TtStatementBase NextStatement { get; set; }
        public TtExpressionStatement(TtExpressionBase exp)
        {
            Expression = exp;
        }

        public override bool Equals(object obj)
        {
            var val = obj as TtExpressionStatement;
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
    [Rtti.Meta(NameAlias = new string[] { "EngineNS.Bricks.CodeBuilder.UTest_Expressions@EngineCore", "EngineNS.Bricks.CodeBuilder.UTest_Expressions" })]
    public class TtTest_Expressions
    {
        public static TtNamespaceDeclaration GetTestNamespace()
        {
            var ns = new TtNamespaceDeclaration("ExpressionTest");
            var cls = new TtClassDeclaration()
            {
                VisitMode = EVisisMode.Public,
                IsStruct = false,
                ClassName = "TestClass",
            };
            cls.SupperClassNames.Add("SuperClass");
            ns.Classes.Add(cls);

            var publicIntVar = new TtVariableDeclaration()
            {
                VariableType = new TtTypeReference(typeof(int)),
                VariableName = "PublicIntVal",
                InitValue = new TtPrimitiveExpression(0),
                Comment = new TtCommentStatement("this is a test int value"),
                VisitMode = EVisisMode.Public,
            };
            cls.Properties.Add(publicIntVar);
            var protectStringVar = new TtVariableDeclaration()
            {
                VariableType = new TtTypeReference(typeof(string)),
                VariableName = "ProtectStringVar",
                InitValue = new TtPrimitiveExpression("string value"),
                Comment = new TtCommentStatement("this is a test string value"),
                VisitMode = EVisisMode.Protected,
            };
            cls.Properties.Add(protectStringVar);
            var privateBooleanVar = new TtVariableDeclaration()
            {
                VariableType = new TtTypeReference(typeof(bool)),
                VariableName = "ProtectBooleanVar",
                InitValue = new TtPrimitiveExpression(true),
                Comment = new TtCommentStatement("this is a test boolean value"),
                VisitMode = EVisisMode.Protected,
            };
            cls.Properties.Add(privateBooleanVar);
            var localInt8Var = new TtVariableDeclaration()
            {
                VariableType = new TtTypeReference(typeof(sbyte)),
                VariableName = "LocalInt8Var",
                InitValue = new TtPrimitiveExpression((sbyte)1),
                Comment = new TtCommentStatement("this is a test Int8 value"),
                VisitMode = EVisisMode.Protected,
            };
            cls.Properties.Add(localInt8Var);
            var internalUInt8Var = new TtVariableDeclaration()
            {
                VariableType = new TtTypeReference(typeof(byte)),
                VariableName = "ProtectUInt8Var",
                InitValue = new TtPrimitiveExpression((byte)2),
                Comment = new TtCommentStatement("this is a test UInt8 value"),
                VisitMode = EVisisMode.Protected,
            };
            cls.Properties.Add(internalUInt8Var);
            var int16Var = new TtVariableDeclaration()
            {
                VariableType = new TtTypeReference(typeof(Int16)),
                VariableName = "Int16Var",
                InitValue = new TtPrimitiveExpression((Int16)3),
            };
            cls.Properties.Add(int16Var);
            var int32Var = new TtVariableDeclaration()
            {
                VariableType = new TtTypeReference(typeof(Int32)),
                VariableName = "Int32Var",
                InitValue = new TtPrimitiveExpression((Int32)4),
            };
            cls.Properties.Add(int32Var);
            var int64Var = new TtVariableDeclaration()
            {
                VariableType = new TtTypeReference(typeof(Int64)),
                VariableName = "Int64Var",
                InitValue = new TtPrimitiveExpression((Int64)5),
            };
            cls.Properties.Add(int64Var);
            var uInt16Var = new TtVariableDeclaration()
            {
                VariableType = new TtTypeReference(typeof(UInt16)),
                VariableName = "UInt16Var",
                InitValue = new TtPrimitiveExpression((UInt16)6),
            };
            cls.Properties.Add(uInt16Var);
            var uInt32Var = new TtVariableDeclaration()
            {
                VariableType = new TtTypeReference(typeof(UInt32)),
                VariableName = "UInt32Var",
                InitValue = new TtPrimitiveExpression((UInt32)7),
            };
            cls.Properties.Add(uInt32Var);
            var uInt64Var = new TtVariableDeclaration()
            {
                VariableType = new TtTypeReference(typeof(UInt64)),
                VariableName = "UInt64Var",
                InitValue = new TtPrimitiveExpression((UInt64)8),
            };
            cls.Properties.Add(uInt64Var);
            var floatVar = new TtVariableDeclaration()
            {
                VariableType = new TtTypeReference(typeof(float)),
                VariableName = "FloatVar",
                InitValue = new TtPrimitiveExpression((float)8),
            };
            cls.Properties.Add(floatVar);
            var doubleVar = new TtVariableDeclaration()
            {
                VariableType = new TtTypeReference(typeof(double)),
                VariableName = "DoubleVar",
                InitValue = new TtPrimitiveExpression((double)8),
            };
            cls.Properties.Add(doubleVar);

            var protectedMethod = new TtMethodDeclaration()
            {
                VisitMode = EVisisMode.Protected,
                MethodName = "ProtectedMethodTest",
                ReturnValue = int32Var,
                Comment = new TtCommentStatement("this is protected method test"),
                IsOverride = false,
            };
            cls.AddMethod(protectedMethod);
            var privateMethod = new TtMethodDeclaration()
            {
                VisitMode = EVisisMode.Private,
                MethodName = "PrivateMethodTest",
                ReturnValue = floatVar,
                Comment = new TtCommentStatement("this is private method test"),
                IsOverride = false,
            };
            cls.AddMethod(privateMethod);
            var publicMethod = new TtMethodDeclaration()
            {
                VisitMode = EVisisMode.Public,
                MethodName = "PublicMethodWithArgsTest",
                Comment = new TtCommentStatement("this is public method test"),
                ReturnValue = int32Var,
                IsOverride = false,
            };
            cls.AddMethod(publicMethod);
            publicMethod.Arguments.Add(new TtMethodArgumentDeclaration()
            {
                VariableType = new TtTypeReference(typeof(byte)),
                VariableName = "byteArg",
                InitValue = new TtPrimitiveExpression((byte)1),
                OperationType = EMethodArgumentAttribute.Default,
                IsParamArray = false,
            });
            publicMethod.Arguments.Add(new TtMethodArgumentDeclaration()
            {
                VariableType = new TtTypeReference(typeof(sbyte)),
                VariableName = "sbyteArg",
                InitValue = new TtPrimitiveExpression((sbyte)-1),
                OperationType = EMethodArgumentAttribute.In,
                IsParamArray = false,
            });
            publicMethod.Arguments.Add(new TtMethodArgumentDeclaration()
            {
                VariableType = new TtTypeReference(typeof(Int16)),
                VariableName = "int16Arg",
                InitValue = new TtPrimitiveExpression((Int16)(-2)),
                OperationType = EMethodArgumentAttribute.Out,
                IsParamArray = false,
            });
            publicMethod.Arguments.Add(new TtMethodArgumentDeclaration()
            {
                VariableType = new TtTypeReference(typeof(UInt16)),
                VariableName = "uint16Arg",
                InitValue = new TtPrimitiveExpression((UInt16)2),
                OperationType = EMethodArgumentAttribute.Ref,
                IsParamArray = false,
            });
            publicMethod.Arguments.Add(new TtMethodArgumentDeclaration()
            {
                VariableType = new TtTypeReference(typeof(int[])),
                VariableName = "paramArg",
                OperationType = EMethodArgumentAttribute.Default,
                IsParamArray = true,
            });

            var methodInvoke = new TtMethodInvokeStatement()
            {
                Host = new TtSelfReferenceExpression(),
                MethodName = "PublicMethodWithArgsTest",
            };
            methodInvoke.ReturnValue = int32Var;
            methodInvoke.Arguments.Add(new TtMethodInvokeArgumentExpression()
            {
                Expression = new TtPrimitiveExpression((byte)10),
            });
            methodInvoke.Arguments.Add(new TtMethodInvokeArgumentExpression()
            {
                Expression = new TtPrimitiveExpression((sbyte)(-10)),
                OperationType = EMethodArgumentAttribute.In,
            });
            methodInvoke.Arguments.Add(new TtMethodInvokeArgumentExpression()
            {
                Expression = new TtPrimitiveExpression((Int16)(-20)),
                OperationType = EMethodArgumentAttribute.Out,
            });
            methodInvoke.Arguments.Add(new TtMethodInvokeArgumentExpression()
            {
                Expression = new TtPrimitiveExpression((UInt16)20),
                OperationType = EMethodArgumentAttribute.Ref,
            });
            methodInvoke.Arguments.Add(new TtMethodInvokeArgumentExpression()
            {
                Expression = new TtPrimitiveExpression(0),
            });
            methodInvoke.Arguments.Add(new TtMethodInvokeArgumentExpression()
            {
                Expression = new TtPrimitiveExpression(1),
            });
            methodInvoke.Arguments.Add(new TtMethodInvokeArgumentExpression()
            {
                Expression = new TtPrimitiveExpression(2),
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