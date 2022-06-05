using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.GamePlay
{
    [Macross.UMacross]
    public partial class UMacrossGame
    {
        [Rtti.Meta]
        public virtual async System.Threading.Tasks.Task<bool> BeginPlay(UGameInstance host)
        {
            await host.InitViewportSlate(UEngine.Instance.Config.MainRPolicyName);

            return true;
        }
        [Rtti.Meta]
        public virtual void Tick(UGameInstance host, int elapsedMillisecond)
        {

        }
        [Rtti.Meta]
        public virtual void BeginDestroy(UGameInstance host)
        {
            host.FinalViewportSlate();
        }
    }
    [Rtti.Meta(Flags = Rtti.MetaAttribute.EMetaFlags.NoMacrossCreate)]
    public partial class UGameInstance : UModuleHost<UGameInstance>, ITickable
    {
        public virtual void TickLogic(int ellapse)
        {
            WorldViewportSlate?.TickLogic(ellapse);
        }
        public virtual void TickRender(int ellapse)
        {
            WorldViewportSlate?.TickRender(ellapse);
        }
        public virtual void TickSync(int ellapse)
        {
            WorldViewportSlate?.TickSync(ellapse);
        }

        [Rtti.Meta]
        public UGameViewportSlate WorldViewportSlate { get; } = new UGameViewportSlate(true);

        Macross.UMacrossGetter<UMacrossGame> mMcObject;
        public Macross.UMacrossGetter<UMacrossGame> McObject
        {
            get
            {
                if (mMcObject == null)
                    mMcObject = Macross.UMacrossGetter<UMacrossGame>.NewInstance();
                return mMcObject;
            }
        }
        protected override UGameInstance GetHost()
        {
            return this;
        }
        public virtual async System.Threading.Tasks.Task<bool> BeginPlay()
        {
            return await McObject?.Get()?.BeginPlay(this);
        }
        public virtual void Tick(int elapsedMillisecond)
        {
            McObject?.Get()?.Tick(this, elapsedMillisecond);
        }
        public virtual void BeginDestroy()
        {
            McObject?.Get()?.BeginDestroy(this);
        }
        [Rtti.Meta]
        public async System.Threading.Tasks.Task InitViewportSlate(
            [RName.PGRName(FilterExts = Bricks.RenderPolicyEditor.URenderPolicyAsset.AssetExt)]
            RName rPolicy, 
            float zMin = 0, float zMax = 1)
        {
            await WorldViewportSlate.Initialize(null, rPolicy, zMin, zMax);
            WorldViewportSlate.RenderPolicy.DisableShadow = false;
            UEngine.Instance.GfxDevice.MainWindow.NativeWindow.RegEventProcessor(WorldViewportSlate);
        }
        [Rtti.Meta]
        public void FinalViewportSlate()
        {
            UEngine.Instance.GfxDevice.MainWindow.NativeWindow.UnregEventProcessor(WorldViewportSlate);
            Editor.UMainEditorApplication.UnregRootForm(WorldViewportSlate);
        }

        [Rtti.Meta]
        public async System.Threading.Tasks.Task<bool> LoadScene(
            [RName.PGRName(FilterExts = GamePlay.Scene.UScene.AssetExt)]
            RName mapName)
        {
            var viewport = this.WorldViewportSlate;
            var world = viewport.World;

            var scene = await GamePlay.Scene.UScene.LoadScene(world, mapName);
            if (scene != null)
            {
                world.Root.ClearChildren();
                world.Root.SetStyle(GamePlay.Scene.UNode.ENodeStyles.VisibleFollowParent);
                scene.Parent = world.Root;

                //await CreateCharacter(WorldViewportSlate.World, WorldViewportSlate.World.Root);
                await CreateCharacter(WorldViewportSlate.World);
                return true;
            }
            return false;
        }
        public GamePlay.Character.UCharacter ChiefPlayer;
        [Rtti.Meta]
        public async System.Threading.Tasks.Task CreateCharacter(UWorld world)
        {
            EngineNS.GamePlay.Scene.UNode root = world.Root;
            var playerData = new EngineNS.GamePlay.Character.UCharacter.UCharacterData();
            ChiefPlayer = new EngineNS.GamePlay.Character.UCharacter();
            await ChiefPlayer.InitializeNode(WorldViewportSlate.World, playerData, EngineNS.GamePlay.Scene.EBoundVolumeType.Box, typeof(EngineNS.GamePlay.UPlacement));
            ChiefPlayer.Parent = root;
            ChiefPlayer.NodeData.Name = "UActor";
            ChiefPlayer.HitproxyType = EngineNS.Graphics.Pipeline.UHitProxy.EHitproxyType.None;
            ChiefPlayer.IsCastShadow = true;
            ChiefPlayer.SetStyle(EngineNS.GamePlay.Scene.UNode.ENodeStyles.VisibleFollowParent);

            var meshData1 = new EngineNS.GamePlay.Scene.UMeshNode.UMeshNodeData();
            meshData1.MeshName = RName.GetRName("utest/puppet/mesh/puppet.ums");
            meshData1.MdfQueueType = EngineNS.Rtti.UTypeDesc.TypeStr(typeof(EngineNS.Graphics.Mesh.UMdfSkinMesh));
            meshData1.AtomType = EngineNS.Rtti.UTypeDesc.TypeStr(typeof(EngineNS.Graphics.Mesh.UMesh.UAtom));
            var meshNode1 = new EngineNS.GamePlay.Scene.UMeshNode();
            await meshNode1.InitializeNode(world, meshData1, EngineNS.GamePlay.Scene.EBoundVolumeType.Box, typeof(EngineNS.GamePlay.UPlacement));
            meshNode1.NodeData.Name = "Robot1";
            meshNode1.Parent = ChiefPlayer;
            meshNode1.Placement.SetTransform(new DVector3(0.0f), new Vector3(0.01f), Quaternion.Identity);
            meshNode1.HitproxyType = EngineNS.Graphics.Pipeline.UHitProxy.EHitproxyType.FollowParent;
            meshNode1.IsAcceptShadow = false;
            meshNode1.IsCastShadow = true;

            //var sapnd = new EngineNS.Animation.SceneNode.USkeletonAnimPlayNode.USkeletonAnimPlayNodeData();
            //sapnd.Name = "PlayAnim";
            //sapnd.AnimatinName = RName.GetRName("utest/puppet/animation/w2_walk_aim_f_loop_ip.animclip");
            //await EngineNS.Animation.SceneNode.USkeletonAnimPlayNode.AddSkeletonAnimPlayNode(world, meshNode1, sapnd, EngineNS.GamePlay.Scene.EBoundVolumeType.Box, typeof(EngineNS.GamePlay.UIdentityPlacement));
            var sapnd = new EngineNS.Animation.SceneNode.UBlendSpaceAnimPlayNode.UBlendSpaceAnimPlayNodeData();
            sapnd.Name = "PlayAnim";
            sapnd.AnimatinName = RName.GetRName("utest/puppet/animation/w2_walk_aim_f_loop_ip.animclip");
            sapnd.OverrideAsset = true;
            sapnd.Axises.Add(new EngineNS.Animation.Asset.BlendSpace.UBlendSpace_Axis("Speed", -3, 3));
            sapnd.Axises.Add(new EngineNS.Animation.Asset.BlendSpace.UBlendSpace_Axis("V"));
            sapnd.Points.Add(new EngineNS.Animation.SceneNode.FBlendSpacePoint(RName.GetRName("utest/puppet/animation/w2_stand_aim_idle_ip.animclip"), Vector3.Zero));
            sapnd.Points.Add(new EngineNS.Animation.SceneNode.FBlendSpacePoint(RName.GetRName("utest/puppet/animation/w2_walk_aim_f_loop_ip.animclip"), new Vector3(3, 0, 0)));
            await EngineNS.Animation.SceneNode.UBlendSpaceAnimPlayNode.AddBlendSpace2DAnimPlayNode(world, meshNode1, sapnd, EngineNS.GamePlay.Scene.EBoundVolumeType.Box, typeof(EngineNS.GamePlay.UIdentityPlacement));

            var characterController = new EngineNS.GamePlay.Controller.UCharacterController();
            await characterController.InitializeNode(WorldViewportSlate.World, new EngineNS.GamePlay.Scene.UNodeData(), EngineNS.GamePlay.Scene.EBoundVolumeType.Box, typeof(EngineNS.GamePlay.UPlacement));
            characterController.Parent = world.Root;
            characterController.ControlledCharacter = ChiefPlayer;

            var movement = new EngineNS.GamePlay.Movemnet.UMovement();
            await movement.InitializeNode(WorldViewportSlate.World, new EngineNS.GamePlay.Scene.UNodeData() { Name = "Movement" }, EngineNS.GamePlay.Scene.EBoundVolumeType.Box, typeof(EngineNS.GamePlay.UPlacement));
            movement.Parent = ChiefPlayer;

            characterController.MovementNode = movement;

            var springArm = new EngineNS.GamePlay.Camera.UCameraSpringArm();
            var springArmData = new EngineNS.GamePlay.Camera.UCameraSpringArm.UCameraSpringArmData();
            //springArmData.TargetOffset = DVector3.Up * 1.5f;
            await springArm.InitializeNode(WorldViewportSlate.World, springArmData, EngineNS.GamePlay.Scene.EBoundVolumeType.Box, typeof(EngineNS.GamePlay.UPlacement));

            springArm.Parent = ChiefPlayer;

            characterController.CameraControlNode = springArm;

            var camera = new EngineNS.GamePlay.Camera.UCamera();
            await camera.InitializeNode(WorldViewportSlate.World, new EngineNS.GamePlay.Scene.UNodeData(), EngineNS.GamePlay.Scene.EBoundVolumeType.Box, typeof(EngineNS.GamePlay.UPlacement));
            camera.Parent = springArm;
            camera.Camera = WorldViewportSlate.RenderPolicy.DefaultCamera;
        }
    }
}

namespace EngineNS
{
    public partial class UEngine
    {
        [Rtti.Meta(Flags = Rtti.MetaAttribute.EMetaFlags.MacrossReadOnly)]
        public GamePlay.UGameInstance GameInstance
        {
            get;
            set;
        }
    }
}