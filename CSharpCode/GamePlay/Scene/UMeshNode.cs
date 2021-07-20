using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace EngineNS.GamePlay.Scene
{
    public partial class UMeshNode : UNode
    {
        public class UMeshNodeData : UNodeData
        {
            [Rtti.Meta]
            [RName.PGRName(FilterExts = Graphics.Mesh.UMaterialMesh.AssetExt)]
            public RName MeshName { get; set; }
            [Rtti.Meta]
            [RName.PGRName(FilterExts = Graphics.Mesh.UMaterialMesh.AssetExt)]
            public RName CollideName { get; set; }
            [Rtti.Meta]
            [ReadOnly(true)]
            public string MdfQueueType { get; set; } = Rtti.UTypeDesc.TypeStr(typeof(Graphics.Mesh.UMdfStaticMesh));
            [Rtti.Meta]
            [ReadOnly(true)]
            public string AtomType { get; set; } = Rtti.UTypeDesc.TypeStr(typeof(Graphics.Mesh.UMesh.UAtom));

            [EGui.Controls.PropertyGrid.PGTypeEditor(typeof(Graphics.Pipeline.Shader.UMdfQueue))]
            public Rtti.UTypeDesc MdfQueue
            {
                get
                {
                    return Rtti.UTypeDesc.TypeOf(MdfQueueType);
                }
                set
                {
                    MdfQueueType = Rtti.UTypeDesc.TypeStr(value);
                }
            }
            [EGui.Controls.PropertyGrid.PGTypeEditor(typeof(Graphics.Mesh.UMesh.UAtom))]
            public Rtti.UTypeDesc Atom
            {
                get
                {
                    return Rtti.UTypeDesc.TypeOf(AtomType);
                }
                set
                {
                    AtomType = Rtti.UTypeDesc.TypeStr(value);
                }
            }
        }
        public UMeshNode(UNodeData data, EBoundVolumeType bvType, Type placementType)
            : base(data, bvType, placementType)
        {
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
            if (mMesh == null)
                return;
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
        public static async System.Threading.Tasks.Task<UMeshNode> AddMeshNode(UNode parent, UNodeData data, Type placementType, Vector3 pos, Vector3 scale, Quaternion quat)
        {
            var meshData = data as UMeshNodeData;
            var materialMesh = await UEngine.Instance.GfxDevice.MaterialMeshManager.GetMaterialMesh(meshData.MeshName);
            if (materialMesh == null)
                return null;
            var mesh = new Graphics.Mesh.UMesh();
            
            var ok = mesh.Initialize(materialMesh, meshData.MdfQueue, meshData.Atom);
            if (ok == false)
                return null;

            var meshNode = AddMeshNode(parent, data, placementType, mesh, ref pos, ref scale, ref quat);
            if (meshData.CollideName != null)
            {
                var collideMesh = await UEngine.Instance.GfxDevice.MeshPrimitiveManager.GetMeshPrimitive(meshData.CollideName);
                if (collideMesh != null)
                {
                    if (collideMesh.MeshDataProvider == null)
                    {
                        await collideMesh.LoadMeshDataProvider();
                    }
                    meshNode.mMeshDataProvider = collideMesh.MeshDataProvider;
                }
            }
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
            base.OnNodeLoaded();

            UpdateAbsTransform();
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
                    var mesh = new Graphics.Mesh.UMesh();
                    mesh.Initialize(cookedMesh, materials1, Rtti.UTypeDescGetter<Graphics.Mesh.UMdfStaticMesh>.TypeDesc);
                    Mesh = mesh;
                };
                action();
                return;
            }
            else
            {
                System.Action action = async () =>
                {
                    var materialMesh = await UEngine.Instance.GfxDevice.MaterialMeshManager.GetMaterialMesh(meshData.MeshName);
                    if (materialMesh != null)
                    {
                        var mesh = new Graphics.Mesh.UMesh();
                        mesh.Initialize(materialMesh, Rtti.UTypeDesc.TypeOf(meshData.MdfQueueType), Rtti.UTypeDesc.TypeOf(meshData.AtomType));

                        Mesh = mesh;
                    }
                };
                action();
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
            mMesh.SetWorldMatrix(ref Placement.AbsTransformWithScale);
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

        Graphics.Mesh.CMeshDataProvider mMeshDataProvider;
        public Graphics.Mesh.CMeshDataProvider MeshDataProvider
        {
            get => mMeshDataProvider;
            set => mMeshDataProvider = value;
        }
        public unsafe override bool OnLineCheckTriangle(in Vector3 start, in Vector3 end, ref VHitResult result)
        {
            if (mMeshDataProvider == null)
                return false;

            fixed(Vector3* pStart = &start)
            fixed (Vector3* pEnd = &end)
            fixed (VHitResult* pResult = &result)
            {
                Vector3 scale = Placement.Scale;
                if (-1 != mMeshDataProvider.mCoreObject.IntersectTriangle(&scale, pStart, pEnd, pResult))
                    return true;
                return false;
            }
        }
    }
}
