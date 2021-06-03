#pragma once
#include "../BaseHead.h"

namespace tpl_utility
{
	struct NullObject
	{
		NullObject();
	};

	template < typename type >
	struct Type2Type
	{
		typedef type			OriType;
	};

	template < typename type , int id >
	struct Int2Type
	{
		enum{
			TypeID			=	id,
		};
		typedef type			Type;
	};

	template< class left , class right >
	struct ClassEqual
	{
		const static bool Result = false;
	};
	template< class left >
	struct ClassEqual<left,left>
	{
		const static bool Result = true;
	};

	template< class type >
	struct IsConst
	{
		enum{
			Result = 0,
		};
	};
	template< class type >
	struct IsConst<const type>
	{
		enum{
			Result = 1,
		};
	};
	template< class type >
	struct IsConst<const type*>
	{
		enum{
			Result = 1,
		};
	};
	template< class type >
	struct IsConst<const type&>
	{
		enum{
			Result = 1,
		};
	};

	template< class type >
	struct IsRefer
	{
		enum{
			Result = 0,
		};
	};
	template< class type >
	struct IsRefer<type&>
	{
		enum{
			Result = 1,
		};
	};
	template< class type >
	struct IsRefer<const type&>
	{
		enum{
			Result = 1,
		};
	};

	template< typename type >
	struct TypeTraits
	{
		typedef type		Result;
	};
	template< typename type >
	struct TypeTraits<type*>
	{
		typedef type		Result;
	};
	template< typename type >
	struct TypeTraits<const type*>
	{
		typedef type		Result;
	};
	template< typename type >
	struct TypeTraits<type&>
	{
		typedef type		Result;
	};
	template< typename type >
	struct TypeTraits<const type&>
	{
		typedef type		Result;
	};

	template <bool Condition,typename r1,typename r2>
	struct Selector
	{
		typedef r1		Result;
	};
	template <typename r1,typename r2>
	struct Selector<false,r1,r2>
	{
		typedef r2		Result;
	};

	class Err_ConditionIsFalse{};
	template <bool Condition,typename Err_Output=Err_ConditionIsFalse>
	struct CompileTest
	{
		CompileTest(){
			Err_Output::_unExistingCode();
		}
	};
	template <typename Err_Output>
	struct CompileTest<true,Err_Output>
	{
	};

	template<typename fromType,typename toType>
	struct CanConvert
	{
		typedef char SmallType ;
		typedef int LargeType ;
		static SmallType TestConvert( const toType& );
		static LargeType TestConvert( ... );
		
		const static bool Result = ( sizeof( TestConvert( *(fromType*)(0)) ) == sizeof(SmallType) );
	};

	template <typename T>
	struct IsAbstract
	{
		typedef char SmallType ;
		typedef int LargeType ;

		template <typename U>
		static SmallType Test( U (*)[1] );
		template <typename U>
		static LargeType Test(...);

		const static bool Result = sizeof( Test<T>(NULL) ) == sizeof(LargeType);
	};
	
	template<class type>
	struct IsPointer{
		struct VPointerProxy{
			VPointerProxy(const volatile void*); // no implementation
		};
		static CHAR IsPtr(VPointerProxy);
		static INT IsPtr(...);
		enum{
			Result = (sizeof( IsPtr( *(type*)(0) ))==sizeof(CHAR) ),
		};
	};
	template<>
	struct IsPointer<void>{
		enum{
			Result = 0,
		};
	};
	template<class type>
	struct IsPointer<type&>{
		enum{
			Result = 0,
		};
	};
	template<class type>
	struct IsPointer<const type&>{
		enum{
			Result = 0,
		};
	};

