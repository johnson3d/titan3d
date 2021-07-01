using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Editor
{
    public class MetaViewEditor : Graphics.Pipeline.IRootForm
    {
        public MetaViewEditor()
        {
            Editor.UMainEditorApplication.RegRootForm(this);
        }
        public bool Visible { get; set; } = true;
        public uint DockId { get; set; }
        public ImGuiCond_ DockCond { get; set; } = ImGuiCond_.ImGuiCond_FirstUseEver;
        public EGui.Controls.PropertyGrid.PropertyGrid PGMember = new EGui.Controls.PropertyGrid.PropertyGrid() { IsReadOnly = true };
        public string mFilterText;
        async System.Threading.Tasks.Task DoTest()
        {
            //await UEngine.Instance.GfxDevice.MaterialManager.GetMaterial(RName.GetRName("UTest/ttt.material"));

            //var shaderCompiler = new Editor.ShaderCompiler.UHLSLCompiler();
            //var env = RName.GetRName("shaders/MobileOpaque.shadingenv", RName.ERNameType.Engine);
            //var mtl = RName.GetRName("UTest/ttt.material");
            //var mdf = typeof(Graphics.Mesh.UMdfStaticMesh);
            //var vsDesc = shaderCompiler.CompileShader(env.Address, "VS_Main", EShaderType.EST_VertexShader, "5_0", mtl, mdf, null, true, true, false, false);

            //var shading = Graphics.Pipeline.Shader.UShadingEnvManager.Instance.GetShadingEnv<Graphics.Pipeline.Shader.Mobile.UBasePassOpaque>();
            //var material = await UEngine.Instance.GfxDevice.MaterialManager.GetMaterial(mtl);
            //var mdfQueue = new Graphics.Mesh.UMdfStaticMesh();
            //await UEngine.Instance.GfxDevice.EffectManager.GetEffect(shading, material, mdfQueue);
            
            if (false)
            {
                var rect = Graphics.Mesh.CMeshDataProvider.MakeRect2D(-1, -1, 2, 2, 0.0F, false);
                var rectMesh = rect.ToMesh();
                rectMesh.AssetName = RName.GetRName("mesh/base/screen_rect.vms", RName.ERNameType.Engine);
                rectMesh.SaveAssetTo(rectMesh.AssetName);

                var ameta = rectMesh.CreateAMeta();
                ameta.SetAssetName(rectMesh.AssetName);
                ameta.AssetId = Guid.NewGuid();
                ameta.TypeStr = Rtti.UTypeDescManager.Instance.GetTypeStringFromType(rectMesh.GetType());
                ameta.Description = $"This is a {rectMesh.GetType().FullName}";
                ameta.SaveAMeta();
            }
            else
            {
                GamePlay.Scene.UNode aabbTreeRoot = null;

                var mainEditor = UEngine.Instance.GfxDevice.MainWindow as Graphics.Pipeline.USlateApplication;
                var world = mainEditor.GetWorldViewportSlate().World;
                var root = mainEditor.GetWorldViewportSlate().World.Root.FindFirstChild("DrawMeshNode");
                if (root == null)
                {
                    var scene = world.Root.GetNearestParentScene();
                    root = scene.NewNode(typeof(GamePlay.Scene.UNode), new GamePlay.Scene.UNodeData() { Name = "DrawMeshNode" }, GamePlay.Scene.EBoundVolumeType.Box, typeof(GamePlay.UPlacementBase));
                    root.Parent = world.Root;
                }
                root.ClearChildren();

                var materials = new Graphics.Pipeline.Shader.UMaterialInstance[2];
                materials[0] = await UEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(RName.GetRName("utest/ddd.uminst"));
                materials[1] = await UEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(RName.GetRName("utest/ground.uminst"));
                //materials[1].UsedRSView[0].Value = RName.GetRName("UTest/texture/ground_01.srv");
                //materials[1].SaveAssetTo(materials[1].AssetName);
                if (materials[0] == null)
                    return;
                var puppetMesh =await UEngine.Instance.GfxDevice.MeshPrimitiveManager.GetMeshPrimitive(RName.GetRName("utest/mesh/puppet_low_ue4.vms"));
                var materialMesh = await UEngine.Instance.GfxDevice.MaterialMeshManager.GetMaterialMesh(RName.GetRName("utest/mesh/puppet_low_ue4.ums"));
                if (materialMesh == null)
                {
                    materialMesh = UEngine.Instance.AssetMetaManager.NewAsset<Graphics.Mesh.UMaterialMesh>(RName.GetRName("utest/mesh/puppet_low_ue4.ums"));
                    materialMesh.Initialize(puppetMesh, materials);
                    materialMesh.SaveAssetTo(RName.GetRName("utest/mesh/puppet_low_ue4.ums"));
                }

                var mesh = new Graphics.Mesh.UMesh();

                //var ok = mesh.Initialize(puppetMesh, materials, Rtti.UTypeDescGetter<Graphics.Mesh.UMdfStaticMesh>.TypeDesc);
                //var ok = mesh.Initialize(materialMesh, Rtti.UTypeDescGetter<Graphics.Mesh.UMdfStaticMesh>.TypeDesc);
                var ok = mesh.Initialize(materialMesh, Rtti.UTypeDescGetter<Graphics.Mesh.UMdfStaticMesh>.TypeDesc);
                if (ok)
                {
                    //var trans = Matrix.Scaling(0.01f);
                    //mesh.SetWorldMatrix(ref trans);// Matrix.mIdentity);

                    var meshNode = GamePlay.Scene.UMeshNode.AddMeshNode(root, new GamePlay.Scene.UNodeData(), typeof(GamePlay.UPlacement), mesh, new Vector3(5,5,5), new Vector3(0.01f), Quaternion.Identity);
                    meshNode.HitproxyType = Graphics.Pipeline.UHitProxy.EHitproxyType.Root;
                    meshNode.NodeData.Name = "Robot0";
                    meshNode.IsScaleChildren = false;
                    meshNode.IsCastShadow = true;

                    var mesh1 = new Graphics.Mesh.UMesh();

                    mesh1.Initialize(puppetMesh, materials, Rtti.UTypeDescGetter<Graphics.Mesh.UMdfStaticMesh>.TypeDesc);
                    var meshNode1 = GamePlay.Scene.UMeshNode.AddMeshNode(meshNode, new GamePlay.Scene.UNodeData(), typeof(GamePlay.UPlacement), mesh1, new Vector3(3, 3, 3), new Vector3(0.01f), Quaternion.RotationAxis(Vector3.UnitY, (float)Math.PI/4));// Quaternion.Identity);
                    meshNode1.NodeData.Name = "Robot1";
                    meshNode1.Parent = meshNode;
                    meshNode1.HitproxyType = Graphics.Pipeline.UHitProxy.EHitproxyType.FollowParent;
                    //meshNode1.Placement.Position = meshNode1.Placement.Position;
                    meshNode1.IsCastShadow = true;
                    aabbTreeRoot = meshNode;
                }

                var cookedMesh = Graphics.Mesh.CMeshDataProvider.MakeBoxWireframe(0, 0, 0, 5, 5, 5).ToMesh();
                var mesh2 = new Graphics.Mesh.UMesh();
                {
                    var materials1 = new Graphics.Pipeline.Shader.UMaterialInstance[1];
                    materials1[0] = await UEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(RName.GetRName("utest/box_wite.uminst"));
                    var colorVar = materials1[0].FindVar("clr4_0");
                    if (colorVar != null)
                    {
                        colorVar.Value = "1,0,1,1";
                    }
                    ok = mesh2.Initialize(cookedMesh, materials1, Rtti.UTypeDescGetter<Graphics.Mesh.UMdfStaticMesh>.TypeDesc);
                    if (ok)
                    {   
                        //var trans = Matrix.Scaling(1);
                        //mesh.SetWorldMatrix(ref trans);
                        var boxNode = GamePlay.Scene.UMeshNode.AddMeshNode(root, new GamePlay.Scene.UNodeData(), typeof(GamePlay.UPlacement), mesh2, Vector3.Zero, new Vector3(1.0f), Quaternion.Identity);
                        boxNode.NodeData.Name = "BoxNode";
                        boxNode.HitproxyType = Graphics.Pipeline.UHitProxy.EHitproxyType.Root;
                    }
                }

                var terrainMesh = new Graphics.Mesh.UMesh();
                var grid = Graphics.Mesh.CMeshDataProvider.MakeGridIndices(127, 127);
                var gridMesh = grid.ToMesh();
                var tMaterials = new Graphics.Pipeline.Shader.UMaterialInstance[1];
                tMaterials[0] = await UEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(RName.GetRName("utest/ground.uminst"));
                //tMaterials[0].SaveAssetTo(tMaterials[1].AssetName);
                ok = terrainMesh.Initialize(gridMesh, tMaterials, Rtti.UTypeDescGetter<Graphics.Mesh.UMdfTerrainMesh>.TypeDesc);
                if (ok)
                {
                    //var trAttach = new Graphics.Pipeline.Mobile.UBasePassTerrain.UTerrainAttachment();
                    var oriImage = new Bricks.Procedure.Buffer2D.Image();
                    oriImage.SetSize(128, 128, 0);
                    var perlin = new Bricks.Procedure.Buffer2D.NoisePerlin();
                    perlin.Amptitude = 1500;
                    perlin.Freq = 0.02f;
                    perlin.Octaves = 3;
                    perlin.Process(oriImage);

                    var perlin2 = new Bricks.Procedure.Buffer2D.NoisePerlin();
                    perlin2.Amptitude = 300;
                    perlin2.Freq = 0.82f;
                    perlin2.Octaves = 3;
                    perlin2.Process(oriImage);

                    var addOp = new Bricks.Procedure.Buffer2D.PixelAdd();
                    addOp.Process(perlin.mResultImage, perlin2.mResultImage);

                    var mdfTerrain = terrainMesh.MdfQueue as Graphics.Mesh.UMdfTerrainMesh;
                    mdfTerrain.HeightMapRSV = addOp.mResultImage.CreateAsTexture2D();//await UEngine.Instance.GfxDevice.TextureManager.GetTexture(RName.GetRName("utest/taxi.srv"));
                    //terrainMesh.Tag = trAttach;
                    float minH, maxH;
                    addOp.mResultImage.GetHeghtRange(out minH, out maxH);
                    var aabb = new BoundingBox(0, minH * mdfTerrain.HeightStep, 0, 128.0f * mdfTerrain.GridSize, maxH * mdfTerrain.HeightStep, 128.0f * mdfTerrain.GridSize);
                    terrainMesh.MaterialMesh.Mesh.mCoreObject.SetAABB(ref aabb);
                    //terrainMesh.SetWorldMatrix(ref Matrix.mIdentity);
                    //mainEditor.WorldViewportSlate.RenderPolicy.VisibleMeshes.Add(terrainMesh);
                    //mainEditor.WorldViewportSlate.World.Root.AddMesh(terrainMesh, Vector3.Zero, Vector3.UnitXYZ, Quaternion.Identity);
                    var trNode = GamePlay.Scene.UMeshNode.AddMeshNode(root, new GamePlay.Scene.UNodeData(), typeof(GamePlay.UPlacement), terrainMesh, Vector3.Zero, new Vector3(1.0f), Quaternion.Identity);
                    trNode.HitproxyType = Graphics.Pipeline.UHitProxy.EHitproxyType.Root;
                    trNode.IsAcceptShadow = true;
                }

                mainEditor.GetWorldViewportSlate().ShowBoundVolumes(true, null);
            }
        }
        async System.Threading.Tasks.Task DoTest2()
        {
            await Thread.AsyncDummyClass.DummyFunc();
            var kk = new Rtti.MetaAttribute();
            System.GC.Collect();
            //var root = UEngine.Instance.FileManager.GetRoot(IO.FileManager.ERootDir.Current);

            //UEngine.Instance.MacrossModule.ReloadAssembly(root + "/net5.0/GameProject.dll");

            //var typeDesc = Rtti.UTypeDescManager.Instance.GetTypeDescFromFullName("utest.mt");
            //var ins = System.Activator.CreateInstance(typeDesc.SystemType);
            //var method = typeDesc.SystemType.GetMethod("Function_0");
            //method.Invoke(ins, null);
        }
        async System.Threading.Tasks.Task DoTest3()
        {
            await UEngine.Instance.StartPlayInEditor(UEngine.Instance.GfxDevice.MainWindow, typeof(Graphics.Pipeline.Mobile.UMobileEditorFSPolicy), RName.GetRName("Demo0.mcrs"));
        }
        async System.Threading.Tasks.Task DoTest4()
        {
            await Thread.AsyncDummyClass.DummyFunc();
            UEngine.Instance.EndPlayInEditor();
        }
        async System.Threading.Tasks.Task MacrossRun()
        {
            await Thread.AsyncDummyClass.DummyFunc();
            Macross.UMacrossDebugger.Instance.Run();
        }
        async System.Threading.Tasks.Task MacrossDisableAllBreaks()
        {
            await Thread.AsyncDummyClass.DummyFunc();
            Macross.UMacrossDebugger.Instance.SetBreakStateAll(false);
        }
        async System.Threading.Tasks.Task MacrossEnableAllBreaks()
        {
            await Thread.AsyncDummyClass.DummyFunc();
            Macross.UMacrossDebugger.Instance.SetBreakStateAll(true);
        }
        public unsafe void OnDraw()
        {
            if (Visible == false)
                return;
            ImGuiAPI.SetNextWindowDockID(DockId, DockCond);
            ImGuiTreeNodeFlags_ flags = ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Leaf | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_NoTreePushOnOpen | ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_Bullet;
            if (ImGuiAPI.Begin("MetaView", null, ImGuiWindowFlags_.ImGuiWindowFlags_None))
            {
                if (ImGuiAPI.IsWindowDocked())
                {
                    DockId = ImGuiAPI.GetWindowDockID();
                }
                var sz = new Vector2(-1, 0);
                if (ImGuiAPI.Button("DoTest", ref sz))
                {
                    var nu = DoTest();
                }
                if (ImGuiAPI.Button("DoTest2", ref sz))
                {
                    var nu = DoTest2();
                }
                if (ImGuiAPI.Button("StartPIE", ref sz))
                {
                    var nu = DoTest3();
                }
                if (ImGuiAPI.Button("EndPIE", ref sz))
                {
                    var nu = DoTest4();
                }
                if (ImGuiAPI.Button("Macross GO!", ref sz))
                {
                    var nu = MacrossRun();
                }
                if (ImGuiAPI.Button("DisableAllBreaks", ref sz))
                {
                    var nu = MacrossDisableAllBreaks();
                }
                if (ImGuiAPI.Button("EnableAllBreaks", ref sz))
                {
                    var nu = MacrossEnableAllBreaks();
                }
                if (ImGuiAPI.CollapsingHeader("ClassView", ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_None))
                {
                    ImGuiAPI.PushItemWidth(-1);
                    var buffer = BigStackBuffer.CreateInstance(256);
                    buffer.SetText(mFilterText);
                    ImGuiAPI.InputText("##in", buffer.GetBuffer(), (uint)buffer.GetSize(), ImGuiInputTextFlags_.ImGuiInputTextFlags_None, null, (void*)0);
                    mFilterText = buffer.AsText();
                    buffer.DestroyMe();

                    ImGuiAPI.Separator();

                    foreach (var i in Rtti.UClassMetaManager.Instance.Metas)
                    {
                        if (string.IsNullOrEmpty(mFilterText) == false && i.Key.Contains(mFilterText) == false)
                        {
                            continue;
                        }
                        if (ImGuiAPI.TreeNode(i.Key))
                        {
                            if (ImGuiAPI.IsItemClicked(ImGuiMouseButton_.ImGuiMouseButton_Left))
                            {
                                PGMember.Target = i.Value;
                            }
                            foreach (var j in i.Value.MetaVersions)
                            {
                                if (ImGuiAPI.TreeNodeEx(j.Key.ToString(), flags))
                                {
                                    if (ImGuiAPI.IsItemClicked(ImGuiMouseButton_.ImGuiMouseButton_Left))
                                    {
                                        PGMember.Target = j.Value;
                                    }
                                }
                            }
                            ImGuiAPI.TreePop();
                        }
                    }
                }
                if (ImGuiAPI.CollapsingHeader("Property", ImGuiTreeNodeFlags_.ImGuiTreeNodeFlags_None))
                {
                    PGMember.OnDraw(false, false, false);
                }
            }
            ImGuiAPI.End();
        }
    }
}
