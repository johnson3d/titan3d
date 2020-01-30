using System;
using System.Collections;
using System.Collections.Generic;
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

namespace EditorCommon.Controls.SceneNodes
{
    public class SceneGraphNodeViewData : TreeListView.TreeItemViewModel, EditorCommon.DragDrop.IDragAbleObject
    {
        EngineNS.GamePlay.SceneGraph.ISceneNode mSrcSceneNode;
        public EngineNS.GamePlay.SceneGraph.ISceneNode SrcSceneNode => mSrcSceneNode;

        public string HighLightString
        {
            get { return (string)GetValue(HighLightStringProperty); }
            set { SetValue(HighLightStringProperty, value); }
        }
        public static readonly DependencyProperty HighLightStringProperty = DependencyProperty.Register("HighLightString", typeof(string), typeof(SceneGraphNodeViewData), new FrameworkPropertyMetadata(null));

        public bool Visible
        {
            get { return (bool)GetValue(VisibleProperty); }
            set { SetValue(VisibleProperty, value); }
        }
        public static readonly DependencyProperty VisibleProperty = DependencyProperty.Register("Visible", typeof(bool), typeof(SceneGraphNodeViewData), new FrameworkPropertyMetadata(true, new PropertyChangedCallback(OnVisibleChanged)));
        public static void OnVisibleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as SceneGraphNodeViewData;
            bool newValue = (bool)e.NewValue;
            if(ctrl.mSrcSceneNode != null)
            {
                //foreach(var actorData in ctrl.mSrcSceneNode.Actors)
                //{
                //    EngineNS.GamePlay.Actor.GActor actor;
                //    if (actorData.Value.TryGetTarget(out actor))
                //        actor.Visible = newValue;
                //}
                //ctrl.mSrcSceneNode.Visible = newValue;

                foreach(SceneGraphNodeViewData child in ctrl.Children)
                {
                    child.Visible = newValue;
                }
            }
        }

        public System.Windows.FrameworkElement GetDragVisual()
        {
            return null;
        }

        #region DragDrop

        public override bool EnableDrop => true;

        #endregion

