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
	std::vector<std::string>	MetaInfos;
	bool HasMeta(const char* info)
	{
		for (const auto& i : MetaInfos)
		{
			if (i == info)
				return true;
		}
		return false;
	}
	void AddMeta(const char* info)
	{
		for (const auto& i : MetaInfos)
		{
			if (i == info)
				return;
		}
		MetaInfos.push_back(info);
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
	static AuxRttiEnum<Type>*		Instance;
	static const bool IsEnum = false;
};

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
	EnumDesc = &AuxRttiEnum<name>::Instance;\
	IsEnum = true;\
StructEnd(name)

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
	std::string			Name;
	RttiStruct*			ThisObject;

	RttiStruct*			ResultType;
	std::vector<std::pair<std::string,RttiStruct*>>	Arguments;
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
	typedef Result(BindClass::*MethodFun)();
	VMethod0(MethodFun fun, const char* name);	
	MethodFun	FuncPtr;

	virtual void Invoke(void* pThis, ArgumentStream& args, ArgumentStream& result) const override
	{
		InvokeImpl<Result>(pThis, args, result);
	}
private:
	template<typename TR>
	void InvokeImpl(void* pThis, ArgumentStream& args, ArgumentStream& result) const
	{
		TR ret = (((BindClass*)pThis)->*(FuncPtr))();
		result << ret;
	}
	template<>
	void InvokeImpl<void>(void* pThis, ArgumentStream& args, ArgumentStream& result) const
	{
		(((BindClass*)pThis)->*(FuncPtr))();
	}
};

#define Method_Arg_Def(index) \
	T##index a##index = T##index();\
	args >> a##index;

template<typename Result, typename BindClass, typename T0>
struct VMethod1 : public RttiMethodBase
{
	typedef Result(BindClass::*MethodFun)(T0);
	VMethod1(MethodFun fun, const char* name, const char* a0);
	MethodFun	FuncPtr;

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
		TR ret = (((BindClass*)pThis)->*(FuncPtr))(a0);
		result << ret;
	}
	template<>
	void InvokeImpl<void>(void* pThis, ArgumentStream& args, ArgumentStream& result) const
	{
		args.mReadPosition = 0;
		Method_Arg_Def(0);
		(((BindClass*)pThis)->*(FuncPtr))(a0);
	}
};

template<typename Result, typename BindClass, typename T0, typename T1>
struct VMethod2 : public RttiMethodBase
{
	typedef Result(BindClass::*MethodFun)(T0,T1);
	VMethod2(MethodFun fun, const char* name, const char* a0, const char* a1);
	MethodFun	FuncPtr;

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
		TR ret = (((BindClass*)pThis)->*(FuncPtr))(a0, a1);
		result << ret;
	}
	template<>
	void InvokeImpl<void>(void* pThis, ArgumentStream& args, ArgumentStream& result) const
	{
		args.mReadPosition = 0;
		Method_Arg_Def(0);
		Method_Arg_Def(1);
		(((BindClass*)pThis)->*(FuncPtr))(a0, a1);
	}
};

template<typename Result, typename BindClass, typename T0, typename T1, typename T2>
struct VMethod3 : public RttiMethodBase
{
	typedef Result(BindClass::*MethodFun)(T0, T1, T2);
	VMethod3(MethodFun fun, const char* name, const char* a0, const char* a1, const char* a2);
	MethodFun	FuncPtr;

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
		TR ret = (((BindClass*)pThis)->*(FuncPtr))(a0, a1, a2);
		result << ret;
	}
	template<>
	void InvokeImpl<void>(void* pThis, ArgumentStream& args, ArgumentStream& result) const
	{
		args.mReadPosition = 0;
		Method_Arg_Def(0);
		Method_Arg_Def(1);
		Method_Arg_Def(2);
		(((BindClass*)pThis)->*(FuncPtr))(a0, a1, a2);
	}
};

