using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CodeGenerateSystem.Base;
using EditorCommon.CodeGenerateSystem;

namespace CodeDomNode
{
    [EngineNS.Rtti.MetaClass]
    public class SequenceConstructParam : CodeGenerateSystem.Base.ConstructionParams
    {

    }
    [CodeGenerateSystem.CustomConstructionParams(typeof(SequenceConstructParam))]
    public partial class Sequence : CodeGenerateSystem.Base.BaseNodeControl
    {
        LinkPinControl mSeqInPin = new LinkPinControl();
        partial void InitConstruction();
        public Sequence(CodeGenerateSystem.Base.ConstructionParams csParam)
            : base(csParam)
        {
            NodeName = "Sequence(序列)";
            InitConstruction();
            AddLinkPinInfo("SeqIn", mSeqInPin, null);
        }
        public static void InitNodePinTypes(CodeGenerateSystem.Base.ConstructionParams smParam)
        {
            CollectLinkPinInfo(smParam, "SeqIn", CodeGenerateSystem.Base.enLinkType.Method, CodeGenerateSystem.Base.enBezierType.Left, CodeGenerateSystem.Base.enLinkOpType.End, true);
        }
        public override async Task GCode_CodeDom_GenerateCode(CodeTypeDeclaration codeClass, CodeStatementCollection codeStatementCollection, LinkPinControl element, GenerateCodeContext_Method context)
        {
            for(int i=0; i<mChildNodes.Count; i++)
            {
                var elm = mChildNodes[i] as SequenceElement;
                if (elm == null)
                    continue;
                await elm.GCode_CodeDom_GenerateCode(codeClass, codeStatementCollection, null, context);
            }
        }
    }

    [EngineNS.Rtti.MetaClass]
    public class SequenceElementConstructParam : CodeGenerateSystem.Base.ConstructionParams
    {
        [EngineNS.Rtti.MetaData]
        public int ElementIdx { get; set; }

        public override INodeConstructionParams Duplicate()
        {
            var retVal = base.Duplicate() as SequenceElementConstructParam;
            retVal.ElementIdx = ElementIdx;
            return retVal;
        }
    }
    [CodeGenerateSystem.CustomConstructionParams(typeof(SequenceElementConstructParam))]
    public partial class SequenceElement : CodeGenerateSystem.Base.BaseNodeControl
    {
        CodeGenerateSystem.Base.LinkPinControl mElemPin = new CodeGenerateSystem.Base.LinkPinControl();

        public int ElementIdx
        {
            get
            {
                var param = CSParam as SequenceElementConstructParam;
                if(param != null)
                    return param.ElementIdx;
                return 0;
            }
            set
            {
                var param = CSParam as SequenceElementConstructParam;
                if(param != null)
                    param.ElementIdx = value;
                if (mElemPin != null)
                    mElemPin.NameString = "[" + value + "]";
                OnPropertyChanged("ElementIdx");
            }
        }

        partial void InitConstruction();
        public SequenceElement(CodeGenerateSystem.Base.ConstructionParams csParam)
            : base(csParam)
        {
            var param = CSParam as SequenceElementConstructParam;

            NodeType = enNodeType.ChildNode;
            InitConstruction();
            mElemPin.NameString = "[" + param.ElementIdx + "]";
            AddLinkPinInfo("ElemPin", mElemPin, null);
        }

        public static void InitNodePinTypes(CodeGenerateSystem.Base.ConstructionParams csParam)
        {
            CollectLinkPinInfo(csParam, "ElemPin", CodeGenerateSystem.Base.enLinkType.Method, CodeGenerateSystem.Base.enBezierType.Right, CodeGenerateSystem.Base.enLinkOpType.Start, false);
        }

        public override async Task GCode_CodeDom_GenerateCode(CodeTypeDeclaration codeClass, CodeStatementCollection codeStatementCollection, LinkPinControl element, GenerateCodeContext_Method context)
        {
            if(mElemPin.HasLink)
            {
                var linkObj = mElemPin.GetLinkedObject(0);
                await linkObj.GCode_CodeDom_GenerateCode(codeClass, codeStatementCollection, mElemPin.GetLinkedPinControl(0), context);
            }
        }
    }
}
