using System;
using System.Collections.Generic;
using System.Text;
using System.CodeDom;
using CodeGenerateSystem.Base;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows;

namespace CodeDomNode.Animation
{
    public class LAAdditiveBlendSpaceControlConstructionParams : CodeGenerateSystem.Base.ConstructionParams
    {
        [EngineNS.Rtti.MetaData]
        public string XName { get; set; } = "XValue";
        [EngineNS.Rtti.MetaData]
        public string YName { get; set; } = "YValue";
        [EngineNS.Rtti.MetaData]
        public string NodeName { get; set; } = "AdditiveBlendSpace";
        [EngineNS.Rtti.MetaData]
        public EngineNS.RName FileName { get; set; } =EngineNS.RName.EmptyName;
        [EngineNS.Rtti.MetaData]
        public bool Is1D { get; set; } = true;
        [EngineNS.Rtti.MetaData]
        public string SyncPlayPercentGrop { get; set; } = "";

        public Action<EngineNS.RName> OnAdded = null;
        public override EditorCommon.CodeGenerateSystem.INodeConstructionParams Duplicate()
        {
            var retVal = base.Duplicate() as LAAdditiveBlendSpaceControlConstructionParams;

            retVal.XName = XName;
            retVal.YName = YName;
            retVal.Is1D = Is1D;
            retVal.NodeName = NodeName;
            retVal.FileName = FileName;
            retVal.SyncPlayPercentGrop = SyncPlayPercentGrop;
            retVal.OnAdded = OnAdded;
            return retVal;
        }
    }
    [CodeGenerateSystem.CustomConstructionParamsAttribute(typeof(LAAdditiveBlendSpaceControlConstructionParams))]
    public partial class LAAdditiveBlendSpaceControl : CodeGenerateSystem.Base.BaseNodeControl
    {
        public Action<EngineNS.RName> OnAdded = null;
        public ImageSource TitleImage
        {
            get { return (ImageSource)GetValue(TitleImageProperty); }
            set { SetValue(TitleImageProperty, value); }
        }
        public static readonly DependencyProperty TitleImageProperty = DependencyProperty.Register("TitleImage", typeof(ImageSource), typeof(LAAdditiveBlendSpaceControl), new FrameworkPropertyMetadata(null));

        CodeGenerateSystem.Base.LinkPinControl mXLinkHandle = new CodeGenerateSystem.Base.LinkPinControl();
        CodeGenerateSystem.Base.LinkPinControl mYLinkHandle = new CodeGenerateSystem.Base.LinkPinControl();
        CodeGenerateSystem.Base.LinkPinControl mOutLinkHandle = new CodeGenerateSystem.Base.LinkPinControl();
        public bool Is1D { get; set; } = true;
        public EngineNS.RName FileName { get; set; } = EngineNS.RName.EmptyName;
        partial void InitConstruction();

        #region DP
        public string SyncPlayPercentGrop
        {
            get { return (string)GetValue(SyncPlayPercentGropProperty); }
            set
            {
                SetValue(SyncPlayPercentGropProperty, value);
                var para = CSParam as LAAdditiveBlendSpaceControlConstructionParams;
                para.SyncPlayPercentGrop = value;
            }
        }
        public static readonly DependencyProperty SyncPlayPercentGropProperty = DependencyProperty.Register("ActiveValue", typeof(string), typeof(LAAdditiveBlendSpaceControl), new UIPropertyMetadata("", OnSyncPlayPercentGropPropertyyChanged));
        private static void OnSyncPlayPercentGropPropertyyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = sender as LAAdditiveBlendSpaceControl;
            if ((e.NewValue == e.OldValue))
                return;
            ctrl.SyncPlayPercentGrop = (string)e.NewValue;
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
            cpInfos.Add(CodeGenerateSystem.Base.CustomPropertyInfo.GetFromParamInfo(typeof(string), "SyncPlayPercentGrop", new Attribute[] { new EngineNS.Rtti.MetaDataAttribute() }));

            mTemplateClassInstance = CodeGenerateSystem.Base.PropertyClassGenerator.CreateClassInstanceFromCustomPropertys(cpInfos, this);


