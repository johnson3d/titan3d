using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.ComponentModel;

namespace EngineNS
{
    public enum EPrimitiveType
    {
        EPT_PointList = 1,
        EPT_LineList = 2,
        EPT_LineStrip = 3,
        EPT_TriangleList = 4,
        EPT_TriangleStrip = 5,
        EPT_TriangleFan = 6,
    };
    public struct CDrawPrimitiveDesc
    {
        public void SetDefault()
        {
            PrimitiveType = EPrimitiveType.EPT_TriangleList;
            BaseVertexIndex = 0;
            StartIndex = 0;
            NumPrimitives = 0;
            NumInstances = 1;
        }
        [Editor.Editor_ShowInPropertyGrid]
        [ReadOnly(true)]
        public EPrimitiveType PrimitiveType;
        [Editor.Editor_ShowInPropertyGrid]
        [ReadOnly(true)]
        public UInt32 BaseVertexIndex;
        [Editor.Editor_ShowInPropertyGrid]
        [ReadOnly(true)]
        public UInt32 StartIndex;
        [Editor.Editor_ShowInPropertyGrid]
        [ReadOnly(true)]
        public UInt32 NumPrimitives;
        [Editor.Editor_ShowInPropertyGrid]
        [ReadOnly(true)]
        public UInt32 NumInstances;
        public bool IsIndexDraw()
        {
		    return StartIndex != 0xFFFFFFFF;
	    }
    }
    
    public class CPass : AuxCoreObject<CPass.NativePointer>
    {
        public struct NativePointer : INativePointer
        {
            public IntPtr Pointer;
            public IntPtr GetPointer()
            {
                return Pointer;
            }
            public void SetPointer(IntPtr ptr)
            {
                Pointer = ptr;
            }
            public override string ToString()
            {
                return "0x" + Pointer.ToString("x");
            }
        }
        public CPass(NativePointer self)
        {
            mCoreObject = self;
        }
        public override void Cleanup()
        {
            if (mRenderPipeline != null)
            {
                mRenderPipeline.Cleanup();
                mRenderPipeline = null;
            }

            mGpuProgram = null;

            mGeometryMesh = null;
            if (mShaderResources != null)
            {
                mShaderResources.Cleanup();
                mShaderResources = null;
            }
            if (mShaderSamplerBinder != null)
            {
                mShaderSamplerBinder.Cleanup();
                mShaderSamplerBinder = null;
            }
            mViewPort = null;
            base.Cleanup();

            Core_Release(true);
        }
        public static CPass CreatePassFromPtr(NativePointer ptr)
        {
            var result = new CPass(ptr);
            result.Core_AddRef();

            var pipeLinePtr = SDK_IPass_GetPipeline(ptr);
            if (pipeLinePtr.Pointer == IntPtr.Zero)
                return null;
            result.mRenderPipeline = new CRenderPipeline(pipeLinePtr, true);

            var GpuProgram = SDK_IPass_GetGpuProgram(ptr);
            if (GpuProgram.Pointer == IntPtr.Zero)
            {
                return null;
            }
            else
            {
                CShaderProgram Temp = new CShaderProgram(GpuProgram);
                Temp.Core_AddRef();
                result.mGpuProgram = Temp;
            }
            
            var srvs = SDK_IPass_GetShaderResurces(ptr);
            result.mShaderResources = new CShaderResources(srvs);

            //var srvs = SDK_IPass_GetShaderSma(ptr);
            //result.mShaderResources = new CShaderResources(srvs);

            return result;
        }

        public void OnCookRenderData(Graphics.CGfxShadingEnv ShadingEnv, PrebuildPassIndex ppi)
        {
            //当CookSpecRenderLayerDataToPass调用后的回调，在这里保留一个可以以后做最后渲染数据的hack
            //如Effect
            
            if (this.ShadingEnv.EnvCode.Version != EnvCodeVersion)
            {
                var src = this.Effect.Desc;
                var desc = Graphics.CGfxEffectDesc.CreateDesc(src.MtlShaderPatch, src.MdfQueueShaderPatch, this.ShadingEnv.EnvCode);
                var effect = CEngine.Instance.EffectManager.GetEffect(CEngine.Instance.RenderContext, desc);
                EnvCodeVersion = this.ShadingEnv.EnvCode.Version;
                //await effect.AwaitLoad();
                effect.PreUse((success) =>
                {
                    this.Effect = effect;
                });
            }
        }

