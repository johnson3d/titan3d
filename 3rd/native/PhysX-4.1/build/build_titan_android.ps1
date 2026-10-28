# Build PhysX 4.1 for Android (static libs) with NDK r21e, libc++ (c++_static). The engine only ships arm64-v8a release.
# Output: physx/bin/android.<abi>.fp-soft/<config>/
param(
    [string]$Ndk = $(if ($env:ANDROID_NDK_ROOT) { $env:ANDROID_NDK_ROOT } else { 'E:/Android/Sdk/ndk/21.4.7075529' }),
    [string[]]$Abis = @('arm64-v8a'),
    [string[]]$Configs = @('release'),
    [string]$ApiLevel = '21'
)
$ErrorActionPreference = 'Stop'

$Ndk = $Ndk -replace '\\', '/'
if (-not (Test-Path "$Ndk/build/cmake/android.toolchain.cmake")) { throw "Android NDK not found: $Ndk" }

# Prefer Ninja (PATH, then the copy bundled with VS2022); fall back to the NDK's make.
$Ninja = Get-Command ninja -ErrorAction SilentlyContinue | Select-Object -ExpandProperty Source -First 1
if (-not $Ninja) {
    $vswhere = "${env:ProgramFiles(x86)}/Microsoft Visual Studio/Installer/vswhere.exe"
    if (Test-Path $vswhere) {
        foreach ($vs in (& $vswhere -all -products * -property installationPath)) {
            $candidate = Join-Path $vs 'Common7/IDE/CommonExtensions/Microsoft/CMake/Ninja/ninja.exe'
            if (Test-Path $candidate) { $Ninja = $candidate; break }
        }
    }
}
if ($Ninja) {
    $Generator = @('-G', 'Ninja', "-DCMAKE_MAKE_PROGRAM=$($Ninja -replace '\\', '/')")
} else {
    $Generator = @('-G', 'MinGW Makefiles', "-DCMAKE_MAKE_PROGRAM=$Ndk/prebuilt/windows-x86_64/bin/make.exe")
}

$PhysXRoot = ($PSScriptRoot -replace '\\', '/')
$Externals = (Resolve-Path "$PSScriptRoot/../externals").Path -replace '\\', '/'
$env:PHYSX_ROOT_DIR = $PhysXRoot
$env:PM_CMakeModules_PATH = "$Externals/cmakemodules"
$env:PM_PxShared_PATH = (Resolve-Path "$PSScriptRoot/../pxshared").Path -replace '\\', '/'
$env:PM_PATHS = "$Externals/cmakemodules;$Externals/targa"

$Jobs = [string]$env:NUMBER_OF_PROCESSORS
if (-not $Jobs) { $Jobs = '8' }

foreach ($abi in $Abis) {
    foreach ($cfg in $Configs) {
        Write-Host "==== Building android $abi $cfg ====" -ForegroundColor Cyan
        $BuildDir = Join-Path $PSScriptRoot "compiler/android-$abi-$cfg"
        if (Test-Path $BuildDir) { Remove-Item -Recurse -Force $BuildDir }
        New-Item -ItemType Directory -Force $BuildDir | Out-Null

        & cmake -S "$PhysXRoot/compiler/public" -B $BuildDir `
            @Generator `
            --no-warn-unused-cli `
            "-DCMAKE_POLICY_VERSION_MINIMUM=3.5" `
            "-DCMAKE_PROJECT_INCLUDE=$PhysXRoot/buildtools/TitanRelaxWarnings.cmake" `
            "-DCMAKE_TOOLCHAIN_FILE=$Ndk/build/cmake/android.toolchain.cmake" `
            "-DANDROID_NDK=$Ndk" `
            "-DANDROID_ABI=$abi" `
            "-DANDROID_PLATFORM=android-$ApiLevel" `
            -DANDROID_STL=c++_static `
            -DANDROID_ARM_NEON=TRUE `
            -DTARGET_BUILD_PLATFORM=android -DPX_OUTPUT_ARCH=arm `
            "-DCMAKE_BUILD_TYPE=$cfg" `
            "-DCMAKE_PREFIX_PATH=$env:PM_PATHS" `
            "-DPHYSX_ROOT_DIR=$PhysXRoot" `
            "-DPX_OUTPUT_LIB_DIR=$PhysXRoot" `
            "-DPX_OUTPUT_BIN_DIR=$PhysXRoot" `
            -DPX_BUILDSNIPPETS=FALSE -DPX_BUILDPUBLICSAMPLES=FALSE `
            -DPX_GENERATE_STATIC_LIBRARIES=TRUE `
            "-DCMAKE_INSTALL_PREFIX=$PhysXRoot/install/android-$ApiLevel/PhysX"
        if ($LASTEXITCODE -ne 0) { throw "CMake generate failed for $abi $cfg ($LASTEXITCODE)" }

        & cmake --build $BuildDir --parallel $Jobs
        if ($LASTEXITCODE -ne 0) { throw "Build failed for $abi $cfg ($LASTEXITCODE)" }
    }
    # include/PxConfig.h is regenerated per platform; keep the matching copy next to the binaries.
    Copy-Item "$PhysXRoot/include/PxConfig.h" "$PhysXRoot/bin/android.$abi.fp-soft/PxConfig.h" -Force
}

Write-Host "Done. Output: $PhysXRoot/bin/android.<abi>.fp-soft" -ForegroundColor Green
