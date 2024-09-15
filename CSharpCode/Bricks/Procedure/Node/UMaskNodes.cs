using System;
using System.Collections.Generic;
using System.ComponentModel;
using EngineNS.Bricks.NodeGraph;

namespace EngineNS.Bricks.Procedure.Node
{
    public class UMaskBase : UBinocularWithMask
    {
        public UMaskBase()
        {
            OutputDesc.BufferType = Rtti.TtTypeDesc.TypeOf<USuperBuffer<sbyte, FSByteOperator>>();
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
            return base.GetOutBufferCreator(pin);
        }
        public override void OnLinkedFrom(PinIn iPin, UNodeBase OutNode, PinOut oPin)
        {
            ParentGraph.RemoveLinkedInExcept(iPin, OutNode, oPin.Name);

            if (iPin == LeftPin)
            {
                var left = oPin.Tag as UBufferCreator;
                var right = RightPin.Tag as UBufferCreator;

                (LeftPin.Tag as UBufferCreator).BufferType = left.BufferType;
                if (right.BufferType != left.BufferType)
                {
                    right.BufferType = left.BufferType;
                    this.ParentGraph.RemoveLinkedIn(RightPin);
                }
            }
        }
    }

    [Bricks.CodeBuilder.ContextMenu("GreatEqual", "Mask\\GreatEqual", UPgcGraph.PgcEditorKeyword)]
    public class UGreatEqual : UMaskBase
    {
        public unsafe override void OnPerPixel(UPgcGraph graph, UPgcNodeBase node, UBufferComponent result, int x, int y, int z, object tag)
        {
            var arg = tag as ULeftRightBuffer;
            var left = arg.Left;
            var right = arg.Right;
            var resultType = arg.ResultType;
            var leftType = arg.LeftType;
            var rightType = arg.RightType;

            if (right == null || this.IsMask(x, y, z, arg.Mask) == false)
            {
                return;
            }

            var uvw = result.GetUVW(x, y, z);
            var cmp = left.PixelOperator.Compare(left.GetSuperPixelAddress(x, y, z), right.GetSuperPixelAddress(in uvw));
            if (cmp >= 0)
            {
                result.SetPixel<sbyte>(x, y, z, 1);
            }
            else
            {
                result.SetPixel<sbyte>(x, y, z, 0);
            }
        }
    }
    [Bricks.CodeBuilder.ContextMenu("Great", "Mask\\Great", UPgcGraph.PgcEditorKeyword)]
    public class UGreat : UMaskBase
    {
        public unsafe override void OnPerPixel(UPgcGraph graph, UPgcNodeBase node, UBufferComponent result, int x, int y, int z, object tag)
        {
            var arg = tag as ULeftRightBuffer;
            var left = arg.Left;
            var right = arg.Right;
            var resultType = arg.ResultType;
            var leftType = arg.LeftType;
            var rightType = arg.RightType;

            if (right == null || this.IsMask(x, y, z, arg.Mask) == false)
            {
                return;
            }

            var uvw = result.GetUVW(x, y, z);
            var cmp = left.PixelOperator.Compare(left.GetSuperPixelAddress(x, y, z), right.GetSuperPixelAddress(in uvw));
            if (cmp > 0)
            {
                result.SetPixel<sbyte>(x, y, z, 1);
            }
            else
            {
                result.SetPixel<sbyte>(x, y, z, 0);
            }
        }
    }
    [Bricks.CodeBuilder.ContextMenu("LessEqual", "Mask\\LessEqual", UPgcGraph.PgcEditorKeyword)]
    public class ULessEqual : UMaskBase
    {
        public unsafe override void OnPerPixel(UPgcGraph graph, UPgcNodeBase node, UBufferComponent result, int x, int y, int z, object tag)
        {
            var arg = tag as ULeftRightBuffer;
            var left = arg.Left;
            var right = arg.Right;
            var resultType = arg.ResultType;
            var leftType = arg.LeftType;
            var rightType = arg.RightType;

            if (right == null || this.IsMask(x, y, z, arg.Mask) == false)
            {
                return;
            }

            var uvw = result.GetUVW(x, y, z);
            var cmp = left.PixelOperator.Compare(left.GetSuperPixelAddress(x, y, z), right.GetSuperPixelAddress(in uvw));
            if (cmp <= 0)
            {
                result.SetPixel<sbyte>(x, y, z, 1);
            }
            else
            {
                result.SetPixel<sbyte>(x, y, z, 0);
            }
        }
    }
    [Bricks.CodeBuilder.ContextMenu("Less", "Mask\\Less", UPgcGraph.PgcEditorKeyword)]
    public class ULess : UMaskBase
    {
        public unsafe override void OnPerPixel(UPgcGraph graph, UPgcNodeBase node, UBufferComponent result, int x, int y, int z, object tag)
        {
            var arg = tag as ULeftRightBuffer;
            var left = arg.Left;
            var right = arg.Right;
            var resultType = arg.ResultType;
            var leftType = arg.LeftType;
            var rightType = arg.RightType;

            if (right == null || this.IsMask(x, y, z, arg.Mask) == false)
            {
                return;
            }

            var uvw = result.GetUVW(x, y, z);
            var cmp = left.PixelOperator.Compare(left.GetSuperPixelAddress(x, y, z), right.GetSuperPixelAddress(in uvw));
            if (cmp < 0)
            {
                result.SetPixel<sbyte>(x, y, z, 1);
            }
            else
            {
                result.SetPixel<sbyte>(x, y, z, 0);
            }
        }
    }
    [Bricks.CodeBuilder.ContextMenu("Equal", "Mask\\Equal", UPgcGraph.PgcEditorKeyword)]
    public class UEqual : UMaskBase
    {
        public unsafe override void OnPerPixel(UPgcGraph graph, UPgcNodeBase node, UBufferComponent result, int x, int y, int z, object tag)
        {
            var arg = tag as ULeftRightBuffer;
            var left = arg.Left;
            var right = arg.Right;
            var resultType = arg.ResultType;
            var leftType = arg.LeftType;
            var rightType = arg.RightType;

            if (right == null || this.IsMask(x, y, z, arg.Mask) == false)
            {
                return;
            }

            var uvw = result.GetUVW(x, y, z);
            var cmp = left.PixelOperator.Compare(left.GetSuperPixelAddress(x, y, z), right.GetSuperPixelAddress(in uvw));
            if (cmp == 0)
            {
                result.SetPixel<sbyte>(x, y, z, 1);
            }
            else
            {
                result.SetPixel<sbyte>(x, y, z, 0);
            }
        }
    }
    [Bricks.CodeBuilder.ContextMenu("NotEqual", "Mask\\NotEqual", UPgcGraph.PgcEditorKeyword)]
    public class UNotEqual : UMaskBase
    {
        public unsafe override void OnPerPixel(UPgcGraph graph, UPgcNodeBase node, UBufferComponent result, int x, int y, int z, object tag)
        {
            var arg = tag as ULeftRightBuffer;
            var left = arg.Left;
            var right = arg.Right;
            var resultType = arg.ResultType;
            var leftType = arg.LeftType;
            var rightType = arg.RightType;

            if (right == null || this.IsMask(x, y, z, arg.Mask) == false)
            {
                return;
            }

            var uvw = result.GetUVW(x, y, z);
            var cmp = left.PixelOperator.Compare(left.GetSuperPixelAddress(x, y, z), right.GetSuperPixelAddress(in uvw));
            if (cmp != 0)
            {
                result.SetPixel<sbyte>(x, y, z, 1);
            }
            else
            {
                result.SetPixel<sbyte>(x, y, z, 0);
            }
        }
    }
    
