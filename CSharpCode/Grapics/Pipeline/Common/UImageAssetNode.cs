using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Graphics.Pipeline.Common
{
    public class UImageAssetNode : URenderGraphNode
    {
        public Common.URenderGraphPin ImagePinOut = Common.URenderGraphPin.CreateOutput("Image", false, EPixelFormat.PXF_R8G8B8A8_UNORM);
        public UImageAssetNode()
        {
            Name = "ImageAssetNode";
        }
        public override void Cleanup()
        {
            ImageSrv = null;
            base.Cleanup();
        }
        public EPixelFormat ImageFormat 
        { 
            get
            {
                return ImagePinOut.Attachement.Format;
            }
        }
        public override void InitNodePins()
        {
            ImagePinOut.LifeMode = UAttachBuffer.ELifeMode.Imported;
            AddOutput(ImagePinOut, EGpuBufferViewType.GBVT_Srv | EGpuBufferViewType.GBVT_Uav);

            ImageName = RName.GetRName("texture/default_envmap.srv", RName.ERNameType.Engine);
        }
        public unsafe override void FrameBuild()
        {
            var attachement = RenderGraph.AttachmentCache.ImportAttachment(ImagePinOut);            
            attachement.Srv = ImageSrv;
        }
        public RHI.CShaderResourceView ImageSrv;
        [Rtti.Meta]
        public RName ImageName
        {
            get
            {
                if (ImageSrv == null)
                    return null;
                return ImageSrv.AssetName;
            }
            set
            {
                System.Action action = async () =>
                {
                    ImageSrv = await UEngine.Instance.GfxDevice.TextureManager.GetTexture(value);
                    ImagePinOut.Attachement.Format = ImageSrv.mCoreObject.GetFormat();
                    ImagePinOut.Attachement.Width = (uint)ImageSrv.mCoreObject.mTxDesc.Width;
                    ImagePinOut.Attachement.Height = (uint)ImageSrv.mCoreObject.mTxDesc.Height;
                };
                action();
            }
        }
    }
}
