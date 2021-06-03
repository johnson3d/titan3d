#pragma once

#include <vector>

namespace DynSimulate
{
	class VDelegateDummy
	{
		
	};

	struct VInvoker
	{
		typedef void ( VDelegateDummy::*FunAdress )();
		void*		BindObject;
		FunAdress	Address;
		VInvoker()
		{
			BindObject = NULL;
			Address = NULL;
		}
		void Cleanup()
		{
			BindObject = NULL;
		}
	};

	struct VDelegateBase
	{
	protected:
		std::vector<VInvoker>		Methords;
	public:
		~VDelegateBase()
		{
			for( size_t i=0 ; i<Methords.size() ; i++ )
			{
				Methords[i].Cleanup();
			}
		}

		template <class _Type>
		bool HasDelegate( void* obj,_Type addr )
		{
			for( size_t i=0 ; i<Methords.size() ; i++ )
			{
				_Type nf = reinterpret_cast<_Type>(Methords[i].Address);
				if( Methords[i].BindObject==obj && nf == addr )
				{
					return true;
				}
			}
			return false;
		}

		template <class _Type>
		bool RemoveDelegate(void* obj,_Type addr)
		{
			for( size_t i=0 ; i<Methords.size() ; i++ )
			{
				_Type nf = reinterpret_cast<_Type>(Methords[i].Address);
				if( Methords[i].BindObject==obj && nf==addr )
				{
					Methords.erase( Methords.begin() + i );
					return true;
				}
			}
		}

		void RemoveAll()
		{
			for( size_t i=0 ; i<Methords.size() ; i++ )
			{
				Methords[i].Cleanup();
			}
			Methords.clear();
		}
	};

	template< class _Return >
	struct VDelegate0 : public VDelegateBase
	{
		template<class BindClass>
		void AddDelegate( BindClass* obj , _Return (BindClass::*MemberFun)() )
		{
			if( HasDelegate(obj,MemberFun) )
				return;

			VInvoker tmp;
			
			tmp.BindObject = obj;
			tmp.Address = reinterpret_cast<VInvoker::FunAdress>(MemberFun);

			Methords.push_back( tmp );
		}

		void operator()()
		{
			typedef _Return( VDelegateDummy::*PMA )();

			for( size_t i=0 ; i<Methords.size() ; i++ )
			{
				PMA nf = reinterpret_cast<PMA>(Methords[i].Address);
				(((VDelegateDummy*)Methords[i].BindObject)->*(nf))();
			}
		}
	};

	template< class _Return , class _Arg1>
	struct VDelegate1 : public VDelegateBase
	{
		template<class BindClass>
		void AddDelegate( BindClass* obj , _Return (BindClass::*MemberFun)(_Arg1) )
		{
			if( HasDelegate(obj,MemberFun) )
				return;

			VInvoker tmp;
			typedef _Return (BindClass::*PMA)(_Arg1);
			
			tmp.BindObject = obj;
			tmp.Address = reinterpret_cast<VInvoker::FunAdress>(MemberFun);;

			Methords.push_back( tmp );
		}

		void operator()(_Arg1 a1)
		{
			typedef _Return( VDelegateDummy::*PMA )(_Arg1);

			for( size_t i=0 ; i<Methords.size() ; i++ )
			{
				PMA nf = reinterpret_cast<PMA>(Methords[i].Address);
				(((VDelegateDummy*)Methords[i].BindObject)->*(nf))(a1);
			}
		}
	};

	template< class _Return , class _Arg1 , class _Arg2>
	struct VDelegate2 : public VDelegateBase
	{
		template<class BindClass>
		void AddDelegate( BindClass* obj , _Return (BindClass::*MemberFun)(_Arg1,_Arg2) )
		{
			if( HasDelegate(obj,MemberFun) )
				return;

			VInvoker tmp;
			
			tmp.BindObject = obj;
			tmp.Address = reinterpret_cast<VInvoker::FunAdress>(MemberFun);

			Methords.push_back( tmp );
		}

