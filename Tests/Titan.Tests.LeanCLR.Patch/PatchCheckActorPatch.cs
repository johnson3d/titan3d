namespace Titan.HostGame
{
    // 只用于 Stage B 拒绝用例，不会被激活
    public class TtPatchCheckActor__Patch : TtPatchCheckActor
    {
        public new string Value() => "wrong return type";
        public new int Pick(int value) => value + 1;
        public new int Pick(float value) => (int)value + 1;
    }
}
