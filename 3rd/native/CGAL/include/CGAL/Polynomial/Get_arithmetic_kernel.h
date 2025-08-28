// Copyright (c) 2006-2008 Max-Planck-Institute Saarbruecken (Germany).
// All rights reserved.
//
// This file is part of CGAL (www.cgal.org)
//
// $URL: https://github.com/CGAL/cgal/blob/v6.1-beta1/Polynomial/include/CGAL/Polynomial/Get_arithmetic_kernel.h $
// $Id: include/CGAL/Polynomial/Get_arithmetic_kernel.h b2f6f03d3fa $
// SPDX-License-Identifier: LGPL-3.0-or-later OR LicenseRef-Commercial
//
//
// Author(s)     : Michael Hemmer   <hemmer@mpi-inf.mpg.de>


#ifndef CGAL_POLYNOMIAL_GET_ARITHMETIC_KERNEL_H
#define CGAL_POLYNOMIAL_GET_ARITHMETIC_KERNEL_H

#include <CGAL/basic.h>
#include <CGAL/Get_arithmetic_kernel.h>

namespace CGAL {

template <class COEFF>
struct Get_arithmetic_kernel<Polynomial<COEFF> >{
  typedef Get_arithmetic_kernel<COEFF> GET;
  typedef typename GET::Arithmetic_kernel Arithmetic_kernel;
};

} //namespace CGAL

#endif
