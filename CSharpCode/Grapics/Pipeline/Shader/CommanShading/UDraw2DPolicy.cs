using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline.Shader.CommanShading
{
    public class UBasePassPolicy : URenderPolicy
    {
        public UShadingEnv mBasePassShading;
        public override Shader.UShadingEnv GetPassShading(EShadingType type, Mesh.UMesh mesh, int atom, Pipeline.Common.URenderGraphNode node)
        {
            switch (type)
            {
                case EShadingType.BasePass:
                    return mBasePassShading;
            }
            return null;
        }
        public override void OnDrawCall(Pipeline.URenderPolicy.EShadingType shadingType, NxRHI.UGraphicDraw drawcall, Mesh.UMesh mesh, int atom)
        {
            mBasePassShading.OnDrawCall(shadingType, drawcall, this, mesh);
        }
    }
    public class UCopy2DPolicy : URenderPolicy
    {//坐标在-1,1，直接拷贝给ps
        UCopy2DShading mBasePassShading;
        public Graphics.Pipeline.URenderPolicy ViewPolicy;
        public override Shader.UShadingEnv GetPassShading(EShadingType type, Mesh.UMesh mesh, int atom, Pipeline.Common.URenderGraphNode node)
        {
            switch (type)
            {
                case EShadingType.BasePass:
                    if (mBasePassShading == null)
                        mBasePassShading = UEngine.Instance.ShadingEnvManager.GetShadingEnv<UCopy2DShading>();
                    return mBasePassShading;
            }
            return null;
        }
        public override void OnDrawCall(Pipeline.URenderPolicy.EShadingType shadingType, NxRHI.UGraphicDraw drawcall, Mesh.UMesh mesh, int atom)
        {
            mBasePassShading.OnDrawCall(drawcall, this, mesh);
        }
    }
    public class UDraw2DPolicy : URenderPolicy
    {//屏幕像素坐标
        URect2DShading mBasePassShading;
        public override Shader.UShadingEnv GetPassShading(EShadingType type, Mesh.UMesh mesh, int atom, Pipeline.Common.URenderGraphNode node)
        {
            switch (type)
            {
                case EShadingType.BasePass:
                    if (mBasePassShading == null)
                        mBasePassShading = UEngine.Instance.ShadingEnvManager.GetShadingEnv<URect2DShading>();
                    return mBasePassShading;
            }
            return null;
        }
    }
}
