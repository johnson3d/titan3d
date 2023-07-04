# 编译运行环境
- 1.安装2022
- 2.安装C#开发环境
- 3.建议安装C#移动开发环境
# 编译构建
## Windows编译引擎
1. 编译Core.Window工程（C++）
2. 编译Engine.Window工程（C#）
3. 编译MainEditor工程（C#）
4. 第一次编译引擎如果失败，报告缺失一些生成代码，请单独调试运行CppWeavingTools和CSharpCodeTools两个工程一次 
## Windows编译Android APK
1. 编译Core.Android工程（C++）
2. 编译Engine.Android程（C#）
# 调试与运行
1. 设置MainEditor为当前项目
2. 调试命令行参数为config=\$(SolutionDir)content\EngineConfig.cfg use_renderdoc=false
3. 调试工作目录为$(SolutionDir)binaries\
4. Run/Debug [编辑器使用文档](Documents\Index.md)。
# 开发者注意事项
1. 不要提交大文件(20M以上)，避免lfs使用
2. [常用代码](:/3404bc5266aa4459a6e2be96b56ac5bf)
3. 新增加了C++的Bricks一定要记得添加对应宏，否则会C#找不到C++函数，方法参阅注意事项2
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