//Copyright (C) 2013  INRIA - Sophia Antipolis
//
// This file is part of CGAL (www.cgal.org).
//
// $URL: https://github.com/CGAL/cgal/blob/v6.1-beta1/Scale_space_reconstruction_3/include/CGAL/Scale_space_reconstruction_3/internal/Auto_count.h $
// $Id: include/CGAL/Scale_space_reconstruction_3/internal/Auto_count.h b2f6f03d3fa $
// SPDX-License-Identifier: GPL-3.0-or-later OR LicenseRef-Commercial
//
// Author(s):      Thijs van Lankveld


#ifndef CGAL_INTERNAL_AUTO_COUNT_H
#define CGAL_INTERNAL_AUTO_COUNT_H

#include <CGAL/license/Scale_space_reconstruction_3.h>


#include <functional>
#include <utility>


namespace CGAL {

namespace internal {

//  constructs a pair containing the object and the number of these pairs previously constructed.
/*  \ingroup PkgScaleSpaceReconstruction3Auxiliary
 *  \tparam T is the type of object to count.
 *  \tparam C is the number type of the counter.
 *  \todo Make thread-safe.
 */
template < class T, class C = unsigned int >
class Auto_count
: public CGAL::cpp98::unary_function< const T&, std::pair< T, C > > {
    mutable C i; // Note, not thread-safe.
public:
/// \name Constructors
/// \{
    /// starts a new count.
    Auto_count(): i(0) {}
/// \}

/// \name Operations
/// \{
    /// constructs a pair with the object and the number of pairs previously constructed.
    /** \param t is the current object.
     *  \return a pair containing the object and the number of pairs previously constructed.
     */
    std::pair< T, C > operator()( const T& t ) const { return std::make_pair( t, i++ ); }
/// \}
};

} // namespace internal

} // namespace CGAL

#endif // CGAL_INTERNAL_AUTO_COUNT_H
