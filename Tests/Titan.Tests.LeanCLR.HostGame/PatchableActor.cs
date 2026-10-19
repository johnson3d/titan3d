namespace Titan.HostGame
{
    // 模拟 Macross 生成代码：可补丁方法首行为入口桩
    [EngineNS.LeanCLR.TtLeanClr]
    public class TtPatchableActor
    {
        public int Hp = 100;

        [EngineNS.Rtti.Meta]
        public int GetHp() => Hp;

        [EngineNS.Rtti.Meta]
        public void SetHp(int value) { Hp = value; }

        [EngineNS.Rtti.Meta]
        public int Attack(int amount)
        {
            if (EngineNS.Macross.TtMacrossPatch.Active && EngineNS.Macross.TtMacrossPatch.TryInvoke(TtPatchableActorIds.Attack, this, out System.Int32 __mcPatchRet, amount))
                return __mcPatchRet;
            return amount;
        }

        [EngineNS.Rtti.Meta]
        public int Bonus()
        {
            if (EngineNS.Macross.TtMacrossPatch.Active && EngineNS.Macross.TtMacrossPatch.TryInvoke(TtPatchableActorIds.Bonus, this, out System.Int32 __mcPatchRet))
                return __mcPatchRet;
            return 1;
        }

        [EngineNS.Rtti.Meta]
        public string Describe(string name)
        {
            if (EngineNS.Macross.TtMacrossPatch.Active && EngineNS.Macross.TtMacrossPatch.TryInvoke(TtPatchableActorIds.Describe, this, out System.String __mcPatchRet, name))
                return __mcPatchRet;
            return "orig:" + name;
        }

        [EngineNS.Rtti.Meta]
        public void Heal(int amount)
        {
            if (EngineNS.Macross.TtMacrossPatch.Active && EngineNS.Macross.TtMacrossPatch.TryInvoke(TtPatchableActorIds.Heal, this, amount))
                return;
            Hp += amount;
        }

        [EngineNS.Rtti.Meta]
        public int Explode()
        {
            if (EngineNS.Macross.TtMacrossPatch.Active && EngineNS.Macross.TtMacrossPatch.TryInvoke(TtPatchableActorIds.Explode, this, out System.Int32 __mcPatchRet))
                return __mcPatchRet;
            return -1;
        }

        [EngineNS.Rtti.Meta]
        public float Ratio(float value)
        {
            if (EngineNS.Macross.TtMacrossPatch.Active && EngineNS.Macross.TtMacrossPatch.TryInvoke(TtPatchableActorIds.Ratio, this, out System.Single __mcPatchRet, value))
                return __mcPatchRet;
            return value;
        }

        [EngineNS.Rtti.Meta]
        public TtPatchableActor Echo(TtPatchableActor other)
        {
            if (EngineNS.Macross.TtMacrossPatch.Active && EngineNS.Macross.TtMacrossPatch.TryInvoke(TtPatchableActorIds.Echo, this, out Titan.HostGame.TtPatchableActor __mcPatchRet, other))
                return __mcPatchRet;
            return null;
        }

        [EngineNS.Rtti.Meta]
        public int Chain(int amount)
        {
            if (EngineNS.Macross.TtMacrossPatch.Active && EngineNS.Macross.TtMacrossPatch.TryInvoke(TtPatchableActorIds.Chain, this, out System.Int32 __mcPatchRet, amount))
                return __mcPatchRet;
            return 0;
        }

        [EngineNS.Rtti.Meta]
        public int Untouched(int amount)
        {
            if (EngineNS.Macross.TtMacrossPatch.Active && EngineNS.Macross.TtMacrossPatch.TryInvoke(TtPatchableActorIds.Untouched, this, out System.Int32 __mcPatchRet, amount))
                return __mcPatchRet;
            return amount + 1;
        }

        [EngineNS.Rtti.Meta]
        public EElement GetElement(EElement element)
        {
            if (EngineNS.Macross.TtMacrossPatch.Active && EngineNS.Macross.TtMacrossPatch.TryInvoke(TtPatchableActorIds.GetElement, this, out Titan.HostGame.EElement __mcPatchRet, element))
                return __mcPatchRet;
            return element;
        }

        [EngineNS.Rtti.Meta]
        public FDamage ApplyDamage(FDamage damage)
        {
            if (EngineNS.Macross.TtMacrossPatch.Active && EngineNS.Macross.TtMacrossPatch.TryInvoke(TtPatchableActorIds.ApplyDamage, this, out Titan.HostGame.FDamage __mcPatchRet, damage))
                return __mcPatchRet;
            return damage;
        }

        // 无入口桩：补丁经 intrinsic 调到这里，再经 Attack 的入口桩嵌套进入 VM
        [EngineNS.Rtti.Meta]
        public int AttackTwice(int amount) => Attack(amount) + Attack(amount);
    }

    // 已注册的派生宿主类型：其 VM 桩继承基类桩，不继承基类的 __Patch
    [EngineNS.LeanCLR.TtLeanClr]
    public class TtPatchableActorChild : TtPatchableActor
    {
        [EngineNS.Rtti.Meta]
        public int ChildBonus() => 5;
    }

    // 补丁校验用：其 __Patch 故意改了返回类型并声明重载
    [EngineNS.LeanCLR.TtLeanClr]
    public class TtPatchCheckActor
    {
        [EngineNS.Rtti.Meta]
        public int Value() => 1;

        [EngineNS.Rtti.Meta]
        public int Pick(int value) => value;

        [EngineNS.Rtti.Meta]
        public int Pick(float value) => (int)value;
    }

    public static class TtPatchableActorIds
    {
        public const string ClassName = "Titan.HostGame.TtPatchableActor";
        public static readonly ulong Attack = Id("Attack(System.Int32)");
        public static readonly ulong Bonus = Id("Bonus()");
        public static readonly ulong Describe = Id("Describe(System.String)");
        public static readonly ulong Heal = Id("Heal(System.Int32)");
        public static readonly ulong Explode = Id("Explode()");
        public static readonly ulong Ratio = Id("Ratio(System.Single)");
        public static readonly ulong Echo = Id("Echo(Titan.HostGame.TtPatchableActor)");
        public static readonly ulong Chain = Id("Chain(System.Int32)");
        public static readonly ulong Untouched = Id("Untouched(System.Int32)");
        public static readonly ulong GetElement = Id("GetElement(Titan.HostGame.EElement)");
        public static readonly ulong ApplyDamage = Id("ApplyDamage(Titan.HostGame.FDamage)");
        public static string Signature(string method) => ClassName + "::" + method;
        private static ulong Id(string method) => EngineNS.Macross.TtMacrossPatchId.Compute(Signature(method));
    }
}
