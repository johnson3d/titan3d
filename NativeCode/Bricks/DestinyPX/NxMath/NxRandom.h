#include "NxReal.h"
#include "../../../Base/random/mt.h"

namespace NxMath
{
	template <typename Type = NxReal<NxFloat32>, std::uint64_t Level = 65536>
	class NxRandom
	{
	public:
		using RDType = std::uint64_t;
		MersenneTwister ObjectMT;
	public:
		NxRandom(RDType seed)
		{
			ObjectMT.init_genrand64(seed);
		}
		inline RDType Next()
		{
			return ObjectMT.genrand64_int64();
		}
		
		inline Type GetUnit()
		{
			auto r = Next();
			auto r0 = (double)(r % Level);
			auto r00 = Type::ByInt((int)r0);
			return r00 / Type::ByInt((int)Level);
		}
	};
}

