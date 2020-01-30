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
    public class LASelectPoseByEnumControlConstructionParams : CodeGenerateSystem.Base.ConstructionParams
    {
        [EngineNS.Rtti.MetaData]
        public string ActiveEnum { get; set; } = "";
        [EngineNS.Rtti.MetaData]
        public string NodeName { get; set; } = "SelectPoseByEnum";

        public override EditorCommon.CodeGenerateSystem.INodeConstructionParams Duplicate()
        {
            var retVal = base.Duplicate() as LASelectPoseByEnumControlConstructionParams;
            retVal.ActiveEnum = ActiveEnum;
            retVal.NodeName = NodeName;
            return retVal;
        }
    }
    [CodeGenerateSystem.CustomConstructionParamsAttribute(typeof(LASelectPoseByEnumControlConstructionParams))]
    public partial class LASelectPoseByEnumControl : CodeGenerateSystem.Base.BaseNodeControl
    {
        partial void InitConstruction();

        #region DP

        public string ActiveEnum
        {
            get { return (string)GetValue(ActiveEnumProperty); }
            set
            {
                SetValue(ActiveEnumProperty, value);
                var para = CSParam as LASelectPoseByEnumControlConstructionParams;
                para.ActiveEnum = value;
            }
        }
        public static readonly DependencyProperty ActiveEnumProperty = DependencyProperty.Register("ActiveEnum", typeof(string), typeof(LASelectPoseByEnumControl), new UIPropertyMetadata("", OnActiveEnumPropertyyChanged));
        private static void OnActiveEnumPropertyyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = sender as LASelectPoseByEnumControl;
            if ((e.NewValue == e.OldValue))
                return;
            ctrl.ActiveEnum = (string)e.NewValue;
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
            //cpInfos.Add(CodeGenerateSystem.Base.CustomPropertyInfo.GetFromParamInfo(typeof(Int32), "ActiveValue", new Attribute[] { new EngineNS.Rtti.MetaDataAttribute() }));

            mTemplateClassInstance = CodeGenerateSystem.Base.PropertyClassGenerator.CreateClassInstanceFromCustomPropertys(cpInfos, this, null, false);

            //CreateBinding(mTemplateClassInstance, "ActiveValue", LASelectPoseByEnumControl.ActiveValueProperty, ActiveValue);

        }
        void CreateBinding(GeneratorClassBase templateClassInstance, string templateClassPropertyName, DependencyProperty bindedDP, object defaultValue)
        {
            var pro = templateClassInstance.GetType().GetProperty(templateClassPropertyName);
            pro.SetValue(templateClassInstance, defaultValue);
            SetBinding(bindedDP, new Binding(templateClassPropertyName) { Source = templateClassInstance, Mode = BindingMode.TwoWay });
        }
        #endregion

        public LASelectPoseByEnumControl(LASelectPoseByEnumControlConstructionParams csParam)
            : base(csParam)
        {
            InitConstruction();
            mChildNodeContainer = LinkNodeStackPanel;
            NodeName = csParam.NodeName;
            //ActiveValue = csParam.ActiveValue;
            BindingTemplateClassInstanceProperties();
            IsOnlyReturnValue = true;
            InitializeLinkControl(csParam);
        }

        public override void ContainerLoadComplete(NodesContainer container)
        {

        }
        Type mEnumType = null;
        #region InitializeLinkControl
        #region AddDeleteValueLink
        private void ActiveValueLinkHandle_OnAddLinkInfo(LinkInfo linkInfo)
        {
            var type = linkInfo.m_linkFromObjectInfo.HostNodeControl.GCode_GetType(linkInfo.m_linkFromObjectInfo, null);
            if (type.IsEnum)
            {
                var names = type.GetEnumNames();
                if (mEnumType != type && mEnumType != null)
                {
                    for (int i = 0; i < mPins.Count; ++i)
                    {
                        DeletePose(i);
                    }
                }
                mEnumType = type;
                ActiveEnum = type.Name;
            }
        }
        private void ActiveValueLinkHandle_OnDelLinkInfo(LinkInfo linkInfo)
        {

        }

        public override bool CanLink(LinkPinControl selfLinkPin, LinkPinControl otherLinkPin)
        {
            if (selfLinkPin == mActiveLinkHandle)
            {
                var type = otherLinkPin.HostNodeControl.GCode_GetType(otherLinkPin, null);
                if (type.IsEnum)
                {
                    return true;
                }
                return false;
            }
            return true;
        }

        #endregion AddDeleteValueLink
        LinkPinControl mActiveLinkHandle = new CodeGenerateSystem.Base.LinkPinControl();
        //LinkPinControl mTrueLinkHandle = new CodeGenerateSystem.Base.LinkPinControl();
        //LinkPinControl mFalseLinkHandle = new CodeGenerateSystem.Base.LinkPinControl();
        //LinkPinControl mTrueBlendValueLinkHandle = new CodeGenerateSystem.Base.LinkPinControl();
        //LinkPinControl mFalseBlendValueLinkHandle = new CodeGenerateSystem.Base.LinkPinControl();
        LinkPinControl mOutLinkHandle = new CodeGenerateSystem.Base.LinkPinControl();
        LASPChildPoseLinkNodeControl mDefaultCtrl = null;
        LASPChildBlendTimeNodeControl mDefaultBlendTimeCtrl = null;
        void InitializeLinkControl(LASelectPoseByEnumControlConstructionParams csParam)
        {
            mActiveLinkHandle = ActiveValueHandle;

            mOutLinkHandle = OutPoseHandle;
            mOutLinkHandle.MultiLink = false;

            mActiveLinkHandle.NameStringVisible = Visibility.Visible;
            mActiveLinkHandle.NameString = "ActiveValue";
            mActiveLinkHandle.OnAddLinkInfo += ActiveValueLinkHandle_OnAddLinkInfo;
            mActiveLinkHandle.OnDelLinkInfo += ActiveValueLinkHandle_OnDelLinkInfo;


            AddLinkPinInfo("EnumActiveValueHandle", mActiveLinkHandle, null);

            AddLinkPinInfo("OutLinkHandle", mOutLinkHandle, null);

            var childPoseNodeCP = new LASPChildPoseLinkNodeControlConstructionParams()
            {
                IndexValue = "Default",
                CSType = CSParam.CSType,
                HostNodesContainer = this.HostNodesContainer,
            };
            mDefaultCtrl = new LASPChildPoseLinkNodeControl(childPoseNodeCP);
            AddChildNode(mDefaultCtrl, LinkNodeStackPanel);

            var childBlendTimeNodeCP = new LASPChildBlendTimeNodeControlConstructionParams()
            {
                IndexValue = "Default",
                BlendTimeValue = 0.1f,
                CSType = CSParam.CSType,
                HostNodesContainer = this.HostNodesContainer,
            };
            mDefaultBlendTimeCtrl = new LASPChildBlendTimeNodeControl(childBlendTimeNodeCP);
            mDefaultBlendTimeCtrl.Margin = new Thickness(4, 0, 0, 0);
            AddChildNode(mDefaultBlendTimeCtrl, LinkNodeStackPanel);
        }
        #endregion InitializeLinkControl
        public static void InitNodePinTypes(CodeGenerateSystem.Base.ConstructionParams smParam)
        {
            CollectLinkPinInfo(smParam, "EnumActiveValueHandle", CodeGenerateSystem.Base.enLinkType.Int32, CodeGenerateSystem.Base.enBezierType.Left, CodeGenerateSystem.Base.enLinkOpType.End, false);
            CollectLinkPinInfo(smParam, "OutLinkHandle", CodeGenerateSystem.Base.enLinkType.AnimationPose, CodeGenerateSystem.Base.enBezierType.Right, CodeGenerateSystem.Base.enLinkOpType.Start, false);
        }
        public override void OnOpenContextMenu(ContextMenu contextMenu)
        {
            RefreshAddMenu(contextMenu);
            RefreshDeleteMenu(contextMenu);
        }
        void RefreshAddMenu(ContextMenu contextMenu)
        {
            if (mEnumType == null)
                return;
            var names = mEnumType.GetEnumNames();
            for (int i = 0; i < names.Length; ++i)
            {
                bool alreadyAdded = false;
                for (int j = 0; j < mPins.Count; ++j)
                {
                    if (names[i] == mPins[j].IndexValue)
                    {
                        alreadyAdded = true;
                        break;
                    }
                }
                if (!alreadyAdded)
                {
                    var item = new MenuItem()
                    {
                        Name = "Add_Pose",
                        Header = "Add" + names[i] + "Pose",
                        Tag = names[i],
                        Style = TryFindResource(new ComponentResourceKey(typeof(ResourceLibrary.CustomResources), "MenuItem_Default")) as Style,
                    };
                    item.Click += (itemSender, itemE) =>
                    {
                        var menuItem = itemSender as MenuItem;
                        var index = (string)menuItem.Tag;
                        AddPose(index);
                    };
                    contextMenu.Items.Add(item);
                }
            }
        }
        void RefreshDeleteMenu(ContextMenu contextMenu)
        {
            for (int i = 0; i < mPins.Count; ++i)
            {
                var item = new MenuItem()
                {
                    Name = "Delete_Pose",
                    Header = "Delete" + mPins[i].IndexValue + "Pose ",
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

        public override BaseNodeControl Duplicate(DuplicateParam param)
        {
            var copy = base.Duplicate(param) as LASelectPoseByEnumControl;
            copy.ClearChildNode();
            foreach (var child in mChildNodes)
            {
                var copyedChild = child.Duplicate(param);
                copy.AddChildNode(copyedChild, copy.LinkNodeStackPanel);
            }
            return copy;
        }
        List<LASPChildPoseLinkNodeControl> mPins = new List<LASPChildPoseLinkNodeControl>();
        List<float> mBlendTimes = new List<float>();
        List<LASPChildBlendTimeNodeControl> mBlendTimePins = new List<LASPChildBlendTimeNodeControl>();
        void AddPose(string name)
        {
            var childPoseNodeCP = new LASPChildPoseLinkNodeControlConstructionParams()
            {
                IndexValue = name,
                CSType = CSParam.CSType,
                HostNodesContainer = this.HostNodesContainer,
            };
            var childePoseNode = new LASPChildPoseLinkNodeControl(childPoseNodeCP);
            InsertChildNode(mPins.Count + 1, childePoseNode, LinkNodeStackPanel);

            var childBlendTimeNodeCP = new LASPChildBlendTimeNodeControlConstructionParams()
            {
                IndexValue = name,
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
                var csParam = node.CSParam as LASPChildPoseLinkNodeControlConstructionParams;
                if (csParam.IndexValue != "Default")
                {
                    mPins.Add(node as LASPChildPoseLinkNodeControl);
                }
                base.AddChildNode(node, container);
            }
            if (node is LASPChildBlendTimeNodeControl)
            {

                node.Margin = new Thickness(4, 0, 0, 0);
                base.AddChildNode(node, container);
                var csParam = node.CSParam as LASPChildBlendTimeNodeControlConstructionParams;
                if (csParam.IndexValue != "Default")
                {
                    mBlendTimePins.Add(node as LASPChildBlendTimeNodeControl);
                }
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
        }

        public override string GCode_GetTypeString(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            return "LASelectPoseByEnumControl";
        }

        public override Type GCode_GetType(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            return typeof(LASelectPoseByEnumControl);
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
            var defaultLinkObj = mDefaultCtrl.ValueIn.GetLinkedObject(0, true);
            var defaultLinkElm = mDefaultCtrl.ValueIn.GetLinkedPinControl(0, true);
            CodeVariableReferenceExpression defaultItem = null;
            if (defaultLinkObj != null)
            {
                await defaultLinkObj.GCode_CodeDom_GenerateCode(codeClass, codeStatementCollection, defaultLinkElm, context);
                defaultItem = await CreateBlendTree_PoseItemForBlend(codeClass, codeStatementCollection, mDefaultCtrl.ValueIn, ValidName + "_" + mDefaultCtrl.IndexValue + "PoseItem", mDefaultBlendTimeCtrl.ValueIn, mDefaultBlendTimeCtrl.BlendTimeValue, context);
            }
            List<CodeVariableReferenceExpression> mPoseItemList = new List<CodeVariableReferenceExpression>();
            for (int i = 0; i < mPins.Count; ++i)
            {
                if (mPins[i] == mDefaultCtrl)
                    continue;
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
            if (defaultItem != null)
            {
                var addDefaultInvoke = new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(new CodeMethodReferenceExpression(new CodeVariableReferenceExpression(ValidName), "Add"), new CodeExpression[] { new CodePrimitiveExpression(-1), defaultItem });
                codeStatementCollection.Add(addDefaultInvoke);
            }
            for (int i = 0; i < mPoseItemList.Count; ++i)
            {
                int index = (int)Enum.Parse(mEnumType, mPins[i].IndexValue);
                var addInvoke = new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(new CodeMethodReferenceExpression(new CodeVariableReferenceExpression(ValidName), "Add"), new CodeExpression[] { new CodePrimitiveExpression(index), mPoseItemList[i] });
                codeStatementCollection.Add(addInvoke);

            }
            var selectMethod = await CreateSelectedMethod(codeClass, ValidName + ValidName + "_ActiveSelectValue", typeof(int), ActiveEnum, mActiveLinkHandle, context);
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

            var blendTimeEvaluateMethod =await Helper.CreateEvaluateMethod(codeClass, itemName + "_BlendTimeEvaluateMethod", typeof(float), defaultValue, blendTimeLinkHandle, context);
            var blendTimeAssign = new CodeAssignStatement();
            blendTimeAssign.Left = new CodeFieldReferenceExpression(new CodeVariableReferenceExpression(itemName), "EvaluateBlendTimeFunc");
            blendTimeAssign.Right = new CodeVariableReferenceExpression(blendTimeEvaluateMethod.Name);
            codeStatementCollection.Add(blendTimeAssign);
            return new CodeVariableReferenceExpression(itemName);
        }
        public async System.Threading.Tasks.Task<CodeMemberMethod> CreateSelectedMethod(CodeTypeDeclaration codeClass, string methodName, Type returnType, string defaultValue, LinkPinControl linkHandle, GenerateCodeContext_Method context)
        {
            var valueEvaluateMethod = new CodeMemberMethod();
            valueEvaluateMethod.Name = methodName;
            valueEvaluateMethod.Attributes = MemberAttributes.Public | MemberAttributes.Final;
            valueEvaluateMethod.ReturnType = new CodeTypeReference(returnType);
            var valueEvaluateMethodContex = new GenerateCodeContext_Method(context.ClassContext, valueEvaluateMethod);
            var value = await Helper.GetEvaluateValueExpression(codeClass, valueEvaluateMethodContex, valueEvaluateMethod, linkHandle, defaultValue);
            var cast = new CodeGenerateSystem.CodeDom.CodeCastExpression(typeof(int),value);
            valueEvaluateMethod.Statements.Add(new CodeMethodReturnStatement(cast));
            codeClass.Members.Add(valueEvaluateMethod);

            return valueEvaluateMethod;
        }
    }
}
