using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CodeGenerateSystem.Base
{
    public delegate void Delegate_StartLink(LinkPinControl objInfo);
    public delegate void Delegate_EndLink(LinkPinControl objInfo);

    [System.Flags]
    [EngineNS.IO.Serializer.EnumSizeAttribute(typeof(EngineNS.IO.Serializer.UInt64Enum))]
    public enum enLinkType : ulong
    {
        Unknow = 0,
        Statement = 1 << 0,
        StatementMirror = 1 << 1,

        IF_Condition = 1 << 2,
        IF_Result = 1 << 3,

        Method = 1 << 4,
        Delegate = 1 << 5 | enLinkType.Method,
        //MethodResult = 1 << 5,
        //MethodListOut,

        Bool = 1 << 6,
        Int32 = 1 << 7,
        Int64 = 1 << 8,
        Single = 1 << 9,
        Double = 1 << 10,
        String = 1 << 11,
        Vector2 = 1 << 12,
        Vector3 = 1 << 13,
        Vector4 = 1 << 14,
        Guid = 1 << 15,
        Byte = 1 << 16,
        SByte = 1 << 17,
        Int16 = 1 << 18,
        UInt16 = 1 << 19,
        UInt32 = 1 << 20,
        UInt64 = 1 << 21,
        Color4 = Vector4,

        Value = enLinkType.Bool | enLinkType.Byte | enLinkType.SByte | enLinkType.Int16 | enLinkType.UInt16 | enLinkType.UInt32 | enLinkType.UInt64 | enLinkType.Int32 | enLinkType.Int64 | enLinkType.Single | enLinkType.Double | enLinkType.String | enLinkType.Vector2 | enLinkType.Vector3 | enLinkType.Vector4,
        NumbericalValue = enLinkType.Byte | enLinkType.SByte | enLinkType.Int16 | enLinkType.UInt16 | enLinkType.UInt32 | enLinkType.UInt64 | enLinkType.Int32 | enLinkType.Int64 | enLinkType.Single | enLinkType.Double,
        IntegerTypeValue = enLinkType.Byte | enLinkType.SByte | enLinkType.Int16 | enLinkType.UInt16 | enLinkType.UInt32 | enLinkType.UInt64 | enLinkType.Int32 | enLinkType.Int64,
        FloatTypeValue = enLinkType.Single | enLinkType.Double,
        UnsignedNumbericalValue = enLinkType.Byte | enLinkType.UInt16 | enLinkType.UInt32 | enLinkType.UInt64,
        SignedNumbericalValue = enLinkType.SByte | enLinkType.Int16 | enLinkType.Int32 | enLinkType.Int64,
        VectorValue = enLinkType.Vector2 | enLinkType.Vector3 | enLinkType.Vector4,

        Struct = 1 << 23,
        ClassField = 1 << 24,
        Class = 1 << 25,

        // Material
        Float1 = 1 << 26,
        //Half1 = Float1,
        Float2 = 1 << 27,
        //Half2 = Float2,
        Float3 = 1 << 28,
        //Half3 = Float3,
        Float4 = 1 << 29,
        //Half4 = Float4,
        Texture = 1 << 30,
        AnimationPose = ((ulong)1) << 31,
        Float4x4 = ((ulong)1) << 32,
        LAState = ((ulong)1) << 33,
        //Half4x4 = Float4x4,

        UInt1 = UInt32,
        UInt2 = ((ulong)1) << 33,
        UInt3 = ((ulong)1) << 34,
        UInt4 = ((ulong)1) << 35,

        BehaviorTree = ((ulong)1) << 36,
		Module = ((ulong)1) << 37,

        Enumerable = ((ulong)1) << 40,
        IntPtr = ((ulong)1) << 41,
        GenericParameter = ((ulong)1) << 42,

        //server        
        GateServer = ((ulong)1) << 50,
        HallServer = ((ulong)1) << 51,
        DataServer = ((ulong)1) << 52,
        ComServer = ((ulong)1) << 53,
        LogServer = ((ulong)1) << 54,
        PathFindServer = ((ulong)1) << 55,
        RegServer = ((ulong)1) << 56,
        
        All = ulong.MaxValue,
    }

    public enum enBezierType
    {
        None,
        Left,
        Right,
        Top,
        Bottom,
    }

    public enum enLinkOpType
    {
        Start = 1<<0,
        End = 1<<1,
        Both = Start | End,
    }
    
    public partial class LinkInfo
    {
        public LinkPinControl m_linkFromObjectInfo;
        public LinkPinControl m_linkToObjectInfo;

        // 函数直接的链接
        public bool IsMethodLink = false;

        partial void LinkInfoConstruction(Canvas drawCanvas);
        partial void SetVisible(Visibility visible);
        protected LinkInfo() { }
        public LinkInfo(Canvas drawCanvas, LinkPinControl startObj, LinkPinControl endObj)
        {
            if (startObj == null || endObj == null)
                return;

            m_linkFromObjectInfo = startObj;
            m_linkToObjectInfo = endObj;

            LinkInfoConstruction(drawCanvas);

            if (CreateLink())
                SetVisible(Visibility.Visible);
        }

        static bool TypeCanLinkWith(Type start, Type end)
        {
            if (start.IsGenericType && end.IsGenericType)
            {
                var startTypes = start.GetGenericArguments();
                var endTypes = end.GetGenericArguments();
                if (startTypes.Length != 1 || endTypes.Length != 1)
                    throw new InvalidOperationException();
                return TypeCanLinkWith(startTypes[0], endTypes[0]);
            }
            else if (start.IsArray && end.IsArray)
            {
                var elementTypeStart = start.GetElementType();
                var elementTypeEnd = end.GetElementType();
                return TypeCanLinkWith(elementTypeStart, elementTypeEnd);
            }
            else
            {
                if (start.IsSubclassOf(end))
                    return true;
                else if (end.IsSubclassOf(start))
                    return true;
                else if (start.GetInterface(end.FullName) != null)
                    return true;
                else if (end.GetInterface(start.FullName) != null)
                    return true;
            }
            return false;
        }
        public static bool CanLinkWith(LinkPinControl startObj, LinkPinControl endObj)
        {
            if ((startObj.LinkOpType == endObj.LinkOpType) && (startObj.LinkOpType != enLinkOpType.Both))
                return false;
            if (!endObj.HostNodeControl.CanLink(endObj, startObj))
                return false;
            if (!startObj.HostNodeControl.CanLink(startObj, endObj))
                return false;

            if ((startObj.LinkType & endObj.LinkType) > 0)
            {
                if (string.IsNullOrEmpty(startObj.ClassType) || string.IsNullOrEmpty(endObj.ClassType))
                    return true;
                if (startObj.ClassType == "System.Object" || endObj.ClassType == "System.Object")
                    return true;
                else if (startObj.ClassType == endObj.ClassType)
                    return true;
                else if (startObj.LinkType == endObj.LinkType)
                {
                    if (startObj.LinkType == enLinkType.Class || startObj.LinkType == enLinkType.Struct)
                    {
                        var startType = startObj.HostNodeControl.GCode_GetType(startObj, null);
                        var endType = endObj.HostNodeControl.GCode_GetType(endObj, null);
                        if(startType != null && endType != null)
                        {
                            if(endType.IsInterface)
                            {
                                if (startType.GetInterface(endType.FullName) != null)
                                    return true;
                            }
                            else
                            {
                                if (startType.IsSubclassOf(endType))
                                    return true;
                            }
                            if(startType.IsInterface)
                            {
                                if (endType.GetInterface(startType.FullName) != null)
                                    return true;
                            }
                            else
                            {
                                if (endType.IsSubclassOf(startType))
                                    return true;
                            }
                            return false;
                        }
                    }
                    else
                        return true;
                }
                //else if ((startObj.ClassType == typeof(EngineNS.Color4).FullName && startObj.ClassType == typeof(EngineNS.Vector4).FullName) ||
                //        (startObj.ClassType == typeof(EngineNS.Vector4).FullName && startObj.ClassType == typeof(EngineNS.Color4).FullName))
                //    return true;
                else
                {
                    if (startObj.ClassType.Contains("System.Collections.Generic.List`1[[System.Object") ||
                       endObj.ClassType.Contains("System.Collections.Generic.List`1[[System.Object"))
                        return true;
                    else
                    {
                        var typeStart = EngineNS.Rtti.RttiHelper.GetTypeFromTypeFullName(startObj.ClassType, startObj.HostNodeControl.CSParam.CSType);
                        var typeEnd = EngineNS.Rtti.RttiHelper.GetTypeFromTypeFullName(endObj.ClassType, startObj.HostNodeControl.CSParam.CSType);
                        if (typeStart == null || typeEnd == null)
                            return false;

                        return TypeCanLinkWith(typeStart, typeEnd);
                    }
                }
            }
            else
            {
                if (startObj.LinkType == enLinkType.GenericParameter || endObj.LinkType == enLinkType.GenericParameter)
                    return true;
                if (((startObj.LinkType & enLinkType.NumbericalValue) != 0) &&
                   ((endObj.LinkType & enLinkType.NumbericalValue) != 0))
                    return true;
            }

            return false;
        }

        partial void SetColor(LinkPinControl pinCtrl);
        partial void RemoveLinkPath();
        // 根据信息创建链接
        protected bool CreateLink()
        {
            if (m_linkFromObjectInfo == null || m_linkToObjectInfo == null)
                return false;

            if (m_linkFromObjectInfo.AddLinkInfo(this) == true &&
               m_linkToObjectInfo.AddLinkInfo(this) == true)
            {
                SetColor(m_linkFromObjectInfo);
                m_linkFromObjectInfo.HostNodeControl.AfterLink();
                m_linkToObjectInfo.HostNodeControl.AfterLink();
                return true;
            }

            //Clear();
            RemoveLinkPath();

            return false;
        }
        public virtual BaseNodeControl AddTransition()
        {
            return null;
        }
        public virtual void RemoveTransition(BaseNodeControl transitionNode)
        {

        }
        public void ResetLink()
        {
            if (m_linkFromObjectInfo == null || m_linkToObjectInfo == null)
                return;

            if (m_linkFromObjectInfo.AddLinkInfo(this) == true &&
               m_linkToObjectInfo.AddLinkInfo(this) == true)
            {
                m_drawCanvas.Children.Add(m_LinkPath);
                SetColor(m_linkFromObjectInfo);
            }
        }
        public virtual void Clear()
        {
            RemoveLinkPath();

            m_linkFromObjectInfo.RemoveLink(this);
            m_linkToObjectInfo.RemoveLink(this);

            SetVisible(Visibility.Hidden);
        }

        public bool IsEqual(LinkInfo info)
        {
            if (m_linkFromObjectInfo.IsEqual(info.m_linkFromObjectInfo) &&
                m_linkToObjectInfo.IsEqual(info.m_linkToObjectInfo))
                return true;

            return false;
        }


    }
}
