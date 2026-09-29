using EngineNS.GamePlay;
using EngineNS.GamePlay.Scene;
using EngineNS.Support;
using EngineNS.Thread.Async;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;

namespace EngineNS.Bricks.FX.Weather
{
    // CPU 端天气图 + 3D 噪声生成器
    public class TtCloudNoiseGenerator : IO.BaseSerializer, IDisposable
    {
        public void Dispose()
        {
            if (WeatherSrv != null)
            {
                WeatherSrv.Dispose();
                WeatherSrv = null;
            }
            if (CloudNoiseSrv != null)
            {
                CloudNoiseSrv.Dispose();
                CloudNoiseSrv = null;
            }
        }
        [System.ComponentModel.Category("Option")]
        public NxRHI.TtSrView WeatherSrv { get; set; }
        [System.ComponentModel.Category("Option")]
        public NxRHI.TtSrView CloudNoiseSrv { get; set; }
        public class TtWeatherMapSettings : IO.BaseSerializer
        {
            [Rtti.Meta("")]
            [System.ComponentModel.Category("Option")]
            public int Width { get; set; } = 512;
            [Rtti.Meta("")]
            [System.ComponentModel.Category("Option")]
            public int Height { get; set; } = 512;
            [Rtti.Meta("")]
            [System.ComponentModel.Category("Option")]
            public float CoverageFrequency { get; set; } = 8.0f;
            [Rtti.Meta("")]
            [System.ComponentModel.Category("Option")]
            public float CoverageAmount { get; set; } = 0.7f;
            [Rtti.Meta("")]
            [System.ComponentModel.Category("Option")]
            public float DensityFrequency { get; set; } = 1000.0f;
            [Rtti.Meta("")]
            [System.ComponentModel.Category("Option")]
            public float DensityAmount { get; set; } = 0.5f;
            [Rtti.Meta("")]
            [System.ComponentModel.Category("Option")]
            public bool AddCirrus { get; set; } = true;
            [Rtti.Meta("")]
            [System.ComponentModel.Category("Option")]
            public float CirrusFrequency { get; set; } = 0.001f;
            [Rtti.Meta("")]
            [System.ComponentModel.Category("Option")]
            public float CirrusStrength { get; set; } = 0.3f;
        }
        TtWeatherMapSettings mWeatherSettings = new TtWeatherMapSettings();
        [Rtti.Meta("")]
        [System.ComponentModel.Category("Option")]
        public TtWeatherMapSettings WeatherSettings
        {
            get { return mWeatherSettings; }
            set { mWeatherSettings = value; }
        }
        public async Thread.Async.TtTask ReGenRenderResources()
        {
            if (WeatherSrv != null)
            {
                WeatherSrv.Dispose();
                WeatherSrv = null;
            }
            if (CloudNoiseSrv != null)
            {
                CloudNoiseSrv.Dispose();
                CloudNoiseSrv = null;
            }
            await TtEngine.Instance.EventPoster.Post((state) =>
            {
                {
                    var weatherTex = GenerateWeatherMap(mWeatherSettings, Seed);
                    NxRHI.FSrvDesc srvDesc = new NxRHI.FSrvDesc();
                    srvDesc.SetTexture2D();
                    var texDesc = weatherTex.mCoreObject.Desc;
                    srvDesc.Texture2D.MipLevels = texDesc.MipLevels;
                    WeatherSrv = TtEngine.Instance.GfxDevice.RenderContext.CreateSRV(weatherTex, in srvDesc);
                    WeatherSrv.AssetName = RName.GetRName("VolumetricCloudWeather.srv", RName.ERNameType.Transient);
                }

                {
                    var cloudNoiseTex = GenerateNoise3D();
                    NxRHI.FSrvDesc srvDesc = new NxRHI.FSrvDesc();
                    srvDesc.SetTexture3D();
                    var texDesc = cloudNoiseTex.mCoreObject.Desc;
                    srvDesc.Format = texDesc.Format;
                    srvDesc.Texture3D.MipLevels = texDesc.MipLevels;
                    CloudNoiseSrv = TtEngine.Instance.GfxDevice.RenderContext.CreateSRV(cloudNoiseTex, in srvDesc);
                    CloudNoiseSrv.AssetName = RName.GetRName("VolumetricCloudNoise3D.srv", RName.ERNameType.Transient);
                }
                return true;
            }, EAsyncTarget.AsyncIO);
        }

        public int Resolution = 64;
        [Rtti.Meta("")]
        [System.ComponentModel.Category("Option")]
        [EGui.Controls.PropertyGrid.TtValueChangeStep(0.001f)]
        public Vector3 Scale { get; set; } = new Vector3(0.08f, 0.08f, 0.08f);
        [Rtti.Meta("")]
        [System.ComponentModel.Category("Option")]
        [EGui.Controls.PropertyGrid.TtValueChangeStep(0.001f)]
        public Vector3 WorleyScale { get; set; } = new Vector3(0.1f, 0.1f, 0.1f);

