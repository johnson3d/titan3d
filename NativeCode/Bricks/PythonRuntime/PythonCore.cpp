#include "PythonCore.h"

#define new VNEW

#pragma comment(lib, "python3.lib")

NS_BEGIN

namespace PythonWrapper
{
	void FPyUtility::InitializePython()
	{
		Py_Initialize(); 
	}
	int FPyUtility::PythonStateEnsure()
	{
		return PyGILState_Ensure();
	}
	void FPyUtility::PythonStateRelease(int state)
	{
		PyGILState_Release((PyGILState_STATE)state);
	}
	void FPyUtility::AddPyRef(FPyObjPtr p)
	{
		Py_INCREF((PyObject*)p);
	}
	void FPyUtility::DecPyRef(FPyObjPtr p)
	{
		Py_DECREF((PyObject*)p);
	}
	FPyObjPtr FPyUtility::From(int value)
	{
		return PyLong_FromLong(value);
	}
	FPyObjPtr FPyUtility::From(unsigned int value)
	{
		return PyLong_FromUnsignedLong(value);
	}
	FPyObjPtr FPyUtility::From(long long value)
	{
		return PyLong_FromLongLong(value);
	}
	FPyObjPtr FPyUtility::From(unsigned long long value)
	{
		return PyLong_FromUnsignedLongLong(value);
	}
	FPyObjPtr FPyUtility::From(double value)
	{
		return PyFloat_FromDouble(value);
	}
	FPyObjPtr FPyUtility::From(bool value)
	{
		if (value)
		{
			return Py_True;
		}
		else
		{
			return Py_False;
		}
	}
	FPyObjPtr FPyUtility::From(const char* value)
	{
		return PyUnicode_FromString(value);
	}
	FPyObjPtr FPyUtility::From(const wchar_t* value)
	{
		auto n = StringHelper::wstrtostr(value);
		return PyUnicode_FromString(n.c_str());
	}
	int FPyUtility::ToInt32(FPyObjPtr obj)
	{
		return PyLong_AsLong((PyObject*)obj);
	}
	unsigned int FPyUtility::ToUInt32(FPyObjPtr obj)
	{
		return PyLong_AsUnsignedLong((PyObject*)obj);
	}
	long long FPyUtility::ToInt64(FPyObjPtr obj)
	{
		return PyLong_AsLongLong((PyObject*)obj);
	}
	unsigned long long FPyUtility::ToUInt64(FPyObjPtr obj)
	{
		return PyLong_AsUnsignedLongLong((PyObject*)obj);
	}
	double FPyUtility::ToDouble(FPyObjPtr obj)
	{
		return PyFloat_AsDouble((PyObject*)obj);
	}
	bool FPyUtility::ToBool(FPyObjPtr obj)
	{
		if (obj == Py_True)
			return true;
		else if (obj == Py_False)
			return false;

		return false;
	}
	std::string FPyUtility::ToString(FPyObjPtr obj)
	{
		auto PyBytesObj = PyUnicode_AsUTF8String((PyObject*)obj);
		const char* PyUtf8Buffer = PyBytes_AsString(PyBytesObj);
		return PyUtf8Buffer;
	}
	std::wstring FPyUtility::ToWString(FPyObjPtr obj)
	{
		if constexpr (sizeof(wchar_t) == 2)
		{
			auto PyBytesObj = PyUnicode_AsUTF16String((PyObject*)obj);
			wchar_t* PyUtf8Buffer = (wchar_t*)PyBytes_AsString(PyBytesObj);
			return PyUtf8Buffer;
		}
		else if constexpr (sizeof(wchar_t) == 4)
		{
			auto PyBytesObj = PyUnicode_AsUTF32String((PyObject*)obj);
			wchar_t* PyUtf8Buffer = (wchar_t*)PyBytes_AsString(PyBytesObj);
			return PyUtf8Buffer;
		}
		else
		{
			return "";
		}
	}
	FPyObjPtr FPyUtility::CallObject(FPyObjPtr callable, FPyTuple* args)
	{
		return PyObject_CallObject((PyObject*)callable, args->mHost);
	}

