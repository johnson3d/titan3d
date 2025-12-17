using EngineNS.Bricks.AssetImpExp;
using EngineNS.GamePlay;
using EngineNS.GamePlay.Scene;
using EngineNS.Support;
using EngineNS.Thread.Async;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using static EngineNS.Bricks.FX.Weather.TtCloudNoiseGenerator;

namespace EngineNS.Bricks.FX.Weather
{
    public class TtCloudNoiseGenerator : IO.BaseSerializer, IDisposable
    {
        public void Dispose() 
        {
            CoreSDK.DisposeObject(ref WeatherSrv);
            CoreSDK.DisposeObject(ref CloudNoiseSrv);
        }
        public NxRHI.TtSrView WeatherSrv;
        public NxRHI.TtSrView CloudNoiseSrv;
        public class TtWeatherMapSettings : IO.BaseSerializer
        {
            [Rtti.Meta("")]
            [Category("Option")]
            public int Width { get; set; } = 256;
            [Rtti.Meta("")]
            [Category("Option")]
            public int Height { get; set; } = 256;
            [Rtti.Meta("")]
            [Category("Option")]
            public float CoverageFrequency { get; set; } = 8.0f;
            [Rtti.Meta("")]
            [Category("Option")]
            public float CoverageAmount { get; set; } = 0.7f;
            [Rtti.Meta("")]
            [Category("Option")]
            public float DensityFrequency { get; set; } = 0.005f;
            [Rtti.Meta("")]
            [Category("Option")]
            public float DensityAmount { get; set; } = 0.5f;
            [Rtti.Meta("")]
            [Category("Option")]
            public bool AddCirrus { get; set; } = true;
            [Rtti.Meta("")]
            [Category("Option")]
            public float CirrusFrequency { get; set; } = 0.001f;
            [Rtti.Meta("")]
            [Category("Option")]
            public float CirrusStrength { get; set; } = 0.3f;
        }
        TtWeatherMapSettings mWeatherSettings = new TtWeatherMapSettings();
        [Rtti.Meta("")]
        [Category("Option")]
        public TtWeatherMapSettings WeatherSettings
        {
            get
            {
                return mWeatherSettings;
            }
            set
            {
                mWeatherSettings = value;
            }
        }
        public async Thread.Async.TtTask ReGenRenderResources()
        {
            CoreSDK.DisposeObject(ref WeatherSrv);
            CoreSDK.DisposeObject(ref CloudNoiseSrv);
            await TtEngine.Instance.EventPoster.Post((state) =>
            {
                {
                    var weatherTex = GenerateWeatherMap(mWeatherSettings);

                    NxRHI.FSrvDesc srvDesc = new NxRHI.FSrvDesc();
                    srvDesc.SetTexture2D();
                    var texDesc = weatherTex.mCoreObject.Desc;
                    srvDesc.Texture2D.MipLevels = texDesc.MipLevels;
                    WeatherSrv = TtEngine.Instance.GfxDevice.RenderContext.CreateSRV(weatherTex, in srvDesc);
                }

                {
                    var cloudNoiseTex = GenerateNoise3D();
                    NxRHI.FSrvDesc srvDesc = new NxRHI.FSrvDesc();
                    srvDesc.SetTexture3D();
                    var texDesc = cloudNoiseTex.mCoreObject.Desc;
                    srvDesc.Format = texDesc.Format;
                    srvDesc.Texture3D.MipLevels = texDesc.MipLevels;
                    CloudNoiseSrv = TtEngine.Instance.GfxDevice.RenderContext.CreateSRV(cloudNoiseTex, in srvDesc);
                }
                return true;
            }, EAsyncTarget.AsyncIO);
        }

        public int Resolution = 64;
        public Vector3 Scale = Vector3.One;

        [Rtti.Meta("")]
        [Category("Option")]
        public int Octaves { get; set; } = 4;
        [Rtti.Meta("")]
        [Category("Option")]
        public float Frequency { get; set; } = 1.0f;
        [Rtti.Meta("")]
        [Category("Option")]
        public float Lacunarity { get; set; } = 2.0f;
        [Rtti.Meta("")]
        [Category("Option")]
        public float Gain { get; set; } = 0.5f;

