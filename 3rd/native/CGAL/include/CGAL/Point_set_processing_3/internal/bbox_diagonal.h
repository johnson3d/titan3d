// Copyright (c) 2020 GeometryFactory (France).
// All rights reserved.
//
// This file is part of CGAL (www.cgal.org).
//
// $URL: https://github.com/CGAL/cgal/blob/v6.1-beta1/Point_set_processing_3/include/CGAL/Point_set_processing_3/internal/bbox_diagonal.h $
// $Id: include/CGAL/Point_set_processing_3/internal/bbox_diagonal.h b2f6f03d3fa $
// SPDX-License-Identifier: GPL-3.0-or-later OR LicenseRef-Commercial
//
// Author(s) : Simon Giraudot

#ifndef CGAL_PSP_INTERNAL_BBOX_DIAGONAL_H
#define CGAL_PSP_INTERNAL_BBOX_DIAGONAL_H

#include <CGAL/license/Point_set_processing_3.h>

namespace CGAL
{
namespace Point_set_processing_3
{
namespace internal
{

template <typename Kernel, typename PointRange, typename PointMap>
double bbox_diagonal (const PointRange& points, PointMap point_map, const typename Kernel::Point_2&)
{
  CGAL::Bbox_2 bbox = CGAL::bbox_2 (CGAL::make_transform_iterator_from_property_map (points.begin(), point_map),
                                    CGAL::make_transform_iterator_from_property_map (points.end(), point_map));

  return CGAL::approximate_sqrt
    ((bbox.xmax() - bbox.xmin()) * (bbox.xmax() - bbox.xmin())
     + (bbox.ymax() - bbox.ymin()) * (bbox.ymax() - bbox.ymin()));
}

template <typename Kernel, typename PointRange, typename PointMap>
double bbox_diagonal (const PointRange& points, PointMap point_map, const typename Kernel::Point_3&)
{
  CGAL::Bbox_3 bbox = CGAL::bbox_3 (CGAL::make_transform_iterator_from_property_map (points.begin(), point_map),
                                    CGAL::make_transform_iterator_from_property_map (points.end(), point_map));

  return CGAL::approximate_sqrt
    ((bbox.xmax() - bbox.xmin()) * (bbox.xmax() - bbox.xmin())
     + (bbox.ymax() - bbox.ymin()) * (bbox.ymax() - bbox.ymin())
     + (bbox.zmax() - bbox.zmin()) * (bbox.zmax() - bbox.zmin()));
}

template <typename PointRange, typename PointMap>
double bbox_diagonal (const PointRange& points, PointMap point_map)
{
  typedef typename boost::property_traits<PointMap>::value_type Point;
  return bbox_diagonal<typename Kernel_traits<Point>::Kernel> (points, point_map, Point());
}

} // namespace internal
} // namespace Point_set_processing_3
} // namespace CGAL


#endif // CGAL_PSP_INTERNAL_BBOX_DIAGONAL_H