    public class ULogicBoolean : UPgcNodeBase
    {
        [Browsable(false)]
        public PinIn LeftPin { get; set; } = new PinIn();
        [Browsable(false)]
        public PinIn RightPin { get; set; } = new PinIn();
        [Browsable(false)]
        public PinOut ResultPin { get; set; } = new PinOut();
        public UBufferCreator InputDesc { get; } = UBufferCreator.CreateInstance<USuperBuffer<sbyte, FSByteOperator>>(-1, -1, -1);
        public UBufferCreator OutputDesc { get; } = UBufferCreator.CreateInstance<USuperBuffer<sbyte, FSByteOperator>>(-1, -1, -1);
        public ULogicBoolean()
        {
            Icon.Size = new Vector2(25, 25);
            Icon.Color = 0xFF00FF00;
            TitleColor = 0xFF204020;
            BackColor = 0x80808080;

            AddInput(LeftPin, "Left", InputDesc);
            AddInput(RightPin, "Right", InputDesc);
            AddOutput(ResultPin, "Result", OutputDesc);
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
    }
    [Bricks.CodeBuilder.ContextMenu("And", "Mask\\Bool\\And", UPgcGraph.PgcEditorKeyword)]
    public class UBooleanAnd : ULogicBoolean
    {
        public unsafe override bool OnProcedure(UPgcGraph graph)
        {
            var left = graph.BufferCache.FindBuffer(LeftPin);
            var right = graph.BufferCache.FindBuffer(RightPin);
            var result = graph.BufferCache.FindBuffer(ResultPin);

            result.DispatchPixels((target, x, y, z) =>
            {
                var l = left.GetPixel<sbyte>(x, y, z);
                var uvw = right.GetUVW(x, y, z);
                var r = right.GetPixel<sbyte>(in uvw);
                if (l != 0 && r != 0)
                {
                    target.SetPixel<sbyte>(x, y, z, 1);
                }
                else
                {
                    target.SetPixel<sbyte>(x, y, z, 0);
                }
            }, true);

            left.LifeCount--;
            right.LifeCount--;
            return true;
        }
    }
    [Bricks.CodeBuilder.ContextMenu("Or", "Mask\\Bool\\Or", UPgcGraph.PgcEditorKeyword)]
    public class UBooleanOr : ULogicBoolean
    {
        public unsafe override bool OnProcedure(UPgcGraph graph)
        {
            var left = graph.BufferCache.FindBuffer(LeftPin);
            var right = graph.BufferCache.FindBuffer(RightPin);
            var result = graph.BufferCache.FindBuffer(ResultPin);

            result.DispatchPixels((target, x, y, z) =>
            {
                var l = left.GetPixel<sbyte>(x, y, z);
                var uvw = right.GetUVW(x, y, z);
                var r = right.GetPixel<sbyte>(in uvw);
                if (l != 0 || r != 0)
                {
                    target.SetPixel<sbyte>(x, y, z, 1);
                }
                else
                {
                    target.SetPixel<sbyte>(x, y, z, 0);
                }
            }, true);

            left.LifeCount--;
            right.LifeCount--;
            return true;
        }
    }
    [Bricks.CodeBuilder.ContextMenu("XOr", "Mask\\Bool\\XOr", UPgcGraph.PgcEditorKeyword)]
    public class UBooleanXOr : ULogicBoolean
    {
        public unsafe override bool OnProcedure(UPgcGraph graph)
        {
            var left = graph.BufferCache.FindBuffer(LeftPin);
            var right = graph.BufferCache.FindBuffer(RightPin);
            var result = graph.BufferCache.FindBuffer(ResultPin);

            result.DispatchPixels((target, x, y, z) =>
            {
                var l = left.GetPixel<sbyte>(x, y, z);
                var uvw = right.GetUVW(x, y, z);
                var r = right.GetPixel<sbyte>(in uvw);
                if (l != r )
                {
                    target.SetPixel<sbyte>(x, y, z, 1);
                }
                else
                {
                    target.SetPixel<sbyte>(x, y, z, 0);
                }
            }, true);

            left.LifeCount--;
            right.LifeCount--;
            return true;
        }
    }
    [Bricks.CodeBuilder.ContextMenu("Not", "Mask\\Bool\\Not", UPgcGraph.PgcEditorKeyword)]
    public class UBooleanNot : UPgcNodeBase
    {
        [Browsable(false)]
        public PinIn SrcPin { get; set; } = new PinIn();
        [Browsable(false)]
        public PinOut ResultPin { get; set; } = new PinOut();
        public UBufferCreator InputDesc { get; } = UBufferCreator.CreateInstance<USuperBuffer<sbyte, FSByteOperator>>(-1, -1, -1);
        public UBufferCreator OutputDesc { get; } = UBufferCreator.CreateInstance<USuperBuffer<sbyte, FSByteOperator>>(-1, -1, -1);
        public UBooleanNot()
        {
            Icon.Size = new Vector2(25, 25);
            Icon.Color = 0xFF00FF00;
            TitleColor = 0xFF204020;
            BackColor = 0x80808080;

            AddInput(SrcPin, "Src", InputDesc);
            AddOutput(ResultPin, "Result", OutputDesc);
        }
        public override UBufferCreator GetOutBufferCreator(PinOut pin)
        {
            if (ResultPin == pin)
            {
                var graph = ParentGraph as UPgcGraph;
                var buffer = graph.BufferCache.FindBuffer(SrcPin);
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
            var left = graph.BufferCache.FindBuffer(SrcPin);
            var result = graph.BufferCache.FindBuffer(ResultPin);

            result.DispatchPixels((target, x, y, z) =>
            {
                var lv = left.GetPixel<sbyte>(x, y, z);
                if (lv == 0)
                    target.SetPixel<sbyte>(x, y, z, 1);
                else
                    target.SetPixel<sbyte>(x, y, z, 0);
            }, true);

            left.LifeCount--;
            return true;
        }
    }

