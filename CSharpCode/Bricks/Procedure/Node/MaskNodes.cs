using System;
using System.Collections.Generic;
using System.ComponentModel;
using EngineNS.Bricks.NodeGraph;

namespace EngineNS.Bricks.Procedure.Node
{
    [Rtti.Meta("", NameAlias = new string[] { "EngineNS.Bricks.Procedure.Node.UMaskBase@EngineCore", "EngineNS.Bricks.Procedure.Node.UMaskBase" })]
    public class TtMaskBase : TtBinocularWithMask
    {
        public TtMaskBase()
        {
            OutputDesc.BufferType = Rtti.TtTypeDesc.TypeOf<TtSuperBuffer<sbyte, FSByteOperator>>();
        }
        public override TtBufferCreator GetOutBufferCreator(PinOut pin)
        {
            if (ResultPin == pin)
            {
                var graph = ParentGraph as TtPgcGraph;
                var buffer = graph.BufferCache.FindBuffer(LeftPin);
                if (buffer != null)
                {
                    OutputDesc.SetSize(buffer.BufferCreator);
                    return OutputDesc;
                }
            }
            return base.GetOutBufferCreator(pin);
        }
        public override void OnLinkedFrom(PinIn iPin, TtNodeBase OutNode, PinOut oPin, TtPinLinker linker)
        {
            ParentGraph.RemoveLinkedInExcept(iPin, OutNode, oPin.Name);

            if (iPin == LeftPin)
            {
                var left = oPin.Tag as TtBufferCreator;
                var right = RightPin.Tag as TtBufferCreator;

                (LeftPin.Tag as TtBufferCreator).BufferType = left.BufferType;
                if (right.BufferType != left.BufferType)
                {
                    right.BufferType = left.BufferType;
                    this.ParentGraph.RemoveLinkedIn(RightPin);
                }
            }
        }
    }

    [Bricks.CodeBuilder.ContextMenu("GreatEqual", "Mask\\GreatEqual", TtPgcGraph.PgcEditorKeyword)]
    [Rtti.Meta("", NameAlias = new string[] { "EngineNS.Bricks.Procedure.Node.UGreatEqual@EngineCore", "EngineNS.Bricks.Procedure.Node.UGreatEqual" })]
    public class TtGreatEqual : TtMaskBase
    {
        public unsafe override void OnPerPixel(TtPgcGraph graph, TtPgcNodeBase node, TtBufferComponent result, int x, int y, int z, object tag)
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
    [Bricks.CodeBuilder.ContextMenu("Great", "Mask\\Great", TtPgcGraph.PgcEditorKeyword)]
    [Rtti.Meta("", NameAlias = new string[] { "EngineNS.Bricks.Procedure.Node.UGreat@EngineCore", "EngineNS.Bricks.Procedure.Node.UGreat" })]
    public class TtGreat : TtMaskBase
    {
        public unsafe override void OnPerPixel(TtPgcGraph graph, TtPgcNodeBase node, TtBufferComponent result, int x, int y, int z, object tag)
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
    [Bricks.CodeBuilder.ContextMenu("LessEqual", "Mask\\LessEqual", TtPgcGraph.PgcEditorKeyword)]
    [Rtti.Meta("", NameAlias = new string[] { "EngineNS.Bricks.Procedure.Node.ULessEqual@EngineCore", "EngineNS.Bricks.Procedure.Node.ULessEqual" })]
    public class TtLessEqual : TtMaskBase
    {
        public unsafe override void OnPerPixel(TtPgcGraph graph, TtPgcNodeBase node, TtBufferComponent result, int x, int y, int z, object tag)
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
    [Bricks.CodeBuilder.ContextMenu("Less", "Mask\\Less", TtPgcGraph.PgcEditorKeyword)]
    [Rtti.Meta("", NameAlias = new string[] { "EngineNS.Bricks.Procedure.Node.ULess@EngineCore", "EngineNS.Bricks.Procedure.Node.ULess" })]
    public class TtLess : TtMaskBase
    {
        public unsafe override void OnPerPixel(TtPgcGraph graph, TtPgcNodeBase node, TtBufferComponent result, int x, int y, int z, object tag)
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
    [Bricks.CodeBuilder.ContextMenu("Equal", "Mask\\Equal", TtPgcGraph.PgcEditorKeyword)]
    [Rtti.Meta("", NameAlias = new string[] { "EngineNS.Bricks.Procedure.Node.UEqual@EngineCore", "EngineNS.Bricks.Procedure.Node.UEqual" })]
    public class TtEqual : TtMaskBase
    {
        public unsafe override void OnPerPixel(TtPgcGraph graph, TtPgcNodeBase node, TtBufferComponent result, int x, int y, int z, object tag)
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
    [Bricks.CodeBuilder.ContextMenu("NotEqual", "Mask\\NotEqual", TtPgcGraph.PgcEditorKeyword)]
    [Rtti.Meta("", NameAlias = new string[] { "EngineNS.Bricks.Procedure.Node.UNotEqual@EngineCore", "EngineNS.Bricks.Procedure.Node.UNotEqual" })]
    public class TtNotEqual : TtMaskBase
    {
        public unsafe override void OnPerPixel(TtPgcGraph graph, TtPgcNodeBase node, TtBufferComponent result, int x, int y, int z, object tag)
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
    
