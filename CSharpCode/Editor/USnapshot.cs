using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Editor
{
    public class USnapshot
    {
        public RHI.CShaderResourceView mTextureRSV;
        public unsafe static void Save(string file, RHI.CShaderResourceView srv, ICommandList cmdlist_hp)
        {
            var rc = UEngine.Instance.GfxDevice.RenderContext;
            var fence = rc.CreateFence();
            ITexture2D* pTexture = (ITexture2D*)0;
            cmdlist_hp.CreateReadableTexture2D((ITexture2D**)&pTexture, srv.mCoreObject, new IFrameBuffers());
            cmdlist_hp.Signal(fence.mCoreObject, 0);
            var texture = new ITexture2D(pTexture);
            UEngine.Instance.GfxDevice.RegFenceQuery(fence, (arg) =>
            {
                void* pData;
                uint rowPitch;
                uint depthPitch;                
                if (texture.Map(cmdlist_hp, 0, &pData, &rowPitch, &depthPitch) != 0)
                {
                    //texture.BuildImageBlob(mHitProxyData.mCoreObject, pData, rowPitch);
                    texture.Unmap(cmdlist_hp, 0);
                }
            });           
            
        }
        public static USnapshot Load(string file)
        {
            return null;
        }
    }
}