	template<class T,class U>
	struct Typelist
	{
		typedef T Head;
		typedef U Tail;
	};

#define TYPELIST_1(T1) Typelist<T1,tpl_utility::NullObject>
#define TYPELIST_2(T1,T2) Typelist<T1,tpl_utility::TYPELIST_1(T2)>
#define TYPELIST_3(T1,T2,T3) Typelist<T1,tpl_utility::TYPELIST_2(T2,T3)>
#define TYPELIST_4(T1,T2,T3,T4) Typelist<T1,tpl_utility::TYPELIST_3(T2,T3,T4)>
#define TYPELIST_5(T1,T2,T3,T4,T5) Typelist<T1,tpl_utility::TYPELIST_4(T2,T3,T4,T5)>
#define TYPELIST_6(T1,T2,T3,T4,T5,T6) Typelist<T1,tpl_utility::TYPELIST_5(T2,T3,T4,T5,T6)>
#define TYPELIST_7(T1,T2,T3,T4,T5,T6,T7) Typelist<T1,tpl_utility::TYPELIST_6(T2,T3,T4,T5,T6,T7)>
#define TYPELIST_8(T1,T2,T3,T4,T5,T6,T7,T8) Typelist<T1,tpl_utility::TYPELIST_7(T2,T3,T4,T5,T6,T7,T8)>

	template< typename Tlist , int Index >
	struct TListIndexOf
	{
		typedef typename TListIndexOf<typename Tlist::Tail,Index-1>::Result		Result;
	};
	template< typename Tlist >
	struct TListIndexOf<Tlist,0>
	{
		typedef typename Tlist::Head			Result;
	};

	template< typename Tlist >
	struct TListLength
	{
		enum{
			Length		= TListLength<typename Tlist::Tail>::Length + 1,
		};
	};
	template<>
	struct TListLength<NullObject>
	{
		enum{
			Length		= 0,
		};
	};

	template< typename TList,int Index,bool bInRange=(Index<TListLength<TList>::Length) >
	struct SafeTListIndexOf
	{
		typedef typename TListIndexOf<TList,Index>::Result Result;
	};
	template< typename TList,int Index >
	struct SafeTListIndexOf<TList,Index,false>
	{
		typedef NullObject Result;
	};

	template <typename ResultType , typename TList , typename BindClass>
	struct Functor
	{
		typedef BindClass			HostClass;
		typedef TList				ArgTypeList;
		typedef ResultType			TReturnType;

		static const bool HasReturn = (tpl_utility::ClassEqual<tpl_utility::NullObject,TReturnType>::Result==false
			&& tpl_utility::ClassEqual<void,TReturnType>::Result==false );

		bool						IsGlobalCall;

		Functor()
			: IsGlobalCall(false)
		{
			
		}

		typedef typename SafeTListIndexOf<TList,0>::Result	P0;
		typedef typename SafeTListIndexOf<TList,1>::Result	P1;
		typedef typename SafeTListIndexOf<TList,2>::Result	P2;
		typedef typename SafeTListIndexOf<TList,3>::Result	P3;
		typedef typename SafeTListIndexOf<TList,4>::Result	P4;
		typedef typename SafeTListIndexOf<TList,5>::Result	P5;
		typedef typename SafeTListIndexOf<TList,6>::Result	P6;
		typedef typename SafeTListIndexOf<TList,7>::Result	P7;

		typedef ResultType (*GlobalFun0)();
		typedef ResultType (*GlobalFun1)(P0);
		typedef ResultType (*GlobalFun2)(P0,P1);
		typedef ResultType (*GlobalFun3)(P0,P1,P2);
		typedef ResultType (*GlobalFun4)(P0,P1,P2,P3);
		typedef ResultType (*GlobalFun5)(P0,P1,P2,P3,P4);
		typedef ResultType (*GlobalFun6)(P0,P1,P2,P3,P4,P5);
		typedef ResultType (*GlobalFun7)(P0,P1,P2,P3,P4,P5,P6);
		typedef ResultType (*GlobalFun8)(P0,P1,P2,P3,P4,P5,P6,P7);

