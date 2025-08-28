// Copyright (c) 2014 GeometryFactory Sarl (France)
// All rights reserved.
//
// This file is part of CGAL (www.cgal.org)
//
// $URL: https://github.com/CGAL/cgal/blob/v6.1-beta1/STL_Extension/include/CGAL/Has_timestamp.h $
// $Id: include/CGAL/Has_timestamp.h b2f6f03d3fa $
// SPDX-License-Identifier: LGPL-3.0-or-later OR LicenseRef-Commercial
//
// Author(s)     : Jane Tournois

#ifndef CGAL_HAS_TIMESTAMP_H
#define CGAL_HAS_TIMESTAMP_H

#include <boost/mpl/has_xxx.hpp>
#include <CGAL/tags.h>

namespace CGAL {

namespace internal {

  BOOST_MPL_HAS_XXX_TRAIT_DEF(Has_timestamp)

  // Used by Compact container to make the comparison of iterator
  // depending on the insertion order rather than the object address
  // when the object class defines a Has_timestamp tag
  // This is for example used in to make Mesh_3 deterministic, see
  // classes implementing concepts MeshCellBase_3 and MeshVertexBase_3
  template <typename T, bool has_ts = has_Has_timestamp<T>::value>
  struct Has_timestamp : public T::Has_timestamp
    // when T has a Has_timestamp tag
  {};

  template <typename T>
  struct Has_timestamp<T, false> : public Tag_false
    // when T does not have a Has_timestamp tag
  {};

  template <typename T>
  constexpr bool has_timestamp_v = Has_timestamp<T>::value;

} // end namespace internal
} // end namespace CGAL

#endif // CGAL_HAS_TIMESTAMP_H
