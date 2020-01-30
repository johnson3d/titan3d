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
            ETC2 = (1<<1),
            ASTC = (1 << 2),
        }
        public static ETexCompressMode TexCompressFlags = ETexCompressMode.PNG;
        private static unsafe bool CookTxPicImpl(RName rn, string targetPath, bool copyRInfo, bool isandroid)
        {
            var xnd = EngineNS.IO.XndHolder.SyncLoadXND(rn.Address);
            if (xnd == null)
                return false;

            var targetXnd = EngineNS.IO.XndHolder.NewXNDHolder();

            var txDesc = new EngineNS.CTxPicDesc();
            {
                var attr = xnd.Node.FindAttrib("Desc");
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
            if (copyRInfo)
            {
                var attr = xnd.Node.FindAttrib("PNG");
                if (attr == null)
                    return false;
                attr.BeginRead();
                attr.Read(out pngData, (int)attr.Length);
                attr.EndRead();

                if(copyRInfo)
                {
                    var sattr = xnd.Node.AddAttrib("PNG");
                    sattr.BeginWrite();
                    sattr.Write(pngData, pngData.Length);
                    sattr.EndWrite();
                }
            }

            bool hasCompressData = false;
            if ((TexCompressFlags & ETexCompressMode.ETC2)!=0 && txDesc.EtcFormat != ETCFormat.UNKNOWN)
            {
                hasCompressData = true;
                using (var etcBlob = EngineNS.Support.CBlobProxy2.CreateBlobProxy())
                {
                    fixed (byte* dataPtr = &pngData[0])
                    {
                        var texCompressor = new EngineNS.Bricks.TexCompressor.CTexCompressor();
                        texCompressor.EncodePng2ETC((IntPtr)dataPtr, (uint)pngData.Length, txDesc.EtcFormat, txDesc.MipLevel, etcBlob);
                        etcBlob.BeginRead();
                    }

                    if (etcBlob.DataLength >= 0)
                    {
                        var etcNode = targetXnd.Node.AddNode("EtcMips", 0, 0);
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

            if ((TexCompressFlags & ETexCompressMode.ASTC) != 0)
            {
                //"AstcMips"
                hasCompressData = true;
                //EngineNS.Editor.Runner.ProcessExecuter.RunProcess("astcenc -cl example.png example.astc 6x6 -medium",.....)
            }

            if ((TexCompressFlags & ETexCompressMode.PNG) != 0 || hasCompressData==false)
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
