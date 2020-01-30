using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using CodeGenerateSystem.Base;
using EngineNS.IO;
using EngineNS.IO.Serializer;
using EngineNS.Rtti;

namespace CodeDomNode
{
    [EngineNS.Rtti.MetaClass]
    public class MethodParamInfoAssist : EngineNS.IO.Serializer.Serializer, INotifyPropertyChanged
    {
        #region INotifyPropertyChangedMembers
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
        }
        #endregion

        [EngineNS.Rtti.MetaData]
        public System.CodeDom.FieldDirection FieldDirection
        {
            get;
            set;
        } = FieldDirection.In;
        [EngineNS.Rtti.MetaData]
        public int TypeLinkIndex { get; set; } = -1;
        [EngineNS.Rtti.MetaData]
        public bool IsParamsArray { get; set; } = false;
        [EngineNS.Rtti.MetaData]
        public Type ParameterType { get; set; } = null;
        [EngineNS.Rtti.MetaData]
        public Type ParameterDisplayType { get; set; } = null;
        string mParamName;
        [EngineNS.Rtti.MetaData]
        public string ParamName
        {
            get => mParamName;
            set
            {
                mParamName = value;
                OnPropertyChanged("ParamName");
            }
        }

        string mDisplayParamName;
        [EngineNS.Rtti.MetaData]
        public string UIDisplayParamName
        {
            get
            {
               return string.IsNullOrEmpty(mDisplayParamName) ? ParamName : mDisplayParamName;
            }
            set
            {
                mDisplayParamName = value;
            }
        }

