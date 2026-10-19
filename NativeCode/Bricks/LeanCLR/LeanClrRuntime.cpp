#include "LeanClrRuntime.h"

#pragma push_macro("interface")
#undef interface
#define LEANCLR_STATIC
#include "public/leanclr.h"
#include "vm/assembly.h"
#include "vm/class.h"
#include "vm/field.h"
#include "vm/gchandle.h"
#include "vm/intrinsics.h"
#include "vm/method.h"
#include "vm/object.h"
#include "vm/rt_string.h"
#include "vm/rt_managed_types.h"
#include "vm/settings.h"
#include "metadata/metadata_name.h"
#include "metadata/rt_metadata.h"
#include "metadata/module_def.h"
#include "utils/string_builder.h"
#pragma pop_macro("interface")

#define new VNEW

NS_BEGIN

namespace LeanCLR
{
    TtLeanClrRuntime* TtLeanClrRuntime::sActiveRuntime = nullptr;
    FLeanClrIntrinsicDispatcher TtLeanClrRuntime::mIntrinsicDispatcher = nullptr;

    // BindHostObjectClass 绑定的 VM 侧宿主对象基类与 handle 字段偏移（含对象头）
    static const leanclr::metadata::RtClass* gHostObjectClass = nullptr;
    static INT32 gHostHandleOffset = -1;

    // ── shared helpers: both TtLeanClrMethod and FLeanClrIntrinsicCall delegate here ──

    static VNameString GetMethodAssemblyName(const LeanclrMethodInfo* raw)
    {
        auto method = reinterpret_cast<const leanclr::metadata::RtMethodInfo*>(raw);
        if (method == nullptr || method->parent == nullptr || method->parent->image == nullptr)
            return VNameString();
        return VNameString(method->parent->image->get_name_no_ext());
    }

    static VNameString GetMethodClassName(const LeanclrMethodInfo* raw)
    {
        auto method = reinterpret_cast<const leanclr::metadata::RtMethodInfo*>(raw);
        if (method == nullptr || method->parent == nullptr)
            return VNameString();

        leanclr::utils::Utf8StringBuilder builder;
        auto result = leanclr::metadata::MetadataName::append_type_full_name(builder, method->parent->by_val,
            leanclr::metadata::TypeNameFormat::InternalName, false);
        if (!result.is_ok())
            return VNameString();
        return VNameString(builder.get_const_chars());
    }

    static VNameString GetMethodMethodName(const LeanclrMethodInfo* raw)
    {
        auto method = reinterpret_cast<const leanclr::metadata::RtMethodInfo*>(raw);
        return method == nullptr || method->name == nullptr ? VNameString() : VNameString(method->name);
    }

    static VNameString GetMethodSignature(const LeanclrMethodInfo* raw)
    {
        auto method = reinterpret_cast<const leanclr::metadata::RtMethodInfo*>(raw);
        if (method == nullptr)
            return VNameString();

        leanclr::utils::Utf8StringBuilder builder;
        auto result = leanclr::metadata::MetadataName::append_method_full_name_with_params(builder, method,
            leanclr::metadata::TypeNameFormat::InternalName);
        if (!result.is_ok())
            return VNameString();
        return VNameString(builder.get_const_chars());
    }

    static UINT GetMethodArgumentCount(const LeanclrMethodInfo* raw)
    {
        auto method = reinterpret_cast<const leanclr::metadata::RtMethodInfo*>(raw);
        return method == nullptr ? 0 : method->parameter_count;
    }

    static VNameString GetMethodTypeName(const leanclr::metadata::RtTypeSig* type)
    {
        if (type == nullptr)
            return VNameString();

        leanclr::utils::Utf8StringBuilder builder;
        auto result = leanclr::metadata::MetadataName::append_type_full_name(builder, type,
            leanclr::metadata::TypeNameFormat::InternalName, false);
        if (!result.is_ok())
            return VNameString();
        return VNameString(builder.get_const_chars());
    }

    static VNameString GetMethodArgumentType(const LeanclrMethodInfo* raw, UINT index)
    {
        auto method = reinterpret_cast<const leanclr::metadata::RtMethodInfo*>(raw);
        if (method == nullptr || index >= method->parameter_count)
            return VNameString();
        return GetMethodTypeName(method->parameters[index]);
    }

