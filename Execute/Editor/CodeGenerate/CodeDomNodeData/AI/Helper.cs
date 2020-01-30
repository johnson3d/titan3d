using CodeGenerateSystem.Base;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace CodeDomNode.AI
{
    [EngineNS.Rtti.MetaClass]
    public class CenterDataWarpper : EngineNS.IO.Serializer.Serializer
    {
        EngineNS.RName mCenterDataName = EngineNS.RName.EmptyName;
        [EngineNS.Rtti.MetaData]
        public EngineNS.RName CenterDataName
        {
            get => mCenterDataName;
            set
            {
                if (CenterDataName == value)
                    return;
                mCenterDataName = value;
                mCenterDataGetter = EngineNS.CEngine.Instance.MacrossDataManager.NewObjectGetter<EngineNS.GamePlay.Actor.GCenterData>(value);
                CenterDataNameChange?.Invoke();
            }
        }
        public Action CenterDataNameChange;
        EngineNS.Macross.MacrossGetter<EngineNS.GamePlay.Actor.GCenterData> mCenterDataGetter = null;
        public EngineNS.Macross.MacrossGetter<EngineNS.GamePlay.Actor.GCenterData> CenterDataGetter
        {
            get
            {
                if(mCenterDataGetter == null)
                {
                    mCenterDataGetter = EngineNS.CEngine.Instance.MacrossDataManager.NewObjectGetter<EngineNS.GamePlay.Actor.GCenterData>(mCenterDataName);
                    if(mCenterDataName == EngineNS.RName.EmptyName)
                    {
                        System.Diagnostics.Debug.Assert(false);
                    }
                }
                return mCenterDataGetter;
            }
        }
        public Type CenterDataType { get => CenterDataGetter.Get(false).GetType(); }
    }
    public class BehaviorTree_BTCenterDataConstructionParams : ConstructionParams
    {
        [EngineNS.Rtti.MetaData]
        public CenterDataWarpper BTCenterDataWarpper { get; set; } = new CenterDataWarpper();
        public override EditorCommon.CodeGenerateSystem.INodeConstructionParams Duplicate()
        {
            var retVal = base.Duplicate() as BehaviorTree_BTCenterDataConstructionParams;
            retVal.BTCenterDataWarpper = BTCenterDataWarpper;
            return retVal;
        }
    }
    public class BehaviorTree_BTCenterDataControl : CodeGenerateSystem.Base.BaseNodeControl
    {
        CenterDataWarpper mBTCenterDataWarpper = null;
        public CenterDataWarpper BTCenterDataWarpper
        {
            get => mBTCenterDataWarpper;
            set
            {
                mBTCenterDataWarpper = value;
                var cp = CSParam as BehaviorTree_BTCenterDataConstructionParams;
                cp.BTCenterDataWarpper = value;
                mBTCenterDataWarpper.CenterDataNameChange = OnCenterDataChange;
            }
        }
        public BehaviorTree_BTCenterDataControl()
        {

        }
        public BehaviorTree_BTCenterDataControl(BehaviorTree_BTCenterDataConstructionParams csParam) : base(csParam)
        {
            mBTCenterDataWarpper = csParam.BTCenterDataWarpper;
            mBTCenterDataWarpper.CenterDataNameChange = OnCenterDataChange;
        }
        public virtual void OnCenterDataChange()
        {
        }
        public virtual void CalculateNodePriority(ref int priority)
        {

        }
        public virtual void ResetNodePriority()
        {
            var type = this.GetType();
            var pp = type.GetProperty("Priotiry");
            if (pp != null)
                pp.SetValue(this, "-1");
        }
    }
    public class BehaviorTree_BTNodeInnerNodeConstructionParams : BehaviorTree_BTCenterDataConstructionParams
    {
        public override EditorCommon.CodeGenerateSystem.INodeConstructionParams Duplicate()
        {
            var retVal = base.Duplicate() as BehaviorTree_BTCenterDataConstructionParams;
            return retVal;
        }
    }

    public class BehaviorTree_BTNodeInnerNode : BehaviorTree_BTCenterDataControl
    {

        #region ShowProperty
        protected CodeGenerateSystem.Base.GeneratorClassBase mTemplateClassInstance = null;
        public override object GetShowPropertyObject()
        {
            return mTemplateClassInstance;
        }
        protected void CreateBinding(GeneratorClassBase templateClassInstance, string templateClassPropertyName, DependencyProperty bindedDP, object defaultValue)
        {
            var pro = templateClassInstance.GetType().GetProperty(templateClassPropertyName);
            pro.SetValue(templateClassInstance, defaultValue);
            SetBinding(bindedDP, new Binding(templateClassPropertyName) { Source = templateClassInstance, Mode = BindingMode.TwoWay });
        }
        #endregion ShowProperty
        #region Move
        bool mIsLButtonDown = false;
        protected override void DragObj_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            mIsLButtonDown = true;
            //base.DragObj_MouseLeftButtonDown(sender, e);
            Mouse.Capture(this);
        }
        protected override void DragObj_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            mIsLButtonDown = false;
            SelectedNodeIngoreParent();
            Mouse.Capture(null);
            e.Handled = true;
            //base.DragObj_MouseLeftButtonUp(sender, e);
        }
        protected override void DragObj_MouseMove(object sender, MouseEventArgs e)
        {
            if (mIsLButtonDown)
            {
                _OnMoveNode(this);
                e.Handled = true;
            }
        }
        #endregion Move
        public static void InitNodePinTypes(CodeGenerateSystem.Base.ConstructionParams smParam)
        {
        }
        public BehaviorTree_BTNodeInnerNode()
        {

        }
        public BehaviorTree_BTNodeInnerNode(BehaviorTree_BTNodeInnerNodeConstructionParams csParam) : base(csParam)
        {
        }
        public override void CalculateNodePriority(ref int priority)
        {
            priority++;
            var type = this.GetType();
            var pp = type.GetProperty("Priotiry");
            pp.SetValue(this, priority.ToString());
        }
    }
    public class BehaviorTree_BTNodeModifiersConstructionParams : BehaviorTree_BTCenterDataConstructionParams
    {
        [EngineNS.Rtti.MetaData]
        public List<BTNodeModifier> BTNodeModifiers { get; set; } = new List<BTNodeModifier>();
        public override EditorCommon.CodeGenerateSystem.INodeConstructionParams Duplicate()
        {
            var retVal = base.Duplicate() as BehaviorTree_BTNodeModifiersConstructionParams;
            retVal.BTNodeModifiers = BTNodeModifiers;
            return retVal;
        }
    }

    public class BehaviorTree_BTNodeModifiers : BehaviorTree_BTCenterDataControl
    {
        public BehaviorTree_BTNodeModifiers()
        {

        }
        public BehaviorTree_BTNodeModifiers(BehaviorTree_BTNodeModifiersConstructionParams csParam) : base(csParam)
        {
            BTNodeModifiers = csParam.BTNodeModifiers;
        }
        protected void CreateBinding(GeneratorClassBase templateClassInstance, string templateClassPropertyName, DependencyProperty bindedDP, object defaultValue)
        {
            var pro = templateClassInstance.GetType().GetProperty(templateClassPropertyName);
            pro.SetValue(templateClassInstance, defaultValue);
            SetBinding(bindedDP, new Binding(templateClassPropertyName) { Source = templateClassInstance, Mode = BindingMode.TwoWay });
        }
        public override BaseNodeControl Duplicate(DuplicateParam param)
        {
            var node = base.Duplicate(param) as BehaviorTree_BTNodeModifiers;
            for(int i = 0;i< ServiceNodes.Count;++i)
            {
                node.AddService(ServiceNodes[i].Duplicate(param));
            }
            for (int i = 0; i < DecoratorNodes.Count; ++i)
            {
                node.AddDecorator(DecoratorNodes[i].Duplicate(param));
            }
            return node;
        }
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
        }
        #region InitializeLinkControl
        protected CodeGenerateSystem.Base.LinkPinControl mInLinkHandle = new CodeGenerateSystem.Base.LinkPinControl();
        protected CodeGenerateSystem.Base.LinkPinControl mOutLinkHandle = new CodeGenerateSystem.Base.LinkPinControl();
        protected virtual void InitializeLinkControl(ConstructionParams csParam)
        {
            if (mInLinkHandle != null)
            {
                mInLinkHandle.OnAddLinkInfo += InLinkHandle_OnAddLinkInfo;
                mInLinkHandle.OnDelLinkInfo += InLinkHandle_OnDelLinkInfo;
                mInLinkHandle.OnClearLinkInfo += InLinkHandle_OnClearLinkInfo;
            }
            if (mOutLinkHandle != null)
            {
                mOutLinkHandle.OnAddLinkInfo += OutLinkHandle_OnAddLinkInfo;
                mOutLinkHandle.OnDelLinkInfo += OutLinkHandle_OnDelLinkInfo;
                mOutLinkHandle.OnClearLinkInfo += OutLinkHandle_OnClearLinkInfo;
            }
        }
        private void InLinkHandle_OnAddLinkInfo(LinkInfo linkInfo)
        {
            ReCalculatePriotiry();
        }
        private void InLinkHandle_OnDelLinkInfo(LinkInfo linkInfo)
        {
            ReCalculatePriotiry();
        }
        private void InLinkHandle_OnClearLinkInfo()
        {
            ReCalculatePriotiry();
        }
        private void OutLinkHandle_OnAddLinkInfo(LinkInfo linkInfo)
        {
            ReCalculatePriotiry();
        }
        private void OutLinkHandle_OnDelLinkInfo(LinkInfo linkInfo)
        {
            ReCalculatePriotiry();
        }
        private void OutLinkHandle_OnClearLinkInfo()
        {
            ReCalculatePriotiry();
        }
        public override bool CanLink(LinkPinControl selfLinkPin, LinkPinControl otherLinkPin)
        {
            if (selfLinkPin.HostNodeControl == otherLinkPin.HostNodeControl)
                return false;
            return base.CanLink(selfLinkPin, otherLinkPin);
        }
        public void ReCalculatePriotiry()
        {
            for (int i = 0; i < HostNodesContainer.CtrlNodeList.Count; ++i)
            {
                var node = HostNodesContainer.CtrlNodeList[i] as BehaviorTree_BTCenterDataControl;
                if (node != null)
                {
                    node.ResetNodePriority();
                }
            }
            for (int i = 0; i < HostNodesContainer.CtrlNodeList.Count; ++i)
            {
                var root = HostNodesContainer.CtrlNodeList[i] as BehaviorTree_RootControl;
                if (root != null)
                {
                    root.ReCalculatePriority();
                }
            }
        }
        public static void InitNodePinTypes(CodeGenerateSystem.Base.ConstructionParams smParam)
        {
            CollectLinkPinInfo(smParam, "InLinkHandle", CodeGenerateSystem.Base.enLinkType.BehaviorTree, CodeGenerateSystem.Base.enBezierType.Top, CodeGenerateSystem.Base.enLinkOpType.End, false);
            CollectLinkPinInfo(smParam, "OutLinkHandle", CodeGenerateSystem.Base.enLinkType.BehaviorTree, CodeGenerateSystem.Base.enBezierType.Bottom, CodeGenerateSystem.Base.enLinkOpType.Start, true);
        }
        #endregion InitializeLinkControl
        #region ContextMenu & childNode
        [EngineNS.Rtti.MetaData]
        public List<BTNodeModifier> BTNodeModifiers { get; set; } = new List<BTNodeModifier>();
        protected List<BaseNodeControl> mServiceNodes = new List<BaseNodeControl>();
        public List<CodeGenerateSystem.Base.BaseNodeControl> ServiceNodes
        {
            get => mServiceNodes;
        }
        public List<CodeGenerateSystem.Base.BaseNodeControl> DecoratorNodes
        {
            get => mChildNodes;
        }
        //public ObservableCollection<CodeGenerateSystem.Base.BaseNodeControl> DecoratorNodes { get; set; } = new ObservableCollection<CodeGenerateSystem.Base.BaseNodeControl>();
        public override void OnOpenContextMenu(ContextMenu contextMenu)
        {
            Helper.ConstructContextMenu(contextMenu, this);
        }
        public class NodeUndoRedoData
        {
            public BaseNodeControl Ctrl;
            public CodeGenerateSystem.Controls.NodesContainerControl SubContainer;
        }
        public void DeleteClick(BaseNodeControl ctrl)
        {
            if (!CheckCanDelete())
                return;
            if (mChildNodes.Contains(ctrl))
            {
                NodeUndoRedoData data = new NodeUndoRedoData();
                data.Ctrl = ctrl;
                if(HostNodesContainer.HostControl.SubNodesContainers.ContainsKey(ctrl.Id))
                {
                    data.SubContainer = HostNodesContainer.HostControl.SubNodesContainers[ctrl.Id];
                }
                var redoAction = new Action<object>((obj) =>
                {
                    DeleteDecorator(data.Ctrl);
                    //HostNodesContainer.HostControl.SubNodesContainers.Remove(data.Ctrl.Id);
                });
                redoAction.Invoke(null);
                EditorCommon.UndoRedo.UndoRedoManager.Instance.AddCommand(HostNodesContainer.HostControl.UndoRedoKey, null, redoAction, null,
                                                (obj) =>
                                                {
                                                    AddDecorator(ctrl);
                                                    //HostNodesContainer.HostControl.SubNodesContainers.Add(data.Ctrl.Id, data.SubContainer);
                                                }, "Delete Node");
            }
            if (mServiceNodes.Contains(ctrl))
            {
                var redoAction = new Action<object>((obj) =>
                {
                    DeleteService(ctrl);
                });
                redoAction.Invoke(null);
                EditorCommon.UndoRedo.UndoRedoManager.Instance.AddCommand(HostNodesContainer.HostControl.UndoRedoKey, null, redoAction, null,
                                                (obj) =>
                                                {
                                                    AddService(ctrl);
                                                }, "Delete Node");
            }
            IsDirty = true;
        }
        protected override void DragObj_MouseMove(object sender, MouseEventArgs e)
        {
            base.DragObj_MouseMove(sender, e);
        }
        #region Decorator
        public void AddDecorator(BaseNodeControl ctrl)
        {
            AddChildNode(ctrl, mChildNodeContainer);
            var bnm = new BTNodeModifier();
            bnm.CSParam = ctrl.CSParam;
            bnm.CtrlType = ctrl.GetType();
            BTNodeModifiers.Add(bnm);
            ReCalculatePriotiry();
        }

        public void AddDecorator(ConstructionParams modifiderParam, Type ctrlType, bool add2ModifierList)
        {
            modifiderParam.CSType = HostNodesContainer.CSType;
            var node = System.Activator.CreateInstance(ctrlType, new object[] { modifiderParam }) as BaseNodeControl;
            AddChildNode(node, mChildNodeContainer);
            if (add2ModifierList)
            {
                var bnm = new BTNodeModifier();
                bnm.CSParam = modifiderParam;
                bnm.CtrlType = ctrlType;
                BTNodeModifiers.Add(bnm);
            }
            ReCalculatePriotiry();
        }
        public void DeleteDecorator(BaseNodeControl ctrl)
        {
            var index = DecoratorNodes.IndexOf(ctrl);
            RemoveChildNode(ctrl);
            BTNodeModifiers.RemoveAt(index);
            ReCalculatePriotiry();
        }
        protected override void AddChildNode(BaseNodeControl node, Panel container)
        {
            if (container == null)
                return;
            if (DecoratorNodes.Contains(node))
                return;
            node.HostNodesContainer = HostNodesContainer;
            DecoratorNodes.Add(node);
            node.SetParentNode(this, true, System.Windows.Visibility.Visible);

            //DecoratorNodes.Add(node);
            node.OnSelectNode += DecoratorNode_OnSelectNode;
            node.OnMoveNode += ChildNode_OnMoveNode;
            container.Children.Add(node);
            mChildNodeContainer = container;
        }
        private void DecoratorNode_OnSelectNode(BaseNodeControl node, bool bSelected, bool unselectedOther)
        {
            for (int i = 0; i < DecoratorNodes.Count; ++i)
            {
                DecoratorNodes[i].Selected = false;
            }
            for (int i = 0; i < ServiceNodes.Count; ++i)
            {
                ServiceNodes[i].Selected = false;
            }
            var assist = this.HostNodesContainer.HostControl as Macross.NodesControlAssist;
            assist.HostControl.OnSelectNodeControl(node);
        }
        private void ChildNode_OnMoveNode(BaseNodeControl node)
        {
            var mousePos = Mouse.GetPosition(mChildNodeContainer);
            for (int i = 0; i < mChildNodeContainer.Children.Count; ++i)
            {
                if (mChildNodeContainer.Children[i] == node)
                    continue;
                var childWidth = (mChildNodeContainer.Children[i] as BaseNodeControl).ActualHeight;
                var childPos = mChildNodeContainer.Children[i].TranslatePoint(new Point(0, 0), mChildNodeContainer);
                if (mousePos.Y > childPos.Y && mousePos.Y < childPos.Y + childWidth)
                {
                    if (mousePos.Y < childPos.Y + childWidth * 0.5f)
                    {
                        mChildNodeContainer.Children.Remove(node);
                        mChildNodeContainer.Children.Insert(i, node);
                        DecoratorNodes.Remove(node);
                        DecoratorNodes.Insert(i, node);
                        ReCalculatePriotiry();
                        break;
                    }
                }
            }
            //ChildrenPanel.Children.Remove(node);
            //throw new NotImplementedException();
        }
        protected override void StartDrag(UIElement dragObj, MouseButtonEventArgs e)
        {
            base.StartDrag(dragObj, e);
            int deepestZIndex = -10;
            // 计算所有在范围内的节点
            for (int i = 0; i < DecoratorNodes.Count; ++i)
            {
                var zIndex = Canvas.GetZIndex(DecoratorNodes[i]);
                if (deepestZIndex > zIndex)
                    deepestZIndex = zIndex;
                DecoratorNodes[i].CalculateDeltaPt(e);
            }
            Canvas.SetZIndex(this, deepestZIndex - 1);
        }
        protected override void DragMove(MouseEventArgs e)
        {
            base.DragMove(e);
            var pt = e.GetPosition(ParentDrawCanvas);
            for (int i = 0; i < DecoratorNodes.Count; i++)
            {
                DecoratorNodes[i].MoveWithPt(pt);
            }
            ReCalculatePriotiry();
        }
        #endregion Decorator
        #region Service
        protected System.Windows.Controls.Panel mServiceContainer;
        public void AddService(BaseNodeControl ctrl)
        {
            AddServiceNode(ctrl, mServiceContainer);
            //var bnm = new BTNodeModifier();
            //bnm.CSParam = ctrl.CSParam;
            //bnm.CtrlType = ctrl.GetType();
            //BTNodeModifiers.Add(bnm);
        }
        public void AddService(ConstructionParams modifiderParam, Type ctrlType, bool add2ModifierList)
        {
            modifiderParam.CSType = HostNodesContainer.CSType;
            var node = System.Activator.CreateInstance(ctrlType, new object[] { modifiderParam }) as BaseNodeControl;
            AddServiceNode(node, mServiceContainer);
            if (add2ModifierList)
            {
                var bnm = new BTNodeModifier();
                bnm.CSParam = modifiderParam;
                bnm.CtrlType = ctrlType;
                //BTNodeModifiers.Add(bnm);
            }
        }
        public void DeleteService(BaseNodeControl ctrl)
        {
            //var index = ServiceNodes.IndexOf(ctrl);
            ctrl.Clear();
            mServiceNodes.Remove(ctrl);
            ctrl.HostNodesContainer = null;
            mServiceContainer.Children.Remove(ctrl);
            //BTNodeModifiers.RemoveAt(index);
        }
        protected void AddServiceNode(BaseNodeControl node, Panel container)
        {
            if (container == null)
                return;
            if (ServiceNodes.Contains(node))
                return;
            node.HostNodesContainer = HostNodesContainer;
            ServiceNodes.Add(node);
            node.SetParentNode(this, true, System.Windows.Visibility.Visible);

            //DecoratorNodes.Add(node);
            node.OnSelectNode += ServiceNode_OnSelectNode;
            //node.OnMoveNode += ChildNode_OnMoveNode;
            container.Children.Add(node);
            mServiceContainer = container;

        }
        private void ServiceNode_OnSelectNode(BaseNodeControl node, bool bSelected, bool unselectedOther)
        {
            for (int i = 0; i < DecoratorNodes.Count; ++i)
            {
                DecoratorNodes[i].Selected = false;
            }
            for (int i = 0; i < ServiceNodes.Count; ++i)
            {
                ServiceNodes[i].Selected = false;
            }
            var assist = this.HostNodesContainer.HostControl as Macross.NodesControlAssist;
            assist.HostControl.OnSelectNodeControl(node);
        }
        #endregion Service
        //protected override bool NodeNameCheckCanChange(string newVal)
        //{
        //    return GetValidClipName(newVal) == newVal;
        //}
        //protected override void NodeNameChangedOverride(BaseNodeControl d, string oldVal, string newVal)
        //{
        //    for (int i = 0; i < ContainedTransitonNodes.Count; ++i)
        //    {
        //        ContainedTransitonNodes[i].NodeName = newVal;
        //    }
        //}
        #endregion ContextMenu 
        public override void ResetNodePriority()
        {
            for (int i = 0; i < mChildNodes.Count; ++i)
            {
                var node = mChildNodes[i] as BehaviorTree_BTCenterDataControl;
                if (node != null)
                {
                    node.ResetNodePriority();
                }
            }
            var type = this.GetType();
            var pp = type.GetProperty("Priotiry");
            if (pp != null)
                pp.SetValue(this, "-1");
        }
        public override void CalculateNodePriority(ref int priority)
        {
            for (int i = 0; i < mChildNodes.Count; ++i)
            {
                var node = mChildNodes[i] as BehaviorTree_BTCenterDataControl;
                if (node != null)
                {
                    node.CalculateNodePriority(ref priority);
                }
            }

            priority++;
            var type = this.GetType();
            var pp = type.GetProperty("Priotiry");
            pp.SetValue(this, priority.ToString());
            var linkObjs = mOutLinkHandle.GetLinkedObjects();
            for (int i = 0; i < linkObjs.Length; ++i)
            {
                for (int j = i; j < linkObjs.Length; ++j)
                {
                    if (linkObjs[i].GetLeftInCanvas(false) > linkObjs[j].GetLeftInCanvas(false))
                    {
                        var temp = linkObjs[i];
                        linkObjs[i] = linkObjs[j];
                        linkObjs[j] = temp;
                    }
                }
            }
            for (int i = 0; i < linkObjs.Length; ++i)
            {
                var node = linkObjs[i] as BehaviorTree_BTCenterDataControl;
                if (node != null)
                {
                    node.CalculateNodePriority(ref priority);
                }
            }
        }
        public override void Clear()
        {
            base.Clear();
            for (int i = 0; i < DecoratorNodes.Count; ++i)
            {
                HostNodesContainer.DeleteNode(DecoratorNodes[i]);
            }
            DecoratorNodes.Clear();
            for (int i = 0; i < ServiceNodes.Count; ++i)
            {
                HostNodesContainer.DeleteNode(ServiceNodes[i]);
            }
            ServiceNodes.Clear();
        }
        #region Save&Load
        public override void Save(EngineNS.IO.XndNode xndNode, bool newGuid)
        {
            base.Save(xndNode, newGuid);
            var childNode = xndNode.AddNode("ServiceNodes", 0, 0);
            foreach (var child in ServiceNodes)
            {
                var tempNode = childNode.AddNode(child.GetType().Name, 0, 0);
                var cnhAtt = tempNode.AddAttrib("_childNodeHead");
                cnhAtt.Version = 1;
                cnhAtt.BeginWrite();
                var nodeTypeSaveString = CodeGenerateSystem.Program.GetNodeControlTypeSaveString(child.GetType());
                cnhAtt.Write(nodeTypeSaveString);
                //cnhAtt.Write(child.CSParam.ConstructParam);
                cnhAtt.EndWrite();
                child.CSParam.Write(tempNode);
                child.Save(tempNode, newGuid);
            }
        }
        public override async System.Threading.Tasks.Task Load(EngineNS.IO.XndNode xndNode)
        {
            await base.Load(xndNode);
            var childNode = xndNode.FindNode("ServiceNodes");
            if (childNode == null)
                return;
            foreach (var cNode in childNode.GetNodes())
            {
                try
                {
                    var cnhAtt = cNode.FindAttrib("_childNodeHead");
                    if (cnhAtt != null)
                    {
                        switch (cnhAtt.Version)
                        {
                            case 1:
                                {
                                    cnhAtt.BeginRead();
                                    string nodeTypeSaveString;
                                    cnhAtt.Read(out nodeTypeSaveString);
                                    cnhAtt.EndRead();
                                    var ctrlType = CodeGenerateSystem.Program.GetNodeControlTypeFromSaveString(nodeTypeSaveString);
                                    if (ctrlType == null)
                                    {
                                        EngineNS.Profiler.Log.WriteLine(EngineNS.Profiler.ELogTag.Error, CodeGenerateSystem.Program.MessageCategory, $"类型错误 BaseNodeControl.Load {nodeTypeSaveString}");
                                        continue;
                                    }
                                    var csParam = CreateConstructionParam(ctrlType);
                                    csParam.CSType = mCSParam.CSType;
                                    csParam.HostNodesContainer = mCSParam.HostNodesContainer;
                                    csParam.Read(cNode);

                                    SetConstructionParams_Visual(csParam);
                                    var ins = (BaseNodeControl)System.Activator.CreateInstance(ctrlType, new object[] { csParam });
                                    //ins.IsChildNode = true;
                                    await ins.Load(cNode);
                                    AddServiceNode(ins, mServiceContainer);
                                }
                                break;
                        }
                    }
                }
                catch (System.Exception e)
                {
                    EngineNS.Profiler.Log.WriteLine(EngineNS.Profiler.ELogTag.Error, CodeGenerateSystem.Program.MessageCategory, e.ToString());
                }
            }
        }
        #endregion Save&Load
        public string ValidName
        {
            get { return StringRegex.GetValidName(NodeName + "_" + Id.ToString()); }
        }
        public override CodeExpression GCode_CodeDom_GetSelfRefrence(LinkPinControl element, GenerateCodeContext_Method context, GenerateCodeContext_PreNode preNodeContext = null)
        {
            if (DecoratorNodes.Count > 0)
            {
                return DecoratorNodes[0].GCode_CodeDom_GetSelfRefrence(null, null);
            }
            return new CodeVariableReferenceExpression(ValidName);
        }
        public override async System.Threading.Tasks.Task GCode_CodeDom_GenerateCode(CodeTypeDeclaration codeClass, CodeStatementCollection codeStatementCollection, LinkPinControl element, GenerateCodeContext_Method context)
        {
            for (int i = 0; i < DecoratorNodes.Count; ++i)
            {
                var ctrl = DecoratorNodes[i];
                await ctrl.GCode_CodeDom_GenerateCode(codeClass, codeStatementCollection, element, context);
            }
            for (int i = 0; i < DecoratorNodes.Count - 1; ++i)
            {
                var childAssign = new CodeAssignStatement();
                childAssign.Left = new CodeFieldReferenceExpression(DecoratorNodes[i].GCode_CodeDom_GetSelfRefrence(null, null), "Child");
                childAssign.Right = DecoratorNodes[i + 1].GCode_CodeDom_GetSelfRefrence(null, null);
                codeStatementCollection.Add(childAssign);
            }
            var behavior = await GCode_CodeDom_GenerateBehavior(codeClass, codeStatementCollection, element, context);
            if (DecoratorNodes.Count > 0)
            {
                var childAssign = new CodeAssignStatement();
                childAssign.Left = new CodeFieldReferenceExpression(DecoratorNodes[DecoratorNodes.Count - 1].GCode_CodeDom_GetSelfRefrence(null, null), "Child");
                childAssign.Right = behavior;
                codeStatementCollection.Add(childAssign);
            }
            for (int i = 0; i < ServiceNodes.Count; ++i)
            {
                var ctrl = ServiceNodes[i];
                await ctrl.GCode_CodeDom_GenerateCode(codeClass, codeStatementCollection, element, context);
            }
            for (int i = 0; i < ServiceNodes.Count; ++i)
            {
                var addChild = new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(new CodeMethodReferenceExpression(behavior, "AddService"), new CodeExpression[] { ServiceNodes[i].GCode_CodeDom_GetSelfRefrence(null, null) });
                codeStatementCollection.Add(addChild);
            }
        }
        public virtual async System.Threading.Tasks.Task<CodeExpression> GCode_CodeDom_GenerateBehavior(CodeTypeDeclaration codeClass, CodeStatementCollection codeStatementCollection, LinkPinControl element, GenerateCodeContext_Method context)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            return null;
        }
    }
    public class StringRegex
    {
        public static string GetValidName(string name)
        {
            return Regex.Replace(name, "[ \\[ \\] \\^ \\-*×――(^)$%~!@#$…&%￥—+=<>《》!！??？:：•`·、。，；,.;\"‘’“”-]", "");
        }
    }
    public class Helper
    {
        public static async System.Threading.Tasks.Task<CodeMemberMethod> CreateEvaluateMethod(CodeTypeDeclaration codeClass, string methodName, Type returnType, Type centerDataType, string property, object defaultValue, GenerateCodeContext_Method context)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            var valueEvaluateMethod = new CodeMemberMethod();
            valueEvaluateMethod.Name = methodName;
            valueEvaluateMethod.Attributes = MemberAttributes.Public | MemberAttributes.Final;
            var valueEvaluateMethodContex = new GenerateCodeContext_Method(context.ClassContext, valueEvaluateMethod);
            var centerData = new CodeFieldReferenceExpression(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "HostActor"), "CenterData");
            var currentTypeCenterData = new CodeGenerateSystem.CodeDom.CodeCastExpression(centerDataType, centerData);
            var value = new CodeFieldReferenceExpression(currentTypeCenterData, property);
            var castValue = new CodeGenerateSystem.CodeDom.CodeCastExpression(returnType, value);
            valueEvaluateMethod.ReturnType = new CodeTypeReference(returnType);
            valueEvaluateMethod.Statements.Add(new CodeMethodReturnStatement(castValue));
            codeClass.Members.Add(valueEvaluateMethod);

            return valueEvaluateMethod;
        }
        public static void ConstructContextMenu(ContextMenu contextMenu, BehaviorTree_BTNodeModifiers ctrl)
        {
            var decoratorItem = new MenuItem()
            {
                Name = "Decorator",
                Header = "Decorator",
                Style = ctrl.TryFindResource(new ComponentResourceKey(typeof(ResourceLibrary.CustomResources), "MenuItem_Default")) as Style,
            };
            var item = new MenuItem()
            {
                Name = "ConditionLoop",
                Header = "ConditionLoop",
                Style = ctrl.TryFindResource(new ComponentResourceKey(typeof(ResourceLibrary.CustomResources), "MenuItem_Default")) as Style,
            };
            item.Click += (itemSender, itemE) =>
            {
                var csParam = new BehaviorTree_ConditionLoopControlConstructionParams();
                csParam.BTCenterDataWarpper.CenterDataName = ctrl.BTCenterDataWarpper.CenterDataName;
                ctrl.AddDecorator(csParam, typeof(BehaviorTree_ConditionLoopControl), true);
            };
            //decoratorItem.Items.Add(item);
            item = new MenuItem()
            {
                Name = "CustomCondition",
                Header = "CustomCondition",
                Style = ctrl.TryFindResource(new ComponentResourceKey(typeof(ResourceLibrary.CustomResources), "MenuItem_Default")) as Style,
            };
            item.Click += (itemSender, itemE) =>
            {
                var csParam = new BehaviorTree_ConditionFuncDecoratorControlConstructionParams();
                csParam.BTCenterDataWarpper.CenterDataName = ctrl.BTCenterDataWarpper.CenterDataName;
                ctrl.AddDecorator(csParam, typeof(BehaviorTree_ConditionFuncDecoratorControl), true);
            };
            decoratorItem.Items.Add(item);
            contextMenu.Items.Add(decoratorItem);
            var serviceItem = new MenuItem()
            {
                Name = "Service",
                Header = "Service",
                Style = ctrl.TryFindResource(new ComponentResourceKey(typeof(ResourceLibrary.CustomResources), "MenuItem_Default")) as Style,
            };
            item = new MenuItem()
            {
                Name = "CustomService",
                Header = "CustomService",
                Style = ctrl.TryFindResource(new ComponentResourceKey(typeof(ResourceLibrary.CustomResources), "MenuItem_Default")) as Style,
            };
            item.Click += (itemSender, itemE) =>
            {
                var csParam = new BehaviorTree_CustomServiceControlConstructionParams();
                csParam.BTCenterDataWarpper.CenterDataName = ctrl.BTCenterDataWarpper.CenterDataName;
                ctrl.AddService(csParam, typeof(BehaviorTree_CustomServiceControl), true);
            };
            serviceItem.Items.Add(item);
            contextMenu.Items.Add(serviceItem);
        }
    }

    [EngineNS.Rtti.MetaClass]
    public class BTNodeModifier : EngineNS.IO.Serializer.Serializer
    {
        [EngineNS.Rtti.MetaData]
        public ConstructionParams CSParam { get; set; }
        [EngineNS.Rtti.MetaData]
        public Type CtrlType { get; set; }
    }
}
