using System;
using System.Collections.Generic;
using System.Text;

namespace CodeDomNode
{
    [EngineNS.Rtti.MetaClass]
    public class BreakContinueConstructParam : CodeGenerateSystem.Base.ConstructionParams
    {

    }
    [CodeGenerateSystem.CustomConstructionParams(typeof(BreakContinueConstructParam))]
    public partial class BreakContinueNode : CodeGenerateSystem.Base.BaseNodeControl
    {
        CodeGenerateSystem.Base.LinkPinControl mCtrlMethod_Pre = new CodeGenerateSystem.Base.LinkPinControl();

        partial void InitConstruction();
        public BreakContinueNode(CodeGenerateSystem.Base.ConstructionParams csParam)
            : base(csParam)
        {
            InitConstruction();
            AddLinkPinInfo("CtrlMethod_Pre", mCtrlMethod_Pre, null);
        }
        public static void InitNodePinTypes(CodeGenerateSystem.Base.ConstructionParams smParam)
        {
            CollectLinkPinInfo(smParam, "CtrlMethod_Pre", CodeGenerateSystem.Base.enLinkType.Method, CodeGenerateSystem.Base.enBezierType.Top, CodeGenerateSystem.Base.enLinkOpType.End, false);
        }

        string mValueStr = "";
        public override async System.Threading.Tasks.Task GCode_CodeDom_GenerateCode(System.CodeDom.CodeTypeDeclaration codeClass, System.CodeDom.CodeStatementCollection codeStatementCollection, CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            var retStatement = new System.CodeDom.CodeMethodReturnStatement();
            codeStatementCollection.Add(new System.CodeDom.CodeSnippetExpression(mValueStr));
        }
    }
}
