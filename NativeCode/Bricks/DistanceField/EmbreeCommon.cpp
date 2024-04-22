#include "EmbreeCommon.h"
#include "perlin/perlin.h"

#if defined(PLATFORM_WIN)
#pragma comment(lib,"embree3.lib")
#pragma comment(lib,"tbb.lib")
#endif

#define  new VNEW

NS_BEGIN

void EmbreeFilterFunc(const struct RTCFilterFunctionNArguments* args)
{
	FEmbreeGeometry* EmbreeGeometry = (FEmbreeGeometry*)args->geometryUserPtr;
	FEmbreeTriangleDesc Desc = EmbreeGeometry->TriangleDescs[RTCHitN_primID(args->hit, 1, 0)];

	FEmbreeIntersectionContext& IntersectionContext = *static_cast<FEmbreeIntersectionContext*>(args->context);
	IntersectionContext.ElementIndex = Desc.ElementIndex;

	const RTCHit& EmbreeHit = *(RTCHit*)args->hit;
	if (IntersectionContext.SkipPrimId != RTC_INVALID_GEOMETRY_ID && IntersectionContext.SkipPrimId == EmbreeHit.primID)
	{
		// Ignore hit in order to continue tracing
		args->valid[0] = 0;
	}
}

void EmbreeErrorFunc(void* userPtr, RTCError code, const char* str)
{
	VFX_LTRACE(ELTT_Error, "Embree error: %s Code=%u", str, (UINT32)code);
}

void EmbreeManager::SetupEmbreeScene(VNameString meshName, NxRHI::FMeshDataProvider& meshProvider, float DistanceFieldResolutionScale, FEmbreeScene& embreeScene)
{
	//NxRHI::FMeshDataProvider meshProvider;
	//if (meshProvider.LoadFromMeshPrimitive(&meshPrimitive, NxRHI::EVertexStreamType::VST_FullMask) == false)
	//	return;

	UINT NumVertices = meshProvider.GetVertexNumber();
	UINT NumTriangles = meshProvider.GetPrimitiveNumber();
	UINT NumIndices = NumTriangles*3;
	embreeScene.NumIndices = NumTriangles;

	// 1, init environment 
	embreeScene.EmbreeDevice = rtcNewDevice(nullptr);
	rtcSetDeviceErrorFunction(embreeScene.EmbreeDevice, EmbreeErrorFunc, nullptr);
	RTCError ReturnErrorNewDevice = rtcGetDeviceError(embreeScene.EmbreeDevice);
	if (ReturnErrorNewDevice != RTC_ERROR_NONE)
	{
		VFX_LTRACE(ELTT_Warning, "GenerateSignedDistanceFieldVolumeData failed for %s. Embree rtcNewDevice failed. Code: %d", meshName.c_str(), (INT32)ReturnErrorNewDevice);
		return;
	}

	embreeScene.EmbreeScene = rtcNewScene(embreeScene.EmbreeDevice);
	rtcSetSceneFlags(embreeScene.EmbreeScene, RTC_SCENE_FLAG_NONE);
	RTCError ReturnErrorNewScene = rtcGetDeviceError(embreeScene.EmbreeDevice);
	if (ReturnErrorNewScene != RTC_ERROR_NONE)
	{
		VFX_LTRACE(ELTT_Warning, "GenerateSignedDistanceFieldVolumeData failed for %s. Embree rtcNewScene failed. Code: %d", meshName.c_str(), (INT32)ReturnErrorNewScene);
		rtcReleaseDevice(embreeScene.EmbreeDevice);
		return;
	}

	// 2, calculate valid triangles
	std::vector<INT32> FilteredTriangles;
	FilteredTriangles.reserve(NumTriangles);
	auto pPos = (v3dxVector3*)meshProvider.GetStream(NxRHI::VST_Position)->GetData();
	for (UINT32 triangleIndex = 0; triangleIndex < NumTriangles; ++triangleIndex)
	{
		UINT32 i0, i1, i2;
		if (meshProvider.GetTriangle(triangleIndex, &i0, &i1, &i2))
		{
			v3dxVector3 v0 = pPos[i0];
			v3dxVector3 v1 = pPos[i1];
			v3dxVector3 v2 = pPos[i2];

			v3dxVector3 triangleNormal = (v0 - v2).crossProduct(v1 - v2);
			bool bDegenerateTriangle = triangleNormal.getLengthSq() < SMALL_EPSILON;
			if (!bDegenerateTriangle)
				FilteredTriangles.push_back(triangleIndex);
		}
	}

	// 3, fill RTCGeometry(vertices,indices,triangleDesc) in embreeScene
	const INT32 NumBufferVerts = 1; // Reserve extra space at the end of the array, as embree has an internal bug where they read and discard 4 bytes off the end of the array
	embreeScene.Geometry.VertexArray.resize(NumVertices + NumBufferVerts);

	const auto NumFilteredIndices = FilteredTriangles.size() * 3;

	embreeScene.Geometry.IndexArray.resize(NumFilteredIndices);

	v3dxVector3* EmbreeVertices = embreeScene.Geometry.VertexArray.data();
	UINT32* EmbreeIndices = embreeScene.Geometry.IndexArray.data();
	embreeScene.Geometry.TriangleDescs.reserve(FilteredTriangles.size());

	for (INT32 FilteredTriangleIndex = 0; FilteredTriangleIndex < FilteredTriangles.size(); FilteredTriangleIndex++)
	{
		UINT32 I0, I1, I2;
		v3dxVector3 V0, V1, V2;

		const INT32 TriangleIndex = FilteredTriangles[FilteredTriangleIndex];
		if (meshProvider.GetTriangle(TriangleIndex, &I0, &I1, &I2))
		{
			V0 = pPos[I0];
			V1 = pPos[I1];
			V2 = pPos[I2];
		}

		// TODO calculate bTriangleIsTwoSided from materials
		bool bTriangleIsTwoSided = false;

		EmbreeIndices[FilteredTriangleIndex * 3 + 0] = I0;
		EmbreeIndices[FilteredTriangleIndex * 3 + 1] = I1;
		EmbreeIndices[FilteredTriangleIndex * 3 + 2] = I2;

		EmbreeVertices[I0] = V0;
		EmbreeVertices[I1] = V1;
		EmbreeVertices[I2] = V2;

		FEmbreeTriangleDesc Desc;
		// Store bGenerateAsIfTwoSided in material index
		bool bGenerateAsIfTwoSided = false;
		Desc.ElementIndex = bGenerateAsIfTwoSided || bTriangleIsTwoSided ? 1 : 0;
		embreeScene.Geometry.TriangleDescs.push_back(Desc);
	}

	RTCGeometry Geometry = rtcNewGeometry(embreeScene.EmbreeDevice, RTC_GEOMETRY_TYPE_TRIANGLE);
	embreeScene.Geometry.InternalGeometry = Geometry;

	rtcSetSharedGeometryBuffer(Geometry, RTC_BUFFER_TYPE_VERTEX, 0, RTC_FORMAT_FLOAT3, EmbreeVertices, 0, sizeof(v3dxVector3), NumVertices);
	rtcSetSharedGeometryBuffer(Geometry, RTC_BUFFER_TYPE_INDEX, 0, RTC_FORMAT_UINT3, EmbreeIndices, 0, sizeof(UINT32) * 3, FilteredTriangles.size());

	rtcSetGeometryUserData(Geometry, &embreeScene.Geometry);
	rtcSetGeometryIntersectFilterFunction(Geometry, EmbreeFilterFunc);

	rtcCommitGeometry(Geometry);
	rtcAttachGeometry(embreeScene.EmbreeScene, Geometry);
	rtcReleaseGeometry(Geometry);

	rtcCommitScene(embreeScene.EmbreeScene);

	RTCError ReturnError = rtcGetDeviceError(embreeScene.EmbreeDevice);
	if (ReturnError != RTC_ERROR_NONE)
	{
		VFX_LTRACE(ELTT_Warning, "GenerateSignedDistanceFieldVolumeData failed for %s. Embree rtcCommitScene failed. Code: %d", meshName.c_str(), (INT32)ReturnError);
		return;
	}
}

