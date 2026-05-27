using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline.Common
{
    public partial class TtGpuSceneNode
    {
        public TtRenderGraphPin PointLightsPinOut = TtRenderGraphPin.CreateOutput("PointLights", false, EPixelFormat.PXF_UNKNOWN, NxRHI.EBufferType.BFT_SRV | NxRHI.EBufferType.BFT_UAV);
        public TtRenderGraphPin SpotLightsPinOut = TtRenderGraphPin.CreateOutput("SpotLights", false, EPixelFormat.PXF_UNKNOWN, NxRHI.EBufferType.BFT_SRV | NxRHI.EBufferType.BFT_UAV);

        public TtCpu2GpuBuffer<Shader.FPointLight> PointLights = new TtCpu2GpuBuffer<Shader.FPointLight>();
        public TtCpu2GpuBuffer<Shader.FSpotLight> SpotLights = new TtCpu2GpuBuffer<Shader.FSpotLight>();

        public void Initialize_Light(TtRenderPolicy policy, string debugName)
        {
            PointLights.Initialize(NxRHI.EBufferType.BFT_SRV);
            SpotLights.Initialize(NxRHI.EBufferType.BFT_SRV);
        }

        private void Dispose_Light()
        {
            PointLights?.Dispose();
            PointLights = null;

            SpotLights?.Dispose();
            SpotLights = null;

            CoreSDK.DisposeObject(ref PointLightsAttachement);
            CoreSDK.DisposeObject(ref SpotLightsAttachement);
        }
        TtAttachBuffer PointLightsAttachement = new TtAttachBuffer();
        TtAttachBuffer SpotLightsAttachement = new TtAttachBuffer();
        private unsafe void FrameBuild_Light()
        {
            PointLightsPinOut.Attachement.Height = (uint)PointLights.DataArray.Count;
            PointLightsPinOut.Attachement.Width = (uint)sizeof(Shader.FPointLight);
            var attachement = RenderGraph.AttachmentCache.ImportAttachment(PointLightsPinOut, PointLightsAttachement);
            attachement.GpuResource = PointLights.GpuBuffer;
            attachement.Srv = PointLights.Srv;
            attachement.Uav = PointLights.Uav;

            SpotLightsPinOut.Attachement.Height = (uint)SpotLights.DataArray.Count;
            SpotLightsPinOut.Attachement.Width = (uint)sizeof(Shader.FSpotLight);
            var spotAttachement = RenderGraph.AttachmentCache.ImportAttachment(SpotLightsPinOut, SpotLightsAttachement);
            spotAttachement.GpuResource = SpotLights.GpuBuffer;
            spotAttachement.Srv = SpotLights.Srv;
            spotAttachement.Uav = SpotLights.Uav;
        }
        private void TickLogic_Light(GamePlay.TtWorld world, Graphics.Pipeline.TtRenderPolicy policy, NxRHI.TtCommandList cmd)
        {
            PointLights.Clear();
            SpotLights.Clear();

            if (policy.DisablePointLight == false)
            {
                foreach (var i in CpuCullNode.VisParameter.VisibleNodes)
                {
                    var pointLight = i as GamePlay.Scene.TtPointLightNode;
                    if (pointLight != null)
                    {
                        var lightData = pointLight.NodeData as GamePlay.Scene.TtPointLightNode.TtLightNodeData;

                        Shader.FPointLight light;
                        var pos = pointLight.Placement.AbsTransform.Position;
                        var localPos = pos.ToLocalPosition(world.CameraOffset);
                        light.PositionAndRadius = new Vector4(localPos, lightData.Radius);
                        light.ColorAndIntensity = new Vector4(lightData.Color.X, lightData.Color.Y, lightData.Color.Z, lightData.Intensity);
                        pointLight.IndexInGpuScene = PointLights.PushData(light);
                        continue;
                    }

                    var spotLight = i as GamePlay.Scene.TtSpotLightNode;
                    if (spotLight != null)
                    {
                        var spotData = spotLight.NodeData as GamePlay.Scene.TtSpotLightNode.TtSpotLightNodeData;

                        Shader.FSpotLight sLight;
                        var pos = spotLight.Placement.AbsTransform.Position;
                        var posF = pos.ToLocalPosition(world.CameraOffset);
                        var dir = spotLight.GetDirection();
                        float radius = spotData.Radius;
                        float innerCos = (float)Math.Cos(spotData.InnerConeAngle * Math.PI / 180.0);
                        float outerCos = (float)Math.Cos(spotData.OuterConeAngle * Math.PI / 180.0);
                        float outerSin = (float)Math.Sin(spotData.OuterConeAngle * Math.PI / 180.0);

                        sLight.PositionAndRadius = new Vector4(posF, radius);
                        sLight.DirectionAndInnerCos = new Vector4(dir.X, dir.Y, dir.Z, innerCos);
                        sLight.ColorAndIntensity = new Vector4(spotData.Color.X, spotData.Color.Y, spotData.Color.Z, spotData.Intensity);
                        sLight.OuterCosAndPad = new Vector4(outerCos, 0, 0, 0);

                        // CPU precomputed cone bounding sphere (UE5 approach)
                        Vector3 boundCenter;
                        float boundRadius;
                        if (outerCos >= 0.5f)
                        {
                            // Narrow cone: sphere center offset along axis
                            boundCenter = posF + new Vector3(dir.X, dir.Y, dir.Z) * (radius * outerCos);
                            boundRadius = radius * outerSin;
                        }
                        else
                        {
                            // Wide cone: minimal enclosing sphere
                            float halfOverCos = 0.5f * radius / outerCos;
                            boundCenter = posF + new Vector3(dir.X, dir.Y, dir.Z) * halfOverCos;
                            boundRadius = halfOverCos;
                        }
                        sLight.BoundCenterAndBoundRadius = new Vector4(boundCenter, boundRadius);

                        spotLight.IndexInGpuScene = SpotLights.PushData(sLight);
                    }
                }

                PointLights.Flush2GPU(cmd.mCoreObject);
                SpotLights.Flush2GPU(cmd.mCoreObject);
                this.PerGpuSceneCbv.SetValue("GpuScene_PointLightNum", (uint)PointLights.DataArray.Count);
                this.PerGpuSceneCbv.SetValue("GpuScene_SpotLightNum", (uint)SpotLights.DataArray.Count);
            }
            else
            {
                this.PerGpuSceneCbv.SetValue("GpuScene_PointLightNum", (uint)0);
                this.PerGpuSceneCbv.SetValue("GpuScene_SpotLightNum", (uint)0);
            }
        }
    }
}
