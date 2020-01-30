using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using EngineNS.IO;

namespace CodeDomNode
{
    [EngineNS.Rtti.MetaClass]
    public class MakeListConstructParam : CodeGenerateSystem.Base.ConstructionParams
    {

    }
    [CodeGenerateSystem.CustomConstructionParams(typeof(MakeListConstructParam))]
    public partial class MakeList : CodeGenerateSystem.Base.BaseNodeControl
    {
        CodeGenerateSystem.Base.LinkPinControl mArrayOutPin = new CodeGenerateSystem.Base.LinkPinControl();
        Type mOrigionType = typeof(object);
        public Type OrigionType
        {
            get => mOrigionType;
            set
            {
                if (mOrigionType == value)
                    return;
                mOrigionType = value ?? throw new InvalidOperationException();

                mArrayOutPin.ClassType = $"System.Collections.Generic.List`1[[{OrigionType.FullName}, {OrigionType.Assembly.FullName}]]";

                foreach (var child in mChildNodes)
                {
                    var elm = child as MakeListElement;
                    if (elm != null)
                    {
                        elm.ArrayTypeBrush = ArrayTypeBrush;
                        elm.OrigionType = OrigionType;
                    }
                }

                OnOrigionTypeChanged_WPF();
            }
        }
        partial void OnOrigionTypeChanged_WPF();

        partial void InitConstruction();
        public MakeList(CodeGenerateSystem.Base.ConstructionParams csParam)
            : base(csParam)
        {
            NodeName = "Make List";
            InitConstruction();
            var pin = AddLinkPinInfo("ArrayOut", mArrayOutPin, null);
            pin.OnAddLinkInfo += ArrayOut_OnAddLinkInfo;
            pin.OnDelLinkInfo += ArrayOut_OnDelLinkInfo;
        }
        public static void InitNodePinTypes(CodeGenerateSystem.Base.ConstructionParams smParam)
        {
            CollectLinkPinInfo(smParam, "ArrayOut", CodeGenerateSystem.Base.enLinkType.Enumerable, CodeGenerateSystem.Base.enBezierType.Right, CodeGenerateSystem.Base.enLinkOpType.Start, true);
        }

        void ArrayOut_OnAddLinkInfo(CodeGenerateSystem.Base.LinkInfo linkInfo)
        {
            var type = GetElementTypeFromArrayLink();
            if(type != null && type != typeof(object))
            {
                OrigionType = type;
                foreach (var child in mChildNodes)
                {
                    var elm = child as MakeListElement;
                    if (elm != null)
                    {
                        //elm.ArrayTypeBrush = 
                        elm.OrigionType = OrigionType;
                    }
                }
            }
        }
        void ArrayOut_OnDelLinkInfo(CodeGenerateSystem.Base.LinkInfo linkInfo)
        {
            var count = ElementHasLinkCount();
            if (count > 0)
                OrigionType = GetElementTypeFromElementChildren();
            else
                OrigionType = typeof(object);
        }

        public void UpdateWhenElementPinLinkChange(bool onAdd, MakeListElement element)
        {
            var count = ElementHasLinkCount();
            if (count == 1 && onAdd)
            {
                if (!mArrayOutPin.HasLink || OrigionType == typeof(object))
                {
                    OrigionType = element.OrigionType;
                }
                ArrayTypeBrush = element.GetLinkTargetBrush();
            }
            else if (count == 1 && !onAdd)
            {
                if (!mArrayOutPin.HasLink)
                {
                    OrigionType = typeof(object);
                    ArrayTypeBrush = System.Windows.Media.Brushes.Gray;
                    //foreach (var child in mChildNodes)
                    //{
                    //    var elm = child as MakeArrayElement;
                    //    if (elm != null)
                    //    {
                    //        elm.ArrayTypeBrush = ArrayTypeBrush;
                    //        elm.OrigionType = typeof(object);
                    //    }
                    //}
                }
            }
        }

