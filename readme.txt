For Vulkan:
Building
1 Open EngineAll.sln with VS2019/2022 
2 Compile Core.Window(C++2019)
3 Compile Engine.Window(C#)
4 Compile Core.Window(C++2019) again(first time only: Compile Engine.Window maybe genarate some cpp files !_!)
5 Active MainEditor Project
6 Set Debug Command: config=$(SolutionDir)content\EngineConfigForVK.cfg use_renderdoc=false
7 Set working directory: $(SolutionDir)binaries\
8 Run-Debug MainEditor

Reproduce:
1 Run Application
2 Double Click: ContentBrowser/Game/utest/box_wite
3 Double Click: ContentBrowser/Game/utest/ddd

NativeCode\NextRHI\Vulkan\VKEvent.cpp line 91
vkGetSemaphoreCounterValue will get 0xffffffffffffffff 
but hr is VK_SUCCESS still, and nothing output by validation