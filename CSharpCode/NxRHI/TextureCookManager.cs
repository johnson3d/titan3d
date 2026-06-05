using BCnEncoder.Encoder;
using BCnEncoder.Shared;
using BCnEncoder.Shared.ImageFiles;
using EngineNS.IO;
using StbImageSharp;
using System;
using System.IO;

namespace EngineNS.NxRHI
{
    /// <summary>
    /// Static manager for cooked texture cache under Cache/CookedAssets/texture/.
    /// Each platform produces its own file: {GUID}.dxt, {GUID}.astc, {GUID}.etc2
    /// so switching preview platform doesn't invalidate existing cooked data.
    /// New .srv files store only raw source data ("RawSource" node).
    /// This manager handles cooking (BC/ASTC/ETC2 compression) on demand
    /// and provides the loading path for GetTexture.
    /// </summary>
    public static class TtTextureCookManager
    {
        private const string RawSourceNodeName = "RawSource";
        private const string CookedTimestampAttr = "SourceTimestamp";
        private const string TextureSubDir = "texture";

        #region Path & Validation

        /// <summary>
        /// Get the platform-specific file extension for cooked textures.
        /// </summary>
        private static string GetPlatformExtension()
        {
            var config = TtEngine.Instance.GfxDevice.Config;
            switch (config.TextureAssetCompressType)
            {
                case Graphics.Pipeline.TtGfxDeviceConfig.ETextureAssetCompressType.DXT:
                    return ".dxt";
                case Graphics.Pipeline.TtGfxDeviceConfig.ETextureAssetCompressType.ASTC:
                    return ".astc";
                case Graphics.Pipeline.TtGfxDeviceConfig.ETextureAssetCompressType.ETC2:
                    return ".etc2";
                default:
                    return ".raw";
            }
        }

        /// <summary>
        /// Get the cooked texture directory path (Cache/cookedassets/texture/).
        /// </summary>
        private static string GetCookedTextureDir()
        {
            var cacheRoot = TtEngine.Instance.FileManager.GetPath(
                TtFileManager.ERootDir.Cache, TtFileManager.ESystemDir.CookedAssets);
            return cacheRoot + TextureSubDir + "/";
        }

        /// <summary>
        /// Get the cooked cache file path for a given asset GUID and current platform.
        /// Example: Cache/cookedassets/texture/a1b2c3d4e5f6...{32hex}.dxt
        /// </summary>
        public static string GetCookedPath(Guid assetId)
        {
            return GetCookedTextureDir() + assetId.ToString("N") + GetPlatformExtension();
        }

        /// <summary>
        /// Get the cooked cache file path for a given RName asset.
        /// </summary>
        public static string GetCookedPath(RName assetName)
        {
            var ameta = TtEngine.Instance.AssetMetaManager.GetAssetMeta(assetName) as TtSrViewAMeta;
            if (ameta == null)
                return null;
            return GetCookedPath(ameta.AssetId);
        }

