using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using EngineNS.IO;

namespace CodeDomNode
{
    [EngineNS.Rtti.MetaClass]
    public class ListInsertConstructParam : CodeGenerateSystem.Base.ConstructionParams
    {

    }
    [CodeGenerateSystem.CustomConstructionParams(typeof(ListInsertConstructParam))]
    public partial class ListInsert : CodeGenerateSystem.Base.BaseNodeControl
    {
        CodeGenerateSystem.Base.LinkPinControl mMethodPrePin = new CodeGenerateSystem.Base.LinkPinControl();
        CodeGenerateSystem.Base.LinkPinControl mArrayInPin = new CodeGenerateSystem.Base.LinkPinControl();
        CodeGenerateSystem.Base.LinkPinControl mValueInPin = new CodeGenerateSystem.Base.LinkPinControl();
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
                mElementType = value;

                if (mElementType != null && mElementType != typeof(object) && !string.IsNullOrEmpty(mElementValueStr))
                {
                    if (mElementType.IsEnum)
                        InElementValue = EngineNS.Rtti.RttiHelper.EnumTryParse(mElementType, mElementValueStr);
                    else
                        InElementValue = System.Convert.ChangeType(mElementValueStr, mElementType);
                }

                if (mArrayInPin != null)
                {
                    if (mElementType == null)
                        mArrayInPin.ClassType = "";
                    else
                        mArrayInPin.ClassType = $"System.Collections.Generic.List`1[[{mElementType.FullName}, {mElementType.Assembly.FullName}]]";
                }
                if (mValueInPin != null)
                {
                    if (mElementType == null || mElementType == typeof(object))
                    {
                        mValueInPin.LinkType = CodeGenerateSystem.Base.enLinkType.All;
                        mValueInPin.ClassType = "";
                    }
                    else
                    {
                        mValueInPin.LinkType = CodeGenerateSystem.Base.LinkPinControl.GetLinkTypeFromCommonType(mElementType);
                        mValueInPin.ClassType = mElementType.FullName;
                    }
                }

