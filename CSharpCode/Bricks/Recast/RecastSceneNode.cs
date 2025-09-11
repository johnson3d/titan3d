using EngineNS.Bricks.NodeGraph;
using EngineNS.Bricks.PhysicsCore;
using EngineNS.GamePlay;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace EngineNS.Bricks.Recast
{
    [Bricks.CodeBuilder.ContextMenu("Recast", "Recast", GamePlay.Scene.TtNode.EditorKeyword)]
    [GamePlay.Scene.TtNode(NodeDataType = typeof(TtRecastSceneNode.TtRecastSceneNodeData), DefaultNamePrefix = "Recast")]
    public class TtRecastSceneNode : GamePlay.Scene.TtVisual
    {
        public class TtRecastSceneNodeData : GamePlay.Scene.TtNodeData
        {
            Guid mNodeId = Guid.NewGuid();
            [Rtti.Meta("")]
            public Guid NodeId
            {
                get => mNodeId;
                set { mNodeId = value; }
            }
        }
        public override void Dispose()
        {
            CoreSDK.DisposeObject(ref mDebugMesh);
            CoreSDK.DisposeObject(ref mDebugMeshWireFrame);
            CoreSDK.DisposeObject(ref mNavMesh); 
            base.Dispose();
        }
        class NavMeshAttribute : EGui.Controls.PropertyGrid.PGCustomValueEditorAttribute
        {
            public NavMeshAttribute()
            {
                
            }
            public override bool OnDraw(in EditorInfo info, out object newValue)
            {
                newValue = info.Value;
                if (ImGuiAPI.Button("BuildNavMesh", in Vector2.Zero))
                {
                    //var lst = info.ObjectInstance as List<TtRecastSceneNode>;
                    //if (lst != null)
                    //{
                    //    foreach (var i in lst)
                    //    {
                    //        i.BuildNavMesh();
                    //    }
                    //}   
                    var lst = info.ObjectInstance as System.Collections.IList;
                    if (lst != null)
                    {
                        foreach (var i in lst)
                        {
                            var node = i as TtRecastSceneNode;
                            if (node != null)
                            {
                                node.BuildNavMesh();
                            }
                        }
                    }
                }
                return false;
            }
        }
        [NavMeshAttribute]
        [Category("Option")]
        public bool NavMeshBuilder
        {
            get => true;
        }
        public TtNavMesh NavMesh
        {
            get => mNavMesh;
        }
        Guid mNodeId = Guid.NewGuid();
        [Category("Option")]
        public override Guid NodeId
        {
            get
            {
                if (GetNodeData<TtRecastSceneNodeData>() == null)
                    return Guid.Empty;
                return GetNodeData<TtRecastSceneNodeData>().NodeId;
            }
            set
            {
                if (GetNodeData<TtRecastSceneNodeData>() == null)
                    return;

                mNodeId = value; 
            }
        }
        public bool BuildNavMesh()
        {
            var world = this.GetWorld();
            TtTileMeshBuilder ttTileMeshBuilder = new TtTileMeshBuilder();
            TtInputGeom ttInputGeom = new TtInputGeom();
            var rp = new TtWorld.TtVisParameter();
            rp.CullFilters = TtWorld.TtVisParameter.EVisCullFilter.None;
            rp.CullCamera = null;

            var hs = this.Placement.Scale * 0.5f;
            var box = new Aabb(this.Placement.Position, hs);
            rp.World = world;
            rp.CullBox = box.AsBoundingBox();
            rp.IsGatherVisibleNodes = false;
            rp.IsGatherVisibleMeshes = (node, arg) =>
            {
                if (node.IsBuildNavMesh)
                {
                    return true;
                }
                return false;
            };
            world.GatherVisibleMeshes(rp);
            var wholeMdp = new Graphics.Mesh.TtMeshDataProvider();
            wholeMdp.Init((1 << (int)NxRHI.EVertexStreamType.VST_Position), true, 0);
            foreach (var i in rp.VisibleMeshes)
            {
                foreach (var j in i.Mesh.MaterialMesh.SubMeshes) 
                {
                    var mdp = new Graphics.Mesh.TtMeshDataProvider();
                    mdp.InitFrom(j.Mesh);
                    wholeMdp.MergeFromMesh(mdp, i.Mesh.GetWorldMatrix());
                }
            }
            ttInputGeom.LoadMesh(wholeMdp, 1.0f);
            ttTileMeshBuilder.SetInputGeom(ttInputGeom);
            CoreSDK.DisposeObject(ref mDebugMesh);
            CoreSDK.DisposeObject(ref mDebugMeshWireFrame);
            mNavMesh = ttTileMeshBuilder.BuildNavi();

            using (var xnd = new IO.TtXndHolder(this.NodeName, 0, 0))
            {
                mNavMesh.Save2Xnd(xnd.RootNode);
                xnd.SaveXnd(world.Root.AssetName.Address + $"/nodes/{this.NodeId}.node");
            }   

            return true;
        }
        public TtNavMesh mNavMesh;
        public Graphics.Mesh.TtRenderMesh mDebugMesh;
        public Graphics.Mesh.TtRenderMesh mDebugMeshWireFrame;
        public override void OnGatherVisibleMeshes(TtWorld.TtVisParameter rp)
        {
            if ((rp.CullFilters & GamePlay.TtWorld.TtVisParameter.EVisCullFilter.NavMesh) == 0)
                return;

            var hs = this.Placement.Scale * 0.5f;
            var box = new Aabb(DVector3.Zero, hs);
            rp.AddAABB(box, Color4f.FromColor4b(Color4b.Yellow), in this.Placement.TransformRef);
            if (mNavMesh == null)
                return;
            if (mDebugMesh == null)
                mDebugMesh = mNavMesh.CreateRenderMesh().ToDrawMesh(TtEngine.Instance.GfxDevice.MaterialManager.NavMeshDebugMaterial);
            if (mDebugMeshWireFrame == null)
                mDebugMeshWireFrame = mNavMesh.CreateRenderMesh().ToDrawMesh(TtEngine.Instance.GfxDevice.MaterialManager.NavMeshDebugWireMaterial);
            rp.AddVisibleMesh(mDebugMesh);
            rp.AddVisibleMesh(mDebugMeshWireFrame);
            base.OnGatherVisibleMeshes(rp);
        }
        protected override void OnAbsTransformChanged()
        {
            var hs = Placement.Scale * 0.5f;
            BoundVolume.mLocalAABB.Minimum = -hs;
            BoundVolume.mLocalAABB.Maximum = hs;
            base.OnAbsTransformChanged();
        }
    }
}