	int FPyUtility::RunPythonString(const char* code)
	{
		return PyRun_SimpleString(code);
	}
	FPyTuple::FPyTuple(void* host)
	{
		mHost = (PyObject*)host;
		Py_INCREF(mHost);
	}
	FPyTuple::~FPyTuple()
	{
		Py_DECREF(mHost);
		mHost = nullptr;
	}
	int FPyTuple::SetItem(UINT index, FPyObjPtr value)
	{
		return PyTuple_SetItem(mHost, index, (PyObject*)value);
	}
	FPyObjPtr FPyTuple::GetItem(UINT index)
	{
		return PyTuple_GetItem(mHost, index);
	}
	FPyModule::FPyModule(FPyModule* parent, const char* name)
	{
		Name = name;
		if (parent != nullptr)
		{
			struct PyModuleDef modDesc =
			{
				PyModuleDef_HEAD_INIT,
				Name.c_str(), /* name of module */
				Name.c_str(), /* Doc string (may be NULL) */
				-1, /* Size of per-interpreter state or -1 */
				nullptr /* Method table */
			};
			PyModule = PyModule_Create(&modDesc);
			Py_INCREF(PyModule);

			PyModule_AddObject(parent->PyModule, name, PyModule);
		}
		else
		{
			PyModule = PyImport_AddModule(name);
			Py_INCREF(PyModule);
		}
	}
	FPyModule::~FPyModule()
	{
		Py_DECREF(PyModule);
		PyModule = nullptr;
	}
	void FPyModule::AddType(FPyTypeDefine* type)
	{
		//Py_INCREF(type);
		//PyModule_AddObject(PyModule, type->Name.c_str(), (PyObject*)&type->TypeObject);
		if (PyType_Ready(&type->TypeObject) == 0)
			PyModule_AddType(PyModule, &type->TypeObject);
	}
	static int FPyTypeDefine_Init(PyObject* InSelf, PyObject* InArgs, PyObject* InKwds)
	{
		return 0;
	}
	FPyTypeDefine::FPyTypeDefine(FPyModule* module, const char* name)
	{
		HostModule = module;
		Name = name;

		TypeObject =
			{
				PyVarObject_HEAD_INIT(nullptr, 0)
				Name.c_str(), /* tp_name */
				sizeof(FPyClassWrapper), /* tp_basicsize */
			};
		TypeObject.tp_flags = Py_TPFLAGS_DEFAULT;
		TypeObject.tp_doc = "TitanEngine Python Binder";
		TypeObject.tp_init = (initproc)&FPyTypeDefine_Init;
		//tp_new = (newfunc)&FPyTypeDefine::New;
	}
	FPyClassWrapper* FPyTypeDefine::AllocPyClassWrapper(void* InType)
	{
		auto pObject = (FPyClassWrapper*)((PyTypeObject*)InType)->tp_alloc((PyTypeObject*)InType, 0);
		return pObject;
	}
	void FPyTypeDefine::FreePyClassWrapper(void* wrapper)
	{
		Py_TYPE((PyObject*)wrapper)->tp_free((PyObject*)wrapper);
	}
	void FPyTypeDefine::SetMethods(FPyMethodDefine* method)
	{
		MethodDefine = method;
		TypeObject.tp_methods = MethodDefine->GetInner();
	}
	void FPyTypeDefine::SetProperties(FPyPropertyDefine* props)
	{
		PropertyDefine = props;
		TypeObject.tp_getset = PropertyDefine->GetInner();
	}

	FPyMethodDefine::FPyMethodDefine() 
	{
		PyMethodDef end{};
		Methods.push_back(end);
	}
	void FPyMethodDefine::AddMethod(const char* name, FPyMethodDefine::FPyFunction func, const char* doc)
	{
		auto pData = MakeWeakRef(new FDefExtra());
		pData->Name = name;
		pData->Doc = doc;
		MethodExtra.push_back(pData);
		PyMethodDef tmp{};
		tmp.ml_name = pData->Name.c_str();
		tmp.ml_doc = pData->Doc.c_str();
		tmp.ml_flags = METH_VARARGS;
		tmp.ml_meth = (PyCFunction)(func);
		Methods.insert(Methods.end() - 1, tmp);
	}
}

NS_END