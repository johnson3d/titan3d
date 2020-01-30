#include "v3dxVector3.h"
#include "v3dxCylinder.h"
#include "v3dxIntersectFunction.h"

#define new VNEW

bool v3dxCylinder::intersect(const v3dxCylinder &cylinder) const
{
	return ::intersect(*this, cylinder);
}

bool v3dxCylinder::intersect( const v3dxSphere &sphere ) const
{
	return ::intersect( *this, sphere );
}

bool v3dxCylinder::intersect( const v3dxBox3 &box ) const
{
	return ::intersect( *this, box );
}
