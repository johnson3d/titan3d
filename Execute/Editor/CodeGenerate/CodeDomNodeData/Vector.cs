using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Text;
using EngineNS.IO;

namespace CodeDomNode
{
    [CodeGenerateSystem.CustomConstructionParams(typeof(VectorConstructionParams))]
    public partial class Vector : CodeGenerateSystem.Base.BaseNodeControl
    {
        CodeGenerateSystem.Base.LinkPinControl mCtrlvalue_VectorIn = new CodeGenerateSystem.Base.LinkPinControl();
        CodeGenerateSystem.Base.LinkPinControl mCtrlvalue_VectorOut = new CodeGenerateSystem.Base.LinkPinControl();

        Type mValueType;
        class linkData
        {
            public CodeGenerateSystem.Base.LinkPinControl Element;
            public string KeyName;
        }
        List<linkData> mLinkInDic = new List<linkData>();
        List<linkData> mLinkOutDic = new List<linkData>();

        public float X
        {
            get
            {
                var param = CSParam as VectorConstructionParams;
                if(param != null)
                    return param.Value.X;
                return 0;
            }
            set
            {
                var param = CSParam as VectorConstructionParams;
                if(param != null)
                    param.Value.X = value;
                OnPropertyChanged("X");
            }
        }
        public float Y
        {
            get
            {
                var param = CSParam as VectorConstructionParams;
                if (param != null)
                    return param.Value.Y;
                return 0;
            }
            set
            {
                var param = CSParam as VectorConstructionParams;
                if (param != null)
                    param.Value.Y = value;
                OnPropertyChanged("Y");
            }
        }
        public float Z
        {
            get
            {
                var param = CSParam as VectorConstructionParams;
                if (param != null)
                    return param.Value.Z;
                return 0;
            }
            set
            {
                var param = CSParam as VectorConstructionParams;
                if (param != null)
                    param.Value.Z = value;
                OnPropertyChanged("Z");
            }
        }
        public float W
        {
            get
            {
                var param = CSParam as VectorConstructionParams;
                if (param != null)
                    return param.Value.W;
                return 0;
            }
            set
            {
                var param = CSParam as VectorConstructionParams;
                if (param != null)
                    param.Value.W = value;
                OnPropertyChanged("W");
            }
        }

        [EngineNS.Rtti.MetaClass]
        public class VectorConstructionParams : CodeGenerateSystem.Base.ConstructionParams
        {
            [EngineNS.Rtti.MetaData]
            public CodeGenerateSystem.Base.enLinkType LinkType { get; set; } = CodeGenerateSystem.Base.enLinkType.Vector3;
            [EngineNS.Rtti.MetaData]
            public EngineNS.Vector4 Value;

            public override EditorCommon.CodeGenerateSystem.INodeConstructionParams Duplicate()
            {
                var retVal = base.Duplicate() as VectorConstructionParams;
                retVal.LinkType = LinkType;
                retVal.Value = Value;
                return retVal;
            }
            public override bool Equals(object obj)
            {
                if (!base.Equals(obj))
                    return false;
                var param = obj as VectorConstructionParams;
                if (param == null)
                    return false;
                if (LinkType == param.LinkType)
                    return true;
                return false;
            }
            public override int GetHashCode()
            {
                return (base.GetHashCodeString() + LinkType.ToString()).GetHashCode();
            }
        }

