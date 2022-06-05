using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Editor
{
    public class USnapshot
    {
        public RHI.CShaderResourceView mTextureRSV;
        public unsafe static void Save(RName rname, IO.IAssetMeta ameta, ITexture2D tex, ICommandList cmdlist_hp, uint x, uint y, uint w, uint h)
        {
            var file = rname.Address + ".snap";
            var rc = UEngine.Instance.GfxDevice.RenderContext;
            var fence = rc.CreateFence();
            ITexture2D* pTexture = (ITexture2D*)0;
            cmdlist_hp.CreateReadableTexture2D((ITexture2D**)&pTexture, tex, new IFrameBuffers());
            cmdlist_hp.Signal(fence.mCoreObject, 0);
            var texture = new ITexture2D(pTexture);
            UEngine.Instance.GfxDevice.RegFenceQuery(fence, (arg) =>
            {
                void* pData;
                uint rowPitch;
                uint depthPitch;
                if (texture.MapMipmap(cmdlist_hp, 0, 0, &pData, &rowPitch, &depthPitch) != 0)
                {
                    var bufferData = new Support.CBlobObject();
                    texture.BuildImageBlob(bufferData.mCoreObject, pData, rowPitch);
                    texture.UnmapMipmap(cmdlist_hp, 0, 0);

                    SavePng(ameta, file, bufferData, w, h, x, y);
                }
                texture.NativeSuper.NativeSuper.NativeSuper.Release();
            });
        }
        public unsafe static void Save(RName rname, IO.IAssetMeta ameta, RHI.CShaderResourceView srv, ICommandList cmdlist_hp)
        {
            var file = rname.Address + ".snap";
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
                if (texture.MapMipmap(cmdlist_hp, 0, 0, &pData, &rowPitch, &depthPitch) != 0)
                {
                    var bufferData = new Support.CBlobObject();
                    texture.BuildImageBlob(bufferData.mCoreObject, pData, rowPitch);
                    texture.UnmapMipmap(cmdlist_hp, 0, 0);

                    SavePng(ameta, file, bufferData);
                }
                texture.NativeSuper.NativeSuper.NativeSuper.Release();
            });
        }
        public unsafe static bool SavePng(IO.IAssetMeta ameta, string file, Support.CBlobObject bufferData)
        {
            byte* pPixelData = (byte*)bufferData.mCoreObject.GetData();
            if (pPixelData == (byte*)0)
                return false;
            var pBitmapDesc = (PixelDesc*)pPixelData;
            pPixelData += sizeof(PixelDesc);

            using (var memStream = new System.IO.FileStream(file, System.IO.FileMode.OpenOrCreate))// .MemoryStream(pBitmapDesc->Stride * pBitmapDesc->Height))
            {
                var writer = new StbImageWriteSharp.ImageWriter();
                var image = StbImageSharp.ImageResult.FromResult(pPixelData, (int)pBitmapDesc->Width, (int)pBitmapDesc->Height, StbImageSharp.ColorComponents.RedGreenBlueAlpha, StbImageSharp.ColorComponents.RedGreenBlueAlpha);
                if (pBitmapDesc->Width > 128)
                {
                    float rate = 128 / (float)pBitmapDesc->Width;
                    int height = (int)((float)pBitmapDesc->Height * rate);
                    image = StbImageSharp.ImageProcessor.GetBoxDownSampler(image, 128, height);
                }

                writer.WritePng(image.Data, image.Width, image.Height, StbImageWriteSharp.ColorComponents.RedGreenBlueAlpha, memStream);
                ameta.ResetSnapshot();
            }
            return true;
        }
        public unsafe static bool SavePng(IO.IAssetMeta ameta, string file, Support.CBlobObject bufferData, uint TarW, uint TarH, uint SrcX, uint SrcY)
        {
            byte* pPixelData = (byte*)bufferData.mCoreObject.GetData();
            if (pPixelData == (byte*)0)
                return false;
            var pBitmapDesc = (PixelDesc*)pPixelData;
            pPixelData += sizeof(PixelDesc);

            using (var memStream = new System.IO.FileStream(file, System.IO.FileMode.OpenOrCreate))// .MemoryStream(pBitmapDesc->Stride * pBitmapDesc->Height))
            {
                var writer = new StbImageWriteSharp.ImageWriter();
                var image = StbImageSharp.ImageResult.FromResult(pPixelData, (int)pBitmapDesc->Width, (int)pBitmapDesc->Height, StbImageSharp.ColorComponents.RedGreenBlueAlpha, StbImageSharp.ColorComponents.RedGreenBlueAlpha);
                image = StbImageSharp.ImageProcessor.StretchBlt(TarW, TarH, image, SrcX, SrcY, TarW, TarH);

                if (pBitmapDesc->Width > 128)
                {
                    float rate = 128 / (float)pBitmapDesc->Width;
                    int height = (int)((float)pBitmapDesc->Height * rate);
                    image = StbImageSharp.ImageProcessor.GetBoxDownSampler(image, 128, height);
                }

                writer.WritePng(image.Data, image.Width, image.Height, StbImageWriteSharp.ColorComponents.RedGreenBlueAlpha, memStream);
                ameta.ResetSnapshot();
            }
            return true;
        }
        public static async System.Threading.Tasks.Task<USnapshot> Load(string file)
        {
            USnapshot result = new USnapshot();
            result.mTextureRSV = await UEngine.Instance.GfxDevice.TextureManager.CreateTexture(file);
            if (result.mTextureRSV == null)
                return null;
            return result;
        }
    }
}
