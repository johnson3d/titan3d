using System;
using System.Collections.Generic;
using System.ComponentModel;
using EngineNS.Bricks.NodeGraph;

namespace EngineNS.Bricks.Procedure.Node
{
    [Bricks.CodeBuilder.ContextMenu("FloatValue", "Float1\\FloatValue", UPgcGraph.PgcEditorKeyword)]
    public class UFloat1ValueNode : UPgcNodeBase
    {
        [Browsable(false)]
        public PinOut ResultPin { get; set; } = new PinOut();
        public UBufferCreator OutputDesc { get; } = UBufferCreator.CreateInstance<USuperBuffer<float, FFloatOperator>>(-1, -1, -1);
        public UFloat1ValueNode()
        {
            Icon.Size = new Vector2(25, 25);
            Icon.Color = 0xFF00FF00;
            TitleColor = 0xFF204020;
            BackColor = 0x80808080;

            AddOutput(ResultPin, "Result", OutputDesc);
        }
        [Rtti.Meta]
        public float Value { get; set; } = 1.0f;
        public unsafe override bool OnProcedure(UPgcGraph graph)
        {
            var result = graph.BufferCache.FindBuffer(ResultPin);
            
            for (int i = 0; i < result.Depth; i++)
            {
                for (int j = 0; j < result.Height; j++)
                {
                    for (int k = 0; k < result.Width; k++)
                    {
                        result.SetFloat1(k, j, i, Value);
                    }
                }
            }
            return true;
        }
        public override UBufferCreator GetOutBufferCreator(PinOut pin)
        {
            return OutputDesc;
        }
    }
    [Bricks.CodeBuilder.ContextMenu("CurveValue", "Float1\\CurveValue", UPgcGraph.PgcEditorKeyword)]
    public class UFloat1CurveNode : UPgcNodeBase
    {
        [Browsable(false)]
        public PinIn ValuePin { get; set; } = new PinIn();
        [Browsable(false)]
        public PinIn BezierPin { get; set; } = new PinIn();
        [Browsable(false)]
        public PinOut ResultPin { get; set; } = new PinOut();
        public UBufferCreator Float1Desc { get; } = UBufferCreator.CreateInstance<USuperBuffer<float, FFloatOperator>>(-1, -1, -1);
        public UBufferCreator OutputDesc { get; } = UBufferCreator.CreateInstance<USuperBuffer<float, FFloatOperator>>(-1, -1, -1);
        public UFloat1CurveNode()
        {
            Icon.Size = new Vector2(25, 25);
            Icon.Color = 0xFF00FF00;
            TitleColor = 0xFF204020;
            BackColor = 0x80808080;

            AddInput(ValuePin, "Value", Float1Desc);
            AddInput(BezierPin, "Bezier", null, "Bezier");
            AddOutput(ResultPin, "Result", OutputDesc);
        }
        public override UBufferCreator GetOutBufferCreator(PinOut pin)
        {
            if (ResultPin == pin)
            {
                var graph = ParentGraph as UPgcGraph;
                var buffer = graph.BufferCache.FindBuffer(ValuePin);
                if (buffer != null)
                {
                    OutputDesc.SetSize(buffer.BufferCreator);
                    return OutputDesc;
                }
            }
            return null;
        }
        public unsafe override bool OnProcedure(UPgcGraph graph)
        {
            var heightComp = graph.BufferCache.FindBuffer(ValuePin);
            var result = graph.BufferCache.FindBuffer(ResultPin);
            var bzNode = GetInputNode(graph, BezierPin) as UBezier;

            result.DispatchPixels((target, x, y, z) =>
            {
                var height = heightComp.GetPixel<float>(x, y, z);
                var vzValue = BezierCalculate.ValueOnBezier(bzNode.BzPoints, height);
                target.SetFloat1(x, y, z, vzValue.Y);
            }, true);

            heightComp.LifeCount--;
            return true;
        }
    }
    [Bricks.CodeBuilder.ContextMenu("Perlin", "Float1\\Perlin", UPgcGraph.PgcEditorKeyword)]
    public partial class UNoisePerlin : UPgcNodeBase
    {
        [Browsable(false)]
        public PinOut ResultPin { get; set; } = new PinOut();
        public UBufferCreator Float1Desc { get; set; } = UBufferCreator.CreateInstance<USuperBuffer<float, FFloatOperator>>(-1, -1, -1);
        public UNoisePerlin()
        {
            Icon.Size = new Vector2(25, 25);
            Icon.Color = 0xFF00FF00;
            TitleColor = 0xFF204020;
            BackColor = 0x80808080;

            AddOutput(ResultPin, "Result", Float1Desc);
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
        int mSeed = (int)Support.TtTime.GetTickCount();
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
        [Rtti.Meta]
        public float Border { get; set; } = 3.0f;
        [Rtti.Meta]
        public Support.TtPerlin2.EFbmMode FbmMode { get; set; } = Support.TtPerlin2.EFbmMode.Classic;
        [Rtti.Meta]
        public float Lacumarity { get; set; } = 2.0f;
        [Rtti.Meta]
        public float Gain { get; set; } = 0.5f;
        [Rtti.Meta]
        public float Rotator { get; set; } = 0.0f;
        //protected Support.CPerlin mPerlin;
        protected Support.TtPerlin2 mPerlin;
        protected void UpdatePerlin()
        {
            //mPerlin = new Support.CPerlin(Octaves, Freq, Amptitude, Seed, SamplerSize);
            mPerlin = new Support.TtPerlin2(Seed, SamplerSize);
        }
        public DVector3 StartPosition = DVector3.Zero;
        public override UBufferCreator GetOutBufferCreator(PinOut pin)
        {
            return Float1Desc;
        }
        public override bool InitProcedure(UPgcGraph graph)
        {
            UpdatePerlin();
            return true;
        }
        public unsafe override bool OnProcedure(UPgcGraph graph)
        {
            if (TryLoadOutBufferFromCache(graph, ResultPin))
                return true;

            var resultComp = graph.BufferCache.FindBuffer(ResultPin);
            float XScale = 1.0f * GridSize / (resultComp.Width - 1);
            float YScale = 1.0f * GridSize / (resultComp.Height - 1);
            //for (int i = 0; i < resultComp.Height; i++)
            //{
            //    for (int j = 0; j < resultComp.Width; j++)
            //    {
            //        var value = (float)mPerlin.mCoreObject.Get(StartPosition.X + GridSize * j + XScale * j, StartPosition.Z + GridSize * i + YScale * i);
            //        resultComp.SetSuperPixelAddress(j, i, 0, &value);
            //    }
            //}
            resultComp.DispatchPixels((result, x, y, z) =>
            {
                //var value = (float)mPerlin.mCoreObject.Get(StartPosition.X + GridSize * x + XScale * x, StartPosition.Z + GridSize * y + YScale * y);
                var coord = new DVector2(StartPosition.X + GridSize * x + XScale * x, StartPosition.Z + GridSize * y + YScale * y);
                var value = (float)mPerlin.GetPerlinValue(FbmMode, coord, Octaves, Freq, Amptitude, Lacumarity, Gain, Rotator);
                resultComp.SetSuperPixelAddress(x, y, 0, &value);
            }, true);

            this.SaveOutBufferToCache(graph, ResultPin);
            return true;
        }

        public override Hash160 GetOutBufferHash(PinOut pin)
        {
            if (pin == ResultPin)
            {
                string hashStr = $"{StartPosition.ToString()}_{Octaves}_{Freq}_{Amptitude}_{Seed}_{SamplerSize}_{GridSize}_{Border}";
                return Hash160.CreateHash160(hashStr);
            }
            return Hash160.Emtpy;
        }
        public override void OnAfterProcedure(UPgcGraph graph)
        {

        }
    }
    [Bricks.CodeBuilder.ContextMenu("Normalize", "Float1\\Normalize", UPgcGraph.PgcEditorKeyword)]
    public class UFloat1Normalize : UMonocular
    {
        public UFloat1Normalize()
        {
            
        }
        public unsafe override bool OnProcedure(UPgcGraph graph)
        {
            var src = graph.BufferCache.FindBuffer(SrcPin);
            var result = graph.BufferCache.FindBuffer(ResultPin);
            float min, max;
            src.GetRangeUnsafe<float, FFloatOperator>(out min, out max);
            var range = max - min;
            for (int i = 0; i < result.Depth; i++)
            {
                for (int j = 0; j < result.Height; j++)
                {
                    for (int k = 0; k < result.Width; k++)
                    {
                        var Value = src.GetFloat1(k, j, i);
                        Value = (Value - min) / range;
                        result.SetFloat1(k, j, i, Value);
                    }
                }
            }
            return true;
        }
        public override UBufferCreator GetOutBufferCreator(PinOut pin)
        {
            return ResultDesc;
        }
    }
    [Bricks.CodeBuilder.ContextMenu("Clamp", "Float1\\Clamp", UPgcGraph.PgcEditorKeyword)]
    public class UFloat1Clamp : UMonocular
    {
        public UFloat1Clamp()
        {

        }
        [Rtti.Meta]
        public float MinValue { get; set; } = 0.0f;
        [Rtti.Meta]
        public float MaxValue { get; set; } = 1.0f;
        public unsafe override bool OnProcedure(UPgcGraph graph)
        {
            var src = graph.BufferCache.FindBuffer(SrcPin);
            var result = graph.BufferCache.FindBuffer(ResultPin);            
            for (int i = 0; i < result.Depth; i++)
            {
                for (int j = 0; j < result.Height; j++)
                {
                    for (int k = 0; k < result.Width; k++)
                    {
                        var Value = src.GetFloat1(k, j, i);
                        Value = MathHelper.Clamp(Value, MinValue, MaxValue);
                        result.SetFloat1(k, j, i, Value);
                    }
                }
            }
            return true;
        }
        public override UBufferCreator GetOutBufferCreator(PinOut pin)
        {
            return ResultDesc;
        }
    }
    [Bricks.CodeBuilder.ContextMenu("Smooth", "Float1\\Smooth", UPgcGraph.PgcEditorKeyword)]
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
            var curComp = graph.BufferCache.FindBuffer(SrcPin);
            var resultComp = graph.BufferCache.FindBuffer(ResultPin);

