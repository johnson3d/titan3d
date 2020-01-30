#include "TimeKeys.h"
#include<algorithm>

#define new VNEW

TimeKeys::TimeKeys()
	:m_uDuration(0)
	,m_uFrameCount(0)
	,m_uKeyCount(0)
	,m_uResSize(0)
{
	m_pTimes = 0;
}

TimeKeys::~TimeKeys()
{
	FinalCleanup();
}

TimeKeys::TimeKeys(const TimeKeys & ref)
{
	m_uDuration = ref.m_uDuration;
	m_uFrameCount = ref.m_uFrameCount;
	m_uKeyCount = ref.m_uKeyCount;
	m_pTimes = new UINT[m_uKeyCount];
	memcpy(m_pTimes,ref.m_pTimes,sizeof(UINT) * m_uKeyCount);
	m_uResSize = ref.m_uResSize;
}

TimeKeys & TimeKeys::operator = (const TimeKeys & ref)
{
	if(&ref != this)
	{
		FinalCleanup();

		m_uDuration = ref.m_uDuration;
		m_uFrameCount = ref.m_uFrameCount;
		m_uKeyCount = ref.m_uKeyCount;
		m_pTimes = new UINT[m_uKeyCount];
		memcpy(m_pTimes,ref.m_pTimes,sizeof(UINT) * m_uKeyCount);
		m_uResSize = ref.m_uResSize;
	}

	return *this;
}

void TimeKeys::FinalCleanup()
{
	m_uDuration = 0;
	m_uFrameCount = 0;
	m_uKeyCount = 0;
	delete [] m_pTimes;
	m_pTimes = NULL;
	m_uResSize = 0;
}

vBOOL TimeKeys::GetTweenTimeParam(INT64 n64Time,UINT & uFrm1,UINT & uFrm2,FLOAT & fSlerp) const
{
	//if(m_uKeyCount <= 0 || m_uDuration <= 0 || m_pTimes == NULL)
	if(m_uKeyCount <= 0 )
		return FALSE;

	if( m_uKeyCount==1 )
	{
		uFrm1 = uFrm2 = 0;
		fSlerp = 0.0f;
		return TRUE;
	}

	UINT uTime = ((UINT)n64Time) % (m_uDuration+1);
	UINT * pFinded = std::lower_bound(m_pTimes,m_pTimes + m_uKeyCount,uTime);
	////ASSERT(pFinded <= m_pTimes + m_uKeyCount);
	/*if( *pFinded == uTime )
	{
		uFrm1 = uFrm2 = (UINT)(pFinded - m_pTimes);
		fSlerp = 0.0f;
	}
	else */
	if( pFinded == m_pTimes + m_uKeyCount )
	{
		uFrm2 = m_uKeyCount-1;
		uFrm1 = uFrm2-1;
		// 之前的错误代码:
		//fSlerp = 0.0f;
		fSlerp = 1.0f;
	}
	else
	{
		uFrm2 = (UINT)(pFinded - m_pTimes);
		if( uFrm2==0 )
		{
			uFrm1 = 0;
			fSlerp = 0.0f;
		}
		else
		{
			uFrm1 = uFrm2 - 1;
			fSlerp = FLOAT(uTime - m_pTimes[uFrm1]) / (m_pTimes[uFrm2] - m_pTimes[uFrm1]);
		}
	}
	return TRUE;
	///////

	//for(UINT i=0; i<m_uKeyCount; ++i)
	//{
	//	if(m_pTimes[i] == uTime)
	//	{
	//		uFrm1 = uFrm2 = i;
	//		fSlerp = 1.0f;
	//		return TRUE;
	//	}
	//	else if(m_pTimes[i] > uTime)
	//	{
	//		if(i == 0)
	//		{
	//			uFrm1 = uFrm2 = 0;
	//			fSlerp = 1.0f;
	//		}
	//		else
	//		{
	//			uFrm1 = i - 1;
	//			uFrm2 = i;
	//			fSlerp = FLOAT(uTime - m_pTimes[uFrm1]) / (m_pTimes[uFrm2] - m_pTimes[uFrm1]);
	//		}
	//		return TRUE;
	//	}
	//}
	//	uFrm2 = m_uKeyCount-1;
	//	uFrm1 = uFrm2-1;
	//	fSlerp = 0.0f;

	//return TRUE;
}

UINT * TimeKeys::CreateTimes(UINT uKeyCount)
{
	if(m_uKeyCount != uKeyCount)
	{
		delete [] m_pTimes;
		m_pTimes = new UINT[uKeyCount];
		m_uKeyCount = uKeyCount;
	}
	return m_pTimes;
}
