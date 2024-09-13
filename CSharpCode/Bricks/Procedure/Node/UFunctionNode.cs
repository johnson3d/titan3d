using System;
using System.Collections.Generic;
using System.ComponentModel;
using EngineNS.Bricks.NodeGraph;

namespace EngineNS.Bricks.Procedure.Node
{
    [Bricks.CodeBuilder.ContextMenu("Bezier", "Function\\Bezier", UPgcGraph.PgcEditorKeyword)]
    public partial class UBezier : UPgcNodeBase
    {
        [Browsable(false)]
        public PinOut ResultPin { get; set; } = new PinOut();
        //public UBufferCreator OutputDesc { get; } = UBufferCreator.CreateInstance<USuperBuffer<float, FFloatOperator>>(-1, -1, -1);
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
        public float RangeX
        {
            get => MaxX - MinX;
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
        public float RangeY
        {
            get => MaxY - MinY;
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

            Icon.Size = new Vector2(25, 25);
            Icon.Color = 0xFF00FF00;
            TitleColor = 0xFF204020;
            BackColor = 0x80808080;

            AddOutput(ResultPin, "Result", null, "Bezier");

            mBezierCtrl.Initialize(MinX, MinY, MaxX, MaxY);
        }
        public override UBufferCreator GetOutBufferCreator(PinOut pin)
        {
            return null;
        }
        public override Hash160 GetOutBufferHash(PinOut pin)
        {
            if (pin == ResultPin)
            {
                string hashStr = "";
                hashStr += MinX;
                hashStr += MaxX;
                hashStr += MinY;
                hashStr += MaxX;
                foreach (var i in BzPoints)
                {
                    hashStr += i.ToString();
                }
                return Hash160.CreateHash160(hashStr);
            }
            return Hash160.Emtpy;
        }
        public unsafe override void OnPreviewDraw(in Vector2 prevStart, in Vector2 prevEnd, ImDrawList cmdlist)
        {
            //var ctrlPos = ParentGraph.CanvasToViewport(in prevStart);
            //ctrlPos -= ImGuiAPI.GetWindowPos();
            //ImGuiAPI.SetCursorPos(in ctrlPos);

            //ImGuiAPI.InvisibleButton("canvas", prevEnd - prevStart, ImGuiButtonFlags_.ImGuiButtonFlags_MouseButtonLeft | ImGuiButtonFlags_.ImGuiButtonFlags_MouseButtonRight);
            mBezierCtrl.OnDrawCanvas(in prevStart, prevEnd - prevStart);
        }
        [Rtti.Meta]
        public float GetY(float x)
        {
            var vzValue = BezierCalculate.ValueOnBezier(BzPoints, x);
            return vzValue.Y;
        }
    }
    /*
     0 1 2
     3 c 4
     5 6 7
     */
    [Bricks.CodeBuilder.ContextMenu("CalcNormal", "Function\\CalcNormal", UPgcGraph.PgcEditorKeyword)]
    public partial class UCalcNormal : UPgcNodeBase
    {
        [Browsable(false)]
        public PinIn HFieldPin { get; set; } = new PinIn(); 
        [Browsable(false)]
        public PinOut XPin { get; set; } = new PinOut();
        [Browsable(false)]
        public PinOut YPin { get; set; } = new PinOut();
        [Browsable(false)]
        public PinOut ZPin { get; set; } = new PinOut();
        public UBufferCreator Float1Desc { get; } = UBufferCreator.CreateInstance<USuperBuffer<float, FFloatOperator>>(-1, -1, -1);
        public UBufferCreator OutputFloat1Desc { get; } = UBufferCreator.CreateInstance<USuperBuffer<float, FFloatOperator>>(-1, -1, -1);
        public UCalcNormal()
        {
            Icon.Size = new Vector2(25, 25);
            Icon.Color = 0xFF00FF00;
            TitleColor = 0xFF204020;
            BackColor = 0x80808080;

            AddInput(HFieldPin, "Height", Float1Desc);
            AddOutput(XPin, "X", OutputFloat1Desc);
            AddOutput(YPin, "Y", OutputFloat1Desc);
            AddOutput(ZPin, "Z", OutputFloat1Desc);
        }
        public override UBufferCreator GetOutBufferCreator(PinOut pin)
        {
            if (XPin == pin || YPin == pin || ZPin == pin)
            {
                var graph = ParentGraph as UPgcGraph;
                var buffer = graph.BufferCache.FindBuffer(HFieldPin);
                if (buffer != null)
                {
                    OutputFloat1Desc.SetSize(buffer.BufferCreator);
                    return OutputFloat1Desc;
                }
            }
            return null;
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
            heightFiels.GetRangeUnsafe<float, FFloatOperator>(out minHeight, out maxHeight);
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

                    float altInfo = heightFiels.GetPixel<float>(j, i);
                    float v_du = heightFiels.GetPixel<float>(j + 1, i);
                    float v_dv = heightFiels.GetPixel<float>(j, i + 1);

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
    [Bricks.CodeBuilder.ContextMenu("Normalize3D", "Function\\Normalize3D", UPgcGraph.PgcEditorKeyword)]
    public partial class UNormalize3D : UPgcNodeBase
    {
        [Browsable(false)]
        public PinIn InXPin { get; set; } = new PinIn();
        [Browsable(false)]
        public PinIn InYPin { get; set; } = new PinIn();
        [Browsable(false)]
        public PinIn InZPin { get; set; } = new PinIn();
        [Browsable(false)]
        public PinOut XPin { get; set; } = new PinOut();
        [Browsable(false)]
        public PinOut YPin { get; set; } = new PinOut();
        [Browsable(false)]
        public PinOut ZPin { get; set; } = new PinOut();
        public UBufferCreator Float1Desc { get; } = UBufferCreator.CreateInstance<USuperBuffer<float, FFloatOperator>>(-1, -1, -1);
        public UBufferCreator OutputFloat1Desc { get; } = UBufferCreator.CreateInstance<USuperBuffer<float, FFloatOperator>>(-1, -1, -1);
        public UNormalize3D()
        {
            Icon.Size = new Vector2(25, 25);
            Icon.Color = 0xFF00FF00;
            TitleColor = 0xFF204020;
            BackColor = 0x80808080;

            AddInput(InXPin, "InX", Float1Desc);
            AddInput(InYPin, "InY", Float1Desc);
            AddInput(InZPin, "InZ", Float1Desc);
            AddOutput(XPin, "X", OutputFloat1Desc);
            AddOutput(YPin, "Y", OutputFloat1Desc);
            AddOutput(ZPin, "Z", OutputFloat1Desc);
        }
        public override UBufferCreator GetOutBufferCreator(PinOut pin)
        {
            if (XPin == pin || YPin == pin || ZPin == pin)
            {
                var graph = ParentGraph as UPgcGraph;
                var buffer = graph.BufferCache.FindBuffer(InXPin);
                if (buffer != null)
                {
                    OutputFloat1Desc.SetSize(buffer.BufferCreator);
                    return OutputFloat1Desc;
                }
            }
            return null;
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
                    nor.X = xSrc.GetPixel<float>(j, i);
                    nor.Y = ySrc.GetPixel<float>(j, i);
                    nor.Z = zSrc.GetPixel<float>(j, i);
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
    [Bricks.CodeBuilder.ContextMenu("BezierValueMap", "Function\\BezierValueMap", UPgcGraph.PgcEditorKeyword)]
    public partial class UBezierValueMap : UPgcNodeBase
    {
        [Browsable(false)]
        public PinIn InPin { get; set; } = new PinIn();
        [Browsable(false)]
        public PinIn BezierPin { get; set; } = new PinIn();
        [Browsable(false)]
        public PinOut ResultPin { get; set; } = new PinOut();
        public UBufferCreator Float1Desc { get; } = UBufferCreator.CreateInstance<USuperBuffer<float, FFloatOperator>>(-1, -1, -1);
        public UBufferCreator OutputFloat1Desc { get; } = UBufferCreator.CreateInstance<USuperBuffer<float, FFloatOperator>>(-1, -1, -1);
        public UBezierValueMap()
        {
            Icon.Size = new Vector2(25, 25);
            Icon.Color = 0xFF00FF00;
            TitleColor = 0xFF204020;
            BackColor = 0x80808080;

            AddInput(InPin, "Height", Float1Desc);
            AddInput(BezierPin, "Bezier", null, "Bezier");
            AddOutput(ResultPin, "Result", OutputFloat1Desc);
        }
        public override UBufferCreator GetOutBufferCreator(PinOut pin)
        {
            if (ResultPin == pin)
            {
                var graph = ParentGraph as UPgcGraph;
                var buffer = graph.BufferCache.FindBuffer(InPin);
                if (buffer != null)
                {
                    OutputFloat1Desc.SetSize(buffer.BufferCreator);
                    return OutputFloat1Desc;
                }
            }
            return null;
        }
        public override bool OnProcedure(UPgcGraph graph)
        {
            var bzNode = GetInputNode(graph, BezierPin) as UBezier;
            var heightComp = graph.BufferCache.FindBuffer(InPin);
            var resultComp = graph.BufferCache.FindBuffer(ResultPin);

            var heightMapHash = heightComp.CalcPixelHash();
            var bzHash = bzNode.GetOutBufferHash(graph.FindInLinkerSingle(BezierPin).OutPin);
            var testHash = Hash160.CreateHash160(heightMapHash.ToString() + bzHash.ToString());
            if (this.TryLoadOutBufferFromCache(graph, ResultPin, in testHash))
            {
                return true;
            }

            var rangeX = (bzNode.MaxX - bzNode.MinX);
            var rangeY = (bzNode.MaxY - bzNode.MinY);

            resultComp.DispatchPixels((result, x, y, z) =>
            {
                var height = heightComp.GetPixel<float>(x, y, z);
                var vzValue = BezierCalculate.ValueOnBezier(bzNode.BzPoints, height);// - bzNode.MinX);// ((double)j) * rangeX / (double)resultComp.Width);

                result.SetPixel(x, y, z, vzValue.Y);
            }, true);

            this.SaveOutBufferToCache(graph, ResultPin, testHash);
            heightComp.LifeCount--;
            //MaterialIdManager.BuildSRV(TtEngine.Instance.GfxDevice.RenderContext.mCoreObject.GetImmCommandList());
            return true;
        }
    }

    [Bricks.CodeBuilder.ContextMenu("BoolSelect", "Control\\BoolSelect", UPgcGraph.PgcEditorKeyword)]
    public partial class USelectNode : UBinocularWithMask
    {
        public unsafe override bool OnProcedure(UPgcGraph graph)
        {
            var mask = graph.BufferCache.FindBuffer(MaskPin);
            var trueBuffer = graph.BufferCache.FindBuffer(LeftPin);
            var falseBuffer = graph.BufferCache.FindBuffer(RightPin);
            var resultComp = graph.BufferCache.FindBuffer(ResultPin);

            resultComp.DispatchPixels((result, x, y, z) =>
            {
                var uvw = result.GetUVW(x, y, z);
                var s = mask.GetPixel<sbyte>(uvw);
                if (s == 0)
                {
                    result.PixelOperator.Copy(null, result.GetSuperPixelAddress(x, y, z), null, trueBuffer.GetSuperPixelAddress(in uvw));
                }
                else
                {
                    result.PixelOperator.Copy(null, result.GetSuperPixelAddress(x, y, z), null, falseBuffer.GetSuperPixelAddress(in uvw));
                }
            }, true);

            mask.LifeCount--;
            trueBuffer.LifeCount--;
            falseBuffer.LifeCount--;
            //MaterialIdManager.BuildSRV(TtEngine.Instance.GfxDevice.RenderContext.mCoreObject.GetImmCommandList());
            return true;
        }
    }

    [Bricks.CodeBuilder.ContextMenu("CalcNormal", "Function\\FastPoissonDisk", UPgcGraph.PgcEditorKeyword)]
    public partial class UFastPoissonDiskSampling2DNode : UPgcNodeBase
    {
        [Browsable(false)]
        public PinIn MaskPin { get; set; } = new PinIn();
        [Browsable(false)]
        public PinIn HeightPin { get; set; } = new PinIn();
        [Browsable(false)]
        public PinIn RadiusPin { get; set; } = new PinIn();
        [Browsable(false)]
        public PinOut ResultPin { get; set; } = new PinOut();
        [Rtti.Meta]
        public float Radius { get; set; } = 10;
        [Rtti.Meta]
        public int Seed { get; set; } = 10000;
        [Rtti.Meta]
        public int CalculateDeep { get; set; } = 30;

        public UBufferCreator FloatBuffer { get; } = UBufferCreator.CreateInstance<USuperBuffer<float, FFloatOperator>>(-1, -1, -1);
        public UBufferCreator OutputFloat3Desc { get; } = UBufferCreator.CreateInstance<USuperBuffer<Vector3, FFloat3Operator>>(-1, -1, -1);

        public UFastPoissonDiskSampling2DNode()
        {
            Icon.Size = new Vector2(25, 25);
            Icon.Color = 0xFF00FF00;
            TitleColor = 0xFF204020;
            BackColor = 0x80808080;

            AddInput(MaskPin, "Mask", UBufferCreator.CreateInstance<USuperBuffer<sbyte, FSByteOperator>>(-1, -1, -1));
            AddInput(HeightPin, "Height", FloatBuffer);
            AddInput(RadiusPin, "Radius", FloatBuffer);
            AddOutput(ResultPin, "Result", OutputFloat3Desc);
        }

        public override UBufferCreator GetOutBufferCreator(PinOut pin)
        {
            if(ResultPin == pin)
            {
                //var graph = ParentGraph as UPgcGraph;
                //var maskBuffer = graph.BufferCache.FindBuffer(MaskPin);
                //var radiusBuffer = graph.BufferCache.FindBuffer(RadiusPin);
                //var radius = Radius;
                //if (radiusBuffer != null)
                //{
                //    radius = radiusBuffer.GetPixel<float>(0, 0);
                //}
                //if (radius <= 0)
                //    radius = 10;

                //var width = maskBuffer.Width;
                //var height = maskBuffer.Height;
                //var xCount = (int)Math.Ceiling(width / radius);
                //var yCount = (int)Math.Ceiling(height / radius);

                //OutputFloat2Desc.XSize = 1;

                return OutputFloat3Desc;
            }
            return null;
        }

        public bool IsMask(int x, int y, int z, USuperBuffer<sbyte, FSByteOperator> maskBuffer)
        {
            if (maskBuffer == null)
                return true;
            var uvw = maskBuffer.GetUVW(x,y,z);
            return maskBuffer.GetPixel<sbyte>(uvw) == 1;
        }

        public override bool OnProcedure(UPgcGraph graph)
        {
            var maskBuffer = graph.BufferCache.FindBuffer(MaskPin) as USuperBuffer<sbyte, FSByteOperator>;
            var heightBuffer = graph.BufferCache.FindBuffer(HeightPin);
            var resultBuffer = graph.BufferCache.FindBuffer(ResultPin);
            resultBuffer.ResizePixels();
            var radiusBuffer = graph.BufferCache.FindBuffer(RadiusPin);
            var radius = Radius;
            if(radiusBuffer != null)
            {
                radius = radiusBuffer.GetPixel<float>(0,0,0);
            }
            if (radius <= 0)
                radius = 10;

            UInt64 width, height;
            if(maskBuffer == null)
            {
                width = (UInt64)graph.DefaultCreator.XSize;
                height = (UInt64)graph.DefaultCreator.YSize;
            }
            else
            {
                width = (UInt64)maskBuffer.Width;
                height = (UInt64)maskBuffer.Height;
            }

            var points = EngineNS.Algorithm.FastPoissonDisk.Calculate2D(width, height, radius, Seed, CalculateDeep);

            unsafe
            {
                float scaleX = 1;
                float scaleY = 1;
                if(heightBuffer != null)
                {
                    scaleX = (float)heightBuffer.Width / width;
                    scaleY = (float)heightBuffer.Height / height;
                }
                for (int i = 0; i < points.Count; i++)
                {
                    var pt = points[i];
                    if (IsMask((int)pt.X, (int)pt.Y, 0, maskBuffer))
                    {
                        float zVal = 0;
                        if(heightBuffer != null)
                            zVal = heightBuffer.GetPixel<float>((int)(pt.X * scaleX), (int)(pt.Y * scaleY));

                        Vector3 pixel = new Vector3(pt.X, pt.Y, zVal);
                        resultBuffer.AddPixel(pixel);
                    }
                }
            }

            return true;
        }

        protected override void PreviewSRVProcedure(UPgcGraph graph)
        {
            if(graph.GraphEditor != null)
            {
                if(mPreviewResultIndex >= 0)
                {
                    var maskBuffer = graph.BufferCache.FindBuffer(MaskPin) as USuperBuffer<sbyte, FSByteOperator>;
                    if (maskBuffer == null)
                        return;

                    var previewBuffer = GetResultBuffer(mPreviewResultIndex);
                    if (previewBuffer == null)
                        return;

                    if (PreviewSRV != null)
                    {
                        PreviewSRV?.FreeTextureHandle();
                        PreviewSRV = null;
                    }

                    NxRHI.TtTexture texture;
                    unsafe
                    {
                        Vector3 minV = Vector3.MaxValue;
                        Vector3 maxV = Vector3.MinValue;
                        previewBuffer.GetRangeUnsafe<Vector3, FFloat3Operator>(out minV, out maxV);
                        if(minV.Z >= 0 && minV.Z <= 1 && maxV.Z >= 0 && maxV.Z <= 1)
                        {
                            minV.Z = 0;
                            maxV.Z = 1;
                        }
                        var zRange = maxV.Z - minV.Z;

                        var desc = new NxRHI.FTextureDesc();
                        desc.SetDefault();
                        if (maskBuffer == null)
                        {
                            desc.Width = (uint)graph.DefaultCreator.XSize;
                            desc.Height = (uint)graph.DefaultCreator.YSize;
                        }
                        else
                        {
                            desc.Width = (uint)maskBuffer.Width;
                            desc.Height = (uint)maskBuffer.Height;
                        }
                        desc.MipLevels = 1;
                        desc.Format = EPixelFormat.PXF_R8G8B8A8_UNORM;
                        var initData = new NxRHI.FMappedSubResource();
                        initData.SetDefault();
                        desc.InitData = &initData;

                        var count = desc.Width * desc.Height;
                        var tarPixels = new Byte4[count];
                        for (var i = 0; i < count; i++)
                        {
                            tarPixels[i].X = 0;
                            tarPixels[i].Y = 0;
                            tarPixels[i].Z = 0;
                            tarPixels[i].W = 255;
                        }
                        var ptCount = previewBuffer.Width;
                        var pSlice = (Vector3*)previewBuffer.GetSliceAddress(0);
                        for (int i = 0; i < ptCount; i++)
                        {
                            var pt = pSlice[i];
                            var idx = (int)(pt.Y) * desc.Width + (int)(pt.X);
                            if (zRange == 0)
                                tarPixels[idx].X = 255;
                            else
                                tarPixels[idx].X = (byte)((pt.Z - minV.Z) * 255.0f / zRange);
                        }
                        fixed(Byte4* p = &tarPixels[0])
                        {
                            initData.RowPitch = desc.Width * (uint)sizeof(Byte4);
                            initData.pData = p;
                            texture = TtEngine.Instance.GfxDevice.RenderContext.CreateTexture(in desc);
                        }

                        var rsvDesc = new NxRHI.FSrvDesc();
                        rsvDesc.SetTexture2D();
                        rsvDesc.Type = NxRHI.ESrvType.ST_Texture2D;
                        rsvDesc.Format = EPixelFormat.PXF_R8G8B8A8_UNORM;
                        rsvDesc.Texture2D.MipLevels = desc.MipLevels;
                        PreviewSRV = TtEngine.Instance.GfxDevice.RenderContext.CreateSRV(texture, in rsvDesc);
                    }
                }
            }
        }
    }
}
