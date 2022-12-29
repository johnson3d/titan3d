# 常用代码
## 1.性能分析
```C#
[ThreadStatic]
private static Profiler.TimeScope ScopeTick = Profiler.TimeScopeManager.GetTimeScope(typeof(UMovement), nameof(TickLogic));
using (new Profiler.TimeScopeHelper(ScopeTick))
{
	//do sth
}
```
## 2.插件开发
1.确保插件dll包含如下实现
UPluginLoader是用来描述插件的
AssemblyEntry是用来描述DLL模块在UTypeDesc的信息的，通常处理类型和Meta信息
```C#
namespace EngineNS.Plugins.你的插件名
{
    public class UPluginLoader
    {
        public static UGameItemPlugin? mPluginObject = new UGameItemPlugin();
        public static Bricks.AssemblyLoader.UPlugin GetPluginObject()
        {
            return mPluginObject;
        }
    }
}

namespace EngineNS.Rtti
{
    public class AssemblyEntry
    {
        public class UGameServerAssemblyDesc : UAssemblyDesc
        {
            public UGameServerAssemblyDesc()
            {
                Profiler.Log.WriteLine(Profiler.ELogTag.Info, "Core", "Plugins:你的插件名 AssemblyDesc Created");
            }
            ~UGameServerAssemblyDesc()
            {
                Profiler.Log.WriteLine(Profiler.ELogTag.Info, "Core", "Plugins:你的插件名 AssemblyDesc Destroyed");
            }
            public override string Name { get => "你的插件名"; }
            public override string Service { get { return "Plugins"; } }
            public override bool IsGameModule { get { return false; } }
            public override string Platform { get { return "Global"; } }
        }
        static UGameServerAssemblyDesc AssmblyDesc = new UGameServerAssemblyDesc();
        public static UAssemblyDesc GetAssemblyDesc()
        {
            return AssmblyDesc;
        }
    }
}

```
2.插件编译结果路径
binaries\Plugins\Debug\net6.0
3.在插件目录还要添加插件同名.plugin文件，内容大致如下：
```XML
<?xml version="1.0" encoding="utf-8"?>
<Root Type="EngineNS.Bricks.AssemblyLoader.UPluginDescriptor@EngineCore">
  <LoadOnInit Type="System.Boolean@Unknown" Value="False" />
  <Platforms Type="System.Collections.Generic.List&lt;EngineNS.EPlatformType@EngineCore,&gt;@Unknown" Count="1">
    <e_0 Value="PLTF_Windows" />
  </Platforms>
  <Dependencies Type="System.Collections.Generic.List&lt;System.String@Unknown,&gt;@Unknown" Count="1">
    <e_0 Value="GameServer" />
  </Dependencies>
</Root>
```
Plugins目录下CopyPlugins.bat在修改*.plugin后目前需要手工执行，刷新到插件目录
## 3.Native Bricks开发
目前需要在Base/BaseHead.h里面根据平台添加类似 #define HasModule_NextRHI 
这个用来确保生成胶水代码参与编译构建，否则会发生C#调用C++找不到函数