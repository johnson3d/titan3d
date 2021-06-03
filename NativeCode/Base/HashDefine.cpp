#include "HashDefine.h"
#include "BaseHead.h"

#define new VNEW

Hash64 Hash64::Empty;

Hash64 HashHelper::MurmurHash64B(const void * key, int len, unsigned int seed)
{
	const unsigned int m = 0x5bd1e995;
	const int r = 24;

	unsigned int h1 = seed ^ len;
	unsigned int h2 = 0;

	const unsigned int * data = (const unsigned int *)key;

	while (len >= 8)
	{
		unsigned int k1 = *data++;
		k1 *= m; k1 ^= k1 >> r; k1 *= m;
		h1 *= m; h1 ^= k1;
		len -= 4;

		unsigned int k2 = *data++;
		k2 *= m; k2 ^= k2 >> r; k2 *= m;
		h2 *= m; h2 ^= k2;
		len -= 4;
	}

	if (len >= 4)
	{
		unsigned int k1 = *data++;
		k1 *= m; k1 ^= k1 >> r; k1 *= m;
		h1 *= m; h1 ^= k1;
		len -= 4;
	}

	switch (len)
	{
	case 3: h2 ^= ((unsigned char*)data)[2] << 16;
	case 2: h2 ^= ((unsigned char*)data)[1] << 8;
	case 1: h2 ^= ((unsigned char*)data)[0];
		h2 *= m;
	};

	h1 ^= h2 >> 18; h1 *= m;
	h2 ^= h1 >> 22; h2 *= m;
	h1 ^= h2 >> 17; h1 *= m;
	h2 ^= h1 >> 19; h2 *= m;

	Hash64 result;
	result.Int64Value = (UINT64)((UINT64)h1 << 32) | (UINT64)h2;

	return result;
}

Hash64 HashHelper::CalcHash64(const void * key, int len)
{
	return MurmurHash64B(key, len, 0xee6b27eb);
}

extern "C"
{
	VFX_API void SDK_HashHelper_CalcHash64(Hash64* hash, const void * key, int len)
	{
		*hash = HashHelper::CalcHash64(key, len);
	}
}