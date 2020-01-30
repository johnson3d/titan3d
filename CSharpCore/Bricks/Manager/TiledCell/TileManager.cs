using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using EngineNS.GamePlay.SceneGraph;

namespace EngineNS.Bricks.Manager.TiledCell
{
    [GamePlay.SceneGraph.ModuleInitInfo(typeof(TileManager.TileManagerInitializer), "", "Manager", "TileManager")]
    [EngineNS.Editor.Editor_MacrossClass(ECSType.Common, Editor.Editor_MacrossClassAttribute.enMacrossType.Useable)]
    public class TileManager : GamePlay.SceneGraph.GSceneModule
    {
        [Rtti.MetaClassAttribute]
        public class TileManagerInitializer : GamePlay.SceneGraph.GSceneModule.GSceneModuleInitializer
        {
            [Rtti.MetaData]
            public BoundingBox Bounding
            {
                get;
                set;
            } = BoundingBox.EmptyBox();
            [Rtti.MetaData]
            public int XGrid
            {
                get;
                set;
            } = 10;
            [Rtti.MetaData]
            public int ZGrid
            {
                get;
                set;
            } = 10;
        }
        private TileManagerInitializer CurInit
        {
            get
            {
                return (TileManagerInitializer)this.Initializer;
            }
        }
        float mXCellSize;
        [ReadOnly(true)]
        public float XCellSize
        {
            get { return mXCellSize; }
            set
            {
                mXCellSize = value;
                OnPropertyChanged("XCellSize");
            }
        }
        float mZCellSize;
        [ReadOnly(true)]
        public float ZCellSize
        {
            get { return mZCellSize; }
            set
            {
                mZCellSize = value;
                OnPropertyChanged("ZCellSize");
            }
        }
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.ReadOnly)]
        public float StartX
        {
            get { return CurInit.Bounding.Minimum.X; }
            set
            {
                var box = CurInit.Bounding;
                box.Minimum.X = value;
                Reset(box, CurInit.XGrid, CurInit.ZGrid);
            }
        }
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.ReadOnly)]
        public float StartZ
        {
            get { return CurInit.Bounding.Minimum.Z; }
            set
            {
                var box = CurInit.Bounding;
                box.Minimum.Z = value;
                Reset(box, CurInit.XGrid, CurInit.ZGrid);
            }
        }
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.ReadOnly)]
        public float XSize
        {
            get { return CurInit.Bounding.Maximum.X - CurInit.Bounding.Minimum.X; }
            set
            {
                var box = CurInit.Bounding;
                box.Maximum.X = box.Minimum.X + value;
                Reset(box, CurInit.XGrid, CurInit.ZGrid);
            }
        }
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.ReadOnly)]
        public float ZSize
        {
            get { return CurInit.Bounding.Maximum.Z - CurInit.Bounding.Minimum.Z; }
            set
            {
                var box = CurInit.Bounding;
                box.Maximum.Z = box.Minimum.Z + value;
                Reset(box, CurInit.XGrid, CurInit.ZGrid);
            }
        }
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.ReadOnly)]
        public int XGrid
        {
            get { return CurInit.XGrid; }
            set
            {
                Reset(CurInit.Bounding, value, CurInit.ZGrid);
            }
        }
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.ReadOnly)]
        public int ZGrid
        {
            get { return CurInit.ZGrid; }
            set
            {
                Reset(CurInit.Bounding, CurInit.XGrid, value);
            }
        }
        private Cell[,] mCells;
        public Cell[,] Cells
        {
            get { return mCells; }
        }
        public override async Task<bool> SetInitializer(CRenderContext rc, GSceneGraph host, GSceneModuleInitializer v)
        {
            await base.SetInitializer(rc, host, v);

            var init = v as TileManagerInitializer;

            if(init.Bounding.IsEmpty())
            {
                init.Bounding = host.GetBoundingBox();
            }
            
            Reset(init.Bounding, init.XGrid, init.ZGrid);
            return true;
        }
        internal void Reset(BoundingBox box, int xGrid, int zGrid)
        {
            var pointActors = new Dictionary<PointActorComponent, PointActorComponent>();
            var boxActors = new Dictionary<BoxActorComponent, BoxActorComponent>();
            for (int i = 0; i < ZGrid; i++)
            {
                for (int j = 0; j < XGrid; j++)
                {
                    var cc = GetCell(j, i);
                    if (cc != null)
                    {
                        foreach(var k in cc.PointActors)
                        {
                            pointActors[k] = k;
                        }
                        foreach (var k in cc.BoxActors)
                        {
                            boxActors[k] = k;
                        }
                    }
                }
            }

            CurInit.Bounding = box;
            XCellSize = CurInit.Bounding.GetSize().X / xGrid;
            ZCellSize = CurInit.Bounding.GetSize().Z / zGrid;
            CurInit.XGrid = xGrid;
            CurInit.ZGrid = zGrid;

            mCells = new Cell[ZGrid, XGrid];
            foreach(var i in pointActors)
            {
                i.Value.CurCell = null;
                UpdatePointComponent(i.Value);
            }
            foreach (var i in boxActors)
            {
                i.Value.CurBox.InitEmptyBox();
                UpdateBoxComponent(i.Value);
            }
        }
        public delegate bool FOnTouch(GamePlay.Actor.GActor actor, bool isPoint);
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public void TryTouch(Vector3 center, float radius, FOnTouch onTouch)
        {
            if (onTouch == null)
                return;
            BoundingBox aabb = new BoundingBox();
            var temp = new Vector3(radius, radius, radius);
            aabb.Minimum = center - temp;
            aabb.Maximum = center + temp;
            int sx = (int)((aabb.Minimum.X - CurInit.Bounding.Minimum.X) / XCellSize);
            int sz = (int)((aabb.Minimum.Z - CurInit.Bounding.Minimum.Z) / ZCellSize);
            int ex = (int)((aabb.Maximum.X - CurInit.Bounding.Minimum.X) / XCellSize);
            int ez = (int)((aabb.Maximum.Z - CurInit.Bounding.Minimum.Z) / ZCellSize);
            for (int i = sz; i < ez; i++)
            {
                for (int j = sx; j < ex; j++)
                {
                    var cc = GetCell(j, i);
                    if (cc != null)
                    {
                        using (var iter = cc.PointActors.GetEnumerator())
                        {
                            while(iter.MoveNext())
                            {
                                if (onTouch(iter.Current.Host, true) == false)
                                    return;
                            }
                        }
                        using (var iter = cc.BoxActors.GetEnumerator())
                        {
                            while (iter.MoveNext())
                            {
                                if (onTouch(iter.Current.Host, false) == false)
                                    return;
                            }
                        }
                    }
                }
            }
        }
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public Cell GetCell(int x, int z)
        {
            if (mCells == null)
                return null;
            if (x < 0 || z < 0 || x >= XGrid || z >= ZGrid)
                return null;
            if(mCells[z, x]==null)
            {
                mCells[z, x] = new Cell();
            }
            return mCells[z, x];
        }
        public void UpdatePointComponent(PointActorComponent pointComp)
        {
            var actor = pointComp.Host;
            int x = (int)((actor.Placement.Location.X - CurInit.Bounding.Minimum.X) / XCellSize);
            int z = (int)((actor.Placement.Location.Z - CurInit.Bounding.Minimum.Z) / ZCellSize);
            var newCell = GetCell(x, z);
            if (pointComp.CurCell != newCell)
            {
                if (pointComp.CurCell != null)
                    pointComp.CurCell.PointActors.Remove(pointComp);
                if (newCell != null)
                {
                    newCell.PointActors.Add(pointComp);
                    pointComp.CurCell = newCell;
                }
            }
        }
        public void UpdateBoxComponent(BoxActorComponent boxComp)
        {
            var actor = boxComp.Host;
            BoundingBox aabb = boxComp.CurBox;
            if (aabb.IsEmpty() == false)
            {
                int sx = (int)((aabb.Minimum.X - CurInit.Bounding.Minimum.X) / XCellSize);
                int sz = (int)((aabb.Minimum.Z - CurInit.Bounding.Minimum.Z) / ZCellSize);
                int ex = (int)((aabb.Maximum.X - CurInit.Bounding.Minimum.X) / XCellSize);
                int ez = (int)((aabb.Maximum.Z - CurInit.Bounding.Minimum.Z) / ZCellSize);
                for (int i = sz; i < ez; i++)
                {
                    for (int j = sx; j < ex; j++)
                    {
                        var cc = GetCell(j, i);
                        if (cc != null)
                        {
                            cc.BoxActors.Remove(boxComp);
                        }
                    }
                }
            }

            {
                actor.GetAABB(ref aabb);
                boxComp.CurBox = aabb;
                int sx = (int)((aabb.Minimum.X - CurInit.Bounding.Minimum.X) / XCellSize);
                int sz = (int)((aabb.Minimum.Z - CurInit.Bounding.Minimum.Z) / ZCellSize);
                int ex = (int)((aabb.Maximum.X - CurInit.Bounding.Minimum.X) / XCellSize);
                int ez = (int)((aabb.Maximum.Z - CurInit.Bounding.Minimum.Z) / ZCellSize);
                for (int i = sz; i < ez; i++)
                {
                    for (int j = sx; j < ex; j++)
                    {
                        var cc = GetCell(j, i);
                        if (cc != null)
                        {
                            cc.BoxActors.Add(boxComp);
                        }
                    }
                }
            }
        }
        public void UpdateActor(GamePlay.Actor.GActor actor)
        {
            var pointComp = actor.GetComponent<PointActorComponent>();
            if(pointComp!=null)
            {
                UpdatePointComponent(pointComp);
            }
            else
            {
                var boxComp = actor.GetComponent<BoxActorComponent>();
                if(boxComp!=null)
                    UpdateBoxComponent(boxComp);
            }
        }
    }
}
