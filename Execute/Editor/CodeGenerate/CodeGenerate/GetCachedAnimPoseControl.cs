using System;
using System.Collections.Generic;
using System.Text;
using System.CodeDom;
using CodeGenerateSystem.Base;
using System.Threading.Tasks;

namespace CodeGenerateSystem.Controls
{
    public class GetCachedAnimPoseConstructionParams : CodeGenerateSystem.Base.AnimMacrossConstructionParams
    {

    }
    [CodeGenerateSystem.CustomConstructionParamsAttribute(typeof(GetCachedAnimPoseConstructionParams))]
    public partial class GetCachedAnimPoseControl : CodeGenerateSystem.Base.BaseNodeControl
    {
        CodeGenerateSystem.Base.LinkPinControl mCtrlValueLinkHandle = new CodeGenerateSystem.Base.LinkPinControl();
        partial void InitConstruction();
        public GetCachedAnimPoseControl(GetCachedAnimPoseConstructionParams csParam):base(csParam)
        {
            InitConstruction();

            var cpInfos = new List<CodeGenerateSystem.Base.CustomPropertyInfo>();
            cpInfos.Add(CodeGenerateSystem.Base.CustomPropertyInfo.GetFromParamInfo(typeof(string), "Name", new Attribute[] { new EngineNS.Rtti.MetaDataAttribute() }));
            mTemplateClassInstance = CodeGenerateSystem.Base.PropertyClassGenerator.CreateClassInstanceFromCustomPropertys(cpInfos, this, true);

            var clsType = mTemplateClassInstance.GetType();
            var xNamePro = clsType.GetProperty("Name");
            xNamePro.SetValue(mTemplateClassInstance, csParam.NodeName);

            NodeName = csParam.NodeName;
            IsOnlyReturnValue = true;
            AddLinkPinInfo("AnimPoseOutHandle", mCtrlValueLinkHandle, null);
        }
        CodeGenerateSystem.Base.GeneratorClassBase mTemplateClassInstance = null;
        public override object GetShowPropertyObject()
        {
            return mTemplateClassInstance;
        }
        public static void InitNodePinTypes(CodeGenerateSystem.Base.ConstructionParams smParam)
        {
            CollectLinkPinInfo(smParam, "AnimPoseOutHandle", CodeGenerateSystem.Base.enLinkType.AnimationPose, CodeGenerateSystem.Base.enBezierType.Right, CodeGenerateSystem.Base.enLinkOpType.Start, true);
        }

        public override string GCode_GetTypeString(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            return "AnimationPose";
        }

        public override Type GCode_GetType(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            return typeof(GetCachedAnimPoseControl);
        }
        public override CodeExpression GCode_CodeDom_GetSelfRefrence(LinkPinControl element, GenerateCodeContext_Method context, GenerateCodeContext_PreNode preNodeContext = null)
        {
            //var validName = System.Text.RegularExpressions.Regex.Replace(NodeName, "[ \\[ \\] \\^ \\*×――(^)$%~!@#$…&%￥+=<>《》!！??？:：•`·、。，；,.;\"‘’“”-]", "");
            return null;//new CodeMethodInvokeExpression(new CodeMethodReferenceExpression(new CodeThisReferenceExpression(), "GetCachedAnimPose"), new CodePrimitiveExpression(NodeName));
        }
        public override System.CodeDom.CodeExpression GCode_CodeDom_GetValue(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context, CodeGenerateSystem.Base.GenerateCodeContext_PreNode preNodeContext = null)
        {
            return null;//new System.CodeDom.CodeSnippetExpression("System.Math.Sin((System.DateTime.Now.Millisecond*0.001)*2*System.Math.PI)");
        }
        public override async System.Threading.Tasks.Task GCode_CodeDom_GenerateCode(CodeTypeDeclaration codeClass, CodeStatementCollection codeStatementCollection, LinkPinControl element, GenerateCodeContext_Method context)
        {
            if (!mCtrlValueLinkHandle.HasLink)
                return;
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            CodeMethodInvokeExpression getCachedPose = new CodeMethodInvokeExpression(new CodeMethodReferenceExpression(new CodeThisReferenceExpression(), "GetCachedAnimPose"), new CodePrimitiveExpression(NodeName));
            CodeFieldReferenceExpression poseRef = null;
            if (context.AnimAssetTickHostReferenceExpression is CodeThisReferenceExpression)
            {
                poseRef = new CodeFieldReferenceExpression(new CodeFieldReferenceExpression(context.AnimAssetTickHostReferenceExpression, "AnimationPoseProxy"),"Pose");
            }
            else
            {
                poseRef = new CodeFieldReferenceExpression(context.AnimAssetTickHostReferenceExpression, "CachedPose");
            }
            CodeAssignStatement animPoseAssign = new CodeAssignStatement();
            animPoseAssign.Left = poseRef;
            animPoseAssign.Right = new CodeFieldReferenceExpression(getCachedPose, "AnimationPose");
            codeStatementCollection.Add(animPoseAssign);
        }
    }
}