            CreateBinding(mTemplateClassInstance, "SyncPlayPercentGrop", LAAdditiveBlendSpaceControl.SyncPlayPercentGropProperty, SyncPlayPercentGrop);

        }
        void CreateBinding(GeneratorClassBase templateClassInstance, string templateClassPropertyName, DependencyProperty bindedDP, object defaultValue)
        {
            var pro = templateClassInstance.GetType().GetProperty(templateClassPropertyName);
            pro.SetValue(templateClassInstance, defaultValue);
            SetBinding(bindedDP, new Binding(templateClassPropertyName) { Source = templateClassInstance, Mode = BindingMode.TwoWay });
        }
        #endregion

        public LAAdditiveBlendSpaceControl(LAAdditiveBlendSpaceControlConstructionParams csParam)
            : base(csParam)
        {
            InitConstruction();
            mXLinkHandle = XValueHandle;
            mYLinkHandle = YValueHandle;
            mOutLinkHandle = OutPoseHandle;
            mXLinkHandle.MultiLink = false;
            mYLinkHandle.MultiLink = false;
            mOutLinkHandle.MultiLink = false;

            NodeName = csParam.NodeName;
            Is1D = csParam.Is1D;
            FileName = csParam.FileName;
            SyncPlayPercentGrop = csParam.SyncPlayPercentGrop;
            OnAdded = csParam.OnAdded;
            BindingTemplateClassInstanceProperties();
            if(Is1D)
            {
                var bs = EditorCommon.Utility.EditableAnimationOperation.CreateAdditiveBlendSpace1D(FileName);
                csParam.XName = bs.BlendAxises[0].AxisName;
                bs.BlendAxises[0].OnAxisNameChange += LAAdditiveBlendSpaceControl_OnXAxisNameChange;
            }
            else
            {
                var bs = EditorCommon.Utility.EditableAnimationOperation.CreateAdditiveBlendSpace2D(FileName);
                csParam.XName = bs.BlendAxises[0].AxisName;
                csParam.YName = bs.BlendAxises[1].AxisName;
                bs.BlendAxises[0].OnAxisNameChange += LAAdditiveBlendSpaceControl_OnXAxisNameChange;
                bs.BlendAxises[1].OnAxisNameChange += LAAdditiveBlendSpaceControl_OnYAxisNameChange;
            }
             

            IsOnlyReturnValue = true;
            XValueHandle.NameStringVisible = Visibility.Visible;
            XValueHandle.NameString = csParam.XName;
            if (Is1D)
                YValueHandle.Visibility = Visibility.Collapsed;
            else
            {
                YValueHandle.Visibility = Visibility.Visible;
                YValueHandle.NameStringVisible = Visibility.Visible;
                YValueHandle.NameString = csParam.YName;
            }

            AddLinkPinInfo("XLinkHandle", mXLinkHandle, null);
            AddLinkPinInfo("YLinkHandle", YValueHandle, null);
            AddLinkPinInfo("OutLinkHandle", mOutLinkHandle, null);
            if(Is1D)
                TitleImage = TryFindResource("AnimationNode_AimOffsetBlendSpace1D_64x") as ImageSource;
            else
                TitleImage = TryFindResource("AnimationNode_AimOffsetBlendSpace_64x") as ImageSource;


            EngineNS.Bricks.Animation.AnimNode.AdditiveBlendSpace abs = null;
            if (Is1D)
            {
                abs = EngineNS.Bricks.Animation.AnimNode.AdditiveBlendSpace1D.CreateSync(FileName);

            }
            else
            {
                abs = EngineNS.Bricks.Animation.AnimNode.AdditiveBlendSpace2D.CreateSync(FileName);
            }
            if (abs != null)
                OnAdded?.Invoke(EngineNS.RName.GetRName(abs.GetElementProperty(EngineNS.Bricks.Animation.AnimNode.ElementPropertyType.EPT_Skeleton)));
        }

