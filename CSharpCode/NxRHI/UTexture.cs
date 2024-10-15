using BCnEncoder.Encoder;
using BCnEncoder.Shared;
using EngienNS.Bricks.ImageDecoder;
using EngineNS.Bricks.CodeBuilder.MacrossNode;
using EngineNS.EGui.Controls;
using EngineNS.IO;
using Jither.OpenEXR;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Toolkit.HighPerformance;
using NPOI.OpenXmlFormats.Vml;
using NPOI.SS.Formula.Functions;
using NPOI.Util;
using Org.BouncyCastle.Cms;
using Org.BouncyCastle.Crypto.IO;
using SixLabors.ImageSharp.ColorSpaces;
using SixLabors.ImageSharp.PixelFormats;
using StbImageSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Linq;
using EngineNS.Graphics.Pipeline.Shader;
using Mono.Cecil.Cil;

namespace EngineNS.NxRHI
{
    [Rtti.Meta(NameAlias = new string[] { "EngineNS.NxRHI.USrViewAMeta@EngineCore", "EngineNS.NxRHI.USrViewAMeta" })]
    public class TtSrViewAMeta : IO.IAssetMeta
    {
        public override string TypeExt
        {
            get => TtSrView.AssetExt;
        }
        protected override Color4b GetBorderColor()
        {
            return TtEngine.Instance.EditorInstance.Config.TextureBoderColor;
        }
        public override string GetAssetTypeName()
        {
            return "SrView";
        }
        public override async System.Threading.Tasks.Task<IO.IAsset> LoadAsset()
        {
            return await TtEngine.Instance.GfxDevice.TextureManager.GetTexture(GetAssetName());
        }
        public override void OnBeforeRenamedAsset(IAsset asset, RName name)
        {
            ((TtSrView)asset).LoadOriginImageObject();
            CoreSDK.CheckResult(TtEngine.Instance.GfxDevice.TextureManager.UnsafeRemove(name) == asset);
        }
        public override void OnAfterRenamedAsset(IAsset asset, RName name)
        {
            ((TtSrView)asset).FreeOriginImageObject();
            TtEngine.Instance.GfxDevice.TextureManager.UnsafeAdd(name, (TtSrView)asset);
        }
        Thread.Async.TtTask<TtSrView>? SnapTask;
        Thread.Async.TtTask<EngineNS.Graphics.Pipeline.Shader.TtEffect>? EffectTask;
        public override bool CanRefAssetType(IO.IAssetMeta ameta)
        {
            //纹理不会引用别的资产
            return false;
        }
        //public unsafe override void OnDraw(in ImDrawList cmdlist, in Vector2 sz, EGui.Controls.UContentBrowser ContentBrowser)
        //{
        //    var start = ImGuiAPI.GetItemRectMin();
        //    var end = start + sz;

        //    var name = IO.FileManager.GetPureName(GetAssetName().Name);
        //    var tsz = ImGuiAPI.CalcTextSize(name, false, -1);
        //    Vector2 tpos;
        //    tpos.Y = start.Y + sz.Y - tsz.Y;
        //    tpos.X = start.X + (sz.X - tsz.X) * 0.5f;
        //    //ImGuiAPI.PushClipRect(in start, in end, true);

        //    end.Y -= tsz.Y;
        //    OnDrawSnapshot(in cmdlist, ref start, ref end);
        //    cmdlist.AddRect(in start, in end, (uint)GetBorderColor().ToAbgr(),
        //        EGui.UCoreStyles.Instance.SnapRounding, ImDrawFlags_.ImDrawFlags_RoundCornersAll, EGui.UCoreStyles.Instance.SnapThinkness);

        //    cmdlist.AddText(in tpos, 0xFFFF00FF, name, null);
        //    //ImGuiAPI.PopClipRect();

        //    DrawPopMenu(ContentBrowser);
        //}
        public override void OnShowIconTimout(int time)
        {
            if (SnapTask != null)
            {
                CoreSDK.DisposeObject(ref CmdParameters);
                SnapTask = null;
            }
        }
        protected bool mShowA = false;
        EngineNS.Editor.Forms.TtTextureViewerCmdParams CmdParameters = null;
        private async Thread.Async.TtTask<EngineNS.Graphics.Pipeline.Shader.TtEffect> GetEffect(bool isCubemap)
        {
            TtShadingEnv shading = null;
            if(isCubemap)
                shading = await TtEngine.Instance.ShadingEnvManager.GetShadingEnv<EngineNS.Editor.Forms.USlateTextureCubeViewerShading>();
            else
                shading = await TtEngine.Instance.ShadingEnvManager.GetShadingEnv<EngineNS.Editor.Forms.USlateTextureViewerShading>();
            return await TtEngine.Instance.GfxDevice.EffectManager.GetEffect(shading,
                TtEngine.Instance.GfxDevice.MaterialManager.ScreenMaterial,
                new Graphics.Mesh.UMdfStaticMesh());
        }
        public override void OnDrawSnapshot(in ImDrawList cmdlist, ref Vector2 start, ref Vector2 end)
        {
            if (SnapTask == null)
            {
                var rc = TtEngine.Instance.GfxDevice.RenderContext;
                SnapTask = TtEngine.Instance.GfxDevice.TextureManager.GetTexture(this.GetAssetName(), 1);
                //cmdlist.AddText(in start, 0xFFFFFFFF, "texture", null);

                return;
            }
            if (EffectTask == null)
            {
                if(SnapTask.Value.IsCompleted == true && SnapTask.Value.Result != null)
                    EffectTask = GetEffect(SnapTask.Value.Result.PicDesc.CubeFaces == 6);
                return;
            }
            if (SnapTask.Value.IsCompleted == false || EffectTask.Value.IsCompleted == false)
            {
                cmdlist.AddText(in start, 0xFFFFFFFF, "loading...", null);
                return;
            }
            unsafe
            {
                if(CmdParameters==null && SnapTask.Value.Result!=null && EffectTask.Value.Result!=null)
                {
                    var rc = TtEngine.Instance.GfxDevice.RenderContext;
                    var SlateEffect = EffectTask.Value.Result;

                    var iptDesc = new NxRHI.TtInputLayoutDesc();
                    unsafe
                    {
                        iptDesc.mCoreObject.AddElement("POSITION", 0, EPixelFormat.PXF_R32G32_FLOAT, 0, 0, 0, 0);
                        iptDesc.mCoreObject.AddElement("TEXCOORD", 0, EPixelFormat.PXF_R32G32_FLOAT, 0, (uint)sizeof(Vector2), 0, 0);
                        iptDesc.mCoreObject.AddElement("COLOR", 0, EPixelFormat.PXF_R8G8B8A8_UNORM, 0, (uint)sizeof(Vector2) * 2, 0, 0);
                        //iptDesc.SetShaderDesc(SlateEffect.GraphicsEffect);
                    }
                    iptDesc.mCoreObject.SetShaderDesc(SlateEffect.DescVS.mCoreObject);
                    var InputLayout = rc.CreateInputLayout(iptDesc); //TtEngine.Instance.GfxDevice.InputLayoutManager.GetPipelineState(rc, iptDesc);
                    SlateEffect.ShaderEffect.mCoreObject.BindInputLayout(InputLayout.mCoreObject);

                    var cmdParams = EGui.TtImDrawCmdParameters.CreateInstance<EngineNS.Editor.Forms.TtTextureViewerCmdParams>();
                    var cbBinder = SlateEffect.ShaderEffect.FindBinder("ProjectionMatrixBuffer");
                    cmdParams.CBuffer = rc.CreateCBV(cbBinder);
                    cmdParams.Drawcall.BindShaderEffect(SlateEffect);
                    cmdParams.Drawcall.BindCBuffer(cbBinder.mCoreObject, cmdParams.CBuffer);
                    cmdParams.Drawcall.BindSRV(TtNameTable.FontTexture, SnapTask.Value.Result);
                    cmdParams.Drawcall.BindSampler(TtNameTable.Samp_FontTexture, TtEngine.Instance.GfxDevice.SamplerStateManager.PointState);

                    cmdParams.IsNormalMap = 0;
                    if (SnapTask.Value.Result.PicDesc.Format == EPixelFormat.PXF_BC5_UNORM || SnapTask.Value.Result.PicDesc.Format == EPixelFormat.PXF_BC5_TYPELESS || SnapTask.Value.Result.PicDesc.Format == EPixelFormat.PXF_BC5_SNORM)
                        cmdParams.IsNormalMap = 1;

                    CmdParameters = cmdParams;
                }

                var uv0 = new Vector2(0, 0);
                var uv1 = new Vector2(1, 1);
                if (SnapTask.Value.Result != null)
                {
                    cmdlist.AddImage(CmdParameters.GetHandle(), in start, in end, in uv0, in uv1, 0xFFFFFFFF);
                }

                // support preview A channel
                //var textPos = end - new Vector2(32, 32);
                //cmdlist.AddText(textPos, mShowA ? 0xFFFFFFFF : 0x00FF00FF, "A", null);
                //if (ImGuiAPI.IsMouseClicked(ImGuiMouseButton_.ImGuiMouseButton_Left, false) && ImGuiAPI.IsMouseHoveringRect(textPos, end, true))
                //{
                //    CmdParameters.ColorMask.W = mShowA ? 1 : 0;
                //    mShowA = !mShowA;
                //}
            }
            //cmdlist.AddText(in start, 0xFFFFFFFF, "texture", null);
        }
        protected override void OnDrawPopMenu(EGui.Controls.TtContentBrowser ContentBrowser)
        {
            base.OnDrawPopMenu(ContentBrowser);

            Support.TtAnyPointer menuData = new Support.TtAnyPointer();
            var drawList = ImGuiAPI.GetWindowDrawList();
            if (EGui.UIProxy.MenuItemProxy.MenuItem("ReImport", null, false, null, in drawList, in menuData, ref mRefGraphMenuState))
            {
                //renwind todo
            }
        }

        public override void DrawTooltip()
        {
            if (SnapTask == null || !SnapTask.Value.IsCompleted)
                return;
            if (SnapTask.Value.Result == null)
            {
                SnapTask = null;
                return;
            }
            CtrlUtility.DrawHelper(
                "Name: " + GetAssetName().Name,
                "Desc: " + Description,
                "Address: " + GetAssetName().Address,
                "Res: " + SnapTask.Value.Result.PicDesc.Width + "X" + SnapTask.Value.Result.PicDesc.Height + "\r\n" +
                "Format: " + SnapTask.Value.Result.PicDesc.Format + "\r\n" +
                "CubeFaces: " + SnapTask.Value.Result.PicDesc.CubeFaces + "\r\n" +
                "MipLevel: " + SnapTask.Value.Result.PicDesc.MipLevel + "\r\n" +
                "IsSRGB: " + SnapTask.Value.Result.PicDesc.sRGB + "\r\n" +
                "IsNormal: " + SnapTask.Value.Result.PicDesc.IsNormal);
        }
    }
    [Rtti.Meta(NameAlias = new string[] { "EngineNS.NxRHI.USrView@EngineCore", "EngineNS.NxRHI.USrView" })]
    [TtSrView.Import]
    [IO.AssetCreateMenu(MenuName = "Texture")]
    public partial class TtSrView : AuxPtrType<NxRHI.ISrView>, IO.IAsset, IO.IStreaming
    {
        public class TtPicDesc
        {
            public TtPicDesc()
            {
                Desc.SetDefault();
            }
            public FPictureDesc Desc;
            //public uint dwStructureSize { get => Desc.dwStructureSize; set => Desc.dwStructureSize = value; }
            [ReadOnly(true)]
            public ETextureCompressFormat CompressFormat { get => Desc.CompressFormat; set => Desc.CompressFormat = value; }
            [ReadOnly(true)]
            public EPixelFormat Format { get => Desc.Format; set => Desc.Format = value; }
            public uint CubeFaces { get => Desc.CubeFaces; set => Desc.CubeFaces = value; }
            public int MipLevel { get => Desc.MipLevel; set => Desc.MipLevel = value; }
            [ReadOnly(true)]
            public int Width { get => Desc.Width; set => Desc.Width = value; }
            [ReadOnly(true)]
            public int Height { get => Desc.Height; set => Desc.Height = value; }
            public byte BitNumRed { get => Desc.BitNumRed; set => Desc.BitNumRed = value; }
            public byte BitNumGreen { get => Desc.BitNumGreen; set => Desc.BitNumGreen = value; }
            public byte BitNumBlue { get => Desc.BitNumBlue; set => Desc.BitNumBlue = value; }
            public byte BitNumAlpha { get => Desc.BitNumAlpha; set => Desc.BitNumAlpha = value; }
            public bool DontCompress 
            {
                get => Desc.DontCompress != 0 ? true : false;
                set => Desc.DontCompress = value ? 1 : 0;
            }
            public bool sRGB
            {
                get => Desc.sRGB != 0 ? true : false;
                set => Desc.sRGB = value ? 1 : 0;
            }
            public bool StripOriginSource
            {
                get => Desc.StripOriginSource != 0 ? true : false;
                set => Desc.StripOriginSource = value ? 1 : 0;
            }
            bool mAutoCheckNormal = true;
            public bool AutoCheckNormal { get => mAutoCheckNormal; set => mAutoCheckNormal = value; }
            bool mIsNormal;
            public bool IsNormal { get => mIsNormal; set => mIsNormal=value; }
            bool mIsAutoSaveSrcImage = false;
            public bool IsAutoSaveSrcImage { get => mIsAutoSaveSrcImage; set => mIsAutoSaveSrcImage = value; }
            public List<Vector3i> MipSizes { get; } = new List<Vector3i>();
            public List<Vector2i> BlockDimenstions { get; } = new List<Vector2i>();
            public int BlockSize = 0;
        }
        public EPixelFormat SrvFormat
        {
            get
            {
                return mCoreObject.GetBufferAsTexture().Desc.Format;
            }
        }
        public TtPicDesc PicDesc { get; set; }
        public TtTexture StreamingTexture { get; private set; } = null;
        public object TagObject;
        public static int NumOfInstance = 0;
        public static int NumOfGCHandle = 0;

        public void SetDebugName(string name)
        {
            mCoreObject.NativeSuper.SetDebugName(name);
        }

        #region Cubemap
        // transform world space vector to a space relative to the face
        static Vector3 TransformSideToWorldSpace(uint CubemapFace, Vector3 InDirection)
        {
            float x = InDirection.X, y = InDirection.Y, z = InDirection.Z;

            Vector3 Ret = new Vector3(0, 0, 0);

            // see http://msdn.microsoft.com/en-us/library/bb204881(v=vs.85).aspx
            switch (CubemapFace)
            {
                case 0: Ret = new Vector3(+z, -y, -x); break;
                case 1: Ret = new Vector3(-z, -y, +x); break;
                case 2: Ret = new Vector3(+x, +z, +y); break;
                case 3: Ret = new Vector3(+x, -z, -y); break;
                case 4: Ret = new Vector3(+x, -y, +z); break;
                case 5: Ret = new Vector3(-x, -y, -z); break;
            }

            // this makes it with the Unreal way (z and y are flipped)
            return Ret;
        }

        // transform vector relative to the face to world space
        static Vector3 TransformWorldToSideSpace(uint CubemapFace, Vector3 InDirection)
        {
            // undo Unreal way (z and y are flipped)
            float x = InDirection.X, y = InDirection.Z, z = InDirection.Y;

            Vector3 Ret = new Vector3(0, 0, 0);

            // see http://msdn.microsoft.com/en-us/library/bb204881(v=vs.85).aspx
            switch (CubemapFace)
            {
                case 0: Ret = new Vector3(-z, -y, +x); break;
                case 1: Ret = new Vector3(+z, -y, -x); break;
                case 2: Ret = new Vector3(+x, +z, +y); break;
                case 3: Ret = new Vector3(+x, -z, -y); break;
                case 4: Ret = new Vector3(+x, -y, +z); break;
                case 5: Ret = new Vector3(-x, -y, -z); break;
            }

            return Ret;
        }

        static Vector3 ComputeSSCubeDirectionAtTexelCenter(uint x, uint y, float InvSideExtent)
        {
            // center of the texels
            Vector3 DirectionSS = new Vector3((x +0.5f) *InvSideExtent * 2 - 1, (y + 0.5f) * InvSideExtent * 2 - 1, 1);
            DirectionSS.Normalize();
            return DirectionSS;
        }

        static Vector3 ComputeWSCubeDirectionAtTexelCenter(uint CubemapFace, uint x, uint y, float InvSideExtent)
        {
            Vector3 DirectionSS = ComputeSSCubeDirectionAtTexelCenter(x, y, InvSideExtent);
            Vector3 DirectionWS = TransformSideToWorldSpace(CubemapFace, DirectionSS);
            return DirectionWS;
        }

        static int ComputeLongLatCubemapExtents(int SrcImageWidth, int MaxCubemapTextureResolution)
        {
            int width = 1 << (int)MathHelper.ILog2Const((uint)SrcImageWidth / 2);
            return MathHelper.Clamp(width, 32, MaxCubemapTextureResolution);
        }

        /**
 * View in to an image that allows access by converting a direction to longitude and latitude.
 */
        struct ImageViewLongLat
        {
            /** Image colors. */
            Vector4[] ImageColors;
            /** Width of the image. */
            int SizeX;
            /** Height of the image. */
            int SizeY;

            /** Initialization constructor. */
            public ImageViewLongLat(ImageResultFloat Image)
            {
                SizeX = Image.Width;
                SizeY = Image.Height;
                ImageColors = new Vector4[Image.Width * Image.Height];
                if (Image.Comp == ColorComponents.RedGreenBlueAlpha)
                {
                    for (int i = 0; i < ImageColors.Length; ++i)
                    {
                        ImageColors[i].X = Image.Data[i * 4 + 0];
                        ImageColors[i].Y = Image.Data[i * 4 + 1];
                        ImageColors[i].Z = Image.Data[i * 4 + 2];
                        ImageColors[i].W = Image.Data[i * 4 + 3];
                    }
                }
            }

            /** Wraps X around W. */
            static void WrapTo(ref int X, int W)
            {
                X = X % W;

                if (X < 0)
                {
                    X += W;
                }
            }

