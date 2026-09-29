using System;
using System.Collections.Generic;
using System.ComponentModel;
using EngineNS.Bricks.NodeGraph;

namespace EngineNS.Bricks.Procedure.Node
{
    [Rtti.Meta("", NameAlias = new string[] { "EngineNS.Bricks.Procedure.Node.UMonocular@EngineCore", "EngineNS.Bricks.Procedure.Node.UMonocular" })]
    public class TtMonocular : TtPgcNodeBase
    {
        [Browsable(false)]
        public PinIn SrcPin { get; set; } = new PinIn();
        [Browsable(false)]
        public PinOut ResultPin { get; set; } = new PinOut();
        [Rtti.Meta("")]
        public TtBufferCreator SourceDesc { get; } = TtBufferCreator.CreateInstance<TtSuperBuffer<float, FFloatOperator>>(-1, -1, -1);
        [Rtti.Meta("")]
        public TtBufferCreator ResultDesc { get; } = TtBufferCreator.CreateInstance<TtSuperBuffer<float, FFloatOperator>>(-1, -1, -1);
        public virtual TtBufferCreator GetResultDesc()
        {
            return ResultDesc;
        }
        public TtMonocular()
        {
            Icon.Size = new Vector2(25, 25);
            Icon.Color = 0xFF00FF00;
            TitleColor = 0xFF204020;
            BackColor = 0x80808080;

            AddInput(SrcPin, "Src", SourceDesc);
            AddOutput(ResultPin, "Result", GetResultDesc());
        }

        public override void OnLinkedFrom(PinIn iPin, TtNodeBase OutNode, PinOut oPin, TtPinLinker linker)
        {
            base.OnLinkedFrom(iPin, OutNode, oPin, linker);
        }

        public override TtBufferCreator GetOutBufferCreator(PinOut pin)
        {
            if (ResultPin == pin)
            {
                var graph = ParentGraph as TtPgcGraph;
                var buffer = graph.BufferCache.FindBuffer(SrcPin);
                if (buffer != null)
                {
                    return buffer.BufferCreator;
                }
            }
            return null;
        }
    }

    [Rtti.Meta("", NameAlias = new string[] { "EngineNS.Bricks.Procedure.Node.UMonocularWithMask@EngineCore", "EngineNS.Bricks.Procedure.Node.UMonocularWithMask" })]
    public class TtMonocularWithMask : TtMonocular
    {
        [Browsable(false)]
        public PinIn MaskPin { get; set; } = new PinIn();
        public TtMonocularWithMask()
        {
            AddInput(MaskPin, "Mask", TtBufferCreator.CreateInstance<TtSuperBuffer<sbyte, FSByteOperator>>(-1, -1, -1));
        }
        public bool IsMask(int x, int y, int z, TtSuperBuffer<sbyte, FSByteOperator> maskBuffer)
        {
            if (maskBuffer == null)
                return true;
            var uvw = maskBuffer.GetUVW(x, y, z);
            return maskBuffer.GetPixel<sbyte>(in uvw) == 1;
        }
        public bool IsMask(in Vector3 uvw, TtSuperBuffer<sbyte, FSByteOperator> maskBuffer)
        {
            if (maskBuffer == null)
                return true;

            return maskBuffer.GetPixel<sbyte>(in uvw) == 1;
        }
    }

    [Rtti.Meta("", NameAlias = new string[] { "EngineNS.Bricks.Procedure.Node.UAnyTypeMonocular@EngineCore", "EngineNS.Bricks.Procedure.Node.UAnyTypeMonocular" })]
    public class TtAnyTypeMonocular : TtMonocularWithMask
    {
        public override bool IsMatchLinkedPin(TtBufferCreator input, TtBufferCreator output)
        {
            //base.IsMatchLinkedPin(input, output);
            return true;
        }
        public override void OnLinkedFrom(PinIn iPin, TtNodeBase OutNode, PinOut oPin, TtPinLinker linker)
        {
            base.OnLinkedFrom(iPin, OutNode, oPin, linker);

            var input = oPin.Tag as TtBufferCreator;
            var output = ResultPin.Tag as TtBufferCreator;

            (SrcPin.Tag as TtBufferCreator).BufferType = input.BufferType;
            if (output.BufferType != input.BufferType)
            {   
                output.BufferType = input.BufferType;
                //DefaultBufferCreator.BufferType = input.BufferType;
                this.ParentGraph.RemoveLinkedOut(ResultPin);
            }
        }
    }