    [Bricks.CodeBuilder.ContextMenu("Sdf", "Mask\\Sdf", UPgcGraph.PgcEditorKeyword)]
    public partial class USdfCalculator : UMonocular
    {
        [Browsable(false)]
        public PinOut ClosestPin { get; set; } = new PinOut();
        public UBufferCreator ClosestDesc { get; } = UBufferCreator.CreateInstance<USuperBuffer<Vector3i, FInt3Operator>>(-1, -1, -1);
        public USdfCalculator()
        {
            SourceDesc.BufferType = Rtti.TtTypeDescGetter<USuperBuffer<sbyte, FSByteOperator>>.TypeDesc;

            AddOutput(ClosestPin, "Closest", ClosestDesc);
        }
        public override UBufferCreator GetOutBufferCreator(PinOut pin)
        {
            if (ResultPin == pin)
            {
                var graph = ParentGraph as UPgcGraph;
                var buffer = graph.BufferCache.FindBuffer(SrcPin);
                if (buffer != null)
                {
                    ResultDesc.SetSize(buffer.BufferCreator);
                    return ResultDesc;
                }
            }
            else if (ClosestPin == pin)
            {
                var graph = ParentGraph as UPgcGraph;
                var buffer = graph.BufferCache.FindBuffer(SrcPin);
                if (buffer != null)
                {
                    ClosestDesc.SetSize(buffer.BufferCreator);
                    return ClosestDesc;
                }
            }
            return null;
        }
        public unsafe override bool OnProcedure(UPgcGraph graph)
        {
            var curComp = graph.BufferCache.FindBuffer(SrcPin);
            var resultComp = graph.BufferCache.FindBuffer(ResultPin);
            var closestComp = graph.BufferCache.FindBuffer(ClosestPin);
            var sdfGrid = new Support.TtSdfGrid();
            sdfGrid.mCoreObject.InitGrid(curComp.Width, curComp.Height);
            for (int j = 0; j < curComp.Height; j++)
            {
                for (int k = 0; k < curComp.Width; k++)
                {
                    var s = curComp.GetPixel<sbyte>(k, j);
                    if (s == 0)
                        sdfGrid.mCoreObject.SetEmpty(k, j);
                    else
                        sdfGrid.mCoreObject.SetInside(k, j);
                }
            }
            sdfGrid.mCoreObject.GenerateSDF();

            for (int j = 0; j < curComp.Height; j++)
            {
                for (int k = 0; k < curComp.Width; k++)
                {
                    var dist = sdfGrid.mCoreObject.Get(k, j).Distance();
                    resultComp.SetFloat1(k, j, 0, dist);
                }
            }

            curComp.LifeCount--;
            return true;
        }
    }
}
