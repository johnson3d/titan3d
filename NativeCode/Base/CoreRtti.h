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
#include "vfxtypes_nw.h"
#endif

#include "TypeUtility.h"
#include "string/vfxstring.h"

typedef unsigned long long	vIID;
typedef int	vBOOL;

NS_BEGIN

//#define NEW_INHEAD new(__FILE__, __LINE__)
#define NEW_INHEAD new

class VIUnknown;
struct RttiStruct;
typedef VNameString RttiNameString;
struct RttiType;
static const char* EngineNSString = "EngineNS";
static const char* EngineNSStringEx = "EngineNS::";

struct FArgumentStream
{
	FArgumentStream()
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
inline FArgumentStream& operator <<(FArgumentStream& stream, const ArgType& v)
{
	auto size = stream.mArguments.size();
	stream.mArguments.resize(size + sizeof(ArgType));
	memcpy(&stream.mArguments[size], &v, sizeof(ArgType));
	return stream;
}
template <typename ArgType>
inline FArgumentStream& operator >>(FArgumentStream& stream, ArgType& v)
{
	memcpy(&v, &stream.mArguments[stream.mReadPosition], sizeof(ArgType));
	stream.mReadPosition += sizeof(ArgType);
	return stream;
}
template <>
inline FArgumentStream& operator <<(FArgumentStream& stream, const std::string& v)
{
	auto len = (int)v.length();
	stream << len;
	auto size = stream.mArguments.size();
	stream.mArguments.resize(size + sizeof(char) * len);
	memcpy(&stream.mArguments[size], v.c_str(), sizeof(char) * len);
	return stream;
}
template <>
inline FArgumentStream& operator >>(FArgumentStream& stream, std::string& v)
{
	int len = 0;
	stream >> len;
	v.resize(len);
	memcpy(&v[0], &stream.mArguments[stream.mReadPosition], sizeof(char) * len);
	stream.mReadPosition += sizeof(char) * len;
	return stream;
}

class TR_CLASS()
	VIUnknownBase
{
protected:
	std::atomic<int>	RefCount;
public:
	VIUnknownBase(const VIUnknownBase & rh)
	{
		assert(false);
	}
	inline VIUnknownBase& operator = (const VIUnknownBase & rh) {
		assert(false);
		return *this;
	}
	VIUnknownBase()
	{
		RefCount = 1;
	}
	virtual ~VIUnknownBase() {}

	virtual long AddRef()
	{
		return ++RefCount;
	}

	virtual void Release()
	{
		RefCount--;
		if (RefCount == 0)
		{
			DeleteThis();
		}
		return;
	}
	virtual void DeleteThis();

	virtual const RttiStruct* GetRtti() const;

	template<class _CastType>
	_CastType* CastTo();
	VIUnknownBase* CastTo(RttiStruct* type);

	virtual void OnPropertyChanged([[maybe_unused]] const RttiNameString& name, [[maybe_unused]] const RttiType& type) {}
};

template<typename _Type>
struct VTypeHelper
{
	enum
	{
		TypeSizeOf = sizeof(_Type),
	};
	static void AssignOperator(void* pTar, void*& pSrc)
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

template<>
struct VTypeHelper<void>
{
	enum
	{
		TypeSizeOf = 0,
	};
	static void AssignOperator(void* pTar, void*& pSrc)
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

#define ENGINE_RTTI(name) virtual const RttiStruct* GetRtti() const override\
	{ \
		return AuxRttiStruct<name>::GetClassObject(); \
	}\
	TR_DECL(name)

#define ENGINE_RTTI_IMPL(name) //RttiStruct AuxRttiStruct<name>::Instance;

struct FMetaBase : public VIUnknownBase
{
	virtual const RttiStruct* GetRtti() const = 0;
};

struct RttiMetaInfo
{
	std::vector<FMetaBase*> MetaInfos;
	void AddMeta(FMetaBase* value)
	{
		for (auto& i : MetaInfos)
		{
			if (i == value)
				return;
		}
		value->AddRef();
		MetaInfos.push_back(value);
	}
	FMetaBase* GetFirstMeta(RttiStruct* rtti) const
	{
		for (auto& i : MetaInfos)
		{
			if (i->GetRtti() == rtti)
				return i;
		}
		return nullptr;
	}
	template<class _MetaType>
	_MetaType* GetFirstMeta() const;
	void CleanupMetas()
	{
		for (auto& i : MetaInfos)
		{
			i->Release();
		}
		MetaInfos.clear();
	}
	virtual ~RttiMetaInfo()
	{
		CleanupMetas();
	}
};

struct TR_CLASS()
	RttiEnum : public RttiMetaInfo
{
	RttiNameString				Name;
	RttiNameString				NameSpace;
	struct EnumMemberDesc : public RttiMetaInfo
	{
		RttiNameString		Name;
		int					Value;
	};
	std::vector<EnumMemberDesc>		Members;

	EnumMemberDesc* PushMember(const char* name, int value)
	{
		EnumMemberDesc tmp;
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
		std::string result = NameSpace;
		return result + "::" + Name.c_str();
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

class TR_CLASS()
	RttiEnumManager
{
	std::vector<RttiEnum*>				AllEnumTyps;
public:
	void RegEnumType(const char* name, RttiEnum * type);
	static RttiEnumManager* GetInstance();

	void BuildRtti();
	void FinalCleanup();

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

#define EnumEnd(name, ns) EnumEnd_Impl(name, ns)
#define EnumEnd_Impl(name, ns) \
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

struct TR_CLASS()
RttiType
{
	/*RttiType()
	{
		Type = nullptr;
		NumOfPointer = 0;
		IsRefer = false;
		ArrayElements = 0;
	}*/
	RttiStruct*			Type;
	int					NumOfPointer;
	bool				IsRefer;
	unsigned int		ArrayElements;
	bool operator == (const RttiType& rh) const {
		if (IsRefer != rh.IsRefer ||
			NumOfPointer != rh.NumOfPointer ||
			Type != rh.Type)
		{
			return false;
		}
		return true;
	}
	bool operator != (const RttiType& rh) const {
		return !(this->operator==(rh));
	}
	template<typename _Type>
	void BuildType();
	template<typename _Type>
	bool IsType() const;
	void SetValue(void* pTar, const void* pSrc) const;
};

struct TR_CLASS()
RttiParameter
{
	RttiNameString		Name;
	RttiType			ParameterType;
	
	template<typename _Type>
	static RttiParameter BuildParameter(const char* name);
};

struct TR_CLASS()
	RttiMethodBase : public RttiMetaInfo
{
	RttiMethodBase()
	{
		IsStatic = false;
		IsConst = false;
		ThisClass = nullptr;
	}
	RttiNameString		Name;
	RttiStruct* ThisClass;
	RttiType			ResultType;
	std::vector<RttiParameter>	Arguments;
	bool				IsStatic;
	bool				IsConst;

	virtual void* GetFunctionPtr() = 0;
	template <class _ThisType, class ResultType, class... ArgTypes>
	ResultType UnsafeInvokeArgs(_ThisType * pThis, ArgTypes... args) {
		typedef ResultType(_ThisType::* MyFunctionType)(ArgTypes... args);
		auto func = (MyFunctionType)GetFunctionPtr();
		return ((pThis)->*func)(args...);
	}
	virtual void Invoke(void* pThis, FArgumentStream & args, FArgumentStream & result) const
	{

	}
	bool MatchArgument(const RttiStruct * returnType, const std::vector<RttiStruct*>&args) const
	{
		if (returnType != ResultType.Type)
			return false;
		if (args.size() != Arguments.size())
			return false;
		for (size_t i = 0; i < args.size(); i++)
		{
			if (args[i] != Arguments[i].ParameterType.Type)
				return false;
		}
		return true;
	}
};

struct TR_CLASS()
	RttiConstructor : public RttiMetaInfo
{
	template <class _ThisType, class... ArgTypes>
	static _ThisType* CreateInstance(ArgTypes... args)
	{
		return new _ThisType(args...);
	}
	std::vector<RttiParameter> mArguments;
	bool MatchArgument(const std::vector<RttiType>&args) const
	{
		if (args.size() != mArguments.size())
			return false;
		for (size_t i = 0; i < args.size(); i++)
		{
			if (args[i] != mArguments[i].ParameterType)
				return false;
		}
		return true;
	}
	virtual void* CreateInstance(FArgumentStream & args) const
	{
		return nullptr;
	}
};

struct TR_CLASS()
	RttiTypeConverter
{
	template<class SuperType, class ThisType>
	SuperType* CastTo(ThisType* ptr) const
	{
		return static_cast<SuperType*>(ptr);
	}
	template<class SuperType, class ThisType>
	ThisType* DownCastTo(SuperType* ptr) const
	{
		if constexpr (std::is_polymorphic<SuperType>::value && std::is_polymorphic<ThisType>::value)
		{
			return dynamic_cast<ThisType*>(ptr);
		}
		else if constexpr (!std::is_polymorphic<SuperType>::value && !std::is_polymorphic<ThisType>::value)
		{
			return (ThisType*)ptr;
		}
		else
		{
			return nullptr;
		}
	}
	virtual ~RttiTypeConverter() {};
	virtual void* CastInvoke(void* ptr) const = 0;
	virtual void* DownCastInvoke(void* ptr) const = 0;
};

template<class _Type, class _BaseType>
struct AuxRttiTypeConverter : public RttiTypeConverter
{
	virtual void* CastInvoke(void* ptr) const override
	{
		return CastTo<_BaseType,_Type>((_Type*)ptr);
	}
	virtual void* DownCastInvoke(void* ptr) const override
	{
		return DownCastTo<_BaseType, _Type>((_Type*)ptr);
	}
};

struct TR_CLASS()
	MemberDesc : public RttiMetaInfo
{
	RttiStruct*			DeclareClass;
	RttiType			MemberType;
	unsigned int		Offset;
	unsigned int		Size;
	RttiNameString		MemberName;

	template<typename T>
	inline void SetValue(void* pThis, const T & v) const;
	template<typename T>
	inline T* GetValueAddress(void* pThis) const;
	inline void* GetValueAddress(void* pThis) const
	{
		return ((char*)pThis + Offset);
	}
	void SetValue(void* pHost, const void* pValueAddress) const;
};

struct TR_CLASS()
	RttiStruct : public RttiMetaInfo
{
	RttiNameString				Name;
	RttiNameString				NameSpace;
	unsigned int				Size;
	RttiEnum* EnumDesc;
	bool						IsEnum;

	typedef void (FnAssign)(void* pTar, const void* pSrc);

	std::function<FnAssign> Assignment;

	struct FBaseType
	{
		RttiStruct*			ClassType;
		RttiTypeConverter*	TypeConverter;
	};
	std::vector<FBaseType>		BaseTypes;
	struct TupleVisitor
	{
		template<typename T, typename ArgType>
		static void VisitElement(void* pArg, const T&, const ArgType*);
	};
	template<class Type>
	void BuildClassInfo(const char* name, const char* space)
	{
		Name = name;
		if (space != nullptr)
			NameSpace = space;
		Size = sizeof(Type);
		IsEnum = AuxRttiEnum<Type>::IsEnum;
		Assignment = [](void* pTar, const void* pSrc)
		{
			*(Type*)pTar = *(Type*)pSrc;
		};
	}
	template<class ThisType, class... ArgTypes>
	void BuildBaseTypes()
	{
		ForeachTypeList<ThisType, TupleVisitor, sizeof...(ArgTypes), ArgTypes...>::Visit(this, (ThisType*)nullptr);
	}

	std::vector<MemberDesc*>		Members;
	std::vector<RttiMethodBase*>	Methods;
	std::vector<RttiConstructor*>	Constructors;
	RttiStruct();
	virtual ~RttiStruct()
	{
		Cleanup();
	}
	void Cleanup()
	{
		for (auto i : Members)
		{
			delete i;
		}
		Members.clear();

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

		for (auto i : BaseTypes)
		{
			delete i.TypeConverter;
		}
		Constructors.clear();

		CleanupMetas();
	}
	bool IsA(RttiStruct * pTar);
	vBOOL GetIsEnum() const {
		return IsEnum ? 1 : 0;
	}
	const char* GetName() const {
		return Name.c_str();
	}
	const char* GetNameSpace() const {
		return NameSpace.c_str();
	}
	std::string GetFullName() {
		std::string result = NameSpace;
		return result + "::" + Name.c_str();
	}
	unsigned int GetSize() const {
		return Size;
	}
	unsigned int GetMemberNumber() const {
		return (unsigned int)Members.size();
	}
	unsigned int FindMemberIndex(const char* name) const {
		for (unsigned int i = 0; i < (unsigned int)Members.size(); i++)
		{
			if (Members[i]->MemberName == name)
				return i;
		}
		return -1;
	}
	const RttiType* GetMemberType(unsigned int index) const {
		return &Members[index]->MemberType;
	}
	const char* GetMemberName(unsigned int index) const {
		return Members[index]->MemberName.c_str();
	}
	const MemberDesc* FindMember(const char* name) const {
		for (const auto& i : Members)
		{
			if (i->MemberName == name)
				return i;
		}
		return nullptr;
	}
	unsigned int GetMethodNumber() const {
		return (unsigned int)Methods.size();
	}
	unsigned int FindMethodIndex(const char* name) const {
		for (unsigned int i = 0; i < (unsigned int)Methods.size(); i++)
		{
			if (Methods[i]->Name == name)
				return i;
		}
		return -1;
	}
	const RttiMethodBase* FindMethod(const char* name) const {
		for (auto i : Methods)
		{
			if (i->Name == name)
				return i;
		}
		return nullptr;
	}
	const RttiMethodBase* FindMethodWithArguments(const char* name, const RttiStruct * returnType, const std::vector<RttiStruct*>&args) const {
		for (auto i : Methods)
		{
			if (i->Name == name && i->MatchArgument(returnType, args))
			{
				return i;
			}
		}
		return nullptr;
	}
	const RttiConstructor* FindConstructor(const std::vector<RttiType>& args) const
	{
		for (auto i : Constructors)
		{
			if (i->MatchArgument(args))
				return i;
		}
		return nullptr;
	}

	const RttiTypeConverter* FindBaseConverter(RttiStruct* baseType)
	{
		for (auto i : BaseTypes)
		{
			if (i.ClassType == baseType)
			{
				return i.TypeConverter;
			}
		}
		return nullptr;
	}
	void* CastSuper(void* ptr, const RttiStruct* baseType) const
	{
		if(this == baseType)
			return ptr;
		for (auto i : BaseTypes)
		{
			auto pBasePtr = i.TypeConverter->CastInvoke(ptr);
			if (i.ClassType == baseType)
			{
				return pBasePtr;
			}
			else
			{
				auto result = i.ClassType->CastSuper(pBasePtr, baseType);
				if (result != nullptr)
					return result;
			}
		}
		return nullptr;
	}
	void* DownCast(void* ptr, const RttiStruct* fromType) const
	{
		if (this == fromType)
			return ptr;
		for (auto i : BaseTypes)
		{
			if (i.ClassType == fromType)
			{
				return i.TypeConverter->DownCastInvoke(ptr);
			}
			else
			{
				auto result = i.ClassType->DownCast(ptr, fromType);
				if (result != nullptr)
					return i.TypeConverter->DownCastInvoke(result);
			}
		}
		return nullptr;
	}
	template<typename _MemberType>
	MemberDesc* PushMember(unsigned int offset, unsigned int size, const char* name)
	{
		//using noConstType = typename std::remove_const<_MemberType>::type;
		//using realType = typename std::remove_all_extents<noConstType>::type;
		//using pureType = typename remove_all_ref_ptr<realType>::type;

		MemberDesc* desc = new MemberDesc();
		desc->MemberType.BuildType<_MemberType>();
		desc->MemberName = name;
		desc->Offset = offset;
		desc->Size = size;
		desc->DeclareClass = this;

		Members.push_back(desc);
		return desc;
	}
	RttiMethodBase* PushMethod(RttiMethodBase * method) {
		Methods.push_back(method);
		return method;
	}
	RttiConstructor* PushConstructor(RttiConstructor* method) {
		Constructors.push_back(method);
		return method;
	}

	virtual void Init();
};

template<typename Type>
struct AuxRttiStruct
{
	static RttiStruct		Instance;
	static RttiStruct* GetClassObject() {
		return &Instance;
	}
};

template<typename Type>
RttiStruct		AuxRttiStruct<Type>::Instance;

template<typename Type>
RttiStruct* GetClassObject() {
	return AuxRttiStruct<Type>::GetClassObject();
}

template<class _MetaType>
_MetaType* RttiMetaInfo::GetFirstMeta() const
{
	return (_MetaType*)GetFirstMeta(GetClassObject<_MetaType>());
}

template<class _CastType>
_CastType* VIUnknownBase::CastTo()
{
	return (_CastType*)AuxRttiStruct<_CastType>::GetClassObject()->DownCast(this, GetRtti());
}

template<typename T, typename ArgType>
void RttiStruct::TupleVisitor::VisitElement(void* pArg, const T&, const ArgType*)
{
	auto kls = (RttiStruct*)pArg;

	RttiStruct::FBaseType baseType;
	baseType.ClassType = AuxRttiStruct<T>::GetClassObject();
	baseType.TypeConverter = new AuxRttiTypeConverter<ArgType, T>();
	kls->BaseTypes.push_back(baseType);
}

struct AuxRttiBuilderBase
{
	AuxRttiBuilderBase();
	virtual void BuildRtti() = 0;
	template<class Type>
	void BuildClassInfo(RttiStruct* rtti, const char* name, const char* space)
	{
		rtti->BuildClassInfo<Type>(name, space);
	}
};

template<class Type>
struct AuxRttiBuilder;

struct RttiStruct;

class TR_CLASS()
RttiStructManager
{
	std::vector<AuxRttiBuilderBase*>	StructBuilders;
	std::vector<RttiStruct*>			AllStructTyps;
public:
	void RegStructBuilder(AuxRttiBuilderBase* builder);
	void RegStructType(RttiStruct* type);
	static RttiStructManager* GetInstance();
	
	~RttiStructManager();
	void BuildRtti();
	void FinalCleanup();

	RttiStruct* FindStruct(const char* name);
	unsigned int GetStructNumber() {
		return (unsigned int)AllStructTyps.size();
	}
	RttiStruct* GetStruct(unsigned int index) {
		return AllStructTyps[index];
	}
};

template <class _ThisType, class _ResultType, class... ArgTypes>
struct RttiMethodImpl : public RttiMethodBase
{
	typedef _ResultType(_ThisType::* FunctionType)(ArgTypes... args);
	typedef _ResultType(_ThisType::* FunctionTypeConst)(ArgTypes... args) const;
	typedef _ResultType(*FunctionTypeStatic)(ArgTypes... args);
	union {
		void* FunAddress;
		FunctionType FunctionPtr;
		FunctionTypeConst ConstFunctionPtr;
		FunctionTypeStatic StaticFunctionPtr;
	};

	struct TupleVisitor
	{
		template<typename T>
		static void VisitElement(void* pArg, const T&, const void*)
		{
			auto method = (RttiMethodBase*)pArg;
			RttiType tmp;
			tmp.IsRefer = VIsReferType<T>::ResultValue;
			tmp.NumOfPointer = TypePointerCounter<T>::Value;
			tmp.Type = AuxRttiStruct<typename remove_all_ref_ptr<T>::type>::GetClassObject();
			RttiParameter prm;
			//prm.Name = 
			prm.ParameterType = tmp;
			method->Arguments.push_back(prm);
		}
	};
	RttiMethodImpl(const char* name, FunctionType fn)
	{
		Name = name;
		IsConst = false;
		FunctionPtr = fn;
		ThisClass = AuxRttiStruct<typename remove_all_ref_ptr<_ThisType>::type>::GetClassObject();

		ResultType.IsRefer = VIsReferType<_ResultType>::ResultValue;
		ResultType.NumOfPointer = TypePointerCounter<_ResultType>::Value;
		ResultType.Type = AuxRttiStruct<typename remove_all_ref_ptr<_ResultType>::type>::GetClassObject();

		Arguments.clear();
		ForeachTypeList<void, TupleVisitor, sizeof...(ArgTypes), ArgTypes...>::Visit(this, nullptr);
	}
	RttiMethodImpl(const char* name, FunctionTypeConst fn)
	{
		Name = name;
		IsConst = true;
		ConstFunctionPtr = fn;
		ThisClass = AuxRttiStruct<typename remove_all_ref_ptr<_ThisType>::type>::GetClassObject();

		ResultType.IsRefer = VIsReferType<_ResultType>::ResultValue;
		ResultType.NumOfPointer = TypePointerCounter<_ResultType>::Value;
		ResultType.Type = AuxRttiStruct<typename remove_all_ref_ptr<_ResultType>::type>::GetClassObject();

		Arguments.clear();
		ForeachTypeList<void, TupleVisitor, sizeof...(ArgTypes), ArgTypes...>::Visit(this, nullptr);
	}
	RttiMethodImpl(const char* name, FunctionTypeStatic fn)
	{
		Name = name;
		IsStatic = true;
		StaticFunctionPtr = fn;
		ThisClass = AuxRttiStruct<typename remove_all_ref_ptr<_ThisType>::type>::GetClassObject();

		ResultType.IsRefer = VIsReferType<_ResultType>::ResultValue;
		ResultType.NumOfPointer = TypePointerCounter<_ResultType>::Value;
		ResultType.Type = AuxRttiStruct<typename remove_all_ref_ptr<_ResultType>::type>::GetClassObject();

		Arguments.clear();
		ForeachTypeList<void, TupleVisitor, sizeof...(ArgTypes), ArgTypes...>::Visit(this, nullptr);
	}
	virtual void* GetFunctionPtr() override
	{
		return FunAddress;
	}
	_ResultType InvokeArgs([[maybe_unused]] void* pThis, ArgTypes... args)
	{
		if (IsConst)
		{
			return (((_ThisType*)pThis)->*ConstFunctionPtr)(args...);
		}
		else
		{
			return (((_ThisType*)pThis)->*FunctionPtr)(args...);
		}
	}
	virtual void Invoke([[maybe_unused]] void* pThis, [[maybe_unused]] FArgumentStream& args, [[maybe_unused]] FArgumentStream& result) const override
	{
		if constexpr (sizeof...(ArgTypes) == 0)
		{
			if (IsConst)
			{
				if constexpr (std::is_same<_ResultType, void>::value)
				{
					(((_ThisType*)pThis)->*ConstFunctionPtr)();
				}
				else
				{
					result << (((_ThisType*)pThis)->*ConstFunctionPtr)();
				}
			}
			else if (IsStatic)
			{
				if constexpr (std::is_same<_ResultType, void>::value)
				{
					StaticFunctionPtr();
				}
				else
				{
					result << StaticFunctionPtr();
				}
			}
			else
			{
				if constexpr (std::is_same<_ResultType, void>::value)
				{
					(((_ThisType*)pThis)->*FunctionPtr)();
				}
				else
				{
					result << (((_ThisType*)pThis)->*FunctionPtr)();
				}
			}
		}
		else if constexpr (sizeof...(ArgTypes) == 1)
		{
			typedef typename  VTypeList_GetAt<0, VTypeList<ArgTypes...>>::ResultType A0Type;
			auto a0 = typename VRefAsPtr<A0Type>::ResultType();
			args >> a0;
			if (IsConst)
			{
				if constexpr (std::is_same<_ResultType, void>::value)
				{
					(((_ThisType*)pThis)->*ConstFunctionPtr)(a0);
				}
				else
				{
					result << (((_ThisType*)pThis)->*ConstFunctionPtr)((A0Type)a0);
				}
			}
			else if (IsStatic)
			{
				if constexpr (std::is_same<_ResultType, void>::value)
				{
					StaticFunctionPtr((A0Type)a0);
				}
				else
				{
					result << StaticFunctionPtr((A0Type)a0);
				}
			}
			else
			{
				if constexpr (std::is_same<_ResultType, void>::value)
				{
					(((_ThisType*)pThis)->*FunctionPtr)((A0Type)a0);
				}
				else
				{
					result << (((_ThisType*)pThis)->*FunctionPtr)((A0Type)a0);
				}
			}
		}
		else if constexpr (sizeof...(ArgTypes) == 2)
		{
			typedef typename VTypeList_GetAt<0, VTypeList<ArgTypes...>>::ResultType A0Type;
			typedef typename VTypeList_GetAt<1, VTypeList<ArgTypes...>>::ResultType A1Type;
			auto a0 = typename VRefAsPtr<A0Type>::ResultType();
			auto a1 = typename VRefAsPtr<A1Type>::ResultType();
			args >> a0;
			args >> a1;
			#define CallArg2 (A0Type)a0, (A1Type)a1
			if (IsConst)
			{
				if constexpr (std::is_same<_ResultType, void>::value)
				{
					(((_ThisType*)pThis)->*ConstFunctionPtr)(CallArg2);
				}
				else
				{
					result << (((_ThisType*)pThis)->*ConstFunctionPtr)(CallArg2);
				}
			}
			else if (IsStatic)
			{
				if constexpr (std::is_same<_ResultType, void>::value)
				{
					StaticFunctionPtr(CallArg2);
				}
				else
				{
					result << StaticFunctionPtr(CallArg2);
				}
			}
			else
			{
				if constexpr (std::is_same<_ResultType, void>::value)
				{
					(((_ThisType*)pThis)->*FunctionPtr)(CallArg2);
				}
				else
				{
					result << (((_ThisType*)pThis)->*FunctionPtr)(CallArg2);
				}
			}
		}
		else if constexpr (sizeof...(ArgTypes) == 3)
		{
			typedef typename VTypeList_GetAt<0, VTypeList<ArgTypes...>>::ResultType A0Type;
			typedef typename VTypeList_GetAt<1, VTypeList<ArgTypes...>>::ResultType A1Type;
			typedef typename VTypeList_GetAt<2, VTypeList<ArgTypes...>>::ResultType A2Type;
			auto a0 = typename VRefAsPtr<A0Type>::ResultType();
			auto a1 = typename VRefAsPtr<A1Type>::ResultType();
			auto a2 = typename VRefAsPtr<A2Type>::ResultType();
			args >> a0;
			args >> a1;
			args >> a2;
			#define CallArg3 (A0Type)a0, (A1Type)a1, (A2Type)a2
			if (IsConst)
			{
				if constexpr (std::is_same<_ResultType, void>::value)
				{
					(((_ThisType*)pThis)->*ConstFunctionPtr)(CallArg3);
				}
				else
				{
					result << (((_ThisType*)pThis)->*ConstFunctionPtr)(CallArg3);
				}
			}
			else if (IsStatic)
			{
				if constexpr (std::is_same<_ResultType, void>::value)
				{
					StaticFunctionPtr(CallArg3);
				}
				else
				{
					result << StaticFunctionPtr(CallArg3);
				}
			}
			else
			{
				if constexpr (std::is_same<_ResultType, void>::value)
				{
					(((_ThisType*)pThis)->*FunctionPtr)(CallArg3);
				}
				else
				{
					result << (((_ThisType*)pThis)->*FunctionPtr)(CallArg3);
				}
			}
		}
		else if constexpr (sizeof...(ArgTypes) == 4)
		{
			typedef typename VTypeList_GetAt<0, VTypeList<ArgTypes...>>::ResultType A0Type;
			typedef typename VTypeList_GetAt<1, VTypeList<ArgTypes...>>::ResultType A1Type;
			typedef typename VTypeList_GetAt<2, VTypeList<ArgTypes...>>::ResultType A2Type;
			typedef typename VTypeList_GetAt<3, VTypeList<ArgTypes...>>::ResultType A3Type;
			auto a0 = typename VRefAsPtr<A0Type>::ResultType();
			auto a1 = typename VRefAsPtr<A1Type>::ResultType();
			auto a2 = typename VRefAsPtr<A2Type>::ResultType();
			auto a3 = typename VRefAsPtr<A3Type>::ResultType();
			args >> a0;
			args >> a1;
			args >> a2;
			args >> a3;
			#define CallArg4 (A0Type)a0, (A1Type)a1, (A2Type)a2, (A3Type)a3
			if (IsConst)
			{
				if constexpr (std::is_same<_ResultType, void>::value)
				{
					(((_ThisType*)pThis)->*ConstFunctionPtr)(CallArg4);
				}
				else
				{
					result << (((_ThisType*)pThis)->*ConstFunctionPtr)(CallArg4);
				}
			}
			else if (IsStatic)
			{
				if constexpr (std::is_same<_ResultType, void>::value)
				{
					StaticFunctionPtr(CallArg4);
				}
				else
				{
					result << StaticFunctionPtr(CallArg4);
				}
			}
			else
			{
				if constexpr (std::is_same<_ResultType, void>::value)
				{
					(((_ThisType*)pThis)->*FunctionPtr)(CallArg4);
				}
				else
				{
					result << (((_ThisType*)pThis)->*FunctionPtr)(CallArg4);
				}
			}
		}
		else if constexpr (sizeof...(ArgTypes) == 5)
		{
			typedef typename VTypeList_GetAt<0, VTypeList<ArgTypes...>>::ResultType A0Type;
			typedef typename VTypeList_GetAt<1, VTypeList<ArgTypes...>>::ResultType A1Type;
			typedef typename VTypeList_GetAt<2, VTypeList<ArgTypes...>>::ResultType A2Type;
			typedef typename VTypeList_GetAt<3, VTypeList<ArgTypes...>>::ResultType A3Type;
			typedef typename VTypeList_GetAt<4, VTypeList<ArgTypes...>>::ResultType A4Type;
			auto a0 = typename VRefAsPtr<A0Type>::ResultType();
			auto a1 = typename VRefAsPtr<A1Type>::ResultType();
			auto a2 = typename VRefAsPtr<A2Type>::ResultType();
			auto a3 = typename VRefAsPtr<A3Type>::ResultType();
			auto a4 = typename VRefAsPtr<A4Type>::ResultType();
			args >> a0;
			args >> a1;
			args >> a2;
			args >> a3;
			args >> a4;
			#define CallArg5 (A0Type)a0, (A1Type)a1, (A2Type)a2, (A3Type)a3, (A4Type)a4
			if (IsConst)
			{
				if constexpr (std::is_same<_ResultType, void>::value)
				{
					(((_ThisType*)pThis)->*ConstFunctionPtr)((A0Type)a0, (A1Type)a1, (A2Type)a2, (A2Type)a3, (A2Type)a4);
				}
				else
				{
					result << (((_ThisType*)pThis)->*ConstFunctionPtr)((A0Type)a0, (A1Type)a1, (A2Type)a2, (A2Type)a3, (A2Type)a4);
				}
			}
			else if (IsStatic)
			{
				if constexpr (std::is_same<_ResultType, void>::value)
				{
					StaticFunctionPtr((A0Type)a0, (A1Type)a1, (A2Type)a2, (A2Type)a3, (A2Type)a4);
				}
				else
				{
					result << StaticFunctionPtr((A0Type)a0, (A1Type)a1, (A2Type)a2, (A2Type)a3, (A2Type)a4);
				}
			}
			else
			{
				if constexpr (std::is_same<_ResultType, void>::value)
				{
					(((_ThisType*)pThis)->*FunctionPtr)((A0Type)a0, (A1Type)a1, (A2Type)a2, (A2Type)a3, (A2Type)a4);
				}
				else
				{
					result << (((_ThisType*)pThis)->*FunctionPtr)((A0Type)a0, (A1Type)a1, (A2Type)a2, (A2Type)a3, (A2Type)a4);
				}
			}
		}
		else if constexpr (sizeof...(ArgTypes) == 6)
		{
			typedef typename VTypeList_GetAt<0, VTypeList<ArgTypes...>>::ResultType A0Type;
			typedef typename VTypeList_GetAt<1, VTypeList<ArgTypes...>>::ResultType A1Type;
			typedef typename VTypeList_GetAt<2, VTypeList<ArgTypes...>>::ResultType A2Type;
			typedef typename VTypeList_GetAt<3, VTypeList<ArgTypes...>>::ResultType A3Type;
			typedef typename VTypeList_GetAt<4, VTypeList<ArgTypes...>>::ResultType A4Type;
			typedef typename VTypeList_GetAt<5, VTypeList<ArgTypes...>>::ResultType A5Type;
			auto a0 = typename VRefAsPtr<A0Type>::ResultType();
			auto a1 = typename VRefAsPtr<A1Type>::ResultType();
			auto a2 = typename VRefAsPtr<A2Type>::ResultType();
			auto a3 = typename VRefAsPtr<A3Type>::ResultType();
			auto a4 = typename VRefAsPtr<A4Type>::ResultType();
			auto a5 = typename VRefAsPtr<A5Type>::ResultType();
			args >> a0;
			args >> a1;
			args >> a2;
			args >> a3;
			args >> a4;
			args >> a5;
			#define CallArg6 (A0Type)a0, (A1Type)a1, (A2Type)a2, (A3Type)a3, (A4Type)a4, (A5Type)a5
			if (IsConst)
			{
				if constexpr (std::is_same<_ResultType, void>::value)
				{
					(((_ThisType*)pThis)->*ConstFunctionPtr)(CallArg6);
				}
				else
				{
					result << (((_ThisType*)pThis)->*ConstFunctionPtr)(CallArg6);
				}
			}
			else if (IsStatic)
			{
				if constexpr (std::is_same<_ResultType, void>::value)
				{
					StaticFunctionPtr(CallArg6);
				}
				else
				{
					result << StaticFunctionPtr(CallArg6);
				}
			}
			else
			{
				if constexpr (std::is_same<_ResultType, void>::value)
				{
					(((_ThisType*)pThis)->*FunctionPtr)(CallArg6);
				}
				else
				{
					result << (((_ThisType*)pThis)->*FunctionPtr)(CallArg6);
				}
			}
		}
		else if constexpr (sizeof...(ArgTypes) == 7)
		{
			typedef typename VTypeList_GetAt<0, VTypeList<ArgTypes...>>::ResultType A0Type;
			typedef typename VTypeList_GetAt<1, VTypeList<ArgTypes...>>::ResultType A1Type;
			typedef typename VTypeList_GetAt<2, VTypeList<ArgTypes...>>::ResultType A2Type;
			typedef typename VTypeList_GetAt<3, VTypeList<ArgTypes...>>::ResultType A3Type;
			typedef typename VTypeList_GetAt<4, VTypeList<ArgTypes...>>::ResultType A4Type;
			typedef typename VTypeList_GetAt<5, VTypeList<ArgTypes...>>::ResultType A5Type;
			typedef typename VTypeList_GetAt<6, VTypeList<ArgTypes...>>::ResultType A6Type;
			auto a0 = typename VRefAsPtr<A0Type>::ResultType();
			auto a1 = typename VRefAsPtr<A1Type>::ResultType();
			auto a2 = typename VRefAsPtr<A2Type>::ResultType();
			auto a3 = typename VRefAsPtr<A3Type>::ResultType();
			auto a4 = typename VRefAsPtr<A4Type>::ResultType();
			auto a5 = typename VRefAsPtr<A5Type>::ResultType();
			auto a6 = typename VRefAsPtr<A6Type>::ResultType();
			args >> a0;
			args >> a1;
			args >> a2;
			args >> a3;
			args >> a4;
			args >> a5;
			args >> a6;
			#define CallArg7 (A0Type)a0, (A1Type)a1, (A2Type)a2, (A3Type)a3, (A4Type)a4, (A5Type)a5, (A6Type)a6
			if (IsConst)
			{
				if constexpr (std::is_same<_ResultType, void>::value)
				{
					(((_ThisType*)pThis)->*ConstFunctionPtr)(CallArg7);
				}
				else
				{
					result << (((_ThisType*)pThis)->*ConstFunctionPtr)(CallArg7);
				}
			}
			else if (IsStatic)
			{
				if constexpr (std::is_same<_ResultType, void>::value)
				{
					StaticFunctionPtr(CallArg7);
				}
				else
				{
					result << StaticFunctionPtr(CallArg7);
				}
			}
			else
			{
				if constexpr (std::is_same<_ResultType, void>::value)
				{
					(((_ThisType*)pThis)->*FunctionPtr)(CallArg7);
				}
				else
				{
					result << (((_ThisType*)pThis)->*FunctionPtr)(CallArg7);
				}
			}
		}
		else if constexpr (sizeof...(ArgTypes) == 8)
		{
			typedef typename VTypeList_GetAt<0, VTypeList<ArgTypes...>>::ResultType A0Type;
			typedef typename VTypeList_GetAt<1, VTypeList<ArgTypes...>>::ResultType A1Type;
			typedef typename VTypeList_GetAt<2, VTypeList<ArgTypes...>>::ResultType A2Type;
			typedef typename VTypeList_GetAt<3, VTypeList<ArgTypes...>>::ResultType A3Type;
			typedef typename VTypeList_GetAt<4, VTypeList<ArgTypes...>>::ResultType A4Type;
			typedef typename VTypeList_GetAt<5, VTypeList<ArgTypes...>>::ResultType A5Type;
			typedef typename VTypeList_GetAt<6, VTypeList<ArgTypes...>>::ResultType A6Type;
			typedef typename VTypeList_GetAt<7, VTypeList<ArgTypes...>>::ResultType A7Type;
			auto a0 = typename VRefAsPtr<A0Type>::ResultType();
			auto a1 = typename VRefAsPtr<A1Type>::ResultType();
			auto a2 = typename VRefAsPtr<A2Type>::ResultType();
			auto a3 = typename VRefAsPtr<A3Type>::ResultType();
			auto a4 = typename VRefAsPtr<A4Type>::ResultType();
			auto a5 = typename VRefAsPtr<A5Type>::ResultType();
			auto a6 = typename VRefAsPtr<A6Type>::ResultType();
			auto a7 = typename VRefAsPtr<A7Type>::ResultType();
			args >> a0;
			args >> a1;
			args >> a2;
			args >> a3;
			args >> a4;
			args >> a5;
			args >> a6;
			args >> a7;
			#define CallArg8 (A0Type)a0, (A1Type)a1, (A2Type)a2, (A3Type)a3, (A4Type)a4, (A5Type)a5, (A6Type)a6, (A7Type)a7
			if (IsConst)
			{
				if constexpr (std::is_same<_ResultType, void>::value)
				{
					(((_ThisType*)pThis)->*ConstFunctionPtr)(CallArg8);
				}
				else
				{
					result << (((_ThisType*)pThis)->*ConstFunctionPtr)(CallArg8);
				}
			}
			else if (IsStatic)
			{
				if constexpr (std::is_same<_ResultType, void>::value)
				{
					StaticFunctionPtr(CallArg8);
				}
				else
				{
					result << StaticFunctionPtr(CallArg8);
				}
			}
			else
			{
				if constexpr (std::is_same<_ResultType, void>::value)
				{
					(((_ThisType*)pThis)->*FunctionPtr)(CallArg8);
				}
				else
				{
					result << (((_ThisType*)pThis)->*FunctionPtr)(CallArg8);
				}
			}
		}
		else
		{
			ASSERT(false);
		}
	}
};

template <class _ThisType, class... ArgTypes>
struct RttiConstructorImpl : public RttiConstructor
{
	struct TupleVisitor
	{
		template<typename T>
		static void VisitElement(void* pArg, const T&, void*)
		{
			auto method = (RttiConstructor*)pArg;
			RttiType tmp;
			tmp.IsRefer = VIsReferType<T>::ResultValue;
			tmp.NumOfPointer = TypePointerCounter<T>::Value;
			tmp.Type = AuxRttiStruct<typename remove_all_ref_ptr<T>::type>::GetClassObject();
			RttiParameter prm;
			//prm.Name = 
			prm.ParameterType = tmp;
			method->mArguments.push_back(prm);
		}
	};
	RttiConstructorImpl()
	{
		mArguments.clear();
		ForeachTypeList<void, TupleVisitor, sizeof...(ArgTypes), ArgTypes...>::Visit(this, nullptr);
	}
	virtual void* CreateInstance(FArgumentStream& args) const override
	{
		if constexpr (sizeof...(ArgTypes) == 0)
		{
			return new _ThisType();
		}
		else if constexpr (sizeof...(ArgTypes) == 1)
		{
			typedef typename VTypeList_GetAt<0, ArgTypes...>::ResultType A0Type;
			auto a0 = typename VRefAsPtr<A0Type>::ResultType();
			args >> a0;
			#define CallArg1 (A0Type)a0
			return new _ThisType(CallArg1);
		}
		else if constexpr (sizeof...(ArgTypes) == 2)
		{
			typedef typename VTypeList_GetAt<0, ArgTypes...>::ResultType A0Type;
			typedef typename VTypeList_GetAt<1, ArgTypes...>::ResultType A1Type;
			auto a0 = typename VRefAsPtr<A0Type>::ResultType();
			auto a1 = typename VRefAsPtr<A1Type>::ResultType();
			args >> a0;
			args >> a1;
			#define CallArg2 (A0Type)a0, (A1Type)a1
			return new _ThisType(CallArg2);
		}
		else if constexpr (sizeof...(ArgTypes) == 3)
		{
			typedef typename VTypeList_GetAt<0, ArgTypes...>::ResultType A0Type;
			typedef typename VTypeList_GetAt<1, ArgTypes...>::ResultType A1Type;
			typedef typename VTypeList_GetAt<2, ArgTypes...>::ResultType A2Type;
			auto a0 = typename VRefAsPtr<A0Type>::ResultType();
			auto a1 = typename VRefAsPtr<A1Type>::ResultType();
			auto a2 = typename VRefAsPtr<A2Type>::ResultType();
			args >> a0;
			args >> a1;
			args >> a2;
			#define CallArg3 (A0Type)a0, (A1Type)a1, (A2Type)a2
			return new _ThisType(CallArg3);
		}
		else if constexpr (sizeof...(ArgTypes) == 4)
		{
			typedef typename VTypeList_GetAt<0, ArgTypes...>::ResultType A0Type;
			typedef typename VTypeList_GetAt<1, ArgTypes...>::ResultType A1Type;
			typedef typename VTypeList_GetAt<2, ArgTypes...>::ResultType A2Type;
			typedef typename VTypeList_GetAt<3, ArgTypes...>::ResultType A3Type;
			auto a0 = typename VRefAsPtr<A0Type>::ResultType();
			auto a1 = typename VRefAsPtr<A1Type>::ResultType();
			auto a2 = typename VRefAsPtr<A2Type>::ResultType();
			auto a3 = typename VRefAsPtr<A3Type>::ResultType();
			args >> a0;
			args >> a1;
			args >> a2;
			args >> a3;
			#define CallArg4 (A0Type)a0, (A1Type)a1, (A2Type)a2, (A3Type)a3
			return new _ThisType(CallArg4);
		}
		else
		{
			ASSERT(false);
			return nullptr;
		}
	}
};

#define Combine3(a, b, c) a##b##c
#define Combine2(a, b) Combine3(a, b, )

#if defined(PLATFORM_WIN)
	#define CombineFullName(ns, n) Combine3(ns, ::, n)
#else
	#define CombineFullName(ns, n) ns::n
#endif

#define StructBegin(Type, ns) StructBegin_Impl(Type, ns)

#define StructBegin_Impl(Type, ns) \
template <>\
struct AuxRttiBuilder<CombineFullName(ns, Type)> : public AuxRttiBuilderBase\
{\
	static AuxRttiBuilder<Type> Instance;\
	typedef CombineFullName(ns, Type) ThisType;\
	virtual void BuildRtti() override\
	{\
		auto pRtti = AuxRttiStruct<ThisType>::GetClassObject();\
		pRtti->Cleanup();\
		BuildClassInfo<ThisType>(pRtti, #Type, #ns);\
		MemberDesc* __current_member = nullptr;\
		RttiMethodBase* __current_method = nullptr;\
		RttiConstructor* __current_constructor = nullptr;

#define  __vsizeof(_type, _name) sizeof(((_type*)nullptr)->_name)

#define AppendClassMetaInfo(meta, ...) pRtti->AddMeta(MakeWeakRef(new meta(__VA_ARGS__)));
#define AppendMemberMetaInfo(meta, ...) { if(__current_member!=nullptr){__current_member->AddMeta(MakeWeakRef(new meta(__VA_ARGS__)));} }
#define AppendMethodMetaInfo(meta, ...) { if(__current_method!=nullptr){__current_method->AddMeta(MakeWeakRef(new meta(__VA_ARGS__)));} }
#define AppendConstructorMetaInfo(meta, ...) { if(__current_constructor!=nullptr){__current_constructor->AddMeta(MakeWeakRef(new meta(__VA_ARGS__)));} }

#define StructMember(_name) \
		{\
			using declType = decltype(((ThisType*)nullptr)->_name);\
			__current_member = pRtti->PushMember<declType>(__voffsetof(ThisType, _name), __vsizeof(ThisType, _name), #_name);\
		}

#define Struct_Method(name) __current_method = pRtti->PushMethod(new RttiMethodImpl(#name, &CombineFullName(ThisType,name)));
#define Struct_MethodEX(name, RType, ...) __current_method = pRtti->PushMethod(new RttiMethodImpl<ThisType,RType,##__VA_ARGS__>(#name, &CombineFullName(ThisType,name)));
#define Struct_StaticMethod(t,name) __current_method = pRtti->PushMethod(new RttiMethodImpl<t>(#name, &CombineFullName(t,name)));
#define Struct_StaticMethodEX(name, t, RType, ...) __current_method = pRtti->PushMethod(new RttiMethodImpl<t,RType,##__VA_ARGS__>(#name, &CombineFullName(ThisType,name)));

#define StructConstructor(...) __current_constructor = pRtti->PushConstructor(new RttiConstructorImpl<ThisType, ##__VA_ARGS__>());

#define StructEnd(Type, ...) \
		__current_member = nullptr;\
		__current_method = nullptr; \
		__current_constructor = nullptr;\
		pRtti->BuildBaseTypes<Type, ##__VA_ARGS__>();\
		pRtti->Init();\
	}\
};\
AuxRttiBuilder<Type> AuxRttiBuilder<Type>::Instance;

template<typename T>
void MemberDesc::SetValue(void* pThis, const T& v) const
{
	if (MemberType.IsType<T>() == false)
	{
		return;
	}
	*(T*)((BYTE*)pThis + Offset) = v;
	//MemberType.Type->AssignOperator((BYTE*)pThis + Offset, v);
}
template<typename T>
T* MemberDesc::GetValueAddress(void* pThis) const
{
	if (MemberType.IsType<T>() == false)
	{
		return nullptr;
	}
	BYTE* pAddress = (BYTE*)pThis + Offset;
	return (T*)(pAddress);
}

template<typename _Type>
void RttiType::BuildType()
{
	using noConstType = typename std::remove_const<_Type>::type;
	using realType = typename std::remove_all_extents<noConstType>::type;
	using pureType = typename remove_all_ref_ptr<realType>::type;

	Type = &AuxRttiStruct<pureType>::Instance;

	IsRefer = std::is_reference<_Type>::value;
	NumOfPointer = TypePointerCounter<_Type>::Value;
	ArrayElements = sizeof(_Type) / sizeof(pureType);
}

template<typename _Type>
bool RttiType::IsType() const
{
	using noConstType = typename std::remove_const<_Type>::type;
	using realType = typename std::remove_all_extents<noConstType>::type;
	using pureType = typename remove_all_ref_ptr<realType>::type;

	if (IsRefer != std::is_reference<_Type>::value)
	{
		return false;
	}
	if (NumOfPointer != TypePointerCounter<_Type>::Value)
	{
		return false;
	}
	if (Type != AuxRttiStruct<pureType>::GetClassObject())
	{
		return false;
	}
	return true;
}

template<typename _Type>
RttiParameter RttiParameter::BuildParameter(const char* name)
{
	RttiParameter result;
	result.Name = name;
	result.ParameterType.BuildType<_Type>();

	return result;
}

NS_END