#pragma once
#include "NxMatrix.h"

namespace NxMath
{
	template <typename Type = NxReal<NxFloat32>>
	struct NxTransform
	{
		using ThisType = NxTransform<Type>;
		using Matrix = NxMatrix4x4<Type>;
		using Vector3 = NxVector3<Type>;
		NxQuat<Type> Quat;
		NxVector3<Type> Position;
		NxVector3<Type> Scale;//为了Hierarchical计算方便，我们设定mScale在Transform中只影响本节点而不传递，如果需要整体放缩，在Node上新增一个ScaleMatrix
		NxTransform()
			: Quat(NxQuat<Type>::Identity())
			, Position(Vector3::Zero())
			, Scale(Vector3::One())
		{

		}
		NxTransform(const Vector3& pos, const Vector3& scale, const NxQuat<Type>& quat) 
			: Quat(quat)
			, Position(pos)
			, Scale(scale)
		{
			
		}
		inline static constexpr NxTransform GetIdentity() {
			return NxTransform(Vector3::Zero(), Vector3::One(), NxQuat<Type>::Identity());
		}

		inline Matrix ToMatrixWithScale() const
		{
			Matrix OutMatrix;

			OutMatrix[3][0] = Position.X;
			OutMatrix[3][1] = Position.Y;
			OutMatrix[3][2] = Position.Z;

			auto x2 = Quat.X + Quat.X;
			auto y2 = Quat.Y + Quat.Y;
			auto z2 = Quat.Z + Quat.Z;
			{
				auto xx2 = Quat.X * x2;
				auto yy2 = Quat.Y * y2;
				auto zz2 = Quat.Z * z2;

				OutMatrix[0][0] = (Type::One() - (yy2 + zz2)) * Scale.X;
				OutMatrix[1][1] = (Type::One() - (xx2 + zz2)) * Scale.Y;
				OutMatrix[2][2] = (Type::One() - (xx2 + yy2)) * Scale.Z;
			}
			{
				auto yz2 = Quat.Y * z2;
				auto wx2 = Quat.W * x2;

				OutMatrix[2][1] = (yz2 - wx2) * Scale.Z;
				OutMatrix[1][2] = (yz2 + wx2) * Scale.Y;
			}
			{
				auto xy2 = Quat.X * y2;
				auto wz2 = Quat.W * z2;

				OutMatrix[1][0] = (xy2 - wz2) * Scale.Y;
				OutMatrix[0][1] = (xy2 + wz2) * Scale.X;
			}
			{
				auto xz2 = Quat.X * z2;
				auto wy2 = Quat.W * y2;

				OutMatrix[2][0] = (xz2 + wy2) * Scale.Z;
				OutMatrix[0][2] = (xz2 - wy2) * Scale.X;
			}

			OutMatrix[0][3] = Type::Zero();
			OutMatrix[1][3] = Type::Zero();
			OutMatrix[2][3] = Type::Zero();
			OutMatrix[3][3] = Type::One();

			return OutMatrix;
		}

		inline Matrix ToMatrixNoScale() const
		{
			Matrix OutMatrix;

			OutMatrix[3][0] = Position.X;
			OutMatrix[3][1] = Position.Y;
			OutMatrix[3][2] = Position.Z;

			auto x2 = Quat.X + Quat.X;
			auto y2 = Quat.Y + Quat.Y;
			auto z2 = Quat.Z + Quat.Z;
			{
				auto xx2 = Quat.X * x2;
				auto yy2 = Quat.Y * y2;
				auto zz2 = Quat.Z * z2;

				OutMatrix[0][0] = (Type::One() - (yy2 + zz2));
				OutMatrix[1][1] = (Type::One() - (xx2 + zz2));
				OutMatrix[2][2] = (Type::One() - (xx2 + yy2));
			}
			{
				auto yz2 = Quat.Y * z2;
				auto wx2 = Quat.W * x2;

				OutMatrix[2][1] = (yz2 - wx2);
				OutMatrix[1][2] = (yz2 + wx2);
			}
			{
				auto xy2 = Quat.X * y2;
				auto wz2 = Quat.W * z2;

				OutMatrix[1][0] = (xy2 - wz2);
				OutMatrix[0][1] = (xy2 + wz2);
			}
			{
				auto xz2 = Quat.X * z2;
				auto wy2 = Quat.W * y2;

				OutMatrix[2][0] = (xz2 + wy2);
				OutMatrix[0][2] = (xz2 - wy2);
			}

			OutMatrix[0][3] = Type::Zero();
			OutMatrix[1][3] = Type::Zero();
			OutMatrix[2][3] = Type::Zero();
			OutMatrix[3][3] = Type::One();

			return OutMatrix;
		}

