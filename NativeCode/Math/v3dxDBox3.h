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

	v3dxDBox3(const v3dxDVector3& v) : minbox(v.x, v.y, v.y), maxbox(v.x, v.y, v.z) { }

	v3dxDBox3(const v3dxDVector3& v1, const v3dxDVector3& v2) :
		minbox(v1.x, v1.y, v1.z), maxbox(v2.x, v2.y, v2.z) {
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
			minbox.x = x1; minbox.y = y1; minbox.z = z1;
			maxbox.x = x2; maxbox.y = y2; maxbox.z = z2;
		}
	}

	inline double GetBulk() const {
		return (maxbox.x - minbox.x) * (maxbox.y - minbox.y) * (maxbox.z - minbox.z);
	}
	inline double GetWidth() const {
		return (maxbox.x - minbox.x);
	}
	inline double GetLength() const {
		return (maxbox.y - minbox.y);
	}
	inline double GetHeight() const {
		return (maxbox.z - minbox.z);
	}
	inline v3dxDVector3 GetExtend() const {
		v3dxDVector3 ext;
		ext = (maxbox - minbox) * 0.5;
		return ext;
	}

	inline double MinX() const {
		return minbox.x;
	}
	inline double MinY() const {
		return minbox.y;
	}
	inline double MinZ() const {
		return minbox.z;
	}
	inline double MaxX() const {
		return maxbox.x;
	}
	inline double MaxY() const {
		return maxbox.y;
	}
	inline double MaxZ() const {
		return maxbox.z;
	}

	inline void SetMinX(double f) {
		minbox.x = f;
	}
	inline void SetMinY(double f) {
		minbox.y = f;
	}
	inline void SetMinZ(double f) {
		minbox.z = f;
	}
	inline void SetMaxX(double f) {
		maxbox.x = f;
	}
	inline void SetMaxY(double f) {
		maxbox.y = f;
	}
	inline void SetMaxZ(double f) {
		maxbox.z = f;
	}

	inline double Min(int idx) const {
		return idx == 1 ? minbox.y : idx == 0 ? minbox.x : minbox.z;
	}
	inline double Max(int idx) const {
		return idx == 1 ? maxbox.y : idx == 0 ? maxbox.x : maxbox.z;
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
		minbox.x = FLT_MAX;  minbox.y = FLT_MAX;  minbox.z = FLT_MAX;
		maxbox.x = -FLT_MAX;  maxbox.y = -FLT_MAX;  maxbox.z = -FLT_MAX;
	}
	inline void InitializeBox(double x, double y, double z) {
		minbox.x = maxbox.x = x;
		minbox.y = maxbox.y = y;
		minbox.z = maxbox.z = z;
	}
	inline void InitializeBox(v3dxDVector3 p) {
		InitializeBox(p.x, p.y, p.z);
	}
	inline void OptimalVertex(double x, double y, double z) {
		if (x < minbox.x) minbox.x = x; if (x > maxbox.x) maxbox.x = x;
		if (y < minbox.y) minbox.y = y; if (y > maxbox.y) maxbox.y = y;
		if (z < minbox.z) minbox.z = z; if (z > maxbox.z) maxbox.z = z;
	}
	inline void OptimalVertex(const v3dxDVector3& v) {
		OptimalVertex(v.x, v.y, v.z);
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
		return !(minbox.x > box.Max().x
			|| minbox.y > box.Max().y
			|| minbox.z > box.Max().z
			|| maxbox.x < box.Min().x
			|| maxbox.y < box.Min().y
			|| maxbox.z < box.Min().z);
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
