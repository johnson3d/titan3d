using System;
using System.Collections.Generic;
using EngineNS.Bricks.NodeGraph;

namespace EngineNS.Bricks.Procedure.Node
{
    [Bricks.CodeBuilder.ContextMenu("Image", "Image\\Loader", UPgcGraph.PgcEditorKeyword)]
    public partial class UImageLoader : UPgcNodeBase
    {
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public PinOut RgbPin { get; set; } = new PinOut();
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public PinOut RedPin { get; set; } = new PinOut();
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public PinOut GreenPin { get; set; } = new PinOut();
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public PinOut BluePin { get; set; } = new PinOut();
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public PinOut AlphaPin { get; set; } = new PinOut();

        public UBufferCreator RgbBufferCreator { get; } = UBufferCreator.CreateInstance<USuperBuffer<Vector3, FFloat3Operator>>(-1, -1, -1);
        public UImageLoader()
        {
            PrevSize = new Vector2(100, 100);

            Icon.Size = new Vector2(25, 25);
            Icon.Color = 0xFF00FF00;
            TitleColor = 0xFF204020;
            BackColor = 0x80808080;

            AddOutput(RgbPin, "RGB", RgbBufferCreator);
            AddOutput(RedPin, "R", DefaultBufferCreator);
            AddOutput(GreenPin, "G", DefaultBufferCreator);
            AddOutput(BluePin, "B", DefaultBufferCreator);
            AddOutput(AlphaPin, "A", DefaultBufferCreator);
        }
        ~UImageLoader()
        {
            if (TextureSRV != null)
            {
                TextureSRV?.FreeTextureHandle();
                TextureSRV = null;
            }
        }
        RName mImageName;
        [Rtti.Meta]
        [RName.PGRName(FilterExts = RHI.CShaderResourceView.AssetExt)]
        public RName ImageName
        {
            get => mImageName;
            set
            {
                mImageName = value;
                System.Action exec = async () =>
                {
                    TextureSRV = await UEngine.Instance.GfxDevice.TextureManager.GetTexture(value);
                };
                exec();
            }
        }
        private RHI.CShaderResourceView TextureSRV;
        public override int PreviewResultIndex
        {
            get => mPreviewResultIndex;
            set
            {
                mPreviewResultIndex = 0;
            }
        }
        public override void UpdateAMetaReferences(IO.IAssetMeta ameta)
        {
            ameta.AddReferenceAsset(ImageName);
        }
        public override unsafe bool InitProcedure(UPgcGraph graph)
        {
            if (ImageName == null)
                return false;

            StbImageSharp.ImageResult image = null;

            using (var xnd = IO.CXndHolder.LoadXnd(ImageName.Address))
            {
                if (xnd == null)
                    return false;

                var pngAttr = xnd.RootNode.TryGetAttribute("Png");
                if (pngAttr.IsValidPointer == false)
                    return false;

                var ar = pngAttr.GetReader(null);
                byte[] data;
                ar.ReadNoSize(out data, (int)pngAttr.GetReaderLength());
                pngAttr.ReleaseReader(ref ar);

                using (var memStream = new System.IO.MemoryStream(data, false))
                {
                    image = StbImageSharp.ImageResult.FromStream(memStream, StbImageSharp.ColorComponents.RedGreenBlueAlpha);
                    if (image == null)
                        return false;
                }
            }

            DefaultBufferCreator.BufferType = Rtti.UTypeDesc.TypeOf<USuperBuffer<float, FFloatOperator>>();
            if (image != null)
            {
                UBufferConponent red = null;
                UBufferConponent green = null;
                UBufferConponent blue = null;
                UBufferConponent alpha = null;
                int PixelSize = 0;
                int LineSize = 0;
                switch (image.Comp)
                {
                    case StbImageSharp.ColorComponents.RedGreenBlue:
                        {
                            PixelSize = 3;
                            LineSize = 3 * image.Width;
                            LineSize = (int)CoreDefine.Roundup((uint)LineSize, 4);

                            DefaultBufferCreator.XSize = image.Width;
                            DefaultBufferCreator.YSize = image.Height;
                            DefaultBufferCreator.ZSize = 1;

                            red = UBufferConponent.CreateInstance(UBufferCreator.CreateInstance<USuperBuffer<float, FFloatOperator>>(image.Width, image.Height, 1));
                            green = UBufferConponent.CreateInstance(UBufferCreator.CreateInstance<USuperBuffer<float, FFloatOperator>>(image.Width, image.Height, 1));
                            blue = UBufferConponent.CreateInstance(UBufferCreator.CreateInstance<USuperBuffer<float, FFloatOperator>>(image.Width, image.Height, 1));

                            var rgb = UBufferConponent.CreateInstance(UBufferCreator.CreateInstance<USuperBuffer<Vector3, FFloat3Operator>>(image.Width, image.Height, 1));

                            graph.BufferCache.RegBuffer(RedPin, red);
                            graph.BufferCache.RegBuffer(GreenPin, green);
                            graph.BufferCache.RegBuffer(BluePin, blue);

                            graph.BufferCache.RegBuffer(RgbPin, rgb);

                            fixed (byte* p = &image.Data[0])
                            {
                                for (int i = 0; i < image.Height; i++)
                                {
                                    for (int j = 0; j < image.Width; j++)
                                    {
                                        var rgbValue = new Vector3();
                                        float v = (float)p[LineSize * i + j * PixelSize] / 256.0f;
                                        rgbValue.X = v;
                                        red.SetPixel(j, i, v);

                                        v = (float)p[LineSize * i + j * PixelSize + 1] / 256.0f;
                                        rgbValue.Y = v;
                                        green.SetPixel(j, i, v);

                                        v = (float)p[LineSize * i + j * PixelSize + 2] / 256.0f;
                                        rgbValue.Y = v;
                                        blue.SetPixel(j, i, v);

                                        rgb.SetPixel<Vector3>(j, i, in rgbValue);
                                    }
                                }
                            }
                        }
                        break;
                    case StbImageSharp.ColorComponents.RedGreenBlueAlpha:
                        {
                            PixelSize = 4;
                            LineSize = 4 * image.Width;

                            DefaultBufferCreator.XSize = image.Width;
                            DefaultBufferCreator.YSize = image.Height;
                            DefaultBufferCreator.ZSize = 1;

                            red = UBufferConponent.CreateInstance(UBufferCreator.CreateInstance<USuperBuffer<float, FFloatOperator>>(image.Width, image.Height, 1));
                            green = UBufferConponent.CreateInstance(UBufferCreator.CreateInstance<USuperBuffer<float, FFloatOperator>>(image.Width, image.Height, 1));
                            blue = UBufferConponent.CreateInstance(UBufferCreator.CreateInstance<USuperBuffer<float, FFloatOperator>>(image.Width, image.Height, 1));
                            alpha = UBufferConponent.CreateInstance(UBufferCreator.CreateInstance<USuperBuffer<float, FFloatOperator>>(image.Width, image.Height, 1));

                            var rgb = UBufferConponent.CreateInstance(UBufferCreator.CreateInstance<USuperBuffer<Vector3, FFloat3Operator>>(image.Width, image.Height, 1));

                            graph.BufferCache.RegBuffer(RedPin, red);
                            graph.BufferCache.RegBuffer(GreenPin, green);
                            graph.BufferCache.RegBuffer(BluePin, blue);
                            graph.BufferCache.RegBuffer(AlphaPin, alpha);

                            graph.BufferCache.RegBuffer(RgbPin, rgb);

                            fixed (byte* p = &image.Data[0])
                            {
                                for (int i = 0; i < image.Height; i++)
                                {
                                    for (int j = 0; j < image.Width; j++)
                                    {
                                        var rgbValue = new Vector3();
                                        float v = (float)p[LineSize * i + j * PixelSize] / 255.0f;
                                        rgbValue.X = v;
                                        blue.SetPixel(j, i, v);

                                        v = (float)p[LineSize * i + j * PixelSize + 1] / 255.0f;
                                        rgbValue.Y = v;
                                        green.SetPixel(j, i, v);

                                        v = (float)p[LineSize * i + j * PixelSize + 2] / 255.0f;
                                        rgbValue.Z = v;
                                        red.SetPixel(j, i, v);

                                        v = (float)p[LineSize * i + j * PixelSize + 3] / 255.0f;
                                        alpha.SetPixel(j, i, v);

                                        rgb.SetPixel<Vector3>(j, i, in rgbValue);
                                    }
                                }
                            }
                        }
                        break;
                }
            }

            return true;
        }

