// Copyright (c) 2006  Tel-Aviv University (Israel).
// All rights reserved.
//
// This file is part of CGAL (www.cgal.org).
//
// $URL: https://github.com/CGAL/cgal/blob/v6.1-beta1/Minkowski_sum_2/include/CGAL/Polygon_convex_decomposition_2.h $
// $Id: include/CGAL/Polygon_convex_decomposition_2.h b2f6f03d3fa $
// SPDX-License-Identifier: GPL-3.0-or-later OR LicenseRef-Commercial
//
// Author(s)     : Ron Wein   <wein_r@yahoo.com>

#ifndef CGAL_POLYGON_CONVEX_DECOMPOSITION_H
#define CGAL_POLYGON_CONVEX_DECOMPOSITION_H

#include <CGAL/license/Minkowski_sum_2.h>


#include <CGAL/Minkowski_sum_2/Decomposition_strategy_adapter.h>
#include <vector>

namespace CGAL {

/*!
 * \class
 * The \cgalBigO{n^4} optimal strategy for decomposing a polygon into convex
 * sub-polygons.
 */
template <typename Kernel_,
          typename Container_ = std::vector<typename Kernel_::Point_2> >
class Optimal_convex_decomposition_2 :
  public Polygon_decomposition_strategy_adapter<Kernel_, Container_,
            Tag_optimal_convex_parition>
{
public:
  typedef Kernel_                                  Kernel;
  typedef CGAL::Polygon_2<Kernel, Container_>      Polygon_2;
  typedef typename Kernel::Point_2                 Point_2;
};

/*!
 * \class
 * Hertel and Mehlhorn's \cgalBigO{n} approximation strategy for decomposing a
 * polygon into convex sub-polygons.
 */
template <typename Kernel_,
          typename Container_ = std::vector<typename Kernel_::Point_2> >
class Hertel_Mehlhorn_convex_decomposition_2 :
  public Polygon_decomposition_strategy_adapter<Kernel_, Container_,
            Tag_approx_convex_parition>
{
public:
  typedef Kernel_                                  Kernel;
  typedef CGAL::Polygon_2<Kernel, Container_>      Polygon_2;
  typedef typename Kernel::Point_2                 Point_2;
};

/*!
 * \class
 * Greene's \cgalBigO{n log(n)} approximation strategy for decomposing a polygon into
 * convex sub-polygons.
 */
template <typename Kernel_,
          typename Container_ = std::vector<typename Kernel_::Point_2> >
class Greene_convex_decomposition_2 :
  public Polygon_decomposition_strategy_adapter<Kernel_, Container_,
            Tag_Greene_convex_parition>
{
public:
  typedef Kernel_                                  Kernel;
  typedef CGAL::Polygon_2<Kernel, Container_>      Polygon_2;
  typedef typename Kernel::Point_2                 Point_2;
};

} //namespace CGAL

#endif
