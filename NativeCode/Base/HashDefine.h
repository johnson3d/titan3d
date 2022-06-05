#pragma once

#if defined(PLATFORM_WIN)
#include <WinSock2.h>
#include <mswsock.h>
#endif

#include <cstring>
#include <string>

struct Hash64_t
{
	unsigned long long Int64Value;
};

struct Hash64 : public Hash64_t
{
	Hash64()
	{
		Int64Value = 0;
	}
	static Hash64	Empty;
};

class HashHelper
{
	typedef unsigned int uint;
public:
	Hash64 static MurmurHash64B(const void * key, int len, unsigned int seed);
	Hash64 static CalcHash64(const void * key, int len);

	static uint DefaultHash(const std::string& str)
	{
		return JSHash(str);
	}
	static uint RSHash(const std::string& str)
	{
		uint b = 378551;
		uint a = 63689;
		uint hash = 0;

		for (size_t i = 0; i < str.length(); i++)
		{
			hash = hash * a + str[i];
			a = a * b;
		}

		return hash;
	}
	static uint JSHash(const std::string& str)
	{
		uint hash = 1315423911;

		for (size_t i = 0; i < str.length(); i++)
		{
			hash ^= ((hash << 5) + str[i] + (hash >> 2));
		}

		return hash;
	}
	static uint ELFHash(const std::string& str)
	{
		uint hash = 0;
		uint x = 0;

		for (size_t i = 0; i < str.length(); i++)
		{
			hash = (hash << 4) + str[i];

			if ((x = hash & 0xF0000000) != 0)
			{
				hash ^= (x >> 24);
			}
			hash &= ~x;
		}
		return hash;
	}
	static uint BKDRHash(const std::string& str)
	{
		uint seed = 131; // 31 131 1313 13131 131313 etc..   
		uint hash = 0;

		for (size_t i = 0; i < str.length(); i++)
		{
			hash = (hash * seed) + str[i];
		}

		return hash;
	}
	/* End Of BKDR Hash Function */
	static uint SDBMHash(const std::string& str)
	{
		uint hash = 0;

		for (size_t i = 0; i < str.length(); i++)
		{
			hash = str[i] + (hash << 6) + (hash << 16) - hash;
		}

		return hash;
	}
	/* End Of SDBM Hash Function */
	static uint DJBHash(const std::string& str)
	{
		uint hash = 5381;

		for (size_t i = 0; i < str.length(); i++)
		{
			hash = ((hash << 5) + hash) + str[i];
		}

		return hash;
	}
	/* End Of DJB Hash Function */
	static uint DEKHash(const std::string& str)
	{
		int hash = (int)str.length();

		for (size_t i = 0; i < str.length(); i++)
		{
			hash = ((hash << 5) ^ (hash >> 27)) ^ str[i];
		}

		return (uint)hash;
	}
	/* End Of DEK Hash Function */
	static uint BPHash(const std::string& str)
	{
		uint hash = 0;

		for (size_t i = 0; i < str.length(); i++)
		{
			hash = hash << 7 ^ str[i];
		}

		return hash;
	}
	/* End Of BP Hash Function */
	static uint FNVHash(const std::string& str)
	{
		uint fnv_prime = 0x811C9DC5;
		uint hash = 0;

		for (size_t i = 0; i < str.length(); i++)
		{
			hash *= fnv_prime;
			hash ^= str[i];
		}

		return hash;
	}
	/* End Of FNV Hash Function */
	//static uint APHash(const std::string& str)
	static uint APHash(const char* str)
	{
		auto length = (unsigned int)strlen(str);
		return APHash((unsigned char*)str, length);
	}
	static uint APHash(unsigned char* str, unsigned int length)
	{
		uint hash = 0xAAAAAAAA;

		for (unsigned int i = 0; i < length; i++)
		{
			if ((i & 1) == 0)
			{
				hash ^= ((hash << 7) ^ str[i] * (hash >> 3));
			}
			else
			{
				hash ^= (~((hash << 11) + str[i] ^ (hash >> 5)));
			}
		}

		return hash;
	}
};
