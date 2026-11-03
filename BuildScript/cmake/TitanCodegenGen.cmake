include_guard(GLOBAL)
include("${CMAKE_CURRENT_LIST_DIR}/TitanCodegen.cmake")

# dotnet 首次运行会打印欢迎/遥测横幅，之后还原阶段又长时间静默；关掉横幅，
# 进度交给 titan_run_with_progress 的心跳输出，避免日志“有开头没后文”被当成卡死。
set(ENV{DOTNET_NOLOGO} 1)
set(ENV{DOTNET_CLI_TELEMETRY_OPTOUT} 1)
set(ENV{DOTNET_SKIP_FIRST_TIME_EXPERIENCE} 1)

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
  # Android：把工具链配置的 apiLevel 同步给 clang --target（缺省 28 = 仓库最低支持版本）。
  if(TITAN_PLATFORM STREQUAL "Android")
    if(NOT TITAN_ANDROID_API_LEVEL)
      set(TITAN_ANDROID_API_LEVEL 28)
    endif()
    list(APPEND _args "ApiLevel=${TITAN_ANDROID_API_LEVEL}")
  endif()
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

# 长任务（NuGet 还原、工具编译）在无输出时容易被误判为卡死：
# 统一用 progress-run.sh 包裹，每 10s 打印已耗时；子命令输出与退出码原样透传。
# 注意：不能拿来转发含 "\;" 转义分号的参数（函数调用会重新拆 argv），
# 所以工具执行段在 titan_run_platform_codegen 中直接展开 execute_process。
function(titan_run_with_progress label)
  execute_process(
    COMMAND bash "${TITAN_ROOT}/BuildScript/common/progress-run.sh" "${label}" 10 ${ARGN}
    WORKING_DIRECTORY "${TITAN_ROOT}"
    COMMAND_ERROR_IS_FATAL ANY)
endfunction()

