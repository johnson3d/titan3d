// Copyright (c) 2024 GeometryFactory Sarl (France)
// All rights reserved.
//
// This file is part of CGAL (www.cgal.org)
//
// $URL: https://github.com/CGAL/cgal/blob/v6.1-beta1/STL_Extension/include/CGAL/STL_Extension/internal/Has_nested_type_Has_filtered_predicates_tag.h $
// $Id: include/CGAL/STL_Extension/internal/Has_nested_type_Has_filtered_predicates_tag.h b2f6f03d3fa $
// SPDX-License-Identifier: LGPL-3.0-or-later OR LicenseRef-Commercial
//
// Author(s)     : Sébastien Loriot

#ifndef CGAL_HAS_NESTED_TYPE_HAS_FILTERED_PREDICATES_TAG_H
#define CGAL_HAS_NESTED_TYPE_HAS_FILTERED_PREDICATES_TAG_H

#include <boost/mpl/has_xxx.hpp>

namespace CGAL {
namespace internal {

BOOST_MPL_HAS_XXX_TRAIT_NAMED_DEF(Has_nested_type_Has_filtered_predicates_tag, Has_filtered_predicates_tag, false)

} } // end namespace CGAL::internal

#endif // CGAL_HAS_NESTED_TYPE_HAS_FILTERED_PREDICATES_TAG_H