            resultComp.DispatchPixels((result, x, y, z) =>
            {
                unsafe
                {
                    var pixels = stackalloc float[9];
                    var center = curComp.GetPixel<float>(x, y);
                    pixels[1 * 3 + 1] = center;

                    {//line1
                        if (curComp.IsValidPixel(x - 1, y - 1))
                            pixels[0 * 3 + 0] = curComp.GetPixel<float>(x - 1, y - 1);
                        else
                            pixels[0 * 3 + 0] = center;

                        if (curComp.IsValidPixel(x, y - 1))
                            pixels[0 * 3 + 1] = curComp.GetPixel<float>(x, y - 1);
                        else
                            pixels[0 * 3 + 1] = center;

                        if (curComp.IsValidPixel(x + 1, y - 1))
                            pixels[0 * 3 + 2] = curComp.GetPixel<float>(x + 1, y - 1);
                        else
                            pixels[0 * 3 + 2] = center;
                    }

                    {//line2
                        if (curComp.IsValidPixel(x - 1, y))
                            pixels[1 * 3 + 0] = curComp.GetPixel<float>(x - 1, y);
                        else
                            pixels[1 * 3 + 0] = center;

                        if (curComp.IsValidPixel(x + 1, y))
                            pixels[1 * 3 + 2] = curComp.GetPixel<float>(x + 1, y);
                        else
                            pixels[1 * 3 + 2] = center;
                    }

                    {//line3
                        if (curComp.IsValidPixel(x - 1, y + 1))
                            pixels[2 * 3 + 0] = curComp.GetPixel<float>(x - 1, y + 1);
                        else
                            pixels[2 * 3 + 0] = center;

                        if (curComp.IsValidPixel(x, y + 1))
                            pixels[2 * 3 + 1] = curComp.GetPixel<float>(x, y + 1);
                        else
                            pixels[2 * 3 + 1] = center;

                        if (curComp.IsValidPixel(x + 1, y + 1))
                            pixels[2 * 3 + 2] = curComp.GetPixel<float>(x + 1, y + 1);
                        else
                            pixels[2 * 3 + 2] = center;
                    }

                    float value = pixels[0 * 3 + 0] * BlurMatrix[0, 0] + pixels[0 * 3 + 1] * BlurMatrix[0, 1] + pixels[0 * 3 + 2] * BlurMatrix[0, 2]
                        + pixels[1 * 3 + 0] * BlurMatrix[1, 0] + pixels[1 * 3 + 1] * BlurMatrix[1, 1] + pixels[1 * 3 + 2] * BlurMatrix[1, 2]
                        + pixels[2 * 3 + 0] * BlurMatrix[2, 0] + pixels[2 * 3 + 1] * BlurMatrix[2, 1] + pixels[2 * 3 + 2] * BlurMatrix[2, 2];
                    if (ClampBorder)
                    {
                        if (y == 0 || x == 0 || y == curComp.Height - 1 || x == curComp.Width - 1)
                        {
                            result.SetPixel(x, y, center);
                        }
                        else
                        {
                            result.SetPixel(x, y, value);
                        }
                    }
                    else
                    {
                        result.SetPixel(x, y, value);
                    }
                }
            }, true);

