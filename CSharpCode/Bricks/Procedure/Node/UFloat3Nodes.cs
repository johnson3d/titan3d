using System;
using System.Collections.Generic;
using EngineNS.Bricks.NodeGraph;


namespace EngineNS.Bricks.Procedure.Node
{
    [Bricks.CodeBuilder.ContextMenu("Float3Unpack", "Float3\\Unpack", UPgcGraph.PgcEditorKeyword)]
    public class UFloat3UnpackNodes : UPgcNodeBase
    {
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public PinIn InXYZ { get; set; } = new PinIn();
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public PinOut XPin { get; set; } = new PinOut();
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public PinOut YPin { get; set; } = new PinOut();
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public PinOut ZPin { get; set; } = new PinOut();

        public UBufferCreator InputFloat3Desc = UBufferCreator.CreateInstance< USuperBuffer<Vector3, FFloat3Operator>>(-1, -1, -1);
        public UBufferCreator OutputFloatDesc = UBufferCreator.CreateInstance<USuperBuffer<float, FFloatOperator>>(-1, -1, -1);
        public UFloat3UnpackNodes()
        {
            Icon.Size = new Vector2(25, 25);
            Icon.Color = 0xFF00FF00;
            TitleColor = 0xFF204020;
            BackColor = 0x80808080;

            AddInput(InXYZ, "InXYZ", InputFloat3Desc);
            AddOutput(XPin, "X", OutputFloatDesc);
            AddOutput(YPin, "Y", OutputFloatDesc);
            AddOutput(ZPin, "Z", OutputFloatDesc);
        }
        public override UBufferCreator GetOutBufferCreator(PinOut pin)
        {
            var graph = ParentGraph as UPgcGraph;
            if (XPin == pin || YPin == pin || ZPin == pin)
            {   
                var buffer = graph.BufferCache.FindBuffer(InXYZ);
                if (buffer != null)
                {
                    var result = buffer.BufferCreator.Clone();
                    result.BufferType = Rtti.UTypeDesc.TypeOf<USuperBuffer<float, FFloatOperator>>();
                    return result;
                }
            }
            return base.GetOutBufferCreator(pin);
        }
        public override unsafe bool InitProcedure(UPgcGraph graph)
        {
            return true;
        }
        public override bool OnProcedure(UPgcGraph graph)
        {
            var inXYZ = graph.BufferCache.FindBuffer(InXYZ);
            var xResult = graph.BufferCache.FindBuffer(XPin);
            var yResult = graph.BufferCache.FindBuffer(YPin);
            var zResult = graph.BufferCache.FindBuffer(ZPin);

            for (int i = 0; i < inXYZ.Depth; i++)
            {
                for (int j = 0; j < inXYZ.Height; j++)
                {
                    for (int k = 0; k < inXYZ.Width; k++)
                    {
                        var xyz = inXYZ.GetPixel<Vector3>(k, j, i);
                        xResult.SetPixel<float>(k, j, i, xyz.X);
                        yResult.SetPixel<float>(k, j, i, xyz.Y);
                        zResult.SetPixel<float>(k, j, i, xyz.Z);
                    }
                }
            }

            return true;
        }
    }

    [Bricks.CodeBuilder.ContextMenu("Float3Pack", "Float3\\Pack", UPgcGraph.PgcEditorKeyword)]
    public class UFloat3PackNodes : UPgcNodeBase
    {
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public PinIn XPin { get; set; } = new PinIn();
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public PinIn YPin { get; set; } = new PinIn();
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public PinIn ZPin { get; set; } = new PinIn();
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public PinOut OutXYZ { get; set; } = new PinOut();

        public UBufferCreator InputFloatDesc = UBufferCreator.CreateInstance< USuperBuffer <float, FFloatOperator>>(-1, -1, -1);
        public UBufferCreator OutputFloat3Desc = UBufferCreator.CreateInstance<USuperBuffer<Vector3, FFloat3Operator>>(-1, -1, -1);