    static VNameString GetMethodReturnType(const LeanclrMethodInfo* raw)
    {
        auto method = reinterpret_cast<const leanclr::metadata::RtMethodInfo*>(raw);
        return method == nullptr ? VNameString() : GetMethodTypeName(method->return_type);
    }

    // ── TtLeanClrMethod ──

    VNameString TtLeanClrMethod::GetAssemblyName() const { return GetMethodAssemblyName(Method); }
    VNameString TtLeanClrMethod::GetClassName() const { return GetMethodClassName(Method); }
    VNameString TtLeanClrMethod::GetMethodName() const { return GetMethodMethodName(Method); }
    VNameString TtLeanClrMethod::GetSignature() const { return GetMethodSignature(Method); }
    UINT TtLeanClrMethod::GetArgumentCount() const { return GetMethodArgumentCount(Method); }
    VNameString TtLeanClrMethod::GetArgumentType(UINT index) const { return GetMethodArgumentType(Method, index); }
    VNameString TtLeanClrMethod::GetReturnType() const { return GetMethodReturnType(Method); }

    UINT TtLeanClrMethod::GetArgBufferByteSize() const
    {
        return Method == nullptr ? 0 : (UINT)(leanclr_get_total_arg_stack_object_size(Method) * LEANCLR_STACK_OBJECT_SIZE);
    }

    UINT TtLeanClrMethod::GetRetBufferByteSize() const
    {
        return Method == nullptr ? 0 : (UINT)(leanclr_get_return_value_stack_object_size(Method) * LEANCLR_STACK_OBJECT_SIZE);
    }

    bool TtLeanClrMethod::IsStatic() const
    {
        auto method = reinterpret_cast<const leanclr::metadata::RtMethodInfo*>(Method);
        return method != nullptr && leanclr::vm::Method::is_static(method);
    }

    // ── TtLeanClrObject ──

    TtLeanClrObject::~TtLeanClrObject()
    {
        if (mGCHandle != nullptr)
        {
            leanclr::vm::GCHandle::free_handle(mGCHandle);
            mGCHandle = nullptr;
        }
    }

    void* TtLeanClrObject::GetTarget() const
    {
        return mGCHandle == nullptr ? nullptr : leanclr::vm::GCHandle::get_target(mGCHandle);
    }

    static void** GetHostHandleSlot(leanclr::vm::RtObject* obj)
    {
        if (obj == nullptr || gHostObjectClass == nullptr || gHostHandleOffset < 0 ||
            !leanclr::vm::Class::has_class_parent_fast(obj->klass, gHostObjectClass))
            return nullptr;
        return reinterpret_cast<void**>(reinterpret_cast<BYTE*>(obj) + gHostHandleOffset);
    }

    bool TtLeanClrObject::SetHostHandle(void* handle)
    {
        auto slot = GetHostHandleSlot(static_cast<leanclr::vm::RtObject*>(GetTarget()));
        if (slot == nullptr)
            return false;
        *slot = handle;
        return true;
    }

    void* TtLeanClrObject::GetHostHandle() const
    {
        auto slot = GetHostHandleSlot(static_cast<leanclr::vm::RtObject*>(GetTarget()));
        return slot == nullptr ? nullptr : *slot;
    }

    // ── FLeanClrIntrinsicCall ──

    FLeanClrIntrinsicCall::FLeanClrIntrinsicCall(const LeanclrMethodInfo* method, const LeanclrStackObject* args,
                                                 LeanclrStackObject* ret, LeanclrException** exception)
        : mMethod(method)
        , mArgs(args)
        , mReturn(ret)
        , mException(exception)
    {
    }

    VNameString FLeanClrIntrinsicCall::GetAssemblyName() const { return GetMethodAssemblyName(mMethod); }
    VNameString FLeanClrIntrinsicCall::GetClassName() const { return GetMethodClassName(mMethod); }
    VNameString FLeanClrIntrinsicCall::GetMethodName() const { return GetMethodMethodName(mMethod); }
    VNameString FLeanClrIntrinsicCall::GetSignature() const { return GetMethodSignature(mMethod); }
    VNameString FLeanClrIntrinsicCall::GetArgumentType(UINT index) const { return GetMethodArgumentType(mMethod, index); }
    VNameString FLeanClrIntrinsicCall::GetReturnType() const { return GetMethodReturnType(mMethod); }

