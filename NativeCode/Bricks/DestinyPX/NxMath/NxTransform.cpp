#include "NxTransform.h"

namespace NxMath
{
	struct TransformTest
	{
		TransformTest()
		{
			Test1<NxReal<NxFloat32>>();
			Test1<NxReal<NxFixed64<24>>>();
		}
		template<typename Type>
		void Test1()
		{
			NxTransform<Type> transform, base, relative;
			transform.ToMatrixWithScale();
			relative = transform.GetRelativeTransform(base);

			NxTransformNoScale<Type> transform_ns, base_ns, relative_ns;
			NxTransformNoScale<Type>::Multiply(transform_ns, base_ns, relative_ns);
			//NxTransform<Type>::GetRelativeTransformUsingMatrixWithScale(transform, base, relative);
		}
	};

	static TransformTest GTestTrasform;
}