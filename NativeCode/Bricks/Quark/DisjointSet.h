#pragma once

#include <vector>
#include "../../Base/BaseHead.h"

NS_BEGIN

class FDisjointSet
{
public:
			FDisjointSet() {}
			FDisjointSet( const UINT32 Size );
	
	void	Init( UINT32 Size );
	void	Reset();
	void	AddDefaulted( UINT32 Num = 1 );

	void	Union( UINT32 x, UINT32 y );
	void	UnionSequential( UINT32 x, UINT32 y );
	UINT32	Find( UINT32 i );

	UINT32	operator[]( UINT32 i ) const	{ return Parents[i]; }

private:
	std::vector< UINT32 >	Parents;
};

NS_END