        public SceneGraphNodeViewData(SceneGraphNodeViewData parent, EngineNS.GamePlay.SceneGraph.ISceneNode srcNode)
        {
            Parent = parent;
            mSrcSceneNode = srcNode;
            Name = srcNode.Name;
            BindingOperations.SetBinding(this, NameProperty, new Binding("Name") { Source = srcNode, Mode = BindingMode.TwoWay });
            BindingOperations.SetBinding(this, VisibleProperty, new Binding("Visible") { Source = srcNode, Mode = BindingMode.TwoWay });
        }
    }

    /// <summary>
    /// SceneGraph.xaml 的交互逻辑
    /// </summary>
    public partial class SceneGraph : UserControl
    {
        public delegate void Delegate_OnSelectSceneGraphs(List<EngineNS.GamePlay.SceneGraph.ISceneNode> graphNodes);
        public Delegate_OnSelectSceneGraphs OnSelectSceneGraphs;

        TreeListView.ObservableCollectionAdv<TreeListView.ITreeModel> mSceneGraphNodeViewDatas = new TreeListView.ObservableCollectionAdv<TreeListView.ITreeModel>();
        Dictionary<EngineNS.GamePlay.SceneGraph.ISceneNode, SceneGraphNodeViewData> mViewDataDic = new Dictionary<EngineNS.GamePlay.SceneGraph.ISceneNode, SceneGraphNodeViewData>();

        public SceneGraph()
        {
            InitializeComponent();
        }

        EditorCommon.ViewPort.ViewPortControl mViewport;
        public void BindViewPort(EditorCommon.ViewPort.ViewPortControl viewport)
        {
            mViewport = viewport;
            viewport.DAddActor -= Viewport_DAddActor;
            viewport.DAddActor += Viewport_DAddActor;
            viewport.DRemoveActor -= Viewport_DRemoveActor;
            viewport.DRemoveActor += Viewport_DRemoveActor;
        }

        private void Viewport_DAddActor(EngineNS.GamePlay.Actor.GActor actor)
        {
            foreach(var comp in actor.Components)
            {
                var sgComp = comp as EngineNS.GamePlay.Component.ISceneGraphComponent;
                if(sgComp != null)
                {
                    var node = sgComp.GetSceneNode();
                    SceneGraphNodeViewData parentData = null;
                    if(node.Parent == null)
                    {
                        throw new InvalidOperationException("node.Parent不能为空");
                    }
                    mViewDataDic.TryGetValue(node.Parent, out parentData);
                    CreateNodeItem(parentData, node);
                }
            }
        }
        private void Viewport_DRemoveActor(EngineNS.GamePlay.Actor.GActor actor)
        {
            foreach(var comp in actor.Components)
            {
                var sgComp = comp as EngineNS.GamePlay.Component.ISceneGraphComponent;
                if(sgComp != null)
                {
                    var node = sgComp.GetSceneNode();
                    if(node != null)
                    {
                        SceneGraphNodeViewData nodeData = null;
                        mViewDataDic.TryGetValue(node, out nodeData);
                        if (nodeData != null)
                        {
                            var parentData = nodeData.Parent as SceneGraphNodeViewData;
                            parentData.Children.Remove(nodeData);
                            RemoveNodeFromDic(node);
                        }
                    }
                }
            }
        }
        void RemoveNodeFromDic(EngineNS.GamePlay.SceneGraph.ISceneNode node)
        {
            mViewDataDic.Remove(node);
            foreach(var childNode in node.ChildrenNode)
            {
                RemoveNodeFromDic(childNode);
            }
        }

        public void RefreshFromWorld(EngineNS.GamePlay.GWorld world)
        {
            mViewDataDic.Clear();
            mSceneGraphNodeViewDatas.Clear();
            using(var i = world.GetSceneEnumerator())
            {
                while (i.MoveNext())
                {
                    var nodeV = CreateNodeItem(null, i.Current.Value);
                    mSceneGraphNodeViewDatas.Add(nodeV);
                }
            }
            TreeList_SceneGraphs.TreeListItemsSource = mSceneGraphNodeViewDatas;
        }
        SceneGraphNodeViewData CreateNodeItem(SceneGraphNodeViewData parent, EngineNS.GamePlay.SceneGraph.ISceneNode srcNode)
        {
            var viewData = new SceneGraphNodeViewData(parent, srcNode);
            if(parent != null)
            {
                parent.Children.Add(viewData);
                viewData.Parent = parent;
            }
            mViewDataDic.Add(srcNode, viewData);

            foreach (var childNode in srcNode.ChildrenNode)
            {
                CreateNodeItem(viewData, childNode);
            }
            return viewData;
        }

        private void Button_Refresh_Click(object sender, RoutedEventArgs e)
        {
            RefreshFromWorld(EngineNS.CEngine.Instance.GameEditorInstance.World);
        }

        bool mMouseDown = false;
        Point mMouseLeftButtonDownPos;
        readonly string DragDropType = "SceneGraphDragDropType";
        private void TreeList_SceneGraphs_MouseMove(object sender, MouseEventArgs e)
        {
            if(e.LeftButton == MouseButtonState.Pressed)
            {
                if(mMouseDown == false)
                {
                    mMouseDown = true;
                    mMouseLeftButtonDownPos = e.GetPosition(TreeList_SceneGraphs);
                }
                else
                {
                    var pos = e.GetPosition(TreeList_SceneGraphs);
                    var len = (pos - mMouseLeftButtonDownPos).Length;
                    if(len > 10)
                    {
                        var dragObjList = new List<EditorCommon.DragDrop.IDragAbleObject>();
                        foreach(EditorCommon.TreeListView.TreeNode item in TreeList_SceneGraphs.SelectedItems)
                        {
                            var dragObj = item.Tag as EditorCommon.DragDrop.IDragAbleObject;
                            if (dragObj == null)
                                continue;

                            dragObjList.Add(dragObj);
                        }
                        EditorCommon.DragDrop.DragDropManager.Instance.StartDrag(DragDropType, dragObjList.ToArray());
                        mMouseDown = false;
                    }
                }
            }
        }
        private void TreeList_SceneGraphs_MouseUp(object sender, MouseButtonEventArgs e)
        {
            mMouseDown = false;
        }
        private void TreeList_SceneGraphs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TreeList_SceneGraphs.SelectedIndex < 0)
                return;

            var graphNodes = new List<EngineNS.GamePlay.SceneGraph.ISceneNode>();
            foreach (EditorCommon.TreeListView.TreeNode item in TreeList_SceneGraphs.SelectedItems)
            {
                var data = item.Tag as SceneGraphNodeViewData;
                graphNodes.Add(data.SrcSceneNode);
            }
            OnSelectSceneGraphs?.Invoke(graphNodes);
        }
        public void UnSelectAll()
        {
            TreeList_SceneGraphs.SelectedIndex = -1;
        }
    }
}
