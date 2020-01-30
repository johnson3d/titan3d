using System;
using System.Collections;
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
using EditorCommon.Resources;
using EngineNS;
using EngineNS.GamePlay;

namespace EditorCommon.Controls.SceneNodes
{
    public class ActorData : TreeListView.TreeItemViewModel, EditorCommon.DragDrop.IDragAbleObject
    {
        public string HighLightString
        {
            get { return (string)GetValue(HighLightStringProperty); }
            set { SetValue(HighLightStringProperty, value); }
        }

        public bool Visible
        {
            get { return (bool)GetValue(VisibleProperty); }
            set { SetValue(VisibleProperty, value); }
        }

        public static readonly DependencyProperty HighLightStringProperty = DependencyProperty.Register("HighLightString", typeof(string), typeof(ActorData), new FrameworkPropertyMetadata(null));
        public static readonly DependencyProperty VisibleProperty = DependencyProperty.Register("Visible", typeof(bool), typeof(ActorData), new FrameworkPropertyMetadata(true));

        public EngineNS.GamePlay.Actor.GActor Actor { get; set; }

        public ActorData()
        {
            Children = new TreeListView.ObservableCollectionAdv<TreeListView.ITreeModel>();
        }

        public System.Windows.FrameworkElement GetDragVisual()
        {
            return null;
        }

        public override bool EnableDrop => true;
    }

    /// <summary>
    /// SceneNodes.xaml 的交互逻辑
    /// </summary>
    public partial class SceneNodes : UserControl, INotifyPropertyChanged
    {
        private ViewPort.ViewPortControl vp;
        TreeListView.ObservableCollectionAdv<TreeListView.ITreeModel> TreeViewItemsNodes = new TreeListView.ObservableCollectionAdv<TreeListView.ITreeModel>();
  
        public SceneNodes()
        {
            InitializeComponent();
            TreeViewActors.TreeListItemsSource = TreeViewItemsNodes;
        }

		//TODO..
        //Dictionary<Guid, TreeListView.ObservableCollectionAdv<TreeListView.ITreeModel>> ActorDatas = new Dictionary<Guid, TreeListView.ObservableCollectionAdv<TreeListView.ITreeModel>>();
        public void BindViewPort(ViewPort.ViewPortControl viewport)
        {
            vp = viewport;
            vp.DRefreshActors -= new ViewPort.ViewPortControl.DelegateRefreshActors(RefreshActors);
            vp.DRefreshActors += new ViewPort.ViewPortControl.DelegateRefreshActors(RefreshActors);

            vp.DAddActor -= new ViewPort.ViewPortControl.DelegateOperationActor(AddActor);
            vp.DAddActor += new ViewPort.ViewPortControl.DelegateOperationActor(AddActor);

            vp.DRemoveActor -= new ViewPort.ViewPortControl.DelegateOperationActor(RemoveActor);
            vp.DRemoveActor += new ViewPort.ViewPortControl.DelegateOperationActor(RemoveActor);

            //vp.DSelectActor -= new ViewPort.ViewPortControl.DelegateOperationActor(FocusActorItem);
            //vp.DSelectActor += new ViewPort.ViewPortControl.DelegateOperationActor(FocusActorItem);

            //vp.DSelectActors -= new ViewPort.ViewPortControl.DelegateSelectActors(FocusActorItems);
            //vp.DSelectActors += new ViewPort.ViewPortControl.DelegateSelectActors(FocusActorItems);
        }

        private void Test_Click(object sender, RoutedEventArgs e)
        {
            RefreshActors( );
        }

        //Update UI after control actors.. 
        public void RefreshActors( )
        {
            //ActorDatas.Clear();
            if (vp != null)
            {
                TreeViewItemsNodes.Clear();
                ShowVPActors(vp);
            }

            SelectActors.Clear();
        }

        private void ShowVPActors(ViewPort.ViewPortControl viewport)
        {
            if (viewport.World == null)
                return;

            foreach (var listactor in viewport.World.Actors)
            {
                EngineNS.GamePlay.Actor.GActor actor = listactor.Value as EngineNS.GamePlay.Actor.GActor;
                if (actor.Parent == null)
                    AddActorToItem(TreeViewItemsNodes, actor);
            }
        }

