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
//		v3dKeyTime描述的关键帧信息
//===========================================================================
class TimeKeys
{
public:
	//-----------------------------------------------------------------------
	// Summary:
	//		v3dKeyTime的构造函数
	//-----------------------------------------------------------------------
	TimeKeys();

	//-----------------------------------------------------------------------
	// Summary:
	//		v3dKeyTime的析构函数
	//-----------------------------------------------------------------------
	~TimeKeys();

	//-----------------------------------------------------------------------
	// Summary:
	//		v3dKeyTime的拷贝构造函数
	//-----------------------------------------------------------------------
	TimeKeys(const TimeKeys & ref);

	//-----------------------------------------------------------------------
	// Summary:
	//		v3dKeyTime的拷贝赋值函数
	//-----------------------------------------------------------------------
	TimeKeys & operator = (const TimeKeys & ref);

	//-----------------------------------------------------------------------
	// Summary:
	//		清理所有数据.可重复调用.
	//-----------------------------------------------------------------------
	void FinalCleanup();

	//-----------------------------------------------------------------------
	// Summary:
	//		调用本函数来查看是否需要双面渲染.
	// Parameters:
	//		uTime - 传入参数,指定得到插值信息的时间.内部对时间按照总时间长取余.
	//		uFrm1 - 输出参数,插值的前一帧
	//		uFrm2 - 输出参数,插值的后一帧
	//		fSlerp - 输出参数,插值的后一帧的影响系数
	//		插值结果=uFrm1+(uFrm2-uFrm1)*fSlerp
	// Returns:
	//		如果所有数据都准备好了,返回非零值,否则,返回零.
	//求插值结果的运算从来不会失败.
	//-----------------------------------------------------------------------
	vBOOL GetTweenTimeParam(INT64 uTime,UINT & uFrm1,UINT & uFrm2,FLOAT & fSlerp) const;

	//-----------------------------------------------------------------------
	// Summary:
	//		保存关键帧信息到文件.
	// Returns:
	//		成功返回非零,否则,返回零.
	//-----------------------------------------------------------------------
	template<class _iotype>
	vBOOL Save(_iotype & file) const;

	//-----------------------------------------------------------------------
	// Summary:
	//		加载关键帧数据.
	// Returns:
	//		成功返回非零,否则,返回零.
	//-----------------------------------------------------------------------
	template<class _iotype>
	vBOOL Load(_iotype & file);

	//-----------------------------------------------------------------------
	// Summary:
	//		得到关键帧信息总的时长.
	// Returns:
	//		返回关键帧信息总的时长.单位毫秒.
	//-----------------------------------------------------------------------
	UINT GetDuration() const{ return m_uDuration;}

	//-----------------------------------------------------------------------
	// Summary:
	//		得到关键帧信息总的帧数.
	// Returns:
	//		返回关键帧信息总的帧数.
	//-----------------------------------------------------------------------
	UINT GetFrameCount() const{ return m_uFrameCount;}

	//-----------------------------------------------------------------------
	// Summary:
	//		得到关键帧数目.
	// Returns:
	//		返回关键帧数目.
	//-----------------------------------------------------------------------
	UINT GetKeyCount() const{ return m_uKeyCount;}

	//-----------------------------------------------------------------------
	// Summary:
	//		得到关键帧的时间列表数组.
	// Returns:
	//		返回关键帧的时间列表数组.个数通过GetKeyCount()得到.
	//-----------------------------------------------------------------------
	UINT * GetTimes() const{return m_pTimes;}

	//-----------------------------------------------------------------------
	// Summary:
	//		得到指定关键帧的时间.
	// Parameters:
	//		uIndex - 指定的关键帧.
	// Returns:
	//		返回指定关键帧的时间.单位毫秒.
	//-----------------------------------------------------------------------
	UINT & operator [](size_t uIndex){ return m_pTimes[uIndex];}

	//-----------------------------------------------------------------------
	// Summary:
	//		得到指定关键帧的时间.
	// Parameters:
	//		uIndex - 指定的关键帧.
	// Returns:
	//		返回指定关键帧的时间.单位毫秒.
	//-----------------------------------------------------------------------
	UINT operator [](size_t uIndex) const{ return m_pTimes[uIndex];}

	//-----------------------------------------------------------------------
	// Summary:
	//		创建关键帧数据.
	// Parameters:
	//		uKeyCount - 关键帧数目.
	// Returns:
	//		返回关键帧的时间列表数组.
	//-----------------------------------------------------------------------
	UINT * CreateTimes(UINT uKeyCount);

	//-----------------------------------------------------------------------
	// Summary:
	//		设置关键帧的时间长度和总帧数信息.
	// Parameters:
	//		uDuration - 关键帧信息总的时长.
	//		uFrameCount - 关键帧信息总的帧数.
	//-----------------------------------------------------------------------
	void __SetDurationFrame(UINT uDuration,UINT uFrameCount)
	{
		m_uDuration = uDuration;
		m_uFrameCount = uFrameCount;
	}

	//-----------------------------------------------------------------------
	// Summary:
	//		得到内部使用的资源的尺寸.
	// Returns:
	//		返回内部使用的资源的尺寸,单位BYTE.
	//-----------------------------------------------------------------------
	UINT GetResSize() const{ return m_uResSize;}
protected:
	UINT			m_uDuration;			//关键帧信息总的时长
	UINT			m_uFrameCount;			//关键帧信息总的帧数
	UINT			m_uKeyCount;			//关键帧数目
    UINT			m_uResSize;				//使用的资源的尺寸
	UINT *			m_pTimes;				//关键帧的时间列表信息
	
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
