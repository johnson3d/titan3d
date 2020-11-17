// ***************************************************************
//  vfxReflectionMethod   version:  1.0   date: 04/02/2008
//  -------------------------------------------------------------
//  johnson
//  -------------------------------------------------------------
//  Copyright (C) 2008 - All Rights Reserved
// ***************************************************************
// 
// ***************************************************************

#pragma once

#include "../generic/vfxTemplateUtil.h"
#include "vfxReflectBase.h"

namespace Reflection
{
#define DEF_REFCONVERTER( _Arg , __Arg ) typedef typename tpl_utility::Selector< tpl_utility::IsRefer<_Arg>::Result , typename tpl_utility::TypeTraits<_Arg>::Result* ,_Arg>::Result __Arg;
#define FILL_PARAMTYPE( obj , _Arg ) obj.ClassType = ClassExporter<_Arg>::ClassType();\
	obj.IsRefer = tpl_utility::IsRefer<_Arg>::Result;\
	obj.IsPointer = tpl_utility::IsPointer<_Arg>::Result;\
	obj.IsConst = tpl_utility::IsConst<_Arg>::Result;

	template<class type>
	struct TypeGen
	{
		typedef type				TypeSelf;
		typedef type *				TypePtr;
		typedef const type *		TypeCPtr;
		typedef type &				TypeRef;
		typedef const type &		TypeCRef;
	};

	template<typename classname,bool IsAbstract = tpl_utility::IsAbstract<classname>::Result >
	struct CreateSelector
	{
		static void* TCreateObject(){
#if defined(_DEBUG) && !defined(UNNEW)
			return new(__FILE__,__LINE__) classname;
#else
			return new classname;
#endif
		}
	};
	template<typename classname>
	struct CreateSelector<classname,true>
	{
		static void* TCreateObject(){
			return NULL;
		}
	};
	
#define BUILD_CASS( classname ) Reflection::ClassExporter<classname>::BuildType();

