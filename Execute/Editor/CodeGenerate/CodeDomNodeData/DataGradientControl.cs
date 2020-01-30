using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using CodeGenerateSystem.Base;
using EditorCommon.CodeGenerateSystem;
using EngineNS.IO;

namespace CodeDomNode
{
    [EngineNS.Rtti.MetaClass]
    public class DataGradientControlConstructParam : CodeGenerateSystem.Base.ConstructionParams
    {
        [EngineNS.Rtti.MetaData]
        public double Width { get; set; } = 687;
        [EngineNS.Rtti.MetaData]
        public double Height { get; set; } = 212;

        public override EditorCommon.CodeGenerateSystem.INodeConstructionParams Duplicate()
        {
            var retVal = base.Duplicate() as DataGradientControlConstructParam;
            retVal.Width = Width;
            retVal.Height = Height;
            return retVal;
        }
        public override bool Equals(object obj)
        {
            if (!base.Equals(obj))
                return false;
            var param = obj as DataGradientControlConstructParam;
            if (param == null)
                return false;
            if (Width != param.Width)
                return false;
            if (Height != param.Height)
                return false;
            return true;
        }
        public override int GetHashCode()
        {
            return (base.GetHashCodeString() + Width.ToString() + Height.ToString()).GetHashCode();
        }

    }
    [CodeGenerateSystem.CustomConstructionParams(typeof(DataGradientControlConstructParam))]
    public partial class DataGradientControl : CodeGenerateSystem.Base.BaseNodeControl
    {
        CodeGenerateSystem.Base.LinkPinControl mCtrlValueIn = new CodeGenerateSystem.Base.LinkPinControl();
        public CodeGenerateSystem.Base.LinkPinControl CtrlValueIn
        {
            get => mCtrlValueIn;
        }

        static Type mValueType = typeof(EngineNS.Color4);
        // 临时类，用于选中后显示参数属性

        List<CodeGenerateSystem.Base.LinkPinControl> mInComponentLinks = new List<CodeGenerateSystem.Base.LinkPinControl>();

        partial void InitConstruction();
        public DataGradientControl(CodeGenerateSystem.Base.ConstructionParams csParam)
            : base(csParam)
        {
            InitConstruction();

            var linkType = CodeGenerateSystem.Base.LinkPinControl.GetLinkTypeFromTypeString(mValueType.FullName);

            mInComponentLinks.Add(mCtrlValueIn);
            AddLinkPinInfo("CtrlValueIn", mCtrlValueIn, null);
        }
        public static void InitNodePinTypes(CodeGenerateSystem.Base.ConstructionParams smParam)
        {
            //var linkType = CodeGenerateSystem.Base.LinkPinControl.GetLinkTypeFromTypeString(mValueType.FullName);
            CollectLinkPinInfo(smParam, "CtrlValueIn", CodeGenerateSystem.Base.enLinkType.NumbericalValue, CodeGenerateSystem.Base.enBezierType.Left, CodeGenerateSystem.Base.enLinkOpType.End, false);
        }

        #region 生成代码

        public override string GCode_GetValueName(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            string strValueName = "";
            if (string.IsNullOrEmpty(NodeName) || NodeName == CodeGenerateSystem.Program.NodeDefaultName)
                strValueName = "value_" + EngineNS.Editor.Assist.GetValuedGUIDString(Id);
            else
                strValueName = NodeName;

            //TODO..

            return strValueName;
        }
        

        public override string GCode_GetTypeString(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            if (mInComponentLinks.Contains(element))
                return "System.Single";

            return mValueType.FullName;
        }

        public override Type GCode_GetType(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            if (mInComponentLinks.Contains(element))
                return typeof(System.Single);

            return mValueType;
        }
        
        public override CodeExpression GCode_CodeDom_GetValue(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context, CodeGenerateSystem.Base.GenerateCodeContext_PreNode preNodeContext = null)
        {
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
                if (!mCtrlValueIn.GetLinkedObject(0, true).IsOnlyReturnValue)
                    await mCtrlValueIn.GetLinkedObject(0, true).GCode_CodeDom_GenerateCode(codeClass, codeStatementCollection, mCtrlValueIn.GetLinkedPinControl(0, true), context);
                
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
            
            // 收集用于调试的数据的代码
        }

