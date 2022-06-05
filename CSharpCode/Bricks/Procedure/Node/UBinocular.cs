using System;
using System.Collections.Generic;
using EngineNS.Bricks.NodeGraph;

namespace EngineNS.Bricks.Procedure.Node
{
    public class ULeftRightBuffer
    {
        public UBufferConponent Left;
        public UBufferConponent Right;
        public Rtti.UTypeDesc ResultType;
        public Rtti.UTypeDesc LeftType;
        public Rtti.UTypeDesc RightType;
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
            Icon.Size = new Vector2(25, 25);
            Icon.Color = 0xFF00FF00;
            TitleColor = 0xFF204020;
            BackColor = 0x80808080;

            AddInput(LeftPin, "Left", DefaultInputDesc);
            AddInput(RightPin, "Right", DefaultInputDesc);
            AddOutput(ResultPin, "Result", DefaultBufferCreator);
        }
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
            arg.Left = left;
            arg.LeftType = leftType;
            arg.Right = right;
            arg.RightType = rightType;
            arg.ResultType = resultType;
            DispatchBuffer(graph, result, arg, true);
            //for (int i = 0; i < result.Depth; i++)
            //{
            //    for (int j = 0; j < result.Height; j++)
            //    {
            //        for (int k = 0; k < result.Width; k++)
            //        {
            //            float l_x = (float)(k * left.Width) / (float)result.Width;
            //            float l_y = (float)(j * left.Height) / (float)result.Height;
            //            float l_z = (float)(i * left.Depth) / (float)result.Depth;

