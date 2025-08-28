// Copyright (c) 2005 Rijksuniversiteit Groningen (Netherlands)
// All rights reserved.
//
// This file is part of CGAL (www.cgal.org).
//
// $URL: https://github.com/CGAL/cgal/blob/v6.1-beta1/Skin_surface_3/include/CGAL/mesh_union_of_balls_3.h $
// $Id: include/CGAL/mesh_union_of_balls_3.h b2f6f03d3fa $
// SPDX-License-Identifier: GPL-3.0-or-later OR LicenseRef-Commercial
//
//
// Author(s)     : Nico Kruithof <Nico@cs.rug.nl>

#ifndef CGAL_MESH_UNION_OF_BALLS_3_H
#define CGAL_MESH_UNION_OF_BALLS_3_H

#include <CGAL/license/Skin_surface_3.h>

#include <CGAL/mesh_skin_surface_3.h>

namespace CGAL {

template <class UnionOfBalls_3, class Polyhedron>
void mesh_union_of_balls_3(UnionOfBalls_3 const &union_of_balls, Polyhedron &p)
{
  union_of_balls.mesh_surface_3(p);
}

} //namespace CGAL

#endif // CGAL_MESH_UNION_OF_BALLS_3_H
