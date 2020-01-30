using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CodeGenerateSystem.Base;

namespace MaterialEditor.Controls
{
    [EngineNS.Rtti.MetaClass]
    public class ColorControlConstructionParams : NodeControlConstructionBase { }
    /// <summary>
    /// Color.xaml 的交互逻辑
    /// </summary>
    [CodeGenerateSystem.ShowInNodeList("Params/Color", "设置颜色值")]
    [CodeGenerateSystem.CustomConstructionParams(typeof(ColorControlConstructionParams))]
    public partial class ColorControl : BaseNodeControl_ShaderVar
    {
        // 临时类，用于选中后显示参数属性
        CodeGenerateSystem.Base.GeneratorClassBase mTemplateClassInstance = null;

        //protected override string NodeNameCoerceValueCallbackOverride(CodeGenerateSystem.Base.BaseNodeControl d, string baseValue)
        //{
        //    if (ShaderVarInfo != null)
        //    {
        //        if (ShaderVarInfo.Rename(GCode_GetValueName(null)) == false)
        //        {
        //            EditorCommon.MessageBox.Show("名称" + GCode_GetValueName(null) + "已经被使用，请换其他名称");
        //            return NodeName;
        //        }
        //    }
        //    return baseValue;
        //}
        public override bool IsInConstantBuffer
        {
            get => IsGeneric;
            protected set => IsGeneric = value;
        }
        SolidColorBrush mColorBrush = Brushes.White;
        public SolidColorBrush ColorBrush
        {
            get { return mColorBrush; }
            set
            {
                mColorBrush = value;
                OnPropertyChanged("ColorBrush");
            }
        }

        EngineNS.Color mColorObject = EngineNS.Color.White;
        public EngineNS.Color ColorObject
        {
            get => mColorObject;
            set
            {
                mColorObject = value;
                ColorObjectBinder = value;
                OnPropertyChanged("ColorObject");
            }
        }

        public EngineNS.Color ColorObjectBinder
        {
            get { return (EngineNS.Color)GetValue(ColorObjectBinderProperty); }
            set { SetValue(ColorObjectBinderProperty, value); }
        }
        public static readonly DependencyProperty ColorObjectBinderProperty =
            DependencyProperty.Register("ColorObjectBinder", typeof(EngineNS.Color), typeof(ColorControl),
            new FrameworkPropertyMetadata(EngineNS.Color.White, new PropertyChangedCallback(OnColorObjectBinderChanged)));

        public static void OnColorObjectBinderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as ColorControl;

            var newColor = (EngineNS.Color)e.NewValue;
            control.ColorObject = newColor;
            
