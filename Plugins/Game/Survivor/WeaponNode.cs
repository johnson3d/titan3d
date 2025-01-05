using EngineNS;
using EngineNS.GamePlay;
using EngineNS.GamePlay.Character;
using EngineNS.GamePlay.Scene;
using EngineNS.Graphics.Pipeline;
using EngineNS.Thread.Async;
using System;
using System.Collections.Generic;
using System.Text;
using static Survivor.TtWeaponController_Line;

namespace Survivor
{
    public class TtWeaponController
    {
        public static TtWeaponController GetTtWeaponController(int weaponId)
        {
            TtWeaponController controller = null;
            switch (weaponId)
            {
                case 1001:
                    controller = new TtWeaponController_Line();
                    break;
                case 1002:
                    controller = new TtWeaponController_Nearest();
                    break;
                case 1003:
                    controller = new TtWeaponController_Back();
                    break;
                case 1004:
                    controller = new TtWeaponController_Circle();
                    break;
                case 1005:
                    controller = new TtWeaponController_Surround();
                    break;
            }
            return controller;
        }
        public static TtWeaponController GetTtWeaponController(string weaponType)
        {
            var type = EngineNS.Rtti.TtTypeDesc.TypeOf($"Survivor.TtWeaponController_{weaponType}@Survivor");
            if (type == null)
                return null;
            if (type.IsSubclassOf(typeof(TtWeaponController)) == false)
                return null;
            return EngineNS.Rtti.TtTypeDescManager.CreateInstance(type) as TtWeaponController;
        }
        public TtWeaponNode WeaponNode { get; set; }
        public TtWeaponData WeaponData { get => WeaponNode.WeaponData; }
        protected float mCurrentTime = 0;
        public virtual void Init()
        {

        }

        public virtual void Tick(TtWorld world)
        {
            //Prefab move

            //hitcheck, some weapon dont need phy hit
        }

        public virtual void OnBulletHit(TtNode bulletPrefab)
        {

        }
    }

    public class TtWeaponController_Melee : TtWeaponController
    {
        public override void Init()
        {

        }

        public override void Tick(TtWorld world)
        {
            var playerNode = (EngineNS.TtEngine.Instance.GameInstance.MacrossGame as TtMacrossSurvivorGame).GameMode.Player;
            mCurrentTime += world.DeltaTimeSecond;
            var distance = Vector3.Distance(playerNode.Placement.AbsTransform.Position.ToSingleVector3(),
                    WeaponNode.Placement.AbsTransform.Position.ToSingleVector3());
            if (mCurrentTime > WeaponData.CoolDown && distance <= WeaponData.AttackRange)
            {
                Fire();
                //fire prefab
                mCurrentTime = 0;
            }
            base.Tick(world);
        }
        protected void Fire()
        {
            var playerNode = (EngineNS.TtEngine.Instance.GameInstance.MacrossGame as TtMacrossSurvivorGame).GameMode.Player;
            WeaponNode.Attack(playerNode, null);
        }
    }


