#pragma once

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

NS_END
