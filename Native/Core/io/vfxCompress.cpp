#include "../string/vfxstring.h"

#define new VNEW

template <class Type>
int T_vfxRLE_Ecode(const Type * in, unsigned long in_length_by_count, void * _out)
{
	if (_out == NULL)
	{
		int r = 0;
		for (; in_length_by_count > 1;)
		{
			if (in[0] == in[1])
			{
				unsigned long c = 1;
				for (; in[0] == in[c] && c < 127 && c < in_length_by_count; c++);
				r += 1 + sizeof(Type);
				in += c;
				in_length_by_count -= c;
			}
			else
			{
				unsigned long c = 1;
				for (; in[c] != in[c + 1] && c < 127 && c < in_length_by_count; c++);
				r += 1 + sizeof(Type) * c;
				in += c;
				in_length_by_count -= c;
			}
		}
		if (in_length_by_count == 1)
			r += 1 + sizeof(Type);
		return r;
	}
	else
	{
		BYTE * out = (BYTE *)_out;
		for (; in_length_by_count > 1;)
		{
			if (in[0] == in[1])
			{
				unsigned long c = 1;
				for (; in[0] == in[c] && c < 127 && c < in_length_by_count; c++);
				*out = (BYTE)c;
				++out;
				*((Type *)out) = *in;
				out += sizeof(Type);
				in += c;
				in_length_by_count -= c;
			}
			else
			{
				unsigned long c = 1;
				for (; in[c] != in[c + 1] && c < 127 && c < in_length_by_count; c++);
				*out = (BYTE)(c + 127);
				++out;
				memcpy(out, in, c * sizeof(Type));
				out += c * sizeof(Type);
				in += c;
				in_length_by_count -= c;
			}
		}
		if (in_length_by_count == 1)
		{
			*out = (BYTE)128;
			++out;
			*((Type *)out) = *in;
			out += sizeof(Type);
		}
		return (int)(out - (BYTE *)_out);
	}
}

template <class Type>
int T_vfxRLE_Unecode(const void * _in, int in_length_by_bytes, Type * out)
{
	if (out == NULL)
	{
		BYTE * in = (BYTE *)_in;
		int r = 0;
		for (; in_length_by_bytes > 0;)
		{
			if (*in > 127)
			{
				int c = *in - 127;
				in += 1 + sizeof(Type) * c;
				in_length_by_bytes -= 1 + sizeof(Type) * c;
				r += c;
			}
			else
			{
				int c = *in;
				in += 1 + sizeof(Type);
				in_length_by_bytes -= 1 + sizeof(Type);
				r += c;
			}
		}
		return r;
	}
	else
	{
		BYTE * in = (BYTE *)_in;
		Type * backup = out;
		for (; in_length_by_bytes > 0;)
		{
			if (*in > 127)
			{
				int c = *in - 127;
				memcpy(out, in + 1, sizeof(Type) * c);
				out += c;
				in += 1 + sizeof(Type) * c;
				in_length_by_bytes -= 1 + sizeof(Type) * c;
			}
			else
			{
				int c = *in;
				++in;
				for (int i = 0; i < c; ++i)
					out[i] = *(Type *)in;
				out += c;
				in += sizeof(Type);
				in_length_by_bytes -= 1 + sizeof(Type);
			}
		}
		return (int)(out - backup);
	}
}

void SubBytes(const BYTE* lh, const BYTE* rh, BYTE* pOut, int length)
{
	for (int i = 0; i < length; i++)
	{
		pOut[i] = lh[i] - rh[i];
	}
}

void AddBytes(const BYTE* lh, const BYTE* rh, BYTE* pOut, int length)
{
	for (int i = 0; i < length; i++)
	{
		pOut[i] = lh[i] + rh[i];
	}
}

extern "C"
{
	VFX_API int SDK_Compress_RLE(const BYTE * in, unsigned long in_length_by_count, BYTE* _out)
	{
		return T_vfxRLE_Ecode<BYTE>(in, in_length_by_count, _out);
	}

	VFX_API int SDK_UnCompress_RLE(const BYTE * in, unsigned long in_length_by_count, BYTE* _out)
	{
		return T_vfxRLE_Unecode<BYTE>(in, in_length_by_count, _out);
	}

	VFX_API void SDK_ByteArray_Sub(const BYTE* lh, const BYTE* rh, BYTE* pOut, int length)
	{
		SubBytes(lh, rh, pOut, length);
	}

	VFX_API void SDK_ByteArray_Add(const BYTE* lh, const BYTE* rh, BYTE* pOut, int length)
	{
		AddBytes(lh, rh, pOut, length);
	}
};
