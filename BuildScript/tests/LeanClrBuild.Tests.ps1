#requires -Version 5.1
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).ProviderPath
$scriptPath = Join-Path $repoRoot 'BuildScript\leanclr\build-leanclr.ps1'
$manifestPath = Join-Path $repoRoot 'BuildScript\common\dependencies.json'
$ignorePath = Join-Path $repoRoot '.gitignore'
$coreWindowProjectPath = Join-Path $repoRoot 'Core.Window\Core.Window.vcxproj'
$leanClrRuntimeHeaderPath = Join-Path $repoRoot 'NativeCode\Bricks\LeanCLR\LeanClrRuntime.h'
$leanClrRuntimeCppPath = Join-Path $repoRoot 'NativeCode\Bricks\LeanCLR\LeanClrRuntime.cpp'
$leanClrRtMetadataPath = Join-Path $repoRoot '3rd\native\LeanCLR\runtime\metadata\rt_metadata.h'
$leanClrAssemblyPath = Join-Path $repoRoot '3rd\native\LeanCLR\runtime\vm\assembly.cpp'
$leanClrDispatcherPath = Join-Path $repoRoot 'CSharpCode\Bricks\AssemblyLoader\LeanClrIntrinsicDispatcher.cs'
$leanClrInvokeHelperPath = Join-Path $repoRoot 'CSharpCode\Bricks\AssemblyLoader\LeanClrInvokeHelper.cs'
$assemblyLoaderProjectPath = Join-Path $repoRoot 'CSharpCode\Bricks\AssemblyLoader\AssemblyLoader.projitems'
$baseHeadPath = Join-Path $repoRoot 'NativeCode\Base\BaseHead.h'

if (-not (Test-Path -LiteralPath $scriptPath)) {
    throw "LeanCLR build script was not found: $scriptPath"
}

$scriptContent = Get-Content -Raw -LiteralPath $scriptPath
foreach ($expectedText in @(
    "ValidateSet('Debug', 'Release', 'All')",
    'CMAKE_MSVC_RUNTIME_LIBRARY',
    'LEANCLR_GC_DEBUG',
    'Visual Studio 17 2022',
    'leanclr.lib'
)) {
    if (-not $scriptContent.Contains($expectedText)) {
        throw "LeanCLR build script is missing required configuration: $expectedText"
    }
}

$manifest = Get-Content -Raw -LiteralPath $manifestPath | ConvertFrom-Json
$leanClr = @($manifest.nativeDependencyGroups | Where-Object { $_.id -eq 'LeanCLR coreclr runtime' })
if ($leanClr.Count -ne 1) {
    throw 'LeanCLR dependency manifest entry must exist exactly once.'
}

foreach ($requiredPath in @($leanClr[0].requiredPaths)) {
    if ($requiredPath -match 'leanclr\.lib$') {
        throw "LeanCLR dependency manifest must not require generated library: $requiredPath"
    }
}

$baseHead = Get-Content -Raw -LiteralPath $baseHeadPath
if (-not $baseHead.Contains('#define HasModule_LeanCLR')) {
    throw 'Core.Window must enable HasModule_LeanCLR so generated native binder exports are compiled.'
}

$coreWindowProjectContent = Get-Content -Raw -LiteralPath $coreWindowProjectPath
foreach ($expectedText in @(
    '<Target Name="EnsureLeanClrStaticLibrary" BeforeTargets="PrepareForBuild"',
    '''$(Platform)'' == ''x64''',
    '''$(Configuration)'' == ''Debug''',
    '''$(Configuration)'' == ''Release''',
    '!Exists(''$(LeanClrLibraryPath)'')',
    'build-leanclr.ps1',
    '-Configuration $(Configuration)'
)) {
    if (-not $coreWindowProjectContent.Contains($expectedText)) {
        throw "Core.Window LeanCLR build target is missing required configuration: $expectedText"
    }
}

