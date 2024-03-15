#ifndef	_PGC_Commmon_H_
#define _PGC_Commmon_H_
#include "../../Inc/GlobalDefine.cginc"
#include "../../Inc/Math.cginc"

int2 GetNeighbor(int index)
{
    const int dx[] = { 0, -1, 1, -1, 1, 0, 0, -1, 1 };
    const int dy[] = { 0, -1, -1, 1, 1, -1, 1, 0, 0 };
    
    return int2(dx[index], dy[index]);
}

#endif//_PGC_Commmon_H_