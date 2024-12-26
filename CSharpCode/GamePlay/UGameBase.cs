using EngineNS.Bricks.PhysicsCore.SceneNode;
using EngineNS.GamePlay.Camera;
using EngineNS.GamePlay.Controller;
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
    //[Rtti.Meta(NameAlias = new string[] { "EngineNS.GamePlay.UMacrossGame@EngineCore", "EngineNS.GamePlay.UMacrossGame" })]
    public partial class TtMacrossGame
    {
        [Rtti.Meta]
        public virtual async System.Threading.Tasks.Task<bool> BeginPlay(TtGameInstance host)
        {
            await host.InitViewportSlate(TtEngine.Instance.Config.MainRPolicyName);

            return true;
        }
        [Rtti.Meta]
        public virtual void Tick(TtGameInstance host, float elapsedMillisecond)
        {

        }
        [Rtti.Meta]
        public virtual void BeginDestroy(TtGameInstance host)
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
    public partial class TtGameInstance : TtModuleHost<TtGameInstance>, ITickable, IDisposable
    {
        public int GetTickOrder()
        {
            return -1;
        }
        public void Dispose()
        {
            CoreSDK.DisposeObject(ref mMcObject);
            FinalViewportSlate();
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
        public TtWorld GameWorld
        {
            get => WorldViewportSlate.World;
        }
        [Rtti.Meta]
        public TtGameViewportSlate WorldViewportSlate { get; private set; } = new TtGameViewportSlate(true);
        [Rtti.Meta]
        public Graphics.Pipeline.TtCamera DefaultCamera 
        {
            get => WorldViewportSlate.RenderPolicy.DefaultCamera;
        }
        Macross.TtMacrossGetter<TtMacrossGame> mMcObject;
        public Macross.TtMacrossGetter<TtMacrossGame> McObject
        {
            get
            {
                if (mMcObject == null)
                    mMcObject = Macross.TtMacrossGetter<TtMacrossGame>.NewInstance();
                return mMcObject;
            }
        }
        [Rtti.Meta]
        public TtMacrossGame MacrossGame
        {
            get
            {
                return McObject?.Get();
            }
        }
        protected override TtGameInstance GetHost()
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
            if (WorldViewportSlate == null)
                return;
            TtEngine.Instance.GfxDevice.SlateApplication?.NativeWindow.UnregEventProcessor(WorldViewportSlate);
            TtEngine.RootFormManager.UnregRootForm(WorldViewportSlate);
            WorldViewportSlate?.Dispose();
            WorldViewportSlate = null;
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
        public async System.Threading.Tasks.Task CreateCharacterFromPrefab(Scene.TtScene scene,
            [RName.PGRName(FilterExts = TtPrefab.AssetExt)]
            RName prefabName)
        {
            await CreateCharacterFromPrefabDetial(scene, prefabName, true, false);
        }
        [Rtti.Meta]
        public async System.Threading.Tasks.Task CreateCharacterFromPrefabDetial(Scene.TtScene scene,
            [RName.PGRName(FilterExts = TtPrefab.AssetExt)]
            RName prefabName,
            bool orientCameraRoation,
            bool OrientToMovmement)
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
            await CharacterController.InitializeNode(scene.World, new TtCharacterController.TtCharacterControllerNodeData(), EngineNS.GamePlay.Scene.EBoundVolumeType.Box, typeof(EngineNS.GamePlay.TtPlacement));
            CharacterController.OrientCameraRoation = orientCameraRoation;
            CharacterController.OrientToMovmement = OrientToMovmement;
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
            await characterController.InitializeNode(scene.World, new TtCharacterController.TtCharacterControllerNodeData(), EngineNS.GamePlay.Scene.EBoundVolumeType.Box, typeof(EngineNS.GamePlay.TtPlacement));
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
    }
}

namespace EngineNS
{
    public partial class TtEngine
    {
        [Rtti.Meta(Flags = Rtti.MetaAttribute.EMetaFlags.NoSerializable | Rtti.MetaAttribute.EMetaFlags.MacrossReadOnly)]
        public GamePlay.TtGameInstance GameInstance
        {
            get;
            set;
        }
        [Rtti.Meta()]
        public static object RunMacross(out bool isOk,
            //[Rtti.MetaParameter(FilterType = typeof(object), ConvertOutArguments = Rtti.MetaParameterAttribute.EArgumentFilter.R)]
            //System.Type type, 
            Macross.TtMacrossGetterBase getter, string func, params object[] parameters)
        {
            isOk = false;
            if (getter.InnerObject == null)
                return null;
            var mtd = getter.InnerObject.GetType().GetMethod(func);
            if (mtd == null)
                return null;
            isOk = true;
            return mtd.Invoke(getter.InnerObject, parameters);
        }
    }
}

#if TitanEngine_AutoGen_Macross
#region TitanEngine_AutoGen_Macross


namespace EngineNS.GamePlay
{
	partial class TtMacrossGame
	{
		private static EngineNS.Macross.TtMacrossBreak macross_break_BeginPlay_2026881306 = new EngineNS.Macross.TtMacrossBreak("EngineNS.GamePlay.TtMacrossGame->System.Threading.Tasks.Task<bool> BeginPlay(TtGameInstance host)");
		public async System.Threading.Tasks.Task<bool> macross_BeginPlay (string nodeName, TtGameInstance host) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":host", host);
				}
			}
			var _return_value = await BeginPlay(host);
			macross_break_BeginPlay_2026881306.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_Tick_2968508069 = new EngineNS.Macross.TtMacrossBreak("EngineNS.GamePlay.TtMacrossGame->void Tick(TtGameInstance host, float elapsedMillisecond)");
		public unsafe void macross_Tick (string nodeName, TtGameInstance host, float elapsedMillisecond) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":host", host);
					stackframe.SetWatchVariable(nodeName + ":elapsedMillisecond", elapsedMillisecond);
				}
			}
			Tick(host, elapsedMillisecond);
			macross_break_Tick_2968508069.TryBreak();
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_BeginDestroy_2026881306 = new EngineNS.Macross.TtMacrossBreak("EngineNS.GamePlay.TtMacrossGame->void BeginDestroy(TtGameInstance host)");
		public unsafe void macross_BeginDestroy (string nodeName, TtGameInstance host) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":host", host);
				}
			}
			BeginDestroy(host);
			macross_break_BeginDestroy_2026881306.TryBreak();
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_DelegateTest_130575545 = new EngineNS.Macross.TtMacrossBreak("EngineNS.GamePlay.TtMacrossGame->void DelegateTest(int param1, Delegate_DelegateTest delegateParam)");
		public unsafe void macross_DelegateTest (string nodeName, int param1, Delegate_DelegateTest delegateParam) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":param1", param1);
					stackframe.SetWatchVariable(nodeName + ":delegateParam", delegateParam);
				}
			}
			DelegateTest(param1, delegateParam);
			macross_break_DelegateTest_130575545.TryBreak();
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_TestFunction_3182854657 = new EngineNS.Macross.TtMacrossBreak("EngineNS.GamePlay.TtMacrossGame->void TestFunction(int paramInt, bool param2)");
		public unsafe void macross_TestFunction (string nodeName, int paramInt, bool param2) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":paramInt", paramInt);
					stackframe.SetWatchVariable(nodeName + ":param2", param2);
				}
			}
			TestFunction(paramInt, param2);
			macross_break_TestFunction_3182854657.TryBreak();
		}
	}
}


