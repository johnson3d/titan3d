// Copyright (c) 2008 Max-Planck-Institute Saarbruecken (Germany).
// All rights reserved.
//
// This file is part of CGAL (www.cgal.org)
//
// $URL: https://github.com/CGAL/cgal/blob/v6.1-beta1/Polynomial/include/CGAL/Polynomial_type_generator.h $
// $Id: include/CGAL/Polynomial_type_generator.h b2f6f03d3fa $
// SPDX-License-Identifier: LGPL-3.0-or-later OR LicenseRef-Commercial
//
//
// Author(s)     : Michael Hemmer <hemmer@informatik.uni-mainz.de>
//
// ========================================================================


#ifndef CGAL_POLYNOMIAL_TYPE_GENERATOR_H
#define CGAL_POLYNOMIAL_TYPE_GENERATOR_H

#include <CGAL/disable_warnings.h>

#include <CGAL/Polynomial_traits_d.h>

namespace CGAL {

template <class T, int d>
struct Polynomial_type_generator
{
private:
  typedef typename Polynomial_type_generator<T,d-1>::Type Coeff;
public:
  typedef CGAL::Polynomial<Coeff> Type;
};

template <class T>
struct Polynomial_type_generator<T,0>{ typedef T Type; };

} //namespace CGAL

#include <CGAL/enable_warnings.h>

#endif // CGAL_POLYNOMIAL_GENERATOR_H