    void* FLeanClrIntrinsicCall::GetUserData() const
    {
        auto method = reinterpret_cast<const leanclr::metadata::RtMethodInfo*>(mMethod);
        return method == nullptr ? nullptr : method->user_data;
    }

    void FLeanClrIntrinsicCall::SetUserData(void* userData)
    {
        auto method = const_cast<leanclr::metadata::RtMethodInfo*>(reinterpret_cast<const leanclr::metadata::RtMethodInfo*>(mMethod));
        if (method != nullptr)
            method->user_data = userData;
    }

    UINT FLeanClrIntrinsicCall::GetArgumentCount() const { return GetMethodArgumentCount(mMethod); }

    void* FLeanClrIntrinsicCall::GetArgumentBuffer() const
    {
        return const_cast<LeanclrStackObject*>(mArgs);
    }

    void* FLeanClrIntrinsicCall::GetReturnBuffer() const
    {
        return mReturn;
    }

    bool FLeanClrIntrinsicCall::GetArgument(UINT index, void* value, size_t valueSize) const
    {
        auto method = reinterpret_cast<const leanclr::metadata::RtMethodInfo*>(mMethod);
        if (method == nullptr || mArgs == nullptr || value == nullptr || index >= method->parameter_count)
            return false;
        if (method->arg_descs[index].stack_object_size < leanclr_get_stack_object_size_of_byte_size(valueSize))
            return false;

        // For instance methods the 'this' reference occupies the first stack-object slot;
        // arg_descs[] only describes the declared parameters, so skip slot 0.
        size_t offset = leanclr::vm::Method::is_instance(method) ? 1 : 0;
        for (UINT i = 0; i < index; ++i)
            offset += method->arg_descs[i].stack_object_size;
        leanclr_get_argument(mArgs, &offset, value, valueSize);
        return true;
    }

    bool FLeanClrIntrinsicCall::GetArgumentBoolean(UINT index, bool& value) const
    {
        return GetArgument(index, &value, sizeof(value));
    }

    bool FLeanClrIntrinsicCall::GetArgumentInt32(UINT index, INT32& value) const
    {
        return GetArgument(index, &value, sizeof(value));
    }

    bool FLeanClrIntrinsicCall::GetArgumentInt64(UINT index, INT64& value) const
    {
        return GetArgument(index, &value, sizeof(value));
    }

    bool FLeanClrIntrinsicCall::GetArgumentFloat(UINT index, float& value) const
    {
        return GetArgument(index, &value, sizeof(value));
    }

    bool FLeanClrIntrinsicCall::GetArgumentDouble(UINT index, double& value) const
    {
        return GetArgument(index, &value, sizeof(value));
    }

    bool FLeanClrIntrinsicCall::SetReturn(const void* value, size_t valueSize)
    {
        auto method = reinterpret_cast<const leanclr::metadata::RtMethodInfo*>(mMethod);
        if (method == nullptr || mReturn == nullptr || value == nullptr ||
            method->ret_stack_object_size < leanclr_get_stack_object_size_of_byte_size(valueSize))
            return false;
        leanclr_set_return_value(mReturn, value, valueSize);
        return true;
    }

    bool FLeanClrIntrinsicCall::SetReturnBoolean(bool value)
    {
        // 小于 4 字节的返回值按 int32 写满，ret 与 args 共用内存时不留参数残余
        INT32 boolValue = value ? 1 : 0;
        return SetReturn(&boolValue, sizeof(boolValue));
    }

    bool FLeanClrIntrinsicCall::SetReturnInt32(INT32 value)
    {
        return SetReturn(&value, sizeof(value));
    }

    bool FLeanClrIntrinsicCall::SetReturnInt64(INT64 value)
    {
        return SetReturn(&value, sizeof(value));
    }

    bool FLeanClrIntrinsicCall::SetReturnFloat(float value)
    {
        return SetReturn(&value, sizeof(value));
    }

    bool FLeanClrIntrinsicCall::SetReturnDouble(double value)
    {
        return SetReturn(&value, sizeof(value));
    }

    void FLeanClrIntrinsicCall::SetFailed()
    {
        mSucceeded = false;
    }

    bool FLeanClrIntrinsicCall::IsSucceeded() const
    {
        return mSucceeded;
    }

    TtLeanClrRuntime::TtLeanClrRuntime()
    {
    }

