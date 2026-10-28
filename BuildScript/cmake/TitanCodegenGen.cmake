include_guard(GLOBAL)
include("${CMAKE_CURRENT_LIST_DIR}/TitanCodegen.cmake")

# 托管源码生成只读取 <平台托管工程> 的求值输入；参数列表保留含空格的根目录。
function(titan_codegen_source_args out_var titan_root)
  if(NOT TITAN_CONFIGURATION)
    set(TITAN_CONFIGURATION Debug)
  endif()
  if(NOT TITAN_CONFIGURATION MATCHES "^(Debug|Release)$")
    message(FATAL_ERROR "TITAN_CONFIGURATION must be Debug or Release")
  endif()
  set(_args
    "${titan_root}/Rpc_Engine.txt"
    "mode=Rpc+AutoSync+Macross"
    "TitanRoot=${titan_root}"
    "Platform=${TITAN_PLATFORM}"
    "Configuration=${TITAN_CONFIGURATION}"
    "csproj=${titan_root}/Module/${TITAN_PLATFORM_ENGINE}/${TITAN_PLATFORM_ENGINE}.csproj")
  set(${out_var} "${_args}" PARENT_SCOPE)
endfunction()

# 组装 CppWeavingTools 的平台参数；ARGN 为宏列表，合并成单个 Defines= 参数（分号转义，避免被拆成多个 argv）
function(titan_codegen_weaving_args out_var titan_root sysroot resource_dir)
  if(NOT TITAN_PLATFORM OR NOT TITAN_PLATFORM_DIR)
    message(FATAL_ERROR "[titan-codegen] 先调用 titan_platform_context")
  endif()
  set(_out "${titan_root}/codegen/${TITAN_PLATFORM}/NativeBinder")
  if(NOT TITAN_CONFIGURATION)
    set(TITAN_CONFIGURATION Debug)
  endif()
  if(NOT TITAN_CONFIGURATION MATCHES "^(Debug|Release)$")
    message(FATAL_ERROR "TITAN_CONFIGURATION must be Debug or Release")
  endif()
  list(JOIN ARGN "\\;" _defines)
  set(_args
    "vcxproj=${titan_root}/${TITAN_PLATFORM_PROJECT}"
    "CppOut=${_out}"
    "CsOut=${_out}"
    "ModuleNC=EngineNS.CoreSDK.CoreModule"
    "Pch=${titan_root}/${TITAN_PLATFORM_DIR}/pch.h"
    "TargetCppPOD=${_out}/PODStructDefine.h"
    "Platform=${TITAN_PLATFORM}"
    "Configuration=${TITAN_CONFIGURATION}"
    "SysRoot=${sysroot}"
    "ResourceDir=${resource_dir}"
    "Defines=${_defines}")
  set(${out_var} "${_args}" PARENT_SCOPE)
endfunction()

# Android 解析器需要 flags 里的平台内 include（PhysX 等已不留在 vcxproj）；
# 整体作为单个 argv 传入，含空格/分号时必须转义，否则会被拆成多个参数。
function(titan_codegen_android_ext_include out_var titan_root sysroot)
  if(NOT TITAN_PLATFORM STREQUAL "Android")
    set(${out_var} "" PARENT_SCOPE)
    return()
  endif()
  list(TRANSFORM TITAN_ANDROID_INCLUDES PREPEND "${titan_root}/" OUTPUT_VARIABLE _extra)
  list(APPEND _extra "${sysroot}/usr/include/c++/v1" "${sysroot}/usr/include/aarch64-linux-android")
  list(REMOVE_DUPLICATES _extra)
  list(JOIN _extra "\\;" _ext)
  set(${out_var} "ExtInclude=${_ext}" PARENT_SCOPE)
endfunction()

# 两个源码工具都只编译自身源码：先完成自举，再执行当前平台的胶水生成。
# 调用前必须设置平台上下文、TITAN_ROOT、TITAN_CONFIGURATION 与平台 flags（提供 TITAN_<前缀>_DEFINES）。
function(titan_run_platform_codegen sysroot resource_dir)
  if(NOT TITAN_PLATFORM OR NOT TITAN_ROOT)
    message(FATAL_ERROR "[titan-codegen] 平台上下文或仓库根缺失")
  endif()
  execute_process(COMMAND "${CMAKE_COMMAND}" -DTITAN_CHECK=ON
    -P "${TITAN_ROOT}/${TITAN_PLATFORM_DIR}/RefreshSources.cmake" COMMAND_ERROR_IS_FATAL ANY)
  if(TITAN_CODEGEN_ALLOW_PARSE_ERRORS)
    message(FATAL_ERROR "AllowParseErrors 仅用于手工诊断，不能生成有效 ${TITAN_PLATFORM} stamp")
  endif()
  set(_binder "${TITAN_ROOT}/codegen/${TITAN_PLATFORM}/NativeBinder")
  file(REMOVE "${_binder}/.codegen.stamp")
  find_program(DOTNET dotnet REQUIRED)
  set(_tools "${TITAN_ROOT}/binaries/Tools/net8.0")
  message(STATUS "[TitanCodeGen] Building CppWeavingTools...")
  execute_process(COMMAND "${DOTNET}" build "${TITAN_ROOT}/Module/CppWeavingTools/CppWeavingTools.csproj"
    -c "${TITAN_CONFIGURATION}" --nologo -v q
    WORKING_DIRECTORY "${TITAN_ROOT}" COMMAND_ERROR_IS_FATAL ANY)
  message(STATUS "[TitanCodeGen] Building CSharpCodeTools...")
  execute_process(COMMAND "${DOTNET}" build "${TITAN_ROOT}/Module/CSharpCodeTools/CSharpCodeTools.csproj"
    -c "${TITAN_CONFIGURATION}" "-p:TitanPlatform=${TITAN_PLATFORM}" -p:SkipTitanCodeGen=true --nologo -v q
    WORKING_DIRECTORY "${TITAN_ROOT}" COMMAND_ERROR_IS_FATAL ANY)
  titan_codegen_weaving_args(_args "${TITAN_ROOT}" "${sysroot}" "${resource_dir}"
    ${TITAN_${TITAN_PLATFORM_PREFIX}_DEFINES})
  titan_codegen_android_ext_include(_ext_include "${TITAN_ROOT}" "${sysroot}")
  if(_ext_include)
    list(APPEND _args "${_ext_include}")
  endif()
  message(STATUS "[TitanCodeGen] Running CppWeavingTools...")
  execute_process(COMMAND "${DOTNET}" "${_tools}/CppWeavingTools.dll" ${_args}
    WORKING_DIRECTORY "${TITAN_ROOT}" COMMAND_ERROR_IS_FATAL ANY)
  titan_codegen_source_args(_source_args "${TITAN_ROOT}")
  message(STATUS "[TitanCodeGen] Running CSharpCodeTools...")
  execute_process(COMMAND "${DOTNET}" "${_tools}/CSharpCodeTools.dll" ${_source_args}
    WORKING_DIRECTORY "${TITAN_ROOT}" COMMAND_ERROR_IS_FATAL ANY)
  titan_codegen_inputs_fingerprint(_fp _inputs "${TITAN_ROOT}")
  file(WRITE "${_binder}/.codegen.stamp" "{\"platform\":\"${TITAN_PLATFORM}\",\"configuration\":\"${TITAN_CONFIGURATION}\",\"fingerprint\":\"${TITAN_SOURCES_FINGERPRINT}\",\"inputsFingerprint\":\"${_fp}\",\"parseFailures\":0}\n")
endfunction()
