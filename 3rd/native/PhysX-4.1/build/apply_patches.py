"""Patch PhysX 4.1 build tooling for VS2022 / modern toolchains and copy the Titan build scripts. Idempotent.

Usage: python apply_patches.py [PhysX repo root]
"""
import os
import shutil
import sys

PHYSX = os.path.abspath(sys.argv[1]) if len(sys.argv) > 1 else r'E:\GitHub\PhysX'
HERE = os.path.dirname(os.path.abspath(__file__))


def patch(path, old, new, marker):
    with open(path, 'r', newline='') as f:
        text = f.read()
    if marker in text:
        print('already patched:', path)
        return
    if '\r\n' in text:
        old = old.replace('\n', '\r\n')
        new = new.replace('\n', '\r\n')
    if old not in text:
        sys.exit('pattern not found in ' + path + ':\n' + old)
    with open(path, 'w', newline='') as f:
        f.write(text.replace(old, new, 1))
    print('patched:', path)


patch(os.path.join(PHYSX, 'externals', 'cmakemodules', 'GetCompilerAndPlatform.cmake'),
      '\tELSEIF (MSVC_VERSION GREATER_EQUAL 1920)',
      '\tELSEIF (MSVC_VERSION GREATER_EQUAL 1930)\n\t\tSET(COMPILER_SUFFIX "vc143")\n\tELSEIF (MSVC_VERSION GREATER_EQUAL 1920)',
      'vc143')

patch(os.path.join(PHYSX, 'physx', 'buildtools', 'cmake_generate_projects.py'),
      "        elif self.compiler == 'xcode':",
      "        elif self.compiler == 'vc17':\n            outString = outString + '-G \\\"Visual Studio 17 2022\\\"'\n        elif self.compiler == 'xcode':",
      "'vc17'")

# Lets mac arm64 and iOS share the ios platform cmake files while writing to different bin dirs.
patch(os.path.join(PHYSX, 'externals', 'cmakemodules', 'GetCompilerAndPlatform.cmake'),
      'FUNCTION (GetPlatformBinName PLATFORM_BIN_NAME LIBPATH_SUFFIX)\n',
      'FUNCTION (GetPlatformBinName PLATFORM_BIN_NAME LIBPATH_SUFFIX)\n'
      '\tIF(PX_OUTPUT_PLATFORM_BIN_NAME)\n'
      '\t\tSET(${PLATFORM_BIN_NAME} ${PX_OUTPUT_PLATFORM_BIN_NAME} PARENT_SCOPE)\n'
      '\t\tRETURN()\n'
      '\tENDIF()\n',
      'PX_OUTPUT_PLATFORM_BIN_NAME')

copies = {
    'vc17win64.xml': 'physx/buildtools/presets/public/vc17win64.xml',
    'build_titan_windows.ps1': 'physx/build_titan_windows.ps1',
    'build_titan_android.ps1': 'physx/build_titan_android.ps1',
    'build_titan_apple.sh': 'physx/build_titan_apple.sh',
    'TitanRelaxWarnings.cmake': 'physx/buildtools/TitanRelaxWarnings.cmake',
}
for src, dst in copies.items():
    s = os.path.join(HERE, src)
    d = os.path.join(PHYSX, *dst.split('/'))
    if not os.path.exists(s):
        continue
    if src.endswith('.sh'):
        # Shell scripts must keep LF line endings to run on macOS.
        with open(s, 'rb') as f:
            data = f.read().replace(b'\r\n', b'\n')
        with open(d, 'wb') as f:
            f.write(data)
    else:
        shutil.copyfile(s, d)
    print('copied:', dst)
