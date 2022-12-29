#include "MeshLoaderObj.h"
#include <stdio.h>
#include <stdlib.h>
#include <cstring>

#include <math.h>

#define new VNEW

NS_BEGIN

rcMeshLoaderObj::rcMeshLoaderObj() :
	m_scale(1.0f),
	m_verts(0),
	m_tris(0),
	m_normals(0),
	m_vertCount(0),
	m_triCount(0)
{
}

rcMeshLoaderObj::~rcMeshLoaderObj()
{
	Safe_DeleteArray(m_verts);
	Safe_DeleteArray(m_normals);
	Safe_DeleteArray(m_tris);
}
		
void rcMeshLoaderObj::addVertex(float x, float y, float z, int& cap)
{
	if (m_vertCount+1 > cap)
	{
		cap = !cap ? 8 : cap*2;
		float* nv = new float[cap*3];
		if (m_vertCount)
			memcpy(nv, m_verts, m_vertCount*3*sizeof(float));
		Safe_DeleteArray(m_verts);
		m_verts = nv;
	}
	float* dst = &m_verts[m_vertCount*3];
	*dst++ = x*m_scale;
	*dst++ = y*m_scale;
	*dst++ = z*m_scale;
	m_vertCount++;
}

void rcMeshLoaderObj::addTriangle(int a, int b, int c, int& cap)
{
	if (m_triCount+1 > cap)
	{
		cap = !cap ? 8 : cap*2;
		int* nv = new int[cap*3];
		if (m_triCount)
			memcpy(nv, m_tris, m_triCount*3*sizeof(int));
		Safe_DeleteArray(m_tris);
		m_tris = nv;
	}
	int* dst = &m_tris[m_triCount*3];
	*dst++ = a;
	*dst++ = b;
	*dst++ = c;
	m_triCount++;
}

//static char* parseRow(char* buf, char* bufEnd, char* row, int len)
//{
//	bool start = true;
//	bool done = false;
//	int n = 0;
//	while (!done && buf < bufEnd)
//	{
//		char c = *buf;
//		buf++;
//		// multirow
//		switch (c)
//		{
//			case '\\':
//				break;
//			case '\n':
//				if (start) break;
//				done = true;
//				break;
//			case '\r':
//				break;
//			case '\t':
//			case ' ':
//				if (start) break;
//				// else falls through
//			default:
//				start = false;
//				row[n++] = c;
//				if (n >= len-1)
//					done = true;
//				break;
//		}
//	}
//	row[n] = '\0';
//	return buf;
//}
//
//static int parseFace(char* row, int* data, int n, int vcnt)
//{
//	int j = 0;
//	while (*row != '\0')
//	{
//		// Skip initial white space
//		while (*row != '\0' && (*row == ' ' || *row == '\t'))
//			row++;
//		char* s = row;
//		// Find vertex delimiter and terminated the string there for conversion.
//		while (*row != '\0' && *row != ' ' && *row != '\t')
//		{
//			if (*row == '/') *row = '\0';
//			row++;
//		}
//		if (*s == '\0')
//			continue;
//		int vi = atoi(s);
//		data[j++] = vi < 0 ? vi+vcnt : vi-1;
//		if (j >= n) return j;
//	}
//	return j;
//}
//
//bool rcMeshLoaderObj::load(const std::string& filename)
//{
//	char* buf = 0;
//	FILE* fp = fopen(filename.c_str(), "rb");
//	if (!fp)
//		return false;
//	if (fseek(fp, 0, SEEK_END) != 0)
//	{
//		fclose(fp);
//		return false;
//	}
//	long bufSize = ftell(fp);
//	if (bufSize < 0)
//	{
//		fclose(fp);
//		return false;
//	}
//	if (fseek(fp, 0, SEEK_SET) != 0)
//	{
//		fclose(fp);
//		return false;
//	}
//	buf = new char[bufSize];
//	if (!buf)
//	{
//		fclose(fp);
//		return false;
//	}
//	size_t readLen = fread(buf, bufSize, 1, fp);
//	fclose(fp);
//
//	if (readLen != 1)
//	{
//		delete[] buf;
//		return false;
//	}
//
//	char* src = buf;
//	char* srcEnd = buf + bufSize;
//	char row[512];
//	int face[32];
//	float x,y,z;
//	int nv;
//	int vcap = 0;
//	int tcap = 0;
//	
//	while (src < srcEnd)
//	{
//		// Parse one row
//		row[0] = '\0';
//		src = parseRow(src, srcEnd, row, sizeof(row)/sizeof(char));
//		// Skip comments
//		if (row[0] == '#') continue;
//		if (row[0] == 'v' && row[1] != 'n' && row[1] != 't')
//		{
//			// Vertex pos
//			sscanf(row+1, "%f %f %f", &x, &y, &z);
//			addVertex(x, y, z, vcap);
//		}
//		if (row[0] == 'f')
//		{
//			// Faces
//			nv = parseFace(row+1, face, 32, m_vertCount);
//			for (int i = 2; i < nv; ++i)
//			{
//				const int a = face[0];
//				const int b = face[i-1];
//				const int c = face[i];
//				if (a < 0 || a >= m_vertCount || b < 0 || b >= m_vertCount || c < 0 || c >= m_vertCount)
//					continue;
//				addTriangle(a, b, c, tcap);
//			}
//		}
//	}
//
//	delete [] buf;
//
//	// Calculate normals.
//	m_normals = new float[m_triCount*3];
//	for (int i = 0; i < m_triCount*3; i += 3)
//	{
//		const float* v0 = &m_verts[m_tris[i]*3];
//		const float* v1 = &m_verts[m_tris[i+1]*3];
//		const float* v2 = &m_verts[m_tris[i+2]*3];
//		float e0[3], e1[3];
//		for (int j = 0; j < 3; ++j)
//		{
//			e0[j] = v1[j] - v0[j];
//			e1[j] = v2[j] - v0[j];
//		}
//		float* n = &m_normals[i];
//		n[0] = e0[1]*e1[2] - e0[2]*e1[1];
//		n[1] = e0[2]*e1[0] - e0[0]*e1[2];
//		n[2] = e0[0]*e1[1] - e0[1]*e1[0];
//		float d = sqrtf(n[0]*n[0] + n[1]*n[1] + n[2]*n[2]);
//		if (d > 0)
//		{
//			d = 1.0f/d;
//			n[0] *= d;
//			n[1] *= d;
//			n[2] *= d;
//		}
//	}
//	
//	m_filename = filename;
//	return true;
//}

