using System;
using System.Collections.Generic;
using System.Text;

namespace CodeGenerateSystem.Base
{
    public enum ENodeHandleType
    {
        AddNodeControl,
        DeleteNodeControl,
        UpdateNodeControl
    }


    public partial class NodesContainer
    {
        public delegate void Delegate_OnOperateNodeControl(CodeGenerateSystem.Base.BaseNodeControl node);
        public event Delegate_OnOperateNodeControl OnAddedNodeControl;
        public event Delegate_OnOperateNodeControl OnDeletedNodeControl;
        public event Delegate_OnOperateNodeControl OnBeferDeleteNodeControl;
        public event Delegate_OnOperateNodeControl OnInitializeNodeControl;

        public EngineNS.ECSType CSType
        {
            get;
            set;
        } = EngineNS.ECSType.Common;

        protected Guid mGuid = Guid.NewGuid();
        public virtual Guid GUID
        {
            get { return mGuid; }
            set
            {
                mGuid = value;

                // AI部分
                //m_codeAIClassName = Program.AICode_StateSetNamePre + Program.GetValuedGUIDString(GUID);
                //m_codeNameSpace = Program.AICode_NameSpacePre + m_codeAIClassName;
                //m_codeHostName = "MyHost";
            }
        }

        public delegate void Delegate_OnDirtyChanged(bool dirty);
        public event Delegate_OnDirtyChanged OnDirtyChanged;
        protected bool mIgnoreDirtySet = false;
        protected bool mIsDirty = false;
        public bool IsDirty
        {
            get { return mIsDirty; }
            set
            {
                if (mIgnoreDirtySet)
                    return;

                mIsDirty = value;

                if (mIsDirty == false)
                {
                    foreach (var ctrl in mCtrlNodeList)
                    {
                        ctrl.IsDirty = false;
                    }
                }

                OnContainerDirtyChanged(mIsDirty);
                OnDirtyChanged?.Invoke(mIsDirty);
            }
        }

        // 代码的命名空间
        protected string m_codeNameSpace;
        public string CodeNameSpace { get { return m_codeNameSpace; } }

        // Container中包含的节点列表
        protected List<CodeGenerateSystem.Base.BaseNodeControl> mCtrlNodeList = new List<CodeGenerateSystem.Base.BaseNodeControl>();
        public List<CodeGenerateSystem.Base.BaseNodeControl> CtrlNodeList
        {
            get { return mCtrlNodeList; }
        }

        //public string GetNodeName(string name)
        //{
        //    bool find = false;
        //    foreach (var i in mCtrlNodeList)
        //    {
        //        if (i.NodeName == name)
        //        {
        //            find = true;
        //            break;
        //        }
        //    }
        //    if (!find)
        //    {
        //        return name;
        //    }

        //    int index = 1;
        //    while (true)
        //    {
        //        string newName = "";
        //        if (name.IndexOf("(") > 0)
        //        {
        //            newName = name.Insert(name.IndexOf("("), index.ToString());
        //        }
        //        else
        //        {
        //            newName = name + index.ToString();
        //        }

        //        find = false;
        //        foreach (var i in mCtrlNodeList)
        //        {
        //            if (i.NodeName == newName)
        //            {
        //                find = true;
        //                break;
        //            }
        //        }
        //        if (!find)
        //        {
        //            return newName;
        //        }
        //        else
        //        {
        //            index++;
        //        }
        //    }
        //}

        protected virtual void OnContainerDirtyChanged(bool dirty)
        {

        }

        public delegate void Delegate_OnContainLinkNodesChanged(bool bContain, EngineNS.ECSType csType);
        public Delegate_OnContainLinkNodesChanged OnContainLinkNodesChanged;
        bool mContainLinkNodes = false;
        public bool ContainLinkNodes
        {
            get { return mContainLinkNodes; }
            set
            {
                mContainLinkNodes = value;
                OnContainLinkNodesChanged?.Invoke(mContainLinkNodes, CSType);
            }
        }

        List<CodeGenerateSystem.Base.BaseNodeControl> mOrigionNodeControls = new List<CodeGenerateSystem.Base.BaseNodeControl>();
        public List<CodeGenerateSystem.Base.BaseNodeControl> OrigionNodeControls
        {
            get { return mOrigionNodeControls; }
        }

        public void Initialize(EngineNS.ECSType csType)
        {
            CSType = csType;
        }

        #region 节点操作

