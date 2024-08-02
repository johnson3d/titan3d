using System;
using System.Collections.Generic;
using System.Linq;
using EngineNS.Bricks.NodeGraph;

namespace EngineNS.Bricks.CodeBuilder.ShaderNode
{
    public class UShaderEditorStyles
    {
        public static UShaderEditorStyles Instance = new UShaderEditorStyles();
        public EGui.UUvAnim FunctionIcon = new EGui.UUvAnim(0xFF00FF00, 25);
        public uint FunctionTitleColor = 0xFF204020;
        public uint FunctionBGColor = 0x80808080;
        public LinkDesc NewInOutPinDesc(string linkType = "Value")
        {
            var styles = UNodeGraphStyles.DefaultStyles;

            var result = new LinkDesc();
            result.Icon.TextureName = RName.GetRName(styles.PinConnectedVarImg, RName.ERNameType.Engine);
            result.Icon.Size = new Vector2(15, 11);
            result.DisconnectIcon.TextureName = RName.GetRName(styles.PinDisconnectedVarImg, RName.ERNameType.Engine);
            result.DisconnectIcon.Size = new Vector2(15, 11);

            result.ExtPadding = 0;
            result.LineThinkness = 3;
            result.LineColor = 0xFFFF0000;
            result.CanLinks.Add(linkType);
            return result;
        }
    }

