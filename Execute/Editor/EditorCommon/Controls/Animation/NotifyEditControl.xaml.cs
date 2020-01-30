using EngineNS.Bricks.Animation.AnimNode;
using EngineNS.Bricks.Animation.Notify;
using EngineNS.Bricks.Animation.Skeleton;
using System;
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

namespace EditorCommon.Controls.Animation
{
    public class NotifyTrackEventArgs : EventArgs
    {
        public int TrackNum = 0;
        public NotifyTrackEventArgs(int tracknum)
        {
            TrackNum = tracknum;
        }
    }
    public class AnimNotifyEditControlEventArgs
    {
        public int TrackNum;
        public AnimNotifyNodeControl Ctrl;
        public AnimNotifyEditControlEventArgs(int trackNum, AnimNotifyNodeControl ctrl)
        {
            TrackNum = trackNum;
            Ctrl = ctrl;
        }
    }
    /// <summary>
    /// Interaction logic for NotifyEditeControl.xaml
    /// </summary>
    public partial class NotifyEditControl : UserControl
    {
        public static void CreateTextSeparator(ContextMenu menu, string text)
        {
            menu.Items.Add(new ResourceLibrary.Controls.Menu.TextSeparator()
            {
                Style = Application.Current.MainWindow.TryFindResource(new ComponentResourceKey(typeof(ResourceLibrary.CustomResources), "TextMenuSeparatorStyle")) as Style,
                Text = text,
            });
        }
        public event EventHandler OnLoaded;
        public event EventHandler<NotifyTrackEventArgs> OnTrackAdd;
        public event EventHandler<NotifyTrackEventArgs> OnTrackRemove;
        public event EventHandler<AnimNotifyEditControlEventArgs> OnNotifyPickUp;
        public event EventHandler<AnimNotifyEditControlEventArgs> OnNotifyDropDown;
        public event EventHandler<AnimNotifyEditControlEventArgs> OnMouseIn;
        public event EventHandler<AnimNotifyEditControlEventArgs> OnMouseOut;
        public event EventHandler<AnimNotifyEditControlEventArgs> OnAddNotify;
        public event EventHandler<AnimNotifyEditControlEventArgs> OnRemoveNotify;
        public event EventHandler<AnimNotifyEditControlEventArgs> OnChangeNotifyTrack;
        public event EventHandler<AnimNotifyEditControlEventArgs> OnNotifySelected;
        public CGfxSkeleton SkeletonAsset
        {
            get;
            set;
        }
        public EditorAnimationClip EditorAnimationClip
        {
            get;
            set;
        }
        int mTrackNum = 0;
        public int TrackNum
        {
            get => mTrackNum;
            set
            {
                mTrackNum = value;
                EngineNS.CEngine.Instance.EventPoster.RunOn(() =>
                {
                    TrackNumTextBlock.Text = value.ToString();
                    return true;
                }, EngineNS.Thread.Async.EAsyncTarget.Editor);
                //change notify trackID
            }
        }
        public event EventHandler<TickBarScaleEventArgs> OnTickBarScaling;
        public NotifyEditControl()
        {
            InitializeComponent();
            AnimSlider.OnTickBarScaling += AnimSlider_OnTickBarScaling;
            AnimSlider.OnTickBarRightButtonClick += AnimSlider_OnTickBarRightButtonClick;
            AnimSlider.OnNotifyCtrlPickUp += AnimSlider_OnNotifyCtrlPickUp;
            AnimSlider.OnNotifyCtrlDropDown += AnimSlider_OnNotifyCtrlDropDown;
            AnimSlider.OnNotifyCtrlRightButtonDown += AnimSlider_OnNotifyCtrlRightButtonDown;
            AnimSlider.OnNotifyCtrlLeftButtonDown += AnimSlider_OnNotifyCtrlLeftButtonDown;
        }


        private void AnimSlider_OnNotifyCtrlRightButtonDown(object sender, AnimNotifySliderEventArgs e)
        {
            rightClickMousePoint = e.MousePoint;
            AnimSlider.TickBarContexMenu.Items.Clear();
            MenuItem item = new MenuItem();
            item.Name = "MenuItem_NotifyEditorControl_Delete";
            item.Header = "Delete";
            item.Foreground = Brushes.White;
            item.Click += (obj, handler) =>
            {
                RemoveNotifyNode(e.Ctrl);
            };
            AnimSlider.TickBarContexMenu.Visibility = Visibility.Visible;
            AnimSlider.TickBarContexMenu.Items.Add(item);
            AnimSlider.TickBarContexMenu.PlacementTarget = sender as Canvas;
            AnimSlider.TickBarContexMenu.IsOpen = true;
        }

