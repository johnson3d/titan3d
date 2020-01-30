#pragma once
#include "../../../Graphics/GfxPreHead.h"
#include "../2019.2/include/fbxsdk.h"
class FbxDataConverter
{
public:
	FbxDataConverter();
	~FbxDataConverter();
public:
	static void SetJointPostConversionMatrix(FbxAMatrix ConversionMatrix) { JointPostConversionMatrix = ConversionMatrix; }
	static const FbxAMatrix &GetJointPostConversionMatrix() { return JointPostConversionMatrix; }

	static void SetAxisConversionMatrix(FbxAMatrix ConversionMatrix) { AxisConversionMatrix = ConversionMatrix; AxisConversionMatrixInv = ConversionMatrix.Inverse(); }
	static const FbxAMatrix &GetAxisConversionMatrix() { return AxisConversionMatrix; }
	static const FbxAMatrix &GetAxisConversionMatrixInv() { return AxisConversionMatrixInv; }

	static v3dxVector2 ConvertUV(FbxVector2& Vector);
	static v3dxVector3 ConvertPos(FbxVector4& Vector);
	static v3dxVector3 ConvertPos(v3dxVector3& Vector);
	static v3dxVector3 ConvertDir(FbxVector4& Vector);
	static v3dxVector3 ConvertDir(v3dxVector3& Vector);
	static v3dxVector3 ConvertEuler(FbxDouble3& Euler);
	static v3dxVector3 ConvertScale(FbxDouble3& Vector);
	static v3dxVector3 ConvertScale(v3dxVector3& Vector);
	static v3dxVector3 ConvertScale(FbxVector4& Vector);
	//static v3dxVector3 ConvertRotation(FbxQuaternion Quaternion);
	//static v3dxVector3 ConvertRotationToVect(FbxQuaternion Quaternion, bool bInvertRot);
	static v3dxQuaternion ConvertQuat(FbxQuaternion& Quaternion);
	static float ConvertDist(FbxDouble& Distance);
	//static bool ConvertPropertyValue(FbxProperty& FbxProperty, UProperty& UnrealProperty, union UPropertyValue& OutUnrealPropertyValue);
	//static FTransform ConvertTransform(FbxAMatrix Matrix);
	static v3dxMatrix4 ConvertMatrix(FbxAMatrix& Matrix);

	/*
	 * Convert fbx linear space color to sRGB v3dxColor4
	 */
	static v3dxColor4 ConvertColor(FbxDouble3& Color);
	static v3dxColor4 ConvertColor(FbxColor& Color);

	static FbxVector4 ConvertToFbxPos(v3dxVector3& Vector);
	static FbxVector4 ConvertToFbxRot(v3dxVector3& Vector);
	static FbxVector4 ConvertToFbxScale(v3dxVector3& Vector);

	/*
	* Convert sRGB v3dxColor4 to fbx linear space color
	*/
	static FbxDouble3   ConvertToFbxColor(v3dxColor4& Color);
	static FbxString	ConvertToFbxString(std::string& stdString);
	static FbxString	ConvertToFbxString(const char* string);
	static std::string	ConvertToStdString(FbxString& fbxString);
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


