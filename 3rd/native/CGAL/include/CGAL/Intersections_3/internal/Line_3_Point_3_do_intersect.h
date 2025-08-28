// Copyright (c) 2018 INRIA Sophia-Antipolis (France)
// All rights reserved.
//
// This file is part of CGAL (www.cgal.org)
//
// $URL: https://github.com/CGAL/cgal/blob/v6.1-beta1/Intersections_3/include/CGAL/Intersections_3/internal/Line_3_Point_3_do_intersect.h $
// $Id: include/CGAL/Intersections_3/internal/Line_3_Point_3_do_intersect.h b2f6f03d3fa $
// SPDX-License-Identifier: LGPL-3.0-or-later OR LicenseRef-Commercial
//
//
// Author(s)     : Maxime Gimeno

#ifndef CGAL_INTERNAL_INTERSECTIONS_3_LINE_3_POINT_3_DO_INTERSECT_H
#define CGAL_INTERNAL_INTERSECTIONS_3_LINE_3_POINT_3_DO_INTERSECT_H

namespace CGAL {
namespace Intersections {
namespace internal {

template <class K>
inline typename K::Boolean
do_intersect(const typename K::Point_3& pt,
             const typename K::Line_3& line,
             const K& k)
{
  return k.has_on_3_object()(line, pt);
}

template <class K>
inline typename K::Boolean
do_intersect(const typename K::Line_3& line,
             const typename K::Point_3& pt,
             const K& k)
{
  return k.has_on_3_object()(line, pt);
}

} // namespace internal
} // namespace Intersections
} // namespace CGAL

#endif // CGAL_INTERNAL_INTERSECTIONS_3_LINE_3_POINT_3_DO_INTERSECT_H