		typedef ResultType ( BindClass::*MemberFun0 )();
		typedef ResultType ( BindClass::*MemberFun1 )(P0);
		typedef ResultType ( BindClass::*MemberFun2 )(P0,P1);
		typedef ResultType ( BindClass::*MemberFun3 )(P0,P1,P2);
		typedef ResultType ( BindClass::*MemberFun4 )(P0,P1,P2,P3);
		typedef ResultType ( BindClass::*MemberFun5 )(P0,P1,P2,P3,P4);
		typedef ResultType ( BindClass::*MemberFun6 )(P0,P1,P2,P3,P4,P5);
		typedef ResultType ( BindClass::*MemberFun7 )(P0,P1,P2,P3,P4,P5,P6);
		typedef ResultType ( BindClass::*MemberFun8 )(P0,P1,P2,P3,P4,P5,P6,P7);

		union FunAddress{
			GlobalFun0		Gfun0;
			GlobalFun1		Gfun1;
			GlobalFun2		Gfun2;
			GlobalFun3		Gfun3;
			GlobalFun4		Gfun4;
			GlobalFun5		Gfun5;
			GlobalFun6		Gfun6;
			GlobalFun7		Gfun7;
			GlobalFun8		Gfun8;

			MemberFun0		Mfun0;
			MemberFun1		Mfun1;
			MemberFun2		Mfun2;
			MemberFun3		Mfun3;
			MemberFun4		Mfun4;
			MemberFun5		Mfun5;
			MemberFun6		Mfun6;
			MemberFun7		Mfun7;
			MemberFun8		Mfun8;
		} Address;

		enum{
			ParamNum = TListLength<TList>::Length,
		};		

		void Init( ResultType(BindClass::*MemberFun)() ){
			Address.Mfun0 = MemberFun;
		}
		void Init( ResultType(BindClass::*MemberFun)(P0) ){
			Address.Mfun1 = MemberFun;
		}
		void Init( ResultType(BindClass::*MemberFun)(P0,P1) ){
			Address.Mfun2 = MemberFun;
		}
		void Init( ResultType(BindClass::*MemberFun)(P0,P1,P2) ){
			Address.Mfun3 = MemberFun;
		}
		void Init( ResultType(BindClass::*MemberFun)(P0,P1,P2,P3) ){
			Address.Mfun4 = MemberFun;
		}
		void Init( ResultType(BindClass::*MemberFun)(P0,P1,P2,P3,P4) ){
			Address.Mfun5 = MemberFun;
		}
		void Init( ResultType(BindClass::*MemberFun)(P0,P1,P2,P3,P4,P5) ){
			Address.Mfun6 = MemberFun;
		}
		void Init( ResultType(BindClass::*MemberFun)(P0,P1,P2,P3,P4,P5,P6) ){
			Address.Mfun7 = MemberFun;
		}
		void Init( ResultType(BindClass::*MemberFun)(P0,P1,P2,P3,P4,P5,P6,P7) ){
			Address.Mfun8 = MemberFun;
		}

		void Init(ResultType(*GlobalFun)())
		{
			IsGlobalCall = true;
			Address.Gfun0 = GlobalFun;
		}
		void Init( ResultType(*GlobalFun)(P0) ){
			IsGlobalCall = true;
			Address.Gfun1 = GlobalFun;
		}
		void Init( ResultType(*GlobalFun)(P0,P1) ){
			IsGlobalCall = true;
			Address.Gfun2 = GlobalFun;
		}
		void Init( ResultType(*GlobalFun)(P0,P1,P2) ){
			IsGlobalCall = true;
			Address.Gfun3 = GlobalFun;
		}
		void Init( ResultType(*GlobalFun)(P0,P1,P2,P3) ){
			IsGlobalCall = true;
			Address.Gfun4 = GlobalFun;
		}
		void Init( ResultType(*GlobalFun)(P0,P1,P2,P3,P4) ){
			IsGlobalCall = true;
			Address.Gfun5 = GlobalFun;
		}
		void Init( ResultType(*GlobalFun)(P0,P1,P2,P3,P4,P5) ){
			IsGlobalCall = true;
			Address.Gfun6 = GlobalFun;
		}
		void Init( ResultType(*GlobalFun)(P0,P1,P2,P3,P4,P5,P6) ){
			IsGlobalCall = true;
			Address.Gfun7 = GlobalFun;
		}
		void Init( ResultType(*GlobalFun)(P0,P1,P2,P3,P4,P5,P6,P7) ){
			IsGlobalCall = true;
			Address.Gfun8 = GlobalFun;
		}

