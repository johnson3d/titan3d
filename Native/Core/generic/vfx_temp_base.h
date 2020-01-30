// vfx_temp_base.h
//
// VictoryCore Code
// ����ģ���������
//
// Author : johnson
// Modifer :	
// Create Timer :	2003-7-25 20:47
// Modify Timer :	
//-----------------------------------------------------------------------------
#ifndef __vfx_temp_base_h__2003_7_25_20_47
#define __vfx_temp_base_h__2003_7_25_20_47

#pragma once

#if defined(PLATFORM_WIN)
#pragma warning(push)
#pragma warning(disable:4291)
#endif

template <class TYPE>
struct __vfxDefaultCompare
{
	//return value:
	//t1 < t2 ? -1
	//t1 == t2 ? 0
	//t1 > t2 ? 1
	inline INT_PTR operator () (const TYPE & t1, const TYPE & t2)
	{	return (t1 < t2) ? -1 : ((t2 < t1) ? 1 : 0); }
};
template<> struct __vfxDefaultCompare<INT8>
{
	inline INT_PTR operator () (const INT8 & t1, const INT8 & t2)
	{	return t1 - t2;}
};
template<> struct __vfxDefaultCompare<INT16>
{
	inline INT_PTR operator () (const INT16 & t1, const INT16 & t2)
	{	return t1 - t2;}
};
template<> struct __vfxDefaultCompare<INT32>
{
	inline INT_PTR operator () (const INT32 & t1, const INT32 & t2)
	{	return t1 - t2;}
};
template<> struct __vfxDefaultCompare<INT64>
{
	inline INT_PTR operator () (const INT64 & t1, const INT64 & t2)
	{	
		if(sizeof(INT_PTR) == 8)
			return INT(t1 - t2);
		else
			return (t1 < t2) ? -1 : ((t2 < t1) ? 1 : 0);
	}
};
template<> struct __vfxDefaultCompare<UINT8>
{
	inline INT_PTR operator () (const UINT8 & t1, const UINT8 & t2)
	{	return (INT_PTR)t1 - (INT_PTR)t2;}
};
template<> struct __vfxDefaultCompare<UINT16>
{
	inline INT_PTR operator () (const UINT16 & t1, const UINT16 & t2)
	{	return (INT_PTR)t1 - (INT_PTR)t2;}
};
template<> struct __vfxDefaultCompare<UINT32>
{
	inline INT_PTR operator () (const UINT32 & t1, const UINT32 & t2)
	{	return (INT_PTR)(t1 - t2);}
};
template<> struct __vfxDefaultCompare<UINT64>
{
	inline INT_PTR operator () (const UINT64 & t1, const UINT64 & t2)
	{	
		if(sizeof(INT_PTR) == 8)
			return (INT_PTR)(t1 - t2);
		else
			return (t1 < t2) ? -1 : ((t2 < t1) ? 1 : 0);
	}
};
template<> struct __vfxDefaultCompare<ULONG>
{
	inline INT_PTR operator () (const ULONG & t1, const ULONG & t2)
	{	return (INT_PTR)(t1 - t2);}
};

template <class TYPE,class KEY>
struct __vfxDefaultCompareWithKey
{
	//return value:
	//type < key ? -1
	//type == key ? 0
	//type > key ? 1
	inline INT_PTR operator () (const TYPE & type, const KEY & key)
	{	return (type < key) ? -1 : ((key < type) ? 1 : 0); }
};

template <class TYPE>
struct __vfxDefaultEqual
{
	inline INT_PTR operator () (const TYPE & t1, const TYPE & t2)
	{	return __vfxDefaultCompare<TYPE>()(t1,t2) == 0; }
};

template<class Type1, class Type2>
struct VPair 
{
	typedef Type1 first_type;
	typedef Type2 second_type;

	Type1 first;
	Type2 second;

	VPair( ){}
	VPair(const Type1& __Val1,const Type2& __Val2)
		:first(__Val1),second(__Val2)
		{	}
	//template<class Other1, class Other2>
	VPair(const VPair<Type1, Type2>& _Right)
		:first(_Right.first),second(_Right.second)
		{	}
};

template<class __Key>
class VNewWrapper
{
public:
	__Key	__t;
	
	VNewWrapper(){}
	VNewWrapper(const __Key & key):__t(key){}

	template<class _Ty>
	void * operator new(size_t, _Ty* p){	
		return p;
	}
};

//-----------------------------------------------------------------------------


