using CodeGenerateSystem.Base;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace CodeDomNode.Animation
{
    public class AnimationAssetConstructionParams : CodeGenerateSystem.Base.ConstructionParams
    {
        [EngineNS.Rtti.MetaData]
        public AnimAsset AnimAsset { get; set; }
        [EngineNS.Rtti.MetaData]
        public EngineNS.RName NodeName { get; set; }
        [EngineNS.Rtti.MetaData]
        public List<string> Notifies { get; set; }
        [EngineNS.Rtti.MetaData]
        public bool IsLoop { get; set; } = true;
        public override EditorCommon.CodeGenerateSystem.INodeConstructionParams Duplicate()
        {
            var retVal = base.Duplicate() as AnimationAssetConstructionParams;
            retVal.NodeName = NodeName;
            retVal.Notifies = Notifies;
            retVal.AnimAsset = AnimAsset;
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
        CodeGenerateSystem.Base.GeneratorClassBase mTemplateClassInstance = null;
        public override object GetShowPropertyObject()
        {
            return mTemplateClassInstance;
        }
        public AnimationAsset(AnimationAssetConstructionParams csParam)
            : base(csParam)
        {
            InitConstruction();

            

            if (csParam.AnimAsset == null)
            {
                csParam.AnimAsset = new AnimAsset();
                if (csParam.HostNodesContainer.HostNode is AnimStateControl)
                {
                    csParam.AnimAsset.AnimAssetLocationName = csParam.HostNodesContainer.HostNode.NodeName;
                    csParam.AnimAsset.AnimAssetLocation = AnimAssetLocation.State;
                    csParam.AnimAsset.AnimAssetName = csParam.NodeName.PureName();
                }
                else
                {
                    System.Diagnostics.Debugger.Break();
                }
            }
            var cpInfos = new List<CodeGenerateSystem.Base.CustomPropertyInfo>();
            cpInfos.Add(CodeGenerateSystem.Base.CustomPropertyInfo.GetFromParamInfo(typeof(bool), "IsLoop", new Attribute[] { new EngineNS.Rtti.MetaDataAttribute() }));
            mTemplateClassInstance = CodeGenerateSystem.Base.PropertyClassGenerator.CreateClassInstanceFromCustomPropertys(cpInfos, this);
            var clsType = mTemplateClassInstance.GetType();
            var xNamePro = clsType.GetProperty("IsLoop");
            xNamePro.SetValue(mTemplateClassInstance, csParam.IsLoop);
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
            var validName = StringRegex.GetValidName(NodeName);
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
            var validName = StringRegex.GetValidName(NodeName);
            System.CodeDom.CodeVariableDeclarationStatement st = new CodeVariableDeclarationStatement(new CodeTypeReference(typeof(EngineNS.Bricks.Animation.AnimNode.AnimationClip)), validName, new CodeObjectCreateExpression(new CodeTypeReference(typeof(EngineNS.Bricks.Animation.AnimNode.AnimationClip))));
            codeStatementCollection.Add(st);

            var calcMethod = new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(new CodeSnippetExpression("EngineNS.RName"), "GetRName", new CodePrimitiveExpression(RNameNodeName.Name));
            CodeAssignStatement nodeNameAssign = new CodeAssignStatement();

            CodeVariableDeclarationStatement rNameSt = new CodeVariableDeclarationStatement(new CodeTypeReference(typeof(EngineNS.RName)), "animName");
            nodeNameAssign.Left = new CodeVariableReferenceExpression("animName");
            nodeNameAssign.Right = calcMethod;

            codeStatementCollection.Add(rNameSt);
            codeStatementCollection.Add(nodeNameAssign);

            CodeVariableReferenceExpression animRef = new CodeVariableReferenceExpression(validName);
            CodeMethodReferenceExpression methodRef = new CodeMethodReferenceExpression(animRef, "Init");
            var methodInvoke = new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(methodRef, new CodeExpression[] { new CodeVariableReferenceExpression("animName") });

            codeStatementCollection.Add(methodInvoke);
            var animCP = CSParam as AnimationAssetConstructionParams;
            CodeFieldReferenceExpression isLoopField = new CodeFieldReferenceExpression();
            isLoopField.FieldName = "IsLoop";
            isLoopField.TargetObject = animRef;
            CodeAssignStatement isLoopAssign = new CodeAssignStatement();
            isLoopAssign.Left = isLoopField;
            isLoopAssign.Right = new CodePrimitiveExpression((bool)mTemplateClassInstance.GetType().GetProperty("IsLoop").GetValue(mTemplateClassInstance));
            codeStatementCollection.Add(isLoopAssign);


            CodeFieldReferenceExpression animPoseField = new CodeFieldReferenceExpression();
            animPoseField.FieldName = "AnimationPoseProxy";
            animPoseField.TargetObject = animRef;

            CodeAssignStatement animPoseAssign = new CodeAssignStatement();
            animPoseAssign.Left = animPoseField;
            animPoseAssign.Right = context.AnimAssetAnimPoseProxyReferenceExpression;
            codeStatementCollection.Add(animPoseAssign);

            CodeMethodReferenceExpression addAnimTickComponentRef = new CodeMethodReferenceExpression(context.AnimAssetTickHostReferenceExpression, "AddTickComponent");
            var addAnimTickComponentInvoke = new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(addAnimTickComponentRef, new CodeExpression[] { animRef });
            codeStatementCollection.Add(addAnimTickComponentInvoke);
            
            var info = await EditorCommon.Resources.ResourceInfoManager.Instance.CreateResourceInfoFromFile(RNameNodeName.Address+".rinfo", null);
            var animationInfo = info as EditorCommon.ResourceInfos.AnimationClipResourceInfo;
            animCP.Notifies.Clear();
            foreach (var pair in animationInfo.NotifyTrackMap)
            {
                animCP.Notifies.Add(pair.NotifyName);
            }
            for (int i = 0;i<Notifies.Count;++i)
            {
                var notify = Notifies[i];
               var validNotifyName = StringRegex.GetValidName(notify);
                validNotifyName = "Anim_Notify_" + validNotifyName;
                if (hasTheNotifyMethod(codeClass, validNotifyName))
                {
                    var attachNotifyEventExp = new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(new CodeMethodReferenceExpression(animRef, "AttachNotifyEvent"),new CodeExpression[] { new CodePrimitiveExpression(i), new CodeMethodReferenceExpression(new CodeThisReferenceExpression(), validNotifyName) });
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
                if (member is CodeGenerateSystem.CodeDom.CodeMemberMethod)
                {
                    var method = member as CodeGenerateSystem.CodeDom.CodeMemberMethod;
                    if (method.Name == name)
                        return true;
                }
            }
            return false;
        }
    }
}