		inline void SetToRelativeTransform(const ThisType& ParentTransform)
		{
			// A * B(-1) = VQS(B)(-1) (VQS (A))
			// 
			// Scale = S(A)/S(B)
			// Rotation = Q(B)(-1) * Q(A)
			// Translation = 1/S(B) *[Q(B)(-1)*(T(A)-T(B))*Q(B)]
			// where A = this, B = Other

			auto SafeRecipScale3D = GetSafeScaleReciprocal(ParentTransform.Scale);
			auto InverseRot = ParentTransform.Quat.Inverse();

			Scale *= SafeRecipScale3D;
			Position = (InverseRot * (Position - ParentTransform.Position)) * SafeRecipScale3D;
			Quat = InverseRot * Quat;
		}

		inline ThisType GetRelativeTransform(const ThisType& Other)
		{
			// A * B(-1) = VQS(B)(-1) (VQS (A))
			// 
			// Scale = S(A)/S(B)
			// Rotation = Q(B)(-1) * Q(A)
			// Translation = 1/S(B) *[Q(B)(-1)*(T(A)-T(B))*Q(B)]
			// where A = this, B = Other
			ThisType Result;

			if (Scale.HasNagative() || Other.Scale.HasNagative())
			{
				// @note, if you have 0 scale with negative, you're going to lose rotation as it can't convert back to quat
				GetRelativeTransformUsingMatrixWithScale(Result, *this, Other);
			}
			else
			{
				Vector3 SafeRecipScale3D = GetSafeScaleReciprocal(Other.Scale);
				Result.Scale = Scale * SafeRecipScale3D;

				if (Other.Quat.IsNormalized() == false)
				{
					return GetIdentity();
				}

				auto Inverse = Other.Quat.Invert();
				Result.Quat = Inverse * Quat;

				auto dist = Position - Other.Position;
				auto tmp = (Inverse * dist);
				Result.Position = tmp * SafeRecipScale3D;
			}
			return Result;
		}
		inline ThisType GetRelativeTransformReverse(const ThisType& Other)
		{
			// A (-1) * B = VQS(B)(VQS (A)(-1))
			// 
			// Scale = S(B)/S(A)
			// Rotation = Q(B) * Q(A)(-1)
			// Translation = T(B)-S(B)/S(A) *[Q(B)*Q(A)(-1)*T(A)*Q(A)*Q(B)(-1)]
			// where A = this, and B = Other
			ThisType Result;

			auto SafeRecipScale3D = GetSafeScaleReciprocal(Scale);
			Result.Scale = Other.Scale * SafeRecipScale3D;

			Result.Quat = Other.Quat * Quat.Invert();

			Result.Position = Other.Position - (Result.Quat * Position) * Result.Scale;

			return Result;
		}
		inline static void Multiply(ThisType& OutTransform, const ThisType& A, const ThisType& B)
		{
			if (A.Scale.HasNagative() || B.Scale.HasNagative())
			{
				// @note, if you have 0 scale with negative, you're going to lose rotation as it can't convert back to quat
				MultiplyUsingMatrixWithScale(OutTransform, A, B);
			}
			else
			{
				OutTransform.Quat = A.Quat * B.Quat;
				OutTransform.Quat.Normalize();
				OutTransform.Scale = A.Scale * B.Scale;
				OutTransform.Position = B.Quat * (A.Position * B.Scale) + B.Position;
			}
			//var mtx1 = A.ToMatrixWithScale();
			//var mtx2 = B.ToMatrixWithScale();
			//var mtx = mtx1 * mtx2;
			//mtx.Decompose(out OutTransform.mScale, out OutTransform.mQuat, out OutTransform.mPosition);
		}
		inline static void MultiplyNoParentScale(ThisType& OutTransform, const ThisType& A, const ThisType& B)
		{
			OutTransform.Quat = A.mQuat * B.mQuat;
			OutTransform.Quat.Normalize();
			OutTransform.Scale = A.mScale;
			OutTransform.Position = B.Quat * (A.Position) + B.Position;

			//var mtx1 = A.ToMatrixWithScale();
			//var mtx2 = B.ToMatrixNoScale();
			//var mtx = mtx1 * mtx2;
			//mtx.Decompose(out OutTransform.mScale, out OutTransform.mQuat, out OutTransform.mPosition);
		}
		inline static void MultiplyUsingMatrixWithScale(ThisType& OutTransform, const ThisType& A, const ThisType& B)
		{
			ConstructTransformFromMatrixWithDesiredScale(A.ToMatrixWithScale(), B.ToMatrixWithScale(), A.Scale * B.Scale, OutTransform);
		}
		inline NxVector3<Type> TransformVector3(const NxVector3<Type>& V)
		{
			auto Transform = NxQuat<Type>::RotateVector3(Quat, V * Scale);
			return Transform;
		}
		inline NxVector4<Type> TransformVector4(const NxVector4<Type>& V)
		{
			//Transform using QST is following
			//QST(P) = Q*S*P*-Q + T where Q = quaternion, S = scale, T = translation

			auto Transform = NxVector4<Type>(NxQuat<Type>::RotateVector3(Quat, V.XYZ() * Scale), Type::Zero());
			if (V.W == Type::One())
			{
				Transform += NxVector4<Type>(Position, Type::One());
			}

			return Transform;
		}
		inline NxVector3<Type> TransformPosition(const NxVector3<Type>& V)
		{
			return NxQuat<Type>::RotateVector3(Quat, V * Scale) + Position;
		}
	private:
		inline static void GetRelativeTransformUsingMatrixWithScale(ThisType& OutTransform, const ThisType& Base, const ThisType& Relative)
		{
			// the goal of using M is to get the correct orientation
			// but for translation, we still need scale
			auto AM = Base.ToMatrixWithScale();
			auto BM = Relative.ToMatrixWithScale();
			// get combined scale
			auto SafeRecipScale3D = GetSafeScaleReciprocal(Relative.Scale);
			auto DesiredScale3D = Base.Scale * SafeRecipScale3D;
			BM.Inverse();
			ConstructTransformFromMatrixWithDesiredScale(AM, BM, DesiredScale3D, OutTransform);
		}
		inline static void ConstructTransformFromMatrixWithDesiredScale(const Matrix& AMatrix, const Matrix& BMatrix, const Vector3& DesiredScale, ThisType& OutTransform)
		{
			// the goal of using M is to get the correct orientation
			// but for translation, we still need scale
			auto M = AMatrix * BMatrix;
			M.NoScale();

			// apply negative scale back to axes
			auto SignedScale = DesiredScale.GetSignVector();
			M.SetColume3(0, M.GetScaledAxis(EAxisType::X) * SignedScale.X);
			M.SetColume3(1, M.GetScaledAxis(EAxisType::Y) * SignedScale.Y);
			M.SetColume3(2, M.GetScaledAxis(EAxisType::Z) * SignedScale.Z);

			// @note: if you have negative with 0 scale, this will return rotation that is identity
			// since matrix loses that axes            
			auto Rotation = Matrix::ToQuat(M);
			Rotation.Normalize();

			// set values back to output
			OutTransform.Scale = DesiredScale;
			OutTransform.Quat = Rotation;

			// technically I could calculate this using FTransform but then it does more quat multiplication 
			// instead of using Scale in matrix multiplication
			// it's a question of between RemoveScaling vs using FTransform to move translation
			OutTransform.Position = M.GetTranslate();
		}
		inline static Vector3 GetSafeScaleReciprocal(const Vector3& InScale, Type Tolerance = Type::Epsilon())
		{
			Vector3 SafeReciprocalScale;
			if (Type::Abs(InScale.X) <= Tolerance)
			{
				SafeReciprocalScale.X = Type::Zero();
			}
			else
			{
				SafeReciprocalScale.X = Type::One() / InScale.X;
			}

			if (Type::Abs(InScale.Y) <= Tolerance)
			{
				SafeReciprocalScale.Y = Type::Zero();
			}
			else
			{
				SafeReciprocalScale.Y = Type::One() / InScale.Y;
			}

			if (Type::Abs(InScale.Z) <= Tolerance)
			{
				SafeReciprocalScale.Z = Type::Zero();
			}
			else
			{
				SafeReciprocalScale.Z = Type::One() / InScale.Z;
			}

			return SafeReciprocalScale;
		}
	};

