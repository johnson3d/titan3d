using EngineNS;
using EngineNS.GamePlay;
using EngineNS.GamePlay.Scene;
using EngineNS.Graphics.Pipeline;
using EngineNS.Thread.Async;
using System;
using System.Collections.Generic;
using System.Text;

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
        List<FSimpleElementController> WeaponPrefabs = new List<FSimpleElementController>();
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
            foreach (var weapon in WeaponPrefabs)
            {
                weapon.Update(world);
                var distance = Vector3.Distance(weapon.OriginalLocation,
                    weapon.Element.Placement.AbsTransform.Position.ToSingleVector3());
                if (distance > WeaponData.AttackRange)
                {
                    mBeRemoved.Add(weapon);
                }
            }
            foreach (var weapon in mBeRemoved)
            {
                weapon.Element.Parent = null;
                WeaponPrefabs.Remove(weapon);
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
                    if(WeaponPrefab.Placement is TtIdentityPlacement)
                    {
                        WeaponPrefab.NodeData.Placement = new TtPlacement();
                        WeaponPrefab.NodeData.Placement.HostNode = WeaponPrefab;
                    }
                    var singleControll = new FSimpleElementController();
                    singleControll.Element = WeaponPrefab;
                    singleControll.Speed = WeaponNode.RoleData.ProjectileSpeed;
                    singleControll.Direction = EngineNS.Quaternion.RotateVector3(WeaponNode.Parent.Placement.Quat, Vector3.Forward);
                    singleControll.OriginalLocation = WeaponNode.Parent.Placement.AbsTransform.Position.ToSingleVector3();
                    WeaponPrefab.Placement.Position = DVector3.Up + singleControll.OriginalLocation;
                    WeaponPrefabs.Add(singleControll);
                }
            }
        }
    }
    //距离玩家最近的敌人依次发射n枚飞行道具，碰到怪物造成伤害，根据击穿数量判断伤害数量
    public class TtWeaponController_Nearest : TtWeaponController
    {
        List<TtPrefabNode> WeaponPrefabs = new List<TtPrefabNode>();
        public override void Init()
        {

        }
        List<TtPrefabNode> mBeRemoved = new List<TtPrefabNode>();
        public override void Tick(TtWorld world)
        {
            mCurrentTime += world.DeltaTimeSecond;
            if (mCurrentTime > WeaponData.CoolDown)
            {
                Fire();
                //fire prefab
                mCurrentTime = 0;
            }
            foreach (var weapon in WeaponPrefabs)
            {
                var dir = EngineNS.Quaternion.RotateVector3(WeaponNode.Parent.Placement.Quat, Vector3.Forward);
                weapon.Placement.Position += dir * WeaponData.ProjectileSpeed * world.TimeSecond;
                var distance = Vector3.Distance(Vector3.Zero, weapon.Placement.Position.ToSingleVector3());
                if (distance > WeaponData.AttackRange)
                {
                    mBeRemoved.Add(weapon);
                }
            }
            foreach (var weapon in mBeRemoved)
            {
                weapon.Parent = null;
                WeaponPrefabs.Remove(weapon);
                TtEngine.Instance.GameInstance.PrefabPoolManager.ReleasePrefab(weapon);
            }
            mBeRemoved.Clear();
            base.Tick(world);
        }
        protected void Fire()
        {
            var weaponPrefabName = RName.ParseFrom(WeaponData.Shape);
            if(weaponPrefabName != null)
            {
                var WeaponPrefab = EngineNS.TtEngine.Instance.GameInstance.PrefabPoolManager.CreatePrefab(RName.ParseFrom(WeaponData.Shape));
                if (WeaponPrefab != null)
                {
                    WeaponPrefab.Parent = WeaponNode.Parent;
                    WeaponPrefab.Placement.Position = DVector3.Up;
                    WeaponPrefabs.Add(WeaponPrefab);
                }
            }  
        }
    }
    //朝向玩家最后移动方向发射n枚飞行道具，碰到怪物造成伤害。根据击穿数量判断伤害数量
    public class TtWeaponController_Back : TtWeaponController
    {
        List<TtPrefabNode> WeaponPrefabs = new List<TtPrefabNode>();
        public override void Init()
        {

        }
        List<TtPrefabNode> mBeRemoved = new List<TtPrefabNode>();
        public override void Tick(TtWorld world)
        {
            mCurrentTime += world.DeltaTimeSecond;
            if (mCurrentTime > WeaponData.CoolDown)
            {
                Fire();
                //fire prefab
                mCurrentTime = 0;
            }
            foreach (var weapon in WeaponPrefabs)
            {
                var dir = EngineNS.Quaternion.RotateVector3(WeaponNode.Parent.Placement.Quat, Vector3.Forward);
                weapon.Placement.Position += dir * WeaponData.ProjectileSpeed * world.TimeSecond;
                var distance = Vector3.Distance(Vector3.Zero, weapon.Placement.Position.ToSingleVector3());
                if (distance > WeaponData.AttackRange)
                {
                    mBeRemoved.Add(weapon);
                }
            }
            foreach (var weapon in mBeRemoved)
            {
                weapon.Parent = null;
                WeaponPrefabs.Remove(weapon);
                TtEngine.Instance.GameInstance.PrefabPoolManager.ReleasePrefab(weapon);
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
                    WeaponPrefab.Parent = WeaponNode.Parent;
                    WeaponPrefab.Placement.Position = DVector3.Up;
                    WeaponPrefabs.Add(WeaponPrefab);
                }
            }
        }
    }
    //玩家周围形成不间断的保护结界，碰撞怪物后造成伤害

    public class TtWeaponController_Circle : TtWeaponController
    {

    }
    //单个经幡以玩家为圆心旋转，每个经幡计算碰撞攻击半径，持续时间结束后消失
    public class TtWeaponController_Surround : TtWeaponController
    {

    }

    //无论角色的武器还是怪物的近远程攻击都算做武器攻击
    public class TtWeaponNode : EngineNS.GamePlay.Scene.TtSceneActorNode
    {
        public class TtWeaponNodeData : EngineNS.GamePlay.Scene.TtNodeData
        {
            [EngineNS.Rtti.Meta]
            public int WeaponId = 0;
        }
        public TtWeaponNodeData WeaponNodeData { get => NodeData as TtWeaponNodeData; }
        public override async TtTask<bool> InitializeNode(TtWorld world, TtNodeData data, EBoundVolumeType bvType, Type placementType)
        {
            await base.InitializeNode(world, data, bvType, placementType);
            var macrossSurvivorGame = EngineNS.TtEngine.Instance.GameInstance.MacrossGame as TtMacrossSurvivorGame;
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
            return true;
        }
        public TtWeaponData WeaponData { get; set; } = null;
        public TtRoleData RoleData { get; set; } = null;
        protected TtWeaponController mWeaponController = null;

        public override bool OnTickLogic(TtWorld world, TtRenderPolicy policy)
        {
            mWeaponController.Tick(world);
            return base.OnTickLogic(world, policy);
        }
    }
}
