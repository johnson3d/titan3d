/********************************************************************
	V3D					A Powerful 3D Enjine
	File:				v3dxbox3.h
	Created Time:		30:6:2002   16:33
	Modify Time:
	Original Author:	< johnson >
	More Author:	
	Abstract:			?
	
	Note:				

*********************************************************************/


#ifndef __v3dxBox3__H__
#define __v3dxBox3__H__

#include "v3dxVector3.h"

#pragma pack(push,4)

class v3dxSphere;

enum v3dxBox3Coner
{
	BOX3_CORNER_xyz = 0,
	BOX3_CORNER_xyZ = 1,
	BOX3_CORNER_xYz = 2,
	BOX3_CORNER_xYZ = 3,
	BOX3_CORNER_Xyz = 4,
	BOX3_CORNER_XyZ = 5,
	BOX3_CORNER_XYz = 6,
	BOX3_CORNER_XYZ = 7,
	BOX3_CORNER_NUMBER = 8
};

enum v3dxBox3Side
{
	BOX3_SIDE_x = 0,				
	BOX3_SIDE_X = 1,
	BOX3_SIDE_y = 2,
	BOX3_SIDE_Y = 3,
	BOX3_SIDE_z = 4,
	BOX3_SIDE_Z = 5,
	BOX3_SIDE_NUMBER = 6,
};

struct v3dBox3_t
{
	v3dVector3_t minbox;
	v3dVector3_t maxbox;
};

class v3dxBox3
{
public:
	v3dxBox3 () :
    minbox ( 1000000.0f,
             1000000.0f,
			 1000000.0f),
    maxbox (-1000000.0f,
            -1000000.0f,
			-1000000.0f) {}

	v3dxBox3 (const v3dxVector3& v) : minbox (v.X,v.Y,v.Y), maxbox (v.X,v.Y,v.Z) { }

	v3dxBox3 (const v3dxVector3& v1, const v3dxVector3& v2) :
  			minbox (v1.X,v1.Y,v1.Z), maxbox (v2.X,v2.Y,v2.Z){ 
				if (IsEmpty()) 
					InitializeBox(); 
			}

	v3dxBox3 (float x1, float y1, float z1, float x2, float y2, float z2) :
			minbox (x1, y1, z1), maxbox (x2, y2, z2){ 
				if (IsEmpty()) 
					InitializeBox(); 
			}

	void Set (const v3dxVector3& bmin, const v3dxVector3& bmax){ 
		minbox = bmin;
		maxbox = bmax; 
	}

	void Set (float x1, float y1, float z1, float x2, float y2, float z2){
		if (x1>x2 || y1>y2 || z1>z2) 
			InitializeBox();
		else{
			minbox.X = x1; minbox.Y = y1; minbox.Z = z1;
			maxbox.X = x2; maxbox.Y = y2; maxbox.Z = z2;
		}
	}

	inline float GetBulk() const{
		return (maxbox.X - minbox.X)*(maxbox.Y - minbox.Y)*(maxbox.Z - minbox.Z);
	}
	inline float GetWidth() const{
		return (maxbox.X - minbox.X);
	}
	inline float GetLength() const{
		return (maxbox.Y - minbox.Y);
	}
	inline float GetHeight() const{
		return (maxbox.Z - minbox.Z);
	}
	inline v3dxVector3 GetExtend() const{
		v3dxVector3 ext;
		ext =(maxbox-minbox)*0.5f;
		return ext;
	}
	inline v3dxVector3 GetSize() const {
		v3dxVector3 ext;
		ext = (maxbox - minbox);
		return ext;
	}

	inline float MinX () const { 
		return minbox.X; 
	}
	inline float MinY () const { 
		return minbox.Y; 
	}
	inline float MinZ () const { 
		return minbox.Z; 
	}
	inline float MaxX () const { 
		return maxbox.X; 
	}
	inline float MaxY () const { 
		return maxbox.Y; 
	}
	inline float MaxZ () const { 
		return maxbox.Z; 
	}
	
	inline void SetMinX (float f) {
		minbox.X = f;
	}
	inline void SetMinY (float f) { 
		minbox.Y = f; 
	}
	inline void SetMinZ (float f) {
		minbox.Z = f; 
	}
	inline void SetMaxX (float f) { 
		maxbox.X = f;
	}
	inline void SetMaxY (float f) { 
		maxbox.Y = f; 
	}
	inline void SetMaxZ (float f) { 
		maxbox.Z = f; 
	}
	
	inline float Min (int idx) const{ 
		return idx == 1 ? minbox.Y : idx == 0 ? minbox.X : minbox.Z; 
	}
	inline float Max (int idx) const{ 
		return idx == 1 ? maxbox.Y : idx == 0 ? maxbox.X : maxbox.Z; 
	}
	const v3dxVector3& Min () const { 
		return minbox; 
	}
	const v3dxVector3& Max () const { 
		return maxbox; 
	}