                OnElementTypeChanged_WPF();
            }
        }
        partial void OnElementTypeChanged_WPF();
        string mElementValueStr;
        object mInElementValue;
        public object InElementValue
        {
            get => mInElementValue;
            set
            {
                mInElementValue = value;
                OnPropertyChanged("InElementValue");
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
        public ListInsert(CodeGenerateSystem.Base.ConstructionParams csParam)
            : base(csParam)
        {
            IsOnlyReturnValue = true;
            NodeName = "Insert";
            InitConstruction();

            AddLinkPinInfo("MethodPrePin", mMethodPrePin);
            var arrayInPinInfo = AddLinkPinInfo("ArrayInPin", mArrayInPin);
            arrayInPinInfo.OnAddLinkInfo += ArrayInPin_OnAddLinkInfo;
            arrayInPinInfo.OnDelLinkInfo += ArrayInPin_OnDelLinkInfo;
            var valueInPinInfo = AddLinkPinInfo("ValueInPin", mValueInPin);
            valueInPinInfo.OnAddLinkInfo += ValueInPinInfo_OnAddLinkInfo;
            valueInPinInfo.OnDelLinkInfo += ValueInPinInfo_OnDelLinkInfo;
            AddLinkPinInfo("IndexInPin", mIndexInPin);
            AddLinkPinInfo("MethodNextPin", mMethodNextPin);
        }
        public static void InitNodePinTypes(CodeGenerateSystem.Base.ConstructionParams csParam)
        {
            CollectLinkPinInfo(csParam, "MethodPrePin", CodeGenerateSystem.Base.enLinkType.Method, CodeGenerateSystem.Base.enBezierType.Left, CodeGenerateSystem.Base.enLinkOpType.End, true);
            CollectLinkPinInfo(csParam, "ArrayInPin", CodeGenerateSystem.Base.enLinkType.Enumerable, CodeGenerateSystem.Base.enBezierType.Left, CodeGenerateSystem.Base.enLinkOpType.End, false);
            CollectLinkPinInfo(csParam, "ValueInPin", CodeGenerateSystem.Base.enLinkType.All, CodeGenerateSystem.Base.enBezierType.Left, CodeGenerateSystem.Base.enLinkOpType.End, false);
            CollectLinkPinInfo(csParam, "MethodNextPin", CodeGenerateSystem.Base.enLinkType.Method, CodeGenerateSystem.Base.enBezierType.Right, CodeGenerateSystem.Base.enLinkOpType.Start, false);
            CollectLinkPinInfo(csParam, "IndexInPin", typeof(System.Int32), CodeGenerateSystem.Base.enBezierType.Right, CodeGenerateSystem.Base.enLinkOpType.Start, true);
        }

        private void ValueInPinInfo_OnAddLinkInfo(CodeGenerateSystem.Base.LinkInfo linkInfo)
        {
            var elmType = linkInfo.m_linkFromObjectInfo.GetLinkedObject(0, true).GCode_GetType(linkInfo.m_linkFromObjectInfo.GetLinkedPinControl(0, true), null);
            if (ElementType == null || ElementType == typeof(object))
                ElementType = elmType;
        }
        private void ValueInPinInfo_OnDelLinkInfo(CodeGenerateSystem.Base.LinkInfo linkInfo)
        {
            if (!mArrayInPin.HasLink)
                ElementType = typeof(object);
        }

        void ArrayInPin_OnAddLinkInfo(CodeGenerateSystem.Base.LinkInfo linkInfo)
        {
            var listType = linkInfo.m_linkFromObjectInfo.GetLinkedObject(0, true).GCode_GetType(linkInfo.m_linkFromObjectInfo.GetLinkedPinControl(0, true), null);
            if (listType.GetInterface(typeof(IEnumerable).FullName) != null)
            {
                if (listType.IsGenericType)
                {
                    if (ElementType != null && ElementType != typeof(object))
                    {
                        var argType = listType.GetGenericArguments()[0];
                        if (ElementType != argType)
                        {
                            if (ElementType.IsSubclassOf(argType))
                                ElementType = argType;
                            else
                                throw new InvalidOperationException();
                        }
                    }
                    else
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
            if (mValueInPin.HasLink)
            {
                ElementType = mValueInPin.GetLinkedObject(0, true).GCode_GetType(mValueInPin.GetLinkedPinControl(0, true), null);
            }
            else
                ElementType = typeof(object);
        }

        public override void Save(XndNode xndNode, bool newGuid)
        {
            var att = xndNode.AddAttrib("listInsertData");
            att.BeginWrite();
            var typeStr = EngineNS.Rtti.RttiHelper.GetTypeSaveString(ElementType);
            att.Write(typeStr);
            if (mInElementValue == null)
                att.Write("");
            else
                att.Write(mInElementValue.ToString());
            att.Write(IndexValue);
            att.EndWrite();

            base.Save(xndNode, newGuid);
        }
        public override async System.Threading.Tasks.Task Load(XndNode xndNode)
        {
            var att = xndNode.FindAttrib("listInsertData");
            if(att != null)
            {
                att.BeginRead();
                string typeStr;
                att.Read(out typeStr);
                att.Read(out mElementValueStr);
                att.Read(out mIndexValue);
                IndexValue = mIndexValue;
                ElementType = EngineNS.Rtti.RttiHelper.GetTypeFromSaveString(typeStr);
                att.EndRead();
            }

            await base.Load(xndNode);
        }

        #region 代码生成

        public override string GCode_GetValueName(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            throw new InvalidOperationException();
        }
        public override string GCode_GetTypeString(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            if (element == mIndexInPin)
                return "System.Int32";
            else if (element == mArrayInPin)
            {
                if (ElementType == null || ElementType == typeof(object))
                    return "";
                else
                {
                    return $"System.Collections.Generic.List<{EngineNS.Rtti.RttiHelper.GetAppTypeString(ElementType)}>";
                }
            }
            else if (element == mValueInPin)
            {
                return EngineNS.Rtti.RttiHelper.GetAppTypeString(ElementType);
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
            else if (element == mValueInPin)
            {
                return ElementType;
            }
            throw new InvalidOperationException();
        }
        public override CodeExpression GCode_CodeDom_GetValue(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context, CodeGenerateSystem.Base.GenerateCodeContext_PreNode preNodeContext = null)
        {
            throw new InvalidOperationException();
        }

        public override async System.Threading.Tasks.Task GCode_CodeDom_GenerateCode(CodeTypeDeclaration codeClass, CodeStatementCollection codeStatementCollection, CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            if (mArrayInPin.HasLink)
            {
                var tagExp = mArrayInPin.GetLinkedObject(0, true).GCode_CodeDom_GetValue(mArrayInPin.GetLinkedPinControl(0, true), context);

                if (!mArrayInPin.GetLinkedObject(0, true).IsOnlyReturnValue)
                    await mArrayInPin.GetLinkedObject(0, true).GCode_CodeDom_GenerateCode(codeClass, codeStatementCollection, mArrayInPin.GetLinkedPinControl(0, true), context);
                CodeExpression indexInValueExp;
                if(mIndexInPin.HasLink)
                {
                    if (!mIndexInPin.GetLinkedObject(0, true).IsOnlyReturnValue)
                        await mIndexInPin.GetLinkedObject(0, true).GCode_CodeDom_GenerateCode(codeClass, codeStatementCollection, mIndexInPin.GetLinkedPinControl(0, true), context);
                    indexInValueExp = mIndexInPin.GetLinkedObject(0, true).GCode_CodeDom_GetValue(mIndexInPin.GetLinkedPinControl(0, true), context);
                }
                else
                {
                    indexInValueExp = new CodePrimitiveExpression((Int32)IndexValue);
                }

                var condExp = new CodeConditionStatement();
                codeStatementCollection.Add(condExp);
                condExp.Condition = new CodeBinaryOperatorExpression(indexInValueExp, CodeBinaryOperatorType.LessThan, new CodePropertyReferenceExpression(tagExp, "Count"));

                if (mValueInPin.HasLink)
                {
                    if (!mValueInPin.GetLinkedObject(0, true).IsOnlyReturnValue)
                        await mValueInPin.GetLinkedObject(0, true).GCode_CodeDom_GenerateCode(codeClass, codeStatementCollection, mValueInPin.GetLinkedPinControl(0, true), context);
                    condExp.TrueStatements.Add(new CodeExpressionStatement(new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(
                                                                tagExp,
                                                                "Insert",
                                                                indexInValueExp,
                                                                mValueInPin.GetLinkedObject(0, true).GCode_CodeDom_GetValue(mValueInPin.GetLinkedPinControl(0, true), context)
                                                                )));
                }
                else
                {
                    if (ElementType.IsEnum)
                    {
                        condExp.TrueStatements.Add(new CodeExpressionStatement(new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(
                                                                tagExp,
                                                                "Insert",
                                                                indexInValueExp,
                                                                new CodeSnippetExpression(EngineNS.Rtti.RttiHelper.GetAppTypeString(ElementType) + "." + InElementValue.ToString()))));
                    }
                    else if (ElementType == typeof(string) || (ElementType.IsValueType && ElementType.IsPrimitive))
                    {
                        try
                        {
                            var val = System.Convert.ChangeType(InElementValue, ElementType);
                            condExp.TrueStatements.Add(new CodeExpressionStatement(new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(
                                                                tagExp,
                                                                "Insert",
                                                                indexInValueExp,
                                                                new CodePrimitiveExpression(val))));
                        }
                        catch (System.Exception)
                        {

                        }
                    }
                    else
                    {
                        condExp.TrueStatements.Add(new CodeExpressionStatement(new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(
                                                            tagExp,
                                                            "Insert",
                                                            indexInValueExp,
                                                            new CodeObjectCreateExpression(ElementType, new CodeExpression[0]))));
                    }
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