		void operator()(_Arg1 a1,_Arg2 a2)
		{
			typedef _Return( VDelegateDummy::*PMA )(_Arg1,_Arg2);

			for( size_t i=0 ; i<Methords.size() ; i++ )
			{
				PMA nf = reinterpret_cast<PMA>(Methords[i].Address);
				(((VDelegateDummy*)Methords[i].BindObject)->*(nf))(a1,a2);
			}
		}
	};

	template< class _Return , class _Arg1 , class _Arg2 , class _Arg3>
	struct VDelegate3 : public VDelegateBase
	{
		template<class BindClass>
		void AddDelegate( BindClass* obj , _Return (BindClass::*MemberFun)(_Arg1,_Arg2,_Arg3) )
		{
			if( HasDelegate(obj,MemberFun) )
				return;

			VInvoker tmp;
			
			tmp.BindObject = obj;
			tmp.Address = reinterpret_cast<VInvoker::FunAdress>(MemberFun);

			Methords.push_back( tmp );
		}

		void operator()(_Arg1 a1,_Arg2 a2,_Arg3 a3)
		{
			typedef _Return( VDelegateDummy::*PMA )(_Arg1,_Arg2,_Arg3);

			for( size_t i=0 ; i<Methords.size() ; i++ )
			{
				PMA nf = reinterpret_cast<PMA>(Methords[i].Address);
				(((VDelegateDummy*)Methords[i].BindObject)->*(nf))(a1,a2,a3);
			}
		}
	};

	template< class _Return , class _Arg1 , class _Arg2 , class _Arg3 , class _Arg4 >
	struct VDelegate4 : public VDelegateBase
	{
		template<class BindClass>
		void AddDelegate( BindClass* obj , _Return (BindClass::*MemberFun)(_Arg1,_Arg2,_Arg3,_Arg4) )
		{
			if( HasDelegate(obj,MemberFun) )
				return;

			VInvoker tmp;
			
			tmp.BindObject = obj;
			tmp.Address = reinterpret_cast<VInvoker::FunAdress>(MemberFun);

			Methords.push_back( tmp );
		}

		void operator()(_Arg1 a1,_Arg2 a2,_Arg3 a3,_Arg4 a4)
		{
			typedef _Return( VDelegateDummy::*PMA )(_Arg1,_Arg2,_Arg3,_Arg4);

			for( size_t i=0 ; i<Methords.size() ; i++ )
			{
				PMA nf = reinterpret_cast<PMA>(Methords[i].Address);
				(((VDelegateDummy*)Methords[i].BindObject)->*(nf))(a1,a2,a3,a4);
			}
		}
	};

	template< class _Return , class _Arg1 , class _Arg2 , class _Arg3 , class _Arg4 , class _Arg5 >
	struct VDelegate5 : public VDelegateBase
	{
		template<class BindClass>
		void AddDelegate( BindClass* obj , _Return (BindClass::*MemberFun)(_Arg1,_Arg2,_Arg3,_Arg4,_Arg5) )
		{
			if( HasDelegate(obj,MemberFun) )
				return;

			VInvoker tmp;

			tmp.BindObject = obj;
			tmp.Address = reinterpret_cast<VInvoker::FunAdress>(MemberFun);

			Methords.push_back( tmp );
		}

		void operator()(_Arg1 a1,_Arg2 a2,_Arg3 a3,_Arg4 a4,_Arg5 a5)
		{
			typedef _Return( VDelegateDummy::*PMA )(_Arg1,_Arg2,_Arg3,_Arg4,_Arg5);

			for( size_t i=0 ; i<Methords.size() ; i++ )
			{
				PMA nf = reinterpret_cast<PMA>(Methords[i].Address);
				(((VDelegateDummy*)Methords[i].BindObject)->*(nf))(a1,a2,a3,a4,a5);
			}
		}
	};

	template< class _Return , class _Arg1 , class _Arg2 , class _Arg3 , class _Arg4 , class _Arg5 , class _Arg6 >
	struct VDelegate6 : public VDelegateBase
	{
		template<class BindClass>
		void AddDelegate( BindClass* obj , _Return (BindClass::*MemberFun)(_Arg1,_Arg2,_Arg3,_Arg4,_Arg5,_Arg6) )
		{
			if( HasDelegate(obj,MemberFun) )
				return;

			VInvoker tmp;

			tmp.BindObject = obj;
			tmp.Address = reinterpret_cast<VInvoker::FunAdress>(MemberFun);

			Methords.push_back( tmp );
		}

