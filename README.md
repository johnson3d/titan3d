# 编译运行环境
- Titan3D 启动！
- ![总览图](Documents/picture/main_edtor.png)
- 1.安装2022
- 2.安装C#开发环境
- 3.建议安装C#移动开发环境
# 引擎特色
- 1.C++/C#混合编程，C++作为底层，C#作为上层，通过自动胶水代码残生，C#可以完成完整调用C++功能
- 2.基于协程(Coroutines)的多线程构架，告别Callback Hell
- 3.基于宏图的图形化脚本构架，用户可以0代码实现超复杂游戏逻辑
- 4.完全用户自定义渲染管线，通过编辑器编辑RenderGraph，通过C#扩展RenderGraphNode，真正做到用户需要的一切效果都自定制
- 5.自带多进程服务器集群构架，大型MMO项目可用
- 6.基于HLSL的Shading开发，全平台Shader反射后统一了渲染资源绑定，是最接近原生DX开发用户习惯的模式
# 支持特性
- 1.支持DX11,DX12,Vulkan,OpenGLES(废弃)
- 2.支持Windows,Android平台
- 3.Amplification Shader,Mesh Shader,RayTracing Shader
- 4.图形化编辑器配置RenderGraph资产，引擎自带DeferredShading和MobileShading配置RenderGraph资产
- 5.图形化Material Shader编辑器
- 6.InGame UI编辑器支持2D,3D游戏UI编辑
- 7.GpuScene+IndirectDraw构成GpuDriven结构
- 8.粒子系统，采用图形化逻辑编辑，支持CPU,GPU粒子切换
- 9.SDF字体，放缩友好
- 10.基于Node的场景编辑器，世界大纲可以为任何Node设置csharp脚本
- 11.图形化动作状态机，和宏图配合完成游戏逻辑
- 12.图形化脚本编辑器，宏图(Macross)系统几乎可以全功能实现引擎项目功能
- 13.内嵌RenderDoc，可以config配置
- 14.基于TCP/IP的RPC网络通讯，自带超大型服务器集群构架
- 15.采用双精度坐标，支持CDLOD地形，先天无限世界圣体
- 16.Excel数据自动映射数据结构，自动读写
- 17.插件动态加载卸载，游戏，引擎功能都可通过插件扩展
- 18.专用的prefab编辑器
# 编译构建
## Windows编译引擎
1. **第一次编译引擎，很多时候需要单独调试运行CppWeavingTools和CSharpCodeTools两个工程一次，确保codegen下面NativeBinder和Cs2Cpp目录产生了必要的临时cpp,cs文件** 
2. 如果第一次产生NativeBinder失败，有可能需要安装llvm
3. 编译Core.Window工程（C++）
4. 编译Engine.Window工程（C#）
5. 编译MainEditor工程（C#）
6. **因为github的LFS限制，可能需要运行一下Setup.bat做一些运行环境配置**
## Windows编译Android APK
1. 编译Core.Android工程（C++）
2. 编译Engine.Android程（C#）
# 调试与运行
1. 设置MainEditor为当前项目
2. 调试命令行参数为config=\$(SolutionDir)content\EngineConfig.cfg use_renderdoc=false
3. 调试工作目录为$(SolutionDir)binaries\
4. 运行与调试，请阅读[**引擎配置与编辑器使用文档**](Documents/Index.md)。
5. 遇到一些奇怪IO相关Crash或者异常，可以尝试删除本地cache目录
# 开发者注意事项
1. 不要提交大文件(20M以上)，避免lfs使用
2. [常用代码](CodeLib.md)
3. **新增加了C++的Bricks一定要记得添加对应宏**，否则会C#找不到C++函数，方法参阅注意事项2
# 控制台程序
## 特殊参数
- 1.ExeCmd=决定执行的命令
- 2.ExtraCmd={n}这个n是确定启动后，控制台可以输入的参数个数
## 保存资产到最新
- 保存指定资产到最新版本，解决MetaVersion爆炸问题
ExeCmd=SaveAsLastest AssetType=Scene+Mesh+Material+MaterialInst+Texture CookCfg=\$(SolutionDir)content\EngineConfigForCook.cfg 
## 启动Root服务器
- 方法1：ExeCmd=StartRootServer CookCfg=\$(SolutionDir)content\EngineConfigForRootServer.cfg 
- 方法2：ExtraCmd=1 CookCfg=$(SolutionDir)content\EngineConfigForRootServer.cfg 在控制台输入ExeCmd=StartRootServer
## 启动Login服务器
- 方法1：ExeCmd=StartLoginServer CookCfg=\$(SolutionDir)content\EngineConfigForRootServer.cfg 
- 方法2：ExtraCmd=1 CookCfg=$(SolutionDir)content\EngineConfigForRootServer.cfg 在控制台输入ExeCmd=StartLoginServer
## 升级CppWeavingTools
- 升级Nuget的libclang，本机查找Microsoft Visual Studio\2022\Enterprise\VC\Tools\Llvm\x64\bin拷贝到binaries\Tools\对应.net版本
- 右键libClangSharp查看nuget文件位置