    TtLeanClrRuntime::~TtLeanClrRuntime()
    {
        if (sActiveRuntime == this && !mInitialized)
            sActiveRuntime = nullptr;
    }

    bool TtLeanClrRuntime::SetError(const char* operation, INT32 errorCode)
    {
        mLastError = operation;
        VFX_LTRACE(ELTT_Error, "LeanCLR %s failed, error=%d\n", operation, errorCode);
        return false;
    }

    bool TtLeanClrRuntime::SetException(const char* operation, LeanclrException* exception)
    {
        std::string error = operation;
        auto* rtException = reinterpret_cast<leanclr::vm::RtException*>(exception);
        if (rtException != nullptr && rtException->klass != nullptr)
        {
            leanclr::utils::Utf8StringBuilder typeName;
            if (leanclr::metadata::MetadataName::append_type_full_name(typeName, rtException->klass->by_val,
                leanclr::metadata::TypeNameFormat::FullName, false).is_ok())
            {
                error += ": ";
                error.append(typeName.get_const_chars(), typeName.length());
            }
            if (rtException->message != nullptr)
            {
                leanclr::utils::Utf8StringBuilder message(leanclr::vm::String::get_chars_ptr(rtException->message),
                    static_cast<size_t>(leanclr::vm::String::get_length(rtException->message)));
                error += ": ";
                error.append(message.get_const_chars(), message.length());
            }
        }
        mLastError = std::move(error);
        VFX_LTRACE(ELTT_Error, "LeanCLR %s\n", mLastError.c_str());
        return false;
    }

    bool TtLeanClrRuntime::RegisterAssembly(const char* assemblyName, IBlobObject* assemblyBytes, IBlobObject* pdbBytes)
    {
        if (mInitialized || assemblyName == nullptr || assemblyBytes == nullptr || assemblyBytes->GetSize() == 0)
            return SetError("RegisterAssembly");

        FAssemblyData data;
        const auto dllSize = assemblyBytes->GetSize();
        auto dll = static_cast<const BYTE*>(assemblyBytes->GetData());
        data.Dll.assign(dll, dll + dllSize);
        if (pdbBytes != nullptr && pdbBytes->GetSize() > 0)
        {
            const auto pdbSize = pdbBytes->GetSize();
            auto pdb = static_cast<const BYTE*>(pdbBytes->GetData());
            data.Pdb.assign(pdb, pdb + pdbSize);
        }
        mAssemblies[assemblyName] = std::move(data);
        return true;
    }

    bool TtLeanClrRuntime::LoadAssemblyBytes(const char* assemblyName, const char* extension, leanclr::vm::FileData& fileData)
    {
        auto runtime = sActiveRuntime;
        if (runtime == nullptr || assemblyName == nullptr || extension == nullptr)
            return false;

        auto it = runtime->mAssemblies.find(assemblyName);
        if (it == runtime->mAssemblies.end())
            return false;

        const auto& bytes = strcmp(extension, "pdb") == 0 ? it->second.Pdb : it->second.Dll;
        if (bytes.empty())
            return false;

        fileData.data = bytes.data();
        fileData.length = bytes.size();
        fileData.shared = true;
        return true;
    }

    bool TtLeanClrRuntime::Initialize()
    {
        if (mInitialized)
            return true;
        if (sActiveRuntime != nullptr && sActiveRuntime != this)
            return SetError("Initialize: only one LeanCLR runtime is supported per process");
        if (mAssemblies.empty())
            return SetError("Initialize: core library bytes must be registered first");

        sActiveRuntime = this;
        leanclr::vm::Settings::set_file_loader(&TtLeanClrRuntime::LoadAssemblyBytes);
        const auto result = leanclr_initialize_runtime();
        if (result != 0)
        {
            sActiveRuntime = nullptr;
            return SetError("Initialize", result);
        }

        mInitialized = true;
        mOwnerThread = std::this_thread::get_id();
        return true;
    }