# 两个源码工具都只编译自身源码：先完成自举，再执行当前平台的胶水生成。
# 调用前必须设置平台上下文、TITAN_ROOT、TITAN_CONFIGURATION 与平台 flags（提供 TITAN_<前缀>_DEFINES）。
function(titan_run_platform_codegen sysroot resource_dir)
  if(NOT TITAN_PLATFORM OR NOT TITAN_ROOT)
    message(FATAL_ERROR "[titan-codegen] 平台上下文或仓库根缺失")
  endif()
  message(STATUS "[TitanCodeGen] 1/4 刷新平台源码清单（RefreshSources，TITAN_CHECK=ON）...")
  execute_process(COMMAND "${CMAKE_COMMAND}" -DTITAN_CHECK=ON
    -P "${TITAN_ROOT}/${TITAN_PLATFORM_DIR}/RefreshSources.cmake" COMMAND_ERROR_IS_FATAL ANY)
  if(TITAN_CODEGEN_ALLOW_PARSE_ERRORS)
    message(FATAL_ERROR "AllowParseErrors 仅用于手工诊断，不能生成有效 ${TITAN_PLATFORM} stamp")
  endif()
  set(_binder "${TITAN_ROOT}/codegen/${TITAN_PLATFORM}/NativeBinder")
  file(REMOVE "${_binder}/.codegen.stamp")
  find_program(DOTNET dotnet REQUIRED)
  # TFM 单一事实源：dependencies.json 的 dotnetSdk.targetFramework。
  file(STRINGS "${TITAN_ROOT}/BuildScript/common/dependencies.json" _titan_tfm REGEX "\"targetFramework\"[ \t]*:[ \t]*\"(net[0-9.]+)\"")
  string(REGEX REPLACE ".*\"(net[0-9.]+)\".*" "\\1" TITAN_MANAGED_TFM "${_titan_tfm}")
  if("${TITAN_MANAGED_TFM}" STREQUAL "")
    message(FATAL_ERROR "无法从 dependencies.json 解析 dotnetSdk.targetFramework")
  endif()
  set(_tools "${TITAN_ROOT}/binaries/Tools/${TITAN_MANAGED_TFM}")
  set(_weaving_project "${TITAN_ROOT}/Module/CppWeavingTools/CppWeavingTools.csproj")
  set(_cs_tools_project "${TITAN_ROOT}/Module/CSharpCodeTools/CSharpCodeTools.csproj")
  # 首次还原约 150MB；NuGet 下载阶段本身不打印进度，靠心跳行证明仍在运行。
  # 网络慢可 export RestoreSources=<镜像源 v3 index> 后重跑，例如华为云 NuGet 镜像。
  message(STATUS "[TitanCodeGen] 2/4 还原 NuGet 依赖并编译两个胶水工具（首次约 150MB，慢时可 export RestoreSources=<镜像源>）...")
  message(STATUS "[TitanCodeGen] 还原 CppWeavingTools 依赖...")
  titan_run_with_progress("NuGet 还原 CppWeavingTools" "${DOTNET}" restore "${_weaving_project}" -v minimal)
  message(STATUS "[TitanCodeGen] 编译 CppWeavingTools...")
  titan_run_with_progress("编译 CppWeavingTools" "${DOTNET}" build "${_weaving_project}"
    -c "${TITAN_CONFIGURATION}" --no-restore -v minimal)
  message(STATUS "[TitanCodeGen] 还原 CSharpCodeTools 依赖...")
  titan_run_with_progress("NuGet 还原 CSharpCodeTools" "${DOTNET}" restore "${_cs_tools_project}" -v minimal
    "-p:TitanPlatform=${TITAN_PLATFORM}" -p:SkipTitanCodeGen=true)
  message(STATUS "[TitanCodeGen] 编译 CSharpCodeTools...")
  titan_run_with_progress("编译 CSharpCodeTools" "${DOTNET}" build "${_cs_tools_project}"
    -c "${TITAN_CONFIGURATION}" --no-restore -v minimal
    "-p:TitanPlatform=${TITAN_PLATFORM}" -p:SkipTitanCodeGen=true)
  titan_codegen_weaving_args(_args "${TITAN_ROOT}" "${sysroot}" "${resource_dir}"
    ${TITAN_${TITAN_PLATFORM_PREFIX}_DEFINES})
  titan_codegen_android_ext_include(_ext_include "${TITAN_ROOT}" "${sysroot}")
  if(_ext_include)
    list(APPEND _args "${_ext_include}")
  endif()
  message(STATUS "[TitanCodeGen] 3/4 运行 CppWeavingTools 解析头文件并生成绑定（约 1-3 分钟）...")
  # 不能经函数转发：_args 里的 Defines= 用 "\;" 转义分号，函数调用会把它重新拆成多个 argv；
  # execute_process 直接展开才能保持单个参数。
  execute_process(
    COMMAND bash "${TITAN_ROOT}/BuildScript/common/progress-run.sh" "CppWeavingTools 解析头文件/生成绑定" 10
      "${DOTNET}" "${_tools}/CppWeavingTools.dll" ${_args}
    WORKING_DIRECTORY "${TITAN_ROOT}" COMMAND_ERROR_IS_FATAL ANY)
  titan_codegen_source_args(_source_args "${TITAN_ROOT}")
  message(STATUS "[TitanCodeGen] 4/4 运行 CSharpCodeTools 生成托管源码...")
  execute_process(
    COMMAND bash "${TITAN_ROOT}/BuildScript/common/progress-run.sh" "CSharpCodeTools 生成托管源码" 10
      "${DOTNET}" "${_tools}/CSharpCodeTools.dll" ${_source_args}
    WORKING_DIRECTORY "${TITAN_ROOT}" COMMAND_ERROR_IS_FATAL ANY)
  titan_codegen_inputs_fingerprint(_fp _inputs "${TITAN_ROOT}")
  file(WRITE "${_binder}/.codegen.stamp" "{\"platform\":\"${TITAN_PLATFORM}\",\"configuration\":\"${TITAN_CONFIGURATION}\",\"fingerprint\":\"${TITAN_SOURCES_FINGERPRINT}\",\"inputsFingerprint\":\"${_fp}\",\"parseFailures\":0}\n")
endfunction()
