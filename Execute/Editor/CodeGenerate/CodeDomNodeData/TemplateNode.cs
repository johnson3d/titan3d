using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Text;

namespace CodeDomNode
{
    public partial class TemplateNode : CodeGenerateSystem.Base.BaseNodeControl
    {
        CodeGenerateSystem.Base.LinkControl mCtrlreturnLink = new CodeGenerateSystem.Base.LinkControl();
        CodeGenerateSystem.Base.LinkControl mCtrlparamLink = new CodeGenerateSystem.Base.LinkControl();
        CodeGenerateSystem.Base.LinkObjInfo mReturnLinkInfo;
        CodeGenerateSystem.Base.LinkObjInfo mParanLinkInfo;
        Type mReturnType;
        Type mParamType;

        string mTitleNameText = "模板";
        public string TitleNameText
        {
            get { return mTitleNameText; }
            set
            {
                mTitleNameText = value;
                OnPropertyChanged("TitleNameText");
            }
        }
        string mIDText = "Id";
        public string IDText
        {
            get { return mIDText; }
            set
            {
                mIDText = value;
                OnPropertyChanged("IDText");
            }
        }
        Type mComboBoxTemplateSelectItem = null;
        public Type ComboBoxTemplateSelectItem
        {
            get { return mComboBoxTemplateSelectItem; }
            set
            {
                mComboBoxTemplateSelectItem = value;
                OnPropertyChanged("ComboBoxTemplateSelectItem");
            }
        }
        List<Type> mTemplateTypes = new List<Type>();
        List<CodeGenerateSystem.Base.CustomPropertyInfo> mCustomPropertyInfos = new List<CodeGenerateSystem.Base.CustomPropertyInfo>();

        partial void InitConstruction();
        partial void Init_WPF();
        public TemplateNode(CodeGenerateSystem.Base.ConstructionParams csParam, string param)
            : base(csParam, param)
        {
            InitConstruction();

            NodeName = TitleNameText;
            if (HostNodesContainer != null)
            {
                NodeName = HostNodesContainer.GetNodeName(TitleNameText);
                TitleNameText = NodeName;
            }

            mReturnLinkInfo = AddLinkObject(CodeGenerateSystem.Base.enLinkType.Class, mCtrlreturnLink, CodeGenerateSystem.Base.enBezierType.Right, CodeGenerateSystem.Base.enLinkOpType.Start, null, true);
            //             mReturnLinkInfo.OnAddLinkInfo += (info)=>
            //             {
            //                 Method_Pre.Visibility = Visibility.Collapsed;
            //                 Method_Next.Visibility = Visibility.Collapsed;
            //             };
            //             mReturnLinkInfo.OnDelLinkInfo += (info) =>
            //             {
            //                 Method_Pre.Visibility = Visibility.Visible;
            //                 Method_Next.Visibility = Visibility.Visible;
            //             };
            mParanLinkInfo = AddLinkObject(CodeGenerateSystem.Base.enLinkType.NumbericalValue, mCtrlparamLink, CodeGenerateSystem.Base.enBezierType.Left, CodeGenerateSystem.Base.enLinkOpType.End, null, false);

            //AddLinkObject(CodeGenerateSystem.Base.enLinkType.Method, Method_Pre, CodeGenerateSystem.Base.enBezierType.Top, CodeGenerateSystem.Base.enLinkOpType.End, Method_Pre.BackBrush, false);
            //AddLinkObject(CodeGenerateSystem.Base.enLinkType.Method, Method_Next, CodeGenerateSystem.Base.enBezierType.Bottom, CodeGenerateSystem.Base.enLinkOpType.Start, Method_Next.BackBrush, false);

            var types = EngineNS.Rtti.RttiHelper.GetTypes();
            foreach (var type in types)
            {
                var atts = type.GetCustomAttributes(typeof(CSUtility.Data.DataTemplateAttribute), false);
                if (atts.Length <= 0)
                    continue;

                mTemplateTypes.Add(type);
            }

            Init_WPF();
            SetParameters();
            foreach (var i in mTemplateTypes)
            {
                if (i.FullName == param)
                {
                    ComboBoxTemplateSelectItem = i;
                    mReturnType = mComboBoxTemplateSelectItem;
                    break;
                }
            }
            mParamType = CSUtility.Data.DataTemplateManagerAssist.Instance.GetDataTemplateIDType(mReturnType);
            mParanLinkInfo.LinkType = CodeGenerateSystem.Base.LinkObjInfo.GetLinkTypeFromCommonType(mParamType);
        }
        protected void SetParameters()
        {
            if (mParamType == null)
                return;

            var cpInfo = CodeGenerateSystem.Base.CustomPropertyInfo.GetFromParamInfo(mParamType, "Id");
            mCustomPropertyInfos.Add(cpInfo);
        }
        public override string GetLinkObjParamInfo(CodeGenerateSystem.Base.LinkObjInfo linkObj)
        {
            string returnStr = "";

            if (linkObj.LinkElement == mCtrlparamLink)
            {
                returnStr += ":" + IDText;
            }
            else
            {
                returnStr += ":";
            }
            returnStr += ":" + linkObj.LinkType.ToString();
            return returnStr;
        }
        partial void RefreshValueFromTemplateClass2PropertyInfoList();
        partial void RefreshValueFromPropertyInfoList2TemplateClass();
        public override void Save(EngineNS.IO.XmlNode xmlNode, bool newGuid, EngineNS.IO.XmlHolder holder)
        {
            RefreshValueFromTemplateClass2PropertyInfoList();
            if(mCustomPropertyInfos.Count > 0)
            {
                var node = xmlNode.AddNode("DefaultParamValue", "", holder);
                foreach(var proInfo in mCustomPropertyInfos)
                {
                    if (!CodeGenerateSystem.Base.PropertyClassGenerator.IsPropertyInfoValid(proInfo))
                        continue;

                    var cNode = node.AddNode(proInfo.PropertyName, "", holder);
                    CSUtility.Support.IConfigurator.SaveValue(cNode, proInfo.PropertyType, proInfo.CurrentValue, holder);
                }
            }

            base.Save(xmlNode, newGuid, holder);
        }
        public override void Load(EngineNS.IO.XmlNode xmlNode, double deltaX, double deltaY)
        {
            base.Load(xmlNode, deltaX, deltaY);

            var node = xmlNode.FindNode("DefaultParamValue");
            if (node != null)
            {
                foreach(var proInfo in mCustomPropertyInfos)
                {
                    var cNode = node.FindNode(proInfo.PropertyName);
                    if(cNode != null)
                    {
                        var attType = cNode.FindAttrib("Type");
                        var proType = EngineNS.Rtti.RttiHelper.GetTypeFromSaveString(attType.Value, mCSParam.CSType);
                        proInfo.CurrentValue = CSUtility.Support.IConfigurator.ReadValue(cNode, proType);
                    }
                }
            }
            RefreshValueFromPropertyInfoList2TemplateClass();
        }
        public override string GCode_GetValueType(CodeGenerateSystem.Base.LinkControl element)
        {
            return mReturnType.FullName;
        }

