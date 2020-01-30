using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace CodeDomNode
{
    [EngineNS.Rtti.MetaClass]
    public class ListFindConstructParam : CodeGenerateSystem.Base.ConstructionParams
    {

    }
    [CodeGenerateSystem.CustomConstructionParams(typeof(ListFindConstructParam))]
    public partial class ListFind : CodeGenerateSystem.Base.BaseNodeControl
    {
        CodeGenerateSystem.Base.LinkPinControl mArrayInPin = new CodeGenerateSystem.Base.LinkPinControl();
        CodeGenerateSystem.Base.LinkPinControl mValueInPin = new CodeGenerateSystem.Base.LinkPinControl();
        CodeGenerateSystem.Base.LinkPinControl mIndexOutPin = new CodeGenerateSystem.Base.LinkPinControl();

        Type mElementType = typeof(object);
        public Type ElementType
        {
            get => mElementType;
            set
            {
                if (mElementType == value)
                    return;
                mElementType = value ?? throw new InvalidOperationException();

                if (mElementType != null && mElementType != typeof(object) && !string.IsNullOrEmpty(mElementValueStr))
                {
                    if (mElementType.IsEnum)
                        InElementValue = EngineNS.Rtti.RttiHelper.EnumTryParse(mElementType, mElementValueStr);
                    else
                        InElementValue = System.Convert.ChangeType(mElementValueStr, mElementType);
                }

                if (mArrayInPin != null)
                {
                    if (mElementType == typeof(object))
                        mArrayInPin.ClassType = "";
                    else
                        mArrayInPin.ClassType = $"System.Collections.Generic.List`1[[{mElementType.FullName}, {mElementType.Assembly.FullName}]]";
                }
                if (mValueInPin != null)
                {
                    if (mElementType == typeof(object))
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

        partial void InitConstruction();
        public ListFind(CodeGenerateSystem.Base.ConstructionParams csParam)
            : base(csParam)
        {
            NodeName = "Find";
            InitConstruction();

            var arrayInPinInfo = AddLinkPinInfo("ArrayInPin", mArrayInPin);
            arrayInPinInfo.OnAddLinkInfo += ArrayInPin_OnAddLinkInfo;
            arrayInPinInfo.OnDelLinkInfo += ArrayInPin_OnDelLinkInfo;
            var valueInPinInfo = AddLinkPinInfo("ValueInPin", mValueInPin);
            valueInPinInfo.OnAddLinkInfo += ValueInPinInfo_OnAddLinkInfo;
            valueInPinInfo.OnDelLinkInfo += ValueInPinInfo_OnDelLinkInfo;
            AddLinkPinInfo("IndexOutPin", mIndexOutPin);
        }

        private void ValueInPinInfo_OnAddLinkInfo(CodeGenerateSystem.Base.LinkInfo linkInfo)
        {
            if (ElementType == typeof(object))
            {
                var elmType = linkInfo.m_linkFromObjectInfo.GetLinkedObject(0, true).GCode_GetType(linkInfo.m_linkFromObjectInfo.GetLinkedPinControl(0, true), null);
                ElementType = elmType;
            }
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
                    if (ElementType != typeof(object))
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

        public static void InitNodePinTypes(CodeGenerateSystem.Base.ConstructionParams csParam)
        {
            CollectLinkPinInfo(csParam, "ArrayInPin", CodeGenerateSystem.Base.enLinkType.Enumerable, CodeGenerateSystem.Base.enBezierType.Left, CodeGenerateSystem.Base.enLinkOpType.End, false);
            CollectLinkPinInfo(csParam, "ValueInPin", CodeGenerateSystem.Base.enLinkType.All, CodeGenerateSystem.Base.enBezierType.Left, CodeGenerateSystem.Base.enLinkOpType.End, false);
            CollectLinkPinInfo(csParam, "IndexOutPin", typeof(System.Int32), CodeGenerateSystem.Base.enBezierType.Right, CodeGenerateSystem.Base.enLinkOpType.Start, true);
        }
        public override void Save(EngineNS.IO.XndNode xndNode, bool newGuid)
        {
            var att = xndNode.AddAttrib("listAddData");
            att.BeginWrite();
            var typeStr = EngineNS.Rtti.RttiHelper.GetTypeSaveString(ElementType);
            att.Write(typeStr);
            if (mInElementValue == null)
                att.Write("");
            else
                att.Write(mInElementValue.ToString());
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
                att.Read(out mElementValueStr);
                ElementType = EngineNS.Rtti.RttiHelper.GetTypeFromSaveString(typeStr);
                att.EndRead();
            }

            await base.Load(xndNode);
        }

        #region 代码生成

        public override bool Pin_UseOrigionParamName(CodeGenerateSystem.Base.LinkPinControl linkElement)
        {
            if (linkElement == mIndexOutPin)
                return true;
            else
                throw new InvalidOperationException();
        }

        public override string GCode_GetValueName(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            if (element == mIndexOutPin || element == null)
                return $"listFindIndex_" + EngineNS.Editor.Assist.GetValuedGUIDString(Id);
            else
                throw new InvalidOperationException();
        }
        public override string GCode_GetTypeString(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            if (element == mIndexOutPin || element == null)
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
            else if (element == mValueInPin)
            {
                return EngineNS.Rtti.RttiHelper.GetAppTypeString(ElementType);
            }
            throw new InvalidOperationException();
        }
        public override Type GCode_GetType(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            if (element == mIndexOutPin || element == null)
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
            if (element == null || element == mIndexOutPin)
                return new System.CodeDom.CodeVariableReferenceExpression(GCode_GetValueName(null, context));
            else
                throw new InvalidOperationException();
        }

        CodeVariableDeclarationStatement mVariableDeclarationStatement = null;
        public override async System.Threading.Tasks.Task GCode_CodeDom_GenerateCode(CodeTypeDeclaration codeClass, CodeStatementCollection codeStatementCollection, CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            if (mIndexOutPin.HasLink)
            {
                bool exist = false;
                if (mVariableDeclarationStatement != null)
                {
                    foreach (var statement in context.Method.Statements)
                    {
                        var state = statement as CodeVariableDeclarationStatement;
                        if (state != null && state.Name == mVariableDeclarationStatement.Name)
                        {
                            exist = true;
                            break;
                        }
                    }
                }
                if (!exist)
                {
                    var typeStr = GCode_GetTypeString(null, context);
                    mVariableDeclarationStatement = new CodeVariableDeclarationStatement(typeStr, GCode_GetValueName(null, context), new CodeObjectCreateExpression(typeStr));
                    context.Method.Statements.Insert(0, mVariableDeclarationStatement);
                }
            }

            if (mArrayInPin.HasLink)
            {
                var tagExp = mArrayInPin.GetLinkedObject(0, true).GCode_CodeDom_GetValue(mArrayInPin.GetLinkedPinControl(0, true), context);

                if (!mArrayInPin.GetLinkedObject(0, true).IsOnlyReturnValue)
                    await mArrayInPin.GetLinkedObject(0, true).GCode_CodeDom_GenerateCode(codeClass, codeStatementCollection, mArrayInPin.GetLinkedPinControl(0, true), context);
                CodeExpression indexOfInvoke = new CodePrimitiveExpression(0);
                if (mValueInPin.HasLink)
                {
                    if (!mValueInPin.GetLinkedObject(0, true).IsOnlyReturnValue)
                        await mValueInPin.GetLinkedObject(0, true).GCode_CodeDom_GenerateCode(codeClass, codeStatementCollection, mValueInPin.GetLinkedPinControl(0, true), context);
                    indexOfInvoke = new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(
                                                                tagExp,
                                                                "IndexOf",
                                                                mValueInPin.GetLinkedObject(0, true).GCode_CodeDom_GetValue(mValueInPin.GetLinkedPinControl(0, true), context)
                                                                );
                }
                else
                {
                    if (ElementType.IsEnum)
                    {
                        indexOfInvoke = new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(
                                                                tagExp,
                                                                "IndexOf",
                                                                new CodeSnippetExpression(EngineNS.Rtti.RttiHelper.GetAppTypeString(ElementType) + "." + InElementValue.ToString()));
                    }
                    else if (ElementType == typeof(string) || (ElementType.IsValueType && ElementType.IsPrimitive))
                    {
                        try
                        {
                            var val = System.Convert.ChangeType(InElementValue, ElementType);
                            indexOfInvoke = new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(
                                                                tagExp,
                                                                "IndexOf",
                                                                new CodePrimitiveExpression(val));
                        }
                        catch (System.Exception)
                        {

                        }
                    }
                    else
                    {
                        indexOfInvoke = new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(
                                                            tagExp,
                                                            "IndexOf",
                                                            new CodeObjectCreateExpression(ElementType, new CodeExpression[0]));
                    }
                }

                if (mIndexOutPin.HasLink)
                {
                    // 参考代码 var idx = xxx.Count - 1;
                    codeStatementCollection.Add(new CodeAssignStatement(GCode_CodeDom_GetValue(null, context), indexOfInvoke));
                }
                else
                {
                    codeStatementCollection.Add(new CodeExpressionStatement(indexOfInvoke));
                }
            }
        }

        #endregion
    }
}