    public partial class UShaderEditor : Editor.IAssetEditor, IO.ISerializer, ITickable, IRootForm
    {
        public int GetTickOrder()
        {
            return 0;
        }
        public UShaderEditor()
        {
            PreviewViewport = new Editor.TtPreviewViewport();
        }
        ~UShaderEditor()
        {
            Dispose();
        }
        public void Dispose()
        {
            CoreSDK.DisposeObject(ref PreviewViewport);
            MaterialPropGrid.Target = null;
        }
        EGui.TtCodeEditor mShaderEditor = new EGui.TtCodeEditor();
        public async Thread.Async.TtTask<bool> Initialize()
        {
            await EngineNS.Thread.TtAsyncDummyClass.DummyFunc();
            mShaderEditor.mCoreObject.SetLanguage("HLSL");
            mShaderEditor.mCoreObject.ApplyLangDefine();
            mShaderEditor.mCoreObject.ApplyErrorMarkers();
            return true;
        }
        protected bool mVisible = true;
        public bool Visible { get => mVisible; set => mVisible = value; }
        public uint DockId { get; set; }
        public ImGuiCond_ DockCond { get; set; } = ImGuiCond_.ImGuiCond_FirstUseEver;
        public IRootForm GetRootForm()
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
        public float LoadingPercent { get; set; } = 1.0f;
        public string ProgressText { get; set; } = "Loading";
        bool IsStarting = false;
        protected async System.Threading.Tasks.Task Initialize_PreviewMaterial(Graphics.Pipeline.TtViewportSlate viewport, USlateApplication application, Graphics.Pipeline.URenderPolicy policy, float zMin, float zMax)
        {
            viewport.RenderPolicy = policy;

            await viewport.World.InitWorld();

            (viewport as Editor.TtPreviewViewport).CameraController.ControlCamera(viewport.RenderPolicy.DefaultCamera);

            var materials = new Graphics.Pipeline.Shader.UMaterial[1];
            materials[0] = Material;
            if (materials[0] == null)
                return;
            var mesh = new Graphics.Mesh.TtMesh();
            var rect = Graphics.Mesh.UMeshDataProvider.MakeBox(-0.5f, -0.5f, -0.5f, 1, 1, 1);
            var rectMesh = rect.ToMesh();
            var ok = mesh.Initialize(rectMesh, materials, Rtti.UTypeDescGetter<Graphics.Mesh.UMdfStaticMesh>.TypeDesc);
            if (ok)
            {
                //mesh.DirectSetWorldMatrix(ref Matrix.mIdentity);
                
                var meshNode = await GamePlay.Scene.UMeshNode.AddMeshNode(viewport.World, viewport.World.Root, new GamePlay.Scene.UMeshNode.UMeshNodeData(), typeof(GamePlay.UPlacement), mesh, DVector3.Zero, Vector3.One, Quaternion.Identity);
                meshNode.HitproxyType = Graphics.Pipeline.UHitProxy.EHitproxyType.Root;
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
            //this.RenderPolicy.GBuffers.SunLightColor = new Vector3(1, 1, 1);
            //this.RenderPolicy.GBuffers.SunLightDirection = new Vector3(1, 1, 1);
            //this.RenderPolicy.GBuffers.SkyLightColor = new Vector3(0.1f, 0.1f, 0.1f);
            //this.RenderPolicy.GBuffers.GroundLightColor = new Vector3(0.1f, 0.1f, 0.1f);
            //this.RenderPolicy.GBuffers.UpdateViewportCBuffer();

            var gridNode = await GamePlay.Scene.UGridNode.AddGridNode(viewport.World, viewport.World.Root);
            gridNode.ViewportSlate = this.PreviewViewport;
        }
        public async Thread.Async.TtTask<bool> OpenEditor(Editor.UMainEditorApplication mainEditor, RName name, object arg)
        {
            if (IsStarting)
                return false;

            MaterialGraph.ShaderEditor = this;
            MaterialGraph.ResetGraph();
            IsStarting = true;
            Material = await UEngine.Instance.GfxDevice.MaterialManager.CreateMaterial(name);
            Material.IsEditingMaterial = true;
            //Material = await UEngine.Instance.GfxDevice.MaterialManager.GetMaterial(name);
            if (Material == null)
            {
                IsStarting = false;
                return false;
            }

            //EngineNS.EGui.Controls.NodeGraph.PinLinker
            //var graphStr = Material.GraphXMLString?.Replace("EngineNS.EGui.Controls.NodeGraph.PinLinker", "EngineNS.Bricks.NodeGraph.UPinLinker");
            var xml = IO.TtFileManager.LoadXmlFromString(Material.GraphXMLString);            
            
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

            for(int i=0; i<MaterialGraph.Nodes.Count; i++)
            {
                MaterialGraph.SetDefaultActionForNode(MaterialGraph.Nodes[i]);
            }

            AssetName = name;
            IsStarting = false;

            await NodePropGrid.Initialize();

            await MaterialPropGrid.Initialize();
            //MaterialPropGrid.IsReadOnly = true;
            MaterialPropGrid.Target = Material;

            PreviewViewport.PreviewAsset = AssetName;
            PreviewViewport.Title = $"MaterialPreview:{AssetName}";
            PreviewViewport.OnInitialize = Initialize_PreviewMaterial;
            await PreviewViewport.Initialize(UEngine.Instance.GfxDevice.SlateApplication, UEngine.Instance.Config.MainRPolicyName, 0, 1);
            
            await PreviewPropGrid.Initialize();
            PreviewPropGrid.PGName = $"PGMaterialPreview:{AssetName}";
            PreviewPropGrid.Target = PreviewViewport;

            GraphRenderer.SetGraph(MaterialGraph);

            UEngine.Instance.TickableManager.AddTickable(this);
            return true;
        }
        public void OnCloseEditor()
        {
            UEngine.Instance.GfxDevice.EffectManager.RemoveEditingMaterial(AssetName);
            UEngine.Instance.TickableManager.RemoveTickable(this);
            Dispose();
        }
        public void OnEvent(in Bricks.Input.Event e)
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
        public Bricks.NodeGraph.UGraphRenderer GraphRenderer = new NodeGraph.UGraphRenderer();
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
        public Editor.TtPreviewViewport PreviewViewport;
        #region DrawUI
        protected ImGuiWindowClass mDockKeyClass;
        public ImGuiWindowClass DockKeyClass => mDockKeyClass;

        public bool IsDrawing { get; set; }
        public unsafe void OnDraw()
        {
            if (Visible == false || Material == null)
                return;

            var pivot = new Vector2(0);
            ImGuiAPI.SetNextWindowSize(in WindowSize, ImGuiCond_.ImGuiCond_FirstUseEver);
            IsDrawing = EGui.UIProxy.DockProxy.BeginMainForm(AssetName.Name, this, ImGuiWindowFlags_.ImGuiWindowFlags_NoSavedSettings);
            if (IsDrawing)
            {
                if (ImGuiAPI.IsWindowFocused(ImGuiFocusedFlags_.ImGuiFocusedFlags_RootAndChildWindows))
                {
                    var mainEditor = UEngine.Instance.GfxDevice.SlateApplication as Editor.UMainEditorApplication;
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

            DrawTextEditor();
            DrawShaderGraph();
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
            ImGuiAPI.DockBuilderSplitNode(rightId, ImGuiDir_.ImGuiDir_Left, 0.8f, ref middleId, ref rightId);
            ImGuiAPI.DockBuilderSplitNode(rightId, ImGuiDir_.ImGuiDir_Down, 0.5f, ref rightDownId, ref rightUpId);
            ImGuiAPI.DockBuilderSplitNode(middleId, ImGuiDir_.ImGuiDir_Down, 0.3f, ref downId, ref middleId);
            ImGuiAPI.DockBuilderSplitNode(middleId, ImGuiDir_.ImGuiDir_Left, 0.2f, ref leftId, ref middleId);

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
                var code = GenHLSLCode();
                System.Diagnostics.Trace.WriteLine(Material.DefineCode.TextCode);
                System.Diagnostics.Trace.WriteLine(code);

                var xml = new System.Xml.XmlDocument();
                var xmlRoot = xml.CreateElement($"Root", xml.NamespaceURI);
                xml.AppendChild(xmlRoot);
                IO.SerializerHelper.WriteObjectMetaFields(xml, xmlRoot, this);
                var xmlText = IO.TtFileManager.GetXmlText(xml);
                Material.GraphXMLString = xmlText;
                //if (code == Material.HLSLCode)
                //    return;
                //Material.UpdateShaderCode(false);
                //Material.HLSLCode = code;// GenHLSLCode();
                //Material.SerialId++;
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
                    NodePropGrid.HideInheritDeclareType = Rtti.UTypeDescGetter<UNodeBase>.TypeDesc;
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
            IO.SerializerHelper.WriteObjectMetaFields(xml, xmlRoot, this);
            var xmlText = IO.TtFileManager.GetXmlText(xml);
            Material.GraphXMLString = xmlText;
            GenHLSLCode();
            //Material.UpdateShaderCode(false);
            //Material.HLSLCode = GenHLSLCode();
            Material.SaveAssetTo(Material.AssetName);
            //Material.SerialId++;

            //Editor.USnapshot.Save(Material.AssetName, Material.GetAMeta(), PreviewViewport.RenderPolicy.GetFinalShowRSV(), UEngine.Instance.GfxDevice.RenderContext.mCoreObject.GetImmCommandList());

            if (await UEngine.Instance.GfxDevice.MaterialManager.ReloadMaterial(Material.AssetName))
            {
                
            }
        }
        #endregion

        Backends.UHLSLCodeGenerator mHLSLCodeGen = new Backends.UHLSLCodeGenerator();
        private string GenHLSLCode()
        {
            Material.UsedRSView.Clear();
            Material.UsedUniformVars.Clear();
            Material.UsedSamplerStates.Clear();

            var MaterialClass = new UClassDeclaration();

            var gen = mHLSLCodeGen.GetCodeObjectGen(Rtti.UTypeDescGetter<UMethodDeclaration>.TypeDesc);
            BuildCodeStatementsData data = new BuildCodeStatementsData()
            {
                ClassDec = MaterialClass,
                NodeGraph = MaterialGraph,
                UserData = Material,
                CodeGen = mHLSLCodeGen,
            };
            MaterialOutput.BuildStatements(null, ref data);
            string code = "";
            var incGen = mHLSLCodeGen.GetCodeObjectGen(Rtti.UTypeDescGetter<TtIncludeDeclaration>.TypeDesc);
            UCodeGeneratorData genData = new UCodeGeneratorData()
            {
                Method = null,
                CodeGen = mHLSLCodeGen,
                UserData = Material,
            };
            Material.IncludeFiles.Clear();
            foreach (var i in MaterialClass.PreIncludeHeads)
            {
                incGen.GenCodes(i, ref code, ref genData);
                Material.IncludeFiles.Add(i.FilePath);
            }
            genData = new UCodeGeneratorData()
            {
                Method = MaterialOutput.VSFunction,
                CodeGen = mHLSLCodeGen,
                UserData = Material,
            };
            gen.GenCodes(MaterialOutput.VSFunction, ref code, ref genData);
            genData = new UCodeGeneratorData()
            {
                Method = MaterialOutput.PSFunction,
                CodeGen = mHLSLCodeGen,
                UserData = Material,
            };
            gen.GenCodes(MaterialOutput.PSFunction, ref code, ref genData);

            Material.HLSLCode = code;
            Material.VSNeedStreams = MaterialOutput.GetVSNeedStreams();
            Material.PSNeedInputs = MaterialOutput.GetPSNeedInputs();

            if (Material.NormalMode == Graphics.Pipeline.Shader.UMaterial.ENormalMode.NormalMap)
            {
                if (Material.VSNeedStreams.Contains(NxRHI.EVertexStreamType.VST_Normal) == false)
                    Material.VSNeedStreams.Add(NxRHI.EVertexStreamType.VST_Normal);
                if (Material.VSNeedStreams.Contains(NxRHI.EVertexStreamType.VST_Tangent) == false)
                    Material.VSNeedStreams.Add(NxRHI.EVertexStreamType.VST_Tangent);

                if (Material.PSNeedInputs.Contains(Graphics.Pipeline.Shader.EPixelShaderInput.PST_Normal) == false)
                    Material.PSNeedInputs.Add(Graphics.Pipeline.Shader.EPixelShaderInput.PST_Normal);
                if (Material.PSNeedInputs.Contains(Graphics.Pipeline.Shader.EPixelShaderInput.PST_Tangent) == false)
                    Material.PSNeedInputs.Add(Graphics.Pipeline.Shader.EPixelShaderInput.PST_Tangent);
            }
            else if (Material.NormalMode == Graphics.Pipeline.Shader.UMaterial.ENormalMode.Normal)
            {
                if (Material.VSNeedStreams.Contains(NxRHI.EVertexStreamType.VST_Normal) == false)
                    Material.VSNeedStreams.Add(NxRHI.EVertexStreamType.VST_Normal);
                if (Material.PSNeedInputs.Contains(Graphics.Pipeline.Shader.EPixelShaderInput.PST_Normal) == false)
                    Material.PSNeedInputs.Add(Graphics.Pipeline.Shader.EPixelShaderInput.PST_Normal);
            }
        
            Material.UpdateShaderCode(false);

            mShaderEditor.mCoreObject.SetText(code);
            return code;
        }
        //[Obsolete]
        //private string GenHLSLCode_Old()
        //{
        //    var gen = new Bricks.CodeBuilder.HLSL.UHLSLGen();

        //    Material.UsedRSView.Clear();
        //    Material.UsedUniformVars.Clear();
        //    Material.UsedSamplerStates.Clear();
        //    foreach (IBaseNode node in MaterialGraph.Nodes)
        //    {
        //        node.PreGenExpr();
        //        var type = node.GetType();
        //        if (type == typeof(Var.VarDimF1) ||
        //            type == typeof(Var.VarDimF2) ||
        //            type == typeof(Var.VarDimF3) ||
        //            type == typeof(Var.VarDimF4) ||
        //            type == typeof(Var.VarColor3) ||
        //            type == typeof(Var.VarColor4))
        //        {
        //            node.OnMaterialEditorGenCode(Material);
        //            //var varNode = node as Var.VarNode;
        //            //if(varNode.IsUniform)
        //            //{
        //            //    var valueProp = type.GetProperty("Value");
        //            //    var value = valueProp.GetValue(node, null);
        //            //    var tmp = new Graphics.Pipeline.Shader.UMaterial.NameValuePair();
        //            //    tmp.VarType = gen.GetTypeString(varNode.VarType.SystemType);
        //            //    tmp.Name = node.Name;
        //            //    tmp.Value = value.ToString();
        //            //    Material.UsedUniformVars.Add(tmp);
        //            //}
        //        }
        //        else if (type == typeof(Var.Texture2D))
        //        {
        //            node.OnMaterialEditorGenCode(Material);
        //        }
        //        else if (type == typeof(Var.SamplerState))
        //        {
        //            node.OnMaterialEditorGenCode(Material);
        //        }
        //        else if (type == typeof(Control.SampleLevel2DNode) || type == typeof(Control.Sample2DNode) ||
        //            type == typeof(Control.SampleArrayLevel2DNode) || type == typeof(Control.SampleArray2DNode))
        //        {
        //            node.OnMaterialEditorGenCode(Material);
        //        }
        //        else
        //        {
        //            node.OnMaterialEditorGenCode(Material);
        //        }
        //    }
            
        //    var expr = this.MaterialOutput.GetExpr(MaterialGraph, gen, null, false);

        //    var funGen = gen.GetGen(typeof(DefineFunction));
        //    funGen.GenLines(expr, gen);

        //    Material.HLSLCode = gen.ClassCode;

        //    Material.UpdateShaderCode(false);
        //    Material.SerialId++;

        //    return gen.ClassCode;
        //}
    }
}

namespace EngineNS.Graphics.Pipeline.Shader
{
    [Editor.UAssetEditor(EditorType = typeof(Bricks.CodeBuilder.ShaderNode.UShaderEditor))]
    public partial class UMaterial
    {
    }
}