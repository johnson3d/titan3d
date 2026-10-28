using System.Runtime.InteropServices;
using EngineNS;
using EngineNS.LeanCLR;
using EngineNS.Support;

#if DEBUG
const string configuration = "Debug";
#else
const string configuration = "Release";
#endif

var repoRoot = FindRepoRoot();
var nativeDirectory = Environment.GetEnvironmentVariable("TITAN_TEST_NATIVE_DIRECTORY") ??
    Path.Combine(repoRoot, "binaries", configuration.ToLowerInvariant());
var nativeModule = Path.Combine(nativeDirectory, "Core.Window.dll");
if (!File.Exists(nativeModule))
    throw new FileNotFoundException($"Build the {configuration} Core.Window native module before running this probe.", nativeModule);
if (!NativeMethods.SetDllDirectory(nativeDirectory))
    throw new InvalidOperationException($"Failed to add the native module directory: {nativeDirectory}");

RunNativeMemoryProbe();
var loadedNativeModule = System.Diagnostics.Process.GetCurrentProcess().Modules
    .Cast<System.Diagnostics.ProcessModule>().Single(module => module.ModuleName.Equals("Core.Window.dll", StringComparison.OrdinalIgnoreCase));
if (!loadedNativeModule.FileName.Equals(Path.GetFullPath(nativeModule), StringComparison.OrdinalIgnoreCase))
    throw new InvalidOperationException($"实际加载的 native DLL 与验证目标不同：{loadedNativeModule.FileName}");
Console.WriteLine($"Native memory probes use: {loadedNativeModule.FileName}");
RunNativeMemoryLockProbe();

var bclDirectory = Path.Combine(repoRoot, "content", "LeanCLR");
var scriptDirectory = Path.Combine(repoRoot, "Tests", "Titan.Tests.LeanCLR.Script", "bin", configuration, "netstandard2.0");
var scriptPath = Path.Combine(scriptDirectory, "Titan.Tests.LeanCLR.Script.dll");
if (!File.Exists(scriptPath))
    throw new FileNotFoundException("Build Titan.Tests.LeanCLR.Script before running this probe.", scriptPath);

var runtime = TtLeanClrRuntime.CreateInstance();
if (!runtime.IsValidPointer)
    throw new InvalidOperationException("NativeBinder did not create TtLeanClrRuntime.");

var patchProbe = new MacrossPatchProbe(repoRoot, configuration, Path.Combine(scriptDirectory, "Titan.Tests.LeanCLR.HostGame.LeanClrStub.dll"));
var vmAssemblies = patchProbe.VmAssemblies;
RegisterAssembly(runtime, "mscorlib", Path.Combine(bclDirectory, "mscorlib.dll"), vmAssemblies);
RegisterAssembly(runtime, "System", Path.Combine(bclDirectory, "System.dll"), vmAssemblies);
RegisterAssembly(runtime, "System.Core", Path.Combine(bclDirectory, "System.Core.dll"), vmAssemblies);
RegisterAssembly(runtime, "netstandard", Path.Combine(bclDirectory, "netstandard.dll"), vmAssemblies);
RegisterAssembly(runtime, "Titan.Tests.LeanCLR.Script", scriptPath, vmAssemblies);
// 宿主对象桩：引擎桩提供 HostHandle 字段，HostGame 桩由生成器产出。native 只允许在 Initialize 前注册
RegisterAssembly(runtime, TtLeanClrHostBinding.EngineStubAssemblyName, Path.Combine(scriptDirectory, "Titan.LeanClr.EngineStub.dll"), vmAssemblies);
RegisterAssembly(runtime, "Titan.Tests.LeanCLR.HostGame.LeanClrStub", Path.Combine(scriptDirectory, "Titan.Tests.LeanCLR.HostGame.LeanClrStub.dll"), vmAssemblies);

// Macross 补丁 Stage A 只能在 Initialize 前注册补丁程序集
MacrossPatchProbe.RunPureTests();
MacrossPatchStubProbe.Run();
patchProbe.Stage((name, path) => { RegisterAssembly(runtime, name, path, null); return true; });

