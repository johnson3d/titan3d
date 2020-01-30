using System;
using System.Collections.Generic;
using System.Text;
using EngineNS.IO;

namespace CodeDomNode
{
    [EngineNS.Rtti.MetaClass]
    public class TypeCastConstructParam : CodeGenerateSystem.Base.ConstructionParams
    {

    }
    [CodeGenerateSystem.CustomConstructionParams(typeof(TypeCastConstructParam))]
    public partial class TypeCastControl : CodeGenerateSystem.Base.BaseNodeControl
    {
        CodeGenerateSystem.Base.LinkPinControl mCtrlClassLinkHandle_In = new CodeGenerateSystem.Base.LinkPinControl();
        CodeGenerateSystem.Base.LinkPinControl mCtrlClassLinkHandle_Out = new CodeGenerateSystem.Base.LinkPinControl();

        EngineNS.ECSType mCSType = EngineNS.ECSType.Common;

        string mTargetTypeName = "";
        public string TargetTypeName
        {
            get { return mTargetTypeName; }
            set
            {
                mTargetTypeName = value;
                OnPropertyChanged("TargetTypeName");
            }
        }

        partial void InitConstruction();
        public TypeCastControl(CodeGenerateSystem.Base.ConstructionParams csParam)
            : base(csParam)
        {
            InitConstruction();

            mCSType = (EngineNS.ECSType)System.Enum.Parse(typeof(EngineNS.ECSType), csParam.ConstructParam);
            NodeName = "强制类型转换";
            AddLinkPinInfo("CtrlClassLinkHandle_In", mCtrlClassLinkHandle_In, null);
            AddLinkPinInfo("CtrlClassLinkHandle_Out", mCtrlClassLinkHandle_Out, null);
        }
        public static void InitNodePinTypes(CodeGenerateSystem.Base.ConstructionParams smParam)
        {
            CollectLinkPinInfo(smParam, "CtrlClassLinkHandle_In", CodeGenerateSystem.Base.enLinkType.All, CodeGenerateSystem.Base.enBezierType.Left, CodeGenerateSystem.Base.enLinkOpType.End, false);
            CollectLinkPinInfo(smParam, "CtrlClassLinkHandle_Out", CodeGenerateSystem.Base.enLinkType.All, CodeGenerateSystem.Base.enBezierType.Right, CodeGenerateSystem.Base.enLinkOpType.Start, true);
        }
        public override void Save(XndNode xndNode, bool newGuid)
        {
            var att = xndNode.AddAttrib("Data");
            att.Version = 0;
            att.BeginWrite();
            att.Write(TargetTypeName);
            att.EndWrite();
            base.Save(xndNode, newGuid);
        }
        public override async System.Threading.Tasks.Task Load(XndNode xndNode)
        {
            await base.Load(xndNode);
            var att = xndNode.FindAttrib("Data");
            if(att != null)
            {
                att.BeginRead();
                switch(att.Version)
                {
                    case 0:
                        {
                            att.Read(out mTargetTypeName);
                        }
                        break;
                }
                att.EndRead();
            }
        }
        
        public override string GCode_GetTypeString(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            if (element == mCtrlClassLinkHandle_Out)
            {
                return TargetTypeName;
            }

            return "";
        }

        public override Type GCode_GetType(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            if (element == mCtrlClassLinkHandle_Out)
            {
                return EngineNS.Rtti.RttiHelper.GetTypeFromTypeFullName(TargetTypeName);
            }

            return null;
        }

        public override async System.Threading.Tasks.Task GCode_CodeDom_GenerateCode(System.CodeDom.CodeTypeDeclaration codeClass, System.CodeDom.CodeStatementCollection codeStatementCollection, CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            if (element == mCtrlClassLinkHandle_Out)
            {
                if (mCtrlClassLinkHandle_In.HasLink)
                {
                    if (!mCtrlClassLinkHandle_In.GetLinkedObject(0, true).IsOnlyReturnValue)
                        await mCtrlClassLinkHandle_In.GetLinkedObject(0, true).GCode_CodeDom_GenerateCode(codeClass, codeStatementCollection, mCtrlClassLinkHandle_In.GetLinkedPinControl(0, true), context);
                }
            }
        }

        public override System.CodeDom.CodeExpression GCode_CodeDom_GetValue(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context, CodeGenerateSystem.Base.GenerateCodeContext_PreNode preNodeContext = null)
        {
            if (element == mCtrlClassLinkHandle_Out)
            {
                if (mCtrlClassLinkHandle_In.HasLink)
                {
                    return new CodeGenerateSystem.CodeDom.CodeCastExpression(GCode_GetType(element, context),
                                                                 mCtrlClassLinkHandle_In.GetLinkedObject(0, true).GCode_CodeDom_GetValue(mCtrlClassLinkHandle_In.GetLinkedPinControl(0, true), context));
                }
            }

            return null;
        }
    }
}