            {
                control.ColorBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(newColor.A, newColor.R, newColor.G, newColor.B));
                control.ShaderVarInfo.SetValueStr(control.GetValueString());
            }
            control.IsDirty = true;
        }

        string mStrValueType = "float4";
        List<CodeGenerateSystem.Controls.LinkInControl> mInComponentLinks = new List<CodeGenerateSystem.Controls.LinkInControl>();
        List<CodeGenerateSystem.Controls.LinkOutControl> mOutComponentLinks = new List<CodeGenerateSystem.Controls.LinkOutControl>();

        public ColorControl(CodeGenerateSystem.Base.ConstructionParams smParam)
            : base(smParam)
        {
            InitializeComponent();

            ShaderVarInfo.EditorType = "Color";
            //ShaderVarInfo.VarName = GCode_GetValueName(null);

            // 创建用于显示属性的临时类
            var cpInfos = new List<CodeGenerateSystem.Base.CustomPropertyInfo>();
            var cpInfo = CodeGenerateSystem.Base.CustomPropertyInfo.GetFromParamInfo(typeof(EngineNS.Color), "Color", new Attribute[] { new DisplayNameAttribute("颜色"), new EngineNS.Editor.Editor_ColorPicker(), new EngineNS.Rtti.MetaDataAttribute() });
            cpInfos.Add(cpInfo);
            cpInfo = CodeGenerateSystem.Base.CustomPropertyInfo.GetFromParamInfo(typeof(bool), "IsParam", new Attribute[] { new EngineNS.Rtti.MetaDataAttribute() });
            cpInfos.Add(cpInfo);
            mTemplateClassInstance = CodeGenerateSystem.Base.PropertyClassGenerator.CreateClassInstanceFromCustomPropertys(cpInfos, this);
            var classType = mTemplateClassInstance.GetType();
            var property = classType.GetProperty("Color");
            property.SetValue(mTemplateClassInstance, ColorObject);
            BindingOperations.SetBinding(this, ColorControl.ColorObjectBinderProperty, new Binding("Color") { Source = mTemplateClassInstance, Mode=BindingMode.TwoWay });
            BindingOperations.SetBinding(this, BaseNodeControl_ShaderVar.IsGenericBindProperty, new Binding("IsParam") { Source = mTemplateClassInstance, Mode=BindingMode.TwoWay });
            if (HostNodesContainer != null && HostNodesContainer.HostControl != null)
                mTemplateClassInstance.EnableUndoRedo(HostNodesContainer.HostControl.UndoRedoKey, "Color");

            AddLinkPinInfo("ValueIn", ValueIn, null);
            AddLinkPinInfo("ValueOut", ValueOut, ValueOut.BackBrush);
            AddLinkPinInfo("ValueOut_Float3", ValueOut_Float3, ValueOut_Float3.BackBrush);

            mInComponentLinks.Add(ValueInR);
            mInComponentLinks.Add(ValueInG);
            mInComponentLinks.Add(ValueInB);
            mInComponentLinks.Add(ValueInA);
            AddLinkPinInfo("ValueInR", ValueInR, null);
            AddLinkPinInfo("ValueInG", ValueInG, null);
            AddLinkPinInfo("ValueInB", ValueInB, null);
            AddLinkPinInfo("ValueInA", ValueInA, null);
            mOutComponentLinks.Add(ValueOutR);
            mOutComponentLinks.Add(ValueOutG);
            mOutComponentLinks.Add(ValueOutB);
            mOutComponentLinks.Add(ValueOutA);
            AddLinkPinInfo("ValueOutR", ValueOutR, ValueOutR.BackBrush);
            AddLinkPinInfo("ValueOutG", ValueOutG, ValueOutG.BackBrush);
            AddLinkPinInfo("ValueOutB", ValueOutB, ValueOutB.BackBrush);
            AddLinkPinInfo("ValueOutA", ValueOutA, ValueOutA.BackBrush);

            InitializeShaderVarInfo();
        }

        public static void InitNodePinTypes(CodeGenerateSystem.Base.ConstructionParams smParam)
        {
            CollectLinkPinInfo(smParam, "ValueIn", CodeGenerateSystem.Base.enLinkType.Float4, CodeGenerateSystem.Base.enBezierType.Left, CodeGenerateSystem.Base.enLinkOpType.End, false);
            CollectLinkPinInfo(smParam, "ValueOut", CodeGenerateSystem.Base.enLinkType.Float4, CodeGenerateSystem.Base.enBezierType.Right, CodeGenerateSystem.Base.enLinkOpType.Start, true);
            CollectLinkPinInfo(smParam, "ValueOut_Float3", CodeGenerateSystem.Base.enLinkType.Float3, CodeGenerateSystem.Base.enBezierType.Right, CodeGenerateSystem.Base.enLinkOpType.Start, true);
            CollectLinkPinInfo(smParam, "ValueInR", CodeGenerateSystem.Base.enLinkType.Float1, CodeGenerateSystem.Base.enBezierType.Left, CodeGenerateSystem.Base.enLinkOpType.End, false);
            CollectLinkPinInfo(smParam, "ValueInG", CodeGenerateSystem.Base.enLinkType.Float1, CodeGenerateSystem.Base.enBezierType.Left, CodeGenerateSystem.Base.enLinkOpType.End, false);
            CollectLinkPinInfo(smParam, "ValueInB", CodeGenerateSystem.Base.enLinkType.Float1, CodeGenerateSystem.Base.enBezierType.Left, CodeGenerateSystem.Base.enLinkOpType.End, false);
            CollectLinkPinInfo(smParam, "ValueInA", CodeGenerateSystem.Base.enLinkType.Float1, CodeGenerateSystem.Base.enBezierType.Left, CodeGenerateSystem.Base.enLinkOpType.End, false);
            CollectLinkPinInfo(smParam, "ValueOutR", CodeGenerateSystem.Base.enLinkType.Float1, CodeGenerateSystem.Base.enBezierType.Right, CodeGenerateSystem.Base.enLinkOpType.Start, true);
            CollectLinkPinInfo(smParam, "ValueOutG", CodeGenerateSystem.Base.enLinkType.Float1, CodeGenerateSystem.Base.enBezierType.Right, CodeGenerateSystem.Base.enLinkOpType.Start, true);
            CollectLinkPinInfo(smParam, "ValueOutB", CodeGenerateSystem.Base.enLinkType.Float1, CodeGenerateSystem.Base.enBezierType.Right, CodeGenerateSystem.Base.enLinkOpType.Start, true);
            CollectLinkPinInfo(smParam, "ValueOutA", CodeGenerateSystem.Base.enLinkType.Float1, CodeGenerateSystem.Base.enBezierType.Right, CodeGenerateSystem.Base.enLinkOpType.Start, true);
        }

        public override object GetShowPropertyObject()
        {
            return mTemplateClassInstance;
        }
        public override BaseNodeControl Duplicate(DuplicateParam param)
        {
            var copyedNode = base.Duplicate(param) as ColorControl;
            var nodeName = copyedNode.NodeName;
            copyedNode.mTemplateClassInstance.DisableUndoRedo();
            CodeGenerateSystem.Base.PropertyClassGenerator.CloneClassInstanceProperties(mTemplateClassInstance, copyedNode.mTemplateClassInstance);
            copyedNode.ColorObjectBinder = ColorObjectBinder;
            copyedNode.IsGenericBind = IsGenericBind;
            var namePro = copyedNode.mTemplateClassInstance.GetType().GetProperty("Name");
            if (namePro != null)
                namePro.SetValue(copyedNode.mTemplateClassInstance, nodeName);
            if (param.TargetNodesContainer != null)
                copyedNode.mTemplateClassInstance.EnableUndoRedo(param.TargetNodesContainer.HostControl.UndoRedoKey, "Color");
            return copyedNode;
        }
        public override void Save(EngineNS.IO.XndNode xndNode, bool newGuid)
        {
            var att = xndNode.AddAttrib("colorCtrlData");

            att.BeginWrite();
            att.Version = 2;
            CodeGenerateSystem.Base.PropertyClassGenerator.SaveClassInstanceProperties(mTemplateClassInstance, att);
            att.EndWrite();
            base.Save(xndNode, newGuid);
        }
        public override async System.Threading.Tasks.Task Load(EngineNS.IO.XndNode xndNode)
        {
            await base.Load(xndNode);

            var att = xndNode.FindAttrib("colorCtrlData");
            att.BeginRead();
            switch(att.Version)
            {
                case 0:
                    {
                        EngineNS.Color color;
                        att.Read(out color);
                        ColorObject = color;
                        bool geric;
                        att.Read(out geric);
                        mIsGeneric = geric;
                    }
                    break;
                case 1:
                    {
                        // OnPropertyChanged异步了，会导致加载后IsDirty状态设置不正确，这里强制刷一下值再绑定
                        BindingOperations.ClearBinding(this, ColorControl.ColorObjectBinderProperty);
                        BindingOperations.ClearBinding(this, BaseNodeControl_ShaderVar.IsGenericBindProperty);

                        att.ReadMetaObject(mTemplateClassInstance);

                        var type = mTemplateClassInstance.GetType();
                        var pro = type.GetProperty("Color");
                        ColorObjectBinder = (EngineNS.Color)pro.GetValue(mTemplateClassInstance);
                        pro = type.GetProperty("IsParam");
                        IsGenericBind = (bool)pro.GetValue(mTemplateClassInstance);
                        BindingOperations.SetBinding(this, ColorControl.ColorObjectBinderProperty, new Binding("Color") { Source = mTemplateClassInstance, Mode = BindingMode.TwoWay });
                        BindingOperations.SetBinding(this, BaseNodeControl_ShaderVar.IsGenericBindProperty, new Binding("IsParam") { Source = mTemplateClassInstance, Mode = BindingMode.TwoWay });
                    }
                    break;
                case 2:
                    {
                        // OnPropertyChanged异步了，会导致加载后IsDirty状态设置不正确，这里强制刷一下值再绑定
                        BindingOperations.ClearBinding(this, ColorControl.ColorObjectBinderProperty);
                        BindingOperations.ClearBinding(this, BaseNodeControl_ShaderVar.IsGenericBindProperty);

                        CodeGenerateSystem.Base.PropertyClassGenerator.LoadClassInstanceProperties(mTemplateClassInstance, att);

                        var type = mTemplateClassInstance.GetType();
                        var pro = type.GetProperty("Color");
                        ColorObjectBinder = (EngineNS.Color)pro.GetValue(mTemplateClassInstance);
                        pro = type.GetProperty("IsParam");
                        IsGenericBind = (bool)pro.GetValue(mTemplateClassInstance);
                        BindingOperations.SetBinding(this, ColorControl.ColorObjectBinderProperty, new Binding("Color") { Source = mTemplateClassInstance, Mode = BindingMode.TwoWay });
                        BindingOperations.SetBinding(this, BaseNodeControl_ShaderVar.IsGenericBindProperty, new Binding("IsParam") { Source = mTemplateClassInstance, Mode = BindingMode.TwoWay });
                    }
                    break;
            }
            att.EndRead();

            InitializeShaderVarInfo();
        }
        
        protected override void InitializeShaderVarInfo()
        {
            ShaderVarInfo.Initialize(GCode_GetValueName(null, null),
                                     Program.GetShaderVarTypeFromValueType(GCode_GetTypeString(null, null)),
                                     GetValueString());
        }

        public string GetValueString()
        {
            string retStr = (ColorObject.R / 255.0f).ToString() + "," +
                            (ColorObject.G / 255.0f).ToString() + "," +
                            (ColorObject.B / 255.0f).ToString() + "," +
                            (ColorObject.A / 255.0f).ToString();
            return retStr;
        }

        #region 代码生成

        public override string GCode_GetValueName(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            string strValueName = "";
            if (element == null || element == ValueOut || element == ValueIn)
            {
                strValueName = Program.GetValidNodeName(this);
            }
            else if(element == ValueOut_Float3)
            {
                strValueName = Program.GetValidNodeName(this) + ".xyz";
            }
            else if (element == ValueOutR || element == ValueInR)
                strValueName = GCode_GetValueName(null, context) + ".x";
            else if (element == ValueOutG || element == ValueInG)
                strValueName = GCode_GetValueName(null, context) + ".y";
            else if (element == ValueOutB || element == ValueInB)
                strValueName = GCode_GetValueName(null, context) + ".z";
            else if (element == ValueOutA || element == ValueInA)
                strValueName = GCode_GetValueName(null, context) + ".w";

            return strValueName;
        }

        public override string GCode_GetTypeString(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            if (element == null || element == ValueOut || element == ValueIn)
                return mStrValueType;
            else if(element == ValueOut_Float3)
            {
                return "float3";
            }
            else if (mInComponentLinks.Contains(element) ||
                     mOutComponentLinks.Contains(element))
                return "float1";

            return "";
        }

        public override string GetValueDefine()
        {
            if (IsGeneric)
                return mStrValueType + " " + GCode_GetValueName(null, null) + ";\r\n";
            else
                return "";
        }

        public override void GCode_GenerateCode(ref string strDefinitionSegment, ref string strSegment, int nLayer, CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            string strTab = GCode_GetTabString(nLayer);
            if(!IsGeneric)
            {
                var strIdentity = mStrValueType + " " + GCode_GetValueName(null, context);
                var strDefinition = "    " + strIdentity + " = " + mStrValueType + "(" + GetValueString() + ");\r\n";

                if (!strDefinitionSegment.Contains(strDefinition))
                    strDefinitionSegment += strDefinition;
            }

            if(ValueIn.HasLink)
            {
                ValueIn.GetLinkedObject(0, true).GCode_GenerateCode(ref strDefinitionSegment, ref strSegment, nLayer, ValueIn.GetLinkedPinControl(0, true), context);

                var inType = ValueIn.GetLinkedObject(0, true).GCode_GetTypeString(ValueIn.GetLinkedPinControl(0, true), context);
                var rightStr = ValueIn.GetLinkedObject(0, true).GCode_GetValueName(ValueIn.GetLinkedPinControl(0, true), context);
                strSegment += strTab + GCode_GetValueName(null, context) + " = " + rightStr + ";\r\n";
            }

            // 处理xyzw等有链接的情况
            foreach(var link in mInComponentLinks)
            {
                if(link.HasLink)
                {
                    link.GetLinkedObject(0, true).GCode_GenerateCode(ref strDefinitionSegment, ref strSegment, nLayer, link.GetLinkedPinControl(0, true), context);

                    var rightStr = link.GetLinkedObject(0, true).GCode_GetValueName(link.GetLinkedPinControl(0, true), context);
                    var inType = link.GetLinkedObject(0, true).GCode_GetTypeString(link.GetLinkedPinControl(0, true), context);
                    if (inType == "float2" || inType == "float3" || inType == "float4")
                        rightStr += ".x";

                    strSegment += strTab + GCode_GetValueName(link, context) + " = " + rightStr + ";\r\n";
                }
            }
        }

        #endregion
    }
}
