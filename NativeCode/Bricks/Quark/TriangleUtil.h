
#pragma once
#include "../../Base/BaseHead.h"
#include "../../Math/v3dxVector3.h"
#include <vector>
#include <map>

#include "HashTable.h"


NS_BEGIN

   
static inline UINT MurmurFinalize32(UINT Hash)
{
    Hash ^= Hash >> 16;
    Hash *= 0x85ebca6b;
    Hash ^= Hash >> 13;
    Hash *= 0xc2b2ae35;
    Hash ^= Hash >> 16;
    return Hash;
}
static inline UINT Murmur32(std::initializer_list< UINT > InitList)
{
    UINT Hash = 0;
    for (auto Element : InitList)
    {
        Element *= 0xcc9e2d51;
        Element = (Element << 15) | (Element >> (32 - 15));
        Element *= 0x1b873593;

        Hash ^= Element;
        Hash = (Hash << 13) | (Hash >> (32 - 13));
        Hash = Hash * 5 + 0xe6546b64;
    }

    return MurmurFinalize32(Hash);
}

inline UINT HashPosition( const v3dxVector3& Position )
{
	union { float f; UINT i; } x;
	union { float f; UINT i; } y;
	union { float f; UINT i; } z;

	x.f = Position.X;
	y.f = Position.Y;
	z.f = Position.Z;

	return Murmur32( {
		Position.X == 0.0f ? 0u : x.i,
		Position.Y == 0.0f ? 0u : y.i,
		Position.Z == 0.0f ? 0u : z.i
	} );
}
template< class T >
static constexpr inline T Square(const T A)
{
    return A * A;
}
template< class T >
static constexpr inline T Max3(const T A, const T B, const T C)
{
    return std::max(std::max(A, B), C);
}

// 0, 1, 2 => 1, 2, 0
inline UINT Cycle3( UINT Value )
{
	UINT ValueMod3 = Value % 3;
	UINT Value1Mod3 = ( 1 << ValueMod3 ) & 3;
	return Value - ValueMod3 + Value1Mod3;
}

inline UINT Cycle3( UINT Value, UINT Offset )
{
	return Value - Value % 3 + ( Value + Offset ) % 3;
}

inline float EquilateralArea( float EdgeLength )
{
	const float sqrt3_4 = 0.4330127f;
	return sqrt3_4 * Square( EdgeLength * EdgeLength );
}

inline float EquilateralEdgeLength( float Area )
{
	const float sqrt3_4 = 0.4330127f;
	return std::sqrt( Area / sqrt3_4 );
}

// a,b,c are tessellation factors for each edge
inline INT32 ApproxNumTris( INT32 a, INT32 b, INT32 c )
{
	// Heron's formula divided by area of unit triangle
	float s = 0.5f * ( a + b + c );
	float NumTris = 4.0f * std::sqrt( std::max( 0.0625f, s * (s - a) * (s - b) * (s - c) / 3.0f ) );
	INT32 MaxFactor = Max3( a, b, c );
	return std::max( (INT32)std::round( NumTris ), MaxFactor );
}
static UINT FloorLog2(UINT Value)
{
    // Use BSR to return the log2 of the integer
    // return 0 if value is 0
	DWORD BitIndex;
    return _BitScanReverse(&BitIndex, Value) ? BitIndex : 0;
}

namespace Barycentric
{
	// [ Schindler and Chen 2012, "Barycentric Coordinates in Olympiad Geometry" https://web.evanchen.cc/handouts/bary/bary-full.pdf ]
	inline float LengthSquared( const v3dxVector3& Barycentrics0, const v3dxVector3& Barycentrics1, const v3dxVector3& EdgeLengthsSqr )
	{
		// Barycentric displacement vector:
		// 0 = x + y + z
		v3dxVector3 Disp = Barycentrics0 - Barycentrics1;

		/*	TODO change edge order to match ariel coords
				v0
				/\
			e2 /  \ e0
			  /____\
			v2  e1  v1
		*/

		// Length of displacement
		return	-Disp.X * Disp.Y * EdgeLengthsSqr[0]
				-Disp.Y * Disp.Z * EdgeLengthsSqr[1]
				-Disp.Z * Disp.X * EdgeLengthsSqr[2];
	}

	inline float SubtriangleArea( const v3dxVector3& Barycentrics0, const v3dxVector3& Barycentrics1, const v3dxVector3& Barycentrics2, float TriangleArea )
	{
		// Area * Determinant using triple product
		return TriangleArea * std::abs( Barycentrics0.dotProduct( Barycentrics1.crossProduct(Barycentrics2) ) );
	}

	// https://math.stackexchange.com/questions/3748903/closest-point-to-triangle-edge-with-barycentric-coordinates
	inline float DistanceToEdge( float Barycentric, float EdgeLength, float TriangleArea )
	{
		return 2.0f * Barycentric * TriangleArea / EdgeLength;
	}
}

