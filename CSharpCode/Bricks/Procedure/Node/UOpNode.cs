using System;
using System.Collections.Generic;
using EngineNS.Bricks.NodeGraph;

namespace EngineNS.Bricks.Procedure.Node
{
    public partial class UOpNode : UPgcNodeBase
    {
        [Rtti.Meta]
        public int DefaultWidth { get; set; } = -1;
        [Rtti.Meta]
        public int DefaultHeight { get; set; } = -1;
        public override Int32_2 GetOutPinSize(PinOut pin)
        {
            var graph = this.ParentGraph as UPgcGraph;
            Int32_2 result = new Int32_2(DefaultWidth, DefaultHeight);
            if (result.x == -1)
            {
                result.x = graph.DefaultWidth;
            }
            if (result.y == -1)
            {
                result.y = graph.DefaultHeight;
            }
            return result;
        }
        [Rtti.Meta]
        public UPgcNodeBase GetInputNode(UPgcGraph graph, int index)
        {
            var linker = graph.FindInLinkerSingle(Inputs[index]);
            if (linker == null)
                return null;
            return linker.OutNode as UPgcNodeBase;
        }
        [Rtti.Meta]
        public UPgcNodeBase GetInputNode(UPgcGraph graph, PinIn pin)
        {
            var linker = graph.FindInLinkerSingle(pin);
            if (linker == null)
                return null;
            return linker.OutNode as UPgcNodeBase;
        }
    }
    public class UImageLoader : UPgcNodeBase
    {
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public PinOut RedPin { get; set; } = new PinOut();
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public PinOut GreenPin { get; set; } = new PinOut();
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public PinOut BluePin { get; set; } = new PinOut();
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public PinOut AlphaPin { get; set; } = new PinOut();
        public UImageLoader()
        {
            PrevSize = new Vector2(100, 100);

            RedPin.Name = " R";
            GreenPin.Name = " G";
            BluePin.Name = " B";
            AlphaPin.Name = " A";

            RedPin.Link = UPgcEditorStyles.Instance.NewInOutPinDesc();
            GreenPin.Link = UPgcEditorStyles.Instance.NewInOutPinDesc();
            BluePin.Link = UPgcEditorStyles.Instance.NewInOutPinDesc();
            AlphaPin.Link = UPgcEditorStyles.Instance.NewInOutPinDesc();

            Icon.Size = new Vector2(25, 25);
            Icon.Color = 0xFF00FF00;
            TitleColor = 0xFF204020;
            BackColor = 0x80808080;

            AddPinOut(RedPin);
            AddPinOut(GreenPin);
            AddPinOut(BluePin);
            AddPinOut(AlphaPin);
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
                            red = new UBufferConponent();
                            green = new UBufferConponent();
                            blue = new UBufferConponent();

                            red.SetSize(image.Width, image.Height, 0);
                            graph.BufferCache.RegBuffer(RedPin, red);

                            green.SetSize(image.Width, image.Height, 0);
                            graph.BufferCache.RegBuffer(GreenPin, green);

                            blue.SetSize(image.Width, image.Height, 0);
                            graph.BufferCache.RegBuffer(BluePin, blue);

                            fixed (byte* p = &image.Data[0])
                            {
                                for (int i = 0; i < image.Height; i++)
                                {
                                    for (int j = 0; j < image.Width; j++)
                                    {
                                        float v = (float)p[LineSize * i + j * PixelSize] / 256.0f;
                                        red.SetPixel(j, i, v);

                                        v = (float)p[LineSize * i + j * PixelSize + 1] / 256.0f;
                                        green.SetPixel(j, i, v);

                                        v = (float)p[LineSize * i + j * PixelSize + 2] / 256.0f;
                                        blue.SetPixel(j, i, v);
                                    }
                                }
                            }
                        }
                        break;
                    case StbImageSharp.ColorComponents.RedGreenBlueAlpha:
                        {
                            PixelSize = 4;
                            LineSize = 4 * image.Width;
                            red = new UBufferConponent();
                            green = new UBufferConponent();
                            blue = new UBufferConponent();
                            alpha = new UBufferConponent();

                            red.SetSize(image.Width, image.Height, 0);
                            graph.BufferCache.RegBuffer(RedPin, red);

                            green.SetSize(image.Width, image.Height, 0);
                            graph.BufferCache.RegBuffer(GreenPin, green);

                            blue.SetSize(image.Width, image.Height, 0);
                            graph.BufferCache.RegBuffer(BluePin, blue);

                            alpha.SetSize(image.Width, image.Height, 0);
                            graph.BufferCache.RegBuffer(AlphaPin, alpha);

                            fixed (byte* p = &image.Data[0])
                            {
                                for (int i = 0; i < image.Height; i++)
                                {
                                    for (int j = 0; j < image.Width; j++)
                                    {
                                        float v = (float)p[LineSize * i + j * PixelSize] / 255.0f;
                                        blue.SetPixel(j, i, v);

                                        v = (float)p[LineSize * i + j * PixelSize + 1] / 255.0f;
                                        green.SetPixel(j, i, v);

                                        v = (float)p[LineSize * i + j * PixelSize + 2] / 255.0f;
                                        red.SetPixel(j, i, v);

                                        v = (float)p[LineSize * i + j * PixelSize + 3] / 255.0f;
                                        alpha.SetPixel(j, i, v);
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
    public class UMonocular : UOpNode
    {
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public PinIn SrcPin { get; set; } = new PinIn();
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public PinOut ResultPin { get; set; } = new PinOut();
        
        public UMonocular()
        {
            SrcPin.Name = " Src";
            ResultPin.Name = " Result";

            SrcPin.Link = UPgcEditorStyles.Instance.NewInOutPinDesc();
            ResultPin.Link = UPgcEditorStyles.Instance.NewInOutPinDesc();

            Icon.Size = new Vector2(25, 25);
            Icon.Color = 0xFF00FF00;
            TitleColor = 0xFF204020;
            BackColor = 0x80808080;

            AddPinIn(SrcPin);
            AddPinOut(ResultPin);
        }

        public override Int32_2 GetOutPinSize(PinOut pin)
        {
            if(ResultPin == pin)
            {
                var graph = ParentGraph as UPgcGraph;
                var buffer = graph.BufferCache.FindBuffer(SrcPin);
                if (buffer != null)
                {
                    return new Int32_2(buffer.Width, buffer.Height); 
                }
            }
            return base.GetOutPinSize(pin);
        }
    }
    public class UBinocular : UOpNode
    {
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public PinIn LeftPin { get; set; } = new PinIn();
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public PinIn RightPin { get; set; } = new PinIn();
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public PinOut ResultPin { get; set; } = new PinOut();
        public UBinocular()
        {
            LeftPin.Name = " Left";
            RightPin.Name = " Right";
            ResultPin.Name = " Result";

            LeftPin.Link = UPgcEditorStyles.Instance.NewInOutPinDesc();
            RightPin.Link = UPgcEditorStyles.Instance.NewInOutPinDesc();
            ResultPin.Link = UPgcEditorStyles.Instance.NewInOutPinDesc();

            Icon.Size = new Vector2(25, 25);
            Icon.Color = 0xFF00FF00;
            TitleColor = 0xFF204020;
            BackColor = 0x80808080;

            AddPinIn(LeftPin);
            AddPinIn(RightPin);
            AddPinOut(ResultPin);
        }
        public override Int32_2 GetOutPinSize(PinOut pin)
        {
            if (ResultPin == pin)
            {
                var graph = ParentGraph as UPgcGraph;
                var buffer = graph.BufferCache.FindBuffer(LeftPin);
                if (buffer != null)
                {
                    return new Int32_2(buffer.Width, buffer.Height);
                }
            }
            return base.GetOutPinSize(pin);
        }
    }

    public class UCopyRect : UMonocular
    {
        [Rtti.Meta]
        public int X { get; set; } = 0;
        [Rtti.Meta]
        public int Y { get; set; } = 0;
        public override Int32_2 GetOutPinSize(PinOut pin)
        {
            var graph = this.ParentGraph as UPgcGraph;
            Int32_2 result = new Int32_2(DefaultWidth, DefaultHeight);
            if (result.x == -1)
            {
                result.x = graph.DefaultWidth;
            }
            if (result.y == -1)
            {
                result.y = graph.DefaultHeight;
            }
            return result;
        }
        public override bool OnProcedure(UPgcGraph graph)
        {
            var curComp = graph.BufferCache.FindBuffer(SrcPin);
            var resultComp = graph.BufferCache.FindBuffer(ResultPin);
            for (int i = 0; i < resultComp.Height; i++)
            {
                for (int j = 0; j < resultComp.Width; j++)
                {
                    var pxl = curComp.GetPixel(X + j, Y + i);

                    resultComp.SetPixel(j, i, pxl);
                }
            }
            curComp.LifeCount--;
            return true;
        }
    }

    public class UStretch : UMonocular
    {
        public override Int32_2 GetOutPinSize(PinOut pin)
        {
            var graph = this.ParentGraph as UPgcGraph;
            Int32_2 result = new Int32_2(DefaultWidth, DefaultHeight);
            if (result.x == -1)
            {
                result.x = graph.DefaultWidth;
            }
            if (result.y == -1)
            {
                result.y = graph.DefaultHeight;
            }
            return result;
        }
        public override bool OnProcedure(UPgcGraph graph)
        {
            var curComp = graph.BufferCache.FindBuffer(SrcPin);
            var resultComp = graph.BufferCache.FindBuffer(ResultPin);
            for (int i = 0; i < resultComp.Height; i++)
            {
                for (int j = 0; j < resultComp.Width; j++)
                {
                    float x = (float)(j * curComp.Width) / (float)resultComp.Width;
                    float y = (float)(i * curComp.Height) / (float)resultComp.Height;
                    var pxl = curComp.GetPixel((int)x, (int)y);

                    resultComp.SetPixel(j, i, pxl);
                }
            }
            curComp.LifeCount--;
            return true;
        }
    }

    public class UMulValue : UMonocular
    {
        public UMulValue()
        {
            PrevSize = new Vector2(70, 30);
        }
        [Rtti.Meta]
        public float Value { get; set; } = 1.0f;
        public override bool OnProcedure(UPgcGraph graph)
        {
            var curComp = graph.BufferCache.FindBuffer(SrcPin);
            var resultComp = graph.BufferCache.FindBuffer(ResultPin);
            for (int i = 0; i < curComp.Height; i++)
            {
                for (int j = 0; j < curComp.Width; j++)
                {
                    var pxl = curComp.GetPixel(j, i);

                    resultComp.SetPixel(j, i, pxl * Value);
                }
            }
            curComp.LifeCount--;
            return true;
        }

        public unsafe override void OnPreviewDraw(in Vector2 prevStart, in Vector2 prevEnd, ImDrawList cmdlist)
        {
            unsafe
            {
                cmdlist.AddText(in prevStart, 0xFFFFFFFF, $"{Value}", null);
            }
        }
    }

    public class USmoothGaussion : UMonocular
    {
        static float[,] BlurMatrix = 
        {
            { 0.0947416f, 0.118318f, 0.0947416f },
            { 0.118318f, 0.147761f, 0.118318f },
            { 0.0947416f, 0.118318f, 0.0947416f }
        };
        [Rtti.Meta]
        public bool ClampBorder { get; set; } = true;
        public override bool OnProcedure(UPgcGraph graph)
        {
            //for (int n = 0; n < SmoothNum; n++)
            {
                var curComp = graph.BufferCache.FindBuffer(SrcPin);
                var resultComp = graph.BufferCache.FindBuffer(ResultPin);
                float[,] pixels = new float[3, 3];
                for (int i = 0; i < curComp.Height; i++)
                {
                    for (int j = 0; j < curComp.Width; j++)
                    {
                        var center = curComp.GetPixel(j, i);
                        pixels[1, 1] = center;

                        var v = curComp.GetPixel(j - 1, i - 1);
                        {//line1
                            if (v != float.MaxValue)
                                pixels[0, 0] = v;
                            else
                                pixels[0, 0] = center;

                            v = curComp.GetPixel(j, i - 1);
                            if (v != float.MaxValue)
                                pixels[0, 1] = v;
                            else
                                pixels[0, 1] = center;

                            v = curComp.GetPixel(j + 1, i - 1);
                            if (v != float.MaxValue)
                                pixels[0, 2] = v;
                            else
                                pixels[0, 2] = center;
                        }

                        {//line2
                            v = curComp.GetPixel(j - 1, i);
                            if (v != float.MaxValue)
                                pixels[1, 0] = v;
                            else
                                pixels[1, 0] = center;

                            v = curComp.GetPixel(j + 1, i);
                            if (v != float.MaxValue)
                                pixels[1, 2] = v;
                            else
                                pixels[1, 2] = center;
                        }

                        {//line3
                            v = curComp.GetPixel(j - 1, i + 1);
                            if (v != float.MaxValue)
                                pixels[2, 0] = v;
                            else
                                pixels[2, 0] = center;

                            v = curComp.GetPixel(j, i + 1);
                            if (v != float.MaxValue)
                                pixels[2, 1] = v;
                            else
                                pixels[2, 1] = center;

                            v = curComp.GetPixel(j + 1, i + 1);
                            if (v != float.MaxValue)
                                pixels[2, 2] = v;
                            else
                                pixels[2, 2] = center;
                        }

                        float value = pixels[0, 0] * BlurMatrix[0, 0] + pixels[0, 1] * BlurMatrix[0, 1] + pixels[0, 2] * BlurMatrix[0, 2]
                            + pixels[1, 0] * BlurMatrix[1, 0] + pixels[1, 1] * BlurMatrix[1, 1] + pixels[1, 2] * BlurMatrix[1, 2]
                            + pixels[2, 0] * BlurMatrix[2, 0] + pixels[2, 1] * BlurMatrix[2, 1] + pixels[2, 2] * BlurMatrix[2, 2];
                        if (ClampBorder)
                        {
                            if (i == 0 || j == 0 || i == curComp.Height - 1 || j == curComp.Width - 1)
                            {
                                resultComp.SetPixel(j, i, center);
                            }
                            else
                            {
                                resultComp.SetPixel(j, i, value);
                            }
                        }
                        else
                        {
                            resultComp.SetPixel(j, i, value);
                        }
                    }
                }

                curComp.LifeCount--;
            }
            
            return true;
        }
    }
    public class UNoisePerlin : UOpNode
    {
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public PinOut ResultPin { get; set; } = new PinOut();
        public UNoisePerlin()
        {
            ResultPin.Name = " Result";

            ResultPin.Link = UPgcEditorStyles.Instance.NewInOutPinDesc();

            Icon.Size = new Vector2(25, 25);
            Icon.Color = 0xFF00FF00;
            TitleColor = 0xFF204020;
            BackColor = 0x80808080;

            AddPinOut(ResultPin);
        }
        int mOctaves = 3;
        [Rtti.Meta]
        public int Octaves 
        {
            get => mOctaves;
            set
            {
                mOctaves = value;
                //UpdatePerlin();
            }
        }
        float mFreq = 3.5f;
        [Rtti.Meta]
        public float Freq 
        {
            get => mFreq;
            set
            {
                mFreq = value;
                //UpdatePerlin();
            }
        }
        float mAmptitude = 10.0f;
        [Rtti.Meta]
        public float Amptitude 
        {
            get => mAmptitude;
            set
            {
                mAmptitude = value;
                //UpdatePerlin();
            }
        }
        int mSeed = (int)Support.Time.GetTickCount();
        [Rtti.Meta]
        public int Seed 
        { 
            get => mSeed;
            set
            {
                mSeed = value;
                //UpdatePerlin();
            }
        }
        int mSamplerSize = 1024;
        [Rtti.Meta]
        public int SamplerSize
        {
            get
            {
                return mSamplerSize;
            }
            set
            {
                mSamplerSize = value;
                //UpdatePerlin();
            }
        }

        [Rtti.Meta]
        public float GridSize { get; set; } = 1.0f;
        protected Support.CPerlin mPerlin;
        protected void UpdatePerlin()
        {
            mPerlin = new Support.CPerlin(Octaves, Freq, Amptitude, Seed, SamplerSize);
        }
        public DVector3 StartPosition = DVector3.Zero;
        public override bool InitProcedure(UPgcGraph graph)
        {
            UpdatePerlin();
            return true;
        }
        public override bool OnProcedure(UPgcGraph graph)
        {
            var resultComp = graph.BufferCache.FindBuffer(ResultPin);

            float XScale = 1.0f * GridSize / (resultComp.Width - 1);
            float YScale = 1.0f * GridSize / (resultComp.Height - 1);
            for (int i = 0; i < resultComp.Height; i++)
            {
                for (int j = 0; j < resultComp.Width; j++)
                {
                    var value = (float)mPerlin.mCoreObject.Get(StartPosition.X + GridSize * j + XScale * j, StartPosition.Z + GridSize * i + YScale * i);
                    resultComp.SetPixel(j, i, value);
                }
            }
            return true;
        }
    }

    public class UBezier : UOpNode
    {
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public PinOut ResultPin { get; set; } = new PinOut();
        public EGui.Controls.BezierControl mBezierCtrl = new EGui.Controls.BezierControl();
        public EGui.Controls.BezierControl BezierCtrl
        {
            get => mBezierCtrl;
        }
        [Rtti.Meta]
        public Vector2 GraphSize
        {
            get
            {
                return PrevSize;
            }
            set
            {
                PrevSize = value;
                this.OnPositionChanged();
            }
        }

        [Rtti.Meta]
        public float MinX
        {
            get => mBezierCtrl.MinX;
            set
            {
                mBezierCtrl.MinX = value;
            }
        }
        [Rtti.Meta]
        public float MaxX
        {
            get => mBezierCtrl.MaxX;
            set
            {
                mBezierCtrl.MaxX = value;
            }
        }
        [Rtti.Meta]
        public float MinY
        {
            get => mBezierCtrl.MinY;
            set
            {
                mBezierCtrl.MinY = value;
            }
        }
        [Rtti.Meta]
        public float MaxY
        {
            get => mBezierCtrl.MaxY;
            set
            {
                mBezierCtrl.MaxY = value;
            }
        }
        [Rtti.Meta]
        public List<BezierPointBase> BzPoints 
        {
            get => mBezierCtrl.BezierPoints; 
            set
            {
                mBezierCtrl.BezierPoints = value;
            }
        }
        public bool LockLinkedControlPoint
        {
            get => mBezierCtrl.LockLinkedControlPoint;
            set
            {
                mBezierCtrl.LockLinkedControlPoint = value;
            }
        }
        public UBezier()
        {
            PrevSize = new Vector2(200, 60);
            ResultPin.Name = " Result";

            ResultPin.Link = UPgcEditorStyles.Instance.NewInOutPinDesc("Bezier");

            Icon.Size = new Vector2(25, 25);
            Icon.Color = 0xFF00FF00;
            TitleColor = 0xFF204020;
            BackColor = 0x80808080;

            AddPinOut(ResultPin);

            mBezierCtrl.Initialize(MinX, MinY, MaxX, MaxY);
        }
        public unsafe override void OnPreviewDraw(in Vector2 prevStart, in Vector2 prevEnd, ImDrawList cmdlist)
        {
            mBezierCtrl.OnDrawCanvas(in prevStart, prevEnd - prevStart);
        }
    }

    public class UStretchBlt : UBinocular
    {
        [Rtti.Meta]
        public uint SrcX { get; set; } = 0;
        [Rtti.Meta]
        public uint SrcY { get; set; } = 0;
        [Rtti.Meta]
        public int SrcW { get; set; } = -1;
        [Rtti.Meta]
        public int SrcH { get; set; } = -1;

        [Rtti.Meta]
        public uint DstX { get; set; } = 0;
        [Rtti.Meta]
        public uint DstY { get; set; } = 0;
        [Rtti.Meta]
        public int DstW { get; set; } = -1;
        [Rtti.Meta]
        public int DstH { get; set; } = -1;
        public override Int32_2 GetOutPinSize(PinOut pin)
        {
            if (ResultPin == pin)
            {
                var graph = ParentGraph as UPgcGraph;
                var buffer = graph.BufferCache.FindBuffer(LeftPin);
                if (buffer != null)
                {
                    return new Int32_2(buffer.Width, buffer.Height);
                }
            }
            return base.GetOutPinSize(pin);
        }
        public override bool OnProcedure(UPgcGraph graph)
        {
            var left = graph.BufferCache.FindBuffer(LeftPin);
            var right = graph.BufferCache.FindBuffer(RightPin);
            var resultComp = graph.BufferCache.FindBuffer(ResultPin);
            //Copy Left to Result
            for (int i = 0; i < resultComp.Height; i++)
            {
                for (int j = 0; j < resultComp.Width; j++)
                {
                    var pxl = left.GetPixel(j, i);

                    resultComp.SetPixel(j, i, pxl);
                }
            }
            int width = DstW;
            int height = DstH;
            if (width < 0)
            {
                width = left.Width - (int)SrcX;
            }
            if (height < 0)
            {
                height = left.Height - (int)SrcY;
            }
            int srcwidth = SrcW;
            int srcheight = SrcH;
            if (srcwidth < 0)
            {
                srcwidth = right.Width - (int)DstX;
            }
            if (srcheight < 0)
            {
                srcheight = right.Height - (int)DstY;
            }
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    float x = (float)(j * srcwidth) / (float)width;
                    float y = (float)(i * srcheight) / (float)height;
                    
                    var pxl = right.GetPixel((int)SrcX + (int)x, (int)SrcY + (int)y);
                    resultComp.SetPixel((int)DstX + j, (int)DstY + i, pxl);
                }
            }

            left.LifeCount--;
            right.LifeCount--;
            return true;
        }
    }
    public class UPixelAdd : UBinocular
    {
        public override bool OnProcedure(UPgcGraph graph)
        {
            var left = graph.BufferCache.FindBuffer(LeftPin);
            var right = graph.BufferCache.FindBuffer(RightPin);
            if (right != null)
            {
                if (left.Width != right.Width || left.Height != right.Height)
                {
                    left.LifeCount--;
                    right.LifeCount--;
                    return false;
                }
            }
            
            var result = graph.BufferCache.FindBuffer(ResultPin);
            for (int i = 0; i < result.Height; i++)
            {
                for (int j = 0; j < result.Width; j++)
                {
                    float rValue = 0;
                    if (right != null)
                    {
                        rValue = right.GetPixel(j, i);
                    }
                    result.SetPixel(j, i, left.GetPixel(j, i) + rValue);
                }
            }
            left.LifeCount--;
            if (right != null)
                right.LifeCount--;
            return true;
        }
    }
    public class UPixelSub : UBinocular
    {
        public override bool OnProcedure(UPgcGraph graph)
        {
            var left = graph.BufferCache.FindBuffer(LeftPin);
            var right = graph.BufferCache.FindBuffer(RightPin);
            if (right != null)
            {
                if (left.Width != right.Width || left.Height != right.Height)
                {
                    left.LifeCount--;
                    right.LifeCount--;
                    return false;
                }
            }

            var result = graph.BufferCache.FindBuffer(ResultPin);
            for (int i = 0; i < result.Height; i++)
            {
                for (int j = 0; j < result.Width; j++)
                {
                    float rValue = 0;
                    if (right != null)
                    {
                        rValue = right.GetPixel(j, i);
                    }
                    result.SetPixel(j, i, left.GetPixel(j, i) - rValue);
                }
            }
            left.LifeCount--;
            if (right != null)
                right.LifeCount--;
            return true;
        }
    }
    public class UPixelMul : UBinocular
    {
        public override bool OnProcedure(UPgcGraph graph)
        {
            var left = graph.BufferCache.FindBuffer(LeftPin);
            var right = graph.BufferCache.FindBuffer(RightPin);
            if (right != null)
            {
                if (left.Width != right.Width || left.Height != right.Height)
                {
                    left.LifeCount--;
                    right.LifeCount--;
                    return false;
                }
            }

            var result = graph.BufferCache.FindBuffer(ResultPin);
            for (int i = 0; i < result.Height; i++)
            {
                for (int j = 0; j < result.Width; j++)
                {
                    float rValue = 0;
                    if (right != null)
                    {
                        rValue = right.GetPixel(j, i);
                    }
                    result.SetPixel(j, i, left.GetPixel(j, i) * rValue);
                }
            }
            left.LifeCount--;
            if (right != null)
                right.LifeCount--;
            return true;
        }
    }
    public class UPixelDiv : UBinocular
    {
        public override bool OnProcedure(UPgcGraph graph)
        {
            var left = graph.BufferCache.FindBuffer(LeftPin);
            var right = graph.BufferCache.FindBuffer(RightPin);
            if (right != null)
            {
                if (left.Width != right.Width || left.Height != right.Height)
                {
                    left.LifeCount--;
                    right.LifeCount--;
                    return false;
                }
            }

            var result = graph.BufferCache.FindBuffer(ResultPin);
            for (int i = 0; i < result.Height; i++)
            {
                for (int j = 0; j < result.Width; j++)
                {
                    float rValue = 0;
                    if (right != null)
                    {
                        rValue = right.GetPixel(j, i);
                    }
                    result.SetPixel(j, i, left.GetPixel(j, i) / rValue);
                }
            }
            left.LifeCount--;
            if (right != null)
                right.LifeCount--;
            return true;
        }
    }
    /*
     0 1 2
     3 c 4
     5 6 7
     */
    public class UCalcNormal : UPgcNodeBase
    {
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public PinIn HFieldPin { get; set; } = new PinIn(); 
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public PinOut XPin { get; set; } = new PinOut();
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public PinOut YPin { get; set; } = new PinOut();
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public PinOut ZPin { get; set; } = new PinOut();
        public UCalcNormal()
        {
            HFieldPin.Name = " Height";
            XPin.Name = "X";
            YPin.Name = "Y";
            ZPin.Name = "Z";

            HFieldPin.Link = UPgcEditorStyles.Instance.NewInOutPinDesc();
            XPin.Link = UPgcEditorStyles.Instance.NewInOutPinDesc();
            YPin.Link = UPgcEditorStyles.Instance.NewInOutPinDesc();
            ZPin.Link = UPgcEditorStyles.Instance.NewInOutPinDesc();

            Icon.Size = new Vector2(25, 25);
            Icon.Color = 0xFF00FF00;
            TitleColor = 0xFF204020;
            BackColor = 0x80808080;

            AddPinIn(HFieldPin);
            AddPinOut(XPin);
            AddPinOut(YPin);
            AddPinOut(ZPin);
        }
        public override Int32_2 GetOutPinSize(PinOut pin)
        {
            if (XPin == pin || YPin == pin || ZPin == pin)
            {
                var graph = ParentGraph as UPgcGraph;
                var buffer = graph.BufferCache.FindBuffer(HFieldPin);
                if (buffer != null)
                {
                    return new Int32_2(buffer.Width, buffer.Height);
                }
            }
            return base.GetOutPinSize(pin);
        }
        [Rtti.Meta]
        public float GridSize { get; set; } = 1.0f;
        //public float HeightRange;
        public override bool OnProcedure(UPgcGraph graph)
        {
            var heightFiels = graph.BufferCache.FindBuffer(HFieldPin);
            var xResult = graph.BufferCache.FindBuffer(XPin);
            var yResult = graph.BufferCache.FindBuffer(YPin);
            var zResult = graph.BufferCache.FindBuffer(ZPin);
            float minHeight, maxHeight;
            heightFiels.GetRange(out minHeight, out maxHeight);
            //float range = HeightRange;// maxHeight - minHeight;
            for (int i = 1; i < heightFiels.Height - 1; i++)
            {
                for (int j = 1; j < heightFiels.Width - 1; j++)
                {
                    #region old
                    //float h1 = heightFiels.GetPixel(j, i);
                    //float h2 = heightFiels.GetPixel(j + 1, i);
                    //float h3 = heightFiels.GetPixel(j, i + 1);
                    //float h4 = heightFiels.GetPixel(j + 1, i + 1);

                    //var V1 = Vector3.Normalize(new Vector3(1.0f, 0.0f, h2 - h1));//正方形边
                    //var V2 = Vector3.Normalize(new Vector3(1.0f, 0.0f, h4 - h3));//正方形边
                    //var V3 = Vector3.Normalize(new Vector3(1.0f, -1.0f, h4 - h1));//斜边
                    //var V4 = Vector3.Normalize(new Vector3(-1.0f, -1.0f, h3 - h2));//斜边

                    //var Normal1 = Vector3.Cross(in V4, in V1);
                    //var Normal2 = Vector3.Cross(in V3, in V1);
                    //var Normal3 = Vector3.Cross(in V3, in V2);
                    //var Normal4 = Vector3.Cross(in V4, in V2);

                    //var n = Vector3.Normalize(Normal1 + Normal2 + Normal3 + Normal4);

                    //{
                    //    var hc = (heightFiels.GetPixel(j, i) - minHeight);
                    //    var h0 = (heightFiels.GetPixel(j - 1, i + 1) - minHeight);
                    //    var h1 = (heightFiels.GetPixel(j, i + 1) - minHeight);
                    //    var h2 = (heightFiels.GetPixel(j + 1, i + 1) - minHeight);
                    //    var h3 = (heightFiels.GetPixel(j - 1, i) - minHeight);
                    //    var h4 = (heightFiels.GetPixel(j + 1, i) - minHeight);
                    //    var h5 = (heightFiels.GetPixel(j - 1, i - 1) - minHeight);
                    //    var h6 = (heightFiels.GetPixel(j, i - 1) - minHeight);
                    //    var h7 = (heightFiels.GetPixel(j + 1, i - 1) - minHeight);

                    //    hc /= range;
                    //    h0 /= range;
                    //    h1 /= range;
                    //    h2 /= range;
                    //    h3 /= range;
                    //    h4 /= range;
                    //    h5 /= range;
                    //    h6 /= range;
                    //    h7 /= range;

                    //    var vc = new Vector3(0, hc, 0);
                    //    var v0 = new Vector3(-1, h0, 1);
                    //    var v1 = new Vector3(0, h1, 1);
                    //    var v2 = new Vector3(1, h2, 1);
                    //    var v3 = new Vector3(-1, h3, 0);
                    //    var v4 = new Vector3(1, h4, 0);
                    //    var v5 = new Vector3(-1, h5, -1);
                    //    var v6 = new Vector3(0, h6, -1);
                    //    var v7 = new Vector3(1, h7, -1);

                    //    var n1 = Vector3.Cross(vc - v1, v0 - v1);
                    //    var n2 = Vector3.Cross(v2 - v1, vc - v1);
                    //    var n3 = Vector3.Cross(vc - v4, v2 - v4);
                    //    var n4 = Vector3.Cross(v7 - v4, vc - v4);
                    //    var n5 = Vector3.Cross(vc - v6, v7 - v6);
                    //    var n6 = Vector3.Cross(v5 - v6, vc - v6);
                    //    var n7 = Vector3.Cross(vc - v3, v5 - v3);
                    //    var n8 = Vector3.Cross(v0 - v3, vc - v3);

                    //    n1.Normalize();
                    //    n2.Normalize();
                    //    n3.Normalize();
                    //    n4.Normalize();
                    //    n5.Normalize();
                    //    n6.Normalize();
                    //    n7.Normalize();
                    //    n8.Normalize();

                    //    var n = n1 + n2 + n3 + n4 + n6 + n6 + n7 + n8;
                    //    n /= 8.0f;
                    //    n.Normalize();
                    //}
                    #endregion

                    float altInfo = heightFiels.GetPixel(j, i);
                    float v_du = heightFiels.GetPixel(j + 1, i);
                    float v_dv = heightFiels.GetPixel(j, i + 1);

                    var A = new Vector3(GridSize, (v_du - altInfo), 0);
                    var B = new Vector3(0, (v_dv - altInfo), -GridSize);

                    var n = Vector3.Cross(A, B);
                    n = Vector3.Normalize(n);

                    xResult.SetPixel(j, i, n.X);
                    yResult.SetPixel(j, i, n.Y);
                    zResult.SetPixel(j, i, n.Z);
                }
            }
            //for (int i = 1; i < heightFiels.Height - 1; i++)
            //{
            //    for (int j = 1; j < heightFiels.Width - 1; j++)
            //    {
            //        var xV = xResult.GetPixel(i, i);
            //        var yV = yResult.GetPixel(i, i);
            //        var zV = zResult.GetPixel(i, i);
            //    }
            //}
            heightFiels.LifeCount--;
            return true;
        }
    }
    public class UNormalize3D : UPgcNodeBase
    {
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public PinIn InXPin { get; set; } = new PinIn();
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public PinIn InYPin { get; set; } = new PinIn();
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public PinIn InZPin { get; set; } = new PinIn();
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public PinOut XPin { get; set; } = new PinOut();
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public PinOut YPin { get; set; } = new PinOut();
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public PinOut ZPin { get; set; } = new PinOut();
        public UNormalize3D()
        {
            InXPin.Name = " InX";
            InYPin.Name = " InY";
            InZPin.Name = " InZ";
            XPin.Name = "X";
            YPin.Name = "Y";
            ZPin.Name = "Z";

            InXPin.Link = UPgcEditorStyles.Instance.NewInOutPinDesc();
            InYPin.Link = UPgcEditorStyles.Instance.NewInOutPinDesc();
            InZPin.Link = UPgcEditorStyles.Instance.NewInOutPinDesc();
            XPin.Link = UPgcEditorStyles.Instance.NewInOutPinDesc();
            YPin.Link = UPgcEditorStyles.Instance.NewInOutPinDesc();
            ZPin.Link = UPgcEditorStyles.Instance.NewInOutPinDesc();

            Icon.Size = new Vector2(25, 25);
            Icon.Color = 0xFF00FF00;
            TitleColor = 0xFF204020;
            BackColor = 0x80808080;

            AddPinIn(InXPin);
            AddPinIn(InYPin);
            AddPinIn(InZPin);
            AddPinOut(XPin);
            AddPinOut(YPin);
            AddPinOut(ZPin);
        }
        public override Int32_2 GetOutPinSize(PinOut pin)
        {
            if (XPin == pin || YPin == pin || ZPin == pin)
            {
                var graph = ParentGraph as UPgcGraph;
                var buffer = graph.BufferCache.FindBuffer(InXPin);
                if (buffer != null)
                {
                    return new Int32_2(buffer.Width, buffer.Height);
                }
            }
            return base.GetOutPinSize(pin);
        }
        public override bool OnProcedure(UPgcGraph graph)
        {
            var xSrc = graph.BufferCache.FindBuffer(InXPin);
            var ySrc = graph.BufferCache.FindBuffer(InYPin);
            var zSrc = graph.BufferCache.FindBuffer(InZPin);

            if (xSrc.Width != ySrc.Width || xSrc.Width != zSrc.Width ||
                xSrc.Height != ySrc.Height || xSrc.Height != zSrc.Height)
            {
                return false;
            }

            var xResult = graph.BufferCache.FindBuffer(XPin);
            var yResult = graph.BufferCache.FindBuffer(YPin);
            var zResult = graph.BufferCache.FindBuffer(ZPin);
            for (int i = 1; i < xSrc.Height - 1; i++)
            {
                for (int j = 1; j < xSrc.Width - 1; j++)
                {
                    Vector3 nor;
                    nor.X = xSrc.GetPixel(j, i);
                    nor.Y = ySrc.GetPixel(j, i);
                    nor.Z = zSrc.GetPixel(j, i);
                    nor.Normalize();

                    xResult.SetPixel(j, i, nor.X);
                    yResult.SetPixel(j, i, nor.Y);
                    zResult.SetPixel(j, i, nor.Z);
                }
            }
            xSrc.LifeCount--;
            ySrc.LifeCount--;
            zSrc.LifeCount--;

            return true;
        }
    }
    public class UPreviewImage : UPgcNodeBase
    {
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public PinIn InXPin { get; set; } = new PinIn();
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public PinIn InYPin { get; set; } = new PinIn();
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public PinIn InZPin { get; set; } = new PinIn();
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public PinOut XPin { get; set; } = new PinOut();
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public PinOut YPin { get; set; } = new PinOut();
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public PinOut ZPin { get; set; } = new PinOut();
        public UPreviewImage()
        {
            PrevSize = new Vector2(100, 100);

            InXPin.Name = " InX";
            InYPin.Name = " InY";
            InZPin.Name = " InZ";
            XPin.Name = "X";
            YPin.Name = "Y";
            ZPin.Name = "Z";

            InXPin.Link = UPgcEditorStyles.Instance.NewInOutPinDesc();
            InYPin.Link = UPgcEditorStyles.Instance.NewInOutPinDesc();
            InZPin.Link = UPgcEditorStyles.Instance.NewInOutPinDesc();
            XPin.Link = UPgcEditorStyles.Instance.NewInOutPinDesc();
            YPin.Link = UPgcEditorStyles.Instance.NewInOutPinDesc();
            ZPin.Link = UPgcEditorStyles.Instance.NewInOutPinDesc();

            Icon.Size = new Vector2(25, 25);
            Icon.Color = 0xFF00FF00;
            TitleColor = 0xFF204020;
            BackColor = 0x80808080;

            AddPinIn(InXPin);
            AddPinIn(InYPin);
            AddPinIn(InZPin);
            AddPinOut(XPin);
            AddPinOut(YPin);
            AddPinOut(ZPin);
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
        public override Int32_2 GetOutPinSize(PinOut pin)
        {
            return base.GetOutPinSize(pin);
        }
        public override bool OnProcedure(UPgcGraph graph)
        {
            var xSrc = graph.BufferCache.FindBuffer(InXPin);
            var ySrc = graph.BufferCache.FindBuffer(InYPin);
            var zSrc = graph.BufferCache.FindBuffer(InZPin);
            
            graph.BufferCache.RegBuffer(XPin, xSrc);
            graph.BufferCache.RegBuffer(YPin, ySrc);
            graph.BufferCache.RegBuffer(ZPin, zSrc);

            if (graph.GraphEditor != null)
            {
                var norMap = new Bricks.Procedure.UImage2D();
                norMap.Initialize(xSrc.Width, xSrc.Height,
                    xSrc,
                    ySrc,
                    zSrc,
                    null);

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
    public class USdfCalculator : UMonocular
    {
    }

    public class UEndingNode : UOpNode
    {
        public UEndingNode()
        {
            Icon.Size = new Vector2(25, 25);
            Icon.Color = 0xFF00FF00;
            TitleColor = 0xFF204020;
            BackColor = 0x80808080;
            //PinNumber = 4;
        }
        [Rtti.Meta]
        public int PinNumber
        {
            get
            {
                return this.Inputs.Count;
            }
            set
            {
                foreach(var i in Inputs)
                {
                    this.ParentGraph.RemoveLinkedIn(i);
                }
                foreach (var i in Outputs)
                {
                    this.ParentGraph.RemoveLinkedOut(i);
                }
                this.Inputs.Clear();
                this.Outputs.Clear();
                for (int i = 0; i < value; i++)
                {
                    var inPin = new PinIn();
                    inPin.Name = $"in{i}";
                    inPin.Link = UPgcEditorStyles.Instance.NewInOutPinDesc();
                    AddPinIn(inPin);

                    var outPin = new PinOut();
                    outPin.Name = $"out{i}";
                    outPin.Link = UPgcEditorStyles.Instance.NewInOutPinDesc();
                    AddPinOut(outPin);
                }

                OnPositionChanged();
            }
        }
        public override UBufferConponent GetResultBuffer(int index)
        {
            if (index < 0 || index >= Inputs.Count)
                return null;
            var graph = ParentGraph as UPgcGraph;
            return graph.BufferCache.FindBuffer(Inputs[index]);
        }
        public override bool OnProcedure(UPgcGraph graph)
        {
            //for (int i = 0; i < Inputs.Count; i++)
            //{
            //    var buffer = GetResultBuffer(i);
            //    if (buffer != null)
            //    {
            //        buffer.LifeCount--;
            //    }
            //}
            return true;
        }
    }
}

namespace EngineNS.UTest
{
    [UTest.UTest]
    public class UTest_Procedure
    {
        public void UnitTestEntrance()
        {

        }
    }
}