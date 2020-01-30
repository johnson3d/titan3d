using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using CodeGenerateSystem.Base;
using EngineNS.IO;

namespace CodeDomNode
{
    [CodeGenerateSystem.CustomConstructionParamsAttribute(typeof(ForeachNodeConstructionParams))]
    public partial class ForeachNode : CodeGenerateSystem.Base.BaseNodeControl
    {
        public class ForeachNodeConstructionParams : CodeGenerateSystem.Base.ConstructionParams
        {
            public enum enLoopValueType
            {
                List,
                Array,
                Dictionary,
                IntPtr,
            }
            public enLoopValueType LoopValueType = enLoopValueType.List;
            public ForeachNodeConstructionParams()
            {
            }
            public override EditorCommon.CodeGenerateSystem.INodeConstructionParams Duplicate()
            {
                var retVal = base.Duplicate() as ForeachNodeConstructionParams;
                retVal.LoopValueType = LoopValueType;
                return retVal;
            }
            public override bool Equals(object obj)
            {
                if (!base.Equals(obj))
                    return false;
                var param = obj as ForeachNodeConstructionParams;
                if (param == null)
                    return false;
                if (LoopValueType == param.LoopValueType)
                    return true;
                return false;
            }
            public override int GetHashCode()
            {
                return (base.GetHashCodeString() + LoopValueType.ToString()).GetHashCode();
            }
            //public override void Write(EngineNS.IO.XndNode xndNode)
            //{
            //    var att = xndNode.AddAttrib("ConstructionParams");
            //    att.Version = 0;
            //    att.BeginWrite();
            //    att.Write(ConstructParam);
            //    var temp = (byte)LoopValueType;
            //    att.Write(temp);
            //    att.EndWrite();
            //}
            //public override void Read(EngineNS.IO.XndNode xndNode)
            //{
            //    var att = xndNode.FindAttrib("ConstructionParams");
            //    if (att != null)
            //    {
            //        att.BeginRead();
            //        switch (att.Version)
            //        {
            //            case 0:
            //                att.Read(out ConstructParam);
            //                byte temp;
            //                att.Read(out temp);
            //                LoopValueType = (enLoopValueType)temp;
            //                break;
            //        }
            //        att.EndRead();
            //    }
            //}
        }

        CodeGenerateSystem.Base.LinkPinControl mCtrlMethodLink_Pre = new CodeGenerateSystem.Base.LinkPinControl();
        CodeGenerateSystem.Base.LinkPinControl mCtrlMethodLink_Completed = new CodeGenerateSystem.Base.LinkPinControl();
        CodeGenerateSystem.Base.LinkPinControl mCtrlMethodLink_LoopBody = new CodeGenerateSystem.Base.LinkPinControl();
        CodeGenerateSystem.Base.LinkPinControl mCtrlArrayElement = new CodeGenerateSystem.Base.LinkPinControl();
        CodeGenerateSystem.Base.LinkPinControl mCtrlArrayIndex = new CodeGenerateSystem.Base.LinkPinControl();
        CodeGenerateSystem.Base.LinkPinControl mCtrlArrayIn = new CodeGenerateSystem.Base.LinkPinControl();
        CodeGenerateSystem.Base.LinkPinControl mCtrlArrayCount = new CodeGenerateSystem.Base.LinkPinControl();

        CodeGenerateSystem.Base.LinkPinControl mCtrlDicKey = new CodeGenerateSystem.Base.LinkPinControl();
        CodeGenerateSystem.Base.LinkPinControl mCtrlDicValue = new CodeGenerateSystem.Base.LinkPinControl();

        Visibility mArrayCountVisibility = Visibility.Collapsed;
        public Visibility ArrayCountVisibility
        {
            get { return mArrayCountVisibility; }
            set
            {
                mArrayCountVisibility = value;
                OnPropertyChanged("ArrayCountVisibility");
            }
        }
        Visibility mDicTypeVisibility = Visibility.Collapsed;
        public Visibility DicTypeVisibility
        {
            get { return mDicTypeVisibility; }
            set
            {
                mDicTypeVisibility = value;
                OnPropertyChanged("DicTypeVisibility");
            }
        }
        Visibility mListTypeVisibility = Visibility.Visible;
        public Visibility ListTypeVisibility
        {
            get { return mListTypeVisibility; }
            set
            {
                mListTypeVisibility = value;
                OnPropertyChanged("ListTypeVisibility");
            }
        }

