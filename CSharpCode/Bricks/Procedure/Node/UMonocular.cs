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

            AddInput(SrcPin, " Src", DefaultInputDesc);
            AddOutput(ResultPin, " Result", DefaultBufferCreator);
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
}
