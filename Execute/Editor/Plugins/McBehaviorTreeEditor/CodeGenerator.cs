using CodeDomNode.AI;
using CodeDomNode.Animation;
using CodeGenerateSystem.Base;
using Macross;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace McBehaviorTreeEditor
{

    public class CodeGenerator : Macross.CodeGenerator
    {
        public override async Task GenerateMethods(IMacrossOperationContainer linkCtrl, CodeTypeDeclaration macrossClass, CodeGenerateSystem.Base.GenerateCodeContext_Class codeClassContext)
        {
            await base.GenerateMethods(linkCtrl, macrossClass, codeClassContext);
            Category graphCategory;
            CodeGenerateSystem.CodeDom.CodeMemberMethod constructLAGraphMethod = null;
            foreach (var member in macrossClass.Members)
            {
                if (member is CodeGenerateSystem.CodeDom.CodeMemberMethod)
                {
                    var method = member as CodeGenerateSystem.CodeDom.CodeMemberMethod;
                    if (method.Name == "ConstructBTGraph")
                        constructLAGraphMethod = method;
                }
            }
            if (constructLAGraphMethod == null)
            {
                constructLAGraphMethod = new CodeGenerateSystem.CodeDom.CodeMemberMethod();
                constructLAGraphMethod.Name = "ConstructBTGraph";
                constructLAGraphMethod.Attributes = MemberAttributes.Override | MemberAttributes.Public;
                macrossClass.Members.Add(constructLAGraphMethod);
            }
            if (linkCtrl.MacrossOpPanel.CategoryDic.TryGetValue(McBTMacrossPanel.BehaviorTreeCategoryName, out graphCategory))
            {
                for (int k = 0; k < graphCategory.Items.Count; ++k)
                {
                    var linkNodes = graphCategory.Items[k].Children;
                    var btBridge = new BehaviorTreeBridge();
                    btBridge.ConstructBTGraphMethod = constructLAGraphMethod;
                    await GenerateBehaviorTree(graphCategory.Items[k], linkCtrl, macrossClass, codeClassContext, btBridge);

                }
            }
        }

        async Task GenerateBehaviorTree(CategoryItem item, IMacrossOperationContainer linkCtrl, CodeTypeDeclaration macrossClass, CodeGenerateSystem.Base.GenerateCodeContext_Class codeClassContext, BehaviorTreeBridge behaviorTreeBridge)
        {
            var btLinkCtrl = linkCtrl as McBTMacrossLinkControl;
            var nodesContainer = await linkCtrl.GetNodesContainer(item, true, true);
            foreach (var ctrl in nodesContainer.NodesControl.CtrlNodeList)
            {
                ctrl.ReInitForGenericCode();
                if (ctrl is BehaviorTree_BTCenterDataControl)
                {
                    var cc = ctrl as BehaviorTree_BTCenterDataControl;
                    cc.BTCenterDataWarpper.CenterDataName = btLinkCtrl.BTCenterDataWarpper.CenterDataName;
                }
            }
            var constructBTGraphMethod = behaviorTreeBridge.ConstructBTGraphMethod;
            foreach (var ctrl in nodesContainer.NodesControl.CtrlNodeList)
            {
                if ((ctrl is BehaviorTree_RootControl))
                {
                    var btRootCtrl = ctrl as BehaviorTree_RootControl;
                    var initMethod = new CodeMemberMethod();  
                    initMethod.Name = "InitBehaviorTree";
                    initMethod.Attributes = MemberAttributes.Public;
                    //var param = new CodeParameterDeclarationExpression(new CodeTypeReference(typeof(EngineNS.Bricks.Animation.Pose.CGfxSkeletonPose)), "inPose");
                    //initMethod.Parameters.Add(param);
                    macrossClass.Members.Add(initMethod);
                    var methodContext = new GenerateCodeContext_Method(codeClassContext, initMethod);
                    await btRootCtrl.GCode_CodeDom_GenerateCode_GenerateBehaviorTree(macrossClass, initMethod.Statements, null, methodContext);
                    var initLambaAssign = new CodeAssignStatement();
                    initLambaAssign.Left = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "InitBehaviorTreeFunc");
                    initLambaAssign.Right = new CodeVariableReferenceExpression(initMethod.Name);
                    constructBTGraphMethod.Statements.Add(initLambaAssign);
                    break;
                }
            }
        }
    }
}
