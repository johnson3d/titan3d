using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace EngineNS.Bricks.Particle
{
    public class TtEffectorQueue : IDisposable
    {
        public void Dispose()
        {
            CoreSDK.DisposeObject(ref CBuffer);
            CoreSDK.DisposeObject(ref mParticleUpdateDrawcall);
            ForParameters.Reset();
        }
        public string Name;
        public List<TtEffector> Effectors { get; } = new List<TtEffector>();
        public TtNebulaShader Shader { get; set; }
        public NxRHI.UCbView CBuffer;
        public NxRHI.UComputeDraw mParticleUpdateDrawcall;

        public unsafe void UpdateComputeDrawcall(NxRHI.UGpuDevice rc, TtEmitter emitter)
        {
            TtGpuParticleResources gpuResources = emitter.GpuResources;

            var coreBinder = TtEngine.Instance.GfxDevice.CoreShaderBinder;
            if (mParticleUpdateDrawcall == null)
            {
                mParticleUpdateDrawcall = rc.CreateComputeDraw();
                coreBinder.CBPerParticle.UpdateFieldVar(Shader.Particle_Update.mComputeShader, "cbParticleDesc");
                CBuffer = rc.CreateCBV(coreBinder.CBPerParticle.Binder.mCoreObject);

                CBuffer.SetValue(coreBinder.CBPerParticle.ParticleRandomPoolSize, TtEngine.Instance.NebulaTemplateManager.ShaderRandomPoolSize);
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
                mParticleUpdateDrawcall.BindSrv("bfRandomPool", TtEngine.Instance.NebulaTemplateManager.RandomPoolSrv);

                mParticleUpdateDrawcall.BindUav("bfParticles", gpuResources.ParticlesBuffer.Uav);
                mParticleUpdateDrawcall.BindUav("bfEmitterData", gpuResources.SystemDataBuffer.Uav);
                mParticleUpdateDrawcall.BindUav("bfFreeParticles", gpuResources.AllocatorBuffer.Uav);

                mParticleUpdateDrawcall.BindUav("bfDrawArg", gpuResources.DrawArgUav);
                mParticleUpdateDrawcall.BindUav("bfDispatchArg", gpuResources.DispatchArgUav);

                mParticleUpdateDrawcall.BindIndirectDispatchArgsBuffer(gpuResources.DispatchArgBuffer);
            }

            CBuffer.SetValue(coreBinder.CBPerParticle.ParticleElapsedTime, TtEngine.Instance.ElapsedSecond);
            CBuffer.SetValue(coreBinder.CBPerParticle.ParticleRandomSeed, TtEngine.Instance.NebulaTemplateManager.mRandom.Next());

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
                result += $"#define USE_EFFECTOR_{i.Name}\n";
            }
            foreach (var i in Effectors)
            {
                result += i.GetHLSL();
            }
            var codeBuilder = new Bricks.CodeBuilder.UHLSLCodeGenerator();
            string sourceCode = "";
            //var codeBuilder = new Bricks.CodeBuilder.HLSL.UHLSLGen();
            codeBuilder.AddLine("\nvoid DoParticleEffectors(TtEmitter emt, inout FParticle particle)", ref sourceCode);
            codeBuilder.PushSegment(ref sourceCode);
            int index = 0;
            foreach (var i in Effectors)
            {
                codeBuilder.AddLine($"{i.Name}_EffectorExecute(emt, ParticleElapsedTime, particle, EffectorParameters{index});", ref sourceCode);
                index++;
            }
            codeBuilder.PopSegment(ref sourceCode);

            codeBuilder.AddLine("#define USER_PARTICLE_DOEFFECTORS", ref sourceCode);

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
        public unsafe void Update(TtEmitter emitter, float elapsed)
        {
            var pParticles = (FParticle*)emitter.mCoreObject.GetParticleAddress();
            var pAlives = emitter.mCoreObject.GetCurrentAliveAddress();
            uint aliveNum = emitter.mCoreObject.GetLiveNumber();
            
            bool bUseMT = true;
            if (bUseMT)
            {
                ForParameters.emitter = emitter;
                ForParameters.This = this;
                ForParameters.pParticles = pParticles;
                ForParameters.aliveNum = aliveNum;
                ForParameters.pAlives = pAlives;
                ForParameters.elapsed = elapsed;
            }

            foreach (var e in Effectors)
            {
                if (bUseMT == false)
                {
                    for (uint i = 0; i < aliveNum; i++)
                    {
                        var index = pAlives[i];
                        var cur = (FParticle*)&pParticles[index];
                        if (cur->Life <= 0)
                        {
                            emitter.OnDeadParticle(index, ref *cur);
                            continue;
                        }
                        e.DoEffect(emitter, elapsed, cur);
                    }
                }
                else
                {
                    ForParameters.effector = e;
                    var numTask = TtEngine.Instance.EventPoster.NumOfPool;
                    TtEngine.Instance.EventPoster.ParrallelFor((int)numTask, static (i, arg1, arg2, state) =>
                    {
                        var ForParameters = (TtForParameters)arg1;
                        int stride = (int)ForParameters.aliveNum / (int)state.UserArguments.NumOfParrallelFor + 1;
                        var start = i * stride;
                        for (int n = 0; n < stride; n++)
                        {
                            var nn = start + n;
                            if (nn >= ForParameters.aliveNum)
                                break;
                            var index = ForParameters.pAlives[nn];
                            var cur = (FParticle*)&ForParameters.pParticles[index];
                            if (cur->Life <= 0)
                            {
                                ForParameters.emitter.OnDeadParticle(index, ref *cur);
                                continue;
                            }
                            ForParameters.effector.DoEffect(ForParameters.emitter, ForParameters.elapsed, cur);
                        }
                    }, ForParameters);
                    ForParameters.effector = null;
                }
            }

            if (bUseMT == false)
            {
                for (uint i = 0; i < aliveNum; i++)
                {
                    var index = pAlives[i];
                    var cur = (FParticle*)&pParticles[index];
                    emitter.OnParticleTick(emitter, elapsed, ref *cur);
                    cur->Location += cur->Velocity * elapsed;
                }
            }
            else
            {
                var numTask = TtEngine.Instance.EventPoster.NumOfPool;
                TtEngine.Instance.EventPoster.ParrallelFor(numTask, static (i, arg1, arg2, state) =>
                {
                    var ForParameters = (TtForParameters)arg1;
                    int stride = (int)ForParameters.aliveNum / (int)state.UserArguments.NumOfParrallelFor + 1;
                    var start = i * stride;
                    for (int n = 0; n < stride; n++)
                    {
                        var nn = start + n;
                        if (nn >= ForParameters.aliveNum)
                            break;
                        var index = ForParameters.pAlives[nn];
                        var cur = (FParticle*)&ForParameters.pParticles[index];
                        ForParameters.emitter.OnParticleTick(ForParameters.emitter, ForParameters.elapsed, ref *cur);
                        cur->Location += cur->Velocity * ForParameters.elapsed;
                    }   
                }, ForParameters);
            }
            ForParameters.Reset();
        }
        private unsafe class TtForParameters
        {
            public TtEmitter emitter;
            public TtEffectorQueue This;
            public TtEffector effector;
            public FParticle* pParticles;
            public uint aliveNum;
            public uint* pAlives;
            public float elapsed;
            public void Reset()
            {
                emitter = null;
                This = null;
                pParticles = null;
                pAlives = null;
            }
        }
        private TtForParameters ForParameters = new TtForParameters();
    }

    public class TtEffector
    {
        public virtual string Name
        {
            get { return "NullEffector"; }
        }
        public unsafe virtual void DoEffect(TtEmitter emitter, float elapsed, void* particle)
        {
            DoEffect(emitter, elapsed, ref *(FParticle*)particle);
        }
        public unsafe virtual void DoEffect(TtEmitter emitter, float elapsed, ref FParticle particle)
        {

        }

        protected virtual void AddParameters(Bricks.CodeBuilder.UHLSLCodeGenerator codeBuilder, ref string sourceCode)
        {
            
        }
        protected void AddParameters(Bricks.CodeBuilder.UHLSLCodeGenerator codeBuilder, ref string sourceCode, System.Type paramType)
        {
            var members = paramType.GetFields();
            foreach (var i in members)
            {
                codeBuilder.AddLine($"{EngineNS.Editor.ShaderCompiler.TtShaderCodeManager.ToHLSLTypeString(i.FieldType)} {i.Name};", ref sourceCode);
            }
        }
        public string GetParametersDefine()
        {
            var codeBuilder = new Bricks.CodeBuilder.UHLSLCodeGenerator();
            string sourceCode = "";
            //var codeBuilder = new Bricks.CodeBuilder.HLSL.UHLSLGen();

            codeBuilder.AddLine($"struct {Name}_EffectorParameters", ref sourceCode);
            codeBuilder.PushSegment(ref sourceCode);
            {
                AddParameters(codeBuilder, ref sourceCode);
            }
            codeBuilder.PopSegment(ref sourceCode);
            sourceCode += ";";

            return sourceCode;
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

    public class TtAuxEffector<T> : TtEffector where T : unmanaged
    {
        protected T mEffectorParameter;
        public override TtEffector CloneEffector()
        {
            var result = Rtti.UTypeDescManager.CreateInstance(GetType()) as TtAuxEffector<T>;
            result.mEffectorParameter = mEffectorParameter;
            return result;
        }
        protected override void AddParameters(Bricks.CodeBuilder.UHLSLCodeGenerator codeBuilder, ref string sourceCode)
        {
            AddParameters(codeBuilder, ref sourceCode, typeof(T));
        }
        public override void SetCBuffer(uint index, NxRHI.UCbView CBuffer)
        {
            CBuffer.SetValue($"EffectorParameters{index}", in mEffectorParameter);
        }
        public override string GetHLSL()
        {
            var codeBuilder = new Bricks.CodeBuilder.UHLSLCodeGenerator();
            string sourceCode = "";
            //var codeBuilder = new Bricks.CodeBuilder.HLSL.UHLSLGen();

            var code = IO.TtFileManager.ReadAllText($"{RName.GetRName("Shaders\\Bricks\\Particle\\Effectors.compute", RName.ERNameType.Engine).Address}");
            codeBuilder.AddLine(code, ref sourceCode);

            return sourceCode;
        }
    }


    public class TtAcceleratedEffector : TtAuxEffector<TtAcceleratedEffector.FAcceleratedEffector>
    {
        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 16)]
        public struct FAcceleratedEffector
        {
            public Vector3 AccelerationMin;
            public uint FAcceleratedEffector_Pad0;
            public Vector3 AccelerationRange;
            public uint FAcceleratedEffector_Pad1;
        };
        public override string Name
        {
            get { return "Accelerated"; }
        }
        public Vector3 AccelerationMin
        {
            get => mEffectorParameter.AccelerationMin;
            set => mEffectorParameter.AccelerationMin = value;
        }
        public Vector3 AccelerationRange
        {
            get => mEffectorParameter.AccelerationRange;
            set => mEffectorParameter.AccelerationRange = value;
        }
        public override unsafe void DoEffect(TtEmitter emitter, float elapsed, void* particle)
        {
            ref var cur = ref *(FParticle*)particle;
            //cur.Location += Acceleration * elapsed * (1.0f + emitter.RandomUnit() * 2.5f);
            cur.Velocity += (AccelerationMin + AccelerationRange * emitter.RandomUnit(ref *(FParticle*)particle)) * elapsed;
        }
    }

    public class TtColorEffector : TtAuxEffector<TtColorEffector.FColorEffector>
    {
        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 16)]
        public struct FColorEffector
        {
            public Vector4 OpColorMin;
            public Vector4 OpColorRange;
        };
        public override string Name
        {
            get { return "Color"; }
        }
        public Vector4 OpColorMin
        {
            get => mEffectorParameter.OpColorMin;
            set => mEffectorParameter.OpColorMin = value;
        }
        public Vector4 OpColorRange
        {
            get => mEffectorParameter.OpColorRange;
            set => mEffectorParameter.OpColorRange = value;
        }
        public override unsafe void DoEffect(TtEmitter emitter, float elapsed, void* particle)
        {
            ref var cur = ref *(FParticle*)particle;
            //cur.Location += Acceleration * elapsed * (1.0f + emitter.RandomUnit() * 2.5f);
            Color4f clr = cur.Colorf;
            clr += OpColorMin + OpColorRange * emitter.RandomUnit(ref *(FParticle*)particle) * elapsed;
            clr.Red %= 1.0f;
            clr.Green %= 1.0f;
            clr.Blue %= 1.0f;
            clr.Alpha %= 1.0f;
            //Color4f. .Clamp(CoreSDK.Cla
            cur.Colorf = clr;
        }
    }

    public class TtScaleEffector : TtAuxEffector<TtScaleEffector.FScaleEffector>
    {
        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 16)]
        public struct FScaleEffector
        {
            public float OpScaleMin;
            public float OpScaleRange;
            public uint ColorEffector_Pad1;
            public uint ColorEffector_Pad2;
        };
        public override string Name
        {
            get { return "Scale"; }
        }
        public float OpScaleMin
        {
            get => mEffectorParameter.OpScaleMin;
            set => mEffectorParameter.OpScaleMin = value;
        }
        public float OpScaleRange
        {
            get => mEffectorParameter.OpScaleRange;
            set => mEffectorParameter.OpScaleRange = value;
        }
        public override unsafe void DoEffect(TtEmitter emitter, float elapsed, void* particle)
        {
            ref var cur = ref *(FParticle*)particle;
            //cur.Location += Acceleration * elapsed * (1.0f + emitter.RandomUnit() * 2.5f);
            float scale = OpScaleMin + emitter.RandomUnit(ref *(FParticle*)particle) * OpScaleRange;
            cur.Scale += scale * elapsed;
        }
    }
}