        [Rtti.Meta("")]
        [System.ComponentModel.Category("Option")]
        public int Octaves { get; set; } = 4;
        [Rtti.Meta("")]
        [System.ComponentModel.Category("Option")]
        public float Frequency { get; set; } = 100.0f;
        [Rtti.Meta("")]
        [System.ComponentModel.Category("Option")]
        public float Lacunarity { get; set; } = 2.0f;
        [Rtti.Meta("")]
        [System.ComponentModel.Category("Option")]
        [EGui.Controls.PropertyGrid.TtValueChangeStep(0.01f)]
        public float Gain { get; set; } = 0.5f;

        [Rtti.Meta("")]
        [System.ComponentModel.Category("Option")]
        [EGui.Controls.PropertyGrid.TtValueChangeStep(0.01f)]
        public float PerlinWeight { get; set; } = 0.2f;
        [Rtti.Meta("")]
        [System.ComponentModel.Category("Option")]
        public float BillowPower { get; set; } = 1.0f;

        [Rtti.Meta("")]
        [System.ComponentModel.Category("Option")]
        // 噪声随机种子. 以前是拿 tick 当种子, 每次启动云的样子都不一样, 出了问题也没法复现
        public int Seed { get; set; } = 20260916;

        public TtPerlin2 Perlin3D = null;
        [Rtti.Meta("")]
        [System.ComponentModel.Category("Option")]
        public TtWorly3D Worley3D { get; set; } = new TtWorly3D();
        public Support.IRemapCurve RemapCurve = null;

        #region WeatherMap
        public static unsafe NxRHI.TtTexture GenerateWeatherMap(TtWeatherMapSettings weatherMapSettings, int seed)
        {
            int width = weatherMapSettings.Width;
            int height = weatherMapSettings.Height;

            Color4f[] colors = new Color4f[width * height];

            // 频率的单位是"整张图横跨多少个噪声格". 老场景里存过 1000 这种按逐纹素缩放语义填的值,
            // 512 个纹素跨 1000 个格等于每个纹素自己占一格, Perlin 直接退化成白噪声
            float coverageFreq = SanitizeFrequency(weatherMapSettings.CoverageFrequency, width, 8.0f);
            float densityFreq = SanitizeFrequency(weatherMapSettings.DensityFrequency, width, 6.0f);

            var perlin = new Support.TtPerlin2(seed, 1024);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float u = (float)x / width;
                    float v = (float)y / height;

                    // R: 覆盖率. 避开 Perlin 置换表的 0 周期边界；该边界两侧不连续，会在 WRAP 采样时形成世界空间平面接缝
                    float coverage = (float)TileablePerlinNoise(
                        u, v, coverageFreq, 400.0f, perlin);
                    coverage = (coverage + 1.0f) / 2.0f;
                    coverage = MathHelper.Clamp(coverage * weatherMapSettings.CoverageAmount, 0, 1);

                    // G: 密度
                    float density = (float)TileablePerlinNoise(
                        u, v, densityFreq, 100.0f, perlin);
                    density = (density + 1.0f) / 2.0f;
                    density = MathHelper.Clamp(density * weatherMapSettings.DensityAmount, 0, 1);

                    // B: 云类型(低频, 0=层云 1=积云)
                    float cloudType = (float)TileablePerlinNoise(
                        u, v, 2.0f, 200.0f, perlin);
                    cloudType = MathHelper.Clamp((cloudType + 1.0f) / 2.0f, 0, 1);
                    if (weatherMapSettings.AddCirrus)
                        cloudType = MathHelper.Clamp(cloudType + weatherMapSettings.CirrusStrength * 0.2f, 0, 1);

                    // A: 海拔变化
                    float elevation = (float)TileablePerlinNoise(
                        u, v, 3.0f, 300.0f, perlin);
                    elevation = (elevation + 1.0f) / 2.0f;

                    // Color4f 的构造器是 (alpha, red, green, blue). 直接按 rgba 的顺序传进去
                    // 会让四个通道整体错位一位: coverage 跑到 A 上没人读, density 占了 R,
                    // shader 拿 weather.r 当覆盖率用的时候实际读到的是密度通道. 逐字段赋值避免再踩
                    var texel = new Color4f();
                    texel.Red = coverage;
                    texel.Green = density;
                    texel.Blue = cloudType;
                    texel.Alpha = elevation;
                    colors[x + y * width] = texel;
                }
            }