    //躯干发出水平斜线，攻击直线方向有效距离内的全部怪物并造成伤害和击退效果。
    public class TtWeaponController_Line : TtWeaponController
    {
        public struct FSimpleElementController
        {
            public TtPrefabNode Element;
            public Vector3 Direction;
            public float Speed;
            public Vector3 OriginalLocation;
            public void Update(TtWorld world)
            {
                Element.Placement.Position += Direction * Speed * world.DeltaTimeSecond;
            }
        }
        List<FSimpleElementController> BulletPrefabs = new List<FSimpleElementController>();
        public override void Init()
        {
            
        }
        List<FSimpleElementController> mBeRemoved = new List<FSimpleElementController>();
        public override void Tick(TtWorld world)
        {
            mCurrentTime += world.DeltaTimeSecond;
            if (mCurrentTime > WeaponData.CoolDown)
            {
                Fire();
                //fire prefab
                mCurrentTime = 0;
            }
            foreach (var bullet in BulletPrefabs)
            {
                bullet.Update(world);
                var distance = Vector3.Distance(bullet.OriginalLocation,
                    bullet.Element.Placement.AbsTransform.Position.ToSingleVector3());
                if (distance > WeaponData.AttackRange)
                {
                    mBeRemoved.Add(bullet);
                }
            }
            foreach (var weapon in mBeRemoved)
            {
                weapon.Element.RemoveFromWorld();
                BulletPrefabs.Remove(weapon);
                TtEngine.Instance.GameInstance.PrefabPoolManager.ReleasePrefab(weapon.Element);
            }
            mBeRemoved.Clear();
            base.Tick(world);
        }
        protected void Fire()
        {
            var weaponPrefabName = RName.ParseFrom(WeaponData.Shape);
            if (weaponPrefabName != null)
            {
                var WeaponPrefab = EngineNS.TtEngine.Instance.GameInstance.PrefabPoolManager.CreatePrefab(RName.ParseFrom(WeaponData.Shape));
                if (WeaponPrefab != null)
                {
                    WeaponPrefab.Parent = WeaponNode.Parent.Parent;
                    var proxyNode = WeaponPrefab.FindFirstChild<TtWeaponProxyNode>() as TtWeaponProxyNode;
                    proxyNode.WeaponNode = WeaponNode;

                    var singleControll = new FSimpleElementController();
                    singleControll.Element = WeaponPrefab;
                    singleControll.Speed = WeaponNode.RoleData.ProjectileSpeed;
                    singleControll.Direction = EngineNS.Quaternion.RotateVector3(WeaponNode.Parent.Placement.Quat, Vector3.Forward);
                    singleControll.OriginalLocation = WeaponNode.Parent.Placement.AbsTransform.Position.ToSingleVector3();
                    WeaponPrefab.Placement.Position = DVector3.Up + singleControll.OriginalLocation;
                    BulletPrefabs.Add(singleControll);
                }
            }
        }
    }
    //距离玩家最近的敌人依次发射n枚飞行道具，碰到怪物造成伤害，根据击穿数量判断伤害数量
    public class TtWeaponController_Nearest : TtWeaponController
    {
        public struct FNearestElementController
        {
            public TtPrefabNode Element;
            public TtMonsterNode Target;
            public float Speed;
            public void Update(TtWorld world)
            {
                if (Target.StateNode.IsDead)
                    return;
                var dir = Target.MonseterPlacement.AbsTransform.Position - Element.Placement.AbsTransform.Position;
                dir.Y = 0;
                dir.Normalize();
                Element.Placement.Position += dir * Speed * world.DeltaTimeSecond;
            }
        }
        List<FNearestElementController> BulletPrefabs = new List<FNearestElementController>();
        public override void Init()
        {

        }
        List<FNearestElementController> mBeRemoved = new List<FNearestElementController>();
        public override void Tick(TtWorld world)
        {
            mCurrentTime += world.DeltaTimeSecond;
            if (mCurrentTime > WeaponData.CoolDown)
            {
                Fire();
                //fire prefab
                mCurrentTime = 0;
            }
            foreach (var bullet in BulletPrefabs)
            {
                bullet.Update(world);
            }
            CleanupBullet();
            base.Tick(world);
        }
        private void CleanupBullet()
        {
            lock(mBeRemoved)
            {
                foreach (var weapon in mBeRemoved)
                {
                    weapon.Element.RemoveFromWorld();
                    BulletPrefabs.Remove(weapon);
                    TtEngine.Instance.GameInstance.PrefabPoolManager.ReleasePrefab(weapon.Element);
                }
                mBeRemoved.Clear();
            }
        }
        protected void Fire()
        {
            TtMonsterNode nearestNode = GetNearestMonster();
            if(nearestNode == null) 
                return;

            var bulletPrefabName = RName.ParseFrom(WeaponData.Shape);
            if(bulletPrefabName != null)
            {
                var bulletPrefab = EngineNS.TtEngine.Instance.GameInstance.PrefabPoolManager.CreatePrefab(RName.ParseFrom(WeaponData.Shape));
                if (bulletPrefab != null)
                {
                    bulletPrefab.Parent = WeaponNode.Parent.Parent;
                    var proxyNode = bulletPrefab.FindFirstChild<TtWeaponProxyNode>() as TtWeaponProxyNode;
                    proxyNode.WeaponNode = WeaponNode;

                    var controller = new FNearestElementController();
                    controller.Element = bulletPrefab;
                    controller.Speed = WeaponNode.RoleData.ProjectileSpeed;
                    controller.Target = nearestNode;
                    var character = WeaponNode.Parent.FindFirstChild<TtCharacter>();
                    if(character != null)
                    {
                        bulletPrefab.Placement.Position = DVector3.Up + character.Placement.AbsTransform.Position.ToSingleVector3();
                        BulletPrefabs.Add(controller);
                    }
                }
            }
        }
        public override void OnBulletHit(TtNode bulletPrefab)
        {
            foreach (var bullet in BulletPrefabs)
            {
                if (bullet.Element == bulletPrefab)
                {
                    mBeRemoved.Add(bullet);
                }
            }
        }
        protected TtMonsterNode GetNearestMonster()
        {
            List<TtMonsterNode> monsters = new List<TtMonsterNode>();
            WeaponNode.ParentScene.IterateNodes(static (nd, arg) =>
            {
                if(nd is TtMonsterNode)
                {
                    var tp = (List<TtMonsterNode>)arg;
                    tp.Add(nd as TtMonsterNode);
                }
                return true;
            }, monsters);
            TtMonsterNode nearestNode = null;
            float distance = WeaponData.AttackRange;
            foreach(var monster in monsters)
            {
                var candidateDis = Vector3.Distance(monster.MonseterPlacement.AbsTransform.Position.ToSingleVector3(),
                                                    WeaponNode.Parent.Placement.AbsTransform.Position.ToSingleVector3());
                if(distance >= candidateDis)
                {
                    distance = candidateDis;
                    nearestNode = monster;
                }
            }
            return nearestNode;
        }
    }
    //朝向玩家最后移动方向发射n枚飞行道具，碰到怪物造成伤害。根据击穿数量判断伤害数量
    public class TtWeaponController_Back : TtWeaponController
    {
        
    }
    //玩家周围形成不间断的保护结界，碰撞怪物后造成伤害

