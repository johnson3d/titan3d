#include "FbxDataConverter.h"
#include "../2019.2/include/fbxsdk.h"


FbxDataConverter::FbxDataConverter()
{
}


FbxDataConverter::~FbxDataConverter()
{
}

FbxAMatrix FbxDataConverter::JointPostConversionMatrix;
FbxAMatrix FbxDataConverter::AxisConversionMatrix;
FbxAMatrix FbxDataConverter::AxisConversionMatrixInv;


v3dxVector2 FbxDataConverter::ConvertUV(FbxVector2& Vector)
{
	v3dxVector2 Out;
	Out.x = static_cast<float>(Vector[0]);
	Out.y = 1.0f - static_cast<float>(Vector[1]);
	return Out;
}

v3dxVector3 FbxDataConverter::ConvertPos(FbxVector4& Vector)
{
	v3dxVector3 Out;
	//Out[0] = (float)Vector[0];
	//// flip Y, then the right-handed axis system is converted to LHS
	//Out[1] = -(float)Vector[1];
	//Out[2] = (float)Vector[2];
	Out[0] = (float)Vector[0];
	Out[1] = (float)Vector[2];
	Out[2] = (float)Vector[1];
	return Out;
}

v3dxVector3 FbxDataConverter::ConvertPos(v3dxVector3& Vector)
{
	v3dxVector3 Out;
	Out[0] = (float)Vector[0];
	Out[1] = (float)Vector[2];
	Out[2] = (float)Vector[1];
	return Out;
}

v3dxVector3 FbxDataConverter::ConvertDir(FbxVector4& Vector)
{
	v3dxVector3 Out;
	//Out[0] = (float)Vector[0];
	//Out[1] = -(float)Vector[1];
	//Out[2] = (float)Vector[2];
	Out[0] = (float)Vector[0];
	Out[1] = (float)Vector[2];
	Out[2] = (float)Vector[1];
	return Out;
}


v3dxVector3 FbxDataConverter::ConvertDir(v3dxVector3& Vector)
{
	v3dxVector3 Out;
	Out[0] = (float)Vector[0];
	Out[1] = (float)Vector[2];
	Out[2] = (float)Vector[1];
	return Out;
}

v3dxVector3 FbxDataConverter::ConvertEuler(FbxDouble3& Euler)
{
	//it's wrong
	/*v3dxVector3 Out;
	Out[0] = (float)Euler[0];
	Out[1] = (float)Euler[2];
	Out[2] = (float)Euler[1];
	return Out;*/
	return v3dxVector3::ZERO;
}


v3dxVector3 FbxDataConverter::ConvertScale(FbxDouble3& Vector)
{
	v3dxVector3 Out;
	Out[0] = (float)Vector[0];
	Out[1] = (float)Vector[2];
	Out[2] = (float)Vector[1];
	return Out;
}


v3dxVector3 FbxDataConverter::ConvertScale(FbxVector4& Vector)
{
	v3dxVector3 Out;
	Out[0] = (float)Vector[0];
	Out[1] = (float)Vector[2];
	Out[2] = (float)Vector[1];
	return Out;
}


v3dxVector3 FbxDataConverter::ConvertScale(v3dxVector3& Vector)
{
	v3dxVector3 Out;
	Out[0] = (float)Vector[0];
	Out[1] = (float)Vector[2];
	Out[2] = (float)Vector[1];
	return Out;
}

//v3dxVector3 FbxDataConverter::ConvertRotation(FbxQuaternion Quaternion)
//{
//	v3dxVector3 Out(ConvertRotToQuat(Quaternion));
//	return Out;
//}
//
////-------------------------------------------------------------------------
////
////-------------------------------------------------------------------------
//v3dxVector3 FbxDataConverter::ConvertRotationToVect(FbxQuaternion Quaternion, bool bInvertOrient)
//{
//	v3dxQuaternion UnrealQuaternion = ConvertRotToQuat(Quaternion);
//	v3dxVector3 Euler;
//	Euler = UnrealQuaternion.Euler();
//	if (bInvertOrient)
//	{
//		Euler.Y = -Euler.Y;
//		Euler.Z = 180.f + Euler.Z;
//	}
//	return Euler;
//}

//-------------------------------------------------------------------------
//
//-------------------------------------------------------------------------
v3dxQuaternion FbxDataConverter::ConvertQuat(FbxQuaternion& Quaternion)
{
	v3dxQuaternion quat;
	quat.x = -(float)Quaternion[0];
	quat.y = -(float)Quaternion[2];
	quat.z = -(float)Quaternion[1];
	quat.w = (float)Quaternion[3];

	return quat;
}

