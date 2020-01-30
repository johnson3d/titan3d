using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.IO;

namespace EngineNS
{
    public class BitmapProc
    {
        static int[,] Kernel = new int[2, 2]
	    {
		    { 70, 10},
		    { 10, 10},
	    };
        private static System.Drawing.Color GenerateOnePixel(System.Drawing.Bitmap src, int PosW, int PosH)
        {
            int Ob = 0;
            int Og = 0;
            int Or = 0;
            int Oa = 0;
            int SumKernel = 0;
            for (int h = 0; h < 2; h++)
            {
                int NowH = PosH * 2 + h;
                if (NowH > src.Height - 1)
                    continue;
                for (int w = 0; w < 2; w++)
                {
                    int NowW = PosW * 2 + w;
                    if (NowW > src.Width - 1)
                        continue;

                    var p = src.GetPixel(NowW, NowH);

                    Oa += (int)p.A * Kernel[h,w];
                    Ob += (int)p.B * Kernel[h,w];
                    Og += (int)p.G * Kernel[h,w];
                    Or += (int)p.R * Kernel[h,w];
                    SumKernel += Kernel[h,w];
                }
            }
            if (SumKernel == 0)
                return System.Drawing.Color.FromArgb(0);
            Oa /= SumKernel;
            Ob /= SumKernel;
            Og /= SumKernel;
            Or /= SumKernel;
            return System.Drawing.Color.FromArgb(Oa, Or, Og, Ob);
        }
        public static System.Drawing.Bitmap GenerateMip(System.Drawing.Bitmap src, int tarWidth, int tarHeight)
        {
            var bmp = new System.Drawing.Bitmap(tarWidth, tarHeight);
            for (int h = 0; h < tarHeight; h++)
            {
                for (int w = 0; w < tarWidth; w++)
                {
                    var clr = GenerateOnePixel(src, w, h);
                    bmp.SetPixel(w, h, clr);
                }
            }
            return bmp;
        }
        public static System.Drawing.Bitmap ScaleBitmap(System.Drawing.Bitmap src, int tarWidth, int tarHeight)
        {
            var bmp = new System.Drawing.Bitmap(tarWidth, tarHeight);
            var graph = System.Drawing.Graphics.FromImage(bmp);

            graph.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Bilinear;
            graph.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.GammaCorrected;
            graph.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            graph.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.None;

            //float width = tarWidth;
            //float height = tarHeight;
            //var brush = new SolidBrush(System.Drawing.Color.Black);
            //graph.FillRectangle(brush, new System.Drawing.RectangleF(0, 0, width, height));

            graph.DrawImage(src, new System.Drawing.Rectangle(0, 0, tarWidth, tarHeight));
            return bmp;
        }
        public static bool SaveTxPic(IO.XndHolder xnd, ref CTxPicDesc txDesc, string filename, 
            ETCFormat etcFormat = ETCFormat.RGBA8,
            int mipMapLevel = 0)
        {
            #region Read Pixels
            var imageimport = new EngineNS.Bricks.ImageImport.ImageImport();
            EngineNS.Support.CBlobObject blob = new EngineNS.Support.CBlobObject();
            imageimport.LoadTexture(filename, blob);
            int w = imageimport.GetWidth();
            int h = imageimport.GetHeight();
            int channels = imageimport.GetChannels();

            byte[] data = blob.ToBytes();//new byte[w * h * channels];
            System.Drawing.Imaging.PixelFormat format = System.Drawing.Imaging.PixelFormat.Undefined;
            if (channels == 4)
                format = System.Drawing.Imaging.PixelFormat.Format32bppArgb;
            else if (channels == 3)
                format = System.Drawing.Imaging.PixelFormat.Format24bppRgb;
            else if (channels == 1)
                format = System.Drawing.Imaging.PixelFormat.Format32bppArgb;

            var bitmap = new System.Drawing.Bitmap(w, h, format);
            int offset = 0;
            for (int j = 0; j < h; j++)
            {
                for (int i = 0; i < w; i++)
                {
                    if (channels == 4)
                    {
                        System.Drawing.Color color = System.Drawing.Color.FromArgb(data[offset + 3], data[offset], data[offset + 1], data[offset + 2]);
                        offset += 4;
                        bitmap.SetPixel(i, j, color);
                    }
                    else if (channels == 3)
                    {
                        System.Drawing.Color color = System.Drawing.Color.FromArgb(data[offset], data[offset + 1], data[offset + 2]);
                        offset += 3;
                        bitmap.SetPixel(i, j, color);
                    }
                    else if (channels == 1)
                    {
                        System.Drawing.Color color = System.Drawing.Color.FromArgb(data[offset++]);
                        bitmap.SetPixel(i, j, color);
                    }
                }
            }
            var tagBitmap = bitmap.Clone(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height), System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            #endregion

            if (bitmap.PixelFormat == System.Drawing.Imaging.PixelFormat.Format8bppIndexed)
            {
                txDesc.EtcFormat = ETCFormat.UNKNOWN;
            }
            txDesc.Width = w;
            txDesc.Height = h;

            SaveTxPic(xnd, ref txDesc, tagBitmap, etcFormat, mipMapLevel);
            return true;
        }
        public static void SaveTxPic(IO.XndHolder xnd, ref CTxPicDesc txDesc, Bitmap tagBitmap,
            ETCFormat etcFormat = ETCFormat.RGBA8,
            int mipMapLevel = 0)
        {
            #region Png
            var attr = xnd.Node.AddAttrib("PNG");
            attr.BeginWrite();
            var tagStream = new MemoryStream();
            tagBitmap.Save(tagStream, System.Drawing.Imaging.ImageFormat.Png);
            var pngData = tagStream.ToArray();
            attr.Write(pngData, pngData.Length);
            attr.EndWrite();
            #endregion
            
            #region PngMips
            var mipsNode = xnd.Node.AddNode("PngMips", 0, 0);
            int curMip = 0;
            while (true)
            {
                var mipAttr = mipsNode.AddAttrib($"Mip_{curMip}");
                curMip++;
                mipAttr.BeginWrite();
                var mipStream = new MemoryStream();
                tagBitmap.Save(mipStream, System.Drawing.Imaging.ImageFormat.Png);
                var pngMipData = mipStream.ToArray();
                mipAttr.Write(pngMipData, pngMipData.Length);
                mipAttr.EndWrite();

                if (txDesc.MipLevel == curMip)
                {
                    break;
                }
                if (tagBitmap.Width == 1 && tagBitmap.Height == 1)
                    break;
                var mipWidth = tagBitmap.Width / 2;
                var mipHeight = tagBitmap.Height / 2;
                if (mipWidth == 0)
                    mipWidth = 1;
                if (mipHeight == 0)
                    mipHeight = 1;
                //tagBitmap = EngineNS.BitmapProc.ScaleBitmap(tagBitmap, mipWidth, mipHeight);
                tagBitmap = EngineNS.BitmapProc.GenerateMip(tagBitmap, mipWidth, mipHeight);
            }
            #endregion

            if (CEngine.IsWriteEtc)
            {
                if (etcFormat != ETCFormat.UNKNOWN)
                {
                    using (var etcBlob = EngineNS.Support.CBlobProxy2.CreateBlobProxy())
                    {
                        unsafe
                        {
                            fixed (byte* dataPtr = &pngData[0])
                            {
                                var texCompressor = new EngineNS.Bricks.TexCompressor.CTexCompressor();
                                texCompressor.EncodePng2ETC((IntPtr)dataPtr, (uint)pngData.Length, etcFormat, mipMapLevel, etcBlob);
                                etcBlob.BeginRead();
                            }
                        }
                        if (etcBlob.DataLength >= 0)
                        {
                            var etcNode = xnd.Node.AddNode("EtcMips", 0, 0);
                            int fmt = 0;
                            int MipLevel = 0;
                            etcBlob.Read(out fmt);
                            etcBlob.Read(out MipLevel);
                            var layer = new EngineNS.Bricks.TexCompressor.ETCLayer();
                            for (int i = 0; i < MipLevel; i++)
                            {
                                etcBlob.Read(out layer);
                                byte[] etcMipData;
                                etcBlob.Read(out etcMipData, (int)layer.Size);

                                var mipAttr = etcNode.AddAttrib($"Mip_{i}");
                                mipAttr.BeginWrite();
                                mipAttr.Write(layer);
                                mipAttr.Write(etcMipData, etcMipData.Length);
                                mipAttr.EndWrite();
                            }
                        }
                    }
                }
            }

            attr = xnd.Node.AddAttrib("Desc");
            txDesc.MipLevel = curMip;
            txDesc.EtcFormat = etcFormat;
            #region Desc
            attr.Version = 3;
            attr.BeginWrite();
            unsafe
            {
                fixed (EngineNS.CTxPicDesc* descPin = &txDesc)
                {
                    attr.Write((IntPtr)(descPin), sizeof(EngineNS.CTxPicDesc));
                }
            }
            attr.EndWrite();
            #endregion
        }
        public static System.Drawing.Bitmap LoadPngBitmapFromTxPic(IO.XndHolder xnd)
        {   
            #region Read Png
            var attr = xnd.Node.FindAttrib("PNG");
            byte[] bytes = null;
            attr.BeginRead();
            attr.Read(out bytes, (int)attr.Length);
            attr.EndRead();

            var stream = new System.IO.MemoryStream(bytes);
            return new Bitmap(stream);
            #endregion
        }

