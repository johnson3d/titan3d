using System;
using System.Collections.Generic;
using System.Text;

namespace CodeDomNode
{
    [EngineNS.Rtti.MetaClass]
    public class ArithmeticConstructParam : CodeGenerateSystem.Base.ConstructionParams
    {

    }
    [CodeGenerateSystem.CustomConstructionParams(typeof(ArithmeticConstructParam))]
    public partial class Arithmetic : CodeGenerateSystem.Base.BaseNodeControl
    {
        CodeGenerateSystem.Base.LinkPinControl mCtrlValue1 = new CodeGenerateSystem.Base.LinkPinControl();
        CodeGenerateSystem.Base.LinkPinControl mCtrlValue2 = new CodeGenerateSystem.Base.LinkPinControl();
        CodeGenerateSystem.Base.LinkPinControl mCtrlResultLink = new CodeGenerateSystem.Base.LinkPinControl();

        public override bool HasMultiOutLink
        {
            get
            {
                return mCtrlResultLink.GetLinkInfosCount() > 0;
            }
        }

        static CodeGenerateSystem.Base.enLinkType GetLinkType(CodeGenerateSystem.Base.ConstructionParams csParam)
        {
            switch (csParam.ConstructParam)
            {
                case "＋":
                    return CodeGenerateSystem.Base.enLinkType.NumbericalValue | CodeGenerateSystem.Base.enLinkType.VectorValue | CodeGenerateSystem.Base.enLinkType.String;
                case "－":
                case "×":
                case "÷":
                    return CodeGenerateSystem.Base.enLinkType.NumbericalValue | CodeGenerateSystem.Base.enLinkType.VectorValue;
                case "&&":
                case "||":
                    return CodeGenerateSystem.Base.enLinkType.Bool;
                case "&":
                case "|":
                    return CodeGenerateSystem.Base.enLinkType.Byte | CodeGenerateSystem.Base.enLinkType.UInt16 | CodeGenerateSystem.Base.enLinkType.UInt32 | CodeGenerateSystem.Base.enLinkType.UInt64;
            }
            return CodeGenerateSystem.Base.enLinkType.Unknow;
        }

        partial void InitConstruction();
        public Arithmetic(CodeGenerateSystem.Base.ConstructionParams csParam)
            : base(csParam)
        {
            InitConstruction();

            var inPin1 = AddLinkPinInfo("CtrlValue1", mCtrlValue1, null);
            inPin1.OnAddLinkInfo += InPin1_OnAddLinkInfo;
            inPin1.OnDelLinkInfo += InPin1_OnDelLinkInfo;
            var inPin2 = AddLinkPinInfo("CtrlValue2", mCtrlValue2, null);
            inPin2.OnAddLinkInfo += InPin2_OnAddLinkInfo;
            inPin2.OnDelLinkInfo += InPin2_OnDelLinkInfo;
            AddLinkPinInfo("CtrlResultLink", mCtrlResultLink, null);
        }
        void InPin1_OnAddLinkInfo(CodeGenerateSystem.Base.LinkInfo linkInfo)
        {
            //var pin1 = GetLinkPinInfo(mCtrlValue1);
            //pin1.LinkType = linkInfo.m_linkFromObjectInfo.GetLinkType(0, true);
            //var pin2 = GetLinkPinInfo(mCtrlValue2);
            //if(pin2.HasLink)
            //{
            //    var pinOut = GetLinkPinInfo(mCtrlResultLink);
            //    pinOut.LinkType = GetAvilableType()
            //}
        }
        void InPin1_OnDelLinkInfo(CodeGenerateSystem.Base.LinkInfo linkInfo)
        {

        }
        void InPin2_OnAddLinkInfo(CodeGenerateSystem.Base.LinkInfo linkInfo)
        {

        }
        void InPin2_OnDelLinkInfo(CodeGenerateSystem.Base.LinkInfo linkInfo)
        {

        }
        public static void InitNodePinTypes(CodeGenerateSystem.Base.ConstructionParams smParam)
        {
            CodeGenerateSystem.Base.enLinkType value1LinkType = CodeGenerateSystem.Base.enLinkType.Unknow;
            CodeGenerateSystem.Base.enLinkType value2LinkType = CodeGenerateSystem.Base.enLinkType.Unknow;
            CodeGenerateSystem.Base.enLinkType resultLinkType = CodeGenerateSystem.Base.enLinkType.Unknow;

            value1LinkType = GetLinkType(smParam);
            value2LinkType = GetLinkType(smParam);
            if (smParam.ConstructParam == "&&" || smParam.ConstructParam == "||")
            {
                value1LinkType |= CodeGenerateSystem.Base.enLinkType.Class;
                value2LinkType |= CodeGenerateSystem.Base.enLinkType.Class;
            }
            resultLinkType = GetLinkType(smParam);

            CollectLinkPinInfo(smParam, "CtrlValue1", value1LinkType, CodeGenerateSystem.Base.enBezierType.Left, CodeGenerateSystem.Base.enLinkOpType.End, false);
            CollectLinkPinInfo(smParam, "CtrlValue2", value2LinkType, CodeGenerateSystem.Base.enBezierType.Left, CodeGenerateSystem.Base.enLinkOpType.End, false);
            CollectLinkPinInfo(smParam, "CtrlResultLink", resultLinkType, CodeGenerateSystem.Base.enBezierType.Right, CodeGenerateSystem.Base.enLinkOpType.Start, true);
        }

