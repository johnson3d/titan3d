using System;
using System.Collections.Generic;
using System.Text;
using EngineNS.IO;

namespace CodeDomNode
{
    [EngineNS.Rtti.MetaClass]
    public class BezierControlConstructParam : CodeGenerateSystem.Base.ConstructionParams
    {

    }
    [CodeGenerateSystem.CustomConstructionParams(typeof(BezierControlConstructParam))]
    public partial class BezierControl : CodeGenerateSystem.Base.BaseNodeControl
    {
        CodeGenerateSystem.Base.LinkPinControl mCtrlValueInputHandle = new CodeGenerateSystem.Base.LinkPinControl();
        CodeGenerateSystem.Base.LinkPinControl mCtrlValueOutputHandle = new CodeGenerateSystem.Base.LinkPinControl();
        CodeGenerateSystem.Base.LinkPinControl mCtrlValueYMaxInputHandle = new CodeGenerateSystem.Base.LinkPinControl();
        CodeGenerateSystem.Base.LinkPinControl mCtrlValueYMinInputHandle = new CodeGenerateSystem.Base.LinkPinControl();
        CodeGenerateSystem.Base.LinkPinControl mCtrlValueXMinInputHandle = new CodeGenerateSystem.Base.LinkPinControl();
        CodeGenerateSystem.Base.LinkPinControl mCtrlValueXMaxInputHandle = new CodeGenerateSystem.Base.LinkPinControl();
        CodeGenerateSystem.Base.LinkPinControl mCtrlValueXLoopHandle = new CodeGenerateSystem.Base.LinkPinControl();

        List<EngineNS.BezierPointBase> mBezierPoints = new List<EngineNS.BezierPointBase>();
        partial void SetBezierPointsToCtrl();
        partial void GetBezierPointsFromCtrl();

        CodeGenerateSystem.Base.GeneratorClassBase mTemplateClassInstance = null;

        partial void InitConstruction();
        public BezierControl(CodeGenerateSystem.Base.ConstructionParams csParam)
            : base(csParam)
        {
            var cpInfos = new List<CodeGenerateSystem.Base.CustomPropertyInfo>();
            cpInfos.Add(CodeGenerateSystem.Base.CustomPropertyInfo.GetFromParamInfo(typeof(double), "XMax", new Attribute[] { new EngineNS.Rtti.MetaDataAttribute() }));
            cpInfos.Add(CodeGenerateSystem.Base.CustomPropertyInfo.GetFromParamInfo(typeof(double), "XMin", new Attribute[] { new EngineNS.Rtti.MetaDataAttribute() }));
            cpInfos.Add(CodeGenerateSystem.Base.CustomPropertyInfo.GetFromParamInfo(typeof(double), "YMax", new Attribute[] { new EngineNS.Rtti.MetaDataAttribute() }));
            cpInfos.Add(CodeGenerateSystem.Base.CustomPropertyInfo.GetFromParamInfo(typeof(double), "YMin", new Attribute[] { new EngineNS.Rtti.MetaDataAttribute() }));
            cpInfos.Add(CodeGenerateSystem.Base.CustomPropertyInfo.GetFromParamInfo(typeof(bool), "XLoop", new Attribute[] { new EngineNS.Rtti.MetaDataAttribute() }));
            mTemplateClassInstance = CodeGenerateSystem.Base.PropertyClassGenerator.CreateClassInstanceFromCustomPropertys(cpInfos, this);

            InitConstruction();
            NodeName = "Bezier";

            // X输入
            AddLinkPinInfo("CtrlValueInputHandle", mCtrlValueInputHandle, null);
            // Y输出
            AddLinkPinInfo("CtrlValueOutputHandle", mCtrlValueOutputHandle, null);
            // Y轴最大值
            AddLinkPinInfo("CtrlValueYMaxInputHandle", mCtrlValueYMaxInputHandle, null);
            // Y轴最小值
            AddLinkPinInfo("CtrlValueYMinInputHandle", mCtrlValueYMinInputHandle, null);
            // X轴最小值
            AddLinkPinInfo("CtrlValueXMinInputHandle", mCtrlValueXMinInputHandle, null);
            // X轴最大值
            AddLinkPinInfo("CtrlValueXMaxInputHandle", mCtrlValueXMaxInputHandle, null);
            // X循环
            AddLinkPinInfo("CtrlValueXLoopHandle", mCtrlValueXLoopHandle, null);
        }
        public static void InitNodePinTypes(CodeGenerateSystem.Base.ConstructionParams smParam)
        {
            // X输入
            CollectLinkPinInfo(smParam, "CtrlValueInputHandle", CodeGenerateSystem.Base.enLinkType.NumbericalValue, CodeGenerateSystem.Base.enBezierType.Left, CodeGenerateSystem.Base.enLinkOpType.End, false);
            // Y输出
            CollectLinkPinInfo(smParam, "CtrlValueOutputHandle", CodeGenerateSystem.Base.enLinkType.NumbericalValue, CodeGenerateSystem.Base.enBezierType.Right, CodeGenerateSystem.Base.enLinkOpType.Start, true);
            // Y轴最大值
            CollectLinkPinInfo(smParam, "CtrlValueYMaxInputHandle", CodeGenerateSystem.Base.enLinkType.NumbericalValue, CodeGenerateSystem.Base.enBezierType.Left, CodeGenerateSystem.Base.enLinkOpType.End, false);
            // Y轴最小值
            CollectLinkPinInfo(smParam, "CtrlValueYMinInputHandle", CodeGenerateSystem.Base.enLinkType.NumbericalValue, CodeGenerateSystem.Base.enBezierType.Left, CodeGenerateSystem.Base.enLinkOpType.End, false);
            // X轴最小值
            CollectLinkPinInfo(smParam, "CtrlValueXMinInputHandle", CodeGenerateSystem.Base.enLinkType.NumbericalValue, CodeGenerateSystem.Base.enBezierType.Left, CodeGenerateSystem.Base.enLinkOpType.End, false);
            // X轴最大值
            CollectLinkPinInfo(smParam, "CtrlValueXMaxInputHandle", CodeGenerateSystem.Base.enLinkType.NumbericalValue, CodeGenerateSystem.Base.enBezierType.Left, CodeGenerateSystem.Base.enLinkOpType.End, false);
            // X循环
            CollectLinkPinInfo(smParam, "CtrlValueXLoopHandle", CodeGenerateSystem.Base.enLinkType.Bool, CodeGenerateSystem.Base.enBezierType.Left, CodeGenerateSystem.Base.enLinkOpType.End, false);
        }