		ResultType GCall(BindClass* pBindObj){
			return (Address.Gfun0)();
		}
		ResultType GCall( BindClass* pBindObj,P0 p0 ){
			return (Address.Gfun1)(p0);
		}
		ResultType GCall( BindClass* pBindObj,P0 p0 , P1 p1 ){
			return (Address.Gfun2)(p0,p1);
		}
		ResultType GCall( BindClass* pBindObj,P0 p0 , P1 p1 , P2 p2 ){
			return (Address.Gfun3)(p0,p1,p2);
		}
		ResultType GCall( BindClass* pBindObj,P0 p0 , P1 p1 , P2 p2 , P3 p3 ){
			return (Address.Gfun4)(p0,p1,p2,p3);
		}
		ResultType GCall( BindClass* pBindObj,P0 p0 , P1 p1 , P2 p2 , P3 p3 , P4 p4 ){
			return (Address.Gfun5)(p0,p1,p2,p3,p4);
		}
		ResultType GCall( BindClass* pBindObj,P0 p0 , P1 p1 , P2 p2 , P3 p3 , P4 p4 , P5 p5 ){
			return (Address.Gfun6)(p0,p1,p2,p3,p4,p5);
		}
		ResultType GCall( BindClass* pBindObj,P0 p0 , P1 p1 , P2 p2 , P3 p3 , P4 p4 , P5 p5 , P6 p6 ){
			return (Address.Gfun7)(p0,p1,p2,p3,p4,p5,p6);
		}
		ResultType GCall( BindClass* pBindObj,P0 p0 , P1 p1 , P2 p2 , P3 p3 , P4 p4 , P5 p5 , P6 p6 , P7 p7 ){
			return (Address.Gfun8)(p0,p1,p2,p3,p4,p5,p6,p7);
		}

		ResultType MCall(BindClass* pBindObj){
			return (pBindObj->*Address.Mfun0)();
		}
		ResultType MCall( BindClass* pBindObj,P0 p0 ){
			return (pBindObj->*Address.Mfun1)(p0);
		}
		ResultType MCall( BindClass* pBindObj,P0 p0 , P1 p1 ){
			return (pBindObj->*Address.Mfun2)(p0,p1);
		}
		ResultType MCall( BindClass* pBindObj,P0 p0 , P1 p1 , P2 p2 ){
			return (pBindObj->*Address.Mfun3)(p0,p1,p2);
		}
		ResultType MCall( BindClass* pBindObj,P0 p0 , P1 p1 , P2 p2 , P3 p3 ){
			return (pBindObj->*Address.Mfun4)(p0,p1,p2,p3);
		}
		ResultType MCall( BindClass* pBindObj,P0 p0 , P1 p1 , P2 p2 , P3 p3 , P4 p4 ){
			return (pBindObj->*Address.Mfun5)(p0,p1,p2,p3,p4);
		}
		ResultType MCall( BindClass* pBindObj,P0 p0 , P1 p1 , P2 p2 , P3 p3 , P4 p4 , P5 p5 ){
			return (pBindObj->*Address.Mfun6)(p0,p1,p2,p3,p4,p5);
		}
		ResultType MCall( BindClass* pBindObj,P0 p0 , P1 p1 , P2 p2 , P3 p3 , P4 p4 , P5 p5 , P6 p6 ){
			return (pBindObj->*Address.Mfun7)(p0,p1,p2,p3,p4,p5,p6);
		}
		ResultType MCall( BindClass* pBindObj,P0 p0 , P1 p1 , P2 p2 , P3 p3 , P4 p4 , P5 p5 , P6 p6 , P7 p7 ){
			return (pBindObj->*Address.Mfun8)(p0,p1,p2,p3,p4,p5,p6,p7);
		}