        public unsafe override void OnPreviewDraw(in Vector2 prevStart, in Vector2 prevEnd, ImDrawList cmdlist)
        {
            if (TextureSRV == null)
                return;

            var uv0 = new Vector2(0, 0);
            var uv1 = new Vector2(1, 1);
            unsafe
            {
                cmdlist.AddImage(TextureSRV.GetTextureHandle().ToPointer(), in prevStart, in prevEnd, in uv0, in uv1, 0xFFFFFFFF);
            }
        }
    }
    [Bricks.CodeBuilder.ContextMenu("Preview", "Image\\Preview", UPgcGraph.PgcEditorKeyword)]
    public partial class UPreviewImage : UPgcNodeBase
    {
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public PinIn InXYZPin { get; set; } = new PinIn();
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public PinIn InXPin { get; set; } = new PinIn();
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public PinIn InYPin { get; set; } = new PinIn();
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public PinIn InZPin { get; set; } = new PinIn();
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public PinOut XYZPin { get; set; } = new PinOut();
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public PinOut XPin { get; set; } = new PinOut();
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public PinOut YPin { get; set; } = new PinOut();
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public PinOut ZPin { get; set; } = new PinOut();

        public UBufferCreator XYZBufferCreator { get; } = UBufferCreator.CreateInstance<USuperBuffer<Vector3, FFloat3Operator>>(-1, -1, -1);
        public UPreviewImage()
        {
            PrevSize = new Vector2(100, 100);

            Icon.Size = new Vector2(25, 25);
            Icon.Color = 0xFF00FF00;
            TitleColor = 0xFF204020;
            BackColor = 0x80808080;

            AddInput(InXYZPin, "InXYZ", XYZBufferCreator);
            AddInput(InXPin, "InX", DefaultInputDesc);
            AddInput(InYPin, "InY", DefaultInputDesc);
            AddInput(InZPin, "InZ", DefaultInputDesc);

            AddOutput(XYZPin, "XYZ", XYZBufferCreator);
            AddOutput(XPin, "X", DefaultBufferCreator);
            AddOutput(YPin, "Y", DefaultBufferCreator);
            AddOutput(ZPin, "Z", DefaultBufferCreator);
        }
        public override int PreviewResultIndex
        {
            get => mPreviewResultIndex;
            set
            {
                mPreviewResultIndex = -1;
            }
        }
        public unsafe override void OnPreviewDraw(in Vector2 prevStart, in Vector2 prevEnd, ImDrawList cmdlist)
        {
            if (PreviewSRV == null)
                return;

            var uv0 = new Vector2(0, 0);
            var uv1 = new Vector2(1, 1);
            unsafe
            {
                cmdlist.AddImage(PreviewSRV.GetTextureHandle().ToPointer(), in prevStart, in prevEnd, in uv0, in uv1, 0xFFFFFFFF);
            }
        }
        public override UBufferCreator GetOutBufferCreator(PinOut pin)
        {
            return base.GetOutBufferCreator(pin);
        }
        public override bool OnProcedure(UPgcGraph graph)
        {
            var xyzSrc = graph.BufferCache.FindBuffer(InXYZPin) as USuperBuffer<Vector3, FFloat3Operator>;
            var xSrc = graph.BufferCache.FindBuffer(InXPin) as USuperBuffer<float, FFloatOperator>;
            var ySrc = graph.BufferCache.FindBuffer(InYPin) as USuperBuffer<float, FFloatOperator>;
            var zSrc = graph.BufferCache.FindBuffer(InZPin) as USuperBuffer<float, FFloatOperator>;

            if (xyzSrc != null)
            {
                graph.BufferCache.RegBuffer(XYZPin, xyzSrc);
            }
            if (xSrc != null)
            {
                graph.BufferCache.RegBuffer(XPin, xSrc);
            }
            if (ySrc != null)
            {
                graph.BufferCache.RegBuffer(YPin, ySrc);
            }
            if (zSrc != null)
            {
                graph.BufferCache.RegBuffer(ZPin, zSrc);
            }

            if (graph.GraphEditor != null)
            {
                var norMap = new Bricks.Procedure.UImage2D();

                if (xyzSrc != null)
                {
                    norMap.Initialize(xyzSrc.Width, xyzSrc.Height,
                        xyzSrc,
                        null,
                        0);
                }
                else
                {
                    norMap.Initialize(xSrc.Width, xSrc.Height,
                        xSrc,
                        ySrc,
                        zSrc,
                        null);
                }

                if (PreviewSRV != null)
                {
                    PreviewSRV?.FreeTextureHandle();
                    PreviewSRV = null;
                }
                PreviewSRV = norMap.CreateRGBA8Texture2D();
            }
            return true;
        }
    }
    public partial class USdfCalculator : UMonocular
    {
    }
    [Bricks.CodeBuilder.ContextMenu("Surface", "Image\\Surface", UPgcGraph.PgcEditorKeyword)]
    public partial class UImageSurface : UPgcNodeBase
    {
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public PinIn HeightPin { get; set; } = new PinIn();
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public PinIn NormPin { get; set; } = new PinIn();
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public PinOut ResultPin { get; set; } = new PinOut();
        