            /** Const access to a texel. */
            Vector4 Access(int X, int Y)
        	{
		        return ImageColors[X + Y * SizeX];
	        }

            /** Makes a filtered lookup. */
            Vector4 LookupFiltered(float X, float Y)
    	    {
                int X0 = (int)MathHelper.Floor(X);
                int Y0 = (int)MathHelper.Floor(Y);

                float FracX = X - X0;
                float FracY = Y - Y0;

                int X1 = X0 + 1;
                int Y1 = Y0 + 1;

                WrapTo(ref X0, SizeX);
                WrapTo(ref X1, SizeX);
                Y0 = MathHelper.Clamp(Y0, 0, (int) (SizeY - 1));
		        Y1 = MathHelper.Clamp(Y1, 0, (int) (SizeY - 1));

		        Vector4 CornerRGB00 = Access(X0, Y0);
                Vector4 CornerRGB10 = Access(X1, Y0);
                Vector4 CornerRGB01 = Access(X0, Y1);
                Vector4 CornerRGB11 = Access(X1, Y1);

                Vector4 CornerRGB0 = Vector4.Lerp(CornerRGB00, CornerRGB10, FracX);
                Vector4 CornerRGB1 = Vector4.Lerp(CornerRGB01, CornerRGB11, FracX);

		        return Vector4.Lerp(CornerRGB0, CornerRGB1, FracY);
	        }

            /** Makes a filtered lookup using a direction. */
            public Vector4 LookupLongLat(Vector3 NormalizedDirection)
	        {
                // see http://gl.ict.usc.edu/Data/HighResProbes
                // latitude-longitude panoramic format = equirectangular mapping
                float X = (1 + MathHelper.Atan2(NormalizedDirection.X, NormalizedDirection.Z) / MathHelper.PI) / 2 * SizeX;
                float Y = MathHelper.Acos(NormalizedDirection.Y) / MathHelper.PI * SizeY;

                return LookupFiltered(X, Y);
            }
        };

        static void CopyFaceToCubemapContinus(Vector4[] faceData, Vector4[] cubeMapExpand, int faceStartIndex, int Extent)
        {
            var faceDataSize = Extent * Extent;
            for (int y = 0; y < Extent; ++y)
            {
                for (int x = 0; x < Extent; ++x)
                {
                    cubeMapExpand[faceStartIndex + x + y * 4 * Extent] = faceData[x + y * Extent];
                }
            }
        }

        /**
         * Generates the base cubemap mip from a longitude-latitude 2D image.
         * @param OutMip - The output mip.
         * @param SrcImage - The source longlat image.
         */
        public static void GenerateBaseCubeMipFromLongitudeLatitude2D(ref StbImageSharp.ImageResultFloat OutMip, StbImageSharp.ImageResultFloat LongLatImage, int MaxCubemapTextureResolution)
        {
            ImageViewLongLat LongLatView = new ImageViewLongLat(LongLatImage);

            // TODO_TEXTURE: Expose target size to user.
            int Extent = ComputeLongLatCubemapExtents(LongLatImage.Width, MaxCubemapTextureResolution);
            float InvExtent = 1.0f / Extent;

            Vector4[] faceDataContinus = new Vector4[6 * Extent * Extent];
            Vector4[][] faceDatas = new Vector4[6][];
            for (uint Face = 0; Face < 6; ++Face)
            {
                faceDatas[Face] = new Vector4[Extent*Extent];
                //Vector4[] faceData = new Vector4[Extent * Extent];
                for (int y = 0; y < Extent; ++y)
                {
                    for (int x = 0; x < Extent; ++x)
                    {
                        Vector3 DirectionWS = ComputeWSCubeDirectionAtTexelCenter(Face, (uint)x, (uint)y, InvExtent);
                        faceDatas[Face][x + y*Extent] = LongLatView.LookupLongLat(DirectionWS);
                    }
                }

                faceDatas[Face].CopyTo(faceDataContinus, Face * Extent * Extent);
            }

            float[] floatArray = faceDataContinus.SelectMany(vector => new float[] { vector.X, vector.Y, vector.Z, vector.W }).ToArray();
            unsafe
            {
                fixed (float* floatPtr = floatArray)
                {
                    OutMip = StbImageSharp.ImageResultFloat.FromResult(floatPtr, Extent, 6 * Extent, ColorComponents.RedGreenBlueAlpha, ColorComponents.RedGreenBlueAlpha);
                }

                #region Debug
                bool bDebug = false;
                if (bDebug)
                {
                    var faceDataSize = Extent * Extent;
                    Vector4[] cubeMapExpand = new Vector4[4 * 3 * faceDataSize];

                    CopyFaceToCubemapContinus(faceDatas[0], cubeMapExpand, 2 * Extent + 4 * faceDataSize, Extent);
                    CopyFaceToCubemapContinus(faceDatas[1], cubeMapExpand, 4 * faceDataSize, Extent);
                    CopyFaceToCubemapContinus(faceDatas[2], cubeMapExpand, 1 * Extent, Extent);
                    CopyFaceToCubemapContinus(faceDatas[3], cubeMapExpand, 1 * Extent + 8 * faceDataSize, Extent);
                    CopyFaceToCubemapContinus(faceDatas[4], cubeMapExpand, 1 * Extent + 4 * faceDataSize, Extent);
                    CopyFaceToCubemapContinus(faceDatas[5], cubeMapExpand, 3 * Extent + 4 * faceDataSize, Extent);

                    float[] floatArrayDebug = cubeMapExpand.SelectMany(vector => new float[] { vector.X, vector.Y, vector.Z, vector.W }).ToArray();

                    fixed (float* floatPtrDebug = floatArrayDebug)
                    {
                        OutMip = StbImageSharp.ImageResultFloat.FromResult(floatPtrDebug, 4 * Extent, 3 * Extent, ColorComponents.RedGreenBlueAlpha, ColorComponents.RedGreenBlueAlpha);

                        var sourceFile = "F:/CubeFaces" + ".hdr";
                        using (var stream = System.IO.File.Create(sourceFile))
                        {
                            var writer = new StbImageWriteSharp.ImageWriter();
                            writer.WriteHdr(floatPtrDebug, 4 * Extent, 3 * Extent, StbImageWriteSharp.ColorComponents.RedGreenBlueAlpha, stream);
                        }
                    }
                }
                #endregion
            }

        }
        #endregion

        public class NormalmapChecker
        {
            // These values are the threshold values for the average vector's
            // length to be considered within limits as a normal map normal
            const float NormalMapMinLengthConfidenceThreshold = 0.55f;
            const float NormalMapMaxLengthConfidenceThreshold = 1.1f;

            // This value is the threshold value for the average vector to be considered
            // to be going in the correct direction.
            const float NormalMapDeviationThreshold = 0.8f;

            // Samples from the texture will be taken in blocks of this size^2
            const int SampleTileEdgeLength = 4;

            // We sample up to this many tiles in each axis. Sampling more tiles
            // will likely be more accurate, but will take longer.
            const int MaxTilesPerAxis = 16;

            // This is used in the comparison with "mid-gray"
            const float ColorComponentNearlyZeroThreshold = (2.0f / 255.0f);

            // This is used when comparing alpha to zero to avoid picking up sprites
            const float AlphaComponentNearlyZeroThreshold = (1.0f / 255.0f);

            // These values are chosen to make the threshold colors (from uint8 textures)
            // discard the top most and bottom most two values, i.e. 0, 1, 254 and 255 on
            // the assumption that these are likely invalid values for a general normal map
            const float ColorComponentMinVectorThreshold = (2.0f / 255.0f) * 2.0f - 1.0f;
            const float ColorComponentMaxVectorThreshold = (253.0f / 255.0f) * 2.0f - 1.0f;

            // This is the threshold delta length for a vector to be considered as a unit vector
            const float NormalVectorUnitLengthDeltaThreshold = 0.45f;

            // Rejected to taken sample ratio threshold.
            const float RejectedToTakenRatioThreshold = 0.33f;

            void EvaluateSubBlock(StbImageSharp.ImageResult image, int Left, int Top, int Width, int Height)
            {
                for (int Y = Top; Y != (Top + Height); Y++)
                {
                    for (int X = Left; X != (Left + Width); X++)
                    {
                        var ColorSample = image.GetPixel(X, Y).ToColor4Float();
                        if (image.Comp == ColorComponents.RedGreenBlue || image.Comp == ColorComponents.Grey)
                            ColorSample.Alpha = 1.0f;

                        // Nearly black or transparent pixels don't contribute to the calculation
                        if ((ColorSample.Alpha - AlphaComponentNearlyZeroThreshold) < MathHelper.Epsilon || 
                            ColorSample.IsAlmostBlack())
                        {
                            continue;
                        }

                        // Scale and bias, if required, to get a signed vector
                        float Vx = ColorSample.Red * 2.0f - 1.0f;
                        float Vy = ColorSample.Green * 2.0f - 1.0f;
                        float Vz = ColorSample.Blue * 2.0f - 1.0f;

                        float Length = MathHelper.Sqrt(Vx * Vx + Vy * Vy + Vz * Vz);
                        if (Length < ColorComponentNearlyZeroThreshold)
                        {
                            // mid-grey pixels representing (0,0,0) are also not considered as they may be used to denote unused areas
                            continue;
                        }

                        // If the vector is sufficiently different in length from a unit vector, consider it invalid.
                        if (MathHelper.Abs(Length - 1.0f) > NormalVectorUnitLengthDeltaThreshold)
                        {
                            NumSamplesRejected++;
                            continue;
                        }

                        // If the vector is pointing backwards then it is an invalid sample, so consider it invalid
                        if (Vz < 0.0f)
                        {
                            NumSamplesRejected++;
                            continue;
                        }

                        AverageColor = AverageColor + ColorSample;
                        NumSamplesTaken++;
                    }
                }
            }

            /**
             * DoesTextureLookLikelyToBeANormalMap
             *
             * Makes a best guess as to whether a texture represents a normal map or not.
             * Will not be 100% accurate, but aims to be as good as it can without usage
             * information or relying on naming conventions.
             *
             * The heuristic takes samples in small blocks across the texture (if the texture
             * is large enough). The assumption is that if the texture represents a normal map
             * then the average direction of the resulting vector should be somewhere near {0,0,1}.
             * It samples in a number of blocks spread out to decrease the chance of hitting a
             * single unused/blank area of texture, which could happen depending on uv layout.
             *
             * Any pixels that are black, mid-gray or have a red or green value resulting in X or Y
             * being -1 or +1 are ignored on the grounds that they are invalid values. Artists
             * sometimes fill the unused areas of normal maps with color being the {0,0,1} vector,
             * but that cannot be relied on - those areas are often black or gray instead.
             *
             * If the heuristic manages to sample enough valid pixels, the threshold being based
             * on the total number of samples it will be looking at, then it takes the average
             * vector of all the sampled pixels and checks to see if the length and direction are
             * within a specific tolerance. See the namespace at the top of the file for tolerance
             * value specifications. If the vector satisfies those tolerances then the texture is
             * considered to be a normal map.
             */
            public bool DoesTextureLookLikelyToBeANormalMap(StbImageSharp.ImageResult image)
            {
                int TextureSizeX = image.Width;
                int TextureSizeY = image.Height;

                // Calculate the number of tiles in each axis, but limit the number
                // we interact with to a maximum of 16 tiles (4x4)
                int NumTilesX = Math.Min(TextureSizeX / SampleTileEdgeLength, MaxTilesPerAxis);
                int NumTilesY = Math.Min(TextureSizeY / SampleTileEdgeLength, MaxTilesPerAxis);

                //if (!Sampler.SetSourceTexture(Texture))
                //{
                //    return false;
                //}

                if ((NumTilesX > 0) &&
                    (NumTilesY > 0))
                {
                    // If texture is large enough then take samples spread out across the image
                    NumSamplesThreshold = (NumTilesX * NumTilesY) * 4; // on average 4 samples per tile need to be valid...

                    for (int TileY = 0; TileY < NumTilesY; TileY++)
                    {
                        int Top = (TextureSizeY / NumTilesY) * TileY;

                        for (int TileX = 0; TileX < NumTilesX; TileX++)
                        {
                            int Left = (TextureSizeX / NumTilesX) * TileX;

                            EvaluateSubBlock(image, Left, Top, SampleTileEdgeLength, SampleTileEdgeLength);
                        }
                    }
                }
                else
                {
                    NumSamplesThreshold = (TextureSizeX * TextureSizeY) / 4;

                    // Texture is small enough to sample all texels
                    EvaluateSubBlock(image, 0, 0, TextureSizeX, TextureSizeY);
                }

                // if we managed to take a reasonable number of samples then we can evaluate the result
                if (NumSamplesTaken >= NumSamplesThreshold)
                {
                    float RejectedToTakenRatio = (float)(NumSamplesRejected) / (float)(NumSamplesTaken);
                    if (RejectedToTakenRatio >= RejectedToTakenRatioThreshold)
                    {
                        // Too many invalid samples, probably not a normal map
                        return false;
                    }

                    AverageColor = AverageColor * (1.0f/(float)NumSamplesTaken);

                    // See if the resulting vector lies anywhere near the {0,0,1} vector
                    float Vx = AverageColor.Red * 2.0f - 1.0f;
                    float Vy = AverageColor.Green * 2.0f - 1.0f;
                    float Vz = AverageColor.Blue * 2.0f - 1.0f;

                    float Magnitude = MathHelper.Sqrt(Vx * Vx + Vy * Vy + Vz * Vz);

                    // The normalized value of the Z component tells us how close to {0,0,1} the average vector is
                    float NormalizedZ = Vz / Magnitude;

                    // if the average vector is longer than or equal to the min length, shorter than the max length
                    // and the normalized Z value means that the vector is close enough to {0,0,1} then we consider
                    // this a normal map
                    return ((Magnitude >= NormalMapMinLengthConfidenceThreshold) &&
                            (Magnitude < NormalMapMaxLengthConfidenceThreshold) &&
                            (NormalizedZ >= NormalMapDeviationThreshold));
                }

                // Not enough samples, don't trust the result at all
                return false;
            }

            int NumSamplesTaken;
            int NumSamplesRejected;
            int NumSamplesThreshold;
            Color4f AverageColor;
        }
        public class ImportAttribute : IO.IAssetCreateAttribute
        {
            bool bPopOpen = false;
            bool bFileExisting = false;
            RName mDir;
            string mName;
            string mSourceFile;
            public TtPicDesc mDesc = new TtPicDesc();
            ImGui.ImGuiFileDialog mFileDialog = TtEngine.Instance.EditorInstance.FileDialog.mFileDialog;
            EGui.Controls.PropertyGrid.PropertyGrid PGAsset = new EGui.Controls.PropertyGrid.PropertyGrid();
            public override async Thread.Async.TtTask DoCreate(RName dir, Rtti.TtTypeDesc type, string ext)
            {
                mDir = dir;
                mDesc.Desc.SetDefault();
                var noused = PGAsset.Initialize();
                PGAsset.Target = mDesc;
            }
            public unsafe void _DumpBasicPicDesc()
            {
                using (var stream = System.IO.File.OpenRead(mSourceFile))
                {
                    var image = StbImageSharp.ImageResult.FromStream(stream, StbImageSharp.ColorComponents.Default);
                    if (image != null)
                    {
                        mDesc.Width = image.Width;
                        mDesc.Height = image.Height;

                        mDesc.MipLevel = Math.Max(CalcMipLevel(mDesc.Width, mDesc.Height, true)-2, 1);
                    }
                }
            }
            public override unsafe bool OnDraw(EGui.Controls.TtContentBrowser ContentBrowser)
            {
                if (bPopOpen == false)
                    ImGuiAPI.OpenPopup($"Import SRV", ImGuiPopupFlags_.ImGuiPopupFlags_None);
                bool retValue = false;
                var visible = true;
                ImGuiAPI.SetNextWindowSize(new Vector2(200, 500), ImGuiCond_.ImGuiCond_FirstUseEver);
                if (ImGuiAPI.BeginPopupModal($"Import SRV", &visible, ImGuiWindowFlags_.ImGuiWindowFlags_None))
                {
                    if(string.IsNullOrEmpty(ContentBrowser.CurrentImporterFile))
                    {
                        var sz = new Vector2(-1, 0);
                        if (ImGuiAPI.Button("Select Image", in sz))
                        {
                            mFileDialog.OpenModal("ChooseFileDlgKey", "Choose File", ".png,.PNG,.jpg,.JPG,.bmp,.BMP,.tga,.TGA,.exr,.EXR,.hdr,.HDR", ".");
                        }
                        // display
                        if (mFileDialog.DisplayDialog("ChooseFileDlgKey"))
                        {
                            // action if OK
                            if (mFileDialog.IsOk() == true)
                            {
                                mSourceFile = mFileDialog.GetFilePathName();
                                mName = IO.TtFileManager.GetPureName(mSourceFile);
                                _DumpBasicPicDesc();
                            }
                            // close
                            mFileDialog.CloseDialog();
                        }
                    }
                    else if(string.IsNullOrEmpty(mSourceFile))
                    {
                        mSourceFile = ContentBrowser.CurrentImporterFile;
                        mName = IO.TtFileManager.GetPureName(mSourceFile);
                        _DumpBasicPicDesc();
                    }

                    if (bFileExisting)
                    {
                        var clr = new Vector4(1, 0, 0, 1);
                        ImGuiAPI.TextColored(in clr, $"Source:{mSourceFile}");
                    }
                    else
                    {
                        var clr = new Vector4(1, 1, 1, 1);
                        ImGuiAPI.TextColored(in clr, $"Source:{mSourceFile}");
                    }
                    ImGuiAPI.Separator();

                    using (var buffer = BigStackBuffer.CreateInstance(128))
                    {
                        buffer.SetTextUtf8(mName);
                        ImGuiAPI.InputText("##in_rname", buffer.GetBuffer(), (uint)buffer.GetSize(), ImGuiInputTextFlags_.ImGuiInputTextFlags_None, null, (void*)0);
                        var name = buffer.AsTextUtf8();
                        if (mName != name)
                        {
                            mName = name;
                            bFileExisting = IO.TtFileManager.FileExists(mDir.Address + mName + NxRHI.TtSrView.AssetExt);
                        }
                    }

                    var btSz = Vector2.Zero;
                    if (bFileExisting == false)
                    {
                        if (ImGuiAPI.Button("Create Asset", in btSz))
                        {
                            if (ImportImage())
                            {
                                ImGuiAPI.CloseCurrentPopup();
                                retValue = true;
                            }
                        }
                        ImGuiAPI.SameLine(0, 20);
                    }
                    if (ImGuiAPI.Button("Cancel", in btSz))
                    {
                        ImGuiAPI.CloseCurrentPopup();
                        retValue = true;
                    }

                    ImGuiAPI.Separator();

                    PGAsset.OnDraw(false, false, false);

                    ImGuiAPI.EndPopup();
                }
                if (!visible)
                    retValue = true;
                return retValue;
            }
            private unsafe bool ImportImage()
            {
                using (var stream = System.IO.File.OpenRead(mSourceFile))
                {
                    if (stream == null)
                        return false;

                    var extName = IO.TtFileManager.GetExtName(mSourceFile);
                    var rn = RName.GetRName(mDir.Name + mName + TtSrView.AssetExt, mDir.RNameType);
                    var xnd = new IO.TtXndHolder("USrView", 0, 0);

                    if (extName.ToLower() == ".hdr")
                    {
                        var imageFloat = StbImageSharp.ImageResultFloat.FromStream(stream, StbImageSharp.ColorComponents.RedGreenBlueAlpha);
                        if (imageFloat == null)
                            return false;

                        StbImageSharp.ImageResultFloat processedImage = null;
                        if (mDesc.CubeFaces == 6)
                        {
                            TtSrView.GenerateBaseCubeMipFromLongitudeLatitude2D(ref processedImage, imageFloat, 512);
                        }
                        else
                            processedImage = imageFloat;

                        TtSrView.SaveTexture(rn, xnd.RootNode.mCoreObject, processedImage, mDesc);
                    }
                    else if (extName.ToLower() == ".exr")
                    {
                        var file = new Jither.OpenEXR.EXRFile(stream);
                        if (file.Parts.Count == 0)
                            return false;

                        TtSrView.SaveTexture(rn, xnd.RootNode.mCoreObject, file, mDesc);
                    }
                    else
                    {
                        ImageResult image = null;
                        image = StbImageSharp.ImageResult.FromStream(stream, StbImageSharp.ColorComponents.Default);
                        if (image == null)
                            return false;

                        if(mDesc.AutoCheckNormal == true)
                        {
                            NormalmapChecker normalChecker = new NormalmapChecker();
                            mDesc.IsNormal = normalChecker.DoesTextureLookLikelyToBeANormalMap(image);
                        }

                        if(mDesc.MipLevel==0)
                        {
                            if (mDesc.Height < 64 && mDesc.Width < 64)
                                mDesc.MipLevel = 1;
                        }

                        TtSrView.SaveTexture(rn, xnd.RootNode.mCoreObject, image, mDesc);
                    }

                    xnd.SaveXnd(rn.Address);

                    var ameta = new TtSrViewAMeta();
                    ameta.SetAssetName(rn);
                    ameta.AssetId = Guid.NewGuid();
                    ameta.TypeStr = Rtti.TtTypeDescManager.Instance.GetTypeStringFromType(typeof(TtSrView));
                    ameta.Description = $"This is a {typeof(TtSrView).FullName}\n";
                    ameta.SaveAMeta((IAsset)null);

                    TtEngine.Instance.AssetMetaManager.RegAsset(ameta);
                }
                return true;
            }

