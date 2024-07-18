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
    }
    public class TtGpuParticleResources : IDisposable
    {
        public void Dispose()
        {
            CoreSDK.DisposeObject(ref ParticlesBuffer);
            CoreSDK.DisposeObject(ref ParticlesUav);
            CoreSDK.DisposeObject(ref ParticlesSrv);

            CoreSDK.DisposeObject(ref FreeParticlesBuffer);
            CoreSDK.DisposeObject(ref FreeParticlesUav);

            CoreSDK.DisposeObject(ref TempReturnParticlesBuffer);
            CoreSDK.DisposeObject(ref TempReturnParticlesUav);

            CoreSDK.DisposeObject(ref SystemDataBuffer);
            CoreSDK.DisposeObject(ref SystemDataUav);

            CoreSDK.DisposeObject(ref CurAlivesBuffer);
            CoreSDK.DisposeObject(ref CurAlivesUav);
            CoreSDK.DisposeObject(ref CurAlivesSrv);

            CoreSDK.DisposeObject(ref BackendAlivesBuffer);
            CoreSDK.DisposeObject(ref BackendAlivesUav);
            CoreSDK.DisposeObject(ref BackendAlivesSrv);

            CoreSDK.DisposeObject(ref DispatchArgBuffer);
            CoreSDK.DisposeObject(ref DispatchArgUav);

            CoreSDK.DisposeObject(ref DrawArgBuffer);
            CoreSDK.DisposeObject(ref DrawArgUav);
        }
        public static uint BufferHeadSize = 4;//uint 
        public NxRHI.UBuffer ParticlesBuffer;
        public NxRHI.UUaView ParticlesUav;
        public NxRHI.USrView ParticlesSrv;

        public NxRHI.UBuffer FreeParticlesBuffer;
        public NxRHI.UUaView FreeParticlesUav;

        public NxRHI.UBuffer TempReturnParticlesBuffer;
        public NxRHI.UUaView TempReturnParticlesUav;

        public NxRHI.UBuffer SystemDataBuffer;
        public NxRHI.UUaView SystemDataUav;

        public NxRHI.UBuffer CurAlivesBuffer;
        public NxRHI.UUaView CurAlivesUav;
        public NxRHI.USrView CurAlivesSrv;

        public NxRHI.UBuffer BackendAlivesBuffer;
        public NxRHI.UUaView BackendAlivesUav;
        public NxRHI.USrView BackendAlivesSrv;

        public NxRHI.UCbView DrawIdBuffer;
        public NxRHI.UBuffer DispatchArgBuffer;
        public NxRHI.UUaView DispatchArgUav;

        public NxRHI.UBuffer DrawArgBuffer;
        public NxRHI.UUaView DrawArgUav;
        public unsafe void Initialize(TtEmitter emitter, in FParticleSystemBase sysData) 
        {
            var rc = UEngine.Instance.GfxDevice.RenderContext;
            var bfDesc = new NxRHI.FBufferDesc();
            bfDesc.SetDefault(false, NxRHI.EBufferType.BFT_SRV | NxRHI.EBufferType.BFT_UAV);
            bfDesc.Type = NxRHI.EBufferType.BFT_UAV | NxRHI.EBufferType.BFT_SRV;
            bfDesc.StructureStride = (uint)sizeof(FParticleBase);
            bfDesc.Size = (uint)sizeof(FParticleBase) * emitter.MaxParticle;
            {
                var pAddr = (FParticleBase*)CoreSDK.Alloc(bfDesc.Size, "Nebula", 0);                
                for (uint i = 0; i < emitter.MaxParticle; i++)
                {
                    ((FParticleBase*)&pAddr[i])->RandomSeed = (uint)emitter.RandomNext();
                }
                bfDesc.InitData = pAddr;
                ParticlesBuffer = rc.CreateBuffer(in bfDesc);
                CoreSDK.Free(pAddr);
            }

            var uavDesc = new NxRHI.FUavDesc();
            uavDesc.SetBuffer(false);
            uavDesc.Buffer.FirstElement = 0;
            uavDesc.Buffer.NumElements = emitter.MaxParticle;
            uavDesc.Buffer.StructureByteStride = bfDesc.StructureStride;
            ParticlesUav = rc.CreateUAV(ParticlesBuffer, in uavDesc);

            var srvDesc = new NxRHI.FSrvDesc();
            srvDesc.SetBuffer(false);
            srvDesc.Buffer.NumElements = emitter.MaxParticle;
            srvDesc.Buffer.StructureByteStride = bfDesc.StructureStride;
            ParticlesSrv = rc.CreateSRV(ParticlesBuffer, in srvDesc);

            {
                bfDesc.StructureStride = (uint)sizeof(FParticleSystemBase);
                bfDesc.Size = (uint)sizeof(FParticleSystemBase) * 1;
                fixed (FParticleSystemBase* pAddr = &sysData)
                {
                    bfDesc.InitData = pAddr;
                    SystemDataBuffer = rc.CreateBuffer(in bfDesc);
                }

                uavDesc.Buffer.FirstElement = 0;
                uavDesc.Buffer.NumElements = 1;
                uavDesc.Buffer.StructureByteStride = bfDesc.StructureStride;
                SystemDataUav = rc.CreateUAV(SystemDataBuffer, in uavDesc);
            }

            bfDesc.Type |= NxRHI.EBufferType.BFT_RAW;
            {   
                bfDesc.MiscFlags = NxRHI.EResourceMiscFlag.RM_BUFFER_ALLOW_RAW_VIEWS;
                bfDesc.StructureStride = (uint)sizeof(uint);
                bfDesc.Size = (uint)sizeof(uint) * (emitter.MaxParticle + BufferHeadSize);
                var initData = new uint[emitter.MaxParticle + BufferHeadSize];
                initData[0] = emitter.MaxParticle;
                initData[1] = 0;
                initData[2] = 0;
                initData[3] = 0;
                for (uint i = 0; i < emitter.MaxParticle; i++)
                {
                    initData[BufferHeadSize + i] = emitter.MaxParticle - 1 - i;
                }
                fixed (uint* pAddr = &initData[0])
                {
                    bfDesc.InitData = pAddr;
                    FreeParticlesBuffer = rc.CreateBuffer(in bfDesc);
                }
                bfDesc.InitData = IntPtr.Zero.ToPointer();
                TempReturnParticlesBuffer = rc.CreateBuffer(in bfDesc);

                uavDesc.SetBuffer(true); 
                uavDesc.Format = EPixelFormat.PXF_R32_TYPELESS;
                uavDesc.Buffer.FirstElement = 0;
                uavDesc.Buffer.NumElements = emitter.MaxParticle + BufferHeadSize;
                uavDesc.Buffer.StructureByteStride = bfDesc.StructureStride;
                FreeParticlesUav = rc.CreateUAV(FreeParticlesBuffer, in uavDesc);
                TempReturnParticlesUav = rc.CreateUAV(TempReturnParticlesBuffer, in uavDesc);
            }
            
            {
                bfDesc.MiscFlags = (NxRHI.EResourceMiscFlag.RM_BUFFER_ALLOW_RAW_VIEWS);

                bfDesc.StructureStride = (uint)sizeof(uint);
                bfDesc.Size = (uint)sizeof(uint) * (emitter.MaxParticle + BufferHeadSize);
                bfDesc.InitData = IntPtr.Zero.ToPointer();
                CurAlivesBuffer = rc.CreateBuffer(in bfDesc);
                BackendAlivesBuffer = rc.CreateBuffer(in bfDesc);

                uavDesc.SetBuffer(true);
                uavDesc.Format = EPixelFormat.PXF_R32_TYPELESS;
                //uavDesc.Buffer.Flags = (UInt32)EUAVBufferFlag.UAV_FLAG_RAW;
                uavDesc.Buffer.FirstElement = 0;
                uavDesc.Buffer.NumElements = (emitter.MaxParticle + BufferHeadSize);
                uavDesc.Buffer.StructureByteStride = bfDesc.StructureStride;
                CurAlivesUav = rc.CreateUAV(CurAlivesBuffer, in uavDesc);
                BackendAlivesUav = rc.CreateUAV(BackendAlivesBuffer, in uavDesc);

                //srvDesc.Buffer.FirstElement = 0;
                //srvDesc.Buffer.ElementWidth = (uint)sizeof(float);
                //srvDesc.Buffer.NumElements = emitter.MaxParticle + BufferHeadSize;
                srvDesc.SetBuffer(true);
                srvDesc.Format = EPixelFormat.PXF_R32_TYPELESS;
                srvDesc.Buffer.NumElements = emitter.MaxParticle + BufferHeadSize;
                srvDesc.Buffer.StructureByteStride = bfDesc.StructureStride;
                CurAlivesSrv = rc.CreateSRV(CurAlivesBuffer, in srvDesc);
                BackendAlivesSrv = rc.CreateSRV(BackendAlivesBuffer, in srvDesc);
            }

            {
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

                uavDesc.SetBuffer(true);
                uavDesc.Format = EPixelFormat.PXF_R32_TYPELESS;
                uavDesc.Buffer.FirstElement = 0;
                uavDesc.Buffer.NumElements = (uint)(sizeof(NxRHI.FIndirectDispatchArgument) / sizeof(int)) * NumOfArgument;
                uavDesc.Buffer.StructureByteStride = bfDesc.StructureStride;
                DispatchArgUav = rc.CreateUAV(DispatchArgBuffer, in uavDesc);
                uavDesc.Buffer.NumElements = (uint)(sizeof(NxRHI.FIndirectDrawArgument) / sizeof(int)) * NumOfArgument;
                DrawArgUav = rc.CreateUAV(DrawArgBuffer, in uavDesc);                
            }
            bfDesc.Type &= (~NxRHI.EBufferType.BFT_RAW);
        }
        public void SwapBuffer()
        {
            MathHelper.Swap(ref CurAlivesBuffer, ref BackendAlivesBuffer);
            MathHelper.Swap(ref CurAlivesUav, ref BackendAlivesUav);
            MathHelper.Swap(ref CurAlivesSrv, ref BackendAlivesSrv);
        }
    }
    public class TtEffectorQueue : IDisposable
    {
        public void Dispose()
        {
            CoreSDK.DisposeObject(ref CBuffer);
            CoreSDK.DisposeObject(ref mParticleUpdateDrawcall);
            CoreSDK.DisposeObject(ref mParticleSetupDrawcall);
        }
        public string Name;
        public List<IEffector> Effectors { get; } = new List<IEffector>();
        public UNebulaShader Shader { get; set; }
        public NxRHI.UCbView CBuffer;
        public NxRHI.UComputeDraw mParticleUpdateDrawcall;
        public NxRHI.UComputeDraw mParticleSetupDrawcall;
        
        public unsafe void UpdateComputeDrawcall(NxRHI.UGpuDevice rc, IParticleEmitter emitter)
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

                mParticleUpdateDrawcall.SetComputeEffect(Shader.Particle_Update);

                mParticleUpdateDrawcall.BindCBuffer("cbParticleDesc", CBuffer);
                mParticleUpdateDrawcall.BindSrv("bfRandomPool", UEngine.Instance.NebulaTemplateManager.RandomPoolSrv);

                mParticleUpdateDrawcall.BindUav("bfParticles", gpuResources.ParticlesUav);
                mParticleUpdateDrawcall.BindUav("bfSystemData", gpuResources.SystemDataUav);
                mParticleUpdateDrawcall.BindUav("bfFreeParticles", gpuResources.FreeParticlesUav);
                mParticleUpdateDrawcall.BindUav("bfTempReturnParticles", gpuResources.TempReturnParticlesUav);

                mParticleUpdateDrawcall.BindUav("bfDrawArg", gpuResources.DrawArgUav);
                mParticleUpdateDrawcall.BindUav("bfDispatchArg", gpuResources.DispatchArgUav);

                mParticleUpdateDrawcall.BindIndirectDispatchArgsBuffer(gpuResources.DispatchArgBuffer);
            }

            CBuffer.SetValue(coreBinder.CBPerParticle.ParticleElapsedTime, UEngine.Instance.ElapsedSecond);
            CBuffer.SetValue(coreBinder.CBPerParticle.ParticleRandomSeed, emitter.RandomNext());

            emitter.SetCBuffer(CBuffer);

            uint index = 0;
            foreach(var i in emitter.EmitterShapes)
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

            mParticleUpdateDrawcall.BindUav("bfCurAlives", gpuResources.CurAlivesUav);            
            mParticleUpdateDrawcall.BindUav("bfBackendAlives", gpuResources.BackendAlivesUav);
            
            if (mParticleSetupDrawcall == null)
            {
                mParticleSetupDrawcall = rc.CreateComputeDraw();
                mParticleSetupDrawcall.SetComputeEffect(Shader.Particle_SetupParameters);
                mParticleSetupDrawcall.BindCBuffer("cbParticleDesc", CBuffer);                
            }
            mParticleSetupDrawcall.BindUav("bfCurAlives", gpuResources.CurAlivesUav);
            mParticleSetupDrawcall.BindUav("bfDrawArg", gpuResources.DrawArgUav);
            mParticleSetupDrawcall.BindUav("bfDispatchArg", gpuResources.DispatchArgUav);
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
            codeBuilder.AddLine("\nvoid DoParticleEffectors(uint3 id, inout FParticleBase particle)", ref sourceCode);
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
    public interface IParticleEmitter : IDisposable
    {
        IParticleEmitter CloneEmitter();
        void InitEmitter(NxRHI.UGpuDevice rc, Graphics.Mesh.TtMesh mesh, uint maxParticle);
        //void Cleanup();
        bool SetCurrentQueue(string name);
        void Update(UParticleGraphNode particleSystem, float elapsed);
        unsafe void Flush2GPU(NxRHI.ICommandList cmd);
        uint MaxParticle { get; }
        List<TtShape> EmitterShapes { get; }
        Dictionary<string, TtEffectorQueue> EffectorQueues { get; }
        TtEffectorQueue CurrentQueue { get; set; }
        TtGpuParticleResources GpuResources { get; }
        string GetParticleDefine();
        string GetSystemDataDefine();
        string GetCBufferDefines();
        string GetHLSL();

        float RandomUnit();
        float RandomSignedUnit();
        int RandomNext();
        Vector3 RandomVector(bool normalized = true);
        Graphics.Mesh.TtMesh Mesh { get; set; }

        void SetCBuffer(NxRHI.UCbView CBuffer);
    }
    public partial class TtEmitter : AuxPtrType<IEmitter>, IParticleEmitter 
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

        public virtual IParticleEmitter CloneEmitter()
        {
            var emt = Rtti.UTypeDescManager.CreateInstance(this.GetType()) as TtEmitter;
            emt.IsGpuDriven = IsGpuDriven;
            var mesh = new Graphics.Mesh.TtMesh();
            mesh.Initialize(Mesh.MaterialMesh, Rtti.UTypeDescGetter<UParticleMdfQueue>.TypeDesc); //mesh.MdfQueue
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

            var nblMdf = mesh.MdfQueue as UParticleMdfQueue;
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
        public virtual string GetHLSL()
        {
            var codeBuilder = new Bricks.CodeBuilder.Backends.UHLSLCodeGenerator();
            string sourceCode = "";
            //var codeBuilder = new Bricks.CodeBuilder.HLSL.UHLSLGen();

            var code = IO.TtFileManager.ReadAllText($"{RName.GetRName("UTest\\Particles\\USimpleEmitter\\Emitter.compute").Address}");
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
        public virtual async Thread.Async.TtTask InitEmitter(RName meshName, uint maxParticle)
        {
            NxRHI.UGpuDevice rc = UEngine.Instance.GfxDevice.RenderContext;
            var umesh = await UEngine.Instance.GfxDevice.MaterialMeshManager.GetMaterialMesh(meshName);
            var mesh = new Graphics.Mesh.TtMesh();
            mesh.Initialize(umesh, Rtti.UTypeDescGetter<UParticleMdfQueue>.TypeDesc);
            InitEmitter(rc, mesh, maxParticle);
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

            var nblMdf = mesh.MdfQueue as UParticleMdfQueue;
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
        public void AddEffector(string queueName, IEffector effector)
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
                mGpuResources.ParticlesBuffer.UpdateGpuData(cmd, 16, mCoreObject.GetParticleAddress(), MaxParticle * (uint)sizeof(FParticleBase));
            }
            if (mGpuResources.CurAlivesBuffer != null)
            {
                mGpuResources.CurAlivesBuffer.UpdateGpuData(cmd, 16, mCoreObject.GetCurrentAliveAddress(), MaxParticle * (uint)sizeof(uint));//mCoreObject.GetLiveNumber()
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