inline void SubtriangleBarycentrics( UINT TriX, UINT TriY, UINT FlipTri, UINT NumSubdivisions, v3dxVector3 Barycentrics[3] )
{
	/*
		Vert order:
		1    0__1
		|\   \  |
		| \   \ |  <= flip triangle
		|__\   \|
		0   2   2
	*/

	UINT VertXY[3][2] =
	{
		{ TriX,		TriY	},
		{ TriX,		TriY + 1},
		{ TriX + 1,	TriY	},
	};
	VertXY[0][1] += FlipTri;
	VertXY[1][0] += FlipTri;

	for( int Corner = 0; Corner < 3; Corner++ )
	{
		Barycentrics[ Corner ][0] = float(VertXY[ Corner ][0]);
		Barycentrics[ Corner ][1] = float(VertXY[ Corner ][1]);
		Barycentrics[ Corner ][2] = float(NumSubdivisions - VertXY[ Corner ][0] - VertXY[ Corner ][1]);
		Barycentrics[ Corner ]   /= float(NumSubdivisions);
	}
}

// Find edge with opposite direction that shares these 2 verts.
/*
	  /\
	 /  \
	o-<<-o
	o->>-o
	 \  /
	  \/
*/
class FEdgeHash
{
public:
	FHashTable HashTable;

	FEdgeHash( INT32 Num )
		: HashTable( 1 << FloorLog2( Num ), Num )
	{}

	template< typename FGetPosition >
	void Add_Concurrent( INT32 EdgeIndex, FGetPosition&& GetPosition )
	{
		v3dxVector3 Position0;
		bool isValid = GetPosition(EdgeIndex, Position0);
		if (!isValid)
			return;

		v3dxVector3 Position1;
		isValid = GetPosition(Cycle3(EdgeIndex), Position1);
		if (!isValid)
			return;
				
		UINT Hash0 = HashPosition( Position0 );
		UINT Hash1 = HashPosition( Position1 );
		UINT Hash = Murmur32( { Hash0, Hash1 } );

		HashTable.Add_Concurrent( Hash, EdgeIndex );
	}

	template< typename FGetPosition, typename FuncType >
	void ForAllMatching( INT32 EdgeIndex, bool bAdd, FGetPosition&& GetPosition, FuncType&& Function )
	{
		v3dxVector3 Position0;
		bool isValid = GetPosition(EdgeIndex, Position0);
		if (!isValid)
			return;

		v3dxVector3 Position1;
		isValid = GetPosition(Cycle3(EdgeIndex), Position1);
		if (!isValid)
			return;
				
		UINT Hash0 = HashPosition( Position0 );
		UINT Hash1 = HashPosition( Position1 );
		UINT Hash = Murmur32( { Hash1, Hash0 } );
		
		for( UINT OtherEdgeIndex = HashTable.First( Hash ); HashTable.IsValid( OtherEdgeIndex ); OtherEdgeIndex = HashTable.Next( OtherEdgeIndex ) )
		{
			v3dxVector3 temp0;
			v3dxVector3 temp1;
			if (!GetPosition(Cycle3(OtherEdgeIndex), temp0) || !GetPosition(OtherEdgeIndex, temp1))
				continue;

			if( Position0 == temp0 && Position1 == temp1)
			{
				// Found matching edge.
				Function( EdgeIndex, OtherEdgeIndex );
			}
		}

		if( bAdd )
			HashTable.Add( Murmur32( { Hash0, Hash1 } ), EdgeIndex );
	}
};


struct FAdjacency
{
	std::vector< INT32 >				Direct;

	typedef std::vector<INT32> ExtendedValues;
	std::map< INT32, ExtendedValues >	Extended;

	FAdjacency( INT32 Num )
	{
		Direct.resize( Num );
	}

	void InsertToExtends(INT32 k, INT32 v)
	{
        auto keyIter = Extended.find(k);
        if (keyIter != Extended.end())
        {
            auto values = keyIter->second;
            for (int i = 0; i < values.size(); i++)
            {
				if (values[i] == v)
					return;
            }
			values.push_back(v);
        }
	}

	void	Link( INT32 EdgeIndex0, INT32 EdgeIndex1 )
	{
		if( Direct[ EdgeIndex0 ] < 0 && 
			Direct[ EdgeIndex1 ] < 0 )
		{
			Direct[ EdgeIndex0 ] = EdgeIndex1;
			Direct[ EdgeIndex1 ] = EdgeIndex0;
		}
		else
		{
			// TODO:
            //Extended.AddUnique( EdgeIndex0, EdgeIndex1 );
            //Extended.AddUnique( EdgeIndex1, EdgeIndex0 );
            InsertToExtends(EdgeIndex0, EdgeIndex1);
            InsertToExtends(EdgeIndex1, EdgeIndex0);
		}
	}

	template< typename FuncType >
	void	ForAll( INT32 EdgeIndex, FuncType&& Function ) const
	{
		INT32 AdjIndex = Direct[ EdgeIndex ];
		if( AdjIndex != -1 )
		{
			Function( EdgeIndex, AdjIndex );
		}

		auto keyIter = Extended.find(EdgeIndex);
		if (keyIter != Extended.end())
		{
			auto values = keyIter->second;
			for (UINT i = 0; i < (UINT)values.size(); i++)
			{
				Function(EdgeIndex, values[i]);
			}
		}
	}
};

NS_END