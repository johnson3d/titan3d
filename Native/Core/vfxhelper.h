// vfxhelper.h
// 
// VictoryCore Code
// ¸¨Öúº¯Êý¿â
//
// Author : johnson3d
// More author :
// Create time : 2002-6-13
// Modify time :
//-----------------------------------------------------------------------------

#ifndef __LHELPER_H__
#define __LHELPER_H__
#include "../BaseHead.h"
#include "string/vfxstring.h"

#pragma once

	inline vBOOL _vfxIsDirSep(TCHAR ch){	
		return (ch == vT('\\') || ch == vT('/')); 
	}

	//Â·¾¶
	
	// index of the most significant bit in the mask
	inline int HighestBit(UINT32 mask)
	{
		int base;

		if (mask & 0xffff0000)
		{
			if (mask & 0xff000000)
			{
				base = 24;
				mask >>= 24;
			}
			else
			{
				base = 16;
				mask >>= 16;
			}
		}
		else
		{
			if (mask & 0x0000ff00)
			{
				base = 8;
				mask >>= 8;
			}
			else
			{
				base = 0;
			}
		}

		if (mask & 0x0000ff00)
		{
			base += 8;
			mask >>= 8;
		}

		if (mask & 0x000000f0)
		{
			base += 4;
			mask >>= 4;
		}

		if (mask & 0x0000000c)
		{
			base += 2;
			mask >>= 2;
		}

		if (mask & 0x00000002)
		{
			++base;
			mask >>= 1;
		}

		return base - 1 + (mask & 1);
	}

	// index of the least significant bit in the mask
	inline int LowestBit(UINT32 mask)
	{
		if (!mask)
			return -1;

		int base;

		if (!(mask & 0xffff))
		{
			if (!(mask & 0x00ff0000))
			{
				base = 24;
				mask >>= 24;
			}
			else
			{
				base = 16;
				mask >>= 16;
			}
		}
		else
		{
			if (!(mask & 0x00ff))
			{
				base = 8;
				mask >>= 8;
			}
			else
			{
				base = 0;
			}
		}

		if (!(mask & 0x000f))
		{
			base += 4;
			mask >>= 4;
		}

		if (!(mask & 0x0003))
		{
			base += 2;
			mask >>= 2;
		}

		return base + 1 - (mask & 1);
	}
	// vBOOL WStringToString(const VStringW &wstr, VBaseStringA &str);
	

#endif