        public UFloat3PackNodes()
        {
            Icon.Size = new Vector2(25, 25);
            Icon.Color = 0xFF00FF00;
            TitleColor = 0xFF204020;
            BackColor = 0x80808080;

            AddInput(XPin, "X", InputFloatDesc);
            AddInput(YPin, "Y", InputFloatDesc);
            AddInput(ZPin, "Z", InputFloatDesc);
            AddOutput(OutXYZ, "OutXYZ", OutputFloat3Desc);
        }
        public override UBufferCreator GetOutBufferCreator(PinOut pin)
        {
            var graph = ParentGraph as UPgcGraph;
            if (OutXYZ == pin )
            {
                var buffer = graph.BufferCache.FindBuffer(XPin);
                if (buffer != null)
                {
                    var result = buffer.BufferCreator.Clone();
                    result.BufferType = Rtti.UTypeDesc.TypeOf<USuperBuffer<Vector3, FFloat3Operator>>();
                    return result;
                }
            }
            return base.GetOutBufferCreator(pin);
        }
        public override bool OnProcedure(UPgcGraph graph)
        {
            var ResultXYZ = graph.BufferCache.FindBuffer(OutXYZ);
            var xComp = graph.BufferCache.FindBuffer(XPin);
            var yComp = graph.BufferCache.FindBuffer(YPin);
            var zComp = graph.BufferCache.FindBuffer(ZPin);

            for (int i = 0; i < ResultXYZ.Depth; i++)
            {
                for (int j = 0; j < ResultXYZ.Height; j++)
                {
                    for (int k = 0; k < ResultXYZ.Width; k++)
                    {
                        float x = 0;
                        if (xComp.IsValidPixel(k, j, i))
                            x = xComp.GetPixel<float>(k, j, i);
                        float y = 0;
                        if (xComp.IsValidPixel(k, j, i))
                            y = yComp.GetPixel<float>(k, j, i);
                        float z = 0;
                        if (zComp.IsValidPixel(k, j, i))
                            z = zComp.GetPixel<float>(k, j, i);

                        ResultXYZ.SetPixel<Vector3>(k, j, i, new Vector3(x, y, z));
                    }
                }
            }

            return true;
        }
    }

    /*
     0 1 2
     3 c 4
     5 6 7
     */
    [Bricks.CodeBuilder.ContextMenu("Float3HeightToNormal", "Float3\\Height2Normal", UPgcGraph.PgcEditorKeyword)]
    public class UFloat3HeightToNormal : UPgcNodeBase
    {
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public PinIn HFieldPin { get; set; } = new PinIn();
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public PinOut Normal { get; set; } = new PinOut();

        public UBufferCreator InputFloatDesc = UBufferCreator.CreateInstance<USuperBuffer<float, FFloatOperator>>(-1, -1, -1);
        public UBufferCreator OutputFloat3Desc = UBufferCreator.CreateInstance<USuperBuffer<Vector3, FFloat3Operator>>(-1, -1, -1);

        public UFloat3HeightToNormal()
        {
            Icon.Size = new Vector2(25, 25);
            Icon.Color = 0xFF00FF00;
            TitleColor = 0xFF204020;
            BackColor = 0x80808080;

            AddInput(HFieldPin, "Height", InputFloatDesc);
            AddOutput(Normal, "Normal", OutputFloat3Desc);
        }
        public override UBufferCreator GetOutBufferCreator(PinOut pin)
        {
            if (Normal == pin)
            {
                var graph = ParentGraph as UPgcGraph;
                var buffer = graph.BufferCache.FindBuffer(HFieldPin);
                if (buffer != null)
                {
                    var result = buffer.BufferCreator.Clone();
                    result.BufferType = Rtti.UTypeDesc.TypeOf<USuperBuffer<Vector3, FFloat3Operator>>();
                    return result;
                }
            }
            return base.GetOutBufferCreator(pin);
        }
        [Rtti.Meta]
        public float GridSize { get; set; } = 1.0f;
        //public float HeightRange;
        public override bool OnProcedure(UPgcGraph graph)
        {
            var heightFiels = graph.BufferCache.FindBuffer(HFieldPin);
            var xyzResult = graph.BufferCache.FindBuffer(Normal);
            float minHeight, maxHeight;
            heightFiels.GetRangeUnsafe<float, FFloatOperator>(out minHeight, out maxHeight);
            //float range = HeightRange;// maxHeight - minHeight;
            xyzResult.DispatchPixels((result, x, y, z) =>
            {
                if (x >= result.Width - 1 || y >= result.Height - 1)
                    return;
                float altInfo = heightFiels.GetPixel<float>(x, y);
                float v_du = heightFiels.GetPixel<float>(x + 1, y);
                float v_dv = heightFiels.GetPixel<float>(x, y + 1);

                var A = new Vector3(GridSize, (v_du - altInfo), 0);
                var B = new Vector3(0, (v_dv - altInfo), -GridSize);

                var n = Vector3.Cross(A, B);
                n = Vector3.Normalize(n);

                xyzResult.SetPixel(x, y, n);
            }, true);

            //for (int i = 1; i < heightFiels.Height - 1; i++)
            //{
            //    for (int j = 1; j < heightFiels.Width - 1; j++)
            //    {
            //        float altInfo = heightFiels.GetPixel<float>(j, i);
            //        float v_du = heightFiels.GetPixel<float>(j + 1, i);
            //        float v_dv = heightFiels.GetPixel<float>(j, i + 1);

            //        var A = new Vector3(GridSize, (v_du - altInfo), 0);
            //        var B = new Vector3(0, (v_dv - altInfo), -GridSize);

            //        var n = Vector3.Cross(A, B);
            //        n = Vector3.Normalize(n);

            //        xyzResult.SetPixel(j, i, n);
            //    }
            //}
            heightFiels.LifeCount--;
            return true;
        }
    }

