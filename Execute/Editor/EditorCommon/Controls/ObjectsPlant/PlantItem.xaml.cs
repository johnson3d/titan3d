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

namespace EditorCommon.Controls.ObjectsPlant
{
    /// <summary>
    /// Interaction logic for PlantItem.xaml
    /// </summary>
    public partial class PlantItem : UserControl, EditorCommon.DragDrop.IDragAbleObject, EditorCommon.DragDrop.IDragToViewport
    {
        public ImageSource Icon
        {
            get { return (ImageSource)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }
        public static readonly DependencyProperty IconProperty = DependencyProperty.Register("Icon", typeof(ImageSource), typeof(PlantItem), new FrameworkPropertyMetadata(null));

        public string ItemName
        {
            get { return (string)GetValue(ItemNameProperty); }
            set { SetValue(ItemNameProperty, value); }
        }
        public static readonly DependencyProperty ItemNameProperty = DependencyProperty.Register("ItemName", typeof(string), typeof(PlantItem), new FrameworkPropertyMetadata(""));

        public Type ItemType;

        public PlantItem()
        {
            InitializeComponent();
        }
        Point mDownPoint;
        private void PlantItem_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.Background = TryFindResource(new ComponentResourceKey(typeof(ResourceLibrary.CustomResources), "ToggleButtonChecked")) as Brush;
            var elm = sender as IInputElement;
            mDownPoint = e.GetPosition(this);
            e.Handled = true;
            Mouse.Capture(elm);
        }

        public static readonly string DragDropType = "PlantItem";
        private void PlantItem_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var pt = e.GetPosition(this);
                if(((pt.X - mDownPoint.X) > 3) ||
                   ((pt.Y - mDownPoint.Y) > 3))
                {
                    this.Background = null;
                    EditorCommon.DragDrop.DragDropManager.Instance.StartDrag(DragDropType, new DragDrop.IDragAbleObject[] { this });
                }
            }
        }

        private void PlantItem_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var pos = e.GetPosition(this);
            if (pos.X >= 0 && pos.Y >= 0 && pos.X <= this.ActualWidth && pos.Y <= this.ActualHeight)
                this.Background = TryFindResource(new ComponentResourceKey(typeof(ResourceLibrary.CustomResources), "LightBrush")) as Brush;
            else
                this.Background = null;
            Mouse.Capture(null);
        }

        private void PlantItem_MouseEnter(object sender, MouseEventArgs e)
        {
            this.Background = TryFindResource(new ComponentResourceKey(typeof(ResourceLibrary.CustomResources), "LightBrush")) as Brush;
        }

        private void PlantItem_MouseLeave(object sender, MouseEventArgs e)
        {
            this.Background = null;
        }

        public FrameworkElement GetDragVisual()
        {
            return this;
        }

        EditorCommon.ViewPort.PreviewActorContainer mPreviewActor = null;
        public async Task OnDragEnterViewport(ViewPort.ViewPortControl viewport, System.Windows.Forms.DragEventArgs e)
        {
            if(mPreviewActor != null)
            {
                await mPreviewActor.AwaitLoad();
            }
            else
            {
                mPreviewActor = new ViewPort.PreviewActorContainer();
                var item = System.Activator.CreateInstance(ItemType) as EngineNS.Editor.IPlantable;
                if (item == null)
                    throw new InvalidOperationException("使用 EngineNS.Editor.Editor_PlantAbleActor 需要继承自 EngineNS.Editor.IPlantable ");
                var viewPortPos = viewport.PointFromScreen(new System.Windows.Point(e.X, e.Y));
                var pos = viewport.GetPickRayLineCheckPosition((float)viewPortPos.X, (float)viewPortPos.Y);
                var param = new EngineNS.Editor.PlantableItemCreateActorParam()
                {
                    View = viewport.RPolicy.BaseSceneView,
                    Location = pos,
                };
                mPreviewActor.mPreviewActor = await item.CreateActor(param);
                mPreviewActor.mPreviewActor.Placement.Location = pos;
                mPreviewActor.ReleaseWaitContext();
            }
            viewport.World.AddActor(mPreviewActor.mPreviewActor);
            viewport.World.DefaultScene.AddActor(mPreviewActor.mPreviewActor);
        }
        public async Task OnDragLeaveViewport(ViewPort.ViewPortControl viewport, System.EventArgs e)
        {
            if (mPreviewActor != null)
                await mPreviewActor.AwaitLoad();

            viewport.World.RemoveActor(mPreviewActor.mPreviewActor.ActorId);
            viewport.World.DefaultScene.RemoveActor(mPreviewActor.mPreviewActor.ActorId);
        }
        public async Task OnDragOverViewport(ViewPort.ViewPortControl viewport, System.Windows.Forms.DragEventArgs e)
        {
            if (mPreviewActor != null)
                await mPreviewActor.AwaitLoad();

            var viewPortPos = viewport.PointFromScreen(new System.Windows.Point(e.X, e.Y));
            var pos = viewport.GetPickRayLineCheckPosition((float)viewPortPos.X, (float)viewPortPos.Y);
            mPreviewActor.mPreviewActor.Placement.Location = pos;
        }
        public async Task OnDragDropViewport(ViewPort.ViewPortControl viewport, System.Windows.Forms.DragEventArgs e)
        {
            var item = System.Activator.CreateInstance(ItemType) as EngineNS.Editor.IPlantable;
            var viewPortPos = viewport.PointFromScreen(new System.Windows.Point(e.X, e.Y));
            var pos = viewport.GetPickRayLineCheckPosition((float)viewPortPos.X, (float)viewPortPos.Y);
            var param = new EngineNS.Editor.PlantableItemCreateActorParam()
            {
                View = viewport.RPolicy.BaseSceneView,
                Location = pos,
            };
            var dropActor = await item.CreateActor(param);

            if(string.IsNullOrEmpty(dropActor.SpecialName))
                dropActor.SpecialName = EngineNS.GamePlay.SceneGraph.GSceneGraph.GeneratorActorSpecialNameInEditor(ItemName, viewport.World);
            viewport.AddActor(dropActor);
        }

        public async Task AddComponent(EngineNS.GamePlay.Actor.GActor actor, EngineNS.GamePlay.Component.GComponent component)
        {
            if (actor != null)
            {
                Type type = component.GetType();
                var atts = type.GetCustomAttributes(typeof(EngineNS.GamePlay.Component.CustomConstructionParamsAttribute), false);
                EngineNS.GamePlay.Component.GComponent.GComponentInitializer initializer = null;
                if (atts.Length > 0)
                {
                    for (int i = 0; i < atts.Length; i++)
                    {
                        var ccAtt = atts[i] as EngineNS.GamePlay.Component.CustomConstructionParamsAttribute;
                        if (ccAtt != null)
                        {
                            initializer = Activator.CreateInstance(ccAtt.ConstructionParamsType) as EngineNS.GamePlay.Component.GComponent.GComponentInitializer;
                            break;
                        }
                    }
                }
                if (initializer != null)
                {
                    await component.SetInitializer(EngineNS.CEngine.Instance.RenderContext, actor , actor, initializer);
                }

                actor.AddComponent(component);
            }
        }
    }
}
