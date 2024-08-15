using EngineNS.NxRHI;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Editor
{
    public class USnapshot
    {
        public NxRHI.USrView mTextureRSV;
        public enum ESnapSide
        {
            Center,
            Left,
            Right,
        }
        public unsafe static void Save(RName rname, IO.IAssetMeta ameta, NxRHI.ITexture tex, uint x, uint y, uint w, uint h, ESnapSide side = ESnapSide.Center)
        {
            var file = rname.Address + ".snap";
            var rc = TtEngine.Instance.GfxDevice.RenderContext;
            var fenceDesc = new NxRHI.FFenceDesc();
            fenceDesc.m_InitValue = 0;
            var fence = rc.CreateFence(in fenceDesc, file);
            var cpDraw = TtEngine.Instance.GfxDevice.RenderContext.CreateCopyDraw();
            var readable = tex.CreateReadable(TtEngine.Instance.GfxDevice.RenderContext.mCoreObject, 0, cpDraw.mCoreObject);
            TtEngine.Instance.GfxDevice.RenderSwapQueue.QueueCmd((NxRHI.ICommandList im_cmd, ref NxRHI.FRCmdInfo info) =>
            {
                im_cmd.PushGpuDraw(cpDraw.mCoreObject.NativeSuper);
                im_cmd.FlushDraws();
                TtEngine.Instance.GfxDevice.RenderContext.GpuQueue.IncreaseSignal(fence);
            }, "Copy Snap Texture");

            TtEngine.Instance.EventPoster.RunOn((state) =>
            {
                fence.Wait(1);
                TtEngine.Instance.GfxDevice.RenderSwapQueue.QueueCmd((NxRHI.ICommandList im_cmd, ref NxRHI.FRCmdInfo info) =>
                {
                    var gpuDataBlob = new Support.UBlobObject();
                    var bufferData = new Support.UBlobObject();
                    readable.FetchGpuData(0, gpuDataBlob.mCoreObject);
                    NxRHI.ITexture.BuildImage2DBlob(bufferData.mCoreObject, gpuDataBlob.mCoreObject, tex.Desc);
                    TtEngine.Instance.EventPoster.RunOn((state) =>
                    {
                        SavePng(ameta, file, bufferData, side);
                        return true;
                    }, Thread.Async.EAsyncTarget.AsyncEditor);
                }, "Fetch");
                return true;
            }, Thread.Async.EAsyncTarget.AsyncEditor);
        }
        public unsafe static void Save(RName rname, IO.IAssetMeta ameta, NxRHI.USrView srv)
        {
            uint w = srv.GetTexture().Desc.Width;
            uint h = srv.GetTexture().Desc.Height;
            Save(rname, ameta, srv.GetTexture(),0,0,w,h);
        }
        public unsafe static bool SavePng(IO.IAssetMeta ameta, string file, Support.UBlobObject bufferData, ESnapSide side = ESnapSide.Center)
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
                switch (side)
                {
                    case ESnapSide.Center:
                        image = StbImageSharp.ImageProcessor.GetCenterSquare(image);
                        break;
                    case ESnapSide.Left:
                        image = StbImageSharp.ImageProcessor.GetCenterLeft(image);
                        break;
                    case ESnapSide.Right:
                        image = StbImageSharp.ImageProcessor.GetCenterRight(image);
                        break;
                    default:
                        image = StbImageSharp.ImageProcessor.GetCenterSquare(image);
                        break;
                }
                
                if (pBitmapDesc->Width > 128)
                {
                    float rate = 128 / (float)image.Width;// pBitmapDesc->Width;
                    int height = (int)((float)image.Height * rate);
                    image = StbImageSharp.ImageProcessor.GetBoxDownSampler(image, 128, height);
                }

                writer.WritePng(image.Data, image.Width, image.Height, StbImageWriteSharp.ColorComponents.RedGreenBlueAlpha, memStream);
                ameta.ResetSnapshot();
            }
            TtEngine.Instance.SourceControlModule.AddFile(file);
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
            result.mTextureRSV = await TtEngine.Instance.GfxDevice.TextureManager.CreateTexture(file);
            if (result.mTextureRSV == null)
                return null;
            return result;
        }
    }
}
