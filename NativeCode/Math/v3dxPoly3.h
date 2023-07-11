/********************************************************************
	V3D					A Powerful 3D Enjine
	File:				v3dxpoly3.h
	Created Time:		30:6:2002   16:30
	Modify Time:
	Original Author:	< johnson >
	More Author:	
	Abstract:			?
	
	Note:				

*********************************************************************/

#ifndef __v3dxPoly3__H__
#define __v3dxPoly3__H__

#include "v3dxVector3.h"
#include "v3dxPlane3.h"
#include "v3dxBox3.h"

#define POL_SAME_PLANE 0
#define POL_FRONT 1
#define POL_BACK 2
#define POL_SPLIT_NEEDED 3

#pragma pack(push,4)

class v3dxPoly3
{
public:
	v3dxVector3							PlaneNormal;
	v3dxVector3							*vertices;//3d����
	int									num_vertices;//������Ŀ
	int									max_vertices;//��󶥵���Ŀ
public:
	 v3dxPoly3 (int start_size = 12);//���캯��
	 v3dxPoly3 (const v3dxPoly3& copy);//�������캯��
	 v3dxPoly3 & operator = (const v3dxPoly3& copy);//�������ƺ���
	 ~v3dxPoly3 ();//��������

	inline void UpdatePlaneNormal()
	{
		PlaneNormal = computeNormal();
	}
	 void getInversePoly( v3dxPoly3* pPoly );
	void makeEmpty (){//��ʼ��һ�������Ϊ�յ�
		num_vertices = 0; 
	}
	int getNumVertices () const { 
		return num_vertices; 
	}
	v3dxVector3* getVertices () const { 
		return vertices; 
	}
	v3dxVector3* getVertex (int i) const{
		if (i<0 || i>=num_vertices) return NULL;
		return &(vertices[i]);
	}
	v3dxVector3& operator[] (int i){
		return vertices[i];
	}
	const v3dxVector3& operator[] (int i) const{
		return vertices[i];
	}
	v3dxVector3* getFirst () const{ 
		if (num_vertices<=0) 
			return NULL;  
		else return vertices; 
	}
	v3dxVector3* getLast () const{ 
		if (num_vertices<=0) 
			return NULL; 
		else 
			return &vertices[num_vertices-1]; 
	}
	//����һ�������Ƿ��ڶ�����ڲ�
	 bool in (const v3dxVector3& v) const;
	//����һ�������Ƿ��ڶ�����ڲ�
	bool InDFace ( const v3dxVector3& v ) const{
		int i, i1;
		i1 = num_vertices-1;
		int prevType = 0;
		for (i = 0 ; i < num_vertices ; i++){
			int type = v3dxWhichSide3D_v2( &v, &vertices[i1], &vertices[i] );
			if( prevType*type<0 )
				return false;
			if( type!=0 )
				prevType = type;
			i1 = i;
		}
		return true;
	}
	 static bool in (v3dxVector3* poly, int num_poly, const v3dxVector3& v);
	//����һ��ָ�������Ŀ����Ķ����
	 void makeRoom (int new_max);
	void setNumVertices (int n) { makeRoom (n); num_vertices = n; }
	
	//���һ�����㵽����Σ����ر���ӵĶ��������
	 int addVertex (float x, float y, float z,vBOOL bUpdateNormal=FALSE);
	int addVertex (const v3dxVector3& v) { 
		return addVertex (v.x, v.y, v.z); 
	}
	
	void setVertices (v3dxVector3 *v, int num){ 
		memcpy (vertices, v, (num_vertices = num) * sizeof (v3dxVector3)); 
	}
	
	//��������������һ��ƽ����Ǹ�λ��
	//�����������ϣ�����POL_SAME_PLANE
	//�����ȫ�����ƽ��ǰ�棬����POL_FRONT
	//�����ȫ�����ƽ�汳�󣬷���POL_BACK
	//���򣬷���POL_SPLIT_NEEDED
	int classify (const v3dxPlane3& pl) const;
	
	//��ֱ��������������
	int classifyX (float x) const;
	int classifyY (float y) const;
	int classifyZ (float z) const;

