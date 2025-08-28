// Copyright (c) 2011  INRIA Sophia-Antipolis (France).
// All rights reserved.
//
// This file is part of CGAL (www.cgal.org)
//
// $URL: https://github.com/CGAL/cgal/blob/v6.1-beta1/Spatial_sorting/include/CGAL/Hilbert_sort_middle_base.h $
// $Id: include/CGAL/Hilbert_sort_middle_base.h b2f6f03d3fa $
// SPDX-License-Identifier: LGPL-3.0-or-later OR LicenseRef-Commercial
//
// Author(s)     :  Olivier Devillers

#ifndef CGAL_HILBERT_SORT_MIDDLE_BASE_H
#define CGAL_HILBERT_SORT_MIDDLE_BASE_H

#include <CGAL/config.h>
#include <algorithm>

namespace CGAL {

namespace internal {

template <class RandomAccessIterator, class Cmp>
RandomAccessIterator
fixed_hilbert_split (RandomAccessIterator begin, RandomAccessIterator end,
                     Cmp cmp = Cmp ())
{
  if (begin >= end)
    return begin;

  return std::partition (begin, end, cmp);
}

} // namespace internal

} // namespace CGAL

#endif//CGAL_HILBERT_SORT_MIDDLE_BASE_H