		void operator()(_Arg1 a1,_Arg2 a2,_Arg3 a3,_Arg4 a4,_Arg5 a5,_Arg6 a6)
		{
			typedef _Return( VDelegateDummy::*PMA )(_Arg1,_Arg2,_Arg3,_Arg4,_Arg5,_Arg6);

			for( size_t i=0 ; i<Methords.size() ; i++ )
			{
				PMA nf = reinterpret_cast<PMA>(Methords[i].Address);
				(((VDelegateDummy*)Methords[i].BindObject)->*(nf))(a1,a2,a3,a4,a5,a6);
			}
		}
	};

	template< class _Return , class _Arg1 , class _Arg2 , class _Arg3 , class _Arg4 , class _Arg5 , class _Arg6 , class _Arg7 >
	struct VDelegate7 : public VDelegateBase
	{
		template<class BindClass>
		void AddDelegate( BindClass* obj , _Return (BindClass::*MemberFun)(_Arg1,_Arg2,_Arg3,_Arg4,_Arg5,_Arg6,_Arg7) )
		{
			if( HasDelegate(obj,MemberFun) )
				return;

			VInvoker tmp;

			tmp.BindObject = obj;
			tmp.Address = reinterpret_cast<VInvoker::FunAdress>(MemberFun);

			Methords.push_back( tmp );
		}

		void operator()(_Arg1 a1,_Arg2 a2,_Arg3 a3,_Arg4 a4,_Arg5 a5,_Arg6 a6,_Arg7 a7)
		{
			typedef _Return( VDelegateDummy::*PMA )(_Arg1,_Arg2,_Arg3,_Arg4,_Arg5,_Arg6,_Arg7);

			for( size_t i=0 ; i<Methords.size() ; i++ )
			{
				PMA nf = reinterpret_cast<PMA>(Methords[i].Address);
				(((VDelegateDummy*)Methords[i].BindObject)->*(nf))(a1,a2,a3,a4,a5,a6,a7);
			}
		}
	};

	template< class _Return , class _Arg1 , class _Arg2 , class _Arg3 , class _Arg4 , class _Arg5 , class _Arg6 , class _Arg7 , class _Arg8 >
	struct VDelegate8 : public VDelegateBase
	{
		template<class BindClass>
		void AddDelegate( BindClass* obj , _Return (BindClass::*MemberFun)(_Arg1,_Arg2,_Arg3,_Arg4,_Arg5,_Arg6,_Arg7,_Arg8) )
		{
			if( HasDelegate(obj,MemberFun) )
				return;

			VInvoker tmp;

			tmp.BindObject = obj;
			tmp.Address = reinterpret_cast<VInvoker::FunAdress>(MemberFun);

			Methords.push_back( tmp );
		}

		void operator()(_Arg1 a1,_Arg2 a2,_Arg3 a3,_Arg4 a4,_Arg5 a5,_Arg6 a6,_Arg7 a7,_Arg8 a8)
		{
			typedef _Return( VDelegateDummy::*PMA )(_Arg1,_Arg2,_Arg3,_Arg4,_Arg5,_Arg6,_Arg7,_Arg8);

			for( size_t i=0 ; i<Methords.size() ; i++ )
			{
				PMA nf = reinterpret_cast<PMA>(Methords[i].Address);
				(((VDelegateDummy*)Methords[i].BindObject)->*(nf))(a1,a2,a3,a4,a5,a6,a7,a8);
			}
		}
	};

	///example

	namespace Example
	{
		struct TT1
		{
			float Fun1(){
				return 0;
			}
			float Fun2(int){
				return 0;
			}
		};

		void _test_function()
		{
			TT1 o;
			VDelegate0<float> t;
			t.AddDelegate( &o , &TT1::Fun1 );
			//t.RemoveDelgate(&o,&TT::Fun2);
			t();

			VDelegate1<float,int> t2;
			t2.AddDelegate( &o , &TT1::Fun2 );
			t2(3);
		}
	}
}