if (!runtime.Initialize())
{
    unsafe
    {
        CoreSDK.IUnknown_Release(runtime.NativePointer.ToPointer());
    }
    throw new InvalidOperationException("LeanCLR runtime initialization failed.");
}
unsafe
{
    RunDispatcherCleanupLifetimeProbe(runtime);
    var dispatcher = new TtLeanClrIntrinsicDispatcher();
    if (!dispatcher.AttachRuntime(runtime))
        throw new InvalidOperationException("LeanCLR dispatcher failed to attach the runtime.");
    if (!dispatcher.RegisterAssembly(typeof(Titan.HostGame.TtProbeActor).Assembly))
        throw new InvalidOperationException("LeanCLR dispatcher failed to register the HostGame proxies.");

    const string intrinsicSignature = "Titan.HotPatch.HotPatchProbe::IntrinsicAdd(System.Int32,System.Int32)";
    const string instanceIntrinsicSignature = "Titan.HotPatch.HotPatchInstanceProbe::IntrinsicAdd(System.Int32,System.Int32)";
    var signature = VNameString.FromString(intrinsicSignature);
    var instanceSignature = VNameString.FromString(instanceIntrinsicSignature);
    var replacementHandlerCalls = 0;
    TtLeanClrIntrinsicDispatcher.FIntrinsicHandler replacementHandler = call =>
    {
        ++replacementHandlerCalls;
        return call->SetReturnInt32(-1);
    };
    var instanceHandlerCalls = 0;
    TtLeanClrIntrinsicDispatcher.FIntrinsicHandler instanceHandler = call =>
    {
        ++instanceHandlerCalls;
        var left = 0;
        var right = 0;
        return call->GetArgumentInt32(0, ref left) &&
               call->GetArgumentInt32(1, ref right) &&
               left == 20 && right == 22 &&
               call->SetReturnInt32(-42);
    };

    TtLeanClrMethod addMethod = default;
    TtLeanClrMethod instanceAddMethod = default;
    TtLeanClrMethod hitMethod = default;
    TtLeanClrMethod selfIsSameMethod = default;
    TtLeanClrMethod scaleMethod = default;
    TtLeanClrMethod stringRoundTripMethod = default;
    TtLeanClrMethod echoMethod = default;
    TtLeanClrMethod repeatMethod = default;
    TtLeanClrMethod measureMethod = default;
    TtLeanClrMethod greetMethod = default;
    TtLeanClrMethod throwMethod = default;
    TtLeanClrObject instance = default;
    try
    {
        if (!runtime.LoadAssembly("Titan.Tests.LeanCLR.Script"))
            throw new InvalidOperationException("LeanCLR failed to load the netstandard2.0 probe assembly.");

        // Intrinsic 劫持只在方法首次解析 invoker 时生效，必须在加载/调用热更方法前注册。
        bool isRegistered = true;
        if (isRegistered)
        {
            dispatcher.Register(signature, replacementHandler);
            dispatcher.Register(instanceSignature, instanceHandler);
        }

        addMethod = runtime.FindStaticMethod("Titan.Tests.LeanCLR.Script", "Titan.HotPatch.HotPatchProbe", "Add");
        if (!addMethod.IsValidPointer)
            throw new InvalidOperationException("LeanCLR failed to find HotPatchProbe.Add.");
        var result = TtLeanClrInvokeHelper.Invoke<int>(runtime, addMethod, 20, 22);
        if (isRegistered)
        {
            if (result != -1 || replacementHandlerCalls != 1)
                throw new InvalidOperationException(
                    $"LeanCLR Add did not dispatch IntrinsicAdd to the replacement handler: result={result}, calls={replacementHandlerCalls}.");
            Console.WriteLine("LeanCLR intrinsic probe passed: Add dispatched IntrinsicAdd to replacementHandler and returned -1.");
        }
        else
        {
            if (result != 42)
                throw new InvalidOperationException($"LeanCLR Add did not return the expected result: {result}.");
            Console.WriteLine($"LeanCLR Add return the expected result: {result}.");
        }

        instance = runtime.CreateObject("Titan.Tests.LeanCLR.Script", "Titan.HotPatch.HotPatchInstanceProbe");
        if (!instance.IsValidPointer)
            throw new InvalidOperationException("LeanCLR failed to create HotPatchInstanceProbe.");
        instanceAddMethod = runtime.FindMethod("Titan.Tests.LeanCLR.Script", "Titan.HotPatch.HotPatchInstanceProbe", "Add");
        if (!instanceAddMethod.IsValidPointer)
            throw new InvalidOperationException("LeanCLR failed to find HotPatchInstanceProbe.Add.");

        result = TtLeanClrInvokeHelper.InvokeInstance<int>(runtime, instance, instanceAddMethod, 20, 22);
        var expectedInstanceResult = isRegistered ? -42 : 42;
        var expectedInstanceCalls = isRegistered ? 1 : 0;
        if (result != expectedInstanceResult || instanceHandlerCalls != expectedInstanceCalls)
            throw new InvalidOperationException(
                $"LeanCLR instance dispatch failed: result={result}, calls={instanceHandlerCalls}.");
        Console.WriteLine($"LeanCLR instance probe passed: result={result}, calls={instanceHandlerCalls}.");

        // 宿主调 VM 的 string 实参与返回值：按值拷贝，null 透传
        echoMethod = runtime.FindStaticMethod("Titan.Tests.LeanCLR.Script", "Titan.HotPatch.HotPatchProbe", "Echo");
        repeatMethod = runtime.FindStaticMethod("Titan.Tests.LeanCLR.Script", "Titan.HotPatch.HotPatchProbe", "Repeat");
        measureMethod = runtime.FindMethod("Titan.Tests.LeanCLR.Script", "Titan.HotPatch.HotPatchInstanceProbe", "Measure");
        greetMethod = runtime.FindMethod("Titan.Tests.LeanCLR.Script", "Titan.HotPatch.HotPatchInstanceProbe", "Greet");
        if (!echoMethod.IsValidPointer || !repeatMethod.IsValidPointer || !measureMethod.IsValidPointer || !greetMethod.IsValidPointer)
            throw new InvalidOperationException("LeanCLR failed to find the string probe methods.");
        var echo = TtLeanClrInvokeHelper.InvokeString(runtime, echoMethod, "勇者");
        var echoEmpty = TtLeanClrInvokeHelper.InvokeString(runtime, echoMethod, "");
        var echoNull = TtLeanClrInvokeHelper.InvokeString(runtime, echoMethod, new object[] { null });
        var repeat = TtLeanClrInvokeHelper.InvokeString(runtime, repeatMethod, "ab", 3);
        var measure = TtLeanClrInvokeHelper.InvokeInstance<int>(runtime, instance, measureMethod, "勇者");
        var measureNull = TtLeanClrInvokeHelper.InvokeInstance<int>(runtime, instance, measureMethod, new object[] { null });
        var greet = TtLeanClrInvokeHelper.InvokeInstanceString(runtime, instance, greetMethod, "勇者");
        if (echo != "勇者" || echoEmpty != "" || echoNull != null || repeat != "ababab" || measure != 2 || measureNull != -1 || greet != "Hi 勇者")
            throw new InvalidOperationException(
                $"LeanCLR host string invoke failed: echo={echo}, empty={echoEmpty?.Length}, null={echoNull == null}, repeat={repeat}, measure={measure}, measureNull={measureNull}, greet={greet}.");
        Console.WriteLine($"LeanCLR host string invoke probe passed: Repeat={repeat}, Measure={measure}, MeasureNull={measureNull}, EchoNull={echoNull == null}.");

        // VM 托管异常经 GetLastError 带回 "操作: 类型全名: message"
        throwMethod = runtime.FindStaticMethod("Titan.Tests.LeanCLR.Script", "Titan.HotPatch.HotPatchProbe", "ThrowProbe");
        if (!throwMethod.IsValidPointer)
            throw new InvalidOperationException("LeanCLR failed to find HotPatchProbe.ThrowProbe.");
        if (runtime.InvokeWithBuffer(throwMethod, null, 0, null, throwMethod.GetRetBufferByteSize()))
            throw new InvalidOperationException("LeanCLR ThrowProbe did not fail.");
        var lastError = runtime.GetLastError().Text;
        if (lastError != "InvokeWithBuffer: System.InvalidOperationException: LeanCLR netstandard probe")
            throw new InvalidOperationException($"LeanCLR exception detail mismatch: {lastError}");
        Console.WriteLine($"LeanCLR exception detail probe passed: {lastError}");

        // 非属主线程调用失败；AcquireOwnerThread 后可调用，再切回主线程
        int otherResult = 0; bool otherBefore = true, otherAcquired = false;
        var other = new Thread(() =>
        {
            otherBefore = runtime.InvokeWithBuffer(throwMethod, null, 0, null, throwMethod.GetRetBufferByteSize()) ||
                          runtime.GetLastError().Text != "InvokeWithBuffer";
            otherAcquired = runtime.AcquireOwnerThread();
            otherResult = TtLeanClrInvokeHelper.Invoke<int>(runtime, addMethod, 1, 2);
        });
        other.Start();
        other.Join();
        if (otherBefore || !otherAcquired || otherResult != -1 || !runtime.AcquireOwnerThread())
            throw new InvalidOperationException($"LeanCLR owner thread probe failed: before={otherBefore}, acquired={otherAcquired}, result={otherResult}.");
        Console.WriteLine("LeanCLR owner thread probe passed.");

        // dispatcher 缓存属主线程；跨线程时切换 native 属主，切回后主线程继续可用
        bool dispatcherOther = false; int dispatcherResult = 0;
        var dispatcherThread = new Thread(() =>
        {
            dispatcherOther = dispatcher.EnsureOwnerThread();
            dispatcherResult = TtLeanClrInvokeHelper.Invoke<int>(runtime, addMethod, 1, 2);
        });
        dispatcherThread.Start();
        dispatcherThread.Join();
        if (!dispatcherOther || dispatcherResult != -1 || !dispatcher.EnsureOwnerThread() ||
            TtLeanClrInvokeHelper.Invoke<int>(runtime, addMethod, 1, 2) != -1)
            throw new InvalidOperationException("LeanCLR dispatcher EnsureOwnerThread probe failed.");
        Console.WriteLine("LeanCLR dispatcher owner thread probe passed.");

        // 生成器代理：脚本调用 [TtLeanClr] 宿主桩方法，经 HostHandle 找回宿主对象执行真实方法
        var actor = new Titan.HostGame.TtProbeActor();
        var vmActor = TtLeanClrHostBinding.GetOrCreate(actor);
        hitMethod = FindProbeMethod(runtime, "Hit");
        selfIsSameMethod = FindProbeMethod(runtime, "SelfIsSame");
        scaleMethod = FindProbeMethod(runtime, "Scale");
        stringRoundTripMethod = FindProbeMethod(runtime, "StringRoundTrip");

        var hit = TtLeanClrInvokeHelper.Invoke<int>(runtime, hitMethod, vmActor, 30);
        if (hit != 72 || actor.Hp != 70)
            throw new InvalidOperationException($"LeanCLR host proxy Hit failed: result={hit}, hp={actor.Hp}.");
        var selfIsSame = TtLeanClrInvokeHelper.Invoke<bool>(runtime, selfIsSameMethod, vmActor);
        if (!selfIsSame)
            throw new InvalidOperationException("LeanCLR host proxy Self did not round-trip to the same VM object.");
        var scaled = TtLeanClrInvokeHelper.Invoke<float>(runtime, scaleMethod, vmActor, 2f);
        if (scaled != 3f)
            throw new InvalidOperationException($"LeanCLR host proxy Scale failed: result={scaled}.");
        Console.WriteLine($"LeanCLR host proxy probe passed: Hit={hit}, Hp={actor.Hp}, SelfIsSame={selfIsSame}, Scale={scaled}.");

        var stringRoundTrip = TtLeanClrInvokeHelper.Invoke<bool>(runtime, stringRoundTripMethod, vmActor);
        if (!stringRoundTrip || actor.LastGreeting != "勇者")
            throw new InvalidOperationException($"LeanCLR host proxy string round-trip failed: result={stringRoundTrip}, lastGreeting={actor.LastGreeting}.");
        Console.WriteLine($"LeanCLR host proxy string probe passed: StringRoundTrip={stringRoundTrip}, LastGreetingLength={actor.LastGreeting.Length}.");

        // 放在最后：会改写宿主类型映射并激活补丁
        patchProbe.Run(dispatcher);
    }
    finally
    {
        TtLeanClrRuntime.SetIntrinsicDispatcher(null);
        foreach (var method in new[] { hitMethod, selfIsSameMethod, scaleMethod, stringRoundTripMethod, echoMethod, repeatMethod, measureMethod, greetMethod, throwMethod })
        {
            if (method.IsValidPointer)
                CoreSDK.IUnknown_Release(method.NativePointer.ToPointer());
        }
        if (instanceAddMethod.IsValidPointer)
            CoreSDK.IUnknown_Release(instanceAddMethod.NativePointer.ToPointer());
        if (instance.IsValidPointer)
            CoreSDK.IUnknown_Release(instance.NativePointer.ToPointer());
        if (addMethod.IsValidPointer)
            CoreSDK.IUnknown_Release(addMethod.NativePointer.ToPointer());
        GC.KeepAlive(dispatcher);
        GC.KeepAlive(replacementHandler);
        GC.KeepAlive(instanceHandler);
    }
}