$leanClrRuntimeHeader = Get-Content -Raw -LiteralPath $leanClrRuntimeHeaderPath
foreach ($expectedText in @(
    'struct TR_CLASS(SV_LayoutStruct = 8)',
    'FLeanClrIntrinsicCall',
    'typedef void (*FLeanClrIntrinsicDispatcher)(FLeanClrIntrinsicCall* call);',
    'static void SetIntrinsicDispatcher(FLeanClrIntrinsicDispatcher dispatcher);',
    'bool RegisterIntrinsicFunc(const char* signature);',
    'VNameString GetSignature() const;',
    'UINT GetArgumentCount() const;',
    'VNameString GetArgumentType(UINT index) const;',
    'VNameString GetReturnType() const;',
    'UINT GetArgBufferByteSize() const;',
    'UINT GetRetBufferByteSize() const;',
    'bool InvokeWithBuffer(TtLeanClrMethod* method, BYTE* argBuffer, UINT argBufferByteSize, BYTE* retBuffer, UINT retBufferByteSize);',
    'void* GetUserData() const;',
    'void SetUserData(void* userData);',
    'bool GetArgumentInt32(UINT index, INT32& value) const;',
    'bool SetReturnInt32(INT32 value);',
    'std::set<std::string> mIntrinsicSignatures;',
    'using EngineNS::LeanCLR::FLeanClrIntrinsicCall;',
    'bool IsStatic() const;',
    'TtLeanClrObject : public IWeakRefObject',
    'void* mGCHandle = nullptr;',
    'TtLeanClrMethod* FindMethod(const char* assemblyName, const char* fullClassName, const char* methodName);',
    'TtLeanClrObject* CreateObject(const char* assemblyName, const char* fullClassName);',
    'bool InvokeInstanceWithBuffer(TtLeanClrObject* instance, TtLeanClrMethod* method, BYTE* argBuffer, UINT argBufferByteSize, BYTE* retBuffer, UINT retBufferByteSize);',
    'void* GetArgumentBuffer() const;',
    'void* GetReturnBuffer() const;',
    'void* GetTarget() const;',
    'bool SetHostHandle(void* handle);',
    'void* GetHostHandle() const;',
    'bool BindHostObjectClass(const char* assemblyName, const char* fullClassName, const char* fieldName);',
    'static INT32 GetHostHandleOffset();'
)) {
    if (-not $leanClrRuntimeHeader.Contains($expectedText)) {
        throw "LeanCLR intrinsic dispatcher declaration is missing: $expectedText"
    }
}

$leanClrRuntimeCpp = Get-Content -Raw -LiteralPath $leanClrRuntimeCppPath
if ($leanClrRuntimeCpp.Contains('        AddRef();')) {
    throw 'LeanCLR runtime Initialize must not retain an unmatched self-reference.'
}

if ($leanClrRuntimeHeader.Contains('InvokeInt32Int32ToInt32') -or $leanClrRuntimeCpp.Contains('InvokeInt32Int32ToInt32')) {
    throw 'LeanCLR runtime must not retain the signature-specific InvokeInt32Int32ToInt32 test API.'
}

foreach ($expectedText in @(
    'leanclr::RtResultVoid Titan_LeanclrMethodInvoker',
    'FLeanClrIntrinsicCall call(reinterpret_cast<const LeanclrMethodInfo*>(method),',
    'TtLeanClrRuntime::mIntrinsicDispatcher(&call);',
    'leanclr::vm::Intrinsics::register_intrinsic(it->c_str(), nullptr, &Titan_LeanclrMethodInvoker);',
    'return GetMethodArgumentType(Method, index);',
    'return GetMethodReturnType(Method);',
    'return GetMethodArgumentType(mMethod, index);',
    'return GetMethodReturnType(mMethod);',
    'leanclr_invoke_with_buffer(method->Method,',
    'return method == nullptr ? nullptr : method->user_data;',
    'method->user_data = userData;',
    'auto result = leanclr::vm::Assembly::load_by_name(assemblyName);',
    'return SetError("LoadAssembly", static_cast<INT32>(result.unwrap_err()));',
    'leanclr::vm::Method::is_static(method)',
    'LEANCLR_NEWOBJ_INTERNAL(rtKlass',
    'leanclr::vm::GCHandle::new_handle(',
    'leanclr::vm::GCHandle::free_handle(',
    'leanclr::vm::GCHandle::get_target(',
    'leanclr::vm::Class::is_value_type(',
    'leanclr::vm::Method::is_instance(method)',
    '#include "vm/field.h"',
    'leanclr::vm::Field::get_field_offset_includes_object_header_for_all_type(',
    'INT32 boolValue = value ? 1 : 0;'
)) {
    if (-not $leanClrRuntimeCpp.Contains($expectedText)) {
        throw "LeanCLR intrinsic dispatcher implementation is missing: $expectedText"
    }
}