        UInt32 mCountDefaultValue = 0;
        public UInt32 CountDefaultValue
        {
            get { return mCountDefaultValue; }
            set
            {
                mCountDefaultValue = value;
                OnPropertyChanged("CountDefaultValue");
            }
        }

        Visibility mCountDefaultValueVisible = Visibility.Visible;
        public Visibility CountDefaultValueVisible
        {
            get { return mCountDefaultValueVisible; }
            set
            {
                mCountDefaultValueVisible = value;
                OnPropertyChanged("CountDefaultValueVisible");
            }
        }

        partial void InitConstruction();
        public ForeachNode(CodeGenerateSystem.Base.ConstructionParams csParam)
            : base(csParam)
        {
            var param = csParam as ForeachNodeConstructionParams;

            NodeName = "foreach";
            InitConstruction();
            
            AddLinkPinInfo("CtrlMethodLink_Pre", mCtrlMethodLink_Pre, null);
            AddLinkPinInfo("CtrlMethodLink_Complete", mCtrlMethodLink_Completed, null);
            AddLinkPinInfo("CtrlMethodLink_LoopBody", mCtrlMethodLink_LoopBody, null);

            AddLinkPinInfo("CtrlArrayElement", mCtrlArrayElement, null);
            AddLinkPinInfo("CtrlArrayIndex", mCtrlArrayIndex, null);

            AddLinkPinInfo("CtrlDicKey", mCtrlDicKey, null);
            AddLinkPinInfo("CtrlDicValue", mCtrlDicValue, null);

            var countLinkPin = AddLinkPinInfo("CtrlArrayCount", mCtrlArrayCount, null);
            countLinkPin.OnAddLinkInfo += (info) =>
            {
                CountDefaultValueVisible = Visibility.Collapsed;
            };
            countLinkPin.OnDelLinkInfo += (info) =>
            {
                CountDefaultValueVisible = Visibility.Visible;
            };
            var linkObj = AddLinkPinInfo("CtrlArrayIn", mCtrlArrayIn, null);
            linkObj.OnAddLinkInfo += OnAddLinkInfo;
            linkObj.OnDelLinkInfo += (info) =>
            {
                DicTypeVisibility = Visibility.Collapsed;
                ListTypeVisibility = Visibility.Collapsed;

                //this.HostNodesContainer.RefreshNodeProperty();
            };
        }

        public static void InitNodePinTypes(CodeGenerateSystem.Base.ConstructionParams smParam)
        {
            CollectLinkPinInfo(smParam, "CtrlMethodLink_Pre", CodeGenerateSystem.Base.enLinkType.Method, CodeGenerateSystem.Base.enBezierType.Left, CodeGenerateSystem.Base.enLinkOpType.End, false);
            CollectLinkPinInfo(smParam, "CtrlMethodLink_Complete", CodeGenerateSystem.Base.enLinkType.Method, CodeGenerateSystem.Base.enBezierType.Right, CodeGenerateSystem.Base.enLinkOpType.Start, false);
            CollectLinkPinInfo(smParam, "CtrlMethodLink_LoopBody", CodeGenerateSystem.Base.enLinkType.Method, CodeGenerateSystem.Base.enBezierType.Right, CodeGenerateSystem.Base.enLinkOpType.Start, false);
            CollectLinkPinInfo(smParam, "CtrlArrayElement", CodeGenerateSystem.Base.enLinkType.All, CodeGenerateSystem.Base.enBezierType.Right, CodeGenerateSystem.Base.enLinkOpType.Start, true);
            CollectLinkPinInfo(smParam, "CtrlArrayIndex", CodeGenerateSystem.Base.enLinkType.Int32, CodeGenerateSystem.Base.enBezierType.Right, CodeGenerateSystem.Base.enLinkOpType.Start, true);
            CollectLinkPinInfo(smParam, "CtrlArrayIn", CodeGenerateSystem.Base.enLinkType.Enumerable | CodeGenerateSystem.Base.enLinkType.IntPtr, CodeGenerateSystem.Base.enBezierType.Left, CodeGenerateSystem.Base.enLinkOpType.End, false);
            CollectLinkPinInfo(smParam, "CtrlArrayCount", CodeGenerateSystem.Base.enLinkType.Int32, CodeGenerateSystem.Base.enBezierType.Left, CodeGenerateSystem.Base.enLinkOpType.End, false);

            CollectLinkPinInfo(smParam, "CtrlDicKey", CodeGenerateSystem.Base.enLinkType.All, CodeGenerateSystem.Base.enBezierType.Right, CodeGenerateSystem.Base.enLinkOpType.Start, true);
            CollectLinkPinInfo(smParam, "CtrlDicValue", CodeGenerateSystem.Base.enLinkType.All, CodeGenerateSystem.Base.enBezierType.Right, CodeGenerateSystem.Base.enLinkOpType.Start, true);
        }

