namespace Titan.HostGame
{
    public class TtPatchableActor__Patch : TtPatchableActor
    {
        public new int Attack(int amount) => amount * 10 + Bonus();
        public new string Describe(string name) => "patched:" + name + ":" + GetHp();
        public new void Heal(int amount) => SetHp(GetHp() + amount * 2);
        public new int Explode() => throw new System.InvalidOperationException("patch boom");
        public new float Ratio(float value) => value * 0.25f;
        public new TtPatchableActor Echo(TtPatchableActor other) => other ?? this;
        public new int Chain(int amount) => AttackTwice(amount);
        public new EElement GetElement(EElement element) => element == EElement.Fire ? EElement.Ice : EElement.Lightning;
        public new FDamage ApplyDamage(FDamage damage) => new FDamage { Amount = damage.Amount * 2, Element = damage.Element + 10 };
    }
}
