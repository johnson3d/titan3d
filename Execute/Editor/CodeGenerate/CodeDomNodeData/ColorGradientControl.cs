using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Media;
using EngineNS.IO;

namespace CodeDomNode
{
    [EngineNS.Rtti.MetaClass]
    public class ColorGradientControlConstructParam : CodeGenerateSystem.Base.ConstructionParams
    {
        [EngineNS.Rtti.MetaData]
        public double Width { get; set; } = 442;

        public override EditorCommon.CodeGenerateSystem.INodeConstructionParams Duplicate()
        {
            var retVal = base.Duplicate() as ColorGradientControlConstructParam;
            retVal.Width = Width;
            return retVal;
        }
        public override bool Equals(object obj)
        {
            if (!base.Equals(obj))
                return false;
            var param = obj as ColorGradientControlConstructParam;
            if (param == null)
                return false;
            if (Width != param.Width)
                return false;
            return true;
        }
        public override int GetHashCode()
        {
            return (base.GetHashCodeString() + Width.ToString()).GetHashCode();
        }

    }
    [CodeGenerateSystem.CustomConstructionParams(typeof(ColorGradientControlConstructParam))]
    public partial class ColorGradientControl : CodeGenerateSystem.Base.BaseNodeControl
    {
        [EngineNS.Rtti.MetaClass]
        public class GradientData : EngineNS.IO.Serializer.Serializer
        {
            [EngineNS.Rtti.MetaData]
            public float Offset;
            [EngineNS.Rtti.MetaData]
            public EngineNS.Color4 GradientColor;
            public GradientData(float v1, EngineNS.Color4 v2)
            {
                Offset = v1;
                GradientColor = v2;
            }

            public GradientData()
            {
                Offset = 0.0f;
            }
        }
        [EngineNS.Rtti.MetaData]
        List<GradientData> GradientDatas = new List<GradientData>();
        CodeGenerateSystem.Base.LinkPinControl mCtrlvalue_ColorOut = new CodeGenerateSystem.Base.LinkPinControl();
        CodeGenerateSystem.Base.LinkPinControl mCtrlValueIn = new CodeGenerateSystem.Base.LinkPinControl();
        CodeGenerateSystem.Base.LinkPinControl mCtrlValueOutR = new CodeGenerateSystem.Base.LinkPinControl();
        CodeGenerateSystem.Base.LinkPinControl mCtrlValueOutG = new CodeGenerateSystem.Base.LinkPinControl();
        CodeGenerateSystem.Base.LinkPinControl mCtrlValueOutB = new CodeGenerateSystem.Base.LinkPinControl();
        CodeGenerateSystem.Base.LinkPinControl mCtrlValueOutA = new CodeGenerateSystem.Base.LinkPinControl();

        static Type mValueType = typeof(EngineNS.Color4);
        // 临时类，用于选中后显示参数属性

        List<CodeGenerateSystem.Base.LinkPinControl> mInComponentLinks = new List<CodeGenerateSystem.Base.LinkPinControl>();
        List<CodeGenerateSystem.Base.LinkPinControl> mOutComponentLinks = new List<CodeGenerateSystem.Base.LinkPinControl>();

