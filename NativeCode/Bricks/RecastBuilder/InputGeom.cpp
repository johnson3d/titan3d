#include "../../NextRHI/NxGeomMesh.h"
#include "InputGeom.h"
#include "Recast.h"
#include "SampleInterfaces.h"

#define new VNEW

NS_BEGIN

ENGINE_RTTI_IMPL(EngineNS::InputGeom);

static bool intersectSegmentTriangle(const float* sp, const float* sq,
									 const float* a, const float* b, const float* c,
									 float &t)
{
	float v, w;
	float ab[3], ac[3], qp[3], ap[3], norm[3], e[3];
	rcVsub(ab, b, a);
	rcVsub(ac, c, a);
	rcVsub(qp, sp, sq);
	
	// Compute triangle normal. Can be precalculated or cached if
	// intersecting multiple segments against the same triangle
	rcVcross(norm, ab, ac);
	
	// Compute denominator d. If d <= 0, segment is parallel to or points
	// away from triangle, so exit early
	float d = rcVdot(qp, norm);
	if (d <= 0.0f) return false;
	
	// Compute intersection t value of pq with plane of triangle. A ray
	// intersects iff 0 <= t. Segment intersects iff 0 <= t <= 1. Delay
	// dividing by d until intersection has been found to pierce triangle
	rcVsub(ap, sp, a);
	t = rcVdot(ap, norm);
	if (t < 0.0f) return false;
	if (t > d) return false; // For segment; exclude this code line for a ray test
	
	// Compute barycentric coordinate components and test if within bounds
	rcVcross(e, qp, ap);
	v = rcVdot(ac, e);
	if (v < 0.0f || v > d) return false;
	w = -rcVdot(ab, e);
	if (w < 0.0f || v + w > d) return false;
	
	// Segment/ray intersects triangle. Perform delayed division
	t /= d;
	
	return true;
}

static char* parseRow(char* buf, char* bufEnd, char* row, int len)
{
	bool start = true;
	bool done = false;
	int n = 0;
	while (!done && buf < bufEnd)
	{
		char c = *buf;
		buf++;
		// multirow
		switch (c)
		{
			case '\n':
				if (start) break;
				done = true;
				break;
			case '\r':
				break;
			case '\t':
			case ' ':
				if (start) break;
				// else falls through
			default:
				start = false;
				row[n++] = c;
				if (n >= len-1)
					done = true;
				break;
		}
	}
	row[n] = '\0';
	return buf;
}

// Returns true if 'a' is more lower-left than 'b'.
inline bool cmppt(const float* a, const float* b)
{
	if (a[0] < b[0]) return true;
	if (a[0] > b[0]) return false;
	if (a[2] < b[2]) return true;
	if (a[2] > b[2]) return false;
	return false;
}

// Returns true if 'c' is left of line 'a'-'b'.
inline bool left(const float* a, const float* b, const float* c)
{
	const float u1 = b[0] - a[0];
	const float v1 = b[2] - a[2];
	const float u2 = c[0] - a[0];
	const float v2 = c[2] - a[2];
	return u1 * v2 - v1 * u2 < 0;
}

// Calculates convex hull on xz-plane of points on 'pts',
// stores the indices of the resulting hull in 'out' and
// returns number of points on hull.
static int convexhull(const float* pts, int npts, int* out)
{
	// Find lower-leftmost point.
	int hull = 0;
	for (int i = 1; i < npts; ++i)
		if (cmppt(&pts[i * 3], &pts[hull * 3]))
			hull = i;
	// Gift wrap hull.
	int endpt = 0;
	int i = 0;
	do
	{
		out[i++] = hull;
		endpt = 0;
		for (int j = 1; j < npts; ++j)
			if (hull == endpt || left(&pts[hull * 3], &pts[endpt * 3], &pts[j * 3]))
				endpt = j;
		hull = endpt;
	} while (endpt != out[0]);

	return i;
}

InputGeom::InputGeom() :
	m_chunkyMesh(0),
	m_mesh(0),
	m_offMeshConCount(0),
	m_volumeCount(0),
	m_npts(0),
	m_nhull(0),
	m_polyOffset(0.0f),
	m_boxHeight(6.0f),
	m_boxDescent(1.0f)
{
}

InputGeom::~InputGeom()
{
	delete m_chunkyMesh;
	delete m_mesh;
}

BuildContext GRCContext;

