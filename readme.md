---

---

```

```

**开源协议**

​	暂时决定先采用LGPL协议吧，后面看情况松绑:)

**安装步骤**

1下载附加资源
	https://pan.baidu.com/s/1nK8uyY01y8dwnVj-8mbotg 提取码：6666 

2解压
	titan3d_extra.rar请解压到引擎根目录

3运行Setup.bat(目前这一步不需要了-.-)
	他将解压拷贝必要的dll到binaries对应的目录

4使用vs2017或者vs2019打开工程
	路径在根目录的EngineALl.sln

5先编译Native下的Core.Windows工程

6编译CSharp下的GameBuilder
	这是一个控制台程序，此后编辑器会调用来编译宏图

7编译CSharp下的CoreEditor

8调试运行CoreEditor工程

<img src="https://github.com/johnson3d/titan3d/blob/master/document/pic/t1.png" alt="t1" style="zoom: 80%;" />

**引擎特色**

1. 开源，采用模块化共享工程的思想搭建的整个引擎，自定义扩充和精简引擎快捷高效
2. 无论PC平台还是移动平台，都可以在visual studio中轻松打包与调试
3. 面向新一代渲染API构架，基于多线程设计
4. 底层采用C++编写，框架层采用C#，逻辑层采用Graph编辑器，兼顾了运行效率与开发效率
5. 基于协程思想设计的异步框架，提供了对用户非常友好的多线程，异步开发模式
6. 轻量级的渲染管线支持，大量RHI接口通过C#模式提供，用户可以高度定制自己的渲染管线和策略

**特性列表**

1. 内核C++实现，主框架，编辑器等由C#实现，通过P-Invoke实现托管非托管调用， 通过C++和C#的反射绑定托管与非托管对象数据
3. 围绕宏图(Macross)构架游戏逻辑
4. 高度可定制的渲染管线，可全部在C#中实现渲染特性扩展
5. 支持ShaderGraph的材质编辑器
6. 基于Graph编辑器的动作图，BlendSpace等
7. 基于Macross的UI编辑与事件响应
8. 基于Meta元数据的序列化构架，几乎无损耗的无限版本的正确兼容读取
9. 基于Macross的全代码驱动粒子系统
10. 基于Macross的行为树与状态机编辑器器
11. 初步的GpuDriven
12. 基于excel的表单数据驱动，可和macross完美互动

**todo**

1. 重构UI系统，使得编辑器也使用自己的ui绘制，提高性能和方便跨平台
2. 基于ComputeShader思想重构粒子系统
3. 完善打包构建流程
4. 实现Hi-z，RVT等，实现完善的GpuDriven面向新的渲染构架
5. 扩充例如tilebase ds等的渲染策略
6. 资产集合包的导出与导入，完善资产的重命名，修改路径，删除等操作
7. 分布式构建
8. 地形与超大世界

**命令行**

EditorCMD示例

1. Cook游戏

   cook entry=samplers/mergeinstance/mergeinstance.macross platform=android shadermodel=5+4+3 copyrinfo cookshader recompile genvsproj ddc_clear_texture texencoder=PNG+ETC2 pak=D:/OpenSource/titan3d/cooked/android/a.tpak

2. 强制刷新修复rinfo的引用关系记录

   fresh_rinfo dir=E:\titan3d\Content\editor  type=gms+instmtl+material+macross+uvanim

3. 重命名资源

   rname_change  name=editor/basemesh/box.vms#editor/basemesh/box1.vms

   rname_change  dir=editor/base&mesh/#editor/basemesh/ 注意，这里&代替了空格

   rname_change  savemap=map/demo0.map

4. 打包tpak文件

   pack src=D:/OpenSource/titan3d/cooked/android/a.alist tar=D:/OpenSource/titan3d/cooked/android/a.tpak

   src参数assetslist文件为纯文本，内容是需要打包的资产清单

   一行描述一个资产的源与包内目标路径，每行格式如下：

   D:/OpenSource/titan3d/cooked/android/Assets/content/cenginedesc.cfg Assets/content/cenginedesc.cfg normal

   tar参数是生成的tpak目标包文件路径，打包完成会生成一个.ftab的伴随文件，用于对包内资源进行定位和描述，还会生成一个.vtree文件描述包内虚拟文件树，可以用VrDirectory来加载

5. 

AloneGame示例

1. usecooked=android gpu=Adreno540 create_debug_layer textureformat=none 纹理格式目前只有none和etc2