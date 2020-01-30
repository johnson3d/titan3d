using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace CodeGenerateSystem.CodeDom
{
    [
        ClassInterface(ClassInterfaceType.AutoDispatch),
        ComVisible(true),
        Serializable,
    ]
    public class CodeCastExpression : System.CodeDom.CodeExpression
    {
        public string TargetTypeStr
        {
            get;
            set;
        }
        public System.CodeDom.CodeExpression Expression
        {
            get;
            set;
        }
        bool mIsValueType = false;
        public bool IsValueType => mIsValueType;
        Type mTargetType;
        public CodeCastExpression(Type targetType, System.CodeDom.CodeExpression expression)
        {
            if(targetType == null)
            {
                throw new InvalidOperationException("targetType is null");
            }
            mTargetType = targetType;
            mIsValueType = mTargetType.IsValueType;
            TargetTypeStr = EngineNS.Rtti.RttiHelper.GetAppTypeString(targetType);
            Expression = expression;
        }
        public CodeCastExpression(string targetTypeStr, bool isValueType, System.CodeDom.CodeExpression expression)
        {
            if(string.IsNullOrEmpty(targetTypeStr))
            {
                throw new InvalidOperationException("targetTypeStr is null");
            }
            TargetTypeStr = targetTypeStr;
            Expression = expression;
            mIsValueType = isValueType;
        }
    }
}
