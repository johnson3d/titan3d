using System;
using System.Collections.Generic;
using EngineNS.Bricks.NodeGraph;

namespace EngineNS.Bricks.Procedure.Node
{
    public class UUserNodeDefine
    {
        internal UUserNode HostNode;
        public class UValueEditorAttribute : EGui.Controls.PropertyGrid.PGCustomValueEditorAttribute
        {
            string mPinName = "";
            public override bool OnDraw(in EditorInfo info, out object newValue)
            {
                newValue = info.Value;
                var nodeDef = newValue as UUserNodeDefine;
                ImGuiAPI.InputText("Name", ref mPinName);
                if (ImGuiAPI.Button("ModifyInput"))
                {
                    var pin = nodeDef.HostNode.FindPinIn(mPinName);
                    if (pin == null)
                    {
                        pin = new PinIn();
                        pin.Name = mPinName;
                        pin.Link = UPgcEditorStyles.Instance.NewInOutPinDesc();
                        nodeDef.HostNode.AddPinIn(pin);
                    }
                    else
                    {
                        nodeDef.HostNode.ParentGraph.RemoveLinkedIn(pin);
                        nodeDef.HostNode.RemovePinIn(pin);
                    }
                    nodeDef.HostNode.OnPositionChanged();
                }
                if (ImGuiAPI.Button("ModifyOutput"))
                {
                    var pin = nodeDef.HostNode.FindPinOut(mPinName);
                    if (pin == null)
                    {
                        pin = new PinOut();
                        pin.Name = mPinName;
                        pin.Link = UPgcEditorStyles.Instance.NewInOutPinDesc();
                        nodeDef.HostNode.AddPinOut(pin);
                    }
                    else
                    {
                        nodeDef.HostNode.ParentGraph.RemoveLinkedOut(pin);
                        nodeDef.HostNode.RemovePinOut(pin);
                    }
                    nodeDef.HostNode.OnPositionChanged();
                }
                return false;
            }
        }
    }
    public partial class UUserNode : UOpNode
    {
        public UUserNode()
        {
            NodeDefine.HostNode = this;

            Icon.Size = new Vector2(25, 25);
            Icon.Color = 0xFFFF8000;
            TitleColor = 0xFF402020;
            BackColor = 0x80808080;
        }
        [UUserNodeDefine.UValueEditor]
        public UUserNodeDefine NodeDefine
        {
            get;
        } = new UUserNodeDefine();
        [Rtti.Meta]
        public List<string> UserInputs
        {
            get
            {
                var result = new List<string>();
                foreach(var i in Inputs)
                {
                    result.Add(i.Name);
                }
                return result;
            }
            set
            {
                foreach(var i in Inputs)
                {
                    ParentGraph.RemoveLinkedIn(i);
                }
                Inputs.Clear();
                foreach(var i in value)
                {
                    var pin = new PinIn();
                    pin.Name = i;
                    pin.Link = UPgcEditorStyles.Instance.NewInOutPinDesc();
                    AddPinIn(pin);
                }
                this.OnPositionChanged();
            }
        }
        [Rtti.Meta]
        public List<string> UserOutputs
        {
            get
            {
                var result = new List<string>();
                foreach (var i in Outputs)
                {
                    result.Add(i.Name);
                }
                return result;
            }
            set
            {
                foreach (var i in Outputs)
                {
                    ParentGraph.RemoveLinkedOut(i);
                }
                Outputs.Clear();
                foreach (var i in value)
                {
                    var pin = new PinOut();
                    pin.Name = i;
                    pin.Link = UPgcEditorStyles.Instance.NewInOutPinDesc();
                    AddPinOut(pin);
                }
                this.OnPositionChanged();
            }
        }
        
        public override bool OnProcedure(UPgcGraph graph)
        {
            var bzNode = GetInputNode(graph, 0) as UBezier;
            var resultComp = graph.BufferCache.FindBuffer(Outputs[0]);
            var rangeX = (bzNode.MaxX - bzNode.MinX);
            for (int i = 0; i< resultComp.Height; i++)
            {
                var vzValueH = BezierCalculate.ValueOnBezier(bzNode.BzPoints, ((double)i) * rangeX / (double)resultComp.Width);
                for (int j = 0; j < resultComp.Width; j++)
                {
                    var vzValue = BezierCalculate.ValueOnBezier(bzNode.BzPoints, ((double)j) * rangeX / (double)resultComp.Width);
                    //resultComp.SetPixel(j, i, vzValue.Y + vzValueH.Y);
                    resultComp.SetPixel(j, i, vzValue.Y);
                }
            }
            return true;
        }
    }

