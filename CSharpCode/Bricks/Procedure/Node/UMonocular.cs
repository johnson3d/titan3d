using System;
using System.Collections.Generic;
using System.ComponentModel;
using EngineNS.Bricks.NodeGraph;

namespace EngineNS.Bricks.Procedure.Node
{
    public class UMonocular : UPgcNodeBase
    {
        [Browsable(false)]
        public PinIn SrcPin { get; set; } = new PinIn();
        [Browsable(false)]
        public PinOut ResultPin { get; set; } = new PinOut();
        [Rtti.Meta]
        public UBufferCreator SourceDesc { get; } = UBufferCreator.CreateInstance<USuperBuffer<float, FFloatOperator>>(-1, -1, -1);
        [Rtti.Meta]
        public UBufferCreator ResultDesc { get; } = UBufferCreator.CreateInstance<USuperBuffer<float, FFloatOperator>>(-1, -1, -1);
        public virtual UBufferCreator GetResultDesc()
        {
            return ResultDesc;
        }
        public UMonocular()
        {
            Icon.Size = new Vector2(25, 25);
            Icon.Color = 0xFF00FF00;
            TitleColor = 0xFF204020;
            BackColor = 0x80808080;

            AddInput(SrcPin, "Src", SourceDesc);
            AddOutput(ResultPin, "Result", GetResultDesc());
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
            return null;
        }
    }

    public class UMonocularWithMask : UMonocular
    {
        [Browsable(false)]
        public PinIn MaskPin { get; set; } = new PinIn();
        public UMonocularWithMask()
        {
            AddInput(MaskPin, "Mask", UBufferCreator.CreateInstance<USuperBuffer<sbyte, FSByteOperator>>(-1, -1, -1));
        }
        public bool IsMask(int x, int y, int z, USuperBuffer<sbyte, FSByteOperator> maskBuffer)
        {
            if (maskBuffer == null)
                return true;
            var uvw = maskBuffer.GetUVW(x, y, z);
            return maskBuffer.GetPixel<sbyte>(in uvw) == 1;
        }
        public bool IsMask(in Vector3 uvw, USuperBuffer<sbyte, FSByteOperator> maskBuffer)
        {
            if (maskBuffer == null)
                return true;

            return maskBuffer.GetPixel<sbyte>(in uvw) == 1;
        }
    }

    public class UAnyTypeMonocular : UMonocularWithMask
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

            (SrcPin.Tag as UBufferCreator).BufferType = input.BufferType;
            if (output.BufferType != input.BufferType)
            {   
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
            if (ResultPin == pin)
            {
                var graph = ParentGraph as UPgcGraph;
                var buffer = graph.BufferCache.FindBuffer(SrcPin);
                if (buffer != null)
                {
                    ResultDesc.BufferType = buffer.BufferCreator.BufferType;
                    return ResultDesc;
                }
            }
            return null;
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
            if (ResultPin == pin)
            {
                var graph = ParentGraph as UPgcGraph;
                var buffer = graph.BufferCache.FindBuffer(SrcPin);
                if (buffer != null)
                {
                    ResultDesc.BufferType = buffer.BufferCreator.BufferType;
                    return ResultDesc;
                }
            }
            return null;
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
            var mask = graph.BufferCache.FindBuffer(MaskPin) as USuperBuffer<sbyte, FSByteOperator>; ;
            var left = graph.BufferCache.FindBuffer(SrcPin);
            var result = graph.BufferCache.FindBuffer(ResultPin);
            var op = result.PixelOperator;

            var MulValue = Value;
            var resultType = result.BufferCreator.ElementType;
            var leftType = left.BufferCreator.ElementType;
            var rightType = Rtti.TtTypeDescGetter<float>.TypeDesc;
            for (int i = 0; i < result.Depth; i++)
            {
                for (int j = 0; j < result.Height; j++)
                {
                    for (int k = 0; k < result.Width; k++)
                    {
                        if (this.IsMask(k, j, i, mask) == false)
                        {
                            op.Copy(resultType, result.GetSuperPixelAddress(k, j, i), leftType, left.GetSuperPixelAddress(k, j, i));
                            continue;
                        }
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
    [Bricks.CodeBuilder.ContextMenu("Abs", "BaseOp\\Abs", UPgcGraph.PgcEditorKeyword)]
    public class UAbsNode : UAnyTypeMonocular
    {
        public UAbsNode()
        {
            
        }
        public unsafe override bool OnProcedure(UPgcGraph graph)
        {
            var mask = graph.BufferCache.FindBuffer(MaskPin) as USuperBuffer<sbyte, FSByteOperator>; ;
            var left = graph.BufferCache.FindBuffer(SrcPin);
            var result = graph.BufferCache.FindBuffer(ResultPin);
            var op = result.PixelOperator;

            var resultType = result.BufferCreator.ElementType;
            var leftType = left.BufferCreator.ElementType;
            for (int i = 0; i < result.Depth; i++)
            {
                for (int j = 0; j < result.Height; j++)
                {
                    for (int k = 0; k < result.Width; k++)
                    {
                        if (this.IsMask(k, j, i, mask) == false)
                        {
                            op.Copy(resultType, result.GetSuperPixelAddress(k, j, i), leftType, left.GetSuperPixelAddress(k, j, i));
                            continue;
                        }
                        op.Abs(result.GetSuperPixelAddress(k, j, i), left.GetSuperPixelAddress(k, j, i));
                    }
                }
            }

            left.LifeCount--;
            return true;
        }
    }
}
