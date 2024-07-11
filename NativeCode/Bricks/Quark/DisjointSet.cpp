
#include "DisjointSet.h"

NS_BEGIN

FDisjointSet::FDisjointSet( const UINT Size )
{
	Init( Size );
}

void FDisjointSet::Init( UINT Size )
{
	Parents.resize( Size);
	for( UINT i = 0; i < Size; i++ )
	{
		Parents[i] = i;
	}
}

void FDisjointSet::Reset()
{
	Parents.clear();
}

void FDisjointSet::AddDefaulted( UINT Num )
{
	UINT Start = (UINT)Parents.size();
	Parents.resize(Num + Start);

	for( UINT i = Start; i < Start + Num; i++ )
	{
		Parents[i] = i;
	}
}

// Union with splicing
inline void FDisjointSet::Union( UINT x, UINT y )
{
	UINT px = Parents[x];
	UINT py = Parents[y];

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
void FDisjointSet::UnionSequential( UINT x, UINT y )
{
	assert( x >= y );
	assert( x == Parents[x] );

	UINT px = x;
	UINT py = Parents[y];
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
UINT FDisjointSet::Find( UINT i )
{
	// Find root
	UINT Start = i;
	UINT Root = Parents[i];
	while( Root != i )
	{
		i = Root;
		Root = Parents[i];
	}

	// Point all nodes on path to root
	i = Start;
	UINT Parent = Parents[i];
	while( Parent != Root )
	{
		Parents[i] = Root;
		i = Parent;
		Parent = Parents[i];
	}

	return Root;
}

NS_END