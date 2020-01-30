using CodeGenerateSystem.Base;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Data;

namespace CodeDomNode.Animation
{
    public class LAGraphNodeControlConstructionParams : CodeGenerateSystem.Base.ConstructionParams
    {
        [EngineNS.Rtti.MetaData]
        public string LAGNodeName = "LAGNode";
        [EngineNS.Rtti.MetaData]
        public bool IsSelfGraphNode { get; set; } = false;
        [EngineNS.Rtti.MetaData]
        public Guid LinkedCategoryItemID { get; set; } = Guid.Empty;
        [EngineNS.Rtti.MetaData]
        [EngineNS.IO.Serializer.CustomFieldSerializer(typeof(TransitionCrossfadeSerializer))]
        public Dictionary<Guid, TransitionCrossfade> TransitionCrossfadeDic { get; set; } = new Dictionary<Guid, TransitionCrossfade>();
        private class TransitionCrossfadeSerializer : EngineNS.IO.Serializer.CustomSerializer
        {
            public override object ReadValue(EngineNS.IO.Serializer.IReader pkg)
            {
                Dictionary<Guid, TransitionCrossfade> dict = new Dictionary<Guid, TransitionCrossfade>();
                int count;
                pkg.Read(out count);
                for (int i = 0; i < count; ++i)
                {
                    Guid id;
                    TransitionCrossfade cf = new TransitionCrossfade();
                    pkg.Read(out id);
                    pkg.Read(cf);
                    dict.Add(id, cf);
                }
                return dict;
            }
            public override void WriteValue(object obj, EngineNS.IO.Serializer.IWriter pkg)
            {
                var dict = obj as Dictionary<Guid, TransitionCrossfade>;
                pkg.Write(dict.Count);
                using (var it = dict.GetEnumerator())
                {
                    while (it.MoveNext())
                    {
                        pkg.Write(it.Current.Key);
                        pkg.Write(it.Current.Value);
                    }
                }
            }
        }
        public override EditorCommon.CodeGenerateSystem.INodeConstructionParams Duplicate()
        {
            var retVal = base.Duplicate() as LAGraphNodeControlConstructionParams;
            retVal.IsSelfGraphNode = IsSelfGraphNode;
            retVal.LAGNodeName = LAGNodeName;
            retVal.LinkedCategoryItemID = LinkedCategoryItemID;
            retVal.TransitionCrossfadeDic = TransitionCrossfadeDic;
            return retVal;
        }
    }
    [CodeGenerateSystem.CustomConstructionParamsAttribute(typeof(LAGraphNodeControlConstructionParams))]
    public partial class LAGraphNodeControl : CodeGenerateSystem.Base.BaseNodeControl
    {
        public Dictionary<Guid, TransitionCrossfade> TransitionCrossfadeDic { get; set; } = new Dictionary<Guid, TransitionCrossfade>();
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
        public object GetTransitionCrossfadeShowPropertyObject(Guid id)
        {
            if (!TransitionCrossfadeDic.ContainsKey(id))
            {
                TransitionCrossfadeDic.Add(id, new TransitionCrossfade());

            }
            var csParams = new ConstructionParams();
            csParams.CSType = CSParam.CSType;
            var tfs = new LinkNodeTransitionCrossfadeShow(csParams,this, id);
            return tfs.GetShowPropertyObject();
        }
        public override object GetShowPropertyObject()
        {
            return mTemplateClassInstance;
        }
        public void SetName(string name)
        {
            NodeName = name;
            var param = CSParam as LAGraphNodeControlConstructionParams;
            param.LAGNodeName = name;
        }
        public LAGraphNodeControl(LAGraphNodeControlConstructionParams csParam)
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
            var param = smParam as LAGraphNodeControlConstructionParams;
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
            return typeof(LAClipNodeControl);
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

            //var calcMethod = new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(new CodeSnippetExpression("EngineNS.RName"), "GetRName", new CodePrimitiveExpression(RNameNodeName.Name));
            //CodeAssignStatement nodeNameAssign = new CodeAssignStatement();

            //CodeVariableDeclarationStatement rNameSt = new CodeVariableDeclarationStatement(new CodeTypeReference(typeof(EngineNS.RName)), "animName");
            //nodeNameAssign.Left = new CodeVariableReferenceExpression("animName");
            //nodeNameAssign.Right = calcMethod;

            //codeStatementCollection.Add(rNameSt);
            //codeStatementCollection.Add(nodeNameAssign);

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

            //var info = await EditorCommon.Resources.ResourceInfoManager.Instance.CreateResourceInfoFromFile(RNameNodeName.Address + ".rinfo", null);
            //var animationInfo = info as EditorCommon.ResourceInfos.AnimationSequenceResourceInfo;
            //animCP.Notifies.Clear();
            //foreach (var pair in animationInfo.NotifyTrackMap)
            //{
            //    animCP.Notifies.Add(pair.NotifyName);
            //}
            for (int i = 0; i < Notifies.Count; ++i)
            {
                var notify = Notifies[i];
                var validNotifyName = StringRegex.GetValidName(notify);
                validNotifyName = "Anim_Notify_" + validNotifyName;
                if (hasTheNotifyMethod(codeClass, validNotifyName))
                {
                    var attachNotifyEventExp = new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(new CodeMethodReferenceExpression(animRef, "AttachNotifyEvent"), new CodeExpression[] { new CodePrimitiveExpression(i), new CodeMethodReferenceExpression(new CodeThisReferenceExpression(), validNotifyName) });
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
