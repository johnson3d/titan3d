#pragma once
#include <tuple>
#include <string>
#include <vector>

//#define EngineNS Titan3D
#define NS_BEGIN namespace EngineNS{
#define NS_END }

#if defined(__REF_CLANG__)
	#define TR_CLASS(...) __attribute__((annotate(#__VA_ARGS__)))
	#define TR_FUNCTION(...) __attribute__((annotate(#__VA_ARGS__)))
	#define TR_MEMBER(...) __attribute__((annotate(#__VA_ARGS__)))
	#define TR_ENUM(...) __attribute__((annotate(#__VA_ARGS__)))
	#define TR_ENUM_MEMBER(...) __attribute__((annotate(#__VA_ARGS__)))
	#define TR_CONSTRUCTOR(...) __attribute__((annotate(#__VA_ARGS__)))
	#define TR_CALLBACK(...) __attribute__((annotate(#__VA_ARGS__)))

	#define TR_META(...) __attribute__((annotate(#__VA_ARGS__)))

	#define TR_DISCARD(...) __attribute__((annotate(#__VA_ARGS__)))
#else
	#define TR_CLASS(...)
	#define TR_FUNCTION(...)
	#define TR_MEMBER(...)
	#define TR_ENUM(...)
	#define TR_ENUM_MEMBER(...)
	#define TR_CONSTRUCTOR(...)
	#define TR_CALLBACK(...)

	#define TR_META(...)

	#define TR_DISCARD(...)
#endif

#define TR_DECL(type) friend struct type##_Visitor;\
						friend struct AuxRttiBuilder<type>;

NS_BEGIN

struct FArgumentStream
{
	FArgumentStream()
		: mReadPosition(0)
	{
	}
	size_t				mReadPosition;
	std::vector<unsigned char>	mArguments;
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

template<class TYPE>
inline void Safe_Delete(TYPE*& p) {
	if (p == NULL)
	{
		return;
	}
	TYPE* refPtr = p;
	p = NULL;
	delete refPtr;
	refPtr = NULL;
}

template<class TYPE>
inline void Safe_DeleteArray(TYPE*& p) {
	if (p == NULL)
	{
		return;
	}
	TYPE* refPtr = p;
	p = NULL;
	delete[] refPtr;
	refPtr = NULL;
}

template<typename T>
void Safe_Release(T*& p)
{
	if (p == NULL)
		return;
	p->Release();
	p = nullptr;
}

template<class T>
class AutoRef
{
	T* Ptr;
public:
	AutoRef()
	{
		Ptr = nullptr;
	}
	static AutoRef<T> _MakeWeakRef(T* ptr)
	{
		AutoRef<T> result;
		result.Ptr = ptr;
		return result;
	}
	AutoRef(T* ptr)
	{
		Ptr = ptr;
		if (Ptr != nullptr)
			Ptr->AddRef();
	}
	AutoRef(const AutoRef<T>& rh)
	{
		Ptr = rh.Ptr;
		if (Ptr != nullptr)
			Ptr->AddRef();
	}
	~AutoRef()
	{
		Safe_Release(Ptr);
	}
	void StrongRef(T* ptr)
	{
		if (ptr)
			ptr->AddRef();
		Safe_Release(Ptr);
		Ptr = ptr;
	}
	void WeakRef(T* ptr)
	{
		Safe_Release(Ptr);
		Ptr = ptr;
	}
	void Clear()
	{
		Safe_Release(Ptr);
	}
	AutoRef<T>& operator = (const AutoRef<T>& rh)
	{
		Safe_Release(Ptr);
		Ptr = rh.Ptr;
		if (Ptr != nullptr)
			Ptr->AddRef();
		return *this;
	}
	/*AutoRef<T>& operator = (T* rh)
	{
		if (rh != nullptr)
			rh->AddRef();
		Safe_Release(Ptr);
		Ptr = rh;
		return *this;
	}*/
	bool operator==(T* rh) const
	{
		return (Ptr == rh);
	}
	bool operator !=(T* rv) const
	{
		return (Ptr != rv);
	}

