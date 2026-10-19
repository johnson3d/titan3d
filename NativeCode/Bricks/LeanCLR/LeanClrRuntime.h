#pragma once
#include "../../Base/BlobObject.h"
#include <map>
#include <set>
#include <string>
#include <thread>
#include <vector>

struct LeanclrMethodInfo;
struct LeanclrStackObject;
struct LeanclrException;
namespace leanclr
{
    namespace core
    {
        class ResultVoid;
    }
    namespace interp
    {
        union RtStackObject;
    }
    namespace metadata
    {
        struct RtMethodInfo;
    }
    namespace vm
    {
        struct FileData;
    }
}

NS_BEGIN

namespace LeanCLR
{
    struct TR_CLASS(SV_LayoutStruct = 8)
        FLeanClrIntrinsicCall
    {
    public:
        VNameString GetAssemblyName() const;
        VNameString GetClassName() const;
        VNameString GetMethodName() const;
        VNameString GetSignature() const;
        VNameString GetArgumentType(UINT index) const;
        VNameString GetReturnType() const;
        void* GetUserData() const;
        void SetUserData(void* userData);
        UINT GetArgumentCount() const;
        // 原始栈帧：每槽 8 字节，实例方法槽 0 为 this；args 与 ret 可能指向同一块内存，必须先读完参数再写返回值
        void* GetArgumentBuffer() const;
        void* GetReturnBuffer() const;
        bool GetArgumentBoolean(UINT index, bool& value) const;
        bool GetArgumentInt32(UINT index, INT32& value) const;
        bool GetArgumentInt64(UINT index, INT64& value) const;
        bool GetArgumentFloat(UINT index, float& value) const;
        bool GetArgumentDouble(UINT index, double& value) const;
        bool SetReturnBoolean(bool value);
        bool SetReturnInt32(INT32 value);
        bool SetReturnInt64(INT64 value);
        bool SetReturnFloat(float value);
        bool SetReturnDouble(double value);
        void SetFailed();
        bool IsSucceeded() const;

    private:
        friend leanclr::core::ResultVoid Titan_LeanclrMethodInvoker(void (*methodPtr)(), const leanclr::metadata::RtMethodInfo* method,
                                                                    const leanclr::interp::RtStackObject* args, leanclr::interp::RtStackObject* ret);
        FLeanClrIntrinsicCall(const LeanclrMethodInfo* method, const LeanclrStackObject* args,
                              LeanclrStackObject* ret, LeanclrException** exception);
        bool GetArgument(UINT index, void* value, size_t valueSize) const;
        bool SetReturn(const void* value, size_t valueSize);

        const LeanclrMethodInfo* mMethod = nullptr;
        const LeanclrStackObject* mArgs = nullptr;
        LeanclrStackObject* mReturn = nullptr;
        LeanclrException** mException = nullptr;
        bool mSucceeded = true;
    };

    TR_CALLBACK(SV_CallConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)
    typedef void (*FLeanClrIntrinsicDispatcher)(FLeanClrIntrinsicCall* call);

    class TR_CLASS()
        TtLeanClrMethod : public IWeakRefObject
    {
    public:
        ENGINE_RTTI(TtLeanClrMethod)

        const LeanclrMethodInfo* Method = nullptr;

        VNameString GetAssemblyName() const;
        VNameString GetClassName() const;
        VNameString GetMethodName() const;
        VNameString GetSignature() const;
        UINT GetArgumentCount() const;
        VNameString GetArgumentType(UINT index) const;
        VNameString GetReturnType() const;
        UINT GetArgBufferByteSize() const;
        UINT GetRetBufferByteSize() const;
        bool IsStatic() const;
    };

    class TR_CLASS()
        TtLeanClrObject : public IWeakRefObject
    {
    public:
        ENGINE_RTTI(TtLeanClrObject)
        ~TtLeanClrObject();

        void* GetTarget() const;
        // 写入 TtLeanClrRuntime::BindHostObjectClass 绑定的 handle 字段；对象须派生自该类
        bool SetHostHandle(void* handle);
        void* GetHostHandle() const;

    private:
        friend class TtLeanClrRuntime;
        void* mGCHandle = nullptr;
    };

    class TR_CLASS()
        TtLeanClrRuntime : public IWeakRefObject
    {
    public:
        ENGINE_RTTI(TtLeanClrRuntime)

        TtLeanClrRuntime();
        ~TtLeanClrRuntime();

        bool RegisterAssembly(const char* assemblyName, IBlobObject* assemblyBytes, IBlobObject* pdbBytes = nullptr);
        bool Initialize();
        static void SetIntrinsicDispatcher(FLeanClrIntrinsicDispatcher dispatcher);
        bool RegisterIntrinsicFunc(const char* signature);
        bool LoadAssembly(const char* assemblyName);
        TtLeanClrMethod* FindStaticMethod(const char* assemblyName, const char* fullClassName, const char* methodName);
        TtLeanClrMethod* FindMethod(const char* assemblyName, const char* fullClassName, const char* methodName);
        TtLeanClrObject* CreateObject(const char* assemblyName, const char* fullClassName);
        bool InvokeWithBuffer(TtLeanClrMethod* method, BYTE* argBuffer, UINT argBufferByteSize, BYTE* retBuffer, UINT retBufferByteSize);
        bool InvokeInstanceWithBuffer(TtLeanClrObject* instance, TtLeanClrMethod* method, BYTE* argBuffer, UINT argBufferByteSize, BYTE* retBuffer, UINT retBufferByteSize);
        bool BindHostObjectClass(const char* assemblyName, const char* fullClassName, const char* fieldName);
        // 含对象头的 handle 字段偏移，未绑定返回 -1
        static INT32 GetHostHandleOffset();
        // System.String 的长度与首字符偏移（含对象头），供宿主直接读取 VM 字符串
        static INT32 GetStringLengthOffset();
        static INT32 GetStringCharsOffset();
        // 由 UTF-16 字符创建 VM 字符串，返回未入根的裸指针，只能在下次 GC 前交给 VM
        void* NewString(const void* utf16Chars, INT32 charCount);
        bool IsInitialized() const;
        // 把 VM 属主切换为当前线程；调用方保证与上一属主线程上的 VM 调用已串行
        bool AcquireOwnerThread();
        // 最近一次失败的描述；VM 抛出托管异常时为 "操作: 异常类型全名: message"
        VNameString GetLastError() const;

    private:
        friend leanclr::core::ResultVoid Titan_LeanclrMethodInvoker(void (*methodPtr)(), const leanclr::metadata::RtMethodInfo* method,
                                                                    const leanclr::interp::RtStackObject* args, leanclr::interp::RtStackObject* ret);
        struct FAssemblyData
        {
            std::vector<BYTE> Dll;
            std::vector<BYTE> Pdb;
        };

        static bool LoadAssemblyBytes(const char* assemblyName, const char* extension, leanclr::vm::FileData& fileData);
        bool SetError(const char* operation, INT32 errorCode = 0);
        bool SetException(const char* operation, LeanclrException* exception);

        std::map<std::string, FAssemblyData> mAssemblies;
        std::set<std::string> mIntrinsicSignatures;
        bool mInitialized = false;
        std::thread::id mOwnerThread;
        std::string mLastError;
        static TtLeanClrRuntime* sActiveRuntime;
        static FLeanClrIntrinsicDispatcher mIntrinsicDispatcher;
    };
}

NS_END

// NativeBinder 生成的全局 extern "C" 导出会以未限定名展开回调参数类型
using EngineNS::LeanCLR::FLeanClrIntrinsicCall;
