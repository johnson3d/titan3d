// vfxtemplate.h
// 
// VictoryCore Code
// ����ģ�弯��
//
// Author : lanzhengpeng(������)
// More author :
// Create time : 2002-6-13
// Modify time :
//-----------------------------------------------------------------------------

#ifndef __VFX_TEMPLATE_H__
#define __VFX_TEMPLATE_H__

#include "vfx_temp_base.h"

#pragma once

//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
// ���ֲ���
//-----------------------------------------------------------------------------
//return: success return value between 0 and size - 1,else if below zero.
template <class TYPE,class KEY,class _Compare>
INT_PTR _vfxQuickFind(const TYPE * array,size_t size,const KEY & key,_Compare comp)
{
	INT_PTR begin = 0;
	INT_PTR end = size;
	INT_PTR mid = (begin + end) / 2;

	while(begin < end)
	{
		INT_PTR temp = comp(array[mid],key);

		if(temp == 0)
			return mid;

		if(temp > 0)
			end = mid - 1;
		else if(temp < 0)
			begin = mid + 1;
		mid = (begin + end) / 2;
	}
	if((mid < (INT)size) && (comp(array[mid],key) == 0))
		return mid;
	return -1;
}

//return: success return value between 0 and size - 1,else if below zero.
template <class TYPE,class KEY,class _Compare>
inline INT_PTR _vfxQFind(const TYPE * array,size_t size,const KEY & key,_Compare comp)
{
	return _vfxQuickFind(array,size,key,comp);
}

//return: success return value between 0 and size - 1,else if below zero.
template <typename TYPE, typename KEY>
inline INT _vfxQFind(const TYPE * array,size_t size,const KEY & key)
{
	return _vfxQuickFind(array,size,key,__vfxDefaultCompareWithKey<TYPE, KEY>());
}

//return: the value is closest position
template <typename TYPE, typename KEY,class _Compare>
INT_PTR _vfxQuickFindPos(const TYPE * array,size_t size,const KEY & key,_Compare comp)
{
	INT_PTR begin = 0;
	INT_PTR end = size;
	INT_PTR mid = (begin + end) / 2;

	while(begin < end)
	{
		INT_PTR temp = comp(array[mid],key);

		if(temp == 0)
			return mid;

		if(temp > 0)
			end = mid - 1;
		else if(temp < 0)
			begin = mid + 1;
		mid = (begin + end) / 2;
	}
	if((mid < (INT)size) && (comp(array[mid],key) < 0))
		return mid + 1;
	return mid;
}

//return: the value is closest position
template <class TYPE,class KEY,class _Compare>
inline INT_PTR _vfxQFindPos(const TYPE * array,size_t size,const KEY & key,_Compare comp)
{
	return _vfxQuickFindPos(array,size,key,comp);
}

//return: the value is closest position
template <class TYPE,class KEY>
inline INT_PTR _vfxQFindPos(const TYPE * array,size_t size,const KEY & key)
{
	return _vfxQuickFindPos(array,size,key,__vfxDefaultCompareWithKey<TYPE,KEY>());
}

//-----------------------------------------------------------------------------
// ��������
//-----------------------------------------------------------------------------
template <class TYPE, class _Compare>
inline INT_PTR _vfxPartion( TYPE * array, INT_PTR low, INT_PTR high, _Compare comp)
{
    TYPE     temp = array[ low ] ;

    while( low < high )
    {
        while( comp( temp, array[ high ] ) <= 0 && low < high )
            --high;
        array[ low ] = array[ high ] ;

        while( comp( array[ low ], temp ) <= 0 && low < high )
            ++low;
        array[ high ] = array[ low ] ;
    }

    array[ low ] = temp ;

    return low ;
}

#if !defined(PLATFORM_WIN)
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Wdeprecated-register"
#endif

template <class TYPE, class _Compare>
void _vfxQuickSort( TYPE * array, INT_PTR low, INT_PTR high, _Compare comp)
{
	//register INT_PTR     medium;
	INT_PTR     medium;

	if( low < high )
	{
		medium = _vfxPartion<TYPE,_Compare>( array, low, high, comp );
		_vfxQuickSort<TYPE,_Compare>( array, low, medium - 1, comp );
		_vfxQuickSort<TYPE,_Compare>( array, medium + 1, high, comp );
	}
}
#if !defined(PLATFORM_WIN)
#pragma clang diagnostic pop
#endif

template <typename TYPE>
inline void _vfxQSort( TYPE * array, size_t count )
{
	_vfxQuickSort( array, 0, count - 1, __vfxDefaultCompare<TYPE>() );
}

template <typename TYPE, class _Compare>
inline void _vfxQSort( TYPE * array, size_t count, _Compare comp )
{
	_vfxQuickSort( array, 0, count - 1, comp );
}

template <typename TYPE, class _Compare>
void _vfxSort( TYPE * array, size_t count, _Compare comp)
{
	for(size_t j=0;j<count;++j)
		for(size_t i=j+1;i<count;++i)
			if(comp(array[i],array[j]) < 0)
				_vfxSwap(array[i],array[j]);
}
//return: success return value between 0 and size - 1,else if below zero.
template <typename TYPE, typename KEY,class _Compare>
inline INT_PTR _vfxFind(const TYPE * array,size_t size,const KEY & key,_Compare comp)
{
	for(size_t i=0;i<size;++i)
		if(comp(array[i],key) == 0)
			return (INT)i;
	return -1;
}

