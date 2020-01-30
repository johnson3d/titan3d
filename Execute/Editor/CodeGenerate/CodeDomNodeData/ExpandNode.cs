using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using CodeGenerateSystem.Base;
using EngineNS.IO;

namespace CodeDomNode
{
    [CodeGenerateSystem.CustomConstructionParams(typeof(ExpandNodeConstructParam))]
    public partial class ExpandNode : CodeGenerateSystem.Base.BaseNodeControl
    {
        [EngineNS.Rtti.MetaClass]
        public class ExpandNodeConstructParam : CodeGenerateSystem.Base.ConstructionParams
        {
            [EngineNS.Rtti.MetaData]
            public Type ExpandType { get; set; }
            [EngineNS.Rtti.MetaData]
            public List<string> ActiveMembers { get; set; } = new List<string>();

            public ExpandNodeConstructParam()
            {
            }

            public override EditorCommon.CodeGenerateSystem.INodeConstructionParams Duplicate()
            {
                var retVal = base.Duplicate() as ExpandNodeConstructParam;
                retVal.ExpandType = ExpandType;
                retVal.ActiveMembers = new List<string>(ActiveMembers);
                return retVal;
            }
            public override bool Equals(object obj)
            {
                if (!base.Equals(obj))
                    return false;
                var param = obj as ExpandNodeConstructParam;
                if (param == null)
                    return false;
                if (ExpandType == param.ExpandType)
                    return true;
                return false;
            }
            public override int GetHashCode()
            {
                return (base.GetHashCodeString() + EngineNS.Rtti.RttiHelper.GetAppTypeString(ExpandType)).GetHashCode();
            }
        }

        // 临时类，用于选中后显示参数属性
        CodeGenerateSystem.Base.GeneratorClassBase mTemplateClassInstance_All = null;
        CodeGenerateSystem.Base.GeneratorClassBase mTemplateClassInstance_Show = null;
        public CodeGenerateSystem.Base.GeneratorClassBase TemplateClassInstance_Show
        {
            get { return mTemplateClassInstance_Show; }
        }

        CodeGenerateSystem.Base.LinkPinControl mCtrlMethodPin_Pre = new CodeGenerateSystem.Base.LinkPinControl();
        CodeGenerateSystem.Base.LinkPinControl mCtrlMethodPin_Next = new CodeGenerateSystem.Base.LinkPinControl();
        CodeGenerateSystem.Base.LinkPinControl mCtrlTarget = new CodeGenerateSystem.Base.LinkPinControl();
        CodeGenerateSystem.Base.LinkPinControl mCtrlTargetOut = new CodeGenerateSystem.Base.LinkPinControl();
        StackPanel mParamPanel = null;
        partial void InitConstruction();
        public ExpandNode(CodeGenerateSystem.Base.ConstructionParams csParam)
            : base(csParam)
        {
            InitConstruction();
            IsOnlyReturnValue = true;

            AddLinkPinInfo("CtrlMethodPin_Pre", mCtrlMethodPin_Pre);
            AddLinkPinInfo("CtrlMethodPin_Next", mCtrlMethodPin_Next);
            AddLinkPinInfo("CtrlTarget", mCtrlTarget);
            AddLinkPinInfo("CtrlTargetOut", mCtrlTargetOut);

            var param = csParam as ExpandNodeConstructParam;
            NodeName = EngineNS.Rtti.RttiHelper.GetAppTypeString(param.ExpandType);

            CreateTemplateClass_All();
            CreateTemplateClass_Show();
        }
        public static void InitNodePinTypes(CodeGenerateSystem.Base.ConstructionParams smParam)
        {
            var param = smParam as ExpandNodeConstructParam;

            CollectLinkPinInfo(smParam, "CtrlMethodPin_Pre", CodeGenerateSystem.Base.enLinkType.Method, CodeGenerateSystem.Base.enBezierType.Left, CodeGenerateSystem.Base.enLinkOpType.End, false);
            CollectLinkPinInfo(smParam, "CtrlMethodPin_Next", CodeGenerateSystem.Base.enLinkType.Method, CodeGenerateSystem.Base.enBezierType.Right, CodeGenerateSystem.Base.enLinkOpType.Start, false);
            CollectLinkPinInfo(smParam, "CtrlTarget", param.ExpandType, CodeGenerateSystem.Base.enBezierType.Left, CodeGenerateSystem.Base.enLinkOpType.End, false);
            CollectLinkPinInfo(smParam, "CtrlTargetOut", param.ExpandType, CodeGenerateSystem.Base.enBezierType.Right, CodeGenerateSystem.Base.enLinkOpType.Start, true);
        }