        private void LAAdditiveBlendSpaceControl_OnXAxisNameChange(string newName)
        {
            XValueHandle.NameString = newName;
        }
        private void LAAdditiveBlendSpaceControl_OnYAxisNameChange(string newName)
        {
            YValueHandle.NameString = newName;
        }
        public override void ContainerLoadComplete(NodesContainer container)
        {

        }
        public static void InitNodePinTypes(CodeGenerateSystem.Base.ConstructionParams smParam)
        {
            CollectLinkPinInfo(smParam, "XLinkHandle", CodeGenerateSystem.Base.enLinkType.Single, CodeGenerateSystem.Base.enBezierType.Left, CodeGenerateSystem.Base.enLinkOpType.End, false);
            CollectLinkPinInfo(smParam, "YLinkHandle", CodeGenerateSystem.Base.enLinkType.Single, CodeGenerateSystem.Base.enBezierType.Left, CodeGenerateSystem.Base.enLinkOpType.End, false);
            CollectLinkPinInfo(smParam, "OutLinkHandle", CodeGenerateSystem.Base.enLinkType.AnimationPose, CodeGenerateSystem.Base.enBezierType.Right, CodeGenerateSystem.Base.enLinkOpType.Start, false);
        }

        public override string GCode_GetTypeString(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            return "LAAdditiveBlendSpaceControl";
        }

