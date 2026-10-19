namespace EngineNS.LeanCLR
{
    /// <summary>
    /// 所有 [TtLeanClr] 宿主类在 VM 中的桩基类。
    /// HostHandle 由宿主侧通过 native 字段偏移写入（宿主对象的弱 GCHandle），脚本不可访问。
    /// </summary>
    public abstract class TtLeanClrHostObject
    {
#pragma warning disable CS0169
        private System.IntPtr HostHandle;
#pragma warning restore CS0169

        protected TtLeanClrHostObject()
        {
        }
    }
}
