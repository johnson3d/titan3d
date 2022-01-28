// file_2_memory.h
//
//
// Author : johnson
// Modifier :	
// Create Timer :	 2004-10-24 4:09
// Modify Timer :	
//-------------------------------------------------------------------------------------------------
#pragma once


//#include "../victorycoreex/vfxhash_string.h"

//#include "../xnd/vfxxnd.h"
#include "../thread/vfxcritical.h"
#include "../io/vfxfile.h"
#include "res_2_memory.h"

NS_BEGIN

const vIID vIID_VFile2Memory = 0x149386ee4ccfeb6a;
class VFile2Memory : public VRes2Memory
{
	VCritical					mLocker;
public:
	ENGINE_RTTI(VFile2Memory);
	/*!	\copydoc VRes2Memory::Ptr */
	virtual  VResPtr	Ptr(UINT_PTR offset , UINT_PTR size=-1) override;
	/*!	\copydoc VRes2Memory::Free */
	virtual  vBOOL		Free() override;
	/*!	\copydoc VRes2Memory::Length */
	virtual  UINT_PTR	Length() const override;
	/*!	\copydoc VRes2Memory::Name */
	virtual  LPCSTR		Name() const override; 

	virtual  void		TryReleaseHolder() override;

	 VFile2Memory();
	 ~VFile2Memory();

	 vBOOL Create(LPCSTR pszFile,vBOOL bShareFile = FALSE);
	 void Close();
	inline  void SetName(const VStringA & str){
		mName = str;
	}
	virtual long GetRefCount() override{
		return mPtrRef;
	}
private:
	VStringA	mName;
	
	ViseFile	mFile;
	BYTE*		mCachedBuffer;
	INT_PTR		mCachedStarter;
	INT_PTR		mCachedSize;
	//INT			mPtrRef;
	std::atomic<long> mPtrRef;
	vBOOL		mIsClosing;
};

class VMemoryResPtr : public VRes2Memory
{
public:
	virtual  VResPtr	Ptr(UINT_PTR offset=0 , UINT_PTR size=0);
	virtual  vBOOL		Free();
	virtual  UINT_PTR	Length() const;
	virtual  LPCSTR		Name() const; 

	 VMemoryResPtr();
	 ~VMemoryResPtr();

	 vBOOL Create(LPCSTR pszName,VResPtr pPtr,DWORD dwLength);
private:
	VResPtr		m_ptrBase;
	UINT_PTR	m_dwLength;
	VStringA	m_strName;
};

class	VFile2ResFactory : public VResFactory
{
	/*!	\copydoc VResFactory::CreateRes */
	virtual  VRes2Memory*	CreateRes(LPCSTR pszFile,vBOOL bShareFile);
};

NS_END
