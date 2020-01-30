using System;
using System.Collections.Generic;
using System.Text;
using EngineNS.IO;

namespace CodeDomNode
{
    [EngineNS.Rtti.MetaClass]
    public class Value_BooleanConstructParam : CodeGenerateSystem.Base.ConstructionParams
    {

    }
    [CodeGenerateSystem.CustomConstructionParams(typeof(Value_BooleanConstructParam))]
    public partial class Value_Boolean : CodeGenerateSystem.Base.BaseNodeControl
    {
        CodeGenerateSystem.Base.LinkPinControl mCtrlValueLinkHandle = new CodeGenerateSystem.Base.LinkPinControl();

        int mComboBox_TF_SelectedIndex = 0;
        public int ComboBox_TF_SelectedIndex
        {
            get { return mComboBox_TF_SelectedIndex; }
            set
            {
                mComboBox_TF_SelectedIndex = value;
                OnPropertyChanged("ComboBox_TF_SelectedIndex");
            }
        }

        bool mValue = true;
        partial void InitConstruction();
        public Value_Boolean(CodeGenerateSystem.Base.ConstructionParams csParam)
            : base(csParam)
        {
            InitConstruction();
            if(!string.IsNullOrEmpty(CSParam.ConstructParam))
            {
                String[] splits = csParam.ConstructParam.Split(',');
                mValue = System.Convert.ToBoolean(splits[0]);
            }

            if (mValue)
                ComboBox_TF_SelectedIndex = 0;
            else
                ComboBox_TF_SelectedIndex = 1;

            IsOnlyReturnValue = true;
            NodeName = "数值.Boolean(布尔)";

            AddLinkPinInfo("CtrlValueLinkHandle", mCtrlValueLinkHandle, null);
        }
        public static void InitNodePinTypes(CodeGenerateSystem.Base.ConstructionParams smParam)
        {
            CollectLinkPinInfo(smParam, "CtrlValueLinkHandle", CodeGenerateSystem.Base.enLinkType.Bool, CodeGenerateSystem.Base.enBezierType.Right, CodeGenerateSystem.Base.enLinkOpType.Start, true);
        }

        public override void Save(XndNode xndNode, bool newGuid)
        {
            CSParam.ConstructParam = mValue.ToString();
            base.Save(xndNode, newGuid);
        }

        public override string GCode_GetTypeString(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            return "System.Boolean";
        }

        public override Type GCode_GetType(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            return typeof(System.Boolean);
        }

        public override System.CodeDom.CodeExpression GCode_CodeDom_GetValue(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context, CodeGenerateSystem.Base.GenerateCodeContext_PreNode preNodeContext = null)
        {
            return new System.CodeDom.CodePrimitiveExpression(mValue);
        }
    }
}
