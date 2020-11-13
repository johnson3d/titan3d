#pragma once
#include <string>
#include <map>
#include <vector>
#include <functional>
#if defined(PLATFORM_WIN)
#include <WinSock2.h>
#include <mswsock.h>
#include <windows.h>
#else
#include "../vfxtypes_nw.h"
#endif

typedef unsigned long long	vIID;
typedef int	vBOOL;

#define EngineNS Titan3D
#define NS_BEGIN namespace EngineNS{
#define NS_END }

NS_BEGIN

#define NEW_INHEAD new(__FILE__, __LINE__)

template<typename Type>
inline Type VGetTypeDefault()
{
	return (Type)(0);
}

inline void VGetTypeDefault()
{

}

template <typename R, typename... Args>
struct TFunction_traits_helper
{
	static constexpr auto param_count = sizeof...(Args);
	using return_type = R;

	template <std::size_t N>
	using param_type = std::tuple_element_t<N, std::tuple<Args...>>;
};

template <typename T>
struct TFunction_traits;

template <typename R, typename... Args>
struct TFunction_traits<R(*)(Args...)> : public TFunction_traits_helper<R, Args...>
{
};

template <typename R, typename... Args>
struct TFunction_traits<R(&)(Args...)> : public TFunction_traits_helper<R, Args...>
{
};

template <typename R, typename... Args>
struct TFunction_traits<R(Args...)> : public TFunction_traits_helper<R, Args...>
{
};

template <typename ClassType, typename R, typename... Args>
struct TFunction_traits<R(ClassType::*)(Args...)> : public TFunction_traits_helper<R, Args...>
{
	using class_type = ClassType;
};

template <typename ClassType, typename R, typename... Args>
struct TFunction_traits<R(ClassType::*)(Args...) const> : public TFunction_traits_helper<R, Args...>
{
	using class_type = ClassType;
};

template <typename T>
struct remove_all_ref_ptr { typedef T type; };

template <typename T>
struct remove_all_ref_ptr<const T> : public remove_all_ref_ptr<T> { };

template <typename T>
struct remove_all_ref_ptr<T *> : public remove_all_ref_ptr<T> { };

template <typename T>
struct remove_all_ref_ptr<const T *> : public remove_all_ref_ptr<T> { };

template <typename T>
struct remove_all_ref_ptr<T * const> : public remove_all_ref_ptr<T> { };

template <typename T>
struct remove_all_ref_ptr<const T * const> : public remove_all_ref_ptr<T> { };

template <typename T>
struct remove_all_ref_ptr<T * volatile> : public remove_all_ref_ptr<T> { };

template <typename T>
struct remove_all_ref_ptr<const T * volatile> : public remove_all_ref_ptr<T> { };

template <typename T>
struct remove_all_ref_ptr<const T * const volatile> : public remove_all_ref_ptr<T> { };

template <typename T>
struct remove_all_ref_ptr<T &> : public remove_all_ref_ptr<T> { };

template <typename T>
struct remove_all_ref_ptr<const T &> : public remove_all_ref_ptr<T> { };

template <typename T>
struct remove_all_ref_ptr<T &&> : public remove_all_ref_ptr<T> { };

class VIUnknown;

typedef VIUnknown* (*FConstructor)(const char*, int);

class CoreRtti
{
public:
	CoreRtti(const char* name, const char* super, const vIID& id, 
		unsigned int size, FConstructor fun, const char* file, int line);
	~CoreRtti()
	{

	}
public:
	CoreRtti*				SuperClass;
	unsigned int			Size;
	std::string				ClassName;
	std::string				SuperClassName;
	vIID					ClassId;
	FConstructor			Constructor;
	const char*				AllocFile;
	int						AllocLine;

public:
	unsigned int GetSize() const {
		return Size;
	}
	const char* GetClassName() const {
		return ClassName.c_str();
	}
	const char* GetSuperClassName() const {
		return SuperClassName.c_str();
	}
	vIID GetClassId() const {
		return ClassId;
	}
	VIUnknown* CreateInstance(const char* file, int line) {
		return Constructor(file, line);
	}
};

template<typename Type, bool canNew> 
struct TObjectCreator
{
	static Type* NewObject(const char* file, int line) {
		return new(file, line) Type();
	}
};

template<typename Type>
struct TObjectCreator<Type, false>
{
	static Type* NewObject(const char* file, int line) {
		return nullptr;
	}
};

#define RTTI_DEF(name, id, canNew) static CoreRtti _RttiInfo; \
					 static const vIID __UID__ = id;\
					 static VIUnknown* NewObject(const char* file, int line){ return TObjectCreator<name, canNew>::NewObject(file, line); }\
					 virtual CoreRtti* GetRtti() override{ return &_RttiInfo; }

#define RTTI_IMPL(name, super) CoreRtti name::_RttiInfo(#name, #super, name::__UID__, sizeof(name), &name::NewObject, __FILE__, __LINE__);\
					const vIID name::__UID__;

class CoreRttiManager
{
protected:
	friend CoreRtti;
	std::map<std::string, CoreRtti*>		Classes;
	std::map<vIID, CoreRtti*>				Classes_Id;

	std::vector< std::pair<std::string, CoreRtti*> > AllRttis;
public:
	static CoreRttiManager* GetInstance();

	CoreRttiManager();
	~CoreRttiManager();
	void BuildRtti();
	void Finalize();
	const CoreRtti* FindRtti(const char* name);
	const CoreRtti* FindRtti(vIID id);

	unsigned int GetRttiNumber() {
		return (unsigned int)AllRttis.size();
	}
	CoreRtti* GetRtti(unsigned int index) {
		return AllRttis[index].second;
	}
};

struct RttiMetaInfo
{
	std::map<std::string, std::string> MetaInfos;
	bool HasMeta(const char* info)
	{
		auto iter = MetaInfos.find(info);
		if (iter == MetaInfos.end())
			return false;
		return true;
	}
	void AddMeta(const char* name, const char* value)
	{
		MetaInfos[name] = value;
	}
	std::string GetMetaValue(const char* name)
	{
		auto iter = MetaInfos.find(name);
		if (iter != MetaInfos.end())
			return iter->second;
		return "";
	}
	virtual ~RttiMetaInfo()
	{

	}
};

struct RttiEnum : public RttiMetaInfo
{
	std::string					Name;
	std::string					NameSpace;
	struct MemberDesc : public RttiMetaInfo
	{
		std::string		Name;
		int				Value;
	};
	std::vector<MemberDesc>		Members;

	MemberDesc* PushMember(const char* name, int value)
	{
		MemberDesc tmp;
		tmp.Name = name;
		tmp.Value = value;
		Members.push_back(tmp);
		return &Members[Members.size() - 1];
	}

	const char* GetName() const {
		return Name.c_str();
	}
	const char* GetNameSpace() const {
		return NameSpace.c_str();
	}
	std::string GetFullName() {
		return NameSpace + "::" + Name;
	}
	unsigned int GetMemberNumber() const {
		return (unsigned int)Members.size();
	}
	const char* GetMemberName(unsigned int index) {
		return Members[index].Name.c_str();
	}
	int GetMemberValue(unsigned int index) {
		return Members[index].Value;
	}
	void Init();
};

template<typename Type>
struct AuxRttiEnum : public RttiEnum
{
	static AuxRttiEnum<Type>		Instance;
	static const bool IsEnum = false;
};

//template<typename Type>
//AuxRttiEnum<Type>		AuxRttiEnum<Type>::Instance;

class RttiEnumManager
{
	std::map<std::string, RttiEnum*>	EnumTyps;
	std::vector<RttiEnum*>				AllEnumTyps;
public:
	void RegEnumType(const char* name, RttiEnum* type);
	static RttiEnumManager* GetInstance();

	void BuildRtti();
	void Finalize();

	RttiEnum* FindEnum(const char* name);
	unsigned int GetEnumNumber() {
		return (unsigned int)AllEnumTyps.size();
	}
	RttiEnum* GetEnum(unsigned int index) {
		return AllEnumTyps[index];
	}
};

#define EnumBegin(name) \
template<> \
struct AuxRttiEnum<name> : public RttiEnum \
{ \
	static AuxRttiEnum<name> Instance;\
	static const bool IsEnum = true; \
	AuxRttiEnum() \
	{
		

#define EnumMember(m) PushMember(#m, m);

#define EnumEnd(name, ns) \
		Name = #name; \
		NameSpace = #ns; \
		Init();\
	}\
};\
StructBegin(name, ns)\
	EnumDesc = &AuxRttiEnum<ns::name>::Instance;\
	IsEnum = true;\
StructEnd(void)

#define EnumImpl(name) \
AuxRttiEnum<name> AuxRttiEnum<name>::Instance;\
AuxRttiStruct<name> AuxRttiStruct<name>::Instance;

struct ArgumentStream
{
	ArgumentStream()
		: mReadPosition(0)
	{
	}
	size_t				mReadPosition;
	std::vector<BYTE>	mArguments;
	void Reset()
	{
		mReadPosition = 0;
		mArguments.clear();
	}
};

template <typename ArgType>
inline ArgumentStream& operator <<(ArgumentStream& stream, const ArgType& v)
{
	auto size = stream.mArguments.size();
	stream.mArguments.resize(size + sizeof(ArgType));
	memcpy(&stream.mArguments[size], &v, sizeof(ArgType));
	return stream;
}
template <typename ArgType>
inline ArgumentStream& operator >>(ArgumentStream& stream, ArgType& v)
{
	memcpy(&v, &stream.mArguments[stream.mReadPosition], sizeof(ArgType));
	stream.mReadPosition += sizeof(ArgType);
	return stream;
}

template <>
inline ArgumentStream& operator <<(ArgumentStream& stream, const std::string& v)
{
	auto len = (int)v.length();
	stream << len;
	auto size = stream.mArguments.size();
	stream.mArguments.resize(size + sizeof(char)*len);
	memcpy(&stream.mArguments[size], v.c_str(), sizeof(char)*len);
	return stream;
}

template <>
inline ArgumentStream& operator >>(ArgumentStream& stream, std::string& v)
{
	int len = 0;
	stream >> len;
	v.resize(len);
	memcpy(&v[0], &stream.mArguments[stream.mReadPosition], sizeof(char)*len);
	stream.mReadPosition += sizeof(char)*len;
	return stream;
}

struct RttiStruct;
struct RttiMethodBase : public RttiMetaInfo
{
	RttiMethodBase()
	{
		IsStatic = false;
		IsConst = false;
		ThisObject = nullptr;
		ResultType = nullptr;
	}
	std::string			Name;
	RttiStruct*			ThisObject;

	RttiStruct*			ResultType;
	std::vector<std::pair<std::string,RttiStruct*>>	Arguments;
	bool				IsStatic;
	bool				IsConst;
	virtual void Invoke(void* pThis, ArgumentStream& args, ArgumentStream& result) const
	{
		
	}
	bool MatchArgument(const RttiStruct* returnType, const std::vector<RttiStruct*>& args) const
	{
		if (returnType != ResultType)
			return false;
		if (args.size() != Arguments.size())
			return false;
		for (size_t i = 0; i < args.size(); i++)
		{
			if (args[i] != Arguments[i].second)
				return false;
		}
		return true;
	}
};

struct RttiConstructor : public RttiMetaInfo
{
	std::vector<RttiStruct*> mArguments;
	bool MatchArgument(const std::vector<RttiStruct*>& args) const
	{
		if (args.size() != mArguments.size())
			return false;
		for (size_t i = 0; i < args.size(); i++)
		{
			if (args[i] != mArguments[i])
				return false;
		}
		return true;
	}
	virtual void* CreateInstance(ArgumentStream& args) const
	{
		return nullptr;
	}
};

template<typename Result, typename BindClass>
struct VMethod0 : public RttiMethodBase
{
	typedef Result(*StaticMethodFun)();
	typedef Result(BindClass::*MethodFun)();
	typedef Result(BindClass::*ConstMethodFun)() const;
	VMethod0(MethodFun fun, const char* name);
	VMethod0(StaticMethodFun fun, const char* name);
	VMethod0(ConstMethodFun fun, const char* name)
		: VMethod0((MethodFun)fun, name)
	{
		IsConst = true;
	}
	union
	{
		StaticMethodFun StaticFuncPtr;
		MethodFun FuncPtr;
		//ConstMethodFun ConstFuncPtr;
	};

	virtual void Invoke(void* pThis, ArgumentStream& args, ArgumentStream& result) const override
	{
		InvokeImpl<Result>(pThis, args, result);
	}
private:
	template<typename TR>
	void InvokeImpl(void* pThis, ArgumentStream& args, ArgumentStream& result) const
	{
		if (IsStatic)
		{
			auto ret = (StaticFuncPtr)();
			result << ret;
		}
		else
		{
			auto ret = (((BindClass*)pThis)->*(FuncPtr))();
			result << ret;
		}
	}
	template<>
	void InvokeImpl<void>(void* pThis, ArgumentStream& args, ArgumentStream& result) const
	{
		if (IsStatic)
		{
			(StaticFuncPtr)();
		}
		else
		{
			(((BindClass*)pThis)->*(FuncPtr))();
		}
	}
};

