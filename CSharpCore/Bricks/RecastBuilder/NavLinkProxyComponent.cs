using System;
using System.Collections.Generic;
using System.Text;

using System.ComponentModel;

namespace EngineNS.Bricks.RecastRuntime
{
    [Editor.Editor_PlantAbleActor("Navigation", " Nav Link Proxy")]
    public class NavLinkProxyComponent : EngineNS.GamePlay.Component.GVisualComponent, EngineNS.Editor.IPlantable
    {
        public enum LinkDirection:int
        {
            Forward = 0,
            Both = 1,
            Reverse = 2,
        }
    
        public class NavLinkProxyComponentInitializer : GamePlay.Component.GComponent.GComponentInitializer
        {
            [Rtti.MetaData]
            public Vector3 StartPos
            {
                get;
                set;
            } = new Vector3(-5.0f, 0.0f, 0.0f);

            [Rtti.MetaData]
            public Vector3 EndPos
            {
                get;
                set;
            } = new Vector3(5.0f, 0.0f, 0.0f);

            [Rtti.MetaData]
            public float Radius
            {
                get;
                set;
            } = 1.0f;

            [Rtti.MetaData]
            public LinkDirection Direction
            {
                get;
                set;
            } = LinkDirection.Both;
        }

        Vector3 CStartPos = new Vector3(-5.0f, 0.0f, 0.0f);
        Vector3 CEndPos = new Vector3(5.0f, 0.0f, 0.0f);

        Vector3 mStartPos = new Vector3(-5.0f, 0.0f, 0.0f);
        public Vector3 StartPos
        {
            get
            {
                return mStartPos;
            }
            //set
            //{
            //    mStartPos = value;
            //}
        }

        Vector3 mEndPos = new Vector3(5.0f, 0.0f, 0.0f);
        public Vector3 EndPos
        {
            get
            {
                return mEndPos;
            }
            //set
            //{
            //    mEndPos = value;
            //}
        }

        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public float Radius
        {
            get
            {
                var initializer = Initializer as NavLinkProxyComponentInitializer;
                if (initializer != null)
                {
                    return initializer.Radius;
                }

                return 0;
            }
            set
            {
                var initializer = Initializer as NavLinkProxyComponentInitializer;
                if (initializer != null)
                {
                    initializer.Radius = value;
                }
            }
        }


        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public LinkDirection Direction
        {
            get
            {
                var initializer = Initializer as NavLinkProxyComponentInitializer;
                if (initializer != null)
                {
                    return initializer.Direction;
                }

                return 0;
            }
            set
            {
                var initializer = Initializer as NavLinkProxyComponentInitializer;
                if (initializer != null)
                {
                    initializer.Direction = value;
                }
            }
          
        }

        EngineNS.Bricks.GraphDrawer.GraphLines GraphLines = new EngineNS.Bricks.GraphDrawer.GraphLines();
        EngineNS.Bricks.GraphDrawer.McMulLinesGen LinesGen = new EngineNS.Bricks.GraphDrawer.McMulLinesGen();
 
        GamePlay.Component.GMeshComponent mLineMeshComponent;
        public async System.Threading.Tasks.Task InitGraphLines()
        {
            LinesGen.Interval = 0.1f;
            LinesGen.Segement = 0.2f;
            GraphLines.LinesGen = LinesGen;
            EngineNS.Graphics.CGfxMaterialInstance mtl = await EngineNS.CEngine.Instance.MaterialInstanceManager.GetMaterialInstanceAsync(
            EngineNS.CEngine.Instance.RenderContext,
            EngineNS.RName.GetRName("editor/volume/mi_volume_octree.instmtl"));//rotator

            EngineNS.Vector3 p1 = Vector3.Lerp(CStartPos, CEndPos, 0.3f);
            p1.Y = p1.Y + 5.0f;

            EngineNS.Vector3 p2 = Vector3.Lerp(CStartPos, CEndPos, 0.7f);
            p2.Y = p1.Y;
            LinesGen.SetBezier3D(CStartPos, p1, p2, CEndPos, 100);

            var init = await GraphLines.Init(mtl, 0.0f);

            //GraphLines.GraphActor.Placement.Location = EngineNS.Vector3.Zero;

            mLineMeshComponent = GraphLines.GraphActor.GetComponent<GamePlay.Component.GMeshComponent>();
        }

