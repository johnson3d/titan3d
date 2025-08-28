// Copyright (c) 2018 INRIA Sophia-Antipolis (France).
// All rights reserved.
//
// This file is part of CGAL (www.cgal.org).
//
// $URL: https://github.com/CGAL/cgal/blob/v6.1-beta1/Shape_detection/include/CGAL/Shape_detection/Region_growing/Polygon_mesh.h $
// $Id: include/CGAL/Shape_detection/Region_growing/Polygon_mesh.h b2f6f03d3fa $
// SPDX-License-Identifier: GPL-3.0-or-later OR LicenseRef-Commercial
//
//
// Author(s)     : Florent Lafarge, Simon Giraudot, Thien Hoang, Dmitry Anisimov
//

#ifndef CGAL_SHAPE_DETECTION_REGION_GROWING_POLYGON_MESH_H
#define CGAL_SHAPE_DETECTION_REGION_GROWING_POLYGON_MESH_H

/// \cond SKIP_IN_MANUAL
#include <CGAL/license/Shape_detection.h>
/// \endcond

/**
* \ingroup PkgShapeDetectionRef
* \file CGAL/Shape_detection/Region_growing/Polygon_mesh.h
* A convenience header that includes all classes related to the region growing algorithm on a polygon mesh.
*/

#include <CGAL/Shape_detection/Region_growing/Polygon_mesh/Polyline_graph.h>
#include <CGAL/Shape_detection/Region_growing/Polygon_mesh/One_ring_neighbor_query.h>

#include <CGAL/Shape_detection/Region_growing/Polygon_mesh/Least_squares_plane_fit_region.h>
#include <CGAL/Shape_detection/Region_growing/Polygon_mesh/Least_squares_plane_fit_sorting.h>

#endif // CGAL_SHAPE_DETECTION_REGION_GROWING_POLYGON_MESH_H
