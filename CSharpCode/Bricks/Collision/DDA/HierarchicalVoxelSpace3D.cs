using EngineNS.Bricks.Collision.DDA;
using EngineNS.GamePlay;
using EngineNS.GamePlay.Scene;
using MathNet.Numerics.LinearAlgebra.Factorization;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.Bricks.Collision.DDA
{
    [Bricks.CodeBuilder.ContextMenu("HVX", "HVX", UNode.EditorKeyword)]
    [UNode(NodeDataType = typeof(UNodeData), DefaultNamePrefix = "HVX")]
    public class TtHierarchicalVoxelSpace3D : GamePlay.Scene.USceneActorNode
    {
        public override async Thread.Async.TtTask<bool> InitializeNode(UWorld world, UNodeData data, EBoundVolumeType bvType, Type placementType)
        {
            await base.InitializeNode(world, data, bvType, placementType);
            
            var vxSpace = new TtVoxelSpace3D(256, 64, 256);
            vxSpace.InjectRandomVoxels(0.005f);
            CreateHVS(vxSpace, 4);

            await CreateDebugMesh(world);

            //var SpaceSize = new Vector3(vxSpace.Side.X * vxSpace.VoxelSize, vxSpace.Side.Y * vxSpace.VoxelSize, vxSpace.Side.Z * vxSpace.VoxelSize);
            //var start = MathHelper.RandomDirection(false) * SpaceSize;
            //var end = MathHelper.RandomDirection(false) * SpaceSize;
            //await SetDebugLine(world, start, end);
            return true;
        }
        protected override void OnParentChanged(UNode prev, UNode cur)
        {
            
        }
        public TtVoxelSpace3D[] MipLayers = null;
        public DVector3 StartPosition { get; set; }
        public void CreateHVS(TtVoxelSpace3D detailLayer, int numOfLayer)
        {
            MipLayers = new TtVoxelSpace3D[numOfLayer];
            detailLayer.HVX = this;
            MipLayers[numOfLayer - 1] = detailLayer;
            for (int i = numOfLayer - 1 - 1; i>=0; i--)
            {
                var cur = MipLayers[i + 1];
                MipLayers[i] = new TtVoxelSpace3D((int)cur.Side.X / 2, (int)cur.Side.Y / 2, (int)cur.Side.Z / 2);
                MipLayers[i].HVX = this;
                UpdateHVS(cur, MipLayers[i]);
            }
            MipLayers[0].CellSide = MipLayers[0].Side.GetMaxValue();
        }
        public static void UpdateHVS(TtVoxelSpace3D high, TtVoxelSpace3D low)
        {
            low.VoxelSize = high.VoxelSize * 2;
            for (int i = 0; i < high.Side.Z; i++)
            {//z
                for (int j = 0; j < high.Side.Y; j++)
                {//y
                    for (int k = 0; k < high.Side.X; k++)
                    {//x
                        var vx = high.Voxels[i, j, k];
                        if (vx.HasPayload())
                        {
                            low.Voxels[i / 2, j / 2, k / 2].SetPayload(vx.Payload);
                        }
                    }
                }
            }
        }
        public bool LineCheck(in DVector3 start, in DVector3 end, out Vector3 hitIndex)
        {
            var sStart = (start - StartPosition).ToSingleVector3();
            //todo:fix pos to space box
            var dir = (end - start).ToSingleVector3();
            var sEnd = sStart + dir;
            dir.Normalize();
            return LineCheck(ref sStart, in sEnd, in dir, out hitIndex);
        }
        public int CheckCount = 0;
        public bool LineCheck(ref Vector3 sStart, in Vector3 sEnd, in Vector3 dir, out Vector3 hitIndex)
        {
            CheckCount = 0;
            foreach(var i in MipLayers)
            {
                i.CheckCount = 0;
            }
            hitIndex = Vector3.MinusOne;

            int curLayer = 0;
            Vector3 tHitIndex;
            float t = 0;
            while (true)
            {
                var ret = MipLayers[curLayer].LineCheck(ref sStart, in sEnd, in dir, ref t, out tHitIndex);
                t += 0.1f;
                sStart = sStart + dir * t;
                if (ret)
                {
                    curLayer++;
                    if (curLayer == MipLayers.Length)
                    {
                        hitIndex = tHitIndex;
                        return true;
                    }
                }
                else
                {
                    if (curLayer == 0)
                        return false;
                    curLayer--;
                }
            }
        }
        public TtVoxelSpace3D DetailLayer
        {
            get
            {
                return MipLayers[MipLayers.Length - 1];
            }
        }
        public Graphics.Mesh.TtMesh VxDebugMesh;
        public UMeshNode HVXDebugNode;
        public UMeshNode HVXDebugLineNode;
        public UMeshNode HVXDebugHitNode;
        public Graphics.Mesh.UMdfInstanceStaticMesh DebugMeshInstanceMdf
        {
            get
            {
                if (VxDebugMesh == null)
                    return null;
                return VxDebugMesh.MdfQueue as Graphics.Mesh.UMdfInstanceStaticMesh;
            }
        }

        public async System.Threading.Tasks.Task CreateDebugMesh(GamePlay.UWorld world)
        {
            var material = await TtEngine.Instance.GfxDevice.MaterialInstanceManager.CreateMaterialInstance(RName.GetRName("utest/box_wite.uminst"));
            VxDebugMesh = new Graphics.Mesh.TtMesh();
            var rect = Graphics.Mesh.UMeshDataProvider.MakeBox(-0.5f, -0.5f, -0.5f, 1, 1, 1, 0xffff00ff);
            var rectMesh = rect.ToMesh();
            var materials = new Graphics.Pipeline.Shader.TtMaterial[1];
            materials[0] = material;
            VxDebugMesh.Initialize(rectMesh, materials, Rtti.UTypeDescGetter<Graphics.Mesh.UMdfInstanceStaticMesh>.TypeDesc);
            VxDebugMesh.MdfQueue.MdfDatas = this;

            var meshNode = await GamePlay.Scene.UMeshNode.AddMeshNode(world, world.Root, new GamePlay.Scene.UMeshNode.UMeshNodeData(), typeof(GamePlay.UPlacement), VxDebugMesh, DVector3.Zero, Vector3.One, Quaternion.Identity);
            meshNode.SetStyle(GamePlay.Scene.UNode.ENodeStyles.VisibleFollowParent);
            meshNode.NodeData.Name = "Debug_HVXMeshNode";
            meshNode.IsAcceptShadow = false;
            meshNode.IsCastShadow = false;
            meshNode.HitproxyType = Graphics.Pipeline.UHitProxy.EHitproxyType.None;

            HVXDebugNode = meshNode;

            var Voxels = DetailLayer.Voxels;
            var maxSize = Voxels.GetLength(0) * Voxels.GetLength(1) * Voxels.GetLength(2);
            DebugMeshInstanceMdf.InstanceModifier.SetCapacity((uint)maxSize, false);
            
            for (int z = 0; z < Voxels.GetLength(0); z++)
            {//z
                for (int y = 0; y < Voxels.GetLength(1); y++)
                {//y
                    for (int x = 0; x < Voxels.GetLength(2); x++)
                    {//x
                        if (Voxels[x, y, z].Payload != null)
                        {
                            var pos = new Vector3(x, y, z);
                            pos += new Vector3(0.5f);
                            //DebugMeshInstanceMdf.InstanceModifier.PushInstance(in pos, in Vector3.One, in Quaternion.Identity, in Vector4ui.Zero, meshNode.HitProxy.ProxyId);

                            var instance = new Graphics.Mesh.Modifier.FVSInstanceData();
                            instance.Position = pos;
                            instance.Scale = Vector3.One;
                            instance.Quat = Quaternion.Identity;

                            DebugMeshInstanceMdf.InstanceModifier.PushInstance(in instance, new Graphics.Mesh.Modifier.FCullBounding());
                        }
                    }
                }
            }
        }
        public async System.Threading.Tasks.Task SetDebugLine(GamePlay.UWorld world, Vector3 from, Vector3 to)
        {
            if (HVXDebugLineNode != null)
            {
                HVXDebugLineNode.Parent = null;
                HVXDebugLineNode = null;
            }
            
            if (HVXDebugHitNode != null)
            {
                HVXDebugHitNode.Parent = null;
                HVXDebugHitNode = null;
            }

            var material = await TtEngine.Instance.GfxDevice.MaterialInstanceManager.CreateMaterialInstance(RName.GetRName("utest/box_wite.uminst"));
            var materials = new Graphics.Pipeline.Shader.TtMaterial[1];
            materials[0] = material;

            Vector3 hitResult;
            var dir = to - from;
            dir.Normalize();
            var lineFrom = from;
            var lineTo = to;
            if (this.LineCheck(ref from, in to, in dir, out hitResult))
            {
                lineTo = from;

                var sphereDebugMesh = new Graphics.Mesh.TtMesh();
                var sphere = Graphics.Mesh.UMeshDataProvider.MakeSphere(0.3f, 8, 8, 0xFFFFFF00);
                var sphereMesh = sphere.ToMesh();
                sphereDebugMesh.Initialize(sphereMesh, materials, Rtti.UTypeDescGetter<Graphics.Mesh.UMdfStaticMesh>.TypeDesc);
                sphereDebugMesh.MdfQueue.MdfDatas = this;

                var meshNode1 = await GamePlay.Scene.UMeshNode.AddMeshNode(world, this, new GamePlay.Scene.UMeshNode.UMeshNodeData(), typeof(GamePlay.UPlacement), sphereDebugMesh, DVector3.Zero, Vector3.One, Quaternion.Identity);
                meshNode1.SetStyle(GamePlay.Scene.UNode.ENodeStyles.VisibleFollowParent);
                meshNode1.NodeData.Name = "Debug_HVXHitNode";
                meshNode1.IsAcceptShadow = false;
                meshNode1.IsCastShadow = false;
                meshNode1.HitproxyType = Graphics.Pipeline.UHitProxy.EHitproxyType.None;

                meshNode1.Placement.Position = lineTo.AsDVector();

                HVXDebugHitNode = meshNode1;
            }
            var lineMesh = new Graphics.Mesh.TtMesh();
            var rect = Graphics.Mesh.UMeshDataProvider.MakeLine(in lineFrom, in lineTo, 0xFF50ff80);
            var rectMesh = rect.ToMesh();
            lineMesh.Initialize(rectMesh, materials, Rtti.UTypeDescGetter<Graphics.Mesh.UMdfStaticMesh>.TypeDesc);
            lineMesh.MdfQueue.MdfDatas = this;

            var meshNode = await GamePlay.Scene.UMeshNode.AddMeshNode(world, this, new GamePlay.Scene.UMeshNode.UMeshNodeData(), typeof(GamePlay.UPlacement), lineMesh, DVector3.Zero, Vector3.One, Quaternion.Identity);
            meshNode.SetStyle(GamePlay.Scene.UNode.ENodeStyles.VisibleFollowParent);
            meshNode.NodeData.Name = "Debug_HVXLineNode";
            meshNode.IsAcceptShadow = false;
            meshNode.IsCastShadow = false;
            meshNode.HitproxyType = Graphics.Pipeline.UHitProxy.EHitproxyType.None;

            HVXDebugLineNode = meshNode;
        }
        public override void OnCommand(object cmd)
        {
            var vxSpace = DetailLayer;
            if (vxSpace == null)
                return;
            var SpaceSize = new Vector3(vxSpace.Side.X * vxSpace.VoxelSize, vxSpace.Side.Y * vxSpace.VoxelSize, vxSpace.Side.Z * vxSpace.VoxelSize);
            var start = MathHelper.RandomDirection(false) * SpaceSize;
            var end = MathHelper.RandomDirection(false) * SpaceSize;
            var nu = SetDebugLine(this.GetWorld(), start, end);
        }
    }
}

