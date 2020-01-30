#pragma once

#include "../../BaseHead.h"

NS_BEGIN
class XNDAttrib;

class vBitset
{

public:
	typedef unsigned int block_type;		// 最小存储单元块的类型
	typedef unsigned int size_type;		// 最小存储单元块的大小类型
private:
	enum { BLOCK_BYTES = sizeof(block_type) };	// 最小存储单元块的字节数
	enum { BLOCK_BITS = 8 * BLOCK_BYTES };	//最小单元存储块的位数，平台通用性，所以使用numeric_limits

private:
	void leftShift(size_type num);		// 左移操作
	void rightShift(size_type num);		// 右移操作

private:
	size_type m_bitNum;		// bitset中位的个数
	size_type m_size;		// block_type的个数
	block_type *m_pBits;	// 存储bit位
	block_type m_mask;		// 假如bitset的位数是5，而m_pBits[0]=0xFFFFFFFF,m_mask用来表示
							// m_pBits[0]的后5位有效

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
	vBitset& operator <<= (size_type num);		// 左移赋值操作
	vBitset& operator >>= (size_type num);		// 右移赋值操作
	vBitset& operator &= (const vBitset &val);
	vBitset& operator |= (const vBitset &val);
	vBitset& operator ^= (const vBitset &val);
	bool operator == (const vBitset& val) const;
	vBitset& operator ~();						// 取反操作符
												// bool operator[] (size_type pos) const;
												// vBitsetReference operator[] (size_type pos);

	vBitset& flip();							// 反转所有位操作
	vBitset& flip(size_type pos);				// 反转pos位的操作
	vBitset& set(size_type pos, bool tagSet = true);	// 设置bitset中的某一位的值
	vBitset& set(bool tagSet = false);			// 设置bitset中每一位的值
	bool test(size_type pos) const;				// 测试相应位是1还是0,1返回true,0返回false

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