// vfxSimpleMap.h
//
// VictoryCore Code
// class VSimpleMap
//
// Author : lanzhengpeng(������)
// Modifer :	
// Create Timer :	23:6:2002   14:34
// Modify Timer :	
// 
//-----------------------------------------------------------------------------


#pragma warning(disable : 4291)

// intended for small number of simple types or pointers
template <class TKey, class TVal, class TCOMP = __vfxDefaultCompare<TKey> >
class VSimpleMap
{
public:
	typedef TKey							key_type;
	typedef TVal							value_type;
	typedef typename VPair<const TKey,TVal>			mapped_type;
	typedef typename VPair<const TKey,TVal> *		iterator;
	typedef const typename VPair<const TKey,TVal> *	const_iterator;
	typedef INT_PTR							size_type;
	typedef TCOMP							key_compare;
	//typedef reverse_iterator<iterator>	reverse_iterator
	
	mapped_type *			m_pArray;
	INT_PTR					m_nSize;

// Implementation
	template<class __Key,class __Val>
	class Wrapper
	{
	public:
		VPair<__Key,__Val>	__t;

		Wrapper(const __Key & _key,const __Val & _val)
			:__t(_key,_val)
			{	}
		template <class _Ty>
		void *operator new(size_t, _Ty* p)
			{	return p;}
	};
	struct __Compare
	{
		inline INT_PTR operator () (const mapped_type & t1,const key_type & t2)
		{	return key_compare()(t1.first,t2);}
	};

// Construction/destruction
	VSimpleMap()
		:m_pArray(0),m_nSize(0)
		{	}
	~VSimpleMap()
		{	RemoveAll();}

// Operations
	inline INT_PTR GetSize() const
	{	
		return m_nSize;	
	}
	vBOOL Add(const key_type & key, const value_type & val)
	{
		INT_PTR nIndex = FindKey(key);
		if(nIndex >= 0)
		{
			m_pArray[nIndex].second = val;
		}
		else
		{
			mapped_type * array = (mapped_type *)realloc(m_pArray,(m_nSize + 1) * sizeof(mapped_type));
			if(array == NULL)
				return FALSE;
			m_pArray = array;
			nIndex = _vfxQFindPos(m_pArray,m_nSize,key,__Compare());
			if(nIndex != m_nSize)
				memmove((void *)&m_pArray[nIndex + 1],(void *)&m_pArray[nIndex],(m_nSize - nIndex) * sizeof(mapped_type));

			new(&m_pArray[nIndex]) Wrapper<key_type,value_type>(key,val);
			m_nSize++;
		}
		return TRUE;
	}
	vBOOL Remove(const key_type & key)
	{
		INT_PTR nIndex = FindKey(key);
		if(nIndex == -1)
			return FALSE;
		if(nIndex != (m_nSize - 1))
		{
			m_pArray[nIndex].~VPair();
			memmove((void*)&m_pArray[nIndex], (void*)&m_pArray[nIndex + 1], (m_nSize - (nIndex + 1)) * sizeof(mapped_type));
		}

		mapped_type * array = (mapped_type *)realloc(m_pArray,(m_nSize - 1) * sizeof(mapped_type));
		if(array != NULL || m_nSize == 1)
			m_pArray = array;
		m_nSize--;
		
		return TRUE;
	}
	void RemoveAll()
	{
		if(m_pArray != NULL)
		{
			for(INT_PTR i = 0; i < m_nSize; i++)
			{
				m_pArray[i].~VPair();
			}
			free(m_pArray);
			m_pArray = NULL;
		}
		m_nSize = 0;
	}
	vBOOL SetAt(const key_type & key, const value_type & val)
	{
		INT_PTR nIndex = FindKey(key);
		if(nIndex == -1)
			return FALSE;

		m_pArray[nIndex].~VPair();
		SetAtIndex(nIndex,key,val);
		return TRUE;
	}
	value_type Lookup(const key_type & key)
	{
		INT_PTR nIndex = FindKey(key);
		if(nIndex < 0)
			return NULL;    // must be able to convert
		return GetValueAt(nIndex);
	}
	key_type ReverseLookup(const value_type & val)
	{
		INT_PTR nIndex = FindVal(val);
		if(nIndex < 0)
			return NULL;    // must be able to convert
		return GetKeyAt(nIndex);
	}
	const key_type & GetKeyAt(INT_PTR nIndex) const
	{
		VFX_ASSERT(nIndex >= 0 && nIndex < m_nSize);
		return m_pArray[nIndex].first;
	}
	const value_type & GetValueAt(INT_PTR nIndex) const
	{
		VFX_ASSERT(nIndex >= 0 && nIndex < m_nSize);
		return m_pArray[nIndex].second;
	}

