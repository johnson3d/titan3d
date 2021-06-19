using System;
using System.Collections.Generic;
using EngineNS;

namespace EngineNS.Rtti
{
    public class AssemblyEntry
    {
        public class GameAssemblyDesc : AssemblyDesc
        {
            public GameAssemblyDesc()
            {
                Profiler.Log.WriteLine(Profiler.ELogTag.Info, "Core", "GameAssemblyDesc Created");
            }
            ~GameAssemblyDesc()
            {
                Profiler.Log.WriteLine(Profiler.ELogTag.Info, "Core", "GameAssemblyDesc Destroyed");
            }
            public override string Name { get => "GameProject"; }
            public override string Service { get { return "Game"; } }
            public override bool IsGameModule { get { return true; } }
            public override string Platform { get { return "Windows"; } }

            public override object CreateInstance(RName name)
            {
                if (name.Name == "demo0.mcrs")
                {
                    return new GameProject.Demo0Game();
                }
                return null;
            }
        }
        static GameAssemblyDesc AssmblyDesc = new GameAssemblyDesc();
        public static AssemblyDesc GetAssemblyDesc()
        {
            return AssmblyDesc;
        }
    }
}


namespace GameProject
{
    public class Demo0Game : EngineNS.GamePlay.IGameBase
    {
        ~Demo0Game()
        {

        }
        EngineNS.Macross.UMacrossStackFrame mFrame_BeginPlay = new EngineNS.Macross.UMacrossStackFrame();
        public override async System.Threading.Tasks.Task<bool> BeginPlay(EngineNS.GamePlay.UGameBase host)
        {
            using (var guard = new EngineNS.Macross.UMacrossStackGuard(mFrame_BeginPlay))
            {
                await base.BeginPlay(host);

                var app = UEngine.Instance.GfxDevice.MainWindow as EngineNS.Graphics.Pipeline.USlateApplication;
                var world = app.GetWorldViewportSlate().World;
                var root = world.Root.FindFirstChild("DrawMeshNode");
                if (root == null)
                {
                    var scene = world.Root.GetNearestParentScene();
                    root = scene.NewNode(typeof(EngineNS.GamePlay.Scene.UNode), new EngineNS.GamePlay.Scene.UNodeData() { Name = "DrawMeshNode" }, EngineNS.GamePlay.Scene.EBoundVolumeType.Box, typeof(EngineNS.GamePlay.UPlacementBase));
                    root.Parent = world.Root;
                }
                root.ClearChildren();

                await UEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(RName.GetRName("utest/box_wite.uminst"));

                var materials = new EngineNS.Graphics.Pipeline.Shader.UMaterialInstance[2];
                materials[0] = await UEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(RName.GetRName("utest/ddd.uminst"));
                materials[1] = await UEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(RName.GetRName("utest/ddd.uminst"));
                if (materials[0] == null)
                    return false;
                var puppetMesh = await UEngine.Instance.GfxDevice.MeshPrimitiveManager.GetMeshPrimitive(RName.GetRName("utest/mesh/puppet_low_ue4.vms"));
                var materialMesh = await UEngine.Instance.GfxDevice.MaterialMeshManager.GetMaterialMesh(RName.GetRName("utest/mesh/puppet_low_ue4.ums"));
                if (materialMesh == null)
                {
                    materialMesh = UEngine.Instance.AssetMetaManager.NewAsset<EngineNS.Graphics.Mesh.UMaterialMesh>(RName.GetRName("utest/mesh/puppet_low_ue4.ums"));
                    materialMesh.Initialize(puppetMesh, materials);
                    materialMesh.SaveAssetTo(RName.GetRName("utest/mesh/puppet_low_ue4.ums"));
                }

                var mesh = new EngineNS.Graphics.Mesh.UMesh();

                //var ok = mesh.Initialize(puppetMesh, materials, Rtti.UTypeDescGetter<Graphics.Mesh.UMdfStaticMesh>.TypeDesc);
                //var ok = mesh.Initialize(materialMesh, Rtti.UTypeDescGetter<Graphics.Mesh.UMdfStaticMesh>.TypeDesc);
                var ok = mesh.Initialize(materialMesh, EngineNS.Rtti.UTypeDescGetter<EngineNS.Graphics.Mesh.UMdfStaticMesh>.TypeDesc);
                if (ok)
                {
                    //var trans = Matrix.Scaling(0.01f);
                    //mesh.SetWorldMatrix(ref trans);// Matrix.mIdentity);

                    var meshNode = EngineNS.GamePlay.Scene.UMeshNode.AddMeshNode(root, new EngineNS.GamePlay.Scene.UNodeData(), typeof(EngineNS.GamePlay.UPlacement), mesh, new Vector3(5, 5, 5), new Vector3(0.01f), Quaternion.Identity);
                    meshNode.HitproxyType = EngineNS.Graphics.Pipeline.UHitProxy.EHitproxyType.Root;
                    meshNode.NodeData.Name = "Robot0";
                    meshNode.IsScaleChildren = false;
                    meshNode.IsCastShadow = true;
                    meshNode.IsAcceptShadow = true;

                    var mesh1 = new EngineNS.Graphics.Mesh.UMesh();

                    mesh1.Initialize(puppetMesh, materials, EngineNS.Rtti.UTypeDescGetter<EngineNS.Graphics.Mesh.UMdfStaticMesh>.TypeDesc);
                    var meshNode1 = EngineNS.GamePlay.Scene.UMeshNode.AddMeshNode(meshNode, new EngineNS.GamePlay.Scene.UNodeData(), typeof(EngineNS.GamePlay.UPlacement), mesh1, new Vector3(3, 3, 3), new Vector3(0.01f), Quaternion.RotationAxis(Vector3.UnitY, (float)Math.PI / 4));// Quaternion.Identity);
                    meshNode1.NodeData.Name = "Robot1";
                    meshNode1.Parent = meshNode;
                    meshNode1.HitproxyType = EngineNS.Graphics.Pipeline.UHitProxy.EHitproxyType.FollowParent;
                    //meshNode1.Placement.Position = meshNode1.Placement.Position;
                }

                var terrainMesh = new EngineNS.Graphics.Mesh.UMesh();
                var grid = EngineNS.Graphics.Mesh.CMeshDataProvider.MakeGridIndices(127, 127);
                var gridMesh = grid.ToMesh();
                var tMaterials = new EngineNS.Graphics.Pipeline.Shader.UMaterialInstance[1];
                tMaterials[0] = await UEngine.Instance.GfxDevice.MaterialInstanceManager.GetMaterialInstance(RName.GetRName("utest/ddd.uminst"));
                ok = terrainMesh.Initialize(gridMesh, tMaterials, EngineNS.Rtti.UTypeDescGetter<EngineNS.Graphics.Mesh.UMdfTerrainMesh>.TypeDesc);
                if (ok)
                {
                    //var trAttach = new Graphics.Pipeline.Mobile.UBasePassTerrain.UTerrainAttachment();
                    var oriImage = new EngineNS.Bricks.Procedure.Buffer2D.Image();
                    oriImage.SetSize(128, 128, 0);
                    var perlin = new EngineNS.Bricks.Procedure.Buffer2D.NoisePerlin();
                    perlin.Amptitude = 1500;
                    perlin.Freq = 0.02f;
                    perlin.Octaves = 3;
                    perlin.Process(oriImage);

                    var perlin2 = new EngineNS.Bricks.Procedure.Buffer2D.NoisePerlin();
                    perlin2.Amptitude = 300;
                    perlin2.Freq = 0.82f;
                    perlin2.Octaves = 3;
                    perlin2.Process(oriImage);

                    var addOp = new EngineNS.Bricks.Procedure.Buffer2D.PixelAdd();
                    addOp.Process(perlin.mResultImage, perlin2.mResultImage);

                    var mdfTerrain = terrainMesh.MdfQueue as EngineNS.Graphics.Mesh.UMdfTerrainMesh;
                    mdfTerrain.HeightMapRSV = addOp.mResultImage.CreateAsTexture2D();//await UEngine.Instance.GfxDevice.TextureManager.GetTexture(RName.GetRName("utest/taxi.srv"));
                    //terrainMesh.Tag = trAttach;
                    float minH, maxH;
                    addOp.mResultImage.GetHeghtRange(out minH, out maxH);
                    var aabb = new BoundingBox(0, minH * mdfTerrain.HeightStep, 0, 128.0f * mdfTerrain.GridSize, maxH * mdfTerrain.HeightStep, 128.0f * mdfTerrain.GridSize);
                    terrainMesh.MaterialMesh.Mesh.mCoreObject.SetAABB(ref aabb);
                    //terrainMesh.SetWorldMatrix(ref Matrix.mIdentity);
                    //mainEditor.WorldViewportSlate.RenderPolicy.VisibleMeshes.Add(terrainMesh);
                    //mainEditor.WorldViewportSlate.World.Root.AddMesh(terrainMesh, Vector3.Zero, Vector3.UnitXYZ, Quaternion.Identity);
                    var trNode = EngineNS.GamePlay.Scene.UMeshNode.AddMeshNode(root, new EngineNS.GamePlay.Scene.UNodeData(), typeof(EngineNS.GamePlay.UPlacement), terrainMesh, Vector3.Zero, new Vector3(1.0f), Quaternion.Identity);
                    trNode.HitproxyType = EngineNS.Graphics.Pipeline.UHitProxy.EHitproxyType.Root;
                    trNode.IsAcceptShadow = true;
                }

                app.GetWorldViewportSlate().ShowBoundVolumes(true, null);
                return true;
            }
        }
        public override void Tick(EngineNS.GamePlay.UGameBase host, int elapsedMillisecond)
        {
            base.Tick(host, elapsedMillisecond);
        }
        public override void BeginDestroy(EngineNS.GamePlay.UGameBase host)
        {
            base.BeginDestroy(host);
        }
    }
}