        partial void InitConstruction();
        public ColorGradientControl(CodeGenerateSystem.Base.ConstructionParams csParam)
            : base(csParam)
        {
            var cpInfos = new List<CodeGenerateSystem.Base.CustomPropertyInfo>();
            var cpInfo = CodeGenerateSystem.Base.CustomPropertyInfo.GetFromParamInfo(typeof(EngineNS.Color), "Color", new Attribute[] { new DisplayNameAttribute("颜色"), new EngineNS.Editor.Editor_ColorPicker(), new EngineNS.Rtti.MetaDataAttribute() });
            cpInfos.Add(cpInfo);
            InitConstruction();

            var linkType = CodeGenerateSystem.Base.LinkPinControl.GetLinkTypeFromTypeString(mValueType.FullName);
            AddLinkPinInfo("Ctrlvalue_ColorOut", mCtrlvalue_ColorOut, null);

            mInComponentLinks.Add(mCtrlValueIn);
            AddLinkPinInfo("CtrlValueIn", mCtrlValueIn, null);

            mOutComponentLinks.Add(mCtrlValueOutR);
            mOutComponentLinks.Add(mCtrlValueOutG);
            mOutComponentLinks.Add(mCtrlValueOutB);
            mOutComponentLinks.Add(mCtrlValueOutA);
            AddLinkPinInfo("CtrlValueOutR", mCtrlValueOutR, null);
            AddLinkPinInfo("CtrlValueOutG", mCtrlValueOutG, null);
            AddLinkPinInfo("CtrlValueOutB", mCtrlValueOutB, null);
            AddLinkPinInfo("CtrlValueOutA", mCtrlValueOutA, null);
        }
        public static void InitNodePinTypes(CodeGenerateSystem.Base.ConstructionParams smParam)
        {
            //var linkType = CodeGenerateSystem.Base.LinkPinControl.GetLinkTypeFromTypeString(mValueType.FullName);
            CollectLinkPinInfo(smParam, "Ctrlvalue_ColorOut", mValueType, CodeGenerateSystem.Base.enBezierType.Right, CodeGenerateSystem.Base.enLinkOpType.Start, true);
            CollectLinkPinInfo(smParam, "CtrlValueIn", CodeGenerateSystem.Base.enLinkType.NumbericalValue, CodeGenerateSystem.Base.enBezierType.Left, CodeGenerateSystem.Base.enLinkOpType.End, false);
            CollectLinkPinInfo(smParam, "CtrlValueOutR", CodeGenerateSystem.Base.enLinkType.NumbericalValue, CodeGenerateSystem.Base.enBezierType.Right, CodeGenerateSystem.Base.enLinkOpType.Start, true);
            CollectLinkPinInfo(smParam, "CtrlValueOutG", CodeGenerateSystem.Base.enLinkType.NumbericalValue, CodeGenerateSystem.Base.enBezierType.Right, CodeGenerateSystem.Base.enLinkOpType.Start, true);
            CollectLinkPinInfo(smParam, "CtrlValueOutB", CodeGenerateSystem.Base.enLinkType.NumbericalValue, CodeGenerateSystem.Base.enBezierType.Right, CodeGenerateSystem.Base.enLinkOpType.Start, true);
            CollectLinkPinInfo(smParam, "CtrlValueOutA", CodeGenerateSystem.Base.enLinkType.NumbericalValue, CodeGenerateSystem.Base.enBezierType.Right, CodeGenerateSystem.Base.enLinkOpType.Start, true);
        }

        #region SaveLoad

        public override void Save(XndNode xndNode, bool newGuid)
        {
            GetGradientDatas();
            var att = xndNode.AddAttrib("DefaultParamValue");
            att.Version = 0;
            att.BeginWrite();
            att.Write(GradientDatas.Count);
            for (int i = 0; i < GradientDatas.Count; i++)
            {
                att.WriteMetaObject(GradientDatas[i]);
            }
            att.EndWrite();
            base.Save(xndNode, newGuid);
        }

        public override async System.Threading.Tasks.Task Load(XndNode xndNode)
        {
            await base.Load(xndNode);
            var att = xndNode.FindAttrib("DefaultParamValue");
            if (att != null)
            {
                att.BeginRead();
                switch (att.Version)
                {
                    case 0:
                        {
                            int count = 0;
                            att.Read(out count);
                            for (int i = 0; i < count; i++)
                            {
                                GradientData obj = new GradientData(); ;
                                att.ReadMetaObject(obj);
                                GradientDatas.Add(obj);
                            }
                            SetGradientDatas(GradientDatas);
                        }
                        break;
                }
                att.EndRead();
            }
        }

        #endregion

        #region 生成代码

        public override string GCode_GetValueName(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            string strValueName = "";
            if (string.IsNullOrEmpty(NodeName) || NodeName == CodeGenerateSystem.Program.NodeDefaultName)
                strValueName = "color_" + EngineNS.Editor.Assist.GetValuedGUIDString(Id);
            else
                strValueName = NodeName;

            var name = GetValuePureName(element);
            if (!string.IsNullOrEmpty(name))
                strValueName += "." + name;

            return strValueName;
        }