        public override Type GCode_GetType(CodeGenerateSystem.Base.LinkControl element)
        {
            return mReturnType;
        }

        public override void GCode_CodeDom_GenerateCode(CodeTypeDeclaration codeClass, CodeStatementCollection codeStatementCollection, CodeGenerateSystem.Base.LinkControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            if (element == null || element == mCtrlreturnLink)
            {
                var linkOI = GetLinkObjInfo(mCtrlparamLink);
                if (linkOI.HasLink)
                {
                    if (!linkOI.GetLinkObject(0, true).IsOnlyReturnValue)
                        linkOI.GetLinkObject(0, true).GCode_CodeDom_GenerateCode(codeClass, codeStatementCollection, linkOI.GetLinkElement(0, true), context);
                }
            }
            //             if(element == Method_Pre)
            //             {
            //                 string value = "CSUtility.Data.DataTemplateManager<";
            //                 value += mParamType.FullName + ", " + mReturnType.FullName + ">.Instance";
            // 
            //                 System.CodeDom.CodeMethodReferenceExpression methodRef = new System.CodeDom.CodeMethodReferenceExpression();
            //                 methodRef.TargetObject = new System.CodeDom.CodeSnippetExpression(value);
            //                 methodRef.MethodName = "GetDataTemplate";
            // 
            //                 CodeExpression[] exps = new CodeExpression[1];
            //                 var info = GetLinkObjInfo(paramLink);
            //                 if (info.HasLink)
            //                 {
            //                     var ep = info.GetLinkObject(0, true).GCode_CodeDom_GetValue(info.GetLinkElement(0, true));
            //                     exps[0] = ep;                    
            //                 }
            //                 else
            //                 {
            //                     var proInfo = mTemplateClassInstance.GetType().GetProperty("Id");
            //                     var classValue = proInfo.GetValue(mTemplateClassInstance);                    
            //                     exps[0] = new CodePrimitiveExpression(classValue);
            //                 }
            //                 var exp = new System.CodeDom.CodeMethodInvokeExpression(methodRef, exps);
            //                 codeStatementCollection.Add(exp);
            //             }
        }

        public override CodeExpression GCode_CodeDom_GetValue(CodeGenerateSystem.Base.LinkControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            RefreshValueFromTemplateClass2PropertyInfoList();
            if (element == mCtrlreturnLink)
            {
                var info = GetLinkObjInfo(mCtrlparamLink);

                string value = "CSUtility.Data.DataTemplateManager<";
                value += mParamType.FullName + ", " + mReturnType.FullName + ">.Instance";

                System.CodeDom.CodeMethodReferenceExpression methodRef = new System.CodeDom.CodeMethodReferenceExpression();
                methodRef.TargetObject = new System.CodeDom.CodeSnippetExpression(value);
                methodRef.MethodName = "GetDataTemplate";

                CodeExpression[] exps = new CodeExpression[1];
                if (info.HasLink)
                {
                    var ep = info.GetLinkObject(0, true).GCode_CodeDom_GetValue(info.GetLinkElement(0, true), context);
                    exps[0] = ep;
                }
                else
                {
                    foreach(var proInfo in mCustomPropertyInfos)
                    {
                        if(proInfo.PropertyName == "Id")
                        {
                            exps[0] = new CodePrimitiveExpression(proInfo.CurrentValue);
                            break;
                        }
                    }
                }
                return new System.CodeDom.CodeMethodInvokeExpression(methodRef, exps);
            }
            return base.GCode_CodeDom_GetValue(element, context);
        }
    }
}
