using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows;
using System.ComponentModel;

namespace EditorCommon
{
    [EngineNS.Rtti.MetaClass]
    public class TextureDesc : EngineNS.IO.Serializer.Serializer, INotifyPropertyChanged
    {
        #region INotifyPropertyChangedMembers
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
        }
        #endregion

        public EngineNS.CTxPicDesc PicDesc = new EngineNS.CTxPicDesc();
        public byte[] RawData;

        [EngineNS.Rtti.MetaData]
        public bool sRGB
        {
            get => PicDesc.sRGB != 0;
            set
            {
                PicDesc.sRGB = value ? 1 : 0;
                OnPropertyChanged("sRGB");
            }
        }

        EngineNS.ETCFormat mETCFormat = EngineNS.ETCFormat.RGBA8;
        [EngineNS.Rtti.MetaData]
        public EngineNS.ETCFormat ETCFormat
        {
            get => PicDesc.EtcFormat;
            set
            {
                PicDesc.EtcFormat = value;
                OnPropertyChanged("ETCFormat");
            }
        }
        [EngineNS.Rtti.MetaData]
        public int MipMapLevel
        {
            get => PicDesc.MipLevel;
            set
            {
                PicDesc.MipLevel = value;
                OnPropertyChanged("MipMapLevel");
            }
        }

