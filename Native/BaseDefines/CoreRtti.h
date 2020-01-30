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

struct RttiEnum
{
	std::string					Name;
	std::string					NameSpace;
	struct MemberDesc
	{
		std::string		Name;
		int				Value;
	};
	std::vector<MemberDesc>		Members;

	void PushMember(const char* name, int value)
	{
		MemberDesc tmp;
		tmp.Name = name;
		tmp.Value = value;
		Members.push_back(tmp);
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
struct RttiMethodBase
{
	std::string			Name;
	RttiStruct*			ThisObject;

	RttiStruct*			ResultType;
	std::vector<std::pair<std::string,RttiStruct*>>	Arguments;
	virtual void Invoke(void* pThis, ArgumentStream& args, ArgumentStream& result) const
	{
		
	}
};

struct RttiConstructor
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

template<typename Result, typename BindClass, typename T0>
struct VMethod1 : public RttiMethodBase
{
	typedef Result(BindClass::*MethodFun)(T0);
	VMethod1(VMethod1::MethodFun fun, const char* name, const char* a0);
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
		T0 a0 = T0();
		args >> a0;
		TR ret = (((BindClass*)pThis)->*(FuncPtr))(a0);
		result << ret;
	}
	template<>
	void InvokeImpl<void>(void* pThis, ArgumentStream& args, ArgumentStream& result) const
	{
		T0 a0 = T0();
		args >> a0;
		(((BindClass*)pThis)->*(FuncPtr))(a0);
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
		T0 a0 = T0();
		args >> a0;
		return new Klass(a0);
	}
};

struct RttiStruct
{
	RttiStruct*					ParentStructType;
	std::string					Name;
	std::string					NameSpace;
	unsigned int				Size;
	RttiEnum*					EnumDesc;
	bool						IsEnum;
	struct MemberDesc
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
	const RttiConstructor* FindConstructor(const std::vector<RttiStruct*>& args) const
	{
		for (auto i : Constructors)
		{
			if (i->MatchArgument(args))
				return i;
		}
		return nullptr;
	}
	void PushMember(RttiStruct* type, unsigned int offset, unsigned int size, unsigned int arrayElements, const char* name, bool isPointer);

	template<typename Result, typename Klass>
	void PushMethod0(typename VMethod0<Result, Klass>::MethodFun fun, const char* name)
	{
		auto desc = new(__FILE__, __LINE__) VMethod1<Result, Klass>(fun, name);
		
		Methods.push_back(desc);
	}

	template<typename Result, typename Klass, typename T0>
	void PushMethod1(typename VMethod1<Result, Klass, T0>::MethodFun fun, const char* name, const char* a0)
	{
		auto desc = new(__FILE__,__LINE__) VMethod1<Result, Klass, T0>(fun, name, a0);

		Methods.push_back(desc);
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
	{
#define  __vsizeof(_type, _name) sizeof(((_type*)nullptr)->_name)

#define StructMember(_name) \
		{\
			using declType = decltype(((ThisStructType*)nullptr)->_name);\
			using noPointerType = std::remove_pointer<declType>::type;\
			using realType = std::remove_extent<noPointerType>::type;\
			unsigned int size = __vsizeof(ThisStructType, _name);\
			PushMember(&AuxRttiStruct<realType>::Instance, __voffsetof(ThisStructType, _name), size, size/sizeof(realType),\
					#_name, std::is_pointer<decltype(((ThisStructType*)nullptr)->_name)>::value);\
		}

#define StructMethod0(fun, name) \
		PushMethod0(fun, #name);

//其实TR作为函数返回值类型，可以通过std::invoke_result萃取出来，不过考虑到c++17的支持度，以及没有完整引入function_traits，
//我们参数编译期获取也不完整，就让生成反射代码稍微复杂一点吧，反正最后最好是通过工具产生这些宏代码
#define StructMethod1(fun, name, TR, A0, a0) \
		PushMethod1<TR, ThisStructType, A0>(fun, #name, #a0);


#define StructConstructor0() \
		{\
			auto desc = new(__FILE__, __LINE__) VConstructor0<ThisStructType>();\
			Constructors.push_back(desc);\
		}

#define StructConstructor1(_T0) \
		{\
			auto desc = new(__FILE__, __LINE__) VConstructor1<ThisStructType, _T0>();\
			Constructors.push_back(desc);\
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

NS_END