// Copyright (c) 2019  GeometryFactory (France). All rights reserved.
//
// This file is part of CGAL (www.cgal.org).
//
// $URL: https://github.com/CGAL/cgal/blob/v6.1-beta1/Surface_mesh_simplification/include/CGAL/Surface_mesh_simplification/Policies/Edge_collapse/GarlandHeckbert_policies.h $
// $Id: include/CGAL/Surface_mesh_simplification/Policies/Edge_collapse/GarlandHeckbert_policies.h b2f6f03d3fa $
// SPDX-License-Identifier: GPL-3.0-or-later OR LicenseRef-Commercial
//
// Author(s)     : Baskin Burak Senbaslar,
//                 Mael Rouxel-Labbé,
//                 Julian Komaromy

#ifndef CGAL_SURFACE_MESH_SIMPLIFICATION_POLICIES_GARLANDHECKBERT_POLICIES_H
#define CGAL_SURFACE_MESH_SIMPLIFICATION_POLICIES_GARLANDHECKBERT_POLICIES_H

#include <CGAL/license/Surface_mesh_simplification.h>

#include <CGAL/config.h>

#include <CGAL/Surface_mesh_simplification/Policies/Edge_collapse/GarlandHeckbert_plane_policies.h>
#include <CGAL/Surface_mesh_simplification/Policies/Edge_collapse/GarlandHeckbert_triangle_policies.h>
#include <CGAL/Surface_mesh_simplification/Policies/Edge_collapse/GarlandHeckbert_probabilistic_plane_policies.h>
#include <CGAL/Surface_mesh_simplification/Policies/Edge_collapse/GarlandHeckbert_probabilistic_triangle_policies.h>

namespace CGAL {
namespace Surface_mesh_simplification {

template<typename TriangleMesh, typename GeomTraits>
using GarlandHeckbert_policies CGAL_DEPRECATED = GarlandHeckbert_plane_policies<TriangleMesh, GeomTraits>;

} // namespace Surface_mesh_simplification
} // namespace CGAL

#endif // CGAL_SURFACE_MESH_SIMPLIFICATION_POLICIES_GARLANDHECKBERT_POLICIES_H
