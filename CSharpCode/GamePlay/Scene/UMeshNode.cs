using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.GamePlay.Scene
{
    public partial class UMeshNode : UNode
    {
        public UMeshNode(UNodeData data, EBoundVolumeType bvType, Type placementType)
            : base(data, bvType, placementType)
        {
            this.SetStyle(ENodeStyles.VisibleMeshProvider | ENodeStyles.VisibleFollowParent);
        }
        public override void GetDrawMesh(List<Graphics.Mesh.UMesh> meshes)
        {
            meshes.Add(mMesh);
            foreach(var i in Children)
            {
                if(i.HitproxyType == Graphics.Pipeline.UHitProxy.EHitproxyType.FollowParent)
                    i.GetDrawMesh(meshes);
            }
        }
        public override void OnHitProxyChanged()
        {
            if (this.HitProxy == null)
            {
                mMesh.IsDrawHitproxy = false;
                return;
            }

            if (HitproxyType != Graphics.Pipeline.UHitProxy.EHitproxyType.None)
            {
                mMesh.IsDrawHitproxy = true;
                var value = HitProxy.ConvertHitProxyIdToVector4();
                mMesh.SetHitproxy(ref value);
            }
            else
            {
                mMesh.IsDrawHitproxy = false;
            }
        }
        public static UMeshNode AddMeshNode(UNode parent, UNodeData data, Type placementType, Graphics.Mesh.UMesh mesh, Vector3 pos, Vector3 scale, Quaternion quat)
        {
            return AddMeshNode(parent, data, placementType, mesh, ref pos, ref scale, ref quat);
        }
        public static UMeshNode AddMeshNode(UNode parent, UNodeData data, Type placementType, Graphics.Mesh.UMesh mesh, ref Vector3 pos, ref Vector3 scale, ref Quaternion quat)
        {
            var scene = parent.GetNearestParentScene();
            var meshNode = scene.NewNode(typeof(UMeshNode), data, EBoundVolumeType.Box, placementType) as UMeshNode;
            if (mesh.MaterialMesh.Mesh.AssetName != null)
                meshNode.NodeData.Name = mesh.MaterialMesh.Mesh.AssetName.Name;
            else
                meshNode.NodeData.Name = meshNode.Id.ToString();
            meshNode.Mesh = mesh;
            meshNode.Parent = parent;
            
            meshNode.Placement.SetTransform(ref pos, ref scale, ref quat);

            return meshNode;
        }
        public UBoxBV GetBoxBV()
        {
            return BoundVolume as UBoxBV;
        }
        Graphics.Mesh.UMesh mMesh;
        public Graphics.Mesh.UMesh Mesh 
        {
            get 
            {
                return mMesh;
            }
            set
            {
                if (mMesh != null)
                {
                    mMesh.HostNode = null;
                }
                
                mMesh = value;

                if (mMesh != null)
                {
                    BoundVolume.LocalAABB = mMesh.MaterialMesh.Mesh.mCoreObject.mAABB;
                    mMesh.HostNode = this;
                }
                else
                    BoundVolume.LocalAABB.InitEmptyBox();
                UpdateAABB();
                Parent?.UpdateAABB();
            }
        }
        public override void OnGatherVisibleMeshes(UWorld.UVisParameter rp)
        {
            rp.VisibleMeshes.Add(mMesh);
        }
        protected override void OnAbsTransformChanged()
        {
            if (mMesh == null)
                return;
            mMesh.SetWorldMatrix(ref Placement.AbsTransform);
        }
        static Macross.UMacrossStackFrame mLogicTickFrame = new Macross.UMacrossStackFrame();
        static Macross.UMacrossBreak mTestBreak = new Macross.UMacrossBreak("UMeshNode.OnTickLogic", true);
        public override bool OnTickLogic()
        {
            using (var guard = new Macross.UMacrossStackGuard(mLogicTickFrame))
            {
                mTestBreak.TryBreak();
            }
                
            return true;
        }
    }
}