	 v3dxVector3 GetCorner (int corner) const;

	inline v3dxVector3 GetCenter () const { 
		return (minbox+maxbox)/2; 
	}
	void SetCenter (const v3dxVector3& c);
	void SetSize (const v3dxVector3& s);
	
	bool In (float x, float y, float z) const;
	bool In (const v3dxVector3& v) const;
	inline bool In (const v3dVector3_t& v) const{
		return In( (const v3dxVector3&)v );
	}
	bool Overlap (const v3dxBox3& box) const;
	bool Contains (const v3dxBox3& box) const;
	bool IsEmpty () const;
	inline void InitializeBox (){
		minbox.X =  FLT_MAX;  minbox.Y = FLT_MAX;  minbox.Z = FLT_MAX;
		maxbox.X = -FLT_MAX;  maxbox.Y = -FLT_MAX;  maxbox.Z = -FLT_MAX;
	}
	inline void InitializeBox (float x,float y,float z){
		minbox.X = maxbox.X = x;
		minbox.Y = maxbox.Y = y;
		minbox.Z = maxbox.Z = z;
	}
	inline void InitializeBox (v3dxVector3 p){
		InitializeBox(p.X,p.Y,p.Z);
	}
	inline void MergeVertex (float x, float y, float z){
		if (x < minbox.X) minbox.X = x; if (x > maxbox.X) maxbox.X = x;
		if (y < minbox.Y) minbox.Y = y; if (y > maxbox.Y) maxbox.Y = y;
		if (z < minbox.Z) minbox.Z = z; if (z > maxbox.Z) maxbox.Z = z;
	}
	inline void MergeVertex (const v3dxVector3& v){ 
		MergeVertex(v.X,v.Y,v.Z); 
	}
	inline void Inflate (const v3dxVector3& v){ // added by Jones
		minbox -= v; maxbox += v;
	}

	vBOOL IsCrossByDatial(const v3dxVector3* pvPos,const v3dxVector3* pvDir) const;

	 bool AdjacentX (const v3dxBox3& other) const;
	 bool AdjacentY (const v3dxBox3& other) const;
	 bool AdjacentZ (const v3dxBox3& other) const;
	 int Adjacent (const v3dxBox3& other) const;

	vBOOL IsIntersectBox3( const v3dxBox3& box ) const{
		return !(minbox.X > box.Max().X
			|| minbox.Y > box.Max().Y
			|| minbox.Z > box.Max().Z
			|| maxbox.X < box.Min().X
			|| maxbox.Y < box.Min().Y
			|| maxbox.Z < box.Min().Z);
	}

	float GetSurface()
	{
		return GetWidth() * GetHeight() + GetWidth() * GetLength() + GetLength() * GetHeight();
	}

	 bool Between (const v3dxBox3& box1, const v3dxBox3& box2) const;
	 void ManhattanDistance (const v3dxBox3& other, v3dxVector3& dist) const;
	 float SquaredOriginDist() const;

	 bool intersect(const v3dxSphere &sphere) const;

	v3dxBox3& operator+= (const v3dxBox3& box);
	v3dxBox3& operator+= (const v3dxVector3& point);
	v3dxBox3& operator*= (const v3dxBox3& box);

	friend v3dxBox3 operator+ (const v3dxBox3& box1, const v3dxBox3& box2);
	friend v3dxBox3 operator+ (const v3dxBox3& box, const v3dxVector3& point);
	friend v3dxBox3 operator* (const v3dxBox3& box1, const v3dxBox3& box2);

	friend bool operator== (const v3dxBox3& box1, const v3dxBox3& box2);
	friend bool operator!= (const v3dxBox3& box1, const v3dxBox3& box2);
	friend bool operator< (const v3dxBox3& box1, const v3dxBox3& box2);
	friend bool operator> (const v3dxBox3& box1, const v3dxBox3& box2);
	friend bool operator< (const v3dxVector3& point, const v3dxBox3& box);
public:
	v3dxVector3				minbox;
	v3dxVector3				maxbox;
private:
};

class v3dxBoxEx3 : public v3dxBox3
{
public:
	void UpdateCorner(){
		for( int i=0 ; i<BOX3_CORNER_NUMBER ; i++ ){
			aCorner[i] = GetCorner(i);
		}
	}
	const v3dxVector3* GetDirectionCorner(int i) const{
		return &aCorner[i];
	}
public:
	v3dxVector3				aCorner[ BOX3_CORNER_NUMBER ];
};

#include "v3dxBox3.inl.h"

#pragma pack(pop)

#endif