            //float[,] pixels = new float[3, 3];
            //for (int i = 0; i < curComp.Height; i++)
            //{
            //    for (int j = 0; j < curComp.Width; j++)
            //    {
            //        var center = curComp.GetPixel<float>(j, i);
            //        pixels[1, 1] = center;

            //        {//line1
            //            if (curComp.IsValidPixel(j - 1, i - 1))
            //                pixels[0, 0] = curComp.GetPixel<float>(j - 1, i - 1);
            //            else
            //                pixels[0, 0] = center;

            //            if (curComp.IsValidPixel(j, i - 1))
            //                pixels[0, 1] = curComp.GetPixel<float>(j, i - 1);
            //            else
            //                pixels[0, 1] = center;

            //            if (curComp.IsValidPixel(j + 1, i - 1))
            //                pixels[0, 2] = curComp.GetPixel<float>(j + 1, i - 1);
            //            else
            //                pixels[0, 2] = center;
            //        }

            //        {//line2
            //            if (curComp.IsValidPixel(j - 1, i))
            //                pixels[1, 0] = curComp.GetPixel<float>(j - 1, i);
            //            else
            //                pixels[1, 0] = center;

            //            if (curComp.IsValidPixel(j + 1, i))
            //                pixels[1, 2] = curComp.GetPixel<float>(j + 1, i);
            //            else
            //                pixels[1, 2] = center;
            //        }

