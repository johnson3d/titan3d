# macOS 本地生成 C++/C# 胶水（等价于 Windows 的 BuildScript/msbuild/TitanCodeGen.targets）
# 用法（仓库根）: cmake -DTITAN_CONFIGURATION=Debug -P Core.Mac/GenerateCodegen.cmake
# Release 改用 TITAN_CONFIGURATION=Release；解析失败不能放行生成有效 stamp。
# 自举与执行段在 BuildScript/cmake/TitanCodegenGen.cmake，各平台共用；此处只提供主机工具链。
cmake_minimum_required(VERSION 3.24)
get_filename_component(TITAN_ROOT "${CMAKE_CURRENT_LIST_DIR}/.." ABSOLUTE)

if(NOT CMAKE_HOST_SYSTEM_NAME STREQUAL "Darwin")
  message(FATAL_ERROR "GenerateCodegen.cmake 只用于 macOS；Windows 由 BuildScript/msbuild/TitanCodeGen.targets 生成")
endif()

include("${TITAN_ROOT}/BuildScript/cmake/TitanPlatformContext.cmake")
titan_platform_context(Mac "Core.Mac/Core.Mac.vcxproj" MAC)
include("${CMAKE_CURRENT_LIST_DIR}/cmake/TitanMacFlags.cmake")
include("${TITAN_ROOT}/BuildScript/cmake/TitanCodegen.cmake")
include("${TITAN_ROOT}/BuildScript/cmake/TitanCodegenGen.cmake")

execute_process(COMMAND xcrun --show-sdk-path
  OUTPUT_VARIABLE _sysroot OUTPUT_STRIP_TRAILING_WHITESPACE COMMAND_ERROR_IS_FATAL ANY)
execute_process(COMMAND xcrun clang -print-resource-dir
  OUTPUT_VARIABLE _resource_dir OUTPUT_STRIP_TRAILING_WHITESPACE COMMAND_ERROR_IS_FATAL ANY)
message(STATUS "[TitanCodeGen] SysRoot=${_sysroot}")
message(STATUS "[TitanCodeGen] ResourceDir=${_resource_dir}")

titan_run_platform_codegen("${_sysroot}" "${_resource_dir}")
message(STATUS "[TitanCodeGen] Code generation completed.")
