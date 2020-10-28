// vfxarray.h
// 
// VictoryCore Code
//
// Author : johnson
// More author :
// Create time : 2003-2-5   11:09
// Modify time :
//-----------------------------------------------------------------------------
#ifndef __vfxarray_h__2003_11_09
#define __vfxarray_h__2003_11_09

#include "vfxtemplate.h"
#include "vfx_temp_base.h"

#pragma once

#if defined(PLATFORM_WIN)
	#pragma warning(push)
	#pragma warning(disable:4786)
	#pragma warning(disable:4114)
	#pragma warning(disable:4291)
#else
	#pragma clang diagnostic push
	#pragma clang diagnostic ignored "-Wundefined-bool-conversion"
#endif

//-----------------------------------------------------------------------------

#define ArraySizeType	INT

template<class TYPE, class ARG_TYPE>
class VArray
{
public:
// Construction
	VArray();

	inline ArraySizeType GetMaxSize() const{
		return m_nMaxSize;
	}
	void InstantArray( ArraySizeType nMax , ArraySizeType nGrowBy = -1, vBOOL bFreeWhenEmpty = TRUE ){
		SetSize( nMax , nGrowBy, bFreeWhenEmpty);
		//		m_nSize = 0;
	}
// Attributes
	ArraySizeType GetSize() const;
	ArraySizeType	GetByteSize() const	{
		return GetSize() * sizeof(TYPE);
	}
	ArraySizeType GetUpperBound() const;
	void SetSize(ArraySizeType nNewSize, ArraySizeType nGrowBy = -1, vBOOL bFreeWhenEmpty = TRUE);

// Operations
	// Clean up
	void FreeExtra();
	void RemoveAll(vBOOL bFree = TRUE);

	// Accessing elements
	const TYPE & GetAt(ArraySizeType nIndex) const;
	void SetAt(ArraySizeType nIndex, ARG_TYPE newElement);
	TYPE& ElementAt(ArraySizeType nIndex);

	// Direct Access to the element data (may return NULL)
	const TYPE* GetData() const;
	TYPE* GetData();

	// Potentially growing the array
	void SetAtGrow(ArraySizeType nIndex, ARG_TYPE newElement);
	ArraySizeType Add(ARG_TYPE newElement);
	ArraySizeType Append(const VArray& src);
	ArraySizeType Append(TYPE* ptr, ArraySizeType num);
	void Copy(const VArray& src);

	// overloaded operator helpers
	const TYPE & operator[](ArraySizeType nIndex) const;
	TYPE& operator[](ArraySizeType nIndex);

	// Operations that move elements around
	void InsertAt(ArraySizeType nIndex, ARG_TYPE newElement, ArraySizeType nCount = 1);
	void RemoveAt(ArraySizeType nIndex, ArraySizeType nCount = 1);
	void InsertAt(ArraySizeType nStartIndex, VArray* pNewArray);

	// Find the index of one element (added by Jones)
	// NOTE: If this array was sorted, use _vfxQFind() function instead.
	int  IndexOf(const TYPE& value) { return ( m_nSize > 0 ) ? IndexOf( value, 0, m_nSize ) : -1; }
	int  IndexOf(const TYPE& value, ArraySizeType iStart) { return IndexOf( value, iStart, m_nSize - iStart ); }
	int  IndexOf(const TYPE& value, ArraySizeType nIndex, ArraySizeType nNumElements);

// Implementation
protected:
	TYPE* m_pData;   // the actual array of data
	ArraySizeType m_nSize;     // # of elements (upperBound - 1)
	ArraySizeType m_nMaxSize;  // max allocated
	ArraySizeType m_nGrowBy;   // grow amount

public:
	~VArray();
};

/////////////////////////////////////////////////////////////////////////////
// VArray<TYPE, ARG_TYPE> inline functions

template<class TYPE, class ARG_TYPE>
inline ArraySizeType VArray<TYPE, ARG_TYPE>::GetSize() const
	{ return m_nSize; }