        public UInt32 UserFlags
        {
            get
            {
                return SDK_IPass_GetUserFlags(CoreObject);
            }
            set
            {
                SDK_IPass_SetUserFlags(CoreObject, value);
            }
        }

        public float LodLevel
        {
            get
            {
                return SDK_IPass_GetLod(CoreObject);
            }
            set
            {
                SDK_IPass_SetLod(CoreObject, value);
            }
        }
        Graphics.CGfxShadingEnv mShadingEnv;
        public Graphics.CGfxShadingEnv ShadingEnv
        {
            get { return mShadingEnv; }
            set 
            { 
                mShadingEnv = value;
                EnvCodeVersion = value.EnvCode.Version;
            }
        }
        internal UInt32 EnvCodeVersion;
        Graphics.CGfxEffect mEffect;
        
        public Graphics.CGfxEffect Effect
        {
            get { return mEffect; }
            set
            {
                mEffect = value;
                GpuProgram = value.ShaderProgram;
            }
        }

        public Graphics.Mesh.CGfxMtlMesh MtlMesh;
        public UInt32 MeshIndex;
        public PrebuildPassIndex PassIndex;
        private Graphics.CGfxMaterialInstance mMtlInst;
        public Graphics.CGfxMaterialInstance MtlInst
        {
            get
            {
                if (MtlMesh != null)
                    return MtlMesh.MtlInst;
                return mMtlInst;
            }
        }
        public void InitPass(CRenderContext RHICtx, Graphics.CGfxEffect effect, Graphics.CGfxShadingEnv shadingEnv, Graphics.Mesh.CGfxMtlMesh mesh, UInt32 index)
        {
            if (IsEffectValid == 2)
            {
                if(Effect!=effect)
                {
                    Effect = effect;
                    if (Effect.CacheData.PerFrameId != UInt32.MaxValue)
                        BindCBuffer(Effect.ShaderProgram, Effect.CacheData.PerFrameId, CEngine.Instance.PerFrameCBuffer);
                }
            }
            ShadingEnv = shadingEnv;
            Effect = effect;
            MtlMesh = mesh;
            MeshIndex = index;
            
            if (mRenderPipeline == null)
            {
                var RplDesc = new CRenderPipelineDesc();
                RenderPipeline = RHICtx.CreateRenderPipeline(RplDesc);
            }

            this.BindGeometry(MtlMesh.mRootSceneMesh.MeshPrimitives, index, 0);
        }
        public async System.Threading.Tasks.Task<bool> InitPassForViewportView(CRenderContext RHICtx, Graphics.CGfxShadingEnv shadingEnv, Graphics.CGfxMaterialInstance MtlInst,
            Graphics.Mesh.CGfxMesh ViewportMesh)
        {
            var effect = GetEffect(RHICtx, shadingEnv.EnvCode, MtlInst, ViewportMesh);
            if (effect == null)
                return false;
            await effect.AwaitLoad();
            if (effect.ShaderProgram == null)
                return false;

            this.ShadingEnv = shadingEnv;
            this.Effect = effect;
            this.mMtlInst = MtlInst;
            this.MtlMesh = ViewportMesh.MtlMeshArray[0];
            if (mRenderPipeline == null)
            {
                var RplDesc = new CRenderPipelineDesc();
                RenderPipeline = RHICtx.CreateRenderPipeline(RplDesc);
            }

            BindCBuffer(effect.ShaderProgram, effect.CacheData.PerInstanceId, ViewportMesh.MtlMeshArray[0].CBuffer);
            BindCBuffer(effect.ShaderProgram, effect.CacheData.PerFrameId, CEngine.Instance.PerFrameCBuffer);

            this.BindGeometry(ViewportMesh.MeshPrimitives, 0, 0);
            
            PreUse();

            return true;
        }
        //private UInt32 ShaderEnvVersion = 0;
        private UInt32 EffectVersion = 0;
        //public bool IsValid
        //{
        //    get
        //    {
        //        if(Effect.Desc.EnvShaderPatch.Version != ShaderEnvVersion)
        //        {
        //            //ShaderEnv变了，材质等其他信息没有变
        //            //引擎重新更新ShaderEnv等宏变化的信息，产生新的desc
        //            //然后重新从EffectManager中获得新的Effect
        //            //同时EffectVersion要复原，保证后面PreUse
        //            var desc = MtlMesh.GetEffectDesc(MtlMesh.mRootSceneMesh, Effect.Desc.EnvShaderPatch);
        //            var newEffect = CEngine.Instance.EffectManager.GetEffect(CEngine.Instance.RenderContext, desc);
        //            ShaderEnvVersion = Effect.Desc.EnvShaderPatch.Version;
        //            if (newEffect != Effect)
        //            {
        //                Effect = newEffect;
        //                EffectVersion = 0;
        //            }
        //        }
        //        return (EffectVersion == Effect.Version);
        //    }
        //}
        private int IsEffectValid = 0;
        public bool PreUse()
        {
            if (IsEffectValid==2 && Effect.IsValid)
                return true;
            else if (IsEffectValid == 1)
                return false;

            IsEffectValid = 1;
            return Effect.PreUse((successed) =>
            {
                if (successed == false)
                {
                    return;
                }
                var rc = CEngine.Instance.RenderContext;
                GpuProgram = Effect.ShaderProgram;
                if(Effect.CacheData.PerFrameId!= UInt32.MaxValue)
                    BindCBuffer(Effect.ShaderProgram, Effect.CacheData.PerFrameId, CEngine.Instance.PerFrameCBuffer);
                
                if (MtlMesh != null)
                {
                    //MtlMesh.MtlInst.BindTextures(ShaderResources, Effect.ShaderProgram);
                    MtlMesh.FillPassData(rc, this, Effect, PassIndex, true);

                    BindCBuffer(Effect.ShaderProgram, Effect.CacheData.PerInstanceId, MtlMesh.CBuffer);
                    BindCBuffer(Effect.ShaderProgram, Effect.CacheData.CBID_Mesh, MtlMesh.mRootSceneMesh.CBuffer);
                }
                else
                {
                    if (MtlInst!=null && Effect.CacheData.PerInstanceId != UInt32.MaxValue)
                    {
                        var mCBuffer = rc.CreateConstantBuffer(Effect.ShaderProgram, Effect.CacheData.PerInstanceId);

                        MtlInst.SetCBufferVars(mCBuffer);
                        BindCBuffer(Effect.ShaderProgram, Effect.CacheData.PerInstanceId, mCBuffer);
                    }
                }
                EffectVersion = Effect.Version;
                IsEffectValid = 2;
            });
        }
        private Graphics.CGfxEffect GetEffect(CRenderContext RHICtx, 
            Graphics.GfxEnvShaderCode ShaderEnv, 
            Graphics.CGfxMaterialInstance mtl,
            Graphics.Mesh.CGfxMesh mesh)
        {
            var desc = Graphics.CGfxEffectDesc.CreateDesc(mtl.Material, mesh.MdfQueue, ShaderEnv);
            
            return CEngine.Instance.EffectManager.GetEffect(RHICtx, desc);
        }
        CRenderPipeline mRenderPipeline;
        public CRenderPipeline RenderPipeline
        {
            get { return mRenderPipeline; }
            set
            {
                mRenderPipeline = value;
                if (value != null)
                {
                    SDK_IPass_BindPipeline(CoreObject, value.CoreObject);
                }
                else
                {
                    CRenderPipeline.NativePointer temp;
                    temp.Pointer = IntPtr.Zero;
                    SDK_IPass_BindPipeline(CoreObject, temp);
                }
                
            }
        }

