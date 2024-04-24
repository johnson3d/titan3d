using EngineNS.Bricks.NodeGraph;
using EngineNS.Bricks.WorldSimulator;
using EngineNS.EGui.Controls;
using EngineNS.EGui.Controls.PropertyGrid;
using EngineNS.Rtti;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text;

namespace EngineNS.Bricks.CodeBuilder.MacrossNode
{
    public class UnionPinDefine : UNodePinDefineBase
    {
        [Rtti.Meta]
        [EGui.Controls.PropertyGrid.PGTypeEditor()]
        public UTypeDesc Type { get; set; } = UTypeDesc.TypeOf(typeof(int));
        [Rtti.Meta]
        [Browsable(false)]
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
        public UnionNode HostUnion;

        [Rtti.Meta]
        [Browsable(false)]
        public bool IsStart { get; set; }
        List<UNodePinDefineBase> mUserInputs = new List<UNodePinDefineBase>();
        [Rtti.Meta]
        [Browsable(false)]
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
        [Browsable(false)]
        public List<UNodePinDefineBase> UserOutputs
        {
            get => mUserOutputs;
            set
            {
                mUserOutputs = value;
                UpdateOutputs();
            }
        }
        List<PinIn> mTempPinIns = new List<PinIn>();
        public void UpdateInputs()
        {
            mTempPinIns.Clear();
            for (int i = 0; i < mUserInputs.Count; i++)
            {
                var def = mUserInputs[i] as UnionPinDefine;
                PinIn pin = null;
                for (int inputIdx = Inputs.Count - 1; inputIdx >= 0; inputIdx--)
                {
                    if ((Inputs[inputIdx].Name == def.Name) && (Inputs[inputIdx].Tag == def.Type))
                    {
                        pin = Inputs[inputIdx];
                        Inputs.RemoveAt(inputIdx);
                        break;
                    }
                }
                if (pin == null)
                {
                    pin = new PinIn();
                    pin.Name = def.Name;
                    pin.Tag = def.Type;
                }
                mTempPinIns.Add(pin);
            }
            for (int i = 0; i < Inputs.Count; i++)
            {
                ParentGraph.RemoveLinkedIn(Inputs[i]);
                Inputs[i].HostNode = null;
            }
            Inputs.Clear();
            for (int i = 0; i < mTempPinIns.Count; i++)
            {
                AddPinIn(mTempPinIns[i]);
            }

            LayoutDirty = true;
        }
        List<PinOut> mTempPinOuts = new List<PinOut>();
        public void UpdateOutputs()
        {
            mTempPinOuts.Clear();
            for (int i = 0; i < mUserOutputs.Count; i++)
            {
                var def = mUserOutputs[i] as UnionPinDefine;
                PinOut pin = null;
                for (int idx = Outputs.Count - 1; idx >= 0; idx--)
                {
                    if ((Outputs[idx].Name == def.Name) && (Outputs[idx].Tag == def.Type))
                    {
                        pin = Outputs[idx];
                        Outputs.RemoveAt(idx);
                        break;
                    }
                }
                if (pin == null)
                {
                    pin = new PinOut();
                    pin.Name = def.Name;
                    pin.Tag = def.Type;
                }
                mTempPinOuts.Add(pin);
            }
            for (int i = 0; i < Outputs.Count; i++)
            {
                ParentGraph.RemoveLinkedOut(Outputs[i]);
                Outputs[i].HostNode = null;
            }
            Outputs.Clear();
            for (int i = 0; i < mTempPinOuts.Count; i++)
            {
                AddPinOut(mTempPinOuts[i]);
            }

            LayoutDirty = true;
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

        public override void OnMouseStayPin(NodePin stayPin, UNodeGraph graph)
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

        public override object GetPropertyEditObject()
        {
            return HostUnion;
        }
    }

    public class UnionNode : UNodeBase, IUnionNode, INodeWithContextMenu
    {
        [Rtti.Meta, Browsable(false)]
        public UNodeGraph ContentGraph { get; set; }
        [Rtti.Meta, Browsable(false)]
        public Guid InputNodeId { get; set; }
        [Rtti.Meta, Browsable(false)]
        public Guid OutputNodeId { get; set; }
        List<UNodePinDefineBase> mUserInputs = new List<UNodePinDefineBase>();
        [Rtti.Meta]
        [EGui.Controls.PropertyGrid.PGBaseType(typeof(UnionPinDefine))]
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
        [EGui.Controls.PropertyGrid.PGBaseType(typeof(UnionPinDefine))]
        public List<UNodePinDefineBase> UserOutputs
        {
            get => mUserOutputs;
            set
            {
                mUserOutputs = value;
                UpdateOutputs();
            }
        }
        [Browsable(false)]
        public TtMenuItem ContextMenu { get; set; } = new TtMenuItem();
        [Browsable(false), Rtti.Meta]
        public List<UnionNodePropertyData> PropertyDatas { get; set; } = new List<UnionNodePropertyData>();