namespace EngineNS.UTest
{
    [UTest.UTest(Enable = false)]
    public partial class UTest_VoxelSpace3D
    {
        public void UnitTestEntrance()
        {
            var hvxSpace = new TtHierarchicalVoxelSpace3D();
            var vxSpace = new TtVoxelSpace3D(256, 64, 256);
            vxSpace.InjectRandomVoxels(0.005f);
            hvxSpace.CreateHVS(vxSpace.Clone(), 4);
            vxSpace.CellSide = vxSpace.Side.GetMaxValue();
            int hit = 0;
            int hit1 = 0;
            for (int i = 0; i < 256; i++)
            {
                var SpaceSize = new Vector3(vxSpace.Side.X * vxSpace.VoxelSize, vxSpace.Side.Y * vxSpace.VoxelSize, vxSpace.Side.Z * vxSpace.VoxelSize);
                var start = MathHelper.RandomDirection(false) * SpaceSize;
                var end = MathHelper.RandomDirection(false) * SpaceSize;
                var dir = end - start;
                dir.Normalize();

                var start1 = start;
                Vector3 result;
                float t = 0;
                if (vxSpace.LineCheck(ref start1, in end, in dir, ref t, out result))
                    hit++;
                start1 = start1 + dir * t;

                var start2 = start;
                if (hvxSpace.LineCheck(ref start2, in end, in dir, out result))
                {
                    hit1++;
                }
                if (hit1 != hit)
                    continue;
            }
            if (hit1 != hit)
                return;
        }
    }
}
