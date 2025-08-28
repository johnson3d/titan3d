// Copyright (c) 1997-2007  ETH Zurich (Switzerland).
// All rights reserved.
//
// This file is part of CGAL (www.cgal.org).
//
// $URL: https://github.com/CGAL/cgal/blob/v6.1-beta1/QP_solver/include/CGAL/QP_solver/debug.h $
// $Id: include/CGAL/QP_solver/debug.h b2f6f03d3fa $
// SPDX-License-Identifier: GPL-3.0-or-later OR LicenseRef-Commercial
//
//
// Author(s)     : Sven Schoenherr
//                 Bernd Gaertner <gaertner@inf.ethz.ch>
//                 Franz Wessendorp
//                 Kaspar Fischer

#ifndef CGAL_QP_DEBUG_H
#define CGAL_QP_DEBUG_H

#include <CGAL/license/QP_solver.h>


// macro definitions
// =================

// debug
// -----
#if (    defined( CGAL_QP_NO_DEBUG)\
      || defined( CGAL_QP_NO_ASSERTIONS)\
      || defined( CGAL_NO_ASSERTIONS)\
      || defined( CGAL_NO_DEBUG) || defined( NDEBUG))
#  define  CGAL_qpe_debug  if ( 0)
#else
#  define  CGAL_qpe_debug  if ( 1)
#endif // qpe debug

#endif // CGAL_QP_DEBUG_H

// ===== EOF ==================================================================