        partial void InitConstruction();
        public Vector(CodeGenerateSystem.Base.ConstructionParams csParam)
            : base(csParam)
        {
            InitConstruction();
            AddLinkPinInfo("Ctrlvalue_VectorIn", mCtrlvalue_VectorIn, null);
            AddLinkPinInfo("Ctrlvalue_VectorOut", mCtrlvalue_VectorOut, null);

            var param = csParam as VectorConstructionParams;
            if(param != null)
            {
                switch (param.LinkType)
                {
                    case CodeGenerateSystem.Base.enLinkType.Vector2:
                        mValueType = typeof(EngineNS.Vector2);
                        AddFloatValue("X");
                        AddFloatValue("Y");
                        break;
                    case CodeGenerateSystem.Base.enLinkType.Vector3:
                        mValueType = typeof(EngineNS.Vector3);
                        AddFloatValue("X");
                        AddFloatValue("Y");
                        AddFloatValue("Z");
                        break;
                    case CodeGenerateSystem.Base.enLinkType.Vector4:
                        mValueType = typeof(EngineNS.Vector4);
                        AddFloatValue("X");
                        AddFloatValue("Y");
                        AddFloatValue("Z");
                        AddFloatValue("W");
                        break;
                }
            }
            NodeName = mValueType.Name;
        }
        public static void InitNodePinTypes(CodeGenerateSystem.Base.ConstructionParams smParam)
        {
            var param = smParam as VectorConstructionParams;
            if(param != null)
            {
                CollectLinkPinInfo(smParam, "Ctrlvalue_VectorIn", param.LinkType, CodeGenerateSystem.Base.enBezierType.Left, CodeGenerateSystem.Base.enLinkOpType.End, false);
                CollectLinkPinInfo(smParam, "Ctrlvalue_VectorOut", param.LinkType, CodeGenerateSystem.Base.enBezierType.Right, CodeGenerateSystem.Base.enLinkOpType.Start, true);
                switch (param.LinkType)
                {
                    case CodeGenerateSystem.Base.enLinkType.Vector2:
                        CollectLinkPinInfo(smParam, "X_in", CodeGenerateSystem.Base.enLinkType.NumbericalValue, CodeGenerateSystem.Base.enBezierType.Left, CodeGenerateSystem.Base.enLinkOpType.End, false);
                        CollectLinkPinInfo(smParam, "X_out", CodeGenerateSystem.Base.enLinkType.NumbericalValue, CodeGenerateSystem.Base.enBezierType.Right, CodeGenerateSystem.Base.enLinkOpType.Start, true);

                        CollectLinkPinInfo(smParam, "Y_in", CodeGenerateSystem.Base.enLinkType.NumbericalValue, CodeGenerateSystem.Base.enBezierType.Left, CodeGenerateSystem.Base.enLinkOpType.End, false);
                        CollectLinkPinInfo(smParam, "Y_out", CodeGenerateSystem.Base.enLinkType.NumbericalValue, CodeGenerateSystem.Base.enBezierType.Right, CodeGenerateSystem.Base.enLinkOpType.Start, true);
                        break;
                    case CodeGenerateSystem.Base.enLinkType.Vector3:
                        CollectLinkPinInfo(smParam, "X_in", CodeGenerateSystem.Base.enLinkType.NumbericalValue, CodeGenerateSystem.Base.enBezierType.Left, CodeGenerateSystem.Base.enLinkOpType.End, false);
                        CollectLinkPinInfo(smParam, "X_out", CodeGenerateSystem.Base.enLinkType.NumbericalValue, CodeGenerateSystem.Base.enBezierType.Right, CodeGenerateSystem.Base.enLinkOpType.Start, true);

                        CollectLinkPinInfo(smParam, "Y_in", CodeGenerateSystem.Base.enLinkType.NumbericalValue, CodeGenerateSystem.Base.enBezierType.Left, CodeGenerateSystem.Base.enLinkOpType.End, false);
                        CollectLinkPinInfo(smParam, "Y_out", CodeGenerateSystem.Base.enLinkType.NumbericalValue, CodeGenerateSystem.Base.enBezierType.Right, CodeGenerateSystem.Base.enLinkOpType.Start, true);

                        CollectLinkPinInfo(smParam, "Z_in", CodeGenerateSystem.Base.enLinkType.NumbericalValue, CodeGenerateSystem.Base.enBezierType.Left, CodeGenerateSystem.Base.enLinkOpType.End, false);
                        CollectLinkPinInfo(smParam, "Z_out", CodeGenerateSystem.Base.enLinkType.NumbericalValue, CodeGenerateSystem.Base.enBezierType.Right, CodeGenerateSystem.Base.enLinkOpType.Start, true);
                        break;
                    case CodeGenerateSystem.Base.enLinkType.Vector4:
                        CollectLinkPinInfo(smParam, "X_in", CodeGenerateSystem.Base.enLinkType.NumbericalValue, CodeGenerateSystem.Base.enBezierType.Left, CodeGenerateSystem.Base.enLinkOpType.End, false);
                        CollectLinkPinInfo(smParam, "X_out", CodeGenerateSystem.Base.enLinkType.NumbericalValue, CodeGenerateSystem.Base.enBezierType.Right, CodeGenerateSystem.Base.enLinkOpType.Start, true);

                        CollectLinkPinInfo(smParam, "Y_in", CodeGenerateSystem.Base.enLinkType.NumbericalValue, CodeGenerateSystem.Base.enBezierType.Left, CodeGenerateSystem.Base.enLinkOpType.End, false);
                        CollectLinkPinInfo(smParam, "Y_out", CodeGenerateSystem.Base.enLinkType.NumbericalValue, CodeGenerateSystem.Base.enBezierType.Right, CodeGenerateSystem.Base.enLinkOpType.Start, true);

                        CollectLinkPinInfo(smParam, "Z_in", CodeGenerateSystem.Base.enLinkType.NumbericalValue, CodeGenerateSystem.Base.enBezierType.Left, CodeGenerateSystem.Base.enLinkOpType.End, false);
                        CollectLinkPinInfo(smParam, "Z_out", CodeGenerateSystem.Base.enLinkType.NumbericalValue, CodeGenerateSystem.Base.enBezierType.Right, CodeGenerateSystem.Base.enLinkOpType.Start, true);

                        CollectLinkPinInfo(smParam, "W_in", CodeGenerateSystem.Base.enLinkType.NumbericalValue, CodeGenerateSystem.Base.enBezierType.Left, CodeGenerateSystem.Base.enLinkOpType.End, false);
                        CollectLinkPinInfo(smParam, "W_out", CodeGenerateSystem.Base.enLinkType.NumbericalValue, CodeGenerateSystem.Base.enBezierType.Right, CodeGenerateSystem.Base.enLinkOpType.Start, true);
                        break;
                }
            }
        }

