using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using EngineNS.IO;

namespace CodeDomNode
{
    public class ForLoopConstructionParams : CodeGenerateSystem.Base.ConstructionParams
    {

    }

    [CodeGenerateSystem.CustomConstructionParamsAttribute(typeof(ForLoopConstructionParams))]
    public partial class ForLoopNode : CodeGenerateSystem.Base.BaseNodeControl
    {
        CodeGenerateSystem.Base.LinkPinControl mCtrlMethodLink_Pre = new CodeGenerateSystem.Base.LinkPinControl();
        CodeGenerateSystem.Base.LinkPinControl mCtrlMethodLink_Completed = new CodeGenerateSystem.Base.LinkPinControl();
        CodeGenerateSystem.Base.LinkPinControl mCtrlMethodLink_LoopBody = new CodeGenerateSystem.Base.LinkPinControl();
        CodeGenerateSystem.Base.LinkPinControl mCtrlIndex = new CodeGenerateSystem.Base.LinkPinControl();
        CodeGenerateSystem.Base.LinkPinControl mCtrlIndexFirst = new CodeGenerateSystem.Base.LinkPinControl();
        CodeGenerateSystem.Base.LinkPinControl mCtrlIndexLast = new CodeGenerateSystem.Base.LinkPinControl();

        Int32 mFirstIndex = 0;
        public Int32 FirstIndex
        {
            get => mFirstIndex;
            set
            {
                mFirstIndex = value;
                OnPropertyChanged("FirstIndex");
            }
        }
        Visibility mFirstIndexVisibility = Visibility.Visible;
        public Visibility FirstIndexVisibility
        {
            get => mFirstIndexVisibility;
            set
            {
                mFirstIndexVisibility = value;
                OnPropertyChanged("FirstIndexVisibility");
            }
        }
        Int32 mLastIndex = 0;
        public Int32 LastIndex
        {
            get => mLastIndex;
            set
            {
                mLastIndex = value;
                OnPropertyChanged("LastIndex");
            }
        }
        Visibility mLastIndexVisibility = Visibility.Visible;
        public Visibility LastIndexVisibility
        {
            get => mLastIndexVisibility;
            set
            {
                mLastIndexVisibility = value;
                OnPropertyChanged("LastIndexVisibility");
            }
        }

        partial void InitConstruction();
        public ForLoopNode(CodeGenerateSystem.Base.ConstructionParams csParam)
            : base(csParam)
        {
            NodeName = "for";
            InitConstruction();

            AddLinkPinInfo("CtrlMethodLink_Pre", mCtrlMethodLink_Pre, null);
            AddLinkPinInfo("CtrlMethodLink_Completed", mCtrlMethodLink_Completed, null);
            AddLinkPinInfo("CtrlMethodLink_LoopBody", mCtrlMethodLink_LoopBody, null);
            AddLinkPinInfo("CtrlIndex", mCtrlIndex, null);
            var idxFirstPin = AddLinkPinInfo("CtrlIndexFirst", mCtrlIndexFirst, null);
            idxFirstPin.OnAddLinkInfo += (linkInfo) =>
            {
                FirstIndexVisibility = Visibility.Collapsed;
            };
            idxFirstPin.OnDelLinkInfo += (linkInfo) =>
            {
                FirstIndexVisibility = Visibility.Visible;
            };
            var idxLastPin = AddLinkPinInfo("CtrlIndexLast", mCtrlIndexLast, null);
            idxLastPin.OnAddLinkInfo += (linkInfo) =>
            {
                LastIndexVisibility = Visibility.Collapsed;
            };
            idxLastPin.OnDelLinkInfo += (linkInfo) =>
            {
                LastIndexVisibility = Visibility.Visible;
            };
        }

        public static void InitNodePinTypes(CodeGenerateSystem.Base.ConstructionParams smParam)
        {
            CollectLinkPinInfo(smParam, "CtrlMethodLink_Pre", CodeGenerateSystem.Base.enLinkType.Method, CodeGenerateSystem.Base.enBezierType.Left, CodeGenerateSystem.Base.enLinkOpType.End, false);
            CollectLinkPinInfo(smParam, "CtrlMethodLink_Completed", CodeGenerateSystem.Base.enLinkType.Method, CodeGenerateSystem.Base.enBezierType.Right, CodeGenerateSystem.Base.enLinkOpType.Start, false);
            CollectLinkPinInfo(smParam, "CtrlMethodLink_LoopBody", CodeGenerateSystem.Base.enLinkType.Method, CodeGenerateSystem.Base.enBezierType.Right, CodeGenerateSystem.Base.enLinkOpType.Start, false);
            CollectLinkPinInfo(smParam, "CtrlIndex", CodeGenerateSystem.Base.enLinkType.Int32, CodeGenerateSystem.Base.enBezierType.Right, CodeGenerateSystem.Base.enLinkOpType.Start, true);
            CollectLinkPinInfo(smParam, "CtrlIndexFirst", CodeGenerateSystem.Base.enLinkType.Int32, CodeGenerateSystem.Base.enBezierType.Left, CodeGenerateSystem.Base.enLinkOpType.End, false);
            CollectLinkPinInfo(smParam, "CtrlIndexLast", CodeGenerateSystem.Base.enLinkType.Int32, CodeGenerateSystem.Base.enBezierType.Left, CodeGenerateSystem.Base.enLinkOpType.End, false);
        }

