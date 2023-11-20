#pragma once
#include <math.h>
#include <atomic>
#include <algorithm>
#include <assert.h>

namespace NxMath
{
	template <typename T>
	struct ElementType { using ResulType = T; };
	template<typename T, size_t N>
	struct ElementType <T[N]> : public ElementType<T> {};

	struct ArrayInfo
	{
		template<typename T>
		static constexpr int Length(const T& array)
		{
			return sizeof(array) / sizeof(ElementType<T>::ResulType);
		}
	};
}

