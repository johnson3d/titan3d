using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using CodeGenerateSystem.Base;
using EngineNS.IO;
using EngineNS.IO.Serializer;
using EngineNS.Rtti;

namespace CodeDomNode
{
    public interface IVariableTypeChangeProcessClass
    {
        void OnVariableTypeChanged(VariableType variableType);
    }

    [EngineNS.Rtti.MetaClass]
    public class VariableType : EngineNS.IO.Serializer.Serializer, INotifyPropertyChanged
    {
        #region INotifyPropertyChangedMembers
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
        }
        #endregion

        //public delegate void Delegate_VariableTypeChanged(VariableType vt);
        //public Delegate_VariableTypeChanged OnVariableTypeChanged;

        Type mType = typeof(Int32);
        [EngineNS.Rtti.MetaData]
        public Type Type
        {
            get => mType;
            set
            {
                mType = value;
                OnPropertyChanged("Type");
                //OnVariableTypeChanged?.Invoke(this);
            }
        }
        public enum enArrayType
        {
            Single,
            Array,
        }
        enArrayType mArrayType = enArrayType.Single;
        [EngineNS.Rtti.MetaData]
        public enArrayType ArrayType
        {
            get => mArrayType;
            set
            {
                mArrayType = value;
                OnPropertyChanged("ArrayType");
                //OnVariableTypeChanged?.Invoke(this);
            }
        }

        // 初始化时之声明不创建
        bool mNotCreateInInitialize = false;
        [EngineNS.Rtti.MetaData]
        public bool NotCreateInInitialize
        {
            get => mNotCreateInInitialize;
            set
            {
                mNotCreateInInitialize = value;
                OnPropertyChanged("NotCreateInInitialize");
            }
        }

        bool mIsMacrossGetter = false;
        [EngineNS.Rtti.MetaData]
        public bool IsMacrossGetter
        {
            get => mIsMacrossGetter;
            set
            {
                mIsMacrossGetter = value;
                OnPropertyChanged("IsMacrossGetter");
            }
        }

        Type mMacrossClassType = null;
        [EngineNS.Rtti.MetaData]
        [Browsable(false)]
        public Type MacrossClassType
        {
            get => mMacrossClassType;
            set
            {
                mMacrossClassType = value;
            }
        }

        string mTypeName = "Int32";
        [EngineNS.Rtti.MetaData]
        [Browsable(false)]
        public string TypeName
        {
            get => mTypeName;
            set
            {
                mTypeName = value;
                OnPropertyChanged("TypeName");
            }
        }

        public enum enTypeState
        {
            ObjectReference,
            ClassReference,
        }
        enTypeState mTypeState = enTypeState.ObjectReference;
        [EngineNS.Rtti.MetaData]
        public enTypeState TypeState
        {
            get => mTypeState;
            set
            {
                mTypeState = value;
                OnPropertyChanged("TypeState");
            }
        }

        EngineNS.ECSType mCSType = EngineNS.ECSType.Client;
        [EngineNS.Rtti.MetaData]
        public EngineNS.ECSType CSType
        {
            get => mCSType;
            set
            {
                mCSType = value;
                OnPropertyChanged("CSType");
            }
        }
        public VariableType()
        {

        }
        public VariableType(EngineNS.ECSType csType)
        {
            mCSType = csType;
        }
        public VariableType(Type type, EngineNS.ECSType csType)
        {
            Type = type;
            mCSType = csType;
        }

        public Type GetActualType()
        {
            switch (ArrayType)
            {
                case enArrayType.Single:
                    return Type;
                case enArrayType.Array:
                    {
                        var typeSaveStr = RttiHelper.GetTypeSaveString(Type);
                        if (typeSaveStr == null)
                            throw new InvalidOperationException("元素类型不合法");
                        //"Common|mscorlib@System.Collections.Generic.List`1[[Client|EngineCore@EngineNS.GamePlay.Actor.GActor]]";
                        var listTypeStr = $"Common|mscorlib@System.Collections.Generic.List`1[[{typeSaveStr}]]";
                        var arrayType = EngineNS.Rtti.RttiHelper.GetTypeFromSaveString(listTypeStr);
                        if (arrayType == null)
                            throw new InvalidOperationException($"数组类型{typeSaveStr}不合法");
                        return arrayType;
                    }
                default:
                    {
                        throw new InvalidOperationException("ArrayType不合法");
                    }
            }
        }
        public bool IsValueType()
        {
            return Type.IsValueType;
        }

