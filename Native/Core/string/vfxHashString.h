/********************************************************************
	created:	2007/03/02
	created:	2:3:2007   14:35
	filename: 	d:\New-Work\Victory\Code\victory3d\v3dHashString.h
	file path:	d:\New-Work\Victory\Code\victory3d
	file base:	v3dHashString
	file ext:	h
	author:		johnson
	
	purpose:	
*********************************************************************/
#ifndef __v3dHashString_h_2_3_2007_14_35__
#define __v3dHashString_h_2_3_2007_14_35__
#pragma once

struct vfxHashString : public VStringA
{
	unsigned int			dwHashCode;

	vfxHashString();
	vfxHashString(const char * psz);
	vfxHashString(const VStringA & ref);
	vfxHashString(const vfxHashString & ref);

	vfxHashString & operator = (const char * psz);
	vfxHashString & operator = (const VStringA& ref);
	vfxHashString & operator = (const vfxHashString & ref);

	/*bool operator < ( const v3dHashString<STRING>& r )
	{
		if( dwHashCode==r.dwHashCode )
			return (const STRING &)(*this) < (const STRING &)r;
		else
			return dwHashCode<r.dwHashCode;
	}*/

	void _CalcHashCode()
	{
		dwHashCode = APHash(this->c_str());
	}

	static unsigned int APHash(const char * str)
	{
		unsigned int hash = 0;
		int i;

		for (i=0; *str; i++)
		{
			if ((i & 1) == 0)
			{
				hash ^= ((hash << 7) ^ (*str++) ^ (hash >> 3));
			}
			else
			{
				hash ^= (~((hash << 11) ^ (*str++) ^ (hash >> 5)));
			}
		}

		return (hash & 0x7FFFFFFF);
	}

	static unsigned int DJBHash(const char *str)
	{
		unsigned int hash = 5381;

		while (*str)
		{
			hash += (hash << 5) + (*str++);
		}

		return (hash & 0x7FFFFFFF);
	}

	static unsigned int RSHash(const char *str)
	{
		unsigned int b = 378551;
		unsigned int a = 63689;
		unsigned int hash = 0;

		while (*str)
		{
			hash = hash * a + (*str++);
			a *= b;
		}

		return (hash & 0x7FFFFFFF);
	}
};

struct _HashStringCompare
{
	typedef vfxHashString	HashString;
	enum
	{	// parameters for hash table
		bucket_size = 4,	// 0 < bucket_size
		min_buckets = 8		// min_buckets = 2 ^^ N, 0 < N
	};	
	// hash _Keyval to size_t value
	inline size_t operator()(const HashString & _s) const 
	{
		return _s.dwHashCode;
	}
	// test if _Keyval1 ordered before _Keyval2
	inline bool operator()(const HashString & _Left,const HashString & _Right) const
	{
		return _Left < _Right;
	}
};

inline vfxHashString::vfxHashString()
: dwHashCode(0)
{
}

inline vfxHashString::vfxHashString(const char * psz)
: VStringA(psz)
{
	_CalcHashCode();
}

inline vfxHashString::vfxHashString(const VStringA & ref)
: VStringA(ref)
{
	_CalcHashCode();
}

inline vfxHashString::vfxHashString(const vfxHashString& ref)
: VStringA(ref)
{
	dwHashCode = ref.dwHashCode;
}

inline vfxHashString& vfxHashString::operator = (const char * psz)
{
	VStringA::operator = (psz);
	_CalcHashCode();
	return *this;
}

inline vfxHashString& vfxHashString::operator = (const VStringA& ref)
{
	VStringA::operator = (ref);
	_CalcHashCode();
	return *this;
}

inline vfxHashString& vfxHashString::operator = (const vfxHashString& ref)
{
	if(&ref != this)
	{
		VStringA::operator = (ref);
		dwHashCode = ref.dwHashCode;
	}
	return *this;
}

#endif//#ifdef __d:\New-Work\Victory\Code\victory3d_h_2_3_2007_14_35__