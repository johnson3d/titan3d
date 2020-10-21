// ***************************************************************
//  vfxReflectBase   version:  1.0   ��  date: 04/02/2008
//  -------------------------------------------------------------
//  johnson
//  -------------------------------------------------------------
//  Copyright (C) 2008 - All Rights Reserved
// ***************************************************************
// 
// ***************************************************************

#pragma once

#include <map>
#include <vector>

#include "../../BaseHead.h"
#include "../generic/vfxTemplateUtil.h"

#define DYNAMIC_REFLECT(prefix,iid) prefix virtual const Reflection::VClassType* GetClassType() const; public:static const vIID __UID__ = iid;
#if defined(PLATFORM_WIN)
#	define IMP_DYNAMIC_REFLECT(classname)	const Reflection::VClassType* classname::GetClassType() const\
		{\
			return Reflection::ClassExporter<classname>::ClassType();\
		} 
#else
#	define IMP_DYNAMIC_REFLECT(classname)	const Reflection::VClassType* classname::GetClassType() const\
		{\
			return Reflection::ClassExporter<classname>::ClassType();\
		} \
		const vIID classname::__UID__;
#endif
	
namespace Reflection
{
	struct VClassType;
	struct VReflectBase;
	struct VMethodBase;

	enum ObjType
	{
		OT_Void,
		OT_Enum,
		OT_Float,
		OT_Double,
		OT_Int8,
		OT_Int16,
		OT_Int32,
		OT_Int64,
		OT_UInt8,
		OT_UInt16,
		OT_UInt32,
		OT_UInt64,
		OT_ReflectObject,
		OT_AnyPtr,
	};

	union ObjBase
	{
		float			f;
		double			d;
		INT8			i8;
		INT16			i16;
		INT32			i32;
		INT64			i64;
		UINT8			ui8;
		UINT16			ui16;
		UINT32			ui32;
		UINT64			ui64;

		int				e;
		VReflectBase*	p;
		void*			any_ptr;
		CHAR*			pChar;
		WCHAR*			pWChar;

		void operator=(float v){
			f = v;
		}
		void operator=(double v){
			d = v;
		}
		void operator=(INT8 v){
			i8 = v;
		}
		void operator=(INT16 v){
			i16 = v;
		}
		void operator=(INT32 v){
			i32 = v;
		}
		void operator=(INT64 v){
			i64 = v;
		}
		void operator=(UINT8 v){
			ui8 = v;
		}
		void operator=(UINT16 v){
			ui16 = v;
		}
		void operator=(UINT32 v){
			ui32 = v;
		}
		void operator=(UINT64 v){
			ui64 = v;
		}
		void operator=(VReflectBase* v){
			p = v;
		}
	};

	struct VReflectBase
	{
		virtual ~VReflectBase()
		{
		}
		 static ObjBase			ReturnTemp;
		 virtual const VClassType* GetClassType() const{ return NULL;}
	};

	struct VAttribute : public VReflectBase
	{
		DYNAMIC_REFLECT(,0xfa2a151c4b98be54);
	};

	template <class T, size_t dwInitialSize = 4*1024, size_t dwMaximumSize = 4*1024>
	class HeapAllocator
	{
	public:
		typedef T                value_type;
		typedef T*               pointer;
		typedef const T*         const_pointer;
		typedef T&               reference;
		typedef const T&         const_reference;
		typedef size_t           size_type;
		typedef ptrdiff_t        difference_type;

		HeapAllocator()
		{
		}

		HeapAllocator(const HeapAllocator&)
		{
		}

		~HeapAllocator()
		{
		}

		template <class U>
		HeapAllocator(const HeapAllocator<U>& )
		{
			
		}

		template <class U>
		struct rebind{typedef HeapAllocator<U> other;};

		pointer address(value_type& x)
		{
			return &x;
		}

		const_pointer address(const value_type& x)
		{
			return &x;
		}

		pointer allocate(size_type n, const_pointer = 0)
		{
			void* p = malloc( n*sizeof(value_type) );
			memset(p,0,n*sizeof(value_type));
			return static_cast<pointer>(p);
		}

		void deallocate(pointer p, size_type)
		{
			free( p );
		}

		size_type max_size() const
		{
			return static_cast<size_type>(dwInitialSize/sizeof(value_type));
		}

		void construct(pointer p, const value_type& x)
		{
			new(p) value_type(x);
		}

		void destroy(pointer p)
		{
			p->~value_type();
		}
	};

