// res_2_memory.h
//
//
// Author : johnson&lzp
// Modifer :	
// Create Timer :	 2004-10-24 4:04
// Modify Timer :	
//-------------------------------------------------------------------------------------------------
#ifndef __res_2_memory_h__2004_10_24_4_04__
#define __res_2_memory_h__2004_10_24_4_04__

#pragma once
typedef const void * VResPtr;

#include "../IUnknown.h"

NS_BEGIN

const vIID vIID_VRes2Memory = 0xe9ff69794c917a6c;
struct VRes2Memory : public EngineNS::VIUnknown
{
	virtual VResPtr		Ptr( UINT_PTR offset=0 , UINT_PTR size=0 ) = 0;
	virtual vBOOL		Free() = 0;
	virtual UINT_PTR	Length() const = 0;
	virtual LPCSTR		Name() const = 0; 

	virtual void		TryReleaseHolder()
	{
		return;
	}
	virtual long		GetRefCount()
	{
		return 0;
	}
};

const vIID vIID_VResFactory = 0xe6588b7b4c917a83;
struct VResFactory : public EngineNS::VIUnknown
{
	virtual VRes2Memory* CreateRes(LPCSTR pszFile,vBOOL bShareFile) = 0;
	virtual bool IsDownloading(LPCSTR pszFile) = 0;
	virtual LPCSTR GetDefaultResource(LPCSTR pszFile) = 0;
};

NS_END

#endif // end #ifndef __res_2_memory_h__2004_10_24_4_04__
