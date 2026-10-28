#pragma once

#include <unordered_map>
#include "alloc/general_allocator.h"

namespace leanclr
{
namespace utils
{
template <typename K, typename V, class _Hasher = std::hash<K>, class _Keyeq = std::equal_to<K>,
          class _Allocator = alloc::GeneralAllocator<std::pair<const K, V>>>
using HashMap = std::unordered_map<K, V, _Hasher, _Keyeq, _Allocator>;
} // namespace utils
} // namespace leanclr