        #endregion
    }

    [EngineNS.Rtti.MetaClass]
    public class DataGradientElementConstructParam : CodeGenerateSystem.Base.ConstructionParams
    {
        [EngineNS.Rtti.MetaData]
        public int ElementIdx { get; set; }
        [EngineNS.Rtti.MetaData]
        public double RenderWidth { get; set; } = 442;
        [EngineNS.Rtti.MetaData]
        public string TypeStr { get; set; } = "Float";

        public override INodeConstructionParams Duplicate()
        {
            var retParam = base.Duplicate() as DataGradientElementConstructParam;
            retParam.ElementIdx = ElementIdx;
            retParam.RenderWidth = RenderWidth;
            retParam.TypeStr = TypeStr;
            return retParam;
        }
    }
    [CodeGenerateSystem.CustomConstructionParams(typeof(DataGradientElementConstructParam))]
    public partial class DataGradientElement : CodeGenerateSystem.Base.BaseNodeControl
    {
        [EngineNS.Rtti.MetaClass]
        public class GradientData : EngineNS.IO.Serializer.Serializer
        {
            [EngineNS.Rtti.MetaData]
            public float Offset;
            [EngineNS.Rtti.MetaData]
            public EngineNS.Vector4 Value;
        }

        public List<GradientData> GradientDatas = new List<GradientData>();


        CodeGenerateSystem.Base.LinkPinControl mElemPin = new CodeGenerateSystem.Base.LinkPinControl();

        public int ElementIdx
        {
            get
            {
                var param = CSParam as DataGradientElementConstructParam;
                if (param != null)
                    return param.ElementIdx;
                return 0;
            }
            set
            {
                var param = CSParam as DataGradientElementConstructParam;
                if (param != null)
                    param.ElementIdx = value;
                if (mElemPin != null)
                    mElemPin.NameString = "[" + value + "]";
                OnPropertyChanged("ElementIdx");
            }
        }

        partial void InitConstruction();
        public DataGradientElement(CodeGenerateSystem.Base.ConstructionParams csParam)
            : base(csParam)
        {
            var param = CSParam as DataGradientElementConstructParam;

            NodeType = enNodeType.ChildNode;
            
            InitConstruction();
            TypeStr = param.TypeStr;
            RenderWidth = param.RenderWidth;
            mElemPin.NameString = "[" + param.ElementIdx + "]";

            if (param.TypeStr.Equals("Float"))
            {
                AddLinkPinInfo("DataGradient1", mElemPin);
            }
            else if (param.TypeStr.Equals("Float2"))
            {
                AddLinkPinInfo("DataGradient2", mElemPin);
            }
            else if (param.TypeStr.Equals("Float3"))
            {
                AddLinkPinInfo("DataGradient3", mElemPin);
            }
            else if (param.TypeStr.Equals("Float4"))
            {
                AddLinkPinInfo("DataGradient4", mElemPin);
            }

        }

        public static void InitNodePinTypes(CodeGenerateSystem.Base.ConstructionParams csParam)
        {
            CollectLinkPinInfo(csParam, "DataGradient1", CodeGenerateSystem.Base.enLinkType.Single, CodeGenerateSystem.Base.enBezierType.Right, CodeGenerateSystem.Base.enLinkOpType.Start, false);
            CollectLinkPinInfo(csParam, "DataGradient2", CodeGenerateSystem.Base.enLinkType.Vector2, CodeGenerateSystem.Base.enBezierType.Right, CodeGenerateSystem.Base.enLinkOpType.Start, false);
            CollectLinkPinInfo(csParam, "DataGradient3", CodeGenerateSystem.Base.enLinkType.Vector3, CodeGenerateSystem.Base.enBezierType.Right, CodeGenerateSystem.Base.enLinkOpType.Start, false);
            CollectLinkPinInfo(csParam, "DataGradient4", CodeGenerateSystem.Base.enLinkType.Vector4, CodeGenerateSystem.Base.enBezierType.Right, CodeGenerateSystem.Base.enLinkOpType.Start, false);
        }