        partial void AddFloatValue_WPF(Action<CodeGenerateSystem.Base.LinkPinControl> inAction, Action<CodeGenerateSystem.Base.LinkPinControl> outAction, string keyName);
        void AddFloatValue(string keyName)
        {
            var inCtrl = new CodeGenerateSystem.Base.LinkPinControl();
            var outCtrl = new CodeGenerateSystem.Base.LinkPinControl();

            AddFloatValue_WPF(
                (inC)=>
                {
                    inCtrl = inC;
                },
                (outC)=>
                {
                    outCtrl = outC;
                },
                keyName);
            AddLinkPinInfo($"{keyName}_in", inCtrl, null);
            var linkData_In = new linkData()
            {
                Element = inCtrl,
                KeyName = keyName,
            };
            mLinkInDic.Add(linkData_In);

            AddLinkPinInfo($"{keyName}_out", outCtrl, null);
            var linkData_Out = new linkData()
            {
                Element = outCtrl,
                KeyName = keyName,
            };
            mLinkOutDic.Add(linkData_Out);
        }
        
        public override string GCode_GetValueName(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            string vecName = "vec_" + EngineNS.Editor.Assist.GetValuedGUIDString(Id);
            if (element == null || element == mCtrlvalue_VectorOut)
                return vecName;
            else
            {
                foreach (var data in mLinkOutDic)
                {
                    if (data.Element == element)
                    {
                        return vecName + "." + data.KeyName;
                    }
                }
                foreach (var data in mLinkInDic)
                {
                    if (data.Element == element)
                        return vecName + "." + data.KeyName;
                }
            }

            return vecName;
        }

        public override string GCode_GetTypeString(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            if (element == mCtrlvalue_VectorOut)
                return mValueType.FullName;
            else
            {
                foreach (var data in mLinkOutDic)
                {
                    if (data.Element == element)
                    {
                        return "System.Single";
                    }
                }
                foreach (var data in mLinkInDic)
                {
                    if (data.Element == element)
                        return "System.Single";
                }
            }

            return mValueType.FullName;
        }

        public override Type GCode_GetType(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            if (element == mCtrlvalue_VectorOut)
                return mValueType;
            else
            {
                foreach (var data in mLinkOutDic)
                {
                    if (data.Element == element)
                    {
                        return typeof(System.Single);
                    }
                }
                foreach (var data in mLinkInDic)
                {
                    if (data.Element == element)
                        return typeof(System.Single);
                }
            }

            return mValueType;
        }

