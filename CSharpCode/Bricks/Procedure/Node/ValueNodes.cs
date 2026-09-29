using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using EngineNS.Bricks.NodeGraph;

namespace EngineNS.Bricks.Procedure.Node
{
    [Bricks.CodeBuilder.ContextMenu("Double3Value", "Float3\\Double3Value", TtPgcGraph.PgcEditorKeyword)]
    [Rtti.Meta("", NameAlias = new string[] { "EngineNS.Bricks.Procedure.Node.UDouble3ValueNode@EngineCore", "EngineNS.Bricks.Procedure.Node.UDouble3ValueNode" })]
    public class TtDouble3ValueNode : TtPgcNodeBase
    {
        [Browsable(false)]
        public PinOut ResultPin { get; set; } = new PinOut();
        public TtBufferCreator OutputDesc { get; } = TtBufferCreator.CreateInstance<TtSuperBuffer<DVector3, FDouble3Operator>>(-1, -1, -1);
        public TtDouble3ValueNode()
        {
            Icon.Size = new Vector2(25, 25);
            Icon.Color = 0xFF00FF00;
            TitleColor = 0xFF204020;
            BackColor = 0x80808080;

            AddOutput(ResultPin, "Result", OutputDesc);
        }
        [Rtti.Meta("")]
        public DVector3 Value { get; set; } = DVector3.One;
        public unsafe override bool OnProcedure(TtPgcGraph graph)
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
        public override TtBufferCreator GetOutBufferCreator(PinOut pin)
        {
            return OutputDesc;
        }
    }

    [Bricks.CodeBuilder.ContextMenu("QuaternionValue", "Values\\QuaternionValue", TtPgcGraph.PgcEditorKeyword)]
    [Rtti.Meta("", NameAlias = new string[] { "EngineNS.Bricks.Procedure.Node.UQuaternionValueNode@EngineCore", "EngineNS.Bricks.Procedure.Node.UQuaternionValueNode" })]
    public class TtQuaternionValueNode : TtPgcNodeBase
    {
        [Browsable(false)]
        public PinOut ResultPin { get; set; } = new PinOut();
        public TtBufferCreator OutputDesc { get; } = TtBufferCreator.CreateInstance<TtSuperBuffer<Quaternion, FQuaternionOperator>>(-1, -1, -1);
        public TtQuaternionValueNode()
        {
            Icon.Size = new Vector2(25, 25);
            Icon.Color = 0xFF00FF00;
            TitleColor = 0xFF204020;
            BackColor = 0x80808080;

            AddOutput(ResultPin, "Result", OutputDesc);
        }
        [Rtti.Meta("")]
        public Quaternion Value { get; set; } = Quaternion.Identity;
        public unsafe override bool OnProcedure(TtPgcGraph graph)
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
        public override TtBufferCreator GetOutBufferCreator(PinOut pin)
        {
            return OutputDesc;
        }
    }
    [Bricks.CodeBuilder.ContextMenu("IntValue", "Values\\IntValue", TtPgcGraph.PgcEditorKeyword)]
    [Rtti.Meta("", NameAlias = new string[] { "EngineNS.Bricks.Procedure.Node.UIntValueNode@EngineCore", "EngineNS.Bricks.Procedure.Node.UIntValueNode" })]
    public class TtIntValueNode : TtPgcNodeBase
    {
        [Browsable(false)]
        public PinOut ResultPin { get; set; } = new PinOut();
        public TtBufferCreator OutputDesc { get; } = TtBufferCreator.CreateInstance<TtSuperBuffer<int, FIntOperator>>(-1, -1, -1);
        public TtIntValueNode()
        {
            Icon.Size = new Vector2(25, 25);
            Icon.Color = 0xFF00FF00;
            TitleColor = 0xFF204020;
            BackColor = 0x80808080;

            AddOutput(ResultPin, "Result", OutputDesc);
        }
        [Rtti.Meta("")]
        public int Value { get; set; } = 0;
        public unsafe override bool OnProcedure(TtPgcGraph graph)
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
        public override TtBufferCreator GetOutBufferCreator(PinOut pin)
        {
            return OutputDesc;
        }
    }
}