        public override string ToString()
        {
            return (Type.FullName + ArrayType.ToString() + TypeState.ToString() + CSType.ToString());
        }
        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }
        public override bool Equals(object obj)
        {
            var val = obj as VariableType;
            if (val == null)
                return false;
            if ((Type == val.Type) &&
                (ArrayType == val.ArrayType) &&
                (TypeState == val.TypeState) &&
                (CSType == val.CSType))
                return true;
            return false;

        }
    }


    [EngineNS.Editor.Editor_DoNotShowBaseClassProperties]
    [EngineNS.Rtti.MetaClass]
    public class CustomMethodInfo : DependencyObject, Macross.ICategoryItemPropertyClass
    {
        ///////////////////////////////////////////////////////
        //Guid TestId = Guid.NewGuid();
        ///////////////////////////////////////////////////////
        #region INotifyPropertyChangedMembers
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
        }
        #endregion

        #region ISerializer
        public void ReadObject(EngineNS.IO.Serializer.IReader pkg)
        {
            EngineNS.IO.Serializer.SerializerHelper.ReadObject(this, pkg);
            for(int i=0; i<InParams.Count; i++)
            {
                InParams[i].HostMethodInfo = this;
            }
            for(int i=0; i<OutParams.Count; i++)
            {
                OutParams[i].HostMethodInfo = this;
            }
        }
        public void ReadObject(EngineNS.IO.Serializer.IReader pkg, EngineNS.Rtti.MetaData metaData)
        {
            EngineNS.IO.Serializer.SerializerHelper.ReadObject(this, pkg, metaData);
            for (int i = 0; i < InParams.Count; i++)
            {
                InParams[i].HostMethodInfo = this;
            }
            for (int i = 0; i < OutParams.Count; i++)
            {
                OutParams[i].HostMethodInfo = this;
            }
        }

        public void ReadObjectXML(EngineNS.IO.XmlNode node)
        {
            EngineNS.IO.Serializer.SerializerHelper.ReadObjectXML(this, node);
            for (int i = 0; i < InParams.Count; i++)
            {
                InParams[i].HostMethodInfo = this;
            }
            for (int i = 0; i < OutParams.Count; i++)
            {
                OutParams[i].HostMethodInfo = this;
            }
        }

        public void WriteObject(EngineNS.IO.Serializer.IWriter pkg)
        {
            EngineNS.IO.Serializer.SerializerHelper.WriteObject(this, pkg);
        }
        public void WriteObject(EngineNS.IO.Serializer.IWriter pkg, EngineNS.Rtti.MetaData metaData)
        {
            EngineNS.IO.Serializer.SerializerHelper.WriteObject(this, pkg, metaData);
        }

        public void WriteObjectXML(EngineNS.IO.XmlNode node)
        {
            EngineNS.IO.Serializer.SerializerHelper.WriteObjectXML(this, node);
        }
        public EngineNS.IO.Serializer.ISerializer CloneObject()
        {
            return EngineNS.IO.Serializer.SerializerHelper.CloneObject(this);
        }
        #endregion

        public Macross.INodesContainerDicKey GetNodesContainerDicKey(Guid keyId) { return null; }

        [Browsable(false)]
        [EngineNS.Rtti.MetaData]
        public EngineNS.ECSType CSType { get; set; } = EngineNS.ECSType.Common;

        [EngineNS.Rtti.MetaData]
        public bool OverrideAble
        {
            get { return (bool)GetValue(OverrideAbleProperty); }
            set { SetValue(OverrideAbleProperty, value); }
        }
        public static readonly DependencyProperty OverrideAbleProperty = DependencyProperty.Register("OverrideAble", typeof(bool), typeof(CustomMethodInfo), new UIPropertyMetadata(false, null));
        [EngineNS.Rtti.MetaData]
        public string MethodName
        {
            get { return (string)GetValue(MethodNameProperty); }
            set { SetValue(MethodNameProperty, value); }
        }
        public static readonly DependencyProperty MethodNameProperty = DependencyProperty.Register("MethodName", typeof(string), typeof(CustomMethodInfo), new UIPropertyMetadata("", OnMethodNamePropertyChanged));
        private static void OnMethodNamePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = sender as CustomMethodInfo;
            ctrl.OnPropertyChanged("DisplayName");
        }
        string mDisplayName = "";
        [EngineNS.Rtti.MetaData]
        [Browsable(false)]
        public string DisplayName
        {
            get
            {
                if (string.IsNullOrEmpty(mDisplayName))
                    return MethodName;
                return mDisplayName;
            }
            set
            {
                mDisplayName = value;
                OnPropertyChanged("DisplayName");
            }
        }

        [EngineNS.Rtti.MetaData]
        [Browsable(false)]
        public bool IsDelegateEvent
        {
            get;
            set;
        } = false;

        [EngineNS.Rtti.MetaData]
        public string Tooltip
        {
            get { return (string)GetValue(TooltipProperty); }
            set { SetValue(TooltipProperty, value); }
        }
        public static readonly DependencyProperty TooltipProperty = DependencyProperty.Register("Tooltip", typeof(string), typeof(CustomMethodInfo), new UIPropertyMetadata(""));

        [EngineNS.Rtti.MetaData]
        public bool IsAsync
        {
            get { return (bool)GetValue(IsAsyncProperty); }
            set { SetValue(IsAsyncProperty, value); }
        }
        public static readonly DependencyProperty IsAsyncProperty = DependencyProperty.Register("IsAsync", typeof(bool), typeof(CustomMethodInfo), new UIPropertyMetadata(false, new PropertyChangedCallback(IsAsyncPropertyChangedCallback), new CoerceValueCallback(IsAsyncCoerceValueCallback)));
        static void IsAsyncPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var methodInfo = d as CustomMethodInfo;
            var newValue = (bool)e.NewValue;

        }
        static object IsAsyncCoerceValueCallback(DependencyObject d, object baseValue)
        {
            var methodInfo = d as CustomMethodInfo;
            if (methodInfo.OutParams.Count > 1)
            {
                EditorCommon.MessageBox.Show("函数包含多个输出参数，不能设置为异步，异步函数只能包含最多一个输出参数");
                return false;
            }
            return baseValue;
        }
  //      [EngineNS.Rtti.MetaData]
  //     public List<Attribute> Attributes = new List<Attribute>();

        [EngineNS.Rtti.MetaClass]
        [EngineNS.Editor.Editor_DoNotShowBaseClassProperties]
        public class FunctionParam : DependencyObject, EngineNS.IO.Serializer.ISerializer, CodeDomNode.IVariableTypeChangeProcessClass
        {
            ///////////////////////////////////////////////////////
            //Guid TestId = Guid.NewGuid();
            ///////////////////////////////////////////////////////

            public CustomMethodInfo HostMethodInfo;

            [EngineNS.Rtti.MetaData]
            public string ParamName
            {
                get { return (string)GetValue(ParamNameProperty); }
                set { SetValue(ParamNameProperty, value); }
            }
            public static readonly DependencyProperty ParamNameProperty = DependencyProperty.Register("ParamName", typeof(string), typeof(FunctionParam), new UIPropertyMetadata("", null, new CoerceValueCallback(ParamNameCoerceValueCallback)));
            static object ParamNameCoerceValueCallback(DependencyObject d, object baseValue)
            {
                var funcParam = d as FunctionParam;
                if (funcParam.HostMethodInfo == null)
                    return baseValue;
                // 判断参数名称是否重复
                var newValue = (string)baseValue;
                var inParams = funcParam.HostMethodInfo.InParams;
                for (int i=0; i<inParams.Count; i++)
                {
                    if (inParams[i] == funcParam)
                        continue;

                    if(inParams[i].ParamName == newValue)
                    {
                        // 重复
                        return funcParam.ParamName;
                    }
                }
                var outParams = funcParam.HostMethodInfo.OutParams;
                for (int i = 0; i < outParams.Count; i++)
                {
                    if (outParams[i] == funcParam)
                        continue;

                    if (outParams[i].ParamName == newValue)
                    {
                        // 重复
                        return funcParam.ParamName;
                    }
                }
                return baseValue;
            }


            [EngineNS.Editor.Editor_PropertyGridDataTemplateAttribute("TypeSetterTemplate")]
            [EngineNS.Rtti.MetaData]
            public VariableType ParamType
            {
                get { return (VariableType)GetValue(ParamTypeProperty); }
                set { SetValue(ParamTypeProperty, value); }
            }
            public List<Attribute> Attributes { get; set; } = new List<Attribute>();
            ObservableCollection<Macross.AttributeType> mAttributeTypeProxys = new ObservableCollection<Macross.AttributeType>();
            [EngineNS.Rtti.MetaData]
            [EngineNS.Editor.Editor_ListCustomAddRemoveActionAttribute(typeof(Macross.VariableCategoryItemPropertys.CustomAddRemoveAttributeTypeProxysPrivider))]
            public ObservableCollection<Macross.AttributeType> AttributeTypeProxys
            {
                get
                {
                    return mAttributeTypeProxys;
                }
                set
                {
                    mAttributeTypeProxys = value;
                }
            }

            
            public static readonly DependencyProperty ParamTypeProperty = DependencyProperty.Register("ParamType", typeof(VariableType), typeof(FunctionParam), new FrameworkPropertyMetadata(null));

            public delegate void Delegate_OnParamTypeChanged(VariableType newType);
            public event Delegate_OnParamTypeChanged OnParamTypeChanged;
            public void OnVariableTypeChanged(CodeDomNode.VariableType variableType)
            {
                ParamType = variableType;
                OnParamTypeChanged?.Invoke(variableType);
            }

            public bool IsEqual(FunctionParam param)
            {
                if ((ParamName == param.ParamName) &&
                   (ParamType.Equals(param.ParamType)))
                {
                        return true;
                }
                return false;
            }
            public override string ToString()
            {
                return ParamName + ParamType.ToString();
            }

            public MethodParamInfoAssist CreateParamInfoAssist(System.CodeDom.FieldDirection dir)
            {
                var retValue = new MethodParamInfoAssist();
                retValue.FieldDirection = dir;
                retValue.IsParamsArray = false;
                retValue.ParameterType = ParamType.GetActualType();
                retValue.ParamName = ParamName;
                BindingOperations.SetBinding(this, ParamNameProperty, new Binding("ParamName") { Source = retValue, Mode = BindingMode.TwoWay });
                foreach (var att in Attributes)
                {
                    if (att is EngineNS.Editor.Editor_MacrossMethodParamTypeAttribute)
                    {
                        retValue.ParameterDisplayType = ((EngineNS.Editor.Editor_MacrossMethodParamTypeAttribute)att).ParamType;
                    }
                }
                return retValue;
            }
            
            #region ISerializer
            public void ReadObjectXML(XmlNode node)
            {
                EngineNS.IO.Serializer.SerializerHelper.ReadObjectXML(this, node);
            }

            public void WriteObjectXML(XmlNode node)
            {
                EngineNS.IO.Serializer.SerializerHelper.WriteObjectXML(this, node);
            }

            public void ReadObject(IReader pkg)
            {
                EngineNS.IO.Serializer.SerializerHelper.ReadObject(this, pkg);
            }

            public void ReadObject(IReader pkg, MetaData metaData)
            {
                EngineNS.IO.Serializer.SerializerHelper.ReadObject(this, pkg, metaData);
            }

            public void WriteObject(IWriter pkg)
            {
                EngineNS.IO.Serializer.SerializerHelper.WriteObject(this, pkg);
            }

            public void WriteObject(IWriter pkg, MetaData metaData)
            {
                EngineNS.IO.Serializer.SerializerHelper.WriteObject(this, pkg, metaData);
            }
            public EngineNS.IO.Serializer.ISerializer CloneObject()
            {
                return EngineNS.IO.Serializer.SerializerHelper.CloneObject(this);
            }
            #endregion
        }

        [EngineNS.Rtti.MetaData]
        [EngineNS.Editor.Editor_PropertyGridDataTemplateAttribute("MethodParamSetterTemplate")]
        public ObservableCollection<FunctionParam> InParams
        {
            get { return (ObservableCollection<FunctionParam>)GetValue(InParamsProperty); }
            set { SetValue(InParamsProperty, value); }
        }
        public static readonly DependencyProperty InParamsProperty = DependencyProperty.Register("InParams", typeof(ObservableCollection<FunctionParam>), typeof(CustomMethodInfo), new UIPropertyMetadata(null));

        [EngineNS.Rtti.MetaData]
        [EngineNS.Editor.Editor_PropertyGridDataTemplateAttribute("MethodParamSetterTemplate")]
        public ObservableCollection<FunctionParam> OutParams
        {
            get { return (ObservableCollection<FunctionParam>)GetValue(OutParamsProperty); }
            set { SetValue(OutParamsProperty, value); }
        }
        public static readonly DependencyProperty OutParamsProperty = DependencyProperty.Register("OutParams", typeof(ObservableCollection<FunctionParam>), typeof(CustomMethodInfo), new UIPropertyMetadata(null));
                
        public CustomMethodInfo()
        {
            InParams = new ObservableCollection<FunctionParam>();
            OutParams = new ObservableCollection<FunctionParam>();
        }
        public bool IsEqual(CustomMethodInfo info)
        {
            if (info == null)
                return false;
            if ((MethodName != info.MethodName) || (Tooltip != info.Tooltip) || (InParams.Count != info.InParams.Count) || (OutParams.Count != info.OutParams.Count))
                return false;
            for (int i = 0; i < InParams.Count; i++)
            {
                if (!InParams[i].IsEqual(info.InParams[i]))
                    return false;
            }
            for (int i = 0; i < OutParams.Count; i++)
            {
                if (!OutParams[i].IsEqual(info.OutParams[i]))
                    return false;
            }
            return true;
        }
        public override string ToString()
        {
            string retVal = MethodName;
            foreach(var param in InParams)
            {
                retVal += param.ToString();
            }
            foreach(var param in OutParams)
            {
                retVal += param.ToString();
            }

            return retVal;
        }

        public interface IFunctionResetOperationNode
        {
            Task ResetMethodInfo(CustomMethodInfo methodInfo);
            CustomMethodInfo GetMethodInfo();
        }
        public interface IFunctionInParamOperation
        {
            Task OnAddedInParam(FunctionParam param);
            Task OnInsertInParam(int index, FunctionParam param);
            Task OnRemovedInParam(int index, FunctionParam param);
        }
        public interface IFunctionOutParamOperation
        {
            Task OnAddedOutParam(FunctionParam param);
            Task OnInsertOutParam(int index, FunctionParam param);
            Task OnRemovedOutParam(int index, FunctionParam param);
        }

        //public delegate void Delegate_OnProcessParam(FunctionParam param);
        //public event Delegate_OnProcessParam OnAddedInParam;
        //public delegate void Delegate_OnRemoveParam(int index, FunctionParam param);
        //public event Delegate_OnRemoveParam OnRemovedInParam;
        List<IFunctionInParamOperation> mInParamOperations = new List<IFunctionInParamOperation>();
        public void AddInParamOperation(IFunctionInParamOperation op)
        {
            if (!mInParamOperations.Contains(op))
                mInParamOperations.Add(op);
        }
        public void RemoveInParamOperation(IFunctionInParamOperation op)
        {
            mInParamOperations.Remove(op);
        }
        public void _OnAddedInParam(FunctionParam param)
        {
            //OnAddedInParam?.Invoke(param);
            var array = mInParamOperations.ToArray();
            foreach (var op in array)
            {
                op.OnAddedInParam(param);
            }
        }
        public void _OnInsertInParam(int index, FunctionParam param)
        {
            var array = mInParamOperations.ToArray();
            foreach(var op in array)
            {
                op.OnInsertInParam(index, param);
            }
        }
        public void _OnRemovedInParam(int index, FunctionParam param)
        {
            //OnRemovedInParam?.Invoke(index, param);
            var array = mInParamOperations.ToArray();
            foreach(var op in array)
            {
                op.OnRemovedInParam(index, param);
            }
        }
        //public event Delegate_OnProcessParam OnAddedOutParam;
        //public event Delegate_OnRemoveParam OnRemovedOutParam;
        List<IFunctionOutParamOperation> mOutParamOperations = new List<IFunctionOutParamOperation>();
        public void AddOutParamOperation(IFunctionOutParamOperation op)
        {
            if (!mOutParamOperations.Contains(op))
                mOutParamOperations.Add(op);
        }
        public void RemoveOutParamOperation(IFunctionOutParamOperation op)
        {
            mOutParamOperations.Remove(op);
        }
        public void _OnAddedOutParam(FunctionParam param)
        {
            //OnAddedOutParam?.Invoke(param);
            var array = mOutParamOperations.ToArray();
            foreach (var op in array)
                op.OnAddedOutParam(param);
        }
        public void _OnRemovedOutParam(int index, FunctionParam param)
        {
            //OnRemovedOutParam?.Invoke(index, param);
            var array = mOutParamOperations.ToArray();
            foreach (var op in array)
                op.OnRemovedOutParam(index, param);
        }
    }

    [CodeGenerateSystem.CustomConstructionParams(typeof(MethodCustomConstructParam))]
    public partial class MethodCustom : CodeGenerateSystem.Base.BaseNodeControl, CodeDomNode.CustomMethodInfo.IFunctionInParamOperation, CustomMethodInfo.IFunctionResetOperationNode, CodeGenerateSystem.Base.IMethodGenerator
    {
        [EngineNS.Rtti.MetaClass]
        public class MethodCustomConstructParam : CodeGenerateSystem.Base.ConstructionParams
        {
            [EngineNS.Rtti.MetaData]
            public bool IsShowProperty = true;
            CustomMethodInfo mMethodInfo;
            [EngineNS.Rtti.MetaData]
            public CustomMethodInfo MethodInfo
            {
                get => mMethodInfo;
                set
                {
                    if (mMethodInfo == value)
                        return;
                    //if(mMethodInfo != null)
                    //{
                    //    mMethodInfo.OnAddedInParam = null;
                    //    mMethodInfo.OnRemovedInParam = null;
                    //}

                    mMethodInfo = value;
                }
            }
            

            public MethodCustomConstructParam()
            {
            }
            public override EditorCommon.CodeGenerateSystem.INodeConstructionParams Duplicate()
            {
                var retVal = base.Duplicate() as MethodCustomConstructParam;
                retVal.MethodInfo = MethodInfo;
                retVal.IsShowProperty = IsShowProperty;
                return retVal;
            }
            public override bool Equals(object obj)
            {
                if (!base.Equals(obj))
                    return false;
                var param = obj as MethodCustomConstructParam;
                if (param == null)
                    return false;
                if (MethodInfo != param.MethodInfo)
                    return false;
                return true;
            }
            public override int GetHashCode()
            {
                return (base.GetHashCodeString() + MethodInfo.ToString()).GetHashCode();
            }
            //public override void Write(EngineNS.IO.XndNode xndNode)
            //{
            //    var att = xndNode.AddAttrib("ConstructionParams");
            //    att.Version = 0;
            //    att.BeginWrite();
            //    att.Write(ConstructParam);
            //    att.WriteMetaObject(MethodInfo);
            //    att.EndWrite();
            //}
            public override void Read(EngineNS.IO.XndNode xndNode)
            {
                var att = xndNode.FindAttrib("ConstructionParams");
                if (att != null)
                {
                    att.BeginRead();
                    switch (att.Version)
                    {
                        case 0:
                            att.Read(out mConstructParam);
                            if (MethodInfo == null)
                                MethodInfo = new CustomMethodInfo();
                            att.ReadMetaObject(MethodInfo);
                            break;
                        case 1:
                            att.ReadMetaObject(this);
                            break;
                    }
                    att.EndRead();
                }
            }
        }

        CodeGenerateSystem.Base.LinkPinControl mCtrlMethodPin_Next = new CodeGenerateSystem.Base.LinkPinControl();
        StackPanel mParamsPanel = null;

        partial void InitConstruction();
        public MethodCustom(CodeGenerateSystem.Base.ConstructionParams csParam)
            : base(csParam)
        {
            InitConstruction();

            IsOnlyReturnValue = true;
            AddLinkPinInfo("CtrlMethodPin_Next", mCtrlMethodPin_Next, null);

            var param = csParam as MethodCustomConstructParam;
            //param.MethodInfo.OnAddedInParam -= AddParam;
            //param.MethodInfo.OnAddedInParam += AddParam;
            //param.MethodInfo.OnRemovedInParam -= RemoveParam;
            //param.MethodInfo.OnRemovedInParam += RemoveParam;
            param.MethodInfo.AddInParamOperation(this);

            mParamsPanel?.Children.Clear();
            foreach(var methodParam in param.MethodInfo.InParams)
            {
                // AddParam(methodParam);
                var noUse = OnAddedInParam(methodParam);
            }
        }

        //public void AddParam(CustomMethodInfo.FunctionParam funcParam)
        public async Task OnAddedInParam(CodeDomNode.CustomMethodInfo.FunctionParam funcParam)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            var csParam = CSParam as MethodCustomConstructParam;
            bool needref = false;
            if (funcParam.ParamType.Type.IsByRef)
            {
                string name = funcParam.ParamType.Type.FullName.Remove(funcParam.ParamType.Type.FullName.Length - 1);
                Type temp = funcParam.ParamType.Type.Assembly.GetType(name);
                if (temp != null && temp.IsClass == false)
                {
                    //fp.ParamType = new CodeDomNode.VariableType(funcParam.ParamType.Type, HostControl.CSType);
                    funcParam.ParamType.Type = temp;
                    needref = true;
                }
            }
            var pm = new MethodInvokeParameterControl.MethodInvokeParameterConstructionParams()
            {
                CSType = csParam.CSType,
                HostNodesContainer = csParam.HostNodesContainer,
                ConstructType = MethodInvokeNode.enParamConstructType.MethodCustom,
                ParamInfo = funcParam.CreateParamInfoAssist(needref == false ? System.CodeDom.FieldDirection.In : System.CodeDom.FieldDirection.Ref),
            };
            var paramCtrl = new MethodInvokeParameterControl(pm);
            funcParam.OnParamTypeChanged -= paramCtrl.UpdateParamType;
            funcParam.OnParamTypeChanged += paramCtrl.UpdateParamType;
            AddChildNode(paramCtrl, mParamsPanel);
        }
        public async Task OnInsertInParam(int index, CodeDomNode.CustomMethodInfo.FunctionParam funcParam)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            var csParam = CSParam as MethodCustomConstructParam;
            var pm = new MethodInvokeParameterControl.MethodInvokeParameterConstructionParams()
            {
                CSType = csParam.CSType,
                HostNodesContainer = csParam.HostNodesContainer,
                ConstructType = MethodInvokeNode.enParamConstructType.MethodCustom,
                ParamInfo = funcParam.CreateParamInfoAssist(System.CodeDom.FieldDirection.In),
            };
            var paramCtrl = new MethodInvokeParameterControl(pm);
            funcParam.OnParamTypeChanged -= paramCtrl.UpdateParamType;
            funcParam.OnParamTypeChanged += paramCtrl.UpdateParamType;
            InsertChildNode(index, paramCtrl, mParamsPanel);
        }
        //void RemoveParam(int index, CustomMethodInfo.FunctionParam funcParam)
        public async Task OnRemovedInParam(int index, CodeDomNode.CustomMethodInfo.FunctionParam param)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            var node = mChildNodes[index] as MethodInvokeParameterControl;
            if(node != null)
            {
                if(node.ParamPin.HasLink)
                {
                    var pinInfo = node.ParamPin;
                    pinInfo.Clear();
                }

                param.OnParamTypeChanged -= node.UpdateParamType;
                RemoveChildNodeByIndex(index);
            }
        }

        public CustomMethodInfo GetMethodInfo()
        {
            var pm = CSParam as MethodCustomConstructParam;
            return pm.MethodInfo;
        }

        partial void ResetMethodInfo_WPF(CodeDomNode.CustomMethodInfo methodInfo);
        public async Task ResetMethodInfo(CodeDomNode.CustomMethodInfo methodInfo)
        {
            // 加载完成后处理
            var pp = mCSParam as MethodCustomConstructParam;
            pp.MethodInfo.RemoveInParamOperation(this);

            // In
            // 比较新旧MethodInfo参数，对参数进行增删
            for(int i=0; i<methodInfo.InParams.Count; i++)
            {
                if(i >= pp.MethodInfo.InParams.Count)
                {
                    await OnAddedInParam(methodInfo.InParams[i]);
                }
                else
                {
                    if (pp.MethodInfo.InParams[i].IsEqual(methodInfo.InParams[i]))
                    {
                        var pm = mChildNodes[i] as MethodInvokeParameterControl;
                        pm.ResetParamInfo(methodInfo.InParams[i], FieldDirection.In);
                    }
                    else
                    {
                        await OnRemovedInParam(i, pp.MethodInfo.InParams[i]);
                        await OnInsertInParam(i, methodInfo.InParams[i]);
                    }
                }
            }
            // 删除多余的参数
            for(int i=methodInfo.InParams.Count; i<pp.MethodInfo.InParams.Count; i++)
            {
                await OnRemovedInParam(i, pp.MethodInfo.InParams[i]);
            }

            pp.MethodInfo = methodInfo;
            ResetMethodInfo_WPF(methodInfo);

            methodInfo.AddInParamOperation(this);
        }

        public static void InitNodePinTypes(CodeGenerateSystem.Base.ConstructionParams smParam)
        {
            CollectLinkPinInfo(smParam, "CtrlMethodPin_Next", CodeGenerateSystem.Base.enLinkType.Method, CodeGenerateSystem.Base.enBezierType.Right, CodeGenerateSystem.Base.enLinkOpType.Start, false);
        }

        #region GenerateCode

        public override async System.Threading.Tasks.Task GCode_CodeDom_GenerateCode(CodeTypeDeclaration codeClass, CodeStatementCollection codeStatementCollection, LinkPinControl element, GenerateCodeContext_Method context)
        {
            // 通过代理生成的函数
            var csParam = CSParam as MethodCustomConstructParam;

            var methodCode = new CodeGenerateSystem.CodeDom.CodeMemberMethod();
            methodCode.Comments.Add(new CodeCommentStatement(this.ShowNodeName));
            methodCode.Attributes = MemberAttributes.Final | MemberAttributes.Private;
            methodCode.CustomAttributes.Add(new CodeAttributeDeclaration(
                                                                    new CodeTypeReference(typeof(EngineNS.Editor.MacrossMemberAttribute).FullName, 0),
                                                                    new CodeAttributeArgument(new CodeSnippetExpression("EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable"))));
            methodCode.Name = context.DelegateMethodName;
            methodCode.IsAsync = csParam.MethodInfo.IsAsync;
            List<string> genericParamNames = new List<string>();
            var catchparamName = "(";
            foreach(var paramNode in mChildNodes)
            {
                var paramExp = new CodeParameterDeclarationExpression();
                var pm = paramNode as MethodInvokeParameterControl;
                var pmParam = pm.CSParam as MethodInvokeParameterControl.MethodInvokeParameterConstructionParams;
                paramExp.Direction = pm.ParamFlag;
                paramExp.Name = pmParam.ParamInfo.ParamName;
                var paramType = pm.ParamType;
                paramExp.Type = new CodeTypeReference(paramType);
                if (paramType.IsGenericParameter && !genericParamNames.Contains(paramType.Name))
                {
                    genericParamNames.Add(paramType.Name);
                    methodCode.TypeParameters.Add(new CodeTypeParameter(paramType.Name));
                }

                methodCode.Parameters.Add(paramExp);
                catchparamName += paramExp.Type + " " + paramExp.Name + ",";
            }
            bool hasReturn = false;
            if(csParam.MethodInfo.IsAsync)
            {
                // 异步函数只能由return，不能有out参数
                if (csParam.MethodInfo.OutParams.Count == 1)
                {
                    var param = csParam.MethodInfo.OutParams[0];
                    var paramType = param.ParamType.GetActualType();
                    methodCode.ReturnType = new CodeTypeReference($"System.Threading.Tasks.Task<{EngineNS.Rtti.RttiHelper.GetAppTypeString(paramType)}>");
                    context.ReturnValueType = paramType;
                    hasReturn = true;
                }
            }
            else
            {
                foreach (var param in csParam.MethodInfo.OutParams)
                {
                    if (param.ParamName == "Return")
                    {
                        var paramType = param.ParamType.GetActualType();
                        methodCode.ReturnType = new CodeTypeReference(paramType);
                        if (paramType.IsGenericParameter && !genericParamNames.Contains(paramType.Name))
                        {
                            genericParamNames.Add(paramType.Name);
                            methodCode.TypeParameters.Add(new CodeTypeParameter(paramType.Name));
                        }
                        context.ReturnValueType = paramType;
                        hasReturn = true;
                    }
                    else
                    {
                        var paramExp = new CodeParameterDeclarationExpression();
                        paramExp.Direction = FieldDirection.Out;
                        paramExp.Name = param.ParamName;
                        var paramType = param.ParamType.GetActualType();
                        if (paramType.IsByRef)
                        {
                            var typefullname = paramType.FullName.Substring(0, paramType.FullName.Length - 1);
                            paramType = paramType.Assembly.GetType(typefullname);
                        }
                        paramExp.Type = new CodeTypeReference(paramType);
                        if (paramType.IsGenericParameter && !genericParamNames.Contains(paramType.Name))
                        {
                            genericParamNames.Add(paramType.Name);
                            methodCode.TypeParameters.Add(new CodeTypeParameter(paramType.Name));
                        }

                        methodCode.Parameters.Add(paramExp);
                        catchparamName += paramExp.Type + " " + paramExp.Name + ",";
                    }
                }
            }

            if (!hasReturn)
            {
                if (csParam.MethodInfo.IsAsync)
                    methodCode.ReturnType = new CodeTypeReference($"System.Threading.Tasks.Task");
                else
                    methodCode.ReturnType = new CodeTypeReference(typeof(void));
            }

            if(context.MethodGenerateData != null)
            {
                foreach (var localParam in context.MethodGenerateData.LocalParams)
                {
                    var defVal = CodeGenerateSystem.Program.GetDefaultValueFromType(localParam.ParamType);
                    var initExp = Program.GetValueCode(methodCode.Statements, localParam.ParamType, defVal);
                    methodCode.Statements.Add(new CodeVariableDeclarationStatement(localParam.ParamType, localParam.ParamName, initExp));
                }
            }

            catchparamName = catchparamName.TrimEnd(',');
            catchparamName += ")";

            var tryCatchExp = new CodeTryCatchFinallyStatement();
            tryCatchExp.TryStatements.Add(new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(new CodeVariableReferenceExpression(context.ClassContext.ScopFieldName), "Begin", new CodeExpression[0]));
            var exName = "ex_" + EngineNS.Editor.Assist.GetValuedGUIDString(Id);
            var cah = new CodeCatchClause(exName);
            cah.Statements.Add(new CodeExpressionStatement(
                                        new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(
                                            new CodeSnippetExpression("EngineNS.Profiler.Log"), "WriteException",
                                            new CodeVariableReferenceExpression(exName),
                                            new CodePrimitiveExpression("Macross异常"))));
            tryCatchExp.CatchClauses.Add(cah);
            tryCatchExp.FinallyStatements.Add(new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(new CodeVariableReferenceExpression(context.ClassContext.ScopFieldName), "End", new CodeExpression[0]));
            methodCode.Statements.Add(tryCatchExp);

            if(!csParam.MethodInfo.IsAsync)
            {
                foreach (var param in csParam.MethodInfo.OutParams)
                {
                    if (param.ParamName == "Return")
                        continue;

                    var paramType = param.ParamType.GetActualType();
                    if (paramType.IsGenericParameter)
                    {
                        methodCode.Statements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(param.ParamName),
                                                                          new CodeSnippetExpression($"default({paramType.Name})")));
                    }
                    else if (paramType.IsPrimitive || paramType == typeof(string))
                    {
                        methodCode.Statements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(param.ParamName),
                                                                          new CodePrimitiveExpression(CodeGenerateSystem.Program.GetDefaultValueFromType(param.ParamType.GetActualType()))));
                    }
                    else
                    {
                        if (paramType.IsByRef)
                        {
                            string name = paramType.FullName.Remove(paramType.FullName.Length - 1);
                            paramType = paramType.Assembly.GetType(name);
                        }
                        methodCode.Statements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(param.ParamName),
                                                                          new CodeObjectCreateExpression(paramType, new CodeExpression[0])));
                    }
                    //methodCode.Statements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(param.ParamName),
                    //new CodePrimitiveExpression(CodeGenerateSystem.Program.GetDefaultValueFromType(param.ParamType.GetActualType()))));
                }
            }
            codeClass.Members.Add(new CodeSnippetTypeMember("#pragma warning disable 1998"));
            codeClass.Members.Add(methodCode);
            codeClass.Members.Add(new CodeSnippetTypeMember("#pragma warning restore 1998"));

            #region Debug
            // 收集用于调试的数据的代码
            var debugCodes = CodeDomNode.BreakPoint.BeginMacrossDebugCodeStatments(tryCatchExp.TryStatements);
            foreach (var param in mChildNodes)
            {
                var pm = param as MethodInvokeParameterControl;
                CodeDomNode.BreakPoint.GetGatherDataValueCodeStatement(debugCodes, pm.ParamPin.GetLinkPinKeyName(), pm.GCode_CodeDom_GetValue(pm.ParamPin, context), pm.GCode_GetTypeString(pm.ParamPin, context), context);
            }
            // 断点
            var breakCondStatement = CodeDomNode.BreakPoint.BreakCodeStatement(codeClass, debugCodes, HostNodesContainer.GUID, Id);
            // 设置数据
            foreach (var param in mChildNodes)
            {
                var pm = param as MethodInvokeParameterControl;
                CodeDomNode.BreakPoint.GetSetDataValueCodeStatement(breakCondStatement.TrueStatements, pm.ParamPin.GetLinkPinKeyName(), pm.GCode_CodeDom_GetValue(pm.ParamPin, context), pm.GCode_GetType(pm.ParamPin, context));
            }
            CodeDomNode.BreakPoint.EndMacrossDebugCodeStatements(tryCatchExp.TryStatements, debugCodes);
            #endregion

            if(mCtrlMethodPin_Next.HasLink)
            {
                var methodContext = new CodeGenerateSystem.Base.GenerateCodeContext_Method(context.ClassContext, methodCode);
                await mCtrlMethodPin_Next.GetLinkedObject(0, false).GCode_CodeDom_GenerateCode(codeClass, tryCatchExp.TryStatements, mCtrlMethodPin_Next.GetLinkedPinControl(0, false), methodContext);
            }

            if(hasReturn)
            {
                Type retType = context.ReturnValueType;
                if (csParam.MethodInfo.IsAsync)
                {
                    retType = csParam.MethodInfo.OutParams[0].ParamType.GetActualType();
                }

                if (retType.IsGenericParameter)
                    methodCode.Statements.Add(new CodeMethodReturnStatement(new CodeSnippetExpression($"default({retType.Name})")));
                else if(retType.IsPrimitive || retType == typeof(string))
                    methodCode.Statements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(CodeGenerateSystem.Program.GetDefaultValueFromType(retType))));
                else
                {
                    methodCode.Statements.Add(new CodeMethodReturnStatement(new CodeObjectCreateExpression(retType, new CodeExpression[0])));
                }
            }
        }
        public override async System.Threading.Tasks.Task GCode_CodeDom_GenerateCode(CodeTypeDeclaration codeClass, LinkPinControl element, GenerateCodeContext_Class context)
        {
            await GCode_CodeDom_GenerateMethodCode(codeClass, element, context, null);
        }
        public async System.Threading.Tasks.Task GCode_CodeDom_GenerateMethodCode(CodeTypeDeclaration codeClass, LinkPinControl element, GenerateCodeContext_Class context, MethodGenerateData data)
        {
            // 正常的自定义函数
            var csParam = CSParam as MethodCustomConstructParam;

            var methodCode = new CodeGenerateSystem.CodeDom.CodeMemberMethod();
            var customAttributes = "EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable";
            if (csParam.MethodInfo.OverrideAble)
            {
                methodCode.Attributes = MemberAttributes.Public;
                customAttributes = customAttributes + "|" + "EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Overrideable";
            }
            else
                methodCode.Attributes = MemberAttributes.Final | MemberAttributes.Public;

            methodCode.Name = NodeName;
            methodCode.CustomAttributes.Add(new CodeAttributeDeclaration(
                                                                    new CodeTypeReference(typeof(EngineNS.Editor.MacrossMemberAttribute).FullName, 0),
                                                                    new CodeAttributeArgument(new CodeSnippetExpression(customAttributes))));
            methodCode.CustomAttributes.Add(new CodeAttributeDeclaration(
                                                                    new CodeTypeReference(typeof(EngineNS.Editor.Editor_Guid).FullName, 0),
                                                                    new CodeAttributeArgument(new CodePrimitiveExpression(this.HostNodesContainer.GUID.ToString()))));
            methodCode.IsAsync = csParam.MethodInfo.IsAsync;
            if (data != null)
            {
                foreach (var localParam in data.LocalParams)
                {
                    var defVal = CodeGenerateSystem.Program.GetDefaultValueFromType(localParam.ParamType);
                    var initExp = Program.GetValueCode(methodCode.Statements, localParam.ParamType, defVal);
                    methodCode.Statements.Add(new CodeVariableDeclarationStatement(localParam.ParamType, localParam.ParamName, initExp));
                }
            }

            var paramPreStr = "temp___";
            var catchParamName = "(";
            foreach(var paramNode in mChildNodes)
            {
                var paramExp = new System.CodeDom.CodeParameterDeclarationExpression();
                var pm = paramNode as MethodInvokeParameterControl;
                var pmParam = pm.CSParam as MethodInvokeParameterControl.MethodInvokeParameterConstructionParams;
                paramExp.Direction = pm.ParamFlag;

                if(pmParam.ParamInfo.ParameterDisplayType != null)
                {
                    paramExp.CustomAttributes.Add(new CodeAttributeDeclaration(new CodeTypeReference(
                                                                    typeof(EngineNS.Editor.Editor_MacrossMethodParamTypeAttribute)),
                                                                    new CodeAttributeArgument(new CodeTypeOfExpression(pmParam.ParamInfo.ParameterDisplayType))));
                    paramExp.Name = paramPreStr + pmParam.ParamInfo.ParamName;

                    methodCode.Statements.Add(new CodeVariableDeclarationStatement(pmParam.ParamInfo.ParameterDisplayType, pmParam.ParamInfo.ParamName, new CodeGenerateSystem.CodeDom.CodeCastExpression(pmParam.ParamInfo.ParameterDisplayType, new CodeVariableReferenceExpression(paramExp.Name))));
                }
                else
                    paramExp.Name = pmParam.ParamInfo.ParamName;


                bool needref = false;
                if (pm.ParamType.IsByRef)
                {
                    string name = pm.ParamType.FullName.Remove(pm.ParamType.FullName.Length - 1);
                    Type temp = pm.ParamType.Assembly.GetType(name);
                    if (temp != null && temp.IsClass == false)
                    {
                        paramExp.Direction = FieldDirection.Ref;
                        paramExp.Type = new CodeTypeReference(temp);
                        catchParamName += "ref " + name + " " + paramExp.Name + ",";
                        needref = true;
                    }
                }

                if (needref)
                {
                    
                }
                else
                {
                    paramExp.Type = new CodeTypeReference(pmParam.ParamInfo.ParameterType);

                    methodCode.Parameters.Add(paramExp);
                    catchParamName += paramExp.Type + " " + paramExp.Name + ",";
                }
            }
            bool hasReturn = false;
            var methodContext = new CodeGenerateSystem.Base.GenerateCodeContext_Method(context, methodCode);
            if(csParam.MethodInfo.IsAsync)
            {
                // 异步函数只能由return，不能有out参数
                if(csParam.MethodInfo.OutParams.Count == 1)
                {
                    var param = csParam.MethodInfo.OutParams[0];
                    var paramType = param.ParamType.GetActualType();
                    methodCode.ReturnType = new CodeTypeReference($"System.Threading.Tasks.Task<{EngineNS.Rtti.RttiHelper.GetAppTypeString(paramType)}>");
                    methodContext.ReturnValueType = paramType;
                    hasReturn = true;
                }
            }
            else
            {
                foreach (var param in csParam.MethodInfo.OutParams)
                {
                    if (param.ParamName == "Return")
                    {
                        var paramType = param.ParamType.GetActualType();
                        methodCode.ReturnType = new CodeTypeReference(paramType);

                        methodContext.ReturnValueType = paramType;
                        hasReturn = true;
                    }
                    else
                    {
                        var paramExp = new CodeParameterDeclarationExpression();
                        paramExp.Direction = FieldDirection.Out;
                        paramExp.Name = param.ParamName;
                        var type = param.ParamType.Type;
                        if (type.IsByRef)
                        {
                            var typefullname = type.FullName.Substring(0, type.FullName.Length - 1);
                            type = type.Assembly.GetType(typefullname);
                        }
                        paramExp.Type = new CodeTypeReference(type);

                        methodCode.Parameters.Add(paramExp);
                        catchParamName += paramExp.Type + " " + paramExp.Name + ",";
                    }
                }
            }

            if (!hasReturn)
            {
                // unsafe
                //methodCode.ReturnType = new CodeTypeReference($"unsafe void");
                if (csParam.MethodInfo.IsAsync)
                    methodCode.ReturnType = new CodeTypeReference($"System.Threading.Tasks.Task");
                else
                    methodCode.ReturnType = new CodeTypeReference(typeof(void));
            }

            catchParamName = catchParamName.TrimEnd(',');
            catchParamName += ")";

            var tryCatchExp = new CodeTryCatchFinallyStatement();
            tryCatchExp.TryStatements.Add(new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(new CodeVariableReferenceExpression(context.ScopFieldName), "Begin", new CodeExpression[0]));
            var exName = "ex_" + EngineNS.Editor.Assist.GetValuedGUIDString(Id);
            var cah = new CodeCatchClause(exName);
            cah.Statements.Add(new CodeExpressionStatement(
                                        new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(
                                            new CodeSnippetExpression("EngineNS.Profiler.Log"), "WriteException",
                                            new CodeVariableReferenceExpression(exName),
                                            new CodePrimitiveExpression("Macross异常"))));
            tryCatchExp.CatchClauses.Add(cah);
            tryCatchExp.FinallyStatements.Add(new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(new CodeVariableReferenceExpression(context.ScopFieldName), "End", new CodeExpression[0]));
            methodCode.Statements.Add(tryCatchExp);

            if(!csParam.MethodInfo.IsAsync)
            {
                foreach (var param in csParam.MethodInfo.OutParams)
                {
                    if (param.ParamName == "Return")
                        continue;

                    var paramType = param.ParamType.GetActualType();
                    if (paramType.IsGenericParameter)
                    {
                        methodCode.Statements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(param.ParamName),
                                                                          new CodeSnippetExpression($"default({paramType.Name})")));
                    }
                    else if (paramType.IsPrimitive || paramType == typeof(string))
                    {
                        methodCode.Statements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(param.ParamName),
                                                                          new CodePrimitiveExpression(CodeGenerateSystem.Program.GetDefaultValueFromType(param.ParamType.GetActualType()))));
                    }
                    else
                    {
                        methodCode.Statements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(param.ParamName),
                                                                          new CodeObjectCreateExpression(paramType, new CodeExpression[0])));
                    }
                }
            }
            codeClass.Members.Add(new CodeSnippetTypeMember("#pragma warning disable 1998"));
            codeClass.Members.Add(methodCode);
            codeClass.Members.Add(new CodeSnippetTypeMember("#pragma warning restore 1998"));

            #region Debug
            //var methodContext = new CodeGenerateSystem.Base.GenerateCodeContext_Method(context, methodCode);
            if (!hasReturn)
            {
                if (csParam.MethodInfo.IsAsync)
                    methodContext.ReturnValueType = typeof(System.Threading.Tasks.Task);
            }
           
            // 收集用于调试的数据的代码
            var debugCodes = CodeDomNode.BreakPoint.BeginMacrossDebugCodeStatments(tryCatchExp.TryStatements);
            foreach(var param in mChildNodes)
            {
                var pm = param as MethodInvokeParameterControl;
                CodeDomNode.BreakPoint.GetGatherDataValueCodeStatement(debugCodes, pm.ParamPin.GetLinkPinKeyName(), pm.GCode_CodeDom_GetValue(pm.ParamPin, methodContext), pm.GCode_GetTypeString(pm.ParamPin, methodContext), methodContext);
            }
            // 断点
            var breakCondStatement = CodeDomNode.BreakPoint.BreakCodeStatement(codeClass, debugCodes, HostNodesContainer.GUID, Id);
            // 设置数据
            foreach(var param in mChildNodes)
            {
                var pm = param as MethodInvokeParameterControl;

                CodeDomNode.BreakPoint.GetSetDataValueCodeStatement(breakCondStatement.TrueStatements, pm.ParamPin.GetLinkPinKeyName(), pm.GCode_CodeDom_GetValue(pm.ParamPin, methodContext), pm.GCode_GetType(pm.ParamPin, methodContext));
            }
            CodeDomNode.BreakPoint.EndMacrossDebugCodeStatements(tryCatchExp.TryStatements, debugCodes);
            #endregion

            if (mCtrlMethodPin_Next.HasLink)
            {
                if (hasReturn)
                {
                    if(csParam.MethodInfo.IsAsync)
                    {
                        var retType = csParam.MethodInfo.OutParams[0].ParamType.GetActualType();
                        methodContext.ReturnValueType = typeof(System.Threading.Tasks.Task<>).MakeGenericType(retType);
                    }
                }
                else
                {
                    if (csParam.MethodInfo.IsAsync)
                        methodContext.ReturnValueType = typeof(System.Threading.Tasks.Task);
                    else
                        methodContext.ReturnValueType = typeof(void);
                }
                await mCtrlMethodPin_Next.GetLinkedObject(0, false).GCode_CodeDom_GenerateCode(codeClass, tryCatchExp.TryStatements, mCtrlMethodPin_Next.GetLinkedPinControl(0, false), methodContext);
            }

            if (hasReturn)
            {
                Type retType = methodContext.ReturnValueType;
                if (csParam.MethodInfo.IsAsync)
                {
                    retType = csParam.MethodInfo.OutParams[0].ParamType.GetActualType();
                }

                if (retType.IsGenericParameter)
                    methodCode.Statements.Add(new CodeMethodReturnStatement(new CodeSnippetExpression($"default({retType.Name})")));
                else if (retType.IsPrimitive || retType == typeof(string))
                    methodCode.Statements.Add(new CodeMethodReturnStatement(new CodePrimitiveExpression(CodeGenerateSystem.Program.GetDefaultValueFromType(retType))));
                else
                {
                    methodCode.Statements.Add(new CodeMethodReturnStatement(new CodeObjectCreateExpression(retType, new CodeExpression[0])));
                }
            }
        }

        public override bool Pin_UseOrigionParamName(LinkPinControl linkElement)
        {
            return true;
        }

        #endregion
    }
}