    [Bricks.CodeBuilder.ContextMenu("CopyRect", "BaseOp\\CopyRect", TtPgcGraph.PgcEditorKeyword)]
    [Rtti.Meta("", NameAlias = new string[] { "EngineNS.Bricks.Procedure.Node.UCopyRect@EngineCore", "EngineNS.Bricks.Procedure.Node.UCopyRect" })]
    public class TtCopyRect : TtAnyTypeMonocular
    {
        [Rtti.Meta("")]
        public int X { get; set; } = 0;
        [Rtti.Meta("")]
        public int Y { get; set; } = 0;
        [Rtti.Meta("")]
        public int Z { get; set; } = 0;
        public override TtBufferCreator GetOutBufferCreator(PinOut pin)
        {
            if (ResultPin == pin)
            {
                var graph = ParentGraph as TtPgcGraph;
                var buffer = graph.BufferCache.FindBuffer(SrcPin);
                if (buffer != null)
                {
                    ResultDesc.BufferType = buffer.BufferCreator.BufferType;
                    return ResultDesc;
                }
            }
            return null;
        }
        public unsafe override bool OnProcedure(TtPgcGraph graph)
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

    [Bricks.CodeBuilder.ContextMenu("Stretch", "BaseOp\\Stretch", TtPgcGraph.PgcEditorKeyword)]
    [Rtti.Meta("", NameAlias = new string[] { "EngineNS.Bricks.Procedure.Node.UStretch@EngineCore", "EngineNS.Bricks.Procedure.Node.UStretch" })]
    public class TtStretch : TtAnyTypeMonocular
    {
        public override TtBufferCreator GetOutBufferCreator(PinOut pin)
        {
            if (ResultPin == pin)
            {
                var graph = ParentGraph as TtPgcGraph;
                var buffer = graph.BufferCache.FindBuffer(SrcPin);
                if (buffer != null)
                {
                    ResultDesc.BufferType = buffer.BufferCreator.BufferType;
                    return ResultDesc;
                }
            }
            return null;
        }
        public unsafe override bool OnProcedure(TtPgcGraph graph)
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
    [Bricks.CodeBuilder.ContextMenu("MulValue", "BaseOp\\MulValue", TtPgcGraph.PgcEditorKeyword)]
    [Rtti.Meta("", NameAlias = new string[] { "EngineNS.Bricks.Procedure.Node.UMulValue@EngineCore", "EngineNS.Bricks.Procedure.Node.UMulValue" })]
    public class TtMulValue : TtAnyTypeMonocular
    {
        public TtMulValue()
        {
            PrevSize = new Vector2(70, 30);
        }
        [Rtti.Meta("")]
        public float Value { get; set; } = 1.0f;
        public unsafe override bool OnProcedure(TtPgcGraph graph)
        {
            var mask = graph.BufferCache.FindBuffer(MaskPin) as TtSuperBuffer<sbyte, FSByteOperator>; ;
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
    [Bricks.CodeBuilder.ContextMenu("Abs", "BaseOp\\Abs", TtPgcGraph.PgcEditorKeyword)]
    [Rtti.Meta("", NameAlias = new string[] { "EngineNS.Bricks.Procedure.Node.UAbsNode@EngineCore", "EngineNS.Bricks.Procedure.Node.UAbsNode" })]
    public class TtAbsNode : TtAnyTypeMonocular
    {
        public TtAbsNode()
        {
            
        }
        public unsafe override bool OnProcedure(TtPgcGraph graph)
        {
            var mask = graph.BufferCache.FindBuffer(MaskPin) as TtSuperBuffer<sbyte, FSByteOperator>; ;
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
