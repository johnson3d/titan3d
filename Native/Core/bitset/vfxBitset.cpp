#include "../precompile.h"
#include "vfxBitset.h"
#include "../xnd/vfxxnd.h"

#define new VNEW

NS_BEGIN

vBitset::vBitset(void)
{
}

vBitset::vBitset(vBitset::size_type bitNum)
{
	m_bitNum = bitNum;
	size_type free_bits = (BLOCK_BITS - m_bitNum % BLOCK_BITS) % BLOCK_BITS;
	m_size = m_bitNum / BLOCK_BITS + (free_bits == 0 ? 0 : 1);
	m_pBits = new block_type[m_size];
	ASSERT(m_pBits);
	set(false);
	m_mask = ~block_type(0);
	m_mask >>= free_bits;
}
vBitset::vBitset(const vBitset &val)
{
	m_size = val.m_size;
	m_pBits = new block_type[m_size];
	ASSERT(m_pBits);
	memcpy(m_pBits, val.m_pBits, m_size * BLOCK_BYTES);
	m_bitNum = val.m_bitNum;
	m_mask = val.m_mask;
}

vBitset::~vBitset(void)
{
	delete[] m_pBits;
}

vBitset& vBitset::operator=(const vBitset &val)
{
	if (this == &val)
		return (*this);

	if (m_size != val.m_size)
	{
		delete[] m_pBits;
		m_size = val.m_size;
		m_pBits = new block_type[m_size];
		ASSERT(m_pBits);
	}

	memcpy(m_pBits, val.m_pBits, m_size * BLOCK_BYTES);
	m_bitNum = val.m_bitNum;
	m_mask = val.m_mask;
	return (*this);
}

vBitset& vBitset::operator<<=(vBitset::size_type num)
{
	leftShift(num);
	return (*this);
}

vBitset& vBitset::operator>>=(vBitset::size_type num)
{
	rightShift(num);
	return (*this);
}

vBitset::size_type vBitset::size() const
{
	return m_bitNum;
}

bool vBitset::Save(EngineNS::XNDAttrib* pAttr, bool withBegin)
{
	if (pAttr == NULL)
		return false;

	if (withBegin)
		pAttr->BeginWrite();
	pAttr->Write(m_pBits, m_size * BLOCK_BYTES);
	if (withBegin)
		pAttr->EndWrite();
	return true;
}
bool vBitset::Load(EngineNS::XNDAttrib* pAttr, bool withBegin)
{
	if (pAttr == NULL)
		return false;

	if (withBegin)
		pAttr->BeginRead(__FILE__, __LINE__);
	pAttr->Read(m_pBits, m_size * BLOCK_BYTES);
	if (withBegin)
		pAttr->EndRead();
	return true;
}


vBitset& vBitset::operator&=(const vBitset &val)
{
	if (m_bitNum != val.m_bitNum)
	{
		ASSERT(0);
	}

	for (size_type i = 0; i<m_size; i++)
		m_pBits[i] &= val.m_pBits[i];

	return (*this);
}

vBitset& vBitset::operator|=(const vBitset &val)
{
	if (m_bitNum != val.m_bitNum)
	{
		ASSERT(0);
	}

	for (size_type i = 0; i<m_size; i++)
	{
		m_pBits[i] |= val.m_pBits[i];
	}
	return (*this);
}

vBitset& vBitset::operator ^= (const vBitset &val)
{
	if (m_bitNum != val.m_bitNum)
	{
		ASSERT(0);
	}

	for (size_type i = 0; i<m_size; i++)
		m_pBits[i] ^= val.m_pBits[i];
	return (*this);
}

bool vBitset::operator == (const vBitset& val) const
{
	if (m_bitNum != val.m_bitNum)
		return false;

	for (size_type i = 0; i<m_size; ++i)
	{
		if (m_pBits[i] != val.m_pBits[i])
			return false;
	}

	return true;
}

vBitset& vBitset::operator~()
{
	return vBitset(*this).flip();
}

//bool vBitset::operator[] (vBitset::size_type pos) const
//{
//	return test(pos);
//}
//vBitset::vBitsetReference vBitset::operator[] (vBitset::size_type pos)
//{
//	return (vBitsetReference(*this, pos));
//}

void vBitset::leftShift(vBitset::size_type num)
{
	if (num >= m_bitNum)
	{
		set(false);
		return;
	}

	size_type eleNum = num / BLOCK_BITS;
	size_type bitNum = num % BLOCK_BITS;
	if (eleNum != 0)
	{
		block_type *pTmp = new block_type[m_size];
		ASSERT(pTmp);
		memcpy(pTmp, m_pBits, (m_size - eleNum)*BLOCK_BYTES);
		memcpy(m_pBits + eleNum, pTmp, (m_size - eleNum)*BLOCK_BYTES);
		memset(m_pBits, 0, eleNum*BLOCK_BYTES);
		delete[] pTmp;
	}

	if (bitNum != 0)
	{
		block_type* pTmp = m_pBits + m_size - 1;
		for (; pTmp > m_pBits; --pTmp)
		{
			*pTmp = (*pTmp << bitNum) | (*(pTmp - 1) >> (BLOCK_BITS - bitNum));	//*pTmp�ĵ�λ����(*(pTmp-1))�ĸ�λ
		}
		*pTmp <<= bitNum;
	}
	m_pBits[m_size - 1] &= m_mask;		// ���������λ��Ԫ������λ��0
}
void vBitset::rightShift(vBitset::size_type num)
{
	if (num >= m_bitNum)
	{
		set(false);
		return;
	}
	size_type eleNum = num / BLOCK_BITS;
	size_type bitNum = num % BLOCK_BITS;
	if (eleNum != 0)
	{
		block_type* pTmp = new block_type[m_size];
		ASSERT(pTmp);
		memcpy(pTmp, m_pBits + eleNum, (m_size = eleNum)*BLOCK_BYTES);
		memcpy(m_pBits, pTmp, (m_size - eleNum)*BLOCK_BYTES);
		memset(m_pBits + m_size - eleNum, 0, eleNum * BLOCK_BYTES);
		delete[] pTmp;
	}

	if (bitNum != 0)
	{
		block_type* pTmp = m_pBits;
		for (; pTmp < m_pBits + m_size - 1; ++pTmp)
		{
			*pTmp = (*pTmp >> bitNum) | (*(pTmp + 1) << (BLOCK_BITS - bitNum));
		}
		*pTmp >>= bitNum;
	}
}