        /// <summary>
        /// Check if a valid cooked cache exists for the given .srv asset (current platform).
        /// Validates source timestamp to detect stale cache.
        /// </summary>
        public static bool IsCookValid(RName srvAsset)
        {
            var cookedPath = GetCookedPath(srvAsset);
            if (cookedPath == null || !TtFileManager.FileExists(cookedPath))
                return false;

            long srvTimestamp = GetSourceTimestamp(srvAsset.Address);

            using (var xnd = TtXndHolder.LoadXnd(cookedPath))
            {
                if (xnd == null)
                    return false;

                var tsAttr = xnd.RootNode.TryGetAttribute(CookedTimestampAttr);
                if (!tsAttr.IsValidPointer)
                    return false;
                long cachedTimestamp;
                using (var ar = tsAttr.GetReader(null))
                {
                    ar.Read(out cachedTimestamp);
                }
                if (cachedTimestamp != srvTimestamp)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Check if the XND root node contains the new "RawSource" node (new format .srv).
        /// If not, this is an old-format .srv and should use legacy loading.
        /// </summary>
        public static bool HasRawSource(IO.TtXndNode rootNode)
        {
            var rawNode = rootNode.TryGetChildNode(RawSourceNodeName);
            return rawNode.IsValidPointer;
        }

        #endregion

        #region Cook

        /// <summary>
        /// Cook a texture asset: read raw source from .srv, compress per current platform config,
        /// and save to platform-specific cooked file.
        /// </summary>
        public static bool Cook(RName srvAsset)
        {
            var cookedPath = GetCookedPath(srvAsset);
            if (cookedPath == null)
                return false;

            TtSrView.TtPicDesc desc = null;
            bool isHdr = false;
            TtMemImage ldrImage = null;
            ImageResultFloat hdrImage = null;

            // Load raw source from .srv
            using (var xnd = TtXndHolder.LoadXnd(srvAsset.Address))
            {
                if (xnd == null)
                    return false;

                desc = TtTextureHelper.LoadPictureDesc(xnd.RootNode);
                if (desc == null)
                    return false;

                var rawNode = xnd.RootNode.TryGetChildNode(RawSourceNodeName);
                if (!rawNode.IsValidPointer)
                    return false;

                isHdr = desc.IsHdr();

                if (isHdr)
                {
                    hdrImage = LoadHdrFromRawNode(rawNode);
                    if (hdrImage == null)
                        return false;
                }
                else
                {
                    ldrImage = LoadLdrFromRawNode(rawNode);
                    if (ldrImage == null)
                        return false;
                }
            }

            // Determine compression format
            desc.CompressFormat = TtSrView.SelectCompressFormat(desc);

            // Ensure output directory exists
            TtFileManager.SureDirectory(GetCookedTextureDir());

            // Write cooked data
            using (var xnd = new TtXndHolder("CookedTexture", 0, 0))
            {
                if (isHdr)
                {
                    TtSrView.CookTextureTo(srvAsset, xnd.RootNode.mCoreObject, hdrImage, desc);
                }
                else
                {
                    TtSrView.CookTextureTo(srvAsset, xnd.RootNode.mCoreObject, ldrImage, desc);
                }

                // Write source timestamp for cache invalidation
                long srvTimestamp = GetSourceTimestamp(srvAsset.Address);
                var tsAttr = xnd.RootNode.mCoreObject.GetOrAddAttribute(CookedTimestampAttr, 0, 0, true);
                using (var aw = tsAttr.GetWriter(8))
                {
                    aw.Write(srvTimestamp);
                }

                xnd.SaveXnd(cookedPath);
            }

            return true;
        }

        /// <summary>
        /// Async version of Cook for background processing.
        /// </summary>
        public static async Thread.Async.TtTask<bool> CookAsync(RName srvAsset)
        {
            return await TtEngine.Instance.EventPoster.Post((state) =>
            {
                return Cook(srvAsset);
            }, Thread.Async.EAsyncTarget.AsyncIO);
        }

        #endregion

        #region Load

        /// <summary>
        /// Load a GPU texture from the cooked file. Uses the same loading path
        /// as LoadTexture2DMipLevel but from the cooked XND.
        /// </summary>
        public static unsafe TtTexture LoadFromCooked(RName srvAsset, int mipLevel, out TtSrView.TtPicDesc outDesc)
        {
            outDesc = null;
            var cookedPath = GetCookedPath(srvAsset);
            if (cookedPath == null || !TtFileManager.FileExists(cookedPath))
                return null;

            using (var xnd = TtXndHolder.LoadXnd(cookedPath))
            {
                if (xnd == null)
                    return null;

                outDesc = TtTextureHelper.LoadPictureDesc(xnd.RootNode);
                if (outDesc == null)
                    return null;

                if (mipLevel == -1 || mipLevel > outDesc.MipLevel)
                    mipLevel = outDesc.MipLevel;

                return TtSrView.LoadTexture2DMipLevel(srvAsset, xnd.RootNode, outDesc, mipLevel, null);
            }
        }

        /// <summary>
        /// Full load path for new-format .srv: check cache → cook if needed → load from cache.
        /// Returns the created TtTexture, or null on failure.
        /// </summary>
        public static unsafe TtTexture LoadOrCook(RName srvAsset, IO.TtXndNode srvRootNode, int mipLevel, out TtSrView.TtPicDesc outDesc)
        {
            outDesc = null;

            // Try loading from existing valid cache
            if (IsCookValid(srvAsset))
            {
                var tex = LoadFromCooked(srvAsset, mipLevel, out outDesc);
                if (tex != null)
                    return tex;
            }

            // Cache miss or invalid — cook now (synchronous on first load)
            if (!Cook(srvAsset))
                return null;

            return LoadFromCooked(srvAsset, mipLevel, out outDesc);
        }

        #endregion

        #region Raw Source I/O

        /// <summary>
        /// Save raw LDR image data to the "RawSource" node in a .srv XND (native node).
        /// Stores as PNG-encoded bytes for lossless compression.
        /// </summary>
        public static unsafe void SaveLdrToRawNode(EngineNS.XndNode parentNode, TtMemImage image)
        {
            var rawNode = parentNode.GetOrAddNode(RawSourceNodeName, 0, 0, true);

            // Store format indicator
            var fmtAttr = rawNode.GetOrAddAttribute("Format", 0, 0, true);
            using (var aw = fmtAttr.GetWriter(8))
            {
                aw.Write("LDR");
            }

            // Store as PNG bytes (lossless, good compression)
            using (var memStream = new MemoryStream())
            {
                var writer = new StbImageWriteSharp.ImageWriter();
                var comp = image.Comp == ColorComponents.RedGreenBlueAlpha
                    ? StbImageWriteSharp.ColorComponents.RedGreenBlueAlpha
                    : StbImageWriteSharp.ColorComponents.RedGreenBlue;
                writer.WritePng(image.Data, image.Width, image.Height, comp, memStream);
                var pngData = memStream.ToArray();

                var dataAttr = rawNode.GetOrAddAttribute("Data", 0, 0, true);
                using (var aw = dataAttr.GetWriter((ulong)pngData.Length))
                {
                    aw.WriteNoSize(pngData, pngData.Length);
                }
            }

            // Store dimensions for quick access without decoding
            var dimAttr = rawNode.GetOrAddAttribute("Dimensions", 0, 0, true);
            using (var aw = dimAttr.GetWriter(16))
            {
                aw.Write(image.Width);
                aw.Write(image.Height);
                aw.Write((int)image.Comp);
            }
        }

        /// <summary>
        /// Save raw HDR image data to the "RawSource" node in a .srv XND (native node).
        /// Stores as HDR-encoded bytes.
        /// </summary>
        public static unsafe void SaveHdrToRawNode(EngineNS.XndNode parentNode, ImageResultFloat image)
        {
            var rawNode = parentNode.GetOrAddNode(RawSourceNodeName, 0, 0, true);

            // Store format indicator
            var fmtAttr = rawNode.GetOrAddAttribute("Format", 0, 0, true);
            using (var aw = fmtAttr.GetWriter(8))
            {
                aw.Write("HDR");
            }

            // Store as HDR bytes
            using (var memStream = new MemoryStream())
            {
                var writer = new StbImageWriteSharp.ImageWriter();
                var writeComp = Bricks.ImageDecoder.UStbImageUtility.ConvertColorComponent(image.Comp);
                fixed (void* ptr = image.Data)
                {
                    writer.WriteHdr(ptr, image.Width, image.Height, writeComp, memStream);
                }
                var hdrData = memStream.ToArray();

                var dataAttr = rawNode.GetOrAddAttribute("Data", 0, 0, true);
                using (var aw = dataAttr.GetWriter((ulong)hdrData.Length))
                {
                    aw.WriteNoSize(hdrData, hdrData.Length);
                }
            }

            // Store dimensions
            var dimAttr = rawNode.GetOrAddAttribute("Dimensions", 0, 0, true);
            using (var aw = dimAttr.GetWriter(16))
            {
                aw.Write(image.Width);
                aw.Write(image.Height);
                aw.Write((int)image.Comp);
            }
        }

        /// <summary>
        /// Save raw EXR data (original file bytes) to the "RawSource" node (native node).
        /// </summary>
        public static unsafe void SaveExrToRawNode(EngineNS.XndNode parentNode, byte[] exrFileBytes, int width, int height)
        {
            var rawNode = parentNode.GetOrAddNode(RawSourceNodeName, 0, 0, true);

            var fmtAttr = rawNode.GetOrAddAttribute("Format", 0, 0, true);
            using (var aw = fmtAttr.GetWriter(8))
            {
                aw.Write("EXR");
            }

            var dataAttr = rawNode.GetOrAddAttribute("Data", 0, 0, true);
            using (var aw = dataAttr.GetWriter((ulong)exrFileBytes.Length))
            {
                aw.WriteNoSize(exrFileBytes, exrFileBytes.Length);
            }

            var dimAttr = rawNode.GetOrAddAttribute("Dimensions", 0, 0, true);
            using (var aw = dimAttr.GetWriter(16))
            {
                aw.Write(width);
                aw.Write(height);
                aw.Write(4); // RGBA
            }
        }

        /// <summary>
        /// Load LDR image from a RawSource XndNode (native).
        /// </summary>
        public static unsafe TtMemImage LoadLdrFromRawNode(EngineNS.XndNode rawNode)
        {
            var dataAttr = new EngineNS.XndAttribute(rawNode.TryGetAttribute("Data"));
            if (!dataAttr.IsValidPointer)
                return null;

            byte[] pngData;
            using (var ar = dataAttr.GetReader(null))
            {
                ar.ReadNoSize(out pngData, (int)dataAttr.GetReaderLength());
            }

            using (var memStream = new MemoryStream(pngData))
            {
                return TtMemImage.FromStream(memStream, ColorComponents.Default);
            }
        }

        /// <summary>
        /// Load HDR image from a RawSource XndNode (native).
        /// </summary>
        public static unsafe ImageResultFloat LoadHdrFromRawNode(EngineNS.XndNode rawNode)
        {
            var fmtAttr = new EngineNS.XndAttribute(rawNode.TryGetAttribute("Format"));
            if (!fmtAttr.IsValidPointer)
                return null;

            string format;
            using (var ar = fmtAttr.GetReader(null))
            {
                ar.Read(out format);
            }

            var dataAttr = new EngineNS.XndAttribute(rawNode.TryGetAttribute("Data"));
            if (!dataAttr.IsValidPointer)
                return null;

            byte[] data;
            using (var ar = dataAttr.GetReader(null))
            {
                ar.ReadNoSize(out data, (int)dataAttr.GetReaderLength());
            }

            if (format == "HDR")
            {
                using (var memStream = new MemoryStream(data))
                {
                    return ImageResultFloat.FromStream(memStream, ColorComponents.RedGreenBlueAlpha);
                }
            }
            else if (format == "EXR")
            {
                using (var memStream = new MemoryStream(data))
                {
                    var exrFile = new Jither.OpenEXR.EXRFile(memStream);
                    return TtSrView.LoadExrToImageFloat(exrFile);
                }
            }

            return null;
        }

        #endregion

        #region Helpers

        private static long GetSourceTimestamp(string filePath)
        {
            if (!File.Exists(filePath))
                return 0;
            return File.GetLastWriteTimeUtc(filePath).Ticks;
        }

        #endregion
    }
}
