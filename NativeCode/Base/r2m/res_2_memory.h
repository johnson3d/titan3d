// res_2_memory.h
//
//
// Author : johnson
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

struct VRes2Memory : public EngineNS::IWeakReference
{
	virtual VResPtr		Ptr(UINT64 offset=0, UINT64 size=0 ) = 0;
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

struct VResFactory : public EngineNS::IWeakReference
{
	virtual VRes2Memory* CreateRes(LPCSTR pszFile,vBOOL bShareFile) = 0;
	virtual bool IsDownloading(LPCSTR pszFile) = 0;
	virtual LPCSTR GetDefaultResource(LPCSTR pszFile) = 0;
};

NS_END

#endif // end #ifndef __res_2_memory_h__2004_10_24_4_04__