	struct VClassType
	{
		struct VMember
		{
			VMember( VClassType* t , LPCSTR n , UINT o , vBOOL bC , vBOOL bP )
				: Type(t)
				, Name(n)
				, Offset(o)
				, IsConst(bC)
				, IsPointer(bP)
			{

			}
			virtual ~VMember()
			{
				for( size_t i=0 ; i<Attrs.size() ; i++ )
				{
					delete Attrs[i];
				}
				Attrs.clear();
			}
			VClassType*		Type;
			LPCSTR			Name;
			UINT			Offset;

			vBOOL			IsConst;
			vBOOL			IsPointer;
			std::vector< VAttribute*,HeapAllocator<VAttribute*> >	Attrs;
		};

		VClassType()
			: Super(NULL)
			, ClassID(0)
			, CreateObject(NULL)
		{
			Type = OT_ReflectObject;
		}
		VClassType( ObjType t , LPCSTR fn , vIID id ,VClassType* s=NULL )
			: Super(s)
			, CreateObject(NULL)
		{
			Type = t;
			ClassID = id;
			FullName = fn;
		}
		 ~VClassType();
		VClassType*			Super;

		ObjType				Type;
		LPCSTR				FullName;
		vIID				ClassID;

		std::vector< VMember,HeapAllocator<VMember> >			Members;
		std::vector<VMethodBase*,HeapAllocator<VMethodBase*> >		Methods;

		typedef void*(*fnCreateObject)();

		fnCreateObject		CreateObject;

		template< typename type >
		type* vfxCreateObject() const{
			if( CreateObject ){
				VReflectBase* pObj = (VReflectBase*)CreateObject();
				return ClassConverter(pObj,(type*)0);
			}
			return NULL;
		}

		template< typename type >
		type* vfxCreateObjectUnSafe() const{
			if( CreateObject ){
				return (type*)CreateObject();
			}
			return NULL;
		}

		ObjType GetSuperType() const {
			return Type;
		}

		bool CanConvertTo( const VClassType* pType ) const{
			if( this==pType )
				return true;
			else if( Super!=NULL )
				return Super->CanConvertTo(pType);
			else
				return false;				
		}

		bool CanConvertTo( const vIID& id ) const{
			if( this->ClassID==id )
				return true;
			else if( Super!=NULL )
				return Super->CanConvertTo(id);
			else
				return false;				
		}

		template<class type>
		type& GetMember(void* pHostObj , int i) const{
			return (reinterpret_cast<type*>(reinterpret_cast<UINT_PTR>(pHostObj) + Members[i].Offset))[0];
		}
	};

	struct VMethodBase
	{
		struct ParamType
		{
			typedef VClassType*	VClassTypePtr;
			VClassType*			ClassType;
			vBOOL				IsConst;
			vBOOL				IsPointer;
			vBOOL				IsRefer;

			operator VClassTypePtr (){
				return ClassType;
			}
		};
		VMethodBase()
			: IsConstructor(false)
		{
			
		}
		~VMethodBase()
		{
			for( size_t i=0 ; i<Attrs.size() ; i++ )
			{
				delete Attrs[i];
			}
			Attrs.clear();
		}
		typedef void ( VReflectBase::*FunAdress )();
		LPCSTR				Name;
		FunAdress			Address;
		bool				IsConstructor;

		ParamType					ReturnType;
		std::vector<ParamType>		ArgTypes;

		std::vector<VAttribute*>	Attrs;

		 virtual void Invoke( ObjBase* retValue , VReflectBase* pBindObject , std::vector<ObjBase*>& pArgs ) = 0;
		 virtual void Constructor( void** ppRetValue , std::vector<ObjBase*>& pArgs ) = 0;
	};

	struct VAssembly
	{
		struct String_Less
		{
			bool operator()( LPCSTR _Left, LPCSTR _Right) const
			{
                return strcmp( _Left , _Right )<0;
			}
		};

		typedef std::map< vIID , VClassType*,std::less<vIID>,HeapAllocator< std::pair<const vIID,VClassType*> > >	IIDMap;
		typedef std::map< LPCSTR , VClassType* ,String_Less , HeapAllocator< std::pair<const LPCSTR,VClassType*> > >	NameMap;

		IIDMap			m_IIDTypes;
		NameMap			m_NameTypes;

	public:
		 VAssembly();
		 ~VAssembly();

		 void RegClassType( LPCSTR name , const vIID& id , VClassType* pType );