    [Bricks.CodeBuilder.ContextMenu("Float3Normalize", "Float3\\Normalize", UPgcGraph.PgcEditorKeyword)]
    public class UFloat3Normalize : UPgcNodeBase
    {
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public PinIn InXYZ { get; set; } = new PinIn();
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public PinOut OutXYZ { get; set; } = new PinOut();
        public UBufferCreator InputFloat3Desc = UBufferCreator.CreateInstance<USuperBuffer<Vector3, FFloat3Operator>>(-1, -1, -1);
        public UBufferCreator OutputFloat3Desc = UBufferCreator.CreateInstance<USuperBuffer<Vector3, FFloat3Operator>>(-1, -1, -1);
        public UFloat3Normalize()
        {
            Icon.Size = new Vector2(25, 25);
            Icon.Color = 0xFF00FF00;
            TitleColor = 0xFF204020;
            BackColor = 0x80808080;

            AddInput(InXYZ, "InXYZ", InputFloat3Desc);
            AddOutput(OutXYZ, "XYZ", OutputFloat3Desc);
        }
        public override UBufferCreator GetOutBufferCreator(PinOut pin)
        {
            if (OutXYZ == pin )
            {
                var graph = ParentGraph as UPgcGraph;
                var buffer = graph.BufferCache.FindBuffer(InXYZ);
                if (buffer != null)
                {
                    return buffer.BufferCreator;
                }
            }
            return base.GetOutBufferCreator(pin);
        }
        public override bool OnProcedure(UPgcGraph graph)
        {
            var xyzSrc = graph.BufferCache.FindBuffer(InXYZ);
            var xyzResult = graph.BufferCache.FindBuffer(OutXYZ);
            for (int i = 0; i < xyzResult.Depth; i++)
            {
                for (int j = 0; j < xyzResult.Height; j++)
                {
                    for (int k = 0; k < xyzResult.Width; k++)
                    {
                        Vector3 nor = xyzSrc.GetPixel<Vector3>(k, j, i);
                        nor.Normalize();

                        xyzResult.SetPixel(k, j, i, nor);
                    }
                }
            }

            xyzSrc.LifeCount--;
            return true;
        }
    }

    [Bricks.CodeBuilder.ContextMenu("Float3Gaussion", "Float3\\Gaussion", UPgcGraph.PgcEditorKeyword)]
    public class UFloat3Gaussion : UMonocular
    {
        static float[,] BlurMatrix =
        {
            { 0.0947416f, 0.118318f, 0.0947416f },
            { 0.118318f, 0.147761f, 0.118318f },
            { 0.0947416f, 0.118318f, 0.0947416f }
        };
        public UFloat3Gaussion()
        {
            DefaultInputDesc.BufferType = Rtti.UTypeDesc.TypeOf<USuperBuffer<Vector3, FFloat3Operator>>();
            DefaultBufferCreator.BufferType = Rtti.UTypeDesc.TypeOf<USuperBuffer<Vector3, FFloat3Operator>>();
        }
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
                    var pixels = stackalloc Vector3[9];
                    var center = curComp.GetPixel<Vector3>(x, y);
                    pixels[1 * 3 + 1] = center;