        // 根据两个运算的类型获得一个合法的类型, 如果两个值不能运算则返回Unknow
        private CodeGenerateSystem.Base.enLinkType GetAvilableType(CodeGenerateSystem.Base.enLinkType type1, CodeGenerateSystem.Base.enLinkType type2)
        {
            switch (CSParam.ConstructParam)
            {
                case "＋":
                case "－":
                case "×":
                case "÷":
                    {
                        if (type1 == CodeGenerateSystem.Base.enLinkType.String ||
                           type2 == CodeGenerateSystem.Base.enLinkType.String)
                            return CodeGenerateSystem.Base.enLinkType.String;

                        if ((type1 & CodeGenerateSystem.Base.enLinkType.VectorValue) == type1 ||
                           (type1 & CodeGenerateSystem.Base.enLinkType.VectorValue) == CodeGenerateSystem.Base.enLinkType.VectorValue)
                        {
                            if ((type2 & CodeGenerateSystem.Base.enLinkType.NumbericalValue) == type2 ||
                                 (type2 & CodeGenerateSystem.Base.enLinkType.NumbericalValue) == CodeGenerateSystem.Base.enLinkType.NumbericalValue)
                                return type1;
                            if (type1 == type2)
                                return type1;
                            if ((type2 & CodeGenerateSystem.Base.enLinkType.VectorValue) == type2 ||
                                (type2 & CodeGenerateSystem.Base.enLinkType.VectorValue) == CodeGenerateSystem.Base.enLinkType.VectorValue)
                                return type1;
                            return CodeGenerateSystem.Base.enLinkType.Unknow;
                        }
                        if ((type2 & CodeGenerateSystem.Base.enLinkType.VectorValue) == type2 ||
                            (type2 & CodeGenerateSystem.Base.enLinkType.VectorValue) == CodeGenerateSystem.Base.enLinkType.VectorValue)
                        {
                            if ((type1 & CodeGenerateSystem.Base.enLinkType.NumbericalValue) == type1 ||
                                (type1 & CodeGenerateSystem.Base.enLinkType.NumbericalValue) == CodeGenerateSystem.Base.enLinkType.NumbericalValue)
                                return type2;
                            if (type1 == type2)
                                return type1;
                            if ((type1 & CodeGenerateSystem.Base.enLinkType.VectorValue) == type1 ||
                                 (type1 & CodeGenerateSystem.Base.enLinkType.VectorValue) == CodeGenerateSystem.Base.enLinkType.VectorValue)
                                return type2;
                            return CodeGenerateSystem.Base.enLinkType.Unknow;
                        }
                        if ((type1 & CodeGenerateSystem.Base.enLinkType.NumbericalValue) == type1 &&
                           (type2 & CodeGenerateSystem.Base.enLinkType.NumbericalValue) == type2)
                        {
                            if (type1 == CodeGenerateSystem.Base.enLinkType.NumbericalValue &&
                               type2 == CodeGenerateSystem.Base.enLinkType.NumbericalValue)
                                return CodeGenerateSystem.Base.enLinkType.NumbericalValue;
                            else if (type1 == CodeGenerateSystem.Base.enLinkType.Double ||
                               type2 == CodeGenerateSystem.Base.enLinkType.Double)
                                return CodeGenerateSystem.Base.enLinkType.Double;
                            else if (type1 == CodeGenerateSystem.Base.enLinkType.Single ||
                                    type2 == CodeGenerateSystem.Base.enLinkType.Single)
                                return CodeGenerateSystem.Base.enLinkType.Single;
                            else
                            {
                                switch (type1)
                                {
                                    case CodeGenerateSystem.Base.enLinkType.UInt64:
                                        {
                                            switch (type2)
                                            {
                                                case CodeGenerateSystem.Base.enLinkType.UInt64:
                                                case CodeGenerateSystem.Base.enLinkType.UInt32:
                                                case CodeGenerateSystem.Base.enLinkType.UInt16:
                                                case CodeGenerateSystem.Base.enLinkType.Byte:
                                                    return CodeGenerateSystem.Base.enLinkType.UInt64;
                                                default:
                                                    return CodeGenerateSystem.Base.enLinkType.Unknow;
                                            }
                                        }
                                    case CodeGenerateSystem.Base.enLinkType.UInt32:
                                        {
                                            switch (type2)
                                            {
                                                case CodeGenerateSystem.Base.enLinkType.UInt64:
                                                    return CodeGenerateSystem.Base.enLinkType.UInt64;
                                                case CodeGenerateSystem.Base.enLinkType.UInt32:
                                                case CodeGenerateSystem.Base.enLinkType.UInt16:
                                                case CodeGenerateSystem.Base.enLinkType.Byte:
                                                    return CodeGenerateSystem.Base.enLinkType.UInt32;
                                                case CodeGenerateSystem.Base.enLinkType.Int64:
                                                case CodeGenerateSystem.Base.enLinkType.Int32:
                                                case CodeGenerateSystem.Base.enLinkType.Int16:
                                                case CodeGenerateSystem.Base.enLinkType.SByte:
                                                    return CodeGenerateSystem.Base.enLinkType.Int64;
                                            }
                                        }
                                        break;
                                    case CodeGenerateSystem.Base.enLinkType.UInt16:
                                        {
                                            switch (type2)
                                            {
                                                case CodeGenerateSystem.Base.enLinkType.UInt64:
                                                    return CodeGenerateSystem.Base.enLinkType.UInt64;
                                                case CodeGenerateSystem.Base.enLinkType.UInt32:
                                                    return CodeGenerateSystem.Base.enLinkType.UInt32;
                                                case CodeGenerateSystem.Base.enLinkType.UInt16:
                                                case CodeGenerateSystem.Base.enLinkType.Byte:
                                                    return CodeGenerateSystem.Base.enLinkType.Int32;
                                                case CodeGenerateSystem.Base.enLinkType.Int64:
                                                    return CodeGenerateSystem.Base.enLinkType.Int64;
                                                case CodeGenerateSystem.Base.enLinkType.Int32:
                                                case CodeGenerateSystem.Base.enLinkType.Int16:
                                                case CodeGenerateSystem.Base.enLinkType.SByte:
                                                    return CodeGenerateSystem.Base.enLinkType.Int32;
                                            }
                                        }
                                        break;
                                    case CodeGenerateSystem.Base.enLinkType.Byte:
                                        {
                                            switch (type2)
                                            {
                                                case CodeGenerateSystem.Base.enLinkType.UInt64:
                                                    return CodeGenerateSystem.Base.enLinkType.UInt64;
                                                case CodeGenerateSystem.Base.enLinkType.UInt32:
                                                    return CodeGenerateSystem.Base.enLinkType.UInt32;
                                                case CodeGenerateSystem.Base.enLinkType.UInt16:
                                                case CodeGenerateSystem.Base.enLinkType.Byte:
                                                    return CodeGenerateSystem.Base.enLinkType.Int32;
                                                case CodeGenerateSystem.Base.enLinkType.Int64:
                                                    return CodeGenerateSystem.Base.enLinkType.Int64;
                                                case CodeGenerateSystem.Base.enLinkType.Int32:
                                                case CodeGenerateSystem.Base.enLinkType.Int16:
                                                case CodeGenerateSystem.Base.enLinkType.SByte:
                                                    return CodeGenerateSystem.Base.enLinkType.Int32;
                                            }
                                        }
                                        break;
                                    case CodeGenerateSystem.Base.enLinkType.Int64:
                                        {
                                            switch (type2)
                                            {
                                                case CodeGenerateSystem.Base.enLinkType.UInt64:
                                                    return CodeGenerateSystem.Base.enLinkType.Unknow;
                                                case CodeGenerateSystem.Base.enLinkType.UInt32:
                                                case CodeGenerateSystem.Base.enLinkType.UInt16:
                                                case CodeGenerateSystem.Base.enLinkType.Byte:
                                                case CodeGenerateSystem.Base.enLinkType.Int64:
                                                case CodeGenerateSystem.Base.enLinkType.Int32:
                                                case CodeGenerateSystem.Base.enLinkType.Int16:
                                                case CodeGenerateSystem.Base.enLinkType.SByte:
                                                    return CodeGenerateSystem.Base.enLinkType.Int64;
                                            }
                                        }
                                        break;
                                    case CodeGenerateSystem.Base.enLinkType.Int32:
                                        {
                                            switch (type2)
                                            {
                                                case CodeGenerateSystem.Base.enLinkType.UInt64:
                                                    return CodeGenerateSystem.Base.enLinkType.Unknow;
                                                case CodeGenerateSystem.Base.enLinkType.UInt32:
                                                case CodeGenerateSystem.Base.enLinkType.Int64:
                                                    return CodeGenerateSystem.Base.enLinkType.Int64;
                                                case CodeGenerateSystem.Base.enLinkType.UInt16:
                                                case CodeGenerateSystem.Base.enLinkType.Byte:
                                                case CodeGenerateSystem.Base.enLinkType.Int32:
                                                case CodeGenerateSystem.Base.enLinkType.Int16:
                                                case CodeGenerateSystem.Base.enLinkType.SByte:
                                                    return CodeGenerateSystem.Base.enLinkType.Int32;
                                            }
                                        }
                                        break;
                                    case CodeGenerateSystem.Base.enLinkType.Int16:
                                        {
                                            switch (type2)
                                            {
                                                case CodeGenerateSystem.Base.enLinkType.UInt64:
                                                    return CodeGenerateSystem.Base.enLinkType.Unknow;
                                                case CodeGenerateSystem.Base.enLinkType.UInt32:
                                                case CodeGenerateSystem.Base.enLinkType.Int64:
                                                    return CodeGenerateSystem.Base.enLinkType.Int64;
                                                case CodeGenerateSystem.Base.enLinkType.UInt16:
                                                case CodeGenerateSystem.Base.enLinkType.Byte:
                                                case CodeGenerateSystem.Base.enLinkType.Int32:
                                                case CodeGenerateSystem.Base.enLinkType.Int16:
                                                case CodeGenerateSystem.Base.enLinkType.SByte:
                                                    return CodeGenerateSystem.Base.enLinkType.Int32;
                                            }
                                        }
                                        break;
                                    case CodeGenerateSystem.Base.enLinkType.SByte:
                                        {
                                            switch (type2)
                                            {
                                                case CodeGenerateSystem.Base.enLinkType.UInt64:
                                                    return CodeGenerateSystem.Base.enLinkType.Unknow;
                                                case CodeGenerateSystem.Base.enLinkType.UInt32:
                                                case CodeGenerateSystem.Base.enLinkType.Int64:
                                                    return CodeGenerateSystem.Base.enLinkType.Int64;
                                                case CodeGenerateSystem.Base.enLinkType.UInt16:
                                                case CodeGenerateSystem.Base.enLinkType.Byte:
                                                case CodeGenerateSystem.Base.enLinkType.Int32:
                                                case CodeGenerateSystem.Base.enLinkType.Int16:
                                                case CodeGenerateSystem.Base.enLinkType.SByte:
                                                    return CodeGenerateSystem.Base.enLinkType.Int32;
                                            }
                                        }
                                        break;
                                    default:
                                        return type1;
                                }
                            }
                        }
                    }
                    break;
                case "&&":
                case "||":
                    //if (type1 == CodeGenerateSystem.Base.enLinkType.Bool && type2 == CodeGenerateSystem.Base.enLinkType.Bool)
                    return CodeGenerateSystem.Base.enLinkType.Bool;
                //break;
                case "&":
                case "|":
                    if (((type1 & CodeGenerateSystem.Base.enLinkType.UnsignedNumbericalValue) == type1) &&
                       ((type2 & CodeGenerateSystem.Base.enLinkType.UnsignedNumbericalValue) == type2))
                    {
                        switch (type1)
                        {
                            case CodeGenerateSystem.Base.enLinkType.Byte:
                                {
                                    switch (type2)
                                    {
                                        case CodeGenerateSystem.Base.enLinkType.Byte:
                                        case CodeGenerateSystem.Base.enLinkType.UInt16:
                                            return CodeGenerateSystem.Base.enLinkType.Int32;
                                        case CodeGenerateSystem.Base.enLinkType.UInt32:
                                            return CodeGenerateSystem.Base.enLinkType.UInt32;
                                        case CodeGenerateSystem.Base.enLinkType.UInt64:
                                            return CodeGenerateSystem.Base.enLinkType.UInt64;
                                    }
                                }
                                break;
                            case CodeGenerateSystem.Base.enLinkType.UInt16:
                                {
                                    switch (type2)
                                    {
                                        case CodeGenerateSystem.Base.enLinkType.Byte:
                                        case CodeGenerateSystem.Base.enLinkType.UInt16:
                                            return CodeGenerateSystem.Base.enLinkType.Int32;
                                        case CodeGenerateSystem.Base.enLinkType.UInt32:
                                            return CodeGenerateSystem.Base.enLinkType.UInt32;
                                        case CodeGenerateSystem.Base.enLinkType.UInt64:
                                            return CodeGenerateSystem.Base.enLinkType.UInt64;
                                    }
                                }
                                break;
                            case CodeGenerateSystem.Base.enLinkType.UInt32:
                                {
                                    switch (type2)
                                    {
                                        case CodeGenerateSystem.Base.enLinkType.Byte:
                                        case CodeGenerateSystem.Base.enLinkType.UInt16:
                                        case CodeGenerateSystem.Base.enLinkType.UInt32:
                                            return CodeGenerateSystem.Base.enLinkType.UInt32;
                                        case CodeGenerateSystem.Base.enLinkType.UInt64:
                                            return CodeGenerateSystem.Base.enLinkType.UInt64;
                                    }
                                }
                                break;
                            case CodeGenerateSystem.Base.enLinkType.UInt64:
                                {
                                    switch (type2)
                                    {
                                        case CodeGenerateSystem.Base.enLinkType.Byte:
                                        case CodeGenerateSystem.Base.enLinkType.UInt16:
                                        case CodeGenerateSystem.Base.enLinkType.UInt32:
                                        case CodeGenerateSystem.Base.enLinkType.UInt64:
                                            return CodeGenerateSystem.Base.enLinkType.UInt64;
                                    }
                                }
                                break;
                        }
                    }
                    break;
            }

            return CodeGenerateSystem.Base.enLinkType.Unknow;
        }
        public override string GCode_GetValueName(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            var strValueName = "arithmeticResult_" + EngineNS.Editor.Assist.GetValuedGUIDString(Id);

            return strValueName;
        }
        public override string GCode_GetTypeString(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            if (element == mCtrlResultLink || element == null)
            {
                if (mCtrlValue1.HasLink && mCtrlValue2.HasLink)
                {
                    var leftType = mCtrlValue1.GetLinkType(0, true);
                    if ((leftType & CodeGenerateSystem.Base.enLinkType.VectorValue) == CodeGenerateSystem.Base.enLinkType.VectorValue ||
                        (leftType & CodeGenerateSystem.Base.enLinkType.NumbericalValue) == CodeGenerateSystem.Base.enLinkType.NumbericalValue ||
                        (leftType & CodeGenerateSystem.Base.enLinkType.Value) == CodeGenerateSystem.Base.enLinkType.Value ||
                        (leftType & CodeGenerateSystem.Base.enLinkType.UnsignedNumbericalValue) == CodeGenerateSystem.Base.enLinkType.UnsignedNumbericalValue ||
                        (leftType & CodeGenerateSystem.Base.enLinkType.SignedNumbericalValue) == CodeGenerateSystem.Base.enLinkType.SignedNumbericalValue)
                    {
                        leftType = CodeGenerateSystem.Base.LinkPinControl.GetLinkTypeFromTypeString(mCtrlValue1.GetLinkedObject(0, true).GCode_GetTypeString(mCtrlValue1.GetLinkedPinControl(0, true), context));
                    }

                    var rightType = mCtrlValue2.GetLinkType(0, true);
                    if ((rightType & CodeGenerateSystem.Base.enLinkType.VectorValue) == CodeGenerateSystem.Base.enLinkType.VectorValue ||
                        (rightType & CodeGenerateSystem.Base.enLinkType.NumbericalValue) == CodeGenerateSystem.Base.enLinkType.NumbericalValue ||
                        (rightType & CodeGenerateSystem.Base.enLinkType.Value) == CodeGenerateSystem.Base.enLinkType.Value ||
                        (rightType & CodeGenerateSystem.Base.enLinkType.UnsignedNumbericalValue) == CodeGenerateSystem.Base.enLinkType.UnsignedNumbericalValue ||
                        (rightType & CodeGenerateSystem.Base.enLinkType.SignedNumbericalValue) == CodeGenerateSystem.Base.enLinkType.SignedNumbericalValue)
                    {
                        rightType = CodeGenerateSystem.Base.LinkPinControl.GetLinkTypeFromTypeString(mCtrlValue2.GetLinkedObject(0, true).GCode_GetTypeString(mCtrlValue2.GetLinkedPinControl(0, true), context));
                    }

                    var retType = GetAvilableType(leftType, rightType);
                    switch (retType)
                    {
                        case CodeGenerateSystem.Base.enLinkType.Bool:
                            return "System.Boolean";
                        case CodeGenerateSystem.Base.enLinkType.Int32:
                            return "System.Int32";
                        case CodeGenerateSystem.Base.enLinkType.Int64:
                            return "System.Int64";
                        case CodeGenerateSystem.Base.enLinkType.Single:
                            return "System.Single";
                        case CodeGenerateSystem.Base.enLinkType.Double:
                        case CodeGenerateSystem.Base.enLinkType.NumbericalValue:
                            return "System.Double";
                        case CodeGenerateSystem.Base.enLinkType.String:
                            return "System.String";
                        case CodeGenerateSystem.Base.enLinkType.Vector2:
                            return "EngineNS.Vector2";
                        case CodeGenerateSystem.Base.enLinkType.Vector3:
                            return "EngineNS.Vector3";
                        case CodeGenerateSystem.Base.enLinkType.Vector4:
                            return "EngineNS.Vector4";
                        case CodeGenerateSystem.Base.enLinkType.Byte:
                            return "System.Byte";
                        case CodeGenerateSystem.Base.enLinkType.SByte:
                            return "System.SByte";
                        case CodeGenerateSystem.Base.enLinkType.Int16:
                            return "System.Int16";
                        case CodeGenerateSystem.Base.enLinkType.UInt16:
                            return "System.UInt16";
                        case CodeGenerateSystem.Base.enLinkType.UInt32:
                            return "System.UInt32";
                        case CodeGenerateSystem.Base.enLinkType.UInt64:
                            return "System.UInt64";
                    }
                }
            }

            return base.GCode_GetTypeString(element, context);
        }

