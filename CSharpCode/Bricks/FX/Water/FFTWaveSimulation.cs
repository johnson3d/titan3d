using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.FX.Water
{
    using MathNet.Numerics;
    using MathNet.Numerics.IntegralTransforms;
    using Org.BouncyCastle.Tsp;

    public class TtFftWaveSimulation
    {
        // 模拟参数
        private int N;                      // 网格分辨率 (N x N)
        private int Rows, Cols;             // 行数和列数
        private float L;                    // 物理域大小 (米)
        private float Dt;                   // 时间步长 (秒)
        private float G = 9.81f;            // 重力加速度 (m/s²)

        // 数据存储（使用一维数组）
        private System.Numerics.Complex[] HeightFieldFrequency;  // 频率域高度场（一维）
        private float[,] HeightFieldSpatial;     // 空间域高度场（二维，便于访问）
        private System.Numerics.Complex[] TempComplexArray;      // 临时复数数组用于FFT

        // 波数网格（仍使用二维便于理解）
        private float[,] Kx;    // X方向波数
        private float[,] Kz;    // Z方向波数
        private float[,] K;     // 波数幅值
        private float[,] Omega; // 角频率

        // 随机数生成
        private Random mRandom = new Random();

        /// <summary>
        /// 初始化海浪模拟
        /// </summary>
        public TtFftWaveSimulation(int resolution, float domainSize, float timeStep)
        {
            N = resolution;
            Rows = N;
            Cols = N;
            L = domainSize;
            Dt = timeStep;

            // 分配内存
            HeightFieldFrequency = new System.Numerics.Complex[Rows * Cols];
            HeightFieldSpatial = new float[Rows, Cols];
            TempComplexArray = new System.Numerics.Complex[Rows * Cols];

            Kx = new float[Rows, Cols];
            Kz = new float[Rows, Cols];
            K = new float[Rows, Cols];
            Omega = new float[Rows, Cols];

            // 初始化波数网格
            InitializeWaveNumbers();

            // 初始化海浪频谱
            InitializeWaveSpectrum();
        }

        /// <summary>
        /// 初始化波数网格
        /// </summary>
        private void InitializeWaveNumbers()
        {
            float dk = 2.0f * MathF.PI / L;

            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Cols; j++)
                {
                    // 计算波数 (注意处理N/2处的折叠)
                    float ki = (i < Rows/2) ? i * dk : (i - Rows) * dk;
                    float kj = (j < Cols/2) ? j * dk : (j - Cols) * dk;

                    Kx[i, j] = ki;
                    Kz[i, j] = kj;

                    // 计算波数幅值
                    float kLen = MathF.Sqrt(ki * ki + kj * kj);
                    K[i, j] = kLen;

                    // 计算角频率 (深水波色散关系)
                    Omega[i, j] = (kLen > 0.0001f) ? MathF.Sqrt(G * kLen) : 0;
                }
            }
        }

        /// <summary>
        /// 使用Phillips频谱初始化频率域数据
        /// </summary>
        private void InitializeWaveSpectrum()
        {
            float A = 0.0001f; // 全局振幅系数
            float windSpeed = 10.0f;
            float windSpeedSq = windSpeed * windSpeed;
            Vector2 windDir = new Vector2(1.0f, 0.0f);

            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Cols; j++)
                {
                    int index = i * Cols + j;
                    float ki = Kx[i, j];
                    float kj = Kz[i, j];
                    float kLen = K[i, j];

                    if (kLen < 0.0001f)
                    {
                        HeightFieldFrequency[index] = System.Numerics.Complex.Zero;
                        continue;
                    }

                    // 计算Phillips频谱
                    Vector2 kVec = new Vector2(ki, kj);
                    Vector2 windDirNormalized = Vector2.Normalize(windDir);
                    float kDotW = Vector2.Dot(Vector2.Normalize(kVec), windDirNormalized);

                    float L = windSpeedSq / G;
                    float damping = 0.001f;

                    float phillips = MathF.Exp(-1.0f / (kLen * L * kLen * L))
                                   / (kLen * kLen * kLen * kLen)
                                   * (kDotW * kDotW)
                                   * MathF.Exp(-kLen * kLen * damping * damping);

                    // 生成随机复数 (高斯分布)
                    float r1 = (float)(mRandom.NextDouble() * 2 - 1);
                    float r2 = (float)(mRandom.NextDouble() * 2 - 1);

                    System.Numerics.Complex amplitude = new System.Numerics.Complex(
                        r1 * MathF.Sqrt(phillips * A / 2.0f),
                        r2 * MathF.Sqrt(phillips * A / 2.0f)
                    );

                    // 确保Hermitian对称性
                    if (i == 0 && j == 0)
                    {
                        // DC分量为实数
                        HeightFieldFrequency[index] = System.Numerics.Complex.Zero;
                    }
                    else
                    {
                        HeightFieldFrequency[index] = amplitude;

                        // 设置对称位置的共轭
                        int iSym = (Rows - i) % Rows;
                        int jSym = (Cols - j) % Cols;
                        int symIndex = iSym * Cols + jSym;

                        // 避免重复设置（对于i=0或j=0的特殊情况）
                        if (symIndex != index)
                        {
                            HeightFieldFrequency[symIndex] = System.Numerics.Complex.Conjugate(amplitude);
                        }
                    }
                }
            }

            // 生成初始空间域高度场
            UpdateSpatialHeightField();
        }

        /// <summary>
        /// 时间演化 - 在频率域更新相位
        /// </summary>
        public void TimeStep()
        {
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Cols; j++)
                {
                    int index = i * Cols + j;

                    // 获取当前频率域值
                    System.Numerics.Complex h0 = HeightFieldFrequency[index];

                    // 计算时间演化后的值: h(t+dt) = h(t) * exp(-i * omega * dt)
                    float phase = -Omega[i, j] * Dt;

                    // 使用欧拉公式: exp(iθ) = cosθ + i*sinθ
                    System.Numerics.Complex rotation = System.Numerics.Complex.FromPolarCoordinates(1.0f, phase);

                    HeightFieldFrequency[index] = h0 * rotation;
                }
            }

            // 更新空间域高度场
            UpdateSpatialHeightField();
        }

        public static void FourierInverse2D(System.Numerics.Complex[,] data, int rows, int cols)
        {
            // 1. 每一行做 1D Inverse FFT
            System.Numerics.Complex[] row = new System.Numerics.Complex[cols];
            for (int r = 0; r < rows; r++)
            {   
                for (int c = 0; c < cols; c++)
                    row[c] = data[r, c];

                Fourier.Inverse(row, FourierOptions.Matlab);

                for (int c = 0; c < cols; c++)
                    data[r, c] = row[c];
            }

            System.Numerics.Complex[] col = new System.Numerics.Complex[rows];
            // 2. 每一列做 1D Inverse FFT
            for (int c = 0; c < cols; c++)
            {   
                for (int r = 0; r < rows; r++)
                    col[r] = data[r, c];

                Fourier.Inverse(col, FourierOptions.Matlab);

                for (int r = 0; r < rows; r++)
                    data[r, c] = col[r];
            }
        }

        /// <summary>
        /// 通过逆FFT更新空间域高度场
        /// </summary>
        private void UpdateSpatialHeightField()
        {
            // 复制频率域数据到临时数组
            Array.Copy(HeightFieldFrequency, TempComplexArray, HeightFieldFrequency.Length);

            // 执行二维逆FFT（使用一维数组和行列参数）
            Fourier.Inverse2D(TempComplexArray, Rows, Cols);

            // 提取实部作为高度场
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Cols; j++)
                {
                    int index = i * Cols + j;
                    HeightFieldSpatial[i, j] = (float)TempComplexArray[index].Real;
                }
            }
        }

        /// <summary>
        /// 获取水面顶点数据
        /// </summary>
        public bool GetWaveMeshData(ref WaveMeshData data)
        {
            if (data == null)
            {
                data = new WaveMeshData(Rows * Cols);
                for (int i = 0; i < Rows; i++)
                {
                    for (int j = 0; j < Cols; j++)
                    {
                        int index = i * Cols + j;

                        // 计算顶点位置
                        float x = (float)i / (Rows - 1) * L - L / 2.0f;
                        float z = (float)j / (Cols - 1) * L - L / 2.0f;
                        float y = HeightFieldSpatial[i, j];

                        data.vertices[index] = new Vector3(x, y, z);
                        data.normals[index] = CalculateNormal(i, j);
                        data.uvs[index] = new Vector2((float)i / Rows, (float)j / Cols);
                    }
                }

                // 生成三角形索引（用于网格渲染）
                GenerateTriangleIndices(data);
            }

            if (data.vertices.Length!=Rows * Cols)
                return false;

            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Cols; j++)
                {
                    int index = i * Cols + j;

                    // 计算顶点位置
                    float x = (float)i / (Rows - 1) * L - L / 2.0f;
                    float z = (float)j / (Cols - 1) * L - L / 2.0f;
                    float y = HeightFieldSpatial[i, j];

                    data.vertices[index] = new Vector3(x, y, z);
                    data.normals[index] = CalculateNormal(i, j);
                    data.uvs[index] = new Vector2((float)i / Rows, (float)j / Cols);
                }
            }

            return true;
        }

        /// <summary>
        /// 计算顶点法线（简化版本）
        /// </summary>
        private Vector3 CalculateNormal(int i, int j)
        {
            // 边界处理：使用周期边界条件
            int iNext = (i + 1) % Rows;
            int iPrev = (i > 0) ? i - 1 : Rows - 1;
            int jNext = (j + 1) % Cols;
            int jPrev = (j > 0) ? j - 1 : Cols - 1;

            float height = HeightFieldSpatial[i, j];
            float heightXNext = HeightFieldSpatial[iNext, j];
            float heightXPrev = HeightFieldSpatial[iPrev, j];
            float heightZNext = HeightFieldSpatial[i, jNext];
            float heightZPrev = HeightFieldSpatial[i, jPrev];

            // 计算梯度
            float dx = (heightXNext - heightXPrev) / (2.0f * L / Rows);
            float dz = (heightZNext - heightZPrev) / (2.0f * L / Cols);

            // 法线向量（垂直于表面）
            Vector3 normal = new Vector3(-dx, 1.0f, -dz);
            return Vector3.Normalize(normal);
        }

        /// <summary>
        /// 生成三角形索引
        /// </summary>
        private void GenerateTriangleIndices(WaveMeshData data)
        {
            int triangleCount = (Rows - 1) * (Cols - 1) * 2;
            data.triangles = new int[triangleCount * 3];

            int triIndex = 0;
            for (int i = 0; i < Rows - 1; i++)
            {
                for (int j = 0; j < Cols - 1; j++)
                {
                    int topLeft = i * Cols + j;
                    int topRight = i * Cols + (j + 1);
                    int bottomLeft = (i + 1) * Cols + j;
                    int bottomRight = (i + 1) * Cols + (j + 1);

                    // 第一个三角形（左上三角形）
                    data.triangles[triIndex++] = topLeft;
                    data.triangles[triIndex++] = bottomLeft;
                    data.triangles[triIndex++] = topRight;

                    // 第二个三角形（右下三角形）
                    data.triangles[triIndex++] = topRight;
                    data.triangles[triIndex++] = bottomLeft;
                    data.triangles[triIndex++] = bottomRight;
                }
            }
        }

        /// <summary>
        /// 添加局部扰动
        /// </summary>
        public void AddDisturbance(float centerX, float centerZ, float radius, float strength)
        {
            // 在空间域添加扰动
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Cols; j++)
                {
                    float x = (float)i / (Rows - 1) * L - L / 2.0f;
                    float z = (float)j / (Cols - 1) * L - L / 2.0f;

                    float dx = x - centerX;
                    float dz = z - centerZ;
                    float distSq = dx * dx + dz * dz;
                    float radiusSq = radius * radius;

                    if (distSq < radiusSq)
                    {
                        float factor = MathF.Exp(-distSq / (radiusSq * 0.5f));
                        HeightFieldSpatial[i, j] += strength * factor;
                    }
                }
            }

            // 更新频率域数据以保持一致性
            UpdateFrequencyFromSpatial();
        }

        /// <summary>
        /// 从空间域数据更新频率域数据
        /// </summary>
        private void UpdateFrequencyFromSpatial()
        {
            // 将空间域数据复制到临时数组
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Cols; j++)
                {
                    int index = i * Cols + j;
                    TempComplexArray[index] = new System.Numerics.Complex(HeightFieldSpatial[i, j], 0);
                }
            }

            // 执行正向FFT
            Fourier.Forward2D(TempComplexArray, Rows, Cols);

            // 复制回频率域数组
            Array.Copy(TempComplexArray, HeightFieldFrequency, TempComplexArray.Length);

            // 确保Hermitian对称性（由于从实数数据变换而来，这步通常自动满足）
            EnforceHermitianSymmetry();
        }

        /// <summary>
        /// 强制Hermitian对称性
        /// </summary>
        private void EnforceHermitianSymmetry()
        {
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Cols; j++)
                {
                    int index = i * Cols + j;

                    // 特殊点处理
                    if ((i == 0 && j == 0) ||
                        (i == 0 && j == Cols/2) ||
                        (i == Rows/2 && j == 0) ||
                        (i == Rows/2 && j == Cols/2))
                    {
                        // 这些点必须是实数
                        HeightFieldFrequency[index] = new System.Numerics.Complex(
                            HeightFieldFrequency[index].Real, 0);
                    }
                    else
                    {
                        // 确保共轭对称
                        int iSym = (Rows - i) % Rows;
                        int jSym = (Cols - j) % Cols;
                        int symIndex = iSym * Cols + jSym;

                        if (symIndex > index) // 避免重复设置
                        {
                            System.Numerics.Complex conj = System.Numerics.Complex.Conjugate(HeightFieldFrequency[index]);
                            HeightFieldFrequency[symIndex] = conj;
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// 水面网格数据结构
    /// </summary>
    public class WaveMeshData
    {
        public Vector3[] vertices;   // 顶点位置
        public Vector3[] normals;    // 法线
        public Vector2[] uvs;        // UV坐标
        public int[] triangles;      // 三角形索引

        public WaveMeshData(int vertexCount)
        {
            vertices = new Vector3[vertexCount];
            normals = new Vector3[vertexCount];
            uvs = new Vector2[vertexCount];
            triangles = new int[0];
        }
        public unsafe void MakeMesh(ref Graphics.Mesh.TtMeshDataProvider meshBuilder)
        {
            NxRHI.FMeshDataProvider builder;
            if (meshBuilder == null)
            {
                meshBuilder = new Graphics.Mesh.TtMeshDataProvider();

                meshBuilder.AssetName = RName.GetRName("@MakeFFTWater", RName.ERNameType.Transient);
                builder = meshBuilder.mCoreObject;
                uint streams = (uint)((1 << (int)NxRHI.EVertexStreamType.VST_Position) |
                    (1 << (int)NxRHI.EVertexStreamType.VST_Normal) |
                    (1 << (int)NxRHI.EVertexStreamType.VST_Color) |
                    (1 << (int)NxRHI.EVertexStreamType.VST_UV));
                builder.Init(streams, false, 1);

                var dpDesc = new NxRHI.FMeshAtomDesc();
                dpDesc.SetDefault();
                dpDesc.NumPrimitives = (uint)triangles.Length / 3;

                for (int i = 0; i<vertices.Length; i++)
                {
                    builder.AddVertex(vertices[i], normals[i], uvs[i], 0xFFFFFFFF);
                }
                for (int i = 0; i<triangles.Length / 3; i++)
                {
                    builder.AddTriangle((uint)triangles[i * 3], (uint)triangles[i * 3 + 1], (uint)triangles[i * 3 + 2]);
                }
                builder.PushAtomLOD(0, &dpDesc);
                builder.CalcAABB();
            }
            else
            {
                builder = meshBuilder.mCoreObject;

                for (int i = 0; i<vertices.Length; i++)
                {
                    builder.SetVertex(i, vertices[i], normals[i], uvs[i], 0xFFFFFFFF);
                }
            }
        }
    }

    [Bricks.CodeBuilder.ContextMenu("FftWater", "Graphics\\FftWater", GamePlay.Scene.TtNode.EditorKeyword)]
    [GamePlay.Scene.TtNode(NodeDataType = typeof(GamePlay.Scene.TtNodeData), DefaultNamePrefix = "FftWater")]
    public class TtFftWaterNode : GamePlay.Scene.TtVisual
    {
        protected override async Thread.Async.TtTask<bool> InitializeNode(GamePlay.TtWorld world, GamePlay.Scene.TtNodeData data, GamePlay.Scene.EBoundVolumeType bvType, Type placementType)
        {
            if (await base.InitializeNode(world, data, bvType, placementType) == false)
                return false;

            mWaveSim = new TtFftWaveSimulation(128, 100.0f, 0.016f);
            return true;
        }
        TtFftWaveSimulation mWaveSim = null;
        Graphics.Mesh.TtRenderMesh mMesh = null;
        Graphics.Mesh.TtMeshDataProvider mMeshBuilder = null;
        public override void OnGatherVisibleMeshes(GamePlay.TtWorld.TtVisParameter rp)
        {
            UpdateCameralOffset(rp.World);

            if (mMesh == null)
                return;

            this.CheckDirty();

            rp.AddVisibleMesh(mMesh);
        }
        WaveMeshData mMeshData = null;
        public unsafe override bool OnTickLogic(TtNodeTickParameters args)
        {
            if (mWaveSim==null)
                return true;

            mWaveSim.TimeStep();
            mWaveSim.GetWaveMeshData(ref mMeshData);

            mMeshData.MakeMesh(ref mMeshBuilder);
            if (mMesh==null)
            {
                mMesh = mMeshBuilder.ToDrawMesh(TtEngine.Instance.GfxDevice.MaterialManager.NavMeshDebugWireMaterial);
            }
            else
            {
                mMeshBuilder.ToMesh(mMesh.MaterialMesh.SubMeshes[0].Mesh);
            }
            return true;
        }
    }

    class Program
    {
        internal static void Test(string[] args)
        {
            // 初始化模拟
            int resolution = 128;  // 128x128网格
            float domainSize = 100.0f;  // 100米 x 100米
            float timeStep = 0.016f;  // 约60FPS的时间步长

            TtFftWaveSimulation waveSim = new TtFftWaveSimulation(resolution, domainSize, timeStep);

            WaveMeshData meshData = null;
            // 模拟循环
            for (int frame = 0; frame < 1000; frame++)
            {
                // 时间演化
                waveSim.TimeStep();

                // 获取顶点数据用于渲染
                waveSim.GetWaveMeshData(ref meshData);
                // 这里可以将vertexData传递给渲染系统

                // 可选：添加随机扰动
                if (frame % 100 == 0)
                {
                    Random rand = new Random();
                    float x = (float)(rand.NextDouble() * 100 - 50);
                    float z = (float)(rand.NextDouble() * 100 - 50);
                    waveSim.AddDisturbance(x, z, 5.0f, 0.5f);
                }

                // 控制帧率
                System.Threading.Thread.Sleep(16);
            }
        }
    }
}
