// v3dKeyTime.h : header file
//
// Author : johnson
// Modifer :	
// Create Timer :	 2006-4-16   21:27
// Modify Timer :	 
//--------------------------------------------------------------------------------------------------
#pragma once
#include "../precompile.h"

//===========================================================================
// Summary:
//===========================================================================
class TimeKeys
{
public:
	//-----------------------------------------------------------------------
	// Summary:
	//-----------------------------------------------------------------------
	TimeKeys();

	//-----------------------------------------------------------------------
	// Summary:
	//-----------------------------------------------------------------------
	~TimeKeys();

	//-----------------------------------------------------------------------
	// Summary:
	//-----------------------------------------------------------------------
	TimeKeys(const TimeKeys & ref);

	//-----------------------------------------------------------------------
	// Summary:
	//-----------------------------------------------------------------------
	TimeKeys & operator = (const TimeKeys & ref);

	//-----------------------------------------------------------------------
	// Summary:
	//-----------------------------------------------------------------------
	void FinalCleanup();

	//-----------------------------------------------------------------------
	vBOOL GetTweenTimeParam(INT64 uTime,UINT & uFrm1,UINT & uFrm2,FLOAT & fSlerp) const;

	//-----------------------------------------------------------------------
	//-----------------------------------------------------------------------
	template<class _iotype>
	vBOOL Save(_iotype & file) const;

	//-----------------------------------------------------------------------
	//-----------------------------------------------------------------------
	template<class _iotype>
	vBOOL Load(_iotype & file);

	//-----------------------------------------------------------------------
	//-----------------------------------------------------------------------
	UINT GetDuration() const{ return m_uDuration;}

	//-----------------------------------------------------------------------
	//-----------------------------------------------------------------------
	UINT GetFrameCount() const{ return m_uFrameCount;}

	//-----------------------------------------------------------------------
	// Summary:
	//-----------------------------------------------------------------------
	UINT GetKeyCount() const{ return m_uKeyCount;}

	//-----------------------------------------------------------------------
	// Summary:
	//-----------------------------------------------------------------------
	UINT * GetTimes() const{return m_pTimes;}

	//-----------------------------------------------------------------------
	// Summary:
	//-----------------------------------------------------------------------
	UINT & operator [](size_t uIndex){ return m_pTimes[uIndex];}

	//-----------------------------------------------------------------------
	// Summary:
	//-----------------------------------------------------------------------
	UINT operator [](size_t uIndex) const{ return m_pTimes[uIndex];}

	//-----------------------------------------------------------------------
	// Summary:
	//-----------------------------------------------------------------------
	UINT * CreateTimes(UINT uKeyCount);

	//-----------------------------------------------------------------------
	// Summary:
	//-----------------------------------------------------------------------
	void __SetDurationFrame(UINT uDuration,UINT uFrameCount)
	{
		m_uDuration = uDuration;
		m_uFrameCount = uFrameCount;
	}

	//-----------------------------------------------------------------------
	// Summary:
	//-----------------------------------------------------------------------
	UINT GetResSize() const{ return m_uResSize;}
protected:
	UINT			m_uDuration;
	UINT			m_uFrameCount;
	UINT			m_uKeyCount;
    UINT			m_uResSize;
	UINT *			m_pTimes;
	
};

template<class _iotype>
vBOOL TimeKeys::Save(_iotype & file) const
{
	file.Write(&m_uDuration,sizeof(m_uDuration));
	file.Write(&m_uFrameCount,sizeof(m_uFrameCount));
	file.Write(&m_uKeyCount,sizeof(m_uKeyCount));

	file.Write(m_pTimes,sizeof(UINT) * m_uKeyCount);
	return TRUE;
}

template<class _iotype>
vBOOL TimeKeys::Load(_iotype & file)
{
	UINT uKeyCount = 0;
	m_uDuration = 0;
	m_uFrameCount = 0;
	file.Read(&m_uDuration, sizeof(m_uDuration));
	file.Read(&m_uFrameCount, sizeof(m_uFrameCount));
	file.Read(&uKeyCount, sizeof(uKeyCount));
	if (uKeyCount <= 0)
		return FALSE;

	CreateTimes(uKeyCount);
	file.Read(m_pTimes, sizeof(UINT) * uKeyCount);

	UINT uTimeBase = m_pTimes[0];
	for (UINT i = 1; i < uKeyCount; ++i)
	{
		m_pTimes[i] -= uTimeBase;
	}

	if (m_uDuration > m_pTimes[uKeyCount - 1])
		return FALSE;

	m_uResSize = sizeof(UINT) * uKeyCount;
	return TRUE;
}