    public class TtWeaponController_Circle : TtWeaponController
    {

    }
    //单个经幡以玩家为圆心旋转，每个经幡计算碰撞攻击半径，持续时间结束后消失
    public class TtWeaponController_Surround : TtWeaponController
    {

    }

    [EngineNS.Bricks.CodeBuilder.ContextMenu("WeaponProxyNode", "WeaponProxyNode", TtNode.EditorKeyword)]
    [TtNode(NodeDataType = typeof(TtNodeData), DefaultNamePrefix = "WeaponProxyNode")]
    [EngineNS.EGui.Controls.PropertyGrid.PGCategoryFilters(ExcludeFilters = new string[] { "Misc" })]
    public class TtWeaponProxyNode : TtLightWeightNodeBase
    {
        [EngineNS.Rtti.Meta]
        public TtWeaponNode WeaponNode { get; set; } = null;
    }
    //无论角色的武器还是怪物的近远程攻击都算做武器攻击
    public partial class TtWeaponNode : EngineNS.GamePlay.Scene.TtSceneActorNode
    {
        public class TtWeaponNodeData : EngineNS.GamePlay.Scene.TtNodeData
        {
            [EngineNS.Rtti.Meta]
            public int WeaponId = 0;
            [EngineNS.Rtti.Meta]
            public string WeaponType = "";
        }
        public TtWeaponNodeData WeaponNodeData { get => NodeData as TtWeaponNodeData; }
        public override async TtTask<bool> InitializeNode(TtWorld world, TtNodeData data, EBoundVolumeType bvType, Type placementType)
        {
            await base.InitializeNode(world, data, bvType, placementType);
            var macrossSurvivorGame = EngineNS.TtEngine.Instance.GameInstance.MacrossGame as TtMacrossSurvivorGame;
            if(WeaponNodeData.WeaponId > 0)
            {
                WeaponData = macrossSurvivorGame.GameMode.WeaponManager.GetData("ItemId", WeaponNodeData.WeaponId);
                if (WeaponData == null)
                {
                    EngineNS.Profiler.Log.WriteLine<EngineNS.Profiler.TtGameplayGategory>(EngineNS.Profiler.ELogTag.Warning, $"Weapon({WeaponNodeData.WeaponId}) not found");
                }
                else
                {
                    mWeaponController = TtWeaponController.GetTtWeaponController(WeaponData.ItemId);
                    mWeaponController.WeaponNode = this;
                    mWeaponController.Init();
                }
            }
            else if(!string.IsNullOrEmpty(WeaponNodeData.WeaponType))
            {
                mWeaponController = TtWeaponController.GetTtWeaponController(WeaponNodeData.WeaponType);
                mWeaponController.WeaponNode = this;
                WeaponData = new TtWeaponData();
                WeaponData.CoolDown = 1;
                mWeaponController.Init();
            }
 
            return true;
        }
        public TtWeaponData WeaponData { get; set; } = null;
        public TtRoleData RoleData { get; set; } = null;
        protected TtWeaponController mWeaponController = null;
        public override EngineNS.Profiler.TimeScope GetScopeTickLogic()
        {
            return TtOnTickLogicScope<TtWeaponNode>.Scope;
        }
        public override bool OnTickLogic(TtNodeTickParameters args)
        {
            mWeaponController.Tick(args.World);
            return base.OnTickLogic(args);
        }
        [EngineNS.Rtti.Meta]
        public void Attack(TtNode targetNode, TtNode bulletNode)
        {
            if(targetNode == this.Parent)
                return;

            var stateNode = targetNode.FindFirstChild<TtStateNode>(null, true) as TtStateNode;
            stateNode.BeAttacked(this);
            mWeaponController.OnBulletHit(bulletNode);
        }
    }
}
#if TitanEngine_AutoGen_Macross
#region TitanEngine_AutoGen_Macross


namespace Survivor
{
	partial class TtWeaponNode
	{
		private static EngineNS.Macross.TtMacrossBreak macross_break_Attack_2680228188 = new EngineNS.Macross.TtMacrossBreak("Survivor.TtWeaponNode->void Attack(TtNode targetNode, TtNode bulletNode)");
		public unsafe void macross_Attack (string nodeName, TtNode targetNode, TtNode bulletNode) 
		{
			using(var stackframe = EngineNS.Macross.TtMacrossStackTracer.CurrentFrame)
			{
				if(stackframe != null)
				{
					stackframe.SetWatchVariable(nodeName + ":targetNode", targetNode);
					stackframe.SetWatchVariable(nodeName + ":bulletNode", bulletNode);
				}
			}
			Attack(targetNode, bulletNode);
			macross_break_Attack_2680228188.TryBreak();
		}
	}
}
#endregion//TitanEngine_AutoGen_Macross
#endif//TitanEngine_AutoGen_Macross