//-------------------------------------------------------------------------
//
//-------------------------------------------------------------------------
float FbxDataConverter::ConvertDist(FbxDouble& Distance)
{
	float Out;
	Out = (float)Distance;
	return Out;
}

//FTransform FbxDataConverter::ConvertTransform(FbxAMatrix Matrix)
//{
//	FTransform Out;
//
//	v3dxQuaternion Rotation = ConvertRotToQuat(Matrix.GetQ());
//	v3dxVector3 Origin = ConvertPos(Matrix.GetT());
//	v3dxVector3 Scale = ConvertScale(Matrix.GetS());
//
//	Out.SetTranslation(Origin);
//	Out.SetScale3D(Scale);
//	Out.SetRotation(Rotation);
//
//	return Out;
//}

v3dxMatrix4 FbxDataConverter::ConvertMatrix(FbxAMatrix& Matrix)
{
	v3dxQuaternion flipRot = ConvertQuat(Matrix.GetQ());
	v3dxVector3 rot;
	v3dxYawPitchRollQuaternionRotation(flipRot, &rot);
	v3dxVector3 flipPos = ConvertPos(Matrix.GetT());
	v3dxVector3 flipSacle = ConvertScale(Matrix.GetS());
	v3dxMatrix4 result;
	result.makeTrans(flipPos, flipSacle, flipRot);
	return result;
}

v3dxColor4 FbxDataConverter::ConvertColor(FbxDouble3& Color)
{
	//Fbx is in linear color space
	v3dxColor4 SRGBColor = v3dxColor4((float)Color[0], (float)Color[1], (float)Color[2]);
	return SRGBColor;
}


v3dxColor4 FbxDataConverter::ConvertColor(FbxColor& Color)
{
	v3dxColor4 SRGBColor = v3dxColor4((float)Color.mRed, (float)Color.mGreen, (float)Color.mBlue, (float)Color.mAlpha);
	return SRGBColor;
}

FbxVector4 FbxDataConverter::ConvertToFbxPos(v3dxVector3& Vector)
{
	FbxVector4 Out;
	Out[0] = Vector[0];
	Out[1] = -Vector[1];
	Out[2] = Vector[2];

	return Out;
}

FbxVector4 FbxDataConverter::ConvertToFbxRot(v3dxVector3& Vector)
{
	FbxVector4 Out;
	Out[0] = Vector[0];
	Out[1] = -Vector[1];
	Out[2] = -Vector[2];

	return Out;
}

FbxVector4 FbxDataConverter::ConvertToFbxScale(v3dxVector3& Vector)
{
	FbxVector4 Out;
	Out[0] = Vector[0];
	Out[1] = Vector[1];
	Out[2] = Vector[2];

	return Out;
}

FbxDouble3 FbxDataConverter::ConvertToFbxColor(v3dxColor4& Color)
{
	//Fbx is in linear color space
	v3dxColor4 FbxLinearColor(Color);
	FbxDouble3 Out;
	Out[0] = FbxLinearColor.r;
	Out[1] = FbxLinearColor.g;
	Out[2] = FbxLinearColor.b;

	return Out;
}

FbxString FbxDataConverter::ConvertToFbxString(std::string& stdString)
{
	FbxString retStr = "";
	char * newStr = NULL;
	auto uftStr = VStringA_Gbk2Utf8(stdString.c_str());
	retStr = uftStr.c_str();
	return retStr;
}

FbxString FbxDataConverter::ConvertToFbxString(const char* string)
{
	FbxString retStr = "";
	char * newStr = NULL;
	auto uftStr = VStringA_Gbk2Utf8(string);
	retStr = uftStr.c_str();
	return retStr;
}

std::string FbxDataConverter::ConvertToStdString(FbxString& fbxString)
{
	std::string retStr(VStringA_Utf82Gbk(fbxString.Buffer()).c_str());
	return retStr;
}

std::string FbxDataConverter::ConvertToStdString(const char* string)
{
	std::string retStr (VStringA_Utf82Gbk(string).c_str());
	return retStr;
}

//FbxString FbxDataConverter::ConvertToFbxString(FName Name)
//{
//	FbxString OutString;
//
//	FString UnrealString;
//	Name.ToString(UnrealString);
//
//	OutString = TCHAR_TO_UTF8(*UnrealString);
//
//	return OutString;
//}
//
//FbxString FbxDataConverter::ConvertToFbxString(const FString& String)
//{
//	FbxString OutString;
//
//	OutString = TCHAR_TO_UTF8(*String);
//
//	return OutString;
//}
