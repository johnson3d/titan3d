# macOS 原生库: binaries/<config>/Core.Mac.dylib（Mach-O dylib，托管侧经 CoreSDK.CoreModule 的 PMac 分支引用）
# 配置（仓库根）: cmake -S Core.Mac -B intermediate/cpp/Core.Mac/Debug -G Ninja -DCMAKE_BUILD_TYPE=Debug
# 中间产物统一落 intermediate/cpp/<Project>/<Config>，与 MSBuild 的 intermediate 惯例一致；源码目录内构建会被拒绝。
# 源清单来自入库的 generated/TitanSources.cmake；过期时按提示运行 cmake -P Core.Mac/RefreshSources.cmake
# 胶水需先在本机生成: cmake -P Core.Mac/GenerateCodegen.cmake
cmake_minimum_required(VERSION 3.24)
project(TitanCoreMac LANGUAGES NONE)
# 胶水 stamp 和源文件配置元数据都是单配置契约。
get_property(_titan_multi_config GLOBAL PROPERTY GENERATOR_IS_MULTI_CONFIG)
if(_titan_multi_config)
  message(FATAL_ERROR "Core.Mac requires a single-config generator (Ninja or Unix Makefiles)")
endif()

get_filename_component(TITAN_ROOT "${CMAKE_CURRENT_LIST_DIR}/.." ABSOLUTE)
include("${TITAN_ROOT}/BuildScript/cmake/TitanPlatformContext.cmake")
titan_platform_context(Mac "Core.Mac/Core.Mac.vcxproj" MAC)
include("${TITAN_ROOT}/BuildScript/cmake/TitanSourceList.cmake")
include("${TITAN_ROOT}/BuildScript/cmake/TitanCodegen.cmake")
include("${CMAKE_CURRENT_LIST_DIR}/cmake/TitanMacFlags.cmake")
include("${CMAKE_CURRENT_LIST_DIR}/generated/TitanSources.cmake")

# 只校验不重生成：Mac 的源清单、模块头和托管能力必须匹配平台工程。
execute_process(COMMAND "${CMAKE_COMMAND}" -DTITAN_CHECK=ON -P "${CMAKE_CURRENT_LIST_DIR}/RefreshSources.cmake" COMMAND_ERROR_IS_FATAL ANY)
titan_sources_fingerprint(_titan_fp "${TITAN_ROOT}" ${TITAN_SOURCES_INPUTS})
if(NOT _titan_fp STREQUAL TITAN_SOURCES_FINGERPRINT)
  message(FATAL_ERROR "[titan-sources] Core.Mac/generated/TitanSources.cmake 已过期。\n请在仓库根运行: cmake -P Core.Mac/RefreshSources.cmake 并提交生成文件")
endif()
message(STATUS "[titan-sources] fingerprint OK")
foreach(_in IN LISTS TITAN_SOURCES_INPUTS)
  set_property(DIRECTORY APPEND PROPERTY CMAKE_CONFIGURE_DEPENDS "${TITAN_ROOT}/${_in}")
endforeach()

if(NOT CMAKE_HOST_SYSTEM_NAME STREQUAL "Darwin")
  message(FATAL_ERROR "Core.Mac 只用于 macOS；Windows 请使用 Core.Window/Core.Window.vcxproj")
endif()
if(NOT CMAKE_BUILD_TYPE)
  set(CMAKE_BUILD_TYPE "${TITAN_CONFIGURATION}" CACHE STRING "Debug or Release" FORCE)
endif()
if(NOT CMAKE_BUILD_TYPE STREQUAL TITAN_CONFIGURATION)
  message(FATAL_ERROR "CMAKE_BUILD_TYPE 与 TITAN_CONFIGURATION 必须一致")
endif()
# 构建目录布局与其他 CMake 平台一致：中间产物落 intermediate/cpp/Core.Mac/<Config>，
# 在 Core.Mac 源码目录内 configure 会直接把 CMakeCache/ninja 日志写进仓库树，这里直接拒绝。
include("${TITAN_ROOT}/BuildScript/cmake/TitanBuildLayout.cmake")
titan_require_out_of_source_build("${CMAKE_CURRENT_LIST_DIR}" "${TITAN_ROOT}" "Core.Mac" "${TITAN_CONFIGURATION}")
if(CMAKE_OSX_ARCHITECTURES AND NOT CMAKE_OSX_ARCHITECTURES STREQUAL "arm64")
  message(FATAL_ERROR "Core.Mac 首版只支持 arm64")
