using MathNet.Numerics.Statistics.Mcmc;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.Particle
{
    [Flags]
    public enum EParticleFlags : uint
    {
        EmitShape = (1u << 31),//Spawn by Shapes
        EmitIndex = (1u << 30),//Spawn by Particle Index
        FlagMask = 0xF0000000,
    }
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 16)]
    public struct FParticleBase
    {
        public uint Flags;
        public float Life;
        public float Scale;
        public uint RandomSeed;
        public Vector3 Location;
        public uint Pad1;
    }
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 16)]
    public struct FParticleSystemBase
    {
        public Vector3 Location;
        public uint Flags;
        public Vector4i TempData;
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
        
        public Graphics.Pipeline.TtGpuBuffer<FParticleSystemBase> SystemDataBuffer = new Graphics.Pipeline.TtGpuBuffer<FParticleSystemBase>();
        public Graphics.Pipeline.TtGpuBuffer<FParticleBase> ParticlesBuffer = new Graphics.Pipeline.TtGpuBuffer<FParticleBase>();
        public Graphics.Pipeline.TtGpuBuffer<uint> AllocatorBuffer = new Graphics.Pipeline.TtGpuBuffer<uint>();
        public Graphics.Pipeline.TtGpuBuffer<uint> CurAlivesBuffer = new Graphics.Pipeline.TtGpuBuffer<uint>();
        public Graphics.Pipeline.TtGpuBuffer<uint> BackendAlivesBuffer = new Graphics.Pipeline.TtGpuBuffer<uint>();
        
        public NxRHI.UBuffer DispatchArgBuffer;
        public NxRHI.UUaView DispatchArgUav;

        public NxRHI.UBuffer DrawArgBuffer;
        public NxRHI.UUaView DrawArgUav;
        public unsafe void Initialize(TtEmitter emitter, in FParticleSystemBase sysData) 
        {
            var rc = UEngine.Instance.GfxDevice.RenderContext;

            fixed (FParticleSystemBase* pAddr = &sysData)
            {
                SystemDataBuffer.SetSize(1, pAddr, NxRHI.EBufferType.BFT_UAV);
            }

            {
                var pAddr = (FParticleBase*)CoreSDK.Alloc(emitter.MaxParticle * (uint)sizeof(FParticleBase), "Nebula", 0);
                for (uint i = 0; i < emitter.MaxParticle; i++)
                {
                    ((FParticleBase*)&pAddr[i])->RandomSeed = (uint)emitter.RandomNext();
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
                CurAlivesBuffer.SetSize(emitter.MaxParticle, pAddr, NxRHI.EBufferType.BFT_UAV | NxRHI.EBufferType.BFT_SRV);
                BackendAlivesBuffer.SetSize(emitter.MaxParticle, pAddr, NxRHI.EBufferType.BFT_UAV | NxRHI.EBufferType.BFT_SRV);
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
                bfDesc.InitData = &pInitData;
                DispatchArgBuffer = rc.CreateBuffer(in bfDesc);
                
                bfDesc.Size = (uint)sizeof(NxRHI.FIndirectDrawArgument) * NumOfArgument;
                bfDesc.InitData = IntPtr.Zero.ToPointer();
                DrawArgBuffer = rc.CreateBuffer(in bfDesc);

                NxRHI.FUavDesc uavDesc = new NxRHI.FUavDesc();
                uavDesc.SetBuffer(true);
                uavDesc.SetBuffer(true);
                uavDesc.Format = EPixelFormat.PXF_R32_TYPELESS;
                uavDesc.Buffer.FirstElement = 0;
                uavDesc.Buffer.NumElements = (uint)(sizeof(NxRHI.FIndirectDispatchArgument) / sizeof(int)) * NumOfArgument;
                uavDesc.Buffer.StructureByteStride = bfDesc.StructureStride;
                DispatchArgUav = rc.CreateUAV(DispatchArgBuffer, in uavDesc);
                uavDesc.Buffer.NumElements = (uint)(sizeof(NxRHI.FIndirectDrawArgument) / sizeof(int)) * NumOfArgument;
                DrawArgUav = rc.CreateUAV(DrawArgBuffer, in uavDesc);                
            }
        }
        public void SwapBuffer()
        {
            MathHelper.Swap(ref CurAlivesBuffer, ref BackendAlivesBuffer);
        }
    }
    
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
            var emt = Rtti.UTypeDescManager.CreateInstance(this.GetType()) as TtEmitter;
            emt.IsGpuDriven = IsGpuDriven;
            var mesh = new Graphics.Mesh.TtMesh();
            mesh.Initialize(Mesh.MaterialMesh, Rtti.UTypeDescGetter<TtParticleMdfQueue>.TypeDesc); //mesh.MdfQueue
            emt.InitEmitter(UEngine.Instance.GfxDevice.RenderContext, mesh, 1024);

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
        public FParticleSystemBase SystemData = default;
        public Dictionary<string, TtEffectorQueue> EffectorQueues { get; } = new Dictionary<string, TtEffectorQueue>();
        public TtEffectorQueue CurrentQueue { get; set; }
        public Support.ULogicTimerManager Timers { get; } = new Support.ULogicTimerManager();
        public List<TtShape> EmitterShapes { get; } = new List<TtShape>();
        public TtEmitter()
        {
            mCoreObject = IEmitter.CreateInstance();
        }
        
        public uint MaxParticle { get; set; }
        Graphics.Mesh.TtMesh mMesh;
        public Graphics.Mesh.TtMesh Mesh { get => mMesh; set => mMesh = value; }
        #region HLSL
        string GetTypeDef(Type type)
        {
            string memberType = "";
            if (type == typeof(Vector4))
            {
                memberType = "float4";
            }
            else if (type == typeof(Vector3))
            {
                memberType = "float3";
            }
            else if (type == typeof(Vector2))
            {
                memberType = "float2";
            }
            else if (type == typeof(uint))
            {
                memberType = "uint";
            }
            else if (type == typeof(float))
            {
                memberType = "uint";
            }
            else
            {
                System.Diagnostics.Debug.Assert(false);
            }
            return memberType;
        }
        public string GetParticleDefine()
        {
            string result = "";
            return result;
        }
        public string GetSystemDataDefine()
        {
            string result = "";
            return result;
        }
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
            return RName.GetRName("Shaders/Bricks/Particle/SimpleEmitter/Emitter.compute", RName.ERNameType.Engine);
        }
        public virtual string GetHLSL()
        {
            var codeBuilder = new Bricks.CodeBuilder.Backends.UHLSLCodeGenerator();
            string sourceCode = "";
            //var codeBuilder = new Bricks.CodeBuilder.HLSL.UHLSLGen();

            var code = IO.TtFileManager.ReadAllText($"{GetEmitterShader().Address}");
            codeBuilder.AddLine(code, ref sourceCode);

            codeBuilder.AddLine("\nvoid DoParticleEmitShape(uint3 id, inout FParticle cur, uint shapeIndex)", ref sourceCode);
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
                            codeBuilder.AddLine($"{i.Name}_UpdateLocation(id, EmitShape{index}, cur);", ref sourceCode);
                        }
                        codeBuilder.PopSegment(ref sourceCode);
                        codeBuilder.AddLine($"break;", ref sourceCode);
                        index++;
                    }
                }
                codeBuilder.PopSegment(ref sourceCode);
            }
            codeBuilder.PopSegment(ref sourceCode);

            return sourceCode;
        }
        #endregion
        [Rtti.Meta]
        public virtual async Thread.Async.TtTask<bool> InitEmitter(RName meshName, uint maxParticle)
        {
            NxRHI.UGpuDevice rc = UEngine.Instance.GfxDevice.RenderContext;
            var umesh = await UEngine.Instance.GfxDevice.MaterialMeshManager.GetMaterialMesh(meshName);
            if (umesh == null)
                return false;
            var mesh = new Graphics.Mesh.TtMesh();
            mesh.Initialize(umesh, Rtti.UTypeDescGetter<TtParticleMdfQueue>.TypeDesc);
            InitEmitter(rc, mesh, maxParticle);
            return true;
        }
        public virtual unsafe void InitEmitter(NxRHI.UGpuDevice rc, Graphics.Mesh.TtMesh mesh, uint maxParticle)
        {
            MaxParticle = maxParticle;
            mCoreObject.InitEmitter((uint)sizeof(FParticleBase), maxParticle);

            Mesh = mesh;
            if (rc != null)
            {
                mGpuResources = new TtGpuParticleResources();
                mGpuResources.Initialize(this, SystemData);
            }

            var nblMdf = mesh.MdfQueue as TtParticleMdfQueue;
            nblMdf.Emitter = this;
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
        #region Update
        public unsafe void Update(UParticleGraphNode particleSystem, float elapsed)
        {
            if (IsGpuDriven)
            {
                UpdateGPU(particleSystem, elapsed);
                GpuResources.SwapBuffer();
            }
            else
            {
                UpdateCPU(elapsed);
                Flush2GPU(particleSystem.BasePass.DrawCmdList.mCoreObject);
            }
        }
        public unsafe void UpdateCPU(float elapsed)
        {
            Timers.UpdateTimer(elapsed);

            DoUpdateSystem();

            mCoreObject.UpdateLife(elapsed);

            if (CurrentQueue != null)
            {
                CurrentQueue.Update(this, elapsed);
                OnQueueExecuted(CurrentQueue);
            }

            if (mCoreObject.IsChanged())
            {
                var pParticles = (FParticleBase*)mCoreObject.GetParticleAddress();
                var bkNum = mCoreObject.GetBackendNumber();
                var pBackend = mCoreObject.GetBackendAliveAddress();
                for (uint i = 0; i < bkNum; i++)
                {
                    var index = pBackend[i];
                    var cur = (FParticleBase*)&pParticles[index];
                    var flags = cur->Flags;
                    if (HasFlags(in *cur, EParticleFlags.EmitShape) != 0)
                    {
                        uint shapeIndex = GetParticleData(flags) % (uint)EmitterShapes.Count;
                        EmitterShapes[(int)shapeIndex].UpdateLocation(this, cur);
                    }
                    OnInitParticle(pParticles, ref pParticles[index]);
                }
                mCoreObject.Recycle();
            }
        }
        public unsafe void UpdateGPU(UParticleGraphNode particleSystem, float elapsed)
        {
            CurrentQueue.UpdateComputeDrawcall(UEngine.Instance.GfxDevice.RenderContext, this);

            var cmdlist = particleSystem.BasePass.DrawCmdList;

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
                mGpuResources.ParticlesBuffer.GpuBuffer.UpdateGpuData(cmd, 16, mCoreObject.GetParticleAddress(), MaxParticle * (uint)sizeof(FParticleBase));
            }
            if (mGpuResources.CurAlivesBuffer != null)
            {
                mGpuResources.CurAlivesBuffer.GpuBuffer.UpdateGpuData(cmd, 16, mCoreObject.GetCurrentAliveAddress(), MaxParticle * (uint)sizeof(uint));//mCoreObject.GetLiveNumber()
            }
        }
        public void SetCBuffer(NxRHI.UCbView CBuffer)
        {
            CBuffer.SetValue("SystemData", in SystemData);
        }
        #endregion
        public EParticleFlags HasFlags(in FParticleBase particle, EParticleFlags flags)
        {
            return (EParticleFlags)(particle.Flags & (uint)flags);
        }
        public void SetFlags(ref FParticleBase particle, EParticleFlags flags)
        {
            particle.Flags |= (uint)flags;
        }
        public uint GetParticleData(uint flags)
        {
            return (flags & (uint)(~EParticleFlags.FlagMask));
        }
        public uint SetParticleFlags(EParticleFlags flags, uint data)
        {
            return (uint)flags | (data & ((uint)~EParticleFlags.FlagMask));
        }
        public virtual void DoUpdateSystem()
        {

        }
        public unsafe virtual void OnInitParticle(FParticleBase* pParticleArray, ref FParticleBase particle)
        {

        }
        public unsafe virtual void OnDeadParticle(uint index, ref FParticleBase particle)
        {

        }
        protected virtual void OnQueueExecuted(TtEffectorQueue queue)
        {

        }

        #region Random
        public float RandomUnit()//[0,1]
        {
            return (float)UEngine.Instance.NebulaTemplateManager.mRandom.NextDouble();
        }
        public float RandomSignedUnit()//[-1,1]
        {
            return UEngine.Instance.NebulaTemplateManager.RandomSignedUnit();
        }
        public int RandomNext()
        {
            return UEngine.Instance.NebulaTemplateManager.mRandom.Next();
        }
        public Vector3 RandomVector(bool normalized = true)
        {
            var result = new Vector3();
            result.X = RandomSignedUnit();
            result.Y = RandomSignedUnit();
            result.Z = RandomSignedUnit();
            if (normalized)
            {
                result.Normalize();
            }
            return result;
        }
        #endregion

        #region RenderResurce        
        TtGpuParticleResources mGpuResources;
        public TtGpuParticleResources GpuResources 
        {
            get => mGpuResources;
        }
        #endregion
    }
}