	//�ָ��������Σ�����ֻ����ǰ���
	void cutToPlane (const v3dxPlane3& split_plane);

	//�ø�����ƽ�棬�ָ���������
	 void splitWithPlane (v3dxPoly3& front, v3dxPoly3& back,
  			const v3dxPlane3& split_plane) const;

	// ������Plane�ָ�������Polygon
	 v3dxPoly3 clipByPlanes(v3dxPlane3* Planes, int PlaneCount);


	//�ָ��������
	 void splitWithPlaneX (v3dxPoly3& front, v3dxPoly3& back, float x) const;
	 void splitWithPlaneY (v3dxPoly3& front, v3dxPoly3& back, float y) const;
	 void splitWithPlaneZ (v3dxPoly3& front, v3dxPoly3& back, float z) const;
	
	//���㷨����
	 static v3dxVector3 computeNormal (v3dxVector3* vertices, int num);
	inline v3dxVector3 computeNormal () const{
		return computeNormal (vertices, num_vertices);
	}

	//�������ε�ƽ��??(���ƽ��������ʲô�أ�)
	 static v3dxPlane3 computePlane (v3dxVector3* vertices, int num);

	//�����������ε�ƽ��
	v3dxPlane3 computePlane () const{
		return computePlane (vertices, num_vertices);
	}

	//�����������
	 float getSignedArea() const;
  
	//������������
	v3dxVector3 getCenter () const;
};

inline int v3dxPoly3::classify (const v3dxPlane3& pl) const
{
	int i;
	int front = 0, back = 0;

	for (i = 0 ; i < num_vertices ; i++)
	{
		float dot = pl.classify (vertices[i]);
		if (std::abs(dot) < EPSILON)
			dot = 0;
		if (dot < 0.f) 
			back++;
		else if (dot > 0.f) 
			front++;
	}
	if (back == 0 && front == 0) 
		return POL_SAME_PLANE;
	if (back == 0) 
		return POL_FRONT;
	if (front == 0)
		return POL_BACK;
	return POL_SPLIT_NEEDED;
}

inline int v3dxPoly3::classifyX (float x) const
{
	int i;
	int front = 0, back = 0;

	for (i = 0 ; i < num_vertices ; i++)
	{
		float xx = vertices[i].x-x;
		if (xx < -EPSILON) 
			front++;
		else if (xx > EPSILON) 
			back++;
	}
	if (back == 0) return POL_FRONT;
	if (front == 0) return POL_BACK;
	return POL_SPLIT_NEEDED;
}

inline int v3dxPoly3::classifyY (float y) const
{
	int i;
	int front = 0, back = 0;

	for (i = 0 ; i < num_vertices ; i++)
	{
		float yy = vertices[i].y-y;
		if (yy < -EPSILON) 
			front++;
		else if (yy > EPSILON) 
			back++;
	}
	if (back == 0) return POL_FRONT;
	if (front == 0) return POL_BACK;
	return POL_SPLIT_NEEDED;
}

inline int v3dxPoly3::classifyZ (float z) const
{
	int i;
	int front = 0, back = 0;

	for (i = 0 ; i < num_vertices ; i++)
	{
		float zz = vertices[i].z-z;
		if (zz < -EPSILON) 
			front++;
		else if (zz > EPSILON) 
			back++;
	}
	if (back == 0) return POL_FRONT;
	if (front == 0) return POL_BACK;
	return POL_SPLIT_NEEDED;
}

inline float v3dxPoly3::getSignedArea () const
{
	float area = 0.0;
	//������������Σ���������(0,1,2), (0,2,3), (0,3,4), �ȵ�..
	for (int i=0 ; i < num_vertices-2 ; i++)
		area += (float)v3dxArea3 ( &vertices[0], &vertices[i+1], &vertices[i+2] );
	return area / 2.0f;
}

inline v3dxVector3 v3dxPoly3::getCenter () const
{
	int i;
	v3dxBox3 bbox;
	bbox.InitializeBox(vertices[0]);
	for (i = 1 ; i < num_vertices ; i++)
		bbox.MergeVertex(vertices[i]);
	return bbox.GetCenter ();
}

#pragma pack(pop)

#endif