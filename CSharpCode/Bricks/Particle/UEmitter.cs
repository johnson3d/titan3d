using MathNet.Numerics.Statistics.Mcmc;
using Microsoft.CodeAnalysis;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace EngineNS.Bricks.Particle
{
    [Flags]
    [EngineNS.Editor.ShaderCompiler.TtShaderDefine(ShaderName = "EParticleFlags")]
    public enum EParticleFlags : uint
    {
        EmitShape = (1u << 31),//Spawn by Shapes
        EmitIndex = (1u << 30),//Spawn by Particle Index
        FlagMask = 0xF0000000,
    }
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 16)]
    [EngineNS.Editor.ShaderCompiler.TtShaderDefine(ShaderName = "FParticle")]
    public struct FParticle
    {
        [EngineNS.Editor.ShaderCompiler.TtShaderDefine(ShaderName = "Flags")]
        public uint mFlags;//system need
        [EngineNS.Editor.ShaderCompiler.TtShaderDefine(ShaderName = "Life")]
        public float mLife;//system need
        [EngineNS.Editor.ShaderCompiler.TtShaderDefine(ShaderName = "Scale")]
        public float mScale;
        [EngineNS.Editor.ShaderCompiler.TtShaderDefine(ShaderName = "RandomSeed")]
        public uint mRandomSeed;
        [EngineNS.Editor.ShaderCompiler.TtShaderDefine(ShaderName = "Location")]
        public Vector3 mLocation;
        [EngineNS.Editor.ShaderCompiler.TtShaderDefine(ShaderName = "Color")]
        public uint mColor;
        [EngineNS.Editor.ShaderCompiler.TtShaderDefine(ShaderName = "Velocity")]
        public Vector3 mVelocity;
        [EngineNS.Editor.ShaderCompiler.TtShaderDefine(ShaderName = "Rotator")]
        public uint mRotator;
        [Rtti.Meta("",ShaderName = "Flags")]
        public uint Flags { get => mFlags; set => mFlags = value; }
        [Rtti.Meta("",ShaderName = "Life")]
        public float Life { get => mLife; set => mLife = value; }
        [Rtti.Meta("",ShaderName = "Scale")]
        public float Scale { get => mScale; set => mScale = value; }
        [Rtti.Meta("",ShaderName = "RandomSeed")]
        public uint RandomSeed { get => mRandomSeed; set => mRandomSeed = value; }
        [Rtti.Meta("",ShaderName = "Location")]
        public Vector3 Location { get => mLocation; set => mLocation = value; }
        [Rtti.Meta("",ShaderName = "Color")]
        public uint Color { get => mColor; set => mColor = value; }
        public Color4f Colorf
        {
            get
            {
                return Color4f.FromColor4b(EngineNS.Color4b.FromArgb((int)Color));
            }
            set
            {
                Color = value.ToArgb();
            }
        }
        [Rtti.Meta("",ShaderName = "Velocity")]
        public Vector3 Velocity { get => mVelocity; set => mVelocity = value; }
        [Rtti.Meta("",ShaderName = "Rotator")]
        public uint Rotator { get => mRotator; set => mRotator = value; }
        //float[0-1]
        public Vector3 Rotatorf
        {
            get
            {
                var result = new Vector3();
                result.X = ((float)((byte)(mRotator >> 16)))/ 255.0f;
                result.Y = ((float)((byte)(mRotator >> 8))) / 255.0f;
                result.Z = ((float)((byte)(mRotator))) / 255.0f;

                //result *= (float)Math.PI * 2.0f;
                return result;
            }
            set
            {
                //var t = value / (float)Math.PI * 2.0f;
                var t = value;
                var x = (UInt32)(t.X * 255.0f);
                var y = (UInt32)(t.Y * 255.0f);
                var z = (UInt32)(t.Z * 255.0f);

                mRotator = x;
                mRotator |= y << 8;
                mRotator |= z << 16;
            }
        }
    }

    [Flags]
    [EngineNS.Editor.ShaderCompiler.TtShaderDefine(ShaderName = "EParticleEmitterStyles")]
    public enum EParticleEmitterStyles : uint
    {
        FreeFaceToCameral = 1,
        YawFaceToCameral = (1 << 1),
    };
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 16)]
    [EngineNS.Editor.ShaderCompiler.TtShaderDefine(ShaderName = "FParticleEmitter")]
    public struct FParticleEmitter
    {
        [EngineNS.Editor.ShaderCompiler.TtShaderDefine(ShaderName = "Location")]
        public Vector3 mLocation;
        [EngineNS.Editor.ShaderCompiler.TtShaderDefine(ShaderName = "Flags")]
        public uint mFlags;

        [EngineNS.Editor.ShaderCompiler.TtShaderDefine(ShaderName = "Velocity")]
        public Vector3 mVelocity;
        [EngineNS.Editor.ShaderCompiler.TtShaderDefine(ShaderName = "Flags1")]
        public uint mFlags1;

        [EngineNS.Editor.ShaderCompiler.TtShaderDefine(ShaderName = "CameralEuler")]
        public FRotator mCameralEuler;
        [EngineNS.Editor.ShaderCompiler.TtShaderDefine(ShaderName = "Flags2")]
        public uint mFlags2;

        [EngineNS.Editor.ShaderCompiler.TtShaderDefine(ShaderName = "TempData")]
        public Vector4i mTempData;//for compute UAV
        [Rtti.Meta("",ShaderName = "Location")]
        public Vector3 Location { get => mLocation; set => mLocation = value; }
        [Rtti.Meta("",Flags = Rtti.MetaAttribute.EMetaFlags.CanRefForMacross, ShaderName = "Flags")]
        public uint Flags { get => mFlags; set => mFlags = value; }
        [Rtti.Meta("",ShaderName = "Velocity")]
        public Vector3 Velocity { get => mVelocity; set => mVelocity = value; }
        [Rtti.Meta("",Flags = Rtti.MetaAttribute.EMetaFlags.CanRefForMacross, ShaderName = "Flags1")]
        public uint Flags1 { get => mFlags1; set => mFlags1 = value; }
        [Rtti.Meta("",ShaderName = "CameralEuler")]
        public FRotator CameralEuler { get => mCameralEuler; set => mCameralEuler = value; }
        [Rtti.Meta("",Flags = Rtti.MetaAttribute.EMetaFlags.CanRefForMacross, ShaderName = "Flags2")]
        public uint Flags2 { get => mFlags2; set => mFlags2 = value; }
        [Rtti.Meta("",ShaderName = "TempData")]
        public Vector4i TempData { get => mTempData; set => mTempData = value; }
    }
    public class TtGpuParticleResources : IDisposable
    {
        public void Dispose()
        {
            CoreSDK.DisposeObject(ref SystemDataBuffer);
            CoreSDK.DisposeObject(ref ParticlesBuffer);
            CoreSDK.DisposeObject(ref AllocatorBuffer);
            CoreSDK.DisposeObject(ref CurAlivesBuffer);
            CoreSDK.DisposeObject(ref BackendAlivesBuffer);

            CoreSDK.DisposeObject(ref DispatchArgBuffer);
            CoreSDK.DisposeObject(ref DispatchArgUav);

            CoreSDK.DisposeObject(ref DrawArgBuffer);
            CoreSDK.DisposeObject(ref DrawArgUav);
        }
        
        public Graphics.Pipeline.TtGpuBuffer<FParticleEmitter> SystemDataBuffer = new Graphics.Pipeline.TtGpuBuffer<FParticleEmitter>();
        public Graphics.Pipeline.TtGpuBuffer<FParticle> ParticlesBuffer = new Graphics.Pipeline.TtGpuBuffer<FParticle>();
        public Graphics.Pipeline.TtGpuBuffer<uint> AllocatorBuffer = new Graphics.Pipeline.TtGpuBuffer<uint>();
        public Graphics.Pipeline.TtGpuBuffer<uint> CurAlivesBuffer = new Graphics.Pipeline.TtGpuBuffer<uint>();
        public Graphics.Pipeline.TtGpuBuffer<uint> BackendAlivesBuffer = new Graphics.Pipeline.TtGpuBuffer<uint>();
        
        public NxRHI.TtBuffer DispatchArgBuffer;
        public NxRHI.TtUaView DispatchArgUav;

        public NxRHI.TtBuffer DrawArgBuffer;
        public NxRHI.TtUaView DrawArgUav;
        public unsafe void Initialize(TtEmitter emitter, in FParticleEmitter sysData) 
        {
            var rc = TtEngine.Instance.GfxDevice.RenderContext;

            fixed (FParticleEmitter* pAddr = &sysData)
            {
                SystemDataBuffer.SetSize(1, pAddr, NxRHI.EBufferType.BFT_UAV);
            }

            {
                var pAddr = (FParticle*)CoreSDK.Alloc(emitter.MaxParticle * (uint)sizeof(FParticle), "Nebula", 0);
                for (uint i = 0; i < emitter.MaxParticle; i++)
                {
                    ((FParticle*)&pAddr[i])->RandomSeed = (uint)TtEngine.Instance.NebulaTemplateManager.mRandom.Next();
                    ((FParticle*)&pAddr[i])->Flags = i;
                }
                ParticlesBuffer.SetSize(emitter.MaxParticle, pAddr, NxRHI.EBufferType.BFT_UAV | NxRHI.EBufferType.BFT_SRV);
                CoreSDK.Free(pAddr);
            }

            {
                var pAddr = (uint*)CoreSDK.Alloc(emitter.MaxParticle * sizeof(uint) + 8, "Nebula", 0);
                pAddr[0] = emitter.MaxParticle;
                pAddr[1] = 0;
                for (uint i = 0; i < emitter.MaxParticle; i++)
                {
                    pAddr[2 + i] = i;// emitter.MaxParticle - 1 - i;
                }
                AllocatorBuffer.SetSize(emitter.MaxParticle, pAddr, NxRHI.EBufferType.BFT_UAV);
                CoreSDK.Free(pAddr);
            }

            {
                var pAddr = (uint*)CoreSDK.Alloc(emitter.MaxParticle * sizeof(uint) + 4, "Nebula", 0);
                pAddr[0] = 0;
                CurAlivesBuffer.SetSize(emitter.MaxParticle + 1, pAddr, NxRHI.EBufferType.BFT_UAV | NxRHI.EBufferType.BFT_SRV);
                BackendAlivesBuffer.SetSize(emitter.MaxParticle + 1, pAddr, NxRHI.EBufferType.BFT_UAV | NxRHI.EBufferType.BFT_SRV);
                CoreSDK.Free(pAddr);
            }
            {
                NxRHI.FBufferDesc bfDesc = new NxRHI.FBufferDesc();
                bfDesc.SetDefault(true, NxRHI.EBufferType.BFT_UAV | NxRHI.EBufferType.BFT_SRV);
                bfDesc.Type |= NxRHI.EBufferType.BFT_IndirectArgs;
                bfDesc.MiscFlags = (NxRHI.EResourceMiscFlag.RM_DRAWINDIRECT_ARGS | NxRHI.EResourceMiscFlag.RM_BUFFER_ALLOW_RAW_VIEWS);

                //Urgly DX12: For pass the member DrawID, Argument buffer must have some padding data to read whole stride
                //1.SV_DrawId doesn't provide by DX12
                //2.D3D12_INDIRECT_ARGUMENT_TYPE_DRAW_INDEXED must be put in latest desc;
                //3.ExecuteIndirect will crash, if argument buffer is not big enough
                //So, We create n+1 FIndirectDispatchArgument/FIndirectDrawArgument for the buffer
                const int NumOfArgument = 2;
                bfDesc.StructureStride = (uint)sizeof(uint);
                bfDesc.Size = (uint)sizeof(NxRHI.FIndirectDispatchArgument) * NumOfArgument;
                NxRHI.FIndirectDispatchArgument pInitData = new NxRHI.FIndirectDispatchArgument();
                pInitData.X = 1;
                pInitData.Y = 1;
                pInitData.Z = 1;

                var initData = new NxRHI.FMappedSubResource();
                initData.pData = &pInitData;
                initData.RowPitch = (uint)sizeof(NxRHI.FIndirectDispatchArgument);

                bfDesc.InitData = &initData;
                DispatchArgBuffer = rc.CreateBuffer(in bfDesc);
                
                bfDesc.Size = (uint)sizeof(NxRHI.FIndirectDrawIndexArgument) * NumOfArgument;
                bfDesc.InitData = (NxRHI.FMappedSubResource*)IntPtr.Zero.ToPointer();
                DrawArgBuffer = rc.CreateBuffer(in bfDesc);

                NxRHI.FUavDesc uavDesc = new NxRHI.FUavDesc();
                uavDesc.SetBuffer(true);
                uavDesc.SetBuffer(true);
                uavDesc.Format = EPixelFormat.PXF_R32_TYPELESS;
                uavDesc.Buffer.FirstElement = 0;
                uavDesc.Buffer.NumElements = (uint)(sizeof(NxRHI.FIndirectDispatchArgument) / sizeof(int)) * NumOfArgument;
                uavDesc.Buffer.StructureByteStride = bfDesc.StructureStride;
                DispatchArgUav = rc.CreateUAV(DispatchArgBuffer, in uavDesc);
                uavDesc.Buffer.NumElements = (uint)(sizeof(NxRHI.FIndirectDrawIndexArgument) / sizeof(int)) * NumOfArgument;
                DrawArgUav = rc.CreateUAV(DrawArgBuffer, in uavDesc);                
            }
        }
        public void SwapBuffer()
        {
            MathHelper.Swap(ref CurAlivesBuffer, ref BackendAlivesBuffer);
        }
    }

    [Rtti.Meta("",ShaderName = "TtEmitter")]
    public partial class TtEmitter : AuxPtrType<IEmitter> 
    {
        public override void Dispose()
        {
            //CoreSDK.DisposeObject(ref mMesh);
            //GpuResources.Dispose();

            //foreach (var i in EffectorQueues)
            //{
            //    i.Value.Dispose();
            //}
            //EffectorQueues.Clear();
            //CurrentQueue = null;

            base.Dispose();
        }

        public virtual TtEmitter CloneEmitter()
        {
            var emt = Rtti.TtTypeDescManager.CreateInstance(this.GetType()) as TtEmitter;
            emt.IsGpuDriven = IsGpuDriven;
            var mesh = new Graphics.Mesh.TtRenderMesh();
            mesh.Initialize(Mesh.MaterialMesh, Rtti.TtTypeDescGetter<TtParticleMdfQueue>.TypeDesc); //mesh.MdfQueue
            emt.InitEmitter(TtEngine.Instance.GfxDevice.RenderContext, mesh, 1024);

            foreach (var i in EmitterShapes)
            {
                var shp = i.CloneShape();
                emt.EmitterShapes.Add(shp);
            }

            foreach (var i in EffectorQueues)
            {
                foreach (var j in i.Value.Effectors)
                {
                    emt.AddEffector(i.Key, j.CloneEffector());
                }
            }

            var nblMdf = mesh.MdfQueue as TtParticleMdfQueue;
            nblMdf.Emitter = emt;

            return emt;
        }
        public bool IsGpuDriven { get; set; } = false;
        public FParticleEmitter EmitterData = default;
        public float TimerRemain { get; set; } = float.MaxValue;
        [Rtti.Meta("")]
        public float TimerInterval { get; set; } = float.MaxValue;
        [Rtti.Meta("",ShaderName = "EmitterDataRef[0]", Flags = Rtti.MetaAttribute.EMetaFlags.NoSerializable)]
        public ref FParticleEmitter EmitterDataRef
        {
            get
            {
                return ref EmitterData;
            }
        }
        public Dictionary<string, TtEffectorQueue> EffectorQueues { get; } = new Dictionary<string, TtEffectorQueue>();
        public TtEffectorQueue CurrentQueue { get; set; }
        public Support.TtLogicTimerManager Timers { get; } = new Support.TtLogicTimerManager();
        public List<TtShape> EmitterShapes { get; } = new List<TtShape>();
        public TtEmitter()
        {
            mCoreObject = IEmitter.CreateInstance();
        }
        
        public uint MaxParticle { get; set; }
        Graphics.Mesh.TtRenderMesh mMesh;
        public Graphics.Mesh.TtRenderMesh Mesh { get => mMesh; set => mMesh = value; }
        #region HLSL
        public virtual string GetCBufferDefines()
        {
            string result = "";
            int index = 0;
            foreach (var i in EmitterShapes)
            {
                result += $"{i.Name} EmitShape{index};\n";
                index++;
            }
            return result;
        }
        public virtual RName GetEmitterShader()
        {
            if (ShaderName != null)
            {
                return ShaderName;
            }
            return RName.GetRName(mMcObject.Name.Name + $"/{mMcObject.Name.PureName}.shader", mMcObject.Name.RNameType);
        }
        public virtual string GetEmitShapeHLSL()
        {
            var codeBuilder = new Bricks.CodeBuilder.UHLSLCodeGenerator();
            string sourceCode = "";
            //var codeBuilder = new Bricks.CodeBuilder.HLSL.UHLSLGen();

            var code = IO.TtFileManager.ReadAllText($"{GetEmitterShader().Address}");
            codeBuilder.AddLine(code, ref sourceCode);

            codeBuilder.AddLine("\nvoid DoParticleEmitShape(TtEmitter emt, inout FParticle cur, uint shapeIndex)", ref sourceCode);
            codeBuilder.PushSegment(ref sourceCode);
            {
                int index = 0;
                codeBuilder.AddLine("switch(shapeIndex)", ref sourceCode);
                codeBuilder.PushSegment(ref sourceCode);
                {
                    foreach (var i in EmitterShapes)
                    {
                        codeBuilder.AddLine($"case {index}:", ref sourceCode);
                        codeBuilder.PushSegment(ref sourceCode);
                        {
                            codeBuilder.AddLine($"{i.Name}_UpdateLocation(emt, EmitShape{index}, cur);", ref sourceCode);
                        }
                        codeBuilder.PopSegment(ref sourceCode);
                        codeBuilder.AddLine($"break;", ref sourceCode);
                        index++;
                    }
                }
                codeBuilder.PopSegment(ref sourceCode);
            }
            codeBuilder.PopSegment(ref sourceCode);

            codeBuilder.AddLine("#define USER_EMITSHAPE", ref sourceCode);

            return sourceCode;
        }
        #endregion

        #region Queue&Emitter
        [Rtti.Meta("")]
        public virtual async Thread.Async.TtTask<bool> InitEmitter(RName meshName, uint maxParticle)
        {
            NxRHI.TtGpuDevice rc = TtEngine.Instance.GfxDevice.RenderContext;
            var umesh = await meshName.GetAsset<Graphics.Mesh.TtMaterialMesh>();
            if (umesh == null)
                return false;
            var mesh = new Graphics.Mesh.TtRenderMesh();
            mesh.Initialize(umesh, Rtti.TtTypeDescGetter<TtParticleMdfQueue>.TypeDesc);
            InitEmitter(rc, mesh, maxParticle);
            return true;
        }
        public virtual unsafe void InitEmitter(NxRHI.TtGpuDevice rc, Graphics.Mesh.TtRenderMesh mesh, uint maxParticle)
        {
            MaxParticle = maxParticle;
            mCoreObject.InitEmitter((uint)sizeof(FParticle), maxParticle);

            EmitterData.Velocity = Vector3.Zero;
            Mesh = mesh;
            if (rc != null)
            {
                mGpuResources = new TtGpuParticleResources();
                mGpuResources.Initialize(this, EmitterData);
            }

            var nblMdf = mesh.MdfQueue as TtParticleMdfQueue;
            nblMdf.Emitter = this;

            mParticleStartSecond = TtEngine.Instance.TickCountSecond;
            TimerRemain = TimerInterval;
        }
        public bool SetCurrentQueue(string name)
        {
            var queue = GetEffectorQueue(name);
            if (queue != null)
            {
                CurrentQueue = queue;
                return true;
            }
            return false;
        }
        public TtEffectorQueue GetEffectorQueue(string name)
        {
            TtEffectorQueue queue;
            if (EffectorQueues.TryGetValue(name, out queue))
            {
                return queue;
            }
            return null;
        }
        public void AddEffector(string queueName, TtEffector effector)
        {
            TtEffectorQueue queue;
            if(EffectorQueues.TryGetValue(queueName, out queue)==false)
            {
                queue = new TtEffectorQueue();
                queue.Name = queueName;
                EffectorQueues.Add(queueName, queue);
            }
            queue.Effectors.Add(effector);

            if (CurrentQueue == null)
                CurrentQueue = queue;
        }
        #endregion

        #region Update
        private float mParticleStartSecond;
        public unsafe void Update(TtNebulaParticle nebula, Graphics.Pipeline.TtRenderPolicy policy, UParticleGraphNode particleSystem, float elapsed, Vector3 Location)
        {
            if (Mesh == null)
                return;
            EmitterData.Location = Location;
            var quat = Quaternion.RotationMatrix(policy.DefaultCamera.GetViewMatrix());
            EmitterData.CameralEuler = quat.ToEuler();
            var coreBinder = Graphics.Pipeline.TtCoreShaderBinder.TtPerParticleCBufferVarIndexer.Instance;
            var timeSecond = TtEngine.Instance.TickCountSecond - mParticleStartSecond;
            CurrentQueue?.CBuffer?.SetValue(coreBinder.ParticleStartSecond, timeSecond);
            TimerRemain -= elapsed;
            if (TimerRemain < 0)
            {   
                CurrentQueue?.CBuffer?.SetValue(coreBinder.OnTimerState, 1);
                TimerRemain = TimerInterval;
                OnTimer(timeSecond);
            }
            else
            {
                CurrentQueue?.CBuffer?.SetValue(coreBinder.OnTimerState, 0);
            }
            if (IsGpuDriven)
            {
                UpdateGPU(particleSystem, elapsed);
                GpuResources.SwapBuffer();
            }
            else
            {
                UpdateCPU(nebula, particleSystem, elapsed);
                Flush2GPU(particleSystem.mCmdList.mCoreObject);
            }
        }
        public unsafe void UpdateCPU(TtNebulaParticle nebula, UParticleGraphNode particleSystem, float elapsed)
        {
            Timers.UpdateTimer(elapsed);

            DoUpdateSystem();

            mCoreObject.UpdateLife(elapsed);

            if (CurrentQueue != null)
            {
                CurrentQueue.Update(nebula, particleSystem, this, elapsed);
                OnQueueExecuted(CurrentQueue);
            }

            if (mCoreObject.IsChanged())
            {
                var pParticles = (FParticle*)mCoreObject.GetParticleAddress();
                var bkNum = mCoreObject.GetBackendNumber();
                var pBackend = mCoreObject.GetBackendAliveAddress();
                for (uint i = 0; i < bkNum; i++)
                {
                    var index = pBackend[i];
                    var cur = (FParticle*)&pParticles[index];
                    var flags = cur->Flags;
                    if (HasFlags(in *cur, EParticleFlags.EmitShape) != 0)
                    {
                        uint shapeIndex = GetParticleData(flags) % (uint)EmitterShapes.Count;
                        EmitterShapes[(int)shapeIndex].UpdateLocation(this, cur);

                        cur->Location = Location + cur->Location;
                    }
                    OnInitParticle(ref pParticles[index]);
                }
                mCoreObject.Recycle();
            }
        }
        public unsafe void UpdateGPU(UParticleGraphNode particleSystem, float elapsed)
        {
            if (CurrentQueue ==null || CurrentQueue.Shader == null || CurrentQueue.Shader.Particle_Update == null)
                return;
            CurrentQueue.UpdateComputeDrawcall(TtEngine.Instance.GfxDevice.RenderContext, this);

            var cmdlist = particleSystem.mCmdList;

            cmdlist.PushGpuDraw(CurrentQueue.mParticleUpdateDrawcall);
            //CurrentQueue.mParticleUpdateDrawcall.Commit(cmdlist);
            //CurrentQueue.mParticleSetupDrawcall.mCoreObject.BuildPass(cmdlist);
        }
        public unsafe void Flush2GPU(NxRHI.ICommandList cmd)
        {
            if (mCoreObject.GetLiveNumber() == 0)
                return;
            if (mGpuResources.ParticlesBuffer != null)
            {
                mGpuResources.ParticlesBuffer.GpuBuffer.UpdateGpuData(cmd, 0, mCoreObject.GetParticleAddress(), MaxParticle * (uint)sizeof(FParticle));
            }
            if (mGpuResources.CurAlivesBuffer != null)
            {
                mGpuResources.CurAlivesBuffer.GpuBuffer.UpdateGpuData(cmd, 4, mCoreObject.GetCurrentAliveAddress(), mCoreObject.GetLiveNumber() * (uint)sizeof(uint));//mCoreObject.GetLiveNumber()
            }
        }
        public void SetCBuffer(NxRHI.TtCbView CBuffer)
        {
            CBuffer.SetValue("EmitterData", in EmitterData);
        }
        #endregion

        #region Macross API
        [Category("Option")]
        [Rtti.Meta("",ShaderName = "Location")]
        public Vector3 Location
        {
            get => EmitterData.Location;
            set => EmitterData.Location = value;
        }
        [Category("Option")]
        [Rtti.Meta("",ShaderName = "Velocity")]
        public Vector3 Velocity
        {
            get => EmitterData.Velocity;
            set
            {
                EmitterData.Velocity = value;
            }
        }
        [Rtti.Meta("",ShaderName = "Color2Uint")]
        public uint Color2Uint(Color4f color)
        {
            return color.ToArgb();
        }
        [Rtti.Meta("",ShaderName = "Uint2Color4f")]
        public Color4f Uint2Color4f(uint value)
        {
            return Color4f.FromColor4b(Color4b.FromArgb((int)value));
        }
        [Rtti.Meta("",ShaderName = "HasFlags")]
        public EParticleFlags HasFlags(in FParticle particle, EParticleFlags flags)
        {
            return (EParticleFlags)(particle.Flags & (uint)flags);
        }
        [Rtti.Meta("",ShaderName = "GetParticleData")]
        public uint GetParticleData(uint flags)
        {
            return (flags & (uint)(~EParticleFlags.FlagMask));
        }
        [Rtti.Meta("",ShaderName = "SetParticleFlags")]
        public uint SetParticleFlags(EParticleFlags flags, uint data)
        {
            return (uint)flags | (data & ((uint)~EParticleFlags.FlagMask));
        }
        [Rtti.Meta("",ShaderName = "Spawn")]
        public uint Spawn(uint num, uint flags, float life)
        {
            return mCoreObject.Spawn(num, flags, life);
        }
        [Rtti.Meta("",ShaderName = "GetParticle")]
        public unsafe FParticle GetParticle(uint index)
        {
            var pParticles = (FParticle*)mCoreObject.GetParticleAddress();
            return pParticles[index];
        }
        private static uint rand_lcg(uint rng_state)
        {
            // LCG values from Numerical Recipes
            rng_state = 1664525 * rng_state + 1013904223;
            return rng_state;
        }
        #region Random
        [Rtti.Meta("",ShaderName = "RandomUnit")]
        public float RandomUnit(ref FParticle cur)//[0,1]
        {
            //cur.RandomSeed = rand_lcg(cur.RandomSeed);
            return (float)TtEngine.Instance.NebulaTemplateManager.mRandom.NextDouble();
        }
        [Rtti.Meta("",ShaderName = "RandomSignedUnit")]
        public float RandomSignedUnit(ref FParticle cur)//[-1,1]
        {
            return TtEngine.Instance.NebulaTemplateManager.RandomSignedUnit();
        }
        [Rtti.Meta("",ShaderName = "RandomNext")]
        public int RandomNext(ref FParticle cur)
        {
            return TtEngine.Instance.NebulaTemplateManager.mRandom.Next();
        }
        [Rtti.Meta("",ShaderName = "RandomVector3")]
        public Vector3 RandomVector3(ref FParticle cur, bool normalized = true)
        {
            var result = new Vector3();
            result.X = RandomSignedUnit(ref cur);
            result.Y = RandomSignedUnit(ref cur);
            result.Z = RandomSignedUnit(ref cur);
            if (normalized)
            {
                result.Normalize();
            }
            return result;
        }
        [Rtti.Meta("",ShaderName = "RandomVector3")]
        public Vector4 RandomVector4(ref FParticle cur)
        {
            var result = new Vector4();
            result.X = RandomSignedUnit(ref cur);
            result.Y = RandomSignedUnit(ref cur);
            result.Z = RandomSignedUnit(ref cur);
            result.W = RandomSignedUnit(ref cur);
            return result;
        }
        #endregion

        #endregion

        #region Callback
        public virtual void DoUpdateSystem()
        {
            mMcObject?.Get()?.DoUpdateSystem(this);
        }
        public unsafe virtual void OnInitParticle(ref FParticle particle)
        {
            mMcObject?.Get()?.OnInitParticle(this, ref particle);
        }
        public unsafe virtual void OnDeadParticle(uint index, ref FParticle particle)
        {
            mMcObject?.Get()?.OnDeadParticle(this, index, ref particle);
        }
        public unsafe virtual void OnTimer(float second)
        {
            mMcObject?.Get()?.OnTimer(this, second);
        }
        public virtual void OnParticleTick(TtEmitter emitter, float elapsed, ref FParticle particle)
        {
            mMcObject?.Get()?.OnParticleTick(emitter, elapsed, ref particle);
        }
        protected virtual void OnQueueExecuted(TtEffectorQueue queue)
        {

        }
        [Category("Option")]
        [Rtti.Meta("")]
        [RName.PGRName(FilterExts = CodeBuilder.TtMacross.AssetExt, MacrossType = typeof(TtEmitterMacross))]
        public RName McName
        {
            get
            {
                if (mMcObject == null)
                    return null;
                return mMcObject.Name;
            }
            set
            {
                if (value == null)
                {
                    mMcObject = null;
                    return;
                }
                if (mMcObject == null)
                {
                    mMcObject = Macross.TtMacrossGetter<TtEmitterMacross>.NewInstance();
                }
                mMcObject.Name = value;
            }
        }
        Macross.TtMacrossGetter<TtEmitterMacross> mMcObject;
        public Macross.TtMacrossGetter<TtEmitterMacross> McObject
        {
            get => mMcObject;
        }
        [Category("Option")]
        [Rtti.Meta("")]
        [RName.PGRName(FilterExts = Graphics.Pipeline.Shader.TtShaderAsset.AssetExt, ShaderType = "NebulaEmitter")]
        public RName ShaderName
        {
            get;
            set;
        } = null;
        #endregion

        #region RenderResurce        
        TtGpuParticleResources mGpuResources;
        public TtGpuParticleResources GpuResources 
        {
            get => mGpuResources;
        }
        #endregion
    }

    [Macross.TtMacross(IsGenShader = true)]
    public partial class TtEmitterMacross
    {
        [Rtti.Meta("")]
        public string HLSLDoUpdateSystem { get; set; } = "";
        [Rtti.Meta("")]
        public string HLSLOnInitParticle { get; set; } = "";
        [Rtti.Meta("")]
        public string HLSLOnDeadParticle { get; set; } = "";
        [Rtti.Meta("",ShaderName = "DoUpdateSystem")]
        public virtual void DoUpdateSystem(TtEmitter emt)
        {

        }
        [Rtti.Meta("",ShaderName = "OnInitParticle")]
        public unsafe virtual void OnInitParticle(TtEmitter emt, ref FParticle particle)
        {

        }
        [Rtti.Meta("",ShaderName = "OnDeadParticle")]
        public unsafe virtual void OnDeadParticle(TtEmitter emt, uint index, ref FParticle particle)
        {

        }
        [Rtti.Meta("",ShaderName = "OnParticleTick")]
        public virtual unsafe void OnParticleTick(TtEmitter emt, float elapsed, ref FParticle particle)
        {

        }
        [Rtti.Meta("",ShaderName = "OnTimer")]
        public virtual unsafe void OnTimer(TtEmitter emt, float second)
        {

        }
    }
}


