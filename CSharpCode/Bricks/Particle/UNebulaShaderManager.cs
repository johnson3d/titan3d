using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.Particle
{
    public class TtNebulaShader
    {
        public static readonly Vector3ui Dispatch_SetupDimArray1 = new Vector3ui(32, 1, 1);
        public NxRHI.TtComputeEffect Particle_Update;

        public TtEmitter Emitter;
        public RName NebulaName;
        public NxRHI.TtShaderCode CBufferVar;
        public NxRHI.TtShaderCode HLSLDefine;
        public NxRHI.TtShaderCode HLSLCode;
        public class UNebulaInclude : EngineNS.Editor.ShaderCompiler.TtHLSLInclude
        {
            public TtNebulaShader Host;
            public override unsafe NxRHI.FShaderCode* GetHLSLCode(string includeName, out bool bIncluded)
            {
                if (includeName.EndsWith("/NebulaModifierDefine"))
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

        public async Thread.Async.TtTask<bool> Init(Hash160 codeHash, RName nebula, TtEmitter emitter, string cbVar, string define, string code)
        {
            var defines = new NxRHI.TtShaderDefinitions();
            defines.mCoreObject.AddDefine("DispatchX", $"{Dispatch_SetupDimArray1.X}");
            defines.mCoreObject.AddDefine("DispatchY", $"{Dispatch_SetupDimArray1.Y}");
            defines.mCoreObject.AddDefine("DispatchZ", $"{Dispatch_SetupDimArray1.Z}");
            defines.mCoreObject.AddDefine("NebulaCodeHash", $"{codeHash}");

            HLSLDefine = new NxRHI.TtShaderCode();
            HLSLDefine.TextCode = define;
            HLSLCode = new NxRHI.TtShaderCode();
            HLSLCode.TextCode = code;
            CBufferVar = new NxRHI.TtShaderCode();
            CBufferVar.TextCode = cbVar;

            var rc = TtEngine.Instance.GfxDevice.RenderContext;
            var incProvider = new UNebulaInclude();
            incProvider.Host = this;
            Particle_Update = await TtEngine.Instance.GfxDevice.EffectManager.GetComputeEffect(RName.GetRName("Shaders/Bricks/Particle/Particle.compute", RName.ERNameType.Engine),
                "CS_Particle_Update", NxRHI.EShaderType.SDT_ComputeShader, null, defines, incProvider);

            return true;
        }
    }
    public class UNebulaShaderManager
    {
        public Dictionary<Hash160, TtNebulaShader> Shaders { get; } = new Dictionary<Hash160, TtNebulaShader>();
    }
    public class UNebulaTemplateManager : UModule<TtEngine>
    {
        public uint ShaderRandomPoolSize = 65535;
        public NxRHI.TtBuffer RandomPoolBuffer;
        public Random mRandom = new Random((int)Support.TtTime.GetTickCount());

        public NxRHI.TtSrView RandomPoolSrv;
        public UNebulaShaderManager NebulaShaderManager { get; } = new UNebulaShaderManager();
        public Dictionary<RName, TtNebulaParticle> Particles { get; } = new Dictionary<RName, TtNebulaParticle>();
        public float RandomSignedUnit()//[-1,1]
        {
            return ((float)mRandom.NextDouble() - 0.5f) * 2.0f;
        }
        public override async System.Threading.Tasks.Task<bool> Initialize(TtEngine host)
        {
            await Thread.TtAsyncDummyClass.DummyFunc();
            
            var rc = TtEngine.Instance.GfxDevice.RenderContext;
            unsafe
            {
                var bfDesc = new NxRHI.FBufferDesc();

                bfDesc.SetDefault(false, NxRHI.EBufferType.BFT_UAV | NxRHI.EBufferType.BFT_SRV);
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
                srvDesc.SetBuffer(false);
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
        public async System.Threading.Tasks.Task<TtNebulaParticle> GetParticle(RName name)
        {
            if (name == null)
                return null;
            TtNebulaParticle result;
            if (Particles.TryGetValue(name, out result))
                return await result.CloneNebula();

            result = await TtNebulaParticle.LoadAsset(name, false);
            
            Particles.Add(name, result);
            return await result.CloneNebula();
        }
        public async System.Threading.Tasks.Task<TtNebulaParticle> CreateParticle(RName name)
        {
            if (name == null)
                return null;
            
            return await TtNebulaParticle.LoadAsset(name, true);

            //return await result.CloneNebula();
        }
        public async Thread.Async.TtTask<bool> UpdateShaders(TtNebulaParticle particle)
        {
            foreach (var i in particle.Emitter.Values)
            {
                var defineCode = "";
                var emitterCode = i.GetEmitShapeHLSL();
                string cbVarDefine = i.GetCBufferDefines();
                foreach (var j in i.EffectorQueues.Values)
                {
                    var hlslVar = cbVarDefine + j.GetCBufferDefines();
                    var hlslDefine = defineCode + j.GetParametersDefine();
                    var hlsl = emitterCode + j.GetHLSL();
                    var hash = Hash160.CreateHash160(hlslVar + hlslDefine + hlsl);
                    TtNebulaShader shader;
                    if (NebulaShaderManager.Shaders.TryGetValue(hash, out shader) == false)
                    {
                        shader = new TtNebulaShader();
                        await shader.Init(hash, particle.AssetName, i, hlslVar, hlslDefine, hlsl);
                        NebulaShaderManager.Shaders.Add(hash, shader);
                    }
                    j.Shader = shader;
                }
            }
            return true;
        }
    }
}

namespace EngineNS
{
    partial class TtEngine
    {
        public Bricks.Particle.UNebulaTemplateManager NebulaTemplateManager
        {
            get;
        } = new Bricks.Particle.UNebulaTemplateManager();
    }
}
