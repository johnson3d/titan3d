#pragma once
#include <tuple>

#define EngineNS Titan3D
#define NS_BEGIN namespace EngineNS{
#define NS_END }

#define TR_CLASS(...)
#define TR_FUNCTION(...)
#define TR_MEMBER(...)
#define TR_ENUM(...)
#define TR_ENUM_MEMBER(...)
#define TR_CONSTRUCTOR(...)
#define TR_CALLBACK(...)

#define TR_DECL(type) friend struct AuxRttiStruct<type>;\
					friend struct type##_Visitor;

#define TR_DISCARD(...)

NS_BEGIN

template<typename Type>
inline Type VGetTypeDefault()
{
	return Type();
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


template<typename _type>
struct VRefAsPtr
{
	typedef _type Result;
};

template<typename _type>
struct VRefAsPtr<_type&>
{
	typedef _type* Result;
};

template<typename _type>
struct VRefAsPtr<const _type&>
{
	typedef _type* Result;
};

template<typename _type>
struct VTypeAsRef
{
	typedef _type& Result;
};

template<typename _type>
struct VTypeAsRef<_type&>
{
	typedef _type& Result;
};

template<typename _type>
struct VTypeAsRef<const _type&>
{
	typedef _type& Result;
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

inline void TestForeachTuple()
{
	auto t = std::make_tuple(1, 1.2f);
	ForEachTuple(t, [](auto& e) {});
	ForEachTuple(t, VisitTupleElement());
}

template<class T1, class... Args>
void MutiArg(const T1&t1, Args... args)
{
	//对第一个调用参数进行操作
	//.....

	//对参数包进行递归解析，这个函数会一直调用直到其参数个数为1时停止调用
	MutiArg(args...);
}

template<class T1>
void MutiArg(const T1&t1)
{
	//do something with t1
}

NS_END
