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
    public class UGpuParticleResources
    {
        public static uint BufferHeadSize = 4;//uint 
        public RHI.CGpuBuffer ParticlesBuffer;
        public RHI.CUnorderedAccessView ParticlesUav;
        public RHI.CShaderResourceView ParticlesSrv;

        public RHI.CGpuBuffer FreeParticlesBuffer;
        public RHI.CUnorderedAccessView FreeParticlesUav;

        public RHI.CGpuBuffer TempReturnParticlesBuffer;
        public RHI.CUnorderedAccessView TempReturnParticlesUav;

        public RHI.CGpuBuffer SystemDataBuffer;
        public RHI.CUnorderedAccessView SystemDataUav;

        public RHI.CGpuBuffer CurAlivesBuffer;
        public RHI.CUnorderedAccessView CurAlivesUav;
        public RHI.CShaderResourceView CurAlivesSrv;

        public RHI.CGpuBuffer BackendAlivesBuffer;
        public RHI.CUnorderedAccessView BackendAlivesUav;
        public RHI.CShaderResourceView BackendAlivesSrv;

        public RHI.CGpuBuffer DispatchArgBuffer;
        public RHI.CUnorderedAccessView DispatchArgUav;

        public RHI.CGpuBuffer DrawArgBuffer;
        public RHI.CUnorderedAccessView DrawArgUav;
        public unsafe void Initialize<FParticle, FParticleSystem>(UEmitter<FParticle, FParticleSystem> emitter, in FParticleSystem sysData) 
            where FParticle : unmanaged
            where FParticleSystem : unmanaged
        {
            var rc = UEngine.Instance.GfxDevice.RenderContext;
            var bfDesc = new IGpuBufferDesc();
            bfDesc.SetMode(false, true);
            bfDesc.StructureByteStride = (uint)sizeof(FParticle);
            bfDesc.ByteWidth = (uint)sizeof(FParticle) * emitter.MaxParticle;
            {
                var pAddr = (FParticle*)CoreSDK.Alloc(bfDesc.ByteWidth, "Nebula", 0);                
                for (uint i = 0; i < emitter.MaxParticle; i++)
                {
                    ((FParticleBase*)&pAddr[i])->RandomSeed = (uint)emitter.RandomNext();
                }
                ParticlesBuffer = rc.CreateGpuBuffer(in bfDesc, (IntPtr)pAddr);
                CoreSDK.Free(pAddr);
            }

            var uavDesc = new IUnorderedAccessViewDesc();
            uavDesc.SetBuffer();
            uavDesc.Buffer.FirstElement = 0;
            uavDesc.Buffer.NumElements = emitter.MaxParticle;
            ParticlesUav = rc.CreateUnorderedAccessView(ParticlesBuffer, in uavDesc);

            var srvDesc = new IShaderResourceViewDesc();
            srvDesc.SetBuffer();
            srvDesc.mGpuBuffer = ParticlesBuffer.mCoreObject;
            srvDesc.Buffer.NumElements = emitter.MaxParticle;
            ParticlesSrv = rc.CreateShaderResourceView(in srvDesc);

            {
                bfDesc.StructureByteStride = (uint)sizeof(FParticleSystem);
                bfDesc.ByteWidth = (uint)sizeof(FParticleSystem) * 1;
                fixed (FParticleSystem* pAddr = &sysData)
                {
                    SystemDataBuffer = rc.CreateGpuBuffer(in bfDesc, (IntPtr)pAddr);
                }

                uavDesc.Buffer.FirstElement = 0;
                uavDesc.Buffer.NumElements = 1;
                SystemDataUav = rc.CreateUnorderedAccessView(SystemDataBuffer, in uavDesc);
            }

            {
                bfDesc.MiscFlags = (UInt32)(EResourceMiscFlag.BUFFER_ALLOW_RAW_VIEWS);
                bfDesc.StructureByteStride = (uint)sizeof(uint);
                bfDesc.ByteWidth = (uint)sizeof(uint) * (emitter.MaxParticle + BufferHeadSize);
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
                    FreeParticlesBuffer = rc.CreateGpuBuffer(in bfDesc, (IntPtr)pAddr);
                }
                TempReturnParticlesBuffer = rc.CreateGpuBuffer(in bfDesc, IntPtr.Zero);

                uavDesc.Format = EPixelFormat.PXF_R32_TYPELESS;
                uavDesc.Buffer.Flags = (UInt32)EUAVBufferFlag.UAV_FLAG_RAW;
                uavDesc.Buffer.FirstElement = 0;
                uavDesc.Buffer.NumElements = emitter.MaxParticle + BufferHeadSize;
                FreeParticlesUav = rc.CreateUnorderedAccessView(FreeParticlesBuffer, in uavDesc);
                TempReturnParticlesUav = rc.CreateUnorderedAccessView(TempReturnParticlesBuffer, in uavDesc);
            }
            
            {
                bfDesc.MiscFlags = (UInt32)(EResourceMiscFlag.BUFFER_ALLOW_RAW_VIEWS);

                bfDesc.StructureByteStride = (uint)sizeof(uint);
                bfDesc.ByteWidth = (uint)sizeof(uint) * (emitter.MaxParticle + BufferHeadSize);
                CurAlivesBuffer = rc.CreateGpuBuffer(in bfDesc, IntPtr.Zero);
                BackendAlivesBuffer = rc.CreateGpuBuffer(in bfDesc, IntPtr.Zero);

                uavDesc.Format = EPixelFormat.PXF_R32_TYPELESS;
                uavDesc.Buffer.Flags = (UInt32)EUAVBufferFlag.UAV_FLAG_RAW;
                uavDesc.Buffer.FirstElement = 0;
                uavDesc.Buffer.NumElements = (emitter.MaxParticle + BufferHeadSize);
                CurAlivesUav = rc.CreateUnorderedAccessView(CurAlivesBuffer, in uavDesc);
                BackendAlivesUav = rc.CreateUnorderedAccessView(BackendAlivesBuffer, in uavDesc);

                srvDesc.Format = EPixelFormat.PXF_R32_TYPELESS;
                srvDesc.mGpuBuffer = CurAlivesBuffer.mCoreObject;
                srvDesc.Buffer.NumElements = emitter.MaxParticle + BufferHeadSize;
                CurAlivesSrv = rc.CreateShaderResourceView(in srvDesc);
                srvDesc.mGpuBuffer = BackendAlivesBuffer.mCoreObject;
                BackendAlivesSrv = rc.CreateShaderResourceView(in srvDesc);
            }

            {
                bfDesc.MiscFlags = (UInt32)(EResourceMiscFlag.DRAWINDIRECT_ARGS | EResourceMiscFlag.BUFFER_ALLOW_RAW_VIEWS);

                bfDesc.StructureByteStride = (uint)sizeof(uint);
                bfDesc.ByteWidth = (uint)sizeof(uint) * 3;
                var pInitData = stackalloc uint[3];
                pInitData[0] = 1;
                pInitData[1] = 1;
                pInitData[2] = 1;
                DispatchArgBuffer = rc.CreateGpuBuffer(in bfDesc, (IntPtr)pInitData);
                bfDesc.ByteWidth = (uint)sizeof(uint) * 5;
                DrawArgBuffer = rc.CreateGpuBuffer(in bfDesc, IntPtr.Zero);

                uavDesc.Format = EPixelFormat.PXF_R32_TYPELESS;
                uavDesc.Buffer.Flags = (UInt32)EUAVBufferFlag.UAV_FLAG_RAW;
                uavDesc.Buffer.FirstElement = 0;
                uavDesc.Buffer.NumElements = 3;
                DispatchArgUav = rc.CreateUnorderedAccessView(DispatchArgBuffer, in uavDesc);
                uavDesc.Buffer.NumElements = 5;
                DrawArgUav = rc.CreateUnorderedAccessView(DrawArgBuffer, in uavDesc);                
            }
        }
        public void Cleanup()
        {
            
        }
        public void SwapBuffer()
        {
            CoreDefine.Swap(ref CurAlivesBuffer, ref BackendAlivesBuffer);
            CoreDefine.Swap(ref CurAlivesUav, ref BackendAlivesUav);
            CoreDefine.Swap(ref CurAlivesSrv, ref BackendAlivesSrv);
        }
    }
    public class UEffectorQueue
    {
        public string Name;
        public List<IEffector> Effectors { get; } = new List<IEffector>();
        public UNebulaShader Shader { get; set; }
        public RHI.CConstantBuffer CBuffer;
        public RHI.CComputeDrawcall mParticleUpdateDrawcall;
        public RHI.CComputeDrawcall mParticleSetupDrawcall;
        #region VarIndex
        private static RHI.FNameVarIndex ParticleRandomPoolSize = new RHI.FNameVarIndex("ParticleRandomPoolSize");
        private static RHI.FNameVarIndex Draw_IndexCountPerInstance = new RHI.FNameVarIndex("Draw_IndexCountPerInstance");
        private static RHI.FNameVarIndex Draw_StartIndexLocation = new RHI.FNameVarIndex("Draw_StartIndexLocation");
        private static RHI.FNameVarIndex Draw_BaseVertexLocation = new RHI.FNameVarIndex("Draw_BaseVertexLocation");
        private static RHI.FNameVarIndex Draw_StartInstanceLocation = new RHI.FNameVarIndex("Draw_StartInstanceLocation");
        private static RHI.FNameVarIndex ParticleMaxSize = new RHI.FNameVarIndex("ParticleMaxSize");
        private static RHI.FNameVarIndex ParticleElapsedTime = new RHI.FNameVarIndex("ParticleElapsedTime");
        private static RHI.FNameVarIndex ParticleRandomSeed = new RHI.FNameVarIndex("ParticleRandomSeed"); 
        #endregion
        public unsafe void UpdateComputeDrawcall(RHI.CRenderContext rc, IParticleEmitter emitter)
        {
            UGpuParticleResources gpuResources = emitter.GpuResources;

            if (mParticleUpdateDrawcall == null)
            {
                mParticleUpdateDrawcall = rc.CreateComputeDrawcall();

                var cbIndex = Shader.CSDesc_Particle_Update.mCoreObject.GetReflector().FindShaderBinder(EShaderBindType.SBT_CBuffer, "cbParticleDesc");
                CBuffer = rc.CreateConstantBuffer2(Shader.CSDesc_Particle_Update, cbIndex);
                CBuffer.SetValue(ref ParticleRandomPoolSize, UEngine.Instance.NebulaTemplateManager.ShaderRandomPoolSize);
                var dpDesc = emitter.Mesh.MaterialMesh.Mesh.mCoreObject.GetAtom(0, 0);
                CBuffer.SetValue(ref Draw_IndexCountPerInstance, dpDesc->m_NumPrimitives * 3);
                CBuffer.SetValue(ref Draw_StartIndexLocation, dpDesc->m_StartIndex);
                CBuffer.SetValue(ref Draw_BaseVertexLocation, dpDesc->m_BaseVertexIndex);
                CBuffer.SetValue(ref Draw_StartInstanceLocation, 0);
                CBuffer.SetValue(ref ParticleMaxSize, emitter.MaxParticle);

                mParticleUpdateDrawcall.SetComputeShader(Shader.CS_Particle_Update);
                mParticleUpdateDrawcall.BindCBuffer("cbParticleDesc", CBuffer);
                mParticleUpdateDrawcall.BindSrv("bfRandomPool", UEngine.Instance.NebulaTemplateManager.RandomPoolSrv);

                mParticleUpdateDrawcall.BindUav("bfParticles", gpuResources.ParticlesUav);
                mParticleUpdateDrawcall.BindUav("bfSystemData", gpuResources.SystemDataUav);
                mParticleUpdateDrawcall.BindUav("bfFreeParticles", gpuResources.FreeParticlesUav);
                mParticleUpdateDrawcall.BindUav("bfTempReturnParticles", gpuResources.TempReturnParticlesUav);

                mParticleUpdateDrawcall.BindUav("bfDrawArg", gpuResources.DrawArgUav);
                mParticleUpdateDrawcall.BindUav("bfDispatchArg", gpuResources.DispatchArgUav);

                mParticleUpdateDrawcall.SetDispatchIndirectBuffer(gpuResources.DispatchArgBuffer, 0);
            }

            CBuffer.SetValue(ref ParticleElapsedTime, UEngine.Instance.ElapsedSecond);
            CBuffer.SetValue(ref ParticleRandomSeed, emitter.RandomNext());

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
                mParticleSetupDrawcall = rc.CreateComputeDrawcall();
                mParticleSetupDrawcall.SetComputeShader(Shader.CS_Particle_SetupParameters);
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
        public unsafe void Update<FParticle, FParticleSystem>(UEmitter<FParticle, FParticleSystem> emiter, float elapsed) 
            where FParticle : unmanaged
            where FParticleSystem : unmanaged
        {
            var pParticles = (FParticle*)emiter.mCoreObject.GetParticleAddress();
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
    public interface IParticleEmitter
    {
        void InitEmitter(RHI.CRenderContext rc, Graphics.Mesh.UMesh mesh, uint maxParticle);
        void Cleanup();
        bool SetCurrentQueue(string name);
        void Update(UParticleGraphNode particleSystem, float elapsed);
        unsafe void Flush2GPU(ICommandList cmd);
        uint MaxParticle { get; }
        List<UShape> EmitterShapes { get; }
        Dictionary<string, UEffectorQueue> EffectorQueues { get; }
        UEffectorQueue CurrentQueue { get; set; }
        UGpuParticleResources GpuResources { get; }
        string GetParticleDefine();
        string GetSystemDataDefine();
        string GetCBufferDefines();
        string GetHLSL();

        float RandomUnit();
        float RandomSignedUnit();
        int RandomNext();
        Vector3 RandomVector(bool normalized = true);
        Graphics.Mesh.UMesh Mesh { get; set; }

        void SetCBuffer(RHI.CConstantBuffer CBuffer);
    }
    public class UEmitter<FParticle, FParticleSystem> : AuxPtrType<IEmitter>, IParticleEmitter 
        where FParticle : unmanaged
        where FParticleSystem : unmanaged
    {
        public bool IsGpuDriven { get; set; } = false;
        public FParticleSystem SystemData = default;
        public Dictionary<string, UEffectorQueue> EffectorQueues { get; } = new Dictionary<string, UEffectorQueue>();
        public UEffectorQueue CurrentQueue { get; set; }
        public Support.ULogicTimerManager Timers { get; } = new Support.ULogicTimerManager();
        public List<UShape> EmitterShapes { get; } = new List<UShape>();
        public UEmitter()
        {
            mCoreObject = IEmitter.CreateInstance();
        }
        ~UEmitter()
        {
            Cleanup();
        }
        public virtual void Cleanup()
        {
            GpuResources.Cleanup();
        }
        public uint MaxParticle { get; set; }
        public Graphics.Mesh.UMesh Mesh { get; set; }
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
            var type = typeof(FParticle);
            var flds = type.GetFields();
            foreach (var i in flds)
            {
                if (i.Name == "BaseData")
                    continue;
                result += $"{GetTypeDef(i.FieldType)} {i.Name};\n";
            }
            return result;
        }
        public string GetSystemDataDefine()
        {
            string result = "";
            var type = typeof(FParticleSystem);
            var flds = type.GetFields();
            foreach (var i in flds)
            {
                if (i.Name == "BaseData")
                    continue;
                result += $"{GetTypeDef(i.FieldType)} {i.Name};\n";
            }
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
            return "";
        }
        #endregion
        public virtual unsafe void InitEmitter(RHI.CRenderContext rc, Graphics.Mesh.UMesh mesh, uint maxParticle)
        {
            MaxParticle = maxParticle;
            mCoreObject.InitEmitter((uint)sizeof(FParticle), maxParticle);

            Mesh = mesh;
            if (rc != null)
            {
                mGpuResources = new UGpuParticleResources();
                mGpuResources.Initialize(this, SystemData);
            }
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
        public UEffectorQueue GetEffectorQueue(string name)
        {
            UEffectorQueue queue;
            if (EffectorQueues.TryGetValue(name, out queue))
            {
                return queue;
            }
            return null;
        }
        public void AddEffector(string queueName, IEffector effector)
        {
            UEffectorQueue queue;
            if(EffectorQueues.TryGetValue(queueName, out queue)==false)
            {
                queue = new UEffectorQueue();
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
                var pParticles = (FParticle*)mCoreObject.GetParticleAddress();
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
            CurrentQueue.mParticleUpdateDrawcall.BuildPass(cmdlist);
            //CurrentQueue.mParticleSetupDrawcall.mCoreObject.BuildPass(cmdlist);
        }
        public unsafe void Flush2GPU(ICommandList cmd)
        {
            if (mCoreObject.GetLiveNumber() == 0)
                return;
            if (mGpuResources.ParticlesBuffer != null)
            {
                mGpuResources.ParticlesBuffer.mCoreObject.UpdateBufferData(cmd, 16, mCoreObject.GetParticleAddress(), MaxParticle * (uint)sizeof(FParticle));
            }
            if (mGpuResources.CurAlivesBuffer != null)
            {
                mGpuResources.CurAlivesBuffer.mCoreObject.UpdateBufferData(cmd, 16, mCoreObject.GetCurrentAliveAddress(), MaxParticle * (uint)sizeof(uint));//mCoreObject.GetLiveNumber()
            }
        }
        private static RHI.FNameVarIndex SystemDataIndex = new RHI.FNameVarIndex("SystemData");
        public void SetCBuffer(RHI.CConstantBuffer CBuffer)
        {
            CBuffer.SetValue(ref SystemDataIndex, in SystemData);
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
        public unsafe virtual void OnInitParticle(FParticle* pParticleArray, ref FParticle particle)
        {

        }
        public unsafe virtual void OnDeadParticle(uint index, ref FParticle particle)
        {

        }
        protected virtual void OnQueueExecuted(UEffectorQueue queue)
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
        UGpuParticleResources mGpuResources;
        public UGpuParticleResources GpuResources 
        {
            get => mGpuResources;
        }
        #endregion
    }
}