bool rcMeshLoaderObj::load(NxRHI::FMeshDataProvider* mesh, float scale)
{
	m_scale = scale;
	auto posData = mesh->GetStream(NxRHI::EVertexStreamType::VST_Position);
	v3dxVector3* pPos = (v3dxVector3*)posData->GetData();
	auto indexData = mesh->GetIndices();
	
	int vcap = 0;
	auto vertCount = mesh->GetVertexNumber();
	for (UINT i = 0; i < vertCount; i++)
	{
		addVertex(pPos[i].x, pPos[i].y, pPos[i].z, vcap);
	}

	int triCount = mesh->GetPrimitiveNumber();
	int* pIndex32 = nullptr;
	std::vector<int> index32Data;
	if (mesh->IsIndex32 == false)
	{
		USHORT* pIndices = (USHORT*)indexData->GetData();
		
		index32Data.resize(triCount * 3);
		for (int i=0; i<triCount * 3; i++)
		{
			index32Data[i] = pIndices[i];
		}
		pIndex32 = &index32Data[0];
	}
	else
	{
		pIndex32 = (int*)indexData->GetData();
	}

	int tcap = 0;
	for (int i = 0; i < triCount; i++)
	{
		addTriangle(pIndex32[i*3 + 0], pIndex32[i * 3 + 1], pIndex32[i * 3 + 2], tcap);
	}

	m_normals = new float[m_triCount * 3];
	for (int i = 0; i < m_triCount * 3; i += 3)
	{
		const float* v0 = &m_verts[m_tris[i] * 3];
		const float* v1 = &m_verts[m_tris[i + 1] * 3];
		const float* v2 = &m_verts[m_tris[i + 2] * 3];
		float e0[3], e1[3];
		for (int j = 0; j < 3; ++j)
		{
			e0[j] = v1[j] - v0[j];
			e1[j] = v2[j] - v0[j];
		}
		float* n = &m_normals[i];
		n[0] = e0[1] * e1[2] - e0[2] * e1[1];
		n[1] = e0[2] * e1[0] - e0[0] * e1[2];
		n[2] = e0[0] * e1[1] - e0[1] * e1[0];
		float d = sqrtf(n[0] * n[0] + n[1] * n[1] + n[2] * n[2]);
		if (d > 0)
		{
			d = 1.0f / d;
			n[0] *= d;
			n[1] *= d;
			n[2] *= d;
		}
	}

	return true;
}

NS_END