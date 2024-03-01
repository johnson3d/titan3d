// v3dxBox3.inl
// 
// Victory 3D Code
// Box3 inline funtion
//
// Author : johnson
// Modifier : lanzhengpeng
// More author :
// Create time :
// Modify time : 2002-6-16 
//-----------------------------------------------------------------------------

inline void v3dxBox3::SetCenter (const v3dxVector3& c)
{
  v3dxVector3 move = c-GetCenter ();
  minbox += move;
  maxbox += move;
}

inline void v3dxBox3::SetSize (const v3dxVector3& s)
{
  v3dxVector3 center = GetCenter ();
  minbox = center - s*.5;
  maxbox = center + s*.5;
}

inline bool v3dxBox3::In (float x, float y, float z) const
{
	if (x < minbox.X || x > maxbox.X) return false;
	if (y < minbox.Y || y > maxbox.Y) return false;
	if (z < minbox.Z || z > maxbox.Z) return false;
	return true;
}

inline bool v3dxBox3::In (const v3dxVector3& v) const
{
	return In (v.X, v.Y, v.Z);
}

inline bool v3dxBox3::Overlap (const v3dxBox3& box) const
{
	if (maxbox.X < box.minbox.X || minbox.X > box.maxbox.X) return false;
	if (maxbox.Y < box.minbox.Y || minbox.Y > box.maxbox.Y) return false;
	if (maxbox.Z < box.minbox.Z || minbox.Z > box.maxbox.Z) return false;
	return true;
}

inline bool v3dxBox3::Contains (const v3dxBox3& box) const
{
	return (box.minbox.X >= minbox.X && box.maxbox.X <= maxbox.X) &&
           (box.minbox.Y >= minbox.Y && box.maxbox.Y <= maxbox.Y) &&
           (box.minbox.Z >= minbox.Z && box.maxbox.Z <= maxbox.Z);
}

inline bool v3dxBox3::IsEmpty () const
{
	if (minbox.X > maxbox.X) return true;
	if (minbox.Y > maxbox.Y) return true;
	if (minbox.Z > maxbox.Z) return true;
	return false;
}

inline vBOOL v3dxBox3::IsCrossByDatial(const v3dxVector3* pvPos,const v3dxVector3* pvDir) const
{
/*	return D3DXBoxBoundProbe( (D3DXVECTOR3*)&minbox,
				(D3DXVECTOR3*)&maxbox,
				(D3DXVECTOR3*)pvPos,
				(D3DXVECTOR3*)pvDir);
*/
	return FALSE;
}

inline v3dxBox3& v3dxBox3::operator+= (const v3dxBox3& box)
{
	if (box.minbox.X < minbox.X) minbox.X = box.minbox.X;
	if (box.minbox.Y < minbox.Y) minbox.Y = box.minbox.Y;
	if (box.minbox.Z < minbox.Z) minbox.Z = box.minbox.Z;
	if (box.maxbox.X > maxbox.X) maxbox.X = box.maxbox.X;
	if (box.maxbox.Y > maxbox.Y) maxbox.Y = box.maxbox.Y;
	if (box.maxbox.Z > maxbox.Z) maxbox.Z = box.maxbox.Z;
	return *this;
}

inline v3dxBox3& v3dxBox3::operator+= (const v3dxVector3& point)
{
	if (point.X < minbox.X) minbox.X = point.X;
	if (point.X > maxbox.X) maxbox.X = point.X;
	if (point.Y < minbox.Y) minbox.Y = point.Y;
	if (point.Y > maxbox.Y) maxbox.Y = point.Y;
	if (point.Z < minbox.Z) minbox.Z = point.Z;
	if (point.Z > maxbox.Z) maxbox.Z = point.Z;
	return *this;
}

inline v3dxBox3& v3dxBox3::operator*= (const v3dxBox3& box)
{
	if (box.minbox.X > minbox.X) minbox.X = box.minbox.X;
	if (box.minbox.Y > minbox.Y) minbox.Y = box.minbox.Y;
	if (box.minbox.Z > minbox.Z) minbox.Z = box.minbox.Z;
	if (box.maxbox.X < maxbox.X) maxbox.X = box.maxbox.X;
	if (box.maxbox.Y < maxbox.Y) maxbox.Y = box.maxbox.Y;
	if (box.maxbox.Z < maxbox.Z) maxbox.Z = box.maxbox.Z;
	return *this;
}