    leanclr::RtResultVoid Titan_LeanclrMethodInvoker(leanclr::metadata::RtManagedMethodPointer,
        const leanclr::metadata::RtMethodInfo* method, const leanclr::interp::RtStackObject* args,
        leanclr::interp::RtStackObject* ret)
    {
        if (TtLeanClrRuntime::mIntrinsicDispatcher == nullptr)
        {
            VFX_LTRACE(ELTT_Error, "LeanCLR intrinsic dispatcher is not registered.\n");
            RET_ERR(leanclr::core::RtErr::ExecutionEngine);
        }

        FLeanClrIntrinsicCall call(reinterpret_cast<const LeanclrMethodInfo*>(method),
            reinterpret_cast<const LeanclrStackObject*>(args), reinterpret_cast<LeanclrStackObject*>(ret), nullptr);
        TtLeanClrRuntime::mIntrinsicDispatcher(&call);
        if (!call.IsSucceeded())
        {
            VFX_LTRACE(ELTT_Error, "LeanCLR managed intrinsic dispatcher failed.\n");
            RET_ERR(leanclr::core::RtErr::ExecutionEngine);
        }
        return leanclr::core::Unit{};
    }

    void TtLeanClrRuntime::SetIntrinsicDispatcher(FLeanClrIntrinsicDispatcher dispatcher)
    {
        mIntrinsicDispatcher = dispatcher;
    }

    bool TtLeanClrRuntime::RegisterIntrinsicFunc(const char* signature)
    {
        if (!mInitialized || std::this_thread::get_id() != mOwnerThread || signature == nullptr)
            return SetError("RegisterIntrinsicFunc");

        auto [it, inserted] = mIntrinsicSignatures.emplace(signature);
        if (!inserted)
            return true;

        leanclr::vm::Intrinsics::register_intrinsic(it->c_str(), nullptr, &Titan_LeanclrMethodInvoker);
        return true;
    }

    bool TtLeanClrRuntime::LoadAssembly(const char* assemblyName)
    {
        if (!mInitialized || std::this_thread::get_id() != mOwnerThread || assemblyName == nullptr)
            return SetError("LoadAssembly");

        auto result = leanclr::vm::Assembly::load_by_name(assemblyName);
        if (result.is_err())
            return SetError("LoadAssembly", static_cast<INT32>(result.unwrap_err()));
        return true;
    }

    TtLeanClrMethod* TtLeanClrRuntime::FindStaticMethod(const char* assemblyName, const char* fullClassName, const char* methodName)
    {
        if (!mInitialized || std::this_thread::get_id() != mOwnerThread || assemblyName == nullptr || fullClassName == nullptr || methodName == nullptr)
        {
            SetError("FindStaticMethod");
            return nullptr;
        }

        LeanclrException* exception = nullptr;
        auto assembly = leanclr_get_assembly(assemblyName);
        auto module = assembly == nullptr ? nullptr : leanclr_get_module_by_assembly(assembly);
        auto klass = module == nullptr ? nullptr : leanclr_get_class_by_name(module, fullClassName, false, &exception);
        auto method = klass == nullptr || exception != nullptr ? nullptr : leanclr_get_class_method_by_name(klass, methodName, &exception);
        if (method == nullptr || exception != nullptr)
        {
            SetError("FindStaticMethod");
            return nullptr;
        }

        auto result = new TtLeanClrMethod();
        result->Method = method;
        return result;
    }

    bool TtLeanClrRuntime::InvokeWithBuffer(TtLeanClrMethod* method, BYTE* argBuffer, UINT argBufferByteSize, BYTE* retBuffer, UINT retBufferByteSize)
    {
        if (!mInitialized || std::this_thread::get_id() != mOwnerThread || method == nullptr || method->Method == nullptr)
            return SetError("InvokeWithBuffer");

        const auto expectedArgBufferByteSize = method->GetArgBufferByteSize();
        const auto expectedRetBufferByteSize = method->GetRetBufferByteSize();
        if (argBufferByteSize != expectedArgBufferByteSize || retBufferByteSize != expectedRetBufferByteSize ||
            (expectedArgBufferByteSize != 0 && argBuffer == nullptr) ||
            (expectedRetBufferByteSize != 0 && retBuffer == nullptr))
            return SetError("InvokeWithBuffer: invalid buffer");

        LeanclrException* exception = nullptr;
        leanclr_invoke_with_buffer(method->Method,
            reinterpret_cast<LeanclrStackObject*>(argBuffer),
            reinterpret_cast<LeanclrStackObject*>(retBuffer),
            &exception);
        if (exception != nullptr)
            return SetException("InvokeWithBuffer", exception);
        return true;
    }