    [Rtti.Meta("", NameAlias = new string[] { "EngineNS.Bricks.Procedure.Node.ULogicBoolean@EngineCore", "EngineNS.Bricks.Procedure.Node.ULogicBoolean" })]
    public class TtLogicBoolean : TtPgcNodeBase
    {
        [Browsable(false)]
        public PinIn LeftPin { get; set; } = new PinIn();
        [Browsable(false)]
        public PinIn RightPin { get; set; } = new PinIn();
        [Browsable(false)]
        public PinOut ResultPin { get; set; } = new PinOut();
        public TtBufferCreator InputDesc { get; } = TtBufferCreator.CreateInstance<TtSuperBuffer<sbyte, FSByteOperator>>(-1, -1, -1);
        public TtBufferCreator OutputDesc { get; } = TtBufferCreator.CreateInstance<TtSuperBuffer<sbyte, FSByteOperator>>(-1, -1, -1);
        public TtLogicBoolean()
        {
            Icon.Size = new Vector2(25, 25);
            Icon.Color = 0xFF00FF00;
            TitleColor = 0xFF204020;
            BackColor = 0x80808080;

            AddInput(LeftPin, "Left", InputDesc);
            AddInput(RightPin, "Right", InputDesc);
            AddOutput(ResultPin, "Result", OutputDesc);
        }
        public override TtBufferCreator GetOutBufferCreator(PinOut pin)
        {
            if (ResultPin == pin)
            {
                var graph = ParentGraph as TtPgcGraph;
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
    [Bricks.CodeBuilder.ContextMenu("And", "Mask\\Bool\\And", TtPgcGraph.PgcEditorKeyword)]
    [Rtti.Meta("", NameAlias = new string[] { "EngineNS.Bricks.Procedure.Node.UBooleanAnd@EngineCore", "EngineNS.Bricks.Procedure.Node.UBooleanAnd" })]
    public class TtBooleanAnd : TtLogicBoolean
    {
        public unsafe override bool OnProcedure(TtPgcGraph graph)
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
    [Bricks.CodeBuilder.ContextMenu("Or", "Mask\\Bool\\Or", TtPgcGraph.PgcEditorKeyword)]
    [Rtti.Meta("", NameAlias = new string[] { "EngineNS.Bricks.Procedure.Node.UBooleanOr@EngineCore", "EngineNS.Bricks.Procedure.Node.UBooleanOr" })]
    public class TtBooleanOr : TtLogicBoolean
    {
        public unsafe override bool OnProcedure(TtPgcGraph graph)
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
    [Bricks.CodeBuilder.ContextMenu("XOr", "Mask\\Bool\\XOr", TtPgcGraph.PgcEditorKeyword)]
    [Rtti.Meta("", NameAlias = new string[] { "EngineNS.Bricks.Procedure.Node.UBooleanXOr@EngineCore", "EngineNS.Bricks.Procedure.Node.UBooleanXOr" })]
    public class TtBooleanXOr : TtLogicBoolean
    {
        public unsafe override bool OnProcedure(TtPgcGraph graph)
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
    [Bricks.CodeBuilder.ContextMenu("Not", "Mask\\Bool\\Not", TtPgcGraph.PgcEditorKeyword)]
    [Rtti.Meta("", NameAlias = new string[] { "EngineNS.Bricks.Procedure.Node.UBooleanNot@EngineCore", "EngineNS.Bricks.Procedure.Node.UBooleanNot" })]
    public class TtBooleanNot : TtPgcNodeBase
    {
        [Browsable(false)]
        public PinIn SrcPin { get; set; } = new PinIn();
        [Browsable(false)]
        public PinOut ResultPin { get; set; } = new PinOut();
        public TtBufferCreator InputDesc { get; } = TtBufferCreator.CreateInstance<TtSuperBuffer<sbyte, FSByteOperator>>(-1, -1, -1);
        public TtBufferCreator OutputDesc { get; } = TtBufferCreator.CreateInstance<TtSuperBuffer<sbyte, FSByteOperator>>(-1, -1, -1);
        public TtBooleanNot()
        {
            Icon.Size = new Vector2(25, 25);
            Icon.Color = 0xFF00FF00;
            TitleColor = 0xFF204020;
            BackColor = 0x80808080;

            AddInput(SrcPin, "Src", InputDesc);
            AddOutput(ResultPin, "Result", OutputDesc);
        }
        public override TtBufferCreator GetOutBufferCreator(PinOut pin)
        {
            if (ResultPin == pin)
            {
                var graph = ParentGraph as TtPgcGraph;
                var buffer = graph.BufferCache.FindBuffer(SrcPin);
                if (buffer != null)
                {
                    OutputDesc.SetSize(buffer.BufferCreator);
                    return OutputDesc;
                }
            }
            return null;
        }
        public unsafe override bool OnProcedure(TtPgcGraph graph)
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

    [Bricks.CodeBuilder.ContextMenu("Sdf", "Mask\\Sdf", TtPgcGraph.PgcEditorKeyword)]
    [Rtti.Meta("", NameAlias = new string[] { "EngineNS.Bricks.Procedure.Node.USdfCalculator@EngineCore", "EngineNS.Bricks.Procedure.Node.USdfCalculator" })]
    public partial class TtSdfCalculator : TtMonocular
    {
        [Browsable(false)]
        public PinOut ClosestPin { get; set; } = new PinOut();
        public TtBufferCreator ClosestDesc { get; } = TtBufferCreator.CreateInstance<TtSuperBuffer<Vector3i, FInt3Operator>>(-1, -1, -1);
        public TtSdfCalculator()
        {
            SourceDesc.BufferType = Rtti.TtTypeDescGetter<TtSuperBuffer<sbyte, FSByteOperator>>.TypeDesc;

            AddOutput(ClosestPin, "Closest", ClosestDesc);
        }
        public override TtBufferCreator GetOutBufferCreator(PinOut pin)
        {
            if (ResultPin == pin)
            {
                var graph = ParentGraph as TtPgcGraph;
                var buffer = graph.BufferCache.FindBuffer(SrcPin);
                if (buffer != null)
                {
                    ResultDesc.SetSize(buffer.BufferCreator);
                    return ResultDesc;
                }
            }
            else if (ClosestPin == pin)
            {
                var graph = ParentGraph as TtPgcGraph;
                var buffer = graph.BufferCache.FindBuffer(SrcPin);
                if (buffer != null)
                {
                    ClosestDesc.SetSize(buffer.BufferCreator);
                    return ClosestDesc;
                }
            }
            return null;
        }
        public unsafe override bool OnProcedure(TtPgcGraph graph)
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
