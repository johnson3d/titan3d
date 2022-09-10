using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Editor
{
    public class USnapshot
    {
        public NxRHI.USrView mTextureRSV;
        public unsafe static void Save(RName rname, IO.IAssetMeta ameta, NxRHI.ITexture tex, uint x, uint y, uint w, uint h)
        {
            var file = rname.Address + ".snap";
            var rc = UEngine.Instance.GfxDevice.RenderContext;
            var fenceDesc = new NxRHI.FFenceDesc();
            fenceDesc.m_InitValue = 0;
            var fence = rc.CreateFence(in fenceDesc, file);
            var cpuDesc = tex.Desc;
            cpuDesc.CpuAccess = NxRHI.ECpuAccess.CAS_READ;
            cpuDesc.Usage = NxRHI.EGpuUsage.USAGE_STAGING;
            cpuDesc.BindFlags = (NxRHI.EBufferType)0;
            cpuDesc.InitData = (NxRHI.FMappedSubResource*)IntPtr.Zero.ToPointer();

            NxRHI.FSubResourceFootPrint CopyBufferFootPrint = new NxRHI.FSubResourceFootPrint();
            CopyBufferFootPrint.Width = cpuDesc.Width;
            CopyBufferFootPrint.Height = cpuDesc.Height;
            CopyBufferFootPrint.Depth = 1;
            CopyBufferFootPrint.Format = cpuDesc.Format;
            CopyBufferFootPrint.RowPitch = (uint)CoreSDK.GetPixelFormatByteWidth(CopyBufferFootPrint.Format) * cpuDesc.Width;
            var pAlignment = UEngine.Instance.GfxDevice.RenderContext.mCoreObject.GetGpuResourceAlignment();
            if (CopyBufferFootPrint.RowPitch % pAlignment->TexturePitchAlignment > 0)
            {
                CopyBufferFootPrint.RowPitch = (CopyBufferFootPrint.RowPitch / pAlignment->TexturePitchAlignment + 1) * pAlignment->TexturePitchAlignment;
            }
            var texture = UEngine.Instance.GfxDevice.RenderContext.CreateTextureToCpuBuffer(in cpuDesc, in CopyBufferFootPrint);

            UEngine.Instance.GfxDevice.RenderCmdQueue.QueueCmd((im_cmd, name) =>
            {
                var dstTex = texture as NxRHI.UTexture;
                var dstBf = texture as NxRHI.UBuffer;
                if (dstTex != null)
                {
                    var box = new NxRHI.FSubresourceBox();
                    im_cmd.CopyTextureRegion(dstTex.mCoreObject, 0, 0,0,0, tex, 0, in box);
                }
                else if (dstBf != null)
                {
                    im_cmd.CopyTextureToBuffer(dstBf.mCoreObject, in CopyBufferFootPrint, tex, 0);
                }
                UEngine.Instance.GfxDevice.RenderContext.CmdQueue.SignalFence(fence, 1);
            }, "Copy Snap Texture");

            UEngine.Instance.EventPoster.RunOn(() =>
            {
                fence.Wait(1);
                UEngine.Instance.GfxDevice.RenderCmdQueue.QueueCmd((im_cmd, name) =>
                {
                    var gpuDataBlob = new Support.CBlobObject();
                    var bufferData = new Support.CBlobObject();
                    texture.GetGpuBufferDataPointer().FetchGpuData(im_cmd, 0, gpuDataBlob.mCoreObject);
                    NxRHI.ITexture.BuildImage2DBlob(bufferData.mCoreObject, gpuDataBlob.mCoreObject, cpuDesc);
                    UEngine.Instance.EventPoster.RunOn(() =>
                    {
                        SavePng(ameta, file, bufferData);
                        return null;
                    }, Thread.Async.EAsyncTarget.AsyncEditor);
                }, "Fetch");
                return null;
            }, Thread.Async.EAsyncTarget.AsyncEditor);
        }
        public unsafe static void Save(RName rname, IO.IAssetMeta ameta, NxRHI.USrView srv)
        {
            var file = rname.Address + ".snap";
            var rc = UEngine.Instance.GfxDevice.RenderContext;
            var fenceDesc = new NxRHI.FFenceDesc();
            fenceDesc.m_InitValue = 0;
            var fence = rc.CreateFence(in fenceDesc, file);
            var cpuDesc = srv.mCoreObject.GetBufferAsTexture().Desc;
            cpuDesc.CpuAccess = NxRHI.ECpuAccess.CAS_READ;
            cpuDesc.Usage = NxRHI.EGpuUsage.USAGE_STAGING;
            cpuDesc.BindFlags = (NxRHI.EBufferType)0;

            NxRHI.FSubResourceFootPrint CopyBufferFootPrint = new NxRHI.FSubResourceFootPrint();
            CopyBufferFootPrint.Width = cpuDesc.Width;
            CopyBufferFootPrint.Height = cpuDesc.Height;
            CopyBufferFootPrint.Depth = 1;
            CopyBufferFootPrint.Format = cpuDesc.Format;
            CopyBufferFootPrint.RowPitch = (uint)CoreSDK.GetPixelFormatByteWidth(CopyBufferFootPrint.Format) * cpuDesc.Width;
            var pAlignment = UEngine.Instance.GfxDevice.RenderContext.mCoreObject.GetGpuResourceAlignment();
            if (CopyBufferFootPrint.RowPitch % pAlignment->TexturePitchAlignment > 0)
            {
                CopyBufferFootPrint.RowPitch = (CopyBufferFootPrint.RowPitch / pAlignment->TexturePitchAlignment + 1) * pAlignment->TexturePitchAlignment;
            }
            var texture = UEngine.Instance.GfxDevice.RenderContext.CreateTextureToCpuBuffer(in cpuDesc, in CopyBufferFootPrint);

            UEngine.Instance.GfxDevice.RenderCmdQueue.QueueCmd((im_cmd, name) =>
            {
                var dstTex = texture as NxRHI.UTexture;
                var dstBf = texture as NxRHI.UBuffer;
                if (dstTex != null)
                {
                    var box = new NxRHI.FSubresourceBox();
                    im_cmd.CopyTextureRegion(dstTex.mCoreObject, 0, 0, 0, 0, srv.mCoreObject.GetBufferAsTexture(), 0, in box);
                }
                else if (dstBf != null)
                {
                    im_cmd.CopyTextureToBuffer(dstBf.mCoreObject, in CopyBufferFootPrint, srv.mCoreObject.GetBufferAsTexture(), 0);
                }
                UEngine.Instance.GfxDevice.RenderContext.CmdQueue.SignalFence(fence, 1);
            }, "Copy Snap SRV");

            UEngine.Instance.EventPoster.RunOn(() =>
            {
                fence.Wait(1);
                UEngine.Instance.GfxDevice.RenderCmdQueue.QueueCmd((im_cmd, name) =>
                {
                    var gpuDataBlob = new Support.CBlobObject();
                    var bufferData = new Support.CBlobObject();
                    texture.GetGpuBufferDataPointer().FetchGpuData(im_cmd, 0, gpuDataBlob.mCoreObject);
                    NxRHI.ITexture.BuildImage2DBlob(bufferData.mCoreObject, gpuDataBlob.mCoreObject, cpuDesc);
                    UEngine.Instance.EventPoster.RunOn(() =>
                    {
                        SavePng(ameta, file, bufferData);
                        return null;
                    }, Thread.Async.EAsyncTarget.AsyncEditor);
                }, "Fetch");
                return null;
            }, Thread.Async.EAsyncTarget.AsyncEditor);
        }
        public unsafe static bool SavePng(IO.IAssetMeta ameta, string file, Support.CBlobObject bufferData)
        {
            byte* pPixelData = (byte*)bufferData.mCoreObject.GetData();
            if (pPixelData == (byte*)0)
                return false;
            var pBitmapDesc = (NxRHI.FPixelDesc*)pPixelData;
            pPixelData += sizeof(NxRHI.FPixelDesc);

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
            var pBitmapDesc = (NxRHI.FPixelDesc*)pPixelData;
            pPixelData += sizeof(NxRHI.FPixelDesc);

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