#define Method_Arg_Def(index) \
	T##index a##index = T##index();\
	args >> a##index;

template<typename Result, typename BindClass, typename T0>
struct VMethod1 : public RttiMethodBase
{
	typedef Result(*StaticMethodFun)(T0);
	typedef Result(BindClass::*MethodFun)(T0);
	typedef Result(BindClass::*ConstMethodFun)(T0) const;
	VMethod1(MethodFun fun, const char* name, const char* a0);
	VMethod1(StaticMethodFun fun, const char* name, const char* a0);
	VMethod1(ConstMethodFun fun, const char* name, const char* a0)
		: VMethod1((MethodFun)fun, name, a0)
	{
		IsConst = true;
	}
	union
	{
		StaticMethodFun StaticFuncPtr;
		MethodFun FuncPtr;
	};

	virtual void Invoke(void* pThis, ArgumentStream& args, ArgumentStream& result) const override
	{
		InvokeImpl<Result>(pThis, args, result);
	}

private:
	template<typename TR>
	void InvokeImpl(void* pThis, ArgumentStream& args, ArgumentStream& result) const
	{
		args.mReadPosition = 0;
		Method_Arg_Def(0);
		if (IsStatic)
		{
			auto ret = (StaticFuncPtr)(a0);
			result << ret;
		}
		else
		{
			auto ret = (((BindClass*)pThis)->*(FuncPtr))(a0);
			result << ret;
		}
	}
	template<>
	void InvokeImpl<void>(void* pThis, ArgumentStream& args, ArgumentStream& result) const
	{
		args.mReadPosition = 0;
		Method_Arg_Def(0);
		if (IsStatic)
		{
			(StaticFuncPtr)(a0);
		}
		else
		{
			(((BindClass*)pThis)->*(FuncPtr))(a0);
		}
	}
};

template<typename Result, typename BindClass, typename T0, typename T1>
struct VMethod2 : public RttiMethodBase
{
	typedef Result(*StaticMethodFun)(T0, T1);
	typedef Result(BindClass::*MethodFun)(T0,T1);
	typedef Result(BindClass::*ConstMethodFun)(T0, T1) const;
	VMethod2(MethodFun fun, const char* name, const char* a0, const char* a1);
	VMethod2(StaticMethodFun fun, const char* name, const char* a0, const char* a1);
	VMethod2(ConstMethodFun fun, const char* name, const char* a0, const char* a1)
		: VMethod2((MethodFun)fun, name, a0, a1)
	{
		IsConst = true;
	}
	union
	{
		StaticMethodFun StaticFuncPtr;
		MethodFun FuncPtr;
	};

	virtual void Invoke(void* pThis, ArgumentStream& args, ArgumentStream& result) const override
	{
		InvokeImpl<Result>(pThis, args, result);
	}

private:
	template<typename TR>
	void InvokeImpl(void* pThis, ArgumentStream& args, ArgumentStream& result) const
	{
		args.mReadPosition = 0;
		Method_Arg_Def(0);
		Method_Arg_Def(1);
		if (IsStatic)
		{
			auto ret = (StaticFuncPtr)(a0, a1);
			result << ret;
		}
		else
		{
			auto ret = (((BindClass*)pThis)->*(FuncPtr))(a0,a1);
			result << ret;
		}
	}
	template<>
	void InvokeImpl<void>(void* pThis, ArgumentStream& args, ArgumentStream& result) const
	{
		args.mReadPosition = 0;
		Method_Arg_Def(0);
		Method_Arg_Def(1);
		if (IsStatic)
		{
			(StaticFuncPtr)(a0, a1);
		}
		else
		{
			(((BindClass*)pThis)->*(FuncPtr))(a0, a1);
		}
	}
};

template<typename Result, typename BindClass, typename T0, typename T1, typename T2>
struct VMethod3 : public RttiMethodBase
{
	typedef Result(*StaticMethodFun)(T0, T1, T2);
	typedef Result(BindClass::*MethodFun)(T0, T1, T2);
	typedef Result(BindClass::*ConstMethodFun)(T0, T1, T2) const;
	VMethod3(MethodFun fun, const char* name, const char* a0, const char* a1, const char* a2);
	VMethod3(StaticMethodFun fun, const char* name, const char* a0, const char* a1, const char* a2);
	VMethod3(ConstMethodFun fun, const char* name, const char* a0, const char* a1, const char* a2)
		: VMethod3((MethodFun)fun, name, a0, a1, a2)
	{
		IsConst = true;
	}
	union
	{
		StaticMethodFun StaticFuncPtr;
		MethodFun FuncPtr;
	};

	virtual void Invoke(void* pThis, ArgumentStream& args, ArgumentStream& result) const override
	{
		InvokeImpl<Result>(pThis, args, result);
	}

private:
	template<typename TR>
	void InvokeImpl(void* pThis, ArgumentStream& args, ArgumentStream& result) const
	{
		args.mReadPosition = 0;
		Method_Arg_Def(0);
		Method_Arg_Def(1);
		Method_Arg_Def(2);
		if (IsStatic)
		{
			auto ret = (StaticFuncPtr)(a0, a1, a2);
			result << ret;
		}
		else
		{
			auto ret = (((BindClass*)pThis)->*(FuncPtr))(a0, a1, a2);
			result << ret;
		}
	}
	template<>
	void InvokeImpl<void>(void* pThis, ArgumentStream& args, ArgumentStream& result) const
	{
		args.mReadPosition = 0;
		Method_Arg_Def(0);
		Method_Arg_Def(1);
		Method_Arg_Def(2);
		if (IsStatic)
		{
			(StaticFuncPtr)(a0, a1, a2);
		}
		else
		{
			(((BindClass*)pThis)->*(FuncPtr))(a0, a1, a2);
		}
	}
};

template<typename Result, typename BindClass, typename T0, typename T1, typename T2, typename T3>
struct VMethod4 : public RttiMethodBase
{
	typedef Result(*StaticMethodFun)(T0, T1, T2, T3);
	typedef Result(BindClass::*MethodFun)(T0, T1, T2, T3);
	typedef Result(BindClass::*ConstMethodFun)(T0, T1, T2, T3) const;
	VMethod4(MethodFun fun, const char* name, const char* a0, const char* a1, const char* a2, const char* a3);
	VMethod4(StaticMethodFun fun, const char* name, const char* a0, const char* a1, const char* a2, const char* a3);
	VMethod4(ConstMethodFun fun, const char* name, const char* a0, const char* a1, const char* a2, const char* a3)
		: VMethod4((MethodFun)fun, name, a0, a1, a2, a3)
	{
		IsConst = true;
	}
	union
	{
		StaticMethodFun StaticFuncPtr;
		MethodFun FuncPtr;
	};

	virtual void Invoke(void* pThis, ArgumentStream& args, ArgumentStream& result) const override
	{
		InvokeImpl<Result>(pThis, args, result);
	}

private:
	template<typename TR>
	void InvokeImpl(void* pThis, ArgumentStream& args, ArgumentStream& result) const
	{
		args.mReadPosition = 0;
		Method_Arg_Def(0);
		Method_Arg_Def(1);
		Method_Arg_Def(2);
		Method_Arg_Def(3);
		if (IsStatic)
		{
			auto ret = (StaticFuncPtr)(a0, a1, a2, a3);
			result << ret;
		}
		else
		{
			auto ret = (((BindClass*)pThis)->*(FuncPtr))(a0, a1, a2, a3);
			result << ret;
		}
	}
	template<>
	void InvokeImpl<void>(void* pThis, ArgumentStream& args, ArgumentStream& result) const
	{
		args.mReadPosition = 0;
		Method_Arg_Def(0);
		Method_Arg_Def(1);
		Method_Arg_Def(2);
		Method_Arg_Def(3);
		if (IsStatic)
		{
			(StaticFuncPtr)(a0, a1, a2, a3);
		}
		else
		{
			(((BindClass*)pThis)->*(FuncPtr))(a0, a1, a2, a3);
		}
	}
};

template<typename Result, typename BindClass, typename T0, typename T1, typename T2, typename T3, typename T4>
struct VMethod5 : public RttiMethodBase
{
	typedef Result(*StaticMethodFun)(T0, T1, T2, T3, T4);
	typedef Result(BindClass::*MethodFun)(T0, T1, T2, T3, T4);
	typedef Result(BindClass::*ConstMethodFun)(T0, T1, T2, T3, T4) const;
	VMethod5(MethodFun fun, const char* name, const char* a0, const char* a1, const char* a2, const char* a3, const char* a4);
	VMethod5(StaticMethodFun fun, const char* name, const char* a0, const char* a1, const char* a2, const char* a3, const char* a4);
	VMethod5(ConstMethodFun fun, const char* name, const char* a0, const char* a1, const char* a2, const char* a3, const char* a4)
		: VMethod5((MethodFun)fun, name, a0, a1, a2, a3, a4)
	{
		IsConst = true;
	}
	union
	{
		StaticMethodFun StaticFuncPtr;
		MethodFun FuncPtr;
	};

	virtual void Invoke(void* pThis, ArgumentStream& args, ArgumentStream& result) const override
	{
		InvokeImpl<Result>(pThis, args, result);
	}

private:
	template<typename TR>
	void InvokeImpl(void* pThis, ArgumentStream& args, ArgumentStream& result) const
	{
		args.mReadPosition = 0;
		Method_Arg_Def(0);
		Method_Arg_Def(1);
		Method_Arg_Def(2);
		Method_Arg_Def(3);
		Method_Arg_Def(4);
		if (IsStatic)
		{
			auto ret = (StaticFuncPtr)(a0, a1, a2, a3, a4);
			result << ret;
		}
		else
		{
			auto ret = (((BindClass*)pThis)->*(FuncPtr))(a0, a1, a2, a3, a4);
			result << ret;
		}
	}
	template<>
	void InvokeImpl<void>(void* pThis, ArgumentStream& args, ArgumentStream& result) const
	{
		args.mReadPosition = 0;
		Method_Arg_Def(0);
		Method_Arg_Def(1);
		Method_Arg_Def(2);
		Method_Arg_Def(3);
		Method_Arg_Def(4);
		if (IsStatic)
		{
			(StaticFuncPtr)(a0, a1, a2, a3, a4);
		}
		else
		{
			(((BindClass*)pThis)->*(FuncPtr))(a0, a1, a2, a3, a4);
		}
	}
};

template<typename Result, typename BindClass, typename T0, typename T1, typename T2, typename T3, typename T4, typename T5>
struct VMethod6 : public RttiMethodBase
{
	typedef Result(*StaticMethodFun)(T0, T1, T2, T3, T4, T5);
	typedef Result(BindClass::*MethodFun)(T0, T1, T2, T3, T4, T5);
	typedef Result(BindClass::*ConstMethodFun)(T0, T1, T2, T3, T4, T5) const;
	VMethod6(MethodFun fun, const char* name, const char* a0, const char* a1, const char* a2, const char* a3, const char* a4, const char* a5);
	VMethod6(StaticMethodFun fun, const char* name, const char* a0, const char* a1, const char* a2, const char* a3, const char* a4, const char* a5);
	VMethod6(ConstMethodFun fun, const char* name, const char* a0, const char* a1, const char* a2, const char* a3, const char* a4, const char* a5)
		: VMethod6((MethodFun)fun, name, a0, a1, a2, a3, a4, a5)
	{
		IsConst = true;
	}
	union
	{
		StaticMethodFun StaticFuncPtr;
		MethodFun FuncPtr;
	};

	virtual void Invoke(void* pThis, ArgumentStream& args, ArgumentStream& result) const override
	{
		InvokeImpl<Result>(pThis, args, result);
	}

private:
	template<typename TR>
	void InvokeImpl(void* pThis, ArgumentStream& args, ArgumentStream& result) const
	{
		args.mReadPosition = 0;
		Method_Arg_Def(0);
		Method_Arg_Def(1);
		Method_Arg_Def(2);
		Method_Arg_Def(3);
		Method_Arg_Def(4);
		Method_Arg_Def(5);
		if (IsStatic)
		{
			auto ret = (StaticFuncPtr)(a0, a1, a2, a3, a4, a5);
			result << ret;
		}
		else
		{
			auto ret = (((BindClass*)pThis)->*(FuncPtr))(a0, a1, a2, a3, a4, a5);
			result << ret;
		}
	}
	template<>
	void InvokeImpl<void>(void* pThis, ArgumentStream& args, ArgumentStream& result) const
	{
		args.mReadPosition = 0;
		Method_Arg_Def(0);
		Method_Arg_Def(1);
		Method_Arg_Def(2);
		Method_Arg_Def(3);
		Method_Arg_Def(4);
		Method_Arg_Def(5);
		if (IsStatic)
		{
			(StaticFuncPtr)(a0, a1, a2, a3, a4, a5);
		}
		else
		{
			(((BindClass*)pThis)->*(FuncPtr))(a0, a1, a2, a3, a4, a5);
		}
	}
};

