#pragma once

#include "vfxReflectionMethod.h"

namespace tpl_utility
{
	template<typename FunctorType>struct DeserialCall
	{
		template <typename DummyType>        
		static typename FunctorType::ReturnType Call( Reflection::VReflectBase* pBindObject , typename FunctorType& fun , VFile_Base& io , tpl_utility::Int2Type<DummyType,1> dummy=tpl_utility::Int2Type<DummyType,1>() )        
		{
			FunctorType::P0 arg0;                
			io.Read( &arg0 , sizeof(FunctorType::P0) );                
			return fun.MCall( pBindObject , arg0 );        
		}        
		template <typename DummyType>        
		static typename FunctorType::ReturnType Call( Reflection::VReflectBase* pBindObject , typename FunctorType& fun , VFile_Base& io , tpl_utility::Int2Type<DummyType,2> dummy=tpl_utility::Int2Type<DummyType,2>() )        
		{
			FunctorType::P0 arg0;                
			FunctorType::P0 arg1;                
			io.Read( &arg0 , sizeof(FunctorType::P0) );                
			io.Read( &arg1 , sizeof(FunctorType::P1) );                
			return fun.MCall( pBindObject , arg0 , arg1 );        
		}
	};
	
	struct VCallerBase
	{        
		typedef void ( Reflection::VReflectBase::*FunAdress )();        
		FunAdress                       Address;        
		virtual void Process() = 0;
	};
	
	template< typename FunctorType >struct AsyncCaller : public VCallerBase
	{        
		FunctorType                             Caller;        
		VMemFile                                io;        
		typedef typename FunctorType::ReturnType                ReturnType;        
		typedef typename FunctorType::P0                                P0;        
		typedef typename FunctorType::P1                                P1;        
		template< typename BindClass >        
		AsyncCaller( ReturnType(BindClass::*MemberFun)(P0), typename FunctorType::P0 arg0)        
		{                
			Caller.Init(MemberFun);                
			io.Open(NULL,0);                
			io.Write( &arg0 , sizeof(FunctorType::P0) );        
		}        
		
		template< typename BindClass >        
		AsyncCaller( ReturnType(BindClass::*MemberFun)(P0,P1), typename FunctorType::P0 arg0,typename FunctorType::P1 arg1)        
		{                
			Caller.Init(MemberFun);                
			io.Open(NULL,0);                
			io.Write( &arg0 , sizeof(FunctorType::P0) );                
			io.Write( &arg1 , sizeof(FunctorType::P1) );        
		}        
		virtual void Process()        
		{                
			DeserialCall<FunctorType>::Call( NULL , Caller , io , tpl_utility::Int2Type<int,FunctorType::ParamNum>() );        
		}
	};
	
	class TestCaller
	{
	public:        
		void Do(int a)        
		{                
			//typeof(a) aa;        
		}        
		void Do(int a,float b)        
		{        
		}
	};
	
	void MyTest()
	{
		typedef Functor<void,TYPELIST_1( int )> DoFuncType;        
		AsyncCaller<DoFuncType> obj( &TestCaller::Do , int(5) );        
		obj.Process();
	}
}