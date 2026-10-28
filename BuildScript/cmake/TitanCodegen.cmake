include_guard(GLOBAL)

# 把 '#include "X:/.../titanengine/<anchor>/..."' 或 POSIX 绝对路径改写为相对仓库根的 include
# 可选参数 titan_root：优先按仓库根前缀剥离（大小写不敏感），避免根路径自身含 /3rd/ 等 anchor 段时截断错位
function(titan_normalize_include_paths content_var)
  set(_content "${${content_var}}")
  set(_root_prefix "")
  if(ARGC GREATER 1 AND NOT "${ARGV1}" STREQUAL "")
    string(REPLACE "\\" "/" _root_prefix "${ARGV1}/")
    string(REGEX REPLACE "/+$" "/" _root_prefix "${_root_prefix}")
    string(TOLOWER "${_root_prefix}" _root_prefix)
    string(LENGTH "${_root_prefix}" _root_len)
  endif()
  string(REGEX MATCHALL "#include \"([A-Za-z]:|/)[^\"\r\n]*\"" _incs "${_content}")
  if(_incs)
    list(REMOVE_DUPLICATES _incs)
  endif()
  foreach(_inc IN LISTS _incs)
    string(REGEX REPLACE "^#include \"(.*)\"$" "\\1" _path "${_inc}")
    string(REPLACE "\\" "/" _path "${_path}")
    if(NOT _root_prefix STREQUAL "")
      string(TOLOWER "${_path}" _lower)
      string(FIND "${_lower}" "${_root_prefix}" _pos)
      if(_pos EQUAL 0)
        string(SUBSTRING "${_path}" ${_root_len} -1 _rel)
        string(REPLACE "${_inc}" "#include \"${_rel}\"" _content "${_content}")
        continue()
      endif()
    endif()
    set(_best -1)
    foreach(_anchor "/Core.Mac/" "/Core.Android/" "/Core.Window/" "/NativeCode/" "/codegen/" "/3rd/")
      string(FIND "${_path}" "${_anchor}" _pos)
      if(_pos GREATER_EQUAL 0 AND (_best EQUAL -1 OR _pos LESS _best))
        set(_best ${_pos})
      endif()
    endforeach()
    if(_best EQUAL -1)
      message(FATAL_ERROR "titan_normalize_include_paths: no known anchor in ${_inc}")
    endif()
    math(EXPR _start "${_best} + 1")
    string(SUBSTRING "${_path}" ${_start} -1 _rel)
    string(REPLACE "${_inc}" "#include \"${_rel}\"" _content "${_content}")
  endforeach()
  set(${content_var} "${_content}" PARENT_SCOPE)
endfunction()

function(titan_normalize_file in_file out_file titan_root)
  file(READ "${in_file}" _content)
  titan_normalize_include_paths(_content "${titan_root}")
  set(_old "")
  if(EXISTS "${out_file}")
    file(READ "${out_file}" _old)
  endif()
  if(NOT "${_old}" STREQUAL "${_content}")
    file(WRITE "${out_file}" "${_content}")
  endif()
endfunction()

