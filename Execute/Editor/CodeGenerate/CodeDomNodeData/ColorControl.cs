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
    public class ColorControlConstructParam : CodeGenerateSystem.Base.ConstructionParams
    {

    }
    [CodeGenerateSystem.CustomConstructionParams(typeof(ColorControlConstructParam))]
    public partial class ColorControl : CodeGenerateSystem.Base.BaseNodeControl
    {
        CodeGenerateSystem.Base.LinkPinControl mCtrlvalue_ColorIn = new CodeGenerateSystem.Base.LinkPinControl();
        CodeGenerateSystem.Base.LinkPinControl mCtrlvalue_ColorOut = new CodeGenerateSystem.Base.LinkPinControl();
        CodeGenerateSystem.Base.LinkPinControl mCtrlValueInR = new CodeGenerateSystem.Base.LinkPinControl();
        CodeGenerateSystem.Base.LinkPinControl mCtrlValueInG = new CodeGenerateSystem.Base.LinkPinControl();
        CodeGenerateSystem.Base.LinkPinControl mCtrlValueInB = new CodeGenerateSystem.Base.LinkPinControl();
        CodeGenerateSystem.Base.LinkPinControl mCtrlValueInA = new CodeGenerateSystem.Base.LinkPinControl();
        CodeGenerateSystem.Base.LinkPinControl mCtrlValueOutR = new CodeGenerateSystem.Base.LinkPinControl();
        CodeGenerateSystem.Base.LinkPinControl mCtrlValueOutG = new CodeGenerateSystem.Base.LinkPinControl();
        CodeGenerateSystem.Base.LinkPinControl mCtrlValueOutB = new CodeGenerateSystem.Base.LinkPinControl();
        CodeGenerateSystem.Base.LinkPinControl mCtrlValueOutA = new CodeGenerateSystem.Base.LinkPinControl();

        static Type mValueType = typeof(EngineNS.Color4);
        // 临时类，用于选中后显示参数属性
        CodeGenerateSystem.Base.GeneratorClassBase mTemplateClassInstance = null;

        SolidColorBrush mColorBrush = Brushes.White;
        public SolidColorBrush ColorBrush
        {
            get { return mColorBrush; }
            set
            {
                mColorBrush = value;
                OnPropertyChanged("ColorBrush");
            }
        }

        List<CodeGenerateSystem.Base.LinkPinControl> mInComponentLinks = new List<CodeGenerateSystem.Base.LinkPinControl>();
        List<CodeGenerateSystem.Base.LinkPinControl> mOutComponentLinks = new List<CodeGenerateSystem.Base.LinkPinControl>();

        partial void InitConstruction();
        public ColorControl(CodeGenerateSystem.Base.ConstructionParams csParam)
            : base(csParam)
        {
            var cpInfos = new List<CodeGenerateSystem.Base.CustomPropertyInfo>();
            var cpInfo = CodeGenerateSystem.Base.CustomPropertyInfo.GetFromParamInfo(typeof(EngineNS.Color), "Color", new Attribute[] { new DisplayNameAttribute("颜色"), new EngineNS.Editor.Editor_ColorPicker(), new EngineNS.Rtti.MetaDataAttribute() });
            cpInfos.Add(cpInfo);
            mTemplateClassInstance = CodeGenerateSystem.Base.PropertyClassGenerator.CreateClassInstanceFromCustomPropertys(cpInfos, this);
            InitConstruction();

            var linkType = CodeGenerateSystem.Base.LinkPinControl.GetLinkTypeFromTypeString(mValueType.FullName);
            AddLinkPinInfo("Ctrlvalue_ColorIn", mCtrlvalue_ColorIn, null);
            AddLinkPinInfo("Ctrlvalue_ColorOut", mCtrlvalue_ColorOut, null);

            mInComponentLinks.Add(mCtrlValueInR);
            mInComponentLinks.Add(mCtrlValueInG);
            mInComponentLinks.Add(mCtrlValueInB);
            mInComponentLinks.Add(mCtrlValueInA);
            AddLinkPinInfo("CtrlValueInR", mCtrlValueInR, null);
            AddLinkPinInfo("CtrlValueInG", mCtrlValueInG, null);
            AddLinkPinInfo("CtrlValueInB", mCtrlValueInB, null);
            AddLinkPinInfo("CtrlValueInA", mCtrlValueInA, null);

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
            CollectLinkPinInfo(smParam, "Ctrlvalue_ColorIn", mValueType, CodeGenerateSystem.Base.enBezierType.Left, CodeGenerateSystem.Base.enLinkOpType.End, false);
            CollectLinkPinInfo(smParam, "Ctrlvalue_ColorOut", mValueType, CodeGenerateSystem.Base.enBezierType.Right, CodeGenerateSystem.Base.enLinkOpType.Start, true);
            CollectLinkPinInfo(smParam, "CtrlValueInR", CodeGenerateSystem.Base.enLinkType.NumbericalValue, CodeGenerateSystem.Base.enBezierType.Left, CodeGenerateSystem.Base.enLinkOpType.End, false);
            CollectLinkPinInfo(smParam, "CtrlValueInG", CodeGenerateSystem.Base.enLinkType.NumbericalValue, CodeGenerateSystem.Base.enBezierType.Left, CodeGenerateSystem.Base.enLinkOpType.End, false);
            CollectLinkPinInfo(smParam, "CtrlValueInB", CodeGenerateSystem.Base.enLinkType.NumbericalValue, CodeGenerateSystem.Base.enBezierType.Left, CodeGenerateSystem.Base.enLinkOpType.End, false);
            CollectLinkPinInfo(smParam, "CtrlValueInA", CodeGenerateSystem.Base.enLinkType.NumbericalValue, CodeGenerateSystem.Base.enBezierType.Left, CodeGenerateSystem.Base.enLinkOpType.End, false);
            CollectLinkPinInfo(smParam, "CtrlValueOutR", CodeGenerateSystem.Base.enLinkType.NumbericalValue, CodeGenerateSystem.Base.enBezierType.Right, CodeGenerateSystem.Base.enLinkOpType.Start, true);
            CollectLinkPinInfo(smParam, "CtrlValueOutG", CodeGenerateSystem.Base.enLinkType.NumbericalValue, CodeGenerateSystem.Base.enBezierType.Right, CodeGenerateSystem.Base.enLinkOpType.Start, true);
            CollectLinkPinInfo(smParam, "CtrlValueOutB", CodeGenerateSystem.Base.enLinkType.NumbericalValue, CodeGenerateSystem.Base.enBezierType.Right, CodeGenerateSystem.Base.enLinkOpType.Start, true);
            CollectLinkPinInfo(smParam, "CtrlValueOutA", CodeGenerateSystem.Base.enLinkType.NumbericalValue, CodeGenerateSystem.Base.enBezierType.Right, CodeGenerateSystem.Base.enLinkOpType.Start, true);
        }

        #region SaveLoad

        public override void Save(XndNode xndNode, bool newGuid)
        {
            var att = xndNode.AddAttrib("DefaultParamValue");
            att.Version = 1;
            att.BeginWrite();
            CodeGenerateSystem.Base.PropertyClassGenerator.SaveClassInstanceProperties(mTemplateClassInstance, att);
            att.EndWrite();
            base.Save(xndNode, newGuid);
        }

        public override async System.Threading.Tasks.Task Load(XndNode xndNode)
        {
            await base.Load(xndNode);
            var att = xndNode.FindAttrib("DefaultParamValue");
            if(att != null)
            {
                att.BeginRead();
                switch(att.Version)
                {
                    case 0:
                        {
                            att.ReadMetaObject(mTemplateClassInstance);
                        }
                        break;
                    case 1:
                        {
                            CodeGenerateSystem.Base.PropertyClassGenerator.LoadClassInstanceProperties(mTemplateClassInstance, att);
                        }
                        break;
                }
                att.EndRead();
            }

            var classType = mTemplateClassInstance.GetType();
            var property = classType.GetProperty("Color");
            EngineNS.Color color = (EngineNS.Color)property.GetValue(mTemplateClassInstance);
            ColorBrush = new SolidColorBrush(Color.FromArgb(color.A, color.R, color.G, color.B));
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
            if (element == mCtrlValueOutR || element == mCtrlValueInR)
                return "Red";
            else if (element == mCtrlValueOutG || element == mCtrlValueInG)
                return "Green";
            else if (element == mCtrlValueOutB || element == mCtrlValueInB)
                return "Blue";
            else if (element == mCtrlValueOutA || element == mCtrlValueInA)
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

        public override CodeExpression GCode_CodeDom_GetValue(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context, CodeGenerateSystem.Base.GenerateCodeContext_PreNode preNodeContext = null)
        {
            if (element == mCtrlValueOutR || element == mCtrlValueInR)
                return new CodeFieldReferenceExpression(new CodeVariableReferenceExpression(GCode_GetValueName(null, context)), "Red");
            else if (element == mCtrlValueOutG || element == mCtrlValueInG)
                return new CodeFieldReferenceExpression(new CodeVariableReferenceExpression(GCode_GetValueName(null, context)), "Green");
            else if (element == mCtrlValueOutB || element == mCtrlValueInB)
                return new CodeFieldReferenceExpression(new CodeVariableReferenceExpression(GCode_GetValueName(null, context)), "Blue");
            else if (element == mCtrlValueOutA || element == mCtrlValueInA)
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
                mVariableDeclaration.InitExpression = new CodeObjectCreateExpression(mValueType, new CodePrimitiveExpression(ColorBrush.Color.A / 255.0f),
                                                                         new CodePrimitiveExpression(ColorBrush.Color.R / 255.0f),
                                                                         new CodePrimitiveExpression(ColorBrush.Color.G / 255.0f),
                                                                         new CodePrimitiveExpression(ColorBrush.Color.B / 255.0f));
                context.Method.Statements.Insert(0, mVariableDeclaration);
            }

            if (mCtrlvalue_ColorIn.HasLink)
            {
                if (!mCtrlvalue_ColorIn.GetLinkedObject(0, true).IsOnlyReturnValue)
                    await mCtrlvalue_ColorIn.GetLinkedObject(0, true).GCode_CodeDom_GenerateCode(codeClass, codeStatementCollection, element, context);

                var assignStatement = new System.CodeDom.CodeAssignStatement(
                                                                        new System.CodeDom.CodeVariableReferenceExpression(strValueName),
                                                                        mCtrlvalue_ColorIn.GetLinkedObject(0, true).GCode_CodeDom_GetValue(mCtrlvalue_ColorIn.GetLinkedPinControl(0, true), context));
                codeStatementCollection.Add(assignStatement);
            }

            // r、g、b、a有链接的情况
            foreach (var link in mInComponentLinks)
            {
                if (link.HasLink)
                {
                    if (!link.GetLinkedObject(0, true).IsOnlyReturnValue)
                        await link.GetLinkedObject(0, true).GCode_CodeDom_GenerateCode(codeClass, codeStatementCollection, element, context);

                    var fieldRef = new CodeFieldReferenceExpression();
                    fieldRef.TargetObject = new CodeVariableReferenceExpression(strValueName);
                    fieldRef.FieldName = GetValuePureName(link);
                    var statValAss = new CodeAssignStatement();
                    statValAss.Left = fieldRef;
                    statValAss.Right = new CodeGenerateSystem.CodeDom.CodeCastExpression(typeof(float), link.GetLinkedObject(0, true).GCode_CodeDom_GetValue(link.GetLinkedPinControl(0, true), context));
                    codeStatementCollection.Add(statValAss);
                }
            }

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

        #endregion
    }
}