        string GetValuePureName(CodeGenerateSystem.Base.LinkPinControl element)
        {
            if (element == mCtrlValueOutR)
                return "Red";
            else if (element == mCtrlValueOutG)
                return "Green";
            else if (element == mCtrlValueOutB)
                return "Blue";
            else if (element == mCtrlValueOutA)
                return "Alpha";

            return "";
        }

        public override string GCode_GetTypeString(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            if (mInComponentLinks.Contains(element) ||
               mOutComponentLinks.Contains(element))
                return "System.Single";

            return mValueType.FullName;
        }

        public override Type GCode_GetType(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            if (mInComponentLinks.Contains(element) ||
                mOutComponentLinks.Contains(element))
                return typeof(System.Single);

            return mValueType;
        }

        partial void GetGradientDatas();
        partial void SetGradientDatas(List<GradientData> datas);
        public override CodeExpression GCode_CodeDom_GetValue(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context, CodeGenerateSystem.Base.GenerateCodeContext_PreNode preNodeContext = null)
        {
            if (element == mCtrlValueOutR)
                return new CodeFieldReferenceExpression(new CodeVariableReferenceExpression(GCode_GetValueName(null, context)), "Red");
            else if (element == mCtrlValueOutG)
                return new CodeFieldReferenceExpression(new CodeVariableReferenceExpression(GCode_GetValueName(null, context)), "Green");
            else if (element == mCtrlValueOutB)
                return new CodeFieldReferenceExpression(new CodeVariableReferenceExpression(GCode_GetValueName(null, context)), "Blue");
            else if (element == mCtrlValueOutA)
                return new CodeFieldReferenceExpression(new CodeVariableReferenceExpression(GCode_GetValueName(null, context)), "Alpha");

            return new CodeVariableReferenceExpression(GCode_GetValueName(null, context));
        }