            var sourceLayer = new NxRHI.TtTextureUtility.TtTex2dLayer();
            sourceLayer.Pixels = colors;
            sourceLayer.Width = width;
            sourceLayer.Height = height;
            sourceLayer.NormalizeLayer();
            return sourceLayer.CreateTexture2D(EPixelFormat.PXF_R8G8B8A8_UNORM, 1);
        }
        // 噪声格必须留够纹素才能插值出平滑结构, 少于 8 个纹素一格就已经接近逐纹素白噪声了.
        // 这种值进了 shader 的 coverage 会变成一根根 24m 宽、跨满整个云层厚度的竖直细柱(满天拉丝),
        // 所以按旧语义填进来的荒谬值一律当脏数据处理, 回落到默认频率
        private static float SanitizeFrequency(float freq, int texels, float defFreq)
        {
            float maxFreq = texels / 8.0f;
            return (freq <= 0.0f || freq > maxFreq) ? defFreq : freq;
        }
        // 真正的无缝平铺: 取噪声的 4 个平移副本按 uv 双线性混合, u=0 与 u=1 处的取值恒等.
        // 旧实现只是把 ±32 偏移的采样做高斯加权求和, 与纹理宽度没有任何周期关系,
        // 贴到世界上后每 1/CloudScale 米就有一道接缝, 透视下就是一堆汇聚到地平线消失点的放射直线
        private static float TileablePerlinNoise(float u, float v, float freq, float offset, Support.TtPerlin2 perlin)
        {
            float x = u * freq + offset;
            float y = v * freq + offset;
            float n00 = (float)perlin.Noise(x, y);
            float n10 = (float)perlin.Noise(x - freq, y);
            float n01 = (float)perlin.Noise(x, y - freq);
            float n11 = (float)perlin.Noise(x - freq, y - freq);
            return (1 - u) * (1 - v) * n00 + u * (1 - v) * n10 +
                   (1 - u) * v * n01 + u * v * n11;
        }
        #endregion

        public unsafe NxRHI.TtTexture GenerateNoise3D()
        {
            int size = Resolution;
            Color4f[] colors = new Color4f[size * size * size];

            Perlin3D = new TtPerlin2(Seed);
            Worley3D.Seed = Seed;

            float maxValue = float.MinValue;
            float minValue = float.MaxValue;

            float[] perlinNoise = GenerateFractalPerlinNoise(size, Perlin3D);
            float[] worleyNoise = GenerateFractalWorleyNoise(size, Worley3D);

            float WorleyWeight = 1 - PerlinWeight;
            for (int z = 0; z < size; z++)
            {
                for (int y = 0; y < size; y++)
                {
                    for (int x = 0; x < size; x++)
                    {
                        int index = x + y * size + z * size * size;
                        float perlin = perlinNoise[index];
                        float worley = worleyNoise[index];
                        float value = perlin * PerlinWeight + worley * WorleyWeight;
                        value = MathF.Pow(value, BillowPower);
                        if (RemapCurve != null)
                            value = RemapCurve.Evaluate(value);
                        colors[index] = new Color4f(value, value, value, 1.0f);
                        maxValue = MathF.Max(maxValue, value);
                        minValue = MathF.Min(minValue, value);
                    }
                }
            }

            if (MathF.Abs(maxValue - minValue) > 0.001f)
            {
                for (int i = 0; i < colors.Length; i++)
                {
                    float value = (colors[i].r - minValue) / (maxValue - minValue);
                    colors[i] = new Color4f(value, value, value, 1.0f);
                }
            }

            var sourceLayer = new NxRHI.TtTextureUtility.TtTex3dLayer();
            sourceLayer.Pixels = colors;
            sourceLayer.Width = size;
            sourceLayer.Height = size;
            sourceLayer.Depth = size;
            int mipLevels = 1 + (int)MathF.Floor(MathF.Log2(size));
            return sourceLayer.CreateTexture3D(EPixelFormat.PXF_R16_FLOAT, mipLevels);
        }

        private float[] GenerateFractalPerlinNoise(int size, TtPerlin2 Perlin3D)
        {
            float cellsX = ToCellCount(Scale.x, 4.0f);
            float cellsY = ToCellCount(Scale.y, 4.0f);
            float cellsZ = ToCellCount(Scale.z, 4.0f);

            float[] noise = new float[size * size * size];
            for (int z = 0; z < size; z++)
            {
                float w = (float)z / size;
                for (int y = 0; y < size; y++)
                {
                    float v = (float)y / size;
                    for (int x = 0; x < size; x++)
                    {
                        float u = (float)x / size;
                        float value = 0.0f;
                        float amp = 1.0f;
                        float norm = 0.0f;
                        float freq = Frequency;
                        for (int o = 0; o < Octaves; o++)
                        {
                            value += TileablePerlin3D(Perlin3D, u, v, w,
                                cellsX * freq, cellsY * freq, cellsZ * freq) * amp;
                            norm += amp;
                            amp *= Gain;
                            freq *= Lacunarity;
                        }
                        noise[x + y * size + z * size * size] = (norm > 0.0f) ? value / norm : 0.0f;
                    }
                }
            }
            return noise;
        }
        // Scale / WorleyScale 的语义是"整个体积在每个轴上横跨多少个噪声格".
        // 旧资产里存的是 0.05 / 0.01 这种逐纹素坐标缩放值, 按那个取法整张 64³
        // 只覆盖不到一个噪声格, 也就是一段平滑斜坡 —— 纹理里根本没有任何高频结构,
        // 云也就没有棉絮细节, 所以 <1 的旧值统一回落到默认格数
        private static float ToCellCount(float legacyScale, float defCells)
        {
            return (legacyScale < 1.0f) ? defCells : MathF.Round(legacyScale);
        }
        // 无缝平铺的单个 Perlin 八度: 取 8 个平移副本按 uvw 三线性混合,
        // u=0 与 u=1 处的取值恒等, 因此纹理在 wrap 采样下没有接缝
        private static float TileablePerlin3D(TtPerlin2 perlin, float u, float v, float w,
            float fx, float fy, float fz)
        {
            float x0 = u * fx, y0 = v * fy, z0 = w * fz;
            float x1 = x0 - fx, y1 = y0 - fy, z1 = z0 - fz;
            float iu = 1.0f - u, iv = 1.0f - v, iw = 1.0f - w;
            float n = 0.0f;
            n += (float)perlin.Noise(x0, y0, z0) * iu * iv * iw;
            n += (float)perlin.Noise(x1, y0, z0) * u * iv * iw;
            n += (float)perlin.Noise(x0, y1, z0) * iu * v * iw;
            n += (float)perlin.Noise(x1, y1, z0) * u * v * iw;
            n += (float)perlin.Noise(x0, y0, z1) * iu * iv * w;
            n += (float)perlin.Noise(x1, y0, z1) * u * iv * w;
            n += (float)perlin.Noise(x0, y1, z1) * iu * v * w;
            n += (float)perlin.Noise(x1, y1, z1) * u * v * w;
            return n;
        }
        private float[] GenerateFractalWorleyNoise(int size, TtWorly3D Worly3D)
        {
            // Worley 不能用上面的副本混合法: 它靠的就是尖锐的格子结构, 混两份会把泡泡叠成一团糊.
            // 改成让它的特征点自己按格数取模, 噪声本身就是精确周期的
            int cells = (int)ToCellCount(WorleyScale.x, 6.0f);
            Worly3D.WrapCells = cells;

            float[] noise = new float[size * size * size];
            for (int z = 0; z < size; z++)
            {
                float nz = (float)z / size * cells;
                for (int y = 0; y < size; y++)
                {
                    float ny = (float)y / size * cells;
                    for (int x = 0; x < size; x++)
                    {
                        float nx = (float)x / size * cells;
                        float value = Worly3D.GetWorleyValue(nx, ny, nz, 1.0f);
                        value += 0.5f;
                        noise[x + y * size + z * size * size] = value;
                    }
                }
            }
            return noise;
        }
    }

    public class TtVolumetricCloudShading : Graphics.Pipeline.Shader.TtGraphicsShadingEnv
    {
        public TtVolumetricCloudShading()
        {
            CodeName = RName.GetRName("shaders/bricks/fx/volumetriccloud.cginc", RName.ERNameType.Engine);
            this.UpdatePermutation().AddWaitTask();
        }
        public override NxRHI.EVertexStreamType[] GetNeedStreams()
        {
            return new NxRHI.EVertexStreamType[] { NxRHI.EVertexStreamType.VST_Position,
                NxRHI.EVertexStreamType.VST_UV,};
        }
        protected override void EnvShadingDefines(in FPermutationId id, NxRHI.TtShaderDefinitions defines)
        {
        }
        public override void OnDrawCall(NxRHI.ICommandList cmd, NxRHI.TtGraphicDraw drawcall, Graphics.Pipeline.TtRenderPolicy policy, Graphics.Mesh.TtRenderMesh.TtAtom atom)
        {
            var aaNode = drawcall.TagObject as TtVolumetricCloudNode;
            if (aaNode == null || aaNode.IsCloudRenderingEnabled == false)
            {
                base.OnDrawCall(cmd, drawcall, policy, atom);
                return;
            }

            var index = drawcall.FindBinder("ColorBuffer");
            if (index.IsValidPointer)
            {
                var attachBuffer = aaNode.GetAttachBuffer(aaNode.ColorPinIn);
                drawcall.BindSRV(index, attachBuffer.Srv);
            }
            index = drawcall.FindBinder("Samp_ColorBuffer");
            if (index.IsValidPointer)
                drawcall.BindSampler(index, TtEngine.Instance.GfxDevice.SamplerStateManager.DefaultState);

            index = drawcall.FindBinder("DepthBuffer");
            if (index.IsValidPointer)
            {
                var attachBuffer = aaNode.GetAttachBuffer(aaNode.DepthPinIn);
                if (attachBuffer != null)
                    drawcall.BindSRV(index, attachBuffer.Srv);
            }
            index = drawcall.FindBinder("Samp_DepthBuffer");
            if (index.IsValidPointer)
                drawcall.BindSampler(index, TtEngine.Instance.GfxDevice.SamplerStateManager.PointState);

            index = drawcall.FindBinder("WeatherTex");
            if (index.IsValidPointer && aaNode.SceneNode.NoiseGen.WeatherSrv != null)
                drawcall.BindSRV(index, aaNode.SceneNode.NoiseGen.WeatherSrv);
            index = drawcall.FindBinder("Samp_WeatherTex");
            if (index.IsValidPointer)
                drawcall.BindSampler(index, TtEngine.Instance.GfxDevice.SamplerStateManager.DefaultState);

            index = drawcall.FindBinder("CloudNoiseTex");
            if (index.IsValidPointer && aaNode.SceneNode.NoiseGen.CloudNoiseSrv != null)
                drawcall.BindSRV(index, aaNode.SceneNode.NoiseGen.CloudNoiseSrv);
            index = drawcall.FindBinder("Samp_CloudNoiseTex");
            if (index.IsValidPointer)
                drawcall.BindSampler(index, TtEngine.Instance.GfxDevice.SamplerStateManager.DefaultState);

            index = drawcall.FindBinder("cbShadingEnv");
            if (index.IsValidPointer)
            {
                if (aaNode.ShadingCbv == null)
                {
                    // §1.1: CreateCBV 后立即全字段 SetValue + MarkDirty + FlushDirty
                    aaNode.ShadingCbv = TtEngine.Instance.GfxDevice.RenderContext.CreateCBV(index);
                    aaNode.ShadingCbv.SetValue("ShadingStruct", in aaNode.SceneNode.ShadingStruct);
                    aaNode.ShadingCbv.MarkDirty();
                    aaNode.ShadingCbv.FlushDirty();
                }
                drawcall.BindCBV(index, aaNode.ShadingCbv);
            }
            base.OnDrawCall(cmd, drawcall, policy, atom);
        }
    }

    [Bricks.CodeBuilder.ContextMenu("VolumetricCloud", "Post\\VolumetricCloud", Bricks.RenderPolicyEditor.TtPolicyGraph.RGDEditorKeyword)]
    [Rtti.Meta("", NameAlias = new string[] { "EngineNS.Bricks.FX.Weather.TtVolumeCloudNode@EngineCore", "EngineNS.Bricks.FX.Weather.TtVolumeCloudNode" })]
    public class TtVolumetricCloudNode : Graphics.Pipeline.Common.TAuxSceenSpaceNode<TtVolumetricCloudNode>
    {
        public Graphics.Pipeline.TtRenderGraphPin ColorPinIn = Graphics.Pipeline.TtRenderGraphPin.CreateInputOutput("Color", NxRHI.EBufferType.BFT_SRV);
        public Graphics.Pipeline.TtRenderGraphPin DepthPinIn = Graphics.Pipeline.TtRenderGraphPin.CreateInput("Depth", NxRHI.EBufferType.BFT_SRV);
        public TtVolumetricCloudNode()
        {
            Name = "VolumetricCloud";
        }
        // RenderGraphNode.Enable=false 会让 RenderGraph 连 BeforeTick 都跳过，无法建立 Color -> Result 旁路。
        // 场景侧运行期开关必须使用这个字段，保持图结构启用，仅跳过体积云自身的渲染工作。
        [Browsable(false)]
        public bool RuntimeEnable { get; set; } = true;
        [Browsable(false)]
        public bool IsCloudRenderingEnabled => RuntimeEnable && SceneNode != null;
        public override void Dispose()
        {
            SceneNode = null;
            CoreSDK.DisposeObject(ref ShadingCbv);
            base.Dispose();
        }
        public override void InitNodePins()
        {
            AddInputOutput(ColorPinIn);
            // Depth 允许悬空: 老 rpolicy (旧 TtVolumeCloudNode) 没有连 Depth,
            // 不设 IsAllowInputNull 会让 BuildGraph 报 hasInputError -> CreateRenderPolicy 返回 null.
            // 悬空时 OnDrawCall 里 GetAttachBuffer(DepthPinIn) 为 null, 已做空绑定保护.
            DepthPinIn.IsAllowInputNull = true;
            AddInput(DepthPinIn);
            base.InitNodePins();
        }
        public TtVolumetricCloudShading mBasePassShading;
        public override Graphics.Pipeline.Shader.TtGraphicsShadingEnv GetPassShading(Graphics.Mesh.TtRenderMesh.TtAtom atom = null)
        {
            return mBasePassShading;
        }
        public override async Thread.Async.TtTask Initialize(Graphics.Pipeline.TtRenderPolicy policy, string debugName)
        {
            await base.Initialize(policy, debugName);
            mBasePassShading = await Graphics.Pipeline.Shader.TtShadingEnv.CreateShadingEnv<TtVolumetricCloudShading>();
        }
        public override void BeforeTick(Graphics.Pipeline.TtRenderPolicy policy)
        {
            if (IsCloudRenderingEnabled == false)
            {
                MoveAttachment(ColorPinIn, ResultPinOut);
                return;
            }

            var buffer = FindAttachBuffer(ColorPinIn);
            if (buffer != null && ResultPinOut.Attachement.Format != buffer.BufferDesc.Format)
            {
                CreateGBuffers(policy, buffer.BufferDesc.Format);
                ResultPinOut.Attachement.Format = buffer.BufferDesc.Format;
            }
        }
        public override void Tick(TtWorld world, Graphics.Pipeline.TtRenderPolicy policy, NxRHI.TtCommandList frameCmdList, bool bClear)
        {
            if (IsCloudRenderingEnabled == false)
                return;

            if (ShadingCbv != null)
            {
                ShadingCbv.SetValue("ShadingStruct", in SceneNode.ShadingStruct);
            }
            base.Tick(world, policy, frameCmdList, bClear);
        }
        #region Rhi Resouces
        public NxRHI.TtCbView ShadingCbv;
        public TtVolumetricCloudSceneNode SceneNode;
        #endregion
    }

    [Bricks.CodeBuilder.ContextMenu("VolumetricCloud", "Graphics\\VolumetricCloud", TtNode.EditorKeyword)]
    [TtNode(NodeDataType = typeof(TtVolumetricCloudSceneNode.TtThisNodeData), DefaultNamePrefix = "VolumetricCloud")]
    [Rtti.Meta("", NameAlias = new string[] { "EngineNS.Bricks.FX.Weather.TtVolumeCloudSceneNode@EngineCore", "EngineNS.Bricks.FX.Weather.TtVolumeCloudSceneNode" })]
    public class TtVolumetricCloudSceneNode : GamePlay.Scene.TtVisual
    {
        [Rtti.Meta("", NameAlias = new string[] { "EngineNS.Bricks.FX.Weather.TtVolumeCloudSceneNode.TtThisNodeData@EngineCore", "EngineNS.Bricks.FX.Weather.TtVolumeCloudSceneNode.TtThisNodeData" })]
        public class TtThisNodeData : TtNodeData
        {
            public TtThisNodeData()
            {
                mShadingStruct.SetDefault();
            }
            [Rtti.Meta("")]
            public bool CloudEnable { get; set; } = true;

            [Rtti.Meta("")]
            [System.ComponentModel.Category("Option")]
            public TtCloudNoiseGenerator NoiseGen { get; set; } = new TtCloudNoiseGenerator();

            internal FShadingStruct mShadingStruct = new FShadingStruct();
            [Rtti.Meta("")]
            public FShadingStruct ShadingStruct
            {
                get => mShadingStruct;
                set => mShadingStruct = value;
            }
        }
        [System.ComponentModel.Category("Option")]
        [System.ComponentModel.DisplayName("Enable")]
        public bool CloudEnable
        {
            get => GetNodeData<TtThisNodeData>().CloudEnable;
            set => GetNodeData<TtThisNodeData>().CloudEnable = value;
        }

        [Rtti.Meta("")]
        [System.ComponentModel.Category("Option")]
        public TtCloudNoiseGenerator NoiseGen
        {
            get => GetNodeData<TtThisNodeData>()?.NoiseGen;
        }
        public class TtReGenTexture : EGui.Controls.PropertyGrid.TtButtonAttribute
        {
            bool IsGenarating = false;
            protected override void OnButtonClick(in EditorInfo info)
            {
                if (IsGenarating)
                    return;
                if (info.ObjectInstance.GetType().GetInterface("IList") != null)
                {
                    IsGenarating = true;
                    var lst = info.ObjectInstance as System.Collections.IList;
                    foreach (var i in lst)
                    {
                        var node = i as TtVolumetricCloudSceneNode;
                        if (node != null)
                        {
                            node.NoiseGen.ReGenRenderResources().AddWaitTask((task) =>
                            {
                                IsGenarating = false;
                            });
                        }
                    }
                }
                else
                {
                    IsGenarating = true;
                    var node = info.ObjectInstance as TtVolumetricCloudSceneNode;
                    if (node != null)
                    {
                        node.NoiseGen.ReGenRenderResources().AddWaitTask((task) =>
                        {
                            IsGenarating = false;
                        });
                    }
                }
            }
        }
        [TtReGenTexture(ButtonText = "ReGenTexture")]
        [System.ComponentModel.Category("Editor")]
        public bool ReGenTexture
        {
            get { return false; }
            set { }
        }

        // 与 HLSL VolumetricCloud.cginc 的 FShadingStruct 逐字段严格对齐(Pack=16)。引擎单位 = 米。
        // 外层类由 TtVolumeCloudSceneNode 改名而来, 嵌套类型的类型串是独立解析的,
        // 老场景里 ShadingStruct 属性存的是 TtVolumeCloudSceneNode.FShadingStruct@EngineCore,
        // 少了这个别名会 Typeof failed -> 属性类型不匹配 -> 整个 NodeData 反序列化失败。
        [Rtti.Meta("", NameAlias = new string[] { "EngineNS.Bricks.FX.Weather.TtVolumeCloudSceneNode.FShadingStruct@EngineCore", "EngineNS.Bricks.FX.Weather.TtVolumeCloudSceneNode.FShadingStruct" })]
        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 16)]
        public struct FShadingStruct
        {
            public void SetDefault()
            {
                CloudColor = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
                AmbientColor = new Vector4(0.35f, 0.4f, 0.5f, 1.0f);
                LightColor = new Vector4(0.98039216f, 0.98039216f, 0.8235294f, 1.0f);

                CloudDensity = 1.0f;
                CloudCoverage = 0.85f;
                CloudBottom = 1500.0f;   // 米
                CloudTop = 4000.0f;      // 米

                CloudScale = new Vector2(0.00008f, 0.00008f);
                LightAbsorption = 0.008f;
                DarknessThreshold = 0.01f;

                LightDir = Vector3.Down;
                MaxSteps = 192;

                PhaseG = 0.8f;
                PhaseG2 = -0.3f;
                PhaseBlend = 0.5f;
                LightSteps = 8;

                PowderScale = 1.0f;
                DetailScale = 0.3f;
                PlanetRadius = 6360000.0f; // 米
                BaseNoiseScale = 0.00015f;
            }
            public Vector4 CloudColor;
            public Vector4 AmbientColor;
            public Vector4 LightColor;

            public float CloudDensity;
            public float CloudCoverage;
            public float CloudBottom;
            public float CloudTop;

            public Vector2 CloudScale;
            public float LightAbsorption;
            public float DarknessThreshold;

            public Vector3 LightDir;
            public int MaxSteps;

            public float PhaseG;
            public float PhaseG2;
            public float PhaseBlend;
            public int LightSteps;

            public float PowderScale;
            public float DetailScale;
            public float PlanetRadius;
            public float BaseNoiseScale;
        }
        public ref FShadingStruct ShadingStruct => ref GetNodeData<TtThisNodeData>().mShadingStruct;

        [EGui.Controls.PropertyGrid.TtColor4PickerEditor]
        [System.ComponentModel.Category("Option")]
        public Vector4 CloudColor
        {
            get => GetNodeData<TtThisNodeData>().mShadingStruct.CloudColor;
            set => GetNodeData<TtThisNodeData>().mShadingStruct.CloudColor = value;
        }
        [EGui.Controls.PropertyGrid.TtColor4PickerEditor]
        [System.ComponentModel.Category("Option")]
        public Vector4 AmbientColor
        {
            get => GetNodeData<TtThisNodeData>().mShadingStruct.AmbientColor;
            set => GetNodeData<TtThisNodeData>().mShadingStruct.AmbientColor = value;
        }
        [EGui.Controls.PropertyGrid.TtColor4PickerEditor]
        [System.ComponentModel.Category("Option")]
        public Vector4 LightColor
        {
            get => GetNodeData<TtThisNodeData>().mShadingStruct.LightColor;
            set => GetNodeData<TtThisNodeData>().mShadingStruct.LightColor = value;
        }
        [System.ComponentModel.Category("Option")]
        [EGui.Controls.PropertyGrid.TtValueChangeStep(0.001f)]
        public float CloudDensity
        {
            get => GetNodeData<TtThisNodeData>().mShadingStruct.CloudDensity;
            set => GetNodeData<TtThisNodeData>().mShadingStruct.CloudDensity = value;
        }
        [System.ComponentModel.Category("Option")]
        [EGui.Controls.PropertyGrid.TtValueChangeStep(0.001f)]
        public float CloudCoverage
        {
            get => GetNodeData<TtThisNodeData>().mShadingStruct.CloudCoverage;
            set => GetNodeData<TtThisNodeData>().mShadingStruct.CloudCoverage = value;
        }
        [System.ComponentModel.Category("Option")]
        public float CloudBottom
        {
            get => GetNodeData<TtThisNodeData>().mShadingStruct.CloudBottom;
            set => GetNodeData<TtThisNodeData>().mShadingStruct.CloudBottom = value;
        }
        [System.ComponentModel.Category("Option")]
        public float CloudTop
        {
            get => GetNodeData<TtThisNodeData>().mShadingStruct.CloudTop;
            set => GetNodeData<TtThisNodeData>().mShadingStruct.CloudTop = value;
        }
        [System.ComponentModel.Category("Option")]
        [EGui.Controls.PropertyGrid.TtValueChangeStep(0.00001f)]
        public Vector2 CloudScale
        {
            get => GetNodeData<TtThisNodeData>().mShadingStruct.CloudScale;
            set => GetNodeData<TtThisNodeData>().mShadingStruct.CloudScale = value;
        }
        [System.ComponentModel.Category("Option")]
        [EGui.Controls.PropertyGrid.TtValueChangeStep(0.001f)]
        public float LightAbsorption
        {
            get => GetNodeData<TtThisNodeData>().mShadingStruct.LightAbsorption;
            set => GetNodeData<TtThisNodeData>().mShadingStruct.LightAbsorption = value;
        }
        [System.ComponentModel.Category("Option")]
        [EGui.Controls.PropertyGrid.TtValueChangeStep(0.001f)]
        public float DarknessThreshold
        {
            get => GetNodeData<TtThisNodeData>().mShadingStruct.DarknessThreshold;
            set => GetNodeData<TtThisNodeData>().mShadingStruct.DarknessThreshold = value;
        }
        [System.ComponentModel.Category("Option")]
        public int MaxSteps
        {
            get => GetNodeData<TtThisNodeData>().mShadingStruct.MaxSteps;
            set => GetNodeData<TtThisNodeData>().mShadingStruct.MaxSteps = value;
        }
        [System.ComponentModel.Category("Phase")]
        [EGui.Controls.PropertyGrid.TtValueChangeStep(0.01f)]
        public float PhaseG
        {
            get => GetNodeData<TtThisNodeData>().mShadingStruct.PhaseG;
            set => GetNodeData<TtThisNodeData>().mShadingStruct.PhaseG = value;
        }
        [System.ComponentModel.Category("Phase")]
        [EGui.Controls.PropertyGrid.TtValueChangeStep(0.01f)]
        public float PhaseG2
        {
            get => GetNodeData<TtThisNodeData>().mShadingStruct.PhaseG2;
            set => GetNodeData<TtThisNodeData>().mShadingStruct.PhaseG2 = value;
        }
        [System.ComponentModel.Category("Phase")]
        [EGui.Controls.PropertyGrid.TtValueChangeStep(0.01f)]
        public float PhaseBlend
        {
            get => GetNodeData<TtThisNodeData>().mShadingStruct.PhaseBlend;
            set => GetNodeData<TtThisNodeData>().mShadingStruct.PhaseBlend = value;
        }
        [System.ComponentModel.Category("Phase")]
        public int LightSteps
        {
            get => GetNodeData<TtThisNodeData>().mShadingStruct.LightSteps;
            set => GetNodeData<TtThisNodeData>().mShadingStruct.LightSteps = value;
        }
        [System.ComponentModel.Category("Option")]
        [EGui.Controls.PropertyGrid.TtValueChangeStep(0.01f)]
        public float PowderScale
        {
            get => GetNodeData<TtThisNodeData>().mShadingStruct.PowderScale;
            set => GetNodeData<TtThisNodeData>().mShadingStruct.PowderScale = value;
        }
        [System.ComponentModel.Category("Option")]
        [EGui.Controls.PropertyGrid.TtValueChangeStep(0.01f)]
        public float DetailScale
        {
            get => GetNodeData<TtThisNodeData>().mShadingStruct.DetailScale;
            set => GetNodeData<TtThisNodeData>().mShadingStruct.DetailScale = value;
        }
        [System.ComponentModel.Category("Option")]
        [EGui.Controls.PropertyGrid.TtValueChangeStep(0.0001f)]
        public float BaseNoiseScale
        {
            get => GetNodeData<TtThisNodeData>().mShadingStruct.BaseNoiseScale;
            set => GetNodeData<TtThisNodeData>().mShadingStruct.BaseNoiseScale = value;
        }
        public TtVolumetricCloudNode RenderNode = null;
        public override void Dispose()
        {
            if (RenderNode != null)
            {
                RenderNode.SceneNode = null;
            }
            if (NoiseGen != null)
            {
                NoiseGen.Dispose();
            }
            base.Dispose();
        }
        protected override async TtTask<bool> InitializeNode(TtWorld world, TtNodeData data, EBoundVolumeType bvType, Type placementType)
        {
            var ret = await base.InitializeNode(world, data, bvType, placementType);
            await NoiseGen.ReGenRenderResources();
            this.IsNoTick = false;
            this.IsParallelTick = false;
            return ret;
        }
        public override bool OnTickLogic(TtNodeTickParameters args)
        {
            var renderNode = args.Policy?.FindFirstNode<TtVolumetricCloudNode>();
            if (ReferenceEquals(RenderNode, renderNode) == false)
            {
                if (RenderNode != null && ReferenceEquals(RenderNode.SceneNode, this))
                    RenderNode.SceneNode = null;

                RenderNode = renderNode;
                if (RenderNode != null)
                    RenderNode.SceneNode = this;
            }

            if (RenderNode != null)
            {
                RenderNode.RuntimeEnable = CloudEnable;
                if (CloudEnable)
                {
                    var sun = GetWorld().GetSun();
                    if (sun != null)
                        LightDir = sun.DirectionLight.Direction;
                }
            }
            return base.OnTickLogic(args);
        }
        [Browsable(false)]
        public Vector3 LightDir
        {
            get => GetNodeData<TtThisNodeData>().mShadingStruct.LightDir;
            set => GetNodeData<TtThisNodeData>().mShadingStruct.LightDir = value;
        }
    }
}
