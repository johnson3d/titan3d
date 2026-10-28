# Build PhysX 4.1 for Windows x64 with Visual Studio 2022 (DLL, static CRT /MT, /MTd for debug).
# Output: physx/bin/win.x86_64.vc143.mt/<config>/
param(
    [string[]]$Configs = @('debug', 'checked', 'profile', 'release'),
    [switch]$SkipGenerate
)
$ErrorActionPreference = 'Stop'

$PhysXRoot = ($PSScriptRoot -replace '\\', '/')
$Externals = (Resolve-Path "$PSScriptRoot/../externals").Path -replace '\\', '/'
$env:PHYSX_ROOT_DIR = $PhysXRoot
$env:PM_CMakeModules_PATH = "$Externals/cmakemodules"
$env:PM_PxShared_PATH = (Resolve-Path "$PSScriptRoot/../pxshared").Path -replace '\\', '/'
$env:PM_PATHS = "$Externals/cmakemodules;$Externals/targa"

$BuildDir = Join-Path $PSScriptRoot 'compiler/vc17win64'

if (-not $SkipGenerate) {
    if (Test-Path $BuildDir) { Remove-Item -Recurse -Force $BuildDir }
    New-Item -ItemType Directory -Force $BuildDir | Out-Null
    & cmake -S "$PhysXRoot/compiler/public" -B $BuildDir `
        -G 'Visual Studio 17 2022' -A x64 `
        --no-warn-unused-cli `
        "-DCMAKE_POLICY_VERSION_MINIMUM=3.5" `
        -DTARGET_BUILD_PLATFORM=windows -DPX_OUTPUT_ARCH=x86 `
        "-DCMAKE_PREFIX_PATH=$env:PM_PATHS" `
        "-DPHYSX_ROOT_DIR=$PhysXRoot" `
        "-DPX_OUTPUT_LIB_DIR=$PhysXRoot" `
        "-DPX_OUTPUT_BIN_DIR=$PhysXRoot" `
        -DPX_BUILDSNIPPETS=FALSE -DPX_BUILDPUBLICSAMPLES=FALSE `
        -DPX_GENERATE_STATIC_LIBRARIES=FALSE `
        -DNV_USE_STATIC_WINCRT=TRUE -DNV_USE_DEBUG_WINCRT=TRUE `
        -DPX_FLOAT_POINT_PRECISE_MATH=FALSE `
        "-DCMAKE_INSTALL_PREFIX=$PhysXRoot/install/vc17win64/PhysX"
    if ($LASTEXITCODE -ne 0) { throw "CMake generate failed ($LASTEXITCODE)" }
}

foreach ($cfg in $Configs) {
    Write-Host "==== Building windows $cfg ====" -ForegroundColor Cyan
    & cmake --build $BuildDir --config $cfg --parallel -- /nologo /v:minimal
    if ($LASTEXITCODE -ne 0) { throw "Build $cfg failed ($LASTEXITCODE)" }
}

# include/PxConfig.h is regenerated per platform (PX_PHYSX_STATIC_LIB or not); keep the matching copy next to the binaries.
$OutDir = "$PhysXRoot/bin/win.x86_64.vc143.mt"
Copy-Item "$PhysXRoot/include/PxConfig.h" "$OutDir/PxConfig.h" -Force

Write-Host "Done. Output: $OutDir" -ForegroundColor Green
