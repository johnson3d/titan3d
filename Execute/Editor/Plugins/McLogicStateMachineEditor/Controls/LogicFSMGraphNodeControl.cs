using CodeGenerateSystem.Base;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Data;

namespace McLogicStateMachineEditor.Controls
{
    public class LogicFSMGraphNodeControlConstructionParams : CodeGenerateSystem.Base.ConstructionParams
    {
        [EngineNS.Rtti.MetaData]
        public string LAGNodeName = "LogicGraphNode";
        [EngineNS.Rtti.MetaData]
        public bool IsSelfGraphNode { get; set; } = false;
        [EngineNS.Rtti.MetaData]
        public Guid LinkedCategoryItemID { get; set; } = Guid.Empty;
        public override EditorCommon.CodeGenerateSystem.INodeConstructionParams Duplicate()
        {
            var retVal = base.Duplicate() as LogicFSMGraphNodeControlConstructionParams;
            retVal.IsSelfGraphNode = IsSelfGraphNode;
            retVal.LAGNodeName = LAGNodeName;
            retVal.LinkedCategoryItemID = LinkedCategoryItemID;
            return retVal;
        }
    }
    [CodeGenerateSystem.CustomConstructionParamsAttribute(typeof(LogicFSMGraphNodeControlConstructionParams))]
    public partial class LogicFSMGraphNodeControl : CodeGenerateSystem.Base.BaseNodeControl
    {
        public bool IsSelfGraphNode { get; set; } = false;
        public Guid LinkedCategoryItemID { get; set; } = Guid.Empty;
        CodeGenerateSystem.Base.LinkPinControl mCtrlValueLinkHandle = new CodeGenerateSystem.Base.LinkPinControl();
        public LinkPinControl CtrlValueLinkHandle
        {
            get => mCtrlValueLinkHandle;
        }
        public List<string> Notifies = new List<string>();
        partial void InitConstruction();
        CodeGenerateSystem.Base.GeneratorClassBase mTemplateClassInstance = null;
        public override object GetShowPropertyObject()
        {
            return mTemplateClassInstance;
        }
        public void SetName(string name)
        {
            NodeName = name;
            var param = CSParam as LogicFSMGraphNodeControlConstructionParams;
            param.LAGNodeName = name;
        }
        public LogicFSMGraphNodeControl(LogicFSMGraphNodeControlConstructionParams csParam)
            : base(csParam)
        {
            InitConstruction();



            //if (csParam.AnimAsset == null)
            //{
            //    csParam.AnimAsset = new AnimAsset();
            //    if (csParam.HostNodesContainer.HostNode is AnimStateControl)
            //    {
            //        csParam.AnimAsset.AnimAssetLocationName = csParam.HostNodesContainer.HostNode.NodeName;
            //        csParam.AnimAsset.AnimAssetLocation = AnimAssetLocation.State;
            //        csParam.AnimAsset.AnimAssetName = csParam.NodeName.PureName();
            //    }
            //    else
            //    {
            //        System.Diagnostics.Debugger.Break();
            //    }
            //}
            var cpInfos = new List<CodeGenerateSystem.Base.CustomPropertyInfo>();
            mTemplateClassInstance = CodeGenerateSystem.Base.PropertyClassGenerator.CreateClassInstanceFromCustomPropertys(cpInfos, this, null, false);
            var clsType = mTemplateClassInstance.GetType();
            //var xNamePro = clsType.GetProperty("IsLoop");
            //xNamePro.SetValue(mTemplateClassInstance, csParam.IsLoop);
            //RNameNodeName = csParam.NodeName;
            NodeName = csParam.LAGNodeName;
            LinkedCategoryItemID = csParam.LinkedCategoryItemID;
            IsOnlyReturnValue = true;
            IsSelfGraphNode = csParam.IsSelfGraphNode;
            if (IsSelfGraphNode)
            {
                IsDeleteable = false;
                mCtrlValueLinkHandle = BottomValueLinkHandle;
                LeftValueLinkHandle.Visibility = System.Windows.Visibility.Collapsed;
                AddLinkPinInfo("BottomLogicClipLinkHandle", mCtrlValueLinkHandle, null);
                if (HostNodesContainer != null && NodeName != HostNodesContainer.HostControl.LinkedCategoryItemName)
                {
                    SetName(HostNodesContainer.HostControl.LinkedCategoryItemName);
                }
            }
            else
            {
                mCtrlValueLinkHandle = LeftValueLinkHandle;
                BottomValueLinkHandle.Visibility = System.Windows.Visibility.Collapsed;
                AddLinkPinInfo("LeftLogicClipLinkHandle", mCtrlValueLinkHandle, null);
            }
        }
        public override bool CanDuplicate()
        {
            return false;
        }
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            var textBlock = Template.FindName("OutGridShowText", this) as TextBlock;
            textBlock.SetBinding(TextBlock.TextProperty, new Binding("NodeName") { Source = this, Mode = BindingMode.TwoWay });
            //this.SetBinding(NodeNameBinderProperty, new Binding("Text") { Source = textBlock, Mode = BindingMode.TwoWay });
        }
        public static void InitNodePinTypes(CodeGenerateSystem.Base.ConstructionParams smParam)
        {
            var param = smParam as LogicFSMGraphNodeControlConstructionParams;
            if (param.IsSelfGraphNode)
                CollectLinkPinInfo(smParam, "BottomLogicClipLinkHandle", CodeGenerateSystem.Base.enLinkType.LAState, CodeGenerateSystem.Base.enBezierType.Bottom, CodeGenerateSystem.Base.enLinkOpType.Start, true);
            else
                CollectLinkPinInfo(smParam, "LeftLogicClipLinkHandle", CodeGenerateSystem.Base.enLinkType.LAState, CodeGenerateSystem.Base.enBezierType.Left, CodeGenerateSystem.Base.enLinkOpType.End, true);
        }

        public override string GCode_GetTypeString(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            return "LogicClipNode";
        }

        public override Type GCode_GetType(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            return typeof(LogicFSMGraphNodeControl);
        }
        public override CodeExpression GCode_CodeDom_GetSelfRefrence(LinkPinControl element, GenerateCodeContext_Method context, GenerateCodeContext_PreNode preNodeContext = null)
        {
            var validName = StringRegex.GetValidName(NodeName);
            return new CodeVariableReferenceExpression(validName);
        }
        public override System.CodeDom.CodeExpression GCode_CodeDom_GetValue(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context, CodeGenerateSystem.Base.GenerateCodeContext_PreNode preNodeContext = null)
        {
            return new System.CodeDom.CodeSnippetExpression("System.Math.Sin((System.DateTime.Now.Millisecond*0.001)*2*System.Math.PI)");
        }
        public override async System.Threading.Tasks.Task GCode_CodeDom_GenerateCode(CodeTypeDeclaration codeClass, CodeStatementCollection codeStatementCollection, LinkPinControl element, GenerateCodeContext_Method context)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
        }
    }
}
