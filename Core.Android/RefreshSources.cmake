# Android 原生源清单一键刷新（仓库根执行）:
#   cmake -P Core.Android/RefreshSources.cmake                  刷新 Core.Android/generated/TitanSources.cmake
#   cmake -DTITAN_CHECK=ON -P Core.Android/RefreshSources.cmake 只检查是否过期（CI 用，过期返回非 0）
# 何时需要: 改了任何 .vcxitems、Core.Android.vcxproj 的 Shared Import、Core.Android/AndroidModules.cmake 之后。
# 刷新后提交生成文件。写入与能力渲染逻辑在 BuildScript/cmake/TitanRefreshSources.cmake，各平台共用。
cmake_minimum_required(VERSION 3.24)
get_filename_component(TITAN_ROOT "${CMAKE_CURRENT_LIST_DIR}/.." ABSOLUTE)
include("${TITAN_ROOT}/BuildScript/cmake/TitanPlatformContext.cmake")
titan_platform_context(Android "Core.Android/Core.Android.vcxproj" ANDROID)
include("${TITAN_ROOT}/BuildScript/cmake/TitanRefreshSources.cmake")
titan_refresh_platform_sources()
