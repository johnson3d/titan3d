using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace CodeDomNode
{
    [EngineNS.Rtti.MetaClass]
    public class ListRemoveAtConstructParam : CodeGenerateSystem.Base.ConstructionParams
    {

    }
    [CodeGenerateSystem.CustomConstructionParams(typeof(ListRemoveAtConstructParam))]
    public partial class ListRemoveAt : CodeGenerateSystem.Base.BaseNodeControl
    {
        CodeGenerateSystem.Base.LinkPinControl mMethodPrePin = new CodeGenerateSystem.Base.LinkPinControl();
        CodeGenerateSystem.Base.LinkPinControl mArrayInPin = new CodeGenerateSystem.Base.LinkPinControl();
        CodeGenerateSystem.Base.LinkPinControl mIndexInPin = new CodeGenerateSystem.Base.LinkPinControl();
        CodeGenerateSystem.Base.LinkPinControl mMethodNextPin = new CodeGenerateSystem.Base.LinkPinControl();

        Type mElementType = typeof(object);
        public Type ElementType
        {
            get => mElementType;
            set
            {
                if (mElementType == value)
                    return;
                mElementType = value ?? throw new InvalidOperationException();

                if (mArrayInPin != null)
                {
                    if (mElementType == typeof(object))
                        mArrayInPin.ClassType = "";
                    else
                        mArrayInPin.ClassType = $"System.Collections.Generic.List`1[[{mElementType.FullName}, {mElementType.Assembly.FullName}]]";
                }
            }
        }

        UInt32 mIndexValue = 0;
        public UInt32 IndexValue
        {
            get => mIndexValue;
            set
            {
                mIndexValue = value;
                OnPropertyChanged("IndexValue");
            }
        }

        partial void InitConstruction();
        public ListRemoveAt(CodeGenerateSystem.Base.ConstructionParams csParam)
            : base(csParam)
        {
            IsOnlyReturnValue = true;
            NodeName = "RemoveAt";
            InitConstruction();

            AddLinkPinInfo("MethodPrePin", mMethodPrePin);
            var arrayInPinInfo = AddLinkPinInfo("ArrayInPin", mArrayInPin);
            arrayInPinInfo.OnAddLinkInfo += ArrayInPin_OnAddLinkInfo;
            arrayInPinInfo.OnDelLinkInfo += ArrayInPin_OnDelLinkInfo;
            var valueInPinInfo = AddLinkPinInfo("IndexInPin", mIndexInPin);
            AddLinkPinInfo("MethodNextPin", mMethodNextPin);
        }

        void ArrayInPin_OnAddLinkInfo(CodeGenerateSystem.Base.LinkInfo linkInfo)
        {
            var listType = linkInfo.m_linkFromObjectInfo.GetLinkedObject(0, true).GCode_GetType(linkInfo.m_linkFromObjectInfo.GetLinkedPinControl(0, true), null);
            if (listType.GetInterface(typeof(IEnumerable).FullName) != null)
            {
                if (listType.IsGenericType)
                {
                    ElementType = listType.GetGenericArguments()[0];
                }
                else
                    throw new InvalidOperationException();
            }
            else
                throw new InvalidOperationException();
        }
        void ArrayInPin_OnDelLinkInfo(CodeGenerateSystem.Base.LinkInfo linkInfo)
        {
            ElementType = typeof(object);
        }
        public static void InitNodePinTypes(CodeGenerateSystem.Base.ConstructionParams csParam)
        {
            CollectLinkPinInfo(csParam, "MethodPrePin", CodeGenerateSystem.Base.enLinkType.Method, CodeGenerateSystem.Base.enBezierType.Left, CodeGenerateSystem.Base.enLinkOpType.End, true);
            CollectLinkPinInfo(csParam, "ArrayInPin", CodeGenerateSystem.Base.enLinkType.Enumerable, CodeGenerateSystem.Base.enBezierType.Left, CodeGenerateSystem.Base.enLinkOpType.End, false);
            CollectLinkPinInfo(csParam, "IndexInPin", typeof(System.Int32), CodeGenerateSystem.Base.enBezierType.Left, CodeGenerateSystem.Base.enLinkOpType.End, false);
            CollectLinkPinInfo(csParam, "MethodNextPin", CodeGenerateSystem.Base.enLinkType.Method, CodeGenerateSystem.Base.enBezierType.Right, CodeGenerateSystem.Base.enLinkOpType.Start, false);
        }
        public override void Save(EngineNS.IO.XndNode xndNode, bool newGuid)
        {
            var att = xndNode.AddAttrib("listAddData");
            att.BeginWrite();
            var typeStr = EngineNS.Rtti.RttiHelper.GetTypeSaveString(ElementType);
            att.Write(typeStr);
            att.Write(IndexValue);
            att.EndWrite();

            base.Save(xndNode, newGuid);
        }
        public override async System.Threading.Tasks.Task Load(EngineNS.IO.XndNode xndNode)
        {
            var att = xndNode.FindAttrib("listAddData");
            if (att != null)
            {
                att.BeginRead();
                string typeStr;
                att.Read(out typeStr);
                att.Read(out mIndexValue);
                IndexValue = mIndexValue;
                ElementType = EngineNS.Rtti.RttiHelper.GetTypeFromSaveString(typeStr);
                att.EndRead();
            }

            await base.Load(xndNode);
        }
        #region 代码生成

        public override string GCode_GetTypeString(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            if (element == mIndexInPin)
                return "System.Int32";
            else if (element == mArrayInPin)
            {
                if (ElementType == typeof(object))
                    return "";
                else
                {
                    return $"System.Collections.Generic.List<{EngineNS.Rtti.RttiHelper.GetAppTypeString(ElementType)}>";
                }
            }
            throw new InvalidOperationException();
        }
        public override Type GCode_GetType(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            if (element == mIndexInPin)
                return typeof(System.Int32);
            else if (element == mArrayInPin)
            {
                if (ElementType == null || ElementType == typeof(object))
                    return null;
                else
                {
                    var typeStr = $"System.Collections.Generic.List`1[[{ElementType.FullName}, {ElementType.Assembly.FullName}]]";
                    return EngineNS.Rtti.RttiHelper.GetTypeFromTypeFullName(typeStr, CSParam.CSType);
                }
            }
            throw new InvalidOperationException();
        }

        //CodeVariableDeclarationStatement mVariableDeclarationStatement = null;
        public override async System.Threading.Tasks.Task GCode_CodeDom_GenerateCode(CodeTypeDeclaration codeClass, CodeStatementCollection codeStatementCollection, CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            if (mArrayInPin.HasLink)
            {
                var tagExp = mArrayInPin.GetLinkedObject(0, true).GCode_CodeDom_GetValue(mArrayInPin.GetLinkedPinControl(0, true), context);

                if (!mArrayInPin.GetLinkedObject(0, true).IsOnlyReturnValue)
                    await mArrayInPin.GetLinkedObject(0, true).GCode_CodeDom_GenerateCode(codeClass, codeStatementCollection, mArrayInPin.GetLinkedPinControl(0, true), context);

                var condExp = new CodeConditionStatement();
                codeStatementCollection.Add(condExp);
                condExp.Condition = new CodeBinaryOperatorExpression(new CodePrimitiveExpression((Int32)IndexValue), CodeBinaryOperatorType.LessThan, new CodePropertyReferenceExpression(tagExp, "Count"));

                if (mIndexInPin.HasLink)
                {
                    if (!mIndexInPin.GetLinkedObject(0, true).IsOnlyReturnValue)
                        await mIndexInPin.GetLinkedObject(0, true).GCode_CodeDom_GenerateCode(codeClass, codeStatementCollection, mIndexInPin.GetLinkedPinControl(0, true), context);
                    condExp.TrueStatements.Add(new CodeExpressionStatement(new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(
                                                                tagExp,
                                                                "RemoveAt",
                                                                mIndexInPin.GetLinkedObject(0, true).GCode_CodeDom_GetValue(mIndexInPin.GetLinkedPinControl(0, true), context)
                                                                )));
                }
                else
                {
                    condExp.TrueStatements.Add(new CodeExpressionStatement(new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(
                                                            tagExp,
                                                            "RemoveAt",
                                                            new CodePrimitiveExpression((Int32)IndexValue))));
                }
            }

            if (context.GenerateNext)
            {
                if (mMethodNextPin.HasLink)
                    await mMethodNextPin.GetLinkedObject(0, false).GCode_CodeDom_GenerateCode(codeClass, codeStatementCollection, mMethodNextPin.GetLinkedPinControl(0, false), context);
            }
        }

        #endregion

    }
}
