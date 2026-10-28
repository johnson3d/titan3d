# Android 本地生成 C++/C# 胶水（等价于 Windows 的 BuildScript/msbuild/TitanCodeGen.targets）
# 用法（仓库根）: cmake "-DTITAN_NDK_ROOT=<r27>" -DTITAN_CONFIGURATION=Debug -P Core.Android/GenerateCodegen.cmake
# Release 改用 TITAN_CONFIGURATION=Release；解析失败不能放行生成有效 stamp。
# 自举与执行段在 BuildScript/cmake/TitanCodegenGen.cmake，各平台共用；此处只提供主机工具链。
cmake_minimum_required(VERSION 3.24)
get_filename_component(TITAN_ROOT "${CMAKE_CURRENT_LIST_DIR}/.." ABSOLUTE)

if(NOT CMAKE_HOST_SYSTEM_NAME STREQUAL "Windows")
  message(FATAL_ERROR "一期 Android 胶水生成要求 Windows 主机")
endif()

if(NOT TITAN_NDK_ROOT)
  set(TITAN_NDK_ROOT "$ENV{ANDROID_NDK_ROOT}")
endif()
set(_llvm "${TITAN_NDK_ROOT}/toolchains/llvm/prebuilt/windows-x86_64")
set(_sysroot "${_llvm}/sysroot")
file(GLOB _resource_dirs LIST_DIRECTORIES TRUE "${_llvm}/lib/clang/*")
list(SORT _resource_dirs COMPARE NATURAL ORDER DESCENDING)
if(NOT EXISTS "${_sysroot}/usr/include/jni.h" OR NOT _resource_dirs)
  message(FATAL_ERROR "NDK sysroot/resource-dir 缺失；传入 TITAN_NDK_ROOT=<r27 目录>")
endif()
list(GET _resource_dirs 0 _resource_dir)

include("${TITAN_ROOT}/BuildScript/cmake/TitanPlatformContext.cmake")
titan_platform_context(Android "Core.Android/Core.Android.vcxproj" ANDROID)
include("${TITAN_ROOT}/BuildScript/cmake/TitanCodegen.cmake")
include("${TITAN_ROOT}/BuildScript/cmake/TitanCodegenGen.cmake")
include("${CMAKE_CURRENT_LIST_DIR}/cmake/TitanAndroidFlags.cmake")

message(STATUS "[TitanCodeGen] SysRoot=${_sysroot}")
message(STATUS "[TitanCodeGen] ResourceDir=${_resource_dir}")
titan_run_platform_codegen("${_sysroot}" "${_resource_dir}")
message(STATUS "[TitanCodeGen] Code generation completed.")
