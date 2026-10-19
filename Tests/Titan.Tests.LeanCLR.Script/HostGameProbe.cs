namespace Titan.HotPatch;

public static class HostGameProbe
{
    public static int Hit(Titan.HostGame.TtProbeActor actor, int amount) => actor.Damage(amount) + Titan.HostGame.TtProbeActor.Twice(1);

    public static bool SelfIsSame(Titan.HostGame.TtProbeActor actor) => actor.Self() == actor && actor.IsAlive();

    public static float Scale(Titan.HostGame.TtProbeActor actor, float value) => actor.Scale(value);

    // string 参数与返回值：VM 字面量传给宿主，宿主新建的串回到 VM 再参与比较与拼接
    public static bool StringRoundTrip(Titan.HostGame.TtProbeActor actor) =>
        actor.Greet("勇者") == "Hi 勇者" &&
        (actor.Greet("x") + "!").Length == 5 &&
        actor.Greet(null) == null &&
        Titan.HostGame.TtProbeActor.Length("勇者") == 2 &&
        Titan.HostGame.TtProbeActor.Length("") == 0 &&
        Titan.HostGame.TtProbeActor.Length(null) == -1 &&
        actor.Greet("勇者") == "Hi 勇者";
}