        public override CodeExpression GCode_CodeDom_GetValue(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context, CodeGenerateSystem.Base.GenerateCodeContext_PreNode preNodeContext = null)
        {
            if (element == mCtrlvalue_VectorOut)
                return new System.CodeDom.CodeVariableReferenceExpression(GCode_GetValueName(null, context));
            else
            {
                foreach (var data in mLinkOutDic)
                {
                    if (data.Element == element)
                        return new System.CodeDom.CodeVariableReferenceExpression(GCode_GetValueName(null, context) + "." + data.KeyName);
                }
            }

            return base.GCode_CodeDom_GetValue(element, context);
        }

        System.CodeDom.CodeStatement mVarDec;
        System.CodeDom.CodeAssignStatement mAssignCode;
        //System.CodeDom.CodeVariableDeclarationStatement mVecVariableDeclaration = new System.CodeDom.CodeVariableDeclarationStatement();
        public override async System.Threading.Tasks.Task GCode_CodeDom_GenerateCode(CodeTypeDeclaration codeClass, CodeStatementCollection codeStatementCollection, CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            var strValueName = GCode_GetValueName(null, context);
            if (!context.Method.Statements.Contains(mVarDec))
            {
                mVarDec = new CodeAssignStatement(new CodeSnippetExpression(mValueType.FullName + " " + strValueName), CodeGenerateSystem.Program.GetDefaultValueExpressionFromType(mValueType));//, paramCodeName, CodeGenerateSystem.Program.GetDefaultValueExpressionFromType(ParamType));
                context.Method.Statements.Insert(0, mVarDec);
            }
            if (mCtrlvalue_VectorIn.HasLink)
            {
                if (!mCtrlvalue_VectorIn.GetLinkedObject(0, true).IsOnlyReturnValue)
                    await mCtrlvalue_VectorIn.GetLinkedObject(0, true).GCode_CodeDom_GenerateCode(codeClass, codeStatementCollection, mCtrlvalue_VectorIn.GetLinkedPinControl(0, true), context);


                if (!codeStatementCollection.Contains(mAssignCode))
                {
                    mAssignCode = new CodeAssignStatement(new System.CodeDom.CodeVariableReferenceExpression(strValueName), mCtrlvalue_VectorIn.GetLinkedObject(0, true).GCode_CodeDom_GetValue(mCtrlvalue_VectorIn.GetLinkedPinControl(0, true), context));
                    codeStatementCollection.Add(mAssignCode);
                }
            }
            else
            {
                if (!codeStatementCollection.Contains(mAssignCode))
                {
                    mAssignCode = new CodeAssignStatement();
                    mAssignCode.Left = new System.CodeDom.CodeVariableReferenceExpression(strValueName);
                    var paramExp = new System.CodeDom.CodeExpression[mLinkInDic.Count];
                    var param = CSParam as VectorConstructionParams;
                    if(param != null)
                    {
                        for (int i = 0; i < mLinkInDic.Count; i++)
                        {
                            paramExp[i] = new System.CodeDom.CodePrimitiveExpression(param.Value[i]);
                        }
                    }
                    mAssignCode.Right = new CodeObjectCreateExpression(mValueType, paramExp);
                    codeStatementCollection.Add(mAssignCode);
                }
            }

            foreach (var data in mLinkInDic)
            {
                var linkOI = data.Element;
                if (linkOI.HasLink)
                {
                    if (!linkOI.GetLinkedObject(0, true).IsOnlyReturnValue)
                        await linkOI.GetLinkedObject(0, true).GCode_CodeDom_GenerateCode(codeClass, codeStatementCollection, linkOI.GetLinkedPinControl(0, true), context);

                    var fieldRef = new System.CodeDom.CodeFieldReferenceExpression();
                    fieldRef.TargetObject = new CodeVariableReferenceExpression(strValueName);
                    fieldRef.FieldName = data.KeyName;
                    var statValAss = new System.CodeDom.CodeAssignStatement();
                    statValAss.Left = fieldRef;
                    statValAss.Right = new CodeGenerateSystem.CodeDom.CodeCastExpression(typeof(float), linkOI.GetLinkedObject(0, true).GCode_CodeDom_GetValue(linkOI.GetLinkedPinControl(0, true), context));
                    codeStatementCollection.Add(statValAss);
                }
            }
        }
    }
}
