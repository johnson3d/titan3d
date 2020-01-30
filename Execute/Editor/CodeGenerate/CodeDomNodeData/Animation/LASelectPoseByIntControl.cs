using CodeGenerateSystem.Base;
using EngineNS.Bricks.Animation.AnimNode;
using EngineNS.Bricks.Animation.AnimStateMachine;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace CodeDomNode.Animation
{
    public class LASelectPoseByIntControlConstructionParams : CodeGenerateSystem.Base.ConstructionParams
    {
        [EngineNS.Rtti.MetaData]
        public Int32 ActiveValue { get; set; } = 0;
        [EngineNS.Rtti.MetaData]
        public string NodeName { get; set; } = "SelectPoseByInt";

        public override EditorCommon.CodeGenerateSystem.INodeConstructionParams Duplicate()
        {
            var retVal = base.Duplicate() as LASelectPoseByIntControlConstructionParams;

            retVal.ActiveValue = ActiveValue;
            retVal.NodeName = NodeName;
            return retVal;
        }
    }
    [CodeGenerateSystem.CustomConstructionParamsAttribute(typeof(LASelectPoseByIntControlConstructionParams))]
    public partial class LASelectPoseByIntControl : CodeGenerateSystem.Base.BaseNodeControl
    {
        partial void InitConstruction();

        #region DP

        public Int32 ActiveValue
        {
            get { return (Int32)GetValue(ActiveValueProperty); }
            set
            {
                SetValue(ActiveValueProperty, value);
                var para = CSParam as LASelectPoseByIntControlConstructionParams;
                para.ActiveValue = value;
            }
        }
        public static readonly DependencyProperty ActiveValueProperty = DependencyProperty.Register("ActiveValue", typeof(Int32), typeof(LASelectPoseByIntControl), new UIPropertyMetadata(0, OnActiveValuePropertyyChanged));
        private static void OnActiveValuePropertyyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = sender as LASelectPoseByIntControl;
            if ((e.NewValue == e.OldValue))
                return;
            ctrl.ActiveValue = (Int32)e.NewValue;
        }

        #endregion

        #region ShowProperty
        CodeGenerateSystem.Base.GeneratorClassBase mTemplateClassInstance = null;
        public override object GetShowPropertyObject()
        {
            return mTemplateClassInstance;
        }
        void BindingTemplateClassInstanceProperties()
        {
            var cpInfos = new List<CodeGenerateSystem.Base.CustomPropertyInfo>();
            cpInfos.Add(CodeGenerateSystem.Base.CustomPropertyInfo.GetFromParamInfo(typeof(Int32), "ActiveValue", new Attribute[] { new EngineNS.Rtti.MetaDataAttribute() }));

            mTemplateClassInstance = CodeGenerateSystem.Base.PropertyClassGenerator.CreateClassInstanceFromCustomPropertys(cpInfos, this, null, false);

            CreateBinding(mTemplateClassInstance, "ActiveValue", LASelectPoseByIntControl.ActiveValueProperty, ActiveValue);

        }
        void CreateBinding(GeneratorClassBase templateClassInstance, string templateClassPropertyName, DependencyProperty bindedDP, object defaultValue)
        {
            var pro = templateClassInstance.GetType().GetProperty(templateClassPropertyName);
            pro.SetValue(templateClassInstance, defaultValue);
            SetBinding(bindedDP, new Binding(templateClassPropertyName) { Source = templateClassInstance, Mode = BindingMode.TwoWay });
        }
        #endregion

        public LASelectPoseByIntControl(LASelectPoseByIntControlConstructionParams csParam)
            : base(csParam)
        {
            InitConstruction();
            mChildNodeContainer = LinkNodeStackPanel;
            NodeName = csParam.NodeName;
            ActiveValue = csParam.ActiveValue;
            BindingTemplateClassInstanceProperties();
            IsOnlyReturnValue = true;
            InitializeLinkControl(csParam);
        }

        public override void ContainerLoadComplete(NodesContainer container)
        {

        }
        #region InitializeLinkControl
        #region AddDeleteValueLink
        private void ActiveValueLinkHandle_OnDelLinkInfo(LinkInfo linkInfo)
        {
            ActiveValueTextBlock.Visibility = Visibility.Visible;
        }

        private void ActiveValueLinkHandle_OnAddLinkInfo(LinkInfo linkInfo)
        {
            ActiveValueTextBlock.Visibility = Visibility.Collapsed;
        }

        #endregion AddDeleteValueLink
        LinkPinControl mActiveLinkHandle = new CodeGenerateSystem.Base.LinkPinControl();
        //LinkPinControl mTrueLinkHandle = new CodeGenerateSystem.Base.LinkPinControl();
        //LinkPinControl mFalseLinkHandle = new CodeGenerateSystem.Base.LinkPinControl();
        //LinkPinControl mTrueBlendValueLinkHandle = new CodeGenerateSystem.Base.LinkPinControl();
        //LinkPinControl mFalseBlendValueLinkHandle = new CodeGenerateSystem.Base.LinkPinControl();
        LinkPinControl mOutLinkHandle = new CodeGenerateSystem.Base.LinkPinControl();
        void InitializeLinkControl(LASelectPoseByIntControlConstructionParams csParam)
        {
            mActiveLinkHandle = ActiveValueHandle;

            mOutLinkHandle = OutPoseHandle;
            mOutLinkHandle.MultiLink = false;

            mActiveLinkHandle.NameStringVisible = Visibility.Visible;
            mActiveLinkHandle.NameString = "ActiveValue";
            mActiveLinkHandle.OnAddLinkInfo += ActiveValueLinkHandle_OnAddLinkInfo;
            mActiveLinkHandle.OnDelLinkInfo += ActiveValueLinkHandle_OnDelLinkInfo;


            AddLinkPinInfo("ActiveValueHandle", mActiveLinkHandle, null);

            AddLinkPinInfo("OutLinkHandle", mOutLinkHandle, null);
        }
        #endregion InitializeLinkControl
        public static void InitNodePinTypes(CodeGenerateSystem.Base.ConstructionParams smParam)
        {
            CollectLinkPinInfo(smParam, "ActiveValueHandle", CodeGenerateSystem.Base.enLinkType.Int32, CodeGenerateSystem.Base.enBezierType.Left, CodeGenerateSystem.Base.enLinkOpType.End, false);
            CollectLinkPinInfo(smParam, "OutLinkHandle", CodeGenerateSystem.Base.enLinkType.AnimationPose, CodeGenerateSystem.Base.enBezierType.Right, CodeGenerateSystem.Base.enLinkOpType.Start, false);

            CollectLinkPinInfo(smParam, "InPoseLinkHandle", CodeGenerateSystem.Base.enLinkType.AnimationPose, CodeGenerateSystem.Base.enBezierType.Right, CodeGenerateSystem.Base.enLinkOpType.Start, false);
        }
        public override void OnOpenContextMenu(ContextMenu contextMenu)
        {
            var item = new MenuItem()
            {
                Name = "AddPose",
                Header = "AddPose",
                Style = TryFindResource(new ComponentResourceKey(typeof(ResourceLibrary.CustomResources), "MenuItem_Default")) as Style,
            };
            item.Click += (itemSender, itemE) =>
            {
                AddPose();
            };
            contextMenu.Items.Add(item);
            RefreshDeleteMenu(contextMenu);
        }
        public override BaseNodeControl Duplicate(DuplicateParam param)
        {
            var copy = base.Duplicate(param) as LASelectPoseByIntControl;
            copy.ClearChildNode();
            foreach (var child in mChildNodes)
            {
                var copyedChild = child.Duplicate(param);
                copy.AddChildNode(copyedChild, copy.LinkNodeStackPanel);
            }
            return copy;
        }
        void RefreshDeleteMenu(ContextMenu contextMenu)
        {
            for (int i = 0; i < mPins.Count; ++i)
            {
                var item = new MenuItem()
                {
                    Name = "DeletePose",
                    Header = "DeletePose " + mPins[i].IndexValue,
                    Tag = i,
                    Style = TryFindResource(new ComponentResourceKey(typeof(ResourceLibrary.CustomResources), "MenuItem_Default")) as Style,
                };
                item.Click += (itemSender, itemE) =>
                {
                    var menuItem = itemSender as MenuItem;
                    var index = (int)menuItem.Tag;
                    DeletePose(index);
                };
                contextMenu.Items.Add(item);

            }
        }
        List<LASPChildPoseLinkNodeControl> mPins = new List<LASPChildPoseLinkNodeControl>();
        List<float> mBlendTimes = new List<float>();
        List<LASPChildBlendTimeNodeControl> mBlendTimePins = new List<LASPChildBlendTimeNodeControl>();
        void AddPose()
        {
            var childPoseNodeCP = new LASPChildPoseLinkNodeControlConstructionParams()
            {
                IndexValue = mPins.Count.ToString(),
                CSType = CSParam.CSType,
                HostNodesContainer = this.HostNodesContainer,
            };
            var childePoseNode = new LASPChildPoseLinkNodeControl(childPoseNodeCP);
            if (mPins.Count == 0)
                AddChildNode(childePoseNode, LinkNodeStackPanel);
            else
                InsertChildNode(mPins.Count, childePoseNode, LinkNodeStackPanel);

            var childBlendTimeNodeCP = new LASPChildBlendTimeNodeControlConstructionParams()
            {
                IndexValue = mBlendTimePins.Count.ToString(),
                BlendTimeValue = 0.1f,
                CSType = CSParam.CSType,
                HostNodesContainer = this.HostNodesContainer,
            };
            var childeBlendTimeNode = new LASPChildBlendTimeNodeControl(childBlendTimeNodeCP);
            childeBlendTimeNode.Margin = new Thickness(4, 0, 0, 0);
            AddChildNode(childeBlendTimeNode, LinkNodeStackPanel);
        }
        protected override void AddChildNode(BaseNodeControl node, Panel container)
        {
            if (node is LASPChildPoseLinkNodeControl)
            {
                base.AddChildNode(node, container);
                mPins.Add(node as LASPChildPoseLinkNodeControl);
            }
            if (node is LASPChildBlendTimeNodeControl)
            {
                node.Margin = new Thickness(4, 0, 0, 0);
                base.AddChildNode(node, container);
                mBlendTimePins.Add(node as LASPChildBlendTimeNodeControl);
            }
        }
        protected override void InsertChildNode(int index, BaseNodeControl node, Panel container)
        {
            if (node is LASPChildPoseLinkNodeControl)
            {
                mPins.Add(node as LASPChildPoseLinkNodeControl);
                base.InsertChildNode(index, node, container);
            }
        }
        void DeletePose(int index)
        {
            RemoveChildNode(mPins[index]);
            mPins.RemoveAt(index);
            RemoveChildNode(mBlendTimePins[index]);
            mBlendTimePins.RemoveAt(index);
            RefreshIndex();
        }
        void RefreshIndex()
        {
            for (int i = 0; i < mPins.Count; ++i)
            {
                mPins[i].IndexValue = i.ToString();
            }
            for (int i = 0; i < mPins.Count; ++i)
            {
                mBlendTimePins[i].IndexValue = i.ToString();
            }
        }
        public override string GCode_GetTypeString(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            return "LASelectPoseByEnumControl";
        }

        public override Type GCode_GetType(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            return typeof(LASelectPoseByIntControl);
        }
        public string ValidName
        {
            get { return StringRegex.GetValidName(NodeName + "_" + Id.ToString()); }
        }
        public override CodeExpression GCode_CodeDom_GetSelfRefrence(LinkPinControl element, GenerateCodeContext_Method context, GenerateCodeContext_PreNode preNodeContext = null)
        {
            return new CodeVariableReferenceExpression(ValidName);
        }
        public override System.CodeDom.CodeExpression GCode_CodeDom_GetValue(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context, CodeGenerateSystem.Base.GenerateCodeContext_PreNode preNodeContext = null)
        {
            return null;
        }
        public override async System.Threading.Tasks.Task GCode_CodeDom_GenerateCode(CodeTypeDeclaration codeClass, CodeStatementCollection codeStatementCollection, LinkPinControl element, GenerateCodeContext_Method context)
        {
            List<CodeVariableReferenceExpression> mPoseItemList = new List<CodeVariableReferenceExpression>();
            for (int i = 0; i < mPins.Count; ++i)
            {
                var linkObj = mPins[i].ValueIn.GetLinkedObject(0, true);
                var linkElm = mPins[i].ValueIn.GetLinkedPinControl(0, true);
                if (linkObj == null)
                    continue;
                await linkObj.GCode_CodeDom_GenerateCode(codeClass, codeStatementCollection, linkElm, context);
                var item = await CreateBlendTree_PoseItemForBlend(codeClass, codeStatementCollection, mPins[i].ValueIn, ValidName + "_" + mPins[i].IndexValue + "PoseItem", mBlendTimePins[i].ValueIn, mBlendTimePins[i].BlendTimeValue, context);
                mPoseItemList.Add(item);
            }
            Type nodeType = typeof(EngineNS.Bricks.Animation.BlendTree.Node.BlendTree_SelectPoseByInt);
            CodeVariableDeclarationStatement stateVarDeclaration = new CodeVariableDeclarationStatement(nodeType, ValidName, new CodeObjectCreateExpression(new CodeTypeReference(nodeType)));
            codeStatementCollection.Add(stateVarDeclaration);
            for (int i = 0; i < mPoseItemList.Count; ++i)
            {
                var addInvoke = new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(new CodeMethodReferenceExpression(new CodeVariableReferenceExpression(ValidName), "Add"), new CodeExpression[] { new CodePrimitiveExpression(mPins[i].Index), mPoseItemList[i] });
                codeStatementCollection.Add(addInvoke);

            }
            var selectMethod = await CreateSelectedMethod(codeClass, ValidName + "_ActiveSelectValue", typeof(int), ActiveValue, mActiveLinkHandle, context);
            var selectMethodAssign = new CodeAssignStatement();
            selectMethodAssign.Left = new CodeFieldReferenceExpression(new CodeVariableReferenceExpression(ValidName), "EvaluateSelectedFunc");
            selectMethodAssign.Right = new CodeVariableReferenceExpression(selectMethod.Name);
            codeStatementCollection.Add(selectMethodAssign);
            return;
        }
        public async System.Threading.Tasks.Task<CodeVariableReferenceExpression> CreateBlendTree_PoseItemForBlend(CodeTypeDeclaration codeClass, CodeStatementCollection codeStatementCollection, LinkPinControl poseLinkHandle, string itemName, LinkPinControl blendTimeLinkHandle, object defaultValue, GenerateCodeContext_Method context)
        {
            var poseLinkObj = poseLinkHandle.GetLinkedObject(0, true);
            var truePoseItemForBlend = new CodeVariableDeclarationStatement(typeof(EngineNS.Bricks.Animation.BlendTree.Node.BlendTree_PoseItemForBlend), itemName, new CodeObjectCreateExpression(new CodeTypeReference(typeof(EngineNS.Bricks.Animation.BlendTree.Node.BlendTree_PoseItemForBlend))));
            codeStatementCollection.Add(truePoseItemForBlend);
            var truePoseNodeAssign = new CodeAssignStatement();
            truePoseNodeAssign.Left = new CodeFieldReferenceExpression(new CodeVariableReferenceExpression(itemName), "PoseNode");
            truePoseNodeAssign.Right = poseLinkObj.GCode_CodeDom_GetSelfRefrence(null, null);
            codeStatementCollection.Add(truePoseNodeAssign);

            var blendTimeEvaluateMethod = await Helper.CreateEvaluateMethod(codeClass, itemName + "_BlendTimeEvaluateMethod", typeof(float), defaultValue, blendTimeLinkHandle, context);
            var blendTimeAssign = new CodeAssignStatement();
            blendTimeAssign.Left = new CodeFieldReferenceExpression(new CodeVariableReferenceExpression(itemName), "EvaluateBlendTimeFunc");
            blendTimeAssign.Right = new CodeVariableReferenceExpression(blendTimeEvaluateMethod.Name);
            codeStatementCollection.Add(blendTimeAssign);
            return new CodeVariableReferenceExpression(itemName);
        }
        public async System.Threading.Tasks.Task<CodeMemberMethod> CreateSelectedMethod(CodeTypeDeclaration codeClass, string methodName, Type returnType, object defaultValue, LinkPinControl linkHandle, GenerateCodeContext_Method context)
        {
            var valueEvaluateMethod = new CodeMemberMethod();
            valueEvaluateMethod.Name = methodName;
            valueEvaluateMethod.Attributes = MemberAttributes.Public | MemberAttributes.Final;
            valueEvaluateMethod.ReturnType = new CodeTypeReference(returnType);
            var valueEvaluateMethodContex = new GenerateCodeContext_Method(context.ClassContext, valueEvaluateMethod);
            var value = await Helper.GetEvaluateValueExpression(codeClass, valueEvaluateMethodContex, valueEvaluateMethod, linkHandle, defaultValue);
            valueEvaluateMethod.Statements.Add(new CodeMethodReturnStatement(value));
            codeClass.Members.Add(valueEvaluateMethod);

            return valueEvaluateMethod;
        }
    }
}