template<class TYPE, class ARG_TYPE>
inline ArraySizeType VArray<TYPE, ARG_TYPE>::GetUpperBound() const
	{ return m_nSize-1; }
template<class TYPE, class ARG_TYPE>
inline void VArray<TYPE, ARG_TYPE>::RemoveAll(vBOOL bFree)
	{ SetSize(0, -1, bFree); }
template<class TYPE, class ARG_TYPE>
inline const TYPE & VArray<TYPE, ARG_TYPE>::GetAt(ArraySizeType nIndex) const
	{ ASSERT(nIndex >= 0 && nIndex < m_nSize);
		return m_pData[nIndex]; }
template<class TYPE, class ARG_TYPE>
inline void VArray<TYPE, ARG_TYPE>::SetAt(ArraySizeType nIndex, ARG_TYPE newElement)
	{ ASSERT(nIndex >= 0 && nIndex < m_nSize);
		m_pData[nIndex] = newElement; }
template<class TYPE, class ARG_TYPE>
inline TYPE& VArray<TYPE, ARG_TYPE>::ElementAt(ArraySizeType nIndex){ 
	if (nIndex >= 0 && nIndex < m_nSize)
	{
		return m_pData[nIndex];
	}
	else
	{
		ASSERT(false);
		return m_pData[0];
	}
}
template<class TYPE, class ARG_TYPE>
inline const TYPE* VArray<TYPE, ARG_TYPE>::GetData() const
	{ return (const TYPE*)m_pData; }
template<class TYPE, class ARG_TYPE>
inline TYPE* VArray<TYPE, ARG_TYPE>::GetData()
	{ return (TYPE*)m_pData; }
template<class TYPE, class ARG_TYPE>
inline ArraySizeType VArray<TYPE, ARG_TYPE>::Add(ARG_TYPE newElement)
	{ ArraySizeType nIndex = m_nSize;
		SetAtGrow(nIndex, newElement);
		return nIndex; }
template<class TYPE, class ARG_TYPE>
inline const TYPE & VArray<TYPE, ARG_TYPE>::operator[](ArraySizeType nIndex) const
	{ return GetAt(nIndex); }
template<class TYPE, class ARG_TYPE>
inline TYPE& VArray<TYPE, ARG_TYPE>::operator[](ArraySizeType nIndex)
	{ return ElementAt(nIndex); }

/////////////////////////////////////////////////////////////////////////////
// VArray<TYPE, ARG_TYPE> out-of-line functions

template<class TYPE, class ARG_TYPE>
VArray<TYPE, ARG_TYPE>::VArray()
{
	m_pData = NULL;
	m_nSize = m_nMaxSize = m_nGrowBy = 0;
}

template<class TYPE, class ARG_TYPE>
VArray<TYPE, ARG_TYPE>::~VArray()
{
	if (m_pData != NULL)
	{
		VFX::__DestructElements<TYPE>(m_pData, m_nSize);
		delete[] (BYTE*)m_pData;
	}
}

