// Copyright (c) 1997-2000  Max-Planck-Institute Saarbruecken (Germany).
// All rights reserved.
//
// This file is part of CGAL (www.cgal.org).
//
// $URL: https://github.com/CGAL/cgal/blob/v6.1-beta1/Nef_2/include/CGAL/Is_extended_kernel.h $
// $Id: include/CGAL/Is_extended_kernel.h b2f6f03d3fa $
// SPDX-License-Identifier: GPL-3.0-or-later OR LicenseRef-Commercial
//
//
// Author(s)     : Andreas Fabri <andreas.fabri@geometryfactory.com>

#ifndef CGAL_IS_EXTENDED_KERNEL_H
#define CGAL_IS_EXTENDED_KERNEL_H

#include <CGAL/license/Nef_2.h>


#include <CGAL/tags.h>

namespace CGAL {

template<class Kernel>
struct Is_extended_kernel {
       typedef Tag_false value_type;
};

} //namespace CGAL

#endif
