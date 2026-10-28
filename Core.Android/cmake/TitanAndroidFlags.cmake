include_guard(GLOBAL)

# 平台工程生成的配置元数据供编译与胶水共同消费，不维护第二份宏表。
include("${CMAKE_CURRENT_LIST_DIR}/../generated/TitanSources.cmake")
if(NOT TITAN_CONFIGURATION)
  if(CMAKE_BUILD_TYPE)
    set(TITAN_CONFIGURATION "${CMAKE_BUILD_TYPE}")
  else()
    set(TITAN_CONFIGURATION Debug)
  endif()
endif()
if(NOT TITAN_CONFIGURATION MATCHES "^(Debug|Release)$")
  message(FATAL_ERROR "TITAN_CONFIGURATION must be Debug or Release")
endif()
string(TOUPPER "${TITAN_CONFIGURATION}" _cfg)
set(TITAN_ANDROID_DEFINES ${TITAN_ANDROID_DEFINES_${_cfg}})
set(TITAN_ANDROID_INCLUDES ${TITAN_ANDROID_INCLUDES_${_cfg}})
# PhysX 头路径只为 Android 保留在 flags；vcxproj 显示工程不再维护它。
list(APPEND TITAN_ANDROID_INCLUDES
  "3rd/native/PhysX-4.1/physx/include" "3rd/native/PhysX-4.1/PxShared/include")
# MeshSimplify 使用 CGAL 头文件（EPICK 内核）；Android 的 vcxproj 以前没有这两条路径，
# 是因为旧工程根本无法在 arm64 下编译。
list(APPEND TITAN_ANDROID_INCLUDES "3rd/native/CGAL/Include" "3rd/native/boost-1.89.0")
list(REMOVE_DUPLICATES TITAN_ANDROID_INCLUDES)
# CGAL CORE 默认把 BigInt 定义为 boost::multiprecision::mpz_int，但 Android 侧没有 gmp 实体与
# gmp.h（3rd/native/CGAL/auxiliary/gmp 只有 Windows 库），必须切到 header-only 的 cpp_int 后端。
list(APPEND TITAN_ANDROID_DEFINES "CGAL_DISABLE_GMP")
list(REMOVE_DUPLICATES TITAN_ANDROID_DEFINES)
