// Copyright (c) 2006  GeometryFactory (France). All rights reserved.
//
// This file is part of CGAL (www.cgal.org).
//
// $URL: https://github.com/CGAL/cgal/blob/v6.1-beta1/Surface_mesh_simplification/include/CGAL/Null_matrix.h $
// $Id: include/CGAL/Null_matrix.h b2f6f03d3fa $
// SPDX-License-Identifier: GPL-3.0-or-later OR LicenseRef-Commercial
//
// Author(s)     : Fernando Cacciola <fernando.cacciola@geometryfactory.com>
//
#ifndef CGAL_NULL_MATRIX_H
#define CGAL_NULL_MATRIX_H

#include <CGAL/license/Surface_mesh_simplification.h>

namespace CGAL {

class Null_matrix {};

static const Null_matrix NULL_MATRIX = Null_matrix();

} // namespace CGAL

#endif // CGAL_NULL_MATRIX_H