        partial void GetGradientDatas();
        partial void SetGradientDatas(List<GradientData> datas);

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

        public override string GCode_GetValueName(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            string strValueName = "";
            if (string.IsNullOrEmpty(NodeName) || NodeName == CodeGenerateSystem.Program.NodeDefaultName)
                strValueName = "value_" + EngineNS.Editor.Assist.GetValuedGUIDString(Id);
            else
                strValueName = NodeName;

            //TODO..

            return strValueName;
        }

        public override CodeExpression GCode_CodeDom_GetValue(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context, CodeGenerateSystem.Base.GenerateCodeContext_PreNode preNodeContext = null)
        {
            return new System.CodeDom.CodeVariableReferenceExpression(GCode_GetValueName(null, context));
        }

        Type mValueType = typeof(float);
        partial void SetValueType(string typestr)
        {
            if (typestr.Equals("Float"))
            {
                mValueType = typeof(float);
            }
            else if (typestr.Equals("Float2"))
            {
                mValueType = typeof(EngineNS.Vector2);
            }
            else if (typestr.Equals("Float3"))
            {
                mValueType = typeof(EngineNS.Vector3);
            }
            else if (typestr.Equals("Float4"))
            {
                mValueType = typeof(EngineNS.Vector4);
            }
        }
        
