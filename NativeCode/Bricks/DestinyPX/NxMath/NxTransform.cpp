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
			//NxTransform<Type>::GetRelativeTransformUsingMatrixWithScale(transform, base, relative);
		}
	};

	static TransformTest GTestTrasform;
}