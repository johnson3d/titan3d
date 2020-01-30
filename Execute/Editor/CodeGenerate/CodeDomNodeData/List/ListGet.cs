using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace CodeDomNode
{
    [CodeGenerateSystem.CustomConstructionParams(typeof(ListGetConstructionParams))]
    public partial class ListGet : CodeGenerateSystem.Base.BaseNodeControl
    {
        [EngineNS.Rtti.MetaClass]
        public class ListGetConstructionParams : CodeGenerateSystem.Base.ConstructionParams
        {
            [EngineNS.Rtti.MetaData]
            public bool IsGetRef
            {
                get;
                set;
            } = false;

            public ListGetConstructionParams()
            {
            }
            public override EditorCommon.CodeGenerateSystem.INodeConstructionParams Duplicate()
            {
                var retVal = base.Duplicate() as ListGetConstructionParams;
                retVal.IsGetRef = IsGetRef;
                return retVal;
            }
            public override bool Equals(object obj)
            {
                if (!base.Equals(obj))
                    return false;
                var param = obj as ListGetConstructionParams;
                if (param == null)
                    return false;
                if (param.IsGetRef == IsGetRef)
                    return true;

                return false;
            }
            public override int GetHashCode()
            {
                return (GetHashCodeString() + IsGetRef.ToString()).GetHashCode();
            }
            //public override void Write(EngineNS.IO.XndNode xndNode)
            //{
            //    var att = xndNode.AddAttrib("ConstructionParams");
            //    att.Version = 0;
            //    att.BeginWrite();
            //    att.Write(ConstructParam);
            //    att.Write(IsGetRef);
            //    att.EndWrite();
            //}
            public override void Read(EngineNS.IO.XndNode xndNode)
            {
                var att = xndNode.FindAttrib("ConstructionParams");
                if (att != null)
                {
                    att.BeginRead();
                    switch (att.Version)
                    {
                        case 0:
                            att.Read(out mConstructParam);
                            bool getRet;
                            att.Read(out getRet);
                            IsGetRef = getRet;
                            break;
                        case 1:
                            att.ReadMetaObject(this);
                            break;
                    }
                    att.EndRead();
                }
            }
        }

        CodeGenerateSystem.Base.LinkPinControl mArrayInPin = new CodeGenerateSystem.Base.LinkPinControl();
        CodeGenerateSystem.Base.LinkPinControl mIndexInPin = new CodeGenerateSystem.Base.LinkPinControl();
        CodeGenerateSystem.Base.LinkPinControl mValueOutPin = new CodeGenerateSystem.Base.LinkPinControl();

        Type mElementType = typeof(object);
        public Type ElementType
        {
            get => mElementType;
            set
            {
                if (mElementType == value)
                    return;
                mElementType = value;

                if (mArrayInPin != null)
                {
                    if (mElementType == null)
                        mArrayInPin.ClassType = "";
                    else
                        mArrayInPin.ClassType = $"System.Collections.Generic.List`1[[{mElementType.FullName}, {mElementType.Assembly.FullName}]]";
                }
                if (mValueOutPin != null)
                {
                    if (mElementType == null || mElementType == typeof(object))
                    {
                        mValueOutPin.LinkType = CodeGenerateSystem.Base.enLinkType.All;
                        mValueOutPin.ClassType = "";
                    }
                    else
                    {
                        mValueOutPin.LinkType = CodeGenerateSystem.Base.LinkPinControl.GetLinkTypeFromCommonType(mElementType);
                        mValueOutPin.ClassType = mElementType.FullName;
                    }
                }

                OnElementTypeChanged_WPF();
            }
        }
        partial void OnElementTypeChanged_WPF();
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
        public ListGet(CodeGenerateSystem.Base.ConstructionParams csParam)
            : base(csParam)
        {
            NodeName = "Get";
            InitConstruction();

            var arrayInPinInfo = AddLinkPinInfo("ArrayInPin", mArrayInPin);
            arrayInPinInfo.OnAddLinkInfo += ArrayInPin_OnAddLinkInfo;
            arrayInPinInfo.OnDelLinkInfo += ArrayInPin_OnDelLinkInfo;
            AddLinkPinInfo("IndexInPin", mIndexInPin);
            var valueOutPinInfo = AddLinkPinInfo("ValueOutPin", mValueOutPin);
            valueOutPinInfo.OnAddLinkInfo += ValueOutPinInfo_OnAddLinkInfo;
            valueOutPinInfo.OnDelLinkInfo += ValueOutPinInfo_OnDelLinkInfo;
        }
        public static void InitNodePinTypes(CodeGenerateSystem.Base.ConstructionParams csParam)
        {
            CollectLinkPinInfo(csParam, "ArrayInPin", CodeGenerateSystem.Base.enLinkType.Enumerable, CodeGenerateSystem.Base.enBezierType.Left, CodeGenerateSystem.Base.enLinkOpType.End, false);
            CollectLinkPinInfo(csParam, "IndexInPin", typeof(System.Int32), CodeGenerateSystem.Base.enBezierType.Left, CodeGenerateSystem.Base.enLinkOpType.End, false);
            CollectLinkPinInfo(csParam, "ValueOutPin", CodeGenerateSystem.Base.enLinkType.All, CodeGenerateSystem.Base.enBezierType.Right, CodeGenerateSystem.Base.enLinkOpType.Start, true);
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
            if (mValueOutPin.HasLink)
            {
                ElementType = mValueOutPin.GetLinkedObject(0, false).GCode_GetType(mValueOutPin.GetLinkedPinControl(0, false), null);
            }
            else
                ElementType = typeof(object);
        }
        private void ValueOutPinInfo_OnAddLinkInfo(CodeGenerateSystem.Base.LinkInfo linkInfo)
        {
            var elmType = linkInfo.m_linkFromObjectInfo.GetLinkedObject(0, true).GCode_GetType(linkInfo.m_linkFromObjectInfo.GetLinkedPinControl(0, true), null);
            if (ElementType == null || ElementType == typeof(object))
                ElementType = elmType;
        }
        private void ValueOutPinInfo_OnDelLinkInfo(CodeGenerateSystem.Base.LinkInfo linkInfo)
        {
            if (!mArrayInPin.HasLink)
                ElementType = typeof(object);
        }
        public override void Save(EngineNS.IO.XndNode xndNode, bool newGuid)
        {
            var att = xndNode.AddAttrib("listInsertData");
            att.BeginWrite();
            var typeStr = EngineNS.Rtti.RttiHelper.GetTypeSaveString(ElementType);
            att.Write(typeStr);
            att.Write(IndexValue);
            att.EndWrite();

            base.Save(xndNode, newGuid);
        }
        public override async System.Threading.Tasks.Task Load(EngineNS.IO.XndNode xndNode)
        {
            var att = xndNode.FindAttrib("listInsertData");
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
        public override bool Pin_UseOrigionParamName(CodeGenerateSystem.Base.LinkPinControl linkElement)
        {
            if (linkElement == mValueOutPin)
                return true;
            else
                throw new InvalidOperationException();
        }

        public override string GCode_GetValueName(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            if(element == null || element == mValueOutPin)
                return $"listGetValue_" + EngineNS.Editor.Assist.GetValuedGUIDString(Id);
            else
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
            else if (element == mValueOutPin || element == null)
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
            else if (element == mValueOutPin || element == null)
            {
                return ElementType;
            }
            throw new InvalidOperationException();
        }
        public override CodeExpression GCode_CodeDom_GetValue(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context, CodeGenerateSystem.Base.GenerateCodeContext_PreNode preNodeContext = null)
        {
            if (element == null || element == mValueOutPin)
                return new CodeVariableReferenceExpression(GCode_GetValueName(null, context));
            else
                throw new InvalidOperationException();
        }

        CodeVariableDeclarationStatement mVariableDeclarationStatement = null;
        public override async System.Threading.Tasks.Task GCode_CodeDom_GenerateCode(CodeTypeDeclaration codeClass, CodeStatementCollection codeStatementCollection, CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            if (mValueOutPin.HasLink)
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
                    mVariableDeclarationStatement = new CodeVariableDeclarationStatement(typeStr, GCode_GetValueName(null, context), CodeGenerateSystem.Program.GetDefaultValueExpressionFromType(GCode_GetType(null, context)));
                    context.Method.Statements.Insert(0, mVariableDeclarationStatement);
                }
            }

            if (mArrayInPin.HasLink)
            {
                var tagExp = mArrayInPin.GetLinkedObject(0, true).GCode_CodeDom_GetValue(mArrayInPin.GetLinkedPinControl(0, true), context);

                if (!mArrayInPin.GetLinkedObject(0, true).IsOnlyReturnValue)
                    await mArrayInPin.GetLinkedObject(0, true).GCode_CodeDom_GenerateCode(codeClass, codeStatementCollection, mArrayInPin.GetLinkedPinControl(0, true), context);
                CodeExpression indexInValueExp;
                if (mIndexInPin.HasLink)
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
                condExp.TrueStatements.Add(new CodeAssignStatement(GCode_CodeDom_GetValue(null, context), new CodeArrayIndexerExpression(tagExp, indexInValueExp)));
            }
        }

        #endregion

    }
}