        public UBufferCreator HeightCreator { get; } = UBufferCreator.CreateInstance<USuperBuffer<float, FFloatOperator>>(-1, -1, -1);
        public UBufferCreator NormCreator { get; } = UBufferCreator.CreateInstance<USuperBuffer<Vector3, FFloat3Operator>>(-1, -1, -1);
        public UBufferCreator SurfCreator { get; } = UBufferCreator.CreateInstance<USuperBuffer<FSquareSurface, FSquareSurfaceOperator>>(-1, -1, -1);
        public UImageSurface()
        {
            Icon.Size = new Vector2(25, 25);
            Icon.Color = 0xFF00FF00;
            TitleColor = 0xFF204020;
            BackColor = 0x80808080;

            AddInput(HeightPin, "Height", HeightCreator);
            AddInput(NormPin, "Normal", NormCreator);
            AddOutput(ResultPin, "Surface", HeightCreator, "Surface");
        }
        public override UBufferCreator GetOutBufferCreator(PinOut pin)
        {
            if (pin == ResultPin)
            {
                var graph = ParentGraph as UPgcGraph;
                var buffer = graph.BufferCache.FindBuffer(HeightPin);
                if (buffer != null)
                {
                    SurfCreator.XSize = buffer.BufferCreator.XSize;
                    SurfCreator.YSize = buffer.BufferCreator.YSize;
                    SurfCreator.ZSize = buffer.BufferCreator.ZSize;
                    return SurfCreator;
                }
            }
            return base.GetOutBufferCreator(pin);
        }
        public override bool OnProcedure(UPgcGraph graph)
        {
            var hMap = graph.BufferCache.FindBuffer(HeightPin) as USuperBuffer<float, FFloatOperator>;
            var norMap = graph.BufferCache.FindBuffer(NormPin) as USuperBuffer<Vector3, FFloat3Operator>;
            var result = graph.BufferCache.FindBuffer(ResultPin) as USuperBuffer<FSquareSurface, FSquareSurfaceOperator>;

            for (int i = 0; i < result.Height; i++)
            {
                for (int j = 0; j < result.Width; j++)
                {
                    FSquareSurface squre;

                    var uvw = result.GetClampedUVW(j, i, 0);
                    var h = hMap.GetPixel<float>(in uvw);
                    var n = Vector3.UnitY;
                    if (norMap != null)
                        n = norMap.GetPixel<Vector3>(in uvw);
                    squre.UV_0_0 = new Vector4(in n, h);

                    uvw = result.GetClampedUVW(j + 1, i, 0);
                    h = hMap.GetPixel<float>(in uvw);
                    if (norMap != null)
                        n = norMap.GetPixel<Vector3>(in uvw);
                    squre.UV_1_0 = new Vector4(in n, h);

                    uvw = result.GetClampedUVW(j, i + 1, 0);
                    h = hMap.GetPixel<float>(in uvw);
                    if (norMap != null)
                        n = norMap.GetPixel<Vector3>(in uvw);
                    squre.UV_0_1 = new Vector4(in n, h);

                    uvw = result.GetClampedUVW(j + 1, i + 1, 0);
                    h = hMap.GetPixel<float>(in uvw);
                    if (norMap != null)
                        n = norMap.GetPixel<Vector3>(in uvw);
                    squre.UV_1_1 = new Vector4(in n, h);

                    result.SetPixel(j, i, 0, in squre);
                }
            }
            return true;
        }
        //uv [0-1]
        public static Vector4 GetHeight(UBufferConponent buffer, float u, float v)
        {
            var surface = buffer as USuperBuffer<FSquareSurface, FSquareSurfaceOperator>;
            if (surface == null)
                return Vector4.Zero;
            var x = (int)(u * (float)surface.Width);
            var y = (int)(v * (float)surface.Height);
            ref var quad = ref surface.GetPixel<FSquareSurface>(new Vector3(u, v, 0));
            float remain_u = (u - ((float)x / (float)surface.Width)) / (1.0f / (float)surface.Width);
            float remain_v = (v - ((float)y / (float)surface.Height)) / (1.0f / (float)surface.Height);
            return quad.GetPoint(remain_u, remain_v);
        }
    }
    [Bricks.CodeBuilder.ContextMenu("SamplerSurface", "Image\\SamplerSurface", UPgcGraph.PgcEditorKeyword)]
    public partial class USamplerSurface : UPgcNodeBase
    {
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public PinIn SurfacePin { get; set; } = new PinIn();
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public PinOut ResultPin { get; set; } = new PinOut();
        [Rtti.Meta]
        public float UStart { get; set; } = 0;
        [Rtti.Meta]
        public float VStart { get; set; } = 0;
        [Rtti.Meta]
        public float UEnd { get; set; } = 1;
        [Rtti.Meta]
        public float VEnd { get; set; } = 1;
        public UBufferCreator HeightCreator { get; } = UBufferCreator.CreateInstance<USuperBuffer<float, FFloatOperator>>(-1, -1, -1);
        public USamplerSurface()
        {
            Icon.Size = new Vector2(25, 25);
            Icon.Color = 0xFF00FF00;
            TitleColor = 0xFF204020;
            BackColor = 0x80808080;

            AddInput(SurfacePin, "Surface", HeightCreator, "Surface");
            AddOutput(ResultPin, "Result", HeightCreator);
        }
        public override bool OnProcedure(UPgcGraph graph)
        {
            var result = graph.BufferCache.FindBuffer(ResultPin) as USuperBuffer<float, FFloatOperator>;
            var surface = graph.BufferCache.FindBuffer(SurfacePin) as USuperBuffer<FSquareSurface, FSquareSurfaceOperator>;
            var rangU = UEnd - UStart;
            var rangV = VEnd - VStart;
            for (int i = 0; i < result.Height; i++)
            {
                float v = i * rangV / result.Height;
                for (int j = 0; j < result.Width; j++)
                {
                    float u = j * rangU / result.Width;
                    var h = UImageSurface.GetHeight(surface, UStart + u, VStart + v);
                    result.SetFloat1(j, i, 0, h.W);
                }
            }
            return true;
        }
    }
}