template<typename Result, typename BindClass, typename T0, typename T1, typename T2, typename T3>
struct VMethod4 : public RttiMethodBase
{
	typedef Result(BindClass::*MethodFun)(T0, T1, T2, T3);
	VMethod4(MethodFun fun, const char* name, const char* a0, const char* a1, const char* a2, const char* a3);
	MethodFun	FuncPtr;

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
		TR ret = (((BindClass*)pThis)->*(FuncPtr))(a0, a1, a2, a3);
		result << ret;
	}
	template<>
	void InvokeImpl<void>(void* pThis, ArgumentStream& args, ArgumentStream& result) const
	{
		args.mReadPosition = 0;
		Method_Arg_Def(0);
		Method_Arg_Def(1);
		Method_Arg_Def(2);
		Method_Arg_Def(3);
		(((BindClass*)pThis)->*(FuncPtr))(a0, a1, a2, a3);
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
	VConstructor1()
	{
		mArguments.push_back(&AuxRttiStruct<T0>::Instance);
	}
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
	VConstructor2()
	{
		mArguments.push_back(&AuxRttiStruct<T0>::Instance);
		mArguments.push_back(&AuxRttiStruct<T1>::Instance);
	}
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
		void SetValue(void* pThis, const T* v) const
		{
			if (AuxRttiStruct<T>::Instance.IsA(MemberType) == false)
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
		T* GetValueAddress(void* pThis) const
		{
			if (AuxRttiStruct<T>::Instance.IsA(MemberType) == false)
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
	~RttiStruct()
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
		auto desc = new(__FILE__, __LINE__) VMethod1<Result, Klass>(fun, name);
		
		Methods.push_back(desc);
		return desc;
	}

	template<typename Result, typename Klass, typename T0>
	RttiMethodBase* PushMethod1(typename VMethod1<Result, Klass, T0>::MethodFun fun, const char* name, const char* a0)
	{
		auto desc = new(__FILE__,__LINE__) VMethod1<Result, Klass, T0>(fun, name, a0);

		Methods.push_back(desc);
		return desc;
	}

	template<typename Result, typename Klass, typename T0, typename T1>
	RttiMethodBase* PushMethod2(typename VMethod2<Result, Klass, T0, T1>::MethodFun fun, const char* name, const char* a0, const char* a1)
	{
		auto desc = new(__FILE__, __LINE__) VMethod2<Result, Klass, T0, T1>(fun, name, a0, a1);

		Methods.push_back(desc);
		return desc;
	}

	template<typename Result, typename Klass, typename T0, typename T1, typename T2>
	RttiMethodBase* PushMethod3(typename VMethod3<Result, Klass, T0, T1, T2>::MethodFun fun, const char* name, const char* a0, const char* a1, const char* a2)
	{
		auto desc = new(__FILE__, __LINE__) VMethod3<Result, Klass, T0, T1, T2>(fun, name, a0, a1, a2);

		Methods.push_back(desc);
		return desc;
	}

	template<typename Result, typename Klass, typename T0, typename T1, typename T2, typename T3>
	RttiMethodBase* PushMethod4(typename VMethod4<Result, Klass, T0, T1, T2, T3>::MethodFun fun, const char* name, const char* a0, const char* a1, const char* a2, const char* a3)
	{
		auto desc = new(__FILE__, __LINE__) VMethod4<Result, Klass, T0, T1, T2, T3>(fun, name, a0, a1, a2, a3);

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

#define StructBegin(Type, ns) \
template<> \
struct AuxRttiStruct<Type> : public RttiStruct\
{\
	typedef Type	ThisStructType;\
	static AuxRttiStruct<Type>		Instance;\
	AuxRttiStruct()\
	{\
		Size = sizeof(Type);\
		Name = #Type;\
		NameSpace = #ns;\
		RttiStructManager::GetInstance()->RegStructType(GetFullName().c_str(), this);\
		IsEnum = AuxRttiEnum<Type>::IsEnum; \
	}\
	virtual void AssignOperator(void* pTar, const void* pSrc) const override\
	{\
		*(Type*)pTar = *(Type*)pSrc;\
	}\
	virtual void Init() override\
	{\
		RttiStruct::MemberDesc* __current_member = nullptr;\
		RttiMethodBase* __current_method = nullptr;\
		RttiConstructor* __current_constructor = nullptr;

#define  __vsizeof(_type, _name) sizeof(((_type*)nullptr)->_name)

#define AddClassMetaInfo(info) { this->AddMeta(info); }
#define AppendMemberMetaInfo(info) { if(__current_member!=nullptr){__current_member->AddMeta(info);} }
#define AppendMethodMetaInfo(info) { if(__current_method!=nullptr){__current_method->AddMeta(info);} }
#define AppendConstructorMetaInfo(info) { if(__current_constructor!=nullptr){__current_constructor->AddMeta(info);} }

#define StructMember(_name) \
		{\
			using declType = decltype(((ThisStructType*)nullptr)->_name);\
			using noPointerType = std::remove_pointer<declType>::type;\
			using realType = std::remove_extent<noPointerType>::type;\
			unsigned int size = __vsizeof(ThisStructType, _name);\
			__current_member = PushMember(&AuxRttiStruct<realType>::Instance, __voffsetof(ThisStructType, _name), size, size/sizeof(realType),\
					#_name, std::is_pointer<decltype(((ThisStructType*)nullptr)->_name)>::value);\
		}

#define StructMethod0(name) \
		{\
			using TResult = TFunction_traits<decltype(&ThisStructType::name)>::return_type; \
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
			__current_method = PushMethod2<TResult, ThisStructType, TA0, TA1>(funAddress, #name, #a0, #a1, #a2);\
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
			__current_method = PushMethod4<TResult, ThisStructType, TA0, TA1>(funAddress, #name, #a0, #a1, #a2, #a3);\
		}

#define StructMethodEx4(name, TResult, TA0, a0, TA1, a1, TA2, a2, TA3, a3) \
		{\
			__current_method = PushMethod4<TResult, ThisStructType, TA0, TA1, TA2, TA3>(&ThisStructType::name, #name, #a0, #a1, #a2, #a3);\
		}

#define StructConstructor0() \
		{\
			auto desc = new(__FILE__, __LINE__) VConstructor0<ThisStructType>();\
			Constructors.push_back(desc);\
			__current_constructor = desc;\
		}

#define StructConstructor1(_T0) \
		{\
			auto desc = new(__FILE__, __LINE__) VConstructor1<ThisStructType, _T0>();\
			Constructors.push_back(desc);\
			__current_constructor = desc;\
		}

#define StructConstructor2(_T0, _T1) \
		{\
			auto desc = new(__FILE__, __LINE__) VConstructor2<ThisStructType, _T0, _T1>();\
			Constructors.push_back(desc);\
			__current_constructor = desc;\
		}

#define StructEnd(ParentType) \
		ParentStructType = &AuxRttiStruct<ParentType>::Instance;;\
		RttiStruct::Init();\
	}\
};
#define StructImpl(Type) AuxRttiStruct<Type> AuxRttiStruct<Type>::Instance;

template<typename Result, typename Klass>
VMethod0<Result, Klass>::VMethod0(MethodFun fun, const char* name)
{
	FuncPtr = fun;
	Name = name;
	ResultType = &AuxRttiStruct<Result>::Instance;
	ThisObject = &AuxRttiStruct<Klass>::Instance;
}

template<typename Result, typename Klass, typename T0>
VMethod1<Result, Klass, T0>::VMethod1(MethodFun fun, const char* name, const char* a0)
{
	//static_assert(std::is_same<Result, decltype(fun)>::value);
	FuncPtr = fun;
	Name = name;
	ResultType = &AuxRttiStruct<Result>::Instance;
	ThisObject = &AuxRttiStruct<Klass>::Instance;

	Arguments.push_back(std::make_pair(a0,&AuxRttiStruct<T0>::Instance));
}

template<typename Result, typename Klass, typename T0, typename T1>
VMethod2<Result, Klass, T0, T1>::VMethod2(MethodFun fun, const char* name, const char* a0, const char* a1)
{
	//static_assert(std::is_same<Result, decltype(fun)>::value);
	FuncPtr = fun;
	Name = name;
	ResultType = &AuxRttiStruct<Result>::Instance;
	ThisObject = &AuxRttiStruct<Klass>::Instance;

	Arguments.push_back(std::make_pair(a0, &AuxRttiStruct<T0>::Instance));
	Arguments.push_back(std::make_pair(a1, &AuxRttiStruct<T1>::Instance));
}

template<typename Result, typename Klass, typename T0, typename T1, typename T2>
VMethod3<Result, Klass, T0, T1, T2>::VMethod3(MethodFun fun, const char* name, const char* a0, const char* a1, const char* a2)
{
	//static_assert(std::is_same<Result, decltype(fun)>::value);
	FuncPtr = fun;
	Name = name;
	ResultType = &AuxRttiStruct<Result>::Instance;
	ThisObject = &AuxRttiStruct<Klass>::Instance;

	Arguments.push_back(std::make_pair(a0, &AuxRttiStruct<T0>::Instance));
	Arguments.push_back(std::make_pair(a1, &AuxRttiStruct<T1>::Instance));
	Arguments.push_back(std::make_pair(a2, &AuxRttiStruct<T2>::Instance));
}

template<typename Result, typename Klass, typename T0, typename T1, typename T2, typename T3>
VMethod4<Result, Klass, T0, T1, T2, T3>::VMethod4(MethodFun fun, const char* name, const char* a0, const char* a1, const char* a2, const char* a3)
{
	//static_assert(std::is_same<Result, decltype(fun)>::value);
	FuncPtr = fun;
	Name = name;
	ResultType = &AuxRttiStruct<Result>::Instance;
	ThisObject = &AuxRttiStruct<Klass>::Instance;

	Arguments.push_back(std::make_pair(a0, &AuxRttiStruct<T0>::Instance));
	Arguments.push_back(std::make_pair(a1, &AuxRttiStruct<T1>::Instance));
	Arguments.push_back(std::make_pair(a2, &AuxRttiStruct<T2>::Instance));
	Arguments.push_back(std::make_pair(a3, &AuxRttiStruct<T3>::Instance));
}

NS_END