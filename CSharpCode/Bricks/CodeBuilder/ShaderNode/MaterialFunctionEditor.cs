using System;
using System.Collections.Generic;
using EngineNS.Graphics.Pipeline.Shader;

namespace EngineNS.Bricks.CodeBuilder.ShaderNode
{
    public class TtMaterialFunctionEditor : Editor.IAssetEditor, ITickable, IRootForm
    {
        public RName AssetName { get; set; }
        public TtMaterialFunction MaterialFunction;
        public TtMaterial PreviewMaterial;

        public int GetTickOrder()
        {
            return 0;
        }
        public TtMaterialFunctionEditor()
        {
            PreviewViewport = new Editor.TtPreviewViewport();
        }
        ~TtMaterialFunctionEditor()
        {
            Dispose();
        }
        public void Dispose()
        {
            CoreSDK.DisposeObject(ref PreviewViewport);
            MaterialPropGrid.Target = null;
        }
        public async Thread.Async.TtTask<bool> Initialize()
        {
            await EngineNS.Thread.TtAsyncDummyClass.DummyFunc();
            mShaderEditor.mCoreObject.SetLanguage("HLSL");
            mShaderEditor.mCoreObject.ApplyLangDefine();
            mShaderEditor.mCoreObject.ApplyErrorMarkers();
            return true;
        }
        EGui.TtCodeEditor mShaderEditor = new EGui.TtCodeEditor();
        #region IRootForm
        protected bool mVisible = true;
        public bool Visible { get => mVisible; set => mVisible = value; }
        public uint DockId { get; set; }
        public ImGuiCond_ DockCond { get; set; } = ImGuiCond_.ImGuiCond_FirstUseEver;
        public IRootForm GetRootForm()
        {
            return this;
        }
        #endregion
        #region Tickable
        public void TickLogic(float ellapse)
        {
            PreviewViewport.TickLogic(ellapse);
        }
        public void TickRender(float ellapse)
        {
            PreviewViewport.TickRender(ellapse);
        }
        public void TickBeginFrame(float ellapse)
        {

        }
        public void TickSync(float ellapse)
        {
            PreviewViewport.TickSync(ellapse);
        }
        #endregion
        #region IAssetEditor
        public string GetWindowsName()
        {
            return AssetName.Name;
        }
        public float LoadingPercent { get; set; } = 1.0f;
        public string ProgressText { get; set; } = "Loading";
        bool IsStarting = false;
        protected async System.Threading.Tasks.Task<bool> Initialize_PreviewMaterial(Graphics.Pipeline.TtViewportSlate viewport, TtSlateApplication application, Graphics.Pipeline.TtRenderPolicy policy, float zMin, float zMax)
        {
            viewport.RenderPolicy = policy;

            await viewport.World.InitWorld();

            (viewport as Editor.TtPreviewViewport).CameraController.ControlCamera(viewport.RenderPolicy.DefaultCamera);

            var materials = new Graphics.Pipeline.Shader.TtMaterial[1];
            materials[0] = PreviewMaterial;
            if (materials[0] == null)
                return false;
            var mesh = new Graphics.Mesh.TtMesh();
            //var rect = Graphics.Mesh.UMeshDataProvider.MakeBox(-0.5f, -0.5f, -0.5f, 1, 1, 1);
            //var rectMesh = rect.ToMesh();
            var rectMesh = await TtEngine.Instance.GfxDevice.MeshPrimitiveManager.CreateMeshPrimitive(RName.GetRName("mesh/base/sphere.vms", RName.ERNameType.Engine));
            var ok = mesh.Initialize(rectMesh, materials, Rtti.TtTypeDescGetter<Graphics.Mesh.UMdfStaticMesh>.TypeDesc);
            if (ok)
            {
                //mesh.DirectSetWorldMatrix(ref Matrix.mIdentity);

                var meshNode = await GamePlay.Scene.TtMeshNode.AddMeshNode(viewport.World, viewport.World.Root, new GamePlay.Scene.TtMeshNode.TtMeshNodeData(), typeof(GamePlay.TtPlacement), mesh, DVector3.Zero, Vector3.One, Quaternion.Identity);
                meshNode.HitproxyType = Graphics.Pipeline.TtHitProxy.EHitproxyType.Root;
                meshNode.NodeData.Name = "PreviewObject";
                meshNode.IsAcceptShadow = false;
                meshNode.IsCastShadow = true;
            }

            var aabb = mesh.MaterialMesh.AABB;
            float radius = aabb.GetMaxSide();
            DBoundingSphere sphere;
            sphere.Center = aabb.GetCenter().AsDVector();
            sphere.Radius = radius;
            policy.DefaultCamera.AutoZoom(in sphere);

            var gridNode = await GamePlay.Scene.UGridNode.AddGridNode(viewport.World, viewport.World.Root);
            gridNode.ViewportSlate = this.PreviewViewport;
            return true;
        }
        public async Thread.Async.TtTask<bool> OpenEditor(Editor.UMainEditorApplication mainEditor, RName name, object arg)
        {
            if (IsStarting)
                return false;

            MaterialGraph.ShaderEditor = this;
            MaterialGraph.ResetGraph();
            IsStarting = true;
            MaterialFunction = await TtEngine.Instance.GfxDevice.MaterialFunctionManager.CreateMaterialFunction(name);
            //MaterialFunction.IsEditingMaterial = true;
            if (MaterialFunction == null)
            {
                IsStarting = false;
                return false;
            }

            //EngineNS.EGui.Controls.NodeGraph.PinLinker
            //var graphStr = Material.GraphXMLString?.Replace("EngineNS.EGui.Controls.NodeGraph.PinLinker", "EngineNS.Bricks.NodeGraph.UPinLinker");
            if (string.IsNullOrEmpty(MaterialFunction.GraphXMLString) == false)
            {
                var xml = IO.TtFileManager.LoadXmlFromString(MaterialFunction.GraphXMLString);
                if (xml != null)
                {
                    object pThis = this.MaterialGraph;
                    IO.SerializerHelper.ReadObjectMetaFields(this, xml.LastChild as System.Xml.XmlElement, ref pThis, null);
                }
                else
                {
                    System.Diagnostics.Debug.Assert(false);
                    MaterialFunction.GraphXMLString = MaterialFunction.GraphXMLString.Substring(0, MaterialFunction.GraphXMLString.Length - 1);
                    xml = IO.TtFileManager.LoadXmlFromString(MaterialFunction.GraphXMLString);
                    object pThis = this.MaterialGraph;
                    IO.SerializerHelper.ReadObjectMetaFields(this, xml.LastChild as System.Xml.XmlElement, ref pThis, null);
                }
            }

            for (int i = 0; i < MaterialGraph.Nodes.Count; i++)
            {
                MaterialGraph.SetDefaultActionForNode(MaterialGraph.Nodes[i]);
            }

            AssetName = name;
            IsStarting = false;

            await NodePropGrid.Initialize();

            await MaterialPropGrid.Initialize();
            //MaterialPropGrid.IsReadOnly = true;
            MaterialPropGrid.Target = MaterialFunction;

            PreviewViewport.PreviewAsset = AssetName;
            PreviewViewport.Title = $"MaterialPreview:{AssetName}";
            PreviewViewport.OnInitialize = Initialize_PreviewMaterial;
            await PreviewViewport.Initialize(TtEngine.Instance.GfxDevice.SlateApplication, TtEngine.Instance.Config.SimpleRPolicyName, 0, 1);

            await PreviewPropGrid.Initialize();
            PreviewPropGrid.PGName = $"PGMaterialPreview:{AssetName}";
            PreviewPropGrid.Target = PreviewViewport;

            GraphRenderer.SetGraph(MaterialGraph);

            TtEngine.Instance.TickableManager.AddTickable(this);
            return true;
        }
        public void OnCloseEditor()
        {
            TtEngine.Instance.GfxDevice.EffectManager.RemoveEditingMaterial(AssetName);
            TtEngine.Instance.TickableManager.RemoveTickable(this);
            Dispose();
        }
        public void OnEvent(in Bricks.Input.Event e)
        {
            //PreviewViewport.OnEvent(ref e);
        }
        #endregion

