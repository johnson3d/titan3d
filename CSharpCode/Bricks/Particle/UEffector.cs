using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.Particle
{
    public class TtEffectorQueue : IDisposable
    {
        public void Dispose()
        {
            CoreSDK.DisposeObject(ref CBuffer);
            CoreSDK.DisposeObject(ref mParticleUpdateDrawcall);
        }
        public string Name;
        public List<TtEffector> Effectors { get; } = new List<TtEffector>();
        public UNebulaShader Shader { get; set; }
        public NxRHI.UCbView CBuffer;
        public NxRHI.UComputeDraw mParticleUpdateDrawcall;

        public unsafe void UpdateComputeDrawcall(NxRHI.UGpuDevice rc, TtEmitter emitter)
        {
            TtGpuParticleResources gpuResources = emitter.GpuResources;

            var coreBinder = UEngine.Instance.GfxDevice.CoreShaderBinder;
            if (mParticleUpdateDrawcall == null)
            {
                mParticleUpdateDrawcall = rc.CreateComputeDraw();
                coreBinder.CBPerParticle.UpdateFieldVar(Shader.Particle_Update.mComputeShader, "cbParticleDesc");
                CBuffer = rc.CreateCBV(coreBinder.CBPerParticle.Binder.mCoreObject);

                CBuffer.SetValue(coreBinder.CBPerParticle.ParticleRandomPoolSize, UEngine.Instance.NebulaTemplateManager.ShaderRandomPoolSize);
                var dpDesc = emitter.Mesh.SubMeshes[0].Atoms[0].GetMeshAtomDesc(0);
                CBuffer.SetValue(coreBinder.CBPerParticle.Draw_IndexCountPerInstance, dpDesc->m_NumPrimitives * 3);
                CBuffer.SetValue(coreBinder.CBPerParticle.Draw_StartIndexLocation, dpDesc->m_StartIndex);
                CBuffer.SetValue(coreBinder.CBPerParticle.Draw_BaseVertexLocation, dpDesc->m_BaseVertexIndex);
                CBuffer.SetValue(coreBinder.CBPerParticle.Draw_StartInstanceLocation, 0);
                CBuffer.SetValue(coreBinder.CBPerParticle.ParticleMaxSize, emitter.MaxParticle);
                
                CBuffer.SetValue(coreBinder.CBPerParticle.AllocatorCapacity, emitter.MaxParticle);
                CBuffer.SetValue(coreBinder.CBPerParticle.CurAliveCapacity, emitter.MaxParticle);
                CBuffer.SetValue(coreBinder.CBPerParticle.BackendAliveCapacity, emitter.MaxParticle);
                CBuffer.SetValue(coreBinder.CBPerParticle.ParticleCapacity, emitter.MaxParticle);

                mParticleUpdateDrawcall.SetComputeEffect(Shader.Particle_Update);

                mParticleUpdateDrawcall.BindCBuffer("cbParticleDesc", CBuffer);
                mParticleUpdateDrawcall.BindSrv("bfRandomPool", UEngine.Instance.NebulaTemplateManager.RandomPoolSrv);

                mParticleUpdateDrawcall.BindUav("bfParticles", gpuResources.ParticlesBuffer.Uav);
                mParticleUpdateDrawcall.BindUav("bfSystemData", gpuResources.SystemDataBuffer.Uav);
                mParticleUpdateDrawcall.BindUav("bfFreeParticles", gpuResources.AllocatorBuffer.Uav);

                mParticleUpdateDrawcall.BindUav("bfDrawArg", gpuResources.DrawArgUav);
                mParticleUpdateDrawcall.BindUav("bfDispatchArg", gpuResources.DispatchArgUav);

                mParticleUpdateDrawcall.BindIndirectDispatchArgsBuffer(gpuResources.DispatchArgBuffer);
            }

            CBuffer.SetValue(coreBinder.CBPerParticle.ParticleElapsedTime, UEngine.Instance.ElapsedSecond);
            CBuffer.SetValue(coreBinder.CBPerParticle.ParticleRandomSeed, emitter.RandomNext());

            emitter.SetCBuffer(CBuffer);

            uint index = 0;
            foreach (var i in emitter.EmitterShapes)
            {
                i.SetCBuffer(index, CBuffer);
                index++;
            }

            index = 0;
            foreach (var i in this.Effectors)
            {
                i.SetCBuffer(index, CBuffer);
                index++;
            }

            mParticleUpdateDrawcall.BindUav("bfCurAlives", gpuResources.CurAlivesBuffer.Uav);
            mParticleUpdateDrawcall.BindUav("bfBackendAlives", gpuResources.BackendAlivesBuffer.Uav);
        }
        public string GetParametersDefine()
        {
            string result = "";
            foreach (var i in Effectors)
            {
                result += i.GetParametersDefine();
            }
            return result;
        }
        public string GetHLSL()
        {
            string result = "";
            foreach (var i in Effectors)
            {
                result += i.GetHLSL();
            }
            var codeBuilder = new Bricks.CodeBuilder.Backends.UHLSLCodeGenerator();
            string sourceCode = "";
            //var codeBuilder = new Bricks.CodeBuilder.HLSL.UHLSLGen();
            codeBuilder.AddLine("\nvoid DoParticleEffectors(uint3 id, inout FParticle particle)", ref sourceCode);
            codeBuilder.PushSegment(ref sourceCode);
            int index = 0;
            foreach (var i in Effectors)
            {
                codeBuilder.AddLine($"{i.Name}_EffectorExecute(id, particle, EffectorParameters{index});", ref sourceCode);
                index++;
            }
            codeBuilder.PopSegment(ref sourceCode);

            result += sourceCode;
            return result;
        }
        public string GetCBufferDefines()
        {
            string result = "";
            int index = 0;
            foreach (var i in Effectors)
            {
                result += $"{i.Name}_EffectorParameters EffectorParameters{index};\n";
                index++;
            }
            return result;
        }
        public unsafe void Update(TtEmitter emiter, float elapsed)
        {
            var pParticles = (FParticleBase*)emiter.mCoreObject.GetParticleAddress();
            var pAlives = emiter.mCoreObject.GetCurrentAliveAddress();
            uint aliveNum = emiter.mCoreObject.GetLiveNumber();
            foreach (var e in Effectors)
            {
                for (uint i = 0; i < aliveNum; i++)
                {
                    var index = pAlives[i];
                    var cur = (FParticleBase*)&pParticles[index];
                    if (cur->Life <= 0)
                    {
                        emiter.OnDeadParticle(index, ref pParticles[index]);
                        continue;
                    }
                    e.DoEffect(emiter, elapsed, cur);
                }
            }
        }
    }

    public class TtEffector
    {
        public virtual string Name
        {
            get { return "NullEffector"; }
        }
        public unsafe virtual void DoEffect(TtEmitter emitter, float elapsed, void* particle)
        {
            DoEffect(emitter, elapsed, ref *(FParticleBase*)particle);
        }
        public unsafe virtual void DoEffect(TtEmitter emitter, float elapsed, ref FParticleBase particle)
        {

        }
        public virtual string GetParametersDefine()
        {
            return "";
        }
        public virtual string GetHLSL()
        {
            return "";
        }
        public virtual void SetCBuffer(uint index, NxRHI.UCbView CBuffer)
        {

        }
        public virtual TtEffector CloneEffector()
        {
            return null;
        }
    }

    public class TtAcceleratedEffector : TtEffector
    {
        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 16)]
        public struct FAcceleratedEffector
        {
            public Vector3 Acceleration;
            public uint FAcceleratedEffector_Pad0;
        };
        public override TtEffector CloneEffector()
        {
            var result = new TtAcceleratedEffector();
            result.mAcceleratedEffector = mAcceleratedEffector;
            return result;
        }
        public override string Name
        {
            get { return "Accelerated"; }
        }
        FAcceleratedEffector mAcceleratedEffector;
        public Vector3 Acceleration
        {
            get => mAcceleratedEffector.Acceleration;
            set => mAcceleratedEffector.Acceleration = value;
        }        
        public override string GetParametersDefine()
        {
            var codeBuilder = new Bricks.CodeBuilder.Backends.UHLSLCodeGenerator();
            string sourceCode = "";
            //var codeBuilder = new Bricks.CodeBuilder.HLSL.UHLSLGen();

            codeBuilder.AddLine($"struct {Name}_EffectorParameters", ref sourceCode);
            codeBuilder.PushSegment(ref sourceCode);
            {
                codeBuilder.AddLine("float3 Acceleration;", ref sourceCode);
            }
            codeBuilder.PopSegment(ref sourceCode);
            sourceCode += ";";

            return sourceCode;
        }
        public override string GetHLSL()
        {
            var codeBuilder = new Bricks.CodeBuilder.Backends.UHLSLCodeGenerator();
            string sourceCode = "";
            //var codeBuilder = new Bricks.CodeBuilder.HLSL.UHLSLGen();

            var code = IO.TtFileManager.ReadAllText($"{RName.GetRName("Shaders\\Bricks\\Particle\\Effectors.compute", RName.ERNameType.Engine).Address}");
            codeBuilder.AddLine(code, ref sourceCode);

            return sourceCode;
        }
        public override unsafe void DoEffect(TtEmitter emitter, float elapsed, void* particle)
        {
            ref var cur = ref *(FParticleBase*)particle;
            cur.Location += Acceleration * elapsed * (1.0f + emitter.RandomUnit() * 2.5f);
        }
        public override void SetCBuffer(uint index, NxRHI.UCbView CBuffer)
        {
            CBuffer.SetValue($"EffectorParameters{index}", in mAcceleratedEffector);
        }
    }
}
