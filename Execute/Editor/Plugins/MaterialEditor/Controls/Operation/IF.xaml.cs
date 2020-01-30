using System;
using System.Windows;
using System.Windows.Controls;
using CodeGenerateSystem.Base;

namespace MaterialEditor.Controls.Operation
{
    [EngineNS.Rtti.MetaClass]
    public class IFConstructionParams : NodeControlConstructionBase { }
    /// <summary>
    /// Interaction logic for IF.xaml
    /// </summary>
    [CodeGenerateSystem.ShowInNodeList("Math/If", "逻辑判断")]
    [CodeGenerateSystem.CustomConstructionParams(typeof(IFConstructionParams))]
    public partial class IF : BaseNodeControl
    {
        public IF(CodeGenerateSystem.Base.ConstructionParams smParam)
            : base(smParam)
        {
            InitializeComponent();

            AddLinkPinInfo("ValueA",  ValueA, null);
            AddLinkPinInfo("ValueB",  ValueB, null);
            AddLinkPinInfo("ValueAgtB", ValueAgtB, null);
            AddLinkPinInfo("ValueAeqB", ValueAeqB, null);
            AddLinkPinInfo("ValueAltB", ValueAltB, null);
            AddLinkPinInfo("ResultHandle", ResultHandle, ResultHandle.BackBrush);
            //AddLinkObject(enLinkType.Bool, ValueBool, enBezierType.Left, enLinkOpType.End, null, false);
        }

        public static void InitNodePinTypes(CodeGenerateSystem.Base.ConstructionParams smParam)
        {
            enLinkType codValueLinkType = enLinkType.Float1 | enLinkType.Int32 | enLinkType.Int64 | enLinkType.Double | enLinkType.Single;
            CollectLinkPinInfo(smParam, "ValueA", codValueLinkType, enBezierType.Left, enLinkOpType.End, false);
            CollectLinkPinInfo(smParam, "ValueB", codValueLinkType, enBezierType.Left, enLinkOpType.End, false);
            enLinkType resultValueType = enLinkType.Int32 | enLinkType.Float1 | enLinkType.Float2 | enLinkType.Float3 | enLinkType.Float4;
            CollectLinkPinInfo(smParam, "ValueAgtB", resultValueType, enBezierType.Left, enLinkOpType.End, false);
            CollectLinkPinInfo(smParam, "ValueAeqB", resultValueType, enBezierType.Left, enLinkOpType.End, false);
            CollectLinkPinInfo(smParam, "ValueAltB", resultValueType, enBezierType.Left, enLinkOpType.End, false);
            CollectLinkPinInfo(smParam, "ResultHandle", resultValueType, enBezierType.Right, enLinkOpType.Start, true);
        }

