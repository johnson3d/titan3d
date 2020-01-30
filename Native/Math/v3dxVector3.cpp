/********************************************************************
	V3D					A Powerful 3D Enjine
	File:				v3dxvector3.cpp
	Created Time:		30:6:2002   16:33
	Modify Time:
	Original Author:	< johnson >
	More Author:	
	Abstract:			?
	
	Note:				

*********************************************************************/
#include "v3dxVector3.h"
#include "v3dxQuaternion.h"
#include "v3dxMatrix3.h"

#define new VNEW

const v3dxVector3 v3dxVector3::ZERO( 0, 0, 0 );
const v3dxVector3 v3dxVector3::UNIT_X( 1, 0, 0 );
const v3dxVector3 v3dxVector3::UNIT_Y( 0, 1, 0 );
const v3dxVector3 v3dxVector3::UNIT_Z( 0, 0, 1 );

const v3dxVector3 v3dxVector3::UNIT_MINUS_X( -1, 0, 0 );
const v3dxVector3 v3dxVector3::UNIT_MINUS_Y( 0, -1, 0 );
const v3dxVector3 v3dxVector3::UNIT_MINUS_Z( 0, 0, -1 );

const v3dxVector3 v3dxVector3::UNIT_SCALE(1, 1, 1);

