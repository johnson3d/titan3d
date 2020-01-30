/********************************************************************
	created:	2007/07/24
	created:	24:7:2007   20:11
	filename: 	e:\Works\VictoryProject\Victory\Code\vfxgeometry\Inc\v3dxSpline.h
	file path:	e:\Works\VictoryProject\Victory\Code\vfxgeometry\Inc
	file base:	v3dxSpline
	file ext:	h
	author:		Jones
	
	purpose:	v3dxSpline:样条曲线类
				v3dxCurve2:用于调整z=0平面[0～x]区间内y数值的2d曲线
*********************************************************************/
#ifndef __V3DSPLINE__
#define __V3DSPLINE__

#include "v3dxVector3.h"

#pragma pack(push,4)

//V3D_BEGIN

//class VString;

class v3dxSpline
{
public:
	struct sSubPoint
	{
		sSubPoint():vPos(0,0,0),vVel1(0,0,0),vVel2(0,0,0),fDist(0){}
		v3dxVector3	vPos;
		v3dxVector3	vVel1, vVel2;
		float		fDist;
	};
public:
	v3dxSpline():m_fMaxDistance(0),m_bTangentOnTail(FALSE){
	}
	virtual ~v3dxSpline(){
		removeAll();
	}
	virtual  void insertNode(int idx, const v3dxVector3 &pos);
	virtual  void deleteNode(int idx);
	virtual  void setPosition(int idx, const v3dxVector3 &pos);
	virtual  v3dxVector3 getPosition(float fTime, int* pIndex = NULL, v3dxVector3* pVel = NULL);
	virtual  void genLineNodes(std::vector<v3dxVector3>* aArray, float fDistStep = 1.f, vBOOL bGetLessNode = TRUE);
	 virtual void buildVelocity(int idx);
	 void setBeginVel(int idx, const v3dxVector3 &vel, vBOOL bWithEnd = TRUE);
	 void setEndVel(int idx, const v3dxVector3 &vel, vBOOL bWithBegin = TRUE);
	 void buildVelocities();
	 void enableTangentOnTail(vBOOL b);
	 v3dxSpline& operator = (const v3dxSpline& r);
	virtual void removeAll(){ m_aNodeArray.clear(); m_fMaxDistance = 0.f; }
	int  getNodeCount(){
		return (int)m_aNodeArray.size();
	}
	inline float getMaxDistance(){
		return m_fMaxDistance;
	}
	inline sSubPoint* getNode(int i){
		if (i >= 0 && i < (int)m_aNodeArray.size())
			return &m_aNodeArray[i];
		return NULL;
	}
	inline vBOOL isTangentOnTail(){
		return m_bTangentOnTail;
	}

protected:
	std::vector<sSubPoint> m_aNodeArray;// 顶点
	float	m_fMaxDistance;		// 线段集合总长度
	vBOOL	m_bTangentOnTail;	// 在首尾处设定切线
};

class v3dxCurve2 : public v3dxSpline
{
public:
	 v3dxCurve2();
	 v3dxCurve2(float fVal1, float fVal2, float fAspect = 0, float fMin = FLT_MIN, float fMax = FLT_MAX);
	virtual ~v3dxCurve2(){}
	virtual  void insertNode(int idx, const v3dxVector3 &pos);
	virtual  void deleteNode(int idx);
	virtual  void setPosition(int idx, const v3dxVector3 &pos);
	virtual  v3dxVector3 getPosition(float fTime, int* pIndex = NULL, v3dxVector3* pVel = NULL);
	virtual  void genLineNodes(std::vector<v3dxVector3>* aArray, float fDistStep = 1.f, vBOOL bGetLessNode = TRUE);
	virtual  void removeAll();
	 float getValue(float fTime);
	 v3dxCurve2& operator = (const v3dxCurve2& r);
	 void setViewAspect(float fAspect);
	 void setValue(float fBgn, float fEnd, float fRand);
	// void setValueByString(LPCSTR str);
	// VString getValueString();
	 void setValBegin(float v);
	 void setValEnd(float v);
	 void setValRange(float fMin, float fMax);
	 void enableStraightMode(vBOOL b);
	inline void enableHeadRandom(vBOOL b){
		m_bRandomOnHead = b;
	}
	inline vBOOL isStraightMode(){
		return m_bStraightMode;
	}
	inline vBOOL isRandomOnHead(){
		return m_bRandomOnHead;
	}
	inline void	setValRand(float v){
		m_fValRand = v;
	}
	inline float getValBegin() {
		if ( m_bRandomOnHead )
			return m_fValBgn + m_fValRand * (((float)((rand()%2)-1)*(float)rand())/RAND_MAX);
		return m_fValBgn;
	}
	inline float getValEnd() {
		return m_fValEnd;
	}
	inline float getValMin() {
		return m_fValMin;
	}
	inline float getValMax() {
		return m_fValMax;
	}
	inline float getValRange()	{
		float v = m_fValMax - m_fValMin;
		//if ( fabsf(v) < 2.f )
		//	return 2.f;
		return v;
	}
	inline float getValRand(){
		return m_fValRand;
	}
	inline float getHoriLength(){
		return m_fHoriLength;
	}

protected:
	float	m_fValBgn;	// 数值初值
	float	m_fValEnd;	// 数值末值
	float	m_fValMin;	// 数值最大值
	float	m_fValMax;	// 数值最小值
	float	m_fHoriLength;// 水平线长度
	float	m_fValRand;	// 数值随机幅度
	vBOOL	m_bRandomOnHead;// 随机幅度只作用于第一个顶点
	vBOOL	m_bStraightMode;// 平直模式
};
//V3D_END

#pragma pack(pop)

#endif // __V3DXSPLINE__
