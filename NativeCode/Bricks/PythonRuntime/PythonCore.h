#pragma once
#include "../../Base/IUnknown.h"
#include "../../Base/string/vfxstring.h"
#if defined(PLATFORM_WIN)
#include "../../../3rd/native/Python3/Win64/include/Python.h"
#include "../../../3rd/native/Python3/Win64/include/pyconfig.h"
#elif defined(PLATFORM_DROID)
#endif

NS_BEGIN

namespace PythonWrapper
{
	typedef void* FPyObjPtr;
	class FPyTuple;
	class TR_CLASS()
		FPyUtility : public VIUnknown
	{
	public:
		static void InitializePython();
		static int PythonStateEnsure();
		static void PythonStateRelease(int state);
		static void AddPyRef(FPyObjPtr p);
		static void DecPyRef(FPyObjPtr p);
		static FPyObjPtr From(int value);
		static FPyObjPtr From(unsigned int value);
		static FPyObjPtr From(long long value);
		static FPyObjPtr From(unsigned long long value);
		static FPyObjPtr From(double value);
		static FPyObjPtr From(bool value);
		static FPyObjPtr From(const char* value);
		static FPyObjPtr From(const wchar_t* value);

		static int ToInt32(FPyObjPtr obj);
		static unsigned int ToUInt32(FPyObjPtr obj);
		static long long ToInt64(FPyObjPtr obj);
		static unsigned long long ToUInt64(FPyObjPtr obj);
		static double ToDouble(FPyObjPtr obj);
		static bool ToBool(FPyObjPtr obj);
		static std::string ToString(FPyObjPtr obj);
		static std::wstring ToWString(FPyObjPtr obj);

		static FPyObjPtr CallObject(FPyObjPtr callable, FPyTuple* args);
		static int RunPythonString(const char* code);
	};

	class TR_CLASS()
		FPyTuple : public VIUnknown
	{
	public:
		PyObject* mHost;
	public:
		FPyTuple(UINT num)
		{
			mHost = PyTuple_New(num);
		}
		FPyTuple(void* host);
		~FPyTuple();
		int SetItem(UINT index, FPyObjPtr value);
		FPyObjPtr GetItem(UINT index);
	};

	class FPyTypeDefine;
	struct TR_CLASS(SV_LayoutStruct = 8)
		FPyClassWrapper
	{
		PyObject_HEAD
		FPyTypeDefine* Define = nullptr;
		void* UserData = nullptr;
	};

	class TR_CLASS()
		FPyModule : public VIUnknown
	{
		std::string					Name;
		PyObject*					PyModule = nullptr;
	public:
		FPyModule(FPyModule* parent, const char* name);
		~FPyModule();
		void AddType(FPyTypeDefine* type);
		const char* GetName() const {
			return Name.c_str();
		}
	};
	
	class FPyMethodDefine;
	class FPyPropertyDefine;
	class TR_CLASS()
		FPyTypeDefine : public VIUnknown
	{
	public:
		PyTypeObject	TypeObject{};
		FPyModule*		HostModule = nullptr;
		std::string		Name;
		AutoRef<FPyMethodDefine>	MethodDefine;
		AutoRef<FPyPropertyDefine>	PropertyDefine;
		typedef bool FOnNewPyClassWrapper(FPyClassWrapper* pObj, FPyTypeDefine* pDefine);
		static std::function<FOnNewPyClassWrapper>		OnNewPyClassWrapper;
	public:
		FPyTypeDefine(FPyModule * module, const char* name);
		static FPyClassWrapper* AllocPyClassWrapper(void* InType);
		static void FreePyClassWrapper(void* wrapper);
		TR_CALLBACK(SV_CallConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)
			typedef void* (*FNewFunction)(void* InType, void* InArgs, void* InKwds);
		void SetNewFunction(FNewFunction fun) {
			TypeObject.tp_new = (newfunc)fun;
		}
		TR_CALLBACK(SV_CallConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)
			typedef void (*FDeallocFunction)(void* InSelf);
		void SetDeallocFunction(FDeallocFunction fun) {
			TypeObject.tp_dealloc = (destructor)fun;
		}

		void SetMethods(FPyMethodDefine* method);
		void SetProperties(FPyPropertyDefine* props);
	};

	struct FDefExtra : public VIUnknown
	{
		std::string Name;
		std::string Doc;
	};

	class TR_CLASS()
		FPyMethodDefine : public VIUnknown
	{
	public:
		std::vector<PyMethodDef>		Methods;
		std::vector<AutoRef<FDefExtra>>	MethodExtra;

		FPyMethodDefine();
		PyMethodDef* GetInner() {
			if (Methods.size() == 0)
				return nullptr;
			return &Methods[0];
		}
		TR_CALLBACK(SV_CallConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)
			typedef void* (*FPyFunction)(void* , void*);
		void AddMethod(const char* name, FPyFunction func, const char* doc);
	};

	class TR_CLASS()
		FPyPropertyDefine : public VIUnknown
	{
	public:
		std::vector<PyGetSetDef>	Properties;
		FPyPropertyDefine() {}
		PyGetSetDef* GetInner() {
			if (Properties.size() == 0)
				return nullptr;
			return &Properties[0];
		}
	};
}

NS_END