namespace VFX{
	template<class TYPE>
		inline void __ConstructElements(TYPE* pElements)
	{
		new(pElements) VNewWrapper<TYPE>;
	}
	template<class TYPE>
		inline void __ConstructElements(TYPE* pElements, INT_PTR nElements)
	{
		for( ; nElements--; pElements++ )
		{
			new(pElements) VNewWrapper<TYPE>;
		}
	}
	template<class TYPE>
		inline void __ConstructElementsEx(TYPE* pElements, INT_PTR nElements)
	{
		memset(pElements,0,sizeof(TYPE)*nElements);
		for( ; nElements--; pElements++ )
		{
			new(pElements) VNewWrapper<TYPE>;
		}
	}
	template<class TYPE>
		inline void __ConstructElements(TYPE* pElements, INT_PTR nElements,const TYPE & ref)
	{
		for( ; nElements--; pElements++ )
		{
			new(pElements) VNewWrapper<TYPE>(ref);
		}
	}
	template<class TYPE>
		inline void __ConstructElementsEx(TYPE* pElements, INT_PTR nElements,const TYPE & ref)
	{
		memset(pElements,0,sizeof(TYPE)*nElements);
		__ConstructElements(pElements,nElements,ref);
	}
	
	template<class TYPE>
		inline void __DestructElements(TYPE* pElements)
	{
		pElements->~TYPE();
	}
	template<class TYPE>
		inline void __DestructElements(TYPE* pElements, INT_PTR nCount)
	{
		// call the destructor(s)
		for (; nCount--; pElements++)
			pElements->~TYPE();
	}
	
	template<class TYPE>
		inline void __CopyElements(TYPE* pDest, const TYPE* pSrc, INT_PTR nCount)
	{
		// default is element-copy using assignment
		while(nCount--)
			*pDest++ = *pSrc++;
	}
	
	template<class TYPE, class ARG_TYPE>
		inline vBOOL __CompareElements(const TYPE* pElement1, const ARG_TYPE* pElement2)
	{
		return *pElement1 == *pElement2;
	}
}

//-----------------------------------------------------------------------------
// ������ѧ����
//-----------------------------------------------------------------------------

template<class U,class V> inline U vfxMIN(const U & lh, const V & rh)
{ return ((lh < rh) ? lh : U(rh)); }
template<class U,class V> inline U vfxMAX(const U & lh, const V & rh)
{ return ((lh > rh) ? lh : U(rh)); }
template<class T> inline T vfxMOD (const T & i, const T & j)
{ return (i<0)?j-(-i%j):i%j; }
template<class T> inline T vfxDIV (const T & i, const T & j)
{ return (i<0)?-(-i/j)-1:i/j; }
template<class T> inline T vfxABS (const T & i)
{ return (i>0)?i:-i; }

template<class T>
struct _max
{
	T _M_t;
	template<class U,class V>
	_max(const U & lh,const V & rh)
	{
		_M_t = lh > rh ? T(lh) : T(rh);
	}
	inline operator T () const{
		return _M_t;
	}
};
template<class T>
struct _min
{
	T _M_t;
	template<class U,class V>
	_min(const U & lh,const V & rh)
	{
		_M_t = lh < rh ? T(lh) : T(rh);
	}
	inline operator T () const{
		return _M_t;
	}
};

template<typename T> inline T vfxLimit(const T& v , const T& minv , const T& maxv){
	if( v<=minv ){
		return minv;
	}
	else if( v>=maxv ){
		return maxv;
	}
	else{
		return v;
	}
}
template<typename T> inline bool vfxEq(const T& v1 , const T& v2 , const T& Epsilon){
	return (vfxABS(v1-v2) <= Epsilon);
}
template<typename T> inline bool vfxGE(const T& v1 , const T& v2 , const T& Epsilon){
	return ( v1-v2 >= Epsilon);
}
template<typename T> inline bool vfxLE(const T& v1 , const T& v2 , const T& Epsilon){
	return ( v2-v1 >= Epsilon);
}
template<typename T> inline T vfxPow2(const T& v ){
	return v*v;
}
template<typename T> inline T vfxPow(const T& v , int n){
	T tmp = v;
	for( int i=1;i<n;i++ ){
		tmp*=v;
	}
	return tmp;
}

//-----------------------------------------------------------------------------
// Miscellaneous helper functions
//-----------------------------------------------------------------------------

#if !defined(PLATFORM_WIN)
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Wconversion-null"
#endif

#define VFX_NO_EQUALOP( Class ) Class( const Class& src );\
	const Class& operator =( const Class& src );

#endif //end __vfx_temp_base_h__2003_7_25_20_47