        public override bool Equals(object obj)
        {
            var info = obj as MethodParamInfoAssist;
            if (info == null)
                return false;
            if ((FieldDirection == info.FieldDirection) &&
               (IsParamsArray == info.IsParamsArray) &&
               (ParameterType == info.ParameterType) &&
               (ParamName == info.ParamName))
                return true;
            return false;
        }
        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }
        public override string ToString()
        {
            return (FieldDirection.ToString() +
                    IsParamsArray.ToString() +
                    ParameterType.FullName +
                    ParamName);
        }
    }

    [EngineNS.Rtti.MetaClass]
    public class MethodInfoAssist : EngineNS.IO.Serializer.Serializer
    {
        [EngineNS.Rtti.MetaData]
        public string Path { get; set; }
        Type mParentClassType;
        public Type ParentClassType
        {
            get
            {
                if (mParentClassType == null)
                    mParentClassType = EngineNS.Rtti.RttiHelper.GetTypeFromSaveString(mParentClassTypeSaveName);
                return mParentClassType;
            }
            set
            {
                mParentClassType = value;
                if(mParentClassType != null)
                    mParentClassTypeSaveName = EngineNS.Rtti.RttiHelper.GetTypeSaveString(mParentClassType);
            }
        }
        string mParentClassTypeSaveName;
        [EngineNS.Rtti.MetaData]
        public string ParentClassTypeSaveName
        {
            get => mParentClassTypeSaveName;
            set
            {
                mParentClassTypeSaveName = value;
                if(string.IsNullOrEmpty(mParentClassTypeSaveName))
                    mParentClassType = EngineNS.Rtti.RttiHelper.GetTypeFromSaveString(mParentClassTypeSaveName);
            }
        }

        [EngineNS.Rtti.MetaData]
        public string MethodName { get; set; }
        string mShowMethodName;
        [EngineNS.Rtti.MetaData]
        public string ShowMethodName
        {
            get
            {
                if (string.IsNullOrEmpty(mShowMethodName))
                    return MethodName;
                return mShowMethodName;
            }
            set
            {
                mShowMethodName = value;
            }
        }

        [EngineNS.Rtti.MetaData]
        public List<MethodParamInfoAssist> Params { get; set; } = new List<MethodParamInfoAssist>();
        [EngineNS.Rtti.MetaData]
        public Type ReturnDisplayType { get; set; } = null;
        [EngineNS.Rtti.MetaData]
        public Type ReturnType { get; set; }
        // 根据链接的函数参数下标所标示的函数参数来确定返回类型
        [EngineNS.Rtti.MetaData]
        public int ReturnTypeLinkIndex { get; set; } = -1;
        [EngineNS.Rtti.MetaData]
        public bool IsFamily { get; set; } = false;
        [EngineNS.Rtti.MetaData]
        public bool IsFamilyAndAssembly { get; set; } = false;
        [EngineNS.Rtti.MetaData]
        public bool IsFamilyOrAssembly { get; set; } = false;
        [EngineNS.Rtti.MetaData]
        public bool IsPublic { get; set; }
        [EngineNS.Rtti.MetaData]
        public bool IsFromMacross { get; set; } = false;
        [EngineNS.Rtti.MetaData]
        public Guid FuncId { get; set; } = Guid.Empty;
        public enum enHostType
        {
            This,       // this
            Static,     // 静态类
            //Instance,   // 单件类 多线程问题，单件不再处理
            Target,     // 需要目标
            Base,       // 父类
            Local,      // 局部空间（如函数内部等）
        }
        [EngineNS.Rtti.MetaData]
        public enHostType HostType { get; set; } = enHostType.This;

        [EngineNS.Rtti.MetaData]
        public bool MC_Callable { get; set; } = false;
        [EngineNS.Rtti.MetaData]
        public bool MC_Overrideable { get; set; } = false;

        public bool IsEvent()
        {
            if (ReturnType != typeof(void))
                return false;

            foreach(var param in Params)
            {
                if (param.FieldDirection != FieldDirection.In)
                    return false;
            }
            return true;
        }
        public override string ToString()
        {
            var str = Path + ParentClassType?.FullName + MethodName;
            for (int i = 0; i < Params.Count; i++)
                str += Params[i].ToString();
            if(ReturnType != null)
                str += ReturnType.FullName;
            return str;
        }
        public override bool Equals(object obj)
        {
            var assist = obj as MethodInfoAssist;
            if (assist == null)
                return false;
            if ((Path != assist.Path) ||
               (ParentClassType != assist.ParentClassType) ||
               (MethodName != assist.MethodName))
                return false;
            if (Params.Count != assist.Params.Count)
                return false;
            if (ReturnType != assist.ReturnType)
                return false;
            for(int i=0; i<Params.Count; i++)
            {
                if (Params[i] != assist.Params[i])
                    return false;
            }
            return true;
        }
        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }
    }

    [CodeGenerateSystem.CustomConstructionParamsAttribute(typeof(MethodNodeConstructionParams))]
    public partial class MethodInvokeNode : CodeGenerateSystem.Base.BaseNodeControl, IRNameContainer
    {
        #region IRNameContainer
        public void CollectRefRNames(List<EngineNS.RName> rNames)
        {
            var tcType = mTemplateClassInstance.GetType();
            foreach (var cNode in mChildNodes)
            {
                var cCtrl = cNode as CodeDomNode.MethodInvokeParameterControl;
                if (cCtrl == null)
                    continue;

                if (cCtrl.ParamType == typeof(EngineNS.RName))
                {
                    var param = cCtrl.CSParam as MethodInvokeParameterControl.MethodInvokeParameterConstructionParams;
                    var property = tcType.GetProperty(param.ParamInfo.ParamName);
                    var value = property.GetValue(mTemplateClassInstance) as EngineNS.RName;
                    if (!rNames.Contains(value))
                        rNames.Add(value);
                }
                else if (cCtrl.ParamType.GetInterface(typeof(EngineNS.Macross.IMacrossType).FullName) != null)
                {

                    EngineNS.RName ValueRName = EngineNS.RName.GetRName(cCtrl.ParamType.FullName + ".macross");//EngineNS.Macross.MacrossFactory.Instance.GetMacrossRName(cCtrl.ParamType); 
                    if (ValueRName != EngineNS.RName.EmptyName)
                    {
                        if (!rNames.Contains(ValueRName))
                            rNames.Add(ValueRName);
                    }

                }
        
            }
        }
        #endregion

        [MetaClass]
        public class MethodNodeConstructionParams : CodeGenerateSystem.Base.ConstructionParams
        {
            [MetaData]
            public MethodInfoAssist MethodInfo { get; set; }

            public MethodNodeConstructionParams()
            {
            }
            public override EditorCommon.CodeGenerateSystem.INodeConstructionParams Duplicate()
            {
                var retVal = base.Duplicate() as MethodNodeConstructionParams;
                retVal.MethodInfo = MethodInfo;
                return retVal;
            }
            public override bool Equals(object obj)
            {
                if (!base.Equals(obj))
                    return false;
                var param = obj as MethodNodeConstructionParams;
                if (param == null)
                    return false;
                if (MethodInfo == param.MethodInfo)
                    return true;
                return false;
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
                                MethodInfo = new MethodInfoAssist();
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

        // 该参数会存盘，不要改变顺序及数值
        public enum enParamConstructType
        {
            MethodInvoke,
            MethodOverride,
            Return,
            MethodCustom,
            Delegate,
			ReturnCustom,
            MethodCustomInvoke_In,
            MethodCustomInvoke_Out,
            MethodInvoke_Out,
        }

        CodeGenerateSystem.Base.LinkPinControl mCtrlMethodLink_Target = new CodeGenerateSystem.Base.LinkPinControl();
        CodeGenerateSystem.Base.LinkPinControl mCtrlMethodLink_Pre = new CodeGenerateSystem.Base.LinkPinControl();
        CodeGenerateSystem.Base.LinkPinControl mCtrlMethodLink_Next = new CodeGenerateSystem.Base.LinkPinControl();
        CodeGenerateSystem.Base.LinkPinControl mCtrlreturnLink = new CodeGenerateSystem.Base.LinkPinControl();

        CodeGenerateSystem.Base.LinkPinControl mReturnLinkInfo;

        Type mReturnType;
        public Type MethodReturnType
        {
            get { return mReturnType; }
        }
        bool mAutoGenericIsNullCode = true;
        public bool AutoGenericIsNullCode
        {
            get { return mAutoGenericIsNullCode; }
            set
            {
                mAutoGenericIsNullCode = value;
                OnPropertyChanged("AutoGenericIsNullCode");
            }
        }
        public List<CodeGenerateSystem.Base.CustomPropertyInfo> StandardParamPropertyInfos
        {
            get;
            private set;
        } = new List<CodeGenerateSystem.Base.CustomPropertyInfo>();

        StackPanel mInputParamsPanel = null;
        StackPanel mOutputParamsPanel = null;
        // 临时类，用于选中后显示参数属性
        CodeGenerateSystem.Base.GeneratorClassBase mTemplateClassInstance = null;
        public CodeGenerateSystem.Base.GeneratorClassBase TemplateClassInstance
        {
            get { return mTemplateClassInstance; }
        }

        partial void InitConstruction();
        public MethodInvokeNode(CodeGenerateSystem.Base.ConstructionParams csParam)
            : base(csParam)
        {
            InitConstruction();
            IsOnlyReturnValue = true;
            try
            {
                var param = csParam as MethodNodeConstructionParams;
                //if (string.IsNullOrEmpty(csParam.ConstructParam))
                //    return;

                //var methodParamSplits = csParam.ConstructParam.Split(';');
                //if (methodParamSplits.Length < 2)
                //    return;

                //var splits = methodParamSplits[0].Split(',');
                //string name = splits[0];
                //if (HostNodesContainer != null)
                //{
                //    name = HostNodesContainer.GetNodeName(name);
                //}
                NodeName = param.MethodInfo.MethodName;
                ShowNodeName = param.MethodInfo.ShowMethodName;
                switch (param.MethodInfo.HostType)
                {
                    //case MethodInfoAssist.enHostType.Instance:
                    //    ClassInstanceName = param.MethodInfo.ParentClassType.FullName + ".Instance";
                    //    break;
                    case MethodInfoAssist.enHostType.Static:
                    case MethodInfoAssist.enHostType.This:
                    case MethodInfoAssist.enHostType.Target:
                    case MethodInfoAssist.enHostType.Base:
                        ClassInstanceName = param.MethodInfo?.ParentClassType?.FullName;
                        break;
                }
                //mMethodInfo = Program.GetMethodInfoFromParam(methodParamSplits[0]);
                //if(mMethodInfo == null)
                //{
                //    throw  new InvalidProgramException($"连线函数不存在：Name={NodeName}, Param={csParam.ConstructParam}");
                //    //System.Diagnostics.Debug.WriteLine($"连线函数不存在，可能会导致逻辑异常！：Name={NodeName}, Param={csParam.ConstructParam}");
                //}
                //if (mMethodInfo != null)
                //{
                //    SetParameters(mMethodInfo.GetParameters(), StandardParamPropertyInfos);
                //    SetReturn(mMethodInfo.ReturnType);
                //}
                var result = SetParameters(param.MethodInfo, StandardParamPropertyInfos);
                var retType = param.MethodInfo.ReturnType;
                if (param.MethodInfo.ReturnDisplayType != null)
                    retType = param.MethodInfo.ReturnDisplayType;
                SetReturn(retType);
                //else
                //{
                //    // 连线函数不存在
                //    HasError = true;
                //    ErrorDescription = $"连线函数不存在！";
                //    var mpSplits = methodParamSplits[0].Split(',');
                //    //EngineNS.ECSType csType = EngineNS.ECSType.All;
                //    //if (mpSplits.Length > 5)
                //    //    csType = (EngineNS.ECSType)EngineNS.Rtti.RttiHelper.EnumTryParse(typeof(EngineNS.ECSType), mpSplits[5]);
                //    var csType = csParam.CSType;
                //    if (!string.IsNullOrEmpty(mpSplits[3]))
                //    {
                //        var paramSplits = mpSplits[3].Split('/');
                //        SetParameters(paramSplits, csType);
                //    }
                //    if (!string.IsNullOrEmpty(mpSplits[4]))
                //    {
                //        var retType = EngineNS.Rtti.RttiHelper.GetTypeFromSaveString(mpSplits[4]);
                //        SetReturn(retType);
                //    }
                //}

                var link = AddLinkPinInfo("CtrlMethodLink_Target", mCtrlMethodLink_Target, null);
                link.OnAddLinkInfo += (LinkInfo linkInfo)=>
                {
                    base.AfterLink();
                    OnTargetLinkAddLinkInfo_WPF(linkInfo);
                };
                link.OnDelLinkInfo += (LinkInfo linkInfo) =>
                {
                    base.BreakLink();
                    OnTargetLinkDelLinkInfo_WPF(linkInfo);
                };
                AddLinkPinInfo("CtrlMethodLink_Pre", mCtrlMethodLink_Pre, null);
                AddLinkPinInfo("CtrlMethodLink_Next", mCtrlMethodLink_Next, null);
                if(result)
                    CreateTemplateClass(StandardParamPropertyInfos);
                BuildParamTypeLinkDic();
            }
            catch (System.Exception e)
            {
                HasError = true;
                ErrorDescription = $"连线函数节点异常！：Name={NodeName}, Param={csParam.ConstructParam}\r\n{e.ToString()}";
            }
        }
        partial void BuildParamTypeLinkDic();
        protected override Panel GetChildNodeContainer(BaseNodeControl childNode)
        {
            if(childNode is MethodInvokeParameterControl)
            {
                if (((MethodInvokeParameterControl)childNode).IsConstructOut)
                    return mOutputParamsPanel;
            }

            return mChildNodeContainer;
        }

        partial void OnTargetLinkAddLinkInfo_WPF(CodeGenerateSystem.Base.LinkInfo info);
        partial void OnTargetLinkDelLinkInfo_WPF(CodeGenerateSystem.Base.LinkInfo info);
        public static void InitNodePinTypes(CodeGenerateSystem.Base.ConstructionParams smParam)
        {
            var csParam = smParam as MethodNodeConstructionParams;
            CollectLinkPinInfo(smParam, "CtrlMethodLink_Target", csParam.MethodInfo.ParentClassType, CodeGenerateSystem.Base.enBezierType.Left, CodeGenerateSystem.Base.enLinkOpType.End, false);

            var retType = csParam.MethodInfo.ReturnType;
            if (csParam.MethodInfo.ReturnDisplayType != null)
                retType = csParam.MethodInfo.ReturnDisplayType;
            if (retType != null)
            {
                if (retType.BaseType == typeof(System.Threading.Tasks.Task))
                {
                    var genericType = retType.GetGenericArguments()[0];
                    CollectLinkPinInfo(smParam, "CtrlreturnLink", genericType, CodeGenerateSystem.Base.enBezierType.Right, CodeGenerateSystem.Base.enLinkOpType.Start, true);
                }
                else
                    CollectLinkPinInfo(smParam, "CtrlreturnLink", retType, CodeGenerateSystem.Base.enBezierType.Right, CodeGenerateSystem.Base.enLinkOpType.Start, true);
            }
            CollectLinkPinInfo(smParam, "CtrlMethodLink_Pre", CodeGenerateSystem.Base.enLinkType.Method, CodeGenerateSystem.Base.enBezierType.Left, CodeGenerateSystem.Base.enLinkOpType.End, true);
            CollectLinkPinInfo(smParam, "CtrlMethodLink_Next", CodeGenerateSystem.Base.enLinkType.Method, CodeGenerateSystem.Base.enBezierType.Right, CodeGenerateSystem.Base.enLinkOpType.Start, false);
        }

        public void CreateTemplateClass(List<CodeGenerateSystem.Base.CustomPropertyInfo> cpInfos)
        {
            var csParam = CSParam as MethodNodeConstructionParams;
            mTemplateClassInstance = CodeGenerateSystem.Base.PropertyClassGenerator.CreateClassInstanceFromCustomPropertys(cpInfos, this, $"MethodInvoke_{csParam.MethodInfo.ParentClassType.FullName}.{NodeName}", false);
            InitTemplateClass_WPF(cpInfos);
        }

        partial void InitTemplateClass_WPF(List<CodeGenerateSystem.Base.CustomPropertyInfo> propertys);

        // 设置函数参数
        protected bool SetParameters(MethodInfoAssist methodInfoAssist, List<CodeGenerateSystem.Base.CustomPropertyInfo> cpInfos)
        {
            if (methodInfoAssist == null)
                return false;

            var methodInfo = Program.GetMethodInfoFromMethodInfoAssist(methodInfoAssist);
            if (methodInfo == null)
                return false;

            mInputParamsPanel?.Children.Clear();
            mOutputParamsPanel?.Children.Clear();
            cpInfos.Clear();
            int paramIndex = 0;
            foreach (var paramInfo in methodInfoAssist.Params)
            {
                CodeGenerateSystem.Base.BaseNodeControl pc = null;
                if (paramInfo.ParameterType.IsSubclassOf(typeof(System.Delegate)))
                {
                    var csParam = new MethodInvoke_DelegateControl.MethodInvoke_DelegateControlConstructionParams()
                    {
                        CSType = mCSParam.CSType,
                        HostNodesContainer = mCSParam.HostNodesContainer,
                        ConstructParam = this.NodeName,
                        ConstructType = enParamConstructType.MethodInvoke,
                        ParamInfo = paramInfo,
                    };
                    pc = new MethodInvoke_DelegateControl(csParam);
                }
                else
                {
                    if (paramInfo.IsParamsArray)
                    {
                        var csParam = new ParamParameterControl.ParamParameterConstructionParams()
                        {
                            CSType = mCSParam.CSType,
                            HostNodesContainer = mCSParam.HostNodesContainer,
                            ConstructParam = "",
                            ConstructType = enParamConstructType.MethodInvoke,
                            ParamInfo = paramInfo,
                        };
                        var ppc = new ParamParameterControl(csParam);
                        if (Program.IsTypeValidInPropertyGridShow(paramInfo.ParameterType))
                        {
                            var infos = ppc.GetCPInfos();
                            cpInfos.AddRange(infos);
                        }
                        pc = ppc;
                    }
                    else
                    {
                        if (Program.IsTypeValidInPropertyGridShow(paramInfo.ParameterType))
                        {
                            var param = methodInfo.GetParameters()[paramIndex];
                            var cpInfo = Program.GetFromParamInfo(param);
                            cpInfos.Add(cpInfo);
                        }
                        var csParam = new MethodInvokeParameterControl.MethodInvokeParameterConstructionParams()
                        {
                            CSType = mCSParam.CSType,
                            HostNodesContainer = mCSParam.HostNodesContainer,
                            ConstructParam = "",
                            ConstructType = enParamConstructType.MethodInvoke,
                            ParamInfo = paramInfo,
                        };
                        pc = new MethodInvokeParameterControl(csParam);

                        switch(paramInfo.FieldDirection)
                        {
                            case FieldDirection.Out:
                            case FieldDirection.Ref:
                                {
                                    var param = new MethodInvokeParameterControl.MethodInvokeParameterConstructionParams()
                                    {
                                        CSType = mCSParam.CSType,
                                        HostNodesContainer = mCSParam.HostNodesContainer,
                                        ConstructParam = "",
                                        ConstructType = enParamConstructType.MethodInvoke_Out,
                                        ParamInfo = paramInfo,
                                    };
                                    var pCtrl = new MethodInvokeParameterControl(param);
                                    AddChildNodeNoChanageContainer(pCtrl, mOutputParamsPanel);
                                }
                                break;
                        }
                    }
                }

                //pc?.SetToolTip(EngineNS.Rtti.AttributeHelper.GetCustomAttributePropertyValue(paramInfo, typeof(DescriptionAttribute).FullName, "Description", false));
                if(pc != null)
                    AddChildNode(pc, mInputParamsPanel);

                paramIndex++;
            }

            return true;
        }
        // 根据返回值设置界面
        partial void SetReturn_WPF(Type returnType);
        protected void SetReturn(Type returnType)
        {
            mReturnType = returnType;
            SetReturn_WPF(returnType);
            if (returnType != typeof(void))
            {
                mReturnLinkInfo = AddLinkPinInfo("CtrlreturnLink", mCtrlreturnLink, null);
            }
        }

        string mReturnTypeText = "类型";
        public string ReturnTypeText
        {
            get { return mReturnTypeText; }
            set
            {
                mReturnTypeText = value;
                OnPropertyChanged("ReturnTypeText");
            }
        }

        public override void Save(XndNode xndNode, bool newGuid)
        {
            var att = xndNode.AddAttrib("defaultParam");
            att.Version = 2;
            att.BeginWrite();
            if (mTemplateClassInstance != null)
            {
                att.Write((Byte)1);
                CodeGenerateSystem.Base.PropertyClassGenerator.SaveClassInstanceProperties(mTemplateClassInstance, att);
            }
            else
                att.Write((Byte)0);
            att.Write(AutoGenericIsNullCode);
            att.EndWrite();
            base.Save(xndNode, newGuid);
        }
        public override async System.Threading.Tasks.Task Load(XndNode xndNode)
        {
            try
            {
                await base.Load(xndNode);
                var att = xndNode.FindAttrib("defaultParam");
                if(att != null)
                {
                    att.BeginRead();
                    switch(att.Version)
                    {
                        case 0:
                            {
                                att.ReadMetaObject(mTemplateClassInstance);
                                att.Read(out mAutoGenericIsNullCode);
                            }
                            break;
                        case 1:
                            {
                                Byte result;
                                att.Read(out result);
                                if (result == 1)
                                    att.ReadMetaObject(mTemplateClassInstance);
                                att.Read(out mAutoGenericIsNullCode);
                            }
                            break;
                        case 2:
                            {
                                Byte result;
                                att.Read(out result);
                                if (result == 1)
                                    CodeGenerateSystem.Base.PropertyClassGenerator.LoadClassInstanceProperties(mTemplateClassInstance, att);
                                att.Read(out mAutoGenericIsNullCode);
                            }
                            break;
                    }
                    att.EndRead();
                }
                BuildParamTypeLinkDic();
            }
            catch (System.Exception e)
            {
                HasError = true;
                ErrorDescription = "节点读取失败！";
                System.Diagnostics.Debug.WriteLine($"节点读取失败！：Name={NodeName}\r\n{e.ToString()}");
            }

            InitTemplateClass_WPF(null);
        }
        partial void SetErrorShowToolTip(string toolTip);

        #region UsefulMember

        //CodeGenerateSystem.Base.UsefulMemberHostData mHostUsefulMemberData = new CodeGenerateSystem.Base.UsefulMemberHostData();
        //public CodeGenerateSystem.Base.UsefulMemberHostData HostUsefulMemberData
        //{
        //    get { return mHostUsefulMemberData; }
        //    set { mHostUsefulMemberData = value; }
        //}
        //// 类实例名称
        string mClassInstanceName;
        public string ClassInstanceName
        {
            get { return mClassInstanceName; }
            set
            {
                mClassInstanceName = value;
                OnPropertyChanged("ClassInstanceName");
            }
        }
        //partial void InitializeLinkLine();
        #endregion

        #region 代码生成

        public override string GCode_GetValueName(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            if(element == mCtrlreturnLink)
            {
                return "methodReturnValue_" + EngineNS.Editor.Assist.GetValuedGUIDString(Id);
            }
            return string.Empty;
        }
        public override string GCode_GetTypeString(CodeGenerateSystem.Base.LinkPinControl element, GenerateCodeContext_Method context)
        {
            if (element == mCtrlreturnLink)
            {
                if(mReturnType.BaseType == typeof(System.Threading.Tasks.Task))
                {
                    var genericType = mReturnType.GetGenericArguments()[0];
                    return EngineNS.Rtti.RttiHelper.GetAppTypeString(genericType);
                }
                else
                    return EngineNS.Rtti.RttiHelper.GetAppTypeString(mReturnType);
            }

            return string.Empty;
        }

        public override Type GCode_GetType(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            if (element == mCtrlreturnLink)
            {
                if(mReturnType.BaseType == typeof(System.Threading.Tasks.Task))
                {
                    var genericType = mReturnType.GetGenericArguments()[0];
                    return genericType;
                }
                else
                    return mReturnType;
            }

            return null;
        }

        //public override void ReInitForGenericCode()
        //{
        //    mMethodInvokeStatment.Clear();
        //}

        System.CodeDom.CodeVariableDeclarationStatement mVariableDeclarationStatement = null;
        Dictionary<string, System.CodeDom.CodeVariableDeclarationStatement> mParamDeclarationStatementsDic = new Dictionary<string, CodeVariableDeclarationStatement>();
        Dictionary<string, System.CodeDom.CodeStatementCollection> mParamDeclarationInitStatementsDic = new Dictionary<string, CodeStatementCollection>();
        //Dictionary<System.CodeDom.CodeMemberMethod, System.CodeDom.CodeStatement> mMethodInvokeStatment = new Dictionary<CodeMemberMethod, CodeStatement>();
        public override async System.Threading.Tasks.Task GCode_CodeDom_GenerateCode(CodeTypeDeclaration codeClass, CodeStatementCollection codeStatementCollection, CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            if (this.HasError)
                return;

            //// 同一个函数节点只能生成一次
            //if (mMethodInvokeStatment.ContainsKey(context.Method))
            //    return;

            //if (mMethodInfo == null)
            //{
            //    base.GCode_CodeDom_GenerateCode(codeClass, codeStatementCollection, element, context);
            //    return;
            //}
            var consParam = CSParam as MethodNodeConstructionParams;

            //if (MemberParentNode != null && !MemberParentNode.IsOnlyReturnValue)
            //{
            //    var tempContext = context.Copy();
            //    tempContext.GenerateNext = false;
            //    MemberParentNode?.GCode_CodeDom_GenerateCode(codeClass, codeStatementCollection, mHostUsefulMemberData.LinkObject?.LinkElement, tempContext);
            //}
            if(mCtrlMethodLink_Target.HasLink)
            {
                var linkObj = mCtrlMethodLink_Target.GetLinkedObject(0, true);
                if(!linkObj.IsOnlyReturnValue)
                    await linkObj.GCode_CodeDom_GenerateCode(codeClass, codeStatementCollection, mCtrlMethodLink_Target.GetLinkedPinControl(0, true), context);
            }

            if (element == mCtrlMethodLink_Pre || element == mCtrlreturnLink)
            {
                //if (codeStatementCollection.Contains(mMethodInvokeStatment))
                //    return;

                // 参数名称数量
                //int tempParamNamesCount = 0;
                // 函数参数数量
                int tempMethodParamExpCount = 0;
                List<BaseNodeControl> inParams = new List<BaseNodeControl>(mChildNodes.Count);
                List<BaseNodeControl> outParams = new List<BaseNodeControl>(mChildNodes.Count);
                foreach (var paramNode in mChildNodes)
                {
                    if (paramNode is ParamParameterControl)
                    {
                        throw new InvalidOperationException("未实现");
                        //var paramCtrl = paramNode as ParamParameterControl;
                        //tempParamNamesCount += paramCtrl.ParamEllipses.Count;
                        //tempMethodParamExpCount++;
                    }
                    else if (paramNode is MethodInvoke_DelegateControl)
                    {
                        var paramCtrl = paramNode as MethodInvoke_DelegateControl;
                        paramCtrl.TempParamNames = new CodeExpression[paramCtrl.GetChildNodes().Count];
                        //tempParamNamesCount += paramCtrl.GetChildNodes().Count;
                        tempMethodParamExpCount++;
                        inParams.Add(paramCtrl);
                    }
                    else if (paramNode is MethodInvokeParameterControl)
                    {
                        var paramCtrl = paramNode as MethodInvokeParameterControl;
                        switch(((MethodInvokeParameterControl.MethodInvokeParameterConstructionParams)(paramCtrl.CSParam)).ConstructType)
                        {
                            case enParamConstructType.MethodInvoke:
                                {
                                    //tempParamNamesCount++;
                                    tempMethodParamExpCount++;
                                    inParams.Add(paramCtrl);
                                }
                                break;
                            case enParamConstructType.MethodInvoke_Out:
                                {
                                    outParams.Add(paramCtrl);
                                }
                                break;
                            default:
                                throw new InvalidOperationException("只有 MethodInvoke,MethodInvoke_Out这两种，不应该出现其他类型");
                        }
                    }
                    else
                        throw new InvalidOperationException("未实现");
                }

                //CodeExpression[] tempParamNames = new CodeExpression[tempParamNamesCount];
                bool exist = false;
                foreach (var statement in context.Method.Statements)
                {
                    if (statement is CodeVariableDeclarationStatement)
                    {
                        var state = statement as CodeVariableDeclarationStatement;
                        if (state != null && mVariableDeclarationStatement != null && state.Name == mVariableDeclarationStatement.Name)
                        {
                            exist = true;
                            break;
                        }
                    }
                }
                Type finalRetType = mReturnType;
                if (!exist && mReturnType != typeof(void) && mReturnType != typeof(System.Threading.Tasks.Task))
                {
                    var retName = GCode_GetValueName(mCtrlreturnLink, context);
                    if(mReturnType.BaseType == typeof(System.Threading.Tasks.Task))
                    {
                        if(mCtrlreturnLink.HasLink)
                        {
                            finalRetType = finalRetType.GetGenericArguments()[0];
                            if (finalRetType.IsGenericParameter)
                            {
                                mVariableDeclarationStatement = new CodeVariableDeclarationStatement(mCtrlreturnLink.ClassType, retName, CodeGenerateSystem.Program.GetDefaultValueExpressionFromType(finalRetType));
                                context.Method.Statements.Insert(0, mVariableDeclarationStatement);
                                if (mCtrlreturnLink.ClassType.Contains("*") && !context.ClassContext.PtrInvokeParamNames.Contains(retName))
                                    context.ClassContext.PtrInvokeParamNames.Add(retName);
                            }
                            else
                            {
                                mVariableDeclarationStatement = new CodeVariableDeclarationStatement(finalRetType, retName, CodeGenerateSystem.Program.GetDefaultValueExpressionFromType(finalRetType));
                                context.Method.Statements.Insert(0, mVariableDeclarationStatement);
                                if (finalRetType.IsPointer && !context.ClassContext.PtrInvokeParamNames.Contains(retName))
                                    context.ClassContext.PtrInvokeParamNames.Add(retName);
                            }
                        }
                    }
                    else if(mReturnType.IsGenericParameter)
                    {
                        mVariableDeclarationStatement = new CodeVariableDeclarationStatement(mCtrlreturnLink.ClassType, retName, CodeGenerateSystem.Program.GetDefaultValueExpressionFromType(finalRetType));
                        context.Method.Statements.Insert(0, mVariableDeclarationStatement);
                        if (mCtrlreturnLink.ClassType.Contains("*") && !context.ClassContext.PtrInvokeParamNames.Contains(retName))
                            context.ClassContext.PtrInvokeParamNames.Add(retName);
                    }
                    else
                    {
                        mVariableDeclarationStatement = new CodeVariableDeclarationStatement(finalRetType, retName, CodeGenerateSystem.Program.GetDefaultValueExpressionFromType(finalRetType));
                        context.Method.Statements.Insert(0, mVariableDeclarationStatement);
                        if (finalRetType.IsPointer && !context.ClassContext.PtrInvokeParamNames.Contains(retName))
                            context.ClassContext.PtrInvokeParamNames.Add(retName);
                    }
                }

                // 参数
                //foreach (var paramNode in inParams)
                foreach(var paramNode in mChildNodes)
                {
                    if (paramNode is MethodInvokeParameterControl)
                    {
                        var paramCtrl = paramNode as MethodInvokeParameterControl;
                        await paramCtrl.GCode_CodeDom_GenerateCode(codeClass, codeStatementCollection, null, context);

                        if(paramCtrl.IsUseLinkFromParamName())
                        {
                            paramCtrl.TempParamName = paramCtrl.GetLinkFromParamName(context);
                        }
                        else
                        {
                            var tempParamName = paramCtrl.GCode_GetValueName(null, context);//"param_" + EngineNS.Editor.Assist.GetValuedGUIDString(Id) + "_" + paramCtrl.ParamName;

                            System.CodeDom.CodeVariableDeclarationStatement varDec;
                            CodeStatementCollection initStatement = new CodeStatementCollection();
                            if (!mParamDeclarationStatementsDic.TryGetValue(tempParamName, out varDec))
                            {
                                if(paramCtrl.HasLink())
                                    varDec = new System.CodeDom.CodeVariableDeclarationStatement(paramCtrl.ParamType, tempParamName, CodeGenerateSystem.Program.GetDefaultValueExpressionFromType(paramCtrl.ParamType));
                                else
                                {
                                    var paramCtrlCSParam = paramCtrl.CSParam as MethodInvokeParameterControl.MethodInvokeParameterConstructionParams;
                                    var proInfo = TemplateClassInstance.GetType().GetProperty(paramCtrlCSParam.ParamInfo.ParamName);
                                    object proValue;
                                    if (proInfo == null)
                                        proValue = CodeGenerateSystem.Program.GetDefaultValueFromType(paramCtrl.ParamType);
                                    else
                                        proValue = proInfo.GetValue(TemplateClassInstance);
                                    var initExp = Program.GetValueCode(initStatement, paramCtrl.ParamType, proValue);
                                    varDec = new System.CodeDom.CodeVariableDeclarationStatement(paramCtrl.ParamType, tempParamName, initExp);
                                }
                                mParamDeclarationStatementsDic[tempParamName] = varDec;
                                mParamDeclarationInitStatementsDic[tempParamName] = initStatement;
                            }
                            else
                            {
                                mParamDeclarationInitStatementsDic.TryGetValue(tempParamName, out initStatement);
                            }
                            if (!context.Method.Statements.Contains(varDec))
                            {
                                context.Method.Statements.Insert(0, varDec);
                                for(int stIdx = initStatement.Count-1; stIdx >= 0; stIdx--)
                                {
                                    context.Method.Statements.Insert(0, initStatement[stIdx]);
                                }
                            }

                            paramCtrl.TempParamName = new System.CodeDom.CodeVariableReferenceExpression(tempParamName);
                            //if(paramCtrl.NeedCopyParam())
                            if(paramCtrl.HasLink())
                            {
                                var state = new System.CodeDom.CodeAssignStatement(paramCtrl.TempParamName, new CodeGenerateSystem.CodeDom.CodeCastExpression(paramCtrl.ParamType, paramCtrl.GCode_CodeDom_GetValue(null, context)));
                                codeStatementCollection.Add(state);
                            }
                        }
                    }
                    else if (paramNode is MethodInvoke_DelegateControl)
                    {
                        var paramCtrl = paramNode as MethodInvoke_DelegateControl;
                        await paramCtrl.GCode_CodeDom_GenerateCode(codeClass, codeStatementCollection, null, context);
                        var childNodes = paramCtrl.GetChildNodes();
                        for (int childIdx = 0; childIdx < childNodes.Count; childIdx++)
                        {
                            var childCtrl = childNodes[childIdx] as MethodInvokeParameterControl;
                            if (childCtrl.IsUseLinkFromParamName())
                                paramCtrl.TempParamNames[childIdx] = childCtrl.GetLinkFromParamName(context);
                            else
                            {
                                var tempParamName = childCtrl.GCode_GetValueName(null, context);
                                System.CodeDom.CodeVariableDeclarationStatement varDec;
                                CodeStatementCollection initStatement = new CodeStatementCollection();
                                if (!mParamDeclarationStatementsDic.TryGetValue(tempParamName, out varDec))
                                {
                                    if (childCtrl.HasLink())
                                        varDec = new System.CodeDom.CodeVariableDeclarationStatement(childCtrl.ParamType, tempParamName, CodeGenerateSystem.Program.GetDefaultValueExpressionFromType(childCtrl.ParamType));
                                    else
                                    {
                                        var paramCtrlCSParam = paramCtrl.CSParam as MethodInvokeParameterControl.MethodInvokeParameterConstructionParams;
                                        var proInfo = TemplateClassInstance.GetType().GetProperty(paramCtrlCSParam.ParamInfo.ParamName);
                                        object proValue;
                                        if (proInfo == null)
                                            proValue = CodeGenerateSystem.Program.GetDefaultValueFromType(paramCtrl.ParamType);
                                        else
                                            proValue = proInfo.GetValue(TemplateClassInstance);
                                        var initExp = Program.GetValueCode(initStatement, paramCtrl.ParamType, proValue);
                                        varDec = new System.CodeDom.CodeVariableDeclarationStatement(paramCtrl.ParamType, tempParamName, initExp);
                                    }
                                    mParamDeclarationStatementsDic[tempParamName] = varDec;
                                    mParamDeclarationInitStatementsDic[tempParamName] = initStatement;
                                }
                                else
                                {
                                    mParamDeclarationInitStatementsDic.TryGetValue(tempParamName, out initStatement);
                                }
                                if(!context.Method.Statements.Contains(varDec))
                                {
                                    context.Method.Statements.Insert(0, varDec);
                                    for (int stIdx = initStatement.Count - 1; stIdx >= 0; stIdx--)
                                    {
                                        context.Method.Statements.Insert(0, initStatement[stIdx]);
                                    }
                                }

                                paramCtrl.TempParamNames[childIdx] = new System.CodeDom.CodeVariableReferenceExpression(tempParamName);
                                if(childCtrl.HasLink())
                                {
                                    var state = new System.CodeDom.CodeAssignStatement(paramCtrl.TempParamNames[childIdx], new CodeGenerateSystem.CodeDom.CodeCastExpression(childCtrl.ParamType, childCtrl.GCode_CodeDom_GetValue(null, context)));
                                    codeStatementCollection.Add(state);
                                }
                            }
                        }
                    }
                    else if (paramNode is ParamParameterControl)
                    {
                        throw new InvalidOperationException("未实现");
                        //var paramCtrl = paramNode as ParamParameterControl;
                        //paramCtrl.GCode_CodeDom_GenerateCode(codeClass, codeStatementCollection, null, context);

                        //foreach (var data in paramCtrl.ParamEllipses)
                        //{
                        //    var dataParam = data.Value.CSParam as MethodInvokeParameterControl.MethodInvokeParameterConstructionParams;
                        //    var tempParamName = "param_" + EngineNS.Editor.Assist.GetValuedGUIDString(Id) + "_" + dataParam.ParamInfo.ParamName;
                        //    System.CodeDom.CodeVariableDeclarationStatement varDec;
                        //    if (!mParamDeclarationStatementsDic.TryGetValue(tempParamName, out varDec))
                        //    {
                        //        varDec = new System.CodeDom.CodeVariableDeclarationStatement(data.Value.ParamType, tempParamName,
                        //                                            CodeGenerateSystem.Program.GetDefaultValueExpressionFromType(data.Value.ParamType));
                        //        mParamDeclarationStatementsDic[tempParamName] = varDec;
                        //    }
                        //    if (!context.Method.Statements.Contains(varDec))
                        //    {
                        //        context.Method.Statements.Insert(0, varDec);
                        //    }
                        //    tempParamNames[i] = new System.CodeDom.CodeVariableReferenceExpression(tempParamName);
                        //    //var state = new System.CodeDom.CodeAssignStatement(tempParamNames[i],
                        //    //                                                   new CodeGenerateSystem.CodeDom.CodeCastExpression(data.Value.ParamType, data.Value.GCode_CodeDom_GetValue(null, context)));
                        //    //codeStatementCollection.Add(state);
                        //    i++;
                        //}
                    }
                }

                // 收集用于调试的数据的代码
                var debugCodes = CodeDomNode.BreakPoint.BeginMacrossDebugCodeStatments(codeStatementCollection);
                foreach (var paramNode in mChildNodes)
                {
                    if (paramNode is MethodInvokeParameterControl)
                    {
                        var paramCtrl = paramNode as MethodInvokeParameterControl;
                        string typeStr;

                        if (paramCtrl.IsUseLinkFromParamName())
                            typeStr = paramCtrl.ParamPin.GetLinkedObject(0).GCode_GetTypeString(paramCtrl.ParamPin.GetLinkedPinControl(0), context);
                        else
                            typeStr = paramCtrl.GCode_GetTypeString(paramCtrl.ParamPin, context);
                        var ctrlCSParam = paramCtrl.CSParam as MethodInvokeParameterControl.MethodInvokeParameterConstructionParams;
                        CodeDomNode.BreakPoint.GetGatherDataValueCodeStatement(debugCodes, paramCtrl.ParamPin.GetLinkPinKeyName(), paramCtrl.TempParamName, typeStr, context);
                    }
                    else if (paramNode is MethodInvoke_DelegateControl)
                    {
                        var paramCtrl = paramNode as MethodInvoke_DelegateControl;
                        var childNodes = paramCtrl.GetChildNodes();
                        for (int childIdx = 0; childIdx < childNodes.Count; childIdx++)
                        {
                            var childCtrl = childNodes[childIdx] as MethodInvokeParameterControl;
                            string typeStr;
                            if (childCtrl.ParamPin.HasLink)
                                typeStr = childCtrl.ParamPin.GetLinkedObject(0).GCode_GetTypeString(childCtrl.ParamPin.GetLinkedPinControl(0), context);
                            else
                                typeStr = childCtrl.GCode_GetTypeString(childCtrl.ParamPin, context);
                            var childCtrlCSParam = childCtrl.CSParam as MethodInvokeParameterControl.MethodInvokeParameterConstructionParams;
                            CodeDomNode.BreakPoint.GetGatherDataValueCodeStatement(debugCodes, childCtrl.ParamPin.GetLinkPinKeyName(), paramCtrl.TempParamNames[childIdx], typeStr, context);
                        }
                    }
                    else if(paramNode is ParamParameterControl)
                    {
                        throw new InvalidOperationException();
                    }
                }
                // 调试用代码
                var breakCondStatement = CodeDomNode.BreakPoint.BreakCodeStatement(codeClass, debugCodes, HostNodesContainer.GUID, Id);

                System.CodeDom.CodeExpression[] expColls = new System.CodeDom.CodeExpression[tempMethodParamExpCount];
                // 设置数据代码
                int expIdx = 0;
                foreach (var paramNode in inParams)
                {
                    if (paramNode is MethodInvokeParameterControl)
                    {
                        var paramCtrl = paramNode as MethodInvokeParameterControl;
                        var ctrlCSParam = paramCtrl.CSParam as MethodInvokeParameterControl.MethodInvokeParameterConstructionParams;
                        Type type;
                        if (paramCtrl.IsUseLinkFromParamName())
                            type = paramCtrl.ParamPin.GetLinkedObject(0).GCode_GetType(paramCtrl.ParamPin.GetLinkedPinControl(0), context);
                        else
                            type = paramCtrl.GCode_GetType(paramCtrl.ParamPin, context);
                        CodeDomNode.BreakPoint.GetSetDataValueCodeStatement(breakCondStatement.TrueStatements, paramCtrl.ParamPin.GetLinkPinKeyName(), paramCtrl.TempParamName, type);

                        bool useDisplayType = ctrlCSParam.ParamInfo.ParameterDisplayType != null && ctrlCSParam.ParamInfo.ParameterDisplayType != ctrlCSParam.ParamInfo.ParameterType;
                        if (useDisplayType)
                        {
                            paramCtrl.TempDisplayWithTypeParamName = "tempParam_" + EngineNS.Editor.Assist.GetValuedGUIDString(Guid.NewGuid());
                            codeStatementCollection.Add(new System.CodeDom.CodeVariableDeclarationStatement(ctrlCSParam.ParamInfo.ParameterType, paramCtrl.TempDisplayWithTypeParamName, new CodeGenerateSystem.CodeDom.CodeCastExpression(ctrlCSParam.ParamInfo.ParameterType, paramCtrl.TempParamName)));
                        }
                        if (!paramCtrl.IsConstructOut)
                        {
                            if (useDisplayType)
                            {
                                expColls[expIdx] = new System.CodeDom.CodeDirectionExpression(paramCtrl.ParamFlag, new CodeVariableReferenceExpression(paramCtrl.TempDisplayWithTypeParamName));
                            }
                            else
                            {
                                expColls[expIdx] = new System.CodeDom.CodeDirectionExpression(paramCtrl.ParamFlag, paramCtrl.TempParamName);
                            }
                            expIdx++;
                        }
                    }
                    else if (paramNode is MethodInvoke_DelegateControl)
                    {
                        expColls[expIdx] = paramNode.GCode_CodeDom_GetValue(null, context);
                        expIdx++;
                    }
                    else if (paramNode is ParamParameterControl)
                    {
                        throw new InvalidOperationException("未实现");
                        //var paramCtrl = paramNode as ParamParameterControl;
                        //foreach (var data in paramCtrl.ParamEllipses)
                        //{
                        //    CodeDomNode.BreakPoint.GetSetDataValueCodeStatement(breakCondStatement.TrueStatements, Id, data.Value.ParamName,
                        //                            tempParamNames[expIdx],
                        //                            data.Value.ParamType.FullName);

                        //    expColls[expIdx] = new System.CodeDom.CodeDirectionExpression(data.Value.ParamFlag, tempParamNames[expIdx]);

                        //    expIdx++;
                        //}
                    }
                    else
                    {
                        throw new InvalidOperationException("未实现");

                        //expIdx++;
                    }
                }
                CodeDomNode.BreakPoint.EndMacrossDebugCodeStatements(codeStatementCollection, debugCodes);

                //foreach(var paramNode in outParams)
                //{
                //    if(paramNode is MethodInvokeParameterControl)
                //    {
                //        var paramCtrl = paramNode as MethodInvokeParameterControl;
                //        var paramCtrlParam = paramCtrl.CSParam as MethodInvokeParameterControl.MethodInvokeParameterConstructionParams;
                //        if (paramCtrlParam.ParamInfo.ParameterDisplayType != null && paramCtrlParam.ParamInfo.ParameterDisplayType != paramCtrlParam.ParamInfo.ParameterType)
                //        {
                //            codeStatementCollection.Add(new System.CodeDom.CodeVariableDeclarationStatement(paramCtrlParam.ParamInfo.ParameterType, paramCtrl.TempDisplayWithTypeParamName, paramCtrl.TempParamName));
                //        }
                //    }
                //}

                // 函数调用
                System.CodeDom.CodeMethodReferenceExpression methodRef = new System.CodeDom.CodeMethodReferenceExpression();
                bool needAwait = false;
                if (mReturnType == typeof(System.Threading.Tasks.Task) || mReturnType.BaseType == typeof(System.Threading.Tasks.Task))
                    needAwait = context.IsReturnTypeIsTask();

                switch (consParam.MethodInfo.HostType)
                {
                    //case MethodInfoAssist.enHostType.Instance:
                    //    {
                    //        methodRef.TargetObject = new System.CodeDom.CodeSnippetExpression(consParam.MethodInfo.ParentClassType.FullName + ".Instance");
                    //    }
                    //    break;
                    case MethodInfoAssist.enHostType.Static:
                        {
                            methodRef.TargetObject = new System.CodeDom.CodeTypeReferenceExpression(consParam.MethodInfo.ParentClassType);
                        }
                        break;
                    case MethodInfoAssist.enHostType.This:
                        {
                            if(mCtrlMethodLink_Target.HasLink)
                            {
                                methodRef.TargetObject = mCtrlMethodLink_Target.GetLinkedObject(0, true).GCode_CodeDom_GetValue(mCtrlMethodLink_Target.GetLinkedPinControl(0, true), context);
                            }
                            else
                            {
                                methodRef.TargetObject = new CodeThisReferenceExpression();
                            }
                        }
                        break;
                    case MethodInfoAssist.enHostType.Base:
                        {
                            if (mCtrlMethodLink_Target.HasLink)
                            {
                                methodRef.TargetObject = mCtrlMethodLink_Target.GetLinkedObject(0, true).GCode_CodeDom_GetValue(mCtrlMethodLink_Target.GetLinkedPinControl(0, true), context);
                            }
                            else
                            {
                                methodRef.TargetObject = new CodeBaseReferenceExpression();
                            }
                        }
                        break;
                    case MethodInfoAssist.enHostType.Target:
                        {
                            if (mCtrlMethodLink_Target.HasLink)
                            {
                                methodRef.TargetObject = mCtrlMethodLink_Target.GetLinkedObject(0, true).GCode_CodeDom_GetValue(mCtrlMethodLink_Target.GetLinkedPinControl(0, true), context);
                            }
                            else
                                throw new InvalidOperationException("不应该出现这种情况,Target类型应该连线");
                        }
                        break;
                }
                // Code MethodInvoke Expression
                System.CodeDom.CodeStatement methodInvokeStatment;
                methodRef.MethodName = consParam.MethodInfo.MethodName;
                if (mReturnType == typeof(void) || mReturnType == typeof(System.Threading.Tasks.Task))
                {
                    var methodInvokeExp = new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(methodRef, needAwait, expColls);
                    methodInvokeStatment = new System.CodeDom.CodeExpressionStatement(methodInvokeExp);
                    codeStatementCollection.Add(methodInvokeStatment);
                }
                else
                {
                    if((mReturnType.BaseType == typeof(System.Threading.Tasks.Task)))
                    {
                        if(mCtrlreturnLink.HasLink)
                        {
                            if(needAwait)
                            {
                                var methodInvokeExp = new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(methodRef, needAwait, expColls);
                                methodInvokeStatment = new System.CodeDom.CodeAssignStatement(GCode_CodeDom_GetValue(mCtrlreturnLink, context), new CodeGenerateSystem.CodeDom.CodeCastExpression(finalRetType, methodInvokeExp));
                                codeStatementCollection.Add(methodInvokeStatment);
                            }
                            else
                            {
                                var tempTaskValueName = "task_" + EngineNS.Editor.Assist.GetValuedGUIDString(Guid.NewGuid());
                                var methodReturnType = consParam.MethodInfo.ReturnType;
                                codeStatementCollection.Add(new CodeVariableDeclarationStatement(methodReturnType, tempTaskValueName, new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(methodRef, needAwait, expColls)));
                                var conditionStatement = new CodeConditionStatement();
                                conditionStatement.Condition = new CodeBinaryOperatorExpression(new CodePropertyReferenceExpression(
                                                                                                        new CodeVariableReferenceExpression(tempTaskValueName), "IsCompleted"), 
                                                                                                        CodeBinaryOperatorType.ValueEquality, 
                                                                                                        new CodePrimitiveExpression(true));
                                conditionStatement.TrueStatements.Add(new System.CodeDom.CodeAssignStatement(GCode_CodeDom_GetValue(mCtrlreturnLink, context),
                                                                                                             new CodeGenerateSystem.CodeDom.CodeCastExpression(GCode_GetType(mCtrlreturnLink, context), new CodePropertyReferenceExpression(new CodeVariableReferenceExpression(tempTaskValueName), "Result"))));
                                conditionStatement.FalseStatements.Add(new CodeThrowExceptionStatement(new CodeObjectCreateExpression(typeof(System.InvalidProgramException), new CodePrimitiveExpression($"异步函数 {consParam.MethodInfo.MethodName} 操作未执行完成!"))));
                                codeStatementCollection.Add(conditionStatement);
                            }
                        }
                        else
                        {
                            var methodInvokeExp = new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(methodRef, needAwait, expColls);
                            methodInvokeStatment = new CodeVariableDeclarationStatement("var", GCode_GetValueName(mCtrlreturnLink, context), methodInvokeExp);
                            codeStatementCollection.Add(methodInvokeStatment);
                        }
                    }
                    else if(mReturnType.IsGenericParameter)
                    {
                        if(mCtrlreturnLink.HasLink)
                        {
                            var methodInvokeExp = new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(methodRef, needAwait, expColls);
                            methodInvokeStatment = new System.CodeDom.CodeAssignStatement(GCode_CodeDom_GetValue(mCtrlreturnLink, context), new CodeGenerateSystem.CodeDom.CodeCastExpression(finalRetType, methodInvokeExp));
                            codeStatementCollection.Add(methodInvokeStatment);
                        }
                        else
                        {
                            var methodInvokeExp = new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(methodRef, needAwait, expColls);
                            methodInvokeStatment = new System.CodeDom.CodeExpressionStatement(methodInvokeExp);
                            codeStatementCollection.Add(methodInvokeStatment);
                        }
                    }
                    else 
                    {
                        var methodInvokeExp = new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(methodRef, needAwait, expColls);
                        methodInvokeStatment = new System.CodeDom.CodeAssignStatement(GCode_CodeDom_GetValue(mCtrlreturnLink, context), new CodeGenerateSystem.CodeDom.CodeCastExpression(finalRetType, methodInvokeExp));
                        codeStatementCollection.Add(methodInvokeStatment);
                    }
                    // 返回值为空判断不需要，有些逻辑可能要自己判断
                    //if (mReturnType.IsClass && AutoGenericIsNullCode)
                    //{
                    //    var condExp = new System.CodeDom.CodeConditionStatement();
                    //    condExp.Condition = new System.CodeDom.CodeBinaryOperatorExpression(
                    //                                    GCode_CodeDom_GetValue(mCtrlreturnLink, context),
                    //                                    CodeBinaryOperatorType.ValueEquality,
                    //                                    new CodePrimitiveExpression(null));
                    //    if (context.ReturnValueType == typeof(void))
                    //        condExp.TrueStatements.Add(new CodeMethodReturnStatement());
                    //    else
                    //        condExp.TrueStatements.Add(new CodeMethodReturnStatement(CodeGenerateSystem.Program.GetDefaultValueExpressionFromType(context.ReturnValueType)));
                    //    codeStatementCollection.Add(condExp);
                    //}
                }
                //mMethodInvokeStatment.Add(context.Method, methodInvokeStatment);

                foreach(var paramNode in inParams)
                {
                    if(paramNode is MethodInvokeParameterControl)
                    {
                        var paramCtrl = paramNode as MethodInvokeParameterControl;
                        var paramCtrlParam = paramCtrl.CSParam as MethodInvokeParameterControl.MethodInvokeParameterConstructionParams;
                        if(paramCtrlParam.ParamInfo.FieldDirection == FieldDirection.Out)
                        {
                            if (paramCtrlParam.ParamInfo.ParameterDisplayType != null && paramCtrlParam.ParamInfo.ParameterDisplayType != paramCtrlParam.ParamInfo.ParameterType)
                            {
                                codeStatementCollection.Add(new System.CodeDom.CodeAssignStatement(paramCtrl.TempParamName, new CodeGenerateSystem.CodeDom.CodeCastExpression(paramCtrlParam.ParamInfo.ParameterDisplayType, new CodeVariableReferenceExpression(paramCtrl.TempDisplayWithTypeParamName))));
                            }
                        }
                    }
                }
                foreach (var paramNode in outParams)
                {
                    if (paramNode is MethodInvokeParameterControl)
                    {
                        var paramCtrl = paramNode as MethodInvokeParameterControl;
                        if (paramCtrl.NeedCopyParam())
                        {
                            var assignStatement = new System.CodeDom.CodeAssignStatement(
                                                                    paramCtrl.GCode_CodeDom_GetValue(null, context),
                                                                    paramCtrl.TempParamName);
                            codeStatementCollection.Add(assignStatement);
                        }
                    }

                    // param修饰符修饰的参数不能为ref或out所以这里不做处理
                }

                // 返回值获取
                if(mCtrlreturnLink.HasLink)
                {
                    debugCodes = CodeDomNode.BreakPoint.BeginMacrossDebugCodeStatments(codeStatementCollection);
                    CodeDomNode.BreakPoint.GetGatherDataValueCodeStatement(debugCodes, mCtrlreturnLink.GetLinkPinKeyName(),
                                                                            GCode_CodeDom_GetValue(mCtrlreturnLink, context),
                                                                            GCode_GetTypeString(mCtrlreturnLink, context), context);
                    CodeDomNode.BreakPoint.EndMacrossDebugCodeStatements(codeStatementCollection, debugCodes);
                }

                if (context.GenerateNext)
                {
                    if (mCtrlMethodLink_Next.HasLink)
                    {
                        await mCtrlMethodLink_Next.GetLinkedObject(0, false).GCode_CodeDom_GenerateCode(codeClass, codeStatementCollection, mCtrlMethodLink_Next.GetLinkedPinControl(0, false), context);
                    }
                }
            }
        }

        public override CodeExpression GCode_CodeDom_GetValue(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context, CodeGenerateSystem.Base.GenerateCodeContext_PreNode preNodeContext = null)
        {
            //if (mMethodInfo == null)
            //    return base.GCode_CodeDom_GetValue(element, context);

            if (element == mCtrlreturnLink)
            {
                return new System.CodeDom.CodeVariableReferenceExpression(GCode_GetValueName(mCtrlreturnLink, context));
            }

            return base.GCode_CodeDom_GetValue(element, context);
        }

        #endregion
    }

    ////////////////////////////////////////////////////////////////////////////////////////////
    [CodeGenerateSystem.CustomConstructionParams(typeof(ParamParameterConstructionParams))]
    public partial class ParamParameterControl : CodeGenerateSystem.Base.BaseNodeControl
    {
        [MetaClass]
        public class ParamParameterConstructionParams : CodeGenerateSystem.Base.ConstructionParams
        {
            [MetaData]
            public MethodInvokeNode.enParamConstructType ConstructType
            {
                get;
                set;
            } = MethodInvokeNode.enParamConstructType.MethodInvoke;
            [MetaData]
            public MethodParamInfoAssist ParamInfo { get; set; }
            public ParamParameterConstructionParams()
            {
            }

            public override EditorCommon.CodeGenerateSystem.INodeConstructionParams Duplicate()
            {
                var retVal = base.Duplicate() as ParamParameterConstructionParams;
                retVal.ConstructType = ConstructType;
                retVal.ParamInfo = ParamInfo;
                return retVal;
            }

            public override bool Equals(object obj)
            {
                if (!base.Equals(obj))
                    return false;
                var param = obj as ParamParameterConstructionParams;
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
            //public override void Write(EngineNS.IO.XndNode xndNode)
            //{
            //    var att = xndNode.AddAttrib("ConstructionParams");
            //    att.Version = 0;
            //    att.BeginWrite();
            //    att.Write(ConstructParam);
            //    byte consType = (byte)ConstructType;
            //    att.Write(consType);
            //    att.WriteMetaObject(ParamInfo);
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
                            byte consType;
                            att.Read(out consType);
                            ConstructType = (MethodInvokeNode.enParamConstructType)consType;
                            if (ParamInfo == null)
                                ParamInfo = new MethodParamInfoAssist();
                            att.ReadMetaObject(ParamInfo);
                            break;
                        case 1:
                            att.ReadMetaObject(this);
                            break;
                    }
                    att.EndRead();
                }
            }
        }
        //public CodeGenerateSystem.Base.GeneratorClassBase TemplateClassInstance
        //{
        //    get
        //    {
        //        var miNode = ParentNode as MethodInvokeNode;
        //        if (miNode == null)
        //            return null;
        //        return miNode.TemplateClassInstance;
        //    }
        //}
        Dictionary<CodeGenerateSystem.Base.LinkPinControl, MethodInvokeParameterControl> mParamEllipses = new Dictionary<CodeGenerateSystem.Base.LinkPinControl, MethodInvokeParameterControl>();
        public Dictionary<CodeGenerateSystem.Base.LinkPinControl, MethodInvokeParameterControl> ParamEllipses
        {
            get { return mParamEllipses; }
        }
        StackPanel mParamsPanel = null;

        Type mParamType;
        public Type ParamType
        {
            get { return mParamType; }
        }
        Type mParamArrayType;
        public Type ParamArrayType
        {
            get { return mParamArrayType; }
        }
        string mParamName;
        public string ParamName
        {
            get { return mParamName; }
        }
        partial void InitConstruction();
        public ParamParameterControl(CodeGenerateSystem.Base.ConstructionParams csParam)
            : base(csParam)
        {
            var param = csParam as ParamParameterConstructionParams;
            switch (param.ConstructType)
            {
                case MethodInvokeNode.enParamConstructType.MethodInvoke:
                case MethodInvokeNode.enParamConstructType.Return:
                case MethodInvokeNode.enParamConstructType.ReturnCustom:
                    break;
                case MethodInvokeNode.enParamConstructType.MethodOverride:
                case MethodInvokeNode.enParamConstructType.MethodCustom:
                case MethodInvokeNode.enParamConstructType.MethodCustomInvoke_Out:
                case MethodInvokeNode.enParamConstructType.MethodCustomInvoke_In:
                case MethodInvokeNode.enParamConstructType.MethodInvoke_Out:
                    IsOnlyReturnValue = true;
                    break;
                default:
                    throw new InvalidOperationException();
            }

            InitConstruction();

            NodeType = enNodeType.ChildNode;

            //var csType = EngineNS.ECSType.All;
            //if (HostNodesContainer != null)
            //    csType = HostNodesContainer.mCSType;

            mParamName = param.ParamInfo.ParamName;
            mParamArrayType = param.ParamInfo.ParameterType;
            if (mParamArrayType == null)
                return;
            var elementTypeName = mParamArrayType.FullName.Remove(mParamArrayType.FullName.IndexOf('['));
            mParamType = mParamArrayType.Assembly.GetType(elementTypeName);

            var paramInfoStr = GetParameterInfoString();
            AddParam(paramInfoStr, false);
        }

        public List<CodeGenerateSystem.Base.CustomPropertyInfo> GetCPInfos()
        {
            var retList = new List<CodeGenerateSystem.Base.CustomPropertyInfo>();
            int i = 1;
            foreach (var ellip in ParamEllipses)
            {
                var proInfo = new CodeGenerateSystem.Base.CustomPropertyInfo();
                proInfo.PropertyName = mParamName + i;
                proInfo.PropertyType = mParamType;
                proInfo.DefaultValue = CodeGenerateSystem.Program.GetDefaultValueFromType(proInfo.PropertyType);
                proInfo.CurrentValue = proInfo.DefaultValue;
                retList.Add(proInfo);
                i++;
            }
            return retList;
        }

        partial void SetDeleteButton(MethodInvokeParameterControl childCtrl, CodeGenerateSystem.Base.LinkPinControl paramEllip);
        partial void RecreateMethodInvokeTemplateClass();
        private string GetParameterInfoString()
        {
            return mParamName + (mParamEllipses.Count + 1) + ":" + EngineNS.Rtti.RttiHelper.GetTypeSaveString(mParamType) + ":";
        }
        private void AddParam(string paramInfo, bool canDeleted)
        {
            var csParam = new MethodInvokeParameterControl.MethodInvokeParameterConstructionParams()
            {
                CSType = mCSParam.CSType,
                HostNodesContainer = mCSParam.HostNodesContainer,
                ConstructParam = paramInfo,
                ConstructType = MethodInvokeNode.enParamConstructType.MethodInvoke,
            };
            var childCtrl = new MethodInvokeParameterControl(csParam);
            var paramEllip = childCtrl.ParamPin;
            mParamEllipses[paramEllip] = childCtrl;

            SetDeleteButton(childCtrl, paramEllip);
            AddChildNode(childCtrl, mParamsPanel);

            RecreateMethodInvokeTemplateClass();
        }
        public override async System.Threading.Tasks.Task Load(XndNode xndNode)
        {
            await base.Load(xndNode);

            int index = 0;
            foreach (var child in mChildNodes)
            {
                if (index != 0)
                {
                    var ctrl = child as MethodInvokeParameterControl;
                    if (ctrl == null)
                        continue;

                    var paramEllip = ctrl.ParamPin;
                    mParamEllipses[paramEllip] = ctrl;
                    SetDeleteButton(ctrl, paramEllip);
                }
                index++;
            }
        }
        public override async System.Threading.Tasks.Task GCode_CodeDom_GenerateCode(CodeTypeDeclaration codeClass, CodeStatementCollection codeStatementCollection, CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            foreach (var child in mChildNodes)
            {
                var ctrl = child as MethodInvokeParameterControl;
                if (ctrl != null)
                    await ctrl.GCode_CodeDom_GenerateCode(codeClass, codeStatementCollection, element, context);
            }
        }

        public override CodeExpression GCode_CodeDom_GetValue(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context, CodeGenerateSystem.Base.GenerateCodeContext_PreNode preNodeContext = null)
        {
            var inElli = element;
            if (inElli != null)
            {
                MethodInvokeParameterControl ctrl;
                if (mParamEllipses.TryGetValue(inElli, out ctrl))
                {
                    return ctrl.GCode_CodeDom_GetValue(element, context);
                }
            }
            return base.GCode_CodeDom_GetValue(element, context);
        }

        public override string GCode_GetTypeString(CodeGenerateSystem.Base.LinkPinControl element, GenerateCodeContext_Method context)
        {
            var inElli = element;
            if (inElli != null)
            {
                MethodInvokeParameterControl ctrl;
                if (mParamEllipses.TryGetValue(inElli, out ctrl))
                {
                    return ctrl.GCode_GetTypeString(element, context);
                }
            }
            return base.GCode_GetTypeString(element, context);
        }

        public override Type GCode_GetType(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            var inElli = element;
            if (inElli != null)
            {
                MethodInvokeParameterControl ctrl;
                if (mParamEllipses.TryGetValue(inElli, out ctrl))
                {
                    return ctrl.GCode_GetType(element, context);
                }
            }
            return base.GCode_GetType(element, context);
        }
        public override string GCode_GetValueName(LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            return ParamName;
        }
    }
    ////////////////////////////////////////////////////////////////////////////////////////////
    //[CodeGenerateSystem.CustomConstructionParams(typeof(DelegateParameterConstructionParams))]
    //public partial class DelegateParameterControl : CodeGenerateSystem.Base.BaseNodeControl
    //{
    //    [EngineNS.Rtti.MetaClass]
    //    public class DelegateParameterConstructionParams : CodeGenerateSystem.Base.ConstructionParams
    //    {
    //        [EngineNS.Rtti.MetaData]
    //        public MethodInvokeNode.enParamConstructType ConstructType
    //        {
    //            get;
    //            set;
    //        } = MethodInvokeNode.enParamConstructType.MethodInvoke;
    //        [EngineNS.Rtti.MetaData]
    //        public MethodParamInfoAssist ParamInfo
    //        {
    //            get;
    //            set;
    //        }
    //        public DelegateParameterConstructionParams()
    //        {
    //        }
    //        public override ConstructionParams Duplicate()
    //        {
    //            var retVal = base.Duplicate() as DelegateParameterConstructionParams;
    //            retVal.ConstructType = ConstructType;
    //            retVal.ParamInfo = ParamInfo;
    //            return retVal;
    //        }
    //        public override bool Equals(object obj)
    //        {
    //            if (!base.Equals(obj))
    //                return false;
    //            var param = obj as DelegateParameterConstructionParams;
    //            if (param == null)
    //                return false;
    //            if ((ConstructType == param.ConstructType) &&
    //                (ParamInfo == param.ParamInfo))
    //                return true;
    //            return false;
    //        }

    //        public override int GetHashCode()
    //        {
    //            return (base.GetHashCodeString() + ConstructType.ToString() + ParamInfo.ToString()).GetHashCode();
    //        }
    //        //public override void Write(XndNode xndNode)
    //        //{
    //        //    var att = xndNode.AddAttrib("ConstructionParams");
    //        //    att.Version = 0;
    //        //    att.BeginWrite();
    //        //    att.Write(ConstructParam);
    //        //    byte consType = (byte)ConstructType;
    //        //    att.Write(consType);
    //        //    att.WriteMetaObject(ParamInfo);
    //        //    att.EndWrite();
    //        //}
    //        public override void Read(XndNode xndNode)
    //        {
    //            var att = xndNode.FindAttrib("ConstructionParams");
    //            if (att != null)
    //            {
    //                att.BeginRead();
    //                switch (att.Version)
    //                {
    //                    case 0:
    //                        att.Read(out mConstructParam);
    //                        byte consType;
    //                        att.Read(out consType);
    //                        ConstructType = (MethodInvokeNode.enParamConstructType)consType;
    //                        if (ParamInfo == null)
    //                            ParamInfo = new MethodParamInfoAssist();
    //                        att.ReadMetaObject(ParamInfo);
    //                        break;
    //                    case 1:
    //                        att.ReadMetaObject(this);
    //                        break;
    //                }
    //                att.EndRead();
    //            }
    //        }
    //    }
    //    CodeGenerateSystem.Base.LinkControl mParamSquare = new CodeGenerateSystem.Base.LinkControl();
    //    public CodeGenerateSystem.Base.LinkControl ParamSquare
    //    {
    //        get { return mParamSquare; }
    //    }
    //    class ParamData
    //    {
    //        public MethodParamInfoAssist Param;
    //        public CodeGenerateSystem.Base.LinkControl Control;
    //        public CodeGenerateSystem.Base.LinkPinControl LinkObjectInfo;
    //    }
    //    ParamData[] mMethodParamDatas;


    //    string mParamFlag = "";
    //    public string MethodName
    //    {
    //        get { return "Method_" + ParamName + EngineNS.Editor.Assist.GetValuedGUIDString(this.Id); }
    //    }
    //    partial void InitMethodParam(System.Reflection.ParameterInfo meParam);
    //    partial void InitConstruction();
    //    public DelegateParameterControl(CodeGenerateSystem.Base.ConstructionParams csParam)
    //        : base(csParam)
    //    {
    //        var param = csParam as DelegateParameterConstructionParams;
    //        switch (param.ConstructType)
    //        {
    //            case MethodInvokeNode.enParamConstructType.MethodInvoke:
    //            case MethodInvokeNode.enParamConstructType.Return:
    //                break;
    //            case MethodInvokeNode.enParamConstructType.MethodOverride:
    //            case MethodInvokeNode.enParamConstructType.MethodCustom:
    //                IsOnlyReturnValue = true;
    //                break;
    //        }

    //        NodeName = param.ConstructParam;
    //        NodeType = enNodeType.ChildNode;
    //        InitConstruction();
    //        AddLinkPinInfo("ParamSquare", mParamSquare, null);

    //        // 代理函数参数
    //        var method = param.ParamInfo.ParameterType.GetMethod("Invoke");
    //        var methodParams = method.GetParameters();
    //        mMethodParamDatas = new ParamData[methodParams.Length];
    //        int i = 0;
    //        foreach (var meParam in methodParams)
    //        {
    //            var squre = new CodeGenerateSystem.Base.LinkControl();
    //            var paramAssist = CodeDomNode.Program.GetMethodParamInfoAssistFromParamInfo(meParam);
    //            InitMethodParam(paramAssist);
    //            var loInfo = AddLinkPinInfo($"param_{meParam.Name}", squre, null);
    //            var paramData = new ParamData()
    //            {
    //                Param = paramAssist,
    //                Control = squre,
    //                LinkObjectInfo = loInfo,
    //            };
    //            mMethodParamDatas[i] = paramData;
    //            i++;
    //        }
    //    }
    //    public static void InitNodePinTypes(CodeGenerateSystem.Base.ConstructionParams smParam)
    //    {
    //        CollectLinkPinInfo(smParam, "ParamSquare", CodeGenerateSystem.Base.enLinkType.Delegate, CodeGenerateSystem.Base.enBezierType.Right, CodeGenerateSystem.Base.enLinkOpType.Start, false);

    //        var param = smParam as DelegateParameterConstructionParams;
    //        var paramType = param.ParamInfo.ParameterType;
    //        var method = paramType.GetMethod("Invoke");
    //        var methodParams = method.GetParameters();
    //        foreach (var meParam in methodParams)
    //        {
    //            CollectLinkPinInfo(smParam, $"param_{meParam.Name}", CodeGenerateSystem.Base.LinkPinControl.GetLinkTypeFromTypeString(meParam.ParameterType.FullName), CodeGenerateSystem.Base.enBezierType.Right, CodeGenerateSystem.Base.enLinkOpType.Start, true);
    //        }
    //    }

    //    string mClassName = "";
    //    public override void GCode_CodeDom_GenerateCode(System.CodeDom.CodeTypeDeclaration codeClass, System.CodeDom.CodeStatementCollection codeStatementCollection, CodeGenerateSystem.Base.LinkControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
    //    {
    //        if (element == mParamSquare || element == null)
    //        {
    //            //CodeMemberMethod memberMethod = null;
    //            mClassName = codeClass.Name;
    //            var linkOI = GetLinkPinInfo(mParamSquare);
    //            if (linkOI.HasLink)
    //            {
    //                ////声明参数结构体  
    //                //var className = "Class_" + EngineNS.Editor.Assist.GetValuedGUIDString(this.Id);
    //                //var classCode = new System.CodeDom.CodeTypeDeclaration(className);
    //                //foreach (var node in HostNodesContainer.CtrlNodeList)
    //                //{
    //                //    if (node is MethodNode)
    //                //    {
    //                //        var methodNode = node as MethodNode;
    //                //        memberMethod = methodNode.MethodCode;
    //                //        if (memberMethod != null)
    //                //        {
    //                //            foreach (var pa in memberMethod.Parameters)
    //                //            {
    //                //                var param = pa as CodeParameterDeclarationExpression;
    //                //                if (param != null)
    //                //                {
    //                //                    var field = new CodeMemberField(param.Type, param.Name);
    //                //                    field.Attributes = MemberAttributes.Public;
    //                //                    classCode.Members.Add(field);
    //                //                }
    //                //            }
    //                //        }
    //                //    }
    //                //}
    //                //codeClass.Members.Add(classCode);

    //                ////初始化参数结构体
    //                //var fieldName = "Var_" + EngineNS.Editor.Assist.GetValuedGUIDString(this.Id);
    //                //var filedClass = new CodeMemberField(classCode.Name, fieldName);
    //                //filedClass.Attributes = MemberAttributes.Static;
    //                //filedClass.InitExpression = new CodeObjectCreateExpression(filedClass.Type);
    //                //codeClass.Members.Add(filedClass);

    //                //foreach (var pa in memberMethod.Parameters)
    //                //{
    //                //    var param = pa as CodeParameterDeclarationExpression;
    //                //    if (param != null)
    //                //    {
    //                //        codeStatementCollection.Add(new CodeAssignStatement(
    //                //            new CodeFieldReferenceExpression(new CodeVariableReferenceExpression(fieldName), param.Name)
    //                //            , new CodeSnippetExpression(param.Name)));
    //                //    }
    //                //}

    //                //声明回调函数
    //                var method = mParamType.GetMethod("Invoke");
    //                if (method == null)
    //                    return;

    //                System.CodeDom.CodeMemberMethod methodCode = new System.CodeDom.CodeMemberMethod();
    //                if ((context.Method.Attributes & MemberAttributes.Static) == MemberAttributes.Static)
    //                    methodCode.Attributes = System.CodeDom.MemberAttributes.Static;
    //                methodCode.Name = MethodName;
    //                methodCode.ReturnType = new CodeTypeReference(method.ReturnType);

    //                var tryCatchExp = new System.CodeDom.CodeTryCatchFinallyStatement();
    //                var cah = new System.CodeDom.CodeCatchClause("ex", new System.CodeDom.CodeTypeReference("System.Exception"));
    //                cah.Statements.Add(new System.CodeDom.CodeExpressionStatement(
    //                    new System.CodeDom.CodeMethodInvokeExpression(
    //                        new System.CodeDom.CodeSnippetExpression("Log.FileLog"),
    //                        "WriteLine",
    //                        new System.CodeDom.CodeExpression[]
    //                        {
    //                                new System.CodeDom.CodeSnippetExpression($"\"{context.ClassContext.EventInfo?.GetPerfCounterKeyName(context.ClassContext.ClassName)}中类{mClassName}.{methodCode.Name}调用出错! \" + ex.ToString()"),
    //                        })));
    //                tryCatchExp.CatchClauses.Add(cah);
    //                methodCode.Statements.Add(tryCatchExp);

    //                var performanceKeyName = "Perf_" + MethodName;
    //                var perfField = new System.CodeDom.CodeMemberField(typeof(EngineNS.Profiler.TimeScope).FullName, performanceKeyName);
    //                perfField.Attributes |= MemberAttributes.Static;
    //                perfField.InitExpression = new System.CodeDom.CodeMethodInvokeExpression(
    //                                                                                                    new System.CodeDom.CodeVariableReferenceExpression(typeof(EngineNS.Profiler.TimeScope).FullName),
    //                                                                                                    "GetPerfCounter",
    //                                                                                                    new System.CodeDom.CodeExpression[] { new System.CodeDom.CodePrimitiveExpression($"{context?.ClassContext?.EventInfo?.GetPerfCounterKeyName(context.ClassContext.ClassName)}.{MethodName}") });
    //                context.ClassContext.CodeClass.Members.Add(perfField);
    //                tryCatchExp.TryStatements.Add(new System.CodeDom.CodeMethodInvokeExpression(
    //                                                                                        new System.CodeDom.CodeVariableReferenceExpression(performanceKeyName),
    //                                                                                        "Begin", new System.CodeDom.CodeExpression[0]));

    //                // 代理函数自身参数
    //                foreach (var param in method.GetParameters())
    //                {
    //                    if (param.ParameterType.FullName == typeof(EngineNS.Editor.Runner.EventContext).FullName)
    //                        continue;
    //                    methodCode.Parameters.Add(new System.CodeDom.CodeParameterDeclarationExpression(param.ParameterType, param.Name));
    //                }

    //                // 父级函数参数
    //                foreach (var pa in context.Method.Parameters)
    //                {
    //                    var param = pa as CodeParameterDeclarationExpression;
    //                    if (param != null)
    //                    {
    //                        methodCode.Parameters.Add(new System.CodeDom.CodeParameterDeclarationExpression(param.Type, param.Name));
    //                    }
    //                }

    //                var newContext = new CodeGenerateSystem.Base.GenerateCodeContext_Method(context.ClassContext, methodCode)
    //                {
    //                    ReturnValueType = method.ReturnType
    //                };
    //                linkOI.GetLinkObject(0, false).GCode_CodeDom_GenerateCode(codeClass, tryCatchExp.TryStatements, linkOI.GetLinkElement(0, false), newContext);

    //                if (method.ReturnType != typeof(void))
    //                {
    //                    if (!(methodCode.Statements[methodCode.Statements.Count - 1] is System.CodeDom.CodeMethodReturnStatement))
    //                    {
    //                        var defaultValue = CodeGenerateSystem.Program.GetDefaultValueExpressionFromType(method.ReturnType);
    //                        var retStatement = new System.CodeDom.CodeMethodReturnStatement(defaultValue);
    //                        methodCode.Statements.Add(retStatement);
    //                    }
    //                }

    //                tryCatchExp.FinallyStatements.Add(new System.CodeDom.CodeMethodInvokeExpression(
    //                                                                                                new System.CodeDom.CodeVariableReferenceExpression(performanceKeyName),
    //                                                                                                "End", new System.CodeDom.CodeExpression[0]));

    //                codeClass.Members.Add(methodCode);
    //            }
    //        }
    //    }

    //    public override System.CodeDom.CodeExpression GCode_CodeDom_GetValue(CodeGenerateSystem.Base.LinkControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context, CodeGenerateSystem.Base.GenerateCodeContext_PreNode preNodeContext = null)
    //    {
    //        if (element == mParamSquare || element == null)
    //        {
    //            var linkOI = GetLinkPinInfo(mParamSquare);
    //            if (linkOI.HasLink)
    //            {
    //                var method = mParamType.GetMethod("Invoke");
    //                if (method == null)
    //                    return base.GCode_CodeDom_GetValue(element, context);

    //                // ((a, b)=>{ methodInvoke(paramX, paramY}; })
    //                string paramValueStr = "(";
    //                foreach (var param in method.GetParameters())
    //                {
    //                    paramValueStr += "__" + param.Name + ",";
    //                }
    //                paramValueStr = paramValueStr.TrimEnd(',');
    //                paramValueStr += ")";

    //                string methodParamStr = "";

    //                // 代理函数自身参数
    //                foreach (var param in method.GetParameters())
    //                {
    //                    if (param.ParameterType.FullName == typeof(EngineNS.Editor.Runner.EventContext).FullName)
    //                        continue;
    //                    methodParamStr += "__" + param.Name + ",";
    //                }

    //                // 父级函数参数
    //                foreach (var pa in context.Method.Parameters)
    //                {
    //                    var param = pa as CodeParameterDeclarationExpression;
    //                    if (param != null)
    //                    {
    //                        //if(param.Name == "context" && param.Type.BaseType == typeof(EngineNS.Editor.Runner.EventContext).ToString())
    //                        //    methodParamStr += "__" + param.Name + ",";
    //                        //else
    //                        methodParamStr += param.Name + ",";
    //                    }
    //                }

    //                methodParamStr = methodParamStr.TrimEnd(',');

    //                if (method.ReturnType != typeof(void))
    //                {
    //                    return new System.CodeDom.CodeSnippetExpression("(" + paramValueStr + "=>{ return " + MethodName + "(" + methodParamStr + "); })");
    //                }
    //                else
    //                    return new System.CodeDom.CodeSnippetExpression("(" + paramValueStr + "=>{ " + MethodName + "(" + methodParamStr + "); })");

    //            }
    //            else
    //            {
    //                return new System.CodeDom.CodePrimitiveExpression(null);
    //            }
    //        }
    //        else
    //        {
    //            if (mMethodParamDatas != null)
    //            {
    //                foreach (var data in mMethodParamDatas)
    //                {
    //                    if (data.Control == element)
    //                    {
    //                        return new System.CodeDom.CodeVariableReferenceExpression(data.ParamName);
    //                    }
    //                }
    //            }
    //        }

    //        return base.GCode_CodeDom_GetValue(element, context);
    //    }
    //    public override string GCode_GetValueType(CodeGenerateSystem.Base.LinkControl element, GenerateCodeContext_Method context)
    //    {
    //        if (element == mParamSquare || element == null)
    //        {
    //            return EngineNS.Rtti.RttiHelper.GetAppTypeString(mParamType);
    //        }
    //        else
    //        {
    //            foreach (var data in mMethodParamDatas)
    //            {
    //                if (data.Control == element)
    //                    return EngineNS.Rtti.RttiHelper.GetAppTypeString(data.ParamType);
    //            }
    //        }
    //        return base.GCode_GetValueType(element, context);
    //    }

    //    public override Type GCode_GetType(CodeGenerateSystem.Base.LinkControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
    //    {
    //        if (element == mParamSquare || element == null)
    //        {
    //            return mParamType;
    //        }
    //        else
    //        {
    //            foreach (var data in mMethodParamDatas)
    //            {
    //                if (data.Control == element)
    //                    return data.ParamType;
    //            }
    //        }
    //        return base.GCode_GetType(element, context);
    //    }

    //    public override string GCode_GetValueName(CodeGenerateSystem.Base.LinkControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
    //    {
    //        if (element == mParamSquare || element == null)
    //        {
    //            return mParamName;
    //        }
    //        else
    //        {
    //            foreach (var data in mMethodParamDatas)
    //            {
    //                if (data.Control == element)
    //                    return data.ParamName;
    //            }
    //        }
    //        return base.GCode_GetValueName(element, context);
    //    }

    //}
}