vBitset& vBitset::set(vBitset::size_type pos, bool tagSet)
{
	ASSERT(pos <= m_bitNum && pos >= 0);

	size_type eleNum = pos / BLOCK_BITS;
	size_type bitNum = pos % BLOCK_BITS;

	block_type mask = 1;
	mask <<= bitNum;

	if (tagSet)
	{
		m_pBits[eleNum] |= mask;
	}
	else
	{
		m_pBits[eleNum] &= ~mask;
	}

	return (*this);
}
vBitset& vBitset::set(bool tagSet)
{
	if (tagSet)
	{
		//int val = std::numeric_limits<unsigned char>::max();
		//int val = 255;
		memset(m_pBits, 255, m_size * BLOCK_BYTES);
		m_pBits[m_size - 1] &= m_mask;
	}
	else
	{
		memset(m_pBits, 0, m_size * BLOCK_BYTES);
	}

	return (*this);
}
vBitset& vBitset::flip()
{
	for (size_type i = 0; i<m_size; ++i)
		m_pBits[i] = ~m_pBits[i];

	m_pBits[m_size - 1] &= m_mask;

	return (*this);
}
vBitset& vBitset::flip(vBitset::size_type pos)
{
	ASSERT(pos <= m_bitNum && pos >= 0);

	size_type eleNum = pos / BLOCK_BITS;
	size_type bitNum = pos % BLOCK_BITS;
	block_type mask = 1;
	mask <<= bitNum;
	m_pBits[eleNum] = m_pBits[eleNum] ^ mask;
	return (*this);
}
bool vBitset::test(vBitset::size_type pos) const
{
	ASSERT(pos <= m_bitNum && pos >= 0);

	size_type eleNum = pos / BLOCK_BITS;
	size_type bitNum = pos % BLOCK_BITS;
	block_type mask = 1;
	mask <<= bitNum;
	return ((m_pBits[eleNum] & mask) > 0);
}

//////////////////////////////////////////////////////////////////////////

vBitset operator << (const vBitset& val, vBitset::size_type num)
{
	return vBitset(val) <<= num;
}
vBitset operator >> (const vBitset& val, vBitset::size_type num)
{
	return vBitset(val) >>= num;
}
vBitset operator & (const vBitset& l, const vBitset& r)
{
	return vBitset(l) &= r;
}
vBitset operator | (const vBitset& l, const vBitset& r)
{
	return vBitset(l) |= r;
}
vBitset operator ^ (const vBitset& l, const vBitset& r)
{
	return vBitset(l) ^= r;
}

NS_END

using namespace EngineNS;

extern "C"
{
	VFX_API vBitset* SDK_vBitset_New()
	{
		return new vBitset(0xFFFFFFFF);
	}
	VFX_API void SDK_vBitset_Delete(vBitset* self)
	{
		delete self;
	}
	VFX_API void SDK_vBitset_Copy(vBitset* self, vBitset* src)
	{
		if (self == nullptr)
			return;

		self->operator=(*src);
	}
	VFX_API void SDK_vBitset_LeftShift(vBitset* self, UINT num)
	{
		if (self == nullptr)
			return;

		self->operator<<=(num);
	}
	VFX_API void SDK_vBitset_RightShift(vBitset* self, UINT num)
	{
		if (self == nullptr)
			return;

		self->operator>>=(num);
	}
	VFX_API void SDK_vBitset_And(vBitset* self, vBitset* right)
	{
		if (self == nullptr)
			return;

		self->operator&=(*right);
	}
	VFX_API void SDK_vBitset_Or(vBitset* self, vBitset* right)
	{
		if (self == nullptr)
			return;

		self->operator|=(*right);
	}
	VFX_API void SDK_vBitset_ExclusiveOr(vBitset* self, vBitset* right)
	{
		if (self == nullptr)
			return;

		self->operator^=(*right);
	}
	VFX_API void SDK_vBitset_Not(vBitset* self)
	{
		if (self == nullptr)
			return;

		self->operator~();
	}
	VFX_API void SDK_vBitset_Save(vBitset* self, XNDAttrib* pAttr)
	{
		if (self == nullptr)
			return;
		self->Save(pAttr, false);
	}
	VFX_API vBOOL SDK_vBitset_Load(vBitset* self, XNDAttrib* pAttr)
	{
		if (self == nullptr)
			return FALSE;
		return self->Load(pAttr, false) ? TRUE : FALSE;
	}
}