if (-not (Test-Path -LiteralPath $leanClrDispatcherPath)) {
    throw "LeanCLR managed dispatcher was not found: $leanClrDispatcherPath"
}
$leanClrDispatcher = Get-Content -Raw -LiteralPath $leanClrDispatcherPath
foreach ($expectedText in @(
    'public unsafe class TtLeanClrIntrinsicDispatcher : TtModule<TtEngine>',
    'public override async Thread.Async.TtTask<bool> Initialize(TtEngine host)',
    'public override void Cleanup(TtEngine host)',
    'host.Config.LeanClrAssemblies',
    'TtLeanClrRuntime.CreateInstance()',
    'mRuntime.Initialize()',
    'public void Invoke(FLeanClrIntrinsicCall* call)',
    'TtLeanClrRuntime.SetIntrinsicDispatcher(mNativeInvoke);',
    '!mHandlers.ContainsKey(signature.Index)',
    'mRuntime.RegisterIntrinsicFunc(signature.Text)',
    'Dictionary<int, FIntrinsicHandler>',
    'List<FIntrinsicHandler> mHandlersByToken',
    'mHandlersByToken[token - 1]',
    'mHandlersByToken.Add(handler)',
    'call->GetUserData()',
    'call->SetUserData((void*)(nint)token)',
    'public LeanCLR.TtLeanClrIntrinsicDispatcher LeanClrIntrinsicDispatcher { get; }'
)) {
    if (-not $leanClrDispatcher.Contains($expectedText)) {
        throw "LeanCLR managed dispatcher is missing: $expectedText"
    }
}
foreach ($forbiddenPattern in @(
    'static unsafe class TtLeanClrIntrinsicDispatcher',
    '\bsTokensBySignature\b',
    '\bsHandlers\w*\b',
    '\bsRuntime\b'
)) {
    if ($leanClrDispatcher -cmatch $forbiddenPattern) {
        throw "LeanCLR managed dispatcher must not contain static state: $forbiddenPattern"
    }
}

$engineSourcePath = Join-Path $repoRoot 'CSharpCode\Base\Engine.cs'
$engineSource = Get-Content -Raw -LiteralPath $engineSourcePath
if (-not $engineSource.Contains('public List<RName> LeanClrAssemblies { get; set; }')) {
    throw 'TtEngineConfig LeanCLR assembly configuration is missing: LeanClrAssemblies'
}
# VM 程序集改为扫描 content\LeanCLR 自动注册，LeanClrAssemblies 只作补充；默认列 BCL 会让未用热更的工程也启动 VM
if ($engineSource.Contains('RName.GetRName("LeanCLR/mscorlib.dll")')) {
    throw 'TtEngineConfig.LeanClrAssemblies must default to empty; BCL is picked up by the content\LeanCLR scan.'
}
if (-not $leanClrDispatcher.Contains('TtLeanClrAssemblyScanner.Collect(')) {
    throw 'LeanCLR dispatcher must collect VM assemblies by scanning content\LeanCLR.'
}
if (-not (Get-Content -Raw -LiteralPath $assemblyLoaderProjectPath).Contains('LeanClrAssemblyScanner.cs')) {
    throw 'LeanClrAssemblyScanner.cs must be included in AssemblyLoader.projitems.'
}

foreach ($bclName in @('mscorlib.dll', 'System.dll', 'System.Core.dll', 'netstandard.dll')) {
    $bclPath = Join-Path $repoRoot "content\LeanCLR\$bclName"
    if (-not (Test-Path -LiteralPath $bclPath)) {
        throw "LeanCLR default BCL assembly was not found: $bclPath"
    }
}

$leanClrAssembly = Get-Content -Raw -LiteralPath $leanClrAssemblyPath
if ($leanClrAssembly.Contains('mod->get_reference_assemblies(ref_assemblies)')) {
    throw 'LeanCLR must resolve AssemblyRef entries lazily instead of loading the full BCL dependency graph.'
}