template<typename Result, typename BindClass, typename T0, typename T1, typename T2, typename T3, typename T4, typename T5, typename T6>
struct VMethod7 : public RttiMethodBase
{
	typedef Result(*StaticMethodFun)(T0, T1, T2, T3, T4, T5, T6);
	typedef Result(BindClass::*MethodFun)(T0, T1, T2, T3, T4, T5, T6);
	typedef Result(BindClass::*ConstMethodFun)(T0, T1, T2, T3, T4, T5, T6) const;
	VMethod7(MethodFun fun, const char* name, const char* a0, const char* a1, const char* a2, const char* a3, const char* a4, const char* a5, const char* a6);
	VMethod7(StaticMethodFun fun, const char* name, const char* a0, const char* a1, const char* a2, const char* a3, const char* a4, const char* a5, const char* a6);
	VMethod7(ConstMethodFun fun, const char* name, const char* a0, const char* a1, const char* a2, const char* a3, const char* a4, const char* a5, const char* a6)
		: VMethod7((MethodFun)fun, name, a0, a1, a2, a3, a4, a5, a6)
	{
		IsStatic = true;
	}
	union
	{
		StaticMethodFun StaticFuncPtr;
		MethodFun FuncPtr;
	};

	virtual void Invoke(void* pThis, ArgumentStream& args, ArgumentStream& result) const override
	{
		InvokeImpl<Result>(pThis, args, result);
	}

private:
	template<typename TR>
	void InvokeImpl(void* pThis, ArgumentStream& args, ArgumentStream& result) const
	{
		args.mReadPosition = 0;
		Method_Arg_Def(0);
		Method_Arg_Def(1);
		Method_Arg_Def(2);
		Method_Arg_Def(3);
		Method_Arg_Def(4);
		Method_Arg_Def(5);
		Method_Arg_Def(6);
		if (IsStatic)
		{
			auto ret = (StaticFuncPtr)(a0, a1, a2, a3, a4, a5, a6);
			result << ret;
		}
		else
		{
			auto ret = (((BindClass*)pThis)->*(FuncPtr))(a0, a1, a2, a3, a4, a5, a6);
			result << ret;
		}
	}
	template<>
	void InvokeImpl<void>(void* pThis, ArgumentStream& args, ArgumentStream& result) const
	{
		args.mReadPosition = 0;
		Method_Arg_Def(0);
		Method_Arg_Def(1);
		Method_Arg_Def(2);
		Method_Arg_Def(3);
		Method_Arg_Def(4);
		Method_Arg_Def(5);
		Method_Arg_Def(6);
		if (IsStatic)
		{
			(StaticFuncPtr)(a0, a1, a2, a3, a4, a5, a6);
		}
		else
		{
			(((BindClass*)pThis)->*(FuncPtr))(a0, a1, a2, a3, a4, a5, a6);
		}
	}
};

template<typename Result, typename BindClass, typename T0, typename T1, typename T2, typename T3, typename T4, typename T5, typename T6, typename T7>
struct VMethod8 : public RttiMethodBase
{
	typedef Result(*StaticMethodFun)(T0, T1, T2, T3, T4, T5, T6, T7);
	typedef Result(BindClass::*MethodFun)(T0, T1, T2, T3, T4, T5, T6, T7);
	typedef Result(BindClass::*ConstMethodFun)(T0, T1, T2, T3, T4, T5, T6, T7) const;
	VMethod8(MethodFun fun, const char* name, const char* a0, const char* a1, const char* a2, const char* a3, const char* a4, const char* a5, const char* a6, const char* a7);
	VMethod8(StaticMethodFun fun, const char* name, const char* a0, const char* a1, const char* a2, const char* a3, const char* a4, const char* a5, const char* a6, const char* a7);
	VMethod8(ConstMethodFun fun, const char* name, const char* a0, const char* a1, const char* a2, const char* a3, const char* a4, const char* a5, const char* a6, const char* a7)
		: VMethod8((MethodFun)fun, name, a0, a1, a2, a3, a4, a5, a6, a7)
	{

	}
	union
	{
		StaticMethodFun StaticFuncPtr;
		MethodFun FuncPtr;
	};

	virtual void Invoke(void* pThis, ArgumentStream& args, ArgumentStream& result) const override
	{
		InvokeImpl<Result>(pThis, args, result);
	}

private:
	template<typename TR>
	void InvokeImpl(void* pThis, ArgumentStream& args, ArgumentStream& result) const
	{
		args.mReadPosition = 0;
		Method_Arg_Def(0);
		Method_Arg_Def(1);
		Method_Arg_Def(2);
		Method_Arg_Def(3);
		Method_Arg_Def(4);
		Method_Arg_Def(5);
		Method_Arg_Def(6);
		Method_Arg_Def(7);
		if (IsStatic)
		{
			auto ret = (StaticFuncPtr)(a0, a1, a2, a3, a4, a5, a6, a7);
			result << ret;
		}
		else
		{
			auto ret = (((BindClass*)pThis)->*(FuncPtr))(a0, a1, a2, a3, a4, a5, a6, a7);
			result << ret;
		}
	}
	template<>
	void InvokeImpl<void>(void* pThis, ArgumentStream& args, ArgumentStream& result) const
	{
		args.mReadPosition = 0;
		Method_Arg_Def(0);
		Method_Arg_Def(1);
		Method_Arg_Def(2);
		Method_Arg_Def(3);
		Method_Arg_Def(4);
		Method_Arg_Def(5);
		Method_Arg_Def(6);
		Method_Arg_Def(7);
		if (IsStatic)
		{
			(StaticFuncPtr)(a0, a1, a2, a3, a4, a5, a6, a7);
		}
		else
		{
			(((BindClass*)pThis)->*(FuncPtr))(a0, a1, a2, a3, a4, a5, a6, a7);
		}
	}
};

template<typename Result, typename BindClass, typename T0, typename T1, typename T2, typename T3, typename T4, typename T5, typename T6, typename T7, typename T8>
struct VMethod9 : public RttiMethodBase
{
	typedef Result(*StaticMethodFun)(T0, T1, T2, T3, T4, T5, T6, T7, T8);
	typedef Result(BindClass::*MethodFun)(T0, T1, T2, T3, T4, T5, T6, T7, T8);
	typedef Result(BindClass::*ConstMethodFun)(T0, T1, T2, T3, T4, T5, T6, T7, T8) const;
	VMethod9(MethodFun fun, const char* name, const char* a0, const char* a1, const char* a2, const char* a3, const char* a4, const char* a5, const char* a6, const char* a7, const char* a8);
	VMethod9(StaticMethodFun fun, const char* name, const char* a0, const char* a1, const char* a2, const char* a3, const char* a4, const char* a5, const char* a6, const char* a7, const char* a8);
	VMethod9(ConstMethodFun fun, const char* name, const char* a0, const char* a1, const char* a2, const char* a3, const char* a4, const char* a5, const char* a6, const char* a7, const char* a8)
		: VMethod9((MethodFun)fun, name, a0, a1, a2, a3, a4, a5, a6, a7, a8)
	{

	}
	union
	{
		StaticMethodFun StaticFuncPtr;
		MethodFun FuncPtr;
	};

	virtual void Invoke(void* pThis, ArgumentStream& args, ArgumentStream& result) const override
	{
		InvokeImpl<Result>(pThis, args, result);
	}

private:
	template<typename TR>
	void InvokeImpl(void* pThis, ArgumentStream& args, ArgumentStream& result) const
	{
		args.mReadPosition = 0;
		Method_Arg_Def(0);
		Method_Arg_Def(1);
		Method_Arg_Def(2);
		Method_Arg_Def(3);
		Method_Arg_Def(4);
		Method_Arg_Def(5);
		Method_Arg_Def(6);
		Method_Arg_Def(7);
		Method_Arg_Def(8);
		if (IsStatic)
		{
			auto ret = (StaticFuncPtr)(a0, a1, a2, a3, a4, a5, a6, a7, a8);
			result << ret;
		}
		else
		{
			auto ret = (((BindClass*)pThis)->*(FuncPtr))(a0, a1, a2, a3, a4, a5, a6, a7, a8);
			result << ret;
		}
	}
	template<>
	void InvokeImpl<void>(void* pThis, ArgumentStream& args, ArgumentStream& result) const
	{
		args.mReadPosition = 0;
		Method_Arg_Def(0);
		Method_Arg_Def(1);
		Method_Arg_Def(2);
		Method_Arg_Def(3);
		Method_Arg_Def(4);
		Method_Arg_Def(5);
		Method_Arg_Def(6);
		Method_Arg_Def(7);
		Method_Arg_Def(8);
		if (IsStatic)
		{
			(StaticFuncPtr)(a0, a1, a2, a3, a4, a5, a6, a7, a8);
		}
		else
		{
			(((BindClass*)pThis)->*(FuncPtr))(a0, a1, a2, a3, a4, a5, a6, a7, a8);
		}
	}
};

template<typename Klass>
struct VConstructor0 : public RttiConstructor
{
	virtual void* CreateInstance(ArgumentStream& args) const
	{
		return new Klass();
	}
};

template<typename Klass, typename T0>
struct VConstructor1 : public RttiConstructor
{
	VConstructor1();
	virtual void* CreateInstance(ArgumentStream& args) const
	{	
		args.mReadPosition = 0;
		Method_Arg_Def(0);
		return new Klass(a0);
	}
};

template<typename Klass, typename T0, typename T1>
struct VConstructor2 : public RttiConstructor
{
	VConstructor2();
	virtual void* CreateInstance(ArgumentStream& args) const
	{
		Method_Arg_Def(0);
		Method_Arg_Def(1);
		return new Klass(a0, a1);
	}
};

struct RttiStruct : public RttiMetaInfo
{
	RttiStruct*					ParentStructType;
	std::string					Name;
	std::string					NameSpace;
	unsigned int				Size;
	RttiEnum*					EnumDesc;
	bool						IsEnum;
	struct MemberDesc : public RttiMetaInfo
	{
		RttiStruct*			MemberType;
		unsigned int		Offset;
		unsigned int		Size;
		unsigned int		ArrayElements;
		std::string			MemberName;
		bool				IsPointer;

		template<typename T>
		inline void SetValue(void* pThis, const T* v) const;
		template<typename T>
		inline T* GetValueAddress(void* pThis) const;
	};
	std::vector<MemberDesc>			Members;
	std::vector<RttiMethodBase*>	Methods;
	std::vector<RttiConstructor*>	Constructors;
	RttiStruct()
	{
		IsEnum = false;
		EnumDesc = nullptr;
		ParentStructType = nullptr;
	}
	virtual ~RttiStruct()
	{
		Cleanup();
	}
	void Cleanup()
	{
		for (auto i : Methods)
		{
			delete i;
		}
		Methods.clear();

		for (auto i : Constructors)
		{
			delete i;
		}
		Constructors.clear();
	}
	bool IsA(RttiStruct* pTar);
	virtual void AssignOperator(void* pTar, const void* pSrc) const
	{
		memcpy(pTar, pSrc, Size);
	}
	vBOOL GetIsEnum() const {
		return IsEnum?1:0;
	}
	const char* GetName() const {
		return Name.c_str();
	}
	const char* GetNameSpace() const {
		return NameSpace.c_str();
	}
	std::string GetFullName() {
		return NameSpace + "::" + Name;
	}
	unsigned int GetSize() const {
		return Size;
	}
	unsigned int GetMemberNumber() const {
		return (unsigned int)Members.size();
	}
	unsigned int FindMemberIndex(const char* name) const{
		for (unsigned int i = 0; i < (unsigned int)Members.size(); i++)
		{
			if (Members[i].MemberName == name)
				return i;
		}
		return -1;
	}
	RttiStruct* GetMemberType(unsigned int index) const {
		return Members[index].MemberType;
	}
	const char* GetMemberName(unsigned int index) const {
		return Members[index].MemberName.c_str();
	}
	const MemberDesc* FindMember(const char* name) const {
		for (const auto& i : Members)
		{
			if (i.MemberName == name)
				return &i;
		}
		return nullptr;
	}
	unsigned int GetMethodNumber() const {
		return (unsigned int)Methods.size();
	}
	unsigned int FindMethodIndex(const char* name) const{
		for (unsigned int i = 0; i < (unsigned int)Methods.size(); i++)
		{
			if (Methods[i]->Name == name)
				return i;
		}
		return -1;
	}
	const RttiMethodBase* FindMethod(const char* name) const{
		for (auto i : Methods)
		{
			if (i->Name == name)
				return i;
		}
		return nullptr;
	}
	const RttiMethodBase* FindMethodWithArguments(const char* name, const RttiStruct* returnType, const std::vector<RttiStruct*>& args) const {
		for (auto i : Methods)
		{
			if (i->Name == name && i->MatchArgument(returnType, args))
			{
				return i;
			}
		}
		return nullptr;
	}
	const RttiConstructor* FindConstructor(const std::vector<RttiStruct*>& args) const
	{
		for (auto i : Constructors)
		{
			if (i->MatchArgument(args))
				return i;
		}
		return nullptr;
	}
	MemberDesc* PushMember(RttiStruct* type, unsigned int offset, unsigned int size, unsigned int arrayElements, const char* name, bool isPointer);