        public override void CommitVisual(CCommandList cmd, Graphics.CGfxCamera camera, GamePlay.SceneGraph.CheckVisibleParam param)
        {
            if (EngineNS.CIPlatform.Instance.PlayMode == CIPlatform.enPlayMode.Editor)
                mLineMeshComponent?.CommitVisual(cmd, camera, param);
        }

        public override void OnUpdateDrawMatrix(ref Matrix drawMatrix)
        {
            base.OnUpdateDrawMatrix(ref drawMatrix);
            mLineMeshComponent.OnUpdateDrawMatrix(ref drawMatrix);
            Vector4 result;
            Vector3.Transform(ref CStartPos, ref drawMatrix, out result);
            mStartPos.X = result.X;
            mStartPos.Y = result.Y;
            mStartPos.Z = result.Z;
            OnPropertyChanged("StartPos");

            Vector3.Transform(ref CEndPos, ref drawMatrix, out result);
            mEndPos.X = result.X;
            mEndPos.Y = result.Y;
            mEndPos.Z = result.Z;
            OnPropertyChanged("EndPos");
        }

        public async System.Threading.Tasks.Task<GamePlay.Actor.GActor> CreateActor(Editor.PlantableItemCreateActorParam param)
        {
            var rc = EngineNS.CEngine.Instance.RenderContext;

            var actor = new EngineNS.GamePlay.Actor.GActor();
            actor.ActorId = Guid.NewGuid();
            var placement = new EngineNS.GamePlay.Component.GPlacementComponent();
            actor.Placement = placement;
            placement.Location = param.Location;

            //var init = new OctreeVolumeComponentInitializer();
            //init.SpecialName = "VolumeData";
            //await SetInitializer(rc, actor, init);

            //mOctree = new Octree();
            //mOctree.LinkedActorId = actor.ActorId;
            if (mLineMeshComponent == null)
                await InitGraphLines();
            mLineMeshComponent.Host = actor;
            var mat = actor.Placement.WorldMatrix;
            mLineMeshComponent.OnUpdateDrawMatrix(ref mat);

            var init = new NavLinkProxyComponentInitializer();
            init.SpecialName = " NavLinkProxy";
            await SetInitializer(rc, actor, actor, init);
            //var aabb = new BoundingBox(-0.5f, -0.5f, -0.5f, 0.5f, 0.5f, 0.5f);
            //BoundingBox.Merge(ref actor.LocalBoundingBox, ref aabb, out actor.LocalBoundingBox);

            actor.AddComponent(this);

            actor.IsNavgation = true;
            return actor;
        }

        //private NavLinkProxyComponentInitializer Initializer;
        public override async System.Threading.Tasks.Task<bool> SetInitializer(CRenderContext rc, GamePlay.IEntity host, GamePlay.Component.IComponentContainer hostContainer, GComponentInitializer v)
        {
            if (!await base.SetInitializer(rc, host, hostContainer, v))
                return false;

            if (mLineMeshComponent == null)
                await InitGraphLines();

            if (mLineMeshComponent != null)
            {
                mLineMeshComponent.Entity = host;

                Host.PlacementChange -= PlacementChange;
                Host.PlacementChange += PlacementChange;
                var mat = this.Host.Placement.WorldMatrix;
                mLineMeshComponent.OnUpdateDrawMatrix(ref mat);
            }

            var initializer = v as NavLinkProxyComponentInitializer;

            Radius = initializer.Radius;
            Direction = initializer.Direction;

            Initializer = initializer;

            return true;
        }

        public void PlacementChange()
        {
        }
    }
}
