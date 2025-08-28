// Copyright (c) 2018 GeometryFactory
// All rights reserved.
//
// This file is part of CGAL (www.cgal.org)
//
// $URL: https://github.com/CGAL/cgal/blob/v6.1-beta1/Intersections_3/include/CGAL/Intersections_3/internal/Iso_cuboid_3_Tetrahedron_3_do_intersect.h $
// $Id: include/CGAL/Intersections_3/internal/Iso_cuboid_3_Tetrahedron_3_do_intersect.h b2f6f03d3fa $
// SPDX-License-Identifier: LGPL-3.0-or-later OR LicenseRef-Commercial
//
//
// Author(s)     : Andreas Fabri

#ifndef CGAL_INTERNAL_INTERSECTIONS_3_ISO_CUBOID_3_TETRAHEDRON_3_DO_INTERSECT_H
#define CGAL_INTERNAL_INTERSECTIONS_3_ISO_CUBOID_3_TETRAHEDRON_3_DO_INTERSECT_H

#include <CGAL/Intersections_3/internal/Tetrahedron_3_Bounded_3_do_intersect.h>

namespace CGAL {
namespace Intersections {
namespace internal {

template <class K>
inline
typename K::Boolean
do_intersect(const typename K::Tetrahedron_3& tet,
             const typename K::Iso_cuboid_3& ic,
             const K& k)
{
  return do_intersect_tetrahedron_bounded(ic, tet, ic[0], k);
}

template <class K>
inline
typename K::Boolean
do_intersect(const typename K::Iso_cuboid_3& ic,
             const typename K::Tetrahedron_3& tet,
             const K& k)
{
  return do_intersect_tetrahedron_bounded(ic, tet, ic[0], k);
}

} // namespace internal
} // namespace Intersections
} // namespace CGAL

#endif // CGAL_INTERNAL_INTERSECTIONS_3_ISO_CUBOID_3_TETRAHEDRON_3_DO_INTERSECT_H
