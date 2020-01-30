using System;
using System.Collections.Generic;
using System.Text;
using EngineNS.GamePlay.Component;

namespace EngineNS.Bricks.Manager.TiledCell
{
    public class Cell
    {
        public List<PointActorComponent> PointActors = new List<PointActorComponent>();
        public List<BoxActorComponent> BoxActors = new List<BoxActorComponent>();
    }
    public class TileManagedActorComponent : GamePlay.Component.GComponent, IPlaceable
    {
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public TileManager Manager
        {
            get;
            set;
        }
        public GPlacementComponent Placement
        {
            get
            {
                return this.Host.Placement;
            }
            set
            {

            }
        }
        public override void OnAddedScene()
        {
            Manager = this.Host.Scene.GetModule(typeof(TileManager)) as TileManager;
        }
        public virtual void OnPlacementChanged(GPlacementComponent placement)
        {
            
        }
        public virtual void OnPlacementChangedUninfluencePhysics(GPlacementComponent placement)
        {
            //useless
        }
    }
    public class TileManagedEditComponent : TileManagedActorComponent
    {
        public int XGrid
        {
            get;
            set;
        }
        public int ZGrid
        {
            get;
            set;
        }
        public override void OnPlacementChanged(GPlacementComponent placement)
        {
            if (Manager == null)
                return;

            var box = new BoundingBox();
            Host.GetAABB(ref box);
            Manager.Reset(box, XGrid, ZGrid);
        }
    }
    public class PointActorComponent : TileManagedActorComponent
    {
        public Cell CurCell;
        public override void OnPlacementChanged(GPlacementComponent placement)
        {
            if (Manager == null)
                return;

            Manager.UpdatePointComponent(this);
        }
    }
    public class BoxActorComponent : TileManagedActorComponent
    {
        public BoundingBox CurBox;
        public override void OnPlacementChanged(GPlacementComponent placement)
        {
            if (Manager == null)
                return;
            Manager.UpdateBoxComponent(this);
        }
    }
}