		 VClassType* FindClassType( vIID id );
		 VClassType* FindClassType( LPCSTR name );

		static  VClassType*	FindClassTypeInWhole(vIID id);
		static  VClassType*	FindClassTypeInWhole(LPCSTR name);
	};

	template< int >
	struct VAssemblyProvider
	{
		
	};

	///////////////////////////////////////////////////////////////////////////////////////////////////

	template <class type>
	type ConvertObject(ObjBase* o,VClassType* pType){
		return (type)o->any_ptr;
	}
	template <> 
	inline VReflectBase* ConvertObject<VReflectBase*>(ObjBase* o,VClassType* pType){
		if( o->p->GetClassType()!=pType )
		{
			throw 1;
		}
		return o->p;
	}
	template <>
	inline float ConvertObject<float>(ObjBase* o,VClassType* pType){
		if( pType->GetSuperType()!=OT_Float )
		{
			throw 1;
		}
		return o->f;
	}
	template <>
	inline double ConvertObject<double>(ObjBase* o,VClassType* pType){
		if( pType->GetSuperType()!=OT_Double )
		{
			throw 1;
		}
		return o->d;
	}
	template <>
	inline INT8 ConvertObject<INT8>(ObjBase* o,VClassType* pType){
		if( pType->GetSuperType()!=OT_Int8 )
		{
			throw 1;
		}
		return o->i8;
	}
	template <>
	inline INT16 ConvertObject<INT16>(ObjBase* o,VClassType* pType){
		if( pType->GetSuperType()!=OT_Int16 )
		{
			throw 1;
		}
		return o->i16;
	}
	template <>
	inline INT32 ConvertObject<INT32>(ObjBase* o,VClassType* pType){
		if( pType->GetSuperType()!=OT_Int32 )
		{
			throw 1;
		}
		return o->i32;
	}
	template <>
	inline INT64 ConvertObject<INT64>(ObjBase* o,VClassType* pType){
		if( pType->GetSuperType()!=OT_Int64 )
		{
			throw 1;
		}
		return o->i64;
	}
	template <>
	inline UINT8 ConvertObject<UINT8>(ObjBase* o,VClassType* pType){
		if( pType->GetSuperType()!=OT_UInt8 )
		{
			throw 1;
		}
		return o->ui8;
	}
	template <>
	inline UINT16 ConvertObject<UINT16>(ObjBase* o,VClassType* pType){
		if( pType->GetSuperType()!=OT_UInt16 )
		{
			throw 1;
		}
		return o->ui16;
	}
	template <>
	inline UINT32 ConvertObject<UINT32>(ObjBase* o,VClassType* pType){
		if( pType->GetSuperType()!=OT_UInt32 )
		{
			throw 1;
		}
		return o->ui32;
	}
	template <>
	inline UINT64 ConvertObject<UINT64>(ObjBase* o,VClassType* pType){
		if( pType->GetSuperType()!=OT_UInt64 )
		{
			throw 1;
		}
		return o->ui64;
	}


	template<class type>
	struct ClassExporter
	{
	};

	template<>
	struct ClassExporter<tpl_utility::NullObject>
	{
		 static VClassType* ClassType(){ static VClassType Instance(OT_Void,("NullObject"),0); return &Instance;}
	};

	template<> struct ClassExporter<void>
	{
		 static VClassType* ClassType(){ static VClassType Instance(OT_Void,("void"),0); return &Instance;}
	};
	template<> struct ClassExporter<void*>
	{
		 static VClassType* ClassType(){ return ClassExporter<void>::ClassType();}
	};
	template<> struct ClassExporter<const void*>
	{
		 static VClassType* ClassType(){ return ClassExporter<void>::ClassType();}
	};

	template<class type>
	const type* ClassConverter(const VReflectBase* Ptr,type*)
	{
		if(Ptr==NULL)
			return NULL;
		if( Ptr->GetClassType()->CanConvertTo( ClassExporter<type>::ClassType() ) )
			return (const type*)(Ptr);
		return NULL;
	};

	template<class type>
	type* ClassConverter(VReflectBase* Ptr,type*)
	{
		if(Ptr==NULL)
			return NULL;
		if( Ptr->GetClassType()->CanConvertTo( ClassExporter<type>::ClassType() ) )
			return (type*)(Ptr);
		return NULL;
	};

}
#define VTypeof( cn ) Reflection::ClassExporter<cn>::ClassType()
#define VTypeCast(cn,p) Reflection::ClassConverter<cn>(p,0)