	template<typename Result, typename Klass>
	RttiMethodBase* PushMethod0(typename VMethod0<Result, Klass>::MethodFun fun, const char* name)
	{
		auto desc = NEW_INHEAD VMethod0<Result, Klass>(fun, name);
		
		Methods.push_back(desc);
		return desc;
	}
	template<typename Result, typename Klass>
	RttiMethodBase* PushMethod0(typename VMethod0<Result, Klass>::ConstMethodFun fun, const char* name)
	{
		auto desc = NEW_INHEAD VMethod0<Result, Klass>(fun, name);

		Methods.push_back(desc);
		return desc;
	}
	template<typename Result, typename Klass>
	RttiMethodBase* PushMethod0(typename VMethod0<Result, Klass>::StaticMethodFun fun, const char* name)
	{
		auto desc = NEW_INHEAD VMethod0<Result, Klass>(fun, name);

		Methods.push_back(desc);
		return desc;
	}

	template<typename Result, typename Klass, typename T0>
	RttiMethodBase* PushMethod1(typename VMethod1<Result, Klass, T0>::MethodFun fun, const char* name, const char* a0)
	{
		auto desc = NEW_INHEAD VMethod1<Result, Klass, T0>(fun, name, a0);

		Methods.push_back(desc);
		return desc;
	}
	template<typename Result, typename Klass, typename T0>
	RttiMethodBase* PushMethod1(typename VMethod1<Result, Klass, T0>::ConstMethodFun fun, const char* name, const char* a0)
	{
		auto desc = NEW_INHEAD VMethod1<Result, Klass, T0>(fun, name, a0);

		Methods.push_back(desc);
		return desc;
	}
	template<typename Result, typename Klass, typename T0>
	RttiMethodBase* PushMethod1(typename VMethod1<Result, Klass, T0>::StaticMethodFun fun, const char* name, const char* a0)
	{
		auto desc = NEW_INHEAD VMethod1<Result, Klass, T0>(fun, name, a0);

		Methods.push_back(desc);
		return desc;
	}

	template<typename Result, typename Klass, typename T0, typename T1>
	RttiMethodBase* PushMethod2(typename VMethod2<Result, Klass, T0, T1>::MethodFun fun, const char* name, const char* a0, const char* a1)
	{
		auto desc = NEW_INHEAD VMethod2<Result, Klass, T0, T1>(fun, name, a0, a1);

		Methods.push_back(desc);
		return desc;
	}
	template<typename Result, typename Klass, typename T0, typename T1>
	RttiMethodBase* PushMethod2(typename VMethod2<Result, Klass, T0, T1>::ConstMethodFun fun, const char* name, const char* a0, const char* a1)
	{
		auto desc = NEW_INHEAD VMethod2<Result, Klass, T0, T1>(fun, name, a0, a1);

		Methods.push_back(desc);
		return desc;
	}
	template<typename Result, typename Klass, typename T0, typename T1>
	RttiMethodBase* PushMethod2(typename VMethod2<Result, Klass, T0, T1>::StaticMethodFun fun, const char* name, const char* a0, const char* a1)
	{
		auto desc = NEW_INHEAD VMethod2<Result, Klass, T0, T1>(fun, name, a0, a1);

		Methods.push_back(desc);
		return desc;
	}

	template<typename Result, typename Klass, typename T0, typename T1, typename T2>
	RttiMethodBase* PushMethod3(typename VMethod3<Result, Klass, T0, T1, T2>::MethodFun fun, const char* name, const char* a0, const char* a1, const char* a2)
	{
		auto desc = NEW_INHEAD VMethod3<Result, Klass, T0, T1, T2>(fun, name, a0, a1, a2);

		Methods.push_back(desc);
		return desc;
	}
	template<typename Result, typename Klass, typename T0, typename T1, typename T2>
	RttiMethodBase* PushMethod3(typename VMethod3<Result, Klass, T0, T1, T2>::ConstMethodFun fun, const char* name, const char* a0, const char* a1, const char* a2)
	{
		auto desc = NEW_INHEAD VMethod3<Result, Klass, T0, T1, T2>(fun, name, a0, a1, a2);

		Methods.push_back(desc);
		return desc;
	}
	template<typename Result, typename Klass, typename T0, typename T1, typename T2>
	RttiMethodBase* PushMethod3(typename VMethod3<Result, Klass, T0, T1, T2>::StaticMethodFun fun, const char* name, const char* a0, const char* a1, const char* a2)
	{
		auto desc = NEW_INHEAD VMethod3<Result, Klass, T0, T1, T2>(fun, name, a0, a1, a2);

		Methods.push_back(desc);
		return desc;
	}

	template<typename Result, typename Klass, typename T0, typename T1, typename T2, typename T3>
	RttiMethodBase* PushMethod4(typename VMethod4<Result, Klass, T0, T1, T2, T3>::MethodFun fun, const char* name, const char* a0, const char* a1, const char* a2, const char* a3)
	{
		auto desc = NEW_INHEAD VMethod4<Result, Klass, T0, T1, T2, T3>(fun, name, a0, a1, a2, a3);

		Methods.push_back(desc);
		return desc;
	}
	template<typename Result, typename Klass, typename T0, typename T1, typename T2, typename T3>
	RttiMethodBase* PushMethod4(typename VMethod4<Result, Klass, T0, T1, T2, T3>::ConstMethodFun fun, const char* name, const char* a0, const char* a1, const char* a2, const char* a3)
	{
		auto desc = NEW_INHEAD VMethod4<Result, Klass, T0, T1, T2, T3>(fun, name, a0, a1, a2, a3);

		Methods.push_back(desc);
		return desc;
	}
	template<typename Result, typename Klass, typename T0, typename T1, typename T2, typename T3>
	RttiMethodBase* PushMethod4(typename VMethod4<Result, Klass, T0, T1, T2, T3>::StaticMethodFun fun, const char* name, const char* a0, const char* a1, const char* a2, const char* a3)
	{
		auto desc = NEW_INHEAD VMethod4<Result, Klass, T0, T1, T2, T3>(fun, name, a0, a1, a2, a3);

		Methods.push_back(desc);
		return desc;
	}

	template<typename Result, typename Klass, typename T0, typename T1, typename T2, typename T3, typename T4>
	RttiMethodBase* PushMethod5(typename VMethod5<Result, Klass, T0, T1, T2, T3, T4>::MethodFun fun, const char* name, const char* a0, const char* a1, const char* a2, const char* a3, const char* a4)
	{
		auto desc = NEW_INHEAD VMethod5<Result, Klass, T0, T1, T2, T3, T4>(fun, name, a0, a1, a2, a3, a4);

		Methods.push_back(desc);
		return desc;
	}
	template<typename Result, typename Klass, typename T0, typename T1, typename T2, typename T3, typename T4>
	RttiMethodBase* PushMethod5(typename VMethod5<Result, Klass, T0, T1, T2, T3, T4>::ConstMethodFun fun, const char* name, const char* a0, const char* a1, const char* a2, const char* a3, const char* a4)
	{
		auto desc = NEW_INHEAD VMethod5<Result, Klass, T0, T1, T2, T3, T4>(fun, name, a0, a1, a2, a3, a4);

		Methods.push_back(desc);
		return desc;
	}
	template<typename Result, typename Klass, typename T0, typename T1, typename T2, typename T3, typename T4>
	RttiMethodBase* PushMethod5(typename VMethod5<Result, Klass, T0, T1, T2, T3, T4>::StaticMethodFun fun, const char* name, const char* a0, const char* a1, const char* a2, const char* a3, const char* a4)
	{
		auto desc = NEW_INHEAD VMethod5<Result, Klass, T0, T1, T2, T3, T4>(fun, name, a0, a1, a2, a3, a4);

		Methods.push_back(desc);
		return desc;
	}

	template<typename Result, typename Klass, typename T0, typename T1, typename T2, typename T3, typename T4, typename T5>
	RttiMethodBase* PushMethod6(typename VMethod6<Result, Klass, T0, T1, T2, T3, T4, T5>::MethodFun fun, const char* name, const char* a0, const char* a1, const char* a2, const char* a3, const char* a4, const char* a5)
	{
		auto desc = NEW_INHEAD VMethod6<Result, Klass, T0, T1, T2, T3, T4, T5>(fun, name, a0, a1, a2, a3, a4, a5);

		Methods.push_back(desc);
		return desc;
	}
	template<typename Result, typename Klass, typename T0, typename T1, typename T2, typename T3, typename T4, typename T5>
	RttiMethodBase* PushMethod6(typename VMethod6<Result, Klass, T0, T1, T2, T3, T4, T5>::ConstMethodFun fun, const char* name, const char* a0, const char* a1, const char* a2, const char* a3, const char* a4, const char* a5)
	{
		auto desc = NEW_INHEAD VMethod6<Result, Klass, T0, T1, T2, T3, T4, T5>(fun, name, a0, a1, a2, a3, a4, a5);

		Methods.push_back(desc);
		return desc;
	}
	template<typename Result, typename Klass, typename T0, typename T1, typename T2, typename T3, typename T4, typename T5>
	RttiMethodBase* PushMethod6(typename VMethod6<Result, Klass, T0, T1, T2, T3, T4, T5>::StaticMethodFun fun, const char* name, const char* a0, const char* a1, const char* a2, const char* a3, const char* a4, const char* a5)
	{
		auto desc = NEW_INHEAD VMethod6<Result, Klass, T0, T1, T2, T3, T4, T5>(fun, name, a0, a1, a2, a3, a4, a5);

		Methods.push_back(desc);
		return desc;
	}

	template<typename Result, typename Klass, typename T0, typename T1, typename T2, typename T3, typename T4, typename T5, typename T6>
	RttiMethodBase* PushMethod7(typename VMethod7<Result, Klass, T0, T1, T2, T3, T4, T5, T6>::MethodFun fun, const char* name, const char* a0, const char* a1, const char* a2, const char* a3, const char* a4, const char* a5, const char* a6)
	{
		auto desc = NEW_INHEAD VMethod7<Result, Klass, T0, T1, T2, T3, T4, T5, T6>(fun, name, a0, a1, a2, a3, a4, a5, a6);

		Methods.push_back(desc);
		return desc;
	}
	template<typename Result, typename Klass, typename T0, typename T1, typename T2, typename T3, typename T4, typename T5, typename T6>
	RttiMethodBase* PushMethod7(typename VMethod7<Result, Klass, T0, T1, T2, T3, T4, T5, T6>::ConstMethodFun fun, const char* name, const char* a0, const char* a1, const char* a2, const char* a3, const char* a4, const char* a5, const char* a6)
	{
		auto desc = NEW_INHEAD VMethod7<Result, Klass, T0, T1, T2, T3, T4, T5, T6>(fun, name, a0, a1, a2, a3, a4, a5, a6);

		Methods.push_back(desc);
		return desc;
	}
	template<typename Result, typename Klass, typename T0, typename T1, typename T2, typename T3, typename T4, typename T5, typename T6>
	RttiMethodBase* PushMethod7(typename VMethod7<Result, Klass, T0, T1, T2, T3, T4, T5, T6>::StaticMethodFun fun, const char* name, const char* a0, const char* a1, const char* a2, const char* a3, const char* a4, const char* a5, const char* a6)
	{
		auto desc = NEW_INHEAD VMethod7<Result, Klass, T0, T1, T2, T3, T4, T5, T6>(fun, name, a0, a1, a2, a3, a4, a5, a6);

		Methods.push_back(desc);
		return desc;
	}