        protected override void CollectionErrorMsg()
        {
            //var lOI = GetLinkPinInfo(ValueA);
            //if (!lOI.bHasLink)
            //    AddErrorMsg(ValueA, CodeGenerateSystem.Controls.ErrorReportControl.ReportType.Error, "判断条件参数A未设置值");

            //lOI = GetLinkPinInfo(ValueB);
            //if (!lOI.bHasLink)
            //    AddErrorMsg(ValueB, CodeGenerateSystem.Controls.ErrorReportControl.ReportType.Error, "判断条件参数B未设置值");

            //var lOIAgtB = GetLinkPinInfo(ValueAgtB);
            //var lOIAeqB = GetLinkPinInfo(ValueAeqB);
            //var lOIAltB = GetLinkPinInfo(ValueAltB);
            //if (!lOIAgtB.bHasLink && !lOIAeqB.bHasLink && !lOIAltB.bHasLink)
            //    AddErrorMsg(new List<FrameworkElement> { ValueAgtB, ValueAeqB, ValueAltB }, CodeGenerateSystem.Controls.ErrorReportControl.ReportType.Error, "结果未设置值，必须至少设置一个");

            ////string vtAgtB = "", vtAeqB = "", vtAltB = "";
            ////if (lOIAgtB.bHasLink)
            ////    vtAgtB = lOIAgtB.GetLinkObject(0, true).GCode_GetValueType(lOIAgtB.GetLinkElement(0, true));
            ////if (lOIAeqB.bHasLink)
            ////    vtAeqB = lOIAeqB.GetLinkObject(0, true).GCode_GetValueType(lOIAeqB.GetLinkElement(0, true));
            ////if (lOIAltB.bHasLink)
            ////    vtAltB = lOIAltB.GetLinkObject(0, true).GCode_GetValueType(lOIAltB.GetLinkElement(0, true));
            //var lOI1LinkType = lOIAgtB.GetLinkType(0, true);
            //var lOI2LinkType = lOIAeqB.GetLinkType(0, true);
            //var lOI3LinkType = lOIAltB.GetLinkType(0, true);
            //if (lOI1LinkType != lOI2LinkType)
            //    AddErrorMsg(new List<FrameworkElement> { ValueAgtB, ValueAeqB }, CodeGenerateSystem.Controls.ErrorReportControl.ReportType.Error, "参数类型不一致， A>B:" + lOI1LinkType.ToString() + " A=B:" + lOI2LinkType.ToString());
            //if (lOI1LinkType != lOI3LinkType)
            //    AddErrorMsg(new List<FrameworkElement> { ValueAgtB, ValueAltB }, CodeGenerateSystem.Controls.ErrorReportControl.ReportType.Error, "参数类型不一致, A>B:" + lOI1LinkType.ToString() + " A<B:" + lOI3LinkType.ToString());
            //if (lOI2LinkType != lOI3LinkType)
            //    AddErrorMsg(new List<FrameworkElement> { ValueAeqB, ValueAltB }, CodeGenerateSystem.Controls.ErrorReportControl.ReportType.Error, "参数类型不一致, A=B:" + lOI2LinkType.ToString() + " A<B:" + lOI3LinkType.ToString());

            //var lOIR = GetLinkPinInfo(ResultHandle);
            //if (!lOIR.bHasLink)
            //    AddErrorMsg(ResultHandle, CodeGenerateSystem.Controls.ErrorReportControl.ReportType.Error, "未将结果设置给任意变量，当前节点没有作用");

            bool calculateAgtB = false, calculateAeqB = false, calculateAltB = false;
            string valTypeStr_AgtB = "", valTypeStr_AeqB = "", valTypeStr_AltB = "";
            int de_AgtB = 1, de_AeqB = 1, de_AltB = 1;
            if(ValueAgtB.HasLink)
            {
                var lo_AgtB = ValueAgtB.GetLinkedObject(0, true);   // A>B
                valTypeStr_AgtB = lo_AgtB.GCode_GetTypeString(ValueAgtB.GetLinkedPinControl(0, true), null);
                var pureStr = valTypeStr_AgtB.TrimEnd('1', '2', '3', '4');
                var deStr = valTypeStr_AgtB.Replace(pureStr, "");
                de_AgtB = System.Convert.ToInt32(deStr);
                calculateAgtB = true;
            }
            if(ValueAeqB.HasLink)
            {
                var lo_AeqB = ValueAeqB.GetLinkedObject(0, true);   // A==B
                valTypeStr_AeqB = lo_AeqB.GCode_GetTypeString(ValueAeqB.GetLinkedPinControl(0, true), null);
                var pureStr = valTypeStr_AeqB.TrimEnd('1', '2', '3', '4');
                var deStr = valTypeStr_AeqB.Replace(pureStr, "");
                de_AeqB = System.Convert.ToInt32(deStr);
                calculateAeqB = true;
            }
            if (ValueAltB.HasLink)
            {
                var lo_AltB = ValueAltB.GetLinkedObject(0, true);   // A<B
                valTypeStr_AltB = lo_AltB.GCode_GetTypeString(ValueAltB.GetLinkedPinControl(0, true), null);
                var pureStr = valTypeStr_AltB.TrimEnd('1', '2', '3', '4');
                var deStr = valTypeStr_AltB.Replace(pureStr, "");
                de_AltB = System.Convert.ToInt32(deStr);
                calculateAltB = true;
            }
            if(calculateAgtB && calculateAeqB && (valTypeStr_AgtB != valTypeStr_AeqB))
            {
                if (de_AgtB != de_AeqB)
                {
                    HasError = true;
                    ErrorDescription = $"A>B({valTypeStr_AgtB})与A==B({valTypeStr_AeqB})参数类型不一致";
                }
            }
            if(calculateAgtB && calculateAltB && (valTypeStr_AgtB != valTypeStr_AltB))
            {
                if (de_AgtB != de_AltB)
                {
                    HasError = true;
                    ErrorDescription = $"A>B({valTypeStr_AgtB})与A<B({valTypeStr_AltB})参数类型不一致";
                }
            }
            if(calculateAeqB && calculateAltB && (valTypeStr_AeqB != valTypeStr_AltB))
            {
                if (de_AeqB != de_AltB)
                {
                    HasError = true;
                    ErrorDescription = $"A==B({valTypeStr_AeqB})与A<B({valTypeStr_AltB})参数类型不一致";
                }
            }
        }

