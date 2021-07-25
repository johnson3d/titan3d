#pragma once

#include "../IUnknown.h"
#include "../../Math/v3dxVector3.h"

class v3dxVector3;
NS_BEGIN

//template<typename _PointType, class _FType = double>
//_FType KM_GetDistanceSq(const vfxKMeans<_PointType, _FType>::kmPoint& lh, const kmPoint<_PointType, _FType>& rh)
//{
//	FType result = 0;
//	/*for (int i = 0; i < Dimension; i++)
//	{
//		auto dist = lh.mValue[i] - rh.mValue[i];
//		result += dist * dist;
//	}*/
//	return result;
//}
//
//template<typename _PointType, class _FType = double>
//void KM_GetCenter(const vfxKMeans<_PointType, _FType>::kmPoint* points, int num, kmPoint<_PointType, _FType>& outCenter)
//{
//
//}

template<typename _PointType>
class kmPoint
{
public:
	typedef _PointType PointType;

	int mPointId, mClusterId;
	PointType mPoint;
public:
	kmPoint()
	{
		mPointId = -1;
		mClusterId = -1;
	}
	kmPoint(int id, const PointType& pt)
	{
		mPointId = id;
		mPoint = pt;
		mClusterId = 0; // Initially not assigned to any cluster
	}
	/*static FType GetDistanceSq(const kmPoint& lh, const kmPoint& rh)
	{
		FType result = 0;
		for (int i = 0; i < Dimension; i++)
		{
			auto dist = lh.mValue[i] - rh.mValue[i];
			result += dist * dist;
		}
		return result;
	}*/

	int GetCluster() {
		return mClusterId;
	}

	int GetID() {
		return mPointId;
	}

	void SetCluster(int val) {
		mClusterId = val;
	}
};

template<typename _PointType, class _FType, typename _PointOperator>
class vfxKMeans
{
public:
	typedef kmPoint<_PointType> _PointDefine;
	typedef _FType FType;
	class kmCluster
	{
	public:
		int mClusterId;
		_PointDefine mCentroid;
		std::vector<_PointDefine> mPoints;

	public:
		kmCluster(int clusterId, const _PointDefine& centroid)
		{
			this->mClusterId = clusterId;
			mCentroid = centroid;
			this->AddPoint(centroid);
		}

		void AddPoint(_PointDefine p)
		{
			p.SetCluster(this->mClusterId);
			mPoints.push_back(p);
		}

		bool RemovePoint(int pointId)
		{
			int size = mPoints.size();

			for (int i = 0; i < size; i++)
			{
				if (mPoints[i].GetID() == pointId)
				{
					mPoints.erase(mPoints.begin() + i);
					return true;
				}
			}
			return false;
		}

		void RemoveAllPoints() { 
			mPoints.clear();
		}

		int GetId() { 
			return mClusterId;
		}

		_PointDefine GetPoint(int pos) {
			return mPoints[pos];
		}

		int GetSize() { 
			return (int)mPoints.size();
		}
	};
public:
	int K, MaxIters, NumOfPoints;
	std::vector<kmCluster> mClusters;
	void Reset()
	{
		K = MaxIters = NumOfPoints = 0;
		mClusters.clear();
	}

	void ClearClusters()
	{
		for (int i = 0; i < K; i++)
		{
			mClusters[i].RemoveAllPoints();
		}
	}

	int GetNearestClusterId(_PointDefine point)
	{
		FType sum, min_dist;
		int NearestClusterId;

		sum = _PointOperator::GetDistanceSq(mClusters[0].mCentroid, point);
		
		min_dist = sqrt(sum);
		// min_dist = sum;
		NearestClusterId = mClusters[0].GetId();

		for (int i = 1; i < K; i++)
		{
			FType dist;
			
			sum = _PointOperator::GetDistanceSq(mClusters[i].mCentroid, point);
			dist = sqrt(sum);

			// dist = sum;

			if (dist < min_dist)
			{
				min_dist = dist;
				NearestClusterId = mClusters[i].GetId();
			}
		}

		return NearestClusterId;
	}

public:
	vfxKMeans()
	{
		Reset();
	}