        #region SaveLoad

        public override void Save(XndNode xndNode, bool newGuid)
        {
            GetBezierPointsFromCtrl();
            var att = xndNode.AddAttrib("Data");
            att.Version = 1;
            att.BeginWrite();
            CodeGenerateSystem.Base.PropertyClassGenerator.SaveClassInstanceProperties(mTemplateClassInstance, att);
            att.Write((Int32)mBezierPoints.Count);
            foreach(var bPt in mBezierPoints)
            {
                var pt = new EngineNS.BezierPointBase(bPt.Position, bPt.ControlPoint);
                att.WriteMetaObject(pt);
            }
            att.Write(mBezierWidth);
            att.Write(mBezierHeight);
            att.EndWrite();
            base.Save(xndNode, newGuid);
        }
        delegate EngineNS.BezierPointBase Delegate_CreateBezierPoint();
        Delegate_CreateBezierPoint OnCreateBezierPoint = null;
        public override async System.Threading.Tasks.Task Load(XndNode xndNode)
        {
            var att = xndNode.FindAttrib("Data");
            if(att != null)
            {
                att.BeginRead();
                switch(att.Version)
                {
                    case 0:
                        {
                            att.ReadMetaObject(mTemplateClassInstance);
                            Int32 count;
                            att.Read(out count);
                            for(int i=0; i<count; i++)
                            {
                                var bPt = att.ReadMetaObject() as EngineNS.BezierPointBase;
                                var pt = new EditorCommon.Controls.LineXBezierControl.BezierPoint(LineXBezierCtrl);
                                pt.Position = bPt.Position;
                                pt.ControlPoint = bPt.ControlPoint;
                                mBezierPoints.Add(pt);
                            }
                            att.Read(out mBezierWidth);
                            att.Read(out mBezierHeight);
                        }
                        break;
                    case 1:
                        {
                            CodeGenerateSystem.Base.PropertyClassGenerator.LoadClassInstanceProperties(mTemplateClassInstance, att);
                            Int32 count;
                            att.Read(out count);
                            for (int i = 0; i < count; i++)
                            {
                                var bPt = att.ReadMetaObject() as EngineNS.BezierPointBase;
                                var pt = new EditorCommon.Controls.LineXBezierControl.BezierPoint(LineXBezierCtrl);
                                pt.Position = bPt.Position;
                                pt.ControlPoint = bPt.ControlPoint;
                                mBezierPoints.Add(pt);
                            }
                            att.Read(out mBezierWidth);
                            att.Read(out mBezierHeight);
                        }
                        break;
                }
                att.EndRead();
            }
            await base.Load(xndNode);
            SetBezierPointsToCtrl();
            foreach (var pt in mBezierPoints)
            {
                ((EditorCommon.Controls.LineXBezierControl.BezierPoint)pt).CalculateOpposite();
            }
        }