        System.CodeDom.CodeVariableDeclarationStatement mVariableDeclaration = new System.CodeDom.CodeVariableDeclarationStatement();
        public override async System.Threading.Tasks.Task GCode_CodeDom_GenerateCode(CodeTypeDeclaration codeClass, CodeStatementCollection codeStatementCollection, CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            var strValueName = GCode_GetValueName(null, context);

            if (!context.Method.Statements.Contains(mVariableDeclaration))
            {
                mVariableDeclaration.Type = new CodeTypeReference(mValueType);
                mVariableDeclaration.Name = strValueName;
                mVariableDeclaration.InitExpression = new CodeObjectCreateExpression(mValueType, new CodePrimitiveExpression(1.0f),
                                                                         new CodePrimitiveExpression(1.0f),
                                                                         new CodePrimitiveExpression(1.0f),
                                                                         new CodePrimitiveExpression(1.0f));
                context.Method.Statements.Insert(0, mVariableDeclaration);
            }


            if (mCtrlValueIn.HasLink)
            {
                if (!mCtrlValueIn.GetLinkedObject(0).IsOnlyReturnValue)
                    await mCtrlValueIn.GetLinkedObject(0).GCode_CodeDom_GenerateCode(codeClass, codeStatementCollection, mCtrlValueIn.GetLinkedPinControl(0), context);

                var express = mCtrlValueIn.GetLinkedObject(0, true).GCode_CodeDom_GetValue(mCtrlValueIn.GetLinkedPinControl(0, true), context);
                GCode_CodeDom_GenerateCode(codeClass, codeStatementCollection, express, strValueName);
                

            }
            else
            {

            }
            if (mCtrlValueIn.HasLink)
            {
                //if (!mCtrlvalue_ColorIn.GetLinkedObject(0, true).IsOnlyReturnValue)
                //    await mCtrlvalue_ColorIn.GetLinkedObject(0, true).GCode_CodeDom_GenerateCode(codeClass, codeStatementCollection, element, context);

                //var assignStatement = new System.CodeDom.CodeAssignStatement(
                //                                                        new System.CodeDom.CodeVariableReferenceExpression(strValueName),
                //                                                        mCtrlvalue_ColorIn.GetLinkedObject(0, true).GCode_CodeDom_GetValue(mCtrlvalue_ColorIn.GetLinkedPinControl(0, true), context));
                //codeStatementCollection.Add(assignStatement);
            }

            // r、g、b、a有链接的情况
            //foreach (var link in mInComponentLinks)
            //{
            //    if (link.HasLink)
            //    {
            //        if (!link.GetLinkedObject(0, true).IsOnlyReturnValue)
            //            await link.GetLinkedObject(0, true).GCode_CodeDom_GenerateCode(codeClass, codeStatementCollection, element, context);

            //        var fieldRef = new CodeFieldReferenceExpression();
            //        fieldRef.TargetObject = new CodeVariableReferenceExpression(strValueName);
            //        fieldRef.FieldName = GetValuePureName(link);
            //        var statValAss = new CodeAssignStatement();
            //        statValAss.Left = fieldRef;
            //        statValAss.Right = new CodeGenerateSystem.CodeDom.CodeCastExpression(typeof(float), link.GetLinkedObject(0, true).GCode_CodeDom_GetValue(link.GetLinkedPinControl(0, true), context));
            //        codeStatementCollection.Add(statValAss);
            //    }
            //}

            // 收集用于调试的数据的代码
            var debugCodes = CodeDomNode.BreakPoint.BeginMacrossDebugCodeStatments(codeStatementCollection);
            CodeDomNode.BreakPoint.GetGatherDataValueCodeStatement(debugCodes, mCtrlvalue_ColorOut.GetLinkPinKeyName(),
                                                                   new CodeVariableReferenceExpression(GCode_GetValueName(mCtrlvalue_ColorOut, context)),
                                                                   GCode_GetTypeString(mCtrlvalue_ColorOut, context), context);
            CodeDomNode.BreakPoint.GetGatherDataValueCodeStatement(debugCodes, mCtrlValueOutR.GetLinkPinKeyName(),
                                                                   new CodeVariableReferenceExpression(GCode_GetValueName(mCtrlValueOutR, context)),
                                                                   GCode_GetTypeString(mCtrlValueOutR, context), context);
            CodeDomNode.BreakPoint.GetGatherDataValueCodeStatement(debugCodes, mCtrlValueOutG.GetLinkPinKeyName(),
                                                                   new CodeVariableReferenceExpression(GCode_GetValueName(mCtrlValueOutG, context)),
                                                                   GCode_GetTypeString(mCtrlValueOutG, context), context);
            CodeDomNode.BreakPoint.GetGatherDataValueCodeStatement(debugCodes, mCtrlValueOutB.GetLinkPinKeyName(),
                                                                   new CodeVariableReferenceExpression(GCode_GetValueName(mCtrlValueOutB, context)),
                                                                   GCode_GetTypeString(mCtrlValueOutB, context), context);
            CodeDomNode.BreakPoint.GetGatherDataValueCodeStatement(debugCodes, mCtrlValueOutA.GetLinkPinKeyName(),
                                                                   new CodeVariableReferenceExpression(GCode_GetValueName(mCtrlValueOutA, context)),
                                                                   GCode_GetTypeString(mCtrlValueOutA, context), context);
            CodeDomNode.BreakPoint.EndMacrossDebugCodeStatements(codeStatementCollection, debugCodes);
        }