        public override void Save(XndNode xndNode, bool newGuid)
        {
            var att = xndNode.AddAttrib("loopData");
            att.BeginWrite();
            att.Write(CountDefaultValue);
            att.EndWrite();

            base.Save(xndNode, newGuid);
        }
        public override async System.Threading.Tasks.Task Load(XndNode xndNode)
        {
            await base.Load(xndNode);

            var att = xndNode.FindAttrib("loopData");
            att.BeginRead();
            att.Read(out mCountDefaultValue);
            att.EndRead();
        }

        string mInputText = "输入";
        public string InputText
        {
            get { return mInputText; }
            set
            {
                mInputText = value;
                OnPropertyChanged("InputText");
            }
        }

        string mCurrentText = "Current";
        public string CurrentText
        {
            get { return mCurrentText; }
            set
            {
                mCurrentText = value;
                OnPropertyChanged("CurrentText");
            }
        }
        string mKeyText = "Key";
        public string KeyText
        {
            get { return mKeyText; }
            set
            {
                mKeyText = value;
                OnPropertyChanged("KeyText");
            }
        }
        string mValueText = "Value";
        public string ValueText
        {
            get { return mValueText; }
            set
            {
                mValueText = value;
                OnPropertyChanged("ValueText");
            }
        }

        Type mElementType;
        Type ElementType
        {
            get { return mElementType; }
            set
            {
                mElementType = value;
                mCtrlArrayElement.LinkType = LinkPinControl.GetLinkTypeFromCommonType(value);
                mCtrlArrayElement.ClassType = value.FullName;
            }
        }
        Type mKeyType;
        Type KeyType
        {
            get { return mKeyType; }
            set
            {
                mKeyType = value;
                mCtrlDicKey.LinkType = LinkPinControl.GetLinkTypeFromCommonType(value);
                mCtrlDicKey.ClassType = value.FullName;
            }
        }
        Type mValueType;
        Type ValueType
        {
            get { return mValueType; }
            set
            {
                mValueType = value;
                mCtrlDicValue.LinkType = LinkPinControl.GetLinkTypeFromCommonType(value);
                mCtrlDicValue.ClassType = value.FullName;
            }
        }
        static List<string> strs = new List<string>()
        {
            "System.Collections.ArrayList",
            "System.Collections.BitArray",
            "System.Collections.Queue",
            "System.Collections.SortedList",
            "System.Collections.Stack",
        };
        void OnAddLinkInfo(CodeGenerateSystem.Base.LinkInfo info)
        {
            var type = info.m_linkFromObjectInfo.HostNodeControl.GCode_GetType(info.m_linkFromObjectInfo, null);
            if (type == null)
            {
                info.Clear();
                return;
            }
            foreach (var i in strs)
            {
                if (type.FullName.IndexOf(i) != -1)
                {
                    ElementType = typeof(Object);
                    DicTypeVisibility = Visibility.Collapsed;
                    ListTypeVisibility = Visibility.Visible;
                    this.HostNodesContainer.RefreshNodeProperty(this, CodeGenerateSystem.Base.ENodeHandleType.AddNodeControl);
                    return;
                }
            }

            var param = CSParam as ForeachNodeConstructionParams;

            ArrayCountVisibility = Visibility.Collapsed;
            if (type.IsPointer)
            {
                ArrayCountVisibility = Visibility.Visible;
                var name = type.FullName.Remove(type.FullName.Length - 1);
                ElementType = type.Assembly.GetType(name);
                DicTypeVisibility = Visibility.Collapsed;
                ListTypeVisibility = Visibility.Visible;
                param.LoopValueType = ForeachNodeConstructionParams.enLoopValueType.IntPtr;
            }
            else if (type.IsArray)
            {
                var name = type.FullName.Substring(0, type.FullName.IndexOf('['));
                ElementType = EngineNS.Rtti.RttiHelper.GetTypeFromTypeFullName(name);
                DicTypeVisibility = Visibility.Collapsed;
                ListTypeVisibility = Visibility.Visible;
                param.LoopValueType = ForeachNodeConstructionParams.enLoopValueType.Array;
            }
            else if (type.IsGenericType)
            {
                if (type.GenericTypeArguments.Length == 1)
                {
                    ElementType = type.GenericTypeArguments[0];
                    DicTypeVisibility = Visibility.Collapsed;
                    ListTypeVisibility = Visibility.Visible;
                    param.LoopValueType = ForeachNodeConstructionParams.enLoopValueType.List;
                }

                if (type.GenericTypeArguments.Length == 2)
                {
                    KeyType = type.GenericTypeArguments[0];
                    ValueType = type.GenericTypeArguments[1];
                    DicTypeVisibility = Visibility.Visible;
                    ListTypeVisibility = Visibility.Collapsed;
                    param.LoopValueType = ForeachNodeConstructionParams.enLoopValueType.Dictionary;
                }
            }
            this.HostNodesContainer.RefreshNodeProperty(this, CodeGenerateSystem.Base.ENodeHandleType.AddNodeControl);
        }