	T* operator->() const
	{
		return Ptr;
	}
	/*T* operator->()
	{
		return Ptr;
	}
	const T* operator->() const
	{
		return Ptr;
	}*/
	template<class ConverType>
	ConverType* UnsafeConvertTo() const
	{
#if PLATFORM_WIN
		return dynamic_cast<ConverType*>(Ptr);
#else
		return (ConverType*)(Ptr);
#endif
	}
	template<class ConverType>
	inline AutoRef<ConverType> As()  const {
		AutoRef<ConverType> result;
		result.StrongRef(UnsafeConvertTo<ConverType>());
		return result;
	}
	template<class ConverType>
	operator AutoRef<ConverType>() const
	{
		AutoRef<ConverType> result;
		result.StrongRef(Ptr);
		return result;
	}
	operator T* () const
	{
		return Ptr;
	}
	T* GetPtr() {
		return Ptr;
	}
	const T* GetPtr() const {
		return Ptr;
	}
};

template<class T>
inline AutoRef<T> MakeWeakRef(T* ptr)
{
	return AutoRef<T>::_MakeWeakRef(ptr);
}

struct VIsAutoRef
{
	template<typename Type>
	constexpr static char TypeTest(const Type*)
	{
		return 1;
	}
	template<typename Type>
	constexpr static char TypeTest(const AutoRef<Type>*)
	{
		return 2;
	}
};


template<typename Type>
inline Type VGetTypeDefault()
{
	return Type();
}

inline void VGetTypeDefault()
{

}

template<typename SType, typename TType>
inline TType VReturnValueMarshal(const SType& v)
{
	return *((TType*)&v);
}

template<>
inline unsigned char VReturnValueMarshal<bool, unsigned char>(const bool& v)
{
	return (unsigned char)(v? 1 : 0);
}

template<typename SType, typename TType>
inline TType VParameterMarshal(SType v)
{
	return *((TType*)&v);
}

template<>
inline std::string VParameterMarshal<char*, std::string>(char* v)
{
	return std::string(v);
}
template<>
inline std::string VParameterMarshal<const char*, std::string>(const char* v)
{
	return std::string(v);
}
//template<>
//inline const std::string& VParameterMarshal<char*, const std::string&>(char* v)
//{
//	return std::string(v);
//}

template <typename R, typename... Args>
struct TFunction_traits_helper
{
	static constexpr auto param_count = sizeof...(Args);
	using return_type = R;

