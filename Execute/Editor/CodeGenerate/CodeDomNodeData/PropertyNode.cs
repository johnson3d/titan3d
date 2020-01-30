using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text;
using System.Windows;
using CodeGenerateSystem.Base;
using EngineNS.IO;
using Macross;

namespace CodeDomNode
{
    [EngineNS.Rtti.MetaClass]
    public class PropertyInfoAssist : EngineNS.IO.Serializer.Serializer, INotifyPropertyChanged
    {
        #region INotifyPropertyChangedMembers
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
        }
        #endregion

        [EngineNS.Rtti.MetaData]
        public string Path { get; set; }
        string mPropertyName;
        [EngineNS.Rtti.MetaData]
        public string PropertyName
        {
            get => mPropertyName;
            set
            {
                mPropertyName = value;
                OnPropertyChanged("PropertyName");
            }
        }
        Type mPropertyType;
        [EngineNS.Rtti.MetaData]
        public Type PropertyType
        {
            get => mPropertyType;
            set
            {
                mPropertyType = value;
                OnPropertyChanged("PropertyType");
            }
        }
        [EngineNS.Rtti.MetaData]
        public Type ParentClassType { get; set; }
        [EngineNS.Rtti.MetaData]
        public string MacrossClassType { get; set; }

        [EngineNS.Rtti.MetaData]
        public bool IsField { get; set; } = false;

        [EngineNS.Rtti.MetaData]
        public CodeDomNode.MethodInfoAssist.enHostType HostType { get; set; } = MethodInfoAssist.enHostType.This;

        public enum enDirection
        {
            Get,
            Set,
        };
        [EngineNS.Rtti.MetaData]
        public enDirection Direction = enDirection.Get;

        public List<Attribute> GetPropertyAttributes()
        {
            var retVal = new List<Attribute>();
            if (ParentClassType == null)
                return retVal;
            var proInfo = ParentClassType.GetProperty(PropertyName);
            if (proInfo == null)
                return retVal;
            retVal.AddRange(proInfo.GetCustomAttributes());
            return retVal;
        }

        public override string ToString()
        {
            return PropertyName + ParentClassType?.FullName;
        }
        public override bool Equals(object obj)
        {
            var assist = obj as PropertyInfoAssist;
            if (assist == null)
                return false;
            if ((PropertyName == assist.PropertyName) &&
               (ParentClassType == assist.ParentClassType))
                return true;
            return false;
        }
        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        //public void OnVariableNameChanged(string name)
        //{
        //    PropertyName = name;
        //}