bool InputGeom::LoadMesh(NxRHI::FMeshDataProvider* mesh, float scale)
{
	return loadMesh(&GRCContext, mesh, scale);
}
		
bool InputGeom::loadMesh(rcContext* ctx, NxRHI::FMeshDataProvider* mesh, float scale)
{
	if (m_mesh)
	{
		delete m_chunkyMesh;
		m_chunkyMesh = 0;
		delete m_mesh;
		m_mesh = 0;
	}
	m_offMeshConCount = 0;
	m_volumeCount = 0;
	
	m_mesh = new rcMeshLoaderObj();
	if (!m_mesh)
	{
		ctx->log(RC_LOG_ERROR, "loadMesh: Out of memory 'm_mesh'.");
		return false;
	}
	if (!m_mesh->load(mesh, scale))
	{
		return false;
	}

	rcCalcBounds(m_mesh->getVerts(), m_mesh->getVertCount(), m_meshBMin, m_meshBMax);

	m_chunkyMesh = new rcChunkyTriMesh;
	if (!m_chunkyMesh)
	{
		ctx->log(RC_LOG_ERROR, "buildTiledNavigation: Out of memory 'm_chunkyMesh'.");
		return false;
	}
	if (!rcCreateChunkyTriMesh(m_mesh->getVerts(), m_mesh->getTris(), m_mesh->getTriCount(), 256, m_chunkyMesh))
	{
		ctx->log(RC_LOG_ERROR, "buildTiledNavigation: Failed to build chunky mesh.");
		return false;
	}		

	return true;
}

bool InputGeom::IsInWalkArea(const v3dxVector3& min, const v3dxVector3& max)
{
	if (WalkDatas.size() == 0)
		return false;

	if (WalkDatas.size() % 2 != 0)
		return false;

	for (int i = 0; i < WalkDatas.size(); i+=2)
	{
		if (WalkDatas[i].x <= min.x && WalkDatas[i].y <= min.y && WalkDatas[i].z <= min.z && WalkDatas[i + 1].x >= max.x && WalkDatas[i + 1].y >= max.y && WalkDatas[i + 1].z >= max.z)
			return true;
	}

	return false;
}

void InputGeom::CreateConvexVolumes(EAreaType areatype, IBlobObject* blob, const v3dxVector3& min, const v3dxVector3& max)
{
	// Create
	int len = blob->GetSize() / sizeof(float);
	float* points = new float[len];
	::memcpy(points, blob->GetData(), blob->GetSize());

	for (int i = 0; i < len; i+=3)
	{
		float* p = &points[i];
		// Add new point 
		if (m_npts < MAX_PTS)
		{
			rcVcopy(&m_pts[m_npts * 3], p);
			m_npts++;
			// Update hull.
			if (m_npts > 1)
				m_nhull = convexhull(m_pts, m_npts, m_hull);
			else
				m_nhull = 0;
		}
		
	}

	// If clicked on that last pt, create the shape.
	//if (m_npts && rcVdistSqr(p, &m_pts[(m_npts - 1) * 3]) < rcSqr(0.2f))
	{
		if (m_nhull > 2)
		{
			// Create shape.
			float verts[MAX_PTS * 3];
			for (int i = 0; i < m_nhull; ++i)
				rcVcopy(&verts[i * 3], &m_pts[m_hull[i] * 3]);

			float minh = min.y, maxh = max.y;
			//for (int i = 0; i < m_nhull; ++i)
			//	minh = rcMin(minh, verts[i * 3 + 1]);
			//minh -= m_boxDescent;
			//maxh = minh + m_boxHeight;

			if (m_polyOffset > 0.01f)
			{
				float offset[MAX_PTS * 2 * 3];
				int noffset = rcOffsetPoly(verts, m_nhull, m_polyOffset, offset, MAX_PTS * 2);
				if (noffset > 0)
					addConvexVolume(offset, noffset, minh, maxh, areatype);
			}
			else
			{
				addConvexVolume(verts, m_nhull, minh, maxh, areatype);
			}
		}

		m_npts = 0;
		m_nhull = 0;

		if (areatype == Walk)
		{
			if (max.x > min.x && max.y > min.y && max.z > min.z)
			{
				if (IsInWalkArea(min, max) == false)
				{
					WalkDatas.push_back(min);
					WalkDatas.push_back(max);
				}
			}
		}
	}
}

