using CodeDomNode.Animation;
using CodeGenerateSystem.Base;
using Macross;
using McLogicStateMachineEditor.Controls;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace McLogicStateMachineEditor
{
    public class CodeGenerator : Macross.CodeGenerator
    {
        public override async Task GenerateMethods(IMacrossOperationContainer linkCtrl, CodeTypeDeclaration macrossClass, CodeGenerateSystem.Base.GenerateCodeContext_Class codeClassContext)
        {
            await base.GenerateMethods(linkCtrl, macrossClass, codeClassContext);
            Category graphCategory;
            CodeMemberMethod constructGraphMethod = null;
            foreach (var member in macrossClass.Members)
            {
                if (member is CodeGenerateSystem.CodeDom.CodeMemberMethod)
                {
                    var method = member as CodeGenerateSystem.CodeDom.CodeMemberMethod;
                    if (method.Name == "ConstructLFSMGraph")
                        constructGraphMethod = method;
                }
            }
            if (constructGraphMethod == null)
            {
                constructGraphMethod = new CodeGenerateSystem.CodeDom.CodeMemberMethod();
                constructGraphMethod.Name = "ConstructLFSMGraph";
                constructGraphMethod.Attributes = MemberAttributes.Override | MemberAttributes.Public;
                macrossClass.Members.Add(constructGraphMethod);
            }
            var statesBridge = new LFSMStatesBridge();
            statesBridge.ConstructLFSMGraphMethod = constructGraphMethod;
            await GenerateLAStateMachine(linkCtrl, macrossClass, codeClassContext, statesBridge);
            if (linkCtrl.MacrossOpPanel.CategoryDic.TryGetValue(McLogicFSMMacrossPanel.LogicStateMachineCategoryName, out graphCategory))
            {
                var linkNodes = graphCategory.Items;
                for (int i = 0; i < linkNodes.Count; i++)
                {
                    var graph = linkNodes[i];
                    await GenerateLAStates(graph, linkCtrl, macrossClass, codeClassContext, statesBridge);
                }
                for (int i = 0; i < linkNodes.Count; i++)
                {
                    var graph = linkNodes[i];
                    await GenerateLATransitions(graph, linkCtrl, macrossClass, codeClassContext, statesBridge);
                }
            }
        }
        #region StateMachine
        async Task GenerateLAStateMachine(IMacrossOperationContainer linkCtrl, CodeTypeDeclaration macrossClass, CodeGenerateSystem.Base.GenerateCodeContext_Class codeClassContext, LFSMStatesBridge lFSMStatesBridge)
        {
            var constructLAGraphMethod = lFSMStatesBridge.ConstructLFSMGraphMethod;
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            lFSMStatesBridge.LFSMStateMachineName = "LFSM_StateMachine";
            var createStateMachineMethodInvoke = new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(new CodeMethodReferenceExpression(new CodeThisReferenceExpression(), "CrteateStateMachine"), new CodeExpression[] { new CodePrimitiveExpression(lFSMStatesBridge.LFSMStateMachineName) });
            CodeVariableDeclarationStatement stateVarDeclaration = new CodeVariableDeclarationStatement(typeof(EngineNS.Bricks.FSM.SFSM.StackBasedFiniteStateMachine), lFSMStatesBridge.LFSMStateMachineName, createStateMachineMethodInvoke);
            constructLAGraphMethod.Statements.Add(stateVarDeclaration);
        }
        string GetCategoryItemNameHierarchical(CategoryItem item, string name = "")
        {
            var temp = name + "_" + item.Name;
            if (item.Parent == null)
                return temp;
            return GetCategoryItemNameHierarchical(item.Parent, temp);
        }
        async Task GenerateLAStates(CategoryItem item, IMacrossOperationContainer linkCtrl, CodeTypeDeclaration macrossClass, CodeGenerateSystem.Base.GenerateCodeContext_Class codeClassContext, LFSMStatesBridge lFSMStatesBridge)
        {
            var nodesContainer = await linkCtrl.GetNodesContainer(item, true);
            lFSMStatesBridge.LFSMStateNodesContainerDic.Add(item.Name, nodesContainer.NodesControl);
            foreach (var ctrl in nodesContainer.NodesControl.CtrlNodeList)
            {
                ctrl.ReInitForGenericCode();
            }
            foreach (var ctrl in nodesContainer.NodesControl.CtrlNodeList)
            {
                if (ctrl is LogicFSMNodeControl)
                {
                    var laClipCtrl = ctrl as LogicFSMNodeControl;
                    await laClipCtrl.GCode_CodeDom_GenerateCode_GenerateLAStates(macrossClass, null, codeClassContext, lFSMStatesBridge);
                    //await laClipCtrl.GCode_CodeDom_GenerateCode_GenerateLogicGraphCustom(macrossClass, null, codeClassContext, lFSMStatesBridge);
                    await laClipCtrl.GCode_CodeDom_GenerateCode_GenerateLAClipStateEvent(macrossClass, null, codeClassContext, lFSMStatesBridge);
                    for (int i = 0; i < laClipCtrl.TransitionNodes.Count; ++i)
                    {
                        var laTranstionCtrl = laClipCtrl.TransitionNodes[i] as LFSMTransitionNodeControl;
                        var linkCtrls = laClipCtrl.TransitionNodes[i].GetLinkPinInfos();
                        for (int j = 0; j < linkCtrls.Length; ++j)
                        {
                            for (int k = 0; k < linkCtrls[j].GetLinkInfosCount(); ++k)
                            {
                                var info = linkCtrls[j].GetLinkInfo(k);
                                var title = GetTransitionName(info);
                                var mcLinkCtrl = linkCtrl as McLogicFSMLinkControl;
                                var container = await mcLinkCtrl.GetLATransitionGraph(info);
                                for (int m = 0; m < container.CtrlNodeList.Count; ++m)
                                {
                                    if (container.CtrlNodeList[m] is LFSMFinalTransitionResultControl)
                                    {
                                        lFSMStatesBridge.LFSMTransitionFinalResultNodesDic.Add(title, container.CtrlNodeList[m]);
                                    }
                                }
                                container = await mcLinkCtrl.GetTransitionExecuteGraph(info);
                                for (int m = 0; m < container.CtrlNodeList.Count; ++m)
                                {
                                    if (container.CtrlNodeList[m] is CodeDomNode.MethodCustom)
                                    {
                                        lFSMStatesBridge.LFSMTransitionExecuteNodesDic.Add(title, container.CtrlNodeList[m]);
                                    }
                                }
                            }
                        }
                    }
                }
                if (ctrl is LogicFSMGraphNodeControl)
                {
                    var laGraphCtrl = ctrl as LogicFSMGraphNodeControl;
                    if (laGraphCtrl.IsSelfGraphNode)
                    {
                        var laTranstionLinkCtrl = laGraphCtrl.CtrlValueLinkHandle as LFSMTransitionLinkControl;
                        for (int k = 0; k < laTranstionLinkCtrl.GetLinkInfosCount(); ++k)
                        {
                            var info = laTranstionLinkCtrl.GetLinkInfo(k);
                            var title = GetTransitionName(info);
                            var mcLinkCtrl = linkCtrl as McLogicFSMLinkControl;
                            var container = await mcLinkCtrl.GetLATransitionGraph(info);
                            for (int m = 0; m < container.CtrlNodeList.Count; ++m)
                            {
                                if (container.CtrlNodeList[m] is LFSMFinalTransitionResultControl)
                                {
                                    lFSMStatesBridge.LFSMTransitionFinalResultNodesDic.Add(title, container.CtrlNodeList[m]);
                                }
                            }
                            container = await mcLinkCtrl.GetTransitionExecuteGraph(info);
                            for (int m = 0; m < container.CtrlNodeList.Count; ++m)
                            {
                                if (container.CtrlNodeList[m] is CodeDomNode.MethodCustom)
                                {
                                    lFSMStatesBridge.LFSMTransitionExecuteNodesDic.Add(title, container.CtrlNodeList[m]);
                                }
                            }
                        }
                    }
                }
            }
            for (int i = 0; i < item.Children.Count; ++i)
            {
                await GenerateLAStates(item.Children[i], linkCtrl, macrossClass, codeClassContext, lFSMStatesBridge);
            }
        }
        string GetTransitionName(LinkInfo info)
        {
            string from, to;
            if (info.m_linkFromObjectInfo.HostNodeControl is LogicFSMGraphNodeControl)
            {
                var gNode = info.m_linkFromObjectInfo.HostNodeControl as LogicFSMGraphNodeControl;
                from = gNode.LinkedCategoryItemID.ToString();
            }
            else if (info.m_linkFromObjectInfo.HostNodeControl is LFSMTransitionNodeControl)
            {
                var gNode = info.m_linkFromObjectInfo.HostNodeControl as LFSMTransitionNodeControl;
                from = gNode.LinkedCategoryItemID.ToString() + "_" + info.m_linkFromObjectInfo.HostNodeControl.NodeName;
            }
            else
            {
                var gNode = info.m_linkFromObjectInfo.HostNodeControl as LogicFSMNodeControl;
                from = gNode.LinkedCategoryItemID.ToString() + "_" + info.m_linkFromObjectInfo.HostNodeControl.NodeName;
                System.Diagnostics.Debug.Assert(false);
            }
            if (info.m_linkToObjectInfo.HostNodeControl is LogicFSMGraphNodeControl)
            {
                var gNode = info.m_linkToObjectInfo.HostNodeControl as LogicFSMGraphNodeControl;
                to = gNode.LinkedCategoryItemID.ToString();
            }
            else
            {
                var gNode = info.m_linkToObjectInfo.HostNodeControl as LogicFSMNodeControl;
                to = gNode.LinkedCategoryItemID.ToString() + "_" + info.m_linkToObjectInfo.HostNodeControl.NodeName;
            }
            return from + "__To__ " + to;
        }
        async Task GenerateLATransitions(CategoryItem item, IMacrossOperationContainer linkCtrl, CodeTypeDeclaration macrossClass, CodeGenerateSystem.Base.GenerateCodeContext_Class codeClassContext, LFSMStatesBridge lFSMStatesBridge)
        {
            var nodesContainer = await linkCtrl.GetNodesContainer(item, true);
            foreach (var ctrl in nodesContainer.NodesControl.CtrlNodeList)
            {
                ctrl.ReInitForGenericCode();
            }
            foreach (var ctrl in nodesContainer.NodesControl.CtrlNodeList)
            {
                if (ctrl is LogicFSMNodeControl)
                {
                    var laClipCtrl = ctrl as LogicFSMNodeControl;
                    await laClipCtrl.GCode_CodeDom_GenerateCode_GenerateLATransitions(macrossClass, null, codeClassContext, lFSMStatesBridge);
                }
            }
            for (int i = 0; i < item.Children.Count; ++i)
            {
                await GenerateLATransitions(item.Children[i], linkCtrl, macrossClass, codeClassContext, lFSMStatesBridge);
            }
        }
        #endregion StateMachine
    }
}