        private void addActorsToItems(TreeListView.ObservableCollectionAdv<TreeListView.ITreeModel> listads, List<EngineNS.GamePlay.Actor.GActor> actors)
        {
            foreach (var actor in actors)
            {
                AddActorToItem(listads, actor);
            }
        }

        public void RemoveActor(EngineNS.GamePlay.Actor.GActor actor)
        {
            //for (int i = 0; i < TreeViewItemsNodes.Count; i++)
            //{
            //    ActorData itemactor = TreeViewItemsNodes[i] as ActorData;
            //    if (itemactor.Actor == actor)
            //    {
            //        TreeViewItemsNodes.RemoveAt(i);
            //        break;
            //    }
            //}

            RefreshActors( );

            SelectActors.Clear(); 
        }

        public void AddActor(EngineNS.GamePlay.Actor.GActor actor)
        {
            if (actor.Parent == null)
            {
                AddActorToItem(TreeViewItemsNodes, actor);
            }
            else
            {
                //TreeListView.ObservableCollectionAdv<TreeListView.ITreeModel> data;
                //if (ActorDatas.TryGetValue(actor.Parent.ActorId, out data))
                //{
                //    AddActorToItem(data, actor);
                //}
            }

            SelectActors.Clear();
        }

        public bool CheckFilter(EngineNS.GamePlay.Actor.GActor actor)
        {
            if (actor.SpecialName != null && actor.SpecialName.ToLower().IndexOf(mLowerFilterString) > -1)
                return true;

            List<EngineNS.GamePlay.Actor.GActor> actors = actor.GetChildrenUnsafe();
            if (actors.Count == 0)
                return false;

            for (int i = 0; i < actors.Count; i++)
            {
                if (CheckFilter(actors[i]))
                {
                    return true;
                }
            }

            return false;
        }
        private void AddActorToItem(TreeListView.ObservableCollectionAdv<TreeListView.ITreeModel> listads, EngineNS.GamePlay.Actor.GActor actor)
        {
            //过滤掉辅助的actor（物理碰撞盒子）
            if (actor.SpecialName != null && actor.SpecialName.Equals(EngineNS.Bricks.HollowMaker.GeomScene.ActorName))
                return;

            bool filter = true;
            if (!string.IsNullOrEmpty(mFilterString))
            {
                filter = CheckFilter(actor);
            }

            if (filter == false)
                return;
 
            String typeall = actor.GetType().ToString();
            String[] type = typeall.Split('.');
            var actorData = new ActorData() { Actor = actor };
            BindingOperations.SetBinding(actorData, ActorData.NameProperty, new Binding("SpecialName") { Source = actor });
            BindingOperations.SetBinding(actorData, ActorData.HighLightStringProperty, new Binding("FilterString") { Source = this });
            BindingOperations.SetBinding(actorData, ActorData.VisibleProperty, new Binding("Visible") { Source = actor });
            listads.Add(actorData);//actor.SpecialName

            if (actor.GetChildrenUnsafe().Count > 0)
                addActorsToItems(actorData.Children, actor.GetChildrenUnsafe());

            
        }

        private void selectPrefabActor(EngineNS.GamePlay.Actor.GActor actor)
        {
        }

