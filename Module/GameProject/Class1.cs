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
