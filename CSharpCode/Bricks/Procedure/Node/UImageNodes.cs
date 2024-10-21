using System;
using System.Collections.Generic;
using EngineNS.Bricks.NodeGraph;
using System.ComponentModel;

namespace EngineNS.Bricks.Procedure.Node
{
    [Bricks.CodeBuilder.ContextMenu("Image", "Image\\Loader", UPgcGraph.PgcEditorKeyword)]
    public partial class UImageLoader : UPgcNodeBase
    {
        [Browsable(false)]
        public PinOut RgbPin { get; set; } = new PinOut();
        [Browsable(false)]
        public PinOut RedPin { get; set; } = new PinOut();
        [Browsable(false)]
        public PinOut GreenPin { get; set; } = new PinOut();
        [Browsable(false)]
        public PinOut BluePin { get; set; } = new PinOut();
        [Browsable(false)]
        public PinOut AlphaPin { get; set; } = new PinOut();

        public UBufferCreator RgbBufferCreator { get; } = UBufferCreator.CreateInstance<USuperBuffer<Vector3, FFloat3Operator>>(-1, -1, -1);
        public UBufferCreator Float1Desc { get; } = UBufferCreator.CreateInstance<USuperBuffer<float, FFloatOperator>>(-1, -1, -1);
        public UImageLoader()
        {
            PrevSize = new Vector2(100, 100);

            Icon.Size = new Vector2(25, 25);
            Icon.Color = 0xFF00FF00;
            TitleColor = 0xFF204020;
            BackColor = 0x80808080;

            AddOutput(RgbPin, "RGB", RgbBufferCreator);
            AddOutput(RedPin, "R", Float1Desc);
            AddOutput(GreenPin, "G", Float1Desc);
            AddOutput(BluePin, "B", Float1Desc);
            AddOutput(AlphaPin, "A", Float1Desc);
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
        [RName.PGRName(FilterExts = NxRHI.TtSrView.AssetExt)]
        public RName ImageName
        {
            get => mImageName;
            set
            {
                mImageName = value;
                System.Action exec = async () =>
                {
                    TextureSRV = await TtEngine.Instance.GfxDevice.TextureManager.GetTexture(value);
                };
                exec();
            }
        }
        private NxRHI.TtSrView TextureSRV;
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
        public override UBufferCreator GetOutBufferCreator(PinOut pin)
        {
            if (RgbPin == pin)
                return RgbBufferCreator;
            return Float1Desc;
        }
        public override unsafe bool InitProcedure(UPgcGraph graph)
        {
            if (ImageName == null)
                return false;

            StbImageSharp.ImageResult image = null;

            using (var xnd = IO.TtXndHolder.LoadXnd(ImageName.Address))
            {
                if (xnd == null)
                    return false;

                var pngAttr = xnd.RootNode.TryGetAttribute("Png");
                if (pngAttr.IsValidPointer == false)
                    return false;

                byte[] data;
                using (var ar = pngAttr.GetReader(null))
                {
                    ar.ReadNoSize(out data, (int)pngAttr.GetReaderLength());
                }

                using (var memStream = new System.IO.MemoryStream(data, false))
                {
                    image = StbImageSharp.ImageResult.FromStream(memStream, StbImageSharp.ColorComponents.RedGreenBlueAlpha);
                    if (image == null)
                        return false;
                }
            }

            Float1Desc.BufferType = Rtti.TtTypeDesc.TypeOf<USuperBuffer<float, FFloatOperator>>();
            if (image != null)
            {
                UBufferComponent red = null;
                UBufferComponent green = null;
                UBufferComponent blue = null;
                UBufferComponent alpha = null;
                int PixelSize = 0;
                int LineSize = 0;
                switch (image.Comp)
                {
                    case StbImageSharp.ColorComponents.RedGreenBlue:
                        {
                            PixelSize = 3;
                            LineSize = 3 * image.Width;
                            LineSize = (int)MathHelper.Roundup((uint)LineSize, 4);

                            Float1Desc.XSize = image.Width;
                            Float1Desc.YSize = image.Height;
                            Float1Desc.ZSize = 1;

                            red = UBufferComponent.CreateInstance(UBufferCreator.CreateInstance<USuperBuffer<float, FFloatOperator>>(image.Width, image.Height, 1));
                            green = UBufferComponent.CreateInstance(UBufferCreator.CreateInstance<USuperBuffer<float, FFloatOperator>>(image.Width, image.Height, 1));
                            blue = UBufferComponent.CreateInstance(UBufferCreator.CreateInstance<USuperBuffer<float, FFloatOperator>>(image.Width, image.Height, 1));

                            var rgb = UBufferComponent.CreateInstance(UBufferCreator.CreateInstance<USuperBuffer<Vector3, FFloat3Operator>>(image.Width, image.Height, 1));

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
                                        float v = (float)p[LineSize * i + j * PixelSize] / 255.0f;
                                        rgbValue.X = v;
                                        red.SetPixel(j, i, v);

                                        v = (float)p[LineSize * i + j * PixelSize + 1] / 255.0f;
                                        rgbValue.Y = v;
                                        green.SetPixel(j, i, v);

                                        v = (float)p[LineSize * i + j * PixelSize + 2] / 255.0f;
                                        rgbValue.Z = v;
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

                            Float1Desc.XSize = image.Width;
                            Float1Desc.YSize = image.Height;
                            Float1Desc.ZSize = 1;

                            red = UBufferComponent.CreateInstance(UBufferCreator.CreateInstance<USuperBuffer<float, FFloatOperator>>(image.Width, image.Height, 1));
                            green = UBufferComponent.CreateInstance(UBufferCreator.CreateInstance<USuperBuffer<float, FFloatOperator>>(image.Width, image.Height, 1));
                            blue = UBufferComponent.CreateInstance(UBufferCreator.CreateInstance<USuperBuffer<float, FFloatOperator>>(image.Width, image.Height, 1));
                            alpha = UBufferComponent.CreateInstance(UBufferCreator.CreateInstance<USuperBuffer<float, FFloatOperator>>(image.Width, image.Height, 1));

                            var rgb = UBufferComponent.CreateInstance(UBufferCreator.CreateInstance<USuperBuffer<Vector3, FFloat3Operator>>(image.Width, image.Height, 1));

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
                cmdlist.AddImage((ulong)TextureSRV.GetTextureHandle(), in prevStart, in prevEnd, in uv0, in uv1, 0xFFFFFFFF);
            }
        }
    }
    [Bricks.CodeBuilder.ContextMenu("Preview", "Image\\Preview", UPgcGraph.PgcEditorKeyword)]
    public partial class UPreviewImage : UPgcNodeBase
    {
        [Browsable(false)]
        public PinIn InXYZPin { get; set; } = new PinIn();
        [Browsable(false)]
        public PinIn InXPin { get; set; } = new PinIn();
        [Browsable(false)]
        public PinIn InYPin { get; set; } = new PinIn();
        [Browsable(false)]
        public PinIn InZPin { get; set; } = new PinIn();
        [Browsable(false)]
        public PinOut XYZPin { get; set; } = new PinOut();
        [Browsable(false)]
        public PinOut XPin { get; set; } = new PinOut();
        [Browsable(false)]
        public PinOut YPin { get; set; } = new PinOut();
        [Browsable(false)]
        public PinOut ZPin { get; set; } = new PinOut();

        public UBufferCreator XYZBufferCreator { get; } = UBufferCreator.CreateInstance<USuperBuffer<Vector3, FFloat3Operator>>(-1, -1, -1);
        public UBufferCreator Float1Desc { get; } = UBufferCreator.CreateInstance<USuperBuffer<float, FFloatOperator>>(-1, -1, -1);
        public UBufferCreator OutputFloat1Desc { get; } = UBufferCreator.CreateInstance<USuperBuffer<float, FFloatOperator>>(-1, -1, -1);
        public UPreviewImage()
        {
            PrevSize = new Vector2(100, 100);

            Icon.Size = new Vector2(25, 25);
            Icon.Color = 0xFF00FF00;
            TitleColor = 0xFF204020;
            BackColor = 0x80808080;

            AddInput(InXYZPin, "InXYZ", XYZBufferCreator);
            AddInput(InXPin, "InX", Float1Desc);
            AddInput(InYPin, "InY", Float1Desc);
            AddInput(InZPin, "InZ", Float1Desc);

            AddOutput(XYZPin, "XYZ", XYZBufferCreator);
            AddOutput(XPin, "X", OutputFloat1Desc);
            AddOutput(YPin, "Y", OutputFloat1Desc);
            AddOutput(ZPin, "Z", OutputFloat1Desc);
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
                cmdlist.AddImage((ulong)PreviewSRV.GetTextureHandle(), in prevStart, in prevEnd, in uv0, in uv1, 0xFFFFFFFF);
            }
        }
        public override UBufferCreator GetOutBufferCreator(PinOut pin)
        {
            if (pin == XYZPin)
            {
                return XYZBufferCreator;
            }
            return OutputFloat1Desc;
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
    [Bricks.CodeBuilder.ContextMenu("Surface", "Image\\Surface", UPgcGraph.PgcEditorKeyword)]
    public partial class UImageSurface : UPgcNodeBase
    {
        [Browsable(false)]
        public PinIn HeightPin { get; set; } = new PinIn();
        [Browsable(false)]
        public PinIn NormPin { get; set; } = new PinIn();
        [Browsable(false)]
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
                    SurfCreator.SetSize(buffer.BufferCreator);
                    return SurfCreator;
                }
            }
            return null;
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
        public static Vector4 GetHeight(UBufferComponent buffer, float u, float v)
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
        [Browsable(false)]
        public PinIn SurfacePin { get; set; } = new PinIn();
        [Browsable(false)]
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
        public override UBufferCreator GetOutBufferCreator(PinOut pin)
        {
            return HeightCreator;
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

    public class UKernal : IO.BaseSerializer
    {
        public UKernal()
        {
            KernalDefine.HostNode = this;
            UpdateKernal();
        }
        uint mKernalX = 3;
        uint mKernalY = 3;
        [Rtti.Meta]
        public uint KernalX
        {
            get => mKernalX;
            set
            {
                mKernalX = value;
                UpdateKernal();
            }
        }
        [Rtti.Meta]
        public uint KernalY
        {
            get => mKernalY;
            set
            {
                mKernalY = value;
                UpdateKernal();
            }
        }
        [Rtti.Meta]
        [Browsable(false)]
        public List<float> KernalValues { get; set; }
        public class UKernalValueDefine
        {
            internal UKernal HostNode;
            public class UValueEditorAttribute : EGui.Controls.PropertyGrid.PGCustomValueEditorAttribute
            {
                public unsafe override bool OnDraw(in EditorInfo info, out object newValue)
                {
                    newValue = info.Value;
                    var nodeDef = newValue as UKernalValueDefine;
                    var kernal = nodeDef.HostNode;
                    for (int i = 0; i < kernal.KernalY; i++)
                    {
                        for (int j = 0; j < kernal.KernalX; j++)
                        {
                            float v = kernal.KernalValues[i * (int)kernal.KernalX + j];
                            ImGuiAPI.SetNextItemWidth(100);
                            if (ImGuiAPI.InputFloat($"##{i}_{j}", ref v, 0, 0, "%.3f", ImGuiInputTextFlags_.ImGuiInputTextFlags_None))
                            {
                                kernal.KernalValues[i * (int)kernal.KernalX + j] = v;
                            }
                            if (j < kernal.KernalX - 1)
                                ImGuiAPI.SameLine(0, 5);
                        }
                    }
                    return false;
                }
            }
        }
        [UKernalValueDefine.UValueEditor]
        public UKernalValueDefine KernalDefine
        {
            get;
        } = new UKernalValueDefine();
        private void UpdateKernal()
        {
            if (KernalValues != null && KernalValues.Count == KernalX * KernalY)
            {
                return;
            }
            var kv = new List<float>();
            for (int i = 0; i < KernalY; i++)
            {
                for (int j = 0; j < KernalX; j++)
                {
                    kv.Add(1.0f);
                }
            }
            KernalValues = kv;
        }
        public float GetValue(float u, float v)
        {
            int x = (int)(u * KernalX);
            int y = (int)(v * KernalY);
            return KernalValues[y * (int)KernalX + x];
        }
        public struct FSamplerData
        {
            public float Value;
            public int Flags;
            public void SetValid(bool v)
            {
                if (v)
                    Flags |= (1 << 0);
                else
                    Flags = Flags & (~(1 << 0));
            }
            public bool IsValid()
            {
                return (Flags & (1 << 0)) != 0;
            }
            public void SetZeroWeight(bool v)
            {
                if (v)
                    Flags |= (1 << 1);
                else
                    Flags = Flags & (~(1 << 1));
            }
            public bool IsZeroWeight()
            {
                return (Flags & (1 << 1)) != 0;
            }
        }
        public unsafe void Sampler(FSamplerData* tmpBuffer, int scale, UBufferComponent src, int x, int y, int z)
        {
            var sizeX = (int)KernalX * scale;
            var sizeY = (int)KernalY * scale;
            int sx = x - sizeX / 2;
            int sy = y - sizeY / 2;
            for (int i = 0; i < sizeY; i++)
            {
                for (int j = 0; j < sizeX; j++)
                {
                    if (src.IsValidPixel(sx + j, sy + i, z))
                    {
                        float u = (float)j / (float)(sizeX);
                        float v = (float)i / (float)(sizeY);
                        var testValue = GetValue(u, v);
                        tmpBuffer[i * sizeX + j].SetZeroWeight(testValue == 0);
                        tmpBuffer[i * sizeX + j].SetValid(true);
                        float value = src.GetFloat1(sx + j, sy + i, z);
                        if (testValue == 0)
                            tmpBuffer[i * sizeX + j].Value = value;
                        else
                            tmpBuffer[i * sizeX + j].Value = value * testValue;
                    }
                    else
                    {
                        tmpBuffer[i * sizeX + j].Value = 0;
                        tmpBuffer[i * sizeX + j].SetValid(false);
                        tmpBuffer[i * sizeX + j].SetZeroWeight(true);
                    }
                }
            }
        }
    }
    [Bricks.CodeBuilder.ContextMenu("Erosion", "Image\\Erosion", UPgcGraph.PgcEditorKeyword)]
    public class UErosion : UMonocular
    {
        [Rtti.Meta]
        public UKernal Kernal { get; set; } = new UKernal();
        [Rtti.Meta]
        public int KernalScale { get; set; } = 1;
        [Rtti.Meta]
        public float DeltaLimit { get; set; } = 0.5f;
        public float ErosionScale { get; set; } = 1.0f;
        public unsafe override bool OnProcedure(UPgcGraph graph)
        {
            var left = graph.BufferCache.FindBuffer(SrcPin);
            var result = graph.BufferCache.FindBuffer(ResultPin);
            var op = result.PixelOperator;

            var resultType = result.BufferCreator.ElementType;
            var leftType = left.BufferCreator.ElementType;
            var rightType = Rtti.TtTypeDescGetter<float>.TypeDesc;

            result.DispatchPixels((result, x, y, z) =>
            {
                unsafe
                {
                    int count = (int)(Kernal.KernalX * KernalScale * Kernal.KernalY * KernalScale);
                    var tmpBuffer = stackalloc UKernal.FSamplerData[count];
                    Kernal.Sampler(tmpBuffer, KernalScale, left, x, y, z);

                    var ov = left.GetFloat1(x, y, z);
                    float minValue = float.MaxValue;
                    bool isErosion = false;
                    for (int i = 0; i < count; i++)
                    {
                        if (tmpBuffer[i].IsValid() == false)
                        {
                            result.SetFloat1(x, y, z, ov);
                            return;
                        }
                        else if (tmpBuffer[i].IsZeroWeight() == false)
                        {
                            var delta = ov - tmpBuffer[i].Value;
                            if (delta > DeltaLimit)
                            {
                                isErosion = true;
                            }
                        }

                        if (tmpBuffer[i].Value < minValue)
                            minValue = tmpBuffer[i].Value;
                    }
                    if (isErosion)
                        result.SetFloat1(x, y, z, minValue * ErosionScale);
                    else
                        result.SetFloat1(x, y, z, ov);
                }
            }, true);

            left.LifeCount--;
            return true;
        }
    }
}
