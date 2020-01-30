using CodeGenerateSystem.Base;
using CodeGenerateSystem.Controls;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Input;

namespace CodeDomNode.Animation
{
    
    public class AnimStateMachineConstructionParams : CodeGenerateSystem.Base.AnimMacrossConstructionParams
    {

    }
    [CodeGenerateSystem.CustomConstructionParamsAttribute(typeof(AnimStateMachineConstructionParams))]
    public partial class AnimStateMachineControl : CodeGenerateSystem.Base.BaseNodeControl
    {
        CodeGenerateSystem.Base.LinkPinControl mCtrlValueLinkHandle = new CodeGenerateSystem.Base.LinkPinControl();

        partial void InitConstruction();

        public AnimStateMachineControl(AnimStateMachineConstructionParams csParam)
            : base(csParam)
        {
            InitConstruction();

            var cpInfos = new List<CodeGenerateSystem.Base.CustomPropertyInfo>();
            cpInfos.Add(CodeGenerateSystem.Base.CustomPropertyInfo.GetFromParamInfo(typeof(string), "Name", new Attribute[] { new EngineNS.Rtti.MetaDataAttribute() }));
            mTemplateClassInstance = CodeGenerateSystem.Base.PropertyClassGenerator.CreateClassInstanceFromCustomPropertys(cpInfos, this);

            var clsType = mTemplateClassInstance.GetType();
            var xNamePro = clsType.GetProperty("Name");
            xNamePro.SetValue(mTemplateClassInstance, csParam.NodeName);

            NodeName = csParam.NodeName;
            IsOnlyReturnValue = true;
            AddLinkPinInfo("AnimStateMachineLinkHandle", mCtrlValueLinkHandle, null);

        }

        public override void MouseLeftButtonDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var noUse = OpenGraph();
        }
        CodeGenerateSystem.Base.GeneratorClassBase mTemplateClassInstance = null;
        public override object GetShowPropertyObject()
        {
            return mTemplateClassInstance;
        }
        async System.Threading.Tasks.Task OpenGraph()
        {
            var param = CSParam as AnimStateMachineConstructionParams;
            var assist = this.HostNodesContainer.HostControl as Macross.NodesControlAssist;
            var data = new SubNodesContainerData()
            {
                ID = Id,
                Title = HostNodesContainer.TitleString + "/" + param.NodeName + ":" + this.Id.ToString(),
            };
            mLinkedNodesContainer = await assist.ShowSubNodesContainer(data);
            if (data.IsCreated)
            {
                await InitializeLinkedNodesContainer();
            }
            mLinkedNodesContainer.OnFilterContextMenu = StateMachineControl_FilterContextMenu;
            mLinkedNodesContainer.EndPreviewLineFunc = EndPreviewLine;
        }
        private void StateMachineControl_FilterContextMenu(CodeGenerateSystem.Controls.NodeListContextMenu contextMenu, CodeGenerateSystem.Controls.NodesContainerControl.ContextMenuFilterData filterData)
        {
            var nodesList = contextMenu.GetNodesList();
            nodesList.ClearNodes();
            var stateMachineCP = new CodeDomNode.Animation.AnimStateMachineConstructionParams()
            {
                CSType = HostNodesContainer.CSType,
                ConstructParam = "",
                NodeName = "StateMachine",
            };
            //nodesList.AddNodesFromType(filterData, typeof(CodeDomNode.Animation.AnimStateMachineControl), "AnimationStateMachine", stateMachineCP, "");
            var stateCP = new CodeDomNode.Animation.AnimStateControlConstructionParams()
            {
                CSType = HostNodesContainer.CSType,
                ConstructParam = "",
                NodeName = "State",
            };
            nodesList.AddNodesFromType(filterData, typeof(CodeDomNode.Animation.AnimStateControl), "State", stateCP, "");
        }
        async System.Threading.Tasks.Task InitializeLinkedNodesContainer()
        {
            var param = CSParam as AnimStateMachineConstructionParams;
            var assist = this.HostNodesContainer.HostControl as Macross.NodesControlAssist;

            if (mLinkedNodesContainer == null)
            {
                var data = new SubNodesContainerData()
                {
                    ID = Id,
                    Title = HostNodesContainer.TitleString + "/" + param.NodeName + ":" + this.Id.ToString(),
                };
                mLinkedNodesContainer = await assist.GetSubNodesContainer(data);
                if (!data.IsCreated)
                    return;

            }
            // 读取graph
            var tempFile = assist.HostControl.GetGraphFileName(assist.LinkedCategoryItemName);
            var linkXndHolder = await EngineNS.IO.XndHolder.LoadXND(tempFile, EngineNS.Thread.Async.EAsyncTarget.AsyncEditor);
            bool bLoaded = false;
            if (linkXndHolder != null)
            {
                var linkNode = linkXndHolder.Node.FindNode("SubLinks");
                var idStr = Id.ToString();
                foreach (var node in linkNode.GetNodes())
                {
                    if (node.GetName() == idStr)
                    {
                        await mLinkedNodesContainer.Load(node);
                        bLoaded = true;
                        break;
                    }
                }
            }
            if (bLoaded)
            {

            }
            else
            {
                var csParam = new CodeDomNode.Animation.StateEntryControlConstructionParams()
                {
                    CSType = param.CSType,
                    HostNodesContainer = mLinkedNodesContainer,
                    ConstructParam = "",
                    NodeName = "Entry"
                };
                var node = mLinkedNodesContainer.AddOrigionNode(typeof(CodeDomNode.Animation.StateEntryControl), csParam, 0, 0) as CodeDomNode.Animation.StateEntryControl;
                node.IsDeleteable = false;

                //var retCSParam = new CodeDomNode.ReturnCustom.ReturnCustomConstructParam()
                //{
                //    CSType = param.CSType,
                //    HostNodesContainer = mLinkedNodesContainer,
                //    ConstructParam = "",
                //};
                //var retNode = mLinkedNodesContainer.AddOrigionNode(typeof(CodeDomNode.ReturnCustom), retCSParam, 300, 0) as CodeDomNode.ReturnCustom;
                //retNode.IsDeleteable = false;
                //retNode.ShowProperty = false;
            }

        }
        public static void InitNodePinTypes(CodeGenerateSystem.Base.ConstructionParams smParam)
        {
            CollectLinkPinInfo(smParam, "AnimStateMachineLinkHandle", CodeGenerateSystem.Base.enLinkType.AnimationPose, CodeGenerateSystem.Base.enBezierType.Right, CodeGenerateSystem.Base.enLinkOpType.Start, true);
        }

