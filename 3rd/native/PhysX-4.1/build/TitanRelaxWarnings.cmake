# Injected via -DCMAKE_PROJECT_INCLUDE by build_titan_android.ps1 / build_titan_apple.sh.
# PhysX 4.1 hardcodes -Werror (and -Weverything on Apple), which newer clang versions turn into
# hard failures on unknown/new warnings. Compile options are emitted after CMAKE_CXX_FLAGS,
# so these override the -Werror coming from the platform CMakeLists.
if(NOT MSVC)
	add_compile_options(-Wno-error -w)
endif()
