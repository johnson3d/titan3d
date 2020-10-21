#pragma once

#include "../../BaseHead.h"

NS_BEGIN
class XNDAttrib;

class vBitset
{

public:
	typedef unsigned int block_type;
	typedef unsigned int size_type;
private:
	enum { BLOCK_BYTES = sizeof(block_type) };
	enum { BLOCK_BITS = 8 * BLOCK_BYTES };

private:
	void leftShift(size_type num);
	void rightShift(size_type num);

private:
	size_type m_bitNum;
	size_type m_size;
	block_type *m_pBits;
	block_type m_mask;

	vBitset(void);

public:
	class vBitsetReference
	{
		friend class vBitset;

	private:
		vBitset * pBitset;	// pointer to the bitset
		size_type myPos;	// position of element in bitset

		vBitsetReference(vBitset& _bitset, size_type pos) :
			pBitset(&_bitset),
			myPos(pos)
		{
		}


	public:
		vBitsetReference & operator=(bool val)
		{
			pBitset->set(myPos, val);
			return (*this);
		}

		vBitsetReference& operator=(const vBitsetReference& bitRef)
		{
			pBitset->set(myPos, bool(bitRef));
			return (*this);
		}

		vBitsetReference& flip()
		{
			pBitset->flip(myPos);
			return (*this);
		}

		bool operator~() const
		{
			return (!pBitset->test(myPos));
		}

		operator bool() const
		{
			return (pBitset->test(myPos));
		}
	};

public:
	vBitset(size_type val);
	vBitset(const vBitset &val);
	~vBitset(void);

	vBitset& operator=(const vBitset& val);
	vBitset& operator <<= (size_type num);
	vBitset& operator >>= (size_type num);
	vBitset& operator &= (const vBitset &val);
	vBitset& operator |= (const vBitset &val);
	vBitset& operator ^= (const vBitset &val);
	bool operator == (const vBitset& val) const;
	vBitset& operator ~();						
												
												

	vBitset& flip();							
	vBitset& flip(size_type pos);				
	vBitset& set(size_type pos, bool tagSet = true);
	vBitset& set(bool tagSet = false);			
	bool test(size_type pos) const;				

	size_type size() const;

	bool Save(XNDAttrib* pAttr, bool withBegin = true);
	bool Load(XNDAttrib* pAttr, bool withBegin = true);
};

vBitset operator << (const vBitset&, vBitset::size_type);
vBitset operator >> (const vBitset&, vBitset::size_type);
vBitset operator & (const vBitset&, const vBitset&);
vBitset operator | (const vBitset&, const vBitset&);
vBitset operator ^ (const vBitset&, const vBitset&);

NS_END