        //bool mUseEtcCompress = true;
        //[EngineNS.Rtti.MetaData]
        //public bool UseEtcCompress
        //{
        //    get => mUseEtcCompress;
        //    set
        //    {
        //        mUseEtcCompress = value;
        //        OnPropertyChanged("UseEtcCompress");
        //    }
        //}
    }

    public class ImageInit
    {
        public static ImageSource[] SyncGetImage(string strFullPath)
        {
            ImageSource image = null;
            try
            {
                if (!string.IsNullOrEmpty(strFullPath))
                {
                    // 判断扩展名
                    int nExtIdx = strFullPath.LastIndexOf('.');
                    string strExt = strFullPath.Substring(nExtIdx).ToLower();
                    switch (strExt)
                    {
                        case ".dds":
                        case ".tga":
                            image = null;//EditorCommon.DDS.DDSLoader.LoadDDS(strFullPath);
                            //image = FrameSet.Assist.DDSConverter.Convert(strFullPath);
                            break;
                        case ".txpic":
                            {
                                var xnd = EngineNS.IO.XndHolder.SyncLoadXND(strFullPath);
                                if (xnd != null)
                                {
                                    var attr = xnd.Node.FindAttrib("PNG");
                                    if (attr == null)
                                        return null;
                                    attr.BeginRead();
                                    byte[] rawData;
                                    attr.Read(out rawData, (int)attr.Length);
                                    attr.EndRead();
                                    if (rawData != null && rawData.Length > 0)
                                    {
                                        var bitMap = new BitmapImage();
                                        bitMap.BeginInit();
                                        bitMap.StreamSource = new System.IO.MemoryStream(rawData);
                                        bitMap.EndInit();
                                        bitMap.CacheOption = BitmapCacheOption.OnLoad;
                                        image = bitMap;
                                    }
                                }
                            }
                            break;
                        case ".snap":
                            {
                                var imgs = EngineNS.CShaderResourceView.LoadSnap(strFullPath);
                                if (imgs == null)
                                    return null;
                                var results = new ImageSource[imgs.Length];
                                for (int i = 0; i < imgs.Length; i++)
                                {
                                    var bitMap = new BitmapImage();
                                    bitMap.BeginInit();
                                    bitMap.StreamSource = imgs[i];
                                    bitMap.EndInit();
                                    bitMap.CacheOption = BitmapCacheOption.OnLoad;
                                    results[i] = bitMap;
                                    //image = bitMap;
                                }
                                return results;
                            }
                        default:
                            if (System.IO.File.Exists(strFullPath))
                            {
                                byte[] bytes = null;

                                try
                                {
                                    using (var binReader = new System.IO.BinaryReader(System.IO.File.Open(strFullPath, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Delete | System.IO.FileShare.Read)))
                                    {
                                        System.IO.FileInfo fi = new System.IO.FileInfo(strFullPath);
                                        bytes = binReader.ReadBytes((int)fi.Length);
                                        //image = new BitmapImage(new Uri(strFullPath));
                                    }
                                }
                                catch (Exception ex)
                                {
                                    EngineNS.Profiler.Log.WriteException(ex);
                                    bytes = null;
                                }
                                if (bytes != null && bytes.Length > 0)
                                {
                                    var bitMap = new BitmapImage();
                                    bitMap.BeginInit();
                                    bitMap.StreamSource = new System.IO.MemoryStream(bytes);
                                    bitMap.EndInit();
                                    bitMap.CacheOption = BitmapCacheOption.OnLoad;
                                    image = bitMap;
                                }
                            }
                            break;
                    }
                }
            }
            catch (System.Exception e)
            {
                var msg = "ImageInit.GetImage: " + e.Message;
                System.Diagnostics.Debug.WriteLine(msg);
            }

            if (image == null)
                return null;
            return new ImageSource[] { image };
        }
        public static async System.Threading.Tasks.Task<ImageSource[]> GetImage(string strFullPath)
        {
            ImageSource image = null;

            try
            {
                if (!string.IsNullOrEmpty(strFullPath))
                {
                    // 判断扩展名
                    int nExtIdx = strFullPath.LastIndexOf('.');
                    string strExt = strFullPath.Substring(nExtIdx).ToLower();
                    switch (strExt)
                    {
                        case ".dds":
                        case ".tga":
                            image = null;//EditorCommon.DDS.DDSLoader.LoadDDS(strFullPath);
                            //image = FrameSet.Assist.DDSConverter.Convert(strFullPath);
                            break;
                        case ".txpic":
                            {
                                var xnd = await EngineNS.IO.XndHolder.LoadXND(strFullPath);
                                if (xnd != null)
                                {
                                    byte[] bytes = await EngineNS.CEngine.Instance.EventPoster.Post(() =>
                                    {
                                        var attr = xnd.Node.FindAttrib("PNG");
                                        if (attr == null)
                                            return null;
                                        attr.BeginRead();
                                        byte[] rawData;
                                        attr.Read(out rawData, (int)attr.Length);
                                        attr.EndRead();
                                        return rawData;
                                    });
                                    if (bytes != null && bytes.Length > 0)
                                    {
                                        var bitMap = new BitmapImage();
                                        bitMap.BeginInit();
                                        bitMap.StreamSource = new System.IO.MemoryStream(bytes);
                                        bitMap.EndInit();
                                        bitMap.CacheOption = BitmapCacheOption.OnLoad;
                                        image = bitMap;
                                    }
                                }
                            }
                            break;
                        case ".snap":
                            {
                                var imgs = await EngineNS.CEngine.Instance.EventPoster.Post(() =>
                                {
                                    return EngineNS.CShaderResourceView.LoadSnap(strFullPath);
                                }, EngineNS.Thread.Async.EAsyncTarget.AsyncIO);
                                if (imgs == null)
                                    return null;
                                var results = new ImageSource[imgs.Length];
                                for (int i = 0; i < imgs.Length; i++)
                                {
                                    try
                                    {
                                        var bitMap = new BitmapImage();
                                        bitMap.BeginInit();
                                        bitMap.StreamSource = imgs[i];
                                        bitMap.EndInit();
                                        bitMap.CacheOption = BitmapCacheOption.OnLoad;
                                        results[i] = bitMap;
                                        //image = bitMap;
                                    }
                                    catch(Exception ex)
                                    {
                                        EngineNS.Profiler.Log.WriteException(ex);
                                        EngineNS.Profiler.Log.WriteLine(EngineNS.Profiler.ELogTag.Warning, "Editor", $"Snap {strFullPath} is invalid");
                                        return null;
                                    }
                                }
                                return results;
                            }
                        default:
                            if (System.IO.File.Exists(strFullPath))
                            {
                                byte[] bytes = null;
                                await EngineNS.CEngine.Instance.EventPoster.Post(() =>
                                {
                                    try
                                    {
                                        using (var binReader = new System.IO.BinaryReader(System.IO.File.Open(strFullPath, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Delete | System.IO.FileShare.Read)))
                                        {
                                            System.IO.FileInfo fi = new System.IO.FileInfo(strFullPath);
                                            bytes = binReader.ReadBytes((int)fi.Length);
                                            //image = new BitmapImage(new Uri(strFullPath));
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        EngineNS.Profiler.Log.WriteException(ex);
                                        bytes = null;
                                    }
                                    return true;
                                }, EngineNS.Thread.Async.EAsyncTarget.AsyncEditor);
                                if (bytes != null && bytes.Length > 0)
                                {
                                    var bitMap = new BitmapImage();
                                    bitMap.BeginInit();
                                    bitMap.StreamSource = new System.IO.MemoryStream(bytes);
                                    bitMap.EndInit();
                                    bitMap.CacheOption = BitmapCacheOption.OnLoad;
                                    image = bitMap;
                                }
                            }
                            break;
                    }
                }

            }
            catch (System.Exception e)
            {
                var msg = "ImageInit.GetImage: " + e.Message;
                System.Diagnostics.Debug.WriteLine(msg);
            }

            //if (image == null)
            //{
            //    var uri = new Uri("pack://application:,,,/ResourceLibrary;component/Icon/picLost.png");
            //    return new BitmapImage(uri);
            //}
            if (image == null)
                return null;
            return new ImageSource[] { image };
        }

        public static async System.Threading.Tasks.Task<ImageSource> GetImage(string strFullPath, Int32Rect rect)
        {
            var imgs = await GetImage(strFullPath);
            var source = imgs[0] as BitmapSource;
            if (source == null)
                return null;
            if (rect.Width <= 0 || rect.Height <= 0)
                return null;
            if ((rect.Width + rect.X) > source.PixelWidth)
                rect.Width = (int)(source.PixelWidth - rect.X);
            if ((rect.Height + rect.Y) > source.PixelHeight)
                rect.Height = (int)(source.PixelHeight - rect.Y);
            var stride = source.Format.BitsPerPixel * rect.Width / 8;
            byte[] data = new byte[rect.Height * stride];
            source.CopyPixels(rect, data, stride, 0);
            return BitmapSource.Create(rect.Width, rect.Height, source.DpiX, source.DpiY, source.Format, source.Palette, data, stride);
        }

        public static async System.Threading.Tasks.Task<ImageSource> GetImage(string strFullPath, System.Drawing.Color opacityColor)
        {
            var imgs = await GetImage(strFullPath);
            var bitMap = imgs[0] as BitmapSource;
            if (bitMap == null)
                return null;

            if (bitMap.Format == PixelFormats.Bgr32 ||
               bitMap.Format == PixelFormats.Bgra32)
            {
                // 生成一个BGRA32格式的BMP，将opacityColor的alpha位埴成255, 其余设成0
                var format = bitMap.Format;
                int strideSrc = (int)bitMap.PixelWidth * format.BitsPerPixel / 8;
                byte[] pixelSrc = new byte[(int)bitMap.PixelHeight * strideSrc];
                bitMap.CopyPixels(pixelSrc, strideSrc, 0);

                //byte delta = 10;
                WriteableBitmap wb = new WriteableBitmap((int)bitMap.PixelWidth, (int)bitMap.PixelHeight, 96, 96, System.Windows.Media.PixelFormats.Bgra32, null);
                var wbPixelWidth = wb.PixelWidth;
                var wbPixelheight = wb.PixelHeight;
                var wbBitsPerPixel = wb.Format.BitsPerPixel;
                byte[] pixelDest = new byte[wbPixelheight * wbPixelWidth * wbBitsPerPixel / 8];
                await EngineNS.CEngine.Instance.EventPoster.Post(() =>
                {
                    for (int x = 0; x < wbPixelWidth; ++x)
                    {
                        for (int y = 0; y < wbPixelheight; ++y)
                        {
                            int indexOfPixel = (x + y * wbPixelWidth) * wbBitsPerPixel / 8;
                            pixelDest[indexOfPixel] = pixelSrc[indexOfPixel];
                            pixelDest[indexOfPixel + 1] = pixelSrc[indexOfPixel + 1];
                            pixelDest[indexOfPixel + 2] = pixelSrc[indexOfPixel + 2];

                            if (opacityColor != System.Drawing.Color.Empty)
                            {
                                // 根据透明色设置Alpha
                                if (pixelDest[indexOfPixel] == opacityColor.R &&
                                   pixelDest[indexOfPixel + 1] == opacityColor.G &&
                                   pixelDest[indexOfPixel + 2] == opacityColor.B)
                                    pixelDest[indexOfPixel + 3] = 0;
                                else
                                    pixelDest[indexOfPixel + 3] = 255;
                            }
                            else
                                pixelDest[indexOfPixel + 3] = 255;
                            //// 品红色透明处理
                            //var vecDelta = new EngineNS.Vector3(0.2126f, 0.7152f, 0.0722f);
                            //if (pixelDest[indexOfPixel] > opacityColor.R - delta &&
                            //   pixelDest[indexOfPixel + 1] < opacityColor.G + delta &&
                            //   pixelDest[indexOfPixel + 2] > opacityColor.B - delta)
                            //{
                            //    SlimDX.Vector3 vecColor = new SlimDX.Vector3(255 - pixelDest[indexOfPixel], pixelDest[indexOfPixel + 1], 255 - pixelDest[indexOfPixel + 2]);
                            //    pixelDest[indexOfPixel + 3] = (byte)(SlimDX.Vector3.Dot(vecDelta, vecColor));
                            //}
                            //else
                            //{
                            //    pixelDest[indexOfPixel + 3] = 255;
                            //}
                        }
                    }
                    return true;
                });

                System.Windows.Int32Rect rect = new System.Windows.Int32Rect(0, 0, (int)wb.PixelWidth, (int)wb.PixelHeight);
                int strideDest = wb.PixelWidth * wb.Format.BitsPerPixel / 8;
                wb.WritePixels(rect, pixelDest, strideDest, 0);

                return wb;
            }

            return bitMap;
        }
        public static byte[] ReadRawData(string strFullPath)
        {
            byte[] bytes;
            using (var binReader = new System.IO.BinaryReader(System.IO.File.Open(strFullPath, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Delete | System.IO.FileShare.Read)))
            {
                if (binReader == null)
                    return null;
                System.IO.FileInfo fi = new System.IO.FileInfo(strFullPath);
                bytes = binReader.ReadBytes((int)fi.Length);
            }
            return bytes;
        }
        public static byte[] ConverImage2PNG(string strFullPath)
        {
            byte[] bytes;
            using (var binReader = new System.IO.BinaryReader(System.IO.File.Open(strFullPath, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Delete | System.IO.FileShare.Read)))
            {
                if (binReader == null)
                    return null;
                System.IO.FileInfo fi = new System.IO.FileInfo(strFullPath);
                bytes = binReader.ReadBytes((int)fi.Length);
            }
            var bitMap = new BitmapImage();
            bitMap.BeginInit();
            bitMap.StreamSource = new System.IO.MemoryStream(bytes);
            bitMap.EndInit();
            bitMap.CacheOption = BitmapCacheOption.OnLoad;

            var pngEncoder = new PngBitmapEncoder();

            pngEncoder.Frames.Add(BitmapFrame.Create(bitMap));
            var memStream = new System.IO.MemoryStream();
            pngEncoder.Save(memStream);
            return memStream.GetBuffer();
        }
    }
}