static unsafe void RunDispatcherCleanupLifetimeProbe(TtLeanClrRuntime runtime)
{
    var pointer = runtime.NativePointer.ToPointer();
    // 红灯时 Cleanup 会多 Release 一次；额外引用保证断言前不会销毁进程级 VM。
    CoreSDK.IUnknown_AddRef(pointer);
    var expectedReferences = CoreSDK.IUnknown_UnsafeGetRefCount(pointer);
    try
    {
        var dispatcher = new TtLeanClrIntrinsicDispatcher();
        if (!dispatcher.AttachRuntime(runtime))
            throw new InvalidOperationException("Cleanup 探针无法接入已初始化的 VM。");
        dispatcher.Cleanup(null!);
        var actualReferences = CoreSDK.IUnknown_UnsafeGetRefCount(pointer);
        if (actualReferences != expectedReferences)
        {
            // 补回错误消费的引用，随后安全撤销探针自身的保护引用。
            if (actualReferences == expectedReferences - 1)
                CoreSDK.IUnknown_AddRef(pointer);
            throw new InvalidOperationException($"Cleanup 不得释放进程级 VM：expected={expectedReferences}, actual={actualReferences}。");
        }
        if (dispatcher.IsRuntimeReady || !runtime.IsInitialized())
            throw new InvalidOperationException("Cleanup 应解除 dispatcher，但保持 VM 已初始化。");
        dispatcher.Cleanup(null!);
        if (CoreSDK.IUnknown_UnsafeGetRefCount(pointer) != expectedReferences)
            throw new InvalidOperationException("重复 Cleanup 不得消费 VM 引用。");
        Console.WriteLine("LeanCLR dispatcher cleanup lifetime probe passed.");
        GC.KeepAlive(dispatcher);
    }
    finally
    {
        CoreSDK.IUnknown_Release(pointer);
    }
}