        public virtual void ClearControlNodes()
        {
            foreach (var node in mCtrlNodeList)
            {
                node.Clear();
                ////////////MainDrawCanvas.Children.Remove(node);
            }
            mCtrlNodeList.Clear();
            mOrigionNodeControls.Clear();

            RefreshNodeProperty(null, ENodeHandleType.UpdateNodeControl);
        }

        public virtual CodeGenerateSystem.Base.LinkPinControl GetLinkObjectWithGUID(ref Guid guid)
        {
            CodeGenerateSystem.Base.LinkPinControl retLinkObj = null;

            foreach (var ctrlNode in mCtrlNodeList)
            {
                retLinkObj = ctrlNode.GetLinkPinInfo(ref guid);
                if (retLinkObj != null)
                    return retLinkObj;
            }

            return retLinkObj;
        }
        public virtual CodeGenerateSystem.Base.BaseNodeControl GetNodeWithGUID(ref Guid guid)
        {
            foreach (var ctrlNode in mCtrlNodeList)
            {
                if (ctrlNode.Id == guid)
                    return ctrlNode;
            }
            return null;
        }

        public delegate void Delegate_OnRefreshLinkControl(CodeGenerateSystem.Base.BaseNodeControl node, ENodeHandleType type);
        public event Delegate_OnRefreshLinkControl OnRefreshLinkControl;
        public virtual void RefreshNodeProperty(CodeGenerateSystem.Base.BaseNodeControl node, ENodeHandleType type)
        {
            if (!IsLoading)
                OnRefreshLinkControl?.Invoke(node, type);
        }

        public CodeGenerateSystem.Base.BaseNodeControl AddOrigionNode(Type nodeType, ConstructionParams csParams, double x, double y)
        {
            mIgnoreDirtySet = true;

            var ctrl = AddNodeControl(nodeType, csParams, x, y);
            ctrl.IsDeleteable = false;

            OrigionNodeControls.Add(ctrl);

            ContainLinkNodes = mCtrlNodeList.Count != OrigionNodeControls.Count;

            mIgnoreDirtySet = false;

            return ctrl;
        }
        partial void SetConstructionParams_WPF(ConstructionParams csParam);
        partial void AddNodeControl_WPF(BaseNodeControl ins, bool addToCanvas, double x, double y);
        public delegate void ScaleChangeDelegate(int scaleDelta);
        public event ScaleChangeDelegate ScaleChange;

        public void ScaleTips(int scaleDelta)
        {
            ScaleChange?.Invoke(scaleDelta);
        }
        public virtual CodeGenerateSystem.Base.BaseNodeControl AddNodeControl(BaseNodeControl ins, double x, double y, bool addToCanvas = true, bool setDirty = true)
        {
            //ins.HostNodesContainer = this;
            //ins.OnMoveNode += new BaseNodeControl.Delegate_MoveNode(OnMoveNode);
            //ins.OnGetOwnerStateSetClassTypeName += new CodeGenerateSystem.Base.BaseNodeControl.Delegate_GetOwnerStateSetClassTypeName(OnGetOwnerStateSetClassTypeName);
            ins.OnDirtyChange = new BaseNodeControl.Delegate_DirtyChanged(OnControlDirtyChanged);
            ////////////if (nodeType == typeof(StatementNode))
            ////////////{
            ////////////    StatementNode stateNode = ins as StatementNode;
            ////////////    stateNode.OnSetDefaultState += new StatementNode.Delegate_OnSetDefaultState(stateNode_OnSetDefaultState);
            ////////////}
            //ins.OnGetLinkObjectWithGUID += new CodeGenerateSystem.Base.BaseNodeControl.Delegate_GetLinkObjectWithGUID(OnGetLinkObjectWithGUID);
            ins.ModifyCreatePosition(ref x, ref y);
            AddNodeControl_WPF(ins, addToCanvas, x, y);
            OnInitializeNodeControl?.Invoke(ins);

            if (addToCanvas)
            {
                mCtrlNodeList.Add(ins);
            }

            ContainLinkNodes = mCtrlNodeList.Count != OrigionNodeControls.Count;
            RefreshNodeProperty(ins, Base.ENodeHandleType.AddNodeControl);
            if (!IsLoading)
            {
                OnAddedNodeControl?.Invoke(ins);
                if (setDirty)
                    IsDirty = true;
            }

            ScaleChange -= ins.ScaleTips;
            ScaleChange += ins.ScaleTips;

            return ins;
        }
        public virtual CodeGenerateSystem.Base.BaseNodeControl AddNodeControl(Type nodeType, EditorCommon.CodeGenerateSystem.INodeConstructionParams csParam, double x, double y, bool addToCanvas = true, bool setDirty = true)
        {
            object[] objects;
            //if(String.IsNullOrEmpty(Params))
            //    objects = new object[] { MainDrawCanvas };
            //else
            objects = new object[] { csParam };
            var ins = (CodeGenerateSystem.Base.BaseNodeControl)System.Activator.CreateInstance(nodeType, objects);
            //ins.HostNodesContainer = this;
            //ins.OnMoveNode += new BaseNodeControl.Delegate_MoveNode(OnMoveNode);
            //ins.OnGetOwnerStateSetClassTypeName += new CodeGenerateSystem.Base.BaseNodeControl.Delegate_GetOwnerStateSetClassTypeName(OnGetOwnerStateSetClassTypeName);
            ins.OnDirtyChange = new BaseNodeControl.Delegate_DirtyChanged(OnControlDirtyChanged);
            ////////////if (nodeType == typeof(StatementNode))
            ////////////{
            ////////////    StatementNode stateNode = ins as StatementNode;
            ////////////    stateNode.OnSetDefaultState += new StatementNode.Delegate_OnSetDefaultState(stateNode_OnSetDefaultState);
            ////////////}
            //ins.OnGetLinkObjectWithGUID += new CodeGenerateSystem.Base.BaseNodeControl.Delegate_GetLinkObjectWithGUID(OnGetLinkObjectWithGUID);
            ins.ModifyCreatePosition(ref x, ref y);
            AddNodeControl_WPF(ins, addToCanvas, x, y);
            OnInitializeNodeControl?.Invoke(ins);

            if (addToCanvas)
            {
                mCtrlNodeList.Add(ins);
            }

            ContainLinkNodes = mCtrlNodeList.Count != OrigionNodeControls.Count;
            RefreshNodeProperty(ins, Base.ENodeHandleType.AddNodeControl);
            if (!IsLoading)
            {
                OnAddedNodeControl?.Invoke(ins);
                if (setDirty)
                    IsDirty = true;
            }

            ScaleChange -= ins.ScaleTips;
            ScaleChange += ins.ScaleTips;

            return ins;
        }

