using System;
using System.Collections.Generic;
using System.ComponentModel;
using EngineNS.Bricks.NodeGraph;
using EngineNS.EGui.Controls;

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
        public override void OnMouseStayPin(NodePin stayPin, UNodeGraph graph)
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
    public class UUnionNode : UPgcNodeBase, IUnionNode, INodeWithContextMenu
    {
        [Rtti.Meta]
        [Browsable(false)]
        public UNodeGraph ContentGraph { get; set; }
        [Rtti.Meta]
        [Browsable(false)]
        public Guid InputNodeId { get; set; }
        [Rtti.Meta]
        [Browsable(false)]
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
        [Browsable(false), Rtti.Meta]
        public List<UnionNodePropertyData> PropertyDatas { get; set; } = new List<UnionNodePropertyData>();
        [Browsable(false)]
        public TtMenuItem ContextMenu { get; set; } = new TtMenuItem();

        public UUnionNode()
        {
            Icon.Size = new Vector2(25, 25);

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

        struct PropertyValueData
        {
            public string Name;
            public UNodeBase Node;
            public EGui.Controls.PropertyGrid.CustomPropertyDescriptor ProInfo;
            public bool IsPropertyCustomization;
        }
        Dictionary<string, PropertyValueData> mProValueDataDic = new Dictionary<string, PropertyValueData>();
        public override void GetProperties(ref EGui.Controls.PropertyGrid.CustomPropertyDescriptorCollection collection, bool parentIsValueType)
        {
            mProValueDataDic.Clear();
            var pros = TypeDescriptor.GetProperties(this);
            var objType = Rtti.TtTypeDesc.TypeOf(this.GetType());
            collection.InitValue(this, objType, pros, parentIsValueType);

            for (int i = 0; i < PropertyDatas.Count; i++)
            {
                var name = PropertyDatas[i].Name;
                var displayName = PropertyDatas[i].DisplayName;
                var node = ContentGraph.FindNode(PropertyDatas[i].NodeId);
                if (node == null)
                    continue;
                var proDesc = EGui.Controls.PropertyGrid.PropertyCollection.PropertyDescPool.QueryObjectSync();
                if (node is EGui.Controls.PropertyGrid.IPropertyCustomization)
                {
                    var tempProperties = EGui.Controls.PropertyGrid.PropertyCollection.PropertyDescCollectionPool.QueryObjectSync();
                    ((EGui.Controls.PropertyGrid.IPropertyCustomization)node).GetProperties(ref tempProperties, parentIsValueType);
                    bool find = false;
                    for (int proIdx = 0; proIdx < tempProperties.Count; proIdx++)
                    {
                        if (tempProperties[proIdx].Name == name)
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
                    proDesc.InitValue(node, Rtti.TtTypeDesc.TypeOf(nodeType), nodePro, parentIsValueType);
                }
                proDesc.DisplayName = displayName;
                proDesc.IsReadonly = PropertyDatas[i].ReadOnly;
                proDesc.Category = PropertyDatas[i].Category;
                var valData = new PropertyValueData()
                {
                    Name = name,
                    Node = node,
                    ProInfo = proDesc,
                    IsPropertyCustomization = (node is EGui.Controls.PropertyGrid.IPropertyCustomization)
                };
                mProValueDataDic[displayName] = valData;
                var proDescInThisNode = EGui.Controls.PropertyGrid.PropertyCollection.PropertyDescPool.QueryObjectSync();
                proDescInThisNode.CopyFrom(proDesc);
                proDescInThisNode.Name = displayName;
                collection.Add(proDescInThisNode);
            }
            IsPropertyVisibleDirty = false;
        }

        public override object GetPropertyValue(string propertyName)
        {
            if (mProValueDataDic.TryGetValue(propertyName, out var valData))
            {
                if (valData.IsPropertyCustomization)
                    return ((EGui.Controls.PropertyGrid.IPropertyCustomization)valData.Node).GetPropertyValue(valData.Name);
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

        public override void SetPropertyValue(string propertyName, object value)
        {
            if (mProValueDataDic.TryGetValue(propertyName, out var valData))
            {
                if (valData.IsPropertyCustomization)
                    ((EGui.Controls.PropertyGrid.IPropertyCustomization)valData.Node).SetPropertyValue(valData.Name, value);
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
                    Vector2i NumOfPin = new Vector2i(nodeDef.HostNode.UserInputs.Count, nodeDef.HostNode.UserOutputs.Count);
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
                    Profiler.Log.WriteLine<Profiler.TtPgcGategory>(Profiler.ELogTag.Warning, $"SubGraph({value}) is not match parentNode({this.ParentGraph.GraphName}:{this.Name})");
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
            ImGui.ImGuiFileDialog mFileDialog = TtEngine.Instance.EditorInstance.FileDialog.mFileDialog;

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
                var pureName = IO.TtFileManager.GetPureName(GraphName.Name);
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
                        var rootType = TtEngine.Instance.FileManager.GetRootDirType(file);
                        var rPath = IO.TtFileManager.GetRelativePath(TtEngine.Instance.FileManager.GetRoot(rootType), file);
                        if (rootType == IO.TtFileManager.ERootDir.Game)
                            mGraphName = RName.GetRName(rPath, RName.ERNameType.Game);
                        else if (rootType == IO.TtFileManager.ERootDir.Engine)
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