        void CreateTemplateClass_All()
        {
            var cpInfos_All = new List<CodeGenerateSystem.Base.CustomPropertyInfo>();
            var param = CSParam as ExpandNodeConstructParam;
            if (param.ExpandType == null)
                return;
            foreach(var pro in param.ExpandType.GetProperties())
            {
                if (!pro.CanWrite)
                    continue;
                var atts = pro.GetCustomAttributes(typeof(EngineNS.Editor.MacrossMemberAttribute), false);
                if (atts == null || atts.Length == 0)
                    continue;

                var mm = atts[0] as EngineNS.Editor.MacrossMemberAttribute;
                // 只读不能设置
                if (mm.HasType(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.ReadOnly))
                    continue;

                var cpInfo = new CodeGenerateSystem.Base.CustomPropertyInfo();
                cpInfo.PropertyName = pro.Name;
                cpInfo.PropertyType = pro.PropertyType;
                foreach (var att in pro.GetCustomAttributes(false))
                {
                    cpInfo.PropertyAttributes.Add(att as System.Attribute);
                }
                cpInfo.DefaultValue = CodeGenerateSystem.Program.GetDefaultValueFromType(cpInfo.PropertyType);
                cpInfo.CurrentValue = cpInfo.DefaultValue;
                cpInfos_All.Add(cpInfo);
            }
            foreach(var field in param.ExpandType.GetFields())
            {
                var atts = field.GetCustomAttributes(typeof(EngineNS.Editor.MacrossMemberAttribute), false);
                if (atts == null || atts.Length == 0)
                    continue;

                var mm = atts[0] as EngineNS.Editor.MacrossMemberAttribute;
                // 只读不能设置
                if (mm.HasType(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.ReadOnly))
                    continue;

                var cpInfo = new CodeGenerateSystem.Base.CustomPropertyInfo();
                cpInfo.PropertyName = field.Name;
                cpInfo.PropertyType = field.FieldType;
                foreach (var att in field.GetCustomAttributes(false))
                {
                    cpInfo.PropertyAttributes.Add(att as System.Attribute);
                }
                cpInfo.DefaultValue = CodeGenerateSystem.Program.GetDefaultValueFromType(cpInfo.PropertyType);
                cpInfo.CurrentValue = cpInfo.DefaultValue;
                cpInfos_All.Add(cpInfo);
            }
            mTemplateClassInstance_All = CodeGenerateSystem.Base.PropertyClassGenerator.CreateClassInstanceFromCustomPropertys(cpInfos_All, this, $"ExpandNode_{param.ExpandType.FullName}.{NodeName}", false);
        }
        void CreateTemplateClass_Show()
        {
            var allType = mTemplateClassInstance_All.GetType();
            if (mTemplateClassInstance_Show != null)
            {
                foreach (var pro in mTemplateClassInstance_Show.GetType().GetProperties())
                {
                    var allPro = allType.GetProperty(pro.Name);
                    allPro.SetValue(mTemplateClassInstance_All, pro.GetValue(mTemplateClassInstance_Show));
                }
            }

            var cpInfos_Show = new List<CodeGenerateSystem.Base.CustomPropertyInfo>();
            var param = CSParam as ExpandNodeConstructParam;
            foreach (var pro in param.ExpandType.GetProperties())
            {
                if (!pro.CanWrite)
                    continue;
                var atts = pro.GetCustomAttributes(typeof(EngineNS.Editor.MacrossMemberAttribute), false);
                if (atts == null || atts.Length == 0)
                    continue;

                var mm = atts[0] as EngineNS.Editor.MacrossMemberAttribute;
                // 只读不能设置
                if (mm.HasType(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.ReadOnly))
                    continue;

                if (param.ActiveMembers.Contains(pro.Name))
                {
                    var cpInfo = new CodeGenerateSystem.Base.CustomPropertyInfo();
                    cpInfo.PropertyName = pro.Name;
                    cpInfo.PropertyType = pro.PropertyType;
                    foreach (var att in pro.GetCustomAttributes(false))
                    {
                        cpInfo.PropertyAttributes.Add(att as System.Attribute);
                    }
                    cpInfo.DefaultValue = CodeGenerateSystem.Program.GetDefaultValueFromType(cpInfo.PropertyType);
                    cpInfo.CurrentValue = cpInfo.DefaultValue;

                    cpInfos_Show.Add(cpInfo);
                }
            }
            foreach (var field in param.ExpandType.GetFields())
            {
                var atts = field.GetCustomAttributes(typeof(EngineNS.Editor.MacrossMemberAttribute), false);
                if (atts == null || atts.Length == 0)
                    continue;

                var mm = atts[0] as EngineNS.Editor.MacrossMemberAttribute;
                // 只读不能设置
                if (mm.HasType(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.ReadOnly))
                    continue;

                if (param.ActiveMembers.Contains(field.Name))
                {
                    var cpInfo = new CodeGenerateSystem.Base.CustomPropertyInfo();
                    cpInfo.PropertyName = field.Name;
                    cpInfo.PropertyType = field.FieldType;
                    foreach (var att in field.GetCustomAttributes(false))
                    {
                        cpInfo.PropertyAttributes.Add(att as System.Attribute);
                    }
                    cpInfo.DefaultValue = CodeGenerateSystem.Program.GetDefaultValueFromType(cpInfo.PropertyType);
                    cpInfo.CurrentValue = cpInfo.DefaultValue;

                    cpInfos_Show.Add(cpInfo);
                }
            }

            mTemplateClassInstance_Show = CodeGenerateSystem.Base.PropertyClassGenerator.CreateClassInstanceFromCustomPropertys(cpInfos_Show, this, "ExpandShow" + Guid.NewGuid().ToString(), false);
            bool createNewAllTC = false;
            foreach (var pro in mTemplateClassInstance_Show.GetType().GetProperties())
            {
                var allPro = allType.GetProperty(pro.Name);
                if (allPro == null)
                    createNewAllTC = true;
                else
                    pro.SetValue(mTemplateClassInstance_Show, allPro.GetValue(mTemplateClassInstance_All));
            }
            if(createNewAllTC)
            {
                var showType = mTemplateClassInstance_Show.GetType();
                CreateTemplateClass_All();
                foreach (var pro in mTemplateClassInstance_All.GetType().GetProperties())
                {
                    var showPro = showType.GetProperty(pro.Name);
                    pro.SetValue(mTemplateClassInstance_All, showPro.GetValue(mTemplateClassInstance_Show));
                }
            }
        }

