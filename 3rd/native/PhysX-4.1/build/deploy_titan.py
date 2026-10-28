"""Copy PhysX 4.1 build outputs into the engine's 3rd/native/PhysX-4.1/physx/lib.

Usage: python deploy_titan.py <PhysX repo root> [engine 3rd/native/PhysX-4.1 dir]

Rebuild workflow (PhysX 4.1.2 source, https://github.com/NVIDIAGameWorks/PhysX branch 4.1):
  1. python apply_patches.py <PhysX repo root>   (VS2022 support, bin dir override, copies the scripts below)
  2. Windows: physx/build_titan_windows.ps1      (DLL, static CRT /MT; needs VS2022)
     Android: physx/build_titan_android.ps1      (static libs, NDK r21e, c++_static, API 21, arm64-v8a release only)
     macOS/iOS: bash physx/build_titan_apple.sh  (static libs, arm64; run on a Mac with Xcode)
  3. python deploy_titan.py <PhysX repo root>

Only debug/release are deployed (the engine has no checked/profile configs); Android ships release only, which
every Core.Android configuration links. Release static libs are stripped of debug info (symbols kept) to keep
the repository small; debug libs keep full debug info.
"""
import glob
import os
import shutil
import subprocess
import sys

HERE = os.path.dirname(os.path.abspath(__file__))
PHYSX = os.path.abspath(sys.argv[1]) if len(sys.argv) > 1 else r'E:\GitHub\PhysX'
DEST = os.path.abspath(sys.argv[2]) if len(sys.argv) > 2 else os.path.dirname(HERE)
BIN = os.path.join(PHYSX, 'physx', 'bin')
LIB = os.path.join(DEST, 'physx', 'lib')
CONFIGS = ('debug', 'release')


def find_ndk_strip():
    roots = [os.environ.get('ANDROID_NDK_ROOT'), r'E:\Android\Sdk\ndk\21.4.7075529']
    for root in filter(None, roots):
        exe = os.path.join(root, 'toolchains', 'llvm', 'prebuilt', 'windows-x86_64', 'bin', 'llvm-strip.exe')
        if os.path.exists(exe):
            return [exe, '--strip-debug']
    return None


def copy_libs(src_dir, dst_dir, patterns, strip_cmd=None):
    os.makedirs(dst_dir, exist_ok=True)
    count = 0
    for pattern in patterns:
        for src in glob.glob(os.path.join(src_dir, pattern)):
            dst = os.path.join(dst_dir, os.path.basename(src))
            if strip_cmd:
                subprocess.check_call(strip_cmd + ['-o', dst, src])
            else:
                shutil.copyfile(src, dst)
            count += 1
    print('%-60s %d files%s' % (os.path.relpath(dst_dir, DEST), count, ' (stripped)' if strip_cmd else ''))


def deploy(bin_name, dst_rel, patterns, release_strip=None, configs=CONFIGS):
    src_root = os.path.join(BIN, bin_name)
    if not os.path.isdir(src_root):
        print('skip (not built):', bin_name)
        return
    for cfg in configs:
        src = os.path.join(src_root, cfg)
        if os.path.isdir(src):
            copy_libs(src, os.path.join(LIB, dst_rel, cfg), patterns, release_strip if cfg == 'release' else None)


deploy('win.x86_64.vc143.mt', 'windows', ['*.dll', '*.lib'])

ndk_strip = find_ndk_strip()
if not ndk_strip:
    print('warning: NDK llvm-strip not found, android release libs are copied unstripped')
deploy('android.arm64-v8a.fp-soft', os.path.join('android', 'arm64-v8a'), ['*.a'], ndk_strip, configs=('release',))

apple_strip = ['xcrun', 'strip', '-S'] if shutil.which('xcrun') else None
for name, dst in (('mac.arm_64', 'mac'), ('ios.arm_64', 'ios')):
    if os.path.isdir(os.path.join(BIN, name)) and not apple_strip:
        print('warning: not on macOS, %s release libs are copied unstripped' % name)
    deploy(name, dst, ['*.a'], apple_strip)
