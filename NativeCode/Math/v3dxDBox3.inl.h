// v3dxDBox3.inl
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

inline void v3dxDBox3::SetCenter (const v3dxDVector3& c)
{
  v3dxDVector3 move = c-GetCenter ();
  minbox = minbox + move;
  maxbox = maxbox + move;
}

inline void v3dxDBox3::SetSize (const v3dxDVector3& s)
{
  v3dxDVector3 center = GetCenter ();
  minbox = center - s*.5;
  maxbox = center + s*.5;
}

inline bool v3dxDBox3::In (double x, double y, double z) const
{
	if (x < minbox.x || x > maxbox.x) return false;
	if (y < minbox.y || y > maxbox.y) return false;
	if (z < minbox.z || z > maxbox.z) return false;
	return true;
}

inline bool v3dxDBox3::In (const v3dxDVector3& v) const
{
	return In (v.x, v.y, v.z);
}

inline bool v3dxDBox3::Overlap (const v3dxDBox3& box) const
{
	if (maxbox.x < box.minbox.x || minbox.x > box.maxbox.x) return false;
	if (maxbox.y < box.minbox.y || minbox.y > box.maxbox.y) return false;
	if (maxbox.z < box.minbox.z || minbox.z > box.maxbox.z) return false;
	return true;
}

inline bool v3dxDBox3::Contains (const v3dxDBox3& box) const
{
	return (box.minbox.x >= minbox.x && box.maxbox.x <= maxbox.x) &&
           (box.minbox.y >= minbox.y && box.maxbox.y <= maxbox.y) &&
           (box.minbox.z >= minbox.z && box.maxbox.z <= maxbox.z);
}

inline bool v3dxDBox3::IsEmpty () const
{
	if (minbox.x > maxbox.x) return true;
	if (minbox.y > maxbox.y) return true;
	if (minbox.z > maxbox.z) return true;
	return false;
}

inline vBOOL v3dxDBox3::IsCrossByDatial(const v3dxDVector3* pvPos,const v3dxDVector3* pvDir) const
{
/*	return D3DXBoxBoundProbe( (D3DXVECTOR3*)&minbox,
				(D3DXVECTOR3*)&maxbox,
				(D3DXVECTOR3*)pvPos,
				(D3DXVECTOR3*)pvDir);
*/
	return FALSE;
}

inline v3dxDBox3& v3dxDBox3::operator+= (const v3dxDBox3& box)
{
	if (box.minbox.x < minbox.x) minbox.x = box.minbox.x;
	if (box.minbox.y < minbox.y) minbox.y = box.minbox.y;
	if (box.minbox.z < minbox.z) minbox.z = box.minbox.z;
	if (box.maxbox.x > maxbox.x) maxbox.x = box.maxbox.x;
	if (box.maxbox.y > maxbox.y) maxbox.y = box.maxbox.y;
	if (box.maxbox.z > maxbox.z) maxbox.z = box.maxbox.z;
	return *this;
}

inline v3dxDBox3& v3dxDBox3::operator+= (const v3dxDVector3& point)
{
	if (point.x < minbox.x) minbox.x = point.x;
	if (point.x > maxbox.x) maxbox.x = point.x;
	if (point.y < minbox.y) minbox.y = point.y;
	if (point.y > maxbox.y) maxbox.y = point.y;
	if (point.z < minbox.z) minbox.z = point.z;
	if (point.z > maxbox.z) maxbox.z = point.z;
	return *this;
}

inline v3dxDBox3& v3dxDBox3::operator*= (const v3dxDBox3& box)
{
	if (box.minbox.x > minbox.x) minbox.x = box.minbox.x;
	if (box.minbox.y > minbox.y) minbox.y = box.minbox.y;
	if (box.minbox.z > minbox.z) minbox.z = box.minbox.z;
	if (box.maxbox.x < maxbox.x) maxbox.x = box.maxbox.x;
	if (box.maxbox.y < maxbox.y) maxbox.y = box.maxbox.y;
	if (box.maxbox.z < maxbox.z) maxbox.z = box.maxbox.z;
	return *this;
}