        Point rightClickMousePoint;
        void CreateItemFormAssembly()
        {
            var types = EngineNS.CEngine.Instance.MacrossDataManager.MacrossScripAssembly.GetTypes();

            if (types.Length > 0)
            {

                for (int i = 0; i < types.Length; ++i)
                {
                    var type = types[i];
                    if (type == typeof(CGfxNotify) || type.IsSubclassOf(typeof(CGfxNotify)))
                    {
                        var name = EngineNS.RName.GetRName(type.FullName.Replace('.', '/') + ".macross");
                        CreateItmeToContextMenu("New " + type.Name, name, NewNotify_Click);
                    }
                }
            }
        }
        private void AnimSlider_OnTickBarRightButtonClick(object sender, TickBarClickEventArgs e)
        {
            rightClickMousePoint = e.MousePoint;
            AnimSlider.TickBarContexMenu.Items.Clear();
            var name = EngineNS.RName.EmptyName;
            CreateTextSeparator(AnimSlider.TickBarContexMenu, "Built-In");
            //专属CGfxNotify类
            CreateItmeToContextMenu("Play Sound", name, PlaySound_Click);
            CreateItmeToContextMenu("Play Effect", EngineNS.RName.GetRName(typeof(EffectNotify).Name), PlayEffect_Click);
            if (SkeletonAsset.Notifies.Count != 0)
            {
                CreateTextSeparator(AnimSlider.TickBarContexMenu, "Exists");
                foreach (var notify in SkeletonAsset.Notifies)
                {
                    CreateItmeToContextMenu(notify.Name, notify.NotifyType, ExistNotify_Click);
                }
            }
            CreateTextSeparator(AnimSlider.TickBarContexMenu, "New");
            CreateItemFormAssembly();
            CreateItmeToContextMenu("New Notify", name, NewNotify_Click);
            AnimSlider.TickBarContexMenu.PlacementTarget = sender as Canvas;
            AnimSlider.TickBarContexMenu.IsOpen = true;
        }
        public class TypeMenuItem : MenuItem
        {
            public EngineNS.RName Type { get; set; }
        }
        void CreateItmeToContextMenu(string itemName, EngineNS.RName type, RoutedEventHandler handler)
        {
            TypeMenuItem item = new TypeMenuItem();
            item.Name = "MenuItem_NotifyEditControl_Item_";// + itemName.Replace(' ', '_');
            item.Header = itemName;
            item.Foreground = Brushes.White;
            item.Click += handler;
            item.Type = type;
            AnimSlider.TickBarContexMenu.Visibility = Visibility.Visible;
            AnimSlider.TickBarContexMenu.Items.Add(item);
        }
        private void TypeNotify_Click(object sender, RoutedEventArgs e)
        {
            var typeMenuItem = sender as TypeMenuItem;
            var type = typeMenuItem.Type;
            var pos = System.Windows.Forms.Control.MousePosition;
            var transform = PresentationSource.FromVisual(this).CompositionTarget.TransformFromDevice;
            var mousePos = transform.Transform(new Point(pos.X, pos.Y));
            var win = new NotifyCreateWindow();
            win.Left = mousePos.X;
            win.Top = mousePos.Y;
            if (win.ShowDialog() == false)
                return;
            var notifyName = win.NotifyTextBox.Text;
            CreateNotifyNode(notifyName, type);
            if (SkeletonAsset.AddNotify(notifyName, type))
                SkeletonAsset.Save();
        }
        private void PlaySound_Click(object sender, RoutedEventArgs e)
        {
            var typeMenuItem = sender as TypeMenuItem;
            var type = typeMenuItem.Type;
            var pos = System.Windows.Forms.Control.MousePosition;
            var transform = PresentationSource.FromVisual(this).CompositionTarget.TransformFromDevice;
            var mousePos = transform.Transform(new Point(pos.X, pos.Y));
            var win = new NotifyCreateWindow();
            win.Left = mousePos.X;
            win.Top = mousePos.Y;
            if (win.ShowDialog() == false)
                return;
            var notifyName = win.NotifyTextBox.Text;
            CreateSoundNotifyNode(notifyName, type);
        }
        private void PlayEffect_Click(object sender, RoutedEventArgs e)
        {
            var typeMenuItem = sender as TypeMenuItem;
            var type = typeMenuItem.Type;
            var pos = System.Windows.Forms.Control.MousePosition;
            var transform = PresentationSource.FromVisual(this).CompositionTarget.TransformFromDevice;
            var mousePos = transform.Transform(new Point(pos.X, pos.Y));
            var win = new NotifyCreateWindow();
            win.Left = mousePos.X;
            win.Top = mousePos.Y;
            if (win.ShowDialog() == false)
                return;
            var notifyName = win.NotifyTextBox.Text;
            CreateEffectNotifyNode(notifyName, type);
        }
        private void NewNotify_Click(object sender, RoutedEventArgs e)
        {
            var typeMenuItem = sender as TypeMenuItem;
            var type = typeMenuItem.Type;
            var pos = System.Windows.Forms.Control.MousePosition;
            var transform = PresentationSource.FromVisual(this).CompositionTarget.TransformFromDevice;
            var mousePos = transform.Transform(new Point(pos.X, pos.Y));
            var win = new NotifyCreateWindow();
            win.Left = mousePos.X;
            win.Top = mousePos.Y;
            if (win.ShowDialog() == false)
                return;
            var notifyName = win.NotifyTextBox.Text;
            CreateNotifyNode(notifyName, type);
            if (SkeletonAsset.AddNotify(notifyName, type))
                SkeletonAsset.Save();
        }

