using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline.Common
{
    public partial class UGpuSceneNode
    {
        public TtRenderGraphPin PointLightsPinOut = TtRenderGraphPin.CreateOutput("PointLights", false, EPixelFormat.PXF_UNKNOWN);
        public struct FPointLight
        {
            public Vector4 PositionAndRadius;
            public Vector4 ColorAndIntensity;
        }
        public TtCpu2GpuBuffer<FPointLight> PointLights = new TtCpu2GpuBuffer<FPointLight>();
        public void Initialize_Light(URenderPolicy policy, string debugName)
        {
            PointLights.Initialize(NxRHI.EBufferType.BFT_SRV);
        }

        private void Dispose_Light()
        {
            PointLights?.Dispose();
            PointLights = null;
        }
        private unsafe void FrameBuild_Light()
        {
            PointLightsPinOut.Attachement.Height = (uint)PointLights.DataArray.Count;
            PointLightsPinOut.Attachement.Width = (uint)sizeof(FPointLight);
            var attachement = RenderGraph.AttachmentCache.ImportAttachment(PointLightsPinOut);
            //if (attachement.Buffer == null)
            //{
            //    PointLights.Flush2GPU(this.BasePass.DrawCmdList.mCoreObject);
            //}
            attachement.Buffer = PointLights.GpuBuffer;
            attachement.Srv = PointLights.Srv;
            attachement.Uav = PointLights.Uav;
        }
        private void TickLogic_Light(GamePlay.UWorld world, Graphics.Pipeline.URenderPolicy policy, NxRHI.UCommandList cmd)
        {
            PointLights.Clear();
            if (policy.DisablePointLight == false)
            {
                foreach (var i in policy.VisibleNodes)
                {
                    var pointLight = i as GamePlay.Scene.UPointLightNode;
                    if (pointLight == null)
                        continue;

                    var lightData = pointLight.NodeData as GamePlay.Scene.UPointLightNode.ULightNodeData;

                    FPointLight light;
                    var pos = pointLight.Placement.Position;
                    light.PositionAndRadius = new Vector4(pos.ToSingleVector3(), lightData.Radius);
                    light.ColorAndIntensity = new Vector4(lightData.Color.X, lightData.Color.Y, lightData.Color.Z, lightData.Intensity);
                    pointLight.IndexInGpuScene = PointLights.PushData(light);
                }
                PointLights.Flush2GPU(cmd.mCoreObject);
            }
        }
    }
}