endif()
set(CMAKE_OSX_ARCHITECTURES arm64 CACHE STRING "Mac architecture" FORCE)
enable_language(C CXX)
set(CMAKE_CXX_STANDARD 20)
set(CMAKE_CXX_STANDARD_REQUIRED ON)
set(CMAKE_CXX_EXTENSIONS OFF)
set(CMAKE_C_STANDARD 11)

option(TITAN_MAC_ALLOW_UNDEFINED "链接时允许未定义符号（只用于定位缺失模块）" OFF)

set(_norm_dir "${CMAKE_CURRENT_BINARY_DIR}/codegen_norm")
titan_prepare_codegen(_glue_srcs "${TITAN_ROOT}" "${_norm_dir}")
list(TRANSFORM TITAN_MODULE_SOURCES PREPEND "${TITAN_ROOT}/" OUTPUT_VARIABLE _module_srcs)
list(TRANSFORM TITAN_MAC_INCLUDES PREPEND "${TITAN_ROOT}/" OUTPUT_VARIABLE _module_dirs)

# 静态 libc++（3rd/native/llvm-project 源码构建的 22.1.8，与系统 SDK libc++ 同代）：
# 系统 libc++.1.dylib 导出 string/vector 出线成员（resize/push_back/append 等），其内部分配
# 走 libc++abi 的 operator new（系统堆）；而 header 内联部分（move 赋值/析构）在引擎 TU 展开、
# 释放绑定引擎 operator delete（VFX 堆）——跨堆错配会让 vfxmemory 把缓冲区前的数据垃圾当
# cookie（静默泄漏，甚至 free 垃圾指针 abort）。改用源码构建的 libc++.a（LIBCXX_ENABLE_NEW_
# DELETE_DEFINITIONS=OFF，不含 operator new/delete 定义）：编译期 -nostdinc++ 换成本地头，
# 链接期 -hidden-lc++ 把整库静态接入且符号全隐藏（防 ODR 冲突/劫持框架）；libc++.a 内部的
# operator new/delete 未定义引用在静态链接期绑定引擎 vfxnew 重载 → 标准库进出全部走 VFX 堆
set(TITAN_LIBCXX_PREFIX "${TITAN_ROOT}/3rd/native/llvm-project/install-mac")
if(NOT EXISTS "${TITAN_LIBCXX_PREFIX}/lib/libc++.a")
  message(FATAL_ERROR "[titan-libcxx] 未找到静态 libc++：${TITAN_LIBCXX_PREFIX}/lib/libc++.a（3rd/native/llvm-project 构建产物）")
endif()
set(TITAN_LIBCXX_INC "${TITAN_LIBCXX_PREFIX}/include/c++/v1")

if("3rd/native/LeanCLR/LeanCLR.vcxitems" IN_LIST TITAN_MAC_MODULES)
  set(BUILD_SHARED_LEANCLR OFF CACHE BOOL "LeanCLR 静态库" FORCE)
  if(CMAKE_BUILD_TYPE STREQUAL Debug)
    set(LEANCLR_GC_DEBUG ON CACHE BOOL "LeanCLR GC 检查" FORCE)
  else()
    set(LEANCLR_GC_DEBUG OFF CACHE BOOL "LeanCLR GC 检查" FORCE)
  endif()
  add_subdirectory("${TITAN_ROOT}/3rd/native/LeanCLR/runtime" "${CMAKE_CURRENT_BINARY_DIR}/leanclr")
  set_target_properties(leanclr PROPERTIES POSITION_INDEPENDENT_CODE ON)
  target_compile_options(leanclr PRIVATE "-nostdinc++" "-isystem" "${TITAN_LIBCXX_INC}")
endif()
add_library(CoreWindow SHARED ${_module_srcs} ${_glue_srcs})
target_compile_definitions(CoreWindow PRIVATE ${TITAN_MAC_DEFINES})
# 编译/链接全部换用静态 libc++（缘由见上方 TITAN_LIBCXX_PREFIX 块），替代系统 libc++.1.dylib
target_compile_options(CoreWindow PRIVATE "-nostdinc++" "-isystem" "${TITAN_LIBCXX_INC}")
target_link_options(CoreWindow PRIVATE
  "-nostdlib++"
  "-L${TITAN_LIBCXX_PREFIX}/lib"
  "-Wl,-hidden-lc++"
  # 新版 SDK 的 __cxa_*/dynamic_cast/personality 在独立 libc++abi.tbd，不再随 -lc++ 带入；
  # 运行期仍是 libSystem 里的同一份系统 libc++abi（异常/RTTI 单一 universe）
  "-Wl,-lc++abi")
