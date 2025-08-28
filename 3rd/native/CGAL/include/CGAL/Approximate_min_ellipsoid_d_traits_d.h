// Copyright (c) 1997-2001  ETH Zurich (Switzerland).
// All rights reserved.
//
// This file is part of CGAL (www.cgal.org).
//
// $URL: https://github.com/CGAL/cgal/blob/v6.1-beta1/Bounding_volumes/include/CGAL/Approximate_min_ellipsoid_d_traits_d.h $
// $Id: include/CGAL/Approximate_min_ellipsoid_d_traits_d.h b2f6f03d3fa $
// SPDX-License-Identifier: GPL-3.0-or-later OR LicenseRef-Commercial
//
//
// Author(s)     : Kaspar Fischer <fischerk@inf.ethz.ch>

#ifndef CGAL_APPROX_MIN_ELL_D_TRAITS_D_H
#define CGAL_APPROX_MIN_ELL_D_TRAITS_D_H

#include <CGAL/license/Bounding_volumes.h>


namespace CGAL {

  template<typename K_, typename ET_>    // kernel and exact number-type
  struct Approximate_min_ellipsoid_d_traits_d {
    typedef double               FT;     // number type (must be double)
    typedef ET_                  ET;     // number type used for exact
                                         // computations (like i.e. computing
                                         // the exact approximation ratio
                                         // in achieved_epsilon())
    typedef typename K_::Point_d Point;  // point type
    typedef typename K_::Cartesian_const_iterator_d Cartesian_const_iterator;
                                         // iterator over point coordinates

    static int dimension(const Point& p)
    {
      return p.dimension();
    }

    static Cartesian_const_iterator cartesian_begin(const Point& p)
    {
      return p.cartesian_begin();
    }
  };

} // namespace CGAL

#endif // CGAL_APPROX_MIN_ELL_D_TRAITS_D_H
