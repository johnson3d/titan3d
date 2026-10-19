// 枚举与可 blittable 结构体——宿主、桩、补丁三个程序集共享。
// 这些类型用于端到端测试 enum/struct 参数的补丁 ABI。
namespace Titan.HostGame
{
    public enum EElement
    {
        Fire = 0,
        Ice = 1,
        Lightning = 2,
    }

    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public struct FDamage
    {
        public int Amount;
        public int Element;
    }
}
