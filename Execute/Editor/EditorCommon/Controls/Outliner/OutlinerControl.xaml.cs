using EngineNS.GamePlay;
using EngineNS.GamePlay.Actor;
using EngineNS.GamePlay.SceneGraph;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EditorCommon.Controls.Outliner
{
    public class OutlinerUndoRedoItem
    {
        public OutlinerItem LastParent { get; set; }
        public OutlinerItem Item { get; set; }
        public OutlinerUndoRedoItem(OutlinerItem lastParent, OutlinerItem item)
        {
            LastParent = lastParent;
            Item = item;
        }
    }
    public class InvisibleInOutliner
    {

    }
    /// <summary>
    /// Interaction logic for OutlinerControl.xaml
    /// </summary>
    public partial class OutlinerControl : UserControl, INotifyPropertyChanged
    {
        #region INotifyPropertyChangedMembers
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
        }
        #endregion
        public string UndoRedoKey
        {
            get;
            set;
        }

        public ViewPort.ViewPortControl ViewPort { get; set; } = null;
        ObservableCollection<OutlinerItem> Items { get; set; } = new ObservableCollection<OutlinerItem>();
        public WPG.PropertyGrid LinkedPropertyGrid { get; set; }
        OutlinerTreeViewOperation mOutlinerTreeViewOperation = new OutlinerTreeViewOperation();
        public OutlinerControl()
        {
            InitializeComponent();
            TreeView_Outliner.ItemsSource = Items;
            mOutlinerTreeViewOperation.OutlinerTreeView = TreeView_Outliner;
            mOutlinerTreeViewOperation.OutlinerControl = this;
            mSelectedItemViews.CollectionChanged += SelectedItemViews_CollectionChanged;
        }

        public void AddActorToWorld()
        {

        }
        #region ViewPort
        public void SetViewPort(ViewPort.ViewPortControl viewPort)
        {
            if (ViewPort != null)
            {
                ViewPort.OnSelectAcotrs -= ViewPort_OnSelectAcotrs;
                ViewPort.OnDuplicateAcotr -= ViewPort_OnDuplicateAcotr;
            }
            ViewPort = viewPort;
            ViewPort.OnSelectAcotrs += ViewPort_OnSelectAcotrs;
            ViewPort.OnDuplicateAcotr += ViewPort_OnDuplicateAcotr;
        }

        private void ViewPort_OnDuplicateAcotr(object sender, ViewPort.ViewPortControl.DuplicateActorEventArgs e)
        {
            for (int j = 0; j < Items.Count; ++j)
            {
                var actorItem = GetOutlinerItemByActor(Items[j], e.Origin);
                if (actorItem != null)
                {
                    OutlinerItem parentItem = actorItem;
                    if (actorItem.Parent != null)
                        parentItem = actorItem.Parent;
                    if (e.Duplicated is GPrefab)
                    {
                        parentItem.Add(new PrefabOutlinerItem(e.Duplicated, this));
                    }
                    else if (e.Duplicated is GActor && !IsBlongToPrefab(e.Duplicated))
                    {
                        parentItem.Add(new ActorOutlinerItem(e.Duplicated, this));
                    }
                }
            }
        }

        public OutlinerItem GetOutlinerItemByActor(OutlinerItem item, GActor actor)
        {
            if (item is PrefabOutlinerItem)
            {
                var actorItem = item as PrefabOutlinerItem;
                if (actorItem.Prefab == actor)
                    return actorItem;
            }
            if (item is ActorOutlinerItem)
            {
                var actorItem = item as ActorOutlinerItem;
                if (actorItem != null)
                {
                    if (actorItem.Actor == actor)
                        return actorItem;
                }
            }
            for (int i = 0; i < item.ChildList.Count; ++i)
            {
                var childResult = GetOutlinerItemByActor(item.ChildList[i], actor);
                if (childResult != null)
                    return childResult;
            }
            return null;
        }
        public OutlinerItem GetOutlinerItemByActor(GActor actor)
        {
            for (int j = 0; j < Items.Count; ++j)
            {
                var actorItem = GetOutlinerItemByActor(Items[j], actor);
                if (actorItem != null)
                {
                    return actorItem;
                }
            }
            return null;
        }
        void UnselectedAll(OutlinerItem item)
        {
            item.IsSelected = false;
            item.Contoured = false;
            item.TreeViewItemIsSelected = false;
            for (int i = 0; i < item.ChildList.Count; ++i)
            {
                UnselectedAll(item.ChildList[i]);
            }
        }
        void UnContouredAll(OutlinerItem item)
        {
            item.Contoured = false;
            for (int i = 0; i < item.ChildList.Count; ++i)
            {
                UnContouredAll(item.ChildList[i]);
            }
        }

        bool ViewPort_PickingActror = false;
        public void SelectAcotrs(ViewPort.ViewPortControl.SelectActorData[] actors)
        {
            List<PrefabOutlinerItem> selectedPrefabs = new List<PrefabOutlinerItem>();
            List<OutlinerItem> selectedActorItems = new List<OutlinerItem>();
            ViewPort_PickingActror = true;
            mSelectedItemViews.CollectionChanged -= SelectedItemViews_CollectionChanged;
            for (int i = 0; i < mSelectedItemViews.Count; ++i)
            {
                if (mSelectedItemViews[i] is PrefabOutlinerItem)
                {
                    selectedActorItems.Add(mSelectedItemViews[i]);
                }
                else if (mSelectedItemViews[i] is ActorOutlinerItem)
                {
                    selectedActorItems.Add(mSelectedItemViews[i]);
                }
            }
            mSelectedItemViews.Clear();
            for (int j = 0; j < Items.Count; ++j)
            {
                UnselectedAll(Items[j]);
            }
            mSelectedItemViews.Clear();
            List<GActor> realSelectActors = new List<GActor>();
            for (int i = 0; i < actors.Length; ++i)
            {
                var actor = actors[i].Actor;
                var actorItem = GetOutlinerItemByActor(actor);
                if (actorItem != null)
                {
                    PrefabOutlinerItem parent = null;
                    if (actorItem.IsBlongToPrefab(out parent))
                    {
                        bool prefabIsAlreadySelected = false;
                        for (int j = 0; j < selectedActorItems.Count; ++j)
                        {
                            if (selectedActorItems[j] == parent || selectedActorItems[j].IsChildOf(parent))
                            {
                                prefabIsAlreadySelected = true;
                                actorItem.IsSelected = true;
                                actorItem.TreeViewItemIsSelected = true;
                                if (actorItem.Parent != null)
                                    actorItem.Parent.IsExpanded = true;
                                mSelectedItemViews.Add(actorItem);
                                realSelectActors.Add(actor);
                            }
                        }
                        if (!prefabIsAlreadySelected)
                        {
                            parent.IsSelected = true;
                            SetSelected(parent.Prefab, true);
                            parent.TreeViewItemIsSelected = true;
                            mSelectedItemViews.Add(parent);
                            realSelectActors.Add(parent.Prefab);
                        }
                    }
                    else
                    {
                        actorItem.IsSelected = true;
                        if (actorItem is ActorOutlinerItem)
                            SetSelected((actorItem as ActorOutlinerItem).Actor, true);
                        if (actorItem is PrefabOutlinerItem)
                            SetSelected((actorItem as PrefabOutlinerItem).Prefab, true);
                        actorItem.TreeViewItemIsSelected = true;
                        if (actorItem.Parent != null)
                            actorItem.Parent.IsExpanded = true;
                        mSelectedItemViews.Add(actorItem);
                        realSelectActors.Add(actor);
                    }
                }
            }
            List<ViewPort.ViewPortControl.SelectActorData> actorDatas = new List<ViewPort.ViewPortControl.SelectActorData>();
            for (int i = 0; i < realSelectActors.Count; ++i)
            {
                var data = new ViewPort.ViewPortControl.SelectActorData();
                data.Actor = realSelectActors[i];
                data.StartTransMatrix = realSelectActors[i].Placement.WorldMatrix;
                actorDatas.Add(data);
            }
            if (realSelectActors.Count == 1)
            {
                var actorItem = GetOutlinerItemByActor(realSelectActors[0]);
                mScrollStack = new Stack<OutlinerItem>();
                GetTreeView_OutlinerParent(TreeView_Outliner, mScrollStack, actorItem);
                BringIntoView(TreeView_Outliner, mScrollStack);
            }
            ViewPort.SelectActorsWithoutNotify(actorDatas.ToArray());
            LinkedPropertyGrid.Instance = realSelectActors;
            ViewPort_PickingActror = false;
            mSelectedItemViews.CollectionChanged += SelectedItemViews_CollectionChanged;
        }

        void SetSelected(GActor actor, bool value)
        {
            actor.Selected = value;
            var children = actor.GetChildrenUnsafe();
            for (int i = 0; i < children.Count; ++i)
            {
                SetSelected(children[i], value);
            }
        }
        private void ViewPort_OnSelectAcotrs(object sender, ViewPort.ViewPortControl.SelectActorData[] e)
        {
            SelectAcotrs(e);
        }
        #endregion

        #region BingdingPrefab
        public PrefabOutlinerItem BindingPrefab(GActor prefab)
        {
            Items.Clear();
            ClearSelectedItemViews();
            return AddItemByPrefab(prefab);
        }
        public PrefabOutlinerItem AddItemByPrefab(GActor actor)
        {
            var item = new PrefabOutlinerItem(actor, this);
            //for (int i = 0; i < actor.GetChildrenUnsafe().Count; ++i)
            //{
            //    AddActorItemRecursion(item, actor.GetChildrenUnsafe()[i]);
            //}
            Items.Add(item);
            return item;
        }
        public void AddItemByActor(GActor actor)
        {
            var item = new ActorOutlinerItem(actor, this);
            for (int i = 0; i < actor.GetChildrenUnsafe().Count; ++i)
            {
                AddActorItemRecursion(item, actor.GetChildrenUnsafe()[i]);
            }
            Items.Add(item);
        }
        #endregion BingdingPrefab

        #region BindedWorld

        bool IsBlongToPrefab(GActor actor)
        {
            if (actor.Parent == null)
                return false;
            if (actor.Parent is GPrefab)
            {
                return true;
            }
            else
            {
                if (IsBlongToPrefab(actor.Parent))
                    return true;
                else
                    return false;

            }
        }

        public void AddItemBySceneGraph(GSceneGraph sceneGraph)
        {
            var sceneGraphItem = new SceneOutlinerItem(sceneGraph, this);
            sceneGraphItem.IsExpanded = true;
            Items.Add(sceneGraphItem);
            //先添加 iSceneNode，确保在所有actor之前
            for (int i = 0; i < sceneGraph.ChildrenNode.Count; ++i)
            {
                AddISceneNodeItemRecursion(sceneGraphItem, null, sceneGraph.ChildrenNode[i]);
            }
            for (int i = 0; i < sceneGraph.ChildrenNode.Count; ++i)
            {
                AddActorItemRecursion(sceneGraphItem, sceneGraph.ChildrenNode[i]);
            }
        }
        void AddISceneNodeItemRecursion(SceneOutlinerItem sceneGraphItem, ISceneNodeOutlinerItem parent, ISceneNode sceneNode)
        {
            var sceneNodeItem = new ISceneNodeOutlinerItem(sceneNode, this);
            if (parent == null)
                sceneGraphItem.AddItem(sceneNodeItem);
            else
                parent.AddItem(sceneNodeItem);
            for (int i = 0; i < sceneNode.ChildrenNode.Count; ++i)
            {
                AddISceneNodeItemRecursion(sceneGraphItem, sceneNodeItem, sceneNode.ChildrenNode[i]);
            }
        }
        bool IsChild(List<GActor> actors, GActor actor)
        {
            for (int i = 0; i < actors.Count; ++i)
            {
                if (actor.Parent == actors[i])
                {
                    return true;
                }
                else
                {
                    if (IsChild(actors[i].GetChildrenUnsafe(), actor))
                        return true;
                }
            }
            return false;
        }
        void AddActorItemRecursion(OutlinerItem parent, GActor actor)
        {
            OutlinerItem item = null;
            if (actor is GPrefab)
            {
                item = new PrefabOutlinerItem(actor, this);
                parent.AddItem(item);
            }
            else
            {
                if (!IsBlongToPrefab(actor))
                {
                    item = new ActorOutlinerItem(actor, this);
                    parent.AddItem(item);
                }
            }
        }
        void AddActorItemRecursion(SceneOutlinerItem sceneGraphItem, ISceneNode sceneNode)
        {
            GActor actor = null;
            List<GActor> actors = new List<GActor>();
            using (var it = sceneNode.Actors.GetEnumerator())
            {
                while (it.MoveNext())
                {
                    actor = it.Current.Value;

                    if (actors.Count == 0)
                        actors.Add(actor);
                    else
                    {
                        if (actor.Parent == null)
                        {
                            actors.Add(actor);
                        }
                        else
                        {
                            bool isChild = IsChild(actors, actor);
                            if (!isChild)
                            {
                                actors.Add(actor);
                            }
                        }

                    }
                }
            }
            for (int i = 0; i < actors.Count; ++i)
            {
                if (!sceneGraphItem.IsContainActorItem(sceneGraphItem.ChildList, actors[i]) && actors[i].Parent == null)
                    AddActorItemRecursion(sceneGraphItem, actors[i]);
            }

            for (int i = 0; i < sceneNode.ChildrenNode.Count; ++i)
            {
                AddActorItemRecursion(sceneGraphItem, sceneNode.ChildrenNode[i]);
            }
        }

        GWorld mBindedWorld = null;
        public void BindingWorld(GWorld world)
        {
            Items.Clear();
            ClearSelectedItemViews();
            mBindedWorld = world;
            InitializeOutliner(world);
            mBindedWorld.OnAddActor += BindedWorld_OnAddActor;
            mBindedWorld.OnRemoveActor += BindedWorld_OnRemoveActor;
        }
        void InitializeOutliner(GWorld world)
        {
            using (var it = world.GetSceneEnumerator())
            {
                while (it.MoveNext())
                {
                    var scene = it.Current.Value;
                    AddItemBySceneGraph(scene);
                }
            }
        }
        private void BindedWorld_OnAddActor(GActor actor)
        {

        }
        private void BindedWorld_OnRemoveActor(GActor actor)
        {

        }
        #endregion BindedWorld

        #region Edit
        List<OutlinerItem> GetAllOutlinerItems()
        {
            List<OutlinerItem> list = new List<OutlinerItem>();
            for (int i = 0; i < Items.Count; ++i)
            {
                FillItemList(Items[i], list);
            }
            return list;
        }
        void FillItemList(OutlinerItem item, List<OutlinerItem> list)
        {
            list.Add(item);
            for (int i = 0; i < item.ChildList.Count; ++i)
            {
                FillItemList(item.ChildList[i], list);
            }
        }
        public string GeneratorValidNameInEditor(string origionName)
        {
            if (string.IsNullOrEmpty(origionName))
                return origionName;
            int index = 1;
            bool isSameName = false;
            var actors = GetAllOutlinerItems();
            actors.Sort((OutlinerItem a, OutlinerItem b) =>
            {
                if (a.Name == null && b.Name == null)
                    return 0;
                if (a.Name != null && b.Name == null)
                    return 1;
                if (a.Name == null && b.Name != null)
                    return -1;
                return a.Name.CompareTo(b.Name);
            });
            foreach (var actor in actors)
            {
                var spName = actor.Name;
                if (string.IsNullOrEmpty(spName))
                    continue;
                if (!spName.Contains(origionName))
                    continue;
                isSameName = true;
                var idx1 = spName.LastIndexOf('(');
                if (idx1 < 0)
                {
                    continue;
                }
                else
                {
                    try
                    {
                        origionName = spName.Substring(0, idx1);
                        var idx2 = spName.LastIndexOf(')');
                        int idx = -1;
                        if (int.TryParse(spName.Substring(idx1 + 1, idx2 - idx1 - 1), out idx))
                        {
                            if (index <= idx)
                                index = idx + 1;
                        }
                    }
                    catch (System.Exception)
                    {
                    }
                }
            }
            if (!isSameName)
                return origionName;
            else
                return origionName + "(" + (index) + ")";
        }
        public List<OutlinerItem> CutClipsBoard = new List<OutlinerItem>();
        public List<OutlinerItem> CopyClipsBoard = new List<OutlinerItem>();
        public bool CheckCanPaste(OutlinerItem item)
        {
            if (mIsCut)
            {
                return (CheckCanPasteTo(item) && CheckPasteCircle(item));
            }
            else
            {
                return CheckCanPasteTo(item);
            }
        }
        public bool CheckCanMakeGroup()
        {
            for (int i = 0; i < SelectedItemViews.Count; ++i)
            {
                if (SelectedItemViews[i] is ISceneNodeOutlinerItem || SelectedItemViews[i] is SceneOutlinerItem || SelectedItemViews[i] is WorldOutlinerItem)
                {
                    return false;
                }
            }
            return true;
        }
        bool CheckCanPasteTo(OutlinerItem item)
        {
            if (item is ActorOutlinerItem || item is PrefabOutlinerItem || item is SceneOutlinerItem)
            {
                List<OutlinerItem> tempList = null;
                if (mIsCut)
                {
                    tempList = CutClipsBoard;
                }
                else
                {
                    tempList = CopyClipsBoard;
                }
                if (tempList.Count == 0)
                    return false;
                for (int i = 0; i < tempList.Count; ++i)
                {
                    if (!(tempList[i] is ActorOutlinerItem) && !(tempList[i] is PrefabOutlinerItem))
                    {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }
        bool mIsCut = false;
        public void Cut(OutlinerItem sender)
        {
            CutClipsBoard.Clear();
            CutClipsBoard.AddRange(SelectedItemViews);
            mIsCut = true;
        }
        public void Copy(OutlinerItem sender)
        {
            CopyClipsBoard.Clear();
            Action action = async () =>
             {
                 for (int i = 0; i < SelectedItemViews.Count; ++i)
                 {
                     CopyClipsBoard.Add(await SelectedItemViews[i].Clone(EngineNS.CEngine.Instance.RenderContext));
                 }
             };
            action.Invoke();
            mIsCut = false;
        }
        public void Paste(OutlinerItem sender)
        {
            PasteCommand(sender);
        }
        public void Duplicate(OutlinerItem sender)
        {
            DuplicateCommand();
        }
        public void Delete(OutlinerItem sender)
        {
            DeleteCommand();
        }
        public void MakeGroup(OutlinerItem sender)
        {
            MakeGroupCommand(sender);
        }
        public void CreateEmptyActor(OutlinerItem sender)
        {
            CreateEmptyActorCommand(sender);
        }
        #region Command
        public class OutlinerInsertChildUndoRedoItem : OutlinerUndoRedoItem
        {
            public int Index { get; set; }
            public OutlinerInsertChildUndoRedoItem(int index, OutlinerItem lastParent, OutlinerItem item) : base(lastParent, item)
            {
                Index = index;
                LastParent = lastParent;
                Item = item;
            }
        }
        public void CreateEmptyActorCommand(OutlinerItem sender)
        {
            var actor = new GActor();
            actor.ActorId = Guid.NewGuid();
            actor.Placement = new EngineNS.GamePlay.Component.GPlacementComponent();
            actor.SpecialName = "EmptyActor";

            sender.Add(new ActorOutlinerItem(actor, this));
            sender.IsExpanded = true;
        }
        public void FocusCommand(OutlinerItem sender)
        {
            if (SelectedItemViews.Count == 0)
                return;
            var actors = new List<GActor>();
            for (int i = 0; i < SelectedItemViews.Count; ++i)
            {
                if (SelectedItemViews[i] is ActorOutlinerItem)
                {
                    actors.Add((SelectedItemViews[i] as ActorOutlinerItem).Actor);
                }
                if (SelectedItemViews[i] is PrefabOutlinerItem)
                {
                    actors.Add((SelectedItemViews[i] as PrefabOutlinerItem).Prefab);
                }
            }
            ViewPort.FocusShow(actors);
        }
        bool CheckPasteCircle(List<OutlinerItem> items, OutlinerItem pasteTarget)
        {
            for (int i = 0; i < items.Count; ++i)
            {
                if (items[i] == pasteTarget)
                    return true;
                if (pasteTarget.IsChildOf(items[i]))
                    return true;
            }
            return false;
        }
        bool CheckPasteCircle(OutlinerItem pasteTarget)
        {
            List<OutlinerItem> items = null;
            if (mIsCut)
                items = CutClipsBoard;
            else
                items = CopyClipsBoard;
            for (int i = 0; i < items.Count; ++i)
            {
                if (items[i] == pasteTarget)
                    return true;
                if (pasteTarget.IsChildOf(items[i]))
                    return true;
            }
            return false;
        }
        public async void PasteCommand(OutlinerItem sender)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            if (SelectedItemViews.Count == 0)
                return;
            OutlinerItem pasteTarget;
            if (SelectedItemViews[SelectedItemViews.Count - 1].Parent == null)
                pasteTarget = SelectedItemViews[SelectedItemViews.Count - 1];
            else
                pasteTarget = SelectedItemViews[SelectedItemViews.Count - 1].Parent;
            if (!CheckCanPasteTo(pasteTarget))
                return;
            if (mIsCut)
            {
                if (CheckPasteCircle(pasteTarget))
                    return;
                List<OutlinerUndoRedoItem> addRemoveItems = new List<OutlinerUndoRedoItem>();
                for (int i = 0; i < CutClipsBoard.Count; ++i)
                {
                    if (CutClipsBoard[i].Parent != null)
                    {
                        addRemoveItems.Add(new OutlinerUndoRedoItem(CutClipsBoard[i].Parent, CutClipsBoard[i]));
                    }
                    else
                    {
                        System.Diagnostics.Debug.Assert(false);
                    }
                }
                var redo = new Action<object>((obj) =>
                {
                    for (int i = 0; i < addRemoveItems.Count; ++i)
                    {
                        if (addRemoveItems[i].LastParent != null)
                        {
                            addRemoveItems[i].LastParent.Remove(addRemoveItems[i].Item);
                            pasteTarget.Add(addRemoveItems[i].Item);
                            pasteTarget.IsExpanded = true;
                        }
                    }
                });
                redo.Invoke(null);
                UndoRedo.UndoRedoManager.Instance.AddCommand(UndoRedoKey, null, redo, null, (obj) =>
                {
                    for (int i = 0; i < addRemoveItems.Count; ++i)
                    {
                        var pair = addRemoveItems[i];
                        pair.Item.Parent.Remove(pair.Item);
                        pair.LastParent.Add(pair.Item);
                        pair.LastParent.IsExpanded = true;
                    }

                }, $"剪切粘贴{addRemoveItems.Count}个对象");
            }
            else
            {
                var items = new List<OutlinerItem>();
                for (int i = 0; i < CopyClipsBoard.Count; ++i)
                {
                    var item = await CopyClipsBoard[i].Clone(EngineNS.CEngine.Instance.RenderContext);
                    items.Add(item);
                }
                Action<Object> redo = (obj) =>
                {
                    for (int i = 0; i < items.Count; ++i)
                    {
                        pasteTarget.Add(items[i]);
                        pasteTarget.IsExpanded = true;
                    }
                };
                redo.Invoke(null);
                UndoRedo.UndoRedoManager.Instance.AddCommand(UndoRedoKey, null, redo, null, (obj) =>
                {
                    for (int i = 0; i < items.Count; ++i)
                    {
                        var pair = items[i];
                        pair.Parent.Remove(pair);
                    }

                }, $"复制粘贴{items.Count}个对象");
            }
        }
        public async void DuplicateCommand()
        {
            var actors = new List<OutlinerUndoRedoItem>();
            for (int i = 0; i < SelectedItemViews.Count; ++i)
            {
                if (SelectedItemViews[i].Parent != null)
                {
                    var actor = await SelectedItemViews[i].Clone(EngineNS.CEngine.Instance.RenderContext);
                    actors.Add(new OutlinerUndoRedoItem(SelectedItemViews[i].Parent, actor));
                }
            }
            Action<Object> redo = (obj) =>
            {
                for (int i = 0; i < actors.Count; ++i)
                {
                    actors[i].LastParent.Add(actors[i].Item);
                }
            };
            redo.Invoke(null);
            UndoRedo.UndoRedoManager.Instance.AddCommand(UndoRedoKey, null, redo, null, (obj) =>
            {
                for (int i = 0; i < actors.Count; ++i)
                {
                    var pair = actors[i];
                    pair.LastParent.Remove(pair.Item);
                }

            }, $"克隆{actors.Count}个对象");
        }
        public void DeleteCommand()
        {
            List<OutlinerUndoRedoItem> addRemoveItems = new List<OutlinerUndoRedoItem>();
            for (int i = 0; i < SelectedItemViews.Count; ++i)
            {
                if (SelectedItemViews[i].Parent == null)
                    continue;
                addRemoveItems.Add(new OutlinerUndoRedoItem(SelectedItemViews[i].Parent, SelectedItemViews[i]));
            }
            ClearSelectedItemViews();
            var redo = new Action<object>((obj) =>
            {
                for (int i = 0; i < addRemoveItems.Count; ++i)
                {
                    var pair = addRemoveItems[i];
                    pair.LastParent.Remove(pair.Item);
                }
            });
            redo.Invoke(null);
            UndoRedo.UndoRedoManager.Instance.AddCommand(UndoRedoKey, null, redo, null, (obj) =>
            {
                for (int i = 0; i < addRemoveItems.Count; ++i)
                {
                    var pair = addRemoveItems[i];
                    pair.LastParent.Add(pair.Item);
                }

            }, $"删除{addRemoveItems.Count}个对象");
        }
        public void MakeGroupCommand(OutlinerItem sender)
        {
            List<OutlinerUndoRedoItem> makeGroupItems = new List<OutlinerUndoRedoItem>();
            List<GActor> selectedActors = new List<GActor>();
            for (int i = 0; i < SelectedItemViews.Count; ++i)
            {
                if (SelectedItemViews[i].Parent == null)
                    continue;
                makeGroupItems.Add(new OutlinerUndoRedoItem(SelectedItemViews[i].Parent, SelectedItemViews[i]));
                if (SelectedItemViews[i] is ActorOutlinerItem)
                {
                    selectedActors.Add((SelectedItemViews[i] as ActorOutlinerItem).Actor);
                }
                if (SelectedItemViews[i] is PrefabOutlinerItem)
                {
                    selectedActors.Add((SelectedItemViews[i] as PrefabOutlinerItem).Prefab);
                }
            }
            EngineNS.Vector3 centerPos = EngineNS.Vector3.Zero;
            foreach (var data in selectedActors)
            {
                centerPos += data.Placement.WorldLocation;
            }
            centerPos = centerPos / selectedActors.Count;

            var actor = new GActor();
            actor.ActorId = Guid.NewGuid();
            actor.SpecialName = "Actor";
            var placement = new EngineNS.GamePlay.Component.GPlacementComponent();
            placement.Location = centerPos;
            actor.Placement = placement;

            var actorItem = new ActorOutlinerItem(actor, this);
            actorItem.TreeViewItemIsSelected = true;
            actorItem.IsSelected = true;
            actorItem.IsExpanded = true;
            var redo = new Action<object>((obj) =>
            {
                Items[0].Add(actorItem);
                for (int i = 0; i < makeGroupItems.Count; ++i)
                {
                    makeGroupItems[i].Item.TreeViewItemIsSelected = false;
                    makeGroupItems[i].Item.IsSelected = false;
                    makeGroupItems[i].LastParent.Remove(makeGroupItems[i].Item);
                    actorItem.Add(makeGroupItems[i].Item);
                    if (makeGroupItems[i].Item is ActorOutlinerItem)
                    {
                        (makeGroupItems[i].Item as ActorOutlinerItem).Actor.Placement.Location -= centerPos;
                    }
                    if (makeGroupItems[i].Item is PrefabOutlinerItem)
                    {
                        (makeGroupItems[i].Item as PrefabOutlinerItem).Prefab.Placement.Location -= centerPos;
                    }
                }
                mScrollStack = new Stack<OutlinerItem>();
                GetTreeView_OutlinerParent(TreeView_Outliner, mScrollStack, actorItem);
                BringIntoView(TreeView_Outliner, mScrollStack);
            });
            redo.Invoke(null);
            UndoRedo.UndoRedoManager.Instance.AddCommand(UndoRedoKey, null, redo, null, (obj) =>
            {
                for (int i = 0; i < makeGroupItems.Count; ++i)
                {
                    var pair = makeGroupItems[i];
                    actorItem.Remove(pair.Item);
                    pair.LastParent.Add(pair.Item);
                    if (makeGroupItems[i].Item is ActorOutlinerItem)
                    {
                        (makeGroupItems[i].Item as ActorOutlinerItem).Actor.Placement.Location += centerPos;
                    }
                    if (makeGroupItems[i].Item is PrefabOutlinerItem)
                    {
                        (makeGroupItems[i].Item as PrefabOutlinerItem).Prefab.Placement.Location += centerPos;
                    }
                    Items[0].Remove(actorItem);
                }

            }, $"打组{makeGroupItems.Count}个对象");
        }
        #endregion Command
        private void OutlinerItem_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            mOutlinerTreeViewOperation.SelectedOutlinerItems = mSelectedItemViews.ToList();

            var treeViewItem = mOutlinerTreeViewOperation.VisualUpwardSearch<TreeViewItem>(e.OriginalSource as DependencyObject) as TreeViewItem;
            if (treeViewItem != null)
            {
                ContextMenu menu = null;
                mOutlinerTreeViewOperation.SelectedTreeViewItem = treeViewItem;
                mOutlinerTreeViewOperation.SelectedOutlinerItem = treeViewItem.Header as OutlinerItem;
                //if (mSelectedItemViews.Count == 1)
                {
                    menu = mOutlinerTreeViewOperation.CreateContextMenu(TryFindResource(new ComponentResourceKey(typeof(ResourceLibrary.CustomResources), "ContextMenu_Default")) as System.Windows.Style);
                    mOutlinerTreeViewOperation.SelectedOutlinerItem.GenerateContexMenu(menu);
                    OutlinerTreeViewOperation.CreateSeparator(menu);
                    mOutlinerTreeViewOperation.CreateSelectMenu(menu);
                }
                //else
                //{
                //    mOutlinerTreeViewOperation.CreateMutiSelectContexMenu(menu);
                //}
                mOutlinerTreeViewOperation.CreateExpandCollapseMenu(menu, treeViewItem.IsExpanded);
                treeViewItem.ContextMenu = menu;
                menu.Visibility = Visibility.Visible;
                menu.PlacementTarget = treeViewItem;
                menu.IsOpen = true;
                e.Handled = true;
            }

        }


        private void TreeView_Outliner_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.F:
                    {
                        FocusCommand(null);
                    }
                    break;
                case Key.Delete:
                    {
                        DeleteCommand();
                    }
                    break;
                case Key.C:
                    {
                        if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                        {
                            Copy(null);
                        }
                        e.Handled = true;
                    }
                    break;
                case Key.X:
                    {
                        if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                        {
                            Cut(null);
                        }
                        e.Handled = true;
                    }
                    break;
                case Key.V:
                    {
                        if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                        {
                            PasteCommand(null);
                        }
                        e.Handled = true;
                    }
                    break;
                case Key.D:
                    {
                        if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                        {
                            DuplicateCommand();
                        }
                        e.Handled = true;
                    }
                    break;
                case Key.Z:
                    {
                        if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                        {
                            UndoRedo.UndoRedoManager.Instance.Undo(UndoRedoKey);
                        }
                        e.Handled = true;
                    }
                    break;
                case Key.Y:
                    {
                        if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                        {
                            UndoRedo.UndoRedoManager.Instance.Redo(UndoRedoKey);
                        }
                        e.Handled = true;
                    }
                    break;
            }

        }
        #endregion Edit

        #region 控件拖动
        Point mMouseDownPos = new Point();
        FrameworkElement mMouseDownElement;
        private void TreeViewItem_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var grid = sender as FrameworkElement;
            var listItem = grid.DataContext as OutlinerItem;
            if (listItem == null)
                return;

            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                mMouseDownElement = grid;
                mMouseDownPos = e.GetPosition(grid);
            }
        }
        private void TreeViewItem_MouseMove(object sender, MouseEventArgs e)
        {
            var grid = sender as FrameworkElement;

            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed && grid == mMouseDownElement)
            {
                var pos = e.GetPosition(grid);
                if ((System.Math.Abs(pos.X - mMouseDownPos.X) > 3) ||
                   (System.Math.Abs(pos.Y - mMouseDownPos.Y) > 3))
                {
                    EditorCommon.DragDrop.IDragAbleObject[] array = mSelectedItemViews.ToArray();
                    bool allActor = true;
                    for (int i = 0; i < mSelectedItemViews.Count; ++i)
                    {
                        if (!(mSelectedItemViews[i] is ActorOutlinerItem) && !(mSelectedItemViews[i] is PrefabOutlinerItem))
                        {
                            allActor = false;
                        }
                    }
                    if (allActor)
                        EditorCommon.DragDrop.DragDropManager.Instance.StartDrag(EditorCommon.Controls.ResourceBrowser.ContentControl.SceneNodeDragType, array);
                    else
                        EditorCommon.DragDrop.DragDropManager.Instance.StartDrag("OutlinerControl_Not_Create_Prefab", array);
                }
            }
        }
        private void TreeViewItem_MouseUp(object sender, MouseButtonEventArgs e)
        {
            mMouseDownElement = null;
        }
        private void TreeViewItem_MouseEnter(object sender, MouseEventArgs e)
        {

        }

        private void TreeViewItem_MouseLeave(object sender, MouseEventArgs e)
        {
            var element = sender as FrameworkElement;
            if (element == null)
                return;
            var item = element.DataContext as OutlinerItem;
            if (item == null)
                return;
            item.UpInsertLineVisible = Visibility.Hidden;
            item.DownInsertLineVisible = Visibility.Hidden;
            item.ChildInsertLineVisible = Visibility.Hidden;
        }
        void ExpandAllTreeViewParent(OutlinerItem view)
        {
            if (view == null)
                return;
            var viewStack = new Stack<OutlinerItem>();
            viewStack.Push(view);
            var viewParent = view.Parent;
            while (viewParent != null)
            {
                viewStack.Push(viewParent);
                viewParent = viewParent.Parent;
            }

            ItemsControl item = TreeView_Outliner;
            while (viewStack.Count > 0)
            {
                var ctrl = viewStack.Pop();
                item = FindTreeViewItem(item, ctrl);
            }
            if (item != null)
                item.BringIntoView();
        }
        private TreeViewItem FindTreeViewItem(ItemsControl item, object data)
        {
            if (item == null)
                return null;
            TreeViewItem findItem = null;
            bool itemIsExpand = false;
            if (item is TreeViewItem)
            {
                TreeViewItem tviCurrent = item as TreeViewItem;
                itemIsExpand = tviCurrent.IsExpanded;
                if (!tviCurrent.IsExpanded)
                {
                    //如果这个TreeViewItem未展开过，则不能通过ItemContainerGenerator来获得TreeViewItem
                    tviCurrent.SetValue(TreeViewItem.IsExpandedProperty, true);
                    //必须使用UpdaeLayour才能获取到TreeViewItem
                    tviCurrent.UpdateLayout();
                }
            }
            // 优先查找同级对象
            for (int i = 0; i < item.Items.Count; i++)
            {
                TreeViewItem tvItem = (TreeViewItem)item.ItemContainerGenerator.ContainerFromIndex(i);
                if (tvItem == null)
                    continue;
                object itemData = item.Items[i];
                if (itemData == data)
                {
                    findItem = tvItem;
                    break;
                }
            }
            if (findItem == null)
            {
                for (int i = 0; i < item.Items.Count; i++)
                {
                    TreeViewItem tvItem = (TreeViewItem)item.ItemContainerGenerator.ContainerFromIndex(i);
                    if (tvItem == null)
                        continue;
                    if (tvItem.Items.Count > 0)
                    {
                        findItem = FindTreeViewItem(tvItem, data);
                        if (findItem != null)
                            break;
                    }
                }
            }
            if (findItem == null)
            {
                TreeViewItem tviCurrent = item as TreeViewItem;
                if (tviCurrent != null)
                {
                    tviCurrent.SetValue(TreeViewItem.IsExpandedProperty, itemIsExpand);
                    tviCurrent.UpdateLayout();
                }
            }
            return findItem;
        }


        #region Select
        ObservableCollection<OutlinerItem> mSelectedItemViews = new ObservableCollection<OutlinerItem>();
        public ObservableCollection<OutlinerItem> SelectedItemViews
        {
            get => mSelectedItemViews;
            set
            {
                mSelectedItemViews = value;
                OnPropertyChanged("SelectedItemViews");
            }
        }
        public void RemoveFromSelectedViews(OutlinerItem item)
        {
            if (mSelectedItemViews.Contains(item))
            {
                mSelectedItemViews.CollectionChanged -= SelectedItemViews_CollectionChanged;
                mSelectedItemViews.Remove(item);
                mSelectedItemViews.CollectionChanged += SelectedItemViews_CollectionChanged;
            }
        }
        void ClearSelectedItemViews()
        {
            mSelectedItemViews.CollectionChanged -= SelectedItemViews_CollectionChanged;
            mSelectedItemViews.Clear();
            mSelectedItemViews.CollectionChanged += SelectedItemViews_CollectionChanged;
        }
        //bool mBroadcastSelectedOperation = true;
        public delegate void SelectedActorsChanged(object sender, List<EngineNS.GamePlay.Actor.GActor> actors);
        public event SelectedActorsChanged OnSelectedActorsChanged;
        private void SelectedItemViews_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            for (int j = 0; j < Items.Count; ++j)
            {
                UnContouredAll(Items[j]);
            }
            LinkedPropertyGrid.Instance = null;
            List<GActor> actors = new List<GActor>();
            if (mSelectedItemViews.Count == 0)
            {
                if (!ViewPort_PickingActror)
                    ViewPort.SelectActorWithoutNotify(null);
            }
            if (mSelectedItemViews.Count == 1)
            {
                LinkedPropertyGrid.Instance = mSelectedItemViews[0].GetShowPropertyObject();
                var actorItem = mSelectedItemViews[0] as ActorOutlinerItem;

                if (actorItem != null)
                {
                    if (!ViewPort_PickingActror)
                    {
                        ViewPort.SelectActorWithoutNotify(actorItem.Actor);
                        SetSelected(actorItem.Actor, true);
                        actors.Add(actorItem.Actor);
                        OnSelectedActorsChanged?.Invoke(this, actors);
                    }
                }
                var prefabItem = mSelectedItemViews[0] as PrefabOutlinerItem;

                if (prefabItem != null)
                {
                    if (!ViewPort_PickingActror)
                    {
                        ViewPort.SelectActorWithoutNotify(prefabItem.Prefab);
                        SetSelected(prefabItem.Prefab, true);
                        actors.Add(prefabItem.Prefab);
                        OnSelectedActorsChanged?.Invoke(this, actors);
                    }
                }
            }
            else
            {
                List<object> properties = new List<object>();
                for (int i = 0; i < mSelectedItemViews.Count; i++)
                {
                    properties.Add(mSelectedItemViews[i].GetShowPropertyObject());
                }
                LinkedPropertyGrid.Instance = properties;
                if (!ViewPort_PickingActror)
                {
                    List<ViewPort.ViewPortControl.SelectActorData> actorDatas = new List<ViewPort.ViewPortControl.SelectActorData>();
                    for (int i = 0; i < mSelectedItemViews.Count; i++)
                    {
                        var actorItem = mSelectedItemViews[i] as ActorOutlinerItem;
                        if (actorItem != null)
                        {
                            var actor = actorItem.Actor;
                            var data = new EditorCommon.ViewPort.ViewPortControl.SelectActorData()
                            {
                                Actor = actor,
                                StartTransMatrix = actor.Placement.WorldMatrix,
                            };
                            actorDatas.Add(data);
                        }
                        var prefabItem = mSelectedItemViews[i] as PrefabOutlinerItem;
                        if (prefabItem != null)
                        {
                            var actor = prefabItem.Prefab;
                            var data = new EditorCommon.ViewPort.ViewPortControl.SelectActorData()
                            {
                                Actor = actor,
                                StartTransMatrix = actor.Placement.WorldMatrix,
                            };
                            actorDatas.Add(data);
                        }
                    }
                    if (actorDatas.Count > 0)
                    {
                        ViewPort.SelectActorsWithoutNotify(actorDatas.ToArray());
                        for (int i = 0; i < actorDatas.Count; ++i)
                        {
                            actors.Add(actorDatas[i].Actor);
                            SetSelected(actorDatas[i].Actor, true);
                        }
                        OnSelectedActorsChanged?.Invoke(this, actors);
                    }
                }
            }


        }

        private void SelectControl(OutlinerItem view)
        {
            if (view == null)
                return;

            view.IsSelected = true;
        }
        private void UnSelectControl(OutlinerItem view, bool withChild = false)
        {
            if (view == null)
                return;

            view.IsSelected = false;

            if (withChild)
            {
                foreach (var ctrl in view.ChildList)
                {
                    UnSelectControl(ctrl, withChild);
                }
            }
        }
        class SelectItemIndexData
        {
            public OutlinerItem Control;
            public int[] TotalIndex;  // 保证在拖动时选中对象的顺序不变
        }
        List<SelectItemIndexData> mSelectItemIndexDatas = new List<SelectItemIndexData>();
        public void UpdateSelectItems(ObservableCollection<OutlinerItem> selectedItems, ObservableCollection<OutlinerItem> unSelectedItems)
        {
            if (unSelectedItems != null)
            {
                foreach (var ctrl in unSelectedItems)
                {
                    UnSelectControl(ctrl);
                    foreach (var data in mSelectItemIndexDatas)
                    {
                        if (data.Control == ctrl)
                        {
                            mSelectItemIndexDatas.Remove(data);
                            break;
                        }
                    }
                }
            }
            if (selectedItems != null)
            {
                foreach (var ctrl in selectedItems)
                {
                    ExpandAllTreeViewParent(ctrl);

                    SelectControl(ctrl);
                    var data = new SelectItemIndexData()
                    {
                        Control = ctrl,
                        TotalIndex = GetItemViewIndex(ctrl)
                    };
                    mSelectItemIndexDatas.Add(data);
                }

                mSelectItemIndexDatas.Sort(CompareSelectItemByIndex);
            }
        }
        private static int CompareSelectItemByIndex(SelectItemIndexData data1, SelectItemIndexData data2)
        {
            var count = System.Math.Min(data1.TotalIndex.Length, data2.TotalIndex.Length);
            for (int i = 0; i < count; i++)
            {
                if (data1.TotalIndex[i] < data2.TotalIndex[i])
                    return -1;
                else if (data1.TotalIndex[i] > data2.TotalIndex[i])
                    return 1;
                else
                    continue;
            }
            if (data1.TotalIndex.Length < data2.TotalIndex.Length)
                return -1;
            else if (data1.TotalIndex.Length > data2.TotalIndex.Length)
                return 1;
            return 0;
        }
        int[] GetItemViewIndex(OutlinerItem view)
        {
            if (view == null)
                return null;
            var idx = Items.IndexOf(view);
            if (idx >= 0)
                return new int[] { idx };
            var rootParent = view;
            int deepCount = 1;
            while (rootParent != null)
            {
                if (rootParent.Parent == null)
                    break;
                rootParent = rootParent.Parent;
                deepCount++;
            }

            var idxArray = new int[deepCount];
            idxArray[0] = Items.IndexOf(rootParent);
            GetTotalChildIndex(rootParent, view, ref idxArray, 1);
            return idxArray;
        }
        bool GetTotalChildIndex(OutlinerItem parent, OutlinerItem ctrl, ref int[] indexArray, int deep)
        {
            int idx = 0;
            foreach (var child in parent.ChildList)
            {
                indexArray[deep] = idx;
                idx++;
                if (child == ctrl)
                    return true;
                else if (child != null)
                {
                    if (GetTotalChildIndex(child, ctrl, ref indexArray, deep + 1))
                        return true;
                }
            }

            return false;
        }

        #endregion

        bool CheckIsDragContainsView(OutlinerItem view)
        {
            foreach (var item in EditorCommon.DragDrop.DragDropManager.Instance.DragedObjectList)
            {
                if (item == view)
                    return true;
            }
            return false;
        }
        bool CheckParentInDraggedItems(OutlinerItem view)
        {
            if (view == null)
                return false;
            if (CheckIsDragContainsView(view.Parent))
                return true;
            return CheckParentInDraggedItems(view.Parent);
        }
        string GetDraggedItemName(EditorCommon.DragDrop.IDragAbleObject obj)
        {
            var view = obj as OutlinerItem;
            if (view != null)
            {
                return view.Name;
            }
            return "";
        }

        Brush mErrorInfoBrush = new SolidColorBrush(Color.FromRgb(255, 113, 113));
        private void Rectangle_InsertChild_DragEnter(object sender, DragEventArgs e)
        {
            var element = sender as FrameworkElement;
            var view = element.DataContext as OutlinerItem;
            view.ChildInsertLineVisible = Visibility.Visible;
            if (CheckIsDragContainsView(view))
            {
                EditorCommon.DragDrop.DragDropManager.Instance.InfoString = $"拖动的控件中包含{view.Name}";
                EditorCommon.DragDrop.DragDropManager.Instance.InfoStringBrush = mErrorInfoBrush;
                mDropType = enDropType.Invalid;
                return;
            }
            if (CheckParentInDraggedItems(view))
            {
                EditorCommon.DragDrop.DragDropManager.Instance.InfoString = $"无法将父放入子{view.Parent.Name}中";
                EditorCommon.DragDrop.DragDropManager.Instance.InfoStringBrush = mErrorInfoBrush;
                mDropType = enDropType.Invalid;
                return;
            }
            if (!view.CheckDrag())
            {
                EditorCommon.DragDrop.DragDropManager.Instance.InfoStringBrush = mErrorInfoBrush;
                mDropType = enDropType.Invalid;
                return;
            }
            if (!view.CanHaveChild)
            {
                EditorCommon.DragDrop.DragDropManager.Instance.InfoString = $"{view.Name}不允许添加子节点";
                EditorCommon.DragDrop.DragDropManager.Instance.InfoStringBrush = mErrorInfoBrush;
                mDropType = enDropType.Invalid;
                return;
            }
            string preStr = "";
            if (EditorCommon.DragDrop.DragDropManager.Instance.DragedObjectList.Count > 1)
            {
                preStr = EditorCommon.DragDrop.DragDropManager.Instance.DragedObjectList.Count + "个控件";
                //if (view.LinkedUIElement is EngineNS.UISystem.Controls.Containers.Border)
                //{
                //    EditorCommon.DragDrop.DragDropManager.Instance.InfoString = $"{view.Name}无法放入多个对象";
                //    EditorCommon.DragDrop.DragDropManager.Instance.InfoStringBrush = mErrorInfoBrush;
                //    mDropType = enDropType.Invalid;
                //}
                //else
                {
                    e.Effects = DragDropEffects.Move;
                    EditorCommon.DragDrop.DragDropManager.Instance.InfoString = $"将{preStr}放入{view.Name}之内";
                    mDropType = enDropType.AddChild;
                }
            }
            else
            {
                e.Effects = DragDropEffects.Move;
                preStr = GetDraggedItemName(EditorCommon.DragDrop.DragDropManager.Instance.DragedObjectList[0]);
                //if (view.LinkedUIElement is EngineNS.UISystem.Controls.Containers.Border)
                //{
                //    var cc = view.LinkedUIElement as EngineNS.UISystem.Controls.Containers.Border;
                //    if (cc.Content != null)
                //    {
                //        ComponentTreeViewItem contentView;
                //        if (mChildDic.TryGetValue(cc.Content, out contentView))
                //            EditorCommon.DragDrop.DragDropManager.Instance.InfoString = $"使用{preStr}替换{view.Name}中的对象{contentView.Name}";
                //        else
                //            EditorCommon.DragDrop.DragDropManager.Instance.InfoString = $"使用{preStr}替换{view.Name}中的对象";
                //        mDropType = enDropType.ReplceContent;
                //    }
                //    else
                //    {
                //        EditorCommon.DragDrop.DragDropManager.Instance.InfoString = $"将{preStr}放入{view.Name}之内";
                //        mDropType = enDropType.SetContent;
                //    }
                //}
                //else
                {
                    EditorCommon.DragDrop.DragDropManager.Instance.InfoString = $"将{preStr}放入{view.Name}之内";
                    mDropType = enDropType.AddChild;
                }
            }
            e.Handled = true;
        }

        private void Rectangle_InsertChild_DragLeave(object sender, DragEventArgs e)
        {
            var element = sender as FrameworkElement;
            var view = element.DataContext as OutlinerItem;
            view.ChildInsertLineVisible = Visibility.Hidden;
            mDropType = enDropType.None;
            e.Handled = true;
        }

        private void Path_InsertUp_DragEnter(object sender, DragEventArgs e)
        {
            var element = sender as FrameworkElement;
            var view = element.DataContext as OutlinerItem;
            if (view.Parent == null)
                return;

            view.UpInsertLineVisible = Visibility.Visible;
            if (CheckIsDragContainsView(view))
            {
                EditorCommon.DragDrop.DragDropManager.Instance.InfoString = $"无法插入自己之前";
                EditorCommon.DragDrop.DragDropManager.Instance.InfoStringBrush = mErrorInfoBrush;
                mDropType = enDropType.Invalid;
                return;
            }
            if (CheckParentInDraggedItems(view))
            {
                EditorCommon.DragDrop.DragDropManager.Instance.InfoString = $"无法将父放入子{view.Parent.Name}中";
                EditorCommon.DragDrop.DragDropManager.Instance.InfoStringBrush = mErrorInfoBrush;
                mDropType = enDropType.Invalid;
                return;
            }
            if (!view.CheckDrag())
            {
                EditorCommon.DragDrop.DragDropManager.Instance.InfoStringBrush = mErrorInfoBrush;
                mDropType = enDropType.Invalid;
                return;
            }
            if (!view.CanHaveChild)
            {
                EditorCommon.DragDrop.DragDropManager.Instance.InfoString = $"{view.Parent.Name}不允许添加子节点";
                EditorCommon.DragDrop.DragDropManager.Instance.InfoStringBrush = mErrorInfoBrush;
                mDropType = enDropType.Invalid;
                return;
            }
            //if (view.Parent.LinkedUIElement is EngineNS.UISystem.Controls.Containers.Border)
            //{
            //    e.Effects = DragDropEffects.None;
            //    EditorCommon.DragDrop.DragDropManager.Instance.InfoString = $"{view.Parent.Name}无法放入多个对象";
            //    EditorCommon.DragDrop.DragDropManager.Instance.InfoStringBrush = mErrorInfoBrush;
            //    mDropType = enDropType.Invalid;
            //    return;
            //}

            e.Effects = DragDropEffects.Move;
            string preStr = "";
            if (EditorCommon.DragDrop.DragDropManager.Instance.DragedObjectList.Count > 1)
            {
                preStr = EditorCommon.DragDrop.DragDropManager.Instance.DragedObjectList.Count + "个控件";
            }
            else
            {
                preStr = GetDraggedItemName(EditorCommon.DragDrop.DragDropManager.Instance.DragedObjectList[0]);
            }
            EditorCommon.DragDrop.DragDropManager.Instance.InfoString = $"将{preStr}插入{view.Name}之前";
            mDropType = enDropType.InsertBefore;
            e.Handled = true;
        }

        private void Path_InsertUp_DragLeave(object sender, DragEventArgs e)
        {
            var element = sender as FrameworkElement;
            var view = element.DataContext as OutlinerItem;
            view.UpInsertLineVisible = Visibility.Hidden;
            mDropType = enDropType.None;
            e.Handled = true;
        }

        private void Path_InsertDown_DragEnter(object sender, DragEventArgs e)
        {
            var element = sender as FrameworkElement;
            var view = element.DataContext as OutlinerItem;
            if (view.Parent == null)
                return;
            view.DownInsertLineVisible = Visibility.Visible;
            if (CheckIsDragContainsView(view))
            {
                EditorCommon.DragDrop.DragDropManager.Instance.InfoString = $"无法插入自己之后";
                EditorCommon.DragDrop.DragDropManager.Instance.InfoStringBrush = mErrorInfoBrush;
                mDropType = enDropType.Invalid;
                return;
            }
            if (CheckParentInDraggedItems(view))
            {
                EditorCommon.DragDrop.DragDropManager.Instance.InfoString = $"无法将父放入子{view.Parent.Name}中";
                EditorCommon.DragDrop.DragDropManager.Instance.InfoStringBrush = mErrorInfoBrush;
                mDropType = enDropType.Invalid;
                return;
            }
            if (!view.CheckDrag())
            {
                EditorCommon.DragDrop.DragDropManager.Instance.InfoStringBrush = mErrorInfoBrush;
                mDropType = enDropType.Invalid;
                return;
            }
            if (view.Parent == null)
            {
                EditorCommon.DragDrop.DragDropManager.Instance.InfoString = $"非法操作";
                EditorCommon.DragDrop.DragDropManager.Instance.InfoStringBrush = mErrorInfoBrush;
                mDropType = enDropType.Invalid;
                return;
            }
            else if (!view.Parent.CanHaveChild)
            {
                EditorCommon.DragDrop.DragDropManager.Instance.InfoString = $"{view.Parent.Name}不允许添加子节点";
                EditorCommon.DragDrop.DragDropManager.Instance.InfoStringBrush = mErrorInfoBrush;
                mDropType = enDropType.Invalid;
                return;
            }
            //if (view.Parent.LinkedUIElement is EngineNS.UISystem.Controls.Containers.Border)
            //{
            //    EditorCommon.DragDrop.DragDropManager.Instance.InfoString = $"{view.Parent.Name}无法放入多个对象";
            //    EditorCommon.DragDrop.DragDropManager.Instance.InfoStringBrush = mErrorInfoBrush;
            //    mDropType = enDropType.Invalid;
            //    return;
            //}
            e.Effects = DragDropEffects.Move;
            string preStr = "";
            if (EditorCommon.DragDrop.DragDropManager.Instance.DragedObjectList.Count > 1)
                preStr = EditorCommon.DragDrop.DragDropManager.Instance.DragedObjectList.Count + "个控件";
            else
            {
                preStr = GetDraggedItemName(EditorCommon.DragDrop.DragDropManager.Instance.DragedObjectList[0]);
            }
            EditorCommon.DragDrop.DragDropManager.Instance.InfoString = $"将{preStr}插入{view.Name}之后";
            mDropType = enDropType.InsertAfter;
            e.Handled = true;
        }

        private void Path_InsertDown_DragLeave(object sender, DragEventArgs e)
        {
            var element = sender as FrameworkElement;
            var view = element.DataContext as OutlinerItem;
            view.DownInsertLineVisible = Visibility.Hidden;
            mDropType = enDropType.None;
            e.Handled = true;
        }

        enum enDropType
        {
            None,
            AddChild,
            ReplceContent,
            SetContent,
            ReplceParentContent,
            InsertBefore,
            InsertAfter,
            Invalid,
        }
        enDropType mDropType;
        private void Rectangle_Drop(object sender, DragEventArgs e)
        {
            var noUse = DropProcess(sender, e);
            e.Handled = true;
        }
        public OutlinerItem CreateOutlinerItemByActor(GActor actor)
        {
            OutlinerItem item = null;
            if (actor is GPrefab)
            {
                item = new PrefabOutlinerItem(actor, this);
            }
            else
            {
                item = new ActorOutlinerItem(actor, this);
            }
            for (int i = 0; i < actor.GetChildrenUnsafe().Count; ++i)
            {
                AddActorItemRecursion(item, actor.GetChildrenUnsafe()[i]);
            }
            return item;
        }
        private async Task DropProcess(object sender, DragEventArgs e)
        {
            await EngineNS.Thread.AsyncDummyClass.DummyFunc();
            var element = sender as FrameworkElement;
            var view = element.DataContext as OutlinerItem;
            if (view == null)
                return;

            var rc = EngineNS.CEngine.Instance.RenderContext;

            switch (mDropType)
            {
                case enDropType.AddChild:
                    {
                        List<OutlinerUndoRedoItem> items = new List<OutlinerUndoRedoItem>();
                        foreach (var item in EditorCommon.DragDrop.DragDropManager.Instance.DragedObjectList)
                        {
                            if (item is OutlinerItem)
                            {
                                var itemView = item as OutlinerItem;
                                if (itemView != null && view != itemView.Parent)
                                {
                                    items.Add(new OutlinerUndoRedoItem(itemView.Parent, itemView));
                                }
                            }
                            if (item is Resources.IResourceInfoCreateActor)
                            {
                                var rInfoItem = item as Resources.IResourceInfoCreateActor;
                                {
                                    var actor = await rInfoItem.CreateActor();
                                    var newItem = CreateOutlinerItemByActor(actor);
                                    items.Add(new OutlinerUndoRedoItem(null, newItem));
                                }
                            }
                        }
                        var redo = new Action<object>((obj) =>
                        {
                            for (int i = 0; i < items.Count; ++i)
                            {
                                if (items[i].LastParent != null)
                                {
                                    items[i].LastParent.Remove(items[i].Item);
                                }
                                view.IsExpanded = true;
                                view.Add(items[i].Item);
                                view.IsExpanded = true;
                            }
                        });
                        redo.Invoke(null);
                        UndoRedo.UndoRedoManager.Instance.AddCommand(UndoRedoKey, null, redo, null, (obj) =>
                        {
                            for (int i = 0; i < items.Count; ++i)
                            {
                                var pair = items[i];
                                pair.Item.Parent.Remove(pair.Item);
                                if (pair.LastParent != null)
                                {
                                    pair.LastParent.Add(pair.Item);
                                    pair.LastParent.IsExpanded = true;
                                }
                            }

                        }, $"添加{items.Count}个对象");

                    }
                    break;
                case enDropType.ReplceContent:
                case enDropType.SetContent:
                    {
                        foreach (var item in EditorCommon.DragDrop.DragDropManager.Instance.DragedObjectList)
                        {
                            var itemView = item as OutlinerItem;
                            if (itemView != null)
                            {
                                view.IsExpanded = true;
                                itemView.Parent.ChildList.Remove(itemView);
                                view.ChildList.Clear();
                                view.ChildList.Add(itemView);
                                itemView.Parent = view;
                            }
                            else
                            {

                            }
                        }
                    }
                    break;
                case enDropType.ReplceParentContent:
                    throw new InvalidOperationException("未实现!");
                case enDropType.InsertBefore:
                case enDropType.InsertAfter:
                    {
                        List<OutlinerInsertChildUndoRedoItem> items = new List<OutlinerInsertChildUndoRedoItem>();
                        var parentItem = view.Parent;
                        int insertIndex = 0;
                        insertIndex = parentItem.ChildList.IndexOf(view);
                        foreach (var item in EditorCommon.DragDrop.DragDropManager.Instance.DragedObjectList)
                        {
                            if (item is OutlinerItem)
                            {
                                var itemView = item as OutlinerItem;
                                if (itemView != null)
                                {
                                    if (itemView.Parent != parentItem)
                                    {
                                        insertIndex++;
                                    }
                                    items.Add(new OutlinerInsertChildUndoRedoItem(insertIndex, itemView.Parent, itemView));
                                }
                                else
                                {

                                }
                            }
                            if (item is Resources.IResourceInfoCreateActor)
                            {
                                var rInfoItem = item as Resources.IResourceInfoCreateActor;
                                {
                                    parentItem.IsExpanded = true;
                                    var actor = await rInfoItem.CreateActor();
                                    var newItem = CreateOutlinerItemByActor(actor);
                                    insertIndex++;
                                    items.Add(new OutlinerInsertChildUndoRedoItem(insertIndex, null, newItem));
                                }
                            }
                        }
                        var redo = new Action<object>((obj) =>
                        {
                            foreach (var item in items)
                            {
                                parentItem.IsExpanded = true;
                                if (item.LastParent != null)
                                    item.LastParent.Remove(item.Item);
                                if (item.Index >= parentItem.ChildList.Count)
                                    parentItem.Add(item.Item);
                                else
                                    parentItem.Instert(item.Index, item.Item);
                                parentItem.IsExpanded = true;
                            }
                        });
                        redo.Invoke(null);
                        UndoRedo.UndoRedoManager.Instance.AddCommand(UndoRedoKey, null, redo, null, (obj) =>
                        {
                            for (int i = 0; i < items.Count; ++i)
                            {
                                var pair = items[i];
                                pair.Item.Parent.Remove(pair.Item);
                                if (pair.LastParent != null)
                                {
                                    pair.LastParent.Add(pair.Item);
                                    pair.LastParent.IsExpanded = true;
                                }
                            }

                        }, $"插入{items.Count}个对象");

                    }
                    break;
                case enDropType.Invalid:
                    break;
            }
        }

        private void TreeView_Outliner_DragEnter(object sender, DragEventArgs e)
        {
            for (int i = 0; i < EditorCommon.DragDrop.DragDropManager.Instance.DragedObjectList.Count; ++i)
            {
                if (EditorCommon.DragDrop.DragDropManager.Instance.DragedObjectList[i] is OutlinerItem)
                    return;
            }

            if (Items.Count == 0)
                return;
            var element = sender as FrameworkElement;
            var view = Items[0];
            if (view == null)
                return;
            view.ChildInsertLineVisible = Visibility.Visible;
            if (!view.CheckDrag())
            {
                EditorCommon.DragDrop.DragDropManager.Instance.InfoStringBrush = mErrorInfoBrush;
                mDropType = enDropType.Invalid;
                return;
            }
            if (!view.CanHaveChild)
            {
                EditorCommon.DragDrop.DragDropManager.Instance.InfoString = $"{view.Name}不允许添加子节点";
                EditorCommon.DragDrop.DragDropManager.Instance.InfoStringBrush = mErrorInfoBrush;
                mDropType = enDropType.Invalid;
                return;
            }
            //if (view.Parent.LinkedUIElement is EngineNS.UISystem.Controls.Containers.Border)
            //{
            //    EditorCommon.DragDrop.DragDropManager.Instance.InfoString = $"{view.Parent.Name}无法放入多个对象";
            //    EditorCommon.DragDrop.DragDropManager.Instance.InfoStringBrush = mErrorInfoBrush;
            //    mDropType = enDropType.Invalid;
            //    return;
            //}
            e.Effects = DragDropEffects.Move;
            string preStr = "";
            if (EditorCommon.DragDrop.DragDropManager.Instance.DragedObjectList.Count > 1)
                preStr = EditorCommon.DragDrop.DragDropManager.Instance.DragedObjectList.Count + "个控件";
            else if (EditorCommon.DragDrop.DragDropManager.Instance.DragedObjectList.Count == 0)
            {
                preStr = GetDraggedItemName(EditorCommon.DragDrop.DragDropManager.Instance.DragedObjectList[0]);
            }
            EditorCommon.DragDrop.DragDropManager.Instance.InfoString = $"将{preStr}放入{view.Name}之内";
            mDropType = enDropType.AddChild;
        }

        private void TreeView_Outliner_DragLeave(object sender, DragEventArgs e)
        {
            for (int i = 0; i < EditorCommon.DragDrop.DragDropManager.Instance.DragedObjectList.Count; ++i)
            {
                if (EditorCommon.DragDrop.DragDropManager.Instance.DragedObjectList[i] is OutlinerItem)
                    return;
            }
            if (Items.Count == 0)
                return;
            var view = Items[0];
            if (view == null)
                return;
            view.ChildInsertLineVisible = Visibility.Hidden;
            mDropType = enDropType.None;
        }

        private void TreeView_Outliner_Drop(object sender, DragEventArgs e)
        {
            for (int i = 0; i < EditorCommon.DragDrop.DragDropManager.Instance.DragedObjectList.Count; ++i)
            {
                if (EditorCommon.DragDrop.DragDropManager.Instance.DragedObjectList[i] is OutlinerItem)
                    return;
            }
            if (Items.Count == 0)
                return;
            var element = sender as FrameworkElement;
            var view = Items[0];
            if (view == null)
                return;
            Action action = async () =>
            {
                switch (mDropType)
                {
                    case enDropType.AddChild:
                        {
                            foreach (var item in EditorCommon.DragDrop.DragDropManager.Instance.DragedObjectList)
                            {
                                if (item is Resources.IResourceInfoCreateActor)
                                {
                                    var rInfoItem = item as Resources.IResourceInfoCreateActor;
                                    {
                                        var actor = await rInfoItem.CreateActor();
                                        var newItem = CreateOutlinerItemByActor(actor);
                                        view.IsExpanded = true;
                                        view.Add(newItem);
                                    }
                                }
                            }
                        }
                        break;
                    case enDropType.Invalid:
                        break;
                }
                view.ChildInsertLineVisible = Visibility.Hidden;
                mDropType = enDropType.None;
            };
            action.Invoke();
        }
        #endregion 控件拖动

        #region Filter
        string mFilterString = "";
        ObservableCollection<OutlinerItem> mFilterItems = new ObservableCollection<OutlinerItem>();
        public string FilterString
        {
            get { return mFilterString; }
            set
            {
                mFilterString = value;
                //InitializeCreateMenu();
                TreeView_Outliner.ItemsSource = null;
                ShowItemsWithFilter(Items, mFilterString);
                TreeView_Outliner.ItemsSource = Items;
                OnPropertyChanged("FilterString");
            }
        }
        private bool ShowItemsWithFilter(ObservableCollection<OutlinerItem> items, string filter)
        {
            bool retValue = false;
            foreach (var item in items)
            {
                if (item == null)
                    continue;

                if (string.IsNullOrEmpty(filter))
                {
                    item.Visibility = Visibility.Visible;
                    item.HighLightString = filter;
                    ShowItemsWithFilter(item.ChildList, filter);
                }
                else
                {
                    if (item.ChildList.Count == 0)
                    {
                        if (item.Name.IndexOf(filter, StringComparison.OrdinalIgnoreCase) > -1)
                        {
                            item.Visibility = System.Windows.Visibility.Visible;
                            item.HighLightString = filter;
                            retValue = true;
                        }
                        else
                        {
                            // 根据名称拼音筛选
                            var pyStr = EngineNS.Localization.PinYin.GetAllPYString(item.Name);
                            if (pyStr.IndexOf(filter, StringComparison.OrdinalIgnoreCase) > -1)
                            {
                                item.Visibility = Visibility.Visible;
                                retValue = true;
                            }
                            else
                                item.Visibility = System.Windows.Visibility.Collapsed;
                        }
                    }
                    else
                    {
                        bool bFind = ShowItemsWithFilter(item.ChildList, filter);
                        if (bFind == false)
                            item.Visibility = System.Windows.Visibility.Collapsed;
                        else
                        {
                            item.Visibility = Visibility.Visible;
                            item.IsExpanded = true;
                            retValue = true;
                        }
                    }
                }
            }
            return retValue;
        }
        #endregion Filter

        private void TreeView_Outliner_MouseEnter(object sender, MouseEventArgs e)
        {

        }

        private void TreeViewItem_Selected(object sender, RoutedEventArgs e)
        {

        }
        #region ScrollTo
        Stack<OutlinerItem> mScrollStack = null;
        private void TreeViewItem_Loaded(object sender, RoutedEventArgs e)
        {
            var item = sender as TreeViewItem;
            //var comItem = item.Header as OutlinerItem;
            //if (comItem == null)
            //    return;
            //if (comItem.NeedScrollToWhenLoaded)
            //{
            //    item.BringIntoView();
            //    comItem.NeedScrollToWhenLoaded = false;
            //}
            if (mScrollStack == null || mScrollStack.Count == 0)
                return;
            if (mScrollStack.Last() == (item.Header))
            {
                mScrollStack.Pop();
                return;
            }
            if (mScrollStack.First() == (item.Header))
            {
                mScrollStack.Pop();
                BringIntoView(item, mScrollStack);
                return;
            }
        }

        private static T FindVisualChild<T>(System.Windows.Media.Visual visual) where T : System.Windows.Media.Visual
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(visual); i++)
            {
                System.Windows.Media.Visual child = (System.Windows.Media.Visual)VisualTreeHelper.GetChild(visual, i);
                if (child != null)
                {
                    T correctlyTyped = child as T;
                    if (correctlyTyped != null)
                    {
                        return correctlyTyped;
                    }

                    T descendent = FindVisualChild<T>(child);
                    if (descendent != null)
                    {
                        return descendent;
                    }
                }
            }

            return null;
        }
        public void BringIntoView(ItemsControl itemControl, Stack<OutlinerItem> outlinerItemStack)
        {
            if (outlinerItemStack == null || outlinerItemStack.Count == 0)
                return;
            var item = outlinerItemStack.First();
            var container = itemControl.ItemContainerGenerator.ContainerFromItem(item) as TreeViewItem;
            if (container != null)
            {
                outlinerItemStack.Pop();
                container.BringIntoView();
                BringIntoView(container as ItemsControl, outlinerItemStack);
            }
            else
            {
                VirtualizingStackPanel itemHost = FindVisualChild<VirtualizingStackPanel>(itemControl);

                if (itemHost != null)
                {
                    itemHost.BringIndexIntoViewPublic(itemControl.Items.IndexOf(item));
                }
            }

        }

        public void GetTreeView_OutlinerParent(ItemsControl control, Stack<OutlinerItem> outlinerItemStack, OutlinerItem item)
        {
            outlinerItemStack.Push(item);
            if (!control.Items.Contains(item))
            {
                GetTreeView_OutlinerParent(control, outlinerItemStack, item.Parent);
            }
        }
        #endregion ScrollTo
    }
}