        public override Type GCode_GetType(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            return typeof(LAAdditiveBlendSpaceControl);
        }
        public override CodeExpression GCode_CodeDom_GetSelfRefrence(LinkPinControl element, GenerateCodeContext_Method context, GenerateCodeContext_PreNode preNodeContext = null)
        {
            return new CodeVariableReferenceExpression(ValidName);
        }
        public override System.CodeDom.CodeExpression GCode_CodeDom_GetValue(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context, CodeGenerateSystem.Base.GenerateCodeContext_PreNode preNodeContext = null)
        {
            return null;
        }
        public string ValidName
        {
            get { return StringRegex.GetValidName(NodeName + "_" + Id.ToString()); }
        }
        public override async System.Threading.Tasks.Task GCode_CodeDom_GenerateCode(CodeTypeDeclaration codeClass, CodeStatementCollection codeStatementCollection, LinkPinControl element, GenerateCodeContext_Method context)
        {
            var valueEvaluateMethod = new CodeMemberMethod();
            valueEvaluateMethod.Name = ValidName + "_InputEvaluate";
            valueEvaluateMethod.Attributes = MemberAttributes.Public | MemberAttributes.Final;
            valueEvaluateMethod.ReturnType = new CodeTypeReference(typeof(EngineNS.Vector3));
            var valueEvaluateMethodContex = new GenerateCodeContext_Method(context.ClassContext, valueEvaluateMethod);
            var tempInputName = "tempInput";
            CodeVariableDeclarationStatement tempInput = new CodeVariableDeclarationStatement(new CodeTypeReference(typeof(EngineNS.Vector3)), tempInputName, new CodeObjectCreateExpression(typeof(EngineNS.Vector3), new CodeExpression[] { new CodePrimitiveExpression(0), new CodePrimitiveExpression(0), new CodePrimitiveExpression(0) }));
            valueEvaluateMethod.Statements.Add(tempInput);

            CodeExpression xValue = null;
            CodeExpression yValue = null;
            xValue = await Helper.GetEvaluateValueExpression(codeClass, valueEvaluateMethodContex, valueEvaluateMethod, mXLinkHandle, 0);
            if (!Is1D)
            {
                yValue =await Helper.GetEvaluateValueExpression(codeClass, valueEvaluateMethodContex, valueEvaluateMethod, mYLinkHandle,0);
            }

            CodeAssignStatement xValueAssign = new CodeAssignStatement();
            xValueAssign.Left = new CodeFieldReferenceExpression(new CodeVariableReferenceExpression(tempInputName), "X");
            xValueAssign.Right = xValue;
            valueEvaluateMethod.Statements.Add(xValueAssign);
            if (!Is1D)
            {
                CodeAssignStatement yValueAssign = new CodeAssignStatement();
                yValueAssign.Left = new CodeFieldReferenceExpression(new CodeVariableReferenceExpression(tempInputName), "Y");
                yValueAssign.Right = yValue;
                valueEvaluateMethod.Statements.Add(yValueAssign);
            }
            valueEvaluateMethod.Statements.Add(new CodeMethodReturnStatement(new CodeVariableReferenceExpression(tempInputName)));
            codeClass.Members.Add(valueEvaluateMethod);


            Type bsType;
            if (Is1D)
            {
                bsType = typeof(EngineNS.Bricks.Animation.BlendTree.Node.BlendTree_AdditiveBlendSpace1D);
            }
            else
            {
                bsType = typeof(EngineNS.Bricks.Animation.BlendTree.Node.BlendTree_AdditiveBlendSpace2D);
            }
            var createLABSMethodInvoke = new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(new CodeMethodReferenceExpression(new CodeTypeReferenceExpression(bsType), "CreateSync"), new CodeExpression[] { new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(new CodeTypeReferenceExpression(typeof(EngineNS.RName)), "GetRName", new CodeExpression[] { new CodePrimitiveExpression(FileName.Name) }) });
            CodeVariableDeclarationStatement stateVarDeclaration = new CodeVariableDeclarationStatement(bsType, ValidName, createLABSMethodInvoke);
            codeStatementCollection.Add(stateVarDeclaration);

            var syncPlayPercentGropAssign = new CodeAssignStatement();
            syncPlayPercentGropAssign.Left = new CodeFieldReferenceExpression(new CodeVariableReferenceExpression(ValidName), "SyncPlayPercentGrop");
            syncPlayPercentGropAssign.Right = new CodePrimitiveExpression(SyncPlayPercentGrop);
            codeStatementCollection.Add(syncPlayPercentGropAssign);

            CodeAssignStatement inputFuncAssign = new CodeAssignStatement();
            inputFuncAssign.Left = new CodeFieldReferenceExpression(new CodeVariableReferenceExpression(ValidName), "EvaluateInput");
            inputFuncAssign.Right = new CodeVariableReferenceExpression(valueEvaluateMethod.Name);
            codeStatementCollection.Add(inputFuncAssign);

            List<string> nofifies = new List<string>();
            var info = await EditorCommon.Resources.ResourceInfoManager.Instance.CreateResourceInfoFromFile(FileName.Address + ".rinfo", null);
            EngineNS.Bricks.Animation.AnimNode.BlendSpace bs = null;
            if (Is1D)
            {
                bs = EngineNS.Bricks.Animation.AnimNode.BlendSpace1D.CreateSync(FileName);

            }
            else
            {
                bs = EngineNS.Bricks.Animation.AnimNode.BlendSpace2D.CreateSync(FileName);
            }
            if (info != null)
            {
                for (int j = 0; j < bs.Samples.Count; ++j)
                {
                    var clip = bs.GetAnimationSample(j);

                    var refInfo = await EditorCommon.Resources.ResourceInfoManager.Instance.CreateResourceInfoFromFile(clip.AnimationName.Address + ".rinfo", null);
                    var animationInfo = refInfo as EditorCommon.ResourceInfos.AnimationClipResourceInfo;
                    nofifies.Clear();
                    foreach (var pair in animationInfo.NotifyTrackMap)
                    {
                        nofifies.Add(pair.NotifyName);
                    }
                    for (int i = 0; i < nofifies.Count; ++i)
                    {
                        var notify = nofifies[i];
                        var validNotifyName = StringRegex.GetValidName(notify);
                        validNotifyName = "Anim_Notify_" + validNotifyName;
                        if (hasTheNotifyMethod(codeClass, validNotifyName))
                        {
                            var attachNotifyEventExp = new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(new CodeMethodReferenceExpression(new CodeFieldReferenceExpression(new CodeVariableReferenceExpression(ValidName), "AdditiveBlendSpace"), "AttachNotifyEvent"), new CodeExpression[] { new CodePrimitiveExpression(j), new CodePrimitiveExpression(i), new CodeMethodReferenceExpression(new CodeThisReferenceExpression(), validNotifyName) });
                            //CodeArrayIndexerExpression arrayIndex = new CodeArrayIndexerExpression(new CodeFieldReferenceExpression(animRef, "Notifies"), new CodePrimitiveExpression(i));
                            //CodeAttachEventStatement attachEvent = new CodeAttachEventStatement(arrayIndex, "OnNotify", new CodeMethodReferenceExpression(new CodeThisReferenceExpression(), validNotifyName));
                            codeStatementCollection.Add(attachNotifyEventExp);
                        }
                    }
                }
            }
            return;
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