        //public void OnVariableTypeChanged(CodeDomNode.VariableType vt)
        //{
        //    PropertyType = vt.Type;
        //}
        
    }

    [CodeGenerateSystem.CustomConstructionParams(typeof(PropertyNodeConstructionParams))]
    public partial class PropertyNode : CodeGenerateSystem.Base.BaseNodeControl
    {
        [EngineNS.Rtti.MetaClass]
        public class PropertyNodeConstructionParams : CodeGenerateSystem.Base.ConstructionParams
        {
            [EngineNS.Rtti.MetaData]
            public PropertyInfoAssist PropertyInfo { get; set; }
            public PropertyNodeConstructionParams()
            {
            }
            public override EditorCommon.CodeGenerateSystem.INodeConstructionParams Duplicate()
            {
                var retVal = base.Duplicate() as PropertyNodeConstructionParams;
                retVal.PropertyInfo = PropertyInfo;
                return retVal;
            }
            public override bool Equals(object obj)
            {
                if (!base.Equals(obj))
                    return false;
                var param = obj as PropertyNodeConstructionParams;
                if (param == null)
                    return false;
                if (PropertyInfo == param.PropertyInfo)
                    return true;
                return false;
            }
            public override int GetHashCode()
            {
                return (base.GetHashCodeString() + PropertyInfo.ToString()).GetHashCode();
            }
            //public override void Write(EngineNS.IO.XndNode xndNode)
            //{
            //    var att = xndNode.AddAttrib("ConstructionParams");
            //    att.Version = 0;
            //    att.BeginWrite();
            //    att.Write(ConstructParam);
            //    att.WriteMetaObject(PropertyInfo);
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
                            if (PropertyInfo == null)
                                PropertyInfo = new PropertyInfoAssist();
                            att.ReadMetaObject(PropertyInfo);
                            break;
                        case 1:
                            att.ReadMetaObject(this);
                            break;
                    }
                    att.EndRead();
                }
            }
        }

        CodeGenerateSystem.Base.LinkPinControl mCtrlMethodInHandle = new CodeGenerateSystem.Base.LinkPinControl();
        CodeGenerateSystem.Base.LinkPinControl mCtrlMethodOutHandle = new CodeGenerateSystem.Base.LinkPinControl();
        CodeGenerateSystem.Base.LinkPinControl mCtrlInHandle_Target = new CodeGenerateSystem.Base.LinkPinControl();
        CodeGenerateSystem.Base.LinkPinControl mCtrlValueInHandle = new CodeGenerateSystem.Base.LinkPinControl();
        CodeGenerateSystem.Base.LinkPinControl mCtrlValueOutHandle = new CodeGenerateSystem.Base.LinkPinControl();

        Visibility mValueOutHandleVisibility = Visibility.Visible;
        public Visibility ValueOutHandleVisibility
        {
            get { return mValueOutHandleVisibility; }
            set
            {
                mValueOutHandleVisibility = value;
                OnPropertyChanged("ValueOutHandleVisibility");
            }
        }
        Visibility mValueInHandleVisibility = Visibility.Visible;
        public Visibility ValueInHandleVisibility
        {
            get { return mValueInHandleVisibility; }
            set
            {
                mValueInHandleVisibility = value;
                OnPropertyChanged("ValueInHandleVisibility");
            }
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

        // 临时类，用于选中后显示参数属性
        CodeGenerateSystem.Base.GeneratorClassBase mTemplateClassInstance = null;
        public CodeGenerateSystem.Base.GeneratorClassBase TemplateClassInstance
        {
            get { return mTemplateClassInstance; }
        }

        string mDirectionStr;
        public string DirectionStr
        {
            get => mDirectionStr;
            set
            {
                mDirectionStr = value;
                OnPropertyChanged("DirectionStr");
            }
        }

        CodeGenerateSystem.Base.LinkPinControl mOutLinkInfo;
        partial void InitConstruction();
        public PropertyNode(CodeGenerateSystem.Base.ConstructionParams csParam)
            : base(csParam)
        {
            InitConstruction();

            try
            {
                var param = csParam as PropertyNodeConstructionParams;

                NodeName = param.PropertyInfo.PropertyName;
                DirectionStr = GetParamPreInfo(param.PropertyInfo.Direction);

                switch (param.PropertyInfo.HostType)
                {
                    //case MethodInfoAssist.enHostType.Instance:
                    //    ClassInstanceName = param.PropertyInfo.ParentClassType.FullName + ".Instance";
                    //    break;
                    case MethodInfoAssist.enHostType.Static:
                    case MethodInfoAssist.enHostType.Target:
                        ClassInstanceName = param.PropertyInfo.ParentClassType.FullName;
                        break;
                    case MethodInfoAssist.enHostType.This:
                    case MethodInfoAssist.enHostType.Base:
                    case MethodInfoAssist.enHostType.Local:
                        if (param.PropertyInfo.ParentClassType != null)
                            ClassInstanceName = param.PropertyInfo.ParentClassType.FullName;
                        else
                            ClassInstanceName = param.PropertyInfo.MacrossClassType;
                        break;
                }

                var targetLink = AddLinkPinInfo("CtrlInHandle_Target", mCtrlInHandle_Target);
                targetLink.OnAddLinkInfo += (CodeGenerateSystem.Base.LinkInfo linkInfo) =>
                {
                    base.AfterLink();
                    OnTargetLinkAddLinkInfo_WPF(linkInfo);
                };
                targetLink.OnDelLinkInfo += (CodeGenerateSystem.Base.LinkInfo linkInfo) =>
                {
                    base.BreakLink();
                    OnTargetLinkDelLinkInfo_WPF(linkInfo);
                };
                switch (param.PropertyInfo.Direction)
                {
                    case PropertyInfoAssist.enDirection.Set:
                        {
                            //ValueOutHandleVisibility = System.Windows.Visibility.Collapsed;
                            AddLinkPinInfo("CtrlMethodInHandle", mCtrlMethodInHandle);
                            AddLinkPinInfo("CtrlMethodOutHandle", mCtrlMethodOutHandle);
                            var valLink = AddLinkPinInfo("CtrlValueInHandle", mCtrlValueInHandle);
                            valLink.OnAddLinkInfo += (CodeGenerateSystem.Base.LinkInfo linkInfo) =>
                            {
                                base.AfterLink();
                                OnValueLinkAddLinkInfo_WPF(linkInfo);
                            };
                            valLink.OnDelLinkInfo += (CodeGenerateSystem.Base.LinkInfo linkInfo) =>
                            {
                                base.BreakLink();
                                OnValueLinkDelLinkInfo_WPF(linkInfo);
                            };
                            mOutLinkInfo = AddLinkPinInfo("CtrlValueOutHandle", mCtrlValueOutHandle);

                            var cpInfo = new CodeGenerateSystem.Base.CustomPropertyInfo();
                            cpInfo.PropertyName = param.PropertyInfo.PropertyName;
                            cpInfo.PropertyType = param.PropertyInfo.PropertyType;
                            cpInfo.DefaultValue = CodeGenerateSystem.Program.GetDefaultValueFromType(cpInfo.PropertyType);
                            cpInfo.CurrentValue = cpInfo.DefaultValue;
                            cpInfo.PropertyAttributes = param.PropertyInfo.GetPropertyAttributes();
                            var cpInfos = new List<CodeGenerateSystem.Base.CustomPropertyInfo>() { cpInfo };
                            var typeFullName = (param.PropertyInfo.ParentClassType != null) ? param.PropertyInfo.ParentClassType.FullName : param.PropertyInfo.MacrossClassType;
                            mTemplateClassInstance = CodeGenerateSystem.Base.PropertyClassGenerator.CreateClassInstanceFromCustomPropertys(cpInfos, this, 
                                                            $"Property_{typeFullName}.{param.PropertyInfo.PropertyName}_{Guid.NewGuid().ToString().Replace("-", "_")}", false);
                            InitTemplateClass_WPF(cpInfos);
                        }
                        break;
                    case PropertyInfoAssist.enDirection.Get:
                        {
                            ValueInHandleVisibility = System.Windows.Visibility.Collapsed;
                            mOutLinkInfo = AddLinkPinInfo("CtrlValueOutHandle", mCtrlValueOutHandle);
                        }
                        break;
                }

            }
            catch (System.Exception e)
            {
                var errorStr = $"连线属性节点异常！：Name={NodeName}, Param={csParam.ConstructParam}\r\nerror:{ErrorDescription}\r\n{e.ToString()}";
                System.Diagnostics.Trace.WriteLine(errorStr);
            }
        }
        partial void InitTemplateClass_WPF(List<CodeGenerateSystem.Base.CustomPropertyInfo> propertys);
        partial void OnTargetLinkAddLinkInfo_WPF(CodeGenerateSystem.Base.LinkInfo info);
        partial void OnTargetLinkDelLinkInfo_WPF(CodeGenerateSystem.Base.LinkInfo info);
        partial void OnValueLinkAddLinkInfo_WPF(CodeGenerateSystem.Base.LinkInfo info);
        partial void OnValueLinkDelLinkInfo_WPF(CodeGenerateSystem.Base.LinkInfo info);

        public static void InitNodePinTypes(CodeGenerateSystem.Base.ConstructionParams smParam)
        {
            var param = smParam as PropertyNodeConstructionParams;
            switch (param.PropertyInfo.Direction)
            {
                case PropertyInfoAssist.enDirection.Set:
                    {
                        CollectLinkPinInfo(smParam, "CtrlMethodInHandle", CodeGenerateSystem.Base.enLinkType.Method, CodeGenerateSystem.Base.enBezierType.Left, CodeGenerateSystem.Base.enLinkOpType.End, false);
                        CollectLinkPinInfo(smParam, "CtrlMethodOutHandle", CodeGenerateSystem.Base.enLinkType.Method, CodeGenerateSystem.Base.enBezierType.Right, CodeGenerateSystem.Base.enLinkOpType.Start, false);
                        CollectLinkPinInfo(smParam, "CtrlValueInHandle", param.PropertyInfo.PropertyType, CodeGenerateSystem.Base.enBezierType.Left, CodeGenerateSystem.Base.enLinkOpType.End, false);
                        CollectLinkPinInfo(smParam, "CtrlValueOutHandle", param.PropertyInfo.PropertyType, CodeGenerateSystem.Base.enBezierType.Right, CodeGenerateSystem.Base.enLinkOpType.Start, true);
                    }
                    break;
                case PropertyInfoAssist.enDirection.Get:
                    {
                        CollectLinkPinInfo(smParam, "CtrlValueOutHandle", param.PropertyInfo.PropertyType, CodeGenerateSystem.Base.enBezierType.Right, CodeGenerateSystem.Base.enLinkOpType.Start, true);
                    }
                    break;
            }
            if(param.PropertyInfo.ParentClassType != null)
                CollectLinkPinInfo(smParam, "CtrlInHandle_Target", param.PropertyInfo.ParentClassType, CodeGenerateSystem.Base.enBezierType.Left, CodeGenerateSystem.Base.enLinkOpType.End, false);
            else
                CollectClassLinkPinInfo(smParam, "CtrlInHandle_Target", param.PropertyInfo.MacrossClassType, CodeGenerateSystem.Base.enBezierType.Left, CodeGenerateSystem.Base.enLinkOpType.End, false);
        }

        partial void ChangeValueType_WPF(CodeDomNode.VariableType varType);
        public void ChangeValueType(CodeDomNode.VariableType varType)
        {
            mCtrlValueInHandle?.Clear();
            mCtrlValueOutHandle?.Clear();

            // Change Type
            var newType = varType.GetActualType();
            var param = CSParam as PropertyNodeConstructionParams;
            param.PropertyInfo.PropertyType = newType;
            switch(param.PropertyInfo.Direction)
            {
                case PropertyInfoAssist.enDirection.Set:
                    {
                        CollectLinkPinInfo(param, "CtrlValueInHandle", param.PropertyInfo.PropertyType, CodeGenerateSystem.Base.enBezierType.Left, CodeGenerateSystem.Base.enLinkOpType.End, false);
                        CollectLinkPinInfo(param, "CtrlValueOutHandle", param.PropertyInfo.PropertyType, CodeGenerateSystem.Base.enBezierType.Right, CodeGenerateSystem.Base.enLinkOpType.Start, true);

                        var pinDescIn = GetLinkPinDesc(param, "CtrlValueInHandle");
                        mCtrlValueInHandle.LinkType = pinDescIn.PinType;
                        mCtrlValueInHandle.ClassType = pinDescIn.ClassType;
                        var pinDescOut = GetLinkPinDesc(param, "CtrlValueOutHandle");
                        mCtrlValueOutHandle.LinkType = pinDescOut.PinType;
                        mCtrlValueOutHandle.ClassType = pinDescOut.ClassType;

                        var cpInfo = new CodeGenerateSystem.Base.CustomPropertyInfo();
                        cpInfo.PropertyName = param.PropertyInfo.PropertyName;
                        cpInfo.PropertyType = param.PropertyInfo.PropertyType;
                        cpInfo.DefaultValue = CodeGenerateSystem.Program.GetDefaultValueFromType(cpInfo.PropertyType);
                        cpInfo.CurrentValue = cpInfo.DefaultValue;
                        cpInfo.PropertyAttributes = param.PropertyInfo.GetPropertyAttributes();
                        var cpInfos = new List<CodeGenerateSystem.Base.CustomPropertyInfo>() { cpInfo };
                        var typeFullName = (param.PropertyInfo.ParentClassType != null) ? param.PropertyInfo.ParentClassType.FullName : param.PropertyInfo.MacrossClassType;
                        mTemplateClassInstance = CodeGenerateSystem.Base.PropertyClassGenerator.CreateClassInstanceFromCustomPropertys(cpInfos, this, $"Property_{typeFullName}.{param.PropertyInfo.PropertyName}_{Guid.NewGuid().ToString().Replace("-", "_")}", false);
                        InitTemplateClass_WPF(cpInfos);
                    }
                    break;
                case PropertyInfoAssist.enDirection.Get:
                    {
                        CollectLinkPinInfo(param, "CtrlValueOutHandle", param.PropertyInfo.PropertyType, CodeGenerateSystem.Base.enBezierType.Right, CodeGenerateSystem.Base.enLinkOpType.Start, true);

                        var pinDescOut = GetLinkPinDesc(param, "CtrlValueOutHandle");
                        mCtrlValueOutHandle.LinkType = pinDescOut.PinType;
                        mCtrlValueOutHandle.ClassType = pinDescOut.ClassType;
                    }
                    break;
            }
        }

        protected override void CollectionErrorMsg()
        {
            var param = CSParam as PropertyNodeConstructionParams;

            if (param.PropertyInfo.ParentClassType == null && string.IsNullOrEmpty(param.PropertyInfo.MacrossClassType))
            {
                HasError = true;
                ErrorDescription = "父类型不存在";
            }
            else if (param.PropertyInfo.PropertyType == null)
            {
                HasError = true;
                ErrorDescription = "属性类型不存在";
            }
            else if(param.PropertyInfo.ParentClassType != null)
            {
                var checkClassType = param.PropertyInfo.ParentClassType;
                if(param.PropertyInfo.ParentClassType.IsPointer)
                {
                    var typeName = param.PropertyInfo.ParentClassType.FullName.TrimEnd('*');
                    checkClassType = param.PropertyInfo.ParentClassType.Assembly.GetType(typeName);
                }
                if(param.PropertyInfo.IsField)
                {
                    var fieldInfo = checkClassType.GetField(param.PropertyInfo.PropertyName);
                    if(fieldInfo == null)
                    {
                        HasError = true;
                        ErrorDescription = "类成员已丢失";
                    }
                }
                else
                {
                    var propInfo = checkClassType.GetProperty(param.PropertyInfo.PropertyName);
                    if (propInfo == null)
                    {
                        HasError = true;
                        ErrorDescription = "所用属性已丢失";
                    }
                }
            }

            switch (param.PropertyInfo.HostType)
            {
                case MethodInfoAssist.enHostType.Target:
                    if(!mCtrlInHandle_Target.HasLink)
                    {
                        HasError = true;
                        ErrorDescription = "Target未链接";
                    }
                    break;
            }
        }

        public override void Save(XndNode xndNode, bool newGuid)
        {
            var param = CSParam as PropertyNodeConstructionParams;
            var att = xndNode.AddAttrib("data");
            att.Version = 2;
            att.BeginWrite();
            att.Write(AutoGenericIsNullCode);

            if (param.PropertyInfo.Direction == PropertyInfoAssist.enDirection.Set)
            {
                CodeGenerateSystem.Base.PropertyClassGenerator.SaveClassInstanceProperties(mTemplateClassInstance, att);
            }
            att.EndWrite();
            base.Save(xndNode, newGuid);
        }
        public override async System.Threading.Tasks.Task Load(XndNode xndNode)
        {
            await base.Load(xndNode);
            var att = xndNode.FindAttrib("data");
            if(att != null)
            {
                att.BeginRead();
                switch(att.Version)
                {
                    case 0:
                        {
                            att.Read(out mAutoGenericIsNullCode);
                        }
                        break;
                    case 1:
                        {
                            att.Read(out mAutoGenericIsNullCode);
                            var param = CSParam as PropertyNodeConstructionParams;
                            if (param.PropertyInfo.Direction == PropertyInfoAssist.enDirection.Set)
                            {
                                att.ReadMetaObject(mTemplateClassInstance);
                            }
                        }
                        break;
                    case 2:
                        {
                            var param = CSParam as PropertyNodeConstructionParams;
                            att.Read(out mAutoGenericIsNullCode);
                            CodeGenerateSystem.Base.PropertyClassGenerator.LoadClassInstanceProperties(mTemplateClassInstance, att);
                        }
                        break;
                }
                att.EndRead();
            }
        }

        //CodeGenerateSystem.Base.UsefulMemberHostData mHostUsefulMemberData = new CodeGenerateSystem.Base.UsefulMemberHostData();
        //public CodeGenerateSystem.Base.UsefulMemberHostData HostUsefulMemberData
        //{
        //    get { return mHostUsefulMemberData; }
        //    set { mHostUsefulMemberData = value; }
        //}
        // 类实例名称
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
        //public override void InitializeUsefulLinkDatas()
        //{
        //    var methodParamSplits = this.CSParam.ConstructParam.Split(';');
        //    if (methodParamSplits.Length < 2)
        //        return;

        //    mHostUsefulMemberData.ParseString(methodParamSplits[1], HostNodesContainer);

        //    switch (mHostUsefulMemberData.HostType)
        //    {
        //        case CodeGenerateSystem.Base.UsefulMemberHostData.enHostType.Static:
        //            ClassInstanceName = mHostUsefulMemberData.ClassTypeFullName;
        //            break;
        //        case CodeGenerateSystem.Base.UsefulMemberHostData.enHostType.Instance:
        //            ClassInstanceName = mHostUsefulMemberData.ClassTypeFullName + ".Instance";
        //            break;
        //        case CodeGenerateSystem.Base.UsefulMemberHostData.enHostType.Normal:
        //            {
        //                //                         var name = mHostUsefulMemberData.HostControl.GCode_GetValueName(mHostUsefulMemberData.LinkObject.LinkElement);
        //                //                         ClassInstanceName = name + "(" + mHostUsefulMemberData.ClassTypeFullName + ")";
        //                ClassInstanceName = mHostUsefulMemberData.ClassTypeFullName;
        //            }
        //            break;
        //    }

        //    mHostUsefulMemberData.HostControl?.AddMoveableChildNode(this);
        //    InitializeLinkLine();
        //}

        #region 代码生成

        public override string GCode_GetValueName(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            return "ClassPropertyValue_" + EngineNS.Editor.Assist.GetValuedGUIDString(Id);
        }

        public override string GCode_GetTypeString(CodeGenerateSystem.Base.LinkPinControl element, GenerateCodeContext_Method context)
        {
            var param = CSParam as PropertyNodeConstructionParams;
            return EngineNS.Rtti.RttiHelper.GetAppTypeString(param.PropertyInfo.PropertyType);
        }

        public override Type GCode_GetType(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            var param = CSParam as PropertyNodeConstructionParams;
            return param.PropertyInfo.PropertyType;
        }

        public override CodeExpression GCode_CodeDom_GetValue(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context, CodeGenerateSystem.Base.GenerateCodeContext_PreNode preNodeContext = null)
        {
            var param = CSParam as PropertyNodeConstructionParams;
            var proExp = new System.CodeDom.CodePropertyReferenceExpression();
            proExp.PropertyName = param.PropertyInfo.PropertyName;
            switch (param.PropertyInfo.HostType)
            {
                //case MethodInfoAssist.enHostType.Instance:
                //    proExp.TargetObject = new System.CodeDom.CodeSnippetExpression(param.PropertyInfo.ParentClassType.FullName + ".Instance");
                //    break;
                case MethodInfoAssist.enHostType.Static:
                    proExp.TargetObject = new System.CodeDom.CodeSnippetExpression(param.PropertyInfo.ParentClassType.FullName);
                    break;
                case MethodInfoAssist.enHostType.This:
                    {
                        if (mCtrlInHandle_Target.HasLink)
                            proExp.TargetObject = mCtrlInHandle_Target.GetLinkedObject(0, true).GCode_CodeDom_GetValue(mCtrlInHandle_Target.GetLinkedPinControl(0, true), context, new CodeGenerateSystem.Base.GenerateCodeContext_PreNode() { NeedDereferencePoint = true });
                        else
                            proExp.TargetObject = new System.CodeDom.CodeThisReferenceExpression();
                    }
                    break;
                case MethodInfoAssist.enHostType.Base:
                    {
                        if (mCtrlInHandle_Target.HasLink)
                            proExp.TargetObject = mCtrlInHandle_Target.GetLinkedObject(0, true).GCode_CodeDom_GetValue(mCtrlInHandle_Target.GetLinkedPinControl(0, true), context, new CodeGenerateSystem.Base.GenerateCodeContext_PreNode() { NeedDereferencePoint = true });
                        else
                            proExp.TargetObject = new CodeBaseReferenceExpression();
                    }
                    break;
                case MethodInfoAssist.enHostType.Target:
                    {
                        if (mCtrlInHandle_Target.HasLink)
                            proExp.TargetObject = mCtrlInHandle_Target.GetLinkedObject(0, true).GCode_CodeDom_GetValue(mCtrlInHandle_Target.GetLinkedPinControl(0, true), context, new CodeGenerateSystem.Base.GenerateCodeContext_PreNode() { NeedDereferencePoint = true });
                    }
                    break;
                case MethodInfoAssist.enHostType.Local:
                    {
                        proExp.TargetObject = null;
                    }
                    break;
            }

            return proExp;
        }

        public override async System.Threading.Tasks.Task GCode_CodeDom_GenerateCode(CodeTypeDeclaration codeClass, CodeStatementCollection codeStatementCollection, CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            if (element == mCtrlValueOutHandle)
            {
                // 收集数据
                var debugCodes = CodeDomNode.BreakPoint.BeginMacrossDebugCodeStatments(codeStatementCollection);
                CodeDomNode.BreakPoint.GetGatherDataValueCodeStatement(debugCodes, mCtrlValueOutHandle.GetLinkPinKeyName(), GCode_CodeDom_GetValue(mCtrlValueOutHandle, context), GCode_GetTypeString(mCtrlValueOutHandle, context), context);
                CodeDomNode.BreakPoint.EndMacrossDebugCodeStatements(codeStatementCollection, debugCodes);
            }
            else
            {
                var param = CSParam as PropertyNodeConstructionParams;
                if (mCtrlInHandle_Target.HasLink)
                {
                    var targetLinkObj = mCtrlInHandle_Target.GetLinkedObject(0, true);
                    if (!targetLinkObj.IsOnlyReturnValue)
                    {
                        await targetLinkObj.GCode_CodeDom_GenerateCode(codeClass, codeStatementCollection, mCtrlInHandle_Target.GetLinkedPinControl(0, true), context);
                    }
                }

                if (param.PropertyInfo.Direction == PropertyInfoAssist.enDirection.Set)
                {
                    #region Debug
                    // 设置属性之前的数据
                    // 收集用于调试的数据的代码
                    var debugCodes = CodeDomNode.BreakPoint.BeginMacrossDebugCodeStatments(codeStatementCollection);
                    CodeDomNode.BreakPoint.GetGatherDataValueCodeStatement(debugCodes, mCtrlValueInHandle.GetLinkPinKeyName(), GCode_CodeDom_GetValue(mCtrlValueInHandle, context), GCode_GetTypeString(mCtrlValueInHandle, context), context);
                    // 断点
                    var breakCondStatement = CodeDomNode.BreakPoint.BreakCodeStatement(codeClass, debugCodes, HostNodesContainer.GUID, Id);
                    //// 设置数据
                    //if (param.PropertyInfo.Direction == PropertyInfoAssist.enDirection.Set)
                    //    CodeDomNode.BreakPoint.GetSetDataValueCodeStatement(breakCondStatement.TrueStatements, Id, mCtrlValueInHandle.Name, GCode_CodeDom_GetValue(mCtrlValueInHandle, context), GCode_GetValueType(mCtrlValueInHandle));
                    CodeDomNode.BreakPoint.EndMacrossDebugCodeStatements(codeStatementCollection, debugCodes);
                    #endregion

                    if (mCtrlValueInHandle.HasLink)
                    {
                        var valInLinkObj = mCtrlValueInHandle.GetLinkedObject(0, true);
                        if (!valInLinkObj.IsOnlyReturnValue)
                            await valInLinkObj.GCode_CodeDom_GenerateCode(codeClass, codeStatementCollection, mCtrlValueInHandle.GetLinkedPinControl(0, true), context);

                        codeStatementCollection.Add(new CodeAssignStatement(GCode_CodeDom_GetValue(null, context), new CodeGenerateSystem.CodeDom.CodeCastExpression(param.PropertyInfo.PropertyType, valInLinkObj.GCode_CodeDom_GetValue(mCtrlValueInHandle.GetLinkedPinControl(0, true), context))));
                    }
                    else
                    {
                        var proType = param.PropertyInfo.PropertyType;
                        // 使用TemplateClass的参数
                        var proInfo = mTemplateClassInstance.GetType().GetProperty(param.PropertyInfo.PropertyName);
                        object proValue;
                        if (proInfo == null)
                            proValue = CodeGenerateSystem.Program.GetDefaultValueFromType(proType);
                        else
                            proValue = proInfo.GetValue(mTemplateClassInstance);

                        Program.GenerateAssignCode(codeStatementCollection, GCode_CodeDom_GetValue(null, context), proType, proValue);
                    }

                    // 赋值之后需要再收集一次数据
                    var debugCodesAfter = CodeDomNode.BreakPoint.BeginMacrossDebugCodeStatments(codeStatementCollection);
                    CodeDomNode.BreakPoint.GetGatherDataValueCodeStatement(debugCodesAfter, mCtrlValueInHandle.GetLinkPinKeyName(), GCode_CodeDom_GetValue(mCtrlValueInHandle, context), GCode_GetTypeString(mCtrlValueInHandle, context), context);
                    //CodeDomNode.BreakPoint.GetGatherDataValueCodeStatement(debugCodesAfter, mCtrlValueOutHandle.GetLinkPinKeyName(), GCode_CodeDom_GetValue(mCtrlValueOutHandle, context), GCode_GetValueType(mCtrlValueOutHandle, context), context);
                    CodeDomNode.BreakPoint.EndMacrossDebugCodeStatements(codeStatementCollection, debugCodesAfter);
                }
                else
                {
                    // 收集数据
                    var debugCodes = CodeDomNode.BreakPoint.BeginMacrossDebugCodeStatments(codeStatementCollection);
                    CodeDomNode.BreakPoint.GetGatherDataValueCodeStatement(debugCodes, mCtrlValueOutHandle.GetLinkPinKeyName(), GCode_CodeDom_GetValue(mCtrlValueOutHandle, context), GCode_GetTypeString(mCtrlValueOutHandle, context), context);
                    CodeDomNode.BreakPoint.EndMacrossDebugCodeStatements(codeStatementCollection, debugCodes);
                }

                if (context.GenerateNext && param.PropertyInfo.Direction == PropertyInfoAssist.enDirection.Set)
                {
                    if (mCtrlMethodOutHandle.HasLink)
                    {
                        await mCtrlMethodOutHandle.GetLinkedObject(0, false).GCode_CodeDom_GenerateCode(codeClass, codeStatementCollection, mCtrlMethodOutHandle.GetLinkedPinControl(0, false), context);
                    }
                }
            }
        }

        public override bool Pin_UseOrigionParamName(LinkPinControl linkElement)
        {
            var param = CSParam as PropertyNode.PropertyNodeConstructionParams;
            if (param.PropertyInfo.IsField)
                return true;
            return false;
        }

        #endregion
    }
}