inline v3dxBox3 operator+ (const v3dxBox3& box1, const v3dxBox3& box2)
{
	return v3dxBox3(
  		TPL_HELP::vfxMIN(box1.minbox.X,box2.minbox.X),
		TPL_HELP::vfxMIN(box1.minbox.Y,box2.minbox.Y),
		TPL_HELP::vfxMIN(box1.minbox.Z,box2.minbox.Z),
		TPL_HELP::vfxMAX(box1.maxbox.X,box2.maxbox.X),
		TPL_HELP::vfxMAX(box1.maxbox.Y,box2.maxbox.Y),
		TPL_HELP::vfxMAX(box1.maxbox.Z,box2.maxbox.Z) );
}

inline v3dxBox3 operator+ (const v3dxBox3& box, const v3dxVector3& point)
{
	return v3dxBox3(
  		TPL_HELP::vfxMIN(box.minbox.X,point.X),
		TPL_HELP::vfxMIN(box.minbox.Y,point.Y),
		TPL_HELP::vfxMIN(box.minbox.Z,point.Z),
		TPL_HELP::vfxMAX(box.maxbox.X,point.X),
		TPL_HELP::vfxMAX(box.maxbox.Y,point.Y),
		TPL_HELP::vfxMAX(box.maxbox.Z,point.Z) );
}

inline v3dxBox3 operator* (const v3dxBox3& box1, const v3dxBox3& box2)
{
	return v3dxBox3(
  		TPL_HELP::vfxMAX(box1.minbox.X,box2.minbox.X),
		TPL_HELP::vfxMAX(box1.minbox.Y,box2.minbox.Y),
		TPL_HELP::vfxMAX(box1.minbox.Z,box2.minbox.Z),
		TPL_HELP::vfxMIN(box1.maxbox.X,box2.maxbox.X),
		TPL_HELP::vfxMIN(box1.maxbox.Y,box2.maxbox.Y),
		TPL_HELP::vfxMIN(box1.maxbox.Z,box2.maxbox.Z));
}

inline bool operator== (const v3dxBox3& box1, const v3dxBox3& box2)
{
	return ( (box1.minbox.X == box2.minbox.X)
  		&& (box1.minbox.Y == box2.minbox.Y)
  		&& (box1.minbox.Z == box2.minbox.Z)
		&& (box1.maxbox.X == box2.maxbox.X)
		&& (box1.maxbox.Y == box2.maxbox.Y)
		&& (box1.maxbox.Z == box2.maxbox.Z) );
}

inline bool operator!= (const v3dxBox3& box1, const v3dxBox3& box2)
{
	return ( (box1.minbox.X != box2.minbox.X)
  		|| (box1.minbox.Y != box2.minbox.Y)
  		|| (box1.minbox.Z != box2.minbox.Z)
		|| (box1.maxbox.X != box2.maxbox.X)
		|| (box1.maxbox.Y != box2.maxbox.Y)
		|| (box1.maxbox.Z != box2.maxbox.Z) );
}

inline bool operator< (const v3dxBox3& box1, const v3dxBox3& box2)
{
	return ( (box1.minbox.X >= box2.minbox.X)
  		&& (box1.minbox.Y >= box2.minbox.Y)
  		&& (box1.minbox.Z >= box2.minbox.Z)
		&& (box1.maxbox.X <= box2.maxbox.X)
		&& (box1.maxbox.Y <= box2.maxbox.Y)
		&& (box1.maxbox.Z <= box2.maxbox.Z) );
}

inline bool operator> (const v3dxBox3& box1, const v3dxBox3& box2)
{
	return ( (box2.minbox.X >= box1.minbox.X)
  		&& (box2.minbox.Y >= box1.minbox.Y)
  		&& (box2.minbox.Z >= box1.minbox.Z)
		&& (box2.maxbox.X <= box1.maxbox.X)
		&& (box2.maxbox.Y <= box1.maxbox.Y)
		&& (box2.maxbox.Z <= box1.maxbox.Z) );
}

inline bool operator< (const v3dxVector3& point, const v3dxBox3& box)
{
	return ( (point.X >= box.minbox.X)
  		&& (point.X <= box.maxbox.X)
		&& (point.Y >= box.minbox.Y)
		&& (point.Y <= box.maxbox.Y)
		&& (point.Z >= box.minbox.Z)
		&& (point.Z <= box.maxbox.Z) );
}