	template<typename Result, typename Klass, typename T0, typename T1, typename T2, typename T3, typename T4, typename T5, typename T6, typename T7>
	RttiMethodBase* PushMethod8(typename VMethod8<Result, Klass, T0, T1, T2, T3, T4, T5, T6, T7>::MethodFun fun, const char* name, const char* a0, const char* a1, const char* a2, const char* a3, const char* a4, const char* a5, const char* a6, const char* a7)
	{
		auto desc = NEW_INHEAD VMethod8<Result, Klass, T0, T1, T2, T3, T4, T5, T6, T7>(fun, name, a0, a1, a2, a3, a4, a5, a6, a7);

		Methods.push_back(desc);
		return desc;
	}
	template<typename Result, typename Klass, typename T0, typename T1, typename T2, typename T3, typename T4, typename T5, typename T6, typename T7>
	RttiMethodBase* PushMethod8(typename VMethod8<Result, Klass, T0, T1, T2, T3, T4, T5, T6, T7>::ConstMethodFun fun, const char* name, const char* a0, const char* a1, const char* a2, const char* a3, const char* a4, const char* a5, const char* a6, const char* a7)
	{
		auto desc = NEW_INHEAD VMethod8<Result, Klass, T0, T1, T2, T3, T4, T5, T6, T7>(fun, name, a0, a1, a2, a3, a4, a5, a6, a7);

		Methods.push_back(desc);
		return desc;
	}
	template<typename Result, typename Klass, typename T0, typename T1, typename T2, typename T3, typename T4, typename T5, typename T6, typename T7>
	RttiMethodBase* PushMethod8(typename VMethod8<Result, Klass, T0, T1, T2, T3, T4, T5, T6, T7>::StaticMethodFun fun, const char* name, const char* a0, const char* a1, const char* a2, const char* a3, const char* a4, const char* a5, const char* a6, const char* a7)
	{
		auto desc = NEW_INHEAD VMethod8<Result, Klass, T0, T1, T2, T3, T4, T5, T6, T7>(fun, name, a0, a1, a2, a3, a4, a5, a6, a7);

		Methods.push_back(desc);
		return desc;
	}

	template<typename Result, typename Klass, typename T0, typename T1, typename T2, typename T3, typename T4, typename T5, typename T6, typename T7, typename T8>
	RttiMethodBase* PushMethod9(typename VMethod9<Result, Klass, T0, T1, T2, T3, T4, T5, T6, T7, T8>::MethodFun fun, const char* name, const char* a0, const char* a1, const char* a2, const char* a3, const char* a4, const char* a5, const char* a6, const char* a7, const char* a8)
	{
		auto desc = NEW_INHEAD VMethod9<Result, Klass, T0, T1, T2, T3, T4, T5, T6, T7, T8>(fun, name, a0, a1, a2, a3, a4, a5, a6, a7, a8);

		Methods.push_back(desc);
		return desc;
	}
	template<typename Result, typename Klass, typename T0, typename T1, typename T2, typename T3, typename T4, typename T5, typename T6, typename T7, typename T8>
	RttiMethodBase* PushMethod9(typename VMethod9<Result, Klass, T0, T1, T2, T3, T4, T5, T6, T7, T8>::ConstMethodFun fun, const char* name, const char* a0, const char* a1, const char* a2, const char* a3, const char* a4, const char* a5, const char* a6, const char* a7, const char* a8)
	{
		auto desc = NEW_INHEAD VMethod9<Result, Klass, T0, T1, T2, T3, T4, T5, T6, T7, T8>(fun, name, a0, a1, a2, a3, a4, a5, a6, a7, a8);

		Methods.push_back(desc);
		return desc;
	}
	template<typename Result, typename Klass, typename T0, typename T1, typename T2, typename T3, typename T4, typename T5, typename T6, typename T7, typename T8>
	RttiMethodBase* PushMethod9(typename VMethod9<Result, Klass, T0, T1, T2, T3, T4, T5, T6, T7, T8>::StaticMethodFun fun, const char* name, const char* a0, const char* a1, const char* a2, const char* a3, const char* a4, const char* a5, const char* a6, const char* a7, const char* a8)
	{
		auto desc = NEW_INHEAD VMethod9<Result, Klass, T0, T1, T2, T3, T4, T5, T6, T7, T8>(fun, name, a0, a1, a2, a3, a4, a5, a6, a7, a8);

		Methods.push_back(desc);
		return desc;
	}

	virtual void Init();
};

class RttiStructManager
{
	std::map<std::string, RttiStruct*>	StructTyps;
	std::vector<RttiStruct*>			AllStructTyps;
public:
	void RegStructType(const char* name, RttiStruct* type);
	static RttiStructManager* GetInstance();
	
	~RttiStructManager();
	void BuildRtti();
	void Finalize();

	RttiStruct* FindStruct(const char* name);
	unsigned int GetStructNumber() {
		return (unsigned int)AllStructTyps.size();
	}
	RttiStruct* GetStruct(unsigned int index) {
		return AllStructTyps[index];
	}
};

template<typename Type>
struct AuxRttiStruct : public RttiStruct
{
	static AuxRttiStruct<Type>		Instance;
};

//template<typename Type>
//AuxRttiStruct<Type>		AuxRttiStruct<Type>::Instance;

template<>
struct AuxRttiStruct<void> : public RttiStruct
{
	static AuxRttiStruct<void>		Instance;
	AuxRttiStruct()
	{
		Size = 0;
		Name = "Void";
		NameSpace = "Global";
		RttiStructManager::GetInstance()->RegStructType(GetFullName().c_str(), this);
	}
};

template<>
struct AuxRttiStruct<bool> : public RttiStruct
{
	static AuxRttiStruct<bool>		Instance;
	AuxRttiStruct()
	{
		Size = sizeof(bool);
		Name = "Boolean";
		NameSpace = "Global";
		RttiStructManager::GetInstance()->RegStructType(GetFullName().c_str(), this);
	}
};

template<>
struct AuxRttiStruct<char> : public RttiStruct
{
	static AuxRttiStruct<char>		Instance;
	AuxRttiStruct()
	{
		Size = sizeof(char);
		Name = "Int8";
		NameSpace = "Global";
		RttiStructManager::GetInstance()->RegStructType(GetFullName().c_str(), this);
	}
};

template<>
struct AuxRttiStruct<short> : public RttiStruct
{
	static AuxRttiStruct<short>		Instance;
	AuxRttiStruct()
	{
		Size = sizeof(short);
		Name = "Int16";
		NameSpace = "Global";
		RttiStructManager::GetInstance()->RegStructType(GetFullName().c_str(), this);
	}
};

template<>
struct AuxRttiStruct<int> : public RttiStruct
{
	static AuxRttiStruct<int>		Instance;
	AuxRttiStruct()
	{
		Size = sizeof(int);
		Name = "Int32";
		NameSpace = "Global";
		RttiStructManager::GetInstance()->RegStructType(GetFullName().c_str(), this);
	}
};

template<>
struct AuxRttiStruct<long long> : public RttiStruct
{
	static AuxRttiStruct<long long>		Instance;
	AuxRttiStruct()
	{
		Size = sizeof(long long);
		Name = "Int64";
		NameSpace = "Global";
		RttiStructManager::GetInstance()->RegStructType(GetFullName().c_str(), this);
	}
};

template<>
struct AuxRttiStruct<unsigned char> : public RttiStruct
{
	static AuxRttiStruct<unsigned char>		Instance;
	AuxRttiStruct()
	{
		Size = sizeof(unsigned char);
		Name = "UInt8";
		NameSpace = "Global";
		RttiStructManager::GetInstance()->RegStructType(GetFullName().c_str(), this);
	}
};

template<>
struct AuxRttiStruct<unsigned short> : public RttiStruct
{
	static AuxRttiStruct<unsigned short>		Instance;
	AuxRttiStruct()
	{
		Size = sizeof(unsigned short);
		Name = "UInt16";
		NameSpace = "Global";
		RttiStructManager::GetInstance()->RegStructType(GetFullName().c_str(), this);
	}
};

template<>
struct AuxRttiStruct<unsigned int> : public RttiStruct
{
	static AuxRttiStruct<unsigned int>		Instance;
	AuxRttiStruct()
	{
		Size = sizeof(unsigned int);
		Name = "UInt32";
		NameSpace = "Global";
		RttiStructManager::GetInstance()->RegStructType(GetFullName().c_str(), this);
	}
};

template<>
struct AuxRttiStruct<unsigned long long> : public RttiStruct
{
	static AuxRttiStruct<unsigned long long>		Instance;
	AuxRttiStruct()
	{
		Size = sizeof(unsigned long long);
		Name = "UInt64";
		NameSpace = "Global";
		RttiStructManager::GetInstance()->RegStructType(GetFullName().c_str(), this);
	}
};

template<>
struct AuxRttiStruct<float> : public RttiStruct
{
	static AuxRttiStruct<float>		Instance;
	AuxRttiStruct()
	{
		Size = sizeof(float);
		Name = "Float";
		NameSpace = "Global";
		RttiStructManager::GetInstance()->RegStructType(GetFullName().c_str(), this);
	}
};

template<>
struct AuxRttiStruct<double> : public RttiStruct
{
	static AuxRttiStruct<double>		Instance;
	AuxRttiStruct()
	{
		Size = sizeof(double);
		Name = "Double";
		NameSpace = "Global";
		RttiStructManager::GetInstance()->RegStructType(GetFullName().c_str(), this);
	}
};

template<>
struct AuxRttiStruct<std::string> : public RttiStruct
{
	static AuxRttiStruct<std::string>		Instance;
	AuxRttiStruct()
	{
		Size = sizeof(std::string);
		Name = "String";
		NameSpace = "Global";
		RttiStructManager::GetInstance()->RegStructType(GetFullName().c_str(), this);
	}
	virtual void AssignOperator(void* pTar, const void* pSrc) const
	{
		*((std::string*)pTar) = *((std::string*)pSrc);
	}
};

template<typename _Type>
struct VTypeHelper
{
	enum 
	{
		TypeSizeOf = sizeof(_Type),
	};
	static void AssignOperator(void* pTar, const void* pSrc)
	{
		*(_Type*)pTar = *(_Type*)pSrc;
	}
	template<int _Size>
	struct VArrayElement
	{
		enum
		{
			Result = _Size / TypeSizeOf,
		};
	};
};

#define VTypeHelperDefine(_Type, TypeSize)	\
template<>\
struct VTypeHelper<typename remove_all_ref_ptr<_Type>::type>\
{\
	typedef typename remove_all_ref_ptr<_Type>::type RealType;\
	enum\
	{\
		TypeSizeOf = TypeSize,\
	};\
	static void AssignOperator(void* pTar, const void* pSrc)\
	{\
		memcpy(pTar, pSrc, TypeSizeOf);\
	}\
	template<int _Size>\
	struct VArrayElement\
	{\
		enum\
		{\
			Result = _Size / TypeSizeOf,\
		};\
	};\
};

template<>
struct VTypeHelper<void>
{
	enum
	{
		TypeSizeOf = 0,
	};
	static void AssignOperator(void* pTar, const void* pSrc)
	{
		
	}
	template<int _Size>
	struct VArrayElement
	{
		enum
		{
			Result = 1,
		};
	};
};