void InputGeom::DeleteConvexVolumesByArea(EAreaType areatype)
{
	const ConvexVolume* vols = getConvexVolumes();
	for (int i = getConvexVolumeCount() - 1; i >= 0; --i)
	{
		if (vols[i].area == areatype)
			deleteConvexVolume(i);
	}
}

void InputGeom::ClearConvexVolumes( )
{
	m_volumeCount = 0;
}

//bool InputGeom::loadGeomSet(rcContext* ctx, const std::string& filepath)
//{
//	char* buf = 0;
//	FILE* fp = fopen(filepath.c_str(), "rb");
//	if (!fp)
//	{
//		return false;
//	}
//	if (fseek(fp, 0, SEEK_END) != 0)
//	{
//		fclose(fp);
//		return false;
//	}
//
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
//	if (readLen != 1)
//	{
//		delete[] buf;
//		return false;
//	}
//	
//	m_offMeshConCount = 0;
//	m_volumeCount = 0;
//	delete m_mesh;
//	m_mesh = 0;
//
//	char* src = buf;
//	char* srcEnd = buf + bufSize;
//	char row[512];
//	while (src < srcEnd)
//	{
//		// Parse one row
//		row[0] = '\0';
//		src = parseRow(src, srcEnd, row, sizeof(row)/sizeof(char));
//		if (row[0] == 'f')
//		{
//			// File name.
//			const char* name = row+1;
//			// Skip white spaces
//			while (*name && isspace(*name))
//				name++;
//			if (*name)
//			{
//				if (!loadMesh(ctx, name))
//				{
//					delete [] buf;
//					return false;
//				}
//			}
//		}
//		else if (row[0] == 'c')
//		{
//			// Off-mesh connection
//			if (m_offMeshConCount < MAX_OFFMESH_CONNECTIONS)
//			{
//				float* v = &m_offMeshConVerts[m_offMeshConCount*3*2];
//				int bidir, area = 0, flags = 0;
//				float rad;
//				sscanf(row+1, "%f %f %f  %f %f %f %f %d %d %d",
//					   &v[0], &v[1], &v[2], &v[3], &v[4], &v[5], &rad, &bidir, &area, &flags);
//				m_offMeshConRads[m_offMeshConCount] = rad;
//				m_offMeshConDirs[m_offMeshConCount] = (unsigned char)bidir;
//				m_offMeshConAreas[m_offMeshConCount] = (unsigned char)area;
//				m_offMeshConFlags[m_offMeshConCount] = (unsigned short)flags;
//				m_offMeshConCount++;
//			}
//		}
//		else if (row[0] == 'v')
//		{
//			// Convex volumes
//			if (m_volumeCount < MAX_VOLUMES)
//			{
//				ConvexVolume* vol = &m_volumes[m_volumeCount++];
//				sscanf(row+1, "%d %d %f %f", &vol->nverts, &vol->area, &vol->hmin, &vol->hmax);
//				for (int i = 0; i < vol->nverts; ++i)
//				{
//					row[0] = '\0';
//					src = parseRow(src, srcEnd, row, sizeof(row)/sizeof(char));
//					sscanf(row, "%f %f %f", &vol->verts[i*3+0], &vol->verts[i*3+1], &vol->verts[i*3+2]);
//				}
//			}
//		}
//		else if (row[0] == 's')
//		{
//			// Settings
//			m_hasBuildSettings = true;
//			sscanf(row + 1, "%f %f %f %f %f %f %f %f %f %f %f %f %f %d %f %f %f %f %f %f %f",
//							&m_buildSettings.cellSize,
//							&m_buildSettings.cellHeight,
//							&m_buildSettings.agentHeight,
//							&m_buildSettings.agentRadius,
//							&m_buildSettings.agentMaxClimb,
//							&m_buildSettings.agentMaxSlope,
//							&m_buildSettings.regionMinSize,
//							&m_buildSettings.regionMergeSize,
//							&m_buildSettings.edgeMaxLen,
//							&m_buildSettings.edgeMaxError,
//							&m_buildSettings.vertsPerPoly,
//							&m_buildSettings.detailSampleDist,
//							&m_buildSettings.detailSampleMaxError,
//							&m_buildSettings.partitionType,
//							&m_buildSettings.navMeshBMin[0],
//							&m_buildSettings.navMeshBMin[1],
//							&m_buildSettings.navMeshBMin[2],
//							&m_buildSettings.navMeshBMax[0],
//							&m_buildSettings.navMeshBMax[1],
//							&m_buildSettings.navMeshBMax[2],
//							&m_buildSettings.tileSize);
//		}
//	}
//	
//	delete [] buf;
//	
//	return true;
//}