	template <typename type>
	struct ClassIDProvider
	{
		const static vIID UID = type::__UID__;
		const static ObjType OriType = OT_ReflectObject;
	};

#define DEF_REFLECT( prefix , classname , parentclass , assemblyname )			\
	template<> struct ClassExporter< classname >\
	{\
		ClassExporter< classname >();\
		static void BuildType(){\
			BuildClass();\
			BuildMethod();\
		}\
		prefix static VClassType* ClassType(){ static VClassType Instance; return &Instance;}\
		prefix static void BuildClass()\
		{\
			VClassType* Instance = ClassType();\
			Instance->Type = ClassIDProvider<classname>::OriType;\
			Instance->FullName = (#classname);\
			Instance->ClassID = ClassIDProvider<classname>::UID; \
			Instance->CreateObject = &CreateSelector<classname>::TCreateObject;\
			Instance->Super = ClassExporter<parentclass>::ClassType();\
			VAssemblyProvider<assemblyname>::GetAssembly()->RegClassType( (#classname) , Instance->ClassID , Instance );\
		};\
		prefix static void BuildMethod();\
	}; \
	template<> struct ClassExporter< classname & >\
	{\
		static VClassType* ClassType(){ return ClassExporter<classname>::ClassType(); }\
	}; \
	template<> struct ClassExporter< const classname & >\
	{\
		static VClassType* ClassType(){ return ClassExporter<classname>::ClassType(); }\
	}; \
	template<> struct ClassExporter< classname * >\
	{\
		static VClassType* ClassType(){ return ClassExporter<classname>::ClassType(); }\
	}; \
	template<> struct ClassExporter< const classname * >\
	{\
		static VClassType* ClassType(){ return ClassExporter<classname>::ClassType(); }\
	};

#define	BEGIN_REFLECT_METHOD( classname )  void ClassExporter< classname >::BuildMethod() {

#define REF_MEMBER( classname , membertype , membername ) \
	ClassType()->Members.push_back( \
		VClassType::VMember( ClassExporter< membertype >::ClassType() , \
				(#membername) , offsetof(classname,membername) , \
				tpl_utility::IsConst<membertype>::Result , \
				tpl_utility::IsPointer<membertype>::Result ) );

#define ADD_MEMBER_ATTR( classname , attr ) {\
		VClassType* pClass = ClassExporter< classname >::ClassType();\
		Reflection::VClassType::VMember* pMember = &pClass->Members[pClass->Members.size()-1];\
		pMember->Attrs.push_back(attr);\
	}

#define REF_METHOD0(classname,funname,_ret)	\
	{\
		typedef FunPtrGotter< typename tpl_utility::Functor<_ret,tpl_utility::NullObject,classname > >	GotterType;\
		static GotterType funObj;\
		GotterType* p_fun = &funObj;\
		p_fun->Name = (#funname);\
		p_fun->Caller.Init(&classname::funname);\
		p_fun->GetInfo();\
		ClassType()->Methods.push_back(p_fun);\
	}
#define REF_CONSTRUCTOR0(classname)	\
	{\
		typedef FunPtrGotter< typename tpl_utility::Functor<classname*,tpl_utility::NullObject,classname > >	GotterType;\
		static GotterType funObj;\
		GotterType* p_fun = &funObj;\
		p_fun->IsConstructor = true;\
		p_fun->Name = (#classname);\
		p_fun->Caller.InitConstructor(&tpl_utility::ConstructorTraits<classname,0,GotterType>::Constructor);\
		p_fun->GetInfo();\
		ClassType()->Methods.push_back(p_fun);\
	}

#define REF_METHOD1(classname,funname,_ret,_arg1)	\
	{\
		typedef FunPtrGotter< typename tpl_utility::Functor<_ret,tpl_utility::TYPELIST_1( _arg1 ),classname> >	GotterType;\
		static GotterType funObj;\
		GotterType* p_fun = &funObj;\
		p_fun->Name = (#funname);\
		p_fun->Caller.Init(&classname::funname);\
		p_fun->GetInfo();\
		ClassType()->Methods.push_back(p_fun);\
	}
#define REF_CONSTRUCTOR1(classname,_arg1)	\
	{\
		typedef FunPtrGotter< typename tpl_utility::Functor<classname*,tpl_utility::TYPELIST_1( _arg1 ),classname> >	GotterType;\
		static GotterType funObj;\
		GotterType* p_fun = &funObj;\
		p_fun->IsConstructor = true;\
		p_fun->Name = (#classname);\
		p_fun->Caller.InitConstructor(&tpl_utility::ConstructorTraits<classname,1,GotterType>::Constructor);\
		p_fun->GetInfo();\
		ClassType()->Methods.push_back(p_fun);\
	}

#define REF_METHOD2(classname,funname,_ret,_arg1,_arg2)	\
	{\
		typedef FunPtrGotter< typename tpl_utility::Functor<_ret,tpl_utility::TYPELIST_2( _arg1 , _arg2 ),classname> >	GotterType;\
		static GotterType funObj;\
		GotterType* p_fun = &funObj;\
		p_fun->Name = (#funname);\
		p_fun->Caller.Init(&classname::funname);\
		p_fun->GetInfo();\
		ClassType()->Methods.push_back(p_fun);\
	}
#define REF_CONSTRUCTOR2(classname,_arg1,_arg2)	\
	{\
		typedef FunPtrGotter< typename tpl_utility::Functor<classname*,tpl_utility::TYPELIST_2( _arg1 , _arg2 ),classname> >	GotterType;\
		static GotterType funObj;\
		GotterType* p_fun = &funObj;\
		p_fun->IsConstructor = true;\
		p_fun->Name = (#classname);\
		p_fun->Caller.InitConstructor(&tpl_utility::ConstructorTraits<classname,2,GotterType>::Constructor);\
		p_fun->GetInfo();\
		ClassType()->Methods.push_back(p_fun);\
	}

#define REF_METHOD3(classname,funname,_ret,_arg1,_arg2,_arg3)	\
	{\
		typedef FunPtrGotter< typename tpl_utility::Functor<_ret,tpl_utility::TYPELIST_3( _arg1 , _arg2 , _arg3 ),classname> >	GotterType;\
		static GotterType funObj;\
		GotterType* p_fun = &funObj;\
		p_fun->Name = (#funname);\
		p_fun->Caller.Init(&classname::funname);\
		p_fun->GetInfo();\
		ClassType()->Methods.push_back(p_fun);\
	}
#define REF_CONSTRUCTOR3(classname,_arg1,_arg2,_arg3)	\
	{\
		typedef FunPtrGotter< typename tpl_utility::Functor<classname*,tpl_utility::TYPELIST_3( _arg1 , _arg2 , _arg3 ),classname> >	GotterType;\
		static GotterType funObj;\
		GotterType* p_fun = &funObj;\
		p_fun->IsConstructor = true;\
		p_fun->Name = (#classname);\
		p_fun->Caller.InitConstructor(&tpl_utility::ConstructorTraits<classname,3,GotterType>::Constructor);\
		p_fun->GetInfo();\
		ClassType()->Methods.push_back(p_fun);\
	}

#define REF_METHOD4(classname,funname,_ret,_arg1,_arg2,_arg3,_arg4)	\
	{\
		typedef FunPtrGotter< typename tpl_utility::Functor<_ret,tpl_utility::TYPELIST_4( _arg1 , _arg2 , _arg3 , _arg4 ),classname> >	GotterType;\
		static GotterType funObj;\
		GotterType* p_fun = &funObj;\
		p_fun->Name = (#funname);\
		p_fun->Caller.Init(&classname::funname);\
		p_fun->GetInfo();\
		ClassType()->Methods.push_back(p_fun);\
	}
#define REF_CONSTRUCTOR4(classname,_arg1,_arg2,_arg3,_arg4)	\
	{\
		typedef FunPtrGotter< typename tpl_utility::Functor<classname*,tpl_utility::TYPELIST_4( _arg1 , _arg2 , _arg3 , _arg4 ),classname> >	GotterType;\
		static GotterType funObj;\
		GotterType* p_fun = &funObj;\
		p_fun->IsConstructor = true;\
		p_fun->Name = (#classname);\
		p_fun->Caller.InitConstructor(&tpl_utility::ConstructorTraits<classname,4,GotterType>::Constructor);\
		p_fun->GetInfo();\
		ClassType()->Methods.push_back(p_fun);\
	}

#define REF_METHOD5(classname,funname,_ret,_arg1,_arg2,_arg3,_arg4,_arg5)	\
	{\
		typedef FunPtrGotter< typename tpl_utility::Functor<_ret,tpl_utility::TYPELIST_5( _arg1 , _arg2 , _arg3 , _arg4 , _arg5 ),classname> >	GotterType;\
		static GotterType funObj;\
		GotterType* p_fun = &funObj;\
		p_fun->Name = (#funname);\
		p_fun->Caller.Init(&classname::funname);\
		p_fun->GetInfo();\
		ClassType()->Methods.push_back(p_fun);\
	}
#define REF_CONSTRUCTOR5(classname,_arg1,_arg2,_arg3,_arg4,_arg5)	\
	{\
		typedef FunPtrGotter< typename tpl_utility::Functor<classname*,tpl_utility::TYPELIST_5( _arg1 , _arg2 , _arg3 , _arg4,_arg5 ),classname> >	GotterType;\
		static GotterType funObj;\
		GotterType* p_fun = &funObj;\
		p_fun->IsConstructor = true;\
		p_fun->Name = (#classname);\
		p_fun->Caller.InitConstructor(&tpl_utility::ConstructorTraits<classname,5,GotterType>::Constructor);\
		p_fun->GetInfo();\
		ClassType()->Methods.push_back(p_fun);\
	}

#define REF_METHOD6(classname,funname,_ret,_arg1,_arg2,_arg3,_arg4,_arg5,_arg6)	\
	{\
		typedef FunPtrGotter< typename tpl_utility::Functor<_ret,tpl_utility::TYPELIST_6( _arg1 , _arg2 , _arg3 , _arg4 , _arg5 , _arg6 ),classname> >	GotterType;\
		static GotterType funObj;\
		GotterType* p_fun = &funObj;\
		p_fun->Name = (#funname);\
		p_fun->Caller.Init(&classname::funname);\
		p_fun->GetInfo();\
		ClassType()->Methods.push_back(p_fun);\
	}
#define REF_CONSTRUCTOR6(classname,_arg1,_arg2,_arg3,_arg4,_arg5,_arg6)	\
	{\
		typedef FunPtrGotter< typename tpl_utility::Functor<classname*,tpl_utility::TYPELIST_6( _arg1 , _arg2 , _arg3 , _arg4,_arg5,_arg6 ),classname> >	GotterType;\
		static GotterType funObj;\
		GotterType* p_fun = &funObj;\
		p_fun->IsConstructor = true;\
		p_fun->Name = (#classname);\
		p_fun->Caller.InitConstructor(&tpl_utility::ConstructorTraits<classname,6,GotterType>::Constructor);\
		p_fun->GetInfo();\
		ClassType()->Methods.push_back(p_fun);\
	}

#define REF_METHOD7(classname,funname,_ret,_arg1,_arg2,_arg3,_arg4,_arg5,_arg6,_arg7)	\
	{\
		typedef FunPtrGotter< typename tpl_utility::Functor<_ret,tpl_utility::TYPELIST_7( _arg1 , _arg2 , _arg3 , _arg4 , _arg5 , _arg6 , _arg7 ),classname> >	GotterType;\
		static GotterType funObj;\
		GotterType* p_fun = &funObj;\
		p_fun->Name = (#funname);\
		p_fun->Caller.Init(&classname::funname);\
		p_fun->GetInfo();\
		ClassType()->Methods.push_back(p_fun);\
	}
#define REF_CONSTRUCTOR7(classname,_arg1,_arg2,_arg3,_arg4,_arg5,_arg6,_arg7)	\
	{\
		typedef FunPtrGotter< typename tpl_utility::Functor<classname*,tpl_utility::TYPELIST_7( _arg1 , _arg2 , _arg3 , _arg4,_arg5,_arg6,_arg7 ),classname> >	GotterType;\
		static GotterType funObj;\
		GotterType* p_fun = &funObj;\
		p_fun->IsConstructor = true;\
		p_fun->Name = (#classname);\
		p_fun->Caller.InitConstructor(&tpl_utility::ConstructorTraits<classname,7,GotterType>::Constructor);\
		p_fun->GetInfo();\
		ClassType()->Methods.push_back(p_fun);\
	}

#define REF_METHOD8(classname,funname,_ret,_arg1,_arg2,_arg3,_arg4,_arg5,_arg6,_arg7,_arg8)	\
	{\
		typedef FunPtrGotter< typename tpl_utility::Functor<_ret,tpl_utility::TYPELIST_8( _arg1 , _arg2 , _arg3 , _arg4 , _arg5 , _arg6 , _arg7 , _arg8 ),classname> >	GotterType;\
		static GotterType funObj;\
		GotterType* p_fun = &funObj;\
		p_fun->Name = (#funname);\
		p_fun->Caller.Init(&classname::funname);\
		p_fun->GetInfo();\
		ClassType()->Methods.push_back(p_fun);\
	}
#define REF_CONSTRUCTOR8(classname,_arg1,_arg2,_arg3,_arg4,_arg5,_arg6,_arg7,_arg8)	\
	{\
		typedef FunPtrGotter< typename tpl_utility::Functor<classname*,tpl_utility::TYPELIST_8( _arg1 , _arg2 , _arg3 , _arg4,_arg5,_arg6,_arg7,_arg8 ),classname> >	GotterType;\
		static GotterType funObj;\
		GotterType* p_fun = &funObj;\
		p_fun->IsConstructor = true;\
		p_fun->Name = (#classname);\
		p_fun->Caller.InitConstructor(&tpl_utility::ConstructorTraits<classname,8,GotterType>::Constructor);\
		p_fun->GetInfo();\
		ClassType()->Methods.push_back(p_fun);\
	}

#define END_REFLECT_METHOD	}

#define FILL_PARAMTYPE_2( obj , _Arg ) if( tpl_utility::ClassEqual<_Arg,tpl_utility::NullObject>::Result==false ) {\
		obj.ClassType = ClassExporter<_Arg>::ClassType();\
		obj.IsRefer = tpl_utility::IsRefer<_Arg>::Result;\
		obj.IsPointer = tpl_utility::IsPointer<_Arg>::Result;\
		obj.IsConst = tpl_utility::IsConst<_Arg>::Result;\
	}

	template<typename FunctorType>
	struct RealCall
	{
		template <typename DummyType>
		static typename FunctorType::TReturnType Call( VReflectBase* pBindObject , FunctorType& fun , std::vector<ObjBase*>& pArgs , std::vector<VMethodBase::ParamType>& ArgTypes , tpl_utility::Int2Type< DummyType,0> dummy)
		{
			if(fun.IsGlobalCall)
				return fun.GCall( (typename FunctorType::HostClass*)pBindObject );
			return fun.MCall( (typename FunctorType::HostClass*)pBindObject );
		}
		template <typename DummyType>
		static typename FunctorType::TReturnType Call( VReflectBase* pBindObject , FunctorType& fun , std::vector<ObjBase*>& pArgs , std::vector<VMethodBase::ParamType>& ArgTypes , tpl_utility::Int2Type<DummyType,1> dummy)
		{
			if(fun.IsGlobalCall)
				return fun.GCall( (typename FunctorType::HostClass*)pBindObject ,
					ConvertObject<typename FunctorType::P0>(pArgs[0],ArgTypes[0]) );
			return fun.MCall( (typename FunctorType::HostClass*)pBindObject ,
				ConvertObject<typename FunctorType::P0>(pArgs[0],ArgTypes[0]) );
		}
		template <typename DummyType>
		static typename FunctorType::TReturnType Call( VReflectBase* pBindObject , FunctorType& fun , std::vector<ObjBase*>& pArgs , std::vector<VMethodBase::ParamType>& ArgTypes , tpl_utility::Int2Type<DummyType,2> dummy)
		{
			if(fun.IsGlobalCall)
				return fun.GCall( (typename FunctorType::HostClass*)pBindObject ,
					ConvertObject<typename FunctorType::P0>(pArgs[0],ArgTypes[0]) ,
					ConvertObject<typename FunctorType::P1>(pArgs[1],ArgTypes[1]) );
			return fun.MCall( (typename FunctorType::HostClass*)pBindObject ,
				ConvertObject<typename FunctorType::P0>(pArgs[0],ArgTypes[0]) ,
				ConvertObject<typename FunctorType::P1>(pArgs[1],ArgTypes[1]) );
		}
		template <typename DummyType>
		static typename FunctorType::TReturnType Call( VReflectBase* pBindObject , FunctorType& fun , std::vector<ObjBase*>& pArgs , std::vector<VMethodBase::ParamType>& ArgTypes , tpl_utility::Int2Type<DummyType,3> dummy)
		{
			if(fun.IsGlobalCall)
				return fun.GCall( (typename FunctorType::HostClass*)pBindObject ,
					ConvertObject<typename FunctorType::P0>(pArgs[0],ArgTypes[0]) ,
					ConvertObject<typename FunctorType::P1>(pArgs[1],ArgTypes[1]),
					ConvertObject<typename FunctorType::P2>(pArgs[2],ArgTypes[2]));
			return fun.MCall( (typename FunctorType::HostClass*)pBindObject ,
				ConvertObject<typename FunctorType::P0>(pArgs[0],ArgTypes[0]) ,
				ConvertObject<typename FunctorType::P1>(pArgs[1],ArgTypes[1]),
				ConvertObject<typename FunctorType::P2>(pArgs[2],ArgTypes[2]));
		}
		template <typename DummyType>
		static typename FunctorType::TReturnType Call( VReflectBase* pBindObject , FunctorType& fun , std::vector<ObjBase*>& pArgs , std::vector<VMethodBase::ParamType>& ArgTypes , tpl_utility::Int2Type<DummyType,4> dummy)
		{
			if(fun.IsGlobalCall)
				return fun.GCall( (typename FunctorType::HostClass*)pBindObject ,
					ConvertObject<typename FunctorType::P0>(pArgs[0],ArgTypes[0]) ,
					ConvertObject<typename FunctorType::P1>(pArgs[1],ArgTypes[1]),
					ConvertObject<typename FunctorType::P2>(pArgs[2],ArgTypes[2]),
					ConvertObject<typename FunctorType::P3>(pArgs[3],ArgTypes[3]));
			return fun.MCall( (typename FunctorType::HostClass*)pBindObject ,
				ConvertObject<typename FunctorType::P0>(pArgs[0],ArgTypes[0]) ,
				ConvertObject<typename FunctorType::P1>(pArgs[1],ArgTypes[1]),
				ConvertObject<typename FunctorType::P2>(pArgs[2],ArgTypes[2]),
				ConvertObject<typename FunctorType::P3>(pArgs[3],ArgTypes[3]));
		}
		template <typename DummyType>
		static typename FunctorType::TReturnType Call( VReflectBase* pBindObject , FunctorType& fun , std::vector<ObjBase*>& pArgs , std::vector<VMethodBase::ParamType>& ArgTypes , tpl_utility::Int2Type<DummyType,5> dummy)
		{
			if(fun.IsGlobalCall)
				return fun.GCall( (typename FunctorType::HostClass*)pBindObject ,
					ConvertObject<typename FunctorType::P0>(pArgs[0],ArgTypes[0]) ,
					ConvertObject<typename FunctorType::P1>(pArgs[1],ArgTypes[1]),
					ConvertObject<typename FunctorType::P2>(pArgs[2],ArgTypes[2]),
					ConvertObject<typename FunctorType::P3>(pArgs[3],ArgTypes[3]),
					ConvertObject<typename FunctorType::P4>(pArgs[4],ArgTypes[4]));
			return fun.MCall( (typename FunctorType::HostClass*)pBindObject ,
				ConvertObject<typename FunctorType::P0>(pArgs[0],ArgTypes[0]) ,
				ConvertObject<typename FunctorType::P1>(pArgs[1],ArgTypes[1]),
				ConvertObject<typename FunctorType::P2>(pArgs[2],ArgTypes[2]),
				ConvertObject<typename FunctorType::P3>(pArgs[3],ArgTypes[3]),
				ConvertObject<typename FunctorType::P4>(pArgs[4],ArgTypes[4]));
		}
		template <typename DummyType>
		static typename FunctorType::TReturnType Call( VReflectBase* pBindObject , FunctorType& fun , std::vector<ObjBase*>& pArgs , std::vector<VMethodBase::ParamType>& ArgTypes , tpl_utility::Int2Type<DummyType,6> dummy)
		{
			if(fun.IsGlobalCall)
				return fun.GCall( (typename FunctorType::HostClass*)pBindObject ,
					ConvertObject<typename FunctorType::P0>(pArgs[0],ArgTypes[0]) ,
					ConvertObject<typename FunctorType::P1>(pArgs[1],ArgTypes[1]),
					ConvertObject<typename FunctorType::P2>(pArgs[2],ArgTypes[2]),
					ConvertObject<typename FunctorType::P3>(pArgs[3],ArgTypes[3]),
					ConvertObject<typename FunctorType::P4>(pArgs[4],ArgTypes[4]),
					ConvertObject<typename FunctorType::P5>(pArgs[5],ArgTypes[5]));
			return fun.MCall( (typename FunctorType::HostClass*)pBindObject ,
				ConvertObject<typename FunctorType::P0>(pArgs[0],ArgTypes[0]) ,
				ConvertObject<typename FunctorType::P1>(pArgs[1],ArgTypes[1]),
				ConvertObject<typename FunctorType::P2>(pArgs[2],ArgTypes[2]),
				ConvertObject<typename FunctorType::P3>(pArgs[3],ArgTypes[3]),
				ConvertObject<typename FunctorType::P4>(pArgs[4],ArgTypes[4]),
				ConvertObject<typename FunctorType::P5>(pArgs[5],ArgTypes[5]));
		}
		template <typename DummyType>
		static typename FunctorType::TReturnType Call( VReflectBase* pBindObject , FunctorType& fun , std::vector<ObjBase*>& pArgs , std::vector<VMethodBase::ParamType>& ArgTypes , tpl_utility::Int2Type<DummyType,7> dummy)
		{
			if(fun.IsGlobalCall)
				return fun.GCall( (typename FunctorType::HostClass*)pBindObject ,
					ConvertObject<typename FunctorType::P0>(pArgs[0],ArgTypes[0]) ,
					ConvertObject<typename FunctorType::P1>(pArgs[1],ArgTypes[1]),
					ConvertObject<typename FunctorType::P2>(pArgs[2],ArgTypes[2]),
					ConvertObject<typename FunctorType::P3>(pArgs[3],ArgTypes[3]),
					ConvertObject<typename FunctorType::P4>(pArgs[4],ArgTypes[4]),
					ConvertObject<typename FunctorType::P5>(pArgs[5],ArgTypes[5]),
					ConvertObject<typename FunctorType::P6>(pArgs[6],ArgTypes[6]));
			return fun.MCall( (typename FunctorType::HostClass*)pBindObject ,
				ConvertObject<typename FunctorType::P0>(pArgs[0],ArgTypes[0]) ,
				ConvertObject<typename FunctorType::P1>(pArgs[1],ArgTypes[1]),
				ConvertObject<typename FunctorType::P2>(pArgs[2],ArgTypes[2]),
				ConvertObject<typename FunctorType::P3>(pArgs[3],ArgTypes[3]),
				ConvertObject<typename FunctorType::P4>(pArgs[4],ArgTypes[4]),
				ConvertObject<typename FunctorType::P5>(pArgs[5],ArgTypes[5]),
				ConvertObject<typename FunctorType::P6>(pArgs[6],ArgTypes[6]));
		}
		template <typename DummyType>
		static typename FunctorType::TReturnType Call( VReflectBase* pBindObject , FunctorType& fun , std::vector<ObjBase*>& pArgs , std::vector<VMethodBase::ParamType>& ArgTypes , tpl_utility::Int2Type<DummyType,8> dummy)
		{
			if(fun.IsGlobalCall)
				return fun.GCall( (typename FunctorType::HostClass*)pBindObject ,
					ConvertObject<typename FunctorType::P0>(pArgs[0],ArgTypes[0]) ,
					ConvertObject<typename FunctorType::P1>(pArgs[1],ArgTypes[1]),
					ConvertObject<typename FunctorType::P2>(pArgs[2],ArgTypes[2]),
					ConvertObject<typename FunctorType::P3>(pArgs[3],ArgTypes[3]),
					ConvertObject<typename FunctorType::P4>(pArgs[4],ArgTypes[4]),
					ConvertObject<typename FunctorType::P5>(pArgs[5],ArgTypes[5]),
					ConvertObject<typename FunctorType::P6>(pArgs[6],ArgTypes[6]),
					ConvertObject<typename FunctorType::P7>(pArgs[7],ArgTypes[7]));
			return fun.MCall( (typename FunctorType::HostClass*)pBindObject ,
				ConvertObject<typename FunctorType::P0>(pArgs[0],ArgTypes[0]) ,
				ConvertObject<typename FunctorType::P1>(pArgs[1],ArgTypes[1]),
				ConvertObject<typename FunctorType::P2>(pArgs[2],ArgTypes[2]),
				ConvertObject<typename FunctorType::P3>(pArgs[3],ArgTypes[3]),
				ConvertObject<typename FunctorType::P4>(pArgs[4],ArgTypes[4]),
				ConvertObject<typename FunctorType::P5>(pArgs[5],ArgTypes[5]),
				ConvertObject<typename FunctorType::P6>(pArgs[6],ArgTypes[6]),
				ConvertObject<typename FunctorType::P7>(pArgs[7],ArgTypes[7]));
		}

		//////////////////////////////////////////////////////////////////////////

		template <typename DummyType>
		static typename FunctorType::TReturnType ConstructorCall( FunctorType& fun , std::vector<ObjBase*>& pArgs , std::vector<VMethodBase::ParamType>& ArgTypes , tpl_utility::Int2Type<DummyType,0> dummy)
		{
			return fun.ConstructorCall();
		}
		template <typename DummyType>
		static typename FunctorType::TReturnType ConstructorCall( FunctorType& fun , std::vector<ObjBase*>& pArgs , std::vector<VMethodBase::ParamType>& ArgTypes , tpl_utility::Int2Type<DummyType,1> dummy)
		{
			return fun.ConstructorCall( 
				ConvertObject<typename FunctorType::P0>(pArgs[0],ArgTypes[0]) );
		}
		template <typename DummyType>
		static typename FunctorType::TReturnType ConstructorCall( FunctorType& fun , std::vector<ObjBase*>& pArgs , std::vector<VMethodBase::ParamType>& ArgTypes , tpl_utility::Int2Type<DummyType,2> dummy)
		{
			return fun.ConstructorCall(
				ConvertObject<typename FunctorType::P0>(pArgs[0],ArgTypes[0]) ,
				ConvertObject<typename FunctorType::P1>(pArgs[1],ArgTypes[1]) );
		}
		template <typename DummyType>
		static typename FunctorType::TReturnType ConstructorCall( FunctorType& fun , std::vector<ObjBase*>& pArgs , std::vector<VMethodBase::ParamType>& ArgTypes , tpl_utility::Int2Type<DummyType,3> dummy)
		{
			return fun.ConstructorCall(
				ConvertObject<typename FunctorType::P0>(pArgs[0],ArgTypes[0]) ,
				ConvertObject<typename FunctorType::P1>(pArgs[1],ArgTypes[1]),
				ConvertObject<typename FunctorType::P2>(pArgs[2],ArgTypes[2]));
		}
		template <typename DummyType>
		static typename FunctorType::TReturnType ConstructorCall( FunctorType& fun , std::vector<ObjBase*>& pArgs , std::vector<VMethodBase::ParamType>& ArgTypes , tpl_utility::Int2Type<DummyType,4> dummy)
        {
			return fun.ConstructorCall(
				ConvertObject<typename FunctorType::P0>(pArgs[0],ArgTypes[0]) ,
				ConvertObject<typename FunctorType::P1>(pArgs[1],ArgTypes[1]),
				ConvertObject<typename FunctorType::P2>(pArgs[2],ArgTypes[2]),
				ConvertObject<typename FunctorType::P3>(pArgs[3],ArgTypes[3]));
		}
		template <typename DummyType>
		static typename FunctorType::TReturnType ConstructorCall( FunctorType& fun , std::vector<ObjBase*>& pArgs , std::vector<VMethodBase::ParamType>& ArgTypes , tpl_utility::Int2Type<DummyType,5> dummy )
		{
			return fun.ConstructorCall(
				ConvertObject<typename FunctorType::P0>(pArgs[0],ArgTypes[0]) ,
				ConvertObject<typename FunctorType::P1>(pArgs[1],ArgTypes[1]),
				ConvertObject<typename FunctorType::P2>(pArgs[2],ArgTypes[2]),
				ConvertObject<typename FunctorType::P3>(pArgs[3],ArgTypes[3]),
				ConvertObject<typename FunctorType::P4>(pArgs[4],ArgTypes[4]));
		}
		template <typename DummyType>
		static typename FunctorType::TReturnType ConstructorCall( FunctorType& fun , std::vector<ObjBase*>& pArgs , std::vector<VMethodBase::ParamType>& ArgTypes , tpl_utility::Int2Type<DummyType,6> dummy )
		{
			return fun.ConstructorCall(
				ConvertObject<typename FunctorType::P0>(pArgs[0],ArgTypes[0]) ,
				ConvertObject<typename FunctorType::P1>(pArgs[1],ArgTypes[1]),
				ConvertObject<typename FunctorType::P2>(pArgs[2],ArgTypes[2]),
				ConvertObject<typename FunctorType::P3>(pArgs[3],ArgTypes[3]),
				ConvertObject<typename FunctorType::P4>(pArgs[4],ArgTypes[4]),
				ConvertObject<typename FunctorType::P5>(pArgs[5],ArgTypes[5]));
		}
		template <typename DummyType>
		static typename FunctorType::TReturnType ConstructorCall( FunctorType& fun , std::vector<ObjBase*>& pArgs , std::vector<VMethodBase::ParamType>& ArgTypes , tpl_utility::Int2Type<DummyType,7> dummy)
		{
			return fun.ConstructorCall(
				ConvertObject<typename FunctorType::P0>(pArgs[0],ArgTypes[0]) ,
				ConvertObject<typename FunctorType::P1>(pArgs[1],ArgTypes[1]),
				ConvertObject<typename FunctorType::P2>(pArgs[2],ArgTypes[2]),
				ConvertObject<typename FunctorType::P3>(pArgs[3],ArgTypes[3]),
				ConvertObject<typename FunctorType::P4>(pArgs[4],ArgTypes[4]),
				ConvertObject<typename FunctorType::P5>(pArgs[5],ArgTypes[5]),
				ConvertObject<typename FunctorType::P6>(pArgs[6],ArgTypes[6]));
		}
		template <typename DummyType>
		static typename FunctorType::TReturnType ConstructorCall( FunctorType& fun , std::vector<ObjBase*>& pArgs , std::vector<VMethodBase::ParamType>& ArgTypes , tpl_utility::Int2Type<DummyType,8> dummy)
		{
			return fun.ConstructorCall(
				ConvertObject<typename FunctorType::P0>(pArgs[0],ArgTypes[0]) ,
				ConvertObject<typename FunctorType::P1>(pArgs[1],ArgTypes[1]),
				ConvertObject<typename FunctorType::P2>(pArgs[2],ArgTypes[2]),
				ConvertObject<typename FunctorType::P3>(pArgs[3],ArgTypes[3]),
				ConvertObject<typename FunctorType::P4>(pArgs[4],ArgTypes[4]),
				ConvertObject<typename FunctorType::P5>(pArgs[5],ArgTypes[5]),
				ConvertObject<typename FunctorType::P6>(pArgs[6],ArgTypes[6]),
				ConvertObject<typename FunctorType::P7>(pArgs[7],ArgTypes[7]));
		}
	};

	struct ProcReturn
	{
		template< typename type >
		void Proc(ObjBase& Ret,type innerRet)
		{
			Ret.p = innerRet;
		}
		void Proc(ObjBase& Ret,FLOAT innerRet)
		{
			Ret.f = innerRet;
		}
		void Proc(ObjBase& Ret,DOUBLE innerRet)
		{
			Ret.d = innerRet;
		}
		void Proc(ObjBase& Ret,INT8 innerRet)
		{
			Ret.i8 = innerRet;
		}
		void Proc(ObjBase& Ret,INT16 innerRet)
		{
			Ret.i16 = innerRet;
		}
		void Proc(ObjBase& Ret,INT32 innerRet)
		{
			Ret.i32 = innerRet;
		}
		void Proc(ObjBase& Ret,INT64 innerRet)
		{
			Ret.i64 = innerRet;
		}
		void Proc(ObjBase& Ret,UINT8 innerRet)
		{
			Ret.ui8 = innerRet;
		}
		void Proc(ObjBase& Ret,UINT16 innerRet)
		{
			Ret.ui16 = innerRet;
		}
		void Proc(ObjBase& Ret,UINT32 innerRet)
		{
			Ret.ui32 = innerRet;
		}
		void Proc(ObjBase& Ret,UINT64 innerRet)
		{
			Ret.ui64 = innerRet;
		}
		void Proc(ObjBase& Ret,CHAR* innerRet)
		{
			Ret.pChar = innerRet;
		}
		void Proc(ObjBase& Ret,WCHAR* innerRet)
		{
			Ret.pWChar = innerRet;
		}
	};

	template<typename FunctorType,bool HasReturn>
	struct Invoke_Selector
	{
		static void Invoker( ObjBase* retValue , VReflectBase* pBindObject , std::vector<ObjBase*>& pArgs , FunctorType& Caller , std::vector<VMethodBase::ParamType>& ArgTypes)
		{
			typename FunctorType::TReturnType ret = RealCall<FunctorType>::Call( pBindObject , Caller , pArgs , ArgTypes , tpl_utility::Int2Type<int,FunctorType::ParamNum>() );
			if(retValue)
			{
				ProcReturn proc;
				proc.Proc( *retValue , ret );
			}
		}
	};

	template<typename FunctorType>
	struct Invoke_Selector<FunctorType,false>
	{
		static void Invoker( ObjBase* retValue , VReflectBase* pBindObject , std::vector<ObjBase*>& pArgs , FunctorType& Caller , std::vector<VMethodBase::ParamType>& ArgTypes )
		{
			RealCall<FunctorType>::Call( pBindObject , Caller , pArgs , ArgTypes , tpl_utility::Int2Type<int,FunctorType::ParamNum>() );
		}
	};

	template<typename FunctorType,bool HasReturn>
	struct Constructor_Selector
	{
		static void Invoker( void** ppRetValue ,std::vector<ObjBase*>& pArgs , FunctorType& Caller, std::vector<VMethodBase::ParamType>& ArgTypes)
		{
			typename FunctorType::TReturnType ret = RealCall<FunctorType>::ConstructorCall( Caller , pArgs , ArgTypes , tpl_utility::Int2Type<int,FunctorType::ParamNum>() );
			*ppRetValue = *(void**)&ret;
		}
	};
	
	template<typename FunctorType>
	struct Constructor_Selector<FunctorType,false>
	{
		static void* Invoker( void** ppRetValue ,std::vector<ObjBase*>& pArgs , FunctorType& Caller, std::vector<VMethodBase::ParamType>& ArgTypes)
		{
			*ppRetValue = NULL;
			return NULL;
		}
	};

	template< typename FunctorType >
	struct FunPtrGotter : VMethodBase
	{
		typedef FunctorType		Functor;
		FunctorType				Caller;
		void GetInfo()
		{
			//Address = reinterpret_cast<VMethodBase::FunAdress>(Caller.Mfun);
			ArgTypes.resize( FunctorType::ParamNum );
			FILL_PARAMTYPE_2(ReturnType,typename FunctorType::TReturnType);

			FILL_PARAMTYPE_2(ArgTypes[0],typename FunctorType::P0);
			FILL_PARAMTYPE_2(ArgTypes[1],typename FunctorType::P1);
			FILL_PARAMTYPE_2(ArgTypes[2],typename FunctorType::P2);
			FILL_PARAMTYPE_2(ArgTypes[3],typename FunctorType::P3);
			FILL_PARAMTYPE_2(ArgTypes[4],typename FunctorType::P4);
			FILL_PARAMTYPE_2(ArgTypes[5],typename FunctorType::P5);
			FILL_PARAMTYPE_2(ArgTypes[6],typename FunctorType::P6);
			FILL_PARAMTYPE_2(ArgTypes[7],typename FunctorType::P7);
		}

#if !defined(PLATFORM_WIN)
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-source-encoding"
#endif
		virtual void Invoke( ObjBase* retValue , VReflectBase* pBindObject , std::vector<ObjBase*>& pArgs )
		{
			if( pArgs.size()<ArgTypes.size() )
				throw "Invoke";
			/*static const bool HasReturn = (tpl_utility::ClassEqual<tpl_utility::NullObject,typename FunctorType::TReturnType>::Result==false
				&& tpl_utility::ClassEqual<void,typename FunctorType::TReturnType>::Result==false );*/
			Invoke_Selector<FunctorType,FunctorType::HasReturn>::Invoker(retValue,pBindObject,pArgs,Caller,ArgTypes);
		};        
        

		//////////////////////////////////////////////////////////////////////////
		virtual void Constructor( void** ppRetValue , std::vector<ObjBase*>& pArgs )
		{
			if( pArgs.size()<ArgTypes.size() )
				throw "Constructor";

			Constructor_Selector<FunctorType,FunctorType::HasReturn>::Invoker(ppRetValue,pArgs,Caller,ArgTypes);
		}
#if !defined(PLATFORM_WIN)
#pragma clang diagnostic pop
#endif
	};

}