		//////////////////////////////////////////////////////////////////////////

		void InitConstructor( ResultType(*GlobalFun)() ){
			IsGlobalCall = true;
			Address.Gfun0 = GlobalFun;
		}
		void InitConstructor( ResultType(*GlobalFun)(P0) ){
			IsGlobalCall = true;
			Address.Gfun1 = GlobalFun;
		}
		void InitConstructor( ResultType(*GlobalFun)(P0,P1) ){
			IsGlobalCall = true;
			Address.Gfun2 = GlobalFun;
		}
		void InitConstructor( ResultType(*GlobalFun)(P0,P1,P2) ){
			IsGlobalCall = true;
			Address.Gfun3 = GlobalFun;
		}
		void InitConstructor( ResultType(*GlobalFun)(P0,P1,P2,P3) ){
			IsGlobalCall = true;
			Address.Gfun4 = GlobalFun;
		}
		void InitConstructor( ResultType(*GlobalFun)(P0,P1,P2,P3,P4) ){
			IsGlobalCall = true;
			Address.Gfun5 = GlobalFun;
		}
		void InitConstructor( ResultType(*GlobalFun)(P0,P1,P2,P3,P4,P5) ){
			IsGlobalCall = true;
			Address.Gfun6 = GlobalFun;
		}
		void InitConstructor( ResultType(*GlobalFun)(P0,P1,P2,P3,P4,P5,P6) ){
			IsGlobalCall = true;
			Address.Gfun7 = GlobalFun;
		}
		void InitConstructor( ResultType(*GlobalFun)(P0,P1,P2,P3,P4,P5,P6,P7) ){
			IsGlobalCall = true;
			Address.Gfun8 = GlobalFun;
		}

		ResultType ConstructorCall(){
			return Address.Gfun0();
		}
		ResultType ConstructorCall( P0 p0 ){
			return Address.Gfun1(p0);
		}
		ResultType ConstructorCall( P0 p0 , P1 p1 ){
			return Address.Gfun2(p0,p1);
		}
		ResultType ConstructorCall( P0 p0 , P1 p1 , P2 p2 ){
			return Address.Gfun3(p0,p1,p2);
		}
		ResultType ConstructorCall( P0 p0 , P1 p1 , P2 p2 , P3 p3 ){
			return Address.Gfun4(p0,p1,p2,p3);
		}
		ResultType ConstructorCall( P0 p0 , P1 p1 , P2 p2 , P3 p3 , P4 p4 ){
			return Address.Gfun5(p0,p1,p2,p3,p4);
		}
		ResultType ConstructorCall( P0 p0 , P1 p1 , P2 p2 , P3 p3 , P4 p4 , P5 p5 ){
			return Address.Gfun6(p0,p1,p2,p3,p4,p5);
		}
		ResultType ConstructorCall( P0 p0 , P1 p1 , P2 p2 , P3 p3 , P4 p4 , P5 p5 , P6 p6 ){
			return Address.Gfun7(p0,p1,p2,p3,p4,p5,p6);
		}
		ResultType ConstructorCall( P0 p0 , P1 p1 , P2 p2 , P3 p3 , P4 p4 , P5 p5 , P6 p6 , P7 p7 ){
			return Address.Gfun8(p0,p1,p2,p3,p4,p5,p6,p7);
		}
	};