        private void ExistNotify_Click(object sender, RoutedEventArgs e)
        {
            var item = sender as TypeMenuItem;
            var notifyName = item.Header as string;
            CreateNotifyNode(notifyName, item.Type);
        }
        void CreateNotifyNode(string notifyName, EngineNS.RName nofityType)
        {
            AnimNotifyNodeControl node = new AnimNotifyNodeControl(BuiltInNotifyType.Default);
            node.NotifyRName = nofityType;
            node.Height = AnimSlider.ActualHeight;
            node.NodeName = notifyName;
            node.TrackNum = TrackNum;
            node.Pos = rightClickMousePoint.X - node.NodeWidth * 0.5;
            AnimSlider.AddNofity(node);
            EditorAnimationClip.AddInstanceNotify(node.CGfxNotify, node.NotifyGetter);
            OnAddNotify?.Invoke(this, new AnimNotifyEditControlEventArgs(TrackNum, node));
        }
        void CreateEffectNotifyNode(string notifyName, EngineNS.RName nofityType)
        {
            AnimNotifyNodeControl node = new AnimNotifyNodeControl(BuiltInNotifyType.Effect);
            node.NotifyRName = nofityType;
            node.Height = AnimSlider.ActualHeight;
            node.NodeName = notifyName;
            node.TrackNum = TrackNum;
            node.Pos = rightClickMousePoint.X - node.NodeWidth * 0.5;
            AnimSlider.AddNofity(node);
            EditorAnimationClip.AddInstanceNotify(node.CGfxNotify, node.NotifyGetter);
            OnAddNotify?.Invoke(this, new AnimNotifyEditControlEventArgs(TrackNum, node));
        }
        void CreateSoundNotifyNode(string notifyName, EngineNS.RName nofityType)
        {
            AnimNotifyNodeControl node = new AnimNotifyNodeControl(BuiltInNotifyType.Sound);
            node.NotifyRName = nofityType;
            node.Height = AnimSlider.ActualHeight;
            node.NodeName = notifyName;
            node.TrackNum = TrackNum;
            node.Pos = rightClickMousePoint.X - node.NodeWidth * 0.5;
            AnimSlider.AddNofity(node);
            EditorAnimationClip.AddInstanceNotify(node.CGfxNotify, node.NotifyGetter);
            OnAddNotify?.Invoke(this, new AnimNotifyEditControlEventArgs(TrackNum, node));
        }
        public void RemoveNotifyNode(AnimNotifyNodeControl node)
        {
            AnimSlider.RemoveNotify(node);
            EditorAnimationClip.RemoveInstanceNotify(node.CGfxNotify);
            OnRemoveNotify?.Invoke(this, new AnimNotifyEditControlEventArgs(TrackNum, node));
        }
        public void ChangeNotifyNodeTrack(AnimNotifyNodeControl node)
        {
            node.Height = AnimSlider.ActualHeight;
            node.TrackNum = TrackNum;
            node.Pos = AnimSlider.TicksScaleInterval * node.LocationFrame + AnimSlider.FirstTickPos;
            AnimSlider.AddNofity(node);
            EditorAnimationClip.AddInstanceNotify(node.CGfxNotify, node.NotifyGetter);
            OnChangeNotifyTrack?.Invoke(this, new AnimNotifyEditControlEventArgs(TrackNum, node));
        }
        public EngineNS.Macross.MacrossGetter<CGfxNotify> AddNotify(string notifyName, CGfxNotify cgfxNotify)
        {
            AnimNotifyNodeControl node = new AnimNotifyNodeControl(BuiltInNotifyType.Default);
            node.Height = AnimSlider.ActualHeight;
            node.NodeName = notifyName;
            node.NotifyRName = EngineNS.RName.GetRName(cgfxNotify.GetType().FullName.Replace('.', '/') + ".macross");
            node.TrackNum = TrackNum;
            node.LocationFrame = (double)cgfxNotify.NotifyTime / (double)AnimSlider.AnimationDuration * AnimSlider.Maximum;
            node.Pos = node.LocationFrame * AnimSlider.TicksScaleInterval + AnimSlider.FirstTickPos;
            AnimSlider.AddNofity(node);
            node.CGfxNotify = cgfxNotify;
            OnAddNotify?.Invoke(this, new AnimNotifyEditControlEventArgs(TrackNum, node));
            return node.NotifyGetter;
        }