            //            float r_x = (float)(k * right.Width) / (float)result.Width;
            //            float r_y = (float)(j * right.Height) / (float)result.Height;
            //            float r_z = (float)(i * right.Depth) / (float)result.Depth;
            //            //result.PixelOperator.Add(resultType, result.GetSuperPixelAddress(k, j, i), leftType, left.GetSuperPixelAddress((int)l_x, (int)l_y, i), rightType, right.GetSuperPixelAddress((int)r_x, (int)r_y, i));
            //            result.PixelOperator.Add(resultType, result.GetSuperPixelAddress(k, j, i), leftType, 
            //                left.GetSuperPixelAddress((int)l_x, (int)l_y, (int)l_z), rightType, 
            //                right.GetSuperPixelAddress((int)r_x, (int)r_y, (int)r_z));
            //        }
            //    }
            //}
            left.LifeCount--;
            if (right != null)
                right.LifeCount--;
            return true;
        }
    }
    [Bricks.CodeBuilder.ContextMenu("Add", "BaseOp\\Add", UPgcGraph.PgcEditorKeyword)]
    public class UPixelAdd : UBinocular
    {
        public unsafe override void OnPerPixel(UPgcGraph graph, UPgcNodeBase node, UBufferConponent result, int x, int y, int z, object tag)
        {
            var arg = tag as ULeftRightBuffer;
            var left = arg.Left;
            var right = arg.Right;
            var resultType = arg.ResultType;
            var leftType = arg.LeftType;
            var rightType = arg.RightType;

            float l_x = (float)(x * left.Width) / (float)result.Width;
            float l_y = (float)(y * left.Height) / (float)result.Height;
            float l_z = (float)(z * left.Depth) / (float)result.Depth;

            if (right == null)
            {
                result.PixelOperator.Copy(resultType, result.GetSuperPixelAddress(x, y, z), leftType, left.GetSuperPixelAddress((int)l_x, (int)l_y, (int)l_z));
                return;
            }

            float r_x = (float)(x * right.Width) / (float)result.Width;
            float r_y = (float)(y * right.Height) / (float)result.Height;
            float r_z = (float)(z * right.Depth) / (float)result.Depth;
            result.PixelOperator.Add(resultType, result.GetSuperPixelAddress(x, y, z), leftType, left.GetSuperPixelAddress((int)l_x, (int)l_y, (int)l_z), rightType, right.GetSuperPixelAddress((int)r_x, (int)r_y, (int)r_z));
        }
    }
    [Bricks.CodeBuilder.ContextMenu("Sub", "BaseOp\\Sub", UPgcGraph.PgcEditorKeyword)]
    public class UPixelSub : UBinocular
    {
        public unsafe override void OnPerPixel(UPgcGraph graph, UPgcNodeBase node, UBufferConponent result, int x, int y, int z, object tag)
        {
            var arg = tag as ULeftRightBuffer;
            var left = arg.Left;
            var right = arg.Right;
            var resultType = arg.ResultType;
            var leftType = arg.LeftType;
            var rightType = arg.RightType;

            float l_x = (float)(x * left.Width) / (float)result.Width;
            float l_y = (float)(y * left.Height) / (float)result.Height;
            float l_z = (float)(z * left.Depth) / (float)result.Depth;

            float r_x = (float)(x * right.Width) / (float)result.Width;
            float r_y = (float)(y * right.Height) / (float)result.Height;
            float r_z = (float)(z * right.Depth) / (float)result.Depth;
            result.PixelOperator.Sub(resultType, result.GetSuperPixelAddress(x, y, z), leftType, left.GetSuperPixelAddress((int)l_x, (int)l_y, (int)l_z), rightType, right.GetSuperPixelAddress((int)r_x, (int)r_y, (int)r_z));
        }
    }
    [Bricks.CodeBuilder.ContextMenu("Mul", "BaseOp\\Mul", UPgcGraph.PgcEditorKeyword)]
    public class UPixelMul : UBinocular
    {
        public unsafe override void OnPerPixel(UPgcGraph graph, UPgcNodeBase node, UBufferConponent result, int x, int y, int z, object tag)
        {
            var arg = tag as ULeftRightBuffer;
            var left = arg.Left;
            var right = arg.Right;
            var resultType = arg.ResultType;
            var leftType = arg.LeftType;
            var rightType = arg.RightType;

            float l_x = (float)(x * left.Width) / (float)result.Width;
            float l_y = (float)(y * left.Height) / (float)result.Height;
            float l_z = (float)(z * left.Depth) / (float)result.Depth;

            float r_x = (float)(x * right.Width) / (float)result.Width;
            float r_y = (float)(y * right.Height) / (float)result.Height;
            float r_z = (float)(z * right.Depth) / (float)result.Depth;
            result.PixelOperator.Mul(resultType, result.GetSuperPixelAddress(x, y, z), leftType, left.GetSuperPixelAddress((int)l_x, (int)l_y, (int)l_z), rightType, right.GetSuperPixelAddress((int)r_x, (int)r_y, (int)r_z));
        }
    }
    [Bricks.CodeBuilder.ContextMenu("Div", "BaseOp\\Div", UPgcGraph.PgcEditorKeyword)]
    public class UPixelDiv : UBinocular
    {
        public unsafe override void OnPerPixel(UPgcGraph graph, UPgcNodeBase node, UBufferConponent result, int x, int y, int z, object tag)
        {
            var arg = tag as ULeftRightBuffer;
            var left = arg.Left;
            var right = arg.Right;
            var resultType = arg.ResultType;
            var leftType = arg.LeftType;
            var rightType = arg.RightType;

            float l_x = (float)(x * left.Width) / (float)result.Width;
            float l_y = (float)(y * left.Height) / (float)result.Height;
            float l_z = (float)(z * left.Depth) / (float)result.Depth;

            float r_x = (float)(x * right.Width) / (float)result.Width;
            float r_y = (float)(y * right.Height) / (float)result.Height;
            float r_z = (float)(z * right.Depth) / (float)result.Depth;
            result.PixelOperator.Div(resultType, result.GetSuperPixelAddress(x, y, z), leftType, left.GetSuperPixelAddress((int)l_x, (int)l_y, (int)l_z), rightType, right.GetSuperPixelAddress((int)r_x, (int)r_y, (int)r_z));
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
