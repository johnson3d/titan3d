using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CodeGenerateSystem.Base
{
    [EngineNS.Rtti.MetaClass]
    public partial class ConstructionParams : EngineNS.IO.Serializer.Serializer, EditorCommon.CodeGenerateSystem.INodeConstructionParams
    {
        EngineNS.ECSType mCSType = EngineNS.ECSType.Common;
        public EngineNS.ECSType CSType
        {
            get { return mCSType; }
            set { mCSType = value; }
        }

        private NodesContainer mHostNodesContainer;
        public virtual NodesContainer HostNodesContainer
        {
            get => mHostNodesContainer;
            set
            {
                mHostNodesContainer = value;
            }
        }

        protected string mConstructParam = "";
        [EngineNS.Rtti.MetaData]
        public string ConstructParam
        {
            get => mConstructParam;
            set => mConstructParam = value;
        }

        [EngineNS.Rtti.MetaData]
        public string DisplayName;
        //public Type NodeType
        //{
        //    get;
        //    private set;
        //}

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            var param = obj as ConstructionParams;
            if (param == null)
                return false;
            if (this.GetType() != obj.GetType())
                return false;
            //if ((CSType == param.CSType) &&
            //    (ConstructParam == param.ConstructParam) &&
            //    (NodeType == param.NodeType))
            //    return true;
            if ((CSType == param.CSType) &&
                (ConstructParam == param.ConstructParam))
                return true;
            return false;
        }
        public override int GetHashCode()
        {
            return GetHashCodeString().GetHashCode();
        }
        protected string GetHashCodeString()
        {
            //return NodeType.FullName + CSType.ToString() + ConstructParam;
            return CSType.ToString() + ConstructParam;
        }

        public ConstructionParams()
        {

        }
        //public ConstructionParams(Type nodeType)
        //{
        //    NodeType = nodeType;
        //}

        public virtual void Write(EngineNS.IO.XndNode xndNode)
        {
            var att = xndNode.AddAttrib("ConstructionParams");
            att.Version = 1;
            att.BeginWrite();
            att.WriteMetaObject(this);
            att.EndWrite();
        }
        public virtual void Read(EngineNS.IO.XndNode xndNode)
        {
            var att = xndNode.FindAttrib("ConstructionParams");
            if (att != null)
            {
                switch (att.Version)
                {
                    case 0:
                        att.BeginRead();
                        string csparam;
                        att.Read(out csparam);
                        ConstructParam = csparam;
                        att.EndRead();
                        break;
                    case 1:
                        {
                            att.BeginRead();
                            var obj = att.ReadMetaObject(this);
                            att.EndRead();
                            if(obj == null)
                            {
                                // 向下兼容，使用基础类型再读取一次
                                att.BeginRead();
                                var param = att.ReadMetaObject() as ConstructionParams;
                                if(param != null)
                                    ConstructParam = param.ConstructParam;
                                att.EndRead();
                            }
                        }
                        break;
                }
            }
        }
        public virtual EditorCommon.CodeGenerateSystem.INodeConstructionParams Duplicate()
        {
            //var retVal = System.Activator.CreateInstance(this.GetType(), NodeType) as ConstructionParams;
            var retVal = System.Activator.CreateInstance(this.GetType()) as ConstructionParams;
            retVal.CSType = CSType;
            retVal.HostNodesContainer = HostNodesContainer;
            retVal.ConstructParam = ConstructParam;
            retVal.DisplayName = DisplayName;
            return retVal;
        }
    }

    public class MethodLocalParamData
    {
        public Type ParamType;
        public string ParamName;
    }
    public class MethodGenerateData
    {
        public List<MethodLocalParamData> LocalParams = new List<MethodLocalParamData>();
    }

    public interface IMethodGenerator
    {
        System.Threading.Tasks.Task GCode_CodeDom_GenerateMethodCode(CodeTypeDeclaration codeClass, LinkPinControl element, GenerateCodeContext_Class context, MethodGenerateData data);
    }
    public interface IRNameContainer
    {
        void CollectRefRNames(List<EngineNS.RName> rNames);
    }
    ///
    /// 潜规则：
    ///     1. 每个继承类需要实现 public static void InitNodePinTypes(CodeGenerateSystem.Base.ConstructionParams smParam)
    ///     2. 需要自定义ConstructionParams 的需要使用 CustomConstructionParamsAttribute CollectNodeConstructionParamType
    /// 
    public partial class BaseNodeControl : INotifyPropertyChanged
    {
        #region INotifyPropertyChangedMembers
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
        }
        #endregion

        protected Guid mId;
        public Guid Id
        {
            get { return mId; }
            set { mId = value; }
        }

        protected System.Windows.Controls.Panel mChildNodeContainer;            // 此节点的子节点的容器
        public delegate void Delegate_DirtyChanged(BaseNodeControl node);
        public Delegate_DirtyChanged OnDirtyChange;

        protected bool m_bIsDirty = false;
        public bool IsDirty
        {
            get { return m_bIsDirty; }
            set
            {
                m_bIsDirty = value;
                OnDirtyChange?.Invoke(this);
            }
        }

        protected string mNodeName = Program.NodeDefaultName;
        public virtual string NodeName
        {
            get { return mNodeName; }
            set
            {
                if (mNodeName == value)
                    return;
                if (!NodeNameCheckCanChange(value))
                    return;
                var oldValue = mNodeName;
                mNodeName = NodeNameCoerceValueCallbackOverride(this, value);
                if (oldValue != value)
                    IsDirty = true;
                NodeNameChangedOverride(this, oldValue, value);
                SetNodeNamePartial();
                OnPropertyChanged("NodeName");
                OnPropertyChanged("ShowNodeName");
            }
        }

        public bool NodeNameAddShowNodeName = true;
        protected string mShowNodeName;
        public string ShowNodeName
        {
            get
            {
                if(string.IsNullOrEmpty(mShowNodeName))
                    return mNodeName;
                if (NodeNameAddShowNodeName)
                {
                    if (mShowNodeName == mNodeName)
                        return mNodeName;
                    else
                        return mNodeName + "(" + mShowNodeName + ")";
                }
                else
                    return mShowNodeName;
            }
            set
            {
                mShowNodeName = value;
                OnPropertyChanged("ShowNodeName");
            }
        }
        partial void SetNodeNamePartial();
        protected virtual bool NodeNameCheckCanChange(string newVal) { return true; }
        protected virtual string NodeNameCoerceValueCallbackOverride(BaseNodeControl d, string baseValue) { return baseValue; }
        protected virtual void NodeNameChangedOverride(BaseNodeControl d, string oldVal, string newVal) { }

        // 注释
        string mComment;
        public virtual string Comment
        {
            get { return mComment; }
            set
            {
                mComment = value;
                IsDirty = true;
                OnPropertyChanged("Comment");
            }
        }
        public NodesContainer HostNodesContainer
        {
            get
            {
                if (mCSParam != null)
                    return mCSParam.HostNodesContainer;
                return null;
            }
            set
            {
                if (mCSParam != null)
                    mCSParam.HostNodesContainer = value;
            }
        }

        protected CodeGenerateSystem.Controls.NodesContainerControl mLinkedNodesContainer;
        public CodeGenerateSystem.Controls.NodesContainerControl LinkedNodesContainer
        {
            get => mLinkedNodesContainer;
            set => mLinkedNodesContainer = value;
        }

        // 打包他的节点
        public BaseNodeControl PackagedNode
        {
            get;
            set;
        }

        public class LinkPinDescDic
        {
            public class PinDesc
            {
                public string Name;
                public enLinkType PinType;
                public enLinkOpType PinOpType;
                public enBezierType BezierType;
                public string ClassType;
                public bool IsMultiLink;
            }

            public Dictionary<string, PinDesc> LinkPinInfosDic
            {
                get;
                private set;
            } = new Dictionary<string, PinDesc>();

        }

        internal static Dictionary<Type, Type> NodeConstructionParamTypeDic
        {
            get;
            private set;
        } = new Dictionary<Type, Type>();
        internal static Dictionary<EditorCommon.CodeGenerateSystem.INodeConstructionParams, LinkPinDescDic> LinkPinDescs
        {
            get;
            private set;
        } = new Dictionary<EditorCommon.CodeGenerateSystem.INodeConstructionParams, LinkPinDescDic>();
        //List<LinkObjInfo> mLinkObjInfoList = new List<LinkObjInfo>();
        //protected Dictionary<object, LinkObjInfo> mLinkObjInfoDic = new Dictionary<object, LinkObjInfo>();
        //public LinkObjInfo GetLinkObjInfo(object key)
        //{
        //    LinkObjInfo outValue = null;
        //    Dictionary<LinkObjInfoKey, LinkObjInfo> dic;
        //    if (mLinkObjInfos.TryGetValue(CSParam, out dic))
        //        dic.TryGetValue(key, out outValue)
        //    mLinkObjInfoDic.TryGetValue(key, out outValue);
        //    return outValue;
        //}
        //////public LinkPinControl[] GetLinkPinInfos()
        //////{
        //////    var retValue = new LinkPinControl[mLinkPinInfoDic_Name.Values.Count];
        //////    mLinkPinInfoDic_Name.Values.CopyTo(retValue, 0);
        //////    return retValue;
        //////}


        protected ConstructionParams mCSParam;
        public ConstructionParams CSParam
        {
            get { return mCSParam; }
        }
        partial void InitConstruction();

        public BaseNodeControl(ConstructionParams csParam)
        {
            Id = System.Guid.NewGuid();
            mCSParam = csParam;
            InitConstruction();
            InvokeInitConstructionParamsType(this.GetType());
            InvokeInitNodePinTypes(this.GetType(), csParam);
        }

        public static Type InvokeInitConstructionParamsType(Type nodeType)
        {
            var atts = nodeType.GetCustomAttributes(typeof(CustomConstructionParamsAttribute), false);
            if (atts.Length > 0)
            {
                var ccAtt = atts[0] as CustomConstructionParamsAttribute;
                CollectNodeConstructionParamType(nodeType, ccAtt.ConstructionParamsType);
                return ccAtt.ConstructionParamsType;
            }
            //throw new InvalidOperationException($"无法找到类型{nodeType.FullName}对应的构造参数类型,请实现从{typeof(CodeGenerateSystem.Base.ConstructionParams).FullName}继承的类型并使用{typeof(CustomConstructionParamsAttribute).FullName}进行标识");
            return typeof(ConstructionParams);
        }
        public static void InvokeInitNodePinTypes(Type nodeType, EditorCommon.CodeGenerateSystem.INodeConstructionParams csParam)
        {
            var method = nodeType.GetMethod("InitNodePinTypes", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static| System.Reflection.BindingFlags.FlattenHierarchy, null, new Type[] { typeof(CodeGenerateSystem.Base.ConstructionParams) }, null);
            if (method == null)
            {
                throw new ArgumentNullException($"类{nodeType.FullName}没有实现函数InitNodePinTypes");
            }
            method.Invoke(null, new object[] { csParam });
        }

        partial void SetToolTip_WPF(object toolTip);
        public void SetToolTip(object toolTip)
        {
            SetToolTip_WPF(toolTip);
        }

        partial void Clear_WPF();
        // Clear带自删除功能，使用时注意
        public virtual void Clear()
        {
            Clear_WPF();

            foreach (var pin in mLinkPinInfoDic_Name.Values)
            {
                pin.Clear();
            }

            //while (m_outLinks.Count > 0)
            //{
            //    m_outLinks[0].RemoveLink();
            //}
            //foreach (var link in m_outLinks)
            //    link.RemoveLink();

            //while (m_inLinks.Count > 0)
            //{
            //    m_inLinks[0].RemoveLink();
            //}
            //foreach (var link in m_inLinks)
            //    link.RemoveLink();

            foreach (var node in mChildNodes)
            {
                node.Clear();
            }
            mChildNodes.Clear();

            //foreach (var node in mMoveableChildNodes.Values)
            //{
            //    node.Clear();
            //}
            //mMoveableChildNodes.Clear();
        }

        #region 代码生成

        //public struct stStringCodeInfo
        //{
        //    public string strSegment;
        //    public string strInclude;
        //    public string strFunction;
        //}
        public bool IsOnlyReturnValue
        {
            get;
            protected set;
        } = false;

        // 刷新数据以便生成代码
        public virtual void ReInitForGenericCode() { }

        // nLayer 代码行前空格的数量
        protected string GCode_GetTabString(int nLayer)
        {
            string retStr = "";

            for (int i = 0; i < nLayer; ++i)
                retStr += "    ";

            return retStr;
        }

        //public virtual string GCode_GetValue(FrameworkElement element)
        //{
        //    //return "//" + GetType() + "-------------------没有值--------------------//";
        //    return "";
        //}

        // 是否拥有多个输出连线
        public virtual bool HasMultiOutLink
        {
            get { return false; }
        }

        public virtual string GCode_GetValueName(LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            var errorStr = $"{NodeName} 没有实现GCode_GetValueName";
            throw new InvalidOperationException(errorStr);
            //return "//" + GetType() + ".GCode_GetValueName(LinkControl element) //-------------------没有值名称----------------//";
        }

        public virtual string GCode_GetTypeString(LinkPinControl element, GenerateCodeContext_Method context)
        {
            var errorStr = $"{NodeName} 没有实现GCode_GetValueType";
            throw new InvalidOperationException(errorStr);
            //return "// " + GetType() + ".GCode_GetValueType(LinkControl element) //-------------------没有值类型----------------//";
        }

        public virtual Type GCode_GetType(LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            return null;
        }
        public virtual void GCode_GenerateCode(ref string strDefinitionSegment, ref string strSegment, int nLayer, LinkPinControl element, CodeGenerateSystem.Base.GenerateCodeContext_Method context)
        {
            //return GCode_GetTabString(nLayer) + "// " + GetType() + ".GCode_GenerateCode(int nLayer, LinkControl element) //-----------------未生成代码------------------//";
        }

        public virtual async System.Threading.Tasks.Task GCode_CodeDom_GenerateCode(CodeNamespace codeNameSpace, LinkPinControl element, GenerateCodeContext_Namespace context)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            CodeCommentStatement ccs = new CodeCommentStatement(this.GetType().ToString() + "未生成代码[void GCode_CodeDom_GenerateCode(CodeNamespace codeNameSpace)]");
            codeNameSpace.Comments.Add(ccs);
        }
        public virtual async System.Threading.Tasks.Task GCode_CodeDom_GenerateCode(CodeTypeDeclaration codeClass, LinkPinControl element, GenerateCodeContext_Class context)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            CodeCommentStatement ccs = new CodeCommentStatement(this.GetType().ToString() + "未生成代码[void GCode_CodeDom_GenerateCode(CodeTypeDeclaration codeClass)]");
            codeClass.Comments.Add(ccs);
        }
        public virtual async System.Threading.Tasks.Task GCode_CodeDom_GenerateCode(CodeTypeDeclaration codeClass, CodeStatementCollection codeStatementCollection, LinkPinControl element, GenerateCodeContext_Method context)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            CodeCommentStatement ccs = new CodeCommentStatement(this.GetType().ToString() + "未生成代码[void GCode_CodeDom_GenerateCode(CodeStatementCollection codeStatementCollection)]");
            codeStatementCollection.Add(ccs);
        }
        public virtual CodeExpression GCode_CodeDom_GetValue(LinkPinControl element, GenerateCodeContext_Method context, GenerateCodeContext_PreNode preNodeContext = null)
        {
            return new CodePrimitiveExpression(null);
        }
        //获取自己节点的引用对象，如果有的话
        public virtual CodeExpression GCode_CodeDom_GetSelfRefrence(LinkPinControl element, GenerateCodeContext_Method context, GenerateCodeContext_PreNode preNodeContext = null)
        {
            return new CodePrimitiveExpression(null);
        }

        public virtual bool Pin_UseOrigionParamName(LinkPinControl linkElement)
        {
            return false;
        }

        ////////////// 代码生成用
        ////////////// 取得包含此节点的StateSet类名称
        ////////////public delegate string Delegate_GetOwnerStateSetClassTypeName();
        ////////////public event Delegate_GetOwnerStateSetClassTypeName OnGetOwnerStateSetClassTypeName;
        ////////////public virtual string GCode_OnGetOwnerStateSetClassTypeName()
        ////////////{
        ////////////    if (OnGetOwnerStateSetClassTypeName != null)
        ////////////        return OnGetOwnerStateSetClassTypeName();

        ////////////    return "";
        ////////////}

        #endregion

        #region 父子节点相关

        protected BaseNodeControl mParentNode;
        public BaseNodeControl ParentNode
        {
            get { return mParentNode; }
        }
        protected List<BaseNodeControl> mChildNodes = new List<BaseNodeControl>();
        public List<CodeGenerateSystem.Base.BaseNodeControl> GetChildNodes()
        {
            return mChildNodes;
        }
        //protected Dictionary<string, BaseNodeControl> mMoveableChildNodes = new Dictionary<string, BaseNodeControl>();

        //protected BaseNodeControl mMemberParentNode;
        //public BaseNodeControl MemberParentNode
        //{
        //    get { return mMemberParentNode; }
        //}

        bool mIsChildNode = false;
        public virtual bool IsChildNode
        {
            get { return mIsChildNode; }
            set
            {
                mIsChildNode = value;

                if (mIsChildNode)
                    IsSelfDeleteable = false;
                else
                    IsSelfDeleteable = true;
            }
        }
        // 自身的右键菜单删除功能
        partial void SetIsSelfDeleteable(bool able);
        bool mSelfDeleteable = true;
        public bool IsSelfDeleteable
        {
            get { return mSelfDeleteable; }
            set
            {
                mSelfDeleteable = value;
                SetIsSelfDeleteable(mSelfDeleteable);
            }
        }
        protected bool m_bMoveable = true;          // 表示此节点是否可用鼠标拖动移动
                                                    // 能否删除
        protected bool mIsDeleteable = true;
        public bool IsDeleteable
        {
            get { return mIsDeleteable; }
            set
            {
                mIsDeleteable = value;
                if (!mIsDeleteable)
                    IsSelfDeleteable = false;
            }
        }
        public virtual bool CheckCanDelete()
        {
            return IsDeleteable;
        }
        //public void AddMoveableChildNode(BaseNodeControl node)
        //{
        //    mMoveableChildNodes[node.NodeName] = node;
        //    node.mMemberParentNode = this;
        //}
        //public void RemoveMoveableChildNode(BaseNodeControl node)
        //{
        //    mMoveableChildNodes.Remove(node.NodeName);
        //    node.mMemberParentNode = null;
        //}
        partial void AddChildNode_WPF(BaseNodeControl node, System.Windows.Controls.Panel container);
        // 带container的node属于控件内的node，不能自主移动
        protected virtual void AddChildNode(BaseNodeControl node, System.Windows.Controls.Panel container)
        {
            if (mChildNodes.Contains(node))
                return;

            //ConditionControl cc = new ConditionControl();
            //cc.OnStartLink += new Delegate_StartLink(StartLink);
            //cc.OnEndLink += new Delegate_EndLink(EndLink);
            //m_conditionControls.Add(cc);
            //LinkStack.Children.Add(cc);

            node.HostNodesContainer = HostNodesContainer;
            node.OnStartLink += new Delegate_StartLink(StartLink);
            node.OnEndLink += new Delegate_EndLink(EndLink);
            //node.OnGetLinkObjectWithGUID += new Delegate_GetLinkObjectWithGUID(Child_OnGetLinkObjectWithGUID);

            mChildNodes.Add(node);
            node.SetUpLinkElement(m_methodUpLinkElement);
            node.SetParentNode(this, false, System.Windows.Visibility.Visible);

            AddChildNode_WPF(node, container);
        }
        partial void InsertChildNode_WPF(int index, BaseNodeControl node, System.Windows.Controls.Panel container);
        protected virtual void InsertChildNode(int index, BaseNodeControl node, System.Windows.Controls.Panel container)
        {
            if (index < 0 || index >= mChildNodes.Count)
                AddChildNode(node, container);
            if (mChildNodes.Contains(node))
                return;

            node.HostNodesContainer = HostNodesContainer;
            node.OnStartLink += new Delegate_StartLink(StartLink);
            node.OnEndLink += new Delegate_EndLink(EndLink);

            mChildNodes.Insert(index, node);
            node.SetUpLinkElement(m_methodUpLinkElement);
            node.SetParentNode(this, false, Visibility.Visible);

            InsertChildNode_WPF(index, node, container);
        }
        partial void InsertChildNodeNoChangeContainer_WPF(int index, BaseNodeControl node, System.Windows.Controls.Panel container);
        protected virtual void InsertChildNodeNoChangeContainer(int index, BaseNodeControl node, System.Windows.Controls.Panel container)
        {
            if (index < 0 || index >= mChildNodes.Count)
                throw new InvalidOperationException("index超出范围");
            if (mChildNodes.Contains(node))
                return;

            node.HostNodesContainer = HostNodesContainer;
            node.OnStartLink += new Delegate_StartLink(StartLink);
            node.OnEndLink += new Delegate_EndLink(EndLink);

            mChildNodes.Insert(index, node);
            node.SetUpLinkElement(m_methodUpLinkElement);
            node.SetParentNode(this, false, Visibility.Visible);

            if (index >= container.Children.Count)
                AddChildNodeNoChanageContainer_WPF(node, container);
            else
                InsertChildNodeNoChangeContainer_WPF(index, node, container);
        }

        protected virtual void RemoveChildNode(BaseNodeControl node)
        {
            node.Clear();
            mChildNodes.Remove(node);
            node.HostNodesContainer = null;
            node.OnStartLink -= StartLink;
            node.OnEndLink -= EndLink;
            RemoveChildFromNodeContainerPanel(node);
        }
        protected virtual void RemoveChildNodeByIndex(int index)
        {
            if (index < 0 || index >= mChildNodes.Count)
                return;
            var node = mChildNodes[index];
            node.HostNodesContainer = null;
            node.OnStartLink -= StartLink;
            node.OnEndLink -= EndLink;
            mChildNodes.RemoveAt(index);
            RemoveChildFromNodeContainerPanel(node);
        }

        //LinkObjInfo Child_OnGetLinkObjectWithGUID(Guid guid)
        //{
        //    if (OnGetLinkObjectWithGUID != null)
        //        return OnGetLinkObjectWithGUID(guid);

        //    return null;
        //}

        // 根据ID查找子节点
        public virtual BaseNodeControl FindChildNode(Guid id)
        {
            foreach (var childNode in mChildNodes)
            {
                if (childNode.Id == id)
                    return childNode;
            }
            //foreach (var childNode in mMoveableChildNodes.Values)
            //{
            //    if (childNode.Id == id)
            //        return childNode;
            //}

            return null;
        }
        public virtual BaseNodeControl FindChildNode(Type nodeType)
        {
            foreach (var childNode in mChildNodes)
            {
                if (childNode.GetType() == nodeType)
                    return childNode;
            }
            //foreach (var childNode in mMoveableChildNodes.Values)
            //{
            //    if (childNode.GetType() == nodeType)
            //        return childNode;
            //}

            return null;
        }

        partial void RemoveChildFromNodeContainerPanel(BaseNodeControl child);
        protected virtual void ClearChildNode()
        {
            foreach (var child in mChildNodes)
            {
                child.OnStartLink -= new Delegate_StartLink(StartLink);
                child.OnEndLink -= new Delegate_EndLink(EndLink);
                RemoveChildFromNodeContainerPanel(child);
            }
            mChildNodes.Clear();
        }

        partial void AddChildNodeNoChanageContainer_WPF(BaseNodeControl node, Panel container);
        protected void AddChildNodeNoChanageContainer(BaseNodeControl node, Panel container)
        {
            if (mChildNodes.Contains(node))
                return;
            mChildNodes.Add(node);
            node.SetParentNode(this, false, Visibility.Visible);
            node.OnStartLink += new Delegate_StartLink(StartLink);
            node.OnEndLink += new Delegate_EndLink(EndLink);
            //node.OnGetLinkObjectWithGUID += new Delegate_GetLinkObjectWithGUID(Child_OnGetLinkObjectWithGUID);
            node.SetUpLinkElement(m_methodUpLinkElement);

            AddChildNodeNoChanageContainer_WPF(node, container);
        }
        partial void SetParentNode_WPF(BaseNodeControl parentNode, bool Moveable, Visibility Visible);
        public void SetParentNode(BaseNodeControl parentNode, bool Moveable, Visibility Visible)
        {
            if (parentNode == null)
                return;
            IsChildNode = true;
            SetParentNode_WPF(parentNode, Moveable, Visible);
            mParentNode = parentNode;
            m_bMoveable = Moveable;
        }

        #endregion

        #region 链接相关

        public virtual bool CanLink(LinkPinControl selfLinkPin, LinkPinControl otherLinkPin)
        {
            return true;
        }

        public LinkPinControl GetLinkPinInfo(string name)
        {
            LinkPinControl outValue;
            mLinkPinInfoDic_Name.TryGetValue(name, out outValue);
            return outValue;
        }

        public virtual LinkPinControl GetLinkPinInfo(ref Guid guid)
        {
            foreach (var pinInfo in mLinkPinInfoDic_Name.Values)
            {
                if (pinInfo.GUID == guid)
                    return pinInfo;
            }

            foreach (var child in mChildNodes)
            {
                var linkObjInfo = child.GetLinkPinInfo(ref guid);
                if (linkObjInfo != null)
                    return linkObjInfo;
            }

            //foreach (var child in mMoveableChildNodes.Values)
            //{
            //    var linkObjInfo = child.GetLinkPinInfo(ref guid);
            //    if (linkObjInfo != null)
            //        return linkObjInfo;
            //}

            return null;
        }

        public static ConstructionParams CreateConstructionParam(Type nodeType)
        {
            Type paramType;
            if (!BaseNodeControl.NodeConstructionParamTypeDic.TryGetValue(nodeType, out paramType))
            {
                paramType = InvokeInitConstructionParamsType(nodeType);
            }
            return System.Activator.CreateInstance(paramType) as ConstructionParams;
        }
        static void CollectNodeConstructionParamType(Type nodeType, Type constructParamType)
        {
            NodeConstructionParamTypeDic[nodeType] = constructParamType;
        }
        protected static LinkPinDescDic.PinDesc GetLinkPinDesc(ConstructionParams csParam, string pinName)
        {
            LinkPinDescDic dic;
            if (!LinkPinDescs.TryGetValue(csParam, out dic))
            {
                return null;
            }
            LinkPinDescDic.PinDesc pinDesc;
            if (dic.LinkPinInfosDic.TryGetValue(pinName, out pinDesc))
                return pinDesc;

            return null;
        }
        protected static LinkPinDescDic.PinDesc CollectLinkPinInfo(ConstructionParams csParam, string pinName, enLinkType pinType, enBezierType bezierType, enLinkOpType pinOpType, bool bMultiLink, bool force = false)
        {
            if (csParam == null)
                return null;
            LinkPinDescDic dic;
            if (!LinkPinDescs.TryGetValue(csParam, out dic))
            {
                dic = new LinkPinDescDic();
                LinkPinDescs[csParam] = dic;
            }

            LinkPinDescDic.PinDesc pinDesc;
            if (force || !dic.LinkPinInfosDic.TryGetValue(pinName, out pinDesc))
            {
                pinDesc = new LinkPinDescDic.PinDesc();
                //dic.LinkPinInfosDic.Add(pinName, pinDesc);
                dic.LinkPinInfosDic[pinName] = pinDesc;
            }
            pinDesc.Name = pinName;
            pinDesc.PinType = pinType;
            pinDesc.BezierType = bezierType;
            pinDesc.PinOpType = pinOpType;
            pinDesc.IsMultiLink = bMultiLink;
            return pinDesc;
        }

        protected static LinkPinDescDic.PinDesc CollectLinkPinInfo(ConstructionParams csParam, string pinName, Type pinType, enBezierType bezierType, enLinkOpType pinOpType, bool bMultiLink, bool force = false)
        {
            var pinDesc = CollectLinkPinInfo(csParam, pinName, LinkPinControl.GetLinkTypeFromCommonType(pinType), bezierType, pinOpType, bMultiLink, force);
            if (pinType != null)
            {
                if (pinType.IsGenericParameter)
                    pinDesc.ClassType = "System.Object";
                else
                    pinDesc.ClassType = pinType.FullName;
            }
            return pinDesc;
        }
        protected static LinkPinDescDic.PinDesc CollectClassLinkPinInfo(ConstructionParams csParam, string pinName, string pinType, enBezierType bezierType, enLinkOpType pinOpType, bool bMultiLink, bool force = false)
        {
            var pinDesc = CollectLinkPinInfo(csParam, pinName, enLinkType.Class, bezierType, pinOpType, bMultiLink, force);
            pinDesc.ClassType = pinType;
            return pinDesc;
        }

        protected Dictionary<string, LinkPinControl> mLinkPinInfoDic_Name = new Dictionary<string, LinkPinControl>();

        public virtual LinkPinControl[] GetLinkPinInfos()
        {
            var retValue = new LinkPinControl[mLinkPinInfoDic_Name.Values.Count];
            mLinkPinInfoDic_Name.Values.CopyTo(retValue, 0);
            return retValue;
        }

        partial void AddLinkPinInfo_WPF(LinkPinControl linkPin, Brush linkColor);

        protected LinkPinControl AddLinkPinInfo(string linkName, LinkPinControl linkPin, Brush linkColor = null)
        {
            linkPin.HostNodeControl = this;

            LinkPinDescDic dic;
            if (!LinkPinDescs.TryGetValue(CSParam, out dic))
                throw new InvalidOperationException($"无法找到连接点信息，是否{this.GetType().FullName}没有实现InitNodePinTypes?");

            LinkPinDescDic.PinDesc pinDesc;
            if (!dic.LinkPinInfosDic.TryGetValue(linkName, out pinDesc))
                throw new InvalidOperationException($"无法找到名为{linkName}的连接点信息，是否{this.GetType().FullName}中没有实现InitNodePinTypes或InitNodePinTypes中没有调用对应的CollectLinkPinInfo?");

            linkPin.LinkName = linkName;
            linkPin.PinName = pinDesc.Name;
            linkPin.HostNodeControl = this;
            linkPin.LinkType = pinDesc.PinType;
            linkPin.BezierType = pinDesc.BezierType;
            linkPin.LinkOpType = pinDesc.PinOpType;
            linkPin.MultiLink = pinDesc.IsMultiLink;
            linkPin.ClassType = pinDesc.ClassType;
            mLinkPinInfoDic_Name.Add(linkName, linkPin);
            //linkObj.OnGetLinkObjectWithGUID += new LinkObjInfo.Delegate_GetLinkObjectWithGUID(LinkObj_OnGetLinkObjectWithGUID);

            AddLinkPinInfo_WPF(linkPin, linkColor);

            return linkPin;
        }

        protected LinkPinControl m_methodUpLinkElement;
        protected void SetUpLinkElement(LinkPinControl element)
        {
            m_methodUpLinkElement = element;
        }

        //////void LinkContextMenuItem_Click(object sender, RoutedEventArgs e)
        //////{
        //////    MenuItem mItem = sender as MenuItem;            
        //////    mLinkObjList[((ContextMenu)mItem.Parent).PlacementTarget].Clear();

        //////    IsDirty = true;
        //////}

        protected void RemoveLinkObject(LinkPinControl linkPin)
        {
            mLinkPinInfoDic_Name.Remove(linkPin.PinName);
        }
        //public delegate LinkObjInfo Delegate_GetLinkObjectWithGUID(Guid guid);
        //public event Delegate_GetLinkObjectWithGUID OnGetLinkObjectWithGUID;
        //LinkObjInfo LinkObj_OnGetLinkObjectWithGUID(Guid guid)
        //{
        //    if (OnGetLinkObjectWithGUID != null)
        //        return OnGetLinkObjectWithGUID(guid);

        //    return null;
        //}

        public virtual event Delegate_StartLink OnStartLink;
        protected virtual void StartLink(LinkPinControl linkObj)
        {
            // 开始结束都能连
            //if ((linkObj.LinkOpType & enLinkOpType.Start) != enLinkOpType.Start)
            //    return;

            if (OnStartLink != null)
                OnStartLink(linkObj);
        }

        public virtual event Delegate_EndLink OnEndLink;
        protected virtual void EndLink(LinkPinControl linkObj)
        {
            // 开始结束都能连
            //if ((linkObj.LinkOpType & enLinkOpType.End) != enLinkOpType.End)
            //    return;

            if (OnEndLink != null)
                OnEndLink(linkObj);
        }

        public virtual void AfterLink()
        {
            HostNodesContainer.IsDirty = true;
        }

        public virtual void BreakLink()
        {
            HostNodesContainer.IsDirty = true;
        }
        partial void UpdateNodeAndLink_WPF();
        public void UpdateNodeAndLink()
        {
            UpdateNodeAndLink_WPF();
        }

        #endregion

        #region 储存读取
        partial void Save_WPF(EngineNS.IO.XndNode xndNode);
        public virtual void Save(EngineNS.IO.XndNode xndNode, bool newGuid)
        {
            //if (newGuid)
            //    xmlNode.AddAttrib("ID", Id.NewGuid().ToString());
            //else
            var att = xndNode.AddAttrib("_baseNodeData");
            att.Version = 0;
            att.BeginWrite();
            if (newGuid)
            {
                var id = Guid.NewGuid();
                att.Write(id);
            }
            else
                att.Write(Id);
            att.Write(NodeName);
            att.EndWrite();

            Save_WPF(xndNode);

            var childNode = xndNode.AddNode("ChildNodes", 0, 0);
            foreach (var child in mChildNodes)
            {
                var tempNode = childNode.AddNode(child.GetType().Name, 0, 0);
                var cnhAtt = tempNode.AddAttrib("_childNodeHead");
                cnhAtt.Version = 1;
                cnhAtt.BeginWrite();
                var nodeTypeSaveString = Program.GetNodeControlTypeSaveString(child.GetType());
                cnhAtt.Write(nodeTypeSaveString);
                //cnhAtt.Write(child.CSParam.ConstructParam);
                cnhAtt.EndWrite();

                child.CSParam.Write(tempNode);

                child.Save(tempNode, newGuid);
            }

            var linkPinsNode = xndNode.AddNode("LinkPins", 0, 0);
            foreach (var linkPin in mLinkPinInfoDic_Name.Values)
            {
                var tempNode = linkPinsNode.AddNode("LinkPin", 0, 0);
                var headerAtt = tempNode.AddAttrib("header");
                headerAtt.Version = 0;
                headerAtt.BeginWrite();
                // 这里故意赋下值，防止PinName改了导致存盘更改
                string key = linkPin.PinName;
                headerAtt.Write(key);
                headerAtt.EndWrite();
                linkPin.Save(tempNode, newGuid);
            }
            //var linkObjsNode = xndNode.AddNode("LinkObjects", 0, 0);
            //foreach (var linkObj in mLinkObjInfoList)
            //{
            //    var tempNode = linkObjsNode.AddNode("LinkObject", 0, 0);
            //    linkObj.Save(tempNode, newGuid);
            //}
        }

        protected virtual Panel GetChildNodeContainer(BaseNodeControl childNode)
        {
            return mChildNodeContainer;
        }
        protected void SetConstructionParams_Visual(ConstructionParams csParam)
        {
            SetConstructionParams_WPF(csParam);
        }
        partial void SetConstructionParams_WPF(ConstructionParams csParam);
        partial void Load_WPF(EngineNS.IO.XndNode xndNode);
        public virtual async System.Threading.Tasks.Task Load(EngineNS.IO.XndNode xndNode)
        //public virtual void Load(System.Xml.XmlElement xmlElement)
        {
            var att = xndNode.FindAttrib("_baseNodeData");
            att.BeginRead();
            switch (att.Version)
            {
                case 0:
                    {
                        Guid id;
                        att.Read(out id);
                        string nodeName;
                        att.Read(out nodeName);
                        mNodeName = nodeName;
                        SetNodeNamePartial();
                        OnPropertyChanged("NodeName");
                        Id = id;
                    }
                    break;
            }
            att.EndRead();

            Load_WPF(xndNode);

            ClearChildNode();
            var childNode = xndNode.FindNode("ChildNodes");
            foreach (var cNode in childNode.GetNodes())
            {
                try
                {
                    var cnhAtt = cNode.FindAttrib("_childNodeHead");
                    if (cnhAtt != null)
                    {
                        switch (cnhAtt.Version)
                        {
                            case 0:
                                {
                                    cnhAtt.BeginRead();
                                    string nodeTypeSaveString;
                                    cnhAtt.Read(out nodeTypeSaveString);
                                    string constructParam;
                                    cnhAtt.Read(out constructParam);
                                    cnhAtt.EndRead();
                                    var ctrlType = Program.GetNodeControlTypeFromSaveString(nodeTypeSaveString);
                                    if (ctrlType == null)
                                    {
                                        EngineNS.Profiler.Log.WriteLine(EngineNS.Profiler.ELogTag.Error, Program.MessageCategory, $"类型错误 BaseNodeControl.Load {nodeTypeSaveString}");
                                        continue;
                                    }
                                    var csParam = new ConstructionParams()
                                    {
                                        CSType = mCSParam.CSType,
                                        HostNodesContainer = mCSParam.HostNodesContainer,
                                        ConstructParam = constructParam,
                                    };

                                    SetConstructionParams_WPF(csParam);
                                    var ins = (BaseNodeControl)System.Activator.CreateInstance(ctrlType, new object[] { csParam });
                                    //ins.IsChildNode = true;
                                    await ins.Load(cNode);
                                    AddChildNode(ins, GetChildNodeContainer(ins));
                                }
                                break;
                            case 1:
                                {
                                    cnhAtt.BeginRead();
                                    string nodeTypeSaveString;
                                    cnhAtt.Read(out nodeTypeSaveString);
                                    cnhAtt.EndRead();
                                    var ctrlType = Program.GetNodeControlTypeFromSaveString(nodeTypeSaveString);
                                    if (ctrlType == null)
                                    {
                                        EngineNS.Profiler.Log.WriteLine(EngineNS.Profiler.ELogTag.Error, Program.MessageCategory, $"类型错误 BaseNodeControl.Load {nodeTypeSaveString}");
                                        continue;
                                    }
                                    var csParam = CreateConstructionParam(ctrlType);
                                    csParam.CSType = mCSParam.CSType;
                                    csParam.HostNodesContainer = mCSParam.HostNodesContainer;
                                    csParam.Read(cNode);

                                    SetConstructionParams_WPF(csParam);
                                    var ins = (BaseNodeControl)System.Activator.CreateInstance(ctrlType, new object[] { csParam });
                                    //ins.IsChildNode = true;
                                    await ins.Load(cNode);
                                    AddChildNode(ins, GetChildNodeContainer(ins));
                                }
                                break;
                        }
                    }
                }
                catch (System.Exception e)
                {
                    EngineNS.Profiler.Log.WriteLine(EngineNS.Profiler.ELogTag.Error, Program.MessageCategory, e.ToString());
                }
            }

            var linkNode = xndNode.FindNode("LinkObjects");
            if (linkNode != null)
            {
                var linkNodes = linkNode.GetNodes();
                int i = 0;
                foreach (var pinInfo in mLinkPinInfoDic_Name.Values)
                {
                    if (i >= linkNodes.Count)
                        break;
                    pinInfo.Load(linkNodes[i]);
                    i++;
                }
            }
            else
            {
                var pinsNode = xndNode.FindNode("LinkPins");
                foreach (var pinNode in pinsNode.GetNodes())
                {
                    var headerAtt = pinNode.FindAttrib("header");
                    if (headerAtt == null)
                        continue;
                    string pinName = "";
                    headerAtt.BeginRead();
                    switch (headerAtt.Version)
                    {
                        case 0:
                            {
                                headerAtt.Read(out pinName);
                            }
                            break;
                    }
                    headerAtt.EndRead();
                    LinkPinControl pin;
                    if (mLinkPinInfoDic_Name.TryGetValue(pinName, out pin))
                        pin.Load(pinNode);
                }
            }

            IsDirty = false;
        }

        public void ConstructLinkInfo(NodesContainer container, List<Guid> includeIdList = null)
        {
            foreach (var linkPin in mLinkPinInfoDic_Name.Values)
            {
                linkPin.ConstructLinkInfo(container, includeIdList);
            }

            foreach (var cNode in mChildNodes)
            {
                cNode.ConstructLinkInfo(container, includeIdList);
            }
        }
        public void CollectPinIds(BaseNodeControl oldNode, Dictionary<Guid, Guid> oldNewCor)
        {
            var oldPins = oldNode.GetLinkPinInfos();
            foreach(var oldPin in oldPins)
            {
                var newPin = GetLinkPinInfo(oldPin.LinkName);
                oldNewCor[oldPin.GUID] = newPin.GUID;
            }

            for(int i=0; i<mChildNodes.Count; i++)
            {
                var oldChild = oldNode.mChildNodes[i];
                var newChild = mChildNodes[i];
                newChild.CollectPinIds(oldChild, oldNewCor);
            }
        }
        public virtual bool CanDuplicate()
        {
            return true;
        }
        public class DuplicateParam
        {
            public NodesContainer TargetNodesContainer { get; set; } = null;
        }
        public virtual BaseNodeControl Duplicate(DuplicateParam param)
        {
            var copyedCSParam = CSParam.Duplicate() as ConstructionParams;
            copyedCSParam.DrawCanvas = CSParam.DrawCanvas;
            var cparams = new object[] { copyedCSParam };
            var copyedNode = (BaseNodeControl)System.Activator.CreateInstance(this.GetType(), cparams);
            copyedNode.NodeName = NodeName;
            copyedNode.SetNodeNamePartial();
            copyedNode.OnPropertyChanged("NodeName");
            copyedNode.Comment = Comment;
            copyedNode.NodeNameAddShowNodeName = NodeNameAddShowNodeName;
            //这里千万不要Duplicate ChildNodes ，在派生类里自己加
            return copyedNode;
        }
        public void CopyLink(BaseNodeControl srcNode, List<BaseNodeControl> allNodes, Dictionary<Guid, Guid> oldNewCor, Canvas drawCanvas)
        {
            var oldPins = srcNode.GetLinkPinInfos();
            foreach (var pin in oldPins)
            {
                var newPin = GetLinkPinInfo(pin.LinkName);
                if (newPin == null)
                    continue;
                foreach(var linkInfo in pin.LinkInfos)
                {
                    var fromPin = linkInfo.m_linkFromObjectInfo;
                    Guid newFromId;
                    if (!oldNewCor.TryGetValue(fromPin.GUID, out newFromId))
                        continue;
                    var toPin = linkInfo.m_linkToObjectInfo;
                    Guid newToId;
                    if (!oldNewCor.TryGetValue(toPin.GUID, out newToId))
                        continue;

                    LinkPinControl newStart = null, newEnd = null;
                    foreach(var node in allNodes)
                    {
                        var st = node.GetLinkPinInfo(ref newFromId);
                        if (st != null)
                            newStart = st;
                        var ed = node.GetLinkPinInfo(ref newToId);
                        if (ed != null)
                            newEnd = ed;
                    }
                    if (newStart != null && newEnd != null)
                        LinkInfo.CreateLinkInfo(newPin.LinkCurveType, drawCanvas, newStart, newEnd);
                }
            }

            for(int i=0; i<mChildNodes.Count; i++)
            {
                var oldChild = srcNode.mChildNodes[i];
                var newChild = mChildNodes[i];
                newChild.CopyLink(oldChild, allNodes, oldNewCor, drawCanvas);
            }
        }

        public virtual void ContainerLoadComplete(NodesContainer container)
        {

        }
        //public virtual void LoadLinks(EngineNS.IO.XndNode xndNode, List<string> includeIdList = null)
        ////public virtual void LoadLinks(System.Xml.XmlElement xmlElement)
        //{
        //    var childNode = xndNode.FindNode("ChildNodes");
        //    foreach (var cXmlNode in childNode.GetNodes())
        //    {
        //        string strGUID = cXmlNode.FindAttrib("ID").Value;
        //        Guid tagGUID = Guid.Parse(strGUID);
        //        foreach (var child in mChildNodes)
        //        {
        //            if (child.Id == tagGUID)
        //                child.LoadLinks(cXmlNode, includeIdList);
        //        }
        //    }
        //    //int idx = 0;
        //    //foreach (var cNode in childNode.GetNodes())
        //    //{
        //    //    mChildNodes[idx].LoadLinks(cNode);
        //    //    idx++;
        //    //}

        //    //             childNode = xmlNode.FindNode("MoveableChildNodes");
        //    //             foreach (var cXmlNode in childNode.GetNodes())
        //    //             {
        //    //                 string strGUID = cXmlNode.FindAttrib("ID").Value;
        //    //                 Guid tagGUID = Guid.Parse(strGUID);
        //    //                 foreach (var child in mMoveableChildNodes.Values)
        //    //                 {
        //    //                     if (child.Id == tagGUID)
        //    //                         child.LoadLinks(cXmlNode, includeIdList);
        //    //                 }
        //    //             }
        //    //idx = 0;
        //    //foreach (var child in mMoveableChildNodes.Values)
        //    //{
        //    //    try
        //    //    {
        //    //        child.LoadLinks(childNode.GetNodes()[idx]);
        //    //        idx++;
        //    //    }
        //    //    catch (System.Exception e)
        //    //    {
        //    //        continue;
        //    //    }                
        //    //}

        //    //System.Xml.XmlNodeList list = xmlElement.GetElementsByTagName("ChildNodes");
        //    //if (list.Count > 0)
        //    //{
        //    //    int idx = 0;
        //    //    foreach (System.Xml.XmlElement element in list[0].ChildNodes)
        //    //    {
        //    //        mChildNodes[idx].Load(element);
        //    //        idx++;
        //    //    }
        //    //}

        //    var links = xndNode.FindNode("LinkObjects");
        //    foreach (EngineNS.IO.XmlNode linkNode in links.GetNodes())
        //    {
        //        string strGUID = linkNode.FindAttrib("ID").Value;
        //        Guid tagGUID = Guid.Parse(strGUID);
        //        foreach (var linkObj in mLinkObjInfoDic.Values)
        //        {
        //            if (linkObj.GUID == tagGUID)
        //                linkObj.LoadLink(linkNode, includeIdList);
        //        }
        //    }

        //    //idx = 0;
        //    //foreach(var linkObj in m_linkObjList.Values)
        //    //{
        //    //    linkObj.LoadLink(links.GetNodes()[idx]);
        //    //    idx++;
        //    //}

        //    //System.Xml.XmlNodeList list = xmlElement.GetElementsByTagName("LinkObjects");
        //    //if (list.Count > 0)
        //    //{
        //    //    int idx = 0;
        //    //    foreach (var linkObj in m_linkObjList.Values)
        //    //    {
        //    //        var el = list[0].ChildNodes[idx] as System.Xml.XmlElement;
        //    //        linkObj.Load(el);
        //    //        idx++;
        //    //    }
        //    //}

        //    //UpdateLink();
        //}

        //public virtual void InitializeUsefulLinkDatas()
        //{
        //    foreach (var child in mChildNodes)
        //    {
        //        child.InitializeUsefulLinkDatas();
        //    }
        //    foreach (var child in mMoveableChildNodes.Values)
        //    {
        //        child.InitializeUsefulLinkDatas();
        //    }
        //}

        #endregion

        partial void SetNameBinding_WPF(object source, string propertyName);
        public void SetNameBinding(object source, string propertyName)
        {
            SetNameBinding_WPF(source, propertyName);
        }
    }
}
