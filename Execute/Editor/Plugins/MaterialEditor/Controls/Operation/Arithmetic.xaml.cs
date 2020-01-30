using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using CodeGenerateSystem.Base;

namespace MaterialEditor.Controls.Operation
{
    [EngineNS.Rtti.MetaClass]
    public class ArithmeticConstructionParams : NodeControlConstructionBase { }
    /// <summary>
    /// Arithmetic.xaml 的交互逻辑
    /// </summary>
    [CodeGenerateSystem.CustomConstructionParams(typeof(ArithmeticConstructionParams))]
    public partial class Arithmetic : BaseNodeControl
    {
        //// 临时类，用于选中后显示参数属性
        //CodeGenerateSystem.Base.GeneratorClassBase mTemplateClassInstance = null;
        //public CodeGenerateSystem.Base.GeneratorClassBase TemplateClassInstance
        //{
        //    get { return mTemplateClassInstance; }
        //}

        string m_strValueName1 = "";
        string m_strValueName2 = "";

        public Arithmetic(CodeGenerateSystem.Base.ConstructionParams smParam)
            : base(smParam)
        {
            InitializeComponent();

            //NodeName = "运算(" + P1_Label.Text + " " + smParam.ConstructParam + " " + P2_Label.Text + ")";
            switch (CSParam.ConstructParam)
            {
                case "＋":
                    NodeName = "Add";
                    break;
                case "－":
                    NodeName = "Subtract";
                    break;
                case "×":
                    NodeName = "Multiply";
                    break;
                case "·":   // 点乘
                case "dot":
                    NodeName = "Dot";
                    break;
                case "÷":
                    NodeName = "Divide";
                    break;
                case "cross":
                    NodeName = "Cross";
                    break;
                default:
                    NodeName = CSParam.ConstructParam;
                    break;
            }

            AddLinkPinInfo("Value1", Value1, null);
            AddLinkPinInfo("Value2", Value2, null);
            AddLinkPinInfo("ResultLink", ResultLink, ResultLink.BackBrush);
        }
        public static void InitNodePinTypes(CodeGenerateSystem.Base.ConstructionParams smParam)
        {
            enLinkType linkType = enLinkType.Float1 | enLinkType.Float2 | enLinkType.Float3 | enLinkType.Float4 | enLinkType.Int32 | enLinkType.Float4x4;
            CollectLinkPinInfo(smParam, "Value1", linkType, enBezierType.Left, enLinkOpType.End, false);
            CollectLinkPinInfo(smParam, "Value2", linkType, enBezierType.Left, enLinkOpType.End, false);
            CollectLinkPinInfo(smParam, "ResultLink", linkType, enBezierType.Right, enLinkOpType.Start, true);
        }
        protected override void CollectionErrorMsg()
        {
            if(!Value1.HasLink)
            {
                HasError = true;
                ErrorDescription = "A 未连线";
            }
            if(!Value2.HasLink)
            {
                HasError = true;
                ErrorDescription = "B 未连线";
            }
            //var val1LinkType = GetLinkPinInfo(Value1).GetLinkType(0, true);
            //var val2LinkType = GetLinkPinInfo(Value2).GetLinkType(0, true);

            //switch (m_strParams)
            //{
            //    case "＋":
            //    case "－":
            //    case "·":   // 点乘
            //        if (val1LinkType != val2LinkType)
            //        {
            //            string strMsg = "左参右参类型不一致，左参：" + val1LinkType.ToString() + " 右参：" + val2LinkType.ToString();
            //            AddErrorMsg(new List<FrameworkElement> { Value1, Value2 }, CodeGenerateSystem.Controls.ErrorReportControl.ReportType.Error, strMsg);
            //        }
            //        break;
            //    case "×":   // 叉乘
            //        break;
            //    case "÷":
            //        break;
            //}
            //// 检测两个参数能否进行运算
            //// 叉乘只能是同维度之间
        }

        public override void GCode_GenerateCode(ref string strDefinitionSegment, ref string strSegment, int nLayer, CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            if (Value1.HasLink)
            {
                Value1.GetLinkedObject(0, true).GCode_GenerateCode(ref strDefinitionSegment, ref strSegment, nLayer, element, context);
                m_strValueName1 = Value1.GetLinkedObject(0, true).GCode_GetValueName(Value1.GetLinkedPinControl(0, true), context);
            }

            if (Value2.HasLink)
            {
                Value2.GetLinkedObject(0, true).GCode_GenerateCode(ref strDefinitionSegment, ref strSegment, nLayer, element, context);
                m_strValueName2 = Value2.GetLinkedObject(0, true).GCode_GetValueName(Value2.GetLinkedPinControl(0, true), context);
            }
        }

        public override string GCode_GetValueName(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            switch (CSParam.ConstructParam)
            {
                case "＋":
                    return "(" + m_strValueName1 + " + " + m_strValueName2 + ")";
                case "－":
                    return "(" + m_strValueName1 + " - " + m_strValueName2 + ")";
                case "×": 
                    return "(" + m_strValueName1 + " * " + m_strValueName2 + ")";
                case "·":   // 点乘
                case "dot":
                    return "dot(" + m_strValueName1 + " ," + m_strValueName2 + ")";
                case "÷":
                    return "(" + m_strValueName1 + "/" + m_strValueName2 + ")";
                case "cross":  // 叉乘
                    {
                        if ((Value1.GetLinkType(0, true) == enLinkType.Float2 && Value2.GetLinkType(0, true) == enLinkType.Float2) ||
                            (Value1.GetLinkType(0, true) == enLinkType.Float3 && Value2.GetLinkType(0, true) == enLinkType.Float3) ||
                            (Value1.GetLinkType(0, true) == enLinkType.Float4 && Value2.GetLinkType(0, true) == enLinkType.Float4))
                            return "cross(" + m_strValueName1 + ", " + m_strValueName2 + ")";

                        return "(" + m_strValueName1 + " * " + m_strValueName2 + ")";
                    }
            }

            return base.GCode_GetValueName(element, context);
        }

        public override string GCode_GetTypeString(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            if (element == null || element == ResultLink)
            {
                if (Value1.HasLink)
                    return Value1.GetLinkedObject(0, true).GCode_GetTypeString(Value1.GetLinkedPinControl(0, true), context);
            }

            return base.GCode_GetTypeString(element, context);
        }
    }
}
