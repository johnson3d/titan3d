include_guard(GLOBAL)

# 构建目录布局唯一来源：CMake 平台的中间产物与 MSBuild 一样落在
# <root>/intermediate/cpp/<Project>/<Config>（intermediate 已被 .gitignore 覆盖），
# 避免 CMakeCache/ninja 日志/对象文件混进源码目录。

# 标准构建目录；config 必须显式给出，避免 Debug/Release 共用同一个目录。
function(titan_default_build_dir out_var root project config)
  if(NOT config MATCHES "^(Debug|Release)$")
    message(FATAL_ERROR "titan_default_build_dir: config 必须是 Debug/Release (${config})")
  endif()
  set(${out_var} "${root}/intermediate/cpp/${project}/${config}" PARENT_SCOPE)
endfunction()

# 源码目录内构建会把生成物写进仓库树，且 CMake 自身会与源码树互相追逐；
# 这里在 configure 阶段直接拒绝，并给出标准目录。
function(titan_require_out_of_source_build source_dir root project config)
  get_filename_component(_src "${source_dir}" ABSOLUTE)
  get_filename_component(_bin "${CMAKE_BINARY_DIR}" ABSOLUTE)
  string(FIND "${_bin}" "${_src}/" _inside)
  if(_bin STREQUAL _src OR _inside EQUAL 0)
    titan_default_build_dir(_expected "${root}" "${project}" "${config}")
    message(FATAL_ERROR "[titan-build] 禁止在源码目录内构建：CMAKE_BINARY_DIR=${_bin} 位于 ${_src} 之下；"
      "请改用 ${_expected}")
  endif()
endfunction()
