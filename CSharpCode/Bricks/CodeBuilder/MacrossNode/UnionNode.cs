using EngineNS.Bricks.NodeGraph;
using EngineNS.Rtti;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.CodeBuilder.MacrossNode
{
    public class UnionPinDefine : UNodePinDefineBase
    {
        [Rtti.Meta]
        public string Name { get; set; } = "UserPin";
        [Rtti.Meta]
        public UTypeDesc Type { get; set; }
        [Rtti.Meta]
        public LinkDesc LinkDesc { get; set; }
        protected override void InitFromPin<T>(T pin)
        {
            Name = pin.Name;
            Type = pin.HostNode.GetPinType(pin);
            LinkDesc = pin.LinkDesc;
        }
    }

    public class EndPointNode : UNodeBase, IEndPointNode
    {
        [Rtti.Meta]
        public bool IsStart { get; set; }
        List<UNodePinDefineBase> mUserInputs = new List<UNodePinDefineBase>();
        [Rtti.Meta]
        public List<UNodePinDefineBase> UserInputs
        {
            get => mUserInputs;
            set
            {
                mUserInputs = value;
                UpdateInputs();
            }
        }
        List<UNodePinDefineBase> mUserOutputs = new List<UNodePinDefineBase>();
        [Rtti.Meta]
        public List<UNodePinDefineBase> UserOutputs
        {
            get => mUserOutputs;
            set
            {
                mUserOutputs = value;
                UpdateOutputs();
            }
        }
        public void UpdateInputs()
        {
            for (int i = 0; i < Inputs.Count; i++)
            {
                ParentGraph.RemoveLinkedIn(Inputs[i]);
            }
            Inputs.Clear();
            for (int i = 0; i < mUserInputs.Count; i++)
            {
                var input = mUserInputs[i] as UnionPinDefine;
                var pin = new PinIn();
                pin.Name = input.Name;
                pin.Tag = input.Type;
                AddPinIn(pin);
            }
        }
        public void UpdateOutputs()
        {
            for (int i = 0; i < Outputs.Count; i++)
            {
                ParentGraph.RemoveLinkedOut(Outputs[i]);
            }
            Outputs.Clear();
            for (int i = 0; i < mUserOutputs.Count; i++)
            {
                var output = mUserOutputs[i] as UnionPinDefine;
                var pin = new PinOut();
                pin.Name = output.Name;
                pin.Tag = output.Type;
                AddPinOut(pin);
            }
        }

        public override UTypeDesc GetOutPinType(PinOut pin)
        {
            for(int i=0; i<Outputs.Count; i++)
            {
                if(Outputs[i] == pin)
                {
                    return ((UnionPinDefine)mUserOutputs[i]).Type;
                }
            }
            return null;
        }

        public override void OnMouseStayPin(NodePin stayPin)
        {
            if(IsStart)
            {
                for(int i=0; i<Outputs.Count; i++)
                {
                    if (Outputs[i].Tag == null || Outputs[i] != stayPin)
                        continue;
                    var valueString = GetRuntimeValueString(Outputs[i].Name + "_" + (uint)NodeId.GetHashCode());
                    var typeString = (Outputs[i].Tag as UTypeDesc).FullName;
                    EGui.Controls.CtrlUtility.DrawHelper($"{valueString}({typeString})");
                }
            }
            else
            {
                for (int i = 0; i < Inputs.Count; i++)
                {
                    if (Inputs[i].Tag == null || Inputs[i] != stayPin)
                        continue;
                    var valueString = GetRuntimeValueString(Inputs[i].Name + "_" + (uint)NodeId.GetHashCode());
                    var typeString = (Inputs[i].Tag as UTypeDesc).FullName;
                    EGui.Controls.CtrlUtility.DrawHelper($"{valueString}({typeString})");
                }
            }
        }
        public void UpdatePinWithDefine(NodePin pin, UNodePinDefineBase pinDef)
        {
            pin.Tag = ((UnionPinDefine)pinDef).Type;
        }

        public override UExpressionBase GetExpression(NodePin pin, ref BuildCodeStatementsData data)
        {
            var unionNode = data.GraphHostNode as UnionNode;
            if (unionNode == null)
                throw new InvalidCastException("Graph host node is not union node");
            if (IsStart)
            {
                var graphStore = data.NodeGraph;
                data.NodeGraph = unionNode.ParentGraph;
                var idx = Outputs.IndexOf(pin as PinOut);
                var tagPin = unionNode.Inputs[idx];
                var opPin = data.NodeGraph.GetOppositePin(tagPin);
                var opNode = data.NodeGraph.GetOppositePinNode(tagPin);
                UExpressionBase retValue = null;
                if (opNode != null && opPin != null)
                {
                    retValue = opNode.GetExpression(opPin, ref data);
                }
                data.NodeGraph = graphStore;
                return retValue;
            }
            else
            {
                var graphStore = data.NodeGraph;
                data.NodeGraph = this.ParentGraph;
                var inPin = pin as PinIn;
                var opPin = data.NodeGraph.GetOppositePin(inPin);
                var opNode = data.NodeGraph.GetOppositePinNode(inPin);
                UExpressionBase retValue = null;
                if(opNode != null && opPin != null)
                {
                    retValue = opNode.GetExpression(opPin, ref data);
                }
                data.NodeGraph = graphStore;
                return retValue;
            }
        }
        public override void BuildStatements(NodePin pin, ref BuildCodeStatementsData data)
        {
            if (IsStart)
            {
                var graphStore = data.NodeGraph;
                data.NodeGraph = this.ParentGraph;
                var pinOut = pin as PinOut;
                var opPin = data.NodeGraph.GetOppositePin(pinOut);
                var opNode = data.NodeGraph.GetOppositePinNode(pinOut);
                if(opNode != null)
                {
                    opNode.BuildStatements(opPin, ref data);
                }
                data.NodeGraph = graphStore;
            }
            else
            {
                var unionNode = data.GraphHostNode as UnionNode;
                if (unionNode == null)
                    throw new InvalidCastException("Graph host node is not union node");
                var graphStore = data.NodeGraph;
                data.NodeGraph = unionNode.ParentGraph;
                var idx = Inputs.IndexOf(pin as PinIn);
                var tagPin = unionNode.Outputs[idx];
                var opPin = data.NodeGraph.GetOppositePin(tagPin);
                var opNode = data.NodeGraph.GetOppositePinNode(tagPin);
                if(opNode != null && opPin != null)
                {
                    opNode.BuildStatements(opPin, ref data);
                }
                data.NodeGraph = graphStore;
            }
        }
    }

    public class UnionNode : UNodeBase, IUnionNode
    {
        [Rtti.Meta]
        public UNodeGraph ContentGraph { get; set; }
        [Rtti.Meta]
        public Guid InputNodeId { get; set; }
        [Rtti.Meta]
        public Guid OutputNodeId { get; set; }
        List<UNodePinDefineBase> mUserInputs = new List<UNodePinDefineBase>();
        [Rtti.Meta]
        public List<UNodePinDefineBase> UserInputs 
        {
            get => mUserInputs; 
            set
            {
                mUserInputs = value;
                UpdateInputs();
            }
        }
        List<UNodePinDefineBase> mUserOutputs = new List<UNodePinDefineBase>();
        [Rtti.Meta]
        public List<UNodePinDefineBase> UserOutputs
        {
            get => mUserOutputs;
            set
            {
                mUserOutputs = value;
                UpdateOutputs();
            }
        }
        public override void OnDoubleClick()
        {
            var render = ParentGraph.GetGraphRenderer();
            if (render != null)
            {
                ContentGraph.GraphName = Name;
                ContentGraph.ParentGraph = ParentGraph;
                render.SetGraph(ContentGraph);
            }
        }
        public void UpdateInputs()
        {
            for (int i = 0; i < Inputs.Count; i++)
            {
                ParentGraph.RemoveLinkedIn(Inputs[i]);
            }
            Inputs.Clear();
            for (int i = 0; i < mUserInputs.Count; i++)
            {
                var input = mUserInputs[i] as UnionPinDefine;
                var pin = new PinIn();
                pin.Name = input.Name;
                pin.Tag = input.Type;
                AddPinIn(pin);
            }
        }
        public void UpdateOutputs()
        {
            for (int i = 0; i < Outputs.Count; i++)
            {
                ParentGraph.RemoveLinkedOut(Outputs[i]);
            }
            Outputs.Clear();
            for (int i = 0; i < mUserOutputs.Count; i++)
            {
                var output = mUserOutputs[i] as UnionPinDefine;
                var pin = new PinOut();
                pin.Name = output.Name;
                pin.Tag = output.Type;
                AddPinOut(pin);
            }
        }

        public override UTypeDesc GetOutPinType(PinOut pin)
        {
            for (int i = 0; i < Outputs.Count; i++)
            {
                if (Outputs[i] == pin)
                {
                    return ((UnionPinDefine)mUserOutputs[i]).Type;
                }
            }
            return null;
        }
        public override UTypeDesc GetInPinType(PinIn pin)
        {
            for (int i = 0; i < Inputs.Count; i++)
            {
                if (Inputs[i] == pin)
                {
                    return ((UnionPinDefine)mUserInputs[i]).Type;
                }
            }
            return null;
        }
        public void UpdatePinWithDefine(NodePin pin, UNodePinDefineBase pinDef)
        {
            pin.Tag = ((UnionPinDefine)pinDef).Type;
        }

        public override void OnMouseStayPin(NodePin stayPin)
        {
            if (stayPin.Tag == null)
                return;

            var valueString = GetRuntimeValueString(stayPin.Name + "_" + (uint)NodeId.GetHashCode());
            var typeString = (stayPin.Tag as UTypeDesc).FullName;
            EGui.Controls.CtrlUtility.DrawHelper($"{valueString}({typeString})");
        }

        public override UExpressionBase GetExpression(NodePin pin, ref BuildCodeStatementsData data)
        {
            if(pin is PinIn)
            {
                var inPin = pin as PinIn;
                var idx = Inputs.IndexOf(inPin);
                var inputNode = ContentGraph.FindNode(InputNodeId);
                var inputNodePin = inputNode.Outputs[idx];
                return inputNode.GetExpression(inputNodePin, ref data);
            }
            else if(pin is PinOut)
            {
                var outPin = pin as PinOut;
                var idx = Outputs.IndexOf(outPin);
                var outputNode = ContentGraph.FindNode(OutputNodeId);
                var outputNodePin = outputNode.Inputs[idx];
                return outputNode.GetExpression(outputNodePin, ref data);
            }

            return null;
        }
        public override void BuildStatements(NodePin pin, ref BuildCodeStatementsData data)
        {
            var hostNodeStore = data.GraphHostNode;
            data.GraphHostNode = this;
            if(pin is PinIn)
            {
                var inPin = pin as PinIn;
                var idx = Inputs.IndexOf(inPin);
                var inputNode = ContentGraph.FindNode(InputNodeId);
                var inputNodePin = inputNode.Outputs[idx];
                inputNode.BuildStatements(inputNodePin, ref data);
            }
            else if(pin is PinOut)
            {
                var outPin = pin as PinOut;
                var idx = Outputs.IndexOf(outPin);
                var outputNode = ContentGraph.FindNode(OutputNodeId);
                var outputNodePin = outputNode.Inputs[idx];
                outputNode.BuildStatements(outputNodePin, ref data);
            }
            data.GraphHostNode = hostNodeStore;
        }
    }
}