        public override void Save(XndNode xndNode, bool newGuid)
        {
            var att = xndNode.AddAttrib("loopData");
            att.BeginWrite();
            att.Write(FirstIndex);
            att.Write(LastIndex);
            att.EndWrite();

            base.Save(xndNode, newGuid);
        }
        public override async Task Load(XndNode xndNode)
        {
            await base.Load(xndNode);

            var att = xndNode.FindAttrib("loopData");
            att.BeginRead();
            att.Read(out mFirstIndex);
            OnPropertyChanged("FirstIndex");
            att.Read(out mLastIndex);
            OnPropertyChanged("LastIndex");
            att.EndRead();
        }

        #region 代码生成

        public override async System.Threading.Tasks.Task GCode_CodeDom_GenerateCode(CodeTypeDeclaration codeClass, CodeStatementCollection codeStatementCollection, CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            if (element != mCtrlMethodLink_Pre)
                return;

            var param = CSParam as ForLoopConstructionParams;

            CodeExpression indexFirstExp;
            CodeExpression indexLastExp;
            if (mCtrlIndexFirst.HasLink)
            {
                if (!mCtrlIndexFirst.GetLinkedObject(0).IsOnlyReturnValue)
                    await mCtrlIndexFirst.GetLinkedObject(0).GCode_CodeDom_GenerateCode(codeClass, codeStatementCollection, mCtrlIndexFirst.GetLinkedPinControl(0), context);
                indexFirstExp = mCtrlIndexFirst.GetLinkedObject(0).GCode_CodeDom_GetValue(mCtrlIndexFirst.GetLinkedPinControl(0), context);
            }
            else
                indexFirstExp = new CodePrimitiveExpression(FirstIndex);

            if (mCtrlIndexLast.HasLink)
            {
                if (!mCtrlIndexLast.GetLinkedObject(0).IsOnlyReturnValue)
                    await mCtrlIndexLast.GetLinkedObject(0).GCode_CodeDom_GenerateCode(codeClass, codeStatementCollection, mCtrlIndexLast.GetLinkedPinControl(0), context);
                indexLastExp = mCtrlIndexLast.GetLinkedObject(0).GCode_CodeDom_GetValue(mCtrlIndexLast.GetLinkedPinControl(0), context);
            }
            else
                indexLastExp = new CodePrimitiveExpression(LastIndex);

            var idxName = GCode_GetValueName(mCtrlIndex, context);
            var forLoop = new CodeIterationStatement();
            forLoop.InitStatement = new CodeVariableDeclarationStatement(typeof(int), idxName, indexFirstExp);
            forLoop.TestExpression = new CodeBinaryOperatorExpression(new CodeVariableReferenceExpression(idxName), CodeBinaryOperatorType.LessThanOrEqual, indexLastExp);
            forLoop.IncrementStatement = new CodeAssignStatement(new CodeVariableReferenceExpression(idxName),
                                                                                        new CodeBinaryOperatorExpression(new CodeVariableReferenceExpression(idxName),
                                                                                                                         CodeBinaryOperatorType.Add,
                                                                                                                         new CodePrimitiveExpression(1)));
            codeStatementCollection.Add(forLoop);
            if(mCtrlMethodLink_LoopBody.HasLink)
            {
                await mCtrlMethodLink_LoopBody.GetLinkedObject(0).GCode_CodeDom_GenerateCode(codeClass, forLoop.Statements, mCtrlMethodLink_LoopBody.GetLinkedPinControl(0), context);
            }
            if(context.GenerateNext)
            {
                if (mCtrlMethodLink_Completed.HasLink)
                    await mCtrlMethodLink_Completed.GetLinkedObject(0).GCode_CodeDom_GenerateCode(codeClass, codeStatementCollection, mCtrlMethodLink_Completed.GetLinkedPinControl(0), context);
            }
        }

        public override string GCode_GetValueName(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            if (element == null || element == mCtrlIndex)
                return $"index_{EngineNS.Editor.Assist.GetValuedGUIDString(this.Id)}";
            return base.GCode_GetValueName(element, context);
        }
        public override CodeExpression GCode_CodeDom_GetValue(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context, CodeGenerateSystem.Base.GenerateCodeContext_PreNode preNodeContext = null)
        {
            if(element == null || element == mCtrlIndex)
            {
                var nameStr = GCode_GetValueName(element, context);
                return new CodeVariableReferenceExpression(nameStr);
            }
            return base.GCode_CodeDom_GetValue(element, context);
        }
        public override Type GCode_GetType(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            if(element == null || element == mCtrlIndex)
            {
                return typeof(int);
            }
            return base.GCode_GetType(element, context);
        }
        public override string GCode_GetTypeString(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            if(element == null || element == mCtrlIndex)
            {
                return EngineNS.Rtti.RttiHelper.GetAppTypeString(typeof(int));
            }
            return base.GCode_GetTypeString(element, context);
        }

        #endregion
    }
}
