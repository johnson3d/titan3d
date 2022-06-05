using System;
using System.Collections.Generic;
using EngineNS.Bricks.NodeGraph;

namespace EngineNS.Bricks.Procedure.Node
{
    public class UMonocular : UOpNode
    {
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public PinIn SrcPin { get; set; } = new PinIn();
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public PinOut ResultPin { get; set; } = new PinOut();

        public UMonocular()
        {
            Icon.Size = new Vector2(25, 25);
            Icon.Color = 0xFF00FF00;
            TitleColor = 0xFF204020;
            BackColor = 0x80808080;

            AddInput(SrcPin, "Src", DefaultInputDesc);
            AddOutput(ResultPin, "Result", DefaultBufferCreator);
        }

        public override void OnLinkedFrom(PinIn iPin, UNodeBase OutNode, PinOut oPin)
        {
            base.OnLinkedFrom(iPin, OutNode, oPin);
        }

        public override UBufferCreator GetOutBufferCreator(PinOut pin)
        {
            if (ResultPin == pin)
            {
                var graph = ParentGraph as UPgcGraph;
                var buffer = graph.BufferCache.FindBuffer(SrcPin);
                if (buffer != null)
                {
                    return buffer.BufferCreator;
                }
            }
            return base.GetOutBufferCreator(pin);
        }
    }

    public class UAnyTypeMonocular : UMonocular
    {
        public override bool IsMatchLinkedPin(UBufferCreator input, UBufferCreator output)
        {
            //base.IsMatchLinkedPin(input, output);
            return true;
        }
        public override void OnLinkedFrom(PinIn iPin, UNodeBase OutNode, PinOut oPin)
        {
            base.OnLinkedFrom(iPin, OutNode, oPin);

            var input = oPin.Tag as UBufferCreator;
            var output = ResultPin.Tag as UBufferCreator;

            if (output.BufferType != input.BufferType)
            {
                (SrcPin.Tag as UBufferCreator).BufferType = input.BufferType;
                output.BufferType = input.BufferType;
                //DefaultBufferCreator.BufferType = input.BufferType;
                this.ParentGraph.RemoveLinkedOut(ResultPin);
            }
        }
    }

    [Bricks.CodeBuilder.ContextMenu("CopyRect", "BaseOp\\CopyRect", UPgcGraph.PgcEditorKeyword)]
    public class UCopyRect : UAnyTypeMonocular
    {
        [Rtti.Meta]
        public int X { get; set; } = 0;
        [Rtti.Meta]
        public int Y { get; set; } = 0;
        [Rtti.Meta]
        public int Z { get; set; } = 0;
        public override UBufferCreator GetOutBufferCreator(PinOut pin)
        {
            var graph = this.ParentGraph as UPgcGraph;
            var result = UBufferCreator.CreateInstance(DefaultBufferCreator.BufferType,
                DefaultBufferCreator.XSize,
                DefaultBufferCreator.YSize,
                DefaultBufferCreator.ZSize);
            if (result.XSize == -1)
            {
                result.XSize = graph.DefaultCreator.XSize;
            }
            if (result.YSize == -1)
            {
                result.YSize = graph.DefaultCreator.YSize;
            }
            if (result.ZSize == -1)
            {
                result.ZSize = graph.DefaultCreator.ZSize;
            }
            return result;
        }
        public unsafe override bool OnProcedure(UPgcGraph graph)
        {
            var curComp = graph.BufferCache.FindBuffer(SrcPin);
            var resultComp = graph.BufferCache.FindBuffer(ResultPin);
            resultComp.DispatchPixels((result, x, y, z) =>
            {
                var srcAddress = curComp.GetSuperPixelAddress(X + x, Y + y, Z + z);

                result.SetSuperPixelAddress(x, y, z, srcAddress);
            }, true);
            //for (int i = 0; i < resultComp.Depth; i++)
            //{
            //    for (int j = 0; j < resultComp.Height; j++)
            //    {
            //        for (int k = 0; k < resultComp.Width; k++)
            //        {
            //            var srcAddress = curComp.GetSuperPixelAddress(X + k, Y + j, Z + i);

            //            resultComp.SetSuperPixelAddress(k, j, i, srcAddress);
            //        }
            //    }
            //}
            curComp.LifeCount--;
            return true;
        }
    }

