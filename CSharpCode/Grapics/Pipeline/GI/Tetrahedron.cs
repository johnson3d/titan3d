using System;
using System.Collections.Generic;
using System.Numerics;

namespace EngineNS.Graphics.Pipeline.GI
{
    //P = w0* P0 + w1* P1 + w2* P2 + w3* P3
    //w0 + w1 + w2 + w3 = 1

    public class TtTetrahedron
    {
        public int[] ProbeIndices = new int[4];
        public Vector3[] Vertices = new Vector3[4];
        public Matrix BarycentricMatrix;
        float Determinant; // 用于快速体积判断

        // 预计算所有需要的数据
        public void Precompute()
        {
            // 构建顶点矩阵
            Matrix vertexMatrix = new Matrix(
                Vertices[0].x, Vertices[1].x, Vertices[2].x, Vertices[3].x,
                Vertices[0].y, Vertices[1].y, Vertices[2].y, Vertices[3].y,
                Vertices[0].z, Vertices[1].z, Vertices[2].z, Vertices[3].z,
                1.0f, 1.0f, 1.0f, 1.0f
            );

            // 计算行列式（用于判断四面体方向和体积）
            Determinant = vertexMatrix.Determinant();

            // 如果行列式为0，说明4点共面，这个四面体无效
            if (Math.Abs(Determinant) < 1e-6f)
            {
                // 处理退化情况
                BarycentricMatrix = Matrix.Identity;
                return;
            }

            // 计算逆矩阵
            vertexMatrix.Inverse();
            BarycentricMatrix = vertexMatrix;
        }

        // 运行时使用
        public static bool IsPointInsideTetrahedron(in Vector3 worldPos, TtTetrahedron tetra)
        {
            // 1. 转换到重心坐标
            Vector4 baryCoords = GetBarycentricCoords(in worldPos, tetra);
            // 2. 检查是否在四面体内部
            // 所有重心坐标都应该 >= 0
            return (baryCoords.X >= -1e-6f &&
                    baryCoords.Y >= -1e-6f &&
                    baryCoords.Z >= -1e-6f &&
                    baryCoords.W >= -1e-6f);
        }

        // 获取重心坐标用于插值
        public static Vector4 GetBarycentricCoords(in Vector3 worldPos, in TtTetrahedron tetra)
        {
            Vector4 homogeneousPos = new Vector4(worldPos.x, worldPos.y, worldPos.z, 1.0f);
            return Vector4.Transform(homogeneousPos, tetra.BarycentricMatrix);
        }
    }

}
