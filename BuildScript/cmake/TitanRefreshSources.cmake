include_guard(GLOBAL)

# 平台源清单的唯一入口。
# 求值由 Module/TitanProjectEval（C#）完成——它是仓库里唯一一份 vcxproj/vcxitems 求值实现，
# CMake 这里只负责：定位工具、传平台参数、以及配置期/CI 的廉价过期检查（不跑求值器）。

# 去掉 \r 后计算 SHA256，CRLF/LF 检出结果一致
function(titan_file_sha256_lf out_var path)
  file(READ "${path}" _c)
  string(REPLACE "\r" "" _c "${_c}")
  string(SHA256 _h "${_c}")
  set(${out_var} "${_h}" PARENT_SCOPE)
endfunction()

# 只比对逐输入哈希：不跑求值器也能判断入库清单是否过期，并指出是哪个输入变了。
# 需要调用方先设置 TITAN_ROOT 与 TITAN_PLATFORM_DIR。
function(titan_verify_sources_fresh sources_path)
  if(NOT TITAN_ROOT)
    message(FATAL_ERROR "[titan-sources] titan_verify_sources_fresh 需要 TITAN_ROOT")
  endif()
  if(NOT EXISTS "${sources_path}")
    message(FATAL_ERROR "[titan-sources] 缺少入库清单: ${sources_path}")
  endif()
  include("${sources_path}")
  if(NOT DEFINED TITAN_SOURCES_INPUT_HASHES)
    message(FATAL_ERROR "[titan-sources] ${sources_path} 不是当前格式（缺 TITAN_SOURCES_INPUT_HASHES），请重新生成")
  endif()
  set(_stale)
  foreach(_entry IN LISTS TITAN_SOURCES_INPUT_HASHES)
    string(FIND "${_entry}" "=" _eq REVERSE)
    if(_eq LESS 0)
      message(FATAL_ERROR "[titan-sources] ${sources_path} 的 TITAN_SOURCES_INPUT_HASHES 条目非法: ${_entry}")
    endif()
    string(SUBSTRING "${_entry}" 0 ${_eq} _rel)
    math(EXPR _hash_start "${_eq} + 1")
    string(SUBSTRING "${_entry}" ${_hash_start} -1 _want)
    if(NOT EXISTS "${TITAN_ROOT}/${_rel}")
      list(APPEND _stale "${_rel} (缺失)")
      continue()
    endif()
    titan_file_sha256_lf(_got "${TITAN_ROOT}/${_rel}")
    if(NOT _got STREQUAL _want)
      list(APPEND _stale "${_rel}")
    endif()
  endforeach()
  if(_stale)
    message(FATAL_ERROR "[titan-sources] ${sources_path} 已过期，输入变化: ${_stale}\n请在仓库根运行: cmake -P ${TITAN_PLATFORM_DIR}/RefreshSources.cmake 并提交生成文件")
  endif()
endfunction()

# 求值器一律用 dotnet run 调用：源码改了就一定重建，不会悄悄用旧程序集。
function(_titan_require_dotnet)
  if(DEFINED TITAN_DOTNET)
    set(_dotnet "${TITAN_DOTNET}")
  else()
    find_program(_dotnet NAMES dotnet)
  endif()
  if(NOT _dotnet)
    message(FATAL_ERROR "[titan-sources] 未找到 dotnet；源清单求值器 Module/TitanProjectEval 需要 .NET SDK")
  endif()
  set(TITAN_DOTNET "${_dotnet}" PARENT_SCOPE)
endfunction()

# 刷新当前平台的源清单、模块头和托管能力；调用前必须设置 TITAN_PLATFORM* 上下文与 TITAN_ROOT。
# TITAN_CHECK=ON 只做哈希校验，供配置期与 CI 门禁使用。
function(titan_refresh_platform_sources)
  if(NOT TITAN_PLATFORM OR NOT TITAN_ROOT)
    message(FATAL_ERROR "平台上下文或仓库根缺失")
  endif()
  if(NOT DEFINED TITAN_MODULES_FILE)
    set(TITAN_MODULES_FILE "${TITAN_PLATFORM_DIR}/${TITAN_PLATFORM}Modules.cmake")
  endif()
  if(NOT DEFINED TITAN_SOURCES_OUT)
    set(TITAN_SOURCES_OUT "${TITAN_ROOT}/${TITAN_PLATFORM_DIR}/generated/TitanSources.cmake")
  endif()

  if(TITAN_CHECK)
    titan_verify_sources_fresh("${TITAN_SOURCES_OUT}")
    message(STATUS "[titan-sources] ${TITAN_PLATFORM} 输入哈希 OK")
    return()
  endif()

  # 平台模块文件只允许声明排除规则，模块清单的唯一来源是平台工程的 Shared Import。
  unset(TITAN_${TITAN_PLATFORM_PREFIX}_MODULES)
  set(TITAN_${TITAN_PLATFORM_PREFIX}_EXCLUDES)
  include("${TITAN_ROOT}/${TITAN_MODULES_FILE}")
  if(DEFINED TITAN_${TITAN_PLATFORM_PREFIX}_MODULES)
    message(FATAL_ERROR "[titan-sources] ${TITAN_PLATFORM_PROJECT} / CodeGen.vcxitems\n${TITAN_MODULES_FILE}: 不允许维护 TITAN_${TITAN_PLATFORM_PREFIX}_MODULES；请修改平台工程的 Shared Import，胶水单独处理")
  endif()

  _titan_require_dotnet()
  # 求值器自身源码也是指纹输入：改求值器必然让清单过期，逼着一起提交。
  set(_args run --project "${TITAN_ROOT}/Module/TitanProjectEval/TitanProjectEval.csproj" -c Release -v q -- refresh
    --root "${TITAN_ROOT}"
    --project "${TITAN_PLATFORM_PROJECT}"
    --platform "${TITAN_PLATFORM}"
    --platform-dir "${TITAN_PLATFORM_DIR}"
    --prefix "${TITAN_PLATFORM_PREFIX}"
    --modules-file "${TITAN_MODULES_FILE}"
    --sources-out "${TITAN_SOURCES_OUT}"
    --impl-input "Module/TitanProjectEval")
  foreach(_re IN LISTS TITAN_${TITAN_PLATFORM_PREFIX}_EXCLUDES)
    list(APPEND _args --exclude "${_re}")
  endforeach()
  execute_process(COMMAND "${TITAN_DOTNET}" ${_args}
    WORKING_DIRECTORY "${TITAN_ROOT}"
    RESULT_VARIABLE _rc)
  if(NOT _rc EQUAL 0)
    message(FATAL_ERROR "[titan-sources] 求值器失败（${_rc}）：Module/TitanProjectEval refresh ${TITAN_PLATFORM}")
  endif()
endfunction()
