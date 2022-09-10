using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.NodeGraph
{
    public class UPinLinker : IO.ISerializer
    {
        public virtual void OnPreRead(object tagObject, object hostObject, bool fromXml)
        {
            var graph = hostObject as UNodeGraph;
            if (graph == null)
                return;
            mGraph = graph;
        }
        public virtual void OnPropertyRead(object tagObject, System.Reflection.PropertyInfo prop, bool fromXml)
        {

        }
        UNodeGraph mGraph;
        public PinIn InPin { get; set; }
        public PinOut OutPin { get; set; }
        public bool InDebuggerLine = false;
        public UNodeBase InNode
        {
            get
            {
                return InPin.HostNode;
            }
        }
        public UNodeBase OutNode
        {
            get
            {
                return OutPin.HostNode;
            }
        }

        public class TSaveData : IO.BaseSerializer
        {
            [Rtti.Meta]
            public Guid InNodeId { get; set; }
            [Rtti.Meta]
            public Guid OutNodeId { get; set; }
            [Rtti.Meta]
            public string InPinName { get; set; }
            [Rtti.Meta]
            public string OutPinName { get; set; }
        }
        [Rtti.Meta]
        public TSaveData SaveData
        {
            get
            {
                var tmp = new TSaveData();
                tmp.InNodeId = InPin.NodeId;
                tmp.OutNodeId = OutPin.NodeId;
                tmp.InPinName = InPin.Name;
                tmp.OutPinName = OutPin.Name;
                return tmp;
            }
            set
            {
                if (mGraph != null)
                {
                    var inNode = mGraph.FindNode(value.InNodeId);
                    if (inNode != null)
                        InPin = inNode.FindPinIn(value.InPinName);

                    var outNode = mGraph.FindNode(value.OutNodeId);
                    if (outNode != null)
                        OutPin = outNode.FindPinOut(value.OutPinName);

                    if (InPin == null || OutPin == null)
                    {
                        mGraph.Linkers.Remove(this);
                    }
                    else
                    {
                        inNode.OnLoadLinker(this);
                        outNode.OnLoadLinker(this);
                        
                        inNode.UpdateLayout();
                        outNode.UpdateLayout();
                    }
                }
            }
        }
    }

    public class ULinkingLine
    {
        public ULinkingLine()
        {
            IsBlocking = false;
        }
        public NodePin StartPin;
        public NodePin HoverPin;

        public bool IsBlocking;
        public Vector2 BlockingEnd;
    }
}