        #region DrawUI
        //public UMaterialOutput MaterialOutput = null;
        public EGui.Controls.PropertyGrid.PropertyGrid NodePropGrid = new EGui.Controls.PropertyGrid.PropertyGrid();
        public EGui.Controls.PropertyGrid.PropertyGrid MaterialPropGrid = new EGui.Controls.PropertyGrid.PropertyGrid();
        public EGui.Controls.PropertyGrid.PropertyGrid PreviewPropGrid = new EGui.Controls.PropertyGrid.PropertyGrid();
        public Editor.TtPreviewViewport PreviewViewport;
        public Bricks.NodeGraph.TtGraphRenderer GraphRenderer = new Bricks.NodeGraph.TtGraphRenderer();
        public TtMaterialFunctionGraph MaterialGraph { get; } = new TtMaterialFunctionGraph();
        protected ImGuiWindowClass mDockKeyClass;
        public ImGuiWindowClass DockKeyClass => mDockKeyClass;
        public float LeftWidth = 0;
        public Vector2 WindowPos;
        public Vector2 WindowSize = new Vector2(800, 600);
        public bool IsDrawing { get; set; }
        public unsafe void OnDraw()
        {
            if (Visible == false || MaterialFunction == null)
                return;

            var pivot = new Vector2(0);
            ImGuiAPI.SetNextWindowSize(in WindowSize, ImGuiCond_.ImGuiCond_FirstUseEver);
            IsDrawing = EGui.UIProxy.DockProxy.BeginMainForm(GetWindowsName(), this, ImGuiWindowFlags_.ImGuiWindowFlags_NoSavedSettings);
            if (IsDrawing)
            {
                if (ImGuiAPI.IsWindowFocused(ImGuiFocusedFlags_.ImGuiFocusedFlags_RootAndChildWindows))
                {
                    var mainEditor = TtEngine.Instance.GfxDevice.SlateApplication as Editor.UMainEditorApplication;
                    if (mainEditor != null)
                        mainEditor.AssetEditorManager.CurrentActiveEditor = this;
                }
                WindowPos = ImGuiAPI.GetWindowPos();
                WindowSize = ImGuiAPI.GetWindowSize();
                DrawToolBar();
                //var sz = new Vector2(-1);
                //ImGuiAPI.BeginChild("Client", ref sz, false, ImGuiWindowFlags_.)
                ImGuiAPI.Separator();
            }
            ResetDockspace();
            EGui.UIProxy.DockProxy.EndMainForm(IsDrawing);

            DrawShaderGraph();
            DrawTextEditor();
            DrawNodeDetails();
            DrawMaterialDetails();
            DrawEditorDetails();

            DrawPreview();
        }
        bool mDockInitialized = false;
        protected void ResetDockspace(bool force = false)
        {
            var pos = ImGuiAPI.GetCursorPos();
            var id = ImGuiAPI.GetID(AssetName.Name + "_Dockspace");
            mDockKeyClass.ClassId = id;
            ImGuiAPI.DockSpace(id, Vector2.Zero, ImGuiDockNodeFlags_.ImGuiDockNodeFlags_None, mDockKeyClass);
            if (mDockInitialized && !force)
                return;
            ImGuiAPI.DockBuilderRemoveNode(id);
            ImGuiAPI.DockBuilderAddNode(id, ImGuiDockNodeFlags_.ImGuiDockNodeFlags_None);
            ImGuiAPI.DockBuilderSetNodePos(id, pos);
            ImGuiAPI.DockBuilderSetNodeSize(id, Vector2.One);
            mDockInitialized = true;

            var rightId = id;
            uint middleId = 0;
            uint downId = 0;
            uint leftId = 0;
            uint rightUpId = 0;
            uint rightDownId = 0;
            ImGuiAPI.DockBuilderSplitNode(rightId, ImGuiDir.ImGuiDir_Left, 0.8f, ref middleId, ref rightId);
            ImGuiAPI.DockBuilderSplitNode(rightId, ImGuiDir.ImGuiDir_Down, 0.5f, ref rightDownId, ref rightUpId);
            ImGuiAPI.DockBuilderSplitNode(middleId, ImGuiDir.ImGuiDir_Down, 0.3f, ref downId, ref middleId);
            ImGuiAPI.DockBuilderSplitNode(middleId, ImGuiDir.ImGuiDir_Left, 0.2f, ref leftId, ref middleId);

            ImGuiAPI.DockBuilderDockWindow(EGui.UIProxy.DockProxy.GetDockWindowName("ShaderGraph", mDockKeyClass), middleId);
            ImGuiAPI.DockBuilderDockWindow(EGui.UIProxy.DockProxy.GetDockWindowName("TextEditor", mDockKeyClass), middleId);
            ImGuiAPI.DockBuilderDockWindow(EGui.UIProxy.DockProxy.GetDockWindowName("NodeDetails", mDockKeyClass), rightDownId);
            ImGuiAPI.DockBuilderDockWindow(EGui.UIProxy.DockProxy.GetDockWindowName("EditorDetails", mDockKeyClass), rightDownId);
            ImGuiAPI.DockBuilderDockWindow(EGui.UIProxy.DockProxy.GetDockWindowName("MaterialDetails", mDockKeyClass), rightDownId);
            ImGuiAPI.DockBuilderDockWindow(EGui.UIProxy.DockProxy.GetDockWindowName("Preview", mDockKeyClass), rightUpId);

            ImGuiAPI.DockBuilderFinish(id);
        }
        protected void DrawToolBar()
        {
            var btSize = Vector2.Zero;
            if (EGui.UIProxy.CustomButton.ToolButton("Save", in btSize))
            {
                var noused = Save();
            }
            ImGuiAPI.SameLine(0, -1);
            if (EGui.UIProxy.CustomButton.ToolButton("Compile", in btSize))
            {
                var code = MaterialFunction.GenMateralFunctionGraphCode(new UHLSLCodeGenerator(), MaterialGraph);
                System.Diagnostics.Trace.WriteLine(MaterialFunction.DefineCode.TextCode);
                System.Diagnostics.Trace.WriteLine(code);

                var xml = new System.Xml.XmlDocument();
                var xmlRoot = xml.CreateElement($"Root", xml.NamespaceURI);
                xml.AppendChild(xmlRoot);
                IO.SerializerHelper.WriteObjectMetaFields(xml, xmlRoot, this);
                var xmlText = IO.TtFileManager.GetXmlText(xml);
                MaterialFunction.GraphXMLString = xmlText;
                MaterialFunction.HLSLCode = code;
            }
        }
        bool ShowNodeGraph = true;
        protected void DrawShaderGraph()
        {
            var sz = new Vector2(-1);
            var show = EGui.UIProxy.DockProxy.BeginPanel(mDockKeyClass, "ShaderGraph", ref ShowNodeGraph, ImGuiWindowFlags_.ImGuiWindowFlags_None);
            if (show)
            {
                GraphRenderer.OnDraw();
            }
            EGui.UIProxy.DockProxy.EndPanel(show);
        }
        bool ShowTextEditor = true;
        protected void DrawTextEditor()
        {
            var sz = new Vector2(-1);
            var show = EGui.UIProxy.DockProxy.BeginPanel(mDockKeyClass, "TextEditor", ref ShowTextEditor, ImGuiWindowFlags_.ImGuiWindowFlags_None);
            if (show)
            {
                var winPos = ImGuiAPI.GetWindowPos();
                var vpMin = ImGuiAPI.GetWindowContentRegionMin();
                var vpMax = ImGuiAPI.GetWindowContentRegionMax();
                mShaderEditor.mCoreObject.Render(AssetName.Name, in Vector2.Zero, false);
            }
            EGui.UIProxy.DockProxy.EndPanel(show);
        }
        bool ShowNodePropGrid = true;
        protected void DrawNodeDetails()
        {
            var sz = new Vector2(-1);
            var show = EGui.UIProxy.DockProxy.BeginPanel(mDockKeyClass, "NodeDetails", ref ShowNodePropGrid, ImGuiWindowFlags_.ImGuiWindowFlags_None);
            if (show)
            {
                if (GraphRenderer.Graph != null && GraphRenderer.Graph.SelectedNodesDirty)
                {
                    List<object> tagList = new List<object>(GraphRenderer.Graph.SelectedNodes.Count);
                    for (int i = 0; i < GraphRenderer.Graph.SelectedNodes.Count; i++)
                    {
                        var node = GraphRenderer.Graph.SelectedNodes[i].Node;
                        if (node.GetPropertyEditObject() == null)
                            continue;
                        tagList.Add(node.GetPropertyEditObject());
                    }
                    NodePropGrid.Target = tagList;
                }
                NodePropGrid.OnDraw(true, false, false);
            }
            EGui.UIProxy.DockProxy.EndPanel(show);
        }
        bool ShowMaterialPropGrid = true;
        protected void DrawMaterialDetails()
        {
            var sz = new Vector2(-1);
            var show = EGui.UIProxy.DockProxy.BeginPanel(mDockKeyClass, "MaterialDetails", ref ShowMaterialPropGrid, ImGuiWindowFlags_.ImGuiWindowFlags_None);
            if (show)
            {
                MaterialPropGrid.OnDraw(true, false, false);
            }
            EGui.UIProxy.DockProxy.EndPanel(show);
        }
        bool ShowEditorPropGrid = true;
        protected void DrawEditorDetails()
        {
            var sz = new Vector2(-1);
            var show = EGui.UIProxy.DockProxy.BeginPanel(mDockKeyClass, "EditorDetails", ref ShowEditorPropGrid, ImGuiWindowFlags_.ImGuiWindowFlags_None);
            if (show)
            {
                PreviewPropGrid.OnDraw(true, false, false);
            }
            EGui.UIProxy.DockProxy.EndPanel(show);
        }
        bool ShowPreview = true;
        protected unsafe void DrawPreview()
        {
            var show = EGui.UIProxy.DockProxy.BeginPanel(mDockKeyClass, "Preview", ref ShowPreview, ImGuiWindowFlags_.ImGuiWindowFlags_None);
            if (show)
            {
                PreviewViewport.ViewportType = Graphics.Pipeline.TtViewportSlate.EViewportType.ChildWindow;
                PreviewViewport.OnDraw();
            }
            EGui.UIProxy.DockProxy.EndPanel(show);
        }
        private async System.Threading.Tasks.Task Save()
        {
            var xml = new System.Xml.XmlDocument();
            var xmlRoot = xml.CreateElement($"Root", xml.NamespaceURI);
            xml.AppendChild(xmlRoot);
            IO.SerializerHelper.WriteObjectMetaFields(xml, xmlRoot, MaterialGraph);
            var xmlText = IO.TtFileManager.GetXmlText(xml);
            MaterialFunction.GraphXMLString = xmlText;
            MaterialFunction.SaveAssetTo(MaterialFunction.AssetName);
            //Material.SerialId++;

            //Editor.USnapshot.Save(Material.AssetName, Material.GetAMeta(), PreviewViewport.RenderPolicy.GetFinalShowRSV(), TtEngine.Instance.GfxDevice.RenderContext.mCoreObject.GetImmCommandList());

            if (await TtEngine.Instance.GfxDevice.MaterialFunctionManager.ReloadMaterialFuntion(MaterialFunction.AssetName))
            {

            }
        }
        #endregion
    }
}

namespace EngineNS.Graphics.Pipeline.Shader
{
    [Editor.UAssetEditor(EditorType = typeof(Bricks.CodeBuilder.ShaderNode.TtMaterialFunctionEditor))]
    public partial class TtMaterialFunction
    {
    }
}