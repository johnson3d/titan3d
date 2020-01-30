using CodeGenerateSystem.Base;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Windows.Media;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace McLogicStateMachineEditor.Controls
{
    public class LFSMTransitionNodeControlConstructionParams : CodeGenerateSystem.Base.ConstructionParams
    {
        [EngineNS.Rtti.MetaData]
        public double Width { get; set; } = 40;
        [EngineNS.Rtti.MetaData]
        public double Height { get; set; } = 9;
        [EngineNS.Rtti.MetaData]
        public Guid LinkedCategoryItemID { get; set; } = Guid.Empty;
       
        public override EditorCommon.CodeGenerateSystem.INodeConstructionParams Duplicate()
        {
            var retVal = base.Duplicate() as LFSMTransitionNodeControlConstructionParams;
            retVal.Width = Width;
            retVal.Height = Height;
            retVal.LinkedCategoryItemID = LinkedCategoryItemID;
            return retVal;
        }
    }

    [CodeGenerateSystem.CustomConstructionParamsAttribute(typeof(LFSMTransitionNodeControlConstructionParams))]
    public partial class LFSMTransitionNodeControl : CodeGenerateSystem.Base.BaseNodeControl
    {
        public Guid LinkedCategoryItemID { get; set; } = Guid.Empty;
        bool mIsSelected = false;
        public bool IsSelected
        {
            get => mIsSelected;
            set
            {
                mIsDeleteable = value;
                if (value == false)
                {
                    BackBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#CC80BAB5"));
                }
                if (value == true)
                {
                    BackBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#CC588C88"));
                }
            }
        }
        public LogicFSMNodeControl HostLogicFSMNodeControl { get; set; } = null;
        public LFSMTransition LFSMTransition { get; set; } = null;
        CodeGenerateSystem.Base.LinkPinControl mCtrlValueLinkHandle = new CodeGenerateSystem.Base.LinkPinControl();
        public CodeGenerateSystem.Base.LinkPinControl CtrlValueLinkHandle
        {
            get => mCtrlValueLinkHandle;
            set => mCtrlValueLinkHandle = value;
        }
        public EngineNS.RName RNameNodeName;
        public List<string> Notifies = new List<string>();
        partial void InitConstruction();

        #region DP
       
       
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
            //cpInfos.Add(CodeGenerateSystem.Base.CustomPropertyInfo.GetFromParamInfo(typeof(float), "Start", new Attribute[] { new EngineNS.Rtti.MetaDataAttribute() }));
            //cpInfos.Add(CodeGenerateSystem.Base.CustomPropertyInfo.GetFromParamInfo(typeof(float), "Duration", new Attribute[] { new EngineNS.Rtti.MetaDataAttribute() }));
            //cpInfos.Add(CodeGenerateSystem.Base.CustomPropertyInfo.GetFromParamInfo(typeof(bool), "TransitionWhenFinish", new Attribute[] { new EngineNS.Rtti.MetaDataAttribute() }));

            mTemplateClassInstance = CodeGenerateSystem.Base.PropertyClassGenerator.CreateClassInstanceFromCustomPropertys(cpInfos, this, null, false);

            //var clsType = mTemplateClassInstance.GetType();
            //var startPro = clsType.GetProperty("Start");
            //startPro.SetValue(mTemplateClassInstance, Start);
            //SetBinding(LFSMTransitionNodeControl.StartProperty, new Binding("Start") { Source = mTemplateClassInstance, Mode = BindingMode.TwoWay });
            //var durationPro = clsType.GetProperty("Duration");
            //durationPro.SetValue(mTemplateClassInstance, Duration);
            //SetBinding(LFSMTransitionNodeControl.DurationProperty, new Binding("Duration") { Source = mTemplateClassInstance, Mode = BindingMode.TwoWay });
            //var transitionWhenFinishPro = clsType.GetProperty("TransitionWhenFinish");
            //transitionWhenFinishPro.SetValue(mTemplateClassInstance, TransitionWhenFinish);
            //SetBinding(LFSMTransitionNodeControl.TransitionWhenFinishProperty, new Binding("TransitionWhenFinish") { Source = mTemplateClassInstance, Mode = BindingMode.TwoWay });

        }
        #endregion
        public LFSMTransitionNodeControl(LFSMTransitionNodeControlConstructionParams csParam)
            : base(csParam)
        {
            InitConstruction();
            LinkedCategoryItemID = csParam.LinkedCategoryItemID;
            Width = csParam.Width;
            BindingTemplateClassInstanceProperties();

            IsOnlyReturnValue = true;
            AddLinkPinInfo("TransitionLinkHandle", mCtrlValueLinkHandle, null);
        }
        public override bool CanDuplicate()
        {
            return false;
        }
        protected override void DragObj_MouseMove(object sender, MouseEventArgs e)
        {
            
        }
        protected override void MenuItem_Click_Del(object sender, RoutedEventArgs e)
        {
            if (!CheckCanDelete())
                return;

            List<UndoRedoData> undoRedoDatas = new List<UndoRedoData>();
            foreach (var lPin in mLinkPinInfoDic_Name.Values)
            {
                for (int i = 0; i < lPin.GetLinkInfosCount(); i++)
                {
                    var lInfo = lPin.GetLinkInfo(i);
                    var data = new UndoRedoData();
                    data.StartObj = lInfo.m_linkFromObjectInfo;
                    data.EndObj = lInfo.m_linkToObjectInfo;
                    undoRedoDatas.Add(data);
                }
            }
            var redoAction = new Action<object>((obj) =>
            {
                HostLogicFSMNodeControl.DeleteTransitionNode(this);
            });
            redoAction.Invoke(null);
            EditorCommon.UndoRedo.UndoRedoManager.Instance.AddCommand(HostNodesContainer.HostControl.UndoRedoKey, null, redoAction, null,
                                            (obj) =>
                                            {
                                                if (m_bMoveable)
                                                {
                                                    if (ParentDrawCanvas != null)
                                                    {
                                                        ParentDrawCanvas.Children.Add(this);
                                                        ParentDrawCanvas.Children.Add(mParentLinkPath);
                                                    }
                                                    HostLogicFSMNodeControl.AddTransitionNode(this);
                                                }
                                                foreach (var data in undoRedoDatas)
                                                {
                                                    var linkInfo = LinkInfo.CreateLinkInfo( enLinkCurveType.BrokenLine, ParentDrawCanvas, data.StartObj, data.EndObj);
                                                }
                                            }, "Delete Node");
            IsDirty = true;
        }

        public static void InitNodePinTypes(CodeGenerateSystem.Base.ConstructionParams smParam)
        {
            CollectLinkPinInfo(smParam, "TransitionLinkHandle", CodeGenerateSystem.Base.enLinkType.LAState, CodeGenerateSystem.Base.enBezierType.Bottom, CodeGenerateSystem.Base.enLinkOpType.Start, true);
        }

        public override string GCode_GetTypeString(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            return "LogicClipNode";
        }

        public override Type GCode_GetType(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            return typeof(LFSMTransitionNodeControl);
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
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();  
        }
    }
}