            public static bool ImportImage(string sourceFile, RName dir, TtPicDesc desc)
            {
                using (var stream = System.IO.File.OpenRead(sourceFile))
                {
                    if (stream == null)
                        return false;
                    var image = StbImageSharp.ImageResult.FromStream(stream, StbImageSharp.ColorComponents.RedGreenBlueAlpha);
                    if (image == null)
                        return false;

                    var name = IO.TtFileManager.GetPureName(sourceFile);
                    var rn = RName.GetRName(dir.Name.TrimEnd('\\').TrimEnd('/') + "/" + name + TtSrView.AssetExt, dir.RNameType);

                    return SaveSrv(image, rn, desc);
                }
            }

            public static bool SaveSrv(Jither.OpenEXR.EXRFile file, RName rn, TtPicDesc desc)
            {
                var part = file.Parts[0];
                System.Diagnostics.Debug.Assert(part.DataReader != null);

                desc.Width = part.DisplayWindow.Width;
                desc.Height = part.DisplayWindow.Height;

                var xnd = new IO.TtXndHolder("USrView", 0, 0);
                TtSrView.SaveTexture(rn, xnd.RootNode.mCoreObject, file, desc);
                xnd.SaveXnd(rn.Address);

                var ameta = new TtSrViewAMeta();
                ameta.SetAssetName(rn);
                ameta.AssetId = Guid.NewGuid();
                ameta.TypeStr = Rtti.TtTypeDescManager.Instance.GetTypeStringFromType(typeof(TtSrView));
                ameta.Description = $"This is a {typeof(TtSrView).FullName}\n";
                ameta.SaveAMeta((IO.IAsset)null);

                TtEngine.Instance.AssetMetaManager.RegAsset(ameta);
                TtEngine.Instance.SourceControlModule.AddFile(rn.Address);

                return true;
            }

            public static bool SaveSrv(StbImageSharp.ImageResultFloat image, RName rn, TtPicDesc desc)
            {
                desc.Width = image.Width;
                desc.Height = image.Height;

                var xnd = new IO.TtXndHolder("USrView", 0, 0);
                TtSrView.SaveTexture(rn, xnd.RootNode.mCoreObject, image, desc);
                xnd.SaveXnd(rn.Address);

                var ameta = new TtSrViewAMeta();
                ameta.SetAssetName(rn);
                ameta.AssetId = Guid.NewGuid();
                ameta.TypeStr = Rtti.TtTypeDescManager.Instance.GetTypeStringFromType(typeof(TtSrView));
                ameta.Description = $"This is a {typeof(TtSrView).FullName}\n";
                ameta.SaveAMeta((IO.IAsset)null);

                TtEngine.Instance.AssetMetaManager.RegAsset(ameta);
                TtEngine.Instance.SourceControlModule.AddFile(rn.Address);
                return true;
            }

            public static bool SaveSrv(StbImageSharp.ImageResult image, RName rn, TtPicDesc desc)
            {
                desc.Width = image.Width;
                desc.Height = image.Height;

                var xnd = new IO.TtXndHolder("USrView", 0, 0);
                TtSrView.SaveTexture(rn, xnd.RootNode.mCoreObject, image, desc);
                xnd.SaveXnd(rn.Address);

                var ameta = new TtSrViewAMeta();
                ameta.SetAssetName(rn);
                ameta.AssetId = Guid.NewGuid();
                ameta.TypeStr = Rtti.TtTypeDescManager.Instance.GetTypeStringFromType(typeof(TtSrView));
                ameta.Description = $"This is a {typeof(TtSrView).FullName}\n";
                ameta.SaveAMeta((IO.IAsset)null);

                TtEngine.Instance.AssetMetaManager.RegAsset(ameta);
                TtEngine.Instance.SourceControlModule.AddFile(rn.Address);
                return true;
            }

            public override bool IsAssetSource(string fileExt)
            {
                fileExt = fileExt.TrimStart('.').ToLower();
                switch (fileExt)
                {
                    case "png":
                    case "jpg":
                    case "jpeg":
                    case "bmp":
                    case "tga":
                    case "exr":
                    case "hdr":
                        return true;
                }

                return false;
            }
            public override void ImportSource(string sourceFile, RName dir)
            {
                ImportImage(sourceFile, dir, new TtPicDesc());
            }
        }

        public ITexture GetTexture()
        {
            return mCoreObject.GetBufferAsTexture();
        }
        public IBuffer GetBuffer()
        {
            return mCoreObject.GetBufferAsBuffer();
        }

        #region IAsset
        public const string AssetExt = ".srv";
        public string TypeExt { get => AssetExt; }
        public IO.IAssetMeta CreateAMeta()
        {
            var result = new TtSrViewAMeta();
            return result;
        }
        public IO.IAssetMeta GetAMeta()
        {
            return TtEngine.Instance.AssetMetaManager.GetAssetMeta(AssetName);
        }
        public void UpdateAMetaReferences(IO.IAssetMeta ameta)
        {
            ameta.RefAssetRNames.Clear();
        }
        object mOriginImageObject = null;
        internal void LoadOriginImageObject()
        {
            var imgType = GetOriginImageType(this.AssetName);
            switch (imgType)
            {
                case EngienNS.Bricks.ImageDecoder.UImageType.PNG:
                    {
                        mOriginImageObject = LoadOriginPng(this.AssetName);
                    }
                    break;
                case EngienNS.Bricks.ImageDecoder.UImageType.HDR:
                    {
                        StbImageSharp.ImageResultFloat imageFloat = new StbImageSharp.ImageResultFloat();
                        LoadOriginHdr(AssetName, ref imageFloat);
                        mOriginImageObject = imageFloat;
                    }
                    break;
                case EngienNS.Bricks.ImageDecoder.UImageType.EXR:
                    {
                        System.IO.Stream outStream = null;
                        mOriginImageObject = LoadOriginExr(AssetName, ref outStream);
                    }
                    break;
                default:
                    mOriginImageObject = null;
                    break;
            }
        }
        internal void FreeOriginImageObject()
        {
            mOriginImageObject = null;
        }
        public void SaveAssetTo(RName name)
        {
            if (mOriginImageObject != null)
            {
                if (mOriginImageObject.GetType() == typeof(ImageResult))
                {
                    ImportAttribute.SaveSrv(mOriginImageObject as ImageResult, name, this.PicDesc);
                }
                else if (mOriginImageObject.GetType() == typeof(ImageResultFloat))
                {
                    ImportAttribute.SaveSrv(mOriginImageObject as ImageResultFloat, name, this.PicDesc);
                }
                else if (mOriginImageObject.GetType() == typeof(EXRFile))
                {
                    ImportAttribute.SaveSrv(mOriginImageObject as EXRFile, name, this.PicDesc);
                }
                return;
            }

            var imgType = GetOriginImageType(this.AssetName);
            switch (imgType)
            {
                case EngienNS.Bricks.ImageDecoder.UImageType.PNG:
                    {
                        var image = LoadOriginPng(this.AssetName);
                        if (image == null)
                        {
                            Profiler.Log.WriteLine<Profiler.TtGraphicsGategory>(Profiler.ELogTag.Warning, $"SaveAssetTo failed: LoadOriginImage({AssetName}) = null");
                            return;
                        }
                        ImportAttribute.SaveSrv(image, name, this.PicDesc);
                    }
                    break;
                case EngienNS.Bricks.ImageDecoder.UImageType.HDR:
                    {
                        StbImageSharp.ImageResultFloat imageFloat = new StbImageSharp.ImageResultFloat();
                        LoadOriginHdr(AssetName, ref imageFloat);
                        ImportAttribute.SaveSrv(imageFloat, name, this.PicDesc);
                    }
                    break;
                case EngienNS.Bricks.ImageDecoder.UImageType.EXR:
                    {
                        System.IO.Stream outStream = null;
                        var file = LoadOriginExr(AssetName, ref outStream);
                        if(file == null)
                        {
                            Profiler.Log.WriteLine<Profiler.TtEditorGategory>(Profiler.ELogTag.Warning, $"SaveAssetTo failed: LoadOriginImage({AssetName}) = null");
                            return;
                        }
                        ImportAttribute.SaveSrv(file, name, this.PicDesc);
                    }
                    break;
            }

            var ameta = this.GetAMeta();
            if (ameta != null)
            {
                UpdateAMetaReferences(ameta);
                ameta.SaveAMeta(this);
            }
        }
        [Rtti.Meta]
        public RName AssetName
        {
            get;
            set;
        }
        #endregion

        #region TextureHandle
        IntPtr mTextureHandle = IntPtr.Zero;
        public bool IsHandle()
        {
            return mTextureHandle != IntPtr.Zero;
        }
        public IntPtr GetTextureHandle()
        {
            if (mTextureHandle == IntPtr.Zero)
            {
                mTextureHandle = System.Runtime.InteropServices.GCHandle.ToIntPtr(
                    System.Runtime.InteropServices.GCHandle.Alloc(this, System.Runtime.InteropServices.GCHandleType.Weak));
                System.Threading.Interlocked.Increment(ref NumOfGCHandle);
            }
            return mTextureHandle;
        }
        public void FreeTextureHandle()
        {
            if (mTextureHandle != IntPtr.Zero)
            {
                var handle = System.Runtime.InteropServices.GCHandle.FromIntPtr(mTextureHandle);
                handle.Free();
                mTextureHandle = IntPtr.Zero;
                System.Threading.Interlocked.Decrement(ref NumOfGCHandle);
            }
        }
        public override void Dispose()
        {
            FreeTextureHandle();
            base.Dispose();
        }
        #endregion

        #region IStreaming
        public int LevelOfDetail { get; set; }
        public int TargetLOD { get; set; }
        public int MaxLOD
        {
            get
            {
                return PicDesc.MipLevel;
            }
        }
        [Browsable(false)]
        public System.Threading.Tasks.Task<bool> CurLoadTask { get; set; }
        public async System.Threading.Tasks.Task<bool> LoadLOD(int level)
        {
            if (level == 0)
            {
                return false;
            }
            if (level < 0 || level > MaxLOD)
                return false;
            var oldTexture = StreamingTexture;
            StreamingTexture = await TtEngine.Instance.EventPoster.Post((state) =>
            {
                var xnd = IO.TtXndHolder.LoadXnd(AssetName.Address);
                if (xnd == null)
                    return null;

                return LoadTexture2DMipLevel(xnd.RootNode, this.PicDesc, level, oldTexture);
            }, Thread.Async.EAsyncTarget.AsyncIO);

            var rc = TtEngine.Instance.GfxDevice.RenderContext;
            LevelOfDetail = level;
            unsafe
            {
                if (StreamingTexture == null)
                {
                    //return this.mCoreObject.UpdateBuffer(rc.mCoreObject, new IGpuBufferData());
                    return false;
                }
                else
                {
                    //var desc = this.mCoreObject.Desc;
                    //desc.Texture2D.MipLevels = tex2d.mCoreObject.Desc.MipLevels;
                    //var srv = rc.mCoreObject.CreateSRV(tex2d.mCoreObject.NativeSuper, in desc);
                    //var fp = this.mCoreObject.NativeSuper.GetFingerPrint();
                    //this.Core_Release();
                    //this.mCoreObject = srv;
                    //this.mCoreObject.NativeSuper.SetFingerPrint(fp + 1);
                    //return true;
                    return this.mCoreObject.UpdateBuffer(rc.mCoreObject, StreamingTexture.mCoreObject.NativeSuper);
                }
            }
        }
        #endregion

        public static UImageType GetOriginImageType(RName name)
        {
            using (var xnd = IO.TtXndHolder.LoadXnd(name.Address))
            {
                if(xnd==null)
                {
                    if (System.IO.File.Exists(name.Address + ".png") == true)
                        return UImageType.PNG;
                    else if (System.IO.File.Exists(name.Address + ".hdr") == true)
                        return UImageType.HDR;
                    else if (System.IO.File.Exists(name.Address + ".exr") == true)
                        return UImageType.EXR;
                }
                var attr = xnd.RootNode.TryGetAttribute("Png");
                if(attr.IsValidPointer)
                    return UImageType.PNG;
                attr = xnd.RootNode.TryGetAttribute("Hdr");
                if (attr.IsValidPointer)
                    return UImageType.HDR;
                attr = xnd.RootNode.TryGetAttribute("Exr");
                if (attr.IsValidPointer)
                    return UImageType.EXR;
            }
            return UImageType.PNG;
        }

        public static Jither.OpenEXR.EXRFile LoadOriginExr(RName name, ref System.IO.Stream outStream)
        {
            using (var xnd = IO.TtXndHolder.LoadXnd(name.Address))
            {
                if (xnd == null)
                {
                    var sourceFile = name.Address + ".exr";
                    outStream = System.IO.File.OpenRead(sourceFile);
                    {
                        if (outStream == null)
                            return null;
                        return new Jither.OpenEXR.EXRFile(outStream);
                    }
                }
                var attr = xnd.RootNode.TryGetAttribute("Exr");
                if (attr.IsValidPointer)
                {
                    byte[] rawData;
                    using (var ar = attr.GetReader(null))
                    {
                        ar.ReadNoSize(out rawData, (int)attr.GetReaderLength());
                    }

                    outStream = new System.IO.MemoryStream(rawData);
                    {
                        var file = new Jither.OpenEXR.EXRFile(outStream);
                        return file;
                    }
                }
            }
            return null;
        }


        public static void LoadOriginHdr(RName name, ref StbImageSharp.ImageResultFloat outImage)
        {
            using (var xnd = IO.TtXndHolder.LoadXnd(name.Address))
            {
                if (xnd == null)
                {
                    var sourceFile = name.Address + ".hdr";
                    using (var stream = System.IO.File.OpenRead(sourceFile))
                    {
                        if (stream == null)
                            return;
                        outImage = StbImageSharp.ImageResultFloat.FromStream(stream, StbImageSharp.ColorComponents.RedGreenBlueAlpha);
                    }
                }
                var attr = xnd.RootNode.TryGetAttribute("Hdr");
                if (attr.IsValidPointer)
                {
                    byte[] rawData;
                    using (var ar = attr.GetReader(null))
                    {
                        ar.ReadNoSize(out rawData, (int)attr.GetReaderLength());
                    }

                    using (var memStream = new System.IO.MemoryStream(rawData))
                    {
                        outImage = StbImageSharp.ImageResultFloat.FromStream(memStream, StbImageSharp.ColorComponents.RedGreenBlueAlpha);
                        return;
                    }
                }
            }
        }

