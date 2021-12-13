using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS
{
    [TypeConverter]
    public struct FTransform
    {
        public static FTransform IdentityForRef = FTransform.CreateTransform(in DVector3.Zero, in Vector3.UnitXYZ, in Quaternion.Identity);
        public readonly static FTransform Identity = FTransform.CreateTransform(in DVector3.Zero, in Vector3.UnitXYZ, in Quaternion.Identity);
        public FTransform(in FTransform trans)
        {
            mPosition = trans.mPosition;
            mScale = trans.mScale;
            mQuat = trans.mQuat;
        }
        public class TypeConverterAttribute : Support.TypeConverterAttributeBase
        {
            public override bool ConvertFromString(ref object obj, string text)
            {
                FTransform result = new FTransform();
                int cur = 0;
                var posStr = GetMatchPair(text, ref cur, '(', ')');
                if (posStr == null)
                    return false;
                var segs = posStr.Split(',');
                result.mPosition = new DVector3(System.Convert.ToDouble(segs[0]),
                    System.Convert.ToDouble(segs[1]),
                    System.Convert.ToDouble(segs[2]));
                var scaleStr = GetMatchPair(text, ref cur, '(', ')');
                if (scaleStr == null)
                    return false;
                segs = scaleStr.Split(',');
                result.mScale = new Vector3(System.Convert.ToSingle(segs[0]),
                    System.Convert.ToSingle(segs[1]),
                    System.Convert.ToSingle(segs[2]));
                var quatStr = GetMatchPair(text, ref cur, '(', ')');
                if (quatStr == null)
                    return false;
                segs = quatStr.Split(',');
                result.mQuat = new Quaternion(System.Convert.ToSingle(segs[0]),
                    System.Convert.ToSingle(segs[1]),
                    System.Convert.ToSingle(segs[2]),
                    System.Convert.ToSingle(segs[3]));

                obj = result;
                return true;
            }
        }

        public override string ToString()
        {
            return $"[Position({mPosition.ToString()}),Scale({mScale.ToString()}),Quat({mQuat.ToString()})]";
        }
        public void InitData()
        {
            mPosition = DVector3.Zero;
            mScale = Vector3.One;
            mQuat = Quaternion.Identity;
        }
        public static FTransform CreateTransform(in DVector3 pos, in Vector3 scale, in Quaternion quat)
        {
            FTransform tmp;
            tmp.mPosition = pos;
            tmp.mScale = scale;
            tmp.mQuat = quat;
            return tmp;
        }
        public bool HasScale
        {
            get
            {
                return Vector3.Equals(in mScale, in Vector3.One) == false;
            }
        }
        public bool IsIdentity
        {
            get
            {
                if (Vector3.Equals(in mScale, in Vector3.One) == false)
                    return false;
                if (DVector3.Equals(in mPosition, in DVector3.Zero) == false)
                    return false;
                if (Quaternion.Equals(in mQuat, in Quaternion.Identity) == false)
                    return false;
                return true;
            }
        }
        public DVector3 mPosition;
        public Vector3 mScale;//为了Hierarchical计算方便，我们设定mScale在Transform中只影响本节点而不传递，如果需要整体放缩，在Node上新增一个ScaleMatrix
        public Quaternion mQuat;

        public DVector3 Position { get => mPosition; set => mPosition = value; }
        public Vector3 Scale { get => mScale; set => mScale = value; }
        public Quaternion Quat { get => mQuat; set => mQuat = value; }

        #region Matrix
        public DMatrix ToDMatrixWithScale()
        {
            DMatrix OutMatrix;

            OutMatrix.M41 = mPosition.X;
            OutMatrix.M42 = mPosition.Y;
            OutMatrix.M43 = mPosition.Z;

            var x2 = mQuat.X + mQuat.X;
            var y2 = mQuat.Y + mQuat.Y;
            var z2 = mQuat.Z + mQuat.Z;
            {
                var xx2 = mQuat.X * x2;
                var yy2 = mQuat.Y * y2;
                var zz2 = mQuat.Z * z2;

                OutMatrix.M11 = (1.0f - (yy2 + zz2)) * mScale.X;
                OutMatrix.M22 = (1.0f - (xx2 + zz2)) * mScale.Y;
                OutMatrix.M33 = (1.0f - (xx2 + yy2)) * mScale.Z;
            }
            {
                var yz2 = mQuat.Y * z2;
                var wx2 = mQuat.W * x2;

                OutMatrix.M32 = (yz2 - wx2) * mScale.Z;
                OutMatrix.M23 = (yz2 + wx2) * mScale.Y;
            }
            {
                var xy2 = mQuat.X * y2;
                var wz2 = mQuat.W * z2;

                OutMatrix.M21 = (xy2 - wz2) * mScale.Y;
                OutMatrix.M12 = (xy2 + wz2) * mScale.X;
            }
            {
                var xz2 = mQuat.X * z2;
                var wy2 = mQuat.W * y2;

                OutMatrix.M31 = (xz2 + wy2) * mScale.Z;
                OutMatrix.M13 = (xz2 - wy2) * mScale.X;
            }

            OutMatrix.M14 = 0.0f;
            OutMatrix.M24 = 0.0f;
            OutMatrix.M34 = 0.0f;
            OutMatrix.M44 = 1.0f;

            return OutMatrix;
        }
        public DMatrix ToDMatrixNoScale()
        {
            DMatrix OutMatrix;

            OutMatrix.M41 = mPosition.X;
            OutMatrix.M42 = mPosition.Y;
            OutMatrix.M43 = mPosition.Z;

            var x2 = mQuat.X + mQuat.X;
            var y2 = mQuat.Y + mQuat.Y;
            var z2 = mQuat.Z + mQuat.Z;
            {
                var xx2 = mQuat.X * x2;
                var yy2 = mQuat.Y * y2;
                var zz2 = mQuat.Z * z2;

                OutMatrix.M11 = (1.0f - (yy2 + zz2));
                OutMatrix.M22 = (1.0f - (xx2 + zz2));
                OutMatrix.M33 = (1.0f - (xx2 + yy2));
            }
            {
                var yz2 = mQuat.Y * z2;
                var wx2 = mQuat.W * x2;

                OutMatrix.M32 = (yz2 - wx2);
                OutMatrix.M23 = (yz2 + wx2);
            }
            {
                var xy2 = mQuat.X * y2;
                var wz2 = mQuat.W * z2;

                OutMatrix.M21 = (xy2 - wz2);
                OutMatrix.M12 = (xy2 + wz2);
            }
            {
                var xz2 = mQuat.X * z2;
                var wy2 = mQuat.W * y2;

                OutMatrix.M31 = (xz2 + wy2);
                OutMatrix.M13 = (xz2 - wy2);
            }

            OutMatrix.M14 = 0.0f;
            OutMatrix.M24 = 0.0f;
            OutMatrix.M34 = 0.0f;
            OutMatrix.M44 = 1.0f;

            return OutMatrix;
        }

        public Matrix ToMatrixWithScale(in DVector3 Offset)
        {
            Matrix OutMatrix;

            OutMatrix.M41 = (float)(mPosition.X - Offset.X);
            OutMatrix.M42 = (float)(mPosition.Y - Offset.Y);
            OutMatrix.M43 = (float)(mPosition.Z - Offset.Z);

            var x2 = mQuat.X + mQuat.X;
            var y2 = mQuat.Y + mQuat.Y;
            var z2 = mQuat.Z + mQuat.Z;
            {
                var xx2 = mQuat.X * x2;
                var yy2 = mQuat.Y * y2;
                var zz2 = mQuat.Z * z2;

                OutMatrix.M11 = (1.0f - (yy2 + zz2)) * mScale.X;
                OutMatrix.M22 = (1.0f - (xx2 + zz2)) * mScale.Y;
                OutMatrix.M33 = (1.0f - (xx2 + yy2)) * mScale.Z;
            }
            {
                var yz2 = mQuat.Y * z2;
                var wx2 = mQuat.W * x2;

                OutMatrix.M32 = (yz2 - wx2) * mScale.Z;
                OutMatrix.M23 = (yz2 + wx2) * mScale.Y;
            }
            {
                var xy2 = mQuat.X * y2;
                var wz2 = mQuat.W * z2;

                OutMatrix.M21 = (xy2 - wz2) * mScale.Y;
                OutMatrix.M12 = (xy2 + wz2) * mScale.X;
            }
            {
                var xz2 = mQuat.X * z2;
                var wy2 = mQuat.W * y2;

                OutMatrix.M31 = (xz2 + wy2) * mScale.Z;
                OutMatrix.M13 = (xz2 - wy2) * mScale.X;
            }

            OutMatrix.M14 = 0.0f;
            OutMatrix.M24 = 0.0f;
            OutMatrix.M34 = 0.0f;
            OutMatrix.M44 = 1.0f;

            return OutMatrix;
        }
        public Matrix ToMatrixNoScale(in DVector3 Offset)
        {
            Matrix OutMatrix;

            OutMatrix.M41 = (float)(mPosition.X - Offset.X);
            OutMatrix.M42 = (float)(mPosition.Y - Offset.Y);
            OutMatrix.M43 = (float)(mPosition.Z - Offset.Z);

            var x2 = mQuat.X + mQuat.X;
            var y2 = mQuat.Y + mQuat.Y;
            var z2 = mQuat.Z + mQuat.Z;
            {
                var xx2 = mQuat.X * x2;
                var yy2 = mQuat.Y * y2;
                var zz2 = mQuat.Z * z2;

                OutMatrix.M11 = (1.0f - (yy2 + zz2));
                OutMatrix.M22 = (1.0f - (xx2 + zz2));
                OutMatrix.M33 = (1.0f - (xx2 + yy2));
            }
            {
                var yz2 = mQuat.Y * z2;
                var wx2 = mQuat.W * x2;

                OutMatrix.M32 = (yz2 - wx2);
                OutMatrix.M23 = (yz2 + wx2);
            }
            {
                var xy2 = mQuat.X * y2;
                var wz2 = mQuat.W * z2;

                OutMatrix.M21 = (xy2 - wz2);
                OutMatrix.M12 = (xy2 + wz2);
            }
            {
                var xz2 = mQuat.X * z2;
                var wy2 = mQuat.W * y2;

                OutMatrix.M31 = (xz2 + wy2);
                OutMatrix.M13 = (xz2 - wy2);
            }

            OutMatrix.M14 = 0.0f;
            OutMatrix.M24 = 0.0f;
            OutMatrix.M34 = 0.0f;
            OutMatrix.M44 = 1.0f;

            return OutMatrix;
        }
        #endregion

        #region Transform
        public static void Multiply(out FTransform OutTransform, in FTransform A, in FTransform B)
        {
            if (A.mScale.HasNagative() || B.mScale.HasNagative())
            {
                // @note, if you have 0 scale with negative, you're going to lose rotation as it can't convert back to quat
                MultiplyUsingMatrixWithScale(out OutTransform, in A, in B);
            }
            else
            {
                OutTransform.mQuat = A.mQuat * B.mQuat;
                OutTransform.mQuat.Normalize();
                OutTransform.mScale = A.mScale * B.mScale;
                OutTransform.mPosition = B.mQuat * (A.mPosition * B.mScale) + B.mPosition;
            }
            //var mtx1 = A.ToMatrixWithScale();
            //var mtx2 = B.ToMatrixWithScale();
            //var mtx = mtx1 * mtx2;
            //mtx.Decompose(out OutTransform.mScale, out OutTransform.mQuat, out OutTransform.mPosition);
        }
        public static void MultiplyNoParentScale(out FTransform OutTransform, in FTransform A, in FTransform B)
        {
            OutTransform.mQuat = A.mQuat * B.mQuat;
            OutTransform.mQuat.Normalize();
            OutTransform.mScale = A.mScale;
            OutTransform.mPosition = B.mQuat * (A.mPosition) + B.mPosition;

            //var mtx1 = A.ToMatrixWithScale();
            //var mtx2 = B.ToMatrixNoScale();
            //var mtx = mtx1 * mtx2;
            //mtx.Decompose(out OutTransform.mScale, out OutTransform.mQuat, out OutTransform.mPosition);
        }
        public static void MultiplyUsingMatrixWithScale(out FTransform OutTransform, in FTransform A, in FTransform B)
        {
            ConstructTransformFromMatrixWithDesiredScale(A.ToDMatrixWithScale(), B.ToDMatrixWithScale(), A.mScale * B.mScale, out OutTransform);
        }

        public FTransform Inverse()
        {
            var InvRotation = mQuat.Inverse();
            // this used to cause NaN if Scale contained 0 
            var InvScale3D = GetSafeScaleReciprocal(mScale);
            var InvTranslation = InvRotation * ((-mPosition) * InvScale3D);

            return CreateTransform(InvTranslation, InvScale3D, InvRotation);
        }
        public void Blend(in FTransform Atom1, in FTransform Atom2, float Alpha)
        {
            if (Alpha <= CoreDefine.Epsilon)
            {
                // if blend is all the way for child1, then just copy its bone atoms
                this = Atom1;
            }
            else if (Alpha >= 1.0f - CoreDefine.Epsilon)
            {
                // if blend is all the way for child2, then just copy its bone atoms
                this = Atom2;
            }
            else
            {
                // Simple linear interpolation for translation and scale.
                mPosition = DVector3.Lerp(Atom1.mPosition, Atom2.mPosition, Alpha);
                mScale = Vector3.Lerp(Atom1.mScale, Atom2.mScale, Alpha);
                mQuat = Quaternion.Lerp(Atom1.mQuat, Atom2.mQuat, Alpha);

                // ..and renormalize
                mQuat.Normalize();
            }
        }
        public void BlendWith(in FTransform OtherAtom, float Alpha)
        {
            if (Alpha > CoreDefine.Epsilon)
            {
                if (Alpha >= 1.0f - CoreDefine.Epsilon)
                {
                    // if blend is all the way for child2, then just copy its bone atoms
                    this = OtherAtom;
                }
                else
                {
                    // Simple linear interpolation for translation and scale.
                    mPosition = DVector3.Lerp(mPosition, OtherAtom.mPosition, Alpha);
                    mScale = Vector3.Lerp(mScale, OtherAtom.mScale, Alpha);
                    mQuat = Quaternion.Lerp(mQuat, OtherAtom.mQuat, Alpha);

                    // ..and renormalize
                    mQuat.Normalize();
                }
            }
        }

        public void GetRelativeTransformUsingMatrixWithScale(out FTransform OutTransform, in FTransform Base, in FTransform Relative)
        {
            // the goal of using M is to get the correct orientation
            // but for translation, we still need scale
            var AM = Base.ToDMatrixWithScale();
            var BM = Relative.ToDMatrixWithScale();
            // get combined scale
            var SafeRecipScale3D = GetSafeScaleReciprocal(in Relative.mScale);
            var DesiredScale3D = Base.mScale * SafeRecipScale3D;
            BM.Inverse();
            ConstructTransformFromMatrixWithDesiredScale(in AM, in BM, in DesiredScale3D, out OutTransform);
        }
        public FTransform GetRelativeTransform(in FTransform Other)
        {
            // A * B(-1) = VQS(B)(-1) (VQS (A))
            // 
            // Scale = S(A)/S(B)
            // Rotation = Q(B)(-1) * Q(A)
            // Translation = 1/S(B) *[Q(B)(-1)*(T(A)-T(B))*Q(B)]
            // where A = this, B = Other
            FTransform Result = new FTransform();

	        if (mScale.HasNagative() || Other.mScale.HasNagative())
	        {
		        // @note, if you have 0 scale with negative, you're going to lose rotation as it can't convert back to quat
		        GetRelativeTransformUsingMatrixWithScale(out Result, in this, in Other);
            }
	        else
	        {
		        Vector3 SafeRecipScale3D = GetSafeScaleReciprocal(in Other.mScale);
                Result.mScale = mScale * SafeRecipScale3D;

		        if (Other.mQuat.IsNormalized() == false)
		        {
			        return FTransform.Identity;
		        }

                var Inverse = Other.mQuat.Inverse();
                Result.mQuat = Inverse * mQuat;

                DVector3 dist = mPosition - Other.mPosition;
                DVector3 tmp = (Inverse * dist);
                Result.mPosition = tmp * SafeRecipScale3D;
	        }

	        return Result;
        }
        public FTransform GetRelativeTransformReverse(in FTransform Other)
        {
            // A (-1) * B = VQS(B)(VQS (A)(-1))
            // 
            // Scale = S(B)/S(A)
            // Rotation = Q(B) * Q(A)(-1)
            // Translation = T(B)-S(B)/S(A) *[Q(B)*Q(A)(-1)*T(A)*Q(A)*Q(B)(-1)]
            // where A = this, and B = Other
            FTransform Result = new FTransform();

            var SafeRecipScale3D = GetSafeScaleReciprocal(in mScale);
            Result.mScale = Other.mScale * SafeRecipScale3D;

            Result.mQuat = Other.mQuat * mQuat.Inverse();

            Result.mPosition = Other.mPosition - (Result.mQuat * mPosition) * Result.mScale;

	        return Result;
        }
        public void SetToRelativeTransform(in FTransform ParentTransform)
        {
	        // A * B(-1) = VQS(B)(-1) (VQS (A))
	        // 
	        // Scale = S(A)/S(B)
	        // Rotation = Q(B)(-1) * Q(A)
	        // Translation = 1/S(B) *[Q(B)(-1)*(T(A)-T(B))*Q(B)]
	        // where A = this, B = Other

	        var SafeRecipScale3D = GetSafeScaleReciprocal(ParentTransform.mScale);
            var InverseRot = ParentTransform.mQuat.Inverse();

            mScale *= SafeRecipScale3D;	
	        mPosition = (InverseRot * (mPosition - ParentTransform.mPosition)) * SafeRecipScale3D;
	        mQuat = InverseRot * mQuat;
        }
        #endregion
        
        #region transform Vector
        public DVector4 TransformVector4(in DVector4 V)
        {
            //Transform using QST is following
            //QST(P) = Q*S*P*-Q + T where Q = quaternion, S = scale, T = translation

            var Transform = new DVector4(Quaternion.RotateVector3(mQuat, V.AsVector3() * mScale), 0.0f);
            if (V.W == 1.0f)
            {
                Transform += new DVector4(mPosition, 1.0f);
            }

            return Transform;
        }
        public Vector3 TransformVector3(in Vector3 V)
        {
            return Quaternion.RotateVector3(mQuat, mScale * V);
        }
        public Vector3 TransformVector3NoScale(in Vector3 V)
        {
            return Quaternion.RotateVector3(mQuat, V);
        }
        public Vector3 InverseTransformVector3(in Vector3 V)
        {
            return Quaternion.RotateVector3(mQuat, V) * GetSafeScaleReciprocal(mScale);
        }
        public Vector3 InverseTransformVector3NoScale(in Vector3 V)
        {
            return Quaternion.RotateVector3(mQuat, V);
        }
        public DVector3 TransformPosition(in DVector3 V)
        {
            return Quaternion.RotateVector3(mQuat, V * mScale) + mPosition;
        }
        public DVector3 TransformPositionNoScale(in DVector3 V)
        {
            return Quaternion.RotateVector3(mQuat, V) + mPosition;
        }
        public DVector3 InverseTransformPosition(in DVector3 V)
        {
            return Quaternion.UnrotateVector3(mQuat, V - mPosition) * GetSafeScaleReciprocal(mScale);
        }
        public DVector3 InverseTransformPositionNoScale(in DVector3 V)
        {
            return Quaternion.UnrotateVector3(mQuat, V - mPosition);
        }
        public Quaternion TransformRotation(in Quaternion Q)
        {
            return mQuat * Q;
        }
        public Quaternion InverseTransformRotation(in Quaternion Q)
        {
            var invQuat = mQuat.Inverse();
            return invQuat * Q;
        }
        #endregion

        #region Scale
        public Vector3 GetSafeScaleReciprocal(in Vector3 InScale, float Tolerance = CoreDefine.Epsilon)
        {
            Vector3 SafeReciprocalScale;
            if (Math.Abs(InScale.X) <= Tolerance)
            {
                SafeReciprocalScale.X = 0.0f;
            }
            else
            {
                SafeReciprocalScale.X = 1 / InScale.X;
            }

            if (Math.Abs(InScale.Y) <= Tolerance)
            {
                SafeReciprocalScale.Y = 0.0f;
            }
            else
            {
                SafeReciprocalScale.Y = 1 / InScale.Y;
            }

            if (Math.Abs(InScale.Z) <= Tolerance)
            {
                SafeReciprocalScale.Z = 0.0f;
            }
            else
            {
                SafeReciprocalScale.Z = 1 / InScale.Z;
            }

            return SafeReciprocalScale;
        }
        public FTransform GetScaled(float InScale)
        {
            var A = new FTransform(this);
            A.mScale *= InScale;

	        return A;
        }
        public Vector3 GetScaledAxis(Matrix.EAxisType InAxis)
        {
	        if (InAxis == Matrix.EAxisType.X)
	        {
		        return TransformVector3(new Vector3(1.0f, 0.0f, 0.0f));
	        }
	        else if (InAxis == Matrix.EAxisType.Y)
	        {
		        return TransformVector3(new Vector3(0.0f, 1.0f, 0.0f));
	        }

            return TransformVector3(new Vector3(0.0f, 0.0f, 1.0f));
        }
        public Vector3 GetScaledAxisNoScale(Matrix.EAxisType InAxis)
        {
            if (InAxis == Matrix.EAxisType.X)
            {
                return TransformVector3NoScale(new Vector3(1.0f, 0.0f, 0.0f));
            }
            else if (InAxis == Matrix.EAxisType.Y)
            {
                return TransformVector3NoScale(new Vector3(0.0f, 1.0f, 0.0f));
            }

            return TransformVector3NoScale(new Vector3(0.0f, 0.0f, 1.0f));
        }
        public static void ConstructTransformFromMatrixWithDesiredScale(in DMatrix AMatrix, in DMatrix BMatrix, in Vector3 DesiredScale, out FTransform OutTransform)
        {
            // the goal of using M is to get the correct orientation
            // but for translation, we still need scale
            var M = AMatrix * BMatrix;
            M.NoScale();

            // apply negative scale back to axes
            var SignedScale = DesiredScale.GetSignVector();
            M.Right = SignedScale.X * M.GetScaledAxis(Matrix.EAxisType.X);
            M.Up = SignedScale.Y * M.GetScaledAxis(Matrix.EAxisType.Y);
            M.Forward = SignedScale.Z * M.GetScaledAxis(Matrix.EAxisType.Z);

            // @note: if you have negative with 0 scale, this will return rotation that is identity
            // since matrix loses that axes            
            var Rotation = DMatrix.RotationMatrix(in M);
            Rotation.Normalize();

            // set values back to output
            OutTransform.mScale = DesiredScale;
            OutTransform.mQuat = Rotation;

            // technically I could calculate this using FTransform but then it does more quat multiplication 
            // instead of using Scale in matrix multiplication
            // it's a question of between RemoveScaling vs using FTransform to move translation
            OutTransform.mPosition = M.Translation;
        }
        #endregion
    }

    public class UBigWorldManager
    {
        public class UWorldBrick
        {
            public UBigWorldManager BigWorld;
            public int X;
            public int Y;
            public int Z;
            public Vector3 GetStart()
            {
                return new Vector3((float)X * BigWorld.GridSize.X, (float)Y * BigWorld.GridSize.Y, (float)Z * BigWorld.GridSize.Z);
            }
        }
        public Vector3 GridSize;
        public Vector3 RcpGridSize;
        UWorldBrick[,,] Bricks;
        public void InitBricks(in Vector3 size, int x = 1024, int y = 1, int z = 1024)
        {
            GridSize = size;
            Bricks = new UWorldBrick[1024, 1, 1024];            
            RcpGridSize.X = 1.0f / size.X;
            RcpGridSize.Y = 1.0f / size.Y;
            RcpGridSize.Z = 1.0f / size.Z;
        }
        public UWorldBrick GetBrick(int x, int y, int z)
        {
            return Bricks[x, y, z];
        }
        public UWorldBrick GetBrick(in Vector3 pos)
        {
            var tmp = pos * RcpGridSize;
            int x = (int)tmp.X;
            int y = (int)tmp.Y;
            int z = (int)tmp.Z;
            x = CoreDefine.Clamp(x, 0, (int)Bricks.GetLongLength(0));
            y = CoreDefine.Clamp(y, 0, (int)Bricks.GetLongLength(1));
            z = CoreDefine.Clamp(z, 0, (int)Bricks.GetLongLength(2));

            lock(Bricks)
            {
                if (Bricks[x, y, z] == null)
                {
                    Bricks[x, y, z] = new UWorldBrick();
                    Bricks[x, y, z].BigWorld = this;
                    Bricks[x, y, z].X = x;
                    Bricks[x, y, z].Y = y;
                    Bricks[x, y, z].Z = z;
                }
                return Bricks[x, y, z];
            }
        }
    }
}