        CodeVariableDeclarationStatement mCountDeclaration;

        public override async System.Threading.Tasks.Task GCode_CodeDom_GenerateCode(CodeTypeDeclaration codeClass, CodeStatementCollection codeStatementCollection, CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            if (element == mCtrlArrayElement || element == mCtrlArrayIndex || element == mCtrlDicKey || element == mCtrlDicValue)
                return;

            var param = CSParam as ForeachNodeConstructionParams;

            if (mCtrlArrayIn.HasLink)
            {
                var valuedGUID = EngineNS.Editor.Assist.GetValuedGUIDString(this.Id);
                var array = "param_" + valuedGUID;
                var current = "current_" + valuedGUID;

                if (!mCtrlArrayIn.GetLinkedObject(0, true).IsOnlyReturnValue)
                    await mCtrlArrayIn.GetLinkedObject(0, true).GCode_CodeDom_GenerateCode(codeClass, codeStatementCollection, mCtrlArrayIn.GetLinkedPinControl(0, true), context);
                var arrayExpression = mCtrlArrayIn.GetLinkedObject(0, true).GCode_CodeDom_GetValue(mCtrlArrayIn.GetLinkedPinControl(0, true), context);

                codeStatementCollection.Add(new CodeVariableDeclarationStatement("var", array, arrayExpression));

                // 调试的代码
                var debugCodes = CodeDomNode.BreakPoint.BeginMacrossDebugCodeStatments(codeStatementCollection);
                CodeDomNode.BreakPoint.BreakCodeStatement(codeClass, debugCodes, HostNodesContainer.GUID, Id);
                CodeDomNode.BreakPoint.EndMacrossDebugCodeStatements(codeStatementCollection, debugCodes);

                var idxStr = $"index_{valuedGUID}";

                string value = "";
                switch(param.LoopValueType)
                {
                    case ForeachNodeConstructionParams.enLoopValueType.Dictionary:
                        value =  "        foreach (var " + current + " in " + array + ")\r\n";
                        value += "        {";
                        codeStatementCollection.Add(new CodeSnippetStatement(value));
                        break;
                    case ForeachNodeConstructionParams.enLoopValueType.IntPtr:
                        {
                            var countStr = $"count_{ valuedGUID}";
                            if (mCountDeclaration == null || !codeStatementCollection.Contains(mCountDeclaration))
                            {
                                var countType = typeof(Int32);
                                if (mCtrlArrayCount.HasLink)
                                {
                                    mCountDeclaration = new CodeVariableDeclarationStatement(countType, countStr);
                                    mCountDeclaration.InitExpression = new CodeGenerateSystem.CodeDom.CodeCastExpression(countType, mCtrlArrayCount.GetLinkedObject(0, true).GCode_CodeDom_GetValue(mCtrlArrayCount.GetLinkedPinControl(0, true), context));
                                    codeStatementCollection.Add(mCountDeclaration);
                                }
                                else
                                {
                                    mCountDeclaration = new CodeVariableDeclarationStatement(countType, countStr);
                                    mCountDeclaration.InitExpression = new CodeGenerateSystem.CodeDom.CodeCastExpression(countType, new CodePrimitiveExpression(CountDefaultValue));
                                    codeStatementCollection.Add(mCountDeclaration);
                                }
                            }

                            value = $"        for (int {idxStr} = 0; {idxStr} < {countStr}; {idxStr}++)";
                            value += "        {";
                            codeStatementCollection.Add(new CodeSnippetStatement(value));
                            codeStatementCollection.Add(new CodeVariableDeclarationStatement(ElementType, current, new CodeVariableReferenceExpression($"{array}[{idxStr}]")));
                        }
                        break;
                    case ForeachNodeConstructionParams.enLoopValueType.Array:
                        value = $"        for (int {idxStr} = 0; {idxStr} < {array}.Length; {idxStr}++)";
                        value += "        {";
                        codeStatementCollection.Add(new CodeSnippetStatement(value));
                        codeStatementCollection.Add(new CodeVariableDeclarationStatement(ElementType, current, new CodeVariableReferenceExpression($"{array}[{idxStr}]")));
                        break;
                    case ForeachNodeConstructionParams.enLoopValueType.List:
                        value = $"        for (int {idxStr} = 0; {idxStr} < {array}.Count; {idxStr}++)";
                        value += "        {";
                        codeStatementCollection.Add(new CodeSnippetStatement(value));
                        codeStatementCollection.Add(new CodeVariableDeclarationStatement(ElementType, current, new CodeVariableReferenceExpression($"{array}[{idxStr}]")));
                        break;
                }


                if (mCtrlMethodLink_LoopBody.HasLink)
                {
                    //if (!mCtrlMethodLink_LoopBody.GetLinkedObject(0, false).IsOnlyReturnValue)
                        await mCtrlMethodLink_LoopBody.GetLinkedObject(0, false).GCode_CodeDom_GenerateCode(codeClass, codeStatementCollection, mCtrlMethodLink_LoopBody.GetLinkedPinControl(0, false), context);
                }

                codeStatementCollection.Add(new CodeSnippetStatement("        }"));

                if (context.GenerateNext)
                {
                    if (mCtrlMethodLink_Completed.HasLink)
                    {
                        //if (!mCtrlMethodLink_Completed.GetLinkedObject(0, false).IsOnlyReturnValue)
                            await mCtrlMethodLink_Completed.GetLinkedObject(0, false).GCode_CodeDom_GenerateCode(codeClass, codeStatementCollection, mCtrlMethodLink_Completed.GetLinkedPinControl(0, false), context);
                    }
                }
            }
        }