#define StructBegin(Type, ns) \
template<> \
struct AuxRttiStruct<typename remove_all_ref_ptr<ns::Type>::type> : public RttiStruct\
{\
	typedef remove_all_ref_ptr<ns::Type>::type	ThisStructType;\
	static AuxRttiStruct<typename remove_all_ref_ptr<ns::Type>::type>		Instance;\
	AuxRttiStruct()\
	{\
		Size = VTypeHelper<ThisStructType>::TypeSizeOf;\
		Name = #Type;\
		NameSpace = #ns;\
		RttiStructManager::GetInstance()->RegStructType(GetFullName().c_str(), this);\
		IsEnum = AuxRttiEnum<ThisStructType>::IsEnum; \
	}\
	virtual void AssignOperator(void* pTar, const void* pSrc) const override\
	{\
		VTypeHelper<ThisStructType>::AssignOperator(pTar, pSrc);\
	}\
	virtual void Init() override\
	{\
		RttiStruct::MemberDesc* __current_member = nullptr;\
		RttiMethodBase* __current_method = nullptr;\
		RttiConstructor* __current_constructor = nullptr;

#define  __vsizeof(_type, _name) sizeof(((_type*)nullptr)->_name)

#define AddClassMetaInfo(n,v) { this->AddMeta(#n,#v); }
#define AppendMemberMetaInfo(n,v) { if(__current_member!=nullptr){__current_member->AddMeta(#n,#v);} }
#define AppendMethodMetaInfo(n,v) { if(__current_method!=nullptr){__current_method->AddMeta(#n,#v);} }
#define AppendConstructorMetaInfo(n,v) { if(__current_constructor!=nullptr){__current_constructor->AddMeta(#n,#v);} }

#define StructMember(_name) \
		{\
			using declType = decltype(((ThisStructType*)nullptr)->_name);\
			using noConstType = std::remove_const<declType>::type;\
			using realType = std::remove_all_extents<noConstType>::type;\
			using pureType = remove_all_ref_ptr<realType>::type; \
			const unsigned int size = __vsizeof(ThisStructType, _name);\
			__current_member = PushMember(&AuxRttiStruct<pureType>::Instance, __voffsetof(ThisStructType, _name), size, VTypeHelper<realType>::VArrayElement<size>::Result,\
					#_name, std::is_pointer<noConstType>::value);\
		}

#define StructMethod0(name) \
		{\
			using TResult = TFunction_traits<decltype(&ThisStructType::name)>::return_type; \
			__current_method = PushMethod0<TResult, ThisStructType>(&ThisStructType::name, #name);\
		}

#define StructMethodEx0(name, TResult) \
		{\
			__current_method = PushMethod0<TResult, ThisStructType>(&ThisStructType::name, #name);\
		}

#define StructMethod1(name, a0) \
		{\
			auto funAddress = &ThisStructType::name;\
			using TFunctionType = decltype(funAddress);\
			static_assert(TFunction_traits<TFunctionType>::param_count==1);\
			using TResult = TFunction_traits<TFunctionType>::return_type;\
			using TA0 = TFunction_traits<TFunctionType>::param_type<0>;\
			__current_method = PushMethod1<TResult, ThisStructType, TA0>(funAddress, #name, #a0);\
		}

#define StructMethodEx1(name, TResult, TA0, a0) \
		{\
			__current_method = PushMethod1<TResult, ThisStructType, TA0>(&ThisStructType::name, #name, #a0);\
		}

#define StructMethod2(name, a0, a1) \
		{\
			auto funAddress = &ThisStructType::name;\
			using TFunctionType = decltype(funAddress);\
			static_assert(TFunction_traits<TFunctionType>::param_count==2);\
			using TResult = TFunction_traits<TFunctionType>::return_type;\
			using TA0 = TFunction_traits<TFunctionType>::param_type<0>;\
			using TA1 = TFunction_traits<TFunctionType>::param_type<1>;\
			__current_method = PushMethod2<TResult, ThisStructType, TA0, TA1>(funAddress, #name, #a0, #a1);\
		}

#define StructMethodEx2(name, TResult, TA0, a0, TA1, a1) \
		{\
			__current_method = PushMethod2<TResult, ThisStructType, TA0, TA1>(&ThisStructType::name, #name, #a0, #a1);\
		}

#define StructMethod3(name, a0, a1, a2) \
		{\
			auto funAddress = &ThisStructType::name;\
			using TFunctionType = decltype(funAddress);\
			static_assert(TFunction_traits<TFunctionType>::param_count==3);\
			using TResult = TFunction_traits<TFunctionType>::return_type;\
			using TA0 = TFunction_traits<TFunctionType>::param_type<0>;\
			using TA1 = TFunction_traits<TFunctionType>::param_type<1>;\
			using TA2 = TFunction_traits<TFunctionType>::param_type<2>;\
			__current_method = PushMethod2<TResult, ThisStructType, TA0, TA1, TA2>(funAddress, #name, #a0, #a1, #a2);\
		}

#define StructMethodEx3(name, TResult, TA0, a0, TA1, a1, TA2, a2) \
		{\
			__current_method = PushMethod3<TResult, ThisStructType, TA0, TA1, TA2>(&ThisStructType::name, #name, #a0, #a1, #a2);\
		}

#define StructMethod4(name, a0, a1, a2, a3) \
		{\
			auto funAddress = &ThisStructType::name;\
			using TFunctionType = decltype(funAddress);\
			static_assert(TFunction_traits<TFunctionType>::param_count==4);\
			using TResult = TFunction_traits<TFunctionType>::return_type;\
			using TA0 = TFunction_traits<TFunctionType>::param_type<0>;\
			using TA1 = TFunction_traits<TFunctionType>::param_type<1>;\
			using TA2 = TFunction_traits<TFunctionType>::param_type<2>;\
			using TA3 = TFunction_traits<TFunctionType>::param_type<3>;\
			__current_method = PushMethod4<TResult, ThisStructType, TA0, TA1, TA2, TA3>(funAddress, #name, #a0, #a1, #a2, #a3);\
		}

#define StructMethodEx4(name, TResult, TA0, a0, TA1, a1, TA2, a2, TA3, a3) \
		{\
			__current_method = PushMethod4<TResult, ThisStructType, TA0, TA1, TA2, TA3>(&ThisStructType::name, #name, #a0, #a1, #a2, #a3);\
		}

#define StructMethod5(name, a0, a1, a2, a3, a4) \
		{\
			auto funAddress = &ThisStructType::name;\
			using TFunctionType = decltype(funAddress);\
			static_assert(TFunction_traits<TFunctionType>::param_count==4);\
			using TResult = TFunction_traits<TFunctionType>::return_type;\
			using TA0 = TFunction_traits<TFunctionType>::param_type<0>;\
			using TA1 = TFunction_traits<TFunctionType>::param_type<1>;\
			using TA2 = TFunction_traits<TFunctionType>::param_type<2>;\
			using TA3 = TFunction_traits<TFunctionType>::param_type<3>;\
			using TA4 = TFunction_traits<TFunctionType>::param_type<4>;\
			__current_method = PushMethod5<TResult, ThisStructType, TA0, TA1, TA2, TA3, TA4>(funAddress, #name, #a0, #a1, #a2, #a3, #a4);\
		}

#define StructMethodEx5(name, TResult, TA0, a0, TA1, a1, TA2, a2, TA3, a3, TA4, a4) \
		{\
			__current_method = PushMethod5<TResult, ThisStructType, TA0, TA1, TA2, TA3, TA4>(&ThisStructType::name, #name, #a0, #a1, #a2, #a3, #a4);\
		}

#define StructMethod6(name, a0, a1, a2, a3, a4, a5) \
		{\
			auto funAddress = &ThisStructType::name;\
			using TFunctionType = decltype(funAddress);\
			static_assert(TFunction_traits<TFunctionType>::param_count==4);\
			using TResult = TFunction_traits<TFunctionType>::return_type;\
			using TA0 = TFunction_traits<TFunctionType>::param_type<0>;\
			using TA1 = TFunction_traits<TFunctionType>::param_type<1>;\
			using TA2 = TFunction_traits<TFunctionType>::param_type<2>;\
			using TA3 = TFunction_traits<TFunctionType>::param_type<3>;\
			using TA4 = TFunction_traits<TFunctionType>::param_type<4>;\
			using TA5 = TFunction_traits<TFunctionType>::param_type<5>;\
			__current_method = PushMethod6<TResult, ThisStructType, TA0, TA1, TA2, TA3, TA4, TA5>(funAddress, #name, #a0, #a1, #a2, #a3, #a4, #a5);\
		}

#define StructMethodEx6(name, TResult, TA0, a0, TA1, a1, TA2, a2, TA3, a3, TA4, a4, TA5, a5) \
		{\
			__current_method = PushMethod6<TResult, ThisStructType, TA0, TA1, TA2, TA3, TA4, TA5>(&ThisStructType::name, #name, #a0, #a1, #a2, #a3, #a4, #a5);\
		}

#define StructMethod7(name, a0, a1, a2, a3, a4, a5, a6) \
		{\
			auto funAddress = &ThisStructType::name;\
			using TFunctionType = decltype(funAddress);\
			static_assert(TFunction_traits<TFunctionType>::param_count==4);\
			using TResult = TFunction_traits<TFunctionType>::return_type;\
			using TA0 = TFunction_traits<TFunctionType>::param_type<0>;\
			using TA1 = TFunction_traits<TFunctionType>::param_type<1>;\
			using TA2 = TFunction_traits<TFunctionType>::param_type<2>;\
			using TA3 = TFunction_traits<TFunctionType>::param_type<3>;\
			using TA4 = TFunction_traits<TFunctionType>::param_type<4>;\
			using TA5 = TFunction_traits<TFunctionType>::param_type<5>;\
			using TA6 = TFunction_traits<TFunctionType>::param_type<6>;\
			__current_method = PushMethod7<TResult, ThisStructType, TA0, TA1, TA2, TA3, TA4, TA5, TA6>(funAddress, #name, #a0, #a1, #a2, #a3, #a4, #a5, #a6);\
		}

#define StructMethodEx7(name, TResult, TA0, a0, TA1, a1, TA2, a2, TA3, a3, TA4, a4, TA5, a5, TA6, a6) \
		{\
			__current_method = PushMethod7<TResult, ThisStructType, TA0, TA1, TA2, TA3, TA4, TA5, TA6>(&ThisStructType::name, #name, #a0, #a1, #a2, #a3, #a4, #a5, #a6);\
		}

#define StructMethod8(name, a0, a1, a2, a3, a4, a5, a6, a7) \
		{\
			auto funAddress = &ThisStructType::name;\
			using TFunctionType = decltype(funAddress);\
			static_assert(TFunction_traits<TFunctionType>::param_count==4);\
			using TResult = TFunction_traits<TFunctionType>::return_type;\
			using TA0 = TFunction_traits<TFunctionType>::param_type<0>;\
			using TA1 = TFunction_traits<TFunctionType>::param_type<1>;\
			using TA2 = TFunction_traits<TFunctionType>::param_type<2>;\
			using TA3 = TFunction_traits<TFunctionType>::param_type<3>;\
			using TA4 = TFunction_traits<TFunctionType>::param_type<4>;\
			using TA5 = TFunction_traits<TFunctionType>::param_type<5>;\
			using TA6 = TFunction_traits<TFunctionType>::param_type<6>;\
			using TA7 = TFunction_traits<TFunctionType>::param_type<7>;\
			__current_method = PushMethod8<TResult, ThisStructType, TA0, TA1, TA2, TA3, TA4, TA5, TA6, TA7>(funAddress, #name, #a0, #a1, #a2, #a3, #a4, #a5, #a6, #a7);\
		}

#define StructMethodEx8(name, TResult, TA0, a0, TA1, a1, TA2, a2, TA3, a3, TA4, a4, TA5, a5, TA6, a6, TA7, a7) \
		{\
			__current_method = PushMethod8<TResult, ThisStructType, TA0, TA1, TA2, TA3, TA4, TA5, TA6, TA7>(&ThisStructType::name, #name, #a0, #a1, #a2, #a3, #a4, #a5, #a6, #a7);\
		}

#define StructMethod9(name, a0, a1, a2, a3, a4, a5, a6, a7, a8) \
		{\
			auto funAddress = &ThisStructType::name;\
			using TFunctionType = decltype(funAddress);\
			static_assert(TFunction_traits<TFunctionType>::param_count==4);\
			using TResult = TFunction_traits<TFunctionType>::return_type;\
			using TA0 = TFunction_traits<TFunctionType>::param_type<0>;\
			using TA1 = TFunction_traits<TFunctionType>::param_type<1>;\
			using TA2 = TFunction_traits<TFunctionType>::param_type<2>;\
			using TA3 = TFunction_traits<TFunctionType>::param_type<3>;\
			using TA4 = TFunction_traits<TFunctionType>::param_type<4>;\
			using TA5 = TFunction_traits<TFunctionType>::param_type<5>;\
			using TA6 = TFunction_traits<TFunctionType>::param_type<6>;\
			using TA7 = TFunction_traits<TFunctionType>::param_type<7>;\
			using TA8 = TFunction_traits<TFunctionType>::param_type<8>;\
			__current_method = PushMethod9<TResult, ThisStructType, TA0, TA1, TA2, TA3, TA4, TA5, TA6, TA7, TA8>(funAddress, #name, #a0, #a1, #a2, #a3, #a4, #a5, #a6, #a7, $a8);\
		}

#define StructMethodEx9(name, TResult, TA0, a0, TA1, a1, TA2, a2, TA3, a3, TA4, a4, TA5, a5, TA6, a6, TA7, a7, TA8, a8) \
		{\
			__current_method = PushMethod9<TResult, ThisStructType, TA0, TA1, TA2, TA3, TA4, TA5, TA6, TA7, TA8>(&ThisStructType::name, #name, #a0, #a1, #a2, #a3, #a4, #a5, #a6, #a7, #a8);\
		}

#define StructConstructor0() \
		{\
			auto desc = NEW_INHEAD VConstructor0<ThisStructType>();\
			Constructors.push_back(desc);\
			__current_constructor = desc;\
		}

#define StructConstructor1(_T0) \
		{\
			auto desc = NEW_INHEAD VConstructor1<ThisStructType, _T0>();\
			Constructors.push_back(desc);\
			__current_constructor = desc;\
		}

#define StructConstructor2(_T0, _T1) \
		{\
			auto desc = NEW_INHEAD VConstructor2<ThisStructType, _T0, _T1>();\
			Constructors.push_back(desc);\
			__current_constructor = desc;\
		}

#define StructEnd(ParentType) \
		__current_member = nullptr;\
		__current_method = nullptr; \
		__current_constructor = nullptr;\
		ParentStructType = &AuxRttiStruct<ParentType>::Instance;;\
		RttiStruct::Init();\
	}\
};

#define StructImpl(Type) \
AuxRttiStruct<typename remove_all_ref_ptr<Type>::type> AuxRttiStruct<typename remove_all_ref_ptr<Type>::type>::Instance;

template<typename T>
void RttiStruct::MemberDesc::SetValue(void* pThis, const T* v) const
{
	if (AuxRttiStruct<typename remove_all_ref_ptr<T>::type>::Instance.IsA(MemberType) == false)
	{
		return;
	}
	if (IsPointer)
	{
		memcpy((BYTE*)pThis + Offset, &v, sizeof(const T*));
	}
	else
	{
		MemberType->AssignOperator((BYTE*)pThis + Offset, v);
	}
}
template<typename T>
T* RttiStruct::MemberDesc::GetValueAddress(void* pThis) const
{
	if (AuxRttiStruct<typename remove_all_ref_ptr<T>::type>::Instance.IsA(MemberType) == false)
		return nullptr;
	BYTE* pAddress = (BYTE*)pThis + Offset;
	if (IsPointer)
	{
		T* result = nullptr;
		memcpy(result, pAddress, sizeof(void*));
		return result;
	}
	else
	{
		return (T*)(pAddress);
	}
}

template<typename Klass, typename T0>
VConstructor1<Klass, T0>::VConstructor1()
{
	mArguments.push_back(&AuxRttiStruct<typename remove_all_ref_ptr<T0>::type>::Instance);
}

template<typename Klass, typename T0, typename T1>
VConstructor2<Klass, T0, T1>::VConstructor2()
{
	mArguments.push_back(&AuxRttiStruct<typename remove_all_ref_ptr<T0>::type>::Instance);
	mArguments.push_back(&AuxRttiStruct<typename remove_all_ref_ptr<T1>::type>::Instance);
}

template<typename Result, typename Klass>
VMethod0<Result, Klass>::VMethod0(MethodFun fun, const char* name)
{
	FuncPtr = fun;
	Name = name;
	ResultType = &AuxRttiStruct<typename remove_all_ref_ptr<Result>::type>::Instance;
	ThisObject = &AuxRttiStruct<typename remove_all_ref_ptr<Klass>::type>::Instance;
}
template<typename Result, typename Klass>
VMethod0<Result, Klass>::VMethod0(StaticMethodFun fun, const char* name)
{
	IsStatic = true;
	StaticFuncPtr = fun;
	Name = name;
	ResultType = &AuxRttiStruct<typename remove_all_ref_ptr<Result>::type>::Instance;
	ThisObject = &AuxRttiStruct<typename remove_all_ref_ptr<Klass>::type>::Instance;
}

template<typename Result, typename Klass, typename T0>
VMethod1<Result, Klass, T0>::VMethod1(MethodFun fun, const char* name, const char* a0)
{
	FuncPtr = fun;
	Name = name;
	ResultType = &AuxRttiStruct<typename remove_all_ref_ptr<Result>::type>::Instance;
	ThisObject = &AuxRttiStruct<typename remove_all_ref_ptr<Klass>::type>::Instance;

	Arguments.push_back(std::make_pair(a0,&AuxRttiStruct<typename remove_all_ref_ptr<T0>::type>::Instance));
}
template<typename Result, typename Klass, typename T0>
VMethod1<Result, Klass, T0>::VMethod1(StaticMethodFun fun, const char* name, const char* a0)
{
	IsStatic = true;
	StaticFuncPtr = fun;
	Name = name;
	ResultType = &AuxRttiStruct<typename remove_all_ref_ptr<Result>::type>::Instance;
	ThisObject = &AuxRttiStruct<typename remove_all_ref_ptr<Klass>::type>::Instance;

	Arguments.push_back(std::make_pair(a0, &AuxRttiStruct<typename remove_all_ref_ptr<T0>::type>::Instance));
}

template<typename Result, typename Klass, typename T0, typename T1>
VMethod2<Result, Klass, T0, T1>::VMethod2(MethodFun fun, const char* name, const char* a0, const char* a1)
{
	FuncPtr = fun;
	Name = name;
	ResultType = &AuxRttiStruct<typename remove_all_ref_ptr<Result>::type>::Instance;
	ThisObject = &AuxRttiStruct<typename remove_all_ref_ptr<Klass>::type>::Instance;

	Arguments.push_back(std::make_pair(a0, &AuxRttiStruct<typename remove_all_ref_ptr<T0>::type>::Instance));
	Arguments.push_back(std::make_pair(a1, &AuxRttiStruct<typename remove_all_ref_ptr<T1>::type>::Instance));
}
template<typename Result, typename Klass, typename T0, typename T1>
VMethod2<Result, Klass, T0, T1>::VMethod2(StaticMethodFun fun, const char* name, const char* a0, const char* a1)
{
	IsStatic = true;
	StaticFuncPtr = fun;
	Name = name;
	ResultType = &AuxRttiStruct<typename remove_all_ref_ptr<Result>::type>::Instance;
	ThisObject = &AuxRttiStruct<typename remove_all_ref_ptr<Klass>::type>::Instance;

	Arguments.push_back(std::make_pair(a0, &AuxRttiStruct<typename remove_all_ref_ptr<T0>::type>::Instance));
	Arguments.push_back(std::make_pair(a1, &AuxRttiStruct<typename remove_all_ref_ptr<T1>::type>::Instance));
}

template<typename Result, typename Klass, typename T0, typename T1, typename T2>
VMethod3<Result, Klass, T0, T1, T2>::VMethod3(MethodFun fun, const char* name, const char* a0, const char* a1, const char* a2)
{
	FuncPtr = fun;
	Name = name;
	ResultType = &AuxRttiStruct<typename remove_all_ref_ptr<Result>::type>::Instance;
	ThisObject = &AuxRttiStruct<typename remove_all_ref_ptr<Klass>::type>::Instance;

	Arguments.push_back(std::make_pair(a0, &AuxRttiStruct<typename remove_all_ref_ptr<T0>::type>::Instance));
	Arguments.push_back(std::make_pair(a1, &AuxRttiStruct<typename remove_all_ref_ptr<T1>::type>::Instance));
	Arguments.push_back(std::make_pair(a2, &AuxRttiStruct<typename remove_all_ref_ptr<T2>::type>::Instance));
}
template<typename Result, typename Klass, typename T0, typename T1, typename T2>
VMethod3<Result, Klass, T0, T1, T2>::VMethod3(StaticMethodFun fun, const char* name, const char* a0, const char* a1, const char* a2)
{
	IsStatic = true;
	StaticFuncPtr = fun;
	Name = name;
	ResultType = &AuxRttiStruct<typename remove_all_ref_ptr<Result>::type>::Instance;
	ThisObject = &AuxRttiStruct<typename remove_all_ref_ptr<Klass>::type>::Instance;

	Arguments.push_back(std::make_pair(a0, &AuxRttiStruct<typename remove_all_ref_ptr<T0>::type>::Instance));
	Arguments.push_back(std::make_pair(a1, &AuxRttiStruct<typename remove_all_ref_ptr<T1>::type>::Instance));
	Arguments.push_back(std::make_pair(a2, &AuxRttiStruct<typename remove_all_ref_ptr<T2>::type>::Instance));
}

template<typename Result, typename Klass, typename T0, typename T1, typename T2, typename T3>
VMethod4<Result, Klass, T0, T1, T2, T3>::VMethod4(MethodFun fun, const char* name, const char* a0, const char* a1, const char* a2, const char* a3)
{
	FuncPtr = fun;
	Name = name;
	ResultType = &AuxRttiStruct<typename remove_all_ref_ptr<Result>::type>::Instance;
	ThisObject = &AuxRttiStruct<typename remove_all_ref_ptr<Klass>::type>::Instance;

	Arguments.push_back(std::make_pair(a0, &AuxRttiStruct<typename remove_all_ref_ptr<T0>::type>::Instance));
	Arguments.push_back(std::make_pair(a1, &AuxRttiStruct<typename remove_all_ref_ptr<T1>::type>::Instance));
	Arguments.push_back(std::make_pair(a2, &AuxRttiStruct<typename remove_all_ref_ptr<T2>::type>::Instance));
	Arguments.push_back(std::make_pair(a3, &AuxRttiStruct<typename remove_all_ref_ptr<T3>::type>::Instance));
}
template<typename Result, typename Klass, typename T0, typename T1, typename T2, typename T3>
VMethod4<Result, Klass, T0, T1, T2, T3>::VMethod4(StaticMethodFun fun, const char* name, const char* a0, const char* a1, const char* a2, const char* a3)
{
	IsStatic = true;
	StaticFuncPtr = fun;
	Name = name;
	ResultType = &AuxRttiStruct<typename remove_all_ref_ptr<Result>::type>::Instance;
	ThisObject = &AuxRttiStruct<typename remove_all_ref_ptr<Klass>::type>::Instance;

	Arguments.push_back(std::make_pair(a0, &AuxRttiStruct<typename remove_all_ref_ptr<T0>::type>::Instance));
	Arguments.push_back(std::make_pair(a1, &AuxRttiStruct<typename remove_all_ref_ptr<T1>::type>::Instance));
	Arguments.push_back(std::make_pair(a2, &AuxRttiStruct<typename remove_all_ref_ptr<T2>::type>::Instance));
	Arguments.push_back(std::make_pair(a3, &AuxRttiStruct<typename remove_all_ref_ptr<T3>::type>::Instance));
}

template<typename Result, typename Klass, typename T0, typename T1, typename T2, typename T3, typename T4>
VMethod5<Result, Klass, T0, T1, T2, T3, T4>::VMethod5(MethodFun fun, const char* name, const char* a0, const char* a1, const char* a2, const char* a3, const char* a4)
{
	FuncPtr = fun;
	Name = name;
	ResultType = &AuxRttiStruct<typename remove_all_ref_ptr<Result>::type>::Instance;
	ThisObject = &AuxRttiStruct<typename remove_all_ref_ptr<Klass>::type>::Instance;

	Arguments.push_back(std::make_pair(a0, &AuxRttiStruct<typename remove_all_ref_ptr<T0>::type>::Instance));
	Arguments.push_back(std::make_pair(a1, &AuxRttiStruct<typename remove_all_ref_ptr<T1>::type>::Instance));
	Arguments.push_back(std::make_pair(a2, &AuxRttiStruct<typename remove_all_ref_ptr<T2>::type>::Instance));
	Arguments.push_back(std::make_pair(a3, &AuxRttiStruct<typename remove_all_ref_ptr<T3>::type>::Instance));
	Arguments.push_back(std::make_pair(a4, &AuxRttiStruct<typename remove_all_ref_ptr<T4>::type>::Instance));
}
template<typename Result, typename Klass, typename T0, typename T1, typename T2, typename T3, typename T4>
VMethod5<Result, Klass, T0, T1, T2, T3, T4>::VMethod5(StaticMethodFun fun, const char* name, const char* a0, const char* a1, const char* a2, const char* a3, const char* a4)
{
	IsStatic = true;
	StaticFuncPtr = fun;
	Name = name;
	ResultType = &AuxRttiStruct<typename remove_all_ref_ptr<Result>::type>::Instance;
	ThisObject = &AuxRttiStruct<typename remove_all_ref_ptr<Klass>::type>::Instance;

	Arguments.push_back(std::make_pair(a0, &AuxRttiStruct<typename remove_all_ref_ptr<T0>::type>::Instance));
	Arguments.push_back(std::make_pair(a1, &AuxRttiStruct<typename remove_all_ref_ptr<T1>::type>::Instance));
	Arguments.push_back(std::make_pair(a2, &AuxRttiStruct<typename remove_all_ref_ptr<T2>::type>::Instance));
	Arguments.push_back(std::make_pair(a3, &AuxRttiStruct<typename remove_all_ref_ptr<T3>::type>::Instance));
	Arguments.push_back(std::make_pair(a4, &AuxRttiStruct<typename remove_all_ref_ptr<T4>::type>::Instance));
}

template<typename Result, typename Klass, typename T0, typename T1, typename T2, typename T3, typename T4, typename T5>
VMethod6<Result, Klass, T0, T1, T2, T3, T4, T5>::VMethod6(MethodFun fun, const char* name, const char* a0, const char* a1, const char* a2, const char* a3, const char* a4, const char* a5)
{
	FuncPtr = fun;
	Name = name;
	ResultType = &AuxRttiStruct<typename remove_all_ref_ptr<Result>::type>::Instance;
	ThisObject = &AuxRttiStruct<typename remove_all_ref_ptr<Klass>::type>::Instance;

	Arguments.push_back(std::make_pair(a0, &AuxRttiStruct<typename remove_all_ref_ptr<T0>::type>::Instance));
	Arguments.push_back(std::make_pair(a1, &AuxRttiStruct<typename remove_all_ref_ptr<T1>::type>::Instance));
	Arguments.push_back(std::make_pair(a2, &AuxRttiStruct<typename remove_all_ref_ptr<T2>::type>::Instance));
	Arguments.push_back(std::make_pair(a3, &AuxRttiStruct<typename remove_all_ref_ptr<T3>::type>::Instance));
	Arguments.push_back(std::make_pair(a4, &AuxRttiStruct<typename remove_all_ref_ptr<T4>::type>::Instance));
	Arguments.push_back(std::make_pair(a5, &AuxRttiStruct<typename remove_all_ref_ptr<T5>::type>::Instance));
}
template<typename Result, typename Klass, typename T0, typename T1, typename T2, typename T3, typename T4, typename T5>
VMethod6<Result, Klass, T0, T1, T2, T3, T4, T5>::VMethod6(StaticMethodFun fun, const char* name, const char* a0, const char* a1, const char* a2, const char* a3, const char* a4, const char* a5)
{
	IsStatic = true;
	StaticFuncPtr = fun;
	Name = name;
	ResultType = &AuxRttiStruct<typename remove_all_ref_ptr<Result>::type>::Instance;
	ThisObject = &AuxRttiStruct<typename remove_all_ref_ptr<Klass>::type>::Instance;

	Arguments.push_back(std::make_pair(a0, &AuxRttiStruct<typename remove_all_ref_ptr<T0>::type>::Instance));
	Arguments.push_back(std::make_pair(a1, &AuxRttiStruct<typename remove_all_ref_ptr<T1>::type>::Instance));
	Arguments.push_back(std::make_pair(a2, &AuxRttiStruct<typename remove_all_ref_ptr<T2>::type>::Instance));
	Arguments.push_back(std::make_pair(a3, &AuxRttiStruct<typename remove_all_ref_ptr<T3>::type>::Instance));
	Arguments.push_back(std::make_pair(a4, &AuxRttiStruct<typename remove_all_ref_ptr<T4>::type>::Instance));
	Arguments.push_back(std::make_pair(a5, &AuxRttiStruct<typename remove_all_ref_ptr<T5>::type>::Instance));
}

template<typename Result, typename Klass, typename T0, typename T1, typename T2, typename T3, typename T4, typename T5, typename T6>
VMethod7<Result, Klass, T0, T1, T2, T3, T4, T5, T6>::VMethod7(MethodFun fun, const char* name, const char* a0, const char* a1, const char* a2, const char* a3, const char* a4, const char* a5, const char* a6)
{
	FuncPtr = fun;
	Name = name;
	ResultType = &AuxRttiStruct<typename remove_all_ref_ptr<Result>::type>::Instance;
	ThisObject = &AuxRttiStruct<typename remove_all_ref_ptr<Klass>::type>::Instance;

	Arguments.push_back(std::make_pair(a0, &AuxRttiStruct<typename remove_all_ref_ptr<T0>::type>::Instance));
	Arguments.push_back(std::make_pair(a1, &AuxRttiStruct<typename remove_all_ref_ptr<T1>::type>::Instance));
	Arguments.push_back(std::make_pair(a2, &AuxRttiStruct<typename remove_all_ref_ptr<T2>::type>::Instance));
	Arguments.push_back(std::make_pair(a3, &AuxRttiStruct<typename remove_all_ref_ptr<T3>::type>::Instance));
	Arguments.push_back(std::make_pair(a4, &AuxRttiStruct<typename remove_all_ref_ptr<T4>::type>::Instance));
	Arguments.push_back(std::make_pair(a5, &AuxRttiStruct<typename remove_all_ref_ptr<T5>::type>::Instance));
	Arguments.push_back(std::make_pair(a6, &AuxRttiStruct<typename remove_all_ref_ptr<T6>::type>::Instance));
}
template<typename Result, typename Klass, typename T0, typename T1, typename T2, typename T3, typename T4, typename T5, typename T6>
VMethod7<Result, Klass, T0, T1, T2, T3, T4, T5, T6>::VMethod7(StaticMethodFun fun, const char* name, const char* a0, const char* a1, const char* a2, const char* a3, const char* a4, const char* a5, const char* a6)
{
	IsStatic = true;
	StaticFuncPtr = fun;
	Name = name;
	ResultType = &AuxRttiStruct<typename remove_all_ref_ptr<Result>::type>::Instance;
	ThisObject = &AuxRttiStruct<typename remove_all_ref_ptr<Klass>::type>::Instance;

	Arguments.push_back(std::make_pair(a0, &AuxRttiStruct<typename remove_all_ref_ptr<T0>::type>::Instance));
	Arguments.push_back(std::make_pair(a1, &AuxRttiStruct<typename remove_all_ref_ptr<T1>::type>::Instance));
	Arguments.push_back(std::make_pair(a2, &AuxRttiStruct<typename remove_all_ref_ptr<T2>::type>::Instance));
	Arguments.push_back(std::make_pair(a3, &AuxRttiStruct<typename remove_all_ref_ptr<T3>::type>::Instance));
	Arguments.push_back(std::make_pair(a4, &AuxRttiStruct<typename remove_all_ref_ptr<T4>::type>::Instance));
	Arguments.push_back(std::make_pair(a5, &AuxRttiStruct<typename remove_all_ref_ptr<T5>::type>::Instance));
	Arguments.push_back(std::make_pair(a6, &AuxRttiStruct<typename remove_all_ref_ptr<T6>::type>::Instance));
}

template<typename Result, typename Klass, typename T0, typename T1, typename T2, typename T3, typename T4, typename T5, typename T6, typename T7>
VMethod8<Result, Klass, T0, T1, T2, T3, T4, T5, T6, T7>::VMethod8(MethodFun fun, const char* name, const char* a0, const char* a1, const char* a2, const char* a3, const char* a4, const char* a5, const char* a6, const char* a7)
{
	FuncPtr = fun;
	Name = name;
	ResultType = &AuxRttiStruct<typename remove_all_ref_ptr<Result>::type>::Instance;
	ThisObject = &AuxRttiStruct<typename remove_all_ref_ptr<Klass>::type>::Instance;

	Arguments.push_back(std::make_pair(a0, &AuxRttiStruct<typename remove_all_ref_ptr<T0>::type>::Instance));
	Arguments.push_back(std::make_pair(a1, &AuxRttiStruct<typename remove_all_ref_ptr<T1>::type>::Instance));
	Arguments.push_back(std::make_pair(a2, &AuxRttiStruct<typename remove_all_ref_ptr<T2>::type>::Instance));
	Arguments.push_back(std::make_pair(a3, &AuxRttiStruct<typename remove_all_ref_ptr<T3>::type>::Instance));
	Arguments.push_back(std::make_pair(a4, &AuxRttiStruct<typename remove_all_ref_ptr<T4>::type>::Instance));
	Arguments.push_back(std::make_pair(a5, &AuxRttiStruct<typename remove_all_ref_ptr<T5>::type>::Instance));
	Arguments.push_back(std::make_pair(a6, &AuxRttiStruct<typename remove_all_ref_ptr<T6>::type>::Instance));
	Arguments.push_back(std::make_pair(a7, &AuxRttiStruct<typename remove_all_ref_ptr<T7>::type>::Instance));
}
template<typename Result, typename Klass, typename T0, typename T1, typename T2, typename T3, typename T4, typename T5, typename T6, typename T7>
VMethod8<Result, Klass, T0, T1, T2, T3, T4, T5, T6, T7>::VMethod8(StaticMethodFun fun, const char* name, const char* a0, const char* a1, const char* a2, const char* a3, const char* a4, const char* a5, const char* a6, const char* a7)
{
	IsStatic = true;
	StaticFuncPtr = fun;
	Name = name;
	ResultType = &AuxRttiStruct<typename remove_all_ref_ptr<Result>::type>::Instance;
	ThisObject = &AuxRttiStruct<typename remove_all_ref_ptr<Klass>::type>::Instance;

	Arguments.push_back(std::make_pair(a0, &AuxRttiStruct<typename remove_all_ref_ptr<T0>::type>::Instance));
	Arguments.push_back(std::make_pair(a1, &AuxRttiStruct<typename remove_all_ref_ptr<T1>::type>::Instance));
	Arguments.push_back(std::make_pair(a2, &AuxRttiStruct<typename remove_all_ref_ptr<T2>::type>::Instance));
	Arguments.push_back(std::make_pair(a3, &AuxRttiStruct<typename remove_all_ref_ptr<T3>::type>::Instance));
	Arguments.push_back(std::make_pair(a4, &AuxRttiStruct<typename remove_all_ref_ptr<T4>::type>::Instance));
	Arguments.push_back(std::make_pair(a5, &AuxRttiStruct<typename remove_all_ref_ptr<T5>::type>::Instance));
	Arguments.push_back(std::make_pair(a6, &AuxRttiStruct<typename remove_all_ref_ptr<T6>::type>::Instance));
	Arguments.push_back(std::make_pair(a7, &AuxRttiStruct<typename remove_all_ref_ptr<T7>::type>::Instance));
}

template<typename Result, typename Klass, typename T0, typename T1, typename T2, typename T3, typename T4, typename T5, typename T6, typename T7, typename T8>
VMethod9<Result, Klass, T0, T1, T2, T3, T4, T5, T6, T7, T8>::VMethod9(MethodFun fun, const char* name, const char* a0, const char* a1, const char* a2, const char* a3, const char* a4, const char* a5, const char* a6, const char* a7, const char* a8)
{
	FuncPtr = fun;
	Name = name;
	ResultType = &AuxRttiStruct<typename remove_all_ref_ptr<Result>::type>::Instance;
	ThisObject = &AuxRttiStruct<typename remove_all_ref_ptr<Klass>::type>::Instance;

	Arguments.push_back(std::make_pair(a0, &AuxRttiStruct<typename remove_all_ref_ptr<T0>::type>::Instance));
	Arguments.push_back(std::make_pair(a1, &AuxRttiStruct<typename remove_all_ref_ptr<T1>::type>::Instance));
	Arguments.push_back(std::make_pair(a2, &AuxRttiStruct<typename remove_all_ref_ptr<T2>::type>::Instance));
	Arguments.push_back(std::make_pair(a3, &AuxRttiStruct<typename remove_all_ref_ptr<T3>::type>::Instance));
	Arguments.push_back(std::make_pair(a4, &AuxRttiStruct<typename remove_all_ref_ptr<T4>::type>::Instance));
	Arguments.push_back(std::make_pair(a5, &AuxRttiStruct<typename remove_all_ref_ptr<T5>::type>::Instance));
	Arguments.push_back(std::make_pair(a6, &AuxRttiStruct<typename remove_all_ref_ptr<T6>::type>::Instance));
	Arguments.push_back(std::make_pair(a7, &AuxRttiStruct<typename remove_all_ref_ptr<T7>::type>::Instance));
	Arguments.push_back(std::make_pair(a8, &AuxRttiStruct<typename remove_all_ref_ptr<T8>::type>::Instance));
}
template<typename Result, typename Klass, typename T0, typename T1, typename T2, typename T3, typename T4, typename T5, typename T6, typename T7, typename T8>
VMethod9<Result, Klass, T0, T1, T2, T3, T4, T5, T6, T7, T8>::VMethod9(StaticMethodFun fun, const char* name, const char* a0, const char* a1, const char* a2, const char* a3, const char* a4, const char* a5, const char* a6, const char* a7, const char* a8)
{
	IsStatic = true;
	StaticFuncPtr = fun;
	Name = name;
	ResultType = &AuxRttiStruct<typename remove_all_ref_ptr<Result>::type>::Instance;
	ThisObject = &AuxRttiStruct<typename remove_all_ref_ptr<Klass>::type>::Instance;

	Arguments.push_back(std::make_pair(a0, &AuxRttiStruct<typename remove_all_ref_ptr<T0>::type>::Instance));
	Arguments.push_back(std::make_pair(a1, &AuxRttiStruct<typename remove_all_ref_ptr<T1>::type>::Instance));
	Arguments.push_back(std::make_pair(a2, &AuxRttiStruct<typename remove_all_ref_ptr<T2>::type>::Instance));
	Arguments.push_back(std::make_pair(a3, &AuxRttiStruct<typename remove_all_ref_ptr<T3>::type>::Instance));
	Arguments.push_back(std::make_pair(a4, &AuxRttiStruct<typename remove_all_ref_ptr<T4>::type>::Instance));
	Arguments.push_back(std::make_pair(a5, &AuxRttiStruct<typename remove_all_ref_ptr<T5>::type>::Instance));
	Arguments.push_back(std::make_pair(a6, &AuxRttiStruct<typename remove_all_ref_ptr<T6>::type>::Instance));
	Arguments.push_back(std::make_pair(a7, &AuxRttiStruct<typename remove_all_ref_ptr<T7>::type>::Instance));
	Arguments.push_back(std::make_pair(a8, &AuxRttiStruct<typename remove_all_ref_ptr<T8>::type>::Instance));
}

NS_END