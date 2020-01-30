using CodeDomNode.Animation;
using CodeGenerateSystem.Base;
using Macross;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace McLogicAnimationEditor
{

    public class CodeGenerator : Macross.CodeGenerator
    {
        public override async Task GenerateMethods(IMacrossOperationContainer linkCtrl, CodeTypeDeclaration macrossClass, CodeGenerateSystem.Base.GenerateCodeContext_Class codeClassContext)
        {
            await base.GenerateMethods(linkCtrl,macrossClass,codeClassContext);
            Category graphCategory;
            CodeMemberMethod constructLAGraphMethod = null;
            foreach (var member in macrossClass.Members)
            {
                if (member is CodeGenerateSystem.CodeDom.CodeMemberMethod)
                {
                    var method = member as CodeGenerateSystem.CodeDom.CodeMemberMethod;
                    if (method.Name == "ConstructLAGraph")
                        constructLAGraphMethod = method;
                }
            }
            if (constructLAGraphMethod == null)
            {
                constructLAGraphMethod = new CodeGenerateSystem.CodeDom.CodeMemberMethod();
                constructLAGraphMethod.Name = "ConstructLAGraph";
                constructLAGraphMethod.Attributes = MemberAttributes.Override | MemberAttributes.Public;
                macrossClass.Members.Add(constructLAGraphMethod);
            }
            if (linkCtrl.MacrossOpPanel.CategoryDic.TryGetValue(MacrossPanel.LogicAnimationGraphNodeCategoryName, out graphCategory))
            {
                for (int k = 0; k < graphCategory.Items.Count; ++k)
                {
                    var linkNodes = graphCategory.Items[k].Children;
                    var lAStatesBridge = new LAStatesBridge();
                    lAStatesBridge.ConstructLAGraphMethod = constructLAGraphMethod;
                    await GenerateLAStateMachine(graphCategory.Items[k], linkCtrl, macrossClass, codeClassContext, lAStatesBridge);
                    for (int i = 0; i < linkNodes.Count; i++)
                    {
                        var graph = linkNodes[i];
                        await GenerateLAStates(graph, linkCtrl, macrossClass, codeClassContext, lAStatesBridge);
                    }
                    for (int i = 0; i < linkNodes.Count; i++)
                    {
                        var graph = linkNodes[i];
                        await GenerateLATransitions(graph, linkCtrl, macrossClass, codeClassContext, lAStatesBridge);
                    }
                }

            }
            if (linkCtrl.MacrossOpPanel.CategoryDic.TryGetValue(MCLAMacrossPanel.LogicAnimationPostProcessCategoryName, out graphCategory))
            {
                for (int k = 0; k < graphCategory.Items.Count; ++k)
                {
                    var linkNodes = graphCategory.Items[k].Children;
                    var lAStatesBridge = new LAStatesBridge();
                    lAStatesBridge.ConstructLAGraphMethod = constructLAGraphMethod;
                    await GenerateLAPostProcess(graphCategory.Items[k], linkCtrl, macrossClass, codeClassContext, lAStatesBridge);

                }
            }
        }
        #region StateMachine
        async Task GenerateLAStateMachine(CategoryItem item, IMacrossOperationContainer linkCtrl, CodeTypeDeclaration macrossClass, CodeGenerateSystem.Base.GenerateCodeContext_Class codeClassContext, LAStatesBridge lAStatesBridge)
        {
            var constructLAGraphMethod = lAStatesBridge.ConstructLAGraphMethod;
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            lAStatesBridge.LAStateMachineName = item.Name;
            var createLAStateMachineMethodInvoke = new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(new CodeMethodReferenceExpression(new CodeThisReferenceExpression(), "CreateLAStateMachine"), new CodeExpression[] { new CodePrimitiveExpression(lAStatesBridge.LAStateMachineName) });
            CodeVariableDeclarationStatement stateVarDeclaration = new CodeVariableDeclarationStatement(typeof(EngineNS.Bricks.Animation.AnimStateMachine.LogicAnimationStateMachine), lAStatesBridge.LAStateMachineName, createLAStateMachineMethodInvoke);
            var layerTypeAssign = new CodeAssignStatement();
            layerTypeAssign.Left = new CodeFieldReferenceExpression(new CodeVariableReferenceExpression(item.Name), "LayerType");
            var layerProp = item.PropertyShowItem as LAAnimLayerCategoryItemPropertys;
            layerTypeAssign.Right = new CodeFieldReferenceExpression(new CodeTypeReferenceExpression(typeof(EngineNS.Bricks.Animation.AnimStateMachine.AnimLayerType)), layerProp.LayerType.ToString());
            lAStatesBridge.LAStateMachine = new CodeVariableReferenceExpression(item.Name);
            constructLAGraphMethod.Statements.Add(stateVarDeclaration);
            constructLAGraphMethod.Statements.Add(layerTypeAssign);
        }
        string GetCategoryItemNameHierarchical(CategoryItem item ,string name="")
        {
            var temp = name + "_" + item.Name;
            if (item.Parent == null)
                return temp;
            return GetCategoryItemNameHierarchical(item.Parent,temp);
        }
        async Task GenerateLAStates(CategoryItem item, IMacrossOperationContainer linkCtrl, CodeTypeDeclaration macrossClass, CodeGenerateSystem.Base.GenerateCodeContext_Class codeClassContext, LAStatesBridge lAStatesBridge)
        {
            var nodesContainer = await linkCtrl.GetNodesContainer(item, true);
            lAStatesBridge.LAStateNodesContainerDic.Add(item.Name, nodesContainer.NodesControl);
            foreach (var ctrl in nodesContainer.NodesControl.CtrlNodeList)
            {
                ctrl.ReInitForGenericCode();
            }
            foreach (var ctrl in nodesContainer.NodesControl.CtrlNodeList)
            {
                if (ctrl is CodeDomNode.Animation.LAClipNodeControl)
                {
                    var laClipCtrl = ctrl as CodeDomNode.Animation.LAClipNodeControl;
                    await laClipCtrl.GCode_CodeDom_GenerateCode_GenerateLAStates(macrossClass, null, codeClassContext, lAStatesBridge);
                    await laClipCtrl.GCode_CodeDom_GenerateCode_GenerateLAClipCustom(macrossClass, null, codeClassContext, lAStatesBridge);
                    await laClipCtrl.GCode_CodeDom_GenerateCode_GenerateLAClipStateEvent(macrossClass, null, codeClassContext, lAStatesBridge);
                    for (int i = 0; i < laClipCtrl.TransitionNodes.Count; ++i)
                    {
                        var laTranstionCtrl = laClipCtrl.TransitionNodes[i] as LATransitionNodeControl;
                        var linkCtrls = laClipCtrl.TransitionNodes[i].GetLinkPinInfos();
                        for (int j = 0; j < linkCtrls.Length; ++j)
                        {
                            for (int k = 0; k < linkCtrls[j].GetLinkInfosCount(); ++k)
                            {
                                var info = linkCtrls[j].GetLinkInfo(k);
                                var title = GetTransitionName(info);
                                var mcLinkCtrl = linkCtrl as MCLAMacrossLinkControl;
                                var container = await mcLinkCtrl.GetLATransitionGraph(info);
                                for (int m = 0; m < container.CtrlNodeList.Count; ++m)
                                {
                                    if (container.CtrlNodeList[m] is LAFinalTransitionResultControl)
                                    {
                                        lAStatesBridge.LATransitionFinalResultNodesDic.Add(title, container.CtrlNodeList[m]);
                                    }
                                }
                                container = await mcLinkCtrl.GetTransitionExecuteGraph(info);
                                for (int m = 0; m < container.CtrlNodeList.Count; ++m)
                                {
                                    if (container.CtrlNodeList[m] is CodeDomNode.MethodCustom)
                                    {
                                        lAStatesBridge.LATransitionExecuteNodesDic.Add(title, container.CtrlNodeList[m]);
                                    }
                                }
                            }
                        }
                    }
                }
                if (ctrl is CodeDomNode.Animation.LAGraphNodeControl)
                {
                    var laGraphCtrl = ctrl as CodeDomNode.Animation.LAGraphNodeControl;
                    if (laGraphCtrl.IsSelfGraphNode)
                    {
                        var laTranstionLinkCtrl = laGraphCtrl.CtrlValueLinkHandle as LATransitionLinkControl;
                        for (int k = 0; k < laTranstionLinkCtrl.GetLinkInfosCount(); ++k)
                        {
                            var info = laTranstionLinkCtrl.GetLinkInfo(k);
                            var title = GetTransitionName(info);
                            var mcLinkCtrl = linkCtrl as MCLAMacrossLinkControl;
                            var container = await mcLinkCtrl.GetLATransitionGraph(info);
                            for (int m = 0; m < container.CtrlNodeList.Count; ++m)
                            {
                                if (container.CtrlNodeList[m] is LAFinalTransitionResultControl)
                                {
                                    lAStatesBridge.LATransitionFinalResultNodesDic.Add(title, container.CtrlNodeList[m]);
                                }
                            }
                            container = await mcLinkCtrl.GetTransitionExecuteGraph(info);
                            for (int m = 0; m < container.CtrlNodeList.Count; ++m)
                            {
                                if (container.CtrlNodeList[m] is CodeDomNode.MethodCustom)
                                {
                                    lAStatesBridge.LATransitionExecuteNodesDic.Add(title, container.CtrlNodeList[m]);
                                }
                            }
                        }
                    }
                }
            }
            for (int i = 0; i < item.Children.Count; ++i)
            {
                await GenerateLAStates(item.Children[i], linkCtrl, macrossClass, codeClassContext, lAStatesBridge);
            }
        }
        string GetTransitionName(LinkInfo info)
        {
            string from, to;
            if (info.m_linkFromObjectInfo.HostNodeControl is LAGraphNodeControl)
            {
                var gNode = info.m_linkFromObjectInfo.HostNodeControl as LAGraphNodeControl;
                from = gNode.LinkedCategoryItemID.ToString();
            }
            else if (info.m_linkFromObjectInfo.HostNodeControl is LATransitionNodeControl)
            {
                var gNode = info.m_linkFromObjectInfo.HostNodeControl as LATransitionNodeControl;
                from = gNode.LinkedCategoryItemID.ToString() + "_" + info.m_linkFromObjectInfo.HostNodeControl.NodeName;
            }
            else
            {
                var gNode = info.m_linkFromObjectInfo.HostNodeControl as LAClipNodeControl;
                from = gNode.LinkedCategoryItemID.ToString() + "_" + info.m_linkFromObjectInfo.HostNodeControl.NodeName;
                System.Diagnostics.Debug.Assert(false);
            }
            if (info.m_linkToObjectInfo.HostNodeControl is LAGraphNodeControl)
            {
                var gNode = info.m_linkToObjectInfo.HostNodeControl as LAGraphNodeControl;
                to = gNode.LinkedCategoryItemID.ToString();
            }
            else
            {
                var gNode = info.m_linkToObjectInfo.HostNodeControl as LAClipNodeControl;
                to = gNode.LinkedCategoryItemID.ToString() + "_" + info.m_linkToObjectInfo.HostNodeControl.NodeName;
            }
            return from + "__To__ " + to;
        }
        async Task GenerateLATransitions(CategoryItem item, IMacrossOperationContainer linkCtrl, CodeTypeDeclaration macrossClass, CodeGenerateSystem.Base.GenerateCodeContext_Class codeClassContext, LAStatesBridge lAStatesBridge)
        {
            var nodesContainer = await linkCtrl.GetNodesContainer(item, true);
            foreach (var ctrl in nodesContainer.NodesControl.CtrlNodeList)
            {
                ctrl.ReInitForGenericCode();
            }
            foreach (var ctrl in nodesContainer.NodesControl.CtrlNodeList)
            {
                if (ctrl is CodeDomNode.Animation.LAClipNodeControl)
                {
                    var laClipCtrl = ctrl as CodeDomNode.Animation.LAClipNodeControl;
                    await laClipCtrl.GCode_CodeDom_GenerateCode_GenerateLATransitions(macrossClass, null, codeClassContext, lAStatesBridge);
                }
            }
            for (int i = 0; i < item.Children.Count; ++i)
            {
                await GenerateLATransitions(item.Children[i], linkCtrl, macrossClass, codeClassContext, lAStatesBridge);
            }
        }
        #endregion StateMachine
        #region PostProcess
        async Task GenerateLAPostProcess(CategoryItem item, IMacrossOperationContainer linkCtrl, CodeTypeDeclaration macrossClass, CodeGenerateSystem.Base.GenerateCodeContext_Class codeClassContext, LAStatesBridge lAStatesBridge)
        {
            var nodesContainer = await linkCtrl.GetNodesContainer(item, true);
            foreach (var ctrl in nodesContainer.NodesControl.CtrlNodeList)
            {
                ctrl.ReInitForGenericCode();
            }
            var constructLAGraphMethod = lAStatesBridge.ConstructLAGraphMethod;
            foreach (var ctrl in nodesContainer.NodesControl.CtrlNodeList)
            {
                if ((ctrl is CodeDomNode.Animation.LAFinalPoseControl))
                {
                    var laFinalPoseCtrl = ctrl as LAFinalPoseControl;
                    var initMethod = new CodeMemberMethod();
                    initMethod.Name = "InitPostProcess";
                    initMethod.Attributes = MemberAttributes.Public;
                    var param = new CodeParameterDeclarationExpression(new CodeTypeReference(typeof(EngineNS.Bricks.Animation.Pose.CGfxSkeletonPose)), "inPose");
                    initMethod.Parameters.Add(param);
                    macrossClass.Members.Add(initMethod);
                    var methodContext = new GenerateCodeContext_Method(codeClassContext, initMethod);
                    await laFinalPoseCtrl.GCode_CodeDom_GenerateCode_GeneratePostProcessBlendTree(macrossClass, initMethod.Statements, null, methodContext);
                    var initLambaAssign = new CodeAssignStatement();
                    initLambaAssign.Left = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "InitializePostProcessFunc");
                    initLambaAssign.Right = new CodeVariableReferenceExpression(initMethod.Name);
                    constructLAGraphMethod.Statements.Add(initLambaAssign);
                }
            }
        }
        #endregion PostProcess
    }
}