#if TitanEngine_AutoGen_Macross
#region TitanEngine_AutoGen_Macross


namespace EngineNS.Bricks.Particle
{
	partial class TtEmitter
	{
		private static EngineNS.Macross.TtMacrossBreak macross_break_InitEmitter_497310126 = new EngineNS.Macross.TtMacrossBreak("EngineNS.Bricks.Particle.TtEmitter->Thread.Async.TtTask<bool> InitEmitter(RName meshName, uint maxParticle)");
		public async Thread.Async.TtTask<bool> macross_InitEmitter (string nodeName, RName meshName, uint maxParticle) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":meshName", meshName);
					stackframe.SetWatchVariable(nodeName + ":maxParticle", maxParticle);
				}
			}
			var _return_value = await InitEmitter(meshName, maxParticle);
			macross_break_InitEmitter_497310126.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_Color2Uint_863315143 = new EngineNS.Macross.TtMacrossBreak("EngineNS.Bricks.Particle.TtEmitter->uint Color2Uint(Color4f color)");
		public unsafe uint macross_Color2Uint (string nodeName, Color4f color) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":color", color);
				}
			}
			var _return_value = Color2Uint(color);
			macross_break_Color2Uint_863315143.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_Uint2Color4f_333382478 = new EngineNS.Macross.TtMacrossBreak("EngineNS.Bricks.Particle.TtEmitter->Color4f Uint2Color4f(uint value)");
		public unsafe Color4f macross_Uint2Color4f (string nodeName, uint value) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":value", value);
				}
			}
			var _return_value = Uint2Color4f(value);
			macross_break_Uint2Color4f_333382478.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_HasFlags_921555369 = new EngineNS.Macross.TtMacrossBreak("EngineNS.Bricks.Particle.TtEmitter->EParticleFlags HasFlags(in FParticle particle, EParticleFlags flags)");
		public unsafe EParticleFlags macross_HasFlags (string nodeName, in FParticle particle, EParticleFlags flags) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":particle", particle);
					stackframe.SetWatchVariable(nodeName + ":flags", flags);
				}
			}
			var _return_value = HasFlags(in particle, flags);
			macross_break_HasFlags_921555369.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_GetParticleData_4075752848 = new EngineNS.Macross.TtMacrossBreak("EngineNS.Bricks.Particle.TtEmitter->uint GetParticleData(uint flags)");
		public unsafe uint macross_GetParticleData (string nodeName, uint flags) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":flags", flags);
				}
			}
			var _return_value = GetParticleData(flags);
			macross_break_GetParticleData_4075752848.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_SetParticleFlags_503397206 = new EngineNS.Macross.TtMacrossBreak("EngineNS.Bricks.Particle.TtEmitter->uint SetParticleFlags(EParticleFlags flags, uint data)");
		public unsafe uint macross_SetParticleFlags (string nodeName, EParticleFlags flags, uint data) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":flags", flags);
					stackframe.SetWatchVariable(nodeName + ":data", data);
				}
			}
			var _return_value = SetParticleFlags(flags, data);
			macross_break_SetParticleFlags_503397206.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_Spawn_4108661384 = new EngineNS.Macross.TtMacrossBreak("EngineNS.Bricks.Particle.TtEmitter->uint Spawn(uint num, uint flags, float life)");
		public unsafe uint macross_Spawn (string nodeName, uint num, uint flags, float life) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":num", num);
					stackframe.SetWatchVariable(nodeName + ":flags", flags);
					stackframe.SetWatchVariable(nodeName + ":life", life);
				}
			}
			var _return_value = Spawn(num, flags, life);
			macross_break_Spawn_4108661384.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_GetParticle_996300053 = new EngineNS.Macross.TtMacrossBreak("EngineNS.Bricks.Particle.TtEmitter->FParticle GetParticle(uint index)");
		public unsafe FParticle macross_GetParticle (string nodeName, uint index) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":index", index);
				}
			}
			var _return_value = GetParticle(index);
			macross_break_GetParticle_996300053.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_RandomUnit_2755317112 = new EngineNS.Macross.TtMacrossBreak("EngineNS.Bricks.Particle.TtEmitter->float RandomUnit(ref FParticle cur)");
		public unsafe float macross_RandomUnit (string nodeName, ref FParticle cur) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":cur", cur);
				}
			}
			var _return_value = RandomUnit(ref cur);
			macross_break_RandomUnit_2755317112.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_RandomSignedUnit_2755317112 = new EngineNS.Macross.TtMacrossBreak("EngineNS.Bricks.Particle.TtEmitter->float RandomSignedUnit(ref FParticle cur)");
		public unsafe float macross_RandomSignedUnit (string nodeName, ref FParticle cur) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":cur", cur);
				}
			}
			var _return_value = RandomSignedUnit(ref cur);
			macross_break_RandomSignedUnit_2755317112.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_RandomNext_2755317112 = new EngineNS.Macross.TtMacrossBreak("EngineNS.Bricks.Particle.TtEmitter->int RandomNext(ref FParticle cur)");
		public unsafe int macross_RandomNext (string nodeName, ref FParticle cur) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":cur", cur);
				}
			}
			var _return_value = RandomNext(ref cur);
			macross_break_RandomNext_2755317112.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_RandomVector3_3310200893 = new EngineNS.Macross.TtMacrossBreak("EngineNS.Bricks.Particle.TtEmitter->Vector3 RandomVector3(ref FParticle cur, bool normalized)");
		public unsafe Vector3 macross_RandomVector3 (string nodeName, ref FParticle cur, bool normalized) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":cur", cur);
					stackframe.SetWatchVariable(nodeName + ":normalized", normalized);
				}
			}
			var _return_value = RandomVector3(ref cur, normalized);
			macross_break_RandomVector3_3310200893.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_RandomVector4_2755317112 = new EngineNS.Macross.TtMacrossBreak("EngineNS.Bricks.Particle.TtEmitter->Vector4 RandomVector4(ref FParticle cur)");
		public unsafe Vector4 macross_RandomVector4 (string nodeName, ref FParticle cur) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":cur", cur);
				}
			}
			var _return_value = RandomVector4(ref cur);
			macross_break_RandomVector4_2755317112.TryBreak();
			return _return_value;
		}
	}
}