inline v3dxDBox3 operator+ (const v3dxDBox3& box1, const v3dxDBox3& box2)
{
	return v3dxDBox3(
  		TPL_HELP::vfxMIN(box1.minbox.x,box2.minbox.x),
		TPL_HELP::vfxMIN(box1.minbox.y,box2.minbox.y),
		TPL_HELP::vfxMIN(box1.minbox.z,box2.minbox.z),
		TPL_HELP::vfxMAX(box1.maxbox.x,box2.maxbox.x),
		TPL_HELP::vfxMAX(box1.maxbox.y,box2.maxbox.y),
		TPL_HELP::vfxMAX(box1.maxbox.z,box2.maxbox.z) );
}

inline v3dxDBox3 operator+ (const v3dxDBox3& box, const v3dxDVector3& point)
{
	return v3dxDBox3(
  		TPL_HELP::vfxMIN(box.minbox.x,point.x),
		TPL_HELP::vfxMIN(box.minbox.y,point.y),
		TPL_HELP::vfxMIN(box.minbox.z,point.z),
		TPL_HELP::vfxMAX(box.maxbox.x,point.x),
		TPL_HELP::vfxMAX(box.maxbox.y,point.y),
		TPL_HELP::vfxMAX(box.maxbox.z,point.z) );
}

inline v3dxDBox3 operator* (const v3dxDBox3& box1, const v3dxDBox3& box2)
{
	return v3dxDBox3(
  		TPL_HELP::vfxMAX(box1.minbox.x,box2.minbox.x),
		TPL_HELP::vfxMAX(box1.minbox.y,box2.minbox.y),
		TPL_HELP::vfxMAX(box1.minbox.z,box2.minbox.z),
		TPL_HELP::vfxMIN(box1.maxbox.x,box2.maxbox.x),
		TPL_HELP::vfxMIN(box1.maxbox.y,box2.maxbox.y),
		TPL_HELP::vfxMIN(box1.maxbox.z,box2.maxbox.z));
}

inline bool operator== (const v3dxDBox3& box1, const v3dxDBox3& box2)
{
	return ( (box1.minbox.x == box2.minbox.x)
  		&& (box1.minbox.y == box2.minbox.y)
  		&& (box1.minbox.z == box2.minbox.z)
		&& (box1.maxbox.x == box2.maxbox.x)
		&& (box1.maxbox.y == box2.maxbox.y)
		&& (box1.maxbox.z == box2.maxbox.z) );
}

inline bool operator!= (const v3dxDBox3& box1, const v3dxDBox3& box2)
{
	return ( (box1.minbox.x != box2.minbox.x)
  		|| (box1.minbox.y != box2.minbox.y)
  		|| (box1.minbox.z != box2.minbox.z)
		|| (box1.maxbox.x != box2.maxbox.x)
		|| (box1.maxbox.y != box2.maxbox.y)
		|| (box1.maxbox.z != box2.maxbox.z) );
}

inline bool operator< (const v3dxDBox3& box1, const v3dxDBox3& box2)
{
	return ( (box1.minbox.x >= box2.minbox.x)
  		&& (box1.minbox.y >= box2.minbox.y)
  		&& (box1.minbox.z >= box2.minbox.z)
		&& (box1.maxbox.x <= box2.maxbox.x)
		&& (box1.maxbox.y <= box2.maxbox.y)
		&& (box1.maxbox.z <= box2.maxbox.z) );
}

inline bool operator> (const v3dxDBox3& box1, const v3dxDBox3& box2)
{
	return ( (box2.minbox.x >= box1.minbox.x)
  		&& (box2.minbox.y >= box1.minbox.y)
  		&& (box2.minbox.z >= box1.minbox.z)
		&& (box2.maxbox.x <= box1.maxbox.x)
		&& (box2.maxbox.y <= box1.maxbox.y)
		&& (box2.maxbox.z <= box1.maxbox.z) );
}

inline bool operator< (const v3dxDVector3& point, const v3dxDBox3& box)
{
	return ( (point.x >= box.minbox.x)
  		&& (point.x <= box.maxbox.x)
		&& (point.y >= box.minbox.y)
		&& (point.y <= box.maxbox.y)
		&& (point.z >= box.minbox.z)
		&& (point.z <= box.maxbox.z) );
}

