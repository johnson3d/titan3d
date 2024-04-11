using System;
using System.Runtime.InteropServices;

namespace EngineNS
{
    public enum ContainmentType : int
	{
		/// <summary>
		/// The two bounding volumes don't intersect at all.
		/// </summary>
		Disjoint,

		/// <summary>
		/// One bounding volume completely contains another.
		/// </summary>
		Contains,

		/// <summary>
		/// The two bounding volumes overlap.
		/// </summary>
		Intersects
	};
	
	/// <summary>
	/// Describes the result of an intersection with a plane in three dimensions.
	/// </summary>
	public enum PlaneIntersectionType : int
	{
		/// <summary>
		/// The object is behind the plane.
		/// </summary>
		Back,

		/// <summary>
		/// The object is in front of the plane.
		/// </summary>
		Front,

		/// <summary>
		/// The object is intersecting the plane.
		/// </summary>
		Intersecting
	};

    public class IDllImportApi
    {
        const string ModuleNC = CoreSDK.CoreModule;

        public delegate int FTestCppCallDelegate(IntPtr item, int type);

        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl)]
        public extern static Int64 Test_CppCallDelegate(FTestCppCallDelegate cb, int count);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl)]
        public extern static int Test_CSharpCallCpp(int a, float b);

        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl)]
        public extern static IntPtr v3dxVec3TransformArray(IntPtr pOut, UInt32 OutStride, IntPtr pV, UInt32 VStride, IntPtr pM, UInt32 n);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl)]
        public unsafe extern static Vector3* v3dxVec3TransformNormalArray(Vector3* pOut, UInt32 OutStride, Vector3* pV, UInt32 VStride, Matrix* pM, UInt32 n);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl)]
        public unsafe extern static Matrix* v3dxMatrix4Inverse( Matrix *pOut, Matrix* pM, float *pDetSerminant);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl)]
        public unsafe extern static DMatrix* v3dxDMatrix4Inverse(DMatrix* pOut, DMatrix* pM, double* pDetSerminant); 
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl)]
        public unsafe extern static Matrix* v3dxMatrix4Transpose_extern(Matrix* pOut, Matrix* pM);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl)]
        public unsafe extern static void v3dxMatrix4NoRotation(Matrix* pOut, Matrix* pSrc);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl)]
        public unsafe extern static int v3dxMatrixDecompose(Vector3 *pOutScale,Quaternion *pOutRotation,Vector3 *pOutTranslation,Matrix *pM);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl)]
        public unsafe extern static void v3dxDMatrixDecompose(Vector3* pOutScale, Quaternion* pOutRotation, DVector3* pOutTranslation, DMatrix* pM);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl)]
        public unsafe extern static Matrix* v3dxMatrixMultiply(Matrix *pOut,Matrix *pM1,Matrix *pM2);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl)]
        public unsafe extern static Matrix* v3dxMatrixAffineTransformation(Matrix* pOut,float Scaling,Vector3 *pRotationCenter,Quaternion *pRotation,Vector3 *pTranslation);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl)]
        public unsafe extern static Vector3 * v3dxPlaneIntersectLine(Vector3 *pOut,Plane *pP,Vector3 *pV1,Vector3 *pV2);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl)]
        public unsafe extern static DVector3* v3dxDPlaneIntersectLine(DVector3* pOut, DPlane* pP, DVector3* pV1, DVector3* pV2);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl)]
        public unsafe extern static Plane* v3dxPlaneScale(Plane* pOut, Plane* pP, float s);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl)]
        public unsafe extern static void v3dxQuaternionToAxisAngle(Quaternion *pQ,Vector3 *pAxis,float *pAngle);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl)]
        public unsafe extern static Quaternion* v3dxQuaternionBaryCentric(Quaternion* pOut, Quaternion* pQ1, Quaternion* pQ2, Quaternion* pQ3, float f, float g);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl)]
        public unsafe extern static Quaternion * v3dxQuaternionExp(Quaternion *pOut,Quaternion *pQ);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl)]
        public unsafe extern static Quaternion* v3dxQuaternionLn(Quaternion* pOut, Quaternion* pQ);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl)]
        public unsafe extern static Quaternion * v3dxQuaternionRotationMatrix(Quaternion *pOut,Matrix *pM);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl)]
        public unsafe extern static Quaternion* v3dxQuaternionSquad(Quaternion* pOut, Quaternion* pQ1, Quaternion* pA, Quaternion* pB, Quaternion* pC, float t);
        //[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl)]
        //public unsafe extern static void D3DXQuaternionSquadSetup(Quaternion* pAOut, Quaternion* pBOut, Quaternion* pCOut, Quaternion* pQ0, Quaternion* pQ1, Quaternion* pQ2, Quaternion* pQ3);
        //[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl)]
        //public unsafe extern static Matrix * D3DXMatrixAffineTransformation2D(Matrix *pOut,float Scaling,Vector2 *pRotationCenter,float Rotation,Vector2 *pTranslation);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl)]
        public unsafe extern static Matrix* v3dxMatrixTransformationOrigin(Matrix* pOut, Vector3* pScaling, Quaternion* pRotation, Vector3* pTranslation);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl)]
        public unsafe extern static DMatrix* v3dxDMatrixTransformationOrigin(DMatrix* pOut, Vector3* pScaling, Quaternion* pRotation, DVector3* pTranslation);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl)]
        public unsafe extern static Matrix* v3dxMatrixTransformation(Matrix* pOut, Vector3* pScalingCenter, Quaternion* pScalingRotation, Vector3* pScaling, Vector3* pRotationCenter, Quaternion* pRotation, Vector3* pTranslation);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl)]
        public unsafe extern static DMatrix* v3dxDMatrixTransformation(DMatrix* pOut, DVector3* pScalingCenter, Quaternion* pScalingRotation, Vector3* pScaling, DVector3* pRotationCenter, Quaternion* pRotation, DVector3* pTranslation);
        //[System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl)]
        //public unsafe extern static Matrix* D3DXMatrixTransformation2D(Matrix* pOut, Vector2* pScalingCenter, float pScalingRotation, Vector2* pScaling, Vector2* pRotationCenter, float Rotation, Vector2* pTranslation);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl)]
        public unsafe extern static Matrix* v3dxMatrixLookAtLH(Matrix* pOut, Vector3* pCamera, Vector3* pAt, Vector3* pUp);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl)]
        public unsafe extern static Matrix* v3dxMatrix4Ortho(Matrix* pOut, float w, float h, float zn, float zf);        
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl)]
        public unsafe extern static Matrix* v3dxMatrix4Perspective(Matrix* pOut, float fovy, float Aspect, float zn, float zf);        
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl)]
        public unsafe extern static Vector3 * v3dxVec3TransformCoordArray(Vector3 *pOut,UInt32 OutStride,Vector3 *pV,UInt32 VStride,Matrix *pM,UInt32 n);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl)]
        public unsafe extern static int v3dxComputeBoundingSphere(Vector3* pFirstPosition, UInt32 NumVertices, UInt32 dwStride, Vector3* pCenter, float* pRadius);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl)]
        public unsafe extern static int v3dIntersectSegSeg(EngineNS.Vector3* ap, EngineNS.Vector3* aq, EngineNS.Vector3* bp, EngineNS.Vector3* bq,float* s, float* t);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl)]
        public unsafe extern static void v3dxMatrix4Mul_CSharp(Matrix* pOut, Matrix* mat1, Matrix* mat2);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl)]
        public unsafe extern static int v3dxIntersectTri(Vector3* p0, Vector3* p1, Vector3* p2, Vector3* pRayPos, Vector3* pRayDir, float* pU, float* pV, float* pDist);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl)]
        public unsafe extern static int v3dxIntersectDTri(DVector3* p0, DVector3* p1, DVector3* p2, DVector3* pRayPos, DVector3* pRayDir, double* pU, double* pV, double* pDist);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl)]
        public unsafe extern static int v3dxLineIntersectBox3(float* pfT_n,
                                                                 EngineNS.Vector3* pvPoint_n,
                                                                 float* pfT_f,
                                                                 EngineNS.Vector3* pvPoint_f,
                                                                 EngineNS.Vector3* pvFrom,
                                                                 EngineNS.Vector3* pvDir,
                                                                 EngineNS.BoundingBox* pBox);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl)]
        public unsafe extern static int v3dxLineIntersectDBox3(double* pfT_n,
                                                                 EngineNS.DVector3* pvPoint_n,
                                                                 double* pfT_f,
                                                                 EngineNS.DVector3* pvPoint_f,
                                                                 EngineNS.DVector3* pvFrom,
                                                                 EngineNS.DVector3* pvDir,
                                                                 EngineNS.DBoundingBox* pBox);
        [System.Runtime.InteropServices.DllImport(ModuleNC, CallingConvention = CallingConvention.Cdecl)]
        public unsafe extern static vBOOL v3dxLineIntersectPlane(float* pfT,
                                                                 Vector3* pvPoint,
                                                                 Vector3* pvFrom,
                                                                 Vector3* pvLength,
                                                                 Vector3* pvA,
                                                                 Vector3* pvB,
                                                                 Vector3* pvC,
                                                                 vBOOL bDoubleFace);
    }
}