        public override Type GCode_GetType(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            if (element == mCtrlResultLink || element == null)
            {
                if (mCtrlValue1.HasLink && mCtrlValue2.HasLink)
                {
                    var leftType = mCtrlValue1.GetLinkType(0, true);
                    if ((leftType & CodeGenerateSystem.Base.enLinkType.VectorValue) == CodeGenerateSystem.Base.enLinkType.VectorValue ||
                        (leftType & CodeGenerateSystem.Base.enLinkType.NumbericalValue) == CodeGenerateSystem.Base.enLinkType.NumbericalValue ||
                        (leftType & CodeGenerateSystem.Base.enLinkType.Value) == CodeGenerateSystem.Base.enLinkType.Value ||
                        (leftType & CodeGenerateSystem.Base.enLinkType.UnsignedNumbericalValue) == CodeGenerateSystem.Base.enLinkType.UnsignedNumbericalValue ||
                        (leftType & CodeGenerateSystem.Base.enLinkType.SignedNumbericalValue) == CodeGenerateSystem.Base.enLinkType.SignedNumbericalValue)
                    {
                        leftType = CodeGenerateSystem.Base.LinkPinControl.GetLinkTypeFromTypeString(mCtrlValue1.GetLinkedObject(0, true).GCode_GetTypeString(mCtrlValue1.GetLinkedPinControl(0, true), context));
                    }

                    var rightType = mCtrlValue2.GetLinkType(0, true);
                    if ((rightType & CodeGenerateSystem.Base.enLinkType.VectorValue) == CodeGenerateSystem.Base.enLinkType.VectorValue ||
                        (rightType & CodeGenerateSystem.Base.enLinkType.NumbericalValue) == CodeGenerateSystem.Base.enLinkType.NumbericalValue ||
                        (rightType & CodeGenerateSystem.Base.enLinkType.Value) == CodeGenerateSystem.Base.enLinkType.Value ||
                        (rightType & CodeGenerateSystem.Base.enLinkType.UnsignedNumbericalValue) == CodeGenerateSystem.Base.enLinkType.UnsignedNumbericalValue ||
                        (rightType & CodeGenerateSystem.Base.enLinkType.SignedNumbericalValue) == CodeGenerateSystem.Base.enLinkType.SignedNumbericalValue)
                    {
                        rightType = CodeGenerateSystem.Base.LinkPinControl.GetLinkTypeFromTypeString(mCtrlValue2.GetLinkedObject(0, true).GCode_GetTypeString(mCtrlValue2.GetLinkedPinControl(0, true), context));
                    }

                    var retType = GetAvilableType(leftType, rightType);
                    switch (retType)
                    {
                        case CodeGenerateSystem.Base.enLinkType.Bool:
                            return typeof(System.Boolean);
                        case CodeGenerateSystem.Base.enLinkType.Int32:
                            return typeof(System.Int32);
                        case CodeGenerateSystem.Base.enLinkType.Int64:
                            return typeof(System.Int64);
                        case CodeGenerateSystem.Base.enLinkType.Single:
                            return typeof(System.Single);
                        case CodeGenerateSystem.Base.enLinkType.Double:
                        case CodeGenerateSystem.Base.enLinkType.NumbericalValue:
                            return typeof(System.Double);
                        case CodeGenerateSystem.Base.enLinkType.String:
                            return typeof(System.String);
                        case CodeGenerateSystem.Base.enLinkType.Vector2:
                            return typeof(EngineNS.Vector2);
                        case CodeGenerateSystem.Base.enLinkType.Vector3:
                            return typeof(EngineNS.Vector3);
                        case CodeGenerateSystem.Base.enLinkType.Vector4:
                            return typeof(EngineNS.Vector4);
                        case CodeGenerateSystem.Base.enLinkType.Byte:
                            return typeof(System.Byte);
                        case CodeGenerateSystem.Base.enLinkType.SByte:
                            return typeof(System.SByte);
                        case CodeGenerateSystem.Base.enLinkType.Int16:
                            return typeof(System.Int16);
                        case CodeGenerateSystem.Base.enLinkType.UInt16:
                            return typeof(System.UInt16);
                        case CodeGenerateSystem.Base.enLinkType.UInt32:
                            return typeof(System.UInt32);
                        case CodeGenerateSystem.Base.enLinkType.UInt64:
                            return typeof(System.UInt64);
                    }
                }
            }

            return base.GCode_GetType(element, context);
        }

