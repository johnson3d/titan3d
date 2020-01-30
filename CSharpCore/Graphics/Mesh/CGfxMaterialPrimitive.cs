using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace EngineNS.Graphics.Mesh
{
    public class CGfxMaterialPrimitive : AuxCoreObject<CGfxMaterialPrimitive.NativePointer>
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

        public CGfxMaterialPrimitive()
        {
            mCoreObject = NewNativeObjectByName<NativePointer>($"{CEngine.NativeNS}::GfxMaterialPrimitive");
        }

        int mWorldMatrixId = -1;
        public int WorldMatrixId
        {
            get { return mWorldMatrixId; }
        }
        int mWorldViewProjectionMatrixId = -1;
        public int WorldViewProjectionMatrixId
        {
            get { return mWorldViewProjectionMatrixId; }
        }
        public void SetWorldMatrix(ref Matrix worldMatrix, CGfxCamera camera)
        {
            if (mCBuffer == null)
                return;
            mCBuffer.SetValue(mWorldMatrixId, worldMatrix, 0);
            if (camera != null)
            {
                var mvp = worldMatrix * camera.ViewProjection;
                mCBuffer.SetValue(mWorldViewProjectionMatrixId, mvp, 0); 
            }
        }
        private CGfxEffect GetEffect(CRenderContext rc, CGfxMesh mesh, bool tryLoad)
        {
            CGfxEffectDesc desc = new CGfxEffectDesc();
            desc.Material = Material.Material;
            desc.ShadingEnv = mesh.ShadingEnv;
            desc.MdfQueue = mesh.MdfQueue;
            desc.Definitions = ShaderDefinition;

            desc.UpdateDefinitions();

            return CEngine.Instance.EffectManager.GetEffect(rc, desc, tryLoad);
        }
        public bool Init(CRenderContext rc, CGfxMesh mesh, CGfxMaterialInstance material)
        {
            Material = material;
            mEffect = GetEffect(rc, mesh, true);
            if (mEffect == null)
                return false;

            UpdateMaterialCBuffer(rc, true);

            return true;
        }

        public void RefreshEffect(CRenderContext rc)
        {
            mEffect.RefreshEffect(rc);
            UpdateMaterialCBuffer(rc, true);
        }
        public void UpdateMaterialCBuffer(CRenderContext rc, bool createCBuffer)
        {
            var cbIndex = mEffect.ShaderProgram.FindCBuffer("cbPerInstance");
            if ((int)cbIndex >= 0)
            {
                if (createCBuffer || mCBuffer == null)
                {
                    mCBuffer?.Cleanup();
                    mCBuffer = rc.CreateConstantBuffer(mEffect.ShaderProgram, cbIndex);
                }
                else
                {
                    var cbDesc = new CConstantBufferDesc();
                    if (mEffect.ShaderProgram.GetCBufferDesc(cbIndex, ref cbDesc))
                    {
                        if (mCBuffer.Size != cbDesc.Size)
                        {
                            Profiler.Log.WriteLine(Profiler.ELogTag.Warning, "Material", $"{mEffect.Desc.Material.Name} CBuffer size is invalid, recreate it");
                            mCBuffer?.Cleanup();
                            mCBuffer = rc.CreateConstantBuffer(mEffect.ShaderProgram, cbIndex);
                        }
                    }
                }

                mWorldMatrixId = mCBuffer.FindVar("WorldMatrix");
                mWorldViewProjectionMatrixId = mCBuffer.FindVar("WorldViewProjectionMatrix");
                Material.SetCBufferVars(mCBuffer);
                //for (UInt32 i = 0; i < Material.VarNumber; i++)
                //{
                //    var name = Material.GetVarName(i, false);
                //    var varIdx = mCBuffer.FindVar(name);

                //    CGfxVar varDesc = new CGfxVar();
                //    Material.GetVarDesc(i, ref varDesc);
                //    ConstantVarDesc varDesc2 = new ConstantVarDesc();
                //    var ret = mCBuffer.GetVarDesc(varIdx, ref varDesc2);
                //    if (false == ret)
                //        continue;

                //    if (varDesc.Type != varDesc2.Type || varDesc.Elements != varDesc2.Elements)
                //    {
                //        Profiler.Log.WriteLine(Profiler.ELogTag.Warning, "ShaderVar", $"MaterialInstance Var {varDesc.Type} don't match CBuffer Var {varDesc2.Type}");
                //        continue;
                //    }

                //    for (UInt32 j = 0; j < varDesc.Elements; j++)
                //    {
                //        switch (varDesc.Type)
                //        {
                //            case EShaderVarType.SVT_Float1:
                //                {
                //                    float value = 0;
                //                    Material.GetVarValue(i, j, ref value);
                //                    mCBuffer.SetValue(varIdx, value, j);
                //                }
                //                break;
                //            case EShaderVarType.SVT_Float2:
                //                {
                //                    Vector2 value = new Vector2();
                //                    Material.GetVarValue(i, j, ref value);
                //                    mCBuffer.SetValue(varIdx, value, j);
                //                }
                //                break;
                //            case EShaderVarType.SVT_Float3:
                //                {
                //                    Vector3 value = new Vector3();
                //                    Material.GetVarValue(i, j, ref value);
                //                    mCBuffer.SetValue(varIdx, value, j);
                //                }
                //                break;
                //            case EShaderVarType.SVT_Float4:
                //                {
                //                    Vector4 value = new Vector4();
                //                    Material.GetVarValue(i, j, ref value);
                //                    mCBuffer.SetValue(varIdx, value, j);
                //                }
                //                break;
                //            case EShaderVarType.SVT_Matrix4x4:
                //                {
                //                    Matrix value = new Matrix();
                //                    Material.GetVarValue(i, j, ref value);
                //                    mCBuffer.SetValue(varIdx, value, j);
                //                }
                //                break;
                //            default:
                //                break;
                //        }
                //    }
                //}
            }
        }


        protected CShaderDefinitions mShaderDefinition = new CShaderDefinitions();
        public CShaderDefinitions ShaderDefinition
        {
            get { return mShaderDefinition; }
        }
        protected CGfxEffect mEffect;
        public CGfxEffect Effect
        {
            get { return mEffect; }
        }
        protected CGfxMaterialInstance mMaterial;
        public CGfxMaterialInstance Material
        {
            get { return mMaterial; }
            protected set
            {
                mMaterial = value;
                if (value != null)
                {
                    SDK_GfxMaterialPrimitive_SetMaterial(CoreObject, value.CoreObject);
                }
            }
        }
        protected CConstantBuffer mCBuffer;
        public CConstantBuffer CBuffer
        {
            get { return mCBuffer; }
        }

        public UInt32 MeshPassMaterialVersion;
        public CShaderProgram MeshPassShaderProgram;
        public void SetPassData(CRenderContext rc, CGfxMesh mesh, UInt32 index, CGfxSceneView vp, CPass pass)
        {
            //OnEffectRefresh(rc);
            if (pass.RenderPipeline == null)
            {
                var rplDesc = new CRenderPipelineDesc();
                pass.RenderPipeline = rc.CreateRenderPipeline(rplDesc);
            }
            pass.RenderPipeline.ShaderProgram = mEffect.ShaderProgram;
            pass.RenderPipeline.RasterizerState = mMaterial.RasterizerState;
            pass.RenderPipeline.DepthStencilState = mMaterial.DepthStencilState;
            pass.RenderPipeline.BlendState = mMaterial.BlendState;
            
            pass.GeometryMesh = mesh.MeshPrimitives.GeometryMesh;
            
            pass.BindCBuffer(mEffect.ShaderProgram, mEffect.PerInstanceId, CBuffer);
            pass.BindCBuffer(mEffect.ShaderProgram, mEffect.PerFrameId, CEngine.Instance.PerFrameCBuffer);

            if (vp != null)
            {
                pass.ViewPort = vp.Viewport;
            }

            var textures = new CShaderResources();
            {
                Material.BindTextures(textures, mEffect.ShaderProgram);
                foreach (var i in Textures)
                {
                    textures.PSBindTexture(i.Key, i.Value);
                }
                mesh.ShadingEnv.BindResources(mesh, CBuffer, textures, mEffect.ShaderProgram);
            }
            pass.ShaderResources = textures;

            CShaderSamplers samplers = new CShaderSamplers();
            {
                CSamplerStateDesc sampDesc = new CSamplerStateDesc();
                sampDesc.SetDefault();
                sampDesc.Filter = ESamplerFilter.SPF_MIN_MAG_MIP_POINT;
                CSamplerState samp = CEngine.Instance.SamplerStateManager.GetSamplerState(rc, sampDesc);
                for (UInt32 i = 0; i < mEffect.ShaderProgram.SamplerNumber; i++)
                {
                    CSamplerBindInfo info = new CSamplerBindInfo();
                    mEffect.ShaderProgram.GetSamplerBindInfo(i, ref info);
                    samplers.PSBindSampler(info.PSBindPoint, samp);
                }
            }
            pass.ShaderSamplers = samplers;

            foreach(var i in mesh.MdfQueue.Modifiers)
            {
                i.OnSetPassData(pass);
            }

            CDrawPrimitiveDesc dpDesc = new CDrawPrimitiveDesc();
            mesh.MeshPrimitives.GetAtom(index, 0, ref dpDesc);
            pass.BindDrawPrimitive(ref dpDesc);
        }
        public void SetTexture(string shaderName, CShaderResourceView rsv)
        {
            var txBind = new CTextureBindInfo();
            Effect.ShaderProgram.FindTextureBindInfo(Material, "txDiffuse", ref txBind);
            Textures[txBind.PSBindPoint] = rsv;
        }
        public Dictionary<UInt32, CShaderResourceView> Textures
        {
            get;
        } = new Dictionary<uint, CShaderResourceView>();
        #region SDK
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static void SDK_GfxMaterialPrimitive_SetMaterial(NativePointer self, CGfxMaterialInstance.NativePointer material);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public extern static CGfxMaterialInstance.NativePointer SDK_GfxMaterialPrimitive_GetMaterial(NativePointer self);
        #endregion
    }
}