                    {//line1
                        if (curComp.IsValidPixel(x - 1, y - 1))
                            pixels[0 * 3 + 0] = curComp.GetPixel<Vector3>(x - 1, y - 1);
                        else
                            pixels[0 * 3 + 0] = center;

                        if (curComp.IsValidPixel(x, y - 1))
                            pixels[0 * 3 + 1] = curComp.GetPixel<Vector3>(x, y - 1);
                        else
                            pixels[0 * 3 + 1] = center;

                        if (curComp.IsValidPixel(x + 1, y - 1))
                            pixels[0 * 3 + 2] = curComp.GetPixel<Vector3>(x + 1, y - 1);
                        else
                            pixels[0 * 3 + 2] = center;
                    }

                    {//line2
                        if (curComp.IsValidPixel(x - 1, y))
                            pixels[1 * 3 + 0] = curComp.GetPixel<Vector3>(x - 1, y);
                        else
                            pixels[1 * 3 + 0] = center;

                        if (curComp.IsValidPixel(x + 1, y))
                            pixels[1 * 3 + 2] = curComp.GetPixel<Vector3>(x + 1, y);
                        else
                            pixels[1 * 3 + 2] = center;
                    }

                    {//line3
                        if (curComp.IsValidPixel(x - 1, y + 1))
                            pixels[2 * 3 + 0] = curComp.GetPixel<Vector3>(x - 1, y + 1);
                        else
                            pixels[2 * 3 + 0] = center;

                        if (curComp.IsValidPixel(x, y + 1))
                            pixels[2 * 3 + 1] = curComp.GetPixel<Vector3>(x, y + 1);
                        else
                            pixels[2 * 3 + 1] = center;

                        if (curComp.IsValidPixel(x + 1, y + 1))
                            pixels[2 * 3 + 2] = curComp.GetPixel<Vector3>(x + 1, y + 1);
                        else
                            pixels[2 * 3 + 2] = center;
                    }

