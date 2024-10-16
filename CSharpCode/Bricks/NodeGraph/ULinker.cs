using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.NodeGraph
{
    public class UPinLinker : IO.ISerializer
    {
        public virtual void OnPreRead(object tagObject, object hostObject, bool fromXml)
        {
            var graph = hostObject as TtNodeGraph;
            if (graph == null)
                return;
            mGraph = graph;
        }
        public virtual void OnPropertyRead(object tagObject, System.Reflection.PropertyInfo prop, bool fromXml)
        {

        }
        TtNodeGraph mGraph;
        public PinIn InPin { get; set; }
        public PinOut OutPin { get; set; }
        public bool InDebuggerLine = false;
        public TtNodeBase InNode
        {
            get
            {
                return InPin?.HostNode;
            }
        }
        public TtNodeBase OutNode
        {
            get
            {
                return OutPin?.HostNode;
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
                if (InPin != null)//??
                {
                    tmp.InNodeId = InPin.NodeId;
                    tmp.InPinName = InPin.Name;
                }
                if (OutPin != null)//??
                {
                    tmp.OutNodeId = OutPin.NodeId;
                    tmp.OutPinName = OutPin.Name;
                }
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
        public Vector2 DragPosition;
        public bool IsDraging
        {
            get
            {
                if (StartPin == null)
                    return false;
                if (StartPin.IsHit(DragPosition.X, DragPosition.Y))
                    return false;
                return true;
            }
        }

        public void Reset()
        {
            StartPin = null;
            HoverPin = null;
            IsBlocking = false;
        }
    }
}
