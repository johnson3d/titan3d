// Copyright (c) 2017  INRIA Sophia-Antipolis (France).
// All rights reserved.
//
// This file is part of CGAL (www.cgal.org)
//
// $URL: https://github.com/CGAL/cgal/blob/v6.1-beta1/STL_Extension/include/CGAL/Hidden_point_memory_policy.h $
// $Id: include/CGAL/Hidden_point_memory_policy.h b2f6f03d3fa $
// SPDX-License-Identifier: LGPL-3.0-or-later OR LicenseRef-Commercial
//
// Author(s)     : Mael Rouxel-Labbé

#ifndef CGAL_HIDDEN_POINT_MEMORY_POLICY_H
#define CGAL_HIDDEN_POINT_MEMORY_POLICY_H

#include <CGAL/tags.h>

namespace CGAL {

// A policy to select whether hidden points should be cached in cells or discarded
// during the construction of a regular triangulation.

template < typename Tag >
struct Hidden_points_memory_policy : public Tag { };

typedef Hidden_points_memory_policy<Tag_true> Keep_hidden_points;
typedef Hidden_points_memory_policy<Tag_false> Discard_hidden_points;

} // namespace CGAL

#endif // CGAL_HIDDEN_POINT_MEMORY_POLICY_H