	void Execute(int k, int iterations, std::vector<_PointDefine>& all_points)
	{
		Reset();

		K = k;
		MaxIters = iterations;

		NumOfPoints = (int)all_points.size();

		// Initializing Clusters
		{
			std::vector<int> used_pointIds;
			for (int i = 1; i <= K; i++)
			{
				while (true)
				{
					int index = rand() % NumOfPoints;

					if (find(used_pointIds.begin(), used_pointIds.end(), index) == used_pointIds.end())
					{
						used_pointIds.push_back(index);
						all_points[index].SetCluster(i);
						kmCluster cluster(i, all_points[index]);
						mClusters.push_back(cluster);
						break;
					}
				}
			}
		}
		int iter = 1;
		while (true)
		{
			bool done = true;

			// Add all points to their nearest cluster
			for (int i = 0; i < NumOfPoints; i++)
			{
				int currentClusterId = all_points[i].GetCluster();
				int nearestClusterId = GetNearestClusterId(all_points[i]);

				if (currentClusterId != nearestClusterId)
				{
					all_points[i].SetCluster(nearestClusterId);
					done = false;
				}
			}

			// clear all existing clusters
			ClearClusters();

			// reassign points to their new clusters
			for (int i = 0; i < NumOfPoints; i++)
			{
				// cluster index is ID-1
				mClusters[all_points[i].GetCluster() - 1].AddPoint(all_points[i]);
			}

			// Recalculating the center of each cluster
			for (int i = 0; i < K; i++)
			{
				_PointOperator::GetCenter(mClusters[i].mPoints, mClusters[i].mCentroid);
				int ClusterSize = mClusters[i].GetSize();				
				if (ClusterSize > 0)
				{	
					/*for (int j = 0; j < Dimension; j++)
					{
						FType sum = 0.0;
						for (int p = 0; p < ClusterSize; p++)
						{
							sum += mClusters[i].GetPoint(p).GetVal(j);
						}
						mClusters[i].SetCentroidByPos(j, sum / ClusterSize);
					}*/
				}
			}

			if (done || iter >= MaxIters)
			{
				break;
			}
			iter++;
		}
	}
};

class TR_CLASS(SV_Dispose=delete self)
KMeamsVector3
{
public:
	struct Vector3Operator
	{
		static float GetDistanceSq(kmPoint<v3dxVector3> lh, kmPoint<v3dxVector3> rh)
		{
			return (lh.mPoint - rh.mPoint).getLengthSq();
		}
		static void GetCenter(std::vector<kmPoint<v3dxVector3>>& points, kmPoint<v3dxVector3>& outCenter)
		{
			if (points.size() == 0)
				return;
			outCenter.mPoint.setValue(0,0,0);
			for (auto& i : points)
			{
				outCenter.mPoint += i.mPoint;
			}
			outCenter.mPoint /= (float)points.size();
		}
	};
	typedef vfxKMeans<v3dxVector3, float, Vector3Operator> KMeansType;
	KMeansType		mKMeans;
	KMeamsVector3()
	{

	}
	void Execute(int k, int iterations, v3dxVector3* points, int num);

	int NumOfCluster() {
		return (int)mKMeans.mClusters.size();
	}
	KMeansType::kmCluster* GetCluster(int index) {
		return &mKMeans.mClusters[index];
	}
	const v3dxVector3* GetClusterCentroid(int indexCluster) {
		return &mKMeans.mClusters[indexCluster].mCentroid.mPoint;
	}
	int GetClusterCentroidID(int indexCluster) {
		return mKMeans.mClusters[indexCluster].mCentroid.GetID();
	}

	int NumOfClusterPoint(int index) {
		return (int)mKMeans.mClusters[index].mPoints.size();
	}
	const v3dxVector3* GetClusterPoint(int indexCluster, int indexPoint) {
		return &mKMeans.mClusters[indexCluster].mPoints[indexPoint].mPoint;
	}
	int GetClusterPointID(int indexCluster, int indexPoint) {
		return mKMeans.mClusters[indexCluster].mPoints[indexPoint].GetID();
	}
};

NS_END