# 计算所有会影响当前平台 C++/C# 胶水的源码输入指纹；输出列表同时用于 CMake 自动重配置。
function(titan_codegen_inputs_fingerprint out_hash out_files titan_root)
  if(NOT TITAN_PLATFORM OR NOT TITAN_PLATFORM_PREFIX OR NOT TITAN_PLATFORM_DIR)
    message(FATAL_ERROR "[titan-codegen] 先调用 titan_platform_context")
  endif()
  set(_patterns
    "${titan_root}/${TITAN_PLATFORM_DIR}/*.h" "${titan_root}/${TITAN_PLATFORM_DIR}/*.hpp" "${titan_root}/${TITAN_PLATFORM_DIR}/*.hxx"
    "${titan_root}/${TITAN_PLATFORM_DIR}/*.inl" "${titan_root}/${TITAN_PLATFORM_DIR}/*.ipp" "${titan_root}/${TITAN_PLATFORM_DIR}/*.inc"
    "${titan_root}/Module/CppWeavingTools/*.cs" "${titan_root}/Module/CppWeavingTools/*.csproj"
    "${titan_root}/Module/CSharpCodeTools/*.cs" "${titan_root}/Module/CSharpCodeTools/*.csproj"
    "${titan_root}/Module/CompilingGenerator/*.cs" "${titan_root}/Module/CompilingGenerator/*.csproj"
    "${titan_root}/Module/TtEngineLints/*.cs" "${titan_root}/Module/TtEngineLints/*.csproj"
    "${titan_root}/CSharpCode/*.cs" "${titan_root}/CSharpCode/*.projitems"
    "${titan_root}/Plugins/*.cs" "${titan_root}/Plugins/*.csproj" "${titan_root}/Plugins/*.projitems"
    "${titan_root}/Plugins/*.props" "${titan_root}/Plugins/*.targets" "${titan_root}/Plugins/*.plugin"
    "${titan_root}/BuildScript/*/*.props" "${titan_root}/BuildScript/*/*.targets"
    "${titan_root}/codegen/Templates/*"
    "${titan_root}/3rd/csharp/*.cs" "${titan_root}/3rd/csharp/*.projitems")
  if(TITAN_${TITAN_PLATFORM_PREFIX}_MODULES)
    foreach(_module IN LISTS TITAN_${TITAN_PLATFORM_PREFIX}_MODULES)
      get_filename_component(_module_dir "${_module}" DIRECTORY)
      foreach(_ext h hpp hxx inl ipp inc vcxitems)
        list(APPEND _patterns "${titan_root}/${_module_dir}/*.${_ext}")
      endforeach()
    endforeach()
  else()
    foreach(_root NativeCode 3rd/native)
      foreach(_ext h hpp hxx inl ipp inc vcxitems)
        list(APPEND _patterns "${titan_root}/${_root}/*.${_ext}")
      endforeach()
    endforeach()
  endif()
  file(GLOB_RECURSE _relative_inputs RELATIVE "${titan_root}" LIST_DIRECTORIES FALSE ${_patterns})
  foreach(_explicit
      Rpc_Engine.txt Directory.Packages.props Directory.Build.props Directory.Build.targets global.json
      Module/${TITAN_PLATFORM_ENGINE}/${TITAN_PLATFORM_ENGINE}.csproj
      ${TITAN_PLATFORM_PROJECT}
      ${TITAN_PLATFORM_DIR}/generated/Titan${TITAN_PLATFORM}Capabilities.props
      ${TITAN_PLATFORM_DIR}/generated/Titan${TITAN_PLATFORM}Capabilities.cs
      ${TITAN_PLATFORM_DIR}/GenerateCodegen.cmake
      ${TITAN_PLATFORM_DIR}/cmake/Titan${TITAN_PLATFORM}Flags.cmake
      BuildScript/cmake/TitanPlatformContext.cmake
      BuildScript/cmake/TitanProjectMetadata.cmake
      BuildScript/cmake/TitanSourceList.cmake
      BuildScript/cmake/TitanRefreshSources.cmake
      BuildScript/cmake/TitanCodegen.cmake
      BuildScript/cmake/TitanCodegenGen.cmake)
    if(EXISTS "${titan_root}/${_explicit}")
      list(APPEND _relative_inputs "${_explicit}")
    endif()
  endforeach()
  list(FILTER _relative_inputs EXCLUDE REGEX "(^|/)(build[^/]*|bin|obj)/")
  list(REMOVE_DUPLICATES _relative_inputs)
  list(SORT _relative_inputs)
  set(_acc "")
  set(_absolute_inputs)
  foreach(_relative IN LISTS _relative_inputs)
    string(REPLACE "\\" "/" _relative "${_relative}")
    set(_absolute "${titan_root}/${_relative}")
    file(READ "${_absolute}" _content)
    string(REPLACE "\r" "" _content "${_content}")
    string(SHA256 _hash "${_content}")
    string(APPEND _acc "${_relative}=${_hash}\n")
    list(APPEND _absolute_inputs "${_absolute}")
  endforeach()
  string(SHA256 _fingerprint "${_acc}")
  set(${out_hash} "${_fingerprint}" PARENT_SCOPE)
  set(${out_files} "${_absolute_inputs}" PARENT_SCOPE)
