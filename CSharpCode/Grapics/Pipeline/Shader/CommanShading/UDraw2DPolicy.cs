using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline.Shader.CommanShading
{
    public class UBasePassPolicy : IRenderPolicy
    {
        public UShadingEnv mBasePassShading;
        public override Shader.UShadingEnv GetPassShading(EShadingType type, Mesh.UMesh mesh)
        {
            switch (type)
            {
                case EShadingType.BasePass:
                    return mBasePassShading;
            }
            return null;
        }
        public override void OnDrawCall(Pipeline.IRenderPolicy.EShadingType shadingType, RHI.CDrawCall drawcall, Mesh.UMesh mesh)
        {
            mBasePassShading.OnDrawCall(drawcall, this, mesh);
        }
    }
    public class UCopy2DPolicy : IRenderPolicy
    {//坐标在-1,1，直接拷贝给ps
        UCopy2DShading mBasePassShading;
        public Graphics.Pipeline.IRenderPolicy ViewPolicy;
        public override Shader.UShadingEnv GetPassShading(EShadingType type, Mesh.UMesh mesh)
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
        public override void OnDrawCall(Pipeline.IRenderPolicy.EShadingType shadingType, RHI.CDrawCall drawcall, Mesh.UMesh mesh)
        {
            mBasePassShading.OnDrawCall(drawcall, this, mesh);
        }
    }
    public class UDraw2DPolicy : IRenderPolicy
    {//屏幕像素坐标
        URect2DShading mBasePassShading;
        public override Shader.UShadingEnv GetPassShading(EShadingType type, Mesh.UMesh mesh)
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