        public override CodeExpression GCode_CodeDom_GetValue(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context, CodeGenerateSystem.Base.GenerateCodeContext_PreNode preNodeContext = null)
        {
            string current = "";
            if(preNodeContext?.NeedDereferencePoint == true && ElementType.IsPointer)
                current = $"(*current_{EngineNS.Editor.Assist.GetValuedGUIDString(this.Id)})";
            else
                current = "current_" + EngineNS.Editor.Assist.GetValuedGUIDString(this.Id);
            if (element == mCtrlArrayElement)
            {
                return new CodeSnippetExpression(current);
            }
            else if(element == mCtrlArrayIndex)
            {
                return new CodeSnippetExpression($"index_{EngineNS.Editor.Assist.GetValuedGUIDString(this.Id)}");
            }
            else if (element == mCtrlDicKey)
            {
                current += ".Key";
                return new CodeSnippetExpression(current);
            }
            else if (element == mCtrlDicValue)
            {
                current += ".Value";
                return new CodeSnippetExpression(current);
            }
            return base.GCode_CodeDom_GetValue(element, context);
        }

        public override Type GCode_GetType(LinkPinControl element, GenerateCodeContext_Method context)
        {
            if (element == mCtrlArrayElement)
            {
                return ElementType;
            }
            else if (element == mCtrlArrayIndex)
            {
                return typeof(int);
            }
            else if (element == mCtrlDicKey)
                return KeyType;
            else if (element == mCtrlDicValue)
                return ElementType;
            return base.GCode_GetType(element, context);
        }
        public override string GCode_GetTypeString(LinkPinControl element, GenerateCodeContext_Method context)
        {
            if(element == mCtrlArrayElement)
            {
                return EngineNS.Rtti.RttiHelper.GetAppTypeString(ElementType);
            }
            else if(element == mCtrlArrayIndex)
            {
                return EngineNS.Rtti.RttiHelper.GetAppTypeString(typeof(int));
            }
            else if (element == mCtrlDicKey)
                return EngineNS.Rtti.RttiHelper.GetAppTypeString(KeyType);
            else if (element == mCtrlDicValue)
                return EngineNS.Rtti.RttiHelper.GetAppTypeString(ElementType);

            return base.GCode_GetTypeString(element, context);
        }
    }
}
