using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EngineNS;

namespace EditorCMD
{
    public class ResourceCooker
    {
        public static AssetCooker.FCookResource CookXls = CookXlsImpl;
        private static bool CookXlsImpl(RName rn, string targetPath, bool copyRInfo, bool isandroid)
        {
            var dataSet = new EngineNS.Bricks.DataProvider.GDataSet();

            var resInfo = CMDEngine.CMDEngineInstance.mInfoManager.CreateResourceInfo("Xls") as ExcelViewEditor.ExcelResourceInfo;
            if (resInfo == null)
                return false;
            resInfo.Load(rn.Address + ".rinfo");
            if (resInfo.MacrossName == null)
                return false;
            Type objType = EngineNS.Macross.MacrossFactory.Instance.GetMacrossType(resInfo.MacrossName);
            bool result = dataSet.LoadExcel(objType, rn);
            if (result)
            {
                dataSet.Save2Xnd(targetPath + ".dateset");

                if (isandroid)
                {
                    CMDEngine.CMDEngineInstance.AddAssetInfos(targetPath + ".dateset");
                }
            }
            if (copyRInfo)
            {
                CEngine.Instance.FileManager.CopyFile(rn.Address, targetPath, true);
            }
            return true;
        }
        public static AssetCooker.FCookResource CookTxPic = CookTxPicImpl;
        [Flags]
        public enum ETexCompressMode
        {
            PNG = 1,
            ETC2 = (1 << 1),
            ASTC = (1 << 2),
        }
        public unsafe struct PKMHeader
        {
            public fixed char m_acMagicNumber[4];
            public fixed char m_acVersion[2];
            public byte m_ucDataType_msb;             // e.g. ETC1_RGB_NO_MIPMAPS
            public byte m_ucDataType_lsb;
            public byte m_ucExtendedWidth_msb;     //  padded to 4x4 blocks
            public byte m_ucExtendedWidth_lsb;
            public byte m_ucExtendedHeight_msb;    //  padded to 4x4 blocks
            public byte m_ucExtendedHeight_lsb;
            public byte m_ucOriginalWidth_msb;
            public byte m_ucOriginalWidth_lsb;
            public byte m_ucOriginalHeight_msb;
            public byte m_ucOriginalHeight_lsb;
        }
        public unsafe struct KTXHeader
        {
            fixed byte m_au8Identifier[12];
            public UInt32 m_u32Endianness;
            public UInt32 m_u32GlType;
            public UInt32 m_u32GlTypeSize;
            public UInt32 m_u32GlFormat;
            public UInt32 m_u32GlInternalFormat;
            public UInt32 m_u32GlBaseInternalFormat;
            public UInt32 m_u32PixelWidth;
            public UInt32 m_u32PixelHeight;
            public UInt32 m_u32PixelDepth;
            public UInt32 m_u32NumberOfArrayElements;
            public UInt32 m_u32NumberOfFaces;
            public UInt32 m_u32NumberOfMipmapLevels;
            public UInt32 m_u32BytesOfKeyValueData;
        }
        static void CalculateMipCount(int nWidth, int nHeight, ref int pWidthMipCount, ref int pHeightMipCount, ref int mipCount, ref bool pIsPowerOfTwo)
        {
            int value = 1;
            int exponentForWidth = 0;

            while ((value << exponentForWidth) <= nWidth)
            {
                ++exponentForWidth;
            }

            pWidthMipCount = exponentForWidth - 1;

            int exponentForHeight = 0;

            while ((value << exponentForHeight) <= nHeight)
                ++exponentForHeight;

            pHeightMipCount = exponentForHeight - 1;

            pWidthMipCount++;
            pHeightMipCount++;
            mipCount = pWidthMipCount > pHeightMipCount ? pWidthMipCount : pHeightMipCount;

            if (value << (exponentForWidth - 1) == nWidth &&
                value << (exponentForHeight - 1) == nHeight)
            {
                pIsPowerOfTwo = true;
            }
            else
            {
                pIsPowerOfTwo = false;
            }
        }
        public static ETexCompressMode TexCompressFlags = ETexCompressMode.PNG;
        private static unsafe bool CookTxPicImpl(RName rn, string targetPath, bool copyRInfo, bool isandroid)
        {
            var xnd = EngineNS.IO.XndHolder.SyncLoadXND(rn.Address);
            if (xnd == null)
                return false;

            var targetXnd = EngineNS.IO.XndHolder.NewXNDHolder();

            var attr = xnd.Node.FindAttrib("Desc");
            var txDesc = new EngineNS.CTxPicDesc();
            {
                if (attr.Version != 3)
                    return false;
                attr.BeginRead();
                attr.Read((IntPtr)(&txDesc), sizeof(EngineNS.CTxPicDesc));
                attr.EndRead();

                var sattr = targetXnd.Node.AddAttrib("Desc");
                sattr.Version = 3;
                sattr.BeginWrite();
                sattr.Write((IntPtr)(&txDesc), sizeof(EngineNS.CTxPicDesc));
                sattr.EndWrite();
            }

            byte[] pngData = null;
            attr = xnd.Node.FindAttrib("PNG");
            if (attr != null)
            {
                attr.BeginRead();
                attr.Read(out pngData, (int)attr.Length);
                attr.EndRead();
            }
            else
            {
                EngineNS.Profiler.Log.WriteLine(EngineNS.Profiler.ELogTag.Warning, "Cook Texture", $"{rn} don't have PNG data");

                var mipsNode = xnd.Node.FindNode("PngMips");
                if (mipsNode != null)
                {
                    var mipAttr = mipsNode.FindAttrib($"Mip_0");
                    mipAttr.BeginRead();
                    mipAttr.Read(out pngData, (int)mipAttr.Length);
                    mipAttr.EndRead();
                }
            }

            if (copyRInfo)
            {
                var sattr = xnd.Node.AddAttrib("PNG");
                sattr.BeginWrite();
                sattr.Write(pngData, pngData.Length);
                sattr.EndWrite();
            }

            var tmpPngFile = CEngine.Instance.FileManager.CookedTemp + Guid.NewGuid().ToString() + ".png";
            var sw = CEngine.Instance.FileManager.OpenFileForWrite(tmpPngFile, EngineNS.IO.EFileType.Texture);
            unsafe
            {
                fixed (byte* p = &pngData[0])
                {
                    sw.Write(p, (UIntPtr)pngData.Length);
                }
            }
            sw.Close();

            bool hasCompressData = false;
            if (pngData != null && (TexCompressFlags & ETexCompressMode.ETC2) != 0 && txDesc.EtcFormat != ETCFormat.UNKNOWN)
            {
                hasCompressData = true;
                string etxDDCKey = rn.ToString() + "->etc";
                var etcCmd = CEngine.Instance.FileManager.Bin + "tools/x64/EtcTool.exe";
                var etcData = CEngine.Instance.FileManager.DDCManager.GetCacheData(etxDDCKey, "texture");
                if (etcData == null)
                {
                    List<string> args = new List<string>();
                    args.Add($"{tmpPngFile}");
                    switch (txDesc.EtcFormat)
                    {
                        case ETCFormat.ETC1:
                            args.Add("-format ETC1");
                            break;
                        case ETCFormat.RGB8:
                            args.Add("-format RGB8");
                            break;
                        case ETCFormat.RGBA8:
                            args.Add("-format RGBA8");
                            break;
                        case ETCFormat.SRGBA8:
                            args.Add("-format SRGBA8");
                            break;
                        case ETCFormat.SRGB8:
                            args.Add("-format SRGB8");
                            break;
                        case ETCFormat.R11:
                        case ETCFormat.SIGNED_R11:
                            args.Add("-format R11");
                            break;
                        case ETCFormat.RGB8A1:
                            args.Add("-format RGB8A1");
                            break;
                        case ETCFormat.SRGB8A1:
                            args.Add("-format SRGB8A1");
                            break;
                        default:
                            args.Add("-format RGBA8");
                            break;
                    }
                    args.Add("-m 3");
                    args.Add("-v");

                    int pWidthMipCount = 0;
                    int pHeightMipCount = 0;
                    int mipCount = 0;
                    bool pIsPowerOfTwo = true;
                    CalculateMipCount(txDesc.Width, txDesc.Height, ref pWidthMipCount, ref pHeightMipCount, ref mipCount, ref pIsPowerOfTwo);
                    args.Add($"-m {mipCount}");

                    args.Add($"-output {tmpPngFile}.ktx");
                    List<string> outInfos = new List<string>();
                    EngineNS.Editor.Runner.ProcessExecuter.RunProcessSync(etcCmd, args, outInfos);

                    var pkmReader = CEngine.Instance.FileManager.OpenFileForRead(tmpPngFile + ".ktx", EngineNS.IO.EFileType.Texture);
                    unsafe
                    {
                        var etcNode = targetXnd.Node.AddNode("EtcMips", 0, 0);

                        KTXHeader ktxHeader = new KTXHeader();
                        pkmReader.Read(&ktxHeader, (UIntPtr)sizeof(KTXHeader));

                        for (int mip = 0; mip < ktxHeader.m_u32NumberOfMipmapLevels; mip++)
                        {
                            var layer = new EngineNS.Bricks.TexCompressor.ETCLayer();
                            pkmReader.Read(&layer, (UIntPtr)sizeof(EngineNS.Bricks.TexCompressor.ETCLayer));
                            byte[] etcMipData = new byte[layer.Size];
                            fixed (byte* pMD = &etcMipData[0])
                            {
                                pkmReader.Read(pMD, (UIntPtr)layer.Size);
                            }

                            var mipAttr = etcNode.AddAttrib($"Mip_{mip}");
                            mipAttr.BeginWrite();
                            mipAttr.Write(layer);
                            mipAttr.Write(etcMipData, etcMipData.Length);
                            mipAttr.EndWrite();
                        }
                    }
                    pkmReader.Cleanup();
                    CEngine.Instance.FileManager.DeleteFile(tmpPngFile + ".ktx");
                    CEngine.Instance.FileManager.DDCManager.SetCacheData(etxDDCKey, "texture", etcData);
                }
            }

            if ((TexCompressFlags & ETexCompressMode.ASTC) != 0)
            {
                //"AstcMips"
                hasCompressData = true;
                //EngineNS.Editor.Runner.ProcessExecuter.RunProcess("astcenc -cl example.png example.astc 6x6 -medium",.....)
            }

            CEngine.Instance.FileManager.DeleteFile(tmpPngFile);

            if ((TexCompressFlags & ETexCompressMode.PNG) != 0 || hasCompressData == false)
            {
                var mipsNode = xnd.Node.FindNode("PngMips");
                if (mipsNode == null)
                    return false;
                var smipsNode = targetXnd.Node.AddNode("PngMips", 0, 0);

                int curMip = 0;
                while (curMip < txDesc.MipLevel)
                {
                    var mipAttr = mipsNode.FindAttrib($"Mip_{curMip}");
                    var smipAttr = smipsNode.AddAttrib($"Mip_{curMip}");
                    curMip++;

                    mipAttr.BeginRead();
                    byte[] pngMipData;
                    mipAttr.Read(out pngMipData, (int)mipAttr.Length);
                    mipAttr.EndRead();

                    smipAttr.BeginWrite();
                    smipAttr.Write(pngMipData, pngMipData.Length);
                    smipAttr.EndWrite();
                }
            }

            EngineNS.IO.XndHolder.SaveXND(targetPath, targetXnd);
            CMDEngine.CMDEngineInstance.AddAssetInfos(targetPath);
            return true;
        }
    }
}
