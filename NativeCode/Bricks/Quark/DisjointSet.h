#pragma once

#include <vector>
#include "../../Base/BaseHead.h"

NS_BEGIN

class FDisjointSet
{
public:
			FDisjointSet() {}
			FDisjointSet( const UINT Size );
	
	void	Init( UINT Size );
	void	Reset();
	void	AddDefaulted( UINT Num = 1 );

	void	Union( UINT x, UINT y );
	void	UnionSequential( UINT x, UINT y );
	UINT	Find( UINT i );

	UINT	operator[]( UINT i ) const	{ return Parents[i]; }

private:
	std::vector< UINT >	Parents;
};

NS_END