	size_type FindKey(const key_type & key) const
	{
		return _vfxQFind(m_pArray,m_nSize,key,__Compare());
	}
	size_type FindVal(const value_type & val) const
	{
		for(INT_PTR i = 0; i < m_nSize; i++)
		{
			if(m_pArray[i].second == val)
				return i;
		}
		return -1;  // not found
	}

public:
	//Returns an iterator addressing the first element in the map. 
	iterator begin()
		{	return m_pArray;}
	const_iterator begin() const
		{	return m_pArray;}
	//Returns an iterator that addresses the location succeeding the last element in a map. 
	iterator end()
		{	return m_pArray + m_nSize;}
	const_iterator end() const
		{	return m_pArray + m_nSize;}
	//Erases all the elements of a map.
	void clear()
	{
		if(m_pArray != NULL)
		{
			for(INT_PTR i = 0; i < m_nSize; i++)
			{
				m_pArray[i].~VPair();
			}
			free(m_pArray);
			m_pArray = NULL;
		}
		m_nSize = 0;
	}
	//Returns the number of elements in a map whose key matches a parameter-specified key. 
	size_type count(const key_type& _Key) const
	{	
		for(INT_PTR i = 0; i < m_nSize; i++)
		{
			if(key_compare()(m_pArray[i].first,_Key) == 0)
				return 1;
		}
		return 0;  // not found
	}
	//Tests if a map is empty. 
	bool empty( ) const
		{	return m_nSize == 0;}
	//Returns an iterator that addresses the location succeeding the last element in a map. 
	VPair<const_iterator,const_iterator> equal_range(const key_type& _Key) const; //@_@
	VPair<iterator,iterator> equal_range(const key_type& _Key); //@_@
	//Removes an element or a range of elements in a map from specified positions 
	iterator erase(iterator _First,iterator _Last)
	{
		if(size() == 0)
			return end();

		VFX_ASSERT(_First <= _Last);
		VFX_ASSERT(_First >= m_pArray && _First < m_pArray + m_nSize);
		VFX_ASSERT(_Last >= m_pArray && _Last <= m_pArray + m_nSize);

		size_type _Pos = _First - m_pArray;

		if(_First < _Last)
		{
			for(iterator _Iter = _First;_Iter < _Last;++_Iter)
				_Iter->~VPair();
			if(_Last < end())
				memmove((void *)_First,(void *)_Last,(m_nSize - (_Last - m_pArray)) * sizeof(mapped_type));

			m_nSize -= (_Last - _First);
			mapped_type * array = (mapped_type *)realloc(m_pArray,m_nSize * sizeof(mapped_type));
			if(array != NULL || m_nSize == 0)
				m_pArray = array;
		}

		return m_pArray + _Pos;
	}
	iterator erase(iterator _Where)
	{
		return erase(_Where,_Where + 1);
	}
	size_type erase(const key_type& _Key)
	{
		iterator _Iter = find(_Key);
		if(_Iter != end())
			erase(_Iter);

		return size();
	}
	//Returns an iterator addressing the location of an element in a map that has a key equivalent to a specified key. 
	iterator find(const key_type & _Key)
	{
		INT_PTR _I = _vfxQFind(m_pArray,m_nSize,_Key,__Compare());
		if(_I < 0)
			return end();
		return m_pArray + _I;
	}
	const_iterator find(const key_type & _Key) const 
	{
		INT_PTR _I = _vfxQFind(m_pArray,m_nSize,_Key,__Compare());
		if(_I < 0)
			return (const_iterator)end();
		return (const_iterator)m_pArray + _I;
	}
	iterator find_value(const value_type & _Val)
	{
		for(INT_PTR i = 0; i < m_nSize; i++)
		{
			if(m_pArray[i].second == _Val)
				return m_pArray + i;
		}
		return end();  // not found
	}
	const_iterator find_value(const value_type & _Val) const
	{
		for(INT_PTR i = 0; i < m_nSize; i++)
		{
			if(m_pArray[i].second == _Val)
				return (const_iterator)m_pArray + i;
		}
		return (const_iterator)end();  // not found
	}
	//Returns a copy of the allocator object used to construct the map. 
	//get_allocator() *_^
	//Inserts an element or a range of elements into the map at a specified position.
	iterator insert(const key_type & _Key,const value_type& _Val = value_type())
	{
		iterator _Iter = find(_Key);
		if(_Iter != end())
		{
			_Iter->second = _Val;
		}
		else
		{
			mapped_type * array = (mapped_type *)realloc(m_pArray,(m_nSize + 1) * sizeof(mapped_type));
			if(array == NULL)
				return end();
			m_pArray = array;
			INT_PTR nIndex = _vfxQFindPos(m_pArray,m_nSize,_Key,__Compare());
			if(nIndex != m_nSize)
				memmove((void *)&m_pArray[nIndex + 1],(void *)&m_pArray[nIndex],(m_nSize - nIndex) * sizeof(mapped_type));

			new(&m_pArray[nIndex]) Wrapper<key_type,value_type>(_Key,_Val);

			_Iter = m_pArray + nIndex;
			m_nSize++;
		}

		return _Iter;
	}
	template<class InputIterator>
	void copy_from(InputIterator _First,InputIterator _Last)
	{
		for(InputIterator::interator _Iter = _First;_Iter != _Last;++_Iter)
			insert(_Iter->first,_Iter->second);
	}
	//Retrieves a copy of the comparison object used to order keys in a map. 
	key_compare key_comp() const
	{
		return key_compare();
	}
	//Retrieves a copy of the comparison object used to order element values in a map. 
	//value_comp  
	//Returns an iterator to the first element in a map that with a key value that is equal to or greater than that of a specified key. 
	iterator lower_bound(const key_type& _Key)
	{
		return find(_Key);
	}
	const_iterator lower_bound(const key_type& _Key) const
	{
		return (const_iterator)find(_Key);
	}
	//Returns an iterator to the first element in a map that with a key value that is greater than that of a specified key. 
	iterator upper_bound(const key_type& _Key)
	{
		iterator _Iter = find(_Key);
		for(;_Iter < end() && (key_compare()(_Iter,_Key) == 0); ++_Iter);
		return _Iter;
	}
	const_iterator upper_bound(const key_type& _Key) const
	{
		const_iterator _Iter = find(_Key);
		for(;_Iter < end() && (key_compare()(_Iter,_Key) == 0); ++_Iter);
		return _Iter;
	}
	//Returns the maximum length of the map. 
	size_type max_size()
		{	return INT_MAX;}
	//Returns an iterator addressing the first element in a reversed map. 
	//rbegin()
	//rend()
	