        public TtPerlin2 Perlin3D = new TtPerlin2((int)Support.TtTime.GetTickCount());
        public TtWorly3D Worly3D = new TtWorly3D();

        public float PerlinWeight = 0.7f;
        public float BillowPower = 1.0f;

        public Support.IRemapCurve RemapCurve = null;

        public unsafe NxRHI.TtTexture GenerateNoise3D()
        {
            int size = Resolution;
            Color4f[] colors = new Color4f[size * size * size];

            float maxValue = float.MinValue;
            float minValue = float.MaxValue;

            // 预计算一些值以提高性能
            float[] perlinNoise = GenerateFractalPerlinNoise(size);
            float[] worleyNoise = GenerateFractalWorleyNoise(size);

            float WorleyWeight = 1 - PerlinWeight;
            // 混合噪声
            for (int z = 0; z < size; z++)
            {
                for (int y = 0; y < size; y++)
                {
                    for (int x = 0; x < size; x++)
                    {
                        int index = x + y * size + z * size * size;

                        float perlin = perlinNoise[index];
                        float worley = worleyNoise[index];

                        // 混合Perlin和Worley噪声
                        float value = perlin * PerlinWeight + worley * WorleyWeight;

                        // 应用Billow效果
                        value = MathF.Pow(value, BillowPower);

                        // 应用重映射曲线
                        if (RemapCurve!=null)
                            value = RemapCurve.Evaluate(value);

                        colors[index] = new Color4f(value, value, value, 1.0f);

                        maxValue = MathF.Max(maxValue, value);
                        minValue = MathF.Min(minValue, value);
                    }
                }
            }

            // 可选归一化
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
            List<NxRHI.TtTextureUtility.TtTex3dLayer> mipDatas = new List<NxRHI.TtTextureUtility.TtTex3dLayer>();
            mipDatas.Add(sourceLayer);
            int MaxLayer = 1;
            int s = size/2;
            while (s>=1)
            {
                if (mipDatas.Count>=MaxLayer)
                    break;
                var next = NxRHI.TtTextureUtility.GenerateMipLayer3D(sourceLayer, s, s, s);
                mipDatas.Add(next);
                s = s/2;
            }

            NxRHI.FMappedSubResource* initData = stackalloc NxRHI.FMappedSubResource[mipDatas.Count];
            try
            {
                for (int i = 0; i<mipDatas.Count; i++)
                {
                    initData[i].RowPitch = (uint)(mipDatas[i].Width * sizeof(Half));
                    initData[i].DepthPitch = (uint)(initData[i].RowPitch * mipDatas[i].Height);
                    initData[i].pData = mipDatas[i].CreateRHalf();
                }

                var texDesc = new NxRHI.FTextureDesc();
                texDesc.SetDefault();
                texDesc.Width = (uint)size;
                texDesc.Height = (uint)size;
                texDesc.Depth = (uint)size;
                texDesc.Format = EPixelFormat.PXF_R16_FLOAT;
                texDesc.MipLevels = (uint)mipDatas.Count;
                texDesc.InitData = initData;
                return TtEngine.Instance.GfxDevice.RenderContext.CreateTexture(in texDesc);
            }
            finally
            {
                for (int i = 0; i<mipDatas.Count; i++)
                {
                    mipDatas[i].DesctroyPixels(initData[i].pData);
                }
            }
        }
        public unsafe NxRHI.TtTexture GenerateWeatherMap(TtWeatherMapSettings weatherMapSettings)
        {
            int width = weatherMapSettings.Width;
            int height = weatherMapSettings.Height;

            Color4f[] colors = new Color4f[width * height];

            var perlin = new Support.TtPerlin2((int)TtEngine.Instance.CurrentTickFrame, 1024);
            // 生成天气图
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // 计算UV
                    float u = (float)x / width;
                    float v = (float)y / height;

                    // 1. 覆盖率（R通道）
                    float coverage = (float)perlin.Noise(
                        u * weatherMapSettings.CoverageFrequency,
                        v * weatherMapSettings.CoverageFrequency);

                    coverage = (coverage + 1.0f)/2.0f;
                    coverage = MathHelper.Clamp(coverage * weatherMapSettings.CoverageAmount, 0, 1);

                    // 2. 密度（G通道）
                    float density = (float)perlin.Noise(
                        u * weatherMapSettings.DensityFrequency + 100,
                        v * weatherMapSettings.DensityFrequency + 100);

                    density = (density + 1.0f)/2.0f;
                    density = MathHelper.Clamp(density * weatherMapSettings.DensityAmount, 0, 1);

                    // 3. 卷云效果（B通道）
                    float cirrus = 0;
                    if (weatherMapSettings.AddCirrus)
                    {
                        cirrus = (float)perlin.Noise(
                            u * weatherMapSettings.CirrusFrequency + 200,
                            v * weatherMapSettings.CirrusFrequency + 200);

                        cirrus = (cirrus + 1.0f)/2.0f;
                        cirrus = MathHelper.Clamp(cirrus * weatherMapSettings.CirrusStrength, 0, 1);
                    }

                    // 4. 海拔变化（A通道）- 可选
                    float elevation = (float)perlin.Noise(
                        u * 0.001f + 300,
                        v * 0.001f + 300);

                    // 组合到颜色
                    colors[x + y * width] = new Color4f(elevation, coverage, density, cirrus);
                }
            }