        public override void Save(XndNode xndNode, bool newGuid)
        {
            var att = xndNode.AddAttrib("makeListData");
            att.BeginWrite();
            var typeStr = EngineNS.Rtti.RttiHelper.GetTypeSaveString(mOrigionType);
            att.Write(typeStr);
            att.EndWrite();
            base.Save(xndNode, newGuid);
        }
        public override async System.Threading.Tasks.Task Load(XndNode xndNode)
        {
            var att = xndNode.FindAttrib("makeListData");
            if(att != null)
            {
                att.BeginRead();
                string typeStr;
                att.Read(out typeStr);
                OrigionType = EngineNS.Rtti.RttiHelper.GetTypeFromSaveString(typeStr);
                att.EndRead();
            }

            await base.Load(xndNode);
        }

        #region 代码生成

        Type GetElementTypeFromArrayLink()
        {
            if (!mArrayOutPin.HasLink)
                return typeof(object);

            Type arrayType = null;
            for(int i=0; i< mArrayOutPin.GetLinkInfosCount(); i++)
            {
                arrayType = mArrayOutPin.GetLinkedObject(i, false).GCode_GetType(mArrayOutPin.GetLinkedPinControl(i, false), null);
                if (arrayType == null || arrayType == typeof(object) ||
                    arrayType == typeof(System.Collections.Generic.List<System.Object>))
                    continue;
                else
                    break;
            }
            if (arrayType == null || arrayType == typeof(object))
                return typeof(object);
            if (arrayType.IsGenericType)
            {
                var types = arrayType.GenericTypeArguments;
                if (types.Length > 0)
                    return types[0];
                return typeof(object);
            }
            else if (arrayType.IsArray)
                return arrayType.GetElementType();
            else
                return typeof(object);
        }
        Type GetElementTypeFromElementChildren()
        {
            Type[] types = new Type[mChildNodes.Count];
            for(int i=0; i<mChildNodes.Count; i++)
            {
                var elm = mChildNodes[i] as MakeListElement;
                if (elm == null)
                    continue;

                types[i] = elm.GCode_GetType(null, null);
            }
            Type retType = typeof(object);
            foreach(var type in types)
            {
                if (retType == null || retType == typeof(object))
                    retType = type;
                else if (retType == type)
                    continue;
                else
                {
                    var tempType = type;
                    do
                    {
                        if(retType.IsSubclassOf(tempType))
                        {
                            retType = tempType;
                            break;
                        }
                        tempType = tempType.BaseType;
                        if (tempType == retType)
                            break;
                    }
                    while (true);
                }
            }
            return retType;
        }
        public override string GCode_GetValueName(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            return "makedList_" + EngineNS.Editor.Assist.GetValuedGUIDString(Id);
        }
        public override string GCode_GetTypeString(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            if (element == null || element == mArrayOutPin)
            {
                //var pinInfo = GetLinkPinInfo(mArrayOutPin); 
                //if(pinInfo.HasLink && element == null)
                //    return $"System.Collections.Generic.List<{EngineNS.Rtti.RttiHelper.GetAppTypeString(GetElementTypeFromArrayLink())}>";
                //else
                //    return $"System.Collections.Generic.List<{EngineNS.Rtti.RttiHelper.GetAppTypeString(GetElementTypeFromElementChildren())}>";
                return $"System.Collections.Generic.List<{EngineNS.Rtti.RttiHelper.GetAppTypeString(mOrigionType)}>";
            }
            else
                throw new InvalidOperationException();
        }
        public override Type GCode_GetType(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            if (element == null || element == mArrayOutPin)
            {
                string typeStr = $"System.Collections.Generic.List`1[[{mOrigionType.FullName}, {mOrigionType.Assembly.FullName}]]";
                //var pinInfo = GetLinkPinInfo(mArrayOutPin);
                //if (pinInfo.HasLink && element == null)
                //{
                //    var type = GetElementTypeFromArrayLink();
                //    typeStr = $"System.Collections.Generic.List`1[[{type.FullName}, {type.Assembly.FullName}]]";
                //}
                //else
                //{
                //    var type = GetElementTypeFromElementChildren();
                //    typeStr = $"System.Collections.Generic.List`1[[{type.FullName}, {type.Assembly.FullName}]]";
                //}
                return EngineNS.Rtti.RttiHelper.GetTypeFromTypeFullName(typeStr, CSParam.CSType);
            }
            else
                throw new InvalidOperationException();
        }
        public override CodeExpression GCode_CodeDom_GetValue(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context, CodeGenerateSystem.Base.GenerateCodeContext_PreNode preNodeContext = null)
        {
            return new System.CodeDom.CodeVariableReferenceExpression(GCode_GetValueName(null, context));
        }

