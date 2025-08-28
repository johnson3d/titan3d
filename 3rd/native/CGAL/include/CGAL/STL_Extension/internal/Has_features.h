// Copyright (c) 2009 INRIA Sophia-Antipolis (France).
// Copyright (c) 2011 GeometryFactory Sarl (France)
// All rights reserved.
//
// This file is part of CGAL (www.cgal.org).
//
// $URL: https://github.com/CGAL/cgal/blob/v6.1-beta1/STL_Extension/include/CGAL/STL_Extension/internal/Has_features.h $
// $Id: include/CGAL/STL_Extension/internal/Has_features.h b2f6f03d3fa $
// SPDX-License-Identifier: LGPL-3.0-or-later OR LicenseRef-Commercial
//
// Author(s)     : Stéphane Tayeb, Laurent Rineau

#ifndef CGAL_HAS_FEATURES_H
#define CGAL_HAS_FEATURES_H

#include <boost/mpl/has_xxx.hpp>
#include <CGAL/tags.h>

namespace CGAL {
namespace internal {

  // A type has_Has_features to check if type 'Has_features' is a nested
  // type of any class
  BOOST_MPL_HAS_XXX_TRAIT_DEF(Has_features)

  template <typename Mesh_domain,
            bool has_Has_features = has_Has_features<Mesh_domain>::value>
  struct Has_features :
    public CGAL::Boolean_tag<Mesh_domain::Has_features::value>
    // when Mesh_domain has the nested type Has_features
  {};

  template <typename Mesh_domain>
  struct Has_features<Mesh_domain, false> : public CGAL::Tag_false
    // when Mesh_domain does not have the nested type Has_features
  {};

} // end namespace internal
} // end namespace CGAL

#endif // CGAL_MESH_3_HAS_FEATURES_H