            float cmin = float.MaxValue;
            float cmax = float.MinValue;
            foreach (var color in colors)
            {
                if (color.r<cmin)
                    cmin = color.r;
                if (color.r>cmax)
                    cmax = color.r;
            }
            float delta = cmax -cmin;
            for (int i = 0; i<colors.Length; i++)
            {
                colors[i].Red = (colors[i].Red - cmin)/delta;
            }

            var sourceLayer = new NxRHI.TtTextureUtility.TtTex2dLayer();
            sourceLayer.Pixels = colors;
            sourceLayer.Width = width;
            sourceLayer.Height = height;
            List<NxRHI.TtTextureUtility.TtTex2dLayer> mipDatas = new List<NxRHI.TtTextureUtility.TtTex2dLayer>();
            mipDatas.Add(sourceLayer);
            int MaxLayer = 1;
            int w = width/2;
            int h = height/2;
            while (w>=1 && h>=1)
            {
                if (mipDatas.Count>=MaxLayer)
                    break;
                var next = NxRHI.TtTextureUtility.GenerateMipLayer2D(sourceLayer, w, h);
                mipDatas.Add(next);
                w = w/2;
                h = h/2;
            }

            NxRHI.FMappedSubResource* initData = stackalloc NxRHI.FMappedSubResource[mipDatas.Count];
            try
            {
                for (int i = 0; i<mipDatas.Count; i++)
                {
                    initData[i].RowPitch = (uint)(mipDatas[i].Width * sizeof(Color4b));
                    initData[i].DepthPitch = (uint)(initData[i].RowPitch * mipDatas[i].Height);
                    initData[i].pData = mipDatas[i].CreateColorR8G8B8A8();
                }

                var texDesc = new NxRHI.FTextureDesc();
                texDesc.SetDefault();
                texDesc.Width = (uint)width;
                texDesc.Height = (uint)height;
                texDesc.Depth = (uint)0;
                texDesc.Format = EPixelFormat.PXF_R8G8B8A8_UNORM;
                texDesc.MipLevels = (uint)mipDatas.Count;
                texDesc.InitData = initData;
                return TtEngine.Instance.GfxDevice.RenderContext.CreateTexture(in texDesc);
            }
            finally
            {
                for (int i = 0; i<mipDatas.Count; i++)
                {
                    mipDatas[i].DesctroyPixels(initData[i].pData);
                }
            }
        }
        private float[] GenerateFractalPerlinNoise(int size)
        {
            float[] noise = new float[size * size * size];

            for (int z = 0; z < size; z++)
            {
                for (int y = 0; y < size; y++)
                {
                    for (int x = 0; x < size; x++)
                    {
                        float nx = (float)x / size * Scale.x;
                        float ny = (float)y / size * Scale.y;
                        float nz = (float)z / size * Scale.z;

                        float value = (float)Perlin3D.GetPerlinValue(TtPerlin2.EFbmMode.Classic, new DVector3(nx, ny, nz), Octaves, Frequency, 1, Lacunarity, Gain);
                        noise[x + y * size + z * size * size] = value;
                    }
                }
            }

            return noise;
        }
        private float[] GenerateFractalWorleyNoise(int size)
        {
            float[] noise = new float[size * size * size];

            for (int z = 0; z < size; z++)
            {
                for (int y = 0; y < size; y++)
                {
                    for (int x = 0; x < size; x++)
                    {
                        float nx = (float)x / size * Scale.x;
                        float ny = (float)y / size * Scale.y;
                        float nz = (float)z / size * Scale.z;

                        float value = Worly3D.GetWorleyValue(nx, ny, nz);
                        noise[x + y * size + z * size * size] = value;
                    }
                }
            }

            return noise;
        }
    }

    public class TtVolumeCloudShading : Graphics.Pipeline.Shader.TtGraphicsShadingEnv
    {
        public TtVolumeCloudShading()
        {
            CodeName = RName.GetRName("shaders/bricks/fx/volumecloud.cginc", RName.ERNameType.Engine);

            this.UpdatePermutation();
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
            var aaNode = drawcall.TagObject as TtVolumeCloudNode;

            var index = drawcall.FindBinder("ColorBuffer");
            if (index.IsValidPointer)
            {
                var attachBuffer = aaNode.GetAttachBuffer(aaNode.ColorPinIn);
                drawcall.BindSRV(index, attachBuffer.Srv);
            }
            index = drawcall.FindBinder("WeatherTex");
            if (index.IsValidPointer)
            {
                drawcall.BindSRV(index, aaNode.SceneNode.NoiseGen.WeatherSrv);
            }
            index = drawcall.FindBinder("CloudNoiseTex");
            if (index.IsValidPointer)
            {
                drawcall.BindSRV(index, aaNode.SceneNode.NoiseGen.CloudNoiseSrv);
            }
            index = drawcall.FindBinder("cbShadingEnv");
            if (index.IsValidPointer)
            {
                if (aaNode.ShadingCbv == null)
                {
                    aaNode.ShadingCbv = TtEngine.Instance.GfxDevice.RenderContext.CreateCBV(index);
                }
                drawcall.BindCBV(index, aaNode.ShadingCbv);
            }
            base.OnDrawCall(cmd, drawcall, policy, atom);
        }
    }

    [Bricks.CodeBuilder.ContextMenu("VolumeCloud", "Post\\VolumeCloud", Bricks.RenderPolicyEditor.UPolicyGraph.RGDEditorKeyword)]
    public class TtVolumeCloudNode : Graphics.Pipeline.Common.TAuxSceenSpaceNode<TtVolumeCloudNode>
    {
        public Graphics.Pipeline.TtRenderGraphPin ColorPinIn = Graphics.Pipeline.TtRenderGraphPin.CreateInputOutput("Color", NxRHI.EBufferType.BFT_SRV);
        public TtVolumeCloudNode()
        {
            Name = "VolumeCloud";
        }
        public override void Dispose()
        {
            SceneNode = null;
            CoreSDK.DisposeObject(ref ShadingCbv);
            base.Dispose();
        }
        public override void InitNodePins()
        {
            AddInputOutput(ColorPinIn);
            base.InitNodePins();
        }
        public TtVolumeCloudShading mBasePassShading;
        public override Graphics.Pipeline.Shader.TtGraphicsShadingEnv GetPassShading(Graphics.Mesh.TtRenderMesh.TtAtom atom = null)
        {
            return mBasePassShading;
        }
        public override async Thread.Async.TtTask Initialize(Graphics.Pipeline.TtRenderPolicy policy, string debugName)
        {
            await base.Initialize(policy, debugName);
            mBasePassShading = await TtEngine.Instance.ShadingEnvManager.GetShadingEnv<TtVolumeCloudShading>();
        }
        public override void TickLogic(TtWorld world, Graphics.Pipeline.TtRenderPolicy policy, NxRHI.TtCommandList frameCmdList, bool bClear)
        {
            if (SceneNode==null)
            {
                MoveAttachment(ColorPinIn, ResultPinOut);
                return;
            }
            if (ShadingCbv!=null && SceneNode!=null)
            {
                ShadingCbv.SetValue("ShadingStruct", in SceneNode.ShadingStruct);
            }
            base.TickLogic(world, policy, frameCmdList, bClear);
        }
        #region Rhi Resouces
        public NxRHI.TtCbView ShadingCbv;
        
        public TtVolumeCloudSceneNode SceneNode;
        #endregion
    }

    [Bricks.CodeBuilder.ContextMenu("VolumeCloud", "Graphics\\VolumeCloud", TtNode.EditorKeyword)]
    [TtNode(NodeDataType = typeof(TtVolumeCloudSceneNode.TtThisNodeData), DefaultNamePrefix = "VolumeCloud")]
    public class TtVolumeCloudSceneNode : GamePlay.Scene.TtVisual
    {
        public class TtThisNodeData : TtNodeData
        {
            public TtThisNodeData()
            {
                mShadingStruct.SetDefault();
            }
            [Rtti.Meta("")]
            [Category("Option")]
            public TtCloudNoiseGenerator NoiseGen { get; set; } = new TtCloudNoiseGenerator();

            internal FShadingStruct mShadingStruct = new FShadingStruct();
            [Rtti.Meta("")]
            public FShadingStruct ShadingStruct
            {
                get => mShadingStruct;
                set => mShadingStruct =value;
            }
        }
        [Rtti.Meta("")]
        [Category("Option")]
        public TtCloudNoiseGenerator NoiseGen 
        { 
            get =>GetNodeData<TtThisNodeData>()?.NoiseGen; 
        } 
        public class TtReGenTexture : EGui.Controls.PropertyGrid.TtButtonAttribute
        {
            bool IsGenarating = false;
            protected override void OnButtonClick(in EditorInfo info)
            {
                if (IsGenarating)
                    return;
                if (info.ObjectInstance.GetType().GetInterface("IList")!=null)
                {
                    IsGenarating = true;
                    var lst = info.ObjectInstance as System.Collections.IList;
                    foreach (var i in lst)
                    {
                        var node = i as TtVolumeCloudSceneNode;
                        if (node!=null)
                        {
                            node.NoiseGen.ReGenRenderResources().AddWaitTask((task)=>
                            {
                                IsGenarating = false;
                            });
                        }
                    }
                }
                else
                {
                    IsGenarating = true;
                    var node = info.ObjectInstance as TtVolumeCloudSceneNode;
                    if (node!=null)
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
        [Category("Option")]
        public bool ReGenTexture
        {
            get
            {
                return false;
            }
            set
            {

            }
        }
        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 16)]
        public struct FShadingStruct
        {
            public void SetDefault()
            {
                CloudColor = Color4b.WhiteSmoke.ToVector4();
                ShadowColor = Color4b.DarkGray.ToVector4();
                LightColor = Color4b.LightGoldenrodYellow.ToVector4();

                CloudDensity = 1;
                CloudCoverage = 1;
                CloudHeightMin = -100;
                CloudHeightMax = 200;

                CloudScale = new Vector2(1, 1);
                LightAbsorption = 0.5f;
                DarknessThreshold = 0.01f;

                LightDir = Vector3.Down;
                MaxSteps = 32;
            }
            public Vector4 CloudColor;
            public Vector4 ShadowColor;
            public Vector4 LightColor;

            public float CloudDensity;
            public float CloudCoverage;
            public float CloudHeightMin;
            public float CloudHeightMax;

            public Vector2 CloudScale;
            public float LightAbsorption;
            public float DarknessThreshold;

            public Vector3 LightDir;
            public int MaxSteps;
        }
        public ref FShadingStruct ShadingStruct => ref GetNodeData<TtThisNodeData>().mShadingStruct;

        [EGui.Controls.PropertyGrid.TtColor4PickerEditor]
        [Category("Option")]
        public Vector4 CloudColor
        {
            get => GetNodeData<TtThisNodeData>().mShadingStruct.CloudColor;
            set => GetNodeData<TtThisNodeData>().mShadingStruct.CloudColor = value;
        }
        [EGui.Controls.PropertyGrid.TtColor4PickerEditor]
        [Category("Option")]
        public Vector4 ShadowColor
        {
            get => GetNodeData<TtThisNodeData>().mShadingStruct.ShadowColor;
            set => GetNodeData<TtThisNodeData>().mShadingStruct.ShadowColor = value;
        }
        [EGui.Controls.PropertyGrid.TtColor4PickerEditor]
        [Category("Option")]
        public Vector4 LightColor
        {
            get => GetNodeData<TtThisNodeData>().mShadingStruct.LightColor;
            set => GetNodeData<TtThisNodeData>().mShadingStruct.LightColor = value;
        }
        [Category("Option")]
        public float CloudDensity
        {
            get => GetNodeData<TtThisNodeData>().mShadingStruct.CloudDensity;
            set => GetNodeData<TtThisNodeData>().mShadingStruct.CloudDensity = value;
        }
        [Category("Option")]
        public float CloudCoverage
        {
            get => GetNodeData<TtThisNodeData>().mShadingStruct.CloudCoverage;
            set => GetNodeData<TtThisNodeData>().mShadingStruct.CloudCoverage = value;
        }
        [Category("Option")]
        public float CloudHeightMin
        {
            get => GetNodeData<TtThisNodeData>().mShadingStruct.CloudHeightMin;
            set => GetNodeData<TtThisNodeData>().mShadingStruct.CloudHeightMin = value;
        }
        [Category("Option")]
        public float CloudHeightMax
        {
            get => GetNodeData<TtThisNodeData>().mShadingStruct.CloudHeightMax;
            set => GetNodeData<TtThisNodeData>().mShadingStruct.CloudHeightMax = value;
        }
        [Category("Option")]
        public Vector2 CloudScale
        {
            get => GetNodeData<TtThisNodeData>().mShadingStruct.CloudScale;
            set => GetNodeData<TtThisNodeData>().mShadingStruct.CloudScale = value;
        }
        [Category("Option")]
        public float LightAbsorption
        {
            get => GetNodeData<TtThisNodeData>().mShadingStruct.LightAbsorption;
            set => GetNodeData<TtThisNodeData>().mShadingStruct.LightAbsorption = value;
        }
        [Category("Option")]
        public float DarknessThreshold
        {
            get => GetNodeData<TtThisNodeData>().mShadingStruct.DarknessThreshold;
            set => GetNodeData<TtThisNodeData>().mShadingStruct.DarknessThreshold = value;
        }
        [Category("Option")]
        public Vector3 LightDir
        {
            get => GetNodeData<TtThisNodeData>().mShadingStruct.LightDir;
            set => GetNodeData<TtThisNodeData>().mShadingStruct.LightDir = value;
        }
        [Category("Option")]
        public int MaxSteps
        {
            get => GetNodeData<TtThisNodeData>().mShadingStruct.MaxSteps;
            set => GetNodeData<TtThisNodeData>().mShadingStruct.MaxSteps = value;
        }
        public TtVolumeCloudNode RenderNode = null;
        public override void Dispose()
        {
            if (RenderNode!=null)
            {
                RenderNode.SceneNode = null;
            }
            if (NoiseGen!=null)
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
            if (RenderNode==null)
            {
                RenderNode = this.GetWorld().ViewportSlate.RenderPolicy?.FindFirstNode<TtVolumeCloudNode>();
                if (RenderNode!=null)
                {
                    RenderNode.SceneNode = this;
                }
            }
            else
            {
                if (GetWorld().GetSun()!=null)
                {
                    this.LightDir = GetWorld().GetSun().DirectionLight.Direction;
                }
            }
            return base.OnTickLogic(args);
        }
    }
}
