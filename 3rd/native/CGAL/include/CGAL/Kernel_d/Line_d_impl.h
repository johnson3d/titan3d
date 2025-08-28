// Copyright (c) 2000,2001
// Utrecht University (The Netherlands),
// ETH Zurich (Switzerland),
// INRIA Sophia-Antipolis (France),
// Max-Planck-Institute Saarbruecken (Germany),
// and Tel-Aviv University (Israel).  All rights reserved.
//
// This file is part of CGAL (www.cgal.org)
//
// $URL: https://github.com/CGAL/cgal/blob/v6.1-beta1/Kernel_d/include/CGAL/Kernel_d/Line_d_impl.h $
// $Id: include/CGAL/Kernel_d/Line_d_impl.h b2f6f03d3fa $
// SPDX-License-Identifier: LGPL-3.0-or-later OR LicenseRef-Commercial
//
//
// Author(s)     : Michael Seel

#ifndef CGAL_LINE_D_C
#define CGAL_LINE_D_C
namespace CGAL {

template <class R>
Line_d<R> Segment_d<R>::supporting_line() const
{ CGAL_assertion_msg((!is_degenerate()),
  "Segment_d::supporting_line(): degenerate segment cannot be converted.");
  return Line_d<R>(Base(*this));
}

template <class R>
Line_d<R> Ray_d<R>::supporting_line() const
{ return Line_d<R>(Base(*this)); }

} //namespace CGAL
#endif //CGAL_LINE_D_C
