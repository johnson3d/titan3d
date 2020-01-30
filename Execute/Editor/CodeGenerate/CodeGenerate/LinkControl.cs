using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CodeGenerateSystem.Base
{
    public enum enLinkCurveType
    {
        Line,
        Bezier,
        BrokenLine,
    }
    public partial class LinkPinControl
    {
        // 链接点所在的节点
        public Base.BaseNodeControl HostNodeControl
        {
            get;
            set;
        }

        // 链接点所在的节点容器
        public Base.NodesContainer HostNodesContainer
        {
            get
            {
                if (HostNodeControl != null)
                    return HostNodeControl.HostNodesContainer;

                return null;
            }
        }

        ////////////////////////////////////////////////////////////////////
        public delegate void Delegate_OnOperateLinkInfo(LinkInfo linkInfo);
        public event Delegate_OnOperateLinkInfo OnAddLinkInfo;
        public event Delegate_OnOperateLinkInfo OnDelLinkInfo;

        public delegate void Delegate_OnClearLinkInfo();
        public event Delegate_OnClearLinkInfo OnClearLinkInfo;

        public List<LinkInfo> LinkInfos
        {
            get;
            protected set;
        } = new List<LinkInfo>();
        public LinkInfo GetLinkInfo(int index)
        {
            if (index < 0 || index >= LinkInfos.Count)
                return null;
            return LinkInfos[index];
        }
        public int GetLinkInfosCount()
        {
            return LinkInfos.Count;
        }
        public bool MultiLink = false;

        //public List<BaseNodeControl> m_virtualNodes = new List<BaseNodeControl>();
        //////public BaseNodeControl LinkNode;
        public Canvas MainDrawCanvas = null;
        public string LinkName;
        public string PinName;

        enLinkCurveType mLinkCurveType = enLinkCurveType.Bezier;
        public enLinkCurveType LinkCurveType
        {
            get => mLinkCurveType;
            set => mLinkCurveType = value;
        }

        enLinkType mlinkType;
        public enLinkType LinkType            // 连接点类型
        {
            get { return mlinkType; }
            set
            {
                mlinkType = value;
                UpdateToolTip();
            }
        }
        partial void UpdateToolTip();

        public enBezierType BezierType;
        //public LinkPinControl LinkElement;     // 链接的控制对象
        public enLinkOpType LinkOpType;   // 该链接是出还是进

        public bool mIsLoading = false;
        public bool mIsLoadingLinks = false;

        public Guid m_oldGuid;  // 用于复制
        public Guid m_copyedGuid;
        protected Guid m_Guid;
        public Guid GUID
        {
            get { return m_Guid; }
            set
            {
                m_oldGuid = m_Guid;
                m_Guid = value;
            }
        }

        protected string m_ClassType;     // 该链接点所表示的对象类型
        public string ClassType
        {
            get { return m_ClassType; }
            set
            {
                m_ClassType = value;
                UpdateToolTip();
            }
        }

        public bool HasLink
        {
            get { return LinkInfos.Count > 0; }
        }
        
        //public LinkObjInfo(string strIdx)
        //{
        //    GUID = Guid.Parse(strIdx);
        //}

        public bool IsEqual(LinkPinControl info)
        {
            //if (HostNodeControl == info.HostNodeControl &&
            //    LinkType == info.LinkType)
            if(m_Guid == info.m_Guid)
                return true;

            return false;
        }

        public static enLinkType GetLinkTypeFromTypeString(string strType)
        {
            enLinkType retType = enLinkType.Unknow;

            switch (strType)
            {
                case "Unknow":
                    retType = enLinkType.Unknow;
                    break;
                case "Statement":
                    retType = enLinkType.Statement;
                    break;
                case "StatementMirror":
                    retType = enLinkType.StatementMirror;
                    break;
                case "IF_Condition":
                    retType = enLinkType.IF_Condition;
                    break;
                case "IF_Result":
                    retType = enLinkType.IF_Result;
                    break;
                case "Method":
                    retType = enLinkType.Method;
                    break;
                case "Delegate":
                    retType = enLinkType.Delegate;
                    break;
                case "Bool":
                    retType = enLinkType.Bool;
                    break;
                case "Int64":
                    retType = enLinkType.Int64;
                    break;
                case "Single":
                    retType = enLinkType.Single;
                    break;
                case "Double":
                    retType = enLinkType.Double;
                    break;
                case "String":
                    retType = enLinkType.String;
                    break;
                case "Vector2":
                    retType = enLinkType.Vector2;
                    break;
                case "Vector3":
                    retType = enLinkType.Vector3;
                    break;
                case "Vector4":
                    retType = enLinkType.Vector4;
                    break;
                case "Guid":
                    retType = enLinkType.Guid;
                    break;
                case "Byte":
                    retType = enLinkType.Byte;
                    break;
                case "SByte":
                    retType = enLinkType.SByte;
                    break;
                case "Int16":
                    retType = enLinkType.Int16;
                    break;
                case "UInt16":
                    retType = enLinkType.UInt16;
                    break;
                case "UInt32":
                    retType = enLinkType.UInt32;
                    break;
                case "UInt64":
                    retType = enLinkType.UInt64;
                    break;
                case "Value":
                    retType = enLinkType.Value;
                    break;
                case "NumbericalValue":
                    retType = enLinkType.NumbericalValue;
                    break;
                case "UnsignedNumbericalValue":
                    retType = enLinkType.UnsignedNumbericalValue;
                    break;
                case "SignedNumbericalValue":
                    retType = enLinkType.SignedNumbericalValue;
                    break;
                case "VectorValue":
                    retType = enLinkType.VectorValue;
                    break;
                case "Struct":
                    retType = enLinkType.Struct;
                    break;
                case "ClassField":
                    retType = enLinkType.ClassField;
                    break;
                case "Class":
                    retType = enLinkType.Class;
                    break;
                case "Float1":
                    retType = enLinkType.Float1;
                    break;
                case "Float2":
                    retType = enLinkType.Float2;
                    break;
                case "Float3":
                    retType = enLinkType.Float3;
                    break;
                case "Float4":
                    retType = enLinkType.Float4;
                    break;
                case "Texture":
                    retType = enLinkType.Texture;
                    break;
                case "AnimationPose":
                    retType = enLinkType.AnimationPose;
                    break;
                case "Float4x4":
                    retType = enLinkType.Float4x4;
                    break;
                case "Enumerable":
                    retType = enLinkType.Enumerable;
                    break;
                case "GateServer":
                    retType = enLinkType.GateServer;
                    break;
                case "HallServer":
                    retType = enLinkType.HallServer;
                    break;
                case "DataServer":
                    retType = enLinkType.DataServer;
                    break;
                case "ComServer":
                    retType = enLinkType.ComServer;
                    break;
                case "LogServer":
                    retType = enLinkType.LogServer;
                    break;
                case "PathFindServer":
                    retType = enLinkType.PathFindServer;
                    break;
                case "RegServer":
                    retType = enLinkType.RegServer;
                    break;
                case "All":
                    retType = enLinkType.All;
                    break;

                case "Int32":
                    retType = enLinkType.Int32;
                    break;
                case "int":
                    retType = enLinkType.Int32;
                    break;
                case "float":
                case "float1":
                case "half":
                case "Half1":
                    retType = enLinkType.Float1;
                    break;
                case "float2":
                case "half2":
                    retType = enLinkType.Float2;
                    break;
                case "float3":
                case "half3":
                    retType = enLinkType.Float3;
                    break;
                case "float4":
                case "half4":
                    retType = enLinkType.Float4;
                    break;
                case "uint":
                case "uint1":
                    retType = enLinkType.UInt1;
                    break;
                case "uint2":
                    retType = enLinkType.UInt2;
                    break;
                case "uint3":
                    retType = enLinkType.UInt3;
                    break;
                case "uint4":
                    retType = enLinkType.UInt4;
                    break;
                case "matrix":
                case "float4x4":
                case "half4x4":
                    retType = enLinkType.Float4x4;
                    break;

                case "texture":
                case "sampler2D":
                    retType = enLinkType.Texture;
                    break;

                case "Module":
                    retType = enLinkType.Module;
                    break;
                default:
                    {
                        foreach (var i in strs)
                        {
                            if (strType.IndexOf(i) != -1)
                                return enLinkType.Enumerable;
                        }

                        var type = EngineNS.Rtti.RttiHelper.GetTypeFromTypeFullName(strType);

                        return GetLinkTypeFromCommonType(type);
                    }
            }
            return retType;
        }
        static List<string> strs = new List<string>()
        {
            "[]",
            "System.Collections.ArrayList",
            "System.Collections.BitArray",
            "System.Collections.Queue",
            "System.Collections.SortedList",
            "System.Collections.Stack",
            "System.Collections.Generic.Dictionary",
            "System.Collections.Generic.HashSet",
            "System.Collections.Generic.LinkedList",
            "System.Collections.Generic.List",
            "System.Collections.Generic.Queue",
            "System.Collections.Generic.SortedDictionary",
            "System.Collections.Generic.SortedList",
            "System.Collections.Generic.SortedSet",
            "System.Collections.Generic.Stack",
        };
        public static enLinkType GetLinkTypeFromCommonType(Type type)
        {
            enLinkType retType = enLinkType.Unknow;
            if (type == null)
                return retType;
            if (type.IsPointer)
                return enLinkType.IntPtr;
            if (type.IsGenericParameter)
                return enLinkType.GenericParameter;
            if (type == typeof(String))
                return enLinkType.String;
            var it = type.GetInterface("System.Collections.IEnumerable");
            if (it != null)
                return enLinkType.Enumerable;
            if ((type.IsClass || type.IsInterface) && type != typeof(System.Object))
                return enLinkType.Class;

            Type tempType = type;
            while (tempType.BaseType != null)
            {
                tempType = tempType.BaseType;
                if (tempType == typeof(Enum))
                    return enLinkType.Int32;
            }

            if (type == typeof(Boolean))
                retType = enLinkType.Bool;
            else if (type == typeof(Byte))
                retType = enLinkType.Byte;
            else if (type == typeof(UInt16))
                retType = enLinkType.UInt16;
            else if (type == typeof(UInt32))
                retType = enLinkType.UInt32;
            else if (type == typeof(UInt64))
                retType = enLinkType.UInt64;
            else if (type == typeof(SByte))
                retType = enLinkType.SByte;
            else if (type == typeof(Int16))
                retType = enLinkType.Int16;
            else if (type == typeof(Int32))
                retType = enLinkType.Int32;
            else if (type == typeof(Int64))
                retType = enLinkType.Int64;
            else if (type == typeof(Single))
                retType = enLinkType.Single;
            else if (type == typeof(Double))
                retType = enLinkType.Double;
            //else if (type == typeof(String))
            //    retType = enLinkType.String;
            else if (type.FullName == typeof(EngineNS.Vector2).FullName)
                retType = enLinkType.Vector2;
            else if (type.FullName == typeof(EngineNS.Vector3).FullName)
                retType = enLinkType.Vector3;
            else if (type.FullName == typeof(EngineNS.Vector4).FullName)
                retType = enLinkType.Vector4;
            else if (type.FullName == typeof(EngineNS.Color4).FullName)
                retType = enLinkType.Color4;
            else if (type.FullName == typeof(EngineNS.Point).FullName || type.FullName == typeof(EngineNS.PointF).FullName ||
                type.FullName == typeof(EngineNS.Rectangle).FullName || type.FullName == typeof(EngineNS.RectangleF).FullName ||
                type.FullName == typeof(EngineNS.Size).FullName || type.FullName == typeof(EngineNS.SizeF).FullName)
                retType = enLinkType.Class;
            else if (type.FullName == typeof(System.Guid).FullName)
                retType = enLinkType.Guid;
            else if (type.IsValueType)
                retType = enLinkType.Struct;
            else if (type == typeof(System.Object))
                retType = enLinkType.All;

            return retType;
        }

        public bool AddLinkInfo(LinkInfo info)
        {
            if (MultiLink)
            {
                foreach (var linkInfo in LinkInfos)
                {
                    if (info.IsEqual(linkInfo))
                        return false;
                }

                LinkInfos.Add(info);
            }
            else
            {
                if (LinkInfos.Count > 0)
                {
                    if (LinkInfos[0] != null)
                        LinkInfos[0].Clear();
                }

                LinkInfos.Add(info);
            }
            info.LinkCurveType = LinkCurveType;
            OnAddLinkInfo?.Invoke(info);
            AddLinkInfo_WPF(info);
            return true;
        }
        partial void AddLinkInfo_WPF(LinkInfo info);

        public void Clear()
        {
            while (LinkInfos.Count > 0)
                LinkInfos[0].Clear();

            LinkInfos.Clear();
            OnClearLinkInfo?.Invoke();
        }

        public void RemoveLink(LinkInfo linkInfo)
        {
            LinkInfos.Remove(linkInfo);
            HostNodeControl.BreakLink();
            OnDelLinkInfo?.Invoke(linkInfo);
            RemoveLink_WPF(linkInfo);
        }
        partial void RemoveLink_WPF(LinkInfo linkInfo);

        public void Save(EngineNS.IO.XndNode xndNode, bool newGuid)
        {
            //xmlNode.AddAttrib("MultiLink", m_MultiLink.ToString());
            var att = xndNode.AddAttrib("linkData");
            att.Version = 0;
            att.BeginWrite();
            if (newGuid)
            {
                m_copyedGuid = Guid.NewGuid();
                att.Write(m_copyedGuid);
            }
            else
                att.Write(GUID);
            att.EndWrite();

            SaveLinks(xndNode, newGuid);
        }

        protected virtual void SaveLinks(EngineNS.IO.XndNode xndNode, bool newGuid)
        {
            var att = xndNode.AddAttrib("Links");
            att.Version = 0;
            att.BeginWrite();
            att.Write((int)(LinkInfos.Count));
            foreach (var link in LinkInfos)
            {
                // 创建新的GUID则输出以前的GUID
                if (newGuid)
                {
                    att.Write(link.m_linkFromObjectInfo.m_copyedGuid);
                    att.Write(link.m_linkToObjectInfo.m_copyedGuid);
                }
                else
                {
                    att.Write(link.m_linkFromObjectInfo.GUID);
                    att.Write(link.m_linkToObjectInfo.GUID);
                }
            }
            att.EndWrite();
        }

        public void Load(EngineNS.IO.XndNode xndNode)
        {
            mIsLoading = true;

            Clear();

            var att = xndNode.FindAttrib("linkData");
            att.BeginRead();
            switch (att.Version)
            {
                case 0:
                    {
                        Guid tempGuid;
                        att.Read(out tempGuid);
                        GUID = tempGuid;
                    }
                    break;
            }
            att.EndRead();
            LoadLink(xndNode);
            mIsLoading = false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xmlNode"></param>
        /// <param name="includeIdList">包含列表（只连接在此列表中的连线）</param>
        protected virtual void LoadLink(EngineNS.IO.XndNode xndNode)
        {
            mIsLoadingLinks = true;
            mLinkPairs.Clear();
            var att = xndNode.FindAttrib("Links");
            att.BeginRead();
            switch (att.Version)
            {
                case 0:
                    {
                        int count = 0;
                        att.Read(out count);
                        for (int i = 0; i < count; i++)
                        {
                            var pair = new LinkPair();
                            att.Read(out pair.LoadedLinkFrom);
                            att.Read(out pair.LoadedLinkTo);
                            mLinkPairs.Add(pair);
                            //var startObj = OnGetLinkObjectWithGUID(linkFrom);
                            //var endObj = OnGetLinkObjectWithGUID(linkTo);
                            //if (startObj != null && endObj != null)
                            //{
                            //    //if(bCopy && (!startObj.m_linkObj.Selected || !endObj.m_linkObj.Selected))
                            //    //    continue;

                            //    var linkInfo = new LinkInfo(MainDrawCanvas, startObj, endObj);
                            //}
                        }
                    }
                    break;
            }
            att.EndRead();

            mIsLoadingLinks = false;
        }
        protected class LinkPair
        {
            public Guid LoadedLinkFrom;
            public Guid LoadedLinkTo;
        }
        protected List<LinkPair> mLinkPairs = new List<LinkPair>();
        public virtual void ConstructLinkInfo(NodesContainer container, List<Guid> includeIdList = null)
        {
            for (int i = 0; i < mLinkPairs.Count; ++i)
            {
                var from = mLinkPairs[i].LoadedLinkFrom;
                var to = mLinkPairs[i].LoadedLinkTo;
                if (from == Guid.Empty || to == Guid.Empty)
                    return;
                if (includeIdList != null && !includeIdList.Contains(from))
                    return;
                if (includeIdList != null && !includeIdList.Contains(to))
                    return;
                var startObj = container.GetLinkObjectWithGUID(ref from);
                var endObj = container.GetLinkObjectWithGUID(ref to);
                if (startObj != null && endObj != null)
                {
                    LinkInfo.CreateLinkInfo(LinkCurveType, MainDrawCanvas, startObj, endObj);
                }
            }
        }

        public BaseNodeControl GetLinkedObject(int nIdx, bool bFromObject)
        {
            if (nIdx >= LinkInfos.Count)
                return null;

            if (bFromObject)
            {
                var node = LinkInfos[nIdx].m_linkFromObjectInfo.HostNodeControl;
                return node;
            }
            else
            {
                var node = LinkInfos[nIdx].m_linkToObjectInfo.HostNodeControl;
                return node;
            }
        }
        public BaseNodeControl GetLinkedObject(int nIdx)
        {
            if (nIdx >= LinkInfos.Count)
                return null;

            if(LinkOpType == enLinkOpType.Start)
            {
                var node = LinkInfos[nIdx].m_linkToObjectInfo.HostNodeControl;
                return node;
            }
            else
            {
                var node = LinkInfos[nIdx].m_linkFromObjectInfo.HostNodeControl;
                return node;
            }
        }
        public BaseNodeControl[] GetLinkedObjects()
        {
            var retVal = new BaseNodeControl[LinkInfos.Count];
            for(int i=0; i<LinkInfos.Count; i++)
            {
                if (LinkOpType == enLinkOpType.Start)
                    retVal[i] = LinkInfos[i].m_linkToObjectInfo.HostNodeControl;
                else
                    retVal[i] = LinkInfos[i].m_linkFromObjectInfo.HostNodeControl;
            }
            return retVal;
        }

        public LinkPinControl GetLinkedPinControl(int nIdx, bool bFromObject)
        {
            if (nIdx >= LinkInfos.Count)
                return null;

            if (bFromObject)
                return LinkInfos[nIdx].m_linkFromObjectInfo;
            else
                return LinkInfos[nIdx].m_linkToObjectInfo;
        }
        public LinkPinControl GetLinkedPinControl(int nIdx)
        {
            if (nIdx >= LinkInfos.Count)
                return null;

            if (this.LinkOpType == enLinkOpType.Start)
                return LinkInfos[nIdx].m_linkToObjectInfo;
            else
                return LinkInfos[nIdx].m_linkFromObjectInfo;
        }

        public enLinkType GetLinkType(int nIdx, bool bFromObject)
        {
            if (nIdx >= LinkInfos.Count)
                return enLinkType.Unknow;

            if (bFromObject)
                return LinkInfos[nIdx].m_linkFromObjectInfo.LinkType;
            else
                return LinkInfos[nIdx].m_linkToObjectInfo.LinkType;
        }

        public virtual bool CanLinkWith(BaseNodeControl.LinkPinDescDic.PinDesc desc)
        {
            if (LinkOpType == desc.PinOpType)
                return false;
            if ((LinkType & desc.PinType) > 0)
            {
                if (string.IsNullOrEmpty(ClassType) || string.IsNullOrEmpty(desc.ClassType))
                    return true;
                else if (ClassType == desc.ClassType)
                    return true;
            }

            return false;
        }
    }
}