    TtLeanClrObject* TtLeanClrRuntime::CreateObject(const char* assemblyName, const char* fullClassName)
    {
        if (!mInitialized || std::this_thread::get_id() != mOwnerThread || assemblyName == nullptr || fullClassName == nullptr)
        {
            SetError("CreateObject");
            return nullptr;
        }

        LeanclrException* exception = nullptr;
        auto assembly = leanclr_get_assembly(assemblyName);
        auto module = assembly == nullptr ? nullptr : leanclr_get_module_by_assembly(assembly);
        auto klass = module == nullptr ? nullptr : leanclr_get_class_by_name(module, fullClassName, false, &exception);
        if (klass == nullptr || exception != nullptr)
        {
            SetError("CreateObject: class not found");
            return nullptr;
        }

        auto* rtKlass = reinterpret_cast<leanclr::metadata::RtClass*>(klass);
        auto initResult = leanclr::vm::Class::initialize_all(rtKlass);
        if (initResult.is_err())
        {
            SetError("CreateObject: class initialization failed");
            return nullptr;
        }

        auto newObjResult = LEANCLR_NEWOBJ_INTERNAL(rtKlass, "TtLeanClrRuntime::CreateObject");
        if (newObjResult.is_err())
        {
            SetError("CreateObject: allocation failed");
            return nullptr;
        }

        auto* gcHandle = leanclr::vm::GCHandle::new_handle(newObjResult.unwrap(), false);
        if (gcHandle == nullptr)
        {
            SetError("CreateObject: GCHandle creation failed");
            return nullptr;
        }

        auto result = new TtLeanClrObject();
        result->mGCHandle = gcHandle;
        return result;
    }

    TtLeanClrMethod* TtLeanClrRuntime::FindMethod(const char* assemblyName, const char* fullClassName, const char* methodName)
    {
        if (!mInitialized || std::this_thread::get_id() != mOwnerThread || assemblyName == nullptr || fullClassName == nullptr || methodName == nullptr)
        {
            SetError("FindMethod");
            return nullptr;
        }

        LeanclrException* exception = nullptr;
        auto assembly = leanclr_get_assembly(assemblyName);
        auto module = assembly == nullptr ? nullptr : leanclr_get_module_by_assembly(assembly);
        auto klass = module == nullptr ? nullptr : leanclr_get_class_by_name(module, fullClassName, false, &exception);
        auto method = klass == nullptr || exception != nullptr ? nullptr : leanclr_get_class_method_by_name(klass, methodName, &exception);
        if (method == nullptr || exception != nullptr)
        {
            SetError("FindMethod");
            return nullptr;
        }

        auto result = new TtLeanClrMethod();
        result->Method = method;
        return result;
    }

    bool TtLeanClrRuntime::InvokeInstanceWithBuffer(TtLeanClrObject* instance, TtLeanClrMethod* method,
        BYTE* argBuffer, UINT argBufferByteSize, BYTE* retBuffer, UINT retBufferByteSize)
    {
        if (!mInitialized || std::this_thread::get_id() != mOwnerThread ||
            instance == nullptr || instance->mGCHandle == nullptr ||
            method == nullptr || method->Method == nullptr)
            return SetError("InvokeInstanceWithBuffer");

        auto rtMethod = reinterpret_cast<const leanclr::metadata::RtMethodInfo*>(method->Method);
        if (leanclr::vm::Method::is_static(rtMethod))
            return SetError("InvokeInstanceWithBuffer: method is static");

        // total_arg_stack_object_size includes the 'this' slot for instance methods
        const auto totalArgBufferByteSize = method->GetArgBufferByteSize();
        const auto expectedRetBufferByteSize = method->GetRetBufferByteSize();
        const UINT thisSlotByteSize = LEANCLR_STACK_OBJECT_SIZE;
        if (totalArgBufferByteSize < thisSlotByteSize)
            return SetError("InvokeInstanceWithBuffer: invalid method");

        const auto expectedUserArgByteSize = totalArgBufferByteSize - thisSlotByteSize;
        if (argBufferByteSize != expectedUserArgByteSize || retBufferByteSize != expectedRetBufferByteSize ||
            (expectedUserArgByteSize != 0 && argBuffer == nullptr) ||
            (expectedRetBufferByteSize != 0 && retBuffer == nullptr))
            return SetError("InvokeInstanceWithBuffer: invalid buffer");

        auto* obj = leanclr::vm::GCHandle::get_target(instance->mGCHandle);
        if (obj == nullptr)
            return SetError("InvokeInstanceWithBuffer: object has been collected");

        // Build combined buffer: [this | user args]
        auto* combinedArgBuffer = (BYTE*)alloca(totalArgBufferByteSize);
        std::memset(combinedArgBuffer, 0, totalArgBufferByteSize);

        auto* stackObj = reinterpret_cast<leanclr::interp::RtStackObject*>(combinedArgBuffer);
        if (leanclr::vm::Class::is_value_type(rtMethod->parent))
            stackObj[0].obj = obj + 1;  // unbox: skip object header
        else
            stackObj[0].obj = obj;

        if (expectedUserArgByteSize > 0 && argBuffer != nullptr)
            std::memcpy(combinedArgBuffer + thisSlotByteSize, argBuffer, expectedUserArgByteSize);

        LeanclrException* exception = nullptr;
        leanclr_invoke_with_buffer(method->Method,
            reinterpret_cast<LeanclrStackObject*>(combinedArgBuffer),
            reinterpret_cast<LeanclrStackObject*>(retBuffer),
            &exception);
        if (exception != nullptr)
            return SetException("InvokeInstanceWithBuffer", exception);
        return true;
    }