$leanClrRtMetadata = Get-Content -Raw -LiteralPath $leanClrRtMetadataPath
if (-not $leanClrRtMetadata.Contains('void* user_data;')) {
    throw 'LeanCLR RtMethodInfo must expose the Titan intrinsic user-data slot.'
}
if ($leanClrRtMetadata -notmatch '(?s)#if LEANCLR_PGO_PROFILE\s+uint32_t profile_slot;\s+#endif\s+void\* user_data;\s+\};') {
    throw 'LeanCLR RtMethodInfo user-data slot must remain the final field after optional upstream fields.'
}

$arrayClassPath = Join-Path $repoRoot '3rd\native\LeanCLR\runtime\vm\array_class.cpp'
$arrayClass = Get-Content -Raw -LiteralPath $arrayClassPath
if (-not $arrayClass.Contains('new_method->user_data = nullptr;')) {
    throw 'Inflated RtMethodInfo copies must clear the intrinsic user-data slot.'
}

$leanClrDeployTargetsPath = Join-Path $repoRoot 'BuildScript\leanclr\LeanClrDeploy.targets'
if (-not (Test-Path -LiteralPath $leanClrDeployTargetsPath)) {
    throw "LeanCLR deploy targets was not found: $leanClrDeployTargetsPath"
}
$leanClrDeployTargets = Get-Content -Raw -LiteralPath $leanClrDeployTargetsPath
# 插件 PostBuild 会把输出目录 pdb 改名为 tpdb 并删掉 obj 里的 pdb，部署必须赶在 PostBuildEvent 之前
foreach ($expectedText in @(
    'AfterTargets="CopyFilesToOutputDirectory"',
    '''$(LeanClrDeployToContent)'' == ''true''',
    'content\LeanCLR\',
    '@(IntermediateAssembly'
)) {
    if (-not $leanClrDeployTargets.Contains($expectedText)) {
        throw "LeanClrDeploy.targets is missing: $expectedText"
    }
}