	template<typename ResultType,int Index,typename Functor>
	struct ConstructorTraits
	{
		static  ResultType* Constructor()
		{
			return NULL;
		}
	};
	template<typename ResultType,typename Functor>
	struct ConstructorTraits<ResultType,0,Functor>
	{
		static  ResultType* Constructor()
		{
			return new ResultType();
		}
	};
	template<typename ResultType,typename Functor>
	struct ConstructorTraits<ResultType,1,Functor>
	{
		static  ResultType* Constructor( typename Functor::P0 p0 )
		{
			return new ResultType( p0 );
		}
	};
	template<typename ResultType,typename Functor>
	struct ConstructorTraits<ResultType,2,Functor>
	{
		static  ResultType* Constructor( typename Functor::P0 p0 , typename Functor::P1 p1 )
		{
			return new ResultType( p0 , p1 );
		}
	};
	template<typename ResultType,typename Functor>
	struct ConstructorTraits<ResultType,3,Functor>
	{
		static  ResultType* Constructor( typename Functor::P0 p0 , typename Functor::P1 p1 , typename Functor::P2 p2 )
		{
			return new ResultType( p0 , p1 , p2 );
		}
	};
	template<typename ResultType,typename Functor>
	struct ConstructorTraits<ResultType,4,Functor>
	{
		static  ResultType* Constructor( typename Functor::P0 p0 , typename Functor::P1 p1 , typename Functor::P2 p2 , typename Functor::P3 p3 )
		{
			return new ResultType( p0 , p1 , p2 , p3 );
		}
	};
	template<typename ResultType,typename Functor>
	struct ConstructorTraits<ResultType,5,Functor>
	{
		static  ResultType* Constructor( typename Functor::P0 p0 , 
							typename Functor::P1 p1 , 
							typename Functor::P2 p2 , 
							typename Functor::P3 p3 , 
							typename Functor::P4 p4 )
		{
			return new ResultType( p0 , p1 , p2 , p3 , p4 );
		}
	};
	template<typename ResultType,typename Functor>
	struct ConstructorTraits<ResultType,6,Functor>
	{
		static  ResultType* Constructor( typename Functor::P0 p0 , 
							typename Functor::P1 p1 , 
							typename Functor::P2 p2 , 
							typename Functor::P3 p3 , 
							typename Functor::P4 p4 , 
							typename Functor::P5 p5 )
		{
			return new ResultType( p0 , p1 , p2 , p3 , p4 , p5 );
		}
	};
	template<typename ResultType,typename Functor>
	struct ConstructorTraits<ResultType,7,Functor>
	{
		static  ResultType* Constructor( typename Functor::P0 p0 , 
							typename Functor::P1 p1 , 
							typename Functor::P2 p2 , 
							typename Functor::P3 p3 , 
							typename Functor::P4 p4 , 
							typename Functor::P5 p5 , 
							typename Functor::P6 p6 )
		{
			return new ResultType( p0 , p1 , p2 , p3 , p4 , p5 , p6 );
		}
	};
	template<typename ResultType,typename Functor>
	struct ConstructorTraits<ResultType,8,Functor>
	{
		static  ResultType* Constructor( typename Functor::P0 p0 , 
							typename Functor::P1 p1 , 
							typename Functor::P2 p2 , 
							typename Functor::P3 p3 , 
							typename Functor::P4 p4 , 
							typename Functor::P5 p5 , 
							typename Functor::P6 p6 , 
							typename Functor::P7 p7 )
		{
			return new ResultType( p0 , p1 , p2 , p3 , p4 , p5 , p6 , p7 );
		}
	};

	template < typename Func >
	struct TraitsFunction
	{
		
	};
	
	template< typename BindClass ,typename _ReturnType,typename _P0>
	struct TraitsFunction< _ReturnType(BindClass::*)(_P0) >
	{
		typedef _ReturnType	Return;
	};

}