template<class TYPE, class ARG_TYPE>
void VArray<TYPE, ARG_TYPE>::SetSize(ArraySizeType nNewSize, ArraySizeType nGrowBy, vBOOL bFreeWhenEmpty)
{
	ASSERT(nNewSize >= 0);

	if (nGrowBy != -1)
		m_nGrowBy = nGrowBy;  // set new size

	if (nNewSize == 0 && bFreeWhenEmpty)
	{
		// shrink to nothing
		if (m_pData != NULL)
		{
			VFX::__DestructElements<TYPE>(m_pData, m_nSize);
			delete[] (BYTE*)m_pData;
			m_pData = NULL;
		}
		m_nSize = m_nMaxSize = 0;
	}
	else if (m_pData == NULL)
	{
		// create one with exact size
#ifdef SIZE_T_MAX
		ASSERT(nNewSize <= SIZE_T_MAX/sizeof(TYPE));    // no overflow
#endif
		m_pData = (TYPE*) new(__FILE__, __LINE__) BYTE[nNewSize * sizeof(TYPE)];
		VFX::__ConstructElementsEx<TYPE>(m_pData, nNewSize);
		m_nSize = m_nMaxSize = nNewSize;
	}
	else if (nNewSize <= m_nMaxSize)
	{
		// it fits
		if (nNewSize > m_nSize)
		{
			// initialize the new elements
			VFX::__ConstructElementsEx<TYPE>(&m_pData[m_nSize], nNewSize-m_nSize);
		}
		else if (m_nSize > nNewSize)
		{
			// destroy the old elements
			VFX::__DestructElements<TYPE>(&m_pData[nNewSize], m_nSize-nNewSize);
		}
		m_nSize = nNewSize;
	}
	else
	{
		// otherwise, grow array
		nGrowBy = m_nGrowBy;
		if (nGrowBy == 0)
		{
			// heuristically determine growth when nGrowBy == 0
			//  (this avoids heap fragmentation in many situations)
			nGrowBy = m_nSize / 8;
			nGrowBy = (nGrowBy < 4) ? 4 : ((nGrowBy > 1024) ? 1024 : nGrowBy);
		}
		ArraySizeType nNewMax;
		if (nNewSize < m_nMaxSize + nGrowBy)
			nNewMax = m_nMaxSize + nGrowBy;  // granularity
		else
			nNewMax = nNewSize;  // no slush

		ASSERT(nNewMax >= m_nMaxSize);  // no wrap around
#ifdef SIZE_T_MAX
		ASSERT(nNewMax <= SIZE_T_MAX/sizeof(TYPE)); // no overflow
#endif
		TYPE* pNewData = (TYPE*) new(__FILE__, __LINE__) BYTE[nNewMax * sizeof(TYPE)];

		// copy new data from old
		memcpy(pNewData, m_pData, m_nSize * sizeof(TYPE));

		// construct remaining elements
		ASSERT(nNewSize > m_nSize);
		VFX::__ConstructElementsEx<TYPE>(&pNewData[m_nSize], nNewSize-m_nSize);

		// get rid of old stuff (note: no destructors called)
		delete[] (BYTE*)m_pData;
		m_pData = pNewData;
		m_nSize = nNewSize;
		m_nMaxSize = nNewMax;
	}
}

template<class TYPE, class ARG_TYPE>
ArraySizeType VArray<TYPE, ARG_TYPE>::Append(const VArray& src)
{
	ASSERT(this);
	ASSERT(this != &src);   // cannot append to itself

	ArraySizeType nOldSize = m_nSize;
	SetSize(m_nSize + src.m_nSize);
	VFX::__CopyElements<TYPE>(m_pData + nOldSize, src.m_pData, src.m_nSize);
	return nOldSize;
}

template<class TYPE, class ARG_TYPE>
ArraySizeType VArray<TYPE, ARG_TYPE>::Append(TYPE* ptr, ArraySizeType num)
{
	ASSERT(this);

	ArraySizeType nOldSize = m_nSize;
	SetSize(m_nSize + num);
	VFX::__CopyElements<TYPE>(m_pData + nOldSize, ptr, num);
	return nOldSize;
}

template<class TYPE, class ARG_TYPE>
void VArray<TYPE, ARG_TYPE>::Copy(const VArray& src)
{
	ASSERT(this != &src);   // cannot append to itself

	SetSize(src.m_nSize);
	VFX::__CopyElements<TYPE>(m_pData, src.m_pData, src.m_nSize);
}

template<class TYPE, class ARG_TYPE>
void VArray<TYPE, ARG_TYPE>::FreeExtra()
{
	if (m_nSize != m_nMaxSize)
	{
		// shrink to desired size
#ifdef SIZE_T_MAX
		ASSERT(m_nSize <= SIZE_T_MAX/sizeof(TYPE)); // no overflow
#endif
		TYPE* pNewData = NULL;
		if (m_nSize != 0)
		{
			pNewData = (TYPE*) new(__FILE__, __LINE__) BYTE[m_nSize * sizeof(TYPE)];
			// copy new data from old
			memcpy(pNewData, m_pData, m_nSize * sizeof(TYPE));
		}

		// get rid of old stuff (note: no destructors called)
		delete[] (BYTE*)m_pData;
		m_pData = pNewData;
		m_nMaxSize = m_nSize;
	}
}

