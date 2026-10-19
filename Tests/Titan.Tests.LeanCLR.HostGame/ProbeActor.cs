namespace Titan.HostGame
{
    [EngineNS.LeanCLR.TtLeanClr]
    public class TtProbeActor
    {
        public int Hp = 100;
        public string LastGreeting;

        [EngineNS.Rtti.Meta]
        public int Damage(int amount)
        {
            Hp -= amount;
            return Hp;
        }

        [EngineNS.Rtti.Meta]
        public static int Twice(int value) => value * 2;

        [EngineNS.Rtti.Meta]
        public bool IsAlive() => Hp > 0;

        [EngineNS.Rtti.Meta]
        public float Scale(float value) => value * 1.5f;

        [EngineNS.Rtti.Meta]
        public TtProbeActor Self() => this;

        [EngineNS.Rtti.Meta]
        public string Greet(string name)
        {
            if (name == null)
                return null;
            LastGreeting = name;
            return "Hi " + name;
        }

        [EngineNS.Rtti.Meta]
        public static int Length(string text) => text == null ? -1 : text.Length;

        [EngineNS.Rtti.Meta(Flags = EngineNS.Rtti.MetaAttribute.EMetaFlags.NoMacrossUseable)]
        public int Hidden() => 7;
    }
}