    public partial class UHeightMappingNode : UOpNode
    {
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public PinIn HeightPin { get; set; } = new PinIn();
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public PinIn BezierPin { get; set; } = new PinIn();
        [EGui.Controls.PropertyGrid.PGCustomValueEditor(HideInPG = true)]
        public PinOut ResultPin { get; set; } = new PinOut();
        public UHeightMappingNode()
        {
            HeightPin.Name = " Height";
            BezierPin.Name = " Bezier";
            ResultPin.Name = " Result";

            HeightPin.Link = UPgcEditorStyles.Instance.NewInOutPinDesc();
            BezierPin.Link = UPgcEditorStyles.Instance.NewInOutPinDesc("Bezier");
            ResultPin.Link = UPgcEditorStyles.Instance.NewInOutPinDesc();

            Icon.Size = new Vector2(25, 25);
            Icon.Color = 0xFF00FF00;
            TitleColor = 0xFF204020;
            BackColor = 0x80808080;

            AddPinIn(HeightPin);
            AddPinIn(BezierPin);
            AddPinOut(ResultPin);
        }

        [Rtti.Meta]
        public Terrain.CDLOD.UTerrainMaterialIdManager MaterialIdManager { get; } = new Terrain.CDLOD.UTerrainMaterialIdManager();
        public override Int32_2 GetOutPinSize(PinOut pin)
        {
            if (ResultPin == pin)
            {
                var graph = ParentGraph as UPgcGraph;
                var buffer = graph.BufferCache.FindBuffer(HeightPin);
                if (buffer != null)
                {
                    return new Int32_2(buffer.Width, buffer.Height);
                }
            }
            return base.GetOutPinSize(pin);
        }
        public override bool OnProcedure(UPgcGraph graph)
        {
            if (MaterialIdManager.MaterialIdArray.Count == 0)
            {
                var dft = new Terrain.CDLOD.UTerrainMaterialId();
                dft.TexDiffuse = UEngine.Instance.GfxDevice.TextureManager.DefaultTexture.AssetName;
                MaterialIdManager.MaterialIdArray.Add(dft);
            }

            var bzNode = GetInputNode(graph, BezierPin) as UBezier;
            var heightComp = graph.BufferCache.FindBuffer(HeightPin);
            var resultComp = graph.BufferCache.FindBuffer(ResultPin);
            var rangeX = (bzNode.MaxX - bzNode.MinX);
            var rangeY = (bzNode.MaxY - bzNode.MinY);

            //var randObj = new Support.URandom();
            //randObj.mCoreObject.SetSeed(20);
            for (int i = 0; i < resultComp.Height; i++)
            {
                for (int j = 0; j < resultComp.Width; j++)
                {
                    var height = heightComp.GetPixel(j, i);
                    var vzValue = BezierCalculate.ValueOnBezier(bzNode.BzPoints, height);// - bzNode.MinX);// ((double)j) * rangeX / (double)resultComp.Width);

                    var norValue = (vzValue.Y - bzNode.MinY) / rangeY;
                    norValue = CoreDefine.Clamp(norValue, 0, 1);
                    int idx = (int)((float)MaterialIdManager.MaterialIdArray.Count * norValue);
                    if (idx >= MaterialIdManager.MaterialIdArray.Count)
                        idx = MaterialIdManager.MaterialIdArray.Count - 1;

                    //idx = randObj.mCoreObject.NextValue16Bit() % 2;
                    float value = idx;
                    //value += 0.4f;// (1.0f / 255.0f) * 0.2f;
                    resultComp.SetPixel(j, i, value);
                }
            }
            heightComp.LifeCount--;
            //MaterialIdManager.BuildSRV(UEngine.Instance.GfxDevice.RenderContext.mCoreObject.GetImmCommandList());
            return true;
        }
    }
}