        public override void GCode_GenerateCode(ref string strDefinitionSegment, ref string strSegment, int nLayer, CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            string strTab = GCode_GetTabString(nLayer);

            if (ValueA == null || ValueA.GetLinkedObject(0, true) == null)
                return;

            string valueNameA;
            ValueA.GetLinkedObject(0, true).GCode_GenerateCode(ref strDefinitionSegment, ref strSegment, nLayer, ValueA.GetLinkedPinControl(0, true), context);
            valueNameA = ValueA.GetLinkedObject(0, true).GCode_GetValueName(ValueA.GetLinkedPinControl(0, true), context);

            string valueNameB;
            ValueB.GetLinkedObject(0, true).GCode_GenerateCode(ref strDefinitionSegment, ref strSegment, nLayer, ValueB.GetLinkedPinControl(0, true), context);
            valueNameB = ValueB.GetLinkedObject(0, true).GCode_GetValueName(ValueB.GetLinkedPinControl(0, true), context);

            var lOI = this.ValueAgtB.HasLink ? this.ValueAgtB : (ValueAeqB.HasLink ? ValueAeqB : (ValueAltB.HasLink ? ValueAltB : null));

            if (lOI == null)
                return;

            // 判断变量是否已经声明过了，没有声明过则进行声明
            string strInitString = "";
            string strValueType = lOI.GetLinkedObject(0, true).GCode_GetTypeString(lOI.GetLinkedPinControl(0, true), context);
            strInitString = Program.GetInitialNewString(strValueType);
            var strValueIdt = strValueType + " " + GCode_GetValueName(null, context) + " = " + strInitString + ";\r\n";
            if (!strDefinitionSegment.Contains(strValueIdt))
                strDefinitionSegment += "    " + strValueIdt;

            var lo_AgtB = ValueAgtB.GetLinkedObject(0, true);   // A>B
            var lo_AeqB = ValueAeqB.GetLinkedObject(0, true);   // A==B
            var lo_AltB = ValueAltB.GetLinkedObject(0, true);   // A<B

            if(lo_AgtB != null)
                lo_AgtB.GCode_GenerateCode(ref strDefinitionSegment, ref strSegment, nLayer + 1, ValueAgtB.GetLinkedPinControl(0, true), context);
            if(lo_AeqB != null && lo_AeqB != lo_AgtB)
                lo_AeqB.GCode_GenerateCode(ref strDefinitionSegment, ref strSegment, nLayer + 1, ValueAeqB.GetLinkedPinControl(0, true), context);
            if(lo_AltB != null && lo_AltB != lo_AgtB && lo_AltB != lo_AeqB)
                lo_AltB.GCode_GenerateCode(ref strDefinitionSegment, ref strSegment, nLayer + 1, ValueAltB.GetLinkedPinControl(0, true), context);
            // A>B
            if (ValueAgtB.HasLink)
            {
                strSegment += strTab + "if( " + valueNameA + " > " + valueNameB + ")\r\n";
                strSegment += strTab + "{\r\n";
                //string strIFSegment = "";
                //lOIAgtB.GetLinkObject(0, true).GCode_GenerateCode(ref strDefinitionSegment, ref strIFSegment, nLayer + 1, lOIAgtB.GetLinkElement(0, true));
                //strSegment += strIFSegment;
                strSegment += GCode_GetTabString(nLayer + 1) + GCode_GetValueName(null, context) + " = " + lo_AgtB.GCode_GetValueName(ValueAgtB.GetLinkedPinControl(0, true), context) + ";\r\n";
                strSegment += strTab + "}\r\n";
            }
            // A==B
            if (ValueAeqB.HasLink)
            {
                //if (ValueAltB.HasLink)
                //{
                //    string strIfType;
                //    if (ValueAgtB.HasLink)
                //        strIfType = "else if";
                //    else
                //        strIfType = "if";
                //    strSegment += strTab + strIfType + "( " + valueNameA + " == " + valueNameB + ")\r\n";
                //}
                //else
                //    strSegment += strTab + "else\r\n";
                strSegment += strTab + "if( " + valueNameA + " == " + valueNameB + ")\r\n";
                strSegment += strTab + "{\r\n";
                //string strIFSegment = "";
                //lOIAeqB.GetLinkObject(0, true).GCode_GenerateCode(ref strDefinitionSegment, ref strIFSegment, nLayer + 1, lOIAeqB.GetLinkElement(0, true));
                //strSegment += strIFSegment;
                strSegment += GCode_GetTabString(nLayer + 1) + GCode_GetValueName(null, context) + " = " + lo_AeqB.GCode_GetValueName(ValueAeqB.GetLinkedPinControl(0, true), context) + ";\r\n";
                strSegment += strTab + "}\r\n";
            }
            // A<B
            if (ValueAltB.HasLink)
            {
                //if (ValueAgtB.HasLink || ValueAeqB.HasLink)
                //    strSegment += strTab + "else\r\n";
                //else
                //    strSegment += strTab + "if( " + valueNameA + " < " + valueNameB + ")\r\n";
                strSegment += strTab + "if( " + valueNameA + " < " + valueNameB + ")\r\n";
                strSegment += strTab + "{\r\n";
                //string strIFSegment = "";
                //lOIAltB.GetLinkObject(0, true).GCode_GenerateCode(ref strDefinitionSegment, ref strIFSegment, nLayer + 1, lOIAltB.GetLinkElement(0, true));
                //strSegment += strIFSegment;
                strSegment += GCode_GetTabString(nLayer + 1) + GCode_GetValueName(null, context) + " = " + lo_AltB.GCode_GetValueName(ValueAltB.GetLinkedPinControl(0, true), context) + ";\r\n";
                strSegment += strTab + "}\r\n";
            }
        }

        public override string GCode_GetValueName(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            string strValueName = "";
            if (element == null || element == ResultHandle)
            {
                strValueName = "Value_" + EngineNS.Editor.Assist.GetValuedGUIDString(Id);
            }

            return strValueName;
        }

        public override string GCode_GetTypeString(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            var lOI = ValueAgtB.HasLink ? ValueAgtB : (ValueAeqB.HasLink ? ValueAeqB : (ValueAltB.HasLink ? ValueAltB : null));

            if(lOI != null)
            {
                return lOI.GetLinkedObject(0, true).GCode_GetTypeString(lOI.GetLinkedPinControl(0, true), context);
            }

            return base.GCode_GetTypeString(element, context);
        }
    }
}