    [Bricks.CodeBuilder.ContextMenu("Stretch", "BaseOp\\Stretch", UPgcGraph.PgcEditorKeyword)]
    public class UStretch : UAnyTypeMonocular
    {

        public override UBufferCreator GetOutBufferCreator(PinOut pin)
        {
            var graph = this.ParentGraph as UPgcGraph;
            var result = UBufferCreator.CreateInstance(DefaultBufferCreator.BufferType,
                DefaultBufferCreator.XSize,
                DefaultBufferCreator.YSize,
                DefaultBufferCreator.ZSize);
            if (result.XSize == -1)
            {
                result.XSize = graph.DefaultCreator.XSize;
            }
            if (result.YSize == -1)
            {
                result.YSize = graph.DefaultCreator.YSize;
            }
            if (result.ZSize == -1)
            {
                result.ZSize = graph.DefaultCreator.ZSize;
            }
            return result;
        }
        public unsafe override bool OnProcedure(UPgcGraph graph)
        {
            var left = graph.BufferCache.FindBuffer(SrcPin);
            var resultComp = graph.BufferCache.FindBuffer(ResultPin);
            var Op = resultComp.PixelOperator;
            var tarType = resultComp.BufferCreator.ElementType;
            var srcType = left.BufferCreator.ElementType;
            for (int i = 0; i < resultComp.Depth; i++)
            {
                for (int j = 0; j < resultComp.Height; j++)
                {
                    for (int k = 0; k < resultComp.Width; k++)
                    {
                        float x = (float)(k * left.Width) / (float)resultComp.Width;
                        float y = (float)(j * left.Height) / (float)resultComp.Height;
                        float z = (float)(i * left.Depth) / (float)resultComp.Depth;

                        Op.Copy(tarType, resultComp.GetSuperPixelAddress(k, j, i), srcType, left.GetSuperPixelAddress((int)x, (int)y, (int)z));
                    }
                }
            }

            left.LifeCount--;
            return true;
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
    [Bricks.CodeBuilder.ContextMenu("MulValue", "BaseOp\\MulValue", UPgcGraph.PgcEditorKeyword)]
    public class UMulValue : UAnyTypeMonocular
    {
        public UMulValue()
        {
            PrevSize = new Vector2(70, 30);
        }
        [Rtti.Meta]
        public float Value { get; set; } = 1.0f;
        public unsafe override bool OnProcedure(UPgcGraph graph)
        {
            var left = graph.BufferCache.FindBuffer(SrcPin);
            var result = graph.BufferCache.FindBuffer(ResultPin);
            var op = result.PixelOperator;

            var MulValue = Value;
            var resultType = result.BufferCreator.ElementType;
            var leftType = left.BufferCreator.ElementType;
            var rightType = Rtti.UTypeDescGetter<float>.TypeDesc;
            for (int i = 0; i < result.Depth; i++)
            {
                for (int j = 0; j < result.Height; j++)
                {
                    for (int k = 0; k < result.Width; k++)
                    {
                        op.Mul(resultType, result.GetSuperPixelAddress(k, j, i), leftType, left.GetSuperPixelAddress(k, j, i), rightType, &MulValue);
                    }
                }
            }

            left.LifeCount--;
            return true;
        }

        public unsafe override void OnPreviewDraw(in Vector2 prevStart, in Vector2 prevEnd, ImDrawList cmdlist)
        {
            base.OnPreviewDraw(in prevStart, in prevEnd, cmdlist);

            unsafe
            {
                cmdlist.AddText(in prevStart, 0xFFFFFFFF, $"{Value}", null);
            }
        }
    }
}
