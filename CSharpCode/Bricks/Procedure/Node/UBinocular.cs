using System;
using System.Collections.Generic;
using EngineNS.Bricks.NodeGraph;

namespace EngineNS.Bricks.Procedure.Node
{
    public class ULeftRightBuffer
    {
        public USuperBuffer<sbyte, FSByteOperator> Mask;
        public UBufferConponent Left;
        public UBufferConponent Right;
        public Rtti.UTypeDesc ResultType;
        public Rtti.UTypeDesc LeftType;
        public Rtti.UTypeDesc RightType;
    }
    public class UBinocular : UPgcNodeBase
    {
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public PinIn LeftPin { get; set; } = new PinIn();
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public PinIn RightPin { get; set; } = new PinIn();
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public PinOut ResultPin { get; set; } = new PinOut();
        [Rtti.Meta]
        public UBufferCreator InputLeftDesc { get; } = UBufferCreator.CreateInstance<USuperBuffer<float, FFloatOperator>>(-1, -1, -1);
        [Rtti.Meta]
        public UBufferCreator InputRightDesc { get; } = UBufferCreator.CreateInstance<USuperBuffer<float, FFloatOperator>>(-1, -1, -1);
        [Rtti.Meta]
        public UBufferCreator OutputDesc { get; } = UBufferCreator.CreateInstance<USuperBuffer<float, FFloatOperator>>(-1, -1, -1);
        public UBinocular()
        {
            Icon.Size = new Vector2(25, 25);
            Icon.Color = 0xFF00FF00;
            TitleColor = 0xFF204020;
            BackColor = 0x80808080;

            AddInput(LeftPin, "Left", InputLeftDesc);
            AddInput(RightPin, "Right", InputRightDesc);
            AddOutput(ResultPin, "Result", OutputDesc);
        }
        public override bool CanLinkFrom(PinIn iPin, UNodeBase OutNode, PinOut oPin)
        {
            if (iPin == LeftPin)
            {
                return true;
            }
            return base.CanLinkFrom(iPin, OutNode, oPin);
        }
        public override void OnLinkedFrom(PinIn iPin, UNodeBase OutNode, PinOut oPin)
        {
            base.OnLinkedFrom(iPin, OutNode, oPin);

            if (iPin == LeftPin)
            {
                var left = oPin.Tag as UBufferCreator;
                var output = ResultPin.Tag as UBufferCreator;
                var right = RightPin.Tag as UBufferCreator;

                (LeftPin.Tag as UBufferCreator).BufferType = left.BufferType;

                if (output.BufferType != left.BufferType)
                {   
                    output.BufferType = left.BufferType;
                    this.ParentGraph.RemoveLinkedOut(ResultPin);
                }
                if (right.BufferType != left.BufferType)
                {
                    right.BufferType = left.BufferType;
                    this.ParentGraph.RemoveLinkedIn(RightPin);
                }
            }   
        }
        public override UBufferCreator GetOutBufferCreator(PinOut pin)
        {
            if (ResultPin == pin)
            {
                var graph = ParentGraph as UPgcGraph;
                var buffer = graph.BufferCache.FindBuffer(LeftPin);
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
            var left = graph.BufferCache.FindBuffer(LeftPin);
            var right = graph.BufferCache.FindBuffer(RightPin);
            if (right != null)
            {
                if (GetInputBufferCreator(LeftPin).ElementType != GetInputBufferCreator(RightPin).ElementType
                    && left.Depth != right.Depth)
                {
                    left.LifeCount--;
                    right.LifeCount--;
                    return false;
                }
            }

            var result = graph.BufferCache.FindBuffer(ResultPin);
            var resultType = result.BufferCreator.ElementType;
            var leftType = left.BufferCreator.ElementType;
            var rightType = leftType;
            if (right != null)
                rightType = right.BufferCreator.ElementType;

            var arg = new ULeftRightBuffer();
            arg.Mask = null;
            arg.Left = left;
            arg.LeftType = leftType;
            arg.Right = right;
            arg.RightType = rightType;
            arg.ResultType = resultType;
            DispatchBuffer(graph, result, arg, true);
            
            left.LifeCount--;
            if (right != null)
                right.LifeCount--;
            return true;
        }
    }
    public class UBinocularWithMask : UBinocular
    {
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public PinIn MaskPin { get; set; } = new PinIn();        
        public UBinocularWithMask()
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
        public unsafe override bool OnProcedure(UPgcGraph graph)
        {
            var left = graph.BufferCache.FindBuffer(LeftPin);
            var right = graph.BufferCache.FindBuffer(RightPin);
            if (right != null)
            {
                if (GetInputBufferCreator(LeftPin).ElementType != GetInputBufferCreator(RightPin).ElementType
                    && left.Depth != right.Depth)
                {
                    left.LifeCount--;
                    right.LifeCount--;
                    return false;
                }
            }

            var result = graph.BufferCache.FindBuffer(ResultPin);
            var resultType = result.BufferCreator.ElementType;
            var leftType = left.BufferCreator.ElementType;
            var rightType = leftType;
            if (right != null)
                rightType = right.BufferCreator.ElementType;

            var arg = new ULeftRightBuffer();
            arg.Mask = graph.BufferCache.FindBuffer(MaskPin) as USuperBuffer<sbyte, FSByteOperator>;
            arg.Left = left;
            arg.LeftType = leftType;
            arg.Right = right;
            arg.RightType = rightType;
            arg.ResultType = resultType;
            DispatchBuffer(graph, result, arg, true);

            left.LifeCount--;
            if (right != null)
                right.LifeCount--;
            return true;
        }
    }
    [Bricks.CodeBuilder.ContextMenu("Add", "BaseOp\\Add", UPgcGraph.PgcEditorKeyword)]
    public class UPixelAdd : UBinocularWithMask
    {
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
            result.PixelOperator.Add(resultType, result.GetSuperPixelAddress(x, y, z), 
                leftType, left.GetSuperPixelAddress(x, y, z), 
                rightType, right.GetSuperPixelAddress(in uvw));
        }
    }
    [Bricks.CodeBuilder.ContextMenu("Sub", "BaseOp\\Sub", UPgcGraph.PgcEditorKeyword)]
    public class UPixelSub : UBinocularWithMask
    {
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
            result.PixelOperator.Sub(resultType, result.GetSuperPixelAddress(x, y, z),
                leftType, left.GetSuperPixelAddress(x, y, z),
                rightType, right.GetSuperPixelAddress(in uvw));
        }
    }
    [Bricks.CodeBuilder.ContextMenu("Mul", "BaseOp\\Mul", UPgcGraph.PgcEditorKeyword)]
    public class UPixelMul : UBinocularWithMask
    {
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
            result.PixelOperator.Mul(resultType, result.GetSuperPixelAddress(x, y, z),
                leftType, left.GetSuperPixelAddress(x, y, z),
                rightType, right.GetSuperPixelAddress(in uvw));
        }
    }
    [Bricks.CodeBuilder.ContextMenu("Div", "BaseOp\\Div", UPgcGraph.PgcEditorKeyword)]
    public class UPixelDiv : UBinocularWithMask
    {
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
            result.PixelOperator.Div(resultType, result.GetSuperPixelAddress(x, y, z),
                leftType, left.GetSuperPixelAddress(x, y, z),
                rightType, right.GetSuperPixelAddress(in uvw));
        }
    }
    [Bricks.CodeBuilder.ContextMenu("Max", "BaseOp\\Max", UPgcGraph.PgcEditorKeyword)]
    public class UPixelMax : UBinocularWithMask
    {
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
            result.PixelOperator.Max(result.GetSuperPixelAddress(x, y, z),
                left.GetSuperPixelAddress(x, y, z),
                right.GetSuperPixelAddress(in uvw));
        }
    }
    [Bricks.CodeBuilder.ContextMenu("Min", "BaseOp\\Min", UPgcGraph.PgcEditorKeyword)]
    public class UPixelMin : UBinocularWithMask
    {
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
            result.PixelOperator.Min(result.GetSuperPixelAddress(x, y, z),
                left.GetSuperPixelAddress(x, y, z),
                right.GetSuperPixelAddress(in uvw));
        }
    }
    [Bricks.CodeBuilder.ContextMenu("Lerp", "BaseOp\\Lerp", UPgcGraph.PgcEditorKeyword)]
    public class UPixelLerp : UBinocularWithMask
    {
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public PinIn FactorPin { get; set; } = new PinIn();
        public UPixelLerp()
        {
            AddInput(FactorPin, "Factor", UBufferCreator.CreateInstance<USuperBuffer<float, FFloatOperator>>(-1, -1, -1));
        }
        public unsafe override bool OnProcedure(UPgcGraph graph)
        {
            var mask = graph.BufferCache.FindBuffer(MaskPin) as USuperBuffer<sbyte, FSByteOperator>;
            var factor = graph.BufferCache.FindBuffer(FactorPin) as USuperBuffer<sbyte, FSByteOperator>;
            var left = graph.BufferCache.FindBuffer(LeftPin);
            var right = graph.BufferCache.FindBuffer(RightPin);
            if (right != null)
            {
                if (GetInputBufferCreator(LeftPin).ElementType != GetInputBufferCreator(RightPin).ElementType
                    && left.Depth != right.Depth)
                {
                    left.LifeCount--;
                    right.LifeCount--;
                    return false;
                }
            }

            var result = graph.BufferCache.FindBuffer(ResultPin);
            var resultType = result.BufferCreator.ElementType;
            var leftType = left.BufferCreator.ElementType;
            var rightType = leftType;
            if (right != null)
                rightType = right.BufferCreator.ElementType;

            result.DispatchPixels((target, x, y, z) =>
            {
                if (this.IsMask(x, y, z, mask) == false)
                {
                    result.PixelOperator.Copy(resultType, result.GetSuperPixelAddress(x, y, z),
                        leftType, left.GetSuperPixelAddress(x, y, z));
                    return;
                }

                var uvw = result.GetUVW(x, y, z);

                float f = 1.0f;
                if (factor != null)
                    f = factor.GetPixel<float>(in uvw);
                result.PixelOperator.Lerp(resultType, result.GetSuperPixelAddress(x, y, z),
                    leftType, left.GetSuperPixelAddress(x, y, z),
                    rightType, right.GetSuperPixelAddress(in uvw), f);
            }, true);

            left.LifeCount--;
            if (right != null)
                right.LifeCount--;
            return true;
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
            result.PixelOperator.Div(resultType, result.GetSuperPixelAddress(x, y, z),
                leftType, left.GetSuperPixelAddress(x, y, z),
                rightType, right.GetSuperPixelAddress(in uvw));
        }
    }
    [Bricks.CodeBuilder.ContextMenu("StretchBlt", "BaseOp\\StretchBlt", UPgcGraph.PgcEditorKeyword)]
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
        public override UBufferCreator GetOutBufferCreator(PinOut pin)
        {
            if (ResultPin == pin)
            {
                var graph = ParentGraph as UPgcGraph;
                var buffer = graph.BufferCache.FindBuffer(LeftPin);
                if (buffer != null)
                {
                    return buffer.BufferCreator;
                }
            }
            return base.GetOutBufferCreator(pin);
        }
        public unsafe override bool OnProcedure(UPgcGraph graph)
        {
            var left = graph.BufferCache.FindBuffer(LeftPin);
            var right = graph.BufferCache.FindBuffer(RightPin);
            var resultComp = graph.BufferCache.FindBuffer(ResultPin);
            //Copy Left to Result
            var op = resultComp.PixelOperator;
            var tarType = resultComp.BufferCreator.ElementType;
            var srcType = left.BufferCreator.ElementType;
            for (int i = 0; i < resultComp.Depth; i++)
            {
                for (int j = 0; j < resultComp.Height; j++)
                {
                    for (int k = 0; k < resultComp.Width; k++)
                    {
                        op.Copy(tarType, resultComp.GetSuperPixelAddress(k, j, i), srcType, left.GetSuperPixelAddress(k, j, i));
                    }
                }
            }
                
            int width = DstW;
            int height = DstH;
            int depth = resultComp.Depth;
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
            for (int i = 0; i < depth; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    for (int k = 0; k < width; k++)
                    {
                        float x = (float)(k * srcwidth) / (float)width;
                        float y = (float)(j * srcheight) / (float)height;

                        op.Copy(tarType, resultComp.GetSuperPixelAddress((int)DstX + k, (int)DstY + j, i),
                            srcType, right.GetSuperPixelAddress((int)SrcX + (int)x, (int)SrcY + (int)y, i));
                    }
                }
            }

            left.LifeCount--;
            right.LifeCount--;
            return true;
        }
    }
}