template <typename TYPE, typename KEY>
inline INT_PTR _vfxFind(const TYPE * array,size_t size,const KEY & key)
{
	return _vfxFind(array,size,key,__vfxDefaultCompareWithKey<TYPE,KEY>());
}

template <class T>
inline void _vfxSwap(T & ref1,T & ref2)
{
	T t = ref1;
	ref1 = ref2,ref2 = t;
}

//-----------------------------------------------------------------------------
// ��ֹ����
//-----------------------------------------------------------------------------
template < class T>
class _VInheritLock
{
	friend T;
	private:
		_VInheritLock(){}
};

//-----------------------------------------------------------------------------
// ����
//-----------------------------------------------------------------------------
template < class T >
class _VSafeGlobal
{
	T * m_pObj;
public:
	_VSafeGlobal() 
		{	m_pObj = new T; }
	~_VSafeGlobal()
		{	delete m_pObj; }
	T * operator -> ()
		{	return m_pObj; }
	const T * operator -> () const
		{	return m_pObj; }
	T * operator & ()
		{	return m_pObj; }
	const T * operator & () const
		{	return m_pObj; }
};

//-----------------------------------------------------------------------------
// �Զ�ָ��(����)
//-----------------------------------------------------------------------------
template < class T >
class VAutoPtr
{
	T * m_pPtr;
public:
	VAutoPtr():m_pPtr(0){}
	VAutoPtr(T * __p):m_pPtr(__p){}
	~VAutoPtr(){delete m_pPtr;}

	VAutoPtr & operator = (T * __p){
		delete m_pPtr;
		m_pPtr = __p;
		return *this;
	}
	VAutoPtr & operator = (VAutoPtr<T> & rhs){
		delete m_pPtr;
		m_pPtr = rhs.m_pPtr;
		rhs.m_pPtr = 0;
		return *this;
	}

	operator T *(){return m_pPtr;}
	T * operator->(){return m_pPtr;}
	T ** operator&(){return &m_pPtr;}
	T & operator*(){return *m_pPtr;}

	void Free(){delete m_pPtr;}
};

template < class T >
class VAutoPtrArray
{
	T * m_pPtr;
public:
	VAutoPtrArray():m_pPtr(0){}
	VAutoPtrArray(T * __p):m_pPtr(__p){}
	~VAutoPtrArray(){delete [] m_pPtr;}

	VAutoPtrArray & operator = (T * __p){
		delete m_pPtr;
		m_pPtr = __p;
		return *this;
	}
	VAutoPtrArray & operator = (VAutoPtrArray<T> & rhs){
		delete m_pPtr;
		m_pPtr = rhs.m_pPtr;
		rhs.m_pPtr = 0;
		return *this;
	}

	operator T *(){return m_pPtr;}
//	T * operator->(){return m_pPtr;}
	T ** operator&(){return &m_pPtr;}
	T & operator*(){return *m_pPtr;}
	T & operator[](size_t __s){return m_pPtr[__s];}
	void Free(){delete[] m_pPtr;}
};

template < class T = HANDLE>
class VAutoHandle
{
	T  m_h;
public:
	VAutoHandle():m_h((T)0){}
	VAutoHandle(T __h):m_h(__h){}
	~VAutoHandle(){Free();}

	VAutoHandle & operator = (T __h){
		Free();
		m_h = __h;
		return *this;
	}
	VAutoHandle & operator = (VAutoHandle<T> & rhs){
		Free();
		m_h = rhs.m_h;
		rhs.m_h = 0;
		return *this;
	}

	operator T (){return m_h;}
	T * operator&(){return &m_h;}

	void Free(){if(m_h) CloseHandle(m_h);}

	friend bool operator == (const VAutoHandle<T> & lhs, T __h){
		return lhs.m_h == __h;
	}
	friend bool operator != (const VAutoHandle<T> & lhs, T __h){
		return lhs.m_h != __h;
	}
	friend bool operator == (T __h, const VAutoHandle<T> & lhs){
		return lhs.m_h == __h;
	}
	friend bool operator != (T __h, const VAutoHandle<T> & lhs){
		return lhs.m_h != __h;
	}
};


namespace VFX
{
	struct VPointerShim{
	// Since the compiler only allows at most one non-trivial
	// implicit conversion we can make use of a shim class to
	// be sure that IsPtr below doesn't accept classes with
	// implicit pointer conversion operators
		VPointerShim(const volatile void*); // no implementation
	};

	// These are the discriminating functions
	CHAR IsPtr(VPointerShim);	// no implementation is required
	INT  IsPtr(...);			// no implementation is required

	template< typename T , typename U>
	struct VIsSame
	{
		enum { RET=false };
	};

	template< typename T>
	struct VIsSame<T, T>
	{
		enum { RET = true};
	};
	