        public override void Save(XndNode xndNode, bool newGuid)
        {
            var att = xndNode.AddAttrib("defaultParam");
            att.Version = 1;
            att.BeginWrite();
            var allType = mTemplateClassInstance_All.GetType();
            foreach(var pro in mTemplateClassInstance_Show.GetType().GetProperties())
            {
                var allPro = allType.GetProperty(pro.Name);
                allPro.SetValue(mTemplateClassInstance_All, pro.GetValue(mTemplateClassInstance_Show));
            }
            CodeGenerateSystem.Base.PropertyClassGenerator.SaveClassInstanceProperties(mTemplateClassInstance_All, att);
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
                                att.ReadMetaObject(mTemplateClassInstance_All);
                                var allType = mTemplateClassInstance_All.GetType();
                                foreach (var pro in mTemplateClassInstance_Show.GetType().GetProperties())
                                {
                                    var allPro = allType.GetProperty(pro.Name);
                                    pro.SetValue(mTemplateClassInstance_Show, allPro.GetValue(mTemplateClassInstance_All));
                                }
                            }
                            break;
                        case 1:
                            {
                                CodeGenerateSystem.Base.PropertyClassGenerator.LoadClassInstanceProperties(mTemplateClassInstance_All, att);
                                var allType = mTemplateClassInstance_All.GetType();
                                foreach (var pro in mTemplateClassInstance_Show.GetType().GetProperties())
                                {
                                    var allPro = allType.GetProperty(pro.Name);
                                    pro.SetValue(mTemplateClassInstance_Show, allPro.GetValue(mTemplateClassInstance_All));
                                }
                            }
                            break;
                    }
                    att.EndRead();
                }

                foreach(var child in mChildNodes)
                {
                    var childParam = child.CSParam as ExpandNodeChild.ExpandNodeChildConstructionParams;
                    foreach (var cb in StackPanel_Members.Children)
                    {
                        var cbTemp = cb as CheckBox;
                        if (((string)cbTemp.Content) == childParam.ParamName)
                        {
                            cbTemp.Tag = child;
                        }
                    }
                }
            }
            catch(System.Exception e)
            {
                HasError = true;
                ErrorDescription = "节点读取失败！";
                System.Diagnostics.Debug.WriteLine($"节点读取失败！：Name=展开 {NodeName}\r\n{e.ToString()}");
            }
        }

        public override CodeExpression GCode_CodeDom_GetValue(LinkPinControl element, GenerateCodeContext_Method context, GenerateCodeContext_PreNode preNodeContext = null)
        {
            if (element == null || element == mCtrlTarget || element == mCtrlTargetOut)
            {
                return mCtrlTarget.GetLinkedObject(0).GCode_CodeDom_GetValue(mCtrlTarget.GetLinkedPinControl(0), context, null);
            }
            else
                throw new InvalidOperationException();
        }
        public override async Task GCode_CodeDom_GenerateCode(CodeTypeDeclaration codeClass, CodeStatementCollection codeStatementCollection, LinkPinControl element, GenerateCodeContext_Method context)
        {
            if(element == null || element == mCtrlMethodPin_Pre)
            {
                var tagLinkedObj = mCtrlTarget.GetLinkedObject(0);
                if (!tagLinkedObj.IsOnlyReturnValue)
                    await tagLinkedObj.GCode_CodeDom_GenerateCode(codeClass, codeStatementCollection, mCtrlTarget.GetLinkedPinControl(0), context);

                var tagExp = tagLinkedObj.GCode_CodeDom_GetValue(mCtrlTarget.GetLinkedPinControl(0), context);

                #region Debug
                // 设置之前的数据
                var debugPreCodes = CodeDomNode.BreakPoint.BeginMacrossDebugCodeStatments(codeStatementCollection);
                foreach (var paramNode in mChildNodes)
                {
                    var ctrl = paramNode as ExpandNodeChild;
                    var ctrlParam = ctrl.CSParam as ExpandNodeChild.ExpandNodeChildConstructionParams;
                    CodeDomNode.BreakPoint.GetGatherDataValueCodeStatement(debugPreCodes, ctrl.CtrlValue_In.GetLinkPinKeyName(), ctrl.GCode_CodeDom_GetValue(null, null), ctrl.GCode_GetTypeString(null, null), context);
                }
                CodeDomNode.BreakPoint.BreakCodeStatement(codeClass, debugPreCodes, HostNodesContainer.GUID, Id);
                CodeDomNode.BreakPoint.EndMacrossDebugCodeStatements(codeStatementCollection, debugPreCodes);
                #endregion

                foreach (var paramNode in mChildNodes)
                {
                    var ctrl = paramNode as ExpandNodeChild;
                    var ctrlParam = ctrl.CSParam as ExpandNodeChild.ExpandNodeChildConstructionParams;
                    await ctrl.GCode_CodeDom_GenerateCode(codeClass, codeStatementCollection, null, context);
                }

                #region Debug
                // 赋值之后再收集一次数据
                var debugCodesAfter = CodeDomNode.BreakPoint.BeginMacrossDebugCodeStatments(codeStatementCollection);
                foreach (var paramNode in mChildNodes)
                {
                    var ctrl = paramNode as ExpandNodeChild;
                    var ctrlParam = ctrl.CSParam as ExpandNodeChild.ExpandNodeChildConstructionParams;
                    CodeDomNode.BreakPoint.GetGatherDataValueCodeStatement(debugCodesAfter, ctrl.CtrlValue_In.GetLinkPinKeyName(), ctrl.GCode_CodeDom_GetValue(null, null), ctrl.GCode_GetTypeString(null, null), context);
                }
                CodeDomNode.BreakPoint.EndMacrossDebugCodeStatements(codeStatementCollection, debugCodesAfter);
                #endregion
            }

            if (context.GenerateNext)
            {
                if (mCtrlMethodPin_Next.HasLink)
                {
                    await mCtrlMethodPin_Next.GetLinkedObject(0).GCode_CodeDom_GenerateCode(codeClass, codeStatementCollection, mCtrlMethodPin_Next.GetLinkedPinControl(0), context);
                }
            }
        }
    }
}