endfunction()

# 仅工程模式：校验当前平台 stamp，规范化独立胶水到 out_dir，返回 .cpp 列表
function(titan_prepare_codegen out_var titan_root out_dir)
  if(NOT TITAN_PLATFORM OR NOT TITAN_PLATFORM_DIR)
    message(FATAL_ERROR "[titan-codegen] 先调用 titan_platform_context")
  endif()
  set(_src_root "${titan_root}/codegen/${TITAN_PLATFORM}/NativeBinder")
  if(NOT EXISTS "${_src_root}/.codegen.stamp")
    message(FATAL_ERROR "[titan-codegen] missing ${TITAN_PLATFORM} stamp: ${_src_root}/.codegen.stamp\n请运行 cmake -P ${TITAN_PLATFORM_DIR}/GenerateCodegen.cmake")
  endif()
  file(READ "${_src_root}/.codegen.stamp" _stamp)
  foreach(_key platform configuration fingerprint inputsFingerprint parseFailures)
    string(JSON _${_key} ERROR_VARIABLE _error GET "${_stamp}" ${_key})
    if(_error)
      message(FATAL_ERROR "[titan-codegen] invalid stamp: ${_key}: ${_error}")
    endif()
  endforeach()
  titan_codegen_inputs_fingerprint(_current_inputs_fingerprint _codegen_inputs "${titan_root}")
  if(NOT _platform STREQUAL TITAN_PLATFORM OR NOT _configuration STREQUAL CMAKE_BUILD_TYPE OR NOT _fingerprint STREQUAL TITAN_SOURCES_FINGERPRINT OR NOT _inputsFingerprint STREQUAL _current_inputs_fingerprint OR NOT _parseFailures STREQUAL "0")
    message(FATAL_ERROR "[titan-codegen] stale or diagnostic ${TITAN_PLATFORM} stamp; regenerate for Configuration=${CMAKE_BUILD_TYPE} (inputs=${_inputsFingerprint}, current=${_current_inputs_fingerprint})")
  endif()
  if(NOT EXISTS "${_src_root}/PODStructDefine.h")
    message(FATAL_ERROR "缺少 ${_src_root}：codegen 被 gitignore，请先在仓库根运行: cmake -P ${TITAN_PLATFORM_DIR}/GenerateCodegen.cmake")
  endif()
  # CONFIGURE_DEPENDS：重新生成 codegen 增删胶水时增量构建也会重新 configure
  file(GLOB_RECURSE _glue CONFIGURE_DEPENDS "${_src_root}/*.cpp2cs.cpp")
  set(_result)
  foreach(_f IN LISTS _glue)
    file(RELATIVE_PATH _rel "${_src_root}" "${_f}")
    set(_out "${out_dir}/codegen/${TITAN_PLATFORM}/NativeBinder/${_rel}")
    titan_normalize_file("${_f}" "${_out}" "${titan_root}")
    list(APPEND _result "${_out}")
  endforeach()
  titan_normalize_file("${_src_root}/PODStructDefine.h" "${out_dir}/codegen/${TITAN_PLATFORM}/NativeBinder/PODStructDefine.h" "${titan_root}")
  set_property(DIRECTORY APPEND PROPERTY CMAKE_CONFIGURE_DEPENDS ${_codegen_inputs} ${_glue} "${_src_root}/PODStructDefine.h" "${_src_root}/.codegen.stamp")
  set(${out_var} "${_result}" PARENT_SCOPE)
endfunction()