namespace EngineNS.Bricks.Particle
{
	partial class TtEmitterMacross
	{
		private static EngineNS.Macross.TtMacrossBreak macross_break_DoUpdateSystem_3168036643 = new EngineNS.Macross.TtMacrossBreak("EngineNS.Bricks.Particle.TtEmitterMacross->void DoUpdateSystem(TtEmitter emt)");
		public unsafe void macross_DoUpdateSystem (string nodeName, TtEmitter emt) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":emt", emt);
				}
			}
			DoUpdateSystem(emt);
			macross_break_DoUpdateSystem_3168036643.TryBreak();
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_OnInitParticle_2541510786 = new EngineNS.Macross.TtMacrossBreak("EngineNS.Bricks.Particle.TtEmitterMacross->void OnInitParticle(TtEmitter emt, ref FParticle particle)");
		public unsafe void macross_OnInitParticle (string nodeName, TtEmitter emt, ref FParticle particle) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":emt", emt);
					stackframe.SetWatchVariable(nodeName + ":particle", particle);
				}
			}
			OnInitParticle(emt, ref particle);
			macross_break_OnInitParticle_2541510786.TryBreak();
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_OnDeadParticle_1537765022 = new EngineNS.Macross.TtMacrossBreak("EngineNS.Bricks.Particle.TtEmitterMacross->void OnDeadParticle(TtEmitter emt, uint index, ref FParticle particle)");
		public unsafe void macross_OnDeadParticle (string nodeName, TtEmitter emt, uint index, ref FParticle particle) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":emt", emt);
					stackframe.SetWatchVariable(nodeName + ":index", index);
					stackframe.SetWatchVariable(nodeName + ":particle", particle);
				}
			}
			OnDeadParticle(emt, index, ref particle);
			macross_break_OnDeadParticle_1537765022.TryBreak();
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_OnParticleTick_2419209792 = new EngineNS.Macross.TtMacrossBreak("EngineNS.Bricks.Particle.TtEmitterMacross->void OnParticleTick(TtEmitter emt, float elapsed, ref FParticle particle)");
		public unsafe void macross_OnParticleTick (string nodeName, TtEmitter emt, float elapsed, ref FParticle particle) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":emt", emt);
					stackframe.SetWatchVariable(nodeName + ":elapsed", elapsed);
					stackframe.SetWatchVariable(nodeName + ":particle", particle);
				}
			}
			OnParticleTick(emt, elapsed, ref particle);
			macross_break_OnParticleTick_2419209792.TryBreak();
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_OnTimer_1215078427 = new EngineNS.Macross.TtMacrossBreak("EngineNS.Bricks.Particle.TtEmitterMacross->void OnTimer(TtEmitter emt, float second)");
		public unsafe void macross_OnTimer (string nodeName, TtEmitter emt, float second) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":emt", emt);
					stackframe.SetWatchVariable(nodeName + ":second", second);
				}
			}
			OnTimer(emt, second);
			macross_break_OnTimer_1215078427.TryBreak();
		}
	}
}
#endregion//TitanEngine_AutoGen_Macross
#endif//TitanEngine_AutoGen_Macross