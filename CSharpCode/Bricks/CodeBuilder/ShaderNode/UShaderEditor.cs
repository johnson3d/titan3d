using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Bricks.CodeBuilder.ShaderNode
{
    public class UShaderEditorStyles
    {
        public static UShaderEditorStyles Instance = new UShaderEditorStyles();
        public EGui.UVAnim FunctionIcon = new EGui.UVAnim(0xFF00FF00, 25);
        public uint FunctionTitleColor = 0xFF204020;
        public uint FunctionBGColor = 0x80808080;
        public EGui.Controls.NodeGraph.NodePin.LinkDesc NewInOutPinDesc(string linkType = "Value")
        {
            var result = new EGui.Controls.NodeGraph.NodePin.LinkDesc();
            result.Icon.Size = new Vector2(20, 20);
            result.ExtPadding = 0;
            result.LineThinkness = 3;
            result.LineColor = 0xFFFF0000;
            result.CanLinks.Add(linkType);
            return result;
        }
    }

    public partial class UShaderEditor : Editor.IAssetEditor, IO.ISerializer, ITickable, Graphics.Pipeline.IRootForm
    {
        public UShaderEditor()
        {
            PreviewViewport = new UPreviewViewport();
        }
        ~UShaderEditor()
        {
            Cleanup();
        }
        public void Cleanup()
        {
            PreviewViewport?.Cleanup();
            PreviewViewport = null;
            MaterialPropGrid.Target = null;
        }
        protected bool mVisible = true;
        public bool Visible { get => mVisible; set => mVisible = value; }
        public uint DockId { get; set; }
        public ImGuiCond_ DockCond { get; set; } = ImGuiCond_.ImGuiCond_FirstUseEver;
        public Graphics.Pipeline.IRootForm GetRootForm()
        {
            return this;
        }
        #region ISerializer
        public void OnPreRead(object tagObject, object hostObject, bool fromXml)
        {

        }
        public void OnPropertyRead(object tagObject, System.Reflection.PropertyInfo prop, bool fromXml)
        {

        }
        #endregion
        #region Tickable
        public void TickLogic(int ellapse)
        {
            PreviewViewport.TickLogic(ellapse);
        }
        public void TickRender(int ellapse)
        {
            PreviewViewport.TickRender(ellapse);
        }
        public void TickSync(int ellapse)
        {
            PreviewViewport.TickSync(ellapse);
        }
        #endregion
        #region IAssetEditor
        bool IsStarting = false;
        public async System.Threading.Tasks.Task<bool> OpenEditor(Editor.UMainEditorApplication mainEditor, RName name, object arg)
        {
            if (IsStarting)
                return false;

            MaterialGraph.ShaderEditor = this;
            IsStarting = true;
            Material = await UEngine.Instance.GfxDevice.MaterialManager.GetMaterial(name);
            if (Material == null)
            {
                IsStarting = false;
                return false;
            }

            var xml = IO.FileManager.LoadXmlFromString(Material.GraphXMLString);
            if (xml != null)
            {
                object pThis = this;
                IO.SerializerHelper.ReadObjectMetaFields(this, xml.LastChild as System.Xml.XmlElement, ref pThis, null);
            }
            
            if (MaterialOutput == null)
            {
                MaterialOutput = UMaterialOutput.NewNode(MaterialGraph);
                MaterialGraph.AddNode(MaterialOutput);
            }

            AssetName = name;
            IsStarting = false;

            MaterialPropGrid.IsReadOnly = true;
            MaterialPropGrid.Target = Material;

            PreviewViewport.Title = "MaterialPreview";
            await PreviewViewport.Initialize(UEngine.Instance.GfxDevice.MainWindow, new Graphics.Pipeline.Mobile.UMobileFSPolicy(), 0, 1);

            PreviewPropGrid.Target = PreviewViewport;

            UEngine.Instance.TickableManager.AddTickable(this);
            return true;
        }
        public void OnCloseEditor()
        {
            UEngine.Instance.TickableManager.RemoveTickable(this);
            Cleanup();
        }
        public void OnEvent(ref SDL2.SDL.SDL_Event e)
        {
            //PreviewViewport.OnEvent(ref e);
        }
        #endregion
        public RName AssetName { get; set; }
        public Graphics.Pipeline.Shader.UMaterial Material;
        public float LeftWidth = 0;
        public Vector2 WindowPos;
        public Vector2 WindowSize = new Vector2(800, 600);
        [Rtti.Meta]
        public UMaterialGraph MaterialGraph { get; } = new UMaterialGraph();
        [Rtti.Meta(Order = 1)]
        public Guid OutputNodeId
        {
            get
            {
                return MaterialOutput.NodeId;
            }
            set
            {
                MaterialOutput = MaterialGraph.FindNode(value) as UMaterialOutput;
                if (MaterialOutput == null)
                {
                    MaterialOutput = UMaterialOutput.NewNode(MaterialGraph);
                    MaterialGraph.AddNode(MaterialOutput);
                }
            }
        }
        public UMaterialOutput MaterialOutput = null;
        public EGui.Controls.PropertyGrid.PropertyGrid NodePropGrid = new EGui.Controls.PropertyGrid.PropertyGrid();
        public EGui.Controls.PropertyGrid.PropertyGrid MaterialPropGrid = new EGui.Controls.PropertyGrid.PropertyGrid();
        public EGui.Controls.PropertyGrid.PropertyGrid PreviewPropGrid = new EGui.Controls.PropertyGrid.PropertyGrid();
        public UPreviewViewport PreviewViewport;
        #region DrawUI
        public unsafe void OnDraw()
        {
            if (Material == null)
                return;

            if (Visible == false)
                return;

            bool drawing = true;
            var pivot = new Vector2(0);
            ImGuiAPI.SetNextWindowSize(ref WindowSize, ImGuiCond_.ImGuiCond_FirstUseEver);
            ImGuiAPI.SetNextWindowDockID(DockId, DockCond);
            if (ImGuiAPI.Begin(Material.AssetName.Name, ref mVisible, ImGuiWindowFlags_.ImGuiWindowFlags_None |
                ImGuiWindowFlags_.ImGuiWindowFlags_NoSavedSettings))
            {
                if (ImGuiAPI.IsWindowDocked())
                {
                    DockId = ImGuiAPI.GetWindowDockID();
                }
                if (ImGuiAPI.IsWindowFocused(ImGuiFocusedFlags_.ImGuiFocusedFlags_RootAndChildWindows))
                {
                    var mainEditor = UEngine.Instance.GfxDevice.MainWindow as Editor.UMainEditorApplication;
                    if (mainEditor != null)
                        mainEditor.AssetEditorManager.CurrentActiveEditor = this;
                }
                WindowPos = ImGuiAPI.GetWindowPos();
                WindowSize = ImGuiAPI.GetWindowSize();
                DrawToolBar();
                ImGuiAPI.Separator();
                ImGuiAPI.Columns(2, null, true);
                if (LeftWidth == 0)
                {
                    ImGuiAPI.SetColumnWidth(0, 300);
                }
                LeftWidth = ImGuiAPI.GetColumnWidth(0);
                var min = ImGuiAPI.GetWindowContentRegionMin();
                var max = ImGuiAPI.GetWindowContentRegionMin();

                DrawLeft(ref min, ref max);
                ImGuiAPI.NextColumn();

                DrawRight(ref min, ref max);
                ImGuiAPI.NextColumn();

                ImGuiAPI.Columns(1, null, true);
            }
            else
            {
                drawing = false;
            }
            ImGuiAPI.End();

            if (drawing)
            {
                if (PreviewDockId != 0)
                {
                    PreviewViewport.DockId = PreviewDockId;
                    PreviewViewport.DockCond = ImGuiCond_.ImGuiCond_Always;
                    PreviewViewport.VieportType = Graphics.Pipeline.UViewportSlate.EVieportType.Window;
                    PreviewViewport.OnDraw();
                }
            }
        }
        protected unsafe void DrawToolBar()
        {
            var btSize = new Vector2(64, 64);
            if (ImGuiAPI.Button("Save", ref btSize))
            {
                var xml = new System.Xml.XmlDocument();
                var xmlRoot = xml.CreateElement($"Root", xml.NamespaceURI);
                xml.AppendChild(xmlRoot);
                IO.SerializerHelper.WriteObjectMetaFields(xml, xmlRoot, this);
                var xmlText = IO.FileManager.GetXmlText(xml);
                Material.GraphXMLString = xmlText;
                Material.HLSLCode = GenHLSLCode();
                Material.SaveAssetTo(Material.AssetName);
            }
            ImGuiAPI.SameLine(0, -1);
            if (ImGuiAPI.Button("Load", ref btSize))
            {
                MaterialOutput = null;
                MaterialGraph.ResetGraph();
                var xml = IO.FileManager.LoadXmlFromString(Material.GraphXMLString);
                object pThis = this;
                IO.SerializerHelper.ReadObjectMetaFields(this, xml.LastChild as System.Xml.XmlElement, ref pThis, null);
            }
            ImGuiAPI.SameLine(0, -1);
            if (ImGuiAPI.Button("Compile", ref btSize))
            {
                System.Diagnostics.Trace.Write(GenHLSLCode());
            }
        }
        uint PreviewDockId = 0;
        protected unsafe void DrawLeft(ref Vector2 min, ref Vector2 max)
        {
            if (PreviewDockId == 0)
                PreviewDockId = ImGuiAPI.GetID($"{AssetName}");

            var size = new Vector2(-1, -1);
            if (ImGuiAPI.BeginChild("LeftWindow", ref size, false, ImGuiWindowFlags_.ImGuiWindowFlags_None))
            {
                ImGuiDockNodeFlags_ dockspace_flags = ImGuiDockNodeFlags_.ImGuiDockNodeFlags_None;
                var winClass = new ImGuiWindowClass();
                winClass.UnsafeCallConstructor();
                var sz = ImGuiAPI.GetWindowSize();
                sz.Y = sz.X;
                ImGuiAPI.DockSpace(PreviewDockId, ref sz, dockspace_flags, ref winClass);
                if (ImGuiAPI.CollapsingHeader("NodeProperty", ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_None))
                {
                    NodePropGrid.OnDraw(true, false, false);
                }
                if (ImGuiAPI.CollapsingHeader("MaterialProperty", ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_None))
                {
                    MaterialPropGrid.OnDraw(true, false, false);
                }
                if (ImGuiAPI.CollapsingHeader("PreviewProperty", ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_None))
                {
                    PreviewPropGrid.OnDraw(true, false, false);
                }
                winClass.UnsafeCallDestructor();
            }
            ImGuiAPI.EndChild();
        }
        protected unsafe void DrawRight(ref Vector2 min, ref Vector2 max)
        {
            var size = new Vector2(-1, -1);
            if (ImGuiAPI.BeginChild("RightWindow", ref size, false, ImGuiWindowFlags_.ImGuiWindowFlags_None))
            {
                MaterialGraph.OnDraw(null, false);
            }
            ImGuiAPI.EndChild();
        }
        #endregion

        private string GenHLSLCode()
        {
            var gen = new Bricks.CodeBuilder.HLSL.UHLSLGen();

            Material.UsedRSView.Clear();
            Material.UsedUniformVars.Clear();
            foreach (IBaseNode node in MaterialGraph.Nodes)
            {
                node.PreGenExpr();
                var type = node.GetType();
                if (type == typeof(Var.VarDimF1) ||
                    type == typeof(Var.VarDimF2) ||
                    type == typeof(Var.VarDimF3) ||
                    type == typeof(Var.VarDimF4) ||
                    type == typeof(Var.VarColor3) ||
                    type == typeof(Var.VarColor4))
                {
                    var varNode = node as Var.VarNode;
                    if(varNode.IsUniform)
                    {
                        var valueProp = type.GetProperty("Value");
                        var value = valueProp.GetValue(node, null);
                        var tmp = new Graphics.Pipeline.Shader.UMaterial.NameValuePair();
                        tmp.VarType = gen.GetTypeString(varNode.VarType.SystemType);
                        tmp.Name = node.Name;
                        tmp.Value = value.ToString();
                        Material.UsedUniformVars.Add(tmp);
                    }
                }
                else if (type == typeof(Var.Texture2D))
                {
                    var tmp = new Graphics.Pipeline.Shader.UMaterial.NameRNamePair();
                    tmp.Name = node.Name;
                    var texNode = node as Var.Texture2D;
                    tmp.Value = texNode.AssetName;
                    Material.UsedRSView.Add(tmp);
                }
                else if (type == typeof(Control.SampleLevel2DNode))
                {
                    var texNode = node as Control.SampleLevel2DNode;
                    var texturePinIn = texNode.FindPinIn("texture");
                    if (texturePinIn.HasLinker() == false)
                    {
                        var tmp = new Graphics.Pipeline.Shader.UMaterial.NameRNamePair();
                        tmp.Name = texNode.TextureVarName;
                        tmp.Value = texNode.AssetName;
                        Material.UsedRSView.Add(tmp);
                    }
                }
            }
            
            var expr = this.MaterialOutput.GetExpr(MaterialGraph, gen, null, false);

            var funGen = gen.GetGen(typeof(DefineFunction));
            funGen.GenLines(expr, gen);

            Material.HLSLCode = gen.ClassCode;

            Material.UpdateShaderCode(false);
            Material.SerialId++;

            return gen.ClassCode;
        }
    }
}

namespace EngineNS.Graphics.Pipeline.Shader
{
    [Editor.UAssetEditor(EditorType = typeof(Bricks.CodeBuilder.ShaderNode.UShaderEditor))]
    public partial class UMaterial
    {
    }
}