	template <typename Type = NxReal<NxFloat32>>
	struct NxTransformNoScale
	{
		using ThisType = NxTransformNoScale<Type>;
		using Matrix = NxMatrix4x4<Type>;
		using Vector3 = NxVector3<Type>;
		NxQuat<Type> Quat;
		NxVector3<Type> Position;
		NxTransformNoScale()
			: Quat(NxQuat<Type>::Identity())
			, Position(Vector3::Zero())
		{

		}
		NxTransformNoScale(const Vector3& pos, const Vector3& scale, const NxQuat<Type>& quat)
			: Quat(quat)
			, Position(pos)
		{

		}
		inline static constexpr NxTransformNoScale GetIdentity() {
			return NxTransformNoScale(Vector3::Zero(), NxQuat<Type>::Identity());
		}

		inline Matrix ToMatrixNoScale() const
		{
			Matrix OutMatrix;

			OutMatrix[3][0] = Position.X;
			OutMatrix[3][1] = Position.Y;
			OutMatrix[3][2] = Position.Z;

			auto x2 = Quat.X + Quat.X;
			auto y2 = Quat.Y + Quat.Y;
			auto z2 = Quat.Z + Quat.Z;
			{
				auto xx2 = Quat.X * x2;
				auto yy2 = Quat.Y * y2;
				auto zz2 = Quat.Z * z2;

				OutMatrix[0][0] = (1.0f - (yy2 + zz2));
				OutMatrix[1][1] = (1.0f - (xx2 + zz2));
				OutMatrix[2][2] = (1.0f - (xx2 + yy2));
			}
			{
				auto yz2 = Quat.Y * z2;
				auto wx2 = Quat.W * x2;

				OutMatrix[2][1] = (yz2 - wx2);
				OutMatrix[1][2] = (yz2 + wx2);
			}
			{
				auto xy2 = Quat.X * y2;
				auto wz2 = Quat.W * z2;

				OutMatrix[1][0] = (xy2 - wz2);
				OutMatrix[0][1] = (xy2 + wz2);
			}
			{
				auto xz2 = Quat.X * z2;
				auto wy2 = Quat.W * y2;

				OutMatrix[2][0] = (xz2 + wy2);
				OutMatrix[0][2] = (xz2 - wy2);
			}

			OutMatrix[0][3] = 0.0f;
			OutMatrix[1][3] = 0.0f;
			OutMatrix[2][3] = 0.0f;
			OutMatrix[3][3] = 1.0f;

			return OutMatrix;
		}