        AnimNotifyNodeControl mPickedNotifyNodeCtrl = null;
        private void AnimSlider_OnNotifyCtrlPickUp(object sender, AnimNotifySliderEventArgs e)
        {
            mPickedNotifyNodeCtrl = e.Ctrl;
            OnNotifyPickUp?.Invoke(this, new AnimNotifyEditControlEventArgs(TrackNum, e.Ctrl));
        }
        private void AnimSlider_OnNotifyCtrlDropDown(object sender, AnimNotifySliderEventArgs e)
        {
            mPickedNotifyNodeCtrl = null;
            OnNotifyDropDown?.Invoke(this, new AnimNotifyEditControlEventArgs(TrackNum, e.Ctrl));
        }
        private void AnimSlider_OnNotifyCtrlLeftButtonDown(object sender, AnimNotifySliderEventArgs e)
        {
            OnNotifySelected?.Invoke(this, new AnimNotifyEditControlEventArgs(TrackNum, e.Ctrl));
        }

        public void TickLogic()
        {
            EngineNS.CEngine.Instance.EventPoster.RunOn(() =>
            {
                var point = Mouse.GetPosition(AnimSlider.TicksCanvas);
                if (AnimSlider.CheckMouseIn(point))
                {
                    OnMouseIn?.Invoke(this, new AnimNotifyEditControlEventArgs(TrackNum, null));
                }
                else
                {
                    OnMouseOut?.Invoke(this, new AnimNotifyEditControlEventArgs(TrackNum, mPickedNotifyNodeCtrl));
                }
                return true;
            }, EngineNS.Thread.Async.EAsyncTarget.Editor);
        }
        private void AnimSlider_OnTickBarScaling(object sender, TickBarScaleEventArgs e)
        {
            OnTickBarScaling?.Invoke(this, e);
        }
        public void TickBarScale(double deltaScale, double percent)
        {
            AnimSlider.TickBarScale(deltaScale, percent);
        }
        /// <summary>
        /// Slider Thumb Value
        /// </summary>
        public double Value
        {
            get { return (float)AnimSlider.Value; }
            set
            {
                EngineNS.CEngine.Instance.EventPoster.RunOn(() =>
                {
                    AnimSlider.Value = (double)value;
                    return true;
                }, EngineNS.Thread.Async.EAsyncTarget.Editor);

            }
        }
        private void AddTrackButton_Click(object sender, RoutedEventArgs e)
        {
            OnTrackAdd?.Invoke(this, new NotifyTrackEventArgs(TrackNum));
        }
        private void RemoveTrackButton_Click(object sender, RoutedEventArgs e)
        {
            OnTrackRemove?.Invoke(this, new NotifyTrackEventArgs(TrackNum));
        }

        private void userControl_Loaded(object sender, RoutedEventArgs e)
        {
            OnLoaded?.Invoke(this, new EventArgs());
        }
    }
}
