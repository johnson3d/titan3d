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
    public partial class TtGameModeBase : IDisposable
    {
        Scene.TtScene mCurrentScene;
        [Rtti.Meta("",Flags = Rtti.MetaAttribute.EMetaFlags.NoSerializable)]
        public Scene.TtScene CurrentScene
        {
            get => mCurrentScene;
            set
            {
                if (mCurrentScene != null)
                {
                    mCurrentScene.GetWorld()?.CollideOctree.Clear();
                }
                mCurrentScene = value;
            }
        }
        [Rtti.Meta("",Flags = Rtti.MetaAttribute.EMetaFlags.NoSerializable)]
        public Controller.TtCharacterController CharacterController { get; set; } = null;
        public virtual void OnSetGameMode(TtGameModeBase prev)
        {

        }
        public virtual void OnUnsetGameMode(TtGameModeBase next)
        {

        }
        public virtual void Tick(TtGameInstance host, float elapsedMillisecond)
        {

        }
        [Rtti.Meta("")]
        public virtual void Dispose()
        {

        }
    }
    [Macross.TtMacross]
    //[Rtti.Meta("",NameAlias = new string[] { "EngineNS.GamePlay.UMacrossGame@EngineCore", "EngineNS.GamePlay.UMacrossGame" })]
    public partial class TtMacrossGame : Macross.AuxMacrossObject
    {
        TtGameModeBase mGameMode = new TtGameModeBase();
        [Rtti.Meta("",Flags = Rtti.MetaAttribute.EMetaFlags.NoSerializable)]
        public TtGameModeBase GameMode { get => mGameMode; }
        [Rtti.Meta("")]
        public void SetGameMode(TtGameModeBase mode)
        {
            mGameMode?.OnUnsetGameMode(mode);
            var saved = mGameMode;
            mGameMode = mode;
            mGameMode?.OnSetGameMode(saved);
        }
        [Rtti.Meta("")]
        public virtual async System.Threading.Tasks.Task<bool> BeginPlay(TtGameInstance host)
        {
            await host.InitViewportSlate(TtEngine.Instance.Config.MainRPolicyName);

            return true;
        }
        [Rtti.Meta("")]
        public virtual void Tick(TtGameInstance host, float elapsedMillisecond)
        {
            GameMode?.Tick(host, elapsedMillisecond);
        }
        [Rtti.Meta("")]
        public virtual void BeginDestroy(TtGameInstance host)
        {
            host.FinalViewportSlate();
        }

        [Rtti.Meta("")]
        public delegate void Delegate_DelegateTest(IAssetMeta meta);
        [Rtti.Meta("")]
        public virtual void DelegateTest(int param1, Delegate_DelegateTest delegateParam)
        {

        }
        [Rtti.Meta("")]
        public virtual void TestFunction(int paramInt, bool param2)
        {

        }
    }
    [Rtti.Meta("",Flags = Rtti.MetaAttribute.EMetaFlags.NoMacrossCreate)]
    public partial class TtGameInstance : TtModuleHost<TtGameInstance>, ITickable, IDisposable
    {
        static int mNodeAliveNumber = 0;
        public static int NodeAliveNumber
        {
            get => mNodeAliveNumber;
        }
        public TtGameInstance()
        {
            this.PrefabPoolManager.World = GameWorld;
            System.Threading.Interlocked.Increment(ref mNodeAliveNumber);
        }
        ~TtGameInstance()
        {
            System.Threading.Interlocked.Decrement(ref mNodeAliveNumber);
        }
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
        [Rtti.Meta("")]
        public TtWorld GameWorld
        {
            get => WorldViewportSlate.World;
        }
        [Rtti.Meta("")]
        public TtGameViewportSlate WorldViewportSlate { get; private set; } = new TtGameViewportSlate(true);
        [Rtti.Meta("")]
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
        [Rtti.Meta("",Flags = Rtti.MetaAttribute.EMetaFlags.NoSerializable)]
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
        public Thread.TtSemaphore GameSemaphore;
        //public virtual async System.Threading.Tasks.Task<bool> BeginPlay()
        //{
        //    if (McObject == null)
        //        return false;
        //    if (McObject.Get() == null)
        //        return false;
        //    GameSemaphore = Thread.TtSemaphore.CreateSemaphore(1);
        //    var ret = await McObject.Get().BeginPlay(this);
        //    GameSemaphore.Release();
        //    return ret;
        //}
        public virtual async Thread.Async.TtTask<bool> BeginPlay()
        {
            if (McObject == null)
                return false;
            if (McObject.Get() == null)
                return false;
            GameSemaphore = Thread.TtSemaphore.CreateSemaphore(1);
            var ret = await McObject.Get().BeginPlay(this);
            GameSemaphore.Release();
            return ret;
        }
        public virtual void Tick(float elapsedMillisecond)
        {
            McObject?.Get()?.Tick(this, elapsedMillisecond);
        }
        public virtual void BeginDestroy()
        {
            McObject?.Get()?.BeginDestroy(this);
            PrefabPoolManager.Dispose();
        }
        [Rtti.Meta("")]
        public async Thread.Async.TtTask InitViewportSlate(
            [RName.PGRName(FilterExts = Bricks.RenderPolicyEditor.TtRenderPolicyAsset.AssetExt)]
            RName rPolicy, 
            float zMin = 0, float zMax = 1)
        {
            if (rPolicy == null)
                rPolicy = TtEngine.Instance.Config.MainRPolicyName;
            await WorldViewportSlate.Initialize(null, rPolicy, zMin, zMax);
            TtEngine.Instance.GfxDevice.SlateApplication.NativeWindow.RegEventProcessor(WorldViewportSlate);
        }
        [Rtti.Meta("")]
        public void FinalViewportSlate()
        {
            if (WorldViewportSlate == null)
                return;
            TtEngine.Instance.GfxDevice.SlateApplication?.NativeWindow.UnregEventProcessor(WorldViewportSlate);
            TtEngine.RootFormManager.UnregRootForm(WorldViewportSlate);
            WorldViewportSlate?.Dispose();
            WorldViewportSlate = null;
        }

        [Rtti.Meta("")]
        public async Thread.Async.TtTask<GamePlay.Scene.TtScene> LoadScene(
            [RName.PGRName(FilterExts = GamePlay.Scene.TtScene.AssetExt)]
            RName mapName, bool bSetToWorld = true)
        {
            var viewport = this.WorldViewportSlate;
            var world = viewport.World;

            var scene = await mapName.GetAsset<TtScene>(world);// GamePlay.Scene.TtScene.LoadScene(world, mapName);
            if (scene != null && bSetToWorld)
            {
                world.Root.ClearChildren();
                world.Root.SetStyle(GamePlay.Scene.TtNode.ENodeStyles.VisibleAlways);
                scene.Parent = world.Root;
                return scene;
            }
            return scene;
        }
        [Rtti.Meta("")]
        public void SetSceneToWorld(TtScene scene)
        {
            var viewport = this.WorldViewportSlate;
            var world = viewport.World;

            //world.Root.ClearChildren();
            world.Root.SetStyle(GamePlay.Scene.TtNode.ENodeStyles.VisibleAlways);
            scene.Parent = world.Root;
        }
        [Rtti.Meta("")]
        public async Thread.Async.TtTask<TtScene> InitViewportSlateWithScene(
            [RName.PGRName(FilterExts = GamePlay.Scene.TtScene.AssetExt)]
            RName mapName,
            float zMin = 0, float zMax = 1, bool bSetToWorld = true)
        {
            var viewport = this.WorldViewportSlate;
            var world = viewport.World;

            var scene = await mapName.GetAsset<TtScene>(world);
            if (scene == null)
            {
                return null;
            }

            var rPolicy = scene.RPolicyName;
            if (rPolicy == null)
                rPolicy = TtEngine.Instance.Config.MainRPolicyName;
            await WorldViewportSlate.Initialize(null, rPolicy, zMin, zMax);
            TtEngine.Instance.GfxDevice.SlateApplication.NativeWindow.RegEventProcessor(WorldViewportSlate);

            if (bSetToWorld)
                SetSceneToWorld(scene);

            return scene;
        }
        
        [Rtti.Meta("")]
        public async Thread.Async.TtTask CreateCharacterFromPrefab(Scene.TtScene scene,
            [RName.PGRName(FilterExts = TtPrefab.AssetExt)]
            RName prefabName)
        {
            this.MacrossGame.GameMode.CharacterController = await CreateCharacterController(scene, prefabName, true, false);
        }
        [Rtti.Meta("")]
        public async Thread.Async.TtTask<TtCharacterController> CreateCharacterController(Scene.TtScene scene,
            [RName.PGRName(FilterExts = TtPrefab.AssetExt)]
            RName prefabName,
            bool orientCameraRoation = true,
            bool OrientToMovmement = false)
        {
            var playerStart = scene.FindFirstChild<TtPlayerStart>();
            
            var prefab = await TtEngine.Instance.PrefabManager.CreatePrefabNode(scene.World, prefabName);
            prefab.Parent = scene;
            var actor = prefab.FindFirstChild<Character.TtCharacter>();
            if (playerStart == null)
            {
                actor.Placement.SetTransform(new DVector3(0, 0, 0), Vector3.One, Quaternion.Identity);
            }
            else
            {
                actor.Placement.SetTransform(playerStart.Placement.TransformData);
            }
            var result = await TtNode.SpawnNode<EngineNS.GamePlay.Controller.TtCharacterController>(scene, null,
                new TtCharacterController.TtCharacterControllerNodeData(), EngineNS.GamePlay.Scene.EBoundVolumeType.Box, typeof(EngineNS.GamePlay.TtPlacement));
            result.OrientCameraRoation = orientCameraRoation;
            result.OrientToMovmement = OrientToMovmement;
            result.ControlledCharacter = actor;

            result.CameraControlNode = actor.FindFirstChild<TtCameraSpringArm>(null, true);
            var camera = actor.FindFirstChild<TtGamePlayCamera>(null, true);
            camera.Camera = WorldViewportSlate.RenderPolicy.DefaultCamera;

            result.MovementNode = actor.FindFirstChild<TtMovement>(null, true);
            return result;
        }
        [Rtti.Meta("")]
        public async Thread.Async.TtTask CreateCharacter(Scene.TtScene scene)
        {
            var playerStart = scene.FindFirstChild<TtPlayerStart>();
            EngineNS.GamePlay.Scene.TtNode root = scene;
            var playerData = new EngineNS.GamePlay.Scene.Actor.TtActor.TtActorData();
            var ChiefPlayer = await TtNode.SpawnNode<Character.TtCharacter>(root, null,
                playerData, EngineNS.GamePlay.Scene.EBoundVolumeType.Box, typeof(EngineNS.GamePlay.TtPlacement));
            ChiefPlayer.Parent = root;
            ChiefPlayer.NodeData.Name = "TtActor";
            ChiefPlayer.HitproxyType = EngineNS.Graphics.Pipeline.TtHitProxy.EHitproxyType.None;
            ChiefPlayer.IsCastShadow = true;
            ChiefPlayer.SetStyle(EngineNS.GamePlay.Scene.TtNode.ENodeStyles.VisibleAlways);
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
            meshData1.MdfQueueType = EngineNS.Rtti.TtTypeDesc.TypeOf(typeof(EngineNS.Graphics.Mesh.TtMdfSkinMesh)).TypeString;
            meshData1.AtomType = EngineNS.Rtti.TtTypeDesc.TypeOf(typeof(EngineNS.Graphics.Mesh.TtRenderMesh.TtAtom)).TypeString;
            var meshNode1 = await TtNode.SpawnNode<EngineNS.GamePlay.Scene.TtMeshNode>(ChiefPlayer, null,
                meshData1, EngineNS.GamePlay.Scene.EBoundVolumeType.Box, typeof(EngineNS.GamePlay.TtPlacement));
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

            var characterController = await TtNode.SpawnNode<EngineNS.GamePlay.Controller.TtCharacterController>(root, null,
                new TtCharacterController.TtCharacterControllerNodeData(), 
                EngineNS.GamePlay.Scene.EBoundVolumeType.Box, typeof(EngineNS.GamePlay.TtPlacement));
            characterController.Parent = root;
            characterController.ControlledCharacter = ChiefPlayer;

            var springArmData = new EngineNS.GamePlay.Camera.TtCameraSpringArm.TtCameraSpringArmData();
            springArmData.TargetOffset = DVector3.Up * 1.0f;
            springArmData.ArmLength = 5;
            var springArm = await TtNode.SpawnNode<EngineNS.GamePlay.Camera.TtCameraSpringArm>(ChiefPlayer, null,
                springArmData, EngineNS.GamePlay.Scene.EBoundVolumeType.Box, typeof(EngineNS.GamePlay.TtPlacement));

            characterController.CameraControlNode = springArm;

            var camera = await TtNode.SpawnNode<EngineNS.GamePlay.Camera.TtGamePlayCamera>(springArm, null,
                new EngineNS.GamePlay.Scene.TtNodeData(), EngineNS.GamePlay.Scene.EBoundVolumeType.Box, typeof(EngineNS.GamePlay.TtPlacement));
            camera.Camera = WorldViewportSlate.RenderPolicy.DefaultCamera;

            var phyNodeData = new TtCapsulePhyControllerNode.TtCapsulePhyControllerNodeData();
            phyNodeData.Height = 1.5f;
            phyNodeData.Radius = 0.5f;
            var phyControl = await TtNode.SpawnNode<TtCapsulePhyControllerNode>(null, null, phyNodeData, Scene.EBoundVolumeType.Box, typeof(EngineNS.GamePlay.TtPlacement), scene.World);
            //phyControl.Parent = ChiefPlayer;

            var movement = await TtNode.SpawnNode<EngineNS.GamePlay.Movemnet.TtCharacterMovement>(ChiefPlayer, null, 
                new EngineNS.GamePlay.Movemnet.TtCharacterMovement.TtCharacterMovementData() { Name = "Movement" }, EngineNS.GamePlay.Scene.EBoundVolumeType.Box, typeof(EngineNS.GamePlay.TtPlacement));
            movement.Parent = ChiefPlayer;

            characterController.MovementNode = movement;
        }
    }
}