    bool TtLeanClrRuntime::BindHostObjectClass(const char* assemblyName, const char* fullClassName, const char* fieldName)
    {
        if (!mInitialized || std::this_thread::get_id() != mOwnerThread || assemblyName == nullptr || fullClassName == nullptr || fieldName == nullptr)
            return SetError("BindHostObjectClass");
        if (!LoadAssembly(assemblyName))
            return SetError("BindHostObjectClass: assembly load failed");

        LeanclrException* exception = nullptr;
        auto assembly = leanclr_get_assembly(assemblyName);
        auto module = assembly == nullptr ? nullptr : leanclr_get_module_by_assembly(assembly);
        auto klass = module == nullptr ? nullptr : leanclr_get_class_by_name(module, fullClassName, false, &exception);
        if (klass == nullptr || exception != nullptr)
            return SetError("BindHostObjectClass: class not found");

        auto* rtKlass = reinterpret_cast<leanclr::metadata::RtClass*>(klass);
        if (leanclr::vm::Class::initialize_all(rtKlass).is_err() || leanclr::vm::Class::is_value_type(rtKlass))
            return SetError("BindHostObjectClass: invalid class");

        auto field = reinterpret_cast<const leanclr::metadata::RtFieldInfo*>(leanclr_get_class_field_by_name(klass, fieldName, &exception));
        if (field == nullptr || exception != nullptr || !leanclr::vm::Field::is_instance(field) ||
            GetMethodTypeName(field->type_sig) != "System.IntPtr")
            return SetError("BindHostObjectClass: handle field must be an instance System.IntPtr");

        gHostObjectClass = rtKlass;
        gHostHandleOffset = static_cast<INT32>(leanclr::vm::Field::get_field_offset_includes_object_header_for_all_type(field));
        return true;
    }

    INT32 TtLeanClrRuntime::GetHostHandleOffset()
    {
        return gHostHandleOffset;
    }

    INT32 TtLeanClrRuntime::GetStringLengthOffset()
    {
        return static_cast<INT32>(offsetof(leanclr::vm::RtString, length));
    }

    INT32 TtLeanClrRuntime::GetStringCharsOffset()
    {
        return static_cast<INT32>(leanclr::vm::String::get_offset_to_string_data());
    }

    void* TtLeanClrRuntime::NewString(const void* utf16Chars, INT32 charCount)
    {
        if (!mInitialized || std::this_thread::get_id() != mOwnerThread || charCount < 0 || (utf16Chars == nullptr && charCount > 0))
        {
            SetError("NewString");
            return nullptr;
        }
        if (charCount == 0)
            return leanclr::vm::String::get_empty_string();
        return leanclr::vm::String::create_string_from_utf16chars(static_cast<const leanclr::Utf16Char*>(utf16Chars), charCount);
    }

    bool TtLeanClrRuntime::IsInitialized() const
    {
        return mInitialized;
    }

    bool TtLeanClrRuntime::AcquireOwnerThread()
    {
        if (!mInitialized)
            return SetError("AcquireOwnerThread");
        mOwnerThread = std::this_thread::get_id();
        return true;
    }

    VNameString TtLeanClrRuntime::GetLastError() const
    {
        return VNameString(mLastError.c_str());
    }
}

NS_END
