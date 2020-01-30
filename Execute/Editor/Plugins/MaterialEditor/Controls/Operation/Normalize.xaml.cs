using System;
using System.Windows;
using System.Windows.Controls;
using CodeGenerateSystem.Base;

namespace MaterialEditor.Controls.Operation
{
    [EngineNS.Rtti.MetaClass]
    public class NormalizeConstructionParams : NodeControlConstructionBase { }
    /// <summary>
    /// Normalize.xaml 的交互逻辑
    /// </summary>
    [CodeGenerateSystem.ShowInNodeList("Math/Normalize", "向量归一化运算")]
    [CodeGenerateSystem.CustomConstructionParams(typeof(NormalizeConstructionParams))]
    public partial class Normalize : BaseNodeControl
    {
        public Normalize(CodeGenerateSystem.Base.ConstructionParams smParam)
            :base(smParam)
        {
            InitializeComponent();

            AddLinkPinInfo("InputLink", InputLink, null);
            AddLinkPinInfo("ResultLink", ResultLink, ResultLink.BackBrush);
        }

        public static void InitNodePinTypes(CodeGenerateSystem.Base.ConstructionParams smParam)
        {
            enLinkType linkType = enLinkType.Float2 | enLinkType.Float3 | enLinkType.Float4;
            CollectLinkPinInfo(smParam, "InputLink", linkType, enBezierType.Left, enLinkOpType.End, false);
            CollectLinkPinInfo(smParam, "ResultLink", linkType, enBezierType.Right, enLinkOpType.Start, true);
        }

        public override string GCode_GetTypeString(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            if (InputLink.HasLink)
            {
                return InputLink.GetLinkedObject(0, true).GCode_GetTypeString(InputLink.GetLinkedPinControl(0, true), context);
            }

            return base.GCode_GetTypeString(element, context);
        }

        public override string GCode_GetValueName(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            if (InputLink.HasLink)
            {
                string strInputValueName = InputLink.GetLinkedObject(0, true).GCode_GetValueName(InputLink.GetLinkedPinControl(0, true), context);
                return "normalize(" + strInputValueName + ")";
            }

            return base.GCode_GetValueName(element, context);
        }

        public override void GCode_GenerateCode(ref string strDefinitionSegment, ref string strSegment, int nLayer, CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            if (InputLink.HasLink)
            {
                InputLink.GetLinkedObject(0, true).GCode_GenerateCode(ref strDefinitionSegment, ref strSegment, nLayer, element, context);
            }
        }
    }
}
