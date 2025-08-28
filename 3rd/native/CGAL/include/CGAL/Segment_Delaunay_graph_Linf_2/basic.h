// Copyright (c) 2015  Università della Svizzera italiana.
// All rights reserved.
//
// This file is part of CGAL (www.cgal.org).
//
// $URL: https://github.com/CGAL/cgal/blob/v6.1-beta1/Segment_Delaunay_graph_Linf_2/include/CGAL/Segment_Delaunay_graph_Linf_2/basic.h $
// $Id: include/CGAL/Segment_Delaunay_graph_Linf_2/basic.h b2f6f03d3fa $
// SPDX-License-Identifier: GPL-3.0-or-later OR LicenseRef-Commercial
//
//
// Author(s)     : Panagiotis Cheilaris, Sandeep Kumar Dey, Evanthia Papadopoulou
//philaris@gmail.com, sandeep.kr.dey@gmail.com, evanthia.papadopoulou@usi.ch

#ifndef CGAL_SEGMENT_DELAUNAY_GRAPH_LINF_2_BASIC_H
#define CGAL_SEGMENT_DELAUNAY_GRAPH_LINF_2_BASIC_H

#include <CGAL/license/Segment_Delaunay_graph_Linf_2.h>


#include <CGAL/basic.h>

#define CGAL_SEGMENT_DELAUNAY_GRAPH_LINF_2_NS \
  CGAL::SegmentDelaunayGraphLinf_2

#ifndef CGAL_SDG_VERBOSE
#define CGAL_SDG_DEBUG(a)
#else
#define CGAL_SDG_DEBUG(a) { a }
#endif

#endif // CGAL_SEGMENT_DELAUNAY_GRAPH_LINF_2_BASIC_H
