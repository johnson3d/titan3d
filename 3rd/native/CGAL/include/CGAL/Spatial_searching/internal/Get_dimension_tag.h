// Copyright (c) 2015 Utrecht University (The Netherlands).
// All rights reserved.
//
// This file is part of CGAL (www.cgal.org).
//
// $URL: https://github.com/CGAL/cgal/blob/v6.1-beta1/Spatial_searching/include/CGAL/Spatial_searching/internal/Get_dimension_tag.h $
// $Id: include/CGAL/Spatial_searching/internal/Get_dimension_tag.h b2f6f03d3fa $
// SPDX-License-Identifier: GPL-3.0-or-later OR LicenseRef-Commercial
//
//
// Author(s)     : Sebastien Loriot

#ifndef CGAL_INTERNAL_GET_DIMENSION_TAG_H
#define CGAL_INTERNAL_GET_DIMENSION_TAG_H

#include <CGAL/license/Spatial_searching.h>


#include <CGAL/Dimension.h>
#include <boost/mpl/has_xxx.hpp>

namespace CGAL{

namespace internal{

  BOOST_MPL_HAS_XXX_TRAIT_NAMED_DEF(Has_dimension_tag,Dimension,false)

  template <class T, bool has_dim = Has_dimension_tag<T>::value>
  struct Get_dimension_tag
  {
    typedef typename T::Dimension Dimension;
  };

  template <class T>
  struct Get_dimension_tag<T, false>{
    typedef Dynamic_dimension_tag Dimension;
  };

} } // end of namespace internal::CGAL

#endif //CGAL_INTERNAL_GET_DIMENSION_TAG_H
