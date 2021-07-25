#include "kmeans.h"

#define new VNEW

NS_BEGIN

void KMeamsVector3::Execute(int k, int iterations, v3dxVector3* points, int num)
{
	std::vector<kmPoint<v3dxVector3>> all_points;
	for (int i = 0; i < num; i++)
	{
		kmPoint<v3dxVector3> v(-1, (float*)&points[i]);
		
		all_points.push_back(v);
	}
	
	mKMeans.Execute(k, iterations, all_points);
}


NS_END