        public static StbImageSharp.ImageResult LoadOriginPng(RName name)
        {
            using (var xnd = IO.TtXndHolder.LoadXnd(name.Address))
            {
                if (xnd == null)
                {
                    var sourceFile = name.Address + ".png";
                    using (var stream = System.IO.File.OpenRead(sourceFile))
                    {
                        if (stream == null)
                            return null;
                        return StbImageSharp.ImageResult.FromStream(stream, StbImageSharp.ColorComponents.RedGreenBlueAlpha);
                    }
                }
                var attr = xnd.RootNode.TryGetAttribute("OriginSource");
                if (attr.IsValidPointer == false)
                {
                    attr = xnd.RootNode.TryGetAttribute("Png");
                }
                if (attr.IsValidPointer)
                {
                    byte[] pngData;
                    using (var ar = attr.GetReader(null))
                    {
                        ar.ReadNoSize(out pngData, (int)attr.GetReaderLength());
                    }

                    using (var memStream = new System.IO.MemoryStream(pngData))
                    {
                        var image = StbImageSharp.ImageResult.FromStream(memStream, StbImageSharp.ColorComponents.Default);
                        return image;
                    }
                }
                else
                {
                    var sourceFile = name.Address + ".png";
                    if (System.IO.File.Exists(sourceFile) == false)
                    {
                        Profiler.Log.WriteLine<Profiler.TtEditorGategory>(Profiler.ELogTag.Warning, $"LoadOriginImage({name}) failed");
                        return null;
                    }
                    using (var stream = System.IO.File.OpenRead(sourceFile))
                    {
                        if (stream == null)
                            return null;
                        return StbImageSharp.ImageResult.FromStream(stream, StbImageSharp.ColorComponents.RedGreenBlueAlpha);
                    }
                }
            }
        }
        #region static function
        public static int CalcMipLevel(int width, int height, bool isAnyZero)
        {
            int mipLevel = 0;
            do
            {
                height = height / 2;
                width = width / 2;
                mipLevel++;

                if (isAnyZero)
                {
                    if ((height == 0 || width == 0))
                    {
                        break;
                    }
                }
                else
                {
                    if ((height == 0 && width == 0))
                    {
                        break;
                    }
                }

                if (height == 0)
                {
                    height = 1;
                }
                if (width == 0)
                {
                    width = 1;
                }
            }
            while (true);
            return mipLevel;
        }
        public static unsafe void SaveTexture(RName assetName, XndNode node, Jither.OpenEXR.EXRFile file, TtPicDesc desc)
        {
            var part = file.Parts[0];
            System.Diagnostics.Debug.Assert(part.DataReader != null);
            byte[] pixelData = new byte[part.DataReader.GetTotalByteCount()];
            string[] channelNames = new[] { "R", "G", "B", "A" };
            if (part.Channels.Count == 3)
                channelNames = new[] { "R", "G", "B" };
            part.DataReader.ReadInterleaved(pixelData, channelNames);

            desc.Width = part.DisplayWindow.Width;
            desc.Height = part.DisplayWindow.Height;

            if (desc.StripOriginSource && assetName != null)
            {
                using (var memStream = new System.IO.FileStream(assetName.Address + ".exr", System.IO.FileMode.OpenOrCreate))
                {
                    file.Write(memStream);
                    part.DataWriter.WriteInterleaved(pixelData, channelNames);
                }
            }
            else
            {
                using (var memStream = new System.IO.MemoryStream())
                {
                    file.Write(memStream);
                    part.DataWriter.WriteInterleaved(pixelData, channelNames);

                    var rawData = memStream.ToArray();
                    var rawAttr = node.GetOrAddAttribute("Exr", 0, 0);
                    using (var ar = rawAttr.GetWriter((ulong)memStream.Position))
                    {
                        ar.WriteNoSize(rawData, (int)memStream.Position);
                    }
                }
            }

            // TODO: suport compress exr
            desc.DontCompress = true;
            if (desc.Width % 4 != 0 || desc.Height % 4 != 0)
            {
                desc.DontCompress = true;
            }
            int mipLevel = 0;
            desc.MipSizes.Clear();
            if (desc.DontCompress == false)
            {
                if (TtEngine.Instance.Config.CompressDxt)
                {
                    desc.CompressFormat = ETextureCompressFormat.TCF_BC6;
                }
                else if (TtEngine.Instance.Config.CompressAstc)
                {
                    System.Diagnostics.Debug.Assert(false);
                    desc.CompressFormat = ETextureCompressFormat.TCF_Astc_4x4_Float;
                }
                else
                {
                    System.Diagnostics.Debug.Assert(false);
                    desc.CompressFormat = ETextureCompressFormat.TCF_None;
                }
            }
            else
            {
                desc.CompressFormat = ETextureCompressFormat.TCF_None;
            }

            switch (desc.CompressFormat)
            {
                case ETextureCompressFormat.TCF_None:
                    {
                        var hdrMipsNode = node.GetOrAddNode("ExrMips", 0, 0);
                        mipLevel = SaveExrMips(hdrMipsNode, file, desc);
                    }
                    break;
            }

            {
                desc.Desc.MipLevel = mipLevel;
                desc.Desc.dwStructureSize = (uint)sizeof(FPictureDesc);
                var attr = node.GetOrAddAttribute("Desc", 2, 0);
                using (var ar = attr.GetWriter((ulong)sizeof(FPictureDesc)))
                {
                    ar.Write(desc.Desc);
                    ar.Write(desc.MipSizes.Count);
                    for (int i = 0; i < desc.MipSizes.Count; i++)
                    {
                        ar.Write(desc.MipSizes[i]);
                        ar.Write(desc.BlockDimenstions[i]);
                    }
                    ar.Write(desc.BlockSize);
                }
            }
        }
        
        public static unsafe void SaveTexture(RName assetName, XndNode node, StbImageSharp.ImageResultFloat image, TtPicDesc desc)
        {
            var writeComp = UStbImageUtility.ConvertColorComponent(image.Comp);
            desc.Height = image.Height;
            desc.Width = image.Width;
            if (desc.StripOriginSource && assetName != null)
            {
                using (var memStream = new System.IO.FileStream(assetName.Address + ".hdr", System.IO.FileMode.OpenOrCreate))
                {
                    var writer = new StbImageWriteSharp.ImageWriter();
                    fixed (void* fptr = image.Data)
                    {
                        writer.WriteHdr(fptr, image.Width, image.Height, writeComp, memStream);
                    }
                }
            }
            else
            {
                if (desc.IsAutoSaveSrcImage == true)
                {
                    using (var memStream = new System.IO.MemoryStream())
                    {
                        var writer = new StbImageWriteSharp.ImageWriter();
                        fixed (void* fptr = image.Data)
                        {
                            writer.WriteHdr(fptr, image.Width, image.Height, writeComp, memStream);
                        }
                        var rawData = memStream.ToArray();


                        var rawAttr = node.GetOrAddAttribute("Hdr", 0, 0);
                        using (var ar = rawAttr.GetWriter((ulong)memStream.Position))
                        {
                            ar.WriteNoSize(rawData, (int)memStream.Position);
                        }
                    }
                }
            }

            if (image.Width % 4 != 0 || image.Height % 4 != 0)
            {
                desc.DontCompress = true;
            }
            int mipLevel = 0;
            var curImage = image;
            desc.MipSizes.Clear();
            if (desc.DontCompress == false)
            {
                if (TtEngine.Instance.Config.CompressDxt)
                {
                    desc.CompressFormat = ETextureCompressFormat.TCF_BC6;
                }
                else if (TtEngine.Instance.Config.CompressAstc)
                {
                    System.Diagnostics.Debug.Assert(false);
                    desc.CompressFormat = ETextureCompressFormat.TCF_Astc_4x4_Float;
                }
                else
                {
                    System.Diagnostics.Debug.Assert(false);
                    desc.CompressFormat = ETextureCompressFormat.TCF_None;
                }
            }
            else
            {
                desc.CompressFormat = ETextureCompressFormat.TCF_None;
            }
            switch (desc.CompressFormat)
            {
                case ETextureCompressFormat.TCF_None:
                    {
                        var hdrMipsNode = node.GetOrAddNode("HdrMips", 0, 0);
                        mipLevel = SaveHdrMips(hdrMipsNode, curImage, desc);
                    }
                    break;
                case ETextureCompressFormat.TCF_BC6:
                    {
                        var pngMipsNode = node.GetOrAddNode("DxtMips", 0, 0);
                        mipLevel = SaveDxtMips_BcEncoder(pngMipsNode, curImage, desc);
                    }
                    break;
                case ETextureCompressFormat.TCF_Astc_4x4:
                case ETextureCompressFormat.TCF_Astc_4x4_Float:
                case ETextureCompressFormat.TCF_Astc_5x4:
                case ETextureCompressFormat.TCF_Astc_5x4_Float:
                case ETextureCompressFormat.TCF_Astc_5x5:
                case ETextureCompressFormat.TCF_Astc_5x5_Float:
                case ETextureCompressFormat.TCF_Astc_6x5:
                case ETextureCompressFormat.TCF_Astc_6x5_Float:
                case ETextureCompressFormat.TCF_Astc_6x6:
                case ETextureCompressFormat.TCF_Astc_6x6_Float:
                case ETextureCompressFormat.TCF_Astc_8x5:
                case ETextureCompressFormat.TCF_Astc_8x5_Float:
                case ETextureCompressFormat.TCF_Astc_8x6:
                case ETextureCompressFormat.TCF_Astc_8x6_Float:
                case ETextureCompressFormat.TCF_Astc_8x8:
                case ETextureCompressFormat.TCF_Astc_8x8_Float:
                case ETextureCompressFormat.TCF_Astc_10x6:
                case ETextureCompressFormat.TCF_Astc_10x6_Float:
                case ETextureCompressFormat.TCF_Astc_10x8:
                case ETextureCompressFormat.TCF_Astc_10x8_Float:
                case ETextureCompressFormat.TCF_Astc_10x10:
                case ETextureCompressFormat.TCF_Astc_10x10_Float:
                case ETextureCompressFormat.TCF_Astc_12x10:
                case ETextureCompressFormat.TCF_Astc_12x10_Float:
                case ETextureCompressFormat.TCF_Astc_12x12:
                case ETextureCompressFormat.TCF_Astc_12x12_Float:
                    {
                        var pngMipsNode = node.GetOrAddNode("AstcMips", 0, 0);
                        //mipLevel = SaveAstcMips_ActcEncoder(pngMipsNode, curImage, desc);
                    }
                    break;
            }

            {
                desc.Desc.MipLevel = mipLevel;
                desc.Desc.dwStructureSize = (uint)sizeof(FPictureDesc);
                var attr = node.GetOrAddAttribute("Desc", 2, 0);
                using (var ar = attr.GetWriter((ulong)sizeof(FPictureDesc)))
                {
                    ar.Write(desc.Desc);
                    ar.Write(desc.MipSizes.Count);
                    for (int i = 0; i < desc.MipSizes.Count; i++)
                    {
                        ar.Write(desc.MipSizes[i]);
                        ar.Write(desc.BlockDimenstions[i]);
                    }
                    ar.Write(desc.BlockSize);
                }
            }
        }
        public static StbImageWriteSharp.ColorComponents GetImageWriteFormat(StbImageSharp.ImageResult image)
        {
            switch(image.Comp)
            {
                case ColorComponents.RedGreenBlue:
                    return StbImageWriteSharp.ColorComponents.RedGreenBlue;
                case ColorComponents.RedGreenBlueAlpha:
                    return StbImageWriteSharp.ColorComponents.RedGreenBlueAlpha;
                case ColorComponents.GreyAlpha:
                    return StbImageWriteSharp.ColorComponents.GreyAlpha;
                case ColorComponents.Grey:
                    return StbImageWriteSharp.ColorComponents.Grey;
            }
            return StbImageWriteSharp.ColorComponents.RedGreenBlue;
        }
        public static unsafe void SaveTexture(RName assetName, XndNode node, StbImageSharp.ImageResult image, TtPicDesc desc)
        {
            desc.Height = image.Height;
            desc.Width = image.Width;
            if (desc.StripOriginSource && assetName != null)
            {
                using (var memStream = new System.IO.FileStream(assetName.Address + ".png", System.IO.FileMode.OpenOrCreate))
                {
                    var writer = new StbImageWriteSharp.ImageWriter();
                    writer.WritePng(image.Data, image.Width, image.Height, GetImageWriteFormat(image), memStream);
                }
            }
            else
            {
                if(desc.IsAutoSaveSrcImage==true)
                {
                    using (var memStream = new System.IO.MemoryStream(image.Data.Length))
                    {
                        var writer = new StbImageWriteSharp.ImageWriter();

                        writer.WritePng(image.Data, image.Width, image.Height, GetImageWriteFormat(image), memStream);
                        var pngData = memStream.ToArray();

                        var size = (uint)memStream.Length;
                        if (size > 0)
                        {
                            var len = CoreSDK.CompressBound_ZSTD(size) + 5;
                            using (var d = BigStackBuffer.CreateInstance((int)len))
                            {
                                void* srcBuffer = System.Runtime.InteropServices.Marshal.UnsafeAddrOfPinnedArrayElement(pngData, 0).ToPointer();
                                var wSize = (uint)CoreSDK.Compress_ZSTD(d.GetBuffer(), len, srcBuffer, size, 1);
                            }
                        }

                        var attr = node.GetOrAddAttribute("Png", 0, 0);
                        using (var ar = attr.GetWriter((ulong)memStream.Position))
                        {
                            ar.WriteNoSize(pngData, (int)memStream.Position);
                        }
                    }
                }
            }

            if (image.Width % 4 != 0 || image.Height % 4 != 0)
            {
                desc.DontCompress = true;
            }
            int mipLevel = 0;
            var curImage = image;
            if (image.Comp == ColorComponents.RedGreenBlue || image.Comp == ColorComponents.Grey)
                desc.BitNumAlpha = 0;
            desc.MipSizes.Clear();
            if (desc.DontCompress == false)
            {
                if (TtEngine.Instance.Config.CompressDxt)
                {
                    if(desc.IsNormal)
                    {
                        desc.CompressFormat = ETextureCompressFormat.TCF_BC5;
                    }
                    else if (desc.BitNumAlpha == 8 || desc.BitNumAlpha == 4)
                    {
                        desc.CompressFormat = ETextureCompressFormat.TCF_Dxt3;
                    }
                    else if (desc.BitNumAlpha == 1)
                    {
                        desc.CompressFormat = ETextureCompressFormat.TCF_Dxt1a;
                    }
                    else
                    {
                        desc.CompressFormat = ETextureCompressFormat.TCF_Dxt1;
                    }
                }
                else if (TtEngine.Instance.Config.CompressEtc)
                {
                    if (desc.BitNumAlpha == 8 || desc.BitNumAlpha == 4)
                    {
                        desc.CompressFormat = ETextureCompressFormat.TCF_Etc2_RGBA8;
                    }
                    else if (desc.BitNumAlpha == 1)
                    {
                        desc.CompressFormat = ETextureCompressFormat.TCF_Etc2_RGBA1;
                    }
                    else
                    {
                        desc.CompressFormat = ETextureCompressFormat.TCF_Etc2_RGB8;
                    }
                }
                else if (TtEngine.Instance.Config.CompressAstc)
                {
                    System.Diagnostics.Debug.Assert(false);
                    desc.CompressFormat = ETextureCompressFormat.TCF_Etc2_RGBA8;
                }
                else
                {
                    System.Diagnostics.Debug.Assert(false);
                    desc.CompressFormat = ETextureCompressFormat.TCF_None;
                }
            }
            else
            {
                desc.CompressFormat = ETextureCompressFormat.TCF_None;
            }
            switch (desc.CompressFormat)
            {
                case ETextureCompressFormat.TCF_None:
                    {
                        var pngMipsNode = node.GetOrAddNode("PngMips", 0, 0);
                        mipLevel = SavePngMips(pngMipsNode, curImage, desc);
                        switch (curImage.Comp)
                        {
                            case StbImageSharp.ColorComponents.RedGreenBlueAlpha:
                            case StbImageSharp.ColorComponents.RedGreenBlue:
                                desc.Format = EPixelFormat.PXF_R8G8B8A8_UNORM;
                                break;
                            case StbImageSharp.ColorComponents.Grey:
                                desc.Format = EPixelFormat.PXF_R8G8B8A8_UNORM;
                                break;
                            case StbImageSharp.ColorComponents.GreyAlpha:
                                desc.Format = EPixelFormat.PXF_R8G8_UNORM;
                                break;
                            default:
                                desc.Format = EPixelFormat.PXF_R8G8B8A8_UNORM;
                                break;
                        }
                    }
                    break;
                case ETextureCompressFormat.TCF_Dxt1://rgb:5-6-5 a:0
                case ETextureCompressFormat.TCF_Dxt1a://rgb:5-6-5 a:1
                case ETextureCompressFormat.TCF_Dxt3://rgb:5-6-5 a:8
                case ETextureCompressFormat.TCF_Dxt5://rg:8-8
                case ETextureCompressFormat.TCF_BC4:
                case ETextureCompressFormat.TCF_BC5:
                case ETextureCompressFormat.TCF_BC6:
                case ETextureCompressFormat.TCF_BC6_FLOAT:
                case ETextureCompressFormat.TCF_BC7_UNORM:
                    {
                        var pngMipsNode = node.GetOrAddNode("DxtMips", 0, 0);
                        mipLevel = SaveDxtMips_BcEncoder(pngMipsNode, curImage, desc);
                    }
                    break;
                case ETextureCompressFormat.TCF_Etc2_RGB8:
                case ETextureCompressFormat.TCF_Etc2_RGBA1:
                case ETextureCompressFormat.TCF_Etc2_RGBA8:
                case ETextureCompressFormat.TCF_Etc2_R11:
                case ETextureCompressFormat.TCF_Etc2_SIGNED_R11:
                case ETextureCompressFormat.TCF_Etc2_RG11:
                case ETextureCompressFormat.TCF_Etc2_SIGNED_RG11:
                    {
                        var pngMipsNode = node.GetOrAddNode("EtcMips", 0, 0);
                        mipLevel = SaveDxtMips_BcEncoder(pngMipsNode, curImage, desc);
                    }
                    break;
                case ETextureCompressFormat.TCF_Astc_4x4:
                case ETextureCompressFormat.TCF_Astc_4x4_Float:
                case ETextureCompressFormat.TCF_Astc_5x4:
                case ETextureCompressFormat.TCF_Astc_5x4_Float:
                case ETextureCompressFormat.TCF_Astc_5x5:
                case ETextureCompressFormat.TCF_Astc_5x5_Float:
                case ETextureCompressFormat.TCF_Astc_6x5:
                case ETextureCompressFormat.TCF_Astc_6x5_Float:
                case ETextureCompressFormat.TCF_Astc_6x6:
                case ETextureCompressFormat.TCF_Astc_6x6_Float:
                case ETextureCompressFormat.TCF_Astc_8x5:
                case ETextureCompressFormat.TCF_Astc_8x5_Float:
                case ETextureCompressFormat.TCF_Astc_8x6:
                case ETextureCompressFormat.TCF_Astc_8x6_Float:
                case ETextureCompressFormat.TCF_Astc_8x8:
                case ETextureCompressFormat.TCF_Astc_8x8_Float:
                case ETextureCompressFormat.TCF_Astc_10x6:
                case ETextureCompressFormat.TCF_Astc_10x6_Float:
                case ETextureCompressFormat.TCF_Astc_10x8:
                case ETextureCompressFormat.TCF_Astc_10x8_Float:
                case ETextureCompressFormat.TCF_Astc_10x10:
                case ETextureCompressFormat.TCF_Astc_10x10_Float:
                case ETextureCompressFormat.TCF_Astc_12x10:
                case ETextureCompressFormat.TCF_Astc_12x10_Float:
                case ETextureCompressFormat.TCF_Astc_12x12:
                case ETextureCompressFormat.TCF_Astc_12x12_Float:
                    {
                        var pngMipsNode = node.GetOrAddNode("AstcMips", 0, 0);
                        mipLevel = SaveAstcMips_ActcEncoder(pngMipsNode, curImage, desc);
                    }
                    break;
            }

            {
                desc.Desc.MipLevel = mipLevel;
                desc.Desc.dwStructureSize = (uint)sizeof(FPictureDesc);
                var attr = node.GetOrAddAttribute("Desc", 2, 0);
                using (var ar = attr.GetWriter((ulong)sizeof(FPictureDesc)))
                {
                    ar.Write(desc.Desc);
                    ar.Write(desc.MipSizes.Count);
                    for (int i = 0; i < desc.MipSizes.Count; i++)
                    {
                        ar.Write(desc.MipSizes[i]);
                        ar.Write(desc.BlockDimenstions[i]);
                    }
                    ar.Write(desc.BlockSize);
                }
            }
        }
        public static int SaveExrMips(XndNode pngMipsNode, Jither.OpenEXR.EXRFile file, TtPicDesc desc)
        {
            int mipLevel = 0;
            var part = file.Parts[0];
            System.Diagnostics.Debug.Assert(part.DataReader != null);
            int height = part.DisplayWindow.Height;
            int width = part.DisplayWindow.Width;
            System.Diagnostics.Debug.Assert(part.Channels.Count > 0);
            switch( part.Channels[0].Type )
            {
                case Jither.OpenEXR.EXRDataType.Float:
                    if (part.Channels.Count == 1)
                        desc.Format = EPixelFormat.PXF_R32_FLOAT;
                    else if (part.Channels.Count == 3)
                        desc.Format = EPixelFormat.PXF_R32G32B32_FLOAT;
                    else if (part.Channels.Count == 4)
                        desc.Format = EPixelFormat.PXF_R32G32B32A32_FLOAT;
                    break;
                case Jither.OpenEXR.EXRDataType.Half:
                    if (part.Channels.Count == 1)
                        desc.Format = EPixelFormat.PXF_R16_FLOAT;
                    else if (part.Channels.Count == 3)
                        desc.Format = EPixelFormat.PXF_R16G16B16A16_FLOAT;
                    else if (part.Channels.Count == 4)
                        desc.Format = EPixelFormat.PXF_R16G16B16A16_FLOAT;
                    break;
                default:
                    System.Diagnostics.Debug.Assert(false);
                    break;
            }

            desc.MipLevel = 1;
            if(part.TilingInformation!=null)
            {
                switch (part.TilingInformation.LevelMode)
                {
                    case Jither.OpenEXR.LevelMode.One:
                    case Jither.OpenEXR.LevelMode.RipMap:
                        desc.MipLevel = 1;
                        break;
                    case Jither.OpenEXR.LevelMode.MipMap:
                        desc.MipLevel = part.TilingInformation.Levels.Count;
                        break;
                }
            }
            do
            {
                var attr = pngMipsNode.GetOrAddAttribute($"ExrMip{mipLevel}", 0, 0);
                var dataSize = part.DataReader.GetTotalByteCount();
                byte[] pixelData = new byte[dataSize];
                int destChannelCount = part.Channels.Count;

                if (part.Channels.Count == 3 && part.Channels[0].Type == Jither.OpenEXR.EXRDataType.Half)
                {
                    destChannelCount = 4;
                    part.DataReader.ReadInterleaved(pixelData, new[] { "R", "G", "B" });
                    var pixelCount = width * height;
                    var pixelBytes = part.Channels[0].Type.GetBytesPerPixel();
                    byte[] destPixelData = new byte[pixelCount * 4 * pixelBytes];
                    System.Half halfOne = (System.Half)1.0;
                    byte[] halfByteArray = BitConverter.GetBytes(halfOne);
                    for (int i = 0; i < pixelCount; ++i)
                    {
                        Array.Copy(pixelData, i * 3 * pixelBytes, destPixelData, i * 4 * pixelBytes, 3 * pixelBytes);
                        Array.Copy(halfByteArray, 0, destPixelData, (i * 4 + 3) * pixelBytes, 2);
                    }
                    using (var ar = attr.GetWriter((ulong)destPixelData.Length))
                    {
                        ar.WriteNoSize(destPixelData, (int)(destPixelData.Length));
                    }

                    #region exrtest
                    bool bExrtest = false;
                    if (bExrtest)
                    {
                        System.Half[] halfPixelData = new System.Half[width * height * 4];
                        for (int i = 0; i < width * height; ++i)
                        {
                            int startIndex = i * 4;
                            halfPixelData[4 * i + 0] = BitConverter.ToHalf(destPixelData, startIndex * 2);
                            halfPixelData[4 * i + 1] = BitConverter.ToHalf(destPixelData, (startIndex + 1) * 2);
                            halfPixelData[4 * i + 2] = BitConverter.ToHalf(destPixelData, (startIndex + 2) * 2);
                            halfPixelData[4 * i + 3] = BitConverter.ToHalf(destPixelData, (startIndex + 3) * 2);
                        }

                        float[] floatPixelData = new float[width * height * 4];
                        for (int i = 0; i < halfPixelData.Length; ++i)
                        {
                            floatPixelData[i] = (float)halfPixelData[i];
                        }
                    }
                    #endregion
                }
                else
                {
                    part.DataReader.ReadInterleaved(pixelData, new[] { "R", "G", "B", "A" });
                    using (var ar = attr.GetWriter((ulong)dataSize))
                    {
                        ar.WriteNoSize(pixelData, (int)dataSize);
                    }

                    #region exrtest
                    bool bExrtest = false;
                    if (bExrtest)
                    {
                        System.Half[] halfPixelData = new System.Half[width * height * 4];
                        for (int i = 0; i < width * height; ++i)
                        {
                            int startIndex = i * 4;
                            halfPixelData[4 * i + 0] = BitConverter.ToHalf(pixelData, startIndex * 2);
                            halfPixelData[4 * i + 1] = BitConverter.ToHalf(pixelData, (startIndex + 1) * 2);
                            halfPixelData[4 * i + 2] = BitConverter.ToHalf(pixelData, (startIndex + 2) * 2);
                            halfPixelData[4 * i + 3] = BitConverter.ToHalf(pixelData, (startIndex + 3) * 2);
                        }

                        float[] floatPixelData = new float[width * height * 4];
                        for (int i = 0; i < halfPixelData.Length; ++i)
                        {
                            floatPixelData[i] = (float)halfPixelData[i];
                        }
                    }
                    #endregion
                }

                desc.MipSizes.Add(new Vector3i() { X = width, Y = height, Z = width * destChannelCount * part.Channels[0].Type.GetBytesPerPixel() });
                height = height / 2;
                width = width / 2;
                if ((height == 0 && width == 0))
                {
                    break;
                }
                mipLevel++;
                if (height == 0)
                    height = 1;
                if (width == 0)
                    width = 1;
                // TODO: get exr mips data
                //curImage = StbImageSharp.ImageProcessor.GetBoxDownSampler(curImage, width, height);
                if (desc.MipLevel > 0 && mipLevel == desc.MipLevel)
                    break;
            }
            while (true);

            return mipLevel;
        }