static unsafe void RunNativeMemoryProbe()
{
    // 旧绑定只有四个参数：必须先确认 ABI，再注册委托，避免错误签名写坏堆。
    var parameters = typeof(CoreSDK.FDelegate_FOnNativeMemAlloc).GetMethod("Invoke")!.GetParameters();
    if (parameters.Length != 5 || parameters[0].ParameterType != typeof(void*))
        throw new InvalidOperationException("分配回调必须透传原始 ptr，当前绑定仍是旧 ABI。");

    var profiler = new EngineNS.Profiler.TtNativeMemory();
    var pointers = new List<nint>();
    var reported = new HashSet<nint>();
    CoreSDK.FDelegate_FOnNativeMemLeak leakCallback = (ptr, size, file, line, id, info) => reported.Add((nint)ptr);
    bool previous = EngineNS.Profiler.TtNativeMemory.IsNoReportLeaks;
    try
    {
        profiler.BeginProfiler();
        CoreSDK.SetMemLeakCallBack(leakCallback);
        EngineNS.Profiler.TtNativeMemory.IsNoReportLeaks = true;
        pointers.Add((nint)CoreSDK.Alloc(64, null!, 1));
        pointers.Add((nint)CoreSDK.Alloc(4096, null!, 2));

        // ThreadStatic 不得抑制其它线程的分配。
        var worker = new System.Threading.Thread(() =>
        {
            pointers.Add((nint)CoreSDK.Alloc(64, null!, 3));
            pointers.Add((nint)CoreSDK.Alloc(4096, null!, 4));
        });
        worker.Start();
        if (!worker.Join(TimeSpan.FromSeconds(10)))
            throw new InvalidOperationException("分配回调重入内存锁时发生阻塞。");

        EngineNS.Profiler.TtNativeMemory.IsNoReportLeaks = false;
        pointers.Add((nint)CoreSDK.Alloc(64, null!, 5));
        pointers.Add((nint)CoreSDK.Alloc(4096, null!, 6));
        CoreSDK.DumpNativeMemoryState("NativeMemoryProbe", 1);
        if (pointers.Count != 6 || pointers.Any(ptr => ptr == 0) ||
            reported.Contains(pointers[0]) || reported.Contains(pointers[1]) ||
            pointers.Skip(2).Any(ptr => !reported.Contains(ptr)))
            throw new InvalidOperationException("线程标记未正确区分大小块、其它线程及恢复后的分配。");
        Console.WriteLine("Native memory probe passed: ptr ABI, small/large marking, thread isolation and flag restoration.");
    }
    finally
    {
        EngineNS.Profiler.TtNativeMemory.IsNoReportLeaks = previous;
        profiler.EndProfiler();
        // 已显式解除回调，避免终结器随后清掉下一个探针注册的回调。
        GC.SuppressFinalize(profiler);
        foreach (var ptr in pointers)
            CoreSDK.Free((void*)ptr);
        GC.KeepAlive(profiler);
        GC.KeepAlive(leakCallback);
    }
}

