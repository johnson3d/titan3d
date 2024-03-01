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


#pragma once

#include "v3dxDVector3.h"

#pragma pack(push,4)

struct v3dDBox3_t
{
	v3dDVector3_t minbox;
	v3dDVector3_t maxbox;
};

class v3dxDBox3
{
public:
	v3dxDBox3() :
		minbox(1000000.0f,
			1000000.0f,
			1000000.0f),
		maxbox(-1000000.0f,
			-1000000.0f,
			-1000000.0f) {}

	v3dxDBox3(const v3dxDVector3& v) : minbox(v.X, v.Y, v.Y), maxbox(v.X, v.Y, v.Z) { }

	v3dxDBox3(const v3dxDVector3& v1, const v3dxDVector3& v2) :
		minbox(v1.X, v1.Y, v1.Z), maxbox(v2.X, v2.Y, v2.Z) {
		if (IsEmpty())
			InitializeBox();
	}

	v3dxDBox3(double x1, double y1, double z1, double x2, double y2, double z2) :
		minbox(x1, y1, z1), maxbox(x2, y2, z2) {
		if (IsEmpty())
			InitializeBox();
	}

	void Set(const v3dxDVector3& bmin, const v3dxDVector3& bmax) {
		minbox = bmin;
		maxbox = bmax;
	}

	void Set(double x1, double y1, double z1, double x2, double y2, double z2) {
		if (x1 > x2 || y1 > y2 || z1 > z2)
			InitializeBox();
		else {
			minbox.X = x1; minbox.Y = y1; minbox.Z = z1;
			maxbox.X = x2; maxbox.Y = y2; maxbox.Z = z2;
		}
	}

	inline double GetBulk() const {
		return (maxbox.X - minbox.X) * (maxbox.Y - minbox.Y) * (maxbox.Z - minbox.Z);
	}
	inline double GetWidth() const {
		return (maxbox.X - minbox.X);
	}
	inline double GetLength() const {
		return (maxbox.Y - minbox.Y);
	}
	inline double GetHeight() const {
		return (maxbox.Z - minbox.Z);
	}
	inline v3dxDVector3 GetExtend() const {
		v3dxDVector3 ext;
		ext = (maxbox - minbox) * 0.5;
		return ext;
	}

	inline double MinX() const {
		return minbox.X;
	}
	inline double MinY() const {
		return minbox.Y;
	}
	inline double MinZ() const {
		return minbox.Z;
	}
	inline double MaxX() const {
		return maxbox.X;
	}
	inline double MaxY() const {
		return maxbox.Y;
	}
	inline double MaxZ() const {
		return maxbox.Z;
	}

	inline void SetMinX(double f) {
		minbox.X = f;
	}
	inline void SetMinY(double f) {
		minbox.Y = f;
	}
	inline void SetMinZ(double f) {
		minbox.Z = f;
	}
	inline void SetMaxX(double f) {
		maxbox.X = f;
	}
	inline void SetMaxY(double f) {
		maxbox.Y = f;
	}
	inline void SetMaxZ(double f) {
		maxbox.Z = f;
	}

	inline double Min(int idx) const {
		return idx == 1 ? minbox.Y : idx == 0 ? minbox.X : minbox.Z;
	}
	inline double Max(int idx) const {
		return idx == 1 ? maxbox.Y : idx == 0 ? maxbox.X : maxbox.Z;
	}
	const v3dxDVector3& Min() const {
		return minbox;
	}
	const v3dxDVector3& Max() const {
		return maxbox;
	}

	v3dxDVector3 GetCorner(int corner) const;

	inline v3dxDVector3 GetCenter() const {
		return (minbox + maxbox) / 2;
	}
	void SetCenter(const v3dxDVector3& c);
	void SetSize(const v3dxDVector3& s);

	bool In(double x, double y, double z) const;
	bool In(const v3dxDVector3& v) const;
	inline bool In(const v3dDVector3_t& v) const {
		return In((const v3dxDVector3&)v);
	}
	bool Overlap(const v3dxDBox3& box) const;
	bool Contains(const v3dxDBox3& box) const;
	bool IsEmpty() const;
	inline void InitializeBox() {
		minbox.X = FLT_MAX;  minbox.Y = FLT_MAX;  minbox.Z = FLT_MAX;
		maxbox.X = -FLT_MAX;  maxbox.Y = -FLT_MAX;  maxbox.Z = -FLT_MAX;
	}
	inline void InitializeBox(double x, double y, double z) {
		minbox.X = maxbox.X = x;
		minbox.Y = maxbox.Y = y;
		minbox.Z = maxbox.Z = z;
	}
	inline void InitializeBox(v3dxDVector3 p) {
		InitializeBox(p.X, p.Y, p.Z);
	}
	inline void OptimalVertex(double x, double y, double z) {
		if (x < minbox.X) minbox.X = x; if (x > maxbox.X) maxbox.X = x;
		if (y < minbox.Y) minbox.Y = y; if (y > maxbox.Y) maxbox.Y = y;
		if (z < minbox.Z) minbox.Z = z; if (z > maxbox.Z) maxbox.Z = z;
	}
	inline void OptimalVertex(const v3dxDVector3& v) {
		OptimalVertex(v.X, v.Y, v.Z);
	}
	inline void Inflate(const v3dxDVector3& v) { // added by Jones
		minbox = minbox - v;
		maxbox = maxbox + v;
	}

	vBOOL IsCrossByDatial(const v3dxDVector3* pvPos, const v3dxDVector3* pvDir) const;

	bool AdjacentX(const v3dxDBox3& other) const;
	bool AdjacentY(const v3dxDBox3& other) const;
	bool AdjacentZ(const v3dxDBox3& other) const;
	int Adjacent(const v3dxDBox3& other) const;

	vBOOL IsIntersectBox3(const v3dxDBox3& box) const {
		return !(minbox.X > box.Max().X
			|| minbox.Y > box.Max().Y
			|| minbox.Z > box.Max().Z
			|| maxbox.X < box.Min().X
			|| maxbox.Y < box.Min().Y
			|| maxbox.Z < box.Min().Z);
	}

	bool Between(const v3dxDBox3& box1, const v3dxDBox3& box2) const;
	void ManhattanDistance(const v3dxDBox3& other, v3dxDVector3& dist) const;
	double SquaredOriginDist() const;

	//bool intersect(const v3dxSphere& sphere) const;

	v3dxDBox3& operator+= (const v3dxDBox3& box);
	v3dxDBox3& operator+= (const v3dxDVector3& point);
	v3dxDBox3& operator*= (const v3dxDBox3& box);

	friend v3dxDBox3 operator+ (const v3dxDBox3& box1, const v3dxDBox3& box2);
	friend v3dxDBox3 operator+ (const v3dxDBox3& box, const v3dxDVector3& point);
	friend v3dxDBox3 operator* (const v3dxDBox3& box1, const v3dxDBox3& box2);

	friend bool operator== (const v3dxDBox3& box1, const v3dxDBox3& box2);
	friend bool operator!= (const v3dxDBox3& box1, const v3dxDBox3& box2);
	friend bool operator< (const v3dxDBox3& box1, const v3dxDBox3& box2);
	friend bool operator> (const v3dxDBox3& box1, const v3dxDBox3& box2);
	friend bool operator< (const v3dxDVector3& point, const v3dxDBox3& box);
public:
	v3dxDVector3				minbox;
	v3dxDVector3				maxbox;
private:
};

#pragma pack(pop)

#include "v3dxDBox3.inl.h"