	template <std::size_t N>
	using param_type = std::tuple_element<N, std::tuple<Args...>>;
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
	const bool IsConst = false;
	using class_type = ClassType;
};

template <typename ClassType, typename R, typename... Args>
struct TFunction_traits<R(ClassType::*)(Args...) const> : public TFunction_traits_helper<R, Args...>
{
	const bool IsConst = true;
	using class_type = ClassType;
};

template <typename T>
struct remove_all_ref_ptr { typedef T type; };

template<typename T>
struct remove_all_ref_ptr<AutoRef<T>> : public remove_all_ref_ptr<T>{};

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

template<typename T>
struct VIsReferType
{
	static const bool ResultValue = false;
};
template<typename T>
struct VIsReferType<T&>
{
	static const bool ResultValue = true;
};
template<typename T>
struct VIsReferType<const T&>
{
	static const bool ResultValue = true;
};

template<typename _type>
struct VRefAsPtr
{
	typedef _type ResultType;
};

template<typename _type>
struct VRefAsPtr<_type&>
{
	typedef _type* ResultType;
};

template<typename _type>
struct VRefAsPtr<const _type&>
{
	typedef _type* ResultType;
};

template<typename _type>
struct VTypeAsRef
{
	typedef _type& ResultType;
};

template<typename _type>
struct VTypeAsRef<_type&>
{
	typedef _type& ResultType;
};

template<typename _type>
struct VTypeAsRef<const _type&>
{
	typedef _type& ResultType;
};

template<class _type>
struct TypePointerCounter
{
	enum {
		Value = 0,
	};
};
template<class _type>
struct TypePointerCounter<_type*> : TypePointerCounter<_type>
{
	enum {
		Value = TypePointerCounter<_type>::Value + 1,
	};
};

template<typename _Tuple, class _Visitor, int _Index>
struct ForEachTupleHelper
{
	static void Iterate(_Tuple& t, _Visitor visitor)
	{
		visitor(std::get<_Index>(t));
		ForEachTupleHelper<_Tuple, _Visitor, _Index - 1>::Iterate(t, visitor);
	}
};

template<typename _Tuple, class _Visitor>
struct ForEachTupleHelper<_Tuple, _Visitor, 0>
{
	static void Iterate(_Tuple& t, _Visitor visitor)
	{
		visitor(std::get<0>(t));
	}
};

template<typename _Tuple, class _Visitor>
void ForEachTuple(_Tuple& t, _Visitor visitor)
{
	ForEachTupleHelper<_Tuple, _Visitor, std::tuple_size<_Tuple>::value - 1>::Iterate(t, visitor);
}

template<class _Visitor>
void ForEachTuple(std::tuple<>& t, _Visitor visitor)
{
	
}

struct VisitTupleElement
{
	template<typename _Type>
	void operator()(_Type& element) {

	}
};

template <typename... T>
struct VTypeList 
{
	static const int Size = sizeof...(T);
	//typedef typename T... ArgTypes;
};

template<typename T>
struct VTypeList_First{};
template<typename first, typename... rest>
struct VTypeList_First<VTypeList<first, rest...>>
{
	using ResultType = first;
};

template<typename T>
struct VTypeList_PopFirst {};
template<typename first, typename... rest>
struct VTypeList_PopFirst<VTypeList<first, rest...>>
{
	using ResultType = VTypeList<rest...>;
};

template<typename TL, typename NewType>
struct VTypeList_PushFront {};
template<typename... TL, typename NewType>
struct VTypeList_PushFront<VTypeList<TL...>, NewType>
{
	using ResultType = VTypeList<NewType, TL...>;
};

template<typename TL, typename NewType>
struct VTypeList_PushBack {};
template<typename... TL, typename NewType>
struct VTypeList_PushBack<VTypeList<TL...>, NewType>
{
	using ResultType = VTypeList<TL..., NewType>;
};

template<typename TL, typename NewType>
struct VTypeList_ReplaceFront {};
template<typename First, typename... rest, typename NewType>
struct VTypeList_ReplaceFront<VTypeList<First, rest...>, NewType>
{
	using ResultType = VTypeList<NewType, rest...>;
};

template<class TL>
struct VTypeList_GetSize {};
template<typename... ArgTypes>
struct VTypeList_GetSize<VTypeList<ArgTypes...>>
{
	static const int ResultValue = sizeof...(ArgTypes);
};

template<int N, typename TL>
struct VTypeList_GetAt {};
template<int N, typename First, typename... rest>
struct VTypeList_GetAt<N, VTypeList<First, rest...>>
{
	using ResultType = typename VTypeList_GetAt<N - 1, VTypeList<rest...>>::ResultType;
};
template<typename First, typename... rest>
struct VTypeList_GetAt<0, VTypeList<First, rest...>>
{
	using ResultType = First;
};

template<int N, typename NewType, typename TL>
struct VTypeList_SetAt {};
template<int N, typename NewType, typename First, typename... rest>
struct VTypeList_SetAt<N, NewType, VTypeList<First, rest...>>
{
	using StripType = typename VTypeList_SetAt<N - 1, NewType, VTypeList<rest...>>::ResultType;
	using ResultType = typename VTypeList_PushFront<StripType, First>::ResultType ;
};
template<typename NewType, typename First, typename... rest>
struct VTypeList_SetAt<0, NewType, VTypeList<First, rest...>>
{
	using ResultType = VTypeList<NewType, rest...>;
};

//template<size_t N, class first, class... rest>
//struct TypeListGet
//{
//	typedef typename TypeListGet<N - 1, rest...>::ResultType ResultType;
//};
//
//template<class first, class... rest>
//struct TypeListGet<0, first, rest...>
//{
//	typedef first ResultType;
//};

template<typename ArgType, typename Visitor, size_t N, class... TupleType>
struct ForeachTypeList
{
	static void Visit(void* arg, ArgType* arg2)
	{
		ForeachTypeList<ArgType, Visitor, N - 1, TupleType...>::Visit(arg, arg2);
		using CurType = typename VTypeList_GetAt<N - 1, VTypeList<TupleType...>>::ResultType;
		const auto& t = typename VRefAsPtr<CurType>::ResultType();
		Visitor::VisitElement(arg, t, arg2);
	}
};

template<typename ArgType, typename Visitor, class... TupleType>
struct ForeachTypeList<ArgType, Visitor, 0, TupleType...>
{
	static void Visit(void*, ArgType* arg2)
	{

	}
};

//inline void TestForeachTuple()
//{
//	auto t = std::make_tuple(1, 1.2f);
//	ForEachTuple(t, [](auto& e) {});
//	ForEachTuple(t, VisitTupleElement());
//}

template<class T1, class... Args>
void MutiArg(const T1&t1, Args... args)
{
	//.....

	MutiArg(args...);
}

template<class T1>
void MutiArg(const T1&t1)
{
	//do something with t1
}

NS_END