    struct VSelectThen
    {       template<class Then, class Else>
            struct Result
            {       typedef Then RET;
            };
    }; // end SelectThen

    struct VSelectElse
    {       template<class Then, class Else>
            struct Result
            {       typedef Else RET;
            };
    }; // end SelectElse

    template<bool Condition>
    struct VSelector
    {       typedef VSelectThen RET;
    }; // end Selector

    template<>
    struct VSelector<false>
    {       typedef VSelectElse RET;
    }; // end Selector<false>

	const INT DEFAULT = -32767;

	const INT NilValue = -32768;

	struct NilCase
	{   enum {tag = NilValue};
		typedef NilCase RET;
	}; // NilCase
}

template <class T>
struct VISPTR{
    // This template meta function takes a type T
    // and returns true exactly when T is a pointer.
    // One can imagine meta-functions discriminating on
    // other criteria.

    enum { RET = (sizeof(VFX::IsPtr(*(T*)0)) == 1) };
}; //ISPTR

template< typename T, typename U>
struct VIS_SAME
{
	enum { RET = false };
};

template< typename T>
struct VIS_SAME<T, T>
{
	enum { RET = true };
};

template<bool Condition, typename Then, typename Else>
struct VIF{
	typedef Then	 RET;
}; // IF
template<class Then, class Else>
struct VIF<false,Then,Else>{
	typedef Else	 RET;
}; // IF

template <INT Tag,class Statement,class Next = VFX::NilCase>
struct VCASE
{   enum {tag = Tag};
    typedef Statement statement;
    typedef Next next;
}; // CASE

template <INT Tag,class aCase>
struct VSWITCH
{      
	typedef typename aCase::next nextCase;
	enum {
		tag = aCase::tag,			// VC++ 5.0 doesn't operate directly on aCase::value in IF<>
		nextTag = nextCase::tag,	// Thus we need a little cheat
		found = (tag == Tag || tag == VFX::DEFAULT)
	};
	typedef typename VIF<(nextTag == VFX::NilValue),
				VFX::NilCase,
				VSWITCH<Tag,nextCase> >
			::RET nextSwitch;
	typedef typename VIF<(found != 0),
				typename aCase::statement,
				typename nextSwitch::RET>
			::RET RET;
}; // SWITCH

template<class T1,class T2>
struct VBindInherit : public T1, public T2
{
};


class VMemoryReader
{
public:
	enum SeekPosition { begin = 0x0, current = 0x1, end = 0x2 };

	VMemoryReader()
		:m_pMemory(NULL),m_nSize(0),m_nPosition(0)
	{
		mDeepCopy = FALSE;
	}
	void SetSource( const void * pStart,size_t nSize )
	{
		m_pMemory = (LPCSTR)pStart;
		m_nSize=(int)nSize;
		m_nPosition = 0;
	}
	VMemoryReader(const void * pStart,size_t nSize, vBOOL deepCopy = TRUE)
		:m_pMemory((LPCSTR)pStart),m_nSize((int)nSize),m_nPosition(0)
	{
		mDeepCopy = deepCopy;
		if (mDeepCopy)
		{
			m_pMemory = (LPCSTR)malloc(nSize+1);
			memcpy((void*)m_pMemory, pStart, nSize);
		}
	}
	~VMemoryReader()
	{
		if (mDeepCopy)
		{
			if (m_pMemory)
			{
				free((void*)m_pMemory);
				m_pMemory = NULL;
			}
		}
	}
	
	template<class TYPE>
	UINT_PTR Read( TYPE& data )
	{
		return Read( &data , sizeof(data) );
	}

	UINT_PTR Read(void * pCopy, UINT_PTR uSize)
	{
		if(m_nPosition + uSize > m_nSize)
			uSize = m_nSize - m_nPosition;
		if(uSize > 0)
			memcpy(pCopy,m_pMemory + m_nPosition,uSize);
		m_nPosition += uSize;
		return uSize;
	}
	UINT_PTR Seek(INT_PTR lOff,size_t uFrom)
	{
		//For Speed
		switch(uFrom)
		{
		case begin:
			m_nPosition = lOff;//m_nPosition = min(m_nSize,lOff);
			break;
		case current:
			m_nPosition += lOff;//m_nPosition += (lOff < 0) ? (min(m_nPosition,-lOff)) : (min(m_nSize - m_nPosition,lOff));
			break;
		case end:
			m_nPosition = m_nSize - lOff;//m_nPosition = m_nSize - min(m_nSize,lOff);
			break;
		}
		if(m_nPosition < 0) m_nPosition = 0;
		if(m_nPosition > (LONG)m_nSize) m_nPosition = (LONG)m_nSize;
		return m_nPosition;
	}
	UINT_PTR GetPosition()
	{
		return m_nPosition;
	}
	UINT_PTR GetLength()
	{
		return m_nSize;
	}
	LPCSTR GetData()
	{
		return m_pMemory;
	}
private:
	vBOOL		mDeepCopy;
	LPCSTR		m_pMemory;
	UINT_PTR	m_nSize;
	INT_PTR		m_nPosition;
};

#endif	//end __VFX_TEMPLATE_H__
