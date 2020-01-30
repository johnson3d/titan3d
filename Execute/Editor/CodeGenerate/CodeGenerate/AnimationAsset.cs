using CodeGenerateSystem.Base;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace CodeGenerateSystem.Controls
{
    public class AnimationAssetConstructionParams : CodeGenerateSystem.Base.ConstructionParams
    {
        [EngineNS.Rtti.MetaData]
        public EngineNS.RName NodeName { get; set; }
        [EngineNS.Rtti.MetaData]
        public List<string> Notifies { get; set; }
        public override EditorCommon.CodeGenerateSystem.INodeConstructionParams Duplicate()
        {
            var retVal = base.Duplicate() as AnimationAssetConstructionParams;
            retVal.NodeName = NodeName;
            retVal.Notifies = Notifies;
            return retVal;
        }
    }
    [CodeGenerateSystem.CustomConstructionParamsAttribute(typeof(AnimationAssetConstructionParams))]
    public partial class AnimationAsset : CodeGenerateSystem.Base.BaseNodeControl
    {
        CodeGenerateSystem.Base.LinkPinControl mCtrlValueLinkHandle = new CodeGenerateSystem.Base.LinkPinControl();
        public EngineNS.RName RNameNodeName;
        public List<string> Notifies = new List<string>();
        partial void InitConstruction();
        public override object GetShowPropertyObject()
        {
            return base.GetShowPropertyObject();
        }
        public AnimationAsset(AnimationAssetConstructionParams csParam)
            : base(csParam)
        {
            InitConstruction();

            RNameNodeName = csParam.NodeName;
            NodeName = csParam.NodeName.PureName();
            Notifies = csParam.Notifies;
            IsOnlyReturnValue = true;
            AddLinkPinInfo("AnimAssetLinkHandle", mCtrlValueLinkHandle, null);
        }

        public static void InitNodePinTypes(CodeGenerateSystem.Base.ConstructionParams smParam)
        {
            CollectLinkPinInfo(smParam, "AnimAssetLinkHandle", CodeGenerateSystem.Base.enLinkType.AnimationPose, CodeGenerateSystem.Base.enBezierType.Right, CodeGenerateSystem.Base.enLinkOpType.Start, true);
        }

        public override string GCode_GetTypeString(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            return "AnimationPose";
        }

        public override Type GCode_GetType(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            return typeof(AnimationAsset);
        }
        public override CodeExpression GCode_CodeDom_GetSelfRefrence(LinkPinControl element, GenerateCodeContext_Method context, GenerateCodeContext_PreNode preNodeContext = null)
        {
            var validName = Regex.Replace(NodeName, "[ \\[ \\] \\^ \\*×――(^)$%~!@#$…&%￥+=<>《》!！??？:：•`·、。，；,.;\"‘’“”-]", "");
            return new CodeVariableReferenceExpression(validName);
        }
        public override System.CodeDom.CodeExpression GCode_CodeDom_GetValue(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context, CodeGenerateSystem.Base.GenerateCodeContext_PreNode preNodeContext = null)
        {
            return new System.CodeDom.CodeSnippetExpression("System.Math.Sin((System.DateTime.Now.Millisecond*0.001)*2*System.Math.PI)");
        }
        public override async System.Threading.Tasks.Task GCode_CodeDom_GenerateCode(CodeTypeDeclaration codeClass, CodeStatementCollection codeStatementCollection, LinkPinControl element, GenerateCodeContext_Method context)
        {
            /*生成代码
           EngineNS.Graphics.Mesh.Animation.CGfxAnimationSequence batmanidle = new EngineNS.Graphics.Mesh.Animation.CGfxAnimationSequence();
            EngineNS.RName animName;
            animName = EngineNS.RName.GetRName("titandemo/character/batman/animation/batman@idle.vanims");
            batmanidle.Init(animName);
            batmanidle.AnimationPose = animPose;
            state.AddTickComponent(batmanidle);
            batmanidle.Notifies[0].OnNotify += this.Anim_Notify_Walk;
            return true;
             */
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            var validName = Regex.Replace(NodeName, "[ \\[ \\] \\^ \\*×――(^)$%~!@#$…&%￥—+=<>《》!！??？:：•`·、。，；,.;\"‘’“”-]", "");
            System.CodeDom.CodeVariableDeclarationStatement st = new CodeVariableDeclarationStatement(new CodeTypeReference(typeof(EngineNS.Graphics.Mesh.Animation.CGfxAnimationSequence)), validName, new CodeObjectCreateExpression(new CodeTypeReference(typeof(EngineNS.Graphics.Mesh.Animation.CGfxAnimationSequence))));
            codeStatementCollection.Add(st);

            var calcMethod = new CodeMethodInvokeExpression(new CodeSnippetExpression("EngineNS.RName"), "GetRName", new CodePrimitiveExpression(RNameNodeName.Name));
            CodeAssignStatement nodeNameAssign = new CodeAssignStatement();

            CodeVariableDeclarationStatement rNameSt = new CodeVariableDeclarationStatement(new CodeTypeReference(typeof(EngineNS.RName)), "animName");
            nodeNameAssign.Left = new CodeVariableReferenceExpression("animName");
            nodeNameAssign.Right = calcMethod;

            codeStatementCollection.Add(rNameSt);
            codeStatementCollection.Add(nodeNameAssign);

            CodeVariableReferenceExpression animRef = new CodeVariableReferenceExpression(validName);
            CodeMethodReferenceExpression methodRef = new CodeMethodReferenceExpression(animRef, "Init");
            CodeMethodInvokeExpression methodInvoke = new CodeMethodInvokeExpression(methodRef, new CodeExpression[] { new CodeVariableReferenceExpression("animName") });

            codeStatementCollection.Add(methodInvoke);

            CodeFieldReferenceExpression animPoseField = new CodeFieldReferenceExpression();
            animPoseField.FieldName = "AnimationPoseProxy";
            animPoseField.TargetObject = animRef;

            CodeAssignStatement animPoseAssign = new CodeAssignStatement();
            animPoseAssign.Left = animPoseField;
            animPoseAssign.Right = context.AnimAssetAnimPoseProxyReferenceExpression;
            codeStatementCollection.Add(animPoseAssign);

            CodeMethodReferenceExpression addAnimTickComponentRef = new CodeMethodReferenceExpression(context.AnimAssetTickHostReferenceExpression, "AddTickComponent");
            CodeMethodInvokeExpression addAnimTickComponentInvoke = new CodeMethodInvokeExpression(addAnimTickComponentRef, new CodeExpression[] { animRef });
            codeStatementCollection.Add(addAnimTickComponentInvoke);
            var animCP = CSParam as CodeGenerateSystem.Controls.AnimationAssetConstructionParams;
            var info = await EditorCommon.Resources.ResourceInfoManager.Instance.CreateResourceInfoFromFile(RNameNodeName.Address+".rinfo", null);
            var animationInfo = info as EditorCommon.ResourceInfos.AnimationSequenceResourceInfo;
            animCP.Notifies.Clear();
            foreach (var pair in animationInfo.NotifyTrackMap)
            {
                animCP.Notifies.Add(pair.NotifyName);
            }
            for (int i = 0;i<Notifies.Count;++i)
            {
                var notify = Notifies[i];
               var validNotifyName = Regex.Replace(notify, "[ \\[ \\] \\^ \\*×――(^)$%~!@#$…&%￥—+=<>《》!！??？:：•`·、。，；,.;\"‘’“”-]", "_");
                validNotifyName = "Anim_Notify_" + validNotifyName;
                if (hasTheNotifyMethod(codeClass, validNotifyName))
                {
                    CodeMethodInvokeExpression attachNotifyEventExp = new CodeMethodInvokeExpression(new CodeMethodReferenceExpression(animRef, "AttachNotifyEvent"),new CodeExpression[] { new CodePrimitiveExpression(i), new CodeMethodReferenceExpression(new CodeThisReferenceExpression(), validNotifyName) });
                    //CodeArrayIndexerExpression arrayIndex = new CodeArrayIndexerExpression(new CodeFieldReferenceExpression(animRef, "Notifies"), new CodePrimitiveExpression(i));
                    //CodeAttachEventStatement attachEvent = new CodeAttachEventStatement(arrayIndex, "OnNotify", new CodeMethodReferenceExpression(new CodeThisReferenceExpression(), validNotifyName));
                    codeStatementCollection.Add(attachNotifyEventExp);
                }
            }
        }

        bool hasTheNotifyMethod(CodeTypeDeclaration codeClass, string name)
        {
            foreach (var member in codeClass.Members)
            {
                 if(member.GetType() == typeof(CodeMemberMethod))
                {
                    var method = member as CodeMemberMethod;
                    if (method.Name == name)
                        return true;
                }
            }
            return false;
        }
    }
}