        [Browsable(false)]
        public bool IsPropertyVisibleDirty { get; set; } = false;

        public UnionNode()
        {
            ContextMenu.AddMenuItem("Config", null,
                (TtMenuItem item, object sender) =>
                {
                    this.ParentGraph.SetConfigUnionNode(this);
                }, null);
        }
        public override void OnDoubleClick()
        {
            var render = ParentGraph.GetGraphRenderer();
            if (render != null)
            {
                ContentGraph.GraphName = Name;
                ContentGraph.ParentGraph = ParentGraph;

                var inputNode = ContentGraph.FindNode(InputNodeId) as EndPointNode;
                if (inputNode != null)
                    inputNode.HostUnion = this;
                var outputNode = ContentGraph.FindNode(OutputNodeId) as EndPointNode;
                if(outputNode != null)
                    outputNode.HostUnion = this;

                render.SetGraph(ContentGraph);
            }
        }
        string GetInputNameErrorString(in EGui.Controls.PropertyGrid.PGCustomValueEditorAttribute.EditorInfo info, UNodePinDefineBase dec, object newValue)
        {
            var newName = (string)newValue;
            for (int i = 0; i < mUserInputs.Count; i++)
            {
                if ((mUserInputs[i].Name == newName) && (mUserInputs[i] != dec))
                    return "Same name with input " + i;
            }
            return null;
        }
        string GetOutputNameErrorString(in EGui.Controls.PropertyGrid.PGCustomValueEditorAttribute.EditorInfo info, UNodePinDefineBase dec, object newValue)
        {
            var newName = (string)newValue;
            for (int i = 0; i < mUserOutputs.Count; i++)
            {
                if ((mUserOutputs[i].Name == newName) && (mUserOutputs[i] != dec))
                    return "Same name with output " + i;
            }
            return null;
        }
        List<PinIn> mTempPinIns = new List<PinIn>();
        public void UpdateInputs()
        {
            mTempPinIns.Clear();
            for(int i=0; i<mUserInputs.Count; i++)
            {
                if (mUserInputs[i].Name == "UserPin")
                {
                    var idx = 0;
                    while(true)
                    {
                        bool find = false;
                        var tempName = "UserPin" + idx;
                        for(int inputIdx = 0; inputIdx < Inputs.Count; inputIdx++)
                        {
                            if (Inputs[inputIdx].Name == tempName)
                            {
                                find = true;
                                idx++;
                                break;
                            }
                        }
                        if(!find)
                        {
                            mUserInputs[i].Name = tempName;
                            break;
                        }
                    }
                }
            }
            for (int i=0; i<mUserInputs.Count; i++)
            {
                var def = mUserInputs[i] as UnionPinDefine;
                def.GetErrorStringAction = GetInputNameErrorString;
                PinIn pin = null;
                for(int inputIdx = Inputs.Count - 1; inputIdx >= 0; inputIdx--)
                {
                    if ((Inputs[inputIdx].Name == def.Name) && (Inputs[inputIdx].Tag == def.Type))
                    {
                        pin = Inputs[inputIdx];
                        Inputs.RemoveAt(inputIdx);
                        break;
                    }
                }
                if(pin == null)
                {
                    pin = new PinIn();
                    pin.Name = def.Name;
                    pin.Tag = def.Type;
                }
                mTempPinIns.Add(pin);
            }
            for(int i=0; i<Inputs.Count; i++)
            {
                ParentGraph.RemoveLinkedIn(Inputs[i]);
                Inputs[i].HostNode = null;
            }
            Inputs.Clear();
            for(int i=0; i<mTempPinIns.Count; i++)
            {
                AddPinIn(mTempPinIns[i]);
            }

            var inputNode = ContentGraph.FindNode(InputNodeId) as EndPointNode;
            if (inputNode != null)
            {
                inputNode.UserOutputs = mUserInputs;
            }

            LayoutDirty = true;
        }
        List<PinOut> mTempPinOuts = new List<PinOut>();
        public void UpdateOutputs()
        {
            mTempPinOuts.Clear();
            for (int i = 0; i < mUserOutputs.Count; i++)
            {
                if (mUserOutputs[i].Name == "UserPin")
                {
                    var idx = 0;
                    while (true)
                    {
                        bool find = false;
                        var tempName = "UserPin" + idx;
                        for (int outputIdx = 0; outputIdx < Outputs.Count; outputIdx++)
                        {
                            if (Outputs[outputIdx].Name == tempName)
                            {
                                find = true;
                                idx++;
                                break;
                            }
                        }
                        if (!find)
                        {
                            mUserOutputs[i].Name = tempName;
                            break;
                        }
                    }
                }
            }
            for (int i=0; i<mUserOutputs.Count; i++)
            {
                var def = mUserOutputs[i] as UnionPinDefine;
                def.GetErrorStringAction = GetOutputNameErrorString;
                PinOut pin = null;
                for(int idx = Outputs.Count - 1; idx >= 0; idx--)
                {
                    if ((Outputs[idx].Name == def.Name) && (Outputs[idx].Tag == def.Type))
                    {
                        pin = Outputs[idx];
                        Outputs.RemoveAt(idx);
                        break;
                    }
                }
                if(pin == null)
                {
                    pin = new PinOut();
                    pin.Name = def.Name;
                    pin.Tag = def.Type;
                }
                mTempPinOuts.Add(pin);
            }
            for(int i=0; i<Outputs.Count; i++)
            {
                ParentGraph.RemoveLinkedOut(Outputs[i]);
                Outputs[i].HostNode = null;
            }
            Outputs.Clear();
            for(int i=0; i<mTempPinOuts.Count; i++)
            {
                AddPinOut(mTempPinOuts[i]);
            }

            var outputNode = ContentGraph.FindNode(OutputNodeId) as EndPointNode;
            if(outputNode != null)
            {
                outputNode.UserInputs = mUserOutputs;
            }

            LayoutDirty = true;
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

        public override void OnMouseStayPin(NodePin stayPin, UNodeGraph graph)
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

        struct PropertyValueData
        {
            public string Name;
            public UNodeBase Node;
            public CustomPropertyDescriptor ProInfo;
            public bool IsPropertyCustomization;
        }
        Dictionary<string, PropertyValueData> mProValueDataDic = new Dictionary<string, PropertyValueData>();
        public void GetProperties(ref CustomPropertyDescriptorCollection collection, bool parentIsValueType)
        {
            mProValueDataDic.Clear();
            var pros = TypeDescriptor.GetProperties(this);
            var objType = Rtti.UTypeDesc.TypeOf(this.GetType());
            collection.InitValue(this, objType, pros, parentIsValueType);

            for(int i=0; i< PropertyDatas.Count; i++)
            {
                var name = PropertyDatas[i].Name;
                var displayName = PropertyDatas[i].DisplayName;
                var node = ContentGraph.FindNode(PropertyDatas[i].NodeId);
                if (node == null)
                    continue;
                var proDesc = PropertyCollection.PropertyDescPool.QueryObjectSync();
                if(node is IPropertyCustomization)
                {
                    var tempProperties = PropertyCollection.PropertyDescCollectionPool.QueryObjectSync();
                    ((IPropertyCustomization)node).GetProperties(ref tempProperties, parentIsValueType);
                    bool find = false;
                    for(int proIdx = 0; proIdx < tempProperties.Count; proIdx++)
                    {
                        if(tempProperties[proIdx].Name == name)
                        {
                            proDesc = tempProperties[proIdx];
                            find = true;
                            break;
                        }
                    }
                    if (!find)
                        continue;
                }
                else
                {
                    var nodeType = node.GetType();
                    var nodePros = TypeDescriptor.GetProperties(node);
                    var nodePro = nodePros[name];
                    if (nodePro == null)
                        continue;
                    proDesc.InitValue(node, Rtti.UTypeDesc.TypeOf(nodeType), nodePro, parentIsValueType);
                }
                proDesc.DisplayName = displayName;
                proDesc.IsReadonly = PropertyDatas[i].ReadOnly;
                proDesc.Category = PropertyDatas[i].Category;
                var valData = new PropertyValueData()
                {
                    Name = name,
                    Node = node,
                    ProInfo = proDesc,
                    IsPropertyCustomization = (node is IPropertyCustomization)
                };
                mProValueDataDic[displayName] = valData;
                var proDescInThisNode = PropertyCollection.PropertyDescPool.QueryObjectSync();
                proDescInThisNode.CopyFrom(proDesc);
                proDescInThisNode.Name = displayName;
                collection.Add(proDescInThisNode);
            }
            IsPropertyVisibleDirty = false;
        }

        public object GetPropertyValue(string propertyName)
        {
            if (mProValueDataDic.TryGetValue(propertyName, out var valData))
            {
                if (valData.IsPropertyCustomization)
                    return ((IPropertyCustomization)valData.Node).GetPropertyValue(valData.Name);
                else
                    return valData.ProInfo.GetValue(valData.Node);
            }
            else
            {
                var proInfo = GetType().GetProperty(propertyName);
                if (proInfo != null)
                    return proInfo.GetValue(this);
            }

            return null;
        }

        public void SetPropertyValue(string propertyName, object value)
        {
            if(mProValueDataDic.TryGetValue(propertyName, out var valData))
            {
                if(valData.IsPropertyCustomization)
                    ((IPropertyCustomization)valData.Node).SetPropertyValue(valData.Name, value);
                else
                {
                    var obj = (object)(valData.Node);
                    valData.ProInfo.SetValue(ref obj, value);
                    valData.Node = (UNodeBase)obj;
                }
            }
            else
            {
                var proInfo = GetType().GetProperty(propertyName);
                if (proInfo != null)
                    proInfo.SetValue(this, value);
            }
        }
    }
}