        public static int SaveHdrMips(XndNode pngMipsNode, StbImageSharp.ImageResultFloat curImage, TtPicDesc desc)
        {
            int mipLevel = 0;
            int height = curImage.Height;
            int width = curImage.Width;

            switch (curImage.Comp)
            {
                case StbImageSharp.ColorComponents.RedGreenBlue:
                    desc.Format = EPixelFormat.PXF_R32G32B32_FLOAT;
                    break;
                case StbImageSharp.ColorComponents.RedGreenBlueAlpha:
                    desc.Format = EPixelFormat.PXF_R32G32B32A32_FLOAT;
                    break;
                default:
                    desc.Format = EPixelFormat.PXF_R32G32B32A32_TYPELESS;
                    break;
            }

            desc.MipLevel = Math.Max(CalcMipLevel(curImage.Width, curImage.Height, true) - 3, 1);
            do
            {
                using (var memStream = new System.IO.MemoryStream())
                {
                    var writer = new StbImageWriteSharp.ImageWriter();
                    unsafe
                    {
                        var writeComp = UStbImageUtility.ConvertColorComponent(curImage.Comp);
                        fixed (void* ptr = curImage.Data)
                        {
                            writer.WriteHdr(ptr, curImage.Width, curImage.Height, writeComp, memStream);
                        }
                    }

                    var hdrData = memStream.ToArray();
                    var attr = pngMipsNode.GetOrAddAttribute($"HdrMip{mipLevel}", 0, 0);
                    using (var ar = attr.GetWriter((ulong)memStream.Position))
                    {
                        ar.WriteNoSize(hdrData, (int)memStream.Position);
                    }
                }
                desc.MipSizes.Add(new Vector3i() { X = width, Y = height, Z = width * (int)curImage.Comp * sizeof(float) });
                height = height / 2;
                width = width / 2;
                if ((height == 0 && width == 0))
                {
                    break;
                }
                mipLevel++;
                if (height == 0)
                    height = 1;
                if (width == 0)
                    width = 1;
                curImage = StbImageSharp.ImageProcessor.GetBoxDownSampler(curImage, width, height);
                if (desc.MipLevel > 0 && mipLevel == desc.MipLevel)
                    break;
            }
            while (true);

            return mipLevel;
        }

