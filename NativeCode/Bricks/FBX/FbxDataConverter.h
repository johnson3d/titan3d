#pragma once
#include "fbxsdk.h"
#include <v3dxVector2.h>
#include "v3dxVector3.h"
#include "v3dxQuaternion.h"
#include "v3dxMatrix4.h"
#include "v3dxColor4.h"
class FBXDataConverter
{
public:
	FBXDataConverter();
	~FBXDataConverter();
public:
	static void SetJointPostConversionMatrix(FbxAMatrix ConversionMatrix) { JointPostConversionMatrix = ConversionMatrix; }
	static const FbxAMatrix &GetJointPostConversionMatrix() { return JointPostConversionMatrix; }

	static void SetAxisConversionMatrix(FbxAMatrix ConversionMatrix) { AxisConversionMatrix = ConversionMatrix; AxisConversionMatrixInv = ConversionMatrix.Inverse(); }
	static const FbxAMatrix &GetAxisConversionMatrix() { return AxisConversionMatrix; }
	static const FbxAMatrix &GetAxisConversionMatrixInv() { return AxisConversionMatrixInv; }

	static v3dxVector2 ConvertUV(const FbxVector2& Vector);
	static v3dxVector3 ConvertPos(const FbxVector4& Vector);
	static v3dxVector3 ConvertPos(const v3dxVector3& Vector);
	static v3dxVector3 ConvertDir(const FbxVector4& Vector);
	static v3dxVector3 ConvertDir(const v3dxVector3& Vector);
	static v3dxVector3 ConvertEuler(const FbxDouble3& Euler);
	static v3dxVector3 ConvertScale(const FbxDouble3& Vector);
	static v3dxVector3 ConvertScale(const v3dxVector3& Vector);
	static v3dxVector3 ConvertScale(const FbxVector4& Vector);
	//static v3dxVector3 ConvertRotation(FbxQuaternion Quaternion);
	//static v3dxVector3 ConvertRotationToVect(FbxQuaternion Quaternion, bool bInvertRot);
	static v3dxQuaternion ConvertQuat(const FbxQuaternion& Quaternion);
	static float ConvertDist(const FbxDouble& Distance);
	//static bool ConvertPropertyValue(FbxProperty& FbxProperty, UProperty& UnrealProperty, union UPropertyValue& OutUnrealPropertyValue);
	//static FTransform ConvertTransform(FbxAMatrix Matrix);
	static v3dxMatrix4 ConvertMatrix(const FbxAMatrix& Matrix);

	/*
	 * Convert fbx linear space color to sRGB v3dxColor4
	 */
	static v3dxColor4 ConvertColor(const FbxDouble3& Color);
	static v3dxColor4 ConvertColor(const FbxColor& Color);

	static FbxVector4 ConvertToFbxPos(const v3dxVector3& Vector);
	static FbxVector4 ConvertToFbxRot(const v3dxVector3& Vector);
	static FbxVector4 ConvertToFbxScale(const v3dxVector3& Vector);

	/*
	* Convert sRGB v3dxColor4 to fbx linear space color
	*/
	static FbxDouble3   ConvertToFbxColor(const v3dxColor4& Color);
	static FbxString	ConvertToFbxString(const std::string& stdString);
	static FbxString	ConvertToFbxString(const char* string);
	static std::string	ConvertToStdString(const FbxString& fbxString);
	static std::string	ConvertToStdString(const char* string);
	// FbxCamera with no rotation faces X with Y-up while ours faces X with Z-up so add a -90 degrees roll to compensate
	//static v3dxVector3 GetCameraRotation() { return v3dxVector3(0.f, 0.f, -90.f); }

	// FbxLight with no rotation faces -Z while ours faces Y so add a 90 degrees pitch to compensate
	//static v3dxVector3 GetLightRotation() { return v3dxVector3(0.f, 90.f, 0.f); }

private:
	static FbxAMatrix JointPostConversionMatrix;
	static FbxAMatrix AxisConversionMatrix;
	static FbxAMatrix AxisConversionMatrixInv;
};


