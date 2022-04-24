using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.RHI
{
    public partial class CDrawCall : AuxPtrType<IDrawCall>
    {
        public Graphics.Pipeline.Shader.UEffect Effect { get; private set; }
        public object TagObject { get; set; }
        private Graphics.Pipeline.Shader.UShadingEnv.FPermutationId PermutationId;
        //internal uint MaterialSerialId = 0;
        private bool IsUpdating = false;
        public bool IsPermutationChanged()
        {
            var shading = Effect.ShadingEnv;
            return PermutationId != shading.mCurrentPermutationId;
        }
        public void CheckPermutation(Graphics.Pipeline.Shader.UMaterial material, Graphics.Pipeline.Shader.UMdfQueue mdf)
        {
            var shading = Effect.ShadingEnv;
            //if (shading == null)
            //    return;
            if (PermutationId == shading.mCurrentPermutationId && material.MaterialHash == Effect.Desc.MaterialHash)
                return;

            if (IsUpdating)
                return;

            PermutationId = shading.mCurrentPermutationId;
            var task = UpdateShaderProgram(shading, material, mdf);
        }
        public async System.Threading.Tasks.Task UpdateShaderProgram(Graphics.Pipeline.Shader.UShadingEnv shading, Graphics.Pipeline.Shader.UMaterial material, Graphics.Pipeline.Shader.UMdfQueue mdf)
        {
            IsUpdating = true;
            PermutationId = shading.mCurrentPermutationId;

            Effect = await UEngine.Instance.GfxDevice.EffectManager.GetEffect(shading, material, mdf);
            if (Effect != null)
            {
                mCoreObject.GetPipeline().BindGpuProgram(Effect.ShaderProgram.mCoreObject);
            }
            IsUpdating = false;
        }
        public unsafe bool CheckMaterialParameters(Graphics.Pipeline.Shader.UMaterial Material)
        {
            //if (MaterialSerialId == Material.SerialId)
            //    return false;
            //MaterialSerialId = Material.SerialId;
            // 这里不应该被调用了，全部再UAtom上记录了材质版本变化
            //强制刷一下GpuProgram，避免出现Material刷新后，后续MaterialInstance的CheckPermutation不能正确绑定
            var pipeline = mCoreObject.GetPipeline();
            pipeline.BindGpuProgram(Effect.ShaderProgram.mCoreObject);

            if (Material.BlendState != null)
                pipeline.BindBlendState(Material.BlendState.mCoreObject);
            if (Material.DepthStencilState != null)
                pipeline.BindDepthStencilState(Material.DepthStencilState.mCoreObject);
            if (Material.RasterizerState != null)
                pipeline.BindRasterizerState(Material.RasterizerState.mCoreObject);

            var textures = new IShaderRViewResources(mCoreObject.GetShaderRViewResources());
            for (int j = 0; j < Material.NumOfSRV; j++)
            {
                var varName = Material.GetNameOfSRV(j);
                unsafe
                {
                    var bindInfo = mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Srv, varName);
                    var srv = Material.TryGetSRV(j);
                    if (bindInfo->PSBindPoint != 0xffffffff && srv != null)
                    {
                        textures.BindPS(bindInfo->PSBindPoint, srv.mCoreObject);
                    }
                    if (bindInfo->VSBindPoint != 0xffffffff && srv != null)
                    {
                        textures.BindVS(bindInfo->VSBindPoint, srv.mCoreObject);
                    }
                }
            }

            if (Effect.ShaderIndexer.cbPerMaterial!= 0xFFFFFFFF)
            {
                if (Material != null)
                {
                    var rc = UEngine.Instance.GfxDevice.RenderContext;
                    Material.PerMaterialCBuffer = rc.CreateConstantBuffer(Effect.ShaderProgram, Effect.ShaderIndexer.cbPerMaterial);
                    Material.UpdateUniformVars(Material.PerMaterialCBuffer);
                    mCoreObject.BindShaderCBuffer(Effect.ShaderIndexer.cbPerMaterial, Material.PerMaterialCBuffer.mCoreObject);
                }
            }
            if (Effect.ShaderIndexer.cbPerFrame != 0xFFFFFFFF)
            {
                mCoreObject.BindShaderCBuffer(Effect.ShaderIndexer.cbPerFrame, UEngine.Instance.GfxDevice.PerFrameCBuffer.mCoreObject);
            }
            return true;
        }

        public void SetInstanceNumber(int instNum)
        {
            mCoreObject.SetInstanceNumber(instNum);
        }
        public void BindCBuffer(string name, CConstantBuffer cbuffer)
        {
            unsafe
            {
                var binder = mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_CBuffer, name);
                if (!CoreSDK.IsNullPointer(binder))
                    BindCBuffer(binder, cbuffer);
            }
        }
        public unsafe void BindCBuffer(IShaderBinder* binder, CConstantBuffer cbuffer)
        {
            mCoreObject.BindShaderCBuffer(binder, cbuffer.mCoreObject);
        }
        public void BindSrv(string name, CShaderResourceView srv)
        {
            unsafe
            {
                var binder = mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Srv, name);
                if (!CoreSDK.IsNullPointer(binder))
                    BindSrv(binder, srv);
            }
        }
        public unsafe void BindSrv(IShaderBinder* binder, CShaderResourceView srv)
        {
            mCoreObject.BindShaderSrv(binder, srv.mCoreObject);
        }
        public void SetIndirectDraw(RHI.CGpuBuffer pBuffer, uint offset)
        {
            mCoreObject.SetIndirectDraw(pBuffer.mCoreObject, offset);
        }
        public ShaderReflector GetReflector()
        {
            return mCoreObject.GetReflector();
        }
    }

    public partial class CComputeDrawcall : AuxPtrType<IComputeDrawcall>
    {
        public void SetComputeShader(RHI.CComputeShader shader)
        {
            mCoreObject.SetComputeShader(shader.mCoreObject);
        }
        public void SetDispatchIndirectBuffer(RHI.CGpuBuffer buffer, uint offset)
        {
            mCoreObject.SetDispatchIndirectBuffer(buffer.mCoreObject, offset);
        }
        public void BindCBuffer(string name, CConstantBuffer cbuffer)
        {
            unsafe
            {
                var binder = mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_CBuffer, name);
                if (!CoreSDK.IsNullPointer(binder))
                {
                    BindCBuffer(binder, cbuffer);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"BindCBuffer: {name} is not found");
                }
            }
        }
        public unsafe void BindCBuffer(IShaderBinder* binder, CConstantBuffer cbuffer)
        {
            mCoreObject.GetCBufferResources().BindCS(binder->m_CSBindPoint, cbuffer.mCoreObject);
        }
        public void BindUav(string name, CUnorderedAccessView uav)
        {
            unsafe
            {
                var binder = mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Uav, name);
                if (!CoreSDK.IsNullPointer(binder))
                {
                    BindUav(binder, uav);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"BindUav: {name} is not found");
                }
            }
        }
        public unsafe void BindUav(IShaderBinder* binder, CUnorderedAccessView uav)
        {
            mCoreObject.GetUavResources().BindCS(binder->m_CSBindPoint, uav.mCoreObject);
        }
        public void BindSrv(string name, CShaderResourceView srv)
        {
            unsafe
            {
                var binder = mCoreObject.GetReflector().GetShaderBinder(EShaderBindType.SBT_Srv, name);
                if (!CoreSDK.IsNullPointer(binder))
                {
                    BindSrv(binder, srv);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"BindSrv: {name} is not found");
                }
            }
        }
        public unsafe void BindSrv(IShaderBinder* binder, CShaderResourceView srv)
        {
            mCoreObject.GetShaderRViewResources().BindCS(binder->m_CSBindPoint, srv.mCoreObject);
        }
        public void BuildPass(ICommandList cmd)
        {
            mCoreObject.BuildPass(cmd);
        }
        public void BuildPass(RHI.CCommandList cmd)
        {
            mCoreObject.BuildPass(cmd.mCoreObject);
        }
    }

    public partial class CCopyDrawcall : AuxPtrType<ICopyDrawcall>
    {
        public void SetCopyBuffer(EngineNS.IGpuBuffer src, uint srcOffset, EngineNS.IGpuBuffer tar, uint tarOffset, uint size)
        {
            mCoreObject.SetCopyBuffer(src, srcOffset, tar, tarOffset, size);
        }
        public void SetCopyTexture2D(EngineNS.IGpuBuffer src, uint srcMip, uint srcX, uint srcY, EngineNS.IGpuBuffer tar, uint tarMip, uint tarX, uint tarY, uint width, uint height)
        {
            mCoreObject.SetCopyTexture2D(src, srcMip, srcX, srcY, tar, tarMip, tarX, tarY, width, height);
        }
        public void BuildPass(RHI.CCommandList cmd)
        {
            mCoreObject.BuildPass(cmd.mCoreObject);
        }
    }
}
