using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace EngineNS.Bricks.NodeGraph
{
    public abstract class UNodePinDefineBase : IO.BaseSerializer
    {
        public static UNodePinDefineBase CreatePinDefineFromPin<T, T_Pin>(T_Pin pin) 
            where T : UNodePinDefineBase, new()
            where T_Pin : NodePin
        {
            var retVal = new T();
            retVal.InitFromPin(pin);
            return retVal;
        }
        protected abstract void InitFromPin<T>(T pin) where T : NodePin;
    }
    public partial interface IEndPointNode
    {
        public string Name { get; set; }
        public Guid NodeId { get; set; }
        public bool IsStart { get; set; }
        public Vector2 Position { get; set; }
        public List<UNodePinDefineBase> UserInputs { get; set; }
        public List<UNodePinDefineBase> UserOutputs { get; set; }
        public List<PinIn> Inputs { get; }
        public List<PinOut> Outputs { get; }
        public PinIn AddPinIn(PinIn pin);
        public PinOut AddPinOut(PinOut pin);
        public void UpdateLayout();
        public void UpdatePinWithDefine(NodePin pin, UNodePinDefineBase pinDef);
    }
    public interface IUnionNode
    {
        public UNodeGraph ContentGraph { get; set; }
        public Guid InputNodeId { get; set; }
        public Guid OutputNodeId { get; set; }
        public List<UNodePinDefineBase> UserInputs { get; set; }
        public List<UNodePinDefineBase> UserOutputs { get; set; }
        public Vector2 Position { get; set; }
        public Vector2 Size { get; set; }
        public List<PinIn> Inputs { get; }
        public List<PinOut> Outputs { get; }

        public PinOut AddPinOut(PinOut pin);
        public PinIn AddPinIn(PinIn pin);
        public void UpdateLayout();

        public void UpdatePinWithDefine(NodePin pin, UNodePinDefineBase pinDef);

        static Dictionary<NodePin, NodePin> mCreateTempPins = new Dictionary<NodePin, NodePin>();
        public static T_N CreateUnionNode<T_N, T_P, T_E>(UNodeGraph graph, List<UNodeBase> nodes, bool addtoGraph = true)
            where T_N : IUnionNode, new()
            where T_P : UNodePinDefineBase, new()
            where T_E : IEndPointNode, new()
        {
            var unionNode = new T_N();
            var contentGraph = Rtti.UTypeDescManager.CreateInstance(graph.GetType()) as UNodeGraph;
            contentGraph.Initialize();
            unionNode.ContentGraph = contentGraph;
            if (addtoGraph)
                graph.AddNode(unionNode as UNodeBase);

            var startNode = new T_E();
            startNode.Name = "Inputs";
            startNode.IsStart = true;
            contentGraph.AddNode(startNode as UNodeBase);
            unionNode.InputNodeId = startNode.NodeId;
            var endNode = new T_E();
            endNode.Name = "Outputs";
            endNode.IsStart = false;
            contentGraph.AddNode(endNode as UNodeBase);
            unionNode.OutputNodeId = endNode.NodeId;

            mCreateTempPins.Clear();
            Vector2 minSize = Vector2.MaxValue;
            Vector2 maxSize = Vector2.MinValue;
            for (int i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];
                minSize = Vector2.Minimize(node.Position, minSize);
                maxSize = Vector2.Maximize(node.Position + node.Size, maxSize);
            }
            var offset = new Vector2(200, 0);
            endNode.Position = new Vector2(offset.X * 2 + (maxSize.X - minSize.X), 0);
            for (int i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];
                var copyedNode = Rtti.UTypeDescManager.CreateInstance(node.GetType()) as UNodeBase;
                nodes[i].CopyTo(copyedNode);
                copyedNode.UserData = contentGraph;
                contentGraph.SetDefaultActionForNode(copyedNode);
                contentGraph.AddNode(copyedNode);
                copyedNode.Position = node.Position - minSize + offset;

                for (int pinIdx = 0; pinIdx < node.Inputs.Count; pinIdx++)
                {
                    var srcPin = node.Inputs[pinIdx];
                    var tagPin = copyedNode.Inputs[pinIdx];
                    mCreateTempPins[srcPin] = tagPin;
                }
                for (int pinIdx = 0; pinIdx < node.Outputs.Count; pinIdx++)
                {
                    var srcPin = node.Outputs[pinIdx];
                    var tagPin = copyedNode.Outputs[pinIdx];
                    mCreateTempPins[srcPin] = tagPin;
                }
            }

            for (int i = 0; i < graph.Linkers.Count; i++)
            {
                var linker = graph.Linkers[i];

                var hasInNode = nodes.Contains(linker.InNode);
                var hasOutNode = nodes.Contains(linker.OutNode);
                if (hasInNode && hasOutNode)
                {
                    var outPin = mCreateTempPins[linker.OutPin] as PinOut;
                    var inPin = mCreateTempPins[linker.InPin] as PinIn;
                    contentGraph.AddLink(outPin, inPin, true);
                }
                else if (hasOutNode)
                {
                    var pinOut = Rtti.UTypeDescManager.CreateInstance(linker.OutPin.GetType()) as PinOut;
                    linker.OutPin.CopyTo(pinOut);
                    unionNode.AddPinOut(pinOut);
                    var pinDef = UNodePinDefineBase.CreatePinDefineFromPin<T_P, PinOut>(linker.OutPin);
                    unionNode.UserOutputs.Add(pinDef);
                    unionNode.UpdatePinWithDefine(pinOut, pinDef);

                    var pinIn = new PinIn();
                    NodePin.CopyTo(pinOut, pinIn);
                    endNode.AddPinIn(pinIn);
                    var enPinDef = UNodePinDefineBase.CreatePinDefineFromPin<T_P, PinOut>(linker.OutPin);
                    endNode.UserInputs.Add(enPinDef);
                    endNode.UpdatePinWithDefine(pinIn, enPinDef);

                    var outPin = mCreateTempPins[linker.OutPin] as PinOut;
                    contentGraph.AddLink(outPin, pinIn, true);

                    linker.OutPin = pinOut;
                }
                else if (hasInNode)
                {
                    var pinIn = Rtti.UTypeDescManager.CreateInstance(linker.InPin.GetType()) as PinIn;
                    linker.InPin.CopyTo(pinIn);
                    unionNode.AddPinIn(pinIn);
                    var pinDef = UNodePinDefineBase.CreatePinDefineFromPin<T_P, PinIn>(linker.InPin);
                    unionNode.UserInputs.Add(pinDef);
                    unionNode.UpdatePinWithDefine(pinIn, pinDef);

                    var pinOut = new PinOut();
                    NodePin.CopyTo(pinIn, pinOut);
                    startNode.AddPinOut(pinOut);
                    var snPinDef = UNodePinDefineBase.CreatePinDefineFromPin<T_P, PinIn>(linker.InPin);
                    startNode.UserOutputs.Add(snPinDef);
                    startNode.UpdatePinWithDefine(pinOut, snPinDef);

                    var inPin = mCreateTempPins[linker.InPin] as PinIn;
                    contentGraph.AddLink(pinOut, inPin, true);

                    linker.InPin = pinIn;
                }
            }

            startNode.UpdateLayout();
            endNode.UpdateLayout();
            unionNode.UpdateLayout();
            unionNode.Position = (minSize + maxSize - unionNode.Size) * 0.5f;

            return unionNode;
        }
        static List<UPinLinker> mTemplinkers = new List<UPinLinker>();
        public static void ExpandUnionNode(UNodeGraph graph, IUnionNode unionNode)
        {
            mCreateTempPins.Clear();
            var contentGraph = unionNode.ContentGraph;
            Vector2 minSize = Vector2.MaxValue;
            Vector2 maxSize = Vector2.MinValue;
            for(int i = 0; i < contentGraph.Nodes.Count; i++)
            {
                var node = contentGraph.Nodes[i];
                if (node is IEndPointNode)
                    continue;
                minSize = Vector2.Minimize(node.Position, minSize);
                maxSize = Vector2.Maximize(node.Position + node.Size, maxSize);
            }
            var offset = unionNode.Position + unionNode.Size * 0.5f - (maxSize + minSize) * 0.5f;
            IEndPointNode startNode = null;
            IEndPointNode endNode = null;
            for (int i = 0; i < contentGraph.Nodes.Count; i++)
            {
                var node = contentGraph.Nodes[i];
                if (node is IEndPointNode)
                {
                    var endPN = node as IEndPointNode;
                    if(endPN.IsStart)
                    {
                        startNode = endPN;
                        for(int pinIdx=0; pinIdx<endPN.Outputs.Count; pinIdx++)
                        {
                            contentGraph.FindOutLinker(endPN.Outputs[pinIdx], mTemplinkers);
                            for(int linkIdx = 0; linkIdx < mTemplinkers.Count; linkIdx++)
                            {
                                mCreateTempPins[unionNode.Inputs[pinIdx]] = mTemplinkers[linkIdx].InPin;
                            }
                        }
                    }
                    else
                    {
                        endNode = endPN;
                        for(int pinIdx = 0; pinIdx < endPN.Inputs.Count; pinIdx++)
                        {
                            contentGraph.FindInLinker(endPN.Inputs[pinIdx], mTemplinkers);
                            for(int linkIdx = 0; linkIdx < mTemplinkers.Count; linkIdx++)
                            {
                                mCreateTempPins[unionNode.Outputs[pinIdx]] = mTemplinkers[linkIdx].OutPin;
                            }
                        }
                    }
                }
                else
                {
                    graph.AddNode(node);
                    node.Position += offset;
                }
            }

            for(int i=0; i<contentGraph.Linkers.Count; i++)
            {
                var linker = contentGraph.Linkers[i];
                if (startNode.Outputs.Contains(linker.OutPin))
                    continue;
                if (endNode.Inputs.Contains(linker.InPin))
                    continue;
                graph.Linkers.Add(linker);
            }

            for(int i=0; i<unionNode.Inputs.Count; i++)
            {
                var pin = unionNode.Inputs[i];
                graph.FindInLinker(pin, mTemplinkers);
                for(int linkerIdx = 0; linkerIdx < mTemplinkers.Count; linkerIdx++)
                {
                    var linker = mTemplinkers[linkerIdx];
                    if(mCreateTempPins.ContainsKey(linker.InPin))
                        linker.InPin = mCreateTempPins[linker.InPin] as PinIn;
                }
            }
            for(int i=0; i<unionNode.Outputs.Count; i++)
            {
                var pin = unionNode.Outputs[i];
                graph.FindOutLinker(pin, mTemplinkers);
                for(int linkerIdx = 0; linkerIdx < mTemplinkers.Count; linkerIdx++)
                {
                    var linker = mTemplinkers[linkerIdx];
                    if(mCreateTempPins.ContainsKey(linker.OutPin))
                        linker.OutPin = mCreateTempPins[linker.OutPin] as PinOut;
                }
            }

            graph.RemoveNode(unionNode as UNodeBase);
        }
    }
}