        public static int SavePngMips(XndNode pngMipsNode, StbImageSharp.ImageResult curImage, TtPicDesc desc)
        {
            int mipLevel = 0;
            int height = curImage.Height;
            int width = curImage.Width;
            do
            {
                using (var memStream = new System.IO.MemoryStream(curImage.Data.Length))
                {
                    var writer = new StbImageWriteSharp.ImageWriter();
                    writer.WritePng(curImage.Data, curImage.Width, curImage.Height, GetImageWriteFormat(curImage), memStream);
                    var pngData = memStream.ToArray();
                    var attr = pngMipsNode.GetOrAddAttribute($"PngMip{mipLevel}", 0, 0);
                    using (var ar = attr.GetWriter((ulong)memStream.Position))
                    {
                        ar.WriteNoSize(pngData, (int)memStream.Position);
                    }
                }
                desc.MipSizes.Add(new Vector3i() { X = width, Y = height, Z = width * 4 });
                height = height / 2;
                width = width / 2;
                if ((height == 0 && width == 0))
                {
                    break;
                }
                mipLevel++;
                if (height == 0)
                    height = 1;
                if (width == 0)
                    width = 1;
                curImage = StbImageSharp.ImageProcessor.GetBoxDownSampler(curImage, width, height);
                if (desc.MipLevel > 0 && mipLevel == desc.MipLevel)
                    break;
            }
            while (true);

            return mipLevel;
        }
        public unsafe static int SaveDxtMips(XndNode mipsNode, StbImageSharp.ImageResult curImage, TtPicDesc desc)
        {
            System.Diagnostics.Debug.Assert(desc.DontCompress == false);

            var srcImage = new TextureCompress.FCubeImage();
            fixed (byte* pImageData = &curImage.Data[0])
            {
                srcImage.m_Image0 = (uint*)pImageData;
                var blobResult = new Support.TtBlobObject();
                if (TextureCompress.CrunchWrap.CompressPixels(16, blobResult.mCoreObject, (uint)curImage.Width, (uint)curImage.Height, in srcImage, desc.CompressFormat, true, desc.sRGB, 0, 255))
                {
                    using (var reader = blobResult.CreateReader())
                    {
                        var ar = new IO.AuxReader<EngineNS.IO.UMemReader>(reader, null);
                        var loadDesc = new FPictureDesc();
                        ar.Read(out loadDesc);

                        System.Diagnostics.Debug.Assert(desc.Width == loadDesc.Width);
                        System.Diagnostics.Debug.Assert(desc.Height == loadDesc.Height);
                        desc.MipLevel = loadDesc.MipLevel;
                        desc.CubeFaces = loadDesc.CubeFaces;
                        desc.Format = loadDesc.Format;

                        desc.MipSizes.Clear();
                        for (uint i = 0; i < desc.Desc.CubeFaces; i++)
                        {
                            var faceNode = mipsNode.GetOrAddNode($"Face{i}", 0, 0);
                            for (uint j = 0; j < desc.Desc.MipLevel; j++)
                            {
                                var mipSize = new Vector3i();
                                ar.Read(out mipSize);
                                desc.MipSizes.Add(mipSize);
                                uint total_face_size = 0;
                                ar.Read(out total_face_size);
                                var pixels = new byte[total_face_size];
                                ar.ReadNoSize(pixels, (int)total_face_size);

                                var attr = faceNode.GetOrAddAttribute($"DxtMip{j}", 0, 0);
                                {
                                    using (var ar2 = attr.GetWriter((ulong)total_face_size))
                                    {
                                        ar2.WriteNoSize(pixels, (int)total_face_size);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return desc.MipLevel;
        }
        public unsafe static int SaveDxtMips_BcEncoder(XndNode mipsNode, StbImageSharp.ImageResultFloat curImage, TtPicDesc desc)
        {
            System.Diagnostics.Debug.Assert(desc.DontCompress == false);

            if(desc.MipLevel == 0)
                desc.MipLevel = CalcMipLevel(curImage.Width, curImage.Height, true) - 3;
            EPixelFormat descPixelFormat = EPixelFormat.PXF_UNKNOWN;
            switch (desc.CompressFormat)
            {
                case ETextureCompressFormat.TCF_BC6:
                    descPixelFormat = EPixelFormat.PXF_BC6H_UF16;
                    break;
            }
            desc.Format = descPixelFormat;
            desc.MipSizes.Clear();

            BcEncoder encoder = new BcEncoder();
            encoder.OutputOptions.GenerateMipMaps = true;
            encoder.OutputOptions.Quality = CompressionQuality.Balanced;
            encoder.OutputOptions.Format = CompressionFormat.Bc6U;
            encoder.OutputOptions.FileFormat = OutputFileFormat.Dds;

            int imageWidth = curImage.Width;
            int imageHeight = curImage.Height;
            if (desc.CubeFaces == 6)
            {
                desc.Width = desc.Height = imageWidth = imageHeight = MathHelper.Min(imageWidth, imageHeight);
            }
            else
                desc.CubeFaces = 1;

            int imageSize = imageWidth * imageHeight;
            System.DateTime beginTime = System.DateTime.Now;
            Profiler.Log.WriteInfoSimple("Start SaveDxtMips_BcEncoder");
            for (uint i = 0; i < desc.Desc.CubeFaces; i++)
            {
                ColorRgbFloat[] colorDataFace = new ColorRgbFloat[imageSize];
                for (int iC = 0; iC < imageSize; ++iC)
                {
                    colorDataFace[iC].r = curImage.Data[(i * imageSize + iC) * (int)curImage.Comp];
                    colorDataFace[iC].g = curImage.Data[(i * imageSize + iC) * (int)curImage.Comp + Math.Min(1, (int)curImage.Comp - 1)];
                    colorDataFace[iC].b = curImage.Data[(i * imageSize + iC) * (int)curImage.Comp + Math.Min(2, (int)curImage.Comp - 1)];
                }
                var memory2DFace = colorDataFace.AsMemory().AsMemory2D(imageHeight, imageWidth);

                var faceNode = mipsNode.GetOrAddNode($"Face{i}", 0, 0);
                for (uint j = 0; j < desc.MipLevel; j++)
                {
                    var mipSize = new Vector3i();
                    var blockDimension = new Vector2i();

                    var pixelsBcn = encoder.EncodeToRawBytesHdr(memory2DFace, (int)j, out mipSize.X, out mipSize.Y);

                    encoder.GetBlockCount(mipSize.X, mipSize.Y, out blockDimension.X, out blockDimension.Y);
                    desc.BlockSize = encoder.GetBlockSize();
                    if(desc.MipSizes.Count < desc.MipLevel)
                    {
                        desc.MipSizes.Add(mipSize);
                        desc.BlockDimenstions.Add(blockDimension);
                    }

                    var attr = faceNode.GetOrAddAttribute($"DxtMip{j}", 0, 0);
                    {
                        using (var ar2 = attr.GetWriter((ulong)pixelsBcn.Length))
                        {
                            ar2.WriteNoSize(pixelsBcn, (int)pixelsBcn.Length);
                        }
                    }
                }
            }

            System.DateTime endTime = System.DateTime.Now;
            Profiler.Log.WriteInfoSimple("End SaveDxtMips_BcEncoder");
            Profiler.Log.WriteInfoSimple("Use Time : " + endTime.Subtract(beginTime).TotalMilliseconds + " ms");

            return desc.MipLevel;
        }

        public unsafe static int SaveDxtMips_BcEncoder(XndNode mipsNode, StbImageSharp.ImageResult curImage, TtPicDesc desc)
        {
            System.Diagnostics.Debug.Assert(desc.DontCompress == false);

            BcEncoder encoder = new BcEncoder();
            bool IsKtx = false;
            desc.CubeFaces = 1;
            if (desc.MipLevel == 0)
                desc.MipLevel = CalcMipLevel(curImage.Width, curImage.Height, true)-3;
            EPixelFormat descPixelFormat = EPixelFormat.PXF_UNKNOWN;
            switch (desc.CompressFormat)
            {
                case ETextureCompressFormat.TCF_Dxt1:
                    if (desc.sRGB)
                        descPixelFormat = EPixelFormat.PXF_BC1_UNORM_SRGB;
                    else
                        descPixelFormat = EPixelFormat.PXF_BC1_UNORM;
                    encoder.OutputOptions.Format = CompressionFormat.Bc1;
                    break;
                case ETextureCompressFormat.TCF_Dxt1a:
                    if (desc.sRGB)
                        descPixelFormat = EPixelFormat.PXF_BC1_UNORM_SRGB;
                    else
                        descPixelFormat = EPixelFormat.PXF_BC1_UNORM;
                    encoder.OutputOptions.Format = CompressionFormat.Bc1WithAlpha;
                    break;
                case ETextureCompressFormat.TCF_Dxt3:
                    if (desc.sRGB)
                        descPixelFormat = EPixelFormat.PXF_BC2_UNORM_SRGB;
                    else
                        descPixelFormat = EPixelFormat.PXF_BC2_UNORM;
                    encoder.OutputOptions.Format = CompressionFormat.Bc2;
                    break;
                case ETextureCompressFormat.TCF_Dxt5:
                    if (desc.sRGB)
                        descPixelFormat = EPixelFormat.PXF_BC3_UNORM_SRGB;
                    else
                        descPixelFormat = EPixelFormat.PXF_BC3_UNORM;
                    encoder.OutputOptions.Format = CompressionFormat.Bc3;
                    break;
                case ETextureCompressFormat.TCF_BC5:
                        descPixelFormat = EPixelFormat.PXF_BC5_UNORM;
                    encoder.OutputOptions.Format = CompressionFormat.Bc5;
                    break;
                case ETextureCompressFormat.TCF_Etc2_RGB8:
                    if (desc.sRGB)
                        descPixelFormat = EPixelFormat.PXF_ETC2_SRGB8;
                    else
                        descPixelFormat = EPixelFormat.PXF_ETC2_RGB8;
                    encoder.OutputOptions.Format = CompressionFormat.Atc;
                    IsKtx = false;
                    break;
                case ETextureCompressFormat.TCF_Etc2_RGBA1:
                    if (desc.sRGB)
                        descPixelFormat = EPixelFormat.PXF_ETC2_SRGBA1;
                    else
                        descPixelFormat = EPixelFormat.PXF_ETC2_RGBA1;
                    encoder.OutputOptions.Format = CompressionFormat.AtcExplicitAlpha;
                    IsKtx = false; 
                    break;
                case ETextureCompressFormat.TCF_Etc2_RGBA8:
                    if (desc.sRGB)
                        descPixelFormat = EPixelFormat.PXF_ETC2_SRGBA8;
                    else
                        descPixelFormat = EPixelFormat.PXF_ETC2_RGBA8;
                    encoder.OutputOptions.Format = CompressionFormat.AtcInterpolatedAlpha;
                    IsKtx = false;
                    break;
                case ETextureCompressFormat.TCF_Etc2_RG11:
                    descPixelFormat = EPixelFormat.PXF_ETC2_RG11;
                    IsKtx = false;
                    break;
                case ETextureCompressFormat.TCF_Etc2_SIGNED_RG11:
                    descPixelFormat = EPixelFormat.PXF_ETC2_SIGNED_RG11;
                    IsKtx = false;
                    break;
                case ETextureCompressFormat.TCF_Etc2_R11:
                    descPixelFormat = EPixelFormat.PXF_ETC2_R11;
                    IsKtx = false;
                    break;
                case ETextureCompressFormat.TCF_Etc2_SIGNED_R11:
                    descPixelFormat = EPixelFormat.PXF_ETC2_SIGNED_R11;
                    IsKtx = false;
                    break;
            }
            desc.Format = descPixelFormat;
            desc.MipSizes.Clear();

            encoder.OutputOptions.FileFormat = IsKtx ? OutputFileFormat.Ktx : OutputFileFormat.Dds;
            encoder.OutputOptions.GenerateMipMaps = true;
            encoder.OutputOptions.Quality = CompressionQuality.Balanced;
            //encoder.OutputOptions.Format = CompressionFormat.Bc2;

            PixelFormat pixelFormat = PixelFormat.Rgba32;
            switch (curImage.Comp)
            {
                case StbImageSharp.ColorComponents.RedGreenBlueAlpha:
                    pixelFormat = PixelFormat.Rgba32;
                    break;
                case StbImageSharp.ColorComponents.RedGreenBlue:
                    pixelFormat = PixelFormat.Rgb24;
                    break;
            }

            for (uint i = 0; i < desc.Desc.CubeFaces; i++)
            {
                System.Threading.Tasks.Task<byte[]>[] taskArray = null;
                bool isEncodeMultiThread = true;
                if (isEncodeMultiThread==true)
                {
                    taskArray = new System.Threading.Tasks.Task<byte[]>[desc.Desc.MipLevel];
                    for (uint j = 0; j < desc.Desc.MipLevel; j++)
                    {
                        taskArray[j] = encoder.EncodeToRawBytesAsync(curImage.Data, curImage.Width, curImage.Height, pixelFormat, (int)j);
                    }
                    System.Threading.Tasks.Task.WaitAll(taskArray);
                }


                var faceNode = mipsNode.GetOrAddNode($"Face{i}", 0, 0);
                for (uint j = 0; j < desc.Desc.MipLevel; j++)
                {
                    var mipSize = new Vector3i();
                    var blockDimension = new Vector2i();
                    byte[] pixelsBcn = null;
                    if(isEncodeMultiThread==true)
                    {
                        pixelsBcn = taskArray[j].Result;
                        encoder.CalculateMipMapSize(curImage.Width, curImage.Height, (int)j, out mipSize.X, out mipSize.Y);
                    }
                    else
                    {
                        pixelsBcn = encoder.EncodeToRawBytes(curImage.Data.AsSpan(), curImage.Width, curImage.Height, pixelFormat, (int)j, out mipSize.X, out mipSize.Y);
                    }
                    encoder.GetBlockCount(mipSize.X, mipSize.Y, out blockDimension.X, out blockDimension.Y);
                    desc.BlockSize = encoder.GetBlockSize();
                    desc.MipSizes.Add(mipSize);
                    desc.BlockDimenstions.Add(blockDimension);

                    if (IsKtx)
                    {
                        var attr = faceNode.GetOrAddAttribute($"EtcMip{j}", 0, 0);
                        {
                            using (var ar2 = attr.GetWriter((ulong)pixelsBcn.Length))
                            {
                                ar2.WriteNoSize(pixelsBcn, (int)pixelsBcn.Length);
                            }
                        }
                    }
                    else
                    {
                        var attr = faceNode.GetOrAddAttribute($"DxtMip{j}", 0, 0);
                        {
                            using (var ar2 = attr.GetWriter((ulong)pixelsBcn.Length))
                            {
                                ar2.WriteNoSize(pixelsBcn, (int)pixelsBcn.Length);
                            }
                        }
                    }
                }
            }

            return desc.MipLevel;
        }
        public unsafe static int SaveAstcMips_ActcEncoder(XndNode mipsNode, StbImageSharp.ImageResult curImage, TtPicDesc desc)
        {
            System.Diagnostics.Debug.Assert(desc.DontCompress == false);
            System.Diagnostics.Debug.Assert(false);
            return 0;
        }
        public static uint GetPixelByteWidth(EPixelFormat format)
        {
            switch (format)
            {
                case EPixelFormat.PXF_R8G8B8A8_UNORM:
                case EPixelFormat.PXF_R8G8B8A8_TYPELESS:
                case EPixelFormat.PXF_R8G8B8A8_SINT:
                case EPixelFormat.PXF_R8G8B8A8_UINT:
                case EPixelFormat.PXF_B8G8R8A8_UNORM:
                case EPixelFormat.PXF_B8G8R8A8_TYPELESS:
                case EPixelFormat.PXF_B8G8R8A8_UNORM_SRGB:
                    return 4;
                default:
                    return 0;
            }
        }
        /* 
        public static unsafe StbImageSharp.ImageResult[] LoadImageLevels(RName name, uint mipLevel, ref UPicDesc desc)
        {
            using (var xnd = IO.TtXndHolder.LoadXnd(name.Address))
            {
                {
                    if (desc != null)
                    {
                        var attr = xnd.RootNode.TryGetAttribute("Desc");
                        using (var ar = attr.GetReader(null))
                        {
                            ar.Read(out desc.Desc);
                        }
                    }
                }
                var pngNode = xnd.RootNode.TryGetChildNode("PngMips");
                if (pngNode.IsValidPointer)
                {
                    if (mipLevel == 0)
                    {
                        mipLevel = pngNode.GetNumOfAttribute();
                    }
                    var result = new StbImageSharp.ImageResult[mipLevel];
                    for (uint i = 0; i < mipLevel; i++)
                    {
                        var mipAttr = pngNode.TryGetAttribute($"PngMip{i}");
                        if (mipAttr.NativePointer == IntPtr.Zero)
                            return null;

                        byte[] data;
                        using (var ar = mipAttr.GetReader(null))
                        {
                            ar.ReadNoSize(out data, (int)mipAttr.GetReaderLength());
                        }

                        using (var memStream = new System.IO.MemoryStream(data, false))
                        {
                            result[i] = StbImageSharp.ImageResult.FromStream(memStream, StbImageSharp.ColorComponents.RedGreenBlueAlpha);
                        }
                    }
                    return result;
                }
                else
                {
                    //todo
                    pngNode = xnd.RootNode.TryGetChildNode("DxtMips");
                }
                return null;
            }
        }
        */
        public static unsafe Support.TtBlobObject[] LoadPixelMipLevels(RName name, uint mipLevel, TtPicDesc desc)
        {
            using (var xnd = IO.TtXndHolder.LoadXnd(name.Address))
            {
                if (desc != null)
                {
                    LoadPictureDesc(xnd.RootNode, desc);
                }

                var pngNode = xnd.RootNode.TryGetChildNode("PngMips");
                if (pngNode.IsValidPointer)
                {
                    if (mipLevel == 0)
                    {
                        mipLevel = pngNode.GetNumOfAttribute();
                    }
                    var result = new StbImageSharp.ImageResult[mipLevel];
                    var blobs = new Support.TtBlobObject[mipLevel];
                    for (uint i = 0; i < mipLevel; i++)
                    {
                        var mipAttr = pngNode.TryGetAttribute($"PngMip{i}");
                        if (mipAttr.NativePointer == IntPtr.Zero)
                            return null;

                        byte[] data;
                        using (var ar = mipAttr.GetReader(null))
                        {
                            ar.ReadNoSize(out data, (int)mipAttr.GetReaderLength());
                        }

                        using (var memStream = new System.IO.MemoryStream(data, false))
                        {
                            result[i] = StbImageSharp.ImageResult.FromStream(memStream, StbImageSharp.ColorComponents.RedGreenBlueAlpha);
                            blobs[i] = new Support.TtBlobObject();
                            fixed (byte* p = &result[i].Data[0])
                            {
                                blobs[i].PushData(p, (uint)result[i].Data.Length);
                            }
                        }
                    }
                    return blobs;
                }
                else
                {
                    //todo
                    var dxtNode = xnd.RootNode.TryGetChildNode("DxtMips");
                    if (dxtNode.IsValidPointer)
                    {
                        for (uint j = 0; j < desc.Desc.CubeFaces; j++)
                        {
                            var faceNode = dxtNode.TryGetChildNode($"Face{j}");
                            if (faceNode.IsValidPointer == false)
                            {
                                continue;
                            }
                            if (mipLevel == 0)
                            {
                                mipLevel = faceNode.GetNumOfAttribute();
                            }
                            var blobs = new Support.TtBlobObject[mipLevel];
                            for (uint i = 0; i < mipLevel; i++)
                            {
                                var realLevel = desc.MipLevel - mipLevel + i;
                                var ptr = faceNode.TryGetAttribute($"DxtMip{realLevel}");
                                if (ptr.NativePointer == IntPtr.Zero)
                                    return null;
                                var mipAttr = ptr;
                                byte[] data;
                                using (var ar = mipAttr.GetReader(null))
                                {
                                    ar.ReadNoSize(out data, (int)mipAttr.GetReaderLength());
                                }

                                blobs[i] = new Support.TtBlobObject();
                                fixed (byte* p = &data[0])
                                {
                                    blobs[i].PushData(p, (uint)data.Length);
                                }
                            }
                            return blobs;
                        }
                    }
                }
                return null;
            }
        }
        public static unsafe TtPicDesc LoadPictureDesc(RName name)
        {
            using (var xnd = IO.TtXndHolder.LoadXnd(name.Address))
            {
                return LoadPictureDesc(xnd.RootNode);
            }
        }
        public static unsafe void LoadPictureDesc(IO.TtXndNode node, TtPicDesc desc)
        {
            var attr = node.TryGetAttribute("Desc");
            using (var ar = attr.GetReader(null))
            {
                //ar.Read(out desc.Desc);
                uint headSize = 0;
                ar.Read(out headSize);
                System.Diagnostics.Debug.Assert(headSize <= sizeof(FPictureDesc));
                fixed (FPictureDesc* pDesc = &desc.Desc)
                {
                    var pDescData = (byte*)pDesc + sizeof(uint);
                    ar.ReadPtr(pDescData, (int)headSize - sizeof(uint));
                }

                int len;
                ar.Read(out len);
                desc.MipSizes.Clear();
                desc.BlockDimenstions.Clear();
                for (int i = 0; i < len; i++)
                {
                    Vector3i tmp = new Vector3i();
                    if (headSize == 20)
                    {
                        ar.Read(out tmp.X);
                        ar.Read(out tmp.Y);
                        tmp.Z = tmp.X * 4;
                    }
                    else
                    {
                        ar.Read(out tmp);
                    }
                    desc.MipSizes.Add(tmp);
                    if (attr.Version == 2)
                    {
                        Vector2i blockDimension = new Vector2i();
                        ar.Read(out blockDimension);
                        desc.BlockDimenstions.Add(blockDimension);
                    }
                }

                if (attr.Version==1)
                {
                    Vector2i blockDimension = new Vector2i();
                    ar.Read(out blockDimension.X);
                    ar.Read(out blockDimension.Y);
                    ar.Read(out desc.BlockSize);
                    desc.BlockDimenstions.Add(blockDimension);
                }
                else if(attr.Version==2)
                {
                    ar.Read(out desc.BlockSize);
                }
            }
        }
        public static unsafe TtPicDesc LoadPictureDesc(IO.TtXndNode node)
        {
            TtPicDesc desc = new TtPicDesc();
            LoadPictureDesc(node, desc);
            return desc;
        }
        public static unsafe TtTexture LoadTexture2DMipLevel(IO.TtXndNode node, TtPicDesc desc, int level, TtTexture oldTexture)
        {
            switch (desc.CompressFormat)
            {
                case ETextureCompressFormat.TCF_None:
                    {
                        var pngNode = node.TryGetChildNode("PngMips");
                        if (pngNode.IsValidPointer)
                            return LoadPngTexture2DMipLevel(node, desc, level);
                        else
                        {
                            var hdrNode = node.TryGetChildNode("HdrMips");
                            if (hdrNode.IsValidPointer)
                                return LoadHdrTexture2DMipLevel(node, desc, level);
                            else
                            {
                                var exrNode = node.TryGetChildNode("ExrMips");
                                if (exrNode.IsValidPointer)
                                    return LoadExrTexture2DMipLevel(node, desc, level);
                            }
                        }
                        return null;
                    }
                case ETextureCompressFormat.TCF_Dxt1:
                case ETextureCompressFormat.TCF_Dxt1a:
                case ETextureCompressFormat.TCF_Dxt3:
                case ETextureCompressFormat.TCF_Dxt5:
                case ETextureCompressFormat.TCF_BC5:
                case ETextureCompressFormat.TCF_BC6:
                case ETextureCompressFormat.TCF_BC6_FLOAT:
                    {
                        oldTexture = null;
                        return LoadDxtTexture2DMipLevel(node, desc, level, oldTexture);
                    }
                default:
                    return null;
            }
        }
        public static unsafe TtTexture LoadTexture2DMipLevel(IO.TtXndNode node, TtPicDesc desc, int level, int channelR, int channelG, int channelB, int channelA)
        {
            switch (desc.CompressFormat)
            {
                case ETextureCompressFormat.TCF_None:
                    {
                        var pngNode = node.TryGetChildNode("PngMips");
                        if (pngNode.IsValidPointer)
                            return LoadPngTexture2DMipLevel(node, desc, level);
                        else
                        {
                            var hdrNode = node.TryGetChildNode("HdrMips");
                            if (hdrNode.IsValidPointer)
                                return LoadHdrTexture2DMipLevel(node, desc, level);
                            else
                            {
                                var exrNode = node.TryGetChildNode("ExrMips");
                                if (exrNode.IsValidPointer)
                                    return LoadExrTexture2DMipLevel(node, desc, level, channelR, channelG, channelB, channelA);
                            }
                        }
                        return null;
                    }
                case ETextureCompressFormat.TCF_Dxt1:
                case ETextureCompressFormat.TCF_Dxt1a:
                case ETextureCompressFormat.TCF_Dxt3:
                case ETextureCompressFormat.TCF_Dxt5:
                case ETextureCompressFormat.TCF_BC6:
                case ETextureCompressFormat.TCF_BC6_FLOAT:
                    {
                        return LoadDxtTexture2DMipLevel(node, desc, level);
                    }
                default:
                    return null;
            }
        }
        #region Load Mips
        private static unsafe TtTexture LoadExrTexture2DMipLevel(TtXndNode node, TtPicDesc desc, int mipLevel)
        {
            if (mipLevel == 0)
                return null;
            var rc = TtEngine.Instance.GfxDevice.RenderContext;

            var exrNode = node.TryGetChildNode("ExrMips");
            if (exrNode.NativePointer == IntPtr.Zero)
                return null;

            var num = (int)desc.CubeFaces * mipLevel;
            if (num == 0)
                return null;
            var handles = stackalloc System.Runtime.InteropServices.GCHandle[num];
            try
            {
                var pInitData = stackalloc FMappedSubResource[num];


                for (uint i = 0; i < mipLevel; i++)
                {
                    var realLevel = desc.MipLevel - mipLevel + i;
                    var ptr = exrNode.TryGetAttribute($"ExrMip{realLevel}");
                    if (ptr.NativePointer == IntPtr.Zero)
                        return null;
                    var mipAttr = ptr;
                    byte[] data;
                    using (var ar = mipAttr.GetReader(null))
                    {
                        ar.ReadNoSize(out data, (int)mipAttr.GetReaderLength());
                    }

                    handles[i] = System.Runtime.InteropServices.GCHandle.Alloc(data, System.Runtime.InteropServices.GCHandleType.Pinned);
                    pInitData[i].m_pData = System.Runtime.InteropServices.Marshal.UnsafeAddrOfPinnedArrayElement(data, 0).ToPointer();
                    pInitData[i].m_RowPitch = (uint)desc.MipSizes[(int)i].Z;
                    pInitData[i].m_DepthPitch = pInitData[i].m_RowPitch * (uint)desc.MipSizes[(int)i].Y;
                }

                var texDesc = new FTextureDesc();
                texDesc.SetDefault();
                texDesc.Width = (uint)desc.MipSizes[desc.MipLevel - mipLevel].X;
                texDesc.Height = (uint)desc.MipSizes[desc.MipLevel - mipLevel].Y;
                texDesc.MipLevels = (uint)mipLevel;
                texDesc.InitData = pInitData;
                texDesc.Format = desc.Format;

                var result = rc.CreateTexture(in texDesc);
                if (result == null)
                    return null;
                return result;
            }
            finally
            {
                for (uint i = 0; i < mipLevel; i++)
                {
                    if (handles[i].IsAllocated)
                        handles[i].Free();
                }
            }
        }
        private static unsafe TtTexture LoadExrTexture2DMipLevel(TtXndNode node, TtPicDesc desc, int mipLevel, int channelR, int channelG, int channelB, int channelA)
        {
            if (mipLevel == 0)
                return null;
            var rc = TtEngine.Instance.GfxDevice.RenderContext;

            var exrNode = node.TryGetChildNode("ExrMips");
            if (exrNode.NativePointer == IntPtr.Zero)
                return null;

            var handles = stackalloc System.Runtime.InteropServices.GCHandle[mipLevel];
            try
            {
                var pInitData = stackalloc FMappedSubResource[mipLevel];


                for (uint i = 0; i < mipLevel; i++)
                {
                    var realLevel = desc.MipLevel - mipLevel + i;
                    var ptr = exrNode.TryGetAttribute($"ExrMip{realLevel}");
                    if (ptr.NativePointer == IntPtr.Zero)
                        return null;
                    var mipAttr = ptr;
                    byte[] data;
                    using (var ar = mipAttr.GetReader(null))
                    {
                        ar.ReadNoSize(out data, (int)mipAttr.GetReaderLength());
                    }

                    channelR = channelR == 0 ? 0 : 1;
                    channelG = channelG == 0 ? 0 : 1;
                    channelB = channelB == 0 ? 0 : 1;
                    channelA = channelA == 0 ? 0 : 1;
                    var channelCount = desc.Desc.PixelChannelCount();
                    var byteWidth = desc.Desc.PixelByteWidth();
                    int extractChannelCount = channelR + channelG + channelB + channelA;
                    if(channelCount == 3)
                    {
                        extractChannelCount = channelR + channelG + channelB;
                        channelA = 0;
                    }
                    else if (channelCount == 2)
                    {
                        extractChannelCount = channelR + channelG;
                        channelB = channelA = 0;
                    }
                    else if (channelCount == 1)
                    {
                        extractChannelCount = channelR;
                        channelG = channelB = channelA = 0;
                    }
                    if (channelCount > 1 && extractChannelCount > 0)
                    {
                        int pixelCount = desc.MipSizes[(int)i].X * desc.MipSizes[(int)i].Y;
                        var channelByteWidth = byteWidth / channelCount;

                        byte[] channelData = new byte[byteWidth * pixelCount];

                        for( int j = 0; j < pixelCount; ++j )
                        {
                            if(extractChannelCount==1)
                            {
                                int iCopyIndex = 0;
                                if (channelG != 0)
                                    iCopyIndex = 1;
                                if (channelB != 0)
                                    iCopyIndex = 2;
                                if (channelA != 0)
                                    iCopyIndex = 3;

                                for( int k = 0; k < channelCount; ++k )
                                {
                                    if (k == 3)
                                    {
                                        System.Half oneHalf = (System.Half)1.0f;
                                        var oneBytes = BitConverter.GetBytes(oneHalf);
                                        Array.Copy(oneBytes, 0, channelData, j * byteWidth + k * channelByteWidth, channelByteWidth);
                                    }
                                    else
                                        Array.Copy(data, j * byteWidth + iCopyIndex * channelByteWidth, channelData, j * byteWidth + k * channelByteWidth, channelByteWidth);                                    
                                }

                            }
                            else
                            {
                                if (channelR != 0)
                                    Array.Copy(data, j * byteWidth, channelData, j * byteWidth, channelByteWidth);
                                if (channelG != 0 && channelCount > 1)
                                    Array.Copy(data, j * byteWidth + channelByteWidth, channelData, j * byteWidth + channelByteWidth, channelByteWidth);
                                if (channelB != 0 && channelCount > 2)
                                    Array.Copy(data, j * byteWidth + 2*channelByteWidth, channelData, j * byteWidth + 2*channelByteWidth, channelByteWidth);
                                if (channelA != 0 && channelCount > 3)
                                    Array.Copy(data, j * byteWidth + 3*channelByteWidth, channelData, j * byteWidth + 3*channelByteWidth, channelByteWidth);
                                else
                                {
                                    System.Half oneHalf = (System.Half)1.0f;
                                    var oneBytes = BitConverter.GetBytes(oneHalf);
                                    Array.Copy(oneBytes, 0, channelData, j * byteWidth + 3 * channelByteWidth, channelByteWidth);
                                }
                            }
                        }

                        handles[i] = System.Runtime.InteropServices.GCHandle.Alloc(channelData, System.Runtime.InteropServices.GCHandleType.Pinned);
                        pInitData[i].m_pData = System.Runtime.InteropServices.Marshal.UnsafeAddrOfPinnedArrayElement(channelData, 0).ToPointer();
                        pInitData[i].m_RowPitch = (uint)desc.MipSizes[(int)i].Z;
                        pInitData[i].m_DepthPitch = pInitData[i].m_RowPitch * (uint)desc.MipSizes[(int)i].Y;

                    }
                    else
                    {
                        handles[i] = System.Runtime.InteropServices.GCHandle.Alloc(data, System.Runtime.InteropServices.GCHandleType.Pinned);
                        pInitData[i].m_pData = System.Runtime.InteropServices.Marshal.UnsafeAddrOfPinnedArrayElement(data, 0).ToPointer();
                        pInitData[i].m_RowPitch = (uint)desc.MipSizes[(int)i].Z;
                        pInitData[i].m_DepthPitch = pInitData[i].m_RowPitch * (uint)desc.MipSizes[(int)i].Y;
                    }
                }

                var texDesc = new FTextureDesc();
                texDesc.SetDefault();
                texDesc.Width = (uint)desc.MipSizes[desc.MipLevel - mipLevel].X;
                texDesc.Height = (uint)desc.MipSizes[desc.MipLevel - mipLevel].Y;
                texDesc.MipLevels = (uint)mipLevel;
                texDesc.InitData = pInitData;
                texDesc.Format = desc.Format;

                var result = rc.CreateTexture(in texDesc);
                if (result == null)
                    return null;
                return result;
            }
            finally
            {
                for (uint i = 0; i < mipLevel; i++)
                {
                    if (handles[i].IsAllocated)
                        handles[i].Free();
                }
            }
        }
        private static unsafe TtTexture LoadHdrTexture2DMipLevel(TtXndNode node, TtPicDesc desc, int mipLevel)
        {
            if (mipLevel == 0)
                return null;
            var rc = TtEngine.Instance.GfxDevice.RenderContext;

            var hdrNode = node.TryGetChildNode("HdrMips");
            if (hdrNode.NativePointer == IntPtr.Zero)
                return null;

            var handles = stackalloc System.Runtime.InteropServices.GCHandle[mipLevel];
            try
            {
                var pInitData = stackalloc FMappedSubResource[mipLevel];

                StbImageSharp.ColorComponents colorComp = StbImageSharp.ColorComponents.RedGreenBlue;
                switch (desc.Format)
                {
                    case EPixelFormat.PXF_R32G32B32_FLOAT:
                        colorComp = ColorComponents.RedGreenBlue;
                        break;
                    case EPixelFormat.PXF_R32G32B32A32_FLOAT:
                        colorComp = ColorComponents.RedGreenBlueAlpha;
                        break;
                    default:
                        colorComp = ColorComponents.RedGreenBlueAlpha;
                        break;
                }
                for (uint i = 0; i < mipLevel; i++)
                {
                    var realLevel = desc.MipLevel - mipLevel + i;
                    var ptr = hdrNode.TryGetAttribute($"HdrMip{realLevel}");
                    if (ptr.NativePointer == IntPtr.Zero)
                        return null;
                    var mipAttr = ptr;
                    byte[] data;
                    using (var ar = mipAttr.GetReader(null))
                    {
                        ar.ReadNoSize(out data, (int)mipAttr.GetReaderLength());
                    }

                    StbImageSharp.ImageResultFloat image;
                    using (var memStream = new System.IO.MemoryStream(data, false))
                    {
                        image = StbImageSharp.ImageResultFloat.FromStream(memStream, colorComp);
                    }
                    handles[i] = System.Runtime.InteropServices.GCHandle.Alloc(image.Data, System.Runtime.InteropServices.GCHandleType.Pinned);
                    //pInitData[i].SetDefault();
                    pInitData[i].m_pData = System.Runtime.InteropServices.Marshal.UnsafeAddrOfPinnedArrayElement(image.Data, 0).ToPointer();
                    pInitData[i].m_RowPitch = (uint)image.Width * (uint)colorComp * sizeof(float);
                    pInitData[i].m_DepthPitch = pInitData[i].m_RowPitch * (uint)image.Height;
                }

                var texDesc = new FTextureDesc();
                texDesc.SetDefault();
                texDesc.Width = (uint)desc.MipSizes[desc.MipLevel - mipLevel].X;
                texDesc.Height = (uint)desc.MipSizes[desc.MipLevel - mipLevel].Y;
                texDesc.MipLevels = (uint)mipLevel;
                texDesc.InitData = pInitData;
                texDesc.Format = desc.Format;

                var result = rc.CreateTexture(in texDesc);
                if (result == null)
                    return null;
                return result;
            }
            finally
            {
                for (uint i = 0; i < mipLevel; i++)
                {
                    if (handles[i].IsAllocated)
                        handles[i].Free();
                }
            }
        }

        private static unsafe TtTexture LoadPngTexture2DMipLevel(TtXndNode node, TtPicDesc desc, int mipLevel)
        {
            if (mipLevel == 0)
                return null;
            var rc = TtEngine.Instance.GfxDevice.RenderContext;

            var pngNode = node.TryGetChildNode("PngMips");
            if (pngNode.NativePointer == IntPtr.Zero)
                return null;

            var handles = stackalloc System.Runtime.InteropServices.GCHandle[mipLevel];
            try
            {
                var pInitData = stackalloc FMappedSubResource[mipLevel];

                StbImageSharp.ColorComponents colorComp = StbImageSharp.ColorComponents.RedGreenBlueAlpha;
                for (uint i = 0; i < mipLevel; i++)
                {
                    var realLevel = desc.MipLevel - mipLevel + i;
                    var ptr = pngNode.TryGetAttribute($"PngMip{realLevel}");
                    if (ptr.NativePointer == IntPtr.Zero)
                        return null;
                    var mipAttr = ptr;
                    byte[] data;
                    using (var ar = mipAttr.GetReader(null))
                    {
                        ar.ReadNoSize(out data, (int)mipAttr.GetReaderLength());
                    }

                    StbImageSharp.ImageResult image;
                    using (var memStream = new System.IO.MemoryStream(data, false))
                    {
                        image = StbImageSharp.ImageResult.FromStream(memStream, StbImageSharp.ColorComponents.RedGreenBlueAlpha);
                    }
                    handles[i] = System.Runtime.InteropServices.GCHandle.Alloc(image.Data, System.Runtime.InteropServices.GCHandleType.Pinned);
                    //pInitData[i].SetDefault();
                    pInitData[i].m_pData = System.Runtime.InteropServices.Marshal.UnsafeAddrOfPinnedArrayElement(image.Data, 0).ToPointer();
                    pInitData[i].m_RowPitch = (uint)image.Width * 4;
                    pInitData[i].m_DepthPitch = pInitData[i].m_RowPitch * (uint)image.Height;

                    colorComp = image.Comp;
                }

                var texDesc = new FTextureDesc();
                texDesc.SetDefault();
                texDesc.Width = (uint)desc.MipSizes[desc.MipLevel - mipLevel].X;
                texDesc.Height = (uint)desc.MipSizes[desc.MipLevel - mipLevel].Y;
                texDesc.MipLevels = (uint)mipLevel;
                texDesc.InitData = pInitData;
                switch (colorComp)
                {
                    case StbImageSharp.ColorComponents.RedGreenBlueAlpha:
                        texDesc.Format = EPixelFormat.PXF_R8G8B8A8_UNORM;
                        break;
                }

                var result = rc.CreateTexture(in texDesc);
                if (result == null)
                    return null;
                return result;
            }
            finally
            {
                for (uint i = 0; i < mipLevel; i++)
                {
                    if (handles[i].IsAllocated)
                        handles[i].Free();
                }
            }
        }
        private static unsafe TtTexture LoadDxtTexture2DMipLevel(TtXndNode node, TtPicDesc desc, int mipLevel)
        {
            if (mipLevel == 0)
                return null;
            var rc = TtEngine.Instance.GfxDevice.RenderContext;

            var pngNode = node.TryGetChildNode("DxtMips");
            if (pngNode.NativePointer == IntPtr.Zero)
                return null;

            var handles = stackalloc System.Runtime.InteropServices.GCHandle[(int)desc.CubeFaces * mipLevel];
            try
            {
                var pInitData = stackalloc FMappedSubResource[(int)desc.CubeFaces * mipLevel];

                for (uint j = 0; j < desc.Desc.CubeFaces; j++)
                {
                    var faceNode = pngNode.TryGetChildNode($"Face{j}");
                    if (faceNode.IsValidPointer == false)
                    {
                        continue;
                    }
                    for (uint i = 0; i < mipLevel; i++)
                    {
                        var realLevel = desc.MipLevel - mipLevel + i;
                        var ptr = faceNode.TryGetAttribute($"DxtMip{realLevel}");
                        if (ptr.NativePointer == IntPtr.Zero)
                            return null;
                        var mipAttr = ptr;
                        byte[] data;
                        using (var ar = mipAttr.GetReader(null))
                        {
                            ar.ReadNoSize(out data, (int)mipAttr.GetReaderLength());
                        }

                        int dataIndex = (int)(j * mipLevel + i);
                        handles[dataIndex] = System.Runtime.InteropServices.GCHandle.Alloc(data, System.Runtime.InteropServices.GCHandleType.Pinned);
                        pInitData[dataIndex].m_pData = System.Runtime.InteropServices.Marshal.UnsafeAddrOfPinnedArrayElement(data, 0).ToPointer();
                        if(desc.BlockSize==0)
                        {
                            pInitData[dataIndex].m_RowPitch = (uint)desc.MipSizes[(int)realLevel].Z;
                            pInitData[dataIndex].m_DepthPitch = pInitData[i].m_RowPitch * (uint)desc.MipSizes[(int)realLevel].Y;
                        }
                        else
                        {
                            if (realLevel > (desc.BlockDimenstions.Count-1))
                                return null;
                            var blockWidth = desc.BlockDimenstions[(int)realLevel].X;
                            var blockHeight = desc.BlockDimenstions[(int)realLevel].Y;
                            pInitData[dataIndex].m_RowPitch = (uint)(blockWidth * desc.BlockSize);
                            pInitData[dataIndex].m_DepthPitch = pInitData[dataIndex].m_RowPitch * (uint)blockHeight;
                        }
                    }
                }
                var texDesc = new FTextureDesc();
                texDesc.SetDefault();
                texDesc.Width = (uint)desc.MipSizes[desc.MipLevel - mipLevel].X;
                texDesc.Height = (uint)desc.MipSizes[desc.MipLevel - mipLevel].Y;
                texDesc.MipLevels = (uint)mipLevel;
                texDesc.InitData = pInitData;
                texDesc.Format = desc.Desc.Format;
                if(desc.CubeFaces==6)
                {
                    texDesc.ArraySize = 6;
                    texDesc.MiscFlags = EResourceMiscFlag.RM_TEXTURECUBE;
                }

                var result = rc.CreateTexture(in texDesc);
                return result;
            }
            finally
            {
                for (uint i = 0; i < (int)desc.CubeFaces * mipLevel; i++)
                {
                    if (handles[i].IsAllocated)
                        handles[i].Free();
                }
            }
        }

        private static unsafe TtTexture LoadDxtTexture2DMipLevel(TtXndNode node, TtPicDesc desc, int mipLevel, TtTexture oldTexture)
        {
            if (oldTexture == null)
            {
                return LoadDxtTexture2DMipLevel(node, desc, mipLevel);
            }

            var rc = TtEngine.Instance.GfxDevice.RenderContext;

            var pngNode = node.TryGetChildNode("DxtMips");
            if (pngNode.NativePointer == IntPtr.Zero)
                return null;

            uint oldLevel = oldTexture.mCoreObject.Desc.MipLevels;
            var texDesc = new FTextureDesc();
            texDesc.SetDefault();
            texDesc.Width = (uint)desc.MipSizes[desc.MipLevel - mipLevel].X;
            texDesc.Height = (uint)desc.MipSizes[desc.MipLevel - mipLevel].Y;
            texDesc.MipLevels = (uint)mipLevel;
            texDesc.Format = desc.Desc.Format;
            if (desc.CubeFaces == 6)
            {
                texDesc.ArraySize = 6;
                texDesc.MiscFlags = EResourceMiscFlag.RM_TEXTURECUBE;
            }

            var result = rc.CreateTexture(in texDesc);

            using (var tsCmd = new NxRHI.FTransientCmd(NxRHI.EQueueType.QU_Transfer, "Texture.LoadMip.CopyOld"))
            {
                var copyNum = Math.Min(mipLevel, oldLevel);
                for (uint j = 0; j < desc.Desc.CubeFaces; j++)
                {
                    for (uint i = 0; i < copyNum; i++)
                    {
                        var cpDraw = rc.CreateCopyDraw();
                        cpDraw.Mode = ECopyDrawMode.CDM_Texture2Texture;
                        cpDraw.BindTextureSrc(oldTexture);
                        cpDraw.BindTextureDest(result);
                        cpDraw.DestSubResource = i;
                        cpDraw.SrcSubResource = i;
                        tsCmd.CmdList.PushGpuDraw(cpDraw.mCoreObject.NativeSuper);
                    }
                }
            }

            if (oldLevel < mipLevel)
            {
                using (var tsCmd = new NxRHI.FTransientCmd(NxRHI.EQueueType.QU_Transfer, "Texture.LoadMip.CopyNewMip"))
                {
                    for (uint j = 0; j < desc.Desc.CubeFaces; j++)
                    {
                        var faceNode = pngNode.TryGetChildNode($"Face{j}");
                        if (faceNode.IsValidPointer == false)
                        {
                            continue;
                        }
                        for (uint i = oldLevel; i < mipLevel; i++)
                        {
                            var ptr = faceNode.TryGetAttribute($"DxtMip{i}");
                            if (ptr.NativePointer == IntPtr.Zero)
                                continue;
                            var mipAttr = ptr;
                            byte[] data;
                            using (var ar = mipAttr.GetReader(null))
                            {
                                ar.ReadNoSize(out data, (int)mipAttr.GetReaderLength());
                            }

                            var blockWidth = desc.BlockDimenstions[(int)i].X;
                            var blockHeight = desc.BlockDimenstions[(int)i].Y;
                            fixed (byte* p = &data[0])
                            {
                                NxRHI.FTextureDesc tDesc = new FTextureDesc();
                                tDesc.SetDefault();
                                tDesc.Usage = EGpuUsage.USAGE_STAGING;
                                tDesc.CpuAccess = ECpuAccess.CAS_WRITE;
                                tDesc.MipLevels = 1;
                                tDesc.Width = (uint)desc.MipSizes[desc.MipLevel - (int)i].X;
                                tDesc.Height = (uint)desc.MipSizes[desc.MipLevel - (int)i].Y;
                                tDesc.Format = desc.Desc.Format;
                                FMappedSubResource subRes = new FMappedSubResource();
                                subRes.pData = p;
                                if (desc.BlockSize == 0)
                                {
                                    subRes.RowPitch = (uint)desc.MipSizes[(int)i].Z;
                                    subRes.DepthPitch = subRes.RowPitch * (uint)desc.MipSizes[(int)i].Y;
                                }
                                else
                                {
                                    subRes.RowPitch = (uint)(blockWidth * desc.BlockSize);
                                    subRes.DepthPitch = subRes.RowPitch * (uint)blockHeight;
                                }
                                tDesc.InitData = &subRes;
                                var tex = rc.CreateTexture(in tDesc);

                                var cpDraw = rc.CreateCopyDraw();
                                cpDraw.Mode = ECopyDrawMode.CDM_Texture2Texture;
                                cpDraw.BindTextureSrc(tex);
                                cpDraw.BindTextureDest(result);
                                cpDraw.DestSubResource = i;
                                cpDraw.SrcSubResource = 0;
                                tsCmd.CmdList.PushGpuDraw(cpDraw.mCoreObject.NativeSuper);
                            }
                        }
                    }
                }
            }
            return result;
        }
        #endregion
        static StbImageWriteSharp.ColorComponents ComponentConvert(StbImageSharp.ColorComponents component)
        {
            switch(component)
            {
                case ColorComponents.Grey:
                    return StbImageWriteSharp.ColorComponents.Grey;
                case ColorComponents.GreyAlpha:
                    return StbImageWriteSharp.ColorComponents.GreyAlpha;
                case ColorComponents.RedGreenBlue:
                    return StbImageWriteSharp.ColorComponents.RedGreenBlue;
                case ColorComponents.RedGreenBlueAlpha:
                    return StbImageWriteSharp.ColorComponents.RedGreenBlueAlpha;
            }
            return StbImageWriteSharp.ColorComponents.RedGreenBlueAlpha;
        }
        public static unsafe void SaveOriginImage(RName rn)
        {
            if (System.IO.File.Exists(rn.Address) == false)
                return;

            var imgType = GetOriginImageType(rn);
            switch (imgType)
            {
                case EngienNS.Bricks.ImageDecoder.UImageType.PNG:
                    {
                        var image = LoadOriginPng(rn);
                        if (image == null)
                        {
                            return;
                        }
                        using (var memStream = new System.IO.FileStream(rn.Address + ".png", System.IO.FileMode.OpenOrCreate))
                        {
                            var writer = new StbImageWriteSharp.ImageWriter();
                            writer.WritePng(image.Data, image.Width, image.Height, ComponentConvert(image.Comp), memStream);
                        }
                    }
                    break;
                case EngienNS.Bricks.ImageDecoder.UImageType.HDR:
                    {
                        StbImageSharp.ImageResultFloat imageFloat = new StbImageSharp.ImageResultFloat();
                        LoadOriginHdr(rn, ref imageFloat);
                        using (var memStream = new System.IO.FileStream(rn.Address + ".hdr", System.IO.FileMode.OpenOrCreate))
                        {
                            var writer = new StbImageWriteSharp.ImageWriter();
                            fixed (void* fptr = imageFloat.Data)
                            {
                                writer.WriteHdr(fptr, imageFloat.Width, imageFloat.Height, ComponentConvert(imageFloat.Comp), memStream);
                            }
                        }
                    }
                    break;
                case EngienNS.Bricks.ImageDecoder.UImageType.EXR:
                    {
                        System.IO.Stream outStream = null;
                        var file = LoadOriginExr(rn, ref outStream);
                        if(file!=null)
                        {
                            var part = file.Parts[0];

                            byte[] pixelData = new byte[part.DataReader.GetTotalByteCount()];
                            string[] channelNames = new[] { "R", "G", "B", "A" };
                            if (part.Channels.Count == 3)
                                channelNames = new[] { "R", "G", "B" };

                            part.DataReader.ReadInterleaved(pixelData, channelNames);
                            file.Write(rn.Address + ".exr");
                            part.DataWriter.WriteInterleaved(pixelData, new[] { "R", "G", "B", "A" });
                        }
                    }
                    break;
            }
        }
        public static async System.Threading.Tasks.Task<TtSrView> LoadSrvMipmap(RName rn, int mipLevel, TtTexture oldTexture)
        {
            TtPicDesc desc = null;
            var tex2d = await TtEngine.Instance.EventPoster.Post((state) =>
            {
                var xnd = IO.TtXndHolder.LoadXnd(rn.Address);
                if (xnd == null)
                    return null;

                desc = LoadPictureDesc(xnd.RootNode);

                if (mipLevel == -1 || mipLevel > desc.MipLevel)
                    mipLevel = desc.MipLevel;

                return LoadTexture2DMipLevel(xnd.RootNode, desc, mipLevel, oldTexture);
            }, Thread.Async.EAsyncTarget.AsyncIO);

            if (tex2d == null)
            {
                Profiler.Log.WriteLine<Profiler.TtIOCategory>(Profiler.ELogTag.Warning, $"LoadSrvMipmap {rn} failed");
                if (TtEngine.Instance.PlayMode == EPlayMode.Editor)
                {
                    //SaveOriginImage(rn);
                }
                return null;
            }

            tex2d.SetDebugName(rn.Name);

            var rc = TtEngine.Instance.GfxDevice.RenderContext;
            var srvDesc = new FSrvDesc();
            srvDesc.SetTexture2D();
            if (desc.CubeFaces == 6)
            {
                srvDesc.Type = ESrvType.ST_TextureCube;
                srvDesc.TextureCube.MipLevels = (uint)mipLevel;
            }
            else
            {
                srvDesc.Type = ESrvType.ST_Texture2D;
                srvDesc.Texture2D.MipLevels = (uint)mipLevel;
            }
            srvDesc.Format = tex2d.mCoreObject.Desc.Format;
            
            var result = rc.CreateSRV(tex2d, in srvDesc);
            result.StreamingTexture = tex2d;
            result.PicDesc = desc;
            result.LevelOfDetail = mipLevel;
            result.TargetLOD = mipLevel;
            result.AssetName = rn;

            result.SetDebugName(rn.Name);
            return result;
        }
        public static async System.Threading.Tasks.Task<TtSrView> LoadSrvMipmap(RName rn, int mipLevel, int channelR, int channelG, int channelB, int channelA)
        {
            TtPicDesc desc = null;
            var tex2d = await TtEngine.Instance.EventPoster.Post((state) =>
            {
                var xnd = IO.TtXndHolder.LoadXnd(rn.Address);
                if (xnd == null)
                    return null;

                desc = LoadPictureDesc(xnd.RootNode);

                if (mipLevel == -1 || mipLevel > desc.MipLevel)
                    mipLevel = desc.MipLevel;

                return LoadTexture2DMipLevel(xnd.RootNode, desc, mipLevel, channelR, channelG, channelB, channelA);
            }, Thread.Async.EAsyncTarget.AsyncIO);

            if (tex2d == null)
            {
                if (TtEngine.Instance.PlayMode == EPlayMode.Editor)
                    SaveOriginImage(rn);
                return null;
            }

            var rc = TtEngine.Instance.GfxDevice.RenderContext;
            var srvDesc = new FSrvDesc();
            srvDesc.SetTexture2D();
            srvDesc.Type = ESrvType.ST_Texture2D;
            srvDesc.Format = tex2d.mCoreObject.Desc.Format;
            srvDesc.Texture2D.MipLevels = (uint)mipLevel;

            var result = rc.CreateSRV(tex2d, in srvDesc);
            result.PicDesc = desc;
            result.LevelOfDetail = mipLevel;
            result.TargetLOD = mipLevel;
            result.AssetName = rn;
            return result;
        }
        #endregion
    }
    public class TtRenderTargetView : AuxPtrType<NxRHI.IRenderTargetView>
    {
        public TtRenderTargetView(IRenderTargetView ptr)
        {
            mCoreObject = ptr;
            mCoreObject.NativeSuper.AddRef();
        }
        public TtRenderTargetView()
        {

        }
    }
    public class TtDepthStencilView : AuxPtrType<NxRHI.IDepthStencilView>
    {
    }

    public class TtTextureManager : IO.TtStreamingManager, ITickable
    {
        public int GetTickOrder()
        {
            return 0;
        }
        public TtTextureManager()
        {

        }
        ~TtTextureManager()
        {
            Cleanup();
            TtEngine.Instance?.TickableManager.RemoveTickable(this);
        }
        public void Cleanup()
        {
            foreach (var i in StreamingAssets)
            {
                var srv = i.Value as TtSrView;
                if (srv == null)
                    continue;
                srv.Dispose();
            }
            StreamingAssets.Clear();
        }
        public TtSrView DefaultTexture;
        public async System.Threading.Tasks.Task Initialize(TtEngine engine)
        {
            DefaultTexture = await GetTexture(engine.Config.DefaultTexture);
        }
        private Thread.UAwaitSessionManager<RName, TtSrView> mCreatingSession = new Thread.UAwaitSessionManager<RName, TtSrView>();
        List<RName> mWaitRemoves = new List<RName>();
        public async Thread.Async.TtTask<TtSrView> CreateTexture(string file)
        {
            if (EngineNS.IO.TtFileManager.FileExists(file) == false)
                return null;
            StbImageSharp.ImageResult image = await TtEngine.Instance.EventPoster.Post((state) =>
            {
                using (var memStream = new System.IO.FileStream(file, System.IO.FileMode.Open))
                {
                    return StbImageSharp.ImageResult.FromStream(memStream, StbImageSharp.ColorComponents.RedGreenBlueAlpha);
                }
            }, Thread.Async.EAsyncTarget.AsyncIO);
            if (image == null)
            {
                return null;
            }

            return CreateTexture(image, file);
        }
        private unsafe TtSrView CreateTexture(StbImageSharp.ImageResult image, string file)
        {
            var texDesc = new FTextureDesc();
            texDesc.SetDefault();
            texDesc.Width = (uint)image.Width;
            texDesc.Height = (uint)image.Height;
            texDesc.MipLevels = (uint)1;
            uint pixelWidth = 4;
            switch (image.Comp)
            {
                case StbImageSharp.ColorComponents.RedGreenBlueAlpha:
                    texDesc.Format = EPixelFormat.PXF_R8G8B8A8_UNORM;
                    pixelWidth = 4;
                    break;
            }
            var data = new FMappedSubResource();
            data.RowPitch = texDesc.m_Width * pixelWidth;
            data.DepthPitch = data.RowPitch * texDesc.Height;
            texDesc.InitData = &data;
            fixed (byte* pData = &image.Data[0])
            {
                data.pData = pData;

                var rc = TtEngine.Instance.GfxDevice.RenderContext;
                var texture2d = rc.CreateTexture(in texDesc);
                texture2d.SetDebugName(file);

                var srvDesc = new FSrvDesc();
                srvDesc.SetTexture2D();
                srvDesc.Type = ESrvType.ST_Texture2D;
                srvDesc.Format = texDesc.Format;
                srvDesc.Texture2D.MipLevels = texDesc.MipLevels;
                var result = rc.CreateSRV(texture2d, in srvDesc);
                //result.PicDesc.Desc = texDesc;
                //result.LevelOfDetail = mipLevel;
                //result.TargetLOD = mipLevel;
                //result.AssetName = rn;

                return result;
            }
        }
        public async Thread.Async.TtTask<TtSrView> GetOrNewTexture(RName rn, int mipLevel = 1)
        {
            var result = await GetTexture(rn, mipLevel);
            if (result != null)
                return result;

            return await TtSrView.LoadSrvMipmap(rn, mipLevel, null);
        }
        public async Thread.Async.TtTask<TtSrView> GetTexture(RName rn, int mipLevel = 1)
        {
            if (rn == null)
                return null;
            TtSrView srv = null;
            IO.IStreaming result;
            lock (StreamingAssets)
            {
                if (StreamingAssets.TryGetValue(rn, out result))
                {
                    srv = result as TtSrView;
                    if (srv == null)
                        return null;
                    srv.TargetLOD = mipLevel;
                    return srv;
                }
            }

            bool isNewSession;
            var session = mCreatingSession.GetOrNewSession(rn, out isNewSession);
            if (isNewSession == false)
            {
                return await session.Await();
            }

            try
            {
                srv = await TtSrView.LoadSrvMipmap(rn, mipLevel, null);
                if (srv == null)
                    return srv;
                lock (StreamingAssets)
                {
                    if (StreamingAssets.TryGetValue(rn, out result) == false)
                    {
                        StreamingAssets.Add(rn, srv);
                    }
                    else
                    {
                        srv = result as TtSrView;
                    }
                }

                return srv;
            }
            finally
            {
                mCreatingSession.FinishSession(rn, session, srv);
            }
        }
        public TtSrView TryGetTexture(RName rn)
        {
            if (rn == null)
                return null;
            lock (StreamingAssets)
            {
                TtSrView srv;
                IO.IStreaming result;
                if (StreamingAssets.TryGetValue(rn, out result))
                {
                    srv = result as TtSrView;
                    if (srv == null)
                        return null;
                    return srv;
                }
                return null;
            }
        }
        public unsafe override bool UpdateTargetLOD(IO.IStreaming asset)
        {
            var srv = asset as TtSrView;
            if (srv == null)
                return false;

            var nowFrame = TtEngine.Instance.CurrentTickFrame;
            var resState = srv.mCoreObject.GetResourceState();
            if (nowFrame - resState->GetAccessFrame() > 15 * (uint)TtEngine.Instance.Config.TargetFps)//15 second & 60 target fps
            {
                srv.TargetLOD = 1;
                //mWaitRemoves.Add(asset.AssetName);
                return true;
            }
            else
            {
                srv.TargetLOD = srv.MaxLOD;
                return true;
            }
        }
        float TickInterval = 150;
        float EllapsedRemainTime = 150;
        [ThreadStatic]
        private static Profiler.TimeScope mScopeTick;
        private static Profiler.TimeScope ScopeTick
        {
            get
            {
                if (mScopeTick == null)
                    mScopeTick = new Profiler.TimeScope(typeof(TtTextureManager), nameof(TickLogic));
                return mScopeTick;
            }
        }
        public void TickLogic(float ellapse)
        {
            using (new Profiler.TimeScopeHelper(ScopeTick))
            {
                EllapsedRemainTime -= ellapse;
                if (EllapsedRemainTime <= 0)
                {
                    UpdateStreamingState();
                    EllapsedRemainTime = TickInterval;
                }
                foreach (var i in mWaitRemoves)
                {
                    StreamingAssets.Remove(i);
                }
                mWaitRemoves.Clear();
            }   
        }
        public void TickRender(float ellapse)
        {

        }
        public void TickBeginFrame(float ellapse)
        {

        }
        public void TickSync(float ellapse)
        {

        }
    }
}
