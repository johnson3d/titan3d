using System;
using System.Collections.Generic;
using System.ComponentModel;
using EngineNS.Bricks.NodeGraph;

namespace EngineNS.Bricks.Procedure.Node
{
    public class UEndPointNode : UPgcNodeBase, IEndPointNode
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
        public UEndPointNode()
        {
            Icon.Size = new Vector2(25, 25);
        }
        public override void OnMouseStayPin(NodePin stayPin)
        {
            var creator = stayPin.Tag as UBufferCreator;
            if (creator != null)
            {
                EGui.Controls.CtrlUtility.DrawHelper($"{creator.ElementType.Name}");
            }
        }

        public void UpdateInputs()
        {
            for (int i = 0; i < Inputs.Count; i++)
            {
                ParentGraph.RemoveLinkedIn(Inputs[i]);
            }
            Inputs.Clear();
            for (int i = 0; i < UserInputs.Count; i++)
            {
                var input = UserInputs[i] as UNodePinDefine;
                var pin = new PinIn();
                AddInput(pin, input.Name, input.BufferCreator, input.TypeValue);
            }
        }
        public void UpdateOutputs()
        {
            for (int i = 0; i < Outputs.Count; i++)
            {
                ParentGraph.RemoveLinkedOut(Outputs[i]);
            }
            Outputs.Clear();
            for (int i = 0; i < UserOutputs.Count; i++)
            {
                var output = UserOutputs[i] as UNodePinDefine;
                var pin = new PinOut();
                AddOutput(pin, output.Name, output.BufferCreator, output.TypeValue);
            }
        }
        public void UpdatePinWithDefine(NodePin pin, UNodePinDefineBase pinDef)
        {
        }