namespace EngineNS.GamePlay
{
	partial class TtGameInstance
	{
		private static EngineNS.Macross.TtMacrossBreak macross_break_InitViewportSlate_3191153360 = new EngineNS.Macross.TtMacrossBreak("EngineNS.GamePlay.TtGameInstance->System.Threading.Tasks.Task InitViewportSlate(RName rPolicy, float zMin, float zMax)");
		public async System.Threading.Tasks.Task macross_InitViewportSlate (string nodeName, RName rPolicy, float zMin, float zMax) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":rPolicy", rPolicy);
					stackframe.SetWatchVariable(nodeName + ":zMin", zMin);
					stackframe.SetWatchVariable(nodeName + ":zMax", zMax);
				}
			}
			await InitViewportSlate(rPolicy, zMin, zMax);
			macross_break_InitViewportSlate_3191153360.TryBreak();
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_FinalViewportSlate_2609910045 = new EngineNS.Macross.TtMacrossBreak("EngineNS.GamePlay.TtGameInstance->void FinalViewportSlate()");
		public unsafe void macross_FinalViewportSlate (string nodeName) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
				}
			}
			FinalViewportSlate();
			macross_break_FinalViewportSlate_2609910045.TryBreak();
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_LoadScene_1006455375 = new EngineNS.Macross.TtMacrossBreak("EngineNS.GamePlay.TtGameInstance->System.Threading.Tasks.Task<GamePlay.Scene.TtScene> LoadScene(RName mapName)");
		public async System.Threading.Tasks.Task<GamePlay.Scene.TtScene> macross_LoadScene (string nodeName, RName mapName) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":mapName", mapName);
				}
			}
			var _return_value = await LoadScene(mapName);
			macross_break_LoadScene_1006455375.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_InitViewportSlateWithScene_2315274476 = new EngineNS.Macross.TtMacrossBreak("EngineNS.GamePlay.TtGameInstance->System.Threading.Tasks.Task<TtScene> InitViewportSlateWithScene(RName mapName, float zMin, float zMax, bool bSetToWorld)");
		public async System.Threading.Tasks.Task<TtScene> macross_InitViewportSlateWithScene (string nodeName, RName mapName, float zMin, float zMax, bool bSetToWorld) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":mapName", mapName);
					stackframe.SetWatchVariable(nodeName + ":zMin", zMin);
					stackframe.SetWatchVariable(nodeName + ":zMax", zMax);
					stackframe.SetWatchVariable(nodeName + ":bSetToWorld", bSetToWorld);
				}
			}
			var _return_value = await InitViewportSlateWithScene(mapName, zMin, zMax, bSetToWorld);
			macross_break_InitViewportSlateWithScene_2315274476.TryBreak();
			return _return_value;
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_SetSceneToWorld_2687476761 = new EngineNS.Macross.TtMacrossBreak("EngineNS.GamePlay.TtGameInstance->void SetSceneToWorld(TtScene scene)");
		public unsafe void macross_SetSceneToWorld (string nodeName, TtScene scene) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":scene", scene);
				}
			}
			SetSceneToWorld(scene);
			macross_break_SetSceneToWorld_2687476761.TryBreak();
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_CreateCharacterFromPrefab_401884465 = new EngineNS.Macross.TtMacrossBreak("EngineNS.GamePlay.TtGameInstance->System.Threading.Tasks.Task CreateCharacterFromPrefab(Scene.TtScene scene, RName prefabName)");
		public async System.Threading.Tasks.Task macross_CreateCharacterFromPrefab (string nodeName, Scene.TtScene scene, RName prefabName) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":scene", scene);
					stackframe.SetWatchVariable(nodeName + ":prefabName", prefabName);
				}
			}
			await CreateCharacterFromPrefab(scene, prefabName);
			macross_break_CreateCharacterFromPrefab_401884465.TryBreak();
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_CreateCharacterFromPrefabDetial_2576849437 = new EngineNS.Macross.TtMacrossBreak("EngineNS.GamePlay.TtGameInstance->System.Threading.Tasks.Task CreateCharacterFromPrefabDetial(Scene.TtScene scene, RName prefabName, bool orientCameraRoation, bool OrientToMovmement)");
		public async System.Threading.Tasks.Task macross_CreateCharacterFromPrefabDetial (string nodeName, Scene.TtScene scene, RName prefabName, bool orientCameraRoation, bool OrientToMovmement) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":scene", scene);
					stackframe.SetWatchVariable(nodeName + ":prefabName", prefabName);
					stackframe.SetWatchVariable(nodeName + ":orientCameraRoation", orientCameraRoation);
					stackframe.SetWatchVariable(nodeName + ":OrientToMovmement", OrientToMovmement);
				}
			}
			await CreateCharacterFromPrefabDetial(scene, prefabName, orientCameraRoation, OrientToMovmement);
			macross_break_CreateCharacterFromPrefabDetial_2576849437.TryBreak();
		}
		private static EngineNS.Macross.TtMacrossBreak macross_break_CreateCharacter_3958660289 = new EngineNS.Macross.TtMacrossBreak("EngineNS.GamePlay.TtGameInstance->System.Threading.Tasks.Task CreateCharacter(Scene.TtScene scene)");
		public async System.Threading.Tasks.Task macross_CreateCharacter (string nodeName, Scene.TtScene scene) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":scene", scene);
				}
			}
			await CreateCharacter(scene);
			macross_break_CreateCharacter_3958660289.TryBreak();
		}
	}
}


namespace EngineNS
{
	partial class TtEngine
	{
		private static EngineNS.Macross.TtMacrossBreak macross_break_RunMacross_3676773373 = new EngineNS.Macross.TtMacrossBreak("EngineNS.TtEngine->static object RunMacross(out bool isOk, Macross.TtMacrossGetterBase getter, string func, object[] parameters)");
		public static unsafe object macross_RunMacross (string nodeName, out bool isOk, Macross.TtMacrossGetterBase getter, string func, object[] parameters) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":getter", getter);
					stackframe.SetWatchVariable(nodeName + ":func", func);
					stackframe.SetWatchVariable(nodeName + ":parameters", parameters);
				}
			}
			var _return_value = RunMacross(out isOk, getter, func, parameters);
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":isOk", isOk);
				}
			}
			macross_break_RunMacross_3676773373.TryBreak();
			return _return_value;
		}
	}
}
#endregion//TitanEngine_AutoGen_Macross
#endif//TitanEngine_AutoGen_Macross