        public override string GCode_GetTypeString(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            return "AnimationPose";
        }

        public override Type GCode_GetType(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            return typeof(AnimStateMachineControl);
        }
        public override CodeExpression GCode_CodeDom_GetSelfRefrence(LinkPinControl element, GenerateCodeContext_Method context, GenerateCodeContext_PreNode preNodeContext = null)
        {
            var validName = StringRegex.GetValidName(NodeName);
            return new CodeVariableReferenceExpression(validName);
        }
        public override System.CodeDom.CodeExpression GCode_CodeDom_GetValue(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context, CodeGenerateSystem.Base.GenerateCodeContext_PreNode preNodeContext = null)
        {
            return null;
        }
        public override async System.Threading.Tasks.Task GCode_CodeDom_GenerateCode(CodeTypeDeclaration codeClass, CodeStatementCollection codeStatementCollection, LinkPinControl element, GenerateCodeContext_Method context)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            /*
            EngineNS.GamePlay.StateMachine.AnimationStateMachine StateMachine = new EngineNS.GamePlay.StateMachine.AnimationStateMachine("StateMachine");
            StateMachine.AnimationPose = this.AnimationPose;
            this.AddStateMachine(StateMachine);
            */
            var validName = StringRegex.GetValidName(NodeName);
            //System.CodeDom.CodeVariableDeclarationStatement st = new CodeVariableDeclarationStatement(new CodeTypeReference(typeof(EngineNS.GamePlay.StateMachine.AnimationStateMachine)), validName, new CodeObjectCreateExpression(new CodeTypeReference(typeof(EngineNS.GamePlay.StateMachine.AnimationStateMachine)),new CodePrimitiveExpression(validName)));
            System.CodeDom.CodeVariableDeclarationStatement st = new CodeVariableDeclarationStatement(new CodeTypeReference(typeof(EngineNS.Bricks.Animation.AnimStateMachine.LogicAnimationStateMachine)), validName);
            codeStatementCollection.Add(st);

            var createStateMachine = new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(new CodeMethodReferenceExpression(new CodeThisReferenceExpression(), "CreateStateMachine"), new CodePrimitiveExpression(validName));
            CodeVariableReferenceExpression stateMachineRef = new CodeVariableReferenceExpression(validName);
            CodeAssignStatement stateMachineAssign = new CodeAssignStatement();
            stateMachineAssign.Left = stateMachineRef;
            stateMachineAssign.Right = createStateMachine;
            codeStatementCollection.Add(stateMachineAssign);

            CodeFieldReferenceExpression animPoseField = new CodeFieldReferenceExpression(stateMachineRef, "AnimationPoseProxy");
            //CodeAssignStatement animPoseAssign = new CodeAssignStatement();
            //animPoseAssign.Left = animPoseField;
            //animPoseAssign.Right = context.InstanceAnimPoseReferenceExpression;
            //codeStatementCollection.Add(animPoseAssign);

            //CodeMethodReferenceExpression methodRef = new CodeMethodReferenceExpression(new CodeThisReferenceExpression(), "AddTickComponent");
            //var methodInvoke = new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(methodRef, new CodeExpression[] { stateMachineRef });
            //codeStatementCollection.Add(methodInvoke);

            if (mLinkedNodesContainer == null)
                return;
            context.AnimStateMachineReferenceExpression = stateMachineRef;
            context.StateMachineAnimPoseReferenceExpression = animPoseField;
            foreach (var ctrl in mLinkedNodesContainer.CtrlNodeList)
            {
                if ((ctrl is CodeDomNode.MethodOverride) ||
                                (ctrl is CodeDomNode.MethodCustom) || ctrl is FinalAnimPoseControl || ctrl is StateEntryControl)
                {
                    await ctrl.GCode_CodeDom_GenerateCode(codeClass, codeStatementCollection, element, context);
                }
            }
        }
    }
}
