using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.RHI
{
    public partial class CDrawCall : AuxPtrType<IDrawCall>
    {
        public Graphics.Pipeline.Shader.UEffect Effect { get; private set; }
        public object TagObject { get; set; }
        private uint PermutationId;
        internal uint MaterialSerialId = 0;
        private bool IsUpdating = false;
        public void CheckPermutation(Graphics.Pipeline.Shader.UMaterial material, Graphics.Pipeline.Shader.UMdfQueue mdf)
        {
            var shading = Effect.ShadingEnv;
            //if (shading == null)
            //    return;
            if (PermutationId == shading.CurrentPermutationId && material.MaterialHash == Effect.Desc.MaterialHash)
                return;

            if (IsUpdating)
                return;

            PermutationId = shading.CurrentPermutationId;
            var task = UpdateShaderProgram(shading, material, mdf);
        }
        public async System.Threading.Tasks.Task UpdateShaderProgram(Graphics.Pipeline.Shader.UShadingEnv shading, Graphics.Pipeline.Shader.UMaterial material, Graphics.Pipeline.Shader.UMdfQueue mdf)
        {
            IsUpdating = true;
            PermutationId = shading.CurrentPermutationId;

            Effect = await UEngine.Instance.GfxDevice.EffectManager.GetEffect(shading, material, mdf);
            if (Effect != null)
            {
                unsafe
                {
                    var pipeline = new IRenderPipeline(mCoreObject.GetPipeline());
                    if (pipeline.NativePointer == IntPtr.Zero)
                    {
                        var desc = new IRenderPipelineDesc();
                        desc.GpuProgram = Effect.ShaderProgram.mCoreObject;
                        var pl = UEngine.Instance.GfxDevice.RenderContext.CreateRenderPipeline(ref desc);
                        mCoreObject.BindPipeline(pl.mCoreObject);
                    }
                    else
                    {
                        pipeline.BindGpuProgram(Effect.ShaderProgram.mCoreObject);
                    }
                }
            }
            IsUpdating = false;
        }
        public unsafe bool CheckMaterialParameters(Graphics.Pipeline.Shader.UMaterial Material)
        {
            if (MaterialSerialId == Material.SerialId)
                return false;
            MaterialSerialId = Material.SerialId;
            //强制刷一下GpuProgram，避免出现Material刷新后，后续MaterialInstance的CheckPermutation不能正确绑定
            var pipeline = new IRenderPipeline(mCoreObject.GetPipeline());
            if (pipeline.NativePointer == IntPtr.Zero)
            {
                var desc = new IRenderPipelineDesc();
                desc.GpuProgram = Effect.ShaderProgram.mCoreObject;
                var pl = UEngine.Instance.GfxDevice.RenderContext.CreateRenderPipeline(ref desc);
                mCoreObject.BindPipeline(pl.mCoreObject);
            }
            else
            {
                pipeline.BindGpuProgram(Effect.ShaderProgram.mCoreObject);
            }

            var textures = new IShaderResources(mCoreObject.GetShaderResources());
            for (int j = 0; j < Material.NumOfSRV; j++)
            {
                var varName = Material.GetNameOfSRV(j);
                var bindInfo = new TSBindInfo();
                unsafe
                {
                    mCoreObject.GetSRVBindInfo(varName, ref bindInfo, sizeof(TSBindInfo));
                }
                var srv = Material.TryGetSRV(j);
                if (bindInfo.PSBindPoint != 0xffffffff && srv != null)
                {
                    textures.PSBindTexture(bindInfo.PSBindPoint, srv.mCoreObject);
                }
                if (bindInfo.VSBindPoint != 0xffffffff && srv != null)
                {
                    textures.VSBindTexture(bindInfo.VSBindPoint, srv.mCoreObject);
                }
            }

            if (Effect.CBPerMaterialIndex != 0xFFFFFFFF)
            {
                if (Material != null)
                {
                    var rc = UEngine.Instance.GfxDevice.RenderContext;
                    Material.PerMaterialCBuffer = rc.CreateConstantBuffer(Effect.ShaderProgram, Effect.CBPerMaterialIndex);
                    Material.UpdateUniformVars(Material.PerMaterialCBuffer);
                    mCoreObject.BindCBufferAll(Effect.CBPerMaterialIndex, Material.PerMaterialCBuffer.mCoreObject);
                }
            }
            if (Effect.CBPerFrameIndex != 0xFFFFFFFF)
            {
                mCoreObject.BindCBufferAll(Effect.CBPerFrameIndex, UEngine.Instance.GfxDevice.PerFrameCBuffer.mCoreObject);
            }
            return true;
        }        
    }
}