namespace EngineNS
{
    public partial class TtEngine
    {
        [Rtti.Meta("",Flags = Rtti.MetaAttribute.EMetaFlags.NoSerializable | Rtti.MetaAttribute.EMetaFlags.MacrossReadOnly)]
        public GamePlay.TtGameInstance GameInstance
        {
            get;
            set;
        }
    }
}

#if TitanEngine_AutoGen_Macross
#region TitanEngine_AutoGen_Macross


namespace EngineNS.GamePlay
{
	partial class TtGameModeBase
	{
		public unsafe void macross_Dispose (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
				}
			}
			Dispose();
		}
	}
}


namespace EngineNS.GamePlay
{
	partial class TtMacrossGame
	{
		public unsafe void macross_SetGameMode (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, TtGameModeBase mode) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
				}
			}
			SetGameMode(mode);
		}
		public async System.Threading.Tasks.Task<bool> macross_BeginPlay (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, TtGameInstance host) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
				}
			}
			var _return_value = await BeginPlay(host);
			return _return_value;
		}
		public unsafe void macross_Tick (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, TtGameInstance host, float elapsedMillisecond) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
				}
			}
			Tick(host, elapsedMillisecond);
		}
		public unsafe void macross_BeginDestroy (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, TtGameInstance host) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
				}
			}
			BeginDestroy(host);
		}
		public unsafe void macross_DelegateTest (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, int param1, Delegate_DelegateTest delegateParam) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
				}
			}
			DelegateTest(param1, delegateParam);
		}
		public unsafe void macross_TestFunction (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, int paramInt, bool param2) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
				}
			}
			TestFunction(paramInt, param2);
		}
	}
}