        #endregion

        #region 代码生成


        public override bool HasMultiOutLink
        {
            get { return true; }
        }

        private string StaticBezierPointsName
        {
            get
            {
                return "mBezierPoints_" + EngineNS.Editor.Assist.GetValuedGUIDString(Id);
            }
        }

        private string BezierPointsListType
        {
            get { return "System.Collections.Generic.List<EngineNS.BezierPointBase>"; }
        }

        public override string GCode_GetTypeString(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            return "System.Double";
        }

        public override Type GCode_GetType(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            return typeof(System.Double);
        }

        public override string GCode_GetValueName(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            return "BezierValue_" + EngineNS.Editor.Assist.GetValuedGUIDString(Id);
        }

        public override System.CodeDom.CodeExpression GCode_CodeDom_GetValue(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context, CodeGenerateSystem.Base.GenerateCodeContext_PreNode preNodeContext = null)
        {
            return new System.CodeDom.CodeVariableReferenceExpression(GCode_GetValueName(null, context));
            //var XInputLinkOI = GetLinkPinInfo(ValueInputHandle);
            //if (!XInputLinkOI.bHasLink)
            //{
            //    return new System.CodeDom.CodePrimitiveExpression(0);
            //}

            //// code: EngineNS.BezierCalculate.ValueOnBezier(mBezierPoints_XXX, XPos);
            //return new System.CodeDom.CodeMethodInvokeExpression(
            //                new System.CodeDom.CodeTypeReferenceExpression(typeof(EngineNS.BezierCalculate)),
            //                "ValueOnBezier",
            //                new System.CodeDom.CodeExpression[]{
            //                    new System.CodeDom.CodeFieldReferenceExpression(new System.CodeDom.CodeThisReferenceExpression(), StaticBezierPointsName),
            //                    XInputLinkOI.GetLinkObject(0, true).GCode_CodeDom_GetValue(XInputLinkOI.GetLinkElement(0, true))
            //                });
        }

        System.CodeDom.CodeMemberField mBezierPointsField = new System.CodeDom.CodeMemberField();
        System.CodeDom.CodeVariableDeclarationStatement mVariableDeclaration = new System.CodeDom.CodeVariableDeclarationStatement();