static unsafe void RunNativeMemoryLockProbe()
{
    if (typeof(CoreSDK).GetMethod(nameof(CoreSDK.SetMemDebugText), new[] { typeof(void*), typeof(string) }) == null)
        throw new InvalidOperationException("SetMemDebugText 必须保留原二参数签名，兼容既有托管调用方。");

    var pointers = new List<nint>();
    var debugInfo = new Dictionary<nint, string?>();
    // 分配回调已经持锁：两个设置接口都必须允许显式跳过锁。
    CoreSDK.FDelegate_FOnNativeMemAlloc allocCallback = (ptr, size, file, line, id) =>
    {
        if (size == 64)
            CoreSDK.SetMemDebugInfo(ptr, "callback", false);
        else
            CoreSDK.SetMemDebugText(ptr, "callback", false);
    };
    CoreSDK.FDelegate_FOnNativeMemLeak leakCallback = (ptr, size, file, line, id, info) =>
        debugInfo[(nint)ptr] = Marshal.PtrToStringUTF8((nint)info);
    try
    {
        CoreSDK.SetMemAllocCallBack(allocCallback);
        CoreSDK.SetMemLeakCallBack(leakCallback);
        pointers.Add((nint)CoreSDK.Alloc(64, null!, 1));
        pointers.Add((nint)CoreSDK.Alloc(4096, null!, 2));
        CoreSDK.SetMemAllocCallBack(null);
        if (pointers.Count != 2 || pointers.Any(ptr => ptr == 0))
            throw new InvalidOperationException("锁参数探针未成功分配大小块。");
        CoreSDK.DumpNativeMemoryState("NativeMemoryLockProbe", 1);
        foreach (var ptr in pointers)
        {
            if (!debugInfo.TryGetValue(ptr, out var info) || info != "callback")
                throw new InvalidOperationException("持锁回调中 SetMemDebugInfo(false) 未写入正确说明。");
        }

        // 回调以外默认加锁，并验证重复更新 DebugInfo 和事后补标记。
        CoreSDK.SetMemDebugInfo((void*)pointers[0], "outside");
        CoreSDK.SetMemDebugText((void*)pointers[1], "outside");
        debugInfo.Clear();
        CoreSDK.DumpNativeMemoryState("NativeMemoryLockProbe", 1);
        foreach (var ptr in pointers)
        {
            if (!debugInfo.TryGetValue(ptr, out var info) || info != "outside")
                throw new InvalidOperationException("普通调用 SetMemDebugInfo(true) 未更新正确说明。");
            CoreSDK.SetMemNoLeakReport((void*)ptr);
        }
        debugInfo.Clear();
        CoreSDK.DumpNativeMemoryState("NativeMemoryLockProbe", 1);
        if (pointers.Any(debugInfo.ContainsKey))
            throw new InvalidOperationException("普通调用 SetMemNoLeakReport(true) 未关闭报告。");
        Console.WriteLine("Native memory lock probe passed: explicit unlocked callbacks and default locked setters.");
    }
    finally
    {
        CoreSDK.SetMemAllocCallBack(null);
        CoreSDK.SetMemLeakCallBack(null);
        foreach (var ptr in pointers)
            CoreSDK.Free((void*)ptr);
        GC.KeepAlive(allocCallback);
        GC.KeepAlive(leakCallback);
    }
}