//bool InputGeom::load(rcContext* ctx, const std::string& filepath)
//{
//	size_t extensionPos = filepath.find_last_of('.');
//	if (extensionPos == std::string::npos)
//		return false;
//
//	std::string extension = filepath.substr(extensionPos);
//	std::transform(extension.begin(), extension.end(), extension.begin(), tolower);
//
//	if (extension == ".gset")
//		return loadGeomSet(ctx, filepath);
//	if (extension == ".obj")
//		return loadMesh(ctx, filepath);
//
//	return false;
//}

//bool InputGeom::saveGeomSet(const BuildSettings* settings)
//{
//	if (!m_mesh) return false;
//	
//	// Change extension
//	std::string filepath = m_mesh->getFileName();
//	size_t extPos = filepath.find_last_of('.');
//	if (extPos != std::string::npos)
//		filepath = filepath.substr(0, extPos);
//
//	filepath += ".gset";
//
//	FILE* fp = fopen(filepath.c_str(), "w");
//	if (!fp) return false;
//	
//	// Store mesh filename.
//	fprintf(fp, "f %s\n", m_mesh->getFileName().c_str());
//
//	// Store settings if any
//	if (settings)
//	{
//		fprintf(fp,
//			"s %f %f %f %f %f %f %f %f %f %f %f %f %f %d %f %f %f %f %f %f %f\n",
//			settings->cellSize,
//			settings->cellHeight,
//			settings->agentHeight,
//			settings->agentRadius,
//			settings->agentMaxClimb,
//			settings->agentMaxSlope,
//			settings->regionMinSize,
//			settings->regionMergeSize,
//			settings->edgeMaxLen,
//			settings->edgeMaxError,
//			settings->vertsPerPoly,
//			settings->detailSampleDist,
//			settings->detailSampleMaxError,
//			settings->partitionType,
//			settings->navMeshBMin[0],
//			settings->navMeshBMin[1],
//			settings->navMeshBMin[2],
//			settings->navMeshBMax[0],
//			settings->navMeshBMax[1],
//			settings->navMeshBMax[2],
//			settings->tileSize);
//	}
//	
//	// Store off-mesh links.
//	for (int i = 0; i < m_offMeshConCount; ++i)
//	{
//		const float* v = &m_offMeshConVerts[i*3*2];
//		const float rad = m_offMeshConRads[i];
//		const int bidir = m_offMeshConDirs[i];
//		const int area = m_offMeshConAreas[i];
//		const int flags = m_offMeshConFlags[i];
//		fprintf(fp, "c %f %f %f  %f %f %f  %f %d %d %d\n",
//				v[0], v[1], v[2], v[3], v[4], v[5], rad, bidir, area, flags);
//	}
//
//	// Convex volumes
//	for (int i = 0; i < m_volumeCount; ++i)
//	{
//		ConvexVolume* vol = &m_volumes[i];
//		fprintf(fp, "v %d %d %f %f\n", vol->nverts, vol->area, vol->hmin, vol->hmax);
//		for (int j = 0; j < vol->nverts; ++j)
//			fprintf(fp, "%f %f %f\n", vol->verts[j*3+0], vol->verts[j*3+1], vol->verts[j*3+2]);
//	}
//	
//	fclose(fp);
//	
//	return true;
//}

static bool isectSegAABB(const float* sp, const float* sq,
						 const float* amin, const float* amax,
						 float& tmin, float& tmax)
{
	static const float EPS = 1e-6f;
	
	float d[3];
	d[0] = sq[0] - sp[0];
	d[1] = sq[1] - sp[1];
	d[2] = sq[2] - sp[2];
	tmin = 0.0;
	tmax = 1.0f;
	
	for (int i = 0; i < 3; i++)
	{
		if (fabsf(d[i]) < EPS)
		{
			if (sp[i] < amin[i] || sp[i] > amax[i])
				return false;
		}
		else
		{
			const float ood = 1.0f / d[i];
			float t1 = (amin[i] - sp[i]) * ood;
			float t2 = (amax[i] - sp[i]) * ood;
			if (t1 > t2) { float tmp = t1; t1 = t2; t2 = tmp; }
			if (t1 > tmin) tmin = t1;
			if (t2 < tmax) tmax = t2;
			if (tmin > tmax) return false;
		}
	}
	
	return true;
}


