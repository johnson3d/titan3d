/********************************************************************
	V3D					A Powerful 3D Enjine
	File:				v3dxsegment3.h
	Created Time:		30:6:2002   16:29
	Modify Time:
	Original Author:	< johnson >
	More Author:	
	Abstract:			?
	
	Note:				

*********************************************************************/

#ifndef __v3dxSegment3__H__
#define __v3dxSegment3__H__

#include "v3dxVector3.h"

#pragma pack(push,4)

class v3dxSegment3
{
public:
	v3dxSegment3 ()
	{	}

	v3dxSegment3 (const v3dxVector3& s, const v3dxVector3& e)
	{ 
		m_vStart = s; 
		m_vEnd = e; 
		m_vLength = m_vEnd - m_vStart;
	}

	inline void set (const v3dxVector3& s, const v3dxVector3& e)
	{ 
		m_vStart = s; 
		m_vEnd = e; 
		m_vLength = m_vEnd - m_vStart;
	}
	inline void setStart (const v3dxVector3& s) { 
		m_vStart = s; 
		m_vLength = m_vEnd - m_vStart;
	}

	inline void setEnd (const v3dxVector3& e) { 
		m_vEnd = e; 
		m_vLength = m_vEnd - m_vStart;
	}

	inline const v3dxVector3& getStart () const { 
		return m_vStart; 
	}

	inline const v3dxVector3& getEnd () const { 
		return m_vEnd; 
	}
	inline const v3dxVector3& getLength () const{ 
		return m_vLength ; 
	}

	inline v3dxVector3& getStart () { 
		return m_vStart; 
	}

	inline v3dxVector3& getEnd () { 
		return m_vEnd; 
	}
	inline v3dxVector3& getLength () { 
		return m_vLength ; 
	}
protected:
	v3dxVector3 m_vStart;
	v3dxVector3 m_vEnd;
	v3dxVector3 m_vLength;
};

#pragma pack(pop)

#endif