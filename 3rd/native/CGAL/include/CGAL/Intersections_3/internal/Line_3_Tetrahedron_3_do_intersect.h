// Copyright (c) 2019 GeometryFactory(France).
// All rights reserved.
//
// This file is part of CGAL (www.cgal.org)
//
// $URL: https://github.com/CGAL/cgal/blob/v6.1-beta1/Intersections_3/include/CGAL/Intersections_3/internal/Line_3_Tetrahedron_3_do_intersect.h $
// $Id: include/CGAL/Intersections_3/internal/Line_3_Tetrahedron_3_do_intersect.h b2f6f03d3fa $
// SPDX-License-Identifier: LGPL-3.0-or-later OR LicenseRef-Commercial
//
//
// Author(s)     : Andreas Fabri,
//                 Laurent Rineau

#ifndef CGAL_INTERNAL_INTERSECTIONS_3_LINE_3_TETRAHEDRON_3_DO_INTERSECT_H
#define CGAL_INTERNAL_INTERSECTIONS_3_LINE_3_TETRAHEDRON_3_DO_INTERSECT_H

#include <CGAL/Intersections_3/internal/Tetrahedron_3_Unbounded_3_do_intersect.h>

namespace CGAL {
namespace Intersections {
namespace internal {

template<typename K>
typename K::Boolean
do_intersect(const typename K::Line_3& unb,
             const typename K::Tetrahedron_3& tet,
             const K& k)
{
  return do_intersect_tetrahedron_unbounded(tet, unb, k);
}

template<typename K>
typename K::Boolean
do_intersect(const typename K::Tetrahedron_3& tet,
             const typename K::Line_3& unb,
             const K& k)
{
  return do_intersect_tetrahedron_unbounded(tet, unb, k);
}

} // namespace internal
} // namespace Intersections
} // namespace CGAL

#endif // CGAL_INTERNAL_INTERSECTIONS_3_LINE_3_TETRAHEDRON_3_DO_INTERSECT_H