        double mBezierWidth = 300;
        double mBezierHeight = 200;
        public override async System.Threading.Tasks.Task GCode_CodeDom_GenerateCode(System.CodeDom.CodeTypeDeclaration codeClass, System.CodeDom.CodeStatementCollection codeStatementCollection, CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            GetBezierPointsFromCtrl();

            if (!codeClass.Members.Contains(mBezierPointsField))
            {
                // code: private static System.Collections.Generic.List<EngineNS.BezierPointBase> mBezierPoints_XXX = new System.Collections.Generic.List<EngineNS.BezierPointBase>(new BezierPoint[] { new EngineNS.BezierPointBase(), new EngineNS.BezierPointBase() });
                mBezierPointsField.Type = new System.CodeDom.CodeTypeReference(BezierPointsListType);
                mBezierPointsField.Name = StaticBezierPointsName;
                mBezierPointsField.Attributes = System.CodeDom.MemberAttributes.Private;
                var arrayCreateExp = new System.CodeDom.CodeArrayCreateExpression();
                arrayCreateExp.CreateType = new System.CodeDom.CodeTypeReference(typeof(EngineNS.BezierPointBase));
                foreach (var bPt in mBezierPoints)
                {
                    var newBPT = new System.CodeDom.CodeObjectCreateExpression();
                    newBPT.CreateType = new System.CodeDom.CodeTypeReference(typeof(EngineNS.BezierPointBase));
                    newBPT.Parameters.Add(new System.CodeDom.CodeObjectCreateExpression(typeof(EngineNS.Vector2),
                                                new System.CodeDom.CodeExpression[]{
                                                    new System.CodeDom.CodePrimitiveExpression(bPt.Position.X),
                                                    new System.CodeDom.CodePrimitiveExpression(bPt.Position.Y)
                                                }));
                    newBPT.Parameters.Add(new System.CodeDom.CodeObjectCreateExpression(typeof(EngineNS.Vector2),
                                                new System.CodeDom.CodeExpression[]{
                                                    new System.CodeDom.CodePrimitiveExpression(bPt.ControlPoint.X),
                                                    new System.CodeDom.CodePrimitiveExpression(bPt.ControlPoint.Y)
                                                }));
                    arrayCreateExp.Initializers.Add(newBPT);
                }
                mBezierPointsField.InitExpression = new System.CodeDom.CodeObjectCreateExpression(BezierPointsListType, arrayCreateExp);
                codeClass.Members.Add(mBezierPointsField);
            }

            // 判断5个输入链接是否需要生成代码
            if (mCtrlValueInputHandle.HasLink)
            {
                if (!mCtrlValueInputHandle.GetLinkedObject(0, true).IsOnlyReturnValue)
                    await mCtrlValueInputHandle.GetLinkedObject(0, true).GCode_CodeDom_GenerateCode(codeClass, codeStatementCollection, mCtrlValueInputHandle.GetLinkedPinControl(0, true), context);
            }

            if (mCtrlValueYMaxInputHandle.HasLink)
            {
                if (!mCtrlValueYMaxInputHandle.GetLinkedObject(0, true).IsOnlyReturnValue)
                    await mCtrlValueYMaxInputHandle.GetLinkedObject(0, true).GCode_CodeDom_GenerateCode(codeClass, codeStatementCollection, mCtrlValueYMaxInputHandle.GetLinkedPinControl(0, true), context);
            }

            if (mCtrlValueYMinInputHandle.HasLink)
            {
                if (!mCtrlValueYMinInputHandle.GetLinkedObject(0, true).IsOnlyReturnValue)
                    await mCtrlValueYMinInputHandle.GetLinkedObject(0, true).GCode_CodeDom_GenerateCode(codeClass, codeStatementCollection, mCtrlValueYMinInputHandle.GetLinkedPinControl(0, true), context);
            }

            if (mCtrlValueXMaxInputHandle.HasLink)
            {
                if (!mCtrlValueXMaxInputHandle.GetLinkedObject(0, true).IsOnlyReturnValue)
                    await mCtrlValueXMaxInputHandle.GetLinkedObject(0, true).GCode_CodeDom_GenerateCode(codeClass, codeStatementCollection, mCtrlValueXMaxInputHandle.GetLinkedPinControl(0, true), context);
            }

            if (mCtrlValueXMinInputHandle.HasLink)
            {
                if (!mCtrlValueXMinInputHandle.GetLinkedObject(0, true).IsOnlyReturnValue)
                    await mCtrlValueXMinInputHandle.GetLinkedObject(0, true).GCode_CodeDom_GenerateCode(codeClass, codeStatementCollection, mCtrlValueXMinInputHandle.GetLinkedPinControl(0, true), context);
            }

            if(mCtrlValueXLoopHandle.HasLink)
            {
                if (!mCtrlValueXLoopHandle.GetLinkedObject(0, true).IsOnlyReturnValue)
                    await mCtrlValueXLoopHandle.GetLinkedObject(0, true).GCode_CodeDom_GenerateCode(codeClass, codeStatementCollection, mCtrlValueXLoopHandle.GetLinkedPinControl(0, true), context);
            }

            // 自身代码生成
            if (!context.Method.Statements.Contains(mVariableDeclaration))
            {
                // 声明
                mVariableDeclaration.Type = new System.CodeDom.CodeTypeReference(typeof(double));
                mVariableDeclaration.Name = GCode_GetValueName(null, context);
                mVariableDeclaration.InitExpression = CodeGenerateSystem.Program.GetDefaultValueExpressionFromType(typeof(double));
                context.Method.Statements.Insert(0, mVariableDeclaration);
            }

            var assignExp = new System.CodeDom.CodeAssignStatement();
            assignExp.Left = new System.CodeDom.CodeVariableReferenceExpression(GCode_GetValueName(null, context));
            var classType = mTemplateClassInstance.GetType();

            System.CodeDom.CodeExpression minXExp, maxXExp, minYExp, maxYExp, isXLoopExp;
            if (mCtrlValueXMinInputHandle.HasLink)
                minXExp = mCtrlValueXMinInputHandle.GetLinkedObject(0, true).GCode_CodeDom_GetValue(mCtrlValueXMinInputHandle.GetLinkedPinControl(0, true), context);
            else
            {
                var pro = classType.GetProperty("XMin");
                var xMin = pro.GetValue(mTemplateClassInstance);
                minXExp = new System.CodeDom.CodePrimitiveExpression(xMin);
            }

            if (mCtrlValueXMaxInputHandle.HasLink)
                maxXExp = mCtrlValueXMaxInputHandle.GetLinkedObject(0, true).GCode_CodeDom_GetValue(mCtrlValueXMaxInputHandle.GetLinkedPinControl(0, true), context);
            else
            {
                var pro = classType.GetProperty("XMax");
                var xMax = pro.GetValue(mTemplateClassInstance);
                maxXExp = new System.CodeDom.CodePrimitiveExpression(xMax);
            }

            if (mCtrlValueYMinInputHandle.HasLink)
                minYExp = mCtrlValueYMinInputHandle.GetLinkedObject(0, true).GCode_CodeDom_GetValue(mCtrlValueYMinInputHandle.GetLinkedPinControl(0, true), context);
            else
            {
                var pro = classType.GetProperty("YMin");
                var yMin = pro.GetValue(mTemplateClassInstance);
                minYExp = new System.CodeDom.CodePrimitiveExpression(yMin);
            }

            if (mCtrlValueYMaxInputHandle.HasLink)
                maxYExp = mCtrlValueYMaxInputHandle.GetLinkedObject(0, true).GCode_CodeDom_GetValue(mCtrlValueYMaxInputHandle.GetLinkedPinControl(0, true), context);
            else
            {
                var pro = classType.GetProperty("YMax");
                var yMax = pro.GetValue(mTemplateClassInstance);
                maxYExp = new System.CodeDom.CodePrimitiveExpression(yMax);
            }

            if (mCtrlValueXLoopHandle.HasLink)
                isXLoopExp = mCtrlValueXLoopHandle.GetLinkedObject(0, true).GCode_CodeDom_GetValue(mCtrlValueXLoopHandle.GetLinkedPinControl(0, true), context);
            else
            {
                var pro = classType.GetProperty("XLoop");
                var isXLoop = pro.GetValue(mTemplateClassInstance);
                isXLoopExp = new System.CodeDom.CodePrimitiveExpression(isXLoop);
            }

            // code: EngineNS.BezierCalculate.ValueOnBezier(mBezierPoints_XXX, XPos);
            assignExp.Right = new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(
                                                new System.CodeDom.CodeTypeReferenceExpression(typeof(EngineNS.BezierCalculate)),
                                                "ValueOnBezier",
                                                new System.CodeDom.CodeExpression[]{
                                                    new System.CodeDom.CodeVariableReferenceExpression(StaticBezierPointsName),     // bezierPtList
                                                    mCtrlValueInputHandle.GetLinkedObject(0, true).GCode_CodeDom_GetValue(mCtrlValueInputHandle.GetLinkedPinControl(0, true), context), // xValue
                                                    minXExp, maxXExp, minYExp, maxYExp,
                                                    new System.CodeDom.CodePrimitiveExpression(0),                  // MinBezierX
                                                    new System.CodeDom.CodePrimitiveExpression(mBezierWidth),   // MaxBezierX
                                                    new System.CodeDom.CodePrimitiveExpression(0),                  // MinBezierY
                                                    new System.CodeDom.CodePrimitiveExpression(mBezierHeight),  // MaxBezierY
                                                    isXLoopExp             // bLoopX
                                        });

            codeStatementCollection.Add(assignExp);

            // 收集用于调试的数据的代码
            var debugCodes = CodeDomNode.BreakPoint.BeginMacrossDebugCodeStatments(codeStatementCollection);
            CodeDomNode.BreakPoint.GetGatherDataValueCodeStatement(debugCodes, this.mCtrlValueInputHandle.GetLinkPinKeyName(),
                                                                   mCtrlValueInputHandle.GetLinkedObject(0, true).GCode_CodeDom_GetValue(mCtrlValueInputHandle.GetLinkedPinControl(0, true), context),
                                                                   mCtrlValueInputHandle.GetLinkedObject(0, true).GCode_GetTypeString(mCtrlValueInputHandle.GetLinkedPinControl(0, true), context), context);
            CodeDomNode.BreakPoint.GetGatherDataValueCodeStatement(debugCodes, mCtrlValueOutputHandle.GetLinkPinKeyName(),
                                                                   assignExp.Left, GCode_GetTypeString(null, context), context);
            CodeDomNode.BreakPoint.EndMacrossDebugCodeStatements(codeStatementCollection, debugCodes);
        }

        #endregion

    }
}
