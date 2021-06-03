/********************************************************************
	V3D					A Powerful 3D Enjine
	File:				v3dxmatrix4.cpp
	Created Time:		30:6:2002   16:35
	Modify Time:
	Original Author:	< johnson >
	More Author:	
	Abstract:			?
	
	Note:				

*********************************************************************/
#include "v3dxMatrix4.h"

#define new VNEW

const v3dxMatrix4 v3dxMatrix4::ZERO(
	0, 0, 0, 0,
	0, 0, 0, 0,
	0, 0, 0, 0,
	0, 0, 0, 0 );

const v3dxMatrix4 v3dxMatrix4::IDENTITY(
	1, 0, 0, 0,
	0, 1, 0, 0,
	0, 0, 1, 0,
	0, 0, 0, 1 );