namespace EngineNS.GamePlay
{
	partial class TtGameInstance
	{
		public async Thread.Async.TtTask macross_InitViewportSlate (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, RName rPolicy, float zMin, float zMax) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
				}
			}
			await InitViewportSlate(rPolicy, zMin, zMax);
		}
		public unsafe void macross_FinalViewportSlate (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
				}
			}
			FinalViewportSlate();
		}
		public async Thread.Async.TtTask<GamePlay.Scene.TtScene> macross_LoadScene (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, RName mapName, bool bSetToWorld) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
				}
			}
			var _return_value = await LoadScene(mapName, bSetToWorld);
			return _return_value;
		}
		public unsafe void macross_SetSceneToWorld (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, TtScene scene) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
				}
			}
			SetSceneToWorld(scene);
		}
		public async Thread.Async.TtTask<TtScene> macross_InitViewportSlateWithScene (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, RName mapName, float zMin, float zMax, bool bSetToWorld) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
				}
			}
			var _return_value = await InitViewportSlateWithScene(mapName, zMin, zMax, bSetToWorld);
			return _return_value;
		}
		public async Thread.Async.TtTask macross_CreateCharacterFromPrefab (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, Scene.TtScene scene, RName prefabName) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
				}
			}
			await CreateCharacterFromPrefab(scene, prefabName);
		}
		public async Thread.Async.TtTask<TtCharacterController> macross_CreateCharacterController (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, Scene.TtScene scene, RName prefabName, bool orientCameraRoation, bool OrientToMovmement) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
				}
			}
			var _return_value = await CreateCharacterController(scene, prefabName, orientCameraRoation, OrientToMovmement);
			return _return_value;
		}
		public async Thread.Async.TtTask macross_CreateCharacter (EngineNS.Macross.TtMacrossStackTracer mcStack, string nodeName, Scene.TtScene scene) 
		{
			var stackframe = mcStack.TopFrame;
			{
				if(stackframe != null)
				{
				}
			}
			await CreateCharacter(scene);
		}
	}
}
#endregion//TitanEngine_AutoGen_Macross
#endif//TitanEngine_AutoGen_Macross