# vfxnew.cpp 重载全局 operator new/delete 且默认可见性会导出；libc++ 的同名符号是弱符号，
# dyld 合并时引擎强符号会劫持 Metal/AppKit 等框架的 new/delete，导致全进程堆错配
# （AGXMetal 经 MTLCopyAllDevices 调引擎 delete 即崩）。属性路线（visibility hidden）
# 与 libc++ <new> 声明冲突走不通，改用链接器把 9 个标准签名从导出表剔除；
# 引擎内部引用在静态链接期已绑定，不受影响
target_link_options(CoreWindow PRIVATE
  "-Wl,-unexported_symbols_list,${CMAKE_CURRENT_SOURCE_DIR}/unexported_operators.exp")
# zlib 共享工程只收了压缩侧 8 个文件；png/tinyexr 还需要解压侧，Mac 在此补齐（Windows 链接路径不同）
if("3rd/native/zlib/zlib.vcxitems" IN_LIST TITAN_MAC_MODULES)
  target_sources(CoreWindow PRIVATE
    "${TITAN_ROOT}/3rd/native/zlib/inflate.c"
    "${TITAN_ROOT}/3rd/native/zlib/uncompr.c")
endif()
# FreeType 链接符号 FT_New_Memory/FT_Done_Memory/FT_Stream_Open 在 Windows 由平台实现提供；Mac 用自带 stdio 版
if("3rd/native/FreeType/FreeType.vcxitems" IN_LIST TITAN_MAC_MODULES)
  target_sources(CoreWindow PRIVATE
    "${TITAN_ROOT}/3rd/native/FreeType/src/base/ftsystem.c")
endif()
foreach(_entry IN LISTS TITAN_MAC_SOURCE_DEFINES)
  string(REPLACE "|" ";" _parts "${_entry}")
  list(GET _parts 0 _source)
  list(GET _parts 1 _config)
  list(GET _parts 2 _define)
  set_property(SOURCE "${TITAN_ROOT}/${_source}" APPEND PROPERTY COMPILE_DEFINITIONS "$<$<CONFIG:${_config}>:${_define}>")
endforeach()
# 规范化胶水目录必须最先，保证 Mac PODStructDefine.h 命中规范化副本
target_include_directories(CoreWindow PRIVATE
  "${_norm_dir}"
  "${TITAN_ROOT}"
  ${_module_dirs}
  "${TITAN_ROOT}/Core.Mac")
# 第一步先求能编过，警告全部关闭；错误上限放宽便于一次收集
target_compile_options(CoreWindow PRIVATE -w -ferror-limit=50)

find_package(Threads REQUIRED)
find_package(Iconv REQUIRED)
target_link_libraries(CoreWindow PRIVATE Threads::Threads Iconv::Iconv)
if(TARGET leanclr)
  target_link_libraries(CoreWindow PRIVATE leanclr)
endif()
# metis：Windows 走 metis.lib；Mac 用仓库内预编译的通用 2 架构静态库
if(EXISTS "${TITAN_ROOT}/3rd/native/metis/5.1.0/libmetis/Mac/${CMAKE_BUILD_TYPE}/libmetis.a")
  add_library(metis STATIC IMPORTED)
  set_target_properties(metis PROPERTIES
    IMPORTED_LOCATION "${TITAN_ROOT}/3rd/native/metis/5.1.0/libmetis/Mac/${CMAKE_BUILD_TYPE}/libmetis.a")
  target_link_libraries(CoreWindow PRIVATE metis)