template<class TYPE, class ARG_TYPE>
void VArray<TYPE, ARG_TYPE>::SetAtGrow(ArraySizeType nIndex, ARG_TYPE newElement)
{
	ASSERT(nIndex >= 0);

	if (nIndex >= m_nSize)
		SetSize(nIndex+1, -1);
	m_pData[nIndex] = newElement;
}

template<class TYPE, class ARG_TYPE>
void VArray<TYPE, ARG_TYPE>::InsertAt(ArraySizeType nIndex, ARG_TYPE newElement, ArraySizeType nCount /*=1*/)
{
	ASSERT(nIndex >= 0);    // will expand to meet need
	ASSERT(nCount > 0);     // zero or negative size not allowed

	if (nIndex >= m_nSize)
	{
		// adding after the end of the array
		SetSize(nIndex + nCount, -1);   // grow so nIndex is valid
	}
	else
	{
		// inserting in the middle of the array
		ArraySizeType nOldSize = m_nSize;
		SetSize(m_nSize + nCount, -1);  // grow it to new size
		// destroy intial data before copying over it
		VFX::__DestructElements<TYPE>(&m_pData[nOldSize], nCount);
		// shift old data up to fill gap
		memmove(&m_pData[nIndex+nCount], &m_pData[nIndex],
			(nOldSize-nIndex) * sizeof(TYPE));

		// re-init slots we copied from
		VFX::__ConstructElementsEx<TYPE>(&m_pData[nIndex], nCount);
	}

	// insert new value in the gap
	ASSERT(nIndex + nCount <= m_nSize);
	while (nCount--)
		m_pData[nIndex++] = newElement;
}

template<class TYPE, class ARG_TYPE>
void VArray<TYPE, ARG_TYPE>::RemoveAt(ArraySizeType nIndex, ArraySizeType nCount)
{
	ASSERT(nIndex >= 0);
	ASSERT(nCount >= 0);
	ASSERT(nIndex + nCount <= m_nSize);

	// just remove a range
	ArraySizeType nMoveCount = m_nSize - (nIndex + nCount);
	VFX::__DestructElements<TYPE>(&m_pData[nIndex], nCount);
	if (nMoveCount)
		memmove(&m_pData[nIndex], &m_pData[nIndex + nCount],
			nMoveCount * sizeof(TYPE));
	m_nSize -= nCount;
}

template<class TYPE, class ARG_TYPE>
void VArray<TYPE, ARG_TYPE>::InsertAt(ArraySizeType nStartIndex, VArray* pNewArray)
{
	ASSERT(pNewArray != NULL);
	ASSERT(nStartIndex >= 0);

	if (pNewArray->GetSize() > 0)
	{
		InsertAt(nStartIndex, pNewArray->GetAt(0), pNewArray->GetSize());
		for (ArraySizeType i = 0; i < pNewArray->GetSize(); i++)
			SetAt(nStartIndex + i, pNewArray->GetAt(i));
	}
}

template<typename TYPE, class ARG_TYPE>
int VArray<TYPE, ARG_TYPE>::IndexOf( const TYPE& value, ArraySizeType iStart, ArraySizeType nNumElements )
{
	// Validate arguments
	if( iStart < 0 || iStart >= m_nSize || nNumElements < 0 || 
		iStart + nNumElements > m_nSize )
	{
		ASSERT( false );
		return -1;
	}
	// Search
	for( ArraySizeType i = iStart; i < (iStart + nNumElements); i++ )
	{
		if( value == m_pData[i] )
			return (int)i;
	}
	// Not found
	return -1;
}

#if defined(PLATFORM_WIN)
#pragma warning(default:4291)
#pragma warning(default:4786)
#pragma warning(default:4114)
#pragma warning(pop)
#endif

#endif //__vfxarray_h__2003_11_09