        System.CodeDom.CodeVariableDeclarationStatement mVariableDeclaration;
        System.CodeDom.CodeAssignStatement mAssignStatement;
        System.CodeDom.CodeVariableDeclarationStatement mVariableDeclarationStatement_Value1;
        System.CodeDom.CodeVariableDeclarationStatement mVariableDeclarationStatement_Value2;
        public override async System.Threading.Tasks.Task GCode_CodeDom_GenerateCode(System.CodeDom.CodeTypeDeclaration codeClass, System.CodeDom.CodeStatementCollection codeStatementCollection, CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            if (!mCtrlValue1.HasLink || !mCtrlValue2.HasLink)
                return;

            // 计算结果
            if (!context.Method.Statements.Contains(mVariableDeclaration))
            {
                var valueType = GCode_GetTypeString(mCtrlResultLink, context);
                var type = EngineNS.Rtti.RttiHelper.GetTypeFromTypeFullName(valueType);
                mVariableDeclaration = new System.CodeDom.CodeVariableDeclarationStatement(
                                                            valueType,
                                                            GCode_GetValueName(mCtrlResultLink, context),
                                                            CodeGenerateSystem.Program.GetDefaultValueExpressionFromType(type));
                context.Method.Statements.Insert(0, mVariableDeclaration);
            }

            // 参数1
            var linkObj1 = mCtrlValue1.GetLinkedObject(0, true);
            var linkElm1 = mCtrlValue1.GetLinkedPinControl(0, true);
            if (!linkObj1.IsOnlyReturnValue)
                await linkObj1.GCode_CodeDom_GenerateCode(codeClass, codeStatementCollection, linkElm1, context);

            System.CodeDom.CodeExpression valueExp1 = null;
            var valueType1Str = linkObj1.GCode_GetTypeString(linkElm1, context);
            var valueType1 = linkObj1.GCode_GetType(linkElm1, context);
            if (CSParam.ConstructParam == "&&" || CSParam.ConstructParam == "||")
            {
                valueType1Str = "System.Boolean";
                valueType1 = typeof(bool);
            }
            if (linkObj1.Pin_UseOrigionParamName(linkElm1))
            {
                if (linkElm1.GetLinkType(0, true) == CodeGenerateSystem.Base.enLinkType.Bool)
                {
                    valueExp1 = linkObj1.GCode_CodeDom_GetValue(linkElm1, context);
                }
                else if (linkElm1.GetLinkType(0, true) == CodeGenerateSystem.Base.enLinkType.Class)
                {
                    var condition = new System.CodeDom.CodeBinaryOperatorExpression(linkObj1.GCode_CodeDom_GetValue(linkElm1, context),
                        System.CodeDom.CodeBinaryOperatorType.IdentityInequality,
                        new System.CodeDom.CodePrimitiveExpression(null));
                    valueExp1 = condition;
                }
                else
                {
                    valueExp1 = linkObj1.GCode_CodeDom_GetValue(linkElm1, context);
                }
            }
            else
            {
                var tempValueName1 = "arithmetic_" + EngineNS.Editor.Assist.GetValuedGUIDString(this.Id) + "_Value1";
                if (!context.Method.Statements.Contains(mVariableDeclarationStatement_Value1))
                {
                    mVariableDeclarationStatement_Value1 = new System.CodeDom.CodeVariableDeclarationStatement(
                                                                    valueType1Str,
                                                                    tempValueName1,
                                                                    CodeGenerateSystem.Program.GetDefaultValueExpressionFromType(EngineNS.Rtti.RttiHelper.GetTypeFromTypeFullName(valueType1Str)));
                    context.Method.Statements.Insert(0, mVariableDeclarationStatement_Value1);
                }


                if (linkElm1.GetLinkType(0, true) == CodeGenerateSystem.Base.enLinkType.Bool)
                {
                    codeStatementCollection.Add(new System.CodeDom.CodeAssignStatement(
                                                     new System.CodeDom.CodeTypeReferenceExpression(tempValueName1),
                                                     linkObj1.GCode_CodeDom_GetValue(linkElm1, context)));
                }
                else if (linkElm1.GetLinkType(0, true) == CodeGenerateSystem.Base.enLinkType.Class)
                {
                    var condition = new System.CodeDom.CodeBinaryOperatorExpression(linkObj1.GCode_CodeDom_GetValue(linkElm1, context),
                        System.CodeDom.CodeBinaryOperatorType.IdentityInequality,
                        new System.CodeDom.CodePrimitiveExpression(null));
                    codeStatementCollection.Add(new System.CodeDom.CodeAssignStatement(
                                                    new System.CodeDom.CodeTypeReferenceExpression(tempValueName1),
                                                    condition));
                }
                else
                {
                    codeStatementCollection.Add(new System.CodeDom.CodeAssignStatement(
                                                     new System.CodeDom.CodeTypeReferenceExpression(tempValueName1),
                                                     linkObj1.GCode_CodeDom_GetValue(linkElm1, context)));
                }
                valueExp1 = new System.CodeDom.CodeVariableReferenceExpression(tempValueName1);
            }
            //var valueExp1 = linkObj1.GCode_CodeDom_GetValue(linkElm1, context);

            // 参数2
            var linkObj2 = mCtrlValue2.GetLinkedObject(0, true);
            var linkElm2 = mCtrlValue2.GetLinkedPinControl(0, true);
            if (!linkObj2.IsOnlyReturnValue)
                await linkObj2.GCode_CodeDom_GenerateCode(codeClass, codeStatementCollection, linkElm2, context);

            System.CodeDom.CodeExpression valueExp2 = null;
            var valueType2Str = linkObj2.GCode_GetTypeString(linkElm2, context);
            var valueType2 = linkObj2.GCode_GetType(linkElm2, context);
            if (CSParam.ConstructParam == "&&" || CSParam.ConstructParam == "||")
            {
                valueType2Str = "System.Boolean";
                valueType2 = typeof(bool);
            }
            if (linkObj2.Pin_UseOrigionParamName(linkElm2))
            {
                if (linkElm2.GetLinkType(0, true) == CodeGenerateSystem.Base.enLinkType.Bool)
                {
                    valueExp2 = linkObj2.GCode_CodeDom_GetValue(linkElm2, context);
                }
                else if (linkElm2.GetLinkType(0, true) == CodeGenerateSystem.Base.enLinkType.Class)
                {
                    var condition = new System.CodeDom.CodeBinaryOperatorExpression(linkObj2.GCode_CodeDom_GetValue(linkElm2, context),
                        System.CodeDom.CodeBinaryOperatorType.IdentityInequality,
                        new System.CodeDom.CodePrimitiveExpression(null));
                    valueExp2 = condition;
                }
                else
                {
                    valueExp2 = linkObj2.GCode_CodeDom_GetValue(linkElm2, context);
                }
            }
            else
            {
                var tempValueName2 = "arithmetic_" + EngineNS.Editor.Assist.GetValuedGUIDString(this.Id) + "_Value2";
                if (!context.Method.Statements.Contains(mVariableDeclarationStatement_Value2))
                {
                    mVariableDeclarationStatement_Value2 = new System.CodeDom.CodeVariableDeclarationStatement(
                                                                    valueType2Str,
                                                                    tempValueName2,
                                                                    CodeGenerateSystem.Program.GetDefaultValueExpressionFromType(EngineNS.Rtti.RttiHelper.GetTypeFromTypeFullName(valueType2Str)));
                    context.Method.Statements.Insert(0, mVariableDeclarationStatement_Value2);
                }


                if (linkElm2.GetLinkType(0, true) == CodeGenerateSystem.Base.enLinkType.Bool)
                {
                    codeStatementCollection.Add(new System.CodeDom.CodeAssignStatement(
                                                new System.CodeDom.CodeTypeReferenceExpression(tempValueName2),
                                                linkObj2.GCode_CodeDom_GetValue(linkElm2, context)));
                }
                else if (linkElm2.GetLinkType(0, true) == CodeGenerateSystem.Base.enLinkType.Class)
                {
                    var condition = new System.CodeDom.CodeBinaryOperatorExpression(linkObj2.GCode_CodeDom_GetValue(linkElm2, context),
                        System.CodeDom.CodeBinaryOperatorType.IdentityInequality,
                        new System.CodeDom.CodePrimitiveExpression(null));
                    codeStatementCollection.Add(new System.CodeDom.CodeAssignStatement(
                                                new System.CodeDom.CodeTypeReferenceExpression(tempValueName2),
                                                condition));
                }
                else
                {
                    codeStatementCollection.Add(new System.CodeDom.CodeAssignStatement(
                                               new System.CodeDom.CodeTypeReferenceExpression(tempValueName2),
                                               linkObj2.GCode_CodeDom_GetValue(linkElm2, context)));
                }
                valueExp2 = new System.CodeDom.CodeVariableReferenceExpression(tempValueName2);
            }
            //var valueExp2 = linkObj2.GCode_CodeDom_GetValue(linkElm2, context);

            // 运算
            var arithmeticExp = new System.CodeDom.CodeBinaryOperatorExpression();
            arithmeticExp.Left = valueExp1;
            arithmeticExp.Right = valueExp2;
            switch (CSParam.ConstructParam)
            {
                case "＋":
                    arithmeticExp.Operator = System.CodeDom.CodeBinaryOperatorType.Add;
                    break;
                case "－":
                    arithmeticExp.Operator = System.CodeDom.CodeBinaryOperatorType.Subtract;
                    break;
                case "×":
                    arithmeticExp.Operator = System.CodeDom.CodeBinaryOperatorType.Multiply;
                    break;
                case "÷":
                    arithmeticExp.Operator = System.CodeDom.CodeBinaryOperatorType.Divide;
                    break;
                case "&&":
                    arithmeticExp.Operator = System.CodeDom.CodeBinaryOperatorType.BooleanAnd;
                    break;
                case "||":
                    arithmeticExp.Operator = System.CodeDom.CodeBinaryOperatorType.BooleanOr;
                    break;
                case "&":
                    arithmeticExp.Operator = System.CodeDom.CodeBinaryOperatorType.BitwiseAnd;
                    break;
                case "|":
                    arithmeticExp.Operator = System.CodeDom.CodeBinaryOperatorType.BitwiseOr;
                    break;
            }

            // 收集用于调试的数据的代码
            var debugCodes = CodeDomNode.BreakPoint.BeginMacrossDebugCodeStatments(codeStatementCollection);
            if (linkObj1.Pin_UseOrigionParamName(linkElm1))
                CodeDomNode.BreakPoint.GetGatherDataValueCodeStatement(debugCodes, this.mCtrlValue1.GetLinkPinKeyName(), valueExp1, valueType1Str, context);
            else
                CodeDomNode.BreakPoint.GetGatherDataValueCodeStatement(debugCodes, this.mCtrlValue1.GetLinkPinKeyName(), valueExp1, valueType1Str, context);
            if (linkObj2.Pin_UseOrigionParamName(linkElm2))
                CodeDomNode.BreakPoint.GetGatherDataValueCodeStatement(debugCodes, this.mCtrlValue2.GetLinkPinKeyName(), valueExp2, valueType2Str, context);
            else
                CodeDomNode.BreakPoint.GetGatherDataValueCodeStatement(debugCodes, this.mCtrlValue2.GetLinkPinKeyName(), valueExp2, valueType2Str, context);
            // 调试用代码
            var breakCondStatement = CodeDomNode.BreakPoint.BreakCodeStatement(codeClass, debugCodes, HostNodesContainer.GUID, Id);
            // 设置数据代码
            if (!linkObj1.Pin_UseOrigionParamName(this.mCtrlValue1))
                CodeDomNode.BreakPoint.GetSetDataValueCodeStatement(breakCondStatement.TrueStatements, this.mCtrlValue1.GetLinkPinKeyName(), valueExp1, valueType1);
            if (!linkObj2.Pin_UseOrigionParamName(this.mCtrlValue2))
                CodeDomNode.BreakPoint.GetSetDataValueCodeStatement(breakCondStatement.TrueStatements, this.mCtrlValue2.GetLinkPinKeyName(), valueExp2, valueType2);
            CodeDomNode.BreakPoint.EndMacrossDebugCodeStatements(codeStatementCollection, debugCodes);

            if (element == mCtrlResultLink)
            {
                // 创建结果并赋值
                if (mAssignStatement == null)
                {
                    mAssignStatement = new System.CodeDom.CodeAssignStatement();
                    mAssignStatement.Left = new System.CodeDom.CodeVariableReferenceExpression(GCode_GetValueName(null, context));
                }
                mAssignStatement.Right = arithmeticExp;

                if (codeStatementCollection.Contains(mAssignStatement))
                {
                    //var assign = new System.CodeDom.CodeAssignStatement(GCode_CodeDom_GetValue(null) , arithmeticExp);
                    //codeStatementCollection.Add(assign);
                }
                else
                    codeStatementCollection.Add(mAssignStatement);

                debugCodes = CodeDomNode.BreakPoint.BeginMacrossDebugCodeStatments(codeStatementCollection);
                CodeDomNode.BreakPoint.GetGatherDataValueCodeStatement(debugCodes, mCtrlResultLink.GetLinkPinKeyName(), GCode_CodeDom_GetValue(mCtrlResultLink, context), GCode_GetTypeString(mCtrlResultLink, context), context);
                CodeDomNode.BreakPoint.EndMacrossDebugCodeStatements(codeStatementCollection, debugCodes);
            }
        }

        public override System.CodeDom.CodeExpression GCode_CodeDom_GetValue(CodeGenerateSystem.Base.LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context, CodeGenerateSystem.Base.GenerateCodeContext_PreNode preNodeContext = null)
        {
            return new System.CodeDom.CodeVariableReferenceExpression(GCode_GetValueName(null, context));
        }
    }
}