		inline void SetToRelativeTransform(const ThisType& ParentTransform)
		{
			// A * B(-1) = VQS(B)(-1) (VQS (A))
			// 
			// Scale = S(A)/S(B)
			// Rotation = Q(B)(-1) * Q(A)
			// Translation = 1/S(B) *[Q(B)(-1)*(T(A)-T(B))*Q(B)]
			// where A = this, B = Other

			auto SafeRecipScale3D = GetSafeScaleReciprocal(ParentTransform.Scale);
			auto InverseRot = ParentTransform.Quat.Inverse();

			Position = (InverseRot * (Position - ParentTransform.Position)) * SafeRecipScale3D;
			Quat = InverseRot * Quat;
		}

		inline ThisType GetRelativeTransform(const ThisType& Other)
		{
			// A * B(-1) = VQS(B)(-1) (VQS (A))
			// 
			// Scale = S(A)/S(B)
			// Rotation = Q(B)(-1) * Q(A)
			// Translation = 1/S(B) *[Q(B)(-1)*(T(A)-T(B))*Q(B)]
			// where A = this, B = Other
			ThisType Result;

			{
				Vector3 SafeRecipScale3D = GetSafeScaleReciprocal(Other.Scale);
				
				if (Other.Quat.IsNormalized() == false)
				{
					return GetIdentity();
				}

				auto Inverse = Other.Quat.Invert();
				Result.Quat = Inverse * Quat;

				auto dist = Position - Other.Position;
				auto tmp = (Inverse * dist);
				Result.Position = tmp * SafeRecipScale3D;
			}
			return Result;
		}
		inline ThisType GetRelativeTransformReverse(const ThisType& Other)
		{
			// A (-1) * B = VQS(B)(VQS (A)(-1))
			// 
			// Scale = S(B)/S(A)
			// Rotation = Q(B) * Q(A)(-1)
			// Translation = T(B)-S(B)/S(A) *[Q(B)*Q(A)(-1)*T(A)*Q(A)*Q(B)(-1)]
			// where A = this, and B = Other
			ThisType Result;

			Result.Quat = Other.Quat * Quat.Invert();

			Result.Position = Other.Position - (Result.Quat * Position);

			return Result;
		}
		inline static void Multiply(ThisType& OutTransform, const ThisType& A, const ThisType& B)
		{
			OutTransform.Quat = A.Quat * B.Quat;
			OutTransform.Quat.Normalize();
			OutTransform.Position = B.Quat * (A.Position) + B.Position;
		}
		inline static void MultiplyNoParentScale(ThisType& OutTransform, const ThisType& A, const ThisType& B)
		{
			OutTransform.Quat = A.Quat * B.Quat;
			OutTransform.Quat.Normalize();
			OutTransform.Position = B.Quat * (A.Position) + B.Position;

			//var mtx1 = A.ToMatrixWithScale();
			//var mtx2 = B.ToMatrixNoScale();
			//var mtx = mtx1 * mtx2;
			//mtx.Decompose(out OutTransform.mScale, out OutTransform.mQuat, out OutTransform.mPosition);
		}
		inline static void MultiplyUsingMatrix(ThisType& OutTransform, const ThisType& A, const ThisType& B)
		{
			ConstructTransformFromMatrix(A.ToMatrixWithScale(), B.ToMatrixWithScale(), OutTransform);
		}
		inline NxVector3<Type> TransformVector3(const NxVector3<Type>& V) const
		{
			auto Transform = NxQuat<Type>::RotateVector3(Quat, V);
			return Transform;
		}
		inline NxVector4<Type> TransformVector4(const NxVector4<Type>& V) const
		{
			//Transform using QST is following
			//QST(P) = Q*S*P*-Q + T where Q = quaternion, S = scale, T = translation

			auto Transform = NxVector4<Type>(NxQuat<Type>::RotateVector3(Quat, V.XYZ()), Type::Zero());
			if (V.W == Type::One())
			{
				Transform += NxVector4<Type>(Position, Type::One());
			}

			return Transform;
		}
		inline NxVector3<Type> TransformPosition(const NxVector3<Type>& V) const
		{
			return NxQuat<Type>::RotateVector3(Quat, V) + Position;
		}
	private:	
		inline static void ConstructTransformFromMatrix(const Matrix& AMatrix, const Matrix& BMatrix, ThisType& OutTransform)
		{
			// the goal of using M is to get the correct orientation
			// but for translation, we still need scale
			auto M = AMatrix * BMatrix;
			M.NoScale();

			// @note: if you have negative with 0 scale, this will return rotation that is identity
			// since matrix loses that axes            
			auto Rotation = Matrix::ToQuat(M);
			Rotation.Normalize();

			// set values back to output
			OutTransform.Quat = Rotation;

			// technically I could calculate this using FTransform but then it does more quat multiplication 
			// instead of using Scale in matrix multiplication
			// it's a question of between RemoveScaling vs using FTransform to move translation
			OutTransform.Position = M.GetTranslate();
		}
	};
}

