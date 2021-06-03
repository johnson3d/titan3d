#ifndef __V3DXSIZE__H__
#define __V3DXSIZE__H__

#include "vfxGeomTypes.h"

#pragma pack(push,4)

struct  v3dxSize {
	v3dxSize() {}
	v3dxSize(int w, int h) : width(w), height(h) {}

	bool isZero() const { return width == 0 && height == 0; }
	void set(int w, int h) { width = w; height = h; }

	v3dxSize &operator+=(const v3dxSize &rhs) { width += rhs.width; height += rhs.height; return *this; }
	v3dxSize &operator-=(const v3dxSize &rhs) { width -= rhs.width; height -= rhs.height; return *this; }
	v3dxSize &operator*=(float c) { width = (int)(width*c); height = (int)(height*c); return *this; }
	v3dxSize &operator/=(float c) { return *this *= (1.0f/c); }

	friend inline bool operator==(const v3dxSize &, const v3dxSize &);
	friend inline bool operator!=(const v3dxSize &, const v3dxSize &);
	friend inline bool operator<=(const v3dxSize &, const v3dxSize &);
	friend inline const v3dxSize operator+(const v3dxSize &, const v3dxSize &);
	friend inline const v3dxSize operator-(const v3dxSize &, const v3dxSize &);
	friend inline const v3dxSize operator*(const v3dxSize &, float);
	friend inline const v3dxSize operator*(float, const v3dxSize &);
	friend inline const v3dxSize operator/(const v3dxSize &, float);

	int width, height;
};

inline bool operator==(const v3dxSize &a, const v3dxSize &b) { return a.width == b.width && a.height == b.height; }
inline bool operator!=(const v3dxSize &a, const v3dxSize &b) { return !(a == b); }
inline bool operator<=(const v3dxSize &a, const v3dxSize &b) { return a.width <= b.width && a.height <= b.height; }
inline const v3dxSize operator+(const v3dxSize &a, const v3dxSize &b) { return v3dxSize(a.width+b.width, a.height+b.height); }
inline const v3dxSize operator-(const v3dxSize &a, const v3dxSize &b) { return v3dxSize(a.width-b.width, a.height-b.height); }
inline const v3dxSize operator*(const v3dxSize &a, float f) { return v3dxSize((int)(a.width * f), (int)(a.height * f)); }
inline const v3dxSize operator*(float f, const v3dxSize &b) { return b * f; }
inline const v3dxSize operator/(const v3dxSize &a, float f) { return a * (1.0f / f); }


struct v3dxSizeF {
	v3dxSizeF();
	v3dxSizeF(float w, float h) : width(w), height(h) {}
	float width, height;
};

#pragma pack(pop)

#endif // end guardian

