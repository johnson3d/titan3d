
#include "DisjointSet.h"

NS_BEGIN

FDisjointSet::FDisjointSet( const UINT32 Size )
{
	Init( Size );
}

void FDisjointSet::Init( UINT32 Size )
{
	Parents.resize( Size);
	for( UINT32 i = 0; i < Size; i++ )
	{
		Parents[i] = i;
	}
}

void FDisjointSet::Reset()
{
	Parents.clear();
}

void FDisjointSet::AddDefaulted( UINT32 Num )
{
	UINT32 Start = (UINT32)Parents.size();
	Parents.resize(Num + Start);

	for( UINT32 i = Start; i < Start + Num; i++ )
	{
		Parents[i] = i;
	}
}

// Union with splicing
inline void FDisjointSet::Union( UINT32 x, UINT32 y )
{
	UINT32 px = Parents[x];
	UINT32 py = Parents[y];

	while( px != py )
	{
		// Pick larger
		if( px < py )
		{
			Parents[x] = py;
			if( x == px )
			{
				return;
			}
			x = px;
			px = Parents[x];
		}
		else
		{
			Parents[y] = px;
			if( y == py )
			{
				return;
			}
			y = py;
			py = Parents[y];
		}
	}
}

// Optimized version of Union when iterating for( x : 0 to N ) unioning x with lower indexes.
// Neither x nor y can have already been unioned with an index > x.
void FDisjointSet::UnionSequential( UINT32 x, UINT32 y )
{
	assert( x >= y );
	assert( x == Parents[x] );

	UINT32 px = x;
	UINT32 py = Parents[y];
	while( px != py )
	{
		Parents[y] = px;
		if( y == py )
		{
			return;
		}
		y = py;
		py = Parents[y];
	}
}

// Find with path compression
UINT32 FDisjointSet::Find( UINT32 i )
{
	// Find root
	UINT32 Start = i;
	UINT32 Root = Parents[i];
	while( Root != i )
	{
		i = Root;
		Root = Parents[i];
	}

	// Point all nodes on path to root
	i = Start;
	UINT32 Parent = Parents[i];
	while( Parent != Root )
	{
		Parents[i] = Root;
		i = Parent;
		Parent = Parents[i];
	}

	return Root;
}

NS_END