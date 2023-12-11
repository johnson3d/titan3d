using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EngineNS;
using EngineNS.GamePlay;

namespace GameProject
{
    public class GAloneGame : EngineNS.GamePlay.UMacrossGame
    {   
        EngineNS.Macross.UMacrossStackFrame mFrame_BeginPlay = new EngineNS.Macross.UMacrossStackFrame();
        public async override System.Threading.Tasks.Task<bool> BeginPlay(EngineNS.GamePlay.UGameInstance host)
        {
            //WorldViewportSlate = new EngineNS.GamePlay.UGameViewportSlate(true);

            //using (var guard = new EngineNS.Macross.UMacrossStackGuard(mFrame_BeginPlay))
            //{
            //    await base.BeginPlay(host);

            //    await EngineNS.Editor.UMainEditorApplication.TestCreateScene(WorldViewportSlate.World, WorldViewportSlate.World.Root, true);

            //    var world = WorldViewportSlate.World;
            //    var root = WorldViewportSlate.World.Root;
            //    WorldViewportSlate.ShowBoundVolumes(true, null);
            //    WorldViewportSlate.CameraMoveSpeed = 100.0f;
            //    WorldViewportSlate.CameraController.ControlCamera(null);
            //    var terrainData = new EngineNS.Bricks.Terrain.CDLOD.UTerrainNode.UTerrainData();
            //    //var ne    wNode = new EngineNS.Bricks.Terrain.CDLOD.UTerrainNode();
            //    var newNode = await root.NewNodeSimple(world, typeof(EngineNS.Bricks.Terrain.CDLOD.UTerrainNode), terrainData) as EngineNS.Bricks.Terrain.CDLOD.UTerrainNode;
            //    newNode.Parent = root;
            //    newNode.IsAcceptShadow = true;


            //    newNode.SetActiveCenter(new DVector3(30, 0, 30));
            //    var alt = newNode.GetAltitude(30, 30);
            //    alt += 10.0f;
            //    await CreateCharacter(world, root);
            //    ChiefPlayer.Placement.Position = new DVector3(30, alt, 30);

            //    return true;
            //}
            return true;
        }

        //public async System.Threading.Tasks.Task CreateCharacter(UWorld world, EngineNS.GamePlay.Scene.UNode root)
        //{
        //    var playerData = new EngineNS.GamePlay.Character.UCharacter.UCharacterData();
        //    ChiefPlayer = new GAlonePlayer();
        //    await ChiefPlayer.InitializeNode(WorldViewportSlate.World, playerData, EngineNS.GamePlay.Scene.EBoundVolumeType.Box, typeof(EngineNS.GamePlay.UPlacement));
        //    ChiefPlayer.Parent = root;
        //    ChiefPlayer.NodeData.Name = "UActor";
        //    ChiefPlayer.HitproxyType = EngineNS.Graphics.Pipeline.UHitProxy.EHitproxyType.None;
        //    ChiefPlayer.IsCastShadow = true;
        //    ChiefPlayer.SetStyle(EngineNS.GamePlay.Scene.UNode.ENodeStyles.VisibleFollowParent);

        //    var meshData1 = new EngineNS.GamePlay.Scene.UMeshNode.UMeshNodeData();
        //    meshData1.MeshName = RName.GetRName("utest/puppet/mesh/puppet.ums");
        //    meshData1.MdfQueueType = EngineNS.Rtti.UTypeDesc.TypeStr(typeof(EngineNS.Graphics.Mesh.UMdfSkinMesh));
        //    meshData1.AtomType = EngineNS.Rtti.UTypeDesc.TypeStr(typeof(EngineNS.Graphics.Mesh.TtMesh.UAtom));
        //    var meshNode1 = new EngineNS.GamePlay.Scene.UMeshNode();
        //    await meshNode1.InitializeNode(world, meshData1, EngineNS.GamePlay.Scene.EBoundVolumeType.Box, typeof(EngineNS.GamePlay.UPlacement));
        //    meshNode1.NodeData.Name = "Robot1";
        //    meshNode1.Parent = ChiefPlayer;
        //    meshNode1.Placement.SetTransform(new DVector3(0.0f), new Vector3(0.01f), Quaternion.Identity);
        //    meshNode1.HitproxyType = EngineNS.Graphics.Pipeline.UHitProxy.EHitproxyType.FollowParent;
        //    meshNode1.IsAcceptShadow = false;
        //    meshNode1.IsCastShadow = true;