        System.CodeDom.CodeVariableDeclarationStatement mVariableDeclaration = new System.CodeDom.CodeVariableDeclarationStatement();
        public void GCode_CodeDom_GenerateCode(CodeTypeDeclaration codeClass, CodeStatementCollection codeStatementCollection, CodeExpression express, string strValueName)
        {
            bool isfloat = mValueType.Equals(typeof(float));
            GetGradientDatas();
            if (GradientDatas.Count > 1)
            {
                for (int i = 1; i < GradientDatas.Count; i++)
                {
                    var value = express;
                    var offset1 = GradientDatas[i - 1].Offset;
                    var value1 = GradientDatas[i - 1].Value;
                    var offset2 = GradientDatas[i].Offset;
                    var value2 = GradientDatas[i].Value;

                    CodeObjectCreateExpression value1obj = new CodeObjectCreateExpression();
                    CodeObjectCreateExpression value2obj = new CodeObjectCreateExpression();

                    CodePrimitiveExpression float1 = new CodePrimitiveExpression();
                    CodePrimitiveExpression float2 = new CodePrimitiveExpression();
                    if (mValueType.Equals(typeof(EngineNS.Vector2)))
                    {
                        value1obj = new CodeObjectCreateExpression("EngineNS.Vector2", new CodeExpression[] { });
                        value1obj.Parameters.Add(new CodePrimitiveExpression(value1.X));
                        value1obj.Parameters.Add(new CodePrimitiveExpression(value1.Y));

                        value2obj = new CodeObjectCreateExpression("EngineNS.Vector2", new CodeExpression[] { });
                        value2obj.Parameters.Add(new CodePrimitiveExpression(value2.X));
                        value2obj.Parameters.Add(new CodePrimitiveExpression(value2.Y));
                    }
                    else if (mValueType.Equals(typeof(EngineNS.Vector3)))
                    {
                        value1obj = new CodeObjectCreateExpression("EngineNS.Vector3", new CodeExpression[] { });
                        value1obj.Parameters.Add(new CodePrimitiveExpression(value1.X));
                        value1obj.Parameters.Add(new CodePrimitiveExpression(value1.Y));
                        value1obj.Parameters.Add(new CodePrimitiveExpression(value1.Z));

                        value2obj = new CodeObjectCreateExpression("EngineNS.Vector3", new CodeExpression[] { });
                        value2obj.Parameters.Add(new CodePrimitiveExpression(value2.X));
                        value2obj.Parameters.Add(new CodePrimitiveExpression(value2.Y));
                        value2obj.Parameters.Add(new CodePrimitiveExpression(value2.Z));
                    }
                    else if (mValueType.Equals(typeof(EngineNS.Vector4)))
                    {
                        value1obj = new CodeObjectCreateExpression("EngineNS.Vector4", new CodeExpression[] { });
                        value1obj.Parameters.Add(new CodePrimitiveExpression(value1.X));
                        value1obj.Parameters.Add(new CodePrimitiveExpression(value1.Y));
                        value1obj.Parameters.Add(new CodePrimitiveExpression(value1.Z));
                        value1obj.Parameters.Add(new CodePrimitiveExpression(value1.W));

                        value2obj = new CodeObjectCreateExpression("EngineNS.Vector4", new CodeExpression[] { });
                        value2obj.Parameters.Add(new CodePrimitiveExpression(value2.X));
                        value2obj.Parameters.Add(new CodePrimitiveExpression(value2.Y));
                        value2obj.Parameters.Add(new CodePrimitiveExpression(value2.Z));
                        value2obj.Parameters.Add(new CodePrimitiveExpression(value2.W));
                    }
                    else
                    {
                        float1 = new CodePrimitiveExpression(value1.X);

                        float2 = new CodePrimitiveExpression(value2.X);
                    }

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

                    CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression methodInvoke;
                    if (isfloat == false)
                    {
                        var typeref = new System.CodeDom.CodeTypeReferenceExpression(mValueType.FullName);
                        //Lerp( Color4 color1, Color4 color2, float amount )
                        methodInvoke = new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(
                        // targetObject that contains the method to invoke.
                        typeref,
                        // methodName indicates the method to invoke.
                        "Lerp",
                        // parameters array contains the parameters for the method.
                        new CodeExpression[] { value2obj, value1obj, v3 });
                    }
                    else
                    {
                        var typeref = new System.CodeDom.CodeTypeReferenceExpression(typeof(EngineNS.MathHelper).FullName);
                        methodInvoke = new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(
                       // targetObject that contains the method to invoke.
                       typeref,
                       // methodName indicates the method to invoke.
                       "FloatLerp",
                       // parameters array contains the parameters for the method.
                       new CodeExpression[] { float2, float1, v3 });

                    }


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

                        if (isfloat == false)
                        {
                            codeStatementCollection.Add(new CodeConditionStatement(
                            first,
                            new CodeAssignStatement(new CodeVariableReferenceExpression(strValueName), value2obj)));
                        }
                        else
                        {
                            codeStatementCollection.Add(new CodeConditionStatement(
                            first,
                            new CodeAssignStatement(new CodeVariableReferenceExpression(strValueName), float2)));
                        }

                    }

                    if (i == 1)
                    {
                        var last = new CodeBinaryOperatorExpression(
                         value,
                         CodeBinaryOperatorType.GreaterThanOrEqual,
                          new CodePrimitiveExpression(offset1));

                        if (isfloat == false)
                        {
                            codeStatementCollection.Add(new CodeConditionStatement(
                           last,
                           new CodeAssignStatement(new CodeVariableReferenceExpression(strValueName), value1obj)));
                        }
                        else
                        {
                            codeStatementCollection.Add(new CodeConditionStatement(
                           last,
                           new CodeAssignStatement(new CodeVariableReferenceExpression(strValueName), float1)));
                        }
                    }
                    codeStatementCollection.Add(new CodeConditionStatement(
                           new CodeBinaryOperatorExpression(greaterthan, CodeBinaryOperatorType.BooleanAnd, lessthan),
                           result));

                }
            }
            else if (GradientDatas.Count == 1)
            {
                var value1 = GradientDatas[0].Value;

                CodeObjectCreateExpression value1obj = new CodeObjectCreateExpression();

                CodePrimitiveExpression float1 = new CodePrimitiveExpression();

                if (mValueType.Equals(typeof(EngineNS.Vector2)))
                {
                    value1obj = new CodeObjectCreateExpression("EngineNS.Vector2", new CodeExpression[] { });
                    value1obj.Parameters.Add(new CodePrimitiveExpression(value1.X));
                    value1obj.Parameters.Add(new CodePrimitiveExpression(value1.Y));
                }
                else if (mValueType.Equals(typeof(EngineNS.Vector3)))
                {
                    value1obj = new CodeObjectCreateExpression("EngineNS.Vector3", new CodeExpression[] { });
                    value1obj.Parameters.Add(new CodePrimitiveExpression(value1.X));
                    value1obj.Parameters.Add(new CodePrimitiveExpression(value1.Y));
                    value1obj.Parameters.Add(new CodePrimitiveExpression(value1.Z));
                }
                else if (mValueType.Equals(typeof(EngineNS.Vector4)))
                {
                    value1obj = new CodeObjectCreateExpression("EngineNS.Vector4", new CodeExpression[] { });
                    value1obj.Parameters.Add(new CodePrimitiveExpression(value1.X));
                    value1obj.Parameters.Add(new CodePrimitiveExpression(value1.Y));
                    value1obj.Parameters.Add(new CodePrimitiveExpression(value1.Z));
                    value1obj.Parameters.Add(new CodePrimitiveExpression(value1.W));
                }
                else
                {
                    float1 = new CodePrimitiveExpression(value1.X);
                }

                if (isfloat == false)
                {
                    codeStatementCollection.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(strValueName), value1obj));
                }
                else
                {
                    codeStatementCollection.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(strValueName), float1));
                }

            }
        }
        public override async Task GCode_CodeDom_GenerateCode(CodeTypeDeclaration codeClass, CodeStatementCollection codeStatementCollection, LinkPinControl element, GenerateCodeContext_Method context)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            var strValueName = GCode_GetValueName(null, context);

            if (!context.Method.Statements.Contains(mVariableDeclaration))
            {
                mVariableDeclaration.Type = new CodeTypeReference(mValueType);
                mVariableDeclaration.Name = strValueName;
                if (mValueType.Equals(typeof(float)))
                {
                    mVariableDeclaration.InitExpression = new CodePrimitiveExpression(0.0f);
                }
                else if (mValueType.Equals(typeof(EngineNS.Vector2)))
                {
                    mVariableDeclaration.InitExpression = new CodeObjectCreateExpression(mValueType, new CodePrimitiveExpression(0.0f),
                                                                        new CodePrimitiveExpression(0.0f));
                }
                else if(mValueType.Equals(typeof(EngineNS.Vector3)))
                {
                    mVariableDeclaration.InitExpression = new CodeObjectCreateExpression(mValueType, new CodePrimitiveExpression(0.0f),
                                                                        new CodePrimitiveExpression(0.0f), new CodePrimitiveExpression(0.0f));
                }
                else if(mValueType.Equals(typeof(EngineNS.Vector4)))
                {
                    mVariableDeclaration.InitExpression = new CodeObjectCreateExpression(mValueType, new CodePrimitiveExpression(0.0f),
                                                                        new CodePrimitiveExpression(0.0f), new CodePrimitiveExpression(0.0f), new CodePrimitiveExpression(0.0f));
                }

                context.Method.Statements.Insert(0, mVariableDeclaration);
            }

            var node = ParentNode as DataGradientControl;
            if (mElemPin.HasLink && node.CtrlValueIn.HasLink)
            {
                //var linkObj = mElemPin.GetLinkedObject(0);
                //await linkObj.GCode_CodeDom_GenerateCode(codeClass, codeStatementCollection, mElemPin.GetLinkedPinControl(0), context);

                if (!node.CtrlValueIn.GetLinkedObject(0).IsOnlyReturnValue)
                    await node.CtrlValueIn.GetLinkedObject(0).GCode_CodeDom_GenerateCode(codeClass, codeStatementCollection, node.CtrlValueIn.GetLinkedPinControl(0), context);

                GCode_CodeDom_GenerateCode(codeClass, codeStatementCollection,
                    node.CtrlValueIn.GetLinkedObject(0).GCode_CodeDom_GetValue(node.CtrlValueIn.GetLinkedPinControl(0), context), strValueName);
               
            }
        }
    }

}