	//Specifies a new size for a map. 
	size_type size( ) const
		{	return m_nSize;}
	//Exchanges the elements of two maps. 
	void swap(VSimpleMap<TKey,TVal> & _Right)
	{	
		mapped_type * _p = m_pArray;
		m_pArray = _Right.m_pArray;
		_Right.m_pArray = _p;
		size_type _s = m_nSize;
		m_nSize = _Right.m_nSize;
		_Right.m_nSize = _s;
	}
	value_type & operator[](const key_type& _Key)
	{
		return find(_Key).second;
	}
	const value_type & operator[](const key_type& _Key) const
	{
		return find(_Key).second;
	}

//	typedef TKey							key_type;
//	typedef TVal							value_type;
//	typedef VPair<const TKey,TVal>			mapped_type;
//	typedef VPair<const TKey,TVal> *		iterator;
//	typedef const VPair<const TKey,TVal> *	const_iterator;
//	typedef INT_PTR							size_type;
//	typedef TCOMP							key_compare;
//	
//	mapped_type *			m_pArray;
//	INT_PTR					m_nSize;
	VSimpleMap(const VSimpleMap<TKey,TVal> & _Right)
		:m_pArray(NULL),m_nSize(0)
	{
		operator = (_Right);
	}
	VSimpleMap & operator = (const VSimpleMap<TKey,TVal> & _Right)
	{
		clear();

		m_nSize = _Right.m_nSize;
		mapped_type * array = (mapped_type *)realloc(m_pArray,m_nSize * sizeof(mapped_type));
		if(array == NULL)
			return *this;
		m_pArray = array;
		for(INT_PTR i=0;i<m_nSize;++i)
			new(&m_pArray[i]) Wrapper<key_type,value_type>(_Right.m_pArray[i].first,_Right.m_pArray[i].second);

		return *this;
	}
};

#pragma warning(default : 4291)

//	VSimpleMap<VString,int>		__map;
//	__map.Add(VString(vT("300")),300);
//	__map.Add(VString(vT("200")),200);
//	__map.Add(VString(vT("100")),100);
//	__map.insert(VString(vT("200")),2000);
//
//	VSimpleMap<VString,int>::iterator _iter = __map.find(VString(vT("400")));
//	if(_iter == __map.end())
//		__map.insert(VString(vT("400")),400);
//
//	VSimpleMap<VString,int>::iterator _iend = __map.end();
//	for(_iter = __map.begin();_iter < _iend;++_iter)
//		std::cout << (const char *)_iter->first << " " << _iter->second << std::endl; 

//
//	VSimpleMap<int,int>	_map;
//	_map.Add(1,100);
//	_map.Add(2,200);
//	_map.Add(3,300);
//	int temp = _map.FindKey(1);
//	temp = _map.GetValueAt(temp);