        //    //var sapnd = new EngineNS.Animation.SceneNode.USkeletonAnimPlayNode.USkeletonAnimPlayNodeData();
        //    //sapnd.Name = "PlayAnim";
        //    //sapnd.AnimatinName = RName.GetRName("utest/puppet/animation/w2_walk_aim_f_loop_ip.animclip");
        //    //await EngineNS.Animation.SceneNode.USkeletonAnimPlayNode.AddSkeletonAnimPlayNode(world, meshNode1, sapnd, EngineNS.GamePlay.Scene.EBoundVolumeType.Box, typeof(EngineNS.GamePlay.UIdentityPlacement));
        //    var sapnd = new EngineNS.Animation.SceneNode.UBlendSpaceAnimPlayNode.UBlendSpaceAnimPlayNodeData();
        //    sapnd.Name = "PlayAnim";
        //    sapnd.AnimatinName = RName.GetRName("utest/puppet/animation/w2_walk_aim_f_loop_ip.animclip");
        //    sapnd.OverrideAsset = true;
        //    sapnd.Axises.Add(new EngineNS.Animation.Asset.BlendSpace.UBlendSpace_Axis("Speed", -3, 3));
        //    sapnd.Axises.Add(new EngineNS.Animation.Asset.BlendSpace.UBlendSpace_Axis("V"));
        //    sapnd.Points.Add(new EngineNS.Animation.SceneNode.FBlendSpacePoint(RName.GetRName("utest/puppet/animation/w2_stand_aim_idle_ip.animclip"), Vector3.Zero));
        //    sapnd.Points.Add(new EngineNS.Animation.SceneNode.FBlendSpacePoint(RName.GetRName("utest/puppet/animation/w2_walk_aim_f_loop_ip.animclip"), new Vector3(3, 0, 0)));
        //    await EngineNS.Animation.SceneNode.UBlendSpaceAnimPlayNode.AddBlendSpace2DAnimPlayNode(world, meshNode1, sapnd, EngineNS.GamePlay.Scene.EBoundVolumeType.Box, typeof(EngineNS.GamePlay.UIdentityPlacement));
            
        //    var characterController = new EngineNS.GamePlay.Controller.UCharacterController();
        //    await characterController.InitializeNode(WorldViewportSlate.World, new EngineNS.GamePlay.Scene.UNodeData(), EngineNS.GamePlay.Scene.EBoundVolumeType.Box, typeof(EngineNS.GamePlay.UPlacement));
        //    characterController.Parent = world.Root;
        //    characterController.ControlledCharacter = ChiefPlayer;

        //    var movement = new EngineNS.GamePlay.Movemnet.UMovement();
        //    await movement.InitializeNode(WorldViewportSlate.World, new EngineNS.GamePlay.Scene.UNodeData() { Name = "Movement" }, EngineNS.GamePlay.Scene.EBoundVolumeType.Box, typeof(EngineNS.GamePlay.UPlacement));
        //    movement.Parent = ChiefPlayer;

        //    characterController.MovementNode = movement;

        //    var springArm = new EngineNS.GamePlay.Camera.UCameraSpringArm();
        //    var springArmData = new EngineNS.GamePlay.Camera.UCameraSpringArm.UCameraSpringArmData();
        //    //springArmData.TargetOffset = DVector3.Up * 1.5f;
        //    await springArm.InitializeNode(WorldViewportSlate.World, springArmData, EngineNS.GamePlay.Scene.EBoundVolumeType.Box, typeof(EngineNS.GamePlay.UPlacement));
            
        //    springArm.Parent = ChiefPlayer;

        //    characterController.CameraControlNode = springArm;

        //    var camera = new EngineNS.GamePlay.Camera.UCamera();
        //    await camera.InitializeNode(WorldViewportSlate.World, new EngineNS.GamePlay.Scene.UNodeData(), EngineNS.GamePlay.Scene.EBoundVolumeType.Box, typeof(EngineNS.GamePlay.UPlacement));
        //    camera.Parent = springArm;
        //    camera.Camera = WorldViewportSlate.RenderPolicy.DefaultCamera;
        //}
        public override void Tick(EngineNS.GamePlay.UGameInstance host, float elapsedMillisecond)
        {
            base.Tick(host, elapsedMillisecond);
        }
        public override void BeginDestroy(EngineNS.GamePlay.UGameInstance host)
        {
            base.BeginDestroy(host);
        }
    }
}