                    Vector3 value;
                    value.X = pixels[0 * 3 + 0].X * BlurMatrix[0, 0] + pixels[0 * 3 + 1].X * BlurMatrix[0, 1] + pixels[0 * 3 + 2].X * BlurMatrix[0, 2]
                        + pixels[1 * 3 + 0].X * BlurMatrix[1, 0] + pixels[1 * 3 + 1].X * BlurMatrix[1, 1] + pixels[1 * 3 + 2].X * BlurMatrix[1, 2]
                        + pixels[2 * 3 + 0].X * BlurMatrix[2, 0] + pixels[2 * 3 + 1].X * BlurMatrix[2, 1] + pixels[2 * 3 + 2].X * BlurMatrix[2, 2];
                    value.Y = pixels[0 * 3 + 0].Y * BlurMatrix[0, 0] + pixels[0 * 3 + 1].Y * BlurMatrix[0, 1] + pixels[0 * 3 + 2].Y * BlurMatrix[0, 2]
                        + pixels[1 * 3 + 0].Y * BlurMatrix[1, 0] + pixels[1 * 3 + 1].Y * BlurMatrix[1, 1] + pixels[1 * 3 + 2].Y * BlurMatrix[1, 2]
                        + pixels[2 * 3 + 0].Y * BlurMatrix[2, 0] + pixels[2 * 3 + 1].Y * BlurMatrix[2, 1] + pixels[2 * 3 + 2].Y * BlurMatrix[2, 2];
                    value.Z = pixels[0 * 3 + 0].Z * BlurMatrix[0, 0] + pixels[0 * 3 + 1].Z * BlurMatrix[0, 1] + pixels[0 * 3 + 2].Z * BlurMatrix[0, 2]
                        + pixels[1 * 3 + 0].Z * BlurMatrix[1, 0] + pixels[1 * 3 + 1].Z * BlurMatrix[1, 1] + pixels[1 * 3 + 2].Z * BlurMatrix[1, 2]
                        + pixels[2 * 3 + 0].Z * BlurMatrix[2, 0] + pixels[2 * 3 + 1].Z * BlurMatrix[2, 1] + pixels[2 * 3 + 2].Z * BlurMatrix[2, 2];
                    if (ClampBorder)
                    {
                        if (x == 0 || y == 0 || x == curComp.Height - 1 || y == curComp.Width - 1)
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
            //Vector3[,] pixels = new Vector3[3, 3];
            //for (int i = 0; i < curComp.Height; i++)
            //{
            //    for (int j = 0; j < curComp.Width; j++)
            //    {
            //        var center = curComp.GetPixel<Vector3>(j, i);
            //        pixels[1, 1] = center;

            //        {//line1
            //            if (curComp.IsValidPixel(j - 1, i - 1))
            //                pixels[0, 0] = curComp.GetPixel<Vector3>(j - 1, i - 1);
            //            else
            //                pixels[0, 0] = center;

            //            if (curComp.IsValidPixel(j, i - 1))
            //                pixels[0, 1] = curComp.GetPixel<Vector3>(j, i - 1);
            //            else
            //                pixels[0, 1] = center;

            //            if (curComp.IsValidPixel(j + 1, i - 1))
            //                pixels[0, 2] = curComp.GetPixel<Vector3>(j + 1, i - 1);
            //            else
            //                pixels[0, 2] = center;
            //        }

            //        {//line2
            //            if (curComp.IsValidPixel(j - 1, i))
            //                pixels[1, 0] = curComp.GetPixel<Vector3>(j - 1, i);
            //            else
            //                pixels[1, 0] = center;

            //            if (curComp.IsValidPixel(j + 1, i))
            //                pixels[1, 2] = curComp.GetPixel<Vector3>(j + 1, i);
            //            else
            //                pixels[1, 2] = center;
            //        }

            //        {//line3
            //            if (curComp.IsValidPixel(j - 1, i + 1))
            //                pixels[2, 0] = curComp.GetPixel<Vector3>(j - 1, i + 1);
            //            else
            //                pixels[2, 0] = center;

            //            if (curComp.IsValidPixel(j, i + 1))
            //                pixels[2, 1] = curComp.GetPixel<Vector3>(j, i + 1);
            //            else
            //                pixels[2, 1] = center;

            //            if (curComp.IsValidPixel(j + 1, i + 1))
            //                pixels[2, 2] = curComp.GetPixel<Vector3>(j + 1, i + 1);
            //            else
            //                pixels[2, 2] = center;
            //        }

            //        Vector3 value;
            //        value.X = pixels[0, 0].X * BlurMatrix[0, 0] + pixels[0, 1].X * BlurMatrix[0, 1] + pixels[0, 2].X * BlurMatrix[0, 2]
            //            + pixels[1, 0].X * BlurMatrix[1, 0] + pixels[1, 1].X * BlurMatrix[1, 1] + pixels[1, 2].X * BlurMatrix[1, 2]
            //            + pixels[2, 0].X * BlurMatrix[2, 0] + pixels[2, 1].X * BlurMatrix[2, 1] + pixels[2, 2].X * BlurMatrix[2, 2];
            //        value.Y = pixels[0, 0].Y * BlurMatrix[0, 0] + pixels[0, 1].Y * BlurMatrix[0, 1] + pixels[0, 2].Y * BlurMatrix[0, 2]
            //            + pixels[1, 0].Y * BlurMatrix[1, 0] + pixels[1, 1].Y * BlurMatrix[1, 1] + pixels[1, 2].Y * BlurMatrix[1, 2]
            //            + pixels[2, 0].Y * BlurMatrix[2, 0] + pixels[2, 1].Y * BlurMatrix[2, 1] + pixels[2, 2].Y * BlurMatrix[2, 2];
            //        value.Z = pixels[0, 0].Z * BlurMatrix[0, 0] + pixels[0, 1].Z * BlurMatrix[0, 1] + pixels[0, 2].Z * BlurMatrix[0, 2]
            //            + pixels[1, 0].Z * BlurMatrix[1, 0] + pixels[1, 1].Z * BlurMatrix[1, 1] + pixels[1, 2].Z * BlurMatrix[1, 2]
            //            + pixels[2, 0].Z * BlurMatrix[2, 0] + pixels[2, 1].Z * BlurMatrix[2, 1] + pixels[2, 2].Z * BlurMatrix[2, 2];
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
    [Bricks.CodeBuilder.ContextMenu("MeshMorph", "Float3\\Morph", UPgcGraph.PgcEditorKeyword)]
    public class UFloat3Morph : UOpNode
    {
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public PinIn InXYZ { get; set; } = new PinIn();
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public PinIn XBezierPin { get; set; } = new PinIn();
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public PinIn YBezierPin { get; set; } = new PinIn();
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public PinIn ZBezierPin { get; set; } = new PinIn();
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public PinOut OutXYZ { get; set; } = new PinOut();

        public UBufferCreator XYZBufferCreator { get; } = UBufferCreator.CreateInstance<USuperBuffer<Vector3, FFloat3Operator>>(-1, -1, -1);

        public enum EFactorAxis
        {
            FactorX,
            FactorY,
            FactorZ,
        }
        [Rtti.Meta]
        public EFactorAxis FactorAxis { get; set; } = EFactorAxis.FactorY;
        public UFloat3Morph()
        {
            Icon.Size = new Vector2(25, 25);
            Icon.Color = 0xFF00FF00;
            TitleColor = 0xFF204020;
            BackColor = 0x80808080;

            AddInput(InXYZ, "XYZ", XYZBufferCreator);
            AddInput(XBezierPin, "XBezier", DefaultInputDesc, "Bezier");
            AddInput(YBezierPin, "YBezier", DefaultInputDesc, "Bezier");
            AddInput(ZBezierPin, "ZBezier", DefaultInputDesc, "Bezier");
            AddOutput(OutXYZ, "XYZ", XYZBufferCreator);
        }
        public override UBufferCreator GetOutBufferCreator(PinOut pin)
        {
            if (OutXYZ == pin)
            {
                var graph = ParentGraph as UPgcGraph;
                var buffer = graph.BufferCache.FindBuffer(InXYZ);
                if (buffer != null)
                {
                    return buffer.BufferCreator;
                }
            }
            return base.GetOutBufferCreator(pin);
        }
        public override bool OnProcedure(UPgcGraph graph)
        {
            var xbzNode = GetInputNode(graph, XBezierPin) as UBezier;
            var ybzNode = GetInputNode(graph, YBezierPin) as UBezier;
            var zbzNode = GetInputNode(graph, ZBezierPin) as UBezier;

            var xyzSrc = graph.BufferCache.FindBuffer(InXYZ);
            var result = graph.BufferCache.FindBuffer(OutXYZ);

            Vector3 xyzMin = Vector3.MaxValue;
            Vector3 xyzMax = Vector3.MinValue;
            for (int i = 0; i < result.Depth; i++)
            {
                for (int j = 0; j < result.Height; j++)
                {
                    for (int k = 0; k < result.Width; k++)
                    {
                        ref var pos = ref xyzSrc.GetPixel<Vector3>(k, j, i);

                        xyzMax = Vector3.Maximize(in xyzMax, in pos);
                        xyzMin = Vector3.Minimize(in xyzMin, in pos);
                    }
                }
            }

            var range = xyzMax - xyzMin;
            for (int i = 0; i < result.Depth; i++)
            {
                for (int j = 0; j < result.Height; j++)
                {
                    for (int k = 0; k < result.Width; k++)
                    {
                        ref var pos = ref xyzSrc.GetPixel<Vector3>(k, j, i);

                        Vector3 dstPos = pos;
                        switch (FactorAxis)
                        {
                            case EFactorAxis.FactorX:
                                {
                                    //if (xbzNode != null)
                                    {
                                        var rate = (pos.X - xyzMin.X) / range.X;
                                        if (xbzNode != null)
                                        {
                                            var fMulvalue = BezierCalculate.ValueOnBezier(ybzNode.BzPoints, rate);
                                            dstPos.Y = dstPos.Y * fMulvalue.Y;
                                        }
                                        if (zbzNode != null)
                                        {
                                            var fMulvalue = BezierCalculate.ValueOnBezier(zbzNode.BzPoints, rate);
                                            dstPos.Z = dstPos.Z * fMulvalue.Y;
                                        }
                                    }
                                }
                                break;
                            case EFactorAxis.FactorY:
                                {
                                    //if (ybzNode != null)
                                    {
                                        var rate = (pos.Y - xyzMin.Y) / range.Y;
                                        if (xbzNode != null)
                                        {
                                            var fMulvalue = BezierCalculate.ValueOnBezier(xbzNode.BzPoints, rate);
                                            dstPos.X = dstPos.X * fMulvalue.Y;
                                        }
                                        if (zbzNode != null)
                                        {
                                            var fMulvalue = BezierCalculate.ValueOnBezier(zbzNode.BzPoints, rate);
                                            dstPos.Z = dstPos.Z * fMulvalue.Y;
                                        }
                                    }
                                }
                                break;
                            case EFactorAxis.FactorZ:
                                {
                                    //if (zbzNode != null)
                                    {
                                        var rate = (pos.Z - xyzMin.Z) / range.Z;
                                        if (xbzNode != null)
                                        {
                                            var fMulvalue = BezierCalculate.ValueOnBezier(xbzNode.BzPoints, rate);
                                            dstPos.X = dstPos.X * fMulvalue.Y;
                                        }
                                        if (ybzNode != null)
                                        {
                                            var fMulvalue = BezierCalculate.ValueOnBezier(zbzNode.BzPoints, rate);
                                            dstPos.Y = dstPos.Y * fMulvalue.Y;
                                        }
                                    }
                                }
                                break;
                        }

                        result.SetPixel<Vector3>(k, j, i, in dstPos);
                    }
                }
            }

            xyzSrc.LifeCount--;
            return true;
        }
    }
    [Bricks.CodeBuilder.ContextMenu("Transform", "Float3\\Transform", UPgcGraph.PgcEditorKeyword)]
    public class UFloat3Transform : UOpNode
    {
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public PinIn InXYZ { get; set; } = new PinIn();        
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public PinOut OutXYZ { get; set; } = new PinOut();
        FTransform mTransform = FTransform.Identity;
        [Rtti.Meta]
        public FTransform Transform 
        { 
            get => mTransform; 
            set { mTransform = value; } 
        }
        public UBufferCreator XYZBufferCreator { get; } = UBufferCreator.CreateInstance<USuperBuffer<Vector3, FFloat3Operator>>(-1, -1, -1);
        public UFloat3Transform()
        {
            Icon.Size = new Vector2(25, 25);
            Icon.Color = 0xFF00FF00;
            TitleColor = 0xFF204020;
            BackColor = 0x80808080;

            AddInput(InXYZ, "XYZ", XYZBufferCreator);
            AddOutput(OutXYZ, "XYZ", XYZBufferCreator);
        }
        public override UBufferCreator GetOutBufferCreator(PinOut pin)
        {
            if (OutXYZ == pin)
            {
                var graph = ParentGraph as UPgcGraph;
                var buffer = graph.BufferCache.FindBuffer(InXYZ);
                if (buffer != null)
                {
                    return buffer.BufferCreator;
                }
            }
            return base.GetOutBufferCreator(pin);
        }
        public override bool OnProcedure(UPgcGraph graph)
        {
            var xyzSrc = graph.BufferCache.FindBuffer(InXYZ);
            var result = graph.BufferCache.FindBuffer(OutXYZ);

            Vector3 xyzMin = Vector3.MaxValue;
            Vector3 xyzMax = Vector3.MinValue;
            var matrix = mTransform.ToMatrixWithScale(in DVector3.Zero);
            result.DispatchPixels((result, x, y, z) =>
            {
                ref var pos = ref xyzSrc.GetPixel<Vector3>(x, y, z);
                //var rPos = mTransform.TransformPosition(in pos)
                var rPos = Vector3.TransformCoordinate(in pos, in matrix);
                result.SetPixel<Vector3>(x, y, z, in rPos);
            }, true);

            //for (int i = 0; i < result.Depth; i++)
            //{
            //    for (int j = 0; j < result.Height; j++)
            //    {
            //        for (int k = 0; k < result.Width; k++)
            //        {
            //            ref var pos = ref xyzSrc.GetPixel<Vector3>(k, j, i);
            //            //var rPos = mTransform.TransformPosition(in pos)
            //            var rPos = Vector3.TransformCoordinate(in pos, in matrix);
            //            result.SetPixel<Vector3>(k, j, i, in rPos);
            //        }
            //    }
            //}
            return true;
        }
    }
}
