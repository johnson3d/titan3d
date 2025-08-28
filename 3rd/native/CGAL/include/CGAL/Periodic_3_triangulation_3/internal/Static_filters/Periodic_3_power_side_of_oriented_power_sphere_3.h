// Copyright (c) 2017  INRIA Sophia-Antipolis (France).
// All rights reserved.
//
// This file is part of CGAL (www.cgal.org)
//
// $URL: https://github.com/CGAL/cgal/blob/v6.1-beta1/Periodic_3_triangulation_3/include/CGAL/Periodic_3_triangulation_3/internal/Static_filters/Periodic_3_power_side_of_oriented_power_sphere_3.h $
// $Id: include/CGAL/Periodic_3_triangulation_3/internal/Static_filters/Periodic_3_power_side_of_oriented_power_sphere_3.h b2f6f03d3fa $
// SPDX-License-Identifier: GPL-3.0-or-later OR LicenseRef-Commercial
//
// Author(s)     : Mael Rouxel-Labbé

#ifndef CGAL_INTERNAL_STATIC_FILTERS_PERIODIC_3_POWER_TEST_3_H
#define CGAL_INTERNAL_STATIC_FILTERS_PERIODIC_3_POWER_TEST_3_H

#include <CGAL/license/Periodic_3_triangulation_3.h>


#include <CGAL/Profile_counter.h>
#include <CGAL/Filtered_kernel/internal/Static_filters/Static_filter_error.h>
#include <CGAL/Filtered_kernel/internal/Static_filters/tools.h>

#include <CGAL/Periodic_3_offset_3.h>

#include <cmath>

namespace CGAL {

namespace internal {

namespace Static_filters_predicates {

template <class K, class Power_side_of_oriented_power_sphere_3_base>
class Periodic_3_power_side_of_oriented_power_sphere_3:
    public Power_side_of_oriented_power_sphere_3_base
{
  typedef Power_side_of_oriented_power_sphere_3_base           Base;

public:
  typedef K                                                    Kernel;

  // @todo
};

} // namespace Static_filters_predicates

} // namespace internal

} //namespace CGAL

#endif // CGAL_INTERNAL_STATIC_FILTERS_PERIODIC_3_POWER_TEST_3_H