            //        {//line3
            //            if (curComp.IsValidPixel(j - 1, i + 1))
            //                pixels[2, 0] = curComp.GetPixel<float>(j - 1, i + 1);
            //            else
            //                pixels[2, 0] = center;

            //            if (curComp.IsValidPixel(j, i + 1))
            //                pixels[2, 1] = curComp.GetPixel<float>(j, i + 1);
            //            else
            //                pixels[2, 1] = center;

            //            if (curComp.IsValidPixel(j + 1, i + 1))
            //                pixels[2, 2] = curComp.GetPixel<float>(j + 1, i + 1);
            //            else
            //                pixels[2, 2] = center;
            //        }

            //        float value = pixels[0, 0] * BlurMatrix[0, 0] + pixels[0, 1] * BlurMatrix[0, 1] + pixels[0, 2] * BlurMatrix[0, 2]
            //            + pixels[1, 0] * BlurMatrix[1, 0] + pixels[1, 1] * BlurMatrix[1, 1] + pixels[1, 2] * BlurMatrix[1, 2]
            //            + pixels[2, 0] * BlurMatrix[2, 0] + pixels[2, 1] * BlurMatrix[2, 1] + pixels[2, 2] * BlurMatrix[2, 2];
            //        if (ClampBorder)
            //        {
            //            if (i == 0 || j == 0 || i == curComp.Height - 1 || j == curComp.Width - 1)
            //            {
            //                resultComp.SetPixel(j, i, center);
            //            }
            //            else
            //            {
            //                resultComp.SetPixel(j, i, value);
            //            }
            //        }
            //        else
            //        {
            //            resultComp.SetPixel(j, i, value);
            //        }
            //    }
            //}
            curComp.LifeCount--;
            return true;
        }
    }
    [Bricks.CodeBuilder.ContextMenu("Sobel", "Float1\\Sobel", UPgcGraph.PgcEditorKeyword)]
    public class USobel : UMonocular
    {
        [Browsable(false)]
        public PinOut DDXPin { get; set; } = new PinOut();
        [Browsable(false)]
        public PinOut DDYPin { get; set; } = new PinOut();
        [Browsable(false)]
        public PinOut AnglePin { get; set; } = new PinOut();
        public USobel()
        {
            AddOutput(DDXPin, "Dx", ResultDesc);
            AddOutput(DDYPin, "Dy", ResultDesc);
            AddOutput(AnglePin, "Angle", ResultDesc);
        }
        public override UBufferCreator GetOutBufferCreator(PinOut pin)
        {
            if (ResultPin == pin || DDXPin == pin || DDYPin == pin || AnglePin == pin)
            {
                var graph = ParentGraph as UPgcGraph;
                var buffer = graph.BufferCache.FindBuffer(SrcPin);
                if (buffer != null)
                {
                    return buffer.BufferCreator;
                }
            }
            return null;
        }
        //https://zhuanlan.zhihu.com/p/113397988
        static float[,] Gx =
        {
            { -1, 0, 1 },
            { -2, 0, 2 },
            { -1, 0, 1 }
        };
        static float[,] Gy =
        {
            { -1, -2, -1 },
            { 0, 0, 0 },
            { 1, 2, 1 }
        };
        public override bool OnProcedure(UPgcGraph graph)
        {
            var curComp = graph.BufferCache.FindBuffer(SrcPin);
            var resultComp = graph.BufferCache.FindBuffer(ResultPin);
            var dxComp = graph.BufferCache.FindBuffer(DDXPin);
            var dyComp = graph.BufferCache.FindBuffer(DDYPin);
            var angleComp = graph.BufferCache.FindBuffer(AnglePin);

            var t = Math.Atan2(0, 0);

            resultComp.DispatchPixels((result, x, y, z) =>
            {
                unsafe
                {
                    var pixels = stackalloc float[9];
                    var center = curComp.GetPixel<float>(x, y);
                    pixels[1 * 3 + 1] = center;

                    {//line1
                        if (curComp.IsValidPixel(x - 1, y - 1))
                            pixels[0 * 3 + 0] = curComp.GetPixel<float>(x - 1, y - 1);
                        else
                            pixels[0 * 3 + 0] = center;

                        if (curComp.IsValidPixel(x, y - 1))
                            pixels[0 * 3 + 1] = curComp.GetPixel<float>(x, y - 1);
                        else
                            pixels[0 * 3 + 1] = center;

                        if (curComp.IsValidPixel(x + 1, y - 1))
                            pixels[0 * 3 + 2] = curComp.GetPixel<float>(x + 1, y - 1);
                        else
                            pixels[0 * 3 + 2] = center;
                    }

                    {//line2
                        if (curComp.IsValidPixel(x - 1, y))
                            pixels[1 * 3 + 0] = curComp.GetPixel<float>(x - 1, y);
                        else
                            pixels[1 * 3 + 0] = center;

                        if (curComp.IsValidPixel(x + 1, y))
                            pixels[1 * 3 + 2] = curComp.GetPixel<float>(x + 1, y);
                        else
                            pixels[1 * 3 + 2] = center;
                    }

                    {//line3
                        if (curComp.IsValidPixel(x - 1, y + 1))
                            pixels[2 * 3 + 0] = curComp.GetPixel<float>(x - 1, y + 1);
                        else
                            pixels[2 * 3 + 0] = center;

                        if (curComp.IsValidPixel(x, y + 1))
                            pixels[2 * 3 + 1] = curComp.GetPixel<float>(x, y + 1);
                        else
                            pixels[2 * 3 + 1] = center;

                        if (curComp.IsValidPixel(x + 1, y + 1))
                            pixels[2 * 3 + 2] = curComp.GetPixel<float>(x + 1, y + 1);
                        else
                            pixels[2 * 3 + 2] = center;
                    }

                    float ddx = pixels[0 * 3 + 0] * Gx[0, 0] + pixels[0 * 3 + 1] * Gx[0, 1] + pixels[0 * 3 + 2] * Gx[0, 2]
                        + pixels[1 * 3 + 0] * Gx[1, 0] + pixels[1 * 3 + 1] * Gx[1, 1] + pixels[1 * 3 + 2] * Gx[1, 2]
                        + pixels[2 * 3 + 0] * Gx[2, 0] + pixels[2 * 3 + 1] * Gx[2, 1] + pixels[2 * 3 + 2] * Gx[2, 2];

                    float ddy = pixels[0 * 3 + 0] * Gy[0, 0] + pixels[0 * 3 + 1] * Gy[0, 1] + pixels[0 * 3 + 2] * Gy[0, 2]
                        + pixels[1 * 3 + 0] * Gy[1, 0] + pixels[1 * 3 + 1] * Gy[1, 1] + pixels[1 * 3 + 2] * Gy[1, 2]
                        + pixels[2 * 3 + 0] * Gy[2, 0] + pixels[2 * 3 + 1] * Gy[2, 1] + pixels[2 * 3 + 2] * Gy[2, 2];

                    float value = MathHelper.Sqrt(ddx * ddx + ddy * ddy);
                    result.SetPixel(x, y, value);
                    dxComp.SetPixel(x, y, ddx);
                    dyComp.SetPixel(x, y, ddy);
                    float theta = (float)Math.Atan2(ddy, ddx);
                    angleComp.SetPixel(x, y, theta);
                }
            }, true);

            curComp.LifeCount--;
            return true;
        }
    }
    [Bricks.CodeBuilder.ContextMenu("Stretch", "Float1\\Stretch", UPgcGraph.PgcEditorKeyword)]
    public class UStretchFloat1 : UPgcNodeBase
    {
        [Browsable(false)]
        public PinIn TargetPin { get; set; } = new PinIn();
        [Browsable(false)]
        public PinIn SourcePin { get; set; } = new PinIn();
        [Browsable(false)]
        public PinOut ResultPin { get; set; } = new PinOut();
        public UBufferCreator Float1Desc { get; } = UBufferCreator.CreateInstance<USuperBuffer<float, FFloatOperator>>(-1, -1, -1);
        public UStretchFloat1()
        {
            Icon.Size = new Vector2(25, 25);
            Icon.Color = 0xFF00FF00;
            TitleColor = 0xFF204020;
            BackColor = 0x80808080;

            AddInput(TargetPin, "Target", Float1Desc);
            AddInput(SourcePin, "Source", Float1Desc);
            AddOutput(ResultPin, "Result", Float1Desc);
        }
        [Rtti.Meta]
        public uint SrcX { get; set; } = 0;
        [Rtti.Meta]
        public uint SrcY { get; set; } = 0;
        [Rtti.Meta]
        public uint SrcW { get; set; } = 1;
        [Rtti.Meta]
        public uint SrcH { get; set; } = 1;

        [Rtti.Meta]
        public uint DstX { get; set; } = 0;
        [Rtti.Meta]
        public uint DstY { get; set; } = 0;
        [Rtti.Meta]
        public uint DstW { get; set; } = 1;
        [Rtti.Meta]
        public uint DstH { get; set; } = 1;
        [Rtti.Meta]
        public UBufferComponent.EBufferSamplerType SamplerType { get; set; } = UBufferComponent.EBufferSamplerType.Linear;
        public override UBufferCreator GetOutBufferCreator(PinOut pin)
        {
            if (ResultPin == pin)
            {
                var graph = ParentGraph as UPgcGraph;
                var buffer = graph.BufferCache.FindBuffer(TargetPin);
                if (buffer != null)
                {
                    return buffer.BufferCreator;
                }
            }
            return null;
        }
        public unsafe override bool OnProcedure(UPgcGraph graph)
        {
            var left = graph.BufferCache.FindBuffer(TargetPin);
            var right = graph.BufferCache.FindBuffer(SourcePin);
            var resultComp = graph.BufferCache.FindBuffer(ResultPin);
            resultComp.DispatchPixels((result, x, y, z) =>
            {
                if (x < DstX || y < DstY ||
                        x >= DstX + DstW || y >= DstY + DstH)
                {
                    //result.SetFloat1(x, y, z, left.Sampler2DFloat1(in uvw));
                    result.SetFloat1(x, y, z, left.GetFloat1(x, y, z));
                }
                else
                {
                    var u = (float)(x - DstX) / DstW;
                    var v = (float)(y - DstY) / DstH;

                    var ox = (u * SrcW + SrcX) / right.Width;
                    var oy = (v * SrcH + SrcY) / right.Height;

                    result.SetFloat1(x, y, z, right.Sampler2DFloat1(ox, oy, 0, SamplerType));
                }
            }, true);
            
            left.LifeCount--;
            right.LifeCount--;
            return true;
        }
    }
    [Bricks.CodeBuilder.ContextMenu("UVWBezier", "Float1\\UVWBezier", UPgcGraph.PgcEditorKeyword)]
    public class UUVWBezierNode : UPgcNodeBase
    {
        [Browsable(false)]
        public PinIn BezierPin { get; set; } = new PinIn();
        [Browsable(false)]
        public PinOut ResultPin { get; set; } = new PinOut();
        public UBufferCreator OutputDesc { get; } = UBufferCreator.CreateInstance<USuperBuffer<float, FFloatOperator>>(-1, -1, -1);
        public UUVWBezierNode()
        {
            Icon.Size = new Vector2(25, 25);
            Icon.Color = 0xFF00FF00;
            TitleColor = 0xFF204020;
            BackColor = 0x80808080;

            AddInput(BezierPin, "Bezier", null, "Bezier");
            AddOutput(ResultPin, "Result", OutputDesc);
        }
        public enum ECoord
        {
            UCoord,
            VCoord,
            WCoord,
        }
        [Rtti.Meta]
        public ECoord Coord { get; set; } = ECoord.UCoord;
        public unsafe override bool OnProcedure(UPgcGraph graph)
        {
            var bzNode = GetInputNode(graph, BezierPin) as UBezier;
            var result = graph.BufferCache.FindBuffer(ResultPin);

            for (int i = 0; i < result.Depth; i++)
            {
                for (int j = 0; j < result.Height; j++)
                {
                    for (int k = 0; k < result.Width; k++)
                    {
                        var uvw = result.GetUVW(k, j, i);

                        float xv = 0;
                        switch (Coord)
                        {
                            case ECoord.UCoord:
                                xv = uvw.X;
                                break;
                            case ECoord.VCoord:
                                xv = uvw.Y;
                                break;
                            case ECoord.WCoord:
                                xv = uvw.Z;
                                break;
                        }

                        var vzValue = BezierCalculate.ValueOnBezier(bzNode.BzPoints, xv);
                        result.SetFloat1(k, j, i, vzValue.Y);
                    }
                }
            }
            return true;
        }
        public override UBufferCreator GetOutBufferCreator(PinOut pin)
        {
            return OutputDesc;
        }
    }
}
