// Copyright (c) 2003,2006  INRIA Sophia-Antipolis (France).
// All rights reserved.
//
// This file is part of CGAL (www.cgal.org).
//
// $URL: https://github.com/CGAL/cgal/blob/v6.1-beta1/Apollonius_graph_2/include/CGAL/Apollonius_graph_2/Traits_wrapper_2.h $
// $Id: include/CGAL/Apollonius_graph_2/Traits_wrapper_2.h b2f6f03d3fa $
// SPDX-License-Identifier: GPL-3.0-or-later OR LicenseRef-Commercial
//
//
// Author(s)     : Menelaos Karavelas <mkaravel@iacm.forth.gr>



#ifndef CGAL_APOLLONIUS_GRAPH_2_TRAITS_WRAPPER_2_H
#define CGAL_APOLLONIUS_GRAPH_2_TRAITS_WRAPPER_2_H

#include <CGAL/license/Apollonius_graph_2.h>



#include <CGAL/Apollonius_graph_2/basic.h>



namespace CGAL {

namespace ApolloniusGraph_2 {


template<class Gt_base>
class Apollonius_graph_traits_wrapper_2 : public Gt_base
{
public:
  //  struct Segment_2  {};
  struct Triangle_2 {};

  Apollonius_graph_traits_wrapper_2() {}
  Apollonius_graph_traits_wrapper_2(const Gt_base& gtb)
    : Gt_base(gtb) {}

};


} //namespace ApolloniusGraph_2

} //namespace CGAL


#endif // CGAL_APOLLONIUS_GRAPH_2_TRAITS_WRAPPER_2_H