static string FindRepoRoot()
{
    for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory != null; directory = directory.Parent)
    {
        if (File.Exists(Path.Combine(directory.FullName, "EngineAll.sln")))
            return directory.FullName;
    }
    throw new DirectoryNotFoundException($"Could not find EngineAll.sln above {AppContext.BaseDirectory}.");
}

static TtLeanClrMethod FindProbeMethod(TtLeanClrRuntime runtime, string name)
{
    var method = runtime.FindStaticMethod("Titan.Tests.LeanCLR.Script", "Titan.HotPatch.HostGameProbe", name);
    if (!method.IsValidPointer)
        throw new InvalidOperationException($"LeanCLR failed to find HostGameProbe.{name}.");
    return method;
}

static unsafe void RegisterAssembly(TtLeanClrRuntime runtime, string assemblyName, string dllPath, List<KeyValuePair<string, string>>? collect = null)
{
    if (!File.Exists(dllPath))
        throw new FileNotFoundException($"LeanCLR assembly was not found: {assemblyName}", dllPath);

    using var dll = CreateBlob(dllPath);
    var pdbPath = Path.ChangeExtension(dllPath, ".pdb");
    using var pdb = File.Exists(pdbPath) ? CreateBlob(pdbPath) : null;
    if (!runtime.RegisterAssembly(assemblyName, dll.mCoreObject, pdb == null ? default : pdb.mCoreObject))
        throw new InvalidOperationException($"LeanCLR failed to register assembly: {assemblyName}");
    collect?.Add(new(assemblyName, dllPath));
}

static unsafe TtBlobObject CreateBlob(string path)
{
    var bytes = File.ReadAllBytes(path);
    var blob = new TtBlobObject();
    fixed (byte* data = bytes)
    {
        blob.PushData(data, (uint)bytes.Length);
    }
    return blob;
}

static partial class NativeMethods
{
    [LibraryImport("kernel32.dll", EntryPoint = "SetDllDirectoryW", SetLastError = true, StringMarshalling = StringMarshalling.Utf16)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool SetDllDirectory(string path);
}