        public override UBufferCreator GetOutBufferCreator(PinOut pin)
        {
            throw new NotImplementedException();
        }
    }
    public class UUnionNode : UPgcNodeBase, IUnionNode
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

        public UUnionNode()
        {
            Icon.Size = new Vector2(25, 25);
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
        public override bool InitProcedure(UPgcGraph graph)
        {
            var cGraph = ContentGraph as UPgcGraph;
            cGraph.GraphEditor = graph.GraphEditor;
            cGraph.BufferCache.ResetCache();

            //SureSubGraphInputOutputs();

            return true;
        }
        public void UpdateInputs()
        {
            for(int i = 0; i < Inputs.Count; i++)
            {
                ParentGraph.RemoveLinkedIn(Inputs[i]);
            }
            Inputs.Clear();
            for(int i=0; i< UserInputs.Count; i++)
            {
                var input = UserInputs[i] as UNodePinDefine;
                var pin = new PinIn();
                AddInput(pin, input.Name, input.BufferCreator, input.TypeValue);
            }
        }
        public void UpdateOutputs()
        {
            for(int i=0; i<Outputs.Count; i++)
            {
                ParentGraph.RemoveLinkedOut(Outputs[i]);
            }
            Outputs.Clear();
            for(int i=0; i<UserOutputs.Count; i++)
            {
                var output = UserOutputs[i] as UNodePinDefine;
                var pin = new PinOut();
                AddOutput(pin, output.Name, output.BufferCreator, output.TypeValue);
            }
        }
        public void UpdatePinWithDefine(NodePin pin, UNodePinDefineBase pinDef)
        {
        }
        public override UBufferCreator GetOutBufferCreator(PinOut pin)
        {
            throw new NotImplementedException();
        }
    }


    [Bricks.CodeBuilder.ContextMenu("SubGraph", "SubGraph", UPgcGraph.PgcEditorKeyword)]
    public partial class USubGraphNode : UPgcNodeBase
    {
        public USubGraphNode()
        {
            PrevSize = new Vector2(100, 60);
            NodeDefine.HostNode = this;
            Icon.Size = new Vector2(25, 25);
            Icon.Color = 0xFFFF8000;
            TitleColor = 0xFF402020;
            BackColor = 0x80808080;

            UpdateInputOutputs();
        }
        public class USubGraphNodeDefine
        {
            internal USubGraphNode HostNode;
            public class UValueEditorAttribute : EGui.Controls.PropertyGrid.PGCustomValueEditorAttribute
            {
                public unsafe override bool OnDraw(in EditorInfo info, out object newValue)
                {
                    newValue = info.Value;
                    var nodeDef = newValue as USubGraphNodeDefine;
                    Int32_2 NumOfPin = new Int32_2(nodeDef.HostNode.UserInputs.Count, nodeDef.HostNode.UserOutputs.Count);
                    if (ImGuiAPI.InputInt2("NumOfPin", (int*)&NumOfPin, ImGuiInputTextFlags_.ImGuiInputTextFlags_None))
                    {
                        if (NumOfPin.X <= 0)
                            NumOfPin.X = 1;
                        if (NumOfPin.Y <= 0)
                            NumOfPin.Y = 1;
                        var oldInputs = nodeDef.HostNode.mUserInputs;
                        var oldOutputs = nodeDef.HostNode.mUserOutputs;
                        nodeDef.HostNode.mUserInputs = new List<UNodePinDefine>();
                        nodeDef.HostNode.mUserOutputs = new List<UNodePinDefine>();
                        for (int i = 0; i < NumOfPin.X; i++)
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
                        for (int i = 0; i < NumOfPin.Y; i++)
                        {
                            if (i < oldOutputs.Count)
                            {
                                nodeDef.HostNode.mUserOutputs.Add(oldOutputs[i]);
                            }
                            else
                            {
                                nodeDef.HostNode.mUserOutputs.Add(new UNodePinDefine());
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
        [USubGraphNodeDefine.UValueEditor]
        public USubGraphNodeDefine NodeDefine
        {
            get;
        } = new USubGraphNodeDefine();
        List<UNodePinDefine> mUserInputs = new List<UNodePinDefine>();
        [Rtti.Meta]
        public List<UNodePinDefine> UserInputs
        {
            get => mUserInputs;
        }
        List<UNodePinDefine> mUserOutputs = new List<UNodePinDefine>();
        [Rtti.Meta]
        public List<UNodePinDefine> UserOutputs
        {
            get => mUserOutputs;
        }
        [Rtti.Meta(Order = 1)]
        public bool SerializeSignal
        {
            get => true;
            set
            {
                UpdateInputOutputs();
            }
        }
        RName mGraphName;
        [Rtti.Meta]
        [RName.PGRName(FilterExts = UPgcAsset.AssetExt)]
        public RName GraphName
        {
            get => mGraphName;
            set
            {
                mGraphName = value;
                GraphAsset = UPgcAsset.LoadAsset(value);
                if (false == SureSubGraphInputOutputs())
                {
                    Profiler.Log.WriteLine(Profiler.ELogTag.Warning, "Pgc", $"SubGraph({value}) is not match parentNode({this.ParentGraph.GraphName}:{this.Name})");
                }
            }
        }
        public UPgcAsset GraphAsset = new UPgcAsset();        
        private bool SureSubGraphInputOutputs()
        {
            bool isMatch = true;
            var subInputs = GraphAsset.AssetGraph.FindFirstNode("SubGraphInputs") as UEndingNode;
            if (subInputs == null)
            {
                subInputs = new UEndingNode();
                subInputs.Name = "SubGraphInputs";
                subInputs.Position = new Vector2(0, 0);
                GraphAsset.AssetGraph.AddNode(subInputs);
                isMatch = false;
            }

            for (int i = 0; i < UserInputs.Count; i++)
            {
                var userInput = subInputs.FindUserInput(UserInputs[i].Name);
                if (userInput == null)
                {
                    userInput = new UNodePinDefine();
                    userInput.Name = UserInputs[i].Name;
                    subInputs.UserInputs.Add(userInput);
                    isMatch = false;
                }
                if (userInput.TypeValue != UserInputs[i].TypeValue)
                {
                    userInput.TypeValue = UserInputs[i].TypeValue;
                    isMatch = false;
                }
                if (userInput.BufferCreator.BufferType != UserInputs[i].BufferCreator.BufferType)
                {
                    userInput.BufferCreator.BufferType = UserInputs[i].BufferCreator.BufferType;
                    isMatch = false;
                }
                userInput.BufferCreator.SetSize(UserInputs[i].BufferCreator);
            }
            
            var subOutputs = GraphAsset.AssetGraph.FindFirstNode("SubGraphOutputs") as UEndingNode;
            if (subOutputs == null)
            {
                subOutputs = new UEndingNode();
                subOutputs.Name = "SubGraphOutputs";
                subOutputs.Position = new Vector2(600, 600);
                GraphAsset.AssetGraph.AddNode(subOutputs);
                isMatch = false;
            }

            for (int i = 0; i < UserOutputs.Count; i++)
            {
                var userOutput = subOutputs.FindUserInput(UserOutputs[i].Name);
                if (userOutput == null)
                {
                    userOutput = new UNodePinDefine();
                    userOutput.Name = UserOutputs[i].Name;
                    subOutputs.UserInputs.Add(userOutput);
                    isMatch = false;
                }
                if (userOutput.TypeValue != UserOutputs[i].TypeValue)
                {
                    userOutput.TypeValue = UserOutputs[i].TypeValue;
                    isMatch = false;
                }
                if (userOutput.BufferCreator.BufferType != UserOutputs[i].BufferCreator.BufferType)
                {
                    userOutput.BufferCreator.BufferType = UserOutputs[i].BufferCreator.BufferType;
                    isMatch = false;
                }
                userOutput.BufferCreator.SetSize(UserOutputs[i].BufferCreator);
            }
            if (isMatch == false)
            {
                subInputs.UpdateInputOutputs();
                subOutputs.UpdateInputOutputs();
            }

            return isMatch;
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
                var pin = new PinIn();
                AddInput(pin, i.Name, i.BufferCreator, i.TypeValue);
            }
            foreach (var i in mUserOutputs)
            {
                var pin = new PinOut();
                AddOutput(pin, i.Name, i.BufferCreator, i.TypeValue);
            }
            OnPositionChanged();
        }
        public override UBufferCreator GetOutBufferCreator(PinOut pin)
        {
            for (int i = 0; i < Outputs.Count; i++)
            {
                if (pin == Outputs[i])
                {
                    return UserOutputs[i].BufferCreator;
                }
            }
            return null;
        }
        public override void OnDoubleClick()
        {
            if (GraphAsset != null)
            {
                var graph = this.ParentGraph as UPgcGraph;
                GraphAsset.AssetGraph.ParentGraph = this.ParentGraph;
                GraphAsset.AssetGraph.GraphName = this.Name;
                this.SureSubGraphInputOutputs();
                graph.GraphEditor.GraphRenderer.SetGraph(GraphAsset.AssetGraph);
            }
        }
        public unsafe override void OnPreviewDraw(in Vector2 prevStart, in Vector2 prevEnd, ImDrawList cmdlist)
        {
            ImGui.ImGuiFileDialog mFileDialog = UEngine.Instance.EditorInstance.FileDialog.mFileDialog;

            var ctrlPos = prevStart;
            ctrlPos -= ImGuiAPI.GetWindowPos();
            ImGuiAPI.SetCursorPos(in ctrlPos);
            ImGuiAPI.PushID($"{this.NodeId.ToString()}");
            if (ImGuiAPI.Button("OpenSubGraph"))
            {
                if (GraphAsset != null)
                {
                    var graph = this.ParentGraph as UPgcGraph;
                    GraphAsset.AssetGraph.ParentGraph = this.ParentGraph;
                    GraphAsset.AssetGraph.GraphName = this.Name;
                    this.SureSubGraphInputOutputs();
                    graph.GraphEditor.GraphRenderer.SetGraph(GraphAsset.AssetGraph);
                }
            }
            var btSize = ImGuiAPI.GetItemRectSize();
            ctrlPos.Y += btSize.Y;
            ImGuiAPI.SetCursorPos(in ctrlPos);
            if (ImGuiAPI.Button("SaveSubGraph"))
            {
                if (GraphAsset != null)
                {
                    if (GraphName == null)
                    {
                        mFileDialog.OpenModal("ChooseFileDlgKey", "Choose File", ".pgc", ".");
                    }
                    else
                    {
                        GraphAsset.SaveAssetTo(GraphName);
                    }
                }
            }
            btSize = ImGuiAPI.GetItemRectSize();
            ctrlPos.Y += btSize.Y;
            if (GraphName != null)
            {
                var pureName = IO.FileManager.GetPureName(GraphName.Name);
                ImGuiAPI.SetCursorPos(in ctrlPos);
                ImGuiAPI.Text(pureName);
            }
            ImGuiAPI.PopID();

            if (mFileDialog.DisplayDialog("ChooseFileDlgKey"))
            {
                if (mFileDialog.IsOk() == true)
                {
                    if (GraphName == null)
                    {
                        var file = mFileDialog.GetFilePathName();
                        var rootType = UEngine.Instance.FileManager.GetRootDirType(file);
                        var rPath = IO.FileManager.GetRelativePath(UEngine.Instance.FileManager.GetRoot(rootType), file);
                        if (rootType == IO.FileManager.ERootDir.Game)
                            mGraphName = RName.GetRName(rPath, RName.ERNameType.Game);
                        else if (rootType == IO.FileManager.ERootDir.Engine)
                            mGraphName = RName.GetRName(rPath, RName.ERNameType.Engine);

                        GraphAsset.AssetName = GraphName;
                        GraphAsset.SaveAssetTo(GraphName);
                    }
                }
                mFileDialog.CloseDialog();
            }
        }
        public override bool InitProcedure(UPgcGraph graph)
        {
            GraphAsset.AssetGraph.GraphEditor = graph.GraphEditor;

            GraphAsset.AssetGraph.BufferCache.ResetCache();

            SureSubGraphInputOutputs();

            return true;
        }
        public override bool OnProcedure(UPgcGraph graph)
        {
            var subInputs = GraphAsset.AssetGraph.FindFirstNode("SubGraphInputs") as UEndingNode;
            for (int i = 0; i < Inputs.Count; i++)
            {
                var buffer = graph.BufferCache.FindBuffer(Inputs[i]);
                var pin = subInputs.FindPinOut(Inputs[i].Name);
                GraphAsset.AssetGraph.BufferCache.RegBuffer(pin, buffer);
            }
            var subOutputs = GraphAsset.AssetGraph.FindFirstNode("SubGraphOutputs") as UEndingNode;
            GraphAsset.AssetGraph.Compile(subOutputs, false);
            for (int i = 0; i < Outputs.Count; i++)
            {
                var buffer = subOutputs.FindBuffer(Outputs[i].Name);
                graph.BufferCache.RegBuffer(Outputs[i], buffer);
            }
            GraphAsset.AssetGraph.BufferCache.ResetCache();

            return true;
        }
    }
}
