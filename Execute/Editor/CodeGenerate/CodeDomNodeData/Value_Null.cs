using System;
using System.Collections.Generic;
using System.Text;

namespace CodeDomNode
{
    [EngineNS.Rtti.MetaClass]
    public class Value_NullConstructParam : CodeGenerateSystem.Base.ConstructionParams
    {

    }
    [CodeGenerateSystem.CustomConstructionParams(typeof(Value_NullConstructParam))]
    public partial class Value_Null : CodeGenerateSystem.Base.BaseNodeControl
    {
        CodeGenerateSystem.Base.LinkPinControl mCtrlValueLinkHandle = new CodeGenerateSystem.Base.LinkPinControl();
        string mTitleLabelText = "null";
        public string TitleLabelText
        {
            get { return mTitleLabelText; }
            set
            {
                mTitleLabelText = value;
                OnPropertyChanged("TitleLabelText");
            }
        }
        partial void InitConstruction();
        public Value_Null(CodeGenerateSystem.Base.ConstructionParams csParam)
            : base(csParam)
        {
            InitConstruction();
            IsOnlyReturnValue = true;
            NodeName = "null";

            AddLinkPinInfo("CtrlValueLinkHandle", mCtrlValueLinkHandle, null);

        }
        public static void InitNodePinTypes(CodeGenerateSystem.Base.ConstructionParams smParam)
        {
            CollectLinkPinInfo(smParam, "CtrlValueLinkHandle", CodeGenerateSystem.Base.enLinkType.Class | CodeGenerateSystem.Base.enLinkType.Vector2 | CodeGenerateSystem.Base.enLinkType.Vector3 | CodeGenerateSystem.Base.enLinkType.Vector4,
                                CodeGenerateSystem.Base.enBezierType.Right, CodeGenerateSystem.Base.enLinkOpType.Start, true);
        }

        public override string GCode_GetTypeString(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            return "System.Object";
        }

        public override Type GCode_GetType(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            return typeof(System.Object);
        }

        public override System.CodeDom.CodeExpression GCode_CodeDom_GetValue(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context, CodeGenerateSystem.Base.GenerateCodeContext_PreNode preNodeContext = null)
        {
            return new System.CodeDom.CodePrimitiveExpression(null);
        }
    }
}
