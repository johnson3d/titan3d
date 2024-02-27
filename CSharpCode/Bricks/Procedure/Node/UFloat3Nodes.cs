using System;
using System.Collections.Generic;
using EngineNS.Bricks.NodeGraph;
using System.ComponentModel;

namespace EngineNS.Bricks.Procedure.Node
{
    [Bricks.CodeBuilder.ContextMenu("Float3Value", "Float3\\Float3Value", UPgcGraph.PgcEditorKeyword)]
    public class UFloat3ValueNode : UPgcNodeBase
    {
        [Browsable(false)]
        public PinOut ResultPin { get; set; } = new PinOut();
        public UBufferCreator OutputDesc { get; } = UBufferCreator.CreateInstance<USuperBuffer<Vector3, FFloat3Operator>>(-1, -1, -1);
        public UFloat3ValueNode()
        {
            Icon.Size = new Vector2(25, 25);
            Icon.Color = 0xFF00FF00;
            TitleColor = 0xFF204020;
            BackColor = 0x80808080;

            AddOutput(ResultPin, "Result", OutputDesc);
        }
        [Rtti.Meta]
        public Vector3 Value { get; set; } = Vector3.One;
        public unsafe override bool OnProcedure(UPgcGraph graph)
        {
            var result = graph.BufferCache.FindBuffer(ResultPin);

            for (int i = 0; i < result.Depth; i++)
            {
                for (int j = 0; j < result.Height; j++)
                {
                    for (int k = 0; k < result.Width; k++)
                    {
                        result.SetFloat3(k, j, i, Value);
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
    [Bricks.CodeBuilder.ContextMenu("Float3Unpack", "Float3\\Unpack", UPgcGraph.PgcEditorKeyword)]
    public class UFloat3UnpackNodes : UPgcNodeBase
    {
        [Browsable(false)]
        public PinIn InXYZ { get; set; } = new PinIn();
        [Browsable(false)]
        public PinOut XPin { get; set; } = new PinOut();
        [Browsable(false)]
        public PinOut YPin { get; set; } = new PinOut();
        [Browsable(false)]
        public PinOut ZPin { get; set; } = new PinOut();

        public UBufferCreator InputFloat3Desc { get; } = UBufferCreator.CreateInstance<USuperBuffer<Vector3, FFloat3Operator>>(-1, -1, -1);
        public UBufferCreator OutputFloatDesc { get; } = UBufferCreator.CreateInstance<USuperBuffer<float, FFloatOperator>>(-1, -1, -1);
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
                    OutputFloatDesc.SetSize(buffer.BufferCreator);
                    return OutputFloatDesc;
                }
            }
            return null;
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
        [Browsable(false)]
        public PinIn XPin { get; set; } = new PinIn();
        [Browsable(false)]
        public PinIn YPin { get; set; } = new PinIn();
        [Browsable(false)]
        public PinIn ZPin { get; set; } = new PinIn();
        [Browsable(false)]
        public PinOut OutXYZ { get; set; } = new PinOut();

        public UBufferCreator InputFloatDesc { get; } = UBufferCreator.CreateInstance< USuperBuffer <float, FFloatOperator>>(-1, -1, -1);
        public UBufferCreator OutputFloat3Desc { get; } = UBufferCreator.CreateInstance<USuperBuffer<Vector3, FFloat3Operator>>(-1, -1, -1);

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
                    OutputFloat3Desc.SetSize(buffer.BufferCreator);
                    return OutputFloat3Desc;
                }
            }
            return null;
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
    [Bricks.CodeBuilder.ContextMenu("UVW", "Float3\\UVW", UPgcGraph.PgcEditorKeyword)]
    public class UUVWNode : UPgcNodeBase
    {
        [Browsable(false)]
        public PinOut ResultPin { get; set; } = new PinOut();
        public UBufferCreator OutputDesc { get; } = UBufferCreator.CreateInstance<USuperBuffer<Vector3, FFloat3Operator>>(-1, -1, -1);
        public UUVWNode()
        {
            Icon.Size = new Vector2(25, 25);
            Icon.Color = 0xFF00FF00;
            TitleColor = 0xFF204020;
            BackColor = 0x80808080;

            AddOutput(ResultPin, "Result", OutputDesc);
        }
        public override UBufferCreator GetOutBufferCreator(PinOut pin)
        {
            return OutputDesc;
        }
        public unsafe override bool OnProcedure(UPgcGraph graph)
        {
            var result = graph.BufferCache.FindBuffer(ResultPin);

            for (int i = 0; i < result.Depth; i++)
            {
                for (int j = 0; j < result.Height; j++)
                {
                    for (int k = 0; k < result.Width; k++)
                    {
                        var uvw = result.GetUVW(k, j, i);

                        result.SetPixel(k, j, i, uvw);
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
        [Browsable(false)]
        public PinIn HFieldPin { get; set; } = new PinIn();
        [Browsable(false)]
        public PinOut Normal { get; set; } = new PinOut();

        public UBufferCreator InputFloatDesc { get; } = UBufferCreator.CreateInstance<USuperBuffer<float, FFloatOperator>>(-1, -1, -1);
        public UBufferCreator OutputFloat3Desc { get; } = UBufferCreator.CreateInstance<USuperBuffer<Vector3, FFloat3Operator>>(-1, -1, -1);

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
                    OutputFloat3Desc.SetSize(buffer.BufferCreator);
                    return OutputFloat3Desc;
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
            var xyzResult = graph.BufferCache.FindBuffer(Normal);
            float minHeight, maxHeight;
            heightFiels.GetRangeUnsafe<float, FFloatOperator>(out minHeight, out maxHeight);
            //float range = HeightRange;// maxHeight - minHeight;
            xyzResult.DispatchPixels((result, x, y, z) =>
            {
                if (x < 1 || y < 1 || x >= result.Width - 1 || y >= result.Height - 1)
                    return;
                //float altInfo = heightFiels.GetPixel<float>(x, y);
                //float v_du = heightFiels.GetPixel<float>(x + 1, y);
                //float v_dv = heightFiels.GetPixel<float>(x, y + 1);

                //var A = new Vector3(GridSize, (v_du - altInfo), 0);
                //var B = new Vector3(0, (v_dv - altInfo), -GridSize);

                //var n = Vector3.Cross(A, B);
                //n = Vector3.Normalize(n);
                /* It's terrain mesh topology
                 *-1-2
                 |/|/|
                 6-0-3
                 |/|/|
                 5-4-*
                 */
                float h0 = heightFiels.GetPixel<float>(x, y);
                float h1 = heightFiels.GetPixel<float>(x, y + 1) - h0;
                float h2 = heightFiels.GetPixel<float>(x + 1, y + 1) - h0;
                float h3 = heightFiels.GetPixel<float>(x + 1, y) - h0;
                float h4 = heightFiels.GetPixel<float>(x, y - 1) - h0;
                float h5 = heightFiels.GetPixel<float>(x - 1, y - 1) - h0;
                float h6 = heightFiels.GetPixel<float>(x - 1, y) - h0;

                Vector3 v0 = new Vector3(0, 0, 0);
                Vector3 v1 = new Vector3(0, h1, GridSize);
                Vector3 v2 = new Vector3(GridSize, h2, GridSize);
                Vector3 v3 = new Vector3(GridSize, h3, 0);
                Vector3 v4 = new Vector3(0, h4, -GridSize);
                Vector3 v5 = new Vector3(-GridSize, h5, -GridSize);
                Vector3 v6 = new Vector3(-GridSize, h6, 0);

                Vector3 n = Vector3.Zero;
                var A = v1 - v0;
                var B = v2 - v0;
                n += Vector3.Normalize(Vector3.Cross(A, B));

                A = B;
                B = v3 - v0;
                n += Vector3.Normalize(Vector3.Cross(A, B));

                A = B;
                B = v4 - v0;
                n += Vector3.Normalize(Vector3.Cross(A, B));

                A = B;
                B = v4 - v0;
                n += Vector3.Normalize(Vector3.Cross(A, B));

                A = B;
                B = v5 - v0;
                n += Vector3.Normalize(Vector3.Cross(A, B));

                A = B;
                B = v6 - v0;
                n += Vector3.Normalize(Vector3.Cross(A, B));

                n /= 6.0f;
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
        [Browsable(false)]
        public PinIn InXYZ { get; set; } = new PinIn();
        [Browsable(false)]
        public PinOut OutXYZ { get; set; } = new PinOut();
        public UBufferCreator InputFloat3Desc { get; } = UBufferCreator.CreateInstance<USuperBuffer<Vector3, FFloat3Operator>>(-1, -1, -1);
        public UBufferCreator OutputFloat3Desc { get; } = UBufferCreator.CreateInstance<USuperBuffer<Vector3, FFloat3Operator>>(-1, -1, -1);
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
                    OutputFloat3Desc.SetSize(buffer.BufferCreator);
                    return OutputFloat3Desc;
                }
            }
            return null;
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

    [Bricks.CodeBuilder.ContextMenu("Dot", "Float3\\Dot", UPgcGraph.PgcEditorKeyword)]
    public class UFloat3Dot : UBinocularWithMask
    {
        public UFloat3Dot()
        {
            InputLeftDesc.BufferType = Rtti.UTypeDesc.TypeOf<USuperBuffer<Vector3, FFloat3Operator>>();
            InputRightDesc.BufferType = Rtti.UTypeDesc.TypeOf<USuperBuffer<Vector3, FFloat3Operator>>();
            OutputDesc.BufferType = Rtti.UTypeDesc.TypeOf<USuperBuffer<float, FFloatOperator>>();
        }
        public override bool CanLinkFrom(PinIn iPin, UNodeBase OutNode, PinOut oPin)
        {
            if (base.CanLinkFrom(iPin, OutNode, oPin) == false)
                return false;
            var input = iPin.Tag as UBufferCreator;
            var output = oPin.Tag as UBufferCreator;

            if (IsMatchLinkedPin(input, output) == false)
            {
                return false;
            }
            return true;
        }
        public override void OnLinkedFrom(PinIn iPin, UNodeBase OutNode, PinOut oPin)
        {
            if (iPin.LinkDesc.CanLinks.Contains("Value"))
            {
                ParentGraph.RemoveLinkedInExcept(iPin, OutNode, oPin.Name);
            }
        }
        public unsafe override void OnPerPixel(UPgcGraph graph, UPgcNodeBase node, UBufferConponent result, int x, int y, int z, object tag)
        {
            var arg = tag as ULeftRightBuffer;
            var left = arg.Left;
            var right = arg.Right;
            var resultType = arg.ResultType;
            var leftType = arg.LeftType;
            var rightType = arg.RightType;

            if (right == null || this.IsMask(x, y, z, arg.Mask) == false)
            {
                result.PixelOperator.Copy(resultType, result.GetSuperPixelAddress(x, y, z),
                    leftType, left.GetSuperPixelAddress(x, y, z));
                return;
            }

            var uvw = result.GetUVW(x, y, z);
            ref var lv = ref left.GetPixel<Vector3>(x, y, z);
            ref var rv = ref right.GetPixel<Vector3>(in uvw);
            var v = Vector3.Dot(in lv, in rv);
            result.SetPixel<float>(x, y, z, in v);
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
            SourceDesc.BufferType = Rtti.UTypeDesc.TypeOf<USuperBuffer<Vector3, FFloat3Operator>>();
            ResultDesc.BufferType = Rtti.UTypeDesc.TypeOf<USuperBuffer<Vector3, FFloat3Operator>>();
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
    public class UFloat3Morph : UPgcNodeBase
    {
        [Browsable(false)]
        public PinIn InXYZ { get; set; } = new PinIn();
        [Browsable(false)]
        public PinIn XBezierPin { get; set; } = new PinIn();
        [Browsable(false)]
        public PinIn YBezierPin { get; set; } = new PinIn();
        [Browsable(false)]
        public PinIn ZBezierPin { get; set; } = new PinIn();
        [Browsable(false)]
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
            AddInput(XBezierPin, "XBezier", null, "Bezier");
            AddInput(YBezierPin, "YBezier", null, "Bezier");
            AddInput(ZBezierPin, "ZBezier", null, "Bezier");
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
                    XYZBufferCreator.SetSize(buffer.BufferCreator);
                    return XYZBufferCreator;
                }
            }
            return null;
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
    public class UFloat3Transform : UPgcNodeBase
    {
        [Browsable(false)]
        public PinIn InXYZ { get; set; } = new PinIn();        
        [Browsable(false)]
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
                    XYZBufferCreator.SetSize(buffer.BufferCreator);
                    return XYZBufferCreator;
                }
            }
            return null;
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
                //var rPos = mRenderTransform.TransformPosition(in pos)
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
            //            //var rPos = mRenderTransform.TransformPosition(in pos)
            //            var rPos = Vector3.TransformCoordinate(in pos, in matrix);
            //            result.SetPixel<Vector3>(k, j, i, in rPos);
            //        }
            //    }
            //}
            return true;
        }
    }
}
