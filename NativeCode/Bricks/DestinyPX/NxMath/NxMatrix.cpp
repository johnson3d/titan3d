#include "NxMatrix.h"

namespace NxMath
{
	struct MatrixVector
	{
		MatrixVector()
		{
			Test1<NxReal<NxFloat32>>();
			Test1<NxReal<NxFixed64<24>>>();
		}
		template<typename Type>
		void Test1()
		{
			using Matrix2x2 = NxMatrix2x2<Type>;
			Matrix2x2 a(typename Matrix2x2::VectorType(Type(1.0f), Type(0)),
						typename Matrix2x2::VectorType(Type(0), Type(1.0f)));
			Matrix2x2 b(typename Matrix2x2::VectorType(Type(0), Type(1)),
						typename Matrix2x2::VectorType(Type(1), Type(0)));

			auto c1 = Matrix2x2::Multiply(a, a);
			auto c2 = Matrix2x2::Multiply(a, b);
			auto c3 = a + b;

			using Matrix4x4 = NxMatrix4x4<Type>;
			auto m4x4_a = Matrix4x4::RotationX(Type(0.2f));
			m4x4_a = m4x4_a + Type::Zero();
		}
	};

	static MatrixVector GTestMatrix;
}