        public BaseNodeControl FindControl(Guid id)
        {
            foreach (var node in mCtrlNodeList)
            {
                if (node.Id == id)
                    return node;

                var childNode = node.FindChildNode(id);
                if (childNode != null)
                    return childNode;
            }

            return null;
        }
        public BaseNodeControl FindControl(Type nodeType)
        {
            foreach (var node in mCtrlNodeList)
            {
                if (node.GetType() == nodeType)
                    return node;

                var childNode = node.FindChildNode(nodeType);
                if (childNode != null)
                    return childNode;
            }

            return null;
        }

        public virtual void OnControlDirtyChanged(BaseNodeControl control)
        {
            if (control.IsDirty)
                IsDirty = true;
        }
        public virtual void DeleteNode(CodeGenerateSystem.Base.BaseNodeControl node, bool needUndoRedo = true)
        {
            OnBeferDeleteNodeControl?.Invoke(node);
            node.Clear();
            mCtrlNodeList.Remove(node);
            ContainLinkNodes = mCtrlNodeList.Count != OrigionNodeControls.Count;
            RefreshNodeProperty(node, Base.ENodeHandleType.DeleteNodeControl);
            OnDeletedNodeControl?.Invoke(node);
            IsDirty = true;
        }

        #endregion

        #region 保存读取

        bool mCheckErrorResult = true;
        partial void CheckNodesError();
        public bool CheckError()
        {
            CheckNodesError();
            return mCheckErrorResult;
        }
        public virtual bool Save(EngineNS.IO.XndNode xndNode)
        {
            var att = xndNode.AddAttrib("Head");
            int ver = 0;
            att.BeginWrite();
            att.Write(ver);
            att.Write(GUID);
            att.EndWrite();
            var childNodesCtrls = xndNode.AddNode("ChildNodesCtrls", 0, 0);
            foreach (var node in mCtrlNodeList)
            {
                var childXnd = childNodesCtrls.AddNode(node.GetType().Name, 0, 0);
                var childAtt = childXnd.AddAttrib("_nodeHead");
                childAtt.Version = 1;
                childAtt.BeginWrite();
                childAtt.Write(Program.GetNodeControlTypeSaveString(node.GetType()));
                //childAtt.Write(node.CSParam.ConstructParam);
                childAtt.EndWrite();

                node.CSParam.Write(childXnd);

                node.Save(childXnd, false);
            }

            var origionCtrlIdsAtt = xndNode.AddAttrib("OrigionCtrlIds");
            origionCtrlIdsAtt.BeginWrite();
            origionCtrlIdsAtt.Write((int)(mOrigionNodeControls.Count));
            foreach (var ctrl in mOrigionNodeControls)
            {
                origionCtrlIdsAtt.Write(ctrl.Id);
            }
            origionCtrlIdsAtt.EndWrite();

            IsDirty = false;

            if (CheckError() == false)
            {
                EditorCommon.MessageBox.Show("节点及链接有错误，请检查！");
                return false;
            }

            return true;
        }