endif()
# PhysX5：Windows 走 #pragma comment(lib) 的导入库；Mac 链接源码自建的 arm64 静态库（3rd/native/PhysX5/Mac/README.md）
if("NativeCode/Bricks/PhysicsCore/NvPhysX5/NvPhysX5.vcxitems" IN_LIST TITAN_MAC_MODULES)
  set(_titan_physx_dir "${TITAN_ROOT}/3rd/native/PhysX5/bin/mac.arm64.clang.mt")
  foreach(_pxlib PhysX PhysXCommon PhysXFoundation PhysXExtensions PhysXCooking PhysXCharacterKinematic PhysXPvdSDK PhysXVehicle2)
    add_library(px_${_pxlib} STATIC IMPORTED)
    # IMPORTED_LOCATION 不支持生成器表达式，按配置展开目录名（debug/release）
    set_target_properties(px_${_pxlib} PROPERTIES
      IMPORTED_LOCATION_DEBUG "${_titan_physx_dir}/debug/lib${_pxlib}_static_64.a"
      IMPORTED_LOCATION_RELEASE "${_titan_physx_dir}/release/lib${_pxlib}_static_64.a"
      IMPORTED_LOCATION_RELWITHDEBINFO "${_titan_physx_dir}/release/lib${_pxlib}_static_64.a"
      IMPORTED_LOCATION_MINSIZEREL "${_titan_physx_dir}/release/lib${_pxlib}_static_64.a")
    target_link_libraries(CoreWindow PRIVATE px_${_pxlib})
  endforeach()
endif()
if(TITAN_MAC_ALLOW_UNDEFINED)
  target_link_options(CoreWindow PRIVATE "LINKER:-undefined,dynamic_lookup")
endif()
# Vulkan：Mac 用仓库自带的 loader + MoltenVK（3rd/native/Vulkan/Mac/README.md），不依赖 Homebrew；
# 运行时整目录在 binaries/net8.0/vulkan/，loader 按 <exe目录>/vulkan/icd.d 找自带的 ICD 清单
if("NativeCode/NextRHI/Vulkan/Vulkan.vcxitems" IN_LIST TITAN_MAC_MODULES)
  set(TITAN_VULKAN_VENDOR_DIR "${TITAN_ROOT}/3rd/native/Vulkan/Mac")
  set(TITAN_VULKAN_RUNTIME_DIR "${TITAN_ROOT}/binaries/net8.0/vulkan")
  if(NOT EXISTS "${TITAN_VULKAN_VENDOR_DIR}/libvulkan.1.dylib")
    message(FATAL_ERROR "[titan-vulkan] 缺少 ${TITAN_VULKAN_VENDOR_DIR}/libvulkan.1.dylib（见该目录 README.md 恢复）")
  endif()
  add_library(titan_vulkan SHARED IMPORTED)
  set_target_properties(titan_vulkan PROPERTIES
    IMPORTED_LOCATION "${TITAN_VULKAN_VENDOR_DIR}/libvulkan.1.dylib")
  target_link_libraries(CoreWindow PRIVATE titan_vulkan)
  # Core.Mac 位于 binaries/<config>/，运行时库统一放在宿主 exe 目录 binaries/net8.0/vulkan/
  target_link_options(CoreWindow PRIVATE "LINKER:-rpath,@loader_path/../net8.0/vulkan")
  # 不写入 CMake 自动生成的绝对 rpath（指向本机 3rd/...），产物随目录搬移仍可解析
  set_target_properties(CoreWindow PROPERTIES SKIP_BUILD_RPATH TRUE)
  add_custom_target(titan_vulkan_runtime ALL
    COMMAND ${CMAKE_COMMAND} -E make_directory "${TITAN_VULKAN_RUNTIME_DIR}/icd.d"
    COMMAND ${CMAKE_COMMAND} -E copy_if_different "${TITAN_VULKAN_VENDOR_DIR}/libvulkan.1.dylib" "${TITAN_VULKAN_RUNTIME_DIR}/libvulkan.1.dylib"
    COMMAND ${CMAKE_COMMAND} -E copy_if_different "${TITAN_VULKAN_VENDOR_DIR}/libMoltenVK.dylib" "${TITAN_VULKAN_RUNTIME_DIR}/libMoltenVK.dylib"
    COMMAND ${CMAKE_COMMAND} -E copy_if_different "${TITAN_VULKAN_VENDOR_DIR}/icd.d/MoltenVK_icd.json" "${TITAN_VULKAN_RUNTIME_DIR}/icd.d/MoltenVK_icd.json"
    COMMENT "[titan-vulkan] 同步自带 loader/MoltenVK/ICD 到 binaries/net8.0/vulkan")
  add_dependencies(CoreWindow titan_vulkan_runtime)
endif()

set_target_properties(CoreWindow PROPERTIES
  OUTPUT_NAME "Core.Mac"
  PREFIX ""
  SUFFIX ".dylib"
  LIBRARY_OUTPUT_DIRECTORY "${TITAN_ROOT}/binaries/$<LOWER_CASE:$<CONFIG>>"
  INSTALL_NAME_DIR "@rpath"
  BUILD_WITH_INSTALL_NAME_DIR ON)
