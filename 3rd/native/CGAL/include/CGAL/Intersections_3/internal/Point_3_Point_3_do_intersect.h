// Copyright (c) 2003  INRIA Sophia-Antipolis (France).
// All rights reserved.
//
// This file is part of CGAL (www.cgal.org)
//
// $URL: https://github.com/CGAL/cgal/blob/v6.1-beta1/Intersections_3/include/CGAL/Intersections_3/internal/Point_3_Point_3_do_intersect.h $
// $Id: include/CGAL/Intersections_3/internal/Point_3_Point_3_do_intersect.h b2f6f03d3fa $
// SPDX-License-Identifier: LGPL-3.0-or-later OR LicenseRef-Commercial
//
//
// Author(s)     : Sébastien Loriot

#ifndef CGAL_INTERNAL_INTERSECTIONS_POINT_3_POINT_3_DO_INTERSECT_H
#define CGAL_INTERNAL_INTERSECTIONS_POINT_3_POINT_3_DO_INTERSECT_H

namespace CGAL {
namespace Intersections {
namespace internal {

template <class K>
inline bool
do_intersect(const typename K::Point_3& pt1,
             const typename K::Point_3& pt2,
             const K& k)
{
  return k.equal_3_object()(pt1, pt2);
}

} // namespace internal
} // namespace Intersections
} // namespace CGAL

#endif // CGAL_INTERNAL_INTERSECTIONS_POINT_3_POINT_3_DO_INTERSECT_H
