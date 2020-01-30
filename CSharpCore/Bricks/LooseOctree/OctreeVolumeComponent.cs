using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using EngineNS.GamePlay.Actor;
using EngineNS.Graphics;

namespace EngineNS.LooseOctree
{
    // 包围盒几何中心处于此范围内的对象会放入八叉树内，包围盒接触但几何中心不在范围内的不做处理
    [Editor.Editor_PlantAbleActor("Volumn", "OctreeVolumn")]
    [GamePlay.Component.CustomConstructionParamsAttribute(typeof(OctreeVolumeComponentInitializer), "松散八叉树组件", "LooseOctree", "OctreeVolumeComponent")]
     public class OctreeVolumeComponent : BoxComponent, GamePlay.Component.ISceneGraphComponent, EngineNS.Editor.IPlantable, GamePlay.Component.IComponentHostSelectOperation
    {
        [Rtti.MetaClass]
        public class OctreeVolumeComponentInitializer : BoxComponent.BoxComponentInitializer//GamePlay.Component.GComponent.GComponentInitializer
        {
            [Rtti.MetaData]
            public LooseOctree.OctreeDesc Desc
            {
                get;
                set;
            } = new OctreeDesc();

            //[Rtti.MetaData]
            //public bool ShowInGame
            //{
            //    get;
            //    set;
            //} = false;
        }

        [DisplayName("参数")]
        public LooseOctree.OctreeDesc Desc
        {
            get { return ((OctreeVolumeComponentInitializer)Initializer).Desc; }
        }

        public override EngineNS.GamePlay.Actor.GActor Host
        {
            get { return base.Host; }
            set
            {
                if (mActorOctree == null)
                {
                    mActorOctree = new Octree();
                }

                mActorOctree.LinkedActorId = value.ActorId;
                base.Host = value;
            }
        }

        Octree mActorOctree;
        public void SetActorOctree(Octree tree)
        {
            mActorOctree = tree;
        }

        public async System.Threading.Tasks.Task<bool> Build(List<EngineNS.GamePlay.Actor.GActor> actors, List<EngineNS.GamePlay.Actor.GActor> unPVSActors, List<EngineNS.GamePlay.Actor.GActor> containActors)
        {
            return await mActorOctree.BuildTree(this, actors, unPVSActors, containActors);
        }

        public GamePlay.SceneGraph.ISceneNode GetSceneNode()
        {
            return mActorOctree;
        }

        public override async Task<GamePlay.Actor.GActor> CreateActor(Editor.PlantableItemCreateActorParam param)
        {
            var rc = EngineNS.CEngine.Instance.RenderContext;

            var actor = new EngineNS.GamePlay.Actor.GActor();
            actor.ActorId = Guid.NewGuid();
            var placement = new EngineNS.GamePlay.Component.GPlacementComponent();
            actor.Placement = placement;
            placement.Location = param.Location;

            var init = new OctreeVolumeComponentInitializer();
            init.SpecialName = "VolumeData";
            await SetInitializer(rc, actor, actor, init);

            if (mActorOctree == null)
            {
                mActorOctree = new Octree();
                mActorOctree.LinkedActorId = actor.ActorId;
            }

            mLineMeshComponent.Host = actor;
            var mat = actor.Placement.WorldMatrix;
            mLineMeshComponent.OnUpdateDrawMatrix(ref mat);
            //var aabb = new BoundingBox(-0.5f, -0.5f, -0.5f, 0.5f, 0.5f, 0.5f);
            //BoundingBox.Merge(ref actor.LocalBoundingBox, ref aabb, out actor.LocalBoundingBox);

            actor.AddComponent(this);
            return actor;
        }

        public override void OnAdded()
        {
            var aabb = new BoundingBox(-0.5f, -0.5f, -0.5f, 0.5f, 0.5f, 0.5f);
            BoundingBox.Merge(ref Host.LocalBoundingBox, ref aabb, out Host.LocalBoundingBox);
            OnUpdateDrawMatrix(ref Host.Placement.mDrawTransform);
            base.OnAdded();
        }

        public override async Task<bool> SetInitializer(CRenderContext rc, GamePlay.IEntity host, GamePlay.Component.IComponentContainer hostContainer, GComponentInitializer v)
        {
            if (!await base.SetInitializer(rc, host, hostContainer, v))
                return false;

            if(mLineMeshComponent == null)
            {
                await base.InitDrawMesh();
                mLineMeshComponent.Entity = host;

                var mat = Host.Placement.WorldMatrix;
                mLineMeshComponent.OnUpdateDrawMatrix(ref mat);
            }

            return true;
        }

        public override void OnAddedScene()
        {
            base.OnAddedScene();

            if(!Host.Scene.IsLoading)
            {
                if (mActorOctree.Parent != null)
                    mActorOctree.Parent.ChildrenNode.Remove(mActorOctree);

                Host.Scene.ChildrenNode.Add(mActorOctree);
                mActorOctree.Parent = Host.Scene;
                mActorOctree.HostSceneGraph = Host.Scene;
                mActorOctree.Name = "Octree_" + Host.PVSId;
            }
        }
        public override void OnRemoveScene()
        {
            base.OnRemoveScene();
            if(mActorOctree != null)
            {
                if (mActorOctree.Parent != null && !Host.Scene.IsLoading)
                {
                    MoveActorToDefaultSceneNode(mActorOctree.Root);

                    mActorOctree.Parent.ChildrenNode.Remove(mActorOctree);
                    if (mActorOctree.HostSceneGraph == mActorOctree.Parent)
                        mActorOctree.HostSceneGraph = null;
                    mActorOctree.Parent = null;
                }
            }
        }
        void MoveActorToDefaultSceneNode(GamePlay.SceneGraph.ISceneNode node)
        {
            if (node == null)
                return;
            var it = node.Actors.GetEnumerator();
            while(it.MoveNext())
            {
                var act = it.Current;
                GActor actor = act.Value;
                Host.Scene.DefaultSceneNode.AddActor(actor);
            }
            it.Dispose();

            foreach(var child in node.ChildrenNode)
            {
                MoveActorToDefaultSceneNode(child);
            }
        }
    }
}
