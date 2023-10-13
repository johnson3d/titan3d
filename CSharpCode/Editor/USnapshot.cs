using EngineNS.NxRHI;
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

            UEngine.Instance.GfxDevice.RenderCmdQueue.QueueCmd((NxRHI.ICommandList im_cmd, ref NxRHI.URenderCmdQueue.FRCmdInfo info) =>
            {
                var cpDraw = UEngine.Instance.GfxDevice.RenderContext.CreateCopyDraw();
                var dstTex = texture as NxRHI.UTexture;
                var dstBf = texture as NxRHI.UBuffer;
                if (dstTex != null)
                {
                    //var box = new NxRHI.FSubresourceBox();
                    //box.Left = x;
                    //box.Right = x + cpuDesc.Width;
                    //box.Top = y;
                    //box.Bottom = y + cpuDesc.Height;
                    //box.Front = 0;
                    //box.Back = 1;
                    //im_cmd.CopyTextureRegion(dstTex.mCoreObject, 0, 0, 0, 0, tex, 0, in box);// (NxRHI.FSubresourceBox*)IntPtr.Zero.ToPointer());

                    //im_cmd.CopyTextureRegion(dstTex.mCoreObject, 0, 0, 0, 0, tex, 0, (NxRHI.FSubresourceBox*)IntPtr.Zero.ToPointer());
                    cpDraw.mCoreObject.Mode = ECopyDrawMode.CDM_Texture2Texture;
                    cpDraw.BindTextureDest(dstTex);
                    cpDraw.mCoreObject.BindTextureSrc(tex);
                    im_cmd.PushGpuDraw(cpDraw.mCoreObject.NativeSuper);
                    im_cmd.FlushDraws();
                }
                else if (dstBf != null)
                {
                    //im_cmd.CopyTextureToBuffer(dstBf.mCoreObject, in CopyBufferFootPrint, tex, 0);

                    cpDraw.mCoreObject.Mode = ECopyDrawMode.CDM_Texture2Buffer;
                    cpDraw.BindBufferDest(dstBf);
                    cpDraw.mCoreObject.BindTextureSrc(tex);
                    cpDraw.mCoreObject.FootPrint = CopyBufferFootPrint;
                    im_cmd.PushGpuDraw(cpDraw.mCoreObject.NativeSuper);
                    im_cmd.FlushDraws();
                }
                UEngine.Instance.GfxDevice.RenderContext.GpuQueue.IncreaseSignal(fence);
            }, "Copy Snap Texture");

            UEngine.Instance.EventPoster.RunOn((state) =>
            {
                fence.Wait(1);
                UEngine.Instance.GfxDevice.RenderCmdQueue.QueueCmd((NxRHI.ICommandList im_cmd, ref NxRHI.URenderCmdQueue.FRCmdInfo info) =>
                {
                    var gpuDataBlob = new Support.UBlobObject();
                    var bufferData = new Support.UBlobObject();
                    texture.GetGpuBufferDataPointer().FetchGpuData(0, gpuDataBlob.mCoreObject);
                    NxRHI.ITexture.BuildImage2DBlob(bufferData.mCoreObject, gpuDataBlob.mCoreObject, cpuDesc);
                    UEngine.Instance.EventPoster.RunOn((state) =>
                    {
                        SavePng(ameta, file, bufferData);
                        return true;
                    }, Thread.Async.EAsyncTarget.AsyncEditor);
                }, "Fetch");
                return true;
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

            UEngine.Instance.GfxDevice.RenderCmdQueue.QueueCmd((NxRHI.ICommandList im_cmd, ref NxRHI.URenderCmdQueue.FRCmdInfo info) =>
            {
                var dstTex = texture as NxRHI.UTexture;
                var dstBf = texture as NxRHI.UBuffer;
                if (dstTex != null)
                {
                    im_cmd.CopyTextureRegion(dstTex.mCoreObject, 0, 0, 0, 0, srv.mCoreObject.GetBufferAsTexture(), 0, (NxRHI.FSubresourceBox*)IntPtr.Zero.ToPointer());
                }
                else if (dstBf != null)
                {
                    im_cmd.CopyTextureToBuffer(dstBf.mCoreObject, in CopyBufferFootPrint, srv.mCoreObject.GetBufferAsTexture(), 0);
                }
                UEngine.Instance.GfxDevice.RenderContext.GpuQueue.IncreaseSignal(fence);
            }, "Copy Snap SRV");

            UEngine.Instance.EventPoster.RunOn((state) =>
            {
                fence.Wait(1);
                UEngine.Instance.GfxDevice.RenderCmdQueue.QueueCmd((NxRHI.ICommandList im_cmd, ref NxRHI.URenderCmdQueue.FRCmdInfo info) =>
                {
                    var gpuDataBlob = new Support.UBlobObject();
                    var bufferData = new Support.UBlobObject();
                    texture.GetGpuBufferDataPointer().FetchGpuData(0, gpuDataBlob.mCoreObject);
                    NxRHI.ITexture.BuildImage2DBlob(bufferData.mCoreObject, gpuDataBlob.mCoreObject, cpuDesc);
                    UEngine.Instance.EventPoster.RunOn((state) =>
                    {
                        SavePng(ameta, file, bufferData);
                        return true;
                    }, Thread.Async.EAsyncTarget.AsyncEditor);
                }, "Fetch");
                return true;
            }, Thread.Async.EAsyncTarget.AsyncEditor);
        }
        public unsafe static bool SavePng(IO.IAssetMeta ameta, string file, Support.UBlobObject bufferData)
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
        public unsafe static bool SavePng(IO.IAssetMeta ameta, string file, Support.UBlobObject bufferData, uint TarW, uint TarH, uint SrcX, uint SrcY)
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