        CShaderProgram mGpuProgram;
        public CShaderProgram GpuProgram
        {
            get
            {
                if(mGpuProgram==null)
                {
                    if (Effect != null && Effect.ShaderProgram != null)
                    {
                        GpuProgram = Effect.ShaderProgram;
                    }
                }
                return mGpuProgram;
            }
            protected set
            {
                mGpuProgram = value;
                if (value != null)
                {
                    SDK_IPass_BindGpuProgram(CoreObject, value.CoreObject);
                }
                else
                {
                    CShaderProgram.NativePointer temp;
                    temp.Pointer = IntPtr.Zero;
                    SDK_IPass_BindGpuProgram(CoreObject, temp);
                }
            }
        }

        Graphics.Mesh.CGfxMeshPrimitives mGeometryMesh;
        public Graphics.Mesh.CGfxMeshPrimitives GeometryMesh
        {
            get { return mGeometryMesh; }
        }
        public void BindGeometry(Graphics.Mesh.CGfxMeshPrimitives mesh, UInt32 atomIndex, float lod)//lod(0-1)
        {
            mGeometryMesh = mesh;
            if (mesh != null)
            {
                SDK_IPass_BindGeometry(CoreObject, mesh.CoreObject, atomIndex, lod);
            }
        }
        CVertexArray mAttachVBs;
        public CVertexArray AttachVBs
        {
            get { return mAttachVBs; }
            set
            {
                mAttachVBs = value;
                if (value != null)
                {
                    SDK_IPass_BindAttachVBs(CoreObject, value.CoreObject);
                }
            }
        }
        CIndexBuffer mAttachIndexBuffer;
        public CIndexBuffer AttachIndexBuffer
        {
            get { return mAttachIndexBuffer; }
            set
            {
                mAttachIndexBuffer = value;
                if (value != null)
                {
                    SDK_IPass_BindAttachIndexBuffer(CoreObject, value.CoreObject);
                }
            }
        }
        CShaderResources mAttachSRVs = null;
        public CShaderResources AttachSRVs
        {
            get { return mAttachSRVs; }
            set
            {
                mAttachSRVs = value;
                if (value != null)
                {
                    SDK_IPass_BindAttachSRVs(CoreObject, value.CoreObject);
                }
            }
        }
        CShaderResources mShaderResources = null;
        public CShaderResources ShaderResources
        {
            get
            {
                if(mShaderResources==null)
                {
                    mShaderResources = new CShaderResources();
                    SDK_IPass_BindShaderResouces(CoreObject, mShaderResources.CoreObject);
                }
                return mShaderResources;
            }
        }
        public void BindShaderTextures(Graphics.Mesh.CGfxMtlMesh mesh)
        {
            if (mesh.Textures.Count == 0)
                return;
            using (var i = mesh.Textures.GetEnumerator())
            {
                while(i.MoveNext())
                {
                    ShaderResources.PSBindTexture(i.Current.Key, i.Current.Value); 
                }
            }
        }
        CShaderSamplers mShaderSamplerBinder;
        public CShaderSamplers ShaderSamplerBinder
        {
            get
            {
                if (mShaderSamplerBinder == null)
                {
                    mShaderSamplerBinder = new CShaderSamplers();
                    SDK_IPass_BindShaderSamplers(CoreObject, mShaderSamplerBinder.CoreObject);
                }
                return mShaderSamplerBinder;
            }
            set
            {
                if (mShaderSamplerBinder == value)
                    return;
                mShaderSamplerBinder = value;
                if (value != null)
                    SDK_IPass_BindShaderSamplers(CoreObject, value.CoreObject);
            }
        }
        CViewport mViewPort;
        public CViewport ViewPort
        {
            get { return mViewPort; }
            set
            {
                if (mViewPort == value)
                    return;
                mViewPort = value;
                if (value != null)
                    SDK_IPass_BindViewPort(CoreObject, value.CoreObject);
            }
        }
        CScissorRect mScissor = null;
        public CScissorRect Scissor
        {
            get { return mScissor; }
            set
            {
                mScissor = value;
                if (value != null)
                    SDK_IPass_BindScissor(CoreObject, value.CoreObject);
                else
                    SDK_IPass_BindScissor(CoreObject, CScissorRect.NativePointer.NullPointer);
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void BindCBuffer(CShaderProgram shaderProgram, string name, CConstantBuffer cbuffer)
        {
            if (cbuffer == null)
                return;
            var index = shaderProgram.FindCBuffer(name);
            SDK_IPass_BindCBuffAll(CoreObject, shaderProgram.CoreObject, index, cbuffer.CoreObject);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void BindCBuffer(CShaderProgram shaderProgram, UInt32 index, CConstantBuffer cbuffer)
        {
            if (cbuffer == null)
                return;

            SDK_IPass_BindCBuffAll(CoreObject, shaderProgram.CoreObject, index, cbuffer.CoreObject);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void BindCBufferVS(UInt32 Index, CConstantBuffer CBuffer)
        {
            SDK_IPass_BindCBufferVS(CoreObject, Index, CBuffer.CoreObject);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void BindCBufferPS(UInt32 Index, CConstantBuffer CBuffer)
        {
            SDK_IPass_BindCBufferPS(CoreObject, Index, CBuffer.CoreObject);
        }
        public void SetInstanceNumber(int instNum)
        {
            SDK_IPass_SetInstanceNumber(CoreObject, instNum);
        }
        public void SetIndirectDraw(CGpuBuffer pBuffer, UInt32 offset)
        {
            if (pBuffer == null)
                return;
            SDK_IPass_SetIndirectDraw(CoreObject, pBuffer.CoreObject, offset);
        }
        public CDrawPrimitiveDesc DrawPrimitive
        {
            get
            {
                unsafe
                {
                    CDrawPrimitiveDesc desc;
                    SDK_IPass_GetDrawPrimitive(CoreObject, &desc);
                    return desc;
                }
            }
        }
        public CConstantBuffer FindCBufferVS(string name)
        {
            var ptr = SDK_IPass_FindCBufferVS(CoreObject, name);
            if (ptr.GetPointer() == IntPtr.Zero)
                return null;
            var result = new CConstantBuffer(ptr);
            result.Core_AddRef();
            return result;
        }
        public CConstantBuffer FindCBufferPS(string name)
        {
            var ptr = SDK_IPass_FindCBufferPS(CoreObject, name);
            if (ptr.GetPointer() == IntPtr.Zero)
                return null;
            var result = new CConstantBuffer(ptr);
            result.Core_AddRef();
            return result;
        }
        #region SDK
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static UInt32 SDK_IPass_GetUserFlags(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_IPass_SetUserFlags(NativePointer self, UInt32 flags);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_IPass_BindViewPort(NativePointer self, CViewport.NativePointer vp);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_IPass_BindScissor(NativePointer self, CScissorRect.NativePointer vp);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_IPass_BindPipeline(NativePointer self, CRenderPipeline.NativePointer pipeline);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]

        public extern static void SDK_IPass_BindGpuProgram(NativePointer self, CShaderProgram.NativePointer GpuProgram);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_IPass_BindGeometry(NativePointer self, Graphics.Mesh.CGfxMeshPrimitives.NativePointer mesh, UInt32 atomIndex, float lod);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_IPass_SetLod(NativePointer self, float lod);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static float SDK_IPass_GetLod(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_IPass_BindAttachVBs(NativePointer self, CVertexArray.NativePointer va);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_IPass_BindAttachIndexBuffer(NativePointer self, CIndexBuffer.NativePointer ib);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_IPass_BindAttachSRVs(NativePointer self, CShaderResources.NativePointer srv);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_IPass_BindShaderResouces(NativePointer self, CShaderResources.NativePointer res);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_IPass_BindShaderSamplers(NativePointer self, CShaderSamplers.NativePointer samps);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_IPass_BindCBufferVS(NativePointer self, UInt32 Index, CConstantBuffer.NativePointer CBuffer);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_IPass_BindCBufferPS(NativePointer self, UInt32 Index, CConstantBuffer.NativePointer CBuffer);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_IPass_SetInstanceNumber(NativePointer self, int instNum);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_IPass_SetIndirectDraw(NativePointer self, CGpuBuffer.NativePointer pBuffer, UInt32 offset);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static CRenderPipeline.NativePointer SDK_IPass_GetPipeline(NativePointer self);

        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static CShaderProgram.NativePointer SDK_IPass_GetGpuProgram(NativePointer self);
        
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static CShaderResources.NativePointer SDK_IPass_GetShaderResurces(NativePointer self);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static unsafe void SDK_IPass_GetDrawPrimitive(NativePointer self, CDrawPrimitiveDesc* desc);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static CConstantBuffer.NativePointer SDK_IPass_FindCBufferVS(NativePointer self, string name);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static CConstantBuffer.NativePointer SDK_IPass_FindCBufferPS(NativePointer self, string name);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_IPass_BindCBuffAll(NativePointer self, CShaderProgram.NativePointer shaderProgram, UInt32 cbIndex, CConstantBuffer.NativePointer CBuffer);
        #endregion
    }
}
