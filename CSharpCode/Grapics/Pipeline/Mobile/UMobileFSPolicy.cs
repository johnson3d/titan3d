using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline.Mobile
{
    public class UMobileFSPolicy : IRenderPolicy
    {
        public override Common.UBasePassNode GetBasePassNode() 
        { 
            return BasePassNode; 
        }
        public UMobileBasePassNode BasePassNode = new UMobileBasePassNode();
        public RHI.CShaderResourceView EnvMapSRV;
        public override RHI.CShaderResourceView GetFinalShowRSV()
        {
            return BasePassNode.GBuffers.GBufferSRV[0];
        }

        public override async System.Threading.Tasks.Task Initialize(float x, float y)
        {
            await BasePassNode.Initialize(this, null, EPixelFormat.PXF_R16G16B16A16_FLOAT, EPixelFormat.PXF_D24_UNORM_S8_UINT, x, y);
            
            EnvMapSRV = await UEngine.Instance.GfxDevice.TextureManager.GetTexture(RName.GetRName("utest/texture/default_envmap.srv"));
        }
        public override void OnResize(float x, float y)
        {
            base.OnResize(x, y);

            BasePassNode.OnResize(x, y);
        }
        public override void Cleanup()
        {
            BasePassNode.Cleanup();
            base.Cleanup();
        }
        public override Shader.UShadingEnv GetPassShading(EShadingType type, Mesh.UMesh mesh, int atom)
        {
            switch (type)
            {
                case EShadingType.BasePass:
                    return BasePassNode.mBasePassShading;
            }
            return null;
        }
        public override void OnDrawCall(Pipeline.IRenderPolicy.EShadingType shadingType, RHI.CDrawCall drawcall, Mesh.UMesh mesh, int atom)
        {
            base.OnDrawCall(shadingType, drawcall, mesh, atom);
            if (shadingType == EShadingType.BasePass)
                BasePassNode.mBasePassShading.OnDrawCall(shadingType, drawcall, this, mesh);
        }
        public unsafe override void TickLogic()
        {
            BasePassNode.TickLogic(this);
        }
        public unsafe override void TickRender()
        {
            BasePassNode.TickRender(this);
        }
        public unsafe override void TickSync()
        {
            BasePassNode.TickSync(this);
        }
    }
}