        public bool IsLoading
        {
            get;
            protected set;
        }
        partial void AfterLoad_WPF();
        public virtual async System.Threading.Tasks.Task Load(EngineNS.IO.XndNode xndNode)
        {
            IsLoading = true;

            var att = xndNode.FindAttrib("Head");
            att.BeginRead();
            int ver = 0;
            att.Read(out ver);
            att.Read(out mGuid);
            att.EndRead();

            ClearControlNodes();

            switch (ver)
            {
                case 0:
                    {
                        // 读取节点信息并创建节点
                        var childNodesCtrls = xndNode.FindNode("ChildNodesCtrls");
                        if (childNodesCtrls == null)
                            return;
                        var xndChildNodes = childNodesCtrls.GetNodes();
                        foreach (var cldNode in xndChildNodes)
                        //foreach (XmlElement element in doc.DocumentElement.ChildNodes)
                        {
                            Type ctrlType = null;
                            ConstructionParams csParam = null;
                            var childAtt = cldNode.FindAttrib("_nodeHead");
                            switch (childAtt.Version)
                            {
                                case 0:
                                    {
                                        childAtt.BeginRead();
                                        string typeSaveName;
                                        childAtt.Read(out typeSaveName);
                                        ctrlType = Program.GetNodeControlTypeFromSaveString(typeSaveName);//  Program.GetType(xmlNode.FindAttrib("Type").Value);
                                        string constructParam;
                                        childAtt.Read(out constructParam);
                                        childAtt.EndRead();
                                        if (ctrlType == null)
                                        {
                                            continue;
                                        }
                                        csParam = new ConstructionParams()
                                        {
                                            CSType = this.CSType,
                                            HostNodesContainer = this,
                                            ConstructParam = constructParam,
                                        };
                                    }
                                    break;
                                case 1:
                                    {
                                        childAtt.BeginRead();
                                        string typeSaveName;
                                        childAtt.Read(out typeSaveName);
                                        ctrlType = Program.GetNodeControlTypeFromSaveString(typeSaveName);//  Program.GetType(xmlNode.FindAttrib("Type").Value);
                                        childAtt.EndRead();
                                        if (ctrlType == null)
                                        {
                                            continue;
                                        }
                                        csParam = BaseNodeControl.CreateConstructionParam(ctrlType);
                                        csParam.CSType = this.CSType;
                                        csParam.HostNodesContainer = this;
                                        csParam.Read(cldNode);
                                    }
                                    break;
                            }

                            SetConstructionParams_WPF(csParam);
                            CodeGenerateSystem.Base.BaseNodeControl nodeControl = AddNodeControl(ctrlType, csParam, double.NaN, double.NaN);
                            await nodeControl.Load(cldNode);

                        }

                        // 读取完成后根据id设置一遍链接节点
                        foreach (var node in mCtrlNodeList)
                        {
                            node.ConstructLinkInfo(this);
                        }
                        //foreach (var node in mCtrlNodeList)
                        //{
                        //    node.InitializeUsefulLinkDatas();
                        //}

                        IsLoading = false;
                        IsDirty = false;
                        RefreshNodeProperty(null, ENodeHandleType.UpdateNodeControl);

                        var origionCtrlIdsAtt = xndNode.FindAttrib("OrigionCtrlIds");
                        if (origionCtrlIdsAtt != null)
                        {
                            origionCtrlIdsAtt.BeginRead();
                            int count;
                            origionCtrlIdsAtt.Read(out count);
                            for (int i = 0; i < count; i++)
                            {
                                Guid oriId;
                                origionCtrlIdsAtt.Read(out oriId);
                                var ctrl = FindControl(oriId);
                                if (ctrl != null)
                                    mOrigionNodeControls.Add(ctrl);
                            }
                            origionCtrlIdsAtt.EndRead();
                        }
                    }
                    break;
            }

            AfterLoad_WPF();
            for (int i = 0; i < CtrlNodeList.Count; ++i)
            {
                CtrlNodeList[i].ContainerLoadComplete(this);
            }
            // 加载完后检查节点错误
            CheckError();
        }

        #endregion
    }
}
