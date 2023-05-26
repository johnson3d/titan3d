using EngineNS.Bricks.Collision.DDA;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.Bricks.Collision.DDA
{
    public class TtVoxelPayload
    {
        
    }
    public class TtVoxel
    {
        private uint Flags = 0;
        private TtVoxelPayload mPayload;
        public TtVoxelPayload Payload { get => mPayload; }
        private const uint XMask = 0x3ff;
        private const uint YMask = XMask << 10;
        private const uint ZMask = YMask << 10;
        private const uint PayloadMask = (1u << 31);
        public Vector3i Index
        {
            get => GetVoxelIndex();
        }
        public Vector3i GetVoxelIndex()
        {
            Vector3i result;
            result.X = (int)(Flags & XMask);
            result.Y = (int)((Flags & YMask) >> 10);
            result.Z = (int)((Flags & ZMask) >> 20);
            return result;
        }
        public void SetVoxelIndex(uint x, uint y, uint z)
        {
            //Flags =  | (x & XMask);
            Flags = (x & XMask) | ((y & XMask) << 10) | ((z & XMask) << 20);
        }
        public void SetPayload(TtVoxelPayload payload)
        {
            mPayload = payload;
            if (payload != null)
                Flags |= PayloadMask;
            else
                Flags = (Flags & (~PayloadMask));
        }
        public bool HasPayload()
        {
            return (Flags & PayloadMask) != 0;
        }
    }

    public class TtVoxelSpace3D
    {
        public TtHierarchicalVoxelSpace3D HVX;
        public int CheckCount = 0;
        public TtVoxel[,,] Voxels { get; private set; }
        public Vector3 Side;
        public float CellSide = 2;
        public float VoxelSize { get; set; } = 1.0f;
        
        private int MaxStep = 0;
        public TtVoxel GetVoxel(in Vector3 index)
        {
            return Voxels[(int)index.X, (int)index.Y, (int)index.Z];
        }
        public TtVoxelSpace3D(int xSize, int ySize, int zSize)
        {
            Side.X = (int)xSize;
            Side.Y = (int)ySize;
            Side.Z = (int)zSize;
            Voxels = new TtVoxel[xSize, ySize, zSize];
            //temp code 
            MaxStep = (int)(xSize + ySize + zSize);
            for (int z = 0; z < Voxels.GetLength(0); z++)
            {//z
                for (int y = 0; y < Voxels.GetLength(1); y++)
                {//y
                    for (int x = 0; x < Voxels.GetLength(2); x++)
                    {//x
                        Voxels[x, y, z] = new TtVoxel();
                        Voxels[x, y, z].SetVoxelIndex((uint)x, (uint)y, (uint)z);
                    }
                }
            }
        }
        
        public TtVoxelSpace3D Clone()
        {
            var result = new TtVoxelSpace3D((int)Side.X, (int)Side.Y, (int)Side.Z);
            result.CellSide = CellSide;
            result.VoxelSize = VoxelSize;
            for (int z = 0; z < Voxels.GetLength(0); z++)
            {//z
                for (int y = 0; y < Voxels.GetLength(1); y++)
                {//y
                    for (int x = 0; x < Voxels.GetLength(2); x++)
                    {
                        if (Voxels[x, y, z].Payload != null)
                        {
                            result.Voxels[x, y, z].SetPayload(Voxels[x, y, z].Payload);
                        }
                    }
                }
            }
            return result;
        }
        public void InjectRandomVoxels(float rate)
        {
            for (int z = 0; z < Voxels.GetLength(0); z++)
            {//z
                for (int y = 0; y < Voxels.GetLength(1); y++)
                {//y
                    for (int x = 0; x < Voxels.GetLength(2); x++)
                    {
                        if (MathHelper.RandomDouble() < rate)
                        {
                            Voxels[x, y, z].SetPayload(new TtVoxelPayload());
                        }
                    }
                }
            }
        }
        public bool LineCheck(ref Vector3 start, in Vector3 end, in Vector3 dir, ref float t, out Vector3 hitIndex)
        {//https://www.researchgate.net/publication/233899848_Efficient_implementation_of_the_3D-DDA_ray_traversal_algorithm_on_GPU_and_its_application_in_radiation_dose_calculation
            hitIndex = Vector3.MinusOne;
            var curIndex = GetVoxelIndex(in start);
            var cell = new Vector3(CellSide);
            var offsetInCell = Vector3.Mod(in curIndex, in cell);

            Vector3 step = Vector3.One;
            if (dir.X < 0)
                step.X = -1.0f;
            if (dir.Y < 0)
                step.Y = -1.0f;
            if (dir.Z < 0)
                step.Z = -1.0f;

            const float MaxInvDirValue = 10000000;
            Vector3 deltaT;
            if (dir.X != 0)
                deltaT.X = Math.Abs(VoxelSize / dir.X);
            else
                deltaT.X = MaxInvDirValue;
            if (dir.Y != 0)
                deltaT.Y = Math.Abs(VoxelSize / dir.Y);
            else
                deltaT.Y = MaxInvDirValue;
            if (dir.Z != 0)
                deltaT.Z = Math.Abs(VoxelSize / dir.Z);
            else
                deltaT.Z = MaxInvDirValue;

            //deltaT代表xyz轴方向要走一个格子，射线需要走的距离
            //T代表到下一个xyz边界射线要走的距离=deltaT * 格子数
            var T = Vector3.Zero;
            T = ((curIndex + 0.5f - start / VoxelSize) * step + 0.5f) * deltaT;

            bool IsHit = false;
            for (int i = 0; (!IsHit) && (i < MaxStep); i++)
            {
                if (Vector3.Less(in curIndex, in Vector3.Zero).Any())
                    break;
                if (Vector3.GreatEqual(curIndex, Side).Any())
                    break;
                HVX.CheckCount++;
                CheckCount++;
                IsHit = GetVoxel(in curIndex).HasPayload();
                if (IsHit)
                {
                    hitIndex = curIndex;
                    return true;
                }
                if (Vector3.Less(in offsetInCell, in Vector3.Zero).Any() ||
                    Vector3.GreatEqual(in offsetInCell, in cell).Any())
                {
                    break;
                }

                var vxIncr = GetStepDir(in T, ref t);
                T += vxIncr * deltaT;
                curIndex += vxIncr * step;

                offsetInCell += vxIncr * step;
            }

            return IsHit;
        }
        private Vector3 GetStepDir(in Vector3 T, ref float t)
        {
            //选择距离下一个边界最近的距离方向
            var dirMask = new Vector3(
                ((T.X <= T.Y) && (T.X <= T.Z)) ? 1 : 0,
                ((T.Y < T.Z) && (T.Y <= T.X)) ? 1 : 0,
                ((T.Z < T.X) && (T.Z <= T.Y)) ? 1 : 0);

            t = dirMask.X > 0 ? T.X : (dirMask.Y > 0 ? T.Y : T.Z);
            return dirMask;
        }
        private Vector3 GetVoxelIndex(in Vector3 pos)
        {
            Vector3 t = pos / VoxelSize;
            return new Vector3(MathHelper.Floor(t.X), MathHelper.Floor(t.Y), MathHelper.Floor(t.Z));
        }
    }
}