void EmbreeManager::DeleteEmbreeScene(FEmbreeScene& embreeScene)
{
	rtcReleaseScene(embreeScene.EmbreeScene);
	rtcReleaseDevice(embreeScene.EmbreeDevice);
}

static v3dxVector3 _UniformSampleHemisphere(v3dxVector2 Uniforms)
{
	Uniforms = Uniforms * 2.0f - 1.0f;

	if (Uniforms == v3dxVector2::Zero)
	{
		return v3dxVector3::ZERO;
	}

	float R;
	float Theta;

	if (Math::Abs(Uniforms.X) > Math::Abs(Uniforms.Y))
	{
		R = Uniforms.X;
		Theta = (float)Math::V3_PI / 4 * (Uniforms.Y / Uniforms.X);
	}
	else
	{
		R = Uniforms.Y;
		Theta = (float)Math::V3_PI / 2 - (float)Math::V3_PI / 4 * (Uniforms.X / Uniforms.Y);
	}

	// concentric disk sample
	const float U = R * Math::Cos(Theta);
	const float V = R * Math::Sin(Theta);
	const float R2 = R * R;

	// map to hemisphere [P. Shirley, Kenneth Chiu; 1997; A Low Distortion Map Between Disk and Square]
	return v3dxVector3(U * Math::Sqrt(2 - R2), V * Math::Sqrt(2 - R2), 1.0f - R2);
}

void GenerateStratifiedUniformHemisphereSamples(INT32 NumSamples, std::vector<v3dxVector3>& Samples)
{
	const INT32 NumSamplesDim = (INT32)(Math::Sqrt((float)NumSamples));

	Samples.reserve(NumSamplesDim * NumSamplesDim);
	vfxRandom random;
	random.SetSeed(0);

	for (INT32 IndexX = 0; IndexX < NumSamplesDim; IndexX++)
	{
		for (INT32 IndexY = 0; IndexY < NumSamplesDim; IndexY++)
		{			
			const float U1 = ((float)random.NextValue16Bit()) / (float)USHRT_MAX;
			const float U2 = ((float)random.NextValue16Bit()) / (float)USHRT_MAX;

			const float Fraction1 = (IndexX + U1) / (float)NumSamplesDim;
			const float Fraction2 = (IndexY + U2) / (float)NumSamplesDim;

			v3dxVector3 Tmp = _UniformSampleHemisphere(v3dxVector2(Fraction1, Fraction2));

			// Workaround issue with compiler optimization by using copy constructor here.
			Samples.push_back(v3dxVector3(Tmp));
		}
	}
}


NS_END