        public void GCode_CodeDom_GenerateCode(CodeTypeDeclaration codeClass, CodeStatementCollection codeStatementCollection, CodeExpression expression, string strValueName)
        {
            GetGradientDatas();
            if (GradientDatas.Count > 1)
            {
                for (int i = 1; i < GradientDatas.Count; i++)
                {
                    var value = expression;
                    var offset1 = GradientDatas[i - 1].Offset;
                    var color1 = GradientDatas[i - 1].GradientColor;
                    var offset2 = GradientDatas[i].Offset;
                    var color2 = GradientDatas[i].GradientColor;

                    CodeObjectCreateExpression color1obj = new CodeObjectCreateExpression("EngineNS.Color4", new CodeExpression[] { });
                    color1obj.Parameters.Add(new CodePrimitiveExpression(color1.Alpha));
                    color1obj.Parameters.Add(new CodePrimitiveExpression(color1.Red));
                    color1obj.Parameters.Add(new CodePrimitiveExpression(color1.Green));
                    color1obj.Parameters.Add(new CodePrimitiveExpression(color1.Blue));

                    CodeObjectCreateExpression color2obj = new CodeObjectCreateExpression("EngineNS.Color4", new CodeExpression[] { });
                    color2obj.Parameters.Add(new CodePrimitiveExpression(color2.Alpha));
                    color2obj.Parameters.Add(new CodePrimitiveExpression(color2.Red));
                    color2obj.Parameters.Add(new CodePrimitiveExpression(color2.Green));
                    color2obj.Parameters.Add(new CodePrimitiveExpression(color2.Blue));

                    //"t = (value - offset1) / (offset2 - offset1) "
                    CodeBinaryOperatorExpression v1 = new CodeBinaryOperatorExpression(
                    value,
                    CodeBinaryOperatorType.Subtract,
                    new CodePrimitiveExpression(offset2));

                    CodeBinaryOperatorExpression v2 = new CodeBinaryOperatorExpression(
                    new CodePrimitiveExpression(offset1),
                    CodeBinaryOperatorType.Subtract,
                    new CodePrimitiveExpression(offset2));

                    CodeBinaryOperatorExpression v3 = new CodeBinaryOperatorExpression(
                    v1,
                    CodeBinaryOperatorType.Divide,
                    v2);

                    var typeref = new System.CodeDom.CodeTypeReferenceExpression("EngineNS.Color4");
                    //Lerp( Color4 color1, Color4 color2, float amount )
                    var methodInvoke = new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(
                    // targetObject that contains the method to invoke.
                    typeref,
                    // methodName indicates the method to invoke.
                    "Lerp",
                    // parameters array contains the parameters for the method.
                    new CodeExpression[] { color2obj, color1obj, v3 });

                    CodeAssignStatement result = new CodeAssignStatement(new CodeVariableReferenceExpression(strValueName), methodInvoke);
                    //  CodeBinaryOperatorExpression result = new CodeBinaryOperatorExpression(
                    //new CodeVariableReferenceExpression(strValueName),
                    // CodeBinaryOperatorType.Assign,
                    // methodInvoke);

                    var greaterthan = new CodeBinaryOperatorExpression(
                   value,
                   CodeBinaryOperatorType.GreaterThan,
                    new CodePrimitiveExpression(offset2));

                    var lessthan = new CodeBinaryOperatorExpression(
                  value,
                  CodeBinaryOperatorType.LessThan,
                   new CodePrimitiveExpression(offset1));

                    if (i == GradientDatas.Count - 1)
                    {
                        var first = new CodeBinaryOperatorExpression(
                          value,
                          CodeBinaryOperatorType.LessThanOrEqual,
                           new CodePrimitiveExpression(offset2));

                        codeStatementCollection.Add(new CodeConditionStatement(
                           first,
                           new CodeAssignStatement(new CodeVariableReferenceExpression(strValueName), color2obj)));
                    }

                    if (i == 1)
                    {
                        var last = new CodeBinaryOperatorExpression(
                         value,
                         CodeBinaryOperatorType.GreaterThanOrEqual,
                          new CodePrimitiveExpression(offset1));

                        codeStatementCollection.Add(new CodeConditionStatement(
                           last,
                           new CodeAssignStatement(new CodeVariableReferenceExpression(strValueName), color1obj)));
                    }
                    codeStatementCollection.Add(new CodeConditionStatement(
                           new CodeBinaryOperatorExpression(greaterthan, CodeBinaryOperatorType.BooleanAnd, lessthan),
                           result));

                }
            }
            else if (GradientDatas.Count == 1)
            {
                var color1 = GradientDatas[0].GradientColor;

                CodeObjectCreateExpression color1obj = new CodeObjectCreateExpression("EngineNS.Color4", new CodeExpression[] { });
                color1obj.Parameters.Add(new CodePrimitiveExpression(color1.Alpha));
                color1obj.Parameters.Add(new CodePrimitiveExpression(color1.Red));
                color1obj.Parameters.Add(new CodePrimitiveExpression(color1.Green));
                color1obj.Parameters.Add(new CodePrimitiveExpression(color1.Blue));

                codeStatementCollection.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(strValueName), color1obj));
            }
        }

        #endregion
    }
}