        public static bool RefreshTxPic(string fileName)
        {
            var xnd = EngineNS.IO.XndHolder.SyncLoadXND(fileName);
            if (xnd == null)
                return false;

            CTxPicDesc txDesc = new CTxPicDesc();
            txDesc.SetDefault();

            #region Desc
            var attr = xnd.Node.FindAttrib("Desc");
            attr.BeginRead();
            unsafe
            {
                if (attr.Version == 3)
                {
                    attr.Read((IntPtr)(&txDesc), sizeof(CTxPicDesc));
                }
                else if (attr.Version == 2)
                {
                    attr.Read(out txDesc.sRGB);
                }
            }
            attr.EndRead();
            #endregion

            var pngBitmap = LoadPngBitmapFromTxPic(xnd);
            
            txDesc.Width = pngBitmap.Width;
            txDesc.Height = pngBitmap.Height;

            var saveXnd = EngineNS.IO.XndHolder.NewXNDHolder();
            SaveTxPic(saveXnd, ref txDesc, pngBitmap, txDesc.EtcFormat, txDesc.MipLevel);

            EngineNS.IO.XndHolder.SaveXND(fileName, saveXnd);
            return true;
        }
        public static void RefreshAllTxPic()
        {
            {
                var allpic = CEngine.Instance.FileManager.GetFiles(CEngine.Instance.FileManager.EngineRoot, "*.txpic");
                int remain = allpic.Count;
                foreach (var i in allpic)
                {
                    RefreshTxPic(i);
                    System.Diagnostics.Debug.WriteLine($"Engine TxPix Remain = {--remain}");
                }
            }
            {
                var allpic = CEngine.Instance.FileManager.GetFiles(CEngine.Instance.FileManager.ProjectRoot, "*.txpic");
                int remain = allpic.Count;
                foreach (var i in allpic)
                {
                    RefreshTxPic(i);
                    System.Diagnostics.Debug.WriteLine($"Project TxPix Remain = {--remain}");
                }
            }
        }
    }
}
