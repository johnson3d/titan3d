using EngineNS.GamePlay.Character;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.Editor
{
    public class UMetaViewEditor : Graphics.Pipeline.IRootForm
    {
        public UMetaViewEditor()
        {
            Editor.UMainEditorApplication.RegRootForm(this);
        }
        public void Cleanup()
        {

        }
        public async System.Threading.Tasks.Task<bool> Initialize()
        {
            await PGMember.Initialize();
            return true;
        }
        public bool Visible { get; set; } = true;
        public uint DockId { get; set; }
        public ImGuiCond_ DockCond { get; set; } = ImGuiCond_.ImGuiCond_FirstUseEver;
        public EGui.Controls.PropertyGrid.PropertyGrid PGMember = new EGui.Controls.PropertyGrid.PropertyGrid() { IsReadOnly = true };
        public string mFilterText;
        async System.Threading.Tasks.Task DoTest()
        {            
            if (false)
            {
                //var shape = Graphics.Mesh.CMeshDataProvider.MakeRect2D(-1, -1, 2, 2, 0.0F, false);
                var shape = Graphics.Mesh.CMeshDataProvider.MakeSphere(1, 20, 20, 0xFFFFFFFF);
                var shapeMesh = shape.ToMesh();
                shapeMesh.AssetName = RName.GetRName("mesh/base/sphere.vms", RName.ERNameType.Engine);
                shapeMesh.SaveAssetTo(shapeMesh.AssetName);

                var ameta = shapeMesh.CreateAMeta();
                ameta.SetAssetName(shapeMesh.AssetName);
                ameta.AssetId = Guid.NewGuid();
                ameta.TypeStr = Rtti.UTypeDescManager.Instance.GetTypeStringFromType(shapeMesh.GetType());
                ameta.Description = $"This is a sphere;//{shapeMesh.GetType().FullName}";
                ameta.SaveAMeta();
            }
            else
            {
                var mainEditor = UEngine.Instance.GfxDevice.MainWindow as Graphics.Pipeline.USlateApplication;
                var viewport = mainEditor.GetWorldViewportSlate();
                var world = viewport.World;
                var root = viewport.World.Root.FindFirstChild("DrawMeshNode");
                if (root == null)
                {
                    var scene = world.Root.GetNearestParentScene();
                    root = await scene.NewNode(world, typeof(GamePlay.Scene.USceneActorNode), new GamePlay.Scene.UNodeData() { Name = "DrawMeshNode" }, GamePlay.Scene.EBoundVolumeType.Box, typeof(GamePlay.UPlacement));
                    root.Parent = world.Root;
                }
                root.ClearChildren();
                root.SetStyle(GamePlay.Scene.UNode.ENodeStyles.VisibleFollowParent);

                var Offset = new DVector3(100000000.0 + 0.872, 0, 0);
                Offset.X = 0;
                root.Placement.Position = Offset;

                viewport.SetCameraOffset(Offset);
                viewport.CameraController.Camera.mCoreObject.LookAtLH(Offset + new DVector3(0, 0, -20), in Offset, in Vector3.Up);

                //Offset.X += 30.25;
                //viewport.SetCameraOffset(Offset);

                await TestCreateScene(world, root, false);

                mainEditor.GetWorldViewportSlate().ShowBoundVolumes(true, null);
            }
        }

        public static async System.Threading.Tasks.Task TestCreateScene(GamePlay.UWorld world, GamePlay.Scene.UNode root, bool hideTerrain = false)
        {
            var materials = new Graphics.Pipeline.Shader.UMaterialInstance[2];
            materials[0] = await UEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(RName.GetRName("utest/ddd.uminst"));
            materials[1] = await UEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(RName.GetRName("utest/ground.uminst"));
            if (materials[0] == null)
                return;

            {
                var meshData = new GamePlay.Scene.UMeshNode.UMeshNodeData();
                meshData.MeshName = RName.GetRName("utest/mesh/skysphere001.ums");
                var meshNode = new GamePlay.Scene.USkyNode();
                await meshNode.InitializeNode(world, meshData, GamePlay.Scene.EBoundVolumeType.Box, typeof(GamePlay.UPlacement));
                meshNode.Parent = root;
                meshNode.Placement.Scale = new Vector3(800.0f);
                meshNode.Placement.Position = DVector3.Zero;
                meshNode.HitproxyType = Graphics.Pipeline.UHitProxy.EHitproxyType.None;
                meshNode.NodeData.Name = "SkySphere";
                meshNode.IsAcceptShadow = false;
                meshNode.IsCastShadow = false;
            }

            {
                var meshData = new GamePlay.Scene.UMeshNode.UMeshNodeData();
                meshData.MeshName = RName.GetRName("utest/puppet/mesh/puppet.ums");
                meshData.CollideName = RName.GetRName("utest/puppet/mesh/puppet.vms");
                var meshNode = new GamePlay.Scene.UMeshNode();
                await meshNode.InitializeNode(world, meshData, GamePlay.Scene.EBoundVolumeType.Box, typeof(GamePlay.UPlacement));
                meshNode.Parent = root;
                meshNode.Placement.SetTransform(new DVector3(0, 0, 0), new Vector3(0.01f), Quaternion.Identity);
                meshNode.HitproxyType = Graphics.Pipeline.UHitProxy.EHitproxyType.Root;
                meshNode.NodeData.Name = "Robot0";
                meshNode.IsAcceptShadow = false;
                meshNode.IsCastShadow = true;

                {
                    var mesh1 = new Graphics.Mesh.UMesh();
                    var puppetMesh = await UEngine.Instance.GfxDevice.MeshPrimitiveManager.GetMeshPrimitive(RName.GetRName("utest/puppet/mesh/puppet.vms"));
                    var materialMesh = await UEngine.Instance.GfxDevice.MaterialMeshManager.GetMaterialMesh(RName.GetRName("utest/puppet/mesh/puppet.ums"));
                    if (materialMesh == null)
                    {
                        materialMesh = UEngine.Instance.AssetMetaManager.NewAsset<Graphics.Mesh.UMaterialMesh>(RName.GetRName("utest/puppet/mesh/puppet.ums"));
                        materialMesh.Initialize(puppetMesh, materials);
                        materialMesh.SaveAssetTo(RName.GetRName("utest/puppet/mesh/puppet.ums"));
                    }
                    mesh1.Initialize(puppetMesh, materials, Rtti.UTypeDesc.TypeOf(typeof(Graphics.Mesh.UMdfSkinMesh)));
                    var meshData1 = new GamePlay.Scene.UMeshNode.UMeshNodeData();                    
                    var meshNode1 = new GamePlay.Scene.UMeshNode();
                    await meshNode1.InitializeNode(world, meshData1, GamePlay.Scene.EBoundVolumeType.Box, typeof(GamePlay.UPlacement));
                    meshNode1.Mesh = mesh1;
                    meshNode1.NodeData.Name = "Robot1";                    
                    meshNode1.Parent = meshNode;
                    meshNode1.Placement.SetTransform(new DVector3(3, 3, 3), new Vector3(0.01f), Quaternion.RotationAxis(Vector3.UnitY, (float)Math.PI / 4));
                    meshNode1.HitproxyType = Graphics.Pipeline.UHitProxy.EHitproxyType.FollowParent;
                    meshNode1.IsAcceptShadow = false;
                    meshNode1.IsCastShadow = true;

                    (meshNode1.NodeData as GamePlay.Scene.UMeshNode.UMeshNodeData).MeshName = RName.GetRName("utest/puppet/mesh/puppet.ums");
                    (meshNode1.NodeData as GamePlay.Scene.UMeshNode.UMeshNodeData).MdfQueueType = Rtti.UTypeDesc.TypeStr(typeof(Graphics.Mesh.UMdfSkinMesh));
                    (meshNode1.NodeData as GamePlay.Scene.UMeshNode.UMeshNodeData).AtomType = Rtti.UTypeDesc.TypeStr(typeof(Graphics.Mesh.UMesh.UAtom));

                    var sapnd = new Animation.SceneNode.USkeletonAnimPlayNode.USkeletonAnimPlayNodeData();
                    sapnd.Name = "PlayAnim";
                    sapnd.AnimatinName = RName.GetRName("utest/puppet/animation/w2_stand_aim_idle_ip.animclip");
                    await Animation.SceneNode.USkeletonAnimPlayNode.AddSkeletonAnimPlayNode(world, meshNode1, sapnd, GamePlay.Scene.EBoundVolumeType.Box, typeof(GamePlay.UIdentityPlacement));
                }
            }

            var materials1 = new Graphics.Pipeline.Shader.UMaterialInstance[1];
            materials1[0] = await UEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(RName.GetRName("utest/ddd.uminst"));

            //var cookedMesh = Graphics.Mesh.CMeshDataProvider.MakeBoxWireframe(0, 0, 0, 5, 5, 5).ToMesh();
            //var cookedMesh = Graphics.Mesh.CMeshDataProvider.MakeSphere(2.5f, 100, 100, 0xfffffff).ToMesh();
            var cookMeshProvider = Graphics.Mesh.CMeshDataProvider.MakeCylinder(2.0f, 0.5f, 3.0f, 100, 100, 0xfffffff);
            var cookedMesh = cookMeshProvider.ToMesh();
            //var cookedMesh = Graphics.Mesh.CMeshDataProvider.MakeTorus(2.0f, 3.0f, 100, 300, 0xfffffff).ToMesh(); 
            //var cookedMesh = Graphics.Mesh.CMeshDataProvider.MakeCapsule(1.0f, 4.0f, 100, 100, 100, Graphics.Mesh.CMeshDataProvider.ECapsuleUvProfile.Aspect, 0xfffffff).ToMesh();
            {
                var mesh2 = new Graphics.Mesh.UMesh();
                var colorVar = materials1[0].FindVar("clr4_0");
                if (colorVar != null)
                {
                    colorVar.Value = "1,0,1,1";
                }
                var ok1 = mesh2.Initialize(cookedMesh, materials1, Rtti.UTypeDesc.TypeOf(typeof(Graphics.Mesh.UMdfStaticMesh)));
                if (ok1)
                {
                    var boxNode = await GamePlay.Scene.UMeshNode.AddMeshNode(world, root, new GamePlay.Scene.UMeshNode.UMeshNodeData(), typeof(GamePlay.UPlacement), mesh2,
                        DVector3.Zero, Vector3.UnitXYZ, Quaternion.Identity);
                    boxNode.NodeData.Name = "BoxNode";
                    boxNode.HitproxyType = Graphics.Pipeline.UHitProxy.EHitproxyType.Root;
                    boxNode.IsCastShadow = true;
                    boxNode.IsAcceptShadow = true;

                    //var pxNode = new Bricks.PhysicsCore.URigidBodyNode();
                    //var rgData = new Bricks.PhysicsCore.URigidBodyNode.URigidBodyNodeData();
                    //rgData.PxActorType = EPhyActorType.PAT_Static;
                    //var pc = UEngine.Instance.PhyModue.PhyContext;

                    //var pxTriMesh = pc.CookTriMesh(cookMeshProvider, null, null, null);
                    //var pxMtls = new Bricks.PhysicsCore.UPhyMaterial[1];
                    //pxMtls[0] = pc.PhyMaterialManager.DefaultMaterial;
                    ////var pxShape = pc.CreateShapeTriMesh(pxMtls, pxTriMesh, Vector3.UnitXYZ, Quaternion.Identity);
                    //var pxShape = pc.CreateShapeBox(pxMtls[0], new Vector3(20,30,10));
                    //pxShape.mCoreObject.SetFlag(EPhysShapeFlag.eVISUALIZATION, true);
                    //pxShape.mCoreObject.SetFlag(EPhysShapeFlag.eSCENE_QUERY_SHAPE, true);
                    //pxShape.mCoreObject.SetFlag(EPhysShapeFlag.eSIMULATION_SHAPE, true);
                    //rgData.PxShapes.Add(pxShape);
                    //await pxNode.InitializeNode(world, rgData, GamePlay.Scene.EBoundVolumeType.Box, typeof(GamePlay.UPlacementBase));
                    //pxNode.Parent = boxNode;

                    boxNode.Placement.Position = new DVector3(0, 0, 0);

                    
                }
            }

            {
                var mesh2 = new Graphics.Mesh.UMesh();
                mesh2.Initialize(cookedMesh, materials1, Rtti.UTypeDesc.TypeOf(typeof(Graphics.Mesh.UMdfStaticMesh)));
                var boxNodeFar = await GamePlay.Scene.UMeshNode.AddMeshNode(world, root, new GamePlay.Scene.UMeshNode.UMeshNodeData(), typeof(GamePlay.UPlacement), mesh2,
                        DVector3.Zero, Vector3.UnitXYZ, Quaternion.Identity);
                boxNodeFar.NodeData.Name = "BoxNodeFarFar";
                boxNodeFar.HitproxyType = Graphics.Pipeline.UHitProxy.EHitproxyType.Root;
                boxNodeFar.IsCastShadow = true;

                boxNodeFar.Placement.Position = new DVector3(1024 * 100, 0, 0);
            }

            {
                var nebulaData = new Bricks.Particle.Simple.USimpleNebulaNode.UNebulaNodeData();
                nebulaData.MeshName = RName.GetRName("utest/mesh/unit_sphere.ums");
                var meshNode = new Bricks.Particle.Simple.USimpleNebulaNode();
                await meshNode.InitializeNode(world, nebulaData, GamePlay.Scene.EBoundVolumeType.Box, typeof(GamePlay.UPlacement));
                meshNode.Parent = root;                
                meshNode.Placement.Position = DVector3.Zero;
                meshNode.HitproxyType = Graphics.Pipeline.UHitProxy.EHitproxyType.None;
                meshNode.NodeData.Name = "NebulaParticle";
                meshNode.IsAcceptShadow = false;
                meshNode.IsCastShadow = false;
            }

            {
                var lightData = new GamePlay.Scene.UPointLightNode.ULightNodeData();
                lightData.Name = "PointLight0";
                lightData.Intensity = 100.0f;
                lightData.Radius = 20.0f;
                lightData.Color = new Vector3(1, 0, 0);
                var lightNode = GamePlay.Scene.UPointLightNode.AddPointLightNode(world, root, lightData, new DVector3(10, 10, 10));
            }
            
            if (hideTerrain == false)
            {
                var terrainNode = new Bricks.Terrain.CDLOD.UTerrainNode();
                var terrainData = new Bricks.Terrain.CDLOD.UTerrainNode.UTerrainData();
                terrainData.Name = "TerrainGen";
                await terrainNode.InitializeNode(world, terrainData, GamePlay.Scene.EBoundVolumeType.Box, typeof(GamePlay.UPlacement));
                terrainNode.Parent = root;
                terrainNode.Placement.Position = DVector3.Zero;
                terrainNode.IsAcceptShadow = true;
                terrainNode.SetActiveCenter(in DVector3.Zero);
            }
            
            var gridNode = await GamePlay.Scene.UGridNode.AddGridNode(world, root);
            //gridNode.SetStyle(GamePlay.Scene.UNode.ENodeStyles.Invisible);

            var app = UEngine.Instance.GfxDevice.MainWindow as Graphics.Pipeline.USlateApplication;
            gridNode.ViewportSlate = app.GetWorldViewportSlate();
        }

        public static async System.Threading.Tasks.Task TestCreateCharacter(GamePlay.UWorld world, GamePlay.Scene.UNode root, bool hideTerrain = false)
        {
            var characterController = new GamePlay.Controller.UCharacterController();
            var player = new GamePlay.Player.UPlayer();
            var playerData = new GamePlay.Player.UPlayer.UPlayerData() { CharacterController = characterController };
            await player.InitializeNode(world, playerData, GamePlay.Scene.EBoundVolumeType.Box, typeof(GamePlay.UPlacement));
            player.Parent = root;

            var character = new GamePlay.Character.UCharacter();
            await character.InitializeNode(world, new GamePlay.Character.UCharacter.UCharacterData(), GamePlay.Scene.EBoundVolumeType.Box, typeof(GamePlay.UPlacement));
            characterController.ControlledCharacter = character;
            var movement = new GamePlay.Movemnet.UMovement();
            movement.Parent = character;

        }


        async System.Threading.Tasks.Task DoTest2()
        {
            await Thread.AsyncDummyClass.DummyFunc();

            var mainEditor = UEngine.Instance.GfxDevice.MainWindow as Graphics.Pipeline.USlateApplication;
            var viewport = mainEditor.GetWorldViewportSlate();

            var curPos = viewport.CameraController.Camera.mCoreObject.GetPosition();

            DVector3 Offset = new DVector3(1024 * 100, 0, 0);
            if (DVector3.Distance(in curPos, in Offset) < 100)
            {
                Offset = DVector3.Zero;
                viewport.SetCameraOffset(Offset);
                viewport.CameraController.Camera.mCoreObject.LookAtLH(Offset + new DVector3(0, 0, -20), in Offset, in Vector3.Up);
            }
            else
            {
                viewport.SetCameraOffset(Offset);
                viewport.CameraController.Camera.mCoreObject.LookAtLH(Offset + new DVector3(0, 0, -20), in Offset, in Vector3.Up);
            }

            //var kk = new Rtti.MetaAttribute();
            //System.GC.Collect();
            //var root = UEngine.Instance.FileManager.GetRoot(IO.FileManager.ERootDir.Current);

            //UEngine.Instance.MacrossModule.ReloadAssembly(root + "/net5.0/GameProject.dll");

            //var typeDesc = Rtti.UTypeDescManager.Instance.GetTypeDescFromFullName("utest.mt");
            //var ins = Rtti.UTypeDescManager.CreateInstance(typeDesc.SystemType);
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
                if (ImGuiAPI.Button("DoTest", in sz))
                {
                    var nu = DoTest();
                }
                if (ImGuiAPI.Button("DoTest2", in sz))
                {
                    var nu = DoTest2();
                }
                if (ImGuiAPI.Button("StartPIE", in sz))
                {
                    var nu = DoTest3();
                }
                if (ImGuiAPI.Button("EndPIE", in sz))
                {
                    var nu = DoTest4();
                }
                if (ImGuiAPI.Button("Macross GO!", in sz))
                {
                    var nu = MacrossRun();
                }
                if (ImGuiAPI.Button("DisableAllBreaks", in sz))
                {
                    var nu = MacrossDisableAllBreaks();
                }
                if (ImGuiAPI.Button("EnableAllBreaks", in sz))
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
