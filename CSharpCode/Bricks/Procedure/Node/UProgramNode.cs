using System;
using System.Collections.Generic;
using EngineNS.Bricks.NodeGraph;

namespace EngineNS.Bricks.Procedure.Node
{
    public class UNodePinDefine : NodeGraph.UNodePinDefineBase
    {
        [Rtti.Meta]
        public string Name { get; set; } = "UserPin";
        [Rtti.Meta]
        public string TypeValue { get; set; } = "Value";
        [Rtti.Meta]
        public UBufferCreator BufferCreator { get; } = UBufferCreator.CreateInstance<USuperBuffer<float, FFloatOperator>>(-1, - 1, -1);

        protected override void InitFromPin<T>(T pin)
        {
            Name = pin.Name;
            UBufferCreator.CopyTo(pin.Tag as UBufferCreator, BufferCreator);
            if (pin.LinkDesc.CanLinks.Count > 0)
                TypeValue = pin.LinkDesc.CanLinks[0];
        }
    }
    [Macross.UMacross]
    public partial class UProgram
    {
        [Rtti.Meta]
        public virtual bool InitProcedure(UPgcGraph graph, UProgramNode node)
        {
            return true;
        }
        [Rtti.Meta]
        public virtual bool OnProcedure(UPgcGraph graph, UProgramNode node)
        {
            //graph.BufferCache.FindBuffer()
            //var tmpBuffer = node.FindBuffer("AAA");
            //if (tmpBuffer != null)
            //{
            //    node.DispatchPixels(graph, tmpBuffer, "AAA");
            //}
            return true;
        }
        [Rtti.Meta]
        public unsafe virtual void OnPerPixel(UPgcGraph graph, UProgramNode node, 
            UBufferConponent resuilt, int x, int y, int z, object tag)
        {
            //resuilt.GetSuperPixelAddress(x, y, z);
            //resuilt.GetPixel<float>(x, y, z);
        }
    }

    [Bricks.CodeBuilder.ContextMenu("Program", "Program", UPgcGraph.PgcEditorKeyword)]
    public partial class UProgramNode : UPgcNodeBase
    {
        public UProgramNode()
        {
            PrevSize = new Vector2(100, 60);
            NodeDefine.HostNode = this;
            Icon.Size = new Vector2(25, 25);
            Icon.Color = 0xFFFF8000;
            TitleColor = 0xFF402020;
            BackColor = 0x80808080;

            UpdateInputOutputs();
        }
        public class UProgramNodeDefine
        {
            internal UProgramNode HostNode;
            public class UValueEditorAttribute : EGui.Controls.PropertyGrid.PGCustomValueEditorAttribute
            {
                public unsafe override bool OnDraw(in EditorInfo info, out object newValue)
                {
                    newValue = info.Value;
                    var nodeDef = newValue as UProgramNodeDefine;
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
                                nodeDef.HostNode.mUserInputs.Add(oldOutputs[i]);
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
        [UProgramNodeDefine.UValueEditor]
        public UProgramNodeDefine NodeDefine
        {
            get;
        } = new UProgramNodeDefine();
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
        public override void UpdateAMetaReferences(IO.IAssetMeta ameta)
        {
            ameta.AddReferenceAsset(ProgramName);
        }
        [Rtti.Meta]
        [RName.PGRName(FilterExts = CodeBuilder.UMacross.AssetExt, MacrossType = typeof(UProgram))]
        public RName ProgramName
        {
            get
            {
                if (mMcProgram == null)
                    return null;
                return mMcProgram.Name;
            }
            set
            {
                if (mMcProgram == null)
                {
                    mMcProgram = Macross.UMacrossGetter<UProgram>.NewInstance();
                }
                mMcProgram.Name = value;
            }
        }
        Macross.UMacrossGetter<UProgram> mMcProgram;
        public Macross.UMacrossGetter<UProgram> McProgram
        { 
            get
            {   
                return mMcProgram;
            }
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
        public unsafe override void OnPreviewDraw(in Vector2 prevStart, in Vector2 prevEnd, ImDrawList cmdlist)
        {
            //var ctrlPos = ParentGraph.CanvasToViewport(in prevStart);
            var ctrlPos = prevStart;
            ctrlPos -= ImGuiAPI.GetWindowPos();
            ImGuiAPI.SetCursorPos(in ctrlPos);
            ImGuiAPI.PushID($"{this.NodeId.ToString()}");
            if (ImGuiAPI.Button("OpenMacross"))
            {
                var mainEditor = UEngine.Instance.GfxDevice.SlateApplication as Editor.UMainEditorApplication;
                if (mainEditor != null)
                {
                    if (ProgramName != null)
                    {
                        var task = mainEditor.AssetEditorManager.OpenEditor(mainEditor, typeof(CodeBuilder.MacrossNode.UMacrossEditor), ProgramName, null);
                    }
                }
            }
            if (ProgramName != null)
            {
                var btSize = ImGuiAPI.GetItemRectSize();
                ctrlPos = prevStart;
                ctrlPos.Y += btSize.Y;
                var pureName = IO.FileManager.GetPureName(ProgramName.Name);
                cmdlist.AddText(in ctrlPos, 0xffffffff, pureName, null);
            }
            ImGuiAPI.PopID();
        }
        public override bool InitProcedure(UPgcGraph graph)
        {
            if (McProgram == null)
                return false;
            return McProgram.Get().InitProcedure(graph, this);
        }
        public override bool OnProcedure(UPgcGraph graph)
        {
            if (McProgram == null)
                return false;
            return McProgram.Get().OnProcedure(graph, this);
        }
        public void DispatchPixels(UPgcGraph graph, UBufferConponent result, object tag)
        {
            var prog = McProgram.Get();
            for (int i = 0; i < result.Depth; i++)
            {
                for (int j = 0; j < result.Height; j++)
                {
                    for (int k = 0; k < result.Width; k++)
                    {
                        prog.OnPerPixel(graph, this, result, k, j, i, tag);
                    }
                }
            }
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
    }
}