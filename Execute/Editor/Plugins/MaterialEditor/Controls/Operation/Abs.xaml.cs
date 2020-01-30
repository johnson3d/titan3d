using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using CodeGenerateSystem.Base;

namespace MaterialEditor.Controls.Operation
{
    [EngineNS.Rtti.MetaClass]
    public class AbsConstructionParams : NodeControlConstructionBase { }
    /// <summary>
    /// Interaction logic for Abs.xaml
    /// </summary>
    [CodeGenerateSystem.ShowInNodeList("Math/Abs", "计算绝对值")]
    [CodeGenerateSystem.CustomConstructionParams(typeof(AbsConstructionParams))]
    public partial class Abs : BaseNodeControl
    {
        public Abs(CodeGenerateSystem.Base.ConstructionParams smParam)
            : base(smParam)
        {
            InitializeComponent();

            AddLinkPinInfo("InputLink", InputLink, null);
            AddLinkPinInfo("ResultLink", ResultLink, ResultLink.BackBrush);
        }

        public static void InitNodePinTypes(CodeGenerateSystem.Base.ConstructionParams smParam)
        {
            enLinkType linkType = enLinkType.Float1 | enLinkType.Float2 | enLinkType.Float3 | enLinkType.Float4;
            CollectLinkPinInfo(smParam, "InputLink", linkType, enBezierType.Left, enLinkOpType.End, false);
            CollectLinkPinInfo(smParam, "ResultLink", linkType, enBezierType.Right, enLinkOpType.Start, true);
        }

        protected override void CollectionErrorMsg()
        {
            if (!InputLink.HasLink)
            {
                HasError = true;
                ErrorDescription = "必需要有输入";
            }
        }

        public override string GCode_GetValueName(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            if (InputLink.HasLink)
            {
                string strInputValueName = InputLink.GetLinkedObject(0, true).GCode_GetValueName(InputLink.GetLinkedPinControl(0, true), context);
                return "abs(" + strInputValueName + ")";
            }

            return base.GCode_GetValueName(element, context);
        }

        public override string GCode_GetTypeString(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            if (InputLink.HasLink)
            {
                return InputLink.GetLinkedObject(0, true).GCode_GetTypeString(InputLink.GetLinkedPinControl(0, true), context);
            }

            return base.GCode_GetTypeString(element, context);
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
