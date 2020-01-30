#ifndef _V3DXINTERSECTFUNCTION_H_
#define _V3DXINTERSECTFUNCTION_H_

#include "vfxGeomTypes.h"

class v3dxCylinder;
class v3dxSphere;
class v3dxBox3;
class v3dxVector3;

// Cylinder
 
bool intersect( const v3dxCylinder &cylinder1, const v3dxCylinder &cylinder2 );


bool intersect( const v3dxCylinder &cylinder, const v3dxSphere &sphere );


bool intersect( const v3dxCylinder &cylinder, const v3dxBox3 &box );

// Box

bool intersect( const v3dxBox3 &box, const v3dxSphere &sphere );

 
bool intersect( const v3dxBox3 &box , const v3dxVector3 tri[3] );


#endif // _V3DXINTERSECTFUNCTION_H_