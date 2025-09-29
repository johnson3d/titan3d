using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.Collision.SDF
{
    public struct FPayload
    {
        public int BrickIndex;
    }
    public unsafe struct FBrick
    {
        public const int BrickSide = 4;
        public const float VoxelSize = 0.25f;
        public fixed bool IsOccupied[BrickSide * BrickSide * BrickSide];
        public bool IsOccupy(int x, int y, int z)
        {
            return IsOccupied[x + y * BrickSide + z * BrickSide * BrickSide];
        }
        public bool IsOccupy(Vector3 localPos, ref Vector3i OutSurfelIndex)
        {
            Vector3 idx = localPos / VoxelSize;
            OutSurfelIndex = new Vector3i((int)idx.X, (int)idx.Y, (int)idx.Z);
            return IsOccupy(OutSurfelIndex.X, OutSurfelIndex.Y, OutSurfelIndex.Z);
        }
        public int RayMarchSDF(TtSDF sdf, Vector3 rayOrigin, Vector3 rayDirection, ref Vector3i OutSurfelIndex)
        {
            var currentPos = rayOrigin;
            for (int i = 0; i<BrickSide*2; i++)
            {
                currentPos = currentPos + rayDirection * VoxelSize * i;
                if (IsOccupy(currentPos, ref OutSurfelIndex))
                    return i;
            }
            return 0;
        }
    }
    public struct FSdfVoxel
    {
        public float Distance;
        public int Payload;
        public FPayload GetPayload(TtSDF sdf)
        {
            if (Payload==-1)
                return new FPayload() { BrickIndex = 0 };
            return sdf.mPayloads[Payload];
        }
        public bool IsContain(Vector3 currentPos)
        {
            if (Payload==-1)
                return false;
            return true;
        }
    }
    public class TtSDF
    {
        public FSdfVoxel[,,] mHighSDF;
        public FPayload[] mPayloads;
        public FBrick[] mBricks;

        public Vector3 HighSDFWorldSize; // 高分辨率 SDF 的覆盖范围（世界空间）
        public Vector3 HighSDFVoxelSize;

        // Ray Marching 参数
        public float MaxDistance = 100f;    // 最大光线距离
        public float Epsilon = 0.01f;       // 命中阈值
        public float MinStepSize = 0.05f;   // 最小步长（避免无限小步长）

        public void BuildSDF()
        {
            mHighSDF = new FSdfVoxel[256, 256, 32];
        }

        /// <summary>
        /// 使用 Hierarchical SDF 进行光线步进
        /// </summary>
        /// <param name="rayOrigin">光线起点（世界坐标）</param>
        /// <param name="rayDirection">光线方向（归一化）</param>
        /// <returns>命中距离（未命中返回 -1）</returns>
        public float RayMarchSDF(Vector3 rayOrigin, Vector3 rayDirection, out int OutBrickIndex, ref Vector3i OutVoxelIndex)
        {
            OutBrickIndex = -1;
            float t = 0f; // 当前光线行进距离

            while (t < MaxDistance)
            {
                Vector3 currentPos = rayOrigin + rayDirection * t;

                Vector3i fineIndex;
                var fine = SampleSDF(mHighSDF, currentPos, HighSDFWorldSize, out fineIndex);
                var finePayload = fine.GetPayload(this);

                //// 命中表面
                //if (finePayload.Distance < Epsilon)
                //    return t;

                Vector3 brickStart;
                brickStart.X = HighSDFVoxelSize.X * fineIndex.X;
                brickStart.Y = HighSDFVoxelSize.Y * fineIndex.Y;
                brickStart.Z = HighSDFVoxelSize.Z * fineIndex.Z;
                var t1 = mBricks[finePayload.BrickIndex].RayMarchSDF(this, currentPos - brickStart, rayDirection, ref OutVoxelIndex);
                if (t1 > 0)
                {
                    OutBrickIndex = finePayload.BrickIndex;
                    return t + FBrick.VoxelSize * t1;
                }

                // 步进（避免步长过小）
                t += MathF.Max(fine.Distance, MinStepSize);
            }

            return -1f; // 未命中
        }
        protected FSdfVoxel SampleSDF(FSdfVoxel[,,] sdf, Vector3 worldPos, Vector3 sdfWorldSize, out Vector3i index)
        {
            // 将世界坐标转换为 SDF 纹理 UVW（0-1）
            Vector3 uvw = worldPos / sdfWorldSize;

            Vector3 sdfSize;
            sdfSize.Z = sdf.GetLength(0);
            sdfSize.Y = sdf.GetLength(1);
            sdfSize.X = sdf.GetLength(2);

            var idx = (uvw* sdfSize);
            index = new Vector3i((int)idx.X, (int)idx.Y, (int)idx.Z);

            return sdf[(int)idx.Z, (int)idx.Y, (int)idx.X];
        }
    }
    public class TtHierarchicalSDF : TtSDF
    {
        // SDF 数据（假设已经预计算）
        public FSdfVoxel[,,] mLowSDF;  // 低分辨率 SDF（粗粒度）
        // SDF 参数
        public Vector3 LowSDFWorldSize;  // 低分辨率 SDF 的覆盖范围（世界空间）
        
        public void BuildHSDF()
        {
            mLowSDF = new FSdfVoxel[64, 64, 16];
            mHighSDF = new FSdfVoxel[256, 256, 32];
        }

        /// <summary>
        /// 使用 Hierarchical SDF 进行光线步进
        /// </summary>
        /// <param name="rayOrigin">光线起点（世界坐标）</param>
        /// <param name="rayDirection">光线方向（归一化）</param>
        /// <returns>命中距离（未命中返回 -1）</returns>
        public float HierarchicalRayMarchSDF(Vector3 rayOrigin, Vector3 rayDirection, out int OutBrickIndex, ref Vector3i OutVoxelIndex)
        {
            OutBrickIndex = -1;
            float t = 0f; // 当前光线行进距离

            while (t < MaxDistance)
            {
                Vector3 currentPos = rayOrigin + rayDirection * t;

                Vector3i coarseIndex;
                // 1. 先查询低分辨率 SDF（粗粒度）
                var coarse = SampleSDF(mLowSDF, currentPos, LowSDFWorldSize, out coarseIndex);
                var coarsePayload = coarse.GetPayload(this);

                // 如果距离足够大，直接大步跳过
                if (coarse.Distance > Epsilon * 2f)
                {
                    t += coarse.Distance;
                    continue;
                }

                // 2. 如果进入高分辨率 SDF 范围，切换细粒度查询
                if (coarse.IsContain(currentPos))
                {
                    Vector3i fineIndex;
                    var fine= SampleSDF(mHighSDF, currentPos, HighSDFWorldSize, out fineIndex);
                    var finePayload = fine.GetPayload(this);

                    //// 命中表面
                    //if (finePayload.Distance < Epsilon)
                    //    return t;

                    Vector3 brickStart;
                    brickStart.X = HighSDFVoxelSize.X * fineIndex.X;
                    brickStart.Y = HighSDFVoxelSize.Y * fineIndex.Y;
                    brickStart.Z = HighSDFVoxelSize.Z * fineIndex.Z;
                    var t1 = mBricks[finePayload.BrickIndex].RayMarchSDF(this, currentPos - brickStart, rayDirection, ref OutVoxelIndex);
                    if (t1 > 0)
                    {
                        OutBrickIndex = finePayload.BrickIndex;
                        return t + FBrick.VoxelSize * t1;
                    }

                    // 步进（避免步长过小）
                    t += MathF.Max(fine.Distance, MinStepSize);
                }
                else
                {
                    // 继续用低分辨率 SDF 步进
                    t += coarse.Distance;
                }
            }

            return -1f; // 未命中
        }
    }
}