$leanClrStubTargets = Get-Content -Raw -LiteralPath (Join-Path $repoRoot 'BuildScript\leanclr\LeanClrStub.targets')
foreach ($expectedText in @(
    '<LeanClrContentSubDir Condition="''$(LeanClrContentSubDir)'' == ''''">$(TtPluginName)</LeanClrContentSubDir>',
    'LeanClrDeploy.targets'
)) {
    if (-not $leanClrStubTargets.Contains($expectedText)) {
        throw "LeanClrStub.targets is missing: $expectedText"
    }
}

$engineStubProject = Get-Content -Raw -LiteralPath (Join-Path $repoRoot 'Module\LeanClrEngineStub\LeanClrEngineStub.csproj')
if (-not $engineStubProject.Contains('BuildScript\leanclr\LeanClrDeploy.targets')) {
    throw 'LeanClrEngineStub.csproj must deploy Titan.LeanClr.EngineStub.dll to content\LeanCLR.'
}

$testStubProject = Get-Content -Raw -LiteralPath (Join-Path $repoRoot 'Tests\Titan.Tests.LeanCLR.HostGame.Stub\Titan.Tests.LeanCLR.HostGame.Stub.csproj')
if (-not $testStubProject.Contains('<LeanClrDeployToContent>false</LeanClrDeployToContent>')) {
    throw 'Test stub projects must not deploy into content\LeanCLR.'
}

$scriptProjectPath = Join-Path $repoRoot 'Tests\Titan.Tests.LeanCLR.Script\Titan.Tests.LeanCLR.Script.csproj'
$scriptProject = Get-Content -Raw -LiteralPath $scriptProjectPath
if (-not $scriptProject.Contains('<TargetFramework>netstandard2.0</TargetFramework>')) {
    throw 'LeanCLR hot-patch script project must target netstandard2.0 so it binds to the netstandard facade.'
}

$probeProjectPath = Join-Path $repoRoot 'Tests\Titan.Tests.LeanCLR\Titan.Tests.LeanCLR.csproj'
$probeProject = Get-Content -Raw -LiteralPath $probeProjectPath
$probeProgramPath = Join-Path $repoRoot 'Tests\Titan.Tests.LeanCLR\Program.cs'
$probeProgram = Get-Content -Raw -LiteralPath $probeProgramPath
foreach ($expectedText in @(
    '<AllowUnsafeBlocks>true</AllowUnsafeBlocks>',
    'Titan.Tests.LeanCLR.Script.csproj'
)) {
    if (-not $probeProject.Contains($expectedText)) {
        throw "LeanCLR probe project is missing: $expectedText"
    }
}
foreach ($expectedText in @(
    'RegisterAssembly(runtime, "netstandard"',
    'runtime.LoadAssembly("Titan.Tests.LeanCLR.Script")',
    'runtime.FindStaticMethod("Titan.Tests.LeanCLR.Script", "Titan.HotPatch.HotPatchProbe", "Add")',
    'dispatcher.Register(signature, replacementHandler);',
    'replacementHandlerCalls != 1',
    'TtLeanClrInvokeHelper.Invoke<int>(runtime, addMethod, 20, 22)',
    'result != -1',
    'runtime.CreateObject(',
    'runtime.FindMethod(',
    'TtLeanClrInvokeHelper.InvokeInstance<int>(',
    'instanceHandlerCalls'
)) {
    if (-not $probeProgram.Contains($expectedText)) {
        throw "LeanCLR end-to-end probe is missing: $expectedText"
    }
}

if (-not (Test-Path -LiteralPath $leanClrInvokeHelperPath)) {
    throw "LeanCLR managed invoke helper was not found: $leanClrInvokeHelperPath"
}
$leanClrInvokeHelper = Get-Content -Raw -LiteralPath $leanClrInvokeHelperPath
foreach ($expectedText in @(
    'public static unsafe class TtLeanClrInvokeHelper',
    'params object[] arguments',
    'where TResult : unmanaged',
    'method.GetArgBufferByteSize()',
    'runtime.InvokeWithBuffer(',
    'InvokeInstance<TResult>(',
    'runtime.InvokeInstanceWithBuffer('
)) {
    if (-not $leanClrInvokeHelper.Contains($expectedText)) {
        throw "LeanCLR managed invoke helper is missing: $expectedText"
    }
}

$assemblyLoaderProject = Get-Content -Raw -LiteralPath $assemblyLoaderProjectPath
foreach ($sourceFile in @('LeanClrIntrinsicDispatcher.cs', 'LeanClrInvokeHelper.cs')) {
    if (-not $assemblyLoaderProject.Contains($sourceFile)) {
        throw "LeanCLR managed source is not included in AssemblyLoader.projitems: $sourceFile"
    }
}

$nativeBinderProjectPath = Join-Path $repoRoot 'codegen\Windows\NativeBinder\CodeGen.vcxitems'
$nativeBinderProject = Get-Content -Raw -LiteralPath $nativeBinderProjectPath
foreach ($expectedEntry in @(
    'FLeanClrIntrinsicCall_3640661289.cpp2cs.cpp',
    'TtLeanClrMethod_3640661289.cpp2cs.cpp',
    'TtLeanClrRuntime_3640661289.cpp2cs.cpp',
    'TtLeanClrObject_3640661289.cpp2cs.cpp'
)) {
    if (-not $nativeBinderProject.Contains($expectedEntry)) {
        throw "LeanCLR native binder entry missing from CodeGen.vcxitems: $expectedEntry"
    }
}
# CodeGen.vcxitems is a generated file (CppWeavingTools overwrites it on every run),
# so per-entry Condition attributes cannot be persisted. Instead, verify that the
# generated binder .cpp files are guarded by #if defined(HasModule_LeanCLR) so they
# compile only in Core.Window (which defines that macro) and are dead code on Android/iOS.
$leanClrBinderDir = Join-Path $repoRoot 'codegen\Windows\NativeBinder\EngineNS\LeanCLR'
foreach ($binderCpp in @(
    'FLeanClrIntrinsicCall_3640661289.cpp2cs.cpp',
    'TtLeanClrMethod_3640661289.cpp2cs.cpp',
    'TtLeanClrRuntime_3640661289.cpp2cs.cpp',
    'TtLeanClrObject_3640661289.cpp2cs.cpp'
)) {
    $binderPath = Join-Path $leanClrBinderDir $binderCpp
    if (-not (Test-Path -LiteralPath $binderPath)) {
        throw "LeanCLR binder source was not generated: $binderPath"
    }
    $binderContent = Get-Content -Raw -LiteralPath $binderPath
    if (-not $binderContent.Contains('#if defined(HasModule_LeanCLR)')) {
        throw "LeanCLR binder source $binderCpp must be guarded by #if defined(HasModule_LeanCLR)."
    }
}

$compilingGeneratorItemsPath = Join-Path $repoRoot 'CSharpCode\CompilingGenerator\CompilingGenerator.projitems'
$compilingGeneratorItems = Get-Content -Raw -LiteralPath $compilingGeneratorItemsPath
if ($compilingGeneratorItems.Contains('LeanClrGenerator')) {
    throw 'LeanCLR proxy generator must stay an opt-in analyzer and not be merged into CompilingGenerator.projitems.'
}

$leanClrPluginTargetsPath = Join-Path $repoRoot 'BuildScript\leanclr\LeanClrPlugin.targets'
$leanClrPluginTargets = Get-Content -Raw -LiteralPath $leanClrPluginTargetsPath
# Plugins\Directory.Build.props 把 obj 移到 intermediate\，默认通配不再排除工程内 obj\；不移除会把落盘的 .g.cs 重复编译
if (-not $leanClrPluginTargets.Contains('<Compile Remove="$(CompilerGeneratedFilesOutputPath)\**" />')) {
    throw 'LeanClrPlugin.targets must exclude emitted generator files from Compile items.'
}

$survivorProjectPath = Join-Path $repoRoot 'Plugins\Game\Survivor\Survivor.All\Survivor.All.csproj'
$survivorProject = Get-Content -Raw -LiteralPath $survivorProjectPath
if (-not $survivorProject.Contains('BuildScript\leanclr\LeanClrPlugin.targets')) {
    throw 'Survivor.All must import LeanClrPlugin.targets.'
}

$survivorStubProjectPath = Join-Path $repoRoot 'Plugins\Game\Survivor\Survivor.LeanClrStub\Survivor.LeanClrStub.csproj'
$survivorStubProject = Get-Content -Raw -LiteralPath $survivorStubProjectPath
# Plugins\Directory.Build.targets 的 PreBuild 会清空 TtOutputBasePath，桩工程必须与插件本体分开目录
foreach ($expectedText in @(
    'BuildScript\leanclr\LeanClrStub.targets',
    '<TtOutputBasePath>$(TtTitanRoot)binaries\LeanClrStub\',
    '<OutDir>$(TtOutputBasePath)</OutDir>'
)) {
    if (-not $survivorStubProject.Contains($expectedText)) {
        throw "Survivor LeanCLR stub project is missing: $expectedText"
    }
}

$ignoreContent = Get-Content -Raw -LiteralPath $ignorePath
if (-not $ignoreContent.Contains('/3rd/native/LeanCLR/lib/')) {
    throw 'Generated LeanCLR library directory must be ignored by Git.'
}

# content\LeanCLR 只有 BCL 入库，其余 VM 程序集都是构建产物
foreach ($ignoredPath in @(
    'content/LeanCLR/Titan.LeanClr.EngineStub.dll',
    'content/LeanCLR/Survivor/Survivor.All.LeanClrStub.dll',
    'content/LeanCLR/Survivor/Survivor.All.LeanClrStub.pdb',
    'binaries/LeanClrStub/Survivor/Survivor.All.LeanClrStub.dll'
)) {
    & git -C $repoRoot check-ignore -q --no-index $ignoredPath
    if ($LASTEXITCODE -ne 0) {
        throw "LeanCLR build output must be ignored by Git: $ignoredPath"
    }
}
foreach ($bclName in @('mscorlib.dll', 'System.dll', 'System.Core.dll', 'netstandard.dll')) {
    & git -C $repoRoot check-ignore -q --no-index "content/LeanCLR/$bclName"
    if ($LASTEXITCODE -eq 0) {
        throw "LeanCLR BCL must stay tracked by Git: content/LeanCLR/$bclName"
    }
}
$global:LASTEXITCODE = 0

Write-Host 'LeanCLR source-build configuration test passed.'
