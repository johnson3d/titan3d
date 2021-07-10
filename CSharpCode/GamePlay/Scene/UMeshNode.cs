using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.GamePlay.Scene
{
    public partial class UMeshNode : UNode
    {
        public class UMeshNodeData : UNodeData
        {
            [Rtti.Meta]
            public RName MeshName { get; set; }
            [Rtti.Meta]
            public string MdfQueueType { get; set; }
            [Rtti.Meta]
            public string AtomType { get; set; }
        }
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
        public override bool IsAcceptShadow
        {
            get
            {
                return base.IsAcceptShadow;
            }
            set
            {
                base.IsAcceptShadow = value;
                if (value == false)
                {
                    if (mMesh.MdfQueue.GetType() == typeof(Graphics.Mesh.UMdfStaticMesh))
                    {
                        mMesh.MdfQueueType = Rtti.UTypeDesc.TypeStr(typeof(Graphics.Mesh.UMdfStaticMesh_NoShadow));
                    }
                    else if (mMesh.MdfQueue.GetType() == typeof(Graphics.Mesh.UMdfTerrainMesh))
                    {
                        mMesh.MdfQueueType = Rtti.UTypeDesc.TypeStr(typeof(Graphics.Mesh.UMdfTerrainMesh_NoShadow));
                    }
                }
                else
                {
                    if (mMesh.MdfQueue.GetType() == typeof(Graphics.Mesh.UMdfStaticMesh_NoShadow))
                    {
                        mMesh.MdfQueueType = Rtti.UTypeDesc.TypeStr(typeof(Graphics.Mesh.UMdfStaticMesh));
                    }
                    else if (mMesh.MdfQueue.GetType() == typeof(Graphics.Mesh.UMdfTerrainMesh_NoShadow))
                    {
                        mMesh.MdfQueueType = Rtti.UTypeDesc.TypeStr(typeof(Graphics.Mesh.UMdfTerrainMesh));
                    }
                }
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

                var meshData = NodeData as UMeshNodeData;
                if (meshData != null)
                {
                    meshData.MeshName = mMesh.MaterialMesh.AssetName;
                    meshData.MdfQueueType = mMesh.MdfQueueType;
                    meshData.AtomType = Rtti.UTypeDesc.TypeStr(mMesh.Atoms[0].GetType());
                }
                UpdateAABB();
                Parent?.UpdateAABB();
            }
        }
        public override void OnNodeLoaded()
        {
            var meshData = NodeData as UMeshNodeData;
            if (meshData == null || meshData.MeshName == null)
            {
                System.Action action = async () =>
                {
                    var cookedMesh = Graphics.Mesh.CMeshDataProvider.MakeBoxWireframe(0, 0, 0, 5, 5, 5).ToMesh();
                    var materials1 = new Graphics.Pipeline.Shader.UMaterialInstance[1];
                    materials1[0] = await UEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(UEngine.Instance.Config.DefaultMaterialInstance);
                    var colorVar = materials1[0].FindVar("clr4_0");
                    if (colorVar != null)
                    {
                        colorVar.Value = "1,0,1,1";
                    }
                    mMesh = new Graphics.Mesh.UMesh();
                    mMesh.Initialize(cookedMesh, materials1, Rtti.UTypeDescGetter<Graphics.Mesh.UMdfStaticMesh>.TypeDesc);
                    
                    UpdateAABB();
                    UpdateAbsTransform();
                };
                action();
                return;
            }

            System.Action action1 = async () =>
            {
                var materialMesh = await UEngine.Instance.GfxDevice.MaterialMeshManager.GetMaterialMesh(meshData.MeshName);
                if (materialMesh != null)
                {
                    mMesh = new Graphics.Mesh.UMesh();
                    mMesh.Initialize(materialMesh, Rtti.UTypeDesc.TypeOf(meshData.MdfQueueType), Rtti.UTypeDesc.TypeOf(meshData.AtomType));

                    UpdateAABB();
                    UpdateAbsTransform();
                }
            };
            action1();
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
        static Macross.UMacrossBreak mTestBreak = new Macross.UMacrossBreak("UMeshNode.OnTickLogic", false);
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
