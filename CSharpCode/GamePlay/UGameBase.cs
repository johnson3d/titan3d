using EngineNS.Bricks.PhysicsCore.SceneNode;
using EngineNS.GamePlay.Camera;
using EngineNS.GamePlay.Movemnet;
using EngineNS.GamePlay.Player;
using EngineNS.GamePlay.Scene;
using EngineNS.GamePlay.Scene.Actor;
using EngineNS.IO;
using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.GamePlay
{
    [Macross.TtMacross]
    public partial class UMacrossGame
    {
        [Rtti.Meta]
        public virtual async System.Threading.Tasks.Task<bool> BeginPlay(UGameInstance host)
        {
            await host.InitViewportSlate(TtEngine.Instance.Config.MainRPolicyName);

            return true;
        }
        [Rtti.Meta]
        public virtual void Tick(UGameInstance host, float elapsedMillisecond)
        {

        }
        [Rtti.Meta]
        public virtual void BeginDestroy(UGameInstance host)
        {
            host.FinalViewportSlate();
        }

        [Rtti.Meta]
        public delegate void Delegate_DelegateTest(IAssetMeta meta);
        [Rtti.Meta]
        public virtual void DelegateTest(int param1, Delegate_DelegateTest delegateParam)
        {

        }
        [Rtti.Meta]
        public virtual void TestFunction(int paramInt, bool param2)
        {

        }
    }
    [Rtti.Meta(Flags = Rtti.MetaAttribute.EMetaFlags.NoMacrossCreate)]
    public partial class UGameInstance : TtModuleHost<UGameInstance>, ITickable
    {
        public int GetTickOrder()
        {
            return -1;
        }
        public virtual void TickLogic(float ellapse)
        {
            WorldViewportSlate?.TickLogic(ellapse);
        }
        public virtual void TickRender(float ellapse)
        {
            
        }
        public void TickBeginFrame(float ellapse)
        {

        }
        public virtual void TickSync(float ellapse)
        {
            WorldViewportSlate?.TickSync(ellapse);
        }

        [Rtti.Meta]
        public TtGameViewportSlate WorldViewportSlate { get; } = new TtGameViewportSlate(true);
        [Rtti.Meta]
        public Graphics.Pipeline.TtCamera DefaultCamera 
        {
            get => WorldViewportSlate.RenderPolicy.DefaultCamera;
        }
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
            if (McObject == null)
                return false;
            if (McObject.Get() == null)
                return false;
            return await McObject.Get().BeginPlay(this);
        }
        public virtual void Tick(float elapsedMillisecond)
        {
            McObject?.Get()?.Tick(this, elapsedMillisecond);
        }
        public virtual void BeginDestroy()
        {
            McObject?.Get()?.BeginDestroy(this);
        }
        [Rtti.Meta]
        public async System.Threading.Tasks.Task InitViewportSlate(
            [RName.PGRName(FilterExts = Bricks.RenderPolicyEditor.TtRenderPolicyAsset.AssetExt)]
            RName rPolicy, 
            float zMin = 0, float zMax = 1)
        {
            if (rPolicy == null)
                rPolicy = TtEngine.Instance.Config.MainRPolicyName;
            await WorldViewportSlate.Initialize(null, rPolicy, zMin, zMax);
            WorldViewportSlate.RenderPolicy.DisableShadow = false;
            TtEngine.Instance.GfxDevice.SlateApplication.NativeWindow.RegEventProcessor(WorldViewportSlate);
        }
        [Rtti.Meta]
        public void FinalViewportSlate()
        {
            TtEngine.Instance.GfxDevice.SlateApplication?.NativeWindow.UnregEventProcessor(WorldViewportSlate);
            TtEngine.RootFormManager.UnregRootForm(WorldViewportSlate);
        }

        [Rtti.Meta]
        public async System.Threading.Tasks.Task<GamePlay.Scene.TtScene> LoadScene(
            [RName.PGRName(FilterExts = GamePlay.Scene.TtScene.AssetExt)]
            RName mapName)
        {
            var viewport = this.WorldViewportSlate;
            var world = viewport.World;

            var scene = await GamePlay.Scene.TtScene.LoadScene(world, mapName);
            if (scene != null)
            {
                world.Root.ClearChildren();
                world.Root.SetStyle(GamePlay.Scene.TtNode.ENodeStyles.VisibleFollowParent);
                scene.Parent = world.Root;
                return scene;
            }
            return null;
        }
        [Rtti.Meta]
        public async System.Threading.Tasks.Task<TtScene> InitViewportSlateWithScene(
            [RName.PGRName(FilterExts = GamePlay.Scene.TtScene.AssetExt)]
            RName mapName,
            float zMin = 0, float zMax = 1, bool bSetToWorld = true)
        {
            var viewport = this.WorldViewportSlate;
            var world = viewport.World;

            var scene = await GamePlay.Scene.TtScene.LoadScene(world, mapName);
            if (scene == null)
            {
                return null;
            }

            var rPolicy = scene.RPolicyName;
            if (rPolicy == null)
                rPolicy = TtEngine.Instance.Config.MainRPolicyName;
            await WorldViewportSlate.Initialize(null, rPolicy, zMin, zMax);
            WorldViewportSlate.RenderPolicy.DisableShadow = false;
            TtEngine.Instance.GfxDevice.SlateApplication.NativeWindow.RegEventProcessor(WorldViewportSlate);

            if (bSetToWorld)
                SetSceneToWorld(scene);

            return scene;
        }
        [Rtti.Meta]
        public void SetSceneToWorld(TtScene scene)
        {
            var viewport = this.WorldViewportSlate;
            var world = viewport.World;

            world.Root.ClearChildren();
            world.Root.SetStyle(GamePlay.Scene.TtNode.ENodeStyles.VisibleFollowParent);
            scene.Parent = world.Root;
        }
        public Controller.TtCharacterController CharacterController { get; set; } = null;
        [Rtti.Meta]
        public async System.Threading.Tasks.Task CreateCharacterFromPrefab(Scene.TtScene scene, RName prefabName)
        {
            var playerStart = scene.FindFirstChild<TtPlayerStart>();
            EngineNS.GamePlay.Scene.TtNode root = scene;

            var prefab = await TtPrefab.LoadPrefab(scene.World, prefabName);
             prefab.Root.Parent = root;
            var actor = prefab.Root.FindFirstChild<TtActor>() as TtActor;
            if (playerStart == null)
            {
                actor.Placement.SetTransform(new DVector3(0, 0, 0), Vector3.One, Quaternion.Identity);
            }
            else
            {
                actor.Placement.SetTransform(playerStart.Placement.TransformData);
            }
            CharacterController = new EngineNS.GamePlay.Controller.TtCharacterController();
            await CharacterController.InitializeNode(scene.World, new EngineNS.GamePlay.Scene.TtNodeData(), EngineNS.GamePlay.Scene.EBoundVolumeType.Box, typeof(EngineNS.GamePlay.TtPlacement));
            CharacterController.Parent = root;
            CharacterController.ControlledCharacter = actor;

            CharacterController.CameraControlNode = actor.FindFirstChild<TtCameraSpringArm>(null, true) as ICameraControlNode;
            var camera = actor.FindFirstChild<TtGamePlayCamera>(null, true) as TtGamePlayCamera;
            camera.Camera = WorldViewportSlate.RenderPolicy.DefaultCamera;

            CharacterController.MovementNode = actor.FindFirstChild<TtMovement>(null, true) as TtMovement;
        }
        [Rtti.Meta]
        public async System.Threading.Tasks.Task CreateCharacter(Scene.TtScene scene)
        {
            var playerStart = scene.FindFirstChild<TtPlayerStart>();
            EngineNS.GamePlay.Scene.TtNode root = scene;
            var playerData = new EngineNS.GamePlay.Scene.Actor.TtActor.TtActorData();
            var ChiefPlayer = new EngineNS.GamePlay.Scene.Actor.TtActor();
            await ChiefPlayer.InitializeNode(scene.World, playerData, EngineNS.GamePlay.Scene.EBoundVolumeType.Box, typeof(EngineNS.GamePlay.TtPlacement));
            ChiefPlayer.Parent = root;
            ChiefPlayer.NodeData.Name = "UActor";
            ChiefPlayer.HitproxyType = EngineNS.Graphics.Pipeline.TtHitProxy.EHitproxyType.None;
            ChiefPlayer.IsCastShadow = true;
            ChiefPlayer.SetStyle(EngineNS.GamePlay.Scene.TtNode.ENodeStyles.VisibleFollowParent);
            if(playerStart == null)
            {
                ChiefPlayer.Placement.SetTransform(new DVector3(0, 0, 0), Vector3.One, Quaternion.Identity);
            }
            else
            {
                ChiefPlayer.Placement.SetTransform(playerStart.Placement.TransformData);
            }

            var meshData1 = new EngineNS.GamePlay.Scene.TtMeshNode.TtMeshNodeData();
            meshData1.MeshName = RName.GetRName("utest/puppet/mesh/puppet.ums");
            meshData1.MdfQueueType = EngineNS.Rtti.TtTypeDesc.TypeOf(typeof(EngineNS.Graphics.Mesh.UMdfSkinMesh)).TypeString;
            meshData1.AtomType = EngineNS.Rtti.TtTypeDesc.TypeOf(typeof(EngineNS.Graphics.Mesh.TtMesh.TtAtom)).TypeString;
            var meshNode1 = new EngineNS.GamePlay.Scene.TtMeshNode();
            await meshNode1.InitializeNode(scene.World, meshData1, EngineNS.GamePlay.Scene.EBoundVolumeType.Box, typeof(EngineNS.GamePlay.TtPlacement));
            meshNode1.NodeData.Name = "Robot1";
            meshNode1.Parent = ChiefPlayer;
            meshNode1.Placement.SetTransform(new DVector3(0.0f), new Vector3(1.0f), Quaternion.Identity);
            meshNode1.HitproxyType = EngineNS.Graphics.Pipeline.TtHitProxy.EHitproxyType.FollowParent;
            meshNode1.IsAcceptShadow = false;
            meshNode1.IsCastShadow = true;

            //var sapnd = new EngineNS.Animation.SceneNode.USkeletonAnimPlayNode.USkeletonAnimPlayNodeData();
            //sapnd.Name = "PlayAnim";
            //sapnd.AnimatinName = RName.GetRName("utest/puppet/animation/w2_walk_aim_f_loop_ip.animclip");
            //await EngineNS.Animation.SceneNode.USkeletonAnimPlayNode.AddSkeletonAnimPlayNode(scene.World, meshNode1, sapnd, EngineNS.GamePlay.Scene.EBoundVolumeType.Box, typeof(EngineNS.GamePlay.UIdentityPlacement));
            var sapnd = new EngineNS.Animation.SceneNode.TtBlendSpaceAnimPlayNode.TtBlendSpaceAnimPlayNodeData();
            sapnd.Name = "PlayAnim";
            sapnd.AnimatinName = RName.GetRName("utest/puppet/animation/w2_walk_aim_f_loop_ip.animclip");
            sapnd.OverrideAsset = true;
            sapnd.Axises.Add(new EngineNS.Animation.Asset.BlendSpace.TtBlendSpace_Axis("Speed", -3, 3));
            sapnd.Axises.Add(new EngineNS.Animation.Asset.BlendSpace.TtBlendSpace_Axis("V"));
            sapnd.Points.Add(new EngineNS.Animation.SceneNode.FBlendSpacePoint(RName.GetRName("utest/puppet/animation/w2_stand_aim_idle_ip.animclip"), Vector3.Zero));
            sapnd.Points.Add(new EngineNS.Animation.SceneNode.FBlendSpacePoint(RName.GetRName("utest/puppet/animation/w2_run_f_loop_ip.animclip"), new Vector3(3, 0, 0)));
            await EngineNS.Animation.SceneNode.TtBlendSpaceAnimPlayNode.AddBlendSpace2DAnimPlayNode(scene.World, meshNode1, sapnd, EngineNS.GamePlay.Scene.EBoundVolumeType.Box, typeof(EngineNS.GamePlay.TtIdentityPlacement));

            var characterController = new EngineNS.GamePlay.Controller.TtCharacterController();
            await characterController.InitializeNode(scene.World, new EngineNS.GamePlay.Scene.TtNodeData(), EngineNS.GamePlay.Scene.EBoundVolumeType.Box, typeof(EngineNS.GamePlay.TtPlacement));
            characterController.Parent = root;
            characterController.ControlledCharacter = ChiefPlayer;

            var springArm = new EngineNS.GamePlay.Camera.TtCameraSpringArm();
            var springArmData = new EngineNS.GamePlay.Camera.TtCameraSpringArm.TtCameraSpringArmData();
            springArmData.TargetOffset = DVector3.Up * 1.0f;
            springArmData.ArmLength = 5;
            await springArm.InitializeNode(scene.World, springArmData, EngineNS.GamePlay.Scene.EBoundVolumeType.Box, typeof(EngineNS.GamePlay.TtPlacement));

            springArm.Parent = ChiefPlayer;

            characterController.CameraControlNode = springArm;

            var camera = new EngineNS.GamePlay.Camera.TtGamePlayCamera();
            await camera.InitializeNode(WorldViewportSlate.World, new EngineNS.GamePlay.Scene.TtNodeData(), EngineNS.GamePlay.Scene.EBoundVolumeType.Box, typeof(EngineNS.GamePlay.TtPlacement));
            camera.Parent = springArm;
            camera.Camera = WorldViewportSlate.RenderPolicy.DefaultCamera;

            var phyControl = new TtCapsulePhyControllerNode();
            var phyNodeData = new TtCapsulePhyControllerNode.TtCapsulePhyControllerNodeData();
            phyNodeData.Height = 1.5f;
            phyNodeData.Radius = 0.5f;
            await phyControl.InitializeNode(scene.World, phyNodeData, Scene.EBoundVolumeType.Box, typeof(EngineNS.GamePlay.TtPlacement));
            //phyControl.Parent = ChiefPlayer;

            var movement = new EngineNS.GamePlay.Movemnet.TtCharacterMovement();
            //movement.EnableGravity = true;
            await movement.InitializeNode(scene.World, new EngineNS.GamePlay.Scene.TtNodeData() { Name = "Movement" }, EngineNS.GamePlay.Scene.EBoundVolumeType.Box, typeof(EngineNS.GamePlay.TtPlacement));
            movement.Parent = ChiefPlayer;

            characterController.MovementNode = movement;
        }

        public async System.Threading.Tasks.Task CreateSpereActor(Scene.TtScene scene)
        {
            EngineNS.GamePlay.Scene.TtNode root = scene;
            var playerData = new EngineNS.GamePlay.Scene.Actor.TtActor.TtActorData();
            var actor = new EngineNS.GamePlay.Scene.Actor.TtActor();
            await actor.InitializeNode(scene.World, playerData, EngineNS.GamePlay.Scene.EBoundVolumeType.Box, typeof(EngineNS.GamePlay.TtPlacement));
            actor.Parent = root;
            actor.NodeData.Name = "UActor";
            actor.HitproxyType = EngineNS.Graphics.Pipeline.TtHitProxy.EHitproxyType.None;
            actor.IsCastShadow = true;
            actor.SetStyle(EngineNS.GamePlay.Scene.TtNode.ENodeStyles.VisibleFollowParent);
            actor.Placement.SetTransform(new DVector3(100, 10, 50), Vector3.One, Quaternion.Identity);

            var phyControl = new TtPhySphereCollisionNode();
            var phyNodeData = new TtPhySphereCollisionNode.UPhySphereCollisionNodeData();
            phyNodeData.Radius = 0.5f;
            await phyControl.InitializeNode(scene.World, phyNodeData, Scene.EBoundVolumeType.Box, typeof(EngineNS.GamePlay.TtPlacement));
            phyControl.Parent = actor;
        }
        public async System.Threading.Tasks.Task CreateBoxActor(Scene.TtScene scene)
        {
            EngineNS.GamePlay.Scene.TtNode root = scene;
            var playerData = new EngineNS.GamePlay.Scene.Actor.TtActor.TtActorData();
            var actor = new EngineNS.GamePlay.Scene.Actor.TtActor();
            await actor.InitializeNode(scene.World, playerData, EngineNS.GamePlay.Scene.EBoundVolumeType.Box, typeof(EngineNS.GamePlay.TtPlacement));
            actor.Parent = root;
            actor.NodeData.Name = "UActor";
            actor.HitproxyType = EngineNS.Graphics.Pipeline.TtHitProxy.EHitproxyType.None;
            actor.IsCastShadow = true;
            actor.SetStyle(EngineNS.GamePlay.Scene.TtNode.ENodeStyles.VisibleFollowParent);
            actor.Placement.SetTransform(new DVector3(100, 2, 50), Vector3.One, Quaternion.Identity);

            var phyControl = new TtPhyBoxCollisionNode();
            var phyNodeData = new TtPhyBoxCollisionNode.UPhyBoxCollisionNodeData();
            phyNodeData.PhyActorType = EPhyActorType.PAT_Static;
            await phyControl.InitializeNode(scene.World, phyNodeData, Scene.EBoundVolumeType.Box, typeof(EngineNS.GamePlay.TtPlacement));
            phyControl.Parent = actor;
        }
    }
}

namespace EngineNS
{
    public partial class TtEngine
    {
        [Rtti.Meta(Flags = Rtti.MetaAttribute.EMetaFlags.Unserializable | Rtti.MetaAttribute.EMetaFlags.MacrossReadOnly)]
        public GamePlay.UGameInstance GameInstance
        {
            get;
            set;
        }
    }
}