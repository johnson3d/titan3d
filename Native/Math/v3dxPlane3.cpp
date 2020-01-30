/********************************************************************
	V3D					A Powerful 3D Enjine
	File:				v3dxplane3.cpp
	Created Time:		30:6:2002   16:35
	Modify Time:
	Original Author:	< johnson >
	More Author:	
	Abstract:			?

	Note:				

*********************************************************************/
#include "v3dxPlane3.h"
#include "v3dxLine.h"

#define new VNEW

bool v3dxPlane3::intersect(const v3dxLine3 &line, float &t) const
{
	if(intersectInfinite(line, t) == false || t < -EPSILON)
	{
		return false;
	}

	return true;
}

bool v3dxPlane3::intersectInfinite(const v3dxLine3 &line, float &t) const
{
	float a;

	a = m_vNormal.dotProduct(line.getDirection());

	if( a <= -EPSILON || a >= EPSILON )
	{
		t = m_vNormal.dotProduct(
			v3dxVector3(m_vNormal * (-m_fDD)) -  line.getPosition()) / a;

		return true;
	}
	else
	{
		float b = m_vNormal.dotProduct(line.getPosition()) - m_fDD;
		if( b <= -EPSILON || b >= EPSILON )
		{
			t = 0.f;
			return true;
		}
	}

	return false;
}

int v3dxPlane3::intersectSegement(const v3dxVector3& start, const v3dxVector3& end, float &t) const
{
	float distStart = classify(start);
	float distEnd = classify(end);
	if (distStart > 0 && distEnd > 0)
		return 1;
	else if (distStart < 0 && distEnd < 0)
		return -1;
	else
	{
		auto len = end - start;
		float segLen = len.getLength();
		t = fabs(distStart * segLen / distEnd);
		return 0;
	}
}