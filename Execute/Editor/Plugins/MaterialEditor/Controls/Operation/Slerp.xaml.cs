using System;
using System.Windows;
using System.Windows.Controls;
using CodeGenerateSystem.Base;

namespace MaterialEditor.Controls.Operation
{
    [EngineNS.Rtti.MetaClass]
    public class SlerpConstructionParams : NodeControlConstructionBase { }
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    [CodeGenerateSystem.ShowInNodeList("Math/Slerp", "插值运算(Slerp)")]
    [CodeGenerateSystem.CustomConstructionParams(typeof(SlerpConstructionParams))]
    public partial class Slerp : BaseNodeControl
    {
        public Slerp(CodeGenerateSystem.Base.ConstructionParams smParam)
            : base(smParam)
        {
            InitializeComponent();

            AddLinkPinInfo("InputLinkX", InputLinkX, null);
            AddLinkPinInfo("InputLinkY", InputLinkY, null);
            AddLinkPinInfo("InputLinkAlpha", InputLinkAlpha, null);
            AddLinkPinInfo("ResultLink", ResultLink, ResultLink.BackBrush);
        }
        public static void InitNodePinTypes(CodeGenerateSystem.Base.ConstructionParams smParam)
        {
            enLinkType linkType = enLinkType.Float1 | enLinkType.Float2 | enLinkType.Float3 | enLinkType.Float4;
            CollectLinkPinInfo(smParam, "InputLinkX", linkType, enBezierType.Left, enLinkOpType.End, false);
            CollectLinkPinInfo(smParam, "InputLinkY", linkType, enBezierType.Left, enLinkOpType.End, false);
            CollectLinkPinInfo(smParam, "InputLinkAlpha", enLinkType.Float1, enBezierType.Left, enLinkOpType.End, false);
            CollectLinkPinInfo(smParam, "ResultLink", linkType, enBezierType.Right, enLinkOpType.Start, true);
        }
        protected override void CollectionErrorMsg()
        {
            if (!InputLinkX.HasLink)
            {
                HasError = true;
                ErrorDescription = "X必需要有输入";
            }
            if (!InputLinkY.HasLink)
            {
                HasError = true;
                ErrorDescription = "Y必需要有输入";
            }
            if (!InputLinkAlpha.HasLink)
            {
                HasError = true;
                ErrorDescription = "Alpha必需要有输入";
            }
        }
        public override string GCode_GetValueName(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            string strInputValueNameX;
            string strInputValueNameY;
            string strInputValueNameAlpha;
            string strFinalInputValueNameAlpha;
            if (InputLinkX.HasLink && InputLinkY.HasLink && InputLinkAlpha.HasLink )
            {
                strInputValueNameX = InputLinkX.GetLinkedObject(0, true).GCode_GetValueName(InputLinkX.GetLinkedPinControl(0, true), context);
                strInputValueNameY = InputLinkY.GetLinkedObject(0, true).GCode_GetValueName(InputLinkY.GetLinkedPinControl(0, true), context);
                strInputValueNameAlpha = InputLinkAlpha.GetLinkedObject(0, true).GCode_GetValueName(InputLinkAlpha.GetLinkedPinControl(0, true), context);

                strFinalInputValueNameAlpha = strInputValueNameAlpha;
                switch (InputLinkX.GetLinkedObject(0, true).GCode_GetTypeString(InputLinkX.GetLinkedPinControl(0, true), context))
                {
                    case "float4":
                        {
                            strFinalInputValueNameAlpha = "float4(" + strInputValueNameAlpha + "," + strInputValueNameAlpha + "," + strInputValueNameAlpha + "," + strInputValueNameAlpha + ")";
                        }
                        break;
                    case "float3":
                        {
                            strFinalInputValueNameAlpha = "float3(" + strInputValueNameAlpha + "," + strInputValueNameAlpha + "," + strInputValueNameAlpha + ")";
                        }
                        break;
                    case "float2":
                        {
                            strFinalInputValueNameAlpha = "float2(" + strInputValueNameAlpha + "," + strInputValueNameAlpha + ")";
                        }
                        break;
                }

                return "lerp(" + strInputValueNameX + "," + strInputValueNameY + "," + strFinalInputValueNameAlpha + ")";
            }

            return base.GCode_GetValueName(element, context);
        }

        public override string GCode_GetTypeString(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            if (InputLinkX.HasLink)
            {
                return InputLinkX.GetLinkedObject(0, true).GCode_GetTypeString(InputLinkX.GetLinkedPinControl(0, true), context);
            }

            return base.GCode_GetTypeString(element, context);
        }

        public override void GCode_GenerateCode(ref string strDefinitionSegment, ref string strSegment, int nLayer, CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            if (InputLinkX.HasLink)
            {
                InputLinkX.GetLinkedObject(0, true).GCode_GenerateCode(ref strDefinitionSegment, ref strSegment, nLayer, element, context);
            }
            if (InputLinkY.HasLink)
            {
                InputLinkY.GetLinkedObject(0, true).GCode_GenerateCode(ref strDefinitionSegment, ref strSegment, nLayer, element, context);
            }
            if (InputLinkAlpha.HasLink)
            {
                InputLinkAlpha.GetLinkedObject(0, true).GCode_GenerateCode(ref strDefinitionSegment, ref strSegment, nLayer, element, context);
            }
        }
    }
}
