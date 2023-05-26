using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.Particle
{
    public class UNebulaShader
    {
        public static readonly Vector3ui Dispatch_SetupDimArray1 = new Vector3ui(32, 1, 1);
        public NxRHI.UComputeEffect Particle_Update;

        public NxRHI.UComputeEffect Particle_SetupParameters;

        public IParticleEmitter Emitter;
        public RName NebulaName;
        public NxRHI.UShaderCode ParticleVar;
        public NxRHI.UShaderCode SystemData;
        public NxRHI.UShaderCode CBufferVar;
        public NxRHI.UShaderCode HLSLDefine;
        public NxRHI.UShaderCode HLSLCode;
        public class UNebulaInclude : EngineNS.Editor.ShaderCompiler.UHLSLInclude
        {
            public UNebulaShader Host;
            public override unsafe NxRHI.FShaderCode* GetHLSLCode(string includeName, out bool bIncluded)
            {
                if (includeName.EndsWith("/ParticleVar"))
                {
                    bIncluded = true;
                    return Host.ParticleVar.mCoreObject;
                }
                else if (includeName.EndsWith("/ParticleSystemVar"))
                {
                    bIncluded = true;
                    return Host.SystemData.mCoreObject;
                }
                else if (includeName.EndsWith("/NebulaModifierDefine"))
                {
                    bIncluded = true;
                    return Host.HLSLDefine.mCoreObject;
                }
                else if (includeName.EndsWith("/NebulaModifier"))
                {
                    bIncluded = true;
                    return Host.HLSLCode.mCoreObject;
                }
                else if (includeName.EndsWith("/ParticleCBufferVar"))
                {
                    bIncluded = true;
                    return Host.CBufferVar.mCoreObject;
                }                
                else
                {
                    bIncluded = false;
                    return (NxRHI.FShaderCode*)0;
                }
            }
        }

        public void Init(RName nebula, IParticleEmitter emitter, string particleVar, string sysData, string cbVar, string define, string code)
        {
            var defines = new NxRHI.UShaderDefinitions();
            defines.mCoreObject.AddDefine("DispatchX", $"{Dispatch_SetupDimArray1.X}");
            defines.mCoreObject.AddDefine("DispatchY", $"{Dispatch_SetupDimArray1.Y}");
            defines.mCoreObject.AddDefine("DispatchZ", $"{Dispatch_SetupDimArray1.Z}");
            defines.mCoreObject.AddDefine("BufferHeadSize", $"{UGpuParticleResources.BufferHeadSize*4}");

            ParticleVar = new NxRHI.UShaderCode();
            ParticleVar.TextCode = particleVar;
            SystemData = new NxRHI.UShaderCode();
            SystemData.TextCode = sysData;
            HLSLDefine = new NxRHI.UShaderCode();
            HLSLDefine.TextCode = define;
            HLSLCode = new NxRHI.UShaderCode();
            HLSLCode.TextCode = code;
            CBufferVar = new NxRHI.UShaderCode();
            CBufferVar.TextCode = cbVar;

            var rc = UEngine.Instance.GfxDevice.RenderContext;
            var incProvider = new UNebulaInclude();
            incProvider.Host = this;
            Particle_Update = UEngine.Instance.GfxDevice.EffectManager.GetComputeEffect(RName.GetRName("Shaders/Bricks/Particle/Particle.compute", RName.ERNameType.Engine),
                "CS_Particle_Update", NxRHI.EShaderType.SDT_ComputeShader, null, defines, incProvider);

            Particle_SetupParameters = UEngine.Instance.GfxDevice.EffectManager.GetComputeEffect(RName.GetRName("Shaders/Bricks/Particle/Particle.compute", RName.ERNameType.Engine),
                "CS_Particle_SetupParameters", NxRHI.EShaderType.SDT_ComputeShader, null, defines, incProvider);
        }
    }
    public class UNebulaShaderManager
    {
        public Dictionary<Hash160, UNebulaShader> Shaders { get; } = new Dictionary<Hash160, UNebulaShader>();
    }
    public class UNebulaTemplateManager : UModule<UEngine>
    {
        public uint ShaderRandomPoolSize = 65535;
        public NxRHI.UBuffer RandomPoolBuffer;
        public Random mRandom = new Random((int)Support.Time.GetTickCount());

        public NxRHI.USrView RandomPoolSrv;
        public UNebulaShaderManager NebulaShaderManager { get; } = new UNebulaShaderManager();
        public Dictionary<RName, UNebulaParticle> Particles { get; } = new Dictionary<RName, UNebulaParticle>();
        public float RandomSignedUnit()//[-1,1]
        {
            return ((float)mRandom.NextDouble() - 0.5f) * 2.0f;
        }
        public override async System.Threading.Tasks.Task<bool> Initialize(UEngine host)
        {
            await Thread.TtAsyncDummyClass.DummyFunc();
            
            var rc = UEngine.Instance.GfxDevice.RenderContext;
            unsafe
            {
                var bfDesc = new NxRHI.FBufferDesc();

                bfDesc.SetDefault(false);
                bfDesc.Type = NxRHI.EBufferType.BFT_UAV | NxRHI.EBufferType.BFT_SRV;
                //bfDesc.MiscFlags = (UInt32)(EResourceMiscFlag.BUFFER_ALLOW_RAW_VIEWS);
                bfDesc.Size = (uint)sizeof(Vector4) * ShaderRandomPoolSize;
                bfDesc.StructureStride = (uint)sizeof(Vector4);
                var initData = new Vector4[ShaderRandomPoolSize];
                for (int i = 0; i < ShaderRandomPoolSize; i++)
                {
                    initData[i] = new Vector4(RandomSignedUnit(), RandomSignedUnit(), RandomSignedUnit(), RandomSignedUnit());
                }
                fixed (Vector4* pAddr = &initData[0])
                {
                    bfDesc.InitData = pAddr;
                    RandomPoolBuffer = rc.CreateBuffer(in bfDesc);
                }

                var srvDesc = new NxRHI.FSrvDesc();
                srvDesc.SetBuffer(0);
                srvDesc.Buffer.FirstElement = 0;
                srvDesc.Buffer.NumElements = ShaderRandomPoolSize;// (uint)sizeof(Vector4);
                srvDesc.Buffer.StructureByteStride = (uint)sizeof(Vector4);
                //srvDesc.Type = NxRHI.ESrvType.ST_BufferEx;
                //srvDesc.Format = EPixelFormat.PXF_R32_TYPELESS;
                //srvDesc.BufferEx.Flags = 1;
                //srvDesc.BufferEx.NumElements = ShaderRandomPoolSize;
                RandomPoolSrv = rc.CreateSRV(RandomPoolBuffer, in srvDesc);
            }
            return true;
        }
        public async System.Threading.Tasks.Task<UNebulaParticle> GetParticle(RName name)
        {
            if (name == null)
                return null;
            UNebulaParticle result;
            if (Particles.TryGetValue(name, out result))
                return result.CloneNebula();

            {//temp code
                result = new UNebulaParticle();
                result.AssetName = name;
                var Emitter = result.AddEmitter(typeof(Simple.USimpleEmitter), "emitter0") as Simple.USimpleEmitter;
                Emitter.IsGpuDriven = true;

                var umesh = await UEngine.Instance.GfxDevice.MaterialMeshManager.GetMaterialMesh(RName.GetRName("utest/mesh/unit_sphere.ums"));
                var mesh = new Graphics.Mesh.UMesh();
                mesh.Initialize(umesh, Rtti.UTypeDescGetter<Simple.USimpleMdfQueue>.TypeDesc);
                Emitter.InitEmitter(UEngine.Instance.GfxDevice.RenderContext, mesh, 1024);

                var sphereShape = new Bricks.Particle.UShapeSphere();
                sphereShape.Radius = 10.0f;
                sphereShape.Thinness = 0.1f;
                var boxShape = new Bricks.Particle.UShapeBox();
                boxShape.Thinness = 0.2f;
                Emitter.EmitterShapes.Add(sphereShape);
                Emitter.EmitterShapes.Add(boxShape);
                var ef1 = new UAcceleratedEffector();
                ef1.Acceleration = new Vector3(0, -0.1f, 0);
                Emitter.AddEffector("default", ef1);
                Emitter.SetCurrentQueue("default");

                var nblMdf = mesh.MdfQueue as Simple.USimpleMdfQueue;
                nblMdf.Emitter = Emitter;

                UEngine.Instance.NebulaTemplateManager.UpdateShaders(result);
            }

            Particles.Add(name, result);
            return result.CloneNebula();
        }
        public void UpdateShaders(UNebulaParticle particle)
        {
            foreach (var i in particle.Emitter.Values)
            {
                var particleVar = i.GetParticleDefine();
                var sysData = i.GetSystemDataDefine();
                var defineCode = "";
                var emitterCode = i.GetHLSL();
                string cbVarDefine = i.GetCBufferDefines();
                foreach (var j in i.EffectorQueues.Values)
                {
                    var hlslVar = cbVarDefine + j.GetCBufferDefines();
                    var hlslDefine = defineCode + j.GetParametersDefine();
                    var hlsl = emitterCode + j.GetHLSL();
                    var hash = Hash160.CreateHash160(particleVar + sysData + hlslVar + hlslDefine + hlsl);
                    UNebulaShader shader;
                    if (NebulaShaderManager.Shaders.TryGetValue(hash, out shader) == false)
                    {
                        shader = new UNebulaShader();
                        shader.Init(particle.AssetName, i, particleVar, sysData, hlslVar, hlslDefine, hlsl);
                        NebulaShaderManager.Shaders.Add(hash, shader);
                    }
                    j.Shader = shader;
                }
            }
        }
    }
}

namespace EngineNS
{
    partial class UEngine
    {
        public Bricks.Particle.UNebulaTemplateManager NebulaTemplateManager
        {
            get;
        } = new Bricks.Particle.UNebulaTemplateManager();
    }
}
