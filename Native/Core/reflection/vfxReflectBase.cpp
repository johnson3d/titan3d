#include "../precompile.h"
#include "vfxReflectBase.h"

#define new VNEW

namespace Reflection
{
	typedef std::vector< VAssembly*,HeapAllocator<VAssembly*> >		AsmVector;
	AsmVector* GetAsms(){
		static AsmVector m_Asms;
		return &m_Asms;
	}
	struct AAA
	{
		LONG m_lRef;
	};

	template<typename T,typename TM, TM T::* Var >
	struct HasMember
	{
		static const bool Result = false;
	};

	template<typename T,typename TMV>
	struct HasRef
	{
		static const bool Result = true;
	};
	template<typename T>
	struct HasRef< T, LONG T::*>
	{
		static const bool Result = false;
	};

	class C1{
	public:
		int V1;
		float V2;
		template<typename Ty,  Ty C1::* Var>
		Ty getvalue( Ty C1::*)
		{
			return Ty();
		}
	};
	

	void test()
	{
		//ClassExporter<double*>::ClassType;
		//VClassType ClassExporter<type>::ClassTypeObj(id,_T(#type));
		/*AAA().m_lRef;

		C1 c1;
		c1.getvalue<int,&C1::V1>(&C1::V1);
		c1.getvalue<float,&C1::V2>(&C1::V2);
		
		bool ret = HasMember<AAA,LONG,&AAA::m_lRef>::Result;

		HasRef<AAA,int>::Result;*/
	}

	VClassType::~VClassType()
	{
		/*for( size_t i=0 ; i<Methods.size() ; i++ )
		{
			delete Methods[i];
		}*/
		Methods.clear();
	}

	VAssembly::VAssembly()
	{
		GetAsms()->push_back(this);
	}

	VAssembly::~VAssembly()
	{
		/*for( std::map< LPCSTR , VClassType* ,String_Less >::iterator i= m_NameTypes.begin() ; i!=m_NameTypes.end() ; ++i )
		{
			delete i->second;
		}*/
		m_NameTypes.clear();

		/*for( std::map< vIID , VClassType* >::iterator i= m_IIDTypes.begin() ; i!=m_IIDTypes.end() ; ++i )
		{
			delete i->second;
		}*/
		m_IIDTypes.clear();

		for( AsmVector::iterator i=GetAsms()->begin() ; i!=GetAsms()->end() ; ++i )
		{
			if( *i == this )
			{
				GetAsms()->erase( i );
				return ;
			}
		}
	}

	void VAssembly::RegClassType( LPCSTR name , const vIID& id , VClassType* pType )
	{
		m_NameTypes.insert( std::make_pair(name,pType) );
		m_IIDTypes.insert( std::make_pair(id,pType) );
	}

	VClassType* VAssembly::FindClassType( vIID id )
	{
		IIDMap::iterator i = m_IIDTypes.find( id );
		if( i!=m_IIDTypes.end() )
		{
			return i->second;
		}
		return NULL;
	}

	VClassType* VAssembly::FindClassType( LPCSTR name )
	{
		NameMap::iterator i = m_NameTypes.find( name );
		if( i!=m_NameTypes.end() )
		{
			return i->second;
		}
		return NULL;
	}

	VClassType*	VAssembly::FindClassTypeInWhole(vIID id)
	{
		for( AsmVector::iterator i=GetAsms()->begin() ; i!=GetAsms()->end() ; ++i )
		{
			VClassType* pType = (*i)->FindClassType( id );
			if( pType )
				return pType;
		}
		return NULL;
	}

	VClassType*	VAssembly::FindClassTypeInWhole(LPCSTR name)
	{
		for( AsmVector::iterator i=GetAsms()->begin() ; i!=GetAsms()->end() ; ++i )
		{
			VClassType* pType = (*i)->FindClassType( name );
			if( pType )
				return pType;
		}
		return NULL;
	}
}