bool InputGeom::raycastMesh(float* src, float* dst, float& tmin)
{
	float dir[3];
	rcVsub(dir, dst, src);

	// Prune hit ray.
	float btmin, btmax;
	if (!isectSegAABB(src, dst, m_meshBMin, m_meshBMax, btmin, btmax))
		return false;
	float p[2], q[2];
	p[0] = src[0] + (dst[0]-src[0])*btmin;
	p[1] = src[2] + (dst[2]-src[2])*btmin;
	q[0] = src[0] + (dst[0]-src[0])*btmax;
	q[1] = src[2] + (dst[2]-src[2])*btmax;
	
	int cid[512];
	const int ncid = rcGetChunksOverlappingSegment(m_chunkyMesh, p, q, cid, 512);
	if (!ncid)
		return false;
	
	tmin = 1.0f;
	bool hit = false;
	const float* verts = m_mesh->getVerts();
	
	for (int i = 0; i < ncid; ++i)
	{
		const rcChunkyTriMeshNode& node = m_chunkyMesh->nodes[cid[i]];
		const int* tris = &m_chunkyMesh->tris[node.i*3];
		const int ntris = node.n;

		for (int j = 0; j < ntris*3; j += 3)
		{
			float t = 1;
			if (intersectSegmentTriangle(src, dst,
										 &verts[tris[j]*3],
										 &verts[tris[j+1]*3],
										 &verts[tris[j+2]*3], t))
			{
				if (t < tmin)
					tmin = t;
				hit = true;
			}
		}
	}
	
	return hit;
}

void InputGeom::addOffMeshConnection(const float* spos, const float* epos, const float rad,
									 unsigned char bidir, unsigned char area, unsigned short flags)
{
	if (m_offMeshConCount >= MAX_OFFMESH_CONNECTIONS) return;
	float* v = &m_offMeshConVerts[m_offMeshConCount*3*2];
	m_offMeshConRads[m_offMeshConCount] = rad;
	m_offMeshConDirs[m_offMeshConCount] = bidir;
	m_offMeshConAreas[m_offMeshConCount] = area;
	m_offMeshConFlags[m_offMeshConCount] = flags;
	m_offMeshConId[m_offMeshConCount] = 1000 + m_offMeshConCount;
	rcVcopy(&v[0], spos);
	rcVcopy(&v[3], epos);
	m_offMeshConCount++;
}

void InputGeom::deleteOffMeshConnection(int i)
{
	m_offMeshConCount--;
	float* src = &m_offMeshConVerts[m_offMeshConCount*3*2];
	float* dst = &m_offMeshConVerts[i*3*2];
	rcVcopy(&dst[0], &src[0]);
	rcVcopy(&dst[3], &src[3]);
	m_offMeshConRads[i] = m_offMeshConRads[m_offMeshConCount];
	m_offMeshConDirs[i] = m_offMeshConDirs[m_offMeshConCount];
	m_offMeshConAreas[i] = m_offMeshConAreas[m_offMeshConCount];
	m_offMeshConFlags[i] = m_offMeshConFlags[m_offMeshConCount];
}

void InputGeom::CSAddOffMeshConnection(v3dxVector3 startpos, v3dxVector3 endpos, float radius, int dir)
{
	const unsigned char area = SAMPLE_POLYAREA_JUMP;
	const unsigned short flags = SAMPLE_POLYFLAGS_JUMP;
	addOffMeshConnection(&startpos[0], &endpos[0], radius, dir == 0 ? 1 : 0, area, flags);
}

void InputGeom::addConvexVolume(const float* verts, const int nverts,
								const float minh, const float maxh, unsigned char area)
{
	if (m_volumeCount >= MAX_VOLUMES) return;
	ConvexVolume* vol = &m_volumes[m_volumeCount++];
	memset(vol, 0, sizeof(ConvexVolume));
	memcpy(vol->verts, verts, sizeof(float)*3*nverts);
	vol->hmin = minh;
	vol->hmax = maxh;
	vol->nverts = nverts;
	vol->area = area;
}

void InputGeom::deleteConvexVolume(int i)
{
	m_volumeCount--;
	m_volumes[i] = m_volumes[m_volumeCount];
}

NS_END