        CodeVariableDeclarationStatement mVariableDeclarationStatement = null;
        public override async System.Threading.Tasks.Task GCode_CodeDom_GenerateCode(CodeTypeDeclaration codeClass, CodeStatementCollection codeStatementCollection, CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            bool exist = false;
            if(mVariableDeclarationStatement != null)
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

                for (int i = 0; i < mChildNodes.Count; i++)
                {
                    var elm = mChildNodes[i] as MakeListElement;
                    if (elm == null)
                        continue;
                    await elm.GCode_CodeDom_GenerateCode(codeClass, codeStatementCollection, null, context);
                }
            }
        }

        public override bool Pin_UseOrigionParamName(CodeGenerateSystem.Base.LinkPinControl linkElement)
        {
            if (linkElement == mArrayOutPin)
                return true;
            else
                throw new InvalidOperationException();
        }

        #endregion
    }

    [EngineNS.Rtti.MetaClass]
    public class MakeListElementConstructParam : CodeGenerateSystem.Base.ConstructionParams
    {

    }
    [CodeGenerateSystem.CustomConstructionParams(typeof(MakeListElementConstructParam))]
    public partial class MakeListElement : CodeGenerateSystem.Base.BaseNodeControl
    {
        CodeGenerateSystem.Base.LinkPinControl mElemPin = new CodeGenerateSystem.Base.LinkPinControl();

        object mElementValue;
        public object ElementValue
        {
            get => mElementValue;
            set
            {
                mElementValue = value;
                OnPropertyChanged("ElementValue");
            }
        }
        string mElementValueStr;

        int mElementIdx;
        public int ElementIdx
        {
            get => mElementIdx;
            set
            {
                mElementIdx = value;
                if(mElemPin != null)
                    mElemPin.NameString = "[" + value + "]";
                OnPropertyChanged("ElementIdx");
            }
        }

        Type mOrigionType;
        public Type OrigionType
        {
            get => mOrigionType;
            set
            {
                mOrigionType = value;
                if(mOrigionType != null && mOrigionType != typeof(object) && !string.IsNullOrEmpty(mElementValueStr))
                {
                    if (mOrigionType.IsEnum)
                        ElementValue = EngineNS.Rtti.RttiHelper.EnumTryParse(mOrigionType, mElementValueStr);
                    else
                        ElementValue = System.Convert.ChangeType(mElementValueStr, mOrigionType);
                }

                if (mElemPin != null)
                {
                    mElemPin.ClassType = mOrigionType.FullName;
                    mElemPin.LinkType = CodeGenerateSystem.Base.LinkPinControl.GetLinkTypeFromCommonType(mOrigionType);
                }

                OnOrigionTypeChanged();
            }
        }
        partial void OnOrigionTypeChanged();
        partial void InitConstruction();
        public MakeListElement(CodeGenerateSystem.Base.ConstructionParams csParam)
            : base(csParam)
        {
            NodeType = enNodeType.ChildNode;
            OrigionType = typeof(object);
            InitConstruction();

            var linkInfo = AddLinkPinInfo("ElemPin", mElemPin, null);
            linkInfo.OnAddLinkInfo += LinkInfo_OnAddLinkInfo;
            linkInfo.OnDelLinkInfo += LinkInfo_OnDelLinkInfo;
        }

        public bool HasLink()
        {
            return mElemPin.HasLink;
        }

        public static void InitNodePinTypes(CodeGenerateSystem.Base.ConstructionParams csParam)
        {
            CollectLinkPinInfo(csParam, "ElemPin", CodeGenerateSystem.Base.enLinkType.All, CodeGenerateSystem.Base.enBezierType.Left, CodeGenerateSystem.Base.enLinkOpType.End, false);
        }

        void LinkInfo_OnAddLinkInfo(CodeGenerateSystem.Base.LinkInfo linkInfo)
        {
            var par = ParentNode as MakeList;
            OrigionType = linkInfo.m_linkFromObjectInfo.HostNodeControl.GCode_GetType(linkInfo.m_linkFromObjectInfo, null);
            par?.UpdateWhenElementPinLinkChange(true, this);
            OrigionType = par.OrigionType;
        }
        void LinkInfo_OnDelLinkInfo(CodeGenerateSystem.Base.LinkInfo linkInfo)
        {
            var par = ParentNode as MakeList;
            par?.UpdateWhenElementPinLinkChange(false, this);
            OrigionType = par.OrigionType;
        }

        public override void Save(XndNode xndNode, bool newGuid)
        {
            var att = xndNode.AddAttrib("makeListData");
            att.Version = 1;
            att.BeginWrite();
            att.Write(ElementIdx);
            if (mElementValue == null)
                att.Write("");
            else
                att.Write(mElementValue.ToString());
            var typeStr = EngineNS.Rtti.RttiHelper.GetTypeSaveString(mOrigionType);
            att.Write(typeStr);
            att.EndWrite();
            base.Save(xndNode, newGuid);
        }
        public override async System.Threading.Tasks.Task Load(XndNode xndNode)
        {
            await base.Load(xndNode);
            var att = xndNode.FindAttrib("makeListData");
            if(att != null)
            {
                att.BeginRead();
                switch(att.Version)
                {
                    case 0:
                        {
                            att.Read(out mElementIdx);
                            ElementIdx = mElementIdx;
                            att.Read(out mElementValueStr);
                        }
                        break;
                    case 1:
                        {
                            att.Read(out mElementIdx);
                            ElementIdx = mElementIdx;
                            att.Read(out mElementValueStr);
                            string typeStr;
                            att.Read(out typeStr);
                            OrigionType = EngineNS.Rtti.RttiHelper.GetTypeFromSaveString(typeStr);
                        }
                        break;
                }
                att.EndRead();
            }
        }

        #region 代码生成

        public override Type GCode_GetType(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            if (mElemPin.HasLink)
            {
                return mElemPin.GetLinkedObject(0, true).GCode_GetType(mElemPin.GetLinkedPinControl(0, true), context);
            }
            else
                return OrigionType;
        }

        public override async System.Threading.Tasks.Task GCode_CodeDom_GenerateCode(CodeTypeDeclaration codeClass, CodeStatementCollection codeStatementCollection, CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            if(mElemPin.HasLink)
            {
                var linkObj = mElemPin.GetLinkedObject(0, true);
                if (!linkObj.IsOnlyReturnValue)
                    await linkObj.GCode_CodeDom_GenerateCode(codeClass, codeStatementCollection, mElemPin.GetLinkedPinControl(0, true), context);

                codeStatementCollection.Add(new CodeExpressionStatement(new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(
                                                    new CodeVariableReferenceExpression(ParentNode.GCode_GetValueName(null, context)),
                                                    "Add", 
                                                    linkObj.GCode_CodeDom_GetValue(mElemPin.GetLinkedPinControl(0, true), context))));
            }
            else
            {
                if(OrigionType.IsEnum)
                {
                    codeStatementCollection.Add(new CodeExpressionStatement(new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(
                                    new CodeVariableReferenceExpression(ParentNode.GCode_GetValueName(null, context)),
                                    "Add",
                                    new CodeSnippetExpression(EngineNS.Rtti.RttiHelper.GetAppTypeString(OrigionType) + "." + ElementValue.ToString()))));
                }
                else if(OrigionType == typeof(string) ||
                   (OrigionType.IsValueType && OrigionType.IsPrimitive))
                {
                    try
                    {
                        var val = System.Convert.ChangeType(ElementValue, OrigionType);
                        codeStatementCollection.Add(new CodeExpressionStatement(new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(
                                                            new CodeVariableReferenceExpression(ParentNode.GCode_GetValueName(null, context)),
                                                            "Add",
                                                            new CodePrimitiveExpression(val))));
                    }
                    catch(System.Exception)
                    {

                    }
                }
                else
                {
                    codeStatementCollection.Add(new CodeExpressionStatement(new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(
                                                        new CodeVariableReferenceExpression(ParentNode.GCode_GetValueName(null, context)),
                                                        "Add",
                                                        new CodeObjectCreateExpression(mOrigionType, new CodeExpression[0]))));
                }
            }
        }

        #endregion
    }
}
