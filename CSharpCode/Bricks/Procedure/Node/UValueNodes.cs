using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using EngineNS.Bricks.NodeGraph;

namespace EngineNS.Bricks.Procedure.Node
{
    [Bricks.CodeBuilder.ContextMenu("Double3Value", "Float3\\Double3Value", UPgcGraph.PgcEditorKeyword)]
    public class UDouble3ValueNode : UPgcNodeBase
    {
        [Browsable(false)]
        public PinOut ResultPin { get; set; } = new PinOut();
        public UBufferCreator OutputDesc { get; } = UBufferCreator.CreateInstance<USuperBuffer<DVector3, FDouble3Operator>>(-1, -1, -1);
        public UDouble3ValueNode()
        {
            Icon.Size = new Vector2(25, 25);
            Icon.Color = 0xFF00FF00;
            TitleColor = 0xFF204020;
            BackColor = 0x80808080;

            AddOutput(ResultPin, "Result", OutputDesc);
        }
        [Rtti.Meta]
        public DVector3 Value { get; set; } = DVector3.One;
        public unsafe override bool OnProcedure(UPgcGraph graph)
        {
            var result = graph.BufferCache.FindBuffer(ResultPin);

            for (int i = 0; i < result.Depth; i++)
            {
                for (int j = 0; j < result.Height; j++)
                {
                    for (int k = 0; k < result.Width; k++)
                    {
                        result.SetDouble3(k, j, i, Value);
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

    [Bricks.CodeBuilder.ContextMenu("QuaternionValue", "Values\\QuaternionValue", UPgcGraph.PgcEditorKeyword)]
    public class UQuaternionValueNode : UPgcNodeBase
    {
        [Browsable(false)]
        public PinOut ResultPin { get; set; } = new PinOut();
        public UBufferCreator OutputDesc { get; } = UBufferCreator.CreateInstance<USuperBuffer<Quaternion, FQuaternionOperator>>(-1, -1, -1);
        public UQuaternionValueNode()
        {
            Icon.Size = new Vector2(25, 25);
            Icon.Color = 0xFF00FF00;
            TitleColor = 0xFF204020;
            BackColor = 0x80808080;

            AddOutput(ResultPin, "Result", OutputDesc);
        }
        [Rtti.Meta]
        public Quaternion Value { get; set; } = Quaternion.Identity;
        public unsafe override bool OnProcedure(UPgcGraph graph)
        {
            var result = graph.BufferCache.FindBuffer(ResultPin);

            for (int i = 0; i < result.Depth; i++)
            {
                for (int j = 0; j < result.Height; j++)
                {
                    for (int k = 0; k < result.Width; k++)
                    {
                        result.SetPixel(k, j, i, Value);
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
    [Bricks.CodeBuilder.ContextMenu("IntValue", "Values\\IntValue", UPgcGraph.PgcEditorKeyword)]
    public class UIntValueNode : UPgcNodeBase
    {
        [Browsable(false)]
        public PinOut ResultPin { get; set; } = new PinOut();
        public UBufferCreator OutputDesc { get; } = UBufferCreator.CreateInstance<USuperBuffer<int, FIntOperator>>(-1, -1, -1);
        public UIntValueNode()
        {
            Icon.Size = new Vector2(25, 25);
            Icon.Color = 0xFF00FF00;
            TitleColor = 0xFF204020;
            BackColor = 0x80808080;

            AddOutput(ResultPin, "Result", OutputDesc);
        }
        [Rtti.Meta]
        public int Value { get; set; } = 0;
        public unsafe override bool OnProcedure(UPgcGraph graph)
        {
            var result = graph.BufferCache.FindBuffer(ResultPin);

            for (int i = 0; i < result.Depth; i++)
            {
                for (int j = 0; j < result.Height; j++)
                {
                    for (int k = 0; k < result.Width; k++)
                    {
                        result.SetInt1(k, j, i, Value);
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
