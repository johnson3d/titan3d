Embree 4.4.1 Windows x64 runtime subset for Titan.

Kept files:
- include/embree4: public Embree C/C++ headers used by Core.Window.
- lib/embree4.lib and lib/tbb.lib: import libraries used by Core.Window.
- bin/embree4.dll, bin/tbb12.dll, bin/tbbmalloc.dll: runtime DLLs copied by eng/build.ps1.
- LICENSE.txt and third-party-programs-TBB.txt: license notices for redistributed binaries.

Removed from the upstream package:
- sample/test executables and sample models.
- CMake package files.
- docs and source/example folders not used by the Windows desktop build.
