#include "v3dxRect.h"
#include<algorithm>
#undef max
#undef min

#define new VNEW

inline v3dxRect::v3dxRect() {}
inline v3dxRect::v3dxRect(int ix, int iy, int iw, int ih) : x(ix), y(iy), width(iw), height(ih) {}
inline v3dxRect::v3dxRect(const v3dxRect &rect) : x(rect.x), y(rect.y), width(rect.width), height(rect.height) {}
inline v3dxRect::~v3dxRect() {}

// add need function below
inline void v3dxRect::clear() {
	x = INT_MAX;
	y = INT_MAX;
	width = INT_MIN;
	height = INT_MIN;
}

inline bool v3dxRect::operator==(const v3dxRect &other) const {
	return (x==other.x) && (y==other.y) && (width==other.width) && (height==other.height);
}

inline bool v3dxRect::contains(int ix, int iy) const {
	return ix >= x && ix < x+width && iy >= y && iy < y+height;
}

inline bool v3dxRect::contains(const v3dxPoint &point) const {
	return contains(point.x, point.y);
}

inline bool v3dxRect::contains(const v3dxRect &r) const {
	if (r.isEmpty())
		return true;

	if (isEmpty())
		return false;

	return x <= r.x && y <= r.y && xMax() >= r.xMax() && yMax() >= r.yMax();
}

inline bool v3dxRect::intersects(const v3dxRect &r) {
	if (x > r.xMax())
		return false;
	if (y > r.yMax())
		return false;
	if (r.x > xMax())
		return false;
	if (r.y > yMax())
		return false;
	return true;
}

inline v3dxPoint v3dxRect::getCenter() const { return v3dxPoint(x + width / 2, y + height / 2); }
inline v3dxPoint v3dxRect::getMins() const { return v3dxPoint(x, y); }
inline v3dxPoint v3dxRect::getMaxs() const { return v3dxPoint(x+width, y+height); }
inline int v3dxRect::xMin() const { return x; }
inline int v3dxRect::yMin() const { return y; } // bottom
inline int v3dxRect::xMax() const { return x + width; }
inline int v3dxRect::yMax() const { return y + height; }

template <class Q>
v3dxRect &v3dxRect::operator/=(Q scale) {
	x /= scale;
	y /= scale;
	width /= scale;
	height /= scale;

	return *this;
}

template <class Q>
v3dxRect v3dxRect::operator/(Q scale) const {
	return v3dxRect(x/scale, y/scale, width/scale, height/scale);
}

template <class Q>
v3dxRect &v3dxRect::operator*=(Q scale) {
	x *= scale;
	y *= scale;
	width *= scale;
	height *= scale;

	return *this;
}

template <class Q>
v3dxRect v3dxRect::operator*(Q scale) const {
	return v3dxRect(x*scale, y*scale, width*scale, height*scale);
}

// if two rect don't intersected, returned rect have zero or negative width
// and height, check isEmpty()
// self rect and other rect must have positive width and height to do this function
inline v3dxRect v3dxRect::intersect(const v3dxRect &other) const {
	v3dxRect r(0, 0, 0, 0);

	r.x = std::max(x, other.x);
	r.y = std::max(y, other.y);
	r.width = std::min(xMax(), other.xMax()) - r.x;
	r.height = std::min(yMax(), other.yMax()) - r.y;

	return r;
}

// lick intersect, but returned rect is self offset base, for texture clipping
inline v3dxRect v3dxRect::intersectToLocal(const v3dxRect &other) const {
	return intersect(other) - getMins();
}

// return a new rect contain both two rect
inline v3dxRect v3dxRect::unite(const v3dxRect &other) const {
	if (isEmpty())
		return other;

	v3dxRect r;

	r.x = std::min(xMin(), other.xMin());
	r.y = std::min(yMin(), other.yMin());
	r.width = std::max(xMax(), other.xMax());
	r.height = std::max(yMax(), other.yMax());

	r.width -= r.x;
	r.height -= r.y;

	return r;
}

// just do intersect
inline v3dxRect v3dxRect::operator&(const v3dxRect &other) const {
	return intersect(other);
}

inline v3dxRect &v3dxRect::operator&=(const v3dxRect &other) {
	*this = *this & other;

	return *this;
}

inline v3dxRect v3dxRect::operator|(const v3dxRect &other) const {
	return unite(other);
}

inline v3dxRect &v3dxRect::operator|=(const v3dxRect &other) {
	*this = *this | other;

	return *this;
}

// just do offset, no resize
inline v3dxRect v3dxRect::operator-(const v3dxPoint &point) const {
	return v3dxRect(x - point.x, y - point.y, width, height);
}

inline v3dxRect &v3dxRect::operator-=(const v3dxPoint &point) {
	x -= point.x;
	y -= point.y;

	return *this;
}

// just do offset, no resize
inline v3dxRect v3dxRect::operator+(const v3dxPoint &point) const {
	return v3dxRect(x + point.x, y + point.y, width, height);
}

inline v3dxRect &v3dxRect::operator+=(const v3dxPoint &point) {
	x += point.x;
	y += point.y;
	return *this;
}

inline bool v3dxRect::isEmpty() const {
	return width <= 0 || height <= 0;
}


inline void v3dxRect::deflate(int m_x, int m_y) {
	x += m_x;
	y += m_y;
	width -= 2 * m_x;
	height -= 2 * m_y;
}

inline void v3dxRect::deflate(const v3dxPoint &point) {
	deflate(point.x, point.y);
}

inline void v3dxRect::inflate(int m_x, int m_y) {
	deflate(-m_x, -m_y);
}

inline void v3dxRect::inflate(const v3dxPoint &point) {
	inflate(point.x, point.y);
}

inline void  v3dxRect::offset(int x) {
	offset(x, x);
}
inline void  v3dxRect::offset(int x, int y) {
	this->x += x;
	this->y += y;
}
inline void  v3dxRect::offset(const v3dxPoint &point) {
	offset(point.x, point.y);
}

//inline Vector4 Rect::toVector4() const {
//	Vector4 v;
//	v.x = x;
//	v.y = y;
//	v.z = width;
//	v.w = height;
//	return v;
//}
