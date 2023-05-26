#pragma once
#include "../../Base/BaseHead.h"
#include "../../Math/v3dxMath.h"

namespace TTStrip
{
	template<typename T>
	struct Point3
	{
		T x, y, z;

		Point3()
			:x(0), y(0), z(0)
		{}

		Point3(T _x, T _y, T _z)
			:x(_x), y(_y), z(_z)
		{}

		T& operator[](int i)
		{
			return *((&x) + i);
		}

		Point3 operator+(const Point3& other) const
		{
			return Point3{ x + other.x, y + other.y, z + other.z };
		}

		Point3 operator-(const Point3& other) const
		{
			return Point3{ x - other.x, y - other.y, z - other.z };
		}
		Point3 operator/(T v) const
		{
			return Point3{ x / v, y / v, z / v };
		}
		Point3 operator*(T v) const
		{
			return Point3{ x * v, y * v, z * v };
		}

		Point3& operator+=(const Point3& other)
		{
			*this = *this + other;

			return *this;
		}

		Point3& operator-=(const Point3& other)
		{
			*this = *this - other;

			return *this;
		}
		Point3 Cross(const Point3& other) const
		{
			return Point3(y * other.z - z * other.y, z * other.x - x * other.z, x * other.y - y * other.x);
		}

		T Dot(const Point3 other) const
		{
			return x * other.x + y * other.y + z * other.z;
		}

		T Length() const;

		Point3 Normalize() const
		{
			T lengh = Length();
			if (lengh != 0)
				return *this / Length();
			else
				return *this;
		}

		Point3<T>& operator = (const Point3<float>& src)
		{
			x = (T)src.x;
			y = (T)src.y;
			z = (T)src.z;
			return *this;
		}
		Point3<T>& operator = (const Point3<double>& src)
		{
			x = (T)src.x;
			y = (T)src.y;
			z = (T)src.z;
			return *this;
		}
	};

	using Float3 = Point3<float>;
	using Double3 = Point3<double>;

	template<>
	inline float Point3<float>::Length() const
	{
		return sqrtf(x * x + y * y + z * z);
	}

	template<>
	inline double Point3<double>::Length() const
	{
		return sqrt(x * x + y * y + z * z);
	}

	struct FHalfEdge
	{
		FHalfEdge* GetReverseEdge() const { return ReverseEdge; }

		UINT Face = UINT_MAX;
		// Face that shares the same edge, which means face that have the reverse halfedge.
		UINT NeighborFace = UINT_MAX;
		// 2 vertices on either end of edge
		UINT StartVertex = UINT_MAX;
		UINT EndVertex = UINT_MAX;

		// ReverseEdge is dual edge of the neighbor face sharing the same edge.
		FHalfEdge* ReverseEdge = nullptr;
		// The next edge following CCW in the same face.
		FHalfEdge* NextEdgeInFace = nullptr;
		// edges share the same end vertex. 
		FHalfEdge* Next = nullptr;
	};

	struct FClusterInfo
	{
		int ClusterId = -1;
		UINT LocalFaceIndex = UINT_MAX;
	};

	class FUseFlag
	{
	public:
		FUseFlag()
			: Mask(0)
		{}

		bool GetUsed() const { return Mask == 1; }
		void SetUsed() { Mask = 1; }
		void ResetUsed() { Mask = 0; }
	private:
		char Mask;
	};

	struct FFace
	{
		UINT VertIdx[3];
		Float3 Normal;
		// Cache 3 edge for fast access.
		FHalfEdge* Edges[3];
	};

	inline FHalfEdge* FindReverseEdge(const FHalfEdge& Edge, const std::vector<FHalfEdge*>& AllEdges)
	{
		FHalfEdge* OtherEdge = AllEdges[Edge.EndVertex];
		while (OtherEdge != nullptr)
		{
			if (OtherEdge->EndVertex == Edge.StartVertex)
				return OtherEdge;
			OtherEdge = OtherEdge->Next;
		}
		return nullptr;
	}


	////////////////////////////////////////////////////////////////////////////////////////////////////
	// Add half edge for the face.
	// All the faces share the same v0 will be put into a linked list of FHalfEdge.
	////////////////////////////////////////////////////////////////////////////////////////////////////
	inline FHalfEdge* AddEdge(UINT v0, UINT v1, UINT face, std::vector<FHalfEdge*>& AllEdges)
	{
		FHalfEdge* MatchingEdge = AllEdges[v0];// FindEdge(v0, v1);
		FHalfEdge* NewEdge = new FHalfEdge();

		NewEdge->Face = face;
		NewEdge->StartVertex = v0;
		NewEdge->EndVertex = v1;
		NewEdge->Next = MatchingEdge;

		AllEdges[v0] = NewEdge;

		// Add ReverseEdge and Neighbor Face for edges. Cache these for fast access in stripification.
		FHalfEdge* ReverseEdge = FindReverseEdge(*NewEdge, AllEdges);
		if (ReverseEdge != nullptr)
		{
			NewEdge->NeighborFace = ReverseEdge->Face;
			NewEdge->ReverseEdge = ReverseEdge;

			ReverseEdge->NeighborFace = NewEdge->Face;
			ReverseEdge->ReverseEdge = NewEdge;
		}
		return NewEdge;
	}

	inline FHalfEdge* GetNextEdgeInFace(const FHalfEdge* Edge)
	{
		return Edge->NextEdgeInFace;
	}

	inline UINT FindFace(UINT v0, UINT v1, const std::vector<FHalfEdge*>& AllEdges)
	{
		const FHalfEdge* e = AllEdges[v0];
		while (e != nullptr)
		{
			if (e->EndVertex == v1)
				return e->Face;
			e = e->Next;
		}
		return UINT_MAX;
	}

	inline void Swap(UINT& a, UINT& b)
	{
		UINT t = a;
		a = b;
		b = t;
	}
}