        bool NeedSelectionChanged = true;
        Dictionary<int, EditorCommon.ViewPort.ViewPortControl.SelectActorData> SelectActors = new Dictionary<int, EditorCommon.ViewPort.ViewPortControl.SelectActorData>();
        private void TreeViewActors_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TreeViewActors.SelectedIndex < 0)
                return;
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.LeftShift))
            {
                RevertSelection( );
                NeedSelectionChanged = false;
                return;
            }

            SelectionChanged( );
        }
        public void UnSelectAll()
        {
            TreeViewActors.SelectedIndex = -1;
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox checkbox = sender as CheckBox;
            StackPanel sp = checkbox.Parent as StackPanel;
            ActorData ac = sp.DataContext as ActorData;
            if (ac != null && ac.Actor != null)
                ac.Actor.Visible = true;
        }

        private void checkBox_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckBox checkbox = sender as CheckBox;
            StackPanel sp = checkbox.Parent as StackPanel;
            ActorData ac = sp.DataContext as ActorData;
            if (ac != null && ac.Actor != null)
                ac.Actor.Visible = false;
        }

        private void RevertSelection( )
        {
            foreach (var i in SelectActors.Keys)
            {
                var item = TreeViewActors.ItemContainerGenerator.ContainerFromItem(TreeViewActors.Items[i]) as TreeListView.TreeListItem;

                if (item != null)
                {
                    item.IsSelected = true;
                }
            }
        }
 
        private void SelectionChanged()
        {
            if (TreeViewActors.SelectedItem == null)
                return;

            SelectActors.Clear();

            if (TreeViewActors.TreeListItemsSource.Count <= 0)
                return;

            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.LeftShift))
            {
                for (int i = 0; i < TreeViewActors.Items.Count; i++)
                {
                    var item = TreeViewActors.ItemContainerGenerator.ContainerFromItem(TreeViewActors.Items[i]) as TreeListView.TreeListItem;

                    if (item != null && item.IsSelected)
                    {
                        ActorData ad = item.Content as ActorData;
                        if (ad != null)
                        {
                            SelectActors.Add(i, new EditorCommon.ViewPort.ViewPortControl.SelectActorData()
                            {
                                Actor = ad.Actor,
                                StartTransMatrix = ad.Actor.Placement.WorldMatrix,
                            });
                        }
                    }
                }

                vp.SelectActors(SelectActors.Values.ToArray());
            }
            else
            {
                var item = TreeViewActors.ItemContainerGenerator.ContainerFromItem(TreeViewActors.Items[TreeViewActors.SelectedIndex]) as TreeListView.TreeListItem;
                if(item != null)
                {
                    ActorData ad = item.Content as ActorData;
                    if (ad != null)
                    {
                        SelectActors.Add(TreeViewActors.SelectedIndex, new EditorCommon.ViewPort.ViewPortControl.SelectActorData()
                        {
                            Actor = ad.Actor,
                            StartTransMatrix = ad.Actor.Placement.WorldMatrix,
                        });
                        vp.SelectActor(ad.Actor);
                    }
                }
            }
        }

        System.Windows.Point mMouseLeftButtonDownPointInTreeView;
        bool mMouseDown = false;
        private void TreeViewItem_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (mMouseDown == false)
                {
                    mMouseDown = true;
                    mMouseLeftButtonDownPointInTreeView = e.GetPosition(TreeViewActors);
                }
                else
                {
                    var pos = e.GetPosition(TreeViewActors);
                    var length = (pos - mMouseLeftButtonDownPointInTreeView).Length;
                    if (length > 15)
                    {
                        var dragObjList = new List<EditorCommon.DragDrop.IDragAbleObject>();
                        foreach (EditorCommon.TreeListView.TreeNode item in TreeViewActors.SelectedItems)
                        {
                            var avm = item.Tag as EditorCommon.DragDrop.IDragAbleObject;
                            if (avm == null)
                                continue;

                            dragObjList.Add(avm);
                        }
                        EditorCommon.DragDrop.DragDropManager.Instance.StartDrag(EditorCommon.Controls.ResourceBrowser.ContentControl.SceneNodeDragType, dragObjList.ToArray());
                        mMouseDown = false;
                    }
                }
            }
        }

        private void TreeViewItem_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (mMouseDown)
            {
                mMouseDown = false;
            }

            if (NeedSelectionChanged == false)
                SelectionChanged( );
        }

        //int prefocusindex = 0;
        //Focus actor ui form viewport. 
        //public void FocusActorItem(EngineNS.GamePlay.Actor.GActor actor)
        //{
        //    if (prefocusindex < TreeViewActors.TreeListItemsSource.Count)
        //    {
        //        ActorData preitemactor = TreeViewActors.TreeListItemsSource[prefocusindex] as ActorData;
        //        if (preitemactor != null)
        //        {
        //            if (preitemactor.Actor == actor)
        //                return;

        //            var item = TreeViewActors.ItemContainerGenerator.ContainerFromItem(TreeViewActors.Items[prefocusindex]) as TreeListView.TreeListItem;
        //            if (item != null)
        //                item.IsSelected = false;
        //        }
        //    }

        //    SelectActors.Clear( );
        //    for (int i = 0; i < TreeViewActors.TreeListItemsSource.Count; i ++)
        //    {
        //        ActorData itemactor = TreeViewActors.TreeListItemsSource[i] as ActorData;
        //        if (itemactor != null && itemactor.Actor == actor)
        //        {

        //            SelectActors.Add(i, new EditorCommon.ViewPort.ViewPortControl.SelectActorData()
        //            {
        //                Actor = actor,
        //                StartTransMatrix = actor.Placement.WorldMatrix,
        //            });
        //            //TODO..
        //            TreeViewActors.Focus();

        //            var item = TreeViewActors.ItemContainerGenerator.ContainerFromItem(TreeViewActors.Items[i]) as TreeListView.TreeListItem;
        //            if (item != null)
        //            {
        //                item.IsEnabled = true;
        //                item.IsSelected = true;
        //                TreeViewActors.Items.MoveCurrentTo(item);
        //            }
        //            prefocusindex = i;
        //            break;
        //        }
        //    }
        //}

        //TODO..
        public void FocusActorItems(List<EngineNS.GamePlay.Actor.GActor> selectActors)//, EngineNS.GamePlay.Actor.GActor actor)
        {
            if (SelectActors.Count > 0)
            {
                int[] skeys = SelectActors.Keys.ToArray();
                for (int i = 0; i < skeys.Length; i ++)
                {
                    var item = TreeViewActors.ItemContainerGenerator.ContainerFromItem(TreeViewActors.Items[skeys[i]]) as TreeListView.TreeListItem;
                    if (item != null)
                        item.IsSelected = false;
                }
            }
 
            SelectActors.Clear();

            if (TreeViewActors.TreeListItemsSource.Count <= 0)
                return;

            var lsad = new List<EngineNS.GamePlay.Actor.GActor>(selectActors);
 
            for (int i = 0; i < TreeViewActors.Items.Count; i++)
            {
               if (lsad.Count == 0)
                   break;

               //TODO.. 有時候拿不到对象
               var item = TreeViewActors.ItemContainerGenerator.ContainerFromItem(TreeViewActors.Items[i]) as TreeListView.TreeListItem;
               if (item != null)
               {
                   ActorData ad = item.Content as ActorData;
                   item.IsSelected = false;
                   for (int j = 0; j < lsad.Count; j++)
                   {
                       if (lsad[j] == ad.Actor)
                       {
                           //if (ad.Actor == actor)
                           //{
                           //    //TreeViewActors.Items.MoveCurrentTo(TreeViewActors.Items[i]);
                           //    //TreeViewActors.Items.MoveCurrentToPosition(i);
                           //    //TreeViewActors.SelectedItem = TreeViewActors.Items[i];
                           //    //TreeViewActors.SelectedIndex = i;
                           //    //TreeViewActors.Items.Refresh();
                           //}


                           EditorCommon.ViewPort.ViewPortControl.SelectActorData outdata;
                           if (SelectActors.TryGetValue(i, out outdata) == false)
                           {
                               SelectActors.Add(i, new EditorCommon.ViewPort.ViewPortControl.SelectActorData()
                               {
                                   Actor = lsad[j],
                                   StartTransMatrix = lsad[j].Placement.WorldMatrix,
                               });
                           }
                         
                           lsad.RemoveAt(j);
                           item.IsSelected = true;
                           break;
                       }
                   }
               }
               else
               {
                   //TODO..
               }
            }
        }

        //Search actors..
        #region INotifyPropertyChangedMembers
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
        }
        #endregion

        string mFilterString = "";
        string mLowerFilterString = "";
        public string FilterString
        {
            get { return mFilterString; }
            set
            {
                mFilterString = value;
                mLowerFilterString = value.ToLower();
               // ShowItemWithFilter(TreeViewItemsNodes, mFilterString);
                OnPropertyChanged("FilterString");

                RefreshActors();
            }
        }

        private void UserControl_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Delete:
                    {
                        if (vp != null && SelectActors.Count > 0)
                        {
                            EditorCommon.ViewPort.ViewPortControl.SelectActorData[] values = SelectActors.Values.ToArray();
                            for (int i = 0; i < values.Length; i++)
                            {
                                var actor = values[i].Actor;
                                vp.RemoveActor(actor);
                            }

                            SelectActors.Clear();
                        }
                    }
                    break;
          
            }
        }

        // Multiple selection.
    }
}

//namespace EngineNS.GamePlay.Actor
//{
//    partial class GActor
//    {
//        public EditorCommon.Controls.SceneNodes.ActorData ActorData;
//    }
    
//}
