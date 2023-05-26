#ifndef _Shader_Debug_Output_cginc_
#define _Shader_Debug_Output_cginc_

struct TtDebugOutput
{
	RWByteAddressBuffer OutputBuffer;
	void WriteLine(uint3 thread, int info)
	{
		uint index = 0;
		OutputBuffer.InterlockedAdd(0, 4, index);
		uint4 data(thread, info);
		OutputBuffer.Store4(4 + index * 4, data);
	}
};


#endif //_Shader_Debug_Output_cginc_