using System;
using System.Collections.Generic;
using System.ComponentModel;
using EngineNS.Bricks.NodeGraph;

namespace EngineNS.Bricks.Procedure.Node
{
    [Bricks.CodeBuilder.ContextMenu("RootNode", "RootNode", UPgcGraph.PgcEditorKeyword)]
    public class UEndingNode : UPgcNodeBase
    {
        public UEndingNode()
        {
            Icon.Size = new Vector2(25, 25);
            Icon.Color = 0xFF00FF00;
            TitleColor = 0xFF204020;
            BackColor = 0x80808080;
            //PinNumber = 4;

            NodeDefine.HostNode = this;
            UpdateInputOutputs();
        }
        public class UEndingNodeDefine
        {
            internal UEndingNode HostNode;
            public class UValueEditorAttribute : EGui.Controls.PropertyGrid.PGCustomValueEditorAttribute
            {
                public unsafe override bool OnDraw(in EditorInfo info, out object newValue)
                {
                    newValue = info.Value;
                    var nodeDef = newValue as UEndingNodeDefine;
                    var NumOfPin = nodeDef.HostNode.UserInputs.Count;
                    if (ImGuiAPI.InputInt("NumOfPin", (int*)&NumOfPin, 0, 0, ImGuiInputTextFlags_.ImGuiInputTextFlags_None))
                    {
                        if (NumOfPin <= 0)
                            NumOfPin = 1;
                        var oldInputs = nodeDef.HostNode.mUserInputs;
                        nodeDef.HostNode.mUserInputs = new List<UNodePinDefine>();
                        for (int i = 0; i < NumOfPin; i++)
                        {
                            if (i < oldInputs.Count)
                            {
                                nodeDef.HostNode.mUserInputs.Add(oldInputs[i]);
                            }
                            else
                            {
                                nodeDef.HostNode.mUserInputs.Add(new UNodePinDefine());
                            }
                        }
                    }
                    if (ImGuiAPI.Button("UpdatePins"))
                    {
                        nodeDef.HostNode.UpdateInputOutputs();
                    }
                    return false;
                }
            }
        }
        [UEndingNodeDefine.UValueEditor]
        public UEndingNodeDefine NodeDefine
        {
            get;
        } = new UEndingNodeDefine();
        List<UNodePinDefine> mUserInputs = new List<UNodePinDefine>();
        [Rtti.Meta]
        public List<UNodePinDefine> UserInputs
        {
            get => mUserInputs;
            set
            {
                mUserInputs = value;
                UpdateInputOutputs();
            }
        }
        public UNodePinDefine FindUserInput(string name)
        {
            foreach(var i in mUserInputs)
            {
                if (i.Name == name)
                    return i;
            }
            return null;
        }
        public void UpdateInputOutputs()
        {
            foreach (var i in Inputs)
            {
                ParentGraph.RemoveLinkedIn(i);
            }
            Inputs.Clear();
            foreach (var i in Outputs)
            {
                ParentGraph.RemoveLinkedOut(i);
            }
            Outputs.Clear();
            foreach (var i in mUserInputs)
            {
                {
                    var pin = new PinIn();
                    AddInput(pin, i.Name, i.BufferCreator, i.TypeValue);
                }
                {
                    var pin = new PinOut();
                    AddOutput(pin, i.Name, i.BufferCreator, i.TypeValue);
                }
            }
            OnPositionChanged();
        }
        public override UBufferCreator GetOutBufferCreator(PinOut pin)
        {
            return null;
        }
        public override bool IsMatchLinkedPin(UBufferCreator input, UBufferCreator output)
        {
            return true;
        }
        public override void OnLoadLinker(UPinLinker linker)
        {
            base.OnLoadLinker(linker);

            var input = linker.OutPin.Tag as UBufferCreator;
            if(input != null)
                (linker.InPin.Tag as UBufferCreator).BufferType = input.BufferType;
        }
        public override void OnLinkedFrom(PinIn iPin, UNodeBase OutNode, PinOut oPin)
        {
            base.OnLinkedFrom(iPin, OutNode, oPin);

            var oPT = oPin.Tag as UBufferCreator;
            if(oPT != null)
                (iPin.Tag as UBufferCreator).BufferType = oPT.BufferType;
        }
        public UBufferComponent GetResultBuffer(string pinName)
        {
            var pin = this.FindPinIn(pinName);
            if (pin == null)
                return null;

            var graph = ParentGraph as UPgcGraph;
            return graph.BufferCache.FindBuffer(pin);
        }
        public override bool OnProcedure(UPgcGraph graph)
        {
            //for (int i = 0; i < Inputs.Count; i++)
            //{
            //    var buffer = GetResultBuffer(i);
            //    if (buffer != null)
            //    {
            //        buffer.LifeCount--;
            //    }
            //}
            return true;
        }
    }
}
